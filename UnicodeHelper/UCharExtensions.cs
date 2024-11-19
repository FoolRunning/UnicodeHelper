namespace UnicodeHelper
{
    public enum HexPadding
    {
        Typical,
        PadToEight,
        Minimal
    }

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
                case HexPadding.PadToEight: formatString = "X8"; break;
                case HexPadding.Minimal: formatString = "X"; break;
                default: formatString = uc <= 0xFFFF ? "X4" : "X8"; break;
            }

            return ((int)uc).ToString(formatString);
        }
    }
}
