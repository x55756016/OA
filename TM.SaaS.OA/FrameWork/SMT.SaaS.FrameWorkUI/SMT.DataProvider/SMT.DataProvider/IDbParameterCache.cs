using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Oracle.DataAccess.Client;
using System.Data.SqlClient;
namespace SMT
{
    public sealed class IDbParameterCache
    {

        #region 私有方法,字段,构造函数
        // 私有构造函数,妨止类被实例化.
        private IDbParameterCache() { }
        // 这个方法要注意
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());
        /// <summary>
        /// 探索运行时的存储过程,返回IDbDataParameter参数数组.
        /// 初始化参数值为 DBNull.Value.
        /// </summary>
        /// <param>一个有效的数据库连接</param>
        /// <param>存储过程名称</param>
        /// <param>是否包含返回值参数</param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        private static IDbDataParameter[] DiscoverSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("IDbConnection 连接对象为NULL");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("存储过程名称为null");
            IDbCommand cmd = DataBaseFactory.GetIDbCommand();//. new IDbCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;
            connection.Open();
            // 检索cmd指定的存储过程的参数信息,并填充到cmd的Parameters参数集中.
           // SqlCommandBuilder.DeriveParameters(cmd); //这里考虑一下如何是区分不同数据库的填充
           
            connection.Close();
            // 如果不包含返回值参数,将参数集中的每一个参数删除.

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }
            // 创建参数数组
            IDbDataParameter[] discoveredParameters = new IDbDataParameter[cmd.Parameters.Count];
            // 将cmd的Parameters参数集复制到discoveredParameters数组.
            cmd.Parameters.CopyTo(discoveredParameters, 0);
            // 初始化参数值为 DBNull.Value.
            foreach (IDbDataParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }
        /// <summary>
        /// IDbDataParameter参数数组的深层拷贝.
        /// </summary>
        /// <param>原始参数数组</param>
        /// <returns>返回一个同样的参数数组</returns>
        private static IDbDataParameter[] CloneParameters(IDbDataParameter[] originalParameters)
        {
            IDbDataParameter[] clonedParameters = new IDbDataParameter[originalParameters.Length];
            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (IDbDataParameter)((ICloneable)originalParameters[i]).Clone();
            }
            return clonedParameters;
        }

        #endregion 私有方法,字段,构造函数结束

        #region 缓存方法
        /// <summary>
        /// 追加参数数组到缓存.
        /// </summary>
        /// <param>一个有效的数据库连接字符串</param>
        /// <param>存储过程名或SQL语句</param>
        /// <param>要缓存的参数数组</param>
        public static void CacheParameterSet(string connectionString, string commandText, params IDbDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("连接字符串不能为空");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("存储过程名或SQL语句不能为空");
            string hashKey = connectionString + ":" + commandText;
            paramCache[hashKey] = commandParameters;
        }
        /// <summary>
        /// 从缓存中获取参数数组.
        /// </summary>
        /// <param>一个有效的数据库连接字符</param>
        /// <param>存储过程名或SQL语句</param>
        /// <returns>参数数组</returns>
        public static IDbDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("连接字符串不能为空");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("存储过程名或SQL语句不能为空");
            string hashKey = connectionString + ":" + commandText;
            IDbDataParameter[] cachedParameters = paramCache[hashKey] as IDbDataParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }
        #endregion 缓存方法结束
        #region 检索指定的存储过程的参数集
        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param>一个有效的数据库连接字符</param>
        /// <param>存储过程名</param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        public static IDbDataParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }
        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param>一个有效的数据库连接字符.</param>
        /// <param>存储过程名</param>
        /// <param>是否包含返回值参数</param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        public static IDbDataParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("连接字符串不能为空");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("存储过程名不能为空");
            using (IDbConnection connection =DataBaseFactory.GetIDbConnection())
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }
        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象).
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param>一个有效的数据库连接字符</param>
        /// <param>存储过程名</param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        internal static IDbDataParameter[] GetSpParameterSet(IDbConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }
        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param>一个有效的数据库连接对象</param>
        /// <param>存储过程名</param>
        /// <param>
        /// 是否包含返回值参数
        /// </param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        internal static IDbDataParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("连接字符串不能为空");
            using (IDbConnection clonedConnection = (IDbConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }
         /// <summary>
        /// [私有]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <param>一个有效的数据库连接对象</param>
        /// <param>存储过程名</param>
        /// <param>是否包含返回值参数</param>
        /// <returns>返回IDbDataParameter参数数组</returns>
        private static IDbDataParameter[] GetSpParameterSetInternal(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("连接字符串不能为空");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("存储过程名不能为空");
            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            IDbDataParameter[] cachedParameters;
            cachedParameters = paramCache[hashKey] as IDbDataParameter[];
            if (cachedParameters == null)
            {
                IDbDataParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }
            return CloneParameters(cachedParameters);

        }
        #endregion 参数集检索结束
    }
}
