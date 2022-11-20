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
    internal partial class StaticSurveyDataLoader
    {
        private const string SHP_SURVEYAREA_COL_SURVEYAREAID = "surveyarea";
        private const string SHP_PARKINGLOCATION_COL_NAME = "Locatie";
        private const string SHP_SECTION_COL_NAME = "localid";

        /// <summary>
        /// Extracts survey area geometries from a shapefile and updates survey areas
        /// </summary>
        /// <param name="surveyAreas"></param>
        private void ExtractSurveyAreasGeoms(List<SurveyArea> surveyAreas)
        {
            using var shpReader = PrepareShapeFileReader(FILENAME_SURVEY_AREAS, SHP_SURVEYAREA_COL_SURVEYAREAID, out var idColIdx);
            
            while (shpReader.Read())
            {
                var surveyAreaLocalId = shpReader.GetString(idColIdx);
                var surveyArea = surveyAreas.FirstOrDefault(sa => sa.LocalId == surveyAreaLocalId);
                if(surveyArea == null)
                    continue;

                if(_extractWkt)
                    surveyArea.GeomWkt = shpReader.Geometry.ToText();
                
                surveyArea.GeoLocation = ExtractGeometry(shpReader.Geometry);
            }
        }
        
        /// <summary>
        /// Extracts parking location geometries from a shapefile and updates parking locations
        /// </summary>
        /// <param name="parkingLocations"></param>
        private void ExtractParkingLocationsGeoms(List<ParkingLocation> parkingLocations)
        {
            using var shpReader = PrepareShapeFileReader(FILENAME_PARKING_LOCATIONS, SHP_PARKINGLOCATION_COL_NAME, out var idColIdx);

            while (shpReader.Read())
            {
                var parkingLocationName = shpReader.GetInt32(idColIdx);
                var parkingLocation = parkingLocations.FirstOrDefault(pl => int.Parse(pl.Name) == parkingLocationName);
                if (parkingLocation == null)
                    continue;

                if (_extractWkt)
                    parkingLocation.GeomWkt = shpReader.Geometry.ToText();

                parkingLocation.GeoLocation = ExtractGeometry(shpReader.Geometry);
            }
        }

        /// <summary>
        /// Extracts section geometries from a shapefile and updates sections
        /// </summary>
        /// <param name="sections"></param>
        private void ExtractSectionsGeoms(List<Section> sections)
        {
            using var shpReader = PrepareShapeFileReader(FILENAME_SECTIONS, SHP_SECTION_COL_NAME, out var idColIdx);

            while (shpReader.Read())
            {
                var sectionLocalId = shpReader.GetString(idColIdx);
                var section = sections.FirstOrDefault(s => s.LocalId == sectionLocalId);
                if (section == null)
                    continue;

                if (_extractWkt)
                    section.GeomWkt = shpReader.Geometry.ToText();

                section.GeoLocation = ExtractGeometry(shpReader.Geometry);
            }
        }


        private const string SHP_LOCAL_ID = "LocId";
        private const string SHP_NAME = "Name";
        private const string SHP_FROM = "From";
        private const string SHP_THROUGH = "Through";
        private const string SHP_AUTHORITY = "Authority";
        private const string SHP_EXTRA_INFO = "ExtraInfo";

        private const string SHP_PARENT_LOCAL_ID = "PrntLocId";
        private const string SHP_SURVEY_AREA_TYPE = "AreaType";

        private static string[] _surveyAreaRequiredShpColumns =
        {
            SHP_LOCAL_ID, SHP_PARENT_LOCAL_ID,
            SHP_NAME, SHP_FROM, SHP_THROUGH, SHP_AUTHORITY, SHP_EXTRA_INFO,
            SHP_SURVEY_AREA_TYPE
        };

        /// <summary>
        /// Extracts survey areas from a shp file
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        private List<SurveyArea> ExtractSurveyAreasShpInternalAsync(string shpFile)
        {
            var output = new List<SurveyArea>();

            using var shpReader = PrepareShapeFileReader(shpFile, _surveyAreaRequiredShpColumns, out var colMap);

            while (shpReader.Read())
            {
                var surveyArea = new SurveyArea
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_LOCAL_ID]),
                    ParentLocalId = ExtractString(shpReader, colMap[SHP_PARENT_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_NAME]),
                    ValidFrom = ParseDateTime(ExtractString(shpReader, colMap[SHP_FROM])),
                    ValidThrough = ParseDateTime(ExtractString(shpReader, colMap[SHP_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_AUTHORITY]),
                    XtraInfo = ExtractString(shpReader, colMap[SHP_EXTRA_INFO]),
                    SurveyAreaType = ExtractString(shpReader, colMap[SHP_SURVEY_AREA_TYPE])
                };

                if (_extractWkt)
                    surveyArea.GeomWkt = shpReader.Geometry.ToText();

                surveyArea.GeoLocation = ExtractGeometry(shpReader.Geometry);
                
                output.Add(surveyArea);
            }

            return output;
        }

        private const string SHP_ALLOWS_TYPE = "AllowsType";
        private const string SHP_FEATURES = "Features";

        private static string[] _parkingLocationsRequiredShpColumns =
        {
            SHP_LOCAL_ID, 
            SHP_NAME, SHP_FROM, SHP_THROUGH, SHP_AUTHORITY, SHP_EXTRA_INFO,
            SHP_ALLOWS_TYPE, SHP_FEATURES
        };

        private List<ParkingLocation> ExtractParkingLocationsShpInternalAsync(string shpFile)
        {
            var output = new List<ParkingLocation>();

            using var shpReader = PrepareShapeFileReader(shpFile, _parkingLocationsRequiredShpColumns, out var colMap);

            while (shpReader.Read())
            {
                var parkingLocation = new ParkingLocation
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_NAME]),
                    ValidFrom = ParseDateTime(ExtractString(shpReader, colMap[SHP_FROM])),
                    ValidThrough = ParseDateTime(ExtractString(shpReader, colMap[SHP_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_AUTHORITY]),
                    XtraInfo = ExtractString(shpReader, colMap[SHP_EXTRA_INFO]),
                    Allows = TryParseParkingLocationAllowsType(ExtractString(shpReader, colMap[SHP_ALLOWS_TYPE]), out var vehicleType)
                        ? new Vehicle
                        {
                            Type = vehicleType
                        }
                        : null,
                    Features = ParseParkingLocationFeature(ExtractString(shpReader, colMap[SHP_FEATURES]))
                };

                if (_extractWkt)
                    parkingLocation.GeomWkt = shpReader.Geometry.ToText();

                parkingLocation.GeoLocation = ExtractGeometry(shpReader.Geometry);

                output.Add(parkingLocation);
            }

            return output;
        }

        private const string SHP_PARKING_LOCATION_LOCAL_ID = "PLcnLocId";
        private const string SHP_LEVEL = "Level";
        private const string SHP_PARKING_SYSTEM_TYPE = "SystemType";

        private static string[] _sectionRequiredShpColumns =
        {
            SHP_LOCAL_ID, SHP_PARKING_LOCATION_LOCAL_ID,
            SHP_NAME, SHP_FROM, SHP_THROUGH, SHP_AUTHORITY,
            SHP_LEVEL,
            SHP_PARKING_SYSTEM_TYPE
        };

        private List<Section> ExtractSectionsShpInternalAsync(string shpFile)
        {
            var output = new List<Section>();

            using var shpReader = PrepareShapeFileReader(shpFile, _sectionRequiredShpColumns, out var colMap);

            while (shpReader.Read())
            {
                var section = new Section
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_LOCAL_ID]),
                    ParkingLocationLocalId = ExtractString(shpReader, colMap[SHP_PARKING_LOCATION_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_NAME]),
                    ValidFrom = ParseDateTime(ExtractString(shpReader, colMap[SHP_FROM])),
                    ValidThrough = ParseDateTime(ExtractString(shpReader, colMap[SHP_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_AUTHORITY]),
                    Level = shpReader.GetInt32(colMap[SHP_LEVEL]),
                    ParkingSpaceOf = TryParseParkingSpaceType(ExtractString(shpReader, colMap[SHP_PARKING_SYSTEM_TYPE]),
                        out var parkingSpaceType)
                        ? new[]
                        {
                            new ParkingSpace
                            {
                                Type = parkingSpaceType
                            }
                        }
                        : null
                };

                if (_extractWkt)
                    section.GeomWkt = shpReader.Geometry.ToText();

                section.GeoLocation = ExtractGeometry(shpReader.Geometry);

                output.Add(section);
            }

            return output;
        }

        private DateTime? ParseDateTime(string wouldBeDateTime)
        {
            if(string.IsNullOrWhiteSpace(wouldBeDateTime))
                return null;

            if(DateTime.TryParse(wouldBeDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                return dateTime;

            return null;
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


        private ShapefileDataReader PrepareShapeFileReader(string fileName, string[] requiredColumns, out Dictionary<string, int> colMap)
        {
            colMap = new Dictionary<string, int>();

            var shpReader = new NetTopologySuite.IO.ShapefileDataReader(fileName, new GeometryFactory());

            var dBaseHdr = shpReader.DbaseHeader;
            var dBaseFieldsToRead = dBaseHdr.Fields.Select((fld, idx) => (idx + 1, fld)).ToList(); //geom idx is 0, hence need to bump up

            foreach (var requiredColumn in requiredColumns)
            {
                if (dBaseFieldsToRead.All(f => f.fld.Name != requiredColumn))
                    throw new System.Exception(
                        $"Missing a required column: {requiredColumn} in {fileName}");

                var fld =
                    dBaseFieldsToRead.FirstOrDefault(f => f.fld.Name == requiredColumn);

                colMap.Add(requiredColumn, fld.Item1);
            }
            
            return shpReader;
        }

        private string ExtractString(ShapefileDataReader shpReader, int idx)
        {
            var obj = shpReader.GetValue(idx);
            if (obj != null)
                return (string) obj;

            return null;
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
