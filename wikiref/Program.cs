using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WikiRef.Commons;
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
        WikiPageCache _wikiPageCache;
        WhitelistHandler _whitelistHandler;

        // Initialize dependencies and config
        private void InitializeDependencies(DefaultOptions options)
        {
            (_bootStrapper = new BootStrapper()).InitializeDependencies(options);
            _config = new AppConfiguration(options);
            _fileHelper = new FileHelper(_bootStrapper.ConsoleHelper);
            _whitelistHandler = new WhitelistHandler(_config, _fileHelper);
            _api = new MediaWikiApi(_bootStrapper.ConsoleHelper, _config, _whitelistHandler, _bootStrapper.RegexHelper, _bootStrapper.NetworkHelper);
            _wikiPageCache = new WikiPageCache(_api);
        }

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<ArchiveOptions, AnalyseOptions, YoutubeDownloadOption>(args)
                .WithParsed<ArchiveOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WayBackMachineArchiver(_config, _bootStrapper.ConsoleHelper, _bootStrapper.NetworkHelper, new JsonWikiPageCache(_fileHelper, _config)).Archive().Wait();
                })
                .WithParsed<YoutubeDownloadOption>(option =>
                {
                    InitializeDependencies(option);
                    new YoutubeBashScriptBuilder(_config, _bootStrapper.ConsoleHelper, new JsonWikiPageCache(_fileHelper, _config)).ConstructBashScript();
                })
                .WithParsed<AnalyseOptions>(option =>
                {
                    InitializeDependencies(option);
                    new WikiAnalyser(_api, _bootStrapper.ConsoleHelper, _wikiPageCache).AnalyseReferences().Wait();
                });
        }

        public void SaveWikiToJson()
        {
            if (_config != null && (_config.OutputJsonToDefaultFile || !String.IsNullOrEmpty(_config.OutputJsonToFile)))
            {
                string filename = _config.OutputJsonToDefaultFile ? String.Empty : _config.OutputJsonToFile;
                string dirname = _config.PutInSubDirectory ? ".json" : String.Empty;
                _fileHelper.SaveWikiPagesToJsonFile(_wikiPageCache.WikiPages, filename, dirname);
            }
        }

        public void SaveRefText()
        {
            if (_config != null && _config.ExportRefText)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var reference in _wikiPageCache.WikiPages.SelectMany(p => p.References).Select(r => r.Content))
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
