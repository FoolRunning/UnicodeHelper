using JetBrains.Annotations;

namespace UnicodeHelper
{
    [PublicAPI]
    public sealed class NameInfo
    {
        public NameInfo(string name, string abbreviation)
        {
            Name = name;
            Abbreviation = abbreviation;
        }

        public string Name { get; internal set; }

        public string Abbreviation { get; internal set; }
    }
}
