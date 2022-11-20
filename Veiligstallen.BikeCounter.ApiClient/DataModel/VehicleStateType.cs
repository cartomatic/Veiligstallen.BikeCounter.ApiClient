using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum VehicleStateType
    {
        unknown = -1,

        /// <summary>
        /// wrak
        /// </summary>
        w,

        /// <summary>
        /// lekke band
        /// </summary>
        l,

        /// <summary>
        /// zonder zadel
        /// </summary>
        z
    }
}
