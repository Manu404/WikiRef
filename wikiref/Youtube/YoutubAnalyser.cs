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

        // Main method for the youtube verb
        public void AnalyseYoutubeVideos()
        {
            BuildYoutubeLinknList();
            DisplayYoutubeLinkList();
            SaveJsonToFile();
        }
        private void BuildYoutubeLinknList()
        {
            Parallel.ForEach(_wikiPageCache.WikiPages, page =>
            {
                _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                page.BuildYoutubeLinkList();
            });
        }

        private void DisplayYoutubeLinkList()
        {
            if (_config.DisplayYoutubeUrlList)
            {
                Parallel.ForEach(_wikiPageCache.WikiPages, page =>
                {
                    if (_config.AggrgateYoutubeUrl) // Display aggregated list
                    {
                        _console.WriteSection("Aggregate youtube video references");
                        foreach (var video in page.AggregatedYoutubeUrls)
                            _console.WriteLineInGray(string.Format("{0} - {1} - {2}", page.Name, video.Name, video.UrlWithoutArguments));

                        _console.WriteLine(string.Format("Youtube : Page {0} - Total links {1} - Total valid links {2} - Total valid unique links {3}", page.Name, 
                            page.YoutubeUrls.Count, 
                            page.YoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count(),
                            page.AggregatedYoutubeUrls.Where(o => o.IsValid == SourceStatus.Valid).Count()));
                    }
                    else // Display raw list
                    {
                        _console.WriteSection("All video youtube refergences");
                        foreach (var video in page.YoutubeUrls)
                            _console.WriteLineInGray(string.Format("{0} - {1} - {2}", page.Name, video.Name, video.Url));
                        _console.WriteLine(string.Format("Youtube links for page {0}: {1}", page.Name, page.YoutubeUrls.Count));
                    }
                });

                _console.WriteLine(String.Format("Youtube : Total links {0} - Total valid unique links {1} - Total valid links {2}", 
                    _wikiPageCache.WikiPages.SelectMany(o => o.References).Count(),
                    _wikiPageCache.WikiPages.SelectMany(o => o.YoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count(),
                    _wikiPageCache.WikiPages.SelectMany(o => o.AggregatedYoutubeUrls).Where(o => o.IsValid == SourceStatus.Valid).Count()));
            }
        }

        private void SaveJsonToFile()
        {
            if (_config.OutputYoutubeUrlJson)
                _fileHelper.SaveJsonToFile(_wikiPageCache.WikiPages.ToList());
        }
    }
}
