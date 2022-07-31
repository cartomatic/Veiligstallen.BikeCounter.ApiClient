using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Organization
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string[] Roles { get; set; }
    }
}
