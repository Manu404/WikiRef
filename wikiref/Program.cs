using CommandLine;
using System;
using System.Diagnostics;
using System.Threading;
using WikiRef.Wiki;

namespace WikiRef
{
    class Backup
    {
        AppConfiguration _config;
        ConsoleHelper _console;
        YoutubeVideoDownloader _youtubeVideoDownloader;
        WikiPageCache _wikiPageCache;

        public Backup(AppConfiguration config, ConsoleHelper console, YoutubeVideoDownloader youtubeVideoDownloader, WikiPageCache wikiPageCache)
        {
            _config = config;
            _console = console;
            _youtubeVideoDownloader = youtubeVideoDownloader;
            _wikiPageCache = wikiPageCache;
        }

        public void DownloadVideos()
        {
            _console.WriteSection("Downloading youtube videos");

            foreach (var page in _wikiPageCache.WikiPages)
                foreach (var video in page.AggregatedYoutubeUrls)
                {
                    if (_config.Throttle != 0)
                    {
                        _console.WriteLine(String.Format("Waiting {0} for throttleling...", _config.Throttle));
                        Thread.Sleep(1000 * _config.Throttle);
                    }
                    
                    _youtubeVideoDownloader.Download(page.Name, video);
                }
        }
    }

    class Program
    {
        //dependencies
        AppConfiguration _config;
        MediaWikiApi _api;
        ConsoleHelper _console;
        FileHelper _fileHelper;
        WhitelistHandler _whitelistHandler;
        YoutubeVideoDownloader _youtubeVideoDownloader;
        RegexHelper _regexHelper;
        WikiPageCache _wikiPageCache;

        // Initialize dependencies and config
        private void InitializeDependencies(DefaultOptions options, Action action)
        {
            _config = new AppConfiguration(options, action);
            _console = new ConsoleHelper(_config);
            _whitelistHandler = new WhitelistHandler();
            _regexHelper = new RegexHelper();
            _api = new MediaWikiApi(_config.WikiUrl, _console, _config, _whitelistHandler, _regexHelper);
            _fileHelper = new FileHelper(_console);
            _youtubeVideoDownloader = new YoutubeVideoDownloader(_console, _config);
            _wikiPageCache = new WikiPageCache(_api);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, YoutubeOptions, AnalyseOptions, BackuptOptions>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option, Action.Archive);
                })
                .WithParsed<YoutubeOptions>(option =>
                {
                    InitializeDependencies(option, Action.Youtube);
                    new YoutubAnalyser(_console,  _api, _config, _fileHelper, _wikiPageCache).AnalyseYoutubeVideos();
                })
                .WithParsed<BackuptOptions>(option =>
                {
                    InitializeDependencies(option, Action.Backup);
                    new Backup(_config, _console, _youtubeVideoDownloader, _wikiPageCache).DownloadVideos();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    InitializeDependencies(option, Action.Analyse);
                    new WikiAnalyser(_api, _console, _wikiPageCache).AnalyseReferences();
                });
        }

        private void SaveTextBuffer()
        {
            if (_config != null && _config.LogOutputToFile) // config null if no paramter given, init never done
                _fileHelper.SaveConsoleOutputToFile();
        }

        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var p = new Program();
            p.ParseCommandlineArgument(args);
            p.SaveTextBuffer();

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

        }
    }
}
