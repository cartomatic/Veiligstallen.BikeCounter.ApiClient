using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CanonicalVehicleDefinition
    {
        /// <summary>
        /// State of parked (i = In voorziening, j = j In voorziening, juist gestald, k = In voorziening, neemt plek in, p = nabij voorziening, x = buiten voorziening). More info at https://docs.crow.nl/datastandaard-fietsparkeren/api-fiets/#enum-vehicleparkstate
        /// </summary>
        public string ParkState { get; set; }

        /// <summary>
        /// Vehicle definition (f = fiets, c = bakfiets, s = snorfiets, b = bromfiets, m = motorfiets, g = gehandicaptenvoertuig, a = anders)
        /// </summary>
        public string Vehicle { get; set; }
    }
}
