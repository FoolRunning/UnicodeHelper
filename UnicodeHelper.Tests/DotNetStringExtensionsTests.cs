using System.Diagnostics.CodeAnalysis;

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

        [TestMethod]
        [DynamicData(nameof(CodepointsTestData))]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void Codepoints_MultipleIterations(string dotNetString)
        {
            UString expectedResult = new(dotNetString);
            IEnumerable<UCodepoint> codepoints = dotNetString.Codepoints();

            Assert.That.SequenceEqual(expectedResult, codepoints);
            Assert.That.SequenceEqual(expectedResult, codepoints);
            Assert.That.SequenceEqual(expectedResult, codepoints);
        }
        #endregion

        #region DetermineDirection tests
        private static IEnumerable<object?[]> DetermineDirectionTestData => CommonTestData.DetermineDirectionTestData;

        [TestMethod]
        [DynamicData(nameof(DetermineDirectionTestData))]
        public void DetermineDirection(string? dotNetString, TextDirection expectedDirection)
        {
            Assert.AreEqual(expectedDirection, dotNetString.DetermineDirection());
        }
        #endregion
    }
}
