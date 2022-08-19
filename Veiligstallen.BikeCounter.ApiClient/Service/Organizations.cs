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
        public Task<IEnumerable<Organization>> GetOrganizationsAsync()
            => GetObjectsAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATIONS));

        /// <summary>
        /// Gets organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<Organization> GetOrganizationAsync(string organizationId)
            => GetObjectAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATION, organizationId));
    }
}
