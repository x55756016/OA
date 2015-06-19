
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core.Modularity;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Xamls.LoginPart;
using SMT.Saas.Tools.PermissionWS;
using System.Reflection;
using System.Windows;

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


        private static ChildSystemLoadManager childsystemM;
        /// <summary>
        /// 托管程序. 对系统中的加载管理以及
        /// </summary>
        public static ChildSystemLoadManager childSystemManager
        {
            get
            {
                if (childsystemM == null)
                {
                    childsystemM = new ChildSystemLoadManager();
                }
                return childsystemM;
            }
        }

        private static List<V_UserMenuPermission> MenuAll;
        ///// <summary>
        ///// 缓存用户菜单数据。
        ///// </summary>
        public static List<V_UserMenuPermission> CacheMenuAll{
            get
            {
                if (MenuAll == null)
                {
                    MenuAll = new List<V_UserMenuPermission>();
                }
                return MenuAll;
            }
            set
            {
                MenuAll = value;
            }
        }

        private static Dictionary<string, Assembly> allChildAssembly;
        public static Dictionary<string, Assembly> allChildSystemAssembly {
            get
            {
                if (allChildAssembly == null)
                {
                    allChildAssembly = new  Dictionary<string,Assembly>();
                }
                return allChildAssembly;
            }
        }


        private static Dictionary<string, FrameworkElement> MenuUIElement;
        /// <summary>
        /// 缓存的所有已经初始化过的菜单页面
        /// </summary>
        public static Dictionary<string, FrameworkElement> CacheMenuUIElement
        {
            get
            {
                if (MenuUIElement == null)
                {
                    MenuUIElement = new Dictionary<string, FrameworkElement>();
                }
                return MenuUIElement;
            }
        }

        private static List<T_SYS_DICTIONARY> CacheSystem;
        ///// 缓存用户菜单数据。
        ///// </summary>
        public static List<T_SYS_DICTIONARY> CacheSystemType {
            get
            {
                if (CacheSystem == null)
                {
                    CacheSystem = new  List<T_SYS_DICTIONARY>();
                }
                return CacheSystem;
            }
            set
            {
                CacheSystem = value;
            }
        }

        ///// 缓存用户Permission
        public static List<string> CacheMenuPermissionList { get; set; }

        public static void Clear()
        {
             
            MainPanel = null;
            CacheMenuPermissionList = new List<string>();
        }

       
    }
}
