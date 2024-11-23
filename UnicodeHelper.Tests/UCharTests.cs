using System.Globalization;

namespace UnicodeHelper
{
    [TestClass]
    public class UCharTests
    {
        #region GetUnicodeCategory tests
        private static IEnumerable<object[]> CategoryTestData =>
        [
            [(UChar)'R', UnicodeCategory.UppercaseLetter],
            [(UChar)'l', UnicodeCategory.LowercaseLetter],
            [(UChar)0x00AF, UnicodeCategory.ModifierSymbol],// MACRON
            [(UChar)0x13D7A, UnicodeCategory.OtherLetter],  // EGYPTIAN HIEROGLYPH
            // Test that codepoint ranges are read in properly
            [(UChar)0xE000, UnicodeCategory.PrivateUse],    // Private Use start
            [(UChar)0xE050, UnicodeCategory.PrivateUse],    // Private Use middle
            [(UChar)0xF8FF, UnicodeCategory.PrivateUse],    // Private Use end
            [(UChar)0x17000, UnicodeCategory.OtherLetter],  // Tangut Ideograph start
            [(UChar)0x17384, UnicodeCategory.OtherLetter],  // Tangut Ideograph middle
            [(UChar)0x187F7, UnicodeCategory.OtherLetter]   // Tangut Ideograph end
        ];

        [TestMethod]
        [DynamicData(nameof(CategoryTestData))]
        public void GetUnicodeCategory(UChar uc, UnicodeCategory expectedCategory)
        {
            Assert.AreEqual(expectedCategory, UChar.GetUnicodeCategory(uc));
        }

        [TestMethod]
        public void GetUnicodeCategory_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                UnicodeCategory dotNetCategory = char.GetUnicodeCategory(c);
                UnicodeCategory uCat = UChar.GetUnicodeCategory(c);
                if (uCat != dotNetCategory)
                {
                    Console.WriteLine($"GetUnicodeCategory doesn't match for character {((UChar)c).ToHexString()} - " +
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
            [(UChar)0x0000, UnicodeBidiClass.BoundaryNeutral],
            [(UChar)' ', UnicodeBidiClass.WhiteSpace],
            [(UChar)'!', UnicodeBidiClass.OtherNeutral],
            [(UChar)'3', UnicodeBidiClass.EuropeanNumber],
            [(UChar)0x00C0, UnicodeBidiClass.LeftToRight],      // LATIN CAPITAL LETTER A WITH GRAVE
            [(UChar)0x0780, UnicodeBidiClass.ArabicLetter],     // THAANA LETTER HAA
            [(UChar)0x07C1, UnicodeBidiClass.RightToLeft],      // NKO DIGIT ONE
            [(UChar)0x0817, UnicodeBidiClass.NonSpacingMark],   // SAMARITAN MARK IN-ALAF
            [(UChar)0x10880, UnicodeBidiClass.RightToLeft],     // NABATAEAN LETTER FINAL ALEPH
            [(UChar)0x10D41, UnicodeBidiClass.ArabicNumber],    // GARAY DIGIT ONE
        ];

        [TestMethod]
        [DynamicData(nameof(BidiTestData))]
        public void GetBidiClass(UChar uc, UnicodeBidiClass expectedBidiClass)
        {
            Assert.AreEqual(expectedBidiClass, UChar.GetBidiClass(uc));
        }
        #endregion

        #region IsUpper tests
        public static IEnumerable<object[]> IsUpperTestData =>
        [
            [(UChar)0x1C89, true],  // CYRILLIC CAPITAL LETTER TJE
            [(UChar)0x10570, true], // VITHKUQI CAPITAL LETTER A
            [(UChar)0x10597, false],// VITHKUQI SMALL LETTER A
            [(UChar)0x20D6, false]  // COMBINING LEFT ARROW ABOVE
        ];

        [TestMethod]
        [DynamicData(nameof(IsUpperTestData))]
        public void IsUpper(UChar uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.IsUpper(uc));
        }

        [TestMethod]
        public void IsUpper_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsUpper = char.IsUpper(c);
                bool isUpper = UChar.IsUpper(c);
                if (isUpper != dotNetIsUpper)
                {
                    Console.WriteLine($"IsUpper doesn't match for character {((UChar)c).ToHexString()} - " +
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
            [(UChar)'C', (UChar)'C'],
            [(UChar)'c', (UChar)'C'],
            [(UChar)0x10570, (UChar)0x10570],// VITHKUQI CAPITAL LETTER A
            [(UChar)0x10597, (UChar)0x10570] // VITHKUQI SMALL LETTER A
        ];

        [TestMethod]
        [DynamicData(nameof(ToUpperTestData))]
        public void ToUpper(UChar uc, UChar expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.ToUpper(uc));
        }

        [TestMethod]
        public void ToUpper_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                char dotNetUpper = char.ToUpper(c);
                UChar upper = UChar.ToUpper(c);
                if (upper != dotNetUpper)
                {
                    Console.WriteLine($"ToLower doesn't match for character {((UChar)c).ToHexString()} - " +
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
            [(UChar)0x1C89, false], // CYRILLIC CAPITAL LETTER TJE
            [(UChar)0x10570, false],// VITHKUQI CAPITAL LETTER A
            [(UChar)0x10597, true], // VITHKUQI SMALL LETTER A
            [(UChar)0x20D6, false]  // COMBINING LEFT ARROW ABOVE
        ];

        [TestMethod]
        [DynamicData(nameof(IsLowerTestData))]
        public void IsLower(UChar uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.IsLower(uc));
        }

        [TestMethod]
        public void IsLower_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsLower = char.IsLower(c);
                bool isLower = UChar.IsLower(c);
                if (isLower != dotNetIsLower)
                {
                    Console.WriteLine($"IsLower doesn't match for character {((UChar)c).ToHexString()} - " +
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
            [(UChar)'C', (UChar)'c'],
            [(UChar)'c', (UChar)'c'],
            [(UChar)0x10570, (UChar)0x10597],// VITHKUQI CAPITAL LETTER A
            [(UChar)0x10597, (UChar)0x10597] // VITHKUQI SMALL LETTER A
        ];

        [TestMethod]
        [DynamicData(nameof(ToLowerTestData))]
        public void ToLower(UChar uc, UChar expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.ToLower(uc));
        }

        [TestMethod]
        public void ToLower_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                char dotNetLower = char.ToLower(c);
                UChar lower = UChar.ToLower(c);
                if (lower != dotNetLower)
                {
                    Console.WriteLine($"ToLower doesn't match for character {((UChar)c).ToHexString()} - " +
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
            [(UChar)0x0000, true],  // NULL
            [(UChar)0x0083, true],  // NO BREAK HERE
            [(UChar)0x16E65, false] // MEDEFAIDRIN SMALL LETTER Z
        ];

        [TestMethod]
        [DynamicData(nameof(IsControlTestData))]
        public void IsControl(UChar uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.IsControl(uc));
        }

        [TestMethod]
        public void IsControl_CompareWithDotNet()
        {
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsControl = char.IsControl(c);
                bool isControl = UChar.IsControl(c);

                Assert.AreEqual(dotNetIsControl, isControl, 
                    $"IsControl doesn't match for character {((UChar)c).ToHexString()} - " +
                    $".Net: {dotNetIsControl}, Found: {isControl}");
            }
        }
        #endregion

        #region IsLetter tests
        private static IEnumerable<object[]> IsLetterTestData =>
        [
            [(UChar)0x0042, true],  // LATIN CAPITAL LETTER B (uppercase letter)
            [(UChar)0x00E3, true],  // LATIN SMALL LETTER A WITH TILDE (lowercase letter)
            [(UChar)0x01C8, true],  // LATIN CAPITAL LETTER L WITH SMALL LETTER J (titlecase letter)
            [(UChar)0x02BB, true],  // MODIFIER LETTER TURNED COMMA (modifier letter)
            [(UChar)0x1C70, true],  // OL CHIKI LETTER EDD (other letter)
            [(UChar)0x0303, false], // COMBINING TILDE
            [(UChar)0x1108A, true], // KAITHI LETTER AI (other letter)
            [(UChar)0x118A1, true], // WARANG CITI CAPITAL LETTER A (uppercase letter)
            [(UChar)0x11834, false] // DOGRA VOWEL SIGN AI
        ];

        [TestMethod]
        [DynamicData(nameof(IsLetterTestData))]
        public void IsLetter(UChar uc, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.IsLetter(uc));
        }

        [TestMethod]
        public void IsLetter_CompareWithDotNet()
        {
            int incorrectCount = 0;
            for (char c = '\u0000'; c < 0xFFFF; c++)
            {
                bool dotNetIsLetter = char.IsLetter(c);
                bool isLetter = UChar.IsLetter(c);
                if (isLetter != dotNetIsLetter)
                {
                    Console.WriteLine($"IsLetter doesn't match for character {((UChar)c).ToHexString()} - " +
                                      $".Net: {dotNetIsLetter}, Found: {isLetter}");
                    incorrectCount++;
                }
            }

            Assert.AreEqual(8, incorrectCount, "Unexpected number of differences from .Net");
        }
        #endregion

        #region GetNumericValue tests
        private static IEnumerable<object[]> GetNumericValueTestData =>
        [
            [(UChar)'0', 0.0],
            [(UChar)'8', 8.0],
            [(UChar)0x0F33, -0.5],    // TIBETAN DIGIT HALF ZERO
            [(UChar)0x00BE, 0.75],    // VULGAR FRACTION THREE QUARTERS
            [(UChar)0x11FD0, 0.25],   // TAMIL FRACTION ONE QUARTER
            [(UChar)0x16B5D, 10000.0] // PAHAWH HMONG NUMBER TEN THOUSANDS
        ];

        [TestMethod]
        [DynamicData(nameof(GetNumericValueTestData))]
        public void GetNumericValue(UChar uc, double expectedResult)
        {
            Assert.AreEqual(expectedResult, UChar.GetNumericValue(uc));
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

                double numeric = UChar.GetNumericValue(c);
                Assert.AreEqual(dotNetNumeric, numeric,
                    $"\nGetNumericValue doesn't match for character {((UChar)c).ToHexString()} - .Net: {dotNetNumeric}, Found: {numeric}");
            }
        }
        #endregion
    }
}
