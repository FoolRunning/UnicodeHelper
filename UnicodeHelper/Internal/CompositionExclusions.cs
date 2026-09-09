using System.Collections.Generic;
using System.IO;

namespace UnicodeHelper.Internal
{
    /// <remarks>This class represents the data in the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#CompositionExclusions.txt">CompositionExclusions.txt</see></remarks>
    /// <remarks>This class is designed to be used and thrown away since the data is only needed during the creation
    /// of the composition mappings.</remarks>
    internal sealed class CompositionExclusions
    {
        #region Constants
        private const int CodepointField = 0;
        private const int FieldCount = 1;
        #endregion

        #region Data fields
        private readonly HashSet<UCodepoint> _exclusions = new HashSet<UCodepoint>();
        #endregion

        #region Constructor
        public CompositionExclusions()
        {
            DataHelper.ReadResource("CompositionExclusions.txt", Load);
        }
        #endregion

        #region Initialization
        private void Load(TextReader textReader)
        {
            foreach (string[] line in DataHelper.ReadDataFile(textReader, FieldCount))
                _exclusions.Add((UCodepoint)DataHelper.ParseHex(line[CodepointField]));
        }
        #endregion

        #region Public methods
        public bool IsExcluded(UCodepoint codepoint)
        {
            return _exclusions.Contains(codepoint);
        }
        #endregion
    }
}
