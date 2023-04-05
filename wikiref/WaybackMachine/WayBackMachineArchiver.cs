using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WikiRef.Wiki;

namespace WikiRef
{
    internal class WayBackMachineArchiver
    {
        private ConsoleHelper _console;
        private AppConfiguration _config;
        private JsonWikiPageCache _wikiPageCache;
        private NetworkHelper _networkHelper;

        public WayBackMachineArchiver(AppConfiguration config, ConsoleHelper console, NetworkHelper networkHelper, JsonWikiPageCache wikiPageCache)
        {
            _config = config;
            _console = console;
            _wikiPageCache = wikiPageCache;
            _networkHelper = networkHelper;

        }

        public async Task Archive()
        {
            foreach (var page in _wikiPageCache.WikiPages)
            {
                await Parallel.ForEachAsync(page.References.Where(r => !r.IsCitation), async (reference, token) =>
                {
                    foreach (var url in reference.Urls.Where(url => !IsYoutubeUrl(url)))
                        await AnalyseUrl(url);
                });
                await Parallel.ForEachAsync(page.YoutubeUrls, async (video, token) =>
                {
                    if (video.IsVideo)
                        await AnalyseUrl(video.VideoUrl);
                    else
                        foreach (var url in video.Urls)
                            await AnalyseUrl(url);
                });
            }
        }

        private async Task AnalyseUrl(string url)
        {
            try
            {
                var snapshot = await Getsnapshot(url);
                if (!snapshot.IsArchived)
                {
                    _console.WriteLine($"Website {url} not archived.");
                    await _networkHelper.GetContent($"https://web.archive.org/save/{url}");
                    _console.WriteLineInOrange($"Archival requested for {url}");

                    if(_config.WaitForArchiving)
                    {
                        while (!snapshot.IsArchived)
                        {
                            _console.WriteLineInGray($"Waiting confirmation for {url}");
                            await Task.Delay(5000);
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

        private async Task<WayBakckMachineSnapshot> Getsnapshot(string url)
        {
            string json = await _networkHelper.GetContent($"https://archive.org/wayback/available?url={url}");
            return new WayBakckMachineSnapshot(json);
        }

        private bool IsYoutubeUrl(string url)
        {
            return (url.Contains("youtu.", StringComparison.InvariantCultureIgnoreCase) || url.Contains("youtube.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
