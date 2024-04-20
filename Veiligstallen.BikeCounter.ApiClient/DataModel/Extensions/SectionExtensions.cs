using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel.Extensions
{
    internal static class SectionExtensions
    {
        /// <summary>
        /// Returns a merge set of objects - returned objects must exist in both sets, otherwise they get excluded
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        public static List<Section> Merge(this IEnumerable<Section> set1, IEnumerable<Section> set2)
        {
            var output = new List<Section>();

            var map1 = set1.ToDictionary(x => x.LocalId, x => x);
            var map2 = set2.ToDictionary(x => x.LocalId, x => x);

            foreach (var kv in map1)
            {
                if (map2.ContainsKey(kv.Key))
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
        public static Section Merge(this Section obj1, Section obj2)
        {
            return new Section
            {
                Authority = obj1.Authority ?? obj2.Authority,
                GeoLocation = obj1.GeoLocation ?? obj2.GeoLocation,
                GeomWkt = obj1.GeomWkt ?? obj2.GeomWkt,
                Id = obj1.Id ?? obj2.Id,
                LocalId = obj1.LocalId ?? obj2.LocalId,
                Name = obj1.Name ?? obj2.Name,
                ValidFrom = obj1.ValidFrom ?? obj2.ValidFrom,
                ValidThrough = obj1.ValidThrough ?? obj2.ValidThrough,
                Layout = obj1.Layout ?? obj2.Layout,
                Level = obj1.Level ?? obj2.Level,
                LevelSub = obj1.LevelSub ?? obj2.LevelSub,
                ParkingLocation = obj1.ParkingLocation ?? obj2.ParkingLocation,
                ParkingLocationLocalId = obj1.ParkingLocationLocalId ?? obj2.ParkingLocationLocalId,
                ParkingSpaceOf = obj1.ParkingSpaceOf ?? obj2.ParkingSpaceOf
            };
        }
    }
}
