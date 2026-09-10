using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace UnicodeHelper.Internal
{
    internal static class DataHelper
    {
        private const string CodePointRangeSeparator = "..";
        private const char FieldSeparator = ';';
        private const char CommentStart = '#';

        /// <summary>
        /// Buffer size for reading the decompressed data. Each read from the deflate stream has a fixed
        /// overhead, so the default 1KB StreamReader buffer makes reading a 2MB file several times slower.
        /// </summary>
        private const int ReadBufferSize = 64 * 1024;

        public static void ReadResource(string resourceFileName, Action<TextReader> readFileAction)
        {
            Stream zipStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UnicodeHelper.Resources.Resources.zip");
            Debug.Assert(zipStream != null, "Unable to find embedded resource zip");

            using (zipStream) 
            // ReSharper disable once AssignNullToNotNullAttribute
            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = zip.GetEntry(resourceFileName);
                Debug.Assert(entry != null, "Unable to find resource file in zip");

                // ReSharper disable once PossibleNullReferenceException
                using (Stream entryStream = entry.Open())
                using (TextReader textReader = new StreamReader(entryStream, Encoding.UTF8, true, ReadBufferSize))
                    readFileAction(textReader);
            }
        }

        /// <summary>
        /// Reads a file in the standard Unicode Character Database format: one record per line with
        /// fields separated by semicolons, <c>#</c> starting a comment that runs to the end of the line,
        /// and blank lines ignored. Fields are trimmed of surrounding whitespace.
        /// </summary>
        /// <param name="reader">The reader for the file</param>
        /// <param name="fieldCount">The maximum number of fields a line may contain. Fields missing
        /// from a line are <c>null</c>.</param>
        /// <remarks>For efficiency, the same array instance is yielded for every line. Callers must
        /// not hold onto the array (or its contents by reference) between iterations.</remarks>
        public static IEnumerable<string[]> ReadDataFile(TextReader reader, int fieldCount)
        {
            return new DataFileReader(reader, fieldCount);
        }

        /// <summary>
        /// Parses a codepoint or codepoint range in the format used by the Unicode data files: a single
        /// hexadecimal codepoint (e.g. <c>0041</c>) or an inclusive range (e.g. <c>0041..005A</c>).
        /// </summary>
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        public static void ParseCodepointRange(string codePointHexValue, out int startCodePoint, out int endCodePoint)
        {
            int separatorIndex = codePointHexValue.IndexOf(CodePointRangeSeparator, StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                startCodePoint = ParseHex(codePointHexValue);
                endCodePoint = startCodePoint;
            }
            else
            {
                startCodePoint = ParseHex(codePointHexValue, 0, separatorIndex);
                int endStart = separatorIndex + CodePointRangeSeparator.Length;
                endCodePoint = ParseHex(codePointHexValue, endStart, codePointHexValue.Length - endStart);
            }
        }

        /// <summary>
        /// Parses a hexadecimal number as found in the Unicode data files (no prefix, no whitespace).
        /// </summary>
        /// <remarks>Considerably faster than <c>int.Parse</c> with <c>NumberStyles.HexNumber</c>,
        /// which matters for the hundreds of thousands of values parsed during initialization.</remarks>
        public static int ParseHex(string hexValue)
        {
            return ParseHex(hexValue, 0, hexValue.Length);
        }

        /// <summary>
        /// Parses a hexadecimal number from the specified portion of a string.
        /// </summary>
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        private static int ParseHex(string hexValue, int start, int length)
        {
            if (length <= 0 || length > 8)
                throw new FormatException("Invalid hexadecimal value: " + hexValue);

            int value = 0;
            int end = start + length;
            for (int i = start; i < end; i++)
            {
                char c = hexValue[i];
                int digit;
                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else if (c >= 'a' && c <= 'f')
                    digit = c - 'a' + 10;
                else
                    throw new FormatException("Invalid hexadecimal value: " + hexValue);

                value = (value << 4) | digit;
            }

            return value;
        }

        #region DataFileReader class
        /// <summary>
        /// Enumerates the records of a Unicode data file (see <see cref="ReadDataFile"/>).
        /// </summary>
        /// <remarks>Handwritten rather than a compiler-generated iterator so that <see cref="MoveNext"/>
        /// can be marked with <see cref="HelperUtils.AggressiveOptimization"/>.</remarks>
        private sealed class DataFileReader : IEnumerable<string[]>, IEnumerator<string[]>
        {
            private readonly TextReader _reader;
            private readonly string[] _fields;

            public DataFileReader(TextReader reader, int fieldCount)
            {
                _reader = reader;
                _fields = new string[fieldCount];
            }

            public string[] Current => _fields;

            object IEnumerator.Current => _fields;

            public IEnumerator<string[]> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            [MethodImpl(HelperUtils.AggressiveOptimization)]
            public bool MoveNext()
            {
                string[] fields = _fields;
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    int lineEnd = line.IndexOf(CommentStart);
                    if (lineEnd < 0)
                        lineEnd = line.Length;

                    int fieldIndex = 0;
                    int fieldStart = 0;
                    while (true)
                    {
                        int separatorIndex = line.IndexOf(FieldSeparator, fieldStart, lineEnd - fieldStart);
                        int fieldEnd = separatorIndex < 0 ? lineEnd : separatorIndex;

                        // Trim whitespace
                        int start = fieldStart;
                        int end = fieldEnd;
                        while (start < end && char.IsWhiteSpace(line[start]))
                            start++;
                        while (end > start && char.IsWhiteSpace(line[end - 1]))
                            end--;

                        if (fieldIndex == fields.Length)
                            throw new InvalidOperationException("Unexpected number of fields in line: " + line);

                        fields[fieldIndex++] = line.Substring(start, end - start);

                        if (separatorIndex < 0)
                            break;

                        fieldStart = separatorIndex + 1;
                    }

                    if (fieldIndex == 1 && fields[0].Length == 0)
                        continue; // Blank or comment-only line

                    for (int i = fieldIndex; i < fields.Length; i++)
                        fields[i] = null;

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
            }
        }
        #endregion
    }
}
