using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;

namespace UnicodeHelper.Internal
{
    internal static class DataHelper
    {
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
    }
}
