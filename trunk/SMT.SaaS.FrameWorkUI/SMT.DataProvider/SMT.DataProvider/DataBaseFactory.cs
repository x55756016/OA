using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;
using System.Data.OleDb;
using System.Data.Odbc;

namespace SMT
{
    class DataBaseFactory
    {
        #region 操作对象 成员
        private static IDbConnection _idbConnection;
        // private IDataReader _iDataReader;
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
       // private static DBProviderType providerType;
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString= DataBaseConfig.GetConnectionString();
        //{
        //    get
        //    {
        //        return DataBaseConfig.GetConnectionString();
        //        //if (SqlParameterCache.GetCachedObject("longkcgetidbconnection")==null)
        //        //{
        //        //    SqlParameterCache.CacheObject("longkcgetidbconnection", DataBaseConfig.GetConnectionString());
        //        //    return DataBaseConfig.GetConnectionString();
        //        //}
        //        //else
        //        //{
        //        //    return SqlParameterCache.GetCachedObject("longkcgetidbconnection").ToString();
        //        //}

        //    }
        //}
        /// <summary>
        /// 数据类型
        /// </summary>
        public static DBProviderType ProviderType
        {
            get
            {       
                    return DataBaseType.GetDBProviderType(); 
            }            
        }
        public static IDbConnection GetIDbConnection()
        {
          
          // IDbConnection IDbConniDbConnection;
            switch (ProviderType)
            {               
                case DBProviderType.SqlServer:
                    IDbConn = new SqlConnection(ConnectionString);
                    break;
                case DBProviderType.OleDb:
                    IDbConn = new OleDbConnection(ConnectionString);
                    break;
                case DBProviderType.Odbc:
                    IDbConn = new OdbcConnection(ConnectionString);
                    break;
                case DBProviderType.Oracle:
                    IDbConn = new OracleConnection(ConnectionString);
                    break;
                default:
                    return null;

            }
            return IDbConn;
        }
        public static IDbConnection GetIDbConnection(string connectionString)
        {

            // IDbConnection IDbConniDbConnection;
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbConn = new SqlConnection(connectionString);
                    break;
                case DBProviderType.OleDb:
                    IDbConn = new OleDbConnection(connectionString);
                    break;
                case DBProviderType.Odbc:
                    IDbConn = new OdbcConnection(connectionString);
                    break;
                case DBProviderType.Oracle:
                    IDbConn = new OracleConnection(connectionString);
                    break;
                default:
                    return null;

            }
            return IDbConn;
        }
        #region IDbCommand
        
        public static IDbCommand GetIDbCommand()
        {
           // providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {           
                 case DBProviderType.SqlServer:
                    IDbComm = new SqlCommand();
                    IDbComm.Connection = IDbConn;
                    return IDbComm;
                case DBProviderType.OleDb:
                    IDbComm = new OleDbCommand();
                    IDbComm.Connection = IDbConn;
                    return IDbComm;
                case DBProviderType.Odbc:
                    IDbComm = new OdbcCommand();
                    IDbComm.Connection = IDbConn;
                    return IDbComm;
                case DBProviderType.Oracle:
                    IDbComm = new OracleCommand();
                    IDbComm.Connection = IDbConn;
                    return IDbComm;
             default:
                    return null;
            }
        }
        public static IDbCommand GetIDbCommand(IDbConnection idbconn)
        {
            // providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbComm = new SqlCommand();
                    IDbComm.Connection = idbconn;
                    return IDbComm;
                case DBProviderType.OleDb:
                    IDbComm = new OleDbCommand();
                    IDbComm.Connection = idbconn;
                    return IDbComm;
                case DBProviderType.Odbc:
                    IDbComm = new OdbcCommand();
                    IDbComm.Connection = idbconn;
                    return IDbComm;
                case DBProviderType.Oracle:
                    IDbComm = new OracleCommand();
                    IDbComm.Connection = idbconn;
                    return IDbComm;
                default:
                    return null;
            }
        }
        public static IDbCommand GetIDbCommand(string sql)
        {
            // providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {               
                case DBProviderType.SqlServer:
                    IDbComm = new SqlCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = CommandType.Text;
                    IDbComm.CommandText = sql;           
                    return IDbComm;
                case DBProviderType.OleDb:
                    IDbComm = new OleDbCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = CommandType.Text;
                    IDbComm.CommandText = sql;
                    return IDbComm;
                case DBProviderType.Odbc:
                    IDbComm = new OdbcCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = CommandType.Text;
                    IDbComm.CommandText = sql;
                    return IDbComm;
                case DBProviderType.Oracle:
                    IDbComm = new OracleCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = CommandType.Text;
                    IDbComm.CommandText = sql;
                    return IDbComm;
                default:
                    return null;
            }
        }
        public static IDbCommand GetIDbCommand(string commandText, CommandType type)
        {
            // providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbComm = new SqlCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = type;
                    IDbComm.CommandText = commandText;
                    return IDbComm;
                case DBProviderType.OleDb:
                    IDbComm = new OleDbCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = type;
                    IDbComm.CommandText = commandText;
                    return IDbComm;
                case DBProviderType.Odbc:
                    IDbComm = new OdbcCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = type;
                    IDbComm.CommandText = commandText;
                    return IDbComm;
                case DBProviderType.Oracle:
                    IDbComm = new OracleCommand();
                    IDbComm.Connection = IDbConn;
                    IDbComm.CommandType = type;
                    IDbComm.CommandText = commandText;
                    return IDbComm;
                default:
                    return null;
            }
        }
        #endregion
        #region IDbDataAdapter
        public static IDbDataAdapter GetIDbDataAdapter()
        {
            //providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbAdpter = new SqlDataAdapter();
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                case DBProviderType.OleDb:
                    IDbAdpter = new OleDbDataAdapter();
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                case DBProviderType.Odbc:
                    IDbAdpter = new OdbcDataAdapter();
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                case DBProviderType.Oracle:
                    IDbAdpter = new OracleDataAdapter();
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
             default:
                    return null;
            }
        }
        public static IDbDataAdapter GetIDbDataAdapter(IDbCommand comm)
        {
            //providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbAdpter = new SqlDataAdapter();
                    IDbAdpter.SelectCommand = comm;
                    return IDbAdpter;
                case DBProviderType.OleDb:
                    IDbAdpter = new OleDbDataAdapter();
                    IDbAdpter.SelectCommand = comm;
                    return IDbAdpter;
                case DBProviderType.Odbc:
                    IDbAdpter = new OdbcDataAdapter();
                    IDbAdpter.SelectCommand = comm;
                    return IDbAdpter;
                case DBProviderType.Oracle:
                    IDbAdpter = new OracleDataAdapter();
                    IDbAdpter.SelectCommand = comm;
                    return IDbAdpter;
                default:
                    return null;
            }
        }
        public static IDbDataAdapter GetIDbDataAdapter(string  commandText,IDbConnection conn)
        {
            //providerType = DataBaseType.GetDBProviderType();
            switch (ProviderType)
            {
                case DBProviderType.SqlServer:
                    IDbAdpter = new SqlDataAdapter(commandText,(SqlConnection)conn);
                    IDbAdpter.SelectCommand = IDbComm;                  
                    return IDbAdpter;
                case DBProviderType.OleDb:
                    IDbAdpter = new OleDbDataAdapter(commandText, (OleDbConnection)conn);
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                case DBProviderType.Odbc:
                    IDbAdpter = new OdbcDataAdapter(commandText, (OdbcConnection)conn);
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                case DBProviderType.Oracle:
                    IDbAdpter = new OracleDataAdapter(commandText, (OracleConnection)conn);
                    IDbAdpter.SelectCommand = IDbComm;
                    return IDbAdpter;
                default:
                    return null;
            }
        }
        #endregion 
        /// <summary>
        /// 得到开一个开始事务
        /// </summary>
        /// <returns></returns>
        public static IDbTransaction GetIDbTransaction()
        {            
            IDbConn = GetIDbConnection();
            IDbTrans = IDbConn.BeginTransaction();
             return IDbTrans;
        }
        /// <summary>
        /// 创建参数的数量
        /// </summary>
        /// <param name="paramsCount">数量</param>
        /// <returns></returns>
        public static IDbDataParameter[] GetIDbParameters(int paramsCount)
        {
            IDbDataParameter[] idbParams = new IDbDataParameter[paramsCount];
            switch (ProviderType)
            {
                #region               
                case DBProviderType.SqlServer:
                    for (int i = 0; i < paramsCount; i++)
                    {
                        idbParams[i] = new SqlParameter();
                    }
                    break;
                case DBProviderType.OleDb:
                    for (int i = 0; i < paramsCount; i++)
                    {
                        idbParams[i] = new OleDbParameter();
                    }
                    break;
                case DBProviderType.Odbc:
                    for (int i = 0; i < paramsCount; i++)
                    {
                        idbParams[i] = new OdbcParameter();
                    }
                    break;
                case DBProviderType.Oracle:
                    for (int i = 0; i < paramsCount; i++)
                    {
                        idbParams[i] = new OracleParameter();
                    }
                    break;
              
                default:
                    idbParams = null;
                    break;
                #endregion
            }
            return idbParams;
        }
        /// <summary>
        /// 根据数据库类型自动转换成标准的SQL语句参数
        /// </summary>
        /// <param name="sqlstring">SQL语句</param>
        /// <returns></returns>
        public static string GetSQL(string sqlstring)
        {           
            if (ProviderType == DBProviderType.SqlServer)
            {
                return sqlstring.Replace(":", "@");
            }
            if (ProviderType == DBProviderType.Oracle)
            {
                return sqlstring.Replace("@", ":");
            }
            else
            {
                return sqlstring;
            }
        }
        /// <summary>
        /// 过滤SQL语句的危险字符,如果有则返回所含有的危险字符，没有则返回null，　分析用户请求是否正常返回是否含有SQL注入式攻击代码
        /// </summary>
        /// <param name="Str">SQL语句 </param>
        /// <returns>返回是否含有SQL注入式攻击代码 </returns>
        public static string FilterSqlString(string Str)
        {
            string ReturnValue = null;
            try
            {
                if (Str.Trim() != "")
                {
                    //string SqlStr = "and ¦exec ¦insert ¦select ¦delete ¦update ¦count ¦* ¦chr ¦mid ¦master ¦truncate ¦char ¦declare";
                    string SqlStr = "exec ¦insert ¦select ¦delete ¦update ¦mid ¦master ¦truncate ¦declare¦<script>¦<script>";
                    string[] anySqlStr = SqlStr.Split('¦');
                    foreach (string ss in anySqlStr)
                    {
                        if (Str.ToLower().IndexOf(ss) >= 0)
                        {
                            ReturnValue+= ss;
                            break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                ReturnValue = e.Message;
            }
            return ReturnValue;
        }

    }
}
