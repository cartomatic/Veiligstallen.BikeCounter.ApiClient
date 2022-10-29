using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veiligstallen.BikeCounter.ApiClient.Loader;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        /// <summary>
        /// Extracts data from excel file & shp files and uploads it to crow wpi
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="msngr"></param>
        /// <returns></returns>
        public async Task ExtractAndUploadStaticDataAsync(string dir, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader(dir);
            await staticDataLoader.ExtractAndUploadDataAsync(this, msngr);
        }

        public async Task ExtractAndUploadSurveyAreasFlatAsync(string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSurveyAreasFlatAsync(this, fName, separator, header, msngr);
        }

        public async Task ExtractAndUploadParkingLocationsFlatAsync(string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadParkingLocationsFlatAsync(this, fName, separator, header, msngr);
        }

        public async Task ExtractAndUploadSectionsFlatAsync(string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSectionsFlatAsync(this, fName, separator, header, msngr);
        }

        public async Task ExtractAndUploadSurveyAreasShpAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSurveyAreasShpAsync(this, fName, msngr);
        }

        public async Task ExtractAndUploadParkingLocationsShpAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadParkingLocationsShpAsync(this, fName, msngr);
        }

        public async Task ExtractAndUploadSectionsShpAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSectionsShpAsync(this, fName, msngr);
        }

    }
}
