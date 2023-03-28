using System;

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
        public void AnalyseReferences()
        {
            if (!string.IsNullOrEmpty(_api.ServerUrl))
            {
                foreach (var page in _wikiPageCache.WikiPages)
                {
                    _console.WriteSection(string.Format("Analyzing page: {0}...", page.Name));
                    page.CheckPageStatus();
                }
            }
        }
    }
}
