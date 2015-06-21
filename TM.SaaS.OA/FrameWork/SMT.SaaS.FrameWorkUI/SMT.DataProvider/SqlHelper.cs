//======================================================================
//
// Copyright (C) 2007-2008 深圳清华天安
// All rights reserved
// 本  机:TIANAN-KEVIN
// 文件名:SqlHelper
// 描  述:
// 创建者:龙康才 
// 时  间:2008-11-10 17:22:59
//
//====================================================================== 
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Reflection;
namespace MSCL
{
    /// <summary>
    /// 数据访问基础类(基于SQLServer)
    /// </summary>
    public abstract class SqlHelper
    {
        /// <summary>
        /// 数据库连接字符串(web.config中的AppSettingsg来配置)
        /// </summary>
        public static readonly string LocalSqlServer = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        #region 分页
        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="fldSort">排序字段</param>
        /// <param name="sort">升序/降序</param>
        /// <param name="condition">条件(不需要where)</param>
        /// <param name="count">记录条数</param>
        /// <returns></returns>
        public static DataSet PageList(string tblName, int pageSize, int pageIndex,
            string fldSort, bool sort, string condition, out int count)
        {
            string sql = GetPagerSQL(condition, pageSize, pageIndex, fldSort, tblName, sort);
            count = GetCount(tblName, condition);
            return GetDataSet(sql);
        }
        /// <summary>
        /// 分页方法
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="fldSort">排序</param>
        /// <param name="tblName">表名</param>
        /// <param name="sort">正序/倒序</param>
        /// <returns></returns>
        public static string GetPagerSQL(string condition, int pageSize, int pageIndex, string fldSort, string tblName, bool sort)
        {
            string strSort = sort ? " ASC" : " DESC";

            if (pageIndex == 1)
            {
                return "select top " + pageSize.ToString() + " * from " + tblName.ToString() + " order by " + fldSort.ToString() + strSort;
            }
            else
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("select top {0} * from {1} ", pageSize, tblName);
                strSql.AppendFormat(" where  {1} not in (select top {0} {1} from {2} ", pageSize * (pageIndex - 1), fldSort, tblName);
                if (!string.IsNullOrEmpty(condition))
                {
                    strSql.AppendFormat(" where {0} order by {1}{2}) and  {0}", condition, fldSort, strSort);
                }
                else
                {
                    strSql.AppendFormat(" order by {0}{1}) ", fldSort, strSort);
                }
                strSql.AppendFormat(" order by {0}{1}", fldSort, strSort);
                return strSql.ToString();
            }
        }
        /// <summary>
        /// 获取表数据量
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="condition">条件</param>
        /// <returns></returns>
        public static int GetCount(string tblName, string condition)
        {
            StringBuilder sql = new StringBuilder(string.Format("select count(*) from {0}", tblName));
            if (!string.IsNullOrEmpty(condition))
            {
                sql.Append(string.Format(" where {0}", condition));
            }
            DataSet ds = GetDataSet(sql.ToString());
            if (ds.Tables[0].Rows[0][0] != null)
            {
                return int.Parse(ds.Tables[0].Rows[0][0].ToString());
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region 参数
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                {
                    if (parm.SqlDbType == SqlDbType.DateTime)
                    {
                        string a = parm.Value.ToString();
                        //  string b = DateTime.MinValue.ToString();
                        // if ((DateTime)parm.Value == DateTime.MinValue)
                        if (a == "1900-1-1 0:00:00")
                        {
                            parm.Value = System.DBNull.Value;
                        }
                    }
                    cmd.Parameters.Add(parm);
                }
            }
        }
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = new SqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter.SqlDbType == SqlDbType.DateTime)
                    {
                        string a = parameter.Value.ToString();
                        // if ((DateTime)parameter.Value == DateTime.MinValue)
                        if (a == "1900-1-1 0:00:00")
                        {
                            parameter.Value = System.DBNull.Value;
                        }
                    }
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }

        /// <summary>
        /// 创建SqlCommand 对象实例(用来返回一个整数值)	
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand 对象实例</returns>
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }
        #endregion

        #region 构造语句常用类
        /// <summary>
        /// 输入参数
        /// </summary>
        /// <param name="ParamName">参数名称</param>
        /// <param name="DbType">参数类型</param>
        /// <param name="Size">参数大小</param>
        /// <param name="Value">参数值.</param>
        /// <returns>New parameter.</returns>
        public static SqlParameter MakeInParam(string ParamName, SqlDbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }
        /// <summary>
        /// 输入参数
        /// </summary>
        /// <param name="ParamName">参数名称</param>
        /// <param name="DbType">参数类型</param>
        /// <param name="Value">参数值</param>
        /// <returns></returns>
        public static SqlParameter MakeInParam(string ParamName, SqlDbType DbType, object Value)
        {
            return MakeParam(ParamName, DbType, 0, ParameterDirection.Input, Value);
        }

        /// <summary>
        /// 输出参数
        /// </summary>
        /// <param name="ParamName">参数名称.</param>
        /// <param name="DbType">参数类型</param>
        /// <param name="Size">参数大小</param>
        /// <returns>New parameter.</returns>
        public static SqlParameter MakeOutParam(string ParamName, SqlDbType DbType, int Size)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        /// <summary>
        /// 存储过程输入参数
        /// </summary>
        /// <param name="ParamName">参数名称.</param>
        /// <param name="DbType">参数类型</param>
        /// <param name="Size">参数大小</param>
        /// <param name="Direction">参数方向</param>
        /// <param name="Value">参数值</param>
        /// <returns>New parameter.</returns>
        public static SqlParameter MakeParam(string ParamName, SqlDbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            SqlParameter param;

            if (Size > 0)
                param = new SqlParameter(ParamName, DbType, Size);
            else
                param = new SqlParameter(ParamName, DbType);

            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
                param.Value = Value;

            return param;
        }
        #endregion 构造语句常用类

        #region 单个数据库操作
        ///<summary>
        /// 通用分页存储过程
        /// </summary>       
        /// <param name="tblName">要显示的表或多个表的连接</param>
        /// <param name="fldName">要显示的字段列表,可为Null,表示*</param>
        /// <param name="pageSize">每页显示的记录个数</param>
        /// <param name="pageIndex">要显示那一页的记录</param>
        /// <param name="fldSort">排序字段列表或条件</param>
        /// <param name="Sort">排序方法，False为升序，True为降序(如果是多字段排列Sort指代最后一个排序字段的排列顺序(最后一个排序字段不加排序标记)--程序传参如：' SortA Asc,SortB Desc,SortC ')</param>
        /// <param name="strCondition">查询条件,不需where,以And开始,可为Null,表示""</param>
        /// <param name="ID">主表的主键</param>
        /// <param name="Dist">是否添加查询字段的DISTINCT 默认False不添加/True添加</param>
        /// <param name="pageCount">查询结果分页后的总页数</param>
        /// <param name="Counts">查询到的记录数</param>
        /// <param name="strSql">最后返回的SQL语句</param>
        /// <returns></returns>

        public static DataSet PageList(string tblName, string fldName, int pageSize, int pageIndex, string fldSort, bool Sort, string strCondition, string ID, bool Dist, out int pageCount, out int Counts, out string strSql)
        {
            SqlParameter[] parameters ={ new SqlParameter("@tblName",SqlDbType.NVarChar,200),
                new SqlParameter("@fldName",SqlDbType.NVarChar,500),
                new SqlParameter("@pageSize",SqlDbType.Int),
                new SqlParameter("@page",SqlDbType.Int),
                new SqlParameter("@fldSort",SqlDbType.NVarChar,200),
                new SqlParameter("@Sort",SqlDbType.Bit),
                new SqlParameter("@strCondition",SqlDbType.NVarChar,1000),
                new SqlParameter("@ID",SqlDbType.NVarChar,150),
                new SqlParameter("@Dist",SqlDbType.Bit),
                new SqlParameter("@pageCount",SqlDbType.Int),
                new SqlParameter("@Counts",SqlDbType.Int),
                new SqlParameter("@strSql",SqlDbType.NVarChar,1000)};

            parameters[0].Value = tblName;
            parameters[1].Value = (fldName == null) ? "*" : fldName;
            parameters[2].Value = (pageSize == 0) ? int.Parse(ConfigurationManager.AppSettings["PageSize"]) : pageSize;
            parameters[3].Value = pageIndex;
            parameters[4].Value = fldSort;
            parameters[5].Value = Sort;
            parameters[6].Value = strCondition == null ? "" : strCondition;
            parameters[7].Value = ID;
            parameters[8].Value = Dist;
            parameters[9].Direction = ParameterDirection.Output;
            parameters[10].Direction = ParameterDirection.Output;
            parameters[11].Direction = ParameterDirection.Output;

            DataSet ds = RunProcedure("PageList", parameters, "ds");

            pageCount = (int)parameters[9].Value;
            Counts = (int)parameters[10].Value;
            strSql = parameters[11].Value.ToString();
            return ds;
        }
        #region 执行简单SQL语句

        /// <summary>
        ///  检测一个记录是否存在(SqlParameter语句方式)
        /// </summary>     
        /// <param name="strSql">SQL语句</param>
        /// <param name="cmdParms">SqlParameter[]参数</param>
        /// <returns></returns>
        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            DataSet ds = GetDataSet(strSql, cmdParms);
            return int.Parse(ds.Tables[0].Rows[0][0].ToString()) > 0;
        }
        /// <summary>
        /// 检测一个记录是否存在(sql语句方式)
        /// </summary>      
        /// <param name="strSql">SQL语句</param>
        /// <returns></returns>
        public static bool ExistsRecord(string strSql)
        {
            bool bol = true;
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand(strSql, connection))
                {
                    try
                    {
                        connection.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            dr.Close();
                            return bol;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        bol = false;
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
            return bol;

        }
        /// <summary>
        /// 检测一个记录是否存在(Sql语句方式)
        /// </summary>
        /// <param name="SQLString">Sql语句方式</param>
        /// <returns></returns>
        public static bool IsHasRecord(string SQLString)
        {
            SqlDataReader dr = ExecuteReader(SQLString);
            if (dr.Read())
            {
                dr.Close();
                return true;
            }
            else
            {
                dr.Close();
                return false;
            }
        }
        /// <summary>
        /// 检测一个记录是否存在(Sql语句方式),SqlParameter[]参数
        /// </summary>
        /// <param name="SQLString">Sql语句方式</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns></returns>
        public static bool IsHasRecordByParams(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlDataReader dr = ExecuteReader(SQLString, cmdParms);
            if (dr.Read())
            {
                dr.Close();
                return true;
            }
            else
            {
                dr.Close();
                return false;
            }
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>   
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExeSQL(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。第一行第一列的值。
        /// </summary>      
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果( string )第一行第一列的值。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns></returns>
        public static string GetOneValue(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return "";
                        }
                        else
                        {
                            return obj.ToString();
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader
        /// </summary>   
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetSqlDataReader(string strSQL)
        {
            SqlConnection connection = new SqlConnection(LocalSqlServer);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string SQLString)
        {
            if (SQLString != null && SQLString.Trim() != "")
            {
                using (SqlConnection connection = new SqlConnection(LocalSqlServer))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        connection.Open();
                        SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                        command.Fill(ds, "ds");
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        ///  执行查询语句，返回DataTable
        /// </summary>     
        /// <param name="SQLString">SQL语句</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string SQLString)
        {
            if ((SQLString != null) && (SQLString.Trim() != ""))
            {
                using (SqlConnection connection = new SqlConnection(LocalSqlServer))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        connection.Open();
                        new SqlDataAdapter(SQLString, connection).Fill(ds, "ds");
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 把一行记录载入哈唏表里key(列名):字符串,   value(值):对像（object）
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static Hashtable GetOneRowHashtable(string sql)
        {
            SqlDataReader dr = SqlHelper.GetSqlDataReader(sql);
            Hashtable has = new Hashtable();
            if (dr.Read())
            {
                for (int k = 0; k < dr.FieldCount; k++)
                {
                    string columName = dr.GetName(k);
                    object columValue = dr[columName];
                    has.Add(columName, columValue);
                }
            }
            dr.Close();
            dr.Dispose();
            return has;
        }

        #endregion 执行简单SQL语句

        #region 执行带参数的SQL语句
        #region
        /// <summary>
        /// 返回表中的记录总数,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static int GetRecordCount(string tableName, params SqlParameter[] cmdParms)
        {
            int recordCount = 0;
            string sql = "select count(*) as co from " + tableName + "";
            SqlDataReader dr = GetSqlDataReader(sql, cmdParms);
            if (dr.Read())
            {
                recordCount = Convert.ToInt32(dr["co"]);
            }
            dr.Close();
            return recordCount;
        }
        /// <summary>
        /// 返回表中的记录总数,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="condition">条件（如："person_id='DF0000011'")</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static int GetRecordCount(string tableName, string condition, params SqlParameter[] cmdParms)
        {
            int recordCount = 0;
            string sql = "select count(*) as co from " + tableName + " where " + condition + "";
            SqlDataReader dr = GetSqlDataReader(sql, cmdParms);
            if (dr.Read())
            {
                recordCount = Convert.ToInt32(dr["co"]);
            }
            dr.Close();
            return recordCount;
        }
        /// <summary>
        /// 返回表中某个字段的最大值,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="strField">字段名(列名)</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static string GetFieldMaxValue(string tableName, string strField, params SqlParameter[] cmdParms)
        {
            string str = "";
            string sql = "select max(" + strField + ") as maxString from " + tableName + "";
            SqlDataReader dr = GetSqlDataReader(sql, cmdParms);

            if (dr.Read())
            {
                str = dr["maxString"].ToString();
            }
            dr.Close();
            return str;
        }
        /// <summary>
        /// 返回表中某一列的值,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名(列名)</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetOneColumn(string tableName, string fieldName, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  " + fieldName + " from " + tableName;
            DataTable dt = GetDataTable(sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某一列的值,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名(列名)</param>
        /// <param name="condition">条件（不用加where）</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetOneColumn(string tableName, string fieldName, string condition, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  " + fieldName + " from " + tableName + " where " + condition;
            DataTable dt = GetDataTable(sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string tableName, string fieldName, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  distinct " + fieldName + " from " + tableName;
            DataTable dt = GetDataTable(sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值,SqlParameter[] 参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="strWhere">条件（不用加where）</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string tableName, string fieldName, string strWhere, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  distinct " + fieldName + " from " + tableName + " where " + strWhere;
            DataTable dt = GetDataTable(sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        #endregion
        /// <summary>
        /// 执行SQL语句，返回影响的记录数,带参数SqlParameter[]
        /// </summary> 
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>影响的记录数</returns>
        public static int ExecSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>    
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static void ExecSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(LocalSqlServer))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。首行首列值,带参数SqlParameter[]
        /// </summary>  
        /// <param name="SQLString">计算查询结果语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果:第一行第一列的值,带参数SqlParameter[]
        /// </summary>  
        /// <param name="SQLString">计算查询结果语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>查询结果（string）</returns>
        public static string GetOneValue(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return "";
                        }
                        else
                        {
                            return obj.ToString();
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader,带参数SqlParameter[]
        /// </summary> 
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(LocalSqlServer);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader,带参数SqlParameter[]
        /// </summary> 
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetSqlDataReader(string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(LocalSqlServer);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        ///  执行查询语句，返回DataTable,带参数SqlParameter[]
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数SqlParameter[]</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回DataSet,带参数SqlParameter[]
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回SqlDataReader,带参数SqlParameter[]
        /// </summary> 
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static Hashtable GetOneRowHashtable(string SQLString, params SqlParameter[] cmdParms)
        {
            Hashtable has = new Hashtable();
            SqlConnection connection = new SqlConnection(LocalSqlServer);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    for (int k = 0; k < dr.FieldCount; k++)
                    {
                        string columName = dr.GetName(k);
                        object columValue = dr[columName];
                        has.Add(columName, columValue);
                    }
                }
                dr.Close();
                dr.Dispose();
                cmd.Parameters.Clear();
                return has;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }
        #endregion 执行带参数的SQL语句

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(LocalSqlServer);
            SqlDataReader returnReader;
            connection.Open();
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader();
            return returnReader;
        }

        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>  
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }
        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>  
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>  
        /// <returns>DataSet</returns>
        public static DataTable GetDataTableByRunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet);
                connection.Close();
                return dataSet.Tables[0];
            }
        }
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, null);
                result = command.ExecuteNonQuery();
                connection.Close();
                return result;
            }
        }
        /// <summary>
        /// 执行存储过程，返回Return值		
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(LocalSqlServer))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                connection.Close();
                return result;
            }
        }
        #endregion 存储过程操作

        #endregion

        #region 多个数据库连接操作
        #region 执行简单SQL语句

        /// <summary>
        ///  检测一个记录是否存在(SqlParameter语句方式)
        /// </summary>     
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="strSql">SQL语句</param>
        /// <param name="cmdParms">SqlParameter[]参数</param>
        /// <returns></returns>
        public static bool Exists(string connectionString, string strSql, params SqlParameter[] cmdParms)
        {
            DataSet ds = GetDataSet(strSql, cmdParms);
            return int.Parse(ds.Tables[0].Rows[0][0].ToString()) > 0;
        }
        /// <summary>
        /// 检测一个记录是否存在(sql语句方式)
        /// </summary>      
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="strSql">SQL语句</param>
        /// <returns></returns>
        public static bool ExistsRecord(string connectionString, string strSql)
        {
            bool bol = true;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(strSql, connection))
                {
                    try
                    {
                        connection.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            dr.Close();
                            return bol;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        bol = false;
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
            return bol;

        }
        /// <summary>
        /// 检测一个记录是否存在(Sql语句方式)
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">Sql语句方式</param>
        /// <returns></returns>
        public static bool IsHasRecord(string connectionString, string SQLString)
        {
            SqlDataReader dr = ExecuteReader(connectionString, SQLString);
            if (dr.Read())
            {
                dr.Close();
                return true;
            }
            else
            {
                dr.Close();
                return false;
            }
        }
        /// <summary>
        /// 检测一个记录是否存在(Sql语句方式),SqlParameter[]参数
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">Sql语句方式</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns></returns>
        public static bool IsHasRecordByParams(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            SqlDataReader dr = ExecuteReader(connectionString, SQLString, cmdParms);
            if (dr.Read())
            {
                dr.Close();
                return true;
            }
            else
            {
                dr.Close();
                return false;
            }
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>   
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExeSQL(string connectionString, string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。第一行第一列的值。
        /// </summary>      
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string connectionString, string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果( string )第一行第一列的值。
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns></returns>
        public static string GetOneValue(string connectionString, string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return "";
                        }
                        else
                        {
                            return obj.ToString();
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 返回表中某一列的值
        /// </summary>
        /// <param name="sql">SQL语句:SELECT  fieldName  from tableName</param>       
        /// <returns></returns>
        public static ArrayList GetOneColumn(string sql)
        {
            ArrayList arrl = new ArrayList();
            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][0].ToString());
                }
            }
            return arrl;
        } 
        /// <summary>
        /// 返回表中某一列的值
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名(列名)</param>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrderby">排序</param>    
        /// <returns></returns>
        public static ArrayList GetOneColumn(string tableName, string fieldName, string strWhere, string strOrderby)
        {
            ArrayList arrl = new ArrayList();
           // string sql = " SELECT  " + fieldName + " from " + tableName + " where " + condition;

            StringBuilder sql = new StringBuilder("SELECT  " + fieldName + " from " + tableName);
            if (!string.IsNullOrEmpty(strWhere))
                sql.Append(" where " + strWhere);
            if (!string.IsNullOrEmpty(strOrderby))
                sql.Append(" order by " + strOrderby);

            DataTable dt = GetDataTable(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值
        /// </summary>
        /// <param name="sql">sql语句:SELECT  distinct fieldName from  tableName</param>            
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string sql)
        {
            ArrayList arrl = new ArrayList();           
            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][0].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">显示字段</param>
        /// <param name="strWhere">条件</param>
        /// <param name="strOrderby">排序</param>       
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string tableName, string fieldName, string strWhere,string strOrderby)
        {
            ArrayList arrl = new ArrayList();
            //string sql = " SELECT  distinct " + fieldName + " from " + tableName + " where " + strWhere;

            StringBuilder sql = new StringBuilder("SELECT  distinct " + fieldName + " from " + tableName);
            if (!string.IsNullOrEmpty(strWhere))
                sql.Append(" where " + strWhere);
            if (!string.IsNullOrEmpty(strOrderby))
                sql.Append(" order by " + strOrderby);

            DataTable dt = GetDataTable(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader
        /// </summary>   
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="strSQL">查询语句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetSqlDataReader(string connectionString, string strSQL)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                SqlDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string connectionString, string SQLString)
        {
            if (SQLString != null && SQLString.Trim() != "")
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        connection.Open();
                        SqlDataAdapter command = new SqlDataAdapter(SQLString, connection);
                        command.Fill(ds, "ds");
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        ///  执行查询语句，返回DataTable
        /// </summary>     
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">SQL语句</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string connectionString, string SQLString)
        {
            if ((SQLString != null) && (SQLString.Trim() != ""))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        connection.Open();
                        new SqlDataAdapter(SQLString, connection).Fill(ds, "ds");
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
            return null;
        }
        /// <summary>
        /// 把一行记录载入哈唏表里key(列名):字符串,   value(值):对像（object）
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static Hashtable GetOneRowHashtable(string connectionString, string sql)
        {

            Hashtable has = new Hashtable();

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    for (int k = 0; k < dr.FieldCount; k++)
                    {
                        string columName = dr.GetName(k);
                        object columValue = dr[columName];
                        has.Add(columName, columValue);
                    }
                }
                dr.Close();
                dr.Dispose();
                return has;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion 执行简单SQL语句

        #region 执行带参数的SQL语句
        #region

        /// <summary>
        /// 返回表中的记录总数,SqlParameter[] 参数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="tableName">表名</param>
        /// <param name="condition">条件（如："person_id='DF0000011'")</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static int GetRecordCount(string connectionString, string tableName, string condition, params SqlParameter[] cmdParms)
        {
            int recordCount = 0;
            string sql = "select count(*) as co from " + tableName + " where " + condition + "";
            SqlDataReader dr = GetSqlDataReader(connectionString, sql, cmdParms);
            if (dr.Read())
            {
                recordCount = Convert.ToInt32(dr["co"]);
            }
            dr.Close();
            return recordCount;
        }
        /// <summary>
        /// 返回表中某个字段的最大值,SqlParameter[] 参数
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="tableName">表名</param>
        /// <param name="strField">字段名(列名)</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static string GetFieldMaxValue(string connectionString, string tableName, string strField, params SqlParameter[] cmdParms)
        {
            string str = "";
            string sql = "select max(" + strField + ") as maxString from " + tableName + "";
            SqlDataReader dr = GetSqlDataReader(connectionString, sql, cmdParms);

            if (dr.Read())
            {
                str = dr["maxString"].ToString();
            }
            dr.Close();
            return str;
        }

        /// <summary>
        /// 返回表中某一列的值,SqlParameter[] 参数
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">SELECT   fieldName from tableName</param>      
        /// <returns></returns>
        public static ArrayList GetOneColumn(string connectionString, string sql)
        {
            ArrayList arrl = new ArrayList();
           // string sql = " SELECT  " + fieldName + " from " + tableName + " where " + condition;
            DataTable dt = GetDataTable(connectionString, sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][0].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某一列的值,SqlParameter[] 参数
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名(列名)</param>
        /// <param name="condition">条件（不用加where）</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetOneColumn(string connectionString, string tableName, string fieldName, string condition, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  " + fieldName + " from " + tableName + " where " + condition;
            DataTable dt = GetDataTable(connectionString, sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">SELECT  distinct fieldName  from  tableName</param>       
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string connectionString, string sql)      
        {
            ArrayList arrl = new ArrayList();
           // string sql = " SELECT  distinct " + fieldName + " from " + tableName + " where " + strWhere;
            DataTable dt = GetDataTable(connectionString, sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][0].ToString());
                }
            }
            return arrl;
        }
        /// <summary>
        /// 返回表中某字段不重复值,SqlParameter[] 参数
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="tableName">表名</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="strWhere">条件（不用加where）</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns></returns>
        public static ArrayList GetDistinctValue(string connectionString, string tableName, string fieldName, string strWhere, params SqlParameter[] cmdParms)
        {
            ArrayList arrl = new ArrayList();
            string sql = " SELECT  distinct " + fieldName + " from " + tableName + " where " + strWhere;
            DataTable dt = GetDataTable(connectionString, sql, cmdParms);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    arrl.Add(dt.Rows[i][fieldName].ToString());
                }
            }
            return arrl;
        }
        #endregion
        /// <summary>
        /// 执行SQL语句，返回影响的记录数,带参数SqlParameter[]
        /// </summary> 
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>影响的记录数</returns>
        public static int ExecSql(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException E)
                    {
                        throw new Exception(E.Message);
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>    
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static void ExecSqlTran(string connectionString, Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlCommand cmd = new SqlCommand();
                    try
                    {
                        //循环
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。首行首列值,带参数SqlParameter[]
        /// </summary>  
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果:第一行第一列的值,带参数SqlParameter[]
        /// </summary>  
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <param name="cmdParms">SqlParameter[] 参数</param>
        /// <returns>查询结果（string）</returns>
        public static string GetOneValue(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return "";
                        }
                        else
                        {
                            return obj.ToString();
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader,带参数SqlParameter[]
        /// </summary> 
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }
        /// <summary>
        /// 执行查询语句，返回SqlDataReader,带参数SqlParameter[]
        /// </summary> 
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetSqlDataReader(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        ///  执行查询语句，返回DataTable,带参数SqlParameter[]
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="cmdParms">参数SqlParameter[]</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回DataSet,带参数SqlParameter[]
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds;
                }
            }
        }

        /// <summary>
        /// 把一行记录载入哈唏表里key(列名):字符串,   value(值):对像（object）
        /// </summary> 
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="SQLString">查询语句</param>
        /// <param name="cmdParms">SqlParameter[]</param>
        /// <returns>SqlDataReader</returns>
        public static Hashtable GetOneRowHashtable(string connectionString, string SQLString, params SqlParameter[] cmdParms)
        {
            Hashtable has = new Hashtable();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    for (int k = 0; k < dr.FieldCount; k++)
                    {
                        string columName = dr.GetName(k);
                        object columValue = dr[columName];
                        has.Add(columName, columValue);
                    }
                }
                dr.Close();
                dr.Dispose();              
                cmd.Parameters.Clear();
                return has;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw new Exception(e.Message);
            }

        }
        #endregion 执行带参数的SQL语句

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader RunProcedure(string connectionString, string storedProcName, IDataParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataReader returnReader;
            connection.Open();
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader();
            return returnReader;
        }

        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>  
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string connectionString, string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }
        /// <summary>
        /// 执行存储过程,带参数IDataParameter[]
        /// </summary>  
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>  
        /// <returns>DataSet</returns>
        public static DataTable GetDataTableByRunProcedure(string connectionString, string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet);
                connection.Close();
                return dataSet.Tables[0];
            }
        }
        /// <summary>
        /// 执行存储过程
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <returns></returns>
        public static int RunProcedure(string connectionString, string storedProcName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, null);
                result = command.ExecuteNonQuery();
                connection.Close();
                return result;
            }
        }
        /// <summary>
        /// 执行存储过程，返回Return值		
        /// </summary>
        ///  <param name="connectionString">数据库连接字符串</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="rowsAffected">影响的行数</param>
        /// <returns></returns>
        public static int RunProcedure(string connectionString, string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                int result;
                connection.Open();
                SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                connection.Close();
                return result;
            }
        }
        #endregion 存储过程操作


        #endregion
    }
}

