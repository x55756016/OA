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
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.HouseManagement
{
    public partial class FrmHouseHireAppManagement : BasePage,IClient
    {
        private SmtOACommonAdminClient client;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private SMTLoading loadbar = new SMTLoading();
        ObservableCollection<string> hireAppID;

        #region 初始化
        private V_HouseHireApp househireapp;

        public V_HouseHireApp Househireapp
        {
            get { return househireapp; }
            set { househireapp = value; }
        }
        public FrmHouseHireAppManagement()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                Utility.DisplayGridToolBarButton(ToolBar, "T_OA_HIREAPP", true);
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
            //client.GetHireAppListPagingCompleted += new EventHandler<GetHireAppListPagingCompletedEventArgs>(client_GetHireAppListPagingCompleted);
            client.GetHireAppListPagingByHouseInfoOrListCompleted += new EventHandler<GetHireAppListPagingByHouseInfoOrListCompletedEventArgs>(client_GetHireAppListPagingByHouseInfoOrListCompleted);
            client.DeleteHireAppCompleted += new EventHandler<DeleteHireAppCompletedEventArgs>(client_DeleteHireAppCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Click += new RoutedEventHandler(FrmHouseInfoManagement_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
            client.AddHireRecordCompleted += new EventHandler<AddHireRecordCompletedEventArgs>(client_AddHireRecordCompleted);
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HIREAPP");
            //SetButtonVisible();
            LoadData();
            this.Loaded += new RoutedEventHandler(FrmHouseHireAppManagement_Loaded);
            this.dgHireApp.CurrentCellChanged += new EventHandler<EventArgs>(dgHireApp_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Househireapp != null)
            { 
                HouseHireAppForm form = new HouseHireAppForm(Action.Read, Househireapp.houseAppObj.HIREAPPID, checkState, "0");
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
                Househireapp = (V_HouseHireApp)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmHouseHireAppManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_HOUSEINFOISSUANCE");
        }

        void client_AddHireRecordCompleted(object sender, AddHireRecordCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("MESSAGE"), Utility.GetResourceStr(""));
                    return;
                }
                else
                { 
                    //入住成功
                    
                }
            }

        }

        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Visibility = Visibility.Collapsed;
                    break;
                case "1":  //审批中
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Visibility = Visibility.Collapsed;
                    break;
                case "2":  //审批通过   审核人身份  
                    //NewButton();
                    //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Visibility = Visibility.Visible;
                    break;
                case "3":  //审批未通过
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Visibility = Visibility.Collapsed;
                    break;
                case "4":  //待审核
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            

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
                    filter += "houseAppObj.CREATEDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "houseAppObj.CREATEDATE <=@" + paras.Count().ToString();
                    paras.Add(DtEnd); 
                }
            }
            loadbar.Start();
            //client.GetHireAppListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
            client.GetHireAppListPagingByHouseInfoOrListAsync(dataPager.PageIndex, dataPager.PageSize, "houseAppObj.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }

        private void BindData(List<V_HouseHireApp> obj, int pageCount)
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
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_HIREAPP");
                checkState = dict.DICTIONARYVALUE.ToString();
                SetButtonVisible();
                LoadData();
            }            
        }

        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {

            if (Househireapp != null)
            {
                if (!string.IsNullOrEmpty(Househireapp.houseAppObj.HIREAPPID))
                {
                    HouseHireAppForm form = new HouseHireAppForm(Action.AUDIT, Househireapp.houseAppObj.HIREAPPID, checkState, "0");
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 500;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));

            }


            //string hireAppID = "";
            //V_HouseHireApp HireAppT = new V_HouseHireApp();

            //if (dgHireApp.ItemsSource != null)
            //{
            //    foreach (object obj in dgHireApp.ItemsSource)
            //    {
            //        if (dgHireApp.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                HireAppT = cb1.Tag as V_HouseHireApp;
            //                hireAppID = HireAppT.houseAppObj.HIREAPPID;
            //                break;
            //            }
            //        }
            //    }

            //}
            //if (string.IsNullOrEmpty(hireAppID))
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{

            //    HouseHireAppForm form = new HouseHireAppForm(Action.AUDIT, hireAppID, checkState, "0");
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 500;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            //}
        }

        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dgHireApp.SelectedItems.Count > 0)
            {

                string Result = "";
                hireAppID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < dgHireApp.SelectedItems.Count; i++)
                    {
                        string hireID = "";
                        hireID = (dgHireApp.SelectedItems[i] as V_HouseHireApp).houseAppObj.HIREAPPID;
                        if ((dgHireApp.SelectedItems[i] as V_HouseHireApp).houseAppObj.CHECKSTATE == "0" || (dgHireApp.SelectedItems[i] as V_HouseHireApp).houseAppObj.CHECKSTATE == "3")
                        {
                            if (!(hireAppID.IndexOf(hireID) > -1))
                            {
                                hireAppID.Add(hireID);
                            }
                        }
                    }
                    if (hireAppID.Count > 0)
                    {
                        client.DeleteHireAppAsync(hireAppID);
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


            //ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            //if (hireAppID == null || hireAppID.Count == 0)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{               
            //    string Result = "";
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.DeleteHireAppAsync(hireAppID);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Househireapp != null)
            {
                if (Househireapp.houseAppObj.CHECKSTATE == "0" || Househireapp.houseAppObj.CHECKSTATE == "3")
                {
                    HouseHireAppForm form = new HouseHireAppForm(Action.Edit, Househireapp.houseAppObj.HIREAPPID, checkState, "0");
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinHeight = 500;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);                       
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("HOUSEAPPNOTEDIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            //string hireAppID = "";
            //V_HouseHireApp HireAppT = new V_HouseHireApp();

            //if (dgHireApp.ItemsSource != null)
            //{
            //    foreach (object obj in dgHireApp.ItemsSource)
            //    {
            //        if (dgHireApp.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                HireAppT = cb1.Tag as V_HouseHireApp;
            //                hireAppID = HireAppT.houseAppObj.HIREAPPID;
            //                break;
            //            }
            //        }
            //    }

            //}
            //if (string.IsNullOrEmpty(hireAppID))
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{

            //    HouseHireAppForm form = new HouseHireAppForm(Action.Edit, hireAppID, checkState, "0");
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 500;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
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

        ///// <summary>
        ///// 退房
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void FrmHouseInfoManagement_Click(object sender, RoutedEventArgs e)
        //{
           

        //    ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
        //    if (hireAppID != null && hireAppID.Count > 0)
        //    {
        //        string Result = "";
        //        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
        //        com.OnSelectionBoxClosed += (obj, result1) =>
        //        {
        //            HouseHireAppForm form = new HouseHireAppForm(Action.Return, hireAppID[0], checkState);
        //            EntityBrowser browser = new EntityBrowser(form);

        //            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    
        //        };
        //        com.SelectionBox(Utility.GetResourceStr("OUTHOUSECONFIRM"), Utility.GetResourceStr("OUTHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

                
        //    }
        //    else
        //    {
                
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CHECKOUTHIRE"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //}

        ///// <summary>
        ///// 入住
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void FrmHouseComein_Click(object sender, RoutedEventArgs e)
        //{
        //    T_OA_HIREAPP HireAppT = new T_OA_HIREAPP();
            
        //    if (dgHireApp.ItemsSource != null)
        //    {
        //        foreach (object obj in dgHireApp.ItemsSource)
        //        {
        //            if (dgHireApp.Columns[0].GetCellContent(obj) != null)
        //            {
        //                CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
        //                if (cb1.IsChecked == true)
        //                {
        //                    HireAppT = cb1.Tag as T_OA_HIREAPP;
        //                    break;
        //                }
        //            }
        //        }

        //    }
            
        //    if (!string.IsNullOrEmpty(HireAppT.HIREAPPID))
        //    {

        //        string Result = "";
        //        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
        //        com.OnSelectionBoxClosed += (obj, result1) =>
        //        {
        //            //添加租赁记录并将，出租申请的标志改为确定入住
        //            T_OA_HIRERECORD hirerecord = new T_OA_HIRERECORD();
        //            hirerecord.HIRERECORD = System.Guid.NewGuid().ToString();                    
        //            hirerecord.RENTER = HireAppT.RENTTYPE;
                    
        //            hirerecord.MANAGECOST = HireAppT.MANAGECOST;
        //            hirerecord.RENTCOST = HireAppT.RENTCOST;
        //            hirerecord.WATER = 0;
        //            hirerecord.ELECTRICITY = 0;
        //            hirerecord.OTHERCOST = 0;
        //            hirerecord.WATERNUM = 0;
        //            hirerecord.ELECTRICITYNUM = 0;
        //            hirerecord.SETTLEMENTDATE = System.DateTime.Now;
        //            hirerecord.SETTLEMENTTYPE = HireAppT.SETTLEMENTTYPE;//付款方式
        //            hirerecord.ISSETTLEMENT = "0"; //是否结算

        //            hirerecord.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
        //            hirerecord.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        //            hirerecord.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //            hirerecord.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //            hirerecord.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //            hirerecord.CREATEDATE = DateTime.Now;

        //            hirerecord.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
        //            hirerecord.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        //            hirerecord.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //            hirerecord.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //            hirerecord.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //            client.AddHireRecordAsync(hirerecord);


        //        };
        //        com.SelectionBox(Utility.GetResourceStr("OUTHOUSECONFIRM"), Utility.GetResourceStr("OUTHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);


        //        //HouseHireAppForm form = new HouseHireAppForm(Action.Return, hireAppID[0], checkState);
        //        //EntityBrowser browser = new EntityBrowser(form);

        //        //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //        //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        //    }
        //    else
        //    {

        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "COMEINHIREHOUSE"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //}

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

        void client_GetHireAppListPagingByHouseInfoOrListCompleted(object sender, GetHireAppListPagingByHouseInfoOrListCompletedEventArgs e)
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"),ex.Message.ToString());
            }
        }

        private void client_GetHireAppListPagingCompleted(object sender, GetHireAppListPagingCompletedEventArgs e)
        {
            
        }


        private void client_DeleteHireAppCompleted(object sender, DeleteHireAppCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
                }
                else
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "HOUSEHIREAPP"));
                    LoadData();
                }
            }
            
        }
        #endregion

        private void dgHireApp_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgHireApp, e.Row, "T_OA_HIREAPP");
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
