using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    [PublicAPI]
    public static class UnicodeNames
    {
        #region Data fields
        private const string ControlName = "<control>";

        private static readonly NameInfo[][] names = new NameInfo[UnicodeData.UnicodeCodepointCount][];
        #endregion

        #region Static constructor
        static UnicodeNames()
        {
            DataHelper.ReadResource("NameAliases.txt", aliasesTextReader =>
            {
                DataHelper.ReadResource("UnicodeData.txt", unicodeDataTextReader =>
                    Init(aliasesTextReader, unicodeDataTextReader));
            });
        }
        #endregion

        #region Initialization
        public static void Init() { } // Just invokes the static constructor

        public static void Init(string nameAliasesFilePath, string unicodeDataFilePath)
        {
            DataHelper.ReadDataFile(nameAliasesFilePath, aliasesTextReader =>
            {
                DataHelper.ReadDataFile(unicodeDataFilePath, unicodeDataTextReader =>
                    Init(aliasesTextReader, unicodeDataTextReader));
            });
        }

        public static void Init(TextReader aliasesTextReader, TextReader unicodeDataTextReader)
        {
            // Load Unicode defaults
            for (int i = 0; i < names.Length; i++)
                names[i] = new[] { new NameInfo("", "") };

            // Default to the names listed in the UnicodeData file
            UnicodeData.ProcessFile(unicodeDataTextReader, (codePoint, line) =>
                names[codePoint][0].Name = line.Name);
            
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ";",
                AllowComments = true,
                IgnoreBlankLines = true
            };

            // Overwrite data with what is in the NameAliases file
            using (CsvReader reader = new CsvReader(aliasesTextReader, config))
            {
                foreach (NameAliasFileLine line in reader.GetRecords<NameAliasFileLine>())
                {
                    int codePoint = int.Parse(line.CodePoint, NumberStyles.HexNumber);
                    if (codePoint == 0xFEFF && line.Alias == "BOM")
                    {
                        Debug.Assert(names[codePoint].Length == 2);
                        names[codePoint][1].Abbreviation = line.Alias;
                        continue;
                    }

                    NameInfo currentInfo = names[codePoint][0];
                    switch (line.Type)
                    {
                        case "correction":
                        case "control":
                            currentInfo.Name = line.Alias;
                            break;

                        case "alternate":
                            names[codePoint] = new[] { currentInfo, new NameInfo(line.Alias, "") };
                            break;
                        
                        case "figment":
                            if (string.IsNullOrEmpty(currentInfo.Name) || currentInfo.Name == ControlName)
                                currentInfo.Name = line.Alias;
                            break;

                        case "abbreviation":
                            currentInfo.Abbreviation = line.Alias;
                            break;
                    }
                }
            }
        }
        #endregion
        
        #region Public methods
        public static IReadOnlyList<NameInfo> GetNames(UChar uc)
        {
            return names[(int)uc];
        }
        #endregion
    }
}
