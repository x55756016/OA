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
    public partial class VehicleUseAppForm_sel : BaseForm,IClient,IEntityEditor
    {
        /// <summary>
        /// 已选中的申请单
        /// </summary>
        public List<T_OA_VEHICLEUSEAPP> _lstVUseApp_Add;
        private SmtOACommonAdminClient vehicleManager = new SmtOACommonAdminClient();
        public VehicleUseAppForm_sel(ref List<T_OA_VEHICLE> lstVehicle, int index, DateTime startDate)
        {
            InitializeComponent();
            vehicleManager.GetCanUseVehicleUseAppInfoListCompleted += new EventHandler<GetCanUseVehicleUseAppInfoListCompletedEventArgs>(GetCanUseVehicleUseAppInfoListCompleted);

           
            dtiStartDate.DateTimeValue = startDate;
            dtiStartDate.tpTime.Visibility = Visibility.Collapsed;
            dtiEndDate.DateTimeValue = startDate.AddDays(10);
            dtiEndDate.tpTime.Visibility = Visibility.Collapsed;
        }
        //窗体加载事件
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _lstVUseApp_Add = new List<T_OA_VEHICLEUSEAPP>();
            GetData(dataPager.PageIndex, "2");
        }
        //加载数据
        private void GetData(int pageIndex, string checkState)
        {
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //查询过滤条件             
            paras.Add(dtiStartDate.DateTimeValue);
            paras.Add(dtiEndDate.DateTimeValue);
            vehicleManager.GetCanUseVehicleUseAppInfoListAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", "", paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID);
        }
        //获取数据
        void GetCanUseVehicleUseAppInfoListCompleted(object sender, GetCanUseVehicleUseAppInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEUSEAPP> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
                dgvVehicleUseAppList.ItemsSource = vehicleInfoData.ToList();
            else
                dgvVehicleUseAppList.ItemsSource = null;
        }
        /// <summary>
        /// 判断是否选择了数据 如果没有 则提示要选择数据
        /// </summary>
        private void CheckData()
        {
            if (!(_lstVUseApp_Add.Count > 0))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("PLEASESELECT", "VEHICLEUSEAPP"));
            }
            else
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex, "2");
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
        public string GetTitle()
        {
            return Utility.GetResourceStr("VehicleUseApp_sel");
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
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "1":
                    search();

                    break;
            }
        }
        //勾选
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_VEHICLEUSEAPP vd = (T_OA_VEHICLEUSEAPP)chk.DataContext;
                _lstVUseApp_Add.Add(vd);
            }
        }

        //取消勾选
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_VEHICLEUSEAPP vd = (T_OA_VEHICLEUSEAPP)chk.DataContext;
                _lstVUseApp_Add.Remove(vd);
            }
        }
        /// <summary>
        /// 查询
        /// </summary>
        public void search()
        {
            dataPager.PageIndex = 1;
            GetData(dataPager.PageIndex, "2");
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
