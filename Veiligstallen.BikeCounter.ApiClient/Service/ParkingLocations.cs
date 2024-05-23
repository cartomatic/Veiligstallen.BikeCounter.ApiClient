using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Esri;
using Newtonsoft.Json;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Loader;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        /// <summary>
        /// Gets a list of parking locations
        /// </summary>
        /// <returns></returns>
        public Task<(IEnumerable<ParkingLocation> data, int total)> GetParkingLocationsAsync(Dictionary<string, string> queryParams)
            => GetObjectsAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.PARKING_LOCATIONS){QueryParams = queryParams});

        /// <summary>
        /// Gets a parking location by id
        /// </summary>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task<ParkingLocation> GetParkingLocationAsync(string parkingLocationId)
            => GetObjectAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.PARKING_LOCATION, parkingLocationId));

        /// <summary>
        /// Creates a parking location
        /// </summary>
        /// <param name="parkingLocation"></param>
        /// <returns></returns>
        public Task<ParkingLocation> CreateParkingLocationAsync(ParkingLocation parkingLocation)
            => CreateObjectAsync(new RequestConfig<ParkingLocation>(Configuration.Routes.PARKING_LOCATIONS, @object: parkingLocation));

        /// <summary>
        /// Updates a parking location
        /// </summary>
        /// <param name="parkingLocation"></param>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task<ParkingLocation> UpdateParkingLocationAsync(ParkingLocation parkingLocation, string parkingLocationId = null)
            => CreateObjectAsync(new RequestConfig<ParkingLocation>(Configuration.Routes.PARKING_LOCATION, parkingLocationId ?? parkingLocation.Id, @object: parkingLocation));

        /// <summary>
        /// Deletes a parking location
        /// </summary>
        /// <param name="parkingLocationId"></param>
        /// <returns></returns>
        public Task DeleteParkingLocationAsync(string parkingLocationId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.PARKING_LOCATION, parkingLocationId));


        /// <summary>
        /// Gets a parking location by authority and local id
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="localId"></param>
        /// <returns></returns>
        public async Task<ParkingLocation> GetParkingLocationAsync(string authority, string localId)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<ParkingLocation[]>(
                _cfg.Endpoint,
                Configuration.Routes.PARKING_LOCATIONS,
                Method.GET,
                authToken: GetAuthorizationHeaderValue(),
                queryParams: new Dictionary<string, object>
                {
                    {nameof(authority), authority},
                    {nameof(localId), localId}
                }
            );

            return apiOut.Response.IsSuccessful
                ? apiOut.Output?.FirstOrDefault()
                : null;
        }

        public async Task<Guid> PrepareParkingLocationsDownloadAsync(Dictionary<string, string> queryParams, FlatFileUtils.FlatFileSeparator separator, string outDir)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException("No XLSX support for this data output");

            queryParams ??= new Dictionary<string, string>();

            //reset paging
            queryParams["start"] = "0";

            //set by the client, but in a case, this is not provided, enforce something large...
            if (!queryParams.ContainsKey("limit") || string.IsNullOrWhiteSpace(queryParams["limit"]) || !int.TryParse(queryParams["limit"], out _))
                queryParams["limit"] = "100000";

            var (data, _) = await GetObjectsAsync<ParkingLocation>(new RequestConfig(Configuration.Routes.PARKING_LOCATIONS) { QueryParams = queryParams });

            var downloadId = Guid.NewGuid();
            var fileName = "parking-locations";

            //tmp working dir
            var tmpDir = Path.Combine(outDir, downloadId.ToString());
            Directory.CreateDirectory(tmpDir);

            //required for the proper dbf output
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var geoJsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();

            //flat file
            using var fs = System.IO.File.OpenWrite(Path.Combine(tmpDir, $"{fileName}.csv"));
            using var sw = new StreamWriter(fs);

            //shp featureset
            var features = new List<Feature>();

            //hdr
            await sw.WriteAsync(
                string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), new[]
                {
                    nameof(ParkingLocation.Id),
                    nameof(ParkingLocation.LocalId),
                    nameof(ParkingLocation.Name),
                    nameof(ParkingLocation.Parent),
                    nameof(ParkingLocation.ValidFrom),
                    nameof(ParkingLocation.ValidThrough),
                    nameof(ParkingLocation.Authority),
                    nameof(ParkingLocation.XtraInfo),
                    nameof(ParkingLocation.Features),
                    nameof(ParkingLocation.Number),
                    nameof(ParkingLocation.GeoLocation),
                    Environment.NewLine
                })
            );

            //content
            foreach (var parkingLocation in data)
            {
                await sw.WriteAsync(
                    string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), new[]
                    {
                        parkingLocation.Id,
                        parkingLocation.LocalId,
                        parkingLocation.Name,
                        parkingLocation.Parent,
                        parkingLocation.ValidFrom?.ToString("yyyy-MM-dd"), //parkingLocation.ValidFrom?.ToString("O"),
                        parkingLocation.ValidThrough?.ToString("yyyy-MM-dd"), //parkingLocation.ValidThrough?.ToString("O"),
                        parkingLocation.Authority,
                        parkingLocation.XtraInfo,
                        string.Join(", ", parkingLocation.Features ?? Array.Empty<string>()),
                        parkingLocation.Number,
                        $"{_cfg.ShapeEndpoint}{_cfg.Endpoint}/{Configuration.Routes.PARKING_LOCATIONS}/{parkingLocation.Id}",
                        Environment.NewLine
                    })
                );

                try
                {
                    using var stringReader = new StringReader(JsonConvert.SerializeObject(parkingLocation.GeoLocation));
                    using var jsonReader = new JsonTextReader(stringReader);
                    var geom = geoJsonSerializer.Deserialize<NetTopologySuite.Geometries.Geometry>(jsonReader);

                    var attributes = new AttributesTable
                    {
                        {ToShpColName(nameof(ParkingLocation.Id)), parkingLocation.Id ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.LocalId)), parkingLocation.LocalId ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.Name)), parkingLocation.Name ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.Parent)), parkingLocation.Parent ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.ValidFrom)), parkingLocation.ValidFrom ?? default},
                        {ToShpColName(nameof(ParkingLocation.ValidThrough)), parkingLocation.ValidThrough ?? default},
                        {ToShpColName(nameof(ParkingLocation.Authority)), parkingLocation.Authority ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.XtraInfo)), parkingLocation.XtraInfo ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.Features)), string.Join(", ", parkingLocation.Features ?? Array.Empty<string>()) ?? string.Empty},
                        {ToShpColName(nameof(ParkingLocation.Number)), parkingLocation.Number ?? string.Empty}
                    };

                    var feature = new Feature(geom, attributes);
                    features.Add(feature);
                }
                catch
                {
                    //ignore
                }
            }

            await sw.FlushAsync();
            sw.Close();

            //shp...
            Shapefile.WriteAllFeatures(features, Path.Combine(tmpDir, $"{fileName}.shp"));

            //wrap all...
            System.IO.Compression.ZipFile.CreateFromDirectory(tmpDir, Path.Combine(outDir, $"{downloadId}.zip"), CompressionLevel.Fastest, false);


            //cleanup
            Directory.Delete(tmpDir, true);

            return downloadId;
        }
    }
}
