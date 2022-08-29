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
                var parent = await apiClient.CreateSurveyAreaAsync(rawParent);
                rawParent.Id = parent.Id;
            }

            //children
            foreach (var rawChild in _surveyAreas.Where(sa => !string.IsNullOrWhiteSpace(sa.ParentLocalId)).ToArray())
            {
                var parent = _surveyAreas.FirstOrDefault(sa => sa.LocalId == rawChild.ParentLocalId);
                if(parent == null)
                    continue;

                rawChild.Parent = parent.Id;

                var child = await apiClient.CreateSurveyAreaAsync(rawChild);
            }
        }

        private async Task UploadParkingLocationsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {
            
        }


        private async Task UploadSSectionsAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient)
        {

        }

    }
}
