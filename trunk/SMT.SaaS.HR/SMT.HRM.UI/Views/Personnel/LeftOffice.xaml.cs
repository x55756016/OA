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
using System.IO;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class LeftOffice : BasePage, IClient
    {
        public string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        PersonnelServiceClient client;
        public LeftOffice()
        {
            InitializeComponent();
            InitEvent();
            //GetEntityLogo("T_HR_LEFTOFFICE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_LEFTOFFICE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);

            client = new PersonnelServiceClient();
            // client.LeftOfficePagingCompleted += new EventHandler<LeftOfficePagingCompletedEventArgs>(client_LeftOfficePagingCompleted);
            client.LeftOfficeViewsPagingCompleted += new EventHandler<LeftOfficeViewsPagingCompletedEventArgs>(client_LeftOfficeViewsPagingCompleted);
            client.LeftOfficeDeleteCompleted += new EventHandler<LeftOfficeDeleteCompletedEventArgs>(client_LeftOfficeDeleteCompleted);
            client.ExportLeftOfficeViewsCompleted += new EventHandler<ExportLeftOfficeViewsCompletedEventArgs>(client_ExportLeftOfficeViewsCompleted);
            this.Loaded += new RoutedEventHandler(LeftOffice_Loaded);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnOutExcel.Visibility = System.Windows.Visibility.Visible;
            ToolBar.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            //ImageButton btnCreate = new ImageButton();
            //btnCreate.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("离职确认")).Click += new RoutedEventHandler(btnCreate_Click);
            //ToolBar.stpOtherAction.Children.Add(btnCreate);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //int a = SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("T_HR_LEFTOFFICE", SMT.SaaS.FrameworkUI.Common.Permissions.Add);
            //MessageBox.Show(a.ToString());
            // if (a<0)
            // {
            //     btnCreate.Visibility = Visibility.Collapsed;
            // }
            // else
            // {
            //     btnCreate.Visibility = Visibility.Visible;
            // }
        }

        void client_ExportLeftOfficeViewsCompleted(object sender, ExportLeftOfficeViewsCompletedEventArgs e)
        {
            loadbar.Stop();
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {

            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;

            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                
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

                DatePicker dtStart = Utility.FindChildControl<DatePicker>(expander, "dpStartDate");
                DateTime dtOutstart = DateTime.MinValue;
                if (dtStart != null)
                {
                    bool flag = DateTime.TryParse(dtStart.Text, out dtOutstart);
                }

                DatePicker dtEnd = Utility.FindChildControl<DatePicker>(expander, "dpEndDate");
                DateTime dtOutEnd = DateTime.MinValue;
                if (dtEnd != null)
                {
                    string end = dtEnd.Text.ToString();
                    bool flag = DateTime.TryParse(dtEnd.Text, out dtOutEnd);
                }
                if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
                {
                    strState = Checkstate;
                }
                loadbar.Start();
                client.ExportLeftOfficeViewsAsync(filter, paras, dtOutstart, dtOutEnd, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
            }
        }





        void LeftOffice_Loaded(object sender, RoutedEventArgs e)
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
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_LEFTOFFICE");
                LoadData();
            }
        }

        void client_LeftOfficePagingCompleted(object sender, LeftOfficePagingCompletedEventArgs e)
        {
            List<T_HR_LEFTOFFICE> list = new List<T_HR_LEFTOFFICE>();
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

        void client_LeftOfficeViewsPagingCompleted(object sender, LeftOfficeViewsPagingCompletedEventArgs e)
        {
            List<V_LEFTOFFICEVIEW> list = new List<V_LEFTOFFICEVIEW>();
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

            DatePicker dtStart = Utility.FindChildControl<DatePicker>(expander, "dpStartDate");
            DateTime dtOutstart = DateTime.MinValue;
            if (dtStart != null)
            {
                bool flag = DateTime.TryParse(dtStart.Text, out dtOutstart);
            }

            DatePicker dtEnd = Utility.FindChildControl<DatePicker>(expander, "dpEndDate");
            DateTime dtOutEnd = DateTime.MinValue;
            if (dtEnd != null)
            {
                string end = dtEnd.Text.ToString();
                bool flag = DateTime.TryParse(dtEnd.Text, out dtOutEnd);
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }


            client.LeftOfficeViewsPagingAsync(dataPager.PageIndex, dataPager.PageSize, "LEFTOFFICEDATE", filter,
             paras, dtOutstart, dtOutEnd, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
            //client.LeftOfficePagingAsync(dataPager.PageIndex, dataPager.PageSize, "LEFTOFFICEDATE", filter,
            //    paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
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
            LeftOfficeForm form = new LeftOfficeForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 350;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                // T_HR_LEFTOFFICE temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICE;
                V_LEFTOFFICEVIEW tempview = DtGrid.SelectedItems[0] as V_LEFTOFFICEVIEW;
                
                T_HR_LEFTOFFICE temp = new T_HR_LEFTOFFICE();
                temp.DIMISSIONID = tempview.DIMISSIONID;
                temp.CREATEUSERID = tempview.CREATEUSERID;
                temp.CHECKSTATE = tempview.CHECKSTATE;
                temp.OWNERCOMPANYID = tempview.OWNERCOMPANYID;
                temp.OWNERDEPARTMENTID = tempview.OWNERDEPARTMENTID;
                temp.OWNERPOSTID = tempview.OWNERPOSTID;
                temp.OWNERID = tempview.EMPLOYEEID;
                temp.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                temp.T_HR_EMPLOYEE.EMPLOYEEID = tempview.EMPLOYEEID;
                temp.T_HR_EMPLOYEE.EMPLOYEECNAME = tempview.EMPLOYEECNAME;
                temp.APPLYDATE = tempview.APPLYDATE;
                temp.LEFTOFFICEDATE = tempview.LEFTOFFICEDATE;
                temp.LEFTOFFICECATEGORY = tempview.LEFTOFFICECATEGORY;
                temp.LEFTOFFICEREASON = tempview.LEFTOFFICEREASON;
                temp.REMARK = tempview.REMARK;
                temp.T_HR_EMPLOYEEPOST = new T_HR_EMPLOYEEPOST();
                temp.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID = tempview.EMPLOYEEPOSTID;
                if (temp.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    LeftOfficeConfirmForm form = new LeftOfficeConfirmForm(FormTypes.New, temp);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.New;
                    // form.MinHeight = 350;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("离职单未审核通过"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

        }
        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                //T_HR_LEFTOFFICE temp = DtGrid.SelectedItems[0] as T_HR_LEFTOFFICE;
                V_LEFTOFFICEVIEW temp = DtGrid.SelectedItems[0] as V_LEFTOFFICEVIEW;
                LeftOfficeForm form = new LeftOfficeForm(FormTypes.Browse, temp.DIMISSIONID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 350;
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
                V_LEFTOFFICEVIEW temp = DtGrid.SelectedItems[0] as V_LEFTOFFICEVIEW;
                if (temp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(temp, "T_HR_LEFTOFFICE", SMT.SaaS.FrameworkUI.OperationType.Edit, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOEDITPERMISSION"),
                //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
                LeftOfficeForm form = new LeftOfficeForm(FormTypes.Edit, temp.DIMISSIONID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                form.MinHeight = 350;
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
                    foreach (V_LEFTOFFICEVIEW tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            flag = true;
                            break;
                        }
                        //if (!SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(tmp, "T_HR_LEFTOFFICE", SMT.SaaS.FrameworkUI.OperationType.Delete, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
                        //{
                        //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSIONDELETE", tmp.T_HR_EMPLOYEE.EMPLOYEECNAME + ",LEFTOFFICE"),
                        //  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        //    return;
                        //}
                        ids.Add(tmp.DIMISSIONID);
                    }
                    if (flag == true)
                    {
                        return;
                    }
                    client.LeftOfficeDeleteAsync(ids);
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

        void client_LeftOfficeDeleteCompleted(object sender, LeftOfficeDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                int result = e.Result;
                if (result > 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "LEFTOFFICE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    LoadData();
                }
                else if (result == 0)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), "抱歉，此为重新提交的单据，不可删除.",
                                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), "服务器删除数据出错.",
                                                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_LEFTOFFICEVIEW temp = DtGrid.SelectedItems[0] as V_LEFTOFFICEVIEW;
                LeftOfficeForm form = new LeftOfficeForm(FormTypes.Audit, temp.DIMISSIONID);
                EntityBrowser browser = new EntityBrowser(form);
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

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {

            if (DtGrid.SelectedItems.Count > 0)
            {
                V_LEFTOFFICEVIEW temp = DtGrid.SelectedItems[0] as V_LEFTOFFICEVIEW;
                //审核通过的不能重新提交,因为重新提交不能改变员工离职状态，所以这里还是可以禁用，
                //要让员工状态改变，可以在员工入职那重新提交
                if (temp.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPEATAUDITERROR"),
                                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    return;
                }
                LeftOfficeForm form = new LeftOfficeForm(FormTypes.Resubmit, temp.DIMISSIONID);
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
            V_LEFTOFFICEVIEW view = e.Row.DataContext as V_LEFTOFFICEVIEW;
            //ImageButton ReportButton = DtGrid.Columns[6].GetCellContent(e.Row).FindName("ReportBtn") as ImageButton;
            Button ReportButton = DtGrid.Columns[8].GetCellContent(e.Row).FindName("ReportBtn") as Button;
            ReportButton.IsEnabled = false;
            int ISCONFIRMED = Convert.ToInt32(view.ISCONFIRMED);
            if (ISCONFIRMED < 0)
            {
                ReportButton.Content = "未确认";
                ReportButton.IsEnabled = true;
            }
            else if (ISCONFIRMED >= 0 && ISCONFIRMED < 2)
            {
                ReportButton.Content = "确认中";
                ReportButton.IsEnabled = false;
            }
            else if (ISCONFIRMED == 2)
            {
                ISCONFIRMED = 3;
                ReportButton.Content = "已确认";
                ReportButton.IsEnabled = false;
            }
            else if (ISCONFIRMED == 3)//by luojie
            {
                ReportButton.Content = "确认未通过";
                ReportButton.IsEnabled = false;
            }
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
