using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    public partial class StaticSurveyDataLoader
    {
        private List<SurveyArea> _surveyAreas;

        private async Task LoadSurveyAreasAsync()
        {
            _surveyAreas = ExtractSurveyAreas();

            ExtractSurveyAreasGeoms(_surveyAreas);


            //upload - parents first, then children
        }


        private void DisposeSurveyAreas()
        {
            _surveyAreas?.Clear();
            _surveyAreas = null;
        }
    }
}
