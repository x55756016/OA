using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data.SqlClient;

namespace SMT
{
    /// <summary>
    /// Sql参数缓存类
    /// </summary>
    class SqlParameterCache
    {
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());
        private SqlParameterCache() { }
        /// <summary>
        /// 缓存参数
        /// </summary>
        /// <param name="cacheKey">关键字</param>
        /// <param name="cmdParms">参数数组</param>
        public static void CacheParameters(string cacheKey, params SqlParameter[] cmdParms)
        {
            parmCache[cacheKey] = cmdParms;
        }
        /// <summary>
        /// 缓存对象
        /// </summary>
        /// <param name="cacheKey">关键字</param>
        /// <param name="cmdParms">对象</param>
        public static void CacheObject(string cacheKey, object obj)
        {
            parmCache[cacheKey] = obj;
        }
        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheKey">关键字</param>
        /// <returns></returns>
        public static object GetCachedObject(string cacheKey)
        {
            object cachedParms = (object[])parmCache[cacheKey];
            return cachedParms;
        }
        /// <summary>
        /// 获取参数对象
        /// </summary>
        /// <param name="cacheKey">关键字</param>
        /// <returns></returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];
            if (cachedParms == null)
                return null;

            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];
            for (int i = 0, j = cachedParms.Length; i < j; i++)
            {
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone(); //复制一份
            }
            return clonedParms;
        }
        /* 调用事例 
        private const string inserSQL = "INSERT INTO Users VALUES (@UserId, @Password)";
        public bool SignIn(string userId, string password)
        {
            SqlParameter[] signOnParms = GetSignOnParameters();  // 调用函数,得到参数数组
            signOnParms[0].Value = userId;
            signOnParms[1].Value = password;
            using (SqlDataReader rdr = SqlDataProvider.ExecuteReader(inserSQL, signOnParms))
            {
                if (rdr.Read())
                {
                    return ture;
                }
                return null;
            }
        }

        private static SqlParameter[] GetSignOnParameters()
        {
            SqlParameter[] parms = SqlParameterCache.GetCachedParameters(inserSQL);  //看缓存中是否有
            if (parms == null)             //开始调用时空，然后创建
            {
                parms = new SqlParameter[] {
                              new SqlParameter(USERID, SqlDbType.VarChar, 50),
                              new SqlParameter(PASSWORD, SqlDbType.VarChar, 50)};
                SqlParameterCache.CacheParameters(inserSQL, parms);    // 调用函数。存入hashtable中，它是用健值对表示的，实现了缓存; 键值是操作数据库的常量字符串
            }
            return parms;
        }    
        */
    }
   
}


