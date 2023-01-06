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
        private readonly string _dir;
        private readonly bool _extractWkt;
        private EventHandler<string> _msngr;
        private bool _dataExtracted;

        public StaticSurveyDataLoader()
        {
        }

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
        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadCompleteDataAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient, EventHandler<string> msngr = null)
        {
            await ExtractCompleteDataAsync(msngr);
            await UploadCompletedDataAsync(apiClient, msngr);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSurveyAreasFlatAsync(Service service, string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;
            Notify("Extracting survey areas...");
            await ExtractSurveyAreasFlatAsync(fName, separator, header);
            Notify("Survey areas extracted!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(service);
            Notify("Survey areas uploaded!");
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadParkingLocationsFlatAsync(Service service, string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsFlatAsync(fName, separator, header);
            Notify("Parking locations extracted!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(service);
            Notify("Parking locations uploaded!");
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSectionsFlatAsync(Service service, string fName, FlatFileUtils.FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting sections...");
            await ExtractSectionsFlatAsync(fName, separator, header);
            Notify("Sections extracted!");

            Notify("Uploading sections...");
            await UploadSectionsAsync(service);
            Notify("Sections uploaded!");
        }

        public async Task LinkSurveyAreasToSurveysFlatAsync(Service service, string fName, FlatFileUtils.FlatFileSeparator separator,
            bool header, string surveyId, bool appendData, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting survey areas ids...");
            var surveyAreasIds = await ExtractSurveyAreasIdsFlatAsync(fName, separator, header);
            Notify("Survey areas ids!");

            Notify($"Linking survey areas to survey {surveyId}...");
            await LinkSurveyAreasToSurveyAsync(service, surveyId, surveyAreasIds, appendData);
            Notify("Survey areas linked!");
        }


        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSurveyAreasShpOnlyAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;
            Notify("Extracting survey areas...");
            await ExtractSurveyAreasShpOnlyAsync(fName);
            Notify("Survey areas extracted!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(service);
            Notify("Survey areas uploaded!");
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadParkingLocationsShpOnlyAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsShpOnlyAsync(fName);
            Notify("Parking locations extracted!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(service);
            Notify("Parking locations uploaded!");
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        public async Task ExtractAndUploadSectionsShpOnlyAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting sections...");
            await ExtractSectionsShpOnlyAsync(fName);
            Notify("Sections extracted!");

            Notify("Uploading sections...");
            await UploadSectionsAsync(service);
            Notify("Sections uploaded!");
        }


        public async Task ExtractAndUploadSurveyAreasAsync(Service service, string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, EventHandler<string> msngr)
        {
            _msngr = msngr;
            Notify("Extracting survey areas...");
            await ExtractSurveyAreasAsync(shpFile, flatFile, flatFileSeparator);
            Notify("Survey areas extracted!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(service);
            Notify("Survey areas uploaded!");
        }

        public async Task ExtractAndUploadParkingLocationsAsync(Service service, string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsAsync(shpFile, flatFile, flatFileSeparator);
            Notify("Parking locations extracted!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(service);
            Notify("Parking locations uploaded!");
        }

        public async Task ExtractAndUploadSectionsAsync(Service service, string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting sections...");
            await ExtractSectionsAsync(shpFile, flatFile, flatFileSeparator);
            Notify("Sections extracted!");

            Notify("Uploading sections...");
            await UploadSectionsAsync(service);
            Notify("Sections uploaded!");
        }


        /// <summary>
        /// Extracts data from excel file & shp files
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        [Obsolete("Format abandoned and not officially supported anymore")]
        protected internal async Task ExtractCompleteDataAsync(EventHandler<string> msngr = null)
        {
            _msngr = msngr;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Notify("Validating files presence...");
            ValidateCompleteDataFilePresence();
            Notify("Files ok!");

            Notify("Loading & validating excel spreadsheet...");
            LoadCompleteDataExcelData();
            Notify("Excel ok!");

            Notify("Extracting survey areas...");
            await ExtractCompleteDataSurveyAreasAsync();
            Notify("Survey areas extracted!");

            Notify("Extracting parking locations...");
            await ExtractCompleteDataParkingLocationsAsync();
            Notify("Parking locations extracted!");

            Notify("Extracting sections...");
            await ExtractCompleteDataSectionsAsync();
            Notify("Sections extracted!");

            _dataExtracted = true;
        }

        /// <summary>
        /// Uploads data to crow api
        /// </summary>
        /// <param name="msngr"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task UploadCompletedDataAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient, EventHandler<string> msngr = null)
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



        private const string COMPLETE_DATA_FILENAME_EXCEL = "Static";
        private const string COMPLETE_DATA_FILENAME_PARKING_LOCATIONS = "ParkingLocation";
        private const string COMPLETE_DATA_FILENAME_SECTIONS = "Sections";
        private const string COMPLETE_DATA_FILENAME_SURVEY_AREAS = "SurveyArea";

        /// <summary>
        /// Validates presence of the files required to perform a static data upload
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        [Obsolete("Format abandoned and not officially supported anymore")]
        private void ValidateCompleteDataFilePresence()
        {
            if (!Directory.Exists(_dir))
                throw new ArgumentException($"Directory does not exist: {_dir}");

            var fileNames = new List<string>
            {
                $"{COMPLETE_DATA_FILENAME_EXCEL}.xlsx"
            };
            foreach (var ext in new[]{"shp", "dbf", "shx"})
            {
                fileNames.AddRange(new[]
                {
                    $"{COMPLETE_DATA_FILENAME_PARKING_LOCATIONS}.{ext}",
                    $"{COMPLETE_DATA_FILENAME_SECTIONS}.{ext}",
                    $"{COMPLETE_DATA_FILENAME_SURVEY_AREAS}.{ext}"
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
            DisposeCompleteDataExcel();
            DisposeExtractedData();
        }
    }
}
