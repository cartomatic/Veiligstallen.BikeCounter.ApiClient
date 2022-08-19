using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Remotion.Linq.Clauses;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Exception;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        private class RequestConfig
        {
            public RequestConfig(string route, string? objectId = null, string? parentId = null)
            {
                Route = route;
                ObjectId = objectId;
                ParentId = parentId;
            }
            public string Route { get; set; }
            public string? ObjectId { get; set; }
            public string? ParentId { get; set; }
        }
        private class RequestConfig<T> : RequestConfig
            where T: class
        {
            public T Object { get; set; }

            /// <inheritdoc />
            public RequestConfig(string route, string? objectId = null, string? parentId = null, T @object = null) 
                : base(route, objectId, parentId)
            {
                Object = @object;
            }
        }

        /// <summary>
        /// Ensures api resulted in a valid output
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="ApiClientException"></exception>
        private void EnsureValidResponse(IRestResponse response)
        {
            //treat 404 as a successful code - a missing object, collection
            if (response.IsSuccessful || response.StatusCode == HttpStatusCode.NotFound)
                return;
            
            throw new ApiClientException(response);
        }

        /// <summary>
        /// Prepares a request route
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private string PrepareRoute(RequestConfig cfg)
        {
            var route = cfg.Route;
            if (string.IsNullOrWhiteSpace(route))
                throw new System.Exception("Missing request route");

            if (!string.IsNullOrWhiteSpace(cfg.ParentId))
                route = route.Replace(Configuration.Routes.PARENT_ID, cfg.ParentId);

            if (!string.IsNullOrWhiteSpace(cfg.ObjectId))
                route = route.Replace(Configuration.Routes.OBJECT_ID, cfg.ObjectId);

            return route;
        }

        /// <summary>
        /// Retrieves a list of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private async Task<IEnumerable<T>> GetObjectsAsync<T>(RequestConfig cfg)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<List<T>>(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.GET,
                authToken: GetAuthorizationHeaderValue(_user, _pass)
            );

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output ?? new List<T>();
        }

        /// <summary>
        /// Retrieves an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private async Task<T> GetObjectAsync<T>(RequestConfig cfg)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<T>(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.GET,
                authToken: GetAuthorizationHeaderValue(_user, _pass)
            );

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output;
        }

        /// <summary>
        /// Creates an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private async Task<T> CreateObjectAsync<T>(RequestConfig<T> cfg)
            where T : class
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<T>(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.POST,
                data: cfg.Object,
                authToken: GetAuthorizationHeaderValue(_user, _pass)
            );

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output;
        }


        /// <summary>
        /// Updates object by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private async Task<T> UpdateObjectAsync<T>(RequestConfig<T> cfg)
            where T : class
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<T>(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.PUT,
                data: cfg.Object,
                authToken: GetAuthorizationHeaderValue(_user, _pass)
            );

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output;
        }


        /// <summary>
        /// Deletes an object by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private async Task DeleteObjectAsync(RequestConfig cfg)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.DELETE,
                authToken: GetAuthorizationHeaderValue(_user, _pass)
            );

            EnsureValidResponse(apiOut);
        }
    }
}
