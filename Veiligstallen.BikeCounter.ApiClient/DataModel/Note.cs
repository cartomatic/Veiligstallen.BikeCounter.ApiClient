using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Note
    {
        [JsonProperty("remark")]
        public string Remark { get; set; }
    }
}
