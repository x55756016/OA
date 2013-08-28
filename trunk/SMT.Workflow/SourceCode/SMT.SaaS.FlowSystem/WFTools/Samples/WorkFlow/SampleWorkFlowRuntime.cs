using System;
using System.Collections.Generic;
using System.Configuration;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Tracking;
using WFTools.Services.Batching.Ado;
using WFTools.Services.Persistence.Ado;
using WFTools.Services.Tracking.Ado;
using WFTools.Utilities.Workflow;

namespace WFTools.Samples.WorkFlow
{
    /// <summary>
    /// Wrapper around the workflow runtime with helper methods
    /// for retrieving the sample workflows.
    /// </summary>
	public class SampleWorkFlowRuntime : WorkflowRuntimeWrapperBase
	{
        public SampleWorkFlowRuntime(ConnectionStringSettings persistenceConnectionString, ConnectionStringSettings trackingConnectionString, bool useLocalTransactions)
        {
            if (persistenceConnectionString == null)
                throw new ArgumentNullException("persistenceConnectionString");

            this.persistenceConnectionString = persistenceConnectionString;
            this.trackingConnectionString = trackingConnectionString;
            this.useLocalTransactions = useLocalTransactions;

            InitialiseAndStartWorkflowRuntime();
        }

        private readonly ConnectionStringSettings persistenceConnectionString;
        /// <summary>
        /// Connection string settings for the persistence service.
        /// </summary>
        protected ConnectionStringSettings PersistenceConnectionString
        {
            get { return persistenceConnectionString; }
        }

        private readonly ConnectionStringSettings trackingConnectionString;
        /// <summary>
        /// Connection string settings for the tracking service.
        /// </summary>
        protected ConnectionStringSettings TrackingConnectionString
        {
            get { return trackingConnectionString; }
        }

        private readonly Boolean useLocalTransactions;
        /// <summary>
        /// Indicates whether to use local transactions when using an AdoWorkBatchService.
        /// </summary>
        protected Boolean UseLocalTransactions
        {
            get { return useLocalTransactions; }
        }

        /// <summary>
        /// Use ADO persistence service.
        /// </summary>
        protected override WorkflowPersistenceService PersistenceService
        {
            get
            {
                return new AdoPersistenceService(
                    PersistenceConnectionString, true, TimeSpan.FromSeconds(30), 
                    TimeSpan.FromSeconds(30));
            }
        }

        /// <summary>
        /// Use ADO tracking service.
        /// </summary>
        protected override TrackingService TrackingService
        {
            get
            {
                if (TrackingConnectionString != null)
                    return new AdoTrackingService(TrackingConnectionString);

                return null;
            }
        }

        /// <summary>
        /// Use the ADO work batch service.
        /// </summary>
        protected override WorkflowCommitWorkBatchService WorkBatchService
        {
            get
            {
                return new AdoWorkBatchService(UseLocalTransactions);
            }
        }

        /// <summary>
        /// Use manual workflow scheduling service.
        /// </summary>
        protected override WorkflowSchedulerService SchedulerService
        {
            get
            {
                return new ManualWorkflowSchedulerService(true);
            }
        }

        /// <summary>
        /// Create an sequential workflow.
        /// </summary>
        public WorkflowInstance CreateSequentialWorkflow()
        {
            return CreateWorkflow(typeof (SequentialWorkFlow));
        }

        /// <summary>
        /// Create a sequential workflow with the specified instance identifier.
        /// </summary>
        public WorkflowInstance CreateSequentialWorkflow(Guid instanceGuid)
        {
            return CreateWorkflow(typeof(SequentialWorkFlow), instanceGuid);
        }

        /// <summary>
        /// Create a sequential workflow with the specified instance identifier.
        /// </summary>
        public WorkflowInstance CreateSequentialWorkflow(Guid instanceGuid, Dictionary<string, object> parameters)
        {
            return CreateWorkflow(typeof (SequentialWorkFlow), instanceGuid, parameters);
        }
    }
}