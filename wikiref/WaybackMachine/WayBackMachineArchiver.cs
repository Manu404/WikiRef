using System;
using System.Linq;
using System.Threading.Tasks;
using WikiRef.Common;
using WikiRef.Wiki;

namespace WikiRef
{
    public class WayBackMachineArchiver
    {
        private IConsole _console;
        private IAppConfiguration _config;
        private INetworkHelper _networkHelper;
        private WikiRefCache _wikiRefCache;

        public WayBackMachineArchiver(IAppConfiguration config, IConsole console, INetworkHelper networkHelper, WikiRefCache wikiPageCache)
        {
            _config = config;
            _console = console;
            _wikiRefCache = wikiPageCache;
            _networkHelper = networkHelper;

        }

        public async Task Archive()
        {
            foreach(var page in _wikiRefCache.Wiki.Namespaces.SelectMany(ns => ns.Pages))
            {
                await Parallel.ForEachAsync(page.References.Where(r => !r.IsCitation), async (reference, token) =>
                {
                    foreach (var url in reference.Urls.Where(url => !IsYoutubeUrl(url.Url) && !IsWaybackMachine(url.Url)))
                    { 
                        await Task.Delay(_config.Throttle);
                        await AnalyzeUrl(url.Url);
                    }
                });
            }
        }

        private async Task AnalyzeUrl(string url)
        {
            try
            {
                WayBakckMachineSnapshot snapshot;
                try
                {
                    if(_config.Throttle != 0)
                        await Task.Delay(_config.Throttle * 1000);
                    snapshot = await Getsnapshot(url);
                }
                catch (Exception ex)
                {
                    _console.WriteLineInRed($"Error archiving {url} - {ex.Message}");
                    return;
                }

                if (!snapshot.IsArchived)
                {
                    _console.WriteLine($"Website {url} not archived.");

                    try
                    {
                        await _networkHelper.GetContent($"https://web.archive.org/save/{url}");
                    }
                    catch (Exception ex)
                    {
                        _console.WriteLineInRed($"Error archiving {url} - {ex.Message}");
                    }
                    _console.WriteLineInOrange($"Archival requested for {url}");

                    if(_config.WaitForArchiving)
                    {
                        while (!snapshot.IsArchived)
                        {
                            _console.WriteLineInGray($"Waiting confirmation for {url}");
                            await Task.Delay(10000);
                            snapshot = await Getsnapshot(url);
                        }
                        _console.WriteLineInGreen($"Archival confirmed for {url}");
                    }
                }
                else
                {
                    _console.WriteLineInGreen($"Website {url} is already archived, snapshot dates from {snapshot.Timestamp.ToString("f")}");
                }
            }
            catch(Exception ex)
            {
                _console.WriteLineInRed($"Error archiving {url} - {ex.Message}");
            }
        }

        public async Task<WayBakckMachineSnapshot> Getsnapshot(string url)
        {
            string json = await _networkHelper.GetContent($"https://archive.org/wayback/available?url={url}");
            return new WayBakckMachineSnapshot(json);
        }

        private bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }

        private bool IsWaybackMachine(string url)
        {
            return url.Contains("archive.org", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
