using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Configuration;
namespace SMT
{
    public  class SMTDataProvider:IDataProvider,IDisposable
    {
        // public static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        #region 操作对象 成员       
        private IDbConnection _idbConnection;
       // private IDataReader _iDataReader;
        private IDbCommand _idbCommand;
        private IDbTransaction _idbTransaction;
        private IDbDataParameter[] _idbParameters;
        private IDbDataAdapter _idbDataAdapter;

        public IDbConnection IDbConn
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

        public IDbCommand IDbComm
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

        public IDbDataAdapter IDbAdpter
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

        public IDbTransaction IDbTrans
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

        public IDbDataParameter[] IDbParams
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
        private string connectionString;
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString
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
        public void CreateParameters(int paramsCount)
        {          
          //  IDbParams = new IDbDataParameter[paramsCount];
            IDbParams = DataBaseFactory.GetIDbParameters(paramsCount);
        }
        /// <summary>
        /// 增加参数值(必须先实现CreateParameters)
        /// </summary>
        /// <param name="index">索引;从0开始</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="objValue">参数值</param>
        public void AddParameters(int index, string paramName, object objValue)
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
        public void AddParameters(int index,DbType dbtype,int size, string paramName, object objValue)
        {
            if (index < IDbParams.Length)
            {
                IDbParams[index].ParameterName = paramName;
                IDbParams[index].Value = objValue;
                IDbParams[index].DbType = dbtype;
                IDbParams[index].Size= size;                          
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
        public void AddParameters(int index, DbType dbtype, int size, string paramName, object objValue, ParameterDirection direction)
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
        public void ClearParameters()
        {
            if (IDbComm.Parameters.Count > 0)
                IDbComm.Parameters.Clear();
        }
        #region 私有方法
      
        //准备命令
        private void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction,
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
        private void AttachParameters(IDbCommand command, IDbDataParameter[] commandParameters)
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
        public void BeginTransaction()
        {
            if (IDbTrans == null)
            {
                IDbTrans = DataBaseFactory.GetIDbTransaction();
            }
            IDbComm.Transaction = IDbTrans;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            if (IDbTrans != null)
            {
                IDbTrans.Commit();
            }
            IDbTrans = null;
        }
        #endregion
        public SMTDataProvider()
        {
            //IDbconn = DataBaseFactory.GetIDbConnection();
            //IDbcmd = DataBaseFactory.GetIDbCommand();
            //IDbadpter = DataBaseFactory.GetIDbDataAdapter();
            //IDbtrans = DataBaseFactory.GetIDbTransaction();
            ////dbparams = DataBaseFactory.GetIDbParameters(); 
        }

        #region IDataProvider 成员
       
        #region 数据库连接的打开与关闭        
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void Open()
        {
            if (IDbConn == null||IDbConn.State==ConnectionState.Closed)
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
        public void Close()
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
        public bool ExistsRecord(string sql)
        {
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
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception e)
            {
                this.Close();
                return false;
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql)
        {
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    IDbComm = DataBaseFactory.GetIDbCommand(sql);
                    //IDbComm.CommandText = sql;
                    //IDbComm.CommandType = CommandType.Text;
                    IDbAdpter = DataBaseFactory.GetIDbDataAdapter(IDbComm);
                    //IDbAdpter.SelectCommand = IDbComm;
                    IDbAdpter.Fill(dataset);
                }
                catch (Exception e)
                {
                    IDbConn.Close();
                    throw new Exception(e.Message);
                }
                finally
                {
                    IDbConn.Close();
                }
                return dataset.Tables[0];
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql)
        {
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                DataSet dataset = new DataSet();
                try
                {
                    IDbComm = DataBaseFactory.GetIDbCommand(sql);
                    //IDbComm.CommandText = sql;
                    //IDbComm.CommandType = CommandType.Text;
                    IDbAdpter = DataBaseFactory.GetIDbDataAdapter(IDbComm);
                    //IDbAdpter.SelectCommand = IDbComm;
                    IDbAdpter.Fill(dataset);
                }
                catch (Exception e)
                {
                    IDbConn.Close();
                    throw new Exception(e.Message);
                }
                finally
                {
                    IDbConn.Close();
                }
                return dataset;
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public IDataReader GetDataReader(string sql)
        {
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
                    // = IDbcmd.ExecuteReader();
                    ireader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                }
                return ireader;
            }           
          catch (Exception e)
            {
                this.Close();
                return null;
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果中上的第一行第一列的值,忽略额外的列和行
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
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
        public int ExecuteNonQuery(string sql)
        {
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                using (IDbComm = DataBaseFactory.GetIDbCommand(sql))
                {
                    try
                    {  
                        int rows = IDbComm.ExecuteNonQuery();
                        IDbConn.Close();
                        return rows;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        return 0;
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
        /// 通过事务执行多条SQL语句
        /// </summary>
        /// <param name="sqlList">SQL语句集</param>
        /// <returns></returns>
        public bool ExecuteByTransaction(List<string> sqlList)
        {
            using (IDbConn = DataBaseFactory.GetIDbConnection())
            {
                IDbConn.Open();
                IDbComm = DataBaseFactory.GetIDbCommand();
                IDbComm.Connection = IDbConn;
                IDbTrans = IDbConn.BeginTransaction();// DataBaseFactory.GetIDbTransaction();                
                if (IDbTrans != null)
                {
                    IDbComm.Transaction = IDbTrans;
                    IDbTrans = IDbConn.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                try
                {
                    for (int i = 0; i < sqlList.Count; i++)
                    {
                        string str = sqlList[i].ToString();
                        if (str.Trim().Length > 1)
                        {
                            IDbComm.CommandText = str;
                            IDbComm.ExecuteNonQuery();
                        }
                    }
                    IDbTrans.Commit();
                    return true;
                }
                catch (Exception exception)
                {

                    IDbTrans.Rollback();
                    IDbConn.Close();
                    return false;
                    throw new Exception(exception.Message);
                }
                finally
                {
                    IDbConn.Close();
                }
            }
        }

        public bool ExecuteByTransaction(Hashtable sqlList)
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
        public bool ExistsRecord(string sql, params IDbDataParameter[] cmdParms)
        {
            IDataReader ireader = null;
            try
            {
                if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
                {
                    IDbConn = DataBaseFactory.GetIDbConnection();
                    IDbConn.Open();
                    IDbComm = DataBaseFactory.GetIDbCommand();
                    PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);                
                    ireader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                    if (ireader.Read())
                    {
                        IDbComm.Parameters.Clear();
                        ireader.Close();
                        this.Close();
                        return true;
                    }
                    else
                    {
                        this.Close();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                this.Close();
                return false;
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, params IDbDataParameter[] cmdParms)
        {         
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
                        return dataSet.Tables[0];
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
        /// 执行SQL语句返回结果集DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, params IDbDataParameter[] cmdParms)
        {
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
                        return dataSet;
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
        /// 执行SQL语句返回结果集IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public IDataReader GetDataReader(string sql, params IDbDataParameter[] cmdParms)
        {
            IDataReader ireader = null;
            try
            {
                if (IDbConn == null || IDbConn.State == ConnectionState.Closed)
                {
                    IDbConn = DataBaseFactory.GetIDbConnection();
                    IDbConn.Open();
                    IDbComm = DataBaseFactory.GetIDbCommand();
                    PrepareCommand(IDbComm, IDbConn, null, CommandType.Text, sql, cmdParms);                  
                    // = IDbcmd.ExecuteReader();
                    ireader = IDbComm.ExecuteReader((CommandBehavior)((int)CommandBehavior.CloseConnection));
                   // IDbComm.Parameters.Clear();
                }
                return ireader;
            }
            catch (Exception e)
            {
                this.Close();
                return null;
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 执行SQL语句返回结果集object
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params IDbDataParameter[] cmdParms)
        {
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
        public int ExecuteNonQuery(string sql, params IDbDataParameter[] cmdParms)
        {
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
                        return rows;
                    }
                    catch (Exception E)
                    {
                        IDbConn.Close();
                        return 0;
                        throw new Exception(E.Message);
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

        #region IDisposable 成员

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Close();
        }

        #endregion

      
    }
}
