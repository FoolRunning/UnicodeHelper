namespace UnicodeHelper
{
    [TestClass]
    public class UStringExtensionsTests
    {
        #region GetDotNetString tests
        private static IEnumerable<object[]> GetDotNetStringIndexExceptionTestData =>
        [
            ["", -1, typeof(ArgumentOutOfRangeException)],
            ["", 1, typeof(ArgumentOutOfRangeException)],
            ["This", 5, typeof(ArgumentOutOfRangeException)],
            ["This is 😮", 10, typeof(ArgumentOutOfRangeException)],
            ["😁🤔😮", 4, typeof(ArgumentOutOfRangeException)]
        ];

        [TestMethod]
        [DynamicData(nameof(GetDotNetStringIndexExceptionTestData))]
        public void GetDotNetStringIndex_InvalidParameters(string testString, int index, 
            Type expectedExceptionType)
        {
            UString us = new(testString);
            Assert.That.ThrowsException(expectedExceptionType, () => us.GetDotNetStringIndex(index));
        }

        public static IEnumerable<object?[]> GetDotNetStringIndexTestData =>
        [
            ["", 0, 0],
            ["This is a string!", 0, 0],
            ["This is a string!", 12, 12],
            ["This is a string!", 17, 17],
            ["العربية", 3, 3],
            ["العربية", 7, 7],
            ["😁🤔😮", 0, 0],
            ["😁🤔😮", 1, 2],
            ["😁🤔😮", 2, 4],
            ["😁🤔😮", 3, 6],
            ["This is 😮", 0, 0],
            ["This is 😮", 8, 8],
            ["This is 😮", 9, 10],
        ];

        [TestMethod]
        [DynamicData(nameof(GetDotNetStringIndexTestData))]
        public void GetDotNetStringIndex(string testString, int index, int expectedIndex)
        {
            UString us = new UString(testString);
            Assert.AreEqual(expectedIndex, us.GetDotNetStringIndex(index));
        }
        #endregion
    }
}
