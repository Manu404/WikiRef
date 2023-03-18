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

        public void Download(string page, YoutubeVideo video)
        {
            try
            {
                var outputFile = String.Format("{0}.mp4", video.FileName);
                if (String.IsNullOrEmpty(video.FileName)) {
                    _console.WriteLineInRed(String.Format("Can't download {0} from {1} - maybe private or violate TOS", video.Url, page));
                    return;
                }

                var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), _rootFolder, page);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                var sourceFile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outputFile));
                var destinationFile = Path.GetFullPath(Path.Combine(outputFolder, outputFile));

                if (File.Exists(destinationFile) && !_config.DownloadRedownload)
                {
                    _console.WriteLineInGray(String.Format("File {0} - {1} from page {2} already exists. Download skipped.", video.Url, video.FileName, page));
                    return;
                }

                _console.WriteLineInGray(String.Format("Downloading {0} - {1} from page {2}", video.Url, video.FileName, page));
                
                Process videoDownloaderCommand = new Process();
                videoDownloaderCommand.StartInfo.FileName = Path.GetFullPath(_toolPath);
                videoDownloaderCommand.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                var args = FormatArguments(video, outputFile);
                videoDownloaderCommand.StartInfo.Arguments = args;
                videoDownloaderCommand.StartInfo.UseShellExecute = false;
                videoDownloaderCommand.StartInfo.RedirectStandardOutput = true;
                videoDownloaderCommand.Start();

                Console.WriteLine(videoDownloaderCommand.StandardOutput.ReadToEnd());

                videoDownloaderCommand.WaitForExit();

                File.Move(sourceFile, destinationFile);

                _console.WriteLineInGray(String.Format("File {0} - {1} from page {2} archived.", video.Url, video.FileName, page));

            }
            catch (Exception ex)
            {
                _console.WriteLineInRed(String.Format("Error downloading {0} - {1} from page {2}", video.Url, video.FileName, page));
                _console.WriteLineInRed(ex.Message);
            }
        }

        private string FormatArguments(YoutubeVideo video, string outputFile)
        {
            return String.Format("{0} -o  \"{1}\" \"{2}\"", _args, outputFile, video.Url);
        }
    }
}
