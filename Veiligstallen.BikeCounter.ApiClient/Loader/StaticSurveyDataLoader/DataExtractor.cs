using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    public partial class StaticSurveyDataLoader
    {
        private List<ParkingLocation> _parkingLocations;
        private List<SurveyArea> _surveyAreas;
        private List<Section> _sections;

        private async Task ExtractParkingLocationsAsync()
        {
            _parkingLocations = ExtractParkingLocations();
            ExtractParkingLocationsGeoms(_parkingLocations);
        }

        private async Task ExtractSurveyAreasAsync()
        {
            _surveyAreas = ExtractSurveyAreas();
            ExtractSurveyAreasGeoms(_surveyAreas);
        }


        private async Task ExtractSectionsAsync()
        {
            _sections = ExtractSections();
            ExtractSectionsGeoms(_sections);
        }

        private void DisposeExtractedData()
        {
            _surveyAreas?.Clear();
            _surveyAreas = null;

            _parkingLocations?.Clear();
            _parkingLocations = null;
        }

    }
}
