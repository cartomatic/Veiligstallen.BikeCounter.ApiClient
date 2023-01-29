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

        /// <summary>
        /// i = In voorziening / j = In voorziening, juist gestald / k = In voorziening, neemt plek in / p = nabij voorziening / x = buiten voorziening
        /// </summary>
        [JsonProperty("parkState")]
        public string ParkState { get; set; }

        [JsonProperty("vehicle")]
        public Vehicle Vehicle { get; set; }

    }
}
