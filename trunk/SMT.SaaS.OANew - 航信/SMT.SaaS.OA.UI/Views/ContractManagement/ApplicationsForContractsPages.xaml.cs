/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-08-07

** 描述：

**    主要用于合同申请数据信息的录入

*********************************************************************************/
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
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.IO;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Application;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ApplicationsForContractsPages : BaseForm, IClient, IEntityEditor, IAudit
    {

        #region 全局变量

        private T_OA_CONTRACTAPP ctappObj;
        private T_OA_CONTRACTPRINT cprinting = new T_OA_CONTRACTPRINT();
        private FormTypes actions;//操作类型
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private SmtOADocumentAdminClient cmsfc;
        private PersonnelServiceClient personclient;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
        private ObservableCollection<string> Party = new ObservableCollection<string>();
        private string ctappID = string.Empty;
        private string strContactType = null;
        private string PartyaId = string.Empty;
        private string PartybId = string.Empty;
        public T_OA_CONTRACTAPP CtappObj
        {
            get { return ctappObj; }
            set
            {
                this.DataContext = value;
                ctappObj = value;
            }
        }
        #endregion

        #region 构造
        public ApplicationsForContractsPages(FormTypes action, string applicationsId)
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
                               {
                                   this.actions = action;
                                   this.ctappID = applicationsId;
                                   InitEvent();
                                   InitData();

                                   if (action == FormTypes.New)
                                   {
                                       cmsfc.GetContractTypeNameInfosToComboxAsync();
                                       //this.audit.Visibility = Visibility.Collapsed;
                                   }
                                   if (action == FormTypes.Audit || action == FormTypes.Browse)
                                   {
                                       ShieldedControl();
                                   }
                                   //ctrFile.SystemName = "OA";
                                   //ctrFile.ModelName = "Contract";
                                   //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
                                   //ctrFile.Event_AllFilesFinished +=
                                   //    new EventHandler<SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(
                                   //        ctrFile_Event_AllFilesFinished);
                               };
        }
        #endregion

        #region InitData
        private void InitData()
        {
            try
            {
                if (actions == FormTypes.New)
                {
                    CtappObj = new T_OA_CONTRACTAPP();
                    CtappObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                    this.StartTime.Text = DateTime.Now.ToShortDateString();
                    this.EndTime.Text = DateTime.Now.ToShortDateString();
                }
                else
                {
                    if (actions == FormTypes.Audit)
                    {
                        actionFlag = DataActionFlag.SubmitComplete;
                    }
                    cmsfc.GetContractApprovalByIdAsync(ctappID);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            cmsfc = new SmtOADocumentAdminClient();
            personclient = new PersonnelServiceClient();
            personclient.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(personclient_GetEmployeeByIDsCompleted);
            cmsfc.GetContractTypeNameInfosToComboxCompleted += new EventHandler<GetContractTypeNameInfosToComboxCompletedEventArgs>(cmsfc_GetContractTypeNameInfosToComboxCompleted);
            cmsfc.UpdateContraApprovalCompleted += new EventHandler<UpdateContraApprovalCompletedEventArgs>(cmsfc_UpdateContraApprovalCompleted);//修改
            cmsfc.ContractApprovalAddCompleted += new EventHandler<ContractApprovalAddCompletedEventArgs>(caswsc_ContractApprovalAddCompleted);//新增
            cmsfc.GetContractApprovalByIdCompleted += new EventHandler<GetContractApprovalByIdCompletedEventArgs>(cmsfc_GetContractApprovalByIdCompleted);//根据合同申请ID查询
        }

        void personclient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)//获取员工姓名(将ID转换为中文名)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();

                        if (vemployeeObj == null)
                            return;
                        var objc = from a in vemployeeObj
                                   where a.EMPLOYEEID == CtappObj.PARTYA
                                   select a.EMPLOYEECNAME;
                        var objcs = from a in vemployeeObj
                                    where a.EMPLOYEEID == CtappObj.PARTYB
                                    select a.EMPLOYEECNAME;
                        if (objc.Count() > 0)//如果数据存在
                        {
                            PartyaId = CtappObj.PARTYA;//将保存的甲方ID赋给变量PartyaId
                            this.txtPartya.Text = objc.FirstOrDefault();//将取回的数据赋给甲方文本框
                        }
                        else  //如果有另外一方数据未从组织架构中获取时执行
                        {
                            this.txtPartya.Text = CtappObj.PARTYA;//甲方
                        }
                        if (objcs.Count() > 0)//如果数据存在
                        {
                            PartybId = CtappObj.PARTYB;//将保存的乙方ID赋给变量PartybId
                            this.txtPartyb.Text = objcs.FirstOrDefault();//将取回的数据赋给乙方文本框
                        }
                        else
                        {
                            this.txtPartyb.Text = CtappObj.PARTYB;//乙方
                        }
                    }
                    else //如果全部未从组织架构中获取数据时执行
                    {
                        this.txtPartyb.Text = CtappObj.PARTYB;//乙方
                        this.txtPartya.Text = CtappObj.PARTYA;//甲方
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void cmsfc_GetContractApprovalByIdCompleted(object sender, GetContractApprovalByIdCompletedEventArgs e)
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
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                        return;
                    }
                    else
                    {
                        CtappObj = e.Result.contractApp;
                        if (actions == FormTypes.Resubmit)//重新提交
                        {
                            CtappObj.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                        }
                        if (actions != FormTypes.New)
                        {
                            strContactType = e.Result.contractApp.CONTRACTTYPEID;
                        }
                        //if (actions == FormTypes.Audit)
                        //{
                        //    audit.XmlObject = DataObjectToXml<T_OA_CONTRACTAPP>.ObjListToXml(CtappObj, "OA");
                        //}
                        if (actions == FormTypes.Edit || actions == FormTypes.Audit
                                  || actions == FormTypes.Browse)
                        {
                            if (CtappObj != null)
                            {
                                switch (CtappObj.CONTRACTFLAG)//申请标志
                                {
                                    case "0":
                                        this.rbtYes.IsChecked = true;//商务合同
                                        break;
                                    case "1":
                                        this.RbtNo.IsChecked = true;//人事合同
                                        break;
                                }
                                this.StartTime.Text = Convert.ToDateTime(CtappObj.STARTDATE).ToShortDateString();
                                this.EndTime.Text = Convert.ToDateTime(CtappObj.ENDDATE).ToShortDateString();
                                this.nudBalanceMonth.Value = Convert.ToInt32(CtappObj.EXPIRATIONREMINDER);//到期提醒天数                                
                                ContractText.RichTextBoxContext = ctappObj.CONTENT;//合同内容
                                this.HasChosenTemplate.IsEnabled = false;
                                this.cbContractLevel.IsEnabled = false;

                                Party.Add(CtappObj.PARTYA);
                                Party.Add(CtappObj.PARTYB);
                                personclient.GetEmployeeByIDsAsync(Party);

                            }
                        }
                        //InitAudit();//审批
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        cmsfc.GetContractTypeNameInfosToComboxAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void cmsfc_GetContractTypeNameInfosToComboxCompleted(object send, GetContractTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.tbxContractTypeID.Items.Clear();
                foreach (T_OA_CONTRACTTYPE obj in e.Result)
                {
                    tbxContractTypeID.Items.Add(obj);
                }
                tbxContractTypeID.DisplayMemberPath = "CONTRACTTYPE";

                if (strContactType == null)
                {
                    SetComboBoxSelect(e.Result, null);
                }
                else
                {
                    SetComboBoxSelect(e.Result, strContactType);
                }
            }
        }

        private void SetComboBoxSelect(ObservableCollection<T_OA_CONTRACTTYPE> cmbData, string assetId)
        {
            tbxContractTypeID.Items.Clear();
            T_OA_CONTRACTTYPE selectObj = null;
            foreach (T_OA_CONTRACTTYPE obj in cmbData)
            {
                tbxContractTypeID.Items.Add(obj);
                if (obj.CONTRACTTYPEID == assetId)
                {
                    selectObj = obj;
                }
            }
            tbxContractTypeID.DisplayMemberPath = "CONTRACTTYPE";
            if (selectObj != null)
            {
                tbxContractTypeID.SelectedItem = selectObj;
            }
            else
            {
                this.tbxContractTypeID.SelectedIndex = 0;
            }
        }
        #endregion

        #region 屏蔽控件
        private void ShieldedControl()
        {
            this.ContractID.IsReadOnly = true;
            this.ContractTitle.IsReadOnly = true;
            this.tbxContractTypeID.IsEnabled = false;
            this.cbContractLevel.IsEnabled = false;
            this.txtPartya.IsReadOnly = true;
            this.txtPartyb.IsReadOnly = true;
            this.StartTime.IsEnabled = false;
            this.EndTime.IsEnabled = false;
            this.HasChosenTemplate.IsEnabled = false;
            this.RbtNo.IsEnabled = false;
            this.rbtYes.IsEnabled = false;
            this.nudBalanceMonth.IsEnabled = false;
            this.btnLookUpPartya.IsEnabled = false;
            this.btnLookUpPartyb.IsEnabled = false;
            //this.ctrFile.IsEnabled = false;
            this.ContractText.GetRichTextbox().IsEnabled = false;
        }
        #endregion

        #region 上传附件//打开 本地文件
        //private void UploadFiles()
        //{
        //    System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
        //    openFileWindow.Multiselect = true;
        //    if (openFileWindow.ShowDialog() == true)
        //        foreach (FileInfo file in openFileWindow.Files)
        //            ctrFile.InitFiles(file.Name, file.OpenRead());
        //}
        //void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        //{
        //    RefreshUI(RefreshedTypes.HideProgressBar);
        //}
        #endregion

        #region LayoutRoot_Loaded&ReloadData
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(ctappID))
            //{
            //    ctrFile.Load_fileData(ctappID);
            //}
        }
        #endregion

        #region 类型、模板列表处理
        private void tbxContractTypeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbxContractTypeID.SelectedItem == null) return;
            string strContractTypeID = ((T_OA_CONTRACTTYPE)tbxContractTypeID.SelectedItem).CONTRACTTYPEID;
            cmsfc.GetContractTypeTemplateNameByContractTypeInfosCompleted += new EventHandler<GetContractTypeTemplateNameByContractTypeInfosCompletedEventArgs>(cmsfc_GetContractTypeTemplateNameByContractTypeInfosCompleted);
            cmsfc.GetContractTypeTemplateNameByContractTypeInfosAsync(strContractTypeID);
        }
        void cmsfc_GetContractTypeTemplateNameByContractTypeInfosCompleted(object sender, GetContractTypeTemplateNameByContractTypeInfosCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        HasChosenTemplate.Items.Clear();
                        List<T_OA_CONTRACTTEMPLATE> tmpTemplate = e.Result.ToList();
                        T_OA_CONTRACTTEMPLATE TemplateT = new T_OA_CONTRACTTEMPLATE();
                        TemplateT.CONTRACTTEMPLATEID = "";
                        string dictname = Utility.GetResourceStr("PLEASESELECTL", TemplateT.CONTRACTTEMPLATEID);
                        TemplateT.CONTRACTTEMPLATENAME = dictname;
                        tmpTemplate.Insert(0, TemplateT);

                        foreach (T_OA_CONTRACTTEMPLATE obj in tmpTemplate)
                        {
                            HasChosenTemplate.Items.Add(obj);
                        }
                    }
                    if (HasChosenTemplate.Items.Count > 0)
                    {
                        HasChosenTemplate.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("获取合同模板名称Completed事件", "OA", "T_OA_CONTRACTAPP", "获取合同模版时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 添加Completed
        void caswsc_ContractApprovalAddCompleted(object sender, ContractApprovalAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "APPLICATIONSFORCONTRACTS"));
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
                ExceptionManager.SendException("添加合同申请Completed事件", "OA", "T_OA_CONTRACTAPP", "添加合同申请时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 修改Completed
        void cmsfc_UpdateContraApprovalCompleted(object sender, UpdateContraApprovalCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    if (e.UserState.ToString() == "Edit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "APPLICATIONSFORCONTRACTS"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                    }
                    else if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "APPLICATIONSFORCONTRACTS"));
                    }
                    else if (e.UserState.ToString() == "Submit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "APPLICATIONSFORCONTRACTS"));
                    }
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("修改合同申请Completed事件", "OA", "T_OA_CONTRACTAPP", "修改合同申请时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 提交审核
        public void SubmitAuditToClose()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }
        #endregion

        #region ComBox模板处理事件
        private void HasChosenTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (HasChosenTemplate.SelectedIndex < 1)
                {
                    if (actions == FormTypes.New)
                    {
                        //ContractText.GetRichTextbox() = string.Empty;
                        ContractTitle.Text = string.Empty;
                        cbContractLevel.SelectedIndex = 0;
                        cbContractLevel.IsEnabled = true;
                        //ContractTitle.IsReadOnly = true;
                    }
                }
                else
                {
                    T_OA_CONTRACTTEMPLATE TemplageText = new T_OA_CONTRACTTEMPLATE();
                    TemplageText = (T_OA_CONTRACTTEMPLATE)HasChosenTemplate.SelectedItem;

                    ContractText.RichTextBoxContext = TemplageText.CONTENT;
                    ContractTitle.Text = TemplageText.CONTRACTTITLE;
                    foreach (T_SYS_DICTIONARY contractLevel in cbContractLevel.Items)
                    {
                        if (contractLevel.DICTIONARYVALUE.ToString() == TemplageText.CONTRACTLEVEL)
                        {
                            cbContractLevel.SelectedItem = contractLevel;
                        }
                    }
                    cbContractLevel.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("ComBox选择模板处理事件", "OA", "T_OA_CONTRACTAPP", "选择合同模版返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContractText.Height = ((Grid)sender).ActualHeight * 0.35;
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "APPLICATIONSFORCONTRACTS");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "APPLICATIONSFORCONTRACTS");
            }
            else if (actions == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT", "APPLICATIONSFORCONTRACTS");
            }
            else if (actions == FormTypes.Browse)
            {
                return Utility.GetResourceStr("VIEWTITLE", "APPLICATIONSFORCONTRACTS");
            }
            else
            {
                return Utility.GetResourceStr("UPLOADL", "APPLICATIONSFORCONTRACTS");
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
                    //UploadFiles();
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
                    Title = Utility.GetResourceStr("SELECTACCESSORIES"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
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

        #region 保存函数
        private void Save()
        {
            try
            {
                if (Check())
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);//点击保存后显示进度条
                    T_SYS_DICTIONARY StrContractLevel = cbContractLevel.SelectedItem as T_SYS_DICTIONARY;//合同级别
                    string StrFlag = string.Empty;
                    string StartTime = string.Empty;//开始时间
                    string StrEndTime = string.Empty;//结束时间

                    StartTime = this.StartTime.Text;
                    StrEndTime = this.EndTime.Text;

                    DateTime DtStart = new DateTime();
                    DateTime DtEnd = new DateTime();
                    if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(StrEndTime))
                    {
                        DtStart = System.Convert.ToDateTime(StartTime);
                        DtEnd = System.Convert.ToDateTime(StrEndTime);
                        if (DtStart >= DtEnd)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("StartDateEndDatemustbegreaterthan", ""));
                            this.StartTime.Focus();
                            RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                            return;
                        }
                    }

                    if (this.rbtYes.IsChecked == true)//商务合同
                    {
                        StrFlag = "0";
                    }
                    if (this.RbtNo.IsChecked == true)//人事合同
                    {
                        StrFlag = "1";
                    }
                    if (actions == FormTypes.New)
                    {
                        string strContractTypeID = ((T_OA_CONTRACTTYPE)tbxContractTypeID.SelectedItem).CONTRACTTYPEID;//类型ID
                        ctappObj.CONTRACTAPPID = System.Guid.NewGuid().ToString();
                        ctappObj.CONTRACTTYPEID = strContractTypeID;
                        ctappObj.CONTRACTLEVEL = StrContractLevel.DICTIONARYVALUE.ToString();
                        ctappObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                        ctappObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人
                        ctappObj.STARTDATE = DtStart;
                        ctappObj.ENDDATE = DtEnd;
                        ctappObj.EXPIRATIONREMINDER = Convert.ToInt32(nudBalanceMonth.Value);//到期提醒天数

                        ctappObj.CONTENT = ContractText.RichTextBoxContext;
                        ctappObj.CONTRACTFLAG = StrFlag;//合同标志
                        ctappObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        ctappObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        ctappObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                        ctappObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        ctappObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        ctappObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        ctappObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        ctappObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        ctappObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        //ctrFile.FormID = ctappObj.CONTRACTAPPID;//上传附件
                        //if (string.IsNullOrEmpty(PartyaId))//如果甲方ID不为空
                        //{
                        //    ctappObj.PARTYA = txtPartya.Text;
                        //}
                        //if (string.IsNullOrEmpty(PartybId))//如果乙方ID不为空
                        //{
                        //    ctappObj.PARTYB = txtPartyb.Text;
                        //}
                        //ctrFile.Save();

                        cmsfc.ContractApprovalAddAsync(ctappObj);
                    }
                    else
                    {
                        string strContractTypeID = ((T_OA_CONTRACTTYPE)tbxContractTypeID.SelectedItem).CONTRACTTYPEID;//类型ID
                        ctappObj.CONTRACTTYPEID = strContractTypeID;
                        ctappObj.CONTRACTLEVEL = StrContractLevel.DICTIONARYVALUE.ToString();
                        ctappObj.STARTDATE = DtStart;
                        ctappObj.ENDDATE = DtEnd;
                        ctappObj.EXPIRATIONREMINDER = Convert.ToInt32(nudBalanceMonth.Value);//到期提醒天数

                        ctappObj.CONTENT = ContractText.RichTextBoxContext;
                        ctappObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                        ctappObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
                        ctappObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                        ctappObj.CONTRACTFLAG = StrFlag;//合同标志
                        //ctrFile.FormID = ctappObj.CONTRACTAPPID;//上传附件
                        //if (string.IsNullOrEmpty(PartyaId))//如果乙方ID不为空
                        //{
                        //    ctappObj.PARTYA = txtPartya.Text;
                        //}
                        //if (string.IsNullOrEmpty(PartybId))//如果乙方ID不为空
                        //{
                        //    ctappObj.PARTYB = txtPartyb.Text;
                        //}
                        //ctrFile.Save();

                        cmsfc.UpdateContraApprovalAsync(ctappObj, "Edit");
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("保存Save函数", "OA", "T_OA_CONTRACTAPP", "保存返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
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

            if (string.IsNullOrEmpty(this.StartTime.Text))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "EFFICDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (string.IsNullOrEmpty(this.EndTime.Text))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TERMINATEDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (this.tbxContractTypeID.SelectedIndex < 0)//合同类型
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "CONTRACTTYPE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.tbxContractTypeID.Focus();
                return false;
            }

            if (this.cbContractLevel.SelectedIndex <= 0)//合同级别
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "CONTRACTLEVEL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.cbContractLevel.Focus();
                return false;
            }
            return true;
        }
        #endregion

        #region 甲方
        private void btnLookUpPartya_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    ctappObj.PARTYA = companyInfo.ObjectID;//甲方
                    txtPartya.Text = companyInfo.ObjectName;
                    PartyaId = companyInfo.ObjectID;//将甲方ID赋给变量PartyaId
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 乙方
        private void btnLookUpPartyb_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    ctappObj.PARTYB = companyInfo.ObjectID;//乙方
                    txtPartyb.Text = companyInfo.ObjectName;
                    PartybId = companyInfo.ObjectID;//将乙方ID赋给变量PartybId
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 输入甲方键盘事件
        private void txtPartya_KeyDown(object sender, KeyEventArgs e)//甲方
        {
            PartyaId = string.Empty;//清空甲方ID
        }
        #endregion

        #region 输入乙方键盘事件
        private void txtPartyb_KeyDown(object sender, KeyEventArgs e)
        {
            PartybId = string.Empty;//清空乙方ID
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_CONTRACTAPP>(CtappObj, "OA");
            Utility.SetAuditEntity(entity, "T_OA_CONTRACTAPP", CtappObj.CONTRACTAPPID, strXmlObjectSource);
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
            if (ctappObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            ctappObj.CHECKSTATE = state;
            cmsfc.UpdateContraApprovalAsync(ctappObj, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (CtappObj != null)
                state = CtappObj.CHECKSTATE;
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