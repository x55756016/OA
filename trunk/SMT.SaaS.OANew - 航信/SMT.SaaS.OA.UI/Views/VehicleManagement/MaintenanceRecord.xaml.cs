using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class MaintenanceRecord : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private SmtOACommonAdminClient maWS = new SmtOACommonAdminClient();
        private bool IsQuery = false;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_MAINTENANCERECORD maintenancerecord = null;

        public MaintenanceRecord()
        {
            InitializeComponent();
            #region 原来的
            /*
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_MAINTENANCERECORD", true);

            maWS.Get_VMRecordsCompleted += new EventHandler<Get_VMRecordsCompletedEventArgs>(Get_VMRecordsCompleted);
            maWS.Del_VMRecordCompleted += new EventHandler<Del_VMRecordCompletedEventArgs>(Del_VMRecordCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            PARENT.Children.Add(loadbar);
            GetData();
            */
            #endregion
            this.Loaded += new RoutedEventHandler(MaintenanceRecord_Loaded);
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCERECORD> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                maintenancerecord = selectInfoList.FirstOrDefault();
                MaintenanceRecordForm_upd form = new MaintenanceRecordForm_upd(FormTypes.Resubmit, maintenancerecord.MAINTENANCERECORDID);
                form.ConserVation = selectInfoList[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 750;
                browser.MinHeight = 550;

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion

        void MaintenanceRecord_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_MAINTENANCERECORD", true);

            maWS.Get_VMRecordsCompleted += new EventHandler<Get_VMRecordsCompletedEventArgs>(Get_VMRecordsCompleted);
            maWS.Del_VMRecordCompleted += new EventHandler<Del_VMRecordCompletedEventArgs>(Del_VMRecordCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            PARENT.Children.Add(loadbar);
            GetData();
            #endregion
            GetEntityLogo("T_OA_MAINTENANCERECORD");
            Utility.CbxItemBinder(cmbConserVationName, "MAINTENANCENAME", "5");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_MAINTENANCERECORD");

        }
        //刷新
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        //分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        //审核
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

            T_OA_MAINTENANCERECORD ent = dg.SelectedItems[0] as T_OA_MAINTENANCERECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                MaintenanceRecordForm_aud form = new MaintenanceRecordForm_aud(FormTypes.Audit, ent.MAINTENANCERECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 750;
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                return;
            }
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCERECORD> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                maintenancerecord = selectInfoList.FirstOrDefault();
                MaintenanceRecordForm_aud form = new MaintenanceRecordForm_aud(FormTypes.Browse, maintenancerecord.MAINTENANCERECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 750;
                browser.MinHeight = 550;

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_MAINTENANCERECORD");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }
        //删除
        void Del_VMRecordCompleted(object sender, Del_VMRecordCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                GetData();
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
                Utility.ShowMessageBox("DELETE", false, false);
        }
        //加载数据
        void Get_VMRecordsCompleted(object sender, Get_VMRecordsCompletedEventArgs e)
        {
            IsQuery = true;
            ObservableCollection<T_OA_MAINTENANCERECORD> dataList = e.Result;
            if (dataList != null)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
            {
                dataPager.DataContext = null;
                dg.ItemsSource = null;
            }
            loadbar.Stop();
        }
        //新建
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MaintenanceRecordForm_add form = new MaintenanceRecordForm_add(FormTypes.New);
            form.EditState = "add";
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 750;
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        //子窗口事件
        void browser_ReloadDataEvent()
        {
            GetData();
        }

        void ccvManager_OkClick(object sender, EventArgs e)
        {
            GetData();
        }
        //加载数据
        private void GetData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (IsQuery)
            {

                string StrContent = "";
                string StrStart = "";
                string StrEnd = "";
                string StrConservateType = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbConserVationName.SelectedItem).DICTIONARYNAME.ToString();
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
                        filter += "REPAIRDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "REPAIRDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }
                if (cmbConserVationName.SelectedIndex > 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "MAINTENANCETYPE ==@" + paras.Count().ToString();//类型
                    paras.Add(StrConservateType);
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

            maWS.Get_VMRecordsAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }
        //修改
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

            T_OA_MAINTENANCERECORD ent = dg.SelectedItems[0] as T_OA_MAINTENANCERECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                MaintenanceRecordForm_upd form = new MaintenanceRecordForm_upd(FormTypes.Edit, ent.MAINTENANCERECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 750;
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                return;
            }
        }
        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCERECORD> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    maintenancerecord = selectInfoList[i];
                    if (maintenancerecord.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try { maWS.Del_VMRecordAsync(selectInfoList); }
                            catch { }
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
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }

        private ObservableCollection<T_OA_MAINTENANCERECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_MAINTENANCERECORD> selectList = new ObservableCollection<T_OA_MAINTENANCERECORD>();
                foreach (T_OA_MAINTENANCERECORD obj in dg.SelectedItems)
                    selectList.Add(obj);
                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }

        #region 查询按钮

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            IsQuery = true;
            GetData();
        }
        #endregion
    }
}