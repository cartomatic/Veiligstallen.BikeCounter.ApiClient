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
        /// Gets a list of organizations
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
        {
            var authHdr = GetAuthorizationHeaderValue(_user, _pass);

            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<List<Organization>>(_cfg.Endpoint,
                Configuration.Routes.ORGANIZATIONS, Method.GET, authToken: authHdr);

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output ?? new List<Organization>();
        }

        /// <summary>
        /// Gets organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task<Organization> GetOrganizationAsync(string organizationId)
        {
            var authHdr = GetAuthorizationHeaderValue(_user, _pass);

            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<Organization>(_cfg.Endpoint,
                $"{Configuration.Routes.ORGANIZATIONS}/{organizationId}", Method.GET, authToken: authHdr);

            EnsureValidResponse(apiOut.Response);

            return apiOut.Output;
        }
    }
}
