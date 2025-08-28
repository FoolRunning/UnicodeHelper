using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Provides a mutable builder for constructing instances of <see cref="UString"/>.
    /// </summary>
    /// <remarks>
    /// This class is designed to efficiently build Unicode strings by appending
    /// individual <see cref="UCodepoint"/> instances, .NET strings, or other <see cref="UString"/> objects.
    /// It manages an internal buffer to minimize memory allocations during string construction.
    /// </remarks>
    [PublicAPI]
    public sealed class UStringBuilder
    {
        #region Constants / Data fields
        private const int DefaultCapacity = 16;
        
        private UCodepoint[] _codepoints;
        private int _length;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with a default capacity.
        /// </summary>
        /// <remarks>
        /// This constructor creates an instance of <see cref="UStringBuilder"/> with an initial capacity
        /// sufficient to hold a small number of <see cref="UCodepoint"/> instances. The capacity can grow
        /// dynamically as needed when appending additional content.
        /// </remarks>
        public UStringBuilder() : this(DefaultCapacity)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with the specified starting capacity.
        /// </summary>
        public UStringBuilder(int startingCapacity) : this (UString.Empty, startingCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with the specified
        /// <see cref="UString"/> as its initial content.
        /// </summary>
        public UStringBuilder(UString ustr) : this(ustr, ustr?.Length ?? 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with the specified
        /// <see cref="UString"/> and starting capacity.
        /// </summary>
        public UStringBuilder(UString ustr, int startingCapacity)
        {
            if (startingCapacity < ustr.Length)
                startingCapacity = ustr.Length;
            _codepoints = new UCodepoint[startingCapacity];
            Append(ustr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with the specified
        /// .Net string as its initial content.
        /// </summary>
        public UStringBuilder(string ustr) : this(ustr, ustr?.Length ?? 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UStringBuilder"/> class with the specified
        /// .Net string and starting capacity.
        /// </summary>
        public UStringBuilder(string str, int startingCapacity)
        {
            if (startingCapacity < str.Length)
                startingCapacity = str.Length;
            _codepoints = new UCodepoint[startingCapacity];
            Append(str);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Appends a single Unicode codepoint.
        /// </summary>
        public void Append(UCodepoint uc)
        {
            EnsureCapacity(1);
            
            _codepoints[_length++] = uc;
        }

        /// <summary>
        /// Appends a Unicode string.
        /// </summary>
        public void Append(UString ustr)
        {
            if (UString.IsNullOrEmpty(ustr))
                return;
            
            EnsureCapacity(ustr.Length);
            
            ustr.CopyTo(_codepoints, _length);
            _length += ustr.Length;
        }

        /// <summary>
        /// Appends a .Net string.
        /// </summary>
        public void Append(string dotNetStr)
        {
            if (string.IsNullOrEmpty(dotNetStr))
                return;
            
            EnsureCapacity(dotNetStr.Length);
            
            foreach (UCodepoint uc in dotNetStr.Codepoints())
                _codepoints[_length++] = uc;
        }

        /// <summary>
        /// Clears the content of the <see cref="UStringBuilder"/> by resetting its length to zero.
        /// </summary>
        /// <remarks>
        /// This method does not release the internal buffer, allowing the <see cref="UStringBuilder"/>
        /// to retain its current capacity for future use. Use this method to efficiently reuse
        /// the same instance for constructing new <see cref="UString"/> objects.
        /// </remarks>
        public void Clear()
        {
            _length = 0;
        }

        /// <summary>
        /// Converts the current <see cref="UStringBuilder"/> instance to an immutable <see cref="UString"/>.
        /// </summary>
        public UString ToUString()
        {
            return _length == 0 ? UString.Empty : new UString(_codepoints, 0, _length);
        }
        #endregion
        
        #region Helper methods
        private void EnsureCapacity(int additionalCapacity)
        {
            int newSize = _codepoints.Length;
            while (_length + additionalCapacity > newSize)
                newSize = newSize * 3 / 2 + 1;
            
            if (newSize > _codepoints.Length)
                Resize(newSize);
        }

        private void Resize(int newSize)
        {
            Debug.Assert(newSize > _codepoints.Length);
            
            UCodepoint[] newArray = new UCodepoint[newSize];
            Array.Copy(_codepoints, newArray, _codepoints.Length);
            _codepoints = newArray;
        }
        #endregion
    }
}
