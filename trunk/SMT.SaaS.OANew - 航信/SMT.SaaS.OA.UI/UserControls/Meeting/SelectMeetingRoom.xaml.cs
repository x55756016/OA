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

using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SelectMeetingRoom : BaseForm,IClient, IEntityEditor
    {

        //private SmtOACommonAdminClient client;
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
        //private SMTLoading loadbar = new SMTLoading(); 
        private string StrCheckState = ""; //审核状态
        DateTime DtStart;//开始时间
        DateTime DtEnd;//结束时间
        List<string> lstroomids = new List<string>();
        #region 初始化
        public SelectMeetingRoom()
        {
            InitializeComponent();
            //PARENT.Children.Add(loadbar);            
            //loadbar.Start();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            InitData();            
            //LoadData();
            
        }

        public SelectMeetingRoom(DateTime dtstart,DateTime dtend)
        {
            InitializeComponent();
            DtStart = dtstart;
            DtEnd = dtend;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            lstroomids.Clear();
            InitData();
            

        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
           
        
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

            client.GetMeetingRoomTreeInfosByCompanyIDAsync("");
        }
        #endregion

        
        private void InitData()
        {
            try
            {
                DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
                organclient.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(organclient_GetCompanyAllCompleted);
                client.GetMeetingRoomTreeInfosByCompanyIDCompleted += new EventHandler<GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs>(client_GetMeetingRoomTreeInfosByCompanyIDCompleted);
                client.GetMeetingRoomInfosCompleted += new EventHandler<GetMeetingRoomInfosCompletedEventArgs>(client_GetMeetingRoomInfosCompleted);
                client.GetMeetingRoomAppBySelectRoomsCompleted += new EventHandler<GetMeetingRoomAppBySelectRoomsCompletedEventArgs>(client_GetMeetingRoomAppBySelectRoomsCompleted);
                client.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
                if (DtStart != null && DtEnd != null)
                {
                    client.GetMeetingRoomAppBySelectRoomsAsync(DtStart,DtEnd,"2");
                }
                
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            
            
        }

        void client_GetMeetingRoomAppBySelectRoomsCompleted(object sender, GetMeetingRoomAppBySelectRoomsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<T_OA_MEETINGROOMAPP> apps = e.Result.ToList();
                apps.ForEach(item => {
                    lstroomids.Add(item.T_OA_MEETINGROOM.MEETINGROOMID);
                });

            }
        }

        
        
        private void browser_ReloadDataEvent()
        {
            LoadData();
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
            LoadData();
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
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {

                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
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
                        //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                    }
                    dataPager.PageCount = e.pageCount;
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            
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
            //过滤被占用的会议室
            if (lstroomids.Count() > 0)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(MEETINGROOMID) ";
                
                paras.Add(lstroomids);
            }


            

        }

        private void LoadData()
        {
            int pageCount = 0;
            GetParas();

            client.GetMeetingRoomInfosAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
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
            SelectedMeetingRoom = obj.FirstOrDefault();
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
                            objTreeNode.Header = company.FirstOrDefault().CNAME;
                            
                            objTreeNode.DataContext = company.FirstOrDefault();
                            objTreeNode.Tag = company.FirstOrDefault().COMPANYID; //公司
                            item.Items.Add(objTreeNode);
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

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            return Utility.GetResourceStr("SELECTTITLE", "MEETINGROOM");
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CANCEL"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
                
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 确定、取消
        private void Save()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Cancel()
        {
            MeetingRoomInfoList.Clear();
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion



        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
            organclient.DoClose();
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
