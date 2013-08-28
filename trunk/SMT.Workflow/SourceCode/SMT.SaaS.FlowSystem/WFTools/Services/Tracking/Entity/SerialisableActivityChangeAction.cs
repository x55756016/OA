using System;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Abstract base class for all activity change actions.
    /// </summary>
    public abstract class SerialisableActivityChangeAction : SerialisableWorkflowChangeAction
    {
        protected SerialisableActivityChangeAction(Type activityType, 
            string qualifiedName, string parentQualifiedName, int order, 
            string activityXoml)
        {
            this.activityType = activityType;
            this.qualifiedName = qualifiedName;
            this.parentQualifiedName = parentQualifiedName;
            this.order = order;
            this.activityXoml = activityXoml;
        }

        private Type activityType;
        /// <summary>
        /// The type of activity which was changed.
        /// </summary>
        public Type ActivityType
        {
            get { return activityType; }
            set { activityType = value; }
        }

        private string qualifiedName;
        /// <summary>
        /// Fully qualified name of the activity in the workflow instance.
        /// </summary>
        public string QualifiedName
        {
            get { return qualifiedName; }
            set { qualifiedName = value; }
        }

        private string parentQualifiedName;
        /// <summary>
        /// Fully qualified name of the parent activity in the workflow instance.
        /// </summary>
        public string ParentQualifiedName
        {
            get { return parentQualifiedName; }
            set { parentQualifiedName = value; }
        }

        private int order;
        /// <summary>
        /// Order in which the activity was changed.
        /// </summary>
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        private string activityXoml;
        /// <summary>
        /// XOML for the activity that was changed.
        /// </summary>
        public string ActivityXoml
        {
            get { return activityXoml; }
            set { activityXoml = value; }
        }
    }
}