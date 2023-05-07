using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiRef.Commons.Data;

namespace WikiRef.Wiki
{

    public class WikiCategory
    {
        [JsonIgnore] public List<WikiPage> Pages { get; set; }
        [JsonProperty] public string Name { get; set; }

        public WikiCategory()
        {
        }
    }
}
