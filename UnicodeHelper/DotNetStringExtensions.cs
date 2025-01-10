using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// A collection of extensions for .Net strings to better work with Unicode codepoints
    /// </summary>
    [PublicAPI]
    public static class DotNetStringExtensions
    {
        /// <summary>
        /// Gets a list of Unicode codepoints in this .Net string.
        /// </summary>
        public static IEnumerable<UCodepoint> Codepoints(this string dotNetString)
        {
            return new DotNetStringEnumerator(dotNetString);
        }

        #region DotNetStringEnumerator class
        private sealed class DotNetStringEnumerator : IEnumerator<UCodepoint>, IEnumerable<UCodepoint>
        {
            private readonly string _dotNetString;
            private int _index = -1;
            private UCodepoint _current;

            public DotNetStringEnumerator(string dotNetString)
            {
                _dotNetString = dotNetString ?? throw new ArgumentNullException(nameof(dotNetString));
            }

            public void Dispose()
            {
                _index = _dotNetString.Length;
            }

            public UCodepoint Current
            {
                get
                {
                    if (_index == -1)
                        throw new InvalidOperationException("Enumerator not started");
                    if (_index >= _dotNetString.Length)
                        throw new InvalidOperationException("Enumerator has completed");
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index += (_current <= 0xFFFF) ? 1 : 2;
                if (_index >= _dotNetString.Length)
                    return false;

                _current = UCodepoint.ReadFromStr(_dotNetString, _index);
                return true;
            }

            public void Reset()
            {
                _index = -1;
                _current = UCodepoint.MinValue;
            }

            public IEnumerator<UCodepoint> GetEnumerator()
            {
                Reset();
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion
    }
}
