using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WikiRef
{
    class YoutubeUrl
    {
        private ConsoleHelper _console;
        private AppConfiguration _config;
        private RegexHelper _regexHelper;
        private NetworkHelper _networkHelper;

        [JsonProperty] public List<string> Urls { get; private set; }
        [JsonProperty] public string VideoId { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public SourceStatus IsValid { get; private set; }
        [JsonProperty] public bool IsPlaylist { get; private set; }
        [JsonProperty] public bool IsUser { get; private set; }
        [JsonProperty] public bool IsCommunity { get; private set; }
        [JsonProperty] public bool IsAbout { get; private set; }
        [JsonProperty] public bool IsChannels { get; private set; }
        [JsonProperty] public bool IsHome { get; private set; }
        [JsonProperty] public bool IsVideo { get; private set; }

        [JsonIgnore]
        public string AggregatedUrls => string.Join(" ", Urls);

        [JsonIgnore]
        public string VideoUrl => $"https://www.youtube.com/watch?v={VideoId}";

        public YoutubeUrl()
        {

        }

        public YoutubeUrl(string url, ConsoleHelper consoleHelper, AppConfiguration configuration, RegexHelper helper, NetworkHelper networkHelper)
        {
            _console = consoleHelper;
            _config = configuration;
            _regexHelper = helper;
            _networkHelper = networkHelper;

            Urls = new List<string>
            {
                url
            };

            CheckTypeOfUrl();
            if(IsVideo)
                VideoId = GetVideoId(url, _regexHelper);

            IsValid = SourceStatus.Undefined;
        }

        public static string GetVideoId(string url, RegexHelper regexHelper)
        {
            try
            {
                var match = regexHelper.ExtractYoutubeVideoIdFromUrlRegex.Match(url);
                return HttpUtility.HtmlDecode(match.Groups["videoid"].Value);
            }
            catch
            {
                return url;
            }
        }

        public async Task<SourceStatus> CheckIsValid()
        {
            await RetreivePageName();
            CheckValidity();
            return IsValid;
        }

        // Valid by default
        private void CheckValidity()
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
            else if ((String.IsNullOrEmpty(Name) || String.IsNullOrWhiteSpace(Name)) && !Urls.Contains("/embed/")) // deleted channel, embeded videos doesen't have title, so htey should be considered valid
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Url invalid cause refer to deleted channel or private.");
                IsValid = SourceStatus.Invalid;
            }
            else
            {
                IsValid = SourceStatus.Valid;
            }
        }

        private void CheckTypeOfUrl()
        {
            if (Urls.Any(n => n.Contains("/user"))) // lien de chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a user page.");
                IsUser = true;
            }
            else if (Urls.Any(n => n.Contains("/playlist"))) // playlist
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a playlist.");
                IsPlaylist = true;
            }
            else if (Urls.Any(n => n.Contains("/about"))) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is an about page.");
                IsAbout = true;
            }
            else if (Urls.Any(n => n.Contains("/community"))) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a community page.");
                IsCommunity = true;
            }
            else if (Urls.Any(n => n.Contains("/featured"))) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a featured/home page.");
                IsHome = true;
            }
            else if (Urls.Any(n => n.Contains("/channels")) || Urls.Any(n => n.Contains("/videos"))) // chaine
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is an home page.");
                IsChannels = true;
            }
            else
            {
                IsVideo = true;
            }
        }

        private async Task RetreivePageName()
        {
            try
            {
                // if already retrived, return
                if (!String.IsNullOrEmpty(Name))
                    return;

                if (_config.Throttle != 0)
                {
                    _console.WriteLine($"Waiting {_config.Throttle} for {Urls.First()} (throttling)...");
                    await Task.Delay(_config.Throttle * 1000);
                }

                string urlToVerify = String.Empty;

                if (Urls.Contains("embed")) // emebed videos have no name in the title, but there's a reference to the orignal video that can be retrieved.
                    urlToVerify = GetSourceUrlFromEmbededVideo();
                if (IsVideo)
                    urlToVerify = VideoUrl;
                else
                    urlToVerify = Urls.First();

                string pageContent = String.Empty;

                if (urlToVerify.Contains("/shorts/"))
                    pageContent = await _networkHelper.GetYoutubeShortContent(urlToVerify);
                else
                    pageContent = await _networkHelper.GetContent(urlToVerify, true);

                string title = _regexHelper.ExtractYoutubeVideoNameFromPageRegex.Matches(pageContent).FirstOrDefault()?.Groups["name"].Value;

                // strip " - youtube" from the title
                StringBuilder stripedTitleBuilder = new StringBuilder();
                if (title.ToLower().EndsWith("youtube"))
                    stripedTitleBuilder.Append(string.Join("-", title.Split('-').SkipLast(1)));

                Name = HttpUtility.HtmlDecode(stripedTitleBuilder.ToString());

                if(_config.Verbose)
                    _console.WriteLineInGray($"Retreiving name for {urlToVerify} - Name : {Name}");
            }
            catch (Exception e)
            {
                _console.WriteLineInRed($"URLs: {AggregatedUrls} - Erreur: {e.Message}");
            }
        }

        public string GetSourceUrlFromEmbededVideo()
        {
            string pageContent = _networkHelper.GetContent(Urls.First()).Result;
            var matches = _regexHelper.ExtractYoutubeUrlFromEmbededVideoRegex.Match(pageContent);
            return HttpUtility.HtmlDecode(matches.Groups["url"].Value);
        }
    }
}
