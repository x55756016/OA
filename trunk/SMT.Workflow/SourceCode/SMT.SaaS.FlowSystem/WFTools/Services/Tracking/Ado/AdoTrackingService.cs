using System;
using System.Configuration;
using System.Data;
using System.Workflow.Runtime.Tracking;
using WFTools.Services.Batching.Ado;
using WFTools.Services.Common.Ado;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Concrete implementation of <see cref="GenericTrackingService" />
    /// that uses the ADO.NET provider architecture for tracking workflow events
    /// any database supported by ADO.NET.
    /// </summary>
    public class AdoTrackingService : GenericTrackingService
    {
        /// <summary>
        /// Construct a new <see cref="AdoTrackingService" /> with the 
        /// specified connection string settings and a default value
        /// for the interval in which to poll for profile changes.
        /// </summary>
        /// <param name="connectionStringSettings">
        /// Connection string settings for the ADO provider.
        /// </param>
        public AdoTrackingService(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null)
                throw new ArgumentNullException("connectionStringSettings");

            this.connectionStringSettings = connectionStringSettings;
        }

        /// <summary>
        /// Construct a new <see cref="AdoTrackingService" /> with the 
        /// specified connection string settings and the interval in which 
        /// to poll for profile changes.
        /// </summary>
        /// <param name="connectionStringSettings">
        /// Connection string settings for the ADO provider.
        /// </param>
        /// <param name="profileChangeInterval">
        /// <see cref="TimeSpan" /> representing the interval to poll for profile
        /// changes.
        /// </param>
        public AdoTrackingService(ConnectionStringSettings connectionStringSettings, TimeSpan profileChangeInterval) : base(profileChangeInterval)
        {
            if (connectionStringSettings == null)
                throw new ArgumentNullException("connectionStringSettings");

            this.connectionStringSettings = connectionStringSettings;
        }

        /// <summary>
        /// The connection string settings used to initialise the resource provider.
        /// </summary>
        private readonly ConnectionStringSettings connectionStringSettings;

        /// <summary>
        /// The active <see cref="ITrackingNameResolver" /> that resolves
        /// names necessary for manipulating the underlying tracking store.
        /// </summary>
        private ITrackingNameResolver nameResolver;

        /// <summary>
        /// The active <see cref="IAdoValueReader" /> that reads values
        /// from <see cref="IDbCommand" /> and <see cref="IDataReader" />
        /// objects.
        /// </summary>
        private IAdoValueReader valueReader;

        /// <summary>
        /// An instance of the <see cref="AdoWorkBatchService" /> or <c>null</c>
        /// if one isn't present in the runtime.
        /// </summary>
        private AdoWorkBatchService workBatchService;

        /// <summary>
        /// Perform startup duties associated with this persistence service.
        /// <remarks>
        /// This implementation calls a virtual method to create a single 
        /// <see cref="ITrackingNameResolver" /> and <see cref="IAdoValueReader" />
        /// for this persistence service.
        /// </remarks>
        /// </summary>
        protected override void Start()
        {
            this.nameResolver = CreateNameResolver();
            this.valueReader = CreateValueReader();
            this.workBatchService = base.Runtime.GetService<AdoWorkBatchService>();

            base.Start();
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
            if (workBatchService != null)
                return workBatchService.CreateResourceProvider(connectionStringSettings);

            return AdoResourceProviderFactory.Create(connectionStringSettings);
        }

        /// <summary>
        /// Create an <see cref="ITrackingNameResolver" /> that resolves names
        /// of commands and parameters for the relevant <see cref="AdoTrackingService" />
        /// persistence store.
        /// </summary>
        /// <returns>
        /// An implementation of <see cref="ITrackingNameResolver" /> appropriate 
        /// for the <see cref="AdoTrackingService" /> tracking store.
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
        /// tracking service's persistence store.
        /// </returns>
        protected virtual IAdoValueReader CreateValueReader()
        {
            return AdoValueReaderFactory.Create(connectionStringSettings.ProviderName);
        }

        /// <summary>
        /// Create an <see cref="ITrackingServiceResourceAccessor" /> that is responsible
        /// for manipulating the underlying tracking store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        protected override ITrackingServiceResourceAccessor CreateAccessor(IResourceProvider resourceProvider)
        {
            IAdoResourceProvider adoResourceProvider = resourceProvider as IAdoResourceProvider;
            if (adoResourceProvider == null)
                throw new ArgumentException(RM.Get_Error_NotIAdoResourceProvider());

            return AdoTrackingResourceAccessorFactory.Create(
                adoResourceProvider, nameResolver, valueReader);
        }

        ///<summary>
        ///Must be overridden in the derived class, and when implemented, returns the channel that the runtime tracking infrastructure uses to send tracking records to the tracking service.
        ///</summary>
        ///
        ///<returns>
        ///The <see cref="T:System.Workflow.Runtime.Tracking.TrackingChannel"></see> that is used to send tracking records to the tracking service.
        ///</returns>
        ///
        ///<param name="parameters">The <see cref="T:System.Workflow.Runtime.Tracking.TrackingParameters"></see> associated with the workflow instance.</param>
        protected override TrackingChannel GetTrackingChannel(TrackingParameters parameters)
        {
            IAdoResourceProvider resourceProvider = ResourceProvider as IAdoResourceProvider;
            if (resourceProvider == null)
                throw new ArgumentException(RM.Get_Error_NotIAdoResourceProvider());

            return new AdoTrackingChannel(resourceProvider, valueReader, nameResolver, parameters);
        }
    }
}
