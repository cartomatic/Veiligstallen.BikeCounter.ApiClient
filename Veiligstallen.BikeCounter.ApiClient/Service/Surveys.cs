using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

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

        public async Task<Guid> PrepareSurveyCountingSheetAsync(string surveyId, string outDir)
        {
            var downloadId = Guid.NewGuid();


            //need parking locations
            //for parking locations need sections
            //also need canonical vehicle category for a survey and vehicles for a category
            //all then needs to be assembled into a flat structure and output as tsv


            //TODO - download all the stuff and assemble a separated output!


            File.WriteAllText(Path.Combine(outDir, $"{downloadId}"), $"temp content {surveyId}");

            return downloadId;
        }
    }
}
