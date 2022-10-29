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
        /// Gets a list of canonical vehicle categories
        /// </summary>
        /// <returns></returns>
        public Task<(IEnumerable<CanonicalVehicleCategory> data, int total)> GetCanonicalVehicleCategoriesAsync()
            => GetObjectsAsync<CanonicalVehicleCategory>(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLE_CATEGORIES));

        /// <summary>
        /// Gets a canonical vehicle category by id
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <returns></returns>
        public Task<CanonicalVehicleCategory> GetCanonicalVehicleCategoryAsync(string canonicalVehicleCategoryId)
            => GetObjectAsync<CanonicalVehicleCategory>(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLE_CATEGORY, canonicalVehicleCategoryId));

        /// <summary>
        /// Creates a canonical vehicle category
        /// </summary>
        /// <param name="canonicalVehicleCategory"></param>
        /// <returns></returns>
        public Task<CanonicalVehicleCategory> CreateCanonicalVehicleCategoryAsync(CanonicalVehicleCategory canonicalVehicleCategory)
            => CreateObjectAsync(new RequestConfig<CanonicalVehicleCategory>(Configuration.Routes.CANONICAL_VEHICLE_CATEGORIES, @object: canonicalVehicleCategory));

        //update via delete create

        /// <summary>
        /// Deletes a canonical vehicle category
        /// </summary>
        /// <param name="canonicalVehicleCategoryId"></param>
        /// <returns></returns>
        public Task DeleteCanonicalVehicleCategoryAsync(string canonicalVehicleCategoryId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.CANONICAL_VEHICLE_CATEGORY, canonicalVehicleCategoryId));
    }
}
