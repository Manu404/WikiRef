using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WikiRef.Common;
using WikiRef.Data;

namespace WikiRef
{
    class YoutubeUrl : Data.YoutubeUrl
    {
        private IConsole _console;
        private IAppConfiguration _config;
        private IRegexHelper _regexHelper;
        private INetworkHelper _networkHelper;

        public YoutubeUrl()
        {

        }

        public YoutubeUrl(IAppConfiguration configuration, IConsole console, IRegexHelper regexHelper, INetworkHelper networkHelper, string url)
        {
            _console = console;
            _config = configuration;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper;

            Urls = new List<string>
            {
                url
            };

            IsValid = SourceStatus.NotSet;

            try
            {
                VideoId = GetVideoId(url, _regexHelper);
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"Error retreiving VideoID for url {url}");
            }

            try
            {
                SetUrlType();
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"Error checking validity for url {url}");
            }
        }

        public static string GetVideoId(string url, IRegexHelper regexHelper)
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

        public async Task<SourceStatus> FetchPageName()
        {
            await RetreivePageName();
            SetValidity();
            return IsValid;
        }

        private void SetValidity()
        {
            if (Name == " - YouTube")
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Invalid URL - video is private or violate TOS.");
                IsValid = SourceStatus.Invalid;
            }
            else if (Name == "YouTube")
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Invalid URL - redirection to homepage.");
                IsValid = SourceStatus.Invalid;
            }
            else if ((String.IsNullOrEmpty(Name) || String.IsNullOrWhiteSpace(Name)) && !Urls.Contains("/embed/")) // deleted channel, embeded videos doesen't have title, so htey should be considered valid
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange("Invalid URL - refer to deleted channel or private.");
                IsValid = SourceStatus.Invalid;
            }
            else
            {
                IsValid = SourceStatus.Valid;
            }
        }

        private void SetUrlType()
        {
            if (Urls.Any(n => n.Contains("/user")))
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a user page.");
                IsUser = true;
            }
            else if (Urls.Any(n => n.Contains("/playlist"))) 
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a playlist.");
                IsPlaylist = true;
            }
            else if (Urls.Any(n => n.Contains("/about")))
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is an about page.");
                IsAbout = true;
            }
            else if (Urls.Any(n => n.Contains("/community")))
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a community page.");
                IsCommunity = true;
            }
            else if (Urls.Any(n => n.Contains("/featured")))
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is a featured/home page.");
                IsHome = true;
            }
            else if (Urls.Any(n => n.Contains("/channels")) || Urls.Any(n => n.Contains("/videos")))
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"{Urls.First()} is an home page.");
                IsChannel = true;
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
                if (!String.IsNullOrEmpty(Name))
                    return;

                if (_config.Throttle != 0)
                {
                    _console.WriteLine($"Waiting {_config.Throttle} seconds for {Urls.First()} (throttling)...");
                    await Task.Delay(_config.Throttle * 1000);
                }

                string urlToVerify;
                if (Urls.Contains("embed"))
                    urlToVerify = GetSourceUrlFromEmbeddedVideo();
                else if (IsVideo)
                    urlToVerify = VideoUrl;
                else
                    urlToVerify = Urls.First();

                string pageContent;
                if (urlToVerify.Contains("/shorts/"))
                    pageContent = await _networkHelper.GetYoutubeShortContent(urlToVerify);
                else
                    pageContent = await _networkHelper.GetContent(urlToVerify, true);

                string title = _regexHelper.ExtractYoutubeVideoNameFromPageRegex.Matches(pageContent).FirstOrDefault()?.Groups["name"].Value;

                // strip " - youtube" from the title
                if (title.ToLower().EndsWith("youtube"))
                    title = string.Join("-", title.Split('-').SkipLast(1));

                Name = HttpUtility.HtmlDecode(title);

                ChannelName = _regexHelper.ExtractChannelNameRegex.Matches(pageContent.Replace("\"", "")).FirstOrDefault()?.Groups["name"].Value;

                if (_config.Verbose)
                    _console.WriteLineInGray($"Retrieving name for {urlToVerify} - Name : {Name}");
            }
            catch (Exception e)
            {
                _console.WriteLineInRed($"Urls: {AggregatedUrls} - Error: {e.Message}");
            }
        }

        public string GetSourceUrlFromEmbeddedVideo()
        {
            string pageContent = _networkHelper.GetContent(Urls.First()).Result;
            var matches = _regexHelper.ExtractYoutubeUrlFromEmbeddedVideoRegex.Match(pageContent);
            return HttpUtility.HtmlDecode(matches.Groups["url"].Value);
        }
    }
}
