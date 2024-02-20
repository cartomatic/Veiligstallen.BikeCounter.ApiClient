using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient.Loader;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Observation : Base
    {
        [JsonProperty("survey")]
        public string Survey { get; set; }

        [JsonProperty("surveyAreaParent")]
        public string SurveyAreaParent { get; set; }

        [JsonProperty("surveyAreaChild")]
        public string SurveyAreaChild { get; set; }

        [JsonProperty("observedProperty")]
        public string ObservedProperty { get; set; }

        [JsonProperty("featureOfInterest")]
        public string FeatureOfInterest { get; set; }

        [JsonProperty("timestampStart")]
        public DateTime? TimestampStart { get; set; }

        [JsonProperty("timestampEnd")]
        public DateTime? TimestampEnd { get; set; }

        [JsonProperty("measurement")]
        public Measurement Measurement { get; set; }

        [JsonProperty("note")]
        public Note Note { get; set; }

        [JsonProperty("contractor")]
        public string Contractor { get; set; }
        
        /// <summary>
        /// utility property used for looking up sections when FeatureOfInterestIsNotDefined
        /// </summary>
        [JsonIgnore]
        public string SectionLocalId { get; set; }

        /// <summary>
        /// Whether or not an observation has invalid timestamps
        /// </summary>
        /// <returns></returns>
        public bool HasInvalidTimeStamps()
            => HasInvalidTimeStampStart() || HasInvalidTimeStampEnd();

        public bool HasInvalidTimeStampStart()
            => !TimestampStart.HasValue || TimestampStart == default(DateTime);

        public bool HasInvalidTimeStampEnd()
            => !TimestampEnd.HasValue || TimestampEnd == default(DateTime);

        public bool HasInvalidTimeStampStartFormat() => _invalidTimeStartFormat;
        public bool HasInvalidTimeStampEndFormat() => _invalidTimeEndFormat;
        public string GetTimeStartStr() => _timestampStartStr;
        public string GetTimeEndStr() => _timestampEndStr;

        private bool _invalidTimeStartFormat;
        private bool _invalidTimeEndFormat;
        private string _timestampStartStr;
        private string _timestampEndStr;

        /// <summary>
        /// Applies time stamps data
        /// </summary>
        /// <param name="timestampStartStr"></param>
        /// <param name="timestampEndStr"></param>
        public void ApplyTimeStamps(string timestampStartStr, string timestampEndStr)
        {
            _timestampStartStr = timestampStartStr;
            _timestampEndStr = timestampEndStr;

            TimestampStart = Parsers.ParseDate(timestampStartStr);
            TimestampEnd = Parsers.ParseDate(timestampEndStr);

            _invalidTimeStartFormat = (!TimestampStart.HasValue || TimestampStart == default(DateTime)) &&
                                      !string.IsNullOrEmpty(_timestampStartStr);

            _invalidTimeEndFormat = (!TimestampEnd.HasValue || TimestampEnd == default(DateTime)) &&
                                    !string.IsNullOrEmpty(_timestampEndStr);
        }

        /// <summary>
        /// Applies time stamps data
        /// </summary>
        /// <param name="r"></param>
        /// <param name="timestampStartColName"></param>
        /// <param name="timestampEndColName"></param>
        public void ApplyTimeStamps(DataRow r, string timestampStartColName, string timestampEndColName)
        {
            _timestampStartStr = r[timestampStartColName].ToString();
            _timestampEndStr = r[timestampEndColName].ToString();


            TimestampStart = ExtractValue<DateTime?>(r[timestampStartColName]);
            TimestampEnd = ExtractValue<DateTime?>(r[timestampEndColName]);

            _invalidTimeStartFormat = (!TimestampStart.HasValue || TimestampStart == default(DateTime)) &&
                                      !string.IsNullOrEmpty(_timestampStartStr);

            _invalidTimeEndFormat = (!TimestampEnd.HasValue || TimestampEnd == default(DateTime)) &&
                                    !string.IsNullOrEmpty(_timestampEndStr);
        }

        private T ExtractValue<T>(object o)
        {
            if (o == DBNull.Value)
                return default;

            try
            {
                return (T)o;
            }
            catch
            {
                //ignore
            }
            return default;
        }
    }
}
