using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnicodeHelper.Internal
{
    internal static class HelperUtils
    {
        /// <summary>
        /// <c>MethodImplOptions.AggressiveOptimization</c>: makes the JIT fully optimize the method on first
        /// call instead of starting with unoptimized (tier-0) code. The named enum member does not exist in
        /// .NET Standard 2.0; runtimes that predate it (.NET Framework) ignore the flag.
        /// </summary>
        /// <remarks>Used on the data-loading code. Tiered compilation only promotes hot methods after a
        /// quiet period with no new methods being compiled, which never happens during initialization,
        /// so without this the loaders run to completion as unoptimized code (roughly 25% slower).</remarks>
        public const MethodImplOptions AggressiveOptimization = (MethodImplOptions)512;

        public static TextDirection DetermineDirection(IEnumerable<UCodepoint> codepoints)
        {
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
        /// Fills the entire array with the specified value.
        /// </summary>
        /// <remarks>Fills a small block by hand and then doubles it with <see cref="Array.Copy(Array,Array,int)"/>,
        /// which runs at memmove speed. This is much faster than a plain loop for the large per-codepoint
        /// tables, particularly since initialization code usually runs before the JIT has optimized it.</remarks>
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        public static void Fill<T>(T[] array, T value)
        {
            int filled = Math.Min(array.Length, 32);
            for (int i = 0; i < filled; i++)
                array[i] = value;

            while (filled < array.Length)
            {
                int count = Math.Min(filled, array.Length - filled);
                Array.Copy(array, 0, array, filled, count);
                filled += count;
            }
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
            SortCanonical(decomposedChar, count, UnicodeData.CombiningClassTable);
        }

        /// <summary>
        /// Sorts the specified decomposed character array in Unicode canonical order using the
        /// specified table of combining classes (indexed by codepoint).
        /// </summary>
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        public static void SortCanonical(UCodepoint[] decomposedChar, int count, byte[] combiningClasses)
        {
            for (int i = 1; i < count; i++)
            {
                byte ucClass = combiningClasses[(int)decomposedChar[i]];
                if (ucClass == 0)
                    continue;

                byte ucClassPrev = combiningClasses[(int)decomposedChar[i - 1]];
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
