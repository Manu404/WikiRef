using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
        List<WikiPage> wikiPages = new List<WikiPage>();

        // Main method for the analyze verb
        private void AnalyzeReferences()
        {
            if (!String.IsNullOrEmpty(_api.ServerUrl))
            {
                if (!String.IsNullOrEmpty(_config.Category)) // if analysing category
                {
                    foreach (var page in _api.GetWikiPageInGivenCategory(_config.Category))
                    {
                        _console.WriteSection(String.Format("Analyzing page: {0}...", page.Name));
                        page.CheckPageStatus();
                    }
                }
                else if (!String.IsNullOrEmpty(_config.Page))
                {
                    _console.WriteSection(String.Format("Analyzing page: {0}...", _config.Page));
                    var page = new WikiPage(_config.Page, _console, _api, _config, _whitelistHandler);
                    page.CheckPageStatus();
                }
            }
        }

        // Main method for the youtube verb
        private void AnalyseYoutubeVideos()
        {
            if (String.IsNullOrEmpty(_api.ServerUrl))
                return;

            if (!String.IsNullOrEmpty(_config.Category)) // if treating cateogory
            {
                foreach (var page in _api.GetWikiPageInGivenCategory(_config.Category))
                {
                    _console.WriteSection(String.Format("Analyzing page: {0}...", page.Name));
                    page.BuildYoutubeLinkList();
                    wikiPages.Add(page);
                }
            }
            else if (!String.IsNullOrEmpty(_config.Page)) // if treating specific page
            {
                _console.WriteSection(String.Format("Analyzing page: {0}...", _config.Page));
                var newpage = new WikiPage(_config.Page, _console, _api, _config, _whitelistHandler);
                newpage.BuildYoutubeLinkList();
                wikiPages.Add(newpage);
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
                        _console.WriteLine(String.Format("Unique video links for page {0}: {1} - Total YouTube links", page.Name, page.AggregatedYoutubeUrls.Count, page.YoutubeUrls.Count));
                    }
                    else // Display raw list
                    {
                        _console.WriteSection("All video youtube refergences");
                        foreach (var video in page.YoutubeUrls)
                            _console.WriteLineInGray(String.Format("{0} - {1} - {2}", page.Name, video.Name, video.Url));
                        _console.WriteLine(String.Format("Youtube links for page {0}: {1} - Unique videos", page.Name, page.YoutubeUrls.Count, page.AggregatedYoutubeUrls.Count));
                    }
                }
                Console.WriteLine("Total YouTube links {0} - Total unique videos {1}", wikiPages.Select(o => o.YoutubeUrls.Count).Sum(), wikiPages.Select(o => o.AggregatedYoutubeUrls.Count).Sum());
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
            _api = new MediaWikiApi(_config.WikiUrl, _console, _config, _whitelistHandler);
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
            var p = new Program();
            p.ParseCommandlineArgument(args);
            p.SaveTextBuffer();
        }
    }
}
