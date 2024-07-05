using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikiRef.Data
{

    public class WikiCategory
    {
        [JsonIgnore] public IEnumerable<WikiPage> Pages { get; set; }
        [JsonProperty] public string Name { get; set; }

        public WikiCategory()
        {
        }
    }
}
