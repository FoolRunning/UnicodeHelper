using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;

namespace UnicodeHelper.Internal
{
    internal sealed class BlocksFileLine
    {
        [Index(0)]
        [UsedImplicitly]
        public string CodePointRange { get; set; }

        [Index(1)]
        [UsedImplicitly]
        public string BlockName { get; set; }
    }
}
