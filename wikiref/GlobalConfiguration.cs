namespace WikiRef
{
    class GlobalConfiguration
    {
        public bool Verbose { get; set; } // DefaultOptions=>Verbose
        public string WikiUrl { get; set; } // DefaultOptions=>Wiki
        public string Category { get; set; } // DefaultOptions=>Category
        public string Page { get; set; } // DefaultOptions=>Page
        public bool Silent { get; set; } // DefaultOptions=>Silent
        public bool ConsoleOutputToFile { get; set; } // DefaultOptions=>ConsoleOutputToFile
        public bool NoColor { get; set; } // DefaultOption=>NoColor

        public string YoutubeListDestinationFile { get; set; } // YoutubeOptions=>Output
        public bool AggrgateYoutubeUrl { get; set; } // YoutubeOptions=>Aggregate
        public bool OutputYoutubeUrlJson { get; set; } // YoutubeOptions=>Json
        public bool DisplayYoutubeUrlList { get; set; } // YoutubeOptions=>display

        public bool DownloadYoutubeVideos { get; set; } // Backup=>download
        public string DownloadArguments { get; set; } // Backup=>Arguments
        public string DownloadToolLocation { get; set; } // Backup=>ToolLocation
        public string DownloadRootFolder { get; set; } // Backup=>RootFolder
        public bool DownloadRedownload { get; set; } // Backup=>Redownload


        public Action Action { get; set; }

        public GlobalConfiguration(DefaultOptions options, Action action)
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
