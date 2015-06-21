using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using Enyim.Caching;
using Enyim.Caching.Memcached;
/*
 *缓存管理 
 * 
 * 
 */
namespace TM.SaaS.CommonBll
{
    public class CacheManagerMem
    {
        protected static MemcachedClient memCacheClient;
        //private static Hashtable SmtCache = Hashtable.Synchronized(new Hashtable());
        public static MemcachedClient CacheClinet
        {
            get
            {
                if (memCacheClient == null)
                {
                    memCacheClient = new MemcachedClient();                    
                }
                return memCacheClient;
            }
        }

        public static void CachaeAddObj(string key,object obj)
        {
            CacheClinet.Store(StoreMode.Set, key, obj);
        }

        public static object GetCacheObj(string key)
        {
            var obj = CacheClinet.Get(key);
            if (obj == null)
            {
                return null;
            }
            return obj;
        }

        public static void RemoveCache(string CacheKey)
        {
            CacheClinet.Remove(CacheKey);
        }



        public static object GetCache(string CacheKey)
        {
            return GetCacheObj(CacheKey);
        }

        public static void AddCache(string CacheKey, object Entity)
        {
            #region 龙康才新增
            if (CacheClinet.Get(CacheKey)==null)
            {
                CachaeAddObj(CacheKey, Entity);
            }
            #endregion
            //SmtCache.Add(CacheKey, Entity);//原来代码
        }

        #region 龙康才新增
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        public static void AddObjectCache(string key, object obj)
        {
            CachaeAddObj(key, obj);
        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        /// <param name="second">缓存时间（分钟）</param>
        public static void AddObjectCache(string key, object obj, int second)
        {
            TimeSpan ds=new TimeSpan(0,0,second);
            CacheClinet.Store(StoreMode.Set, key, obj,ds);

        }
        /// <summary>
        /// 增加一个缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="obj">对象</param>
        public static object AddOrGetExisting(string key, object obj)
        {
           object objExit = CacheClinet.Get(key);
           if (objExit == null)
            {
                CachaeAddObj(key, obj);
            }else
           {
               return objExit;
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
            object objExit = CacheClinet.Get(key);
            if (objExit == null)
            {
                AddObjectCache(key, obj, second);
            }
            else
            {
                return objExit;
            }
            return obj;
        }
        /// <summary>
        ///删除缓存
        /// </summary>
        /// <param name="key">关键字</param>
        public static void RemoveObjectCache(string key)
        {
            RemoveCache(key);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public static object GetObjectCache(string key)
        {
            return GetCacheObj(key);
        }
        #endregion



















        //private ReaderWriterLockSlim _itemsLock = new ReaderWriterLockSlim();
        //private static Hashtable _items = new Hashtable();

        //private object _cacheRefreshFrequencyLock = new object();
        //private TimeSpan _cacheRefreshFrequency = new TimeSpan(0, 0, 10);

        //private Timer _timer = null;

        //public TimeSpan CacheRefreshFrequency
        //{
        //    get
        //    {
        //        TimeSpan res;

        //        lock (_cacheRefreshFrequencyLock)
        //        {
        //            res = _cacheRefreshFrequency;
        //        }

        //        return res;
        //    }
        //    set
        //    {
        //        lock (_cacheRefreshFrequencyLock)
        //        {
        //            _cacheRefreshFrequency = value;
        //        }

        //        int refreshFrequencyMilliseconds = (int)CacheRefreshFrequency.TotalMilliseconds;
        //        this._timer.Change(refreshFrequencyMilliseconds, refreshFrequencyMilliseconds);
        //    }
        //}

        //public int CachedItemsNumber
        //{
        //    get
        //    {
        //        _itemsLock.EnterReadLock();
        //        try
        //        {
        //            return _items.Count;
        //        }
        //        finally
        //        {
        //            _itemsLock.ExitReadLock();
        //        }
        //    }
        //}

        //private CacheManager()
        //{
        //    int refreshFrequencyMilliseconds = (int)CacheRefreshFrequency.TotalMilliseconds;
        //    this._timer = new System.Threading.Timer(new
        //               TimerCallback(CacherefreshTimerCallback),
        //               null, refreshFrequencyMilliseconds, refreshFrequencyMilliseconds);
        //}

        //public static CacheManager Current
        //{
        //    get
        //    {
        //        return _current;
        //    }
        //}

        //public object this[object key]
        //{
          
        //    get
        //    {
        //        if (key == null)
        //            return null;
        //        _itemsLock.EnterUpgradeableReadLock();
        //        try
        //        {
        //            WCFCacheItem res = (WCFCacheItem)_items[key];
        //            if (res != null)
        //            {
        //                if (res.SlidingExpirationTime.TotalMilliseconds > 0)
        //                {
        //                    _itemsLock.EnterWriteLock();
        //                    try
        //                    {
        //                        res.LastAccessTime = DateTime.Now;
        //                    }
        //                    finally
        //                    {
        //                        _itemsLock.ExitWriteLock();
        //                    }
        //                }
        //                return res.ItemValue;
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //        finally
        //        {
        //            _itemsLock.ExitUpgradeableReadLock();
        //        }
        //    }
        //    set
        //    {
        //        _itemsLock.EnterWriteLock();
        //        try
        //        {
        //            _items[key] = new WCFCacheItem(value);
        //        }
        //        finally
        //        {
        //            _itemsLock.ExitWriteLock();
        //        }
        //    }
        //}

        //public void Insert(object key, object value)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}

        //public void Insert(object key, object value, DateTime expirationDate)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value, expirationDate);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}
        //public void Insert(object key, object value, int Minutes)
        //{
        //    DateTime expirationDate = DateTime.Now.AddMinutes(Minutes);
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value, expirationDate);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}
        //public void Insert(object key, object value, TimeSpan expirationTime)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value, expirationTime);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}

        //public void Insert(object key, object value, TimeSpan expirationTime, bool slidingExpiration)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value, expirationTime, slidingExpiration);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}

        //public void Insert(object key, object value, DateTime expirationDate, TimeSpan slidingExpirationTime)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items[key] = new WCFCacheItem(value, expirationDate, slidingExpirationTime);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}

        //public void Remove(object key)
        //{
        //    _itemsLock.EnterWriteLock();
        //    try
        //    {
        //        _items.Remove(key);
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitWriteLock();
        //    }
        //}
        //public void Clear()
        //{
        //    _itemsLock.EnterUpgradeableReadLock();
        //    try
        //    {
        //        Dictionary<object, WCFCacheItem> delItems = new Dictionary<object, WCFCacheItem>();
        //        DateTime dtNow = DateTime.Now;     
        //        if (delItems.Count > 0)
        //        {
        //            _itemsLock.EnterWriteLock();
        //            try
        //            {
        //                foreach (KeyValuePair<object, WCFCacheItem> kvp in delItems)
        //                {
        //                    if (_items.ContainsKey(kvp.Key))
        //                    {
        //                        _items.Remove(kvp.Key);
        //                    }
        //                }
        //            }
        //            finally
        //            {
        //                _itemsLock.ExitWriteLock();
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitUpgradeableReadLock();
        //    }
        //}


        //private void CacherefreshTimerCallback(object state)
        //{
        //    _itemsLock.EnterUpgradeableReadLock();
        //    try
        //    {
        //        Dictionary<object, WCFCacheItem> delItems = new Dictionary<object, WCFCacheItem>();
        //        DateTime dtNow = DateTime.Now;
        //        foreach (DictionaryEntry de in _items)
        //        {
        //            WCFCacheItem ci = (WCFCacheItem)de.Value;
        //            if (ci.ExpirationDate < dtNow)
        //            {
        //                delItems.Add(de.Key, ci);
        //            }
        //            else
        //            {
        //                if (ci.SlidingExpirationTime.TotalMilliseconds > 0)
        //                {
        //                    if (dtNow.Subtract(ci.LastAccessTime).TotalMilliseconds > ci.SlidingExpirationTime.TotalMilliseconds)
        //                    {
        //                        delItems.Add(de.Key, ci);
        //                    }
        //                }
        //            }
        //        }

        //        if (delItems.Count > 0)
        //        {
        //            _itemsLock.EnterWriteLock();
        //            try
        //            {
        //                foreach (KeyValuePair<object, WCFCacheItem> kvp in delItems)
        //                {
        //                    if (_items.ContainsKey(kvp.Key))
        //                    {
        //                        _items.Remove(kvp.Key);
        //                    }
        //                }
        //            }
        //            finally
        //            {
        //                _itemsLock.ExitWriteLock();
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        _itemsLock.ExitUpgradeableReadLock();
        //    }
        //}
    }
}
