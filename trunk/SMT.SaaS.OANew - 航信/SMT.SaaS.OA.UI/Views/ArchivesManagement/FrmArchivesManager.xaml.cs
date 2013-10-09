using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.ArchivesManagement
{
    public partial class FrmArchivesManager : BasePage,IClient
    {
        public FrmArchivesManager()
        {
            InitializeComponent();
            this.Loaded+=new RoutedEventHandler(FrmArchivesManager_Loaded);
        }

        //private string archivesID = ""; //档案ID

        private SmtOADocumentAdminClient client;
        private ObservableCollection<string> archivesDelID ; //删除列表id
        //private CheckBox SelectBox = null;
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_ARCHIVES archivestable;

        public T_OA_ARCHIVES Archivestable
        {
            get { return archivestable; }
            set { archivestable = value; }
        }
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ViewTitles.TextTitle.Text = e.Uri.ToString();            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());            
        }
       

        

        #region  页面初始化
        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.DeleteArchivesCompleted += new EventHandler<DeleteArchivesCompletedEventArgs>(client_DeleteArchivesCompleted);
            client.GetArchivesCompleted += new EventHandler<GetArchivesCompletedEventArgs>(client_GetArchivesCompleted);            
            LoadData();
            //SetStyle();
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //FormToolBar toolbar = new FormToolBar();
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
                Archivestable = (T_OA_ARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Archivestable != null)
            {
                ArchivesAddForm form = new ArchivesAddForm(Action.Read, Archivestable.ARCHIVESID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void dgArchives_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Archivestable = (T_OA_ARCHIVES)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmArchivesManager_Loaded(object sender, RoutedEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "OAARCHIVES", true);
            InitEvent();
            GetEntityLogo("T_OA_ARCHIVES");
        }

                       

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))
            {
                filter += "RECORDTYPE ^@" + paras.Count().ToString();
                paras.Add(txtSearchType.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtSearchTitle.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ARCHIVESTITLE ^@" + paras.Count().ToString();
                paras.Add(txtSearchTitle.Text.Trim());
            }            
            loadbar.Start();

            client.GetArchivesAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        #region 頁面控件樣式
        private int iStyleCategory = 0;

        public int StyleCategory
        {
            set { this.iStyleCategory = value; }
        }

        public void SetStyle()
        {

            switch (iStyleCategory)
            {
                case -1:
                    break;
                case 0:
                    this.LayoutRoot.Style = (Style)App.Current.Resources["LayoutRootStyle"];
                    //this.cbIsReadOnly.Style = (Style)App.Current.Resources["CheckBoxStyle"];
                    //this.grsplSplitterColumn.Style = (Style)App.Current.Resources["SPGridSplitterStyle"];
                    //this.controlsToolkitTUV.Style = (Style)App.Current.Resources["FramecontrolsPagerStyle"];
                    break;
                case 1:
                    this.LayoutRoot.Style = (Style)App.Current.Resources["LayoutRootStyle1"];
                    //this.cbIsReadOnly.Style = (Style)App.Current.Resources["CheckBoxStyle1"];
                    //this.grsplSplitterColumn.Style = (Style)App.Current.Resources["SPGridSplitterStyle1"];
                    //this.controlsToolkitTUV.Style = (Style)App.Current.Resources["FramecontrolsPagerStyle"];
                    break;
            }
        }
        #endregion   
        #endregion        

        #region  完成事件
        private void client_DeleteArchivesCompleted(object sender, DeleteArchivesCompletedEventArgs e)
        {
            //try
            //{
            //    if (!string.IsNullOrEmpty(e.Result))
            //    {
            //        HtmlPage.Window.Alert(e.Result.ToString());
            //    }
            //    LoadData();
            //}
            //catch (Exception ex)
            //{
            //    HtmlPage.Window.Alert(ex.ToString());
            //}
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.errorMsg))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.errorMsg));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "ARCHIVE"));
                }
            }
            else                
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            LoadData();
        }

        private void client_GetArchivesCompleted(object sender, GetArchivesCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Result != null)
                {
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }                
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_ARCHIVES> obj,int pageCount)
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

        #region  查询按钮事件
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region  新增按钮事件

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ArchivesAddForm form = new ArchivesAddForm(Action.Add, "");
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 410;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{} ,true);
            
        }
        #endregion

        #region  修改按钮事件       
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Archivestable != null)
            {
                ArchivesAddForm form = new ArchivesAddForm(Action.Edit, Archivestable.ARCHIVESID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 410;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));                
            }
                        
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }  
        #endregion
        

        #region  删除按钮事件
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (dgArchives.SelectedItems.Count > 0)
            {

                string Result = "";
                string errorMsg = "";
                archivesDelID = new ObservableCollection<string>();
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {

                    for (int i = 0; i < dgArchives.SelectedItems.Count; i++)
                    {
                        string MeetingTypeID = "";
                        MeetingTypeID = (dgArchives.SelectedItems[i] as T_OA_ARCHIVES).ARCHIVESID;
                        if (!(archivesDelID.IndexOf(MeetingTypeID) > -1))
                        {
                            archivesDelID.Add(MeetingTypeID);
                        }
                    }
                    
                    client.DeleteArchivesAsync(archivesDelID, errorMsg);

                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //client.DeleteArchivesAsync(archivesDelID);
        }
        #endregion

        /// <summary>
        /// 获取选择项
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private ObservableCollection<string> GetSelectedItem(Action action)
        {
            if (dgArchives.ItemsSource != null)
            {
                ObservableCollection<string> selectedObj = new ObservableCollection<string>();
                foreach (object obj in dgArchives.ItemsSource)
                {
                    if (dgArchives.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = dgArchives.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (ckbSelect.IsChecked == true)
                        {
                            selectedObj.Add(ckbSelect.Tag.ToString());
                            if (action == Action.Edit)
                            {
                                break;
                            }
                        }
                        
                    }
                }
                if (selectedObj.Count > 0)
                {
                    return selectedObj;
                }
            }
            return null;
        }

        #region  新增、修改完成刷新父窗体
        private void AddFrm_ReloadDataEvent()
        {
            LoadData();
        }

        private void browser_ReloadDataEvent()
        {
            Archivestable = null;
            LoadData();
        }
        #endregion

        #region  模板列按钮事件
        
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                //archivesID.Remove(chkbox.Tag.ToString());
                T_OA_ARCHIVES archives = new T_OA_ARCHIVES();
                archives = chkbox.DataContext as T_OA_ARCHIVES;
                dgArchives.SelectedItems.Remove(archives);
                //CheckBox SelectBox = Utility.FindChildControl<CheckBox>(dgArchives, "SelectAll");
                //if (SelectBox != null && SelectBox.IsChecked == true)
                //{
                //    SelectBox.IsChecked = false;
                //}
                GridHelper.SetUnCheckAll(dgArchives);
            }
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.Value)
            {
                //archivesID.Add(chkbox.Tag.ToString());
                T_OA_ARCHIVES archives = new T_OA_ARCHIVES();
                archives = chkbox.DataContext as T_OA_ARCHIVES;
                dgArchives.SelectedItems.Add(archives);
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            //archivesID.Clear();
            GridHelper.SetUnCheckAll(dgArchives);
        }
        #endregion 

        private void dgArchives_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgArchives, e.Row, "T_OA_ARCHIVES");
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
