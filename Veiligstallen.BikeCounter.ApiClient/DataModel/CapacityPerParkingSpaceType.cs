using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CapacityPerParkingSpaceType
    {
        [JsonProperty("parkingSpaceOf")]
        public ParkingSpace ParkingSpaceOf { get; set; }

        [JsonProperty("numberOfVehicles")]
        public int NumberOfVehicles { get; set; }
    }
}
