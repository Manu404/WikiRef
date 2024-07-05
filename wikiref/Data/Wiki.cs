using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikiRef.Data
{
    public class Wiki
    {
        private List<WikiNamespace> _namespaces;
        private bool _namespaceChanged;

        [JsonProperty] public List<WikiNamespace> Namespaces { get => _namespaces; set { _namespaces = value; _namespaceChanged = true; } }
        [JsonProperty] public string Url { get; set; }

        public Wiki()
        {
            Namespaces = new List<WikiNamespace>();
        }
    }
}
