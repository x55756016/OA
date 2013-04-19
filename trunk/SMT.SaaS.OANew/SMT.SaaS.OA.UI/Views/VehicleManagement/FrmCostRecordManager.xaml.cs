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
namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class FrmCostRecordManager :  BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        public FrmCostRecordManager()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLECOST", true);
            
            vehicleClient.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vimsClient_GetVehicleInfoListCompleted);
            vehicleClient.AddCostRecordListCompleted += new EventHandler<AddCostRecordListCompletedEventArgs>(armSeivice_AddCostRecordListCompleted);
            vehicleClient.UpdateCostRecordListCompleted += new EventHandler<UpdateCostRecordListCompletedEventArgs>(armSeivice_UpdateCostRecordListCompleted);
            vehicleClient.DeleteCostRecordListCompleted += new EventHandler<DeleteCostRecordListCompletedEventArgs>(armSeivice_DeleteCostRecordListCompleted);
            vehicleClient.GetCostRecordListCompleted += new EventHandler<GetCostRecordListCompletedEventArgs>(armSeivice_GetCostRecordListCompleted);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnAlt_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            GetData(dataPager.PageIndex);
            this.Loaded += new RoutedEventHandler(FrmCostRecordManager_Loaded);
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_COSTRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                T_OA_COSTRECORD costRecord = selectItems[0];
                
                CFrmCostRecordManager vehicleInfo = new CFrmCostRecordManager();
                vehicleInfo.EditState = "view";
                vehicleInfo.CostInfo = costRecord;
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "VIEW"));
            }
        }

        void FrmCostRecordManager_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("t_oa_costrecord");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_costrecord");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }
        //分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }
        void armSeivice_GetCostRecordListCompleted(object sender, GetCostRecordListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_COSTRECORD> vehicleInfoData = e.Result;
            List<T_OA_COSTRECORD> vehicleUseApp = null;
            if (vehicleInfoData != null)
            {
                vehicleUseApp = vehicleInfoData.ToList();
                originalNum = vehicleUseApp.Count;
            }
            else
            {
                originalNum = 0;
            }
            BindDateGrid(vehicleUseApp);
            loadbar.Stop();
        }

        void armSeivice_DeleteCostRecordListCompleted(object sender, DeleteCostRecordListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                Utility.ShowMessageBox("DELETE", false, true);
                deletedList.Clear();
                GetData(dataPager.PageIndex);
            }
            else
            {
                Utility.ShowMessageBox("DELETE", false, false);
            }
        }

        void armSeivice_UpdateCostRecordListCompleted(object sender, UpdateCostRecordListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                Utility.ShowMessageBox("UPDATE", false, true);
                GetData(dataPager.PageIndex);
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
            }
        }

        void armSeivice_AddCostRecordListCompleted(object sender, AddCostRecordListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData(dataPager.PageIndex);
                Utility.ShowMessageBox("ADD", false, true);
            }
            else
            {
                Utility.ShowMessageBox("ADD", false, false);
            }
        }

        private ObservableCollection<T_OA_VEHICLE> vehicleInfoList = null;
        private void vimsClient_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            vehicleInfoList = e.Result;
        }

        private SmtOACommonAdminClient vehicleClient = new SmtOACommonAdminClient();

        private void BindDateGrid(List<T_OA_COSTRECORD> vehicleList)
        {
            if (vehicleList == null)
            {
                dg.ItemsSource = null;
                return;
            }
            PagedCollectionView pcv = new PagedCollectionView(vehicleList);
            pcv.PageSize = 20;
            dataPager.DataContext = pcv;
            dg.ItemsSource = pcv;
        }
        private void GetData(int pageIndex)
        {
            
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值  
            if (IsQuery)
            {

                string StrContent = "";
                string StrAssetid = "";
                string StrStart = "";
                string StrEnd = "";

                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();

                StrContent = this.txtConserVationContent.Text.ToString().Trim();
                StrAssetid = this.txtVIN.Text.ToString().Trim();

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
                        filter += "COSTDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "COSTDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }


                if (!string.IsNullOrEmpty(StrContent))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CONTENT ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrContent);
                }

                if (!string.IsNullOrEmpty(StrAssetid))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //filter += "T_OA_VEHICLE.VIN ^@" + paras.Count().ToString();
                    filter +="T_OA_VEHICLE.VIN.Contains(@" + paras.Count().ToString() + ") ";
                    paras.Add(StrAssetid);
                }

            }

            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            pageIndex++;
            loadbar.Start();
            vehicleClient.GetCostRecordListAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CFrmCostRecordManager vehicleInfo = new CFrmCostRecordManager();
            vehicleInfo.EditState = "add";
            EntityBrowser browser = new EntityBrowser(vehicleInfo);
            browser.MinWidth = 500;
            browser.MinHeight = 400;

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            GetData(dataPager.PageIndex);
        }

        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_COSTRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                T_OA_COSTRECORD costRecord = selectItems[0];
                if (costRecord.FORMID != null)
                {
                    MessageBox.Show("不能修改");
                    return;
                }
                CFrmCostRecordManager vehicleInfo = new CFrmCostRecordManager();
                vehicleInfo.EditState = "update";
                vehicleInfo.CostInfo = costRecord;
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "UPDATE"));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_COSTRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                string Result = "";
                SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    if (selectItems != null)
                    {
                        try
                        {
                            vehicleClient.DeleteCostRecordListAsync(selectItems);
                        }
                        catch
                        {

                        }
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            }
        }

        private ObservableCollection<T_OA_COSTRECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_COSTRECORD> selectList = new ObservableCollection<T_OA_COSTRECORD>();
                foreach (T_OA_COSTRECORD obj in dg.SelectedItems)
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

        private int originalNum = 0;
        private ObservableCollection<T_OA_COSTRECORD> deletedList = new ObservableCollection<T_OA_COSTRECORD>();

        #region 查询按钮        
        
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQuery = true;
            GetData(dataPager.PageIndex);
        }
        #endregion
        //标记被删除的对象
    }
}