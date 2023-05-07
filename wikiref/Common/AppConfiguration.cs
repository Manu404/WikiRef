using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;
using System.Runtime;
using WikiRef.Commons;
using WikiRef.Commons.Data;
using System.Security.Cryptography.X509Certificates;

namespace WikiRef
{
    public class AppConfiguration : Commons.AppConfiguration
    {
        public Action Action { get; set; }

        public AppConfiguration(DefaultOptions options) : base(options)
        {
            Action = Action.Undefined;
            InitalizeOptions(options);
        }

        public void InitalizeOptions(DefaultOptions options)
        {
            base.InitalizeOptions(options);

            if (options is ArchiveOptions)
                InitalizeOptions(options as ArchiveOptions);
            else if (options is YoutubeDownloadOption)
                InitalizeOptions(options as YoutubeDownloadOption);
            else if (options is AnalyseOptions)
                InitalizeOptions(options as AnalyseOptions);
            else if (options is PublishOptions)
                InitalizeOptions(options as PublishOptions);
        }

        // analyse verb options
        public string WikiApi { get; private set; }
        public string Namespace { get; set; }
        public string Category { get; private set; }
        public string Page { get; private set; }
        public string OutputJsonToFile { get; private set; }
        public bool OutputJsonToDefaultFile { get; private set; }
        public string WhiteList { get; private set; }

        public void InitalizeOptions(AnalyseOptions options)
        {
            Action = Action.Analyse;

            WikiApi = options.Wiki;
            Namespace = options.Namespace;
            Page = options.Page;
            Category = options.Category;
            WhiteList = options.WhiteList;

            OutputJsonToFile = options.OutputJsonToFile;
            OutputJsonToDefaultFile = options.OutputJsonToDefaultFile;
        }

        // shared option
        public string Inputjson { get; private set; }

        // download-youtube verb argument
        public string DownloadToolArguments { get; private set; }
        public string DownloadToolLocation { get; private set; }
        public string DownloadRootFolder { get; private set; }
        public bool DownloadRedownload { get; private set; }
        public bool DownloadPlaylist { get; private set; }
        public bool DownloadChannel { get; private set; }
        public string DownloadOutpuScriptName { get; private set; }
        public string DownloadVideoFileExtension { get; private set; }
        public void InitalizeOptions(YoutubeDownloadOption options)
        {
            Action = Action.Backup;
            DownloadToolArguments = options.Arguments;
            DownloadToolLocation = options.ToolLocation;
            DownloadRootFolder = options.OutputFolder;
            DownloadRedownload = options.Redownload;
            DownloadPlaylist = options.DownloadPlaylist;
            DownloadChannel = options.DownloadChannel;
            DownloadOutpuScriptName = options.OutputScriptName;
            DownloadVideoFileExtension = options.Extension;
            Inputjson = options.InputJson;
        }

        // archive verb option
        public bool WaitForArchiving { get; private set; }
        public void InitalizeOptions(ArchiveOptions options)
        {
            Action = Action.Archive;
            Inputjson = options.InputJson;
            WaitForArchiving = options.Wait;
        }

        public string User { get; set; }
        public string Password { get; set; }
        public string ReportPage { get; set; }
        public void InitalizeOptions(PublishOptions options)
        {
            Action = Action.Archive;
            Inputjson = options.InputJson;
            WikiApi = options.WikiApi;
            User = options.User;
            Password = options.Password;
            ReportPage = options.ReportPage;

        }
    }
}
