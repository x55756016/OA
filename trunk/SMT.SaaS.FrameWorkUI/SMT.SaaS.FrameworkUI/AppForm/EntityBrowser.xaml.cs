/// <summary>
/// Log No.： 1
/// Modify Desc： 显示KPI打分，记录业务ID和流程模块关联ID（表单提交）
/// Modifier： 冉龙军
/// Modify Date： 2010-09-07
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Common;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Windows.Browser;


namespace SMT.SaaS.FrameworkUI
{
    public partial class EntityBrowser : System.Windows.Controls.Window, IBusinessObject
    {
        public object EntityEditor { get; set; }
        public object Entity { get; set; }
        public AuditControl.AuditControl AuditCtrl { get; set; }
        public KPIControl.KPIScoring KPICtrl { get; set; }
        private Saas.Tools.OrganizationWS.OrganizationServiceClient Orgws;
        private Saas.Tools.HrCommonServiceWS.HrCommonServiceClient HrCommws;
        public RowDefinition EntityBrowseToolBar;

        /// <summary>
        /// 保存并提交
        /// </summary>
        public ImageButton BtnSaveSubmit=new ImageButton();

        public FormTypes FormType { get; set; }
        // 1s 冉龙军
        /// <summary>
        /// 流程打分StepCode
        /// </summary>
        public string FlowStepCode { get; set; }
        /// <summary>
        /// 关闭打分提醒消息的ID
        /// </summary>
        public string RemindGuid { get; set; }
        /// <summary>
        /// 是否KPI打分
        /// </summary>
        public string IsKpi { get; set; }
        /// <summary>
        /// 关闭打分任务消息的ID
        /// </summary>
        public string MessgeID { get; set; }
        // 1e
        Control EditorCtrl;
        TabControl TabContainer;//基本信息面板
        // TabItem tbAudit = new TabItem();
        TabItem tbKPI = new TabItem();

       
        //public SMT.SaaS.FrameworkUI.ToolBar btnToolBar = new ToolBar();


        public EntityBrowser(object editor)
        {
            BtnSaveSubmit.Click += new RoutedEventHandler(btnSubmit_Click);
            HrCommws = new Saas.Tools.HrCommonServiceWS.HrCommonServiceClient();
            Orgws = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
            HrCommws.GetAppConfigByNameCompleted += new EventHandler<Saas.Tools.HrCommonServiceWS.GetAppConfigByNameCompletedEventArgs>(HrCommws_GetAppConfigByNameCompleted);
            Orgws.UpdateCheckStateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(Orgws_UpdateCheckStateCompleted);
            InitializeComponent();

            EditorCtrl = editor as Control;
            EntityBrowseToolBar = this.ToolBarRow;
            // 判断当前加载项是否是流程
            if (editor.GetType().ToString() == "SMT.FlowDesigner.FlowPanel" || editor.GetType().ToString() == "SMT.FlowDesigner.TaskPanel")
            {
                TabContainer = new TabControl();
                Container.Children.Add(EditorCtrl);
                pnlContainer.Visibility = Visibility.Collapsed;
            }
            else
            {

                TabContainer = Common.Utility.FindChildControl<TabControl>(EditorCtrl);
                if (TabContainer == null)
                {
                    TabContainer = new TabControl();
                    TabItem item = new TabItem();
                    item.Header = Common.Utility.GetResourceStr("BASEINFO");

                    TabContainer.Style = (Style)Application.Current.Resources["TabControlStyle"];
                    item.Style = (Style)Application.Current.Resources["TabItemStyle"];

                    item.Content = EditorCtrl;
                    TabContainer.Items.Add(item);


                    pnlContainer.Children.Add(TabContainer);

                    PnlAudit.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //加载当前的信息
                    pnlContainer.Children.Add(EditorCtrl);

                }
            }
            ViewContainer.Visibility = Visibility.Collapsed;
            if (editor is IEntityEditor)
                ((IEntityEditor)editor).OnUIRefreshed += new UIRefreshedHandler(editor_OnUIRefreshed);

            EntityEditor = editor;
            AddAuditControl();
            GenerateEntityTitle();
            //beyond 隐藏kpi
            AddKPIControl();
            //帮助注释掉
            //if (toolBar1 != null)
            //{
            //    toolBar1.ButtonHelp.Click += new RoutedEventHandler(ButtonHelp_Click);
            //}
            //设置entitybrowser内部显示区域高度，以显示垂直进度条
            try
            {
                if (ParentWindow != null)
                {
                    SVShowContent.Width = ParentWindow.ActualWidth;
                    SVShowContent.Height = ParentWindow.ActualHeight - toolBar1.ActualHeight - AuditCtrl.ActualHeight;
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(ex.ToString());

            }
        }
        /// <summary>
        /// xiedx
        /// 2012-8-31
        /// 获取表单中文名
        /// try-catch-finally
        //首先判断是否有审核的，再判断ModelCode传正确否，有的话就赋值，没有就传中文名，最后执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void ButtonHelp_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (FormType == FormTypes.New)
        //    //{
        //    //if (EntityEditor is IAudit && AuditCtrl != null)
        //    //{
        //    //    ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);
        //    //    string str = AuditCtrl.AuditEntity.ModelCode;
        //    //}
        //    //}


        //    //string strName = "";
        //    //strName = "事项审批";
        //    //string aa = Server.UrlEncode(strName);
        //    //string url = "http://localhost:60487/_Samples/showhelp.aspx?menuname=" + aa;
        //    //Response.Redirect(url);
        //    string strModelCode = null;
        //    string strEntityName = null;
        //    string strUrl = "";

        //    try
        //    {
        //        if (EntityEditor is IAudit && AuditCtrl != null)
        //        {
        //            ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);
        //            strModelCode = AuditCtrl.AuditEntity.ModelCode;
                   
        //            strUrl = "http://demo.smt-online.net/New/Services/ckeditor/showhelp.aspx?menucode=ModelCode";
        //            strUrl = strUrl.Replace("ModelCode", strModelCode);
        //        }
        //        else
        //        {
        //            strEntityName = ((IEntityEditor)EntityEditor).GetTitle();
        //            strEntityName = HttpUtility.UrlEncode(strEntityName);
        //           // string bb = HttpUtility.UrlDecode(strEntityName);
        //            strUrl = "http://demo.smt-online.net/New/Services/ckeditor/showhelp.aspx?menuname=EntityName";
        //            strUrl = strUrl.Replace("EntityName", strEntityName);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        strEntityName = ((IEntityEditor)EntityEditor).GetTitle();
        //       // strEntityName = "员工劳动合同";
        //        strEntityName = HttpUtility.UrlEncode(strEntityName);
        //       // string bb = HttpUtility.UrlDecode(strEntityName);
        //        strUrl = "http://demo.smt-online.net/New/Services/ckeditor/showhelp.aspx?menuname=EntityName";
        //        strUrl = strUrl.Replace("EntityName", strEntityName);
        //    }
        //    finally
        //    {
               
        //        HtmlWindow wd = HtmlPage.Window;
        //        //string strHost = Application.Current.Resources["PlatformWShost"].ToString().Split('/')[0];
               
        //        //strUrl = "http://" + strHost + "/" + strUrl;
        //        //strUrl = "http://172.16.1.57/New/Services/ckeditor/Default2.aspx";
        //        // strUrl = "http://localhost:60487/_Samples/showhelp.aspx?menuname=EntityName";
        //       // strUrl = "http://demo.smt-online.net/New/Services/ckeditor/showhelp.aspx?menuname=ModelCodeOrEntityName";
        //        //strUrl = strUrl.Replace("ModelCodeOrEntityName", strModelCodeOrEntityName);
        //        Uri uri = new Uri(strUrl);
        //        //wd.Navigate(uri, "_bank");
        //        HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
        //        options.Directories = false;
        //        options.Location = false;
        //        options.Menubar = false;
        //        options.Status = false;
        //        options.Toolbar = false;
        //        options.Status = false;
        //        options.Resizeable = true;
        //        options.Left = 280;
        //        options.Top = 100;
        //        options.Width = 800;
        //        options.Height = 600;
        //        // HtmlPage.PopupWindow(uri, AssemblyName, options);
        //        string strWindow = System.DateTime.Now.ToString("yyMMddHHmsssfff");
        //        wd.Navigate(uri, strWindow, "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");
        //    }
        //}

      

        void Orgws_UpdateCheckStateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED"));
        }

        void HrCommws_GetAppConfigByNameCompleted(object sender, Saas.Tools.HrCommonServiceWS.GetAppConfigByNameCompletedEventArgs e)
        {
            string[] result = e.Result.Split(';');
            if (result[0] == "true" && result[1] == SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
            {
                //beyond隐藏手动审核
                //Manubtn.Visibility = Visibility.Visible;
            }
        }

        public void Show()
        {
            if (FormType == FormTypes.Audit)
                AddAuditControl();
            //beyond 隐藏kpi
            AddKPIControl();
            base.Show();
        }

        void editor_OnUIRefreshed(object sender, UIRefreshedEventArgs args)
        {
            if (args != null)
            {
                //if (args.RefreshedType == "1")
                //{
                //    this.Close();
                //    ReloadData();
                //}
                //else if (args.RefreshedType == "0")
                //{
                //    this.Close();
                //}
                switch (args.RefreshedType)
                {
                    case RefreshedTypes.All:
                        GenerateLeftMenu();
                        GenerateToolBar();
                        GenerateEntityTitle();
                        ReloadData();
                        break;

                    case RefreshedTypes.EntityInfo:
                        GenerateEntityTitle();
                        break;

                    case RefreshedTypes.LeftMenu:
                        GenerateLeftMenu();
                        break;

                    case RefreshedTypes.ToolBar:
                        GenerateToolBar();
                        break;

                    case RefreshedTypes.Close:
                        this.Close();
                        break;

                    case RefreshedTypes.CloseAndReloadData:
                        this.Close();
                        ReloadData();
                        break;
                    case RefreshedTypes.AuditInfo:
                        BindAudit();
                        // 1s 冉龙军
                        //beyond 隐藏kpi
                        //BindKPI();
                        // 1e
                        break;
                    case RefreshedTypes.ProgressBar:
                        ShowProgressBar();
                        break;

                    case RefreshedTypes.ShowProgressBar:
                        ShowProgressBars();
                        break;
                    case RefreshedTypes.HideProgressBar:
                        HideProgressBars();
                        break;
                    case RefreshedTypes.UploadBar:
                        ShowUploadBar();
                        break;
                    case RefreshedTypes.HideAudit://隐藏审核控件
                        BindPnlAudit();
                        break;
                    case RefreshedTypes.ShowAudit:
                        ShowPnlAudit();//显示审核控件
                        break;

                }
            }
        }

        //显示保存数据滚动条
        void ShowProgressBar()
        {
            if (SmtProgressBar.Visibility == Visibility.Visible)
            {
                SmtLoading.Stop();
                PARENT.Visibility = Visibility.Collapsed;
                SmtProgressBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                SmtProgressBar.Visibility = Visibility.Visible;
                PARENT.Visibility = Visibility.Visible;
                SmtLoading.Start();
            }
        }

        #region 操作当前界面中的加载动画
        void ShowProgressBars()
        {
            PARENT.Visibility = Visibility.Visible;
            SmtProgressBar.Visibility = Visibility.Visible;
            SmtLoading.Start();
        }

        public void HideProgressBars()
        {
            PARENT.Visibility = Visibility.Collapsed;
            SmtProgressBar.Visibility = Visibility.Collapsed;
            SmtLoading.Stop();
        }
        #endregion


        //显示保存数据滚动条
        void CloseProgressBar()
        {
            SmtLoading.Stop();
            PARENT.Visibility = Visibility.Collapsed;
            SmtProgressBar.Visibility = Visibility.Collapsed;
        }

        //显示上传数据的进度条
        void ShowUploadBar()
        {
            SmtLoading.Visibility = Visibility.Collapsed;
            if (UploadBar.Visibility == Visibility.Visible)
            {
                UploadBar.Stop();
                PARENT.Visibility = Visibility.Collapsed;
                SmtProgressBar.Visibility = Visibility.Collapsed;
                UploadBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                UploadBar.Visibility = Visibility.Visible;
                PARENT.Visibility = Visibility.Visible;
                UploadBar.Start();
                UploadBar.Visibility = Visibility.Visible;
            }
        }

        private void ShowEntityContainer()
        {
            EntityContainer.Visibility = Visibility.Visible;
            ViewContainer.Visibility = Visibility.Collapsed;
        }

        private void ShowViewContainer()
        {
            ViewContainer.Visibility = Visibility.Visible;
            EntityContainer.Visibility = Visibility.Collapsed;
        }

        public void HideLeftMenu()
        {
            ViewLeftMenus.Visibility = Visibility.Collapsed;
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            if (EntityEditor is IEntityEditor)
                ((IEntityEditor)EntityEditor).DoAction(((ImageButton)sender).Tag.ToString());
        }

        void CTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = ViewLeftMenus.CTreeView.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            NavigateItem itm = selectedItem.Tag as NavigateItem;

            if (string.IsNullOrEmpty(itm.Url))
            {
                ShowEntityContainer();
            }
            else
            {
                ContentFrame.Navigate(new Uri(itm.Url, UriKind.Relative));
                ShowViewContainer();
            }
        }
        private void GenerateLeftMenu()
        {
            if (EntityEditor is IEntityEditor)
            {

                //初始化左边导航
                ViewLeftMenus.CTreeView.Items.Clear();
                //ViewLeftMenus.Visibility = Visibility.Collapsed;
                ViewLeftMenus.CTreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(CTreeView_SelectedItemChanged);

                List<NavigateItem> items = ((IEntityEditor)EntityEditor).GetLeftMenuItems();
                foreach (var item in items)
                {
                    TreeViewItem myItem = new TreeViewItem();
                    myItem.Header = item.Title;
                    myItem.Tag = item;
                    ViewLeftMenus.CTreeView.Items.Add(myItem);
                }
                if (items.Count < 2)
                {
                    ViewLeftMenus.Visibility = Visibility.Collapsed;
                }
            }
        }
        string state = "";
        private ImageButton Manubtn;
        private void GenerateToolBar()
        {
            //初始化菜单按钮
            toolBar1.ButtonContainer.Children.Clear();

            // string state = "";
            //添加权限按钮
            if (EntityEditor is IAudit)
            {
                state = ((IAudit)EntityEditor).GetAuditState();
                if (FormType == FormTypes.New)
                {
                    //state = "";
                }

                if (state != "-1" && Convert.ToInt32(CheckStates.Approved).ToString() != state
                    && Convert.ToInt32(CheckStates.UnApproved).ToString() != state)
                {
                    ImageButton btn;
                    string img;
                    if (state == Convert.ToInt32(CheckStates.Approving).ToString()
                        || state == Convert.ToInt32(CheckStates.WaittingApproval).ToString()
                        || state == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        //手动审核
                        Manubtn = new ImageButton();
                        Manubtn.TextBlock.Text = UIHelper.GetResourceStr("MANUALAUDIT");
                        img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";
                        Manubtn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                        Manubtn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                        Manubtn.Click += new RoutedEventHandler(btnManualAudit_Click);
                        Manubtn.Visibility = Visibility.Collapsed;
                        toolBar1.ButtonContainer.Children.Add(Manubtn);
                        HrCommws.GetAppConfigByNameAsync("CanManuAudit");

                        //审核通过
                        //btn = new ImageButton();
                        //btn.TextBlock.Text = UIHelper.GetResourceStr("AUDITPASS");
                        //img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png";

                        //btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                        //btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                        //btn.Click += new RoutedEventHandler(btnAudit_Click);
                        //toolBar1.ButtonContainer.Children.Add(btn);


                        //审核不通过
                        //btn = new ImageButton();
                        //btn.TextBlock.Text = UIHelper.GetResourceStr("AUDITNOTPASS");
                        //img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png";

                        //btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                        //btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                        //btn.Click += new RoutedEventHandler(btnAuditNoPass_Click);
                        //toolBar1.ButtonContainer.Children.Add(btn);
                    }

                    //if (state == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    //9是撤单状态
                    if ((state == Convert.ToInt32(CheckStates.UnSubmit).ToString() || state == "9") && (FormType == FormTypes.Edit || FormType == FormTypes.Audit) || FormType == FormTypes.Resubmit)
                    {
                        //提交审核
                        //BtnSaveSubmit = new ImageButton();
                        BtnSaveSubmit.TextBlock.Text = UIHelper.GetResourceStr("SUBMITAUDIT");
                        img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png";

                        BtnSaveSubmit.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                        BtnSaveSubmit.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                        
                        toolBar1.ButtonContainer.Children.Add(BtnSaveSubmit);
                    }
                }

                if (state == Convert.ToInt32(CheckStates.Approved).ToString()
                    || state == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    //转发
                    ImageButton btn;
                    string img;
                    btn = new ImageButton();
                    btn.TextBlock.Text = UIHelper.GetResourceStr("FORWARDRECORD");
                    img = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_schedulerules.png";

                    btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                    btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                    btn.Click += new RoutedEventHandler(btnForward_Click);
                    toolBar1.ButtonContainer.Children.Add(btn);
                }
            }

            if (EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = ((IEntityEditor)EntityEditor).GetToolBarItems();
                if (bars!=null)
                {
                    foreach (var bar in bars)
                    {
                        bool display = true;

                        if (bar.Key == "0" || bar.Key == "1")
                        {
                            if (string.IsNullOrEmpty(state) || Convert.ToInt32(CheckStates.UnSubmit).ToString() == state || (CheckStates.UnSubmit).ToString() == state)
                            {
                                display = true;
                            }
                            else
                                display = false;
                        }
                        if (display)
                        {
                            ImageButton btn = new ImageButton();
                            btn.TextBlock.Text = bar.Title;
                            btn.Tag = bar.Key;
                            btn.Image.Source = new BitmapImage(new Uri(bar.ImageUrl, UriKind.Relative));
                            btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                            btn.Click += new RoutedEventHandler(btn_Click);
                            toolBar1.ButtonContainer.Children.Insert(0, btn);
                        }
                    }
                }
            }
        }
        void btnManualAudit_Click(object sender, RoutedEventArgs e)
        {
            if (EntityEditor is IAudit)
            {
                //SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult rslt = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful;

                //((IAudit)EntityEditor).OnSubmitCompleted(rslt);

                string strKeyName = GetValueFromXMLObjectSource("Attribute", "DataValue", AuditCtrl.AuditEntity.FormID, "Name", AuditCtrl.AuditEntity.XmlObject);
                Orgws.UpdateCheckStateAsync(AuditCtrl.AuditEntity.ModelCode, strKeyName, AuditCtrl.AuditEntity.FormID, "2");
            }
        }

        /// <summary>
        /// 转发按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnForward_Click(object sender, RoutedEventArgs e)
        {
            //当表单需要审核时，才会出现转发的按钮
            if (EntityEditor is IAudit)
            {
                SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient PublicInterface = new SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient();
                //不存在出差报销的转发，改为出差申请的转发
                if (AuditCtrl.AuditEntity.ModelCode.Equals("T_OA_TRAVELREIMBURSEMENT"))
                {
                    AuditCtrl.AuditEntity.ModelCode = "T_OA_BUSINESSTRIP";
                }
                PublicInterface.GetBusinessObjectAsync(AuditCtrl.AuditEntity.SystemCode, AuditCtrl.AuditEntity.ModelCode);
                PublicInterface.GetBusinessObjectCompleted += (fo, fe) =>
                {
                    if (fe.Error == null)
                    {
                        AuditCtrl.AuditEntity.BusinessObjectDefineXML = fe.Result;

                        ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);
                        if (AuditCtrl.AuditEntity.ModelCode.Equals("T_OA_TRAVELREIMBURSEMENT"))
                        {
                            AuditCtrl.AuditEntity.ModelCode = "T_OA_BUSINESSTRIP";
                        }
                        string strExceptionMsg = string.Empty;
                        if (string.IsNullOrEmpty(AuditCtrl.AuditEntity.ModelCode))
                        {
                            strExceptionMsg = "模块代码不能为空";

                            ComfirmWindow.ConfirmationBox("转发异常：" + Utility.GetResourceStr("ERROR"), strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                            return;
                        }

                        if (string.IsNullOrEmpty(AuditCtrl.AuditEntity.FormID))
                        {
                            strExceptionMsg = "表单ID不能为空";

                            ComfirmWindow.ConfirmationBox("转发异常：" + Utility.GetResourceStr("ERROR"), strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                            return;
                        }

                        string strOwnerID = string.Empty, strXmlObjectSource = string.Empty, strAppInfo = string.Empty;
                        strXmlObjectSource = AuditCtrl.AuditEntity.XmlObject;

                        strOwnerID = GetValueFromXMLObjectSource("Attribute", "Name", "OWNERID", "DataValue", strXmlObjectSource);
                        //如果为事项审批，则获取单号
                        if (AuditCtrl.AuditEntity.ModelCode.Equals("T_OA_APPROVALINFO"))
                        {
                            //事项审批单号
                            strAppInfo = GetValueFromXMLObjectSource("Attribute", "Name", "APPROVALCODE", "DataValue", strXmlObjectSource);
                        }
                        if (string.IsNullOrWhiteSpace(strOwnerID))
                        {
                            strExceptionMsg = "转发人信息未正常获取";

                            ComfirmWindow.ConfirmationBox("转发异常：" + Utility.GetResourceStr("ERROR"), strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                            return;
                        }

                        if (strOwnerID != AuditCtrl.AuditEntity.CreateUserID)
                        {
                            strExceptionMsg = "禁止转发他人的单据";

                            ComfirmWindow.ConfirmationBox("转发异常：" + Utility.GetResourceStr("ERROR"), strExceptionMsg, Utility.GetResourceStr("CONFIRMBUTTON"));
                            return;
                        }

                        //选择转发的对象
                        SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, "6", AuditCtrl.AuditEntity.ModelCode);

                        lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
                        lookup.MultiSelected = true;   //不允许多选，避免选择对象太多，导致长时间调用WCF服务


                        lookup.SelectedClick += (obj, ev) =>
                        {
                            ObservableCollection<SMT.Saas.Tools.PersonalRecordWS.T_PF_PERSONALRECORD> entPersonalRecordList = new ObservableCollection<SMT.Saas.Tools.PersonalRecordWS.T_PF_PERSONALRECORD>();

                            if (lookup.SelectedObj != null)
                            {

                                string strDBName = GetValueFromXMLObjectSource("Name", strXmlObjectSource);
                                string strFormName = AuditCtrl.AuditEntity.ModelCode;
                                string strModelId = AuditCtrl.AuditEntity.FormID;

                                foreach (OrganizationControl.ExtOrgObj item in lookup.SelectedObj)
                                {
                                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE entEmployee = item.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                                    if (entEmployee != null)
                                    {
                                        SMT.Saas.Tools.PersonalRecordWS.T_PF_PERSONALRECORD entPersonalRecord = new Saas.Tools.PersonalRecordWS.T_PF_PERSONALRECORD();
                                        entPersonalRecord.PERSONALRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                                        entPersonalRecord.SYSTYPE = strDBName;
                                        entPersonalRecord.MODELCODE = strFormName;
                                        entPersonalRecord.MODELID = strModelId;
                                        entPersonalRecord.CHECKSTATE = "2";
                                        entPersonalRecord.OWNERID = entEmployee.EMPLOYEEID;
                                        entPersonalRecord.OWNERPOSTID = entEmployee.OWNERPOSTID;
                                        entPersonalRecord.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                                        entPersonalRecord.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                                        entPersonalRecord.CREATEDATE = DateTime.Now;
                                        entPersonalRecord.UPDATEDATE = DateTime.Now;
                                        entPersonalRecord.CONFIGINFO = SetSubmitXmlObj(strDBName, strFormName, strModelId, "VIEW");
                                        entPersonalRecord.MODELDESCRIPTION = string.Format("{0}于{1}向您转发了一张{2}单{3}，请查阅！", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName, entPersonalRecord.CREATEDATE.Value.ToString("yyyy年MM月dd日HH:mm:ss"), entPersonalRecord.MODELCODE, strAppInfo);
                                        entPersonalRecord.ISFORWARD = "1";
                                        entPersonalRecord.ISVIEW = "0";

                                        entPersonalRecordList.Add(entPersonalRecord);
                                    }
                                }
                            }

                            if (entPersonalRecordList.Count() > 0)
                            {
                                SMT.Saas.Tools.PersonalRecordWS.PersonalRecordServiceClient clinetPersonalRecord = new Saas.Tools.PersonalRecordWS.PersonalRecordServiceClient();
                                clinetPersonalRecord.AddPersonalRecordsAsync(entPersonalRecordList);
                                clinetPersonalRecord.AddPersonalRecordsCompleted += (o, pe) =>
                                {
                                    if (pe.Error == null)
                                    {
                                        ComfirmWindow.ConfirmationBox("转发成功：", "单据转发成功！", Utility.GetResourceStr("CONFIRMBUTTON"));
                                    }
                                    else
                                    {
                                        ComfirmWindow.ConfirmationBox("转发异常：", Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                                    }
                                };
                            }
                        };

                        lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                    }
                };
            }
        }

        /// <summary>
        /// 根据节点名，及指定节点的属性及其值，返回当前XML中该节点指定属性的值
        /// </summary>
        /// <param name="strNodeName">节点名</param>
        /// <param name="strCheckAttributeName">节点的属性名</param>
        /// <param name="strCheckAttributeValue">节点的属性值</param>
        /// <param name="strResAttributeName">待查的属性名</param>
        /// <param name="strXMLObjectSource">源XML</param>
        /// <returns></returns>
        private static string GetValueFromXMLObjectSource(string strNodeName, string strXMLObjectSource)
        {
            string strRes = string.Empty;
            using (XmlReader reader = XmlReader.Create(new StringReader(strXMLObjectSource)))
            {
                XElement xmlClient = XElement.Load(reader);
                var temp = from c in xmlClient.DescendantsAndSelf(strNodeName)
                           select c;
                strRes = temp.FirstOrDefault().Value;
            }

            return strRes;
        }

        /// <summary>
        /// 根据节点名，及指定节点的属性及其值，返回当前XML中该节点指定属性的值
        /// </summary>
        /// <param name="strNodeName">节点名</param>
        /// <param name="strCheckAttributeName">节点的属性名</param>
        /// <param name="strCheckAttributeValue">节点的属性值</param>
        /// <param name="strResAttributeName">待查的属性名</param>
        /// <param name="strXMLObjectSource">源XML</param>
        /// <returns></returns>
        private static string GetValueFromXMLObjectSource(string strNodeName, string strCheckAttributeName, string strCheckAttributeValue,
            string strResAttributeName, string strXMLObjectSource)
        {
            string strRes = string.Empty;
            if (string.IsNullOrWhiteSpace(strXMLObjectSource))
            {
                return string.Empty;
            }
            using (XmlReader reader = XmlReader.Create(new StringReader(strXMLObjectSource)))
            {
                XElement xmlClient = XElement.Load(reader);
                var temp = from c in xmlClient.DescendantsAndSelf(strNodeName)
                           where c.Attribute(strCheckAttributeName).Value == strCheckAttributeValue
                           select c;
                strRes = temp.FirstOrDefault().Attribute(strResAttributeName).Value;
            }

            return strRes;
        }

        /// <summary>
        /// 构造打开特定Form的查询条件xml
        /// </summary>
        /// <param name="strFormName">完整的Form名称(含命名空间)</param>
        /// <param name="strModelId">当前实体的主键ID</param>
        /// <param name="strFormType">打开Form时设置的FormType("VIEW", "EDIT", "AUDIT")</param>
        /// <returns></returns>
        public static string SetSubmitXmlObj(string strDBName, string strFormName, string strModelId, string strFormType)
        {
            StringBuilder strTemp = new StringBuilder();

            if (!string.IsNullOrEmpty(strDBName) && !string.IsNullOrEmpty(strFormName) && !string.IsNullOrEmpty(strModelId) && !string.IsNullOrEmpty(strFormType))
            {
                strTemp.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                strTemp.Append("<System>");
                strTemp.Append("	<AssemblyName>" + Utility.GetFormConfigResourceValue(strDBName + "ASSEMBLY") + "</AssemblyName>");
                strTemp.Append("	<PublicClass>" + Utility.GetFormConfigResourceValue(strDBName + "CLASS") + "</PublicClass>");
                strTemp.Append("	<ProcessName>" + Utility.GetFormConfigResourceValue(strDBName + "PROCESS") + "</ProcessName>");
                strTemp.Append("	<PageParameter>" + Utility.GetFormConfigResourceValue(strFormName) + "</PageParameter>");
                strTemp.Append("	<ApplicationOrder>" + strModelId + "</ApplicationOrder>");
                strTemp.Append("	<FormTypes>" + strFormType + "</FormTypes>");
                strTemp.Append("</System>");
            }
            return strTemp.ToString();
        }

        private Button btnSubmit;
        public void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //beyond 防止多次提交
            this.btnSubmit = sender as Button;
            this.btnSubmit.IsEnabled = false;
            
            //AuditCtrl.AuditEntity.SystemCode = "OA";
            //AuditCtrl.AuditEntity.ModelCode = "MeetingInfo";
            ManualSubmit();
            //  MessageBox.Show("btnSubmit_Click");
            //if (EntityEditor is IAudit)
            //{
            //    IAudit a = (IAudit)EntityEditor;
            //    a.SetFlowRecordEntity(AuditCtrl.AuditEntity);

            //    IEntityEditor obj = (IEntityEditor)EntityEditor;
            //    //obj.DoAction("0");
            //    AuditCtrl.Submit();
            //    this.FormType = FormTypes.Audit;

            //    //EditorCtrl = editor as Control;
            //    //pnlContainer.Children.Add(EditorCtrl);
            //    //AddAuditControl();
            //}
        }

        public void ManualSubmit()
        {
            Utility cc = new Utility();
            cc.GetBusinessObject(this, AuditCtrl.AuditEntity.SystemCode, AuditCtrl.AuditEntity.ModelCode);
        }

        /// <summary>
        /// 得到业务对象并赋值后正式提交
        /// </summary>
        /// <param name="e"></param>
        public void GetBusinessObjectCompleted(Saas.Tools.PublicInterfaceWS.GetBusinessObjectCompletedEventArgs e)
        {
            //beyond 防止多次提交
            if (this.btnSubmit != null)
            {
                this.btnSubmit.IsEnabled = true;
            }
            //  MessageBox.Show("GetBusinessObjectCompleted");
            if (EntityEditor is IAudit)
            {
                if (e.Result != null)
                    AuditCtrl.AuditEntity.BusinessObjectDefineXML = e.Result;


                ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);
                //if (e.Result!=null)
                //MessageBox.Show(e.Result);
                //else
                //    MessageBox.Show("False");

                IEntityEditor obj = (IEntityEditor)EntityEditor;
                //obj.DoAction("0");
                AuditCtrl.Submit();
                this.FormType = FormTypes.Audit;

            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (EntityEditor is IAudit)
            {
                AuditCtrl.Submit(SMT.SaaS.FrameworkUI.AuditControl.AuditControl.AuditAction.Pass);
            }
        }
        void btnAuditNoPass_Click(object sender, RoutedEventArgs e)
        {
            if (EntityEditor is IAudit)
            {
                AuditCtrl.Submit(SMT.SaaS.FrameworkUI.AuditControl.AuditControl.AuditAction.Fail);
            }
        }
        private void GenerateEntityTitle()
        {
            if (EntityEditor is IEntityEditor)
            {
                this.TitleContent = ((IEntityEditor)EntityEditor).GetTitle();
                FormTitleState.titlename.Text = ((IEntityEditor)EntityEditor).GetStatus();
            }
        }
        private void FloatableWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateLeftMenu();
            GenerateToolBar();
            GenerateEntityTitle();

            if (FormType == FormTypes.New || FormType == FormTypes.Edit)
            {
                if (this.ParentWindow.IsNotNull())
                {
                    this.ParentWindow.Closing += new EventHandler(ParentWindow_Closing);
                }
            }
        }

        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        private void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        #region 审核功能
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果

        private void AddAuditControl()
        {

            //tbAudit = new TabItem();
            // tbAudit.Style = (Style)Application.Current.Resources["TabItemStyle"];
            //tbAudit.Header = Common.Utility.GetResourceStr("AUDITINFO");

            AuditCtrl = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();
            AuditCtrl.HorizontalAlignment = HorizontalAlignment.Stretch;
            AuditCtrl.VerticalAlignment = VerticalAlignment.Stretch;
            AuditCtrl.Auditing += new EventHandler<AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            AuditCtrl.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);

            // tbAudit.Content = AuditCtrl;

            // tbAudit.Visibility = Visibility.Collapsed;
            //pnlAudit.Content = AuditCtrl;
            expander.Visibility = Visibility.Collapsed;
            expander.Header = Common.Utility.GetResourceStr("AUDITINFO");
            expander.Content = AuditCtrl;//加载审核控件
            PnlAudit.Visibility = Visibility.Visible;
            // TabContainer.Items.Add(tbAudit);
        }

        /// <summary>
        ///隐藏审核控件 
        /// </summary>
        public void BindPnlAudit()
        {
            PnlAudit.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        ///显示审核控件 
        /// </summary>
        public void ShowPnlAudit()
        {
            PnlAudit.Visibility = Visibility.Visible;
        }
        private void BindAudit()
        {
            if (EntityEditor is IAudit && AuditCtrl != null)
            {
                ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);

                AuditCtrl.BindingData();
                AuditCtrl.OnBindingDataCompleted += (obj, args) =>
                    {
                        if (state == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            //AuditCtrl.GotoState(AuditControl.AuditFormViewState.AuditStart);
                        }
                    };

                expander.Visibility = Visibility.Visible;
            }
        }

        public void SubmitAduit()
        {
            if (EntityEditor is IAudit)
            {
                ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);

                AuditCtrl.Submit();
            }


        }


        private void AuditCtrl_Auditing(object sender, AuditControl.AuditEventArgs e)
        {
            ShowProgressBar();
            if (EntityEditor is IAuditing)
            {
                ((IAuditing)EntityEditor).OnAuditing(e);
            }
        }

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
                //撤单状态
                case AuditControl.AuditEventArgs.AuditResult.CancelSubmit:
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    //todo 审核中
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
                    //todo 取消
                    SumbitCompleted();
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
                    HandError(e);
                    break;
            }

            //审核通过后需要更新UI，更新UI的时候会执行RefreshUI(RefreshedTypes.ProgressBar);
            //所以这里不需要CloseProgressBar();
        }

        private void SumbitCompleted()
        {
            if (EntityEditor is IAudit)
            {
                ((IAudit)EntityEditor).OnSubmitCompleted(auditResult);
            }
        }

        private void HandError(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs args)
        {
            //MessageBox.Show(Common.Utility.GetResourceStr("AUDITFAILURE"));
            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            string strErrorMessage = Common.Utility.GetResourceStr("AUDITFAILURE");
            try
            {
                strErrorMessage += "\r\n" + args.InnerResult.Err;
            }
            catch (Exception ex)
            {
            }
            ComfirmWindow.ConfirmationBox("审核异常：" + Utility.GetResourceStr("ERROR"), strErrorMessage, Utility.GetResourceStr("CONFIRMBUTTON"));

            CloseProgressBar();
            //MessageBox.Show();
        }
        #endregion

        #region KPI控制

        private void AddKPIControl()
        {
            tbKPI = new TabItem();
            tbKPI.Style = (Style)Application.Current.Resources["TabItemStyle"];
            tbKPI.Header = Common.Utility.GetResourceStr("KPISCORE");

            KPICtrl = new SMT.SaaS.FrameworkUI.KPIControl.KPIScoring();
            KPICtrl.HorizontalAlignment = HorizontalAlignment.Stretch;
            KPICtrl.VerticalAlignment = VerticalAlignment.Stretch;
            //KPICtrl.Auditing += new EventHandler<AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            //KPICtrl.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);

            tbKPI.Content = KPICtrl;

            tbKPI.Visibility = Visibility.Collapsed;
            //pnlAudit.Content = AuditCtrl;

            TabContainer.Items.Add(tbKPI);
        }
        // 1s 冉龙军
        private void BindKPI()
        {
            if (EntityEditor is IAudit && KPICtrl != null)
            {
                //AuditCtrl.OnBindingDataCompleted += (obj, args) =>
                //{
                //    if (state == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                //    {
                //        AuditCtrl.GotoState(AuditControl.AuditFormViewState.AuditStart);
                //    }
                //};
                // 1s 冉龙军
                //待完善
                ((IAudit)EntityEditor).SetFlowRecordEntity(KPICtrl.AuditEntity);
                KPICtrl.BindingData(this.FlowStepCode, this.RemindGuid, this.IsKpi, this.MessgeID);
                //tbKPI.Visibility = Visibility.Visible;

                // 1e

                if (this.IsKpi == "1")
                    // tbAudit.Visibility = Visibility.Collapsed;
                    expander.Visibility = Visibility.Collapsed;
            }
        }
        // 1e

        #endregion


        void ParentWindow_Closing(object sender, EventArgs e)
        {
            //FormClosedEventArgs arg = new FormClosedEventArgs();
            //arg.EntityEditor = this;
            //arg.RefreshedArgs = "ClosedForm";
            ////触发各自form的Formclosed事件
            if (EntityEditor is IClient)
            {
                IClient entityForm = (IClient)EntityEditor;
                //if (entityForm.CheckDataContenxChange())
                //{
                entityForm.ClosedWCFClient();
                //}
            }

            //this.ParentWindow.IsClose = false;
            //string Result = "";
            //ComfirmWindow cw = new ComfirmWindow();
            //cw.OnSelectionBoxClosed += (obj, result) =>
            //{               
            //    this.ParentWindow.IsClose = false;
            //    this.Close();
            //};
            //cw.SelectionBox(Utility.GetResourceStr("CLOSEWINDOW"), Utility.GetResourceStr("CLOSEWINDOWALTER"), ComfirmWindow.titlename, Result);
        }
    }
}

