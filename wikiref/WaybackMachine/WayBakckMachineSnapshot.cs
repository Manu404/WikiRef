using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace WikiRef
{
    public class WayBakckMachineSnapshot
    {
        public bool IsArchived { get; private set; }
        public DateTime Timestamp { get; private set; } 
        public int Status { get; private set; }
        
        public WayBakckMachineSnapshot(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            IsArchived = (jsonObject["archived_snapshots"]["closest"]) != null;
            if (IsArchived)
            {
                Timestamp = DateTime.ParseExact(jsonObject["archived_snapshots"]["closest"]["timestamp"].ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                Status = Int32.Parse(jsonObject["archived_snapshots"]["closest"]["status"].ToString());
            }
        }
    }
}
