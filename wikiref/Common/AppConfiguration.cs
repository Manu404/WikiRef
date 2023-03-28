namespace WikiRef
{
    class AppConfiguration
    {
        public bool Verbose { get; private set; } // DefaultOptions=>Verbose
        public string WikiUrl { get; private set; } // DefaultOptions=>Wiki
        public string Category { get; private set; } // DefaultOptions=>Category
        public string Page { get; private set; } // DefaultOptions=>Page
        public bool Silent { get; private set; } // DefaultOptions=>Silent
        public bool LogOutputToFile { get; private set; } // DefaultOptions=>LogOtuputToFile
        public bool NoColor { get; private set; } // DefaultOption=>NoColor
        public int Throttle { get; private set; } // Default=>Throttle

        public bool ExportToFile { get; private set; }  // export the annalyis output to file

        public string YoutubeListDestinationFile { get; private set; } // YoutubeOptions=>Output
        public bool AggrgateYoutubeUrl { get; private set; } // YoutubeOptions=>Aggregate
        public bool OutputYoutubeUrlJson { get; private set; } // YoutubeOptions=>Json
        public bool DisplayYoutubeUrlList { get; private set; } // YoutubeOptions=>display
        public bool OnlyValidLinks { get; private set; }  // Display or export only valid links.

        public bool DownloadYoutubeVideos { get; private set; } // Backup=>download
        public string DownloadArguments { get; private set; } // Backup=>Arguments
        public string DownloadToolLocation { get; private set; } // Backup=>ToolLocation
        public string DownloadRootFolder { get; private set; } // Backup=>RootFolder
        public bool DownloadRedownload { get; private set; } // Backup=>Redownload
        public bool DownloadPlaylist { get; private set; } // Backupt=>download-playlist
        public bool DownloadChannel { get; private set; } // Backupt=>download-channel
        public string DownloadInputJson { get; private set; }

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
            LogOutputToFile = options.LogOtuputToFile;
            NoColor = options.NoColor;
            Throttle = options.Throttle;

            if (options is YoutubeOptions)
                InitalizeOptions(options as YoutubeOptions);
            else if (options is ArchiveOptions)
                InitalizeOptions(options as ArchiveOptions);
            else if (options is YoutubeDownloadOption)
                InitalizeOptions(options as YoutubeDownloadOption);
        }

        private void InitalizeOptions(YoutubeOptions options)
        {
            AggrgateYoutubeUrl = options.Aggregate;
            OutputYoutubeUrlJson = options.OutputJson;
            DisplayYoutubeUrlList = options.Display;
            OnlyValidLinks = options.OnlyValidLinks;
        }

        private void InitalizeOptions(ArchiveOptions options)
        {

        }

        private void InitalizeOptions(YoutubeDownloadOption options)
        {
            DownloadYoutubeVideos = options.Download;
            DownloadArguments = options.Arguments;
            DownloadToolLocation = options.ToolLocation;
            DownloadRootFolder = options.RootFolder;
            DownloadRedownload = options.Redownload;
            DownloadInputJson = options.InputJson;
        }

    }
}
