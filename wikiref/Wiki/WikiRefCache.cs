using Newtonsoft.Json;
using System.Collections.Generic;
using WikiRef.Commons;

namespace WikiRef.Wiki
{
    public class WikiRefCache
    {
        [JsonProperty] public IEnumerable<WikiPage> WikiPages { get; private set; }
        [JsonProperty] public IEnumerable<string> WhiteList { get; private set; }

        public WikiRefCache()
        {

        }

        public WikiRefCache(MediaWikiApi _api, WhitelistHandler whitelistHandler)
        {
            WikiPages = _api.GetWikiPages().Result;
            WhiteList = whitelistHandler.WhitelistWebsite;
        }
    }

    //public class JsonWikiPageCache
    //{
    //    [JsonProperty("WikiPages")]
    //    public IEnumerable<WikiPage> WikiPages { get; private set; }
    //    [JsonProperty("Whitelist")]
    //    public IEnumerable<string> WhiteList { get; private set; }

    //    public JsonWikiPageCache(FileHelper helper, AppConfiguration config)
    //    {
    //        WikiPages = ;
    //    }
    //}
}
