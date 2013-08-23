using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;
using System.Collections;

namespace SMT.Workflow.SMTCache
{
    public static class CacheOperate
    {
        private static readonly Cache cache;

        static CacheOperate()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                cache = System.Web.HttpContext.Current.Cache;
            }
            else
            {
                cache = HttpRuntime.Cache;
            }
        }
        public static void ClearAll()
        {
            IDictionaryEnumerator CacheEnum = cache.GetEnumerator();
            ArrayList al = new ArrayList();
            while (CacheEnum.MoveNext())
            {
                al.Add(CacheEnum.Key);
            }
            foreach (string key in al)
            {
                cache.Remove(key);
            }
        }
        public static void Remove(string key)
        {
            cache.Remove(key);
        }
        public static void Insert(string key, object value)
        {
            cache.Insert(key, value);
        }
        public static void Insert(string key, object value, CacheDependency dep)
        {
            cache.Insert(key, value, dep);

        }
        public static void Insert(string key, object value, CacheDependency dep, int minuts)
        {
            Insert(key, value, dep, minuts, CacheItemPriority.Default);
        }
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="obj">缓存对象</param>
        /// <param name="dep"></param>
        /// <param name="seconds"></param>
        /// <param name="priority"></param>
        public static void Insert(string key, object obj, CacheDependency dep, int minuts, CacheItemPriority priority)
        {
            if (obj != null)
            {
                cache.Insert(key, obj, dep, DateTime.Now.AddMinutes(minuts), TimeSpan.Zero, priority, null);
            }

        }
        public static object Get(string key)
        {

            return cache[key];
        }
    }
}
