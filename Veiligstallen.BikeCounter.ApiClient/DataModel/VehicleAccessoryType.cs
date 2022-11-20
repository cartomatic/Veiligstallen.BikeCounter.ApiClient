using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehicleAccessoryType
    {
        unknown = -1,

        /// <summary>
        /// kinderzitje
        /// </summary>
        z,

        /// <summary>
        /// fietstas
        /// </summary>
        t,

        /// <summary>
        /// bagagedrager
        /// </summary>
        b,

        /// <summary>
        /// krat / mand
        /// </summary>
        k,

        /// <summary>
        /// Beperkt afwijkend, nl. k, z
        /// </summary>
        p
    }
}
