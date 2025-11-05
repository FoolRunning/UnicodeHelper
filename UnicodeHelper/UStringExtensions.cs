using System;

namespace UnicodeHelper
{
    /// <summary>
    /// Provides extension methods for the <see cref="UString"/> class, enabling additional functionality
    /// such as converting indices and lengths between Unicode strings and .Net strings.
    /// </summary>
    public static class UStringExtensions
    {
        /// <summary>
        /// Used to convert an index in this UString to an index in the equivalent .Net string
        /// </summary>
        public static int GetDotNetStringIndex(this UString ustr, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative.");
            if (index > ustr.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than the length.");
            
            if (index == 0)
                return 0;
            
            if (index == ustr.Length)
                return ustr.CharLength;

            int dotNetStringIndex = 0;
            for (int i = 0; i < index; i++)
                dotNetStringIndex += ustr[i] <= 0xffff ? 1 : 2;

            return dotNetStringIndex;
        }
    }
}
