/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：OracleDataProvider.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/7 14:05:28   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.DataAccess 
	 * 模块名称：数据库操作封装层类库
	 * 描　　述： 微软公司的 System.Data.OracleClient 目的是封装高性能的、可扩展的
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Collections;
using System.Data;

namespace SMT.Workflow.Common.DataAccess
{
    #region 微软公司的 System.Data.OracleClient 数据库操作类

    /// <summary>
    ///  微软公司的 System.Data.OracleClient 数据库操作类
    /// </summary>
    public sealed class OracleDataProvider
    {
        #region 获取数据库连接字符串
        /// <summary>
        /// 获取数据库连接字符串 如:
        ///   connectionStrings
        ///      add name="WorkFlowConnString" providerName="System.Data.OracleClient" connectionString="Data Source=smtsaasdb209;user id=smtwf;password=smtwf;"
        ///  connectionStrings>
        /// </summary>
        /// <param name="connectionKey">配置文件如:web.config;关键字</param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionKey)
        {
            try
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message);
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
                throw new System.Exception(ex.Message);
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
                    throw new System.Exception(ex.Message);
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
                    throw new System.Exception(ex.Message);
                }
            }
        }
        #endregion

        #region 执行SQL语句
        #region ExecuteNonQuery 执行完成没有关闭OracleConnection连接,需要手动关闭
        /// <summary>
        /// 执行SQL语句命令(通过OracleConnection,OracleCommand);执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmd">OracleCommand 对象</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string sql, params OracleParameter[] commandParameters)
        {
            try
            {
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, commandParameters);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句命令(通过OracleConnection,OracleCommand);执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmd">OracleCommand 对象</param>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(OracleConnection connection, OracleCommand cmd, string sql)
        {
            try
            {
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, (OracleParameter[])null);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        ///  执行SQL语句命令(通过OracleConnection);执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="commandType">命令类型(SQL语句或存储过程)</param>
        /// <param name="commandText">命令</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        #endregion
        #region ExecuteSQL 执行完成自动关闭OracleConnection连接
        /// <summary>
        /// 执行SQL语句命令(通过OracleConnection,OracleCommand);执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmd">OracleCommand 对象</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandParameters">参数数组</param>
        /// <returns></returns>
        public static int ExecuteSQL(OracleConnection connection, OracleCommand cmd, string sql, params OracleParameter[] commandParameters)
        {
            try
            {
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, commandParameters);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    cmd.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    cmd.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行SQL语句命令(通过OracleConnection,OracleCommand);执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="cmd">OracleCommand 对象</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static int ExecuteSQL(OracleConnection connection, OracleCommand cmd, string sql)
        {
            try
            {
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, (OracleParameter[])null);
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    cmd.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                    cmd.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行SQL语句命令(通过OracleConnection);执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="parameterValues">参数数组,可以为Null</param>
        /// <returns></returns>
        public static int ExecuteSQL(OracleConnection connection, string sqlString, params OracleParameter[] parameterValues)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sqlString, parameterValues);
                return cmd.ExecuteNonQuery();               
            }
            catch (System.Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        #endregion
        #region 通过事务执行多条SQL语句命令
        /// <summary>
        /// 通过事务执行SQL语句命令
        /// </summary>
        /// <param name="connection">OracleConnection</param>
        /// <param name="SQLStringList">SQL语句列表</param>
        public static void ExecuteSQLTransaction(OracleConnection connection, List<string> SQLStringList)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
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
                // LogHelper.WriteLog("MsOracle.ExecuteSQLTransaction", stringlist, e);
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

        #region ExecuteDataset 执行完成没有关闭OracleConnection连接,需要手动关闭
        /// <summary>
        /// 执行SQL/存储过程返回 DataSet ;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleConnection connection, CommandType commandType, string commandText)
        {
            try
            {
                return ExecuteDataset(connection, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行SQL/存储过程返回  ;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">参数数组 OracleParameter[]</param>
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
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个存储过程并返回结果集 ;执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                    AssignParameterValues(commandParameters, parameterValues);

                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                else
                {
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
                }
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行事务一个命令返回一个结果集. ;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataSet ExecuteDataset(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            try
            {
                return ExecuteDataset(transaction, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集;执行完成没有关闭OracleConnection连接,需要手动关闭

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
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
            catch (System.Exception e)
            {
                if (transaction != null)
                {
                    // Rollback the transaction
                    transaction.Rollback();
                }
                throw new System.Exception(e.Message, e);
            }

            //return the dataset
            return ds;
        }

        /// <summary>
        ///执行一个存储过程命令返回一个结果集;执行完成没有关闭OracleConnection连接,需要手动关闭
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
                OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion

        #region DataTable ;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="sql">SQL语句</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, string sql)
        {
            try
            {
                return ExecuteTable(connection, CommandType.Text, sql, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleConnection connection, CommandType commandType, string commandText)
        {
            try
            {
                return ExecuteTable(connection, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        ///执行SQL语句,返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
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
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="cmd">OracleCommand对象</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static DataTable ExecuteTable(OracleConnection connection, OracleCommand cmd, string sql, params OracleParameter[] commandParameters)
        {
            try
            {

                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="cmd">OracleCommand对象</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataTable ExecuteTable(OracleConnection connection, OracleCommand cmd, string sql)
        {
            try
            {

                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, (OracleParameter[])null);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行一个存储过程并返回DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                    AssignParameterValues(commandParameters, parameterValues);


                    return ExecuteTable(connection, CommandType.StoredProcedure, spName, commandParameters);
                }

                else
                {
                    return ExecuteTable(connection, CommandType.StoredProcedure, spName);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集DataTable;执行完成没有关闭OracleConnection连接,需要手动关闭
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataTable ExecuteTable(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            try
            {
                return ExecuteTable(transaction, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集;执行完成没有关闭OracleConnection连接,需要手动关闭

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
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
        /// 这个方法并没有提供存取到输出参数或返回值参数.执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                    AssignParameterValues(commandParameters, parameterValues);

                    return ExecuteTable(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                else
                {
                    return ExecuteTable(transaction, CommandType.StoredProcedure, spName);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }

        }
        #endregion

        #region GetDataTable ;执行完成自动关闭OracleConnection连接
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="sql">SQL语句</param>
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleConnection connection, string sql)
        {
            try
            {
                return GetDataTable(connection, CommandType.Text, sql, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">OracleConnection</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandParameters">OracleParameter[]</param>
        /// <returns></returns>
        public static DataTable GetDataTable(OracleConnection connection, string sql, params OracleParameter[] commandParameters)
        {
            try
            {
                return GetDataTable(connection, CommandType.Text, sql, (OracleParameter[])commandParameters);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param>
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleConnection connection, CommandType commandType, string commandText)
        {
            try
            {
                return GetDataTable(connection, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        ///执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(conn, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleConnection connection, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                connection.Close();
                connection.Dispose();
                return ds.Tables[0];
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="cmd">OracleCommand对象</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns></returns>
        public static DataTable GetDataTable(OracleConnection connection, OracleCommand cmd, string sql, params OracleParameter[] commandParameters)
        {
            try
            {

                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, commandParameters);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                connection.Close();
                connection.Dispose();
                return ds.Tables[0];
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行SQL语句,返回DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <param name="connection">有效的 OracleConnection</param>
        /// <param name="cmd">OracleCommand对象</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataTable GetDataTable(OracleConnection connection, OracleCommand cmd, string sql)
        {
            try
            {

                PrepareCommand(cmd, connection, (OracleTransaction)null, CommandType.Text, sql, (OracleParameter[])null);

                OracleDataAdapter da = new OracleDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                connection.Close();
                connection.Dispose();
                return ds.Tables[0];
            }
            catch (System.Exception e)
            {
                connection.Close();
                connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行一个存储过程并返回DataTable;执行完成自动关闭OracleConnection连接
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
        public static DataTable GetDataTable(OracleConnection connection, string spName, params object[] parameterValues)
        {
            try
            {
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(connection.ConnectionString, spName);

                    AssignParameterValues(commandParameters, parameterValues);


                    return GetDataTable(connection, CommandType.StoredProcedure, spName, commandParameters);
                }

                else
                {
                    return GetDataTable(connection, CommandType.StoredProcedure, spName);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个命令返回一个结果集DataTable;执行完成自动关闭OracleConnection连接
        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleTransaction transaction, CommandType commandType, string commandText)
        {
            try
            {
                return GetDataTable(transaction, commandType, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }

        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集;执行完成自动关闭OracleConnection连接

        /// </summary>
        /// <remarks>
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, CommandType.StoredProcedure, "GetOrders", new OracleParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="commandType">命令类型:SQL语句或存储过程</param>
        /// <param name="commandText">存储过程名称或 PL/SQL</param> 
        /// <param name="commandParameters">params OracleParameter[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleTransaction transaction, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
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
        /// 这个方法并没有提供存取到输出参数或返回值参数.执行完成自动关闭OracleConnection连接
        /// 
        /// 例如：  
        ///  DataTable ds = ExecuteDataTable(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">有效的 OracleTransaction</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">params object[]</param>
        /// <returns>返回DataSet</returns>
        public static DataTable GetDataTable(OracleTransaction transaction, string spName, params object[] parameterValues)
        {
            try
            {
                if ((parameterValues != null) && (parameterValues.Length > 0))
                {
                    OracleParameter[] commandParameters = OracleProviderParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName);

                    AssignParameterValues(commandParameters, parameterValues);

                    return GetDataTable(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                else
                {
                    return GetDataTable(transaction, CommandType.StoredProcedure, spName);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }

        }
        #endregion
       
        #region ExecuteScalar 执行完成没有关闭OracleConnection连接,需要手动关闭
        /// <summary>
        /// 返回结果集的第一行第一列; 执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, (OracleTransaction)null, commandType, commandText, commandParameters);
                return cmd.ExecuteScalar();
            }
            catch (System.Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 返回结果集的第一行第一列 ; 执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                return ExecuteScalar(connection, CommandType.Text, commandText, (OracleParameter[])null);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 返回结果集的第一行第一列 ; 执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                return ExecuteScalar(connection, CommandType.Text, commandText, commandParameters);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        #endregion

        #region ExecuteReader 执行完成没有关闭OracleConnection连接,需要手动关闭
        private enum OracleConnectionOwnership
        {
            /// <summary>连接是通过OracleProvider拥有和管理</summary>
            Internal,
            /// <summary>连接是拥有和管理的访客</summary>
            External
        }
        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集;执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                return ExecuteReader(connection, (OracleTransaction)null, CommandType.Text, commandText, (OracleParameter[])null, OracleConnectionOwnership.External);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行一个OracleCommand命令并返回结果集;执行完成没有关闭OracleConnection连接,需要手动关闭
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
            try
            {
                return ExecuteReader(connection, (OracleTransaction)null, CommandType.Text, commandText, commandParameters, OracleConnectionOwnership.External);
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 执行一个OracleCommand(返回一个结果OracleDataReader);执行完成没有关闭OracleConnection连接,需要手动关闭
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
            catch (System.Exception e)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                throw new System.Exception(e.Message, e);
            }
        }
        #endregion

        #region 事务处理
        /// <summary>
        /// 事务对象
        /// </summary>
        // private OracleTransaction Transaction { get; set; }
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public OracleCommand BeginTransaction(OracleConnection connObjection)
        {
            try
            {
                OracleCommand cmd = new OracleCommand();
                OracleTransaction Transaction = null;
                if (Transaction == null || Transaction.Connection != connObjection)
                {
                    if (connObjection != null && connObjection.State == ConnectionState.Open)
                    {
                        cmd.Connection = connObjection;
                        Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                        cmd.Transaction = Transaction;
                    }
                    else
                    {
                        connObjection.Open();
                        cmd.Connection = connObjection;
                        Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                        cmd.Transaction = Transaction;
                    }
                }
                return cmd;
            }
            catch (System.Exception e)
            {
                connObjection.Close();
                connObjection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns></returns>
        public OracleCommand BeginTransaction(string connectionString)
        {
            try
            {
                OracleConnection connObjection = CreateOracleConnection(connectionString);
                OracleCommand cmd = new OracleCommand();
                OracleTransaction Transaction = null;
                if (connObjection != null && connObjection.State == ConnectionState.Open)
                {
                    cmd.Connection = connObjection;
                    Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd.Transaction = Transaction;
                }
                else
                {
                    connObjection.Open();
                    cmd.Connection = connObjection;
                    Transaction = connObjection.BeginTransaction(IsolationLevel.ReadCommitted);
                    cmd.Transaction = Transaction;
                }

                return cmd;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(e.Message, e);
            }
        }
        #endregion
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="conn">OracleCommand对象</param>
        public void CommitTransaction(OracleCommand cmd)
        {

            if (cmd.Transaction != null)
            {
                try
                {
                    cmd.Transaction.Commit();
                }
                catch (OracleException e)
                {
                    cmd.Transaction.Rollback();
                    if (cmd.Transaction.Connection.State == ConnectionState.Open)
                    {
                        Close(cmd.Transaction.Connection);
                    }
                    cmd.Transaction.Dispose();
                    cmd.Transaction = null;
                    throw new System.Exception(e.Message, e);
                }
                finally
                {
                    if (cmd.Transaction.Connection.State == ConnectionState.Open)
                    {
                        Close(cmd.Transaction.Connection);
                    }
                    cmd.Transaction.Dispose();
                    cmd.Transaction = null;
                    cmd = null;
                }
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTransaction(OracleCommand cmd)
        {
            if (cmd.Transaction != null)
            {
                try
                {
                    cmd.Transaction.Rollback();
                    if (cmd.Transaction.Connection.State == ConnectionState.Open)
                    {
                        Close(cmd.Transaction.Connection);
                    }
                    cmd.Transaction.Dispose();
                    cmd.Transaction = null;
                    cmd = null;
                }
                catch (OracleException e)
                {
                    cmd.Transaction.Rollback();
                    if (cmd.Transaction.Connection.State == ConnectionState.Open)
                    {
                        Close(cmd.Transaction.Connection);
                    }
                    cmd.Transaction.Dispose();
                    cmd.Transaction = null;
                    cmd = null;
                    throw new System.Exception(e.Message, e);
                }
            }
        }
        #region ExecuteSQLByTransaction
        /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleTransaction Transaction)方法
        /// </summary>
        /// <param name="OracleCommand">OracleCommand</param>
        /// <param name="sql">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public int ExecuteSQLByTransaction(OracleCommand cmd, string sql, params OracleParameter[] parameterValues)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                cmd.CommandText = sql;
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
                }
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                throw new System.Exception(e.Message, e);
            }
        }              /// <summary>
        /// 通过事务执行SQL语命令，执行该方法前，先执行BeginTransaction(OracleConnection connObjection) 方法，最后执行CommitTransaction(OracleTransaction Transaction)方法
        /// </summary>
        /// <param name="cmd">OracleCommand</param>
        /// <param name="sqlString">sql 命令语句</param>
        /// <param name="parameterValues">OracleParameter[]</param>
        /// <returns></returns>
        public int ExecuteSQLByTransaction(OracleCommand cmd, string sqlString)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }
                cmd.CommandText = sqlString;
                cmd.CommandType = CommandType.Text;
                return cmd.ExecuteNonQuery();
            }
            catch (System.Exception e)
            {
                if (cmd.Transaction != null)
                {
                    cmd.Transaction.Rollback();
                }
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                throw new System.Exception(e.Message);
            }
        }

        #endregion
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
                // LogHelper.WriteLog("MsOracle.AssignParameterValues", "参数不匹配参数值的个数", null);
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
        #region  获取值，如果obj是null，近回DBNull.Value=数据库的NULL,否则返回obj
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
    }
    #endregion
    #region 缓存参数类
    /// <summary>
    ///缓存参数类    
    /// </summary>
    public sealed class OracleProviderParameterCache
    {
        #region 私人方法、变量,并构造函数
        //因为这个类只提供了静态方法,使默认构造函数
        //实例被创建 new OracleProviderParameterCache()
        private OracleProviderParameterCache() { }
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

        #endregion

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
    #endregion
}
