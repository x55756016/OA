/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： SQLServer自定义数据访问接口
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using SMT.Foundation.Core.Error;
using SMT.Foundation.Core.Utilities;


namespace SMT.Foundation.Core
{
	/// <summary>
	/// 封装SqlServer的访问方法
	/// </summary>
    public sealed class SqlServerDAO : IDAO, IDisposable
	{
		#region Variable

        // 域名
        private static string _domain = "";

		// 数据库存连接字符串
		private string _connection;
		// 数据库连接对象
		private SqlConnection _oleConn = null;
		// 事务对象
		private SqlTransaction _oleTrans = null;
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
		public SqlServerDAO(string connection)
		{
			_connection = connection;
       
		}

		/// <summary>
		/// 析构函数
		/// </summary>
        ~SqlServerDAO()
		{
			Dispose(false);
		}

		#endregion

		#region OpenConnection

		// 返回打开的数据连接对象
		private SqlConnection OpenConnection()
		{
            using (SqlConnection oleConn = new SqlConnection(_connection))
            {
                //SqlConnection oleConn = new SqlConnection(_connection);
                try
                {
                    oleConn.Open();
                }
                catch (Exception ex)
                {
                    throw new TechException(ex.Message, _connection, ex);
                }
                return oleConn;
            }
		}

		#endregion

		#region Database Instance Content

		/// <summary>
		/// 参数前缀符
		/// </summary>
		public string DatabaseParameterPrefix
		{
			get {return "@";}
		}

		/// <summary>
		/// 数据库类型
		/// </summary>
		public string DatabaseType
		{
			get {return "sqlServer";}
		}

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public string DatabaseString
		{
			get {return _connection;}
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
			SqlCommand oleCmd = new SqlCommand();

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
				oleCmd.CommandText = WrapSql(sql, type);
                oleCmd.CommandType = CommandType.Text;
				// 设置参数
				for (int i = 0; i < parameters.Count; i++)
				{
                    SqlParameter param = new SqlParameter();
					param.ParameterName = parameters[i].ParameterName;
					param.Value = parameters[i].ParameterValue;
                    if (parameters[i].ParameterValue is Guid)
                    {
                        param.DbType = DbType.Guid;
                    }
					oleCmd.Parameters.Add(param);
				}
				ret = oleCmd.ExecuteNonQuery();
				// 获取参数返回值
				for (int i = 0 ; i < parameters.Count ; i++)
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
			SqlCommand oleCmd = new SqlCommand();

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
                oleCmd.CommandText = WrapSql(sql, type);
                oleCmd.CommandType = CommandType.Text;
                // 设置参数
				for (int i = 0 ; i < parameters.Count ; i++)
				{
                    SqlParameter param = new SqlParameter();
					param.ParameterName = parameters[i].ParameterName;
					param.Value = parameters[i].ParameterValue;
                    if (parameters[i].ParameterValue is Guid)
                    {
                        param.DbType = DbType.Guid;
                    }
					oleCmd.Parameters.Add(param);
				}
				ret = oleCmd.ExecuteScalar();
				// 获取参数返回值
				for (int i = 0 ; i < parameters.Count ; i++)
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
		/// <param name="sequence">获取值标识，Sql中为序列名称</param>
		/// <returns>增长值</returns>
		public int GetNextValue(string sequence)
		{
			return 0;
		}

		/// <summary>
		/// 获取序列或自动增长值的当前值
		/// </summary>
		/// <returns>当前值</returns>
		public int GetCurrentValue()
		{
            return TypeConverter.ToInt(ExecuteScalar("SELECT @@identity"));
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
			SqlDataAdapter da = new SqlDataAdapter();
			SqlCommand oleCmd = new SqlCommand();

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
                oleCmd.CommandText = WrapSql(sql, type);
                oleCmd.CommandType = CommandType.Text;
                // 设置参数
				for (int i = 0 ; i < parameters.Count ; i++)
				{
					SqlParameter param = new SqlParameter();
					param.ParameterName = parameters[i].ParameterName;
					param.Value = parameters[i].ParameterValue;
                    if (parameters[i].ParameterValue is Guid)
                    {
                        param.DbType = DbType.Guid;
                    }
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
                //return GetDataTableByRowNum(sql, type, pageIndex, pageSize, parameters);
                return GetDataTableByDataReader(sql, type, pageIndex, pageSize, parameters);
            }
            else
            {
                return GetDataTableByDataReader(sql, type, pageIndex, pageSize, parameters);
            }
        }

        // 用rownum方式
        private DataTable GetDataTableByRowNum(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters)
        {
            //SELECT * 
            //FROM (select *,ROW_NUMBER() Over(order by id) as rowNum from table_info )as myTable
            //where rowNum between 50 and 60;

            //注：SQL Server 的ROW_NUMBER()函数性能，通用性都不够，暂不使用

            return null;

        }

        // 用Reader方式
        private DataTable GetDataTableByDataReader(string sql, CommandType type, int pageIndex, int pageSize, ParameterCollection parameters)
        {
            DataTable ret = new DataTable();

            int startIndex = 0;
            int endIndex = 0;
            int rowIndex = 0;
            DataRow row = null;
            SqlDataReader dr = null;
            SqlCommand oleCmd = new SqlCommand();

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
                oleCmd.CommandText = WrapSql(sql, type);
                oleCmd.CommandType = CommandType.Text;
                // 设置参数
                for (int i = 0; i < parameters.Count; i++)
                {
                    SqlParameter param = new SqlParameter();
                    param.ParameterName = parameters[i].ParameterName;
                    param.Value = parameters[i].ParameterValue;
                    if (parameters[i].ParameterValue is Guid)
                    {
                        param.DbType = DbType.Guid;
                    }
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
                if (_oleTrans != null)
                {
                    _oleTrans.Rollback();
                }
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
			if(!this.disposed)
			{
				if(disposing)
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

        #region Private 

        private string GetImpersonatedDomainUser()
        {            
            string sql = "select suser_sname();";

            if (_domain.Length == 0)
            {
                SqlConnection sqlCon = OpenConnection();
                SqlCommand oleCmd = new SqlCommand();

                oleCmd.Connection = sqlCon;
                oleCmd.CommandText = sql;
				oleCmd.CommandType = CommandType.Text;

                _domain = oleCmd.ExecuteScalar().ToString().Split('\\')[0];
            }

            string[] names = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');

            return _domain + "\\" + names[1];
        }

        private string WrapSql(string srcSql, CommandType type)
        {
            //return srcSql;
            string sql = srcSql;

            sql = "execute as user = '" + GetImpersonatedDomainUser() + "' " + srcSql + " revert;";

            //if (type == CommandType.Text)
            //{
            //    sql = "execute as user = '" + GetImpersonatedDomainUser() + "' " + srcSql + " revert;";
            //}
            //else
            //{
            //    sql = "execute as user = '" + GetImpersonatedDomainUser() + "' " + srcSql + " revert;";
            //}

            return sql;
        }

        #endregion
    }
}