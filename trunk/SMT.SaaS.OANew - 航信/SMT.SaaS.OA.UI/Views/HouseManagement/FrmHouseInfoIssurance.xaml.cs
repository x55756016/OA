using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.Views.HouseManagement
{
    public partial class FrmHouseInfoIssurance : BasePage,IClient
    {
        private T_OA_HOUSEINFOISSUANCE issuanceObj;
        private SmtOACommonAdminClient client;
        private string checkState = ((int)CheckStates.ALL).ToString();
        ObservableCollection<string> issuanceDelID;// = new ObservableCollection<string>();   //参数值     
        //private List<T_OA_HOUSEINFO> houseInfoList;T_OA_HOUSEINFOISSUANCE
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_HOUSEINFOISSUANCE houseinfoissuance;

        public T_OA_HOUSEINFOISSUANCE Houseinfoissuance
        {
            get { return houseinfoissuance; }
            set { houseinfoissuance = value; }
        }
        #region 初始化
        public FrmHouseInfoIssurance()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                Utility.DisplayGridToolBarButton(ToolBar, "T_OA_HOUSEINFOISSUANCE", true);
                PARENT.Children.Add(loadbar);
                InitEvent();
                SetButtonVisible();
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            //LoadData();            
        }

        private void InitEvent()
        {
            //issuanceObj = new T_OA_HOUSEINFOISSUANCE();
            client = new SmtOACommonAdminClient();
            client.GetIssunaceListPagingCompleted += new EventHandler<GetIssunaceListPagingCompletedEventArgs>(client_GetIssunaceListPagingCompleted);
            client.DeleteIssuanceCompleted += new EventHandler<DeleteIssuanceCompletedEventArgs>(client_DeleteIssuanceCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //ToolBar.btnRead.Click += new RoutedEventHandler(btnRead_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);            
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
            //client.GetHouseIssueAndNoticeInfosCompleted += new EventHandler<GetHouseIssueAndNoticeInfosCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosCompleted);
            //client.GetHouseIssueAndNoticeInfosAsync();
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HOUSEINFOISSUANCE");
            this.Loaded += new RoutedEventHandler(FrmHouseInfoIssurance_Loaded);
            this.dgHouse.CurrentCellChanged += new EventHandler<EventArgs>(dgHouse_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Houseinfoissuance != null)
            {
                if (Houseinfoissuance.CHECKSTATE == "2" || Houseinfoissuance.CHECKSTATE == "3")
                {
                    HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.ReSubmit, Houseinfoissuance.ISSUANCEID, ((int)CheckStates.UnSubmit).ToString());
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinHeight = 550;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("HOUSEISSURANCENOTRESUBMIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Houseinfoissuance != null)
            {
                HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.Read, Houseinfoissuance.ISSUANCEID, checkState);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void dgHouse_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Houseinfoissuance = (T_OA_HOUSEINFOISSUANCE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmHouseInfoIssurance_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_HOUSEINFOISSUANCE");
        }

        void client_GetHouseIssueAndNoticeInfosCompleted(object sender, GetHouseIssueAndNoticeInfosCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    List<V_SystemNotice> aa = e.Result.ToList();
                    
                }
            }
        }

             
        #endregion

        #region 完成事件
        private void client_DeleteIssuanceCompleted(object sender, DeleteIssuanceCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "HOUSEINFOISSUANCE"));
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetIssunaceListPagingCompleted(object sender, GetIssunaceListPagingCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        BindData(e.Result.ToList(),e.pageCount);
                    }
                    else
                    {
                        BindData(null,0);                        
                    }
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 绑定网格

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值  
            if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                filter += "ISSUANCETITLE ^@" + paras.Count().ToString();
                paras.Add(txtTitle.Text.Trim());
            }
            //if (!string.IsNullOrEmpty(txtContent.Text.Trim()))
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CONTENT ^@" + paras.Count().ToString();
            //    paras.Add(txtContent.Text.Trim());
            //}
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            client.GetIssunaceListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }

        private void BindData(List<T_OA_HOUSEINFOISSUANCE> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                dgHouse.ItemsSource = null;
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                return;
            }
            dgHouse.ItemsSource = obj;
        }
        #endregion

        #region 按钮事件
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                //issuanceObj = null;
                //issuanceDelID.Remove(chkbox.Tag.ToString());
                issuanceObj = chkbox.DataContext as T_OA_HOUSEINFOISSUANCE;
                dgHouse.SelectedItems.Remove(issuanceObj);
                GridHelper.SetUnCheckAll(dgHouse);
            }
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.Value)
            {
                issuanceObj = chkbox.DataContext as T_OA_HOUSEINFOISSUANCE;
                dgHouse.SelectedItems.Add(issuanceObj);
                //issuanceObj = issuanceViewObj.issuanceObj;
                //issuanceDelID.Add(chkbox.Tag.ToString());
            }
        }       

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {

            //NoticeWebpart form = new NoticeWebpart();
            //OAWebPart form = new OAWebPart();
            //EntityBrowser browser = new EntityBrowser(form);
            //browser.MinHeight = 550;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);


            HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.Add, null, checkState);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
     
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Houseinfoissuance != null)
            {
                if (Houseinfoissuance.CHECKSTATE == "0" || Houseinfoissuance.CHECKSTATE == "3")
                {
                    HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.Edit, Houseinfoissuance.ISSUANCEID, checkState);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinHeight = 550;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("HOUSEISSURANCENOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //ObservableCollection<string> issuanceID = GridHelper.GetSelectItem(dgHouse, "myChkBox", Action.Edit);
            //if (issuanceID == null || issuanceID.Count==0)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要修改的房源信息记录");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{

            //    HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.Edit, issuanceID[0], checkState);
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 550;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{},true);
            //    //CFrmHouseInfoIssuanceAdd addFrm = new CFrmHouseInfoIssuanceAdd(Action.Edit, issuanceObj);
            //    //addFrm.Title = "修改房源信息发布";
            //    //addFrm.Show();
            //    //addFrm.ReloadDataEvent += new BaseForm.refreshGridView(addFrm_ReloadDataEvent);
            //}
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgHouse.SelectedItems.Count > 0)
            {

                string Result = "";
                issuanceDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < dgHouse.SelectedItems.Count; i++)
                    {
                        string GradeID = "";
                        GradeID = (dgHouse.SelectedItems[i] as T_OA_HOUSEINFOISSUANCE).ISSUANCEID;
                        if ((dgHouse.SelectedItems[i] as T_OA_HOUSEINFOISSUANCE).CHECKSTATE == "0" || (dgHouse.SelectedItems[i] as T_OA_HOUSEINFOISSUANCE).CHECKSTATE == "3")
                        {
                            if (!(issuanceDelID.IndexOf(GradeID) > -1))
                            {
                                issuanceDelID.Add(GradeID);
                            }
                        }
                    }
                    if (issuanceDelID.Count > 0)
                    {
                        client.DeleteIssuanceAsync(issuanceDelID);
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

            //issuanceDelID = GridHelper.GetSelectItem(dgHouse, "myChkBox", Action.Delete);
            //if (issuanceDelID == null || issuanceDelID.Count == 0)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要删除的房源信息记录");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    //ComfirmBox deleComfirm = new ComfirmBox();
            //    //deleComfirm.Title = Utility.GetResourceStr("DELETECONFIRM");
            //    //deleComfirm.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //    //deleComfirm.ButtonOK.Click += new RoutedEventHandler(ButtonOK_Click);
            //    //deleComfirm.Show();

            //    //if (HtmlPage.Window.Confirm("您确定要删除选中的房源信息记录吗？"))
            //    //{
            //    //    client.DeleteIssuanceAsync(issuanceDelID);
            //    //}

            //    string Result = "";
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.DeleteIssuanceAsync(issuanceDelID);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {

            if (Houseinfoissuance != null)
            {
                if (!string.IsNullOrEmpty(Houseinfoissuance.ISSUANCEID))
                {
                    HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.AUDIT, Houseinfoissuance.ISSUANCEID, checkState);
                    //HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(FormTypes.Audit, "56677de7-331d-4878-baf3-981884c62108");
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 550;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                
            }

            //ObservableCollection<string> issuanceID = GridHelper.GetSelectItem(dgHouse, "myChkBox", Action.Edit);
            //if (issuanceID == null || issuanceID.Count == 0)
            //{
            //    //HtmlPage.Window.Alert("请先选择需要修改的借阅记录！");

            //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    //HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(Action.AUDIT, issuanceID[0],checkState);
            //    HouseInfoIssuranceForm form = new HouseInfoIssuranceForm(FormTypes.Audit, "56677de7-331d-4878-baf3-981884c62108");
            //    EntityBrowser browser = new EntityBrowser(form);
                 
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
            //}
        }


        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }  

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            client.DeleteIssuanceAsync(issuanceDelID);
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_HOUSEINFOISSUANCE");
                checkState = dict.DICTIONARYVALUE.ToString();
                issuanceObj = null;
                LoadData();
            }  


            //if (ToolBar.cbxCheckState.SelectedItem != null)
            //{
            //    checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
            //    issuanceObj = null;
            //    GridHelper.SetUnCheckAll(dgHouse);
            //    //SetButtonVisible();
            //    //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HOUSEINFOISSUANCE");
            //    LoadData();
            //}
        }

        private void browser_ReloadDataEvent()
        {
            Houseinfoissuance = null;
            LoadData();
        }  

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void SetButtonVisible()
        {
            //switch (checkState)
            //{
            //    case "0":  //草稿
            //        ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //        ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
            //        break;
            //    case "1":  //审批通过
            //        ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Visible;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //         //提交审核
            //        break;
            //    case "2":  //审批中   审核人身份
            //        ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Visible;     //审核
            //         //提交审核
            //        break;
            //    case "3":  //审批未通过
            //        ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
            //        ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
            //        ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
            //        //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
            //        ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
            //        ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
            //        break;
            //}
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(dgHouse);
            LoadData();
        }
        #endregion

        private void dgHouse_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgHouse, e.Row, "T_OA_HOUSEINFOISSUANCE");
        }

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
