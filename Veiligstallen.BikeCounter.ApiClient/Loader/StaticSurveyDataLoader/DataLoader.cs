﻿using System;
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
                if(parent == null)
                    continue;

                Notify($"Uploading child survey area: {rawChild.LocalId} for parent: {rawChild.ParentLocalId}...");

                rawChild.Parent = parent.Id;
                var child = await apiClient.CreateSurveyAreaAsync(rawChild);
            }
        }

        private async Task UploadParkingLocationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            foreach (var parkingLocation in _parkingLocations.Where(pl => pl.GeoLocation != null))
            {
                Notify($"Uploading parking location: {parkingLocation.LocalId}...");

                var pl = await apiClient.CreateParkingLocationAsync(parkingLocation);
                parkingLocation.Id = pl.Id;
            }
        }


        private async Task UploadSectionsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            foreach (var section in _sections)
            {
                var parkingLocation = _parkingLocations.FirstOrDefault(pl =>
                    pl.LocalId == section.ParkingLocationLocalId && pl.Authority == section.Authority && !string.IsNullOrWhiteSpace(pl.Id));

                if(parkingLocation == null)
                    continue;

                Notify($"Uploading section: {section.LocalId} for parking location: {parkingLocation.LocalId}...");

                section.ParkingLocation = parkingLocation.Id;
                var s = await apiClient.CreateSectionAsync(section);
            }
        }

    }
}
