using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingTypeManagement : BasePage,IClient
    {
        //private MeetingManagementServiceClient MeetingClient = new MeetingManagementServiceClient();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList ;
        CheckBox SelectBox = new CheckBox();
        bool IsQueryBtn = false;
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_MEETINGTYPE meetingtype;

        public T_OA_MEETINGTYPE Meetingtype
        {
            get { return meetingtype; }
            set { meetingtype = value; }
        }
        public MeetingTypeManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAMEETINGTYPE", true);
            PARENT.Children.Add(loadbar);
            LoadMeetingTypeInfos();

            /*-------------------------2010.03.02 Added----------------------------*/
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            /*---------------------------------------------------------------------*/
            //ToolBar.btnimport.Visibility = Visibility.Collapsed;
            ToolBar.btnOutExcel.Visibility = Visibility.Collapsed;
            ToolBar.btnOutPDF.Visibility = Visibility.Collapsed;
            //ToolBar.btnRead.Visibility = Visibility.Collapsed;

            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            MeetingClient.GetMeetingTypeInfoByIDCompleted += new EventHandler<GetMeetingTypeInfoByIDCompletedEventArgs>(MeetingType_GetMeetingTypeInfoByIDCompleted);
            MeetingClient.GetMeetingTypeInfosCompleted += new EventHandler<GetMeetingTypeInfosCompletedEventArgs>(MeetingType_GetMeetingTypeInfosCompleted);
            MeetingClient.MeetingTypeBatchDelCompleted += new EventHandler<MeetingTypeBatchDelCompletedEventArgs>(MeetingType_MeetingTypeBatchDelCompleted);
            MeetingClient.MeetingTypeDelCompleted += new EventHandler<MeetingTypeDelCompletedEventArgs>(MeetingType_MeetingTypeDelCompleted);
            this.Loaded += new RoutedEventHandler(MeetingTypeManagement_Loaded);
            //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
            ToolBar.ShowRect();
        }

        void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0)
            {
                Meetingtype = (T_OA_MEETINGTYPE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Meetingtype = (T_OA_MEETINGTYPE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MeetingTypeManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_MEETINGTYPE");
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (DaGr.SelectedItem != null)  //if (Meetingtype != null)   zhaowp 2010.08.19 修改
            {
                MeetingTypeForm form = new MeetingTypeForm(Action.Read, Meetingtype.MEETINGTYPEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 600;
                browser.MinWidth = 680;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingTypeInfos();
        }

        void LoadMeetingTypeInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQueryBtn)
            {
                if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "MEETINGTYPE ^@" + paras.Count().ToString();
                    paras.Add(txtSearchType.Text.Trim());
                }
            }
            //if (!string.IsNullOrEmpty(txtSearchMemo.Text.Trim()))
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "REMARK ^@" + paras.Count().ToString();
            //    paras.Add(txtSearchMemo.Text.Trim());
            //}
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();

            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            loadbar.Start();
            MeetingClient.GetMeetingTypeInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
      
        }

        void MeetingType_GetMeetingTypeInfoByIDCompleted(object sender, GetMeetingTypeInfoByIDCompletedEventArgs e)
        { 

        }

        void MeetingType_GetMeetingTypeInfosCompleted(object sender, GetMeetingTypeInfosCompletedEventArgs e)
        {
            loadbar.Stop();
            IsQueryBtn = false;
            if (e.Error == null)
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

                }
                catch (Exception ex)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                }

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }

        }

        //  绑定DataGird
        private void BindDataGrid(List<T_OA_MEETINGTYPE> obj, int pageCount)
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

        void dpGrid_PageIndexChanged(object sender, EventArgs e)
        {
            //DataPager aa = sender as DataPager;
            //MessageBox.Show("当前是："+aa.PageIndex.ToString() +"页");
        }

        private void KeyDown_Key(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Shift)
            {
                this.DaGr.BeginEdit();
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region  添加按钮
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MeetingTypeForm form = new MeetingTypeForm(Action.Add,"");
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 500;
            browser.MinWidth = 680;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});

            //AddMeetingType AddWin = new AddMeetingType("Add");
            //AddWin.Show();
            //AddWin.ReloadDataEvent += new AddMeetingType.refreshGridView(AddWin_ReloadDataEvent);
        }

        void browser_ReloadDataEvent()
        {
            LoadMeetingTypeInfos();
        }

        void AddWin_ReloadDataEvent()
        {
            LoadMeetingTypeInfos();
        }
        #endregion

        #region 修改按钮

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItem != null)  //if (Meetingtype != null)   zhaowp 2010.08.19 修改
            {
                MeetingTypeForm form = new MeetingTypeForm(Action.Edit, Meetingtype.MEETINGTYPEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 500;
                browser.MinWidth = 680;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ////会议类型ID  
            //string TypeId = "";
            //T_OA_MEETINGTYPE MeetingTypeT = new T_OA_MEETINGTYPE();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingTypeT = cb1.Tag as T_OA_MEETINGTYPE;
            //                //TypeId = cb1.Tag.ToString();
            //                TypeId = MeetingTypeT.MEETINGTYPEID;
            //                break;
            //            }
            //        }
            //    }

            //}


            //if (string.IsNullOrEmpty(TypeId))
            //{                
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{
            //    MeetingTypeForm form = new MeetingTypeForm(Action.Edit, TypeId);
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinHeight = 500;
            //    browser.MinWidth = 500;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
                
            //}

        }

        #endregion

        #region 删除按钮控件事件
        //这里使用一个确认框，需要引入System.Windows.Browser;.
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
                        string MeetingTypeID = "";
                        MeetingTypeID = (DaGr.SelectedItems[i] as T_OA_MEETINGTYPE).MEETINGTYPEID;
                        if (!(DelInfosList.IndexOf(MeetingTypeID) > -1))
                        {
                            DelInfosList.Add(MeetingTypeID);
                        }
                    }
                    loadbar.Start();
                    MeetingClient.MeetingTypeBatchDelAsync(DelInfosList);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }



            //string TypeId = "";            
            
            //T_OA_MEETINGTYPE MeetingTypeT = new T_OA_MEETINGTYPE();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingTypeT = cb1.Tag as T_OA_MEETINGTYPE;
            //                TypeId = MeetingTypeT.MEETINGTYPEID;
            //                DelInfosList.Add(TypeId);
            //                //RoomId += cb1.Tag.ToString() + ",";
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
            //                loadbar.Start();
            //                MeetingClient.MeetingTypeBatchDelAsync(DelInfosList);

            //            }
            //            catch (Exception ex)
            //            {
            //                //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
            //            }
            //        }
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);

            //}
            //else
            //{                
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            //}


            



        }

        private void GetMeetingTypeByID(string TypeId)
        {
            MeetingClient.MeetingTypeDelAsync(TypeId);

        }

        void MeetingType_MeetingTypeDelCompleted(object sender, MeetingTypeDelCompletedEventArgs e)
        {
            MainPage current = Application.Current.RootVisual as MainPage;
            LoadMeetingTypeInfos();
         
        }

        

        void MeetingType_MeetingTypeBatchDelCompleted(object sender, MeetingTypeBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "MEETINGTYPE"));
                    LoadMeetingTypeInfos();
                }
                else
                {
                    if (e.Result.ToString() != "error")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    }
                    
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEFAILED"));
            }
            loadbar.Stop();
            
        }


        #endregion

        #region DaGr LoadingRow事件


        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGTYPE");
            //T_OA_MEETINGTYPE MeetingType = (T_OA_MEETINGTYPE)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            
            ////Tag 可以是对象
            ////mychkBox.Tag = MeetingType.MEETINGTYPEID;
            //mychkBox.Tag = MeetingType;
            
        }

        #endregion

        #region 复选框按钮事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            /*--------------------2010.03.02 Canceled-----------------------*/

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

        #region 查询按钮事件

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadMeetingTypeInfos();
        }

        

        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingTypeInfos();
        }


        #region IClient 成员

        public void ClosedWCFClient()
        {
            MeetingClient.DoClose();
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
