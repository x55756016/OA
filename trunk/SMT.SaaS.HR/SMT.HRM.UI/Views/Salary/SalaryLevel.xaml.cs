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

using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.SalaryWS;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryLevel : BasePage,IClient
    {
        public SalaryLevel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SalaryLevel_Loaded);
            //InitParas();
            //GetEntityLogo("T_HR_SALARYLEVEL");
        }

        void SalaryLevel_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_SALARYLEVEL");
        }

        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();

        private ObservableCollection<T_SYS_DICTIONARY> salaryLevelDicts;
        private ObservableCollection<T_HR_SALARYLEVEL> salaryLevels;

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.GetSalaryLevelPagingCompleted += new EventHandler<GetSalaryLevelPagingCompletedEventArgs>(client_GetSalaryLevelPagingCompleted);
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            
            client.GenerateSalaryLevelCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_GenerateSalaryLevelCompleted);
            //获取字典中的岗位级别
            permissionClient.GetSysDictionaryByCategoryAsync("SALARYLEVEL");            

            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
          //  ToolBar.btnNew.Visibility = Visibility.Collapsed;
            

            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;


            ImageButton btnPreView = new ImageButton();
            btnPreView.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/18_workPlace.png", Utility.GetResourceStr("PREVIEW")).Click += new RoutedEventHandler(btnPreView_Click);
            ToolBar.stpOtherAction.Children.Add(btnPreView);

            ImageButton btnCreate = new ImageButton();
            btnCreate.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("CREATE")).Click += new RoutedEventHandler(btnCreate_Click);
            ToolBar.stpOtherAction.Children.Add(btnCreate);
        }



   
        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            //TextBox txtName = Utility.FindChildControl<TextBox Style="{StaticResource TextBoxStyle}">(expander, "txtName");
            //if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            //{
            //    filter += "T_HR_SALARYSTANDARD.SALARYSTANDARDNAME==@" + paras.Count().ToString();
            //    paras.Add(txtName.Text.Trim());
            //}

            client.GetSalaryLevelPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYLEVELID", filter, paras, pageCount,SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region 完成事件

        void client_GetSalaryLevelPagingCompleted(object sender, GetSalaryLevelPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                salaryLevels = e.Result;
                DtGrid.ItemsSource = salaryLevels;
                dataPager.PageCount = e.pageCount;
            }



            loadbar.Stop();
        }
        void client_GenerateSalaryLevelCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYLEVEL"));
                LoadData();
            }
            loadbar.Stop();

        }
        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                salaryLevelDicts = e.Result;
                //获取已设置的岗位
                LoadData();
            }

        }
        #endregion
        #region  按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {

            Form.Salary.SalaryLevelForm form = new SMT.HRM.UI.Form.Salary.SalaryLevelForm(FormTypes.New, "");

            EntityBrowser browser = new EntityBrowser(form);
            form.MinHeight = 220;
            form.MinWidth = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            //loadbar.Start();
            //var lowLevel = salaryLevelDicts.Max(s => Convert.ToInt32(s.DICTIONARYVALUE));
            //client.GenerateSalaryLevelAsync(lowLevel, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYLEVEL tmplevel = DtGrid.SelectedItems[0] as T_HR_SALARYLEVEL;

                Form.Salary.SalaryLevelForm form = new SMT.HRM.UI.Form.Salary.SalaryLevelForm(FormTypes.Edit,tmplevel.SALARYLEVELID);

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

        void btnPreView_Click(object sender,RoutedEventArgs e)
        {
            Form.Salary.SalarySystemForm form = new SMT.HRM.UI.Form.Salary.SalarySystemForm();
           
          //  EntityBrowser browser = new EntityBrowser(form);
            //browser.FormType = FormTypes.Browse;
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
         //   browser.Show();
            form.Show();
         
          
            
            
        }


        void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            var lowLevel = salaryLevelDicts.Max(s => Convert.ToInt32(s.DICTIONARYVALUE));
           // client.GenerateSalaryLevelAsync(lowLevel, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }
        #endregion



        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYLEVEL", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SALARYLEVEL");
        }
        void browser_ReloadDataEvent()
        {
            LoadData();
        }

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
    }
}
