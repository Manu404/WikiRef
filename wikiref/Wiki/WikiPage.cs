using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WikiRef
{
    class WikiPage
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

        // Public data
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public List<YoutubeUrl> YoutubeUrls { get; private set; }
        [JsonProperty] public List<Reference> References { get; private set; }
        [JsonProperty] public string Content { get; private set; }

        public WikiPage()
        {

        }

        public WikiPage(string name, ConsoleHelper consoleHelper, MediaWikiApi api, AppConfiguration configuration, WhitelistHandler blacklistHandler, RegexHelper regexHelper, NetworkHelper networkHelper)
        {
            YoutubeUrls = new List<YoutubeUrl>();
            References = new List<Reference>();

            Name = name;

            _console = consoleHelper;
            _api = api;
            _config = configuration;
            _blacklistHandler = blacklistHandler;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper; 

            GetPageContentFromApi();
            BuildReferenceList();
            ExtractUrlsFromReference();
        }

        private void GetPageContentFromApi()
        {
            if (isPageContentRetreivedFromApi) return;
            Content = _api.GetPageContent(Name);
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

                    reference.Urls.Add(HttpUtility.UrlDecode(match.Groups["url"].Value));
                });
            });
            areUrlExtracteFromReferences = true;
        }

        public void CheckFormatting(Reference reference)
        {
            try
            {
                // multiple links
                if (reference.Urls.Count > 1)
                {
                    _console.WriteLineInOrange(String.Format("Multiple link in the reference {0} in page {1}", reference.Content, Name)); ;
                    reference.FormattingIssue = true;
                }

                // check if meta once url removed an ref tags
                string meta = reference.Content.Replace("<ref>", "").Replace("</ref>", "");
                foreach (var url in reference.Urls)
                    meta = meta.Replace(url, "");
                if (String.IsNullOrEmpty(meta))
                {
                    _console.WriteLineInOrange(String.Format("No metadata for reference {0} in page {1}", reference.Content, Name));
                    reference.FormattingIssue = true;
                }

                Parallel.ForEach(reference.Urls, url =>
                {
                    try
                    {
                        // check nowiki tag in refs
                        if (url.Contains("</nowiki>"))
                        {
                            _console.WriteLineInOrange(String.Format("<nowiki> tag for reference {0} in page {1}", url, Name));
                            reference.FormattingIssue = true;
                        }
                        // check multiple urls in bracket
                        if (url.EndsWith(']'))
                        {
                            _console.WriteLineInOrange(String.Format("Multiple links in the same ref tag : {0} in page {1}", url, Name));
                            reference.FormattingIssue = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                });
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed(String.Format("Erro checking reference {0} - {1}", reference.Content, ex.Message));
            }
        }


        public async Task CheckReferenceStatus()
        {
            int numberOfcitation = References.Where(r => r.IsCitation).Count();
            int numberOfReference = References.Count - numberOfcitation;
            int checkedurls = 0;

            BuildAggregatedYoutubeUrl();

            await Parallel.ForEachAsync(YoutubeUrls, async (youtubeUrls, token) =>
            {
                await youtubeUrls.CheckIsValid();
            });

            // Check non youtube reference
            await Parallel.ForEachAsync(References.Where(r => !r.IsCitation), async (reference, token) =>
            {
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        if (IsYoutubeUrl(url))
                        {
                            string VideoId = YoutubeUrl.GetVideoId(url, _regexHelper);
                            if (VideoId != null && YoutubeUrls.Any(v => v.VideoId == VideoId))
                                reference.Status = YoutubeUrls.FirstOrDefault(v => v.VideoId == VideoId).IsValid;
                            else if (YoutubeUrls.Any(v => v.Urls.Any(u => u == url)))
                                reference.Status = YoutubeUrls.FirstOrDefault(v => v.Urls.Any(u => u == url)).IsValid;
                            else
                                reference.Status = SourceStatus.Invalid;
                        }
                        else
                            reference.Status = CheckUrlStatus(url);

                        DisplayreferencesStatus(reference.Status, url);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(string.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                    checkedurls += 1;
                }
                CheckFormatting(reference);
            });

            _console.WriteLine(String.Format("{0} reference found and {1} citation references. {2} url verified.", numberOfReference, numberOfcitation, checkedurls));

            if (References.Any(r => r.Status != SourceStatus.Valid))
                _console.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences or invalid urls"));
            else
                _console.WriteLineInGreen(String.Format("All references seems valid"));
        }

        private void DisplayreferencesStatus(SourceStatus status, string url)
        {
            if (status != SourceStatus.Valid || _config.Verbose)
            {
                string displayedUrl = IsYoutubeUrl(url) ? $"VideoID: {YoutubeUrl.GetVideoId(url, _regexHelper)}" : url;
                if (status == SourceStatus.Invalid)
                    _console.WriteLineInRed(string.Format("Invalid reference: {0} -> {1}", displayedUrl, status.ToString()));
                else
                    _console.WriteLineInGray(string.Format("Valid reference: {0} -> {1}", displayedUrl, status.ToString()));
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
                        if (IsYoutubeUrl(url))
                        {
                            string VideoId = YoutubeUrl.GetVideoId(url, _regexHelper);
                            if (!YoutubeUrls.Exists(o => o.VideoId == VideoId || String.IsNullOrEmpty(VideoId)))
                                YoutubeUrls.Add(new YoutubeUrl(url, _console, _config, _regexHelper, _networkHelper));
                            else
                            {
                                YoutubeUrls.FirstOrDefault(o => VideoId == o.VideoId).Urls.Add(url);
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

        private bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
