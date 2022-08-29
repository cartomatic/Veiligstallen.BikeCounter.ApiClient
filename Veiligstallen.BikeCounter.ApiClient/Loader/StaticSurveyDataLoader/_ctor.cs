using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    public partial class StaticSurveyDataLoader : IDisposable
    {
        private const string FILENAME_EXCEL = "Static";
        private const string FILENAME_PARKING_LOCATION = "ParkingLocation";
        private const string FILENAME_SECTIONS = "Sections";
        private const string FILENAME_SURVEY_AREA = "SurveyArea";

        private readonly string _dir;
        private EventHandler<string> _msngr;

        public StaticSurveyDataLoader(string dir)
        {
            _dir = dir;
        }

        /// <summary>
        /// Extracts data from excel file & shp files
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        public async Task ExtractDataAsync(EventHandler<string> msngr = null)
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
                    $"{FILENAME_PARKING_LOCATION}.{ext}",
                    $"{FILENAME_SECTIONS}.{ext}",
                    $"{FILENAME_SURVEY_AREA}.{ext}"
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
