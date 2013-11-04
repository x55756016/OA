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
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class FbAdminForm : UserControl, IEntityEditor
    {

        
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        PermissionServiceClient PermClient = new PermissionServiceClient();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private ObservableCollection<string> StrAddStaffList = new ObservableCollection<string>();  //获取员工时的ID数组
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;     
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpMeetingMember = new List<ExtOrgObj>();
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpHostMember = new List<ExtOrgObj>();
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpRecordMember = new List<ExtOrgObj>();       
        private int SelectIndex = 0; //记录开始时间选择的位置        
        bool SelectChange = false;//下拉框动作
        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID

        private ObservableCollection<T_SYS_FBADMIN> ListFbAdmins = new ObservableCollection<T_SYS_FBADMIN>();

        private List<ExtOrgObj> issuanceExtOrgObj;

        
        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        private RefreshedTypes RefreshType = RefreshedTypes.Close;
        public FbAdminForm()
        {
            InitializeComponent();
            
            InitEvent();
        }

        private void InitEvent()
        {
            //personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
            PermClient.BatchAddFBAdminsCompleted += new EventHandler<BatchAddFBAdminsCompletedEventArgs>(PermClient_BatchAddFBAdminsCompleted);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
        }

        void personclient_GetEmployeeDetailByIDsCompleted(object sender, GetEmployeeDetailByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    StrDepartmentIDsList.Clear();
                    StrCompanyIDsList.Clear();
                    StrPositionIDsList.Clear();
                    if (e.Result != null)
                    {
                        List<V_EMPLOYEEPOST> allPost = e.Result.ToList();
                        if (allPost.Count() > 0)
                        {

                        }
                        vemployeeObj = e.Result.ToList();

                        BindData();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void PermClient_BatchAddFBAdminsCompleted(object sender, BatchAddFBAdminsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "设置预算管理员成功！");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(Convert.ToString(e.Error)));
            }
            RefreshUI(RefreshType);
        }

       

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            AddMeetingStaffInfo();//添加授权人员
        }

        


        #region 添加预算管理员信息
        void AddMeetingStaffInfo()
        {


            if (tmpMeetingMember != null)
            {
                ListFbAdmins.Clear();
                foreach (var employ in vemployeeObj)
                {
                    T_SYS_FBADMIN fbadmin = new T_SYS_FBADMIN();
                    fbadmin.FBADMINID = System.Guid.NewGuid().ToString();
                    fbadmin.EMPLOYEEID = employ.T_HR_EMPLOYEE.EMPLOYEEID;
                    fbadmin.SYSUSERID = "";
                    fbadmin.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    fbadmin.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    fbadmin.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    fbadmin.EMPLOYEECOMPANYID = employ.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                    fbadmin.EMPLOYEEDEPARTMENTID = employ.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    fbadmin.EMPLOYEEPOSTID = employ.EMPLOYEEPOSTS[0].T_HR_POST.POSTID;
                    fbadmin.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    fbadmin.ROLENAME = employ.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME + "预算配置员";
                    fbadmin.ISCOMPANYADMIN = "1";
                    fbadmin.ISSUPPERADMIN = "0";
                    fbadmin.ADDUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                    ListFbAdmins.Add(fbadmin);
                }
            }
            if (ListFbAdmins.Count() > 0)
            {
                PermClient.BatchAddFBAdminsAsync(ListFbAdmins);
            }
           
        }
        
        
        #endregion

        #region 选择授权人员
        
        
        private void AddMembersObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {

                    foreach (var h in ent)
                    {
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)//公司
                        {
                            StrCompanyIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)//部门
                        {

                            StrDepartmentIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)//岗位
                        {
                            StrPositionIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
                        {
                            StrStaffList.Add(h.ObjectID);
                        }
                    }
                    //issuanceExtOrgObj = ent;
                    tmpMeetingMember = ent;                    
                    personclient.GetEmployeeDetailByIDsAsync(StrStaffList);                    
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }

        
        #endregion

        private void BindData()
        {

            if (vemployeeObj == null || vemployeeObj.Count < 1)
            {
                dgmember.ItemsSource = null;

                return;
            }
            else
            {
                dgmember.ItemsSource = vemployeeObj;
            }

        }


        #region dgmember_loadingrow事件


        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST StaffV = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST)e.Row.DataContext;

            Button DelBtn = dgmember.Columns[4].GetCellContent(e.Row).FindName("BtnDel") as Button;
            DelBtn.Tag = StaffV;
            int index = e.Row.GetIndex();
            var cell = dgmember.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();


        }
        #endregion

        #region 删除按钮



        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            Button delBtn = sender as Button;
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST MeetingV = delBtn.Tag as SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST;
            vemployeeObj.Remove(MeetingV);
            dgmember.ItemsSource = null;
            BindData();
        }
        #endregion
        public void AddToClose()
        {
            AddMeetingStaffInfo();
        }

        #region 选择人员
        
        
        private void CheckMember_Click(object sender, RoutedEventArgs e)
        {
            AddMembersObj();
        }
        #endregion


        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "预算管理员配置";
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
                    RefreshType = RefreshedTypes.All;
                    AddToClose();
                    break;
                case "1":
                    RefreshType = RefreshedTypes.CloseAndReloadData;
                    AddToClose();
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
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
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
    }
}
