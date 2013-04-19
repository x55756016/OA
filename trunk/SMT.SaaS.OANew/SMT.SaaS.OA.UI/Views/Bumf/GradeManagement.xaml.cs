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
using SMT.SaaS.FrameworkUI;
//using SMT.SaaS.OA.UI.BumfManagementWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Browser;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class GradeManagement : BasePage
    {


        #region 页面初始化
        //BumfManagementServiceClient BumfClient = new BumfManagementServiceClient();
        SmtOACommonOfficeClient BumfClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList;
        T_OA_GRADED tmpGrade = new T_OA_GRADED();
        CheckBox SelectBox = new CheckBox();
        private bool IsQueryBtn = false; //查询开关
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_GRADED docgrade;

        public T_OA_GRADED Docgrade
        {
            get { return docgrade; }
            set { docgrade = value; }
        }
        public GradeManagement()
        {
            
            InitializeComponent();
            
            Utility.DisplayGridToolBarButton(ToolBar, "OABUMFGRADE", true);
            PARENT.Children.Add(loadbar);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            BumfClient.GetGradeInfosListBySearchCompleted += new EventHandler<GetGradeInfosListBySearchCompletedEventArgs>(BumfClient_GetGradeInfosListBySearchCompleted);
            BumfClient.GradeBatchDelCompleted +=new EventHandler<GradeBatchDelCompletedEventArgs>(BumfClient_GradeBatchDelCompleted);
            LoadGradeInfos();
            //this.Loaded += new RoutedEventHandler(GradeManagement_Loaded);
            DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            DaGr.Loaded += new RoutedEventHandler(DaGr_Loaded);
        }

        void DaGr_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_GRADED");
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Docgrade = (T_OA_GRADED)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void GradeManagement_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Docgrade = (T_OA_GRADED)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (Docgrade != null)
            {
                GradeForm AddWin = new GradeForm(Action.Read, Docgrade);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinHeight = 220;
                browser.MinWidth = 300;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //T_OA_GRADED GradeInfoT = new T_OA_GRADED();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                GradeInfoT = cb1.Tag as T_OA_GRADED;
            //                break;
            //            }
            //        }
            //    }

            //}


            //if (!string.IsNullOrEmpty(GradeInfoT.GRADEDID))
            //{                                
            //    GradeForm AddWin = new GradeForm(Action.Read, GradeInfoT);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.MinHeight = 220;
            //    browser.MinWidth = 300;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    //MessageBox.Show("请选择需要修改的公文类型");
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}
        }
        
        void LoadGradeInfos()
        {
            

            string filter = "";    //查询过滤条件
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQueryBtn)
            {

                string StrTitle = "";
                string StrStart = "";
                string StrEnd = "";
                StrTitle = this.txtGrade.Text.Trim().ToString();


                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();

                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //MessageBox.Show("结束时间不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                    //MessageBox.Show("开始时间不能为空");
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));

                        //MessageBox.Show("开始时间不能大于结束时间");
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "CREATEDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "CREATEDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    filter += "GRADED ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrTitle);
                }

            }

            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            BumfClient.GetGradeInfosListBySearchAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
            
        }


        

        #endregion

        #region 导航

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #endregion        

        #region 模板中checkbox单击事件
        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            /*------------------------------2010.03.05 Canceled------------------------------*/

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

        #region DaGr_loadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_GRADED");
            //T_OA_GRADED OrderInfoT = (T_OA_GRADED)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            //mychkBox.Tag = OrderInfoT;

        }
        #endregion

        #region 删除按钮事件
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (DaGr.SelectedItems.Count > 0)
            {

                string Result = "";
                DelInfosList = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                    {
                        string GradeID = "";
                        GradeID = (DaGr.SelectedItems[i] as T_OA_GRADED).GRADEDID;
                        if (!(DelInfosList.IndexOf(GradeID) > -1))
                        {
                            DelInfosList.Add(GradeID);
                        }
                    }
                    BumfClient.GradeBatchDelAsync(DelInfosList);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //string GradeID = "";
            //T_OA_GRADED GradeT = new T_OA_GRADED();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为

            //            if (cb1.IsChecked == true)
            //            {
            //                GradeT = cb1.Tag as T_OA_GRADED;
            //                GradeID = GradeT.GRADEDID;
            //                DelInfosList.Add(GradeID);

            //            }
            //        }
            //    }

            //}


            //if (DelInfosList.Count() > 0)
            //{
            //    string Result = "";
            //    SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        if (DelInfosList != null)
            //        {
            //            try
            //            {
            //                BumfClient.GradeBatchDelAsync(DelInfosList);

            //            }
            //            catch (Exception ex)
            //            {
            //                //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
            //            }
            //        }
            //        else
            //        {
            //            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //        }
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                
            //}
            //else
            //{
            //    //MessageBox.Show("请您选择需要删除的数据！");
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            //}
                
        }

        void BumfClient_GradeBatchDelCompleted(object sender, GradeBatchDelCompletedEventArgs e)
        {
            DelInfosList.Clear();//清空记录
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "GRADENAME"));
                    LoadGradeInfos();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", "GRADENAME"));
                    return;
                }
            }
        }

        #endregion

        #region 修改按钮事件
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Docgrade != null)
            {
                GradeForm AddWin = new GradeForm(Action.Edit, Docgrade);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinHeight = 220;
                browser.MinWidth = 300;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //T_OA_GRADED GradeInfoT = new T_OA_GRADED();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                GradeInfoT = cb1.Tag as T_OA_GRADED;
            //                break;
            //            }
            //        }
            //    }

            //}


            //if (!string.IsNullOrEmpty(GradeInfoT.GRADEDID))
            //{

            //    //AddGrade AddWin = new AddGrade(Action.Edit, GradeInfoT);
            //    //AddWin.Show();
            //    //AddWin.ReloadDataEvent += new AddGrade.refreshGridView(AddWin_ReloadDataEvent);
            //    GradeForm AddWin = new GradeForm(Action.Edit, GradeInfoT);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.MinHeight = 220;
            //    browser.MinWidth = 300;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    //MessageBox.Show("请选择需要修改的公文类型");
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}

        }

        #endregion     

        #region 添加按钮事件
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            
            GradeForm AddWin = new GradeForm(Action.Add, tmpGrade);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinHeight = 220;
            browser.MinWidth = 300;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        void browser_ReloadDataEvent()
        {
            LoadGradeInfos();
        }

        void AddWin_ReloadDataEvent()
        {
            LoadGradeInfos();
        }
        #endregion

        #region 全选事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            /*---------------------------2010.03.05 Canceled--------------------------*/

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

        #region 查询按钮

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadGradeInfos();

        }

        void BumfClient_GetGradeInfosListBySearchCompleted(object sender, GetGradeInfosListBySearchCompletedEventArgs e)
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
                    loadbar.Stop();
                }
                catch (Exception ex)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                }

                
            }
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_OA_GRADED> obj, int pageCount)
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

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadGradeInfos();
        }


    }
}
