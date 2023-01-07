using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Extensions;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    internal partial class StaticSurveyDataLoader
    {
        private void ResetCollections()
        {
            _surveyAreas?.Clear();
            _parkingLocations?.Clear();
            _sections?.Clear();
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractSurveyAreasFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            //flat objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _surveyAreas = await ExtractSurveyAreasFlatInternalAsync(fName, separator, header);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractSurveyAreasShpOnlyAsync(string fName)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _surveyAreas = ExtractSurveyAreasShpOnlyInternalAsync(fName);
        }

        private async Task ExtractSurveyAreasAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _surveyAreas = ExtractSurveyAreasShpInternal(shpFile)
                .Merge(ExtractSurveyAreasSeparatedInternal(flatFile, flatFileSeparator));
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractParkingLocationsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            //flat objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();

            _parkingLocations = await ExtractParkingLocationsFlatInternalAsync(fName, separator, header);
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractParkingLocationsShpOnlyAsync(string fName)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _parkingLocations = ExtractParkingLocationsShpOnlyInternalAsync(fName);
        }

        private async Task ExtractParkingLocationsAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _parkingLocations = ExtractParkingLocationsShpInternal(shpFile).
                Merge(ExtractParkingLocationsSeparatedInternal(flatFile, flatFileSeparator));
        }

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractSectionsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            //flat objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();

            _sections = await ExtractSectionsFlatInternalAsync(fName, separator, header);
        }

        private Task<IEnumerable<string>> ExtractSurveyAreasIdsFlatAsync(string fName,
            FlatFileUtils.FlatFileSeparator separator, bool header)
            => ExtractSurveyAreasIdsFlatInternalAsync(fName, separator, header);

        [Obsolete("Format abandoned and not officially supported anymore")]
        private async Task ExtractSectionsShpOnlyAsync(string fName)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _sections = ExtractSectionsShpOnlyInternalAsync(fName);
        }

        private async Task ExtractSectionsAsync(string shpFile, string flatFile, FlatFileUtils.FlatFileSeparator flatFileSeparator)
        {
            //shp objects always uploaded 1 by 1, so need to reset collections!
            ResetCollections();
            _sections = ExtractSectionsShpInternal(shpFile).
                Merge(ExtractSectionsSeparatedInternal(flatFile, flatFileSeparator));
        }

        private async Task ExtractObservationsFlatAsync(string fName, FlatFileUtils.FlatFileSeparator separator, bool header)
        {
            _observations = await ExtractObservationsInternalAsync(fName, separator, header);
        }
    }
}
