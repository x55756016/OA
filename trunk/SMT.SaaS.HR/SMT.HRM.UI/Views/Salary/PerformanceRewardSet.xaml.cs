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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Views.Salary
{
    public partial class PerformanceRewardSet : BasePage
    {

        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private string Checkstate { get; set; }
        public PerformanceRewardSet()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PerformanceRewardSet_Loaded);
            //InitParas();
            //GetEntityLogo("T_HR_PERFORMANCEREWARDSET");
        }

        void PerformanceRewardSet_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_PERFORMANCEREWARDSET");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_PERFORMANCEREWARDSET", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
          //  LoadData();
        }


        private void InitParas()
        {
            PARENT.Children.Add(loadbar);

            client.PerformanceRewardSetDeleteCompleted += new EventHandler<PerformanceRewardSetDeleteCompletedEventArgs>(client_PerformanceRewardSetDeleteCompleted);
            client.GetPerformanceRewardSetWithPagingCompleted += new EventHandler<GetPerformanceRewardSetWithPagingCompletedEventArgs>(client_GetPerformanceRewardSetWithPagingCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            //ToolBar.btnAduitNoTPass.Click += new RoutedEventHandler(btnAuitNoTPass_click);
            //ToolBar.btnSumbitAudit.Click += new RoutedEventHandler(btnSumbitAudit_click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDSET;

                Form.Salary.PerformanceRewardSetForm form = new SMT.HRM.UI.Form.Salary.PerformanceRewardSetForm(FormTypes.Browse, tmpEnt.PERFORMANCEREWARDSETID);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }


        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_PERFORMANCEREWARDSET");
                LoadData();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
        }
        void client_GetPerformanceRewardSetWithPagingCompleted(object sender, GetPerformanceRewardSetWithPagingCompletedEventArgs e)
        {
            List<T_HR_PERFORMANCEREWARDSET> list = new List<T_HR_PERFORMANCEREWARDSET>();
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        void client_PerformanceRewardSetDeleteCompleted(object sender, PerformanceRewardSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "PERFORMANCEREWARDSET"));
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

            Form.Salary.PerformanceRewardSetForm form = new SMT.HRM.UI.Form.Salary.PerformanceRewardSetForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 220;
            form.MinWidth = 550;
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
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            //TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            //{
            //    filter += " PERFORMANCECATEGORY==@" + paras.Count().ToString();
            //    paras.Add(txtName.Text.Trim());
            //}
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            client.GetPerformanceRewardSetWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "PERFORMANCEREWARDSETID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDSET;

                Form.Salary.PerformanceRewardSetForm form = new SMT.HRM.UI.Form.Salary.PerformanceRewardSetForm(FormTypes.Edit, tmpEnt.PERFORMANCEREWARDSETID);

                EntityBrowser browser = new EntityBrowser(form);
                form.MinHeight = 220;
                form.MinWidth = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_PERFORMANCEREWARDSET tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.PERFORMANCEREWARDSETID);
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.PerformanceRewardSetDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_PERFORMANCEREWARDSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_PERFORMANCEREWARDSET;
                Form.Salary.PerformanceRewardSetForm form = new Form.Salary.PerformanceRewardSetForm(FormTypes.Audit, tmpEnt.PERFORMANCEREWARDSETID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }
        void btnSumbitAudit_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 提交审核            
        }
        void btnAuitNoTPass_click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核不通过          
        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_PERFORMANCEREWARDSET");
        }
    }
}
