using System;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents a summary of workflow type information that is used by 
    /// the <see cref="GenericTrackingService" /> to persist 
    /// tracking information somewhere useful.
    /// </summary>
    public class WorkflowTypeSummary
    {
        public WorkflowTypeSummary(Type type)
        {
            this.type = type;
        }

        private object internalId;
        /// <summary>
        /// Unique identifier assigned to this object by an
        /// the underlying persistence provider.
        /// </summary>
        public object InternalId
        {
            get { return internalId; }
            set { internalId = value; }
        }

        private Type type;
        /// <summary>
        /// Type of the workflow.
        /// </summary>
        public Type Type
        {
            get { return type; }
            set { type = value; }
        }
    }
}
