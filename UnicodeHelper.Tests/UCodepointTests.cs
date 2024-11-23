using System.Globalization;

namespace UnicodeHelper
{
    [TestClass]
    public class UCodepointTests
    {
        #region GetUnicodeCategory tests
        private static IEnumerable<object[]> CategoryTestData =>
        [
            [(UCodepoint)'R', UnicodeCategory.UppercaseLetter],
            [(UCodepoint)'l', UnicodeCategory.LowercaseLetter],
            [(UCodepoint)0x00AF, UnicodeCategory.ModifierSymbol],// MACRON
            [(UCodepoint)0x13D7A, UnicodeCategory.OtherLetter],  // EGYPTIAN HIEROGLYPH
            // Test that codepoint ranges are read in properly
            [(UCodepoint)0xE000, UnicodeCategory.PrivateUse],    // Private Use start
            [(UCodepoint)0xE050, UnicodeCategory.PrivateUse],    // Private Use middle
            [(UCodepoint)0xF8FF, UnicodeCategory.PrivateUse],    // Private Use end
            [(UCodepoint)0x17000, UnicodeCategory.OtherLetter],  // Tangut Ideograph start
            [(UCodepoint)0x17384, UnicodeCategory.OtherLetter],  // Tangut Ideograph middle
            [(UCodepoint)0x187F7, UnicodeCategory.OtherLetter]   // Tangut Ideograph end
        ];

        [TestMethod]
        [DynamicData(nameof(CategoryTestData))]
        public void GetUnicodeCategory(UCodepoint uc, UnicodeCategory expectedCategory)
        {
            Assert.AreEqual(expectedCategory, UCodepoint.GetUnicodeCategory(uc));
        }

        [TestMethod]
        public void GetUnicodeCategory_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                UnicodeCategory dotNetCategory = char.GetUnicodeCategory(c);
                UnicodeCategory uCat = UCodepoint.GetUnicodeCategory(c);
                if (uCat != dotNetCategory)
                {
                    Console.WriteLine($"GetUnicodeCategory doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetCategory}, Found: {uCat}");
                    Assert.AreEqual(UnicodeCategory.OtherNotAssigned, dotNetCategory);

                    incorrectCount++;
                }
            }

            Assert.AreEqual(22, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region GetBidiClass tests
        private static IEnumerable<object[]> BidiTestData =>
        [
            [(UCodepoint)0x0000, UnicodeBidiClass.BoundaryNeutral],
            [(UCodepoint)' ', UnicodeBidiClass.WhiteSpace],
            [(UCodepoint)'!', UnicodeBidiClass.OtherNeutral],
            [(UCodepoint)'3', UnicodeBidiClass.EuropeanNumber],
            [(UCodepoint)0x00C0, UnicodeBidiClass.LeftToRight],      // LATIN CAPITAL LETTER A WITH GRAVE
            [(UCodepoint)0x0780, UnicodeBidiClass.ArabicLetter],     // THAANA LETTER HAA
            [(UCodepoint)0x07C1, UnicodeBidiClass.RightToLeft],      // NKO DIGIT ONE
            [(UCodepoint)0x0817, UnicodeBidiClass.NonSpacingMark],   // SAMARITAN MARK IN-ALAF
            [(UCodepoint)0x10880, UnicodeBidiClass.RightToLeft],     // NABATAEAN LETTER FINAL ALEPH
            [(UCodepoint)0x10D41, UnicodeBidiClass.ArabicNumber],    // GARAY DIGIT ONE
        ];

        [TestMethod]
        [DynamicData(nameof(BidiTestData))]
        public void GetBidiClass(UCodepoint uc, UnicodeBidiClass expectedBidiClass)
        {
            Assert.AreEqual(expectedBidiClass, UCodepoint.GetBidiClass(uc));
        }
        #endregion

        #region IsUpper tests
        public static IEnumerable<object[]> IsUpperTestData =>
        [
            [(UCodepoint)0x1C89, true],  // CYRILLIC CAPITAL LETTER TJE
            [(UCodepoint)0x10570, true], // VITHKUQI CAPITAL LETTER A
            [(UCodepoint)0x10597, false],// VITHKUQI SMALL LETTER A
            [(UCodepoint)0x20D6, false]  // COMBINING LEFT ARROW ABOVE
        ];

        [TestMethod]
        [DynamicData(nameof(IsUpperTestData))]
        public void IsUpper(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsUpper(uc));
        }

        [TestMethod]
        public void IsUpper_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsUpper = char.IsUpper(c);
                bool isUpper = UCodepoint.IsUpper(c);
                if (isUpper != dotNetIsUpper)
                {
                    Console.WriteLine($"IsUpper doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsUpper}, Found: {isUpper}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(5, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region ToUpper tests
        private static IEnumerable<object[]> ToUpperTestData =>
        [
            [(UCodepoint)'C', (UCodepoint)'C'],
            [(UCodepoint)'c', (UCodepoint)'C'],
            [(UCodepoint)0x10570, (UCodepoint)0x10570],// VITHKUQI CAPITAL LETTER A
            [(UCodepoint)0x10597, (UCodepoint)0x10570] // VITHKUQI SMALL LETTER A
        ];

        [TestMethod]
        [DynamicData(nameof(ToUpperTestData))]
        public void ToUpper(UCodepoint uc, UCodepoint expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.ToUpper(uc));
        }

        [TestMethod]
        public void ToUpper_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                char dotNetUpper = char.ToUpper(c);
                UCodepoint upper = UCodepoint.ToUpper(c);
                if (upper != dotNetUpper)
                {
                    Console.WriteLine($"ToLower doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetUpper.ToHexString()}, Found: {upper.ToHexString()}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(10, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsLower tests
        private static IEnumerable<object[]> IsLowerTestData =>
        [
            [(UCodepoint)0x1C89, false], // CYRILLIC CAPITAL LETTER TJE
            [(UCodepoint)0x10570, false],// VITHKUQI CAPITAL LETTER A
            [(UCodepoint)0x10597, true], // VITHKUQI SMALL LETTER A
            [(UCodepoint)0x20D6, false]  // COMBINING LEFT ARROW ABOVE
        ];

        [TestMethod]
        [DynamicData(nameof(IsLowerTestData))]
        public void IsLower(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsLower(uc));
        }

        [TestMethod]
        public void IsLower_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsLower = char.IsLower(c);
                bool isLower = UCodepoint.IsLower(c);
                if (isLower != dotNetIsLower)
                {
                    Console.WriteLine($"IsLower doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsLower}, Found: {isLower}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(3, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region ToLower tests
        private static IEnumerable<object[]> ToLowerTestData =>
        [
            [(UCodepoint)'C', (UCodepoint)'c'],
            [(UCodepoint)'c', (UCodepoint)'c'],
            [(UCodepoint)0x10570, (UCodepoint)0x10597],// VITHKUQI CAPITAL LETTER A
            [(UCodepoint)0x10597, (UCodepoint)0x10597] // VITHKUQI SMALL LETTER A
        ];

        [TestMethod]
        [DynamicData(nameof(ToLowerTestData))]
        public void ToLower(UCodepoint uc, UCodepoint expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.ToLower(uc));
        }

        [TestMethod]
        public void ToLower_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                char dotNetLower = char.ToLower(c);
                UCodepoint lower = UCodepoint.ToLower(c);
                if (lower != dotNetLower)
                {
                    Console.WriteLine($"ToLower doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetLower.ToHexString()}, Found: {lower.ToHexString()}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(10, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsControl tests
        private static IEnumerable<object[]> IsControlTestData =>
        [
            [(UCodepoint)0x0000, true],  // NULL
            [(UCodepoint)0x0083, true],  // NO BREAK HERE
            [(UCodepoint)0x16E65, false] // MEDEFAIDRIN SMALL LETTER Z
        ];

        [TestMethod]
        [DynamicData(nameof(IsControlTestData))]
        public void IsControl(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsControl(uc));
        }

        [TestMethod]
        public void IsControl_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsControl = char.IsControl(c);
                bool isControl = UCodepoint.IsControl(c);

                Assert.AreEqual(dotNetIsControl, isControl, 
                    $"IsControl doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                    $".Net: {dotNetIsControl}, Found: {isControl}");
            }
        }
        #endregion

        #region IsLetter tests
        private static IEnumerable<object[]> IsLetterTestData =>
        [
            [(UCodepoint)0x0042, true],  // LATIN CAPITAL LETTER B (Lu)
            [(UCodepoint)0x00E3, true],  // LATIN SMALL LETTER A WITH TILDE (Ll)
            [(UCodepoint)0x01C8, true],  // LATIN CAPITAL LETTER L WITH SMALL LETTER J (Lt)
            [(UCodepoint)0x02BB, true],  // MODIFIER LETTER TURNED COMMA (Lm)
            [(UCodepoint)0x1C70, true],  // OL CHIKI LETTER EDD (Lo)
            [(UCodepoint)0x0303, false], // COMBINING TILDE
            [(UCodepoint)0x1108A, true], // KAITHI LETTER AI (Lo)
            [(UCodepoint)0x118A1, true], // WARANG CITI CAPITAL LETTER A (Lu)
            [(UCodepoint)0x11834, false], // DOGRA VOWEL SIGN AI
        ];

        [TestMethod]
        [DynamicData(nameof(IsLetterTestData))]
        public void IsLetter(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsLetter(uc));
        }

        [TestMethod]
        public void IsLetter_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsLetter = char.IsLetter(c);
                bool isLetter = UCodepoint.IsLetter(c);
                if (isLetter != dotNetIsLetter)
                {
                    Console.WriteLine($"IsLetter doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsLetter}, Found: {isLetter}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(8, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsPunctuation tests
        private static IEnumerable<object[]> IsPunctuationTestData =>
        [
            [(UCodepoint)0x203F, true],  // UNDERTIE (Pc)
            [(UCodepoint)0x2010, true],  // HYPHEN (Pd)
            [(UCodepoint)0x201A, true],  // SINGLE LOW-9 QUOTATION MARK (Ps)
            [(UCodepoint)0x2046, true],  // RIGHT SQUARE BRACKET WITH QUILL (Pe)
            [(UCodepoint)0x2E02, true],  // LEFT SUBSTITUTION BRACKET (Pi)
            [(UCodepoint)0x2E03, true],  // RIGHT SUBSTITUTION BRACKET (Pf)
            [(UCodepoint)0x1E95F, true], // ADLAM INITIAL QUESTION MARK (Po)
            [(UCodepoint)'D', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsPunctuationTestData))]
        public void IsPunctuation(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsPunctuation(uc));
        }

        [TestMethod]
        public void IsPunctuation_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsPunctuation = char.IsPunctuation(c);
                bool isPunctuation = UCodepoint.IsPunctuation(c);
                if (isPunctuation != dotNetIsPunctuation)
                {
                    Console.WriteLine($"IsPunctuation doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsPunctuation}, Found: {isPunctuation}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(3, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsSeparator tests
        private static IEnumerable<object[]> IsSeparatorTestData =>
        [
            [(UCodepoint)'\t', false],
            [(UCodepoint)'\r', false],
            [(UCodepoint)'\n', false],
            [(UCodepoint)' ', true],
            [(UCodepoint)0x1680, true],  // OGHAM SPACE MARK (Zs)
            [(UCodepoint)0x2028, true],  // LINE SEPARATOR (Zl)
            [(UCodepoint)0x2029, true],  // PARAGRAPH SEPARATOR (Zp)
            [(UCodepoint)0x3000, true],  // IDEOGRAPHIC SPACE (Zs)
            [(UCodepoint)'E', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsSeparatorTestData))]
        public void IsSeparator(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsSeparator(uc));
        }

        [TestMethod]
        public void IsSeparator_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsSeparator = char.IsSeparator(c);
                bool isSeparator = UCodepoint.IsSeparator(c);
                Assert.AreEqual(dotNetIsSeparator, isSeparator, 
                    $"IsSeparator doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                    $".Net: {dotNetIsSeparator}, Found: {isSeparator}");
            }
        }
        #endregion

        #region IsSymbol tests
        private static IEnumerable<object[]> IsSymbolTestData =>
        [
            [(UCodepoint)0x25B7, true],  // WHITE RIGHT-POINTING TRIANGLE (Sm)
            [(UCodepoint)0xA838, true],  // NORTH INDIC RUPEE MARK (Sc)
            [(UCodepoint)0xAB5B, true],  // MODIFIER BREVE WITH INVERTED BREVE (Sk)
            [(UCodepoint)0x1D816, true], // SIGNWRITING HAND-FIST INDEX MIDDLE CONJOINED INDEX BENT (So)
            [(UCodepoint)'E', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsSymbolTestData))]
        public void IsSymbol(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsSymbol(uc));
        }

        [TestMethod]
        public void IsSymbol_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsSymbol = char.IsSymbol(c);
                bool isSymbol = UCodepoint.IsSymbol(c);
                if (isSymbol != dotNetIsSymbol)
                {
                    Console.WriteLine($"IsSymbol doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsSymbol}, Found: {isSymbol}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(10, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsDigit tests
        private static IEnumerable<object[]> IsDigitTestData =>
        [
            [(UCodepoint)'5', true],
            [(UCodepoint)0x096D, true],  // DEVANAGARI DIGIT SEVEN
            [(UCodepoint)0x1BB0, true],  // SUNDANESE DIGIT ZERO
            [(UCodepoint)0x1E954, true], // ADLAM DIGIT FOUR
            [(UCodepoint)'E', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsDigitTestData))]
        public void IsDigit(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsDigit(uc));
        }

        [TestMethod]
        public void IsDigit_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsDigit = char.IsDigit(c);
                bool isDigit = UCodepoint.IsDigit(c);
                Assert.AreEqual(dotNetIsDigit, isDigit, 
                    $"IsDigit doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                    $".Net: {dotNetIsDigit}, Found: {isDigit}");
            }
        }
        #endregion

        #region IsNumber tests
        private static IEnumerable<object[]> IsNumberTestData =>
        [
            [(UCodepoint)'5', true],
            [(UCodepoint)0x2161, true],  // ROMAN NUMERAL TWO (Nl)
            [(UCodepoint)0x1E954, true], // ADLAM DIGIT FOUR (Nd)
            [(UCodepoint)0x1D2CF, true], // KAKTOVIK NUMERAL FIFTEEN (No)
            [(UCodepoint)'E', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsNumberTestData))]
        public void IsNumber(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsNumber(uc));
        }

        [TestMethod]
        public void IsNumber_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsDigit = char.IsNumber(c);
                bool isDigit = UCodepoint.IsNumber(c);
                Assert.AreEqual(dotNetIsDigit, isDigit, 
                    $"IsNumber doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                    $".Net: {dotNetIsDigit}, Found: {isDigit}");
            }
        }
        #endregion

        #region IsLetterOrDigit tests
        private static IEnumerable<object[]> IsLetterOrDigitTestData =>
        [
            [(UCodepoint)0x0042, true],  // LATIN CAPITAL LETTER B (Lu)
            [(UCodepoint)0x00E3, true],  // LATIN SMALL LETTER A WITH TILDE (Ll)
            [(UCodepoint)0x01C8, true],  // LATIN CAPITAL LETTER L WITH SMALL LETTER J (Lt)
            [(UCodepoint)0x02BB, true],  // MODIFIER LETTER TURNED COMMA (Lm)
            [(UCodepoint)0x1C70, true],  // OL CHIKI LETTER EDD (Lo)
            [(UCodepoint)0x0303, false], // COMBINING TILDE
            [(UCodepoint)0x1108A, true], // KAITHI LETTER AI (Lo)
            [(UCodepoint)0x118A1, true], // WARANG CITI CAPITAL LETTER A (Lu)
            [(UCodepoint)0x11834, false], // DOGRA VOWEL SIGN AI
            [(UCodepoint)'5', true],
            [(UCodepoint)0x096D, true],  // DEVANAGARI DIGIT SEVEN
            [(UCodepoint)0x1BB0, true],  // SUNDANESE DIGIT ZERO
            [(UCodepoint)0x1E954, true], // ADLAM DIGIT FOUR
        ];

        [TestMethod]
        [DynamicData(nameof(IsLetterOrDigitTestData))]
        public void IsLetterOrDigit(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsLetterOrDigit(uc));
        }

        [TestMethod]
        public void IsLetterOrDigit_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsLetterOrDigit = char.IsLetterOrDigit(c);
                bool isLetterOrDigit = UCodepoint.IsLetterOrDigit(c);
                if (isLetterOrDigit != dotNetIsLetterOrDigit)
                {
                    Console.WriteLine($"IsLetterOrDigit doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                                      $".Net: {dotNetIsLetterOrDigit}, Found: {isLetterOrDigit}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(8, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region IsWhiteSpace tests
        private static IEnumerable<object[]> IsWhiteSpaceTestData =>
        [
            [(UCodepoint)'\t', true],
            [(UCodepoint)'\n', true],
            [(UCodepoint)'\r', true],
            [(UCodepoint)' ', true],
            [(UCodepoint)0x00A0, true], // NO-BREAK SPACE
            [(UCodepoint)0x2028, true], // LINE SEPARATOR
            [(UCodepoint)0x3000, true], // IDEOGRAPHIC SPACE
            [(UCodepoint)'Q', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsWhiteSpaceTestData))]
        public void IsWhiteSpace(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsWhiteSpace(uc));
        }

        [TestMethod]
        public void IsWhiteSpace_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsWhiteSpace = char.IsWhiteSpace(c);
                bool isWhiteSpace = UCodepoint.IsWhiteSpace(c);
                Assert.AreEqual(dotNetIsWhiteSpace, isWhiteSpace, 
                    $"IsWhiteSpace doesn't match for character {((UCodepoint)c).ToHexString()} - " +
                    $".Net: {dotNetIsWhiteSpace}, Found: {isWhiteSpace}");
            }
        }
        #endregion

        #region IsDiacritic tests
        private static IEnumerable<object[]> IsDiacriticTestData =>
        [
            [(UCodepoint)0x02B0, true],  // MODIFIER LETTER SMALL H (start of range)
            [(UCodepoint)0x02BA, true],  // MODIFIER LETTER DOUBLE PRIME (middle of range)
            [(UCodepoint)0x02C1, true],  // MODIFIER LETTER REVERSED GLOTTAL STOP (end of range)
            [(UCodepoint)0xA9E5, true],  // MYANMAR SIGN SHAN SAW
            [(UCodepoint)0x10D4E, true], // GARAY VOWEL LENGTH MARK
            [(UCodepoint)'D', false]
        ];

        [TestMethod]
        [DynamicData(nameof(IsDiacriticTestData))]
        public void IsDiacritic(UCodepoint uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.IsDiacritic(uc));
        }
        #endregion

        #region GetNumericValue tests
        private static IEnumerable<object[]> GetNumericValueTestData =>
        [
            [(UCodepoint)'0', 0.0],
            [(UCodepoint)'8', 8.0],
            [(UCodepoint)0x0F33, -0.5],    // TIBETAN DIGIT HALF ZERO
            [(UCodepoint)0x00BE, 0.75],    // VULGAR FRACTION THREE QUARTERS
            [(UCodepoint)0x11FD0, 0.25],   // TAMIL FRACTION ONE QUARTER
            [(UCodepoint)0x16B5D, 10000.0] // PAHAWH HMONG NUMBER TEN THOUSANDS
        ];

        [TestMethod]
        [DynamicData(nameof(GetNumericValueTestData))]
        public void GetNumericValue(UCodepoint uc, double expectedResult)
        {
            Assert.AreEqual(expectedResult, UCodepoint.GetNumericValue(uc));
        }

        [TestMethod]
        public void GetNumericValue_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                double dotNetNumeric = char.GetNumericValue(c);
                
                // .Net doesn't seem to follow the Unicode specification where it states that the
                // default value for numeric value should be NaN.
                // (see https://www.unicode.org/reports/tr44/#Default_Values_Table)
                if (dotNetNumeric == -1.0)
                    dotNetNumeric = double.NaN;

                double numeric = UCodepoint.GetNumericValue(c);
                Assert.AreEqual(dotNetNumeric, numeric,
                    $"\nGetNumericValue doesn't match for character {((UCodepoint)c).ToHexString()} - .Net: {dotNetNumeric}, Found: {numeric}");
            }
        }
        #endregion
    }
}
