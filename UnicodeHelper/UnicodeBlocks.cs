using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>This class represents the data in the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#Blocks.txt">Blocks.txt</see></remarks>
    [PublicAPI]
    public static class UnicodeBlocks
    {
        #region Data fields
        private static readonly List<BlockRange> blocks = new List<BlockRange>();
        #endregion
        
        #region Static constructor
        static UnicodeBlocks()
        {
            DataHelper.ReadResource("Blocks.txt", Init);
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeBlocks using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~50ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor

        /// <summary>
        /// Initializes UnicodeBlocks using the file specified. The file must be in the default
        /// Unicode standard format for a <c>Blocks.txt</c> file.
        /// </summary>
        public static void Init(string unicodeBlocksFilePath)
        {
            DataHelper.ReadDataFile(unicodeBlocksFilePath, Init);
        }

        /// <summary>
        /// Initializes UnicodeBlocks using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>Blocks.txt</c> file.
        /// </summary>
        public static void Init(TextReader textReader)
        {
            using (CsvReader reader = new CsvReader(textReader, DataHelper.CsvConfiguration))
            {
                foreach (BlocksFileLine line in reader.GetRecords<BlocksFileLine>())
                {
                    string[] range = line.CodePointRange.Split(new[] {".."}, StringSplitOptions.None);
                    int startCodePoint = int.Parse(range[0], NumberStyles.HexNumber);
                    int endCodePoint = int.Parse(range[1], NumberStyles.HexNumber);
                    string blockName = line.BlockName.Trim();
                    
                    for (int c = startCodePoint; c <= endCodePoint; c++)
                        blocks.Add(new BlockRange((UCodepoint)startCodePoint, (UCodepoint)endCodePoint, blockName));
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the name of the Unicode block to which the specified character belongs. If the
        /// specified character does not belong to a known block, then "No_Block" is returned
        /// per the Unicode specification for
        /// <see href="https://www.unicode.org/reports/tr44/#Default_Values">Default Values</see>.
        /// </summary>
        public static string GetBlockName(UCodepoint uc)
        {
            int index = blocks.BinarySearch(new BlockRange(uc, uc, null));
            return index >= 0 ? blocks[index].BlockName : "No_Block";
        }
        #endregion

        #region BlockRange struct
        private readonly struct BlockRange : IComparable<BlockRange>
        {
            public readonly string BlockName;

            private readonly UCodepoint _start;
            private readonly UCodepoint _end;
            
            public BlockRange(UCodepoint start, UCodepoint end, string blockName)
            {
                _start = start;
                _end = end;
                BlockName = blockName;
            }

            public int CompareTo(BlockRange other)
            {
                if (_end < other._start)
                    return -1;
                if (_start > other._end)
                    return 1;
                return 0;
            }
        }
        #endregion
    }
}
