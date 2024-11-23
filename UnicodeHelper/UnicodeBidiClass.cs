using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Unicode bidirectional class values
    /// </summary>
    /// <remarks>Descriptions taken directly from the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#Bidi_Class_Values">Bidi_Class_Values</see></remarks>
    [PublicAPI]
    public enum UnicodeBidiClass : byte
    {
        #region Strong types
        /// <summary>
        /// Any strong left-to-right character. Signified by the Unicode bidi designation "L". The value is 0.
        /// </summary>
        LeftToRight,
        /// <summary>
        /// Any strong right-to-left (non-Arabic-type) character. Signified by the Unicode bidi designation "R". The value is 1.
        /// </summary>
        RightToLeft,
        /// <summary>
        /// Any strong right-to-left (Arabic-type) character. Signified by the Unicode bidi designation "AL". The value is 2.
        /// </summary>
        ArabicLetter,
        #endregion

        #region Weak types
        /// <summary>
        /// Any ASCII digit or Eastern Arabic-Indic digit. Signified by the Unicode bidi designation "EN". The value is 3.
        /// </summary>
        EuropeanNumber,
        /// <summary>
        /// Plus and minus signs. Signified by the Unicode bidi designation "ES". The value is 4.
        /// </summary>
        EuropeanSeparator,
        /// <summary>
        /// A terminator in a numeric format context, includes currency signs. Signified by the Unicode bidi designation "ET". The value is 5.
        /// </summary>
        EuropeanTerminator,
        /// <summary>
        /// Any Arabic-Indic digit. Signified by the Unicode bidi designation "AN". The value is 6.
        /// </summary>
        ArabicNumber,
        /// <summary>
        /// Commas, colons, and slashes. Signified by the Unicode bidi designation "CS". The value is 7.
        /// </summary>
        CommonSeparator,
        /// <summary>
        /// Any non-spacing mark. Signified by the Unicode bidi designation "NSM". The value is 8.
        /// </summary>
        NonSpacingMark,
        /// <summary>
        /// Most format characters, control codes, or non-characters. Signified by the Unicode bidi designation "BN". The value is 9.
        /// </summary>
        BoundaryNeutral,
        #endregion
        
        #region Neutral types
        /// <summary>
        /// Various newline characters. Signified by the Unicode bidi designation "B". The value is 10.
        /// </summary>
        ParagraphSeparator,
        /// <summary>
        /// Various segment-related control codes. Signified by the Unicode bidi designation "S". The value is 11.
        /// </summary>
        SegmentSeparator,
        /// <summary>
        /// Spaces. Signified by the Unicode bidi designation "WS". The value is 12.
        /// </summary>
        WhiteSpace,
        /// <summary>
        /// Most other symbols and punctuation marks. Signified by the Unicode bidi designation "ON". The value is 13.
        /// </summary>
        OtherNeutral,
        #endregion
        
        #region Explicit formatting types
        /// <summary>
        /// U+202A: the LR embedding control. Signified by the Unicode bidi designation "LRE". The value is 14.
        /// </summary>
        LeftToRightEmbedding,
        /// <summary>
        /// U+202D: the LR override control. Signified by the Unicode bidi designation "LRO". The value is 15.
        /// </summary>
        LeftToRightOverride,
        /// <summary>
        /// U+202B: the RL embedding control. Signified by the Unicode bidi designation "RLE". The value is 16.
        /// </summary>
        RightToLeftEmbedding,
        /// <summary>
        /// U+202E: the RL override control. Signified by the Unicode bidi designation "RLO". The value is 17.
        /// </summary>
        RightToLeftOverride,
        /// <summary>
        /// U+202C: terminates an embedding or override control. Signified by the Unicode bidi designation "PDF". The value is 18.
        /// </summary>
        PopDirectionalFormat,
        /// <summary>
        /// U+2066: the LR isolate control. Signified by the Unicode bidi designation "LRI". The value is 19.
        /// </summary>
        LeftToRightIsolate,
        /// <summary>
        /// U+2067: the RL isolate control. Signified by the Unicode bidi designation "RLI". The value is 20.
        /// </summary>
        RightToLeftIsolate,
        /// <summary>
        /// U+2068: the first strong isolate control. Signified by the Unicode bidi designation "FSI". The value is 21.
        /// </summary>
        FirstStrongIsolate,
        /// <summary>
        /// U+2069: terminates an isolate control. Signified by the Unicode bidi designation "PDI". The value is 22.
        /// </summary>
        PopDirectionalIsolate
        #endregion
    }
}
