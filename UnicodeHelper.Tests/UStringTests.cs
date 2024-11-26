﻿namespace UnicodeHelper
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
            const string pre = "prefix";
            UString us1 = new(pre + string1);
            us1 = us1.SubString(pre.Length);
            UString us2 = new(string2);

            Assert.AreEqual(expectedResult, us1.Equals(us2), "Equal method failed");
            Assert.AreEqual(expectedResult, us1 == us2, "Equal operator failed");
            Assert.AreEqual(!expectedResult, us1 != us2, "NotEqual operator failed");

            us1 = new(string1);
            us2 = new(pre + string2);
            us2 = us2.SubString(pre.Length);

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
            const string pre = "prefix";
            UString us = new(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToUpperInvariant());

            us = new(pre + testString);
            us = us.SubString(pre.Length); // Test making sure that a substring results in the correct result
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
            const string pre = "prefix";
            UString us = new(testString);
            Assert.AreEqual(new UString(expectedResult), us.ToLowerInvariant());

            us = new(pre + testString);
            us = us.SubString(pre.Length); // Test making sure that a substring results in the correct result
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
            const string pre = "prefix";
            UString us = new(testString);
            UCodepoint[] expectedArray = expectedResult.Select(s => UCodepoint.ReadFromStr(s, 0)).ToArray();
            Assert.That.SequenceEqual(expectedArray, us.ToCodepointArray());

            us = new(pre + testString);
            us = us.SubString(pre.Length); // Test making sure that a substring results in the correct result
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
            const string pre = "prefix";
            UString us = new(testString);
            Assert.That.SequenceEqual(expectedResult, us);

            us = new(pre + testString);
            us = us.SubString(pre.Length); // Test making sure that a substring results in the correct result
            Assert.That.SequenceEqual(expectedResult, us);
        }
        #endregion
    }
}
