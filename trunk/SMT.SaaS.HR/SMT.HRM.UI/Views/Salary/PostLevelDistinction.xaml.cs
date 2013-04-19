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
    public partial class PostLevelDistinction : BasePage
    {
        public PostLevelDistinction()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PostLevelDistinction_Loaded);
            //InitParas();
            //GetEntityLogo("T_HR_POSTLEVELDISTINCTION");
        }

        void PostLevelDistinction_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            GetEntityLogo("T_HR_POSTLEVELDISTINCTION");
        }
        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permissionClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();

        private ObservableCollection<T_SYS_DICTIONARY> postLevelDicts;
        private ObservableCollection<T_HR_POSTLEVELDISTINCTION> postLevels;

        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();

            client.GetAllPostLevelDistinctionCompleted += new EventHandler<SMT.Saas.Tools.SalaryWS.GetAllPostLevelDistinctionCompletedEventArgs>(client_GetAllPostLevelDistinctionCompleted);
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);

            client.PostLevelDistinctionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PostLevelDistinctionUpdateCompleted);
            //获取字典中的岗位级别
            permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");


            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
           // ToolBar.btnNew.Visibility = Visibility.Collapsed;


            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
          
                Form.Salary.PostSalaryForm form = new SMT.HRM.UI.Form.Salary.PostSalaryForm(FormTypes.New, null);

                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 570;
                form.MinHeight = 220;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
           
        }

        void client_PostLevelDistinctionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "POSTLEVEL"));
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           // client.PostLevelDistinctionUpdateAsync(postLevels);
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_POSTLEVELDISTINCTION tmpEnt = DtGrid.SelectedItems[0] as T_HR_POSTLEVELDISTINCTION;

                Form.Salary.PostSalaryForm form = new SMT.HRM.UI.Form.Salary.PostSalaryForm(FormTypes.Edit, tmpEnt.POSTLEVELID);

                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 570;
                form.MinHeight = 220;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }


        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                postLevelDicts = e.Result;
                //获取已设置的岗位
                client.GetAllPostLevelDistinctionAsync();
            }

        }

        void client_GetAllPostLevelDistinctionCompleted(object sender, SMT.Saas.Tools.SalaryWS.GetAllPostLevelDistinctionCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

            }
            else
            {
                postLevels = e.Result;
                foreach (var dict in postLevelDicts)
                {
                    if (!IsLevelAdded(dict))
                    {
                        T_HR_POSTLEVELDISTINCTION level = new T_HR_POSTLEVELDISTINCTION();
                        level.POSTLEVEL = Convert.ToDecimal(dict.DICTIONARYVALUE);
                        level.POSTLEVELID = Guid.NewGuid().ToString();

                        postLevels.Add(level);
                    }
                }

                DtGrid.ItemsSource = postLevels;
            }

            //client.GetAllSalaryLevelAsync();
            loadbar.Stop();
        }
        private bool IsLevelAdded(T_SYS_DICTIONARY dict)
        {
            bool rslt = false;

            var ents = from p in postLevels
                       where p.POSTLEVEL == Convert.ToDecimal(dict.DICTIONARYVALUE)
                       select p;
            rslt = ents.Count() > 0;
            return rslt;
        }
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_POSTLEVELDISTINCTION", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_POSTLEVELDISTINCTION");
        }
        void browser_ReloadDataEvent()
        {
            permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
        }
    }
}
