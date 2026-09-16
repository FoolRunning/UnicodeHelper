using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnicodeHelper.Internal
{
    /// <summary>
    /// Handles normalization of Unicode strings.
    /// </summary>
    /// <remarks>Most of this is ported from the reference implementation at
    /// https://www.w3.org/International/charlint/ </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class NormalizationEngine
    {
        #region Constants
        // Constants for Hangul syllable decomposition
        internal const int SBase = 0xAC00;
        internal const int LBase = 0x1100;
        internal const int VBase = 0x1161;
        internal const int TBase = 0x11A7;
        internal const int LCount = 19;
        internal const int VCount = 21;
        internal const int TCount = 28;
        internal const int NCount = VCount * TCount;   // 588
        internal const int SCount = LCount * NCount;   // 11172
        internal const int LEnd = LBase + LCount - 1; // Inclusive end of L range
        internal const int VEnd = VBase + VCount - 1; // Inclusive end of V range
        internal const int TEnd = TBase + TCount - 1; // Inclusive end of T range
        internal const int SEnd = SBase + SCount - 1; // Inclusive end of syllable range

        /// <summary>Size of the working buffer that holds one decomposed starter with its marks</summary>
        private const int DecompositionScratchSize = 50; // TODO: Figure out reasonable size
        #endregion

        #region Data fields
        private static readonly ArrayPool<UCodepoint> decomposedItemPool = ArrayPool<UCodepoint>.Shared;
        #endregion

        #region Public methods
        public static UString Normalize(UString ustr, NormalizationForm normalizationForm)
        {
            // Quick check: most real-world text is already normalized, so a single pass over a flags table
            // avoids the decomposition and composition passes (and their allocations) entirely.
            if (PassesQuickCheck(ustr, normalizationForm))
                return ustr;

            switch (normalizationForm)
            {
                case NormalizationForm.FormD: return NormalizeDecomposed(ustr, false);
                case NormalizationForm.FormC: return NormalizeComposed(ustr, false);
                case NormalizationForm.FormKD: return NormalizeDecomposed(ustr, true);
                case NormalizationForm.FormKC: return NormalizeComposed(ustr, true);
                default: throw new NotImplementedException("Normalization not supported: " + normalizationForm);
            }
        }

        /// <summary>
        /// Determines whether the string is already in the specified normalization form, i.e. whether
        /// normalizing it would leave it unchanged.
        /// </summary>
        /// <remarks>
        /// Implements the detection approach of <see href="https://www.unicode.org/reports/tr15/#Detecting_Normalization_Forms">UAX #15</see>.
        /// Codepoints that pass the quick check are provably normalized and provably isolated from their
        /// neighbors, so they are skipped. Only the segments that contain a "maybe" codepoint (e.g. a combining
        /// mark, or a jamo that might compose with the syllable before it) are examined further: for the
        /// decomposed forms this is just a check of canonical ordering, and for the composed forms the segment
        /// (which is typically a single starter with a few marks) is normalized into a pooled buffer and
        /// compared to the input. Nothing outside of such segments is ever touched, and no strings are allocated.
        /// </remarks>
        public static bool IsNormalized(UString ustr, NormalizationForm normalizationForm)
        {
            NormalizationFlags quickCheckMask = GetQuickCheckMask(normalizationForm);
            UCodepoint[] codepoints = ustr.Codepoints;
            int end = ustr.StartIndex + ustr.Length;

            // The common case: everything passes the quick check
            int i = FindQuickCheckFailure(codepoints, ustr.StartIndex, end, quickCheckMask);
            if (i == end)
                return true;

            NormalizationFlags notNormalizedMask = GetNotNormalizedMask(normalizationForm);
            bool decomposedForm = normalizationForm == NormalizationForm.FormD || normalizationForm == NormalizationForm.FormKD;
            bool compatMapping = normalizationForm == NormalizationForm.FormKD || normalizationForm == NormalizationForm.FormKC;

            // Buffers for checking composed segments, rented once for the whole string
            UStringBuilder segmentSb = null;
            UCodepoint[] scratch = null;
            try
            {
                // Each segment runs from the last codepoint that passed the quick check (the starter that a
                // following codepoint might combine with) up to the next codepoint that passes it.
                int segmentStart = i > ustr.StartIndex ? i - 1 : i;
                while (true)
                {
                    int segmentEnd = i;
                    NormalizationFlags flags = UnicodeData.GetNormalizationFlags(codepoints[segmentEnd]);
                    do
                    {
                        if ((flags & notNormalizedMask) != NormalizationFlags.None)
                            return false; // This codepoint can never appear in normalized text

                        segmentEnd++;
                        if (segmentEnd == end)
                            break;

                        flags = UnicodeData.GetNormalizationFlags(codepoints[segmentEnd]);
                    } while ((flags & quickCheckMask) != NormalizationFlags.None);

                    if (decomposedForm)
                    {
                        // Nothing in the segment decomposes, so it is normalized if the marks are in canonical order
                        if (!IsCanonicallyOrdered(codepoints, segmentStart, segmentEnd))
                            return false;
                    }
                    else
                    {
                        if (segmentSb == null)
                        {
                            segmentSb = new UStringBuilder();
                            scratch = decomposedItemPool.Rent(DecompositionScratchSize);
                        }

                        if (!IsComposedSegmentNormalized(codepoints, segmentStart, segmentEnd - segmentStart, compatMapping, segmentSb, scratch))
                            return false;
                    }

                    if (segmentEnd == end)
                        return true;

                    // The codepoint at segmentEnd passed the quick check; skip ahead to the next one that doesn't
                    i = FindQuickCheckFailure(codepoints, segmentEnd + 1, end, quickCheckMask);
                    if (i == end)
                        return true;
                    segmentStart = i - 1;
                }
            }
            finally
            {
                segmentSb?.Dispose();
                if (scratch != null)
                    decomposedItemPool.Return(scratch);
            }
        }
        #endregion

        #region Helper methods
        private static NormalizationFlags GetQuickCheckMask(NormalizationForm normalizationForm)
        {
            switch (normalizationForm)
            {
                case NormalizationForm.FormD: return NormalizationFlags.FormDMask;
                case NormalizationForm.FormC: return NormalizationFlags.FormCMask;
                case NormalizationForm.FormKD: return NormalizationFlags.FormKDMask;
                case NormalizationForm.FormKC: return NormalizationFlags.FormKCMask;
                default: throw new NotImplementedException("Normalization not supported: " + normalizationForm);
            }
        }

        /// <summary>
        /// Gets the flags that mark a codepoint that can never appear in text normalized to the specified form.
        /// </summary>
        private static NormalizationFlags GetNotNormalizedMask(NormalizationForm normalizationForm)
        {
            switch (normalizationForm)
            {
                case NormalizationForm.FormD: return NormalizationFlags.HasCanonicalDecomposition | NormalizationFlags.HangulSyllable;
                case NormalizationForm.FormC: return NormalizationFlags.NfcNo;
                case NormalizationForm.FormKD: return NormalizationFlags.HasCompatDecomposition | NormalizationFlags.HangulSyllable;
                case NormalizationForm.FormKC: return NormalizationFlags.NfkcNo;
                default: throw new NotImplementedException("Normalization not supported: " + normalizationForm);
            }
        }

        /// <summary>
        /// Determines whether every codepoint of the string passes the quick check for the specified form.
        /// A result of <c>true</c> guarantees the string is normalized; <c>false</c> means it might not be.
        /// </summary>
        private static bool PassesQuickCheck(UString ustr, NormalizationForm normalizationForm)
        {
            int end = ustr.StartIndex + ustr.Length;
            return FindQuickCheckFailure(ustr.Codepoints, ustr.StartIndex, end, GetQuickCheckMask(normalizationForm)) == end;
        }

        /// <summary>
        /// Finds the index of the first codepoint in the range that does not pass the quick check
        /// (i.e. that might not be normalized), or <paramref name="end"/> if all of them pass.
        /// </summary>
        private static int FindQuickCheckFailure(UCodepoint[] codepoints, int start, int end, NormalizationFlags quickCheckMask)
        {
            int i = start;
            while (i < end && (UnicodeData.GetNormalizationFlags(codepoints[i]) & quickCheckMask) == NormalizationFlags.None)
                i++;
            return i;
        }

        /// <summary>
        /// Determines whether the combining marks in the specified range are in canonical order
        /// (non-decreasing combining class within each run of non-starters).
        /// </summary>
        private static bool IsCanonicallyOrdered(UCodepoint[] codepoints, int start, int end)
        {
            byte lastCombiningClass = 0;
            for (int i = start; i < end; i++)
            {
                byte combiningClass = UnicodeData.GetCombiningClass(codepoints[i]);
                if (combiningClass != 0 && combiningClass < lastCombiningClass)
                    return false;
                lastCombiningClass = combiningClass;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified segment is unchanged by composed normalization by actually
        /// normalizing it (into a pooled buffer) and comparing.
        /// </summary>
        private static bool IsComposedSegmentNormalized(UCodepoint[] codepoints, int start, int count, bool compatMapping,
            UStringBuilder sb, UCodepoint[] scratch)
        {
            sb.Clear();
            Decompose(codepoints, start, count, compatMapping, sb, scratch);
            ComposeInPlace(sb);

            if (sb.Length != count)
                return false;

            for (int i = 0; i < count; i++)
            {
                if (sb[i] != codepoints[start + i])
                    return false;
            }

            return true;
        }

        private static UString NormalizeDecomposed(UString ustr, bool compatMapping)
        {
            UCodepoint[] scratch = decomposedItemPool.Rent(DecompositionScratchSize);
            try
            {
                using (UStringBuilder sb = new UStringBuilder(ustr.Length * (compatMapping ? 3 : 5) / (compatMapping ? 2 : 4)))
                {
                    Decompose(ustr.Codepoints, ustr.StartIndex, ustr.Length, compatMapping, sb, scratch);
                    return sb.ToUString();
                }
            }
            finally
            {
                decomposedItemPool.Return(scratch);
            }
        }

        private static UString NormalizeComposed(UString ustr, bool compatMapping)
        {
            if (ustr.Length == 0)
                return ustr;

            UCodepoint[] scratch = decomposedItemPool.Rent(DecompositionScratchSize);
            try
            {
                using (UStringBuilder sb = new UStringBuilder(ustr.Length * 5 / 4))
                {
                    Decompose(ustr.Codepoints, ustr.StartIndex, ustr.Length, compatMapping, sb, scratch);
                    ComposeInPlace(sb);
                    return sb.ToUString();
                }
            }
            finally
            {
                decomposedItemPool.Return(scratch);
            }
        }

        /// <summary>
        /// Appends the full canonical (or compatibility) decomposition of the specified codepoints, in
        /// canonical order, to the builder.
        /// </summary>
        /// <remarks><paramref name="scratch"/> is working space for one decomposed starter and its marks
        /// (at least <see cref="DecompositionScratchSize"/> elements). Callers rent it so that repeated calls
        /// (e.g. once per segment in <see cref="IsNormalized"/>) share a single buffer.</remarks>
        private static void Decompose(UCodepoint[] codepoints, int start, int count, bool compatMapping,
            UStringBuilder sb, UCodepoint[] scratch)
        {
            NormalizationFlags decompositionFlag = compatMapping ?
                NormalizationFlags.HasCompatDecomposition : NormalizationFlags.HasCanonicalDecomposition;

            int end = start + count;
            int cpCount = 0;
            for (int i = start; i < end; i++)
            {
                UCodepoint uc = codepoints[i];
                NormalizationFlags flags = UnicodeData.GetNormalizationFlags(uc);
                if ((flags & NormalizationFlags.NonZeroCombiningClass) == NormalizationFlags.None)
                {
                    Debug.Assert(i == start || cpCount > 0);
                    HelperUtils.SortCanonical(scratch, cpCount);
                    sb.Append(scratch, cpCount);

                    cpCount = 0;
                }

                if ((flags & decompositionFlag) != NormalizationFlags.None)
                {
                    UCodepoint[] ucDecomp = UnicodeData.GetDecomposition(uc, compatMapping);
                    Array.Copy(ucDecomp, 0, scratch, cpCount, ucDecomp.Length);
                    cpCount += ucDecomp.Length;
                }
                else if ((flags & NormalizationFlags.HangulSyllable) != NormalizationFlags.None)
                    AppendDecomposeHangul(uc, scratch, ref cpCount);
                else
                    scratch[cpCount++] = uc;
            }

            HelperUtils.SortCanonical(scratch, cpCount);
            sb.Append(scratch, cpCount);
        }

        /// <summary>
        /// Decomposes a Hangul syllable into its components and appends them to the list.
        /// </summary>
        /// <remarks>Algorithm taken from https://www.unicode.org/standard/reports/tr15/tr15-21.html </remarks>
        private static void AppendDecomposeHangul(UCodepoint uc, UCodepoint[] decomposedChar, ref int cpCount)
        {
            int syllableSIndex = (int)uc - SBase;
            int syllableLIndex = syllableSIndex / NCount;
            int syllableVIndex = (syllableSIndex % NCount) / TCount;
            int syllableTIndex = syllableSIndex % TCount;

            decomposedChar[cpCount++] = (UCodepoint)(LBase + syllableLIndex);
            decomposedChar[cpCount++] = (UCodepoint)(VBase + syllableVIndex);

            if (syllableTIndex > 0)
                decomposedChar[cpCount++] = (UCodepoint)(TBase + syllableTIndex);
        }

        /// <summary>
        /// Canonically composes the fully decomposed, canonically ordered contents of the builder in place
        /// (the composed result is never longer than the decomposed input, so it can overwrite it).
        /// </summary>
        private static void ComposeInPlace(UStringBuilder sb)
        {
            if (sb.Length == 0)
                return;

            UCodepoint ucStarter = sb[0];
            int starterIndex = 0;
            int lastCombiningClass = -1;
            int targetIndex = 1;
            for (int i = 1; i < sb.Length; i++)
            {
                UCodepoint uc = sb[i];
                byte combiningClass = UnicodeData.GetCombiningClass(uc);
                UCodepoint composite;
                if (lastCombiningClass < combiningClass &&
                    // Only codepoints flagged as composing seconds can ever have a composite, so the
                    // (comparatively expensive) composite lookup is skipped for everything else
                    (UnicodeData.GetNormalizationFlags(uc) & NormalizationFlags.ComposesAsSecond) != NormalizationFlags.None &&
                    (composite = GetComposite(ucStarter, uc)) != UCodepoint.Null)
                {
                    sb[starterIndex] = composite;
                    ucStarter = composite;
                }
                else if (combiningClass == 0)
                {
                    starterIndex = targetIndex;
                    ucStarter = uc;
                    lastCombiningClass = -1;
                    sb[targetIndex++] = uc;
                }
                else
                {
                    lastCombiningClass = combiningClass;
                    sb[targetIndex++] = uc;
                }
            }

            sb.Length = targetIndex;
        }

        /// <summary>
        /// Determines the composite character for a given starter and combining character.
        /// </summary>
        /// <remarks>Algorithm taken from https://www.unicode.org/standard/reports/tr15/tr15-21.html </remarks>
        private static UCodepoint GetComposite(UCodepoint ucStarter, UCodepoint ucCombining)
        {
            // Check for Hangul syllable composition first
            // 1. check to see if two current characters are L and V
            if (ucStarter >= LBase && ucStarter <= LEnd &&
                ucCombining >= VBase && ucCombining <= VEnd)
            {
                int lIndex = (int)ucStarter - LBase;
                int vIndex = (int)ucCombining - VBase;
                return (UCodepoint)((lIndex * VCount + vIndex) * TCount + SBase);
            }

            // 2. check to see if two current characters are LV and T
            // (TBase itself is not a trailing consonant: a T index of 0 means "no trailing consonant",
            // so composing with it would silently drop the character)
            if (ucStarter >= SBase && ucStarter <= SEnd &&
                ((int)ucStarter - SBase) % TCount == 0 &&
                ucCombining > TBase && ucCombining <= TEnd)
            {
                int tIndex = (int)ucCombining - TBase;
                return ucStarter + tIndex;
            }

            // Not Hangul, so look up in composition table
            return UnicodeData.GetComposition(ucStarter, ucCombining);
        }
        #endregion
    }
}
