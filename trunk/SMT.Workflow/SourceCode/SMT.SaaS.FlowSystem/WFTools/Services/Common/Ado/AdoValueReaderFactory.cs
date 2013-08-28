using WFTools.Services.Common.Ado.Configuration;
using WFTools.Utilities;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// Factory for creating <see cref="IAdoValueReader" /> implementations.
    /// </summary>
    public static class AdoValueReaderFactory
    {
        /// <summary>
        /// Given a section name and provider name locate the necessary 
        /// <see cref="IAdoValueReader" /> in the configuration file.
        /// </summary>
        /// <param name="providerName">
        /// The name that uniquely identifies an ADO.NET provider.
        /// </param>
        /// <returns>
        /// An <see cref="IAdoValueReader" />.
        /// </returns>
        public static IAdoValueReader Create(string providerName)
        {
            // locate any mappings for the specified provider
            ProviderNameTypeMapping mapping = CommonAdoProviderSettings.Get()
                .ValueReaders.FindByProviderName(providerName);

            IAdoValueReader valueReader;
            if (mapping != null)
                valueReader = TypeUtilities.CreateInstance<IAdoValueReader>(mapping.Type);
            else
            {
                // no value reader mapping found in config
                // return the default value reader
                valueReader = new DefaultAdoValueReader();
            }

            return valueReader;
        }
    }
}