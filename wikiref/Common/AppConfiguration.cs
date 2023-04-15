using System.Runtime;

namespace WikiRef
{
    class AppConfiguration
    {
        public Action Action { get; set; }

        public AppConfiguration(DefaultOptions options)
        {
            Action = Action.Undefined;
            InitalizeOptions(options);
        }

        // default options
        public bool Silent { get; private set; }
        public bool Verbose { get; private set; }
        public bool NoColor { get; private set; }
        public int Throttle { get; private set; }
        public bool Ipv4Only { get; private set; }
        public bool PutInSubDirectory { get; private set; } 
        public bool ExportRefText { get; private set; } 

        public bool ConsoleOutputToDefaultFile { get; private set; }
        public bool ConsoleOutputToDefaultHtmlFile { get; private set; }
        public string ConsoleOutputToFile { get; private set; }
        public string ConsoleOutputToHtmlFile { get; private set; }

        public void InitalizeOptions(DefaultOptions options)
        {
            Verbose = options.Verbose;
            Silent = options.Silent;
            NoColor = options.NoColor;
            Throttle = options.Throttle;
            Ipv4Only = options.IpV4Only;
            PutInSubDirectory = options.PutInSubDirectory;
            ExportRefText = options.ExportRefText;

            ConsoleOutputToDefaultFile = options.ConsoleOutputToDefaultFile;
            ConsoleOutputToDefaultHtmlFile = options.ConsoleToDefaultHtmlFile;
            ConsoleOutputToFile = options.ConsoleOutputToFile;
            ConsoleOutputToHtmlFile = options.ConsoleToHtmlFile;

            if (options is ArchiveOptions)
                InitalizeOptions(options as ArchiveOptions);
            else if (options is YoutubeDownloadOption)
                InitalizeOptions(options as YoutubeDownloadOption);
            else if (options is AnalyseOptions)
                InitalizeOptions(options as AnalyseOptions);
        }

        // analyse verb options
        public string WikiUrl { get; private set; }
        public string Category { get; private set; }
        public string Page { get; private set; }
        public string OutputJsonToFile { get; private set; }
        public bool OutputJsonToDefaultFile { get; private set; }
        public void InitalizeOptions(AnalyseOptions options)
        {
            Action = Action.Analyse;

            WikiUrl = options.Wiki;
            Page = options.Page;
            Category = options.Category;

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
    }
}
