using CsvHelper;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;

namespace UnicodeHelper.TestData
{
    public sealed record NormalizationTestData(UString Source, 
        UString NfcResult, UString NfdResult, UString NfkcResult, UString NfkdResult, 
        string Description)
    {
        public override string ToString()
        {
            return Description;
        }
    }

    /// <remarks>Test data taken from https://www.unicode.org/Public/UCD/latest/ucd/NormalizationTest.txt</remarks>
    internal static class NormalizationTestDataSet
    {
        #region Data fields
        private static readonly UStringBuilder dataBldr = new();
        private static readonly List<NormalizationTestData> testCases = new();
        #endregion

        #region Static constructor
        static NormalizationTestDataSet()
        {
            Stream? dataStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("UnicodeHelper.TestData.NormalizationTest.txt");
            Debug.Assert(dataStream != null, "Unable to find embedded test data");

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
                AllowComments = false,
                IgnoreBlankLines = true,
                Mode = CsvMode.NoEscape,
                TrimOptions = TrimOptions.None,
                MissingFieldFound = null
            };

            using TextReader textReader = new StreamReader(dataStream);
            using CsvReader reader = new CsvReader(textReader, config);
            foreach (TestDataLine line in reader.GetRecords<TestDataLine>())
            {
                if (line.Source.StartsWith('#') || line.Source.StartsWith('@'))
                    continue;
                
                testCases.Add(new NormalizationTestData(CreateUStringFromCodepoints(line.Source), 
                    CreateUStringFromCodepoints(line.NfcResult),
                    CreateUStringFromCodepoints(line.NfdResult), 
                    CreateUStringFromCodepoints(line.NfkcResult),
                    CreateUStringFromCodepoints(line.NfkdResult), 
                    string.Join("", line.Comments).TrimStart(' ', '#')));
            }
        }
        #endregion

        #region Properties
        public static IEnumerable<NormalizationTestData> TestCases => testCases;
        #endregion

        #region Helper methods
        private static UString CreateUStringFromCodepoints(string? codepoints)
        {
            if (codepoints == null)
                return UString.Empty;
            
            dataBldr.Clear();
            
            foreach (string part in codepoints.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int cp = int.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                dataBldr.Append((UCodepoint)cp);
            }

            return dataBldr.ToUString();
        }
        #endregion

        #region TestDataLine class
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class TestDataLine
        {
            [Index(0)]
            public required string Source { get; set; }

            [Index(1)]
            public string? NfcResult { get; set; }

            [Index(2)]
            public string? NfdResult { get; set; }

            [Index(3)]
            public string? NfkcResult { get; set; }

            [Index(4)]
            public string? NfkdResult { get; set; }

            [Index(5, 20)]
            public required string?[] Comments { get; set; }
        }
        #endregion
    }
}
