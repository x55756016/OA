using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.UserControls;

namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class FrmVehicleInfoManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        public FrmVehicleInfoManager()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_VEHICLE", true);
            
            vehicleManager.GetVehicleInfoListByPageCompleted += new EventHandler<GetVehicleInfoListByPageCompletedEventArgs>(vehicleManager_GetVehicleInfoListByPageCompleted);
            vehicleManager.DeleteVehicleListCompleted += new EventHandler<DeleteVehicleListCompletedEventArgs>(vehicleManager_DeleteVehicleListCompleted);
            #region 原来的
            /*
            ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); 
            GetData(dataPager.PageIndex);
            */
            #endregion
            this.Loaded += new RoutedEventHandler(FrmVehicleInfoManager_Loaded);
            ToolBar.ShowRect();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                VehicleInfo_upd vehicleInfo = new VehicleInfo_upd(FormTypes.Browse);
                vehicleInfo.VehicleInfo = selectItems[0];
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 400;
                //browser.MinWidth = 315;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void FrmVehicleInfoManager_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
         
            ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); 
            GetData(dataPager.PageIndex);

            #endregion
            GetEntityLogo("T_OA_VEHICLE");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           SetRowLogo(dg, e.Row, "T_OA_VEHICLE");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }

        

        void vehicleManager_GetVehicleInfoListByPageCompleted(object sender, GetVehicleInfoListByPageCompletedEventArgs e)
        {
            IsQuery = false;
            if (e.Result != null)
            {
                List<T_OA_VEHICLE> vehicleInfoData = e.Result.ToList();
                BindDateGrid(vehicleInfoData);
            }
            else
                BindDateGrid(null);

            loadbar.Stop();
        }

        void dataPager_PageIndexChanged(object sender, EventArgs e)
        {
            GetData(dataPager.PageIndex);
        }

        void vehicleManager_DeleteVehicleListCompleted(object sender, DeleteVehicleListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData(dataPager.PageIndex);
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
            {
                Utility.ShowMessageBox("DELETE", false, false);
            }
        }
       
        private void BindDateGrid(List<T_OA_VEHICLE> vehicleList)
        {
            if (vehicleList != null && vehicleList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(vehicleList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
            {
                dg.ItemsSource = null;
            }
        } 
        private void GetData(int pageIndex)
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQuery)
            {
                string StrVin = "";
                string StrCarModel = "";
                string StrAsset = "";
                string StrStart = "";
                string StrEnd = "";
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrVin = this.txVIN.Text.ToString().Trim();
                StrCarModel = this.txtCarModel.Text.ToString().Trim();
                StrAsset = this.txtASSETID.Text.ToString().Trim();
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //MessageBox.Show("结束时间不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                    //MessageBox.Show("开始时间不能为空");
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "CREATEDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "CREATEDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrAsset))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "ASSETID ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrAsset);
                }
                if (!string.IsNullOrEmpty(StrVin))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "VIN ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrVin);

                }
                if (!string.IsNullOrEmpty(StrCarModel))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "VEHICLEMODEL ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrCarModel);

                }
            }
           
            vehicleManager.GetVehicleInfoListByPageAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }
        //添加
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            VehicleInfo_add vehicleInfo = new VehicleInfo_add();
            EntityBrowser browser = new EntityBrowser(vehicleInfo);
            browser.MinHeight = 400;
            //browser.MinWidth = 315;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        void browser_ReloadDataEvent()
        {
            GetData(dataPager.PageIndex);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    try
                    {
                        vehicleManager.DeleteVehicleListAsync(selectItems);
                    }
                    catch
                    {

                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                VehicleInfo_upd vehicleInfo = new VehicleInfo_upd(FormTypes.Edit);
                vehicleInfo.VehicleInfo = selectItems[0];
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.MinHeight = 400;
                //browser.MinWidth = 315;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private ObservableCollection<T_OA_VEHICLE> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_VEHICLE> selectList = new ObservableCollection<T_OA_VEHICLE>();
                foreach (T_OA_VEHICLE obj in dg.SelectedItems)
                        selectList.Add(obj);
                    
                if (selectList != null && selectList.Count > 0)
                {
                    return selectList;
                }
            }
            return null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
        private SmtOACommonAdminClient vehicleManager = new SmtOACommonAdminClient();
        private T_OA_VEHICLE selectInfo = null;


        private void SetCheckBoxColumnChecked(bool isChecked)
        {
            if (dg.ItemsSource != null)
            {
                foreach (object obj in dg.ItemsSource)
                {
                    if (dg.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = dg.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        ckbSelect.IsChecked = isChecked;
                    }
                }
            }
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }

        #region 查询按钮        
        
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQuery = true;
            GetData(dataPager.PageIndex);
        }
        #endregion
    }
}