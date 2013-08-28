using System.Collections.Generic;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents the event arguments 
    /// </summary>
    public class SerialisableTrackingWorkflowChangedEventArgs : SerialisableData
    {
        private readonly IList<SerialisableWorkflowChangeAction> changes = new List<SerialisableWorkflowChangeAction>();
        /// <summary>
        /// List of all changes that have been made to the workflow.
        /// </summary>
        public IList<SerialisableWorkflowChangeAction> Changes
        {
            get { return changes; }
        }
    }
}
