using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseInfoChooseForm : BaseForm,IClient, IEntityEditor
    {
        private SmtOACommonAdminClient client;
        private List<V_HouseInfoTree> treeViewItemList;
        private CheckBox phaseBox = new CheckBox();
        private V_HouseInfoTree currentItem;
        private T_OA_HOUSEINFO houseObj;
        public List<T_OA_HOUSEINFO> houseInfoList;
        private ObservableCollection<string> houseID = new ObservableCollection<string>();
        private string filter = "";    //查询过滤条件
        private ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        private string roomNo = "";
        private SMTLoading loadbar = new SMTLoading(); 

        #region 初始化
        public HouseInfoChooseForm()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            InitEvent();
            InitData();
        }

        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();
            client.GetHouseInfoTreeCompleted += new EventHandler<GetHouseInfoTreeCompletedEventArgs>(client_GetHouseInfoTreeCompleted);
            client.GetHouseInfoListPagingCompleted += new EventHandler<GetHouseInfoListPagingCompletedEventArgs>(client_GetHouseInfoListPagingCompleted);
            houseObj = new T_OA_HOUSEINFO();
            houseInfoList = new List<T_OA_HOUSEINFO>();
        }

        private void InitData()
        {
            loadbar.Start();
            client.GetHouseInfoTreeAsync();
        }

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (houseInfoList.Count() > 0)
            {
                houseObj = (T_OA_HOUSEINFO)e.Row.DataContext;
                var entity = from q in houseInfoList
                             where q.HOUSEID == houseObj.HOUSEID
                             select q;
                if (entity.Count() > 0)
                {
                    CheckBox chkbox = DaGr.Columns[0].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                    chkbox.IsChecked = true;
                }
            }
        }
        #endregion

        #region 完成事件
        private void client_GetHouseInfoListPagingCompleted(object sender, GetHouseInfoListPagingCompletedEventArgs e)
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
        

       
        private void client_GetHouseInfoTreeCompleted(object sender, GetHouseInfoTreeCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    treeViewItemList = e.Result.ToList();
                    MyTree.Items.Clear();
                    AddRoot();
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
                    filter += "NUM==@" + paras.Count().ToString();
                    paras.Add(roomNo);
                }
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += "ISRENT==@" + paras.Count().ToString();
            paras.Add("0");   //未出租
        }

        private void LoadData()
        {
            int pageCount = 0;
            GetParas();
            client.GetHouseInfoListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void LoadFindData()
        {
            int pageCount = 0;
            paras.Clear();
            filter = "";
            if (!string.IsNullOrEmpty(txtUptown.Text.Trim()))
            {
                filter += " UPTOWN ^@" + paras.Count().ToString();
                paras.Add(txtUptown.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtHouseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " HOUSENAME ^@" + paras.Count().ToString();
                paras.Add(txtHouseName.Text.Trim());
            }
            //只显示没有出租的房子  2010-7-28
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += " ISRENT ==@" + paras.Count().ToString();
            paras.Add("0");
            
            client.GetHouseInfoListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void BindData(List<T_OA_HOUSEINFO> obj)
        {
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion

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

        #region 按钮事件

        private void MyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = MyTree.SelectedItem as TreeViewItem;
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
                        GridHelper.SetUnCheckAll(DaGr);
                        LoadData();                        
                        break;
                    case "0":  //小区
                        uptown = selectedItem.Header.ToString();
                        currentItem.UPTOWN = uptown;
                        currentItem.HOUSENAME = "";
                        currentItem.FLOOR = 0;
                        GridHelper.SetUnCheckAll(DaGr);
                        LoadData();
                        break;
                    case "1":  //楼
                        uptown = selectedItem.GetParentTreeViewItem().Header.ToString();
                        house = selectedItem.Header.ToString();
                        currentItem.UPTOWN = uptown;
                        currentItem.HOUSENAME = house;
                        currentItem.FLOOR = 0;
                        GridHelper.SetUnCheckAll(DaGr);
                        LoadData();
                        break;
                    case "2": //楼层
                        uptown = selectedItem.GetParentTreeViewItem().GetParentTreeViewItem().Header.ToString();
                        house = selectedItem.GetParentTreeViewItem().Header.ToString();
                        floor = selectedItem.Header.ToString();
                        currentItem.UPTOWN = uptown;
                        currentItem.HOUSENAME = house;
                        currentItem.FLOOR = Convert.ToDecimal(floor);
                        GridHelper.SetUnCheckAll(DaGr);
                        LoadData();
                        break;
                }
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(DaGr);
            LoadData();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadFindData();
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                houseObj = (T_OA_HOUSEINFO)chkbox.DataContext;
                if (houseObj != null)
                {
                    foreach (var h in houseInfoList)
                    {
                        if (h.HOUSEID == houseObj.HOUSEID)
                        {
                            houseInfoList.Remove(h);
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
                houseObj = chkbox.DataContext as T_OA_HOUSEINFO;
                if (houseObj != null)
                {
                    if (houseInfoList.Count > 0)
                    {
                        var entity = from q in houseInfoList
                                     where q.HOUSEID == houseObj.HOUSEID
                                     select q;
                        if (entity.Count() == 0)
                        {
                            houseInfoList.Add(houseObj);
                        }
                    }
                    else
                    {
                        houseInfoList.Add(houseObj);
                    }
                }
            }
        }

      
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            return Utility.GetResourceStr("SELECTTITLE", "HOUSEINFOISSUANCE");
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
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CANCEL"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
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
            houseInfoList.Clear();
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IForm 成员

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
