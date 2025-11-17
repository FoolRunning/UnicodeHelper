using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;

namespace UnicodeHelper.Internal
{
    /// <remarks>This class represents the data in the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#CompositionExclusions.txt">CompositionExclusions.txt</see></remarks>
    /// <remarks>This class is designed to be used and thrown away since the data is only needed during the creation
    /// of the composition mappings.</remarks>
    internal sealed class CompositionExclusions
    {
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
            using (CsvReader reader = new CsvReader(textReader, DataHelper.CsvConfiguration))
            {
                foreach (ExclusionFileLine line in reader.GetRecords<ExclusionFileLine>())
                {
                    string codepoint = line.Codepoint;
                    int commentIndex = codepoint.IndexOf('#');
                    if (commentIndex >= 0)
                        codepoint = codepoint.Substring(0, commentIndex).Trim();
                    
                    _exclusions.Add(UCodepoint.FromHexStr(codepoint));
                }
            }
        }
        #endregion
        
        #region Public methods
        public bool IsExcluded(UCodepoint codepoint)
        {
            return _exclusions.Contains(codepoint);
        }
        #endregion

        #region ExclusionFileLine class
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private sealed class ExclusionFileLine
        {
            [Index(0)]
            public string Codepoint { get; set; }
        }
        #endregion
    }
}
