using System;
using System.Configuration;
using WFTools.Services.Common.Ado.Configuration;

namespace WFTools.Services.Tracking.Ado.Configuration
{
    /// <summary>
    /// Represents configuration for the ADO persistence service.
    /// </summary>
    [Serializable]
    public class TrackingAdoProviderSettings : ConfigurationSection
    {
        private const string configurationSectionName = "wftools.services.tracking.ado";

        /// <summary>
        /// Retrieve the tracking ADO database provider settings.
        /// </summary>
        /// <returns>
        /// <see cref="TrackingAdoProviderSettings" /> containing information from the configuration.
        /// </returns>
        public static TrackingAdoProviderSettings Get()
        {
            return (TrackingAdoProviderSettings)ConfigurationManager.GetSection(configurationSectionName);
        }


        /// <summary>
        /// Available name resolvers for the tracking provider.
        /// </summary>
        [ConfigurationProperty("nameResolvers")]
        [ConfigurationCollection(typeof(ProviderNameTypeMappingCollection))]
        public ProviderNameTypeMappingCollection NameResolvers
        {
            get { return (ProviderNameTypeMappingCollection)base["nameResolvers"]; }
        }

        /// <summary>
        /// Available resource accessors for the tracking provider.
        /// </summary>
        [ConfigurationProperty("resourceAccessors")]
        [ConfigurationCollection(typeof(ProviderNameTypeMappingCollection))]
        public ProviderNameTypeMappingCollection ResourceAccessors
        {
            get { return (ProviderNameTypeMappingCollection)base["resourceAccessors"]; }
        }
    }
}