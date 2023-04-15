using System;
using System.Linq;
using System.Threading.Tasks;

namespace WikiRef.Wiki
{
    class WikiAnalyser
    {
        MediaWikiApi _api;
        ConsoleHelper _console;
        WikiPageCache _wikiPageCache;

        public WikiAnalyser(MediaWikiApi api, ConsoleHelper console, WikiPageCache wikiPageCache)
        {
            _api = api;
            _console = console;
            _wikiPageCache = wikiPageCache;
        }

        // Main method for the analyze verb
        public async Task AnalyseReferences()
        {
            if (!string.IsNullOrEmpty(_api.ServerUrl))
            {
                foreach (var page in _wikiPageCache.WikiPages)
                {
                    _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                    await page.CheckReferenceStatus();
                }
            }

            _console.WriteSection("Analysis summary");
            _console.WriteLineInGray($"Total reference: {_wikiPageCache.WikiPages.Sum(p => p.References.Count)}");
            _console.WriteLineInGray($"Total citation refs: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.IsCitation).Count()}");
            _console.WriteLineInGray($"Reference with formating issues: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.FormattingIssue).Count()}");
            _console.WriteLineInGray($"Wikilinks total: {_wikiPageCache.WikiPages.Sum(p => p.WikiLinks)}");
            _console.WriteLineInGray($"Date total: {_wikiPageCache.WikiPages.Sum(p => p.DatesCount)}");
            _console.WriteLineInGray($"Malformed date: {_wikiPageCache.WikiPages.Sum(p => p.MalformedDates)}");
        }
    }
}
