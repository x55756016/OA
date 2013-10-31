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
    public partial class SysRolePermForms : UserControl, IEntityEditor
    {
        //private T_SYS_ROLE_PERM sysRolePerm;

        //public T_SYS_ROLE_PERM SysRolePerm
        //{
        //    get { return sysRolePerm; }
        //    set
        //    {
        //        sysRolePerm = value;
        //        this.DataContext = value;
        //    }
        //}
        private FormTypes formType;
        private string saveType = "0";       //保存方式 0:添加 1:关闭

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        protected PermissionServiceClient ServiceClient;

        public SysRolePermForms(FormTypes type)
        {
            InitializeComponent();
            FormType = type;
            //InitParas("");
        }

        public SysRolePermForms(FormTypes type, string RolePermID)
        {
            InitializeComponent();
            FormType = type;
            //InitParas(RolePermID);
        }

        //private void InitParas(string RolePermID)
        //{
        //    ServiceClient = new PermissionServiceClient();
        //    ServiceClient.GetSysRolePermByIDCompleted += new EventHandler<GetSysRolePermByIDCompletedEventArgs>(ServiceClient_GetSysRolePermByIDCompleted);
        //    ServiceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
        //    ServiceClient.GetSysRoleByTypeCompleted += new EventHandler<GetSysRoleByTypeCompletedEventArgs>(ServiceClient_GetSysRoleByTypeCompleted);
        //    ServiceClient.FindSysPermissionByTypeCompleted += new EventHandler<FindSysPermissionByTypeCompletedEventArgs>(ServiceClient_FindSysPermissionByTypeCompleted);
        //    ServiceClient.SysRolePermAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysRolePermAddCompleted);
        //    ServiceClient.SysRolePermUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysRolePermUpdateCompleted);
            
        //    if (FormType == FormTypes.New)
        //    {
        //        sysRolePerm = new T_SYS_ROLE_PERM();
        //        SysRolePerm.ID = Guid.NewGuid().ToString();
        //        //绑定系统类型
        //        ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //    }

        //    //初始化角色权限
        //    if (!string.IsNullOrEmpty(RolePermID))
        //    {
        //        ServiceClient.GetSysRolePermByIDAsync(RolePermID);
        //    }
        //}

        //void ServiceClient_GetSysRolePermByIDCompleted(object sender, GetSysRolePermByIDCompletedEventArgs e)
        //{
        //    this.SysRolePerm = e.Result;
        //    //绑定系统类型
        //    ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //}

        //void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        //{
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
        //        cbxSysType.ItemsSource = dicts;
        //        cbxSysType.DisplayMemberPath = "DICTIONARYNAME";

        //        if (SysRolePerm != null)
        //        {
        //            foreach (var item in cbxSysType.Items)
        //            {
        //                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
        //                if (dict != null)
        //                {
        //                    if (SysRolePerm.T_SYS_ROLE != null && dict.DICTIONARYVALUE == SysRolePerm.T_SYS_ROLE.SYSTEMTYPE)
        //                    {
        //                        cbxSysType.SelectedItem = item;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


        //void ServiceClient_SysRolePermAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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

        //void ServiceClient_SysRolePermUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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

        private void cbxSysType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbxSysType.SelectedItem != null)
            //{
            //    T_SYS_DICTIONARY dict = cbxSysType.SelectedItem as T_SYS_DICTIONARY;
            //    if (dict != null)
            //    {
            //        ServiceClient.GetSysRoleByTypeAsync(dict.DICTIONARYVALUE);
            //        ServiceClient.FindSysPermissionByTypeAsync(dict.DICTIONARYVALUE);
            //    }
            //}
        }

        //void ServiceClient_GetSysRoleByTypeCompleted(object sender, GetSysRoleByTypeCompletedEventArgs e)
        //{
        //    cbxSysRole.ItemsSource = null;
        //    if (e.Result != null)
        //    {
        //        //绑定角色名称
        //        List<T_SYS_ROLE> ents = e.Result.ToList();
        //        cbxSysRole.ItemsSource = ents;
        //        cbxSysRole.DisplayMemberPath = "ROLENAME";
        //        foreach (var item in cbxSysRole.Items)
        //        {
        //            T_SYS_ROLE temRole = item as T_SYS_ROLE;
        //            if (temRole != null && SysRolePerm != null && SysRolePerm.T_SYS_ROLE != null
        //                && temRole.ROLEID == this.SysRolePerm.T_SYS_ROLE.ROLEID)
        //            {
        //                cbxSysRole.SelectedItem = item;
        //                break;
        //            }
        //        }
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
        //            if (tmpPerm != null && SysRolePerm != null && SysRolePerm.T_SYS_PERMISSION != null
        //                && tmpPerm.PERMISSIONID == this.SysRolePerm.T_SYS_PERMISSION.PERMISSIONID)
        //            {
        //                cbxSysPerm.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }
        //}

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

        #region 保存并关闭
        public void AddToClose()
        {
            //SysRolePerm.T_SYS_ROLE = cbxSysRole.SelectedItem as T_SYS_ROLE;
            //SysRolePerm.T_SYS_PERMISSION = cbxSysPerm.SelectedItem as T_SYS_PERMISSION;
            //if (FormTypes.New == this.FormType)
            //{
            //    SysRolePerm.CREATEDATE = System.DateTime.Now;
            //    ///TODO增加修改人
            //    SysRolePerm.CREATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;
            //    ServiceClient.SysRolePermAddAsync(this.SysRolePerm);
            //}
            //else
            //{
            //    SysRolePerm.UPDATEDATE = System.DateTime.Now;
            //    ///TODO增加修改人
            //    SysRolePerm.UPDATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;
            //    ServiceClient.SysRolePermUpdateAsync(this.SysRolePerm);
            //}
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }

        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }
        #endregion
    }
}
