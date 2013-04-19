/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-23

** 修改人：刘锦

** 修改时间：2010-07-02

** 描述：

**    主要用于合同查看申请的数据录入，获取已打印、原件上传、审批通过的合同，录入相关的查看申请信息

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Application;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ViewContractApplicationControl : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量
        private T_OA_CONTRACTAPP ctapp = new T_OA_CONTRACTAPP();
        private T_OA_CONTRACTVIEW contractViewObj;
        private T_OA_CONTRACTPRINT cprinting = new T_OA_CONTRACTPRINT();
        private V_ContractPrint cprintingObj = new V_ContractPrint();
        private string strContactType = null; //合同类型
        private FormTypes actions;//操作类型
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private SelectViewRecordsControl viewForm;
        private SmtOADocumentAdminClient cmsfc;
        private PersonnelServiceClient client;
        private string contractViewID = "";
        private V_EMPLOYEEPOST employeepost;
        public T_OA_CONTRACTVIEW ContractViewObj
        {
            get { return contractViewObj; }
            set
            {
                this.DataContext = value;
                contractViewObj = value;
            }
        }
        #endregion

        #region 查看申请构造
        public ViewContractApplicationControl(FormTypes action, string contractViewId)
        {
            actions = action;
            InitializeComponent();
            this.actions = action;
            this.contractViewID = contractViewId;
            InitEvent();
            InitData();
            //audit.Visibility = Visibility.Visible;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }

        //void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        //{
        //    RefreshUI(RefreshedTypes.HideProgressBar);
        //}
        #endregion

        #region InitData
        private void InitData()
        {
            if (actions == FormTypes.New)
            {
                ContractViewObj = new T_OA_CONTRACTVIEW();
                ContractViewObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                client.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);//获取当期用户信息
                this.txtFile.Visibility = Visibility.Collapsed;
                this.ContractText.Visibility = Visibility.Collapsed;
                this.txtContractText.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (actions == FormTypes.Audit)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                cmsfc.GetContractViewByIdAsync(contractViewID);
            }
            if (actions == FormTypes.Audit || actions == FormTypes.Edit)
            {
                this.txtContractText.Visibility = Visibility.Collapsed;
                this.txtFile.Visibility = Visibility.Collapsed;
                this.ContractText.Visibility = Visibility.Collapsed;
                //this.ctrFile.Visibility = Visibility.Collapsed;
            }
            if (actions == FormTypes.Browse)
            {
                this.txtContractText.Visibility = Visibility.Collapsed;
                this.ContractText.Visibility = Visibility.Collapsed;
                this.txtTELL.IsReadOnly = true;
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            cmsfc = new SmtOADocumentAdminClient();
            client = new PersonnelServiceClient();
            client.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(client_GetEmployeeDetailByIDCompleted);
            cmsfc.ContractViewapplicationsAddCompleted += new EventHandler<ContractViewapplicationsAddCompletedEventArgs>(cmsfc_ContractViewapplicationsAddCompleted);//新增合同查看申请
            cmsfc.UpdateContractViewCompleted += new EventHandler<UpdateContractViewCompletedEventArgs>(cmsfc_UpdateContractViewCompleted);//修改合同查看申请
            cmsfc.GetContractViewByIdCompleted += new EventHandler<GetContractViewByIdCompletedEventArgs>(cmsfc_GetContractViewByIdCompleted);//根据查看ID查询
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void cmsfc_GetContractViewByIdCompleted(object sender, GetContractViewByIdCompletedEventArgs e)//根据查看ID查询
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        ContractViewObj = e.Result;//合同查看申请视图
                        if (actions == FormTypes.Resubmit)//重新提交
                        {
                            ContractViewObj.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                        }
                        cprinting = ContractViewObj.T_OA_CONTRACTPRINT;//打印视图
                        ctapp = ContractViewObj.T_OA_CONTRACTPRINT.T_OA_CONTRACTAPP;
                        ContractID.SelectedText = ctapp.CONTRACTCODE;//合同编号
                        ContractTitle.Text = ctapp.CONTRACTTITLE;//标题 
                        this.txtCompanyId.Text = Utility.GetCompanyName(ContractViewObj.OWNERCOMPANYID);//所属公司ID
                        this.txtDepartmentId.Text = Utility.GetDepartmentName(ContractViewObj.OWNERDEPARTMENTID);//所属部门ID

                        txtTELL.Text = ContractViewObj.TEL;
                        txtCreateUser.Text = ContractViewObj.OWNERNAME;
                        //if (actions == FormTypes.Audit)
                        //{
                        //    audit.XmlObject = DataObjectToXml<T_OA_CONTRACTVIEW>.ObjListToXml(ContractViewObj, "OA");
                        //}
                        //InitAudit();//审批
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        //ctrFile.Load_fileData(cprinting.CONTRACTPRINTID);
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
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
                txtCreateUser.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                txtCompanyId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;//公司
                txtDepartmentId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;//部门
                if (ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE != null)
                {
                    txtTELL.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.MOBILE;//手机号码
                }
            }
        }
        #endregion

        #region 修改Completed
        void cmsfc_UpdateContractViewCompleted(object sender, UpdateContractViewCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
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
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "VIEWCONTRACTAPPLICATION"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "VIEWCONTRACTAPPLICATION"));
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "VIEWCONTRACTAPPLICATION"));
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("修改合同查看申请Completed事件", "OA", "T_OA_CONTRACTVIEW", "修改合同查看申请时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 新增Complete
        void cmsfc_ContractViewapplicationsAddCompleted(object sender, ContractViewapplicationsAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
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
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "VIEWCONTRACTAPPLICATION"));
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
                ExceptionManager.SendException("新增合同查看申请Completed事件", "OA", "T_OA_CONTRACTVIEW", "新增合同查看申请时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            try
            {

                RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条

                if (string.IsNullOrEmpty(this.ContractTitle.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASECHOOSETOVIEWTHECONTRACT"));
                    RefreshUI(RefreshedTypes.HideProgressBar);//点击保存后显示进度条
                    return;
                }

                if (string.IsNullOrEmpty(txtTELL.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "TELL"));
                    RefreshUI(RefreshedTypes.HideProgressBar);//点击保存后显示进度条
                    txtTELL.Focus();
                    return;
                }

                //ctrFile.FormID = cprinting.CONTRACTPRINTID;//附件
                //ctrFile.Save();
                if (actions == FormTypes.New)
                {
                    contractViewObj = new T_OA_CONTRACTVIEW();
                    if (employeepost != null)
                    {
                        contractViewObj.OWNERPOSTID = employeepost.EMPLOYEEPOSTS[0].T_HR_POST.POSTID;//岗位ID
                        contractViewObj.OWNERCOMPANYID = employeepost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;//公司ID
                        contractViewObj.OWNERDEPARTMENTID = employeepost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;//部门ID
                        contractViewObj.OWNERID = employeepost.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEEID;//员工ID
                        contractViewObj.OWNERNAME = employeepost.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                    }
                    contractViewObj.CONTRACTVIEWID = System.Guid.NewGuid().ToString();
                    contractViewObj.T_OA_CONTRACTPRINT = cprinting;      //打印实体
                    contractViewObj.TEL = txtTELL.Text;//联系电话
                    contractViewObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                    contractViewObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人
                    contractViewObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                    contractViewObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                    contractViewObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                    contractViewObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                    contractViewObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                    contractViewObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                    contractViewObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                    contractViewObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                    contractViewObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID

                    cmsfc.ContractViewapplicationsAddAsync(contractViewObj);
                }
                else if (actions == FormTypes.Edit)
                {
                    contractViewObj.T_OA_CONTRACTPRINT = cprinting;      //打印实体
                    contractViewObj.TEL = txtTELL.Text;//联系电话
                    contractViewObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交

                    cmsfc.UpdateContractViewAsync(contractViewObj, "Edit");
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("保存合同查看申请Save事件", "OA", "T_OA_CONTRACTVIEW", "保存合同查看申请时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        #endregion

        #region 选择合同
        private void Select()
        {
            viewForm = new SelectViewRecordsControl();
            EntityBrowser browser = new EntityBrowser(viewForm);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            if (viewForm.selecContractInfoObj != null)
            {
                cprintingObj = viewForm.selecContractInfoObj;
                GetContractPrint(cprintingObj);
            }
        }
        #endregion

        #region 获取已审批的合同记录
        private void GetContractPrint(V_ContractPrint contractPrint)
        {
            if (cprinting != null)
            {
                if (!string.IsNullOrEmpty(contractPrint.contractApp.contractApp.CONTRACTTITLE))
                {
                    ContractTitle.Text = contractPrint.contractApp.contractApp.CONTRACTTITLE;//标题
                }
                cprinting = contractPrint.contractPrint;
                ctapp = contractPrint.contractApp.contractApp;

                ContractID.SelectedText = contractPrint.contractApp.contractApp.CONTRACTCODE;//合同编号
                ContractTitle.Text = contractPrint.contractApp.contractApp.CONTRACTTITLE;//标题 
            }
        }

        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "VIEWCONTRACTAPPLICATION");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "VIEWCONTRACTAPPLICATION");
            }
            else if (actions == FormTypes.Browse)
            {
                return Utility.GetResourceStr("VIEWTITLE", "VIEWCONTRACTAPPLICATION");
            }
            else
            {
                return Utility.GetResourceStr("AUDIT", "VIEWCONTRACTAPPLICATION");
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
                    Title = Utility.GetResourceStr("SELECTCONTRACT"),
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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region 提交审核
        public void SubmitAuditToClose()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_CONTRACTVIEW>(ContractViewObj, "OA");
            Utility.SetAuditEntity(entity, "T_OA_CONTRACTVIEW", ContractViewObj.CONTRACTVIEWID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (contractViewObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            contractViewObj.CHECKSTATE = state;
            cmsfc.UpdateContractViewAsync(contractViewObj, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (ContractViewObj != null)
                state = ContractViewObj.CHECKSTATE;
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
            cmsfc.DoClose();
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
