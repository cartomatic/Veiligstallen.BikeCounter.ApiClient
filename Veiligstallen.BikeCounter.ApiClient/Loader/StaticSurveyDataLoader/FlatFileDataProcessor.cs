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
        private static string FLAT_FILE_SURVEY_AREAS = "survey_areas";
        private static string FLAT_FILE_PARKING_LOCATIONS = "parking_locations";
        private static string FLAT_FILE_SECTIONS = "sections";

        private static Dictionary<FlatFileSeparator, char> SEPARATORS = new()
        {
            {FlatFileSeparator.Tab, '\t'},
            {FlatFileSeparator.Semicolon, ';'}
        };

        private static Dictionary<FlatFileSeparator, string> EXTENSIONS = new()
        {
            {FlatFileSeparator.Tab, "tsv"},
            {FlatFileSeparator.Semicolon, ";"}
        };


        public void ExportFlatFiles(string dir, FlatFileSeparator separator = FlatFileSeparator.Tab)
        {
            if (!Enum.IsDefined(typeof(FlatFileSeparator), separator))
                throw new ArgumentException(
                    $"Invalid separator: {separator}; supported separators are: {string.Join(",", SEPARATORS.Keys.Select(k => $"{k}"))}");

            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch
            {
                //ignore
            }

            if (!Directory.Exists(dir))
                throw new ArgumentException($"Directory does not exist or could not create it: {dir}");

            if (!_dataExtracted)
                throw new InvalidOperationException("No data has been extracted so far");

            ExportSurveyAreas(dir, separator);
            ExportParkingLocations(dir, separator);
            ExportSections(dir, separator);
        }

        private void ExportSurveyAreas(string dir, FlatFileSeparator separator)
        {
            var fName = GetFlatFilePath(dir, FLAT_FILE_SURVEY_AREAS, separator);

            //hdr first
            DumpLine(fName, separator, new[]
            {
                nameof(SurveyArea.LocalId),
                nameof(SurveyArea.ParentLocalId),
                nameof(SurveyArea.Name),
                nameof(SurveyArea.ValidFrom),
                nameof(SurveyArea.ValidThrough),
                nameof(SurveyArea.Authority),
                nameof(SurveyArea.ExtraInfo),
                nameof(SurveyArea.SurveyAreaType),
                nameof(SurveyArea.GeomWkt)

            });

            foreach (var surveyArea in _surveyAreas)
            {
                DumpLine(fName, separator, new[]
                {
                    surveyArea.LocalId,
                    surveyArea.ParentLocalId,
                    surveyArea.Name,
                    SerializeDate(surveyArea.ValidFrom),
                    SerializeDate(surveyArea.ValidThrough),
                    surveyArea.Authority,
                    surveyArea.ExtraInfo,
                    surveyArea.SurveyAreaType,
                    surveyArea.GeomWkt
                });
            }
        }

        private void ExportParkingLocations(string dir, FlatFileSeparator separator)
        {
            var fName = GetFlatFilePath(dir, FLAT_FILE_PARKING_LOCATIONS, separator);

            //hdr first
            DumpLine(fName, separator, new[]
            {
                nameof(ParkingLocation.LocalId),
                nameof(ParkingLocation.Name),
                nameof(ParkingLocation.ValidFrom),
                nameof(ParkingLocation.ValidThrough),
                nameof(ParkingLocation.Authority),
                nameof(ParkingLocation.XtraInfo),
                $"{nameof(ParkingLocation.Allows)}_{nameof(ParkingLocation.Allows.Type)}",
                nameof(ParkingLocation.Features),
                nameof(ParkingLocation.GeomWkt)
            });

            foreach (var parkingLocation in _parkingLocations)
            {
                DumpLine(fName, separator, new[]
                {
                    parkingLocation.LocalId,
                    parkingLocation.Name,
                    SerializeDate(parkingLocation.ValidFrom),
                    SerializeDate(parkingLocation.ValidThrough),
                    parkingLocation.Authority,
                    parkingLocation.XtraInfo,
                    SerializeParkingLocationAllowsType(parkingLocation.Allows?.Type),
                    SerializeParkingLocationFeature(parkingLocation.Features),
                    parkingLocation.GeomWkt
                });
            }
        }

        private void ExportSections(string dir, FlatFileSeparator separator)
        {
            var fName = GetFlatFilePath(dir, FLAT_FILE_SECTIONS, separator);

            //hdr first
            DumpLine(fName, separator, new[]
            {
                nameof(Section.LocalId),
                nameof(Section.ParkingLocationLocalId),
                nameof(Section.Name),
                nameof(Section.ValidFrom),
                nameof(Section.ValidThrough),
                nameof(Section.Authority),
                nameof(Section.Level),
                nameof(Section.ParkingSystemType),
                nameof(Section.GeomWkt)
            });

            foreach (var section in _sections)
            {
                DumpLine(fName, separator, new[]
                {
                    section.LocalId,
                    section.ParkingLocationLocalId,
                    section.Name,
                    SerializeDate(section.ValidFrom),
                    SerializeDate(section.ValidThrough),
                    section.Authority,
                    section.Level.ToString(),
                    section.ParkingSystemType,
                    section.GeomWkt
                });
            }
        }

        private string SerializeDate(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("O") : string.Empty;

        private DateTime? ParseDate(string d)
        {
            if (!string.IsNullOrWhiteSpace(d) && DateTime.TryParse(d, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var date))
                return date;
            return null;
        }

        private string SerializeParkingLocationFeature(SurveyAreaType[] features)
            => string.Join(",", features?.Select(x => $"{x}") ?? Array.Empty<string>());

        private SurveyAreaType[] ParseParkingLocationFeature(string s)
        {
            var output = new List<SurveyAreaType>();

            foreach (var wouldBeEnumStrValue in s)
            {
                //if (int.TryParse(wouldBeEnumStrValue.ToString(), out var enumIntValue) &&
                //    Enum.IsDefined(typeof(SurveyAreaType), enumIntValue))
                //    output.Add((SurveyAreaType) enumIntValue);

                //case insensitive
                if(Enum.TryParse<SurveyAreaType>(wouldBeEnumStrValue.ToString(), out var enumValue))
                    output.Add(enumValue);
            }

            return output.Any() ? output.ToArray() : null;
        }

        private string SerializeParkingLocationAllowsType(VehicleType? t)
            => t.HasValue ? $"{t}" : string.Empty;

        private bool TryParseParkingLocationAllowsType(string s, out VehicleType vehicleType)
            => Enum.TryParse<VehicleType>(s, out vehicleType);

        private void DumpLine(string fName, FlatFileSeparator separator, string[] data)
        {
            File.AppendAllLines(fName, new[]
            {
                string.Join(SEPARATORS[separator].ToString(), data)
            });
        }

        private string GetFlatFilePath(string dir, string fName, FlatFileSeparator separator)
        {
            var outFName = Path.Combine(dir, $"{fName}.{EXTENSIONS[separator]}");
            if (File.Exists(outFName))
                File.Delete(outFName);

            return outFName;
        }

        private void ResetCollections()
        {
            _surveyAreas?.Clear();
            _parkingLocations?.Clear();
            _sections?.Clear();
        }

        private async Task ExtractSurveyAreasFlatAsync(string fName, FlatFileSeparator separator, bool header)
        {
            //flat files always uploaded 1 by 1, so need to reset collections!
            ResetCollections();

            _surveyAreas = new List<SurveyArea>();

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
                    

                var data = line.Split(SEPARATORS[separator]);
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
                    ExtraInfo = data[6],
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

                _surveyAreas.Add(surveyArea);

            }
        }

        private async Task ExtractParkingLocationsFlatAsync(string fName, FlatFileSeparator separator, bool header)
        {
            //flat files always uploaded 1 by 1, so need to reset collections!
            ResetCollections();

            _parkingLocations = new List<ParkingLocation>();

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

                var data = line.Split(SEPARATORS[separator]);
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
                    Allows = !string.IsNullOrWhiteSpace(data[6]) && TryParseParkingLocationAllowsType(data[6], out var vehicleType)
                        ? new Vehicle
                        {
                            Type = vehicleType
                        }
                        : null,
                    Features = ParseParkingLocationFeature(data[7])
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

                _parkingLocations.Add(parkingLocation);

            }
        }

        private async Task ExtractSectionsFlatAsync(string fName, FlatFileSeparator separator, bool header)
        {
            //flat files always uploaded 1 by 1, so need to reset collections!
            ResetCollections();

            _sections = new List<Section>();

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

                var data = line.Split(SEPARATORS[separator]);
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
                    ParkingSystemType = data[7]
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

                _sections.Add(section);
            }
        }
    }
}
