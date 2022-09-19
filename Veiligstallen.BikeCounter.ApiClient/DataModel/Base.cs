using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public abstract class Base
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
