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
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Permission.UI.Form;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysUserRoleSet : UserControl
    {
        
        private T_SYS_USER tmpUser=new T_SYS_USER();
        private T_SYS_ROLE role;
        protected PermissionServiceClient ServiceClient;
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        private ObservableCollection<T_SYS_USERROLE> ViewInfosList = new ObservableCollection<T_SYS_USERROLE>();
        private ObservableCollection<T_SYS_ROLE> ViewRoleList = new ObservableCollection<T_SYS_ROLE>();
        private List<T_SYS_USERROLE> tmpRoleList = new List<T_SYS_USERROLE>();//roleentityid 集合
        public T_SYS_ROLE Role
        {
            get { return role; }
            set { role = value; }
        }

        public SysUserRoleSet(T_SYS_USER UserObj)
        {
            tmpUser = UserObj;
            //this.tblTitle.Text = tmpUser.USERNAME.ToString() + "授权";

            InitializeComponent();
            InitControlEvent();
            LoadData();
        }

        
        
        //private SysRoleManagementServiceClient SysRoleClient = new SysRoleManagementServiceClient();

        private void InitControlEvent()
        {
            ServiceClient = new PermissionServiceClient();
            ServiceClient.GetSysRoleInfosCompleted += new EventHandler<GetSysRoleInfosCompletedEventArgs>(SysRoleClient_GetSysRoleInfosCompleted);
            ServiceClient.UserRoleBatchAddInfosCompleted += new EventHandler<UserRoleBatchAddInfosCompletedEventArgs>(SysRoleClient_UserRoleBatchAddInfosCompleted);
            ServiceClient.GetSysUserRoleByUserCompleted += new EventHandler<GetSysUserRoleByUserCompletedEventArgs>(ServiceClient_GetSysUserRoleByUserCompleted);
            
            //ServiceClient.GetSysUserRoleByUser
        }

        void ServiceClient_GetSysUserRoleByUserCompleted(object sender, GetSysUserRoleByUserCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Result != null)
            {
                List<T_SYS_USERROLE> listuserrole = new List<T_SYS_USERROLE>();
                listuserrole = e.Result.ToList();
                foreach (T_SYS_USERROLE roleid in listuserrole)
                {
                    tmpRoleList.Add(roleid);
                }
                SetUserRoleInfos();
            }
        }

        

        void SysRoleClient_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 100;
            }

            DtGrid.ItemsSource = pcv;
            GetUserRoleInfo(tmpUser);
            ServiceClient.GetSysUserRoleByUserAsync(tmpUser.SYSUSERID);
            
            
        }

        #region 获取用户的角色
        private void GetUserRoleInfo(T_SYS_USER UserObj)
        { 

        }
        #endregion

        void SysRoleClient_UserRoleBatchAddInfosCompleted(object sender, UserRoleBatchAddInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "用户授权成功", Utility.GetResourceStr("CONFIRMBUTTON"));
                    
                    
                }
            }
        }

        
        /// <summary>
        /// 加载菜单数据
        /// </summary>
        private void LoadData()
        {
            //ServiceClient.GetSysRoleByTypeAsync(this.txtSearchSystemType.Text.Trim());
            ServiceClient.GetSysRoleInfosAsync("","");
        }


             

        #region 模板中checkbox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == false)
            {
                this.chkAll.Content = "全选";
                this.chkAll.IsChecked = false;
            }
        }
        #endregion

        #region 全选事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.ItemsSource != null)
            {
                if (this.chkAll.IsChecked.Value)//全选
                {

                    foreach (object obj in DtGrid.ItemsSource)
                    {
                        if (DtGrid.Columns[0].GetCellContent(obj) != null)
                        {
                            CheckBox cb1 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为
                            cb1.IsChecked = true;
                        }
                    }
                    chkAll.Content = "全不选";
                }
                else//取消
                {
                    foreach (object obj in DtGrid.ItemsSource)
                    {
                        if (DtGrid.Columns[0].GetCellContent(obj) != null)
                        {
                            CheckBox cb2 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox;
                            cb2.IsChecked = false;
                        }
                    }
                    chkAll.Content = "全选";
                }
            }
        }


        #endregion

        #region LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {

            //T_OA_SENDDOC OrderInfoT = (T_OA_SENDDOC)e.Row.DataContext;
            T_SYS_ROLE RoleT = (T_SYS_ROLE)e.Row.DataContext;
            CheckBox mychkBox = DtGrid.Columns[0].GetCellContent(e.Row).FindName("myChkBtn") as CheckBox;

            mychkBox.Tag = RoleT;

        }

        #endregion



        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string SysRoleID = "";
            T_SYS_ROLE RoleT = new T_SYS_ROLE();
            if (this.DtGrid.ItemsSource != null)
            {
                foreach (object obj in DtGrid.ItemsSource)
                {
                    if (DtGrid.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为

                        if (cb1.IsChecked == true)
                        {
                            RoleT = cb1.Tag as T_SYS_ROLE;
                            //SysRoleID = RoleT.ROLEID;
                            //DelInfosList.Add(SysRoleID);
                            ViewRoleList.Add(RoleT);
                        }
                    }
                }

            }


            if (ViewRoleList.Count > 0)
            {

                
                T_SYS_USER rolewsuser = new T_SYS_USER();
                rolewsuser.SYSUSERID = tmpUser.SYSUSERID;
                rolewsuser.EMPLOYEEID = tmpUser.EMPLOYEEID;
                rolewsuser.EMPLOYEECODE = tmpUser.EMPLOYEECODE;
                rolewsuser.EMPLOYEENAME = tmpUser.EMPLOYEENAME;
                rolewsuser.CREATEDATE = tmpUser.CREATEDATE;
                rolewsuser.CREATEUSER = tmpUser.CREATEUSER;
                rolewsuser.PASSWORD = tmpUser.PASSWORD;
                rolewsuser.UPDATEDATE = tmpUser.UPDATEDATE;
                rolewsuser.UPDATEUSER = tmpUser.UPDATEUSER;
                rolewsuser.USERNAME = tmpUser.USERNAME;
                rolewsuser.STATE = tmpUser.STATE;

                ServiceClient.UserRoleBatchAddInfosAsync(ViewRoleList, rolewsuser, "admin", System.DateTime.Now);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请您选择角色！", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DtGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //SetUserRoleInfos();
        }

        private void SetUserRoleInfos()
        {
            if (tmpRoleList.Count() == 0) return;
            if (DtGrid.ItemsSource != null)
            {
                foreach (object obj in DtGrid.ItemsSource)
                {
                    T_SYS_ROLE menu = new T_SYS_ROLE();
                    if (DtGrid.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DtGrid.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为

                        menu = cb1.Tag as T_SYS_ROLE;
                        var bb = from a in tmpRoleList

                                 where a.T_SYS_ROLE.ROLEID == menu.ROLEID
                                 select a;
                        if (bb.Count() > 0)
                        {
                            cb1.IsChecked = true;


                        }//bb.cout


                    }
                }
            }
        }



    }
}
