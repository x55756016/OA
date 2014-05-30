using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
namespace SMT
{
    public  interface IDataProvider
    {
        #region 对象
        IDbConnection IDbConn{get;set;}
        IDbCommand IDbComm { get; set; }
        IDbDataAdapter IDbAdpter { get; set; }
        IDbTransaction IDbTrans { get; set; }
        IDbDataParameter[] IDbParams { get; set; }
        #endregion
        string ConnectionString { get; set; }
         void Open();
         void Close();        
        #region 没参数
         bool ExistsRecord(string sql);
         DataTable GetDataTable(string sql);
         DataSet GetDataSet(string sql);
         IDataReader GetDataReader(string sql);
         object ExecuteScalar(string sql);
         int ExecuteNonQuery(string sql);
         bool ExecuteByTransaction(List<string> sqlList);
         bool ExecuteByTransaction(Hashtable sqlList);
        #endregion
        #region 有参数
         bool ExistsRecord(string sql, params IDbDataParameter[] cmdParms);
         DataTable GetDataTable(string sql, params IDbDataParameter[] cmdParms);
         DataSet GetDataSet(string sql, params IDbDataParameter[] cmdParms);
         IDataReader GetDataReader(string sql, params IDbDataParameter[] cmdParms);
         object ExecuteScalar(string sql, params IDbDataParameter[] cmdParms);
         int ExecuteNonQuery(string sql, params IDbDataParameter[] cmdParms);
        #endregion
    }
}
