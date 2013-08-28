using WFTools.Services.Common.Ado.Configuration;
using WFTools.Services.Persistence.Ado.Configuration;
using WFTools.Utilities;

namespace WFTools.Services.Persistence.Ado
{
    /// <summary>
    /// Factory for creating <see cref="IPersistenceNameResolver" /> implementations.
    /// </summary>
    public static class PersistenceNameResolverFactory
    {
        /// <summary>
        /// Given a provider name locate the necessary <see cref="IPersistenceNameResolver" /> 
        /// in the configuration file.
        /// </summary>
        /// <param name="providerName">
        /// The name that uniquely identifies an ADO.NET provider.
        /// </param>
        /// <returns>
        /// An <see cref="IPersistenceNameResolver" />.
        /// </returns>
        public static IPersistenceNameResolver Create(string providerName)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = PersistenceAdoProviderSettings.Get()
                    .NameResolvers.FindByProviderName(providerName);

            IPersistenceNameResolver nameResolver = null;
            if (mapping != null)
                nameResolver = TypeUtilities.CreateInstance<IPersistenceNameResolver>(mapping.Type);

            return nameResolver;
        }
    }
}
