using WikiRef.Data;

namespace WikiRef
{
    public interface IAppConfiguration
    {
        Action Action { get; }
        string Category { get; }
        bool ConsoleOutputToDefaultHtmlFile { get; }
        bool ConsoleOutputToDefaultLogFile { get; }
        string ConsoleOutputToHtmlFile { get; }
        string ConsoleOutputToLogFile { get; }
        bool DownloadChannel { get; }
        string DownloadOutputScriptName { get; }
        bool DownloadPlaylist { get; }
        string DownloadRootFolder { get; }
        string DownloadToolArguments { get; }
        string DownloadToolLocation { get; }
        string DownloadVideoFileExtension { get; }
        bool ExportRefToTextFile { get; }
        string Inputjson { get; }
        bool Ipv4Only { get; }
        string Namespace { get; set; }
        bool NoColor { get; }
        bool OutputJsonToDefaultFile { get; }
        string OutputJsonToFile { get; }
        string Page { get; }
        string Password { get; }
        bool Redownload { get; }
        string ReportPage { get; }
        bool Silent { get; }
        int Throttle { get; }
        string Url { get; }
        string User { get; }
        bool Verbose { get; }
        bool WaitForArchiving { get; }
        string WhiteList { get; }
    }

    public class AppConfiguration : IAppConfiguration
    {
        public AppConfiguration(DefaultOptions options)
        {
            Action = Action.Undefined;
            InitalizeOptions(options);
        }

        public bool Silent { get; private set; }
        public bool Verbose { get; private set; }
        public bool NoColor { get; private set; }
        public int Throttle { get; private set; }
        public bool Ipv4Only { get; private set; }

        public bool ConsoleOutputToDefaultLogFile { get; private set; }
        public bool ConsoleOutputToDefaultHtmlFile { get; private set; }
        public string ConsoleOutputToLogFile { get; private set; }
        public string ConsoleOutputToHtmlFile { get; private set; }

        public bool ExportRefToTextFile { get; private set; }

        public void InitalizeOptions(DefaultOptions options)
        {
            Verbose = options.Verbose;
            Silent = options.Silent;
            NoColor = options.NoColor;
            Throttle = options.Throttle;
            Ipv4Only = options.IpV4Only;

            ConsoleOutputToDefaultLogFile = options.ConsoleOutputToDefaultLogFile;
            ConsoleOutputToDefaultHtmlFile = options.ConsoleOutputToDefaultHtmlFile;
            ConsoleOutputToLogFile = options.ConsoleOutputToLogFile;
            ConsoleOutputToHtmlFile = options.ConsoleOutputToHtmlFile;

            ExportRefToTextFile = options.ExportReferencesToFile;

            if (options is ArchiveOptions)
                InitalizeOptions(options as ArchiveOptions);
            else if (options is YoutubeDownloadOptions)
                InitalizeOptions(options as YoutubeDownloadOptions);
            else if (options is AnalyzeOptions)
                InitalizeOptions(options as AnalyzeOptions);
            else if (options is PublishOptions)
                InitalizeOptions(options as PublishOptions);
        }

        // shared options
        public Action Action { get; private set; }
        public string Inputjson { get; private set; }

        // analyze verb options
        public string Url { get; private set; }
        public string Namespace { get; set; }
        public string Category { get; private set; }
        public string Page { get; private set; }
        public string WhiteList { get; private set; }
        public string OutputJsonToFile { get; private set; }
        public bool OutputJsonToDefaultFile { get; private set; }
        public void InitalizeOptions(AnalyzeOptions options)
        {
            Action = Action.Analyse;

            Url = options.Url;
            Namespace = options.Namespace;
            Page = options.Page;
            Category = options.Category;
            WhiteList = options.WhiteList;

            OutputJsonToFile = options.OutputJsonToFile;
            OutputJsonToDefaultFile = options.OutputJsonToDefaultFile;
        }

        // download-youtube verb options
        public string DownloadToolArguments { get; private set; }
        public string DownloadToolLocation { get; private set; }
        public string DownloadRootFolder { get; private set; }
        public bool Redownload { get; private set; }
        public bool DownloadPlaylist { get; private set; }
        public bool DownloadChannel { get; private set; }
        public string DownloadOutputScriptName { get; private set; }
        public string DownloadVideoFileExtension { get; private set; }
        public void InitalizeOptions(YoutubeDownloadOptions options)
        {
            Action = Action.Backup;

            Inputjson = options.InputJson;
            DownloadToolArguments = options.Arguments;
            DownloadToolLocation = options.ToolLocation;
            DownloadRootFolder = options.OutputFolder;
            Redownload = options.Redownload;
            DownloadPlaylist = options.DownloadPlaylist;
            DownloadChannel = options.DownloadChannel;
            DownloadOutputScriptName = options.OutputScriptName;
            DownloadVideoFileExtension = options.Extension;
        }

        // archive verb options
        public bool WaitForArchiving { get; private set; }
        public void InitalizeOptions(ArchiveOptions options)
        {
            Action = Action.Archive;

            Inputjson = options.InputJson;
            WaitForArchiving = options.Wait;
        }

        // publish verb options
        public string User { get; private set; }
        public string Password { get; private set; }
        public string ReportPage { get; private set; }
        public void InitalizeOptions(PublishOptions options)
        {
            Action = Action.Archive;

            Inputjson = options.InputJson;
            Url = options.WikiApi;
            User = options.User;
            Password = options.Password;
            ReportPage = options.ReportPage;

        }
    }
}
