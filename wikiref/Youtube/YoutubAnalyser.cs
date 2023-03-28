using System;
using System.Linq;
using System.Threading.Tasks;
using WikiRef.Wiki;

namespace WikiRef
{
    class YoutubAnalyser
    {
        ConsoleHelper _console;
        MediaWikiApi _api;
        AppConfiguration _config;
        FileHelper _fileHelper;
        WikiPageCache _wikiPageCache;

        public YoutubAnalyser(ConsoleHelper console, MediaWikiApi api, AppConfiguration config, FileHelper fileHelper, WikiPageCache wikiPageCache)
        {
            _console = console;
            _api = api;
            _config = config;
            _fileHelper = fileHelper;
            _wikiPageCache = wikiPageCache;
        }

        public void AnalyseYoutubeVideos()
        {
            BuildYoutubeLinknList();
            DisplayYoutubeLinkList();
            SaveJsonToFile();
        }

        private void BuildYoutubeLinknList()
        {
            foreach(var page in _wikiPageCache.WikiPages)
            {
                _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                page.BuildYoutubeLinkList();
            }
        }

        private void DisplayYoutubeLinkList()
        {
            if (_config.DisplayYoutubeUrlList)
            {
                foreach(var page in _wikiPageCache.WikiPages)
                {
                    if (_config.AggrgateYoutubeUrl) // Display aggregated list
                    {
                        _console.WriteSection("Aggregated youtube references");
                        foreach (var video in page.AggregatedYoutubeUrls)
                            _console.WriteLineInGray(string.Format("{0} - {1} - {2}", page.Name, video.Name, video.UrlWithoutArguments));

                        _console.WriteLineInDarkCyan(string.Format("Youtube : Page {0} - Total links {1} - Total valid links {2} - Total valid unique links {3}", page.Name, 
                            page.YoutubeUrls.Count, 
                            page.YoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count(),
                            page.AggregatedYoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count()));
                    }
                    else // Display raw list
                    {
                        _console.WriteSection("All youtube references");
                        foreach (var video in page.YoutubeUrls)
                            _console.WriteLineInGray(string.Format("{0} - {1} - {2}", page.Name, video.Name, video.Url));
                        _console.WriteLine(string.Format("Youtube links for page {0}: {1}", page.Name, page.YoutubeUrls.Count));
                    }
                }
            }

            _console.WriteSection("Summary");
            _console.WriteLineInDarkCyan(String.Format("Youtube : Total links {0} - Total valid unique links {1} - Total valid links {2}",
                    _wikiPageCache.WikiPages.SelectMany(o => o.References).Count(),
                    _wikiPageCache.WikiPages.SelectMany(o => o.YoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count(),
                    _wikiPageCache.WikiPages.SelectMany(o => o.AggregatedYoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count()));

            if (_wikiPageCache.WikiPages.SelectMany(o => o.YoutubeUrls).Any(v => v.IsValid == SourceStatus.Invalid || v.IsValid == SourceStatus.Undefined))
                PrintErrors();
            else
                _console.WriteLineInGreen(String.Format("All references seems valid"));
        }

        private void PrintErrors()
        {
            _console.WriteSection("Invalid references");
            _console.WriteLineInRed(String.Format("Some references seems invalid, check the error message and/or the wikicode for malformated refrerences or invalid urls"));
            foreach (var page in _wikiPageCache.WikiPages.Where(p => p.YoutubeUrls.Exists(v => v.IsValid == SourceStatus.Invalid || v.IsValid == SourceStatus.Undefined)))
            {
                _console.WriteLine(String.Format("Page {0}", page.Name));
                foreach (var invalidUrl in page.YoutubeUrls.Where(v => v.IsValid == SourceStatus.Invalid || v.IsValid == SourceStatus.Undefined))
                {
                    _console.WriteLineInRed(String.Format(" => Invalid url: {0}", invalidUrl.Url));
                }                
            }
        }

        private void SaveJsonToFile()
        {
            if (String.IsNullOrEmpty(_config.YoutubeAnalysisOutputFilename)) return;
            _fileHelper.SaveWikiPagesToJsonFile(_wikiPageCache.WikiPages.ToList(), _config.YoutubeAnalysisOutputFilename);
        }
    }
}
