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
        [JsonProperty("localId")]
        public string LocalId { get; set; }

        /// <summary>
        /// Parking location's local id
        /// </summary>
        [JsonProperty("parkingLocationLocalId")]
        public string ParkingLocationLocalId { get; set; }

        /// <summary>
        /// Parking location system id
        /// </summary>
        [JsonProperty("parkingLocation")]
        public string ParkingLocation { get; set; }

        /// <summary>
        /// Name of a section
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Layout of a section
        /// </summary>
        [JsonProperty("layout")]
        public string Layout { get; set; }

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
        /// Level
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// Parking system type: r = rek, o = etagerek onder, b = etagerek boven, n = nietje, x = geen voorziening
        /// </summary>
        [JsonProperty("parkingSystemType")]
        public string ParkingSystemType { get; set; }

        /// <summary>
        /// Type of a vehicle owner
        /// </summary>
        [JsonProperty("vehicleOwnerType")]
        public string VehicleOwnerType { get; set; }

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
