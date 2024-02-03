using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Vehicle
    {
        /// <summary>
        /// Vehicle type
        /// </summary>
        [JsonProperty("type")]
        public VehicleType Type { get; set; }

        /// <summary>
        /// Vehicle propulsion
        /// </summary>
        [JsonProperty("propulsion")]
        public VehiclePropulsionType[] Propulsion { get; set; }

        /// <summary>
        /// Vehicle appearance
        /// </summary>
        [JsonProperty("appearance")]
        public VehicleAppearance Appearance { get; set; }

        /// <summary>
        /// Vehicle state
        /// </summary>
        [JsonProperty("state")]
        public VehicleStateType[] State { get; set; }

        /// <summary>
        /// Vehicle accessories
        /// </summary>
        [JsonProperty("accessories")]
        public VehicleAccessory[] Accessories { get; set; }

        /// <summary>
        /// Vehicle owner
        /// </summary>
        [JsonProperty("owner")]
        public VehicleOwner Owner { get; set; }
    }
}
