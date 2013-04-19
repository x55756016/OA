/// <summary>
/// Log No.： 1
/// Modify Desc： 添加滚动条，分隔线，绑定奖金类型，审核功能，组织结构样式
/// Modifier： 冉龙军
/// Modify Date： 2010-08-11
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PerformanceWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.Windows.Media;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Text.RegularExpressions;
namespace SMT.HRM.UI.Form.Performance
{
    public partial class EditPerformance : BaseForm, IEntityEditor, IAudit
    {
        private PersonnelServiceClient client; //人员服务
        private OrganizationServiceClient orgClient;//机构服务
        private PerformanceServiceClient kpiClient;//绩效考核服务
        public FormTypes FormType { get; set; }//窗口状态
        public T_HR_SUMPERFORMANCERECORD SumPerformance { get; set; }//当前绩效考核

        public List<string> personIDList = new List<string>();//绩效考核中的原始绩效人员ID列表
        public T_HR_PERFORMANCERECORD SelectedPerson { get; set; }//当前选择的汇总人员的关联表实体

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;//所有公司
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;//所有部门
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions; //所有岗位
        private bool auditsign = false, signCancel = false;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private bool listCheckIsEnable = true;
        private bool isClose = false;

        /// <summary>
        /// 构造EditGroupPerson
        /// </summary>
        /// <param name="type">窗口状态</param>
        /// <param name="randomGroup">抽查组实体</param>
        public EditPerformance(FormTypes type, T_HR_SUMPERFORMANCERECORD sumPerformance)
        {
            FormType = type;
            this.SumPerformance = sumPerformance;
            InitializeComponent();
            SetPerformanceInfoEnable(false);
            SetReviewInfoEnable(false);
            InitPara();
        }

        /// <summary>
        /// 构造EditGroupPerson
        /// </summary>
        /// <param name="type">窗口状态</param>
        /// <param name="randomGroup">抽查组实体</param>
        public EditPerformance(FormTypes type, string SUMID)
        {
            FormType = type;
            kpiClient = new PerformanceServiceClient();
            SumPerformance = new T_HR_SUMPERFORMANCERECORD();
            SumPerformance.SUMID = SUMID;
            kpiClient.GetSumPerformanceRecordByIDAsync(SUMID);
            InitializeComponent();
            SetPerformanceInfoEnable(false);
            SetReviewInfoEnable(false);
            InitPara();
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
                this.SumPerformance.SUMSTART = DateTime.Now;
                this.SumPerformance.SUMEND = DateTime.Now.AddDays(1);
            }
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_KPIRECORDCOMPLAIN", Complain.OWNERID,
            //        Complain.OWNERPOSTID, Complain.OWNERDEPARTMENTID, Complain.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse) return;
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_SUMPERFORMANCERECORD", SumPerformance.OWNERID,
                    SumPerformance.OWNERPOSTID, SumPerformance.OWNERDEPARTMENTID, SumPerformance.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_SUMPERFORMANCERECORD>(SumPerformance, "HR");
            Utility.SetAuditEntity(entity, "T_HR_SUMPERFORMANCERECORD", SumPerformance.SUMID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            // 1s 冉龙军
            //auditsign = true;
            // 1e
            Utility.UpdateCheckState("T_HR_SUMPERFORMANCERECORD", "SUMID", SumPerformance.SUMID, args);
            string state = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    // 1s 冉龙军
                    auditsign = true;
                    // 1e
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    // 1s 冉龙军
                    auditsign = true;
                    // 1e
                    break;
            }
            // 1s 冉龙军
            if (SumPerformance.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                auditsign = true;
            }
            // 1e
            SumPerformance.CHECKSTATE = state;
            //SumPerformance.CHECKSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            kpiClient.UpdateSumPerformanceAsync(this.SumPerformance);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (SumPerformance != null)
                state = SumPerformance.CHECKSTATE;
            return state;
        }
        #endregion

        public void Cancel()
        {
            signCancel = true;
            Save();
        }

        #region 所有的方法

        /// <summary>
        /// 获取页面数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditPerformance_Loaded(object sender, RoutedEventArgs e)
        {
            kpiClient.GetPerformanceAllBySumIDAsync(SumPerformance.SUMID);
            //  LoadData();
            BindTree();
            //获取部门信息
            orgClient.GetDepartmentByIdAsync(SumPerformance.SUMDEPTID);
            //获取汇总人和审批人信息
            ObservableCollection<string> ids = new ObservableCollection<string>();
            ids.Add(SumPerformance.SUMPERSONID);
            if (SumPerformance.REVIEWERID != null)
                ids.Add(SumPerformance.REVIEWERID);
            client.GetEmployeeByIDsAsync(ids);
        }

        /// <summary>
        /// 设置绩效汇总信息的可用性
        /// </summary>
        /// <param name="p"></param>
        private void SetPerformanceInfoEnable(bool p)
        {
            txtSumName.IsEnabled = p;
            dpSumStart.IsEnabled = p;
            dpSumEnd.IsEnabled = p;
            txtBaseMoney.IsEnabled = false;
            txtSumRemark.IsEnabled = p;

            cboSumType.IsEnabled = p;
            cboBaseMoneyType.IsEnabled = p;

            listCheckIsEnable = p;
        }

        /// <summary>
        /// 设置审批信息的可用性
        /// </summary>
        /// <param name="p"></param>
        private void SetReviewInfoEnable(bool p)
        {
            txtReviewRemark.IsEnabled = p;
            // 1s
            //cboReviewStatus.IsEnabled = false;
            cboReviewStatus.IsEnabled = p;
            // 1e
        }

        /// <summary>
        /// 构造页面触发事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                client = new PersonnelServiceClient();
                client.GetEmployeePagingCompleted += new EventHandler<GetEmployeePagingCompletedEventArgs>(client_GetEmployeePagingCompleted);
                client.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(client_GetEmployeeByIDsCompleted);

                kpiClient = new PerformanceServiceClient();
                kpiClient.GetSumPerformanceRecordByIDCompleted += new EventHandler<GetSumPerformanceRecordByIDCompletedEventArgs>(kpiClient_GetSumPerformanceRecordByIDCompleted);
                kpiClient.GetPerformanceAllBySumIDCompleted += new EventHandler<GetPerformanceAllBySumIDCompletedEventArgs>(client_GetPerformanceAllBySumIDCompleted);
                kpiClient.AddSumPerformanceCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddSumPerformanceCompleted);
                kpiClient.DeleteSumPerformanceCompleted += new EventHandler<DeleteSumPerformanceCompletedEventArgs>(client_DeleteSumPerformanceCompleted);
                kpiClient.UpdateSumPerformanceAndSumCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateSumPerformanceAndSumCompleted);
                kpiClient.UpdateSumPerformanceCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateSumPerformanceCompleted);

                orgClient = new OrganizationServiceClient();
                orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
                orgClient.GetDepartmentByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs>(orgClient_GetDepartmentByIdCompleted);

                DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

                this.Loaded += new RoutedEventHandler(EditPerformance_Loaded);

                if (FormType == FormTypes.New)
                {
                    SumPerformance = new T_HR_SUMPERFORMANCERECORD();
                    SumPerformance.SUMID = Guid.NewGuid().ToString();
                    SumPerformance.T_HR_PERFORMANCERECORD = new ObservableCollection<T_HR_PERFORMANCERECORD>();
                    SumPerformance.SUMPERSONID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SumPerformance.SUMDEPTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    SumPerformance.SUMDATE = DateTime.Now;
                    SumPerformance.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    SetPerformanceInfoEnable(true);
                    SetToolBar();
                }
                else
                {
                    SetReviewInfoEnable(true);
                    kpiClient.GetSumPerformanceRecordByIDAsync(this.SumPerformance.SUMID);
                }
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void kpiClient_GetSumPerformanceRecordByIDCompleted(object sender, GetSumPerformanceRecordByIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }
                else
                {
                    SumPerformance = e.Result as T_HR_SUMPERFORMANCERECORD;
                    if (FormType == FormTypes.Edit)
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                    }
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                }
            }
        }

        /// <summary>
        /// 读取列表信息
        /// </summary>
        void LoadData()
        {
            //绑定汇总信息
            LayoutRoot.DataContext = SumPerformance;
            if (SumPerformance.SUMTYPE != null)
            {

                // 1s 冉龙军
                //try { cboSumType.SelectedIndex = int.Parse(SumPerformance.SUMTYPE); }
                //catch { }
                try
                {
                    cboSumType.SelectedIndex = int.Parse(SumPerformance.SUMTYPE);
                }
                catch
                {
                    throw new Exception();
                }
                // 1e

            }
            // 1s 冉龙军
            //if (SumPerformance.CREATEUSERID != null)
            //{
            //    //try { cboBaseMoneyType.SelectedIndex = int.Parse(SumPerformance.CREATEUSERID.Trim()); }
            //    //catch { }
            //}
            if (SumPerformance.AWARDTYPE != null)
            {
                try
                {
                    cboBaseMoneyType.SelectedIndex = int.Parse(SumPerformance.AWARDTYPE.Trim());
                }
                catch
                {
                    throw new Exception();
                }
            }
            // 1e
            if (SumPerformance.CHECKSTATE != null)
            {
                // 1s 冉龙军
                //try { cboReviewStatus.SelectedIndex = int.Parse(SumPerformance.CHECKSTATE); }
                //catch { }
                try
                {
                    cboReviewStatus.SelectedIndex = int.Parse(SumPerformance.CHECKSTATE);
                }
                catch
                {
                    throw new Exception();
                }
                // 1e
            }

            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        break;
                    case "Department":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        break;
                    case "Post":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = "Post";
                        sValue = post.POSTID;
                        break;
                }
            }
            client.GetEmployeePagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 在内存中增加汇总考核人员
        /// </summary>
        private void AddPersonPerformance()
        {
            //获取当前人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = DtGrid.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //将该人员加入汇总
            SumPerformance.T_HR_PERFORMANCERECORD.Add(SelectedPerson);
            //添加到已关联的ID列表中
            personIDList.Add(ent.EMPLOYEEID);
        }

        /// <summary>
        /// 删除内存中的汇总考核人员
        /// </summary>
        private void DeletePersonPerformance()
        {
            //获取当前人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = DtGrid.SelectedItem as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //将该人员从汇总中删除
            SumPerformance.T_HR_PERFORMANCERECORD.Remove(SelectedPerson);
            //从已关联的ID列表中删除
            personIDList.Remove(ent.EMPLOYEEID);
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                return;
            }
            if (string.IsNullOrEmpty(txtSumName.Text.Trim()))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SUMNAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "SUMNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (cboSumType.SelectedIndex != -1)
                SumPerformance.SUMTYPE = cboSumType.SelectedIndex.ToString();
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SUMTYPE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "SUMTYPE"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(dpSumStart.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTTIME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "STARTTIME"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (string.IsNullOrEmpty(dpSumEnd.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDTIME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ENDTIME"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            if (cboBaseMoneyType.SelectedIndex != -1)
            {
                SumPerformance.AWARDTYPE = cboBaseMoneyType.SelectedIndex.ToString();
            }
            if (SumPerformance.T_HR_PERFORMANCERECORD.Count <= 0)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NEEDSUMPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NEEDSUMPERSON"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SumPerformance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            SumPerformance.UPDATEDATE = DateTime.Now;
            SumPerformance.SUMCOUNT = SumPerformance.T_HR_PERFORMANCERECORD.Count;
            if (FormType == FormTypes.New)
            {

                //所属
                SumPerformance.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SumPerformance.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SumPerformance.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                SumPerformance.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SumPerformance.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SumPerformance.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                SumPerformance.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SumPerformance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SumPerformance.CREATEDATE = DateTime.Now;
                kpiClient.AddSumPerformanceAsync(this.SumPerformance);
            }
            else if (FormType == FormTypes.Edit)
            {
                kpiClient.UpdateSumPerformanceAsync(this.SumPerformance);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }

        /// <summary>
        /// 在三个T_HR_PERFORMANCERECORD的列表中，找出PERSONID为employeeId的实体
        /// </summary>
        /// <param name="p">personID</param>
        /// <returns></returns>
        private T_HR_PERFORMANCERECORD GetPersonFromList(string employeeId)
        {
            if (SumPerformance.T_HR_PERFORMANCERECORD != null)
                foreach (T_HR_PERFORMANCERECORD person in this.SumPerformance.T_HR_PERFORMANCERECORD.ToList())
                {
                    if (person.APPRAISEEID.Equals(employeeId))
                        return person;
                }
            else
                SumPerformance.T_HR_PERFORMANCERECORD = new ObservableCollection<T_HR_PERFORMANCERECORD>();
            //没有找到就新建
            T_HR_PERFORMANCERECORD ent = new T_HR_PERFORMANCERECORD();
            ent.PERFORMANCEID = Guid.NewGuid().ToString();
            ent.T_HR_SUMPERFORMANCERECORD = this.SumPerformance;
            ent.APPRAISEEID = employeeId;
            return ent;
        }

        #endregion 所有的方法

        #region 所有的事件
        /// <summary>
        /// 获取列表人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeePagingCompleted(object sender, GetEmployeePagingCompletedEventArgs e)
        {
            List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> list = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
                DtGrid.SelectedIndex = 0;
                DtGrid.SelectedItem = DtGrid.SelectedItem;
                //DtGrid.CurrentColumn = DtGrid.Columns[1]; 
            }
        }
        /// <summary>
        /// 获取人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {
            List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> list = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    lblSumPerson.Text = ((SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)e.Result[0]).EMPLOYEECNAME;
                    //判断是否有审批人
                    if (e.Result.Count > 1)
                        lblReviewPerson.Text = ((SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)e.Result[1]).EMPLOYEECNAME;
                }
            }
        }

        /// <summary>
        /// 获取所有人员绩效考核后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetPerformanceAllBySumIDCompleted(object sender, GetPerformanceAllBySumIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.SumPerformance.T_HR_PERFORMANCERECORD = e.Result;
                personIDList.Clear();
                foreach (T_HR_PERFORMANCERECORD groupPerson in this.SumPerformance.T_HR_PERFORMANCERECORD)
                {
                    personIDList.Add(groupPerson.APPRAISEEID);
                }
            }
        }

        /// <summary>
        /// 获取人员绩效考核后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetPerformanceRecordByIDCompleted(object sender, GetPerformanceRecordByIDCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                SelectedPerson = new T_HR_PERFORMANCERECORD();
                SelectedPerson.PERFORMANCEID = Guid.NewGuid().ToString();
                SelectedPerson.T_HR_SUMPERFORMANCERECORD = this.SumPerformance;
            }
            else
                SelectedPerson = e.Result;
        }

        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 点击列表的单元格改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)grid.SelectedItems[0];
                SelectedPerson = GetPersonFromList(employee.EMPLOYEEID);
                //kpiClient.GetPerformanceRecordByIDAsync(employee.EMPLOYEEID);
            }
        }

        /// <summary>
        /// 读取每行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //获取该行人员信息
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = e.Row.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //获取该行的CheckBox
            CheckBox ckh = DtGrid.Columns[0].GetCellContent(e.Row).FindName("chkMyChkBox") as CheckBox;
            //是否已经是抽查组人员
            if (personIDList != null && personIDList.IndexOf(ent.EMPLOYEEID) != -1)
            {
                ckh.IsChecked = true;
            }
            //增加CheckBox事件
            ckh.Click += new RoutedEventHandler(chkMyChkBox_Click);
        }

        /// <summary>
        /// 翻页条事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 每行的CheckBox事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkMyChkBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (DtGrid.ItemsSource != null)
            {
                //判断是在内存中增加还是删除
                if (chk.IsChecked.Value)
                    AddPersonPerformance();
                else
                    DeletePersonPerformance();
            }

        }

        /// <summary>
        /// 添加完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddSumPerformanceCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "RANDOMGROUPPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                this.FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);

                if (signCancel)
                {
                    signCancel = false;
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    RefreshUI(RefreshedTypes.All);
                    entBrowser.Close();
                }
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);

        }
        /// <summary>
        /// 获取所有岗位后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetDepartmentByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    lblSumDept.Text = ((SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)e.Result).T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                }
            }
        }
        /// <summary>
        /// 删除完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteSumPerformanceCompleted(object sender, DeleteSumPerformanceCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //if (e.Error.Message == "Repetition")
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                //}
                //else
                //{
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                //}
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "RANDOMGROUPPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_UpdateSumPerformanceAndSumCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "RANDOMGROUPPERSON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 审批完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_UpdateSumPerformanceCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.Error.Message == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "RANDOMGROUPNAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                if (FormType == FormTypes.Edit)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED", "SUMPERFORMANCERECORD"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    if (auditsign)
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
         Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITNOTPASS"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITNOTPASS"),
         Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                }
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);

                if (signCancel)
                {
                    signCancel = false;
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    RefreshUI(RefreshedTypes.All);
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.Close();
                }
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        private void txtBaseMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            string regx = "^[0-9]+(.[0-9]{2})?$";
            if (!Regex.IsMatch(txtBaseMoney.Text.Trim(), regx))
            {
                txtBaseMoney.Text = "";
            }
        }

        #endregion 所有的事件

        #region 树形控件的操作
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("ORGTREESYSCompanyInfo"))
            {
                // allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });

                UICache.CreateCache("ORGTREESYSCompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("ORGTREESYSDepartmentInfo", allDepartments);

                BindCompany();

                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("ORGTREESYSPostInfo", allPositions);
                BindPosition();
            }
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["ORGTREESYSCompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["ORGTREESYSDepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }

            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.DataContext = tmpOrg;

                    //状态在未生效和撤消中时背景色为红色
                    SolidColorBrush brush = new SolidColorBrush();
                    if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                    {
                        brush.Color = Colors.Red;
                        item.Foreground = brush;
                    }
                    else
                    {
                        brush.Color = Colors.Black;
                        item.Foreground = brush;
                    }
                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company;
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
                              select c;
                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company;
                        treeOrganization.Items.Add(item);

                        TopCompany.Add(tmpOrg);
                    }
                }
            }
            //开始递归
            foreach (var topComp in TopCompany)
            {
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            allPositions = Application.Current.Resources["ORGTREESYSPostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (allPositions != null)
            {
                BindPosition();
            }
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为公司
                item.Tag = OrgTreeItemTypes.Company;
                FatherNode.Items.Add(item);

                if (lsCompany.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
            //绑定公司下的部门
            foreach (var childDepartment in lsDepartment)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                item.DataContext = childDepartment;
                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为部门
                item.Tag = OrgTreeItemTypes.Department;
                FatherNode.Items.Add(item);

                if (lsDepartment.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post;
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType.ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        if (tmpDep != null)
                        {
                            if (tmpDep.DEPARTMENTID == parentID)
                                return item;
                        }
                        break;
                }

            }
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        /// <summary>
        /// 点击公司事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            LoadData();
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            // 1s 冉龙军
            //return Utility.GetResourceStr("RANDOMGROUP");
            return Utility.GetResourceStr("PERFORMANCELIST");
            // 1e
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
                    isClose = false;
                    break;
                case "1":
                    Cancel();
                    isClose = true;
                    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("RANDOMGROUPINFO"),
                Tooltip = Utility.GetResourceStr("RANDOMGROUPINFO")
            };
            items.Add(item);
            return items;
        }
        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    List<ToolbarItem> items = new List<ToolbarItem>();
        //    if (FormType != FormTypes.Browse)
        //    {
        //        items = Utility.CreateFormSaveButton();
        //    }

        //    return items;
        //}

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

        private void cboBaseMoneyType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // 1s 冉龙军
            //if (cboBaseMoneyType.SelectedIndex != -1)
            //    txtBaseMoney.IsEnabled = true;
            //else
            //    txtBaseMoney.IsEnabled = false;
            if (cboBaseMoneyType.SelectedIndex == 0)
                txtBaseMoney.IsEnabled = true;
            else
                txtBaseMoney.IsEnabled = false;
            // 1e
        }
        #endregion
    }
}
