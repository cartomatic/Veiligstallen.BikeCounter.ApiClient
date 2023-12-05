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
        private EventHandler<string> _msngr;
        private ExcelDataExtractor _excelDataExtractor;
        private FlatDataExtractor _flatDataExtractor;
        private ShapeFileDataExtractor _shapeFileDataExtractor;

        public StaticSurveyDataLoader()
        {
            _excelDataExtractor = new ExcelDataExtractor();
            _flatDataExtractor = new FlatDataExtractor();
            _shapeFileDataExtractor = new ShapeFileDataExtractor(false);
        }

        public StaticSurveyDataLoader(string dir, bool extractWkt = false)
        {
            _dir = dir;
            _excelDataExtractor = new ExcelDataExtractor();
            _flatDataExtractor = new FlatDataExtractor();
            _shapeFileDataExtractor = new ShapeFileDataExtractor(extractWkt);
        }
        

        private void Notify(string msg)
            => _msngr?.Invoke(this, msg);

        private void NotifyProgress(int progress)
            => _msngr?.Invoke(this, progress.ToString());

        private int CalculateProgress(int counter, int count)
            => (int)Math.Ceiling((double)counter / count * 100);

        /// <inheritdoc />
        public void Dispose()
        {
            _msngr = null;
            _excelDataExtractor?.Dispose();
            _excelDataExtractor = null;
            _flatDataExtractor?.Dispose();
            _flatDataExtractor = null;
            _shapeFileDataExtractor?.Dispose();
            _shapeFileDataExtractor = null;
            DisposeExtractedData();
        }
    }
}
