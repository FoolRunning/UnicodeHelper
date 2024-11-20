using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
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
        // Mem size of data (so far):
        // 1 + 1 + 8 + 4 + 4 + 4 = 22
        // 1,114,112 * 22 = 24,510,464 bytes (about 24MB)

        private static readonly byte[] categories = new byte[UnicodeCodepointCount];
        private static readonly UnicodeBidiClass[] bidiClasses = new UnicodeBidiClass[UnicodeCodepointCount];
        private static readonly double[] numericValues = new double[UnicodeCodepointCount];
        private static readonly UChar[] upperCaseMapping = new UChar[UnicodeCodepointCount];
        private static readonly UChar[] lowerCaseMapping = new UChar[UnicodeCodepointCount];
        private static readonly UChar[] titleCaseMapping = new UChar[UnicodeCodepointCount];

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
        public static void Init() { } // Just invokes the static constructor

        public static void Init(string unicodeDataFilePath)
        {
            DataHelper.ReadDataFile(unicodeDataFilePath, Init);
        }

        public static void Init(TextReader textReader)
        {
            // Load Unicode defaults
            for (int i = 0; i < categories.Length; i++)
                categories[i] = (byte)UnicodeCategory.OtherNotAssigned;

            for (int i = 0; i < numericValues.Length; i++)
                numericValues[i] = double.NaN;

            for (int i = 0; i < upperCaseMapping.Length; i++)
                upperCaseMapping[i] = (UChar)i;

            for (int i = 0; i < lowerCaseMapping.Length; i++)
                lowerCaseMapping[i] = (UChar)i;

            for (int i = 0; i < titleCaseMapping.Length; i++)
                titleCaseMapping[i] = (UChar)i;

            ProcessFile(textReader, UpdateDatabase);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The supported Unicode version
        /// </summary>
        public static Version Version => new Version(16, 0, 0);
        #endregion

        #region Internal methods
        internal static void ProcessFile(TextReader textReader, 
            Action<int, UnicodeDataFileLine> handleLine)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
                IgnoreBlankLines = true
            };

            using (CsvReader reader = new CsvReader(textReader, config))
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
                            handleLine(c, rangeStartLine);
                        
                        rangeStartCodePoint = -1;
                        rangeStartLine = null;
                    }
                    else if (!line.Name.EndsWith(StartOfRangeNameSuffix))
                        handleLine(codePoint, line);
                    else
                    {
                        line.Name = line.Name.Substring(1, line.Name.Length - StartOfRangeNameSuffix.Length - 1);
                        rangeStartCodePoint = codePoint;
                        rangeStartLine = line;
                    }
                }
            }
        }

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
            return numericValues[(int)uc];
        }

        internal static UChar ToUpper(UChar uc)
        {
            return upperCaseMapping[(int)uc];
        }

        internal static UChar ToLower(UChar uc)
        {
            return lowerCaseMapping[(int)uc];
        }
        #endregion

        #region Helper methods
        private static void UpdateDatabase(int codePoint, UnicodeDataFileLine line)
        {
            // Category
            categories[codePoint] = (byte)ConvertCategory(line.GeneralCategory);

            // Bidi class
            bidiClasses[codePoint] = ConvertBidiClass(line.BidiClass);

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

                numericValues[codePoint] = value;
            }
            else
            {
                Debug.Assert(string.IsNullOrEmpty(line.NumericDigit));
                Debug.Assert(string.IsNullOrEmpty(line.NumericDecimal));
            }

            // Uppercase mapping
            if (!string.IsNullOrEmpty(line.UppercaseMapping))
                upperCaseMapping[codePoint] = UChar.FromHexStr(line.UppercaseMapping);

            // Lowercase mapping
            if (!string.IsNullOrEmpty(line.LowercaseMapping))
                lowerCaseMapping[codePoint] = UChar.FromHexStr(line.LowercaseMapping);

            // Titlecase mapping
            if (!string.IsNullOrEmpty(line.TitleCaseMapping))
                titleCaseMapping[codePoint] = UChar.FromHexStr(line.TitleCaseMapping);
            else if (!string.IsNullOrEmpty(line.UppercaseMapping))
                titleCaseMapping[codePoint] = UChar.FromHexStr(line.UppercaseMapping);
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
    }
}
