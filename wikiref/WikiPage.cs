using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
        private ConsoleHelper _consoleHelper;
        private MediaWikiApi _api;
        private GlobalConfiguration _configuration;
        private WhitelistHandler _blacklistHandler;

        // Public data
        public string Name { get; private set; }
        public List<YoutubeVideo> YoutubeUrls { get; private set; }
        public List<Reference> References { get; private set; }
        public string Content { get; private set; }

        public WikiPage(string name, ConsoleHelper consoleHelper, MediaWikiApi api, GlobalConfiguration configuration, WhitelistHandler blacklistHandler)
        {
            YoutubeUrls = new List<YoutubeVideo>();
            References = new List<Reference>();

            Name = name;

            _consoleHelper = consoleHelper;
            _api = api;
            _configuration = configuration;
            _blacklistHandler = blacklistHandler;
        }

        public void BuildReferenceList()
        {
            if (isReferenceListBuilt) return;

            string referenceContainingUrlRegularExpression = @"([<]( *)(ref)?( *)[>]).*?(?:https?|www)?.*?([<]( *)[\/]( *)(ref)?( *)[>])";
            Regex refParser = new Regex(referenceContainingUrlRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = refParser.Matches(Content);

            foreach (Match match in matches)
                References.Add(new Reference(match.Value));

            isReferenceListBuilt = true;
        }

        public void ExtractUrlsFromReference()
        {
            if (areUrlExtracteFromReferences) return;

            foreach (var reference in References)
            {
                string urlfilterRegularExpression = @"\b(?:https?:\/\/|www\.)\S+"; // The following regex could be used @"\b(?:https?://|www\.)\S+(?=</ref)"; but in case of malformated ref containing two url, handled by mediawiki, this report as an error.
                Regex linkParser = new Regex(urlfilterRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                if (linkParser.Matches(reference.Content).Count > 1)
                    _consoleHelper.WriteLineInOrange(String.Format("This reference contains multiple urls. Reference: {0}", reference));

                foreach (Match match in linkParser.Matches(reference.Content))
                    reference.Urls.Add(HttpUtility.UrlDecode(RemoveRefTagIfIcluded(match.Value)));
            }

            areUrlExtracteFromReferences = false;
        }

        private string RemoveRefTagIfIcluded(string url)
        {
            // regex seems complex, but take into account any kind of white space present in the tag at any place
            return Regex.Replace(url, @"([<]( *)[\/]( *)(ref)?( *)[>])", String.Empty);
        }

        private void GetPageContentFromApi()
        {
            if (isPageContentRetreivedFromApi) return;
            Content = _api.GetPageContent(Name);
            isPageContentRetreivedFromApi = true;
        }

        public void BuildYoutubeLinkList()
        {
            GetPageContentFromApi();
            BuildReferenceList();
            ExtractUrlsFromReference();

            int checkedReference = 0, youtubeLinkCount = 0;

            foreach (var reference in References)
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        if (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) ||
                            url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase)) // youtu is used in shorten version of the youtube url
                        {
                            if (_configuration.Verbose)
                                _consoleHelper.WriteLineInGray(String.Format("Found {0}", url));

                            YoutubeUrls.Add(new YoutubeVideo(url, _consoleHelper, _configuration));
                            youtubeLinkCount += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                    checkedReference += 1;
                }
        }

        public void CheckPageStatus()
        {
            GetPageContentFromApi();
            BuildReferenceList();
            ExtractUrlsFromReference();

            int numberOfReference = References.Count;
            int checkedReference = 0;
            SourceStatus result;

            foreach (var reference in References)
            {
                foreach (var url in reference.Urls)
                {
                    try
                    {
                        if (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) ||
                            url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase)) // youtu is used in shorten version of the youtube url
                            result = CheckIfYoutubeVideoviolateTOS(url);
                        else
                            result = CheckUrlStatus(url);

                        if (result != SourceStatus.Valid || _configuration.Verbose)
                        {
                            if (result == SourceStatus.Invalid)
                                _consoleHelper.WriteLineInRed(String.Format("{0} -> {1}", url, result.ToString()));
                            else
                                _consoleHelper.WriteLineInGray(String.Format("{0} -> {1}", url, result.ToString()));
                        }
                    }
                    catch (Exception ex)
                    {
                        _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    }
                    checkedReference += 1;
                }
            }

            _consoleHelper.WriteLine(String.Format("{0} reference found containing urls. {1} url verified", numberOfReference, checkedReference));
            if (checkedReference == numberOfReference)
                _consoleHelper.WriteLineInGreen(String.Format("All references seems valid"));
            else
                _consoleHelper.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences"));
        }

        private SourceStatus CheckUrlStatus(string url)
        {
            try
            {
                if (!_blacklistHandler.CheckIfWebsiteIsWhitelisted(url))
                {
                    _consoleHelper.WriteLineInOrange(String.Format("The url {0} can't be checked for technical reason due to service provider blocking requests.", url));
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

                _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                return SourceStatus.Undefined;
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, e.Message));
                return SourceStatus.Invalid;
            }
        }

        SourceStatus CheckIfYoutubeVideoviolateTOS(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    // No way to detect takedown and private for the moment in the received text, those messages are generated by js

                    //string pageContent = client.DownloadString("https://www.youtube.com/watch?v=iUt7AN6GExY");
                    //if (pageContent.Contains("Cette vidéo a été supprimée", StringComparison.InvariantCultureIgnoreCase)) // Take down par youtube: https://www.youtube.com/watch?v=aFLeeiLqDB0&t=716s
                    //    return SourceStatus.TakedownTos;
                    //else if (pageContent.Contains("Vidéo non disponible", StringComparison.InvariantCultureIgnoreCase)) // rendue privé : https://www.youtube.com/watch?v=iUt7AN6GExY
                    //    return SourceStatus.Private;
                    //else
                    //    return SourceStatus.Valid;

                    // usage of a more basic approach relying on the video name, a video that was take down or private doesn't have a name met-tag
                    YoutubeVideo video = new YoutubeVideo(url, _consoleHelper, _configuration);
                    return String.IsNullOrEmpty(video.Name) ? SourceStatus.Invalid : SourceStatus.Valid;
                }
                catch (WebException ex)
                {
                    _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, ex.Message));
                    return SourceStatus.Undefined;
                }
                catch (Exception e)
                {
                    _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", url, e.Message));
                    return SourceStatus.Invalid;
                }
            }
        }

        public void AggregateYoutubUrl()
        {
            _consoleHelper.WriteLine("Aggregating youtube links");

            List<YoutubeVideo> aggregatedUrlList = new List<YoutubeVideo>();

            foreach (var video in YoutubeUrls)
            {
                if (video.Url.Contains("?t"))
                {
                    string baseUrl = video.Url.Split("?t")[0];
                    if (!aggregatedUrlList.Exists(o => o.Url == baseUrl))
                        aggregatedUrlList.Add(new YoutubeVideo(baseUrl, _consoleHelper, _configuration));
                }
                else if (video.Url.Contains("&t"))
                {
                    string baseUrl = video.Url.Split("&t")[0];
                    if (!aggregatedUrlList.Exists(o => o.Url == baseUrl))
                        aggregatedUrlList.Add(new YoutubeVideo(baseUrl, _consoleHelper, _configuration));
                }
                else
                {
                    aggregatedUrlList.Add(video);
                }
            }

            YoutubeUrls.Clear();
            YoutubeUrls = aggregatedUrlList;            
        }
    }

    class YoutubeVideo
    {
        ConsoleHelper _consoleHelper;
        GlobalConfiguration _configuration;

        public string Url { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }

        public YoutubeVideo(string url, ConsoleHelper consoleHelper, GlobalConfiguration configuration)
        {
            Url = url;
            
            _consoleHelper = consoleHelper;
            _configuration = configuration;

            RetreiveName();
            GenerateFileName();
        }

        public void RetreiveName()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    if(_configuration.Verbose)
                        _consoleHelper.WriteLineInGray(String.Format("Retreiving name for {0}", Url));

                    string pageContent = client.DownloadString(Url);
                    string urlfilterRegularExpression = "(<meta property=.og:title. content=.)(.*?)(.)([>])"; // match <meta name="title" content="TITLE"> in 4 group, title is in group 2
                    Regex linkParser = new Regex(urlfilterRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matches = linkParser.Matches(pageContent);

                    foreach (Match match in matches)
                        Name = HttpUtility.HtmlDecode(match.Groups[2].Value);
                }
                catch (WebException ex)
                {
                    _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, ex.Message));
                }
                catch (Exception e)
                {
                    _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, e.Message));
                }
            }
        }

        private void GenerateFileName()
        {
            if (String.IsNullOrEmpty(Name))
                return;

            // Make video name valid for use as path
            var name = Name;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            name = name.Replace(' ', '_');
            FileName = name;

            // Add youtube code as reference in case the reference doesn't contain file name, split on / then remove & and ? args
            // match:   https://youtu.be/SQuSdHIfuoU?t=37
            //          https://www.youtube.com/watch?v=nA0eTwsdhS8&t=772
            //          https://www.youtube.com/shorts/PElWZRmFwc4
            var youtubeName = Url.Split('/').ToList().Last();
            youtubeName = youtubeName.Split('?').ToList().First();
            youtubeName = youtubeName.Split('&').ToList().First();

            FileName = String.Format("{0}_{1}", name, youtubeName);
        }
    }

    class WebPage
    {
        public string Url { get; set; }

        public WebPage(string url)
        {
            Url = url;
        }
    }

    class Reference
    {
        public string Content { get; set; }
        public List<string> Urls { get; set; }

        public Reference(string content)
        {
            Content = content;
            Urls = new List<string>();
        }
    }
}
