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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysPermMenuForms : UserControl, IEntityEditor
    {
        private string saveType = "0";       //保存方式 0:添加 1:关闭
        //private T_SYS_PERM_MENU sysPermMenu;

        //public T_SYS_PERM_MENU SysPermMenu
        //{
        //    get { return sysPermMenu; }
        //    set 
        //    { 
        //        sysPermMenu = value;
        //        this.DataContext = value;                 
        //    }
        //}
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        protected PermissionServiceClient ServiceClient;

        public SysPermMenuForms(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            //InitParas("");
        }
        public SysPermMenuForms(FormTypes type, string permMenuID)
        {
            InitializeComponent();  
            FormType = type;
            //InitParas(permMenuID);
        }
        //private void InitParas(string permMenuID)
        //{
        //    ServiceClient = new PermissionServiceClient();
        //    ServiceClient.SysPermMenuAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysPermMenuAddCompleted);
        //    ServiceClient.SysPermMenuUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysPermMenuUpdateCompleted);
        //    ServiceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
        //    ServiceClient.GetSysPermMenuByIDCompleted += new EventHandler<GetSysPermMenuByIDCompletedEventArgs>(ServiceClient_GetSysPermMenuByIDCompleted);
        //    ServiceClient.GetSysMenuByTypeCompleted += new EventHandler<GetSysMenuByTypeCompletedEventArgs>(ServiceClient_GetSysMenuByTypeCompleted);
        //    ServiceClient.FindSysPermissionByTypeCompleted += new EventHandler<FindSysPermissionByTypeCompletedEventArgs>(ServiceClient_FindSysPermissionByTypeCompleted);

        //    if (FormType == FormTypes.New)
        //    {
        //        SysPermMenu = new T_SYS_PERM_MENU();
        //        SysPermMenu.ID = Guid.NewGuid().ToString();
        //        //绑定系统类型
        //        ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //    }

        //    //初始化权限菜单
        //    if (!string.IsNullOrEmpty(permMenuID))
        //    {
        //        ServiceClient.GetSysPermMenuByIDAsync(permMenuID);
        //    }


        //}

        //void ServiceClient_FindSysPermissionByTypeCompleted(object sender, FindSysPermissionByTypeCompletedEventArgs e)
        //{
        //    cbxSysPerm.ItemsSource = null;

        //    if (e.Result != null)
        //    {
        //        //绑定权限名称
        //        List<T_SYS_PERMISSION> ents = e.Result.ToList();
        //        cbxSysPerm.ItemsSource = ents;
        //        cbxSysPerm.DisplayMemberPath = "PERMISSIONNAME";
        //        foreach (var item in cbxSysPerm.Items)
        //        {
        //            T_SYS_PERMISSION tmpPerm = item as T_SYS_PERMISSION;
        //            if (tmpPerm!=null && SysPermMenu!=null && SysPermMenu.T_SYS_PERMISSION!=null
        //                &&tmpPerm.PERMISSIONID == this.SysPermMenu.T_SYS_PERMISSION.PERMISSIONID)
        //            {
        //                cbxSysPerm.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }
        //}

        //void ServiceClient_GetSysMenuByTypeCompleted(object sender, GetSysMenuByTypeCompletedEventArgs e)
        //{
        //    cbxSysMenu.ItemsSource = null;

        //    if (e.Result != null)
        //    {
        //        //绑定菜单
        //        List<T_SYS_MENU> ents = e.Result.ToList();
        //        cbxSysMenu.ItemsSource = ents;
        //        cbxSysMenu.DisplayMemberPath = "MENUNAME";

        //        foreach (var item in cbxSysMenu.Items)
        //        {
        //            T_SYS_MENU tmpMenu = item as T_SYS_MENU;
        //            if (tmpMenu != null && SysPermMenu != null && SysPermMenu.T_SYS_MENU != null
        //                && tmpMenu.MENUID == this.SysPermMenu.T_SYS_MENU.MENUID)
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

        //void ServiceClient_GetSysPermMenuByIDCompleted(object sender, GetSysPermMenuByIDCompletedEventArgs e)
        //{
        //    this.SysPermMenu = e.Result;
        //    //绑定系统类型
        //    ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //}

        //void ServiceClient_SysPermMenuUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        MessageBox.Show(Utility.GetResourceStr("MODIFYSUCCESSED"));
        //        this.ReloadData();
        //        this.Close();
        //    }
        //}

        //void ServiceClient_SysPermMenuAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        MessageBox.Show(Utility.GetResourceStr("ADDSUCCESSED",""));
        //        this.ReloadData();
        //        this.Close();
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
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png"
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
            ////T_SYS_PERM_MENU tmpMenu = cbxParentMenu.DataContext as T_SYS_PERM_MENU;
            ////if (tmpMenu != null)
            ////{
            ////    SysMenu. = tmpMenu.T_SYS_MENU2;
            ////}
            //SysPermMenu.T_SYS_PERMISSION = cbxSysPerm.SelectedItem as T_SYS_PERMISSION;
            //SysPermMenu.T_SYS_MENU = cbxSysMenu.SelectedItem as T_SYS_MENU;
            //if (FormTypes.New == this.FormType)
            //{
            //    SysPermMenu.CREATEDATE = System.DateTime.Now;
            //    SysPermMenu.CREATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;

            //    ServiceClient.SysPermMenuAddAsync(this.SysPermMenu);
            //}
            //else
            //{
            //    SysPermMenu.UPDATEDATE = System.DateTime.Now;
            //    SysPermMenu.UPDATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;
            //    ServiceClient.SysPermMenuUpdateAsync(this.SysPermMenu);
            //}
        }

        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }

    }
}
