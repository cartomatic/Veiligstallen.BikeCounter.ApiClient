using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.Sig;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using RestSharp;
using RestSharp.Serializers;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.Loader;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public partial class Service
    {
        /// <summary>
        /// Gets a list of survey areas
        /// </summary>
        /// <returns></returns>
        public Task<(IEnumerable<SurveyArea> data, int total)> GetSurveyAreasAsync(Dictionary<string, string> queryParams)
            => GetObjectsAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREAS){QueryParams = queryParams});

        /// <summary>
        /// Gets a survey area by id
        /// </summary>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task<SurveyArea> GetSurveyAreaAsync(string surveyAreaId)
            => GetObjectAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREA, surveyAreaId));

        /// <summary>
        /// Creates a survey area
        /// </summary>
        /// <param name="surveyArea"></param>
        /// <returns></returns>
        public Task<SurveyArea> CreateSurveyAreaAsync(SurveyArea surveyArea)
            => CreateObjectAsync(new RequestConfig<SurveyArea>(Configuration.Routes.SURVEY_AREAS, @object: surveyArea));

        /// <summary>
        /// Updates a survey area
        /// </summary>
        /// <param name="surveyArea"></param>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task<SurveyArea> UpdateSurveyAreaAsync(SurveyArea surveyArea, string surveyAreaId = null)
            => UpdateObjectAsync(new RequestConfig<SurveyArea>(Configuration.Routes.SURVEY_AREA, surveyAreaId ?? surveyArea.Id, @object: surveyArea));

        /// <summary>
        /// Deletes a survey area
        /// </summary>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task DeleteSurveyAreaAsync(string surveyAreaId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SURVEY_AREA, surveyAreaId));


        /// <summary>
        /// Gets a survey area by authority and local id
        /// </summary>
        /// <param name="authority"></param>
        /// <param name="localId"></param>
        /// <returns></returns>
        public async Task<SurveyArea> GetSurveyAreaByLocalIdAsync(string authority, string localId)
        {
            var apiOut = await Cartomatic.Utils.RestApi.RestApiCall<SurveyArea[]>(
                _cfg.Endpoint,
                Configuration.Routes.SURVEY_AREAS,
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


        /// <summary>
        /// Prepares survey data for download
        /// </summary>
        /// <param name="queryParams"></param>
        /// <param name="outDir"></param>
        /// <returns></returns>
        public async Task<Guid> PrepareSurveyAreasDownloadAsync(Dictionary<string, string> queryParams, FlatFileUtils.FlatFileSeparator separator, string outDir)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException("No XLSX support for this data output");

            queryParams ??= new Dictionary<string, string>();

            //reset paging
            queryParams["start"] = "0";

            //set by the client, but in a case, this is not provided, enforce something large...
            if(!queryParams.ContainsKey("limit") || string.IsNullOrWhiteSpace(queryParams["limit"]) || !int.TryParse(queryParams["limit"], out _))
                queryParams["limit"] = "100000";
            
            var (data, _) = await GetObjectsAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREAS) { QueryParams = queryParams });

            var downloadId = Guid.NewGuid();
            var fileName = "survey-areas";

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
                string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), new []
                {
                    nameof(SurveyArea.Id),
                    nameof(SurveyArea.LocalId),
                    //nameof(SurveyArea.ParentLocalId),
                    nameof(SurveyArea.Name),
                    nameof(SurveyArea.Parent),
                    nameof(SurveyArea.ValidFrom),
                    nameof(SurveyArea.ValidThrough),
                    nameof(SurveyArea.Authority),
                    nameof(SurveyArea.XtraInfo),
                    nameof(SurveyArea.SurveyAreaType),
                    nameof(SurveyArea.GeoLocation),
                    Environment.NewLine
                })
            );

            //content
            foreach (var surveyArea in data)
            {
                await sw.WriteAsync(
                    string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), new[]
                    {
                        surveyArea.Id,
                        surveyArea.LocalId,
                        //surveyArea.ParentLocalId,
                        surveyArea.Name,
                        surveyArea.Parent,
                        surveyArea.ValidFrom?.ToString("yyyy-MM-dd"), //surveyArea.ValidFrom?.ToString("O"),
                        surveyArea.ValidThrough?.ToString("yyyy-MM-dd"), //surveyArea.ValidThrough?.ToString("O"),
                        surveyArea.Authority,
                        surveyArea.XtraInfo,
                        surveyArea.SurveyAreaType,
                        $"{_cfg.ShapeEndpoint}{_cfg.Endpoint}/{Configuration.Routes.SURVEY_AREAS}/{surveyArea.Id}",
                        Environment.NewLine
                    })
                );

                try
                {
                    using var stringReader = new StringReader(JsonConvert.SerializeObject(surveyArea.GeoLocation));
                    using var jsonReader = new JsonTextReader(stringReader);
                    var geom = geoJsonSerializer.Deserialize<NetTopologySuite.Geometries.Geometry>(jsonReader);

                    var attributes = new AttributesTable
                    {
                        {ToShpColName(nameof(SurveyArea.Id)), surveyArea.Id ?? string.Empty},
                        {ToShpColName(nameof(SurveyArea.LocalId)), surveyArea.LocalId ?? string.Empty},
                        //{ToShpColName(nameof(SurveyArea.ParentLocalId)), surveyArea.ParentLocalId},
                        {ToShpColName(nameof(SurveyArea.Name)), surveyArea.Name ?? string.Empty},
                        {ToShpColName(nameof(SurveyArea.Parent)), surveyArea.Parent ?? string.Empty},
                        {ToShpColName(nameof(SurveyArea.ValidFrom)), surveyArea.ValidFrom ?? default},
                        {ToShpColName(nameof(SurveyArea.ValidThrough)), surveyArea.ValidThrough ?? default},
                        {ToShpColName(nameof(SurveyArea.Authority)), surveyArea.Authority ?? string.Empty},
                        {ToShpColName(nameof(SurveyArea.XtraInfo)), surveyArea.XtraInfo ?? string.Empty},
                        {ToShpColName(nameof(SurveyArea.SurveyAreaType)), surveyArea.SurveyAreaType ?? string.Empty}
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
            System.IO.Compression.ZipFile.CreateFromDirectory(tmpDir, Path.Combine(outDir, $"{downloadId}.zip"),CompressionLevel.Fastest, false);


            //cleanup
            Directory.Delete(tmpDir, true);

            return downloadId;
        }
    }
}
