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
    public partial class FrmMaintenanceAppManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_MAINTENANCEAPP selApporvalInfo = null;
        public FrmMaintenanceAppManager()
        {
            InitializeComponent();
            #region 原来的
            /*
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLEMAINTENANCE", true);

            maWS.GetMaintenanceAppListCompleted += new EventHandler<GetMaintenanceAppListCompletedEventArgs>(GetMaintenanceAppListCompleted);
            maWS.DeleteMaintenanceAppListCompleted += new EventHandler<DeleteMaintenanceAppListCompletedEventArgs>(DeleteMaintenanceAppListCompleted);
           
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
            */
            #endregion
            
            //GetData();
            this.Loaded += new RoutedEventHandler(FrmMaintenanceAppManager_Loaded);
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCEAPP> selectItems = GetSelectList();
            if (selectItems != null)
            {
                MaintenanceAppForm form = new MaintenanceAppForm(FormTypes.Resubmit);
                form.ConserVation = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 750;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion

        void FrmMaintenanceAppManager_Loaded(object sender, RoutedEventArgs e)
        {
            #region 原来的
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLEMAINTENANCE", true);

            maWS.GetMaintenanceAppListCompleted += new EventHandler<GetMaintenanceAppListCompletedEventArgs>(GetMaintenanceAppListCompleted);
            maWS.DeleteMaintenanceAppListCompleted += new EventHandler<DeleteMaintenanceAppListCompletedEventArgs>(DeleteMaintenanceAppListCompleted);

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
            #endregion

            GetEntityLogo("t_oa_maintenanceapp");
            Utility.CbxItemBinder(cmbConserVationName, "MAINTENANCENAME", "0");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_maintenanceapp");

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

            T_OA_MAINTENANCEAPP ent = dg.SelectedItems[0] as T_OA_MAINTENANCEAPP;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                MaintenanceAppForm_aud form = new MaintenanceAppForm_aud(FormTypes.Audit, ent.MAINTENANCEAPPID);
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
            ObservableCollection<T_OA_MAINTENANCEAPP> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                selApporvalInfo = selectInfoList.FirstOrDefault();
                MaintenanceAppForm_aud form = new MaintenanceAppForm_aud(FormTypes.Browse, selApporvalInfo.MAINTENANCEAPPID);
                form.ConserVation = selectInfoList[0];
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
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "OAVEHICLEMAINTENANCE");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }
        // 删除
        void DeleteMaintenanceAppListCompleted(object sender, DeleteMaintenanceAppListCompletedEventArgs e)
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
        void GetMaintenanceAppListCompleted(object sender, GetMaintenanceAppListCompletedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCEAPP> dataList = e.Result;
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
        private SmtOACommonAdminClient maWS = new SmtOACommonAdminClient();
        //新建
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            MaintenanceAppForm form = new MaintenanceAppForm(FormTypes.New);
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

            maWS.GetMaintenanceAppListAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }
        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCEAPP> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                selApporvalInfo = selectInfoList.FirstOrDefault();
                selApporvalInfo = selectInfoList[0];
                if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                {
                    MaintenanceAppForm form = new MaintenanceAppForm(FormTypes.Edit);
                    form.EditState = "update";
                    form.ConserVation = selectInfoList[0];
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
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "UPDATE"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        //删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_MAINTENANCEAPP> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    selApporvalInfo = selectInfoList[i];
                    if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try { maWS.DeleteMaintenanceAppListAsync(selectInfoList); }
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

        private ObservableCollection<T_OA_MAINTENANCEAPP> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_MAINTENANCEAPP> selectList = new ObservableCollection<T_OA_MAINTENANCEAPP>();
                foreach (T_OA_MAINTENANCEAPP obj in dg.SelectedItems)
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