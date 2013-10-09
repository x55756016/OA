using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.UserControls;

namespace SMT.SaaS.OA.UI.Views.ArchivesManagement
{
    public partial class FrmArchivesReturn : BasePage,IClient
    {
        private SmtOADocumentAdminClient client;
        //private ObservableCollection<string> lendingID = new ObservableCollection<string>();

        //public ObservableCollection<string> LendingID
        //{
        //    get { return lendingID; }
        //    set { lendingID = value; }
        //}
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_LENDARCHIVES lendarchives;

        public T_OA_LENDARCHIVES Lendarchives
        {
            get { return lendarchives; }
            set { lendarchives = value; }
        }
        public FrmArchivesReturn()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FrmArchivesReturn_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetReturnListByUserIdCompleted += new EventHandler<GetReturnListByUserIdCompletedEventArgs>(client_GetReturnListByUserIdCompleted);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            ToolBar.retEdit.Visibility = Visibility.Collapsed;
            ToolBar.retDelete.Visibility = Visibility.Collapsed;
            ToolBar.retRead.Visibility = Visibility.Collapsed;
            ToolBar.retAudit.Visibility = Visibility.Collapsed;            
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406in.png", Utility.GetResourceStr("RETURN")).Click += new RoutedEventHandler(FrmArchivesReturn_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            LoadData();
            //dgArchives.CurrentCellChanged += new EventHandler<EventArgs>(dgArchives_CurrentCellChanged);
            dgArchives.SelectionChanged += new SelectionChangedEventHandler(dgArchives_SelectionChanged);
            ToolBar.ShowRect();
        }

        void dgArchives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            if (grid.SelectedItems.Count > 0 )
            {
                Lendarchives = (T_OA_LENDARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {
                ArchivesLendingForm form = new ArchivesLendingForm(Action.Read, Lendarchives.LENDARCHIVESID, "2");// 归还为审核通过 所以传值 2
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinWidth = 445;
                browser.MinHeight = 310;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Lendarchives = null;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void dgArchives_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Lendarchives = (T_OA_LENDARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmArchivesReturn_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_LENDARCHIVES", true);
            PARENT.Children.Add(loadbar);
            InitEvent();
            GetEntityLogo("T_OA_LENDARCHIVES");
        }
       
        //归还
        private void FrmArchivesReturn_Click(object sender, RoutedEventArgs e)
        {
            if (Lendarchives != null)
            {
                ArchivesReturnForm form = new ArchivesReturnForm(Lendarchives.LENDARCHIVESID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.Width = 450;
                browser.Height = 280;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                Lendarchives = null;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "RETURN"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值            
            if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_ARCHIVES.RECORDTYPE ^@" + paras.Count().ToString();
                paras.Add(txtSearchType.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtSearchTitle.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_ARCHIVES.ARCHIVESTITLE ^@" + paras.Count().ToString();
                paras.Add(txtSearchTitle.Text.Trim());
            }           
            loadbar.Start();
            client.GetReturnListByUserIdAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);   
        }


        void client_AddArchivesReturnCompleted(object sender, AddArchivesReturnCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("归还档案成功！");
                        //client.GetReturnListByUserIdAsync("admin","","");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "RETURNARCHIVE"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private void client_GetReturnListByUserIdCompleted(object sender, GetReturnListByUserIdCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Result != null)
                {
                    //if (e.Result.Count > 0)
                    //{
                    BindDataGrid(e.Result.ToList(),e.pageCount);
                    //}                    
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_LENDARCHIVES> obj, int pageCount)
        {

            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                dgArchives.ItemsSource = null;
                return;
            }
            dgArchives.ItemsSource = obj;
        }
        #endregion


        #region  按钮事件
        private void checkbox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            //client.GetReturnListByUserIdAsync("admin", this.txtSearchTitle.Text.Trim(), txtSearchType.Text.Trim());
            LoadData();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void browser_ReloadDataEvent()
        {
            //throw new NotImplementedException();
            LoadData();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Lendarchives = null;
            LoadData();
        }        


        private void checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (!chkbox.IsChecked.Value)
            //{
            //    lendingID.Remove(chkbox.Tag.ToString());
            //}
        }

        private void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            //CheckBox chkbox = sender as CheckBox;
            //if (chkbox.IsChecked.Value)
            //{
            //    lendingID.Add(chkbox.Tag.ToString());
            //}
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region IClient 成员

        public void ClosedWCFClient()
        {
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
