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

        public string YoutubeListDestinationFile { get; set; } // YoutubeOptions=>Output
        public bool AggrgateYoutubeUrl { get; set; } // YoutubeOptions=>Aggregate
        public bool OutputYoutubeUrlJson { get; set; } // YoutubeOptions=>Json
        public bool DisplayYoutubeUrlList { get; set; } // YoutubeOptions=>display

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

            if (options is YoutubeOptions)
                InitalizeOptions((YoutubeOptions)options);
            else if (options is WaybackLachineArchivingOptions)
                InitalizeOptions((WaybackLachineArchivingOptions)options);
        }

        private void InitalizeOptions(YoutubeOptions options)
        {
            AggrgateYoutubeUrl = options.Aggregate;
            OutputYoutubeUrlJson = options.OutputJson;
            DisplayYoutubeUrlList = options.Display;
        }

        private void InitalizeOptions(WaybackLachineArchivingOptions options)
        {
        }
    }
}
