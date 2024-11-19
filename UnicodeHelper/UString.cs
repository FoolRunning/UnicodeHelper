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
    public class UString : 
        IEquatable<UString>, 
        IEnumerable<UChar>,
        ICloneable,
        IComparable,
        IComparable<UString>
    {
        #region Data fields
        private readonly UChar[] _characters;
        /// <summary>Index of the character in the list where this string starts (for a substring)</summary>
        private readonly int _startCharacter;

        private int _cachedHash;
        #endregion

        #region Constructors
        public UString(IReadOnlyList<UChar> characters) : this(characters, 0, characters.Count)
        {
        }

        public UString(IReadOnlyList<UChar> characters, int start, int length)
        {
            // TODO: Write tests for this constructor
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start is less than zero");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is less than zero");
            if (start + length > characters.Count)
                throw new ArgumentException("Start and length must reside in array");

            _characters = characters.ToArray();
            _startCharacter = start;
            Length = length;
        }

        public UString(string dotNetString)
        {
            if (dotNetString == null)
                throw new ArgumentNullException(nameof(dotNetString));

            UChar[] chars = new UChar[dotNetString.Length];
            int index = 0;
            for (int i = 0; i < dotNetString.Length; i++)
            {
                UChar uc = UChar.ReadFromStr(dotNetString, i);
                chars[index++] = uc;
                if (uc > 0xFFFF)
                    i++; // Step over low surrogate
            }

            _characters = chars;
            _startCharacter = 0;
            Length = index;
        }

        private UString(int start, int length, UChar[] characters)
        {
            _characters = characters;
            _startCharacter = start;
            Length = length;
        }
        #endregion

        #region Properties
        public int Length { get; }

        public UChar this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "index is less than zero");
                if (index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index), "index must be less than the length of the string");

                return _characters[_startCharacter + index];
            }
        }
        #endregion

        #region Implementation of IEquatable
        public bool Equals(UString other)
        {
            if (ReferenceEquals(other, null) || other.Length != Length)
                return false;

            int end = _startCharacter + Length;
            int otherIndex = other._startCharacter;
            for (int i = _startCharacter; i < end; i++)
            {
                if (_characters[i] != other._characters[otherIndex++])
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
            return new UString(_startCharacter, Length, _characters);
        }
        #endregion
        
        #region Implementation of IEnumerable
        public IEnumerator<UChar> GetEnumerator()
        {
            // TODO: Write tests for this method
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
                yield return _characters[i];
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
            int end = _startCharacter + minLength;
            int otherIndex = other._startCharacter;
            for (int i = _startCharacter; i < end; i++)
            {
                int cmp = _characters[i].CompareTo(other._characters[otherIndex++]);
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
        public int IndexOf(UChar value)
        {
            return IndexOf(value, 0, Length);
        }

        public int IndexOf(UChar value, int startIndex)
        {
            return IndexOf(value, startIndex, Length - startIndex);
        }

        public int IndexOf(UChar value, int startIndex, int count)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public int LastIndexOf(UChar value)
        {
            return LastIndexOf(value, 0, Length);
        }

        public int LastIndexOf(UChar value, int startIndex)
        {
            return LastIndexOf(value, startIndex, Length - startIndex);
        }

        public int LastIndexOf(UChar value, int startIndex, int count)
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

        public IEnumerable<UString> Split(params UChar[] separators)
        {
            return Split(separators, int.MaxValue);
        }

        public IEnumerable<UString> Split(UChar[] separators, int maxCount, 
            StringSplitOptions options = StringSplitOptions.None)
        {
            // TODO: Write tests for this method
            throw new NotImplementedException();
        }

        public UString ToUpperInvariant()
        {
            UChar[] result = new UChar[Length];
            int index = 0;
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
                result[index++] = UnicodeData.ToUpper(_characters[i]);
            return new UString(0, result.Length, result);
        }

        public UString ToLowerInvariant()
        {
            UChar[] result = new UChar[Length];
            int index = 0;
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
                result[index++] = UnicodeData.ToLower(_characters[i]);
            return new UString(0, result.Length, result);
        }

        public UChar[] ToCharArray()
        {
            UChar[] result = new UChar[Length];
            int index = 0;
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
                result[index++] = _characters[i];
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

            return new UString(_startCharacter + start, length, _characters);
        }
        #endregion

        #region Overrides of Object
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            // TODO: Write tests for this method
            if (_cachedHash != 0)
                return _cachedHash;
            
            HashCode hc = new HashCode();
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
                hc.Add(_characters[i]);

            return _cachedHash = hc.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is UString other && Equals(other));
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder(Length);
        
            int end = _startCharacter + Length;
            for (int i = _startCharacter; i < end; i++)
            {
                UChar uc = _characters[i];
                if (uc <= 0xFFFF)
                    result.Append((char)uc);
                else
                    result.Append(uc.ToString());
            }

            return result.ToString();
        }
        #endregion

        #region Operator overrides
        public static bool operator ==(UString us1, UString us2)
        {
            return Equals(us1, us2);
        }

        public static bool operator !=(UString us1, UString us2)
        {
            return !Equals(us1, us2);
        }
        #endregion
    }
}
