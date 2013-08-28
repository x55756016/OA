using System.Data;
using System.Transactions;
using WFTools.Services.Common.Ado;
using WFTools.Services.Common.State;

namespace WFTools.Services.Tracking.Ado.Sql
{
    /// <summary>
    /// SQL Server specific implementation of <see cref="AdoTrackingResourceAccessor" />.
    /// </summary>
    /// <remarks>
    /// This implementation simply overrides some of the batch sizes as
    /// the default SQL Server stored procedures provided by MS tend to
    /// use smaller batch sizes than we use for the other providers.
    /// </remarks>
    public class SqlTrackingResourceAccessor : AdoTrackingResourceAccessor
    {
        /// <summary>
        /// Construct a new <see cref="SqlTrackingResourceAccessor" /> with the
        /// specified <see cref="IAdoResourceProvider" />, 
        /// <see cref="ITrackingNameResolver" />, <see cref="IAdoValueReader" /> 
        /// and <see cref="IStateProvider" />.
        /// </summary>
        /// <param name="resourceProvider">
        /// An <see cref="IAdoResourceProvider" /> used to provide resources for
        /// accessing the tracking store.
        /// </param>
        /// <param name="nameResolver">
        /// An <see cref="ITrackingNameResolver" /> that resolves names
        /// of commands and parameters for the relevant tracking store.
        /// </param>
        /// <param name="valueReader">
        /// An <see cref="IAdoValueReader" /> that reads values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </param>
        public SqlTrackingResourceAccessor(IAdoResourceProvider resourceProvider,
            ITrackingNameResolver nameResolver, IAdoValueReader valueReader) : base(
            resourceProvider, nameResolver, valueReader) { }

        /// <summary>
        /// Construct a new <see cref="SqlTrackingResourceAccessor" /> with the
        /// specified <see cref="IAdoResourceProvider" />, 
        /// <see cref="ITrackingNameResolver" />, <see cref="IAdoValueReader" /> 
        /// and <see cref="IStateProvider" />. All work should be performed in
        /// the specified <see cref="Transaction" />.
        /// </summary>
        /// <param name="resourceProvider">
        /// An <see cref="IAdoResourceProvider" /> used to provide resources for
        /// accessing the tracking store.
        /// </param>
        /// <param name="nameResolver">
        /// An <see cref="ITrackingNameResolver" /> that resolves names
        /// of commands and parameters for the relevant tracking store.
        /// </param>
        /// <param name="valueReader">
        /// An <see cref="IAdoValueReader" /> that reads values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </param>
        /// <param name="transaction">
        /// An <see cref="Transaction" /> in which to perform the work.
        /// </param>
        /// <param name="stateProvider">
        /// An <see cref="IStateProvider" /> that can be used to store state.
        /// </param>
        public SqlTrackingResourceAccessor(IAdoResourceProvider resourceProvider,
            ITrackingNameResolver nameResolver, IAdoValueReader valueReader,
            Transaction transaction, IStateProvider stateProvider) : base(
            resourceProvider, nameResolver, valueReader, transaction, stateProvider) { }

        /// <summary>
        /// The number of user tracking records to batch up when persisting.
        /// </summary>
        /// <remarks>
        /// The default SQL Server stored procedure only supports 
        /// 1 user record at a time.
        /// </remarks>
        protected override int UserTrackingBatchSize
        {
            get { return 1; }
        }

        /// <summary>
        /// The number of workflow tracking records to batch up when persisting.
        /// </summary>
        /// <remarks>
        /// The default SQL Server stored procedure only supports 
        /// 2 workflow tracking records at a time.
        /// </remarks>
        protected override int WorkflowTrackingBatchSize
        {
            get { return 2; }
        }

        /// <summary>
        /// The number of workflow change tracking records to batch up when persisting.
        /// </summary>
        /// <remarks>
        /// The default SQL Server stored procedure only supports 
        /// 2 workflow change records at a time.
        /// </remarks>
        protected override int WorkflowChangeBatchSize
        {
            get { return 2; }
        }

        /// <summary>
        /// The number of activity added actions to batch up when persisting.
        /// </summary>
        /// <remarks>
        /// The default SQL Server stored procedure only supports 
        /// 1 activity added action at a time.
        /// </remarks>
        protected override int ActivityAddedActionBatchSize
        {
            get { return 1; }
        }

        /// <summary>
        /// The number of activity removed actions to batch up when persisting.
        /// </summary>
        /// <remarks>
        /// The default SQL Server stored procedure only supports 
        /// 1 activity removed action at a time.
        /// </remarks>
        protected override int ActivityRemovedActionBatchSize
        {
            get { return 1; }
        }
    }
}