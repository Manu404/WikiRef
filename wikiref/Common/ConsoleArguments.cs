using CommandLine;

namespace WikiRef
{
    [Verb("default", HelpText = "Provide analysis features regarding references. '--help analyse' for more informations.")]
    public class DefaultOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "silent", Required = false, HelpText = "Produce no output in the console.")]
        public bool Silent { get; set; }

        [Option('b', "color-blind", Required = false, HelpText = "Disable coloring of output for compatibility issues with certain terminal.")]
        public bool NoColor { get; set; }

        [Option('t', "throttle", Required = false, HelpText = "Give a value in second to enable throttling to avoid '429 : Too Many Request' errors. Mainly for YouTube. That will slow down the speed of the too but avoid temporary banning.")]
        public int Throttle { get; set; }

        // Log and output 
        [Option('l', Default = false, Required = false, HelpText = "Write the console output in a file in the executing folder using the date and time as name")]
        public bool ConsoleOutputToDefaultFile { get; set; }

        [Option("log", Required = false, HelpText = "Write the console output in a file in the executing folder using the date and time as name")]
        public string ConsoleOutputToFile { get; set; }

        [Option('h', Default = false, Required = false, HelpText = "Write the console output in a file in html format using the date and time as name")]
        public bool ConsoleToDefaultHtmlFile { get; set; }

        [Option("html", Required = false, HelpText = "Write the console output in a file in html format using the date and time as name")]
        public string ConsoleToHtmlFile { get; set; }
    }

    [Verb("analyse", HelpText = "Provide analysis features regarding references. '--help analyze' for more informations.")]
    public class AnalyseOptions : DefaultOptions
    {
        [Option('w', "wiki", Required = true, HelpText = "Url of the wiki to analyze, for eg: https://wikipedia.com - Required")]
        public string Wiki { get; set; }

        [Option('c', "category", Required = true, HelpText = "The category to analyse, without the 'Category:' prefix.", SetName = "category")]
        public string Category { get; set; }

        [Option('p', "page", Required = true, HelpText = "The name of page a to analyse.", SetName = "page")]
        public string Page { get; set; }

        [Option('j', Default = false, Required = false, HelpText = "Output the YouTube urls grouped by page in a file in json format for download")]
        public bool OutputJsonToDefaultFile { get; set; }

        [Option("json", Required = false, HelpText = "Output the YouTube urls grouped by page in a file in json format for download")]
        public string OutputJsonToFile { get; set; }
    }

    [Verb("archive", HelpText = "Generate a wayback machine archive of non-video content. '--help archive' for more informations.")]
    class ArchiveOptions : DefaultOptions
    {

    }

    [Verb("script", HelpText = "Generate a bash script relying  on yt-dlp to download youtube references. '--help backup' for more informations.")]
    class YoutubeDownloadOption : DefaultOptions
    {
        [Option('i', "input-json", Required = true, HelpText = "Json to use as source.")]
        public string InputJson { get; set; }

        [Option("tool", Required = true, HelpText = "Location of yt-dlp.")]
        public string ToolLocation { get; set; }

        [Option('d', "directory", Required = true, HelpText = "Root folder where videos will be placed. A subfolder using the page name will be created to place the video.")]
        public string OutputFolder { get; set; }

        [Option("output-script", Required = false, Default = "download.sh", HelpText = "Name of the output script.")]
        public string OutputScriptName { get; set; }

        [Option('a', "arguments", Default = "-S res,ext:mp4:m4a --recode mp4", HelpText = "Yt -dlp arguments. Default argument produce compressed mp4. Cfr. https://github.com/yt-dlp/yt-dlp for more information")]
        public string Arguments { get; set; }

        [Option("redownload", Default = false, Required = false, HelpText = "Redownload the video, even if they already exists locally.", SetName = "redownload")]
        public bool Redownload { get; set; }

        [Option("download-playlist", Default = false, Required = false, HelpText = "Download playlist url content. Default: false")]
        public bool DownloadPlaylist { get; set; }

        [Option("download-channel", Default = false, Required = false, HelpText = "Download channel url content. Default: false")]
        public bool DownloadChannel { get; set; }
    }
}
