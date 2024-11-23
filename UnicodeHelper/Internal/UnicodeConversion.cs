using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace UnicodeHelper.Internal
{
    internal static class UnicodeConversion
    {
        #region Data fields
        private static readonly Dictionary<string, UnicodeCategory> strToCategoryMap =
            new Dictionary<string, UnicodeCategory>
            {
                // Cased Letters
                { "Lu", UnicodeCategory.UppercaseLetter },
                { "Ll", UnicodeCategory.LowercaseLetter },
                { "Lt", UnicodeCategory.TitlecaseLetter },
                // Other letters
                { "Lm", UnicodeCategory.ModifierLetter },
                { "Lo", UnicodeCategory.OtherLetter },
                // Marks
                { "Mn", UnicodeCategory.NonSpacingMark },
                { "Mc", UnicodeCategory.SpacingCombiningMark },
                { "Me", UnicodeCategory.EnclosingMark },
                // Numbers
                { "Nd", UnicodeCategory.DecimalDigitNumber },
                { "Nl", UnicodeCategory.LetterNumber },
                { "No", UnicodeCategory.OtherNumber },
                // Punctuation
                { "Pc", UnicodeCategory.ConnectorPunctuation },
                { "Pd", UnicodeCategory.DashPunctuation },
                { "Ps", UnicodeCategory.OpenPunctuation },
                { "Pe", UnicodeCategory.ClosePunctuation },
                { "Pi", UnicodeCategory.InitialQuotePunctuation },
                { "Pf", UnicodeCategory.FinalQuotePunctuation },
                { "Po", UnicodeCategory.OtherPunctuation },
                // Symbols
                { "Sm", UnicodeCategory.MathSymbol },
                { "Sc", UnicodeCategory.CurrencySymbol },
                { "Sk", UnicodeCategory.ModifierSymbol },
                { "So", UnicodeCategory.OtherSymbol },
                // Separators
                { "Zs", UnicodeCategory.SpaceSeparator },
                { "Zl", UnicodeCategory.LineSeparator },
                { "Zp", UnicodeCategory.ParagraphSeparator },
                // Others
                { "Cc", UnicodeCategory.Control },
                { "Cf", UnicodeCategory.Format },
                { "Cs", UnicodeCategory.Surrogate },
                { "Co", UnicodeCategory.PrivateUse }
                // File won't contain this category
                //{ "Cn", UnicodeCategory.OtherNotAssigned }
            };

        private static readonly Dictionary<string, UnicodeBidiClass> strToBidiClassMap =
            new Dictionary<string, UnicodeBidiClass>
            {
                // Strong types
                { "L", UnicodeBidiClass.LeftToRight },
                { "R", UnicodeBidiClass.RightToLeft },
                { "AL", UnicodeBidiClass.ArabicLetter },
                // Weak types
                { "EN", UnicodeBidiClass.EuropeanNumber },
                { "ES", UnicodeBidiClass.EuropeanSeparator },
                { "ET", UnicodeBidiClass.EuropeanTerminator },
                { "AN", UnicodeBidiClass.ArabicNumber },
                { "CS", UnicodeBidiClass.CommonSeparator },
                { "NSM", UnicodeBidiClass.NonSpacingMark },
                { "BN", UnicodeBidiClass.BoundaryNeutral },
                // Neutral types
                { "B", UnicodeBidiClass.ParagraphSeparator },
                { "S", UnicodeBidiClass.SegmentSeparator },
                { "WS", UnicodeBidiClass.WhiteSpace },
                { "ON", UnicodeBidiClass.OtherNeutral },
                // Explicit formatting types
                {"LRE", UnicodeBidiClass.LeftToRightEmbedding },
                {"LRO", UnicodeBidiClass.LeftToRightOverride },
                {"RLE", UnicodeBidiClass.RightToLeftEmbedding },
                {"RLO", UnicodeBidiClass.RightToLeftOverride },
                {"PDF", UnicodeBidiClass.PopDirectionalFormat },
                {"LRI", UnicodeBidiClass.LeftToRightIsolate },
                {"RLI", UnicodeBidiClass.RightToLeftIsolate },
                {"FSI", UnicodeBidiClass.FirstStrongIsolate },
                {"PDI", UnicodeBidiClass.PopDirectionalIsolate }
            };

        private static readonly Dictionary<string, UnicodeProperty> strToPropertyMap =
            new Dictionary<string, UnicodeProperty>
            {
                // Base properties
                { "ASCII_Hex_Digit", UnicodeProperty.AsciiHexDigit },
                { "Bidi_Control", UnicodeProperty.BidiControl },
                { "Dash", UnicodeProperty.Dash },
                { "Deprecated", UnicodeProperty.Deprecated },
                { "Diacritic", UnicodeProperty.Diacritic },
                { "Extender", UnicodeProperty.Extender },
                { "Hex_Digit", UnicodeProperty.HexDigit },
#pragma warning disable CS0618 // Type or member is obsolete
                { "Hyphen", UnicodeProperty.Hyphen },
#pragma warning restore CS0618 // Type or member is obsolete
                { "ID_Compat_Math_Continue", UnicodeProperty.IdCompatMathContinue },
                { "ID_Compat_Math_Start", UnicodeProperty.IdCompatMathStart },
                { "Ideographic", UnicodeProperty.Ideographic },
                { "IDS_Binary_Operator", UnicodeProperty.IdsBinaryOperator },
                { "IDS_Trinary_Operator", UnicodeProperty.IdsTrinaryOperator },
                { "IDS_Unary_Operator", UnicodeProperty.IdsUnaryOperator },
                { "Join_Control", UnicodeProperty.JoinControl },
                { "Logical_Order_Exception", UnicodeProperty.LogicalOrderException },
                { "Modifier_Combining_Mark", UnicodeProperty.ModifierCombiningMark },
                { "Noncharacter_Code_Point", UnicodeProperty.NonCharacterCodePoint },
                { "Other_Alphabetic", UnicodeProperty.OtherAlphabetic },
                { "Other_Default_Ignorable_Code_Point", UnicodeProperty.OtherDefaultIgnorableCodePoint },
                { "Other_Grapheme_Extend", UnicodeProperty.OtherGraphemeExtend },
                { "Other_ID_Continue", UnicodeProperty.OtherIdContinue },
                { "Other_ID_Start", UnicodeProperty.OtherIdStart },
                { "Other_Lowercase", UnicodeProperty.OtherLowercase },
                { "Other_Math", UnicodeProperty.OtherMath },
                { "Other_Uppercase", UnicodeProperty.OtherUppercase },
                { "Pattern_Syntax", UnicodeProperty.PatternSyntax },
                { "Pattern_White_Space", UnicodeProperty.PatternWhiteSpace },
                { "Prepended_Concatenation_Mark", UnicodeProperty.PrependedConcatenationMark },
                { "Quotation_Mark", UnicodeProperty.QuotationMark },
                { "Radical", UnicodeProperty.Radical },
                { "Regional_Indicator", UnicodeProperty.RegionalIndicator },
                { "Sentence_Terminal", UnicodeProperty.SentenceTerminal },
                { "Soft_Dotted", UnicodeProperty.SoftDotted },
                { "Terminal_Punctuation", UnicodeProperty.TerminalPunctuation },
                { "Unified_Ideograph", UnicodeProperty.UnifiedIdeograph },
                { "Variation_Selector", UnicodeProperty.VariationSelector },
                { "White_Space", UnicodeProperty.WhiteSpace },
                // Derived properties
                { "Alphabetic", UnicodeProperty.Alphabetic },
                { "Case_Ignorable", UnicodeProperty.CaseIgnorable },
                { "Cased", UnicodeProperty.Cased },
                { "Changes_When_Casefolded", UnicodeProperty.ChangesWhenCasefolded },
                { "Changes_When_Casemapped", UnicodeProperty.ChangesWhenCasemapped },
                { "Changes_When_Lowercased", UnicodeProperty.ChangesWhenLowercased },
                { "Changes_When_Titlecased", UnicodeProperty.ChangesWhenTitlecased },
                { "Changes_When_Uppercased", UnicodeProperty.ChangesWhenUppercased },
                { "Default_Ignorable_Code_Point", UnicodeProperty.DefaultIgnorableCodePoint },
                { "Grapheme_Base", UnicodeProperty.GraphemeBase },
                { "Grapheme_Extend", UnicodeProperty.GraphemeExtend },
#pragma warning disable CS0618 // Type or member is obsolete
                { "Grapheme_Link", UnicodeProperty.GraphemeLink },
#pragma warning restore CS0618 // Type or member is obsolete
                { "ID_Continue", UnicodeProperty.IdContinue },
                { "ID_Start", UnicodeProperty.IdStart },
                { "InCB", UnicodeProperty.IndicConjunctBreak },
                { "Lowercase", UnicodeProperty.Lowercase },
                { "Math", UnicodeProperty.Math },
                { "Uppercase", UnicodeProperty.Uppercase },
                { "XID_Continue", UnicodeProperty.XidContinue },
                { "XID_Start", UnicodeProperty.XidStart },
            };
        #endregion

        public static UnicodeCategory ConvertCategory(string categoryStr)
        {
            return strToCategoryMap[categoryStr];
        }

        public static UnicodeBidiClass ConvertBidiClass(string bidiClassStr)
        {
            return strToBidiClassMap[bidiClassStr];
        }

        public static UnicodeProperty ConvertProperty(string propertyStr)
        {
            return strToPropertyMap[propertyStr];
        }

        public static double ConvertNumeric(string numericStr)
        {
            string[] numbers = numericStr.Split('/');
            double value = double.Parse(numbers[0]);
            if (numbers.Length == 1) 
                return value;

            Debug.Assert(numbers.Length == 2);
            double bottom = double.Parse(numbers[1]);
            return value / bottom;
        }
    }
}
