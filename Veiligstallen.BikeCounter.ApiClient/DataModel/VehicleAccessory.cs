using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class VehicleAccessory
    {
        [JsonProperty("type")]
        public VehicleAccessoryType Type { get; set; }

        [JsonProperty("position")]
        public VehicleAccessoryPosition Position { get; set; }
    }
}
