using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class VehicleTypeCount
    {
        [JsonProperty("canonicalVehicleCode")]
        public string CanonicalVehicleCode { get; set; }

        [JsonProperty("numberOfVehicles")]
        public int NumberOfVehicles { get; set; }
    }
}
