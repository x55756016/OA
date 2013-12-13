/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： Oracle自定义数据访问接口
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.OracleClient;
using SMT.Foundation.Core.Error;
using SMT.Foundation.Core.Utilities;
using System.Reflection;
using System.Data.Objects.DataClasses;
//using WSL.Framework.Core.Error;
//using WSL.Framework.Core.Utilities;

namespace SMT.Foundation.Core
{
    /// <summary>
    /// 封装OracleServer的访问方法
    /// </summary>
    public sealed class OracleDAO : IDAO, IDisposable
    {
        #region Variable
        // 数据库存连接字符串
        private string _connection;
        // 数据库连接对象
        private OracleConnection _oleConn = null;
        // 事务对象
        private OracleTransaction _oleTrans = null;
        // IDisposable参数
        private bool disposed = false;

        // 事务控制计数器
        private int _transCount = 0;

        #endregion

        #region Constructor

        /// <summary>
        ///  构造器
        /// </summary>
        /// <param name="connection">连接字符串</param>
        public OracleDAO(string connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~OracleDAO()
        {
            Dispose(false);
        }

        #endregion

        #region OpenConnection

        // 返回打开的数据连接对象
        private OracleConnection OpenConnection()
        {
            OracleConnection oleConn = new OracleConnection(_connection);

            try
            {
                oleConn.Open();
            }
            catch (Exception ex)
            {
                oleConn = null;
                throw new TechException(ex.Message, _connection,ex);
            }

            return oleConn;
        }

        #endregion

        #region Database Instance Content

        /// <summary>
        /// 参数前缀符
        /// </summary>
        public string DatabaseParameterPrefix
        {
            get { return "@"; }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType
        {
            get { return "sqlServer"; }
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DatabaseString
        {
            get { return _connection; }
            set
            {
                this._connection = value;
            }
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>SQL语句所影响的记录数</returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, CommandType.Text, new ParameterCollection());
        }

        /// <summary>
        /// 执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <returns>SQL语句所影响的记录数</returns>
        public int ExecuteNonQuery(string sql, CommandType type)
        {
            return ExecuteNonQuery(sql, type, new ParameterCollection());
        }

        /// <summary>
        /// 执行数据库操作(Insert,Update,Delete)
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="parameters">相关参数</param>
        /// <returns>SQL语句所影响的记录数</returns>
        public int ExecuteNonQuery(string sql, CommandType type, ParameterCollection parameters)
        {
            int ret = 0;
            OracleCommand oleCmd = new OracleCommand();

            try
            {
                // 判断是否有事务
                if (_transCount == 0)
                {
                    _oleConn = OpenConnection();
                    oleCmd.Connection = _oleConn;
                }
                else
                {
                    oleCmd.Connection = _oleConn;
                    oleCmd.Transaction = _oleTrans;
                }
                oleCmd.CommandText = sql;
                oleCmd.CommandType = type;
                // 设置参数
                for (int i = 0; i < parameters.Count; i++)
                {
                    OracleParameter param = new OracleParameter();
                    param.ParameterName = parameters[i].ParameterName;
                    param.Value = parameters[i].ParameterValue;
                    oleCmd.Parameters.Add(param);
                }
                ret = oleCmd.ExecuteNonQuery();
                // 获取参数返回值
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].ParameterValue = oleCmd.Parameters[parameters[i].ParameterName].Value;
                }
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, sql, ex);
            }
            finally
            {
                oleCmd.Dispose();
                oleCmd = null;
                if (_transCount == 0 && _oleConn != null)
                {
                    _oleConn.Dispose();
                    _oleConn = null;
                }
            }
            return ret;
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, CommandType.Text, new ParameterCollection());
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sql, CommandType type)
        {
            return ExecuteScalar(sql, type, new ParameterCollection());
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略额外的列或行。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句的类型</param>
        /// <param name="parameters">相关参数</param>
        /// <returns>第一行第一列的值</returns>
        public object ExecuteScalar(string sql, CommandType type, ParameterCollection parameters)
        {
            object ret = null;
            OracleCommand oleCmd = new OracleCommand();

            try
            {
                // 判断是否有事务
                if (_transCount == 0)
                {
                    _oleConn = OpenConnection();
                    oleCmd.Connection = _oleConn;
                }
                else
                {
                    oleCmd.Connection = _oleConn;
                    oleCmd.Transaction = _oleTrans;
                }
                oleCmd.CommandText = sql;
                oleCmd.CommandType = type;
                // 设置参数
                for (int i = 0; i < parameters.Count; i++)
                {
                    OracleParameter param = new OracleParameter();
                    param.ParameterName = parameters[i].ParameterName;
                    param.Value = parameters[i].ParameterValue;
                    oleCmd.Parameters.Add(param);
                }
                ret = oleCmd.ExecuteScalar();
                // 获取参数返回值
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].ParameterValue = oleCmd.Parameters[parameters[i].ParameterName].Value;
                }
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, sql, ex);
            }
            finally
            {
                oleCmd.Dispose();
                oleCmd = null;
                if (_transCount == 0 && _oleConn != null)
                {
                    _oleConn.Dispose();
                    _oleConn = null;
                }
            }
            return ret;
        }

        #endregion

        #region Identity

        /// <summary>
        /// 获取序列或自动增长值的下一个值
        /// </summary>
        /// <param name="sequence">获取值标识，Oracle中为序列名称</param>
        /// <returns>增长值</returns>
        public int GetNextValue(string sequence)
        {
            return TypeConverter.ToInt(ExecuteScalar("SELECT " + sequence + ".NEXTVAL FROM dual"));
        }

        /// <summary>
        /// 获取序列或自动增长值的当前值
        /// </summary>
        /// <returns>当前值</returns>
        public int GetCurrentValue()
        {
            return 0;
        }

        #endregion

        #region GetDataTable

        #region 不分页的获取

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql)
        {
            return GetDataTable(sql, CommandType.Text, new ParameterCollection());
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql, CommandType type)
        {
            return GetDataTable(sql, type, new ParameterCollection());
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="parameters">条件参数</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql, CommandType type, ParameterCollection parameters)
        {
            DataTable ret = new DataTable();
            OracleDataAdapter da = new OracleDataAdapter();
            OracleCommand oleCmd = new OracleCommand();

            try
            {
                // 判断是否有事务
                if (_transCount == 0)
                {
                    _oleConn = OpenConnection();
                    oleCmd.Connection = _oleConn;
                }
                else
                {
                    oleCmd.Connection = _oleConn;
                    oleCmd.Transaction = _oleTrans;
                }
                oleCmd.CommandText = sql;
                oleCmd.CommandType = type;
                // 设置参数
                for (int i = 0; i < parameters.Count; i++)
                {
                    OracleParameter param = new OracleParameter();
                    param.ParameterName = parameters[i].ParameterName;
                    param.Value = parameters[i].ParameterValue;
                    oleCmd.Parameters.Add(param);
                }
                da.SelectCommand = oleCmd;
                da.Fill(ret);
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, sql, ex);
            }
            finally
            {
                oleCmd.Dispose();
                oleCmd = null;
                da.Dispose();
                da = null;
                if (_transCount == 0 && _oleConn != null)
                {
                    _oleConn.Dispose();
                    _oleConn = null;
                }
            }
            return ret;
        }

        #endregion

        #region 分页的获取

        /// <summary>
        /// 以DataReader方式分页获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql, int pageIndex, int pageSize)
        {
            return GetDataTable(sql, CommandType.Text, pageIndex, pageSize, new ParameterCollection());
        }

        /// <summary>
        /// 以DataReader方式分页获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql, CommandType type, int pageIndex, int pageSize)
        {
            return GetDataTable(sql, type, pageIndex, pageSize, new ParameterCollection());
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="type">SQL语句类型</param>
        /// <param name="pageIndex">当前页数</param>
        /// <param name="pageSize">当前页显示的记录数</param>
        /// <param name="parameters">条件参数</param>
        /// <returns>数据集</returns>
        public DataTable GetDataTable(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters)
        {
            if (type == CommandType.Text)
            {
                return GetDataTableByRowNum(sql, type, pageIndex, pageSize, parameters);
            }
            else
            {
                return GetDataTableByDataReader(sql, type, pageIndex, pageSize, parameters);
            }
        }

        // 用rownum方式
        private DataTable GetDataTableByRowNum(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters)
        {
            // 对于Oracle，如果执行的是Sql语句，则采用Oracle数据库分页方式，从而减少DataReader占用的资源
            int startIndex = pageSize * (pageIndex - 1);
            int endIndex = startIndex + pageSize;

            string sqlCommand = @"SELECT * FROM
									(
									SELECT A.*, rownum R
									FROM
										(
											{0}
										) A
									WHERE rownum<={1}
									) B
								 WHERE R>{2}";

            return GetDataTable(string.Format(sqlCommand, sql, endIndex, startIndex));
        }

        // 用Reader方式
        private DataTable GetDataTableByDataReader(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters)
        {
            DataTable ret = new DataTable();

            int startIndex = 0;
            int endIndex = 0;
            int rowIndex = 0;
            DataRow row = null;
            OracleDataReader dr = null;
            OracleCommand oleCmd = new OracleCommand();

            try
            {
                startIndex = pageSize * (pageIndex - 1);
                endIndex = startIndex + pageSize - 1;
                // 判断是否有事务
                if (_transCount == 0)
                {
                    _oleConn = OpenConnection();
                    oleCmd.Connection = _oleConn;
                }
                else
                {
                    oleCmd.Connection = _oleConn;
                    oleCmd.Transaction = _oleTrans;
                }
                oleCmd.CommandText = sql;
                oleCmd.CommandType = type;
                // 设置参数
                for (int i = 0; i < parameters.Count; i++)
                {
                    OracleParameter param = new OracleParameter();
                    param.ParameterName = parameters[i].ParameterName;
                    param.Value = parameters[i].ParameterValue;
                    oleCmd.Parameters.Add(param);
                }
                dr = oleCmd.ExecuteReader();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    ret.Columns.Add(dr.GetName(i), dr.GetFieldType(i));
                }

                while (dr.Read())
                {
                    rowIndex++;
                    if (rowIndex > startIndex)
                    {
                        row = ret.NewRow();
                        for (int i = 0; i < ret.Columns.Count; i++)
                        {
                            row[i] = dr.GetValue(i);
                        }
                        ret.Rows.Add(row);
                    }
                    if (rowIndex > endIndex)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, sql, ex);
            }
            finally
            {
                dr = null;
                oleCmd.Dispose();
                oleCmd = null;
                if (_transCount == 0 && _oleConn != null)
                {
                    _oleConn.Dispose();
                    _oleConn = null;
                }
            }

            return ret;
        }

        #endregion

        #endregion

        #region Transaction

        /// <summary>
        /// 启动默认级别事务
        /// </summary>
        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// 启动指定级别事务
        /// </summary>
        public void BeginTransaction(IsolationLevel level)
        {
            try
            {
                _transCount++;
                if (_transCount == 1)
                {
                    _oleConn = OpenConnection();
                    _oleTrans = _oleConn.BeginTransaction(level);
                }
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, _connection, ex);
            }
        }

        /// <summary>
        /// 递交事务
        /// </summary>
        public bool Commit()
        {
            try
            {
                bool ret = false;
                _transCount--;
                if (_transCount == 0)
                {
                    _oleTrans.Commit();

                    ret = true;
                    _oleTrans.Dispose();
                    _oleTrans = null;
                    _oleConn.Dispose();
                    _oleConn = null;
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, _connection, ex);
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            try
            {
                _oleTrans.Rollback();
            }
            catch (Exception ex)
            {
                throw new TechException(ex.Message, _connection, ex);
            }
            finally
            {
                _transCount = 0;
                if (_oleTrans != null)
                {
                    _oleTrans.Dispose();
                    _oleTrans = null;
                }
                if (_oleConn != null)
                {
                    _oleConn.Dispose();
                    _oleConn = null;
                }
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_oleConn != null)
                    {
                        _oleConn.Dispose();
                        _oleConn = null;
                    }
                    _oleTrans = null;
                }
            }
            disposed = true;
        }

        #endregion

        public List<TEntity> GetListEntitiesWithPaging<TEntity>(string sql, int pageIndex, int pageSize) where TEntity : class
        {
            return this.GetListEntitiesWithPaging<TEntity>(sql, CommandType.Text, pageIndex, pageSize, new ParameterCollection());
        }


        public List<TEntity> GetListEntitiesWithPaging<TEntity>(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters) where TEntity : class
        {
            List<TEntity> list = new List<TEntity>();
            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            int num = 0;
            int num2 = 0;
            OracleDataReader reader = null;
            OracleCommand command = new OracleCommand();
            try
            {
                num = (pageSize * (pageIndex - 1)) + 1;
                num2 = (num + pageSize) - 1;
                if (this._transCount == 0)
                {
                    this._oleConn = this.OpenConnection();
                    command.Connection = this._oleConn;
                }
                else
                {
                    command.Connection = this._oleConn;
                    command.Transaction = this._oleTrans;
                }
                sql = "select * from (select cusview.*,rownum row_num from (" + sql + ") cusview where rownum <=" + num2.ToString() + ") WHERE row_num >= " + num.ToString();
                command.CommandText = sql;
                command.CommandType = type;
                for (int i = 0; i < parameters.Count; i++)
                {
                    OracleParameter parameter = new OracleParameter();
                    parameter.ParameterName = parameters[i].ParameterName;
                    parameter.Value = parameters[i].ParameterValue;
                    command.Parameters.Add(parameter);
                }
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    TEntity entity = Activator.CreateInstance<TEntity>();
                    foreach (PropertyInfo info in properties)
                    {
                        if ((((info.PropertyType.BaseType != typeof(EntityReference)) && (info.PropertyType.BaseType != typeof(RelatedEnd))) && ((info.PropertyType != typeof(EntityState)) && (info.PropertyType != typeof(EntityKey)))) && (info.PropertyType.BaseType != typeof(EntityObject)))
                        {
                            this.SetValue<TEntity>(info, entity, reader[info.Name]);
                        }
                    }
                    list.Add(entity);
                }
            }
            catch (Exception exception)
            {
                throw new TechException(exception.Message, sql, exception);
            }
            finally
            {
                reader = null;
                command.Dispose();
                command = null;
                if ((this._transCount == 0) && (this._oleConn != null))
                {
                    this._oleConn.Dispose();
                    this._oleConn = null;
                }
            }
            return list;
        }

        public TEntity SetValue<TEntity>(PropertyInfo property, TEntity entity, object propertyValue) where TEntity : class
        {
            if (property.CanRead)
            {
                if (!((!(property.PropertyType.Name != "String") || property.PropertyType.FullName.Contains("System.DateTime")) || property.PropertyType.FullName.Contains("System.Decimal")))
                {
                    return entity;
                }
                if (propertyValue == null)
                {
                    if (property.PropertyType.Name == "String")
                    {
                        propertyValue = "";
                    }
                    else if (property.PropertyType.FullName.Contains("System.DateTime"))
                    {
                        propertyValue = new DateTime();
                    }
                    else
                    {
                        propertyValue = 0;
                    }
                }
                else if (string.IsNullOrEmpty(propertyValue.ToString()))
                {
                    if (property.PropertyType.Name == "String")
                    {
                        propertyValue = "";
                    }
                    else if (property.PropertyType.FullName.Contains("System.DateTime"))
                    {
                        propertyValue = new DateTime();
                    }
                    else
                    {
                        propertyValue = 0;
                    }
                }
                if (property.CanWrite)
                {
                    if (property.PropertyType.Name == "String")
                    {
                        property.SetValue(entity, propertyValue, null);
                    }
                    else if (property.PropertyType.FullName.Contains("System.Decimal"))
                    {
                        property.SetValue(entity, Convert.ToDecimal(propertyValue), null);
                    }
                    else if (property.PropertyType.FullName.Contains("System.DateTime"))
                    {
                        property.SetValue(entity, Convert.ToDateTime(propertyValue), null);
                    }
                }
            }
            return entity;
        }

 

 


 

 

    }
}