using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    [Obsolete ($"It looks like the {nameof(VehicleState)} is not required as a wrapper for the {nameof(VehicleStateType)} anymore")]
    public class VehicleState
    {
        [JsonProperty("description")]
        public VehicleStateType Description { get; set; }
    }
}
