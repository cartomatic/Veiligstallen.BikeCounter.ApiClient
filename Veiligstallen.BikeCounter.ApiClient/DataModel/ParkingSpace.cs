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
        public ParkingSpaceType Type { get; set; }

        /// <summary>
        /// Vehicles
        /// </summary>
        [JsonProperty("vehicles")]
        public Vehicle[] Vehicles { get; set; }
    }
}
