using System;

namespace UnicodeHelper.Internal
{
    /// <summary>
    /// Per-codepoint facts used by <see cref="NormalizationEngine"/> to skip work. A codepoint with none
    /// of the relevant flags set is guaranteed to be left unchanged by the corresponding normalization form
    /// and can never affect the normalization of its neighbors.
    /// </summary>
    [Flags]
    internal enum NormalizationFlags : byte
    {
        /// <summary>The codepoint doesn't change when going to/from any normalization form</summary>
        None = 0,

        /// <summary>The codepoint has a canonical decomposition mapping</summary>
        HasCanonicalDecomposition = 1 << 0,

        /// <summary>The codepoint has a compatibility (or canonical) decomposition mapping</summary>
        HasCompatDecomposition = 1 << 1,

        /// <summary>The codepoint has a non-zero canonical combining class (may need reordering)</summary>
        NonZeroCombiningClass = 1 << 2,

        /// <summary>The codepoint can combine with a preceding codepoint to form a composite, either directly
        /// (this includes Hangul vowel and trailing-consonant jamo) or because its decomposition begins with
        /// such a codepoint</summary>
        ComposesAsSecond = 1 << 3,

        /// <summary>The codepoint is a precomposed Hangul syllable (decomposes algorithmically)</summary>
        HangulSyllable = 1 << 4,

        /// <summary>Normalizing to Form C changes this codepoint on its own (Unicode <c>NFC_QC=No</c>)</summary>
        NfcNo = 1 << 5,

        /// <summary>Normalizing to Form KC changes this codepoint on its own (Unicode <c>NFKC_QC=No</c>)</summary>
        NfkcNo = 1 << 6,

        // Masks of the flags that force the full normalization algorithm for each form
        FormDMask = HasCanonicalDecomposition | NonZeroCombiningClass | HangulSyllable,
        FormKDMask = HasCompatDecomposition | NonZeroCombiningClass | HangulSyllable,
        FormCMask = NfcNo | NonZeroCombiningClass | ComposesAsSecond,
        FormKCMask = NfkcNo | NonZeroCombiningClass | ComposesAsSecond,
    }
}
