using System;
using System.Buffers;
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
    public sealed class UStringBuilder : IDisposable
    {
        #region Constants / Data fields
        private const int DefaultCapacity = 16;

        private static readonly ArrayPool<UCodepoint> codepointArrayPool = ArrayPool<UCodepoint>.Shared;

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
        public UStringBuilder(int startingCapacity)
        {
            if (startingCapacity < DefaultCapacity)
                startingCapacity = DefaultCapacity;

            _codepoints = codepointArrayPool.Rent(startingCapacity);
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
        public UStringBuilder(UString ustr, int startingCapacity) : 
            this(startingCapacity < ustr.Length ? ustr.Length : startingCapacity)
        {
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
        public UStringBuilder(string str, int startingCapacity) :
            this(startingCapacity < str.Length ? str.Length : startingCapacity)
        {
            Append(str);
        }

        internal UStringBuilder(int startingCapacity, int startingLength) : this(startingCapacity)
        {
            _length = startingLength;
            EnsureCapacity(0);
        }
        #endregion

        #region Implmentation of IDisposable
        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="UStringBuilder"/> class.
        /// </summary>
        /// <remarks>
        /// This method returns the internal buffer of <see cref="UCodepoint"/> instances to the shared pool
        /// and suppresses finalization for the object. After calling this method, the <see cref="UStringBuilder"/>
        /// instance should not be used.
        /// </remarks>
        public void Dispose()
        {
            codepointArrayPool.Return(_codepoints);
            _codepoints = null;
            GC.SuppressFinalize(this);
        }

        ~UStringBuilder()
        {
            codepointArrayPool.Return(_codepoints);
            _codepoints = null;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the length of the <see cref="UStringBuilder"/>.
        /// </summary>
        // TODO: Write tests for this property get/set
        public int Length
        {
            get => _length;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");

                int origLength = _length;
                _length = value;
                EnsureCapacity(0);

                if (value > origLength)
                    Array.Clear(_codepoints, origLength, value - origLength);
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="UCodepoint"/> at the specified index in the <see cref="UStringBuilder"/>.
        /// </summary>
        // TODO: Write tests for this property get/set
        public UCodepoint this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                
                return _codepoints[index];
            }
            set
            {
                if (index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                
                _codepoints[index] = value;
            }
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
        /// Appends an array of Unicode codepoints.
        /// </summary>
        public void Append(UCodepoint[] uCodepoints, int length)
        {
            // TODO: Write tests for this method
            if (uCodepoints == null)
                throw new ArgumentNullException(nameof(uCodepoints));
            if (length < 0 || length > uCodepoints.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            EnsureCapacity(length);
            Array.Copy(uCodepoints, 0, _codepoints, _length, length);
            _length += length;
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
            UCodepoint[] codepoints = new UCodepoint[_length];
            Array.Copy(_codepoints, 0, codepoints, 0, _length);
            return _length == 0 ? UString.Empty : new UString(0, _length, codepoints);
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
            UCodepoint[] newArray = codepointArrayPool.Rent(newSize);
            
            Array.Copy(_codepoints, newArray, _codepoints.Length);
            
            codepointArrayPool.Return(_codepoints);
            _codepoints = newArray;
        }
        #endregion
    }
}
