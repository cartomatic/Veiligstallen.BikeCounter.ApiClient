using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehiclePropulsionType
    {
        unknown = -1,

        /// <summary>
        /// human(Spierkracht)
        /// </summary>
        s,

        /// <summary>
        /// electric(Elektrisch bv e - scooter)
        /// </summary>
        e,

        /// <summary>
        /// combustion(brandstofmotor)
        /// </summary>
        b
    }
}
