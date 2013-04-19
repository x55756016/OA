using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
//using SMT.SaaS.OA.UI.MeetingRoomAppWS;
//using SMT.SaaS.OA.UI.MeetingWs;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingRoomAppManagement : BasePage, IClient
    {

        #region 加载数据
        //private MeetingRoomAppManagementServiceClient RoomAppClient = new MeetingRoomAppManagementServiceClient();
        //private MeetingManagementServiceClient MeetingClient = new MeetingManagementServiceClient();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        private string StrDepartmentID = ""; //部门ID
        private string StrDepartmentName = "";// 部门名
        private string checkState = ((int)CheckStates.ALL).ToString(); //等待审核
        SMT.SaaS.FrameworkUI.AuditControl.AuditControl Audit = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl(); //未提交审核
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果

        T_OA_MEETINGROOMAPP MeetingRoomAppT = new T_OA_MEETINGROOMAPP();
        CheckBox SelectBox = new CheckBox();
        string StrCompanyId = "";
        bool IsQueryBtn = false;
        private SMTLoading loadbar = new SMTLoading();
        private V_MeetingRoomApp meetingroomapp;

        public V_MeetingRoomApp Meetingroomapp
        {
            get { return meetingroomapp; }
            set { meetingroomapp = value; }
        }
        public MeetingRoomAppManagement()
        {
            InitializeComponent();
            //将所有操作放置在Loaded完成之后,保证在页面加载时字典已经载入
            this.Loaded += new RoutedEventHandler(MeetingRoomAppManagement_Loaded);
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Meetingroomapp != null)
            {
                if (Meetingroomapp.roomapp.CHECKSTATE == "2" || Meetingroomapp.roomapp.CHECKSTATE == "3")
                {
                    MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.Resubmit, Meetingroomapp.roomapp, Meetingroomapp.room);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.MinHeight = 350;
                    browser.MinWidth = 340;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    Meetingroomapp = null;
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("MEETINGROOMAPPNOTRESUBMIT"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTONERECORD"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count > 0)
            {
                Meetingroomapp = (V_MeetingRoomApp)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Meetingroomapp = (V_MeetingRoomApp)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MeetingRoomAppManagement_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Utility.DisplayGridToolBarButton(ToolBar, "T_OA_MEETINGROOMAPP", true);
                ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
                ToolBar.btnAudit.Visibility = Visibility.Collapsed;
                ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
                ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
                //            
                ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
                Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
                MeetingClient.MeetingRoomAppUpdateCompleted += new EventHandler<MeetingRoomAppUpdateCompletedEventArgs>(MeetingClient_MeetingRoomAppUpdateCompleted);
                MeetingClient.GetMeetingRoomAppInfosByFlowCompleted += new EventHandler<GetMeetingRoomAppInfosByFlowCompletedEventArgs>(MeetingClient_GetMeetingRoomAppInfosByFlowCompleted);
                //Audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditApp_AuditCompleted);
                MeetingClient.MeetingRoomAppBatchDelCompleted += new EventHandler<MeetingRoomAppBatchDelCompletedEventArgs>(MeetingClient_MeetingRoomAppBatchDelCompleted);
                PARENT.Children.Add(loadbar);
                //LoadMeetingRoomAppInfos();
                //combox_MeetingRoomSelectSource();
                //SetButtonVisible();
                //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
                DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
                ToolBar.ShowRect();
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                //throw(ex);
            }
            GetEntityLogo("T_OA_MEETINGROOMAPP");
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (Meetingroomapp != null)
            {
                MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.Browse, Meetingroomapp.roomapp, Meetingroomapp.room);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 480;
                browser.MinWidth = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Meetingroomapp = null;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingRoomAppInfos();
        }

        void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.AuditControl.AuditControl AuditApp = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();
            V_MeetingRoomApp VMeetingRoom = new V_MeetingRoomApp();

            if (DaGr.ItemsSource != null)
            {
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (cb1.IsChecked == true)
                        {
                            VMeetingRoom = cb1.Tag as V_MeetingRoomApp;
                            break;
                        }
                    }
                }

            }

            if (VMeetingRoom.roomapp != null)
            {

                if (checkState == "1" || checkState == "4") //待审核 或正审核
                {
                    MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.Audit, VMeetingRoom.roomapp, VMeetingRoom.room);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.MinHeight = 480;
                    browser.MinWidth = 400;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }

            }
            else
            {
                //MessageBox.Show("请选择需要修改的会议室申请信息");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "MEETINGROOMAPP"));
                return;
            }
        }


        void MeetingClient_MeetingRoomAppUpdateCompleted(object sender, MeetingRoomAppUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                LoadMeetingRoomAppInfos();
            }
        }
        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_MEETINGROOMAPP");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadMeetingRoomAppInfos();
            }
        }



        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();

            SMT.SaaS.FrameworkUI.AuditControl.AuditControl AuditApp = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();

            if (Meetingroomapp != null)
            {
                if (MeetingRoomAppT.CHECKSTATE == "2")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGROOMAPPAUDITEDNOTAUDIT"));
                    return;
                }
                else
                {

                    if (Meetingroomapp.roomapp.CHECKSTATE == "1" || Meetingroomapp.roomapp.CHECKSTATE == "4") // if (checkState == "1" || checkState == "4") //待审核 或正审核
                    {
                        MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.Audit, Meetingroomapp.roomapp, Meetingroomapp.room);
                        EntityBrowser browser = new EntityBrowser(AddWin);
                        browser.FormType = FormTypes.Audit;
                        browser.MinHeight = 480;
                        browser.MinWidth = 320;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    }
                    Meetingroomapp = null;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }


        private void Cancel()
        {
            LoadMeetingRoomAppInfos();
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            LoadMeetingRoomAppInfos();
        }


        void LoadMeetingRoomAppInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrRoomName = "";
            string StrDepartment = "";

            string StrStart = "";
            string StrEnd = "";
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQueryBtn)
            {

                StrRoomName = this.txtMeetingRoomName.Text.ToString().Trim();
                //StrDepartment = this.PostsObject.Text.ToString().Trim();


                StrStart = DPStart.Text.ToString();
                StrEnd = DPEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();

                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));

                        return;
                    }
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " roomapp.STARTTIME >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " roomapp.ENDTIME <=@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                }

                if (checkState == ((int)CheckStates.UnSubmit).ToString())
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "roomapp.OWNERID==@" + paras.Count().ToString();
                    paras.Add(Common.CurrentLoginUserInfo.EmployeeID);
                }

                if (!string.IsNullOrEmpty(StrRoomName))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "room.MEETINGROOMNAME ^@" + paras.Count().ToString();
                    paras.Add(StrRoomName);
                }
                //if (!string.IsNullOrEmpty(StrDepartment))
                //{
                //    if (!string.IsNullOrEmpty(filter))
                //    {
                //        filter += " and ";
                //    }
                //    filter += "roomapp.DEPARTNAME ==@" + paras.Count().ToString();
                //    paras.Add(StrDepartment);
                //}


            }
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;


            loadbar.Start();
            MeetingClient.GetMeetingRoomAppInfosByFlowAsync(dataPager.PageIndex, dataPager.PageSize, "roomapp.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);

        }

        void MeetingClient_GetMeetingRoomAppInfosByFlowCompleted(object sender, GetMeetingRoomAppInfosByFlowCompletedEventArgs e)
        {
            IsQueryBtn = false;

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
        private void BindDataGrid(List<V_MeetingRoomApp> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }


        void RoomAppClient_GetMeetingRoomAppInfosCompleted(object sender, GetMeetingRoomAppInfosCompletedEventArgs e)
        {
            //if (e.Result != null)
            //{
            //    List<T_OA_MEETINGROOMAPP> infos = new List<T_OA_MEETINGROOMAPP>(e.Result);
            //    dpGrid.PageSize = 20;
            //    PagedCollectionView pager = new PagedCollectionView(infos);
            //    DaGr.ItemsSource = pager;
            //    //DaGr.DataContext = pager;
            //}

        }

        #endregion

        #region 页面导航事件
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 初始化会议室信息

        //private void combox_MeetingRoomSelectSource()
        //{

        //    MeetingClient.GetMeetingRoomNameInfosToComboxCompleted += new EventHandler<GetMeetingRoomNameInfosToComboxCompletedEventArgs>(MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted);
        //    MeetingClient.GetMeetingRoomNameInfosToComboxAsync();

        //}

        //void MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted(object sender, GetMeetingRoomNameInfosToComboxCompletedEventArgs e)
        //{
        //    if (e.Result != null)
        //    {                
        //        this.cbMeetingRoom.ItemsSource = e.Result;
        //        this.cbMeetingRoom.DisplayMemberPath = "MEETINGROOMNAME";
        //        this.cbMeetingRoom.SelectedIndex = 0;
        //    }

        //}

        #endregion


        #region 添加按钮事件

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.New, MeetingRoomAppT, null);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinHeight = 300;
            browser.MinWidth = 350;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //AddMeetingRoomApp AddWin = new AddMeetingRoomApp(Action.Add, MeetingRoomAppT);
            //AddWin.Show();
            //AddWin.ReloadDataEvent += new AddMeetingRoomApp.refreshGridView(AddWin_ReloadDataEvent);
        }

        void AddWin_ReloadDataEvent()
        {
            Meetingroomapp = null;
            LoadMeetingRoomAppInfos();
        }

        #endregion

        #region 修改按钮事件

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Meetingroomapp != null)
            {
                if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Meetingroomapp.roomapp, "T_OA_MEETINGROOMAPP", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
                {
                    if (Meetingroomapp.roomapp.CHECKSTATE == "0" || Meetingroomapp.roomapp.CHECKSTATE == "3")
                    {
                        MeetingRoomAppForm AddWin = new MeetingRoomAppForm(FormTypes.Edit, Meetingroomapp.roomapp, Meetingroomapp.room);
                        EntityBrowser browser = new EntityBrowser(AddWin);
                        browser.FormType = FormTypes.Edit;
                        browser.MinHeight = 380;
                        browser.MinWidth = 350;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("MEETINGROOMAPPNOTEDIT"));
                    }

                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }

                Meetingroomapp = null;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


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
                        if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission((DaGr.SelectedItems[i] as V_MeetingRoomApp).roomapp, "T_OA_MEETINGROOMAPP", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            string DocTypeTemplateID = "";
                            DocTypeTemplateID = (DaGr.SelectedItems[i] as V_MeetingRoomApp).roomapp.MEETINGROOMAPPID;
                            if ((DaGr.SelectedItems[i] as V_MeetingRoomApp).roomapp.CHECKSTATE == "0" || (DaGr.SelectedItems[i] as V_MeetingRoomApp).roomapp.CHECKSTATE == "3")
                            {
                                if (!(DelInfosList.IndexOf(DocTypeTemplateID) > -1))
                                {
                                    DelInfosList.Add(DocTypeTemplateID);
                                }
                            }

                        }
                        else
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                            string StrTip = "";
                            StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条的信息";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrTip, Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;

                            //break;
                        }


                    }
                    if (DelInfosList.Count > 0)
                    {
                        MeetingClient.MeetingRoomAppBatchDelAsync(DelInfosList);
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



        }


        void MeetingClient_MeetingRoomAppBatchDelCompleted(object sender, MeetingRoomAppBatchDelCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "MEETINGROOMAPP"));
                        LoadMeetingRoomAppInfos();
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                }
            }

        }


        #endregion

        #region DaGr_loadingRow事件

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGROOMAPP");
            ////T_OA_MEETINGROOMAPP MeetingRoomAppInfoT = (T_OA_MEETINGROOMAPP)e.Row.DataContext;
            //V_MeetingRoomApp MeetingRoomAppInfoT = (V_MeetingRoomApp)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            ////mychkBox.Tag = MeetingInfoT.MEETINGINFOID;
            //mychkBox.Tag = MeetingRoomAppInfoT;

        }
        #endregion

        #region 复选框按钮事件
        private void chkAll_Click(object sender, RoutedEventArgs e)
        {
            /*-----------------------------2010.03.03 Canceled--------------------------------*/

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

        private void AddAudit_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 查询按钮事件

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQueryBtn = true;
            this.LoadMeetingRoomAppInfos();
            //string StrRoomName = "";
            //string StrDepartment = "";
            //string StrState = "";
            //string StrStart = "";
            //string StrEnd = "";
            //if (this.cbMeetingRoom.SelectedIndex > 0)
            //{
            //    StrRoomName = cbMeetingRoom.SelectedItem.ToString();
            //}            
            //switch (cbxCheckState.SelectedIndex)
            //{ 
            //    case 1:
            //        StrState = "0";
            //        break;
            //    case 2:
            //        StrState = "1";
            //        break;
            //}

            //StrStart = DPStart.Text.ToString();
            //StrEnd = DPEnd.Text.ToString();
            //DateTime DtStart = new DateTime();
            //DateTime DtEnd = new DateTime();
            //if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            //{
            //    DtStart = System.Convert.ToDateTime(StrStart);
            //    DtEnd = System.Convert.ToDateTime(StrEnd);
            //    if (DtStart > DtEnd)
            //    {
            //        MessageBox.Show("开始时间不能大于结束时间");
            //        return;
            //    }
            //}

            //MeetingClient.GetMeetintRoomAppInfosListBySearchCompleted += new EventHandler<GetMeetintRoomAppInfosListBySearchCompletedEventArgs>(RoomAppClient_GetMeetintRoomAppInfosListBySearchCompleted);
            //MeetingClient.GetMeetintRoomAppInfosListBySearchAsync(StrRoomName, StrDepartment, DtStart, DtEnd, StrState, "1");


        }

        void RoomAppClient_GetMeetintRoomAppInfosListBySearchCompleted(object sender, GetMeetintRoomAppInfosListBySearchCompletedEventArgs e)
        {
            //if (!e.Cancelled)
            //{
            //    if (e.Result != null)
            //    {
            //        List<T_OA_MEETINGROOMAPP> infos = new List<T_OA_MEETINGROOMAPP>(e.Result);
            //        if (infos.Count() > 0)
            //        {
            //            dpGrid.PageSize = 3;
            //            PagedCollectionView pager = new PagedCollectionView(infos);
            //            DaGr.ItemsSource = pager;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("对不起，没找到您需要的数据！");
            //    }

            //}

        }

        #endregion

        #region 选择部门

        private void PostsObject_FindClick(object sender, RoutedEventArgs e)
        {
            //SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            //lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            //lookup.SelectedClick += (obj, ev) =>
            //{
            //    List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
            //    if (ent != null && ent.Count > 0)
            //    {
            //        SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
            //        StrDepartmentID = companyInfo.ObjectID;
            //        //StrCompanyId = companyInfo.ObjectID;
            //        PostsObject.Text = companyInfo.ObjectName;
            //        StrDepartmentName = companyInfo.ObjectName;
            //    }
            //};
            //lookup.MultiSelected = false;
            //lookup.Show();
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            this.LoadMeetingRoomAppInfos();
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
