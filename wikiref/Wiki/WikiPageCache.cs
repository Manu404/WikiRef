using System.Collections.Generic;

namespace WikiRef.Wiki
{
    class WikiPageCache
    {
        public IEnumerable<WikiPage> WikiPages { get; private set; }

        public WikiPageCache(MediaWikiApi _api)
        {
            WikiPages = _api.GetWikiPages();
        }
    }

    class JsonWikiPageCache
    {
        public IEnumerable<WikiPage> WikiPages { get; private set; }

        public JsonWikiPageCache(FileHelper helper, AppConfiguration config)
        {
            WikiPages = helper.LoadJsonFromFile(config.Inputjson);
        }
    }
}
