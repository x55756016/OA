
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core.Modularity;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.ViewModel.Menu;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 上下文环境，存储平台中使用的共享数据。
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
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
        /// 用于标识用户是否登录系统
        /// </summary>
        public static bool LoginFlag=false;

        /// <summary>
        /// 主容器. 对登录面板、主面板等进行控制显示。
        /// </summary>
        public static IHost Host;

        /// <summary>
        /// 主容器. 对登录面板、主面板等进行控制显示。
        /// </summary>
        public static IMainPanel MainPanel;

        /// <summary>
        /// 托管程序. 对系统中的加载管理以及
        /// </summary>
        public static Managed Managed;

        /// <summary>
        /// 暂存，登录用户数据 ，目前为平台级别的共享，将移植到 应用程序共享中。
        /// </summary>
        //public static UserLogin LoginUser;

        /// <summary>
        /// 暂存，系统信息，当前登录用户所具备的系统信息。
        /// </summary>
        public static Dictionary<string,SMT.SAAS.Platform.Core.Modularity.ModuleInfo> SystemInfo;

        /// <summary>
        /// 缓存用户菜单数据。
        /// </summary>
        public static ObservableCollection<MenuViewModel> CacheMenu;

        /// 缓存用户菜单数据。
        /// </summary>
        public static ObservableCollection<MenuViewModel> CacheSystemMenu;

        /// 缓存用户菜单数据。
        /// </summary>
        public static ObservableCollection<MenuViewModel> CacheAllMenu;

        public static List<string> CacheMenuPermissionList = new List<string>();

        public static void Clear()
        {
             
            MainPanel = null;
            Managed = null;
            //SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = null;
            SystemInfo = null;
            CacheMenu = null;
            CacheMenuPermissionList = new List<string>();
        }

       
    }
}
