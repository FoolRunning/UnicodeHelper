using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        #region Constants
        // DerivedName.txt fields
        private const int DerivedNameCodePointRangeField = 0;
        private const int DerivedNameNameField = 1;
        private const int DerivedNameFieldCount = 2;

        // NameAliases.txt fields
        private const int AliasCodePointField = 0;
        private const int AliasNameField = 1;
        private const int AliasTypeField = 2;
        private const int AliasFieldCount = 3;
        #endregion

        #region Data fields
        /// <summary>The names returned for a codepoint that has no name (the vast majority of codepoints)</summary>
        private static readonly NameInfo[] noNames = { new NameInfo("", NameType.None) };

        /// <summary>Names for each codepoint. <c>null</c> for codepoints with no name.</summary>
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
        /// Initializes UnicodeBlocks using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>NameAliases.txt</c> file and <c>DerivedName.txt</c> file.
        /// </summary>
        private static void Init(TextReader aliasesTextReader, TextReader derivedNameTextReader)
        {
            // Default to the names listed in the DerivedName file
            foreach (string[] line in DataHelper.ReadDataFile(derivedNameTextReader, DerivedNameFieldCount))
            {
                DataHelper.HandleCodepointRange(line[DerivedNameCodePointRangeField], line[DerivedNameNameField],
                    (n, cp) => AddName(cp, n, NameType.Base));
            }

            // Merge data with what is in the NameAliases file
            foreach (string[] line in DataHelper.ReadDataFile(aliasesTextReader, AliasFieldCount))
            {
                int codePoint = int.Parse(line[AliasCodePointField], NumberStyles.HexNumber);
                string name = line[AliasNameField];
                switch (line[AliasTypeField])
                {
                    case "control": AddName(codePoint, name, NameType.Base); break;
                    case "alternate": AddName(codePoint, name, NameType.Alternate); break;
                    case "abbreviation": AddName(codePoint, name, NameType.Abbreviation); break;
                    case "figment": AddName(codePoint, name, NameType.Figment); break;

                    case "correction":
                        NameInfo[] nameList = names[codePoint];
                        Debug.Assert(nameList != null && nameList.Length == 1, "Unexpected correction of an alternate name");
                        if (nameList == null)
                            AddName(codePoint, name, NameType.Base);
                        else
                            nameList[0] = new NameInfo(name, NameType.Base);
                        break;
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
            return names[(int)uc] ?? noNames;
        }
        #endregion

        #region Helper methods
        private static void AddName(int codepoint, string name, NameType nameType)
        {
            NameInfo[] nameList = names[codepoint];
            if (nameList == null)
                nameList = new NameInfo[1]; // Optimize for one name (vast majority of named codepoints)
            else
                Array.Resize(ref nameList, nameList.Length + 1); // Codepoint has more than one name (rare)
            names[codepoint] = nameList;

            if (name[name.Length - 1] == '*')
            {
                // Name replacement pattern
                name = name.Substring(0, name.Length - 1) + ((UCodepoint)codepoint).ToHexString();
            }
            else
            {
                Debug.Assert(name.IndexOf('*') == -1);
            }
            nameList[nameList.Length - 1] = new NameInfo(name, nameType);
        }
        #endregion
    }
}
