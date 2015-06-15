
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core.Modularity;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Xamls.LoginPart;
using SMT.Saas.Tools.PermissionWS;
using System.Reflection;

// 内容摘要: 上下文环境，存储平台中使用的共享数据。
      
namespace SMT.SAAS.Platform.ViewModel
{
    /// <summary>
    /// 上下文环境，存储全局共享数据和托管程序。
    /// 在用户登录成功后要对此上下文进行初始化。
    /// 目前为平台级别的共享，将移植到 应用程序共享中。
    /// </summary>
    public class Context
    {


        /// <summary>
        /// 主容器. 对登录面板、主面板等进行控制显示。
        /// </summary>
        //public static IHost Host;

        /// <summary>
        /// 主容器. 对登录面板、主面板等进行控制显示。
        /// </summary>
        public static IMainPanel MainPanel;

        /// <summary>
        /// 托管程序. 对系统中的加载管理以及
        /// </summary>
        public static ChildSystemManager childSystemManager=new ChildSystemManager();

        ///// <summary>
        ///// 缓存用户菜单数据。
        ///// </summary>
        public static List<V_UserMenuPermission> CacheMenuAll=new List<V_UserMenuPermission>();


        public static Dictionary<string, Assembly> allChildSystemAssembly=new Dictionary<string,Assembly>();

        ///// 缓存用户菜单数据。
        ///// </summary>
        public static List<T_SYS_DICTIONARY> CacheSystemType=new List<T_SYS_DICTIONARY>();

        ///// 缓存用户Permission
        public static List<string> CacheMenuPermissionList=new List<string>();

        public static void Clear()
        {
             
            MainPanel = null;
            CacheMenuPermissionList = new List<string>();
        }

       
    }
}
