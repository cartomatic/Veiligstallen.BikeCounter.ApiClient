using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public async Task ExtractAndUploadData(string dir, EventHandler<string> msngr = null)
        {
            using var staticDataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader(dir);
            await staticDataLoader.ExtractAndUploadData(this, msngr);
        }
    }
}
