using Newtonsoft.Json;
using System.Collections.Generic;

namespace WikiRef.Data
{
    public class YoutubeUrl
    {
        [JsonProperty] public List<string> Urls { get; set; }
        [JsonProperty] public string VideoId { get; set; }
        [JsonProperty] public string Name { get;  set; }
        [JsonProperty] public string ChannelName { get; set; }
        [JsonProperty] public SourceStatus IsValid { get; set; }
        [JsonProperty] public bool IsPlaylist { get; set; }
        [JsonProperty] public bool IsUser { get;  set; }
        [JsonProperty] public bool IsCommunity { get; set; }
        [JsonProperty] public bool IsAbout { get; set; }
        [JsonProperty] public bool IsChannel { get; set; }
        [JsonProperty] public bool IsHome { get; set; }
        [JsonProperty] public bool IsVideo { get; set; }

        [JsonIgnore]
        public string AggregatedUrls => string.Join(" ", Urls);

        [JsonIgnore]
        public string VideoUrl => $"https://www.youtube.com/watch?v={VideoId}";
    }
}
