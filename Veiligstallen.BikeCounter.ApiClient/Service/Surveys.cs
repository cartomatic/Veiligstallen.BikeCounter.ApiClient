using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            "contractor",
            "observation_capacity_id",
            "observation_capacity_timestamp_start",
            "observation_capacity_timestamp_end",
            "capacity_parkingCapacity",
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
        public async Task<Guid> PrepareSurveyCountingSheetAsync(string surveyId, string outDir, FlatFileSeparator separator)
        {
            var downloadId = Guid.NewGuid();

            
            //this should be reasonable enough to pull all the canonical vehicles data...
            var queryParams = new Dictionary<string, string> {{"start", "0"}, {"limit", "1000"}};

            var survey = await GetSurveyAsync(surveyId);
            var (canonicalVehicles, _) = string.IsNullOrWhiteSpace(survey.CanonicalVehicleCategory)
                ? (Array.Empty<CanonicalVehicle>(), 0)
                : await GetCanonicalVehiclesAsync(survey.CanonicalVehicleCategory, queryParams);

            var (parkingLocations, _) = await GetSurveyPaekingLocationsAsync(surveyId);
            var (sections, _) = await GetSurveySectionsAsync(surveyId);

            var canonicalVehicleHeaders = canonicalVehicles.Select(x => $"{x.Code} ({x.Name})").ToArray();
            var canonicalVehiclesEmptyData = canonicalVehicleHeaders.Select(x => string.Empty).ToArray();

            var dynamicDataEmptyData = _surveyDynamicDataHeaders.Select(x => string.Empty).ToArray();

            var applySeparator = (IEnumerable<string> data) =>
                string.Join(separator == FlatFileSeparator.Semicolon ? ";" : "\t", data);

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
                        $"{_cfg.Endpoint}/{Configuration.Routes.SECTIONS}/{section.Id}",
                        section.ParkingSystemType,
                        section.VehicleOwnerType,
                        section.Level.ToString(),
                        section.ValidFrom?.ToString("yyyy-MM-dd"),
                        section.ValidThrough?.ToString("yyyy-MM-dd")
                    };
                    recData.AddRange(dynamicDataEmptyData);
                    recData.AddRange(canonicalVehiclesEmptyData);

                    data.Add(applySeparator(recData));
                }
            }


            File.WriteAllLines(Path.Combine(outDir, $"{downloadId}"), data);

            return downloadId;
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
        public Task<(IEnumerable<ParkingLocation> data, int total)> GetSurveyPaekingLocationsAsync(string surveyId)
            => GetObjectsAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.SURVEY_PARKING_LOCATIONS, surveyId));

        /// <summary>
        /// Returns all the sections for given survey
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<Section> data, int total)> GetSurveySectionsAsync(string surveyId)
            => GetObjectsAsync<Section>(new RequestConfig(Configuration.Routes.SURVEY_SECTIONS, surveyId));
    }
}
