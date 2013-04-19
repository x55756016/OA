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
//using SMT.SaaS.OA.UI.OrderMealWS;
using System.Windows.Data;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Media.Imaging;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;

namespace SMT.SaaS.OA.UI.Views.OrderMeal
{
    public partial class MyOrderMealManagement : BasePage
    {

        //OrderMealServiceClient OrderMealClient = new OrderMealServiceClient();
        private SmtOAPersonOfficeClient OrderMealClient = new SmtOAPersonOfficeClient();

        private string StrState = "";
        private string tmpStrUserID = Common.CurrentLoginUserInfo.EmployeeID;//用户ID
        T_OA_ORDERMEAL tmpOrderInfoT = new T_OA_ORDERMEAL();
        private string checkState = "2";
        CheckBox SelectBox = new CheckBox();
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        public MyOrderMealManagement()
        {
            InitializeComponent();
            
            InitEvent();
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }
        private void InitEvent()
        {

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(OrderDetailBtn_Click);
            ToolBar.TblCheckState.Text = Utility.GetResourceStr("ORDERMEALSTATE");
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //
            //CheckStateCBX.SetCheckStateCBX(ToolBar.cbxCheckState);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "ORDERMEALSTATE", "2");
            SetButtonVisible();
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            OrderMealClient.GetOrderMealInfosListByTitleTimeSearchCompleted += new EventHandler<GetOrderMealInfosListByTitleTimeSearchCompletedEventArgs>(OrderMealClient_GetOrderMealInfosListByTitleTimeSearchCompleted);
            OrderMealClient.OrderMealInfoDelCompleted += new EventHandler<OrderMealInfoDelCompletedEventArgs>(OrderMealClient_OrderMealInfoDelCompleted);
            OrderMealClient.OrderMealInfoUpdateCompleted += new EventHandler<OrderMealInfoUpdateCompletedEventArgs>(OrderMealClient_OrderMealInfoUpdateCompleted);

        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }    

        // 当用户导航到此页面时执行。
        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            //LoadOrderMealInfos(tmpStrUserID, checkState);
        }
        #endregion


        #region 取消订餐Button
        public void NewCancelOrdermealButton()
        {
            ToolBar.stpOtherAction.Children.Clear();
            ImageButton _imgbutton = new ImageButton();
            _imgbutton.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            _imgbutton.TextBlock.Text = Utility.GetResourceStr("CANCELORDERMEAL");//取消订餐
            _imgbutton.Image.Width = 16.0;
            _imgbutton.Image.Height = 22.0;
            _imgbutton.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            _imgbutton.Style = (Style)Application.Current.Resources["ButtonStyle"];
            _imgbutton.Click += new RoutedEventHandler(_imgbutton_Click);
            ToolBar.stpOtherAction.Children.Add(_imgbutton);

            
        }
        #endregion

        #region 取消订餐事件
        void _imgbutton_Click(object sender, RoutedEventArgs e)
        {

            T_OA_ORDERMEAL EditOrderMealInfoT = new T_OA_ORDERMEAL();

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
                            EditOrderMealInfoT = cb1.Tag as T_OA_ORDERMEAL;
                            break;
                        }
                    }
                }

            }



            if (!string.IsNullOrEmpty(EditOrderMealInfoT.ORDERMEALID))
            {
                if (EditOrderMealInfoT.ORDERMEALFLAG == "1")
                {
                    //MessageBox.Show("已经订餐，不能取消");
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ORDEREDMEALNOCANCEL"));
                }
                else
                {   
                    tmpOrderInfoT = EditOrderMealInfoT;
                    tmpOrderInfoT.ORDERMEALFLAG = "0";
                    tmpOrderInfoT.UPDATEDATE = System.DateTime.Now;
                    tmpOrderInfoT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpOrderInfoT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    OrderMealClient.OrderMealInfoUpdateAsync(tmpOrderInfoT); 

                    
                }
            }
            else
            {
                //MessageBox.Show("请选择需要修改的订餐信息");
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "CANCEL"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


             
        }
        #endregion
                

        void LoadOrderMealInfos(string StrUserID, string StrState)
        { 
            //OrderMealClient.GetOrderMealInfosCompleted +=new EventHandler<GetOrderMealInfosCompletedEventArgs>(OrderMealClient_GetOrderMealInfosCompleted);
            //OrderMealClient.GetOrderMealInfosAsync(tmpStrUserID,StrState);
            string StrTitle = "";


            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrContent = "";

            string StrStart = "";
            string StrEnd = "";


            StrTitle = txtOrderMealTitle.Text.Trim().ToString();

            StrContent = txtContent.Text.Trim().ToString();



            StrStart = dpStart.Text.ToString();
            StrEnd = dpEnd.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
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
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ORDERMEALTITLE ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrTitle);
            }
            if (!string.IsNullOrEmpty(StrContent))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTENT ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrContent);
            }

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            OrderMealClient.GetOrderMealInfosListByTitleTimeSearchAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }

        //#region 按钮控制
        //private void SetButtonVisible()
        //{
        //    switch (checkState)
        //    {
        //        case "0":  //草稿
        //            ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
        //            ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
        //            ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
        //            //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
        //            ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
        //            //ToolBar.btnSumbitAudit.Visibility = Visibility.Visible; //提交审核
        //            break;
        //        case "1":  //审批中
        //            ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
        //            ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
        //            ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
        //            //ToolBar.btnRead.Visibility = Visibility.Visible;          //查看
        //            ToolBar.btnAudit.Visibility = Visibility.Collapsed;       //审核
        //             //提交审核
        //            break;
        //        case "2":  //审批通过
        //            ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
        //            ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
        //            ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
        //            //ToolBar.btnRead.Visibility = Visibility.Collapsed;        //查看
        //            ToolBar.btnAudit.Visibility = Visibility.Collapsed;         //审核
        //             //提交审核
        //            break;
        //        case "3":  //审批未通过
        //            ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
        //            ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
        //            ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
        //            //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
        //            ToolBar.btnAudit.Visibility = Visibility.Collapsed;     //审核
        //             //提交审核
        //            break;
        //        case "4":  //待审核
        //            ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
        //            ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
        //            ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
        //            //ToolBar.btnRead.Visibility = Visibility.Collapsed;      //查看
        //            
        //            ToolBar.btnAudit.Visibility = Visibility.Visible;     //审核
        //            break;
        //    }
        //}
        //#endregion
        


        

        #region 复选框按钮事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {

            //if (DaGr.ItemsSource != null)
            //{
            //    if (this.chkAll.IsChecked.Value)//全选
            //    {

            //        foreach (object obj in DaGr.ItemsSource)
            //        {
            //            if (DaGr.Columns[0].GetCellContent(obj) != null)
            //            {
            //                CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox; //cb为
            //                cb1.IsChecked = true;
            //            }
            //        }
            //        chkAll.Content = "全不选";
            //    }
            //    else//取消
            //    {
            //        foreach (object obj in DaGr.ItemsSource)
            //        {
            //            if (DaGr.Columns[0].GetCellContent(obj) != null)
            //            {
            //                CheckBox cb2 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBtn") as CheckBox;
            //                cb2.IsChecked = false;
            //            }
            //        }
            //        chkAll.Content = "全选";
            //    }
            //}

        }
        #endregion
        
        #region 模板中CheckBox单击事件
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

        #region 添加按钮事件

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            //AddOrderMeal AddWin = new AddOrderMeal("Add");
            //AddWin.Show();
            //AddWin.ReloadDataEvent += new AddOrderMeal.refreshGridView(AddWin_ReloadDataEvent);
        }
        #endregion

        void AddWin_ReloadDataEvent()
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        

        #region 删除按钮

        

        void OrderMealClient_OrderMealInfoDelCompleted(object sender, OrderMealInfoDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //MessageBox.Show("删除数据成功！");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
                    LoadOrderMealInfos(tmpStrUserID, checkState);
                }
                else
                {
                    //MessageBox.Show("删除数据失败！");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
                    LoadOrderMealInfos(tmpStrUserID, checkState);
                }
            }

            
        }

        #endregion

        #region DaGr_LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_OA_ORDERMEAL OrderInfoT = (T_OA_ORDERMEAL)e.Row.DataContext;
            //Button DetailBtn = DaGr.Columns[6].GetCellContent(e.Row).FindName("OrderDetailBtn") as Button; //查看订餐信息
            //Button CancelBtn = DaGr.Columns[6].GetCellContent(e.Row).FindName("CancelOrderBtn") as Button; //取消订餐
            //switch (checkState)
            //{ 
            //    case "0"://取消
            //        CancelBtn.Visibility = Visibility.Collapsed;
            //        break;
            //    case "1"://已订
            //        CancelBtn.Visibility = Visibility.Collapsed;
            //        break;
            //    case "2"://待订
            //        break;
            //}

            CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;

            mychkBox.Tag = OrderInfoT;
            //DetailBtn.Tag = OrderInfoT;
            //CancelBtn.Tag = OrderInfoT;

        }

        #endregion

        #region 查询按钮

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        void OrderMealClient_GetOrderMealInfosListByTitleTimeSearchCompleted(object sender, GetOrderMealInfosListByTitleTimeSearchCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                
                try
                {
                    if (e.Result != null)
                    {
                        SelectBox = Utility.FindChildControl<CheckBox>(DaGr, "SelectAll");
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

        #endregion

        #region 详情按钮

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
                
                
            }
            else
            {
                //MessageBox.Show("请选择需要修改的会议信息");
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


        }
        #endregion

        #region 取消订餐按钮

        private void CancelOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            string MeetingInfoID = "";
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
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    if (InfoT != null)
                    {
                        try
                        {

                            tmpOrderInfoT = InfoT;
                            tmpOrderInfoT.ORDERMEALFLAG = "0";
                            tmpOrderInfoT.UPDATEDATE = System.DateTime.Now;
                            tmpOrderInfoT.UPDATEUSERID = "1";
                            OrderMealClient.OrderMealInfoUpdateCompleted += new EventHandler<OrderMealInfoUpdateCompletedEventArgs>(OrderMealClient_OrderMealInfoUpdateCompleted);
                            OrderMealClient.OrderMealInfoUpdateAsync(tmpOrderInfoT);  

                            

                        }
                        catch (Exception ex)
                        {
                            //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                        }
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("CANCELORDERMEAL"), Utility.GetResourceStr("CANCELORDERMEAL"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            }
            else
            {
                //MessageBox.Show("请您选择需要删除的数据！");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "CANCELORDERMEAL"));
            }


            
            

        }

        void OrderMealClient_OrderMealInfoUpdateCompleted(object sender, OrderMealInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                //MessageBox.Show("订餐信息已被取消");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCEESSED"), Utility.GetResourceStr("CANCELSUCCESSED", "ORDEREDMEAL"));
                LoadOrderMealInfos(tmpStrUserID, checkState);
                
            }
            else
            {
                //MessageBox.Show("取消订餐信息失败");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("CANCELFAILED", "ORDEREDMEAL"));
                return;
            }
        }

        #endregion

        #region 按钮事件
        //新增
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //AddOrderMeal AddWin = new AddOrderMeal("Add");
            //AddWin.Show();
            //AddWin.ReloadDataEvent += new AddOrderMeal.refreshGridView(AddWin_ReloadDataEvent);
            T_OA_ORDERMEAL EditOrderMealInfoT = new T_OA_ORDERMEAL();
            OrderMealForm AddWin = new OrderMealForm(Action.Add, EditOrderMealInfoT);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinHeight = 280;
            browser.MinWidth = 320;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            T_OA_ORDERMEAL EditOrderMealInfoT = new T_OA_ORDERMEAL();

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
                            EditOrderMealInfoT = cb1.Tag as T_OA_ORDERMEAL;
                            break;
                        }
                    }
                }

            }



            if (!string.IsNullOrEmpty(EditOrderMealInfoT.ORDERMEALID))
            {
                if (EditOrderMealInfoT.ORDERMEALFLAG == "1")
                {
                    //MessageBox.Show("信息已经确定，不能修改");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ORDERMEALISOKNOTEDIT"));
                }
                else
                {
                    OrderMealForm AddWin = new OrderMealForm(Action.Edit, EditOrderMealInfoT);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.MinHeight = 280;
                    browser.MinWidth = 320;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

                    //AddOrderMeal AddWin = new AddOrderMeal("Edit", EditOrderMealInfoT);
                    //AddWin.Show();
                    //AddWin.ReloadDataEvent += new AddOrderMeal.refreshGridView(AddWin_ReloadDataEvent);
                }
            }
            else
            {
                //MessageBox.Show("请选择需要修改的订餐信息");
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            
        }

        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string OrderMealID = "";
            T_OA_ORDERMEAL OrderMealInfoT = new T_OA_ORDERMEAL();
            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为

                        if (cb1.IsChecked == true)
                        {
                            OrderMealInfoT = cb1.Tag as T_OA_ORDERMEAL;
                            //OrderMealID += OrderMealInfoT.ORDERMEALID + ",";
                            OrderMealID = OrderMealInfoT.ORDERMEALID;
                            DelInfosList.Add(OrderMealID);
                            //MeetingInfoID += cb1.Tag.ToString() + ",";
                        }
                    }
                }

                
            }


            if (DelInfosList.Count() > 0)
            {

                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    if (DelInfosList != null)
                    {
                        try
                        {
                            OrderMealClient.OrderMealInfoDelAsync(DelInfosList);

                        }
                        catch (Exception ex)
                        {
                            //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                        }
                    }
                };
               com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);             
                
            }
            else
            {
                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            }
            
        }

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
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
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
                //OrderMealDetailInfo AddWin = new OrderMealDetailInfo(EditOrderMealInfoT);
                //AddWin.Show();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "请选择记录");
                return;
            }
        }

        //提交审核
        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //审核
        private void btnApp_Click(object sender, RoutedEventArgs e)
        {

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

        

        //刷新
        private void browser_ReloadDataEvent()
        {
            LoadOrderMealInfos(tmpStrUserID, checkState);
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolBar.cbxCheckState.SelectedItem != null)
            {
                checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                //GridHelper.SetUnCheckAll(dgOrgan);
                SetButtonVisible();
                
                LoadOrderMealInfos(tmpStrUserID, checkState);

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
                    ToolBar.btnNew.Visibility = Visibility.Visible;         //新增
                    ToolBar.btnEdit.Visibility = Visibility.Visible;        //修改
                    ToolBar.btnDelete.Visibility = Visibility.Visible;      //删除
                    NewCancelOrdermealButton();
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


        #region 添加报告Button
        public void NewButton()
        {

            ToolBar.stpOtherAction.Children.Clear();
            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("CANCELORDERMEAL");
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(CancelOrderBtn_Click);
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);
            
            
        }

        
        #endregion

    }
}
