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
using System.Windows.Media.Imaging;


namespace SMT.SaaS.OA.UI.Views.HouseManagement
{
    public partial class MyHouseHireManagement : BasePage,IClient
    {
        private SmtOACommonAdminClient client;
        private string checkState = "5";//审核通过的租房申请
        private SMTLoading loadbar = new SMTLoading();
        private V_HouseHireApp househireapp;

        public V_HouseHireApp Househireapp
        {
            get { return househireapp; }
            set { househireapp = value; }
        }

        #region 初始化
        public MyHouseHireManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "MYHIREAPP", true);
            PARENT.Children.Add(loadbar);
            InitEvent();
            NewButton();
            this.Loaded += new RoutedEventHandler(MyHouseHireManagement_Loaded);
            this.dgHireApp.CurrentCellChanged += new EventHandler<EventArgs>(dgHireApp_CurrentCellChanged);
        }

        void dgHireApp_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                househireapp = (V_HouseHireApp)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MyHouseHireManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_HIRERECORD");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();            
            //client.GetHireAppListPagingByHouseInfoOrListCompleted += new EventHandler<GetHireAppListPagingByHouseInfoOrListCompletedEventArgs>(client_GetHireAppListPagingByHouseInfoOrListCompleted);                        
            client.GetHireAppListPagingByMemberCompleted += new EventHandler<GetHireAppListPagingByMemberCompletedEventArgs>(client_GetHireAppListPagingByMemberCompleted);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_129_p.png", Utility.GetResourceStr("CHECKOUTHIRE")).Click += new RoutedEventHandler(FrmHouseInfoManagement_Click);
                        
            client.AddHireRecordCompleted += new EventHandler<AddHireRecordCompletedEventArgs>(client_AddHireRecordCompleted);
            SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(int.Parse(checkState), ToolBar, "T_OA_HIREAPP");
            
            LoadData();
            ToolBar.ShowRect();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Househireapp != null)
            {
                HouseHireAppForm form = new HouseHireAppForm(Action.Return, Househireapp.houseAppObj.HIREAPPID, checkState, "2");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
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


            //filter += " OWNERID ==@" + paras.Count().ToString();
            //paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
            
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();           
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " houseInfoObj.UPTOWN ^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " houseInfoObj.HOUSENAME ^@" + paras.Count().ToString();
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
                    filter += " houseAppObj.CREATEDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " houseAppObj.CREATEDATE <=@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                    
                    
                }
            }
            loadbar.Start();        
            client.GetHireAppListPagingByMemberAsync(dataPager.PageIndex, dataPager.PageSize, "houseAppObj.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
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
        
        /// <summary>
        /// 退房
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmHouseInfoManagement_Click(object sender, RoutedEventArgs e)
        {

            if (Househireapp != null)
            {
                if (Househireapp.houseAppObj.ISOK == "0")
                {
                    //没有入住不能进行退房操作
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOCOMEINCANNOTCHECKOUT"));
                    return;
                }
                else
                {
                    if (Househireapp.houseAppObj.ISBACK == "1")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("THEHOUSEISBACK"));
                        return;
                    }
                    else
                    {

                        string Result = "";
                        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result1) =>
                        {
                            HouseHireAppForm form = new HouseHireAppForm(Action.Return, Househireapp.houseAppObj.HIREAPPID, checkState, "2");
                            EntityBrowser browser = new EntityBrowser(form);

                            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

                        };
                        com.SelectionBox(Utility.GetResourceStr("OUTHOUSECONFIRM"), Utility.GetResourceStr("OUTHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

                    }

                }
                

            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CHECKOUTHIRE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }

            //if (dgHireApp.SelectedItems.Count > 0)
            //{

            //    string Result = "";
            //    houseDelID = new ObservableCollection<string>();
                
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}


            ////ObservableCollection<string> hireAppID = GridHelper.GetSelectItem(dgHireApp, "myChkBox", Action.Edit);
            //T_OA_HIREAPP HireAppT = new T_OA_HIREAPP();
            //V_HouseHireApp vApp = new V_HouseHireApp();
            //if (dgHireApp.ItemsSource != null)
            //{
            //    foreach (object obj in dgHireApp.ItemsSource)
            //    {
            //        if (dgHireApp.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                vApp = cb1.Tag as V_HouseHireApp;
            //                HireAppT = vApp.houseAppObj;
            //                break;
            //            }
            //        }
            //    }

            //}
            //if (!string.IsNullOrEmpty(HireAppT.HIREAPPID))
            //{
            //    if (HireAppT.ISOK == "0")
            //    {
            //        //没有入住不能进行退房操作
            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOCOMEINCANNOTCHECKOUT"));
            //        return;
            //    }
            //    else
            //    {
            //        if (HireAppT.ISBACK == "1")
            //        {
            //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("THEHOUSEISBACK"));
            //            return;
            //        }
            //        else
            //        {

            //            string Result = "";
            //            SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
            //            com.OnSelectionBoxClosed += (obj, result1) =>
            //            {
            //                HouseHireAppForm form = new HouseHireAppForm(Action.Return, HireAppT.HIREAPPID, checkState, "2");
            //                EntityBrowser browser = new EntityBrowser(form);

            //                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //            };
            //            com.SelectionBox(Utility.GetResourceStr("OUTHOUSECONFIRM"), Utility.GetResourceStr("OUTHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            //        }
            //    }
            //}
            //else
            //{
                
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CHECKOUTHIRE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
        }

        /// <summary>
        /// 入住
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmHouseComein_Click(object sender, RoutedEventArgs e)
        {

            if (Househireapp != null)
            {
                
                if (Househireapp.houseAppObj.ISOK == "1")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("THEHOUSEISCOMEIN"));
                    return;
                }
                else
                {
                    string Result = "";
                    SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result1) =>
                    {
                        //添加租赁记录并将，出租申请的标志改为确定入住
                        HouseHireAppForm form = new HouseHireAppForm(Action.Edit, Househireapp.houseAppObj.HIREAPPID, checkState, "1");
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 500;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);


                    };
                    com.SelectionBox(Utility.GetResourceStr("COMEINHOUSECONFIRM"), Utility.GetResourceStr("COMEINHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                }

                


            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "COMEINHIREHOUSE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //T_OA_HIREAPP HireAppT = new T_OA_HIREAPP();
            //V_HouseHireApp vApp = new V_HouseHireApp();
            //if (dgHireApp.ItemsSource != null)
            //{
            //    foreach (object obj in dgHireApp.ItemsSource)
            //    {
            //        if (dgHireApp.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = dgHireApp.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                vApp = cb1.Tag as V_HouseHireApp;
            //                HireAppT = vApp.houseAppObj;
            //                break;
            //            }
            //        }
            //    }

            //}
            
            //if (!string.IsNullOrEmpty(HireAppT.HIREAPPID))
            //{
            //    if (HireAppT.ISOK == "1")
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("THEHOUSEISCOMEIN"));
            //        return;
            //    }
            //    else
            //    {
            //        string Result = "";
            //        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
            //        com.OnSelectionBoxClosed += (obj, result1) =>
            //        {
            //            //添加租赁记录并将，出租申请的标志改为确定入住
            //            HouseHireAppForm form = new HouseHireAppForm(Action.Edit, HireAppT.HIREAPPID, checkState, "1");
            //            EntityBrowser browser = new EntityBrowser(form);
            //            browser.MinHeight = 500;
            //            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);


            //        };
            //        com.SelectionBox(Utility.GetResourceStr("COMEINHOUSECONFIRM"), Utility.GetResourceStr("COMEINHOUSEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
            //    }

                                
            //}
            //else
            //{

            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "COMEINHIREHOUSE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
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

        void client_GetHireAppListPagingByMemberCompleted(object sender, GetHireAppListPagingByMemberCompletedEventArgs e)
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
        

        #endregion

        #region Button
        public void NewButton()
        {

            ToolBar.stpOtherAction.Children.Clear();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4209.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("CHECKOUTHIRE");// 退房
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 16;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(FrmHouseInfoManagement_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);

            ImageButton MeetingCancelBtn = new ImageButton();
            MeetingCancelBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4211_d.png", UriKind.Relative));
            MeetingCancelBtn.TextBlock.Text = Utility.GetResourceStr("COMEINHIREHOUSE");// 入住
            MeetingCancelBtn.Image.Width = 16.0;
            MeetingCancelBtn.Image.Height = 16;
            MeetingCancelBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            MeetingCancelBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            MeetingCancelBtn.Click += new RoutedEventHandler(FrmHouseComein_Click);


            ToolBar.stpOtherAction.Children.Add(MeetingCancelBtn);



        }
        #endregion

        #region LoadingRow事件

        private void dgHireApp_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgHireApp, e.Row, "T_OA_HIREAPP");

            //V_HouseHireApp hireappT = (V_HouseHireApp)e.Row.DataContext;
            //CheckBox mychkBox = dgHireApp.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            //mychkBox.Tag = hireappT;
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "MYHIREAPP", true);
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
