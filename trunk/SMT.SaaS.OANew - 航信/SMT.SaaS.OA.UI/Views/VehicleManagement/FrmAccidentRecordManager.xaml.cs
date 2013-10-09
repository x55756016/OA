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
namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class FrmAccidentRecordManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        public FrmAccidentRecordManager()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLEACCIDENT", true);

            vehicleClient.GetAccidentRecordListCompleted += new EventHandler<GetAccidentRecordListCompletedEventArgs>(armSeivice_GetAccidentRecordListCompleted);
            vehicleClient.DeleteAccidentRecordListCompleted += new EventHandler<DeleteAccidentRecordListCompletedEventArgs>(armSeivice_DeleteAccidentRecordListCompleted);
            vehicleClient.GetVehicleInfoListCompleted += new EventHandler<GetVehicleInfoListCompletedEventArgs>(vimsClient_GetVehicleInfoListCompleted);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnAlt_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); GetData(dataPager.PageIndex);
            this.Loaded += new RoutedEventHandler(FrmAccidentRecordManager_Loaded);
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_ACCIDENTRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                CFrmAccidentRecordManager vehicleInfo = new CFrmAccidentRecordManager(FormTypes.Browse);
                vehicleInfo.AccidentInfo = selectItems[0];
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 500;
                browser.MinHeight = 300;

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void FrmAccidentRecordManager_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("t_oa_accidentrecord");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_accidentrecord");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }
        void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
        }

        private ObservableCollection<T_OA_VEHICLE> vehicleInfoList = null;
        private void vimsClient_GetVehicleInfoListCompleted(object sender, GetVehicleInfoListCompletedEventArgs e)
        {
            vehicleInfoList = e.Result;
        }

        private SmtOACommonAdminClient vehicleClient = new SmtOACommonAdminClient();

        void armSeivice_DeleteAccidentRecordListCompleted(object sender, DeleteAccidentRecordListCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
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
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }

        private List<T_OA_ACCIDENTRECORD> vehicleUseApp = null;
        void armSeivice_GetAccidentRecordListCompleted(object sender, GetAccidentRecordListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_ACCIDENTRECORD> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
            {
                vehicleUseApp = vehicleInfoData.ToList();
                originalNum = vehicleUseApp.Count;
                BindDateGrid(vehicleUseApp);
            }
            else
            {
                originalNum = 0;
                BindDateGrid(null);
            }
            loadbar.Stop();
        }

        private void BindDateGrid(List<T_OA_ACCIDENTRECORD> vehicleList)
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
            
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQuery)
            {

                string StrContent = "";
                string StrStart = "";
                string StrEnd = "";

                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();

                StrContent = this.txtConserVationContent.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //MessageBox.Show("结束时间不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"));
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
                        filter += "ACCIDENTDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "ACCIDENTDATE <=@" + paras.Count().ToString();//结束时间
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

            }
            loadbar.Start();
            vehicleClient.GetAccidentRecordListAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CFrmAccidentRecordManager vehicleInfo = new CFrmAccidentRecordManager(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(vehicleInfo);
            browser.MinWidth = 500;
            browser.MinHeight = 300;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            GetData(dataPager.PageIndex);
        }
        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_ACCIDENTRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                CFrmAccidentRecordManager vehicleInfo = new CFrmAccidentRecordManager(FormTypes.Edit);
                vehicleInfo.AccidentInfo = selectItems[0];
                EntityBrowser browser = new EntityBrowser(vehicleInfo);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 500;
                browser.MinHeight = 300;

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "UPDATE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> accidentrecordId = new ObservableCollection<string>();
            if (dg.SelectedItems.Count > 0)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    accidentrecordId.Add((dg.SelectedItems[i] as T_OA_ACCIDENTRECORD).ACCIDENTRECORDID);
                }
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    vehicleClient.DeleteAccidentRecordListAsync(accidentrecordId);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        private void SetDataForm()
        { }

        private ObservableCollection<T_OA_ACCIDENTRECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_ACCIDENTRECORD> selectList = new ObservableCollection<T_OA_ACCIDENTRECORD>();
                foreach (T_OA_ACCIDENTRECORD obj in dg.SelectedItems)
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

        private List<T_OA_ACCIDENTRECORD> GetSearchDate(T_OA_ACCIDENTRECORD searchInfo, List<T_OA_ACCIDENTRECORD> accidentRecordInfoList)
        {
            if (searchInfo.T_OA_VEHICLE.ASSETID != null)
            {
                accidentRecordInfoList = accidentRecordInfoList.Where(ent => ent.T_OA_VEHICLE.ASSETID == searchInfo.T_OA_VEHICLE.ASSETID).ToList();
            }
            if (searchInfo.ACCIDENTDATE != null && searchInfo.ACCIDENTDATE != new DateTime(1, 1, 1, 0, 0, 0))
            {
                accidentRecordInfoList = accidentRecordInfoList.Where(ent => ent.ACCIDENTDATE == searchInfo.ACCIDENTDATE).ToList();
            }
            if (accidentRecordInfoList != null && accidentRecordInfoList.Count > 0)
            {
                return accidentRecordInfoList;
            }
            else
            {
                return null;
            }
        }
        private int originalNum = 0;
        private ObservableCollection<T_OA_ACCIDENTRECORD> deletedList = new ObservableCollection<T_OA_ACCIDENTRECORD>();//标记被删除的对象
        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            BindDateGrid(vehicleUseApp);
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