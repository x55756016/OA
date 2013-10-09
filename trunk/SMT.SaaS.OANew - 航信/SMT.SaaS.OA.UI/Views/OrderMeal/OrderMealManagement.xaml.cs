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
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Windows.Data;
using System.Windows.Browser;
//using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.CommForm;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.UserControls;

namespace SMT.SaaS.OA.UI.Views.OrderMeal
{
    public partial class OrderMealManagement : BasePage
    {

        #region 初始化数据
        SmtOAPersonOfficeClient OrderMealClient = new SmtOAPersonOfficeClient();
        
        private string tmpStrUserID = "";//用户ID
        T_OA_ORDERMEAL tmpOrderInfoT = new T_OA_ORDERMEAL();
        private string checkState = "2";  // 0 取消 1 已订  2 待订
        CheckBox SelectBox = new CheckBox();
        private SMTLoading loadbar = new SMTLoading(); 
        public OrderMealManagement()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            InitEvent();

        }

        private void InitEvent()
        {

            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.TblCheckState.Text = Utility.GetResourceStr("ORDERMEALSTATE");
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.btnRead.Click += new RoutedEventHandler(btnRead_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(OrderDetailBtn_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "ORDERMEALSTATE", "2");
            SetButtonVisible();
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            OrderMealClient.GetOrderMealInfosListByTitleTimeSearchCompleted += new EventHandler<GetOrderMealInfosListByTitleTimeSearchCompletedEventArgs>(OrderMealClient_GetOrderMealInfosListByTitleTimeSearchCompleted);
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }
        void LoadOrderMealInfos(string StrUserID, string StrState)
        {
            string StrTitle = "";
            //string StrType = "";
            string StrDepartment = "";
            string StrContent = "";
            string filter = "";
            string StrStart = "";
            string StrEnd = "";
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            StrTitle = txtOrderMealTitle.Text.Trim().ToString();
            StrContent = txtContent.Text.Trim().ToString();

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

                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                    return;
                }
            }
            if (!string.IsNullOrEmpty(StrState))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ORDERMEALFLAG ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrState);
            }
            if (!string.IsNullOrEmpty(StrTitle))
            {
                if (filter != "")
                    filter += " and ";
                filter += "ORDERMEALTITLE ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrTitle);
            }
            if (!string.IsNullOrEmpty(StrContent))
            {
                if (filter != "")
                    filter += " and ";
                filter += "CONTENT ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrContent);
            }

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            OrderMealClient.GetOrderMealInfosListByTitleTimeSearchAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
            
            
        }

        void LoadData()
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }
        #endregion

        #region 页面导航时代码

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            
        }

        
        #endregion


        void OrderMealClient_GetOrderMealInfosListByTitleTimeSearchCompleted(object sender, GetOrderMealInfosListByTitleTimeSearchCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    //SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
                loadbar.Stop();
            }
            catch (Exception ex)
            {

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_OA_ORDERMEAL> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }

        #region DaGr_LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_ORDERMEAL OrderInfoT = (T_OA_ORDERMEAL)e.Row.DataContext;
            
            CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;

            mychkBox.Tag = OrderInfoT;

            

        }

        #endregion


        

       

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {            
            T_OA_ORDERMEAL InfoT = new T_OA_ORDERMEAL();
            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为

                        if (cb1.IsChecked == true)
                        {
                            InfoT = cb1.Tag as T_OA_ORDERMEAL;
                            break;

                        }
                    }
                }

            }



            if (!string.IsNullOrEmpty(InfoT.ORDERMEALID))
            {
                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, strresult) =>
                {
                    if (InfoT != null)
                    {
                        try
                        {

                            AddOrderMealRemark AddWin = new AddOrderMealRemark(Action.Add,InfoT);
                            
                            
                            //MeetingRoomAppForm AddWin = new MeetingRoomAppForm(Action.Add, MeetingRoomAppT, null);
                            EntityBrowser browser = new EntityBrowser(AddWin);
                            browser.MinHeight = 300;
                            browser.MinWidth = 300;
                            AddWin.ReloadDataEvent += new AddOrderMealRemark.refreshGridView(AddWin_ReloadDataEvent);
                            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });



                        }
                        catch (Exception ex)
                        {
                            //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                        }
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("ORDEREDMEAL"), Utility.GetResourceStr("ORDERMEALCONFIRM"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            }
            else
            {
                //MessageBox.Show("请您选择需要删除的数据！");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "CANCELORDERMEAL"));
            }


        }

        void AddWin_ReloadDataEvent()
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        private void OrderDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            //Button DetailBtn = sender as Button;
            //T_OA_ORDERMEAL Orderinfo = DetailBtn.Tag as T_OA_ORDERMEAL;
            //OrderMealDetailInfo AddWin = new OrderMealDetailInfo(Orderinfo);
            //AddWin.Show();
            T_OA_ORDERMEAL MeetingInfoT = new T_OA_ORDERMEAL();

            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            //MeetingInfoId = cb1.Tag.ToString();
                            MeetingInfoT = cb1.Tag as T_OA_ORDERMEAL;
                            break;
                        }
                    }
                }

            }

            if (!string.IsNullOrEmpty(MeetingInfoT.ORDERMEALID))
            {

                OrderMealDetailInfo AddWin = new OrderMealDetailInfo(MeetingInfoT);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinWidth = 260;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });


                //OrderMealDetailInfo AddWin = new OrderMealDetailInfo(MeetingInfoT);
                ////MeetingMangementDetailInfo AddWin = new MeetingMangementDetailInfo(MeetingInfoT);
                //AddWin.Show();
            }
            else
            {
                //MessageBox.Show("请选择需要修改的会议信息");
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }




        #region 按钮事件
        

        
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //client.DeleteOrganAsync(organDelID);
        }

        //查看
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {

            T_OA_ORDERMEAL EditOrderMealInfoT = new T_OA_ORDERMEAL();

            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            //MeetingInfoId = cb1.Tag.ToString();
                            EditOrderMealInfoT = cb1.Tag as T_OA_ORDERMEAL;
                            break;
                        }
                    }
                }

            }
            if (!string.IsNullOrEmpty(EditOrderMealInfoT.ORDERMEALID))
            {
                OrderMealDetailInfo AddWin = new OrderMealDetailInfo(EditOrderMealInfoT);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinWidth = 260;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

                //OrderMealDetailInfo AddWin = new OrderMealDetailInfo(EditOrderMealInfoT);
                //AddWin.Show();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "请选择记录");
                return;
            }
            

        }

        

        //模板列取消选中
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Remove(organObj.organ.ORGANIZATIONID);
            //    organObj = null;
            //}
        }

        //模板列选中
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    organObj = chkbox.DataContext as V_OrganRegister;
            //    organDelID.Add(organObj.organ.ORGANIZATIONID);
            //}
        }

        //查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        //刷新
        private void browser_ReloadDataEvent()
        {
            //LoadData();
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolBar.cbxCheckState.SelectedItem != null)
            {
                checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                
                SetButtonVisible();
                AddWin_ReloadDataEvent();
            }
        }

        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //已取消的订餐
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;        //查看
                    
                    break;
                case "1":  //已订餐
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    //ToolBar.btnRead.Visibility = Visibility.Visible;          //查看
                    
                    break;
                case "2":  //待订餐
                    ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
                    NewButton();
                    //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看


                    break;



            }
            
                        
        }

        //private void orgFrm_ReloadDataEvent()
        //{
        //    LoadData();
        //}

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //GridHelper.SetUnCheckAll(dgOrgan);
            //LoadData();
        }

        #endregion

        //#region 选择部门

        //private void PostsObject_FindClick(object sender, EventArgs e)
        //{
        //    OrganizationLookupForm lookup = new OrganizationLookupForm();
        //    lookup.SelectedObjType = OrgTreeItemTypes.Department;


        //    lookup.SelectedClick += (obj, ev) =>
        //    {
        //        PostsObject.DataContext = lookup.SelectedObj;
        //        if (lookup.SelectedObj is T_HR_DEPARTMENT)
        //        {
        //            T_HR_DEPARTMENT tmp = lookup.SelectedObj as T_HR_DEPARTMENT;

        //            //StrDepartmentID = tmp.DEPARTMENTID;
        //            //StrDepartmentName = tmp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

        //            PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
        //        }

        //    };

        //    lookup.Show();
        //}



        //#endregion



        #region 添加报告Button
        public void NewButton()
        {

            ToolBar.stpOtherAction.Children.Clear();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("ORDEREDMEAL");
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(ConfirmBtn_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);

            
            //ImageButton ChangeMeetingBtn1 = new ImageButton();
            //ChangeMeetingBtn1.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            //ChangeMeetingBtn1.TextBlock.Text = Utility.GetResourceStr("DETAILINFO");
            //ChangeMeetingBtn1.Image.Width = 16.0;
            //ChangeMeetingBtn1.Image.Height = 22.0;
            //ChangeMeetingBtn1.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            //ChangeMeetingBtn1.Style = (Style)Application.Current.Resources["ButtonStyle"];
            //ChangeMeetingBtn1.Click += new RoutedEventHandler(OrderDetailBtn_Click);
            //ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn1);

            
        }

        
        #endregion

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


    }
}
