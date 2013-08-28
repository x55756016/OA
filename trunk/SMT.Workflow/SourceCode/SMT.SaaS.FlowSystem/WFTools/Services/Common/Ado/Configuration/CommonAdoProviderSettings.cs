using System;
using System.Configuration;

namespace WFTools.Services.Common.Ado.Configuration
{
    /// <summary>
    /// Represents common configuration for ADO database providers.
    /// </summary>
    [Serializable]
    public class CommonAdoProviderSettings : ConfigurationSection
    {
        private const string configurationSectionName = "wftools.services.common.ado";

        /// <summary>
        /// Retrieve the common ADO database provider settings.
        /// </summary>
        /// <returns>
        /// <see cref="CommonAdoProviderSettings" /> containing information from the configuration.
        /// </returns>
        public static CommonAdoProviderSettings Get()
        {
            return (CommonAdoProviderSettings)ConfigurationManager.GetSection(configurationSectionName);
        }

        /// <summary>
        /// Available resource providers for ADO.NET providers.
        /// </summary>
        [ConfigurationProperty("resourceProviders")]
        [ConfigurationCollection(typeof(ProviderNameTypeMappingCollection))]
        public ProviderNameTypeMappingCollection ResourceProviders
        {
            get { return (ProviderNameTypeMappingCollection)base["resourceProviders"]; }
        }

        /// <summary>
        /// Available value readers for ADO.NET providers.
        /// </summary>
        [ConfigurationProperty("valueReaders")]
        [ConfigurationCollection(typeof(ProviderNameTypeMappingCollection))]
        public ProviderNameTypeMappingCollection ValueReaders
        {
            get { return (ProviderNameTypeMappingCollection)base["valueReaders"]; }
        }
    }
}