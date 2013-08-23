using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.Workflow.SMTCache
{
    public enum AutoCache
    {
        Auto, UnAuto
    }
   
    public class CacheEntity<TKey, TValue>
    {
        /// <summary>
        /// 缓存主键值
        /// </summary>
        private string cacheKey;
        /// <summary>
        /// 缓存过期时间(分钟)
        /// </summary>
        public int CacheMinutes
        {
            get
            {
                try
                {
                    int iCache = 0;
                    if (iCache == 0)
                    {
                        return 60;
                    }
                    else
                    {
                        return iCache;
                    }
                }
                catch
                {
                    return 60;
                }
            }

        }

        public TValue this[TKey index]
        {
            get
            {
                Dictionary<TKey, CacheObject<TValue>> dicCached = GetCachedEntities();
                if (dicCached.ContainsKey(index))
                {
                    return (TValue)dicCached[index].Object;
                }
                else
                {
                    return default(TValue);
                }
            }
        }
        public CacheEntity(string cacheKey)
        {
            this.cacheKey = cacheKey;
        }
        /// <summary>
        /// 获取缓存键值下的数据字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<TKey, CacheObject<TValue>> GetCachedEntities()
        {
            Dictionary<TKey, CacheObject<TValue>> cachedEntities = CacheOperate.Get(cacheKey) as Dictionary<TKey, CacheObject<TValue>>;
            if (cachedEntities == null)
            {
                cachedEntities = new Dictionary<TKey, CacheObject<TValue>>();
                CacheOperate.Insert(cacheKey, cachedEntities);
            }
            return cachedEntities;
        }
        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="entity">缓存对象</param>
        public void Cache(TKey key, TValue entity)
        {
            try
            {
                Cache(key, entity, null, System.Web.Caching.CacheItemPriority.Default);
            }
            catch
            {
                throw new Exception("Cache Insert Error");
            }
        }
        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="entity">缓存对象</param>
        /// <param name="cachedp">缓存依赖项</param>
        /// <param name="priority">缓存清理优先级</param>
        public void Cache(TKey key, TValue entity, System.Web.Caching.CacheDependency cachedp, System.Web.Caching.CacheItemPriority priority)
        {
            if (entity == null)
                return;
            Dictionary<TKey, CacheObject<TValue>> cachedEntities = GetCachedEntities();
            lock (cachedEntities)
            {
                CacheObject<TValue> cachedEntity = new CacheObject<TValue>(entity);
                if (cachedEntities.ContainsKey(key))
                {
                    cachedEntities[key] = cachedEntity;
                }
                else
                {
                    cachedEntities.Add(key, cachedEntity);

                }
                CacheOperate.Insert(cacheKey, cachedEntities, cachedp, CacheMinutes, priority);
            }
        }
        private bool CheckCycle(DateTime createtime)
        {
            TimeSpan timespan = DateTime.Now - createtime;
            if (timespan.Minutes < (CacheMinutes / 2))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取缓存，根据周期自动延长缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key, AutoCache autocache)
        {
            Dictionary<TKey, CacheObject<TValue>> cachedEntities = GetCachedEntities();
            CacheObject<TValue> cachedEntity;
            if (cachedEntities.ContainsKey(key))
            {
                cachedEntity = cachedEntities[key];
                if (autocache == AutoCache.Auto)
                {
                    if (CheckCycle(cachedEntity.CreateTime))
                    {
                        CacheOperate.Insert(cacheKey, CacheMinutes, null, CacheMinutes, System.Web.Caching.CacheItemPriority.Default);
                    }
                }
                return cachedEntity.Object;
            }
            return default(TValue);
        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            try
            {
                Dictionary<TKey, CacheObject<TValue>> cachedEntities = GetCachedEntities();
                CacheObject<TValue> cachedEntity;
                if (cachedEntities.ContainsKey(key))
                {
                    cachedEntity = cachedEntities[key];
                    return cachedEntity.Object;
                }
                return default(TValue);
            }
            catch
            {

            }
            return default(TValue);
        }
        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            Dictionary<TKey, CacheObject<TValue>> cachedEntities = GetCachedEntities();
            lock (cachedEntities)
            {
                cachedEntities.Remove(key);
            }
        }
        /// <summary>
        /// 移除缓存键
        /// </summary>
        public void Remove()
        {
            CacheOperate.Remove(cacheKey);
        }
        /// <summary>
        /// 获取缓存总数(CacheKeys)
        /// </summary>
        /// <returns></returns>
        public int GetLength
        {
            get
            {
                Dictionary<TKey, CacheObject<TValue>> cachedEntities = GetCachedEntities();
                return cachedEntities.Keys.Count;
            }
        }
    }
}
