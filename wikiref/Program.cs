using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WikiRef.Common;
using WikiRef.Report;
using WikiRef.Wiki;

namespace WikiRef
{
    class Program
    {
        IAppConfiguration _config;
        IConsole _console;
        IFileHelper _fileHelper;
        INetworkHelper _networkHelper;
        WhiteListHelper _whitelistHelper;
        IRegexHelper _regexHelper;
        MediaWikiApi _api;
        WikiRefCache _wikiRefCache;

        static void Main(string[] args)
        {
            Program app = new Program();
            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();
            app.ParseCommandlineArgument(args);
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            app._console.WriteLine(String.Format("Runtime: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));

            app.SaveFiles();
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<AnalyzeOptions, ArchiveOptions, YoutubeDownloadOptions, PublishOptions>(args)
                .WithParsed<AnalyzeOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WikiAnalyzer(_config, _console, _wikiRefCache).AnalyzeReferences().Wait();
                })
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WayBackMachineArchiver(_config, _console, _networkHelper, _fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson)).Archive().Wait();
                })
                .WithParsed<YoutubeDownloadOptions>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubeBashScriptBuilder(_config, _console, _regexHelper, _fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson)).BuildBashScript();
                })
                .WithParsed<PublishOptions>(option =>
                 {
                     InitializeDependencies(option);
                     new ReportPublisher(_config, _console, new ReportBuilder(_fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson))).Publish().Wait();
                 });
        }

        private void InitializeDependencies(DefaultOptions options)
        {
            _config = new AppConfiguration(options);
            _console = new ConsoleHelper(_config, new ConsoleHtmlBuffer());
            _fileHelper = new FileHelper(_config, _console);
            _networkHelper = new NetworkHelper(_config, _console);
            _whitelistHelper = new WhiteListHelper(String.IsNullOrEmpty(_config.WhiteList) ? new List<string>() : _fileHelper.LoadWhiteListFromJsonFile(_config.WhiteList));
            _regexHelper = new RegexHelper();
            _api = new MediaWikiApi(_config, _console, _whitelistHelper, _regexHelper, _networkHelper);
            _wikiRefCache = new WikiRefCache(_config, _console, _api, _whitelistHelper);
        }

        private void SaveFiles()
        {
            _fileHelper.SaveWikiRefCacheToJsonFile(_wikiRefCache);
            _fileHelper.SaveWikiRefCacheReferencesContentToTextFile(_wikiRefCache);
            _fileHelper.SaveConsoleOutputToHtmlFile();
            _fileHelper.SaveConsoleOutputToLogFile();
        }
    }
}
