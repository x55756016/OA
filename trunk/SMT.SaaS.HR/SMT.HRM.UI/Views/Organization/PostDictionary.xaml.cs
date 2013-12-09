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
using SMT.SaaS.FrameworkUI.AutoCompleteComboBox;

namespace SMT.HRM.UI.Views.Organization
{
    public partial class PostDictionary : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient client;
        public string Checkstate { get; set; }
        public PostDictionary()
        {
            InitializeComponent();
            InitControlEvent();
            ///GetEntityLogo("T_HR_POSTDICTIONARY");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }


        private void InitControlEvent()
        {
            PARENT.Children.Add(loadbar);

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
            this.Loaded += new RoutedEventHandler(PostDictionary_Loaded);
            client = new OrganizationServiceClient();
            client.PostDictionaryPagingCompleted += new EventHandler<PostDictionaryPagingCompletedEventArgs>(client_PostDictionaryPagingCompleted);
            client.PostDictionaryDeleteCompleted += new EventHandler<PostDictionaryDeleteCompletedEventArgs>(client_PostDictionaryDeleteCompleted);
            client.GetDepartmentDictionaryAllCompleted += new EventHandler<GetDepartmentDictionaryAllCompletedEventArgs>(client_GetDepartmentDictionaryAllCompleted);

        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_POSTDICTIONARY");
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved))
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                dataPager.PageIndex = 1;
                LoadData();
            }
        }


        void client_GetDepartmentDictionaryAllCompleted(object sender, GetDepartmentDictionaryAllCompletedEventArgs e)
        {
            ObservableCollection<T_HR_DEPARTMENTDICTIONARY> dtmp = new ObservableCollection<T_HR_DEPARTMENTDICTIONARY>();

            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                         Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //ComboBox cbxDepName = Utility.FindChildControl<ComboBox>(expander, "cbxDepName");
                AutoCompleteComboBox acbDepName = Utility.FindChildControl<AutoCompleteComboBox>(expander, "acbDepName");
                if (e.Result != null)
                {
                    dtmp = e.Result;
                    T_HR_DEPARTMENTDICTIONARY all = new T_HR_DEPARTMENTDICTIONARY();
                    all.DEPARTMENTDICTIONARYID = "dictionaryID";
                    all.DEPARTMENTNAME = Utility.GetResourceStr("ALL");
                    dtmp.Insert(0, all);
                }

                var entity = from ent in e.Result
                             orderby ent.DEPARTMENTNAME
                             select ent;
                List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
                dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE").OrderBy(s => s.DICTIONARYVALUE).ToList();

                foreach (T_HR_DEPARTMENTDICTIONARY diction in entity)
                {
                    decimal dptype = Convert.ToDecimal(diction.DEPARTMENTTYPE);
                    var tmp = dicts.Where(s => s.DICTIONARYVALUE == dptype).FirstOrDefault();
                    if (tmp != null)
                    {
                        diction.DEPARTMENTNAME = diction.DEPARTMENTNAME + "(" + tmp.DICTIONARYNAME + ")";
                    }
                }
                acbDepName.ItemsSource = entity;
                acbDepName.ValueMemberPath = "DEPARTMENTNAME";
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            dataPager.PageIndex = 1;
            LoadData();
        }

        void PostDictionary_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_POSTDICTIONARY");

            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_POSTDICTIONARY", true);
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            client.GetDepartmentDictionaryAllAsync();
            // LoadData();

        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            TextBox txtPostCode = Utility.FindChildControl<TextBox>(expander, "txtPostCode");
            if (txtPostCode != null)
            {
                if (!string.IsNullOrEmpty(txtPostCode.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //filter += "POSTCODE==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(POSTCODE)";
                    paras.Add(txtPostCode.Text.Trim());
                }
            }
            TextBox txtPostName = Utility.FindChildControl<TextBox>(expander, "txtPostName");
            if (txtPostName != null)
            {
                if (!string.IsNullOrEmpty(txtPostName.Text))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " @" + paras.Count().ToString() + ".Contains(POSTNAME)";
                    paras.Add(txtPostName.Text.Trim());
                }
            }
            //ComboBox cbxDepName = Utility.FindChildControl<ComboBox>(expander, "cbxDepName");
            AutoCompleteComboBox acbDepName = Utility.FindChildControl<AutoCompleteComboBox>(expander, "acbDepName");
            if (acbDepName != null)
            {
                if (acbDepName.SelectedItem  !=null )
                {
                    T_HR_DEPARTMENTDICTIONARY ent = acbDepName.SelectedItem as T_HR_DEPARTMENTDICTIONARY;
                    if (!string.IsNullOrEmpty(ent.DEPARTMENTDICTIONARYID))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID==@" + paras.Count().ToString();
                        paras.Add(ent.DEPARTMENTDICTIONARYID);
                    }
                }
            }
            client.PostDictionaryPagingAsync(dataPager.PageIndex, dataPager.PageSize, "POSTCODE", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, Checkstate);
        }

        void client_PostDictionaryPagingCompleted(object sender, PostDictionaryPagingCompletedEventArgs e)
        {
            List<T_HR_POSTDICTIONARY> list = new List<T_HR_POSTDICTIONARY>();
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                    list = e.Result.ToList();
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        #region 添加、修改、删除及查询
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            PostDictionaryForms form = new PostDictionaryForms(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 500;
            browser.FormType = FormTypes.New;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_POSTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_POSTDICTIONARY;
                PostDictionaryForms form = new PostDictionaryForms(FormTypes.Browse, tmpEnt.POSTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinWidth = 500;
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
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_POSTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_POSTDICTIONARY;
                if (tmpEnt.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                PostDictionaryForms form = new PostDictionaryForms(FormTypes.Edit, tmpEnt.POSTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                // ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_POSTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_POSTDICTIONARY;

                //if (tmpEnt.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPEATAUDITERROR"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}

                PostDictionaryForms form = new PostDictionaryForms(FormTypes.Resubmit, tmpEnt.POSTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                // ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_POSTDICTIONARY tmpEnt = DtGrid.SelectedItems[0] as T_HR_POSTDICTIONARY;
                PostDictionaryForms form = new PostDictionaryForms(FormTypes.Audit, tmpEnt.POSTDICTIONARYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                // ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
                //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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
                ObservableCollection<string> ids = new ObservableCollection<string>();
                foreach (T_HR_POSTDICTIONARY tmp in DtGrid.SelectedItems)
                {
                    if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                        return;
                    }
                    ids.Add(tmp.POSTDICTIONARYID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.PostDictionaryDeleteAsync(ids, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_PostDictionaryDeleteCompleted(object sender, PostDictionaryDeleteCompletedEventArgs e)
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
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "POSTDICTIONARY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                dataPager.PageIndex = 1;
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
            SetRowLogo(DtGrid, e.Row, "T_HR_POSTDICTIONARY");
        }

        private void cbxDepName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
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

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }

        private void acbDepName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataPager.PageIndex = 1;
            LoadData();
        }
    }
}
