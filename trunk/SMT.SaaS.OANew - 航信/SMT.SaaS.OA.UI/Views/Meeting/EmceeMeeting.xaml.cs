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
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.Saas.Tools.FileUploadWS;
using System.Windows.Data;
using System.Globalization;
using System.IO;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using System.Collections.ObjectModel;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class EmceeMeeting : BasePage
    {
        
        #region 装载数据               

        private string tmpMeetingStaffID="";//上传附件时使用
        [ScriptableMember()]
        public event EventHandler MaximumFileSizeReached;
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        V_MyMeetingInfosManagement SelectMeeting = new V_MyMeetingInfosManagement();
        
        private T_OA_MEETINGSTAFF tmpMeetingStaffT;

        private SMTLoading loadbar = new SMTLoading();
        private V_MyMeetingInfosManagement myemcmeeting;
        private bool IsQueryBtn = false;
        public V_MyMeetingInfosManagement Myemcmeeting
        {
            get { return myemcmeeting; }
            set { myemcmeeting = value; }
        }
        public EmceeMeeting()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAHOSTMEETING", true);
            PARENT.Children.Add(loadbar);    
            ToolBar.Visibility = Visibility.Visible;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(MeetingDetailBtn_Click);
            LoadMeetingInfos();
            MeetingClient.GetMyEmceeMeetingInfosManagementCompleted += new EventHandler<GetMyEmceeMeetingInfosManagementCompletedEventArgs>(MeetingClient_GetMyEmceeMeetingInfosManagementCompleted);
            this.Loaded += new RoutedEventHandler(EmceeMeeting_Loaded);
            ToolBar.ShowRect();
            //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);

        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Myemcmeeting = (V_MyMeetingInfosManagement)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void EmceeMeeting_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_MEETINGSTAFF");
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
                }
                else
                {
                    if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))//选择了开始时间
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OAMeetingInfoT.CREATEDATE >=@" + paras.Count().ToString();
                        paras.Add(System.Convert.ToDateTime(StrStart));
                    }
                    if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))//选择了结束时间
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OAMeetingInfoT.CREATEDATE <=@" + paras.Count().ToString();
                        paras.Add(System.Convert.ToDateTime(StrEnd));
                    }
                }

                loadbar.Start();
                IsStart = true;
                //MeetingClient.GetMeetingInfoListByFlowAsync(dataPager.PageIndex, dataPager.PageSize, "meetinginfo.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
                MeetingClient.GetMyEmceeMeetingInfosManagementAsync(dataPager.PageIndex, dataPager.PageSize, "OAMeetingInfoT.CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
                //MeetingClient.GetMyMeetingInfosManagementAsync(dataPager.PageIndex, dataPager.PageSize, "OAMeetingInfoT.CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
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

        void MeetingClient_GetMyEmceeMeetingInfosManagementCompleted(object sender, GetMyEmceeMeetingInfosManagementCompletedEventArgs e)
        {
            //if (e.Result != null)
            //{
            //    List<V_MyMeetingInfosManagement> infos = new List<V_MyMeetingInfosManagement>(e.Result);
            //    PagedCollectionView pager = new PagedCollectionView(infos);
            //    DaGr.ItemsSource = pager;

            //}

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
            //loadbar.Stop();
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


        #region 页面导航
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

       

        
        #region DaGr_LoadingRom

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGINFO");            
        }

        #endregion

        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        

        #region 确认参加会议
        

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

        

        

        private void MeetingDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Myemcmeeting != null)
            {
                EmceeMeetingForm AddWin = new EmceeMeetingForm(Myemcmeeting);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.MinWidth = 650;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            //if (SelectMeeting == null)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //}
            //else
            //{   
            //    EmceeMeetingForm AddWin = new EmceeMeetingForm(SelectMeeting);             
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.MinWidth = 650;
            //    browser.MinHeight = 600;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                
            //}
        }
        private void AddWin_ReloadDataEvent()
        {
            Myemcmeeting = null;
            LoadMeetingInfos();
        }

        

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DaGr.SelectedItems[0].IsNotNull

            if (DaGr.SelectedItems.Count == 0)
                return;

            SelectMeeting = DaGr.SelectedItems[0] as V_MyMeetingInfosManagement;
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Myemcmeeting = (V_MyMeetingInfosManagement)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
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


        




    }
}
