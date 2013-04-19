using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

using SMT.Saas.Tools.PersonnelWS;

using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form.Personnel;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class PensionAlarmSet : BasePage,IClient
    {
        PersonnelServiceClient client;
        SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orclient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
        SMTLoading loadbar = new SMTLoading();
        public PensionAlarmSet()
        {
            InitializeComponent();
            InitEvent();
            GetEntityLogo("T_HR_PENSIONALARMSET");
            //this.Loaded += (sender, args) =>
            //{
            //    InitEvent();
            //    GetEntityLogo("T_HR_PENSIONALARMSET");
            //};
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_PENSIONALARMSET", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
     
        private void InitEvent()
        {
            try
            {
                PARENT.Children.Add(loadbar);
                //loadbar.Start();
                client = new PersonnelServiceClient();
                client.GetPensionAlarmSetPagingCompleted += new EventHandler<GetPensionAlarmSetPagingCompletedEventArgs>(client_GetPensionAlarmSetPagingCompleted);
                client.PensionAlarmSetDeleteCompleted += new EventHandler<PensionAlarmSetDeleteCompletedEventArgs>(client_PensionAlarmSetDeleteCompleted);
                orclient.GetCompanyAllCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyAllCompletedEventArgs>(orclient_GetCompanyAllCompleted);
                //控件事件
                ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                ToolBar.btnAudit.Visibility = Visibility.Collapsed;
                ToolBar.retAudit.Visibility = Visibility.Collapsed;
                ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
                ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
        }

       



        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            orclient.GetCompanyAllAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            //TextBox txtCompanyID = Utility.FindChildControl<TextBox>(expander, "txtCompanyID");
            //ComboBox cbxCompanyName = Utility.FindChildControl<ComboBox>(expander, "cbxCompanyName");
            //NumericUpDown nrdAlarmpay = Utility.FindChildControl<NumericUpDown>(expander, "nrdAlarmpay");
            //if (!string.IsNullOrEmpty(txtCompanyID.Text.Trim()))
            //{
            //    filter += "COMPANYID==@" + paras.Count().ToString();
            //    paras.Add(txtCompanyID.Text.Trim());
            //}
            //if (cbxCompanyName.SelectedIndex > 0)
            //{
            //    filter += "COMPANYID==@" + paras.Count().ToString();
            //    paras.Add((cbxCompanyName.SelectedItem as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID);
            //}
            //if (nrdAlarmpay.Value.ToString() != "0")
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "ALARMPAY==@" + paras.Count().ToString();
            //    paras.Add(Convert.ToInt32(nrdAlarmpay.Value));
            //}
            client.GetPensionAlarmSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "T_HR_PENSIONALARMSET.COMPANYID", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        void orclient_GetCompanyAllCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyAllCompletedEventArgs e)
        {
            //ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> cmp = new ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
            //if (e.Error != null && e.Error.Message != "")
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            //}
            //else
            //{
            //    ComboBox cbxCompanyName = Utility.FindChildControl<ComboBox>(expander, "cbxCompanyName");
            //    cmp = e.Result;
            //    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY all = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
            //    all.COMPANYID = "companyID";
            //    all.CNAME = Utility.GetResourceStr("ALL");
            //    if (cmp != null)
            //    {
            //        cmp.Insert(0, all);
            //    }
            //    cbxCompanyName.ItemsSource = cmp;
            //    cbxCompanyName.DisplayMemberPath = "CNAME";
            //}
        }
      

        void client_GetPensionAlarmSetPagingCompleted(object sender, GetPensionAlarmSetPagingCompletedEventArgs e)
        {
            List<V_PENSIONALARMSET> list = new List<V_PENSIONALARMSET>();
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
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

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 添加,修改,删除
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoadData();
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
                    foreach (V_PENSIONALARMSET tmp in DtGrid.SelectedItems)
                    {
                        if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_PENSIONALARMSET", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.CNAME + ",PENSIONALARMSET"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        ids.Add(tmp.T_HR_PENSIONALARMSET.PENSIONSETID);
                    }
                    client.PensionAlarmSetDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void client_PensionAlarmSetDeleteCompleted(object sender, PensionAlarmSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PENSIONALARMSET"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_PENSIONALARMSET temp = DtGrid.SelectedItems[0] as V_PENSIONALARMSET;
                PensionAlarmSetForm form = new PensionAlarmSetForm(FormTypes.Browse, temp.T_HR_PENSIONALARMSET.PENSIONSETID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 270.0;
                form.MinWidth = 400;
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
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
               V_PENSIONALARMSET temp = DtGrid.SelectedItems[0] as V_PENSIONALARMSET;
               if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_PENSIONALARMSET", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
               {
                   ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                   return;
               }
                PensionAlarmSetForm form = new PensionAlarmSetForm(FormTypes.Edit, temp.T_HR_PENSIONALARMSET.PENSIONSETID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 270.0;
                form.MinWidth = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            PensionAlarmSetForm form = new PensionAlarmSetForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 270.0;
            form.MinWidth = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void from_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_PENSIONALARMSET");
        }

        private void cbxCompanyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
    }
}
