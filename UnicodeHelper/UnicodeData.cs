using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    /// <summary>
    /// General Unicode data and properties
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
        
        private const int BitShift = 21; // Unicode codepoints use 21 bits
        private const int BitMask = 0x1FFFFF; // 21 bits
        #endregion

        #region Data fields
        private static readonly byte[] categories = new byte[UnicodeCodepointCount];
        private static readonly UnicodeBidiClass[] bidiClasses = new UnicodeBidiClass[UnicodeCodepointCount];
        private static readonly Dictionary<UCodepoint, double> numericValues = new Dictionary<UCodepoint, double>(2000);
        private static readonly Dictionary<UCodepoint, UCodepoint> upperCaseMappings = new Dictionary<UCodepoint, UCodepoint>(1600);
        private static readonly Dictionary<UCodepoint, UCodepoint> lowerCaseMappings = new Dictionary<UCodepoint, UCodepoint>(1600);
        private static readonly Dictionary<UCodepoint, UCodepoint> titleCaseMappings = new Dictionary<UCodepoint, UCodepoint>(1600);
        private static readonly Dictionary<long, UCodepoint> compositionMapping = new Dictionary<long, UCodepoint>(2700);
        private static readonly Dictionary<int, UCodepoint[]> decompositionMapping = new Dictionary<int, UCodepoint[]>(8500);

        private static readonly byte[] combiningClasses = new byte[UnicodeCodepointCount];
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

            CleanUpCompositions();
            FullyExpandDecomposition();
        }
        #endregion

        #region Properties
        /// <summary>
        /// The supported Unicode version of the built-in data
        /// </summary>
        public static Version UnicodeVersion => new Version(16, 0, 0);
        #endregion

        #region Internal methods
        internal static byte GetCombiningClass(UCodepoint uc)
        {
            return combiningClasses[(int)uc];
        }

        internal static UCodepoint GetComposition(UCodepoint ucBase, UCodepoint ucCombining)
        {
            long key = CreateCombiningKey(ucBase, ucCombining, false);
            return compositionMapping.TryGetValue(key, out UCodepoint combined) ? combined : UCodepoint.Null;
        }

        internal static UCodepoint[] GetDecomposition(UCodepoint uc, bool compatMapping)
        {
            int key = CreateDecompKey(uc, compatMapping);
            return decompositionMapping.TryGetValue(key, out UCodepoint[] mapping) ? mapping : null;
        }

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

            // Combining class
            combiningClasses[codePoint] = byte.Parse(line.CombiningClass, CultureInfo.InvariantCulture);

            // Bidi class
            bidiClasses[codePoint] = UnicodeConversion.ConvertBidiClass(line.BidiClass);

            UCodepoint uc = (UCodepoint)codePoint;
            
            HandleDecomposition(uc, line.DecompositionTypeAndMapping);

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

        private static void HandleDecomposition(UCodepoint uc, string decompositionStr)
        {
            if (string.IsNullOrWhiteSpace(decompositionStr))
                return;

            bool compatMapping = false;
            if (decompositionStr.StartsWith("<"))
            {
                // Ignore decomposition type, so remove it from the string
                int spaceIndex = decompositionStr.IndexOf(' ');
                decompositionStr = decompositionStr.Substring(spaceIndex + 1);
                compatMapping = true; // All tagged decompositions are compatibility mappings
            }

            string[] parts = decompositionStr.Split(' ');
            if (parts.Length > 2 && !compatMapping)
                throw new InvalidOperationException($"Unexpected mapping for character {uc.ToHexString()}:{string.Join(", ", parts)}");

            UCodepoint[] mapping = parts.Select(p => (UCodepoint)int.Parse(p, NumberStyles.HexNumber)).ToArray();
            decompositionMapping.Add(CreateDecompKey(uc, compatMapping), mapping);
            if (!compatMapping)
                decompositionMapping.Add(CreateDecompKey(uc, true), mapping);

            if (mapping.Length == 2) // One-to-one mappings are not used for composition
            {
                long key = CreateCombiningKey(mapping[0], mapping[1], compatMapping);
                if (!compositionMapping.ContainsKey(key))
                    compositionMapping.Add(key, uc);
                if (!compatMapping)
                    compositionMapping.Add(CreateCombiningKey(mapping[0], mapping[1], true), uc);
            }

            // TODO: Verify more-than-two codepoint mappings are not used for composition.
            // They don't seem to be based on the test data and normalization reference implementation.
        }

        private static void CleanUpCompositions()
        {
            // Remove any excluded compositions that haven't already been ignored
            CompositionExclusions compositionExclusions = new CompositionExclusions();
            foreach (KeyValuePair<long, UCodepoint> kvp in compositionMapping.ToArray())
            {
                long key = kvp.Key;
                Tuple<UCodepoint, UCodepoint> keyParts = UncreateCombiningKey(key);
                if (GetCombiningClass(keyParts.Item1) != 0 || compositionExclusions.IsExcluded(kvp.Value))
                    compositionMapping.Remove(kvp.Key);
            }
        }

        private static void FullyExpandDecomposition()
        {
            List<UCodepoint> newMapping = new List<UCodepoint>();
            bool changedSomething;
            do
            {
                changedSomething = false;
                foreach (KeyValuePair<int, UCodepoint[]> kvp in decompositionMapping.ToArray())
                {
                    Tuple<UCodepoint, bool> keyParts = UncreateDecompKey(kvp.Key);
                    bool compatMapping = keyParts.Item2;
                    
                    newMapping.Clear();
                    for (int i = 0; i < kvp.Value.Length; i++)
                    {
                        UCodepoint cp = kvp.Value[i];
                        UCodepoint[] decomposition = GetDecomposition(cp, compatMapping);
                        if (decomposition == null)
                            newMapping.Add(cp);
                        else
                        {
                            Debug.Assert(i == 0 || compatMapping);
                            newMapping.AddRange(decomposition);
                            changedSomething = true;
                        }
                    }

                    UCodepoint[] newMappingArray = newMapping.ToArray();
                    if (!changedSomething)
                        HelperUtils.SortCanonical(newMappingArray, newMappingArray.Length); // Sort to avoid sorting later
                    decompositionMapping[kvp.Key] = newMappingArray;
                }
            }
            while (changedSomething);
        }
        
        private static int CreateDecompKey(UCodepoint uc, bool compatMapping)
        {
            return (HelperUtils.BoolToInt(compatMapping) << BitShift) | (int)uc;
        }

        private static Tuple<UCodepoint, bool> UncreateDecompKey(int decompKey)
        {
            return new Tuple<UCodepoint, bool>((UCodepoint)(decompKey & BitMask), (decompKey & (1 << BitShift)) != 0);
        }

        private static long CreateCombiningKey(UCodepoint cpBase, UCodepoint cpCombining, bool compatMapping)
        {
            return ((long)HelperUtils.BoolToInt(compatMapping) << (BitShift * 2)) | ((long)cpBase << BitShift) | (long)cpCombining;
        }

        private static Tuple<UCodepoint, UCodepoint> UncreateCombiningKey(long key)
        {
            return new Tuple<UCodepoint, UCodepoint>((UCodepoint)(int)((key >> BitShift) & BitMask), (UCodepoint)(int)(key & BitMask));
        }
        #endregion

        #region UnicodeDataFileLine class
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class UnicodeDataFileLine
        {
            [Index(0)]
            public string CodePoint { get; set; }

            [Index(1)]
            public string Name { get; set; }
        
            [Index(2)]
            public string GeneralCategory { get; set; }
        
            [Index(3)]
            public string CombiningClass { get; set; }
        
            [Index(4)]
            public string BidiClass { get; set; }
        
            [Index(5)]
            public string DecompositionTypeAndMapping { get; set; }
        
            [Index(6)]
            public string NumericDecimal { get; set; }
        
            [Index(7)]
            public string NumericDigit { get; set; }
        
            [Index(8)]
            public string Numeric { get; set; }
        
            [Index(9)]
            public string IsBidiMirrored { get; set; }
        
            [Index(10)]
            public string ObsoleteName { get; set; }
        
            [Index(11)]
            public string ObsoleteComment { get; set; }
        
            [Index(12)]
            public string UppercaseMapping { get; set; }
        
            [Index(13)]
            public string LowercaseMapping { get; set; }
        
            [Index(14)]
            public string TitleCaseMapping { get; set; }
        }
        #endregion
    }
}
