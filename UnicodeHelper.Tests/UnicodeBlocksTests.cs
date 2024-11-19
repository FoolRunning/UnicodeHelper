namespace UnicodeHelper
{
    [TestClass]
    public class UnicodeBlocksTests
    {
        #region GetBlockName tests
        private static IEnumerable<object[]> GetBlockNameTestData =>
        [
            [(UChar)0x0080, "Latin-1 Supplement"], // Start of block
            [(UChar)0x0095, "Latin-1 Supplement"],
            [(UChar)0x00FF, "Latin-1 Supplement"], // End of block
            [(UChar)0x058B, "Armenian"], // Undefined character
            [(UChar)0x3045, "Hiragana"],
            [(UChar)0x118A0, "Warang Citi"],
            [(UChar)0x11850, "No_Block"]
        ];

        [TestMethod]
        [DynamicData(nameof(GetBlockNameTestData))]
        public void GetBlockName(UChar uc, string expectedBlockName)
        {
            Assert.AreEqual(expectedBlockName, UnicodeBlocks.GetBlockName(uc));
        }
        #endregion
    }
}
