using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
/*
 *缓存管理 
 * 
 * 
 */
namespace SMT.SaaS.BLLCommonServices
{
    public class CacheManager
    {
        //private static Hashtable SmtCache = Hashtable.Synchronized(new Hashtable());


        //public static object GetCache(string CacheKey)
        //{
        //    return CacheManager.SmtCache[CacheKey];
        //}

        //public static void AddCache(string CacheKey, object Entity)
        //{
        //    SmtCache.Add(CacheKey, Entity);
        //}


        //public static void RemoveCache(string CacheKey)
        //{
        //    SmtCache.Remove(CacheKey);
        //}

        private static CacheManager _current = new CacheManager();

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

        private CacheManager()
        {
            int refreshFrequencyMilliseconds = (int)CacheRefreshFrequency.TotalMilliseconds;
            this._timer = new System.Threading.Timer(new
                       TimerCallback(CacherefreshTimerCallback),
                       null, refreshFrequencyMilliseconds, refreshFrequencyMilliseconds);
        }

        public static CacheManager Current
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
