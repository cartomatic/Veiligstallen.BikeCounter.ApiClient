﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Converters;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class SurveyArea : Base
    {

        /// <summary>
        /// Object's local id
        /// </summary>
        [JsonProperty("localId")]
        public string LocalId { get; set; }

        /// <summary>
        /// Local id of a parent
        /// </summary>
        [JsonIgnore]
        public string ParentLocalId { get; set; }

        /// <summary>
        /// Name of survey area
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// System id of a parent
        /// </summary>
        [JsonProperty("parent")]
        public string Parent { get; set; }

        /// <summary>
        /// Date valid from
        /// </summary>
        [JsonProperty("validFrom")]
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Date valid through
        /// </summary>
        [JsonProperty("validThrough")]
        public DateTime? ValidThrough { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        [JsonProperty("authority")]
        public string Authority { get; set; }

        /// <summary>
        /// Extra information
        /// </summary>
        [JsonProperty("xtraInfo")] 
        public string XtraInfo { get; set; }

        /// <summary>
        /// Type of survey area
        /// </summary>
        [JsonProperty("surveyAreaType")]
        public string SurveyAreaType { get; set; }

        /// <summary>
        /// Geometry representation
        /// </summary>
        [JsonConverter(typeof(GeometryConverter))]
        [JsonProperty("geoLocation")]
        public Geometry GeoLocation { get; set; }

        /// <summary>
        /// Geometry wkt
        /// </summary>
        [JsonIgnore]
        public string GeomWkt { get; set; }
    }
}
