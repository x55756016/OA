using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class PublicVacationForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string VacationID { get; set; }

        public T_HR_VACATIONSET entVacationSet { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient clientOrg = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient clientPer = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        #endregion

        #region 初始化

        public PublicVacationForm(FormTypes formtype, string strVacationID)
        {
            InitializeComponent();
            FormType = formtype;
            VacationID = strVacationID;
            this.Loaded += new RoutedEventHandler(PublicVacationForm_Loaded);
        }

        void PublicVacationForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetVacationSetByIDCompleted += new EventHandler<GetVacationSetByIDCompletedEventArgs>(clientAtt_GetVacationSetByIDCompleted);
            clientAtt.AddVacationSetCompleted += new EventHandler<AddVacationSetCompletedEventArgs>(clientAtt_AddVacationSetCompleted);
            clientAtt.ModifyVacationSetCompleted += new EventHandler<ModifyVacationSetCompletedEventArgs>(clientAtt_ModifyVacationSetCompleted);
            clientAtt.GetAllOutPlanDaysRdListByMultSearchCompleted += new EventHandler<GetAllOutPlanDaysRdListByMultSearchCompletedEventArgs>(clientAtt_GetAllOutPlanDaysRdListByMultSearchCompleted);

            #region 获取分配对像
            clientOrg.GetPostByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs>(clientOrg_GetPostByIdCompleted);
            clientOrg.GetDepartmentByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs>(clientOrg_GetDepartmentByIdCompleted);
            clientOrg.GetCompanyByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs>(clientOrg_GetCompanyByIdCompleted);
            clientPer.GetEmployeeByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeByIDCompletedEventArgs>(clientPer_GetEmployeeByIDCompleted);
            #endregion

            UnVisibleGridToolControl(toolbarVacDay);
            UnVisibleGridToolControl(toolbarWorkDay);

            #region OutPlan DataGrid ToolBar事件
            toolbarVacDay.btnNew.Click += new RoutedEventHandler(btnVacDayNew_Click);
            toolbarVacDay.btnDelete.Click += new RoutedEventHandler(btnVacDayDelete_Click);

            toolbarWorkDay.btnNew.Click += new RoutedEventHandler(btnWorkDayNew_Click);
            toolbarWorkDay.btnDelete.Click += new RoutedEventHandler(btnWorkDayDelete_Click);
            #endregion
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                InitForm();
                SetToolBar();
            }
            else
            {
                LoadData();
                if (FormType == FormTypes.Browse)
                {
                    txtPubVacName.IsReadOnly = true;
                    txtPubVacYears.IsReadOnly = true;
                    txtRemark.IsReadOnly = true;

                    cbxkPubVacArea.IsEnabled = false;
                    cbxkAssignedObjectType.IsEnabled = false;
                    //Modified by : Sam
                    //Date : 2011-9-13
                    //For : 这里整个DataGrid Enable掉会导致滚动条不会滚动
                    //dgVacDayList.IsEnabled = false;
                    //dgWorkDayList.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entVacationSet = new T_HR_VACATIONSET();
            entVacationSet.VACATIONID = System.Guid.NewGuid().ToString().ToUpper();
            entVacationSet.VACATIONNAME = string.Empty;

            //工作日历初始值
            entVacationSet.VACATIONYEAR = DateTime.Now.Year.ToString();
            entVacationSet.COUNTYTYPE = "1";
            entVacationSet.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString();
            entVacationSet.ASSIGNEDOBJECTID = string.Empty;

            //权限
            //entVacationSet.UPDATEDATE = DateTime.Now;
            entVacationSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //entVacationSet.UPDATEDATE = System.DateTime.Now;
            entVacationSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entVacationSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entVacationSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entVacationSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entVacationSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            this.DataContext = entVacationSet;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(VacationID))
            {
                return;
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            clientAtt.GetVacationSetByIDAsync(VacationID);
        }

        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("PUBLICVACATIONFORM");
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
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region 私有方法
        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl(FormToolBar toolbar)
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar.btnEdit.Visibility = Visibility.Collapsed;
            toolbar.btnRefresh.Visibility = Visibility.Collapsed;
            toolbar.BtnView.Visibility = Visibility.Collapsed;

            toolbar.retRefresh.Visibility = Visibility.Collapsed;
            toolbar.retRead.Visibility = Visibility.Collapsed;
            toolbar.retEdit.Visibility = Visibility.Collapsed;
            toolbar.retAudit.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 
        /// </summary>
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
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 绑定DataGrid
        /// </summary>
        private void BindGrid()
        {
            string strOwnerID = string.Empty, strVacSetId = string.Empty, strCountyType = string.Empty, strDayType = string.Empty, strSortKey = string.Empty;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " STARTDATE ";
            CheckInputFilter(ref strVacSetId, ref strCountyType, ref strDayType);

            clientAtt.GetAllOutPlanDaysRdListByMultSearchAsync(strOwnerID, strVacSetId, strCountyType, strDayType, strSortKey);
        }

        /// <summary>
        /// 效验查询的参数
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strVacYear"></param>
        /// <param name="strCountyType"></param>
        private void CheckInputFilter(ref string strVacSetId, ref string strCountyType, ref string strDayType)
        {
            if (entVacationSet != null)
            {
                strVacSetId = entVacationSet.VACATIONID;
            }

            if (cbxkPubVacArea.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkPubVacArea.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    strCountyType = entDic.DICTIONARYVALUE.ToString();
                }
            }

            strDayType = string.Empty;

        }

        /// <summary>
        /// 根据起止日期，计算时长并对应行的NumericUpDown进行赋值显示结果
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="nudDays"></param>
        private void CalculateDayCount(DataGrid dgList)
        {
            try
            {
                if (FormType == FormTypes.Browse)
                {
                    return;
                }
                T_HR_OUTPLANDAYS entTemp = dgList.SelectedItem as T_HR_OUTPLANDAYS;
                if (entTemp == null)
                {
                    return;
                }

                DatePicker dtpStartDate = dgList.Columns[3].GetCellContent(dgList.SelectedItem) as DatePicker;
                DatePicker dtpEndDate;
                if (dgList == dgWorkDayList)
                {
                    dtpEndDate = dgList.Columns[4].GetCellContent(dgList.SelectedItem).FindName("dpWorkdayEnddate") as DatePicker;
                }
                else
                {
                    dtpEndDate = dgList.Columns[4].GetCellContent(dgList.SelectedItem).FindName("dpVacdayEnddate") as DatePicker;
                }
                if (dtpStartDate == null)
                {
                    return;
                }

                if (dtpEndDate == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(dtpStartDate.Text))
                {
                    return;
                }

                if (string.IsNullOrEmpty(dtpEndDate.Text))
                {
                    return;
                }

                NumericUpDown nudDays = dgList.Columns[5].GetCellContent(dgList.SelectedItem) as NumericUpDown;

                nudDays.Value = 0;
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();

                DateTime.TryParse(dtpStartDate.Text, out dtStart);
                DateTime.TryParse(dtpEndDate.Text, out dtEnd);

                decimal dDay = 0;
                dDay = CalculateDayCount(dtStart, dtEnd);
                nudDays.Value = dDay.ToDouble();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 计算放假时长
        /// </summary>
        private decimal CalculateDayCount(DateTime dtStart, DateTime dtEnd)
        {
            decimal dDays = 0;

            if (dtEnd.CompareTo(dtStart) < 0)
            {
                return dDays;
            }

            TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
            TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

            dDays = ts.Days + 1;

            return dDays;
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entOutPlanDays"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            string strPubVacName = string.Empty, strPubVacYear = string.Empty, strCountyType = string.Empty, strRemark = string.Empty;

            if (string.IsNullOrEmpty(txtPubVacName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PUBVACNAME"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("PUBVACNAME")));
                flag = false;
                return;
            }
            else
            {
                flag = true;
                strPubVacName = txtPubVacName.Text;
            }

            if (string.IsNullOrEmpty(txtPubVacYears.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PUBVACYEARS"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("PUBVACYEARS")));
                flag = false;
                return;
            }
            else
            {
                int iYear = 0;
                flag = int.TryParse(txtPubVacYears.Text, out iYear);

                if (flag == false)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PUBVACYEARS"), string.Format(Utility.GetResourceStr("REQUIREDINTEGER"), Utility.GetResourceStr("PUBVACYEARS")));
                    return;
                }
                else
                {
                    if (iYear < DateTime.Now.Year)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("PUBVACYEARS"), Utility.GetResourceStr("REQUIREDPUBVACYEARS", "PUBVACYEARS"));
                        flag = false;
                        return;
                    }
                    else
                    {
                        flag = true;
                        strPubVacYear = txtPubVacYears.Text;
                    }
                }
            }

            if (!flag)
            {
                return;
            }

            if (cbxkPubVacArea.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("COUNTYTYPE"), Utility.GetResourceStr("REQUIRED", "COUNTYTYPE"));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkPubVacArea.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("COUNTYTYPE"), Utility.GetResourceStr("REQUIRED", "COUNTYTYPE"));
                    flag = false;
                    return;
                }

                strCountyType = entDic.DICTIONARYVALUE.ToString();
            }

            if (!flag)
            {
                return;
            }

            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTTYPE"), Utility.GetResourceStr("REQUIRED", "ASSIGNEDOBJECTTYPE"));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkPubVacArea.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTTYPE"), Utility.GetResourceStr("REQUIRED", "ASSIGNEDOBJECTTYPE"));
                    flag = false;
                    return;
                }

                strCountyType = entDic.DICTIONARYVALUE.ToString();
            }

            if (!flag)
            {
                return;
            }

            if (lkAssignObject.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("REQUIRED", "ASSIGNEDOBJECTID"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
                if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY entCompany = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;

                    if (entCompany == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entCompany.COMPANYID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT entDepartment = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;

                    if (entDepartment == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entDepartment.DEPARTMENTID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST;

                    if (entPost == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entPost.POSTID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE entEmployee = lkAssignObject.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                    if (entEmployee == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entEmployee.EMPLOYEEID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
            }

            if (!flag)
            {
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entVacationSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                entVacationSet.UPDATEDATE = DateTime.Now;

            }
        }

        /// <summary>
        /// 效验提交的DataGrid的内容
        /// </summary>
        /// <param name="flag"></param>
        private void CheckSubmitOutPlanList(out bool flag)
        {
            ObservableCollection<T_HR_OUTPLANDAYS> entTemps = new ObservableCollection<T_HR_OUTPLANDAYS>();
            flag = true;

            ObservableCollection<T_HR_OUTPLANDAYS> entVacDays = dgVacDayList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;
            ObservableCollection<T_HR_OUTPLANDAYS> entWorkDays = dgWorkDayList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;

            flag = CheckDateForDataGrid(entVacDays);

            if (!flag)
            {
                return;
            }

            flag = CheckDateForDataGrid(entWorkDays);

            if (!flag)
            {
                return;
            }

            if (entVacDays != null)
            {
                foreach (T_HR_OUTPLANDAYS entVacDay in entVacDays)
                {
                    entTemps.Add(entVacDay);
                }
            }

            if (entWorkDays != null)
            {
                foreach (T_HR_OUTPLANDAYS entWorkDay in entWorkDays)
                {
                    entTemps.Add(entWorkDay);
                }
            }

            entVacationSet.T_HR_OUTPLANDAYS = entTemps;
        }

        /// <summary>
        /// 检查DataGrid内的起止日期是否合法
        /// </summary>
        /// <param name="entDays"></param>
        /// <returns></returns>
        private static bool CheckDateForDataGrid(ObservableCollection<T_HR_OUTPLANDAYS> entDays)
        {
            bool flag = false;
            flag = true;
            if (entDays != null)
            {
                foreach (T_HR_OUTPLANDAYS itemVac in entDays)
                {
                    if (itemVac.STARTDATE.Value == null)
                    {
                        flag = false;
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("STARTDATE"), Utility.GetResourceStr("REQUIRED", "STARTDATE"));
                        break;
                    }

                    if (itemVac.ENDDATE.Value == null)
                    {
                        flag = false;
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATE"), Utility.GetResourceStr("REQUIRED", "ENDDATE"));
                        break;
                    }
                    DateTime dt = new DateTime(itemVac.STARTDATE.Value.Year, itemVac.STARTDATE.Value.Month, itemVac.STARTDATE.Value.Day);
                    DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    if (dt < dtNow)
                    {
                        //flag = false;
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("STARTDATE"), "设置的开始日期包含历史日期，系统将不会处理历史日期的考勤异常，如需处理，请联系系统管理员");
                        //break;
                    }
                    if (string.IsNullOrEmpty(itemVac.ISHALFDAY))
                    {
                    }
                    else
                    {
                        if (itemVac.ISHALFDAY == "0")//非半天设置
                        {
                            if (itemVac.STARTDATE.Value > itemVac.ENDDATE.Value)
                            {
                                flag = false;
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATECOMPARE", "ENDDATE,STARTDATE"));
                                break;
                            }
                          
                        }
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    return false;
                }

                CheckSubmitForm(out flag);

                if (!flag)
                {
                    return false;
                }

                CheckSubmitOutPlanList(out flag);

                if (!flag)
                {
                    return false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (FormType == FormTypes.New)
                {
                    clientAtt.AddVacationSetAsync(entVacationSet);
                }
                else
                {
                    clientAtt.ModifyVacationSetAsync(entVacationSet);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();
            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetVacationSetByIDCompleted(object sender, GetVacationSetByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    entVacationSet = e.Result;
                    this.DataContext = entVacationSet;

                    if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                    {
                        clientOrg.GetCompanyByIdAsync(entVacationSet.ASSIGNEDOBJECTID);
                    }
                    else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                    {
                        clientOrg.GetDepartmentByIdAsync(entVacationSet.ASSIGNEDOBJECTID);
                    }
                    else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                    {
                        clientOrg.GetPostByIdAsync(entVacationSet.ASSIGNEDOBJECTID);
                    }
                    else if (entVacationSet.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
                    {
                        string[] sArray = entVacationSet.ASSIGNEDOBJECTID.Split(',');
                        clientPer.GetEmployeeByIDAsync(sArray[0].ToString());
                    }

                    BindGrid();

                    SetToolBar();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
        }

        /// <summary>
        /// 绑定DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAllOutPlanDaysRdListByMultSearchCompleted(object sender, GetAllOutPlanDaysRdListByMultSearchCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    ObservableCollection<T_HR_OUTPLANDAYS> entlist = e.Result;

                    if (entlist == null)
                    {
                        return;
                    }

                    if (entlist.Count() == 0)
                    {
                        return;
                    }

                    ObservableCollection<T_HR_OUTPLANDAYS> entVacDaylist = new ObservableCollection<T_HR_OUTPLANDAYS>();
                    ObservableCollection<T_HR_OUTPLANDAYS> entWorkDaylist = new ObservableCollection<T_HR_OUTPLANDAYS>();


                    string strVacDayType = (Convert.ToInt32(OutPlanDaysType.Vacation) + 1).ToString();
                    string strWorkDayType = (Convert.ToInt32(OutPlanDaysType.WorkDay) + 1).ToString();
                    var qv = from v in entlist
                             where v.DAYTYPE == strVacDayType
                             select v;

                    if (qv.Count() > 0)
                    {
                        qv.ForEach(itemVac =>
                        {
                            entVacDaylist.Add(itemVac);
                        });
                    }

                    dgVacDayList.ItemsSource = entVacDaylist;

                    var qw = from w in entlist
                             where w.DAYTYPE == strWorkDayType
                             select w;

                    if (qv.Count() > 0)
                    {
                        qw.ForEach(itemWork =>
                        {
                            entWorkDaylist.Add(itemWork);
                        });
                    }

                    dgWorkDayList.ItemsSource = entWorkDaylist;

                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为公司）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetCompanyByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY entCompany = e.Result as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                lkAssignObject.DataContext = entCompany;
                lkAssignObject.DisplayMemberPath = "CNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为部门）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetDepartmentByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT entDepartment = e.Result as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                lkAssignObject.DataContext = entDepartment;
                lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为岗位）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetPostByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = e.Result as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                lkAssignObject.DataContext = entPost;
                lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为员工）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPer_GetEmployeeByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = e.Result as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                lkAssignObject.DataContext = ent;
                lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddVacationSetCompleted(object sender, AddVacationSetCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result == "{SAVESUCCESSED}")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                        FormType = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyVacationSetCompleted(object sender, ModifyVacationSetCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result == "{SAVESUCCESSED}")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "PUBLICVACATIONFORM")));
                        InitParas();
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 选择考勤方案应用对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {
            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            OrganizationLookup lookup = new OrganizationLookup();
            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Company;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Department;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Post;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
                lookup.MultiSelected = true;
            }

            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ents = lookup.SelectedObj as List<ExtOrgObj>;
                if (ents == null)
                {
                    return;
                }

                if (ents.Count() == 0)
                {
                    return;
                }

                if (lookup.SelectedObjType == OrgTreeItemTypes.Company)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "CNAME";

                        entVacationSet.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString();
                        entVacationSet.ASSIGNEDOBJECTID = ent.COMPANYID;
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Department)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";

                        entVacationSet.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString();
                        entVacationSet.ASSIGNEDOBJECTID = ent.DEPARTMENTID;
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Post)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";

                        entVacationSet.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString();
                        entVacationSet.ASSIGNEDOBJECTID = ent.POSTID;
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Personnel)
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";

                        entVacationSet.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString();
                        string strIds = string.Empty;

                        foreach (ExtOrgObj item in ents)
                        {
                            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE entEmployee = item.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                            strIds += entEmployee.EMPLOYEEID + ",";
                        }

                        entVacationSet.ASSIGNEDOBJECTID = strIds;
                    }
                }

            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });   
        }

        #region DataGrid日期控制

        /// <summary>
        /// 点选公共休假的起始日期，计算时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpVacdayStartdate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormType != FormTypes.Browse || FormType != FormTypes.Audit)
            {
                CalculateDayCount(dgVacDayList); 
            }
            
        }

        /// <summary>
        /// 点选公共休假的结束日期，计算时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpVacdayEnddate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormType != FormTypes.Browse || FormType != FormTypes.Audit)
            {
                CalculateDayCount(dgVacDayList);
            }            
        }

        /// <summary>
        /// 点选工作周期的起始日期，计算时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpWorkdayStartdate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormType != FormTypes.Browse || FormType != FormTypes.Audit)
            {
                CalculateDayCount(dgWorkDayList);
            }
        }

        /// <summary>
        /// 点选公共休假的起始日期，计算时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpWorkdayEnddate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormType != FormTypes.Browse || FormType != FormTypes.Audit)
            {
                CalculateDayCount(dgWorkDayList);
            }
        }

        #endregion

        /// <summary>
        /// 添加新的列外日期记录(临时保存，不提交到数据库)
        /// </summary>
        /// <param name="dgList"></param>
        /// <param name="strDayType"></param>
        private void AddOutPlanDays(DataGrid dgList, string strDayType)
        {
            ObservableCollection<T_HR_OUTPLANDAYS> ents = new ObservableCollection<T_HR_OUTPLANDAYS>();

            if (dgList.ItemsSource != null)
            {
                ents = dgList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;
            }

            T_HR_OUTPLANDAYS entOutPlanDays = new T_HR_OUTPLANDAYS();
            entOutPlanDays.OUTPLANDAYID = System.Guid.NewGuid().ToString().ToUpper();
            entOutPlanDays.OUTPLANNAME = string.Empty;
            entOutPlanDays.DAYTYPE = strDayType;
            entOutPlanDays.STARTDATE = DateTime.Now;
            entOutPlanDays.ENDDATE = DateTime.Now;
            entOutPlanDays.DAYS = 1;
            entOutPlanDays.T_HR_VACATIONSET = entVacationSet;

            ents.Add(entOutPlanDays);
            dgList.ItemsSource = ents;
        }

        /// <summary>
        /// 删除列外日期记录(临时删除，不提交到数据库)
        /// </summary>
        private void DeleteOutPlanDays(DataGrid dgList)
        {
            if (dgList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<T_HR_OUTPLANDAYS> entList = dgList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;

            ObservableCollection<T_HR_OUTPLANDAYS> entTemps = new ObservableCollection<T_HR_OUTPLANDAYS>();
            for (int i = 0; i < dgList.SelectedItems.Count; i++)
            {
                entTemps.Add(dgList.SelectedItems[i] as T_HR_OUTPLANDAYS);
            }

            int iSel = entTemps.Count;

            for (int i = 0; i < iSel; i++)
            {
                T_HR_OUTPLANDAYS entTemp = entTemps[i] as T_HR_OUTPLANDAYS;

                for (int j = 0; j < entList.Count; j++)
                {
                    if (entList[j].OUTPLANDAYID == entTemp.OUTPLANDAYID)
                    {
                        entList.RemoveAt(j);
                    }
                }
            }

            dgList.ItemsSource = entList;
        }

        void btnVacDayNew_Click(object sender, RoutedEventArgs e)
        {
            string strDayType = (Convert.ToInt32(OutPlanDaysType.Vacation) + 1).ToString();
            AddOutPlanDays(dgVacDayList, strDayType);
        }

        void btnVacDayDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteOutPlanDays(dgVacDayList);
        }

        void btnWorkDayNew_Click(object sender, RoutedEventArgs e)
        {
            string strDayType = (Convert.ToInt32(OutPlanDaysType.WorkDay) + 1).ToString();
            AddOutPlanDays(dgWorkDayList, strDayType);
        }

        void btnWorkDayDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteOutPlanDays(dgWorkDayList);
        }

        #endregion

        private void cbxIsAdjustLeave_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
                {
                    return; 
                }
                if (dgVacDayList.SelectedItem == null)
                {
                    return;
                }

                CheckBox cb = dgVacDayList.Columns[6].GetCellContent(dgVacDayList.SelectedItem) as CheckBox;
                if (cb == null)
                {
                    return;
                }

                T_HR_OUTPLANDAYS entTemp = dgVacDayList.SelectedItem as T_HR_OUTPLANDAYS;
                if (entTemp == null)
                {
                    return;
                }

                ObservableCollection<T_HR_OUTPLANDAYS> entTemps = dgVacDayList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;
                foreach (T_HR_OUTPLANDAYS item in entTemps)
                {
                    if (item.OUTPLANDAYID == entTemp.OUTPLANDAYID)
                    {
                        if (cb.IsChecked.Value == true)
                        {
                            if (entTemp.ISADJUSTLEAVE == "False")
                            {
                                cb.IsChecked = true;
                                item.ISADJUSTLEAVE = Convert.ToInt32(IsChecked.Yes).ToString();                                
                            }
                            else
                            {
                                cb.IsChecked = false;
                                item.ISADJUSTLEAVE = Convert.ToInt32(IsChecked.No).ToString();
                            }
                        }                        
                    }
                }

                //dgVacDayList.ItemsSource = entTemps;
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message);
            }
        }

        #region 公共假期设置半天
        private void checkHaftDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                T_HR_OUTPLANDAYS entTemp = dgVacDayList.SelectedItem as T_HR_OUTPLANDAYS;
                if (entTemp == null)
                {
                    return;
                }
                CheckBox ck = dgVacDayList.Columns[4].GetCellContent(dgVacDayList.SelectedItem).FindName("checkHaftDay") as CheckBox;                
                DatePicker WorkdayEnddate = dgVacDayList.Columns[4].GetCellContent(dgVacDayList.SelectedItem).FindName("dpVacdayEnddate") as DatePicker;
                ComboBox ComboBoxHalfDay = dgVacDayList.Columns[4].GetCellContent(dgVacDayList.SelectedItem).FindName("comboHatfDay") as ComboBox;
                NumericUpDown nudVacDays = dgVacDayList.Columns[5].GetCellContent(dgVacDayList.SelectedItem).FindName("nudVacDays") as NumericUpDown;
                VacDayCheckHalfDay(ck, WorkdayEnddate, ComboBoxHalfDay, nudVacDays, entTemp);               
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message);
            }
        }

        private void dgVacDayList_LoadingRow(object sender, DataGridRowEventArgs e)
        {

            T_HR_OUTPLANDAYS obje = e.Row.DataContext as T_HR_OUTPLANDAYS;
            CheckBox ck = dgVacDayList.Columns[4].GetCellContent(e.Row).FindName("checkHaftDay") as CheckBox;
            ComboBox workComboHatfDay = dgVacDayList.Columns[4].GetCellContent(e.Row).FindName("comboHatfDay") as ComboBox;
            DatePicker WorkdayEnddate = dgVacDayList.Columns[4].GetCellContent(e.Row).FindName("dpVacdayEnddate") as DatePicker;
            NumericUpDown nudVacDays = dgVacDayList.Columns[5].GetCellContent(e.Row).FindName("nudVacDays") as NumericUpDown;

            if (obje.ISHALFDAY == "1")
            {
                ck.IsChecked = true;
                if (obje.PEROID == "0")
                {
                    workComboHatfDay.SelectedIndex = 0;//上午
                }
                else
                {
                    workComboHatfDay.SelectedIndex = 1;//下午
                }
            }
            else
            {
                ck.IsChecked = false;
            }
            VacDayCheckHalfDay(ck, WorkdayEnddate, workComboHatfDay, nudVacDays,obje);
        }

        private void comboHatfDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_HR_OUTPLANDAYS entTemp = dgVacDayList.SelectedItem as T_HR_OUTPLANDAYS;
            if (entTemp == null) return;
            CheckBox ck = dgVacDayList.Columns[4].GetCellContent(entTemp).FindName("checkHaftDay") as CheckBox;
            ComboBox ComboBoxHalfDay = dgVacDayList.Columns[4].GetCellContent(entTemp).FindName("comboHatfDay") as ComboBox;
            
            SetVacDayValue(ck, ComboBoxHalfDay,entTemp);
        }

        private void VacDayCheckHalfDay(CheckBox ck, DatePicker WorkdayEnddate
            , ComboBox ComboBoxHalfDay, NumericUpDown nudVacDays
            ,T_HR_OUTPLANDAYS obje)
        {
            if (ComboBoxHalfDay.SelectedIndex == -1)
            {
                ComboBoxHalfDay.SelectedIndex = 0;
            }

            if (ck.IsChecked.Value == true)
            {
                if (WorkdayEnddate != null)
                {
                    WorkdayEnddate.Visibility = Visibility.Collapsed;
                }
                if (ComboBoxHalfDay != null)
                {
                    ComboBoxHalfDay.Visibility = Visibility.Visible;
                }
                if (nudVacDays != null)
                {
                    nudVacDays.Value = 0.5;
                }
            }
            else
            {
                if (WorkdayEnddate != null)
                {
                    WorkdayEnddate.Visibility = Visibility.Visible;
                }
                if (ComboBoxHalfDay != null)
                {
                    ComboBoxHalfDay.Visibility = Visibility.Collapsed;
                }
                CalculateDayCount(dgVacDayList);
            }
            SetVacDayValue(ck, ComboBoxHalfDay,obje);
        }

        private void SetVacDayValue(CheckBox ck, ComboBox ComboBoxHalfDay, T_HR_OUTPLANDAYS entTemp)
        {
            if (entTemp == null)
            {
                return;
            }
            ObservableCollection<T_HR_OUTPLANDAYS> entTemps = dgVacDayList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;
            foreach (T_HR_OUTPLANDAYS item in entTemps)
            {
                if (item.OUTPLANDAYID == entTemp.OUTPLANDAYID)
                {
                    if (ck.IsChecked.Value == true)
                    {
                        item.ISHALFDAY = Convert.ToInt32(IsChecked.Yes).ToString();
                        item.ENDDATE = item.STARTDATE.Value.AddDays(1).AddMilliseconds(-1);
                        item.DAYS = Convert.ToDecimal(0.5);
                        if (ComboBoxHalfDay.SelectedIndex == 0)//上午
                        {
                            item.PEROID = "0";
                        }
                        else
                        {
                            item.PEROID = "1";
                        }
                    }
                    else
                    {
                        item.ISHALFDAY = Convert.ToInt32(IsChecked.No).ToString();
                    }
                }
            }
        }
        #endregion

        #region "工作日设置半天"
        private void workCheckHaftDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
                {
                    return;
                }
                T_HR_OUTPLANDAYS obje = dgWorkDayList.SelectedItem as T_HR_OUTPLANDAYS;
                if (obje == null)
                {
                    return;
                }

                CheckBox ck = dgWorkDayList.Columns[4].GetCellContent(dgWorkDayList.SelectedItem).FindName("workCheckHaftDay") as CheckBox;
                DatePicker WorkdayEnddate = dgWorkDayList.Columns[4].GetCellContent(dgWorkDayList.SelectedItem).FindName("dpWorkdayEnddate") as DatePicker;
                ComboBox workComboHatfDay = dgWorkDayList.Columns[4].GetCellContent(dgWorkDayList.SelectedItem).FindName("workComboHatfDay") as ComboBox;
                NumericUpDown nudVacDays = dgWorkDayList.Columns[5].GetCellContent(dgWorkDayList.SelectedItem).FindName("nudWorkDays") as NumericUpDown;

                WorkDayCheckHalfDay(ck, WorkdayEnddate, workComboHatfDay, nudVacDays, obje);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message);
            }
        }
        
        private void dgWorkDayList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_HR_OUTPLANDAYS obje = e.Row.DataContext as T_HR_OUTPLANDAYS;
            CheckBox ck = dgWorkDayList.Columns[4].GetCellContent(e.Row).FindName("workCheckHaftDay") as CheckBox;
            ComboBox vacComboHatfDay = dgWorkDayList.Columns[4].GetCellContent(e.Row).FindName("workComboHatfDay") as ComboBox;
            DatePicker vacdayEnddate = dgWorkDayList.Columns[4].GetCellContent(e.Row).FindName("dpWorkdayEnddate") as DatePicker;
            NumericUpDown nudVacDays = dgWorkDayList.Columns[5].GetCellContent(e.Row).FindName("nudWorkDays") as NumericUpDown;

            if (obje.ISHALFDAY == "1")
            {
                ck.IsChecked = true;
                if (obje.PEROID == "0")
                {
                    vacComboHatfDay.SelectedIndex = 0;//上午
                }
                else
                {
                    vacComboHatfDay.SelectedIndex = 1;//下午
                }
            }
            else
            {
                ck.IsChecked = false;
            }
            WorkDayCheckHalfDay(ck, vacdayEnddate, vacComboHatfDay, nudVacDays, obje);
        }

        private void workComboHatfDay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_HR_OUTPLANDAYS entTemp = dgWorkDayList.SelectedItem as T_HR_OUTPLANDAYS;
            if (entTemp == null) return;
            CheckBox ck = dgWorkDayList.Columns[4].GetCellContent(entTemp).FindName("workCheckHaftDay") as CheckBox;
            ComboBox workComboHatfDay = dgWorkDayList.Columns[4].GetCellContent(entTemp).FindName("workComboHatfDay") as ComboBox;

            SetWorkDayValue(ck, workComboHatfDay,entTemp);
        }
        private void WorkDayCheckHalfDay(CheckBox ck, DatePicker WorkdayEnddate
            , ComboBox workComboHatfDay, NumericUpDown nudVacDays
            , T_HR_OUTPLANDAYS entTemp)
        {
           
            if (workComboHatfDay.SelectedIndex == -1)
            {
                workComboHatfDay.SelectedIndex = 0;
            }
            if (ck.IsChecked.Value)
            {
                if (WorkdayEnddate != null)
                {
                    WorkdayEnddate.Visibility = Visibility.Collapsed;
                }
                if (workComboHatfDay != null)
                {
                    workComboHatfDay.Visibility = Visibility.Visible;
                }
                if (nudVacDays != null)
                {
                    nudVacDays.Value = 0.5;
                }
            }
            else
            {
                if (WorkdayEnddate != null)
                {
                    WorkdayEnddate.Visibility = Visibility.Visible;
                }
                if (workComboHatfDay != null)
                {
                    workComboHatfDay.Visibility = Visibility.Collapsed;
                }
                CalculateDayCount(dgWorkDayList);
            }

            SetWorkDayValue(ck, workComboHatfDay, entTemp);
        }

        private void SetWorkDayValue(CheckBox ck, ComboBox workComboHatfDay, T_HR_OUTPLANDAYS entTemp)
        {
            if (entTemp == null)
            {
                return;
            }
            ObservableCollection<T_HR_OUTPLANDAYS> entTemps = dgWorkDayList.ItemsSource as ObservableCollection<T_HR_OUTPLANDAYS>;
            foreach (T_HR_OUTPLANDAYS item in entTemps)
            {
                if (item.OUTPLANDAYID == entTemp.OUTPLANDAYID)
                {
                    if (ck.IsChecked.Value == true)
                    {
                        item.ISHALFDAY = Convert.ToInt32(IsChecked.Yes).ToString();
                        item.ENDDATE = item.STARTDATE.Value.AddDays(1).AddMilliseconds(-1);
                        item.DAYS =Convert.ToDecimal(0.5);
                        if (workComboHatfDay.SelectedIndex == 0)//上午
                        {
                            item.PEROID = "0";
                        }
                        else
                        {
                            item.PEROID = "1";
                        }
                    }
                    else
                    {
                        item.ISHALFDAY = Convert.ToInt32(IsChecked.No).ToString();
                    }
                }
            }
        }
        #endregion



    }
}

