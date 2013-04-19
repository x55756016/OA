using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class FrmVehicleUseAppManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        private string StrDepartmentID = "";
        private string StrDepartmentName = "";
        private T_OA_VEHICLEUSEAPP vehicleuseappInfo = null;
        private string checkState = ((int)CheckStates.ALL).ToString();

        public FrmVehicleUseAppManager()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLEAPP", true);

            vehicleManager.AddVehicleUseAppCompleted += new EventHandler<AddVehicleUseAppCompletedEventArgs>(vehicleManager_AddVehicleUseAppCompleted);
            vehicleManager.AddVehicleUseAppListCompleted += new EventHandler<AddVehicleUseAppListCompletedEventArgs>(vehicleManager_AddVehicleUseAppListCompleted);
            vehicleManager.UpdateVehicleUseAppListCompleted += new EventHandler<UpdateVehicleUseAppListCompletedEventArgs>(vehicleManager_UpdateVehicleUseAppListCompleted);
            vehicleManager.GetVehicleUseAppInfoListCompleted += new EventHandler<GetVehicleUseAppInfoListCompletedEventArgs>(vehicleManager_GetVehicleUseAppInfoListCompleted);
            vehicleManager.DeleteVehicleUseAppListCompleted += new EventHandler<DeleteVehicleUseAppListCompletedEventArgs>(vehicleManager_DeleteVehicleUseAppListCompleted);
            #region 原来的
            /*
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); GetData();
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            */
            #endregion
            this.Loaded += new RoutedEventHandler(FrmVehicleUseAppManager_Loaded);
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (dg.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dg.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            T_OA_VEHICLEUSEAPP ent = dg.SelectedItems[0] as T_OA_VEHICLEUSEAPP;

            VehicleUseAppForm form = new VehicleUseAppForm(FormTypes.Resubmit, ent.VEHICLEUSEAPPID);
            EntityBrowser browser = new EntityBrowser(form);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 500;
            browser.MinHeight = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        #endregion

        void FrmVehicleUseAppManager_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); GetData();
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            #endregion
            GetEntityLogo("t_oa_vehicleuseapp");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_vehicleuseapp");
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        #region 审核
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (dg.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dg.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_OA_VEHICLEUSEAPP ent = dg.SelectedItems[0] as T_OA_VEHICLEUSEAPP;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                VehicleUseAppForm AddWin = new VehicleUseAppForm(FormTypes.Audit, ent.VEHICLEUSEAPPID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                return;
            }
        }
        #endregion

        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEUSEAPP> selectItems = GetSelectListView();
            if (selectItems != null)
            {
                vehicleuseappInfo = selectItems.FirstOrDefault();
                VehicleUseAppForm form = new VehicleUseAppForm(FormTypes.Browse, vehicleuseappInfo.VEHICLEUSEAPPID);
                form.VehicleUsrApp = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion

        string stateFlg = "0";
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "OAVEHICLEAPP");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }

        void vehicleManager_DeleteVehicleUseAppListCompleted(object sender, DeleteVehicleUseAppListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData();
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);
        }

        void vehicleManager_GetVehicleUseAppInfoListCompleted(object sender, GetVehicleUseAppInfoListCompletedEventArgs e)
        {
            //StrDepartmentID = "";
            IsQuery = false;
            //DepartmentName.DisplayMemberPath = "";
            ObservableCollection<T_OA_VEHICLEUSEAPP> vehicleInfoData = e.Result;
            List<T_OA_VEHICLEUSEAPP> vehicleUseApp = null;
            if (vehicleInfoData != null)
            {
                vehicleUseApp = vehicleInfoData.ToList();
            }
            BindDateGrid(vehicleUseApp);
            loadbar.Stop();
        }

        void vehicleManager_UpdateVehicleUseAppListCompleted(object sender, UpdateVehicleUseAppListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                Utility.ShowMessageBox("UPDATE", false, true);
                GetData();
            }
            else
            {
                Utility.ShowMessageBox("UPDATE", false, false);
            }
        }

        void vehicleManager_AddVehicleUseAppListCompleted(object sender, AddVehicleUseAppListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                Utility.ShowMessageBox("ADD", false, true);
                GetData();
            }
            else
            {
                Utility.ShowMessageBox("ADD", false, false);
            }
        }

        void vehicleManager_AddVehicleUseAppCompleted(object sender, AddVehicleUseAppCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData();
                Utility.ShowMessageBox("ADD", false, true);
            }
            else
            {
                Utility.ShowMessageBox("ADD", false, false);
            }
        }

        private void BindDateGrid(List<T_OA_VEHICLEUSEAPP> vehicleList)
        {
            if (vehicleList != null)
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
        private void GetData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值      
            if (IsQuery)
            {
                string StrRoute = "";

                string StrStart = "";
                string StrEnd = "";
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrRoute = this.txtRoute.Text.ToString().Trim();
                //StrDepartmentID = this..Text.ToString().Trim();  //add by zl

                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    loadbar.Stop();
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    loadbar.Stop();
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
                        filter += "STARTTIME >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "STARTTIME <=@" + paras.Count().ToString();//结束时间    //modify bu zl
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrRoute))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "ROUTE ^@" + paras.Count().ToString();//路线
                    paras.Add(StrRoute);
                }
                if (!string.IsNullOrEmpty(StrDepartmentID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "DEPARTMENTID ==@" + paras.Count().ToString();//公司ID
                    paras.Add(StrDepartmentID);

                }

            }
            vehicleManager.GetVehicleUseAppInfoListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            VehicleUseAppForm form = new VehicleUseAppForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 500;
            browser.MinHeight = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            GetData();
        }

        void cfvuaManager_OkClick(object sender, EventArgs e)
        {
            GetData();
        }

        #region 修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dg.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            if (dg.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            T_OA_VEHICLEUSEAPP ent = dg.SelectedItems[0] as T_OA_VEHICLEUSEAPP;
            if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                VehicleUseAppForm AddWin = new VehicleUseAppForm(FormTypes.Edit, ent.VEHICLEUSEAPPID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                return;
            }
        }
        #endregion

        private ObservableCollection<T_OA_VEHICLEUSEAPP> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_VEHICLEUSEAPP> selectList = new ObservableCollection<T_OA_VEHICLEUSEAPP>();
                foreach (T_OA_VEHICLEUSEAPP obj in dg.SelectedItems)
                {
                    if (obj.CHECKSTATE == "0" || obj.CHECKSTATE == "3")
                    {
                        selectList.Add(obj);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                        break;
                    }
                }
                if (selectList != null && selectList.Count > 0)
                {
                    return selectList;
                }
            }
            return null;
        }

        private ObservableCollection<T_OA_VEHICLEUSEAPP> GetSelectListView()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_VEHICLEUSEAPP> selectList = new ObservableCollection<T_OA_VEHICLEUSEAPP>();
                foreach (T_OA_VEHICLEUSEAPP obj in dg.SelectedItems)
                {
                    selectList.Add(obj);
                }
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

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEUSEAPP> selectItems = GetSelectList();
            if (selectItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    vehicleuseappInfo = selectItems[i];
                    if (vehicleuseappInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try
                            {
                                vehicleManager.DeleteVehicleUseAppListAsync(selectItems);
                            }
                            catch
                            {

                            }
                        };
                        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"));
                        return;
                    }
                }
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        #region 查询按钮


        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQuery = true;
            GetData();
        }
        #endregion

        #region 选择部门
        private void btnLookDepartmentName_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    txtDepartmentName.DataContext = companyInfo;
                    StrDepartmentID = companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion
    }
}