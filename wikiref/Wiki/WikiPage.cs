using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WikiRef.Commons;
using WikiRef.Commons.Data;
using WikiRef.Wiki;

namespace WikiRef.Wiki
{
    public class WikiPage : WikiPageData
    {
        // Internal status flags
        private bool isReferenceListBuilt = false;
        private bool areUrlExtracteFromReferences = false;
        private bool isPageContentRetreivedFromApi = false;

        // External Dependencies
        private ConsoleHelper _console;
        private MediaWikiApi _api;
        private AppConfiguration _config;
        private WhitelistHandler _blacklistHandler;
        private RegexHelper _regexHelper;
        private NetworkHelper _networkHelper;

        [JsonIgnore] public int MalformedDates { get; set; }
        [JsonIgnore] public int DatesCount { get; set; }
        [JsonIgnore] public int WikiLinks { get; set; }

        public WikiPage()
        {

        }

        public WikiPage(string name, ConsoleHelper consoleHelper, MediaWikiApi api, AppConfiguration configuration, WhitelistHandler blacklistHandler, RegexHelper regexHelper, NetworkHelper networkHelper)
        {
            YoutubeUrls = new List<YoutubeUrlData>();
            References = new List<Reference>();

            Name = name;

            _console = consoleHelper;
            _api = api;
            _config = configuration;
            _blacklistHandler = blacklistHandler;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper; 

            GetPageContentFromApi().Wait();
            BuildReferenceList();
            ExtractUrlsFromReference();
        }

        private async Task GetPageContentFromApi()
        {
            if (isPageContentRetreivedFromApi) return;
            Content = await _api.GetPageContent(Name);
            isPageContentRetreivedFromApi = true;
        }

        private void BuildReferenceList()
        {
            if (isReferenceListBuilt) return;
            
            MatchCollection matches = _regexHelper.ExtractReferenceFromPageRegex.Matches(Content);

            foreach (Match match in matches)
                References.Add(new Reference(match.Value));

            isReferenceListBuilt = true;
        }

        private void ExtractUrlsFromReference()
        {
            if (areUrlExtracteFromReferences) return;

            Parallel.ForEach(References, reference =>
            {
                var matches = _regexHelper.ExtractUrlFromReferenceRegex.Matches(reference.Content);

                Parallel.ForEach(matches, match =>
                {
                    if (match.Groups["url"].Value.Contains(','))
                        _console.WriteLineInOrange(String.Format("This reference contains multiple urls. Reference: {0}", reference.Content));

                    reference.Urls.Add(new ReferenceUrl(HttpUtility.UrlDecode(match.Groups["url"].Value)));
                });
            });
            areUrlExtracteFromReferences = true;
        }
        
        public async Task CheckReferenceStatus()
        {
            int numberOfcitation = References.Where(r => r.IsCitation).Count();
            int numberOfReference = References.Count - numberOfcitation;
            int checkedurls = 0;

            BuildAggregatedYoutubeUrl();

            await Parallel.ForEachAsync(YoutubeUrls, async (youtubeUrls, token) =>
            {
                await (youtubeUrls as YoutubeUrl).CheckIsValid();
            });

            // Check non youtube reference
            Parallel.ForEach(References, async (reference) =>
            {
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        SourceStatus status = SourceStatus.Valid;
                        if (IsYoutubeUrl(url.Url))
                        {
                            string VideoId = YoutubeUrl.GetVideoId(url.Url, _regexHelper);
                            if (VideoId != null && YoutubeUrls.Any(v => v.VideoId == VideoId))
                                status = YoutubeUrls.FirstOrDefault(v => v.VideoId == VideoId).IsValid;
                            else if (YoutubeUrls.Any(v => v.Urls.Any(u => u == url.Url)))
                                status = YoutubeUrls.FirstOrDefault(v => v.Urls.Any(u => u == url.Url)).IsValid;
                            else
                                status = SourceStatus.Invalid;

                        }
                        else
                            status = CheckUrlStatus(url.Url);

                        url.IsValid = status == SourceStatus.Valid;

                        reference.Status = status;

                        DisplayreferencesStatus(reference, url.Url);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(string.Format("URL: {0} - Erreur: {1}", url.Url, ex.Message));
                    }
                    checkedurls += 1;
                }
            });


            _console.WriteLine(String.Format("{0} reference found and {1} citation references. {2} url verified.", numberOfReference, numberOfcitation, checkedurls));

            if (References.Any(r => r.Status != SourceStatus.Valid))
                _console.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences or invalid urls"));
            else
                _console.WriteLineInGreen(String.Format("All references seems valid"));
        }

        private void DisplayreferencesStatus(Reference reference, string url)
        {
            if (reference.Status != SourceStatus.Valid || _config.Verbose)
            {
                string displayedUrl = IsYoutubeUrl(url) ? $"VideoID: {YoutubeUrl.GetVideoId(url, _regexHelper)}" : url;
                if (reference.Status == SourceStatus.Invalid)
                    _console.WriteLineInRed($"Invalid reference: {displayedUrl} -> {reference.Status}");
                else
                    _console.WriteLineInGray($"Valid reference: {displayedUrl} -> {reference.Status}");
            }
        }

        private SourceStatus CheckUrlStatus(string url)
        {
            try
            {
                if (!_blacklistHandler.CheckIfWebsiteIsWhitelisted(url))
                {
                    _console.WriteLineInOrange(String.Format("The url {0} can't be checked for technical reason due to service provider blocking requests.", url));
                    return SourceStatus.Undefined;
                }

                var result = _networkHelper.GetStatus(url).Result;

                return result == HttpStatusCode.OK ? SourceStatus.Valid : SourceStatus.Invalid;
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("302")) // redirection
                    return SourceStatus.Valid;

                _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                return SourceStatus.Undefined;
            }
            catch (Exception e)
            {
                _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, e.Message));
                return SourceStatus.Invalid;
            }
        }

        public void BuildAggregatedYoutubeUrl()
        {
            _console.WriteLine("Aggregating youtube links");

            foreach(var reference in References.Where(r => !r.IsCitation))
            {
                try
                {
                    foreach (var url in reference.Urls)
                    {
                        if (IsYoutubeUrl(url.Url))
                        {
                            string VideoId = YoutubeUrl.GetVideoId(url.Url, _regexHelper);
                            if (!YoutubeUrls.Exists(o => o.VideoId == VideoId || String.IsNullOrEmpty(VideoId)))
                                YoutubeUrls.Add(new YoutubeUrl(url.Url, _console, _config, _regexHelper, _networkHelper));
                            else
                            {
                                YoutubeUrls.FirstOrDefault(o => VideoId == o.VideoId).Urls.Add(url.Url);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"URL: {reference.Content} - Erreur: {ex.Message}");
                }
            }
        }

        public bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
