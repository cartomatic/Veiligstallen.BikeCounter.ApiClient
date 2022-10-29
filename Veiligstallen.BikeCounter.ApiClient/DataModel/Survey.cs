using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Survey : Base
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("authority")]
        public string Authority { get; set; }

        [JsonProperty("contractor")]
        public string Contractor { get; set; }


        [JsonProperty("license")] 
        public string License { get; set; }
        
        [JsonProperty("canonicalVehicleCategory")]
        public string CanonicalVehicleCategory { get; set; }
    }
}
