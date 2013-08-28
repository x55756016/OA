using System;
using System.Data;
using System.Transactions;
using System.Workflow.Runtime.Tracking;
using WFTools.Services.Common.Ado;
using WFTools.Services.Common.State;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Concrete implementation of <see cref="GenericTrackingChannel" />
    /// that uses the ADO.NET provider architecture for tracking workflow events
    /// using any database supported by ADO.NET.
    /// </summary>
    public class AdoTrackingChannel : GenericTrackingChannel
    {
        /// <summary>
        /// Create an instance of the <see cref="AdoTrackingChannel" />
        /// with the specified <see cref="IAdoResourceProvider" />, <see cref="ITrackingNameResolver" />
        /// and <see cref="IAdoValueReader" />.
        /// </summary>
        /// <param name="resourceProvider">
        /// An <see cref="IAdoResourceProvider" /> that provides resources
        /// for manipulating the underlying tracking store.
        /// </param>
        /// <param name="valueReader">
        /// An <see cref="IAdoValueReader" /> that is responsible for
        /// reading values from the underlying tracking store.
        /// </param>
        /// <param name="nameResolver">
        /// An <see cref="ITrackingNameResolver" /> that is responsible for
        /// resolving names for the underlying tracking store.
        /// </param>
        /// <param name="trackingParameters">
        /// <see cref="TrackingParameters" /> from the <see cref="TrackingService" />.
        /// </param>
        public AdoTrackingChannel(IAdoResourceProvider resourceProvider, 
            IAdoValueReader valueReader, ITrackingNameResolver nameResolver,
            TrackingParameters trackingParameters) : base(resourceProvider, 
            trackingParameters)
        {
            if (valueReader == null)
                throw new ArgumentNullException("valueReader");

            if (nameResolver == null)
                throw new ArgumentNullException("nameResolver");

            this.valueReader = valueReader;
            this.nameResolver = nameResolver;
        }

        /// <summary>
        /// The active <see cref="ITrackingNameResolver" /> that resolves
        /// names necessary for manipulating the underlying persistence store.
        /// </summary>
        private readonly ITrackingNameResolver nameResolver;

        /// <summary>
        /// The active <see cref="IAdoValueReader" /> that reads values
        /// from <see cref="IDbCommand" /> and <see cref="IDataReader" />
        /// objects.
        /// </summary>
        private readonly IAdoValueReader valueReader;

        /// <summary>
        /// An <see cref="IStateProvider" /> that can be used by an accessor
        /// for storing state related to this channel.
        /// </summary>
        private readonly IStateProvider stateProvider = new InMemoryStateProvider();

        /// <summary>
        /// Create an <see cref="ITrackingChannelResourceAccessor" /> that is responsible
        /// for manipulating the underlying tracking store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        /// <param name="transaction">
        /// Transaction to perform operations within.
        /// </param>
        protected override ITrackingChannelResourceAccessor CreateAccessor(IResourceProvider resourceProvider, Transaction transaction)
        {
            IAdoResourceProvider adoResourceProvider = resourceProvider as IAdoResourceProvider;
            if (adoResourceProvider == null)
                throw new ArgumentException(RM.Get_Error_NotIAdoResourceProvider());

            return AdoTrackingResourceAccessorFactory.Create(
                adoResourceProvider, nameResolver, valueReader,
                transaction, stateProvider);
        }
    }
}