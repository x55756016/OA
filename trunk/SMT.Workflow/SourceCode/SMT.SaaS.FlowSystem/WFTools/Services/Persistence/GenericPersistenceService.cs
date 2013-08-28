using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using WFTools.Utilities.Diagnostics;

namespace WFTools.Services.Persistence
{
    /// <summary>
    /// Abstract implementation of <see cref="WorkflowPersistenceService" /> that 
    /// provides the framework necessary for a very generic persistence service. 
    /// 
    /// A resource provider and accessor work hand-in-hand to actually
    /// persist workflow data to the persistence store.
    /// <remarks>
    /// This implementation also supports automatic expiration of workflows
    /// making use of DelayActivity-type constructs.
    /// </remarks>
    /// </summary>
    public abstract class GenericPersistenceService : WorkflowPersistenceService, IPendingWork, IDisposable
    {
        /// <summary>
        /// Construct an instance of the persistence service using the 
        /// specified configuration parameters.
        /// </summary>
        /// <param name="unloadOnIdle">
        /// Indicates whether to unload idle workflow instances.
        /// </param>
        /// <param name="instanceOwnershipDuration">
        /// The timeout before the ownership of a workflow instance is invalidated.
        /// </param>
        /// <param name="loadInterval">
        /// The interval to check for workflow instances with expired timers.
        /// </param>
        protected GenericPersistenceService(bool unloadOnIdle, TimeSpan instanceOwnershipDuration, TimeSpan loadInterval)
        {
            this.unloadOnIdle = unloadOnIdle;
            this.instanceOwnershipDuration = instanceOwnershipDuration;
            this.loadInterval = loadInterval;
            this.serviceId = Guid.NewGuid();
            this.timerLock = new object();
        }

        /// <summary>
        /// Indicates whether to unload idle workflow instances.
        /// </summary>
        private readonly bool unloadOnIdle;

        /// <summary>
        /// Length of time before ownership of a workflow instance
        /// is automatically invalidated.
        /// </summary>
        private readonly TimeSpan instanceOwnershipDuration;

        /// <summary>
        /// Interval between checking for expired DelayActivity workflows.
        /// </summary>
        private readonly TimeSpan loadInterval;

        /// <summary>
        /// Unique identifier of this service.
        /// </summary>
        private readonly Guid serviceId;

        /// <summary>
        /// Lock used when updating the timer.
        /// </summary>
        private readonly object timerLock;

        /// <summary>
        /// Timer used to load workflows with expired timer activities.
        /// </summary>
        private SmartTimer smartTimer;

        /// <summary>
        /// Helper property that indicates when the ownership of a particular
        /// workflow instance will expire.
        /// </summary>
        private DateTime OwnershipTimeout
        {
            get
            {
                if (this.instanceOwnershipDuration == TimeSpan.MaxValue)
                    return DateTime.MaxValue;

                return (DateTime.UtcNow + this.instanceOwnershipDuration);
            }
        }

        /// <summary>
        /// The active <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying persistence store.
        /// </summary>
        private IResourceProvider resourceProvider;

        /// <summary>
        /// Create an <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying persistence store.
        /// </summary>
        /// <remarks>
        /// The resource provider is created once upon Start of the 
        /// persistence service.
        /// </remarks>
        protected abstract IResourceProvider CreateResourceProvider();

        /// <summary>
        /// Create an <see cref="IPersistenceResourceAccessor" /> that is responsible
        /// for manipulating the underlying persistence store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        protected abstract IPersistenceResourceAccessor CreateAccessor(IResourceProvider resourceProvider);

        /// <summary>
        /// Create an <see cref="IPersistenceResourceAccessor" /> that is responsible
        /// for manipulating the underlying persistence store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        /// <param name="transaction">
        /// Transaction to perform operations within.
        /// </param>
        protected abstract IPersistenceResourceAccessor CreateAccessor(IResourceProvider resourceProvider, Transaction transaction);

        /// <summary>
        /// Perform startup duties associated with this persistence service.
        /// <remarks>
        /// This implementation calls a virtual method to create a single 
        /// resource provider for this persistence service.
        /// </remarks>
        /// </summary>
        protected override void Start()
        {
            TraceHelper.Trace();

            try
            {
                // retrieve the active resource provider
                this.resourceProvider = CreateResourceProvider();

                base.Start();
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException = 
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, Guid.Empty);

                throw persistenceException;
            }
        }

        /// <summary>
        /// Perform shutdown duties associated with this persistence service.
        /// <remarks>
        /// This implementation disposes the internal <see cref="SmartTimer" />
        /// responsible for loading expired workflows into memory.
        /// </remarks>
        /// </summary>
        protected override void Stop()
        {
            TraceHelper.Trace();

            try
            {
                base.Stop();

                lock (this.timerLock)
                {
                    if (this.smartTimer != null)
                    {
                        this.smartTimer.Dispose();
                        this.smartTimer = null;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException =
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, Guid.Empty);

                throw persistenceException;
            }
        }

        /// <summary>
        /// Kick-off the timer to check for workflows with expired timers, 
        /// and recover running workflow instances.
        /// </summary>
        protected override void OnStarted()
        {
            TraceHelper.Trace();

            try
            {
                base.OnStarted();

                if (this.loadInterval > TimeSpan.Zero)
                {
                    lock (this.timerLock)
                    {
                        this.smartTimer = new SmartTimer(loadWorkflowsWithExpiredTimers,
                            null, this.loadInterval, this.loadInterval);
                    }
                }

                recoverRunningWorkflows();
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException = 
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, Guid.Empty);

                throw persistenceException;
            }
        }

        /// <summary>
        /// Callback used by the <see cref="SmartTimer" /> to load workflows
        /// with expired timers.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is raised using base.RaiseServicesExceptionNotHandledEvent where the workflow runtime deals with it.")]
        private void loadWorkflowsWithExpiredTimers(object state)
        {
            lock (this.timerLock)
            {
                if (base.State == WorkflowRuntimeServiceState.Started)
                {
                    IList<Guid> expiredWorkflowIds = null;
                    try
                    {
                        expiredWorkflowIds = this.LoadExpiredWorkflowIds();
                    }
                    catch (Exception e)
                    {
                        string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                        TraceHelper.Trace(errorMessage);

                        base.RaiseServicesExceptionNotHandledEvent(
                            new PersistenceException(errorMessage, e),
                            Guid.Empty);
                    }

                    if (expiredWorkflowIds != null)
                    {
                        // load each expired workflow into memory
                        foreach (Guid expiredWorkflowId in expiredWorkflowIds)
                        {
                            try
                            {
                                base.Runtime.GetWorkflow(expiredWorkflowId).Load();
                            }
                            catch (WorkflowOwnershipException)
                            {
                                continue;
                            }
                            catch (ObjectDisposedException)
                            {
                                throw;
                            }
                            catch (InvalidOperationException e)
                            {
                                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                                TraceHelper.Trace(errorMessage);

                                if (!e.Data.Contains("WorkflowNotFound"))
                                {
                                    base.RaiseServicesExceptionNotHandledEvent(
                                        new PersistenceException(errorMessage, e),
                                        expiredWorkflowId);
                                }

                                continue;
                            }
                            catch (Exception e)
                            {
                                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                                TraceHelper.Trace(errorMessage);

                                base.RaiseServicesExceptionNotHandledEvent(
                                    new PersistenceException(errorMessage, e),
                                    expiredWorkflowId);

                                continue;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recovers any running workflows that no longer have a valid owner.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is raised using base.RaiseServicesExceptionNotHandledEvent where the workflow runtime deals with it.")]
        private void recoverRunningWorkflows()
        {
            IList<Guid> workflowIds = LoadNonBlockedWorkflowIds();
            foreach (Guid workflowId in workflowIds)
            {
                try
                {
                    base.Runtime.GetWorkflow(workflowId).Load();
                }
                catch (Exception e)
                {
                    string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                    TraceHelper.Trace(errorMessage);

                    base.RaiseServicesExceptionNotHandledEvent(
                        new PersistenceException(errorMessage, e),
                        workflowId);

                    continue;
                }
            }
        }

        /// <summary>
        /// Retrieve a list of all workflow identifiers that no longer have
        /// a valid owner.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is raised using base.RaiseServicesExceptionNotHandledEvent where the workflow runtime deals with it.")]
        protected virtual IList<Guid> LoadNonBlockedWorkflowIds()
        {
            TraceHelper.Trace();

            IList<Guid> instanceIds;
            try
            {
                using (IPersistenceResourceAccessor resourceAccessor = CreateAccessor(this.resourceProvider))
                {
                    instanceIds = resourceAccessor.RetrieveNonBlockedInstanceIds(this.serviceId, this.OwnershipTimeout);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                base.RaiseServicesExceptionNotHandledEvent(
                    new PersistenceException(errorMessage, e),
                    Guid.Empty);

                instanceIds = new List<Guid>();
            }

            return instanceIds;
        }

        /// <summary>
        /// Retrieve a list of all expired workflow identifiers.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is raised using base.RaiseServicesExceptionNotHandledEvent where the workflow runtime deals with it.")]
        protected virtual IList<Guid> LoadExpiredWorkflowIds()
        {
            TraceHelper.Trace();

            IList<Guid> instanceIds;
            try
            {
                using (IPersistenceResourceAccessor resourceAccessor = CreateAccessor(this.resourceProvider))
                {
                    instanceIds = resourceAccessor.RetrieveExpiredTimerIds(this.serviceId, this.OwnershipTimeout);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                base.RaiseServicesExceptionNotHandledEvent(
                    new PersistenceException(errorMessage, e),
                    Guid.Empty);

                instanceIds = new List<Guid>();
            }

            return instanceIds;
        }

        /// <summary>
        /// Persist a workflow instance to the persistence store.
        /// </summary>
        /// <param name="rootActivity">
        /// Root activity of the workflow instance.
        /// </param>
        /// <param name="unlock">
        /// Indicates whether to unlock the instance when complete.
        /// </param>
        protected override void SaveWorkflowInstanceState(Activity rootActivity, bool unlock)
        {
            TraceHelper.Trace();

            try
            {
                if (rootActivity == null)
                    throw new ArgumentNullException("rootActivity");

                PendingWorkItem workItem = new PendingWorkItem();
                workItem.Status = GetWorkflowStatus(rootActivity);
                workItem.IsBlocked = GetIsBlocked(rootActivity);
                workItem.Info = GetSuspendOrTerminateInfo(rootActivity);
                workItem.StateId = (Guid) rootActivity.GetValue(Activity.ActivityContextGuidProperty);
                workItem.Type = PendingWorkItem.ItemType.Instance;
                workItem.InstanceId = WorkflowEnvironment.WorkflowInstanceId;
                workItem.Unlock = unlock;
                if (workItem.Status != WorkflowStatus.Completed && workItem.Status != WorkflowStatus.Terminated)
                    workItem.SerialisedActivity = GetDefaultSerializedForm(rootActivity);
                else
                    workItem.SerialisedActivity = new byte[0];

                TimerEventSubscription timerEventSubscription =
                    ((TimerEventSubscriptionCollection) rootActivity.GetValue(
                    TimerEventSubscriptionCollection.TimerCollectionProperty)).Peek();

                if (timerEventSubscription == null)
                    workItem.NextTimer = null;
                else
                    workItem.NextTimer = timerEventSubscription.ExpiresAt;

                WorkflowEnvironment.WorkBatch.Add(this, workItem);
            }
            catch (Exception e)
            {
                WfLogHelper.WriteLog("持久化SaveWorkflowInstanceState发生异常:" + e.ToString());
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException =
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException,
                    WorkflowEnvironment.WorkflowInstanceId);

                throw persistenceException;
            }
        }

        /// <summary>
        /// Unlock a workflow instance.
        /// </summary>
        /// <param name="rootActivity">
        /// The root activity of the workflow instance.
        /// </param>
        protected override void UnlockWorkflowInstanceState(Activity rootActivity)
        {
            TraceHelper.Trace();

            try
            {
                PendingWorkItem workItem = new PendingWorkItem();
                workItem.Type = PendingWorkItem.ItemType.ActivationComplete;
                workItem.InstanceId = WorkflowEnvironment.WorkflowInstanceId;

                WorkflowEnvironment.WorkBatch.Add(this, workItem);
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException = 
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, 
                    WorkflowEnvironment.WorkflowInstanceId);

                throw persistenceException;
            }
        }

        ///<summary>
        /// Load the specified workflow instance into memory from the 
        /// persistence store.
        ///</summary>
        ///<param name="instanceId">
        /// The <see cref="T:System.Guid"></see> of the root activity of the workflow instance.
        /// </param>
        protected override Activity LoadWorkflowInstanceState(Guid instanceId)
        {
            TraceHelper.Trace();

            Activity rootActivity;
            try
            {
                using (IPersistenceResourceAccessor resourceAccessor = CreateAccessor(this.resourceProvider))
                {
                    byte[] instanceState = resourceAccessor.RetrieveInstanceState(
                        instanceId, serviceId, OwnershipTimeout);

                    rootActivity = RestoreFromDefaultSerializedForm(instanceState, null);
                }
            }
            catch (Exception e)
            {
                WfLogHelper.WriteLog("加载工作流状态实例发生异常:LoadWorkflowInstanceState("+instanceId+");异常信息:"+e.ToString());

                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException = 
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, instanceId);

                throw persistenceException;
            }

            return rootActivity;
        }

        /// <summary>
        /// Persist a completed scope to the persistence store.
        /// </summary>
        /// <param name="activity">
        /// The completed scope to pesist.
        /// </param>
        protected override void SaveCompletedContextActivity(Activity activity)
        {
            TraceHelper.Trace();

            try
            {
                PendingWorkItem workItem = new PendingWorkItem();
                workItem.Type = PendingWorkItem.ItemType.CompletedScope;
                workItem.SerialisedActivity = GetDefaultSerializedForm(activity);
                workItem.InstanceId = WorkflowEnvironment.WorkflowInstanceId;
                workItem.StateId = (Guid)activity.GetValue(
                    Activity.ActivityContextGuidProperty);

                WorkflowEnvironment.WorkBatch.Add(this, workItem);
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException = 
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, 
                    WorkflowEnvironment.WorkflowInstanceId);

                throw persistenceException;
            }
        }

        /// <summary>
        /// Load a completed scope from the persistence store.
        /// </summary>
        /// <param name="scopeId">
        /// <see cref="Guid" /> representing the unique identifier of the completed scope.
        /// </param>
        /// <param name="outerActivity">
        /// The activity in which to load the completed scope into.
        /// </param>
        protected override Activity LoadCompletedContextActivity(Guid scopeId, Activity outerActivity)
        {
            TraceHelper.Trace();

            Activity contextActivity;
            try
            {
                using (IPersistenceResourceAccessor resourceAccessor = CreateAccessor(this.resourceProvider))
                {
                    byte[] instanceState = resourceAccessor.RetrieveCompletedScope(scopeId);

                    contextActivity = RestoreFromDefaultSerializedForm(instanceState, outerActivity);
                }
            }
            catch (Exception e)
            {
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException =
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, 
                    WorkflowEnvironment.WorkflowInstanceId);

                throw persistenceException;
            }

            return contextActivity;
        }

        /// <summary>
        /// Indicates whether to unload the specified activity when it is idle
        /// persisting them to the persistence store.
        /// </summary>
        /// <param name="activity">
        /// Activity considered for unloading.
        /// </param>
        protected override bool UnloadOnIdle(Activity activity)
        {
            TraceHelper.Trace();

            return this.unloadOnIdle;
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

            return true;
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

            if (transaction == null)
                throw new ArgumentNullException("transaction");

            if (items == null)
                throw new ArgumentNullException("items");

            try
            {
                using (IPersistenceResourceAccessor resourceAccessor = CreateAccessor(this.resourceProvider, transaction))
                {
                    foreach (PendingWorkItem item in items)
                    {
                        switch (item.Type)
                        {
                            case PendingWorkItem.ItemType.Instance:
                                resourceAccessor.InsertInstanceState(item,
                                    serviceId, this.OwnershipTimeout);

                                continue;

                            case PendingWorkItem.ItemType.CompletedScope:
                                resourceAccessor.InsertCompletedScope(
                                    item.InstanceId, item.StateId,
                                    item.SerialisedActivity);

                                continue;

                            case PendingWorkItem.ItemType.ActivationComplete:
                                resourceAccessor.UnlockInstanceState(
                                    item.InstanceId, this.serviceId);

                                continue;
                        }
                    }
                }
                WfLogHelper.WriteLog("持久化提交事务Commit完成......");
            }
            catch (Exception e)
            {
                WfLogHelper.WriteLog("持久化提交事务Commit发生异常:" + e.ToString());
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException =
                    new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, 
                    Guid.Empty);

                throw persistenceException;
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

            try
            {
                if (succeeded)
                {
                    foreach (PendingWorkItem item in items)
                    {
                        if (item.Type == PendingWorkItem.ItemType.Instance && item.NextTimer != null)
                            this.smartTimer.Update(item.NextTimer.Value);
                    }
                    WfLogHelper.WriteLog("持久化提交事务Complete完成......");
                }
            }
            catch (Exception e)
            {
                WfLogHelper.WriteLog("持久化Complete务发生异常:" + e.ToString());
                string errorMessage = RM.Get_Error_PersistenceServiceException(e.ToString());

                TraceHelper.Trace(errorMessage);

                PersistenceException persistenceException
                    = new PersistenceException(errorMessage, e);

                base.RaiseServicesExceptionNotHandledEvent(
                    persistenceException, Guid.Empty);

                throw persistenceException;
            }
        }

        ///<summary>
        /// Clean-up the timers used by the persistence service.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// Clean-up the timers used by the persistence service.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.timerLock)
                {
                    if (this.smartTimer != null)
                    {
                        this.smartTimer.Dispose();
                        this.smartTimer = null;
                    }
                }
            }
        }
    }
}