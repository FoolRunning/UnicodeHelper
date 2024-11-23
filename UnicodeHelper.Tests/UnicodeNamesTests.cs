namespace UnicodeHelper
{
    [TestClass]
    public class UnicodeNamesTests
    {
        #region GetNames tests
        private static IEnumerable<object[]> GetNamesTestData =>
        [
            [(UCodepoint)0x0212, new[] { "LATIN CAPITAL LETTER R WITH INVERTED BREVE" }, 
                new[] { NameType.Base }], // default
            [(UCodepoint)0x0000, new[] { "NULL", "NUL" }, 
                new[] { NameType.Base, NameType.Abbreviation }], // control alias
            [(UCodepoint)0x0080, new[] { "PADDING CHARACTER", "PAD" }, 
                new[] { NameType.Figment, NameType.Abbreviation }], // figment alias
            [(UCodepoint)0x01A2, new[] { "LATIN CAPITAL LETTER GHA" }, 
                new[] { NameType.Base }], // correction
            [(UCodepoint)0xFEFF, new[] { "ZERO WIDTH NO-BREAK SPACE", "BYTE ORDER MARK", "BOM", "ZWNBSP" } , 
                new[] { NameType.Base, NameType.Alternate, NameType.Abbreviation, NameType.Abbreviation }], // alternate
            [(UCodepoint)0x000A, new[] { "LINE FEED", "NEW LINE", "END OF LINE", "LF", "NL", "EOL" } , 
                new[] { NameType.Base, NameType.Base, NameType.Base, NameType.Abbreviation, NameType.Abbreviation, NameType.Abbreviation }],
            [(UCodepoint)0x4E00, new[] { "CJK UNIFIED IDEOGRAPH-4E00" }, new[] { NameType.Base }], // Replacement pattern start (low)
            [(UCodepoint)0x4E57, new[] { "CJK UNIFIED IDEOGRAPH-4E57" }, new[] { NameType.Base }], // Replacement pattern middle (low)
            [(UCodepoint)0x9FFF, new[] { "CJK UNIFIED IDEOGRAPH-9FFF" }, new[] { NameType.Base }], // Replacement pattern end (low)
            [(UCodepoint)0x17052, new[] { "TANGUT IDEOGRAPH-17052" }, new[] { NameType.Base }], // Replacement pattern (high)
        ];

        [TestMethod]
        [DynamicData(nameof(GetNamesTestData))]
        public void GetNames(UCodepoint uc, string[] expectedNames, NameType[] expectedTypes)
        {
            IReadOnlyList<NameInfo> names = UnicodeNames.GetNames(uc);
            for (int i = 0; i < expectedNames.Length; i++)
            {
                Assert.AreEqual(expectedNames[i], names[i].Name);
                Assert.AreEqual(expectedTypes[i], names[i].NameType);
            }
        }
        #endregion
    }
}
