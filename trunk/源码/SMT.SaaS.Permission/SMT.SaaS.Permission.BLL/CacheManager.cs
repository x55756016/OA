/*
 * 文件名：CacheManager.cs
 * 作  用：缓存管理
 * 创建人：向寒咏
 * 创建时间：2010-7-7 14:19:12
 * 修改人：
 * 修改说明：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Runtime.Caching;

namespace SMT.SaaS.Permission.BLL
{
    public class CacheManager
    {
        private static Hashtable SmtCache = Hashtable.Synchronized(new Hashtable());
        public static object GetCache(string CacheKey)
        {
            return CacheManager.SmtCache[CacheKey];
        }

        public static void AddCache(string CacheKey, object Entity)
        {
            #region 龙康才新增
            if (!SmtCache.Contains(CacheKey))
            {
                SmtCache.Add(CacheKey, Entity);
            }
            #endregion
            //SmtCache.Add(CacheKey, Entity);//原来代码
        }


        public static void RemoveCache(string CacheKey)
        {
            SmtCache.Remove(CacheKey);
        }
        #region 龙康才新增
        private static System.Runtime.Caching.ObjectCache objCache = MemoryCache.Default;
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        public static void AddObjectCache(string key, object obj)
        {

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(60);
            if (!objCache.Contains(key))
            {
                // objCache.Set(key, obj, policy);
                objCache.Add(key, obj, policy);
            }
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        /// <param name="second">缓存时间（分钟）</param>
        public static void AddObjectCache(string key, object obj, int second)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(second);
            if (!objCache.Contains(key))
            {
                objCache.Add(key, obj, policy);
            }

        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        public static object AddOrGetExisting(string key, object obj)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(60);
            if (!objCache.Contains(key))
            {
                obj = objCache.AddOrGetExisting(key, obj, policy);
            }
            return obj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">增加一个缓存</param>
        /// <param name="obj">关键字</param>
        /// <param name="second">缓存时间（分钟）</param>
        /// <returns></returns>
        public static object AddOrGetExisting(string key, object obj, int second)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromMinutes(second);
            if (!objCache.Contains(key))
            {
                // objCache.Set(key, obj, policy);
                obj = objCache.Add(key, obj, policy);
            }
            return obj;
        }
        /// <summary>
        ///删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        public static void RemoveObjectCache(string key)
        {
            if (objCache.Contains(key))
            {
                objCache.Remove(key);
            }
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static object GetObjectCache(string key)
        {
            object obj = null;
            if (objCache.Contains(key))
            {
                obj = objCache.Get(key);
            }
            return obj;
        }
        #endregion
    }
}
