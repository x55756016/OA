/// <summary>
/// Log No.： 1
/// Modify Desc： 显示和字典转换打分数据，审核按钮
/// Modifier： 冉龙军
/// Modify Date： 2010-08-03
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


using SMT.Saas.Tools.PerformanceWS;
using SMT.HRM.UI.Form.Performance;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
// 1s 冉龙军
// Form中修改DataGrid的SYSTEMSCORE，MANUALSCORE，RANDOMSCORE的字段绑定
// 1e
namespace SMT.HRM.UI.Views.Performance
{
    public partial class KPITypeList : BasePage
    {
        private PerformanceServiceClient client = new PerformanceServiceClient();
        SMTLoading loadbar = new SMTLoading();
        public T_HR_KPITYPE SelectedKPIType { get; set; }

        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }

        public KPITypeList()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_HR_KPITYPE");
        }

        void KPIType_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            // 1s 冉龙军
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_KPITYPE", false);//KPIPointSet
            // 1e
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "KPITypeList", false);//KPIPointSet
            // 1e
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public void InitPara()
        {
            try
            {
                PARENT.Children.Add(loadbar);
                loadbar.Start();
                //DtGrid.CurrentCellChanged += new EventHandler<DataGridRowEventArgs>(DtGrid_CurrentCellChanged);

                client.GetKPITypePagingCompleted += new EventHandler<GetKPITypePagingCompletedEventArgs>(client_GetKPITypePagingCompleted);
                client.DeleteKPITypesCompleted += new EventHandler<DeleteKPITypesCompletedEventArgs>(client_DeleteKPITypesCompleted);

                this.Loaded += new RoutedEventHandler(KPIType_Loaded);
                this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
                this.ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDel_Click);
                this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                this.ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);
                // 1s 冉龙军
                //this.ToolBar.retNew.Visibility = Visibility.Collapsed;
                //this.ToolBar.btnAudit.Visibility = Visibility.Collapsed;
                // 1e
                //this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
        }



        void client_GetKPITypePagingCompleted(object sender, GetKPITypePagingCompletedEventArgs e)
        {
            List<T_HR_KPITYPE> list = new List<T_HR_KPITYPE>();
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
                else
                {
                    list = null;
                }

                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        void client_DeleteKPITypesCompleted(object sender, DeleteKPITypesCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if(!string.IsNullOrEmpty(e.strMsg))
                {
                    string msg = e.strMsg.Substring(0, e.strMsg.Length - 1) + Utility.GetResourceStr("RELATIONED");
                   // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(msg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(msg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtKPITypeName = Utility.FindChildControl<TextBox>(expander, "txtKPITypeName");
            if (!string.IsNullOrEmpty(txtKPITypeName.Text.Trim()))
            {
                //  filter += "KPITYPENAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(KPITYPENAME)";
                paras.Add(txtKPITypeName.Text.Trim());
            }
            client.GetKPITypePagingAsync(dataPager.PageIndex, dataPager.PageSize, "KPITYPEID", filter, paras, pageCount,SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            KPITypeInfo form = new KPITypeInfo(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 680;
            form.MinHeight = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedKPIType != null)
            {
                KPITypeInfo form = new KPITypeInfo(FormTypes.Browse, SelectedKPIType.KPITYPEID);
                EntityBrowser browser = new EntityBrowser(form);
                // 1s 冉龙军
                browser.FormType = FormTypes.Browse;
                // 1e
                form.MinWidth = 680;
                form.MinHeight = 500;
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
            if (SelectedKPIType != null)
            {
                KPITypeInfo form = new KPITypeInfo(FormTypes.Edit, SelectedKPIType.KPITYPEID);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 680;
                form.MinHeight = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            string strMsg = string.Empty;
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (T_HR_KPITYPE tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.KPITYPEID);
                    }
                    client.DeleteKPITypesAsync(ids, strMsg);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_KPITYPE");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false  ;
            LoadData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectedKPIType = (T_HR_KPITYPE)grid.SelectedItems[0];
            }
        }
    }
}
