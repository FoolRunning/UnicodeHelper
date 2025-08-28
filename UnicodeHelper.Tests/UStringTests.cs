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

        #region LastIndexOf_UCodepoint tests
        private static IEnumerable<object[]> UCodepointLastIndexOfExceptionTestData =>
        [
            ["", 0, 1, typeof(ArgumentOutOfRangeException)],
            ["", 1, 0, typeof(ArgumentOutOfRangeException)],
            ["This", -1, 2, typeof(ArgumentException)],
            ["This", 3, 5, typeof(ArgumentException)],
            ["This", 3, -1, typeof(ArgumentOutOfRangeException)],
            ["This", -1, 1, typeof(ArgumentException)],
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
            ["", (UCodepoint)'A', -1, 0, -1],
            ["This is\r\na test!", (UCodepoint)'\r', 8, 6, 7],
            ["This is\r\na test!", (UCodepoint)'i', 15, 16, 5],
            ["This is\r\na test!", (UCodepoint)'T', 8, 6, -1],
            ["This", (UCodepoint)'T', -1, 0, -1],
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

        #region Private helper methods
        private static UString CreateTestSubstring(string? str)
        {
            const string pre = "prefix";
            const string suff = "suffix";

            str ??= string.Empty;

            UString us = new(pre + str + suff);
            return us.SubString(pre.Length, us.Length - pre.Length - suff.Length);
        }
        #endregion
    }
}
