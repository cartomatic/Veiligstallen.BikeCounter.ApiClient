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
        /// Creates an observation
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Task<Observation> CreateObservationAsync(Observation observation)
            => CreateObjectAsync(new RequestConfig<Observation>(Configuration.Routes.OBSERVATIONS, @object: observation));
    }
}
