using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Media;


namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class CompanySendDocManagement : BasePage,IClient
    {
                        
        #region 加载数据
                
        SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();
        SmtOADocumentAdminClient Adminc = new SmtOADocumentAdminClient();
        PermissionServiceClient PermClient = new PermissionServiceClient();
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        
        private string StrOperationFlag = ""; //用来控制 归档 和 文档发布
        T_OA_SENDDOC tmpSendDoc = new T_OA_SENDDOC();
        string StrIp = "";
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private bool auditflag = false; //提交审核标志
        private bool IsQueryBtn = false; //是否是查询按钮提交
        CheckBox SelectBox = new CheckBox();
        private string checkState = ((int)CheckStates.ALL).ToString(); //等待审核
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        SMT.SaaS.FrameworkUI.AuditControl.AuditControl audit = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();

        private SMTLoading loadbar = new SMTLoading();
        private T_SYS_ENTITYMENUCUSTOMPERM customerPermission;
        private V_BumfCompanySendDoc companysenddoc;
        private string SearchUserID=""; //当前登陆人ID
        public V_BumfCompanySendDoc Companysenddoc
        {
            get { return companysenddoc; }
            set { companysenddoc = value; }
        }

        public CompanySendDocManagement()
        {
            //Common.Userlogin(Common.CurrentLoginUserInfo);
            InitializeComponent();
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            //Utility.DisplayGridToolBarButtonUI(ToolBar, "OABUMFSENDDOC", true);
            Utility.DisplayGridToolBarButtonUI(ToolBar, "T_OA_SENDDOC", true);
            
            PARENT.Children.Add(loadbar);
            this.Loaded+=new RoutedEventHandler(CompanySendDocManagement_Loaded);
            //SendDocClient.GetHouseIssueAndNoticeInfosToMobile
            //Utility.CreateFormFromEngine("38d2b3f2-4c76-4b61-a738-332acb903644", "SMT.SaaS.OA.UI.UserControls.CompanyDocForm", "Edit");
          //TravelDictionaryComboBox.BindComboBox()
        }

        private void InitEvent()
        {
            
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
             //提交审核
            ToolBar.BtnView.Click += new RoutedEventHandler(SendDocDetailBtn_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.stpOtherAction.Visibility = Visibility.Visible;
            audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            SendDocClient.SendDocInfoUpdateCompleted += new EventHandler<SendDocInfoUpdateCompletedEventArgs>(SendDocClient_SendDocInfoUpdateCompleted);                           
            SendDocClient.GetSendDocInfosListByWorkFlowCompleted += new EventHandler<GetSendDocInfosListByWorkFlowCompletedEventArgs>(SendDocClient_GetSendDocInfosListByWorkFlowCompleted);
            SendDocClient.GetDocTypeInfosCompleted += new EventHandler<GetDocTypeInfosCompletedEventArgs>(SendDocClient_GetDocTypeInfosCompleted);
            SendDocClient.SendDocBatchDelCompleted += new EventHandler<SendDocBatchDelCompletedEventArgs>(SendDocClient_SendDocBatchDelCompleted);
            PermClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += new EventHandler<GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs>(PermClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted);
            //LoadSendDocInfos(checkState);
            SetButtonVisible();
            //Adminc.GetHouseIssueAndNoticeInfosToMobileAsync();
            //this.Loaded += new RoutedEventHandler(CompanySendDocManagement_Loaded);
            ToolBar.ShowRect();
           //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);

        }

        void PermClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted(object sender, GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    customerPermission = new T_SYS_ENTITYMENUCUSTOMPERM();
                    customerPermission = e.Result;

                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message,"aaaaaaa",e.Error.Message);
            }
        }

        

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Companysenddoc != null)
            {
                if (companysenddoc.OACompanySendDoc.CHECKSTATE == "2" || companysenddoc.OACompanySendDoc.CHECKSTATE == "3")
                {
                    CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Resubmit, Companysenddoc);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinWidth = 850;
                    browser.MinHeight = 600;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("COMPANYDOCNOTRESUBMIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "SUBMITAUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Companysenddoc = (V_BumfCompanySendDoc)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void CompanySendDocManagement_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            GetEntityLogo("T_OA_SENDDOC");
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            InitComboxSource();
            string Name = "";
            Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtOwnerName.Text = Name;
            ToolTipService.SetToolTip(txtOwnerName, Name);
            if (customerPermission == null)
                PermClient.GetCustomerPermissionByUserIDAndEntityCodeAsync(Common.CurrentLoginUserInfo.SysUserID, "T_OA_SENDDOC");
            
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Companysenddoc = null;//刷新后将选择对象置空
            LoadSendDocInfos(checkState);
        }

        

        void SendDocClient_SendDocBatchDelCompleted(object sender, SendDocBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "COMPANYDOC"));
                    LoadSendDocInfos(checkState);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", "COMPANYDOC"));
                    return;
                }
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {

            if (Companysenddoc != null)
            {
                if (!string.IsNullOrEmpty(Companysenddoc.OACompanySendDoc.SENDDOCID))
                {
                    CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Audit, Companysenddoc);
                    //CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Audit, "582b4077-6bc8-4c1a-9936-1bcea2443e5c");
                    //AuditCompanyDocForm AddWin = new AuditCompanyDocForm(FormTypes.Audit, Companysenddoc.OACompanySendDoc.SENDDOCID);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.FormType = FormTypes.Audit;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                
            }

        }        
        void LoadSendDocInfos(string checkState)
        {
            
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrTitle = ""; //标题
            string StrContent = "";//内容
            string StrStart = "";//添加文档的起始时间
            string StrEnd = "";//添加文档的结束时间
            string StrIsSave = "";//是否归档
            string StrDistrbute = "";//是否发布
            string StrGrade = "";//级别
            string StrProritity = "";//缓急
            string StrDocType = "";//文档类型
            bool IsNull = false;  //用来控制是否有查询条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //if (checkState != ((int)CheckStates.Approving).ToString())
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "OACompanySendDoc.OWNERID==@" + paras.Count().ToString();
            //    paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            //}
            if (SearchUserID == Common.CurrentLoginUserInfo.EmployeeID)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "( OWNERID =@" + paras.Count().ToString();
                paras.Add(SearchUserID);//员工ID值
                filter += " or ";
                filter += "CREATEUSERID =@" + paras.Count().ToString();//添加人的ID
                paras.Add(SearchUserID);
                filter += " ) ";
            }
            else
            {
                if (!string.IsNullOrEmpty(SearchUserID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OWNERID =@" + paras.Count().ToString();
                    paras.Add(SearchUserID);
                }
            }

            
            if (IsQueryBtn)
            {
                T_SYS_DICTIONARY GradeObj = cbxGrade.SelectedItem as T_SYS_DICTIONARY;//级别
                T_SYS_DICTIONARY ProritityObj = cbxProritity.SelectedItem as T_SYS_DICTIONARY;//缓急
                T_OA_SENDDOCTYPE sendtype = new T_OA_SENDDOCTYPE();
                switch (cbxIsSave.SelectedIndex)
                { 
                    case 0:
                        break;
                    case 1:
                        StrIsSave = "0";
                        break;
                    case 2:
                        StrIsSave = "1";
                        break;

                }
                switch (cbxDistrbute.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        StrDistrbute = "0";
                        break;
                    case 2:
                        StrDistrbute = "1";
                        break;

                }
                if (!string.IsNullOrEmpty(StrIsSave))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.ISSAVE =@" + paras.Count().ToString();//标题名称
                    paras.Add(StrIsSave);
                }
                if (!string.IsNullOrEmpty(StrDistrbute))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.ISDISTRIBUTE =@" + paras.Count().ToString();//标题名称
                    paras.Add(StrDistrbute);
                }
                
                StrTitle = this.txtSendDocTitle.Text.Trim().ToString();
                //StrContent = this.txtDocContent.Text.Trim().ToString();
                StrStart = this.dpStart.Text.Trim().ToString();
                StrEnd = this.dpEnd.Text.Trim().ToString();

                if (this.cbxGrade.SelectedIndex>0)
                {
                    StrGrade = GradeObj.DICTIONARYNAME.ToString();

                }
                if (this.cbxProritity.SelectedIndex >0)
                {
                    StrProritity = ProritityObj.DICTIONARYNAME.ToString();

                }
                if (this.cbxdoctype.SelectedIndex >0)
                {
                    sendtype = this.cbxdoctype.SelectedItem as T_OA_SENDDOCTYPE;
                    StrDocType = sendtype.SENDDOCTYPE;

                }
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    IsNull = true;
                    //if (!string.IsNullOrEmpty(filter))
                    //{
                    //    filter += " and ";
                    //}
                    //filter += "OACompanySendDoc.SENDDOCTITLE ^@" + paras.Count().ToString();//标题名称
                    //paras.Add(StrTitle);

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "@" + paras.Count().ToString() + ".Contains(OACompanySendDoc.SENDDOCTITLE) ";
                    paras.Add(StrTitle);
                }
                

                if (!string.IsNullOrEmpty(StrProritity))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.PRIORITIES ^@" + paras.Count().ToString();//缓急名称
                    paras.Add(StrProritity);
                }
                if (this.cbxdoctype.SelectedIndex >0)
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " doctype.SENDDOCTYPE ==@" + paras.Count().ToString();
                    paras.Add(sendtype.SENDDOCTYPE);
                    
                }

                if (!string.IsNullOrEmpty(StrGrade))
                {
                    IsNull = true;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OACompanySendDoc.GRADED ^@" + paras.Count().ToString();//级别名称
                    paras.Add(StrGrade);
                }
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("dtSearch"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                        return;
                    }
                    else
                    {
                        IsNull = true;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OACompanySendDoc.CREATEDATE >=@" + paras.Count().ToString();
                        paras.Add(DtStart);
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OACompanySendDoc.CREATEDATE <=@" + paras.Count().ToString();
                        paras.Add(DtEnd);

                    }
                }
                //IsQueryBtn = false;
            }

            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            SendDocClient.GetSendDocInfosListByWorkFlowAsync(dataPager.PageIndex, dataPager.PageSize, "OACompanySendDoc.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
            
            
        }
        

        void SendDocClient_GetSendDocInfosListByWorkFlowCompleted(object sender, GetSendDocInfosListByWorkFlowCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {                
                if (e.Result != null)
                {
                    
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
                
            }
            catch (Exception ex)
            {
                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        #region  绑定DataGird
        private void BindDataGrid(List<V_BumfCompanySendDoc> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion

        //归档文档信息
        void SendDocClient_GetSavedSendDocInfosCompleted(object sender, GetSavedSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 10;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;
                //DaGr.ItemsSource = infos;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
        }
        //已发布文档信息
        void SendDocClient_GetDistrbutedSendDocInfosCompleted(object sender, GetDistrbutedSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 10;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;
                //DaGr.ItemsSource = infos;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("CAUTION"),Utility.GetResourceStr("NODATA"));
                return;
            }
        }

        

        void SendDocClient_GetSendDocInfosCompleted(object sender, GetSendDocInfosCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_SENDDOC> infos = new List<T_OA_SENDDOC>(e.Result);
                //dpGrid.PageSize = 3;
                PagedCollectionView pager = new PagedCollectionView(infos);
                DaGr.ItemsSource = pager;                
            }
        }
        #endregion

        #region Combox 填充

        private void InitComboxSource()
        {
            //this.cbxGrade.SelectedIndex = 0;
            Combox_ItemSourceDocType();
            //this.cbxProritity.SelectedIndex = 0;
        }
        #endregion

        #region 填充级别信息
        private void Combox_ItemSourceDocType()
        {
            SendDocClient.GetDocTypeInfosAsync();            

        }
        void SendDocClient_GetDocTypeInfosCompleted(object sender, GetDocTypeInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    this.cbxdoctype.Items.Clear();
                    T_OA_SENDDOCTYPE doctype = new T_OA_SENDDOCTYPE();
                    doctype.SENDDOCTYPE = "请选择";
                    e.Result.Insert(0, doctype);

                    this.cbxdoctype.ItemsSource = e.Result;
                    this.cbxdoctype.DisplayMemberPath = "SENDDOCTYPE";
                    this.cbxdoctype.SelectedIndex = 0;
                    
                }
            }
        }
        
        #endregion

        #region 页面导航时代码

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #endregion

        #region 模板中checkbox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked == false)
            {
                if (SelectBox != null)
                {
                    if (SelectBox.IsChecked == true)
                    {
                        SelectBox.IsChecked = false;
                    }
                }
            }
        }
        #endregion

        #region 全选事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region DaGr_loadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_SENDDOC");
            //V_BumfCompanySendDoc OrderInfoT = (V_BumfCompanySendDoc)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;            
            //mychkBox.Tag = OrderInfoT;
            
        }
        #endregion


        #region 删除按钮事件

        void AddWin_ReloadDataEvent()
        {
            LoadSendDocInfos(checkState);
        }
        void DocTypeTemplateClient_DocTypeTemplateBatchDelCompleted(object sender, DocTypeTemplateBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("SUCCESSED"),Utility.GetResourceStr("DELETESUCCESSED","COMPANYDOC"));
                    LoadSendDocInfos(checkState);
                }
                else
                {                
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", "COMPANYDOC"));
                    return;
                }
            }
        }

        #endregion

        #region 查询按钮
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadSendDocInfos(checkState);
        }
        #endregion

        #region 公司发文归档
        private void DocTORecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Companysenddoc != null)
            {
                try
                {
                    SmtOADocumentAdminClient ArchivesClient = new SmtOADocumentAdminClient();
                    T_OA_ARCHIVES ArchiveT = new T_OA_ARCHIVES();
                    ArchiveT.ARCHIVESID = System.Guid.NewGuid().ToString();
                    ArchiveT.ARCHIVESTITLE = Companysenddoc.OACompanySendDoc.SENDDOCTITLE;
                    ArchiveT.COMPANYID = Companysenddoc.OACompanySendDoc.CREATECOMPANYID;
                    ArchiveT.CONTENT = Companysenddoc.OACompanySendDoc.CONTENT;
                    ArchiveT.CREATEDATE = System.DateTime.Now;

                    ArchiveT.RECORDTYPE = "SENDDOC";
                    ArchiveT.SOURCEFLAG = "0"; //自动导入

                    
                    ArchiveT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ArchiveT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    ArchiveT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    ArchiveT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ArchiveT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                    ArchiveT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ArchiveT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    ArchiveT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ArchiveT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    ArchiveT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                    ArchiveT.UPDATEDATE = null;
                    ArchiveT.UPDATEUSERID = "";
                    ArchiveT.UPDATEUSERNAME = "";

                    T_OA_SENDDOC SendDocT = new T_OA_SENDDOC();
                    SendDocT = Companysenddoc.OACompanySendDoc;
                    SendDocT.UPDATEDATE = System.DateTime.Now;
                    //SendDocT.UPDATEUSERID = "1";
                    SendDocT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    SendDocT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    SendDocT.ISSAVE = "1";
                    
                    tmpSendDoc = SendDocT;
                    tmpSendDoc.T_OA_SENDDOCTYPE = Companysenddoc.doctype;

                    ArchivesClient.AddArchivesCompleted += new EventHandler<AddArchivesCompletedEventArgs>(ArchivesClient_AddArchivesCompleted);
                    ArchivesClient.AddArchivesAsync(ArchiveT);

                    //SendDocClient.SendDocBatchDelAsync(DelInfosList);

                }
                catch (Exception ex)
                {
                    //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "COMPANYDOC"), Utility.GetResourceStr("CONFIRMBUTTON"));              
            }    
        }

        void ArchivesClient_AddArchivesCompleted(object sender,AddArchivesCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    StrOperationFlag = "save";
                    tmpSendDoc.ISSAVE = "1";                    
                    SendDocClient.SendDocInfoUpdateCompleted += new EventHandler<SendDocInfoUpdateCompletedEventArgs>(SendDocClient_SendDocInfoUpdateCompleted);
                    string StrReurn = "";
                    SendDocClient.SendDocInfoUpdateAsync(tmpSendDoc, StrReurn);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),e.Result.ToString());
                    return;
                }
            }
        }
        #endregion

        
        #region 公司发文详情
        private void SendDocDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            //Button DetailBtn = sender as Button;
            //V_BumfCompanySendDoc VSendDoc = DetailBtn.Tag as V_BumfCompanySendDoc;
            //SendDocDetailInfo DetailWin = new SendDocDetailInfo(VSendDoc);
            //DetailWin.Show();

            if (Companysenddoc != null)
            {
                //SendDocInfoForm DetailWin = new SendDocInfoForm(Companysenddoc);
                //之前一直用审核状态打开，不解
                //CompanyDocForm DetailWin = new CompanyDocForm(FormTypes.Audit,Companysenddoc.senddoc.SENDDOCID);
                CompanyDocForm DetailWin = new CompanyDocForm(FormTypes.Browse, Companysenddoc.senddoc.SENDDOCID);
                EntityBrowser browser = new EntityBrowser(DetailWin);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 960;
                browser.MinHeight = 520;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Companysenddoc = null;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }
        #endregion

        #region 公文文档发布

        private void IssueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Companysenddoc != null)
            {
                AddDistrbuteForm AddWin = new AddDistrbuteForm(Companysenddoc);

                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinHeight = 380;
                browser.MinWidth = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Companysenddoc = null;
                DaGr.SelectedItem = null;
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("WARING"), Utility.GetResourceStr("PLEASESELECT", "ISSUEDOCUMENT"));
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ISSUEDOCUMENT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

        }

        void SendDocClient_SendDocInfoUpdateCompleted(object sender, SendDocInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (auditflag)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSED"));
                        auditflag = false;
                    }
                    if (e.StrResult != "")//有重复数据
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.StrResult, "COMPANYDOCUMENT"));
                    }

                }
                if (StrOperationFlag == "save") //归档操作
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("PIGEONHOLESUCCESSED"));
                }
                this.LoadSendDocInfos(checkState);
            }
        }

        
        #endregion

        #region 重置按钮
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

            this.dpStart.Text = "";
            this.dpEnd.Text = "";
            txtSendDocTitle.Text = "";
            cbxGrade.SelectedIndex = 0;
            cbxdoctype.SelectedIndex = 0;
            cbxProritity.SelectedIndex = 0;
            cbxIsSave.SelectedIndex = 0;
            cbxDistrbute.SelectedIndex = 0;
            txtOwnerName.Text = "";
            SearchUserID = "";
            

        }
        #endregion


        #region 按钮事件
        //新增
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            V_BumfCompanySendDoc SendDocInfoT = new V_BumfCompanySendDoc();
            CompanyDocForm AddWin = new CompanyDocForm(FormTypes.New, SendDocInfoT);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 850;
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);


            //OAWebPart AddWin = new OAWebPart();
            //EntityBrowser browser = new EntityBrowser(AddWin);
            //browser.MinWidth = 850;
            //browser.MinHeight = 600;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);


        }

        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Companysenddoc != null)
            {
                if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Companysenddoc, "T_OA_SENDDOC", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
                {
                
                    if (companysenddoc.OACompanySendDoc.CHECKSTATE == "0" || companysenddoc.OACompanySendDoc.CHECKSTATE == "3")
                    {
                        
                        CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Edit, Companysenddoc);
                        EntityBrowser browser = new EntityBrowser(AddWin);
                        browser.FormType = FormTypes.Edit;
                        browser.MinWidth = 850;
                        browser.MinHeight = 600;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                        Companysenddoc = null;
                    }
                    else
                    {
                        //CompanyDocForm AddWin = new CompanyDocForm(FormTypes.Edit, Companysenddoc.OACompanySendDoc.SENDDOCID);
                        //EntityBrowser browser = new EntityBrowser(AddWin);
                        //browser.FormType = FormTypes.Edit;
                        //browser.MinWidth = 850;
                        //browser.MinHeight = 600;
                        //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("COMPANYDOCNOTEDIT"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                    
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            


            
        }

        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            DelInfosList = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    T_OA_SENDDOC tmpDoc = new T_OA_SENDDOC();
                    tmpDoc = (DaGr.SelectedItems[i] as V_BumfCompanySendDoc).OACompanySendDoc;
                    if (tmpDoc.CHECKSTATE == "0")
                    {
                        DelInfosList.Add(tmpDoc.SENDDOCID);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, "警告", "只有未提交的数据才能删除");
                        return;
                    }
                }

                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    if (DelInfosList.Count > 0)
                    {
                        loadbar.Start();
                        // 删除
                        SendDocClient.SendDocBatchDelAsync(DelInfosList);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, "警告", "选中项不能被删除");
                    }
                };
                com.SelectionBox("删除确定", "确定删除选中信息吗？", ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox("确认信息", "请先选择需要删除的记录", "确定");
            }

            //if (DaGr.SelectedItems.Count > 0)
            //{

            //    string Result = "";
            //    string StrTip = "";
            //    DelInfosList = new ObservableCollection<string>();
            //    ComfirmWindow com = new ComfirmWindow();
                
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {

            //        for (int i = 0; i < DaGr.SelectedItems.Count; i++)
            //        {
            //            T_OA_SENDDOC tmpDoc = new T_OA_SENDDOC();
            //            tmpDoc = (DaGr.SelectedItems[i] as V_BumfCompanySendDoc).OACompanySendDoc;
            //            string SenddocID = "";
            //            SenddocID = tmpDoc.SENDDOCID;
            //            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmpDoc,"T_OA_SENDDOC", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
            //            {
            //                if (!(DelInfosList.IndexOf(SenddocID) > -1))
            //                {
            //                    if (tmpDoc.CHECKSTATE == "0" )
            //                    {
            //                        DelInfosList.Add(SenddocID);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                            
            //                StrTip = "您不能删除您选中的第" + (i+1).ToString() + "条，标题为" +  tmpDoc.SENDDOCTITLE+"的公文信息";
            //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrTip, Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //                return;
                            
            //                //break;
            //            }
            //        }
            //        if (DelInfosList.Count > 0 )
            //        {
            //            SendDocClient.SendDocBatchDelAsync(DelInfosList);
            //        }
            //        else
            //        {                        
            //                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"));
            //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"),
            //           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                      
                        
            //        }

            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            

        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //client.DeleteOrganAsync(organDelID);
        }

        

        //提交审核
        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            auditflag = true;
            V_BumfCompanySendDoc SendDocInfoT = new V_BumfCompanySendDoc();

            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            SendDocInfoT = cb1.Tag as V_BumfCompanySendDoc;
                            break;
                        }
                    }
                }

            }


            if (!string.IsNullOrEmpty(SendDocInfoT.OACompanySendDoc.SENDDOCID))
            {
                //AddCompanySendDocInfo AddWin = new AddCompanySendDocInfo("Edit", SendDocInfoT);
                //AddWin.Show();
                //AddWin.ReloadDataEvent += new AddCompanySendDocInfo.refreshGridView(AddWin_ReloadDataEvent);

            }
            else
            {
                //MessageBox.Show("请选择需要修改的公文类型");
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "NEEDAUDITCOMPANYDOC"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "NEEDAUDITCOMPANYDOC"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
        }

        //审核
        private void btnApp_Click(object sender, RoutedEventArgs e)
        {

        }

        //模板列取消选中
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        //模板列选中
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        

        //刷新
        private void browser_ReloadDataEvent()
        {
            LoadSendDocInfos(checkState);
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                //checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_SENDDOC");
                checkState = dict.DICTIONARYVALUE.ToString();
                SetButtonVisible();
                LoadSendDocInfos(checkState);            
            }   
            
            
        }

        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿                    
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核                    
                    //NewDetailButton();
                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "1":  //审批中
                    
                    ToolBar.btnAudit.Visibility = Visibility.Collapsed;       //审核
                     //提交审核
                    ToolBar.stpOtherAction.Children.Clear();
                    
                    break;
                case "2":  //审批通过
                    
                    NewButton();
                    break;
                case "3":  //审批未通过
                    
                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "4":  //待审核
                    
                    ToolBar.stpOtherAction.Children.Clear();
                    break;
                case "5":
                    ToolBar.stpOtherAction.Children.Clear();
                    break;
            }
                        
            
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //GridHelper.SetUnCheckAll(dgOrgan);
            LoadSendDocInfos(checkState);
        }

        #endregion

        #region 流程完成动作
        /// <summary>
        /// 提交审核完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            auditResult = e.Result;
            switch (auditResult)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    //todo 审核中
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
                    //todo 取消
                    Cancel();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //todo 终审通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    //todo 审核不通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
                    //todo 审核异常
                    HandError();
                    break;
            }


        }

        private void SumbitCompleted()
        {
            try
            {
                if (tmpSendDoc != null)
                {
                    
                    tmpSendDoc.UPDATEDATE = DateTime.Now;
                    tmpSendDoc.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpSendDoc.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中                            
                            tmpSendDoc.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                            tmpSendDoc.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                            tmpSendDoc.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    string StrReturn = "";
                    SendDocClient.SendDocInfoUpdateAsync(tmpSendDoc,StrReturn);
                }


            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        



        private void Cancel()
        {
            LoadSendDocInfos(checkState);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            return;
        }

        #endregion

        #region Button
        public void NewButton()
        {

            ToolBar.stpOtherAction.Children.Clear();
            if (customerPermission != null)
            {
                ImageButton ChangeMeetingBtn = new ImageButton();
                ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png", UriKind.Relative));
                ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("ISSUEDOCUMENT");// 发布文档
                ChangeMeetingBtn.Image.Width = 16.0;
                ChangeMeetingBtn.Image.Height = 22.0;
                ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
                ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
                ChangeMeetingBtn.Click += new RoutedEventHandler(IssueBtn_Click);
                ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);
            }
            ImageButton MeetingCancelBtn = new ImageButton();
            MeetingCancelBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_settings.png", UriKind.Relative));
            MeetingCancelBtn.TextBlock.Text = Utility.GetResourceStr("RECORD");// 归档
            MeetingCancelBtn.Image.Width = 16.0;
            MeetingCancelBtn.Image.Height = 22.0;
            MeetingCancelBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            MeetingCancelBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            MeetingCancelBtn.Click += new RoutedEventHandler(DocTORecordBtn_Click);


            ToolBar.stpOtherAction.Children.Add(MeetingCancelBtn);

            //ImageButton DetailBtn = new ImageButton();
            //DetailBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            //DetailBtn.TextBlock.Text = Utility.GetResourceStr("DETAILINFO");// "查看";
            //DetailBtn.Image.Width = 16.0;
            //DetailBtn.Image.Height = 22.0;
            //DetailBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            //DetailBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            //DetailBtn.Click += new RoutedEventHandler(SendDocDetailBtn_Click);
            //ToolBar.stpOtherAction.Children.Add(DetailBtn);

        }

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DaGr.SelectedItems.Count == 0)
                return;

            //SelectMeeting = DaGr.SelectedItems[0] as V_BumfCompanySendDoc;
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count > 0 )
            {
                Companysenddoc = (V_BumfCompanySendDoc)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        //public void NewDetailButton()
        //{

        //    ToolBar.stpOtherAction.Children.Clear();
        //    ImageButton ChangeMeetingBtn = new ImageButton();
        //    ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
        //    ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("DETAILINFO");// "查看";
        //    ChangeMeetingBtn.Image.Width = 16.0;
        //    ChangeMeetingBtn.Image.Height = 22.0;
        //    ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
        //    ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
        //    ChangeMeetingBtn.Click += new RoutedEventHandler(SendDocDetailBtn_Click);
        //    ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);

        //}
        #endregion


        #region IClient 成员

        public void ClosedWCFClient()
        {
            SendDocClient.DoClose();
            PermClient.DoClose();
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

        #region 选择人员
        private void btnLookUpOwner_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    //SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位
                    //postLevel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().POSTLEVEL.ToString();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    string depName = dept.ObjectName;//部门

                    

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    
                    //txtOwnerName.Text = userInfo.ObjectName;
                    string Mobile = "";
                    string Tel = "";
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE != null)
                        Mobile = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE.ToString();
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE != null)
                        Tel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE.ToString();
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    txtOwnerName.Text = StrEmployee;
                    //txtTel.Text = userInfo.te
                    ToolTipService.SetToolTip(txtOwnerName, StrEmployee);

                    SearchUserID = userInfo.ObjectID;
                    txtOwnerName.Text = StrEmployee;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion
    }
}
    
