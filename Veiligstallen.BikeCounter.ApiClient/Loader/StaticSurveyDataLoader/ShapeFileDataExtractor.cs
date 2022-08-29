using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Geometry = Veiligstallen.BikeCounter.ApiClient.DataModel.Geometry;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    public partial class StaticSurveyDataLoader
    {
        private const string SHP_SURVEYAREA_COL_SURVEYAREAID = "surveyarea";
        private const string SHP_PARKINGLOCATION_COL_NAME = "Locatie";

        /// <summary>
        /// Extracts survey area geometries and updates survey areas
        /// </summary>
        /// <param name="surveyAreas"></param>
        private void ExtractSurveyAreasGeoms(List<SurveyArea> surveyAreas)
        {
            using var shpReader = PrepareShapeFileReader(FILENAME_SURVEY_AREA, SHP_SURVEYAREA_COL_SURVEYAREAID, out var idColIdx);
            
            while (shpReader.Read())
            {
                var surveyAreaLocalId = shpReader.GetString(idColIdx);
                var surveyArea = surveyAreas.FirstOrDefault(sa => sa.LocalId == surveyAreaLocalId);
                if(surveyArea == null)
                    continue;

                surveyArea.GeoLocation = ExtractGeometry(shpReader.Geometry);
            }
        }

        private void ExtractParkingLocationsGeoms(List<ParkingLocation> parkingLocations)
        {
            using var shpReader = PrepareShapeFileReader(FILENAME_PARKING_LOCATION, SHP_PARKINGLOCATION_COL_NAME, out var idColIdx);

            while (shpReader.Read())
            {
                var parkingLocationName = shpReader.GetInt32(idColIdx);
                var parkingLocation = parkingLocations.FirstOrDefault(pl => int.Parse(pl.Name) == parkingLocationName);
                if (parkingLocation == null)
                    continue;

                parkingLocation.GeoLocation = ExtractGeometry(shpReader.Geometry);
            }
        }

        /// <summary>
        /// Prepares an shp reader for given shp file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="idColName"></param>
        /// <param name="idColIdx"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private ShapefileDataReader PrepareShapeFileReader(string fileName, string idColName, out int idColIdx)
        {
            var fName = Path.Combine(_dir, $"{fileName}.shp");
            var shpReader = new NetTopologySuite.IO.ShapefileDataReader(fName, new GeometryFactory());

            var dBaseHdr = shpReader.DbaseHeader;
            var dBaseFieldsToRead = dBaseHdr.Fields.Select((fld, idx) => (idx + 1, fld)).ToList(); //geom idx is 0, hence need to bump up

            if (dBaseFieldsToRead.All(f => f.fld.Name != idColName))
                throw new System.Exception(
                    $"Missing a required column: {idColName} in {fName}");

            var surveyAreaFld =
                dBaseFieldsToRead.FirstOrDefault(f => f.fld.Name == idColName);

            idColIdx = surveyAreaFld.Item1;

            return shpReader;
        }

        /// <summary>
        /// Extracts shp geom as Veiligstallen.BikeCounter geom
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Geometry ExtractGeometry(NetTopologySuite.Geometries.Geometry g)
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
        private Geometry ExtractMultiPolygonGeometry(NetTopologySuite.Geometries.Geometry g)
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
        private Geometry ExtractPolygonGeometry(NetTopologySuite.Geometries.Geometry g)
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
        private double[][][] ExtractCoords(NetTopologySuite.Geometries.Polygon poly)
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
        private double[][] ExtractCoords(NetTopologySuite.Geometries.LineString ls)
            => ls.Coordinates.Select(c => new[] {c.X, c.Y}).ToArray();
    }
}
