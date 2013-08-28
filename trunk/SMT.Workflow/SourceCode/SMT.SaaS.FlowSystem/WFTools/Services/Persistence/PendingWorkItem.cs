using System;
using System.Diagnostics.CodeAnalysis;
using System.Workflow.Runtime;

namespace WFTools.Services.Persistence
{
    /// <summary>
    /// Represents a pending workflow task that is committed in a single transaction.
    /// </summary>
    public class PendingWorkItem
    {
        private bool isBlocked;
        /// <summary>
        /// Indicates whether the workflow instance is blocked.
        /// </summary>
        public bool IsBlocked
        {
            get { return isBlocked; }
            set { isBlocked = value; }
        }

        private string info;
        /// <summary>
        /// Additional information about the workflow instance.
        /// </summary>
        public string Info
        {
            get { return info; }
            set { info = value; }
        }

        private Guid instanceId;
        /// <summary>
        /// The workflow instance's unique identifier.
        /// </summary>
        public Guid InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        private DateTime? nextTimer;
        /// <summary>
        /// TODO - ???
        /// </summary>
        public DateTime? NextTimer
        {
            get { return nextTimer; }
            set { nextTimer = value; }
        }

        private byte [] serialisedActivity;
        /// <summary>
        /// Serialised representation of the workflow instance.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "byte [] contains serialised data, cannot be represented as a collection")]
        public byte [] SerialisedActivity
        {
            get { return serialisedActivity; }
            set { serialisedActivity = value; }
        }

        private Guid stateId;
        /// <summary>
        /// Identifier of the state that the workflow instance is currently in.
        /// </summary>
        public Guid StateId
        {
            get { return stateId; }
            set { stateId = value; }
        }

        private WorkflowStatus status;
        /// <summary>
        /// The actual status of the workflow instance.
        /// </summary>
        public WorkflowStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        private ItemType type;
        /// <summary>
        /// Indicates what to do with this pending work item.
        /// </summary>
        public ItemType Type
        {
            get { return type; }
            set { type = value; }
        }

        private bool unlock;
        /// <summary>
        /// Indicates whether to unlock a workflow instance.
        /// </summary>
        public bool Unlock
        {
            get { return unlock; }
            set { unlock = value; }
        }

        /// <summary>
        /// Available types of operation used by PendingWorkItem.
        /// </summary>
        public enum ItemType
        {
            Instance,
            CompletedScope,
            ActivationComplete
        }
    }
}