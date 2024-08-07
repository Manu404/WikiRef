﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikiRef.Data
{
    public class WikiNamespace
    {
        [JsonProperty] public IEnumerable<WikiPage> Pages { get; set; }
        [JsonProperty] public string Name { get; set; }

        public WikiNamespace()
        {
            Pages = new List<WikiPage>();
        }
    }
}
