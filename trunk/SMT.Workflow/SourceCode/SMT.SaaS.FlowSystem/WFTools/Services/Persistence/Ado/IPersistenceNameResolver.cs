namespace WFTools.Services.Persistence.Ado
{
    /// <summary>
    /// Interface used for providing DbCommand text and DbParameter names
    /// to the resource accessor.
    /// </summary>
    public interface IPersistenceNameResolver
    {
        /// <summary>
        /// Resolve <see cref="PersistenceCommandName" /> to their database-specific command text.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="PersistenceCommandName" /> indicating which command needs to be resolved.
        /// </param>
        string ResolveCommandName(PersistenceCommandName commandName);

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
        string ResolveParameterName(PersistenceCommandName commandName, PersistenceParameterName parameterName);
    }
}
