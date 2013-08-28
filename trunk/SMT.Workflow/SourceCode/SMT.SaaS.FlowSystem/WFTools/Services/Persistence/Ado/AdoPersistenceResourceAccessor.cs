using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Transactions;
using System.Workflow.Runtime;
using WFTools.Services.Common.Ado;
using WFTools.Services.Persistence;
using System.Data.OracleClient;

namespace WFTools.Services.Persistence.Ado
{
    /// <summary>
    /// Generic implementation of <see cref="IPersistenceResourceAccessor" /> that utilises an 
    /// <see cref="IAdoResourceProvider" /> for obtaining the resources
    /// it needs to communicate with the persistence store.
    /// </summary>
    public class AdoPersistenceResourceAccessor : IPersistenceResourceAccessor
    {
        /// <summary>
        /// Construct a new <see cref="AdoPersistenceResourceAccessor" /> with the
        /// specified <see cref="IAdoResourceProvider" />, 
        /// <see cref="IPersistenceNameResolver" /> and <see cref="IAdoValueReader" /> 
        /// </summary>
        /// <param name="resourceProvider">
        /// An <see cref="IAdoResourceProvider" /> used to provide resources for
        /// accessing the tracking store.
        /// </param>
        /// <param name="nameResolver">
        /// An <see cref="IPersistenceNameResolver" /> that resolves names
        /// of commands and parameters for the relevant tracking store.
        /// </param>
        /// <param name="valueReader">
        /// An <see cref="IAdoValueReader" /> that reads values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </param>
        public AdoPersistenceResourceAccessor(IAdoResourceProvider resourceProvider,
            IPersistenceNameResolver nameResolver, IAdoValueReader valueReader)
            : this(resourceProvider, nameResolver, valueReader, null) { }

        /// <summary>
        /// Construct a new <see cref="AdoPersistenceResourceAccessor" /> with the
        /// specified <see cref="IAdoResourceProvider" />, 
        /// <see cref="IPersistenceNameResolver" /> and <see cref="IAdoValueReader" /> 
        /// All work should be performed in the specified <see cref="Transaction" />.
        /// </summary>
        /// <param name="resourceProvider">
        /// An <see cref="IAdoResourceProvider" /> used to provide resources for
        /// accessing the tracking store.
        /// </param>
        /// <param name="nameResolver">
        /// An <see cref="IPersistenceNameResolver" /> that resolves names
        /// of commands and parameters for the relevant tracking store.
        /// </param>
        /// <param name="valueReader">
        /// An <see cref="IAdoValueReader" /> that reads values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </param>
        /// <param name="transaction">
        /// An <see cref="Transaction" /> in which to perform the work.
        /// </param>
        public AdoPersistenceResourceAccessor(IAdoResourceProvider resourceProvider,
            IPersistenceNameResolver nameResolver, IAdoValueReader valueReader,
            Transaction transaction)
        {
            if (resourceProvider == null)
                throw new ArgumentNullException("resourceProvider");

            if (nameResolver == null)
                throw new ArgumentNullException("nameResolver");

            if (valueReader == null)
                throw new ArgumentNullException("valueReader");

            this.resourceProvider = resourceProvider;
            this.nameResolver = nameResolver;
            this.valueReader = valueReader;

            if (transaction == null)
            {
                this.isConnectionOwner = true;
                this.dbConnection = resourceProvider.CreateConnection();
                this.dbConnection.Open();
            }
            else
                this.dbConnection = resourceProvider.CreateEnlistedConnection(transaction, out this.isConnectionOwner);
        }

        /// <summary>
        /// The resource provider used to create resources for connecting to
        /// and manipulating the persistence store.
        /// </summary>
        private readonly IAdoResourceProvider resourceProvider;

        /// <summary>
        /// The <see cref="IPersistenceNameResolver" /> responsible for resolving names of 
        /// stored procedures and parameters for a particular persistence store.
        /// </summary>
        private readonly IPersistenceNameResolver nameResolver;

        /// <summary>
        /// The <see cref="IAdoValueReader" /> responsible for reading values from
        /// <see cref="IDbCommand" /> and <see cref="IDataReader" /> implementations.
        /// </summary>
        private readonly IAdoValueReader valueReader;

        /// <summary>
        /// The database connection used to connect to the persistence store.
        /// </summary>
        private readonly DbConnection dbConnection;

        /// <summary>
        /// Indicates whether we own the database connection to the persistence store.
        /// </summary>
        private readonly bool isConnectionOwner;

        /// <summary>
        /// Insert a new completed scope into the persistence store.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="scopeId"></param>
        /// <param name="state"></param>
        public void InsertCompletedScope(Guid instanceId, Guid scopeId, byte[] state)
        {
            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.InsertCompletedScope), CommandType.StoredProcedure))
            {
                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertCompletedScope, 
                    PersistenceParameterName.InstanceId), instanceId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertCompletedScope,
                    PersistenceParameterName.ScopeId), scopeId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertCompletedScope,
                    PersistenceParameterName.State), state, AdoDbType.Binary);

                dbCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retrieve a completed scope from the persistence store.
        /// </summary>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        public byte[] RetrieveCompletedScope(Guid scopeId)
        {
            byte[] completedScope = null;

            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.RetrieveCompletedScope), CommandType.StoredProcedure))
            {
                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveCompletedScope,
                    PersistenceParameterName.ScopeId), scopeId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveCompletedScope,
                    PersistenceParameterName.Result), AdoDbType.Int32,
                    ParameterDirection.Output);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveCompletedScope,
                    PersistenceParameterName.State), AdoDbType.Cursor,
                    ParameterDirection.Output);

                int? result;
                using (IDataReader dataReader = dbCommand.ExecuteReader())
                {
                    if (dataReader.Read())
                        completedScope = (byte[])valueReader.GetValue(dataReader, 0);

                    result = valueReader.GetNullableInt32(dbCommand,
                        nameResolver.ResolveParameterName(
                            PersistenceCommandName.RetrieveInstanceState,
                            PersistenceParameterName.Result));
                }

                if (completedScope == null && result > 0)
                {
                    // scope could not be found
                    throw new PersistenceException(RM.Get_Error_ScopeCouldNotBeLoaded(scopeId));
                }
            }

            return completedScope;
        }

        /// <summary>
        /// Insert instance state into the persistence store.
        /// </summary>
        /// <param name="workItem"></param>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        public void InsertInstanceState(PendingWorkItem workItem, Guid ownerId, DateTime ownedUntil)
        {
            WfLogHelper.WriteLog("持久化InsertInstanceState InstanceId=" + workItem.InstanceId + ";ownerId=" + ownerId + ";时间=" + ownedUntil);

            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.InsertInstanceState), CommandType.StoredProcedure))
            {
                string instanceParameter = nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.InstanceId);

                string resultParameter = nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.Result);

                AddParameter(dbCommand, instanceParameter, 
                    workItem.InstanceId, AdoDbType.Guid);

                //DbParameter dp= AddParameter(dbCommand, nameResolver.ResolveParameterName(
                //    PersistenceCommandName.InsertInstanceState,
                //    PersistenceParameterName.State), workItem.SerialisedActivity,
                //    AdoDbType.Binary);

                #region beyond

                OracleCommand cmd2 =(OracleCommand) dbCommand.Connection.CreateCommand();

                //OracleTransaction tx;
                //tx = conn.BeginTransaction();

                //cmd2.Transaction = tx;
                cmd2.CommandText = "declare xx blob; begin dbms_lob.createtemporary(xx, false, 0); :tempblob := xx; end;";
                cmd2.Parameters.Add(new OracleParameter("tempblob", OracleType.Blob));
                cmd2.Parameters["tempblob"].Direction = ParameterDirection.Output;

                cmd2.ExecuteNonQuery();

                OracleLob tempLob;

                tempLob = (OracleLob)cmd2.Parameters["tempblob"].Value;
                tempLob.BeginBatch(OracleLobOpenMode.ReadWrite);
                tempLob.Write(workItem.SerialisedActivity, 0, workItem.SerialisedActivity.Length);
                tempLob.EndBatch();
                //dbCommand.Parameters.Add(new OracleParameter("p_STATE", OracleType.Blob));
                OracleParameter p_STATE = new OracleParameter("p_STATE", OracleType.Blob);
                p_STATE.Direction = ParameterDirection.Input;
                p_STATE.Value = tempLob;
                dbCommand.Parameters.Add(p_STATE);
                
                #endregion 

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.Status), workItem.Status, AdoDbType.Int32);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.Unlock), workItem.Unlock, AdoDbType.Boolean);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.IsBlocked), workItem.IsBlocked,
                    AdoDbType.Boolean);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.Info), workItem.Info, AdoDbType.Text);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.OwnerId), ownerId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.OwnedUntil), ownedUntil.ToUniversalTime(), 
                    AdoDbType.DateTime);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.NextTimer), workItem.NextTimer,
                    AdoDbType.DateTime);

                AddParameter(dbCommand, resultParameter, 
                    AdoDbType.Int32, ParameterDirection.Output);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState,
                    PersistenceParameterName.CurrentOwnerId), AdoDbType.Guid,
                    ParameterDirection.Output);

                dbCommand.ExecuteNonQuery();

                checkResult(dbCommand, resultParameter, instanceParameter);
            }
        }

        /// <summary>
        /// Retrieve instance state from the persistence store.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        /// <returns></returns>
        public byte[] RetrieveInstanceState(Guid instanceId, Guid ownerId, DateTime ownedUntil)
        {
            byte[] instanceState = null;

            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.RetrieveInstanceState), CommandType.StoredProcedure))
            {
                string instanceParameter = nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState, 
                    PersistenceParameterName.InstanceId);

                string resultParameter = nameResolver.ResolveParameterName(
                    PersistenceCommandName.InsertInstanceState, 
                    PersistenceParameterName.Result);

                AddParameter(dbCommand, instanceParameter, instanceId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveInstanceState, 
                    PersistenceParameterName.OwnerId), ownerId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveInstanceState,
                    PersistenceParameterName.OwnedUntil), ownedUntil, AdoDbType.DateTime);

                AddParameter(dbCommand, resultParameter, 
                    AdoDbType.Int32, ParameterDirection.Output);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveInstanceState,
                    PersistenceParameterName.CurrentOwnerId), AdoDbType.Guid,
                    ParameterDirection.Output);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveInstanceState,
                    PersistenceParameterName.State), AdoDbType.Cursor,
                    ParameterDirection.Output);

                int? result;
                using (IDataReader dataReader = dbCommand.ExecuteReader())
                {
                    if (dataReader.Read())
                        instanceState = (byte[])valueReader.GetValue(dataReader, 0);

                    result = valueReader.GetNullableInt32(dbCommand, resultParameter);
                }

                if (instanceState == null && result > 0)
                {
                    // workflow could not be found
                    PersistenceException e = new PersistenceException(
                        RM.Get_Error_InstanceCouldNotBeLoaded(instanceId));

                    e.Data["WorkflowNotFound"] = true;
                    WfLogHelper.WriteLog("检索实例状态 public byte[] RetrieveInstanceState(" + instanceId + ", " + ownerId + ", " + ownedUntil + ")发生异常:" + e.ToString());
                    throw e;
                }
                else
                    checkResult(dbCommand, resultParameter, instanceParameter);
            }

            return instanceState;
        }

        /// <summary>
        /// Unlocks an instance in the persistence store.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="ownerId"></param>
        public void UnlockInstanceState(Guid instanceId, Guid ownerId)
        {
            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.UnlockInstanceState), CommandType.StoredProcedure))
            {
                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.UnlockInstanceState,
                    PersistenceParameterName.InstanceId), instanceId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.UnlockInstanceState,
                    PersistenceParameterName.OwnerId), ownerId, AdoDbType.Guid);

                dbCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retrieve a list of all expired workflow identifiers.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownedUntil"></param>
        /// <returns></returns>
        public IList<Guid> RetrieveExpiredTimerIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> expiredWorkflowIds = new List<Guid>();
            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.RetrieveExpiredTimerIds), CommandType.StoredProcedure))
            {
                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveExpiredTimerIds,
                    PersistenceParameterName.OwnerId), ownerId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveExpiredTimerIds,
                    PersistenceParameterName.OwnedUntil), ownedUntil,
                    AdoDbType.DateTime);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveExpiredTimerIds,
                    PersistenceParameterName.Now), DateTime.UtcNow,
                    AdoDbType.DateTime);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveExpiredTimerIds,
                    PersistenceParameterName.WorkflowIds), AdoDbType.Cursor,
                    ParameterDirection.Output);

                using (IDataReader dataReader = dbCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                        expiredWorkflowIds.Add(valueReader.GetGuid(dataReader, 0));
                }
            }

            return expiredWorkflowIds;
        }

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
        public IList<Guid> RetrieveNonBlockedInstanceIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> workflowIds = new List<Guid>();
            using (DbCommand dbCommand = CreateCommand(nameResolver.ResolveCommandName(PersistenceCommandName.RetrieveNonBlockingInstanceIds), CommandType.StoredProcedure))
            {
                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveNonBlockingInstanceIds,
                    PersistenceParameterName.OwnerId), ownerId, AdoDbType.Guid);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveNonBlockingInstanceIds, 
                    PersistenceParameterName.OwnedUntil), ownedUntil, AdoDbType.DateTime);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveNonBlockingInstanceIds,
                    PersistenceParameterName.Now), DateTime.UtcNow, AdoDbType.DateTime);

                AddParameter(dbCommand, nameResolver.ResolveParameterName(
                    PersistenceCommandName.RetrieveNonBlockingInstanceIds,
                    PersistenceParameterName.WorkflowIds), AdoDbType.Cursor, ParameterDirection.Output);

                using (IDataReader dataReader = dbCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                        workflowIds.Add(valueReader.GetGuid(dataReader, 0));
                }
            }

            return workflowIds;
        }

        private const int ownershipError = -2;

        /// <summary>
        /// Check to see whether the result of a persistence operation
        /// was successful, raise appropriate exceptions if not.
        /// </summary>
        /// <param name="dbCommand">
        /// <see cref="DbCommand" /> to retrieve result information from.
        /// </param>
        /// <param name="resultParameter">
        /// Name of the parameter storing the result.
        /// </param>
        /// <param name="instanceParameter">
        /// Name of the parameter storing the instance identifier.
        /// </param>
        private void checkResult(DbCommand dbCommand, string resultParameter, string instanceParameter)
        {
            int? result = valueReader.GetNullableInt32(dbCommand, resultParameter);
            if (result == ownershipError)
            {
                Guid instanceId = valueReader.GetGuid(dbCommand, instanceParameter);
                WfLogHelper.WriteLog("检查结果:instanceId=" + instanceId.ToString());
                throw new WorkflowOwnershipException(instanceId);
            }
        }

        /// <summary>
        /// Thin wrapper around
        /// <see cref="IAdoResourceProvider.CreateCommand(DbConnection,string)" />
        /// that provides the connection to use for derived implementations.
        /// </summary>
        protected DbCommand CreateCommand(string commandText)
        {
            return resourceProvider.CreateCommand(this.dbConnection, commandText);
        }

        /// <summary>
        /// Thin wrapper around
        /// <see cref="IAdoResourceProvider.CreateCommand(DbConnection,string,CommandType)" />
        /// that provides the connection to use for derived implementations.
        /// </summary>
        protected DbCommand CreateCommand(string commandText, CommandType commandType)
        {
            return resourceProvider.CreateCommand(this.dbConnection, commandText, commandType);
        }

        /// <summary>
        /// Thin wrapper around
        /// <see cref="IAdoResourceProvider.AddParameter(DbCommand,string,object,AdoDbType)" />
        /// that provides the connection to use for derived implementations.
        /// </summary>
        protected DbParameter AddParameter(DbCommand dbCommand, string name, object value, AdoDbType type)
        {
            return resourceProvider.AddParameter(dbCommand, name, value, type);
        }

        /// <summary>
        /// Thin wrapper around
        /// <see cref="IAdoResourceProvider.AddParameter(DbCommand,string,object,AdoDbType,ParameterDirection)" />
        /// that provides the connection to use for derived implementations.
        /// </summary>
        protected DbParameter AddParameter(DbCommand dbCommand, string name, object value, AdoDbType type, ParameterDirection direction)
        {
            return resourceProvider.AddParameter(dbCommand, name, value, type, direction);
        }

        /// <summary>
        /// Thin wrapper around
        /// <see cref="IAdoResourceProvider.AddParameter(DbCommand,string,AdoDbType,ParameterDirection)" />
        /// that provides the connection to use for derived implementations.
        /// </summary>
        protected DbParameter AddParameter(DbCommand dbCommand, string name, AdoDbType type, ParameterDirection direction)
        {
            return resourceProvider.AddParameter(dbCommand, name, type, direction);
        }

        ///<summary>
        /// Close down any database connection and perform associated clean-up.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///<summary>
        /// Close down any database connection and perform associated clean-up.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.isConnectionOwner && this.dbConnection != null)
                    dbConnection.Dispose();
            }
        }
    }
}