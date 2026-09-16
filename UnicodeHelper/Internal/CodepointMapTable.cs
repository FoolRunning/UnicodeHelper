using System.Runtime.CompilerServices;

namespace UnicodeHelper.Internal
{
    /// <summary>
    /// A sparse codepoint-to-codepoint mapping (e.g. case mappings) where unmapped codepoints map to themselves.
    /// </summary>
    /// <remarks>Implemented as a two-level table: the codepoint space is divided into 256-codepoint pages
    /// and only pages that contain at least one mapping are allocated. Lookups are two array reads with no
    /// hashing, which is roughly an order of magnitude faster than a dictionary lookup, while the memory
    /// cost is about 1KB per populated page (a few dozen pages for case mappings).</remarks>
    internal sealed class CodepointMapTable
    {
        #region Constants
        private const int PageShift = 8;
        private const int PageSize = 1 << PageShift;
        private const int PageMask = PageSize - 1;
        private const int PageCount = UnicodeData.UnicodeCodepointCount >> PageShift;
        #endregion

        #region Data fields
        /// <summary>Pages of mappings. A <c>null</c> page means every codepoint in it maps to itself.</summary>
        private readonly UCodepoint[][] _pages = new UCodepoint[PageCount][];
        #endregion

        #region Public methods
        /// <summary>
        /// Adds a mapping from one codepoint to another
        /// </summary>
        public void Add(UCodepoint from, UCodepoint to)
        {
            int codepoint = (int)from;
            int pageIndex = codepoint >> PageShift;
            UCodepoint[] page = _pages[pageIndex];
            if (page == null)
            {
                // New page: start as the identity mapping
                page = new UCodepoint[PageSize];
                int pageBase = pageIndex << PageShift;
                for (int i = 0; i < PageSize; i++)
                    page[i] = (UCodepoint)(pageBase + i);
                _pages[pageIndex] = page;
            }

            page[codepoint & PageMask] = to;
        }

        /// <summary>
        /// Gets the codepoint that the specified codepoint maps to (the codepoint itself if it has no mapping)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UCodepoint Map(UCodepoint uc)
        {
            int codepoint = (int)uc;
            UCodepoint[] page = _pages[codepoint >> PageShift];
            return page == null ? uc : page[codepoint & PageMask];
        }
        #endregion
    }
}
