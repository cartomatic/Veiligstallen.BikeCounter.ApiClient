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
    public partial class StaticSurveyDataLoader
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
        /// Extracts surveys data
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


        private void DisposeExcel()
        {
            _excelDataSet?.Dispose();
        }
    }
}
