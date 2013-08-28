using System;

namespace WFTools.Services.Persistence.Ado.Sql
{
    /// <summary>
    /// SQL Server specific implementation of <see cref="IPersistenceNameResolver" />.
    /// </summary>
    public class SqlPersistenceNameResolver : IPersistenceNameResolver
    {
        /// <summary>
        /// Resolve <see cref="PersistenceCommandName" /> to their database-specific command text.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="PersistenceCommandName" /> indicating which command needs to be resolved.
        /// </param>
        public string ResolveCommandName(PersistenceCommandName commandName)
        {
            switch (commandName)
            {
                case PersistenceCommandName.InsertCompletedScope :
                    return "InsertCompletedScope";
                case PersistenceCommandName.InsertInstanceState :
                    return "InsertInstanceState";
                case PersistenceCommandName.RetrieveCompletedScope :
                    return "RetrieveCompletedScope";
                case PersistenceCommandName.RetrieveInstanceState :
                    return "RetrieveInstanceState";
                case PersistenceCommandName.RetrieveExpiredTimerIds :
                    return "RetrieveExpiredTimerIds";
                case PersistenceCommandName.RetrieveNonBlockingInstanceIds :
                    return "RetrieveNonBlockingInstanceStateIds";
                case PersistenceCommandName.UnlockInstanceState :
                    return "UnlockInstanceState";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNamesNotSupported());
        }

        /// <summary>
        /// Resolve <see cref="PersistenceParameterName" /> to their database-specific parameter name.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="PersistenceCommandName" /> indicating which command the parameter
        /// name needs to be resolved for.
        /// </param>
        /// <param name="parameterName">
        /// A <see cref="PersistenceParameterName" /> indicating which parameter needs to be resolved.
        /// </param>
        public string ResolveParameterName(PersistenceCommandName commandName, PersistenceParameterName parameterName)
        {
            switch (parameterName)
            {
                case PersistenceParameterName.InstanceId :
                    if (commandName == PersistenceCommandName.InsertCompletedScope)
                        return "InstanceID";
                    else
                        return "uidInstanceID";

                case PersistenceParameterName.ScopeId :
                    return "completedScopeID";
                    
                case PersistenceParameterName.State :
                    return "state";

                case PersistenceParameterName.Status :
                    return "status";

                case PersistenceParameterName.Unlock :
                    return "unlocked";

                case PersistenceParameterName.IsBlocked :
                    return "blocked";

                case PersistenceParameterName.Info :
                    return "info";

                case PersistenceParameterName.CurrentOwnerId :
                    return "currentOwnerID";

                case PersistenceParameterName.OwnerId :
                    return "ownerID";

                case PersistenceParameterName.OwnedUntil :
                    return "ownedUntil";

                case PersistenceParameterName.NextTimer :
                    return "nextTimer";

                case PersistenceParameterName.Result :
                    return "result";

                case PersistenceParameterName.Now :
                    return "now";

                case PersistenceParameterName.WorkflowIds :
                    return "workflowIds";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNameParameterNameNotSupported());
        }
    }
}
