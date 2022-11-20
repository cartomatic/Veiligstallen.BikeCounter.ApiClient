using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
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

        private DataSet _excelDataSet;

        /// <summary>
        /// Loads an validates excel data
        /// </summary>
        private void LoadExcelData()
        {
            using var fs = File.OpenRead(Path.Combine(_dir, $"{FILENAME_EXCEL}.xlsx"));

            var excelReader = ExcelDataReader.ExcelReaderFactory.CreateReader(fs);
            var cfg = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };

            _excelDataSet = excelReader.AsDataSet(cfg);

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
                if (!_excelDataSet.Tables.Contains(sheetName))
                    throw new System.Exception($"Could not find a mandatory excel sheet: {sheetName}");
            }

            ValidateColumns(
                _excelDataSet.Tables[EXCEL_SHEET_SURVEY_AREA_STATIC],
                new[]
                {
                    SURVEY_AREA_COL_LOCALID, SURVEY_AREA_COL_PARENTLOCALID, SURVEY_AREA_COL_NAME,
                    SURVEY_AREA_COL_XTRAINFO, SURVEY_AREA_COL_VALIDFROM, SURVEY_AREA_COL_VALIDTHROUGH,
                    SURVEY_AREA_COL_TYPE
                }
            );

            ValidateColumns(
                _excelDataSet.Tables[EXCEL_SHEET_PARKING_LOCATION_STATIC],
                new[]
                {
                    PARKING_LOCATION_COL_NAME, PARKING_LOCATION_COL_FEATURETYPE, PARKING_LOCATION_COL_LOCALID,
                    PARKING_LOCATION_COL_VALIDFROM, PARKING_LOCATION_COL_VALIDTHROUGH, PARKING_LOCATION_COL_XTRAINFO
                }
            );

            ValidateColumns(
                _excelDataSet.Tables[EXCEL_SHEET_SECTION_STATIC],
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
        private List<SurveyArea> ExtractSurveyAreas()
        {
            var output = new List<SurveyArea>();

            foreach (DataRow r in _excelDataSet.Tables[EXCEL_SHEET_SURVEY_AREA_STATIC].Rows)
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
        private List<Section> ExtractSections()
        {
            var output = new List<Section>();

            foreach (DataRow r in _excelDataSet.Tables[EXCEL_SHEET_SECTION_STATIC].Rows)
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
        private List<ParkingLocation> ExtractParkingLocations()
        {
            var output = new List<ParkingLocation>();

            foreach (DataRow r in _excelDataSet.Tables[EXCEL_SHEET_PARKING_LOCATION_STATIC].Rows)
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
        /// Extracts field value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="r"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        private T ExtractFieldValue<T>(DataRow r, string colName)
        {
            if (r[colName] == DBNull.Value)
                return default;

            return (T)r[colName];
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


        private void DisposeExcel()
        {
            _excelDataSet?.Dispose();
        }
    }
}
