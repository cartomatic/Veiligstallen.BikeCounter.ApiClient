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
        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadStaticCompleteDataAsync(string dir, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader(dir);
            await staticDataLoader.ExtractAndUploadCompleteDataAsync(this, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSurveyAreasFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSurveyAreasFlatAsync(this, fName, separator, header, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadParkingLocationsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadParkingLocationsFlatAsync(this, fName, separator, header, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSectionsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSectionsFlatAsync(this, fName, separator, header, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSurveyAreasShpOnlyAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSurveyAreasShpOnlyAsync(this, fName, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadParkingLocationsShpOnlyAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadParkingLocationsShpOnlyAsync(this, fName, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSectionsShpOnlyAsync(string fName, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSectionsShpOnlyAsync(this, fName, msngr);
        }

        public async Task ExtractAndUploadSurveyAreasAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, bool updateMode = false, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSurveyAreasAsync(this, shpFile, flatFile, flatFileSeparator, updateMode, msngr);
        }

        public async Task ExtractAndUploadParkingLocationsAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, bool updateMode = false, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadParkingLocationsAsync(this, shpFile, flatFile, flatFileSeparator, updateMode, msngr);
        }

        public async Task ExtractAndUploadSectionsAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, bool updateMode = false, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadSectionsAsync(this, shpFile, flatFile, flatFileSeparator, updateMode, msngr);
        }

        public async Task LinkSurveyAreasToSurveysFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header, string surveyId, bool appendData, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.LinkSurveyAreasToSurveysFlatAsync(this, fName, separator, header, surveyId, appendData, msngr);
        }

        public async Task ExtractAndUploadCountingSheetAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header, string surveyId, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader();
            await staticDataLoader.ExtractAndUploadCountingSheetAsync(this, fName, separator, header, surveyId, msngr);
        }
    }
}
