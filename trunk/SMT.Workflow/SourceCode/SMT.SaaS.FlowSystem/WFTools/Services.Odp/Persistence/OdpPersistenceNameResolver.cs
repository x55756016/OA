using System;
using WFTools.Services.Persistence.Ado;

namespace WFTools.Services.Odp.Persistence
{
    /// <summary>
    /// ODP.NET specific implementation of <see cref="IPersistenceNameResolver" />.
    /// </summary>
    public class OdpPersistenceNameResolver : IPersistenceNameResolver
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
                case PersistenceCommandName.InsertCompletedScope:
                    return "WORKFLOW_PERSISTENCE_PKG.InsertCompletedScope";
                case PersistenceCommandName.InsertInstanceState:
                    return "WORKFLOW_PERSISTENCE_PKG.InsertInstanceState";
                case PersistenceCommandName.RetrieveCompletedScope:
                    return "WORKFLOW_PERSISTENCE_PKG.RetrieveCompletedScope";
                case PersistenceCommandName.RetrieveInstanceState:
                    return "WORKFLOW_PERSISTENCE_PKG.RetrieveInstanceState";
                case PersistenceCommandName.RetrieveExpiredTimerIds:
                    return "WORKFLOW_PERSISTENCE_PKG.RetrieveExpiredTimerIds";
                case PersistenceCommandName.RetrieveNonBlockingInstanceIds:
                    return "WORKFLOW_PERSISTENCE_PKG.RetrieveNonBlockingInstanceIds";
                case PersistenceCommandName.UnlockInstanceState:
                    return "WORKFLOW_PERSISTENCE_PKG.UnlockInstanceState";
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
        /// <returns>
        /// </returns>
        public string ResolveParameterName(PersistenceCommandName commandName, PersistenceParameterName parameterName)
        {
            switch (parameterName)
            {
                case PersistenceParameterName.InstanceId:
                    return "p_INSTANCE_ID";

                case PersistenceParameterName.ScopeId:
                    return "p_COMPLETED_SCOPE_ID";

                case PersistenceParameterName.State:
                    return "p_STATE";

                case PersistenceParameterName.Status:
                    return "p_STATUS";

                case PersistenceParameterName.Unlock:
                    return "p_UNLOCKED";

                case PersistenceParameterName.IsBlocked:
                    return "p_BLOCKED";

                case PersistenceParameterName.Info:
                    return "p_INFO";

                case PersistenceParameterName.CurrentOwnerId:
                    return "p_CURRENT_OWNER_ID";

                case PersistenceParameterName.OwnerId:
                    return "p_OWNER_ID";

                case PersistenceParameterName.OwnedUntil:
                    return "p_OWNED_UNTIL";

                case PersistenceParameterName.NextTimer:
                    return "p_NEXT_TIMER";

                case PersistenceParameterName.Result:
                    return "p_RESULT";

                case PersistenceParameterName.Now:
                    return "p_NOW";

                case PersistenceParameterName.WorkflowIds :
                    return "p_WORKFLOW_IDS";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNameParameterNameNotSupported());
        }
    }
}
