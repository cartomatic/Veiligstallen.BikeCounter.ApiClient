using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        /// <summary>
        /// Uploads survey areas
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        private async Task UploadSurveyAreasAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            //need to upload parents first and then children so can assign parent ids 
            foreach (var rawParent in _surveyAreas.Where(sa => string.IsNullOrWhiteSpace(sa.ParentLocalId)).ToArray())
            {
                Notify($"Uploading parent survey area: {rawParent.LocalId}...");
                var parent = await apiClient.CreateSurveyAreaAsync(rawParent);
                rawParent.Id = parent.Id;
            }

            //children
            foreach (var rawChild in _surveyAreas.Where(sa => !string.IsNullOrWhiteSpace(sa.ParentLocalId)).ToArray())
            {
                var parent = _surveyAreas.FirstOrDefault(sa => sa.LocalId == rawChild.ParentLocalId);
                if (parent == null)
                {
                    Notify($"Could not find parent: {rawChild.ParentLocalId} for child survey area: {rawChild.LocalId}; skipping ...");
                    continue;
                }

                Notify($"Uploading child survey area: {rawChild.LocalId} for parent: {rawChild.ParentLocalId}...");

                rawChild.Parent = parent.Id;
                var child = await apiClient.CreateSurveyAreaAsync(rawChild);
            }
        }

        /// <summary>
        /// Uploads parking locations
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        private async Task UploadParkingLocationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            foreach (var parkingLocation in _parkingLocations.Where(pl => pl.GeoLocation != null))
            {
                Notify($"Uploading parking location: {parkingLocation.LocalId}...");

                var pl = await apiClient.CreateParkingLocationAsync(parkingLocation);
                parkingLocation.Id = pl.Id;
            }
        }

        /// <summary>
        /// Uploads sections
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        private async Task UploadSectionsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            foreach (var section in _sections)
            {
                ParkingLocation parkingLocation = null;
                if (_parkingLocations != null)
                {
                    parkingLocation = _parkingLocations.FirstOrDefault(pl =>
                        pl.LocalId == section.ParkingLocationLocalId && pl.Authority == section.Authority && !string.IsNullOrWhiteSpace(pl.Id));
                }
                else
                {
                    parkingLocation =
                        await apiClient.GetParkingLocationAsync(section.Authority, section.ParkingLocationLocalId);
                }

                if (parkingLocation == null)
                {
                    Notify($"Could not find parking location: {section.ParkingLocationLocalId} for section: section: {section.LocalId}; skipping...");
                    continue;
                }

                Notify($"Uploading section: {section.LocalId} for parking location: {parkingLocation.LocalId}...");

                section.ParkingLocation = parkingLocation.Id;
                var s = await apiClient.CreateSectionAsync(section);
            }
        }

        private async Task UploadObservationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            var counter = 1;
            foreach (var observation in _observations)
            {
                Notify($"Uploading observation {counter} of {_observations.Count}...");
                
                var _ = await apiClient.CreateObservationAsync(observation);
                counter++;
            }
        }

        /// <summary>
        /// Links survey areas to survey
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="surveyId"></param>
        /// <param name="appendData"></param>
        /// <param name="surveyAreasIds"></param>
        /// <returns></returns>
        private async Task LinkSurveyAreasToSurveyAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient, string surveyId, IEnumerable<string> surveyAreasIds, bool appendData)
        {
            await apiClient.LinkSurveyAreasAsync(surveyId, surveyAreasIds, appendData);
        }

    }
}
