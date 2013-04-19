using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.Saas.Tools.PersonnelWS;
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class Resume : BasePage, IClient
    {
        PersonnelServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        public Resume()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
                LoadData();
                GetEntityLogo("T_HR_RESUME");
            };
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_RESUME", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region 初始化事件及装载数据
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            ObservableCollection<string> paras = new ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            TextBox txtCardNumber = Utility.FindChildControl<TextBox>(expander, "txtCardNumber");

            if (txtName != null)
            {
                if (!string.IsNullOrEmpty(txtName.Text.Trim()))
                {
                    //filter += "NAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(NAME)";
                    paras.Add(txtName.Text.Trim());
                }
            }
            if (txtCardNumber != null)
            {
                if (!string.IsNullOrEmpty(txtCardNumber.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    //  filter += "IDCARDNUMBER==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(IDCARDNUMBER)";
                    paras.Add(txtCardNumber.Text.Trim());
                }
            }
            client.GetResumePagingAsync(dataPager.PageIndex, dataPager.PageSize, "NAME", filter, paras, pageCount);
        }

        private void InitControlEvent()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);

            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;

            ToolBar.ShowRect();
            client = new PersonnelServiceClient();
            client.GetResumePagingCompleted += new EventHandler<GetResumePagingCompletedEventArgs>(client_GetResumePagingCompleted);
            client.ResumeDeleteCompleted += new EventHandler<ResumeDeleteCompletedEventArgs>(client_ResumeDeleteCompleted);
        }



        void client_GetResumePagingCompleted(object sender, GetResumePagingCompletedEventArgs e)
        {
            List<T_HR_RESUME> list = new List<T_HR_RESUME>();
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
            loadbar.Stop();
        }

        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加,修改,删除,查询,审核
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ResumeForm form = new ResumeForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinWidth = 750;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_RESUME temp = DtGrid.SelectedItems[0] as T_HR_RESUME;
                ResumeForm form = new ResumeForm(FormTypes.Browse, temp.RESUMEID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 750;
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
                T_HR_RESUME temp = DtGrid.SelectedItems[0] as T_HR_RESUME;
                if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_RESUME", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                ResumeForm form = new ResumeForm(FormTypes.Edit, temp.RESUMEID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinWidth = 750;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    foreach (T_HR_RESUME tmp in DtGrid.SelectedItems)
                    {
                        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_RESUME", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.NAME + ",RESUME"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        ids.Add(tmp.RESUMEID);
                    }
                    client.ResumeDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_ResumeDeleteCompleted(object sender, ResumeDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "RESUME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_PENSIONMASTER");
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
    }
}
