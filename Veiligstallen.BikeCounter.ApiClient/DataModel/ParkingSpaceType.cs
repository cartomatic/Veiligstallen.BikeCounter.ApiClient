using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public enum ParkingSpaceType
    {
        unknown = -1,

        /// <summary>
        /// geen voorziening
        /// </summary>
        x,

        /// <summary>
        /// rek
        /// </summary>
        r,

        /// <summary>
        /// rek (buitenmodel)
        /// </summary>
        rb,

        /// <summary>
        /// etagerek-boven
        /// </summary>
        b,

        /// <summary>
        /// etagerek-boven (buitenmodel)
        /// </summary>
        bb,

        /// <summary>
        /// etagerek-onder
        /// </summary>
        o,

        /// <summary>
        /// etagerek-onder (buitenmodel)
        /// </summary>
        ob,

        /// <summary>
        /// kluis
        /// </summary>
        k,

        /// <summary>
        /// nietjes
        /// </summary>
        n,

        /// <summary>
        /// vak
        /// </summary>
        v,

        /// <summary>
        /// anders
        /// </summary>
        a
    }
}
