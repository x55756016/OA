/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：MsOracle.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/12/13 14:33:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.DataAccess
	 * 模块名称：
	 * 描　　述： 	 微软公司的 System.Data.OracleClient 目的是封装高性能的、可扩展的
* ---------------------------------------------------------------------*/
using System;
using System.Data;
using System.Xml;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using System.Data.OracleClient;

namespace SMT.Workflow.Common.DataAccess
{



    /// <summary>
    /// 微软公司的 System.Data.OracleClient 目的是封装高性能的、可扩展的
    /// </summary>
    public sealed class MsOracle
    {

        /// <summary>
        /// 获取值，如果obj是null，近回DBNull.Value,否则返回obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValue(object obj)
        {
            if (obj == null||obj.ToString()=="")
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }        
        #region 获取数据库连接字符串
      //  private static Hashtable hast = new Hashtable();
      ///// <summary>
      //  /// 创建数据库连接字符串
      ///// </summary>
      //  /// <param name="connectionString">接字符串</param>
      ///// <param name="cacheKey">缓存关键字</param>
      ///// <returns></returns>
      //  public static string CreateConnectionString(string connectionString,string cacheKey)
      //  {
      //      if (!hast.Contains(cacheKey))
      //      {
      //          //string connectionstring = ConfigurationManager.AppSettings["ConnectionString"].ToString();
      //          hast.Add(cacheKey, connectionString);
      //          return _connectionString=connectionString;
      //      }
      //      else
      //      {
      //          return _connectionString=hast[cacheKey].ToString();
      //      }
      //  }
      //  /// <summary>
      //  /// 创建数据库连接字符串
      //  /// </summary>
      //  /// <param name="connectionString">接字符串</param>
      //  /// <returns></returns>
      //  public static string CreateConnectionString(string connectionString)
      //  {
      //      return _connectionString = connectionString;            
      //  }
        #endregion
        #region 默认打开数据库
        private static OracleConnection conn;
        private static OracleCommand comm;
        private static OracleDataAdapter adpter;

        public static string ConnectionString
        {
            get;
            set;
        }
        /// <summary>
        /// 创建一个OracleConnection连接对象,并打开
        /// </summary>
        /// <returns></returns>
        public static OracleConnection GetOracleConnection()
        {
            conn = new OracleConnection(ConnectionString);
            conn.Open();
            return conn;
        }
        /// <summary>
        /// 创建一个OracleConnection连接对象,并打开
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <returns></returns>
        public static OracleConnection GetOracleConnection(string connectionString)
        {
            conn = new OracleConnection(connectionString);
            conn.Open();
            return conn;
        }
        /// <summary>
        /// 打开数据库
        /// </summary>
        public static void Open()
        {
            conn = new OracleConnection(ConnectionString);
            comm = new OracleCommand();
            adpter = new OracleDataAdapter();
            comm.Connection = conn;
            adpter.SelectCommand = comm;
            comm.CommandType = CommandType.Text;
            try
            {
                conn.Open();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }
        /// <summary>
        /// 关闭数据库
        /// </summary>
        public static void Close()
        {
            if (conn != null)
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                }
                conn = null;
                comm = null;
                adpter = null;
            }
            if (Transaction != null)
            {
                Transaction = null;
            }
        }
        /// <summary>
        ///  关闭数据库
        /// </summary>
        /// <param name="connection">OracleConnection连接对象</param>
        public static void Close(OracleConnection connection)
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {                   
                    connection.Close();
                    connection.Dispose();
                }
                connection = null;
                if (Transaction != null)
                {
                    Transaction = null;
                }
            }
        }
        #region DataTable
        /// <summary>
        /// 执行SQL语句，返回DataTable
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sqlString)
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    dataset = ExecuteDataset(conn, CommandType.Text, sqlString, (OracleParameter[])null);
                }
                catch (OracleException e)
                {
                    conn.Close();
                    LogHelper.WriteLog("MsOracle.GetDataTable", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    conn.Close();
                }
                return dataset.Tables[0];
            }
        }
        public static DataTable GetDataTableByConnection(string connectionString ,string sqlString)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    dataset = ExecuteDataset(conn, CommandType.Text, sqlString, (OracleParameter[])null);
                }
                catch (OracleException e)
                {
                    conn.Close();
                    LogHelper.WriteLog("MsOracle.GetDataTable", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    conn.Close();
                }
                return dataset.Tables[0];
            }
        }
        public static DataSet GetDataSetByConnection(string connectionString, string sqlString)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    dataset = ExecuteDataset(conn, CommandType.Text, sqlString, (OracleParameter[])null);
                }
                catch (OracleException e)
                {
                    conn.Close();
                    LogHelper.WriteLog("MsOracle.GetDataSetByConnection", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    conn.Close();
                }
                return dataset;
            }
        }
        /// <summary>
        /// 执行SQL语句，返回DataTable
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sqlString, params OracleParameter[] param)
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    dataset = ExecuteDataset(conn, CommandType.Text, sqlString, param);
                }
                catch (OracleException e)
                {
                    conn.Close();
                    LogHelper.WriteLog("MsOracle.GetDataTable", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    conn.Close();
                }
                return dataset.Tables[0];
            }
        }
        #endregion
        #region OracleDataReader

        /// <summary>
        /// OracleDataReader
        /// </summary>   
        /// <param name="sqlString">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader GetOracleDataReader(string sqlString)
        {
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleCommand cmd = new OracleCommand(sqlString, connection);
            try
            {
                connection.Open();
                return ExecuteReader(connection, CommandType.Text, sqlString);
            }
            catch (OracleException e)
            {
                connection.Close();
                LogHelper.WriteLog("MsOracle.GetOracleDataReader", sqlString, e);
                throw new System.Exception(e.Message);
            }
        }
        /// <summary>
        /// OracleDataReader
        /// </summary>   
        /// <param name="sqlString">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader GetOracleDataReader(string sqlString, params OracleParameter[] param)
        {
            OracleConnection connection = new OracleConnection(ConnectionString);
            OracleCommand cmd = new OracleCommand(sqlString, connection);
            try
            {
                connection.Open();
                return ExecuteReader(connection, CommandType.Text, sqlString, param);
            }
            catch (OracleException e)
            {
                connection.Close();
                LogHelper.WriteLog("MsOracle.GetOracleDataReader", sqlString, e);
                throw new System.Exception(e.Message);
            }
        }
        #endregion
        #region 返回表中的记录总数
        /// <summary>
        /// 返回表中的记录总数,OracleParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public static int GetRecordCount(string tableName)
        {
            int recordCount = 0;
            string sql = "select count(0) as co from " + tableName + "";
            OracleDataReader dr = GetOracleDataReader(sql, null);
            if (dr.Read())
            {
                recordCount = Convert.ToInt32(dr["co"]);
            }
            dr.Close();
            return recordCount;
        }
        #endregion
        #region 执行一条计算查询结果语句，返回查询结果:第一行第一列的值
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果:第一行第一列的值,带参数OracleParameter[]
        /// </summary>  
        /// <param name="sqlString">计算查询结果语句</param>
        /// <param name="cmdParms">OracleParameter[] 参数</param>
        /// <returns>查询结果（string）</returns>        
        public static string GetOneValue(string sqlString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    return ExecuteScalar(connection, CommandType.Text, sqlString, cmdParms).ToString();
                }
                catch (OracleException e)
                {
                    connection.Close();
                    LogHelper.WriteLog("MsOracle.GetOneValue", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果:第一行第一列的值
        /// </summary>  
        /// <param name="sqlString">计算查询结果语句</param>
        /// <returns>查询结果（string）</returns>
        public static string GetOneValue(string sqlString)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    return ExecuteScalar(connection, CommandType.Text, sqlString).ToString();
                }
                catch (OracleException e)
                {
                    connection.Close();
                    LogHelper.WriteLog("MsOracle.GetOneValue", sqlString, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        #endregion
        #region 执行SQL语句
        public static int ExecuteSQL(string connectionString, string sqlString, params OracleParameter[] parameterValues)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, conn, (OracleTransaction)null, CommandType.Text, sqlString, parameterValues);
                return ExecuteNonQuery(conn, CommandType.Text, sqlString, parameterValues);
            }
        }
        /// <summary>
        /// 执行一个SQL语句命令，没有返回数据集，只返回影响记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteSQL(string sqlString)
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                return ExecuteNonQuery(conn, CommandType.Text, sqlString);
            }
        }
        /// <summary>
        /// 执行一个SQL语句命令，没有返回数据集，只返回影响记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="param">参数数组</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteSQL(string sqlString, params OracleParameter[] param)
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                return ExecuteNonQuery(conn, CommandType.Text, sqlString, param);
            }
        }
        /// <summary>
        /// 执行一个SQL语句命令，没有返回数据集，只返回影响记录数
        /// </summary>
        /// <param name="conn">OracleConnection</param>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="param">参数数组</param>
        /// <returns></returns>
        public static int ExecuteSQL(OracleConnection conn,string sqlString, params OracleParameter[] param)
        {
            if (conn.State==ConnectionState.Closed)
            {
                conn.Open();                
            }
            int n= ExecuteNonQuery(conn, CommandType.Text, sqlString, param);
            conn.Close();
            conn.Dispose();
            return n;
        }
             /// <summary>
        /// 通过事务执行SQL语句命令
        /// </summary>
        /// <param name="SQLStringList"></param>
        public static void ExecuteSQLTransaction(List<string> SQLStringList)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                connection.Open();
                OracleCommand command = new OracleCommand();
                command.Connection = connection;
                OracleTransaction transaction = connection.BeginTransaction();
                if (transaction != null)
                {
                    //command.Transaction = transaction;
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                try
                {
                    for (int i = 0; i < SQLStringList.Count; i++)
                    {
                        string str = SQLStringList[i].ToString();
                        if (str.Trim().Length > 1)
                        {
                            command.CommandText = str;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch (OracleException e)
                {
                    transaction.Rollback();
                    string stringlist = "";
                    foreach (var str in SQLStringList)
                    {
                        stringlist += str.ToString() + "\r\n"; 
                    }
                    LogHelper.WriteLog("MsOracle.ExecuteSQLTransaction", stringlist, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 通过事务执行SQL语句命令
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="SQLStringList"></param>
        public static void ExecuteSQLTransaction(string connectionString, List<string> SQLStringList)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                OracleCommand command = new OracleCommand();
                command.Connection = connection;
                OracleTransaction transaction = connection.BeginTransaction();
                if (transaction != null)
                {
                    //command.Transaction = transaction;
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                try
                {
                    for (int i = 0; i < SQLStringList.Count; i++)
                    {
                        string str = SQLStringList[i].ToString();
                        if (str.Trim().Length > 1)
                        {
                            command.CommandText = str;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch (OracleException e)
                {
                    transaction.Rollback();
                    string stringlist = "";
                    foreach (var str in SQLStringList)
                    {
                        stringlist += str.ToString() + "\r\n";
                    }
                    LogHelper.WriteLog("MsOracle.ExecuteSQLTransaction", stringlist, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 通过事务执行SQL语句命令
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="SQLStringList"></param>
        public static void ExecuteSQLTransaction(OracleConnection connection, List<string> SQLStringList)
        {
            connection.Open();
            OracleCommand command = new OracleCommand();
            command.Connection = connection;
            OracleTransaction transaction = connection.BeginTransaction();
            if (transaction != null)
            {
                //command.Transaction = transaction;
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                for (int i = 0; i < SQLStringList.Count; i++)
                {
                    string str = SQLStringList[i].ToString();
                    if (str.Trim().Length > 1)
                    {
                        command.CommandText = str;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (OracleException e)
            {
                transaction.Rollback();
                string stringlist = "";
                foreach (var str in SQLStringList)
                {
                    stringlist += str.ToString() + "\r\n";
                }
                LogHelper.WriteLog("MsOracle.ExecuteSQLTransaction", stringlist, e);
                throw new System.Exception(e.Message);
            }
            finally
            {
                connection.Close();
            }

        }
        #endregion
        #endregion

        #region 构造函数和私有方法
        private MsOracle() { }
        /// <summary>
        /// 这个方法被用来连接到一个数组的OracleParameters OracleCommand。
        /// 这个方法将会对DbNull赋值给任何参数的方向
        /// InputOutput和一个值为空。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandParameters"></param>
        private static void AttachParameters(OracleCommand command, OracleParameter[] commandParameters)
        {
            foreach (OracleParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }
                command.Parameters.Add(p);
            }
        }

        /// <summary>
        /// 该方法以一个数组的形式返回值指定数组的OracleParameters。
        /// </summary>
        /// <param name="commandParameters">OracleParameters数组</param>
        /// <param name="parameterValues">对象的数组的值被指定</param>
        private static void AssignParameterValues(OracleParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }
            if (commandParameters.Length != parameterValues.Length)
            {
                LogHelper.WriteLog("MsOracle.AssignParameterValues", "参数不匹配参数值的个数", null);
                throw new ArgumentException("参数不匹配参数值的个数");
            }
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        /// <summary>
        ///打开(如果必要的话),指定一个命令、连接、事务、参数类型和参数      
        /// </summary>
        /// <param name="command">OracleCommand</param>
        /// <param name="connection">OracleConnection</param>
        /// <param name="transaction">OracleTransaction 或  'null'</param>
        /// <param name="commandType"> CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param> 
        /// <param name="commandParameters">OracleParameter[]</param>
        private static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                //command.Transaction = transaction;
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            else
            {
                if (command.Parameters.Count > 0)
                    command.Parameters.Clear();
            }
            return;
        }
        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 执行一个命令，没有返回数据集，只返回影响记录数         
        /// 例如：  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </summary>
        /// <param name="connectionString"> OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL </param>  
        /// <returns>影响记录数  </returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        ///执行一个命令，没有返回数据集，只返回影响记录数       
        /// 例如：  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">OracleParameter[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            using (OracleConnection cn = new OracleConnection(connectionString))
            {
                cn.Open();
                return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 执行一个存储过程，没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或存储过程的返回值参数。      
        /// 例如：  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues"> params object[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或返回值参数。   
        /// 例如：  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </summary>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个命令没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或返回值参数。
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters"> OracleParameter[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            //if (Transaction != null)
            //{
            //    cmd.Transaction = Transaction;
            //}
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
            return cmd.ExecuteNonQuery();
        }
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string commandText, params OracleParameter[] commandParameters)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters);
            return cmd.ExecuteNonQuery();
        }
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string commandText)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null);
            return cmd.ExecuteNonQuery();
        }    
        /// <summary>
        /// 执行一个存储过程，没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或存储过程的返回值参数。
        /// </summary>
        /// <remarks>
        /// 这种方法并没有提供存取到输出参数或存储过程的返回值参数
        /// 
        /// 例如：  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        ///执行一个命令，没有返回数据集，只返回影响记录数  
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        ///执行一个命令，没有返回数据集，只返回影响记录数 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">OracleParameter[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
            try
            {
                int returnInt = cmd.ExecuteNonQuery();
                if (transaction != null)
                {
                    transaction.Commit();
                }
                return returnInt;
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return 0;
            }
        }

        /// <summary>
        /// 执行一个存储过程，没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或存储过程的返回值参数。
        /// </summary>
        /// <remarks>
        /// 这种方法并没有提供存取到输出参数或存储过程的返回值参数
        /// 
        /// 例如：  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues"> params object[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery
        #region 事务处理
       
       
      
      
        /// <summary>
        /// 事务对象
        /// </summary>
        private static OracleTransaction Transaction { get; set; }
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public static void BeginTransaction(OracleConnection connObjection)
        {          
            if (Transaction == null||Transaction.Connection != connObjection)
            {
                Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
        }
        /// <summary>
        /// 开始事务
        /// </summary>
        public static void BeginTransaction()
        {
            if (conn == null)
            {
                conn = GetOracleConnection(ConnectionString);
            }
            if (Transaction.Connection != conn)
            {
                Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);
            }
        }
        /// <summary>
        ///  提交事务
        /// </summary>
        public static void CommitTransaction()
        {
            try
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                    if (conn != null)
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                            conn.Dispose();
                        }
                        conn = null;
                        comm = null;
                        adpter = null;
                        Transaction = null;
                    }
                }
            }
            catch (OracleException e)
            {
                Transaction.Rollback();
                Close();
                Transaction = null;
                LogHelper.WriteLog("MsOracle.CommitTransaction",null, e);
                throw new System.Exception(e.Message);
            }
            finally
            {
                Close();
                Transaction = null;
            }
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public static void CommitTransaction(OracleConnection connObjection)
        {
            try
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                    if (connObjection != null)
                    {
                        if (connObjection.State == ConnectionState.Open)
                        {
                            Close(connObjection);
                            Transaction = null;
                        }
                    }
                }
            }
            catch (OracleException e)
            {
                Transaction.Rollback();
                Close(connObjection);
                Transaction = null;
                LogHelper.WriteLog("MsOracle.CommitTransaction(OracleConnection connObjection)", null, e);
                throw new System.Exception(e.Message);
            }
            finally
            {
                Close(connObjection);
                Transaction = null;
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public static void RollbackTransaction(OracleConnection connObjection)
        {
            try
            {
                if (Transaction != null && Transaction.Connection != null)
                {
                    Transaction.Rollback();
                    Close(connObjection);
                    Transaction = null;
                    if (connObjection != null)
                    {
                        if (connObjection.State == ConnectionState.Open)
                        {
                            Close(connObjection);
                        }
                    }
                }
                else
                {
                    Transaction = null;
                    if (connObjection != null)
                    {
                        if (connObjection.State == ConnectionState.Open)
                        {
                            Close(connObjection);
                        }
                    }
                }
            }
            catch (OracleException e)
            {
                Transaction.Rollback();
                Close(connObjection);
                Transaction = null;
                LogHelper.WriteLog("MsOracle.RollbackTransaction(OracleConnection connObjection)", null, e);
                throw new System.Exception(e.Message);
            }           
        }
        #region ExecuteSQLByTransaction
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleConnection connObjection)方法
        /// </summary>
        /// <param name="connObjection">OracleConnection</param>
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public static int ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction() 方法，最后执行CommitTransaction()方法
        /// </summary>    
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public static int ExecuteSQLByTransaction(string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Connection = conn;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    // Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleConnection connObjection)方法
        /// </summary>
        /// <param name="connObjection">OracleConnection</param>
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public static int ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    // Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction() 方法，最后执行CommitTransaction()方法
        /// </summary>    
        /// <param name="sqlString">sql 命令语句</param>
        /// <returns></returns>
        public static int ExecuteSQLByTransaction(string sqlString)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Connection = conn;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    // Transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(string sqlString)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion

        #region ExecuteScalarByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果object)        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public static object ExecuteScalarByTransaction(OracleConnection connObjection, string sqlString, OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
               
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteOracleScalar();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteScalarByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion

        #region ExecuteReaderByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReaderByTransaction(OracleConnection connection, string sqlString, OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;                   
                }              
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteReader();
                //return cmd.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteReaderByTransaction(OracleConnection connection, string sqlString, OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }            
        }
   
        #endregion

        #region GetDataTableByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connObjection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public static DataTable GetDataTableByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }              
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.GetDataTableByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion
        #region GetDataSetByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connObjection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public static DataSet GetDataSetByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet dt = new DataSet();
                da.Fill(dt);
                return dt;
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.GetDataSetByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion
        #endregion
        #region ExecuteDataSet

        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, (OracleParameter[])null);
        }       
        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">OracleParameter[]</param>
        /// <returns>a 返回DataSet</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            using (OracleConnection cn = new OracleConnection(connectionString))
            {
                cn.Open();
                return ExecuteDataset(cn, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        ///返回DataSet
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数。
        /// 
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns> 返回DataSet</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }
        /// <summary>
        /// 分页处理
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTableByPaging(string spName, params object[] parameterValues)
        {
            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                connection.Open();
                OracleParameter[] commandParameters = parameterValues as OracleParameter[];
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.StoredProcedure, spName, commandParameters);
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            //if ((parameterValues != null) && (parameterValues.Length > 0))
            //{
            //    OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(ConnectionString, spName, true);
            //    AssignParameterValues(commandParameters, parameterValues);
            //    return GetDataTableByPaging(ConnectionString, CommandType.StoredProcedure, spName, commandParameters);
            //}
            //else
            //{
            //    return null;
            //}
        }
        private static DataTable GetDataTableByPaging(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {          
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet dt = new DataSet();
                da.Fill(dt);
                return dt.Tables[0];               
            }
           
        }
        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataSet ds = new DataSet();

            da.Fill(ds);

            return ds;
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

               
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            OracleDataAdapter da;
            DataSet ds = new DataSet();
            try
            {
                da = new OracleDataAdapter(cmd);
                da.Fill(ds);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    // Rollback the transaction
                    transaction.Rollback();
                }
            }

            //return the dataset
            return ds;
        }

        /// <summary>
        ///执行一个存储过程命令返回一个结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataSet

        #region ExecuteReader

         private enum OracleConnectionOwnership
        {
            /// <summary>连接是通过OracleProvider拥有和管理</summary>
            Internal,
            /// <summary>连接是拥有和管理的访客</summary>
            External
        }


        /// <summary>
         /// 执行一个OracleCommand(返回一个结果OracleDataReader)
        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="transaction">有效的 OracleTransaction, or 'null'</param>
        /// <param name="commandType">命令类型 (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">以一个数组的形式返回OracleParameters </param>
        /// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by OracleProvider</param>
        /// <returns></returns>
        private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, OracleConnectionOwnership connectionOwnership)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);
            OracleDataReader dr = null;
            try
            {
                if (connectionOwnership == OracleConnectionOwnership.External)
                {
                    dr = cmd.ExecuteReader();
                }
                else
                {
                  //  dr = cmd.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                    dr = cmd.ExecuteReader();
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
                return (OracleDataReader)dr;
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw;
            }
        }

        /// <summary>
        ///执行一个OracleCommand(返回一个结果OracleDataReader)
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteReader(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>      
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </summary>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleConnection cn = new OracleConnection(connectionString);
            cn.Open();
            try
            {
                return ExecuteReader(cn, null, commandType, commandText, commandParameters, OracleConnectionOwnership.Internal);
            }
            catch
            {
                cn.Close();
                throw;
            }
        }

        /// <summary>
        ///执行存储过程通过一个OracleCommand(返回一个结果)对数据库中指定
        ///连接字符串使用提供的参数值。该方法将查询数据库发现参数
        ///存储过程(第一次是每个存储过程称为),指定值基于参数顺序。
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            return ExecuteReader(connection, (OracleTransaction)null, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///   OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
        }

        /// <summary>
        /// 执行存储过程通过一个OracleCommand(返回一个结果)对数据库中指定
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// 返回结果集的第一行第一列
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteScalar(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 返回结果集的第一行第一列
     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create & open an OracleConnection, and dispose of it after we are done.
            using (OracleConnection cn = new OracleConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteScalar(cn, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 返回结果集的第一行第一列
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 返回结果集的第一行第一列
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteScalar(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 返回结果集的第一行第一列
     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集的第一行第一列
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 返回结果集的第一行第一列 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        ///返回结果集的第一行第一列
     
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            try
            {
                object obj = cmd.ExecuteScalar();
                if (transaction != null)
                {
                    transaction.Commit();
                }
                return obj;
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return null;
            }

        }

        /// <summary>
        /// 执行一个存储过程并返回结果集的第一行第一列
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar
    }

    /// <summary>
    ///缓存参数类    
    /// </summary>
    public sealed class MsOracleProviderParameterCache
    {
        #region 私人方法、变量,并构造函数
        //因为这个类只提供了静态方法,使默认构造函数
        //实例被创建 new MsOracleProviderParameterCache()
        private MsOracleProviderParameterCache() { }
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 解决在运行时适当的组OracleParameters存储程序
        /// </summary>
        /// <param name="connectionString">有效的</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">是否包括用于返回值参数</param>
        /// <returns></returns>
        private static OracleParameter[] DiscoverSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            using (OracleConnection cn = new OracleConnection(connectionString))
            using (OracleCommand cmd = new OracleCommand(spName, cn))
            {
                cn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                OracleCommandBuilder.DeriveParameters(cmd);

                if (!includeReturnValueParameter)
                {
                    if (ParameterDirection.ReturnValue == cmd.Parameters[0].Direction)
                        cmd.Parameters.RemoveAt(0);
                }

                OracleParameter[] discoveredParameters = new OracleParameter[cmd.Parameters.Count];

                cmd.Parameters.CopyTo(discoveredParameters, 0);

                return discoveredParameters;
            }
        }

        //深度拷贝的OracleParameter缓存数组
        private static OracleParameter[] CloneParameters(OracleParameter[] originalParameters)
        {
            OracleParameter[] clonedParameters = new OracleParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (OracleParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion 私人方法、变量,并构造函数

        #region 缓存功能

        /// <summary>
        /// 参数数组添加到缓存中
        /// </summary>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <param name="commandParameters">以一个数组的形式返回OracleParameters</param>
        public static void CacheParameterSet(string connectionString, string commandText, params OracleParameter[] commandParameters)
        {
            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        ///以一个数组的形式返回OracleParameters
        /// </summary>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <returns>以一个数组的形式返回OracleParameters</returns>
        public static OracleParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            string hashKey = connectionString + ":" + commandText;

            OracleParameter[] cachedParameters = (OracleParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion c

        #region 查找参数

        /// <summary>
        /// 检索存储的程序的OracleParameters 
       /// 该方法将查询数据库信息,然后把它储存在缓存中为后面的要求作准备。  
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns>以一个数组的形式返回OracleParameters</returns>
        public static OracleParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// 检索存储的程序的OracleParameters
        /// 该方法将查询数据库信息,然后把它储存在缓存中为后面的要求作准备。
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="connectionString">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">一个bool值，说明是否包括返回值参数</param>
           /// <returns>以一个数组的形式返回OracleParameters</returns>
        public static OracleParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            string hashKey = connectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            OracleParameter[] cachedParameters;

            cachedParameters = (OracleParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                cachedParameters = (OracleParameter[])(paramCache[hashKey] = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter));
            }
            return CloneParameters(cachedParameters);
        }

        #endregion

    }

    #region System.Data.OracleClient 简化版
    /// <summary>
    /// 微软公司的 System.Data.OracleClient 简化版
    /// </summary>
    public sealed class MicrosoftOracle
    {
        /// <summary>
        /// 获取数据库连接字符串 如:
        ///   connectionStrings
        ///      add name="WorkFlowConnString" providerName="System.Data.OracleClient" connectionString="Data Source=smtsaasdb209;user id=smtwf;password=smtwf;"
        ///  connectionStrings>
        /// </summary>
        /// <param name="connectionKey">关键字</param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionKey)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
        }
        #region  获取值，如果obj是null，近回DBNull.Value,否则返回obj
        /// <summary>
        /// 获取值，如果obj是null，近回DBNull.Value,否则返回obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValue(object obj)
        {
            if (obj == null || obj.ToString() == "")
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }
        #endregion
        #region 创建一个OracleConnection连接对象,并打开
        /// <summary>
        ///  创建一个OracleConnection连接对象,并打开
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns></returns>
        public static OracleConnection CreateOracleConnection(string connectionString)
        {
            OracleConnection conn = null;
            try
            {
                conn = new OracleConnection(connectionString);
                conn.Open();
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog("CreateOracleConnection出错:" + ex.ToString());
                return null;
            }
            return conn;
        }
        #endregion
        #region 打开数据库
        /// <summary>
        ///  打开数据库
        /// </summary>
        /// <param name="connection">OracleConnection连接对象</param>
        public static void Open(OracleConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                }
                catch (System.Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    LogHelper.WriteLog("打开数据库出错:" + ex.ToString());
                }
            }
        }
        #endregion
        #region 关闭数据库
        /// <summary>
        ///  关闭数据库
        /// </summary>
        /// <param name="connection">OracleConnection连接对象</param>
        public static void Close(OracleConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    connection.Close();
                    connection.Dispose();
                    LogHelper.WriteLog("关闭数据库出错:" + ex.ToString());
                }
            }
        }
        #endregion
        #region 事务处理
        /// <summary>
        /// 事务对象
        /// </summary>
        private  OracleTransaction Transaction { get; set; }
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public  void BeginTransaction(OracleConnection connObjection)
        {
            if (Transaction == null || Transaction.Connection != connObjection)
            {
                if (connObjection != null && connObjection.State == ConnectionState.Open)
                {
                    Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                else
                {
                    LogHelper.WriteLog("数据库连接已关闭!"+connObjection.ConnectionString);
                    connObjection.Open();
                    LogHelper.WriteLog("数据库连接重新打开!" + connObjection.ConnectionString);
                    Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
            }
        }
        #endregion
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public  void CommitTransaction(OracleConnection connObjection)
        {
            if (Transaction != null && Transaction.Connection == connObjection)
            {
                try
                {
                    Transaction.Commit();
                    if (connObjection != null)
                    {
                        Transaction.Dispose();
                        if (connObjection.State == ConnectionState.Open)
                        {
                            Close(connObjection);
                            Transaction = null;
                        }
                    }
                }
                catch (OracleException e)
                {
                    Transaction.Rollback();
                    Close(connObjection);
                    Transaction = null;
                    LogHelper.WriteLog("MsOracle.CommitTransaction(OracleConnection connObjection)", null, e);
                    throw new System.Exception(e.Message);
                }
                finally
                {
                    Close(connObjection);
                    Transaction = null;
                }
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public  void RollbackTransaction(OracleConnection connObjection)
        {
            if (Transaction != null && Transaction.Connection == connObjection)
            {
                try
                {
                    Transaction.Rollback();
                    Close(connObjection);
                    Transaction = null;
                    if (connObjection != null)
                    {
                        if (connObjection.State == ConnectionState.Open)
                        {
                            Close(connObjection);
                        }
                    }
                }
                catch (OracleException e)
                {
                    Transaction.Rollback();
                    Close(connObjection);
                    Transaction = null;
                    LogHelper.WriteLog("MsOracle.RollbackTransaction(OracleConnection connObjection)", null, e);
                    throw new System.Exception(e.Message);
                }
            }
        }
        #region ExecuteSQLByTransaction
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleConnection connObjection)方法
        /// </summary>
        /// <param name="connObjection">OracleConnection</param>
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public  int ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }              /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleConnection connObjection)方法
        /// </summary>
        /// <param name="connObjection">OracleConnection</param>
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public  int ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    // Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                cmd.CommandType = CommandType.Text;
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteSQLByTransaction(OracleConnection connObjection, string sqlString)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }

        #endregion

        #region ExecuteScalarByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果object)        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public  object ExecuteScalarByTransaction(OracleConnection connObjection, string sqlString, OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteOracleScalar();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteScalarByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion

        #region ExecuteReaderByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public  OracleDataReader ExecuteReaderByTransaction(OracleConnection connection, string sqlString, OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                }
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                return cmd.ExecuteReader();
                //return cmd.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.ExecuteReaderByTransaction(OracleConnection connection, string sqlString, OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }

        #endregion

        #region GetDataTableByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connObjection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public  DataTable GetDataTableByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.GetDataTableByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion
        #region GetDataSetByTransaction
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)        /// </summary>   
        /// <param name="connObjection">有效的 OracleConnection</param>   
        /// <param name="sqlString">SQL</param> 
        /// <param name="parameterValues">以一个数组的形式返回OracleParameters ,如果没有参数即为null</param>
        /// <returns></returns>
        public  DataSet GetDataSetByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                if (connObjection.State != ConnectionState.Open)
                {
                    connObjection.Open();
                }
                cmd.Connection = connObjection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlString;
                if (Transaction != null)
                {
                    cmd.Transaction = Transaction;
                    //Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (parameterValues != null)
                {
                    AttachParameters(cmd, parameterValues);
                }
                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet dt = new DataSet();
                da.Fill(dt);
                return dt;
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                    LogHelper.WriteLog("MsOracle.GetDataSetByTransaction(OracleConnection connObjection, string sqlString, params OracleParameter[] parameterValues)", "回滚事务", e);
                }
                throw new System.Exception(e.Message);
            }
        }
        #endregion
        #endregion

        #region 执行SQL语句
        #region ExecuteNonQuery
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string commandText, params OracleParameter[] commandParameters)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters);
            return cmd.ExecuteNonQuery();
        }
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string commandText)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null);
            return cmd.ExecuteNonQuery();
        }    
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                connection.Close();
                connection.Dispose();
                LogHelper.WriteLog("ExecuteNonQuery出错:" + ex.ToString());
                return 0;
            }
        }
        #endregion
        #region ExecuteSQL
        public static int ExecuteSQL(OracleConnection connection, OracleCommand cmd, string commandText, params OracleParameter[] commandParameters)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters);
            return cmd.ExecuteNonQuery();
        }
        public static int ExecuteSQL(OracleConnection connection, OracleCommand cmd, string commandText)
        {
            PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null);
            return cmd.ExecuteNonQuery();
        }    
        public static int ExecuteSQL(OracleConnection conn, string sqlString, params OracleParameter[] parameterValues)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, conn, (OracleTransaction)null, CommandType.Text, sqlString, parameterValues);
                return ExecuteNonQuery(conn, CommandType.Text, sqlString, parameterValues);
            }
            catch (System.Exception ex)
            {
                conn.Close();
                conn.Dispose();
                LogHelper.WriteLog("ExecuteSQL数据库出错:" + ex.ToString());
                return 0;
            }
        }
        #endregion
        #region ExecuteSQLTransaction
        /// <summary>
        /// 通过事务执行SQL语句命令
        /// </summary>
        /// <param name="SQLStringList"></param>
        public static void ExecuteSQLTransaction(OracleConnection connection, List<string> SQLStringList)
        {

            connection.Open();
            OracleCommand command = new OracleCommand();
            command.Connection = connection;
            OracleTransaction transaction = connection.BeginTransaction();
            if (transaction != null)
            {
                //command.Transaction = transaction;
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                for (int i = 0; i < SQLStringList.Count; i++)
                {
                    string str = SQLStringList[i].ToString();
                    if (str.Trim().Length > 1)
                    {
                        command.CommandText = str;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
            catch (OracleException e)
            {
                transaction.Rollback();
                string stringlist = "";
                foreach (var str in SQLStringList)
                {
                    stringlist += str.ToString() + "\r\n";
                }
                LogHelper.WriteLog("MsOracle.ExecuteSQLTransaction", stringlist, e);
                throw new System.Exception(e.Message);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

        }
        #endregion
        #endregion

        #region ExecuteDataset
        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
            catch (System.Exception ex)
            {
                connection.Close();
                connection.Dispose();
                LogHelper.WriteLog("ExecuteDataset出错:" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);


                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }

            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            OracleDataAdapter da;
            DataSet ds = new DataSet();
            try
            {
                da = new OracleDataAdapter(cmd);
                da.Fill(ds);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    // Rollback the transaction
                    transaction.Rollback();
                }
            }

            //return the dataset
            return ds;
        }

        /// <summary>
        ///执行一个存储过程命令返回一个结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion

        #region DataTable
        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, string commandText)
        {
            return ExecuteTable(connection, CommandType.Text, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteTable(connection, commandType, commandText, (OracleParameter[])null);
        }
        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (System.Exception ex)
            {
                connection.Close();
                connection.Dispose();
                LogHelper.WriteLog("ExecuteDataset出错:" + ex.ToString());
                return null;
            }
        }
        public static DataTable ExecuteTable(OracleConnection connection, OracleCommand cmd, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
               
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (System.Exception ex)
            {
                connection.Close();
                connection.Dispose();
                LogHelper.WriteLog("ExecuteDataset出错:" + ex.ToString());
                return null;
            }
        }
        public static DataTable ExecuteTable(OracleConnection connection, OracleCommand cmd, string commandText)
        {
            try
            {

                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (System.Exception ex)
            {
                connection.Close();
                connection.Dispose();
                LogHelper.WriteLog("ExecuteDataset出错:" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);


                return ExecuteTable(connection, CommandType.StoredProcedure, spName, commandParameters);
            }

            else
            {
                return ExecuteTable(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteTable(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            OracleDataAdapter da;
            DataSet ds = new DataSet();
            try
            {
                da = new OracleDataAdapter(cmd);
                da.Fill(ds);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    // Rollback the transaction
                    transaction.Rollback();
                }
            }

            //return the dataset
            return ds.Tables[0];
        }

        /// <summary>
        ///执行一个存储过程命令返回一个结果集
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = MsOracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteTable(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteTable(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region ExecuteScalar
        /// <summary>
        /// 返回结果集的第一行第一列

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
            return cmd.ExecuteScalar();
        }
        /// <summary>
        /// 返回结果集的第一行第一列 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, string commandText)
        {
            return ExecuteScalar(connection, CommandType.Text, commandText, (OracleParameter[])null);
        }
        /// <summary>
        /// 返回结果集的第一行第一列 
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandText">存储过程T-OleDb的名称或命令</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, string commandText, params OracleParameter[] commandParameters)
        {
            return ExecuteScalar(connection, CommandType.Text, commandText, commandParameters);
        }
        #endregion

        #region ExecuteReader
        private enum OracleConnectionOwnership
        {
            /// <summary>连接是通过OracleProvider拥有和管理</summary>
            Internal,
            /// <summary>连接是拥有和管理的访客</summary>
            External
        }
        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, string commandText)
        {
            return ExecuteReader(connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null, OracleConnectionOwnership.External);
        }
        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, string commandText, params OracleParameter[] commandParameters)
        {
            return ExecuteReader(connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters, OracleConnectionOwnership.External);
        }
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader)
        /// </summary>   
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="transaction">有效的 OracleTransaction, or 'null'</param>
        /// <param name="commandType">命令类型 (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">以一个数组的形式返回OracleParameters </param>
        /// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by OracleProvider</param>
        /// <returns></returns>
        private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, OracleConnectionOwnership connectionOwnership)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);
            OracleDataReader dr = null;
            try
            {
                if (connectionOwnership == OracleConnectionOwnership.External)
                {
                    dr = cmd.ExecuteReader();
                }
                else
                {
                    //  dr = cmd.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                    dr = cmd.ExecuteReader();
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
                return (OracleDataReader)dr;
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                throw;
            }
        }
        #endregion

        #region AttachParameters 这个方法被用来连接到一个数组的OracleParameters OracleCommand。
        /// <summary>
        /// 这个方法被用来连接到一个数组的OracleParameters OracleCommand。
        /// 这个方法将会对DbNull赋值给任何参数的方向
        /// InputOutput和一个值为空。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandParameters"></param>
        private static void AttachParameters(OracleCommand command, OracleParameter[] commandParameters)
        {
            foreach (OracleParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }
                command.Parameters.Add(p);
            }
        }
        #endregion
        #region AssignParameterValues 该方法以一个数组的形式返回值指定数组的OracleParameters。
        /// <summary>
        /// 该方法以一个数组的形式返回值指定数组的OracleParameters。
        /// </summary>
        /// <param name="commandParameters">OracleParameters数组</param>
        /// <param name="parameterValues">对象的数组的值被指定</param>
        private static void AssignParameterValues(OracleParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }
            if (commandParameters.Length != parameterValues.Length)
            {
                LogHelper.WriteLog("MsOracle.AssignParameterValues", "参数不匹配参数值的个数", null);
                throw new ArgumentException("参数不匹配参数值的个数");
            }
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }
        #endregion
        #region PrepareCommand 打开(如果必要的话),指定一个命令、连接、事务、参数类型和参数
        /// <summary>
        ///打开(如果必要的话),指定一个命令、连接、事务、参数类型和参数      
        /// </summary>
        /// <param name="command">OracleCommand</param>
        /// <param name="connection">OracleConnection</param>
        /// <param name="transaction">OracleTransaction 或  'null'</param>
        /// <param name="commandType"> CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param> 
        /// <param name="commandParameters">OracleParameter[]</param>
        private static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                //command.Transaction = transaction;
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            command.CommandType = commandType;
            if (command.Parameters.Count > 0)
                command.Parameters.Clear();
            if (commandParameters != null)
            {
               
                AttachParameters(command, commandParameters);
            }          
            return;
        }
        #endregion
    }
    #endregion

}
