using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;
namespace SMT.SaaS.OA.UI.Views
{
    public partial class CFrmCalendarInfo : BaseForm, IClient, IEntityEditor
    {
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private PersonnelServiceClient client = new PersonnelServiceClient();
        private V_EMPLOYEEPOST employeepost;
        private string editStatues = null;
        private T_OA_CALENDAR cordInfo = null;

        public CFrmCalendarInfo()
        {
            InitializeComponent();
            calendarManagement.AddCalendarCompleted += new EventHandler<AddCalendarCompletedEventArgs>(calendarManagement_AddCalendarCompleted);
            calendarManagement.UpdateCalendarCompleted += new EventHandler<UpdateCalendarCompletedEventArgs>(calendarManagement_UpdateCalendarCompleted);
        }
        private DateTime selectDateTime;
        public CFrmCalendarInfo(DateTime selectDateTime, string editSatue)
        {
            InitializeComponent();
            editStatues = editSatue;
            this.selectDateTime = selectDateTime;
            if (cordInfo == null && editStatues == "add")
            {
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
            }
            SetDefaultDate();
            tpTime.Value = Convert.ToDateTime(System.DateTime.Now.ToShortTimeString());
            if (dpSelectDate.Visibility == Visibility.Visible)
                dpSelectDate.Text = System.DateTime.Now.ToShortDateString();

            calendarManagement.AddCalendarCompleted += new EventHandler<AddCalendarCompletedEventArgs>(calendarManagement_AddCalendarCompleted);
            calendarManagement.UpdateCalendarCompleted += new EventHandler<UpdateCalendarCompletedEventArgs>(calendarManagement_UpdateCalendarCompleted);
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
        }
        void calendarManagement_UpdateCalendarCompleted(object sender, UpdateCalendarCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                editStatues = "update";
                Utility.ShowMessageBox("UPDATE", false, true);
                RefreshUI(refreshType);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        #region 获取当前用户信息
        void client_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeepost = e.Result;
                GetAllPost(e.Result);
            }
        }
        private void GetAllPost(V_EMPLOYEEPOST ent)//获取当前员工、公司、岗位、部门、联系电话
        {
            if (ent != null && ent.EMPLOYEEPOSTS != null)
            {
                txtCreateUserName.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
            }
        }
        #endregion

        void calendarManagement_AddCalendarCompleted(object sender, AddCalendarCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    return;
                }
                if (e.Result > 0)
                {
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        editStatues = "update";
                        Utility.ShowMessageBox("ADD", false, true);
                    }
                }
                else
                {
                    Utility.ShowMessageBox("ADD", false, false);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        //修改

        private T_OA_CALENDAR calendarInfo = null;
        public CFrmCalendarInfo(T_OA_CALENDAR selCalendarInfo, string editSatue)
        {
            InitializeComponent();
            editStatues = editSatue;
            calendarManagement.AddCalendarCompleted += new EventHandler<AddCalendarCompletedEventArgs>(calendarManagement_AddCalendarCompleted);
            calendarManagement.UpdateCalendarCompleted += new EventHandler<UpdateCalendarCompletedEventArgs>(calendarManagement_UpdateCalendarCompleted);
            calendarInfo = selCalendarInfo;
            txtNotes.Text = selCalendarInfo.CONTENT;
            txtTitle.Text = selCalendarInfo.TITLE;
            //txtSelectedHour.Text = selCalendarInfo.PLANTIME.Hour.ToString();
            //txtSelectedMin.Text = selCalendarInfo.PLANTIME.Minute.ToString();
            if (calendarInfo != null && editStatues == "update" || editStatues == "view")
            {
                txtCreateUserName.Text = calendarInfo.CREATEUSERNAME;
            }
            tpTime.Value = selCalendarInfo.PLANTIME;
            switch (selCalendarInfo.REPARTREMINDER)
            {
                case "NOTHING":
                    cmbStyle.SelectedIndex = 0;
                    cmbSelectDate.Items.Add(Convert.ToDateTime(selCalendarInfo.REMINDERRMODEL).ToShortDateString());
                    cmbSelectDate.SelectedIndex = 0;
                    dpSelectDate.Text = selCalendarInfo.PLANTIME.ToShortDateString();
                    break;
                case "DAY"://每天
                    cmbStyle.SelectedIndex = 1;
                    break;
                case "WEEK"://每周
                    cmbStyle.SelectedIndex = 2;
                    switch (selCalendarInfo.REMINDERRMODEL)
                    {
                        case "Monday":
                            cmbSelectDate.SelectedIndex = 0;
                            break;
                        case "Tuesday":
                            cmbSelectDate.SelectedIndex = 1;
                            break;
                        case "Wednesday":
                            cmbSelectDate.SelectedIndex = 2;
                            break;
                        case "Thursday":
                            cmbSelectDate.SelectedIndex = 3;
                            break;
                        case "Friday":
                            cmbSelectDate.SelectedIndex = 4;
                            break;
                        case "Saturday":
                            cmbSelectDate.SelectedIndex = 5;
                            break;
                        case "Sunday":
                            cmbSelectDate.SelectedIndex = 6;
                            break;
                    }
                    break;
                case "MONTH"://每月
                    cmbStyle.SelectedIndex = 3;
                    cmbSelectDate.SelectedIndex = Convert.ToInt32(selCalendarInfo.REMINDERRMODEL) - 1;
                    break;
                case "YEAR"://每年
                    cmbStyle.SelectedIndex = 4;
                    if (gdMonthDay.Visibility == Visibility.Visible)
                    {
                        cmbSelectedMonth.SelectedIndex = Convert.ToDateTime(selCalendarInfo.PLANTIME).Month - 1;
                        cmbSelectedDay.SelectedIndex = Convert.ToDateTime(selCalendarInfo.PLANTIME).Day - 1;
                    }

                    break;
            }
            editStatues = editSatue;
            if (editSatue == "view")
            {
                this.IsEnabled = false;
            }
        }

        private SmtOAPersonOfficeClient calendarManagement = new SmtOAPersonOfficeClient();
        //验证输入
        private string Check()
        {
            if (txtTitle.Text.Trim() == "" || txtTitle.Text.Trim() == string.Empty)
                return Utility.GetResourceStr("REQUIRED", "CalendarTitle");
            if (txtNotes.Text.Trim() == "" || txtNotes.Text.Trim() == string.Empty)
                return Utility.GetResourceStr("REQUIRED", "CalendarContent");
            if (tpTime.Value.IsNull())
                return Utility.GetResourceStr("REQUIRED", "PLANTIME");
            if (cmbSelectDate.Visibility == Visibility.Visible)
            {
                if (cmbSelectDate.SelectedIndex < 0)
                    return Utility.GetResourceStr("SELECTDATE");
                if (gdMonthDay.Visibility == Visibility.Visible)
                {
                    if (cmbSelectedDay.SelectedIndex < 0)
                        return Utility.GetResourceStr("SELECTDATE");
                    if (cmbSelectedMonth.SelectedIndex < 0)
                        return Utility.GetResourceStr("SELECTDATE");
                }
            }

            if (cmbStyle.SelectedIndex < 0)
                return Utility.GetResourceStr("REMINDERRMODEL");
            return null;
        }

        private void cmbStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStyle == null) return;
            if (cmbSelectDate != null)
            {
                cmbSelectDate.Items.Clear();
            }
            cmbSelectDate.Visibility = Visibility.Visible;
            switch (cmbStyle.SelectedIndex)
            {
                case 0:
                    SetDefaultDate();
                    dpSelectDate.Visibility = Visibility.Visible;
                    cmbSelectDate.Visibility = Visibility.Collapsed;
                    gdMonthDay.Visibility = Visibility.Collapsed;
                    break;
                case 1://天
                    //SetDefaultDate();
                    dpSelectDate.Visibility = Visibility.Collapsed;
                    cmbSelectDate.Visibility = Visibility.Collapsed;
                    gdMonthDay.Visibility = Visibility.Collapsed;
                    break;
                case 2://周
                    dpSelectDate.Visibility = Visibility.Collapsed;
                    cmbSelectDate.Visibility = Visibility.Visible;
                    gdMonthDay.Visibility = Visibility.Collapsed;
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("SUN"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("MON"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("TUE"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("WED"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("THR"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("FRI"));
                    cmbSelectDate.Items.Add(Utility.GetResourceStr("SAT"));
                    cmbSelectDate.SelectedIndex = DayOfWeekToNumber(System.DateTime.Now);
                    break;
                case 3://月
                    dpSelectDate.Visibility = Visibility.Collapsed;
                    cmbSelectDate.Visibility = Visibility.Visible;
                    gdMonthDay.Visibility = Visibility.Collapsed;
                    for (int i = 1; i < 32; i++)
                    {
                        cmbSelectDate.Items.Add(Utility.GetResourceStr("PM") + i.ToString() + Utility.GetResourceStr("D"));
                    }
                    cmbSelectDate.SelectedIndex = System.DateTime.Now.Day - 1;
                    break;
                case 4://年
                    dpSelectDate.Visibility = Visibility.Collapsed;
                    cmbSelectDate.Visibility = Visibility.Collapsed;
                    gdMonthDay.Visibility = Visibility.Visible;
                    if (gdMonthDay.Visibility == Visibility.Visible)
                    {
                        cmbSelectedMonth.SelectedIndex = System.DateTime.Now.Month - 1;
                        cmbSelectedDay.SelectedIndex = System.DateTime.Now.Day - 1;
                        tpTime.Value = System.DateTime.Now;
                    }
                    else
                    {
                        cmbSelectedDay.SelectedIndex = 0;
                        cmbSelectedMonth.SelectedIndex = 0;
                        tpTime.Value = new DateTime(0, 0, 0, 0, 0, 0);
                    }
                    break;
            }
        }

        private int DayOfWeekToNumber(DateTime dtDate)
        {
            switch (dtDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 0;
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                default:
                    return -1;
            }
        }
        #region 设置默认值
        private void SetDefaultDate()
        {
            if (cmbSelectDate != null)
            {
                cmbSelectDate.Items.Clear();
            }
            if (selectDateTime == null)
            {
                cmbSelectDate.Items.Add(System.DateTime.Now.ToShortDateString());
            }
            else
            {
                cmbSelectDate.Items.Add(selectDateTime.ToShortDateString());
            }
            cmbSelectDate.SelectedIndex = 0;
        }
        #endregion

        private void AddInfo()
        {
            if (editStatues == "add")
                calendarInfo = new T_OA_CALENDAR();

            calendarInfo.CONTENT = txtNotes.Text;
            calendarInfo.TITLE = txtTitle.Text;

            DateTime planTime;
            switch (cmbStyle.SelectedIndex)
            {
                case 0:
                    calendarInfo.REPARTREMINDER = "NOTHING";
                    if (dpSelectDate.Visibility == Visibility.Visible)
                    {
                        if (dpSelectDate.Text.Trim() != "" && dpSelectDate.Text.Trim() != string.Empty)
                        {
                            calendarInfo.REMINDERRMODEL = dpSelectDate.Text + " " + tpTime.Value.Value.ToShortTimeString();
                            planTime = Convert.ToDateTime(dpSelectDate.Text + " " + tpTime.Value.Value.ToShortTimeString());
                            calendarInfo.PLANTIME = planTime;
                        }
                    }
                    else
                        return;
                    break;
                case 1://每天
                    calendarInfo.REPARTREMINDER = "DAY";
                    planTime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);
                    calendarInfo.PLANTIME = planTime;
                    calendarInfo.REMINDERRMODEL = planTime.ToShortTimeString();
                    break;
                case 2://每周
                    calendarInfo.REPARTREMINDER = "WEEK";
                    switch (cmbSelectDate.SelectedIndex)
                    {
                        case 0:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Monday.ToString();
                            break;
                        case 1:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Tuesday.ToString();
                            break;
                        case 2:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Wednesday.ToString();
                            break;
                        case 3:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Thursday.ToString();
                            break;
                        case 4:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Friday.ToString();
                            break;
                        case 5:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Saturday.ToString();
                            break;
                        case 6:
                            calendarInfo.REMINDERRMODEL = DayOfWeek.Sunday.ToString();
                            break;
                    }
                    planTime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);
                    calendarInfo.PLANTIME = planTime;
                    break;
                case 3://每月                    
                    calendarInfo.REPARTREMINDER = "MONTH";
                    if (cmbSelectDate.Visibility == Visibility.Visible)
                    {
                        //取当前日期 输入 时间
                        planTime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, cmbSelectDate.SelectedIndex + 1, tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);
                        calendarInfo.PLANTIME = planTime;
                        calendarInfo.REMINDERRMODEL = planTime.Day.ToString();
                    }
                    break;
                case 4://每年
                    calendarInfo.REPARTREMINDER = "YEAR";
                    //calendarInfo.REMINDERRMODEL = cmbSelectDate.Items[0].ToString();
                    if (gdMonthDay.Visibility == Visibility.Visible)
                    {
                        planTime = new DateTime(System.DateTime.Now.Year, cmbSelectedMonth.SelectedIndex + 1, cmbSelectedDay.SelectedIndex + 1, tpTime.Value.Value.Hour, tpTime.Value.Value.Minute, 0);
                        calendarInfo.PLANTIME = planTime;
                        calendarInfo.REMINDERRMODEL = planTime.ToString();
                    }
                    break;
            }
            if (string.IsNullOrEmpty(this.dpSelectDate.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLANDATECANNOTBEEMPTY"));
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (this.tpTime.Value.Value == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLANNINGTIMECANNOTBEEMPTY"));
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (editStatues == "add")
            {
                calendarInfo.CALENDARID = System.Guid.NewGuid().ToString();
                calendarInfo.CREATEDATE = System.DateTime.Now;
                calendarInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                calendarInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                calendarInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                calendarInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                calendarInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                calendarInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                calendarInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                calendarInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                calendarInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                calendarInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                calendarInfo.UPDATEDATE = System.DateTime.Now;
                calendarInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                calendarInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                calendarManagement.AddCalendarAsync(calendarInfo);
            }
            else
            {
                calendarInfo.UPDATEDATE = System.DateTime.Now;
                calendarInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                calendarInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                calendarManagement.UpdateCalendarAsync(calendarInfo);
            }
        }
        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #region
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (editStatues != "view")
            {
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
            }
            return items;
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        public void DoAction(string actionType)
        {
            string errorString = Check();
            if (errorString != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(errorString));
                return;
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (actionType)
            {
                case "0":
                    refreshType = RefreshedTypes.All;
                    AddInfo();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    AddInfo();
                    break;
            }
        }

        public event UIRefreshedHandler OnUIRefreshed;

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("WORK");
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            calendarManagement.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

