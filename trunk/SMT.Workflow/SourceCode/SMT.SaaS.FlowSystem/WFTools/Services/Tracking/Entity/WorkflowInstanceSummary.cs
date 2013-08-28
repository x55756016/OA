using System;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents a summary of workflow instance information that is used by 
    /// the <see cref="GenericTrackingService" /> to persist 
    /// tracking information somewhere useful.
    /// </summary>
    public class WorkflowInstanceSummary
    {
        public WorkflowInstanceSummary(Guid instanceId, bool isXomlWorkflow, 
            string xomlDocument, Guid contextGuid, Guid callerInstanceId, 
            Guid callerContextGuid, Guid callerParentContextGuid, string callPath)
        {
            this.instanceId = instanceId;
            this.isXomlWorkflow = isXomlWorkflow;
            this.xomlDocument = xomlDocument;
            this.contextGuid = contextGuid;
            this.callerInstanceId = callerInstanceId;
            this.callerContextGuid = callerContextGuid;
            this.callerParentContextGuid = callerParentContextGuid;
            this.callPath = callPath;
        }

        private object internalId;
        /// <summary>
        /// Unique identifier assigned to this object by
        /// the underlying tracking provider.
        /// </summary>
        public object InternalId
        {
            get { return internalId; }
            set { internalId = value; }
        }

        private Guid instanceId;
        /// <summary>
        /// Unique identifier of this workflow instance.
        /// </summary>
        public Guid InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        private Guid contextGuid;
        /// <summary>
        /// Identifier of the context in which this workflow instance
        /// was called.
        /// </summary>
        public Guid ContextGuid
        {
            get { return contextGuid; }
            set { contextGuid = value; }
        }

        private Guid callerInstanceId;
        /// <summary>
        /// The identifier of the workflow instance that called
        /// this workflow instance.
        /// </summary>
        public Guid CallerInstanceId
        {
            get { return callerInstanceId; }
            set { callerInstanceId = value; }
        }

        private string callPath;
        /// <summary>
        /// Concatenated list of activity qualified names which this workflow
        /// instance has called through.
        /// </summary>
	    public string CallPath
	    {
		    get { return callPath;}
		    set { callPath = value;}
	    }

        private Guid callerContextGuid;
        /// <summary>
        /// Context identifier of the workflow instance that called
        /// this workflow instance.
        /// </summary>
        public Guid CallerContextGuid
        {
            get { return callerContextGuid; }
            set { callerContextGuid = value; }
        }

        private Guid callerParentContextGuid;
        /// <summary>
        /// Context identifier of the workflow instance that called the
        /// caller of this workflow instance.
        /// </summary>
        public Guid CallerParentContextGuid
        {
            get { return callerParentContextGuid; }
            set { callerParentContextGuid = value; }
        }

        private WorkflowTypeSummary workflowType;
        /// <summary>
        /// Actual type summary of this workflow instance.
        /// </summary>
        public WorkflowTypeSummary WorkflowType
        {
            get { return workflowType; }
            set { workflowType = value; }
        }

        private ActivitySummary rootActivity;
        /// <summary>
        /// Summary of the root activity of this workflow instance.
        /// </summary>
        public ActivitySummary RootActivity
        {
            get { return rootActivity; }
            set { rootActivity = value; }
        }

        private bool isXomlWorkflow;
        /// <summary>
        /// Indicates whether the workflow instance has been modified
        /// at runtime, in which case, the <see cref="WorkflowType" />
        /// above will not accurately represent the 'real' type
        /// of the instance.
        /// </summary>
        public bool IsXomlWorkflow
        {
            get { return isXomlWorkflow; }
            set { isXomlWorkflow = value; }
        }

        private string xomlDocument;
        /// <summary>
        /// Full XOML document of this workflow instance.
        /// </summary>
        public string XomlDocument
        {
            get { return xomlDocument; }
            set { xomlDocument = value; }
        }
    }
}
