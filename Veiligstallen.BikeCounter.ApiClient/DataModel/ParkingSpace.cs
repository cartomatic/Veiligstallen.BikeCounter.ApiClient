using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class ParkingSpace
    {
        /// <summary>
        /// Type of a parking space
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        //public ParkingSpaceType Type { get; set; } //not strongly typed anymore!

        /// <summary>
        /// Vehicles
        /// </summary>
        [JsonProperty("vehicles")]
        public Vehicle[] Vehicles { get; set; }
    }
}
