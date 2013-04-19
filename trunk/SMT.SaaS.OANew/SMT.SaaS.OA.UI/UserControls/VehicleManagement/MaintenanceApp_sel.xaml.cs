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

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MaintenanceApp_sel : BaseForm,IClient,IEntityEditor
    {
        /// <summary>
        /// 已审核通过的申请单
        /// </summary>
        public List<T_OA_MAINTENANCEAPP> _lst;  
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();

        public MaintenanceApp_sel()
        {
            InitializeComponent();
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Get_VMAppCheckedCompleted += new EventHandler<Get_VMAppCheckedCompletedEventArgs>(Get_VMAppCheckedCompleted);

            DateStart.Text = DateTime.Now.AddDays(-7).ToShortDateString();
            DateEnd.Text = DateTime.Today.ToShortDateString();
        }
        //窗体加载事件
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _lst = new List<T_OA_MAINTENANCEAPP>();
            _VM.GetVehicleInfoListAsync();
           
        }
        //加载数据
        private void GetData(int pageIndex, string checkState)
        {
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //查询过滤条件  
           
            paras.Add(DateTime.Parse(DateStart.Text));
            paras.Add(DateTime.Parse(DateEnd.Text));
            paras.Add((cmbVehicleInfo.SelectedItem as T_OA_VEHICLE).ASSETID);
            _VM.Get_VMAppCheckedAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", "", paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, "2");
        }
        //车辆
        void GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> vehicleInfoList = e.Result;
            if (vehicleInfoList != null)
            {
                cmbVehicleInfo.ItemsSource = vehicleInfoList;
                cmbVehicleInfo.DisplayMemberPath = "VIN";
                cmbVehicleInfo.SelectedIndex = 0;

                GetData(dataPager.PageIndex, "2");
            }
        }
        //获取数据
        void Get_VMAppCheckedCompleted(object sender, Get_VMAppCheckedCompletedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCEAPP> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
                dg.ItemsSource = vehicleInfoData.ToList();
            else
                dg.ItemsSource = null;
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
            return Utility.GetResourceStr("VehicleMaintenanceApp_sel");
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
             {ToolbarItemDisplayTypes.Image,"1","SEARCH","/SMT.SaaS.FrameworkUI;Component/Images/Tool/LookUp.png"},
             {ToolbarItemDisplayTypes.Image,"0","CONFIRMBUTTON","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},
            };
            return VehicleMgt.GetToolBarItems(ref arr);
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "1":
                    search();

                    break;
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

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lst.Clear();
            if (dg.SelectedIndex > -1)
            {
                T_OA_MAINTENANCEAPP vd = dg.SelectedItem as T_OA_MAINTENANCEAPP;
                vd.T_OA_VEHICLE = cmbVehicleInfo.SelectedItem as T_OA_VEHICLE;
                _lst.Add(vd);
            }
        }


        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
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
