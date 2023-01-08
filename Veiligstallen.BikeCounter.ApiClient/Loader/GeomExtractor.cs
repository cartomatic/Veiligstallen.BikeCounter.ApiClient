using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Geometry = Veiligstallen.BikeCounter.ApiClient.DataModel.Geometry;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal static class GeomExtractor
    {
        /// <summary>
        /// Extracts shp geom as Veiligstallen.BikeCounter geom
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Geometry ExtractGeometry(NetTopologySuite.Geometries.Geometry g)
        {
            return g.GeometryType switch
            {
                nameof(NetTopologySuite.Geometries.MultiPolygon) => ExtractMultiPolygonGeometry(g),
                nameof(NetTopologySuite.Geometries.Polygon) => ExtractPolygonGeometry(g),
                _ => throw new NotImplementedException($"{g.GeometryType} is not supported at this time")
            };
        }

        /// <summary>
        /// Extracts multipolygon as Veiligstallen.BikeCounter geom
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private static Geometry ExtractMultiPolygonGeometry(NetTopologySuite.Geometries.Geometry g)
        {
            if (g is not NetTopologySuite.Geometries.MultiPolygon multiPoly)
                throw new System.Exception($"Expected {nameof(NetTopologySuite.Geometries.MultiPolygon)}  but got {g.GeometryType}");

            var polys = new List<double[][][]>();

            for (var idx = 0; idx < multiPoly.Count; idx++)
            {
                var poly = (NetTopologySuite.Geometries.Polygon) multiPoly[idx];

                polys.Add(ExtractCoords(poly));
            }

            return new DataModel.MultiPolygon(polys.ToArray());
        }

        /// <summary>
        /// Extracts polygon as Veiligstallen.BikeCounter geom
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private static Geometry ExtractPolygonGeometry(NetTopologySuite.Geometries.Geometry g)
        {
            if (g is not NetTopologySuite.Geometries.Polygon poly)
                throw new System.Exception($"Expected {nameof(NetTopologySuite.Geometries.Polygon)} but got {g.GeometryType}");

            return new DataModel.Polygon(ExtractCoords(poly));
        }

        /// <summary>
        /// Extracts coords from polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        private static double[][][] ExtractCoords(NetTopologySuite.Geometries.Polygon poly)
        {
            var rings = new List<double[][]>();

            rings.Add(ExtractCoords(poly.ExteriorRing));
            rings.AddRange(poly.InteriorRings.Select(ExtractCoords));

            return rings.ToArray();
        }

        /// <summary>
        /// Extracts coords from a line string
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        private static double[][] ExtractCoords(NetTopologySuite.Geometries.LineString ls)
            => ls.Coordinates.Select(c => new[] {c.X, c.Y}).ToArray();
    }
}
