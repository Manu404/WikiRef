using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiRef.Commons.Data;

namespace WikiRef.Wiki
{
    public class Wiki
    {
        List<WikiCategory> category = new List<WikiCategory>();
        private List<WikiNamespace> namespaces;
        private bool namespaceChanged;
        [JsonIgnore]
        public List<WikiCategory> Categories
        {
            get
            {
                var listOfCats = Namespaces.SelectMany(p => p.Pages).SelectMany(p => p.Categories);
                foreach (var cat in listOfCats)
                {
                    category.Add(new WikiCategory()
                    {
                        Name = cat.Name,
                        Pages = Namespaces.SelectMany(p => p.Pages).Where(p => p.Categories.Contains(cat)).ToList()
                    });
                }
                return category;
            }
        }

        [JsonProperty] public List<WikiNamespace> Namespaces { get => namespaces; set { namespaces = value; namespaceChanged = true; } }
        [JsonProperty] public string URL { get; set; }

        public Wiki()
        {
            Namespaces = new List<WikiNamespace>();
        }
    }
}
