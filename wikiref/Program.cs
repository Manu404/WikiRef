using CommandLine;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using WikiRef.Wiki;

namespace WikiRef
{
    class Program
    {
        //dependencies
        AppConfiguration _config;
        MediaWikiApi _api;
        ConsoleHelper _console;
        FileHelper _fileHelper;
        WhitelistHandler _whitelistHandler;
        RegexHelper _regexHelper;
        WikiPageCache _wikiPageCache;
        HtmlReportHelper _htmlReportBuilder;
        NetworkHelper _networkHelper;

        // Initialize dependencies and config
        private void InitializeDependencies(DefaultOptions options)
        {
            _config = new AppConfiguration(options);
            _htmlReportBuilder = new HtmlReportHelper();
            _console = new ConsoleHelper(_config, _htmlReportBuilder);
            _networkHelper = new NetworkHelper(_console);
            _fileHelper = new FileHelper(_console, _config);
            _whitelistHandler = new WhitelistHandler();
            _regexHelper = new RegexHelper();
            _api = new MediaWikiApi(_config.WikiUrl, _console, _config, _whitelistHandler, _regexHelper, _networkHelper);
            _wikiPageCache = new WikiPageCache(_api);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, AnalyseOptions, YoutubeDownloadOption>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option);
                })
                .WithParsed<YoutubeDownloadOption>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubeBashScriptBuilder(_config, _console, new JsonWikiPageCache(_fileHelper, _config)).ConstructBashScript();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WikiAnalyser(_api, _console, _wikiPageCache).AnalyseReferences().Wait();
                });
        }

        private void SaveConsoleToLog()
        {
            if (_config != null && (_config.ConsoleOutputToDefaultFile || !String.IsNullOrEmpty(_config.ConsoleOutputToFile)))
                _fileHelper.SaveConsoleOutputToFile(_config.ConsoleOutputToDefaultFile ? String.Empty : _config.ConsoleOutputToFile);
        }

        public void SaveConsoleToHtml()
        {
            if (_config != null && (_config.ConsoleOutputToDefaultHtmlFile || !String.IsNullOrEmpty(_config.ConsoleOutputToHtmlFile))) 
                _fileHelper.SaveConsoleOutputToHtmlFile(_htmlReportBuilder.BuildReportContent(), _config.ConsoleOutputToDefaultHtmlFile ? String.Empty : _config.ConsoleOutputToHtmlFile);
        }

        public void SaveWikiToJson()
        {
            if (_config != null && (_config.OutputJsonToDefaultFile || !String.IsNullOrEmpty(_config.OutputJsonToFile)))
                _fileHelper.SaveWikiPagesToJsonFile(_wikiPageCache.WikiPages, _config.OutputJsonToDefaultFile ? String.Empty : _config.OutputJsonToFile);
        }

        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var p = new Program();
            p.ParseCommandlineArgument(args);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            p._console.WriteLine(String.Format("Runtime: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));

            p.SaveWikiToJson();
            p.SaveConsoleToLog();
            p.SaveConsoleToHtml();
        }
    }
}
