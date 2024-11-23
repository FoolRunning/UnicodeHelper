namespace UnicodeHelper
{
    [TestClass]
    public class UnicodeBlocksTests
    {
        #region GetBlockName tests
        private static IEnumerable<object[]> GetBlockNameTestData =>
        [
            [(UCodepoint)0x0080, "Latin-1 Supplement"], // Start of block
            [(UCodepoint)0x0095, "Latin-1 Supplement"],
            [(UCodepoint)0x00FF, "Latin-1 Supplement"], // End of block
            [(UCodepoint)0x058B, "Armenian"], // Undefined character
            [(UCodepoint)0x3045, "Hiragana"],
            [(UCodepoint)0x118A0, "Warang Citi"],
            [(UCodepoint)0x11850, "No_Block"]
        ];

        [TestMethod]
        [DynamicData(nameof(GetBlockNameTestData))]
        public void GetBlockName(UCodepoint uc, string expectedBlockName)
        {
            Assert.AreEqual(expectedBlockName, UnicodeBlocks.GetBlockName(uc));
        }
        #endregion
    }
}
