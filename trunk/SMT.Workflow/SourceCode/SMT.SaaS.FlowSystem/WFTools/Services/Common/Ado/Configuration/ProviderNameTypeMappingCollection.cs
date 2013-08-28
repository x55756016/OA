using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace WFTools.Services.Common.Ado.Configuration
{
    /// <summary>
    /// Strongly-typed collection of <see cref="ProviderNameTypeMapping" />
    /// objects.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface",
        Justification = "This class is derived from ConfigurationElementCollection, doesn't need to be generic.")]
    public class ProviderNameTypeMappingCollection : ConfigurationElementCollection
    {
        ///<summary>
        ///When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A new <see cref="T:System.Configuration.ConfigurationElement"></see>.
        ///</returns>
        ///
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderNameTypeMapping();
        }

        ///<summary>
        ///Gets the element key for a specified configuration element when overridden in a derived class.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Object"></see> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"></see>.
        ///</returns>
        ///
        ///<param name="element">The <see cref="T:System.Configuration.ConfigurationElement"></see> to return the key for. </param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderNameTypeMapping)element).ProviderName;
        }

        /// <summary>
        /// Locate the specified provider->type mappings by name.
        /// </summary>
        public ProviderNameTypeMapping FindByProviderName(string providerName)
        {
            for (int i = 0; i < Count; i++)
            {
                ProviderNameTypeMapping mapping = (ProviderNameTypeMapping)BaseGet(i);
                if (string.Compare(mapping.ProviderName, providerName, true, CultureInfo.InvariantCulture) == 0)
                    return mapping;
            }

            return null;
        }
    }
}