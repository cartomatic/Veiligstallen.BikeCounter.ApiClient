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

        private async Task ExtractParkingLocationsAsync()
        {
            _parkingLocations = ExtractParkingLocations();
            ExtractParkingLocationsGeoms(_parkingLocations);
        }

        private async Task UploadParkingLocationsAsync()
        {

        }

        private void DisposeParkingLocations()
        {
            _parkingLocations?.Clear();
            _parkingLocations = null;
        }
    }
}
