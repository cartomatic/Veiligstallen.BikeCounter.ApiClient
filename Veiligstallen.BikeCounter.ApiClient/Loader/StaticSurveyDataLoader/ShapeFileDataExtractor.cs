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
    internal partial class ShapeFileDataExtractor : IDisposable
    {
        private const string COMPLETE_DATA_SHP_SURVEYAREA_COL_SURVEYAREAID = "surveyarea";
        private const string COMPLETE_DATA_SHP_PARKINGLOCATION_COL_NAME = "Locatie";
        private const string COMPLETE_DATA_SHP_SECTION_COL_NAME = "localid";

        private readonly bool _extractWkt;
        public ShapeFileDataExtractor(bool extractWkt)
        {
            _extractWkt = extractWkt;
        }

        /// <summary>
        /// Extracts survey area geometries from a complete data shape file and updates passed survey areas
        /// </summary>
        /// <param name="surveyAreas"></param>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public void ExtractCompleteDataSurveyAreasGeoms(string fName, List<SurveyArea> surveyAreas)
        {
            using var shpReader = PrepareShapeFileReader(fName, COMPLETE_DATA_SHP_SURVEYAREA_COL_SURVEYAREAID, out var idColIdx);
            
            while (shpReader.Read())
            {
                var surveyAreaLocalId = shpReader.GetString(idColIdx);
                var surveyArea = surveyAreas.FirstOrDefault(sa => sa.LocalId == surveyAreaLocalId);
                if(surveyArea == null)
                    continue;

                if(_extractWkt)
                    surveyArea.GeomWkt = shpReader.Geometry.ToText();
                
                surveyArea.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);
            }
        }

        /// <summary>
        /// Extracts parking location geometries from a complete data shape file and updates passed parking locations
        /// </summary>
        /// <param name="parkingLocations"></param>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public void ExtractCompleteDataParkingLocationsGeoms(string fName, List<ParkingLocation> parkingLocations)
        {
            using var shpReader = PrepareShapeFileReader(fName, COMPLETE_DATA_SHP_PARKINGLOCATION_COL_NAME, out var idColIdx);

            while (shpReader.Read())
            {
                var parkingLocationName = shpReader.GetInt32(idColIdx);
                var parkingLocation = parkingLocations.FirstOrDefault(pl => int.Parse(pl.Name) == parkingLocationName);
                if (parkingLocation == null)
                    continue;

                if (_extractWkt)
                    parkingLocation.GeomWkt = shpReader.Geometry.ToText();

                parkingLocation.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);
            }
        }

        /// <summary>
        /// Extracts section geometries from a complete data shape file and updates passed sections
        /// </summary>
        /// <param name="sections"></param>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public void ExtractCompleteDataSectionsGeoms(string fName, List<Section> sections)
        {
            using var shpReader = PrepareShapeFileReader(fName, COMPLETE_DATA_SHP_SECTION_COL_NAME, out var idColIdx);

            while (shpReader.Read())
            {
                var sectionLocalId = shpReader.GetString(idColIdx);
                var section = sections.FirstOrDefault(s => s.LocalId == sectionLocalId);
                if (section == null)
                    continue;

                if (_extractWkt)
                    section.GeomWkt = shpReader.Geometry.ToText();

                section.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);
            }
        }


        private const string SHP_ONLY_LOCAL_ID = "LocId";
        private const string SHP_ONLY_NAME = "Name";
        private const string SHP_ONLY_FROM = "From";
        private const string SHP_ONLY_THROUGH = "Through";
        private const string SHP_ONLY_AUTHORITY = "Authority";
        private const string SHP_ONLY_EXTRA_INFO = "ExtraInfo";

        private const string SHP_ONLY_PARENT_LOCAL_ID = "PrntLocId";
        private const string SHP_ONLY_SURVEY_AREA_TYPE = "AreaType";

        private static string[] _surveyAreaShpOnlyRequiredColumns =
        {
            SHP_ONLY_LOCAL_ID, SHP_ONLY_PARENT_LOCAL_ID,
            SHP_ONLY_NAME, SHP_ONLY_FROM, SHP_ONLY_THROUGH, SHP_ONLY_AUTHORITY, SHP_ONLY_EXTRA_INFO,
            SHP_ONLY_SURVEY_AREA_TYPE
        };

        /// <summary>
        /// Extracts survey areas from a self contained shp file
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public List<SurveyArea> ExtractSurveyAreasShpOnlyAsync(string shpFile)
        {
            var output = new List<SurveyArea>();

            using var shpReader = PrepareShapeFileReader(shpFile, _surveyAreaShpOnlyRequiredColumns, out var colMap);

            while (shpReader.Read())
            {
                var surveyArea = new SurveyArea
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_ONLY_LOCAL_ID]),
                    ParentLocalId = ExtractString(shpReader, colMap[SHP_ONLY_PARENT_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_ONLY_NAME]),
                    ValidFrom = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_FROM])),
                    ValidThrough = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_ONLY_AUTHORITY]),
                    XtraInfo = ExtractString(shpReader, colMap[SHP_ONLY_EXTRA_INFO]),
                    SurveyAreaType = ExtractString(shpReader, colMap[SHP_ONLY_SURVEY_AREA_TYPE])
                };

                if (_extractWkt)
                    surveyArea.GeomWkt = shpReader.Geometry.ToText();

                surveyArea.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);
                
                output.Add(surveyArea);
            }

            return output;
        }

        private const string SHP_ONLY_ALLOWS_TYPE = "AllowsType";
        private const string SHP_ONLY_FEATURES = "Features";

        private static string[] _parkingLocationsShpOnlyRequiredColumns =
        {
            SHP_ONLY_LOCAL_ID, 
            SHP_ONLY_NAME, SHP_ONLY_FROM, SHP_ONLY_THROUGH, SHP_ONLY_AUTHORITY, SHP_ONLY_EXTRA_INFO,
            SHP_ONLY_ALLOWS_TYPE, SHP_ONLY_FEATURES
        };

        private const string DEFAULT_FORMAT_SHP_ID = "local_id";

        private static string[] _defaultFormatShpRequiredColumns =
        {
            DEFAULT_FORMAT_SHP_ID
        };


        /// <summary>
        /// Extracts surveys from a default format - shp + flat
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        public List<SurveyArea> ExtractSurveyAreasShp(string shpFile)
        {
            var output = new List<SurveyArea>();

            using var shpReader = PrepareShapeFileReader(shpFile, _defaultFormatShpRequiredColumns, out var colMap);
            while (shpReader.Read())
            {
                var obj = new SurveyArea
                {
                    LocalId = ExtractString(shpReader, colMap[DEFAULT_FORMAT_SHP_ID])
                };

                //it looks like there may be some null geoms present in a shape file - how the heck it is possible???
                //perhaps there is a rec in the dbf, but no actual geom

                //only need objects with geometries!
                if (!shpReader.Geometry.IsEmpty)
                {
                    if (_extractWkt)
                        obj.GeomWkt = shpReader.Geometry.ToText();

                    obj.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);

                    output.Add(obj);
                }
            }
            
            return output;
        }


        /// <summary>
        /// Extracts parking locations from a self contained shape file
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public List<ParkingLocation> ExtractParkingLocationsShpOnlyAsync(string shpFile)
        {
            var output = new List<ParkingLocation>();

            using var shpReader = PrepareShapeFileReader(shpFile, _parkingLocationsShpOnlyRequiredColumns, out var colMap);

            while (shpReader.Read())
            {
                var parkingLocation = new ParkingLocation
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_ONLY_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_ONLY_NAME]),
                    ValidFrom = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_FROM])),
                    ValidThrough = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_ONLY_AUTHORITY]),
                    XtraInfo = ExtractString(shpReader, colMap[SHP_ONLY_EXTRA_INFO]),
                    Allows = Parsers.TryParseParkingLocationAllowsType(ExtractString(shpReader, colMap[SHP_ONLY_ALLOWS_TYPE]), out var vehicleType)
                        ? new []
                        {
                            new Vehicle
                            {
                                Type = vehicleType
                            }
                        }
                        : null,
                    Features = Parsers.ParseParkingLocationFeature(ExtractString(shpReader, colMap[SHP_ONLY_FEATURES]), ',')
                };

                if (_extractWkt)
                    parkingLocation.GeomWkt = shpReader.Geometry.ToText();

                parkingLocation.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);

                output.Add(parkingLocation);
            }

            return output;
        }

        private const string SHP_PARKING_LOCATION_LOCAL_ID = "PLcnLocId";
        private const string SHP_LEVEL = "Level";
        private const string SHP_PARKING_SYSTEM_TYPE = "SystemType";

        private static string[] _sectionRequiredShpColumns =
        {
            SHP_ONLY_LOCAL_ID, SHP_PARKING_LOCATION_LOCAL_ID,
            SHP_ONLY_NAME, SHP_ONLY_FROM, SHP_ONLY_THROUGH, SHP_ONLY_AUTHORITY,
            SHP_LEVEL,
            SHP_PARKING_SYSTEM_TYPE
        };

        /// <summary>
        /// Extracts parking locations from a default format - shp + flat
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        public List<ParkingLocation> ExtractParkingLocationsShp(string shpFile)
        {
            var output = new List<ParkingLocation>();

            using var shpReader = PrepareShapeFileReader(shpFile, _defaultFormatShpRequiredColumns, out var colMap);
            while (shpReader.Read())
            {
                var obj = new ParkingLocation
                {
                    LocalId = ExtractString(shpReader, colMap[DEFAULT_FORMAT_SHP_ID])
                };

                //it looks like there may be some null geoms present in a shape file - how the heck it is possible???
                //perhaps there is a rec in the dbf, but no actual geom

                //only need objects with geometries!
                if (!shpReader.Geometry.IsEmpty)
                {
                    if (_extractWkt)
                        obj.GeomWkt = shpReader.Geometry.ToText();

                    obj.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);

                    output.Add(obj);
                }

            }

            return output;
        }

        /// <summary>
        /// Extracts sections from a self contained shape file
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public List<Section> ExtractSectionsShpOnlyAsync(string shpFile)
        {
            var output = new List<Section>();

            using var shpReader = PrepareShapeFileReader(shpFile, _sectionRequiredShpColumns, out var colMap);

            while (shpReader.Read())
            {
                var section = new Section
                {
                    LocalId = ExtractString(shpReader, colMap[SHP_ONLY_LOCAL_ID]),
                    ParkingLocationLocalId = ExtractString(shpReader, colMap[SHP_PARKING_LOCATION_LOCAL_ID]),
                    Name = ExtractString(shpReader, colMap[SHP_ONLY_NAME]),
                    ValidFrom = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_FROM])),
                    ValidThrough = ParseDateTimeShpOnly(ExtractString(shpReader, colMap[SHP_ONLY_THROUGH])),
                    Authority = ExtractString(shpReader, colMap[SHP_ONLY_AUTHORITY]),
                    Level = shpReader.GetInt32(colMap[SHP_LEVEL]),
                    ParkingSpaceOf = Parsers.TryParseParkingSpaceType(ExtractString(shpReader, colMap[SHP_PARKING_SYSTEM_TYPE]),
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

                section.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);

                output.Add(section);
            }

            return output;
        }

        /// <summary>
        /// Extracts sections from a default format - shp + flat
        /// </summary>
        /// <param name="shpFile"></param>
        /// <returns></returns>
        public List<Section> ExtractSectionsShp(string shpFile)
        {
            var output = new List<Section>();

            using var shpReader = PrepareShapeFileReader(shpFile, _defaultFormatShpRequiredColumns, out var colMap);
            while (shpReader.Read())
            {
                var obj = new Section
                {
                    LocalId = ExtractString(shpReader, colMap[DEFAULT_FORMAT_SHP_ID])
                };

                //it looks like there may be some null geoms present in a shape file - how the heck it is possible???
                //perhaps there is a rec in the dbf, but no actual geom

                //only need objects with geometries!
                if (!shpReader.Geometry.IsEmpty)
                {
                    if (_extractWkt)
                        obj.GeomWkt = shpReader.Geometry.ToText();

                    obj.GeoLocation = GeomExtractor.ExtractGeometry(shpReader.Geometry);

                    output.Add(obj);
                }
            }

            return output;
        }

        private DateTime? ParseDateTimeShpOnly(string wouldBeDateTime)
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
        private ShapefileDataReader PrepareShapeFileReader(string fName, string idColName, out int idColIdx)
        {
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

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
