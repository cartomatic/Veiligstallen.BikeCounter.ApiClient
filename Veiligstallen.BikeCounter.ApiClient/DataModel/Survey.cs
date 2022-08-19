using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.DataModel
{
    public class Survey : Base
    {
        public string Name { get; set; }
        public string Authority { get; set; }
        public string Contractor { get; set; }
        public string License { get; set; }
        public string DistinguishesVehicleCategories { get; set; }
    }
}
