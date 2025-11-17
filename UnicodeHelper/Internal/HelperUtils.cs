using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnicodeHelper.Internal
{
    internal static class HelperUtils
    {
        public static TextDirection DetermineDirection(IEnumerable<UCodepoint> codepoints)
        {
            if (codepoints == null)
                throw new ArgumentNullException(nameof(codepoints));

            bool inIsolate = false;
            foreach (UCodepoint uc in codepoints)
            {
                switch (UCodepoint.GetBidiClass(uc))
                {
                    case UnicodeBidiClass.LeftToRightIsolate: 
                    case UnicodeBidiClass.RightToLeftIsolate:
                        inIsolate = true;
                        break;
                    
                    case UnicodeBidiClass.PopDirectionalIsolate: 
                        inIsolate = false; 
                        break;

                    case UnicodeBidiClass.RightToLeft:
                    case UnicodeBidiClass.ArabicLetter:
                        if (!inIsolate)
                            return TextDirection.RtL;
                        break;
                    
                    case UnicodeBidiClass.LeftToRight:
                        if (!inIsolate)
                            return TextDirection.LtR;
                        break;
                }
            }
            
            return TextDirection.Undefined;
        }

        /// <summary>
        /// Efficiently converts a bool to an int (0 or 1).
        /// </summary>
        /// <remarks>Taken from https://stackoverflow.com/a/66993553 </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int BoolToInt(bool b)
        {
            return *(byte*)&b;
        }

        /// <summary>
        /// Sorts the specified decomposed character array in Unicode canonical order (based on combining class).
        /// </summary>
        public static void SortCanonical(UCodepoint[] decomposedChar, int count)
        {
            for (int i = 1; i < count; i++)
            {
                byte ucClass = UnicodeData.GetCombiningClass(decomposedChar[i]);
                if (ucClass == 0)
                    continue;

                byte ucClassPrev = UnicodeData.GetCombiningClass(decomposedChar[i - 1]);
                if (ucClassPrev <= ucClass) 
                    continue;

                // Swap items
                (decomposedChar[i], decomposedChar[i - 1]) = (decomposedChar[i - 1], decomposedChar[i]);
                if (i > 1)
                    i -= 2; // Re-evaluate previous items
            }
        }
    }
}
