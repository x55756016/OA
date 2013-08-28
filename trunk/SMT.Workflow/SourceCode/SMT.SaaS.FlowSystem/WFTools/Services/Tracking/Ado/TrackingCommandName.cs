namespace WFTools.Services.Tracking.Ado
{
     /// <summary>
    /// All command names used for executing commands against 
    /// the tracking store.
    /// </summary>
    public enum TrackingCommandName
    {
        DeleteInstanceTrackingProfile, 
        DeleteTrackingProfile, 
        GetCurrentDefaultTrackingProfile, 
        GetDefaultTrackingProfile, 
        GetInstanceTrackingProfile, 
        GetTrackingProfile, 
        GetTrackingProfileChanges,
        InsertActivities, 
        InsertActivityAddedActions, 
        InsertActivityRemovedActions, 
        InsertActivityTrackingRecords,
        InsertEventAnnotations, 
        InsertTrackingDataAnnotations, 
        InsertTrackingDataItems, 
        InsertUserTrackingRecords, 
        InsertWorkflow, 
        InsertWorkflowInstance,
        InsertWorkflowTrackingRecords,
        UpdateDefaultTrackingProfile,
        UpdateInstanceTrackingProfile,
        UpdateTrackingProfile, 

    }
}