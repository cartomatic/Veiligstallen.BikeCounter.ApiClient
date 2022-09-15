using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class CanonicalVehicle : Base
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public CanonicalVehicleDefinition Json { get; set; }
    }
}
