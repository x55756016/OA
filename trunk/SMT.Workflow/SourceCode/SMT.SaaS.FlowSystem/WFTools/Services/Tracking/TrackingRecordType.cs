using System.Workflow.Runtime.Tracking;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Types of tracking records recorded in a <see cref="TrackingChannel" />.
    /// </summary>
    public enum TrackingRecordType
    {
        Activity, 
        User, 
        Workflow
    }
}
