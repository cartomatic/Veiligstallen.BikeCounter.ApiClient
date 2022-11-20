using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehicleAppearance
    {
        unknown = -1,

        /// <summary>
        /// Kinderfiets
        /// </summary>
        k,

        /// <summary>
        /// Racefiets
        /// </summary>
        r,

        /// <summary>
        /// Ligfiets
        /// </summary>
        l,

        /// <summary>
        /// Bakfiets / Transportfiets
        /// </summary>
        b,

        /// <summary>
        /// Fietskar
        /// </summary>
        f,

        /// <summary>
        /// Vouwfiets
        /// </summary>
        v,

        /// <summary>
        /// Mountainbike
        /// </summary>
        m,

        /// <summary>
        /// Driewieler
        /// </summary>
        d,

        /// <summary>
        /// Tandem
        /// </summary>
        t,

        /// <summary>
        /// Gehandicaptenvoertuig
        /// </summary>
        g,

        /// <summary>
        /// Sterk afwijkend nl. b, f, d, t, g
        /// </summary>
        x
    }
}
