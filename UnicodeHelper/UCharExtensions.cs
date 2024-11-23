namespace UnicodeHelper
{
    #region HexPadding enum
    /// <summary>
    /// The type of padding for hex representations of <see cref="UChar"/>s
    /// </summary>
    public enum HexPadding
    {
        /// <summary>
        /// For most characters, this results in a 4-character hex string padded with zeros.
        /// For the upper planes, this is the minimal number of characters needed to represent
        /// the character (5 or 6).
        /// </summary>
        Typical,
        /// <summary>
        /// Pads all characters to a 6-character hex string padded with zeros. (e.g. 'A' would be "000041")
        /// </summary>
        PadToSix,
        /// <summary>
        /// Never does any padding of the resulting hex string (e.g. 'A' would be "41").
        /// </summary>
        Minimal
    }
    #endregion

    /// <summary>
    /// Set of extensions for handling <see cref="UChar"/>s
    /// </summary>
    public static class UCharExtensions
    {
        /// <summary>
        /// Returns this character as a hexadecimal string (e.g. 'A' would be "0041")
        /// </summary>
        public static string ToHexString(this char c, HexPadding padding = HexPadding.Typical)
        {
            return ToHexString((UChar)c, padding);
        }

        /// <summary>
        /// Returns this character as a hexadecimal string (e.g. 'A' would be "0041")
        /// </summary>
        public static string ToHexString(this UChar uc, HexPadding padding = HexPadding.Typical)
        {
            string formatString;
            switch (padding)
            {
                case HexPadding.PadToSix: formatString = "X6"; break;
                case HexPadding.Minimal: formatString = "X"; break;
                default: formatString = uc <= 0xFFFF ? "X4" : "X"; break;
            }

            return ((int)uc).ToString(formatString);
        }
    }
}
