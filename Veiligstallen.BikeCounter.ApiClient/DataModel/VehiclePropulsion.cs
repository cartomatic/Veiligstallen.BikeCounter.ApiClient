using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class VehiclePropulsion
    {
        [JsonProperty("description")]
        public VehiclePropulsionType Description { get; set; }
    }
}
