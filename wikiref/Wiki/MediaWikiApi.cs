using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WikiRef.Commons;

namespace WikiRef.Wiki
{
    public class MediaWikiApi
    {
        private int _apiCalls;
        private ConsoleHelper _console;
        private WhitelistHandler _whitelistHandler;
        private AppConfiguration _config;
        private RegexHelper _regexHelper;
        NetworkHelper _networkHelper;

        public MediaWikiApi(ConsoleHelper consoleHelper, AppConfiguration config, WhitelistHandler whitelistHandler, RegexHelper regexHelper, NetworkHelper networkHelper)
        {
            _console = consoleHelper;
            _whitelistHandler = whitelistHandler;
            _config = config;
            _regexHelper = regexHelper;
            _networkHelper = networkHelper;
            _apiCalls = 0;
        }

        public async Task<IEnumerable<WikiPage>> GetWikiPagesFromNamespace(string gap_continue = "")
        {
            List<WikiPage> pages = new List<WikiPage>();

            try
            {
                // If query page
                if (string.IsNullOrEmpty(_config.Namespace))
                    return new[] { new WikiPage(_config.Page, _config.Namespace, _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper) };

                if (await _networkHelper.GetStatus(_config.WikiApi) != HttpStatusCode.OK)
                    _console.WriteLineInRed($"Provided api url seems invalid {_config.WikiApi}");

                // if query namespace, paging
                string queryUrl = $"{_config.WikiApi}?action=query&generator=allpages&gaplimit=500&apnamespace={_config.Namespace}&format=json";
                if (!String.IsNullOrEmpty(gap_continue))
                    queryUrl += $"&gapcontinue={gap_continue}";

                _console.WriteLineInGray($"{_apiCalls}::Retrieve {queryUrl}");

                string json = await _networkHelper.GetContent(queryUrl);
                _apiCalls += 1;
                JObject jsonObject = JObject.Parse(json);

                if (jsonObject["continue"] != null && (jsonObject["continue"]["gapcontinue"] != null))
                    pages.AddRange(await GetWikiPagesFromNamespace(jsonObject["continue"]["gapcontinue"].Value<string>()));

                foreach (var page in jsonObject["query"]["pages"])
                    pages.Add(new WikiPage((string)page.Children().First()["title"], _config.Category, _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper));
               
                return pages;
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"Error retreiving pages from {_config.Category}");
                _console.WriteLineInRed(ex.Message);
                return pages;
            }
        }

        public async Task<IEnumerable<WikiPage>> GetWikiPagesFromCategories(string page_continue = "")
        {
            List<WikiPage> pages = new List<WikiPage>();

            try
            {
                // If query page
                if (string.IsNullOrEmpty(_config.Category))
                    return new[] { new WikiPage(_config.Page, _config.Category, _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper) };

                if (await _networkHelper.GetStatus(_config.WikiApi) != HttpStatusCode.OK)
                    _console.WriteLineInRed($"Provided api url seems invalid {_config.WikiApi}");

                // if query category, paging
                string queryUrl = $"{_config.WikiApi}?action=query&list=categorymembers&cmtitle=Category:{_config.Category}&cmlimit=500&format=json";
                if (!String.IsNullOrEmpty(page_continue))
                    queryUrl += $"&cmcontinue={page_continue}";

                _console.WriteLineInGray($"{_apiCalls}::Retrieve {queryUrl}");

                string json = await _networkHelper.GetContent(queryUrl);
                _apiCalls += 1;
                JObject jsonObject = JObject.Parse(json);

                if (jsonObject["continue"] != null && jsonObject["continue"]["cmcontinue"] != null)
                    pages.AddRange(await GetWikiPagesFromCategories(jsonObject["continue"]["cmcontinue"].Value<string>()));

                Parallel.ForEach(jsonObject["query"]["categorymembers"], page =>
                {
                    pages.Add(new WikiPage((string)page["title"], _config.Category, _console, this, _config, _whitelistHandler, _regexHelper, _networkHelper));
                });
                
                return pages;
            }
            catch (Exception ex)
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
                if (string.IsNullOrEmpty(pageName)) return string.Empty;

                var sanitizedPageName = pageName.Replace(" ", "_");
                string queryUrl = $"{_config.WikiApi}?action=query&prop=revisions&titles={sanitizedPageName}&rvslots=*&rvprop=timestamp|user|comment|content&format=json";

                _console.WriteLineInGray($"{_apiCalls}::Retrieve {queryUrl} => {pageName}");

                string json = await _networkHelper.GetContent(queryUrl);
                _apiCalls += 1;

                JObject jsonObject = JObject.Parse(json);
                JToken content = jsonObject.Descendants()
                                            .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == "*")
                                            .Select(p => ((JProperty)p).Value)
                                            .FirstOrDefault();

                return content.ToString();
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"Error treating page {pageName}");
                _console.WriteLineInRed(ex.Message);
                return string.Empty;
            }
        }
    }
}
