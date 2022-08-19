using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel.Converters
{
    public class GeometryConverter: JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            //not required as can write returns false
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            Geometry shape = null;

            var geomType = (string)jo["type"];
            var ja = (JArray)jo["coordinates"];

            switch(geomType)
            {
                case Geometry.TYPE_POINT:
                    shape = new Point(ja.ToObject<double[]>());
                    break;
                case Geometry.TYPE_MULTI_POINT:
                    shape = new MultiPoint(ja.ToObject<double[][]>());
                    break;
                case Geometry.TYPE_LINE_STRING:
                    shape = new LineString(ja.ToObject<double[][]>());
                    break;
                case Geometry.TYPE_MULTI_LINE_STRING:
                    shape = new MultiLineString(ja.ToObject<double[][][]>());
                    break;
                case Geometry.TYPE_POLYGON:
                    shape = new Polygon(ja.ToObject<double[][][]>());
                    break;
                case Geometry.TYPE_MULTI_POLYGON:
                    shape = new MultiPolygon(ja.ToObject<double[][][][]>());
                    break;

                default:
                    break;
            }
            
            return shape;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Geometry);
        }
    }
}
