namespace WFTools.Services.Persistence.Ado
{
    /// <summary>
    /// All parameter names used for executing commands against
    /// the persistence store.
    /// </summary>
    public enum PersistenceParameterName
    {
        InstanceId, 
        ScopeId, 
        State, 
        Status,
        Unlock, 
        IsBlocked, 
        Info, 
        CurrentOwnerId, 
        OwnerId,
        OwnedUntil, 
        NextTimer, 
        Now, 
        Result, 
        WorkflowIds
    }
}