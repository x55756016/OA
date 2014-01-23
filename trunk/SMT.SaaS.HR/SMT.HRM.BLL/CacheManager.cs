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

namespace SMT.HRM.BLL
{
    public class CacheManager
    {
        private static Hashtable SmtCache = Hashtable.Synchronized(new Hashtable());


        public static object GetCache(string CacheKey)
        {
            return CacheManager.SmtCache[CacheKey];
        }

        public static void AddCache(string CacheKey,object Entity)
        {
            if (!SmtCache.ContainsKey(CacheKey))
            {
                SmtCache.Add(CacheKey, Entity);
            }
        }


        public static void RemoveCache(string CacheKey)
        {
            if (SmtCache.ContainsKey(CacheKey))
            {
                SmtCache.Remove(CacheKey);
            }
        }
    }
}
