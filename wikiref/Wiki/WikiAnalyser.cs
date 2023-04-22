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
            _console.WriteLineInGray($"Invalid references: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.Status == SourceStatus.Invalid).Count()}");
            _console.WriteLineInGray($"Undefined references: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.Status == SourceStatus.Undefined).Count()}");
            _console.WriteLineInGray($"Whitelisted references: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.Status == SourceStatus.WhiteListed).Count()}");
            _console.WriteLineInGray($"Reference wihtout metadata: {_wikiPageCache.WikiPages.SelectMany(r => r.References).Where(r => r.FormattingIssue).Count()}");
            _console.WriteLineInGray($"Dates: {_wikiPageCache.WikiPages.Sum(p => p.DatesCount)}");
            _console.WriteLineInGray($"Misformed or missing dates: {_wikiPageCache.WikiPages.Sum(p => p.MalformedDates)}");
            _console.WriteLineInGray($"Wikipedia links: {_wikiPageCache.WikiPages.Sum(p => p.WikiLinks)}");

            _console.WriteSection("Invalid link");
            foreach (var url in _wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Invalid))
                _console.WriteLineInGray(url.Url);

            _console.WriteSection("Undefined link");
            foreach (var url in _wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Undefined))
                _console.WriteLineInGray(url.Url);

            _console.WriteSection("Whitelisted link");
            foreach (var url in _wikiPageCache.WikiPages.SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed))
                _console.WriteLineInGray(url.Url);
        }
    }
}
