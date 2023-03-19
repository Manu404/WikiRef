namespace WikiRef
{
    class AppConfiguration
    {
        public bool Verbose { get; private set; } // DefaultOptions=>Verbose
        public string WikiUrl { get; private set; } // DefaultOptions=>Wiki
        public string Category { get; private set; } // DefaultOptions=>Category
        public string Page { get; private set; } // DefaultOptions=>Page
        public bool Silent { get; private set; } // DefaultOptions=>Silent
        public bool ConsoleOutputToFile { get; private set; } // DefaultOptions=>ConsoleOutputToFile
        public bool NoColor { get; private set; } // DefaultOption=>NoColor
        public int Throttle { get; private set; } // Default=>Throttle


        public string YoutubeListDestinationFile { get; private set; } // YoutubeOptions=>Output
        public bool AggrgateYoutubeUrl { get; private set; } // YoutubeOptions=>Aggregate
        public bool OutputYoutubeUrlJson { get; private set; } // YoutubeOptions=>Json
        public bool DisplayYoutubeUrlList { get; private set; } // YoutubeOptions=>display

        public bool DownloadYoutubeVideos { get; private set; } // Backup=>download
        public string DownloadArguments { get; private set; } // Backup=>Arguments
        public string DownloadToolLocation { get; private set; } // Backup=>ToolLocation
        public string DownloadRootFolder { get; private set; } // Backup=>RootFolder
        public bool DownloadRedownload { get; private set; } // Backup=>Redownload

        public Action Action { get; set; }

        public AppConfiguration(DefaultOptions options, Action action)
        {
            Verbose = false;
            Action = Action.Undefined;
            InitalizeOptions(options, action);
        }

        private void InitalizeOptions(DefaultOptions options, Action action)
        {
            Verbose = options.Verbose;
            WikiUrl = options.Wiki;
            Page = options.Page;
            Category = options.Category;
            Action = action;
            Silent = options.Silent;
            ConsoleOutputToFile = options.ConsoleOutputToFile;
            NoColor = options.NoColor;
            Throttle = options.Throttle;

            if (options is YoutubeOptions)
                InitalizeOptions((YoutubeOptions)options);
            else if (options is ArchiveOptions)
                InitalizeOptions((ArchiveOptions)options);
            else if (options is BackuptOptions)
                InitalizeOptions((BackuptOptions)options);
        }

        private void InitalizeOptions(YoutubeOptions options)
        {
            AggrgateYoutubeUrl = options.Aggregate;
            OutputYoutubeUrlJson = options.OutputJson;
            DisplayYoutubeUrlList = options.Display;
        }

        private void InitalizeOptions(ArchiveOptions options)
        {
        }

        private void InitalizeOptions(BackuptOptions options)
        {
            DownloadYoutubeVideos = options.Download;
            DownloadArguments = options.Arguments;
            DownloadToolLocation = options.ToolLocation;
            DownloadRootFolder = options.RootFolder;
            DownloadRedownload = options.Redownload;
        }

    }
}
