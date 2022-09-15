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
        /// Gets a list of surveys
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Survey>> GetSurveysAsync()
            => GetObjectsAsync<Survey>(new RequestConfig(Configuration.Routes.SURVEYS));

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
    }
}
