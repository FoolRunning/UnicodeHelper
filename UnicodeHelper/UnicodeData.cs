using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

        // UnicodeData.txt fields (see https://www.unicode.org/reports/tr44/#UnicodeData.txt)
        private const int CodePointField = 0;
        private const int NameField = 1;
        private const int GeneralCategoryField = 2;
        private const int CombiningClassField = 3;
        private const int BidiClassField = 4;
        private const int DecompositionTypeAndMappingField = 5;
        private const int NumericDecimalField = 6;
        private const int NumericDigitField = 7;
        private const int NumericField = 8;
        private const int UppercaseMappingField = 12;
        private const int LowercaseMappingField = 13;
        private const int TitleCaseMappingField = 14;
        private const int FieldCount = 15;
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
        /// Initializes UnicodeData using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>UnicodeData.txt</c> file.
        /// </summary>
        private static void Init(TextReader textReader)
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

            UnicodeCategory rangeCategory = UnicodeCategory.OtherNotAssigned;
            UnicodeBidiClass rangeBidiClass = UnicodeBidiClass.OtherNeutral;
            int rangeStartCodePoint = -1;
            foreach (string[] line in DataHelper.ReadDataFile(textReader, FieldCount))
            {
                int codePoint = int.Parse(line[CodePointField], NumberStyles.HexNumber);
                string name = line[NameField];
                if (rangeStartCodePoint != -1)
                {
                    if (!name.EndsWith(EndOfRangeNameSuffix, StringComparison.Ordinal))
                        throw new InvalidOperationException("Start of range not followed by end of range");

                    for (int c = rangeStartCodePoint; c <= codePoint; c++)
                        UpdateDatabaseForRange(c, rangeCategory, rangeBidiClass);

                    rangeStartCodePoint = -1;
                    rangeCategory = UnicodeCategory.OtherNotAssigned;
                    rangeBidiClass = UnicodeBidiClass.OtherNeutral;
                }
                else if (!name.EndsWith(StartOfRangeNameSuffix, StringComparison.Ordinal))
                    UpdateDatabase(codePoint, line);
                else
                {
                    rangeStartCodePoint = codePoint;
                    rangeCategory = UnicodeConversion.ConvertCategory(line[GeneralCategoryField]);
                    rangeBidiClass = UnicodeConversion.ConvertBidiClass(line[BidiClassField]);
                    Debug.Assert(string.IsNullOrEmpty(line[NumericField]));
                    Debug.Assert(string.IsNullOrEmpty(line[LowercaseMappingField]));
                    Debug.Assert(string.IsNullOrEmpty(line[UppercaseMappingField]));
                    Debug.Assert(string.IsNullOrEmpty(line[TitleCaseMappingField]));
                    Debug.Assert(line[CombiningClassField] == "0");
                    Debug.Assert(string.IsNullOrEmpty(line[DecompositionTypeAndMappingField]));
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
        public static Version UnicodeVersion => new Version(17, 0, 0);
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

        private static void UpdateDatabase(int codePoint, string[] line)
        {
            // Category
            categories[codePoint] = (byte)UnicodeConversion.ConvertCategory(line[GeneralCategoryField]);

            // Combining class
            combiningClasses[codePoint] = byte.Parse(line[CombiningClassField], CultureInfo.InvariantCulture);

            // Bidi class
            bidiClasses[codePoint] = UnicodeConversion.ConvertBidiClass(line[BidiClassField]);

            UCodepoint uc = (UCodepoint)codePoint;

            HandleDecomposition(uc, line[DecompositionTypeAndMappingField]);

            // Numeric value
            string numeric = line[NumericField];
            if (!string.IsNullOrEmpty(numeric))
                numericValues.Add(uc, UnicodeConversion.ConvertNumeric(numeric));
            else
            {
                Debug.Assert(string.IsNullOrEmpty(line[NumericDigitField]));
                Debug.Assert(string.IsNullOrEmpty(line[NumericDecimalField]));
            }

            // Uppercase mapping
            string upperMapping = line[UppercaseMappingField];
            if (!string.IsNullOrEmpty(upperMapping))
                upperCaseMappings.Add(uc, UCodepoint.FromHexStr(upperMapping));

            // Lowercase mapping
            string lowerMapping = line[LowercaseMappingField];
            if (!string.IsNullOrEmpty(lowerMapping))
                lowerCaseMappings.Add(uc, UCodepoint.FromHexStr(lowerMapping));

            // Titlecase mapping
            string titleMapping = line[TitleCaseMappingField];
            if (!string.IsNullOrEmpty(titleMapping))
                titleCaseMappings.Add(uc, UCodepoint.FromHexStr(titleMapping));
            else if (!string.IsNullOrEmpty(upperMapping))
                titleCaseMappings.Add(uc, UCodepoint.FromHexStr(upperMapping));
        }

        private static void HandleDecomposition(UCodepoint uc, string decompositionStr)
        {
            if (string.IsNullOrWhiteSpace(decompositionStr))
                return;

            bool compatMapping = false;
            if (decompositionStr[0] == '<')
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
            List<long> toRemove = new List<long>();
            foreach (KeyValuePair<long, UCodepoint> kvp in compositionMapping)
            {
                long key = kvp.Key;
                Tuple<UCodepoint, UCodepoint> keyParts = UncreateCombiningKey(key);
                if (GetCombiningClass(keyParts.Item1) != 0 || compositionExclusions.IsExcluded(kvp.Value))
                    toRemove.Add(kvp.Key);
            }

            foreach (long key in toRemove)
                compositionMapping.Remove(key);
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
    }
}
