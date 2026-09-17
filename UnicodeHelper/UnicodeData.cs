using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private static readonly byte[] categories;
        private static readonly UnicodeBidiClass[] bidiClasses;
        private static readonly Dictionary<UCodepoint, double> numericValues;
        private static readonly CodepointMapTable upperCaseMappings;
        private static readonly CodepointMapTable lowerCaseMappings;
        private static readonly CodepointMapTable titleCaseMappings;
        private static readonly Dictionary<long, UCodepoint> compositionMapping;
        private static readonly Dictionary<int, UCodepoint[]> decompositionMapping;
        private static readonly byte[] combiningClasses;
        private static readonly NormalizationFlags[] normalizationFlags;
        #endregion

        #region Static constructor
        static UnicodeData()
        {
            // All of the loading work is done by a separate class (see the remarks on Loader)
            Loader loader = new Loader();
            DataHelper.ReadResource("UnicodeData.txt", loader.Load);

            categories = loader.Categories;
            bidiClasses = loader.BidiClasses;
            numericValues = loader.NumericValues;
            upperCaseMappings = loader.UpperCaseMappings;
            lowerCaseMappings = loader.LowerCaseMappings;
            titleCaseMappings = loader.TitleCaseMappings;
            compositionMapping = loader.CompositionMapping;
            decompositionMapping = loader.DecompositionMapping;
            combiningClasses = loader.CombiningClasses;
            normalizationFlags = loader.NormalizationFlagsTable;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeData using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~80ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor
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

        /// <summary>
        /// The table of canonical combining classes indexed by codepoint
        /// </summary>
        internal static byte[] CombiningClassTable => combiningClasses;

        /// <summary>
        /// Gets the facts about the specified codepoint that the normalization engine uses to skip work
        /// </summary>
        internal static NormalizationFlags GetNormalizationFlags(UCodepoint uc)
        {
            return normalizationFlags[(int)uc];
        }

        internal static UCodepoint GetComposition(UCodepoint ucBase, UCodepoint ucCombining)
        {
            long key = Keys.CreateCombiningKey(ucBase, ucCombining, false);
            return compositionMapping.TryGetValue(key, out UCodepoint combined) ? combined : UCodepoint.Null;
        }

        internal static UCodepoint[] GetDecomposition(UCodepoint uc, bool compatMapping)
        {
            int key = Keys.CreateDecompKey(uc, compatMapping);
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
            return upperCaseMappings.Map(uc);
        }

        internal static UCodepoint ToLower(UCodepoint uc)
        {
            return lowerCaseMappings.Map(uc);
        }
        #endregion

        #region Keys class
        /// <summary>
        /// Creates the packed keys used for the composition and decomposition dictionaries.
        /// </summary>
        /// <remarks>Kept in its own class so that the loader can use these without going through
        /// <see cref="UnicodeData"/>'s class-initialization check (see remarks on <see cref="Loader"/>).</remarks>
        private static class Keys
        {
            public static int CreateDecompKey(UCodepoint uc, bool compatMapping)
            {
                return (HelperUtils.BoolToInt(compatMapping) << BitShift) | (int)uc;
            }

            public static void UncreateDecompKey(int decompKey, out UCodepoint uc, out bool compatMapping)
            {
                uc = (UCodepoint)(decompKey & BitMask);
                compatMapping = (decompKey & (1 << BitShift)) != 0;
            }

            public static long CreateCombiningKey(UCodepoint cpBase, UCodepoint cpCombining, bool compatMapping)
            {
                return ((long)HelperUtils.BoolToInt(compatMapping) << (BitShift * 2)) | ((long)cpBase << BitShift) | (long)cpCombining;
            }

            public static void UncreateCombiningKey(long key, out UCodepoint cpBase, out UCodepoint cpCombining,
                out bool compatMapping)
            {
                cpBase = (UCodepoint)(int)((key >> BitShift) & BitMask);
                cpCombining = (UCodepoint)(int)(key & BitMask);
                compatMapping = (key >> (BitShift * 2)) != 0;
            }
        }
        #endregion

        #region Loader class
        /// <summary>
        /// Loads the data from a <c>UnicodeData.txt</c> file into a set of tables that are then handed
        /// to <see cref="UnicodeData"/> by its static constructor.
        /// </summary>
        /// <remarks>This is a separate class for performance. While a class's static constructor is
        /// running, every call into a method of that class goes through a class-initialization check
        /// (~80ns). Loading makes hundreds of thousands of such calls, so doing the work in
        /// <see cref="UnicodeData"/> itself made initialization several times slower. Instance methods
        /// of this class carry no such penalty.</remarks>
        private sealed class Loader
        {
            #region Data fields
            public readonly byte[] Categories = new byte[UnicodeCodepointCount];
            public readonly UnicodeBidiClass[] BidiClasses = new UnicodeBidiClass[UnicodeCodepointCount];
            public readonly Dictionary<UCodepoint, double> NumericValues = new Dictionary<UCodepoint, double>(2000);
            public readonly CodepointMapTable UpperCaseMappings = new CodepointMapTable();
            public readonly CodepointMapTable LowerCaseMappings = new CodepointMapTable();
            public readonly CodepointMapTable TitleCaseMappings = new CodepointMapTable();
            public readonly Dictionary<long, UCodepoint> CompositionMapping = new Dictionary<long, UCodepoint>(2700);
            public readonly Dictionary<int, UCodepoint[]> DecompositionMapping = new Dictionary<int, UCodepoint[]>(8500);
            public readonly byte[] CombiningClasses = new byte[UnicodeCodepointCount];
            public readonly NormalizationFlags[] NormalizationFlagsTable = new NormalizationFlags[UnicodeCodepointCount];
            #endregion

            #region Public methods
            /// <summary>
            /// Loads the data using the specified reader. The data must be in the default
            /// Unicode standard format for a <c>UnicodeData.txt</c> file.
            /// </summary>
            [MethodImpl(HelperUtils.AggressiveOptimization)]
            public void Load(TextReader textReader)
            {
                // Load defaults for categories
                HelperUtils.Fill(Categories, (byte)UnicodeCategory.OtherNotAssigned);

                // Load defaults for bidi class. This is dependent on the range of characters
                // where a codepoint occurs.
                // (see https://www.unicode.org/Public/UCD/latest/ucd/extracted/DerivedBidiClass.txt).
                HelperUtils.Fill(BidiClasses, UnicodeBidiClass.LeftToRight);
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
                    int codePoint = DataHelper.ParseHex(line[CodePointField]);
                    string name = line[NameField];
                    if (rangeStartCodePoint != -1)
                    {
                        if (!name.EndsWith(EndOfRangeNameSuffix, StringComparison.Ordinal))
                            throw new InvalidOperationException("Start of range not followed by end of range");

                        for (int c = rangeStartCodePoint; c <= codePoint; c++)
                        {
                            Categories[c] = (byte)rangeCategory;
                            BidiClasses[c] = rangeBidiClass;
                        }

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
                ComputeNormalizationFlags();
            }
            #endregion

            #region Helper methods
            private void SetupBidiRange(int startCodepoint, int endCodepoint, UnicodeBidiClass bidiClass)
            {
                for (int c = startCodepoint; c <= endCodepoint; c++)
                    BidiClasses[c] = bidiClass;
            }

            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void UpdateDatabase(int codePoint, string[] line)
            {
                // Category
                Categories[codePoint] = (byte)UnicodeConversion.ConvertCategory(line[GeneralCategoryField]);

                // Combining class
                byte combiningClass = byte.Parse(line[CombiningClassField], CultureInfo.InvariantCulture);
                CombiningClasses[codePoint] = combiningClass;
                if (combiningClass != 0)
                    NormalizationFlagsTable[codePoint] |= NormalizationFlags.NonZeroCombiningClass;

                // Bidi class
                BidiClasses[codePoint] = UnicodeConversion.ConvertBidiClass(line[BidiClassField]);

                UCodepoint uc = (UCodepoint)codePoint;

                HandleDecomposition(uc, line[DecompositionTypeAndMappingField]);

                // Numeric value
                string numeric = line[NumericField];
                if (!string.IsNullOrEmpty(numeric))
                    NumericValues.Add(uc, UnicodeConversion.ConvertNumeric(numeric));
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(line[NumericDigitField]));
                    Debug.Assert(string.IsNullOrEmpty(line[NumericDecimalField]));
                }

                // Uppercase mapping
                string upperMapping = line[UppercaseMappingField];
                if (!string.IsNullOrEmpty(upperMapping))
                    UpperCaseMappings.Add(uc, (UCodepoint)DataHelper.ParseHex(upperMapping));

                // Lowercase mapping
                string lowerMapping = line[LowercaseMappingField];
                if (!string.IsNullOrEmpty(lowerMapping))
                    LowerCaseMappings.Add(uc, (UCodepoint)DataHelper.ParseHex(lowerMapping));

                // Titlecase mapping
                string titleMapping = line[TitleCaseMappingField];
                if (!string.IsNullOrEmpty(titleMapping))
                    TitleCaseMappings.Add(uc, (UCodepoint)DataHelper.ParseHex(titleMapping));
                else if (!string.IsNullOrEmpty(upperMapping))
                    TitleCaseMappings.Add(uc, (UCodepoint)DataHelper.ParseHex(upperMapping));
            }

            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void HandleDecomposition(UCodepoint uc, string decompositionStr)
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

                UCodepoint[] mapping = new UCodepoint[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    mapping[i] = (UCodepoint)DataHelper.ParseHex(parts[i]);
                DecompositionMapping.Add(Keys.CreateDecompKey(uc, compatMapping), mapping);
                if (!compatMapping)
                    DecompositionMapping.Add(Keys.CreateDecompKey(uc, true), mapping);

                if (mapping.Length == 2) // One-to-one mappings are not used for composition
                {
                    long key = Keys.CreateCombiningKey(mapping[0], mapping[1], compatMapping);
                    if (!CompositionMapping.ContainsKey(key))
                        CompositionMapping.Add(key, uc);
                    if (!compatMapping)
                        CompositionMapping.Add(Keys.CreateCombiningKey(mapping[0], mapping[1], true), uc);
                }

                // TODO: Verify more-than-two codepoint mappings are not used for composition.
                // They don't seem to be based on the test data and normalization reference implementation.
            }

            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void CleanUpCompositions()
            {
                // Remove any excluded compositions that haven't already been ignored
                CompositionExclusions compositionExclusions = new CompositionExclusions();
                List<long> toRemove = new List<long>();
                foreach (KeyValuePair<long, UCodepoint> kvp in CompositionMapping)
                {
                    Keys.UncreateCombiningKey(kvp.Key, out UCodepoint cpBase, out _, out _);
                    if (CombiningClasses[(int)cpBase] != 0 || compositionExclusions.IsExcluded(kvp.Value))
                        toRemove.Add(kvp.Key);
                }

                foreach (long key in toRemove)
                    CompositionMapping.Remove(key);
            }

            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void FullyExpandDecomposition()
            {
                List<UCodepoint> newMapping = new List<UCodepoint>();
                bool changedSomething;
                do
                {
                    changedSomething = false;
                    foreach (KeyValuePair<int, UCodepoint[]> kvp in DecompositionMapping.ToArray())
                    {
                        Keys.UncreateDecompKey(kvp.Key, out _, out bool compatMapping);

                        newMapping.Clear();
                        for (int i = 0; i < kvp.Value.Length; i++)
                        {
                            UCodepoint cp = kvp.Value[i];
                            if (!DecompositionMapping.TryGetValue(Keys.CreateDecompKey(cp, compatMapping), out UCodepoint[] decomposition))
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
                            HelperUtils.SortCanonical(newMappingArray, newMappingArray.Length, CombiningClasses); // Sort to avoid sorting later
                        DecompositionMapping[kvp.Key] = newMappingArray;
                    }
                }
                while (changedSomething);
            }

            /// <summary>
            /// Computes the <see cref="NormalizationFlags"/> for every codepoint from the finished tables.
            /// </summary>
            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void ComputeNormalizationFlags()
            {
                // (NonZeroCombiningClass is set by UpdateDatabase)
                NormalizationFlags[] flags = NormalizationFlagsTable;

                // Hangul syllables decompose algorithmically and vowel/trailing-consonant jamo compose
                // algorithmically (see NormalizationEngine.GetComposite for the exact ranges)
                for (int c = NormalizationEngine.SBase; c <= NormalizationEngine.SEnd; c++)
                    flags[c] |= NormalizationFlags.HangulSyllable;
                for (int c = NormalizationEngine.VBase; c <= NormalizationEngine.VEnd; c++)
                    flags[c] |= NormalizationFlags.ComposesAsSecond;
                for (int c = NormalizationEngine.TBase + 1; c <= NormalizationEngine.TEnd; c++) // TBase itself is not a T jamo
                    flags[c] |= NormalizationFlags.ComposesAsSecond;

                foreach (KeyValuePair<int, UCodepoint[]> kvp in DecompositionMapping)
                {
                    Keys.UncreateDecompKey(kvp.Key, out UCodepoint uc, out bool compatMapping);
                    flags[(int)uc] |= compatMapping ?
                        NormalizationFlags.HasCompatDecomposition : NormalizationFlags.HasCanonicalDecomposition;
                }

                // Primary composites are the codepoints that canonical composition can produce. Since they
                // are exactly what their own decomposition recomposes to, normalizing to Form C leaves them alone.
                HashSet<UCodepoint> primaryComposites = new HashSet<UCodepoint>();
                foreach (KeyValuePair<long, UCodepoint> kvp in CompositionMapping)
                {
                    Keys.UncreateCombiningKey(kvp.Key, out _, out UCodepoint cpCombining, out bool compatMapping);
                    if (compatMapping)
                        continue; // Only the canonical table is used for composition

                    flags[(int)cpCombining] |= NormalizationFlags.ComposesAsSecond;
                    primaryComposites.Add(kvp.Value);
                }

                // A codepoint whose decomposition begins with a composing second can itself combine with a
                // preceding codepoint once it has been decomposed (e.g. U+16D63 U+16D68 normalizes to U+16D6A
                // because U+16D68 decomposes to U+16D67 U+16D67), so it needs the flag as well. The mappings
                // are fully expanded, so a single pass suffices.
                foreach (KeyValuePair<int, UCodepoint[]> kvp in DecompositionMapping)
                {
                    Keys.UncreateDecompKey(kvp.Key, out UCodepoint uc, out _);
                    if ((flags[(int)kvp.Value[0]] & NormalizationFlags.ComposesAsSecond) != 0)
                        flags[(int)uc] |= NormalizationFlags.ComposesAsSecond;
                }

                foreach (KeyValuePair<int, UCodepoint[]> kvp in DecompositionMapping)
                {
                    Keys.UncreateDecompKey(kvp.Key, out UCodepoint uc, out bool compatMapping);
                    bool isPrimaryComposite = primaryComposites.Contains(uc);
                    if (!compatMapping)
                    {
                        if (!isPrimaryComposite)
                            flags[(int)uc] |= NormalizationFlags.NfcNo;
                    }
                    else
                    {
                        // Form KC leaves a codepoint alone only if it is a primary composite whose compatibility
                        // decomposition is no different from its canonical one (both are fully expanded here).
                        bool stable = isPrimaryComposite &&
                            DecompositionMapping.TryGetValue(Keys.CreateDecompKey(uc, false), out UCodepoint[] canonical) &&
                            AreSame(canonical, kvp.Value);
                        if (!stable)
                            flags[(int)uc] |= NormalizationFlags.NfkcNo;
                    }
                }
            }

            private static bool AreSame(UCodepoint[] mapping1, UCodepoint[] mapping2)
            {
                if (mapping1.Length != mapping2.Length)
                    return false;

                for (int i = 0; i < mapping1.Length; i++)
                {
                    if (mapping1[i] != mapping2[i])
                        return false;
                }

                return true;
            }
            #endregion
        }
        #endregion
    }
}
