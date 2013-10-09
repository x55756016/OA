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
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.Assets.Resources;
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Text;
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.OA.UI.UserControls.Meeting;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingRoomManagement : BasePage,IClient
    {


        # region 页面装载数据

        private SmtOACommonOfficeClient MeetingRoom = new SmtOACommonOfficeClient();
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        CheckBox SelectBox = new CheckBox();
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_MEETINGROOM meetingroom;

        public T_OA_MEETINGROOM Meetingroom
        {
            get { return meetingroom; }
            set { meetingroom = value; }
        }
        public MeetingRoomManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAMEETINGROOM", true);
            PARENT.Children.Add(loadbar);    
            LoadMeetingRoomInfos();
            InitEvent();
            
        }
        private void InitEvent()
        {
            
            
            MeetingRoom.GetMeetingRoomInfosCompleted += new EventHandler<GetMeetingRoomInfosCompletedEventArgs>(MeetingRoom_GetMeetingRoomCompleted);
            MeetingRoom.BatchDelMeetingRoomdInfosCompleted += new EventHandler<BatchDelMeetingRoomdInfosCompletedEventArgs>(MeetingRoom_BatchDelMeetingRoomdInfosCompleted);
            
            LoadMeetingRoomInfos();
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.retPDF.Visibility = Visibility.Collapsed;
            
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "0");
            
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            ToolBar.stpOtherAction.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            

            this.Loaded += new RoutedEventHandler(MeetingRoomManagement_Loaded);
            DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Meetingroom = (T_OA_MEETINGROOM)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void MeetingRoomManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_MEETINGROOM");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingRoomInfos();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {

            if (Meetingroom != null)
            {
                MeetingRoomForm orgFrm = new MeetingRoomForm(Action.Read, Meetingroom);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 280;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }



            //string RoomId = "";
            //T_OA_MEETINGROOM ChkRoom = new T_OA_MEETINGROOM();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                ChkRoom = cb1.Tag as T_OA_MEETINGROOM;                            
            //                break;
            //            }
            //        }
            //    }
            //}


            //if (string.IsNullOrEmpty(ChkRoom.MEETINGROOMNAME))
            //{
                
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("VIEW"), Utility.GetResourceStr("PLEASESELECT", "MEETINGROOM"));
            //    return;
            //}
            //else
            //{
            //    MeetingRoomForm orgFrm = new MeetingRoomForm(Action.Read, ChkRoom);
            //    EntityBrowser browser = new EntityBrowser(orgFrm);
            //    browser.MinWidth = 280;
            //    browser.MinHeight = 400;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //}
        }

        
        
        void LoadMeetingRoomInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            
            if (!string.IsNullOrEmpty(txtMeetingRoom.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "MEETINGROOMNAME ^@" + paras.Count().ToString();
                paras.Add(txtMeetingRoom.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtMemo.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "REMARK ^@" + paras.Count().ToString();
                paras.Add(txtMemo.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            
            if (string.IsNullOrEmpty(loginUserInfo.companyID))
            {
                Utility.GetLoginUserInfo(loginUserInfo);
            }
            loadbar.Start();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            //MeetingRoom.GetMeetingRoomInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
            MeetingRoom.GetMeetingRoomInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
            //MeetingRoom.GetMeetingRoomInfosAsync();            
            
        }
    
        void MeetingRoom_GetMeetingRoomCompleted(object sender, GetMeetingRoomInfosCompletedEventArgs e)
        {
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
                    loadbar.Stop();
                }
                catch (Exception ex)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
                }
            }
        }

        private void BindDataGrid(List<T_OA_MEETINGROOM> obj, int pageCount)
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

        
        #region DataGrid LoadingRow事件

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {

            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGROOM");
            //T_OA_MEETINGROOM Room = (T_OA_MEETINGROOM)e.Row.DataContext;
            //CheckBox mychkBox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
            ////Tag 可以是对象
            //mychkBox.Tag = Room;
        }
        #endregion


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
            //                CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
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
            //                CheckBox cb2 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox;
            //                cb2.IsChecked = false;
            //            }
            //        }
            //        chkAll.Content = "全选";
            //    }
            //}

        }
        #endregion

        #region 模板中CheckBox单击事件
        private void myChkBox_Click(object sender, RoutedEventArgs e)
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

        #region  查询按钮事件

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadMeetingRoomInfos();
        }

        
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //GridHelper.SetUnCheckAll(DaGr);
            LoadMeetingRoomInfos();
        }

        #region 按钮事件
        //新增
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MeetingRoomForm orgFrm = new MeetingRoomForm(Action.Add, null);
            //OrganAddForm orgFrm = new OrganAddForm(Action.Add, "");
            EntityBrowser browser = new EntityBrowser(orgFrm);
            browser.MinWidth = 280;
            browser.MinHeight = 360;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Meetingroom != null)
            {
                MeetingRoomForm orgFrm = new MeetingRoomForm(Action.Edit, Meetingroom);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.MinWidth = 280;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            //string RoomId = "";
            //T_OA_MEETINGROOM ChkRoom = new T_OA_MEETINGROOM();
            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                //RoomId = cb1.Tag.ToString();
            //                //T_OA_MEETINGROOM ChkRoom = new T_OA_MEETINGROOM();
            //                ChkRoom = cb1.Tag as T_OA_MEETINGROOM;
            //                //RoomId = ChkRoom.MEETINGROOMID;
            //                break;
            //            }
            //        }
            //    }
            //}


            //if (string.IsNullOrEmpty(ChkRoom.MEETINGROOMNAME))
            //{
            //    //MessageBox.Show("请选择需要修改的会议室！");
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("EDIT"), Utility.GetResourceStr("PLEASESELECT", "MEETINGROOM"));
            //    return;
            //}
            //else
            //{
            //    MeetingRoomForm orgFrm = new MeetingRoomForm(Action.Edit, ChkRoom);
            //    EntityBrowser browser = new EntityBrowser(orgFrm);
            //    browser.MinWidth = 280;
            //    browser.MinHeight = 360;
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});

            //}
        }

        //删除
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
                        string DocTypeTemplateID = "";
                        DocTypeTemplateID = (DaGr.SelectedItems[i] as T_OA_MEETINGROOM).MEETINGROOMID;
                        if (!(DelInfosList.IndexOf(DocTypeTemplateID) > -1))
                        {
                            DelInfosList.Add(DocTypeTemplateID);
                        }
                    }
                    loadbar.Start();
                    MeetingRoom.BatchDelMeetingRoomdInfosAsync(DelInfosList);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            //string RoomId = "";
            //T_OA_MEETINGROOM MeetingRoomT = new T_OA_MEETINGROOM();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (object obj in DaGr.ItemsSource)
            //    {
            //        if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (cb1.IsChecked == true)
            //            {
            //                MeetingRoomT = cb1.Tag as T_OA_MEETINGROOM;
            //                RoomId = MeetingRoomT.MEETINGROOMID;
            //                DelInfosList.Add(RoomId);
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
            //                MeetingRoom.BatchDelMeetingRoomdInfosAsync(DelInfosList);
            //                DelInfosList.Clear();
                            
            //            }
            //            catch(Exception ex)
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

        void MeetingRoom_BatchDelMeetingRoomdInfosCompleted(object sender, BatchDelMeetingRoomdInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                
                if (e.Result.ToString() == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SUCCESSED", "DELETE"));
                    LoadMeetingRoomInfos();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                }
                loadbar.Stop();
                
                
            }
        }
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            
            MeetingRoom.BatchDelMeetingRoomdInfosAsync(DelInfosList);
        }

        //查看
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            //ObservableCollection<string> organID = GridHelper.GetSelectItem(dgOrgan, "myChkBox", Action.Edit);
            //if (organID != null && organID.Count > 0)
            //{
            //    OrganAddForm orgFrm = new OrganAddForm(Action.Read, organID[0]);
            //    EntityBrowser browser = new EntityBrowser(orgFrm);
            //     
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
            //}
            //else
            //{
            //    HtmlPage.Window.Alert("请先选择需要查看的机构！");
            //}
        }

        //提交审核
        private void btnSumbitAudit_Click(object sender, RoutedEventArgs e)
        {
            //V_OrganRegister organObj = null;
            //if (dgOrgan.ItemsSource != null)
            //{
            //    ObservableCollection<string> selectedObj = new ObservableCollection<string>();
            //    foreach (object obj in dgOrgan.ItemsSource)
            //    {
            //        if (dgOrgan.Columns[0].GetCellContent(obj) != null)
            //        {
            //            CheckBox ckbSelect = dgOrgan.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //            if (ckbSelect.IsChecked == true)
            //            {
            //                organObj = ckbSelect.DataContext as V_OrganRegister;
            //                break;
            //            }

            //        }
            //    }

            //}
            //if (organObj != null)
            //{
            //    HtmlPage.Window.Alert("请先选择需要提交审核的机构！");
            //}
            //else
            //{
            //    T_OA_ORGANIZATION organ = new T_OA_ORGANIZATION();
            //    organ = organObj.organ;
            //    organ.CHECKSTATE = "2";
            //    organ.UPDATEUSERID = "User";
            //    organ.UPDATEDATE = DateTime.Now;
            //    Flow_FlowRecord_T entity = new Flow_FlowRecord_T();
            //    entity.CompanyID = "smt";
            //    entity.Content = "dfwfw";
            //    entity.CreateUserID = "admin"; //创建流程用户ID
            //    entity.CreateDate = DateTime.Now;
            //    entity.Flag = "0";
            //    //entity.EditDate = DateTime.Now;
            //    entity.EditUserID = "admin";
            //    entity.FlowCode = "gefege";
            //    entity.FormID = organ.ORGANIZATIONID;//保存的模块表ID
            //    entity.GUID = Guid.NewGuid().ToString();
            //    entity.InstanceID = "";
            //    entity.ModelCode = "OrganApp";  //模块代码
            //    entity.OfficeID = "Manage"; //岗位ID
            //    entity.ParentStateCode = "StartFlow"; //父状态代码
            //    entity.StateCode = "StartFlow";  //状态代码
            //    client.SubmitFlowAsync(organ, entity, "Admin");
            //}
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

        //查询
        //private void SearchBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    LoadMeetingRoomInfos();
        //}

        //刷新
        private void browser_ReloadDataEvent()
        {
            LoadMeetingRoomInfos();
        }

        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ToolBar.cbxCheckState.SelectedItem != null)
            //{
            //    checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
            //    GridHelper.SetUnCheckAll(dgOrgan);
            //    SetButtonVisible();
            //    LoadData();
            //}
        }

        private void SetButtonVisible()
        {
            
            ToolBar.btnNew.Visibility = Visibility.Collapsed;         //新增
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;        //修改
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;      //删除
            //ToolBar.btnRead.Visibility = Visibility.Visible;          //查看
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;       //审核
             //提交审核
                
        }

        //private void orgFrm_ReloadDataEvent()
        //{
        //    LoadData();
        //}

       

        #endregion



        #region IClient 成员

        public void ClosedWCFClient()
        {
            MeetingRoom.DoClose();
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
