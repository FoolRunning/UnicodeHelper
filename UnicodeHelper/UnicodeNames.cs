using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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
        private static readonly NameInfo[][] names;
        #endregion

        #region Static constructor
        static UnicodeNames()
        {
            // All of the loading work is done by a separate class (see the remarks on Loader)
            Loader loader = new Loader();
            DataHelper.ReadResource("NameAliases.txt", aliasesTextReader =>
            {
                DataHelper.ReadResource("DerivedName.txt", derivedNameTextReader =>
                    loader.Load(aliasesTextReader, derivedNameTextReader));
            });
            names = loader.Names;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeNames using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~60ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor
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

        #region Loader class
        /// <summary>
        /// Loads the data from the <c>DerivedName.txt</c> and <c>NameAliases.txt</c> files into a table
        /// that is then handed to <see cref="UnicodeNames"/> by its static constructor.
        /// </summary>
        /// <remarks>This is a separate class for performance. While a class's static constructor is
        /// running, every call into a method of that class goes through a class-initialization check
        /// (~80ns). Loading makes hundreds of thousands of such calls, so doing the work in
        /// <see cref="UnicodeNames"/> itself made initialization noticeably slower. Instance methods
        /// of this class carry no such penalty.</remarks>
        private sealed class Loader
        {
            #region Data fields
            /// <summary>Names for each codepoint. <c>null</c> for codepoints with no name.</summary>
            public readonly NameInfo[][] Names = new NameInfo[UnicodeData.UnicodeCodepointCount][];

            private char[] _nameBuffer = new char[64];
            #endregion

            #region Public methods
            /// <summary>
            /// Loads the data using the specified readers. The data must be in the default
            /// Unicode standard format for a <c>NameAliases.txt</c> file and <c>DerivedName.txt</c> file.
            /// </summary>
            [MethodImpl(HelperUtils.AggressiveOptimization)]
            public void Load(TextReader aliasesTextReader, TextReader derivedNameTextReader)
            {
                // Default to the names listed in the DerivedName file
                foreach (string[] line in DataHelper.ReadDataFile(derivedNameTextReader, DerivedNameFieldCount))
                {
                    DataHelper.ParseCodepointRange(line[DerivedNameCodePointRangeField],
                        out int startCodePoint, out int endCodePoint);
                    string name = line[DerivedNameNameField];
                    if (name[name.Length - 1] == '*')
                    {
                        // Name replacement pattern: the '*' is replaced by the codepoint's hex value
                        for (int c = startCodePoint; c <= endCodePoint; c++)
                            AddName(c, ExpandNamePattern(name, c), NameType.Base);
                    }
                    else
                    {
                        for (int c = startCodePoint; c <= endCodePoint; c++)
                            AddName(c, name, NameType.Base);
                    }
                }

                // Merge data with what is in the NameAliases file
                foreach (string[] line in DataHelper.ReadDataFile(aliasesTextReader, AliasFieldCount))
                {
                    int codePoint = DataHelper.ParseHex(line[AliasCodePointField]);
                    string name = line[AliasNameField];
                    switch (line[AliasTypeField])
                    {
                        case "control": AddName(codePoint, name, NameType.Base); break;
                        case "alternate": AddName(codePoint, name, NameType.Alternate); break;
                        case "abbreviation": AddName(codePoint, name, NameType.Abbreviation); break;
                        case "figment": AddName(codePoint, name, NameType.Figment); break;

                        case "correction":
                            NameInfo[] nameList = Names[codePoint];
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

            #region Helper methods
            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private void AddName(int codepoint, string name, NameType nameType)
            {
                NameInfo[] nameList = Names[codepoint];
                if (nameList == null)
                    nameList = new NameInfo[1]; // Optimize for one name (vast majority of named codepoints)
                else
                    Array.Resize(ref nameList, nameList.Length + 1); // Codepoint has more than one name (rare)
                Names[codepoint] = nameList;

                Debug.Assert(name.IndexOf('*') == -1, "Name patterns must be expanded before being added");
                nameList[nameList.Length - 1] = new NameInfo(name, nameType);
            }

            /// <summary>
            /// Expands a name pattern (e.g. <c>CJK UNIFIED IDEOGRAPH-*</c>) for the specified codepoint by
            /// replacing the trailing <c>*</c> with the codepoint's hex value, formatted as by
            /// <see cref="UCodepointExtensions.ToHexString(UCodepoint,HexPadding)"/> with <see cref="HexPadding.Typical"/>.
            /// </summary>
            /// <remarks>Builds the name with a single string allocation. Over a hundred thousand names are
            /// created this way, and the garbage from building them piecemeal made initialization
            /// noticeably slower through extra garbage collections.</remarks>
            [MethodImpl(HelperUtils.AggressiveOptimization)]
            private string ExpandNamePattern(string pattern, int codepoint)
            {
                Debug.Assert(pattern[pattern.Length - 1] == '*');

                int prefixLength = pattern.Length - 1;
                int hexDigits = codepoint <= 0xFFFF ? 4 : (codepoint <= 0xFFFFF ? 5 : 6);
                int nameLength = prefixLength + hexDigits;
                if (_nameBuffer.Length < nameLength)
                    _nameBuffer = new char[nameLength * 2];

                pattern.CopyTo(0, _nameBuffer, 0, prefixLength);
                for (int i = nameLength - 1, value = codepoint; i >= prefixLength; i--, value >>= 4)
                {
                    int digit = value & 0xF;
                    _nameBuffer[i] = (char)(digit < 10 ? '0' + digit : 'A' + digit - 10);
                }

                return new string(_nameBuffer, 0, nameLength);
            }
            #endregion
        }
        #endregion
    }
}
