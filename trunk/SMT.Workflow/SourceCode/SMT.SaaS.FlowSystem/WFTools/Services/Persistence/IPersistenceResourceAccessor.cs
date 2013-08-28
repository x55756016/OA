using System;
using System.Collections.Generic;

namespace WFTools.Services.Persistence
{
    /// <summary>
    /// Interface that exposes data-access functionality to the 
    /// <see cref="GenericPersistenceService"/>.
    /// </summary>
    public interface IPersistenceResourceAccessor : IDisposable
    {
        /// <summary>
        /// Insert a new completed scope into the database.
        /// </summary>
        /// <param name="instanceId">
        /// <see cref="Guid" /> identifying the workflow instance.
        /// </param>
        /// <param name="scopeId">
        /// <see cref="Guid" /> identifying the scope.</param>
        /// <param name="state">
        /// 
        /// </param>
        void InsertCompletedScope(Guid instanceId, Guid scopeId, byte [] state);

        /// <summary>
        /// Retrieve a completed scope from the database.
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        byte [] RetrieveCompletedScope(Guid scopeId);

        /// <summary>
        /// Insert instance state into the database.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        void InsertInstanceState(PendingWorkItem workItem, Guid ownerId, DateTime ownedUntil);

        /// <summary>
        /// Retrieve instance state from the database.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        /// <returns></returns>
        byte [] RetrieveInstanceState(Guid instanceId, Guid ownerId, DateTime ownedUntil);

        /// <summary>
        /// Unlocks an instance in the database.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="ownerId"></param>
        void UnlockInstanceState(Guid instanceId, Guid ownerId);

        /// <summary>
        /// Retrieve a list of all expired workflow identifiers.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        /// <returns></returns>
        IList<Guid> RetrieveExpiredTimerIds(Guid ownerId, DateTime ownedUntil);

        /// <summary>
        /// Retrieve a list of all workflow identifiers whose ownership has expired.
        /// </summary>
        /// <param name="ownerId">
        /// <see cref="Guid" /> representing new owner's identifier.
        /// </param>
        /// <param name="ownedUntil">
        /// <see cref="DateTime" /> indicating when the new ownership expires.
        /// </param>
        /// <returns>
        /// List of all Guids matching the criteria.
        /// </returns>
        IList<Guid> RetrieveNonBlockedInstanceIds(Guid ownerId, DateTime ownedUntil);
    }
}
