﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Remotion.Linq.Clauses;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        private static string FLAT_FILE_SURVEY_AREAS = "survey_areas";
        private static string FLAT_FILE_PARKING_LOCATIONS = "parking_locations";
        private static string FLAT_FILE_SECTIONS = "sections";

        public enum FlatFileSeparator
        {
            Tab,
            Semicolon
        };

        private static Dictionary<FlatFileSeparator, string> SEPARATORS = new()
        {
            {FlatFileSeparator.Tab, "\t"},
            {FlatFileSeparator.Semicolon, "csv"}
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
                    $"Invalid separator: {separator}; supported separators are: {string.Join(",",SEPARATORS.Keys.Select(k => $"{k}"))}");

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

        private string SerializeParkingLocationFeature(SurveyAreaType[] features)
            => string.Join(",", features?.Select(x => $"{x}") ?? Array.Empty<string>());

        private string SerializeParkingLocationAllowsType(VehicleType? t)
            => t.HasValue ? $"{t}" : string.Empty;

        private void DumpLine(string fName, FlatFileSeparator separator, string[] data)
        {
            File.AppendAllLines(fName, new[]
            {
                string.Join(SEPARATORS[separator], data)
            });
        }

        private string GetFlatFilePath(string dir, string fName, FlatFileSeparator separator)
        {
            var outFName = Path.Combine(dir, $"{fName}.{EXTENSIONS[separator]}");
            if (File.Exists(outFName))
                File.Delete(outFName);

            return outFName;
        }
    }
}
