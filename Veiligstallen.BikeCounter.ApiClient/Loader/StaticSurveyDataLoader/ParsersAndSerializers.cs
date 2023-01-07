using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;
using Veiligstallen.BikeCounter.ApiClient.DataModel;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        private DateTime? ParseDate(string d)
        {
            DateTime date;

            if (!string.IsNullOrWhiteSpace(d) &&
                (
                    DateTime.TryParseExact(d, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "d-M-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "d-M-yyyy H:m:s", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParseExact(d, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date) ||
                    DateTime.TryParse(d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out date)
                )
               )
                return date;

            return null;
        }

        private string SerializeParkingLocationFeature(ParkingLocationFeature[] features)
            => string.Join(",", features?.Select(x => $"{x}") ?? Array.Empty<string>());

        private ParkingLocationFeature[] ParseParkingLocationFeature(string s, char separator)
        {
            var output = new List<ParkingLocationFeature>();

            foreach (var wouldBeEnumStrValue in s.Split(separator))
            {
                //if (int.TryParse(wouldBeEnumStrValue.ToString(), out var enumIntValue) &&
                //    Enum.IsDefined(typeof(SurveyAreaType), enumIntValue))
                //    output.Add((SurveyAreaType) enumIntValue);

                //case insensitive
                if (Enum.TryParse<ParkingLocationFeature>(wouldBeEnumStrValue.ToString(), out var enumValue))
                    output.Add(enumValue);
            }

            return output.Any() ? output.ToArray() : null;
        }

        private string SerializeParkingLocationAllowsType(VehicleType? t)
            => t.HasValue ? $"{t}" : string.Empty;

        private bool TryParseParkingLocationAllowsType(string s, out VehicleType vehicleType)
        {
            vehicleType = VehicleType.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<VehicleType>(s, true, out vehicleType);
        }

        private bool TryParseParkingSpaceType(string s, out ParkingSpaceType parkingSpaceType)
        {
            parkingSpaceType = ParkingSpaceType.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<ParkingSpaceType>(s, true, out parkingSpaceType);
        }

        private bool TryParseVehicleOwnerType(string s, out VehicleOwner ownerType)
        {
            ownerType = VehicleOwner.unknown;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse<VehicleOwner>(s, true, out ownerType);
        }
    }
}
