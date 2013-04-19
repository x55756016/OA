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

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalarySolutionAssign : BasePage, IClient
    {
        private SalaryServiceClient client = new SalaryServiceClient();
        SMTLoading loadbar = new SMTLoading();
        public SalarySolutionAssign()
        {
            InitializeComponent();
            //InitParas();
            //GetEntityLogo("T_HR_SALARYSOLUTIONASSIGN");
            //LoadData();
            this.Loaded += new RoutedEventHandler(SalarySolutionAssign_Loaded);
        }

        void SalarySolutionAssign_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_SALARYSOLUTIONASSIGN");
            LoadData();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYSOLUTIONASSIGN", true);
            //ViewTitles.TextTitle.Text = "月薪>>薪资方案应用"; //GetTitleFromURL(e.Uri.ToString());
            EditTitle("SALARY", "SALARYSOLUTIONASSIGN");
            ToolBar.btnNew.Visibility = Visibility.Visible;
            ToolBar.btnEdit.Visibility = Visibility.Visible;

        }

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.SalarySolutionAssignDeleteCompleted += new EventHandler<SalarySolutionAssignDeleteCompletedEventArgs>(client_SalarySolutionAssignDeleteCompleted);
            client.GetSalarySolutionAssignPagingCompleted += new EventHandler<GetSalarySolutionAssignPagingCompletedEventArgs>(client_GetSalarySolutionAssignPagingCompleted);

            #region 工具栏
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            #endregion
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_SALARYSOLUTIONASSIGN tmpSolution = DtGrid.SelectedItems[0] as V_SALARYSOLUTIONASSIGN;

                Form.Salary.SalarySolutionAssignForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionAssignForm(FormTypes.Browse, tmpSolution.SalarySolutionAssign.SALARYSOLUTIONASSIGNID);

                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                form.MinHeight = 250;
                browser.IsEnabled = false;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void client_GetSalarySolutionAssignPagingCompleted(object sender, GetSalarySolutionAssignPagingCompletedEventArgs e)
        {
            List<V_SALARYSOLUTIONASSIGN> list = new List<V_SALARYSOLUTIONASSIGN>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
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

        void client_SalarySolutionAssignDeleteCompleted(object sender, SalarySolutionAssignDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSOLUTIONASSIGN"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SalarySolutionAssign"));
                LoadData();
            }
        }


        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.SalarySolutionAssignForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionAssignForm(FormTypes.New, "");

            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 250;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += " @" + paras.Count().ToString() + ".Contains(SALARYSOLUTIONNAME)";
                paras.Add(txtName.Text.Trim());
            }
            client.GetSalarySolutionAssignPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYSOLUTIONASSIGNID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_SALARYSOLUTIONASSIGN tmpSolution = DtGrid.SelectedItems[0] as V_SALARYSOLUTIONASSIGN;

                Form.Salary.SalarySolutionAssignForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionAssignForm(FormTypes.Edit, tmpSolution.SalarySolutionAssign.SALARYSOLUTIONASSIGNID);

                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (V_SALARYSOLUTIONASSIGN tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.SalarySolutionAssign.SALARYSOLUTIONASSIGNID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.SalarySolutionAssignDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
        }




        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid.SelectedItems.Count > 0)
            {
                V_SALARYSOLUTIONASSIGN tmpEnt = DtGrid.SelectedItems[0] as V_SALARYSOLUTIONASSIGN;
                Form.Salary.SalarySolutionAssignForm form = new Form.Salary.SalarySolutionAssignForm(FormTypes.Audit, tmpEnt.SalarySolutionAssign.SALARYSOLUTIONASSIGNID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 250;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
        }
        #endregion


        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SALARYSOLUTIONASSIGN");
        }

        private void EditTitle(string parentTitle, string newTitle)
        {
            System.Text.StringBuilder sbtitle = new System.Text.StringBuilder();
            sbtitle.Append(Utility.GetResourceStr(parentTitle));
            sbtitle.Append(">>");
            sbtitle.Append(Utility.GetResourceStr(newTitle));
            ViewTitles.TextTitle.Text = sbtitle.ToString();
        }
    }
}
