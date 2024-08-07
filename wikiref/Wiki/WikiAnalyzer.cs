﻿using System.Linq;
using System.Threading.Tasks;
using WikiRef.Common;
using WikiRef.Data;

namespace WikiRef.Wiki
{
    class WikiAnalyzer
    {
        IConsole _console;
        WikiRefCache _wikiRefCache;
        IAppConfiguration _config;

        public WikiAnalyzer(IAppConfiguration config, IConsole console, WikiRefCache wikiPageCache)
        {
            _console = console;
            _wikiRefCache = wikiPageCache;
            _config = config;   
        }

        // Main method for the analyze verb
        public async Task AnalyzeReferences()
        {
            foreach(var page in _wikiRefCache.Wiki.Namespaces.SelectMany(ns => ns.Pages))
            {
                _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                await (page as WikiPage).CheckReferenceStatus();
            }

            _console.WriteSection("Analysis summary");
            _console.WriteLineInGray($"References: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.References.Count)}");
            _console.WriteLineInGray($"Citation references: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.IsCitation).Count()}");
            _console.WriteLineInGray($"Urls: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).SelectMany(u => u.Urls).Count()}");
            _console.WriteLineInGray($"Invalid references: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.Status == SourceStatus.Invalid).Count()}");
            _console.WriteLineInGray($"Undefined references: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.Status == SourceStatus.Undefined).Count()}");
            _console.WriteLineInGray($"Whitelisted references: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.Status == SourceStatus.WhiteListed).Count()}");
            _console.WriteLineInGray($"Reference wihtout metadata: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(r => r.References).Where(r => r.FormattingIssue).Count()}");
            _console.WriteLineInGray($"Dates: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.DatesCount)}");
            _console.WriteLineInGray($"Misformed or missing dates: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.MalformedDates)}");
            _console.WriteLineInGray($"Wikipedia links: {_wikiRefCache.Wiki.Namespaces.SelectMany(p => p.Pages).Sum(p => p.WikiLinks)}");

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
