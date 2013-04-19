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
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Views.Personnel
{
    public partial class LeftOfficeConfirm : BasePage, IClient
    {
        public string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        PersonnelServiceClient client;
        public LeftOfficeConfirm()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_LEFTOFFICE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_LEFTOFFICECONFIRM", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);

            client = new PersonnelServiceClient();
            //  client.LeftOfficePagingCompleted += new EventHandler<LeftOfficePagingCompletedEventArgs>(client_LeftOfficePagingCompleted);
            client.LeftOfficeConfirmDeleteCompleted += new EventHandler<LeftOfficeConfirmDeleteCompletedEventArgs>(client_LeftOfficeConfirmDeleteCompleted);
            client.LeftOfficeConfirmPagingCompleted += new EventHandler<LeftOfficeConfirmPagingCompletedEventArgs>(client_LeftOfficeConfirmPagingCompleted);

            this.Loaded += new RoutedEventHandler(LeftOfficeConfirm_Loaded);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            //ToolBar.btnNew.Visibility = Visibility.Collapsed;
            //ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ImageButton _ImgButtonViewBasicInfo = new ImageButton();
            _ImgButtonViewBasicInfo.VerticalAlignment = VerticalAlignment.Center;
            _ImgButtonViewBasicInfo.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("员工离职报表")).Click += new RoutedEventHandler(_ImgButtonViewBasicInfo_Click);
            ToolBar.stpOtherAction.Children.Add(_ImgButtonViewBasicInfo);
        }

        void _ImgButtonViewBasicInfo_Click(object sender, RoutedEventArgs e)
        {
            Form.Personnel.EmployeeLeftOfficeReports form = new SMT.HRM.UI.Form.Personnel.EmployeeLeftOfficeReports();
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            
        }

        void client_LeftOfficeConfirmPagingCompleted(object sender, LeftOfficeConfirmPagingCompletedEventArgs e)
        {
            List<T_HR_LEFTOFFICECONFIRM> list = new List<T_HR_LEFTOFFICECONFIRM>();
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
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



        void LeftOfficeConfirm_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_LEFTOFFICE");
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_LEFTOFFICECONFIRM");
                LoadData();
            }
        }

        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (txtEmpName != null)
            {
                if (!string.IsNullOrEmpty(txtEmpName.Text))
                {

                    // filter += "T_HR_EMPLOYEE.EMPLOYEECNAME==@" + paras.Count().ToString();
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                    paras.Add(txtEmpName.Text.Trim());
                }
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }



            client.LeftOfficeConfirmPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME", filter,
                paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
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

        #region 添加,修改,删除,审核
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            //  form.MinHeight = 450;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
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
                T_HR_LEFTOFFICECONFIRM temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICECONFIRM;
                LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.Browse, temp.CONFIRMID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                // form.MinHeight = 450;
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
                T_HR_LEFTOFFICECONFIRM temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICECONFIRM;

                if (temp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_LEFTOFFICECONFIRM", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.Edit, temp.CONFIRMID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                //  form.MinHeight = 450;
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

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    bool flag = false;
                    foreach (T_HR_LEFTOFFICECONFIRM tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            flag = true;
                            break;
                        }
                        //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_LEFTOFFICECONFIRM", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        //{
                        //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_LEFTOFFICE.T_HR_EMPLOYEE.EMPLOYEECNAME + ",LEFTOFFICECONFIRM"),
                        //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        //    return;
                        //}
                        ids.Add(tmp.CONFIRMID);
                    }
                    if (flag == true)
                    {
                        return;
                    }
                    client.LeftOfficeConfirmDeleteAsync(ids);
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

        void client_LeftOfficeConfirmDeleteCompleted(object sender, LeftOfficeConfirmDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "LEFTOFFICE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                LoadData();
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_LEFTOFFICECONFIRM temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICECONFIRM;
                LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.Audit, temp.CONFIRMID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                //  form.MinHeight = 450;
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

        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_LEFTOFFICECONFIRM temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICECONFIRM;
                LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.Resubmit, temp.CONFIRMID);
                EntityBrowser browser = new EntityBrowser(form);
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
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_LEFTOFFICE");
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
