/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-23

** 修改人：刘锦

** 修改时间：2010-07-23

** 描述：

**    主要用于合同打印上传及合同打印；上传：将已打印的合同原件上传

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.IO;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.CommonReportsModel;
using SMT.SAAS.Application;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractPrintUploadControl : BaseForm, IClient, IEntityEditor, ICPOperate, IPrintPage<T_OA_CONTRACTAPP>
    {

        #region 全局变量
        private T_OA_CONTRACTAPP ctapp = new T_OA_CONTRACTAPP();
        private V_ContractApplications printView = new V_ContractApplications();
        private T_OA_CONTRACTPRINT cprinting;
        private string strContactType = null; //合同类型
        private Action actions;//操作类型
        private PersonnelServiceClient personclient;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private ContractPrintingControl printForm;
        private SmtOADocumentAdminClient cmsfc;
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
        private ObservableCollection<string> Party = new ObservableCollection<string>();
        #endregion

        #region 构造
        public ContractPrintUploadControl(Action action, V_ContractPrint AppObj)
        {
            actions = action;
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                InitEvent();
                Utility.CbxItemBinders(ContractLevel, "CONTRACTLEVEL", "0");
                this.StartTime.Text = DateTime.Now.ToString();
                this.EndTime.Text = DateTime.Now.ToString();
                combox_SelectSource();

                if (action == Action.Read || action == Action.FromAnnex)
                {
                    GetSelectContractPrint(AppObj);
                    if (AppObj.contractPrint.SIGNDATE != null)
                    {
                        this.DateSigned.Text = AppObj.contractPrint.SIGNDATE.ToString();//合同签订日期
                    }
                    else
                    {
                        this.DateSigned.Text = string.Empty;
                    }
                    this.nudBalanceMonth.Value = Convert.ToInt32(AppObj.contractPrint.NUM);//打印份数
                }
                if (action == Action.Read)
                {
                    ContractFromAnnex();
                    this.DateSigned.IsEnabled = false;
                    //this.ctrFile.IsEnabled = false;
                    if (AppObj.contractPrint.SIGNDATE != null)
                    {
                        this.DateSigned.Text = AppObj.contractPrint.SIGNDATE.ToString();//合同签订日期
                    }
                    else
                    {
                        this.DateSigned.Text = string.Empty;
                    }
                }
                if (action == Action.Print)
                {
                    this.DateSigned.Text = string.Empty;
                    ContractPrint();
                }
                if (action == Action.FromAnnex)
                {
                    if (AppObj.contractPrint.SIGNDATE != null)
                    {
                        this.DateSigned.Text = AppObj.contractPrint.SIGNDATE.ToString();//合同签订日期
                    }
                    else
                    {
                        this.DateSigned.Text = string.Empty;
                    }
                    ContractFromAnnex();
                }
                //ctrFile.SystemName = "OA";
                //ctrFile.ModelName = "Contract";
                //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
                //ctrFile.Event_AllFilesFinished += new EventHandler<SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(ctrFile_Event_AllFilesFinished);
                #endregion
            };
        }
        #endregion

        #region 合同打印时隐藏页面控件
        private void ContractPrint()
        {
            ContractID.IsReadOnly = true;
            ContractTitle.IsReadOnly = true;
            ContractTypeID.IsEnabled = false;
            ContractLevel.IsEnabled = false;
            txtPartya.IsReadOnly = true;
            txtPartyb.IsReadOnly = true;
            StartTime.IsEnabled = false;
            EndTime.IsEnabled = false;
            HasChosenTemplate.IsEnabled = false;
            RbtNo.IsEnabled = false;
            rbtYes.IsEnabled = false;
            ContractText.IsEnabled = false;
            btnLookUpPartya.IsEnabled = false;
            btnLookUpPartyb.IsEnabled = false;
            this.ContractText.GetRichTextbox().IsEnabled = false;
            this.txtDateSigned.Visibility = Visibility.Collapsed;//签订时间
            this.DateSigned.Visibility = Visibility.Collapsed;
            this.txtFile.Visibility = Visibility.Collapsed;
            //this.ctrFile.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 上传合同附件时隐藏控件
        private void ContractFromAnnex()
        {
            ContractID.IsReadOnly = true;
            ContractTitle.IsReadOnly = true;
            ContractTypeID.IsEnabled = false;
            ContractLevel.IsEnabled = false;
            txtPartya.IsReadOnly = true;
            txtPartyb.IsReadOnly = true;
            StartTime.IsEnabled = false;
            EndTime.IsEnabled = false;
            HasChosenTemplate.IsEnabled = false;
            RbtNo.IsEnabled = false;
            rbtYes.IsEnabled = false;
            ContractText.IsEnabled = false;
            btnLookUpPartya.IsEnabled = false;
            btnLookUpPartyb.IsEnabled = false;
            nudBalanceMonth.IsEnabled = false;
            this.ContractText.GetRichTextbox().IsEnabled = false;
        }
        #endregion

        #region COMBOX 设置数据源
        private void combox_SelectSource()
        {
            cmsfc.GetContractTypeNameInfosToComboxAsync();
            cmsfc.GetContractTypeNameInfosToComboxCompleted += new EventHandler<GetContractTypeNameInfosToComboxCompletedEventArgs>(cmsfc_GetContractTypeNameInfosToComboxCompleted);
        }

        void cmsfc_GetContractTypeNameInfosToComboxCompleted(object send, GetContractTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.ContractTypeID.Items.Clear();
                foreach (T_OA_CONTRACTTYPE obj in e.Result)
                {
                    ContractTypeID.Items.Add(obj);
                }
                ContractTypeID.DisplayMemberPath = "CONTRACTTYPE";

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
            ContractTypeID.Items.Clear();
            T_OA_CONTRACTTYPE selectObj = null;
            foreach (T_OA_CONTRACTTYPE obj in cmbData)
            {
                ContractTypeID.Items.Add(obj);
                if (obj.CONTRACTTYPEID == assetId)
                {
                    selectObj = obj;
                }
            }
            ContractTypeID.DisplayMemberPath = "CONTRACTTYPE";
            if (selectObj != null)
            {
                ContractTypeID.SelectedItem = selectObj;
            }
            else
            {
                this.ContractTypeID.SelectedIndex = 0;
            }
        }
        #endregion

        #region LayoutRoot_Loaded&ReloadData
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            combox_SelectSource();
            //if (cprinting != null)
            //{
            //    ctrFile.Load_fileData(cprinting.CONTRACTPRINTID);
            //}
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

        #region GetSelectContractPrint
        private void GetSelectContractPrint(V_ContractPrint cprintingContract)
        {
            if (cprinting != null)
            {
                if (!string.IsNullOrEmpty(cprintingContract.contractApp.contractApp.CONTRACTTITLE))
                {
                    ContractTitle.Text = cprintingContract.contractApp.contractApp.CONTRACTTITLE;//标题
                }
                cprinting = cprintingContract.contractPrint;//打印
                ctapp = cprintingContract.contractApp.contractApp;

                ContractID.SelectedText = cprintingContract.contractApp.contractApp.CONTRACTCODE;//合同编号
                ContractLevel.SelectedIndex = Convert.ToInt32(cprintingContract.contractApp.contractApp.CONTRACTLEVEL);//级别
                ContractTitle.Text = cprintingContract.contractApp.contractApp.CONTRACTTITLE;//标题
                StartTime.Text = cprintingContract.contractApp.contractApp.STARTDATE.ToString();//开始时间
                EndTime.Text = cprintingContract.contractApp.contractApp.ENDDATE.ToString();//结束时间


                ContractText.RichTextBoxContext = cprintingContract.contractApp.contractApp.CONTENT;
                Party.Add(cprintingContract.contractApp.contractApp.PARTYA);//甲方
                Party.Add(cprintingContract.contractApp.contractApp.PARTYB);//乙方
                personclient.GetEmployeeByIDsAsync(Party);
                switch (cprintingContract.contractApp.contractApp.CONTRACTFLAG)//申请标志
                {
                    case "0":
                        this.RbtNo.IsChecked = false;
                        this.rbtYes.IsChecked = true;
                        break;
                    case "1":
                        this.RbtNo.IsChecked = true;
                        this.rbtYes.IsChecked = false;
                        break;
                }
            }
        }
        #endregion

        #region GetSelectContractApplications
        private void GetSelectContractApplications(V_ContractApplications contractApp)
        {
            if (contractApp != null)
            {
                if (!string.IsNullOrEmpty(contractApp.contractApp.CONTRACTTITLE))
                {
                    ContractTitle.Text = contractApp.contractApp.CONTRACTTITLE;//标题
                }
                cprinting = contractApp.contractPrint;//打印
                ctapp = contractApp.contractApp;

                ContractID.SelectedText = contractApp.contractApp.CONTRACTCODE;//合同编号
                ContractLevel.SelectedIndex = Convert.ToInt32(contractApp.contractApp.CONTRACTLEVEL);//级别
                ContractTitle.Text = contractApp.contractApp.CONTRACTTITLE;//标题
                StartTime.Text = contractApp.contractApp.STARTDATE.ToString();//开始时间
                EndTime.Text = contractApp.contractApp.ENDDATE.ToString();//结束时间
                //ContractText.Text = contractApp.contractApp.CONTENT;//正文

                ContractText.RichTextBoxContext = contractApp.contractApp.CONTENT;
                Party.Add(contractApp.contractApp.PARTYA);//甲方
                Party.Add(contractApp.contractApp.PARTYB);//乙方
                personclient.GetEmployeeByIDsAsync(Party);
                switch (contractApp.contractApp.CONTRACTFLAG)//申请标志
                {
                    case "0":
                        this.RbtNo.IsChecked = false;
                        this.rbtYes.IsChecked = true;
                        break;
                    case "1":
                        this.RbtNo.IsChecked = true;
                        this.rbtYes.IsChecked = false;
                        break;
                }
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            cmsfc = new SmtOADocumentAdminClient();
            cprinting = new T_OA_CONTRACTPRINT();
            personclient = new PersonnelServiceClient();
            cmsfc.ContractPrintingAddCompleted += new EventHandler<ContractPrintingAddCompletedEventArgs>(cmsfc_ContractPrintingAddCompleted);//打印
            cmsfc.UpdateContractPrintingCompleted += new EventHandler<UpdateContractPrintingCompletedEventArgs>(cmsfc_UpdateContractPrintingCompleted);//上传附件
            personclient.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(personclient_GetEmployeeByIDsCompleted);
        }
        #endregion

        #region 获取员工姓名(将ID转换为中文名)
        void personclient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
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
                                   where a.EMPLOYEEID == ctapp.PARTYA
                                   select a.EMPLOYEECNAME;
                        var objcs = from a in vemployeeObj
                                    where a.EMPLOYEEID == ctapp.PARTYB
                                    select a.EMPLOYEECNAME;
                        if (objc.Count() > 0)//如果数据存在
                        {
                            this.txtPartya.Text = objc.FirstOrDefault();//将取回的数据赋给甲方文本框
                        }
                        else  //如果有另外一方数据未从组织架构中获取时执行
                        {
                            this.txtPartya.Text = ctapp.PARTYA;//甲方
                        }
                        if (objcs.Count() > 0)//如果数据存在
                        {
                            this.txtPartyb.Text = objcs.FirstOrDefault();//将取回的数据赋给乙方文本框
                        }
                        else
                        {
                            this.txtPartyb.Text = ctapp.PARTYB;//乙方
                        }
                    }
                    else //如果全部未从组织架构中获取数据时执行
                    {
                        this.txtPartyb.Text = ctapp.PARTYB;//乙方
                        this.txtPartya.Text = ctapp.PARTYA;//甲方
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
        #endregion

        #region 上传附件
        void cmsfc_UpdateContractPrintingCompleted(object sender, UpdateContractPrintingCompletedEventArgs e)//上传附件
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSFULUPLOAD", "CONTRACTATTACHMENTS"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSFULUPLOAD", "CONTRACTATTACHMENTS"));
                        }
                    }
                    RefreshUI(refreshType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("合同打印后上传附件Completed事件", "OA", "T_OA_CONTRACTPRINT", "上传合同附件时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 打印
        void cmsfc_ContractPrintingAddCompleted(object sender, ContractPrintingAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSFULPRINT", "APPLICATIONSFORCONTRACTS"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSFULPRINT", "APPLICATIONSFORCONTRACTS"));
                            this.actions = Action.Edit;
                        }
                    }
                    RefreshUI(refreshType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("添加合同打印Completed事件", "OA", "T_OA_CONTRACTPRINT", "添加合同打印时返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 类型、模板列表处理
        private void ContractTypeID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContractTypeID.SelectedItem == null) return;
            string strContractTypeID = ((T_OA_CONTRACTTYPE)ContractTypeID.SelectedItem).CONTRACTTYPEID;
            cmsfc.GetContractTypeTemplateNameByContractTypeInfosCompleted += new EventHandler<GetContractTypeTemplateNameByContractTypeInfosCompletedEventArgs>(cmsfc_GetContractTypeTemplateNameByContractTypeInfosCompleted);
            cmsfc.GetContractTypeTemplateNameByContractTypeInfosAsync(strContractTypeID);

        }

        void cmsfc_GetContractTypeTemplateNameByContractTypeInfosCompleted(object sender, GetContractTypeTemplateNameByContractTypeInfosCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    HasChosenTemplate.Items.Clear();
                    List<T_OA_CONTRACTTEMPLATE> tmpTemplate = e.Result.ToList();
                    T_OA_CONTRACTTEMPLATE TemplateT = new T_OA_CONTRACTTEMPLATE();
                    TemplateT.CONTRACTTEMPLATEID = "";
                    TemplateT.CONTRACTTEMPLATENAME = "请选择";
                    tmpTemplate.Insert(0, TemplateT);

                    foreach (T_OA_CONTRACTTEMPLATE obj in tmpTemplate)
                    {
                        HasChosenTemplate.Items.Add(obj);
                    }
                    HasChosenTemplate.DisplayMemberPath = "CONTRACTTEMPLATENAME";//类型名称
                    this.HasChosenTemplate.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("获取合同类型模版Completed事件", "OA", "T_OA_CONTRACTPRINT", "cmsfc_GetContractTypeTemplateNameByContractTypeInfosCompleted", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region ComBox模板处理事件
        private void HasChosenTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HasChosenTemplate.SelectedIndex < 1) return;
            T_OA_CONTRACTTEMPLATE TemplageText = new T_OA_CONTRACTTEMPLATE();
            TemplageText = (T_OA_CONTRACTTEMPLATE)HasChosenTemplate.SelectedItem;
            //ContractText.Text = TemplageText.CONTENT;

            ContractText.RichTextBoxContext = TemplageText.CONTENT;
        }
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ContractText.Height = ((Grid)sender).ActualHeight * 0.35;
        }
        #endregion

        #region 选择合同
        private void Select()
        {
            printForm = new ContractPrintingControl();
            EntityBrowser browser = new EntityBrowser(printForm);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            if (printForm != null)
            {
                printView = printForm.printInfo;
                GetSelectContractApplications(printView);
            }
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            if (actions == Action.Print)
            {
                return Utility.GetResourceStr("PRINTS", "APPLICATIONSFORCONTRACTS");
            }
            else if (actions == Action.FromAnnex)
            {
                return Utility.GetResourceStr("UPLOADL", "CONTRACTATTACHMENTS");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "CONTRACTPRINTING");
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
                case "3":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "4":
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
            if (actions != Action.Read && actions != Action.Print)
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
            if (actions == Action.Print)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "4",
                    Title = Utility.GetResourceStr("SELECTCONTRACT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };

                items.Add(item);

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "3",
                    Title = Utility.GetResourceStr("PRINT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_print.png"
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
                    string StartTime = string.Empty;//开始时间
                    string StrEndTime = string.Empty;//结束时间
                    string StrFlag = string.Empty;
                    string StrDateSigned = string.Empty;//签订时间

                    StartTime = this.StartTime.Text.ToString();
                    StrEndTime = this.EndTime.Text.ToString();
                    StrDateSigned = this.DateSigned.Text.ToString();

                    DateTime DtStart = new DateTime();
                    DateTime DtEnd = new DateTime();
                    DateTime DtStrDateSigne = new DateTime();//签订时间

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
                    if (!string.IsNullOrEmpty(StrDateSigned))//签订时间
                    {
                        DtStrDateSigne = System.Convert.ToDateTime(StrDateSigned);
                        if (DtStrDateSigne <= DateTime.Now)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATECOMPARECURENTTIME", "DATESIGNED"));
                            this.StartTime.Focus();
                            RefreshUI(RefreshedTypes.HideProgressBar);//关闭进度条动画
                            return;
                        }
                    }
                    if (this.rbtYes.IsChecked == true)
                    {
                        StrFlag = "1";
                    }
                    if (this.RbtNo.IsChecked == true)
                    {
                        StrFlag = "0";
                    }
                    if (actions == Action.Print) //打印合同
                    {
                        cprinting = new T_OA_CONTRACTPRINT();
                        cprinting.CONTRACTPRINTID = System.Guid.NewGuid().ToString();
                        cprinting.T_OA_CONTRACTAPP = ctapp;//合同申请ID
                        cprinting.NUM = Convert.ToInt32(nudBalanceMonth.Value);//打印份数
                        cprinting.ISUPLOAD = "0";
                        cprinting.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                        cprinting.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;//所属用户ID
                        cprinting.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;//所属用户名
                        cprinting.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//所属公司ID
                        cprinting.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//所属部门ID
                        cprinting.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//所属岗位ID
                        cprinting.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                        cprinting.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                        cprinting.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                        cprinting.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人
                        //ctrFile.FormID = cprinting.CONTRACTPRINTID;//上传附件
                        //ctrFile.Save();

                        cmsfc.ContractPrintingAddAsync(cprinting);
                    }
                    else //上传附件
                    {
                        cprinting.T_OA_CONTRACTAPP = ctapp;//合同申请ID
                        cprinting.SIGNDATE = DtStrDateSigne;//签订时间
                        //if (cprinting.ISUPLOAD == "0")
                        //{
                        //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTSELECTACCESSORIES"));
                        //    RefreshUI(RefreshedTypes.ProgressBar);//关闭进度条动画
                        //    return;
                        //}
                        //else
                        //{
                        //    cprinting.ISUPLOAD = "1";
                        //}
                        cprinting.ISUPLOAD = "1";
                        cprinting.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                        cprinting.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
                        //ctrFile.FormID = cprinting.CONTRACTPRINTID;//上传附件
                        //ctrFile.Save();

                        cmsfc.UpdateContractPrintingAsync(cprinting);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.SendException("保存Save函数", "OA", "T_OA_CONTRACTPRINT", "保存合同打印返回错误", ex, ExceptionLevel.Middle, ExceptionType.Error);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 验证
        private bool Check()
        {
            if (this.nudBalanceMonth.Value <= 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTLESSTHANZERO", "PRINTCOPIES"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.nudBalanceMonth.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(this.ContractTitle.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("PLEASESELECTACONTRACTTOPRINT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.ContractTitle.Focus();
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

        #region 甲方
        private void btnLookUpPartya_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    cprinting.T_OA_CONTRACTAPP.PARTYA = companyInfo.ObjectID;//甲方
                    txtPartya.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 乙方
        private void btnLookUpPartyb_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    cprinting.T_OA_CONTRACTAPP.PARTYB = companyInfo.ObjectID;//乙方
                    txtPartyb.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region rbtYes_Click
        private void rbtYes_Click(object sender, RoutedEventArgs e)//商务
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;
        }
        #endregion

        #region RbtNo_Click
        private void RbtNo_Click(object sender, RoutedEventArgs e)//人事
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;
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

        #region ICPOperate 接口
        public void GetCPSearchCondition()
        {

        }

        public void GetPreview()
        {

        }

        public void GetPrintPage(int PageStart, int PageEnd, int PageCount, int Pagesize)
        {
            PrintExtensions.GetPrintPage(this);
        }

        public bool ISHasMorePage()
        {
            return true;
        }

        public int SetPageSize()
        {
            return 5;
        }

        public string ShowMarks1()
        {
            return "";
        }

        public string ShowMarks2()
        {
            return "";
        }
        #endregion

        #region IPrintPage 接口
        public double PageMaxHeight
        {
            set { this.MaxHeight = value; }
            get { return this.MaxHeight; }
        }

        public List<T_OA_CONTRACTAPP> Source
        {
            set { throw new NotImplementedException(); }
        }
        #endregion


        public System.Windows.Data.PagedCollectionView GetPageView()
        {
            throw new NotImplementedException();
        }

        public event RptRefreshedHandler OnRptRefreshed;
    }
}
