using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WikiRef
{
    enum SourceStatus
    {
        Valid = 0,
        Invalid = 1,
        Undefined = 2,
        //TakedownTos = 3,
        //Private = 4
    }

    enum Action
    {
        Analyse,
        Youtube,
        Archive,
        Backup,
        Undefined
    }

    class Program
    {
        AppConfiguration _config;
        MediaWikiApi _api;
        ConsoleHelper _console;
        FileHelper _fileHelper;
        WhitelistHandler _whitelistHandler;
        YoutubeVideoDownloader _youtubeVideoDownloader;
        RegexHelper _regexHelper;
        List<WikiPage> wikiPages = new List<WikiPage>();

        // Main method for the analyze verb
        private void AnalyzeReferences()
        {
            if (!String.IsNullOrEmpty(_api.ServerUrl))
            {
                foreach (var page in _api.GetWikiPages())
                {
                    _console.WriteSection(String.Format("Analyzing page: {0}...", page.Name));
                    page.CheckPageStatus();
                    page.CheckFormatting();
                }
            }
        }

        // Main method for the youtube verb
        private void AnalyseYoutubeVideos()
        {
            int youtubeLinkCount = 1;
            if (String.IsNullOrEmpty(_api.ServerUrl))
                return;


            //Parallel.ForEach(_api.GetWikiPages(), page =>
            //{
            foreach (var page in _api.GetWikiPages())
            { 
                _console.WriteSection(String.Format("Analyzing page: {0}...", page.Name));
                youtubeLinkCount = page.BuildYoutubeLinkList();
                wikiPages.Add(page);
            }

            if (_config.DisplayYoutubeUrlList)
            {
                foreach (var page in wikiPages)
                {
                    if (_config.AggrgateYoutubeUrl) // Display aggregated list
                    {
                        _console.WriteSection("Aggregate youtube video references");
                        foreach (var video in page.AggregatedYoutubeUrls)
                            _console.WriteLineInGray(String.Format("{0} - {1} - {2}", page.Name, video.Name, video.UrlWithoutArguments));


                        Console.WriteLine("Youtube : Page {0} - Total links {1} - Total valid unique links {2} - Total valid links {3}", page.Name,
                                                                                                                        page.YoutubeUrls.Count(),
                                                                                                                        page.AggregatedYoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count(),
                                                                                                                        page.YoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count());
                    }
                    else // Display raw list
                    {
                        _console.WriteSection("All video youtube refergences");
                        foreach (var video in page.YoutubeUrls)
                            _console.WriteLineInGray(String.Format("{0} - {1} - {2}", page.Name, video.Name, video.Url));
                        _console.WriteLine(String.Format("Youtube links for page {0}: {1} - Unique videos", page.Name, page.YoutubeUrls.Count, page.AggregatedYoutubeUrls.Count));
                    }
                }

                Console.WriteLine("Youtube : Total links {0} - Total valid unique links {1} - Total valid links {2}", youtubeLinkCount,
                                                                                                                        wikiPages.SelectMany(o => o.AggregatedYoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count(), 
                                                                                                                        wikiPages.SelectMany(o => o.YoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count());
            }

            if (_config.OutputYoutubeUrlJson)
                _fileHelper.SaveJsonToFile(wikiPages);

            if(_config.Action == Action.Backup)
            {
                _console.WriteSection("Downloading youtube videos");
                foreach (var page in wikiPages)
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

        // Initialize dependencies and config
        private void Initialize(DefaultOptions options, Action action)
        {
            _config = new AppConfiguration(options, action);
            _console = new ConsoleHelper(_config);
            _whitelistHandler = new WhitelistHandler();
            _regexHelper = new RegexHelper();
            _api = new MediaWikiApi(_config.WikiUrl, _console, _config, _whitelistHandler, _rege);
            _fileHelper = new FileHelper(_console);
            _youtubeVideoDownloader = new YoutubeVideoDownloader(_console, _config);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, YoutubeOptions, AnalyseOptions, BackuptOptions>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    Initialize(option, Action.Archive);
                })
                .WithParsed<YoutubeOptions>(option =>
                {
                    Initialize(option, Action.Youtube);
                    AnalyseYoutubeVideos();
                })
                .WithParsed<BackuptOptions>(option =>
                {
                    Initialize(option, Action.Backup);
                    AnalyseYoutubeVideos();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    Initialize(option, Action.Analyse);
                    AnalyzeReferences();
                });
        }

        private void SaveTextBuffer()
        {
            if (_config != null && _config.ConsoleOutputToFile) // config null if no paramter given, init never done
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
