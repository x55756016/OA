using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendMonthlyBalanceAudit : BaseForm, IEntityEditor, IAudit
    {
        public FormTypes FormType { get; set; }
        public string BalanceObjectType { get; set; }
        public string BalanceObjectValue { get; set; }
        public string CheckState { get; set; }
        public string MonthlyBalanceBatchId { get; set; }
        public T_HR_ATTENDMONTHLYBATCHBALANCE AttendMonthlyBatchBalance { get; set; }
        private ObservableCollection<T_HR_ATTENDMONTHLYBALANCE> entAMBList { get; set; }
        private readonly AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private readonly OrganizationServiceClient orgClient = new OrganizationServiceClient();
        private bool needsubmit = false;

        private List<ToolbarItem> _toolbarItems = new List<ToolbarItem>();
        readonly BasePage basePage = new BasePage();

        #region 初始化
        /// <summary>
        /// 根据主键索引获取指定的考勤月度结算批量审核信息
        /// </summary>
        /// <param name="formtype"></param>
        /// <param name="strMonthlyBatchId"></param>
        public AttendMonthlyBalanceAudit(FormTypes formtype, string strMonthlyBalanceBatchId)
        {
            InitializeComponent();
            FormType = formtype;
            MonthlyBalanceBatchId = strMonthlyBalanceBatchId;
            basePage.GetEntityLogo("T_HR_ATTENDMONTHLYBALANCE");
            this.Loaded += new RoutedEventHandler(AttendMonthlyBalanceAudit_Loaded);
        }

        /// <summary>
        /// 根据分配对象，年，月以及审核状态获取指定的考勤月度结算批量审核信息
        /// </summary>
        /// <param name="formtype"></param>
        /// <param name="strType"></param>
        /// <param name="strValue"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strCheckState"></param>
        public AttendMonthlyBalanceAudit(FormTypes formtype, string strType, string strValue, decimal dBalanceYear, decimal dBalanceMonth, string strCheckState)
        {
            InitializeComponent();
            SMT.HRM.UI.Utility.CheckResourceConverter();
            FormType = formtype;
            BalanceObjectType = strType;
            BalanceObjectValue = strValue;
            CheckState = strCheckState;
            txtBalanceYear.Text = dBalanceYear.ToString();
            nudBalanceMonth.Value = double.Parse(dBalanceMonth.ToString());
            basePage.GetEntityLogo("T_HR_ATTENDMONTHLYBALANCE");
            this.Loaded += new RoutedEventHandler(AttendMonthlyBalanceAudit_Loaded);
        }

        void AttendMonthlyBalanceAudit_Loaded(object sender, RoutedEventArgs e)
        {
            InitForm();
            InitEvents();
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件
        /// </summary>
        private void InitEvents()
        {
            txtBalanceYear.IsEnabled = false;
            nudBalanceMonth.IsEnabled = false;
            cbxkAssignedObjectType.IsEnabled = false;
            lkAssignObject.IsEnabled = false;

            orgClient.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(orgClient_GetCompanyByIdCompleted);
            orgClient.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(orgClient_GetDepartmentByIdCompleted);
            orgClient.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(orgClient_GetPostByIdCompleted);

            clientAtt.GetAttendMonthlyBatchBalanceByMultSearchCompleted += new EventHandler<GetAttendMonthlyBatchBalanceByMultSearchCompletedEventArgs>(clientAtt_GetAttendMonthlyBatchBalanceByMultSearchCompleted);
            clientAtt.GetAttendMonthlyBalanceRdListForAuditCompleted += new EventHandler<GetAttendMonthlyBalanceRdListForAuditCompletedEventArgs>(clientAtt_GetAttendMonthlyBalanceRdListForAuditCompleted);

            clientAtt.AddAttendMonthlyBatchBalanceCompleted += new EventHandler<AddAttendMonthlyBatchBalanceCompletedEventArgs>(clientAtt_AddAttendMonthlyBatchBalanceCompleted);

            clientAtt.GetAttendMonthlyBatchBalanceByIDCompleted += new EventHandler<GetAttendMonthlyBatchBalanceByIDCompletedEventArgs>(clientAtt_GetAttendMonthlyBatchBalanceByIDCompleted);

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
        }        

        /// <summary>
        /// 加载页面数据
        /// </summary>
        private void InitForm()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (string.IsNullOrEmpty(MonthlyBalanceBatchId))
            {
                decimal dBalanceYear = 0, dBalanceMonth = 0;
                string strObjType = string.Empty;
                CheckInputFilter(ref dBalanceYear, ref dBalanceMonth);
                clientAtt.GetAttendMonthlyBatchBalanceByMultSearchAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, BalanceObjectType, BalanceObjectValue, dBalanceYear, dBalanceMonth, CheckState);
            }
            else
            {
                clientAtt.GetAttendMonthlyBatchBalanceByIDAsync(MonthlyBalanceBatchId);
            }
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        private void BindGrid()
        {
            string strOwnerId, strSortKey = string.Empty;
            decimal dBalanceYear = 0, dBalanceMonth = 0;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = "BALANCEYEAR, BALANCEMONTH";
            CheckInputFilter(ref dBalanceYear, ref dBalanceMonth);
            //pageIndex = dataPager.PageIndex;
            //pageIndex = dataPager.PageIndex;
            pageIndex = 0;
            pageSize = 0;

            string strTemp = string.Empty;
            if (cbxkAssignedObjectType.Items.Count() > 0)
            {
                switch (AttendMonthlyBatchBalance.BALANCEOBJECTTYPE)
                {
                    case "1":
                        strTemp = "Company";
                        break;
                    case "2":
                        strTemp = "Department";
                        break;
                    case "3":
                        strTemp = "Post";
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(strTemp))
            {
                return;
            }

            clientAtt.GetAttendMonthlyBalanceRdListForAuditAsync(strTemp, AttendMonthlyBatchBalance.BALANCEOBJECTID, strOwnerId, CheckState, dBalanceYear, dBalanceMonth, strSortKey, pageIndex, pageSize, pageCount);
        }

        /// <summary>
        /// 效验输入内容
        /// </summary>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strObjType"></param>
        private void CheckInputFilter(ref decimal dBalanceYear, ref decimal dBalanceMonth)
        {
            if (!string.IsNullOrEmpty(txtBalanceYear.Text.Trim()))
            {
                decimal.TryParse(txtBalanceYear.Text, out dBalanceYear);
            }

            decimal.TryParse(nudBalanceMonth.Value.ToString(), out dBalanceMonth);
        }

        private void SetToolBar()
        {
            _toolbarItems = Utility.CreateFormEditButton("T_HR_ATTENDMONTHLYBATCHBALANCE", AttendMonthlyBatchBalance.OWNERID,
                    AttendMonthlyBatchBalance.OWNERPOSTID, AttendMonthlyBatchBalance.OWNERDEPARTMENTID, AttendMonthlyBatchBalance.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }
        #endregion

        #region 事件
        /// <summary>
        /// 加载公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY entCompany = e.Result;

                lkAssignObject.DataContext = entCompany;
                lkAssignObject.DisplayMemberPath = "CNAME";
                if (entCompany != null)
                {
                    AttendMonthlyBatchBalance.BALANCEOBJECTNAME = entCompany.CNAME;

                    if (entCompany.COMPANYID != AttendMonthlyBatchBalance.OWNERCOMPANYID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.CompanyID == entCompany.COMPANYID);
                        if (temp != null)
                        {
                            AttendMonthlyBatchBalance.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            AttendMonthlyBatchBalance.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            AttendMonthlyBatchBalance.OWNERPOSTID = temp.FirstOrDefault().PostID;

                            BindGrid();
                        }
                    }
                    else
                    {
                        BindGrid(); 
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 加载部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT entDepartment = e.Result;

                lkAssignObject.DataContext = entDepartment;
                lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                if (entDepartment != null)
                {
                    AttendMonthlyBatchBalance.BALANCEOBJECTNAME = entDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

                    if (entDepartment.DEPARTMENTID != AttendMonthlyBatchBalance.OWNERDEPARTMENTID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.DepartmentID == entDepartment.DEPARTMENTID);
                        if (temp != null)
                        {
                            AttendMonthlyBatchBalance.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            AttendMonthlyBatchBalance.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            AttendMonthlyBatchBalance.OWNERPOSTID = temp.FirstOrDefault().PostID;

                            BindGrid();
                        }
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 加载岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = e.Result;
                lkAssignObject.DataContext = entPost;
                lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                if (entPost != null)
                {
                    AttendMonthlyBatchBalance.BALANCEOBJECTNAME = entPost.T_HR_POSTDICTIONARY.POSTNAME;

                    if (entPost.POSTID != AttendMonthlyBatchBalance.OWNERPOSTID)
                    {
                        var temp = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Where(c => c.PostID == entPost.POSTID);
                        if (temp != null)
                        {
                            AttendMonthlyBatchBalance.OWNERCOMPANYID = temp.FirstOrDefault().CompanyID;
                            AttendMonthlyBatchBalance.OWNERDEPARTMENTID = temp.FirstOrDefault().DepartmentID;
                            AttendMonthlyBatchBalance.OWNERPOSTID = temp.FirstOrDefault().PostID;

                            BindGrid();
                        }
                    }
                    else
                    {
                        MessageBox.Show("提交人岗位跟结算人岗位相同，不能审核。");
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据相关条件加载员工考勤月度结算信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendMonthlyBalanceRdListForAuditCompleted(object sender, GetAttendMonthlyBalanceRdListForAuditCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                entAMBList = e.Result;

                dgAMBList.ItemsSource = entAMBList;
                dataPager.PageCount = e.pageCount;

                if (entAMBList == null)
                {
                    return;
                }

                if (entAMBList.Count() == 0)
                {
                    return;
                }

                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendMonthlyBatchBalanceByIDCompleted(object sender, GetAttendMonthlyBatchBalanceByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                AttendMonthlyBatchBalance = e.Result;
                this.DataContext = AttendMonthlyBatchBalance;


                BalanceObjectType = AttendMonthlyBatchBalance.BALANCEOBJECTTYPE;
                BalanceObjectValue = AttendMonthlyBatchBalance.BALANCEOBJECTID;
                CheckState = AttendMonthlyBatchBalance.CHECKSTATE;
                txtBalanceYear.Text = AttendMonthlyBatchBalance.BALANCEYEAR.Value.ToString();
                nudBalanceMonth.Value = AttendMonthlyBatchBalance.BALANCEMONTH.Value.ToDouble();
                SetBalanceObject();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据相关条件，获取月度考勤批量审核信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendMonthlyBatchBalanceByMultSearchCompleted(object sender, GetAttendMonthlyBatchBalanceByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                AttendMonthlyBatchBalance = e.Result;

                if (AttendMonthlyBatchBalance == null)
                {
                    AttendMonthlyBatchBalance = new T_HR_ATTENDMONTHLYBATCHBALANCE();
                    AttendMonthlyBatchBalance.MONTHLYBATCHID = System.Guid.NewGuid().ToString().ToUpper();

                    AttendMonthlyBatchBalance.BALANCEYEAR = decimal.Parse(txtBalanceYear.Text);
                    AttendMonthlyBatchBalance.BALANCEMONTH = decimal.Parse(nudBalanceMonth.Value.ToString());
                    AttendMonthlyBatchBalance.BALANCEDATE = DateTime.Now;
                    AttendMonthlyBatchBalance.BALANCEOBJECTTYPE = BalanceObjectType;
                    AttendMonthlyBatchBalance.BALANCEOBJECTID = BalanceObjectValue;
                    AttendMonthlyBatchBalance.BALANCEOBJECTNAME = string.Empty;
                    AttendMonthlyBatchBalance.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);
                    AttendMonthlyBatchBalance.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    AttendMonthlyBatchBalance.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    AttendMonthlyBatchBalance.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    AttendMonthlyBatchBalance.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    AttendMonthlyBatchBalance.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    AttendMonthlyBatchBalance.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    AttendMonthlyBatchBalance.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    AttendMonthlyBatchBalance.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    AttendMonthlyBatchBalance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    AttendMonthlyBatchBalance.CREATEDATE = DateTime.Now;
                    AttendMonthlyBatchBalance.REMARK = string.Empty;
                    AttendMonthlyBatchBalance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    AttendMonthlyBatchBalance.UPDATEDATE = DateTime.Now;

                }


                this.DataContext = AttendMonthlyBatchBalance;
                SetBalanceObject();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 提交审核时，加载月度考勤对应机构及其类型
        /// </summary>
        private void SetBalanceObject()
        {
            if (cbxkAssignedObjectType.Items.Count() > 0)
            {
                for (int i = 0; i < cbxkAssignedObjectType.Items.Count(); i++)
                {
                    var entDic = cbxkAssignedObjectType.Items[i] as T_SYS_DICTIONARY;

                    if (entDic == null)
                    {
                        continue;
                    }

                    if (entDic.DICTIONARYVALUE == null)
                    {
                        continue;
                    }

                    if (entDic.DICTIONARYVALUE.Value.ToString() == AttendMonthlyBatchBalance.BALANCEOBJECTTYPE)
                    {
                        cbxkAssignedObjectType.SelectedIndex = i;
                        break;
                    }
                }

                switch (AttendMonthlyBatchBalance.BALANCEOBJECTTYPE)
                {
                    case "1":
                        orgClient.GetCompanyByIdAsync(BalanceObjectValue);
                        break;
                    case "2":
                        orgClient.GetDepartmentByIdAsync(BalanceObjectValue);
                        break;
                    case "3":
                        orgClient.GetPostByIdAsync(BalanceObjectValue);
                        break;
                }
            }
        }

        void clientAtt_AddAttendMonthlyBatchBalanceCompleted(object sender, AddAttendMonthlyBatchBalanceCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (needsubmit == true)
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.ManualSubmit();                    
                    needsubmit = false;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }       

        /// <summary>
        /// 选取分配对象
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

            OrganizationLookupForm lookup = new OrganizationLookupForm();
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

            lookup.SelectedClick += (obj, ev) =>
            {
                lkAssignObject.DataContext = lookup.SelectedObj;

                if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)
                {
                    lkAssignObject.DisplayMemberPath = "CNAME";
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY entCompany = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    BalanceObjectValue = entCompany.COMPANYID;
                }
                else if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT entDepartment = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                    BalanceObjectValue = entDepartment.DEPARTMENTID;
                }
                else if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_POST)
                {
                    lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = lkAssignObject.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                    BalanceObjectValue = entPost.POSTID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });


            InitForm();
        }

        /// <summary>
        /// 加载图片列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAMBList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            basePage.SetRowLogo(dgAMBList, e.Row, "T_HR_ATTENDMONTHLYBALANCE");
            TextBlock tborder = dgAMBList.Columns[1].GetCellContent(e.Row).FindName("tbOrder") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {            
            if (AttendMonthlyBatchBalance.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                return;
            }

            if (entAMBList == null)
            {
                return;
            }

            if (entAMBList.Count() == 0)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            needsubmit = true;

            clientAtt.AddAttendMonthlyBatchBalanceAsync(AttendMonthlyBatchBalance);

            RefreshUI(RefreshedTypes.ProgressBar);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("AUDITMONTHLYBALANCES");
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            return;
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return _toolbarItems;
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

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_ATTENDMONTHLYBATCHBALANCE Info)
        {
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ownerCompany = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(s => s.COMPANYID == Info.OWNERCOMPANYID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ownerDepartment = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(s => s.DEPARTMENTID == Info.OWNERDEPARTMENTID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_POST ownerPost = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(s => s.POSTID == Info.OWNERPOSTID).FirstOrDefault();
            string ownerCompanyName = string.Empty;
            string ownerDepartmentName = string.Empty;
            string ownerPostName = string.Empty;
            if (ownerCompany != null)
            {
                ownerCompanyName = ownerCompany.CNAME;
            }
            if (ownerDepartment != null)
            {
                ownerDepartmentName = ownerDepartment.T_HR_DEPARTMENTDICTIONARY == null ? "" : ownerDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }
            if (ownerPost != null)
            {
                ownerPostName = ownerPost.T_HR_POSTDICTIONARY == null ? "" : ownerPost.T_HR_POSTDICTIONARY.POSTNAME;
            }

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            T_SYS_DICTIONARY objecttype = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "CHECKSTATE", "1", checkState));

            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "BALANCEOBJECTTYPE", Info.BALANCEOBJECTTYPE, objecttype == null ? "" : objecttype.DICTIONARYNAME.ToString()));
            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "BALANCEOBJECTID", Info.BALANCEOBJECTID, lkAssignObject.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "BALANCEUNIT", Info.BALANCEOBJECTID, lkAssignObject.TxtLookUp.Text));

            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_ATTENDMONTHLYBATCHBALANCE", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));

            string a = mx.TableToXml(Info, entAMBList, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras["CreateCompanyID"] = AttendMonthlyBatchBalance.OWNERCOMPANYID;
            paras["CreateDepartmentID"] = AttendMonthlyBatchBalance.OWNERDEPARTMENTID;
            paras["CreatePostID"] = AttendMonthlyBatchBalance.OWNERPOSTID;
            paras["CreateUserID"] = AttendMonthlyBatchBalance.OWNERID;

            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, AttendMonthlyBatchBalance);
            Utility.SetAuditEntity(entity, "T_HR_ATTENDMONTHLYBATCHBALANCE", AttendMonthlyBatchBalance.MONTHLYBATCHID, strXmlObjectSource, paras);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string strCheckState = "";
            string strEditState = Convert.ToInt32(EditStates.UnActived).ToString();
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    strCheckState = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    strCheckState = Utility.GetCheckState(CheckStates.Approved);
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    strCheckState = Utility.GetCheckState(CheckStates.UnApproved);
                    strEditState = Convert.ToInt32(EditStates.Canceled).ToString();
                    break;
            }

            //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDMONTHLYBALANCE")));

            //AttendMonthlyBatchBalance.EDITSTATE = strEditState;
            //AttendMonthlyBatchBalance.CHECKSTATE = strCheckState;

            //RefreshUI(RefreshedTypes.HideProgressBar);
            //RefreshUI(RefreshedTypes.AuditInfo);
            //RefreshUI(RefreshedTypes.All);

            if (strCheckState == Utility.GetCheckState(CheckStates.Approved))
            {
                switch (AttendMonthlyBatchBalance.BALANCEOBJECTTYPE)
                {
                    case "1":
                        clientAtt.CalculateEmployeeAttendanceYearlyByCompanyIDAsync(AttendMonthlyBatchBalance.BALANCEYEAR.ToString(), AttendMonthlyBatchBalance.BALANCEOBJECTID);
                        break;
                    case "2":
                        clientAtt.CalculateEmployeeAttendanceYearlyByDepartmentIDAsync(AttendMonthlyBatchBalance.BALANCEYEAR.ToString(), AttendMonthlyBatchBalance.BALANCEOBJECTID);
                        break;
                    case "3":
                        clientAtt.CalculateEmployeeAttendanceYearlyByPostIDAsync(AttendMonthlyBatchBalance.BALANCEYEAR.ToString(), AttendMonthlyBatchBalance.BALANCEOBJECTID);
                        break;
                }
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (AttendMonthlyBatchBalance != null)
                state = AttendMonthlyBatchBalance.CHECKSTATE;
            return state;
        }

        #endregion

    }
}
