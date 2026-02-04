using JetBrains.Annotations;

namespace UnicodeHelper
{
    /// <summary>
    /// Contains name information about a character
    /// </summary>
    [PublicAPI]
    public readonly struct NameInfo
    {
        internal NameInfo(string name, NameType nameType)
        {
            Name = name;
            NameType = nameType;
        }

        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of name that is represented
        /// </summary>
        public NameType NameType { get; }
    }
}
