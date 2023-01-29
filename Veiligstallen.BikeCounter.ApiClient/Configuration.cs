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
            public const string SURVEY_SURVEY_AREAS = $"surveys/{OBJECT_ID}/survey-areas";
            public const string SURVEY_PARKING_LOCATIONS = $"surveys/{OBJECT_ID}/parking-locations";
            public const string SURVEY_SECTIONS = $"surveys/{OBJECT_ID}/sections";
            public const string SURVEY_OBSERVATIONS = $"surveys/{OBJECT_ID}/observations";
            public const string SURVEY_COMBINED_OBSERVATIONS = $"surveys/{OBJECT_ID}/combinedobservations";

            public const string ORGANIZATIONS = "organisations";
            public const string ORGANIZATION = $"organisations/{OBJECT_ID}";

            public const string PARKING_LOCATIONS = "parking-locations";
            public const string PARKING_LOCATION = $"parking-locations/{OBJECT_ID}";

            public const string CANONICAL_VEHICLE_CATEGORIES = "canonical-vehicle-categories";
            public const string CANONICAL_VEHICLE_CATEGORY = $"canonical-vehicle-categories/{OBJECT_ID}";

            public const string CANONICAL_VEHICLES = $"canonical-vehicle-categories/{PARENT_ID}/canonical-vehicles";
            public const string CANONICAL_VEHICLE = $"canonical-vehicle-categories/{PARENT_ID}/canonical-vehicles/{OBJECT_ID}";

            public const string OBSERVATIONS = "observations";

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
