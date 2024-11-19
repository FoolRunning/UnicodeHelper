using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;

namespace UnicodeHelper.Internal
{
    internal sealed class NameAliasFileLine
    {
        [Index(0)]
        [UsedImplicitly]
        public string CodePoint { get; set; }

        [Index(1)]
        [UsedImplicitly]
        public string Alias { get; set; }

        [Index(2)]
        [UsedImplicitly]
        public string Type { get; set; }
    }
}
