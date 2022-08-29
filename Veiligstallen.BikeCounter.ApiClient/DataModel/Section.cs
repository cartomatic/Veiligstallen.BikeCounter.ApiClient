using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Converters;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Section : Base
    {
        /// <summary>
        /// Object's local id
        /// </summary>
        public string LocalId { get; set; }

        /// <summary>
        /// Parking location's local id
        /// </summary>
        public string ParkingLocationLocalId { get; set; }

        /// <summary>
        /// Parking location system id
        /// </summary>
        public string ParkingLocation { get; set; }

        /// <summary>
        /// Name of a section
        /// </summary>
        public string Name { get; set; }

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
        /// Level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Parking system type: r = rek, o = etagerek onder, b = etagerek boven, n = nietje, x = geen voorziening
        /// </summary>
        public string ParkingSystemType { get; set; }

        /// <summary>
        /// Geometry representation
        /// </summary>
        [JsonConverter(typeof(GeometryConverter))]
        public Geometry GeoLocation { get; set; }
    }
}
