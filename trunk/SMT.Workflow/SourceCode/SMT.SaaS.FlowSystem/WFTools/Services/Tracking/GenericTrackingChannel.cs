using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Transactions;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Tracking;
using WFTools.Services.Tracking.Entity;
using WFTools.Utilities;
using WFTools.Utilities.Diagnostics;
using WFTools.Utilities.Workflow;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Abstract implementation of <see cref="TrackingChannel" /> that provides the
    /// framework necessary for a very generic tracking service. 
    /// 
    /// A resource provider and accessor work hand-in-hand to actually
    /// persist workflow tracking information to the tracking store.
    /// </summary>
    public abstract class GenericTrackingChannel : TrackingChannel, IPendingWork
    {
        /// <summary>
        /// Construct a <see cref="GenericTrackingChannel" /> using
        /// the specified <see cref="IResourceProvider" /> for providing
        /// resources necessary for manipulating the underlying persistence store.
        /// </summary>
        /// <param name="resourceProvider">
        /// A <see cref="IResourceProvider" /> responsible for providing
        /// resources necessary for manipulating the underlying persistence store.
        /// </param>
        /// <param name="trackingParameters">
        /// <see cref="TrackingParameters" /> from the <see cref="TrackingService" />.
        /// </param>
        protected GenericTrackingChannel(IResourceProvider resourceProvider, TrackingParameters trackingParameters)
        {
            if (resourceProvider == null)
                throw new ArgumentNullException("resourceProvider");

            if (trackingParameters == null)
                throw new ArgumentNullException("trackingParameters");

            this.resourceProvider = resourceProvider;
            this.trackingParameters = trackingParameters;
        }

        /// <summary>
        /// The active <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        private readonly IResourceProvider resourceProvider;

        /// <summary>
        /// Parameters passed to us from a <see cref="TrackingService" />.
        /// </summary>
        private readonly TrackingParameters trackingParameters;

        /// <summary>
        /// The <see cref="WorkflowInstanceSummary" /> that represents details
        /// of the workflow instance with which this tracking channel
        /// is associated.
        /// </summary>
        private WorkflowInstanceSummary workflowInstanceSummary;

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
        protected abstract ITrackingChannelResourceAccessor CreateAccessor(IResourceProvider resourceProvider, Transaction transaction);

        ///<summary>
        ///When implemented in a derived class, sends a <see cref="T:System.Workflow.Runtime.Tracking.TrackingRecord"></see> on the <see cref="T:System.Workflow.Runtime.Tracking.TrackingChannel"></see>.
        ///</summary>
        ///
        ///<param name="record">The <see cref="T:System.Workflow.Runtime.Tracking.TrackingRecord"></see> to send.</param>
        protected override void Send(TrackingRecord record)
        {
            TraceHelper.Trace();

            try
            {
                // build a serialisable representation of the tracking record
                // and batch it up ready for persistence during commit
                ActivityTrackingRecord activityTrackingRecord = record as ActivityTrackingRecord;
                if (activityTrackingRecord != null)
                {
                    WorkflowEnvironment.WorkBatch.Add(this,
                        buildSerialisableActivityTrackingRecord(
                            activityTrackingRecord));

                    return;
                }

                UserTrackingRecord userTrackingRecord = record as UserTrackingRecord;
                if (userTrackingRecord != null)
                {
                    WorkflowEnvironment.WorkBatch.Add(this,
                        buildSerialisableUserTrackingRecord(
                            userTrackingRecord));

                    return;
                }

                WorkflowTrackingRecord workflowTrackingRecord = record as WorkflowTrackingRecord;
                if (workflowTrackingRecord != null)
                {
                    if (workflowTrackingRecord.TrackingWorkflowEvent == TrackingWorkflowEvent.Changed)
                    {
                        WorkflowEnvironment.WorkBatch.Add(this,
                            buildSerialisableWorkflowChangeTrackingRecord(
                                workflowTrackingRecord));
                    }
                    else
                    {
                        WorkflowEnvironment.WorkBatch.Add(this,
                            buildSerialisableWorkflowTrackingRecord(
                                workflowTrackingRecord));
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingChannelException(e.ToString());

                TraceHelper.Trace(errorMessage);

                throw new TrackingException(errorMessage, e);
            }
        }

        ///<summary>
        ///When implemented in a derived class, notifies a receiver of data on the tracking channel that the workflow instance associated with the tracking channel has either completed or terminated.
        ///</summary>
        ///
        protected override void InstanceCompletedOrTerminated()
        {
            TraceHelper.Trace();
        }

        ///<summary>
        ///Allows the items in the work batch to assert whether they must commit immediately.
        ///</summary>
        ///
        ///<returns>
        ///true if any item in the collection must be committed immediately; otherwise false.
        ///</returns>
        ///
        ///<param name="items">An <see cref="T:System.Collections.ICollection"></see> of work items.</param>
        public bool MustCommit(ICollection items)
        {
            TraceHelper.Trace();

            return false;
        }

        ///<summary>
        ///Commits the list of work items by using the specified <see cref="T:System.Transactions.Transaction"></see> object.
        ///</summary>
        ///
        ///<param name="items">The work items to be committed.</param>
        ///<param name="transaction">The <see cref="T:System.Transactions.Transaction"></see> associated with the pending work.</param>
        public void Commit(Transaction transaction, ICollection items)
        {
            TraceHelper.Trace();

            try
            {
                using (ITrackingChannelResourceAccessor resourceAccessor = CreateAccessor(resourceProvider, transaction))
                {
                    if (this.workflowInstanceSummary == null)
                        this.workflowInstanceSummary = resourceAccessor.InsertOrGetWorkflowInstance(buildWorkflowInstanceSummary());

                    List<SerialisableActivityTrackingRecord> activityTrackingRecords = new List<SerialisableActivityTrackingRecord>();
                    List<SerialisableUserTrackingRecord> userTrackingRecords = new List<SerialisableUserTrackingRecord>();
                    List<SerialisableWorkflowChangeRecord> workflowChangeRecords = new List<SerialisableWorkflowChangeRecord>();
                    List<SerialisableWorkflowTrackingRecord> workflowTrackingRecords = new List<SerialisableWorkflowTrackingRecord>();

                    // group each type of tracking record ready for processing
                    foreach (object itemToCommit in items)
                    {
                        if (!(itemToCommit is TrackingRecord))
                            continue;

                        SerialisableActivityTrackingRecord activityTrackingRecord = itemToCommit as SerialisableActivityTrackingRecord;
                        if (activityTrackingRecord != null)
                        {
                            activityTrackingRecords.Add(activityTrackingRecord);
                            continue;
                        }

                        SerialisableUserTrackingRecord userTrackingRecord = itemToCommit as SerialisableUserTrackingRecord;
                        if (userTrackingRecord != null)
                        {
                            userTrackingRecords.Add(userTrackingRecord);
                            continue;
                        }

                        SerialisableWorkflowChangeRecord workflowChangeRecord = itemToCommit as SerialisableWorkflowChangeRecord;
                        if (workflowChangeRecord != null)
                        {
                            workflowChangeRecords.Add(workflowChangeRecord);
                            continue;
                        }

                        SerialisableWorkflowTrackingRecord workflowTrackingRecord = itemToCommit as SerialisableWorkflowTrackingRecord;
                        if (workflowTrackingRecord != null)
                        {
                            workflowTrackingRecords.Add(workflowTrackingRecord);
                            continue;
                        }
                    }

                    // send each category of record off to the resource accessor
                    if (activityTrackingRecords.Count > 0)
                        resourceAccessor.InsertTrackingRecords(this.workflowInstanceSummary, activityTrackingRecords);

                    if (userTrackingRecords.Count > 0)
                        resourceAccessor.InsertTrackingRecords(this.workflowInstanceSummary, userTrackingRecords);

                    if (workflowTrackingRecords.Count > 0)
                        resourceAccessor.InsertTrackingRecords(this.workflowInstanceSummary, workflowTrackingRecords);

                    if (workflowChangeRecords.Count > 0)
                        resourceAccessor.InsertTrackingRecords(this.workflowInstanceSummary, workflowChangeRecords);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_TrackingChannelException(e.ToString());

                TraceHelper.Trace(errorMessage);

                throw new TrackingException(errorMessage, e);
            }
        }

        ///<summary>
        ///Called when the transaction has completed.
        ///</summary>
        ///
        ///<param name="items">An <see cref="T:System.Collections.ICollection"></see> of work items.</param>
        ///<param name="succeeded">true if the transaction succeeded; otherwise, false.</param>
        public void Complete(bool succeeded, ICollection items)
        {
            TraceHelper.Trace();
        }

        /// <summary>
        /// Build a summary of the workflow instance that this channel represents.
        /// </summary>
        /// <returns>
        /// <see cref="WorkflowInstanceSummary" /> representing the workflow 
        /// instance.
        /// </returns>
        private WorkflowInstanceSummary buildWorkflowInstanceSummary()
        {
            bool isXomlWorkflow = WorkflowInstanceHelper.IsXomlWorkflow(
                this.trackingParameters.RootActivity);
            string xomlDocument = WorkflowInstanceHelper.GetXomlDocument(
                this.trackingParameters.RootActivity);

            WorkflowInstanceSummary workflowInstanceSummary = new WorkflowInstanceSummary(
                this.trackingParameters.InstanceId, isXomlWorkflow,
                xomlDocument, this.trackingParameters.ContextGuid,
                this.trackingParameters.CallerInstanceId,
                this.trackingParameters.CallerContextGuid,
                this.trackingParameters.CallerParentContextGuid,
                buildCallPath());

            workflowInstanceSummary.WorkflowType = new WorkflowTypeSummary(
                this.trackingParameters.WorkflowType);

            workflowInstanceSummary.RootActivity = buildRootActivitySummary();

            return workflowInstanceSummary;
        }

        /// <summary>
        /// Build a concatenated string of the activity qualified names
        /// that were called to get the workflow instance in its current state.
        /// </summary>
        /// <returns>
        /// String representation of the call path for the workflow instance
        /// represented by this tracking channel.
        /// </returns>
        private string buildCallPath()
        {
            IList<string> callPathList = this.trackingParameters.CallPath;
            if (callPathList == null || callPathList.Count == 0)
                return string.Empty;

            string [] callPath = new string[callPathList.Count];
            for (int i = 0; i < callPathList.Count; i++)
                callPath[i] = callPathList[i];

            return string.Join(".", callPath);
        }

        /// <summary>
        /// Build a tree of activity summaries starting with the root activity
        /// of the workflow instance that this channel represents.
        /// </summary>
        /// <returns>
        /// <see cref="ActivitySummary" /> representing the root activity.
        /// </returns>
        private ActivitySummary buildRootActivitySummary()
        {
            ActivitySummary rootActivitySummary = null;

            Dictionary<string, ActivitySummary> activitySummariesByQualifiedName = new Dictionary<string, ActivitySummary>();
            Queue<Activity> activityQueue = new Queue<Activity>();
            activityQueue.Enqueue(this.trackingParameters.RootActivity);

            while (activityQueue.Count > 0)
            {
                Activity currentActivity = activityQueue.Dequeue();
                ActivitySummary currentActivitySummary = new ActivitySummary(
                    currentActivity.GetType(), currentActivity.QualifiedName);

                if (currentActivity.Parent != null && activitySummariesByQualifiedName.ContainsKey(currentActivity.Parent.QualifiedName))
                {
                    ActivitySummary parentActivitySummary = activitySummariesByQualifiedName[currentActivity.Parent.QualifiedName];
                    currentActivitySummary.ParentActivity = parentActivitySummary;
                    parentActivitySummary.ChildActivities.Add(currentActivitySummary);
                }

                if (rootActivitySummary == null)
                    rootActivitySummary = currentActivitySummary;

                CompositeActivity parentActivity = currentActivity as CompositeActivity;
                if (parentActivity != null)
                {
                    activitySummariesByQualifiedName.Add(currentActivitySummary.QualifiedName, currentActivitySummary);
                    foreach (Activity childActivity in parentActivity.Activities)
                    {
                        if (childActivity.Enabled)
                            activityQueue.Enqueue(childActivity);
                    }
                }
            }

            return rootActivitySummary;
        }

        /// <summary>
        /// Serialise a <see cref="ActivityTrackingRecord" /> ready for persistence.
        /// </summary>
        /// <param name="activityTrackingRecord">
        /// The original <see cref="ActivityTrackingRecord" /> to serialise.
        /// </param>
        /// <returns>
        /// Modified copy of the <see cref="ActivityTrackingRecord" /> containing
        /// the serialised data.
        /// </returns>
        private static SerialisableActivityTrackingRecord buildSerialisableActivityTrackingRecord(ActivityTrackingRecord activityTrackingRecord)
        {
            SerialisableActivityTrackingRecord serialisableActivityTrackingRecord = new SerialisableActivityTrackingRecord();

            serialisableActivityTrackingRecord.ActivityType = activityTrackingRecord.ActivityType;
            serialisableActivityTrackingRecord.Annotations.AddRange(activityTrackingRecord.Annotations);
            serialisableActivityTrackingRecord.ContextGuid = activityTrackingRecord.ContextGuid;
            serialisableActivityTrackingRecord.EventDateTime = activityTrackingRecord.EventDateTime;
            serialisableActivityTrackingRecord.EventOrder = activityTrackingRecord.EventOrder;
            serialisableActivityTrackingRecord.ExecutionStatus = activityTrackingRecord.ExecutionStatus;
            serialisableActivityTrackingRecord.ParentContextGuid = activityTrackingRecord.ParentContextGuid;
            serialisableActivityTrackingRecord.QualifiedName = activityTrackingRecord.QualifiedName;
            
            if (activityTrackingRecord.Body != null)
            {
                for (int i = 0; i < activityTrackingRecord.Body.Count; i++)
                    serialisableActivityTrackingRecord.Body.Add(buildSerialisableTrackingDataItem(activityTrackingRecord.Body[i]));
            }

            if (activityTrackingRecord.EventArgs != null)
                serialisableActivityTrackingRecord.EventArgs = buildSerialisableData(activityTrackingRecord.EventArgs);

            return serialisableActivityTrackingRecord;
        }

        /// <summary>
        /// Serialise a <see cref="UserTrackingRecord" /> ready for persistence.
        /// </summary>
        /// <param name="userTrackingRecord">
        /// The original <see cref="UserTrackingRecord" /> to serialise.
        /// </param>
        /// <returns>
        /// Modified copy of the <see cref="UserTrackingRecord" /> containing
        /// with serialised data.
        /// </returns>
        private static SerialisableUserTrackingRecord buildSerialisableUserTrackingRecord(UserTrackingRecord userTrackingRecord)
        {
            SerialisableUserTrackingRecord serialisableUserTrackingRecord = new SerialisableUserTrackingRecord();

            serialisableUserTrackingRecord.ActivityType = userTrackingRecord.ActivityType;
            serialisableUserTrackingRecord.Annotations.AddRange(userTrackingRecord.Annotations);
            serialisableUserTrackingRecord.ContextGuid = userTrackingRecord.ContextGuid;
            serialisableUserTrackingRecord.EventDateTime = userTrackingRecord.EventDateTime;
            serialisableUserTrackingRecord.EventOrder = userTrackingRecord.EventOrder;
            serialisableUserTrackingRecord.ParentContextGuid = userTrackingRecord.ParentContextGuid;
            serialisableUserTrackingRecord.QualifiedName = userTrackingRecord.QualifiedName;
            serialisableUserTrackingRecord.UserDataKey = userTrackingRecord.UserDataKey;

            if (userTrackingRecord.Body != null)
            {
                for (int i = 0; i < userTrackingRecord.Body.Count; i++)
                    serialisableUserTrackingRecord.Body.Add(buildSerialisableTrackingDataItem(userTrackingRecord.Body[i]));
            }

            if (userTrackingRecord.EventArgs != null)
                serialisableUserTrackingRecord.EventArgs = buildSerialisableData(userTrackingRecord.EventArgs);

            if (userTrackingRecord.UserData != null)
                serialisableUserTrackingRecord.UserData = buildSerialisableData(userTrackingRecord.UserData);

            return serialisableUserTrackingRecord;
        }

        /// <summary>
        /// Serialise a <see cref="WorkflowTrackingRecord" /> ready for persistence. This
        /// method deals with WorkflowTrackingRecords that contain workflow change
        /// details.
        /// </summary>
        /// <param name="workflowTrackingRecord">
        /// The original <see cref="WorkflowTrackingRecord" /> to serialise.
        /// </param>
        /// <returns>
        /// Modified copy of the <see cref="WorkflowTrackingRecord" /> containing
        /// serialised data.
        /// </returns>
        private static SerialisableWorkflowChangeRecord buildSerialisableWorkflowChangeTrackingRecord(WorkflowTrackingRecord workflowTrackingRecord)
        {
            SerialisableWorkflowChangeRecord serialisableWorkflowChangeRecord = new SerialisableWorkflowChangeRecord();

            serialisableWorkflowChangeRecord.Annotations.AddRange(workflowTrackingRecord.Annotations);
            serialisableWorkflowChangeRecord.EventDateTime = workflowTrackingRecord.EventDateTime;
            serialisableWorkflowChangeRecord.EventOrder = workflowTrackingRecord.EventOrder;
            serialisableWorkflowChangeRecord.TrackingWorkflowEvent = workflowTrackingRecord.TrackingWorkflowEvent;

            if (workflowTrackingRecord.EventArgs != null && workflowTrackingRecord.EventArgs is TrackingWorkflowChangedEventArgs)
            {
                TrackingWorkflowChangedEventArgs eventArgs = (TrackingWorkflowChangedEventArgs)workflowTrackingRecord.EventArgs;
                
                serialisableWorkflowChangeRecord.EventArgs =
                    buildSerialisableTrackingWorkflowChangedEventArgs(eventArgs);
            }

            return serialisableWorkflowChangeRecord;
        }

        /// <summary>
        /// Serialise a <see cref="WorkflowTrackingRecord" /> ready for persistence.
        /// </summary>
        /// <param name="workflowTrackingRecord">
        /// The original <see cref="WorkflowTrackingRecord" /> to serialise.
        /// </param>
        /// <returns>
        /// A <see cref="SerialisableWorkflowTrackingRecord" /> containing a serialised
        /// copy of the <see cref="WorkflowTrackingRecord" />.
        /// </returns>
        private static SerialisableWorkflowTrackingRecord buildSerialisableWorkflowTrackingRecord(WorkflowTrackingRecord workflowTrackingRecord)
        {
            SerialisableWorkflowTrackingRecord serialisableWorkflowTrackingRecord = new SerialisableWorkflowTrackingRecord();

            serialisableWorkflowTrackingRecord.Annotations.AddRange(workflowTrackingRecord.Annotations);
            serialisableWorkflowTrackingRecord.EventDateTime = workflowTrackingRecord.EventDateTime;
            serialisableWorkflowTrackingRecord.EventOrder = workflowTrackingRecord.EventOrder;
            serialisableWorkflowTrackingRecord.TrackingWorkflowEvent = workflowTrackingRecord.TrackingWorkflowEvent;

            if (workflowTrackingRecord.EventArgs != null)
            {
                serialisableWorkflowTrackingRecord.EventArgs = buildSerialisableData(workflowTrackingRecord.EventArgs);
                if (serialisableWorkflowTrackingRecord.EventArgs.NonSerialisable)
                {
                    // if this is a termination or other exception event then
                    // we need to get the exception details to trace what happened...
                    Exception workflowException = null;
                    switch (workflowTrackingRecord.TrackingWorkflowEvent)
                    {
                        case TrackingWorkflowEvent.Terminated:
                            workflowException = ((TrackingWorkflowTerminatedEventArgs)
                                workflowTrackingRecord.EventArgs).Exception;

                            break;

                        case TrackingWorkflowEvent.Exception:
                            workflowException = ((TrackingWorkflowExceptionEventArgs)
                                workflowTrackingRecord.EventArgs).Exception;

                            break;
                    }

                    if (workflowException != null)
                    {
                        serialisableWorkflowTrackingRecord.EventArgs = 
                            buildSerialisableData(workflowException.ToString());
                    }
                }
            }

            return serialisableWorkflowTrackingRecord;
        }

        /// <summary>
        /// Serialise a <see cref="TrackingDataItem" /> ready for persistence.
        /// </summary>
        /// <param name="trackingDataItem">
        /// The original <see cref="TrackingDataItem" /> to serialise.
        /// </param>
        /// <returns>
        /// A <see cref="SerialisableTrackingDataItem" /> containing a serialised
        /// copy of the <see cref="TrackingDataItem" />.
        /// </returns>
        private static SerialisableTrackingDataItem buildSerialisableTrackingDataItem(TrackingDataItem trackingDataItem)
        {
            SerialisableTrackingDataItem serialisableTrackingDataItem = new SerialisableTrackingDataItem();

            serialisableTrackingDataItem.Annotations.AddRange(trackingDataItem.Annotations);
            serialisableTrackingDataItem.Data = buildSerialisableData(trackingDataItem.Data);
            serialisableTrackingDataItem.FieldName = trackingDataItem.FieldName;

            return serialisableTrackingDataItem;
        }

        /// <summary>
        /// Serialise a <see cref="TrackingWorkflowChangedEventArgs" /> ready for persistence.
        /// </summary>
        /// <param name="eventArgs">
        /// The original <see cref="TrackingWorkflowChangedEventArgs" /> to serialise.
        /// </param>
        /// <returns>
        /// A <see cref="SerialisableTrackingWorkflowChangedEventArgs" /> containing a 
        /// serialised copy of the <see cref="TrackingWorkflowChangedEventArgs" />.
        /// </returns>
        private static SerialisableTrackingWorkflowChangedEventArgs buildSerialisableTrackingWorkflowChangedEventArgs(TrackingWorkflowChangedEventArgs eventArgs)
        {
            SerialisableTrackingWorkflowChangedEventArgs serialisableEventArgs = new SerialisableTrackingWorkflowChangedEventArgs();

            byte [] serialisedData;
            bool nonSerialisable;

            serialiseData(eventArgs, out serialisedData, out nonSerialisable);

            serialisableEventArgs.NonSerialisable = nonSerialisable;
            serialisableEventArgs.SerialisedData = serialisedData;
            serialisableEventArgs.StringData = eventArgs.ToString();
            serialisableEventArgs.Type = eventArgs.GetType();
            serialisableEventArgs.UnserialisedData = eventArgs;

            for (int i = 0 ; i < eventArgs.Changes.Count ; i++)
            {
                AddedActivityAction addedActivityAction = eventArgs.Changes[i] as AddedActivityAction;
                if (addedActivityAction != null)
                {
                    List<SerialisableActivityAddedAction> addedActions =
                        buildSerialisableAddedActivityAction(addedActivityAction, i);
                    
                    foreach (SerialisableActivityAddedAction addedAction in addedActions)
                        serialisableEventArgs.Changes.Add(addedAction);

                    continue;
                }

                RemovedActivityAction removedActivityAction = eventArgs.Changes[i] as RemovedActivityAction;
                if (removedActivityAction != null)
                {
                    List<SerialisableActivityRemovedAction> removedActions =
                        buildSerialisableRemovedActivityAction(removedActivityAction, i);

                    foreach (SerialisableActivityRemovedAction removedAction in removedActions)
                        serialisableEventArgs.Changes.Add(removedAction);

                    continue;
                }
            }

            return serialisableEventArgs;
        }

        private static List<SerialisableActivityAddedAction> buildSerialisableAddedActivityAction(AddedActivityAction action, int order)
        {
            List<SerialisableActivityAddedAction> addedActivities = new List<SerialisableActivityAddedAction>();

            Queue<Activity> activityQueue = new Queue<Activity>();
            activityQueue.Enqueue(action.AddedActivity);
            while (activityQueue.Count > 0)
            {
                Activity currentActivity = activityQueue.Dequeue();
                if (currentActivity == action.AddedActivity)
                {
                    // this the top-level activity
                    addedActivities.Add(new SerialisableActivityAddedAction(
                        currentActivity.GetType(), currentActivity.QualifiedName,
                        currentActivity.Parent == null ? null : currentActivity.Parent.QualifiedName,
                        order, WorkflowInstanceHelper.GetXomlDocument(currentActivity)));
                }
                else
                {
                    addedActivities.Add(new SerialisableActivityAddedAction(
                        currentActivity.GetType(), currentActivity.QualifiedName,
                        currentActivity.Parent == null ? null : currentActivity.Parent.QualifiedName,
                        -1, WorkflowInstanceHelper.GetXomlDocument(currentActivity)));
                }

                CompositeActivity parentActivity = currentActivity as CompositeActivity;
                if (parentActivity != null)
                {
                    foreach (Activity childActivity in parentActivity.Activities)
                        activityQueue.Enqueue(childActivity);
                }
            }

            return addedActivities;
        }

        private static List<SerialisableActivityRemovedAction> buildSerialisableRemovedActivityAction(RemovedActivityAction action, int order)
        {
            List<SerialisableActivityRemovedAction> removedActivities = new List<SerialisableActivityRemovedAction>();

            Queue<Activity> activityQueue = new Queue<Activity>();
            activityQueue.Enqueue(action.OriginalRemovedActivity);
            while (activityQueue.Count > 0)
            {
                // is this the first record?
                Activity currentActivity = activityQueue.Dequeue();
                if (currentActivity == action.OriginalRemovedActivity)
                {
                    removedActivities.Add(new SerialisableActivityRemovedAction(
                        currentActivity.GetType(), currentActivity.QualifiedName,
                        currentActivity.Parent == null ? null : currentActivity.Parent.QualifiedName,
                        order, WorkflowInstanceHelper.GetXomlDocument(currentActivity)));
                }
                else
                {
                    removedActivities.Add(new SerialisableActivityRemovedAction(
                        currentActivity.GetType(), currentActivity.QualifiedName,
                        currentActivity.Parent == null ? null : currentActivity.Parent.QualifiedName,
                        -1, WorkflowInstanceHelper.GetXomlDocument(currentActivity)));
                }

                CompositeActivity parentActivity = currentActivity as CompositeActivity;
                if (parentActivity != null)
                {
                    foreach (Activity childActivity in parentActivity.Activities)
                        activityQueue.Enqueue(childActivity);
                }
            }

            return removedActivities;
        }

        /// <summary>
        /// Serialise an object ready for persistence.
        /// </summary>
        /// <param name="objectToSerialise">
        /// The original <see cref="TrackingDataItem" /> to serialise.
        /// </param>
        /// <returns>
        /// <see cref="SerialisableData" /> containing a serialised representation
        /// of T.
        /// </returns>
        private static SerialisableData buildSerialisableData(object objectToSerialise)
        {
            SerialisableData serialisableData = new SerialisableData();

            byte [] serialisedData;
            bool nonSerialisable;

            serialiseData(objectToSerialise, out serialisedData, out nonSerialisable);

            serialisableData.NonSerialisable = nonSerialisable;
            serialisableData.SerialisedData = serialisedData;
            serialisableData.StringData = objectToSerialise.ToString();
            serialisableData.Type = objectToSerialise.GetType();
            serialisableData.UnserialisedData = objectToSerialise;

            return serialisableData;
        }

        /// <summary>
        /// Serialise an object into the specified byte stream.
        /// If the object cannot be serialised then the byte stream
        /// will be null and the nonSerialisable parameter will be false.
        /// </summary>
        /// <param name="objectToSerialise">
        /// <c>object</c> to serialise.
        /// </param>
        /// <param name="serialisedData">
        /// Serialised representation of the object.
        /// </param>
        /// <param name="nonSerialisable">
        /// Indicates whether the object was successfully serialised or not.
        /// </param>
        private static void serialiseData(object objectToSerialise, out byte [] serialisedData, out bool nonSerialisable)
        {
            nonSerialisable = false;
            serialisedData = null;

            try
            {
                serialisedData = SerialisationUtilities.Serialise(objectToSerialise);
            }
            catch (SerializationException)
            {
                nonSerialisable = true;
            }
        }
    }
}
