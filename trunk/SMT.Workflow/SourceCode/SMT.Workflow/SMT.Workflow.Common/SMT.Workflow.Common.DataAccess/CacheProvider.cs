/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：CacheProvider.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/12/13 14:33:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Common.DataAccess
	 * 模块名称：
	 * 描　　述： 	 缓存管理类（包括创建对象和静态方法二个）
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;

namespace SMT.Workflow.Common.DataAccess
{
    /// <summary>
    /// 缓存管理类（包括创建对象和静态方法二个）
    /// </summary>
    public class CacheProvider
    {
        public CacheProvider()
        { }
        #region 文件依赖缓存
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="data">泛型对象</param>
        public static void FileCachInsert(string key, object data,string path)
        {
            CacheDependency depend = new CacheDependency(path);//设置缓存依赖对象    
            HttpRuntime.Cache.Insert(key, data, depend);//插入缓存                      
            if (depend.HasChanged)//依赖文件1：//1.xml是否修改            
            {
                LogHelper.WriteLog("缓存文件已修改,上次更改时间:"+depend.UtcLastModified.ToString()+" 路径:" + path);
            }           
        }
        public static object FileCachGet(string key)
        { 
            return HttpRuntime.Cache.Get(key);  
        }
        #endregion
        #region 对象 成员

        /// <summary>
        /// 数量
        /// </summary>
        public int Count
        {
            get { return MCache.Count(); }
        }
        private ObjectCache _mcache;
        private ObjectCache MCache
        {
            get
            {
                if (_mcache == null)
                    _mcache = MemoryCache.Default;
                return _mcache;
            }
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="data">泛型对象</param>
        public void Insert<T>(string key, T data)
        {
            
            MCache.Add(key, data, null);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        public void Insert(string key, object data)
        {
            MCache.Add(key, data, null);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="minutes">缓存时间：分钟</param>
        public void Insert(string key, object data, double minutes)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(minutes);
            MCache.Add(key, data, policy);
            // MCache.Set(key, data, policy);

        }
        /// <summary>
        ///  增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="settings">CacheItemPolicy</param>
        public void Insert(string key, object data, CacheItemPolicy settings)
        {
            MCache.Add(key, data, settings);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="settings">CacheItemPolicy</param>
        public void Insert<T>(string key, T data, CacheItemPolicy settings)
        {
            MCache.Add(key, data, settings);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public object Get(string key)
        {
            return MCache[key];
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return (T)MCache[key];
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="cacheKey">关键字</param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public T Get<T>(string cacheKey, Func<T> getData)
        {
            T tdata = Get<T>(cacheKey);
            if (tdata == null)
            {
                tdata = getData();
                Insert(cacheKey, tdata);
            }
            return tdata;
        }
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        public void Remove(string key)
        {
            MCache.Remove(key);
        }
        /// <summary>
        /// 清除缓存
        /// </summary>
        public void Clear()
        {
            _mcache = MemoryCache.Default;
        }
        public T Get<T>(string cacheKey, Func<T> getData, CacheItemPolicy settings)
        {
            T tdata = Get<T>(cacheKey);
            if (tdata == null)
            {
                tdata = getData();
                Insert(cacheKey, tdata, settings);
            }
            return tdata;
        }
        #endregion
        #region 静态 成员

        /// <summary>
        /// 数量
        /// </summary>
        public static int Number
        {
            get { return SMTCache.Count(); }
        }
        private static ObjectCache _smtSMTCache;
        private static ObjectCache SMTCache
        {
            get
            {
                if (_smtSMTCache == null)
                    _smtSMTCache = MemoryCache.Default;
                return _smtSMTCache;
            }
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="data">泛型对象</param>
        public static void Add<T>(string key, T data)
        {
            SMTCache.Add(key, data, null);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        public static void Add(string key, object data)
        {
            SMTCache.Add(key, data, null);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="minutes">缓存时间：分钟</param>
        public static void Add(string key, object data, double minutes)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(minutes);
            SMTCache.Add(key, data, policy);
            // SMTCache.Set(key, data, policy);

        }
        /// <summary>
        ///  增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="settings">CacheItemPolicy</param>
        public static void Add(string key, object data, CacheItemPolicy settings)
        {
            SMTCache.Add(key, data, settings);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <param name="data">缓存对象</param>
        /// <param name="settings">CacheItemPolicy</param>
        public static void Add<T>(string key, T data, CacheItemPolicy settings)
        {
            SMTCache.Add(key, data, settings);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static object GetCache(string key)
        {
            return SMTCache[key];
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static T GetCache<T>(string key)
        {
            return (T)SMTCache[key];
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="cacheKey">关键字</param>
        /// <param name="getData"></param>
        /// <returns></returns>
        public static T GetCache<T>(string cacheKey, Func<T> getData)
        {
            T tdata = GetCache<T>(cacheKey);
            if (tdata == null)
            {
                tdata = getData();
                Add(cacheKey, tdata);
            }
            return tdata;
        }
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        public static void RemoveCache(string key)
        {
            object obj=SMTCache.Remove(key);
        }
        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            _smtSMTCache = MemoryCache.Default;
        }
        public static T GetCache<T>(string cacheKey, Func<T> getData, CacheItemPolicy settings)
        {
            T tdata = GetCache<T>(cacheKey);
            if (tdata == null)
            {
                tdata = getData();
                Add(cacheKey, tdata, settings);
            }
            return tdata;
        }
        #endregion
    }
}
