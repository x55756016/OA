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
    public partial class FrmVehicleDispatchManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_VEHICLEDISPATCH vehicledispatchInfo = null;
        private bool IsQuery = false;
        private string checkState = ((int)CheckStates.ALL).ToString();
        public FrmVehicleDispatchManager()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAVEHICLEDISPATCH", true);
            this.Loaded += new RoutedEventHandler(FrmVehicleDispatchManager_Loaded);
            vehicleDispatchManager.AddVehicleDispatchListCompleted += new EventHandler<AddVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_AddVehicleDispatchListCompleted);
            vehicleDispatchManager.Gets_VDInfoCompleted += new EventHandler<Gets_VDInfoCompletedEventArgs>(Gets_VDInfoCompleted);
            vehicleDispatchManager.UpdateVehicleDispatchListCompleted += new EventHandler<UpdateVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_UpdateVehicleDispatchListCompleted);
            vehicleDispatchManager.DeleteVehicleDispatchListCompleted += new EventHandler<DeleteVehicleDispatchListCompletedEventArgs>(vehicleDispatchManager_DeleteVehicleDispatchListCompleted);
            #region 原来的
            /*
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

            T_OA_VEHICLEDISPATCH ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCH;

            VehicleDispatchForm_upd form = new VehicleDispatchForm_upd(FormTypes.Resubmit, ent.VEHICLEDISPATCHID);
            EntityBrowser browser = new EntityBrowser(form);
            browser.FormType = FormTypes.Resubmit;
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

        }
        #endregion

        void FrmVehicleDispatchManager_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
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
            GetEntityLogo("t_oa_vehicledispatch");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_vehicledispatch");
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

            T_OA_VEHICLEDISPATCH ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCH;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                VehicleDispatchForm_aud form = new VehicleDispatchForm_aud(FormTypes.Audit, ent.VEHICLEDISPATCHID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
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
        #endregion

        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCH> selectItems = GetSelectList();

            T_OA_VEHICLEDISPATCH ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCH;
            if (ent != null)
            {
                //vehicledispatchInfo = selectItems.FirstOrDefault();
                //VehicleDispatchForm_aud form1 = new VehicleDispatchForm_aud(FormTypes.Browse, vehicledispatchInfo.VEHICLEDISPATCHID);
                VehicleDispatchForm_aud form = new VehicleDispatchForm_aud(FormTypes.Browse, ent.VEHICLEDISPATCHID);
                //form.VehicleDispatch = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        #endregion

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "OAVEHICLEDISPATCH");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }

        private void vehicleDispatchManager_DeleteVehicleDispatchListCompleted(object sender, DeleteVehicleDispatchListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                deletedList.Clear();
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
            }
        }

        private void vehicleDispatchManager_UpdateVehicleDispatchListCompleted(object sender, UpdateVehicleDispatchListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", ""));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("UPDATEFAILED", ""));
            }
        }


        void vehicleDispatchManager_AddVehicleDispatchListCompleted(object sender, AddVehicleDispatchListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDSUCCESSED", ""));
            }
        }
        private void BindDateGrid(List<T_OA_VEHICLEDISPATCH> vehicleList)
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
            vehicleDispatchManager.Gets_VDInfoAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        //获取派车单数据 2
        private void Gets_VDInfoCompleted(object sender, Gets_VDInfoCompletedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCH> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
            {
                List<T_OA_VEHICLEDISPATCH> vehicleUseApp = vehicleInfoData.ToList();
                BindDateGrid(vehicleUseApp);
            }
            else
                BindDateGrid(null);
            loadbar.Stop();
        }

        //新建
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            VehicleDispatchForm_add form = new VehicleDispatchForm_add(FormTypes.New);
            
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 550;
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

            T_OA_VEHICLEDISPATCH ent = dg.SelectedItems[0] as T_OA_VEHICLEDISPATCH;
            if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                VehicleDispatchForm_upd form = new VehicleDispatchForm_upd(FormTypes.Edit, ent.VEHICLEDISPATCHID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                browser.MinHeight = 550;
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

        private void SetDataForm()
        { }

        private ObservableCollection<T_OA_VEHICLEDISPATCH> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_VEHICLEDISPATCH> selectList = new ObservableCollection<T_OA_VEHICLEDISPATCH>();
                foreach (T_OA_VEHICLEDISPATCH obj in dg.SelectedItems)
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
        private ObservableCollection<T_OA_VEHICLEDISPATCH> deletedList = new ObservableCollection<T_OA_VEHICLEDISPATCH>();//标记被删除的对象

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_VEHICLEDISPATCH> selectItems = GetSelectList();
            if (selectItems != null)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    vehicledispatchInfo = selectItems[i];
                    if (vehicledispatchInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try
                            {
                                vehicleDispatchManager.DeleteVehicleDispatchListAsync(selectItems);
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