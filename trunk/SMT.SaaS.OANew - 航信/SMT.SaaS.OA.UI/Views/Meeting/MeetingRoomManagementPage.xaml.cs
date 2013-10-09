using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.OA.UI.UserControls.Meeting;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingRoomManagementPage : BasePage
    {
        private SmtOACommonOfficeClient client = new SmtOACommonOfficeClient();
        private OrganizationServiceClient organclient = new OrganizationServiceClient();
        private List<T_HR_COMPANY> treeCompanyList;
        private List<T_OA_MEETINGROOM> treeViewItemList;
        private CheckBox phaseBox = new CheckBox();
        private T_OA_MEETINGROOM currentItem;
        private T_HR_COMPANY currentcompany;
        private string StrCompanyID = "";
        private T_OA_MEETINGROOM MeetingRoomObj;
        public List<T_OA_MEETINGROOM> MeetingRoomInfoList;
        private ObservableCollection<string> houseID = new ObservableCollection<string>();
        private string filter = "";    //查询过滤条件
        private ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        private string roomNo = "";
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        public T_OA_MEETINGROOM SelectedMeetingRoom { get; set; }
        private SMTLoading loadbar = new SMTLoading();
        public MeetingRoomManagementPage()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAMEETINGROOM", true);
            PARENT.Children.Add(loadbar);
            
            loadbar.Start();
            InitData();
            LoadData();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());   
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            this.LoadData();
        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_MEETINGROOM");
        }
        #region 查询按钮        
        
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 数据选择
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MyTree != null)
            {
                TreeViewItem selectedItem = MyTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    StrCompanyID = selectedItem.Tag.ToString();
                    
                    LoadData();                    
                }
            }

            client.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
        }
        #endregion

        
        private void InitData()
        {
            try
            {
                //DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
                organclient.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(organclient_GetCompanyAllCompleted);
                client.GetMeetingRoomTreeInfosByCompanyIDCompleted += new EventHandler<GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs>(client_GetMeetingRoomTreeInfosByCompanyIDCompleted);
                client.GetMeetingRoomInfosCompleted += new EventHandler<GetMeetingRoomInfosCompletedEventArgs>(client_GetMeetingRoomInfosCompleted);
                client.BatchDelMeetingRoomdInfosCompleted += new EventHandler<BatchDelMeetingRoomdInfosCompletedEventArgs>(client_BatchDelMeetingRoomdInfosCompleted);
                ToolBar.stpCheckState.Visibility = Visibility.Collapsed;
                ToolBar.btnAudit.Visibility = Visibility.Collapsed;
                ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                client.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
                this.Loaded += new RoutedEventHandler(MeetingRoomManagementPage_Loaded);
                DaGr.SelectionChanged += new SelectionChangedEventHandler(DaGr_SelectionChanged);
                ToolBar.ShowRect();
            }
            catch (Exception ex)
            { 
                throw(ex);
            } 
        }

        void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
            {
                return;
            }
            if (grid.SelectedItems.Count >0 )
            {
                SelectedMeetingRoom = (T_OA_MEETINGROOM)grid.SelectedItems[0];
            }
        }

        void client_BatchDelMeetingRoomdInfosCompleted(object sender, BatchDelMeetingRoomdInfosCompletedEventArgs e)
        {
            loadbar.Stop();
            if (!e.Cancelled)
            {

                if (e.Result.ToString() == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "MEETINGROOM"));
                    LoadData();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                    return;
                }
            }
        }

        void MeetingRoomManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_MEETINGROOM");
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
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
                    client.BatchDelMeetingRoomdInfosAsync(DelInfosList);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


            ////throw new NotImplementedException();
            
            //T_OA_MEETINGROOM MeetingRoomT = new T_OA_MEETINGROOM();

            //if (DaGr.ItemsSource != null)
            //{
            //    foreach (var obj in DaGr.SelectedItems)
            //    {
            //        DelInfosList.Add(((T_OA_MEETINGROOM)obj).MEETINGROOMID);

            //        //if (DaGr.Columns[0].GetCellContent(obj) != null)
            //        //{

            //        //    CheckBox cb1 = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
            //        //    if (cb1.IsChecked == true)
            //        //    {
            //        //        MeetingRoomT = cb1.Tag as T_OA_MEETINGROOM;
            //        //        RoomId = MeetingRoomT.MEETINGROOMID;
            //        //        DelInfosList.Add(RoomId);
            //        //        //RoomId += cb1.Tag.ToString() + ",";
            //        //    }

            //        //}
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
            //                client.BatchDelMeetingRoomdInfosAsync(DelInfosList);
            //                DelInfosList.Clear();

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

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMeetingRoom != null)
            {
                MeetingRoomChildWindow orgFrm = new MeetingRoomChildWindow(Action.Read, SelectedMeetingRoom);
                //OrganAddForm orgFrm = new OrganAddForm(Action.Add, "");
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 650;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.LoadData();
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMeetingRoom != null)
            {
                MeetingRoomChildWindow orgFrm = new MeetingRoomChildWindow(Action.Edit, SelectedMeetingRoom);
                //OrganAddForm orgFrm = new OrganAddForm(Action.Add, "");
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.MinWidth = 650;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            
            MeetingRoomChildWindow orgFrm = new MeetingRoomChildWindow(Action.Add, null);
            //OrganAddForm orgFrm = new OrganAddForm(Action.Add, "");
            EntityBrowser browser = new EntityBrowser(orgFrm);
            browser.MinWidth = 650;
            browser.MinHeight = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        private void browser_ReloadDataEvent()
        {
            LoadData();
            client.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
        }

        void organclient_GetCompanyAllCompleted(object sender, GetCompanyAllCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    treeCompanyList = e.Result.ToList();
                    MyTree.Items.Clear();
                    AddRoot();
                }
            }
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectedMeetingRoom = (T_OA_MEETINGROOM)grid.SelectedItems[0];
            }
        }

        #region 完成事件

        void client_GetMeetingRoomInfosCompleted(object sender, GetMeetingRoomInfosCompletedEventArgs e)
        {
            try
            {

                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        MeetingRoomInfoList = e.Result.ToList();
                        BindData(e.Result.ToList());
                    }
                    else
                    {
                        BindData(null);
                    }
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();
        }

        void client_GetMeetingRoomTreeInfosByCompanyIDCompleted(object sender, GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    treeViewItemList = e.Result.ToList();
                    //client.GetMeetingRoomTreeInfosByCompanyIDAsync("");
                    organclient.GetCompanyAllAsync("");
                    //AddRoot();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }


        #endregion

        #region 绑定DataGrid

        /// <summary>
        /// 获取查询条件
        /// </summary>
        private void GetParas()
        {
            paras.Clear();
            filter = "";
            string StrMeetingRoom = "";
            string StrDemo = "";
            StrMeetingRoom = this.txtMeetingRoom.Text.ToString();
            StrDemo = this.txtMemo.Text.ToString();
            if (!string.IsNullOrEmpty(StrCompanyID))
            {
                if (StrCompanyID != "-1") //选择了所有
                {
                    filter += "COMPANYID==@" + paras.Count().ToString();
                    paras.Add(StrCompanyID);
                }
            }
            if (!string.IsNullOrEmpty(StrMeetingRoom))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "MEETINGROOMNAME ^@" + paras.Count().ToString();
                paras.Add(StrMeetingRoom);
            }
            if (!string.IsNullOrEmpty(StrDemo))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "REMARK ^@" + paras.Count().ToString();
                paras.Add(StrDemo);
            }
        }

        private void LoadData()
        {
            int pageCount = 0;
            GetParas();

            client.GetMeetingRoomInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void BindData(List<T_OA_MEETINGROOM> obj)
        {
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
            currentItem = obj.FirstOrDefault();
            //SelectedMeetingRoom = obj.FirstOrDefault();
        }
        #endregion

        #region 绑定树
        private void AddRoot()
        {
            TreeViewItem objTreeNode = new TreeViewItem();

            objTreeNode.Header = "所有";
            objTreeNode.DataContext = null;
            objTreeNode.Tag = "-1"; //Root
            objTreeNode.IsExpanded = true;
            objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
            MyTree.Items.Add(objTreeNode);
            AddCompany(objTreeNode);
            LoadData();       //载入会议室信息
        }


        /// <summary>
        /// 添加小区节点
        /// </summary>
        private void AddCompany(TreeViewItem item)
        {
            if (treeViewItemList != null)
            {
                if (treeViewItemList.Count > 0)
                {
                    var ent = (from q in treeViewItemList
                               select q.COMPANYID).Distinct();
                    if (ent.Count() > 0)
                    {
                        foreach (var h in ent)
                        {
                            TreeViewItem objTreeNode = new TreeViewItem();
                            var company = from a in treeCompanyList
                                          where a.COMPANYID == h
                                          select a;
                            if (company.Count() >0)
                            {
                                objTreeNode.Header = company.FirstOrDefault().CNAME;
                                objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                                objTreeNode.DataContext = company.FirstOrDefault();
                                objTreeNode.Tag = company.FirstOrDefault().COMPANYID; //公司
                                item.Items.Add(objTreeNode);
                            }
                            //AddMeetingRoom(objTreeNode, h);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加公司的会议室
        /// </summary>
        /// <param name="item"></param>
        /// <param name="uptown"></param>
        private void AddMeetingRoom(TreeViewItem item, string CompanyID)
        {
            var ent = (from q in treeViewItemList
                       where q.COMPANYID == CompanyID
                       select q.MEETINGROOMNAME).Distinct();
            if (ent.Count() > 0)
            {
                foreach (var house in ent)
                {
                    TreeViewItem objTreeNode = new TreeViewItem();
                    objTreeNode.Header = house;
                    objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    objTreeNode.DataContext = house;
                    objTreeNode.Tag = "1";  //会议室
                    item.Items.Add(objTreeNode);
                    //AddFloor(objTreeNode, uptown, house);
                }
            }
        }



        #endregion

        #region 按钮事件      

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            chkbox.Style = Application.Current.Resources["CheckBoxStyle"] as Style;
            if (!chkbox.IsChecked.Value)
            {
                MeetingRoomObj = (T_OA_MEETINGROOM)chkbox.DataContext;
                if (MeetingRoomObj != null)
                {
                    foreach (var h in MeetingRoomInfoList)
                    {
                        if (h.MEETINGROOMID == MeetingRoomObj.MEETINGROOMID)
                        {
                            MeetingRoomInfoList.Remove(h);
                            break;
                        }
                    }
                }
            }
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            chkbox.Style = Application.Current.Resources["CheckBoxStyle"] as Style;
            if (chkbox.IsChecked.Value)
            {
                MeetingRoomObj = chkbox.DataContext as T_OA_MEETINGROOM;
                if (MeetingRoomObj != null)
                {
                    if (MeetingRoomInfoList.Count > 0)
                    {
                        var entity = from q in MeetingRoomInfoList
                                     where q.MEETINGROOMID == MeetingRoomObj.MEETINGROOMID
                                     select q;
                        if (entity.Count() == 0)
                        {
                            MeetingRoomInfoList.Add(MeetingRoomObj);
                        }
                    }
                    else
                    {
                        MeetingRoomInfoList.Add(MeetingRoomObj);
                    }
                }
            }
        }
        #endregion
    }
}
