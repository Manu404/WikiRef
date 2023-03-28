using System;
using System.Collections.Generic;
using System.IO;
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

        // Public data
        public string Name { get; private set; }
        public List<YoutubeUrl> YoutubeUrls { get; private set; }
        public List<YoutubeUrl> AggregatedYoutubeUrls { get; private set; }
        public List<Reference> References { get; private set; }
        public string Content { get; private set; }

        public WikiPage(string name, ConsoleHelper consoleHelper, MediaWikiApi api, AppConfiguration configuration, WhitelistHandler blacklistHandler, RegexHelper regexHelper)
        {
            YoutubeUrls = new List<YoutubeUrl>();
            AggregatedYoutubeUrls = new List<YoutubeUrl>();
            References = new List<Reference>();

            Name = name;

            _console = consoleHelper;
            _api = api;
            _config = configuration;
            _blacklistHandler = blacklistHandler;
            _regexHelper = regexHelper;

            GetPageContentFromApi();
            BuildReferenceList();
            ExtractUrlsFromReference();
        }

        private void BuildReferenceList()
        {
            if (isReferenceListBuilt) return;
            
            MatchCollection matches = _regexHelper.ExtractReferenceREgex.Matches(Content);

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
                        _console.WriteLineInOrange(String.Format("This reference contains multiple urls. Reference: {0}", reference));

                    reference.Urls.Add(HttpUtility.UrlDecode(match.Groups["url"].Value));
                });
            });
            areUrlExtracteFromReferences = true;
        }

        private void GetPageContentFromApi()
        {
            if (isPageContentRetreivedFromApi) return;
            Content = _api.GetPageContent(Name);
            isPageContentRetreivedFromApi = true;
        }

        public int BuildYoutubeLinkList()
        {
            YoutubeUrls.Clear();
            AggregatedYoutubeUrls.Clear();

            int youtubeLinkCount = 1;

            Parallel.ForEach(References, reference => {
                Parallel.ForEach(reference.Urls, url =>
                {
                    try
                    {
                        if (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) ||
                            url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase)) // youtu is used in shorten version of the youtube url
                        {
                            var video = new YoutubeUrl(url, _console, _config, _regexHelper);
                            YoutubeUrls.Add(video);

                            if (!AggregatedYoutubeUrls.Exists(a => video.Name == a.Name))
                                AggregatedYoutubeUrls.Add(video);

                            if (_config.Verbose)
                                if (video.IsValid == video.IsValid)
                                    _console.WriteLineInGray(String.Format("Found valide video {0} - {1}", video.Url, video.Name));
                                else
                                    _console.WriteLineInOrange(String.Format("Found invalide video {0} - {1}", video.Url, video.Name));

                            youtubeLinkCount += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(String.Format("#ERROR# URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                });
            });
            return youtubeLinkCount;
        }

        public void CheckFormatting()
        {

            Parallel.ForEach(References, reference =>
            {
                Parallel.ForEach(reference.Urls, url =>
                {
                    try
                    {
                        if (url.Contains("</nowiki>"))
                            _console.WriteLineInOrange(String.Format("<nowiki> tag for reference {0} in page {1}", url, Name));
                        if (url.EndsWith(']'))
                            _console.WriteLineInOrange(String.Format("Multiple references in the same ref tag : {0} in page {1}", url, Name));
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                });
            });
        }

        public void CheckPageStatus()
        {
            int numberOfReference = References.Count;
            int checkedReference = 0;
            SourceStatus result;

            Parallel.ForEach(References, (Action<Reference>)(reference =>
            {
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        if (_config.Throttle != 0)
                        {
                            _console.WriteLine(string.Format("Waiting {0} for throttleling...", _config.Throttle));
                            Thread.Sleep(1000 * _config.Throttle);
                        }

                        if (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) ||
                            url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase)) // youtu is used in shorten version of the youtube url
                            result = CheckYoutubeUrlStatus(url);
                        else
                            result = CheckUrlStatus(url);

                        if (result != SourceStatus.Valid || _config.Verbose)
                        {
                            if (result == SourceStatus.Invalid)
                                _console.WriteLineInOrange(string.Format("Invalid reference: {0} -> {1}", url, result.ToString()));
                            else
                                _console.WriteLineInGray(string.Format("Valid reference: {0} -> {1}", url, result.ToString()));
                        }
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed(string.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                    checkedReference += 1;
                }
            }));

            _console.WriteLine(String.Format("{0} reference found containing urls. {1} url verified.", numberOfReference, checkedReference));

            if (checkedReference == numberOfReference)
                _console.WriteLineInGreen(String.Format("All references seems valid"));
            else
                _console.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences"));
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

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:50.0) Gecko/20100101 Firefox/50.0";
                request.AllowAutoRedirect = true;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                return response.StatusCode == HttpStatusCode.OK ? SourceStatus.Valid : SourceStatus.Invalid;
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

        private SourceStatus CheckYoutubeUrlStatus(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    YoutubeUrl video = new YoutubeUrl(url, _console, _config, _regexHelper);
                    this.YoutubeUrls.Add(video);
                    return video.IsValid;
                }
                catch (WebException ex)
                {
                    _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    return SourceStatus.Undefined;
                }
                catch (Exception e)
                {
                    _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, e.Message));
                    return SourceStatus.Invalid;
                }
            }
        }

        public void AggregateYoutubUrl()
        {
            _console.WriteLine("Aggregating youtube links");

            List<YoutubeUrl> aggregatedUrlList = new List<YoutubeUrl>();
            foreach (var video in YoutubeUrls)
            {
                if (aggregatedUrlList.Exists(o => video.UrlWithoutArguments == o.UrlWithoutArguments))
                    continue;
                aggregatedUrlList.Add(video);
            }
            YoutubeUrls.Clear();
            YoutubeUrls = aggregatedUrlList;            
        }
    }
}
