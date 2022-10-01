using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CanonicalVehicle : Base
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
        
        //public CanonicalVehicleDefinition Json { get; set; }
        [JsonProperty("json")]
        public JObject Json { get; set; }
    }
}
