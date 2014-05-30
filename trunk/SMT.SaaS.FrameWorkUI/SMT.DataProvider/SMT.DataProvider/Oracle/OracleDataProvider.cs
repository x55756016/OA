using System;
using System.Data;
using System.Xml;
using Oracle.DataAccess.Client;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;

namespace SMT
{
    /// <summary>
    /// OracleProvider目的是封装高性能的、可扩展的
    /// </summary>
    public sealed class OracleDataProvider
    {
        #region 默认打开数据库
        private static OracleConnection conn;
        private static OracleCommand comm;
        private static OracleDataAdapter adpter;
        public static string ConnectionString
        {
            get
            {
               // return ConfigurationManager.ConnectionStrings["OracleConnectString"].ToString();
                return DataBaseConfig.GetConnectionString();
            }
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
        }
        #region DataTable
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
                catch (Oracle.DataAccess.Client.OracleException e)
                {
                    conn.Close();
                    throw new Exception(e.Message);
                }
                finally
                {
                    conn.Close();
                }
                return dataset.Tables[0];
            }
        }
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
                catch (Oracle.DataAccess.Client.OracleException e)
                {
                    conn.Close();
                    throw new Exception(e.Message);
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
            catch (Oracle.DataAccess.Client.OracleException e)
            {
                connection.Close();
                throw new Exception(e.Message);
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
            catch (Oracle.DataAccess.Client.OracleException e)
            {
                connection.Close();
                throw new Exception(e.Message);
            }
        }
        #endregion
        #region 返回表中的记录总数
        /// <summary>
        /// 返回表中的记录总数,OracleParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="cmdParms">OracleParameter[] 参数</param>
        /// <returns></returns>
        public static int GetRecordCount(string tableName, params OracleParameter[] cmdParms)
        {
            int recordCount = 0;
            string sql = "select count(0) as co from " + tableName + "";
            OracleDataReader dr = GetOracleDataReader(sql, cmdParms);
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
        /// 执行一条计算查询结果语句，返回查询结果:第一行第一列的值,带参数SqlParameter[]
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
                catch (Oracle.DataAccess.Client.OracleException e)
                {
                    connection.Close();
                    throw new Exception(e.Message);
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
                catch (Oracle.DataAccess.Client.OracleException e)
                {
                    connection.Close();
                    throw new Exception(e.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        #endregion
        #region 执行SQL语句
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
        /// <returns>影响记录数</returns>
        public static int ExecuteSQL(string sqlString, params OracleParameter[] param)
        {
            using (OracleConnection conn = new OracleConnection(ConnectionString))
            {
                return ExecuteNonQuery(conn, CommandType.Text, sqlString, param);
            }
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
                catch (OracleException exception)
                {
                    transaction.Rollback();
                    throw new Exception(exception.Message);
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
                catch (OracleException exception)
                {
                    transaction.Rollback();
                    throw new Exception(exception.Message);
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
            catch (OracleException exception)
            {
                transaction.Rollback();
                throw new Exception(exception.Message);
            }
            finally
            {
                connection.Close();
            }

        }
        #endregion
        #endregion
        #region 构造函数和私有方法
        private OracleDataProvider() { }
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
            return;
        }
        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 执行一个命令，没有返回数据集，只返回影响记录数   
        /// 没有参数
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
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
        /// 有参数
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
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
        /// </summary>
        /// <remarks>
        /// 这种方法并没有提供存取到输出参数或存储过程的返回值参数。
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues"> params object[]</param>
        /// <returns>影响记录数</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connectionString, spName);
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
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
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
        /// e.g.:  
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
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行一个存储过程，没有返回数据集，只返回影响记录数 
        /// 这个方法并没有提供存取到输出参数或存储过程的返回值参数。
        /// </summary>
        /// <remarks>
        /// 这种方法并没有提供存取到输出参数或存储过程的返回值参数
        /// 
        /// e.g.:  
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
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);
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
        /// e.g.:  
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
        /// e.g.:  
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
        /// e.g.:  
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
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);
                AssignParameterValues(commandParameters, parameterValues);
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery

        #region ExecuteDataSet

        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <remarks>
        /// e.g.:  
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
        /// e.g.:  
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
        /// e.g.:  
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
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connectionString, spName);
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
            //    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(ConnectionString, spName, true);
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
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteDataset(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

            //create the DataAdapter & DataSet
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            //return the dataset
            return ds;
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteDataset(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset) against the specified OracleTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            //create the DataAdapter & DataSet
            OracleDataAdapter da;
            DataSet ds = new DataSet();
            try
            {
                da = new OracleDataAdapter(cmd);
                //fill the DataSet using default values for DataTable names, etc.
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
        /// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataSet

        #region ExecuteReader

        /// <summary>
        /// this enum is used to indicate weather the connection was provided by the caller, or created by OracleProvider, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum OracleConnectionOwnership
        {
            /// <summary>Connection is owned and managed by OracleProvider</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }


        /// <summary>
        /// Create and prepare an OracleCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection, on which to execute this command</param>
        /// <param name="transaction">有效的 OracleTransaction, or 'null'</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">an array of OracleParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">indicates whether the connection parameter was provided by the caller, or created by OracleProvider</param>
        /// <returns>OracleDataReader containing the results of the command</returns>
        private static OracleDataReader ExecuteReader(OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText, OracleParameter[] commandParameters, OracleConnectionOwnership connectionOwnership)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

            //create a reader
            OracleDataReader dr = null;

            try
            {
                // call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == OracleConnectionOwnership.External)
                {
                    dr = cmd.ExecuteReader();
                }
                else
                {
                    dr = cmd.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
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
                    // Rollback the transaction
                    transaction.Rollback();
                }
                throw;
            }
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteReader(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create & open an OraclebConnection
            OracleConnection cn = new OracleConnection(connectionString);
            cn.Open();

            try
            {
                //call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(cn, null, commandType, commandText, commandParameters, OracleConnectionOwnership.Internal);
            }
            catch
            {
                //if we fail to return the OracleDataReader, we need to close the connection ourselves
                cn.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via an OracleCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset and takes no parameters) against the provided OracleConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteReader(connection, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset) against the specified OracleConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, (OracleTransaction)null, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>  
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteReader(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        /// Execute an OracleCommand (that returns a resultset) against the specified OracleTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   OracleDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, OracleConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via an OracleCommand (that returns a resultset) against the specified
        /// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  OracleDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>an OracleDataReader containing the resultset generated by the command</returns>
        public static OracleDataReader ExecuteReader(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
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
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
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
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
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
        /// the conneciton string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 返回结果集的第一行第一列
        /// </summary>
        /// <remarks>
        /// e.g.:  
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
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-OleDb command</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

            //execute the command & return the results
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集的第一行第一列
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleConnection connection, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 返回结果集的第一行第一列 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-OleDb command</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteScalar(transaction, commandType, commandText, (OracleParameter[])null);
        }

        /// <summary>
        ///返回结果集的第一行第一列
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-OleDb command</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);

            try
            {
                //execute the command & return the results
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
                    // Rollback the transaction
                    transaction.Rollback();
                }
                return null;
            }

        }

        /// <summary>
        /// 执行一个存储过程并返回结果集的第一行第一列
        /// OracleTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// 这个方法并没有提供存取到输出参数或返回值参数.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回结果集的第一行第一列</returns>
        public static object ExecuteScalar(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            //if we got parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                //pull the parameters for this stored procedure from the parameter cache (or discover them & populet the cache)
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                //assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                //call the overload that takes an array of OracleParameters
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            //otherwise we can just call the SP without params
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar
    }

    /// <summary>
    ///缓存参数类
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class OracleProviderParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new OracleProviderParameterCache()".
        private OracleProviderParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// resolve at run-time the appropriate set of OracleParameters for a stored procedure
        /// </summary>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">whether or not to include ther return value parameter</param>
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

        //deep copy of cached OracleParameter array
        private static OracleParameter[] CloneParameters(OracleParameter[] originalParameters)
        {
            OracleParameter[] clonedParameters = new OracleParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (OracleParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="commandText">the stored procedure name or T-OleDb command</param>
        /// <param name="commandParameters">an array of OracleParameters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params OracleParameter[] commandParameters)
        {
            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="commandText">the stored procedure name or T-OleDb command</param>
        /// <returns>an array of OracleParameters</returns>
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

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of OracleParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <returns>an array of OracleParameters</returns>
        public static OracleParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of OracleParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">有效的 connection string for an OracleConnection</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">a bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>an array of OracleParameters</returns>
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

        #endregion Parameter Discovery Functions

    }
}
