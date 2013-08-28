using System;
using System.Collections.Generic;
using WFTools.Services.Tracking.Entity;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Interface that exposes data-access functionality to the
    /// <see cref="GenericTrackingChannel"/>.
    /// </summary>
    public interface ITrackingChannelResourceAccessor : IDisposable
    {
        /// <summary>
        /// Create a new workflow instance record or return the existing
        /// record if any exists.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// along with all its type information.
        /// </param>
        WorkflowInstanceSummary InsertOrGetWorkflowInstance(WorkflowInstanceSummary workflowInstanceSummary);

        /// <summary>
        /// Insert a list of activity tracking records into the tracking store.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// along with all its type information.
        /// </param>
        /// <param name="activityTrackingRecords">
        /// A list of <see cref="SerialisableActivityTrackingRecord" /> objects
        /// that should be inserted into the tracking store.
        /// </param>
        void InsertTrackingRecords(WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableActivityTrackingRecord> activityTrackingRecords);

        /// <summary>
        /// Insert a list of user tracking records into the tracking store.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// along with all its type information.
        /// </param>
        /// <param name="userTrackingRecords">
        /// A list of <see cref="SerialisableUserTrackingRecord" /> objects
        /// that should be inserted into the tracking store.
        /// </param>
        void InsertTrackingRecords(WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableUserTrackingRecord> userTrackingRecords);

        /// <summary>
        /// Insert a list of workflow tracking records into the tracking store.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// along with all its type information.
        /// </param>
        /// <param name="workflowTrackingRecords">
        /// A list of <see cref="SerialisableWorkflowTrackingRecord" /> objects
        /// that should be inserted into the tracking store.
        /// </param>
        void InsertTrackingRecords(WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableWorkflowTrackingRecord> workflowTrackingRecords);

        /// <summary>
        /// Insert a list of workflow change records into the tracking store.
        /// </summary>
        /// <param name="workflowInstanceSummary">
        /// A <see cref="WorkflowInstanceSummary" /> representing a workflow instance
        /// along with all its type information.
        /// </param>
        /// <param name="workflowChangeRecords">
        /// A list of <see cref="SerialisableWorkflowChangeRecord" /> objects
        /// that should be inserted into the tracking store.
        /// </param>
        void InsertTrackingRecords(WorkflowInstanceSummary workflowInstanceSummary, IList<SerialisableWorkflowChangeRecord> workflowChangeRecords);
    }
}
