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
        private const int SBase = 0xAC00;
        private const int LBase = 0x1100;
        private const int VBase = 0x1161;
        private const int TBase = 0x11A7;
        private const int LCount = 19;
        private const int VCount = 21;
        private const int TCount = 28;
        private const int NCount = VCount * TCount;   // 588
        private const int SCount = LCount * NCount;   // 11172
        private const int LEnd = LBase + LCount - 1; // Inclusive end of L range
        private const int VEnd = VBase + VCount - 1; // Inclusive end of V range
        private const int TEnd = TBase + TCount - 1; // Inclusive end of T range
        private const int SEnd = SBase + SCount - 1; // Inclusive end of syllable range
        #endregion

        #region Data fields
        private static readonly ArrayPool<UCodepoint> decomposedItemPool = ArrayPool<UCodepoint>.Shared;
        #endregion

        #region Public methods
        public static UString Normalize(UString ustr, NormalizationForm normalizationForm)
        {
            switch (normalizationForm)
            {
                case NormalizationForm.FormD: return NormalizeFormD(ustr);
                case NormalizationForm.FormC: return NormalizeFormC(ustr, false);
                case NormalizationForm.FormKD: return NormalizeFormKD(ustr);
                case NormalizationForm.FormKC: return NormalizeFormC(ustr, true);
                default: throw new NotImplementedException("Normalization not supported: " + normalizationForm);
            }
        }
        #endregion

        #region Helper methods
        private static UString NormalizeFormD(UString ustr)
        {
            using (UStringBuilder sb = new UStringBuilder(ustr.Length * 5 / 4))
            {
                DecomposeToSB(ustr, false, sb);
                return sb.ToUString();
            }
        }

        private static UString NormalizeFormKD(UString ustr)
        {
            using (UStringBuilder sb = new UStringBuilder(ustr.Length * 3 / 2))
            {
                DecomposeToSB(ustr, true, sb);
                return sb.ToUString();
            }
        }

        private static void DecomposeToSB(UString ustr, bool compatMapping, UStringBuilder sb)
        {
            UCodepoint[] decomposedChar = decomposedItemPool.Rent(50); // TODO: Figure out reasonable size
            try
            {
                int cpCount = 0;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < ustr.Length; i++)
                {
                    UCodepoint uc = ustr[i];
                    if (UnicodeData.GetCombiningClass(uc) == 0)
                    {
                        Debug.Assert(i == 0 || cpCount > 0);
                        HelperUtils.SortCanonical(decomposedChar, cpCount);
                        sb.Append(decomposedChar, cpCount);

                        cpCount = 0;
                    }

                    AppendDecomposedItem(uc, compatMapping, decomposedChar, ref cpCount);
                }

                HelperUtils.SortCanonical(decomposedChar, cpCount);
                sb.Append(decomposedChar, cpCount);
            }
            finally
            {
                decomposedItemPool.Return(decomposedChar);
            }
        }

        private static void AppendDecomposedItem(UCodepoint uc, bool compatMapping, UCodepoint[] decomposedChar, ref int cpCount)
        {
            UCodepoint[] ucDecomp = UnicodeData.GetDecomposition(uc, compatMapping);
            if (ucDecomp != null)
            {
                Array.Copy(ucDecomp, 0, decomposedChar, cpCount, ucDecomp.Length);
                cpCount += ucDecomp.Length;
            }
            else if (uc < SBase || uc > SEnd)
                decomposedChar[cpCount++] = uc; // Not a Hangul syllable
            else
                AppendDecomposeHangul(uc, decomposedChar, ref cpCount);
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

        private static UString NormalizeFormC(UString ustr, bool compatMapping)
        {
            if (ustr.Length == 0)
                return ustr;

            using (UStringBuilder decomposedSb = new UStringBuilder(ustr.Length * 5 / 4))
            {
                DecomposeToSB(ustr, compatMapping, decomposedSb);
                return ComposeFromSB(decomposedSb);
            }
        }

        private static UString ComposeFromSB(UStringBuilder decomposedSb)
        {
            using (UStringBuilder resultSb = new UStringBuilder(decomposedSb.Length, decomposedSb.Length))
            {
                UCodepoint ucStarter = decomposedSb[0];
                resultSb[0] = ucStarter;
                int starterIndex = 0;
                int lastCombiningClass = -1;
                int targetIndex = 1;
                for (int i = 1; i < decomposedSb.Length; i++)
                {
                    UCodepoint uc = decomposedSb[i];
                    byte combiningClass = UnicodeData.GetCombiningClass(uc);
                    UCodepoint composite;
                    if (lastCombiningClass < combiningClass && 
                        (composite = GetComposite(ucStarter, uc)) != UCodepoint.Null)
                    {
                        resultSb[starterIndex] = composite;
                        ucStarter = composite;
                    }
                    else if (combiningClass == 0)
                    {
                        starterIndex = targetIndex;
                        ucStarter = uc;
                        lastCombiningClass = -1;
                        resultSb[targetIndex++] = uc;
                    }
                    else
                    {
                        lastCombiningClass = combiningClass;
                        resultSb[targetIndex++] = uc;
                    }
                }

                resultSb.Length = targetIndex;

                return resultSb.ToUString();
            }
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
            if (ucStarter >= SBase && ucStarter <= SEnd && 
                ((int)ucStarter - SBase) % TCount == 0 &&
                ucCombining >= TBase && ucCombining <= TEnd)
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
