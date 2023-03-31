using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace WikiRef
{
    class YoutubeUrl
    {
        ConsoleHelper _console;
        AppConfiguration _config;
        RegexHelper _regexHelper;

        [JsonProperty] public string Url { get; private set; }
        [JsonProperty] public string UrlWithoutArguments { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string FileName { get; private set; }
        [JsonProperty] public SourceStatus IsValid { get; private set; }

        [JsonProperty] public bool IsPlaylist { get; private set; }
        [JsonProperty] public bool IsUser { get; private set; }
        [JsonProperty] public bool IsCommunity { get; private set; }
        [JsonProperty] public bool IsAbout { get; private set; }
        [JsonProperty] public bool IsChannels { get; private set; }
        [JsonProperty] public bool IsHome { get; private set; }

        public YoutubeUrl()
        {

        }

        public YoutubeUrl(string url, ConsoleHelper consoleHelper, AppConfiguration configuration, RegexHelper helper)
        {
            Url = url;
            
            _console = consoleHelper;
            _config = configuration;
            _regexHelper = helper;

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
            UrlWithoutArguments = Url;
            if (UrlWithoutArguments.Contains("?t"))
                UrlWithoutArguments = Url.Split("?t")[0];            
            else if (UrlWithoutArguments.Contains("&t"))
                UrlWithoutArguments = Url.Split("&t")[0];            
        }

        private void CheckIfValid()
        {
            if (Name == " - YouTube") // takedown - private or TOS violation
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Url invalid cause video is private or violate TOS.");
                IsValid = SourceStatus.Invalid;
            }
            else if (Name == "YouTube") // redirect home page
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Url invalid cause redirection to homepage.");
                IsValid = SourceStatus.Invalid;
            }
            else if ((String.IsNullOrEmpty(Name) || String.IsNullOrWhiteSpace(Name)) && !Url.Contains("/embed/")) // deleted channel, embeded videos doesen't have title, so htey should be considered valid
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Url invalid cause refer to deleted channel or private.");
                IsValid = SourceStatus.Invalid;
            }
            else if (Url.Contains("/user")) // lien de chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("User channel url.");
                IsValid = SourceStatus.Valid;
                IsUser = true;
            }
            else if (Url.Contains("/playlist")) // playlist
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Playlist");
                IsValid = SourceStatus.Valid;
                IsPlaylist = true;
            }
            else if (Url.Contains("/about")) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("About page");
                IsValid = SourceStatus.Valid;
                IsAbout = true;
            }
            else if (Url.Contains("/community")) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Community page");
                IsValid = SourceStatus.Valid;
                IsCommunity = true;
            }
            else if (Url.Contains("/featured")) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Home page");
                IsValid = SourceStatus.Valid;
                IsHome = true;
            }
            else if (Url.Contains("/channels")) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Home page");
                IsValid = SourceStatus.Valid;
                IsChannels = true;
            }
        }

        private void RetreiveName()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (_config.Throttle != 0)
                    {
                        _console.WriteLine($"Waiting {_config.Throttle} for throttleling...");
                        Thread.Sleep(1000 * _config.Throttle);
                    }
                                                            
                    if (Url.Contains("embed")) // mebed videos have no name in the title, but there's a reference to the orignal video that can be retrieved.
                        GetSourceUrlFromEmbededVideo(client);

                    string pageContent = client.DownloadString(Url);

                    string title = _regexHelper.ExtractYoutubeVideoNameFromPageRegex.Matches(pageContent).FirstOrDefault()?.Groups["name"].Value;

                    StringBuilder stripedTitleBuilder = new StringBuilder();
                    if (title.ToLower().EndsWith("youtube"))
                        stripedTitleBuilder.Append(string.Join("-", title.Split('-').SkipLast(1)));

                    Name = stripedTitleBuilder.ToString();

                    if(_config.Verbose)
                        _console.WriteLineInGray($"Retreiving name for {Url} - Name : {Name}");
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.TooManyRequests)
                        _console.WriteLineInRed($"URL: {Url} - Erreur: {ex.Message} - Retry in 50 seconds");
                    else
                        _console.WriteLineInRed($"URL: {Url} - Erreur: {ex.Message}");
                }
                catch (Exception e)
                {
                    _console.WriteLineInRed($"URL: {Url} - Erreur: {e.Message}");
                }
            } 
        }

        public void GetSourceUrlFromEmbededVideo(WebClient client)
        {
            string pageContent = client.DownloadString(Url);
            var matches = _regexHelper.ExtractYoutubeUrlFromEmbededVideoRegex.Match(pageContent);
            Url = HttpUtility.HtmlDecode(matches.Groups["url"].Value);
        }

        private void GenerateFileName()
        {
            if (String.IsNullOrEmpty(Name) || IsValid == SourceStatus.Invalid)
                return;

            // Make video name valid for use as path
            FileName = (Path.GetInvalidFileNameChars().Aggregate(Name, (current, c) => current.Replace(c, '_'))).Replace(' ', '_');

            //add videoId at the end
            FileName = $"{FileName}_[{GetvideoId()}]";

            if (_config.Verbose)
                _console.WriteLineInGray($"Filename for url {Url} - {FileName}");
        }

        private string GetvideoId()
        {
            var videoId = String.Empty;

            var matches = _regexHelper.YoutubeVideoIdFromUrlRegex.Matches(Url);

            foreach (Match match in matches)
                videoId = HttpUtility.HtmlDecode(match.Groups["videoid"].Value);

            if (videoId.Contains('?'))
                videoId = videoId.Split('?').ToList().First();
            if (videoId.Contains("&"))
                videoId = videoId.Split('&').ToList().First();
            return videoId;
        }
    }
}
