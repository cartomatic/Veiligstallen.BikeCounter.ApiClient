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
    internal partial class StaticSurveyDataLoader
    {
        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task<List<SurveyArea>> ExtractSurveyAreasFlatInternalAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
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
                    ValidFrom = ParseDate(data[3]),
                    ValidThrough = ParseDate(data[4]),
                    Authority = data[5],
                    XtraInfo = data[6],
                    SurveyAreaType = data[7]
                };

                try
                {
                    var geom = wktReader.Read(data[8]);
                    surveyArea.GeoLocation = ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse survey area geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(surveyArea);

            }

            return output;
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task<List<ParkingLocation>> ExtractParkingLocationsFlatInternalAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
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
                    ValidFrom = ParseDate(data[2]),
                    ValidThrough = ParseDate(data[3]),
                    Authority = data[4],
                    XtraInfo = data[5],
                    Allows = TryParseParkingLocationAllowsType(data[6], out var vehicleType)
                        ? new Vehicle
                        {
                            Type = vehicleType
                        }
                        : null,
                    Features = ParseParkingLocationFeature(data[7], ',')
                };

                try
                {
                    var geom = wktReader.Read(data[8]);
                    parkingLocation.GeoLocation = ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse parking location geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(parkingLocation);

            }

            return output;
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task<List<Section>> ExtractSectionsFlatInternalAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
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
                    ValidFrom = ParseDate(data[3]),
                    ValidThrough = ParseDate(data[4]),
                    Authority = data[5],
                    Level = int.TryParse(data[6], out var parsedLevel)
                        ? parsedLevel
                        : 0,
                    ParkingSpaceOf = TryParseParkingSpaceType(data[7], out var parkingSpaceType)
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
                    section.GeoLocation = ExtractGeometry(geom);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception($"Failed to parse section geometry at line {lineCnt}: {ex.Message}");
                }

                output.Add(section);
            }

            return output;
        }

        private async Task<IEnumerable<string>> ExtractSurveyAreasIdsFlatInternalAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
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
        

        private List<SurveyArea> ExtractSurveyAreasSeparatedInternal(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
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
                        colMap = SeparatedPrepareColMap(line, separator);
                        SeparatedVerifyRequiredFieldsPresence(colMap, _surveyAreasColumns);
                        hdrRead = true;
                    }

                    var data = SeparatedExtractDataRow(colMap, line, separator);

                    var surveyArea = new SurveyArea
                    {
                        LocalId = data[SURVEY_AREA_LOCAL_ID],
                        ParentLocalId = data[SURVEY_AREA_PARENT_LOCAL_ID],
                        Name = data[SURVEY_AREA_NAME],
                        ValidFrom = ParseDate(data[SURVEY_AREA_VALID_FROM]),
                        ValidThrough = ParseDate(data[SURVEY_AREA_VALID_THROUGH]),
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
        private const string PARKING_LOCATION_LOCATION_NUMBER = "location_number";

        private static string[] _parkingLocationColumns =
        {
            PARKING_LOCATION_LOCAL_ID, PARKING_LOCATION_NAME, PARKING_LOCATION_FEATURE_TYPE,
            PARKING_LOCATION_VALID_FROM, PARKING_LOCATION_VALID_THROUGH,
            PARKING_LOCATION_AUTHORITY, PARKING_LOCATION_XTRA_INFO,
            PARKING_LOCATION_SURVEY_AREA_LOCAL_ID, PARKING_LOCATION_LOCATION_NUMBER
        };

        private List<ParkingLocation> ExtractParkingLocationsSeparatedInternal(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
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
                        colMap = SeparatedPrepareColMap(line, separator);
                        SeparatedVerifyRequiredFieldsPresence(colMap, _parkingLocationColumns);
                        hdrRead = true;
                    }

                    var data = SeparatedExtractDataRow(colMap, line, separator);

                    var surveyArea = new ParkingLocation
                    {
                        LocalId = data[PARKING_LOCATION_LOCAL_ID],
                        Name = data[PARKING_LOCATION_NAME],
                        ValidFrom = ParseDate(data[PARKING_LOCATION_VALID_FROM]),
                        ValidThrough = ParseDate(data[PARKING_LOCATION_VALID_THROUGH]),
                        Authority = data[PARKING_LOCATION_AUTHORITY],
                        XtraInfo = data[PARKING_LOCATION_XTRA_INFO],
                        //WTF - no such field in the default format????
                        //Allows = TryParseParkingLocationAllowsType(data[6], out var vehicleType)
                        //    ? new Vehicle
                        //    {
                        //        Type = vehicleType
                        //    }
                        //    : null,
                        Features = ParseParkingLocationFeature(data[PARKING_LOCATION_FEATURE_TYPE], ' ')
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


        private static string[] _sectionColumns =
        {
            SECTION_LOCAL_ID,
            //SECTION_PARKING_LOCATION_ID - WTF??? this should not be here!
            SECTION_PARKING_LOCATION_LOCAL_ID,
            SECTION_NAME, SECTION_LAYOUT, SECTION_PARKING_SYSTEM_TYPE, SECTION_VEHICLE_OWNER_TYPE,
            SECTION_LEVEL, SECTION_VALID_FROM, SECTION_VALID_THROUGH, SECTION_NR
        };

        private List<Section> ExtractSectionsSeparatedInternal(string fName, FlatFileUtils.FlatFileSeparator separator)
        {
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
                        colMap = SeparatedPrepareColMap(line, separator);
                        SeparatedVerifyRequiredFieldsPresence(colMap, _sectionColumns);
                        hdrRead = true;
                    }

                    var data = SeparatedExtractDataRow(colMap, line, separator);

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
                            ValidFrom = ParseDate(data[SECTION_VALID_FROM]),
                            ValidThrough = ParseDate(data[SECTION_VALID_THROUGH]),
                            //Authority = data[5], //WTF???? field missing in the model!!!
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
            if (TryParseParkingSpaceType(parkingSpaceTypeStr, out var parkingSpaceType))
            {
                return new ParkingSpace
                {
                    Type = parkingSpaceType,
                    Vehicles = TryParseVehicleOwnerType(ownerTypeStr, out var ownerType)
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
    }
}
