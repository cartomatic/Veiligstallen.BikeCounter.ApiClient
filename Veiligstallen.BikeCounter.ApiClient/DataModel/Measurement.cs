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

        [JsonProperty("capacityPerParkingSpaceTypes")]
        public CapacityPerParkingSpaceType[] CapacityPerParkingSpaceTypes { get; set; }

        [JsonProperty("totalParked")]
        public int? TotalParked { get; set; }

        [JsonProperty("occupiedSpaces")]
        public int? OccupiedSpaces { get; set; }

        [JsonProperty("vacantSpaces")]
        public int? VacantSpaces { get; set; }

        [JsonProperty("vehicleTypeCounts")]
        public VehicleTypeCount[] VehicleTypeCounts { get; set; }

        /// <summary>
        /// id of observation that vacant spaces is based on
        /// </summary>
        [JsonProperty("basedOffCapacity")]
        public string BasedOffCapacity { get; set; }
    }
}
