using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiRef
{
    class MediaWikiApi
    {
        private ConsoleHelper _console;
        private WhitelistHandler _whitelistHandler;
        private AppConfiguration _config;
        private RegexHelper _regexHelper;
        NetworkHelper _networkHelper;

        public string ServerUrl { get { return _config.WikiUrl; } }

        public MediaWikiApi(ConsoleHelper consoleHelper, AppConfiguration config, WhitelistHandler whitelistHandler, RegexHelper regexHelper, NetworkHelper networkHelper)
        {
            _console = consoleHelper;
            _whitelistHandler = whitelistHandler;
            _config = config;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper;    
        }

        public async Task<IEnumerable<WikiPage>> GetWikiPages()
        {
            List<WikiPage> pages = new List<WikiPage>();

            try {
                // If query page
                if (String.IsNullOrEmpty(_config.Category))
                    return new[] { new WikiPage(_config.Page, _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper) };

                // if query category
                string queryUrl = $"{_config.WikiUrl}/api.php?action=query&list=categorymembers&cmtitle=Category:{_config.Category}&cmlimit=500&format=json";
                string json = await _networkHelper.GetContent(queryUrl); 
                JObject jsonObject = JObject.Parse(json);
                foreach (var page in jsonObject["query"]["categorymembers"])
                    pages.Add(new WikiPage((string)page["title"], _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper));
                return pages;
            }
            catch(Exception ex)
            {
                _console.WriteLineInRed($"Error retreiving pages from {_config.Category}");
                _console.WriteLineInRed(ex.Message);
                return pages;
            }
        }

        public async Task<string> GetPageContent(string pageName)
        {
            try
            {
                if (String.IsNullOrEmpty(pageName)) return String.Empty;

                var sanitizedPageName = pageName.Replace(" ", "_");
                string queryUrl = $"{_config.WikiUrl}/api.php?action=query&prop=revisions&titles={sanitizedPageName}&rvslots=*&rvprop=timestamp|user|comment|content&format=json";
                string json = await _networkHelper.GetContent(queryUrl);

                JObject jsonObject = JObject.Parse(json);
                JToken content = jsonObject.Descendants()
                .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == "*")
                .Select(p => ((JProperty)p).Value)
                .FirstOrDefault();

                return content.ToString();
            }
            catch(Exception ex)
            {
                _console.WriteLineInRed($"Error treating page {pageName}");
                _console.WriteLineInRed(ex.Message);
                return String.Empty;
            }
        }
    }
}
