using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Exception;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {

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
    }
}
