using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Converters;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class ParkingLocation : Base
    {
        /// <summary>
        /// Object's local id
        /// </summary>
        public string LocalId { get; set; }

        /// <summary>
        /// Name of parking location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// System id of a parent
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Date valid from
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Date valid through
        /// </summary>
        public DateTime? ValidThrough { get; set; }

        /// <summary>
        /// Authority
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Extra information
        /// </summary>
        public string XtraInfo { get; set; }

        /// <summary>
        /// Geometry representation
        /// </summary>
        [JsonConverter(typeof(GeometryConverter))]
        public Geometry GeoLocation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vehicle Allows { get; set; }

        /// <summary>
        /// Parking location features
        /// </summary>
        public SurveyAreaType[] Features { get; set; }
    }
}
