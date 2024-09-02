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
        /// Gets a list of sections
        /// </summary>
        /// <returns></returns>
        public Task<(IEnumerable<Section> data, int total)> GetSectionsAsync(Dictionary<string, string> queryParams)
            => GetObjectsAsync<Section>(new RequestConfig(Configuration.Routes.SECTIONS){QueryParams = queryParams});

        /// <summary>
        /// Gets a section by id
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public Task<Section> GetSectionAsync(string sectionId)
            => GetObjectAsync<Section>(new RequestConfig(Configuration.Routes.SECTION, sectionId));

        /// <summary>
        /// Gets a section by local Id
        /// </summary>
        /// <param name="localId"></param>
        /// <returns></returns>
        public async Task<Section> GetSectionByLocalIdAsync(string localId)
            => (await GetObjectAsync<Section[]>(new RequestConfig(Configuration.Routes.SECTIONS){QueryParams = new Dictionary<string, string>{{nameof(localId), localId}}})).FirstOrDefault();

        /// <summary>
        /// Creates a section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Task<Section> CreateSectionAsync(Section section)
            => CreateObjectAsync(new RequestConfig<Section>(Configuration.Routes.SECTIONS, @object: section));

        /// <summary>
        /// Updates a section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public Task<Section> UpdateSectionAsync(Section section, string sectionId = null)
            => UpdateObjectAsync(new RequestConfig<Section>(Configuration.Routes.SECTION, sectionId ?? section.Id, @object: section));

        /// <summary>
        /// Deletes a section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public Task DeleteSectionAsync(string sectionId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SECTION, sectionId));

        public async Task<Guid> PrepareSectionsDownloadAsync(Dictionary<string, string> queryParams, FlatFileUtils.FlatFileSeparator separator, string outDir)
        {
            if (separator == FlatFileUtils.FlatFileSeparator.Xlsx)
                throw new NotImplementedException("No XLSX support for this data output");

            queryParams ??= new Dictionary<string, string>();

            //reset paging
            queryParams["start"] = "0";

            //set by the client, but in a case, this is not provided, enforce something large...
            if (!queryParams.ContainsKey("limit") || string.IsNullOrWhiteSpace(queryParams["limit"]) || !int.TryParse(queryParams["limit"], out _))
                queryParams["limit"] = "100000";

            var (data, _) = await GetObjectsAsync<Section>(new RequestConfig(Configuration.Routes.SECTIONS) { QueryParams = queryParams });

            var downloadId = Guid.NewGuid();
            var fileName = "sections";

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
                    nameof(Section.Id),
                    nameof(Section.LocalId),
                    nameof(Section.Name),
                    nameof(Section.SurveyAreaParent),
                    nameof(Section.SurveyAreaChild),
                    nameof(Section.ParkingLocation),
                    
                    nameof(Section.Layout),
                    nameof(Section.ValidFrom),
                    nameof(Section.ValidThrough),
                    nameof(Section.Authority),
                    nameof(Section.Level),
                    nameof(Section.LevelSub),

                    nameof(Section.GeoLocation),
                    Environment.NewLine
                })
            );

            //content
            foreach (var section in data)
            {
                await sw.WriteAsync(
                    string.Join(FlatFileUtils.FlatFileSeparators[separator].ToString(), new[]
                    {
                        section.Id,
                        section.LocalId,
                        section.Name,
                        section.SurveyAreaParent,
                        section.SurveyAreaChild,
                        section.ParkingLocation,
                        section.Layout,
                        section.ValidFrom?.ToString("yyyy-MM-dd"), //section.ValidFrom?.ToString("O"),
                        section.ValidThrough?.ToString("yyyy-MM-dd"), //section.ValidThrough?.ToString("O"),
                        section.Authority,
                        section.Level?.ToString(),
                        section.LevelSub,
                        $"{_cfg.ShapeEndpoint}{_cfg.Endpoint}/{Configuration.Routes.SECTIONS}/{section.Id}",
                        Environment.NewLine
                    })
                );

                try
                {
                    using var stringReader = new StringReader(JsonConvert.SerializeObject(section.GeoLocation));
                    using var jsonReader = new JsonTextReader(stringReader);
                    var geom = geoJsonSerializer.Deserialize<NetTopologySuite.Geometries.Geometry>(jsonReader);

                    if (geom == null)
                        continue;    

                    var attributes = new AttributesTable
                    {
                        {ToShpColName(nameof(Section.Id)), section.Id ?? string.Empty},
                        {ToShpColName(nameof(Section.LocalId)), section.LocalId ?? string.Empty},
                        {ToShpColName(nameof(Section.Name)), section.Name ?? string.Empty},
                        {ToShpColName("SrvyArPrnt"), section.SurveyAreaParent ?? string.Empty},
                        {ToShpColName("SrvyArChld"), section.SurveyAreaChild ?? string.Empty},
                        {ToShpColName(nameof(Section.ParkingLocation)), section.ParkingLocation ?? string.Empty},
                        {ToShpColName(nameof(Section.Layout)), section.Layout ?? string.Empty},
                        {ToShpColName(nameof(Section.ValidFrom)), section.ValidFrom ?? default},
                        {ToShpColName(nameof(Section.ValidThrough)), section.ValidThrough ?? default},
                        {ToShpColName(nameof(Section.Authority)), section.Authority ?? string.Empty},
                        {ToShpColName(nameof(Section.Level)), section.Level ?? default},
                        {ToShpColName(nameof(Section.LevelSub)), section.LevelSub ?? string.Empty}
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
            System.IO.Compression.ZipFile.CreateFromDirectory(tmpDir, Path.Combine(outDir, $"{downloadId}_{features.Count}.zip"), CompressionLevel.Fastest, false);


            //cleanup
            Directory.Delete(tmpDir, true);

            return downloadId;
        }
    }
}
