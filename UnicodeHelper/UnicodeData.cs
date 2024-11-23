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
        private static readonly Dictionary<UChar, double> numericValues = new Dictionary<UChar, double>();
        private static readonly Dictionary<UChar, UChar> upperCaseMappings = new Dictionary<UChar, UChar>();
        private static readonly Dictionary<UChar, UChar> lowerCaseMappings = new Dictionary<UChar, UChar>();
        private static readonly Dictionary<UChar, UChar> titleCaseMappings = new Dictionary<UChar, UChar>();

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
        /// class will initialize it. Since initialization can take a relatively long time (~350ms),
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
                int rangeStartCodePoint = -1;
                UnicodeDataFileLine rangeStartLine = null;
                foreach (UnicodeDataFileLine line in reader.GetRecords<UnicodeDataFileLine>())
                {
                    int codePoint = int.Parse(line.CodePoint, NumberStyles.HexNumber);
                    if (rangeStartCodePoint != -1)
                    {
                        if (!line.Name.EndsWith(EndOfRangeNameSuffix))
                            throw new InvalidOperationException("Start of range not followed by end of range");
                        
                        for (int c = rangeStartCodePoint; c <= codePoint; c++)
                            UpdateDatabase(c, rangeStartLine);
                        
                        rangeStartCodePoint = -1;
                        rangeStartLine = null;
                    }
                    else if (!line.Name.EndsWith(StartOfRangeNameSuffix))
                        UpdateDatabase(codePoint, line);
                    else
                    {
                        line.Name = line.Name.Substring(1, line.Name.Length - StartOfRangeNameSuffix.Length - 1);
                        rangeStartCodePoint = codePoint;
                        rangeStartLine = line;
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The supported Unicode version
        /// </summary>
        public static Version Version => new Version(16, 0, 0);
        #endregion

        #region Internal methods
        internal static UnicodeCategory GetUnicodeCategory(UChar uc)
        {
            return (UnicodeCategory)categories[(int)uc];
        }

        internal static UnicodeBidiClass GetBidiClass(UChar uc)
        {
            return bidiClasses[(int)uc];
        }

        internal static double GetNumericValue(UChar uc)
        {
            return numericValues.TryGetValue(uc, out double value) ? value : double.NaN;
        }

        internal static UChar ToUpper(UChar uc)
        {
            return upperCaseMappings.TryGetValue(uc, out UChar upper) ? upper : uc;
        }

        internal static UChar ToLower(UChar uc)
        {
            return lowerCaseMappings.TryGetValue(uc, out UChar lower) ? lower : uc;
        }
        #endregion

        #region Helper methods
        private static void SetupBidiRange(int startCodepoint, int endCodepoint, UnicodeBidiClass bidiClass)
        {
            for (int c = startCodepoint; c <= endCodepoint; c++)
                bidiClasses[c] = bidiClass;
        }

        private static void UpdateDatabase(int codePoint, UnicodeDataFileLine line)
        {
            // Category
            categories[codePoint] = (byte)ConvertCategory(line.GeneralCategory);

            // Bidi class
            bidiClasses[codePoint] = ConvertBidiClass(line.BidiClass);

            UChar uc = (UChar)codePoint;

            // Numeric value
            if (!string.IsNullOrEmpty(line.Numeric))
            {
                string[] numbers = line.Numeric.Split('/');
                double value = double.Parse(numbers[0]);
                if (numbers.Length > 1)
                {
                    Debug.Assert(numbers.Length == 2);
                    double bottom = double.Parse(numbers[1]);
                    value /= bottom;
                }
                numericValues.Add(uc, value);
            }
            else
            {
                Debug.Assert(string.IsNullOrEmpty(line.NumericDigit));
                Debug.Assert(string.IsNullOrEmpty(line.NumericDecimal));
            }

            // Uppercase mapping
            if (!string.IsNullOrEmpty(line.UppercaseMapping))
                upperCaseMappings.Add(uc, UChar.FromHexStr(line.UppercaseMapping));

            // Lowercase mapping
            if (!string.IsNullOrEmpty(line.LowercaseMapping))
                lowerCaseMappings.Add(uc, UChar.FromHexStr(line.LowercaseMapping));

            // Titlecase mapping
            if (!string.IsNullOrEmpty(line.TitleCaseMapping))
                titleCaseMappings.Add(uc, UChar.FromHexStr(line.TitleCaseMapping));
            else if (!string.IsNullOrEmpty(line.UppercaseMapping))
                titleCaseMappings.Add(uc, UChar.FromHexStr(line.UppercaseMapping));
        }

        private static UnicodeCategory ConvertCategory(string category)
        {
            switch (category)
            {
                // Cased Letters
                case "Lu": return UnicodeCategory.UppercaseLetter;
                case "Ll": return UnicodeCategory.LowercaseLetter;
                case "Lt": return UnicodeCategory.TitlecaseLetter;
                // Other letters
                case "Lm": return UnicodeCategory.ModifierLetter;
                case "Lo": return UnicodeCategory.OtherLetter;
                // Marks
                case "Mn": return UnicodeCategory.NonSpacingMark;
                case "Mc": return UnicodeCategory.SpacingCombiningMark;
                case "Me": return UnicodeCategory.EnclosingMark;
                // Numbers
                case "Nd": return UnicodeCategory.DecimalDigitNumber;
                case "Nl": return UnicodeCategory.LetterNumber;
                case "No": return UnicodeCategory.OtherNumber;
                // Punctuation
                case "Pc": return UnicodeCategory.ConnectorPunctuation;
                case "Pd": return UnicodeCategory.DashPunctuation;
                case "Ps": return UnicodeCategory.OpenPunctuation;
                case "Pe": return UnicodeCategory.ClosePunctuation;
                case "Pi": return UnicodeCategory.InitialQuotePunctuation;
                case "Pf": return UnicodeCategory.FinalQuotePunctuation;
                case "Po": return UnicodeCategory.OtherPunctuation;
                // Symbols
                case "Sm": return UnicodeCategory.MathSymbol;
                case "Sc": return UnicodeCategory.CurrencySymbol;
                case "Sk": return UnicodeCategory.ModifierSymbol;
                case "So": return UnicodeCategory.OtherSymbol;
                // Separators
                case "Zs": return UnicodeCategory.SpaceSeparator;
                case "Zl": return UnicodeCategory.LineSeparator;
                case "Zp": return UnicodeCategory.ParagraphSeparator;
                // Others
                case "Cc": return UnicodeCategory.Control;
                case "Cf": return UnicodeCategory.Format;
                case "Cs": return UnicodeCategory.Surrogate;
                case "Co": return UnicodeCategory.PrivateUse;
                // File won't contain this category
                //case "Cn": return UnicodeCategory.OtherNotAssigned;
                default: throw new ArgumentException($"Unknown category: {category}");
            }
        }

        private static UnicodeBidiClass ConvertBidiClass(string bidiClass)
        {
            switch (bidiClass)
            {
                // Strong types
                case "L": return UnicodeBidiClass.LeftToRight;
                case "R": return UnicodeBidiClass.RightToLeft;
                case "AL": return UnicodeBidiClass.ArabicLetter;

                // Weak types
                case "EN": return UnicodeBidiClass.EuropeanNumber;
                case "ES": return UnicodeBidiClass.EuropeanSeparator;
                case "ET": return UnicodeBidiClass.EuropeanTerminator;
                case "AN": return UnicodeBidiClass.ArabicNumber;
                case "CS": return UnicodeBidiClass.CommonSeparator;
                case "NSM": return UnicodeBidiClass.NonSpacingMark;
                case "BN": return UnicodeBidiClass.BoundaryNeutral;

                // Neutral types
                case "B": return UnicodeBidiClass.ParagraphSeparator;
                case "S": return UnicodeBidiClass.SegmentSeparator;
                case "WS": return UnicodeBidiClass.WhiteSpace;
                case "ON": return UnicodeBidiClass.OtherNeutral;

                // Explicit formatting types
                case "LRE": return UnicodeBidiClass.LeftToRightEmbedding;
                case "LRO": return UnicodeBidiClass.LeftToRightOverride;
                case "RLE": return UnicodeBidiClass.RightToLeftEmbedding;
                case "RLO": return UnicodeBidiClass.RightToLeftOverride;
                case "PDF": return UnicodeBidiClass.PopDirectionalFormat;
                case "LRI": return UnicodeBidiClass.LeftToRightIsolate;
                case "RLI": return UnicodeBidiClass.RightToLeftIsolate;
                case "FSI": return UnicodeBidiClass.FirstStrongIsolate;
                case "PDI": return UnicodeBidiClass.PopDirectionalIsolate;
                
                default: throw new ArgumentException($"Unknown bidi class: {bidiClass}");
            }
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
