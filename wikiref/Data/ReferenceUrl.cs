namespace WikiRef.Data
{
    public class ReferenceUrl
    {
        public string Url { get; set; }
        public SourceStatus SourceStatus { get; set; }

        public ReferenceUrl() { }
        public ReferenceUrl(string Url) 
        {
            this.Url = Url;
        }
    }
}
