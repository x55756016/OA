using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingTemplateManagement : BasePage,IClient
    {

        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList;
        private bool IsQueryBtn = false;
        CheckBox SelectBox = new CheckBox();

        private SMTLoading loadbar = new SMTLoading();
        private T_OA_MEETINGTEMPLATE meetingtemplate;

        public T_OA_MEETINGTEMPLATE Meetingtemplate
        {
            get { return meetingtemplate; }
            set { meetingtemplate = value; }
        }
        public MeetingTemplateManagement()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MeetingTemplateManagement_Loaded);
        }

        private void InitEvent()
        {
            Utility.DisplayGridToolBarButton(ToolBar, "OAMEETINGTYPETEMPLATE", true);
            PARENT.Children.Add(loadbar);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;

            combox_SelectSource();
            MeetingClient.GetTypeTemplateInfosCompleted += new EventHandler<GetTypeTemplateInfosCompletedEventArgs>(TemplateClient_GetTypeTemplateInfosCompleted);
            MeetingClient.BatchDelMeetingTypeTemplateInfosCompleted += new EventHandler<BatchDelMeetingTypeTemplateInfosCompletedEventArgs>(MeetingClient_BatchDelMeetingTypeTemplateInfosCompleted);
            LoadMeetingTypeTemplateInfos();

            //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
            ToolBar.ShowRect();
        }

        void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0 )
            {
                Meetingtemplate = (T_OA_MEETINGTEMPLATE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingTypeTemplateInfos();
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Meetingtemplate = (T_OA_MEETINGTEMPLATE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MeetingTemplateManagement_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            GetEntityLogo("T_OA_MEETINGTEMPLATE");
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (Meetingtemplate != null)
            {
                MeetingTemplateForm form = new MeetingTemplateForm(Action.Read, Meetingtemplate.MEETINGTEMPLATEID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 850;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            ////会议类型ID  
            //string TypeId = "";

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                T_OA_MEETINGTEMPLATE ChkTemplate = new T_OA_MEETINGTEMPLATE();
            //                ChkTemplate = cb1.Tag as T_OA_MEETINGTEMPLATE;

            //                TypeId = ChkTemplate.MEETINGTEMPLATEID.ToString();
            //                break;
            //            }
            //        }
            //    }

            //}


            //if (string.IsNullOrEmpty(TypeId))
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "MEETINGTYPETEMPLATE"));
            //    return;
            //}
            //else
            //{
            //    MeetingTemplateForm form = new MeetingTemplateForm(Action.Read, TypeId, "");
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinWidth = 400;
            //    browser.MinHeight = 500;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}

        }

        void MeetingClient_BatchDelMeetingTypeTemplateInfosCompleted(object sender, BatchDelMeetingTypeTemplateInfosCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            LoadMeetingTypeTemplateInfos();
        }

        void LoadMeetingTypeTemplateInfos()
        {
            string filter = "";    //查询过滤条件
            int pageCount = 0;
            string StrMeetingType = "";
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQueryBtn)
            {

                if (!string.IsNullOrEmpty(txtTemplateName.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "TEMPLATENAME ^@" + paras.Count().ToString();
                    paras.Add(txtTemplateName.Text.Trim());
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
                if (cbMeetingType.SelectedItem != null && cbMeetingType.SelectedIndex != 0)
                {
                    StrMeetingType = ((cbMeetingType.SelectedItem) as T_OA_MEETINGTYPE).MEETINGTYPE;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "T_OA_MEETINGTYPE.MEETINGTYPE ==@" + paras.Count().ToString();
                    paras.Add(StrMeetingType);
                }
                IsQueryBtn = false;
                
            }
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();

            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loadbar.Start();
            MeetingClient.GetTypeTemplateInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);   
        }

        void TemplateClient_GetTypeTemplateInfosCompleted(object sender,GetTypeTemplateInfosCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Result != null)
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

        private void BindDataGrid(List<T_OA_MEETINGTEMPLATE> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        
        void dpGrid_PageIndexChanged(object sender, EventArgs e)
        {
            DataPager aa = sender as DataPager;
            string _text = "";
            MessageWindow.Show<string>("确认", "当前是：" + aa.PageIndex.ToString() + "页", MessageIcon.Exclamation, result => _text = result, "Default", Utility.GetResourceStr("CONFIRM"));
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

        #region 頁面控件樣式
        public void SetStyle(int Istyle)
        {
            switch (Istyle)
            {
                case -1:
                    break;
                case 0:
                    AppConfig.SetStyles("Assets/Styles.xaml", LayoutRoot);
                    break;
                case 1:
                    AppConfig.SetStyles("Assets/ShinyBlue.xaml", LayoutRoot);
                    //SearchBtn.Style = (Style)Application.Current.Resources["CommonButtonStyle1"];
                    break;
                default:
                    break;
            }
        }
        #endregion

         #region COMBOX 设置数据源

        private void combox_SelectSource()
        {
            //MeetingTypeManagementServiceClient typeClient = new MeetingTypeManagementServiceClient();
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);
            
        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbMeetingType.Items.Clear();
                T_OA_MEETINGTYPE itemType = new T_OA_MEETINGTYPE();
                itemType.MEETINGTYPE = "请选择";
                e.Result.Insert(0, itemType);
                this.cbMeetingType.ItemsSource = e.Result;
                this.cbMeetingType.DisplayMemberPath = "MEETINGTYPE";
                this.cbMeetingType.SelectedItem = itemType;
            }
           
        }

        


        #endregion

        #region  添加按钮
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            
            //AddMeetingTemplate AddWin = new AddMeetingTemplate("Add");
            //AddWin.Show();
            //AddWin.ReloadDataEvent += new AddMeetingTemplate.refreshGridView(AddWin_ReloadDataEvent);

            MeetingTemplateForm form = new MeetingTemplateForm(Action.Add, "", "");
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 850;
            browser.MinHeight = 600;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            IsQueryBtn = true;// 按条件查询
            LoadMeetingTypeTemplateInfos();
        }

        void AddWin_ReloadDataEvent()
        {
            LoadMeetingTypeTemplateInfos();
        }
        #endregion

        #region 修改按钮

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItem != null) // if (Meetingtemplate != null)        zhaowp 2010.08.19 修改
            {
                MeetingTemplateForm form = new MeetingTemplateForm(Action.Edit, Meetingtemplate.MEETINGTEMPLATEID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinWidth = 850;
                browser.MinHeight = 600;
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

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                T_OA_MEETINGTEMPLATE ChkTemplate= new T_OA_MEETINGTEMPLATE();
            //                ChkTemplate = cb1.Tag as T_OA_MEETINGTEMPLATE;
                            
            //                TypeId = ChkTemplate.MEETINGTEMPLATEID.ToString();
            //                break;
            //            }
            //        }
            //    }

            //}


            //if (string.IsNullOrEmpty(TypeId))
            //{
            //    //MessageBox.Show("请选择需要修改的会议类型模板！");
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "MEETINGTYPETEMPLATE"));
            //}
            //else
            //{

            //    //AddMeetingTemplate AddWin = new AddMeetingTemplate("Edit", TypeId);
            //    //AddWin.Show();
            //    //AddWin.ReloadDataEvent += new AddMeetingTemplate.refreshGridView(AddWin_ReloadDataEvent);


            //    MeetingTemplateForm form = new MeetingTemplateForm(Action.Edit, TypeId, "");
            //    EntityBrowser browser = new EntityBrowser(form);
            //    browser.MinWidth = 400;
            //    browser.MinHeight = 500;
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
                        string MeetingTemplateID = "";
                        MeetingTemplateID = (DaGr.SelectedItems[i] as T_OA_MEETINGTEMPLATE).MEETINGTEMPLATEID;
                        if (!(DelInfosList.IndexOf(MeetingTemplateID) > -1))
                        {
                            DelInfosList.Add(MeetingTemplateID);
                        }
                    }
                    MeetingClient.BatchDelMeetingTypeTemplateInfosAsync(DelInfosList);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //string TemplateID = "";
            //T_OA_MEETINGTEMPLATE MeetingTemplateT = new T_OA_MEETINGTEMPLATE();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingTemplateT = cb1.Tag as T_OA_MEETINGTEMPLATE;
            //                TemplateID = MeetingTemplateT.MEETINGTEMPLATEID;
            //                DelInfosList.Add(TemplateID);
                            
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
            //                //MeetingClient.BatchDelMeetingRoomdInfosAsync(DelInfosList);
            //                MeetingClient.BatchDelMeetingTypeTemplateInfosAsync(DelInfosList);

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
            //    //MessageBox.Show("请您选择需要删除的数据！");
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            //}
            


        }

       


        


        #endregion

        #region DaGr LoadingRow事件


        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGTEMPLATE");                        
        }

        #endregion

        #region 复选框按钮事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            /*------------------------------2010.03.03 Canceled---------------------------------*/

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
            LoadMeetingTypeTemplateInfos();
        }

        
        

        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingTypeTemplateInfos();
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
