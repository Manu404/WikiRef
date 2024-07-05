using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikiRef.Data
{
    public class WikiPage
    {
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public List<YoutubeUrl> YoutubeUrls { get; set; }
        [JsonProperty] public List<Reference> References { get; set; }
        [JsonProperty] public List<WikiCategory> Categories { get; set; }
        [JsonProperty] public string Content { get; set; }

        [JsonIgnore] public int MalformedDates { get; set; }
        [JsonIgnore] public int DatesCount { get; set; }
        [JsonIgnore] public int WikiLinks { get; set; }
    }
}
