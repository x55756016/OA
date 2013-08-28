using System;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents the removal of an activity from a workflow.
    /// </summary>
    public class SerialisableActivityRemovedAction : SerialisableActivityChangeAction
    {
        public SerialisableActivityRemovedAction(Type activityType, 
            string qualifiedName, string parentQualifiedName, int order, 
            string activityXoml) : base(activityType, qualifiedName, 
            parentQualifiedName, order, activityXoml) { }
    }
}
