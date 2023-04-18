using System;
using System.Linq;
using System.Threading.Tasks;
using WikiRef.Commons;
using WikiRef.Commons.Data;

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
            foreach (var page in _wikiPageCache.WikiPages)
            {
                _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                await page.CheckReferenceStatus();
            }

            _console.WriteSection("Analysis summary");
            _console.WriteLineInGray($"References: {_wikiPageCache.WikiPages.Sum(p => p.References.Count)}");
            _console.WriteLineInGray($"Citation references: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.IsCitation).Count()}");
            _console.WriteLineInGray($"Urls: {_wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Count()}");
            _console.WriteLineInGray($"Invalid or undefined references: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.Status == SourceStatus.Invalid).Count()}");
            _console.WriteLineInGray($"Invalid or undefined urls: {_wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => !u.IsValid).Count()}");
            _console.WriteLineInGray($"Reference wihtout metadata: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.FormattingIssue).Count()}");
            _console.WriteLineInGray($"Dates: {_wikiPageCache.WikiPages.Sum(p => p.DatesCount)}");
            _console.WriteLineInGray($"Misformed or missing dates: {_wikiPageCache.WikiPages.Sum(p => p.MalformedDates)}");
            _console.WriteLineInGray($"Wikipedia links: {_wikiPageCache.WikiPages.Sum(p => p.WikiLinks)}");

            _console.WriteSection("Faulty link");
            foreach (var url in _wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => !u.IsValid))
                _console.WriteLineInGray(url.Url);
        }
    }
}
