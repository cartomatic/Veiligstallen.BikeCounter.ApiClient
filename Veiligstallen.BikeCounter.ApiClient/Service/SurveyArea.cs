using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

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
            => CreateObjectAsync(new RequestConfig<SurveyArea>(Configuration.Routes.SURVEY_AREA, surveyAreaId ?? surveyArea.Id, @object: surveyArea));

        /// <summary>
        /// Deletes a survey area
        /// </summary>
        /// <param name="surveyAreaId"></param>
        /// <returns></returns>
        public Task DeleteSurveyAreaAsync(string surveyAreaId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SURVEY_AREA, surveyAreaId));


        /// <summary>
        /// Prepares survey data for download
        /// </summary>
        /// <param name="queryParams"></param>
        /// <param name="outDir"></param>
        /// <returns></returns>
        public async Task<Guid> PrepareSurveyAreasDownloadAsync(Dictionary<string, string> queryParams, string outDir)
        {
            queryParams ??= new Dictionary<string, string>();

            //reset paging
            queryParams["start"] = "0";
            queryParams["limit"] = "100000";

            var (data, _) = await GetObjectsAsync<SurveyArea>(new RequestConfig(Configuration.Routes.SURVEY_AREAS) { QueryParams = queryParams });

            var downloadId = Guid.NewGuid();

            //serialize
            using var fs = System.IO.File.OpenWrite(Path.Combine(outDir, $"{downloadId}"));
            using var sw = new StreamWriter(fs);

            //hdr
            await sw.WriteAsync(
                string.Join("\t", new []
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
                    string.Join("\t", new[]
                    {
                        surveyArea.Id,
                        surveyArea.LocalId,
                        //surveyArea.ParentLocalId,
                        surveyArea.Name,
                        surveyArea.Parent,
                        surveyArea.ValidFrom?.ToString("O"),
                        surveyArea.ValidThrough?.ToString("O"),
                        surveyArea.Authority,
                        surveyArea.XtraInfo,
                        surveyArea.SurveyAreaType,
                        $"{_cfg.Endpoint}/{Configuration.Routes.SURVEY_AREAS}/{surveyArea.Id}",
                        Environment.NewLine
                    })
                );
            }

            await sw.FlushAsync();
            sw.Close();

            return downloadId;
        }
    }
}
