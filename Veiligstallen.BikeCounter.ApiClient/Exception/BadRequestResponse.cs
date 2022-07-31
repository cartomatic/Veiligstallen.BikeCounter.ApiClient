using System;
using System.Collections.Generic;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.Exception
{
    public class BadRequestResponse
    {
        public BadRequestDetail[] Detail { get; set; }
    }

    public class BadRequestDetail
    {
        public string[] Loc { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
