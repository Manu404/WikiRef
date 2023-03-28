using System.Collections.Generic;

namespace WikiRef
{
    class Reference
    {
        public string Content { get; set; }
        public List<string> Urls { get; set; }
        public SourceStatus Status { get; set; }
        public bool FormattingIssue { get; set; }

        public Reference(string content)
        {
            Content = content;
            Urls = new List<string>();
        }
    }
}
