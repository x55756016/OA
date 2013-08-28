using System;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Represents the addition of an activity to a workflow.
    /// </summary>
    public class SerialisableActivityAddedAction : SerialisableActivityChangeAction
    {
        public SerialisableActivityAddedAction(Type activityType, 
            string qualifiedName, string parentQualifiedName, int order, 
            string activityXoml) : base(activityType, qualifiedName, 
            parentQualifiedName, order, activityXoml) { }
    }
}
