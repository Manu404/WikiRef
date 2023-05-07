using CommandLine;
using WikiRef.Commons;

namespace WikiRef
{
    [Verb("compare", HelpText = "Provide analysis features regarding references. '--help analyse' for more informations.")]
    public class JsonCompareOption : DefaultOptions
    {
        [Option("file-a", Required = true, HelpText = "")]
        public string FileA { get; set; }

        [Option("file-b", Required = true, HelpText = "")]
        public string FileB { get; set; }
    }
}
