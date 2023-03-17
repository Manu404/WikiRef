using CommandLine;

namespace WikiRef
{

    [Verb("default", HelpText = "Provide analysis features regarding references. '--help analyze' for more informations.")]
    public class DefaultOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "silent", Required = false, HelpText = "Produce no output in the console.")]
        public bool Silent { get; set; }

        [Option('c', "category", Required = false, HelpText = "The category to analyse, without the 'Category:' prefix.", SetName = "category")]
        public string Category { get; set; }

        [Option('p', "page", Required = false, HelpText = "The name of page a to analyse.", SetName = "page")]
        public string Page { get; set; }

        [Option('w', "wiki", Required = true, HelpText = "Url of the wiki to analyze, for eg: https://wikipedia.com - Required")]
        public string Wiki { get; set; }

        [Option('o', "file-output", Required = false, HelpText = "Write the console output in a file in the executing folder using the date and time as name")]
        public bool ConsoleOutputToFile { get; set; }

        [Option('f', "no-color", Required = false, HelpText = "Disable coloring of input for certain terminal with compatibility issues.")]
        public bool NoColor { get; set; }
    }

    [Verb("analyze", HelpText = "Provide analysis features regarding references. '--help analyze' for more informations.")]
    public class AnalyseOptions : DefaultOptions
    {
    }

    [Verb("youtube", HelpText = "Generate a list of youtube video links used in references. '--help youtube' for more informations.")]
    class YoutubeOptions : DefaultOptions
    {
        [Option('a', "aggregate", Required = false, HelpText = "Aggregate youtunbe url with same adress but different timestamp.")]
        public bool Aggregate { get; set; }

        [Option('j', "json-output", Required = false, HelpText = "Output the youtube url grouped by page in json format.")]
        public bool OutputJson { get; set; }

        [Option('d', "display", Default = true, Required = false, HelpText = "Display the youtube urls. Defualt: true")]
        public bool Display { get; set; }
    }

    [Verb("archive", HelpText = "Generate a wayback machine archive of non-video content. '--help youtube' for more informations.")]
    class WaybackLachineArchivingOptions : DefaultOptions
    {
    }
}
