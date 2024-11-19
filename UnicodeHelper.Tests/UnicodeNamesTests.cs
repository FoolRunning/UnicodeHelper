namespace UnicodeHelper
{
    [TestClass]
    public class UnicodeNamesTests
    {
        #region GetNames tests
        private static IEnumerable<object[]> GetNameTestData =>
        [
            [(UChar)0x0212, new[] { "LATIN CAPITAL LETTER R WITH INVERTED BREVE" }, new[] { "" }], // default
            [(UChar)0x0000, new[] {"NULL"}, new[] {"NUL"}], // control alias
            [(UChar)0x0080, new[] { "PADDING CHARACTER" }, new[] { "PAD" }], // figment alias
            [(UChar)0x01A2, new[] { "LATIN CAPITAL LETTER GHA" }, new[] { "" }], // correction
            [(UChar)0xFEFF, new[] { "ZERO WIDTH NO-BREAK SPACE", "BYTE ORDER MARK" } , new[] { "ZWNBSP", "BOM" }] // alternate
        ];

        [TestMethod]
        [DynamicData(nameof(GetNameTestData))]
        public void GetName(UChar uc, string[] expectedNames, string[] expectedAbbreviations)
        {
            IReadOnlyList<NameInfo> names = UnicodeNames.GetNames(uc);
            for (int i = 0; i < expectedNames.Length; i++)
            {
                Assert.AreEqual(expectedNames[i], names[i].Name);
                Assert.AreEqual(expectedAbbreviations[i], names[i].Abbreviation);
            }
        }
        #endregion
    }
}
