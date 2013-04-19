using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.Saas.Tools.FileUploadWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.FileUpload;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;


namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MyMeetingInfosManagement : BasePage,IClient
    {

        #region 装载数据

        private string _fileFilter = null;
        private int _maxUpload = 2;
        private string _customParams = null;
        
        private bool _HttpUploader = true;
        private string _uploadHandlerName = null;
        private int _maxFileSize = int.MaxValue;
        private FileCollection _files;
        private UserFile[] _filesarr;
        public T_SYS_FILEUPLOAD[] _FileUpload;
        private bool IsQueryBtn = false;
        private string tmpMeetingStaffID="";//上传附件时使用
        [ScriptableMember()]
        public event EventHandler MaximumFileSizeReached;

        //private MeetingInfoManagementServiceClient MeetingInfoClient = new MeetingInfoManagementServiceClient();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        V_MyMeetingInfosManagement SelectMeeting = new V_MyMeetingInfosManagement();
        //private ObservableCollection<SMT.SaaS.OA.UI.MeetingStaffWS.T_OA_MEETINGSTAFF> MeetingStaffList = new ObservableCollection<SMT.SaaS.OA.UI.MeetingStaffWS.T_OA_MEETINGSTAFF>();
        private T_OA_MEETINGSTAFF tmpMeetingStaffT;

        private SMTLoading loadbar = new SMTLoading();

        private V_MyMeetingInfosManagement mymeetinginfo;

        public V_MyMeetingInfosManagement Mymeetinginfo
        {
            get { return mymeetinginfo; }
            set { mymeetinginfo = value; }
        }

        public MyMeetingInfosManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAMYMEETING", true);
            PARENT.Children.Add(loadbar);    
            ToolBar.Visibility = Visibility.Visible;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            _files = new FileCollection(_customParams, _maxUpload);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(MeetingDetailBtn_Click);
            LoadMeetingInfos();
            this.Loaded += new RoutedEventHandler(MyMeetingInfosManagement_Loaded);
            DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
            MeetingClient.GetMyMeetingInfosManagementCompleted += new EventHandler<GetMyMeetingInfosManagementCompletedEventArgs>(MeetingInfoClient_GetMyMeetingInfosManagementCompleted);
            ToolBar.ShowRect();
        }


        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Mymeetinginfo = (V_MyMeetingInfosManagement)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MyMeetingInfosManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_MEETINGINFO");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingInfos();
        }

        void LoadMeetingInfos()
        {
            bool IsStart = false;//记录圈圈是否驱动
            try
            {
                string StrTitle = "";
                string StrContent = "";
                string StrStart = "";
                string StrEnd = "";
                int pageCount = 0;
                string filter = "";    //查询过滤条件
                ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值           

                if (IsQueryBtn)
                {
                    StrTitle = txtMeetingTitle.Text.Trim().ToString();
                    

                    if (!string.IsNullOrEmpty(StrTitle))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OAMeetingInfoT.MEETINGTITLE ^@" + paras.Count().ToString();
                        paras.Add(StrTitle);
                    }
                    
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
                        else
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "OAMeetingInfoT.CREATEDATE >=@" + paras.Count().ToString();
                            paras.Add(DtStart);
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "OAMeetingInfoT.CREATEDATE <=@" + paras.Count().ToString();
                            paras.Add(DtEnd);
                        }
                    }
                    else
                    {
                        //开始时间不为空  结束时间为空   
                        if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "OAMeetingInfoT.CREATEDATE >=@" + paras.Count().ToString();
                            paras.Add(DtStart);
                        }
                        //结束时间不为空
                        if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                        {
                            if (!string.IsNullOrEmpty(filter))
                            {
                                filter += " and ";
                            }
                            filter += "OAMeetingInfoT.CREATEDATE <=@" + paras.Count().ToString();
                            paras.Add(DtEnd);
                        }
                    }
                }

                loadbar.Start();
                IsStart = true;

                MeetingClient.GetMyMeetingInfosManagementAsync(dataPager.PageIndex, dataPager.PageSize, "OAMeetingInfoT.CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                if (IsStart)
                {
                    loadbar.Stop();
                }
                return;
            }
        }

        void MeetingInfoClient_GetMyMeetingInfosManagementCompleted(object sender, GetMyMeetingInfosManagementCompletedEventArgs e)
        {
            loadbar.Stop();      
            IsQueryBtn = false;
            if (e.Result != null)
            {                
                BindDataGrid(e.Result.ToList(), e.pageCount);
            }
            else
            {
                BindDataGrid(null, 0);
            }      
        }

        private void BindDataGrid(List<V_MyMeetingInfosManagement> obj, int pageCount)
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

        #region 页面加载
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion
        
        #region DaGr_LoadingRom

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            
            V_MyMeetingInfosManagement MeetingV = (V_MyMeetingInfosManagement)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGINFO");
            
            //mychkBox.Tag = MeetingV;
            Button ConfirmMeetingBtn = DaGr.Columns[8].GetCellContent(e.Row).FindName("JoinMeetingBtn") as Button;            
            Button FinishBtn = DaGr.Columns[8].GetCellContent(e.Row).FindName("FinishContentBtn") as Button;

            ConfirmMeetingBtn.Tag = MeetingV;            
            FinishBtn.Tag = MeetingV;
            string Strflag = MeetingV.OAMeetingStaffT.CONFIRMFLAG;
            if (Strflag == "1" || Strflag =="2")
            {
                ConfirmMeetingBtn.Visibility = Visibility.Collapsed;
                FinishBtn.Visibility = Visibility.Visible;                
            }
            else
            {
                if (Strflag == "3")
                {
                    FinishBtn.Visibility = Visibility.Collapsed;
                    ConfirmMeetingBtn.Visibility = Visibility.Collapsed;
                }
                else//Strflag == "0"
                {
                    FinishBtn.Visibility = Visibility.Collapsed;
                    ConfirmMeetingBtn.Visibility = Visibility.Visible;
                }
            }  
        }

        #endregion

        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        
        #region 确认参加会议
        private void JoinMeetingBtn_Click(object sender, RoutedEventArgs e)
        {
            Button ConfirmBtn = sender as Button;

            V_MyMeetingInfosManagement MeetingV = ConfirmBtn.Tag as V_MyMeetingInfosManagement;            
            T_OA_MEETINGSTAFF StaffT = new T_OA_MEETINGSTAFF();
            StaffT = MeetingV.OAMeetingStaffT;
            DateTime DtNow = System.DateTime.Now;
            DateTime DtStart = Convert.ToDateTime(MeetingV.OAMeetingInfoT.STARTTIME);
            if (DtStart >= Convert.ToDateTime(MeetingV.OAMeetingInfoT.ENDTIME))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOWGREATEENDNOTJOIN"));
                return;
            }
            if (DtNow >= DtStart)
            {
                //确认时间超过或等于会议开始时间
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOWGREATESTARTNOJOIN"));
                return;
            }
            else
            {
                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    try
                    {
                        FrmConfirmJoinMeeting form = new FrmConfirmJoinMeeting(MeetingV);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 570;
                        browser.MinWidth = 580;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result1) => { }, true);
                    }
                    catch (Exception ex)
                    {
                        //com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("JOINMEETINGCONFIRM"), Utility.GetResourceStr("JOINMEETINGCONFIRM"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
            }

        }

        private void browser_ReloadDataEvent()
        {
            LoadMeetingInfos();
        }

        void MeetingStaffClient_MeetingStaffUpdateInfosCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "JOINMEETING"));
                LoadMeetingInfos();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
        }
        #endregion

        #region 完善会议内容

        private void FinishContentBtn_Click(object sender, RoutedEventArgs e)
        {
            Button ConfirmBtn = sender as Button;
            V_MyMeetingInfosManagement MeetingV = ConfirmBtn.Tag as V_MyMeetingInfosManagement;
            
            T_OA_MEETINGCONTENT contentT = new T_OA_MEETINGCONTENT();
            contentT = MeetingV.OAMeetingContentT;            
            AddMeetingContentForm AddWin = new AddMeetingContentForm(contentT);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 380;
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            
        }

        void AddWin_ReloadDataEvent()
        {
            Mymeetinginfo = null;
            LoadMeetingInfos();
        }
        #endregion

        private void MeetingDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Mymeetinginfo != null)
            {
                MyMeetingDetailInfoForm AddWin = new MyMeetingDetailInfoForm(Mymeetinginfo);                
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 580;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //if (SelectMeeting.OAMeetingInfoT == null)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{

            //    //V_MyMeetingInfosManagement MeetingV = SelectMeeting;
            //    MyMeetingDetailInfoForm AddWin = new MyMeetingDetailInfoForm(SelectMeeting);
            //    //MyMeetingDetailInfo AddWin = new MyMeetingDetailInfo(SelectMeeting);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.Width = 500;
            //    browser.Height = 600;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //}                    
        }

        

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            
            if (grid.SelectedItems.Count > 0 )
            {
                Mymeetinginfo = (V_MyMeetingInfosManagement)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
            
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingInfos();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            LoadMeetingInfos();
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
