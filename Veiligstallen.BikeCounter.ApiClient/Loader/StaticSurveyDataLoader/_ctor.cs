using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MimeKit.Cryptography;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader : IDisposable
    {
        private const string FILENAME_EXCEL = "Static";
        private const string FILENAME_PARKING_LOCATIONS = "ParkingLocation";
        private const string FILENAME_SECTIONS = "Sections";
        private const string FILENAME_SURVEY_AREAS = "SurveyArea";

        private readonly string _dir;
        private readonly bool _extractWkt;
        private EventHandler<string> _msngr;
        private bool _dataExtracted;

        public StaticSurveyDataLoader(string dir, bool extractWkt = false)
        {
            _extractWkt = extractWkt;
            _dir = dir;
        }

        /// <summary>
        /// Extracts data from excel file & shp files and uploads it to crow wpi
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        public async Task ExtractAndUploadData(Veiligstallen.BikeCounter.ApiClient.Service apiClient, EventHandler<string> msngr = null)
        {
            await ExtractDataAsync(msngr);
            await UploadDataAsync(apiClient, msngr);
        }

        /// <summary>
        /// Extracts data from excel file & shp files
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        protected internal async Task ExtractDataAsync(EventHandler<string> msngr = null)
        {
            _msngr = msngr;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Notify("Validating files presence...");
            ValidateFilePresence();
            Notify("Files ok!");

            Notify("Loading & validating excel spreadsheet...");
            LoadExcelData();
            Notify("Excel ok!");

            Notify("Extracting survey areas...");
            await ExtractSurveyAreasAsync();
            Notify("Survey areas extracted!");

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsAsync();
            Notify("Parking locations extracted!");

            Notify("Extracting sections...");
            await ExtractSectionsAsync();
            Notify("Sections extracted!");

            _dataExtracted = true;
        }

        /// <summary>
        /// Uploads data to crow api
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task UploadDataAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient, EventHandler<string> msngr = null)
        {
            if (!_dataExtracted)
                throw new InvalidOperationException("Data needs to be xtracted before uploading!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(apiClient);
            Notify("Survey areas uploaded!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(apiClient);
            Notify("Parking locations uploaded!");

            Notify("Uploading sections...");
            await UploadSectionsAsync(apiClient);
            Notify("Sections uploaded!");

        }


        private void Notify(string msg)
            => _msngr?.Invoke(this, msg);


        /// <summary>
        /// Validates presence of the files required to perform a static data upload
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        private void ValidateFilePresence()
        {
            if (!Directory.Exists(_dir))
                throw new ArgumentException($"Directory does not exist: {_dir}");

            var fileNames = new List<string>
            {
                $"{FILENAME_EXCEL}.xlsx"
            };
            foreach (var ext in new[]{"shp", "dbf", "shx"})
            {
                fileNames.AddRange(new[]
                {
                    $"{FILENAME_PARKING_LOCATIONS}.{ext}",
                    $"{FILENAME_SECTIONS}.{ext}",
                    $"{FILENAME_SURVEY_AREAS}.{ext}"
                });
            }

            foreach (var fileName in fileNames)
            {
                if (!File.Exists(Path.Combine(_dir, fileName)))
                    throw new System.Exception($"Missing file: {fileName}");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _msngr = null;
            DisposeExcel();
            DisposeExtractedData();
        }
    }
}
