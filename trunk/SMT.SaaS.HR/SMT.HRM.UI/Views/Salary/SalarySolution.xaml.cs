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
    public partial class SalarySolution : BasePage, IClient
    {
        private SalaryServiceClient client = new SalaryServiceClient();
        private string Checkstate { get; set; }
        SMTLoading loadbar = new SMTLoading();
        Dictionary<string, string> postlevels;
        public SalarySolution()
        {
            InitializeComponent();
            //InitParas();
            //GetEntityLogo("T_HR_SALARYSOLUTION");
            //LoadPostLevel();
            this.Loaded += new RoutedEventHandler(SalarySolution_Loaded);
        }

        void SalarySolution_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            GetEntityLogo("T_HR_SALARYSOLUTION");
            LoadPostLevel();
            LoadData();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYSOLUTION", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

        }
        void LoadPostLevel()
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            postlevels = new Dictionary<string, string>();
            var ents = from c in dicts
                       where c.DICTIONCATEGORY == "POSTLEVEL"
                       select c;

            #region  未验证
            if (ents.Count() > 0)
            {
                ents = ents.OrderBy(n => n.DICTIONARYVALUE);
            }
            #endregion

            foreach (var item in ents)
            {
                postlevels.Add(item.DICTIONARYNAME, item.DICTIONARYVALUE.ToString());
            }
        }
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.SalarySolutionDeleteCompleted += new EventHandler<SalarySolutionDeleteCompletedEventArgs>(client_SalarySolutionDeleteCompleted);
            client.GetSalarySolutionPagingCompleted += new EventHandler<GetSalarySolutionPagingCompletedEventArgs>(client_GetSalarySolutionPagingCompleted);
            client.CreateSalaryStandBatchCompleted += new EventHandler<CreateSalaryStandBatchCompletedEventArgs>(client_CreateSalaryStandBatchCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ImageButton btnCreate = new ImageButton();
            btnCreate.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("BATCHCREATESTAND")).Click += new RoutedEventHandler(btnCreate_Click);
            ToolBar.stpOtherAction.Children.Add(btnCreate);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);

        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSOLUTION tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSOLUTION;
                Form.Salary.SalarySolutionForm form = new Form.Salary.SalarySolutionForm(FormTypes.Resubmit, tmpEnt.SALARYSOLUTIONID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Resubmit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }

        void client_CreateSalaryStandBatchCompleted(object sender, CreateSalaryStandBatchCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == "SAVESUCCESSED")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSTANDARD"));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }

                LoadData();
            }
            this.IsEnabled = true;
            loadbar.Stop();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSOLUTION tmpSolution = DtGrid.SelectedItems[0] as T_HR_SALARYSOLUTION;

                Form.Salary.SalarySolutionForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionForm(FormTypes.Browse, tmpSolution.SALARYSOLUTIONID);
                form.SolutionItemWinForm.ToolBar.IsEnabled = false;
                form.SalaryTaxesWinForm.IsEnabled = false;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SALARYSOLUTION");
                LoadData();
            }

            //ToolBar.btnEdit.Visibility = Visibility.Visible;
            //ToolBar.btnNew.Visibility = Visibility.Visible;
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }

        void client_GetSalarySolutionPagingCompleted(object sender, GetSalarySolutionPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSOLUTION> list = new List<T_HR_SALARYSOLUTION>();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

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

        void client_SalarySolutionDeleteCompleted(object sender, SalarySolutionDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSOLUTION"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSOLUTION"));
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

        /// <summary>
        /// 新增方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.SalarySolutionForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionForm(FormTypes.New, "");

            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        /// <summary>
        /// 加载方案集合
        /// </summary>
        private void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            string strState = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            if (txtName != null && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                //filter += "SALARYSOLUTIONNAME==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(SALARYSOLUTIONNAME)";
                paras.Add(txtName.Text.Trim());
            }
            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            client.GetSalarySolutionPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYSOLUTIONNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);

        }
        /// <summary>
        /// 修改方案 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSOLUTION tmpSolution = DtGrid.SelectedItems[0] as T_HR_SALARYSOLUTION;

                Form.Salary.SalarySolutionForm form = new SMT.HRM.UI.Form.Salary.SalarySolutionForm(FormTypes.Edit, tmpSolution.SALARYSOLUTIONID);

                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Edit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }
        /// <summary>
        /// 删除方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ObservableCollection<string> ids = new ObservableCollection<string>();

                foreach (T_HR_SALARYSOLUTION tmp in DtGrid.SelectedItems)
                {
                    if (!(tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() || tmp.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString()))
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTDELETE"));
                        return;
                    }
                    ids.Add(tmp.SALARYSOLUTIONID);
                }


                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.SalarySolutionDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
                return;
            }
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSOLUTION tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSOLUTION;
                Form.Salary.SalarySolutionForm form = new Form.Salary.SalarySolutionForm(FormTypes.Audit, tmpEnt.SALARYSOLUTIONID);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                return;
            }
        }
        /// <summary>
        /// 批量生成标准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            ///TODO:ADD 审核  
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSOLUTION tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSOLUTION;
                if (tmpEnt.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    T_HR_SALARYSTANDARD standtmp = new T_HR_SALARYSTANDARD();
                    standtmp.SALARYSTANDARDID = Guid.NewGuid().ToString();
                    standtmp.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    standtmp.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    standtmp.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    standtmp.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    standtmp.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    standtmp.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    standtmp.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    standtmp.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    loadbar.Start();
                    client.CreateSalaryStandBatchAsync(tmpEnt, postlevels, standtmp);
                    this.IsEnabled = false;
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SALARYSOLUTIONUNAPPROVED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SALARYSOLUTIONUNAPPROVED"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "BATCHCREATESTAND"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "BATCHCREATESTAND"));
                return;
            }
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

        #endregion
        /// <summary>
        /// 加载表头图标 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SALARYSOLUTION");
        }
    }
}
