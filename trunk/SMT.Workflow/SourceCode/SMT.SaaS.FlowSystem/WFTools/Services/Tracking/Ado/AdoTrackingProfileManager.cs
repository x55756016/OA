using System;
using System.Configuration;
using System.Data;
using WFTools.Services.Common.Ado;
using WFTools.Services.Persistence.Ado;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Concrete implementation of <see cref="GenericTrackingProfileManager" />
    /// that uses the ADO.NET provider architecture for managing tracking profiles
    /// using any database supported by ADO.NET.
    /// </summary>
    public class AdoTrackingProfileManager : GenericTrackingProfileManager
    {
        /// <summary>
        /// Construct a new <see cref="AdoTrackingProfileManager" />
        /// with the specified connection string settings.
        /// </summary>
        /// <param name="connectionStringSettings">
        /// Connection string settings for the ADO provider.
        /// </param>
        public AdoTrackingProfileManager(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null)
                throw new ArgumentNullException("connectionStringSettings");

            this.connectionStringSettings = connectionStringSettings;
        }

        /// <summary>
        /// The connection string settings used to initialise the resource provider.
        /// </summary>
        private readonly ConnectionStringSettings connectionStringSettings;

        private ITrackingNameResolver _nameResolver;
        /// <summary>
        /// The active <see cref="ITrackingNameResolver" /> that resolves
        /// names necessary for manipulating the underlying tracking store.
        /// </summary>
        private ITrackingNameResolver nameResolver
        {
            get
            {
                if (_nameResolver == null)
                    _nameResolver = CreateNameResolver();

                return _nameResolver;
            }
        }

        private IAdoValueReader _valueReader;
        /// <summary>
        /// The active <see cref="IAdoValueReader" /> that reads values
        /// from <see cref="IDbCommand" /> and <see cref="IDataReader" /> objects.
        /// </summary>
        private IAdoValueReader valueReader
        {
            get
            {
                if (_valueReader == null)
                    _valueReader = CreateValueReader();

                return _valueReader;
            }
        }

        /// <summary>
        /// Create an <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        /// <remarks>
        /// The resource provider is created once upon Start of the 
        /// tracking service.
        /// </remarks>
        protected override IResourceProvider CreateResourceProvider()
        {
            return AdoResourceProviderFactory.Create(connectionStringSettings);
        }

        /// <summary>
        /// Create an <see cref="ITrackingServiceResourceAccessor" /> that is responsible
        /// for manipulating the underlying tracking store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        protected override ITrackingProfileResourceAccessor CreateAccessor(IResourceProvider resourceProvider)
        {
            IAdoResourceProvider adoResourceProvider = resourceProvider as IAdoResourceProvider;
            if (adoResourceProvider == null)
                throw new ArgumentException(RM.Get_Error_NotIAdoResourceProvider());

            return AdoTrackingResourceAccessorFactory.Create(
                adoResourceProvider, nameResolver, valueReader);
        }

        /// <summary>
        /// Create an <see cref="IPersistenceNameResolver" /> that resolves names
        /// of commands and parameters for the relevant persistence store.
        /// </summary>
        /// <returns>
        /// An implementation of <see cref="IPersistenceNameResolver" /> appropriate for the 
        /// persistence store.
        /// </returns>
        protected virtual ITrackingNameResolver CreateNameResolver()
        {
            return TrackingNameResolverFactory.Create(connectionStringSettings.ProviderName);
        }

        /// <summary>
        /// Create an <see cref="IAdoValueReader" /> that reads values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </summary>
        /// <returns>
        /// An implementation of <see cref="IAdoValueReader" /> appropriate for the 
        /// persistence store.
        /// </returns>
        protected virtual IAdoValueReader CreateValueReader()
        {
            return AdoValueReaderFactory.Create(connectionStringSettings.ProviderName);
        }
    }
}