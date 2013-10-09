using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.OA.UI.Views.HouseManagement
{
    public partial class HireRecordManagement : BasePage,IClient
    {
        

        private SmtOACommonAdminClient client;
        private string checkState = "2";//((int)CheckStates.WaittingApproval).ToString();
        private SMTLoading loadbar = new SMTLoading();
        private V_HireRecord hirerecord;

        public V_HireRecord Hirerecord
        {
            get { return hirerecord; }
            set { hirerecord = value; }
        }
        #region 初始化
        public HireRecordManagement()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                Utility.DisplayGridToolBarButton(ToolBar, "T_OA_HIRERECORD", true);
                PARENT.Children.Add(loadbar);
                InitEvent();
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();            
            client.GetHireRecordListPagingCompleted += new EventHandler<GetHireRecordListPagingCompletedEventArgs>(client_GetHireRecordListPagingCompleted);
            client.DeleteHireAppCompleted += new EventHandler<DeleteHireAppCompletedEventArgs>(client_DeleteHireAppCompleted);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                        
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Click += new RoutedEventHandler(FrmHouseInfoManagement_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "HIRERENTTYPE", checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            client.AddHireRecordCompleted += new EventHandler<AddHireRecordCompletedEventArgs>(client_AddHireRecordCompleted);
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HIREAPP");
            //SetButtonVisible();
            LoadData();
            this.Loaded += new RoutedEventHandler(HireRecordManagement_Loaded);
            dgHireApp.CurrentCellChanged += new EventHandler<EventArgs>(dgHireApp_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Hirerecord != null)
            {                
                HireRecordForm form = new HireRecordForm(Action.Read, Hirerecord);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void dgHireApp_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Hirerecord = (V_HireRecord)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void HireRecordManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_HIRERECORD");
        }

        void client_AddHireRecordCompleted(object sender, AddHireRecordCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr(""), Utility.GetResourceStr(""));
                    return;
                }
                else
                { 
                    //入住成功
                    
                }
            }

        }

        

        

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //checkState = 
            //if (!string.IsNullOrEmpty(StrTitle))
            //{
            //    IsNull = true;
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "OACompanySendDoc.SENDDOCTITLE ^@" + paras.Count().ToString();//标题名称
            //    paras.Add(StrTitle);
            //}
            if (checkState != "2")
            {
                filter += "HouseRecordObj.ISSETTLEMENT ^@" + paras.Count().ToString();//标题名称
                paras.Add(checkState);
            }

            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();           
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                //Utility.GetLoginUserInfo(loginUserInfo);
                loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
                loginUserInfo.departmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            }
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                filter += "houseInfoObj.UPTOWN^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "houseInfoObj.HOUSENAME^@" + paras.Count().ToString();
                paras.Add(txtHouseName.Text.Trim());
            }
            string StrStart = "";
            string StrEnd = "";
            StrStart = dpStart.Text.ToString();
            StrEnd = dpEnd.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                if (DtStart > DtEnd)
                {
                    //MessageBox.Show("开始时间不能大于结束时间");
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                    return;
                }
                else
                {
                    
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "houseAppObj.STARTDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "houseAppObj.STARTDATE <=@" + paras.Count().ToString();
                    paras.Add(DtEnd);


                    
                }
            }
            loadbar.Start();

            client.GetHireRecordListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "HouseRecordObj.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }

        private void BindData(List<V_HireRecord> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {                
                dgHireApp.ItemsSource = null;
                return;
            }
            dgHireApp.ItemsSource = obj;
        }
        #endregion

        #region 按钮事件
        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolBar.cbxCheckState.SelectedItem != null)
            {
                checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                GridHelper.SetUnCheckAll(dgHireApp);
                //SetButtonVisible();
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HIREAPP");
                
                LoadData();
            }
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            if (hireAppID == null || hireAppID.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {

                HouseHireAppForm form = new HouseHireAppForm(Action.AUDIT, hireAppID[0], checkState,"0");
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
        }

        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            if (hireAppID == null || hireAppID.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {               
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.DeleteHireAppAsync(hireAppID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Hirerecord != null)
            {
                if (Hirerecord.HouseRecordObj.ISSETTLEMENT == "0")
                {
                    HireRecordForm form = new HireRecordForm(Action.Edit, Hirerecord);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.MinHeight = 500;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("HIRERECORDISSETTLEMENT"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            //V_HireRecord RecordT = new V_HireRecord();

            //if (this.dgHireApp.ItemsSource != null)
            //{
            //    foreach (object obj in dgHireApp.ItemsSource)
            //    {
            //        if (dgHireApp.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                RecordT = cb1.Tag as V_HireRecord;
            //                break;
            //            }
            //        }
            //    }

            //}

            //if (RecordT.HouseRecordObj == null )
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    if (RecordT.HouseRecordObj.ISSETTLEMENT == "0")
            //    {
            //        HireRecordForm form = new HireRecordForm(Action.Edit, RecordT);
            //        EntityBrowser browser = new EntityBrowser(form);
            //        browser.MinHeight = 500;
            //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            //    }
            //    else
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("HIRERECORDISSETTLEMENT"));
            //        return;
            //    }
            //}
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            HouseHireAppForm form = new HouseHireAppForm(Action.Add, null,checkState,"0");
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void browser_ReloadDataEvent()
        {
            LoadData();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 完成事件

        void client_GetHireRecordListPagingCompleted(object sender, GetHireRecordListPagingCompletedEventArgs e)
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
                        BindData(e.Result.ToList(), e.pageCount);
                    }
                    else
                    {
                        BindData(null, 0);
                    }
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        


        private void client_DeleteHireAppCompleted(object sender, DeleteHireAppCompletedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void dgHireApp_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgHireApp, e.Row, "T_OA_HIRERECORD");
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
