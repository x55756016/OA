using System;
using System.Workflow.Runtime.Tracking;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents a changed tracking profile.
    /// </summary>
    public class TrackingProfileChange
    {
        private Type workflowType;
        /// <summary>
        /// The type of workflow with which the tracking profile is associated.
        /// </summary>
        public Type WorkflowType
        {
            get { return workflowType; }
            set { workflowType = value; }
        }

        private TrackingProfile trackingProfile;
        /// <summary>
        /// The <see cref="TrackingProfile" /> that was updated. This will be 
        /// <c>null</c> if the profile has been removed.
        /// </summary>
        public TrackingProfile TrackingProfile
        {
            get { return trackingProfile; }
            set { trackingProfile = value; }
        }
    }
}