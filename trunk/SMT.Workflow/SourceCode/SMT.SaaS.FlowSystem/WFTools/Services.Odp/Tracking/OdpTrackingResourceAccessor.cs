using System;
using System.Collections.Generic;
using System.Data;
using System.Transactions;
using System.Workflow.ComponentModel;
using Oracle.DataAccess.Client;
using WFTools.Services.Common.Ado;
using WFTools.Services.Common.State;
using WFTools.Services.Tracking;
using WFTools.Services.Tracking.Ado;
using WFTools.Services.Tracking.Entity;

namespace WFTools.Services.Odp.Tracking
{
    /// <summary>
    /// ODP.NET specific implementation of <see cref="AdoTrackingResourceAccessor" />
    /// that makes use of the array binding feature in order to batch up data.
    /// </summary>
    public class OdpTrackingResourceAccessor : AdoTrackingResourceAccessor
    {
        /// <summary>
        /// Construct a new <see cref="OdpTrackingResourceAccessor" /> with the
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
        /// </summary>
        public OdpTrackingResourceAccessor(IAdoResourceProvider resourceProvider, 
            ITrackingNameResolver nameResolver, IAdoValueReader valueReader, 
            Transaction transaction, IStateProvider stateProvider) : 
            base(resourceProvider, nameResolver, valueReader, transaction, stateProvider) { }

        /// <summary>
        /// Construct a new <see cref="OdpTrackingResourceAccessor" /> with the
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
        public OdpTrackingResourceAccessor(IAdoResourceProvider resourceProvider, 
            ITrackingNameResolver nameResolver, IAdoValueReader valueReader) : 
            base(resourceProvider, nameResolver, valueReader) { }

        /// <summary>
        /// The number of activity tracking records to batch up when persisting.
        /// </summary>
        protected override Int32 ActivityTrackingBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of activity tracking records, returning them
        /// as an <see cref="IDictionary{TKey,TValue}" /> indexed by their
        /// unique identifiers.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="activityTrackingRecords">
        /// An <see cref="IList{T}" /> containing a batch of activity tracking 
        /// records to be inserted into the tracking store.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{TKey,TValue}" /> containing the inserted
        /// records and indexed by their unique identifiers.
        /// </returns>
        protected override IDictionary<Int64, SerialisableActivityTrackingRecord> InsertActivityTrackingRecordBatch(
            WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableActivityTrackingRecord> activityTrackingRecords)
        {
            if (activityTrackingRecords.Count == 0 || activityTrackingRecords.Count > ActivityTrackingBatchSize)
                throw new ArgumentOutOfRangeException("activityTrackingRecords");

            Dictionary<Int64, SerialisableActivityTrackingRecord> activityTrackingRecordsById =
                new Dictionary<Int64, SerialisableActivityTrackingRecord>();

            using (OracleCommand oracleCommand = (OracleCommand) CreateCommand("WORKFLOW_TRACKING_PKG.InsertActivityTrackingRecord", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[activityTrackingRecords.Count];
                Int64?[] activityInstanceIds = new Int64?[activityTrackingRecords.Count];
                String[] activityIdentifiers = new String[activityTrackingRecords.Count];
                String[] qualifiedNames = new String[activityTrackingRecords.Count];
                Guid[] contextGuids = new Guid[activityTrackingRecords.Count];
                Guid[] parentContextGuids = new Guid[activityTrackingRecords.Count];
                Int16[] executionStates = new Int16[activityTrackingRecords.Count];
                DateTime[] eventDateTimes = new DateTime[activityTrackingRecords.Count];
                Int32[] eventOrders = new Int32[activityTrackingRecords.Count];

                for (Int32 i = 0 ; i < activityTrackingRecords.Count ; i++)
                {
                    SerialisableActivityTrackingRecord activityTrackingRecord = activityTrackingRecords[i];

                    workflowInstanceIds[i] = (Int64) workflowInstanceSummary.InternalId;
                    activityIdentifiers[i] = BuildActivityIdentifier(activityTrackingRecord);
                    activityInstanceIds[i] = FindActivityInstanceInternalId(activityIdentifiers[i]);
                    qualifiedNames[i] = activityTrackingRecord.QualifiedName;
                    contextGuids[i] = activityTrackingRecord.ContextGuid;
                    parentContextGuids[i] = activityTrackingRecord.ParentContextGuid;
                    executionStates[i] = (Int16)activityTrackingRecord.ExecutionStatus;
                    eventDateTimes[i] = activityTrackingRecord.EventDateTime;
                    eventOrders[i] = activityTrackingRecord.EventOrder;
                }

                oracleCommand.ArrayBindCount = activityTrackingRecords.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                OracleParameter activityInstanceId = (OracleParameter)AddParameter(
                    oracleCommand, "p_ACTIVITY_INSTANCE_ID", activityInstanceIds, 
                    AdoDbType.Int64, ParameterDirection.InputOutput);
                AddParameter(oracleCommand, "p_QUALIFIED_NAME", qualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_CONTEXT_GUID", contextGuids, AdoDbType.Guid);
                AddParameter(oracleCommand, "p_PARENT_CONTEXT_GUID", parentContextGuids, AdoDbType.Guid);
                AddParameter(oracleCommand, "p_ACTIVITY_STATUS_ID", executionStates, AdoDbType.Int16);
                AddParameter(oracleCommand, "p_EVENT_DATE_TIME", eventDateTimes, AdoDbType.DateTime);
                AddParameter(oracleCommand, "p_EVENT_ORDER", eventOrders, AdoDbType.Int32);
                OracleParameter activityStatusEventId = (OracleParameter)AddParameter(
                    oracleCommand, "p_ACTIVITY_STATUS_EVENT_ID", AdoDbType.Int64, 
                    ParameterDirection.Output);

                oracleCommand.ExecuteNonQuery();

                Int64[] populatedActivityInstanceIds = (Int64[]) activityInstanceId.Value;
                Int64[] activityStatusEventIds = (Int64[])activityStatusEventId.Value;
                for (Int32 i = 0; i < activityTrackingRecords.Count; i++)
                {
                    if (activityTrackingRecords[i].ExecutionStatus == ActivityExecutionStatus.Closed)
                        RemoveActivityInstanceInternalId(activityIdentifiers[i]);
                    else
                        UpdateActivityInstanceInternalId(activityIdentifiers[i], populatedActivityInstanceIds[i]);

                    activityTrackingRecordsById.Add(activityStatusEventIds[i], activityTrackingRecords[i]);
                }
            }


            return activityTrackingRecordsById;
        }

        /// <summary>
        /// The number of user tracking records to batch up when persisting.
        /// </summary>
        protected override Int32 UserTrackingBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of user tracking records, returning them
        /// as an <see cref="IDictionary{TKey,TValue}" /> indexed by their
        /// unique identifiers.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="userTrackingRecords">
        /// An <see cref="IList{T}" /> containing a batch of user tracking 
        /// records to be inserted into the tracking store.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{TKey,TValue}" /> containing the inserted
        /// records and indexed by their unique identifiers.
        /// </returns>
        protected override IDictionary<Int64, SerialisableUserTrackingRecord> InsertUserTrackingRecordBatch(
            WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableUserTrackingRecord> userTrackingRecords)
        {
            if (userTrackingRecords.Count == 0 || userTrackingRecords.Count > UserTrackingBatchSize)
                throw new ArgumentOutOfRangeException("userTrackingRecords");

            Dictionary<Int64, SerialisableUserTrackingRecord> userTrackingRecordsById = new Dictionary<Int64, SerialisableUserTrackingRecord>();

            using (OracleCommand oracleCommand = (OracleCommand)CreateCommand("WORKFLOW_TRACKING_PKG.InsertUserTrackingRecord", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[userTrackingRecords.Count];
                Int64?[] activityInstanceIds = new Int64?[userTrackingRecords.Count];
                String[] activityIdentifiers = new String[userTrackingRecords.Count];
                String[] qualifiedNames = new String[userTrackingRecords.Count];
                Guid[] contextGuids = new Guid[userTrackingRecords.Count];
                Guid[] parentContextGuids = new Guid[userTrackingRecords.Count];
                DateTime[] eventDateTimes = new DateTime[userTrackingRecords.Count];
                Int32[] eventOrders = new Int32[userTrackingRecords.Count];
                String[] userDataKeys = new String[userTrackingRecords.Count];
                String[] userDataTypeNames = new String[userTrackingRecords.Count];
                String[] userDataAssemblyNames = new String[userTrackingRecords.Count];
                String[] userDataStrings = new String[userTrackingRecords.Count];
                Byte[][] userDataBlobs = new Byte[userTrackingRecords.Count][];
                Boolean[] userDataNonSerialisables = new Boolean[userTrackingRecords.Count];

                for (Int32 i = 0; i < userTrackingRecords.Count; i++)
                {
                    SerialisableUserTrackingRecord userTrackingRecord = userTrackingRecords[i];

                    workflowInstanceIds[i] = (Int64) workflowInstanceSummary.InternalId;
                    activityIdentifiers[i] = BuildActivityIdentifier(userTrackingRecord);
                    activityInstanceIds[i] = FindActivityInstanceInternalId(activityIdentifiers[i]);
                    qualifiedNames[i] = userTrackingRecord.QualifiedName;
                    contextGuids[i] = userTrackingRecord.ContextGuid;
                    parentContextGuids[i] = userTrackingRecord.ParentContextGuid;
                    eventDateTimes[i] = userTrackingRecord.EventDateTime;
                    eventOrders[i] = userTrackingRecord.EventOrder;
                    userDataKeys[i] = userTrackingRecord.UserDataKey;
                    userDataTypeNames[i] = userTrackingRecord.UserData.Type.FullName;
                    userDataAssemblyNames[i] = userTrackingRecord.UserData.Type.Assembly.FullName;
                    userDataStrings[i] = userTrackingRecord.UserData.StringData;
                    userDataBlobs[i] = userTrackingRecord.UserData.SerialisedData;
                    userDataNonSerialisables[i] = userTrackingRecord.UserData.NonSerialisable;
                }

                oracleCommand.ArrayBindCount = userTrackingRecords.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                OracleParameter activityInstanceId = (OracleParameter)AddParameter(
                    oracleCommand, "p_ACTIVITY_INSTANCE_ID", activityInstanceIds, 
                    AdoDbType.Int64, ParameterDirection.InputOutput);
                AddParameter(oracleCommand, "p_QUALIFIED_NAME", qualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_CONTEXT_GUID", contextGuids, AdoDbType.Guid);
                AddParameter(oracleCommand, "p_PARENT_CONTEXT_GUID", parentContextGuids, AdoDbType.Guid);
                AddParameter(oracleCommand, "p_EVENT_DATE_TIME", eventDateTimes, AdoDbType.DateTime);
                AddParameter(oracleCommand, "p_EVENT_ORDER", eventOrders, AdoDbType.Int32);
                AddParameter(oracleCommand, "p_USER_DATA_KEY", userDataKeys, AdoDbType.String);
                AddParameter(oracleCommand, "p_USER_DATA_TYPE_NAME", userDataTypeNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_USER_DATA_ASSEMBLY_NAME", userDataAssemblyNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_USER_DATA_STR", userDataStrings, AdoDbType.String);
                AddParameter(oracleCommand, "p_USER_DATA_BLOB", userDataBlobs, AdoDbType.Binary);
                AddParameter(oracleCommand, "p_USER_DATA_NON_SERIALISABLE", userDataNonSerialisables, AdoDbType.Boolean);
                OracleParameter userEventId = (OracleParameter)AddParameter(
                    oracleCommand, "p_USER_EVENT_ID", AdoDbType.Int64, 
                    ParameterDirection.Output);

                oracleCommand.ExecuteNonQuery();

                Int64[] populatedActivityInstanceIds = (Int64[]) activityInstanceId.Value;
                Int64[] userEventIds = (Int64[]) userEventId.Value;
                for (Int32 i = 0; i < userTrackingRecords.Count; i++)
                {
                    UpdateActivityInstanceInternalId(activityIdentifiers[i], populatedActivityInstanceIds[i]);
                    userTrackingRecordsById.Add(userEventIds[i], userTrackingRecords[i]);
                }
            }

            return userTrackingRecordsById;
        }

        /// <summary>
        /// The number of workflow tracking records to batch up when persisting.
        /// </summary>
        protected override Int32 WorkflowTrackingBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of workflow tracking records, returning them
        /// as an <see cref="IDictionary{TKey,TValue}" /> indexed by their
        /// unique identifiers.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="workflowTrackingRecords">
        /// An <see cref="IList{T}" /> containing a batch of workflow tracking 
        /// records to be inserted into the tracking store.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{TKey,TValue}" /> containing the inserted
        /// records and indexed by their unique identifiers.
        /// </returns>
        protected override IDictionary<Int64, SerialisableWorkflowTrackingRecord> InsertWorkflowTrackingRecordBatch(
            WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableWorkflowTrackingRecord> workflowTrackingRecords)
        {
            if (workflowTrackingRecords.Count == 0 || workflowTrackingRecords.Count > WorkflowTrackingBatchSize)
                throw new ArgumentOutOfRangeException("workflowTrackingRecords");

            Dictionary<Int64, SerialisableWorkflowTrackingRecord> workflowTrackingRecordsById =
                new Dictionary<Int64, SerialisableWorkflowTrackingRecord>();

            using (OracleCommand oracleCommand = (OracleCommand)CreateCommand("WORKFLOW_TRACKING_PKG.InsertWorkflowTrackingRecord", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[workflowTrackingRecords.Count];
                Int16[] statusIds = new Int16[workflowTrackingRecords.Count];
                DateTime[] eventDateTimes = new DateTime[workflowTrackingRecords.Count];
                Int32[] eventOrders = new Int32[workflowTrackingRecords.Count];
                String[] eventArgTypeNames = new String[workflowTrackingRecords.Count];
                String[] eventArgAssemblyNames = new String[workflowTrackingRecords.Count];
                Byte[][] eventArgs = new Byte[workflowTrackingRecords.Count][];

                for (Int32 i = 0; i < workflowTrackingRecords.Count; i++)
                {
                    SerialisableWorkflowTrackingRecord workflowTrackingRecord = workflowTrackingRecords[i];

                    workflowInstanceIds[i] = (Int64)workflowInstanceSummary.InternalId;
                    statusIds[i] = (Int16)workflowTrackingRecord.TrackingWorkflowEvent;
                    eventDateTimes[i] = workflowTrackingRecord.EventDateTime;
                    eventOrders[i] = workflowTrackingRecord.EventOrder;
                    if (workflowTrackingRecord.EventArgs != null)
                    {
                        eventArgTypeNames[i] = workflowTrackingRecord.EventArgs.Type.FullName;
                        eventArgAssemblyNames[i] = workflowTrackingRecord.EventArgs.Type.Assembly.FullName;
                        eventArgs[i] = workflowTrackingRecord.EventArgs.SerialisedData;
                    }
                }

                oracleCommand.ArrayBindCount = workflowTrackingRecords.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_STATUS", statusIds, AdoDbType.Int16);
                AddParameter(oracleCommand, "p_EVENT_DATE_TIME", eventDateTimes, AdoDbType.DateTime);
                AddParameter(oracleCommand, "p_EVENT_ORDER", eventOrders, AdoDbType.Int32);
                AddParameter(oracleCommand, "p_EVENT_ARG_TYPE_NAME", eventArgTypeNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_EVENT_ARG_ASSEMBLY_NAME", eventArgAssemblyNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_EVENT_ARG", eventArgs, AdoDbType.Binary);
                OracleParameter workflowEventId = (OracleParameter)AddParameter(
                    oracleCommand, "p_WORKFLOW_INSTANCE_EVENT_ID", AdoDbType.Int64,
                    ParameterDirection.Output);

                oracleCommand.ExecuteNonQuery();

                Int64[] workflowEventIds = (Int64[]) workflowEventId.Value;
                for (Int32 i = 0; i < workflowTrackingRecords.Count; i++)
                    workflowTrackingRecordsById.Add(workflowEventIds[i], workflowTrackingRecords[i]);
            }

            return workflowTrackingRecordsById;
        }

        /// <summary>
        /// The number of workflow change tracking records to batch up when persisting.
        /// </summary>
        protected override Int32 WorkflowChangeBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of workflow change tracking records, returning them
        /// as an <see cref="IDictionary{TKey,TValue}" /> indexed by their
        /// unique identifiers.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="workflowChangeRecords">
        /// An <see cref="IList{T}" /> containing a batch of workflow change 
        /// records to be inserted into the tracking store.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{TKey,TValue}" /> containing the inserted
        /// records and indexed by their unique identifiers.
        /// </returns>
        protected override IDictionary<Int64, SerialisableWorkflowChangeRecord> InsertWorkflowChangeRecordBatch(
            WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableWorkflowChangeRecord> workflowChangeRecords)
        {
            if (workflowChangeRecords.Count == 0 || workflowChangeRecords.Count > WorkflowChangeBatchSize)
                throw new ArgumentOutOfRangeException("workflowChangeRecords");

            Dictionary<Int64, SerialisableWorkflowChangeRecord> workflowChangeRecordsById =
                new Dictionary<Int64, SerialisableWorkflowChangeRecord>();

            using (OracleCommand oracleCommand = (OracleCommand)CreateCommand("WORKFLOW_TRACKING_PKG.InsertWorkflowTrackingRecord", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[workflowChangeRecords.Count];
                Int16[] statusIds = new Int16[workflowChangeRecords.Count];
                DateTime[] eventDateTimes = new DateTime[workflowChangeRecords.Count];
                Int32[] eventOrders = new Int32[workflowChangeRecords.Count];
                String[] eventArgTypeNames = new String[workflowChangeRecords.Count];
                String[] eventArgAssemblyNames = new String[workflowChangeRecords.Count];
                Byte[][] eventArgs = new Byte[workflowChangeRecords.Count][];

                for (Int32 i = 0; i < workflowChangeRecords.Count; i++)
                {
                    SerialisableWorkflowChangeRecord workflowChangeRecord = workflowChangeRecords[i];

                    workflowInstanceIds[i] = (Int64)workflowInstanceSummary.InternalId;
                    statusIds[i] = (Int16)workflowChangeRecord.TrackingWorkflowEvent;
                    eventDateTimes[i] = workflowChangeRecord.EventDateTime;
                    eventOrders[i] = workflowChangeRecord.EventOrder;
                    if (workflowChangeRecord.EventArgs != null)
                    {
                        eventArgTypeNames[i] = workflowChangeRecord.EventArgs.Type.FullName;
                        eventArgAssemblyNames[i] = workflowChangeRecord.EventArgs.Type.Assembly.FullName;
                        eventArgs[i] = workflowChangeRecord.EventArgs.SerialisedData;
                    }
                }

                oracleCommand.ArrayBindCount = workflowChangeRecords.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_STATUS", statusIds, AdoDbType.Int16);
                AddParameter(oracleCommand, "p_EVENT_DATE_TIME", eventDateTimes, AdoDbType.DateTime);
                AddParameter(oracleCommand, "p_EVENT_ORDER", eventOrders, AdoDbType.Int32);
                AddParameter(oracleCommand, "p_EVENT_ARG_TYPE_NAME", eventArgTypeNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_EVENT_ARG_ASSEMBLY_NAME", eventArgAssemblyNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_EVENT_ARG", eventArgs, AdoDbType.Binary);
                OracleParameter workflowEventId = (OracleParameter)AddParameter(
                    oracleCommand, "p_WORKFLOW_INSTANCE_EVENT_ID", AdoDbType.Int64,
                    ParameterDirection.Output);

                oracleCommand.ExecuteNonQuery();

                Int64[] workflowEventIds = (Int64[])workflowEventId.Value;
                for (Int32 i = 0; i < workflowChangeRecords.Count; i++)
                    workflowChangeRecordsById.Add(workflowEventIds[i], workflowChangeRecords[i]);
            }

            return workflowChangeRecordsById;
        }

        /// <summary>
        /// The number of activity added actions to batch up when persisting.
        /// </summary>
        protected override Int32 ActivityAddedActionBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of activity added actions.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="addedActions">
        /// An <see cref="IList{T}" /> containing a batch of activity added
        /// actions to be inserted into the tracking store.
        /// </param>
        protected override void InsertActivityAddedActionBatch(WorkflowInstanceSummary workflowInstanceSummary,
            IList<KeyValuePair<Int64, SerialisableActivityAddedAction>> addedActions)
        {
            if (addedActions.Count == 0 || addedActions.Count > ActivityAddedActionBatchSize)
                throw new ArgumentOutOfRangeException("addedActions");

            using (OracleCommand oracleCommand = (OracleCommand) CreateCommand("WORKFLOW_TRACKING_PKG.InsertActivityAddedActions", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[addedActions.Count];
                Int64[] workflowEventIds = new Int64[addedActions.Count];
                String[] qualifiedNames = new String[addedActions.Count];
                String[] typeFullNames = new String[addedActions.Count];
                String[] assemblyFullNames = new String[addedActions.Count];
                String[] parentQualifiedNames = new String[addedActions.Count];
                String[] addedActivityXoml = new String[addedActions.Count];
                Int32[] order = new Int32[addedActions.Count];

                for (int i = 0; i < addedActions.Count; i++)
                {
                    SerialisableActivityAddedAction addedAction = addedActions[i].Value;

                    workflowInstanceIds[i] = (Int64) workflowInstanceSummary.InternalId;
                    workflowEventIds[i] = addedActions[i].Key;
                    qualifiedNames[i] = addedAction.QualifiedName;
                    typeFullNames[i] = addedAction.ActivityType.FullName;
                    assemblyFullNames[i] = addedAction.ActivityType.Assembly.FullName;
                    parentQualifiedNames[i] = addedAction.ParentQualifiedName;
                    addedActivityXoml[i] = addedAction.ActivityXoml;
                    order[i] = addedAction.Order;
                }

                oracleCommand.ArrayBindCount = addedActions.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_EVENT_ID", workflowEventIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_QUALIFIED_NAME", qualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_TYPE_FULL_NAME", typeFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_ASSEMBLY_FULL_NAME", assemblyFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_PARENT_QUALIFIED_NAME", parentQualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_ADDED_ACTIVITY_ACTION", addedActivityXoml, AdoDbType.String);
                AddParameter(oracleCommand, "p_ORDER", order, AdoDbType.Int32);

                oracleCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// The number of activity removed actions to batch up when persisting.
        /// </summary>
        protected override Int32 ActivityRemovedActionBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of activity removed actions.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="removedActions">
        /// An <see cref="IList{T}" /> containing a batch of activity removed
        /// actions to be inserted into the tracking store.
        /// </param>
        protected override void InsertActivityRemovedActionBatch(WorkflowInstanceSummary workflowInstanceSummary,
            IList<KeyValuePair<Int64, SerialisableActivityRemovedAction>> removedActions)
        {
            if (removedActions.Count == 0 || removedActions.Count > ActivityAddedActionBatchSize)
                throw new ArgumentOutOfRangeException("removedActions");

            using (OracleCommand oracleCommand = (OracleCommand)CreateCommand("WORKFLOW_TRACKING_PKG.InsertActivityRemovedActions", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[removedActions.Count];
                Int64[] workflowEventIds = new Int64[removedActions.Count];
                String[] qualifiedNames = new String[removedActions.Count];
                String[] typeFullNames = new String[removedActions.Count];
                String[] assemblyFullNames = new String[removedActions.Count];
                String[] parentQualifiedNames = new String[removedActions.Count];
                String[] removedActivityXoml = new String[removedActions.Count];
                Int32[] order = new Int32[removedActions.Count];

                for (int i = 0; i < removedActions.Count; i++)
                {
                    SerialisableActivityRemovedAction removedAction = removedActions[i].Value;

                    workflowInstanceIds[i] = (Int64)workflowInstanceSummary.InternalId;
                    workflowEventIds[i] = removedActions[i].Key;
                    qualifiedNames[i] = removedAction.QualifiedName;
                    typeFullNames[i] = removedAction.ActivityType.FullName;
                    assemblyFullNames[i] = removedAction.ActivityType.Assembly.FullName;
                    parentQualifiedNames[i] = removedAction.ParentQualifiedName;
                    removedActivityXoml[i] = removedAction.ActivityXoml;
                    order[i] = removedAction.Order;
                }

                oracleCommand.ArrayBindCount = removedActions.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_EVENT_ID", workflowEventIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_QUALIFIED_NAME", qualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_TYPE_FULL_NAME", typeFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_ASSEMBLY_FULL_NAME", assemblyFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_PARENT_QUALIFIED_NAME", parentQualifiedNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_REMOVED_ACTIVITY_ACTION", removedActivityXoml, AdoDbType.String);
                AddParameter(oracleCommand, "p_ORDER", order, AdoDbType.Int32);

                oracleCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// The number of event annotations to batch up when persisting.
        /// </summary>
        protected override Int32 EventAnnotationBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of event annotations.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="type">
        /// The type of tracking record to associate the annotations with.
        /// </param>
        /// <param name="annotations">
        /// An <see cref="IList{T}" /> containing a batch of annotations
        /// to be inserted into the tracking store.
        /// </param>
        protected override void InsertEventAnnotationBatch(WorkflowInstanceSummary workflowInstanceSummary,
            TrackingRecordType type, IList<KeyValuePair<Int64, String>> annotations)
        {
            if (annotations.Count == 0 || annotations.Count > EventAnnotationBatchSize)
                throw new ArgumentOutOfRangeException("annotations");

            using (OracleCommand oracleCommand = (OracleCommand) CreateCommand("WORKFLOW_TRACKING_PKG.InsertEventAnnotation", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[annotations.Count];
                Int64[] eventIds = new Int64[annotations.Count];
                String[] eventTypes = new String[annotations.Count];
                String[] eventAnnotations = new String[annotations.Count];

                String eventType = null;
                switch (type)
                {
                    case TrackingRecordType.Activity:
                        eventType = "A";
                        break;
                    case TrackingRecordType.User:
                        eventType = "U";
                        break;
                    case TrackingRecordType.Workflow:
                        eventType = "W";
                        break;
                }

                for (int i = 0; i < annotations.Count; i++)
                {
                    workflowInstanceIds[i] = (Int64) workflowInstanceSummary.InternalId;
                    eventIds[i] = annotations[i].Key;
                    eventTypes[i] = eventType;
                    eventAnnotations[i] = annotations[i].Value;
                }

                oracleCommand.ArrayBindCount = annotations.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_EVENT_ID", eventIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_EVENT_TYPE", eventTypes, AdoDbType.String);
                AddParameter(oracleCommand, "p_ANNOTATION", eventAnnotations, AdoDbType.String);

                oracleCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// The number of tracking data items to batch up when persisting.
        /// </summary>
        protected override Int32 TrackingDataItemBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of tracking data items.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="type">
        /// The type of tracking record to associate the tracking data items with.
        /// </param>
        /// <param name="trackingDataItems">
        /// An <see cref="IList{T}" /> containing a batch of tracking data items
        /// to be inserted into the tracking store.
        /// </param>
        /// <returns>
        /// An <see cref="IDictionary{TKey,TValue}" /> containing the inserted
        /// records and indexed by their unique identifiers.
        /// </returns>
        protected override IDictionary<Int64, SerialisableTrackingDataItem> InsertTrackingDataItemBatch(
            WorkflowInstanceSummary workflowInstanceSummary, TrackingRecordType type,
            IList<KeyValuePair<Int64, SerialisableTrackingDataItem>> trackingDataItems)
        {
            if (trackingDataItems.Count == 0 || trackingDataItems.Count > TrackingDataItemBatchSize)
                throw new ArgumentOutOfRangeException("trackingDataItems");

            Dictionary<Int64, SerialisableTrackingDataItem> trackingDataItemsById = new Dictionary<Int64, SerialisableTrackingDataItem>();

            using (OracleCommand oracleCommand = (OracleCommand) CreateCommand("WORKFLOW_TRACKING_PKG.InsertTrackingDataItem", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[trackingDataItems.Count];
                Int64[] eventIds = new Int64[trackingDataItems.Count];
                String[] eventTypes = new String[trackingDataItems.Count];
                String[] fieldNames = new String[trackingDataItems.Count];
                String [] typeFullNames = new String[trackingDataItems.Count];
                String[] assemblyFullNames = new String[trackingDataItems.Count];
                String[] dataStrings = new String[trackingDataItems.Count];
                Byte[][] dataBlobs = new Byte[trackingDataItems.Count][];
                Boolean[] dataNonSerialisable = new Boolean[trackingDataItems.Count];
                
                String eventType = null;
                switch (type)
                {
                    case TrackingRecordType.Activity:
                        eventType = "A";
                        break;
                    case TrackingRecordType.User:
                        eventType = "U";
                        break;
                    case TrackingRecordType.Workflow:
                        eventType = "W";
                        break;
                }

                for (int i = 0; i < trackingDataItems.Count; i ++)
                {
                    SerialisableTrackingDataItem trackingDataItem = trackingDataItems[i].Value;

                    workflowInstanceIds[i] = (Int64) workflowInstanceSummary.InternalId;
                    eventIds[i] = trackingDataItems[i].Key;
                    eventTypes[i] = eventType;
                    fieldNames[i] = trackingDataItem.FieldName;
                    typeFullNames[i] = trackingDataItem.Data.Type.FullName;
                    assemblyFullNames[i] = trackingDataItem.Data.Type.Assembly.FullName;
                    dataStrings[i] = trackingDataItem.Data.StringData;
                    dataBlobs[i] = trackingDataItem.Data.SerialisedData;
                    dataNonSerialisable[i] = trackingDataItem.Data.NonSerialisable;
                }

                oracleCommand.ArrayBindCount = trackingDataItems.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_EVENT_ID", eventIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_EVENT_TYPE", eventTypes, AdoDbType.String);
                AddParameter(oracleCommand, "p_FIELD_NAME", fieldNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_TYPE_FULL_NAME", typeFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_ASSEMBLY_FULL_NAME", assemblyFullNames, AdoDbType.String);
                AddParameter(oracleCommand, "p_DATA_STR", dataStrings, AdoDbType.String);
                AddParameter(oracleCommand, "p_DATA_BLOB", dataBlobs, AdoDbType.Binary);
                AddParameter(oracleCommand, "p_DATA_NON_SERIALISABLE", dataNonSerialisable, AdoDbType.Boolean);
                OracleParameter trackingDataItemId = (OracleParameter) AddParameter(
                    oracleCommand, "p_TRACKING_DATA_ITEM_ID", AdoDbType.Int64,
                    ParameterDirection.Output);

                oracleCommand.ExecuteNonQuery();

                Int64[] trackingDataItemIds = (Int64[]) trackingDataItemId.Value;
                for (int i = 0; i < trackingDataItems.Count; i++)
                    trackingDataItemsById.Add(trackingDataItemIds[i], trackingDataItems[i].Value);
            }

            return trackingDataItemsById;
        }

        /// <summary>
        /// The number of tracking data item annotations to batch up when persisting.
        /// </summary>
        protected override Int32 TrackingDataItemAnnotationBatchSize
        {
            get { return 10; }
        }

        /// <summary>
        /// Inserts a batch of tracking data item annotations.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// aInt64 with all its type information.
        /// </param>
        /// <param name="annotations">
        /// An <see cref="IList{T}" /> containing a batch of annotations
        /// to be inserted into the tracking store.
        /// </param>
        protected override void InsertTrackingDataItemAnnotationBatch(WorkflowInstanceSummary workflowInstanceSummary,
            IList<KeyValuePair<Int64, String>> annotations)
        {
            if (annotations.Count == 0 || annotations.Count > EventAnnotationBatchSize)
                throw new ArgumentOutOfRangeException("annotations");

            using (OracleCommand oracleCommand = (OracleCommand)CreateCommand("WORKFLOW_TRACKING_PKG.InsertTrackingDataAnnotation", CommandType.StoredProcedure))
            {
                Int64[] workflowInstanceIds = new Int64[annotations.Count];
                Int64[] trackingDataItemIds = new Int64[annotations.Count];
                String[] trackingDataAnnotations = new String[annotations.Count];

                for (int i = 0; i < annotations.Count; i++)
                {
                    workflowInstanceIds[i] = (Int64)workflowInstanceSummary.InternalId;
                    trackingDataItemIds[i] = annotations[i].Key;
                    trackingDataAnnotations[i] = annotations[i].Value;
                }

                oracleCommand.ArrayBindCount = annotations.Count;

                AddParameter(oracleCommand, "p_WORKFLOW_INSTANCE_ID", workflowInstanceIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_TRACKING_DATA_ITEM_ID", trackingDataItemIds, AdoDbType.Int64);
                AddParameter(oracleCommand, "p_ANNOTATION", trackingDataAnnotations, AdoDbType.String);

                oracleCommand.ExecuteNonQuery();
            }
        }
    }
}