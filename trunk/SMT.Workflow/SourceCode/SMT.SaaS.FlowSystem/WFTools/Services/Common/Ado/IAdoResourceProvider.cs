using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// <see cref="IAdoResourceProvider" /> is used by several workflow services to
    /// provide database resources such as connections, commands, transactions, etc.
    /// </summary>
    public interface IAdoResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Initialise the resource provider with connection string details.
        /// </summary>
        /// <param name="connectionStringSettings">
        /// The connection string settings to initialise with.
        /// </param>
        void Initialise(ConnectionStringSettings connectionStringSettings);

        /// <summary>
        /// The connection string used to connect to the underlhing database.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// The name of the ADO.NET provider for which we provide resources for.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates an ADO connection and enlists it in the specified transaction.
        /// </summary>
        /// <param name="transaction">
        /// The <see cref="Transaction" /> in which to enlist a new connection in.
        /// </param>
        /// <param name="shouldClose">
        /// Indicates whether the caller should close the connection or 
        /// whether it is managed by an external entity.
        /// </param>
        /// <returns>
        /// A <see cref="DbConnection" /> that is enlisted in the specified
        /// <see cref="Transaction" />.
        /// </returns>
        DbConnection CreateEnlistedConnection(Transaction transaction, out bool shouldClose);

        /// <summary>
        /// Creates an ADO connection initialised using the specified
        /// </summary>
        DbConnection CreateConnection();

        /// <summary>
        /// Creates an ADO command initialised with the specified connection
        /// and command text. This uses the default command type.
        /// </summary>
        DbCommand CreateCommand(DbConnection dbConnection, string commandText);

        /// <summary>
        /// Creates an ADO command initialised with the specified connection, 
        /// command text and type.
        /// </summary>
        DbCommand CreateCommand(DbConnection dbConnection, string commandText, CommandType commandType);

        /// <summary>
        /// Add an input parameter initialised with the value and type to the 
        /// specified <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        DbParameter AddParameter(DbCommand dbCommand, string name, object value, AdoDbType type);

        /// <summary>
        /// Add a parameter initialised with the value, type and direction to 
        /// the specified <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        DbParameter AddParameter(DbCommand dbCommand, string name, object value, AdoDbType type, ParameterDirection direction);

        /// <summary>
        /// Add a parameter initialised with the type and direction to the
        /// specified <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        DbParameter AddParameter(DbCommand dbCommand, string name, AdoDbType type, ParameterDirection direction);
    }
}