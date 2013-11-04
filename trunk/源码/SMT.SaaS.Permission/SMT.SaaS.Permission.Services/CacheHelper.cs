#region
////======================================================================
////
//// Copyright (C) 2007-2008 
//// All rights reserved
//// 本  机:TIANAN-KEVIN
//// 文件名:SetCache
//// 描  述:
//// 创建者:龙康才 
//// 时  间:2008-12-15 17:23:49
////
////====================================================================== 
//using System;
//using System.Web;
//using System.Web.Caching;
//using System.Collections;
//using System.Text.RegularExpressions;
//using System.Web.Caching;
//namespace SMT.SaaS.Permission.Services
//{
//    /// <summary>
//    /// 设置Cache操作类
//    /// </summary>
//    public class CacheHelper
//    {
//        #region 用户自定义变量
//        private static readonly Cache _cache;//缓存实例
//        private static readonly int hourfactor;
//        #endregion

//        #region 构造函数
//        static CacheHelper()
//        {
//            hourfactor = 3600;
//            _cache = HttpRuntime.Cache;
//        }

//        private CacheHelper()
//        {
//        }
//        #endregion

//        #region 清除所有缓存
//        /// <summary>
//        /// 清除所有缓存
//        /// </summary>
//        public static void Clear()
//        {
//            //要循环访问 Cache 对象的枚举数
//            IDictionaryEnumerator enumerator = _cache.GetEnumerator();//检索用于循环访问包含在缓存中的键设置及其值的字典枚举数
//            if (enumerator != null)
//            {
//                while (enumerator.MoveNext())
//                {
//                    _cache.Remove(enumerator.Key.ToString());
//                }
//            }
//        }
//        #endregion

//        #region 得到缓存实例
//        /// <summary>
//        /// 得到缓存实例
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <returns>返回缓存实例</returns>
//        public static object GetCache(string key)
//        {
//            return _cache[key];
//        }
//        #endregion

//        #region 缓存实例插入
//        /// <summary>
//        /// 缓存实例插入(默认缓存20分钟)
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <param name="obj">要缓存的对象</param>
//        public static void Insert(string key, object obj)
//        {
//            CacheDependency dep = null;
//            Insert(key, obj, dep, 20);
//        }

//        /// <summary>
//        /// 缓存实例插入
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <param name="obj">要缓存的对象</param>
//        /// <param name="seconds">缓存的时间</param>
//        public static void Insert(string key, object obj, int seconds)
//        {
//            CacheDependency dep = null;
//            Insert(key, obj, dep, seconds);
//        }

//        /// <summary>
//        /// 缓存实例插入(缓存过期时间是一天)
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <param name="obj">要缓存的对象</param>
//        /// <param name="dep">缓存的依赖项</param>
//        public static void Insert(string key, object obj, CacheDependency dep)
//        {
//            Insert(key, obj, dep, hourfactor * 12);
//        }

//        /// <summary>
//        /// 缓存实例插入(缓存过期时间是一天)
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <param name="obj">要缓存的对象</param>
//        /// <param name="xmlPath">缓存的依赖项xml文件的路径（绝对路径）</param>
//        public static void Insert(string key, object obj, string xmlPath)
//        {
//            CacheDependency dep = new CacheDependency(xmlPath);
//            Insert(key, obj, dep, hourfactor * 12);
//        }

//        /// <summary>
//        /// 缓存实例插入
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        /// <param name="obj">要缓存的对象</param>
//        /// <param name="seconds">缓存时间</param>
//        /// <param name="priority">该对象相对于缓存中存储的其他项的成本</param>
//        public static void Insert(string key, object obj, int seconds, CacheItemPriority priority)
//        {
//            Insert(key, obj, null, seconds, priority);
//        }

//        /// <summary>
//        /// 缓存实例插入
//        /// </summary>
//        /// <param name="key">用于引用该对象的缓存键</param>
//        /// <param name="obj">要插入缓存中的对象</param>
//        /// <param name="dep">该项的文件依赖项或缓存键依赖项。当任何依赖项更改时，该对象即无效，并从缓存中移除。如果没有依赖项，则此参数包含空引用（Visual Basic 中为 Nothing）</param>
//        /// <param name="seconds">所插入对象将过期并被从缓存中移除的时间。</param>
//        public static void Insert(string key, object obj, CacheDependency dep, int seconds)
//        {
//            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
//        }

//        /// <summary>
//        /// 缓存实例插入
//        /// </summary>
//        /// <param name="key">用于引用该对象的缓存键</param>
//        /// <param name="obj">要插入缓存中的对象</param>
//        /// <param name="xmlPath">缓存的依赖项xml文件的路径（绝对路径）</param>
//        /// <param name="seconds">所插入对象将过期并被从缓存中移除的时间。</param>
//        public static void Insert(string key, object obj, string xmlPath, int seconds)
//        {
//            CacheDependency dep = new CacheDependency(xmlPath);
//            Insert(key, obj, dep, seconds, CacheItemPriority.Normal);
//        }

//        /// <summary>
//        /// 缓存实例插入
//        /// </summary>
//        /// <param name="key">用于引用该对象的缓存键</param>
//        /// <param name="obj">要插入缓存中的对象</param>
//        /// <param name="dep">该项的文件依赖项或缓存键依赖项。当任何依赖项更改时，该对象即无效，并从缓存中移除。如果没有依赖项，则此参数包含空引用（Visual Basic 中为 Nothing）。</param>
//        /// <param name="seconds">所插入对象将过期并被从缓存中移除的时间。</param>
//        /// <param name="priority">该对象相对于缓存中存储的其他项的成本，由 CacheItemPriority 枚举表示。该值由缓存在退出对象时使用；具有较低成本的对象在具有较高成本的对象之前被从缓存移除。 </param>
//        public static void Insert(string key, object obj, CacheDependency dep, int seconds, CacheItemPriority priority)
//        {
//            if (obj != null)
//            {
//                _cache.Insert(key, obj, dep, DateTime.Now.AddSeconds((double)seconds), TimeSpan.Zero, priority, null);
//            }
//        }
//        #endregion

//        #region 移出单个缓存
//        /// <summary>
//        /// 移出单个缓存
//        /// </summary>
//        /// <param name="key">缓存实例名称</param>
//        public static void Remove(string key)
//        {
//            _cache.Remove(key);
//        }
//        #endregion

//        #region 得到所有使用的Cache键值
//        /// <summary>
//        /// 得到所有使用的Cache键值
//        /// </summary>
//        /// <returns>返回所有的Cache键值</returns>
//        public static ArrayList GetAllCacheKey()
//        {
//            ArrayList arrList = new ArrayList();
//            IDictionaryEnumerator enumerator = _cache.GetEnumerator();
//            if (enumerator != null)
//            {
//                while (enumerator.MoveNext())
//                {
//                    arrList.Add(enumerator.Key);
//                }
//            }
//            return arrList;
//        }
//        #endregion
//        private bool _IsDispose = false;
        
//        #region 初始化
//        /// <summary>
//        /// 垃圾回收
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        /// <param name="disposing">是否释放过资源</param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!this._IsDispose)
//            {
//                if (disposing)
//                {
//                    GC.Collect();
//                }
//            }
//            _IsDispose = true;

//        }
//        /// <summary>
//        /// 析构
//        /// </summary>
//        ~CacheHelper()
//        {
//            Dispose(false);
//        }
//        #endregion
//        /// <summary>
//        /// 设置当前应用程序指定CacheKey的Cache值
//        /// </summary>
//        /// <param name="cacheKey">缓存Key</param>
//        /// <returns></returns>
//        public static bool IsExist(string cacheKey)
//        {
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            if(objCache.Get(cacheKey)!=null)
//            {
//                return true;
//            }
//            else{
//                return false;
//            }
//        }
//        /// <summary>
//        /// 设置当前应用程序指定CacheKey的Cache值
//        /// </summary>
//        /// <param name="cacheKey">缓存Key</param>
//        /// <param name="objPackage">缓存内容</param>
//        public static void SetCache(string cacheKey, object objPackage)
//        {
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            objCache.Insert(cacheKey, objPackage);
//        }
//        /// <summary>
//        /// 设置当前应用程序指定CacheKey的Cache值
//        /// </summary>
//        /// <param name="cacheKey">缓存Key</param>
//        /// <param name="objPackage">缓存内容</param>
//        /// <param name="intMinute">过期时间 分钟</param>
//        public static void SetCache(string cacheKey, object objPackage,int intMinute)
//        {
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            objCache.Insert(cacheKey, objPackage,null,DateTime.Now.AddMinutes(intMinute),TimeSpan.Zero);
//        }
//        /// <summary>
//        /// 获取当前应用程序指定CacheKey的Cache值
//        /// </summary>
//        /// <param name="cacheKey">Key</param>
//        /// <returns></returns>
//        public static object GetCacheValue(string cacheKey)
//        {
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            return objCache[cacheKey];
//        }
        
//        /// <summary>
//        /// 移除指定CacheKey的值
//        /// </summary>
//        /// <param name="cacheKey">缓存Key</param>
//        public static void RemoveCache(string cacheKey)
//        {
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            objCache.Remove(cacheKey);
//        }
//        /// <summary>
//        /// 清空缓存
//        /// </summary>
//        public static void CacheClear()
//        { 
//            System.Web.Caching.Cache objCache = HttpRuntime.Cache;
//            System.Collections.IDictionaryEnumerator cacheEnumer = objCache.GetEnumerator();
//            while (cacheEnumer.MoveNext())
//            {
//                objCache.Remove(cacheEnumer.Key.ToString());
//            }
//        }

//    }
//}
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace SMT.SaaS.Permission.Services
{
    public class WCFCache
    {
        private static WCFCache _current = new WCFCache();

        private ReaderWriterLockSlim _itemsLock = new ReaderWriterLockSlim();
        private static Hashtable _items = new Hashtable();

        private object _cacheRefreshFrequencyLock = new object();
        private TimeSpan _cacheRefreshFrequency = new TimeSpan(0, 0, 10);

        private Timer _timer = null;

        public TimeSpan CacheRefreshFrequency
        {
            get
            {
                TimeSpan res;

                lock (_cacheRefreshFrequencyLock)
                {
                    res = _cacheRefreshFrequency;
                }

                return res;
            }
            set
            {
                lock (_cacheRefreshFrequencyLock)
                {
                    _cacheRefreshFrequency = value;
                }

                int refreshFrequencyMilliseconds = (int)CacheRefreshFrequency.TotalMilliseconds;
                this._timer.Change(refreshFrequencyMilliseconds, refreshFrequencyMilliseconds);
            }
        }

        public int CachedItemsNumber
        {
            get
            {
                _itemsLock.EnterReadLock();
                try
                {
                    return _items.Count;
                }
                finally
                {
                    _itemsLock.ExitReadLock();
                }
            }
        }

        private WCFCache()
        {
            int refreshFrequencyMilliseconds = (int)CacheRefreshFrequency.TotalMilliseconds;
            this._timer = new System.Threading.Timer(new
                       TimerCallback(CacherefreshTimerCallback),
                       null, refreshFrequencyMilliseconds, refreshFrequencyMilliseconds);
        }

        public static WCFCache Current
        {
            get
            {
                return _current;
            }
        }

        public object this[object key]
        {
          
            get
            {
                if (key == null)
                    return null;
                _itemsLock.EnterUpgradeableReadLock();
                try
                {
                    WCFCacheItem res = (WCFCacheItem)_items[key];
                    if (res != null)
                    {
                        if (res.SlidingExpirationTime.TotalMilliseconds > 0)
                        {
                            _itemsLock.EnterWriteLock();
                            try
                            {
                                res.LastAccessTime = DateTime.Now;
                            }
                            finally
                            {
                                _itemsLock.ExitWriteLock();
                            }
                        }
                        return res.ItemValue;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    _itemsLock.ExitUpgradeableReadLock();
                }
            }
            set
            {
                _itemsLock.EnterWriteLock();
                try
                {
                    _items[key] = new WCFCacheItem(value);
                }
                finally
                {
                    _itemsLock.ExitWriteLock();
                }
            }
        }

        public void Insert(object key, object value)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }

        public void Insert(object key, object value, DateTime expirationDate)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value, expirationDate);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
        public void Insert(object key, object value, int Minutes)
        {
            DateTime expirationDate = DateTime.Now.AddMinutes(Minutes);
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value, expirationDate);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
        public void Insert(object key, object value, TimeSpan expirationTime)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value, expirationTime);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }

        public void Insert(object key, object value, TimeSpan expirationTime, bool slidingExpiration)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value, expirationTime, slidingExpiration);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }

        public void Insert(object key, object value, DateTime expirationDate, TimeSpan slidingExpirationTime)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items[key] = new WCFCacheItem(value, expirationDate, slidingExpirationTime);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }

        public void Remove(object key)
        {
            _itemsLock.EnterWriteLock();
            try
            {
                _items.Remove(key);
            }
            finally
            {
                _itemsLock.ExitWriteLock();
            }
        }
        public void Clear()
        {
            _itemsLock.EnterUpgradeableReadLock();
            try
            {
                Dictionary<object, WCFCacheItem> delItems = new Dictionary<object, WCFCacheItem>();
                DateTime dtNow = DateTime.Now;     
                if (delItems.Count > 0)
                {
                    _itemsLock.EnterWriteLock();
                    try
                    {
                        foreach (KeyValuePair<object, WCFCacheItem> kvp in delItems)
                        {
                            if (_items.ContainsKey(kvp.Key))
                            {
                                _items.Remove(kvp.Key);
                            }
                        }
                    }
                    finally
                    {
                        _itemsLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _itemsLock.ExitUpgradeableReadLock();
            }
        }


        private void CacherefreshTimerCallback(object state)
        {
            _itemsLock.EnterUpgradeableReadLock();
            try
            {
                Dictionary<object, WCFCacheItem> delItems = new Dictionary<object, WCFCacheItem>();
                DateTime dtNow = DateTime.Now;
                foreach (DictionaryEntry de in _items)
                {
                    WCFCacheItem ci = (WCFCacheItem)de.Value;
                    if (ci.ExpirationDate < dtNow)
                    {
                        delItems.Add(de.Key, ci);
                    }
                    else
                    {
                        if (ci.SlidingExpirationTime.TotalMilliseconds > 0)
                        {
                            if (dtNow.Subtract(ci.LastAccessTime).TotalMilliseconds > ci.SlidingExpirationTime.TotalMilliseconds)
                            {
                                delItems.Add(de.Key, ci);
                            }
                        }
                    }
                }

                if (delItems.Count > 0)
                {
                    _itemsLock.EnterWriteLock();
                    try
                    {
                        foreach (KeyValuePair<object, WCFCacheItem> kvp in delItems)
                        {
                            if (_items.ContainsKey(kvp.Key))
                            {
                                _items.Remove(kvp.Key);
                            }
                        }
                    }
                    finally
                    {
                        _itemsLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _itemsLock.ExitUpgradeableReadLock();
            }
        }
    }
    public class WCFCacheItem
    {
        private object _itemValue;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _expirationDate = DateTime.MaxValue;
        private TimeSpan _slidingExpirationTime = new TimeSpan();
        private DateTime _lastAccessTime = DateTime.Now;

        public object ItemValue
        {
            get { return _itemValue; }
            set { _itemValue = value; }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        public DateTime ExpirationDate
        {
            get { return _expirationDate; }
            set { _expirationDate = value; }
        }

        public TimeSpan SlidingExpirationTime
        {
            get { return _slidingExpirationTime; }
            set { _slidingExpirationTime = value; }
        }

        public DateTime LastAccessTime
        {
            get { return _lastAccessTime; }
            set { _lastAccessTime = value; }
        }

        public WCFCacheItem(object itemValue)
        {
            this._itemValue = itemValue;
        }

        public WCFCacheItem(object itemValue, DateTime expirationDate)
            : this(itemValue)
        {
            this._expirationDate = expirationDate;
        }

        public WCFCacheItem(object itemValue, TimeSpan expirationTime)
        {
            this._itemValue = itemValue;
            this._expirationDate = this._createdDate.Add(expirationTime);
        }

        public WCFCacheItem(object itemValue, TimeSpan expirationTime, bool slidingExpiration)
        {
            this._itemValue = itemValue;
            if (slidingExpiration)
            {
                this._slidingExpirationTime = expirationTime;
            }
            else
            {
                this._expirationDate = this._createdDate.Add(expirationTime);
            }
        }

        public WCFCacheItem(object itemValue, DateTime expirationDate, TimeSpan slidingExpirationTime)
            : this(itemValue, expirationDate)
        {
            this._slidingExpirationTime = slidingExpirationTime;
        }
    }
}
