﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Loader;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        /// <summary>
        /// Gets a list of surveys
        /// </summary>
        /// <returns></returns>
        public Task<(IEnumerable<Survey> data, int total)> GetSurveysAsync(Dictionary<string, string> queryParams)
            => GetObjectsAsync<Survey>(new RequestConfig(Configuration.Routes.SURVEYS){QueryParams = queryParams});

        /// <summary>
        /// Gets a survey by id
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<Survey> GetSurveyAsync(string surveyId)
            => GetObjectAsync<Survey>(new RequestConfig(Configuration.Routes.SURVEY, surveyId));

        /// <summary>
        /// Creates a survey
        /// </summary>
        /// <param name="survey"></param>
        /// <returns></returns>
        public Task<Survey> CreateSurveyAsync(Survey survey)
            => CreateObjectAsync(new RequestConfig<Survey>(Configuration.Routes.SURVEYS, @object: survey));

        /// <summary>
        /// Updates a survey
        /// </summary>
        /// <param name="survey"></param>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<Survey> UpdateSurveyAsync(Survey survey, string surveyId = null)
            => UpdateObjectAsync(new RequestConfig<Survey>(Configuration.Routes.SURVEY, surveyId ?? survey.Id, @object: survey));

        /// <summary>
        /// Deletes a survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task DeleteSurveyAsync(string surveyId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SURVEY, surveyId));

        /// <summary>
        /// Connects survey area ids to a survey by either overwriting or appending
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="surveyAreasIds"></param>
        /// <param name="appendData"></param>
        /// <returns></returns>
        public async Task LinkSurveyAreasAsync(string surveyId, IEnumerable<string> surveyAreasIds, bool appendData)
        {
            var cfg = new RequestConfig(Configuration.Routes.SURVEY_SURVEY_AREAS, surveyId);

            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                appendData ? Method.PUT : Method.POST,
                authToken: GetAuthorizationHeaderValue(),
                data: surveyAreasIds
            );

            EnsureValidResponse(apiOut);
        }


        private string[] _surveyStaticDataHeaders =
        {
            "section_id",
            "section_localId",
            "parkinglocation_id",
            "parkinglocation_localId",
            "section_name",
            "section_layout",
            "section_url_geolocation",
            "section_parkingSystemType",
            "section_vehicleOwnerType",
            "section_level",
            "section_validFrom",
            "section_validThrough"
        };

        private string[] _surveyDynamicDataHeaders =
        {
            "survey_id",
            "survey_surveyAreaParent",
            "survey_surveyAreaParentLocalId",
            "survey_surveyAreaChild",
            "survey_surveyAreaChildLocalId",
            "contractors",
            "observation_capacity_id",
            "observation_capacity_timestamp_start",
            "observation_capacity_timestamp_end",
            "observation_capacity_parkingCapacity",
            "observation_capacity_note",
            "observation_occupation_id",
            "observation_occupation_timestamp_start",
            "observation_occupation_timestamp_end",
            "occupation_totalParked",
            "observation_occupation_note"
        };

        /// <summary>
        /// Prepares a counting sheet for a survey; returns an id (name) of a file output to the output dir
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="outDir"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public async Task<Guid> PrepareSurveyCountingSheetAsync(string surveyId, string outDir, FlatFileUtils.FlatFileSeparator separator)
        {
            var downloadId = Guid.NewGuid();

            
            //this should be reasonable enough to pull all the canonical vehicles data...
            var queryParams = new Dictionary<string, string> {{"start", "0"}, {"limit", "1000"}};

            var survey = await GetSurveyAsync(surveyId);
            var (canonicalVehicles, _) = string.IsNullOrWhiteSpace(survey.CanonicalVehicleCategory)
                ? (Array.Empty<CanonicalVehicle>(), 0)
                : await GetCanonicalVehiclesAsync(survey.CanonicalVehicleCategory, queryParams);

            var (parkingLocations, _) = await GetSurveyParkingLocationsAsync(surveyId);
            var (sections, _) = await GetSurveySectionsAsync(surveyId);

            var canonicalVehicleHeaders = canonicalVehicles.Select(x => $"{x.Code}").ToArray();
            var canonicalVehiclesEmptyData = canonicalVehicleHeaders.Select(x => string.Empty).ToArray();

            var dynamicDataEmptyData = _surveyDynamicDataHeaders.Select(x => string.Empty).ToArray();
            dynamicDataEmptyData[0] = surveyId;
            dynamicDataEmptyData[1] = string.Join(", ", survey.Contractors);

            var applySeparator = (IEnumerable<string> data) =>
                string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), data);

            //output data container
            var data = new List<string>
            {
                applySeparator(GetSurveyCountingSheetHeaderFields(canonicalVehicleHeaders))
            };


            foreach (var parkingLocation in parkingLocations.OrderBy(x => x.LocalId))
            {
                var subSections = sections.Where(x => x.ParkingLocation == parkingLocation.Id).ToArray();
                foreach (var section in subSections)
                {
                    var recData = new List<string>
                    {
                        section.Id,
                        section.LocalId,
                        parkingLocation.Id,
                        parkingLocation.LocalId,
                        section.Name,
                        section.Layout,
                        $"{_cfg.ShapeEndpoint}{_cfg.Endpoint}/{Configuration.Routes.SECTIONS}/{section.Id}",

                        //section_parkingSystemType
                        section.ParkingSpaceOf?.Any() == true
                            ? string.Join(",", section.ParkingSpaceOf.Select(x => $"{x.Type}"))
                            : null,

                        //section_vehicleOwnerType
                        section.ParkingSpaceOf?.Any() == true
                            ? string.Join(
                                ",",
                                section.ParkingSpaceOf.Aggregate(new List<string>(), (agg, ps) =>
                                {
                                    if (ps.Vehicles?.Any() == true)
                                        agg.AddRange(ps.Vehicles.Select(x => $"{x.Owner}"));

                                    return agg;
                                }).Distinct()
                            )
                            : null,
                        section.Level.ToString(),
                        section.LevelSub,
                        section.ValidFrom?.ToString("yyyy-MM-dd"),
                        section.ValidThrough?.ToString("yyyy-MM-dd")
                    };

                    //some data is per section...
                    dynamicDataEmptyData[1] = section.SurveyAreaParent;
                    dynamicDataEmptyData[2] = section.SurveyAreaParentLocalId;
                    dynamicDataEmptyData[3] = section.SurveyAreaChild;
                    dynamicDataEmptyData[4] = section.SurveyAreaChildLocalId;

                    recData.AddRange(dynamicDataEmptyData);
                    recData.AddRange(canonicalVehiclesEmptyData);

                    data.Add(applySeparator(recData));
                }
            }


            File.WriteAllLines(Path.Combine(outDir, $"{downloadId}"), data);

            return downloadId;
        }

        private static string[] _observationHeaders =
        {
            "survey_id", //Survey.id
            "survey_name", //Survey.name
            "survey_authority", //Survey.authority
            "contractor", //"survey_contractors", //Survey.contractors

            "surveyarea_id", //SurveyArea.id (SurveyArea with empty or no property ‘parent’)
            "surveyarea_localId", //SurveyArea.localId
            "surveyarea_name", //SurveyArea.name
            "surveyarea_xtrainfo", //SurveyArea.xtrainfo

            "surveyarea_id_child",
            "surveyarea_localId_child", //child SurveyArea.id (optional, child is surveyArea with prop parent filled)
            "surveyarea_name_child", //child SurveyArea.name  (optional, child is surveyArea with prop parent filled)
            "surveyarea_xtrainfo_child",

            "geolocation", //https://remote.veiligstallenontwikkel.nl/rest/api/v2/parking-locations/<parking-location.id>, https://remote.veiligstallenontwikkel.nl/rest/api/v2/sections/<section.id>

            "parkinglocation_id", //parkinglocation.id
            "parkinglocation_localId", //parkinglocation.localId
            "parkinglocation_name	", //parkinglocation.name
            "parkinglocation_locationFeatureType", //comma separated list of parkinglocation.features
            "parkinglocation_xtrainfo", //parkinglocation.xtrainfo

            "section_id", //section.id
            "section_localId", //section.localId
            "section_name", //section.name
            "section_layout", //section.layout
            "section_parkingSystemType", //section.
            "section_vehicleOwnerType", //section.
            "section_level", //section.level
            "section_level_sub", //section.levelSub

            "observation_capacity_id", //capacityObservation.id
            "observation_capacity_timestamp_start", //capacityObservation.timestampStart
            "observation_capacity_timestamp_end", //capacityObservation.timestampEnd
            "observation_capacity_parkingCapacity", //capacityObservation.parkingCapacity
            "observation_capacity_note", //capacityObservation.note

            "observation_occupation_id", //occupationObservation.id
            "observation_occupation_timestamp_start", //occupationObservation.timestampStart
            "observation_occupation_timestamp_end", //occupationObservation.timestampEnd
            "observation_occupation_totalParked", //occupationObservation.measurement.totalParked
            "observation_occupation_note", //occupationObservation.note
        };

        /// <summary>
        /// Prepares survey observations xlsx file
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="outDir"></param>
        /// <returns></returns>
        public async Task<Guid> PrepareSurveyObservationsAsync(string surveyId, string outDir, FlatFileUtils.FlatFileSeparator separator)
        {
            if (separator != FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotSupportedException("Only XLSX format is supported for survey observations download");


            var downloadId = Guid.NewGuid();


            //this should be reasonable enough to pull all the canonical vehicles data...
            var queryParams = new Dictionary<string, string> { { "start", "0" }, { "limit", "1000" } };

            var survey = await GetSurveyAsync(surveyId);
            var (canonicalVehicles, _) = string.IsNullOrWhiteSpace(survey.CanonicalVehicleCategory)
                ? (Array.Empty<CanonicalVehicle>(), 0)
                : await GetCanonicalVehiclesAsync(survey.CanonicalVehicleCategory, queryParams);

            var (surveyAreas, _) = await GetSurveySurveyAreasAsync(surveyId);


            
            var canonicalVehicleHeaders = canonicalVehicles.Select(x => $"{x.Code} ({x.Name})").ToArray();
            
            var (combinedObservations, _) = await GetSurveyCombinedObservationsAsync(surveyId);

            IEnumerable<ParkingLocation> parkingLocations = new List<ParkingLocation>();
            if (combinedObservations.Any() && combinedObservations.First().ParkingLocationSummary == null)
                (parkingLocations, _) = await GetSurveyParkingLocationsAsync(surveyId);

            IEnumerable<Section> sections = new List<Section>();
            if (combinedObservations.Any() && combinedObservations.First().SectionSummary == null)
                (sections, _) = await GetSurveySectionsAsync(surveyId);


            //at this stage all the data should be available, so can start pumping it into excel output
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Basis obv sections voor draaitabellen".Substring(0,31));

            var rowIdx = 1;
            EmitHeader(ws, rowIdx, canonicalVehicleHeaders);
            rowIdx++;


            //emit data row
            var cellIdx = 1;
            var emitCell = (string value) =>
            {
                var cell = ws.Cell(rowIdx, cellIdx);
                cell.Value = value;

                //no cell colouring anymore...
                cell.Style.Font.FontColor = GetCellTextOrBorderColor(cellIdx);
                cell.Style.Fill.BackgroundColor = GetCellColor(cellIdx);

                ApplyCellBorder(cell, GetCellTextOrBorderColor(cellIdx));

                cellIdx++;
            };
            var emitNumericCell = (string value) =>
            {
                var cell = ws.Cell(rowIdx, cellIdx);
                

                if (decimal.TryParse((value ?? string.Empty).Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedNumeric))
                {
                    cell.Value = parsedNumeric;
                }
                else
                {
                    cell.Value = value;
                }

                //no cell colouring anymore...
                cell.Style.Font.FontColor = GetCellTextOrBorderColor(cellIdx);
                cell.Style.Fill.BackgroundColor = GetCellColor(cellIdx);

                ApplyCellBorder(cell, GetCellTextOrBorderColor(cellIdx));

                cellIdx++;
            };
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            var toCet = (DateTime? dt) => dt.HasValue ? TimeZoneInfo.ConvertTime(dt.Value, tz).ToString("d-M-yyyy H:mm:ss") : string.Empty;
            foreach (var o in combinedObservations)
            {
                cellIdx = 1;

                emitCell(survey.Id);
                emitCell(survey.Name);
                emitCell(survey.Authority);
                emitCell(o.OccupationObservation?.Contractor ?? o.CapacityObservation?.Contractor ?? string.Join(", ", survey.Contractors));

                var surveyArea = surveyAreas.FirstOrDefault(x => x.Id == o.SurveyArea);
                var surveyAreaParentId = surveyArea?.Parent ?? o.SurveyAreaParent;

                var surveyAreaParent = !string.IsNullOrWhiteSpace(surveyAreaParentId)
                    ? surveyAreas.FirstOrDefault(x => x.Id == surveyAreaParentId)
                    : null;

                //older way of working out the parent survey area
                //var surveyAreaParent = surveyArea != null //this is a stupid fix really as an observation should have a survey; in the test env this wasn't always true...
                //    ? surveyAreas.FirstOrDefault(x => surveyArea.Parent != null && x.Id == surveyArea.Parent) ?? surveyArea
                //    : null;
                //if (surveyAreaParent == surveyArea)
                //    surveyArea = null;

                emitCell(surveyAreaParent?.Id);
                emitCell(surveyAreaParent?.LocalId);
                emitCell(surveyAreaParent?.Name);
                emitCell(surveyAreaParent?.XtraInfo);

                emitCell(surveyArea?.Id);
                emitCell(surveyArea?.LocalId);
                emitCell(surveyArea?.Name);
                emitCell(surveyArea?.XtraInfo);


                emitCell($"{_cfg.ShapeEndpoint}{_cfg.Endpoint}/{Configuration.Routes.SECTIONS}/{o.Section}");


                var parkingLocation = parkingLocations.FirstOrDefault(x => x.Id == o.ParkingLocation);
                emitCell(o.ParkingLocationSummary?.Id ?? parkingLocation?.Id);
                emitCell(o.ParkingLocationSummary?.LocalId ?? parkingLocation?.LocalId);
                emitCell(o.ParkingLocationSummary?.Name ?? parkingLocation?.Name);
                emitCell(string.Join(", ",
                    o.ParkingLocationSummary?.Features?.Select(x => $"{x}") ??
                    parkingLocation?.Features?.Select(x => $"{x}") ?? Array.Empty<string>()));
                emitCell(o.ParkingLocationSummary.XtraInfo ?? parkingLocation?.XtraInfo);


                var section = sections.FirstOrDefault(x => x.Id == o.Section);
                emitCell(o.SectionSummary?.Id ?? section?.Id);
                emitCell(o.SectionSummary?.LocalId ?? section?.LocalId);
                emitCell(o.SectionSummary?.Name ?? section?.Name); 
                emitCell(o.SectionSummary?.Layout ?? section?.Layout);

                emitCell(string.Join(", ",
                    o.SectionSummary?.ParkingSpaceOf?.Select(x => $"{x.Type}") ??
                    section?.ParkingSpaceOf?.Select(x => $"{x.Type}") ?? Array.Empty<string>()));

                emitCell(
                    string.Join(
                        ", ",
                        section?.ParkingSpaceOf?.Aggregate(new List<VehicleOwner>(), (agg, v) =>
                        {
                            agg.AddRange(
                                (v.Vehicles ?? Array.Empty<Vehicle>()).Select(x => x.Owner)
                            );
                            return agg;
                        })
                        .Distinct()
                        ?? Array.Empty<VehicleOwner>()
                    )
                );


                emitCell(section?.Level.ToString());
                emitCell(section?.LevelSub);



                emitCell(o.CapacityObservation?.Id);
                emitCell(toCet(o.CapacityObservation?.TimestampStart));
                emitCell(toCet(o.CapacityObservation?.TimestampEnd));
                emitNumericCell(o.CapacityObservation?.Measurement?.ParkingCapacity.ToString());
                emitCell(o.CapacityObservation?.Note?.Remark);


                emitCell(o.OccupationObservation?.Id);
                emitCell(toCet(o.OccupationObservation?.TimestampStart));
                emitCell(toCet(o.OccupationObservation?.TimestampEnd));
                emitNumericCell(o.OccupationObservation?.Measurement?.TotalParked.ToString());
                emitCell(o.OccupationObservation?.Note?.Remark);

                var capacities = o.OccupationObservation?.Measurement?.VehicleTypeCounts ?? Array.Empty<VehicleTypeCount>();
                //if(capacities.Length != canonicalVehicleHeaders.Length)
                //{
                //    var wtf = true;
                //}
                for (var i = 0; i < canonicalVehicleHeaders.Length; i++)
                {

                    emitNumericCell(capacities.Length > i ? capacities[i].NumberOfVehicles.ToString() : string.Empty);
                }

                rowIdx++; ;
            }

            wb.SaveAs(Path.Combine(outDir, $"{downloadId}.xlsx"));

            return downloadId;
        }

        private void EmitHeader(IXLWorksheet ws, int rowIdx, string[] canonicalVehicleHeaders)
        {
            var cellIdx = 1;
            var emitCell = (string value) =>
            {
                var cell = ws.Cell(rowIdx, cellIdx);

                cell.Value = value;

                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = GetHeaderTextOrBorderColor(cellIdx);

                cell.Style.Fill.BackgroundColor = GetHeaderCellColor(cellIdx);

                ApplyCellBorder(cell, GetHeaderTextOrBorderColor(cellIdx));

                cellIdx++;
            };

            foreach(var hdr in _observationHeaders)
            {
                emitCell(hdr);
            }

            foreach (var hdr in canonicalVehicleHeaders)
            {
                emitCell(hdr);
            }

        }

        private void ApplyCellBorder(IXLCell cell, XLColor color)
        {
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorderColor = color;

            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.TopBorderColor = color;

            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.LeftBorderColor = color;

            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.RightBorderColor = color;
        }

        private XLColor GetHeaderCellColor(int cellIdx)
        {
            if (cellIdx < 5)
                return XLColor.FromArgb(0, 97, 0);
            if (cellIdx < 9)
                return XLColor.FromArgb(56, 118, 29);
            if (cellIdx < 13)
                return XLColor.FromArgb(106, 168, 79);
            if (cellIdx < 14)
                return XLColor.FromArgb(234, 209, 220);
            if (cellIdx < 19)
                return XLColor.FromArgb(182, 215, 168);
            if (cellIdx < 26)
                return XLColor.FromArgb(198, 239, 206);
            if (cellIdx < 31)
                return XLColor.FromArgb(255, 229, 152);
            if (cellIdx < 36)
                return XLColor.FromArgb(254, 242, 203);
            if (cellIdx < 41)
                return XLColor.FromArgb(222, 234, 246);

            return XLColor.NoColor;
        }

        private XLColor GetCellColor(int cellIdx)
        {
            return XLColor.NoColor;
        }

        private XLColor GetHeaderTextOrBorderColor(int cellIdx)
        {
            if (cellIdx < 13)
                return XLColor.White;

            return XLColor.Black;
        }

        private XLColor GetCellTextOrBorderColor(int cellIdx)
        {
            return XLColor.Black;
        }




        /// <summary>
        /// returns header for the survey counting sheet
        /// </summary>
        /// <param name="vehicleHeaders"></param>
        /// <returns></returns>
        IEnumerable<string> GetSurveyCountingSheetHeaderFields(IEnumerable<string> vehicleHeaders)
        {
            var hdrFields = new List<string>();
            hdrFields.AddRange(_surveyStaticDataHeaders);
            hdrFields.AddRange(_surveyDynamicDataHeaders);
            hdrFields.AddRange(vehicleHeaders);

            return hdrFields;
        }

        /// <summary>
        /// Returns all the survey areas for given survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<SurveyArea> data, int total)> GetSurveySurveyAreasAsync(string surveyId)
            => GetObjectsAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_SURVEY_AREAS, surveyId));

        /// <summary>
        /// Returns all the parking locations for given survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<ParkingLocation> data, int total)> GetSurveyParkingLocationsAsync(string surveyId)
            => GetObjectsAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.SURVEY_PARKING_LOCATIONS, surveyId));

        /// <summary>
        /// Returns all the sections for given survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<Section> data, int total)> GetSurveySectionsAsync(string surveyId)
            => GetObjectsAsync<Section>(new RequestConfig(Configuration.Routes.SURVEY_SECTIONS, surveyId));

        /// <summary>
        /// Returns observations for a survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<Observation> data, int total)> GetSurveyObservationsAsync(string surveyId)
            => GetObjectsAsync<Observation>(new RequestConfig(Configuration.Routes.SURVEY_OBSERVATIONS, surveyId));

        /// <summary>
        /// returns combined observations for a survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<CombinedObservation> data, int total)> GetSurveyCombinedObservationsAsync(string surveyId)
            => GetObjectsAsync<CombinedObservation>(new RequestConfig(Configuration.Routes.SURVEY_COMBINED_OBSERVATIONS, surveyId));
    }
}
