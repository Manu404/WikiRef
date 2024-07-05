using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WikiRef.Common;
using WikiRef.Data;

namespace WikiRef.Wiki
{
    public class WikiPage : Data.WikiPage
    {
        private IConsole _console;
        private MediaWikiApi _api;
        private IAppConfiguration _config;
        private WhiteListHelper _whitelist;
        private IRegexHelper _regexHelper;
        private INetworkHelper _networkHelper;

        public WikiPage()
        {

        }

        public WikiPage(IAppConfiguration configuration, IConsole console, MediaWikiApi api, WhiteListHelper whiteList, IRegexHelper regexHelper, INetworkHelper networkHelper, string name, string category)
        {
            YoutubeUrls = new List<Data.YoutubeUrl>();
            References = new List<Reference>();

            Categories = new List<WikiCategory>
            {
                new WikiCategory()
                {
                    Name = category
                }
            };
            Name = name;

            _console = console;
            _api = api;
            _config = configuration;
            _whitelist = whiteList;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper;

            BuildPage().Wait();
        }

        private async Task BuildPage()
        {
            await GetPageContentFromApi();
            BuildReferenceList();
            ExtractUrlsFromReferences();
        }

        private async Task GetPageContentFromApi()
        {
            Content = await _api.GetPageContent(Name);
        }

        private void BuildReferenceList()
        {            
            MatchCollection matches = _regexHelper.ExtractReferenceFromPageRegex.Matches(Content);
            foreach (Match match in matches)
                References.Add(new Reference(match.Value));
        }
        
        private void ExtractUrlsFromReferences()
        {
            foreach(var reference in References)
            {
                var matches = _regexHelper.ExtractUrlFromReferenceRegex.Matches(reference.Content);

                foreach(Match match in matches)
                {
                    if (match.Groups["url"].Value.Contains(','))
                        _console.WriteLineInOrange($"#This reference contains multiple urls. Reference: {reference.Content}");

                    reference.Urls.Add(new ReferenceUrl(HttpUtility.UrlDecode(match.Groups["url"].Value)));
                }
            }
        }
        
        public async Task CheckReferenceStatus()
        {
            int numberOfCitations = References.Where(r => r.IsCitation).Count();
            int numberOfReferences = References.Count - numberOfCitations;
            int checkedUrls = 0;

            BuildAggregatedYoutubeUrls();

            await Parallel.ForEachAsync(YoutubeUrls, async (youtubeUrls, token) =>
            {
                await (youtubeUrls as YoutubeUrl).FetchPageName();
            });

            await Parallel.ForEachAsync(References, async (reference, token) =>
            {
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        SourceStatus status = SourceStatus.Valid;
                        if (!reference.Content.StartsWith("<ref>{{ReferenceText") && !reference.IsCitation)
                            status = SourceStatus.Invalid;
                        else if (IsYoutubeUrl(url.Url))
                        {
                            string VideoId = YoutubeUrl.GetVideoId(url.Url, _regexHelper);
                            if (VideoId != null && YoutubeUrls.Any(v => v.VideoId == VideoId))
                                status = YoutubeUrls.FirstOrDefault(v => v.VideoId == VideoId).IsValid;
                            else if (YoutubeUrls.Any(v => v.Urls.Any(u => u == url.Url)))
                                status = YoutubeUrls.FirstOrDefault(v => v.Urls.Any(u => u == url.Url)).IsValid;
                            else if (reference.IsCitation && IsYoutubeUrl(reference.Content)) // if url contained in citation
                            {
                                var citationVideo = new YoutubeUrl(_config, _console, _regexHelper, _networkHelper, url.Url);
                                await citationVideo.FetchPageName();
                                status = citationVideo.IsValid;
                            }
                            else
                                status = SourceStatus.Invalid;
                        }
                        else
                            status = CheckUrlStatus(url.Url);

                        url.SourceStatus = status;

                        // take the worst case scenario
                        reference.Status = (SourceStatus)Math.Max((int)status, (int)reference.Status);

                        DisplayReferenceStatus(reference, url.Url);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(string.Format("URL: {0} - Error: {1}", url.Url, ex.Message.Trim()));
                    }
                    checkedUrls += 1;
                }
            });


            _console.WriteLine($"{numberOfReferences} references found and {numberOfCitations} citations references. {checkedUrls} url verified.");

            if (References.Any(r => r.Status == SourceStatus.Invalid))
                _console.WriteLineInRed("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences or invalid urls");
            else
                _console.WriteLineInGreen("All references seems valid");
        }

        private void DisplayReferenceStatus(Reference reference, string url)
        {
            if (reference.Status != SourceStatus.Valid || _config.Verbose)
            {
                string displayedUrl = IsYoutubeUrl(url) ? $"VideoID: {YoutubeUrl.GetVideoId(url, _regexHelper)}" : url.Trim();
                if (reference.Status == SourceStatus.Invalid)
                {
                    _console.WriteLineInRed($"#> Invalid reference: {displayedUrl} -> {reference.Status}");
                    _console.WriteLineInRed($"Content: {reference.Content}");
                }
                else if (_config.Verbose && reference.Status == SourceStatus.WhiteListed)
                    _console.WriteLineInOrange($"> #The url {url} is whitelited and wasn't checked.");
                else if(_config.Verbose && reference.Status != SourceStatus.Valid)
                    _console.WriteLineInGray($"#> Valid reference but might have some issues: {displayedUrl} -> {reference.Status}");
            }
        }

        private SourceStatus CheckUrlStatus(string url)
        {
            try
            {
                if (_whitelist.CheckIfUrlIsWhiteListed(url))
                    return SourceStatus.WhiteListed;

                var result = _networkHelper.GetStatus(url).Result;

                return result == HttpStatusCode.OK ? SourceStatus.Valid : SourceStatus.Invalid;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("302"))
                    return SourceStatus.Valid;

                _console.WriteLineInRed($"URL: {url} - Error: {ex.Message}");
                return SourceStatus.Undefined;
            }
            catch (Exception e)
            {
                _console.WriteLineInRed($"URL: {url} - Error: {e.Message}");
                return SourceStatus.Invalid;
            }
        }

        private void BuildAggregatedYoutubeUrls()
        {
            _console.WriteLine("Aggregating youtube links");

            foreach(var url in References.Where(r => !r.IsCitation).SelectMany(r => r.Urls))
            {
                try
                {
                    if (IsYoutubeUrl(url.Url))
                    {
                        string videoId = YoutubeUrl.GetVideoId(url.Url, _regexHelper);
                        if (!YoutubeUrls.Exists(o => o.VideoId == videoId) || String.IsNullOrEmpty(videoId))
                            YoutubeUrls.Add(new YoutubeUrl(_config, _console, _regexHelper, _networkHelper, url.Url));
                        else
                            YoutubeUrls.FirstOrDefault(o => videoId == o.VideoId).Urls.Add(url.Url);
                    }
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"URL: {url.Url} - Error: {ex.Message}");
                }
            };
        }

        private bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
