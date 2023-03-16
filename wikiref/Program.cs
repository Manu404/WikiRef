using CommandLine;
using System;
using System.Collections.Generic;


namespace WikiRef
{
    enum SourceStatus
    {
        Valid = 0,
        Invalid = 1,
        Undefined = 2
    }

    enum Action
    {
        Analyse,
        Youtube,
        Archive,
        Undefined
    }

    class Program
    {
        GlobalConfiguration _config;
        MediaWikiApi _api;
        ConsoleHelper _consoleHelper;
        FileHelper _fileHelper;
        WhitelistHandler _whitelistHandler;

        // Main method for the analyze verb
        private void AnalyzeReferences()
        {
            if (!String.IsNullOrEmpty(_api.ServerUrl))
            {
                if (!String.IsNullOrEmpty(_config.Category)) // if analysing category
                {
                    foreach (var page in _api.GetWikiPageInGivenCategory(_config.Category))
                    {
                        _consoleHelper.DisplayCheckingPageMessage(page.Name);
                        page.CheckPageStatus();
                    }
                }
                else if (!String.IsNullOrEmpty(_config.Page))
                {
                    _consoleHelper.DisplayCheckingPageMessage(_config.Page);
                    var page = new WikiPage(_config.Page, _consoleHelper, _api, _config, _whitelistHandler);
                    page.CheckPageStatus();
                }
            }
        }

        // Main method for the youtube verb
        private void GetYoutubeLinkList()
        {
            List<WikiPage> pages = new List<WikiPage>();

            if (!String.IsNullOrEmpty(_api.ServerUrl))                      
                if (!String.IsNullOrEmpty(_config.Category)) // if treating cateogory
                {
                    foreach (var page in _api.GetWikiPageInGivenCategory(_config.Category))
                    {
                        _consoleHelper.DisplayCheckingPageMessage(page.Name);
                        page.BuildYoutubeLinkList();
                        pages.Add(page);
                    }
                }
                else if (!String.IsNullOrEmpty(_config.Page)) // if treating specific page
                {
                    _consoleHelper.DisplayCheckingPageMessage(_config.Page);
                    var newpage = new WikiPage(_config.Page, _consoleHelper, _api, _config, _whitelistHandler);
                    newpage.BuildYoutubeLinkList();
                    pages.Add(newpage);
                }

            if (_config.AggrgateYoutubeUrl)
                foreach (var page in pages)
                    page.AggregateYoutubUrl();

            if (_config.DisplayYoutubeUrlList)
            {
                int urlCount = 0;
                foreach (var page in pages)
                    foreach (var video in page.YoutubeUrls)
                    {
                        _consoleHelper.WriteLineInGray(String.Format("{0} - {1}", page.Name, video.Url));
                        urlCount += 1;
                    }
                _consoleHelper.WriteLine(String.Format("Unique links: {0}", urlCount));
            }

            if (_config.OutputYoutubeUrlJson)
                _fileHelper.SaveJsonToFile(pages);
        }

        // Initialize dependencies and config
        private void Initialize(DefaultOptions options, Action action)
        {
            _config = new GlobalConfiguration(options, action);
            _consoleHelper = new ConsoleHelper(_config);
            _whitelistHandler = new WhitelistHandler();
            _api = new MediaWikiApi(_config.WikiUrl, _consoleHelper, _config, _whitelistHandler);
            _fileHelper = new FileHelper(_consoleHelper);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<WaybackLachineArchivingOptions, YoutubeOptions, AnalyseOptions>(args)
                .WithParsed<WaybackLachineArchivingOptions>(option =>
                {
                    Initialize(option, Action.Archive);
                })
                .WithParsed<YoutubeOptions>(option =>
                {
                    Initialize(option, Action.Youtube);
                    GetYoutubeLinkList();
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
