using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Browser;
using System.Collections.ObjectModel;
//using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
namespace SMT.SaaS.OA.UI.Views.ArchivesManagement
{
    public partial class FrmArchivesLending : BasePage,IClient
    {

        private SmtOADocumentAdminClient client;
        //private string lendingID = ""; //借阅ID
        private ObservableCollection<string> lendingDelID;
        private SMTLoading loadbar = new SMTLoading(); 

        public ObservableCollection<string> LendingID
        {
            get { return lendingDelID; }
            set { lendingDelID = value; }
        }

        
        //private CFrmLendingAdd AddFrm;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_LENDARCHIVES lendarchives;

        public T_OA_LENDARCHIVES Lendarchives
        {
            get { return lendarchives; }
            set { lendarchives = value; }
        }

        
        #region 初始化

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public FrmArchivesLending()
        {
            //Common.Userlogin(Common.CurrentLoginUserInfo);
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(FrmArchivesLending_Loaded);
        }

        void dgArchives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;

            if (grid.SelectedItems.Count > 0 )
            {
                Lendarchives = (T_OA_LENDARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void dgArchives_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Lendarchives = (T_OA_LENDARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmArchivesLending_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_LENDARCHIVES", true);
            PARENT.Children.Add(loadbar);
            InitEvent();
            dgArchives.CurrentCellChanged += new EventHandler<EventArgs>(dgArchives_CurrentCellChanged);
            dgArchives.SelectionChanged += new SelectionChangedEventHandler(dgArchives_SelectionChanged);
            GetEntityLogo("T_OA_LENDARCHIVES");
        }

        private void InitEvent()
        {
            //lendingDelID = new ObservableCollection<string>();
            client = new SmtOADocumentAdminClient();            
            client.GetLendingListByUserIdCompleted += new EventHandler<GetLendingListByUserIdCompletedEventArgs>(client_GetLendingListByUserIdCompleted);
            client.DeleteArchivesLendingCompleted += new EventHandler<DeleteArchivesLendingCompletedEventArgs>(client_DeleteArchivesLendingCompleted);
            client.IsArchivesCanBrowserCompleted += new EventHandler<IsArchivesCanBrowserCompletedEventArgs>(client_IsArchivesCanBrowserCompleted);
            client.AddArchivesCompleted += new EventHandler<AddArchivesCompletedEventArgs>(client_AddArchivesCompleted);
            client.AddArchivesReturnCompleted += new EventHandler<AddArchivesReturnCompletedEventArgs>(client_AddArchivesReturnCompleted);
            //client.SubmitCommentLendingCompleted += new EventHandler<SubmitCommentLendingCompletedEventArgs>(client_SubmitCommentLendingCompleted);
            //ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(btnRead_Click);
            //ToolBar.btnRead.Click += new RoutedEventHandler(btnRead_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("VIEW")).Click += new RoutedEventHandler(btnRead_Click);
            
            //ToolBar.cbxCheckState.Items.Remove()
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
            SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_LENDARCHIVES");
            ToolBar.ShowRect();
            //LoadData();
            //SetButtonVisible();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {
                if (Lendarchives.CHECKSTATE == "2" || Lendarchives.CHECKSTATE == "3")
                {
                    ArchivesLendingForm form = new ArchivesLendingForm(Action.ReSubmit, Lendarchives.LENDARCHIVESID, checkState);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinWidth = 495;
                    browser.MinHeight = 270;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ARCHIVELENDINGNOTRESUBMIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }

        }

                
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //if (checkState != ((int)CheckStates.Approving).ToString())
            //{
            //    filter += "archivesLending.OWNERID==@" + paras.Count().ToString();
            //}
            //paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_ARCHIVES.RECORDTYPE ^@" + paras.Count().ToString();
                paras.Add(txtSearchType.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtSearchTitle.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_ARCHIVES.ARCHIVESTITLE ^@" + paras.Count().ToString();
                paras.Add(txtSearchTitle.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            //GetLoginUserInfo(loginUserInfo);            
            loadbar.Start();
            client.GetLendingListByUserIdAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }

              

       
        #endregion


        #region 完成事件

        private void client_AddArchivesReturnCompleted(object sender, AddArchivesReturnCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result == "")
                    {
                        //HtmlPage.Window.Alert("归还档案成功！");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("RETRUNSUCCESSED", "ARCHIVE"));
                        LoadData();
                    }
                    else
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_AddArchivesCompleted(object sender, AddArchivesCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("新增档案借阅成功！");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "ARCHIVELENDING"));
                        LoadData();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_IsArchivesCanBrowserCompleted(object sender, IsArchivesCanBrowserCompletedEventArgs e)
        {
            try
            {
                if (e.Result)
                {
                    CFrmArchivesBrowser BrowserFrm = new CFrmArchivesBrowser(lendingDelID[0]);
                    BrowserFrm.Title = "查看档案";
                    BrowserFrm.Show();
                }
                else
                {
                    HtmlPage.Window.Alert("此档案不能被查看！");
                }
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Alert(ex.ToString());
            }
        }

        private void client_DeleteArchivesLendingCompleted(object sender, DeleteArchivesLendingCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Result.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    //提示删除档案借阅删除成功
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "ARCHIVELENDING"));
                }
                LoadData();
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetLendingListByUserIdCompleted(object sender, GetLendingListByUserIdCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Result != null)
                {                    
                    BindDataGrid(e.Result.ToList(),e.pageCount);
                }
                else
                {
                    BindDataGrid(null,0);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_LENDARCHIVES> obj,int pageCount)
        {            
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                dgArchives.ItemsSource = null;
                return;
            }
            dgArchives.ItemsSource = obj;
        }
        #endregion        


        #region  查询、新增、修改、删除
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {           
            LoadData();
        }

        //修改按钮事件
        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> lendingID = GridHelper.GetSelectItem(dgArchives,"myChkBox",Action.Edit);
            if (lendingID == null || lendingID.Count == 0)
            {
                //HtmlPage.Window.Alert("请先选择需要修改的借阅记录！");
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));

            }
            else
            {
                ArchivesLendingForm form = new ArchivesLendingForm(Action.Edit, lendingID[0], checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinWidth = 495;
                browser.MinHeight = 270;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

                //AddFrm = new CFrmLendingAdd(Action.Edit, lendingID[0], checkState);
                //AddFrm.Title = Utility.GetResourceStr("EDITTITLE","LICENSELENDING"); 
                //AddFrm.Show();
                //AddFrm.ReloadDataEvent += new BaseForm.refreshGridView(AddFrm_ReloadDataEvent);
            }
        }

        private void AddFrm_ReloadDataEvent()
        {
            LoadData();
        }



        //删除按钮事件
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dgArchives.SelectedItems.Count > 0)
            {

                string Result = "";
                string errorMsg = "";
                lendingDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < dgArchives.SelectedItems.Count; i++)
                    {
                        string LendArchivesID = "";
                        LendArchivesID = (dgArchives.SelectedItems[i] as T_OA_LENDARCHIVES).LENDARCHIVESID;
                        if ((dgArchives.SelectedItems[i] as T_OA_LENDARCHIVES).CHECKSTATE == "0" || (dgArchives.SelectedItems[i] as T_OA_LENDARCHIVES).CHECKSTATE == "3")
                        {
                            if (!(lendingDelID.IndexOf(LendArchivesID) > -1))
                            {
                                lendingDelID.Add(LendArchivesID);
                            }
                        }
                    }
                    if (lendingDelID.Count > 0)
                    {
                        client.DeleteArchivesLendingAsync(lendingDelID);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTITEMSNOTDELETE"));
                    }

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            client.DeleteArchivesLendingAsync(lendingDelID);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //AddFrm = new CFrmLendingAdd(Action.Add, "", checkState);
            ////AddFrm.Title = "新增借阅记录";
            //AddFrm.Title = Utility.GetResourceStr("ADDTITLE", "ARCHIVELENDING"); 
            //AddFrm.Show();
            //AddFrm.ReloadDataEvent += new BaseForm.refreshGridView(AddFrm_ReloadDataEvent);

            ArchivesLendingForm form = new ArchivesLendingForm(Action.Add, "", checkState);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 400;
            browser.MinHeight = 200;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }        

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {                
                ArchivesLendingForm form = new ArchivesLendingForm(Action.Read, Lendarchives.LENDARCHIVESID, checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 495;
                browser.MinHeight = 270;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Lendarchives = null;
                
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }

            //if (Lendarchives != null)
            //{
            //    ArchivesViewForm form = new ArchivesViewForm(Lendarchives.LENDARCHIVESID);
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinWidth = 445;
            //    browser.MinHeight = 310;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            
        }
  

        private void browser_ReloadDataEvent()
        {
            LoadData();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {
                if (Lendarchives.CHECKSTATE == "0" || Lendarchives.CHECKSTATE == "3")
                {
                    ArchivesLendingForm form = new ArchivesLendingForm(Action.Edit, Lendarchives.LENDARCHIVESID, checkState);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinWidth = 495;
                    browser.MinHeight = 270;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ARCHIVELENDINGNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));                
            }


            
        }

        private void btnRet_Click(object sender, RoutedEventArgs e)
        {
            //if (lendingDelID.Count == 0)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要归还的借阅记录！");
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "RETURN"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "RETURN"));
            //}
            //else
            //{
            //    //ComfirmBox deleComfirm = new ComfirmBox();
            //    //deleComfirm.Title = Utility.GetResourceStr("RETURNCONFIRM");
            //    //deleComfirm.MessageTextBox.Text = Utility.GetResourceStr("RETURNALERT");
            //    //deleComfirm.ButtonOK.Click +=new RoutedEventHandler(ButtonOK_Click1);
            //    //deleComfirm.Show();

            //    string Result = "";
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.AddArchivesReturnAsync(Common.CurrentLoginUserInfo.EmployeeID, lendingDelID);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("RETURNCONFIRM"), Utility.GetResourceStr("RETURNALERT"), ComfirmWindow.titlename, Result);

            //    //if (HtmlPage.Window.Confirm("您确定要归还选中的档案记录吗？"))
            //    //{
            //    //    client.AddArchivesReturnAsync("admin", lendingDelID);
            //    //}
            //}
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {
                ArchivesLendingForm form = new ArchivesLendingForm(Action.AUDIT, Lendarchives.LENDARCHIVESID, checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 495;
                browser.MinHeight = 270;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }                                  


        private void ButtonOK_Click1(object sender, RoutedEventArgs e)
        {
            //client.AddArchivesReturnAsync(Common.CurrentLoginUserInfo.EmployeeID, lendingDelID);
        }

        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            
        }  
        #endregion


        #region  模板列选择 按钮切换       
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    lendObj = chkbox.DataContext as V_ArchivesLending;
            //    dgArchives.SelectedItems.Add(lendObj);
            //}
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    lendObj = chkbox.DataContext as V_ArchivesLending;
            //    dgArchives.SelectedItems.Remove(lendObj);
            //    lendObj = null;
            //}
            GridHelper.SetUnCheckAll(dgArchives);
        }

        private void dgArchives_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgArchives, e.Row, "T_OA_LENDARCHIVES");
            
        }


        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {

                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_LENDARCHIVES");
                checkState = dict.DICTIONARYVALUE.ToString();                
                LoadData();
            }                  
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(dgArchives);
            LoadData();
        }
        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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



