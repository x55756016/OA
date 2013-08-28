using System;
using System.Collections.Generic;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents a summary of an activity in a workflow instance that is used by 
    /// the <see cref="GenericTrackingService" /> to persist 
    /// tracking information somewhere useful.
    /// </summary>
    public class ActivitySummary
    {
        public ActivitySummary(Type type, string qualifiedName)
        {
            this.type = type;
            this.qualifiedName = qualifiedName;
        }

        private Type type;
        /// <summary>
        /// <see cref="Type" /> of the activity.
        /// </summary>
        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        private string qualifiedName;
        /// <summary>
        /// Fully qualified name of this activity in the workflow instance.
        /// </summary>
        public string QualifiedName
        {
            get { return qualifiedName; }
            set { qualifiedName = value; }
        }

        private ActivitySummary parentActivity;
        /// <summary>
        /// Parent <see cref="ActivitySummary" /> of this activity.
        /// </summary>
        public ActivitySummary ParentActivity
        {
            get { return parentActivity; }
            set { parentActivity = value; }
        }

        private IList<ActivitySummary> childActivities = new List<ActivitySummary>();
        /// <summary>
        /// List of children <see cref="ActivitySummary" /> objects 
        /// for this activity.
        /// </summary>
        public IList<ActivitySummary> ChildActivities
        {
            get { return childActivities; }
        }


    }
}
