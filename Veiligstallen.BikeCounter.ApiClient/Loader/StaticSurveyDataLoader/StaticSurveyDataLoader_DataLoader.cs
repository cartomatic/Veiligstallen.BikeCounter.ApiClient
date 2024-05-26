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
            var count = _surveyAreas.Count;
            var counter = 0;

            //need to upload parents first and then children so can assign parent ids 
            foreach (var rawParent in _surveyAreas.Where(sa => string.IsNullOrWhiteSpace(sa.ParentLocalId)).ToArray())
            {
                counter++;
                NotifyProgress(CalculateProgress(counter, count));

                try
                {
                    Notify($"{(string.IsNullOrWhiteSpace(rawParent.Id) ? "Uploading" : "Updating")} parent survey area: {rawParent.LocalId}...");
                    var parent = string.IsNullOrWhiteSpace(rawParent.Id)
                        ? await apiClient.CreateSurveyAreaAsync(rawParent)
                        : await apiClient.UpdateSurveyAreaAsync(rawParent, rawParent.Id);

                    rawParent.Id = parent.Id;
                }
                catch (System.Exception ex)
                {
                    Notify($"[ERROR] {ex.Message}; skipping parent survey area: {rawParent.LocalId}...");
                }
                
            }

            //children
            foreach (var rawChild in _surveyAreas.Where(sa => !string.IsNullOrWhiteSpace(sa.ParentLocalId)).ToArray())
            {
                counter++;
                NotifyProgress(CalculateProgress(counter, count));

                var parent = _surveyAreas.FirstOrDefault(sa => sa.LocalId == rawChild.ParentLocalId);
                if (parent == null)
                {
                    //no parent in the data set - try to look it up in a remote repo
                    parent = await apiClient.GetSurveyAreaByLocalIdAsync(rawChild.Authority, rawChild.ParentLocalId);
                }

                if (parent == null)
                {
                    Notify($"Could not find parent: {rawChild.ParentLocalId} for child survey area: {rawChild.LocalId}; skipping...");
                    continue;
                }

                try
                {
                    Notify($"{(string.IsNullOrWhiteSpace(rawChild.Id) ? "Uploading" : "Updating")} child survey area: {rawChild.LocalId} for parent: {rawChild.ParentLocalId}...");

                    rawChild.Parent = parent.Id;
                    var child = string.IsNullOrWhiteSpace(rawChild.Id) 
                        ? await apiClient.CreateSurveyAreaAsync(rawChild)
                        : await apiClient.UpdateSurveyAreaAsync(rawChild, rawChild.Id);
                }
                catch (System.Exception ex)
                {
                    Notify($"[ERROR] {ex.Message}; skipping child survey area: {rawChild.LocalId} for parent: {rawChild.ParentLocalId}...");
                }
            }
        }

        /// <summary>
        /// Uploads parking locations
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        private async Task UploadParkingLocationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            var count = _parkingLocations.Count(pl => pl.GeoLocation != null);
            var counter = 0;
            foreach (var parkingLocation in _parkingLocations.Where(pl => pl.GeoLocation != null))
            {
                counter++;
                NotifyProgress(CalculateProgress(counter, count));

                try
                {
                    Notify($"{(string.IsNullOrWhiteSpace(parkingLocation.Id) ? "Uploading" : "Updating")} parking location: {parkingLocation.LocalId}...");

                    var pl = string.IsNullOrWhiteSpace(parkingLocation.Id)
                        ? await apiClient.CreateParkingLocationAsync(parkingLocation)
                        : await apiClient.UpdateParkingLocationAsync(parkingLocation, parkingLocation.Id);

                    parkingLocation.Id = pl.Id;
                }
                catch (System.Exception ex)
                {
                    Notify($"[ERROR] {ex.Message}; skipping parking location: {parkingLocation.LocalId}...");
                }
            }
        }

        /// <summary>
        /// Uploads sections
        /// </summary>
        /// <param name="apiClient"></param>
        /// <returns></returns>
        private async Task UploadSectionsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            var count = _sections.Count;
            var counter = 0;
            foreach (var section in _sections)
            {
                counter++;
                NotifyProgress(CalculateProgress(counter, count));
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

                try
                {
                    Notify($"{(string.IsNullOrWhiteSpace(section.Id) ? "Uploading" : "Updating")} section: {section.LocalId} for parking location: {parkingLocation.LocalId}...");

                    section.ParkingLocation = parkingLocation.Id;
                    var s = string.IsNullOrWhiteSpace(section.Id)
                        ? await apiClient.CreateSectionAsync(section)
                        : await apiClient.UpdateSectionAsync(section, section.Id);
                }
                catch (System.Exception ex)
                {
                    Notify($"[ERROR] {ex.Message}; skipping section: {section.LocalId} for parking location: {parkingLocation.LocalId}...");
                }
            }
        }

        private async Task UploadObservationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            var count = _observations.Count;
            var counter = 0;
            foreach (var observation in _observations)
            {
                counter++;
                NotifyProgress(CalculateProgress(counter, count));
                try
                {
                    Notify($"Uploading observation {counter} of {_observations.Count}...");

                    if (observation.HasInvalidTimeStampStart() || observation.HasInvalidTimeStampStartFormat() || observation.HasInvalidTimeStampEndFormat())
                    {
                        var notification =
                            $"Empty start date detected in '{observation.ObservedProperty}.{nameof(Observation.TimestampStart)}' observation for section {observation.SectionLocalId}; skipping...";

                        if (observation.HasInvalidTimeStampStartFormat())
                        {
                            notification =
                                $"Invalid date format for {nameof(Observation.TimestampStart)}: '{observation.GetTimeStartStr()}'";
                        }
                        else if (observation.HasInvalidTimeStampEndFormat())
                        {
                            notification =
                                $"Invalid date format for {nameof(Observation.TimestampEnd)}: '{observation.GetTimeEndStr()}'";
                        }

                        Notify(notification);

                        continue;
                    }

                    if (string.IsNullOrEmpty(observation.FeatureOfInterest))
                    {
                        var section = await apiClient.GetSectionByLocalIdAsync(observation.SectionLocalId);
                        observation.FeatureOfInterest = section?.Id;
                    }

                    if (string.IsNullOrEmpty(observation.FeatureOfInterest))
                    {
                        Notify($"Could not find feature of interest for observation; skipping...");
                        continue;
                    }
                    
                    var _ = await apiClient.CreateObservationAsync(observation);
                }
                catch (System.Exception ex)
                {
                    Notify($"[ERROR] {ex.Message}; skipping observation {counter} of {_observations.Count}...");
                }
                
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
