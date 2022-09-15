using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
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

        private class RemoteApiExceptionDetails
        {
            public string Message { get; set; }
            public bool? Error { get; set; }
        }

        private static string GetRestResponseErrorMessage(IRestResponse response)
        {
            var errorMessage = string.Empty;
            try
            {
                var remoteApiExceptionDetails =
                    JsonConvert.DeserializeObject<RemoteApiExceptionDetails>(response.Content);

                errorMessage = remoteApiExceptionDetails?.Message;
            }
            catch
            {
                //ignore
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                errorMessage =
                    $"{errorMessage}{(string.IsNullOrWhiteSpace(errorMessage) ? response.ErrorMessage : $" ({response.ErrorMessage})")}";
            }

            return $"{(int)response.StatusCode}: {response.StatusDescription}; {errorMessage}";
        }

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
