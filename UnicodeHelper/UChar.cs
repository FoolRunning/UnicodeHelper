using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Represents an entire Unicode codepoint (32-bit value).
    /// </summary>
    [PublicAPI]
    public readonly struct UChar : IComparable<UChar>, IEquatable<UChar>
    {
        /// <summary>
        /// Represents the largest possible value for a <see cref="UChar"/>.
        /// </summary>
        public static readonly UChar MaxValue = new UChar(UnicodeData.MaxUnicodeCodepoint);

        /// <summary>
        /// Represents the smallest possible value for a <see cref="UChar"/>.
        /// </summary>
        public static readonly UChar MinValue = new UChar(0x0000);

        #region Member variables
        private readonly int _codepoint;
        #endregion

        #region Construction
        /// <summary>
        /// Creates a <see cref="UChar"/> from the specified hexadecimal string. The string may
        /// contain the hex specifier prefix ("0x").
        /// </summary>
        public static UChar FromHexStr(string hexStr)
        {
            return CreateChecked(int.Parse(hexStr, NumberStyles.AllowHexSpecifier | NumberStyles.HexNumber));
        }

        /// <summary>
        /// Creates a <see cref="UChar"/> from the specified string at the specified index.
        /// </summary>
        public static UChar ReadFromStr(string str, int index)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UChar(char.ConvertToUtf32(str, index));
        }

        /// <summary>
        /// Creates a <see cref="UChar"/> from a valid high and low surrogate pair of chars
        /// </summary>
        public static UChar FromChars(char highSurrogate, char lowSurrogate)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UChar(char.ConvertToUtf32(highSurrogate, lowSurrogate));
        }

        /// <summary>
        /// Creates a <see cref="UChar"/> from a single char.
        /// Note that there is an implicit cast from char to <see cref="UChar"/> so that should
        /// be used in most cases.
        /// </summary>
        public static UChar FromChar(char c)
        {
            // char is always in valid Unicode range, so no need to check resulting codepoint for valid values
            return new UChar(c);
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
        /// <summary>
        /// Compares this <see cref="UChar"/> to another <see cref="UChar"/>.
        /// </summary>
        public int CompareTo(UChar other)
        {
            return _codepoint - other._codepoint;
        }
        #endregion

        #region Implementation of IEquatable
        /// <inheritdoc />
        public bool Equals(UChar other)
        {
            return _codepoint == other._codepoint;
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Gets the Unicode category for the specified character. If the specified character
        /// does not have any significance to the Unicode Specification (i.e. it's undefined),
        /// then OtherNotAssigned (Cn) is returned per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// </summary>
        public static UnicodeCategory GetUnicodeCategory(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc);
        }

        /// <summary>
        /// Gets the Unicode bidirectional class for the specified character
        /// </summary>
        public static UnicodeBidiClass GetBidiClass(UChar uc)
        {
            return UnicodeData.GetBidiClass(uc);
        }

        /// <summary>
        /// Determines if the specified character belongs to the Unicode category Control (Cc).
        /// </summary>
        public static bool IsControl(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.Control;
        }

        /// <summary>
        /// Determines if the specified character belongs to any Unicode category that is considered to
        /// be a letter. This includes UppercaseLetter (Lu), LowercaseLetter (Ll),
        /// TitlecaseLetter (Lt), ModifierLetter (Lm), and OtherLetter (Lo).
        /// </summary>
        public static bool IsLetter(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) <= UnicodeCategory.OtherLetter;
        }

        /// <summary>
        /// Determines if the specified character belongs to the Unicode category UppercaseLetter (Lu)
        /// </summary>
        public static bool IsUpper(UChar uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.UppercaseLetter;
        }

        /// <summary>
        /// Determines if the specified character belongs to the Unicode category LowercaseLetter (Ll)
        /// </summary>
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

        /// <summary>
        /// Determines if the specified character belongs to any Unicode category that is considered to
        /// be punctuation. This includes ConnectorPunctuation (Pc), DashPunctuation (Pd),
        /// OpenPunctuation (Ps), ClosePunctuation (Pe), InitialPunctuation (Pi), FinalPunctuation (Pf),
        /// and OtherPunctuation (Po).
        /// </summary>
        public static bool IsPunctuation(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if the specified character belongs to any Unicode category that is considered to
        /// be a separator. This includes SpaceSeparator (Zs), LineSeparator (Zl),
        /// and ParagraphSeparator (Zp).
        /// </summary>
        public static bool IsSeparator(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if the specified character belongs to any Unicode category that is considered to
        /// be a symbol. This includes MathSymbol (Sm), CurrencySymbol (Sc),
        /// ModifierSymbol (Sk), OtherSymbol (So).
        /// </summary>
        public static bool IsSymbol(UChar uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if the specified character has the Unicode property of WhiteSpace (Ws).
        /// </summary>
        public static bool IsWhiteSpace(UChar uc)
        {
            // TODO: Write tests for this method
            //return UnicodeProperties.GetProps(uc).HasFlags(UnicodeProperty.WhiteSpace);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the specified character into its uppercase variant. If the Unicode specification
        /// does not specify an uppercase mapping for the specified character, then that same character
        /// is returned.
        /// </summary>
        public static UChar ToUpper(UChar uc)
        {
            return UnicodeData.ToUpper(uc);
        }

        /// <summary>
        /// Converts the specified character into its lowercase variant. If the Unicode specification
        /// does not specify a lowercase mapping for the specified character, then that same character
        /// is returned.
        /// </summary>
        public static UChar ToLower(UChar uc)
        {
            return UnicodeData.ToLower(uc);
        }

        /// <summary>
        /// Converts the specified character into its numeric equivalent.
        /// If the specified character does not have a numeric value, then NaN is returned
        /// per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// <para>For example:</para>
        /// <para>'3' will return 3.0</para>
        /// <para>'\u0665' (ARABIC-INDIC DIGIT FIVE) will return 5.0</para>
        /// <para>'\u00BC' (VULGAR FRACTION ONE QUARTER) will return 0.25</para>
        /// </summary>
        public static double GetNumericValue(UChar uc)
        {
            return UnicodeData.GetNumericValue(uc);
        }
        #endregion

        #region Overrides of Object
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is UChar other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return System.HashCode.Combine(_codepoint);
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
        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than an integer value
        /// </summary>
        public static bool operator <(UChar uc, int v)
        {
            return uc._codepoint < v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than or equal to an integer value
        /// </summary>
        public static bool operator <=(UChar uc, int v)
        {
            return uc._codepoint <= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than an integer value
        /// </summary>
        public static bool operator >(UChar uc, int v)
        {
            return uc._codepoint > v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than or equal to an integer value
        /// </summary>
        public static bool operator >=(UChar uc, int v)
        {
            return uc._codepoint >= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is equal to an integer value
        /// </summary>
        public static bool operator ==(UChar uc, int v)
        {
            return uc._codepoint == v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is not equal to an integer value
        /// </summary>
        public static bool operator !=(UChar uc, int v)
        {
            return uc._codepoint != v;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than a char value
        /// </summary>
        public static bool operator <(UChar uc, char c)
        {
            return uc._codepoint < c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than or equal to a char value
        /// </summary>
        public static bool operator <=(UChar uc, char c)
        {
            return uc._codepoint <= c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than a char value
        /// </summary>
        public static bool operator >(UChar uc, char c)
        {
            return uc._codepoint > c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than or equal to a char value
        /// </summary>
        public static bool operator >=(UChar uc, char c)
        {
            return uc._codepoint >= c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is equal to a char value
        /// </summary>
        public static bool operator ==(UChar uc, char c)
        {
            return uc._codepoint == c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is not equal to a char value
        /// </summary>
        public static bool operator !=(UChar uc, char c)
        {
            return uc._codepoint != c;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than another <see cref="UChar"/> value
        /// </summary>
        public static bool operator <(UChar uc1, UChar uc2)
        {
            return uc1._codepoint < uc2._codepoint;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is less than or equal to another <see cref="UChar"/> value
        /// </summary>
        public static bool operator <=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint <= uc2._codepoint;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than another <see cref="UChar"/> value
        /// </summary>
        public static bool operator >(UChar uc1, UChar uc2)
        {
            return uc1._codepoint > uc2._codepoint;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is greater than or equal to another <see cref="UChar"/> value
        /// </summary>
        public static bool operator >=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint >= uc2._codepoint;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is equal to another <see cref="UChar"/> value
        /// </summary>
        public static bool operator ==(UChar uc1, UChar uc2)
        {
            return uc1._codepoint == uc2._codepoint;
        }

        /// <summary>
        /// Determines if the a <see cref="UChar"/> is not equal to another <see cref="UChar"/> value
        /// </summary>
        public static bool operator !=(UChar uc1, UChar uc2)
        {
            return uc1._codepoint != uc2._codepoint;
        }

        /// <summary>
        /// Adds two <see cref="UChar"/>s together
        /// </summary>
        public static UChar operator +(UChar uc1, UChar uc2)
        {
            return CreateChecked(uc1._codepoint + uc2._codepoint);
        }

        /// <summary>
        /// Adds a <see cref="UChar"/> and an integer together
        /// </summary>
        public static UChar operator +(UChar uc, int value)
        {
            return CreateChecked(uc._codepoint + value);
        }

        /// <summary>
        /// Adds a <see cref="UChar"/> and a char together
        /// </summary>
        public static UChar operator +(UChar uc, char value)
        {
            return CreateChecked(uc._codepoint + value);
        }

        /// <summary>
        /// Subtracts two <see cref="UChar"/>s from each other
        /// </summary>
        public static UChar operator -(UChar uc1, UChar uc2)
        {
            return CreateChecked(uc1._codepoint - uc2._codepoint);
        }

        /// <summary>
        /// Subtracts a <see cref="UChar"/> and an integer from each other
        /// </summary>
        public static UChar operator -(UChar uc, int value)
        {
            return CreateChecked(uc._codepoint - value);
        }

        /// <summary>
        /// Subtracts a <see cref="UChar"/> and a char from each other
        /// </summary>
        public static UChar operator -(UChar uc, char value)
        {
            return CreateChecked(uc._codepoint - value);
        }

        /// <summary>
        /// Adds one to a <see cref="UChar"/>
        /// </summary>
        public static UChar operator ++(UChar uc)
        {
            return CreateChecked(uc._codepoint + 1);
        }

        /// <summary>
        /// Subtracts one from a <see cref="UChar"/>
        /// </summary>
        public static UChar operator --(UChar uc)
        {
            return CreateChecked(uc._codepoint - 1);
        }

        /// <summary>
        /// Converts a char to a <see cref="UChar"/>
        /// </summary>
        public static implicit operator UChar(char c)
        {
            return new UChar(c);
        }

        /// <summary>
        /// Converts a <see cref="UChar"/> to a char
        /// </summary>
        public static explicit operator char(UChar uc)
        {
            return (char)uc._codepoint;
        }

        /// <summary>
        /// Converts a <see cref="UChar"/> to an integer
        /// </summary>
        public static explicit operator int(UChar uc)
        {
            return uc._codepoint;
        }

        /// <summary>
        /// Converts an integer to a <see cref="UChar"/>
        /// </summary>
        public static explicit operator UChar(int value)
        {
            return CreateChecked(value);
        }
        #endregion
    }
}

