using System;
using System.IO;
using System.Linq;
using System.Text;
using WikiRef.Common;
using WikiRef.Data;
using WikiRef.Wiki;

namespace WikiRef
{
    class YoutubeBashScriptBuilder {

        private IConsole _console;
        private IAppConfiguration _config;
        private IRegexHelper _regexHelper;
        private WikiRefCache _wikiRefCache;

        public YoutubeBashScriptBuilder(IAppConfiguration config, IConsole console, IRegexHelper regexHelper, WikiRefCache wikiPageCache)
        {
            _config = config;
            _console = console;
            _regexHelper = regexHelper;
            _wikiRefCache = wikiPageCache;
        }

        public void BuildBashScript()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("#!/bin/bash");

            foreach(var page in _wikiRefCache.Wiki.Namespaces.SelectMany(ns => ns.Pages))
                foreach (var video in page.YoutubeUrls)
                    builder.Append(BuildBashScriptInstruction(page.Name, video));

            _console.WriteLine($"{_wikiRefCache.Wiki.Namespaces.SelectMany(ns => ns.Pages).SelectMany(page => page.YoutubeUrls).Count()} video treated");

            using (StreamWriter file = new StreamWriter(_config.DownloadOutputScriptName))
            {
                file.WriteLine(builder.ToString());
            }
        }

        private string BuildBashScriptInstruction(string page, Data.YoutubeUrl video)
        {
            if (video.IsValid == SourceStatus.Invalid)
            {
                if(_config.Verbose)
                    _console.WriteLineInOrange(String.Format("Download skipped. Invalid. Maybe private or violate TOS. {0} from {1}", video.AggregatedUrls, page));
                return String.Empty;
            }
            if (video.IsChannel && !_config.DownloadChannel)
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange(String.Format("Download skipped. Url is a channel {0} from {1}. Use --download-channel if you want to download it's content", video.AggregatedUrls, page));
                return String.Empty; 
            }
            if (video.IsPlaylist && !_config.DownloadPlaylist)
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange(String.Format("Download skipped. Url is a playlist {0} from {1}. Use --download-playlist if you want to download it's content", video.AggregatedUrls, page));
                return String.Empty;
            }
            else if (!video.IsVideo)
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"Download skipped. {video.AggregatedUrls} is valid a webpage page(s) from {page} but not a video.");
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder();

            string filename = $"{GetFileName(video)}.{_config.DownloadVideoFileExtension}";
            string destinationFilename = GetOutputPath(filename, page);
            bool fileExists = false;

            if (Directory.Exists(GetOutputDirectory(page))) 
                foreach(var file in Directory.GetFiles(GetOutputDirectory(page)))
                {
                    var youtubeVideoId = _regexHelper.ExtractYoutubeIdFromFileNameRegex.Matches(file);
                    if (youtubeVideoId != null && youtubeVideoId.Last().Groups["id"].Value == video.VideoId)
                        fileExists = true;
                }

            if (fileExists && !_config.Redownload)
            {
                if (_config.Verbose)
                    _console.WriteLine($"{destinationFilename} exist.");
                return String.Empty;
            }
            else if(fileExists && _config.Redownload)
            {
                if (_config.Verbose)
                    _console.WriteLine($"remove and download {destinationFilename}.");
                builder.AppendLine($"rm -rf {destinationFilename}");
            }
            else
            {
                if (_config.Verbose)
                    _console.WriteLine($"{destinationFilename} doesn't exist, add for download.");
            }

            builder.AppendLine($"{_config.DownloadToolLocation} {FormatArguments(video, filename, page)}");

            return builder.ToString();
        }

        private string GetFileName(Data.YoutubeUrl video)
        {
            try
            {
                string filename = GetValidPath(video.Name).Replace(' ', '_');

                filename = $"{filename}_[{video.VideoId}]";

                if (_config.Verbose)
                    _console.WriteLineInGray($"Filename for url {video.Name} - {filename}");

                return filename;
            }
            catch (Exception ex) 
            {
                if (_config.Verbose)
                    _console.WriteLineInRed($"Can't generate filename for {video.AggregatedUrls} - [{video.VideoId}] - {ex.Message}");
                return string.Empty;
            }
        }

        private string FormatArguments(Data.YoutubeUrl video, string outputFile, string page)
        {
            return $"{_config.DownloadToolArguments} -o \"{GetOutputPath(outputFile, page)}\" {GetUrlFromVideoId(video)}";
        }

        private string GetUrlFromVideoId(Data.YoutubeUrl video)
        {
            return $"https://www.youtube.com/watch?v={video.VideoId}";
        }

        private string GetOutputPath(string filename, string page)
        {
            return Path.Combine(_config.DownloadRootFolder, GetValidPath(page), GetValidPath(filename)).Replace("'", "_").Replace("\"", "_");
        }

        private string GetOutputDirectory(string page)
        {
            return Path.Combine(_config.DownloadRootFolder, GetValidPath(page)).Replace("'", "_").Replace("\"", "_");
        }

        private string GetValidPath(string path)
        {
            return Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c, '_'));
        }
    }
}
