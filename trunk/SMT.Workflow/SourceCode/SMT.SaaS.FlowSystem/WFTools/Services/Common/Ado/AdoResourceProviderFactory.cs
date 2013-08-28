using System.Configuration;
using WFTools.Services.Common.Ado.Configuration;
using WFTools.Utilities;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// Factory for creating <see cref="IAdoResourceProvider" /> implementations.
    /// </summary>
    public static class AdoResourceProviderFactory
    {
        /// <summary>
        /// Given a provider name either locate the necessary <see cref="IAdoResourceProvider" /> 
        /// in the configuration file or return the default (<see cref="DefaultAdoResourceProvider" />).
        /// </summary>
        /// <param name="connectionStringSettings">
        /// The name that uniquely identifies an ADO.NET provider.
        /// </param>
        /// <returns>
        /// An <see cref="IAdoResourceProvider" />.
        /// </returns>
        public static IAdoResourceProvider Create(ConnectionStringSettings connectionStringSettings)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = CommonAdoProviderSettings.Get()
                .ResourceProviders.FindByProviderName(connectionStringSettings.ProviderName);

            IAdoResourceProvider resourceProvider = null;
            if (mapping != null)
                resourceProvider = TypeUtilities.CreateInstance<IAdoResourceProvider>(mapping.Type);

            if (resourceProvider == null)
            {
                // no resource provider mapping found in config
                // return the default resource provider
                resourceProvider = new DefaultAdoResourceProvider();
            }

            resourceProvider.Initialise(connectionStringSettings);

            return resourceProvider;
        }
    }
}