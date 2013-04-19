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
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class EvectionForm : BaseForm, IEntityEditor
    {
        public FormTypes FormType { get; set; }
        public string EvectionID { get; set; }
        private T_HR_EMPLOYEEEVECTIONRECORD evection;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        public T_HR_EMPLOYEEEVECTIONRECORD Evection
        {
            get { return evection; }
            set 
            { 
                evection = value;
                this.DataContext = value;
            }
        }

        AttendanceServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient perClient;
        public EvectionForm(FormTypes type,string strID)
        {
            InitializeComponent();
            FormType = type;
            EvectionID = strID;
            InitParas();
        }

        private void InitParas()
        {
            client = new AttendanceServiceClient();
            client.GetEmployeeEvectionRecordByIDCompleted += new EventHandler<GetEmployeeEvectionRecordByIDCompletedEventArgs>(client_GetEmployeeEvectionRecordByIDCompleted);
            client.EmployeeEvectionRecordAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeEvectionRecordAddCompleted);
            client.EmployeeEvectionRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeEvectionRecordUpdateCompleted);
            client.GetAttendanceRecordByEmployeeIDCompleted += new EventHandler<GetAttendanceRecordByEmployeeIDCompletedEventArgs>(client_GetAttendanceRecordByEmployeeIDCompleted);

            //获取员工名称，并显示所在的公司架构
            perClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            perClient.GetEmployeeDetailByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs>(perClient_GetEmployeeDetailByIDCompleted);

            if (FormType == FormTypes.New)
            {
                Evection = new T_HR_EMPLOYEEEVECTIONRECORD();
                Evection.EVECTIONRECORDID = Guid.NewGuid().ToString();
                Evection.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpStartDate.Text, "dpStartDate");
                client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpEndDate.Text, "dpEndDate");
                SetToolBar();
            }
            else
            {
                client.GetEmployeeEvectionRecordByIDAsync(EvectionID);
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEEVECTIONRECORD", Evection.OWNERID,
                    Evection.OWNERPOSTID, Evection.OWNERDEPARTMENTID, Evection.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        void client_GetEmployeeEvectionRecordByIDCompleted(object sender, GetEmployeeEvectionRecordByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Evection = e.Result;
                client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpStartDate.Text, "dpStartDate");
                client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpEndDate.Text, "dpEndDate");
                perClient.GetEmployeeDetailByIDAsync(Evection.EMPLOYEEID);

                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));                
            }
        }

        void perClient_GetEmployeeDetailByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeePost = e.Result;
                //赋值
                tbEmpCode.Text = employeePost.T_HR_EMPLOYEE.EMPLOYEECODE;
                tbEmpName.Text = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME;
                tbPostName.Text = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                tbDepName.Text = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                tbCPYName.Text = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                lkEmployeeName.DataContext = employeePost.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));                
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("COMPANY");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee.xaml"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;            
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
        
        private void Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
            }
            else
            {               
                
                //承接人
                //OrganizationWS.V_EMPLOYEEPOST employee = lkEmployeeName.DataContext as OrganizationWS.V_EMPLOYEEPOST;
                //Evection.REPLACEPEOPLE = employee.T_HR_EMPLOYEE.EMPLOYEEID;
                //if (FormType == FormTypes.Edit)
                //{
                //    //如果状态为审核通过，修改时，则修改状态为审核中
                //    if (Evection.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                //    {
                //        Evection.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                //    }
                //    Evection.UPDATEDATE = System.DateTime.Now;
                //    Evection.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; 

                //    client.EmployeeEvectionRecordUpdateAsync(Evection);
                //}
                //else
                //{
                //    //所属员工
                //    Evection.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //    Evection.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                //    Evection.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                //    Evection.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; 

                //    //添加人所属部门ID
                //    Evection.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                //    Evection.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                //    Evection.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //    //出差记录只能本人申请请
                //    Evection.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //    Evection.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                //    Evection.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;
                                        
                //    Evection.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                //    Evection.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //    Evection.CREATEDATE = DateTime.Now;
                    client.EmployeeEvectionRecordAddAsync(Evection);
                //}
            }
        }

        void client_EmployeeEvectionRecordAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEEEVECTIONRECORD"));
            }
            RefreshUI(RefreshedTypes.All);
        }

        void client_EmployeeEvectionRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "EMPLOYEEEVECTIONRECORD"));
            }
            RefreshUI(RefreshedTypes.All);
        }

        private void Cancel()
        {
            
        }

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
                typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        #region 计算出差天数
        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpStartDate.Text != "" && dpEndDate.Text != "")
            {
                if (Convert.ToDateTime(dpStartDate.Text) > Convert.ToDateTime(dpEndDate.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATECOMPARE", "STARTDATETITLE,ENDDATETITLE"));
                    dpStartDate.Focus();
                    dpStartDate.Text = "";
                    return;
                }
            }
            client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpStartDate.Text, "dpStartDate");
        }

        private void dpEndDate_SelectedDateChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (dpStartDate.Text != "" && dpEndDate.Text != "")
            {
                if (Convert.ToDateTime(dpStartDate.Text) > Convert.ToDateTime(dpEndDate.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATECOMPARE", "STARTDATETITLE,ENDDATETITLE"));
                    dpEndDate.Focus();
                    dpEndDate.Text = "";
                    return;
                }
            }
            client.GetAttendanceRecordByEmployeeIDAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, dpEndDate.Text, "dpEndDate");
        }

        void client_GetAttendanceRecordByEmployeeIDCompleted(object sender, GetAttendanceRecordByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.UserState.ToString() == "dpStartDate")
                { 
                    cbxStartTime.ItemsSource = e.Result;
                    cbxStartTime.DisplayMemberPath = "STARTTIME";
                    if (Evection != null)
                    {
                        foreach (var item in cbxStartTime.Items)
                        {
                            V_ATTENDANCERECORD temp = item as V_ATTENDANCERECORD;
                            if (temp.STARTTIME == Evection.STARTTIME)
                            {
                                cbxStartTime.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    cbxEndTime.ItemsSource = e.Result;
                    cbxEndTime.DisplayMemberPath = "ENDTIME";
                    if (Evection != null)
                    {
                        foreach (var item in cbxEndTime.Items)
                        {
                            V_ATTENDANCERECORD temp = item as V_ATTENDANCERECORD;
                            if (temp.ENDTIME == Evection.ENDTIME)
                            {
                                cbxEndTime.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
            AccountDays();
        }

        private void AccountDays()
        {
            double day = 0;
            if (dpStartDate.Text != "" && dpEndDate.Text != "")
            {
                day = Convert.ToDouble((Convert.ToDateTime(dpEndDate.Text) - Convert.ToDateTime(dpStartDate.Text)).Days) + 1;
                V_ATTENDANCERECORD tempStart = cbxStartTime.SelectedItem as V_ATTENDANCERECORD;
                V_ATTENDANCERECORD tempEnd = cbxEndTime.SelectedItem as V_ATTENDANCERECORD;
                if (tempStart != null && tempEnd != null)
                {
                    day = day + (tempEnd.ENDVALUE - tempStart.STARTVALUE);
                }
                nudToTaldays.Value = day;
            }
        }

        private void cbxEndTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountDays();
        }

        #endregion
    }
}

