namespace UnicodeHelper
{
    [TestClass]
    public class DotNetStringExtensionsTests
    {
        #region Codepoints tests
        private static IEnumerable<object[]> CodepointsTestData =>
        [
            ["This is a test"],
            ["العربية"],
            ["😁🤔😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(CodepointsTestData))]
        public void Codepoints(string dotNetString)
        {
            Assert.That.SequenceEqual(new UString(dotNetString), dotNetString.Codepoints());
        }
        #endregion
    }
}
