using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Converters;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class ParkingLocation : Base
    {
        public string LocalId { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidThrough { get; set; }
        public string Authority { get; set; }
        public string XtraInfo { get; set; }

        [JsonConverter(typeof(GeometryConverter))]
        public Geometry GeoLocation { get; set; }

        public Vehicle Allows { get; set; }
        public SurveyAreaType[] Features { get; set; }
    }
}
