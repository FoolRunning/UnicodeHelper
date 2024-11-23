using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace UnicodeHelper
{
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
        public UString(IReadOnlyList<UCodepoint> codepoints) : this(codepoints, 0, codepoints.Count)
        {
        }

        public UString(IReadOnlyList<UCodepoint> codepoints, int start, int length)
        {
            // TODO: Write tests for this constructor
            if (codepoints == null)
                throw new ArgumentNullException(nameof(codepoints));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start is less than zero");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is less than zero");
            if (start + length > codepoints.Count)
                throw new ArgumentException("Start and length must reside in array");

            _codepoints = codepoints.ToArray();
            _startIndex = start;
            Length = length;
        }

        public UString(string dotNetString)
        {
            if (dotNetString == null)
                throw new ArgumentNullException(nameof(dotNetString));

            UCodepoint[] codepoints = new UCodepoint[dotNetString.Length];
            int index = 0;
            for (int i = 0; i < dotNetString.Length; i++)
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

        private UString(int start, int length, UCodepoint[] codepoints)
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
        public object Clone()
        {
            // TODO: Write tests for this method
            // Since this class is immutable, just reuse the same underlying data.
            return new UString(_startIndex, Length, _codepoints);
        }
        #endregion
        
        #region Implementation of IEnumerable
        public IEnumerator<UCodepoint> GetEnumerator()
        {
            // TODO: Write tests for this method
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                yield return _codepoints[i];
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        
        #region Implementation of IComparable
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
        
        public int CompareTo(object obj)
        {
            if (!(obj is UString other))
                throw new ArgumentException("obj is not a UString instance");
            return CompareTo(other);
        }
        #endregion

        #region Other public methods
        public int IndexOf(UCodepoint value)
        {
            return IndexOf(value, 0, Length);
        }

        public int IndexOf(UCodepoint value, int startIndex)
        {
            return IndexOf(value, startIndex, Length - startIndex);
        }

        public int IndexOf(UCodepoint value, int startIndex, int count)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public int LastIndexOf(UCodepoint value)
        {
            return LastIndexOf(value, 0, Length);
        }

        public int LastIndexOf(UCodepoint value, int startIndex)
        {
            return LastIndexOf(value, startIndex, Length - startIndex);
        }

        public int LastIndexOf(UCodepoint value, int startIndex, int count)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
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
            //return IndexOf(value) == 0;
        }

        public bool EndsWith(UString value, bool ignoreCase = false)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
            //return LastIndexOf(value) == Length - value.Length;
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
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = UnicodeData.ToUpper(_codepoints[i]);
            return new UString(0, result.Length, result);
        }

        public UString ToLowerInvariant()
        {
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = UnicodeData.ToLower(_codepoints[i]);
            return new UString(0, result.Length, result);
        }

        public UCodepoint[] ToCharArray()
        {
            UCodepoint[] result = new UCodepoint[Length];
            int index = 0;
            int end = _startIndex + Length;
            for (int i = _startIndex; i < end; i++)
                result[index++] = _codepoints[i];
            return result.ToArray();
        }

        public UString SubString(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start is less than zero");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is less than zero");
            if (start + length > Length)
                throw new ArgumentException("Start and length must reside in the string");

            return new UString(_startIndex + start, length, _codepoints);
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
        #endregion
    }
}
