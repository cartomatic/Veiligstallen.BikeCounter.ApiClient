using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Veiligstallen.BikeCounter.ApiClient
{
    public class Configuration
    {
        public class Routes
        {
            public const string OBJECT_ID = "{objectId}";
            public const string PARENT_ID = "{parentId}";

            //Note: looks like the api likes the trailing slashes because of some reason
            public const string SECTIONS = "sections";
            public const string SECTION = $"sections/{OBJECT_ID}";

            public const string PARKING_FACILTIES = "parking-facilities";

            public const string SURVEY_AREAS = "survey-areas";
            public const string SURVEY_AREA = $"survey-areas/{OBJECT_ID}";

            public const string SURVEYS = "surveys";
            public const string SURVEY = $"surveys/{OBJECT_ID}";

            public const string ORGANIZATIONS = "organisations";
            public const string ORGANIZATION = $"organisations/{OBJECT_ID}";

            public const string PARKING_LOCATIONS = "parking-locations";
            public const string PARKING_LOCATION = $"parking-locations/{OBJECT_ID}";

            public const string AUTH = "auth";
        }

        public string Endpoint { get; set; }
        public string AuthorizationScheme { get; set; } = "Basic";
        public string User { get; set; }
        public string Pass { get; set; }

        private static Configuration _cfg;
        

        public static Configuration Read()
        {
            if (_cfg != null)
                return _cfg;

            var cfg = Cartomatic.Utils.NetCoreConfig.GetNetCoreConfig();

            _cfg = cfg.GetSection("VeiligStallenBikeCounter").Get<Configuration>();

            return _cfg;
        }
    }
}
