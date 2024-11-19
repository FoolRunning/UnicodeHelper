using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Represents an entire Unicode codepoint (32-bit value)
    /// </summary>
    [PublicAPI]
    public readonly struct UChar : IComparable<UChar>, IEquatable<UChar>
    {
        public static readonly UChar MaxValue = new UChar(UnicodeData.MaxUnicodeCodepoint);

        public static readonly UChar MinValue = new UChar(0x0000);

        #region Member variables
        private readonly int _codepoint;
        #endregion

        #region Construction
        public static UChar FromHexStr(string hexStr)
        {
            return CreateChecked(int.Parse(hexStr, NumberStyles.AllowHexSpecifier | NumberStyles.HexNumber));
        }

        public static UChar ReadFromStr(string str, int index)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UChar(char.ConvertToUtf32(str, index));
        }

        public static UChar FromChars(char highSurrogate, char lowSurrogate)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UChar(char.ConvertToUtf32(highSurrogate, lowSurrogate));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UChar CreateChecked(int codepoint)
        {
            if (codepoint < 0 || codepoint > UnicodeData.MaxUnicodeCodepoint)
                throw new ArgumentOutOfRangeException(nameof(codepoint), "Codepoint is outside the valid Unicode range");
            return new UChar(codepoint);
        }

        private UChar(int codepoint)
        {
            _codepoint = codepoint;
        }
        #endregion

        #region Implementation of IComparable
        public int CompareTo(UChar other)
        {
            return _codepoint - other._codepoint;
        }
        #endregion

        #region Implementation of IEquatable
        public bool Equals(UChar other)
        {
            return _codepoint == other._codepoint;
        }
        #endregion

        #region Public static methods
        public static UnicodeCategory GetUnicodeCategory(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc);
        }

        public static bool IsControl(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.Control;
        }

        public static bool IsLetter(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) <= UnicodeCategory.OtherLetter;
        }

        public static bool IsUpper(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.UppercaseLetter;
        }

        public static bool IsLower(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.LowercaseLetter;
        }

        public static bool IsDigit(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsNumber(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsLetterOrDigit(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsPunctuation(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsSeparator(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsSymbol(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static bool IsWhiteSpace(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UChar ToUpper(UChar uc)
        {
            return UnicodeData.ToUpper(uc);
        }

        public static UChar ToLower(UChar uc)
        {
            return UnicodeData.ToLower(uc);
        }

        public static double GetNumericValue(UChar uc)
        {
            return UnicodeData.GetNumericValue(uc);
        }
        #endregion

        #region Overrides of Object
        public override bool Equals(object obj)
        {
            return obj is UChar other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_codepoint);
        }

        /// <summary>
        /// Returns this character as a string
        /// </summary>
        public override string ToString()
        {
            return char.ConvertFromUtf32(_codepoint);
        }
        #endregion

        #region Operator overrides
        public static bool operator <(UChar uc, int v)
        {
            return uc._codepoint < v;
        }

        public static bool operator <=(UChar uc, int v)
        {
            return uc._codepoint <= v;
        }

        public static bool operator >(UChar uc, int v)
        {
            return uc._codepoint > v;
        }

        public static bool operator >=(UChar uc, int v)
        {
            return uc._codepoint >= v;
        }

        public static bool operator ==(UChar uc, int v)
        {
            return uc._codepoint == v;
        }

        public static bool operator !=(UChar uc, int v)
        {
            return uc._codepoint != v;
        }

        public static bool operator <(UChar uc, char c)
        {
            return uc._codepoint < c;
        }

        public static bool operator <=(UChar uc, char c)
        {
            return uc._codepoint <= c;
        }

        public static bool operator >(UChar uc, char c)
        {
            return uc._codepoint > c;
        }

        public static bool operator >=(UChar uc, char c)
        {
            return uc._codepoint >= c;
        }

        public static bool operator ==(UChar uc, char c)
        {
            return uc._codepoint == c;
        }

        public static bool operator !=(UChar uc, char c)
        {
            return uc._codepoint != c;
        }

        public static bool operator <(UChar uc1, UChar uc2)
        {
            return uc1._codepoint < uc2._codepoint;
        }

        public static bool operator <=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint <= uc2._codepoint;
        }

        public static bool operator >(UChar uc1, UChar uc2)
        {
            return uc1._codepoint > uc2._codepoint;
        }

        public static bool operator >=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint >= uc2._codepoint;
        }

        public static bool operator ==(UChar uc1, UChar uc2)
        {
            return uc1._codepoint == uc2._codepoint;
        }

        public static bool operator !=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint != uc2._codepoint;
        }

        public static UChar operator +(UChar uc1, UChar uc2)
        {
            return CreateChecked(uc1._codepoint + uc2._codepoint);
        }

        public static UChar operator +(UChar uc, int value)
        {
            return CreateChecked(uc._codepoint + value);
        }

        public static UChar operator -(UChar uc1, UChar uc2)
        {
            return CreateChecked(uc1._codepoint - uc2._codepoint);
        }

        public static UChar operator -(UChar uc, int value)
        {
            return CreateChecked(uc._codepoint - value);
        }

        public static UChar operator ++(UChar uc)
        {
            return CreateChecked(uc._codepoint + 1);
        }

        public static UChar operator --(UChar uc)
        {
            return CreateChecked(uc._codepoint - 1);
        }

        public static implicit operator UChar(char c)
        {
            return new UChar(c);
        }

        public static explicit operator char(UChar uc)
        {
            return (char)uc._codepoint;
        }

        public static explicit operator int(UChar uc)
        {
            return uc._codepoint;
        }

        public static explicit operator UChar(int value)
        {
            return CreateChecked(value);
        }
        #endregion
    }
}

