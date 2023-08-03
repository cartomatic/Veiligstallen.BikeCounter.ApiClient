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

        [JsonProperty("surveyAreaParent")]
        public string SurveyAreaParent { get; set; }

        [JsonProperty("parkingLocation")]
        public string ParkingLocation { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }

        [JsonProperty("capacityObservation")]
        public Observation CapacityObservation { get; set; }

        [JsonProperty("occupationObservation")]
        public Observation OccupationObservation { get; set; }

        /// <remarks>
        /// Not a part of standard, so cannot rely on the property existence
        /// </remarks>
        [JsonProperty("sectionSummary")]
        public CombinedObservationSectionSummary SectionSummary { get; set; }

        /// <remarks>
        /// Not a part of standard, so cannot rely on the property existence
        /// </remarks>
        [JsonProperty("parkingLocationSummary")]
        public CombinedObservationParkingLocationSummary ParkingLocationSummary { get; set; }
    }

    public class CombinedObservationSectionSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Object's local id
        /// </summary>
        [JsonProperty("localId")]
        public string LocalId { get; set; }

        /// <summary>
        /// Name of parking location
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Layout of a section
        /// </summary>
        [JsonProperty("layout")]
        public string Layout { get; set; }

        /// <summary>
        /// Level
        /// </summary>
        [JsonProperty("level")]
        public int? Level { get; set; }

        /// <summary>
        /// Parking space type
        /// </summary>
        [JsonProperty("parkingSpaceOf")]
        public ParkingSpace[] ParkingSpaceOf { get; set; }
    }

    public class CombinedObservationParkingLocationSummary
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Object's local id
        /// </summary>
        [JsonProperty("localId")]
        public string LocalId { get; set; }

        /// <summary>
        /// Name of parking location
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Extra information
        /// </summary>
        [JsonProperty("xtraInfo")]
        public string XtraInfo { get; set; }

        /// <summary>
        /// Parking location features
        /// </summary>
        [JsonProperty("features")]
        public string[] Features { get; set; }
    }

}
