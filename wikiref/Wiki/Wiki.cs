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
        //private List<WikiCategory> _category = new List<WikiCategory>();
        private List<WikiNamespace> _namespaces;
        private bool _namespaceChanged;

        //[JsonIgnore]
        //public List<WikiCategory> Categories
        //{
        //    get
        //    {
        //        var listOfCats = Namespaces.SelectMany(p => p.Pages).SelectMany(p => p.Categories);
        //        foreach (var cat in listOfCats)
        //        {
        //            _category.Add(new WikiCategory()
        //            {
        //                Name = cat.Name,
        //                Pages = Namespaces.SelectMany(p => p.Pages).Where(p => p.Categories.Contains(cat)).ToList()
        //            });
        //        }
        //        return _category;
        //    }
        //}

        [JsonProperty] public List<WikiNamespace> Namespaces { get => _namespaces; set { _namespaces = value; _namespaceChanged = true; } }
        [JsonProperty] public string URL { get; set; }

        public Wiki()
        {
            Namespaces = new List<WikiNamespace>();
        }
    }
}
