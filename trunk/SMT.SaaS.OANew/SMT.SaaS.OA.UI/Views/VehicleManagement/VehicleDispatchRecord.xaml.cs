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
    public partial class VehicleDispatchRecord : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private bool IsQuery = false;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_VEHICLEDISPATCHRECORD vehicledispatchrecord = null;

        public VehicleDispatchRecord()
        {
            InitializeComponent();
            #region 原来的
            /*
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_VEHICLEDISPATCHRECORD", true);

            vehicleDispatchManager.AddVehicleDispatchListCompleted += new EventHandler<AddVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_AddVehicleDispatchListCompleted);
            vehicleDispatchManager.Gets_VDRecordCompleted += new EventHandler<Gets_VDRecordCompletedEventArgs>(Gets_VDRecordCompleted);
            vehicleDispatchManager.UpdateVehicleDispatchListCompleted += new EventHandler<UpdateVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_UpdateVehicleDispatchListCompleted);
            vehicleDispatchManager.Del_VDRecordCompleted += new EventHandler<Del_VDRecordCompletedEventArgs>(Del_VDRecordCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            */
            #endregion
            this.Loaded += new RoutedEventHandler(VehicleDispatchRecord_Loaded);
            //GetData();
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                vehicledispatchrecord = selectItems.FirstOrDefault();
                VehicleDispatchRecord_upd form = new VehicleDispatchRecord_upd(FormTypes.Resubmit, vehicledispatchrecord.VEHICLEDISPATCHRECORDID);
                form.VehicleDispatchRecord = selectItems[0];
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

        void VehicleDispatchRecord_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_VEHICLEDISPATCHRECORD", true);

            vehicleDispatchManager.AddVehicleDispatchListCompleted += new EventHandler<AddVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_AddVehicleDispatchListCompleted);
            vehicleDispatchManager.Gets_VDRecordCompleted += new EventHandler<Gets_VDRecordCompletedEventArgs>(Gets_VDRecordCompleted);
            vehicleDispatchManager.UpdateVehicleDispatchListCompleted += new EventHandler<UpdateVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_UpdateVehicleDispatchListCompleted);
            vehicleDispatchManager.Del_VDRecordCompleted += new EventHandler<Del_VDRecordCompletedEventArgs>(Del_VDRecordCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            #endregion
            GetEntityLogo("T_OA_VEHICLEDISPATCHRECORD");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_VEHICLEDISPATCHRECORD");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

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

            T_OA_VEHICLEDISPATCHRECORD ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCHRECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                VehicleDispatchRecord_aud form = new VehicleDispatchRecord_aud(FormTypes.Audit, ent.VEHICLEDISPATCHRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 750;
                browser.MinHeight = 600;
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
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> selectItems = GetSelectList();
            if (selectItems != null)
            {
                vehicledispatchrecord = selectItems.FirstOrDefault();
                VehicleDispatchRecord_aud form = new VehicleDispatchRecord_aud(FormTypes.Browse, vehicledispatchrecord.VEHICLEDISPATCHRECORDID);
                form.VehicleDispatchRecord = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 750;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_VEHICLEDISPATCHRECORD");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }

        private void Del_VDRecordCompleted(object sender, Del_VDRecordCompletedEventArgs e)
        {
            int n = e.Result;
            if (n > 0)
                deletedList.Clear();
            PopuMsg(n, n > 0 ? "DELETESUCCESSED" : "DELETEFAILED");
        }
        private void vehicleDispatchManager_UpdateVehicleDispatchListCompleted(object sender, UpdateVehicleDispatchListCompletedEventArgs e)
        {
            int n = e.Result;
            PopuMsg(n, n > 0 ? "UPDATESUCCESSED" : "UPDATEFAILED");
        }

        void vehicleDispatchManager_AddVehicleDispatchListCompleted(object sender, AddVehicleDispatchListCompletedEventArgs e)
        {
            int n = e.Result;
            PopuMsg(n, n > 0 ? "ADDSUCCESSED" : "ADDSUCCESSED");
        }
        private void BindDateGrid(List<T_OA_VEHICLEDISPATCHRECORD> vehicleList)
        {
            if (vehicleList != null && vehicleList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(vehicleList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
                dg.ItemsSource = null;
        }
        /// <summary>
        /// 提示框
        /// </summary>
        /// <param name="n"></param>
        /// <param name="tip"></param>
        private void PopuMsg(int n, string tip)
        {
            if (n > 0)
            {
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr(tip, ""));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(tip, ""));
        }
        /// <summary>
        /// 获取派车单数据 1
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="checkState"></param>
        private void GetData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值 
            if (IsQuery)
            {
                string StrTitle = "";
                string StrStart = "";
                string StrEnd = "";
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrTitle = this.txtRoute.Text.ToString().Trim();

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
                        filter += "STARTTIME >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "STARTTIME <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "ROUTE ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrTitle);
                }

            }
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            vehicleDispatchManager.Gets_VDRecordAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        //获取派车单数据 2
        private void Gets_VDRecordCompleted(object sender, Gets_VDRecordCompletedEventArgs e)
        {
            IsQuery = false;
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
            {
                List<T_OA_VEHICLEDISPATCHRECORD> vehicleUseApp = vehicleInfoData.ToList();
                BindDateGrid(vehicleUseApp);
            }
            else
                BindDateGrid(null);
            loadbar.Stop();
        }

        //新建
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            VehicleDispatchRecord_add form = new VehicleDispatchRecord_add(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 750;
            browser.MinHeight = 600;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            GetData();
        }

        void cfm_OkClick(object sender, EventArgs e)
        {
            GetData();
        }
        //编辑
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

            T_OA_VEHICLEDISPATCHRECORD ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCHRECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                VehicleDispatchRecord_upd form = new VehicleDispatchRecord_upd(FormTypes.Edit, ent.VEHICLEDISPATCHRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 750;
                browser.MinHeight = 600;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                return;
            }
        }

        private void SetDataForm()
        { }

        private ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> selectList = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();
                foreach (T_OA_VEHICLEDISPATCHRECORD obj in dg.SelectedItems)
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
        private SmtOACommonAdminClient vehicleDispatchManager = new SmtOACommonAdminClient();
        private ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> deletedList = new ObservableCollection<T_OA_VEHICLEDISPATCHRECORD>();//标记被删除的对象

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCHRECORD> selectItems = GetSelectList();
            ObservableCollection<string> o = new ObservableCollection<string>();
            if (selectItems != null)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    vehicledispatchrecord = selectItems[i];
                    if (vehicledispatchrecord.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try
                            {
                                foreach (T_OA_VEHICLEDISPATCHRECORD info in selectItems)
                                    o.Add(info.VEHICLEDISPATCHRECORDID);
                                vehicleDispatchManager.Del_VDRecordAsync(o);
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
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

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
    }
}