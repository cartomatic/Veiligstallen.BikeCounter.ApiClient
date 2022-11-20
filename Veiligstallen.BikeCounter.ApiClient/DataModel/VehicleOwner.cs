using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehicleOwner
    {
        unknown = -1,

        /// <summary>
        /// Privéfiets
        /// </summary>
        p,

        /// <summary>
        /// Leasefiets, zoals Swapfiets
        /// </summary>
        l,

        /// <summary>
        /// Huurfiets, zoals OV-fiets
        /// </summary>
        h
    }
}
