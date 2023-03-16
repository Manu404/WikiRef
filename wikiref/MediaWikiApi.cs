﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace WikiRef
{
    class MediaWikiApi
    {
        private ConsoleHelper _consoleHelper;
        private WhitelistHandler _whitelistHandler;
        private GlobalConfiguration _config;

        public string ServerUrl { get; private set; }

        public MediaWikiApi(string serverUrl, ConsoleHelper consoleHelper, GlobalConfiguration config, WhitelistHandler whitelistHandler)
        {
            ServerUrl = serverUrl;
            _consoleHelper = consoleHelper;
            _whitelistHandler = whitelistHandler;
            _config = config;
        }

        public IEnumerable<WikiPage> GetWikiPageInGivenCategory(string categoryName)
        {
            List<WikiPage> pages = new List<WikiPage>();
            try { 
                string queryUrl = $"{ServerUrl}/w/api.php?action=query&list=categorymembers&cmtitle=Category:{categoryName}&cmlimit=500&format=json";
                string json = new WebClient().DownloadString(queryUrl);
                JObject jsonObject = JObject.Parse(json);
                foreach (var page in jsonObject["query"]["categorymembers"])
                    pages.Add(new WikiPage((string)page["title"], _consoleHelper, this, _config, _whitelistHandler));
                return pages;
            }
            catch(Exception ex)
            {
                _consoleHelper.WriteLineInRed(String.Format("Error retreiving pages from {0}", categoryName));
                _consoleHelper.WriteLineInRed(ex.Message);
                return pages;
            }
        }

        public string GetPageContent(string pageName)
        {
            try
            {
                var sanitizedPageName = pageName.Replace(" ", "_");
                string queryUrl = $"{ServerUrl}/w/api.php?action=query&prop=revisions&titles={sanitizedPageName}&rvslots=*&rvprop=timestamp|user|comment|content&format=json";
                string json = new WebClient().DownloadString(queryUrl);

                JObject jsonObject = JObject.Parse(json);
                JToken content = jsonObject.Descendants()
                .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == "*")
                .Select(p => ((JProperty)p).Value)
                .FirstOrDefault();

                return content.ToString();
            }
            catch(Exception ex)
            {
                _consoleHelper.WriteLineInRed(String.Format("Error treating page {0}", pageName));
                _consoleHelper.WriteLineInRed(ex.Message);
                return String.Empty;
            }
        }
    }
}
