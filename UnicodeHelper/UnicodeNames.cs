using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks>This class represents the data in the Unicode specification with name data coming from
    /// <see href="https://www.unicode.org/Public/UCD/latest/ucd/extracted/DerivedName.txt">DerivedName.txt</see>
    /// and combined with
    /// <see href="https://www.unicode.org/reports/tr44/#NameAliases.txt">NameAliases.txt</see></remarks>
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
                DataHelper.ReadResource("DerivedName.txt", derivedNameTextReader =>
                    Init(aliasesTextReader, derivedNameTextReader));
            });
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeNames using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~450ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor

        /// <summary>
        /// Initializes UnicodeNames using the files specified. The files must be in the default
        /// Unicode standard format for a <c>NameAliases.txt</c> file and <c>DerivedName.txt</c> file.
        /// </summary>
        public static void Init(string nameAliasesFilePath, string derivedNameFilePath)
        {
            DataHelper.ReadDataFile(nameAliasesFilePath, aliasesTextReader =>
            {
                DataHelper.ReadDataFile(derivedNameFilePath, derivedNameTextReader =>
                    Init(aliasesTextReader, derivedNameTextReader));
            });
        }

        /// <summary>
        /// Initializes UnicodeBlocks using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>NameAliases.txt</c> file and <c>DerivedName.txt</c> file.
        /// </summary>
        public static void Init(TextReader aliasesTextReader, TextReader derivedNameTextReader)
        {
            // Load Unicode defaults
            for (int i = 0; i < names.Length; i++)
                names[i] = new[] { new NameInfo("", NameType.None) };

            // Default to the names listed in the DerivedName file
            using (CsvReader reader = new CsvReader(derivedNameTextReader, DataHelper.CsvConfiguration))
            {
                foreach (DerivedNameFileLine line in reader.GetRecords<DerivedNameFileLine>())
                {
                    string name = line.Name.Trim();
                    DataHelper.HandleCodepointRange(line.CodePointRange, codepoint =>
                        AddName(codepoint, name, NameType.Base));
                }
            }

            // Merge data with what is in the NameAliases file
            using (CsvReader reader = new CsvReader(aliasesTextReader, DataHelper.CsvConfiguration))
            {
                foreach (NameAliasFileLine line in reader.GetRecords<NameAliasFileLine>())
                {
                    int codePoint = int.Parse(line.CodePoint, NumberStyles.HexNumber);
                    string name = line.Alias;
                    switch (line.Type)
                    {
                        case "control": AddName(codePoint, name, NameType.Base); break;
                        case "alternate": AddName(codePoint, name, NameType.Alternate); break;
                        case "abbreviation": AddName(codePoint, name, NameType.Abbreviation); break;
                        case "figment": AddName(codePoint, name, NameType.Figment); break;
                        
                        case "correction":
                            Debug.Assert(names[codePoint].Length == 1, "Unexpected correction of an alternate name");
                            names[codePoint][0] = new NameInfo(name, NameType.Base);
                            break;
                    }
                }
            }
        }
        #endregion
        
        #region Public methods
        /// <summary>
        /// Gets a list of names defined by the Unicode standard. Name order is not guaranteed,
        /// but should generally start with the most common name for a character.
        /// </summary>
        public static IReadOnlyList<NameInfo> GetNames(UCodepoint uc)
        {
            return names[(int)uc];
        }
        #endregion

        #region Helper methods
        private static void AddName(int codepoint, string name, NameType nameType)
        {
            NameInfo[] nameList = names[codepoint];
            if (nameList[0].NameType != NameType.None)
            {
                Array.Resize(ref nameList, nameList.Length + 1);
                names[codepoint] = nameList;
            }

            int patternIndex = name.IndexOf('*');
            if (patternIndex >= 0)
            {
                // Name replacement pattern
                Debug.Assert(patternIndex == name.Length - 1);
                name = name.Substring(0, patternIndex) + ((UCodepoint)codepoint).ToHexString();
            }
            nameList[nameList.Length - 1] = new NameInfo(name, nameType);
        }
        #endregion

        #region DerivedNameFileLine class
        private sealed class DerivedNameFileLine
        {
            [Index(0)]
            [UsedImplicitly]
            public string CodePointRange { get; set; }

            [Index(1)]
            [UsedImplicitly]
            public string Name { get; set; }
        }
        #endregion

        #region NameAliasFileLine class
        private sealed class NameAliasFileLine
        {
            [Index(0)]
            [UsedImplicitly]
            public string CodePoint { get; set; }

            [Index(1)]
            [UsedImplicitly]
            public string Alias { get; set; }

            [Index(2)]
            [UsedImplicitly]
            public string Type { get; set; }
        }
        #endregion
    }
}
