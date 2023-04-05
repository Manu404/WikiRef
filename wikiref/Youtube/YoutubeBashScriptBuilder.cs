using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WikiRef.Wiki;

namespace WikiRef
{
    class YoutubeBashScriptBuilder {

        private ConsoleHelper _console;
        private AppConfiguration _config;
        private JsonWikiPageCache _wikiPageCache;

        public YoutubeBashScriptBuilder(AppConfiguration config, ConsoleHelper console, JsonWikiPageCache wikiPageCache)
        {
            _config = config;
            _console = console;
            _wikiPageCache = wikiPageCache;
        }

        public void ConstructBashScript()
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            builder.AppendLine("#!/bin/bash");

            foreach (var page in _wikiPageCache.WikiPages)
            {
                foreach (var video in page.YoutubeUrls)
                {
                    i++;
                    builder.Append(ConstructBashInstruction(page.Name, video));
                }
            }

            _console.WriteLine($"{i} video treated");

            // Save script
            using (StreamWriter file = new StreamWriter(_config.DownloadOutpuScriptName))
            {
                file.WriteLine(builder.ToString());
            }
        }

        public string ConstructBashInstruction(string page, YoutubeUrl video)
        {
            if (video.IsValid == SourceStatus.Invalid)
            {
                if(_config.Verbose)
                    _console.WriteLineInOrange(String.Format("Download skipped. Invalid. Maybe private or violate TOS. {0} from {1}", video.AggregatedUrls, page));
                return String.Empty;
            }
            if (video.IsChannels && !_config.DownloadChannel)
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
            else if (video.IsAbout || video.IsCommunity || video.IsHome || video.IsUser)
            {
                if (_config.Verbose)
                    _console.WriteLineInOrange($"Download skipped. {video.AggregatedUrls} is valid a webpage page(s) from {page} but not a video.");
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder();

            var filename = $"{GetFileName(video)}.{_config.DownloadVideoFileExtension}";
            var desitnationFilename = GetOutputPath(filename, page);

            if (File.Exists(desitnationFilename) && !_config.DownloadRedownload)
            {
                if (_config.Verbose)
                    _console.WriteLine($"{desitnationFilename} exist.");
                return String.Empty;
            }
            else if(File.Exists(desitnationFilename) && _config.DownloadRedownload)
            {
                if (_config.Verbose)
                    _console.WriteLine($"remove and download {desitnationFilename}.");
                builder.AppendLine($"rm -rf {desitnationFilename}");
            }
            else
            {
                if (_config.Verbose)
                    _console.WriteLine($"{desitnationFilename} doesn't exist, add for download.");
            }

            builder.AppendLine($"{_config.DownloadToolLocation} {FormatArguments(video, filename, page)}");

            return builder.ToString();
        }

        public string GetFileName(YoutubeUrl video)
        {
            try
            {
                // Make video name valid for use as path
                var filename = GetValidPath(video.Name).Replace(' ', '_');

                //add videoId at the end
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

        private string FormatArguments(YoutubeUrl video, string outputFile, string page)
        {
            return $"{_config.DownloadToolArguments} -o \"{GetOutputPath(outputFile, page)}\" {GetUrlFromVideoId(video)}";
        }

        private string GetUrlFromVideoId(YoutubeUrl video)
        {
            return $"https://www.youtube.com/watch?v={video.VideoId}";
        }

        private string GetOutputPath(string filename, string page)
        {
            return Path.Combine(_config.DownloadRootFolder, GetValidPath(page), GetValidPath(filename)).Replace("'", "_").Replace("\"", "_");
        }

        private string GetValidPath(string path)
        {
            return Path.GetInvalidFileNameChars().Aggregate(path, (current, c) => current.Replace(c, '_'));
        }
    }
}
