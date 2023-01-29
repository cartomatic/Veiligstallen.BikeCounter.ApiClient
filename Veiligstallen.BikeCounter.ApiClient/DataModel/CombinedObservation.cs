using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CombinedObservation
    {
        [JsonProperty("surveyArea")]
        public string SurveyArea { get; set; }

        [JsonProperty("parkingLocation")]
        public string ParkingLocation { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }

        [JsonProperty("capacityObservation")]
        public Observation CapacityObservation { get; set; }

        [JsonProperty("occupationObservation")]
        public Observation OccupationObservation { get; set; }
    }
}
