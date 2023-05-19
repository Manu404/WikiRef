using System;
using System.Collections.Generic;
using System.Linq;


namespace WikiRef.Commons
{
    public class WhitelistHandler
    {
        // some website avoid crawling pages, those are blackliste to avoid false positive
        public List<string> WhitelistWebsite { get; private set; }

        public WhitelistHandler(WikiRef.AppConfiguration config, WikiRef.FileHelper fileHelper)
        {
            WhitelistWebsite = String.IsNullOrEmpty(config.WhiteList) ? new List<string>() : fileHelper.LoadWhitelistFromJsonFile(config.WhiteList);
        }

        public bool CheckIfWebsiteIsWhitelisted(string url)
        {
            return WhitelistWebsite.Any(u => url.ToLower().StartsWith("https://" + u.ToLower(), StringComparison.InvariantCultureIgnoreCase))
            || WhitelistWebsite.Any(u => url.ToLower().StartsWith("http://" + u.ToLower(), StringComparison.InvariantCultureIgnoreCase))
            || WhitelistWebsite.Any(u => url.ToLower().StartsWith(u.ToLower(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
