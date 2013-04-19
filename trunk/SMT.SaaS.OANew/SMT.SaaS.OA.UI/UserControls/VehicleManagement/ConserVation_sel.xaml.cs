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
    public partial class ConserVation_sel : BaseForm,IClient, IEntityEditor
    {
        /// <summary>
        /// 已审核通过的申请单
        /// </summary>
        public List<T_OA_CONSERVATION> _lst;
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();

        public ConserVation_sel()
        {
            InitializeComponent();
            _VM.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(GetVehicleInfoListCompleted);
            _VM.Sel_VCCheckedsCompleted += new EventHandler<Sel_VCCheckedsCompletedEventArgs>(Sel_VCCheckedsCompleted);

            DateStart.Text = DateTime.Now.AddDays(-7).ToShortDateString();
            DateEnd.Text = DateTime.Today.ToShortDateString();
        }
        //窗体加载事件
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _lst = new List<T_OA_CONSERVATION>();
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
            _VM.Sel_VCCheckedsAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", "", paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID, null, "2");
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
        void Sel_VCCheckedsCompleted(object sender, Sel_VCCheckedsCompletedEventArgs e)
        {
            ObservableCollection<T_OA_CONSERVATION> vehicleInfoData = e.Result;
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
            return Utility.GetResourceStr("VehicleConserVation_sel");
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
                T_OA_CONSERVATION vd = dg.SelectedItem as T_OA_CONSERVATION;
                //vd.T_OA_VEHICLE = cmbVehicleInfo.SelectedItem as T_OA_VEHICLE;
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
