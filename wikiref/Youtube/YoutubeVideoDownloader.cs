using System;
using System.Diagnostics;
using System.IO;

namespace WikiRef
{
    class YoutubeVideoDownloader {
        private string _toolPath;
        private string _args;
        private string _rootFolder;

        private ConsoleHelper _console;
        private AppConfiguration _config;

        public YoutubeVideoDownloader(ConsoleHelper consoleHelper, AppConfiguration configuration)
        {
            _console = consoleHelper;
            _config = configuration;
            _toolPath = _config.DownloadToolLocation;
            _rootFolder = _config.DownloadRootFolder;
            _args = _config.DownloadArguments;
        }

        public void Download(string page, YoutubeUrl video)
        {
            try
            {
                if (video.IsValid == SourceStatus.Invalid)
                {
                    _console.WriteLineInOrange(String.Format("Download skipped. Invalid. Maybe private or violate TOS. {0} from {1}", video.Url, page));
                    return;
                }

                var outputFile = String.Format("{0}.mp4", video.FileName);
                var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), _rootFolder, page);

                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                var sourceFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outputFile));
                var destinationFile = Path.GetFullPath(Path.Combine(outputFolder, outputFile));

                if (File.Exists(destinationFile) && !_config.DownloadRedownload)
                {
                    _console.WriteLineInGray(String.Format("File already existe, download skipped. {0} - {1} from page {2}", video.Url, video.FileName, page));
                    return;
                }

                _console.WriteLineInGray(String.Format("Downloading {0} - {1} from page {2}", video.Url, video.FileName, page));

                DownloadVideo(video, outputFile);

                File.Move(sourceFile, destinationFile);

                _console.WriteLineInGray(String.Format("File {0} - {1} from page {2} archived.", video.Url, video.FileName, page));

            }
            catch (Exception ex)
            {
                _console.WriteLineInRed(String.Format("Error downloading {0} - {1} from page {2}", video.Url, video.FileName, page));
                _console.WriteLineInRed(ex.Message);
            }
        }

        private void DownloadVideo(YoutubeUrl video, string outputFile)
        {
            Process videoDownloaderCommand = new Process();
            videoDownloaderCommand.StartInfo.FileName = Path.GetFullPath(_toolPath);
            videoDownloaderCommand.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            var args = FormatArguments(video, outputFile);
            videoDownloaderCommand.StartInfo.Arguments = args;
            videoDownloaderCommand.StartInfo.UseShellExecute = false;
            videoDownloaderCommand.StartInfo.RedirectStandardOutput = true;
            videoDownloaderCommand.Start();
            videoDownloaderCommand.WaitForExit();
        }

        private string FormatArguments(YoutubeUrl video, string outputFile)
        {
            return String.Format("{0} -o  \"{1}\" \"{2}\"", _args, outputFile, video.Url);
        }
    }
}
