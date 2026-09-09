using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace UnicodeHelper.TestData
{
    #region NormalizationTestData record
    public sealed record NormalizationTestData(UString Source,
        UString NfcResult, UString NfdResult, UString NfkcResult, UString NfkdResult,
        string Description)
    {
        public override string ToString()
        {
            return Description;
        }
    }
    #endregion

    /// <remarks>Test data taken from https://www.unicode.org/Public/UCD/latest/ucd/NormalizationTest.txt</remarks>
    internal static class NormalizationTestDataSet
    {
        #region Constants
        // Format of each line: source; NFC; NFD; NFKC; NFKD; # comment
        private const int SourceField = 0;
        private const int NfcField = 1;
        private const int NfdField = 2;
        private const int NfkcField = 3;
        private const int NfkdField = 4;
        private const int CommentField = 5;
        private const int FieldCount = 6;
        #endregion

        #region Data fields
        private static readonly List<NormalizationTestData> testCases = new();
        #endregion

        #region Static constructor
        static NormalizationTestDataSet()
        {
            Stream? dataStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UnicodeHelper.TestData.NormalizationTest.txt");
            Debug.Assert(dataStream != null, "Unable to find embedded test data");

            using UStringBuilder dataBldr = new();
            using TextReader textReader = new StreamReader(dataStream);
            string? line;
            while ((line = textReader.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#' || line[0] == '@')
                    continue;

                // The comment (last field) can itself contain semicolons, so limit the split
                string[] fields = line.Split(';', FieldCount);
                if (fields.Length < FieldCount)
                    throw new InvalidDataException("Unexpected test data line: " + line);

                testCases.Add(new NormalizationTestData(CreateUStringFromCodepoints(fields[SourceField], dataBldr),
                    CreateUStringFromCodepoints(fields[NfcField], dataBldr),
                    CreateUStringFromCodepoints(fields[NfdField], dataBldr),
                    CreateUStringFromCodepoints(fields[NfkcField], dataBldr),
                    CreateUStringFromCodepoints(fields[NfkdField], dataBldr),
                    fields[CommentField].TrimStart(' ', '#')));
            }
        }
        #endregion

        #region Properties
        public static IEnumerable<NormalizationTestData> TestCases => testCases;
        #endregion

        #region Helper methods
        private static UString CreateUStringFromCodepoints(string codepoints, UStringBuilder dataBldr)
        {
            dataBldr.Clear();

            foreach (string part in codepoints.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int cp = int.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                dataBldr.Append((UCodepoint)cp);
            }

            return dataBldr.ToUString();
        }
        #endregion
    }
}
