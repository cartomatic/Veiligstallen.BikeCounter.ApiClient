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
        /// <param name="orderBy"></param>
        /// <param name="order"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Task<(IEnumerable<Organization> data, int total)> GetOrganizationsAsync(string orderBy, string? order = null, int? offset = null, int? limit = null)
            => GetObjectsAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATIONS)
            {
                OrderBy = orderBy ?? RequestConfig.DFLT_ORDER_BY,
                //OrderDirection = order ?? RequestConfig.DFLT_ORDER,
                Offset = offset ?? RequestConfig.DFLT_OFFSET,
                Limit = limit ?? RequestConfig.DFLT_LIMIT
            });

        /// <summary>
        /// Gets organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public Task<Organization> GetOrganizationAsync(string organizationId)
            => GetObjectAsync<Organization>(new RequestConfig(Configuration.Routes.ORGANIZATION, organizationId));
    }
}
