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
    public readonly struct UCodepoint : IComparable<UCodepoint>, IEquatable<UCodepoint>
    {
        #region Data fields
        /// <summary>
        /// Represents the largest possible value for a <see cref="UCodepoint"/>.
        /// </summary>
        public static readonly UCodepoint MaxValue = new UCodepoint(UnicodeData.MaxUnicodeCodepoint);

        /// <summary>
        /// Represents the smallest possible value for a <see cref="UCodepoint"/>.
        /// </summary>
        public static readonly UCodepoint MinValue = new UCodepoint(0x0000);

        private readonly int _value;
        #endregion

        #region Construction
        /// <summary>
        /// Creates a <see cref="UCodepoint"/> from the specified hexadecimal string. The string may
        /// contain the hex specifier prefix ("0x").
        /// </summary>
        public static UCodepoint FromHexStr(string hexStr, bool allowLeadingTrailingWhitespace = true)
        {
            int codepoint = int.Parse(hexStr,
                allowLeadingTrailingWhitespace ? NumberStyles.HexNumber : NumberStyles.AllowHexSpecifier);
            return CreateChecked(codepoint);
        }

        /// <summary>
        /// Creates a <see cref="UCodepoint"/> from the specified string at the specified index.
        /// </summary>
        public static UCodepoint ReadFromStr(string str, int index)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UCodepoint(char.ConvertToUtf32(str, index));
        }

        /// <summary>
        /// Creates a <see cref="UCodepoint"/> from a valid high and low surrogate pair
        /// of .Net characters
        /// </summary>
        public static UCodepoint FromChars(char highSurrogate, char lowSurrogate)
        {
            // ConvertToUtf32 has checks, so no need to check resulting codepoint for valid values
            return new UCodepoint(char.ConvertToUtf32(highSurrogate, lowSurrogate));
        }

        /// <summary>
        /// Creates a <see cref="UCodepoint"/> from a single .Net character.
        /// Note that there is an implicit cast from <c>char</c> to <see cref="UCodepoint"/> so
        /// that should be used in most cases.
        /// </summary>
        public static UCodepoint FromChar(char c)
        {
            // A .Net character is always in the valid Unicode range, so no need to check the
            // resulting codepoint for valid values
            return new UCodepoint(c);
        }

        internal UCodepoint(char value)
        {
            _value = value;
        }

        internal UCodepoint(int value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UCodepoint CreateChecked(int codepoint)
        {
            if (codepoint < 0 || codepoint > UnicodeData.MaxUnicodeCodepoint)
                throw new ArgumentOutOfRangeException(nameof(codepoint), "Codepoint is outside the valid Unicode range");
            return new UCodepoint(codepoint);
        }
        #endregion

        #region Implementation of IComparable
        /// <summary>
        /// Compares this <see cref="UCodepoint"/> to another <see cref="UCodepoint"/>.
        /// </summary>
        public int CompareTo(UCodepoint other)
        {
            return _value - other._value;
        }
        #endregion

        #region Implementation of IEquatable
        /// <inheritdoc />
        public bool Equals(UCodepoint other)
        {
            return _value == other._value;
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Gets the Unicode category for the specified codepoint. If the specified codepoint
        /// is undefined in the Unicode Specification, then OtherNotAssigned (Cn) is returned
        /// per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// </summary>
        public static UnicodeCategory GetUnicodeCategory(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc);
        }

        /// <summary>
        /// Gets the Unicode bidirectional class for the specified codepoint. If the specified
        /// codepoint is undefined in the Unicode Specification, then the default bidirectional
        /// class for the codepoints range is returned per the Unicode specification for
        /// <see href="https://www.unicode.org/Public/UCD/latest/ucd/extracted/DerivedBidiClass.txt">DerivedBidiClass</see>
        /// </summary>
        public static UnicodeBidiClass GetBidiClass(UCodepoint uc)
        {
            return UnicodeData.GetBidiClass(uc);
        }

        // TODO: Should this be here for convenience or stay only in UnicodeProperties?
        //public static UnicodeProperty GetProperties(UCodepoint uc)
        //{
        //    return UnicodeProperties.GetProps(uc);
        //}

        /// <summary>
        /// Determines if the specified Unicode codepoint is a high or low surrogate.
        /// </summary>
        public static bool IsSurrogate(UCodepoint uc)
        {
            return uc >= 0xD800 && uc <= 0xDFFF;
        }

        /// <summary>
        /// Determines if the specified Unicode codepoint is a high surrogate.
        /// </summary>
        public static bool IsHighSurrogate(UCodepoint uc)
        {
            return uc >= 0xD800 && uc <= 0xDBFF;
        }

        /// <summary>
        /// Determines if the specified Unicode codepoint is a low surrogate.
        /// </summary>
        public static bool IsLowSurrogate(UCodepoint uc)
        {
            return uc >= 0xDC00 && uc <= 0xDFFF;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to the Unicode category Control (Cc).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsControl(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.Control;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be a letter. This includes UppercaseLetter (Lu), LowercaseLetter (Ll),
        /// TitlecaseLetter (Lt), ModifierLetter (Lm), and OtherLetter (Lo).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsLetter(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) <= UnicodeCategory.OtherLetter;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to the Unicode category UppercaseLetter (Lu)
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsUpper(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.UppercaseLetter;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to the Unicode category LowercaseLetter (Ll)
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsLower(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.LowercaseLetter;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to the Unicode category DecimalDigitNumber (Nd)
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsDigit(UCodepoint uc)
        {
            return UnicodeData.GetUnicodeCategory(uc) == UnicodeCategory.DecimalDigitNumber;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be a number. This includes DecimalDigitNumber (Nd), LetterNumber (Nl), and OtherNumber (No).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsNumber(UCodepoint uc)
        {
            UnicodeCategory cat = UnicodeData.GetUnicodeCategory(uc);
            return cat >= UnicodeCategory.DecimalDigitNumber && cat <= UnicodeCategory.OtherNumber;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be a letter or digit. This includes UppercaseLetter (Lu), LowercaseLetter (Ll),
        /// TitlecaseLetter (Lt), ModifierLetter (Lm), OtherLetter (Lo), and DecimalDigitNumber (Nd).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsLetterOrDigit(UCodepoint uc)
        {
            UnicodeCategory cat = UnicodeData.GetUnicodeCategory(uc);
            return cat <= UnicodeCategory.OtherLetter || cat == UnicodeCategory.DecimalDigitNumber;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be punctuation. This includes ConnectorPunctuation (Pc), DashPunctuation (Pd),
        /// OpenPunctuation (Ps), ClosePunctuation (Pe), InitialPunctuation (Pi), FinalPunctuation (Pf),
        /// and OtherPunctuation (Po).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsPunctuation(UCodepoint uc)
        {
            UnicodeCategory cat = UnicodeData.GetUnicodeCategory(uc);
            return cat >= UnicodeCategory.ConnectorPunctuation && cat <= UnicodeCategory.OtherPunctuation;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be a separator. This includes SpaceSeparator (Zs), LineSeparator (Zl),
        /// and ParagraphSeparator (Zp).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsSeparator(UCodepoint uc)
        {
            UnicodeCategory cat = UnicodeData.GetUnicodeCategory(uc);
            return cat >= UnicodeCategory.SpaceSeparator && cat <= UnicodeCategory.ParagraphSeparator;
        }

        /// <summary>
        /// Determines if the specified codepoint belongs to any Unicode category that is considered to
        /// be a symbol. This includes MathSymbol (Sm), CurrencySymbol (Sc),
        /// ModifierSymbol (Sk), OtherSymbol (So).
        /// </summary>
        /// <seealso cref="GetUnicodeCategory"/>
        public static bool IsSymbol(UCodepoint uc)
        {
            UnicodeCategory cat = UnicodeData.GetUnicodeCategory(uc);
            return cat >= UnicodeCategory.MathSymbol && cat <= UnicodeCategory.OtherSymbol;
        }

        /// <summary>
        /// Determines if the specified codepoint has the Unicode property of WhiteSpace (Ws).
        /// </summary>
        public static bool IsWhiteSpace(UCodepoint uc)
        {
            return (UnicodeProperties.GetProps(uc) & UnicodeProperty.WhiteSpace) != 0;
        }

        /// <summary>
        /// Determines if the specified codepoint has the Unicode property of Diacritic
        /// </summary>
        public static bool IsDiacritic(UCodepoint uc)
        {
            return (UnicodeProperties.GetProps(uc) & UnicodeProperty.Diacritic) != 0;
        }

        /// <summary>
        /// Converts the specified codepoint into its uppercase variant. If the Unicode specification
        /// does not specify an uppercase mapping for the specified codepoint, then that same codepoint
        /// is returned per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// </summary>
        public static UCodepoint ToUpper(UCodepoint uc)
        {
            return UnicodeData.ToUpper(uc);
        }

        /// <summary>
        /// Converts the specified codepoint into its lowercase variant. If the Unicode specification
        /// does not specify a lowercase mapping for the specified codepoint, then that same codepoint
        /// is returned per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// </summary>
        public static UCodepoint ToLower(UCodepoint uc)
        {
            return UnicodeData.ToLower(uc);
        }

        /// <summary>
        /// Converts the specified codepoint into its numeric equivalent.
        /// If the specified codepoint does not have a numeric value, then NaN is returned
        /// per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// <para>For example:</para>
        /// <para>'3' will return 3.0</para>
        /// <para>'\u0665' (ARABIC-INDIC DIGIT FIVE) will return 5.0</para>
        /// <para>'\u00BC' (VULGAR FRACTION ONE QUARTER) will return 0.25</para>
        /// </summary>
        public static double GetNumericValue(UCodepoint uc)
        {
            return UnicodeData.GetNumericValue(uc);
        }
        #endregion

        #region Overrides of Object
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is UCodepoint other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(_value);
        }

        /// <summary>
        /// Returns this codepoint as a string
        /// </summary>
        public override string ToString()
        {
            return _value <= 0xFFFF ? ((char)_value).ToString() : char.ConvertFromUtf32(_value);
        }
        #endregion

        #region Operator overrides
        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than an integer value
        /// </summary>
        public static bool operator <(UCodepoint uc, int v)
        {
            return uc._value < v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than or equal to an integer value
        /// </summary>
        public static bool operator <=(UCodepoint uc, int v)
        {
            return uc._value <= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than an integer value
        /// </summary>
        public static bool operator >(UCodepoint uc, int v)
        {
            return uc._value > v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than or equal to an integer value
        /// </summary>
        public static bool operator >=(UCodepoint uc, int v)
        {
            return uc._value >= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is equal to an integer value
        /// </summary>
        public static bool operator ==(UCodepoint uc, int v)
        {
            return uc._value == v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is not equal to an integer value
        /// </summary>
        public static bool operator !=(UCodepoint uc, int v)
        {
            return uc._value != v;
        }
        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than an unsigned short integer value
        /// </summary>
        public static bool operator <(UCodepoint uc, ushort v)
        {
            return uc._value < v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than or equal to an unsigned short integer value
        /// </summary>
        public static bool operator <=(UCodepoint uc, ushort v)
        {
            return uc._value <= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than an unsigned short integer value
        /// </summary>
        public static bool operator >(UCodepoint uc, ushort v)
        {
            return uc._value > v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than or equal to an unsigned short integer value
        /// </summary>
        public static bool operator >=(UCodepoint uc, ushort v)
        {
            return uc._value >= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is equal to an unsigned short integer value
        /// </summary>
        public static bool operator ==(UCodepoint uc, ushort v)
        {
            return uc._value == v;
        }
        
        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than a short integer value
        /// </summary>
        public static bool operator <(UCodepoint uc, short v)
        {
            return uc._value < v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than or equal to a short integer value
        /// </summary>
        public static bool operator <=(UCodepoint uc, short v)
        {
            return uc._value <= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than a short integer value
        /// </summary>
        public static bool operator >(UCodepoint uc, short v)
        {
            return uc._value > v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than or equal to a short integer value
        /// </summary>
        public static bool operator >=(UCodepoint uc, short v)
        {
            return uc._value >= v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is equal to a short integer value
        /// </summary>
        public static bool operator ==(UCodepoint uc, short v)
        {
            return uc._value == v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is not equal to a short integer value
        /// </summary>
        public static bool operator !=(UCodepoint uc, short v)
        {
            return uc._value != v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is not equal to an unsigned short value
        /// </summary>
        public static bool operator !=(UCodepoint uc, ushort v)
        {
            return uc._value != v;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than a .Net character value
        /// </summary>
        public static bool operator <(UCodepoint uc, char c)
        {
            return uc._value < c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than or equal to a .Net character value
        /// </summary>
        public static bool operator <=(UCodepoint uc, char c)
        {
            return uc._value <= c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than a .Net character value
        /// </summary>
        public static bool operator >(UCodepoint uc, char c)
        {
            return uc._value > c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than or equal to a .Net character value
        /// </summary>
        public static bool operator >=(UCodepoint uc, char c)
        {
            return uc._value >= c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is equal to a .Net character value
        /// </summary>
        public static bool operator ==(UCodepoint uc, char c)
        {
            return uc._value == c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is not equal to a .Net character value
        /// </summary>
        public static bool operator !=(UCodepoint uc, char c)
        {
            return uc._value != c;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator <(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value < uc2._value;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is less than or equal to another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator <=(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value <= uc2._value;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator >(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value > uc2._value;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is greater than or equal to another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator >=(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value >= uc2._value;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is equal to another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator ==(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value == uc2._value;
        }

        /// <summary>
        /// Determines if the a <see cref="UCodepoint"/> is not equal to another <see cref="UCodepoint"/> value
        /// </summary>
        public static bool operator !=(UCodepoint uc1, UCodepoint uc2)
        {
            return uc1._value != uc2._value;
        }

        /// <summary>
        /// Adds two <see cref="UCodepoint"/>s together
        /// </summary>
        public static UCodepoint operator +(UCodepoint uc1, UCodepoint uc2)
        {
            return CreateChecked(uc1._value + uc2._value);
        }

        /// <summary>
        /// Adds a <see cref="UCodepoint"/> and an integer together
        /// </summary>
        public static UCodepoint operator +(UCodepoint uc, int value)
        {
            return CreateChecked(uc._value + value);
        }

        /// <summary>
        /// Adds a <see cref="UCodepoint"/> and a .Net character together
        /// </summary>
        public static UCodepoint operator +(UCodepoint uc, char value)
        {
            return CreateChecked(uc._value + value);
        }

        /// <summary>
        /// Subtracts two <see cref="UCodepoint"/>s from each other
        /// </summary>
        public static UCodepoint operator -(UCodepoint uc1, UCodepoint uc2)
        {
            return CreateChecked(uc1._value - uc2._value);
        }

        /// <summary>
        /// Subtracts a <see cref="UCodepoint"/> and an integer from each other
        /// </summary>
        public static UCodepoint operator -(UCodepoint uc, int value)
        {
            return CreateChecked(uc._value - value);
        }

        /// <summary>
        /// Subtracts a <see cref="UCodepoint"/> and a .Net character from each other
        /// </summary>
        public static UCodepoint operator -(UCodepoint uc, char value)
        {
            return CreateChecked(uc._value - value);
        }

        /// <summary>
        /// Adds one to a <see cref="UCodepoint"/>
        /// </summary>
        public static UCodepoint operator ++(UCodepoint uc)
        {
            return CreateChecked(uc._value + 1);
        }

        /// <summary>
        /// Subtracts one from a <see cref="UCodepoint"/>
        /// </summary>
        public static UCodepoint operator --(UCodepoint uc)
        {
            return CreateChecked(uc._value - 1);
        }

        /// <summary>
        /// Converts a .Net character to a <see cref="UCodepoint"/>
        /// </summary>
        public static implicit operator UCodepoint(char c)
        {
            return new UCodepoint(c);
        }

        /// <summary>
        /// Converts a <see cref="UCodepoint"/> to a .Net character
        /// </summary>
        public static explicit operator char(UCodepoint uc)
        {
            return (char)uc._value;
        }

        /// <summary>
        /// Converts a <see cref="UCodepoint"/> to an integer
        /// </summary>
        public static explicit operator int(UCodepoint uc)
        {
            return uc._value;
        }

        /// <summary>
        /// Converts an integer to a <see cref="UCodepoint"/>
        /// </summary>
        public static explicit operator UCodepoint(int value)
        {
            return CreateChecked(value);
        }
        #endregion
    }
}

