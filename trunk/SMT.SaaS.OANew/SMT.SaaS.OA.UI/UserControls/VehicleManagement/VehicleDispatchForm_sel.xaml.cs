using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 获取派车单，合并派车
    /// </summary>
    public partial class VehicleDispatchForm_sel : BaseForm,IClient, IEntityEditor
    {
        /// <summary>
        /// 存放选中的派车单
        /// </summary>
        public List<T_OA_VEHICLEDISPATCH> _lstVDispatch;
        private SmtOACommonAdminClient vehicleManager = new SmtOACommonAdminClient();
        public VehicleDispatchForm_sel(ref List<T_OA_VEHICLE> lstVehicle, int index, DateTime startDate)
        {
            InitializeComponent();
            vehicleManager.Gets_VDCheckedCompleted += new EventHandler<Gets_VDCheckedCompletedEventArgs>(Gets_VDCheckedCompleted);

            cmbVehicleInfo.ItemsSource = lstVehicle;
            cmbVehicleInfo.DisplayMemberPath = "VIN";
            cmbVehicleInfo.SelectedIndex = index;
            dtiStartDate.DateTimeValue = startDate;
            dtiStartDate.tpTime.Visibility = Visibility.Collapsed;

        }
        
        //窗体加载事件
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _lstVDispatch = new List<T_OA_VEHICLEDISPATCH>();
            GetData(dataPager.PageIndex, "2");
        }
        //加载数据
        private void GetData(int pageIndex, string checkState)
        {
            if (cmbVehicleInfo != null && cmbVehicleInfo.Items.Count > 0)
            {
                int pageCount = 0;
                ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值     
                paras.Add((cmbVehicleInfo.SelectionBoxItem as T_OA_VEHICLE).ASSETID);
                paras.Add(dtiStartDate.DateTimeValue);

                SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
                vehicleManager.Gets_VDCheckedAsync(pageIndex, dataPager.PageSize, "CREATEDATE", "", paras, pageCount, loginInfo);
            }
        }
        //获取数据 2
        void Gets_VDCheckedCompleted(object sender, Gets_VDCheckedCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCH> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
                dg.ItemsSource = vehicleInfoData.ToList();
            else
                dg.ItemsSource = null;
        }       
        //分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex, "2");
        }
        //勾选
        private void rdb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            if (rdb.IsChecked.Value)
            {
                T_OA_VEHICLEDISPATCH vd = (T_OA_VEHICLEDISPATCH)rdb.DataContext;
                vd.T_OA_VEHICLE = cmbVehicleInfo.SelectedItem as T_OA_VEHICLE;
                _lstVDispatch.Add(vd);
            }
        }
        /// <summary>
        /// 判断是否选择了数据 如果没有 则提示要选择数据
        /// </summary>
        private void CheckData()
        {
            if (!(_lstVDispatch.Count > 0))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("PLEASESELECT", "VEHICLEUSEAPP"));
            }
            else
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        private void rdb_Unchecked(object sender, RoutedEventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            if (!rdb.IsChecked.Value)
            {
                T_OA_VEHICLEDISPATCH vd = (T_OA_VEHICLEDISPATCH)rdb.DataContext;
                _lstVDispatch.Remove(vd);
            }
        }       

        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleDispatch_sel");
        }

        public string GetStatus()
        {
            return "";
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{
             {ToolbarItemDisplayTypes.Image,"1","SEARCH","/SMT.SaaS.FrameworkUI;Component/Images/(09,24).png"},
             {ToolbarItemDisplayTypes.Image,"0","CONFIRMBUTTON","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},
            };
            return VehicleMgt.GetToolBarItems(ref arr);
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    CheckData();
                    
                    break;
                case "1":
                    search();
                    
                    break;
            }
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
        /// <summary>
        /// 查询
        /// </summary>
        public void search()
        {
           // dg.SelectedIndex = -1;
            dataPager.PageIndex = 1;
            GetData(dataPager.PageIndex, "2");
        }

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lstVDispatch.Clear();
            if (dg.SelectedIndex > -1)
            {               
                T_OA_VEHICLEDISPATCH vd = dg.SelectedItem as T_OA_VEHICLEDISPATCH;
                vd.T_OA_VEHICLE = cmbVehicleInfo.SelectedItem as T_OA_VEHICLE;
                _lstVDispatch.Add(vd);
            }
        }




        #region IForm 成员

        public void ClosedWCFClient()
        {
            vehicleManager.DoClose();
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
