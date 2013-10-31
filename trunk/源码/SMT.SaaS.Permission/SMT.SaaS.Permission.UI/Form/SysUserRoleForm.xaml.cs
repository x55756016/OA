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



namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysUserRoleForm : BaseForm
    {
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        //private T_SYS_USER_ROLE sysUserRole;

        //public T_SYS_USER_ROLE SysUserRole
        //{
        //    get { return sysUserRole; }
        //    set 
        //    { 
        //        sysUserRole = value;
        //        this.DataContext = value;
        //    }
        //}

        private List<T_SYS_ROLE> sysRoleEnts = new List<T_SYS_ROLE>();
        /// <summary>
        /// 所有可用角色
        /// </summary>
        public List<T_SYS_ROLE> SysRoleEnts
        {
            get { return sysRoleEnts; }
            set { sysRoleEnts = value; }
        }

        //private List<T_SYS_USER_ROLE> assignedRoles =new List<T_SYS_USER_ROLE>();
        ///// <summary>
        ///// 已分配的角色
        ///// </summary>
        //public List<T_SYS_USER_ROLE> AssignedRoles
        //{
        //    get { return assignedRoles; }
        //    set { assignedRoles = value; }
        //}
        ///// <summary>
        ///// 被删除的用户角色
        ///// </summary>
        //private List<T_SYS_USER_ROLE> deletedUserRoles = new List<T_SYS_USER_ROLE>();

        protected PermissionServiceClient serviceClient;

        public SysUserRoleForm(FormTypes type)
        {
            InitializeComponent();
            FormType = type;
            //InitParas("");
        }

        public SysUserRoleForm(FormTypes type,string userRoleID)
        {
            InitializeComponent();
            FormType = type;
            //InitParas(userRoleID);
        }

        //private void InitParas(string userRoleID)
        //{
        //    //初始化事件
        //    serviceClient = new PermissionServiceClient();
        //    serviceClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(serviceClient_GetSysDictionaryByCategoryCompleted);
        //    serviceClient.GetSysUserRoleByIDCompleted += new EventHandler<GetSysUserRoleByIDCompletedEventArgs>(serviceClient_GetSysUserRoleByIDCompleted);
   
        //    serviceClient.GetSysRoleByTypeCompleted += new EventHandler<GetSysRoleByTypeCompletedEventArgs>(serviceClient_GetSysRoleByTypeCompleted);
        //    serviceClient.SysUserRoleAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(serviceClient_SysUserRoleAddCompleted);
        //    serviceClient.SysUserRoleUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(serviceClient_SysUserRoleUpdateCompleted);
        //    serviceClient.GetSysUserRoleByUserCompleted += new EventHandler<GetSysUserRoleByUserCompletedEventArgs>(serviceClient_GetSysUserRoleByUserCompleted);
        //    serviceClient.SysUserRoleDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(serviceClient_SysUserRoleDeleteCompleted);

        //    if (FormType == FormTypes.New)
        //    {
        //        sysUserRole = new T_SYS_USER_ROLE();
        //        SysUserRole.EMPLOYEEROLEID = Guid.NewGuid().ToString();
        //        //绑定系统类型
        //        serviceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //    }

        //    if (FormType == FormTypes.Edit)
        //    {
        //        //修改时不允许修改用户与系统类型
        //        lkUser.IsEnabled = false;
        //        cbxSysType.IsEnabled = false;
                
        //    }
        //    //初始化用户角色
        //    if (!string.IsNullOrEmpty(userRoleID))
        //    {
        //        serviceClient.GetSysUserRoleByIDAsync(userRoleID);
        //    }
        //}

        //void serviceClient_GetSysUserRoleByIDCompleted(object sender, GetSysUserRoleByIDCompletedEventArgs e)
        //{
        //    this.SysUserRole = e.Result;
        //    lkUser.DataContext = SysUserRole.T_SYS_USER;
        //    //lkUser.TxtLookUp.Text = SysUserRole.T_SYS_USER.USERNAME;

        //    //绑定系统类型
        //    serviceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        //    //绑定用户角色
        //    serviceClient.GetSysUserRoleByUserAsync(SysUserRole.T_SYS_USER.USERSYSPERMISSIONID);
        //}

        //void serviceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        //{
        //    //绑定系统类型
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
        //        cbxSysType.ItemsSource = dicts;
        //        cbxSysType.DisplayMemberPath = "DICTIONARYNAME";

        //        if (SysUserRole != null)
        //        {
        //            foreach (var item in cbxSysType.Items)
        //            {
        //                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
        //                if (dict != null)
        //                {
        //                    if (SysUserRole.T_SYS_ROLE != null && dict.DICTIONARYVALUE == SysUserRole.T_SYS_ROLE.SYSTEMTYPE)
        //                    {
        //                        cbxSysType.SelectedItem = item;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //void serviceClient_GetSysUserRoleByUserCompleted(object sender, GetSysUserRoleByUserCompletedEventArgs e)
        //{
        //    lbxSysRoleAssigned.ItemsSource = null;
        //    if (e.Result != null)
        //    {
        //        AssignedRoles = e.Result.ToList();
        //        lbxSysRoleAssigned.ItemsSource = AssignedRoles;
        //        lbxSysRoleAssigned.DisplayMemberPath = "T_SYS_ROLE.ROLENAME";
        //    }
        //}

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ////删除已删除的记录
            //foreach (T_SYS_USER_ROLE delUserRole in deletedUserRoles)
            //{
            //    serviceClient.SysUserRoleDeleteAsync(delUserRole.EMPLOYEEROLEID);
            //}
            ////添加新增的记录
            //foreach (T_SYS_USER_ROLE userrole in AssignedRoles)
            //{
            //    if (string.IsNullOrEmpty(userrole.EMPLOYEEROLEID))
            //    {
            //        userrole.EMPLOYEEROLEID = Guid.NewGuid().ToString();
            //        serviceClient.SysUserRoleAddAsync(userrole);
            //    }
            //}

        }

        //void serviceClient_SysUserRoleAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        //MessageBox.Show(Utility.GetResourceStr("ADDSUCCESSED",""));
        //        this.ReloadData();
        //        this.Close();
        //    }
        //}

        //void serviceClient_SysUserRoleUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        //MessageBox.Show(Utility.GetResourceStr("MODIFYSUCCESSED"));
        //        this.ReloadData();
        //        this.Close();
        //    }
        //}

        //void serviceClient_SysUserRoleDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        //MessageBox.Show(Utility.GetResourceStr("MODIFYSUCCESSED"));
        //        this.ReloadData();
        //        this.Close();
        //    }
        //}

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbxSysType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbxSysType.SelectedItem != null)
            //{
            //    T_SYS_DICTIONARY dict = cbxSysType.SelectedItem as T_SYS_DICTIONARY;
            //    if (dict != null)
            //    {
            //        serviceClient.GetSysRoleByTypeAsync(dict.DICTIONARYVALUE);
            //        ///TODO执行绑定员工编号的事件
            //    }
            //}
        }

        //void serviceClient_GetSysRoleByTypeCompleted(object sender, GetSysRoleByTypeCompletedEventArgs e)
        //{
        //    lbxSysRoleAll.ItemsSource = null;
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_ROLE> ents = e.Result.ToList();
        //        foreach (var item in lbxSysRoleAssigned.Items)
        //        {
        //            T_SYS_USER_ROLE tempRole = item as T_SYS_USER_ROLE;
        //            foreach (var ent in ents)
        //            {
        //                if (tempRole != null && ent != null && tempRole.T_SYS_ROLE !=null 
        //                    && tempRole.T_SYS_ROLE.ROLEID == ent.ROLEID)
        //                {
        //                    ents.Remove(ent);
        //                    break;
        //                }
        //            }
        //        }
        //        this.SysRoleEnts = ents;
        //        lbxSysRoleAll.ItemsSource = ents;
        //        lbxSysRoleAll.DisplayMemberPath = "ROLENAME";
        //    }
        //}

        private void btnSingleAssign_Click(object sender, RoutedEventArgs e)
        {
            //T_SYS_ROLE tempRole = lbxSysRoleAll.SelectedItem as T_SYS_ROLE;
            //if (tempRole != null)
            //{
            //    SysRoleEnts.Remove(tempRole);

            //    T_SYS_USER_ROLE userRole = new T_SYS_USER_ROLE();


            //    userRole.T_SYS_USER = lkUser.DataContext as T_SYS_USER;
            //    userRole.T_SYS_ROLE = tempRole;

            //    userRole.UPDATEDATE = System.DateTime.Now;
            //    userRole.UPDATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;
            //    userRole.CREATEDATE = System.DateTime.Now;
            //    userRole.CREATEUSER = Common.CurrentConfig.CurrentUser.USERSYSPERMISSIONID;

            //    AssignedRoles.Add(userRole);
            //}
            //BindListBox();
        }

        private void btnAllAssign_Click(object sender, RoutedEventArgs e)
        {
            //foreach(T_SYS_ROLE tempRole in SysRoleEnts)
            //{
            //    AssignedRoles.AddRange(tempRole.T_SYS_USER_ROLE);
            //}
            //BindListBox();
        }

        private void btnSingleRemove_Click(object sender, RoutedEventArgs e)
        {
            //T_SYS_USER_ROLE tempRole = lbxSysRoleAssigned.SelectedItem as T_SYS_USER_ROLE;
            //if (tempRole != null)
            //{
            //    SysRoleEnts.Add(tempRole.T_SYS_ROLE);
            //    AssignedRoles.Remove(tempRole);
            //    //记录删除过的角色
            //    if (!string.IsNullOrEmpty(tempRole.EMPLOYEEROLEID))
            //    {
            //        deletedUserRoles.Add(tempRole);
            //    }
            //}
            //BindListBox();
        }

        private void btnAllRemove_Click(object sender, RoutedEventArgs e)
        {
            //foreach (T_SYS_USER_ROLE tempRole in AssignedRoles)
            //{
            //    SysRoleEnts.Add(tempRole.T_SYS_ROLE);
            //    AssignedRoles.Remove(tempRole);
            //    //记录删除过的角色
            //    if (!string.IsNullOrEmpty(tempRole.EMPLOYEEROLEID))
            //    {
            //        deletedUserRoles.Add(tempRole);
            //    }
            //}
            //BindListBox();

        }
        //private void BindListBox()
        //{
        //    lbxSysRoleAll.ItemsSource = null;
        //    lbxSysRoleAll.ItemsSource = SysRoleEnts;
        //    lbxSysRoleAll.DisplayMemberPath = "ROLENAME";
        //    lbxSysRoleAssigned.ItemsSource = null;
        //    lbxSysRoleAssigned.ItemsSource = AssignedRoles;
        //    lbxSysRoleAssigned.DisplayMemberPath = "T_SYS_ROLE.ROLENAME";
        //}
        //private void HandleUserChanged()
        //{
        //    T_SYS_USER user = lkUser.DataContext as T_SYS_USER;
        //    foreach (T_SYS_USER_ROLE userRole in AssignedRoles)
        //    {
        //        userRole.T_SYS_USER = user;
        //    }
        //}
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            //string[] ColumnNames = new string[2];
            //ColumnNames[0] = "USERNAME";
            //ColumnNames[1] = "EMPLOYEENAME";

            //LookupForm lookup = new LookupForm(EntityNames.SysUser, 
            //    typeof(List<T_SYS_USER>), ColumnNames);

            //lookup.SelectedClick += (o, ev) =>
            //    {
            //        T_SYS_USER ent = lookup.SelectedObj as T_SYS_USER;
            //        if (ent != null)
            //        {                        
            //            lkUser.DataContext = ent;
            //            //lkUser.TxtLookUp.Text = ent.USERNAME;
            //            HandleUserChanged();
            //        }
            //    };

            //lookup.Show();
            
        }


    }
}

