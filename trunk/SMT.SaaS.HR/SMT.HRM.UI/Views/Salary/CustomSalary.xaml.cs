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

using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class CustomSalary : BasePage,IClient
    {
        SalaryServiceClient client;
        SMTLoading loadbar = new SMTLoading();
        private ObservableCollection<string> customSalaryIDs = new ObservableCollection<string>();
        private string Checkstate { get; set; }
        public T_HR_CUSTOMGUERDONSET SelectID { get; set; }
        public ObservableCollection<string> CustomSalaryIDs
        {
            get { return customSalaryIDs; }
            set { customSalaryIDs = value; }
        } 

        public CustomSalary()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(CustomSalary_Loaded);
            //client = new SalaryServiceClient();
            //InitPara();
            //GetEntityLogo("T_HR_CUSTOMGUERDONSET");
        }

        void CustomSalary_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
            GetEntityLogo("T_HR_CUSTOMGUERDONSET");
        }

        public void InitPara()
        {
            PARENT.Children.Add(loadbar);
            try
            { 
                client = new SalaryServiceClient();
                client.GetCustomGuerdonSetPagingCompleted += new EventHandler<GetCustomGuerdonSetPagingCompletedEventArgs>(client_GetCustomGuerdonSetPagingCompleted);
                client.CustomGuerdonSetDeleteCompleted += new EventHandler<CustomGuerdonSetDeleteCompletedEventArgs>(client_CustomGuerdonSetDeleteCompleted);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                loadbar.Stop();
            }

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核     
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET;
                CustomSalaryForm form = new Form.Salary.CustomSalaryForm(FormTypes.Resubmit, tmpEnt.CUSTOMGUERDONSETID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONSET tmpStandard = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET;

                if (customSalaryIDs.Count <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                CustomSalaryForm form = new CustomSalaryForm(FormTypes.Browse, customSalaryIDs[0]);
                form.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinHeight = 240;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void client_GetCustomGuerdonSetPagingCompleted(object sender, GetCustomGuerdonSetPagingCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                List<T_HR_CUSTOMGUERDONSET> list = new List<T_HR_CUSTOMGUERDONSET>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            loadbar.Stop();
        }

        void client_CustomGuerdonSetDeleteCompleted(object sender, CustomGuerdonSetDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "CUSTOMSALARY"));
                LoadData();
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
           LoadData();
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择某审核状态是重新加载数据

            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_CUSTOMGUERDONRECORD");
                Checkstate = dict.DICTIONARYVALUE.ToString();

                #region  重新提交审核
                //if (dict.DICTIONARYVALUE.Value.ToInt32() == Convert.ToInt32(CheckStates.Approved)) 
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    ToolBar.btnReSubmit.Visibility = Visibility.Collapsed;
                //}
                #endregion

                LoadData();
            }  
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_CUSTOMGUERDONSET", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            customSalaryIDs.Clear();
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectID = grid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET;
                customSalaryIDs.Add((grid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET).CUSTOMGUERDONSETID);
            }

        }

        #region 添加,修改,删除,查询,审核


        void btnNew_Click(object sender, RoutedEventArgs e)
        {            
            CustomSalaryForm form = new CustomSalaryForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(form);
            
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            form.MinHeight = 240;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            loadbar.Start();
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            string strState = "";
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += "GUERDONNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(GUERDONNAME)";
                paras.Add(txtName.Text.Trim());
            }
            //if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "CHECKSTATE==@" + paras.Count().ToString();
            //    paras.Add(Checkstate);
            //}
            client.GetCustomGuerdonSetPagingAsync(dataPager.PageIndex, dataPager.PageSize, "GUERDONNAME", filter, paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONSET tmpStandard = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET;
                if (customSalaryIDs.Count <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                    return;
                }
                //CustomSalaryForm form = new CustomSalaryForm(FormTypes.Edit, tmpStandard.CUSTOMGUERDONSETID);
                CustomSalaryForm form = new CustomSalaryForm(FormTypes.Edit, customSalaryIDs[0]);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit; 
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                form.MinHeight = 240;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
            }
        }
         
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //ComfirmBox deleComfir = new ComfirmBox();
            //deleComfir.Title = Utility.GetResourceStr("DELETECONFIRM");
            //deleComfir.MessageTextBox.Text = Utility.GetResourceStr("DELETEALTER");
            //deleComfir.ButtonOK.Click += new RoutedEventHandler(ButtonOK1_Click);
            //deleComfir.Show();

            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_CUSTOMGUERDONSET tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.CUSTOMGUERDONSETID);
                    if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.CustomGuerdonSetDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }

        }

        void ButtonOK1_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_CUSTOMGUERDONSET tmp in DtGrid.SelectedItems)
                {
                    ids.Add(tmp.CUSTOMGUERDONSETID);
                }
                client.CustomGuerdonSetDeleteAsync(ids);
            }

        }


        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核     
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_CUSTOMGUERDONSET tmpEnt = DtGrid.SelectedItems[0] as T_HR_CUSTOMGUERDONSET;
                CustomSalaryForm form = new Form.Salary.CustomSalaryForm(FormTypes.Audit, tmpEnt.CUSTOMGUERDONSETID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else 
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
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
            SetRowLogo(DtGrid, e.Row, "T_HR_CUSTOMGUERDONSET");
        }

    }
}
