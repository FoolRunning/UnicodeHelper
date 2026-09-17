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
            using UStringBuilder usb = new UStringBuilder();
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
            using UStringBuilder usb = new UStringBuilder();
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
            using UStringBuilder usb = new UStringBuilder();
            foreach (string? part in parts)
                usb.Append(part);

            Assert.AreEqual(new UString(expectedResult), usb.ToUString());
        }
        #endregion

        #region Append (UCodepoint[], int length) tests
        public static IEnumerable<object?[]> AppendArrayTestData =>
        [
            // (source array, prefix already in builder, count to append, expected result)
            ["abcde", "", 3, "abc"],
            ["abcde", "xy", 2, "xyab"],
            ["abcde", "xy", 0, "xy"],
            ["abcde", "", 5, "abcde"],
            ["😁🤔😮", "", 3, "😁🤔😮"],
            ["😁🤔😮", "a", 2, "a😁🤔"],
            ["abcdefghijklmnopqrstuvwxyz", "Start: ", 26, "Start: abcdefghijklmnopqrstuvwxyz"],
        ];

        [TestMethod]
        [DynamicData(nameof(AppendArrayTestData))]
        public void Append_UCodepointArray(string source, string prefix, int length, string expectedResult)
        {
            UCodepoint[] all = new UString(source).ToCodepointArray();

            using UStringBuilder usb = new UStringBuilder();
            usb.Append(prefix);
            usb.Append(all, length);

            Assert.AreEqual(new UString(expectedResult), usb.ToUString());
        }

        [TestMethod]
        public void Append_UCodepointArray_NullThrows()
        {
            using UStringBuilder usb = new UStringBuilder();
            Assert.That.ThrowsException(typeof(ArgumentNullException),
                () => usb.Append(null, 0));
        }

        public static IEnumerable<object?[]> AppendArrayLengthExceptionTestData =>
        [
            [(UCodepoint[]?)null, 0, typeof(ArgumentNullException)],
            [new UCodepoint[] { 'a', 'b', 'c' }, -1, typeof(ArgumentOutOfRangeException)],
            [new UCodepoint[] { 'a', 'b', 'c' }, 4, typeof(ArgumentOutOfRangeException)],
        ];

        [TestMethod]
        [DynamicData(nameof(AppendArrayLengthExceptionTestData))]
        public void Append_UCodepointArray_InvalidLengthThrows(UCodepoint[]? array, int length, Type expectedExceptionType)
        {
            using UStringBuilder usb = new UStringBuilder();
            Assert.That.ThrowsException(expectedExceptionType, () => usb.Append(array, length));
        }
        #endregion

        #region Length property tests
        [TestMethod]
        public void Length_GetsCurrentCount()
        {
            using UStringBuilder usb = new UStringBuilder();
            Assert.AreEqual(0, usb.Length);
            usb.Append("abc");
            Assert.AreEqual(3, usb.Length);
        }

        public static IEnumerable<object?[]> LengthSetterTestData =>
        [
            // (initial content, new length, expected length)
            ["abcde", 3, 3],
            ["abc", 5, 5],
            ["abcde", 5, 5],
            ["abcde", 0, 0],
            ["", 4, 4],
        ];

        [TestMethod]
        [DynamicData(nameof(LengthSetterTestData))]
        public void Length_SetResizesLength(string content, int newLength, int expectedLength)
        {
            using UStringBuilder usb = new UStringBuilder();
            usb.Append(content);

            usb.Length = newLength;

            Assert.AreEqual(expectedLength, usb.Length);
        }

        [TestMethod]
        public void Length_IncreasePadsWithNull()
        {
            using UStringBuilder usb = new UStringBuilder("ab");
            usb.Length = 4;

            Assert.AreEqual(4, usb.Length);
            Assert.AreEqual((UCodepoint)'a', usb[0]);
            Assert.AreEqual((UCodepoint)'b', usb[1]);
            Assert.AreEqual(UCodepoint.Null, usb[2]);
            Assert.AreEqual(UCodepoint.Null, usb[3]);
        }

        [TestMethod]
        public void Length_SetNegativeThrows()
        {
            using UStringBuilder usb = new UStringBuilder();
            Assert.That.ThrowsException(typeof(ArgumentOutOfRangeException), () => usb.Length = -1);
        }
        #endregion

        #region Indexer tests
        [TestMethod]
        public void Indexer_GetReturnsCodepoint()
        {
            using UStringBuilder usb = new UStringBuilder("a😁");
            Assert.AreEqual((UCodepoint)'a', usb[0]);
            Assert.AreEqual(UCodepoint.ReadFromStr("😁", 0), usb[1]);
        }

        [TestMethod]
        public void Indexer_SetReplacesCodepoint()
        {
            using UStringBuilder usb = new UStringBuilder("abc");
            usb[1] = (UCodepoint)'X';
            Assert.AreEqual(new UString("aXc"), usb.ToUString());
        }

        [TestMethod]
        public void Indexer_SetUpperPlaneCodepoint()
        {
            using UStringBuilder usb = new UStringBuilder("a b");
            usb[1] = UCodepoint.ReadFromStr("😁", 0);
            Assert.AreEqual(new UString("a😁b"), usb.ToUString());
        }

        public static IEnumerable<object?[]> IndexerOutOfRangeTestData =>
        [
            [0, -1],
            [0, 1],
            [2, -1],
            [2, 2],
        ];

        [TestMethod]
        [DynamicData(nameof(IndexerOutOfRangeTestData))]
        public void Indexer_InvalidIndexThrows(int length, int invalidIndex)
        {
            using UStringBuilder usb = new UStringBuilder(new string('x', length));

            Assert.That.ThrowsException(typeof(IndexOutOfRangeException), () => _ = usb[invalidIndex]);
            Assert.That.ThrowsException(typeof(IndexOutOfRangeException), () => usb[invalidIndex] = (UCodepoint)0);
        }
        #endregion
    }
}
