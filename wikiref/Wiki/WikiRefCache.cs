using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WikiRef.Common;
using WikiRef.Data;

namespace WikiRef.Wiki
{
    public class WikiRefCache
    {
        [JsonProperty] public IEnumerable<string> WhiteList { get; private set; }
        [JsonProperty] public Data.Wiki Wiki { get; private set; }

        public WikiRefCache()
        {
            Wiki = new Data.Wiki();
            WhiteList = new List<string>();
        }

        public WikiRefCache(IAppConfiguration config, IConsole console, MediaWikiApi api, WhiteListHelper whitelistHandler)
        {
            Wiki = new Data.Wiki();
            WhiteList = whitelistHandler.WhiteList;

            Wiki.Url = config.Url;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            WikiNamespace ns = new WikiNamespace();
            if (!string.IsNullOrEmpty(config.Page) || !string.IsNullOrEmpty(config.Category))
            {
                ns.Name = "Principal";
                ns.Pages = api.GetWikiPagesFromCategories().Result.ToList();
            }
            else if (!string.IsNullOrEmpty(config.Namespace))
            {
                ns.Name = config.Namespace;
                ns.Pages = api.GetWikiPagesFromNamespace().Result.ToList();
            }
            Wiki.Namespaces.Add(ns);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            console.WriteLineInGray(String.Format("Runtime: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
        }
    }
}
