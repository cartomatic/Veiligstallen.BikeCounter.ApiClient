﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Organization: Base
    {
        public string Name { get; set; }
        public string[] Roles { get; set; }
    }
}
