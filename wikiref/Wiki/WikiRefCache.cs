using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using WikiRef.Commons;
using WikiRef.Commons.Data;

namespace WikiRef.Wiki
{
    public class WikiRefCache
    {
        [JsonProperty] public IEnumerable<string> WhiteList { get; private set; }
        [JsonProperty] public Wiki Wiki { get; private set; }

        public WikiRefCache()
        {
            Wiki = new Wiki();
        }

        public WikiRefCache(AppConfiguration _config, MediaWikiApi _api, WhitelistHandler whitelistHandler)
        {
            Wiki = new Wiki();

            Wiki.URL = _config.WikiApi;
            if(!string.IsNullOrEmpty(_config.Page))
            {
                WikiNamespace ns = new WikiNamespace();
                ns.Name = "Principal";
                ns.Pages = _api.GetWikiPagesFromCategories().Result.ToList();
                Wiki.Namespaces.Add(ns);
            }
            else if (!string.IsNullOrEmpty(_config.Category))
            {
                WikiNamespace ns = new WikiNamespace();
                ns.Name = "Principal";
                ns.Pages = _api.GetWikiPagesFromCategories().Result.ToList();
                Wiki.Namespaces.Add(ns);
            }
            else if (!string.IsNullOrEmpty(_config.Namespace))
            {
                WikiNamespace ns = new WikiNamespace();
                ns.Name = _config.Namespace;
                ns.Pages = _api.GetWikiPagesFromNamespace().Result.ToList();
                Wiki.Namespaces.Add(ns);
            }
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
