using System;
using System.Collections.Generic;
using System.Linq;

namespace WikiRef.Commons
{
    public class WhitelistHandler
    {
        // some website avoid crawling pages, those are blackliste to avoid false positive
        private List<string> WhitelistWebsite;

        public WhitelistHandler(WikiRef.AppConfiguration config, WikiRef.FileHelper fileHelper)
        {
            WhitelistWebsite = String.IsNullOrEmpty(config.WhiteList) ? new List<string>() : fileHelper.LoadWhitelistFromJsonFile(config.WhiteList);
        }

        public bool CheckIfWebsiteIsWhitelisted(string url)
        {
            return WhitelistWebsite.Any(u => url.ToLower().StartsWith(u.ToLower(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
