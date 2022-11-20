using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class VehicleState
    {
        [JsonProperty("description")]
        public VehicleStateType Description { get; set; }
    }
}
