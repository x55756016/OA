using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Runtime.Caching;
using SMT.Workflow.Common.DataAccess;


namespace SMT.FLOWDAL.ADO
{   
   
    public class ADOHelper
    {
        #region 龙康才新增
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                string conntring = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"].ConnectionString;
                return conntring;
                //if (string.IsNullOrEmpty(CacheProvider.GetCache<string>("ConnectionString")))
                //{
                //    string conntring = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"].ConnectionString;
                //    CacheProvider.Add<string>("ConnectionString", conntring);
                //    return conntring;
                //}
                //else
                //{
                //    return CacheProvider.GetCache<string>("ConnectionString"); 
                //}                
            }
        }
       
        /// <summary>
        /// 创建一个OracleConnection连接对象,并打开
        /// </summary>     
        /// <returns></returns>
        public static OracleConnection GetOracleConnection()
        {
            OracleConnection conn = new OracleConnection();
            try
            {
                System.Configuration.ConnectionStringSettings set = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"];
                if (set != null)
                {
                    conn = new OracleConnection(set.ConnectionString);
                }
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                // LogHelper.WriteLog("数据库连接字符串：" + conn.ConnectionString+" 状态："+conn.State.ToString());
                return conn;
            }
            catch (Exception ee)
            {
                return conn;
                LogHelper.WriteLog("打开数据库出错：数据库连接字符串：" + conn.ConnectionString + " 状态：" + conn.State.ToString() + " \r\n 异常信息：" + ee.ToString()); 
            }
            // if (conn == null)
            // {
            //     System.Configuration.ConnectionStringSettings set = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"];
            //     if (set != null)
            //     {
            //         conn = new OracleConnection(set.ConnectionString);
            //         CacheProvider.Add<string>("ConnectionString", set.ConnectionString);
            //         CacheProvider.Add<OracleConnection>("ContextOracleConnection", conn);
            //     }
            // }
            // if (string.IsNullOrEmpty(conn.ConnectionString))
            // {
            //     conn.ConnectionString = CacheProvider.GetCache<string>("ConnectionString");
            //     if (string.IsNullOrEmpty(conn.ConnectionString))
            //     {
            //         System.Configuration.ConnectionStringSettings set = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"];
            //         if (set != null)
            //         {
            //             conn = new OracleConnection(set.ConnectionString);
            //             CacheProvider.Add<string>("ConnectionString", set.ConnectionString);                         
            //         } 
            //     }
            //     if (conn.State != ConnectionState.Open)
            //     {
            //         conn.Open();
            //     }
            // }
            // else
            // {
            //     if (conn.State != ConnectionState.Open)
            //     {
            //         conn.Open();
            //     }
            // }
            // LogHelper.WriteLog("数据库连接字符串："+conn.ConnectionString);
            //return conn;
        }
        /// <summary>
        /// 事务对象
        /// </summary>
        private static OracleTransaction Transaction { get; set; }
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public static void BeginTransaction(OracleConnection connOjbection)
        {
            if (Transaction.Connection != connOjbection)
            {
                Transaction = connOjbection.BeginTransaction();
            }
        } 
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="conn">OracleConnection连接对象</param>
        public static void CommitTransaction(OracleConnection connOjbection)
        {
            try
            {
                if (Transaction != null)
                {
                    Transaction.Commit();
                    if (connOjbection != null)
                    {
                        if (connOjbection.State == ConnectionState.Open)
                        {
                            connOjbection.Close();
                            connOjbection.Dispose();
                            Transaction = null;
                        }
                    }
                }
            }
            catch (OracleException e)
            {
                Transaction.Rollback();
                connOjbection.Close();
                connOjbection.Dispose();
                Transaction = null;
                throw new System.Exception(e.Message);
            }
            finally
            {
                connOjbection.Close();
                connOjbection.Dispose();
                Transaction = null;
            }
        }
        #endregion
        private static string _ContextOracleConnection;
        public static string ContextOracleConnection
        {
            get
            {
                if (_ContextOracleConnection == null)
                {
                    System.Configuration.ConnectionStringSettings set = System.Configuration.ConfigurationManager.ConnectionStrings["ContextOracleConnection"];
                    if (set != null)
                    {
                        _ContextOracleConnection = set.ConnectionString;
                    }
                    else
                    {
                        _ContextOracleConnection = string.Empty;
                    }
                }
                return _ContextOracleConnection;
            }

        }

        public static void AddParameter(string name, object value, OracleType oracleType, OracleParameterCollection parameters)
        {
            OracleParameter p = new OracleParameter("p" + name, oracleType);
            switch (oracleType)
            {
                case OracleType.NVarChar:
                case OracleType.VarChar:
                case OracleType.Clob:
                case OracleType.NClob:
                    if (value == null)
                    {
                        p.Value = string.Empty;
                    }
                    else
                    {
                        if (value.ToString() == "")
                        {
                            p.Value = string.Empty;
                        }
                        else
                        {
                            p.Value = value;
                        }
                    }
                    
                    break;
                case OracleType.DateTime:
                    p.Value = value == null ? DateTime.Now : value;
                    break;
                default:
                    throw new Exception("AddParameter:oracleType未定义");
                    
            }
            parameters.Add(p);
            
        }
    }

   
}
