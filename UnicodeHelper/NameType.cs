using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Name types
    /// </summary>
    [PublicAPI]
    public enum NameType
    {
        /// <summary>
        /// No name is known
        /// </summary>
        None,
        /// <summary>
        /// A well-known name that is the default name for a codepoint
        /// </summary>
        Base,
        /// <summary>
        /// A few widely used alternate names for format characters
        /// </summary>
        Alternate,
        /// <summary>
        /// Several documented labels for C1 control code points which
        /// were never actually approved in any standard
        /// </summary>
        Figment,
        /// <summary>
        /// Commonly occurring abbreviations (or acronyms) for control codes, format characters,
        /// spaces, and variation selectors
        /// </summary>
        Abbreviation
    }
}
