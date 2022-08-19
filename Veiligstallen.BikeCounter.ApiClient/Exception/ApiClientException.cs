using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

namespace Veiligstallen.BikeCounter.ApiClient.Exception
{
    public class ApiClientException : System.Exception
    {
        public IEnumerable<BadRequestDetail> BadRequestDetails { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string HttpStatusCodeDescription { get; set; }

        public ApiClientException()
        {
        }

        public ApiClientException(string message)
        : base(message)
        {
            
        }

        public ApiClientException(string message, System.Exception innerException)
            : base(message, innerException)
        {

        }

        private static string GetRestResponseErrorMessage(IRestResponse response)
            => $"{(response.ErrorMessage ?? $"{response.StatusCode}: {response.StatusDescription}")}";

        public ApiClientException(IRestResponse response)
            : this(GetRestResponseErrorMessage(response))
        {
            HttpStatusCode = response.StatusCode;
            HttpStatusCodeDescription = response.StatusDescription;

            if (
                response.StatusCode == HttpStatusCode.BadRequest 
                || (int)response.StatusCode == 422) //unprocessable entity
            {
                try
                {
                    var badRequestResponse = JsonConvert.DeserializeObject<BadRequestResponse>(response.Content);
                    BadRequestDetails = badRequestResponse.Detail;
                }
                catch
                {
                    //ignore
                }
            }
        }
    }
}
