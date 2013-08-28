using System;
using System.Configuration;
using WFTools.Services.Common.Ado.Configuration;

namespace WFTools.Services.Persistence.Ado.Configuration
{
    /// <summary>
    /// Represents configuration for the ADO persistence service.
    /// </summary>
    [Serializable]
    public class PersistenceAdoProviderSettings : ConfigurationSection
    {
        private const string configurationSectionName = "wftools.services.persistence.ado";

        /// <summary>
        /// Retrieve the persistence ADO database provider settings.
        /// </summary>
        /// <returns>
        /// <see cref="PersistenceAdoProviderSettings" /> containing information from the configuration.
        /// </returns>
        public static PersistenceAdoProviderSettings Get()
        {
            return (PersistenceAdoProviderSettings)ConfigurationManager.GetSection(configurationSectionName);
        }


        /// <summary>
        /// Available name resolvers for the persistence provider.
        /// </summary>
        [ConfigurationProperty("nameResolvers")]
        [ConfigurationCollection(typeof(ProviderNameTypeMappingCollection))]
        public ProviderNameTypeMappingCollection NameResolvers
        {
            get { return (ProviderNameTypeMappingCollection)base["nameResolvers"]; }
        }
    }
}