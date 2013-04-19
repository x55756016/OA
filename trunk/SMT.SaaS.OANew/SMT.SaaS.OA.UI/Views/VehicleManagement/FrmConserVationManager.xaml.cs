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
    public partial class FrmConserVationManager : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private SmtOACommonAdminClient cvmWS = new SmtOACommonAdminClient();
        private T_OA_CONSERVATION selApporvalInfo = null;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private bool IsQuery = false;
        public FrmConserVationManager()
        {
            InitializeComponent();
            #region 原来的
            /*
            Utility.DisplayGridToolBarButton(ToolBar, "OACONSERVATION", true);

            cvmWS.GetConserVationListCompleted += new EventHandler<GetConserVationListCompletedEventArgs>(cvmWS_GetConserVationListCompleted);
            cvmWS.DeleteConserVationListCompleted += new EventHandler<DeleteConserVationListCompletedEventArgs>(cvmWS_DeleteConserVationListCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            PARENT.Children.Add(loadbar);
            */
            #endregion
            this.Loaded += new RoutedEventHandler(FrmConserVationManager_Loaded);
            //GetData();
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CONSERVATION> selectItems = GetSelectList();
            if (selectItems != null)
            {
                ConserVationForm form = new ConserVationForm(FormTypes.Resubmit);
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

        void FrmConserVationManager_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            Utility.DisplayGridToolBarButton(ToolBar, "OACONSERVATION", true);

            cvmWS.GetConserVationListCompleted += new EventHandler<GetConserVationListCompletedEventArgs>(cvmWS_GetConserVationListCompleted);
            cvmWS.DeleteConserVationListCompleted += new EventHandler<DeleteConserVationListCompletedEventArgs>(cvmWS_DeleteConserVationListCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            PARENT.Children.Add(loadbar);
            #endregion
            GetEntityLogo("t_oa_conservation");
            Utility.CbxItemBinder(cmbConserVationName, "CONSERVANAME", "0");
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "t_oa_conservation");

        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
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

            T_OA_CONSERVATION ent = dg.SelectedItems[0] as T_OA_CONSERVATION;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                ConserVationForm_aud form = new ConserVationForm_aud(FormTypes.Audit, ent.CONSERVATIONID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 750;
                browser.MinHeight = 420;
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
            ObservableCollection<T_OA_CONSERVATION> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                selApporvalInfo = selectInfoList.FirstOrDefault();
                ConserVationForm_aud form = new ConserVationForm_aud(FormTypes.Browse, selApporvalInfo.CONSERVATIONID);
                form.ConserVation = selectInfoList[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 750;
                browser.MinHeight = 420;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "VIEW"));
            }
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "OACONSERVATION");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }

        void cvmWS_DeleteConserVationListCompleted(object sender, DeleteConserVationListCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
        }

        void cvmWS_GetConserVationListCompleted(object sender, GetConserVationListCompletedEventArgs e)
        {
            loadbar.Stop();
            ObservableCollection<T_OA_CONSERVATION> dataList = e.Result;
            if (dataList != null)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList.ToList());
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
            {
                dataPager.DataContext = null;
                dg.ItemsSource = null;
            }

        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ConserVationForm form = new ConserVationForm(FormTypes.New);
            form.EditState = "add";
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 750;
            browser.MinHeight = 430;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        //子窗口事件
        void browser_ReloadDataEvent()
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
                string StrVin = "";
                string StrContent = "";
                string StrStart = "";
                string StrEnd = "";
                string StrConservateType = ((SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY)cmbConserVationName.SelectedItem).DICTIONARYNAME.ToString();
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrVin = this.txtVIN.Text.ToString().Trim();
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

                if (!string.IsNullOrEmpty(StrVin))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "T_OA_VEHICLE.VIN ^@" + paras.Count().ToString();//车牌号
                    paras.Add(StrVin);
                }
                if (!string.IsNullOrEmpty(StrContent))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CONTENT ^@" + paras.Count().ToString();//保养项目
                    paras.Add(StrContent);
                }
                if (!string.IsNullOrEmpty(StrConservateType) && StrConservateType != "所有")
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CONSERVATYPE ^@" + paras.Count().ToString();//保养类型
                    paras.Add(StrConservateType);
                }
            }
            cvmWS.GetConserVationListAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }
        //编辑
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CONSERVATION> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                selApporvalInfo = selectInfoList[0];
                if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                {
                    ConserVationForm form = new ConserVationForm(FormTypes.Edit);
                    form.EditState = "update";
                    form.ConserVation = selectInfoList[0];
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinWidth = 750;
                    browser.MinHeight = 420;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                    return;
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "UPDATE"));
            }
        }

        void ccvManager_OkClick(object sender, EventArgs e)
        {
            GetData();
        }

        private ObservableCollection<T_OA_CONSERVATION> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_CONSERVATION> selectList = new ObservableCollection<T_OA_CONSERVATION>();
                foreach (T_OA_CONSERVATION obj in dg.SelectedItems)
                    selectList.Add(obj);
                if (selectList != null && selectList.Count > 0)
                {
                    return selectList;
                }
            }
            return null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            selectDeleteList = GetSelectList();
            if (selectDeleteList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            for (int i = 0; i < dg.SelectedItems.Count; i++)
            {
                selApporvalInfo = selectDeleteList[i];
                if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {

                        if (selectDeleteList != null)
                        {
                            try
                            {
                                cvmWS.DeleteConserVationListAsync(selectDeleteList);
                            }
                            catch
                            {

                            }
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
        private ObservableCollection<T_OA_CONSERVATION> selectDeleteList = null;

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
        //分页
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
