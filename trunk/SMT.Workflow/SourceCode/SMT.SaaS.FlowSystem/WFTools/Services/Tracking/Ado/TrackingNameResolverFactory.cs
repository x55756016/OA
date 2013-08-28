using WFTools.Services.Common.Ado.Configuration;
using WFTools.Services.Tracking.Ado.Configuration;
using WFTools.Utilities;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Factory for creating <see cref="ITrackingNameResolver" /> implementations.
    /// </summary>
    public static class TrackingNameResolverFactory
    {
        /// <summary>
        /// Given a provider name locate the necessary <see cref="ITrackingNameResolver" /> 
        /// in the configuration file.
        /// </summary>
        /// <param name="providerName">
        /// The name that uniquely identifies an ADO.NET provider.
        /// </param>
        /// <returns>
        /// An <see cref="ITrackingNameResolver" />.
        /// </returns>
        public static ITrackingNameResolver Create(string providerName)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = TrackingAdoProviderSettings.Get()
                    .NameResolvers.FindByProviderName(providerName);

            ITrackingNameResolver nameResolver = null;
            if (mapping != null)
                nameResolver = TypeUtilities.CreateInstance<ITrackingNameResolver>(mapping.Type);

            return nameResolver;
        }
    }
}
