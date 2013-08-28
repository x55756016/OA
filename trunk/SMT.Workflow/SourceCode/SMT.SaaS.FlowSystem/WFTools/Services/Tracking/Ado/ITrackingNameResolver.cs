using System;

namespace WFTools.Services.Tracking.Ado
{
    /// <summary>
    /// Interface used for providing DbCommand text and DbParameter names
    /// to the <see cref="AdoTrackingChannel" /> resource accessor.
    /// </summary>
    public interface ITrackingNameResolver
    {
        /// <summary>
        /// Resolve a <see cref="TrackingCommandName" /> to its database-specific command text.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command needs to be resolved.
        /// </param>
        String ResolveCommandName(TrackingCommandName commandName);

        /// <summary>
        /// Resolve <see cref="TrackingParameterName" /> to its database-specific parameter name.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command the parameter
        /// name needs to be resolved for.
        /// </param>
        /// <param name="parameterName">
        /// A <see cref="TrackingParameterName" /> indicating which parameter needs to be resolved.
        /// </param>
        /// <returns>
        /// </returns>
        String ResolveParameterName(TrackingCommandName commandName, TrackingParameterName parameterName);

        /// <summary>
        /// Resolve a <see cref="TrackingParameterName" /> to its database-specific parameter name
        /// including any additional batching information.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command the parameter
        /// name needs to be resolved for.
        /// </param>
        /// <param name="parameterName">
        /// A <see cref="TrackingParameterName" /> indicating which parameter needs to be resolved.
        /// </param>
        /// <param name="parameterBatch">
        /// <see cref="Int32" /> representing which parameter batch we're dealing with.
        /// </param>
        /// <returns>
        /// </returns>
        String ResolveParameterName(TrackingCommandName commandName, TrackingParameterName parameterName, Int32 parameterBatch);
    }
}