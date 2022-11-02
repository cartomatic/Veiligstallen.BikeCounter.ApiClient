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
        /// Gets a list of canonical vehicles
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <returns></returns>
        public Task<(IEnumerable<CanonicalVehicle> data, int total)> GetCanonicalVehiclesAsync(string canonicalVehicleCategoryId, Dictionary<string, string> queryParams)
            => GetObjectsAsync<CanonicalVehicle>(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLES, parentId: canonicalVehicleCategoryId) {QueryParams = queryParams});

        /// <summary>
        /// Gets a canonical vehicle by id
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <param name="canonicalVehicleCode"></param>
        /// <returns></returns>
        public Task<CanonicalVehicle> GetCanonicalVehicleAsync(string canonicalVehicleCategoryId, string canonicalVehicleCode)
            => GetObjectAsync<CanonicalVehicle>(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLE, parentId: canonicalVehicleCategoryId, objectId: canonicalVehicleCode));

        /// <summary>
        /// Creates a canonical vehicle
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <param name="canonicalVehicle"></param>
        /// <returns></returns>
        public Task<CanonicalVehicle> CreateCanonicalVehicleAsync(string canonicalVehicleCategoryId, CanonicalVehicle canonicalVehicle)
            => CreateObjectAsync(new RequestConfig<CanonicalVehicle>(Configuration.Routes.CANONICAL_VEHICLES, parentId: canonicalVehicleCategoryId, @object: canonicalVehicle));

        //update via delete create

        /// <summary>
        /// Deletes a canonical vehicle
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <param name="canonicalVehicleCode"></param>
        /// <returns></returns>
        public Task DeleteCanonicalVehicleAsync(string canonicalVehicleCategoryId, string canonicalVehicleCode)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLE, parentId: canonicalVehicleCategoryId, objectId: canonicalVehicleCode));
    }
}
