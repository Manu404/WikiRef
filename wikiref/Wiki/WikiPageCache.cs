using System.Collections.Generic;
using WikiRef.Commons;

namespace WikiRef.Wiki
{
    public class WikiPageCache
    {
        public IEnumerable<WikiPage> WikiPages { get; private set; }

        public WikiPageCache(MediaWikiApi _api)
        {
            WikiPages = _api.GetWikiPages().Result;
        }
    }

    public class JsonWikiPageCache
    {
        public IEnumerable<WikiPage> WikiPages { get; private set; }

        public JsonWikiPageCache(FileHelper helper, AppConfiguration config)
        {
            WikiPages = helper.LoadWikiPagesFromJsonFile(config.Inputjson);
        }
    }
}
