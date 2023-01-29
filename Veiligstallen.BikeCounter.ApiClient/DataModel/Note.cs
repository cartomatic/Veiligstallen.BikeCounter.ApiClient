using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Note
    {
        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("isClosed")]
        public bool? IsClosed { get; set; }

        [JsonProperty("isHoliday")]
        public bool? IsHoliday { get; set; }

        [JsonProperty("isEvent")]
        public bool? IsEvent { get; set; }

        [JsonProperty("isUnderConstruction")]
        public bool? IsUnderConstruction { get; set; }
    }
}
