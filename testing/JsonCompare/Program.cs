using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WikiRef.Commons;
using WikiRef.Report;
using WikiRef.Wiki;
using static System.Net.WebRequestMethods;

namespace WikiRef
{
    public class JsonComparisonHelper
    {
        WikiRefCache _filea;
        WikiRefCache _fileb;
        bool isSimilar = true;

        public JsonComparisonHelper(WikiRefCache filea, WikiRefCache fileb)
        {
            _filea = filea;
            _fileb = fileb;

        }
        public bool IsSimilar()
        {
            isSimilar = CompareWhiteList(_filea, _fileb);
            isSimilar = CompareWhiteList(_fileb, _filea);

            isSimilar = CompareWikiPages(_filea, _fileb);
            isSimilar = CompareWikiPages(_fileb, _filea);

            Console.WriteLine($"File are similar : {isSimilar}");

            return isSimilar;
        }

        private bool CompareWikiPages(WikiRefCache filea, WikiRefCache fileb)
        {
            bool equal = true;
            int count = filea.WikiPages.Count();
            if (count != fileb.WikiPages.Count()) return false;

            foreach(var pageA in filea.WikiPages)
            {
                var pageB = fileb.WikiPages.FirstOrDefault(f => f.Content == pageA.Content);
                if (pageB == null) return false;
                
                equal = equal && (pageB.DatesCount == pageA.DatesCount);
                equal = equal && (pageA.MalformedDates == pageB.MalformedDates);
                equal = equal && (pageA.WikiLinks == pageB.WikiLinks);  
                equal = equal && (pageA.Name == pageB.Name);

                if (pageA.References.Count != pageB.References.Count) return false;

                int refCount = pageA.References.Count();
                foreach(var refA in pageA.References)
                {
                    var refB = pageB.References.FirstOrDefault(r => r.Content == refA.Content);
                    if (refB == null) return false;
                    equal = equal && refA.IsCitation == refB.IsCitation;
                    equal = equal && refA.Status == refB.Status;
                    equal = equal && refA.FormattingIssue == refB.FormattingIssue;
                    equal = equal && refA.InvalidUrls == refB.InvalidUrls;
                    refCount -= 1;
                }
                equal = equal && refCount == 0;


                if (pageA.YoutubeUrls.Count != pageB.YoutubeUrls.Count) return false;

                int youtubeUrlsCount = pageA.YoutubeUrls.Count(); 
                foreach (var ytbA in pageA.YoutubeUrls)
                {
                    var ytbB = pageB.YoutubeUrls.FirstOrDefault(r => r.Urls.Any(u => ytbA.Urls.Contains(u)));
                    if (ytbB == null) return false;
                    if (ytbB.Urls.Count != ytbB.Urls.Count) return
                    equal = equal && ytbA.VideoId == ytbB.VideoId;
                    equal = equal && ytbA.Name == ytbB.Name;
                    equal = equal && ytbA.ChannelName == ytbB.ChannelName;
                    equal = equal && ytbA.IsValid == ytbB.IsValid;
                    equal = equal && ytbA.IsPlaylist == ytbB.IsPlaylist;
                    equal = equal && ytbA.IsUser == ytbB.IsUser;
                    equal = equal && ytbA.IsCommunity == ytbB.IsCommunity;
                    equal = equal && ytbA.IsAbout == ytbB.IsAbout;
                    equal = equal && ytbA.IsChannels == ytbB.IsChannels;
                    equal = equal && ytbA.IsHome == ytbB.IsHome;
                    equal = equal && ytbA.IsVideo == ytbB.IsVideo;
                    youtubeUrlsCount -= 1;
                }
                equal = equal && youtubeUrlsCount == 0;

                count -= 1;
            }
            return count == 0 && equal;
        }

        private bool CompareWhiteList(WikiRefCache filea, WikiRefCache fileb)
        {
            int count = filea.WhiteList.Count();
            if (count != fileb.WhiteList.Count()) return false;
            foreach(var whitelist in filea.WhiteList)            
                if(fileb.WhiteList.Contains(whitelist)) count -= 1;
            return count == 0;
        }
    }
    class Program
    {
        BootStrapper _bootStrapper;
        FileHelper _fileHelper;
        AppConfiguration _appConfiguration;
        JsonComparisonHelper _jsonComparisonHelper;
        static int returnValue = 0;

        private void ParseCommandlineArgument(string[] args)
        {
            Parser.Default.ParseArguments<JsonCompareOption>(args)
                .WithParsed<JsonCompareOption>(option =>
                {
                    InitializeDependencies(option);
                    returnValue = _jsonComparisonHelper.IsSimilar() ? 0 : 1;
                });
        }

        private void InitializeDependencies(JsonCompareOption options)
        {
            (_bootStrapper = new BootStrapper()).InitializeDependencies(options);
            _fileHelper = new FileHelper(_bootStrapper.ConsoleHelper);
            string file_a = options.FileA.Replace("/cygdrive/c", "");
            string file_b = options.FileB.Replace("/cygdrive/c", "");
            _jsonComparisonHelper = new JsonComparisonHelper(_fileHelper.LoadWikiRefCacheFromJsonFile(file_a), _fileHelper.LoadWikiRefCacheFromJsonFile(file_b));
        }

        static int Main(string[] args)
        {
            var p = new Program();
            p.ParseCommandlineArgument(args);
            return returnValue;
        }
    }
}
