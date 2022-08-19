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
        /// Gets a list of parking locations
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<ParkingLocation>> GetParkingLocationsAsync()
            => GetObjectsAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.PARKING_LOCATIONS));

        /// <summary>
        /// Gets a parking location by id
        /// </summary>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task<ParkingLocation> GetParkingLocationAsync(string parkingLocationId)
            => GetObjectAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.PARKING_LOCATION, parkingLocationId));

        /// <summary>
        /// Creates a parking location
        /// </summary>
        /// <param name="parkingLocation"></param>
        /// <returns></returns>
        public Task<ParkingLocation> CreateParkingLocationAsync(ParkingLocation parkingLocation)
            => CreateObjectAsync(new RequestConfig<ParkingLocation>(Configuration.Routes.PARKING_LOCATIONS, @object: parkingLocation));

        /// <summary>
        /// Updates a parking location
        /// </summary>
        /// <param name="parkingLocation"></param>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task<ParkingLocation> UpdateParkingLocationAsync(ParkingLocation parkingLocation, string parkingLocationId = null)
            => CreateObjectAsync(new RequestConfig<ParkingLocation>(Configuration.Routes.PARKING_LOCATION, parkingLocationId ?? parkingLocation.Id, @object: parkingLocation));

        /// <summary>
        /// Deletes a parking location
        /// </summary>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task DeleteParkingLocationAsync(string parkingLocationId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.PARKING_LOCATION, parkingLocationId));
    }
}
