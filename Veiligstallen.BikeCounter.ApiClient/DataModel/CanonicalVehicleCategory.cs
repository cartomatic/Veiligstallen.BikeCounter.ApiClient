using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CanonicalVehicleCategory : Base
    {
        //Name not there anymore as of 20221028
        //[JsonProperty("name")]
        //public string Name { get; set; }

        [JsonProperty("authority")]
        public string Authority { get; set; }
    }
}
