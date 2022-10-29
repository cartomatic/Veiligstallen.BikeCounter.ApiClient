using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehicleType
    {
        unknown = -1,

        /// <summary>
        /// fiets
        /// </summary>
        f,

        /// <summary>
        /// bakfiets
        /// </summary>
        c,

        /// <summary>
        /// snorfiets
        /// </summary>
        s,

        /// <summary>
        /// bromfiets
        /// </summary>
        b,

        /// <summary>
        /// motorfiets
        /// </summary>
        m,

        /// <summary>
        /// gehandicaptenvoertuig
        /// </summary>
        g,

        /// <summary>
        /// anders
        /// </summary>
        a
    }
}
