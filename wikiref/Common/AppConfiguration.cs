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
        public bool LogOutputToFile { get; private set; }
        public bool NoColor { get; private set; }
        public int Throttle { get; private set; }
        public void InitalizeOptions(DefaultOptions options)
        {
            Verbose = options.Verbose;
            Silent = options.Silent;
            LogOutputToFile = options.LogOtuputToFile;
            NoColor = options.NoColor;
            Throttle = options.Throttle;

            if (options is YoutubeOptions)
                InitalizeOptions(options as YoutubeOptions);
            else if (options is ArchiveOptions)
                InitalizeOptions(options as ArchiveOptions);
            else if (options is YoutubeDownloadOption)
                InitalizeOptions(options as YoutubeDownloadOption);
            else if (options is AnalyseOptions)
                InitalizeOptions(options as AnalyseOptions);
        }

        // analyse ver options
        public string WikiUrl { get; private set; }
        public string Category { get; private set; }
        public string Page { get; private set; }
        public bool Report { get; private set; }
        public void InitalizeOptions(AnalyseOptions options)
        {
            Action = Action.Analyse;

            WikiUrl = options.Wiki;
            Page = options.Page;
            Category = options.Category;
        }

        // youtube verb argument
        public string YoutubeListDestinationFile { get; private set; }
        public bool AggrgateYoutubeUrl { get; private set; }
        public string YoutubeAnalysisOutputFilename { get; private set; }
        public bool DisplayYoutubeUrlList { get; private set; }
        public void InitalizeOptions(YoutubeOptions options)
        {
            Action = Action.Youtube;

            WikiUrl = options.Wiki;
            Page = options.Page;
            Category = options.Category;

            AggrgateYoutubeUrl = options.Aggregate;
            YoutubeAnalysisOutputFilename = options.OutputJson;
            DisplayYoutubeUrlList = options.Display;
        }

        // download-youtube verb argument
        public string DownloadArguments { get; private set; }
        public string DownloadToolLocation { get; private set; }
        public string DownloadRootFolder { get; private set; }
        public bool DownloadRedownload { get; private set; }
        public bool DownloadPlaylist { get; private set; }
        public bool DownloadChannel { get; private set; }
        public string DownloadInputJson { get; private set; }
        public string DownloadOutpuScriptName { get; private set; }
        public void InitalizeOptions(YoutubeDownloadOption options)
        {
            Action = Action.Backup;
            DownloadArguments = options.Arguments;
            DownloadToolLocation = options.ToolLocation;
            DownloadRootFolder = options.OutputFolder;
            DownloadRedownload = options.Redownload;
            DownloadInputJson = options.InputJson;
            DownloadPlaylist = options.DownloadPlaylist;
            DownloadChannel = options.DownloadChannel;
            DownloadOutpuScriptName = options.OutputScriptName;

        }

        // archive verb option
        public void InitalizeOptions(ArchiveOptions options)
        {
            Action = Action.Archive;
            InitalizeOptions(options as DefaultOptions);
        }

    }
}
