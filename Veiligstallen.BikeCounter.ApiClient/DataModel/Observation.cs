using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Observation : Base
    {
        [JsonProperty("survey")]
        public string Survey { get; set; }

        [JsonProperty("observedProperty")]
        public string ObservedProperty { get; set; }

        [JsonProperty("featureOfInterest")]
        public string FeatureOfInterest { get; set; }

        [JsonProperty("timestampStart")]
        public DateTime? TimestampStart { get; set; }

        [JsonProperty("timestampEnd")]
        public DateTime? TimestampEnd { get; set; }

        [JsonProperty("measurement")]
        public Measurement Measurement { get; set; }

        [JsonProperty("note")]
        public Note Note { get; set; }
        
        /// <summary>
        /// utility property used for looking up sections when FeatureOfInterestIsNotDefined
        /// </summary>
        [JsonIgnore]
        public string SectionLocalId { get; set; }
    }
}
