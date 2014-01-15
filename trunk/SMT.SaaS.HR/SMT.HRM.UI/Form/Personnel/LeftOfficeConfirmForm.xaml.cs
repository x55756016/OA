using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AuditControl;
//using SMT.Saas.Tools.FBServiceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
using SMT.Saas.Tools.DailyManagementWS;
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class LeftOfficeConfirmForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }
        public T_HR_LEFTOFFICECONFIRM LeftOfficeConfirm { get; set; }
        public List<BorrowThing> BThings;
        public string createUserName;
        public string ownerUserName;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        PersonnelServiceClient client = new PersonnelServiceClient();
        SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient oClient = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
        SMT.Saas.Tools.FBServiceWS.FBServiceClient fbClient = new SMT.Saas.Tools.FBServiceWS.FBServiceClient();
        ObservableCollection<T_FB_PERSONACCOUNT> perCount = new ObservableCollection<T_FB_PERSONACCOUNT>();
        //SmtOADocumentAdminClient oaClient = new SmtOADocumentAdminClient();
        SMT.Saas.Tools.FlowWFService.ServiceClient flowClient = new Saas.Tools.FlowWFService.ServiceClient();

        DailyManagementServicesClient DMClient = new DailyManagementServicesClient();
        private string conformID { get; set; }
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public bool needsubmit = false;//提交审核,用于判断是否需要调用提交方法
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        public LeftOfficeConfirmForm(FormTypes formType, string strID)
        {
            InitializeComponent();
            FormType = formType;
            conformID = strID;
            InitParas(strID);
        }
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public LeftOfficeConfirmForm()
        {
            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            }
            if (Application.Current.Resources["DictionaryConverter"] == null)
            {
                Application.Current.Resources.Add("DictionaryConverter", new DictionaryConverter());
            }
            InitializeComponent();
            FormType = FormTypes.New;
            conformID = "";
            InitParas(conformID);
        }

        public LeftOfficeConfirmForm(FormTypes formType, T_HR_LEFTOFFICE leftOffice)
        {
            InitializeComponent();
            FormType = formType;
            LeftOfficeConfirm = new T_HR_LEFTOFFICECONFIRM();
            LeftOfficeConfirm.T_HR_LEFTOFFICE = new T_HR_LEFTOFFICE();
            LeftOfficeConfirm.T_HR_LEFTOFFICE.DIMISSIONID = leftOffice.DIMISSIONID;
            LeftOfficeConfirm.CONFIRMID = Guid.NewGuid().ToString();
            LeftOfficeConfirm.EMPLOYEEID = leftOffice.T_HR_EMPLOYEE.EMPLOYEEID;
            LeftOfficeConfirm.OWNERID = leftOffice.T_HR_EMPLOYEE.EMPLOYEEID;
            LeftOfficeConfirm.OWNERPOSTID = leftOffice.OWNERPOSTID;
            LeftOfficeConfirm.OWNERDEPARTMENTID = leftOffice.OWNERDEPARTMENTID;
            LeftOfficeConfirm.OWNERCOMPANYID = leftOffice.OWNERCOMPANYID;
            LeftOfficeConfirm.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            LeftOfficeConfirm.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            LeftOfficeConfirm.CREATEDATE = DateTime.Now;
            LeftOfficeConfirm.CONFIRMDATE = DateTime.Now;
            this.dpConfirmDate.Text = DateTime.Now.ToString("yyyy-MM-dd");//奇怪
            LeftOfficeConfirm.EMPLOYEECNAME = leftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME;
            LeftOfficeConfirm.APPLYDATE = leftOffice.APPLYDATE;
            LeftOfficeConfirm.LEFTOFFICEDATE = leftOffice.LEFTOFFICEDATE;
            LeftOfficeConfirm.EMPLOYEEPOSTID = leftOffice.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID;
            LeftOfficeConfirm.LEFTOFFICECATEGORY = leftOffice.LEFTOFFICECATEGORY;
            LeftOfficeConfirm.LEFTOFFICEREASON = leftOffice.LEFTOFFICEREASON;
            LeftOfficeConfirm.REMARK = leftOffice.REMARK;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> listCompany = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            txtCompany.Text = (from ent in listCompany
                               where ent.COMPANYID == leftOffice.OWNERCOMPANYID
                               select ent).FirstOrDefault().CNAME;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> listPost = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            txtPost.Text = (from ent in listPost
                            where ent.POSTID == leftOffice.OWNERPOSTID
                            select ent).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
            T_HR_EMPLOYEE ep = new T_HR_EMPLOYEE();
            ep.EMPLOYEECNAME = LeftOfficeConfirm.EMPLOYEECNAME;
            ep.EMPLOYEEID = LeftOfficeConfirm.EMPLOYEEID;
            createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            InitParas("fromLeftoffice");
            conformID = "fromLeftoffice";
            EnabledControl();
            SetToolBar();
            this.DataContext = LeftOfficeConfirm;
            lkEmployeeName.DataContext = ep;
           // fbClient.GetLeavingUserAsync(LeftOfficeConfirm.EMPLOYEEID);
            //oaClient.GetEmployeeNotReturnListByUserIdAsync(LeftOfficeConfirm.EMPLOYEEID);
            //获取员工借还款
            GetPersonAccountData();
        }
        private void InitParas(string strID)
        {
            client.GetLeftOfficeConfirmByIDCompleted += new EventHandler<GetLeftOfficeConfirmByIDCompletedEventArgs>(client_GetLeftOfficeConfirmByIDCompleted);
            client.LeftOfficeConfirmUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_LeftOfficeConfirmUpdateCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            client.LeftOfficeConfirmAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_LeftOfficeConfirmAddCompleted);
            fbClient.GetLeavingUserCompleted += new EventHandler<SMT.Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs>(fbClient_GetLeavingUserCompleted);
            //oaClient.GetEmployeeNotReturnListByUserIdCompleted += new EventHandler<GetEmployeeNotReturnListByUserIdCompletedEventArgs>(oaClient_GetEmployeeNotReturnListByUserIdCompleted);
            flowClient.IsExistFlowDataByUserIDCompleted += new EventHandler<Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs>(flowClient_IsExistFlowDataByUserIDCompleted);
            oClient.GetPostByIdCompleted += new EventHandler<Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs>(oClient_GetPostByIdCompleted);

            DMClient.GetPersonAccountListByMultSearchCompleted += new EventHandler<GetPersonAccountListByMultSearchCompletedEventArgs>(DMClient_GetPersonAccountListByMultSearchCompleted);
            this.Loaded += new RoutedEventHandler(LeftOfficeConfirmForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnabledControl();
                dpConfirmDate.IsEnabled = false;
                dpStopPaymentDate.IsEnabled = false;
            }
            */
            #endregion
        }

      

        void DMClient_GetPersonAccountListByMultSearchCompleted(object sender, GetPersonAccountListByMultSearchCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                //DtBorrowMoney.ClearValue
                if (e.Error != null)
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null && e.Result.Count > 0)
                        {
                            DtBorrowMoney.ItemsSource = e.Result.Where(c => c.BORROWMONEY > 0).ToList();
                            perCount = e.Result;
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未知错误"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        void LeftOfficeConfirmForm_Loaded(object sender, RoutedEventArgs e)
        {
            //重载提交按钮-提交先保存
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnabledControl();
                dpConfirmDate.IsEnabled = false;
                dpStopPaymentDate.IsEnabled = false;
            }
            #endregion
            if (FormType != FormTypes.New)
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetLeftOfficeConfirmByIDAsync(conformID);
            }
            else
            {
                if (string.IsNullOrEmpty(conformID))
                {
                    LeftOfficeConfirm = new T_HR_LEFTOFFICECONFIRM();
                    LeftOfficeConfirm.CONFIRMID = Guid.NewGuid().ToString();
                    LeftOfficeConfirm.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    LeftOfficeConfirm.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    LeftOfficeConfirm.CREATEDATE = DateTime.Now;
                    LeftOfficeConfirm.CONFIRMDATE = DateTime.Now;
                    //dpConfirmDate.Text = DateTime.Now.ToString();
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                    SetToolBar();
                }
            }
        }

        //by luojie 
        //工具栏提交按钮的重载方法
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            needsubmit = true;
            isSubmit = true;

            Save();
        }

        #region 完成事件
        /// <summary>
        /// 获取流程中是否有未处理完的单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void flowClient_IsExistFlowDataByUserIDCompleted(object sender, Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs e)
        {



            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.Result),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    List<T_HR_POST> dictp = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
                    if (dictp != null)
                    {
                        var ent = from c in dictp
                                  where c.FATHERPOSTID == LeftOfficeConfirm.OWNERPOSTID
                                  select c;
                        if (ent.Count() > 0)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("注意，存在下级岗位"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                    SaveConfirm();
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        /// <summary>
        /// 获取创建人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeToEngineCompleted(object sender, GetEmployeeToEngineCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {

                if (e.Result == null)
                {
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    List<EmployeeContactWays> emp = e.Result.ToList();
                    var tmp = emp.Where(s => s.EMPLOYEEID == LeftOfficeConfirm.CREATEUSERID).FirstOrDefault();
                    if (tmp != null)
                    {
                        createUserName = tmp.EMPLOYEENAME;
                    }
                    tmp = emp.Where(s => s.EMPLOYEEID == LeftOfficeConfirm.EMPLOYEEID).FirstOrDefault();
                    if (tmp != null)
                    {
                        ownerUserName = tmp.EMPLOYEENAME;

                    }

                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 未归还物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void oaClient_GetEmployeeNotReturnListByUserIdCompleted(object sender, GetEmployeeNotReturnListByUserIdCompletedEventArgs e)
        //{
        //    BThings = new List<BorrowThing>();
        //    BorrowThing thing;
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        //  DtBorrowMoney.DataContext = e.Result;
        //        if (e.Result != null)
        //        {
        //            foreach (var item in e.Result)
        //            {
        //                thing = new BorrowThing();
        //                string[] tmp = item.Split(new char[] { ',' });
        //                thing.TITLE = tmp[1];
        //                thing.TYPE = tmp[0];
        //                BThings.Add(thing);
        //            }
        //        }
        //        DtBorrowThing.ItemsSource = BThings.ToList();
        //    }
        //}

        /// <summary>
        /// 未还款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fbClient_GetLeavingUserCompleted(object sender, SMT.Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                DtBorrowMoney.ItemsSource = e.Result;

            }
        }
        /// <summary>
        /// 添加确认单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_LeftOfficeConfirmAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.ProgressBar);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        /// <summary>
        /// 修改确认单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_LeftOfficeConfirmUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() == "Edit" && !isSubmit)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    isSubmit = false;
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                toSubmit();
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 提交
        /// </summary>
        private void toSubmit()
        {
            ///by luojie 转至提交按钮原来的方法
            if (needsubmit)
            {
                try
                {
                    
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.ManualSubmit();
                    BackToSubmit();
                }
                catch (Exception ex)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                }

            }
            if (closeFormFlag)
            {
                RefreshUI(RefreshedTypes.Close);
            }
            RefreshUI(RefreshedTypes.All);
        }
        /// <summary>
        ///     回到提交前的状态 by luojie
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            //isSubmit = false;

            //隐藏工具栏 不允许二次提交
            #region 隐藏entitybrowser中的toolbar按钮
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }
            #endregion
            //if (refreshType == RefreshedTypes.CloseAndReloadData)
            //{
            //    //refreshType = RefreshedTypes.AuditInfo;
            //    refreshType = RefreshedTypes.HideProgressBar;
            //}

        }

        /// <summary>
        /// 获取确认单信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetLeftOfficeConfirmByIDCompleted(object sender, GetLeftOfficeConfirmByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                LeftOfficeConfirm = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    LeftOfficeConfirm.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                }
                if (LeftOfficeConfirm.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    dpStopPaymentDate.IsEnabled = false;
                    dpConfirmDate.IsEnabled = false;
                }
                //  this.DataContext = LeftOfficeConfirm.T_HR_LEFTOFFICE;
                this.DataContext = LeftOfficeConfirm;

                //显示所属公司和岗位
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> listCompany = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                txtCompany.Text = (from ent in listCompany
                                   where ent.COMPANYID == LeftOfficeConfirm.OWNERCOMPANYID
                                   select ent).FirstOrDefault().CNAME;
                List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> listPost = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
                //txtPost.Text = (from ent in listPost
                //                where ent.POSTID == LeftOfficeConfirm.OWNERPOSTID
                //                select ent).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                var postName = (from ent in listPost
                                where ent.POSTID == LeftOfficeConfirm.OWNERPOSTID
                                select ent).FirstOrDefault();
                if (postName != null)
                {
                    txtPost.Text = postName.T_HR_POSTDICTIONARY.POSTNAME;
                }
                else
                {
                    oClient.GetPostByIdAsync(LeftOfficeConfirm.OWNERPOSTID);
                }
                //显示姓名
                T_HR_EMPLOYEE etmp = new T_HR_EMPLOYEE();
                etmp.EMPLOYEECNAME = LeftOfficeConfirm.EMPLOYEECNAME;
                etmp.EMPLOYEEID = LeftOfficeConfirm.EMPLOYEEID;
                lkEmployeeName.DataContext = etmp;

                //有离职申请 禁用控件
                if (LeftOfficeConfirm.T_HR_LEFTOFFICE != null)
                {
                    EnabledControl();
                }

                if (LeftOfficeConfirm.STOPPAYMENTDATE != null)
                {
                    dpStopPaymentDate.Text = LeftOfficeConfirm.STOPPAYMENTDATE.ToString();
                }
                //if (LeftOfficeConfirm.CONFIRMDATE != null)
                //{
                //    dpConfirmDate.Text = LeftOfficeConfirm.CONFIRMDATE.ToString();
                //}
               // fbClient.GetLeavingUserAsync(LeftOfficeConfirm.EMPLOYEEID);
                //oaClient.GetEmployeeNotReturnListByUserIdAsync(LeftOfficeConfirm.EMPLOYEEID);

                System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                CreateUserIDs.Add(LeftOfficeConfirm.CREATEUSERID);
                CreateUserIDs.Add(LeftOfficeConfirm.EMPLOYEEID);
                client.GetEmployeeToEngineAsync(CreateUserIDs);
                //获取员工借还款
                GetPersonAccountData();
            }
        }
        void oClient_GetPostByIdCompleted(object sender, Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                txtPost.Text = e.Result.T_HR_POSTDICTIONARY.POSTNAME;
            }
        }
        #endregion
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_LEFTOFFICE", LeftOffice.OWNERID,
            //        LeftOffice.OWNERPOSTID, LeftOffice.OWNERDEPARTMENTID, LeftOffice.OWNERCOMPANYID);

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
                //ToolbarItems = Utility.CreateFormEditButton("T_HR_LEFTOFFICECONFIRM", LeftOfficeConfirm.OWNERID,
                //    LeftOfficeConfirm.OWNERPOSTID, LeftOfficeConfirm.OWNERDEPARTMENTID, LeftOfficeConfirm.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("LEFTOFFICECONFIRM");
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
                    closeFormFlag = true;
                    Save();
                    //Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = "详细信息",
            //    Tooltip = "详细信息"
            //};
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

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_LEFTOFFICECONFIRM Info)
        {
            List<object> ObjectList = new List<object>();
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


            //string postLevelName = string.Empty;
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVLE" && s.DICTIONARYVALUE == employeepost.POSTLEVEL).FirstOrDefault();
            //postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

           // SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "CHECKSTATE", "1", checkState));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "LEFTOFFICECATEGORY", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYVALUE.ToString() : "0", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "LEFTOFFICECATEGORY", "1", "辞职"));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "EMPLOYEEID", Info.EMPLOYEEID, Info.EMPLOYEECNAME));
            // AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "POSTLEVEL", employeepost.POSTLEVEL.ToString(), employeepost.POSTLEVEL.ToString()));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "EMPLOYEECNAME", Info.EMPLOYEECNAME, Info.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "OWNER", Info.EMPLOYEECNAME, Info.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "CREATEUSERNAME", createUserName, createUserName));

            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_LEFTOFFICECONFIRM", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            List<T_FB_PERSONACCOUNT> objPe=new List<T_FB_PERSONACCOUNT>();
            if (perCount != null && perCount.Count > 0)
            {
                objPe = perCount.ToList();
            }
            foreach (var item in objPe)
            {
                AutoList.Add(basedataForChild("T_FB_PERSONACCOUNT", "SPECIALBORROWMONEY", item.SPECIALBORROWMONEY.ToString(), item.SPECIALBORROWMONEY.ToString(), item.PERSONACCOUNTID));
                AutoList.Add(basedataForChild("T_FB_PERSONACCOUNT", "SIMPLEBORROWMONEY", item.SIMPLEBORROWMONEY.ToString(), item.SIMPLEBORROWMONEY.ToString(), item.PERSONACCOUNTID));
                AutoList.Add(basedataForChild("T_FB_PERSONACCOUNT", "BACKUPBORROWMONEY", item.BACKUPBORROWMONEY.ToString(), item.BACKUPBORROWMONEY.ToString(), item.PERSONACCOUNTID));
                ObjectList.Add(item);
            }
            string a = mx.TableToXml(Info, ObjectList, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedataForChild(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);
            return ad;
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
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", ownerUserName);
            para.Add("EMPLOYEEID", LeftOfficeConfirm.EMPLOYEEID);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", ownerUserName);



            Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("LEFTOFFICECATEGORY", (cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            para2.Add("LEFTOFFICECATEGORY","辞职");

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_LEFTOFFICECONFIRM>(LeftOfficeConfirm, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, LeftOfficeConfirm);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", LeftOfficeConfirm.EMPLOYEEID);
            paraIDs.Add("CreatePostID", LeftOfficeConfirm.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", LeftOfficeConfirm.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", LeftOfficeConfirm.OWNERCOMPANYID);
            paraIDs.Add("CreateUserName", LeftOfficeConfirm.EMPLOYEECNAME);

            //Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICECONFIRM", LeftOfficeConfirm.CONFIRMID, strXmlObjectSource,paraIDs);
            //  Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICE", LeftOffice.DIMISSIONID);
            if (LeftOfficeConfirm.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICECONFIRM", LeftOfficeConfirm.CONFIRMID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICECONFIRM", LeftOfficeConfirm.CONFIRMID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //Utility.UpdateCheckState("T_HR_LEFTOFFICE", "DIMISSIONID", LeftOffice.DIMISSIONID, args);
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    //跟新岗位信息
                    //  client.updateAllpostByemployeeIDAsync(LeftOfficeConfirm.T_HR_LEFTOFFICE.T_HR_EMPLOYEE.EMPLOYEEID);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (LeftOfficeConfirm.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            LeftOfficeConfirm.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            // client.LeftOfficeConfirmUpdateAsync(LeftOfficeConfirm, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (LeftOfficeConfirm != null)
                state = LeftOfficeConfirm.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        private bool CheckData()
        {
            if (string.IsNullOrEmpty(LeftOfficeConfirm.EMPLOYEEID))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            //if (cbxEmployeeType.SelectedItem == null)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "LEFTOFFICECATEGORY"),
            //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (string.IsNullOrEmpty(dpStopPaymentDate.Text))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "STOPPAYMENTDATE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
           // if (string.IsNullOrEmpty(dpConfirmDate.Text))
            if (string.IsNullOrEmpty(Convert.ToString(LeftOfficeConfirm.CONFIRMDATE)))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CONFIRMDATE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            return true;
        }

        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            //flowClient.IsExistFlowDataByUserIDAsync(LeftOfficeConfirm.EMPLOYEEID,LeftOfficeConfirm.EMPLOYEEPOSTID);
            //传递离职人的岗位ID
            if (CheckData())
            {
                flowClient.IsExistFlowDataByUserIDAsync(LeftOfficeConfirm.EMPLOYEEID, LeftOfficeConfirm.OWNERPOSTID);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SaveConfirm()
        {
            //判断有没有借款
            if (DtBorrowMoney.ItemsSource != null)
            {
                List<T_FB_PERSONACCOUNT> bors = DtBorrowMoney.ItemsSource as List<T_FB_PERSONACCOUNT>;
                if (bors != null)
                {
                    //2012-8-27
                    foreach (var temp in bors)
                    {
                        if (temp.BORROWMONEY > 0)
                        {
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未处理借款"),
                                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }
                    //if (bors.Count() > 0)
                    //{
                    //    RefreshUI(RefreshedTypes.HideProgressBar);
                    //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未处理借款"),
                    //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //    return;
                    //}

                }
            }
            //2012-09-06
            //确认时间明明有，但是提示没有，在这里再次赋值
            //dpConfirmDate.Text = DateTime.Now.ToString();
            //if (string.IsNullOrEmpty(LeftOfficeConfirm.EMPLOYEEID))
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
            //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return;
            //}
            //if (cbxEmployeeType.SelectedItem == null)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "LEFTOFFICECATEGORY"),
            //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return;
            //}
            //if (string.IsNullOrEmpty(dpConfirmDate.Text))
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CONFIRMDATE"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return;
            //}
            string confirDate = Convert.ToString(dpConfirmDate.Text);
            string stopDate = Convert.ToString(dpStopPaymentDate.Text);
            string applyDate = Convert.ToString(dpApplyDate.Text);
            string leftDate = Convert.ToString(dpLeftDate.Text);
           // MessageBox.Show(confirDate + "***" + stopDate + "***" + applyDate + "***" + leftDate);
            if (!string.IsNullOrEmpty(confirDate))
            {
                LeftOfficeConfirm.CONFIRMDATE = Convert.ToDateTime(confirDate);
            }
            LeftOfficeConfirm.STOPPAYMENTDATE = Convert.ToDateTime(stopDate);
            if (!string.IsNullOrEmpty(applyDate) && !string.IsNullOrEmpty(applyDate.Trim()))
            {
                LeftOfficeConfirm.APPLYDATE = Convert.ToDateTime(applyDate);
            }
            if (!string.IsNullOrEmpty(leftDate))
            {
                LeftOfficeConfirm.LEFTOFFICEDATE = Convert.ToDateTime(leftDate);
            }
            LeftOfficeConfirm.LEFTOFFICECATEGORY = "1";// (cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
            LeftOfficeConfirm.REMARK = txtLeftRemark.Text;
            LeftOfficeConfirm.LEFTOFFICEREASON = txtLeftReason.Text;
            if (!string.IsNullOrEmpty(leftDate))
            {
                LeftOfficeConfirm.LEFTOFFICEDATE = Convert.ToDateTime(leftDate);
            }

            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
                {
                    LeftOfficeConfirm.UPDATEDATE = System.DateTime.Now;
                    LeftOfficeConfirm.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; ;

                    client.LeftOfficeConfirmUpdateAsync(LeftOfficeConfirm, "Edit");
                }
                if (FormType == FormTypes.New)
                {
                    client.LeftOfficeConfirmAddAsync(LeftOfficeConfirm);
                }
            };
            if (LeftOfficeConfirm.LEFTOFFICEDATE.HasValue && LeftOfficeConfirm.STOPPAYMENTDATE.HasValue)
            {
                com.SelectionBox(Utility.GetResourceStr("确认"), "请确认" + LeftOfficeConfirm.EMPLOYEECNAME + "的离职确认日期是" +
                        LeftOfficeConfirm.CONFIRMDATE.Value.ToString("yyyy年MM月dd日") +
                        "，停薪日期是" + LeftOfficeConfirm.STOPPAYMENTDATE.Value.ToString("yyyy年MM月dd日") + "（当天），薪资计算截止至当天", ComfirmWindow.titlename, Result);
                RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.All);
            }

        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
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

        private void GridPagerThing_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridPagerMoney_Click(object sender, RoutedEventArgs e)
        {

        }

        void EnabledControl()
        {
            lkEmployeeName.IsEnabled = false;
           // cbxEmployeeType.IsEnabled = false;
            dpLeftDate.IsEnabled = false;
            dpApplyDate.IsEnabled = false;
            txtLeftReason.IsReadOnly = true;
            txtLeftRemark.IsReadOnly = true;
            DtBorrowMoney.IsReadOnly = true;
            DtBorrowThing.IsReadOnly = true;
            //dpConfirmDate.IsEnabled = false;
            //dpStopPaymentDate.IsEnabled = false;
        }
        public class BorrowThing
        {
            private string type;
            private string title;
            public string TYPE
            {
                get { return type; }
                set { type = value; }
            }
            public string TITLE
            {
                get { return title; }
                set { title = value; }
            }
        }


        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            //oaClient.DoClose();
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

        private void lkEmployeeName_FindClick(object sender, EventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                DtBorrowMoney.ItemsSource = null;
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ents = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ents != null && ents.Count > 0)
                {
                    //岗位
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)ents.FirstOrDefault().ParentObject;
                    string postid = post.ObjectID;
                    this.txtPost.Text = post.ObjectName;

                    //部门
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;

                    //公司
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    this.txtCompany.Text = corp.CNAME;

                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = ents.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    lkEmployeeName.DataContext = ent;
                    ownerUserName = ent.EMPLOYEECNAME;
                    LeftOfficeConfirm.EMPLOYEECNAME = ent.EMPLOYEECNAME;
                    if (ent.T_HR_EMPLOYEEPOST != null)
                    {
                        LeftOfficeConfirm.EMPLOYEEPOSTID = ent.T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().EMPLOYEEPOSTID;
                    }
                    LeftOfficeConfirm.OWNERCOMPANYID = corpid;
                    LeftOfficeConfirm.OWNERDEPARTMENTID = deptid;
                    LeftOfficeConfirm.OWNERPOSTID = postid;
                    LeftOfficeConfirm.OWNERID = ent.EMPLOYEEID;
                    LeftOfficeConfirm.EMPLOYEEID = ent.EMPLOYEEID;
                   // fbClient.GetLeavingUserAsync(LeftOfficeConfirm.EMPLOYEEID);
                    //oaClient.GetEmployeeNotReturnListByUserIdAsync(LeftOfficeConfirm.EMPLOYEEID);
                    //获取员工借还款
                    GetPersonAccountData();
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        //xiedx
        //获取相应人员的借款
        public void GetPersonAccountData()
        {
            if (LeftOfficeConfirm == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(LeftOfficeConfirm.OWNERCOMPANYID))
            {
                return;
            }
            //if (leftOffice.T_HR_EMPLOYEE == null)
            //{
            //    return;
            //}
            T_FB_PERSONACCOUNT temp = new T_FB_PERSONACCOUNT();
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(LeftOfficeConfirm.OWNERCOMPANYID))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(OWNERCOMPANYID) ";
                paras.Add(LeftOfficeConfirm.OWNERCOMPANYID);
            }
            if (!string.IsNullOrEmpty(LeftOfficeConfirm.EMPLOYEEID))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(OWNERID) ";
                paras.Add(LeftOfficeConfirm.EMPLOYEEID);
            }
            DMClient.GetPersonAccountListByMultSearchAsync(filter, paras, "OWNERID");
        }
        
    }
}
