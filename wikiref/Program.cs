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
        ReportHelper _reportHelper;

        // Initialize dependencies and config
        private void InitializeDependencies(DefaultOptions options)
        {
            _config = new AppConfiguration(options);
            _reportHelper = new ReportHelper();
            _console = new ConsoleHelper(_config, _reportHelper);
            _whitelistHandler = new WhitelistHandler();
            _regexHelper = new RegexHelper();
            _api = new MediaWikiApi(_config.WikiUrl, _console, _config, _whitelistHandler, _regexHelper);
            _fileHelper = new FileHelper(_console, _config);
            _wikiPageCache = new WikiPageCache(_api);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, YoutubeOptions, AnalyseOptions, YoutubeDownloadOption>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option);
                })
                .WithParsed<YoutubeOptions>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubAnalyser(_console,  _api, _config, _fileHelper, _wikiPageCache).AnalyseYoutubeVideos();
                })
                .WithParsed<YoutubeDownloadOption>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubeBashScriptBuilder(_config, _console, new JsonWikiPageCache(_fileHelper, _config)).ConstructBashScript();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WikiAnalyser(_api, _console, _wikiPageCache).AnalyseReferences();
                });
        }

        private void SaveTextBuffer()
        {
            if (_config != null && _config.LogOutputToFile) // config null if no paramter given, init never done
                _fileHelper.SaveConsoleOutputToFile();
        }

        private void SaveReport()
        {
            if (_config != null && _config.ConsoleToHtml) // config null if no paramter given, init never done
                _fileHelper.SaveConsoleOutputToHtmlFile(_reportHelper.GetReportContent());
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

            p.SaveTextBuffer();
            p.SaveReport();
        }
    }
}
