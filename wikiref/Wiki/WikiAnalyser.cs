using CommandLine;
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
        WikiRefCache _wikiRefCache;
        AppConfiguration _config;

        public WikiAnalyser(MediaWikiApi api, ConsoleHelper console, WikiRefCache wikiPageCache, AppConfiguration config)
        {
            _api = api;
            _console = console;
            _wikiRefCache = wikiPageCache;
            _config = config;   
        }

        // Main method for the analyze verb
        public async Task AnalyseReferences()
        {
            foreach(var ns in _wikiRefCache.Wiki.Namespaces)
                foreach (var page in ns.Pages.OrderBy(p => p.Name))
                {
                    _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                    await page.CheckReferenceStatus();
                }

            _console.WriteSection("Analysis summary");
            _console.WriteLineInGray($"References: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.References.Count)}");
            _console.WriteLineInGray($"Citation references: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.IsCitation).Count()}");
            _console.WriteLineInGray($"Urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Count()}");
            _console.WriteLineInGray($"Invalid urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Invalid).Count()}");
            _console.WriteLineInGray($"Undefined urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Undefined).Count()}");
            _console.WriteLineInGray($"Whitelisted urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).Count()}");
            _console.WriteLineInGray($"Reference wihtout metadata: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.FormattingIssue).Count()}");
            _console.WriteLineInGray($"Misformed or missing dates: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.MalformedDates)}");
            _console.WriteLineInGray($"Wikipedia urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(s => s.WikiLinks)}");

            _console.WriteSection("Invalid link");
            foreach (var url in _wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Invalid).OrderBy(u => u.Url))
                _console.WriteLineInGray(url.Url);

            if (_config.Verbose)
            {
                _console.WriteSection("Undefined link");
                foreach (var url in _wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.Undefined).OrderBy(u => u.Url))
                    _console.WriteLineInGray(url.Url);

                _console.WriteSection("Whitelisted link");
                foreach (var url in _wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).OrderBy(u => u.Url))
                    _console.WriteLineInGray(url.Url);
            }
        }
    }
}
