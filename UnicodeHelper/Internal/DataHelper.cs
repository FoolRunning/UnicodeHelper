using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using CsvHelper.Configuration;
using ICSharpCode.SharpZipLib.Zip;

namespace UnicodeHelper.Internal
{
    internal static class DataHelper
    {
        public static readonly CsvConfiguration CsvConfiguration = 
            new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = ";",
            AllowComments = true,
            IgnoreBlankLines = true
        };

        public static void ReadResource(string resourceFileName, Action<TextReader> readFileAction)
        {
            Stream zipStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UnicodeHelper.Resources.Resources.zip");
            Debug.Assert(zipStream != null, "Unable to find embedded resource zip");

            using (zipStream)
            using (ZipFile zip = new ZipFile(zipStream))
            {
                ZipEntry entry = zip.GetEntry(resourceFileName);
                Debug.Assert(entry != null, "Unable to find resource file in zip");

                using (Stream entryStream = zip.GetInputStream(entry))
                using (TextReader textReader = new StreamReader(entryStream))
                    readFileAction(textReader);
            }
        }

        public static void ReadDataFile(string resourceFilePath, Action<TextReader> readFileAction)
        {
            using (FileStream stream = new FileStream(resourceFilePath, FileMode.Open, FileAccess.Read))
            using (TextReader textReader = new StreamReader(stream))
                readFileAction(textReader);
        }

        public static void HandleCodepointRange(string codePointHexValue, Action<int> handleCodepoint)
        {
            string[] range = codePointHexValue.Split(new[] {".."}, StringSplitOptions.None);
            int codePoint = int.Parse(range[0], NumberStyles.HexNumber);
            if (range.Length == 1)
                handleCodepoint(codePoint);
            else
            {
                int endCodePoint = int.Parse(range[1], NumberStyles.HexNumber);
                for (int c = codePoint; c <= endCodePoint; c++)
                    handleCodepoint(c);
            }
        }

        public static bool HasFlags(this UnicodeProperty prop, UnicodeProperty toTest)
        {
            return (prop & toTest) != 0;
        }
    }
}
