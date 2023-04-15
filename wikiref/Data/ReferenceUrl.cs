using System.Security.Cryptography.X509Certificates;

namespace WikiRef
{
    public class ReferenceUrl
    {
        public string Url { get; set; }
        public bool IsValid { get; set; }

        public ReferenceUrl() { }
        public ReferenceUrl(string Url) 
        {
            this.Url = Url;
        }
    }
}
