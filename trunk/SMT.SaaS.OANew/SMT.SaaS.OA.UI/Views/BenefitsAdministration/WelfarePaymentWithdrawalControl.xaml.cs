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
using SMT.SaaS.OA.UI.Class;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.SalaryWS;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class WelfarePaymentWithdrawalControl : BaseForm,IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private SalaryServiceClient ssc;//薪资结算
        private SmtOADocumentAdminClient BenefitsAdministration;
        private T_OA_WELFAREDISTRIBUTEUNDO WelfarePaymentWithdrawalInfo;
        private T_OA_WELFAREDISTRIBUTEMASTER distributeMaster = null;
        private FormTypes actions; //操作类型
        private ObservableCollection<T_HR_EMPLOYEEADDSUM> emploeeaddsumList = new ObservableCollection<T_HR_EMPLOYEEADDSUM>();//薪资
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private string checkState = ((int)CheckStates.Approved).ToString();
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private ObservableCollection<string> delteWelfarePaymentDetail = new ObservableCollection<string>();
        private string beingWithdrawnId = string.Empty;//福利发放撤销ID
        private string welfareInfoID = string.Empty;//福利发放ID
        private string Year = string.Empty;
        private string Month = string.Empty;
        private List<T_OA_WELFAREDISTRIBUTEDETAIL> detailTemps = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
        public V_WelfareProvision WelfareProvision = new V_WelfareProvision();
        private PersonnelServiceClient client;
        private V_EMPLOYEEPOST employeepost;
        private SelectWelfarePaymentRecords welfarePaymentRecordsForm;

        public T_OA_WELFAREDISTRIBUTEUNDO InfoObj
        {
            get { return WelfarePaymentWithdrawalInfo; }
            set
            {
                this.DataContext = value;
                WelfarePaymentWithdrawalInfo = value;
            }
        }
        #endregion

        #region 福利发放撤销构造函数
        public WelfarePaymentWithdrawalControl(FormTypes action, string beingWithdrawnId)
        {
            InitializeComponent();
            this.actions = action;
            this.beingWithdrawnId = beingWithdrawnId;
            InitEvent();
            InitData();
            if (action == FormTypes.New)
            {
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
            }
            if (action == FormTypes.Audit || action == FormTypes.Browse)//审批，查看
            {
                ShieldedControl();
            }
        }
        #endregion

        #region 福利发放构造函数
        public WelfarePaymentWithdrawalControl(FormTypes action, V_WelfareProvision AppObj)
        {
            InitializeComponent();
            this.actions = action;
            WelfareProvision = AppObj;
            InitEvent();
            InitData();
            if (action == FormTypes.New)
            {
                BenefitsAdministration.GetProvisionByIdAsync(welfareInfoID);
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
            }
            if (action == FormTypes.Audit || action == FormTypes.Browse)//审批，查看
            {
                ShieldedControl();
            }
        }
        #endregion

        #region InitData
        private void InitData()
        {
            try
            {
                if (actions == FormTypes.New)
                {
                    InfoObj = new T_OA_WELFAREDISTRIBUTEUNDO();
                    InfoObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                }
                else
                {
                    if (actions == FormTypes.Audit)
                    {
                        actionFlag = DataActionFlag.SubmitComplete;
                    }
                    BenefitsAdministration.GetWelfarePaymentWithdrawalByIdAsync(beingWithdrawnId);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 屏蔽控件
        private void ShieldedControl()
        {
            this.txtReleaseName.IsReadOnly = true;
            this.txtRemark.IsReadOnly = true;
            this.txtTELL.IsReadOnly = true;
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            BenefitsAdministration = new SmtOADocumentAdminClient();
            client = new PersonnelServiceClient();
            ssc = new SalaryServiceClient();
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
            BenefitsAdministration.GetWelfarePaymentWithdrawalByIdCompleted += new EventHandler<GetWelfarePaymentWithdrawalByIdCompletedEventArgs>(BenefitsAdministration_GetWelfarePaymentWithdrawalByIdCompleted);
            BenefitsAdministration.WelfarePaymentWithdrawalAddCompleted += new EventHandler<WelfarePaymentWithdrawalAddCompletedEventArgs>(BenefitsAdministration_WelfarePaymentWithdrawalAddCompleted);
            BenefitsAdministration.UpdateWelfarePaymentWithdrawalCompleted += new EventHandler<UpdateWelfarePaymentWithdrawalCompletedEventArgs>(BenefitsAdministration_UpdateWelfarePaymentWithdrawalCompleted);
            BenefitsAdministration.GetProvisionByIdCompleted += new EventHandler<GetProvisionByIdCompletedEventArgs>(BenefitsAdministration_GetProvisionByIdCompleted);
            BenefitsAdministration.GetByIdWelfarePaymentDetailsCompleted += new EventHandler<GetByIdWelfarePaymentDetailsCompletedEventArgs>(BenefitsAdministration_GetByIdWelfarePaymentDetailsCompleted);
        }

        void BenefitsAdministration_GetByIdWelfarePaymentDetailsCompleted(object sender, GetByIdWelfarePaymentDetailsCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    detailTemps = e.Result.ToList();

                    foreach (var detailTemp in detailTemps)//获取年月
                    {
                        distributeMaster = new T_OA_WELFAREDISTRIBUTEMASTER();
                        if (detailTemp != null)
                        {
                            distributeMaster = detailTemp.T_OA_WELFAREDISTRIBUTEMASTER;
                            delteWelfarePaymentDetail.Add(detailTemp.USERID);
                            Year = Convert.ToDateTime(distributeMaster.DISTRIBUTEDATE).Year.ToString();//年
                            Month = Convert.ToDateTime(distributeMaster.DISTRIBUTEDATE).Month.ToString();//月
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void BenefitsAdministration_GetProvisionByIdCompleted(object sender, GetProvisionByIdCompletedEventArgs e)//根据发放ID查询
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result == null)
                    {
                        return;
                    }
                    distributeMaster = e.Result;
                    welfareInfoID = distributeMaster.WELFAREDISTRIBUTEMASTERID;
                    txtReleaseName.Text = distributeMaster.WELFAREDISTRIBUTETITLE;//福利发放名
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void BenefitsAdministration_GetWelfarePaymentWithdrawalByIdCompleted(object sender, GetWelfarePaymentWithdrawalByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result == null)
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFAREPAYMENTWITHDRAWAL"));
                        return;
                    }
                    InfoObj = e.Result;
                    if (actions == FormTypes.Resubmit)//重新提交
                    {
                        InfoObj.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }
                    distributeMaster = InfoObj.T_OA_WELFAREDISTRIBUTEMASTER;//福利发放主表
                    txtReleaseName.Text = distributeMaster.WELFAREDISTRIBUTETITLE;//福利发放名
                    welfareInfoID = distributeMaster.WELFAREDISTRIBUTEMASTERID;
                    txtTELL.Text = InfoObj.TEL;//联系电话
                    txtRemark.Text = InfoObj.REMARK;//备注
                    this.txtRemoveName.Text = InfoObj.OWNERNAME;//撤销人姓名
                    this.txtCompanyId.Text = Utility.GetCompanyName(InfoObj.OWNERCOMPANYID);//所属公司ID
                    this.txtDepartmentId.Text = Utility.GetDepartmentName(InfoObj.OWNERDEPARTMENTID);//所属部门ID
                    //if (actions == FormTypes.Audit)
                    //{
                    //    audit.XmlObject = DataObjectToXml<T_OA_WELFAREDISTRIBUTEUNDO>.ObjListToXml(InfoObj, "OA");
                    //}
                    BenefitsAdministration.GetByIdWelfarePaymentDetailsAsync(welfareInfoID);
                    //InitAudit();//获取审核控件的数据
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }
        #endregion

        #region 选择福利发放记录
        private void Select()
        {
            welfarePaymentRecordsForm = new SelectWelfarePaymentRecords();
            EntityBrowser browser = new EntityBrowser(welfarePaymentRecordsForm);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            if (welfarePaymentRecordsForm != null)
            {
                WelfareProvision = welfarePaymentRecordsForm.WelfareProvision;
                BenefitsAdministration.GetProvisionByIdAsync(WelfareProvision.welfareProvision.WELFAREDISTRIBUTEMASTERID);
            }
        }
        #endregion

        #region 获取当前员工的信息
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
                txtRemoveName.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                txtCompanyId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;//公司
                txtDepartmentId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;//部门
                if (ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE != null)
                {
                    txtTELL.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE;//手机号码
                }
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "WELFAREPAYMENTWITHDRAWAL");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "WELFAREPAYMENTWITHDRAWAL");
            }
            else if (actions == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT", "WELFAREPAYMENTWITHDRAWAL");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "WELFAREPAYMENTWITHDRAWAL");
            }
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
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "2":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Select();
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
            if (actions != FormTypes.Browse && actions != FormTypes.Audit)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "2",
                    Title = Utility.GetResourceStr("SELECTWELFAREPAYMENT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };

                items.Add(item);

                item = new ToolbarItem
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

        #region 添加
        void BenefitsAdministration_WelfarePaymentWithdrawalAddCompleted(object sender, WelfarePaymentWithdrawalAddCompletedEventArgs e)//新增福利发放撤销
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "WELFAREPAYMENTWITHDRAWAL"));
                    if (GlobalFunction.IsSaveAndClose(refreshType))
                    {
                        RefreshUI(refreshType);
                    }
                    else
                    {
                        actions = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 修改
        void BenefitsAdministration_UpdateWelfarePaymentWithdrawalCompleted(object sender, UpdateWelfarePaymentWithdrawalCompletedEventArgs e)//修改福利发放撤销
        {
            try
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        return;
                    }
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "WELFAREPAYMENTWITHDRAWAL"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "WELFAREPAYMENTWITHDRAWAL"));
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "WELFAREPAYMENTWITHDRAWAL"));
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 提交审核
        private void SubmitAuditToClose()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条
                    string strContractCreated = DateTime.Now.ToShortDateString();//创建时间

                    if (actions == FormTypes.New)
                    {
                        WelfarePaymentWithdrawalInfo = new T_OA_WELFAREDISTRIBUTEUNDO();
                        WelfarePaymentWithdrawalInfo.WELFAREDISTRIBUTEUNDOID = System.Guid.NewGuid().ToString();//撤销发放ID
                        WelfarePaymentWithdrawalInfo.T_OA_WELFAREDISTRIBUTEMASTER = distributeMaster;
                        WelfarePaymentWithdrawalInfo.TEL = this.txtTELL.Text;//联系电话
                        WelfarePaymentWithdrawalInfo.REMARK = this.txtRemark.Text.ToString();//备注
                        WelfarePaymentWithdrawalInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                        WelfarePaymentWithdrawalInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                        WelfarePaymentWithdrawalInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        WelfarePaymentWithdrawalInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        WelfarePaymentWithdrawalInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        WelfarePaymentWithdrawalInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        WelfarePaymentWithdrawalInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        WelfarePaymentWithdrawalInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        WelfarePaymentWithdrawalInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        WelfarePaymentWithdrawalInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        WelfarePaymentWithdrawalInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户姓名

                        BenefitsAdministration.WelfarePaymentWithdrawalAddAsync(WelfarePaymentWithdrawalInfo);
                    }
                    else
                    {
                        WelfarePaymentWithdrawalInfo.T_OA_WELFAREDISTRIBUTEMASTER = distributeMaster;
                        WelfarePaymentWithdrawalInfo.TEL = this.txtTELL.Text;//联系电话
                        WelfarePaymentWithdrawalInfo.REMARK = this.txtRemark.Text.ToString();//备注
                        WelfarePaymentWithdrawalInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//审批状态
                        WelfarePaymentWithdrawalInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        WelfarePaymentWithdrawalInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                        BenefitsAdministration.UpdateWelfarePaymentWithdrawalAsync(WelfarePaymentWithdrawalInfo, "Edit");
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            if (string.IsNullOrEmpty(this.txtReleaseName.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTPAYMENTRECORDS"));
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
            }
            return true;
        }
        #endregion

        //#region 审批流程
        ///// <summary>
        ///// 提交审核完成
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        //{
        //    auditResult = e.Result;
        //    switch (auditResult)
        //    {
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
        //            //todo 审核中
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
        //            //todo 取消
        //            Cancel();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
        //            //todo 终审通过
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
        //            //todo 审核不通过
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
        //            //todo 审核异常
        //            HandError();
        //            break;
        //    }
        //}
        //private void Cancel()
        //{
        //    RefreshUI(RefreshedTypes.Close);
        //}
        //private void HandError()
        //{
        //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
        //    RefreshUI(RefreshedTypes.CloseAndReloadData);
        //}
        //private void SumbitCompleted()
        //{
        //    try
        //    {
        //        if (WelfarePaymentWithdrawalInfo != null)
        //        {
        //            WelfarePaymentWithdrawalInfo.UPDATEDATE = DateTime.Now;
        //            WelfarePaymentWithdrawalInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
        //            WelfarePaymentWithdrawalInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        //            switch (auditResult)
        //            {
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
        //                    WelfarePaymentWithdrawalInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
        //                    break;
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
        //                    WelfarePaymentWithdrawalInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
        //                    ssc.EmployeeAddSumByEmployeeIDDeleteAsync(delteWelfarePaymentDetail, Year, Month);//删除薪资中的相关福利发放数据
        //                    break;
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
        //                    WelfarePaymentWithdrawalInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
        //                    break;
        //            }
        //            BenefitsAdministration.UpdateWelfarePaymentWithdrawalAsync(WelfarePaymentWithdrawalInfo);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
        //    }
        //}
        //#endregion

        //#region 获取审核控件的值(InitAudit)
        //private void InitAudit()
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "T_OA_WELFAREDISTRIBUTEUNDO";
        //    entity.FormID = InfoObj.WELFAREDISTRIBUTEUNDOID;
        //    entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //    entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //    entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //    entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    audit.BindingData();
        //}
        //#endregion

        //#region 提交流程
        ///// <summary>
        ///// 提交流程
        ///// </summary>
        //private void SumbitFlow()
        //{
        //    if (InfoObj != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "T_OA_WELFAREDISTRIBUTEUNDO";
        //        entity.FormID = InfoObj.WELFAREDISTRIBUTEUNDOID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        audit.XmlObject = DataObjectToXml<T_OA_WELFAREDISTRIBUTEUNDO>.ObjListToXml(InfoObj, "OA");
        //        audit.Submit();
        //    }
        //}
        //#endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_WELFAREDISTRIBUTEUNDO>(InfoObj, "OA");
            Utility.SetAuditEntity(entity, "T_OA_WELFAREDISTRIBUTEUNDO", InfoObj.WELFAREDISTRIBUTEUNDOID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                    state = Utility.GetCheckState(CheckStates.Approved);
                    ssc.EmployeeAddSumByEmployeeIDDeleteAsync(delteWelfarePaymentDetail, Year, Month);//删除薪资中的相关福利发放数据
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (InfoObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            InfoObj.CHECKSTATE = state;
            BenefitsAdministration.UpdateWelfarePaymentWithdrawalAsync(InfoObj, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (InfoObj != null)
                state = InfoObj.CHECKSTATE;
            if (actions == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            BenefitsAdministration.DoClose();
            ssc.DoClose();
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
