using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Veiligstallen.BikeCounter.ApiClient.Loader
{
    public class FlatFileUtils
    {
        public enum FlatFileSeparator
        {
            Tab,
            Semicolon,

            //this is not a separator really, but simply tells to use an excel parser instead of a text file parser...
            //not pretty, but works
            Xlsx
        }

        public static ImmutableDictionary<FlatFileSeparator, char> FlatFileSeparators = new Dictionary<FlatFileSeparator, char>()
        {
            {FlatFileSeparator.Tab, '\t'},
            {FlatFileSeparator.Semicolon, ';'},
            {FlatFileSeparator.Xlsx, '_'}
        }.ToImmutableDictionary();

        public static ImmutableDictionary<FlatFileSeparator, string> FlatFileExtensions = new Dictionary<FlatFileSeparator, string>()
        {
            {FlatFileSeparator.Tab, "tsv"},
            {FlatFileSeparator.Semicolon, "csv"},
            {FlatFileSeparator.Xlsx, "xlsx"}
        }.ToImmutableDictionary();
    }
}
