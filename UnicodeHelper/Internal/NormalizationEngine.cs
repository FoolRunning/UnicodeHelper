using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnicodeHelper.Internal
{
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
        private static readonly ArrayPool<DecomposedItem> decomposedItemPool = ArrayPool<DecomposedItem>.Shared;
        #endregion

        #region Public methods
        public static UString Normalize(UString ustr, NormalizationForm normalizationForm)
        {
            switch (normalizationForm)
            {
                case NormalizationForm.FormC: return NormalizeFormC(ustr);
                case NormalizationForm.FormD: return NormalizeFormD(ustr);
                default: throw new NotImplementedException("Normalization not supported: " + normalizationForm);
            }
        }
        #endregion
        
        #region Composition (NFC) methods
        private static UString NormalizeFormC(UString ustr)
        {
            if (ustr.Length == 0)
                return ustr;

            using (UStringBuilder decomposedSb = new UStringBuilder(ustr.Length * 5 / 4))
            {
                NormalizeFormDToSB(ustr, decomposedSb);
                
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
                        UCodepoint composite = GetComposite(ucStarter, uc);
                        if (composite != UCodepoint.Null && lastCombiningClass < combiningClass)
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

        #region Decomposition (NFD) methods
        private static UString NormalizeFormD(UString ustr)
        {
            using (UStringBuilder sb = new UStringBuilder(ustr.Length * 5 / 4))
            {
                NormalizeFormDToSB(ustr, sb);
                return sb.ToUString();
            }
        }

        private static void NormalizeFormDToSB(UString ustr, UStringBuilder sb)
        {
            DecomposedItem[] decomposedItem = decomposedItemPool.Rent(32); // TODO: Figure out reasonable size
            try
            {
                int itemCount = 0;
                foreach (UCodepoint uc in ustr)
                {
                    if (UnicodeData.GetCombiningClass(uc) == 0 && itemCount > 0)
                    {
                        Array.Sort(decomposedItem, 0, itemCount);
                        for (int i = 0; i < itemCount; i++)
                            sb.Append(decomposedItem[i].Codepoint);

                        itemCount = 0;
                    }

                    AppendDecomposedItem(uc, decomposedItem, ref itemCount);
                }

                Array.Sort(decomposedItem, 0, itemCount);
                for (int i = 0; i < itemCount; i++)
                    sb.Append(decomposedItem[i].Codepoint);
            }
            finally
            {
                decomposedItemPool.Return(decomposedItem);
            }
        }

        private static void AppendDecomposedItem(UCodepoint uc, DecomposedItem[] decomposedItem, ref int itemCount)
        {
            UCodepoint[] decomposition = UnicodeData.GetDecomposition(uc);
            if (decomposition != null)
            {
                foreach (UCodepoint decompUc in decomposition)
                    AppendDecomposedItem(decompUc, decomposedItem, ref itemCount);
            }
            else if (uc < SBase || uc > SEnd)
                decomposedItem[itemCount++] = new DecomposedItem(uc);
            else
                AppendDecomposeHangul(uc, decomposedItem, ref itemCount);

        }

        /// <summary>
        /// Decomposes a Hangul syllable into its components and appends them to the list.
        /// </summary>
        /// <remarks>Algorithm taken from https://www.unicode.org/standard/reports/tr15/tr15-21.html </remarks>
        private static void AppendDecomposeHangul(UCodepoint uc, DecomposedItem[] decomposedItem, ref int itemCount)
        {
            int syllableSIndex = (int)uc - SBase;
            int syllableLIndex = syllableSIndex / NCount;
            int syllableVIndex = (syllableSIndex % NCount) / TCount;
            int syllableTIndex = syllableSIndex % TCount;

            decomposedItem[itemCount++] = new DecomposedItem((UCodepoint)(LBase + syllableLIndex));
            decomposedItem[itemCount++] = new DecomposedItem((UCodepoint)(VBase + syllableVIndex));
            
            if (syllableTIndex > 0)
                decomposedItem[itemCount++] = new DecomposedItem((UCodepoint)(TBase + syllableTIndex));
        }
        #endregion
    
        #region DecomposedItem struct
        private readonly struct DecomposedItem : IComparable<DecomposedItem>
        {
            public readonly UCodepoint Codepoint;

            private readonly byte _combiningClass;

            public DecomposedItem(UCodepoint codepoint)
            {
                Codepoint = codepoint;
                _combiningClass = UnicodeData.GetCombiningClass(codepoint);
            }

            public int CompareTo(DecomposedItem other)
            {
                return _combiningClass.CompareTo(other._combiningClass);
            }
        }
        #endregion
    }
}
