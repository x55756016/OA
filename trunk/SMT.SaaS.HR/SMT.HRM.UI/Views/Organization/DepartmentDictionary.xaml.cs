using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;

using SMT.HRM.UI.Form.Organization;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class DepartmentDictionary : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient svc;
        public string Checkstate { get; set; }
        public DepartmentDictionary()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
                DepartmentDictionary_Loaded(sender, args);
                GetEntityLogo("T_HR_DEPARTMENTDICTIONARY");
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region 获取数据及初始化事件
        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            TextBox txtDepID = Utility.FindChildControl<TextBox>(expander, "txtDepCode");
            if (txtDepID != null)
            {
                if (!string.IsNullOrEmpty(txtDepID.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //filter += "DEPARTMENTCODE==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(DEPARTMENTCODE)";
                    paras.Add(txtDepID.Text.Trim());
                }
            }
            TextBox txtDepName = Utility.FindChildControl<TextBox>(expander, "txtDepName");
            if (txtDepName != null)
            {
                if (!string.IsNullOrEmpty(txtDepName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //  filter += "DEPARTMENTNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(DEPARTMENTNAME)";
                    paras.Add(txtDepName.Text.Trim());
                }
            }
            ComboBox txtDepType = Utility.FindChildControl<ComboBox>(expander, "cbxDepType");
            if (txtDepType != null)
            {
                if (txtDepType.SelectedIndex > 0)
                {
                    if (!string.IsNullOrEmpty((txtDepType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYID))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "DEPARTMENTTYPE==@" + paras.Count().ToString();
                        // paras.Add(txtDepType.SelectedValue.Trim());
                        paras.Add((txtDepType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE.ToString());
                    }
                }
            }
            svc.DepartmentDictionaryPagingAsync(dataPager.PageIndex, dataPager.PageSize, "DEPARTMENTCODE", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, Checkstate);
        }

        private void InitControlEvent()
        {
            PARENT.Children.Add(loadbar);
            //ComboBox cbx =  this.expander.FindName("cbxDepType") as ComboBox;
            ComboBox cbx = Utility.FindChildControl<ComboBox>(this.expander);
            if (cbx != null)
            {
                cbx.Loaded += new RoutedEventHandler(cbxDepType_Loaded);
            }
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            //ToolBar.retAudit.Visibility = Visibility.Collapsed;
            //ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            //ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //this.Loaded += new RoutedEventHandler(DepartmentDictionary_Loaded);

            svc = new OrganizationServiceClient();
            svc.DepartmentDictionaryPagingCompleted += new EventHandler<DepartmentDictionaryPagingCompletedEventArgs>(svc_DepartmentDictionaryPagingCompleted);
            svc.DepartmentDictionaryDeleteCompleted += new EventHandler<DepartmentDictionaryDeleteCompletedEventArgs>(svc_DepartmentDictionaryDeleteCompleted);
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_DEPARTMENTDICTIONARY");
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                dataPager.PageIndex = 1;
                LoadData();
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            dataPager.PageIndex = 1;
            LoadData();
        }

        void DepartmentDictionary_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            //GetEntityLogo("T_HR_DEPARTMENTDICTIONARY");
            #endregion
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_DEPARTMENTDICTIONARY", true);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        void svc_DepartmentDictionaryPagingCompleted(object sender, DepartmentDictionaryPagingCompletedEventArgs e)
        {
            List<T_HR_DEPARTMENTDICTIONARY> list = new List<T_HR_DEPARTMENTDICTIONARY>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }
        #endregion

        #region 添加，修改，删除，查询
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_DEPARTMENTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_DEPARTMENTDICTIONARY;
                if (tmpEnt.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }

                DepartmentDictionaryForms form = new DepartmentDictionaryForms(FormTypes.Edit, tmpEnt.DEPARTMENTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 500;
                browser.FormType = FormTypes.Edit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                //  ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_DEPARTMENTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_DEPARTMENTDICTIONARY;
                DepartmentDictionaryForms form = new DepartmentDictionaryForms(FormTypes.Browse, tmpEnt.DEPARTMENTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 500;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            }
        }
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            DepartmentDictionaryForms form = new DepartmentDictionaryForms(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 500;
            browser.FormType = FormTypes.New;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_DEPARTMENTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_DEPARTMENTDICTIONARY;

                //if (tmpEnt.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPEATAUDITERROR"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}

                DepartmentDictionaryForms form = new DepartmentDictionaryForms(FormTypes.Resubmit, tmpEnt.DEPARTMENTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 500;
                browser.FormType = FormTypes.Resubmit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_DEPARTMENTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_DEPARTMENTDICTIONARY;
                DepartmentDictionaryForms form = new DepartmentDictionaryForms(FormTypes.Audit, tmpEnt.DEPARTMENTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 500;
                browser.FormType = FormTypes.Audit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            }
        }
        void browser_ReloadDataEvent()
        {
            //dataPager.PageIndex = 1;
            LoadData();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            string Result = "";
            string strMsg = string.Empty;
            if (DtGrid.SelectedItems.Count > 0)
            {
                //判断是否可以删除
                ObservableCollection<string> ids = new ObservableCollection<string>();
                foreach (T_HR_DEPARTMENTDICTIONARY tmp in DtGrid.SelectedItems)
                {
                    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                        return;
                    }
                    ids.Add(tmp.DEPARTMENTDICTIONARYID);
                }

                //提示是否删除
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    svc.DepartmentDictionaryDeleteAsync(ids, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void svc_DepartmentDictionaryDeleteCompleted(object sender, DepartmentDictionaryDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg),
                    //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "DEPARTMENTDICTIONARY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_DEPARTMENTDICTIONARY");
        }

        private void cbxDepType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DictionaryComboBox txtDepType = Utility.FindChildControl<DictionaryComboBox>(expander, "cbxDepType");
            //T_SYS_DICTIONARY ent = ((System.Windows.Controls.Primitives.Selector)(sender)).SelectedItem as T_SYS_DICTIONARY;
            // txtDepType.SelectedValue = ent.DICTIONARYVALUE.GetValueOrDefault().ToString();
            dataPager.PageIndex = 1;
            LoadData();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            svc.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void cbxDepType_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbxDepartmentType = sender as ComboBox;
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
            dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE").OrderBy(s => s.DICTIONARYVALUE).ToList();

            T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
            string dictname = Utility.GetResourceStr("PLEASESELECT", "DEPARTMENTTYPE");
            nuldict.DICTIONARYNAME = dictname;
            nuldict.DICTIONARYVALUE = -3;
            dicts.Insert(0, nuldict);

            cbxDepartmentType.ItemsSource = dicts.ToList();
            cbxDepartmentType.DisplayMemberPath = "DICTIONARYNAME";
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
    }
}
