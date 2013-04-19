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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.VehicleManagement
{
    public partial class ConserVationRecord : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private SmtOACommonAdminClient _VM = new SmtOACommonAdminClient();
        private string checkState = ((int)CheckStates.ALL).ToString();
        private bool IsQuery = false;
        private T_OA_CONSERVATIONRECORD conservationrecordInfo = null;

        public ConserVationRecord()
        {
            InitializeComponent();
            #region 原来的
            /*
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_CONSERVATIONRECORD", true);

            _VM.Get_VCRecordsCompleted += new EventHandler<Get_VCRecordsCompletedEventArgs>(Get_VCRecordsCompleted);
            _VM.Del_VCRecordsCompleted += new EventHandler<Del_VCRecordsCompletedEventArgs>(Del_VCRecordsCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            PARENT.Children.Add(loadbar); GetData(dataPager.PageIndex);
            */
            #endregion
            this.Loaded += new RoutedEventHandler(ConserVationRecord_Loaded);
        }

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CONSERVATIONRECORD> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                conservationrecordInfo = selectInfoList.FirstOrDefault();
                ConserVationRecord_upd form = new ConserVationRecord_upd(FormTypes.Resubmit, conservationrecordInfo.CONSERVATIONRECORDID);
                form.ConserVation = selectInfoList[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 750;
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion

        void ConserVationRecord_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
           
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_CONSERVATIONRECORD", true);

            _VM.Get_VCRecordsCompleted += new EventHandler<Get_VCRecordsCompletedEventArgs>(Get_VCRecordsCompleted);
            _VM.Del_VCRecordsCompleted += new EventHandler<Del_VCRecordsCompletedEventArgs>(Del_VCRecordsCompleted);
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
            GetData(dataPager.PageIndex);
        
            #endregion
            GetEntityLogo("T_OA_CONSERVATIONRECORD");
            Utility.CbxItemBinder(cmbConserVationName, "CONSERVANAME", "0");
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_CONSERVATIONRECORD");

        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex);
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

            T_OA_CONSERVATIONRECORD ent = dg.SelectedItems[0] as T_OA_CONSERVATIONRECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                ConserVationRecord_aud form = new ConserVationRecord_aud(FormTypes.Audit, ent.CONSERVATIONRECORDID);
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
            ObservableCollection<T_OA_CONSERVATIONRECORD> selectInfoList = GetSelectList();
            if (selectInfoList != null && selectInfoList.Count > 0)
            {
                conservationrecordInfo = selectInfoList.FirstOrDefault();
                ConserVationRecord_aud form = new ConserVationRecord_aud(FormTypes.Browse, conservationrecordInfo.CONSERVATIONRECORDID);
                form.ConserVation = selectInfoList[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 750;
                browser.MinHeight = 550;
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
                checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_CONSERVATIONRECORD");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData(dataPager.PageIndex);
            }
            //checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
            //SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(Convert.ToInt32(checkState), ToolBar, "T_OA_CONSERVATIONRECORD");
            //GetData(dataPager.PageIndex, checkState);
        }

        void Del_VCRecordsCompleted(object sender, Del_VCRecordsCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                GetData(dataPager.PageIndex);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
        }

        void Get_VCRecordsCompleted(object sender, Get_VCRecordsCompletedEventArgs e)
        {
            IsQuery = true;
            ObservableCollection<T_OA_CONSERVATIONRECORD> dataList = e.Result;
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
            loadbar.Stop();
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ConserVationRecord_add form = new ConserVationRecord_add(FormTypes.New);
            form.EditState = "add";
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinWidth = 750;
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        //子窗口事件
        void browser_ReloadDataEvent()
        {
            GetData(dataPager.PageIndex);
        }
        //加载数据
        private void GetData(int pageIndex)
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
                    filter += "CONSERVATYPE ==@" + paras.Count().ToString();//类型
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

            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            _VM.Get_VCRecordsAsync(pageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, checkState, loginUserInfo);
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

            T_OA_CONSERVATIONRECORD ent = dg.SelectedItems[0] as T_OA_CONSERVATIONRECORD;
            if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                ConserVationRecord_upd form = new ConserVationRecord_upd(FormTypes.Edit, ent.CONSERVATIONRECORDID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 750;
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

        void ccvManager_OkClick(object sender, EventArgs e)
        {
            GetData(dataPager.PageIndex);
        }

        private ObservableCollection<T_OA_CONSERVATIONRECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_CONSERVATIONRECORD> selectList = new ObservableCollection<T_OA_CONSERVATIONRECORD>();
                foreach (T_OA_CONSERVATIONRECORD obj in dg.SelectedItems)
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
            if (selectDeleteList != null && selectDeleteList.Count > 0)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    conservationrecordInfo = selectDeleteList[i];
                    if (conservationrecordInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow com = new SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            if (selectDeleteList != null)
                            {
                                try
                                {_VM.Del_VCRecordsAsync(selectDeleteList);}
                                catch
                                { }
                            }
                        };
                        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), SMT.SaaS.FrameworkUI.ChildWidow.ComfirmWindow.titlename, Result);
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
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("SELECTWARNING", "DELETE"));
            }
        }
        private ObservableCollection<T_OA_CONSERVATIONRECORD> selectDeleteList = null;

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
