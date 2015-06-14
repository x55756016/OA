using System;
using System.Linq;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Core.Modularity;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

// 内容摘要: 主菜单VIEWMODEL,用户菜单数据在首次读取后将会被缓存。

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
            if (Context.CacheMenu == null || Context.CacheMenu.Count==0)
            {
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
                        ModuleInfo parentModule = catalog.FirstOrDefault(e => e.SystemType == systemMenu.Key.SystemType);

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



        void _services_OnGetUserMenuCompleted(object sender, Model.GetEntityListEventArgs<Model.UserMenu> e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                   // AnalysisMenu(e.Result);
                    _services = null;
                }
            }
        }

        //private void AnalysisMenu(ObservableCollection<Model.UserMenu> menuList)
        //{
        //    ObservableCollection<MenuViewModel> result = new ObservableCollection<MenuViewModel>();
        //    var sysMenulist = menuList.GroupBy(x => new { x.SystemType });

        //    foreach (var systemMenu in sysMenulist)
        //    {
        //        MenuViewModel menu = new MenuViewModel();
        //        ModuleInfo info = null;
        //        if (Context.SystemInfo.ContainsKey(systemMenu.Key.SystemType))
        //        {
        //            info = Context.SystemInfo[systemMenu.Key.SystemType];
        //            menu.MenuName = info.ModuleName;
        //        }

        //        ObservableCollection<MenuViewModel> item = new ObservableCollection<MenuViewModel>();
        //        foreach (var tempMenu in systemMenu.ToList())
        //        {
        //            MenuViewModel menuItem = tempMenu.CloneObject<MenuViewModel>(new MenuViewModel());
        //            if (info != null)
        //                menuItem.SystemType = info.EnterAssembly;

        //            Refresh(menuItem);

        //            item.Add(menuItem);

        //        }
        //        menu.Item = item;

        //        result.Add(menu);
        //    }
        //    Context.CacheMenu = result;
        //    this.Item = result;
        //}

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
        }
    }
}
