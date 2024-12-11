using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Represents a sequence of Unicode codepoints
    /// </summary>
    [PublicAPI]
    public sealed class UString : 
        IEquatable<UString>, 
        IEnumerable<UCodepoint>,
        ICloneable,
        IComparable,
        IComparable<UString>
    {
        #region Data fields
        private readonly UCodepoint[] _codepoints;
        /// <summary>Index of the codepoint in the array where this string starts (for a substring)</summary>
        private readonly int _startIndex;

        private int _cachedHash;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="UString"/> from the specified codepoints
        /// </summary>
        public UString(IReadOnlyList<UCodepoint> codepoints) : this(codepoints, 0, codepoints.Count)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UString"/> from the specified codepoints starting from the
        /// specified starting index and taking the specified number of codepoints
        /// </summary>
        public UString(IReadOnlyList<UCodepoint> codepoints, int startIndex, int count)
        {
            // TODO: Write tests for this constructor
            if (codepoints == null)
                throw new ArgumentNullException(nameof(codepoints));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than zero");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count is less than zero");
            if (startIndex + count > codepoints.Count)
                throw new ArgumentException("startIndex and count must reside in the list");

            // TODO: Since we're making a new array anyway, make an array of the specified subset
            _codepoints = codepoints.ToArray();
            _startIndex = startIndex;
            Length = count;
        }

        /// <summary>
        /// Creates a new <see cref="UString"/> from the specified .Net string
        /// </summary>
        public UString(string dotNetString) : this(dotNetString, 0, dotNetString.Length)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UString"/> from the specified substring of a .Net string
        /// </summary>
        public UString(string dotNetString, int startIndex, int count)
        {
            if (dotNetString == null)
                throw new ArgumentNullException(nameof(dotNetString));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than zero");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count is less than zero");
            if (startIndex + count > dotNetString.Length)
                throw new ArgumentException("startIndex and count must reside in the string");

            UCodepoint[] codepoints = new UCodepoint[count];
            int index = 0;
            int end = startIndex + count;
            for (int i = startIndex; i < end; i++)
            {
                UCodepoint uc = UCodepoint.ReadFromStr(dotNetString, i);
                codepoints[index++] = uc;
                if (uc > 0xFFFF)
                    i++; // Step over low surrogate
            }

            _codepoints = codepoints;
            _startIndex = 0;
            Length = index;
        }

        internal UString(int start, int length, UCodepoint[] codepoints)
        {
            _codepoints = codepoints;
            _startIndex = start;
            Length = length;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of Unicode codepoints that make up this Unicode string
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the Unicode codepoint at the specified index in this Unicode string
        /// </summary>
        public UCodepoint this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be greater than or equal to zero");
                if (index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be less than the length of the string");

                return _codepoints[_startIndex + index];
            }
        }
        #endregion

        #region Implementation of IEquatable
        /// <inheritdoc />
        public bool Equals(UString other)
        {
            if (ReferenceEquals(other, null) || other.Length != Length)
                return false;

            if (ReferenceEquals(other, this))
                return true;

            int end = _startIndex + Length;
            int otherIndex = other._startIndex;
            for (int i = _startIndex; i < end; i++)
            {
                if (_codepoints[i] != other._codepoints[otherIndex++])
                    return false;
            }

            return true;
        }
        #endregion

        #region Implementation of IClonable
        /// <inheritdoc />
        public object Clone()
        {
            // TODO: Write tests for this method
            // Since this class is immutable, just reuse the same underlying data.
            return new UString(_startIndex, Length, _codepoints);
        }
        #endregion
        
        #region Implementation of IEnumerable
        /// <inheritdoc />
        public IEnumerator<UCodepoint> GetEnumerator()
        {
            return new UCodepointEnumerator(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        
        #region Implementation of IComparable
        /// <summary>
        /// Compares this <see cref="UString"/> to another <see cref="UString"/>.
        /// </summary>
        public int CompareTo(UString other)
        {
            // TODO: Write tests for this method
            int minLength = Math.Min(Length, other.Length);
            int end = _startIndex + minLength;
            int otherIndex = other._startIndex;
            for (int i = _startIndex; i < end; i++)
            {
                int cmp = _codepoints[i].CompareTo(other._codepoints[otherIndex++]);
                if (cmp != 0)
                    return cmp;
            }

            return Length - other.Length;
        }

        /// <summary>
        /// Compares this <see cref="UString"/> to another <see cref="UString"/>.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (!(obj is UString other))
                throw new ArgumentException("obj is not a UString instance");
            return CompareTo(other);
        }
        #endregion

        #region Other public methods

        public static UString Concat(UCodepoint uc, UString us)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UString Concat(UString us, UCodepoint uc)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UString Concat(UString us1, UString us2)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UString Concat(UString us1, UString us2, UString us3)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UString Concat(UString us1, UString us2, UString us3, UString us4)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public static UString Concat(params UString[] uStrings)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the zero-based index of the first occurrence of the specified Unicode codepoint in this string.
        /// </summary>
        public int IndexOf(UCodepoint value)
        {
            // TODO: Write tests for this method
            return IndexOf(value, 0, Length);
        }

        /// <summary>
        /// Finds the zero-based index of the first occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position.
        /// </summary>
        public int IndexOf(UCodepoint value, int startIndex)
        {
            // TODO: Write tests for this method
            return IndexOf(value, startIndex, Length - startIndex);
        }

        /// <summary>
        /// Finds the zero-based index of the first occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position and examines a specified number of codepoint positions.
        /// </summary>
        public int IndexOf(UCodepoint value, int startIndex, int count)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than zero");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count is less than zero");
            if (startIndex + count > Length)
                throw new ArgumentException("StartIndex and count must reside in the string");

            int start = _startIndex + startIndex;
            int end = start + count;
            for (int i = start; i < end; i++)
            {
                if (_codepoints[i] == value)
                    return i - _startIndex;
            }

            return -1;
        }

        /// <summary>
        /// Finds the zero-based index of the last occurrence of the specified Unicode codepoint in this string.
        /// </summary>
        public int LastIndexOf(UCodepoint value)
        {
            // TODO: Write tests for this method
            return LastIndexOf(value, Length - 1, Length);
        }

        /// <summary>
        /// Finds the zero-based index of the last occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position and proceeds backward toward the beginning of the
        /// string.
        /// </summary>
        public int LastIndexOf(UCodepoint value, int startIndex)
        {
            // TODO: Write tests for this method
            return LastIndexOf(value, startIndex, startIndex + 1);
        }

        /// <summary>
        /// Finds the zero-based index of the last occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position and proceeds backward toward the beginning of the
        /// string for a specified number of codepoint positions.
        /// </summary>
        public int LastIndexOf(UCodepoint value, int startIndex, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count is less than zero");
            if (startIndex >= Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex must be less than the length of the string");
            if (startIndex - count + 1 < 0)
                throw new ArgumentException("StartIndex and count must reside in the string");

            int start = _startIndex + startIndex;
            int end = start - count;
            for (int i = start; i > end; i--)
            {
                if (_codepoints[i] == value)
                    return i - _startIndex;
            }

            return -1;
        }

        public int IndexOf(UString value)
        {
            // TODO: Write tests for this method
            return IndexOf(value, 0, Length);
        }

        public int IndexOf(UString value, int startIndex)
        {
            // TODO: Write tests for this method
            return IndexOf(value, startIndex, Length - startIndex);
        }

        public int IndexOf(UString value, int startIndex, int count)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public int LastIndexOf(UString value)
        {
            return LastIndexOf(value, 0, Length);
        }

        public int LastIndexOf(UString value, int startIndex)
        {
            return LastIndexOf(value, startIndex, Length - startIndex);
        }

        public int LastIndexOf(UString value, int startIndex, int count)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public bool StartsWith(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public bool EndsWith(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public bool Contains(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public IEnumerable<UString> Split(params UCodepoint[] separators)
        {
            return Split(separators, int.MaxValue);
        }

        public IEnumerable<UString> Split(UCodepoint[] separators, int maxCount, 
            StringSplitOptions options = StringSplitOptions.None)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public UString ToUpperInvariant()
        {
            // TODO: Handle complex uppercasing rules
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = UnicodeData.ToUpper(_codepoints[i]);
            return new UString(0, result.Length, result);
        }

        public UString ToLowerInvariant()
        {
            // TODO: Handle complex lowercasing rules
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = UnicodeData.ToLower(_codepoints[i]);
            return new UString(0, result.Length, result);
        }

        public UCodepoint[] ToCodepointArray()
        {
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = _codepoints[i];
            return result;
        }

        public UString SubString(int start)
        {
            return SubString(start, Length - start);
        }

        public UString SubString(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "startIndex is less than zero");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is less than zero");
            if (start + length > Length)
                throw new ArgumentException("Start and length must reside in the string");

            return new UString(_startIndex + start, length, _codepoints);
        }

        public UString Normalize()
        {
            return Normalize(NormalizationForm.FormC);
        }

        public UString Normalize(NormalizationForm form)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }
        #endregion

        #region Overrides of Object
        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            // TODO: Write tests for this method
            if (_cachedHash != 0)
                return _cachedHash;
            
            HashCode hc = new HashCode();
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                hc.Add(_codepoints[i]);

            return _cachedHash = hc.ToHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is UString other && Equals(other));
        }

        /// <summary>
        /// Converts this Unicode string into a standard .Net string
        /// </summary>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder(Length);
        
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
            {
                UCodepoint uc = _codepoints[i];
                if (uc <= 0xFFFF)
                    result.Append((char)uc);
                else
                    result.Append(uc.ToString());
            }

            return result.ToString();
        }
        #endregion

        #region Operator overrides
        /// <summary>
        /// Determines if this Unicode string is equal to another Unicode string
        /// </summary>
        public static bool operator ==(UString us1, UString us2)
        {
            return Equals(us1, us2);
        }

        /// <summary>
        /// Determines if this Unicode string is not equal to another Unicode string
        /// </summary>
        public static bool operator !=(UString us1, UString us2)
        {
            return !Equals(us1, us2);
        }

        /// <summary>
        /// Combines two Unicode strings together
        /// </summary>
        public static UString operator +(UString us1, UString us2)
        {
            return Concat(us1, us2);
        }

        /// <summary>
        /// Combines a Unicode string and Unicode codepoint together
        /// </summary>
        public static UString operator +(UString us, UCodepoint uc)
        {
            return Concat(us, uc);
        }

        /// <summary>
        /// Combines a Unicode codepoint and Unicode string together
        /// </summary>
        public static UString operator +(UCodepoint uc, UString us)
        {
            return Concat(uc, us);
        }
        #endregion

        #region UCodepointEnumerator class
        private sealed class UCodepointEnumerator : IEnumerator<UCodepoint>
        {
            private UString _str;
            private int _index = -1;
            private UCodepoint _current;

            public UCodepointEnumerator(UString str)
            {
                _str = str;
            }

            public void Dispose()
            {
                if (_str != null)
                    _index = _str.Length;
                _str = null;
            }

            public UCodepoint Current
            {
                get
                {
                    if (_index == -1)
                        throw new InvalidOperationException("Enumerator not started");
                    if (_index >= _str.Length)
                        throw new InvalidOperationException("Enumerator has completed");
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                if (_index >= _str.Length)
                    return false;

                _current = _str._codepoints[_str._startIndex + _index];
                return true;
            }

            public void Reset()
            {
                _index = -1;
                _current = UCodepoint.MinValue;
            }
        }
        #endregion
    }
}
