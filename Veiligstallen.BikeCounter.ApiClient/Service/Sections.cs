using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

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
            => CreateObjectAsync(new RequestConfig<Section>(Configuration.Routes.SECTION, sectionId ?? section.Id, @object: section));

        /// <summary>
        /// Deletes a section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public Task DeleteSectionAsync(string sectionId)
            => DeleteObjectAsync(new RequestConfig(Configuration.Routes.SECTION, sectionId));
    }
}
