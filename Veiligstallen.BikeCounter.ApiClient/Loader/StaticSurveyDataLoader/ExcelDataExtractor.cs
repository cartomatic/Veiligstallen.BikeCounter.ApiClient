using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore.Internal;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        private const string EXCEL_SHEET_SURVEY_AREA_STATIC = "SurveyArea";
        private const string EXCEL_SHEET_PARKING_LOCATION_STATIC = "ParkingLocation_static";
        private const string EXCEL_SHEET_SECTION_STATIC = "Section_static";

        private const string SURVEY_AREA_COL_LOCALID = "surveyarea_localId";
        private const string SURVEY_AREA_COL_PARENTLOCALID = "surveyarea_parentLocalId";
        private const string SURVEY_AREA_COL_NAME = "surveyarea_name";
        private const string SURVEY_AREA_COL_VALIDFROM = "surveyarea_validFrom";
        private const string SURVEY_AREA_COL_VALIDTHROUGH = "surveyarea_validThrough";
        private const string SURVEY_AREA_COL_TYPE = "surveyarea_surveyAreaType";
        private const string SURVEY_AREA_COL_XTRAINFO = "surveyarea_xtrainfo";

        private const string PARKING_LOCATION_COL_LOCALID = "parkinglocation_localId";
        private const string PARKING_LOCATION_COL_VALIDFROM = "parkinglocation_validFrom";
        private const string PARKING_LOCATION_COL_VALIDTHROUGH = "parkinglocation_validThrough";
        private const string PARKING_LOCATION_COL_NAME = "parkinglocation_name";
        private const string PARKING_LOCATION_COL_XTRAINFO = "parkinglocation_xtrainfo";
        private const string PARKING_LOCATION_COL_FEATURETYPE = "parkinglocation_locationFeatureType";

        private const string SECTION_COL_LOCALID = "section_localId";
        private const string SECTION_COL_NAME = "section_name";
        private const string SECTION_COL_VALIDFROM = "section_validFrom";
        private const string SECTION_COL_VALIDTHROUGH = "section_validThrough";
        private const string SECTION_COL_LEVEL = "section_level";
        private const string SECTION_COL_PARKINGSYSTEMTYPE = "section_parkingSystemType";
        private const string SECTION_COL_PARKINGLOCATIONLOCALID = "parkinglocation_localId";

        private const string AUTHORITY = "prorail";

        private DataSet _completeDataExcelDataSet;

        /// <summary>
        /// Loads an validates excel data
        /// </summary>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private void LoadCompleteDataExcelData()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var fs = File.OpenRead(Path.Combine(_dir, $"{COMPLETE_DATA_FILENAME_EXCEL}.xlsx"));

            var excelReader = ExcelDataReader.ExcelReaderFactory.CreateReader(fs);
            var cfg = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };

            _completeDataExcelDataSet = excelReader.AsDataSet(cfg);

            ValidateExcel();
        }

        /// <summary>
        /// Validates excel data against required schema
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        private void ValidateExcel()
        {
            foreach (var sheetName in new[]{EXCEL_SHEET_SURVEY_AREA_STATIC, EXCEL_SHEET_PARKING_LOCATION_STATIC, EXCEL_SHEET_SECTION_STATIC})
            {
                if (!_completeDataExcelDataSet.Tables.Contains(sheetName))
                    throw new System.Exception($"Could not find a mandatory excel sheet: {sheetName}");
            }

            ValidateColumns(
                _completeDataExcelDataSet.Tables[EXCEL_SHEET_SURVEY_AREA_STATIC],
                new[]
                {
                    SURVEY_AREA_COL_LOCALID, SURVEY_AREA_COL_PARENTLOCALID, SURVEY_AREA_COL_NAME,
                    SURVEY_AREA_COL_XTRAINFO, SURVEY_AREA_COL_VALIDFROM, SURVEY_AREA_COL_VALIDTHROUGH,
                    SURVEY_AREA_COL_TYPE
                }
            );

            ValidateColumns(
                _completeDataExcelDataSet.Tables[EXCEL_SHEET_PARKING_LOCATION_STATIC],
                new[]
                {
                    PARKING_LOCATION_COL_NAME, PARKING_LOCATION_COL_FEATURETYPE, PARKING_LOCATION_COL_LOCALID,
                    PARKING_LOCATION_COL_VALIDFROM, PARKING_LOCATION_COL_VALIDTHROUGH, PARKING_LOCATION_COL_XTRAINFO
                }
            );

            ValidateColumns(
                _completeDataExcelDataSet.Tables[EXCEL_SHEET_SECTION_STATIC],
                new[]
                {
                    SECTION_COL_LOCALID, SECTION_COL_NAME, SECTION_COL_VALIDFROM,
                    SECTION_COL_VALIDTHROUGH, SECTION_COL_LEVEL, SECTION_COL_PARKINGSYSTEMTYPE,
                    SECTION_COL_PARKINGLOCATIONLOCALID
                }
            );

        }

        /// <summary>
        /// Validates excel sheet against required columns presence
        /// </summary>
        /// <param name="t"></param>
        /// <param name="colNames"></param>
        /// <exception cref="System.Exception"></exception>
        private void ValidateColumns(DataTable t, string[] colNames)
        {
            foreach (var colName in colNames)
            {
                if (!t.Columns.Contains(colName))
                    throw new System.Exception(
                        $"Required column: {colName} not found in sheet: {EXCEL_SHEET_SURVEY_AREA_STATIC}");
            }
        }

        /// <summary>
        /// Extracts surveys data from xlsx
        /// </summary>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private List<SurveyArea> ExtractCompleteDataSurveyAreas()
        {
            var output = new List<SurveyArea>();

            foreach (DataRow r in _completeDataExcelDataSet.Tables[EXCEL_SHEET_SURVEY_AREA_STATIC].Rows)
            {
                var sa = new SurveyArea
                {
                    Authority = AUTHORITY,
                    LocalId = ExtractFieldValue<string>(r, SURVEY_AREA_COL_LOCALID),
                    ParentLocalId = ExtractFieldValue<string>(r, SURVEY_AREA_COL_PARENTLOCALID),
                    Name = ExtractFieldValue<string>(r, SURVEY_AREA_COL_NAME),
                    XtraInfo = ExtractFieldValue<string>(r, SURVEY_AREA_COL_XTRAINFO),
                    ValidFrom = ExtractFieldValue<DateTime?>(r, SURVEY_AREA_COL_VALIDFROM),
                    ValidThrough = ExtractFieldValue<DateTime?>(r, SURVEY_AREA_COL_VALIDTHROUGH),
                    SurveyAreaType = ExtractFieldValue<string>(r, SURVEY_AREA_COL_TYPE)
                };

                output.Add(sa);
            }


            return output;
        }


        /// <summary>
        /// Extracts sections from xlsx
        /// </summary>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private List<Section> ExtractCompleteDataSections()
        {
            var output = new List<Section>();

            foreach (DataRow r in _completeDataExcelDataSet.Tables[EXCEL_SHEET_SECTION_STATIC].Rows)
            {
                var s = new Section
                {
                    Authority = AUTHORITY,
                    LocalId = ExtractFieldValue<string>(r, SECTION_COL_LOCALID),
                    Name = ExtractFieldValue<string>(r, SECTION_COL_NAME),
                    ValidFrom = ExtractFieldValue<DateTime?>(r, SECTION_COL_VALIDFROM),
                    ValidThrough = ExtractFieldValue<DateTime?>(r, SECTION_COL_VALIDTHROUGH),
                    Level = (int) ExtractFieldValue<double>(r, SECTION_COL_LEVEL),
                    ParkingSpaceOf =
                        TryParseParkingSpaceType(ExtractFieldValue<string>(r, SECTION_COL_PARKINGSYSTEMTYPE),
                            out var parkingSpaceType)
                            ? new []
                            {
                                    new ParkingSpace {
                                        Type = parkingSpaceType
                                    }
                            }
                            
                            : null,
                    ParkingLocationLocalId = ExtractFieldValue<string>(r, SECTION_COL_PARKINGLOCATIONLOCALID),
                };

                output.Add(s);
            }

            return output;
        }

        /// <summary>
        /// Extracts parking locations from xlsx
        /// </summary>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private List<ParkingLocation> ExtractCompleteDataParkingLocations()
        {
            var output = new List<ParkingLocation>();

            foreach (DataRow r in _completeDataExcelDataSet.Tables[EXCEL_SHEET_PARKING_LOCATION_STATIC].Rows)
            {
                var pl = new ParkingLocation
                {
                    Authority = AUTHORITY,
                    LocalId = ExtractFieldValue<string>(r, PARKING_LOCATION_COL_LOCALID),
                    Name = ExtractFieldValue<string>(r, PARKING_LOCATION_COL_NAME),
                    XtraInfo = ExtractFieldValue<string>(r, PARKING_LOCATION_COL_XTRAINFO),
                    ValidFrom = ExtractFieldValue<DateTime?>(r, PARKING_LOCATION_COL_VALIDFROM),
                    ValidThrough = ExtractFieldValue<DateTime?>(r, PARKING_LOCATION_COL_VALIDTHROUGH),
                    Features = ExtractParkingLocationSurveyAreaTypes(r)
                };
                
                output.Add(pl);
            }

            return output;
        }

        /// <summary>
        /// Extracts observations data
        /// </summary>
        /// <param name="fName"></param>
        /// <returns></returns>
        private async Task<List<Observation>> ExtractObservationsXlsxInternalAsync(string fName)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var output = new List<Observation>();

            using var fs = File.OpenRead(fName);

            var excelReader = ExcelDataReader.ExcelReaderFactory.CreateReader(fs);
            var cfg = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };

            var ds = excelReader.AsDataSet(cfg);

            //only one table here, so...
            var tbl = ds.Tables[0];

            //Note: see flatDataExtractor ExtractObservationsSeparatedInternalAsync for extra comments!

            var colMap = new Dictionary<int, string>();
            var idx = 0;
            foreach (DataColumn col in tbl.Columns)
            {
                colMap[idx] = col.ColumnName;
                idx++;
            }

            var vehicleColMap = new Dictionary<int, string>();
            if(tbl.Columns.Count > 24)
            {
                for (var i = 24; i < tbl.Columns.Count; i++)
                {
                    vehicleColMap.Add(i, colMap[i]);
                }
            }

            foreach (DataRow r in tbl.Rows)
            {
                var observationCapacity = new Observation
                {
                    Survey = ExtractFieldValue<string>(r, colMap[12]),
                    ObservedProperty = "capacity",
                    FeatureOfInterest = ExtractFieldValue<string>(r, colMap[0]),
                    TimestampStart = ExtractFieldValue<DateTime>(r, colMap[15]),
                    TimestampEnd = ExtractFieldValue<DateTime>(r, colMap[16]),
                    Note = string.IsNullOrWhiteSpace(ExtractFieldValue<string>(r, colMap[23])) ? null : new Note { Remark = ExtractFieldValue<string>(r, colMap[18]) },
                    Measurement = new Measurement
                    {
                        ParkingCapacity = (int)ExtractFieldValue<double>(r, colMap[17])
                    }
                };
                output.Add(observationCapacity);

                var observationOccupation = new Observation
                {
                    Survey = ExtractFieldValue<string>(r, colMap[12]),
                    ObservedProperty = "occupation",
                    FeatureOfInterest = ExtractFieldValue<string>(r, colMap[0]),
                    TimestampStart = ExtractFieldValue<DateTime>(r, colMap[20]),
                    TimestampEnd = ExtractFieldValue<DateTime>(r, colMap[21]),
                    Note = string.IsNullOrWhiteSpace(ExtractFieldValue<string>(r, colMap[23])) ? null : new Note { Remark = ExtractFieldValue<string>(r, colMap[23]) },
                    Measurement = new Measurement
                    {
                        TotalParked = (int)ExtractFieldValue<double>(r, colMap[17]),
                        VehicleTypeCounts = vehicleColMap.Select(kv => new VehicleTypeCount
                        {
                            CanonicalVehicleCode = kv.Value,
                            NumberOfVehicles = (int)ExtractFieldValue<double>(r, colMap[kv.Key])
                        }).ToArray()
                    }
                };
                output.Add(observationOccupation);
            }

            return output;
        }

        /// <summary>
        /// Extracts survey areas ids for linking
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> ExtractSurveyAreasIdsFlatXlsxInternalAsync(string fName, bool header)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var output = new List<string>();

            using var fs = File.OpenRead(fName);

            var excelReader = ExcelDataReader.ExcelReaderFactory.CreateReader(fs);
            var cfg = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = header
                }
            };

            var ds = excelReader.AsDataSet(cfg);

            //only one table here, so...
            var tbl = ds.Tables[0];

            //first col is the one
            foreach (DataRow r in tbl.Rows)
            {
                output.Add(ExtractFieldValue<string>(r[0]));
            }

            return output;
        }



        /// <summary>
        /// extracts and casts field value to a desired type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        private T ExtractFieldValue<T>(DataRow r, string colName)
            => ExtractFieldValue<T>(r[colName]);

        /// <summary>
        /// extracts and casts field value to a desired type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        private T ExtractFieldValue<T>(object o)
        {
            if (o == DBNull.Value)
                return default;

            return (T)o;
        }

        private ParkingLocationFeature[] ExtractParkingLocationSurveyAreaTypes(DataRow r)
        {
            var output = new List<ParkingLocationFeature>();

            var stringValues = (ExtractFieldValue<string>(r, PARKING_LOCATION_COL_FEATURETYPE)?? string.Empty).Trim().Split(' ');
            foreach (var stringValue in stringValues)
            {
                if (Enum.TryParse(stringValue, true, out ParkingLocationFeature enumValue))
                    output.Add(enumValue);
            }

            return output.ToArray();
        }


        private void DisposeCompleteDataExcel()
        {
            _completeDataExcelDataSet?.Dispose();
        }
    }
}
