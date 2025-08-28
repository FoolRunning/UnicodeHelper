namespace UnicodeHelper
{
    [TestClass]
    public class UStringBuilderTests
    {
        #region Append (UCodepoint) tests
        public static IEnumerable<object?[]> AppendUCodepointTestData =>
        [
            ["  ", " ", " "],
            ["a", "a"],
            ["\U00010570\U00010597", "\U00010570", "\U00010597"], // VITHKUQI letters
            ["😁🤔😮", "😁", "🤔", "😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(AppendUCodepointTestData))]
        public void Append_UCodepoint(string expectedResult, params string[] parts)
        {
            UStringBuilder usb = new UStringBuilder();
            foreach (string part in parts)
                usb.Append(UCodepoint.ReadFromStr(part, 0));

            Assert.AreEqual(new UString(expectedResult), usb.ToUString());
        }
        #endregion

        #region Append (UString) tests
        public static IEnumerable<object?[]> AppendUStringTestData =>
        [
            ["", null],
            ["", ""],
            ["This is a test!", "This", " is a ", "test!"],
            ["😁🤔😮", "😁", "🤔😮"],
            ["This is a lot of text that will require a larger capacity. " +
             "The capacity should increase multiple times to accomodate this long string!" +
             "This is the end!", 
                "This is a lot of text that will require a larger capacity. " +
                    "The capacity should increase multiple times to accomodate this long string!", 
                "This is the end!"]
        ];

        [TestMethod]
        [DynamicData(nameof(AppendUStringTestData))]
        public void Append_UString(string expectedResult, params string?[] parts)
        {
            UStringBuilder usb = new UStringBuilder();
            foreach (string? part in parts)
                usb.Append(part != null ? new UString(part) : null);

            Assert.AreEqual(new UString(expectedResult), usb.ToUString());
        }
        #endregion

        #region Append (.Net string) tests
        public static IEnumerable<object?[]> AppendStringTestData =>
        [
            ["", null],
            ["", ""],
            ["This is a test!", "This", " is a ", "test!"],
            ["😁🤔😮", "😁", "🤔😮"],
            ["This is a lot of text that will require a larger capacity. " +
             "The capacity should increase multiple times to accomodate this long string!" +
             "This is the end!", 
                "This is a lot of text that will require a larger capacity. " +
                "The capacity should increase multiple times to accomodate this long string!", 
                "This is the end!"]
        ];

        [TestMethod]
        [DynamicData(nameof(AppendStringTestData))]
        public void Append_String(string expectedResult, params string?[] parts)
        {
            UStringBuilder usb = new UStringBuilder();
            foreach (string? part in parts)
                usb.Append(part);

            Assert.AreEqual(new UString(expectedResult), usb.ToUString());
        }
        #endregion
    }
}
