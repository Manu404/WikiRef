using System;
using System.Threading.Tasks;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Sites;
using WikiRef.Common;

namespace WikiRef.Report
{
    internal class ReportPublisher
    {
        IAppConfiguration _config;
        IReportBuilder _builder;
        IConsole _console;

        public ReportPublisher(IAppConfiguration config, IConsole IConsole, IReportBuilder builder)
        {
            _config = config;
            _builder = builder; 
            _console = IConsole;
        }

        public async Task Publish()
        {
            try
            {
                var pageName = _config.ReportPage;
                var wikiClient = new WikiClient();
                var site = new WikiSite(wikiClient, _config.Url);
                string message = $"Publication date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
                try
                {

                    _console.WriteLineInGray("Initialize");
                    await site.Initialization;

                    _console.WriteLineInGray("Connect");
                    await site.LoginAsync(_config.User, _config.Password);
                    _console.WriteLineInGray("Connected !");

                    _console.WriteSection($"Treating {pageName}");

                    _console.WriteLineInGray("Fetch page");
                    var page = new WikiPage(site, pageName);
                    await page.RefreshAsync(PageQueryOptions.FetchContent);
                    _console.WriteLineInGray(message);

                    _console.WriteLineInGray("Build Report");
                    page.Content = _builder.BuildReport();

                    _console.WriteLineInGray("Purge cache");
                    await page.PurgeAsync(PagePurgeOptions.ForceRecursiveLinkUpdate);

                    _console.WriteLineInGray("Upload");
                    await page.UpdateContentAsync(message);
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"Error with MediaWiki API : {ex.Message}");
                }

                try
                {
                    _console.WriteLineInGray("Disconnect");
                    await site.LogoutAsync();
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"Error with MediaWiki API : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"Error building report : {ex.Message}");
            }
        }
    }
}
