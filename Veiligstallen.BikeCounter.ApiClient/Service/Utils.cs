using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;
using RestSharp;
using RestSharp.Serializers;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Exception;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        private class RequestConfig
        {
            public RequestConfig(string route, string? objectId = null, string? parentId = null, Dictionary<string, string> queryParams = null)
            {
                Route = route;
                ObjectId = objectId;
                ParentId = parentId;
                QueryParams = queryParams;
            }
            public string Route { get; set; }
            public string? ObjectId { get; set; }
            public string? ParentId { get; set; }
            public Dictionary<string, string> QueryParams { get; set; }
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

        private class CustomEnumConverter : StringEnumConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return base.CanConvert(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var isNullable = (Nullable.GetUnderlyingType(objectType) != null);
                if (reader.TokenType == JsonToken.Null)
                {
                    if (!isNullable)
                    {
                        throw new JsonSerializationException();
                    }
                    return null;
                }

                var type = (isNullable ? Nullable.GetUnderlyingType(objectType) : objectType);

                try
                {
                    if (reader.TokenType == JsonToken.String)
                    {
                        string value = reader.Value?.ToString();
                        if (string.IsNullOrWhiteSpace(value) && isNullable)
                        {
                            return null;
                        }

                        //trim dashes if any - this seems to appear in bike counter data...
                        value = value.Replace("-", string.Empty);

                        try
                        {
                            return Enum.Parse(type, value, true);
                        }
                        catch (System.Exception ex)
                        {
                            //this should be a default value for a type
                            return isNullable ? null : Activator.CreateInstance(type);
                        }
                    }

                    if (reader.TokenType == JsonToken.Integer)
                    {
                        return base.ReadJson(reader, objectType, existingValue, serializer); ;
                    }
                }
                catch (System.Exception ex)
                {
                    throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType?.ToString()}'.", ex);
                }

                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum.");
            }
        }


        private class Sort
        {
            public string Property { get; set; }
            public string Direction { get; set; }
        }

        private ISerializer _serializer = new Cartomatic.Utils.RestSharpSerializers.NewtonSoftJsonSerializer(
            Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<Newtonsoft.Json.JsonConverter>() {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                }
            });

        private JsonConverter[] _converters = new[]
        {
            new CustomEnumConverter()
        };

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
        private async Task<(IEnumerable<T> data, int total)> GetObjectsAsync<T>(RequestConfig cfg)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<List<T>>(
                _cfg.Endpoint,
                PrepareRoute(cfg),
                Method.GET,
                authToken: GetAuthorizationHeaderValue(),
                queryParams: PrepareQueryParams(cfg),
                serializer: _serializer,
                converters: _converters
            );

            EnsureValidResponse(apiOut.Response);

            var data = apiOut.Output ?? new List<T>();
            var total = -1;
            var totalHdr = apiOut.Response.Headers.FirstOrDefault(x => x.Name == "X-Total-Count");
            if (totalHdr != null && int.TryParse(totalHdr.Value.ToString(), out var parsedTotal))
                total = parsedTotal;

            return (data, total);
        }

        private static string[] _reservedQueryParams =
        {
            "start", "limit", "_dc", "page", "sort"
        };

        /// <summary>
        /// Prepares additional query params
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        private Dictionary<string, object> PrepareQueryParams(RequestConfig cfg)
        {
            var queryParams = new Dictionary<string, object>();

            var inParams = cfg.QueryParams ?? new Dictionary<string, string>();
            
            //sorting
            if (inParams.ContainsKey("sort"))
            {
                var sorts = JsonConvert.DeserializeObject<Sort[]>(inParams["sort"]);

                if (sorts != null && sorts.Any())
                {
                    var sort = sorts.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Property));
                    if (sort != null)
                    {
                        queryParams.Add("orderBy", sort.Property);
                        queryParams.Add("orderDirection", string.IsNullOrWhiteSpace(sort.Direction) ? "ASC" : sort.Direction);
                    }
                }
            }

            //paging
            if(inParams.ContainsKey("start") && int.TryParse(inParams["start"], out var offset))
                queryParams.Add(nameof(offset), offset);

            if (inParams.ContainsKey("limit") && int.TryParse(inParams["limit"], out var limit))
                queryParams.Add(nameof(limit), limit);

            //remaining query params
            foreach (var inParamsKey in inParams.Keys)
            {
                if(!_reservedQueryParams.Contains(inParamsKey))
                    queryParams.Add(inParamsKey, inParams[inParamsKey]);
            }

            return queryParams;
        }

        /// <summary>
        /// Camelizes string cases
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string CameliseCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var trimmed = input.Trim();

            return $"{trimmed.Substring(0, 1).ToLower()}{trimmed.Substring(1)}";
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
                authToken: GetAuthorizationHeaderValue(),
                serializer: _serializer,
                converters: _converters
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
                authToken: GetAuthorizationHeaderValue(),
                serializer: _serializer,
                converters: _converters
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
                authToken: GetAuthorizationHeaderValue(),
                serializer: _serializer,
                converters: _converters
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
                authToken: GetAuthorizationHeaderValue()
            );

            EnsureValidResponse(apiOut);
        }
    }
}
