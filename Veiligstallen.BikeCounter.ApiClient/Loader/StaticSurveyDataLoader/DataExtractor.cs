using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        private List<ParkingLocation> _parkingLocations;
        private List<SurveyArea> _surveyAreas;
        private List<Section> _sections;

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractCompleteDataParkingLocationsAsync()
        {
            _parkingLocations = ExtractCompleteDataParkingLocations();
            ExtractCompleteDataParkingLocationsGeoms(_parkingLocations);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractCompleteDataSurveyAreasAsync()
        {
            _surveyAreas = ExtractCompleteDataSurveyAreas();
            ExtractCompleteDataSurveyAreasGeoms(_surveyAreas);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractCompleteDataSectionsAsync()
        {
            _sections = ExtractCompleteDataSections();
            ExtractCompleteDataSectionsGeoms(_sections);
        }

        private void DisposeExtractedData()
        {
            _surveyAreas?.Clear();
            _surveyAreas = null;

            _parkingLocations?.Clear();
            _parkingLocations = null;

            _sections?.Clear();
            _sections = null;
        }
    }
}
