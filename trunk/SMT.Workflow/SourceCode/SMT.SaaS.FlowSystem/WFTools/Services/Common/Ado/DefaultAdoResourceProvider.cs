using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// Implementation of <see cref="IAdoResourceProvider" /> that uses the generic ADO
    /// methods to create database resources.
    /// 
    /// It also serves as a base class for other <see cref="IAdoResourceProvider" />
    /// implementations as it contains several helper methods for that
    /// very purpose.
    /// </summary>
    public class DefaultAdoResourceProvider : IAdoResourceProvider
    {
        /// <summary>
        /// Initialise the resource provider with connection String details.
        /// </summary>
        /// <param name="connectionStringSettings">
        /// The connection String settings to initialise with.
        /// </param>
        public virtual void Initialise(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null)
                throw new ArgumentNullException("connectionStringSettings");

            this.dbProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            if (this.dbProviderFactory == null)
            {
                throw new ArgumentOutOfRangeException("connectionStringSettings",
                    "Invalid connection String settings specified.");
            }
            
            // replace any automatic transaction enlistment parameters with
            // values that indicate manual transaction enlistment is required
            DbConnectionStringBuilder dbConnectionStringBuilder = dbProviderFactory.CreateConnectionStringBuilder();
            dbConnectionStringBuilder.ConnectionString = connectionStringSettings.ConnectionString;

            ReplaceEnlistInConnectionString(dbConnectionStringBuilder);

            this.connectionStringSettings = new ConnectionStringSettings(
                connectionStringSettings.Name, dbConnectionStringBuilder.ToString(), 
                connectionStringSettings.ProviderName);
        }

        private ConnectionStringSettings connectionStringSettings;
        /// <summary>
        /// The connection String settings used to initialise the resource provider.
        /// </summary>
        protected virtual ConnectionStringSettings ConnectionStringSettings
        {
            get { return connectionStringSettings; }
        }

        /// <summary>
        /// The connection String used to connect to the underlhing database.
        /// </summary>
        public String ConnectionString
        {
            get { return connectionStringSettings.ConnectionString; }
        }

        /// <summary>
        /// The name of the ADO.NET provider for which we provide resources for.
        /// </summary>
        public String ProviderName
        {
            get { return connectionStringSettings.ProviderName; }
        }

        private DbProviderFactory dbProviderFactory;
        /// <summary>
        /// The database provider factory used to provide ADO resources.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase",
            Justification = "Mirroring ADO DbProviderFactory class.")]
        protected virtual DbProviderFactory DbProviderFactory
        {
            get { return dbProviderFactory; }
        }

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
        public virtual DbConnection CreateEnlistedConnection(Transaction transaction, out bool shouldClose)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            DbConnection dbConnection = CreateConnection();
            dbConnection.Open();
            dbConnection.EnlistTransaction(transaction);

            shouldClose = true;

            return dbConnection;
        }

        /// <summary>
        /// Replaces any existing 'enlist' parameter in the connection String
        /// with a value indicating that manual enlist is necessary.
        /// </summary>
        protected virtual void ReplaceEnlistInConnectionString(DbConnectionStringBuilder dbConnectionStringBuilder)
        {
            if (dbConnectionStringBuilder.ContainsKey("enlist"))
                dbConnectionStringBuilder.Remove("enlist");

            dbConnectionStringBuilder.Add("enlist", false);
        }

        /// <summary>
        /// Creates an ADO connection initialised using the previously
        /// specified connection String.
        /// </summary>
        public virtual DbConnection CreateConnection()
        {
            DbConnection dbConnection = dbProviderFactory.CreateConnection();

            dbConnection.ConnectionString = connectionStringSettings.ConnectionString;

            return dbConnection;
        }

        /// <summary>
        /// Creates an ADO command initialised with the specified connection
        /// and command text. This uses the default command type.
        /// </summary>
        public virtual DbCommand CreateCommand(DbConnection dbConnection, String commandText)
        {
            return CreateCommand(dbConnection, commandText, CommandType.Text);
        }

        /// <summary>
        /// Creates an ADO command initialised with the specified connection, 
        /// command text and type.
        /// </summary>
        public virtual DbCommand CreateCommand(DbConnection dbConnection, String commandText, CommandType commandType)
        {
            if (dbConnection == null)
                throw new ArgumentNullException("dbConnection");

            if (String.IsNullOrEmpty(commandText))
                throw new ArgumentNullException("commandText");

            DbCommand dbCommand = dbProviderFactory.CreateCommand();

            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;
            dbCommand.Connection = dbConnection;

            return dbCommand;
        }

        /// <summary>
        /// Add an input parameter initialised with the value and type to the 
        /// specified <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public virtual DbParameter AddParameter(DbCommand dbCommand, String name, Object value, AdoDbType type)
        {
            return AddParameter(dbCommand, name, value, type, ParameterDirection.Input);
        }

        /// <summary>
        /// Indicates whether the AddParameter methods should deal with
        /// <see cref="AdoDbType.Cursor" />.
        /// </summary>
        /// <remarks>
        /// Default is <c>false</c>.
        /// </remarks>
        protected virtual bool SupportCursors
        {
            get { return false; }
        }

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
        public virtual DbParameter AddParameter(DbCommand dbCommand, String name, Object value, AdoDbType type, ParameterDirection direction)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (!SupportCursors && type == AdoDbType.Cursor)
                return null;

            DbParameter dbParameter = dbProviderFactory.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Direction = direction;

            PopulateType(dbParameter, type);
            PopulateValue(dbParameter, value);

            dbCommand.Parameters.Add(dbParameter);

            return dbParameter;
        }

        /// <summary>
        /// Add a parameter initialised with the type and direction to the
        /// specified <see cref="DbCommand" />.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public virtual DbParameter AddParameter(DbCommand dbCommand, String name, AdoDbType type, ParameterDirection direction)
        {
            return AddParameter(dbCommand, name, null, type, direction);
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its value.
        /// </summary>
        /// <remarks>
        /// Derived classes can use this as an opportunity to replace the value
        /// with something more appropriate to the underlying provider.
        /// </remarks>
        /// <param name="dbParameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual void PopulateValue(DbParameter dbParameter, Object value)
        {
            if (value == null)
                dbParameter.Value = DBNull.Value;
            else if (value is Enum)
                dbParameter.Value = (Int32) value;
            else
                dbParameter.Value = value;
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its type.
        /// </summary>
        /// <remarks>
        /// Derived classes can use this as an opportunity to replace the type
        /// with something more appropriate to the underlying provider.
        /// </remarks>
        /// <param name="dbParameter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", 
            Justification = "Uses a large switch statement.")]
        protected virtual void PopulateType(DbParameter dbParameter, AdoDbType type)
        {
            switch (type)
            {
                case AdoDbType.AnsiString:
                    dbParameter.DbType = DbType.AnsiString;
                    break;
                case AdoDbType.Binary:
                    dbParameter.DbType = DbType.Binary;
                    break;
                case AdoDbType.Boolean:
                    dbParameter.DbType = DbType.Boolean;
                    break;
                case AdoDbType.Byte:
                    dbParameter.DbType = DbType.Byte;
                    break;
                case AdoDbType.Currency:
                    dbParameter.DbType = DbType.Currency;
                    break;
                case AdoDbType.Date:
                    dbParameter.DbType = DbType.Date;
                    break;
                case AdoDbType.DateTime:
                    dbParameter.DbType = DbType.DateTime;
                    break;
                case AdoDbType.Decimal:
                    dbParameter.DbType = DbType.Decimal;
                    break;
                case AdoDbType.Double:
                    dbParameter.DbType = DbType.Double;
                    break;
                case AdoDbType.Guid:
                    dbParameter.DbType = DbType.Guid;
                    break;
                case AdoDbType.Int16:
                    dbParameter.DbType = DbType.Int16;
                    break;
                case AdoDbType.Int32:
                    dbParameter.DbType = DbType.Int32;
                    break;
                case AdoDbType.Int64:
                    dbParameter.DbType = DbType.Int64;
                    break;
                case AdoDbType.Object:
                    dbParameter.DbType = DbType.Object;
                    break;
                case AdoDbType.SByte:
                    dbParameter.DbType = DbType.SByte;
                    break;
                case AdoDbType.Single:
                    dbParameter.DbType = DbType.Single;
                    break;
                case AdoDbType.String:
                    dbParameter.DbType = DbType.String;
                    break;
                case AdoDbType.Text :
                    dbParameter.DbType = DbType.String;
                    break;
                case AdoDbType.Time:
                    dbParameter.DbType = DbType.Time;
                    break;
                case AdoDbType.UInt16:
                    dbParameter.DbType = DbType.UInt16;
                    break;
                case AdoDbType.UInt32:
                    dbParameter.DbType = DbType.UInt32;
                    break;
                case AdoDbType.UInt64:
                    dbParameter.DbType = DbType.UInt64;
                    break;
                case AdoDbType.VarNumeric:
                    dbParameter.DbType = DbType.VarNumeric;
                    break;
                case AdoDbType.AnsiStringFixedLength:
                    dbParameter.DbType = DbType.AnsiStringFixedLength;
                    break;
                case AdoDbType.StringFixedLength:
                    dbParameter.DbType = DbType.StringFixedLength;
                    break;
                case AdoDbType.Xml:
                    dbParameter.DbType = DbType.Xml;
                    break;
            }
        }
    }
}