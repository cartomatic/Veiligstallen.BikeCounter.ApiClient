using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Measurement
    {
        [JsonProperty("parkingCapacity")]
        public int? ParkingCapacity { get; set; }

        [JsonProperty("totalParked")]
        public int? TotalParked { get; set; }

        [JsonProperty("vehicleTypeCounts")]
        public VehicleTypeCount[] VehicleTypeCounts { get; set; }
    }
}
