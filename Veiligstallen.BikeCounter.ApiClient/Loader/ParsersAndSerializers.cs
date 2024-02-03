using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal static class Parsers
    {
        public static DateTime? ParseDate(string d)
        {
            DateTime date;

            if (!string.IsNullOrWhiteSpace(d) &&
                (
                    DateTime.TryParseExact(d, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "d-M-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "d-M-yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "d-M-yyyy H:m", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParse(d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date)
                )
               )
                return date;

            return null;
        }

        public static string SerializeParkingLocationFeature(ParkingLocationFeature[] features)
            => string.Join(",", features?.Select(x => $"{x}") ?? Array.Empty<string>());

        //public static ParkingLocationFeature[] ParseParkingLocationFeature(string s, char separator)
        //{
        //    var output = new List<ParkingLocationFeature>();

        //    foreach (var wouldBeEnumStrValue in s.Split(separator))
        //    {
        //        //if (int.TryParse(wouldBeEnumStrValue, out var enumIntValue) &&
        //        //    Enum.IsDefined(typeof(SurveyAreaType), enumIntValue))
        //        //    output.Add((SurveyAreaType) enumIntValue);

        //        //case insensitive
        //        if (Enum.TryParse<ParkingLocationFeature>(wouldBeEnumStrValue, true, out var enumValue))
        //            output.Add(enumValue);
        //    }

        //    return output.Any() ? output.ToArray() : null;
        //}
        public static string[] ParseParkingLocationFeature(string s, char separator)
            => s.Split(separator);

        public static string SerializeParkingLocationAllowsType(VehicleType? t)
            => t.HasValue ? $"{t}" : string.Empty;

        public static bool TryParseParkingLocationAllowsType(string s, out VehicleType vehicleType)
        {
            vehicleType = VehicleType.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<VehicleType>(s, true, out vehicleType);
        }

        public static bool TryParseParkingSpaceTypeOld(string s, out ParkingSpaceType parkingSpaceType)
        {
            parkingSpaceType = ParkingSpaceType.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<ParkingSpaceType>(s, true, out parkingSpaceType);
        }
        public static bool TryParseParkingSpaceType(string s, out string parkingSpaceType)
        {
            parkingSpaceType = s;
            return true;
        }

        public static bool TryParseVehicleOwnerType(string s, out VehicleOwner ownerType)
        {
            ownerType = VehicleOwner.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<VehicleOwner>(s, true, out ownerType);
        }
        public static string SerializeDateAsIso8601(DateTime? dt)
            => dt.HasValue ? dt.Value.ToString("O") : string.Empty;
    }
}
