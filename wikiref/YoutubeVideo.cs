using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace WikiRef
{
    class YoutubeVideo
    {
        ConsoleHelper _console;
        AppConfiguration _config;

        public string Url { get; private set; }
        public string UrlWithoutArguments { get; set; }
        public string Name { get; private set; }
        public string FileName { get; private set; }
        public SourceStatus IsValid { get; set; }

        public YoutubeVideo(string url, ConsoleHelper consoleHelper, AppConfiguration configuration)
        {
            Url = url;
            
            _console = consoleHelper;
            _config = configuration;
            IsValid = SourceStatus.Valid;

            RetreiveName();
            CheckIfValid();
            GenerateFileName();
            BuildUrlWihtoutArgument();
        }

        private void BuildUrlWihtoutArgument()
        {
            if (String.IsNullOrEmpty(Url))
                return;

            if (Url.Contains("?t"))            
                Url = Url.Split("?t")[0];            
            else if (Url.Contains("&t"))            
                Url = Url.Split("&t")[0];            
        }

        private void CheckIfValid()
        {
            if (Name == " - YouTube") // takedown - private or TOS violation
                IsValid = SourceStatus.Invalid;
            else if (Name == "YouTube") // redirect home page
                IsValid = SourceStatus.Invalid;
            else if ((String.IsNullOrEmpty(Name) || String.IsNullOrWhiteSpace(Name)) && !Url.Contains("/embed/")) // deleted channel, embeded videos doesen't have title, so htey should be considered valid
                IsValid = SourceStatus.Invalid;
        }

        private void RetreiveName()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (_config.Throttle != 0)
                    {
                        _console.WriteLine(String.Format("Waiting {0} for throttleling...", _config.Throttle));
                        Thread.Sleep(1000 * _config.Throttle);
                    }

                    if (_config.Verbose)
                        _console.WriteLineInGray(String.Format("Retreiving name for {0}", Url));

                    
                    if (Url.Contains("embed")) // mebed videos have no name in the title, but there's a reference to the orignal video that can be retrieved.
                        GetSourceUrlFromEmbededVideo(client);

                    string pageContent = client.DownloadString(Url);
                    string titleFilterExpression = @"(<title>)(?<name>.*?)(</title>)"; // regex developped with regex101, regex and the texting datas available heree: https://regex101.com/r/HOb95o/1
                    Regex linkParser = new Regex(titleFilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matches = linkParser.Matches(pageContent);

                    string title = String.Empty;
                    foreach (Match match in matches)
                        title = HttpUtility.HtmlDecode(match.Groups["name"].Value);

                    string stripedTitle = String.Empty;
                    if (title.ToLower().EndsWith("- youtube"))
                        foreach (var part in title.Split('-').SkipLast(1).ToList())
                            stripedTitle += part;

                    Name = stripedTitle;

                    _console.WriteLineInGray(String.Format("Video name: {0}", Name));
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1} - Retry in {2}sec", Url, ex.Message, 5000));
                    else
                        _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, ex.Message));
                }
                catch (Exception e)
                {
                    _console.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, e.Message));
                }
            } 
        }

        public void GetSourceUrlFromEmbededVideo(WebClient client)
        {
            string pageContent = client.DownloadString(Url);
            string urlfilterRegularExpression = "(<a href=\")(?<url>.*?)(\")"; // regex developped with regex101, regex and the texting datas available heree: s https://regex101.com/r/aqcAnR/1
            Regex linkParser = new Regex(urlfilterRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = linkParser.Matches(pageContent);

            foreach (Match match in matches)
                Url = HttpUtility.HtmlDecode(match.Groups["url"].Value);
        }

        private void GenerateFileName()
        {
            if (String.IsNullOrEmpty(Name) || IsValid == SourceStatus.Invalid)
                return;

            // Make video name valid for use as path
            var name = Name;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())            
                name = name.Replace(c, '_');
            
            name = name.Replace(' ', '_');
            FileName = name;

            var videoId = String.Empty;
            string urlfilterRegularExpression = @"(?<host>.*/)(?<watch>.*v=)?(?<videoid>.*)"; // regex developped with regex101, regex and the texting datas available heree:  https://regex101.com/r/0tLwmD/1
            Regex linkParser = new Regex(urlfilterRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = linkParser.Matches(Url);

            foreach (Match match in matches)
                videoId = HttpUtility.HtmlDecode(match.Groups["videoid"].Value);

            if(videoId.Contains('?'))
                videoId = videoId.Split('?').ToList().First();
            if(videoId.Contains("&"))
                videoId = videoId.Split('&').ToList().First();

            FileName = String.Format("{0}_[{1}]", name, videoId);

            _console.WriteLineInOrange(FileName);
        }
    }
}
