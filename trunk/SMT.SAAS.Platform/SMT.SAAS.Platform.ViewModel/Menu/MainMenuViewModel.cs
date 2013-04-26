using System;
using System.Linq;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Core.Modularity;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 主菜单VIEWMODEL,用户菜单数据在首次读取后将会被缓存。
// 完成日期：2011-04-21  
// 版    本：V1.0 
// 作    者：GaoY
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.ViewModel.Menu
{
    /// <summary>
    /// 主菜单VIEWMODEL,用户菜单数据在首次读取后将会被缓存。
    /// 菜单数据将在用户注销、重新登录后才会获取到最新的菜单数据。
    /// </summary>
    public class MainMenuViewModel : Foundation.BasicViewModel
    {
        private Model.Services.CommonServices _services = null;

        public MainMenuViewModel()
        {
            InitUserMenu();
        }

        private void InitUserMenu()
        {
            if (Context.CacheMenu == null)
            {
                // _services = new Model.Services.CommonServices();
                // _services.OnGetUserMenuCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.UserMenu>>(_services_OnGetUserMenuCompleted);
                GetUserMenu();
            }
            else
            {
                this.Item = Context.CacheMenu;
            }
        }

        private ObservableCollection<MenuViewModel> _item = new ObservableCollection<MenuViewModel>();

        public ObservableCollection<MenuViewModel> Item
        {
            get { return _item; }
            set
            {
                SetValue(ref _item, value, "Item");
            }
        }

        private void GetUserMenu()
        {
            try
            {
                List<ModuleInfo> catalog = Context.Managed.Catalog;

                ObservableCollection<MenuViewModel> result = new ObservableCollection<MenuViewModel>();
                var sysMenulist = catalog.GroupBy(x => new { x.SystemType });

                foreach (var systemMenu in sysMenulist)
                {
                    if (systemMenu.Key.SystemType != null)
                    {
                        ModuleInfo parentModule = catalog.FirstOrDefault(e => e.ModuleCode == systemMenu.Key.SystemType);

                        if (parentModule != null)
                        {
                            MenuViewModel menu = new MenuViewModel();
                            menu.MenuName = parentModule.Description;
                            menu.Content = parentModule;

                            ObservableCollection<MenuViewModel> item = new ObservableCollection<MenuViewModel>();
                            var moduleMenuList = systemMenu.ToList();
                            
                            moduleMenuList.GroupBy(moduleItem => new { moduleItem.ParentModuleID });
                            
                            foreach (var tempMenu in systemMenu.ToList())
                            {
                                MenuViewModel menuItem = new MenuViewModel();
                                menuItem.MenuName = tempMenu.Description;
                                menuItem.MenuIconPath = tempMenu.ModuleIcon;
                                menuItem.Content = tempMenu;                                
                                menuItem.MenuID = tempMenu.ModuleID;

                                item.Add(menuItem);
                            }
                            menu.Item = item;

                            result.Add(menu);
                        }
                    }
                }
                Context.CacheMenu = result;
                this.Item = result;
            }
            catch (Exception ex)
            {

            }
        }

        public ObservableCollection<MenuViewModel> GetAllMenu()
        {
            try
            {
                if (Context.CacheAllMenu != null)
                {
                    return Context.CacheAllMenu;
                }
                return new ObservableCollection<MenuViewModel>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取系统模块相关信息
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<MenuViewModel> GetSystemMenu()
        {
            try
            {
                if (Context.CacheSystemMenu == null)
                {

                    List<ModuleInfo> catalog = Context.Managed.Catalog;

                    ObservableCollection<MenuViewModel> result = new ObservableCollection<MenuViewModel>();

                    ObservableCollection<MenuViewModel> allItem = new ObservableCollection<MenuViewModel>();

                    var sysMenulist = catalog.OrderBy(c=>c.SystemType).GroupBy(x => new { x.SystemType });

                    foreach (var systemMenu in sysMenulist)
                    {
                        if (systemMenu.Key.SystemType != null)
                        {
                            ModuleInfo parentModule = catalog.FirstOrDefault(e => e.ModuleCode == systemMenu.Key.SystemType);

                            if (parentModule != null)
                            {
                                MenuViewModel sysMenu = new MenuViewModel();
                                sysMenu.MenuName = parentModule.Description;
                                sysMenu.MenuID = parentModule.ModuleID;
                                sysMenu.Content = parentModule;

                                var moduleMenuList = from moduleitem in systemMenu.ToList()
                                                     where moduleitem.ParentModuleID == parentModule.ModuleCode
                                                     select moduleitem;
                                if (moduleMenuList.Count() > 0)
                                {
                                    ObservableCollection<MenuViewModel> item = new ObservableCollection<MenuViewModel>();

                                    foreach (var moduleItem in moduleMenuList)
                                    {
                                        MenuViewModel menuItem = new MenuViewModel();
                                        menuItem.MenuName = moduleItem.Description;
                                        menuItem.MenuIconPath = moduleItem.ModuleIcon;
                                        menuItem.Content = moduleItem;
                                        menuItem.MenuID = moduleItem.ModuleID;

                                        foreach (var aitem in GetChildMenu(menuItem.MenuID))
                                        {
                                            allItem.Add(aitem);
                                        }

                                        item.Add(menuItem);
                                    }

                                    sysMenu.Item = item;

                                    result.Add(sysMenu);
                                }
                            }
                        }
                    }
                    Context.CacheSystemMenu = result;
                    Context.CacheAllMenu = allItem;
                    return result;
                }
                else
                {
                    return Context.CacheSystemMenu;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ObservableCollection<MenuViewModel> GetChildMenu(string parentMenuid)
        {
            List<ModuleInfo> catalog = Context.Managed.Catalog;
            var moduleMenuList = from moduleitem in catalog
                                 where moduleitem.ParentModuleID == parentMenuid
                                 select moduleitem;
            ObservableCollection<MenuViewModel> item = new ObservableCollection<MenuViewModel>();

            foreach (var moduleItem in moduleMenuList)
            {
                MenuViewModel menuItem = new MenuViewModel();
                menuItem.MenuName = moduleItem.Description;
                menuItem.MenuIconPath = moduleItem.ModuleIcon;
                menuItem.Content = moduleItem;
                menuItem.MenuID = moduleItem.ModuleID;

                item.Add(menuItem);
            }
            return item;
        }

        void _services_OnGetUserMenuCompleted(object sender, Model.GetEntityListEventArgs<Model.UserMenu> e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    AnalysisMenu(e.Result);
                    _services = null;
                }
            }
        }

        private void AnalysisMenu(ObservableCollection<Model.UserMenu> menuList)
        {
            //ObservableCollection<MenuViewModel> result = new ObservableCollection<MenuViewModel>();
            //var sysMenulist = menuList.GroupBy(x => new { x.SystemType });

            //foreach (var systemMenu in sysMenulist)
            //{
            //    MenuViewModel menu = new MenuViewModel();
            //    ModuleInfo info = null;
            //    if (Context.SystemInfo.ContainsKey(systemMenu.Key.SystemType))
            //    {
            //        info = Context.SystemInfo[systemMenu.Key.SystemType];
            //        menu.MenuName = info.ModuleName;
            //    }

            //    ObservableCollection<MenuViewModel> item = new ObservableCollection<MenuViewModel>();
            //    foreach (var tempMenu in systemMenu.ToList())
            //    {
            //        MenuViewModel menuItem = tempMenu.CloneObject<MenuViewModel>(new MenuViewModel());
            //        if (info != null)
            //            menuItem.SystemType = info.EnterAssembly;

            //        Refresh(menuItem);

            //        item.Add(menuItem);

            //    }
            //    menu.Item = item;

            //    result.Add(menu);
            //}
            //Context.CacheMenu = result;
            //this.Item = result;
        }

        /// <summary>
        /// 从新刷新每个菜单的所属类型。此处数据原本是由权限系统完整的提供
        /// 包含的基本信息为：AssemblyQualifiedName，InitParams
        /// 后续要考虑 系统 与 模块之间的关系 如何使用权限系统 结合平台进行统一控制。
        /// 平台中的所有系统以及 模块信息  与权限系统是相同的。
        /// 授权机制 与 访问模块控制均要由权限系统提供基础数据的支持
        /// </summary>
        /// <param name="menu"></param>
        private void Refresh(MenuViewModel menu)
        {
            //string temp = " , Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            //string assemblyName = "";
            //StringBuilder typeName = new StringBuilder();
            //if (menu.UrlAddress != null)
            //{
            //    assemblyName = menu.SystemType.Length <= 0 ? "" : menu.SystemType;

            //    if (menu.ChildSystemName != null)
            //    {
            //        assemblyName = menu.ChildSystemName;
            //    }

            //    if (assemblyName != "SMT.FB.UI")
            //    {
            //        if (assemblyName == "SMT.EDM.UI")
            //        {
            //            #region 处理进销存

            //            string tempUri = "";

            //            string typetemp = menu.UrlAddress;
            //            if (typetemp.Contains("?"))
            //            {
            //                string[] partUri = typetemp.Split('?');
            //                tempUri = partUri[0].Replace('\\', '.');
            //                tempUri = tempUri.Replace('/', '.');
            //                Dictionary<string, string> initparam = new Dictionary<string, string>();
            //                if (partUri.Length > 1)
            //                {
            //                    string[] parames = partUri[1].Split(',');
            //                    if (parames.Length > 0)
            //                    {
            //                        foreach (var item in parames)
            //                        {
            //                            string[] param = item.Split('=');
            //                            initparam.Add(param[0], param[1]);
            //                        }
            //                    }
            //                    menu.InitParams = initparam;
            //                }
            //            }
            //            else
            //            {
            //                tempUri = menu.UrlAddress.Replace('\\', '.');
            //                tempUri = tempUri.Replace('/', '.');
            //            }

            //            typeName.Append(assemblyName);
            //            typeName.Append(".Views");
            //            typeName.Append(tempUri);
            //            typeName.Append(", ");
            //            typeName.Append(assemblyName);
            //            typeName.Append(temp);


            //            menu.ModuleType = typeName.ToString();
            //            #endregion
            //        }
            //        else
            //        {
            //            #region 正常处理

            //            string defaultName = ".Views";
            //            if (assemblyName == "SMT.FlowDesigner")
            //                defaultName = ".BMP";

            //            string newaddress = menu.UrlAddress.Replace("/", ".");

            //            typeName.Append(assemblyName);
            //            typeName.Append(defaultName);
            //            typeName.Append(newaddress);
            //            typeName.Append(", ");
            //            typeName.Append(assemblyName);
            //            typeName.Append(temp);

            //            menu.ModuleType = typeName.ToString();

            //            #endregion
            //        }
            //    }
            //    else
            //    {
            //        #region 处理预算

            //        string typetemp = menu.UrlAddress;

            //        string[] partUri = typetemp.Split('?');
            //        typeName.Append(assemblyName);
            //        typeName.Append(".Views");
            //        typeName.Append(partUri[0].Replace('/', '.'));
            //        typeName.Append(", ");
            //        typeName.Append(assemblyName);
            //        typeName.Append(temp);

            //        menu.ModuleType = typeName.ToString();

            //        Dictionary<string, string> initparam = new Dictionary<string, string>();
            //        if (partUri.Length > 1)
            //        {
            //            string[] parames = partUri[1].Split(',');
            //            if (parames.Length > 0)
            //            {
            //                foreach (var item in parames)
            //                {
            //                    string[] param = item.Split('=');
            //                    initparam.Add(param[0], param[1]);
            //                }
            //            }
            //            menu.InitParams = initparam;
            //        }


            //        #endregion
            //    }
            //}
        }
    }
}
