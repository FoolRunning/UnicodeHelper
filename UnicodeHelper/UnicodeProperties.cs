using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using JetBrains.Annotations;
using UnicodeHelper.Internal;

namespace UnicodeHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class represents the data in the Unicode specification
    /// <see href="https://www.unicode.org/reports/tr44/#PropList.txt">PropList.txt</see> and
    /// <see href="https://www.unicode.org/reports/tr44/#DerivedCoreProperties.txt">DerivedCoreProperties.txt</see>
    /// </remarks>
    [PublicAPI]
    public static class UnicodeProperties
    {
        #region Data fields
        private static readonly UnicodeProperty[] props = new UnicodeProperty[UnicodeData.UnicodeCodepointCount];
        #endregion

        #region Static constructor
        static UnicodeProperties()
        {
            DataHelper.ReadResource("PropList.txt", propsListTextReader =>
            {
                DataHelper.ReadResource("DerivedCoreProperties.txt", derivedPropsDataTextReader =>
                    Init(propsListTextReader, derivedPropsDataTextReader));
            });
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeProperties using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~50ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor

        /// <summary>
        /// Initializes UnicodeProperties using the files specified. The files must be in the default
        /// Unicode standard format for a <c>PropList.txt</c> file and <c>DerivedCoreProperties.txt</c> file.
        /// </summary>
        public static void Init(string propsListDataFilePath, string derivedPropsDataFilePath)
        {
            DataHelper.ReadDataFile(propsListDataFilePath, propsListTextReader =>
            {
                DataHelper.ReadDataFile(derivedPropsDataFilePath, derivedPropsDataTextReader =>
                    Init(propsListTextReader, derivedPropsDataTextReader));
            });
        }

        /// <summary>
        /// Initializes UnicodeBlocks using the specified reader. The data must be in the default
        /// Unicode standard format for a <c>PropList.txt</c> file and <c>DerivedCoreProperties.txt</c> file.
        /// </summary>
        public static void Init(TextReader propsListTextReader, TextReader derivedPropsTextReader)
        {
            // Load Unicode defaults
            for (int i = 0; i < props.Length; i++)
                props[i] = UnicodeProperty.Undefined;
            
            using (CsvReader reader = new CsvReader(propsListTextReader, DataHelper.CsvConfiguration))
            {
                foreach (PropsFileLine line in reader.GetRecords<PropsFileLine>())
                {
                    string name = DataHelper.RemoveTrailingComment(line.PropertyName);
                    UnicodeProperty property = UnicodeConversion.ConvertProperty(name);
                    DataHelper.HandleCodepointRange(line.CodePointRange, codepoint => 
                        props[codepoint] |= property);
                }
            }
            
            using (CsvReader reader = new CsvReader(derivedPropsTextReader, DataHelper.CsvConfiguration))
            {
                foreach (DerivedPropsFileLine line in reader.GetRecords<DerivedPropsFileLine>())
                {
                    string name = DataHelper.RemoveTrailingComment(line.PropertyName);
                    UnicodeProperty property = UnicodeConversion.ConvertProperty(name);
                    DataHelper.HandleCodepointRange(line.CodePointRange, codepoint =>
                    {
                        if (property != UnicodeProperty.IndicConjunctBreak)
                            props[codepoint] |= property;
                        else
                        {
                            // TODO: Figure out how to handle these properties
                            // Cry. :(
                        }
                    });
                }
            }
        }
        #endregion
        
        #region Public methods
        /// <summary>
        /// Gets the properties associated with the specified character
        /// </summary>
        public static UnicodeProperty GetProps(UCodepoint uc)
        {
            return props[(int)uc];
        }
        #endregion
        
        #region Helper methods
        
        #endregion

        #region DerivedPropsFileLine class
        private sealed class DerivedPropsFileLine
        {
            [Index(0)]
            [UsedImplicitly]
            public string CodePointRange { get; set; }

            [Index(1)]
            [UsedImplicitly]
            public string PropertyName { get; set; }

            [Index(2)]
            [UsedImplicitly]
            public string IndicConjunctBreakProperty { get; set; }
        }
        #endregion

        #region PropsFileLine class
        private sealed class PropsFileLine
        {
            [Index(0)]
            [UsedImplicitly]
            public string CodePointRange { get; set; }

            [Index(1)]
            [UsedImplicitly]
            public string PropertyName { get; set; }
        }
        #endregion
    }
}
