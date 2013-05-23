using System.Windows;

namespace SMT.SAAS.Main.CurrentContext
{
    /// <summary>
    /// 本地内存客户端管理类
    /// </summary>
    public class UICache
    {
        public UICache()
        {
        }
        /// <summary>
        /// 添加客户端内存缓存
        /// </summary>
        /// <param name="strCacheKey">缓存的Key</param>
        /// <param name="source">缓存的数据</param>
        /// <param name="iTime">缓存的时长</param>
        public static void CreateCache(string strCacheKey, object source)
        {
            if (Application.Current.Resources.Contains(strCacheKey))
            {
                RemoveCache(strCacheKey);
            }

            Application.Current.Resources.Add(strCacheKey, source);
        }
        /// <summary>
        /// 获取客户端内存缓存
        /// </summary>
        /// <param name="strCacheKey">缓存的对象key值</param>
        /// <returns>返回缓存的对象</returns>
        public static object GetObjectFromClinetMemorryCache(string strCacheKey)
        {
            if (Application.Current.Resources.Contains(strCacheKey))
            {
                return Application.Current.Resources[strCacheKey];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 移除客户端缓存
        /// </summary>
        /// <param name="strCacheKey"></param>
        public static void RemoveCache(string strCacheKey)
        {
            //Application.Current.Resources[strCacheKey] = null;
            Application.Current.Resources.Remove(strCacheKey);
        }

    }
}
