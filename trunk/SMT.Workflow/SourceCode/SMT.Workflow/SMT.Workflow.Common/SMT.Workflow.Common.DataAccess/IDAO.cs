/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 自定义数据访问接口
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SMT.Workflow.Common.DataAccess
{
    /// <summary>
    /// 数据访问接口
    /// </summary>
    public interface IDAO
    {
        /// <summary>
        /// 打开连接
        /// </summary>
        void Open();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

        #region transacton

        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 开始指定级别的事务
        /// </summary>
        /// <param name="level">事务级别</param>
        void BeginTransaction(IsolationLevel level);

        /// <summary>
        /// 递交事务
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void RollbackTransaction();

        #endregion

        #region Database Instance Content

        /// <summary>
        /// 参数前缀符
        /// </summary>
        string DatabaseParameterPrefix
        {
            get;
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        string DatabaseType
        {
            get;
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        string DatabaseString
        {
            get;
            set;
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>SQL语句所影响的记录数</returns>
        int ExecuteNonQuery(string sql);

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <returns>SQL语句所影响的记录数</returns>
        int ExecuteNonQuery(string sql, CommandType type);

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="parameters">相关参数</param>
        /// <returns>SQL语句所影响的记录数</returns>
        int ExecuteNonQuery(string sql, CommandType type, ParameterCollection parameters);

        int ExecuteNonQuery(string sql, CommandType type, object[] parameters);

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>第一行第一列的值</returns>
        object ExecuteScalar(string sql);

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <returns>第一行第一列的值</returns>
        object ExecuteScalar(string sql, CommandType type);

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="parameters">相关参数</param>
        /// <returns>第一行第一列的值</returns>
        object ExecuteScalar(string sql, CommandType type, ParameterCollection parameters);

        #endregion

        #region Seqence or Identity

        /// <summary>
        /// 获取序列或自动增长值的下一个值，
        /// Oracle数据库：返回select sequence.nextval from dual，
        /// Sql Server数据库：返回select @@identity值，
        /// Access数据库：返回select @@identity值，
        /// </summary>
        /// <param name="sequence">获取值标识</param>
        /// <returns>增长值</returns>
        int GetNextValue(string sequence);

        /// <summary>
        /// 获取序列或自动增长值的当前值，
        /// Oracle数据库：返回0，
        /// Sql Server数据库：返回select @@identity值，
        /// Access数据库：返回select @@identity值，
        /// </summary>
        /// <returns>当前值</returns>
        int GetCurrentValue();

        #endregion

        #region GetDataTable 
       
        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>数据集</returns>
        DataSet GetDataSet(string sql);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <returns>数据集</returns>
        DataSet GetDataSet(string sql, CommandType type);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="parameters">条件参数</param>
        /// <returns>数据集</returns>
        DataSet GetDataSet(string sql, CommandType type, ParameterCollection parameters);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql, CommandType type);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="parameters">条件参数</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql, CommandType type, ParameterCollection parameters);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql, int pageIndex, int pageSize);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql, CommandType type, int pageIndex, int pageSize);

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <param name="parameters">条件参数</param>
        /// <returns>数据集</returns>
        DataTable GetDataTable(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters);

        #endregion
    }
}
