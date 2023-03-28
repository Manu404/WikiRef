using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using WikiRef.Wiki;

namespace WikiRef
{
    class YoutubeBashScriptBuilder {
        private string _toolPath;
        private string _args;

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

            builder.AppendLine("#!/bin/bash");

            foreach (var page in _wikiPageCache.WikiPages)
            {
                var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), _config.DownloadRootFolder, page.Name);
                builder.AppendLine(String.Format("sudo mkdir -p \"{0}\"", outputFolder));

                foreach (var video in page.AggregatedYoutubeUrls)
                {
                    builder.Append(ConstructBashInstruction(page.Name, video));
                }
            }

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
                _console.WriteLineInOrange(String.Format("Download skipped. Invalid. Maybe private or violate TOS. {0} from {1}", video.Url, page));
                return String.Empty;
            }
            if (video.IsChannels && !_config.DownloadChannel)
            {
                _console.WriteLineInOrange(String.Format("Download skipped. Url is a channel {0} from {1}. Use --download-channel if you want to download it's content", video.Url, page));
                return String.Empty; 
            }
            if (video.IsPlaylist && !_config.DownloadPlaylist)
            {
                _console.WriteLineInOrange(String.Format("Download skipped. Url is a playlist {0} from {1}. Use --download-playlist if you want to download it's content", video.Url, page));
                return String.Empty;
            }
            else if (video.IsAbout || video.IsCommunity || video.IsHome || video.IsUser)
            {
                _console.WriteLineInOrange(String.Format("Download skipped. Url is valid a channel page {0} from {1} but not a video.", video.Url, page));
                return String.Empty;
            }

            StringBuilder builder = new StringBuilder();

            var outputFileName = String.Format("{0}.mp4", video.FileName);
            var destinationFile = GetFullFilePath(outputFileName, page);

            if (File.Exists(destinationFile) && !_config.DownloadRedownload)
            {
                return String.Empty;
            }
            else if(File.Exists(destinationFile) && _config.DownloadRedownload)
            {
                builder.AppendLine(String.Format("sudo rm -rf {0}", destinationFile));
            }

            builder.AppendLine(String.Format("sudo {0} {1}", _toolPath, FormatArguments(video, outputFileName, page)));

            return builder.ToString();
        }

        private string FormatArguments(YoutubeUrl video, string outputFile, string page)
        {
            return String.Format("{0} -o '{1}' {2}", _config.DownloadToolLocation, GetFullFilePath(outputFile, page).Replace("'", "_"), video.Url); ;
        }

        private string GetFullFilePath(string outputFile, string page)
        {
            return Path.Combine(_config.DownloadRootFolder, page, outputFile).Replace("'", "_");
        }
    }
}
