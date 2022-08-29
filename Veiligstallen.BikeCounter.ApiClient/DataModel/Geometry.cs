using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.EC.Multiplier;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public abstract class Geometry
    {
        public const string TYPE_POINT = "Point";
        public const string TYPE_MULTI_POINT = "MultiPoint";
        public const string TYPE_LINE_STRING = "LineString";
        public const string TYPE_MULTI_LINE_STRING = "MultiLineString";
        public const string TYPE_POLYGON = "Polygon";
        public const string TYPE_MULTI_POLYGON = "MultiPolygon";

        [JsonProperty("type")]
        public abstract string Type { get; }
    }

    public class Point : Geometry
    {
        public Point(double[] coordinates)
        {
            Coordinates = coordinates;
        }
        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }

        /// <inheritdoc />
        [JsonProperty("type")]
        public override string Type => TYPE_POINT;
    }
    public class MultiPoint : Geometry
    {
        public MultiPoint(double[][] coordinates)
        {
            Coordinates = coordinates;
        }

        [JsonProperty("coordinates")]
        public double[][] Coordinates { get; set; }

        /// <inheritdoc />
        [JsonProperty("type")]
        public override string Type => TYPE_MULTI_POINT;
    }

    public class LineString : Geometry
    {
        public LineString(double[][] coordinates)
        {
            Coordinates = coordinates;
        }

        [JsonProperty("coordinates")]
        public double[][] Coordinates { get; set; }

        /// <inheritdoc />
        [JsonProperty("type")]
        public override string Type => TYPE_LINE_STRING;
    }

    public class MultiLineString : Geometry
    {
        public MultiLineString(double[][][] coordinates)
        {
            Coordinates = coordinates;
        }

        [JsonProperty("coordinates")]
        public double[][][] Coordinates { get; set; }
        
        /// <inheritdoc />
        [JsonProperty("type")] 
        public override string Type => TYPE_MULTI_LINE_STRING;
    }

    public class Polygon : Geometry
    {
        public Polygon(double[][][] coordinates)
        {
            Coordinates = coordinates;
        }

        [JsonProperty("coordinates")]
        public double[][][] Coordinates { get; set; }
        /// <inheritdoc />

        [JsonProperty("type")] 
        public override string Type => TYPE_POLYGON;
    }

    public class MultiPolygon : Geometry
    {
        public MultiPolygon(double[][][][] coordinates)
        {
            Coordinates = coordinates;
        }

        [JsonProperty("coordinates")]
        public double[][][][] Coordinates { get; set; }
        /// <inheritdoc />
        [JsonProperty("type")]
        public override string Type => TYPE_MULTI_POLYGON;
    }
}
