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
            Semicolon
        }

        public static ImmutableDictionary<FlatFileSeparator, char> FlatFileSeparators = new Dictionary<FlatFileSeparator, char>()
        {
            {FlatFileSeparator.Tab, '\t'},
            {FlatFileSeparator.Semicolon, ';'}
        }.ToImmutableDictionary();

        public static ImmutableDictionary<FlatFileSeparator, string> FlatFileExtensions = new Dictionary<FlatFileSeparator, string>()
        {
            {FlatFileSeparator.Tab, "tsv"},
            {FlatFileSeparator.Semicolon, "csv"}
        }.ToImmutableDictionary();
    }
}
