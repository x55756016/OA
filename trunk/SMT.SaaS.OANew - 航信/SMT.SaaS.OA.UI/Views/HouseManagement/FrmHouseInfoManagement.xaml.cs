using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.HouseManagement
{
    public partial class FrmHouseInfoManagement : BasePage,IClient
    {
        private SmtOACommonAdminClient client;
        private List<V_HouseInfoTree> treeViewItemList;
        private V_HouseInfoTree currentItem;
        private T_OA_HOUSEINFO houseObj;
        private ObservableCollection<string> houseDelID ;
        private string filter = "";    //查询过滤条件
        private string filterHire = "";
        private ObservableCollection<object> paras = new ObservableCollection<object>();   //参数值
        private string roomNo = "";
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_HOUSEINFO houseinfo;

        public T_OA_HOUSEINFO Houseinfo
        {
            get { return houseinfo; }
            set { houseinfo = value; }
        }
        public FrmHouseInfoManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_HOUSEINFO", true);
            PARENT.Children.Add(loadbar);
            InitEvent();
            InitData();
        }

        #region 初始化
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();
            client.GetHouseInfoTreeCompleted += new EventHandler<GetHouseInfoTreeCompletedEventArgs>(client_GetHouseInfoTreeCompleted);
            client.GetHouseInfoListPagingCompleted += new EventHandler<GetHouseInfoListPagingCompletedEventArgs>(client_GetHouseInfoListPagingCompleted);
            client.DeleteHouseCompleted += new EventHandler<DeleteHouseCompletedEventArgs>(client_DeleteHouseCompleted);
            client.GetHouseHirerListPagingCompleted += new EventHandler<GetHouseHirerListPagingCompletedEventArgs>(client_GetHouseHirerListPagingCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            
            //ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            //ToolBar.btnRead.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            this.Loaded += new RoutedEventHandler(FrmHouseInfoManagement_Loaded);
            this.DaGr.CurrentCellChanged += new EventHandler<EventArgs>(DaGr_CurrentCellChanged);
            ToolBar.ShowRect();
        }

        void DaGr_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Houseinfo = (T_OA_HOUSEINFO)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmHouseInfoManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_HOUSEINFO");
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Houseinfo != null)
            {
                HouseInfoManagementForm form = new HouseInfoManagementForm(Action.Read, Houseinfo.HOUSEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }
        
        private void InitData()
        {
            
            loadbar.Start();
            client.GetHouseInfoTreeAsync();
            LoadData();
        }


        /// <summary>
        /// 获取查询条件
        /// </summary>
        private void GetParas()
        {
            if (currentItem != null)
            {
                paras.Clear();
                filter = "";
                if (!string.IsNullOrEmpty(currentItem.UPTOWN))
                {
                    filter += "UPTOWN==@" + paras.Count().ToString();
                    paras.Add(currentItem.UPTOWN);
                }
                if (!string.IsNullOrEmpty(currentItem.HOUSENAME))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "HOUSENAME==@" + paras.Count().ToString();
                    paras.Add(currentItem.HOUSENAME);
                }
                if (currentItem.FLOOR != 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "FLOOR==@" + paras.Count().ToString();
                    paras.Add(currentItem.FLOOR);
                }
                if (!string.IsNullOrEmpty(roomNo))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "ROOMCODE==@" + paras.Count().ToString();
                    paras.Add(roomNo);
                }
            }
        }

        //private void GetParasHirer()
        //{
        //    if (currentItem != null)
        //    {
        //        paras.Clear();
        //        filterHire = "";
        //        if (!string.IsNullOrEmpty(currentItem.UPTOWN))
        //        {
        //            filterHire += "houseObj.UPTOWN==@" + paras.Count().ToString();
        //            paras.Add(currentItem.UPTOWN);
        //        }
        //        if (!string.IsNullOrEmpty(currentItem.HOUSENAME))
        //        {
        //            if (!string.IsNullOrEmpty(filterHire))
        //            {
        //                filterHire += " and ";
        //            }
        //            filterHire += "houseObj.HOUSENAME==@" + paras.Count().ToString();
        //            paras.Add(currentItem.HOUSENAME);
        //        }
        //        if (currentItem.FLOOR != 0)
        //        {
        //            if (!string.IsNullOrEmpty(filterHire))
        //            {
        //                filterHire += " and ";
        //            }
        //            filterHire += "houseObj.FLOOR==@" + paras.Count().ToString();
        //            paras.Add(currentItem.FLOOR);
        //        }
        //        if (!string.IsNullOrEmpty(roomNo))
        //        {
        //            if (!string.IsNullOrEmpty(filterHire))
        //            {
        //                filterHire += " and ";
        //            }
        //            filterHire += "houseObj.NUM==@" + paras.Count().ToString();
        //            paras.Add(roomNo);
        //        }                
        //    }
        //}

        private void LoadData()
        {
            int pageCount = 0;
            GetParas();
            client.GetHouseInfoListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void HirerLoadData()
        {
            int pageCount = 0;
            GetParas();
            client.GetHouseHirerListPagingAsync(dataPagerRent.PageIndex, dataPagerRent.PageSize, "houseObj.CREATEDATE descending", filterHire, paras, pageCount);            
        }

        #region 绑定树
        private void AddRoot()
        {           
            TreeViewItem objTreeNode = new TreeViewItem();
            objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
            objTreeNode.Header = "所有";
            objTreeNode.DataContext = null;
            objTreeNode.Tag = "-1"; //Root
            objTreeNode.IsExpanded = true;
            MyTree.Items.Add(objTreeNode);
            AddUpTown(objTreeNode);
            LoadData();       //载入房间信息
        }


        /// <summary>
        /// 添加小区节点
        /// </summary>
        private void AddUpTown(TreeViewItem item)
        {
            if (treeViewItemList != null)
            {
                if (treeViewItemList.Count > 0)
                {
                    var ent = (from q in treeViewItemList
                               select q.UPTOWN).Distinct();
                    if (ent.Count() > 0)
                    {
                        foreach (var h in ent)
                        {
                            TreeViewItem objTreeNode = new TreeViewItem();
                            objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                            objTreeNode.Header = h;
                            objTreeNode.DataContext = h;
                            objTreeNode.Tag = "0"; //小区                            
                            item.Items.Add(objTreeNode);
                            AddHouse(objTreeNode, h);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加楼座节点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="uptown"></param>
        private void AddHouse(TreeViewItem item, string uptown)
        {

            var ent = (from q in treeViewItemList
                       where q.UPTOWN == uptown
                       select q.HOUSENAME).Distinct();
            if (ent.Count() > 0)
            {
                foreach (var house in ent)
                {
                    TreeViewItem objTreeNode = new TreeViewItem();
                    objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    objTreeNode.Header = house;
                    objTreeNode.DataContext = house;
                    objTreeNode.Tag = "1";  //楼
                    item.Items.Add(objTreeNode);
                    AddFloor(objTreeNode, uptown, house);
                }
            }
        }

        /// <summary>
        /// 添加楼层节点
        /// </summary>
        /// <param name="item"></param>
        /// <param name="uptown"></param>
        /// <param name="house"></param>
        private void AddFloor(TreeViewItem item, string uptown, string house)
        {
            var ent = (from q in treeViewItemList
                       where q.UPTOWN == uptown && q.HOUSENAME == house
                       select q.FLOOR).Distinct();
            if (ent.Count() > 0)
            {
                foreach (var floor in ent)
                {
                    TreeViewItem objTreeNode = new TreeViewItem();
                    objTreeNode.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    objTreeNode.Header = floor;
                    objTreeNode.DataContext = floor;
                    objTreeNode.Tag = "2";  //房间
                    item.Items.Add(objTreeNode);
                }
            }
        }
        #endregion
        #endregion

        #region 完成事件
        private void client_GetHouseInfoTreeCompleted(object sender, GetHouseInfoTreeCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        treeViewItemList = e.Result.ToList();
                        MyTree.Items.Clear();
                        AddRoot();
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetHouseInfoListPagingCompleted(object sender, GetHouseInfoListPagingCompletedEventArgs e)
        {
            loadbar.Stop();
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
                        BindData(e.Result.ToList(),e.pageCount);
                    }
                    else
                    {
                        BindData(null,0);
                    }
                    HirerLoadData();
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetHouseHirerListPagingCompleted(object sender, GetHouseHirerListPagingCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        HirerBindData(e.Result.ToList(),e.pageCount);
                    }
                    else
                    {
                        HirerBindData(null,0);
                        //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_DeleteHouseCompleted(object sender, DeleteHouseCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.errorMsg))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.errorMsg));
                }
                currentItem = null; //将查询条件值清空，重新加载
                filter="";
                paras.Clear();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            InitData();
        }
        #endregion

        #region 绑定数据源
        /// <summary>
        /// 绑定房源DataGird
        /// </summary>
        /// <param name="obj"></param>
        private void BindData(List<T_OA_HOUSEINFO> obj,int pageCount)
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


        /// <summary>
        /// 绑定租客DataGird
        /// </summary>
        /// <param name="obj"></param>
        private void HirerBindData(List<V_HouseHirer> obj,int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPagerRent, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DaGrRent.ItemsSource = null;
                return;
            }
            DaGrRent.ItemsSource = obj;
        }
        #endregion

        #region 按钮事件
        /// <summary>
        /// 架构树选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (MyTree != null)
            {
                TreeViewItem selectedItem = MyTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    if (!string.IsNullOrEmpty(selectedItem.Tag.ToString().Trim()))
                    {
                        string uptown = "", house = "", floor = "";
                        roomNo = "";
                        if (currentItem == null)
                        {
                            currentItem = new V_HouseInfoTree();
                        }
                        switch (selectedItem.Tag.ToString())
                        {
                            case "-1":  //ROOT
                                currentItem.UPTOWN = "";
                                currentItem.HOUSENAME = "";
                                currentItem.FLOOR = 0;                                
                                break;
                            case "0":  //小区
                                uptown = selectedItem.Header.ToString();
                                currentItem.UPTOWN = uptown;
                                currentItem.HOUSENAME = "";
                                currentItem.FLOOR = 0;
                                break;
                            case "1":  //楼
                                uptown = selectedItem.GetParentTreeViewItem().Header.ToString();
                                house = selectedItem.Header.ToString();
                                currentItem.UPTOWN = uptown;
                                currentItem.HOUSENAME = house;
                                currentItem.FLOOR = 0;
                                break;
                            case "2": //楼层
                                uptown = selectedItem.GetParentTreeViewItem().GetParentTreeViewItem().Header.ToString();
                                house = selectedItem.GetParentTreeViewItem().Header.ToString();
                                floor = selectedItem.Header.ToString();
                                currentItem.UPTOWN = uptown;
                                currentItem.HOUSENAME = house;
                                currentItem.FLOOR = Convert.ToDecimal(floor);
                                break;
                        }
                        //GridHelper.SetUnCheckAll(DaGr);
                        LoadData();
                        //houseDelID.Clear();
                        //houseObj = null;
                    }
                }
            }
        }
       
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadFindData();
        }

        private void LoadFindData()
        {
            int pageCount = 0;
            paras.Clear();
            filter = "";
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                filter += "UPTOWN^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "HOUSENAME^@" + paras.Count().ToString();
                paras.Add(txtHouseName.Text.Trim());
            }
            client.GetHouseInfoListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount,Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            HouseInfoManagementForm form = new HouseInfoManagementForm(Action.Add, null);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 600; 
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});

        }

        private void addFrm_ReloadDataEvent()
        {
            //houseObj = null;        //设置当前实体为空
            InitData();
        }

        private void browser_ReloadDataEvent()
        {
            //houseObj = null;        //设置当前实体为空
            //houseDelID.Clear();
            Houseinfo = null;
            InitData();
        }  

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (Houseinfo != null)
            {
                HouseInfoManagementForm form = new HouseInfoManagementForm(Action.Edit, Houseinfo.HOUSEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

                        
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (DaGr.SelectedItems.Count > 0)
            {

                string Result = "";
                houseDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                    {
                        string GradeID = "";
                        GradeID = (DaGr.SelectedItems[i] as T_OA_HOUSEINFO).HOUSEID;
                        if (!(houseDelID.IndexOf(GradeID) > -1))
                        {
                            houseDelID.Add(GradeID);
                        }
                    }
                    string errorMsg = "";
                    client.DeleteHouseAsync(houseDelID, errorMsg);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> houseID = new ObservableCollection<string>();
            houseID = GridHelper.GetSelectItem(DaGr, "myChkBox", Action.Edit);
            if (houseID != null && houseID.Count > 0)
            {
                HouseInfoManagementForm form = new HouseInfoManagementForm(Action.AUDIT, houseID[0]);
                EntityBrowser browser = new EntityBrowser(form);
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //HtmlPage.Window.Alert("请先选择需要修改的借阅记录！");
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

       

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            InitData();
        }    

        //private void ButtonOK_Click(object sender, RoutedEventArgs e)
        //{
        //    client.DeleteHouseAsync(houseDelID);
        //}


        //房源DataGird分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            //GridHelper.SetUnCheckAll(DaGr);
            LoadData();
        }

        //租客DataGird分页
        private void GridPager_Click_1(object sender, RoutedEventArgs e)
        {
            HirerLoadData();
        }


        //模板列选择
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    houseObj = null;
            //    houseDelID.Remove(chkbox.Tag.ToString());
            //}
            GridHelper.SetUnCheckAll(DaGr);
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    houseObj = chkbox.DataContext as T_OA_HOUSEINFO;
            //    houseDelID.Add(chkbox.Tag.ToString());
            //}
        }

        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                T_OA_HOUSEINFO houseObj = grid.SelectedItems[0] as T_OA_HOUSEINFO;
                if (houseObj != null)
                {
                    if (currentItem == null)
                    {
                        currentItem = new V_HouseInfoTree();
                    }
                    currentItem.UPTOWN = houseObj.UPTOWN;
                    currentItem.HOUSENAME = houseObj.HOUSENAME;
                    currentItem.FLOOR = houseObj.FLOOR;
                    roomNo = houseObj.ROOMCODE;
                    HirerLoadData();
                }
            }
        }
        
        #endregion

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_HOUSEINFO");
        }

        #region IClient 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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
