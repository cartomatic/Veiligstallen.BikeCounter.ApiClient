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
        private const string ANONYMOUS_ORG = "anonymous";

        /// <summary>
        /// Gets a list of organizations
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Task<(IEnumerable<Organization> data, int total)> GetOrganizationsAsync(Dictionary<string, string> queryParams)
            => GetObjectsAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATIONS)
            {
                QueryParams = queryParams
            });

        /// <summary>
        /// Gets organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<Organization> GetOrganizationAsync(string organizationId)
            => organizationId == ANONYMOUS_ORG
                ? Task.FromResult(new Organization
                {
                    Id = ANONYMOUS_ORG,
                    Name = ANONYMOUS_ORG
                })
                : GetObjectAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATION, organizationId));
    }
}
