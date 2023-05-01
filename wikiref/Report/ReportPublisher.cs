using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Sites;
using WikiRef.Commons;
using WikiRef.Wiki;

namespace WikiRef.Report
{
    internal class ReportPublisher
    {
        AppConfiguration _config;
        ReportBuilder _builder;
        ConsoleHelper _consoleHelper;

        public ReportPublisher(AppConfiguration config, ReportBuilder builder, ConsoleHelper consoleHelper)
        {
            _config = config;
            _builder = builder; 
            _consoleHelper = consoleHelper;
        }

        public async Task Publish()
        {
            try
            {
                var pageName = _config.ReportPage;
                var wikiClient = new WikiClient();
                var site = new WikiSite(wikiClient, _config.WikiApi);
                string message = $"Publication rapport wikiref du {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
            try
            {

                    _consoleHelper.WriteLineInGray("Initilize");
                    await site.Initialization;

                    _consoleHelper.WriteLineInGray("Connect");
                    await site.LoginAsync(_config.User, _config.Password);
                    _consoleHelper.WriteLineInGray("Connected !");

                    _consoleHelper.WriteSection($"Treating {pageName}");

                    _consoleHelper.WriteLineInGray("Fetch page");
                    var page = new WikiClientLibrary.Pages.WikiPage(site, pageName);
                    await page.RefreshAsync(PageQueryOptions.FetchContent);
                    _consoleHelper.WriteLineInGray(message);

                    _consoleHelper.WriteLineInGray("Build Report");
                    page.Content = _builder.BuildReport();

                    _consoleHelper.WriteLineInGray("Purge cache");
                    await page.PurgeAsync(PagePurgeOptions.ForceRecursiveLinkUpdate);

                    _consoleHelper.WriteLineInGray("Upload");
                    await page.UpdateContentAsync(message);
                }
                catch (Exception ex)
                {
                    _consoleHelper.WriteLineInRed($"Error with MediaWiki API : {ex.Message}");
                }
                try
                {
                    _consoleHelper.WriteLineInGray("Disconnect");
                    await site.LogoutAsync();
                }
                catch (Exception ex)
                {
                    _consoleHelper.WriteLineInRed($"Error with MediaWiki API : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _consoleHelper.WriteLineInRed($"Error building report : {ex.Message}");
            }
        }
    }
}
