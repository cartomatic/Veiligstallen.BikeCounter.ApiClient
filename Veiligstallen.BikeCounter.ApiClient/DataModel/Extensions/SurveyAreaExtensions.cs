using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel.Extensions
{
    internal static class SurveyAreaExtensions
    {
        /// <summary>
        /// Returns a merge set of objects - returned objects must exist in both sets, otherwise they get excluded
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        public static List<SurveyArea> Merge(this IEnumerable<SurveyArea> set1, IEnumerable<SurveyArea> set2)
        {
            var output = new List<SurveyArea>();

            var map1 = set1.ToDictionary(x => x.LocalId, x => x);
            var map2 = set2.ToDictionary(x => x.LocalId, x => x);

            foreach (var kv in map1)
            {
                if(map2.ContainsKey(kv.Key))
                    output.Add(kv.Value.Merge(map2[kv.Key]));
            }

            return output;
        }

        /// <summary>
        /// Merges 2 objects and returns a derived one. obj1 properties take precedence over obj2 properties
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static SurveyArea Merge(this SurveyArea obj1, SurveyArea obj2)
        {
            return new SurveyArea
            {
                Authority = obj1.Authority ?? obj2.Authority,
                GeoLocation = obj1.GeoLocation ?? obj2.GeoLocation,
                GeomWkt = obj1.GeomWkt ?? obj2.GeomWkt,
                Id = obj1.Id ?? obj2.Id,
                LocalId = obj1.LocalId ?? obj2.LocalId,
                Name = obj1.Name ?? obj2.Name,
                Parent = obj1.Parent ?? obj2.Parent,
                ParentLocalId = obj1.ParentLocalId ?? obj2.ParentLocalId,
                SurveyAreaType = obj1.SurveyAreaType ?? obj2.SurveyAreaType,
                ValidFrom = obj1.ValidFrom ?? obj2.ValidFrom,
                ValidThrough = obj1.ValidThrough ?? obj2.ValidThrough,
                XtraInfo = obj1.XtraInfo ?? obj2.XtraInfo
            };
        }
    }
}
