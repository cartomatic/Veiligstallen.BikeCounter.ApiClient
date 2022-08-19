using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        /// <summary>
        /// Gets a list of survey areas
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<SurveyArea>> GetSurveyAreasAsync()
            => GetObjectsAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREAS));

        /// <summary>
        /// Gets a survey area by id
        /// </summary>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task<SurveyArea> GetSurveyAreaAsync(string surveyAreaId)
            => GetObjectAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREA, surveyAreaId));

        /// <summary>
        /// Creates a survey area
        /// </summary>
        /// <param name="surveyArea"></param>
        /// <returns></returns>
        public Task<SurveyArea> CreateSurveyAreaAsync(SurveyArea surveyArea)
            => CreateObjectAsync(new RequestConfig<SurveyArea>(Configuration.Routes.SURVEY_AREAS, @object: surveyArea));

        /// <summary>
        /// Updates a survey area
        /// </summary>
        /// <param name="surveyArea"></param>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task<SurveyArea> UpdateSurveyAreaAsync(SurveyArea surveyArea, string surveyAreaId = null)
            => CreateObjectAsync(new RequestConfig<SurveyArea>(Configuration.Routes.SURVEY_AREA, surveyAreaId ?? surveyArea.Id, @object: surveyArea));

        /// <summary>
        /// Deletes a survey area
        /// </summary>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task DeleteSurveyAreaAsync(string surveyAreaId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SURVEY_AREA, surveyAreaId));
    }
}
