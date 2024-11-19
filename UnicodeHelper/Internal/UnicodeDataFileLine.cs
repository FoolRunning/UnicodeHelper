using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;

namespace UnicodeHelper.Internal
{
    internal sealed class UnicodeDataFileLine
    {
        [Index(0)]
        [UsedImplicitly]
        public string CodePoint { get; set; }

        [Index(1)]
        [UsedImplicitly]
        public string Name { get; set; }
        
        [Index(2)]
        [UsedImplicitly]
        public string GeneralCategory { get; set; }
        
        [Index(3)]
        [UsedImplicitly]
        public string CombiningClass { get; set; }
        
        [Index(4)]
        [UsedImplicitly]
        public string BidiClass { get; set; }
        
        [Index(5)]
        [UsedImplicitly]
        public string DecompositionTypeAndMapping { get; set; }
        
        [Index(6)]
        [UsedImplicitly]
        public string NumericDecimal { get; set; }
        
        [Index(7)]
        [UsedImplicitly]
        public string NumericDigit { get; set; }
        
        [Index(8)]
        [UsedImplicitly]
        public string Numeric { get; set; }
        
        [Index(9)]
        [UsedImplicitly]
        public string IsBidiMirrored { get; set; }
        
        [Index(10)]
        [UsedImplicitly]
        public string ObsoleteName { get; set; }
        
        [Index(11)]
        [UsedImplicitly]
        public string ObsoleteComment { get; set; }
        
        [Index(12)]
        [UsedImplicitly]
        public string UppercaseMapping { get; set; }
        
        [Index(13)]
        [UsedImplicitly]
        public string LowercaseMapping { get; set; }
        
        [Index(14)]
        [UsedImplicitly]
        public string TitleCaseMapping { get; set; }
    }
}
