﻿using System;
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
        public async Task ExtractAndUploadDataAsync(Veiligstallen.BikeCounter.ApiClient.Service apiClient, EventHandler<string> msngr = null)
        {
            await ExtractDataAsync(msngr);
            await UploadDataAsync(apiClient, msngr);
        }

        public async Task ExtractAndUploadSurveyAreasFlatAsync(Service service, string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;
            Notify("Extracting survey areas...");
            await ExtractSurveyAreasFlatAsync(fName, separator, header);
            Notify("Survey areas extracted!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(service);
            Notify("Survey areas uploaded!");
        }

        public async Task ExtractAndUploadParkingLocationsFlatAsync(Service service, string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsFlatAsync(fName, separator, header);
            Notify("Parking locations extracted!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(service);
            Notify("Parking locations uploaded!");
        }

        public async Task ExtractAndUploadSectionsFlatAsync(Service service, string fName, FlatFileSeparator separator, bool header, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting sections...");
            await ExtractSectionsFlatAsync(fName, separator, header);
            Notify("Sections extracted!");

            Notify("Uploading sections...");
            await UploadSectionsAsync(service);
            Notify("Sections uploaded!");
        }

        public async Task LinkSurveyAreasToSurveysFlatAsync(Service service, string fName, FlatFileSeparator separator,
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


        public async Task ExtractAndUploadSurveyAreasShpAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;
            Notify("Extracting survey areas...");
            await ExtractSurveyAreasShpAsync(fName);
            Notify("Survey areas extracted!");

            Notify("Uploading survey areas...");
            await UploadSurveyAreasAsync(service);
            Notify("Survey areas uploaded!");
        }

        public async Task ExtractAndUploadParkingLocationsShpAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting parking locations...");
            await ExtractParkingLocationsShpAsync(fName);
            Notify("Parking locations extracted!");

            Notify("Uploading parking locations...");
            await UploadParkingLocationsAsync(service);
            Notify("Parking locations uploaded!");
        }

        public async Task ExtractAndUploadSectionsShpAsync(Service service, string fName, EventHandler<string> msngr)
        {
            _msngr = msngr;

            Notify("Extracting sections...");
            await ExtractSectionsShpAsync(fName);
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
