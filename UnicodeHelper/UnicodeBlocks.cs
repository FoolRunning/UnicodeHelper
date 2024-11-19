using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
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
        public static void Init() { } // Just invokes the static constructor

        public static void Init(string unicodeBlocksFilePath)
        {
            DataHelper.ReadDataFile(unicodeBlocksFilePath, Init);
        }

        public static void Init(TextReader textReader)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                AllowComments = true,
                IgnoreBlankLines = true,
                Delimiter = ";"
            };

            using (CsvReader reader = new CsvReader(textReader, config))
            {
                foreach (BlocksFileLine line in reader.GetRecords<BlocksFileLine>())
                {
                    string[] range = line.CodePointRange.Split(new[] {".."}, StringSplitOptions.None);
                    int startCodePoint = int.Parse(range[0], NumberStyles.HexNumber);
                    int endCodePoint = int.Parse(range[1], NumberStyles.HexNumber);
                    string blockName = line.BlockName.Trim();
                    
                    for (int c = startCodePoint; c <= endCodePoint; c++)
                        blocks.Add(new BlockRange((UChar)startCodePoint, (UChar)endCodePoint, blockName));
                }
            }
        }
        #endregion

        #region Public methods
        public static string GetBlockName(UChar uc)
        {
            int index = blocks.BinarySearch(new BlockRange(uc, uc, null));
            return index >= 0 ? blocks[index].BlockName : "No_Block";
        }
        #endregion

        #region BlockRange struct
        private readonly struct BlockRange : IComparable<BlockRange>
        {
            public readonly string BlockName;

            private readonly UChar _start;
            private readonly UChar _end;
            
            public BlockRange(UChar start, UChar end, string blockName)
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
