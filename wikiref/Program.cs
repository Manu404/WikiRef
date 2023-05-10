using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WikiRef.Commons;
using WikiRef.Report;
using WikiRef.Wiki;

namespace WikiRef
{
    class Program
    {
        //dependencies
        BootStrapper _bootStrapper;
        AppConfiguration _config;
        MediaWikiApi _api;
        FileHelper _fileHelper;
        WikiRefCache _wikiRefCache;
        WhitelistHandler _whitelistHandler;
        ReportBuilder _reportBuilder;

        // Initialize dependencies and config
        private void InitializeDependencies(DefaultOptions options)
        {
            (_bootStrapper = new BootStrapper()).InitializeDependencies(options);
            _config = new AppConfiguration(options);
            _fileHelper = new FileHelper(_bootStrapper.ConsoleHelper);
            _whitelistHandler = new WhitelistHandler(_config, _fileHelper);
            _api = new MediaWikiApi(_bootStrapper.ConsoleHelper, _config, _whitelistHandler, _bootStrapper.RegexHelper, _bootStrapper.NetworkHelper);
            _wikiRefCache = new WikiRefCache(_config, _api, _whitelistHandler, _bootStrapper.ConsoleHelper);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, AnalyseOptions, YoutubeDownloadOption, PublishOptions>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WayBackMachineArchiver(_config, _bootStrapper.ConsoleHelper, _bootStrapper.NetworkHelper, _fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson)).Archive().Wait();
                })
                .WithParsed<YoutubeDownloadOption>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubeBashScriptBuilder(_config, _bootStrapper.ConsoleHelper, _fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson)).ConstructBashScript();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WikiAnalyser(_api, _bootStrapper.ConsoleHelper, _wikiRefCache, _config).AnalyseReferences().Wait();
                })
                .WithParsed<PublishOptions>(option =>
                 {
                     InitializeDependencies(option);
                     new ReportPublisher(_config, new ReportBuilder(_fileHelper.LoadWikiRefCacheFromJsonFile(_config.Inputjson)), _bootStrapper.ConsoleHelper).Publish().Wait();
                 });
        }

        public void SaveWikiToJson()
        {
            if (_config != null && (_config.OutputJsonToDefaultFile || !String.IsNullOrEmpty(_config.OutputJsonToFile)))
            {
                string filename = _config.OutputJsonToDefaultFile ? String.Empty : _config.OutputJsonToFile;
                string dirname = _config.PutInSubDirectory ? ".json" : String.Empty;
                _fileHelper.SaveWikiRefCacheToJsonFile(_wikiRefCache, filename, dirname);
            }
        }

        public void SaveRefText()
        {
            if (_config != null && _config.ExportRefText)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var reference in _wikiRefCache.Wiki.Namespaces.SelectMany( p => p.Pages).SelectMany(p => p.References).Select(r => r.Content))
                {
                    builder.AppendLine(reference);
                }
                _fileHelper.SaveTextTofile(builder.ToString(), "", "", ".txt");
            }
        }

        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var p = new Program();
            p.ParseCommandlineArgument(args);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            p._bootStrapper.ConsoleHelper.WriteLine(String.Format("Runtime: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));

            p.SaveWikiToJson();
            p._bootStrapper.SaveConsoleToHtml();
            p._bootStrapper.SaveConsoleToLog();
            p.SaveRefText();
        }
    }
}
