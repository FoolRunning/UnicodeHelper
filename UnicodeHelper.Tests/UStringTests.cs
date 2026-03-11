using System.Text;
using UnicodeHelper.TestData;

// ReSharper disable ObjectCreationAsStatement
namespace UnicodeHelper
{
    [TestClass]
    public class UStringTests
    {
        #region Construction tests
        private static IEnumerable<object[]> ConstructorTestData =>
        [
            [""],
            [
                "This is\r\na test!", 
                "T", "h", "i", "s", " ", "i", "s", "\r", "\n", "a", " ", "t", "e", "s", "t", "!"
            ],
            ["العربية", "ا", "ل", "ع", "ر", "ب", "ي", "ة"],
            ["😁🤔😮", "😁", "🤔", "😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConstructorTestData))]
        public void Constructor(string testString, params string[] expectedCodepoints)
        {
            UString us = new(testString);
            Assert.AreEqual(testString, us.ToString());
            Assert.AreEqual(expectedCodepoints.Length, us.Length);

            for (int i = 0; i < expectedCodepoints.Length; i++)
            {
                Assert.AreEqual(UCodepoint.ReadFromStr(expectedCodepoints[i], 0), us[i],
                    "Characters differ at index " + i);
            }
        }

        private static IEnumerable<object[]> ConstructorCodepointListExceptionTestData =>
        [
            [new List<UCodepoint>(), -1, 0, typeof(ArgumentOutOfRangeException)],
            [new List<UCodepoint>(), 0, -1, typeof(ArgumentOutOfRangeException)],
            [new List<UCodepoint>(), 0, 1, typeof(ArgumentException)],
            [new List<UCodepoint> { 'a', 'b' }, 1, 2, typeof(ArgumentException)],
        ];

        [TestMethod]
        [DynamicData(nameof(ConstructorCodepointListExceptionTestData))]
        public void Constructor_CodepointList_InvalidParameters(List<UCodepoint> codepoints,
            int startIndex, int count, Type expectedExceptionType)
        {
            Assert.That.ThrowsException(expectedExceptionType,
                () => new UString(codepoints, startIndex, count));
        }

        [TestMethod]
        public void Constructor_CodepointList_NullThrows()
        {
            Assert.That.ThrowsException(typeof(ArgumentNullException),
                () => new UString((IReadOnlyList<UCodepoint>?)null, 0, 0));
        }

        private static IEnumerable<object[]> ConstructorCodepointListTestData =>
        [
            [""],
            ["This is\r\na test!", "T", "h", "i", "s", " ", "i", "s", "\r", "\n", "a", " ", "t", "e", "s", "t", "!"],
            ["العربية", "ا", "ل", "ع", "ر", "ب", "ي", "ة"],
            ["😁🤔😮", "😁", "🤔", "😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConstructorCodepointListTestData))]
        public void Constructor_CodepointList(string testString, params string[] expectedCodepoints)
        {
            UString temp = new(testString);
            List<UCodepoint> codepoints = temp.ToList();

            UString us = new(codepoints);
            Assert.AreEqual(testString, us.ToString());
            Assert.AreEqual(expectedCodepoints.Length, us.Length);
            for (int i = 0; i < expectedCodepoints.Length; i++)
                Assert.AreEqual(UCodepoint.ReadFromStr(expectedCodepoints[i], 0), us[i], "Characters differ at index " + i);
        }

        private static IEnumerable<object[]> ConstructorCodepointListWithRangeTestData =>
        [
            ["This is a test!", 5, 2, "is"],
            ["العربية", 2, 3, "عرب"],
            ["😁🤔😮", 1, 2, "🤔😮"],
            ["Hello", 0, 5, "Hello"],
            ["Hello", 2, 0, ""],
        ];

        [TestMethod]
        [DynamicData(nameof(ConstructorCodepointListWithRangeTestData))]
        public void Constructor_CodepointList_WithRange(string testString, int startIndex, int count, string expectedResult)
        {
            UString temp = new(testString);
            List<UCodepoint> codepoints = temp.ToList();

            UString us = new(codepoints, startIndex, count);
            Assert.AreEqual(new UString(expectedResult), us);
        }
        #endregion

        #region CharLength tests
        private static IEnumerable<object[]> CharLengthTestData =>
        [
            ["test", 4],
            ["", 0],
            ["This is\r\na test!", 16],
            ["العربية", 7],
            ["😁🤔😮", 6],
            ["test😮", 6],
        ];

        [TestMethod]
        [DynamicData(nameof(CharLengthTestData))]
        public void CharLength(string str, int expectedResult)
        {
            UString us = new(str);
            Assert.AreEqual(expectedResult, us.CharLength);

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(str);
            Assert.AreEqual(expectedResult, us.CharLength);
        }
        #endregion

        #region Equals tests
        private static IEnumerable<object[]> EqualsTestData =>
        [
            ["test", "", false],
            ["", "test", false],
            ["", "", true],
            ["test", "test", true],
            ["This is\r\na test!", "This is\r\na test!", true],
            ["This is\r\na test!", "This is a test!", false],
            ["العربية", "لعربية", false],
            ["العربية", "العربية", true],
            ["😁🤔😮", "😁", false],
            ["😁🤔😮", "😁🤔😮", true]
        ];

        [TestMethod]
        [DynamicData(nameof(EqualsTestData))]
        public void Equals(string string1, string string2, bool expectedResult)
        {
            // Test making sure that a substring results in the correct result
            UString us1 = CreateTestSubstring(string1);
            UString us2 = new(string2);

            Assert.AreEqual(expectedResult, us1.Equals(us2), "Equal method failed");
            Assert.AreEqual(expectedResult, us1 == us2, "Equal operator failed");
            Assert.AreEqual(!expectedResult, us1 != us2, "NotEqual operator failed");

            // Test making sure that a substring results in the correct result
            us1 = new(string1);
            us2 = CreateTestSubstring(string2);

            Assert.AreEqual(expectedResult, us1.Equals(us2), "Equal method failed");
            Assert.AreEqual(expectedResult, us1 == us2, "Equal operator failed");
            Assert.AreEqual(!expectedResult, us1 != us2, "NotEqual operator failed");
        }
        #endregion
        
        #region SubString tests
        private static IEnumerable<object[]> SubStringExceptionTestData =>
        [
            ["", 0, 1, typeof(ArgumentException)],
            ["", 1, 0, typeof(ArgumentException)],
            ["This", -1, 2, typeof(ArgumentOutOfRangeException)],
            ["This", 0, 5, typeof(ArgumentException)],
            ["This", 0, -1, typeof(ArgumentOutOfRangeException)],
            ["This", 4, 1, typeof(ArgumentException)],
            ["😁🤔😮", 1, 3, typeof(ArgumentException)],
            ["😁🤔😮", 3, 1, typeof(ArgumentException)]
        ];

        [TestMethod]
        [DynamicData(nameof(SubStringExceptionTestData))]
        public void SubString_InvalidParameters(string testString, int start, int length, 
            Type expectedExceptionType)
        {
            UString us = new(testString);
            Assert.That.ThrowsException(expectedExceptionType, () => us.SubString(start, length));
        }
        
        private static IEnumerable<object[]> SubStringTestData =>
        [
            ["", 0, 0, ""],
            ["This is\r\na test!", 2, 5, "is is"],
            ["This is a test!", 0, 0, ""],
            ["This is a test!", 2, 0, ""],
            ["This is a test!", 15, 0, ""],
            ["العربية", 3, 2, "رب"],
            ["😁🤔😮", 1, 1, "🤔"]
        ];

        [TestMethod]
        [DynamicData(nameof(SubStringTestData))]
        public void SubString(string testString, int start, int length, string expectedResult)
        {
            UString us = new(testString);
            UString expectedUs = new(expectedResult);
            
            Assert.AreEqual(expectedUs, us.SubString(start, length));
        }

        private static IEnumerable<object[]> SubStringSubStringTestData =>
        [
            ["This is\r\na test!", 2, 5, 1, 3, "s i"],
            ["This is\r\na test!", 5, 4, 2, 2, "\r\n"],
            ["This", 1, 2, 1, 0, ""],
            ["This", 0, 4, 0, 4, "This"],
            ["العربية", 3, 2, 0, 1, "ر"],
            ["😁🤔😮", 1, 2, 0, 2, "🤔😮"],
            ["😁🤔😮", 1, 2, 1, 1, "😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(SubStringSubStringTestData))]
        public void SubString_SubStringSubString(string testString, 
            int firstStart, int firstLength, int secondStart, int secondLength,
            string expectedResult)
        {
            UString us = new(testString);
            UString subUs1 = us.SubString(firstStart, firstLength);
            
            Assert.AreEqual(new UString(expectedResult), subUs1.SubString(secondStart, secondLength));
        }
        #endregion

        #region ToUpperInvariant tests
        private static IEnumerable<object[]> ToUpperInvariantTestData =>
        [
            ["", ""],
            ["cat", "CAT"],
            ["CaT", "CAT"],
            ["CAT", "CAT"],
            ["\U00010570\U00010597", "\U00010570\U00010570"] // VITHKUQI letters
        ];

        [TestMethod]
        [DynamicData(nameof(ToUpperInvariantTestData))]
        public void ToUpperInvariant(string testString, string expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToUpperInvariant());

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToUpperInvariant());
        }
        #endregion

        #region ToLowerInvariant tests
        private static IEnumerable<object[]> ToLowerInvariantTestData =>
        [
            ["", ""],
            ["cat", "cat"],
            ["CaT", "cat"],
            ["CAT", "cat"],
            ["\U00010570\U00010597", "\U00010597\U00010597"] // VITHKUQI letters
        ];

        [TestMethod]
        [DynamicData(nameof(ToLowerInvariantTestData))]
        public void ToLowerInvariant(string testString, string expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToLowerInvariant());

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToLowerInvariant());
        }
        #endregion

        #region ToCodepointArray tests
        private static IEnumerable<object[]> ToCodepointArrayTestData =>
        [
            [""],
            ["This is\r\na test!", "T", "h", "i", "s", " ", "i", "s", "\r", "\n", "a", " ", "t", "e", "s", "t", "!"],
            ["العربية", "ا", "ل", "ع", "ر", "ب", "ي", "ة"],
            ["😁🤔😮", "😁", "🤔", "😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(ToCodepointArrayTestData))]
        public void ToCodepointArray(string testString, params string[] expectedResult)
        {
            UString us = new(testString);
            UCodepoint[] expectedArray = expectedResult.Select(s => UCodepoint.ReadFromStr(s, 0)).ToArray();
            Assert.That.SequenceEqual(expectedArray, us.ToCodepointArray());

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            expectedArray = expectedResult.Select(s => UCodepoint.ReadFromStr(s, 0)).ToArray();
            Assert.That.SequenceEqual(expectedArray, us.ToCodepointArray());
        }
        #endregion

        #region Enumerator tests
        private static IEnumerable<object[]> EnumeratorTestData =>
        [
            [""],
            ["This is\r\na test!", (UCodepoint)'T', (UCodepoint)'h', (UCodepoint)'i', (UCodepoint)'s', (UCodepoint)' ', 
                (UCodepoint)'i', (UCodepoint)'s', (UCodepoint)'\r', (UCodepoint)'\n', (UCodepoint)'a', (UCodepoint)' ', 
                (UCodepoint)'t', (UCodepoint)'e', (UCodepoint)'s', (UCodepoint)'t', (UCodepoint)'!'],
            ["العربية", (UCodepoint)'ا', (UCodepoint)'ل', (UCodepoint)'ع', (UCodepoint)'ر', (UCodepoint)'ب', 
                (UCodepoint)'ي', (UCodepoint)'ة'],
            ["😁🤔😮", (UCodepoint)0x0001f601, (UCodepoint)0x0001f914, (UCodepoint)0x0001f62e]
        ];

        [TestMethod]
        [DynamicData(nameof(EnumeratorTestData))]
        public void Enumerator(string testString, params UCodepoint[] expectedResult)
        {
            UString us = new(testString);
            Assert.That.SequenceEqual(expectedResult, us);

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.That.SequenceEqual(expectedResult, us);
        }
        #endregion

        #region IndexOf_UCodepoint tests
        private static IEnumerable<object[]> UCodepointIndexOfExceptionTestData =>
        [
            ["", 0, 1, typeof(ArgumentException)],
            ["", 1, 0, typeof(ArgumentException)],
            ["This", -1, 2, typeof(ArgumentOutOfRangeException)],
            ["This", 0, 5, typeof(ArgumentException)],
            ["This", 0, -1, typeof(ArgumentOutOfRangeException)],
            ["This", 4, 1, typeof(ArgumentException)],
            ["😁🤔😮", 1, 3, typeof(ArgumentException)],
            ["😁🤔😮", 3, 1, typeof(ArgumentException)]
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointIndexOfExceptionTestData))]
        public void IndexOf_UCodepoint_InvalidParameters(string testString, int start, int count, 
            Type expectedExceptionType)
        {
            UString us = new(testString);
            Assert.That.ThrowsException(expectedExceptionType, () => us.IndexOf(' ', start, count));
        }

        private static IEnumerable<object[]> UCodepointIndexOfTestData =>
        [
            ["", (UCodepoint)'A', 0, 0, -1],
            ["This is\r\na test!", (UCodepoint)'\r', 2, 6, 7],
            ["This is\r\na test!", (UCodepoint)'i', 0, 16, 2],
            ["This is\r\na test!", (UCodepoint)'T', 2, 6, -1],
            ["This", (UCodepoint)'s', 4, 0, -1],
            ["This", (UCodepoint)'i', 2, 0, -1],
            ["العربية", (UCodepoint)'ا', 0, 7, 0],
            ["العربية", (UCodepoint)'ة', 0, 7, 6],
            ["😁🤔😮", UCodepoint.ReadFromStr("😁", 0), 0, 3, 0],
            ["😁🤔😮", UCodepoint.ReadFromStr("😮", 0), 1, 2, 2]
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointIndexOfTestData))]
        public void IndexOf_UCodepoint(string testString, UCodepoint codePoint, int start, int count, int expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.IndexOf(codePoint, start, count));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.IndexOf(codePoint, start, count));
        }
        #endregion

        #region IndexOf_UString tests
        private static IEnumerable<object[]> UStringIndexOfExceptionTestData =>
        [
            ["", "a", -1, 0, typeof(ArgumentOutOfRangeException)],
            ["", "a", 0, 1, typeof(ArgumentException)],
            ["This", "a", 0, -1, typeof(ArgumentOutOfRangeException)],
            ["This", "a", -1, 2, typeof(ArgumentOutOfRangeException)],
            ["This", "a", 4, 1, typeof(ArgumentException)],
            ["This", "a", 0, 5, typeof(ArgumentException)],
        ];

        [TestMethod]
        [DynamicData(nameof(UStringIndexOfExceptionTestData))]
        public void IndexOf_UString_InvalidParameters(string testString, string value, int startIndex, int count,
            Type expectedExceptionType)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.That.ThrowsException(expectedExceptionType, () => us.IndexOf(searchValue, startIndex, count));
        }

        private static IEnumerable<object[]> UStringIndexOfTestData =>
        [
            ["This is a test!", "is", 0, 15, 2],
            ["This is a test!", "is", 3, 12, 5],
            ["This is a test!", "test", 0, 15, 10],
            ["This is a test!", "xyz", 0, 15, -1],
            ["This is a test!", "This is a test!", 0, 15, 0],
            ["This is a test!", "is", 6, 9, -1],
            ["العربية", "رب", 0, 7, 3],
            ["😁🤔😮", "🤔😮", 0, 3, 1],
            ["😁🤔😮", "😁", 1, 2, -1],
        ];

        [TestMethod]
        [DynamicData(nameof(UStringIndexOfTestData))]
        public void IndexOf_UString(string testString, string value, int startIndex, int count, int expectedResult)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.AreEqual(expectedResult, us.IndexOf(searchValue, startIndex, count));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.IndexOf(searchValue, startIndex, count));
        }
        #endregion

        #region LastIndexOf_UCodepoint tests
        private static IEnumerable<object[]> UCodepointLastIndexOfExceptionTestData =>
        [
            ["", 0, 0, typeof(ArgumentOutOfRangeException)],
            ["", 0, 1, typeof(ArgumentOutOfRangeException)],
            ["", 1, 0, typeof(ArgumentOutOfRangeException)],
            ["This", -1, 2, typeof(ArgumentOutOfRangeException)],
            ["This", 3, 5, typeof(ArgumentException)],
            ["This", 3, -1, typeof(ArgumentOutOfRangeException)],
            ["This", -1, 1, typeof(ArgumentOutOfRangeException)],
            ["😁🤔😮", 1, 3, typeof(ArgumentException)],
            ["😁🤔😮", 3, 1, typeof(ArgumentOutOfRangeException)]
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointLastIndexOfExceptionTestData))]
        public void LastIndexOf_UCodepoint_InvalidParameters(string testString, int start, int count, 
            Type expectedExceptionType)
        {
            UString us = new(testString);
            Assert.That.ThrowsException(expectedExceptionType, () => us.LastIndexOf(' ', start, count));
        }

        private static IEnumerable<object[]> UCodepointLastIndexOfTestData =>
        [
            ["This is\r\na test!", (UCodepoint)'\r', 8, 6, 7],
            ["This is\r\na test!", (UCodepoint)'i', 15, 16, 5],
            ["This is\r\na test!", (UCodepoint)'T', 8, 6, -1],
            ["This", (UCodepoint)'T', 0, 0, -1],
            ["This", (UCodepoint)'s', 3, 0, -1],
            ["This", (UCodepoint)'i', 2, 0, -1],
            ["العربية", (UCodepoint)'ا', 6, 7, 0],
            ["العربية", (UCodepoint)'ة', 6, 7, 6],
            ["😁🤔😮", UCodepoint.ReadFromStr("😮", 0), 2, 3, 2],
            ["😁🤔😮", UCodepoint.ReadFromStr("😁", 0), 1, 2, 0]
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointLastIndexOfTestData))]
        public void LastIndexOf_UCodepoint(string testString, UCodepoint codePoint, 
            int start, int count, int expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.LastIndexOf(codePoint, start, count));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.LastIndexOf(codePoint, start, count));
        }
        #endregion

        #region LastIndexOf_UString tests
        private static IEnumerable<object[]> UStringLastIndexOfExceptionTestData =>
        [
            ["", "a", 0, 0, typeof(ArgumentOutOfRangeException)],
            ["This", "a", -1, 2, typeof(ArgumentOutOfRangeException)],
            ["This", "a", 4, 1, typeof(ArgumentOutOfRangeException)],
            ["This", "a", 3, -1, typeof(ArgumentOutOfRangeException)],
            ["This", "a", 3, 5, typeof(ArgumentException)],
        ];

        [TestMethod]
        [DynamicData(nameof(UStringLastIndexOfExceptionTestData))]
        public void LastIndexOf_UString_InvalidParameters(string testString, string value, int startIndex, int count,
            Type expectedExceptionType)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.That.ThrowsException(expectedExceptionType, () => us.LastIndexOf(searchValue, startIndex, count));
        }

        private static IEnumerable<object[]> UStringLastIndexOfTestData =>
        [
            ["This is a test!", "is", 14, 15, 5],
            ["This is a test!", "is", 4, 5, 2],
            ["This is a test!", "xyz", 14, 15, -1],
            ["This is a test!", "This", 14, 15, 0],
            ["العربية", "رب", 6, 7, 3],
            ["😁🤔😮", "🤔", 2, 3, 1],
            ["😁🤔😮", "😮", 1, 2, -1],
        ];

        [TestMethod]
        [DynamicData(nameof(UStringLastIndexOfTestData))]
        public void LastIndexOf_UString(string testString, string value, int startIndex, int count, int expectedResult)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.AreEqual(expectedResult, us.LastIndexOf(searchValue, startIndex, count));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.LastIndexOf(searchValue, startIndex, count));
        }
        #endregion

        #region StartsWith_UCodepoint tests
        private static IEnumerable<object[]> UCodepointStartsWithTestData =>
        [
            ["", (UCodepoint)'A', true, false],
            ["", (UCodepoint)'A', false, false],
            ["This test!", (UCodepoint)'h', false, false],
            ["This test!", (UCodepoint)'T', false, true],
            ["This test!", (UCodepoint)'T', true, true],
            ["This test!", (UCodepoint)'t', false, false],
            ["This test!", (UCodepoint)'t', true, true],
            ["العربية", (UCodepoint)'ا', false, true],
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010570", 0), false, true], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010570", 0), true, true], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010597", 0), false, false], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010597", 0), true, true], // VITHKUQI letters
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointStartsWithTestData))]
        public void StartsWith_UCodepoint(string testString, UCodepoint codePoint, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.StartsWith(codePoint, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.StartsWith(codePoint, ignoreCase));
        }
        #endregion

        #region StartsWith_UString tests
        private static IEnumerable<object[]> UStringStartsWithTestData =>
        [
            ["", "a", false, false],
            ["", "", false, true],
            ["This is a test!", "This", false, true],
            ["This is a test!", "this", false, false],
            ["This is a test!", "this", true, true],
            ["This is a test!", "test!", false, false],
            ["This is a test!", "This is a test!", false, true],
            ["العربية", "العر", false, true],
            ["العربية", "ية", false, false],
            ["😁🤔😮", "😁🤔", false, true],
            ["😁🤔😮", "🤔😮", false, false],
            ["\U00010570\U00010597", "\U00010570", false, true],   // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010597", false, false],  // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010597", true, true],    // VITHKUQI case-insensitive
        ];

        [TestMethod]
        [DynamicData(nameof(UStringStartsWithTestData))]
        public void StartsWith_UString(string testString, string value, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.AreEqual(expectedResult, us.StartsWith(searchValue, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.StartsWith(searchValue, ignoreCase));
        }
        #endregion

        #region EndsWith_UCodepoint tests
        private static IEnumerable<object[]> UCodepointEndsWithTestData =>
        [
            ["", (UCodepoint)'A', true, false],
            ["", (UCodepoint)'A', false, false],
            ["This test", (UCodepoint)'s', false, false],
            ["This test", (UCodepoint)'t', false, true],
            ["This test", (UCodepoint)'t', true, true],
            ["This test", (UCodepoint)'T', false, false],
            ["This test", (UCodepoint)'T', true, true],
            ["العربية", (UCodepoint)'ة', false, true],
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010597", 0), false, true], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010597", 0), true, true], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010570", 0), false, false], // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010570", 0), true, true], // VITHKUQI letters
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointEndsWithTestData))]
        public void EndsWith_UCodepoint(string testString, UCodepoint codePoint, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.EndsWith(codePoint, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.EndsWith(codePoint, ignoreCase));
        }
        #endregion

        #region EndsWith_UString tests
        private static IEnumerable<object[]> UStringEndsWithTestData =>
        [
            ["", "a", false, false],
            ["", "", false, true],
            ["This is a test!", "test!", false, true],
            ["This is a test!", "TEST!", false, false],
            ["This is a test!", "TEST!", true, true],
            ["This is a test!", "This", false, false],
            ["This is a test!", "This is a test!", false, true],
            ["العربية", "بية", false, true],
            ["العربية", "العر", false, false],
            ["😁🤔😮", "🤔😮", false, true],
            ["😁🤔😮", "😁🤔", false, false],
            ["\U00010570\U00010597", "\U00010597", false, true],   // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010570", false, false],  // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010570", true, true],    // VITHKUQI case-insensitive
        ];

        [TestMethod]
        [DynamicData(nameof(UStringEndsWithTestData))]
        public void EndsWith_UString(string testString, string value, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.AreEqual(expectedResult, us.EndsWith(searchValue, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.EndsWith(searchValue, ignoreCase));
        }
        #endregion

        #region Contains_UCodepoint tests
        private static IEnumerable<object[]> UCodepointContainsTestData =>
        [
            ["", (UCodepoint)'a', false, false],
            ["This is a test!", (UCodepoint)'i', false, true],
            ["This is a test!", (UCodepoint)'z', false, false],
            ["This is a test!", (UCodepoint)'T', false, true],
            ["this is a test!", (UCodepoint)'T', false, false],
            ["this is a test!", (UCodepoint)'T', true, true],
            ["This is a test!", (UCodepoint)'t', true, true],
            ["العربية", (UCodepoint)'ر', false, true],
            ["العربية", (UCodepoint)'ص', false, false],
            ["😁🤔😮", UCodepoint.ReadFromStr("🤔", 0), false, true],
            ["😁🤔😮", UCodepoint.ReadFromStr("🎉", 0), false, false],
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010570", 0), false, true],  // VITHKUQI letters
            ["\U00010570\U00010597", UCodepoint.ReadFromStr("\U00010597", 0), true, true],   // VITHKUQI case-insensitive
        ];

        [TestMethod]
        [DynamicData(nameof(UCodepointContainsTestData))]
        public void Contains_UCodepoint(string testString, UCodepoint codePoint, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.Contains(codePoint, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.Contains(codePoint, ignoreCase));
        }
        #endregion

        #region Contains_UString tests
        private static IEnumerable<object[]> UStringContainsTestData =>
        [
            ["", "a", false, false],
            ["", "", false, true],
            ["This is a test!", "is", false, true],
            ["This is a test!", "xyz", false, false],
            ["This is a test!", "IS", false, false],
            ["This is a test!", "IS", true, true],
            ["This is a test!", "This is a test!", false, true],
            ["العربية", "رب", false, true],
            ["العربية", "صف", false, false],
            ["😁🤔😮", "🤔😮", false, true],
            ["😁🤔😮", "😮😁", false, false],
            ["\U00010570\U00010597", "\U00010597", false, true],            // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010570\U00010570", false, false], // VITHKUQI letters
            ["\U00010570\U00010597", "\U00010570\U00010570", true, true],   // VITHKUQI case-insensitive
        ];

        [TestMethod]
        [DynamicData(nameof(UStringContainsTestData))]
        public void Contains_UString(string testString, string value, bool ignoreCase, bool expectedResult)
        {
            UString us = new(testString);
            UString searchValue = new(value);
            Assert.AreEqual(expectedResult, us.Contains(searchValue, ignoreCase));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.Contains(searchValue, ignoreCase));
        }
        #endregion

        #region IsNullOrEmpty tests
        private static IEnumerable<object?[]> IsNullOrEmptyTestData =>
        [
            [null, true],
            ["", true],
            [" ", false],
            ["\r\n", false],
            ["a", false],
            ["العربية", false],
            ["😁🤔😮", false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsNullOrEmptyTestData))]
        public void IsNullOrEmpty(string? str, bool expectedResult)
        {
            UString? us = str != null ? new UString(str) : null;
            Assert.AreEqual(expectedResult, UString.IsNullOrEmpty(us));
        }
        #endregion

        #region IsNullOrWhitespace tests
        private static IEnumerable<object?[]> IsNullOrWhitespaceTestData =>
        [
            [null, true],
            ["", true],
            [" ", true],
            ["\r\n", true],
            ["\u00A0\u2002\u202F\u3000", true],
            ["a", false],
            ["العربية", false],
            ["😁🤔😮", false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsNullOrWhitespaceTestData))]
        public void IsNullOrWhitespace(string? str, bool expectedResult)
        {
            UString? us = str != null ? new UString(str) : null;
            Assert.AreEqual(expectedResult, UString.IsNullOrWhitespace(us));
        }
        #endregion

        #region Concat tests
        private static IEnumerable<object?[]> ConcatStringPlusCodepointTestData =>
        [
            [null, (UCodepoint)' ', " "],
            ["", (UCodepoint)' ', " "],
            ["This is\r\na test!", (UCodepoint)'ع', "This is\r\na test!ع"],
            ["العربي", (UCodepoint)'ة', "العربية"],
            ["😁🤔", UCodepoint.ReadFromStr("😮", 0), "😁🤔😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatStringPlusCodepointTestData))]
        public void Concat_StringPlusCodepoint(string? testString, UCodepoint toConcat, string expectedResults)
        {
            UString? us = testString != null ? new UString(testString) : null;
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(us, toConcat), expectedUs);
            Assert.AreEqual(us + toConcat, expectedUs);

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(UString.Concat(us, toConcat), expectedUs);
            Assert.AreEqual(us + toConcat, expectedUs);
        }

        private static IEnumerable<object?[]> ConcatCodepointPlusStringTestData =>
        [
            [null, (UCodepoint)' ', " "],
            ["", (UCodepoint)' ', " "],
            ["This is\r\na test!", (UCodepoint)'ع', "عThis is\r\na test!"],
            ["العربي", (UCodepoint)'ة', "ةالعربي"],
            ["😁🤔", UCodepoint.ReadFromStr("😮", 0), "😮😁🤔"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatCodepointPlusStringTestData))]
        public void Concat_CodepointPlusString(string? testString, UCodepoint toConcat, string expectedResults)
        {
            UString? us = testString != null ? new UString(testString) : null;
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(toConcat, us), expectedUs);
            Assert.AreEqual(toConcat + us, expectedUs);

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(UString.Concat(toConcat, us), expectedUs);
            Assert.AreEqual(toConcat + us, expectedUs);
        }

        private static IEnumerable<object?[]> ConcatStringPlusStringTestData =>
        [
            [null, "", ""],
            ["", null, ""],
            [null, null, ""],
            ["", "", ""],
            ["", "ACB", "ACB"],
            ["This is\r", "\na test!", "This is\r\na test!"],
            ["العر", "بية", "العربية"],
            ["😁🤔", "🤔😮😁", "😁🤔🤔😮😁"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatStringPlusStringTestData))]
        public void Concat_StringPlusString(string? str1, string? str2, string expectedResults)
        {
            UString? us1 = str1 != null ? new UString(str1) : null;
            UString? us2 = str2 != null ? new UString(str2) : null;
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(us1, us2), expectedUs);
            Assert.AreEqual(us1 + us2, expectedUs);

            // Test making sure that a substring results in the correct result
            us1 = CreateTestSubstring(str1);
            us2 = CreateTestSubstring(str2);
            Assert.AreEqual(UString.Concat(us1, us2), expectedUs);
            Assert.AreEqual(us1 + us2, expectedUs);
        }

        private static IEnumerable<object?[]> ConcatStringPlusStringStringTestData =>
        [
            [null, "", "", ""],
            ["", null, "", ""],
            ["", "", null, ""],
            [null, null, null, ""],
            ["", "", "", ""],
            ["", "ACB", "", "ACB"],
            ["This ", "is\r", "\na test!", "This is\r\na test!"],
            ["العر", "ب", "ية", "العربية"],
            ["😁🤔", "🤔", "😮😁", "😁🤔🤔😮😁"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatStringPlusStringStringTestData))]
        public void Concat_StringPlusStringString(string? str1, string? str2, string? str3, string expectedResults)
        {
            UString? us1 = str1 != null ? new UString(str1) : null;
            UString? us2 = str2 != null ? new UString(str2) : null;
            UString? us3 = str3 != null ? new UString(str3) : null;
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(us1, us2, us3), expectedUs);
            Assert.AreEqual(us1 + us2 + us3, expectedUs);

            // Test making sure that a substring results in the correct result
            us1 = CreateTestSubstring(str1);
            us2 = CreateTestSubstring(str2);
            us3 = CreateTestSubstring(str3);
            Assert.AreEqual(UString.Concat(us1, us2, us3), expectedUs);
            Assert.AreEqual(us1 + us2 + us3, expectedUs);
        }

        private static IEnumerable<object?[]> ConcatStringPlusStringStringStringTestData =>
        [
            [null, "", "", "", ""],
            ["", null, "", "", ""],
            ["", "", null, "", ""],
            ["", "", "", null, ""],
            [null, null, null, null, ""],
            ["", "", "", "", ""],
            ["", "", "ACB", "", "ACB"],
            ["This ", "is\r", "\na test", "!", "This is\r\na test!"],
            ["العر", "ب", "ي", "ة", "العربية"],
            ["😁", "🤔", "🤔", "😮😁", "😁🤔🤔😮😁"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatStringPlusStringStringStringTestData))]
        public void Concat_StringPlusStringStringString(string? str1, string? str2, string? str3, string? str4, string expectedResults)
        {
            UString? us1 = str1 != null ? new UString(str1) : null;
            UString? us2 = str2 != null ? new UString(str2) : null;
            UString? us3 = str3 != null ? new UString(str3) : null;
            UString? us4 = str4 != null ? new UString(str4) : null;
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(us1, us2, us3, us4), expectedUs);
            Assert.AreEqual(us1 + us2 + us3 + us4, expectedUs);

            // Test making sure that a substring results in the correct result
            us1 = CreateTestSubstring(str1);
            us2 = CreateTestSubstring(str2);
            us3 = CreateTestSubstring(str3);
            us4 = CreateTestSubstring(str4);
            Assert.AreEqual(UString.Concat(us1, us2, us3, us4), expectedUs);
            Assert.AreEqual(us1 + us2 + us3 + us4, expectedUs);
        }

        private static IEnumerable<object[]> ConcatStringListTestData =>
        [
            [Array.Empty<string?>(), ""],
            [new string?[] { null }, ""],
            [new string?[] { null, null, null, null, null, null }, ""],
            [new [] { "", "", "", "", "", "", "", "" }, ""],
            [new [] { "A", "", "", "", "", "V", "", "" }, "AV"],
            [new [] { "Th", "is ", "is\r", "\na ", "test", "!" }, "This is\r\na test!"],
            [new [] { "العر", "ب", "ي", "ة" }, "العربية"],
            [new [] { "😁", "🤔🤔", "😮😁" }, "😁🤔🤔😮😁"]
        ];

        [TestMethod]
        [DynamicData(nameof(ConcatStringListTestData))]
        public void Concat_StringList(string?[] strings, string expectedResults)
        {
            UString expectedUs = new(expectedResults);
            Assert.AreEqual(UString.Concat(strings.Select(s => s != null ? new UString(s) : null).ToArray()), 
                expectedUs);

            // Test making sure that a substring results in the correct result
            Assert.AreEqual(UString.Concat(strings.Select(CreateTestSubstring).ToArray()), expectedUs);
        }
        #endregion
        
        #region Split tests
        private static IEnumerable<object?[]> SplitExceptionTestData =>
        [
            ["bla", null, 1, typeof(ArgumentException)],
            ["bla", Array.Empty<UCodepoint>(), 1, typeof(ArgumentException)],
            ["bla", new UCodepoint[] { ' ' }, -1, typeof(ArgumentOutOfRangeException)],
            ["bla", new UCodepoint[] { ' ' }, 0, typeof(ArgumentOutOfRangeException)],
        ];

        [TestMethod]
        [DynamicData(nameof(SplitExceptionTestData))]
        public void Split_InvalidParameters(string testString, UCodepoint[]? separators, int maxCount, 
            Type expectedExceptionType)
        {
            UString us = new(testString);
            Assert.That.ThrowsException(expectedExceptionType, () => us.Split(separators, maxCount));
        }
        
        private static IEnumerable<object[]> SplitTestData =>
        [
            ["", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "" }],
            ["", new UCodepoint[] { ' ' }, 1, new[] { "" }],
            ["This is a test!", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "This", "is", "a", "test!" }],
            ["This is a test!", new UCodepoint[] { ' ' }, 2, new[] { "This", "is a test!" }],
            [" This  is a test!  ", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "", "This", "", "is", "a", "test!", "", "" }],
            ["This is\r\na test!", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "This", "is\r\na", "test!" }],
            ["This is\r\na test!", new UCodepoint[] { ' ', '\r', '\n' }, int.MaxValue, new[] { "This", "is", "", "a", "test!" }],
            ["العربية", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "العربية" }],
            ["😁🤔😮", new[] { UCodepoint.ReadFromStr("🤔", 0)  }, int.MaxValue, new[] { "😁", "😮" }],
        ];

        [TestMethod]
        [DynamicData(nameof(SplitTestData))]
        public void Split(string testString, UCodepoint[]? separators, int maxCount, string[] expectedResults)
        {
            UString us = new(testString);
            UString[] expectedUss = expectedResults.Select(er => new UString(er)).ToArray();
            
            Assert.That.SequenceEqual(expectedUss, us.Split(separators, maxCount));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.That.SequenceEqual(expectedUss, us.Split(separators, maxCount));
        }

        private static IEnumerable<object[]> SplitIgnoreEmptyTestData =>
        [
            ["", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "" }],
            ["", new UCodepoint[] { ' ' }, 1, new[] { "" }],
            ["This is a test!", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "This", "is", "a", "test!" }],
            ["This is a test!", new UCodepoint[] { ' ' }, 2, new[] { "This", "is a test!" }],
            [" This  is a test!  ", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "This", "is", "a", "test!" }],
            ["This is\r\na test!", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "This", "is\r\na", "test!" }],
            ["This is\r\na test!", new UCodepoint[] { ' ', '\r', '\n' }, int.MaxValue, new[] { "This", "is", "a", "test!" }],
            ["العربية", new UCodepoint[] { ' ' }, int.MaxValue, new[] { "العربية" }],
            ["😁🤔😮", new[] { UCodepoint.ReadFromStr("🤔", 0)  }, int.MaxValue, new[] { "😁", "😮" }],
        ];

        [TestMethod]
        [DynamicData(nameof(SplitIgnoreEmptyTestData))]
        public void Split_IgnoreEmpty(string testString, UCodepoint[]? separators, int maxCount, string[] expectedResults)
        {
            UString us = new(testString);
            UString[] expectedUss = expectedResults.Select(er => new UString(er)).ToArray();
            
            Assert.That.SequenceEqual(expectedUss, us.Split(separators, maxCount, StringSplitOptions.RemoveEmptyEntries));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.That.SequenceEqual(expectedUss, us.Split(separators, maxCount, StringSplitOptions.RemoveEmptyEntries));
        }
        #endregion
        
        #region ToString tests
        [TestMethod]
        [DynamicData(nameof(SubStringTestData))]
        public void ToString_SubString(string testString, int start, int length, string expectedResult)
        {
            UString us = new(testString);
            Assert.AreEqual(expectedResult, us.ToString(start, length));

            // Test making sure that a substring results in the correct result
            us = CreateTestSubstring(testString);
            Assert.AreEqual(expectedResult, us.ToString(start, length));
        }
        #endregion

        #region DetermineDirection tests
        private static IEnumerable<object?[]> DetermineDirectionTestData => CommonTestData.DetermineDirectionTestData;

        [TestMethod]
        [DynamicData(nameof(DetermineDirectionTestData))]
        public void DetermineDirection(string? testStr, TextDirection expectedDirection)
        {
            if (testStr == null)
                return;
            UString us = new(testStr);
            Assert.AreEqual(expectedDirection, us.DetermineDirection());
        }
        #endregion

        #region CompareTo tests
        private static IEnumerable<object[]> CompareToTestData =>
        [
            ["", "", 0],
            ["a", "a", 0],
            ["a", "b", -1],
            ["b", "a", 1],
            ["ab", "abc", -1],
            ["abc", "ab", 1],
            ["abc", "ABC", 1]
        ];

        [TestMethod]
        [DynamicData(nameof(CompareToTestData))]
        public void CompareTo(string string1, string string2, int expectedResult)
        {
            UString us1 = new(string1);
            UString us2 = new(string2);

            switch (expectedResult)
            {
                case > 0:
                    Assert.IsGreaterThan(0, us1.CompareTo(us2));
                    Assert.IsGreaterThan(0, ((IComparable)us1).CompareTo(us2));
                    break;
                case < 0:
                    Assert.IsLessThan(0, us1.CompareTo(us2));
                    Assert.IsLessThan(0, ((IComparable)us1).CompareTo(us2));
                    break;
                default:
                    Assert.AreEqual(0, us1.CompareTo(us2));
                    Assert.AreEqual(0, ((IComparable)us1).CompareTo(us2));
                    break;
            }
        }
        #endregion

        #region Clone tests
        [TestMethod]
        public void Clone()
        {
            UString original = new("test");
            object cloned = original.Clone();
            
            Assert.IsInstanceOfType(cloned, typeof(UString));
            Assert.AreEqual(original, cloned);
            Assert.AreNotSame(original, cloned); // Should be different objects
        }

        [TestMethod]
        public void Clone_Substring()
        {
            UString original = CreateTestSubstring("test");
            object cloned = original.Clone();
            
            Assert.IsInstanceOfType(cloned, typeof(UString));
            Assert.AreEqual(original, cloned);
            Assert.AreNotSame(original, cloned); // Should be different objects

            UString expected = new("test");
            Assert.AreEqual(expected, cloned);
        }
        #endregion

        #region GetHashCode tests
        private static IEnumerable<object[]> GetHashCodeTestData =>
        [
            [""],
            ["a"],
            ["ab"],
            ["Hello"],
            ["😁🤔😮"]
        ];

        [TestMethod]
        [DynamicData(nameof(GetHashCodeTestData))]
        public void GetHashCode(string testString)
        {
            UString us1 = new(testString);
            
            // Test that GetHash code for same content is consistent
            int hash1 = us1.GetHashCode();
            int hash2 = us1.GetHashCode();
            Assert.AreEqual(hash1, hash2); // Same instance should always give same hash

            UString us2 = new(testString);
            Assert.AreEqual(hash1, us2.GetHashCode()); // Same contents should always give same hash

            // For different strings with same content (substring vs full), the hash should be same
            UString sub = CreateTestSubstring(testString);
            Assert.AreEqual(us1.GetHashCode(), sub.GetHashCode());
        }
        #endregion

        #region Normalization tests
        [TestMethod]
        public void Normalization_FormC()
        {
            foreach (NormalizationTestData testData in NormalizationTestDataSet.TestCases)
            {
                UString result = testData.Source.Normalize(NormalizationForm.FormC);
                Assert.AreEqual(testData.NfcResult, result, 
                    ErrorString(testData.NfcResult, result, testData.Description));
            }
        }
        
        [TestMethod]
        public void Normalization_FormD()
        {
            foreach (NormalizationTestData testData in NormalizationTestDataSet.TestCases)
            {
                UString result = testData.Source.Normalize(NormalizationForm.FormD);
                Assert.AreEqual(testData.NfdResult, result, 
                    ErrorString(testData.NfdResult, result, testData.Description));
            }
        }
        
        [TestMethod]
        public void Normalization_FormKD()
        {
            foreach (NormalizationTestData testData in NormalizationTestDataSet.TestCases)
            {
                UString result = testData.Source.Normalize(NormalizationForm.FormKD);
                Assert.AreEqual(testData.NfkdResult, result, 
                    ErrorString(testData.NfkdResult, result, testData.Description));
            }
        }

        [TestMethod]
        public void Normalization_FormKC()
        {
            foreach (NormalizationTestData testData in NormalizationTestDataSet.TestCases)
            {
                UString result = testData.Source.Normalize(NormalizationForm.FormKC);
                Assert.AreEqual(testData.NfkcResult, result,
                    ErrorString(testData.NfkcResult, result, testData.Description));
            }
        }
        #endregion

        #region Private helper methods
        private static UString CreateTestSubstring(string? str)
        {
            const string pre = "prefix";
            const string suff = "suffix";

            str ??= string.Empty;

            UString us = new(pre + str + suff);
            return us.SubString(pre.Length, us.Length - pre.Length - suff.Length);
        }

        private static string ErrorString(UString expected, UString result, string description)
        {
            return $"\nExp: {UStrToCodepoints(expected)}\nGot: {UStrToCodepoints(result)}\nFor:{description}";
        }

        private static string UStrToCodepoints(UString ustr)
        {
            return string.Join(" ", ustr.Select(uc => uc.ToHexString()));
        }
        #endregion
    }
}
