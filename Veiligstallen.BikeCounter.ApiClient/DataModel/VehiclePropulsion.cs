using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    [Obsolete($"It looks like the {nameof(VehiclePropulsion)} is not required as a wrapper for the {nameof(VehiclePropulsionType)} anymore")]
    public class VehiclePropulsion
    {
        [JsonProperty("description")]
        public VehiclePropulsionType Description { get; set; }
    }
}
