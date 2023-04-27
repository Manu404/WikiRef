using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WikiRef;
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
        private WhitelistHandler _whitelist;
        private RegexHelper _regexHelper;
        private NetworkHelper _networkHelper;

        [JsonIgnore] public int MalformedDates { get; set; }
        [JsonIgnore] public int DatesCount { get; set; }
        [JsonIgnore] public int WikiLinks { get; set; }

        [JsonIgnore] public List<YoutubeUrlData> ThreadSafeYoutubeUrls { get; set; }
        [JsonIgnore] public List<Reference> ThreadSafeReferences { get; set; }

        public WikiPage()
        {

        }

        public WikiPage(string name, ConsoleHelper consoleHelper, MediaWikiApi api, AppConfiguration configuration, WhitelistHandler whiteList, RegexHelper regexHelper, NetworkHelper networkHelper)
        {
            YoutubeUrls = new List<YoutubeUrlData>();
            References = new List<Reference>();

            Name = name;

            _console = consoleHelper;
            _api = api;
            _config = configuration;
            _whitelist = whiteList;
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
                        _console.WriteLineInOrange(String.Format("#This reference contains multiple urls. Reference: {0}", reference.Content));

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

            new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0))
            };

            await Parallel.ForEachAsync(YoutubeUrls, async (youtubeUrls, token) =>
            {
                await (youtubeUrls as YoutubeUrl).FetchPageName();
            });

            // Check non youtube reference
            await Parallel.ForEachAsync(References, async (reference, token) =>
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
                        else if (reference.IsCitation && IsYoutubeUrl(reference.Content)) // if url contained in citation
                        {
                            var citationVideo = new YoutubeUrl(url.Url, _console, _config, _regexHelper, _networkHelper);
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
                        //reference.Status = status;

                        DisplayreferencesStatus(reference, url.Url);
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(string.Format("URL: {0} - Erreur: {1}", url.Url, ex.Message.Trim()));
                    }
                    checkedurls += 1;
                }
            });


            _console.WriteLine(String.Format("{0} reference found and {1} citation references. {2} url verified.", numberOfReference, numberOfcitation, checkedurls));

            if (References.Any(r => r.Status == SourceStatus.Invalid))
                _console.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences or invalid urls"));
            else
                _console.WriteLineInGreen(String.Format("All references seems valid"));
        }

        private void DisplayreferencesStatus(Reference reference, string url)
        {
            if (reference.Status != SourceStatus.Valid || _config.Verbose)
            {
                string displayedUrl = IsYoutubeUrl(url) ? $"VideoID: {YoutubeUrl.GetVideoId(url, _regexHelper)}" : url.Trim();
                if (reference.Status == SourceStatus.Invalid)
                {
                    _console.WriteLineInRed($"#> Invalid reference: {displayedUrl} -> {reference.Status}");
                    _console.WriteLineInRed($"Content: {reference.Content}");
                }
                else if (reference.Status == SourceStatus.WhiteListed)
                    _console.WriteLineInOrange($"> #The url {url} is whitelited and wasn't checked.");
                else
                    _console.WriteLineInGray($"#> Valid reference but might have some issues: {displayedUrl} -> {reference.Status}");
            }
        }

        private SourceStatus CheckUrlStatus(string url)
        {
            try
            {
                if (_whitelist.CheckIfWebsiteIsWhitelisted(url))
                    return SourceStatus.WhiteListed;

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

            foreach( var reference in References.Where(r => !r.IsCitation)) // can't || because collection is modified
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
                                YoutubeUrls.FirstOrDefault(o => VideoId == o.VideoId).Urls.Add(url.Url);
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"URL: {reference.Content} - Erreur: {ex.Message}");
                }
            };

        }

        public bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
