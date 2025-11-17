using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnicodeHelper.Internal;

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
        /// <summary>
        /// Represents an empty, zero-length Unicode string
        /// </summary>
        public static readonly UString Empty = new UString(0, 0, Array.Empty<UCodepoint>());

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

            _codepoints = new UCodepoint[count];
            int end = startIndex + count;
            int index = 0;
            for (int i = startIndex; i < end; i++)
                _codepoints[index++] = codepoints[i];
            
            _startIndex = 0;
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

        internal UString(UCodepoint uc)
        {
            _codepoints = new [] { uc };
            _startIndex = 0;
            Length = 1;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of Unicode codepoints that make up this Unicode string
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the number of .Net characters that make up this Unicode string
        /// </summary>
        public int CharLength
        {
            get
            {
                int charCount = 0;
                int end = _startIndex + Length;
                for (int i = _startIndex; i < end; i++)
                    charCount += _codepoints[i] > 0xFFFF ? 2 : 1;
                return charCount;
            }
        }

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
            return new UStringEnumerator(this);
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

        #region Public static methods
        /// <summary>
        /// Gets whether the specified Unicode string is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(UString us)
        {
            return us == null || us.Length == 0;
        }

        /// <summary>
        /// Gets whether the specified Unicode string is null, empty, or contains only whitespace
        /// </summary>
        public static bool IsNullOrWhitespace(UString us)
        {
            if (IsNullOrEmpty(us))
                return true;

            int start = us._startIndex;
            int end = start + us.Length;
            for (int i = start; i < end; i++)
            {
                if (!UCodepoint.IsWhiteSpace(us._codepoints[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified Unicode string and Unicode codepoint together
        /// </summary>
        public static UString Concat(UString us, UCodepoint uc)
        {
            if (us == null)
                us = Empty;

            UCodepoint[] codepoints = new UCodepoint[us.Length + 1];
            Array.Copy(us._codepoints, us._startIndex, codepoints, 0, us.Length);
            codepoints[codepoints.Length - 1] = uc;
            return new UString(0, codepoints.Length, codepoints);
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified Unicode codepoint and Unicode string together
        /// </summary>
        public static UString Concat(UCodepoint uc, UString us)
        {
            if (us == null)
                us = Empty;

            UCodepoint[] codepoints = new UCodepoint[us.Length + 1];
            codepoints[0] = uc;
            Array.Copy(us._codepoints, us._startIndex, codepoints, 1, us.Length);
            return new UString(0, codepoints.Length, codepoints);
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified two Unicode strings together
        /// </summary>
        public static UString Concat(UString us1, UString us2)
        {
            if (us1 == null)
                us1 = Empty;

            if (us2 == null)
                us2 = Empty;

            if (us1.Length == 0 && us2.Length == 0)
                return Empty;

            UCodepoint[] codepoints = new UCodepoint[us1.Length + us2.Length];
            Array.Copy(us1._codepoints, us1._startIndex, codepoints, 0, us1.Length);
            Array.Copy(us2._codepoints, us2._startIndex, codepoints, us1.Length, us2.Length);
            return new UString(0, codepoints.Length, codepoints);
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified three Unicode strings together
        /// </summary>
        public static UString Concat(UString us1, UString us2, UString us3)
        {
            if (us1 == null)
                us1 = Empty;

            if (us2 == null)
                us2 = Empty;

            if (us3 == null)
                us3 = Empty;

            if (us1.Length == 0 && us2.Length == 0 && us3.Length == 0)
                return Empty;

            UCodepoint[] codepoints = new UCodepoint[us1.Length + us2.Length + us3.Length];
            Array.Copy(us1._codepoints, us1._startIndex, codepoints, 0, us1.Length);
            Array.Copy(us2._codepoints, us2._startIndex, codepoints, us1.Length, us2.Length);
            Array.Copy(us3._codepoints, us3._startIndex, codepoints, us1.Length + us2.Length, us3.Length);
            return new UString(0, codepoints.Length, codepoints);
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified four Unicode strings together
        /// </summary>
        public static UString Concat(UString us1, UString us2, UString us3, UString us4)
        {
            if (us1 == null)
                us1 = Empty;

            if (us2 == null)
                us2 = Empty;

            if (us3 == null)
                us3 = Empty;

            if (us4 == null)
                us4 = Empty;

            if (us1.Length == 0 && us2.Length == 0 && us3.Length == 0 && us4.Length == 0)
                return Empty;

            UCodepoint[] codepoints = new UCodepoint[us1.Length + us2.Length + us3.Length + us4.Length];
            Array.Copy(us1._codepoints, us1._startIndex, codepoints, 0, us1.Length);
            Array.Copy(us2._codepoints, us2._startIndex, codepoints, us1.Length, us2.Length);
            Array.Copy(us3._codepoints, us3._startIndex, codepoints, us1.Length + us2.Length, us3.Length);
            Array.Copy(us4._codepoints, us4._startIndex, codepoints, us1.Length + us2.Length + us3.Length, us4.Length);
            return new UString(0, codepoints.Length, codepoints);
        }

        /// <summary>
        /// Creates a new Unicode string by concatenating the specified Unicode strings together
        /// </summary>
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        public static UString Concat(params UString[] uStrings)
        {
            if (uStrings == null)
                throw new ArgumentNullException(nameof(uStrings));

            int totalLength = 0;
            for (int i = 0; i < uStrings.Length; i++)
            {
                UString us = uStrings[i];
                if (us != null)
                    totalLength += us.Length;
            }
                
            UCodepoint[] codepoints = new UCodepoint[totalLength];
            int startIndex = 0;
            for (int i = 0; i < uStrings.Length; i++)
            {
                UString us = uStrings[i];
                if (IsNullOrEmpty(us)) 
                    continue;
                
                Array.Copy(us._codepoints, us._startIndex, codepoints, startIndex, us.Length);
                startIndex += us.Length;
            }
            return new UString(0, codepoints.Length, codepoints);
        }
        #endregion

        #region Other public methods
        /// <summary>
        /// Finds the zero-based index of the first occurrence of the specified Unicode codepoint in this string.
        /// </summary>
        public int IndexOf(UCodepoint value)
        {
            return IndexOf(value, 0, Length);
        }

        /// <summary>
        /// Finds the zero-based index of the first occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position.
        /// </summary>
        public int IndexOf(UCodepoint value, int startIndex)
        {
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
            return LastIndexOf(value, Length - 1, Length);
        }

        /// <summary>
        /// Finds the zero-based index of the last occurrence of the specified Unicode codepoint in this string.
        /// The search starts at a specified codepoint position and proceeds backward toward the beginning of the
        /// string.
        /// </summary>
        public int LastIndexOf(UCodepoint value, int startIndex)
        {
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
            return IndexOf(value, 0, Length);
        }

        public int IndexOf(UString value, int startIndex)
        {
            return IndexOf(value, startIndex, Length - startIndex);
        }

        public int IndexOf(UString value, int startIndex, int count)
        {
            //if (value == null)
            //    throw new ArgumentNullException(nameof(value));
            //if (startIndex < 0)
            //    throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is less than zero");
            //if (count < 0)
            //    throw new ArgumentOutOfRangeException(nameof(count), "count is less than zero");
            //if (startIndex + count > Length)
            //    throw new ArgumentException("StartIndex and count must reside in the string");

            ////if (startIndex + value.Length)

            //int start = _startIndex + startIndex;
            //int end = start + count;
            //UCodepoint firstCodepoint = value.Length > 0 ? value[0] : UCodepoint.MinValue;
            //for (int i = start; i < end; i++)
            //{
            //    if (_codepoints[i] == firstCodepoint)
            //        return i - _startIndex;
            //}

            //return -1;
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

        /// <summary>
        /// Determines whether the beginning of this Unicode string matches the specified value.
        /// </summary>
        public bool StartsWith(UCodepoint value, bool ignoreCase = false)
        {
            if (Length == 0)
                return false;
            
            UCodepoint start = _codepoints[_startIndex];
            if (start == value)
                return true;

            return ignoreCase &&
                   (UCodepoint.ToLower(start) == value || UCodepoint.ToUpper(start) == value);
        }

        public bool StartsWith(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the end of this Unicode string matches the specified value.
        /// </summary>
        public bool EndsWith(UCodepoint value, bool ignoreCase = false)
        {
            if (Length == 0)
                return false;
            
            UCodepoint end = _codepoints[_startIndex + Length - 1];
            if (end == value)
                return true;

            return ignoreCase &&
                   (UCodepoint.ToLower(end) == value || UCodepoint.ToUpper(end) == value);
        }

        public bool EndsWith(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public bool Contains(UCodepoint value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public bool Contains(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        /// <summary>
        /// Splits a Unicode string into substrings that are based on the characters in an array.
        /// </summary>
        public IReadOnlyList<UString> Split(params UCodepoint[] separators)
        {
            return Split(separators, int.MaxValue);
        }

        /// <summary>
        /// Splits a Unicode string into a maximum number of substrings based on the characters in an array.
        /// </summary>
        public IReadOnlyList<UString> Split(UCodepoint[] separators, int maxCount,
            StringSplitOptions options = StringSplitOptions.None)
        {
            if (separators == null || separators.Length == 0)
                throw new ArgumentException("Separators cannot be null or empty.", nameof(separators));

            if (maxCount <= 0) 
                throw new ArgumentOutOfRangeException(nameof(maxCount), "maxCount must be greater than zero.");
            
            if (options != StringSplitOptions.None && options != StringSplitOptions.RemoveEmptyEntries)
                throw new ArgumentException("Unsupported option: " + options, nameof(options));

            int currentSegmentStart = 0;
            int segmentCount = 0;

            List<UString> result = new List<UString>();
            for (int i = 0; i < Length; i++)
            {
                if (Array.Exists(separators, s => this[i] == s))
                {
                    if (options != StringSplitOptions.RemoveEmptyEntries || i > currentSegmentStart)
                    {
                        result.Add(SubString(currentSegmentStart, i - currentSegmentStart));
                        segmentCount++;
                        
                        if (segmentCount == maxCount - 1)
                        {
                            result.Add(SubString(i + 1));
                            return result;
                        }
                    }

                    currentSegmentStart = i + 1;
                }
            }

            if (currentSegmentStart < Length || (options != StringSplitOptions.RemoveEmptyEntries && currentSegmentStart == Length))
                result.Add(SubString(currentSegmentStart));
            return result;
        }

        /// <summary>
        /// Gets copy of this Unicode string converted to uppercase using the
        /// casing rules of the invariant culture
        /// </summary>
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

        /// <summary>
        /// Gets copy of this Unicode string converted to lowercase using the
        /// casing rules of the invariant culture
        /// </summary>
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

        /// <summary>
        /// Gets this Unicode string as an array of Unicode codepoints
        /// </summary>
        public UCodepoint[] ToCodepointArray()
        {
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = _codepoints[i];
            return result;
        }

        /// <summary>
        /// Retrieves a portion from this Unicode string.
        /// The portion starts at a specified character and continues to the end of the string.
        /// </summary>
        public UString SubString(int start)
        {
            return SubString(start, Length - start);
        }

        /// <summary>
        /// Retrieves a portion from this Unicode string.
        /// The portion starts at a specified character and has the specified length.
        /// </summary>
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

        /// <summary>
        /// Normalizes the current Unicode string to the default normalization form (Form C).
        /// </summary>
        public UString Normalize()
        {
            return Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Normalizes the current Unicode string to the specified Unicode normalization form.
        /// </summary>
        public UString Normalize(NormalizationForm form)
        {
            return NormalizationEngine.Normalize(this, form);
        }

        /// <summary>
        /// Converts this Unicode string into a standard .Net string from a substring of
        /// this Unicode string.
        /// </summary>
        public string ToString(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "startIndex is less than zero");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is less than zero");
            if (start + length > Length)
                throw new ArgumentException("Start and length must reside in the string");

            StringBuilder result = new StringBuilder(length);
        
            int startIndex = _startIndex + start;
            int end = startIndex + length;
            for (int i = startIndex; i < end; i++)
            {
                UCodepoint uc = _codepoints[i];
                if (uc <= 0xFFFF)
                    result.Append((char)uc);
                else
                    result.Append(uc.ToString());
            }

            return result.ToString();
        }
        
        /// <summary>
        /// Determines the overall text direction of this Unicode string.
        /// A result of <see cref="TextDirection.Undefined"/> means that no strongly directional characters
        /// were found, or that any found characters were in an isolate section.
        /// </summary>
        /// <remarks>This algorithm uses The Paragraph Level algorithm specified in
        /// https://www.unicode.org/reports/tr9/#The_Paragraph_Level to determine level. Basically treating
        /// this Unicode string as a "paragraph".</remarks>
        public TextDirection DetermineDirection()
        {
            return HelperUtils.DetermineDirection(this);
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
            return ToString(0, Length);
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
        
        #region Internal methods
        internal void CopyTo(UCodepoint[] array, int arrayIndex)
        {
            Array.Copy(_codepoints, _startIndex, array, arrayIndex, Length);
        }
        #endregion

        #region UStringEnumerator class
        private sealed class UStringEnumerator : IEnumerator<UCodepoint>
        {
            private UString _str;
            private int _index = -1;
            private UCodepoint _current;

            public UStringEnumerator(UString str)
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
                _current = UCodepoint.Null;
            }
        }
        #endregion
    }
}
