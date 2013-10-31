using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.UseractWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysUseractLog : BasePage
    {

        #region 初始化数据 --杨祥红
        UseractLogServiceClient SysUseract = new UseractLogServiceClient();
        ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        private SMTLoading loadbar = new SMTLoading();
        private bool QueryBtn = false;

        public T_SYS_USERACTLOG SelectUser { get; set; }
        public SysUseractLog()
        {
            InitializeComponent();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "SYSUSERMANAGEMENT", true);
            PARENT.Children.Add(loadbar);
            InitControlEvent();
            LoadData();

            GetEntityLogo("T_SYS_USERACTLOG"); 
        }

        //注册事件
        private void InitControlEvent()
        {
            this.Loaded+=new RoutedEventHandler(SysUseractLog_Loaded);
            DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);
            SysUseract.GetSysUseractLogWithPagingCompleted+=new EventHandler<GetSysUseractLogWithPagingCompletedEventArgs>(SysUseract_GetSysUseractLogWithPagingCompleted);
            SysUseract.DeleteUseractLogCompleted += new EventHandler<DeleteUseractLogCompletedEventArgs>(SysUseract_DeleteUseractLogCompleted);
        }
        void SysUseract_DeleteUseractLogCompleted(object sender, DeleteUseractLogCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "删除数据成功！", Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.LoadData();
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "系统出现错误，请与管理员联系", Utility.GetResourceStr("CONFIRMBUTTON"));
                    return;
                }
            }
        }
        void SysUseract_GetSysUseractLogWithPagingCompleted(object sender, GetSysUseractLogWithPagingCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
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
                        loadbar.Stop();
                    }
                    catch (Exception ex)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
          
            QueryBtn = false;
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_USERACTLOG> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DtGrid.ItemsSource = null;
                return;
            }
            DtGrid.ItemsSource = obj;
        }
       //选择发生改变事件
        void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectUser = grid.SelectedItems[0] as T_SYS_USERACTLOG;
            }
        }
        //按钮呈现和触发事件
        void SysUseractLog_Loaded(object sender, RoutedEventArgs e)
        {
          //FormToolBar1.btnNew.Click+=new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
          
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
     
            FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
            FormToolBar1.cbxCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;

            FormToolBar1.ShowRect();
        }
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysUseractLogForm editForm = new SMT.SaaS.Permission.UI.Form.SysUseractLogForm(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }  
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        } 
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (SelectUser != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    SysUseract.DeleteUseractLogAsync(SelectUser);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择需要删除的项！", Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择需要删除的项！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count <= 0)
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            Form.SysUseractLogForm editForm = new SMT.SaaS.Permission.UI.Form.SysUseractLogForm(FormTypes.Edit, SelectUser.LOGID);
           
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 360;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {   
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        void browser_ReloadDataEvent()  
        {
            LoadData();
        }
        #endregion

        #region "添加，修改，删除"

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            QueryBtn = true;
            LoadData();
        }

       
        private void LoadData()
        {
            string filter = "";    //查询过滤条件
            string State = "1";
            int pageCount = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
           // filter += filter += "STATE =@" + paras.Count().ToString();
            paras.Add(State);
            if (QueryBtn)
            {
                string StrName = ""; //姓名
                TextBox UserName = Utility.FindChildControl<TextBox>(expander, "txtSelect");
                StrName = UserName.Text.Trim().ToString();
                if (!string.IsNullOrEmpty(StrName))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CLIENTHOSTADDRESS =@" + paras.Count().ToString();
                    paras.Add(StrName);
                }

            }

            loadbar.Start();
            SMT.Saas.Tools.UseractWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.UseractWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            SysUseract.GetSysUseractLogWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
            //SysUseract.GetSysUseractLogListAsync();
            //SysUseract.CloseAsync();
            //SysUseract.Abort();
            // SysUserClient.GetSysUserAllInfosAsync();
        }
        

        void AddWin_ReloadDataEvent()
        {   
            LoadData();
        }
        #endregion

        #region LoadingRow
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            T_SYS_USERACTLOG UserT = (T_SYS_USERACTLOG)e.Row.DataContext;
            SetRowLogo(DtGrid, e.Row, "T_SYS_USERACTLOG");
        }

        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
