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
using System.Collections.ObjectModel;
using SMT.HRM.UI.Form.Personnel;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class AssessmentFormMaster : BasePage
    {
        SMTLoading loadbar = new SMTLoading();
        PersonnelServiceClient client;
        public AssessmentFormMaster()
        {
            InitializeComponent();
            InitControlEvent();
            GetEntityLogo("T_HR_ASSESSMENTFORMMASTER");
        }
        
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_ASSESSMENTFORMMASTER", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString()); 
        }

        private void InitControlEvent()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            this.Loaded += new RoutedEventHandler(AssessmentFormMaster_Loaded);

            client = new PersonnelServiceClient();
            client.AssessmentFormMasterPagingCompleted += new EventHandler<AssessmentFormMasterPagingCompletedEventArgs>(client_AssessmentFormMasterPagingCompleted);
        }

        void AssessmentFormMaster_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();


            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            TextBox txtCardNumber = Utility.FindChildControl<TextBox>(expander, "txtCardNumber");

            if (!string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                filter += "NAME==@" + paras.Count().ToString();
                paras.Add(txtName.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtCardNumber.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "IDCARDNUMBER==@" + paras.Count().ToString();
                paras.Add(txtCardNumber.Text.Trim());
            }
            client.AssessmentFormMasterPagingAsync(dataPager.PageIndex, dataPager.PageSize, "NAME", 
                filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void client_AssessmentFormMasterPagingCompleted(object sender, AssessmentFormMasterPagingCompletedEventArgs e)
        {
            List<T_HR_ASSESSMENTFORMMASTER> list = new List<T_HR_ASSESSMENTFORMMASTER>();
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
            loadbar.Stop();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_ASSESSMENTFORMMASTER");
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count>0)
            {
                T_HR_ASSESSMENTFORMMASTER ent = DtGrid.SelectedItems[0] as T_HR_ASSESSMENTFORMMASTER;
                CheckGrade form = new CheckGrade(ent.ASSESSMENTFORMMASTERID, ent.EMPLOYEEID, ent.EMPLOYEELEVEL);
                EntityBrowser browser = new EntityBrowser(form);
                
                //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
            }
        }
    }
}
