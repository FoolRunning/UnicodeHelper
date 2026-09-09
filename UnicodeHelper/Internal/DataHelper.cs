using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace UnicodeHelper.Internal
{
    internal static class DataHelper
    {
        private const string CodePointRangeSeparator = "..";
        private const char FieldSeparator = ';';
        private const char CommentStart = '#';

        public static void ReadResource(string resourceFileName, Action<TextReader> readFileAction)
        {
            Stream zipStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UnicodeHelper.Resources.Resources.zip");
            Debug.Assert(zipStream != null, "Unable to find embedded resource zip");

            using (zipStream)
            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = zip.GetEntry(resourceFileName);
                Debug.Assert(entry != null, "Unable to find resource file in zip");

                using (Stream entryStream = entry.Open())
                using (TextReader textReader = new StreamReader(entryStream))
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
            string[] fields = new string[fieldCount];
            string line;
            while ((line = reader.ReadLine()) != null)
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

                yield return fields;
            }
        }

        /// <summary>
        /// Parses a codepoint or codepoint range in the format used by the Unicode data files: a single
        /// hexadecimal codepoint (e.g. <c>0041</c>) or an inclusive range (e.g. <c>0041..005A</c>).
        /// </summary>
        public static void ParseCodepointRange(string codePointHexValue, out int startCodePoint, out int endCodePoint)
        {
            int separatorIndex = codePointHexValue.IndexOf(CodePointRangeSeparator, StringComparison.Ordinal);
            if (separatorIndex < 0)
            {
                startCodePoint = int.Parse(codePointHexValue, NumberStyles.HexNumber);
                endCodePoint = startCodePoint;
            }
            else
            {
                startCodePoint = int.Parse(codePointHexValue.Substring(0, separatorIndex), NumberStyles.HexNumber);
                endCodePoint = int.Parse(codePointHexValue.Substring(separatorIndex + CodePointRangeSeparator.Length),
                    NumberStyles.HexNumber);
            }
        }

        public static void HandleCodepointRange<T>(string codePointHexValue, T target,
            Action<T, int> handleCodepoint)
        {
            ParseCodepointRange(codePointHexValue, out int startCodePoint, out int endCodePoint);
            for (int c = startCodePoint; c <= endCodePoint; c++)
                handleCodepoint(target, c);
        }
    }
}
