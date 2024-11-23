using System;
using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Descriptions taken directly from the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#PropList.txt">PropList.txt</see> and
    /// <see href="https://www.unicode.org/reports/tr44/#DerivedCoreProperties.txt">DerivedCoreProperties.txt</see>
    /// </remarks>
    [PublicAPI]
    [Flags]
    public enum UnicodeProperty : long
    {
        /// <summary>
        /// No properties are defined
        /// </summary>
        Undefined = 0,
        
        #region Normal properties
        /// <summary>
        /// ASCII characters commonly used for the representation of hexadecimal numbers.
        /// </summary>
        AsciiHexDigit = 1L << 0,
        /// <summary>
        /// Format control characters which have specific functions in the Unicode Bidirectional Algorithm
        /// </summary>
        BidiControl = 1L << 1,
        /// <summary>
        /// Punctuation characters explicitly called out as dashes in the Unicode Standard,
        /// plus their compatibility equivalents. Most of these have the category DashPunctuation (Pd),
        /// but some have the category MathSymbol (Sm) because of their use in mathematics.
        /// </summary>
        Dash = 1L << 2,
        /// <summary>
        /// For a machine-readable list of deprecated characters. No characters will ever be removed
        /// from the standard, but the usage of deprecated characters is strongly discouraged.
        /// </summary>
        Deprecated = 1L << 3,
        /// <summary>
        /// Characters that linguistically modify the meaning of another character to which they apply.
        /// Some diacritics are not combining characters, and some combining characters are not diacritics.
        /// Typical examples include accent marks, tone marks or letters, and phonetic modifier letters.
        /// The Diacritic property is used in tooling which assigns default primary weights for characters,
        /// for generation of the DUCET table used by the Unicode Collation Algorithm (UCA).
        /// </summary>
        Diacritic = 1L << 4,
        /// <summary>
        /// Characters whose principal function is to extend the value of a preceding alphabetic character or
        /// to extend the shape of adjacent characters. Typical of these are length marks, gemination marks,
        /// repetition marks, iteration marks, and the Arabic tatweel. The Extender property is used in
        /// tooling which assigns default primary weights for characters, for generation of the DUCET table
        /// used by the Unicode Collation Algorithm (UCA).
        /// </summary>
        Extender = 1L << 5,
        /// <summary>
        /// Characters commonly used for the representation of hexadecimal numbers, plus their compatibility
        /// equivalents with Decomposition_Type=Wide.
        /// </summary>
        HexDigit = 1L << 6,
        /// <summary>
        /// Deprecated in Unicode 6.0.0.
        /// Dashes which are used to mark connections between pieces of words, plus the Katakana middle dot.
        /// The Katakana middle dot functions like a hyphen, but is shaped like a dot rather than a dash.
        /// </summary>
        [Obsolete("Supplanted by LineBreak property values")]
        Hyphen = 1L << 7,
        /// <summary>
        /// Characters considered to be CJKV (Chinese, Japanese, Korean, and Vietnamese) or
        /// other siniform (Chinese writing-related) ideographs. This property roughly defines the class
        /// of "Chinese characters" and does not include characters of other logographic scripts such as
        /// Cuneiform or Egyptian Hieroglyphs. The Ideographic property is used in the definition of
        /// Ideographic Description Sequences.
        /// </summary>
        Ideographic = 1L << 8,
        /// <summary>
        /// Used in mathematical identifier profile in UAX #31
        /// </summary>
        IdCompatMathStart = 1L << 9,
        /// <summary>
        /// Used in mathematical identifier profile in UAX #31.
        /// </summary>
        IdCompatMathContinue = 1L << 10,
        /// <summary>
        /// Used in Ideographic Description Sequences.
        /// </summary>
        IdsUnaryOperator = 1L << 11,
        /// <summary>
        /// Used in Ideographic Description Sequences.
        /// </summary>
        IdsBinaryOperator = 1L << 12,
        /// <summary>
        /// Used in Ideographic Description Sequences.
        /// </summary>
        IdsTrinaryOperator = 1L << 13,
        /// <summary>
        /// Format control characters which have specific functions for control of cursive joining and ligation.
        /// </summary>
        JoinControl = 1L << 14,
        /// <summary>
        /// A small number of spacing vowel letters occurring in certain Southeast Asian scripts such as
        /// Thai and Lao, which use a visual order display model. These letters are stored in text ahead of
        /// syllable-initial consonants, and require special handling for processes such as searching and sorting.
        /// </summary>
        LogicalOrderException = 1L << 15,
        /// <summary>
        /// Arabic combining marks potentially reordered by the AMTRA algorithm specified in UAX #53.
        /// </summary>
        ModifierCombiningMark = 1L << 16,
        /// <summary>
        /// Code points permanently reserved for internal use.
        /// </summary>
        NonCharacterCodePoint = 1L << 17,
        /// <summary>
        /// Used in deriving the Alphabetic property.
        /// </summary>
        OtherAlphabetic = 1L << 18,
        /// <summary>
        /// Used in deriving the Default_Ignorable_Code_Point property.
        /// </summary>
        OtherDefaultIgnorableCodePoint = 1L << 19,
        /// <summary>
        /// Used in deriving the Grapheme_Extend property.
        /// </summary>
        OtherGraphemeExtend = 1L << 20,
        /// <summary>
        /// Used to maintain backward compatibility of
        /// <seealso href="https://www.unicode.org/reports/tr44/#ID_Continue">ID_Continue</seealso>.
        /// </summary>
        OtherIdContinue = 1L << 21,
        /// <summary>
        /// Used to maintain backward compatibility of
        /// <seealso href="https://www.unicode.org/reports/tr44/#ID_Start">ID_Start</seealso>.
        /// </summary>
        OtherIdStart = 1L << 22,
        /// <summary>
        /// Used in deriving the Lowercase property.
        /// </summary>
        OtherLowercase = 1L << 23,
        /// <summary>
        /// Used in deriving the Math property.
        /// </summary>
        OtherMath = 1L << 24,
        /// <summary>
        /// Used in deriving the Uppercase property.
        /// </summary>
        OtherUppercase = 1L << 25,
        /// <summary>
        /// Used for pattern syntax as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<seealso href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</seealso>].
        /// </summary>
        PatternSyntax = 1L << 26,
        /// <summary>
        /// Used for pattern syntax as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<seealso href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</seealso>].
        /// </summary>
        PatternWhiteSpace = 1L << 27,
        /// <summary>
        /// A small class of visible format controls, which precede and then span a sequence of other characters,
        /// usually digits. These have also been known as "subtending marks", because most of them take a form
        /// which visually extends underneath the sequence of following digits.
        /// </summary>
        PrependedConcatenationMark = 1L << 28,
        /// <summary>
        /// Punctuation characters that function as quotation marks.
        /// </summary>
        QuotationMark = 1L << 29,
        /// <summary>
        /// Used in the definition of Ideographic Description Sequences.
        /// </summary>
        Radical = 1L << 30,
        /// <summary>
        /// Property of the regional indicator characters, U+1F1E6..U+1F1FF. This property is
        /// referenced in various segmentation algorithms, to assist in correct breaking
        /// around emoji flag sequences.
        /// </summary>
        RegionalIndicator = 1L << 31,
        /// <summary>
        /// Punctuation characters that generally mark the end of sentences.
        /// Used in Unicode Standard Annex #29, "Unicode Text Segmentation"
        /// [<seealso href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX29">UAX29</seealso>].
        /// </summary>
        SentenceTerminal = 1L << 32,
        /// <summary>
        /// Characters with a "soft dot", like i or j. An accent placed on these characters causes
        /// the dot to disappear. An explicit dot above can be added where required, such as in Lithuanian.
        /// </summary>
        SoftDotted = 1L << 33,
        /// <summary>
        /// Punctuation characters that generally mark the end of textual units.
        /// These marks are not part of the word preceding them. A notable exception is U+002E FULL STOP.
        /// TerminalPunctuation characters may be part of some larger textual unit that they terminate.
        /// </summary>
        TerminalPunctuation = 1L << 34,
        /// <summary>
        /// A property which specifies the exact set of Unified CJK Ideographs in the standard.
        /// This set excludes CJK Compatibility Ideographs (which have canonical decompositions to
        /// Unified CJK Ideographs), as well as characters from the CJK Symbols and Punctuation block.
        /// The class of Unified_Ideograph=Y characters is a proper subset of the class of Ideographic=Y characters.
        /// </summary>
        UnifiedIdeograph = 1L << 35,
        /// <summary>
        /// Indicates characters that are Variation Selectors. For details on the behavior of these characters,
        /// see Section 23.4, Variation Selectors in [Unicode], and Unicode Technical Standard #37,
        /// "Unicode Ideographic Variation Database" 
        /// [<seealso href="https://www.unicode.org/reports/tr41/tr41-34.html#UTS37">UTS37</seealso>].
        /// </summary>
        VariationSelector = 1L << 36,
        /// <summary>
        /// Spaces, separator characters and other control characters which should be treated by
        /// programming languages as "white space" for the purpose of parsing elements. See also
        /// <seealso href="https://www.unicode.org/reports/tr44/#LineBreak.txt">Line_Break</seealso>,
        /// <seealso href="https://www.unicode.org/reports/tr44/#GraphemeBreakProperty.txt">Grapheme_Cluster_Break</seealso>,
        /// <seealso href="https://www.unicode.org/reports/tr44/#SentenceBreakProperty.txt">Sentence_Break</seealso>,
        /// <seealso href="https://www.unicode.org/reports/tr44/#WordBreakProperty.txt">Word_Break</seealso>,
        /// which classify space characters and related controls somewhat differently for particular
        /// text segmentation contexts.
        /// </summary>
        WhiteSpace = 1L << 37,
        #endregion

        #region Derived properties
        /// <summary>
        /// Characters with the Lowercase property. For more information, see Chapter 4,
        /// Character Properties in [Unicode]. Generated from: LowercaseLetter (Ll) + OtherLowercase
        /// </summary>
        Lowercase = 1L << 38,
        /// <summary>
        /// Characters with the Uppercase property. For more information, see Chapter 4,
        /// Character Properties in [Unicode]. Generated from: UppercaseLetter (Lu) + OtherUppercase
        /// </summary>
        Uppercase = 1L << 39,
        /// <summary>
        /// Characters which are considered to be either uppercase, lowercase or titlecase characters.
        /// This property is not identical to the ChangesWhenCasemapped property.
        /// For more information, see D135 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: Lowercase + Uppercase + TitlecaseLetter (Lt)
        /// </summary>
        Cased = 1L << 40,
        /// <summary>
        /// Characters which are ignored for casing purposes.
        /// For more information, see D136 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: Mn + Me + Cf + Lm + Sk + WordBreak=MidLetter + WordBreak=MidNumLet + WordBreak=SingleQuote
        /// </summary>
        CaseIgnorable = 1L << 41,
        /// <summary>
        /// Characters whose normalized forms are not stable under a toLowercase mapping.
        /// For more information, see D139 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: toLowercase(toNFD(X)) != toNFD(X)
        /// </summary>
        ChangesWhenLowercased = 1L << 42,
        /// <summary>
        /// Characters whose normalized forms are not stable under a toUppercase mapping.
        /// For more information, see D140 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: toUppercase(toNFD(X)) != toNFD(X)
        /// </summary>
        ChangesWhenUppercased = 1L << 43,
        /// <summary>
        /// Characters whose normalized forms are not stable under a toTitlecase mapping.
        /// For more information, see D141 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: toTitlecase(toNFD(X)) != toNFD(X)
        /// </summary>
        ChangesWhenTitlecased = 1L << 44,
        /// <summary>
        /// Characters whose normalized forms are not stable under case folding.
        /// For more information, see D142 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: toCasefold(toNFD(X)) != toNFD(X)
        /// </summary>
        ChangesWhenCasefolded = 1L << 45,
        /// <summary>
        /// Characters which may change when they undergo case mapping.
        /// For more information, see D143 in Section 3.13, Default Case Algorithms in [Unicode].
        /// Generated from: ChangesWhenLowercased(X) or ChangesWhenUppercased(X) or ChangesWhenTitlecased(X)
        /// </summary>
        ChangesWhenCasemapped = 1L << 46,
        /// <summary>
        /// Characters with the Alphabetic property. The use of the contributory Other_Alphabetic property in
        /// the derivation of the Alphabetic property enables the inclusion of various combining marks,
        /// such as dependent vowels in many Indic scripts, which function as basic elements to spell out
        /// words of those writing systems. The Alphabetic property is used in tooling which assigns
        /// default primary weights for characters, for generation of the DUCET table used by the
        /// Unicode Collation Algorithm (UCA).
        /// For more information, see Chapter 4, Character Properties in [Unicode].
        /// Generated from: Lowercase + Uppercase + TitlecaseLetter (Lt) + ModifierLetter (Lm) +
        /// OtherLetter (Lo) + LetterNumber (Nl) + OtherAlphabetic
        /// </summary>
        Alphabetic = 1L << 47,
        /// <summary>
        /// For programmatic determination of default ignorable code points.
        /// New characters that should be ignored in rendering (unless explicitly supported) will be
        /// assigned in these ranges, permitting programs to correctly handle the default rendering of
        /// such characters when not otherwise supported.
        /// For more information, see the FAQ Display of Unsupported Characters, and Section 5.21,
        /// Ignoring Characters in Processing in [Unicode].
        /// Generated from:
        /// Other_Default_Ignorable_Code_Point
        /// + Cf (Format characters)
        /// + Variation_Selector
        /// - White_Space
        /// - FFF9..FFFB (Interlinear annotation format characters)
        /// - 13430..1343F (Egyptian hieroglyph format characters)
        /// - Prepended_Concatenation_Mark (Exceptional format characters that should be visible)
        /// </summary>
        DefaultIgnorableCodePoint = 1L << 48,
        /// <summary>
        /// Property used together with the definition of Standard Korean Syllable Block to define "Grapheme base".
        /// See D58 in Chapter 3, Conformance in [Unicode].
        /// Generated from: [0..10FFFF] - Control (Cc) - Format (Cf) - Surrogate (Cs) - PrivateUse (Co) -
        /// OtherNotAssigned (Cn) - LineSeparator (Zl) - ParagraphSeparator (Zp) - GraphemeExtend
        /// Note: Grapheme_Base is a property of individual characters.
        /// That usage contrasts with "grapheme base", which is an attribute of Unicode strings; a grapheme
        /// base may consist of a Korean syllable which is itself represented by a sequence of conjoining jamos.
        /// </summary>
        GraphemeBase = 1L << 49,
        /// <summary>
        /// Property used to define "Grapheme extender". See D59 in Chapter 3, Conformance in [Unicode].
        /// Generated from: Me + Mn + Other_Grapheme_Extend
        /// Note: The set of characters for which GraphemeExtend=Yes is used in the derivation of the
        /// property value GraphemeClusterBreak=Extend. GraphemeClusterBreak=Extend consists of the set of
        /// characters for which GraphemeExtend=Yes or EmojiModifier=Yes. See
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX29">UAX29</see>] and
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UTS51">UTS51</see>].
        /// </summary>
        GraphemeExtend = 1L << 50,
        /// <summary>
        /// Deprecated in Unicode 5.0.0.
        /// Formerly proposed for programmatic determination of grapheme cluster boundaries.
        /// Generated from: CanonicalCombiningClass=Virama
        /// </summary>
        [Obsolete("Duplication of ccc=9")]
        GraphemeLink = 1L << 51,
        /// <summary>
        /// This property defines values used in Grapheme Cluster Break algorithm in
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX29">UAX29</see>].
        /// Generated as follows:
        /// <para>Define the set of applicable scripts. For Unicode 15.1, the set is defined as
        /// S = [\p{sc=Beng}\p{sc=Deva}\p{sc=Gujr}\p{sc=Mlym}\p{sc=Orya}\p{sc=Telu}]</para>
        /// <para>Then for any character C:</para>
        /// <para>InCB = Linker iff C in [S &amp;\p{Indic_Syllabic_Category=Virama}]</para>
        /// <para>InCB = Consonant iff C in [S &amp;\p{Indic_Syllabic_Category=Consonant}]</para>
        /// <para>InCB = Extend iff C in</para>
        /// <para><code>    [\p{gcb=Extend}
        ///     \p{gcb=ZWJ}
        ///     -\p{InCB=Linker}
        ///     -\p{InCB=Consonant}
        ///     -[\u200C]]</code></para>
        /// <para>Otherwise, InCB = None (the default value)</para>
        /// </summary>
        IndicConjunctBreak = 1L << 52,
        /// <summary>
        /// Characters with the Math property. For more information, see Chapter 4, Character Properties in [Unicode].
        /// Generated from: MathSymbol (Sm) + OtherMath
        /// </summary>
        Math = 1L << 53,
        /// <summary>
        /// Used to determine programming identifiers, as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</see>].
        /// </summary>
        IdStart = 1L << 54,
        /// <summary>
        /// Used to determine programming identifiers, as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</see>].
        /// </summary>
        IdContinue = 1L << 55,
        /// <summary>
        /// Used to determine programming identifiers, as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</see>].
        /// </summary>
        XidStart = 1L << 56,
        /// <summary>
        /// Used to determine programming identifiers, as described in Unicode Standard Annex #31,
        /// "Unicode Identifier and Pattern Syntax"
        /// [<see href="https://www.unicode.org/reports/tr41/tr41-34.html#UAX31">UAX31</see>].
        /// </summary>
        XidContinue = 1L << 57,
        #endregion
    }
}
