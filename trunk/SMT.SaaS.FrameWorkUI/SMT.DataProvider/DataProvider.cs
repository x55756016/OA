using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Configuration;
namespace SMT
{
    /// <summary>
    /// 数据处理返回的结果类
    /// </summary>
    public class DataResult
    {
        /// <summary>
        /// 返回SQL语句
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// 影响的记录数
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// DataTable
        /// </summary>
        public DataTable DataTable { get; set; }
        /// <summary>
        /// DataSet
        /// </summary>
        public DataSet DataSet { get; set; }
        /// <summary>
        /// IDataReader
        /// </summary>
        public IDataReader IDataReader { get; set; }
        /// <summary>
        /// IDbDataParameter
        /// </summary>
        public IDbDataParameter[] IDbDataParameter { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }
    }
    public static class DataProvider
    {

        // public static static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        #region 操作对象 成员
        private static IDbConnection _idbConnection;
        // private static IDataReader _iDataReader;
        private static IDbCommand _idbCommand;
        private static IDbTransaction _idbTransaction;
        private static IDbDataParameter[] _idbParameters;
        private static IDbDataAdapter _idbDataAdapter;

        public static IDbConnection IDbConn
        {
            get
            {
                return _idbConnection;
            }
            set
            {
                _idbConnection = value;
            }
        }

        public static IDbCommand IDbComm
        {
            get
            {
                return _idbCommand;
            }
            set
            {
                _idbCommand = value;
            }
        }

        public static IDbDataAdapter IDbAdpter
        {
            get
            {
                return _idbDataAdapter;
            }
            set
            {
                _idbDataAdapter = value;
            }
        }

        public static IDbTransaction IDbTrans
        {
            get
            {
                return _idbTransaction;
            }
            set
            {
                _idbTransaction = value;
            }
        }

        public static IDbDataParameter[] IDbParams
        {
            get
            {
                return _idbParameters;
            }
            set
            {
                _idbParameters = value;
            }
        }

        #endregion
        #region 数据库连接字符串
        private static string connectionString;
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    return DataBaseConfig.GetConnectionString();
                }
                else
                {
                    return connectionString;
                }
            }
            set
            {
                connectionString = value;
            }
        }
        #endregion
        #region 参数设置
        /// <summary>
        /// 创建参数数量
        /// </summary>
        /// <param name="paramsCount">参数个数</param>
        public static void CreateParameters(int paramsCount)
        {
            //IDbParams = new IDbDataParameter[paramsCount];
            IDbParams = DataBaseFactory.GetIDbParameters(paramsCount);
        }
        /// <summary>
        /// 增加参数值(必须先实现CreateParameters)
        /// </summary>
        /// <param name="index">索引;从0开始</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="objValue">参数值</param>
        public static void AddParameters(int index, string paramName, object objValue)
        {
            if (index < IDbParams.Length)
            {
                IDbParams[index].ParameterName = paramName;
                IDbParams[index].Value = objValue;
                //IDbParams[index].DbType = DbType.DateTime;
                //IDbParams[index].Size = 50;
                //IDbParams[index].Direction = ParameterDirection.Input;               
            }
        }
        /// <summary>
        ///  增加参数值(必须先实现CreateParameters)
        /// </summary>
        /// <param name="index">索引;从0开始</param>
        /// <param name="dbtype">字段类型</param>
        /// <param name="size">字段长度</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="objValue">参数值</param>
        public static void AddParameters(int index, DbType dbtype, int size, string paramName, object objValue)
        {
            if (index < IDbParams.Length)
            {
                IDbParams[index].ParameterName = paramName;
                IDbParams[index].Value = objValue;
                IDbParams[index].DbType = dbtype;
                IDbParams[index].Size = size;
            }
        }
        /// <summary>
        /// 增加参数值(必须先实现CreateParameters)
        /// </summary>
        /// <param name="index">索引;从0开始</param>
        /// <param name="dbtype">字段类型</param>
        /// <param name="size">字段长度</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="objValue">参数值</param>
        /// <param name="direction">方向</param>
        public static void AddParameters(int index, DbType dbtype, int size, string paramName, object objValue, ParameterDirection direction)
        {
            if (index < IDbParams.Length)
            {
                IDbParams[index].ParameterName = paramName;
                IDbParams[index].Value = objValue;
                IDbParams[index].DbType = dbtype;
                IDbParams[index].Size = size;
                IDbParams[index].Direction = direction;
            }
        }
        /// <summary>
        /// 清除IDbComm所有的参数
        /// </summary>
        public static void ClearParameters()
        {
            if (IDbComm.Parameters.Count > 0)
                IDbComm.Parameters.Clear();
        }
        #region 私有方法

        //准备命令
        private static void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction,
                                    CommandType commandType, string commandText, IDbDataParameter[] commandParameters)
        {
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }
        //附加参数
        private static void AttachParameters(IDbCommand command, IDbDataParameter[] commandParameters)
        {
            foreach (IDbDataParameter idbParameter in commandParameters)
            {
                if (idbParameter.Direction == ParameterDirection.InputOutput && idbParameter.Value == null)
                {
                    idbParameter.Value = DBNull.Value;
                }
                command.Parameters.Add(idbParameter);
            }
        }


        #endregion
        #endregion
        #region 事务处理
        /// <summary>
        /// 开始事务
        /// </summary>
        public static IDbCommand BeginTransaction()
        {
            IDbConn = DataBaseFactory.GetIDbConnection();
            IDbConn.Open();
            IDbComm = DataBaseFactory.GetIDbCommand();
            IDbTrans = IDbConn.BeginTransaction(IsolationLevel.ReadCommitted);
            IDbComm.Connection = IDbConn;
            IDbComm.Transaction = IDbTrans;
            return IDbComm;
        }
        /// <summary>
        /// 开始事务
        /// </summary>
        public static IDbConnection BeginTransaction(string connectionString)
        {
            IDbConn = DataBaseFactory.GetIDbConnection(connectionString);
            IDbConn.Open();
            IDbComm = DataBaseFactory.GetIDbCommand(IDbConn);
            IDbTrans = IDbConn.BeginTransaction(IsolationLevel.ReadCommitted);
            IDbComm.Connection = IDbConn;
            IDbComm.Transaction = IDbTrans;
            return IDbConn;
        }
        /// <summary>
        /// 通过事务提执行SQL语句(必须放在BeginTransaction()方法与CommitTransaction()方法中间)
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public static DataResult ExecuteSQLByTransaction(string sql)
        {
            DataResult result = new DataResult();
            sql = DataBaseFactory.GetSQL(sql);
             result.Sql = sql +";";
            try
            {
                PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, IDbParams);
                int rows = IDbComm.ExecuteNonQuery();
                IDbComm.Parameters.Clear();

            }
            catch (Exception E)
            {
                result.Error = E.Message;
                IDbConn.Close();              
            }
            return result;
        }
        /// <summary>
        /// 通过事务提执行SQL语句(必须放在BeginTransaction()方法与CommitTransaction()方法中间)
        /// </summary>
        /// <param name="idbconn">IDbConnection</param>
        ///  <param name="sql">SQL语句</param>
        public static DataResult ExecuteSQLByTransaction(IDbCommand iDbComm, string sql)
        {

            DataResult result = new DataResult();
            sql = DataBaseFactory.GetSQL(sql);
             result.Sql = sql +";";
            try
            {
                PrepareCommand(iDbComm, iDbComm.Connection, iDbComm.Transaction, CommandType.Text, sql, IDbParams);
                int rows = iDbComm.ExecuteNonQuery();
                iDbComm.Parameters.Clear();

            }
            catch (Exception E)
            {
                iDbComm.Connection.Close();
                result.Error = E.Message;
            }
            return result;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public static DataResult CommitTransaction()
        {
            DataResult result = new DataResult();
            try
            {
                IDbTrans.Commit();
                return result;
            }
            catch (Exception exception)
            {
                IDbTrans.Rollback();
                IDbConn.Close();
                result.Error = exception.Message;
                return result;               
            }
            finally
            {
                IDbConn.Close();
            }
        }        
        #endregion


        #region IDataProvider 成员

        #region 数据库连接的打开与关闭
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public static void Open()
        {
            if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
            {
                IDbConn = DataBaseFactory.GetIDbConnection();
                IDbComm = DataBaseFactory.GetIDbCommand();
                IDbAdpter = DataBaseFactory.GetIDbDataAdapter();
                // IDbTrans = DataBaseFactory.GetIDbTransaction();
            }
            IDbComm.Connection = IDbConn;
            IDbAdpter.SelectCommand = IDbComm;
            IDbComm.CommandType = CommandType.Text;
            try
            {
                IDbConn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public static void Close()
        {
            if (IDbConn != null)
            {
                if (IDbConn.State == ConnectionState.Open)
                {
                    IDbConn.Close();
                    IDbConn.Dispose();
                }
                IDbConn = null;
                IDbComm = null;
                IDbAdpter = null;
                IDbTrans = null;
            }
        }
        #endregion
        #region SQL语句的操作
        public static DataResult ExistsRecord(string sql)
        {
            DataResult result = new DataResult();
             result.Sql = sql +";";
            IDataReader ireader = null;
            try
            {
                if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
                {
                    IDbConn = DataBaseFactory.GetIDbConnection();
                    IDbConn.Open();
                    IDbComm = DataBaseFactory.GetIDbCommand(sql);
                    //IDbComm.CommandText = sql;
                    //IDbComm.CommandType = CommandType.Text;
                    ireader = IDbComm.ExecuteReader();
                    if (ireader.Read())
                    {
                        result.RecordCount = 1;
                    }
                    else
                    {
                        result.RecordCount = 0;
                    }
                }
                else
                {
                    result.RecordCount = 1;
                }

            }
            catch (Exception e)
            {
                IDbConn.Close();
                result.Error = e.Message;
                return result; 
            }
            return result;
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataResult GetDataTable(string sql)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        IDbParams = null;
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, IDbParams);
                        IDbDataAdapter dataAdapter = DataBaseFactory.GetIDbDataAdapter();
                        dataAdapter.SelectCommand = IDbComm;
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        IDbComm.Parameters.Clear();
                   
                        result.DataTable = dataSet.Tables[0];
                        result.RecordCount = dataSet.Tables[0].Rows.Count;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.Error = E.Message;
                        return result; 
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
            return result;
            //using (IDbConn = DataBaseFactory.GetIDbConnection())
            //{
            //    IDbConn.Open();
            //    DataSet dataset = new DataSet();
            //    try
            //    {
            //        IDbComm = DataBaseFactory.GetIDbCommand(sql);
            //        //IDbComm.CommandText = sql;
            //        //IDbComm.CommandType = CommandType.Text;
            //        IDbAdpter = DataBaseFactory.GetIDbDataAdapter(IDbComm);
            //        //IDbAdpter.SelectCommand = IDbComm;
            //        IDbAdpter.Fill(dataset);
            //    }
            //    catch (Exception e)
            //    {
            //        IDbConn.Close();
            //        throw new Exception(e.Message);
            //    }
            //    finally
            //    {
            //        IDbConn.Close();
            //    }
            //    return dataset.Tables[0];
            //}
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataResult GetDataSet(string sql)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, IDbParams);
                        IDbDataAdapter dataAdapter = DataBaseFactory.GetIDbDataAdapter();
                        dataAdapter.SelectCommand = IDbComm;
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        IDbComm.Parameters.Clear();
                        result.DataSet = dataSet;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.Error = E.Message;
                        return result; 
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
            return result;
            //using (IDbConn = DataBaseFactory.GetIDbConnection())
            //{
            //    IDbConn.Open();
            //    DataSet dataset = new DataSet();
            //    try
            //    {
            //        IDbComm = DataBaseFactory.GetIDbCommand(sql);
            //        //IDbComm.CommandText = sql;
            //        //IDbComm.CommandType = CommandType.Text;
            //        IDbAdpter = DataBaseFactory.GetIDbDataAdapter(IDbComm);
            //        //IDbAdpter.SelectCommand = IDbComm;
            //        IDbAdpter.Fill(dataset);
            //    }
            //    catch (Exception e)
            //    {
            //        IDbConn.Close();
            //        throw new Exception(e.Message);
            //    }
            //    finally
            //    {
            //        IDbConn.Close();
            //    }
            //    return dataset;
            //}
        }
        /// <summary>
        /// 执行SQL语句返回结果集IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataResult GetDataReader(string sql)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            IDbConn = DataBaseFactory.GetIDbConnection();
            IDbConn.Open();
            IDbComm = DataBaseFactory.GetIDbCommand(IDbConn);
            try
            {
                PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, IDbParams);
                result.IDataReader= IDbComm.ExecuteReader();

            }
            catch (Exception E)
            {

                result.Error = E.Message;
                return result; 
            }
            return result;
            //  IDataReader ireader = null;
            //  try
            //  {
            //      if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
            //      {
            //          IDbConn = DataBaseFactory.GetIDbConnection();
            //          IDbConn.Open();
            //          IDbComm = DataBaseFactory.GetIDbCommand(sql);
            //          //IDbComm.CommandText = sql;
            //          //IDbComm.CommandType = CommandType.Text;
            //          // = IDbcmd.ExecuteReader();
            //          ireader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
            //      }
            //      return ireader;
            //  }           
            //catch (Exception e)
            //  {
            //       IDbConn.Close();
            //      return null;
            //      throw new Exception(e.Message);
            //  }
        }
        /// <summary>
        /// 执行SQL语句返回结果中上的第一行第一列的值,忽略额外的列和行
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql)
        {
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand(sql))
                {
                    try
                    {
                        //IDbComm.CommandText = sql;
                        //IDbComm.CommandType = CommandType.Text;
                        object rows = IDbComm.ExecuteScalar();
                        IDbConn.Close();
                        return rows;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        return null;
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
        }
        /// <summary>
        ///  执行SQL语句,并返加影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static DataResult ExecuteNonQuery(string sql)
        {
            DataResult result = new DataResult();
             result.Sql = sql +";";
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand(sql))
                {
                    try
                    {
                        int rows = IDbComm.ExecuteNonQuery();
                        IDbConn.Close();
                        result.RecordCount= rows;
                    }
                    catch (Exception E)
                    {
                        result.Error = E.Message;
                        IDbConn.Close();
                        result.RecordCount=0;
                        return result; 
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 通过事务执行多条SQL语句
        /// </summary>
        /// <param name="sqlList">SQL语句集</param>
        /// <returns></returns>
        public static DataResult ExecuteByTransaction(List<string> sqlList)
        {
            DataResult result = new DataResult();
           
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                IDbComm = DataBaseFactory.GetIDbCommand();
                IDbComm.Connection = IDbConn;
                IDbTrans = IDbConn.BeginTransaction(IsolationLevel.ReadCommitted);// DataBaseFactory.GetIDbTransaction();                
                if (IDbTrans != null)
                {
                    IDbComm.Transaction = IDbTrans;
                   // IDbTrans = IDbConn.BeginTransaction(IsolationLevel.ReadCommitted);
                    
                }
                try
                {
                    for (int i = 0; i < sqlList.Count; i++)
                    {
                        string str = sqlList[i].ToString();
                        if (str.Trim().Length > 1)
                        {
                            IDbComm.CommandText = str;
                           result.RecordCount+= IDbComm.ExecuteNonQuery();
                        }
                    }
                    IDbTrans.Commit();
                    return result;
                }
                catch (Exception exception)
                {

                    IDbTrans.Rollback();
                    IDbConn.Close();
                    result.Error = exception.Message;
                    return result;
                   
                }
                finally
                {
                    IDbConn.Close();
                }
            }
        }

        private static bool ExecuteByTransaction(Hashtable sqlList)
        {
            return false;
        }

        #endregion
        #region SQL语句的操作(参数)
        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static DataResult ExistsRecord(string sql, params IDbDataParameter[] cmdParms)
        {
           
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            result.IDbDataParameter = cmdParms;
          
            try
            {
                if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
                {
                    IDbConn = DataBaseFactory.GetIDbConnection();
                    IDbConn.Open();
                    IDbComm = DataBaseFactory.GetIDbCommand();
                    PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                    result.IDataReader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                    if (result.IDataReader.Read())
                    {
                        IDbComm.Parameters.Clear();
                        result.IDataReader.Close();
                        IDbConn.Close();
                        result.RecordCount = 1;
                    }
                    else
                    {
                        IDbConn.Close();
                        result.RecordCount = 0;
                    
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                IDbConn.Close();
                result.Error=e.Message;
                return result;
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static DataResult GetDataTable(string sql, params IDbDataParameter[] cmdParms)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            result.IDbDataParameter = cmdParms;
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                        IDbDataAdapter dataAdapter = DataBaseFactory.GetIDbDataAdapter();
                        dataAdapter.SelectCommand = IDbComm;
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        IDbComm.Parameters.Clear();
                        result.DataTable= dataSet.Tables[0];
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.Error=E.Message;
                        return result; 
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static DataResult GetDataSet(string sql, params IDbDataParameter[] cmdParms)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            result.IDbDataParameter = cmdParms;
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                        IDbDataAdapter dataAdapter = DataBaseFactory.GetIDbDataAdapter();
                        dataAdapter.SelectCommand = IDbComm;
                        DataSet dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        IDbComm.Parameters.Clear();
                        result.DataSet= dataSet;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.Error=E.Message;
                        return result;                       
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 执行SQL语句返回结果集IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static DataResult GetDataReader(string sql, params IDbDataParameter[] cmdParms)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            result.IDbDataParameter = cmdParms;
            try
            {
                if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
                {
                    IDbConn = DataBaseFactory.GetIDbConnection();
                    IDbConn.Open();
                    IDbComm = DataBaseFactory.GetIDbCommand();
                    PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                    // = IDbcmd.ExecuteReader();
                    result.IDataReader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                    return result;
                    // IDbComm.Parameters.Clear();
                }
                else
                {
                    return result;
                }
               
            }
            catch (Exception e)
            {
                IDbConn.Close();
                result.Error = e.Message;
                return result;
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集object
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, params IDbDataParameter[] cmdParms)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                        object rows = IDbComm.ExecuteScalar();
                        IDbConn.Close();
                        IDbComm.Parameters.Clear();
                        return rows;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        return null;
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 执行SQL语句,并返加影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public static DataResult ExecuteNonQuery(string sql, params IDbDataParameter[] cmdParms)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
            result.IDbDataParameter = cmdParms;
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);
                        int rows = IDbComm.ExecuteNonQuery();
                        IDbConn.Close();
                        IDbComm.Parameters.Clear();
                        result.RecordCount= rows;
                        return result;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.RecordCount = 0;
                        result.Error = E.Message;
                        return result;
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
        }
        public static DataResult ExecuteSQL(string sql)
        {
            sql = DataBaseFactory.GetSQL(sql);
            DataResult result = new DataResult();
             result.Sql = sql +";";
     
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand())
                {
                    try
                    {
                        PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, IDbParams);
                        int rows = IDbComm.ExecuteNonQuery();
                        IDbConn.Close();
                        IDbComm.Parameters.Clear();
                        result.RecordCount= rows;
                        return result;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        result.RecordCount = 0;
                        result.Error = E.Message;
                        return result;
                    }
                    finally
                    {
                        IDbConn.Close();
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
