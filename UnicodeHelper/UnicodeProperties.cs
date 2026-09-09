using System.IO;
using System.Runtime.CompilerServices;
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
        #region Constants
        private const int CodePointRangeField = 0;
        private const int PropertyNameField = 1;
        private const int PropsFileFieldCount = 2;
        private const int DerivedPropsFileFieldCount = 3; // Third field is the Indic_Conjunct_Break value
        #endregion

        #region Data fields
        private static readonly UnicodeProperty[] props;
        #endregion

        #region Static constructor
        static UnicodeProperties()
        {
            UnicodeProperty[] loadedProps = null;
            DataHelper.ReadResource("PropList.txt", propsListTextReader =>
            {
                DataHelper.ReadResource("DerivedCoreProperties.txt", derivedPropsDataTextReader =>
                    loadedProps = Load(propsListTextReader, derivedPropsDataTextReader));
            });
            props = loadedProps;
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes UnicodeProperties using the built-in data.
        /// </summary>
        /// <remarks>Note that this initializer is not strictly needed. Any call to a method on the
        /// class will initialize it. Since initialization can take a relatively long time (~30ms),
        /// this method is provided for convenience in case an application needs to initialize at
        /// a particular moment (e.g. while a progress bar is showing).</remarks>
        public static void Init() { } // Just invokes the static constructor

        /// <summary>
        /// Loads the property data using the specified readers. The data must be in the default
        /// Unicode standard format for a <c>PropList.txt</c> file and <c>DerivedCoreProperties.txt</c> file.
        /// </summary>
        /// <remarks>The per-codepoint work is deliberately a plain loop rather than a per-codepoint callback.
        /// While a class's static constructor is running, every call into a method of that class goes through
        /// a class-initialization check (~80ns), which made a callback-per-codepoint design several times
        /// slower than the actual work.</remarks>
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        private static UnicodeProperty[] Load(TextReader propsListTextReader, TextReader derivedPropsTextReader)
        {
            // Unicode default (UnicodeProperty.Undefined == 0) is already the array default
            UnicodeProperty[] result = new UnicodeProperty[UnicodeData.UnicodeCodepointCount];

            foreach (string[] line in DataHelper.ReadDataFile(propsListTextReader, PropsFileFieldCount))
            {
                UnicodeProperty property = UnicodeConversion.ConvertProperty(line[PropertyNameField]);
                AddProperty(result, line[CodePointRangeField], property);
            }

            foreach (string[] line in DataHelper.ReadDataFile(derivedPropsTextReader, DerivedPropsFileFieldCount))
            {
                UnicodeProperty property = UnicodeConversion.ConvertProperty(line[PropertyNameField]);
                if (property == UnicodeProperty.IndicConjunctBreak)
                {
                    // TODO: Figure out how to handle these properties
                    // Cry. :(
                    continue;
                }

                AddProperty(result, line[CodePointRangeField], property);
            }

            return result;
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
        [MethodImpl(HelperUtils.AggressiveOptimization)]
        private static void AddProperty(UnicodeProperty[] props, string codePointRange, UnicodeProperty property)
        {
            DataHelper.ParseCodepointRange(codePointRange, out int startCodePoint, out int endCodePoint);
            for (int c = startCodePoint; c <= endCodePoint; c++)
                props[c] |= property;
        }
        #endregion
    }
}
