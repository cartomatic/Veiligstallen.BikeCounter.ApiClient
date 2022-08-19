using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {

        /// <summary>
        /// Resets environment - cleans up all the data
        /// </summary>
        /// <returns></returns>
        /// <remarks>This is a convenience method for working with the test environment; API will not support it in the production</remarks>
        public async Task<bool> ResetEnvironmentAsync()
        {
            var authHdr = GetAuthorizationHeaderValue(_user, _pass);

            //basically a reset means wiping out all the data by calling 3 endpoints one by one
            var r1 = await Cartomatic.Utils.RestApi.RestApiCall(_cfg.Endpoint, Configuration.Routes.SECTIONS, Method.DELETE, authToken: authHdr);
            var r2 = await Cartomatic.Utils.RestApi.RestApiCall(_cfg.Endpoint, Configuration.Routes.PARKING_FACILTIES, Method.DELETE, authToken: authHdr);
            var r3 = await Cartomatic.Utils.RestApi.RestApiCall(_cfg.Endpoint, Configuration.Routes.SURVEY_AREAS, Method.DELETE, authToken: authHdr);

            //not found is a test for the time being
            return r1.IsSuccessful && r2.IsSuccessful && r3.IsSuccessful || r1.StatusCode == HttpStatusCode.NotFound && r2.StatusCode == HttpStatusCode.NotFound && r3.StatusCode == HttpStatusCode.NotFound;
        }

    }
}
