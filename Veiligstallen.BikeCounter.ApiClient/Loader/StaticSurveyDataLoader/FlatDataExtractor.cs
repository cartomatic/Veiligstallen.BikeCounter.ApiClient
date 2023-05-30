using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class FlatDataExtractor : IDisposable
    {

        /// <summary>
        /// Extracts complete survey areas from a flat file
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task<List<SurveyArea>> ExtractSurveyAreasFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<SurveyArea>();

            var wktReader = new WKTReader(GeometryFactory.Default);

            var hdrRead = false;
            var fldCount = 9;
            var lineCnt = 0;
            foreach (var line in File.ReadLines(fName))
            {
                lineCnt++;

                if (header && !hdrRead)
                {
                    hdrRead = true;
                    continue;
                }
                    

                var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);
                if (data.Length != fldCount)
                    throw new System.Exception($"Filed to parse flat survey area data at line {lineCnt}; expected ${fldCount}, found {data.Length}");

                var surveyArea = new SurveyArea
                {
                    LocalId = data[0],
                    ParentLocalId = data[1],
                    Name = data[2],
                    ValidFrom = Parsers.ParseDate(data[3]),
                    ValidThrough = Parsers.ParseDate(data[4]),
                    Authority = data[5],
                    XtraInfo = data[6],
                    SurveyAreaType = data[7]
                };

                try
                {
                    var geom = wktReader.Read(data[8]);
                    surveyArea.GeoLocation = GeomExtractor.ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse survey area geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(surveyArea);

            }

            return output;
        }

        /// <summary>
        /// Extracts complete parking locations from a flat file
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task<List<ParkingLocation>> ExtractParkingLocationsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<ParkingLocation>();

            var wktReader = new WKTReader(GeometryFactory.Default);

            var hdrRead = false;
            var fldCount = 9;
            var lineCnt = 0;
            foreach (var line in File.ReadLines(fName))
            {
                lineCnt++;

                if (header && !hdrRead)
                {
                    hdrRead = true;
                    continue;
                }

                var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);
                if (data.Length != fldCount)
                    throw new System.Exception($"Filed to parse flat parking location data at line {lineCnt}; expected ${fldCount}, found {data.Length}");


                var parkingLocation = new ParkingLocation()
                {
                    LocalId = data[0],
                    Name = data[1],
                    ValidFrom = Parsers.ParseDate(data[2]),
                    ValidThrough = Parsers.ParseDate(data[3]),
                    Authority = data[4],
                    XtraInfo = data[5],
                    Allows = Parsers.TryParseParkingLocationAllowsType(data[6], out var vehicleType)
                        ? new Vehicle
                        {
                            Type = vehicleType
                        }
                        : null,
                    Features = Parsers.ParseParkingLocationFeature(data[7], ',')
                };

                try
                {
                    var geom = wktReader.Read(data[8]);
                    parkingLocation.GeoLocation = GeomExtractor.ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse parking location geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(parkingLocation);

            }

            return output;
        }

        /// <summary>
        /// Extracts complete sections from a flat file
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task<List<Section>> ExtractSectionsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<Section>();

            var wktReader = new WKTReader(GeometryFactory.Default);

            var hdrRead = false;
            var fldCount = 9;
            var lineCnt = 0;
            foreach (var line in File.ReadLines(fName))
            {
                lineCnt++;

                if (header && !hdrRead)
                {
                    hdrRead = true;
                    continue;
                }

                var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);
                if (data.Length != fldCount)
                    throw new System.Exception($"Filed to parse flat section data at line {lineCnt}; expected ${fldCount}, found {data.Length}");

                var section = new Section
                {
                    LocalId = data[0],
                    ParkingLocationLocalId = data[1],
                    Name = data[2],
                    ValidFrom = Parsers.ParseDate(data[3]),
                    ValidThrough = Parsers.ParseDate(data[4]),
                    Authority = data[5],
                    Level = int.TryParse(data[6], out var parsedLevel)
                        ? parsedLevel
                        : 0,
                    ParkingSpaceOf = Parsers.TryParseParkingSpaceType(data[7], out var parkingSpaceType)
                        ? new[]
                        {
                            new ParkingSpace
                            {
                                Type = parkingSpaceType
                            }
                        }
                        : null
                };

                try
                {
                    var geom = wktReader.Read(data[8]);
                    section.GeoLocation = GeomExtractor.ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse section geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(section);
            }

            return output;
        }

        /// <summary>
        /// Extracts observations
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public async Task<List<Observation>> ExtractObservationsAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            //Notes:
            //there are 2 types of observations - each depends on the actual data!!!
            //For the time being assuming the column order of the input data MUST always be the same
            //vehicle counts start at column Y

            //input fields columns - indexes and excel addressation
            //
            // 0 A "section_id",
            // 1 B "section_localId",
            // 2 C "parkinglocation_id",
            // 3 D "parkinglocation_localId",
            // 4 E "section_name",
            // 5 F "section_layout",
            // 6 G "section_url_geolocation",
            // 7 H "section_parkingSystemType",
            // 8 I "section_vehicleOwnerType",
            // 9 J "section_level",
            //10 K "section_validFrom",
            //11 L "section_validThrough"
            //
            //12 M "survey_id",
            //13 N "contractor",
            //14 O "observation_capacity_id",
            //15 P "observation_capacity_timestamp_start",
            //16 Q "observation_capacity_timestamp_end",
            //17 R "observation_capacity_parkingCapacity",
            //18 S "observation_capacity_note",
            //19 T "observation_occupation_id",
            //20 U "observation_occupation_timestamp_start",
            //21 V "observation_occupation_timestamp_end",
            //22 W "occupation_totalParked",
            //23 X "observation_occupation_note"
            //
            //24 Y - here start vehicle counts! header needs to have a value!


            var output = new List<Observation>();

            var hdrRead = false;
            var fldCount = 9;
            var lineCnt = 0;
            var vehicleColMap = new Dictionary<int, string>();
            foreach (var line in File.ReadLines(fName))
            {
                lineCnt++;

                if (header && !hdrRead)
                {
                    hdrRead = true;
                    
                    var hdrData = line.Split(FlatFileUtils.FlatFileSeparators[separator]);

                    fldCount = hdrData.Length;

                    if (hdrData.Length > 24)
                    {
                        for (var i = 24; i < hdrData.Length; i++)
                        {
                            vehicleColMap.Add(i, hdrData[i]);
                        }
                    }

                    continue;
                }
                
                var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);
                if (data.Length != fldCount)
                    throw new System.Exception($"Filed to parse flat section data at line {lineCnt}; expected field count to be ${fldCount}, but found {data.Length}");

                var observationCapacity = new Observation
                {
                    Survey = data[12],
                    ObservedProperty = "capacity",
                    FeatureOfInterest = data[0], //this is supposed to be section id
                    SectionLocalId = data[1],
                    TimestampStart = Parsers.ParseDate(data[15]),
                    TimestampEnd = Parsers.ParseDate(data[16]),
                    Note = string.IsNullOrWhiteSpace(data[23]) ? null : new Note { Remark = data[18] },
                    Measurement = new Measurement
                    {
                        ParkingCapacity = int.TryParse(data[17], out var parkingCapacity) ? parkingCapacity : 0
                    }
                };
                if (observationCapacity.TimestampStart.HasValue)
                    output.Add(observationCapacity);

                var observationOccupation = new Observation
                {
                    Survey = data[12],
                    ObservedProperty = "occupation",
                    FeatureOfInterest = data[0], //this is supposed to be section id
                    SectionLocalId = data[1],
                    TimestampStart = Parsers.ParseDate(data[20]),
                    TimestampEnd = Parsers.ParseDate(data[21]),
                    Note = string.IsNullOrWhiteSpace(data[23]) ? null : new Note { Remark = data[23] },
                    Measurement = new Measurement
                    {
                        TotalParked = int.TryParse(data[22], out var totalParked) ? totalParked : 0,
                        VehicleTypeCounts = vehicleColMap.Select(kv => new VehicleTypeCount
                        {
                            CanonicalVehicleCode = kv.Value,
                            NumberOfVehicles = int.TryParse(data[kv.Key], out var vehicleCount) ? vehicleCount : 0
                        }).ToArray()
                    }
                };
                if (observationOccupation.TimestampStart.HasValue)
                    output.Add(observationOccupation);
            }

            return output;
        }
        
        /// <summary>
        /// Extract survey areas ids
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> ExtractSurveyAreasIdsAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var sectionAreaIds = new List<string>();

            var hdrRead = false;
            var lineCnt = 0;
            foreach (var line in File.ReadLines(fName))
            {
                lineCnt++;

                if (header && !hdrRead)
                {
                    hdrRead = true;
                    continue;
                }

                var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);


                //assuming the first field is id and it should be parseable to UUID... but it's not...
                //3847FD42-66AD-4BFB-A40CD37624CDAA37
                //xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
                //Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
                //if (!Guid.TryParse(data[0], out var parsedGuid))
                //    throw new System.Exception($"Could not parse GUID at line {lineCnt}");
                //sectionAreaIds.Add(parsedGuid);

                sectionAreaIds.Add(data[0]);
            }

            return sectionAreaIds;
        }
        


        private const string SURVEY_AREA_LOCAL_ID = "surveyarea_localId";
        private const string SURVEY_AREA_PARENT_ID = "surveyarea_parentId";
        private const string SURVEY_AREA_PARENT_LOCAL_ID = "surveyarea_parentLocalId";
        private const string SURVEY_AREA_VALID_FROM = "surveyarea_validFrom";
        private const string SURVEY_AREA_VALID_THROUGH = "surveyarea_validThrough";
        private const string SURVEY_AREA_AUTHORITY_ID = "surveyarea_authorityId";
        private const string SURVEY_AREA_NAME = "surveyarea_name";
        private const string SURVEY_AREA_XTRA_INFO = "surveyarea_xtrainfo";
        private const string SURVEY_AREA_TYPE = "surveyarea_surveyAreaType";

        private static string[] _surveyAreasColumns =
        {
            SURVEY_AREA_LOCAL_ID, SURVEY_AREA_PARENT_LOCAL_ID,
            //SURVEY_AREA_PARENT_ID - WTF??? this looks like a mistake!!!!
            SURVEY_AREA_VALID_FROM, SURVEY_AREA_VALID_THROUGH, SURVEY_AREA_AUTHORITY_ID,
            SURVEY_AREA_NAME, SURVEY_AREA_XTRA_INFO, SURVEY_AREA_TYPE
        };
        
        /// <summary>
        /// Extracts survey areas data
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="System.Exception"></exception>
        public List<SurveyArea> ExtractSurveyAreas(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<SurveyArea>();

            var hdrRead = false;
            var lineCnt = 0;
            Dictionary<string, int> colMap = null;
            try
            {
                foreach (var line in File.ReadLines(fName))
                {

                    if (!hdrRead)
                    {
                        colMap = PrepareColMap(line, separator);
                        VerifyRequiredFieldsPresence(colMap, _surveyAreasColumns);
                        hdrRead = true;
                    }

                    var data = ExtractDataRow(colMap, line, separator);

                    var surveyArea = new SurveyArea
                    {
                        LocalId = data[SURVEY_AREA_LOCAL_ID],
                        ParentLocalId = data[SURVEY_AREA_PARENT_LOCAL_ID],
                        Name = data[SURVEY_AREA_NAME],
                        ValidFrom = Parsers.ParseDate(data[SURVEY_AREA_VALID_FROM]),
                        ValidThrough = Parsers.ParseDate(data[SURVEY_AREA_VALID_THROUGH]),
                        Authority = data[SURVEY_AREA_AUTHORITY_ID],
                        XtraInfo = data[SURVEY_AREA_XTRA_INFO],
                        SurveyAreaType = data[SURVEY_AREA_TYPE]
                    };

                    output.Add(surveyArea);
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error reading line: {lineCnt + 1}; {ex.Message}");
            }
            
            return output;
        }

        private const string PARKING_LOCATION_LOCAL_ID = "parkinglocation_localId";
        private const string PARKING_LOCATION_NAME = "parkinglocation_name";
        private const string PARKING_LOCATION_FEATURE_TYPE = "parkinglocation_locationFeatureType";
        private const string PARKING_LOCATION_VALID_FROM = "parkinglocation_validFrom";
        private const string PARKING_LOCATION_VALID_THROUGH = "parkingfacility_validThrough";
        private const string PARKING_LOCATION_AUTHORITY = "parkinglocation_authority";
        private const string PARKING_LOCATION_XTRA_INFO = "parkinglocation_xtrainfo";
        private const string PARKING_LOCATION_SURVEY_AREA_LOCAL_ID = "surveyArea_localId";
        private const string PARKING_LOCATION_NUMBER = "location_number";

        private static string[] _parkingLocationColumns =
        {
            PARKING_LOCATION_LOCAL_ID, PARKING_LOCATION_NAME, PARKING_LOCATION_FEATURE_TYPE,
            PARKING_LOCATION_VALID_FROM, PARKING_LOCATION_VALID_THROUGH,
            PARKING_LOCATION_AUTHORITY, PARKING_LOCATION_XTRA_INFO,
            PARKING_LOCATION_SURVEY_AREA_LOCAL_ID, PARKING_LOCATION_NUMBER
        };

        /// <summary>
        /// Extracts parking locations data
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="System.Exception"></exception>
        public List<ParkingLocation> ExtractParkingLocations(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<ParkingLocation>();

            var hdrRead = false;
            var lineCnt = 0;
            Dictionary<string, int> colMap = null;
            try
            {
                foreach (var line in File.ReadLines(fName))
                {

                    if (!hdrRead)
                    {
                        colMap = PrepareColMap(line, separator);
                        VerifyRequiredFieldsPresence(colMap, _parkingLocationColumns);
                        hdrRead = true;
                    }

                    var data = ExtractDataRow(colMap, line, separator);

                    var parkingLocation = new ParkingLocation
                    {
                        LocalId = data[PARKING_LOCATION_LOCAL_ID],
                        Name = data[PARKING_LOCATION_NAME],
                        ValidFrom = Parsers.ParseDate(data[PARKING_LOCATION_VALID_FROM]),
                        ValidThrough = Parsers.ParseDate(data[PARKING_LOCATION_VALID_THROUGH]),
                        Authority = data[PARKING_LOCATION_AUTHORITY],
                        XtraInfo = data[PARKING_LOCATION_XTRA_INFO],
                        Number = data[PARKING_LOCATION_NUMBER],
                        //no such field in the default format; this is now skipped
                        //Allows = TryParseParkingLocationAllowsType(data[6], out var vehicleType)
                        //    ? new Vehicle
                        //    {
                        //        Type = vehicleType
                        //    }
                        //    : null,
                        Features = Parsers.ParseParkingLocationFeature(data[PARKING_LOCATION_FEATURE_TYPE], ' ')
                    };

                    output.Add(parkingLocation);
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error reading line: {lineCnt + 1}; {ex.Message}");
            }

            return output;
        }


        private const string SECTION_LOCAL_ID = "section_localId";
        private const string SECTION_PARKING_LOCATION_ID = "parkinglocation_id";
        private const string SECTION_PARKING_LOCATION_LOCAL_ID = "parkinglocation_localId";
        private const string SECTION_NAME = "section_name";
        private const string SECTION_LAYOUT = "section_layout";
        private const string SECTION_PARKING_SYSTEM_TYPE = "section_parkingSystemType";
        private const string SECTION_VEHICLE_OWNER_TYPE = "section_vehicleOwnerType";
        private const string SECTION_LEVEL = "section_level";
        private const string SECTION_VALID_FROM = "section_validFrom";
        private const string SECTION_VALID_THROUGH = "section_validThrough";
        private const string SECTION_NR = "section_nr";
        private const string SECTION_AUTHORITY = "section_authority";


        private static string[] _sectionColumns =
        {
            SECTION_LOCAL_ID,
            //SECTION_PARKING_LOCATION_ID - WTF??? this should not be here!
            SECTION_PARKING_LOCATION_LOCAL_ID,
            SECTION_NAME, SECTION_LAYOUT, SECTION_PARKING_SYSTEM_TYPE, SECTION_VEHICLE_OWNER_TYPE,
            SECTION_LEVEL, SECTION_VALID_FROM, SECTION_VALID_THROUGH, SECTION_NR,
            SECTION_AUTHORITY
        };

        /// <summary>
        /// Extracts sections data
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="System.Exception"></exception>
        public List<Section> ExtractSections(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException($"XLSX file format not supported by the {nameof(FlatDataExtractor)}");

            var output = new List<Section>();
            var map = new Dictionary<string, Section>();

            var hdrRead = false;
            var lineCnt = 0;
            Dictionary<string, int> colMap = null;
            try
            {
                foreach (var line in File.ReadLines(fName))
                {

                    if (!hdrRead)
                    {
                        colMap = PrepareColMap(line, separator);
                        VerifyRequiredFieldsPresence(colMap, _sectionColumns);
                        hdrRead = true;
                    }

                    var data = ExtractDataRow(colMap, line, separator);

                    if (map.ContainsKey(data[SECTION_LOCAL_ID]))
                    {
                        var section = map[data[SECTION_LOCAL_ID]];
                        var parkingSpace = CreateParkingSpace(data[SECTION_PARKING_SYSTEM_TYPE],
                            data[SECTION_VEHICLE_OWNER_TYPE]);

                        if (parkingSpace != null)
                        {
                            var list = section.ParkingSpaceOf?.ToList() ?? new List<ParkingSpace>();
                            list.Add(parkingSpace);
                            section.ParkingSpaceOf = list.ToArray();
                        }
                    }
                    else
                    {
                        var section = new Section
                        {
                            LocalId = data[SECTION_LOCAL_ID],
                            ParkingLocationLocalId = data[SECTION_PARKING_LOCATION_LOCAL_ID],
                            Name = data[SECTION_NAME],
                            ValidFrom = Parsers.ParseDate(data[SECTION_VALID_FROM]),
                            ValidThrough = Parsers.ParseDate(data[SECTION_VALID_THROUGH]),
                            Authority = data[SECTION_AUTHORITY],
                            Level = int.TryParse(data[SECTION_LEVEL], out var parsedLevel)
                                ? parsedLevel
                                : 0,
                            ParkingSpaceOf = CreateParkingSpaceArr(data[SECTION_PARKING_SYSTEM_TYPE], data[SECTION_VEHICLE_OWNER_TYPE]),
                            Layout = data[SECTION_LAYOUT]
                        };

                        output.Add(section);
                        map.Add(section.LocalId, section);
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error reading line: {lineCnt + 1}; {ex.Message}");
            }

            return output;
        }

        private ParkingSpace[] CreateParkingSpaceArr(string parkingSpaceTypeStr, string ownerTypeStr)
        {
            var parkingSpace = CreateParkingSpace(parkingSpaceTypeStr, ownerTypeStr);
            return parkingSpace != null ? new[] {parkingSpace} : null;
        }

        private ParkingSpace CreateParkingSpace(string parkingSpaceTypeStr, string ownerTypeStr)
        {
            if (Parsers.TryParseParkingSpaceType(parkingSpaceTypeStr, out var parkingSpaceType))
            {
                return new ParkingSpace
                {
                    Type = parkingSpaceType,
                    Vehicles = Parsers.TryParseVehicleOwnerType(ownerTypeStr, out var ownerType)
                        ? new[]
                        {
                            new Vehicle
                            {
                                Owner = ownerType
                            }
                        }
                        : null
                };
            }

            return null;
        }

        /// <summary>
        /// Prepares a col map based on a header row
        /// </summary>
        /// <param name="header"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private Dictionary<string, int> PrepareColMap(string header, FlatFileUtils.FlatFileSeparator separator)
        {
            var fieldNames = header.Split(FlatFileUtils.FlatFileSeparators[separator]);
            var colMap = new Dictionary<string, int>();

            for (var i = 0; i < fieldNames.Length; i++)
            {
                colMap.Add(fieldNames[i], i);
            }

            return colMap;
        }

        /// <summary>
        /// Verifies required fields presence; throws when a required field could not be found
        /// </summary>
        /// <param name="colMap"></param>
        /// <param name="requiredFieldNames"></param>
        private void VerifyRequiredFieldsPresence(Dictionary<string, int> colMap, IEnumerable<string> requiredFieldNames)
        {
            foreach (var fieldName in requiredFieldNames)
            {
                if (!colMap.ContainsKey(fieldName))
                    throw new System.Exception($"Could nit find a required field: {fieldName}");
            }
        }

        /// <summary>
        /// Reads separated data line and returns it as a dict of col name, col value
        /// </summary>
        /// <param name="colMap"></param>
        /// <param name="line"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private Dictionary<string, string> ExtractDataRow(Dictionary<string, int> colMap, string line, FlatFileUtils.FlatFileSeparator separator)
        {
            var data = line.Split(FlatFileUtils.FlatFileSeparators[separator]);
            if (data.Length != colMap.Count)
                throw new System.Exception($"Invalid field count: expected {colMap.Count}, found: {data.Length}");

            return colMap.ToDictionary(kv => kv.Key, kv => data[kv.Value]);
        }


        //public void ExportFlatFiles(string dir, FlatFileSeparator separator = FlatFileSeparator.Tab)
        //{
        //    if (!Enum.IsDefined(typeof(FlatFileSeparator), separator))
        //        throw new ArgumentException(
        //            $"Invalid separator: {separator}; supported separators are: {string.Join(",", SEPARATORS.Keys.Select(k => $"{k}"))}");

        //    try
        //    {
        //        if (!Directory.Exists(dir))
        //            Directory.CreateDirectory(dir);
        //    }
        //    catch
        //    {
        //        //ignore
        //    }

        //    if (!Directory.Exists(dir))
        //        throw new ArgumentException($"Directory does not exist or could not create it: {dir}");

        //    if (!_dataExtracted)
        //        throw new InvalidOperationException("No data has been extracted so far");

        //    ExportSurveyAreas(dir, separator);
        //    ExportParkingLocations(dir, separator);
        //    ExportSections(dir, separator);
        //}

        //private void ExportSurveyAreas(string dir, FlatFileSeparator separator)
        //{
        //    var fName = GetFlatFilePath(dir, FLAT_FILE_SURVEY_AREAS, separator);

        //    //hdr first
        //    DumpLine(fName, separator, new[]
        //    {
        //        nameof(SurveyArea.LocalId),
        //        nameof(SurveyArea.ParentLocalId),
        //        nameof(SurveyArea.Name),
        //        nameof(SurveyArea.ValidFrom),
        //        nameof(SurveyArea.ValidThrough),
        //        nameof(SurveyArea.Authority),
        //        nameof(SurveyArea.XtraInfo),
        //        nameof(SurveyArea.SurveyAreaType),
        //        nameof(SurveyArea.GeomWkt)

        //    });

        //    foreach (var surveyArea in _surveyAreas)
        //    {
        //        DumpLine(fName, separator, new[]
        //        {
        //            surveyArea.LocalId,
        //            surveyArea.ParentLocalId,
        //            surveyArea.Name,
        //            SerializeDate(surveyArea.ValidFrom),
        //            SerializeDate(surveyArea.ValidThrough),
        //            surveyArea.Authority,
        //            surveyArea.XtraInfo,
        //            surveyArea.SurveyAreaType,
        //            surveyArea.GeomWkt
        //        });
        //    }
        //}

        //private void ExportParkingLocations(string dir, FlatFileSeparator separator)
        //{
        //    var fName = GetFlatFilePath(dir, FLAT_FILE_PARKING_LOCATIONS, separator);

        //    //hdr first
        //    DumpLine(fName, separator, new[]
        //    {
        //        nameof(ParkingLocation.LocalId),
        //        nameof(ParkingLocation.Name),
        //        nameof(ParkingLocation.ValidFrom),
        //        nameof(ParkingLocation.ValidThrough),
        //        nameof(ParkingLocation.Authority),
        //        nameof(ParkingLocation.XtraInfo),
        //        $"{nameof(ParkingLocation.Allows)}_{nameof(ParkingLocation.Allows.Type)}",
        //        nameof(ParkingLocation.Features),
        //        nameof(ParkingLocation.GeomWkt)
        //    });

        //    foreach (var parkingLocation in _parkingLocations)
        //    {
        //        DumpLine(fName, separator, new[]
        //        {
        //            parkingLocation.LocalId,
        //            parkingLocation.Name,
        //            SerializeDate(parkingLocation.ValidFrom),
        //            SerializeDate(parkingLocation.ValidThrough),
        //            parkingLocation.Authority,
        //            parkingLocation.XtraInfo,
        //            SerializeParkingLocationAllowsType(parkingLocation.Allows?.Type),
        //            SerializeParkingLocationFeature(parkingLocation.Features),
        //            parkingLocation.GeomWkt
        //        });
        //    }
        //}

        //private void ExportSections(string dir, FlatFileSeparator separator)
        //{
        //    var fName = GetFlatFilePath(dir, FLAT_FILE_SECTIONS, separator);

        //    //hdr first
        //    DumpLine(fName, separator, new[]
        //    {
        //        nameof(Section.LocalId),
        //        nameof(Section.ParkingLocationLocalId),
        //        nameof(Section.Name),
        //        nameof(Section.ValidFrom),
        //        nameof(Section.ValidThrough),
        //        nameof(Section.Authority),
        //        nameof(Section.Level),
        //        nameof(Section.ParkingSystemType),
        //        nameof(Section.GeomWkt)
        //    });

        //    foreach (var section in _sections)
        //    {
        //        DumpLine(fName, separator, new[]
        //        {
        //            section.LocalId,
        //            section.ParkingLocationLocalId,
        //            section.Name,
        //            SerializeDate(section.ValidFrom),
        //            SerializeDate(section.ValidThrough),
        //            section.Authority,
        //            section.Level.ToString(),
        //            section.ParkingSystemType,
        //            section.GeomWkt
        //        });
        //    }
        //}
        

        private void DumpLine(string fName, FlatFileUtils.FlatFileSeparator separator, string[] data)
        {
            File.AppendAllLines(fName, new[]
            {
                string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), data)
            });
        }

        private string GetFilePath(string dir, string fName, FlatFileUtils.FlatFileSeparator separator)
        {
            var outFName = Path.Combine(dir, $"{fName}.{FlatFileUtils.FlatFileExtensions[separator]}");
            if (File.Exists(outFName))
                File.Delete(outFName);

            return outFName;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
