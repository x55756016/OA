using System.Transactions;
using WFTools.Services.Common.Ado;
using WFTools.Services.Common.Ado.Configuration;
using WFTools.Services.Common.State;
using WFTools.Services.Tracking.Ado.Configuration;
using WFTools.Utilities;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Factory for creating <see cref="AdoTrackingResourceAccessor" /> implementations.
    /// </summary>
    public static class AdoTrackingResourceAccessorFactory
    {
        /// <summary>
        /// Given a provider name locate the necessary 
        /// <see cref="AdoTrackingResourceAccessor" /> in the configuration file.
        /// </summary>
        /// <returns>
        /// An <see cref="AdoTrackingResourceAccessor" />.
        /// </returns>
        public static AdoTrackingResourceAccessor Create(IAdoResourceProvider resourceProvider, 
            ITrackingNameResolver nameResolver, IAdoValueReader valueReader)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = TrackingAdoProviderSettings.Get()
                    .ResourceAccessors.FindByProviderName(resourceProvider.ProviderName);

            AdoTrackingResourceAccessor resourceAccessor;
            if (mapping != null)
            {
                resourceAccessor =
                    TypeUtilities.CreateInstance<AdoTrackingResourceAccessor>(
                        mapping.Type, new object[]
                            {
                                resourceProvider, nameResolver, valueReader
                            });
            }
            else
            {
                resourceAccessor = new AdoTrackingResourceAccessor(
                    resourceProvider, nameResolver, valueReader);
            }

            return resourceAccessor;
        }

        /// <summary>
        /// Given a provider name locate the necessary 
        /// <see cref="AdoTrackingResourceAccessor" /> in the configuration file.
        /// </summary>
        /// <returns>
        /// An <see cref="AdoTrackingResourceAccessor" />.
        /// </returns>
        public static AdoTrackingResourceAccessor Create(
            IAdoResourceProvider resourceProvider, ITrackingNameResolver nameResolver,
            IAdoValueReader valueReader, Transaction transaction, IStateProvider stateProvider)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = TrackingAdoProviderSettings.Get()
                    .ResourceAccessors.FindByProviderName(resourceProvider.ProviderName);

            AdoTrackingResourceAccessor resourceAccessor;
            if (mapping != null)
            {
                resourceAccessor =
                    TypeUtilities.CreateInstance<AdoTrackingResourceAccessor>(
                        mapping.Type, new object[]
                            {
                                resourceProvider, nameResolver, valueReader,
                                transaction, stateProvider
                            });
            }
            else
            {
                return new AdoTrackingResourceAccessor(
                    resourceProvider, nameResolver, valueReader,
                    transaction, stateProvider);
            }

            return resourceAccessor;
        }
    }
}
