using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MimeKit.Cryptography;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader : IDisposable
    {
        private readonly string _dir;
        private readonly bool _extractWkt;
        private EventHandler<string> _msngr;

        public StaticSurveyDataLoader()
        {
        }

        public StaticSurveyDataLoader(string dir, bool extractWkt = false)
        {
            _extractWkt = extractWkt;
            _dir = dir;
        }
        

        private void Notify(string msg)
            => _msngr?.Invoke(this, msg);
        
        /// <inheritdoc />
        public void Dispose()
        {
            _msngr = null;
            DisposeCompleteDataExcel();
            DisposeExtractedData();
        }
    }
}
