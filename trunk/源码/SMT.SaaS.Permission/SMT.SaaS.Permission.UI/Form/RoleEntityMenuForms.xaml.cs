using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.Saas.Tools.PermissionWS;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class RoleEntityMenuForms : UserControl, IEntityEditor
    {
        private T_SYS_ROLEENTITYMENU roleMenu;
        private string saveType = "0";       //保存方式 0:添加 1:关闭


        public T_SYS_ROLEENTITYMENU RoleMenu
        {
            get { return roleMenu; }
            set
            {
                roleMenu = value;
                this.DataContext = value;
            }
        }
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        protected PermissionServiceClient ServiceClient;
        //protected SysRoleManagementServiceClient RoleClient;

        public RoleEntityMenuForms(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            InitParas("");
        }
        public RoleEntityMenuForms(FormTypes type, string roleMenuID)
        {
            InitializeComponent();  
            FormType = type;
            InitParas(roleMenuID);
        }
        private void InitParas(string roleMenuID)
        {
            ServiceClient = new PermissionServiceClient();
            //RoleClient = new SysRoleManagementServiceClient();

            ServiceClient.RoleEntityMenuAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_RoleEntityMenuAddCompleted);            
            ServiceClient.RoleEntityMenuUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_RoleEntityMenuUpdateCompleted);
            
            ServiceClient.GetRoleEntityMenuByIDCompleted += new EventHandler<GetRoleEntityMenuByIDCompletedEventArgs>(ServiceClient_GetRoleEntityMenuByIDCompleted);
            //ServiceClient.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
            ServiceClient.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(ServiceClient_GetSysRoleInfosCompleted);
            
            if (FormType == FormTypes.New)
            {
                RoleMenu = new T_SYS_ROLEENTITYMENU();
                RoleMenu.ROLEENTITYMENUID = Guid.NewGuid().ToString();
                //绑定系统类型
                ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
            }

            //初始化权限菜单
            if (!string.IsNullOrEmpty(roleMenuID))
            {
                ServiceClient.GetRoleEntityMenuByIDAsync(roleMenuID);
            }


        }

        void ServiceClient_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            cbxRole.ItemsSource = null;

            if (e.Result != null)
            {
                //绑定权限名称
                List<T_SYS_ROLE> ents = e.Result.ToList();
                cbxRole.ItemsSource = ents;
                cbxRole.DisplayMemberPath = "ROLENAME";
                foreach (var item in cbxRole.Items)
                {
                    T_SYS_ROLE tmpRole = item as T_SYS_ROLE;
                    if (tmpRole != null && RoleMenu != null && RoleMenu.T_SYS_ROLE != null
                        && tmpRole.ROLEID == this.RoleMenu.T_SYS_ROLE.ROLEID)
                    {
                        cbxRole.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        void ServiceClient_GetRoleEntityMenuByIDCompleted(object sender, GetRoleEntityMenuByIDCompletedEventArgs e)
        {
            this.RoleMenu = e.Result;
            //绑定系统类型
            //ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");

            ServiceClient.GetSysMenuByTypeAsync("","");
            ServiceClient.GetSysRoleInfosAsync("","");
            
        }

        void ServiceClient_RoleEntityMenuUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void ServiceClient_RoleEntityMenuAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("ADDSUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }



        //void ServiceClient_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        //{
        //    cbxSysMenu.ItemsSource = null;

        //    if (e.Result != null)
        //    {
        //        //绑定菜单
        //        List<PermissionService.T_SYS_ENTITYMENU> ents = e.Result.ToList();
        //        cbxSysMenu.ItemsSource = ents;
        //        cbxSysMenu.DisplayMemberPath = "MENUNAME";

        //        foreach (var item in cbxSysMenu.Items)
        //        {
        //            PermissionService.T_SYS_ENTITYMENU tmpMenu = item as PermissionService.T_SYS_ENTITYMENU;
        //            if (tmpMenu != null && RoleMenu != null && RoleMenu.T_SYS_ENTITYMENU != null
        //                && tmpMenu.ENTITYMENUID == this.RoleMenu.T_SYS_ENTITYMENU.ENTITYMENUID)
        //            {
        //                cbxSysMenu.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }
        //}

        //void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        //{
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
        //        cbxSysType.ItemsSource = dicts;
        //        cbxSysType.DisplayMemberPath = "DICTIONARYNAME";

        //        if (SysPermMenu != null)
        //        {
        //            foreach (var item in cbxSysType.Items)
        //            { 
        //                T_SYS_DICTIONARY dict  = item as T_SYS_DICTIONARY;
        //                if (dict != null)
        //                {
        //                    if (SysPermMenu.T_SYS_PERMISSION!=null && dict.DICTIONARYVALUE == SysPermMenu.T_SYS_PERMISSION.SYSTEMTYPE)
        //                    {
        //                        cbxSysType.SelectedItem = item;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void cbxSysType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbxSysType.SelectedItem != null)
            //{
            //    T_SYS_DICTIONARY dict = cbxSysType.SelectedItem as T_SYS_DICTIONARY;
            //    if(dict!=null)
            //    {
            //        ServiceClient.GetSysMenuByTypeAsync(dict.DICTIONARYVALUE);
            //        ServiceClient.FindSysPermissionByTypeAsync(dict.DICTIONARYVALUE);
            //    }
            //}

        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "";
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    AddToClose();
                    break;
                case "1":
                    Close();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "确定并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        public void AddToClose()
        {
            if (cbxRole.SelectedItem == null)
            {
                RoleMenu.T_SYS_ROLE = null;
            }
            else
            {
                RoleMenu.T_SYS_ROLE = new T_SYS_ROLE();
                var role = cbxRole.SelectedItem as T_SYS_ROLE;

                RoleMenu.T_SYS_ROLE.ROLEID = role.ROLEID;
                RoleMenu.T_SYS_ROLE.ROLENAME = role.ROLENAME;
            }
            
            //RoleMenu.T_SYS_ENTITYMENU = cbxSysMenu.SelectedItem as PermissionService.T_SYS_ENTITYMENU;

            if (FormTypes.New == this.FormType)
            {
                RoleMenu.CREATEDATE = System.DateTime.Now;
                //RoleMenu.CREATEUSER = Common.CurrentLoginUserInfo.sysuserID;
                RoleMenu.CREATEUSER = Common.CurrentLoginUserInfo.SysUserID;

                ServiceClient.RoleEntityMenuAddAsync(this.RoleMenu);
            }
            else
            {
                RoleMenu.UPDATEDATE = System.DateTime.Now;
                //RoleMenu.UPDATEUSER = Common.CurrentLoginUserInfo. //Common.CurrentConfig.CurrentUser.SYSUSERID;
                ServiceClient.RoleEntityMenuUpdateAsync(this.RoleMenu);
            }
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }

        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }
        private void lkParentMenu_FindClick(object sender, EventArgs e)
        {
            MenuLookupForm lookup = new MenuLookupForm("");


            lookup.SelectedClick += (obj, ev) =>
            {
                lkParentMenu.DataContext = lookup.SelectedObj;
            };
            lookup.Show();
        }
    }
}
