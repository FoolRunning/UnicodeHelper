using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>This class represents the data in the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#UnicodeData.txt">UnicodeData.txt</see></remarks>
    [PublicAPI]
    public static class UnicodeData
    {
        #region Constants
        internal const int MaxUnicodeCodepoint = 0x10FFFF;
        internal const int UnicodeCodepointCount = MaxUnicodeCodepoint + 1;

        private const string StartOfRangeNameSuffix = ", First>";
        private const string EndOfRangeNameSuffix = ", Last>";
        #endregion

        #region Data fields
        private static readonly byte[] categories = new byte[UnicodeCodepointCount];
        private static readonly UnicodeBidiClass[] bidiClasses = new UnicodeBidiClass[UnicodeCodepointCount];
        private static readonly Dictionary<UCodepoint, double> numericValues = new Dictionary<UCodepoint, double>();
        private static readonly Dictionary<UCodepoint, UCodepoint> upperCaseMappings = new Dictionary<UCodepoint, UCodepoint>();
        private static readonly Dictionary<UCodepoint, UCodepoint> lowerCaseMappings = new Dictionary<UCodepoint, UCodepoint>();
        private static readonly Dictionary<UCodepoint, UCodepoint> titleCaseMappings = new Dictionary<UCodepoint, UCodepoint>();

        //private static readonly byte[] s_combiningClasses = new byte[UnicodeCodepointCount];
        //private static readonly int[] s_flags = new int[UnicodeCodepointCount];
        #endregion

        #region Static constructor
        static UnicodeData()
        {
            DataHelper.ReadResource("UnicodeData.txt", Init);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeData using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~300ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor

        /// <summary>
        /// Initializes UnicodeData using the file specified. The file must be in the default
        /// Unicode standard format for a <c>UnicodeData.txt</c> file.
        /// </summary>
        public static void Init(string unicodeDataFilePath)
        {
            DataHelper.ReadDataFile(unicodeDataFilePath, Init);
        }

        /// <summary>
        /// Initializes UnicodeData using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>UnicodeData.txt</c> file.
        /// </summary>
        public static void Init(TextReader textReader)
        {
            // Load defaults for categories
            for (int i = 0; i < categories.Length; i++)
                categories[i] = (byte)UnicodeCategory.OtherNotAssigned;

            // TODO: Test default bidi values

            // Load defaults for bidi class. This is dependent on the range of characters
            // where a codepoint occurs.
            // (see https://www.unicode.org/Public/UCD/latest/ucd/extracted/DerivedBidiClass.txt).
            SetupBidiRange(0x0000, MaxUnicodeCodepoint, UnicodeBidiClass.LeftToRight);
            SetupBidiRange(0x0590, 0x05FF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x0600, 0x07BF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x07C0, 0x085F, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x0860, 0x08FF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x20A0, 0x20CF, UnicodeBidiClass.EuropeanTerminator);
            SetupBidiRange(0xFB1D, 0xFB4F, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0xFB50, 0xFDCF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0xFDF0, 0xFDFF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0xFE70, 0xFEFF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x10800, 0x10CFF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x10D00, 0x10D3F, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x10D40, 0x10EBF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x10EC0, 0x10EFF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x10F00, 0x10F2F, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x10F30, 0x10F6F, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x10F70, 0x10FFF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x1E800, 0x1EC6F, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x1EC70, 0x1ECBF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x1ECC0, 0x1ECFF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x1ED00, 0x1ED4F, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x1ED50, 0x1EDFF, UnicodeBidiClass.RightToLeft);
            SetupBidiRange(0x1EE00, 0x1EEFF, UnicodeBidiClass.ArabicLetter);
            SetupBidiRange(0x1EF00, 0x1EFFF, UnicodeBidiClass.RightToLeft);

            using (CsvReader reader = new CsvReader(textReader, DataHelper.CsvConfiguration))
            {
                UnicodeCategory rangeCategory = UnicodeCategory.OtherNotAssigned;
                UnicodeBidiClass rangeBidiClass = UnicodeBidiClass.OtherNeutral;
                int rangeStartCodePoint = -1;
                foreach (UnicodeDataFileLine line in reader.GetRecords<UnicodeDataFileLine>())
                {
                    int codePoint = int.Parse(line.CodePoint, NumberStyles.HexNumber);
                    if (rangeStartCodePoint != -1)
                    {
                        if (!line.Name.EndsWith(EndOfRangeNameSuffix))
                            throw new InvalidOperationException("Start of range not followed by end of range");
                        
                        for (int c = rangeStartCodePoint; c <= codePoint; c++)
                            UpdateDatabaseForRange(c, rangeCategory, rangeBidiClass);
                        
                        rangeStartCodePoint = -1;
                        rangeCategory = UnicodeCategory.OtherNotAssigned;
                        rangeBidiClass = UnicodeBidiClass.OtherNeutral;
                    }
                    else if (!line.Name.EndsWith(StartOfRangeNameSuffix))
                        UpdateDatabase(codePoint, line);
                    else
                    {
                        line.Name = line.Name.Substring(1, line.Name.Length - StartOfRangeNameSuffix.Length - 1);
                        rangeStartCodePoint = codePoint;
                        rangeCategory = UnicodeConversion.ConvertCategory(line.GeneralCategory);
                        rangeBidiClass = UnicodeConversion.ConvertBidiClass(line.BidiClass);
                        Debug.Assert(string.IsNullOrEmpty(line.Numeric));
                        Debug.Assert(string.IsNullOrEmpty(line.LowercaseMapping));
                        Debug.Assert(string.IsNullOrEmpty(line.UppercaseMapping));
                        Debug.Assert(string.IsNullOrEmpty(line.TitleCaseMapping));
                        Debug.Assert(line.CombiningClass == "0");
                        Debug.Assert(string.IsNullOrEmpty(line.DecompositionTypeAndMapping));
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The supported Unicode version of the built-in data
        /// </summary>
        public static Version UnicodeVersion => new Version(16, 0, 0);
        #endregion

        #region Internal methods
        internal static UnicodeCategory GetUnicodeCategory(UCodepoint uc)
        {
            return (UnicodeCategory)categories[(int)uc];
        }

        internal static UnicodeBidiClass GetBidiClass(UCodepoint uc)
        {
            return bidiClasses[(int)uc];
        }

        internal static double GetNumericValue(UCodepoint uc)
        {
            return numericValues.TryGetValue(uc, out double value) ? value : double.NaN;
        }

        internal static UCodepoint ToUpper(UCodepoint uc)
        {
            return upperCaseMappings.TryGetValue(uc, out UCodepoint upper) ? upper : uc;
        }

        internal static UCodepoint ToLower(UCodepoint uc)
        {
            return lowerCaseMappings.TryGetValue(uc, out UCodepoint lower) ? lower : uc;
        }
        #endregion

        #region Helper methods
        private static void SetupBidiRange(int startCodepoint, int endCodepoint, UnicodeBidiClass bidiClass)
        {
            for (int c = startCodepoint; c <= endCodepoint; c++)
                bidiClasses[c] = bidiClass;
        }

        private static void UpdateDatabaseForRange(int codePoint, 
            UnicodeCategory category, UnicodeBidiClass bidiClass)
        {
            categories[codePoint] = (byte)category;
            bidiClasses[codePoint] = bidiClass;
        }

        private static void UpdateDatabase(int codePoint, UnicodeDataFileLine line)
        {
            // Category
            categories[codePoint] = (byte)UnicodeConversion.ConvertCategory(line.GeneralCategory);

            // Bidi class
            bidiClasses[codePoint] = UnicodeConversion.ConvertBidiClass(line.BidiClass);

            UCodepoint uc = (UCodepoint)codePoint;

            // Numeric value
            if (!string.IsNullOrEmpty(line.Numeric))
                numericValues.Add(uc, UnicodeConversion.ConvertNumeric(line.Numeric));
            else
            {
                Debug.Assert(string.IsNullOrEmpty(line.NumericDigit));
                Debug.Assert(string.IsNullOrEmpty(line.NumericDecimal));
            }

            // Uppercase mapping
            if (!string.IsNullOrEmpty(line.UppercaseMapping))
                upperCaseMappings.Add(uc, UCodepoint.FromHexStr(line.UppercaseMapping));

            // Lowercase mapping
            if (!string.IsNullOrEmpty(line.LowercaseMapping))
                lowerCaseMappings.Add(uc, UCodepoint.FromHexStr(line.LowercaseMapping));

            // Titlecase mapping
            if (!string.IsNullOrEmpty(line.TitleCaseMapping))
                titleCaseMappings.Add(uc, UCodepoint.FromHexStr(line.TitleCaseMapping));
            else if (!string.IsNullOrEmpty(line.UppercaseMapping))
                titleCaseMappings.Add(uc, UCodepoint.FromHexStr(line.UppercaseMapping));
        }
        #endregion

        #region UnicodeDataFileLine class
        private sealed class UnicodeDataFileLine
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
        #endregion
    }
}
