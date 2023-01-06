using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
        /// <summary>
        /// Prepares a col map based on a header row
        /// </summary>
        /// <param name="header"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        private Dictionary<string, int> SeparatedPrepareColMap(string header, FlatFileUtils.FlatFileSeparator separator)
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
        private void SeparatedVerifyRequiredFieldsPresence(Dictionary<string, int> colMap, IEnumerable<string> requiredFieldNames)
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
        private Dictionary<string, string> SeparatedExtractDataRow(Dictionary<string, int> colMap, string line, FlatFileUtils.FlatFileSeparator separator)
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

        private string SeparatedSerializeDate(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("O") : string.Empty;

        private void SeparatedDumpLine(string fName, FlatFileUtils.FlatFileSeparator separator, string[] data)
        {
            File.AppendAllLines(fName, new[]
            {
                string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), data)
            });
        }

        private string SeparatedGetFilePath(string dir, string fName, FlatFileUtils.FlatFileSeparator separator)
        {
            var outFName = Path.Combine(dir, $"{fName}.{FlatFileUtils.FlatFileExtensions[separator]}");
            if (File.Exists(outFName))
                File.Delete(outFName);

            return outFName;
        }
    }
}
