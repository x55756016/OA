using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.DataAccess;

namespace SMT.Workflow.SMTCache
{
    public static class TaskCache
    {
        //待办自动清理时间
        public static int TaskCacheTime
        {
            get
            {
                try
                {
                    return 10;
                }
                catch
                {
                    return 10;
                }
            }
        }
        public const string CacheKey = "SMT-ENGINE-TASK-CACHE";
        public static CacheEntity<string, TaskCacheEntity> cacheEntity = new CacheEntity<string, TaskCacheEntity>(CacheKey);
        /// <summary>
        /// 待办任务缓存刷新
        /// </summary>
        /// <param name="entity"></param>
        public static void TaskCacheReflesh(string strUserID)
        {
            try
            {
                TaskCacheEntity cacheObj = cacheEntity.Get(strUserID);
                if (cacheObj == null)//缓存中不存在用户信息
                {
                    cacheObj = new TaskCacheEntity();
                }
                cacheObj.FonctionTask = true;
                cacheObj.FonctionTaskPage = true;
                cacheObj.LastFreshTime = DateTime.Now;
                cacheEntity.Cache(strUserID, cacheObj);
                //Logs.WriteLog("待办缓存状态TaskCacheReflesh（）：" + strUserID + "||FonctionTask" + cacheObj.FonctionTask + "||FonctionTaskPage" + cacheObj.FonctionTaskPage);
            }
            catch (Exception ex)
            {
                Logs.WriteLog("待办缓存出现异常", ex.Message);
            }
        }
        /// <summary>
        /// 获取当前用户是否有新的待办
        /// </summary>
        /// <param name="strUserID"></param>
        /// <returns></returns>
        public static bool CurrentUserTaskStatus(string strUserID,bool isPage)
        {
            try
            {
                TaskCacheEntity cacheObj = cacheEntity.Get(strUserID);             
                if (cacheObj == null)//如果该用户没有缓存信息，增加新的缓存信息
                {
                    cacheObj = new TaskCacheEntity();
                    cacheObj.FonctionTask = true;
                    cacheObj.FonctionTaskPage = true;
                    cacheObj.LastFreshTime = DateTime.Now;
                    cacheEntity.Cache(strUserID, cacheObj);                 
                    return true;
                }
                else
                {
                    if (isPage)
                    {
                        //如果缓存用户没有新的待办任务，计算变更时间与系统时间
                        if (!cacheObj.FonctionTaskPage)
                        {
                            TimeSpan sp = DateTime.Now - cacheObj.LastFreshTime;
                            if (sp.Minutes > TaskCacheTime)
                            {
                                cacheObj.FonctionTaskPage = true;
                                cacheObj.LastFreshTime = DateTime.Now;
                                cacheEntity.Cache(strUserID, cacheObj);
                            }
                            return cacheObj.FonctionTaskPage;
                        }
                        else//存在缓存用户信息，自动刷新后变更
                        {                          
                            cacheObj.FonctionTaskPage = false;
                            cacheEntity.Cache(strUserID, cacheObj);
                            return true;
                        }
                    }
                    else
                    {
                        //如果缓存用户没有新的待办任务，计算变更时间与系统时间
                        if (!cacheObj.FonctionTask)
                        {
                            TimeSpan sp = DateTime.Now - cacheObj.LastFreshTime;
                            if (sp.Minutes > TaskCacheTime)
                            {
                                cacheObj.FonctionTask = true;
                                cacheObj.LastFreshTime = DateTime.Now;
                                cacheEntity.Cache(strUserID, cacheObj);
                            }
                            return cacheObj.FonctionTask;
                        }
                        else//存在缓存用户信息，自动刷新后变更
                        {
                            cacheObj.FonctionTask = false;                          
                            cacheEntity.Cache(strUserID, cacheObj);
                            return true;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Logs.WriteLog("待办缓存出现异常", ex.Message);
                return true;
            }
        }
        /// <summary>
        /// 列出所有已缓存对象
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, CacheObject<TaskCacheEntity>> ListCacheInfo()
        {
            Dictionary<string, CacheObject<TaskCacheEntity>> list = cacheEntity.GetCachedEntities();
            return list;
        }
        /// <summary>
        /// 删除所有缓存
        /// </summary>
        public static void RemoveAllCache()
        {         
            cacheEntity.Remove();
        }
        /// <summary>
        /// 删除用户缓存
        /// </summary>
        /// <param name="strUserID"></param>
        public static void RemoveCache(string strUserID)
        {         
            if (cacheEntity.Get(strUserID) != null)            
            cacheEntity.Remove(strUserID);
        }
    }
}
