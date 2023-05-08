using CommandLine;
using WikiRef.Commons;

namespace WikiRef
{
    [Verb("analyze", HelpText = "Provide analysis features regarding references. '--help analyse' for more informations.")]
    public class AnalyseOptions : DefaultOptions
    {
        [Option('a', "api", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string Wiki { get; set; }

        [Option('n', "namespace", Required = true, HelpText = "Name of the category to analyse, without the 'Category:' prefix.", SetName = "namespace")]
        public string Namespace { get; set; }

        [Option('c', "category", Required = true, HelpText = "Name of the category to analyse, without the 'Category:' prefix.", SetName = "category")]
        public string Category { get; set; }

        [Option('p', "page", Required = true, HelpText = "Name of page a to analyse.", SetName = "page")]
        public string Page { get; set; }

        [Option('j', Default = false, Required = false, HelpText = "Output the analysis in a json file using the date and time as name")]
        public bool OutputJsonToDefaultFile { get; set; }

        [Option("json", Required = false, HelpText = "Output the analysis in a json file to the given filename")]
        public string OutputJsonToFile { get; set; }

        [Option("whitelist", Required = false, HelpText = "Json file containing white listed domain that will not be analyzed.")]
        public string WhiteList { get; set; }
    }

    [Verb("publish", HelpText = "Publish the analysis result to a wiki. '--help analyse' for more informations.")]
    public class PublishOptions : DefaultOptions
    {
        [Option('a', "api", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string WikiApi { get; set; }

        [Option('i', "input", Required = false, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string InputJson { get; set; }

        [Option('u', "user", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string User { get; set; }

        [Option('p', "password", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string Password { get; set; }

        [Option("report-page", Required = true, HelpText = "Url of the wiki api to analyze, for eg: https://en.wikipedia.org/w/ - Required")]
        public string ReportPage { get; set; }
    }

    [Verb("archive", HelpText = "Generate a wayback machine archive of non-video content. '--help archive' for more informations.")]
    public class ArchiveOptions : DefaultOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input json source from analyze.")]
        public string InputJson { get; set; }

        [Option('w', "wait", Required = false, Default = false, HelpText = "Input json source.")]
        public bool Wait { get; set; }
    }

    [Verb("script", HelpText = "Generate a bash script relying on yt-dlp to download youtube references. '--help script' for more informations.")]
    public class YoutubeDownloadOption : DefaultOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input json source from analyze.")]
        public string InputJson { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Root folder where videos will be placed. Videos will be placed in subfolder per wiki page.")]
        public string OutputFolder { get; set; }

        [Option("output-script", Required = false, Default = "download.sh", HelpText = "Name of the output script.")]
        public string OutputScriptName { get; set; }

        [Option("tool", Required = true, HelpText = "Location of yt-dlp. Go to https://github.com/yt-dlp/yt-dlp for more information")]
        public string ToolLocation { get; set; }

        [Option("tool-arguments", Default = "-S res,ext:mp4:m4a --recode mp4", Required = false, HelpText = "Yt -dlp arguments. Default argument produce compressed mp4. Cfr. https://github.com/yt-dlp/yt-dlp for more information")]
        public string Arguments { get; set; }

        [Option('e', "extension", Default = "mp4", Required = false, HelpText = "Video file extension. Default: mp4. Cfr. https://github.com/yt-dlp/yt-dlp for more information")]
        public string Extension { get; set; }

        [Option("redownload", Default = false, Required = false, HelpText = "Redownload the video, even if they already exists locally.", SetName = "redownload")]
        public bool Redownload { get; set; }

        [Option("download-playlist", Default = false, Required = false, HelpText = "Download playlist url content. Default: false")]
        public bool DownloadPlaylist { get; set; }

        [Option("download-channel", Default = false, Required = false, HelpText = "Download channel url content. Default: false")]
        public bool DownloadChannel { get; set; }
    }
}
