using CommandLine;
using WikiRef.Common;

namespace WikiRef
{
    [Verb("default", HelpText = "Provide analysis features regarding references. '--help analyse' for more informations.")]
    public class DefaultOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Verbose console output.")]
        public bool Verbose { get; set; }

        [Option('s', "silent", Required = false, HelpText = "No console output.")]
        public bool Silent { get; set; }

        [Option('b', "color-blind", Required = false, HelpText = "Disable console coloring (compatibility).")]
        public bool NoColor { get; set; }

        [Option('t', "throttle", Required = false, HelpText = "Provide a value in seconds for throttling to avoid '429 : Too Many Request' errors.")]
        public int Throttle { get; set; }

        [Option('l', Default = false, Required = false, HelpText = "Write the console output in a file using the date and time as name.")]
        public bool ConsoleOutputToDefaultLogFile { get; set; }

        [Option("log", Required = false, HelpText = "Write the console output to the given file.")]
        public string ConsoleOutputToLogFile { get; set; }

        [Option('h', Default = false, Required = false, HelpText = "Write the console output in a file in html format using the date and time as name.")]
        public bool ConsoleOutputToDefaultHtmlFile { get; set; }

        [Option("html", Required = false, HelpText = "Write the console output in a file in html format to the given file.")]
        public string ConsoleOutputToHtmlFile { get; set; }

        [Option('4', "ipv4", Required = false, HelpText = "Default request to ipv4 (compatibility).")]
        public bool IpV4Only { get; set; }

        [Option("export-ref", Required = false, HelpText = "Export references in text file for debug.")]
        public bool ExportReferencesToFile { get; set; }
    }

    [Verb("analyze", HelpText = "Provide analysis features regarding references. '--help analyze' for more informations.")]
    public class AnalyzeOptions : DefaultOptions
    {
        [Option('a', "api", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string Url { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Name of a namespace to analyse.", SetName = "namespace")]
        public string Namespace { get; set; }

        [Option('c', "category", Required = true, HelpText = "Name of a category to analyse.", SetName = "category")]
        public string Category { get; set; }

        [Option('p', "page", Required = true, HelpText = "Name of a page to analyse.", SetName = "page")]
        public string Page { get; set; }

        [Option('j', Default = true, Required = false, HelpText = "Output the analysis in a json file using the date and time as filename.")]
        public bool OutputJsonToDefaultFile { get; set; }

        [Option("json", Required = false, HelpText = "Output the analysis in a json file to the given filename.")]
        public string OutputJsonToFile { get; set; }

        [Option("whitelist", Required = false, HelpText = "Json file containing white listed domains that will not be analyzed.")]
        public string WhiteList { get; set; }
    }

    [Verb("publish", HelpText = "Publish the analysis result to a wiki. '--help analyse' for more informations.")]
    public class PublishOptions : DefaultOptions
    {
        [Option('a', "api", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string WikiApi { get; set; }

        [Option('i', "input", Required = false, HelpText = "Input json source from analyze.")]
        public string InputJson { get; set; }

        [Option('u', "user", Required = true, HelpText = "Username to connect to the mediawiki instance.")]
        public string User { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password to connect to the mediawiki instance.")]
        public string Password { get; set; }

        [Option("report-page", Required = true, HelpText = "The page where to publish the report.")]
        public string ReportPage { get; set; }
    }

    [Verb("archive", HelpText = "Generate a wayback machine archive of non-video content. '--help archive' for more informations.")]
    public class ArchiveOptions : DefaultOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input json source from analyze.")]
        public string InputJson { get; set; }

        [Option('w', "wait", Required = false, Default = false, HelpText = "Wait for the archival to be done before processing other archival requests.")]
        public bool Wait { get; set; }
    }

    [Verb("script", HelpText = "Generate a bash script relying on yt-dlp to download youtube references. '--help script' for more informations.")]
    public class YoutubeDownloadOptions : DefaultOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input json source from analyze.")]
        public string InputJson { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Root folder where videos will be placed. Videos will be placed in subfolder per wiki page.")]
        public string OutputFolder { get; set; }

        [Option("output-script", Required = false, Default = "download.sh", HelpText = "Name of the output script.")]
        public string OutputScriptName { get; set; }

        [Option("tool", Required = true, HelpText = "Location of yt-dlp. Go to https://github.com/yt-dlp/yt-dlp for more informations.")]
        public string ToolLocation { get; set; }

        [Option("tool-arguments", Default = "-S res,ext:mp4:m4a --recode mp4", Required = false, HelpText = "Yt-dlp arguments. Default argument produce compressed mp4. Cfr. https://github.com/yt-dlp/yt-dlp for more information")]
        public string Arguments { get; set; }

        [Option('e', "extension", Default = "mp4", Required = false, HelpText = "Video file extension. Cfr. https://github.com/yt-dlp/yt-dlp for more information")]
        public string Extension { get; set; }

        [Option("redownload", Default = false, Required = false, HelpText = "Redownload the video even if it already exists locally.", SetName = "redownload")]
        public bool Redownload { get; set; }

        [Option("download-playlist", Default = false, Required = false, HelpText = "Download playlist content.")]
        public bool DownloadPlaylist { get; set; }

        [Option("download-channel", Default = false, Required = false, HelpText = "Download channel content.")]
        public bool DownloadChannel { get; set; }
    }
}
