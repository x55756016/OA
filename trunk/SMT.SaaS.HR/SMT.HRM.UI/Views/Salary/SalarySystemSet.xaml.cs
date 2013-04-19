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
    public partial class SalarySystemSet : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private PermissionServiceClient permissionClient = new PermissionServiceClient();
        private ObservableCollection<T_SYS_DICTIONARY> postLevelDicts;
        private ObservableCollection<T_HR_POSTLEVELDISTINCTION> postLevels;
        private T_HR_SALARYSYSTEM salarySystemSelected = new T_HR_SALARYSYSTEM();
        private string Checkstate { get; set; }
        public SalarySystemSet()
        {
            InitializeComponent();
            //InitParas();
            //GetEntityLogo("T_HR_SALARYSYSTEM");
            this.Loaded += new RoutedEventHandler(SalarySystemSet_Loaded);
        }

        void SalarySystemSet_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            GetEntityLogo("T_HR_SALARYSYSTEM");
            LoadData();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_HR_SALARYSYSTEM", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

        }
        private void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.GetSalarySystemWithPagingCompleted += new EventHandler<GetSalarySystemWithPagingCompletedEventArgs>(client_GetSalarySystemWithPagingCompleted);
            client.GetPostLevelDistinctionBySystemIDCompleted += new EventHandler<GetPostLevelDistinctionBySystemIDCompletedEventArgs>(client_GetPostLevelDistinctionBySystemIDCompleted);
            client.PostLevelDistinctionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PostLevelDistinctionUpdateCompleted);
            client.GenerateSalaryLevelCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_GenerateSalaryLevelCompleted);
            client.SalarySystemDeleteCompleted += new EventHandler<SalarySystemDeleteCompletedEventArgs>(client_SalarySystemDeleteCompleted);
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //ToolBar.btnAduitNoTPass.Click += new RoutedEventHandler(btnAuitNoTPass_click);
            //ToolBar.btnSumbitAudit.Click += new RoutedEventHandler(btnSumbitAudit_click);
            //ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ImageButton btnPreView = new ImageButton();
            btnPreView.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/18_workPlace.png", Utility.GetResourceStr("PREVIEW")).Click += new RoutedEventHandler(btnPreView_Click);
            ToolBar.stpOtherAction.Children.Add(btnPreView);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);

            ImageButton btnCreatSalarySystem = new ImageButton();
            btnCreatSalarySystem.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/area/18_import.png", Utility.GetResourceStr("CREATSALARYSYSTEM")).Click += new RoutedEventHandler(btnCreatSalarySystem_Click);
            ToolBar.stpOtherAction.Children.Add(btnCreatSalarySystem);

            // ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            ToolBar.retRefresh.Visibility = Visibility.Collapsed;
            //this.Loaded += new RoutedEventHandler(AuditState_Loaded);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            // permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ///TODO: 重新提交审核 
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSYSTEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;

                Form.Salary.SalarySystemNameForm form = new SMT.HRM.UI.Form.Salary.SalarySystemNameForm(FormTypes.Resubmit, tmpEnt.SALARYSYSTEMID);

                EntityBrowser browser = new EntityBrowser(form);
                //form.MinWidth = 400;
                //form.MinHeight = 230;
                browser.FormType = FormTypes.Resubmit;
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

        void client_SalarySystemDeleteCompleted(object sender, SalarySystemDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSYSTEM"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSYSTEM"));
                LoadData();
            }
        }
        /// <summary>
        /// 生成薪资体系表完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GenerateSalaryLevelCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSED", ""),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSED", ""));
            }
            loadbar.Stop();
        }
        /// <summary>
        /// 生成岗位工资和级差额完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_PostLevelDistinctionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSED", ""),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSED", ""));
            }
            loadbar.Stop();
        }
        /// <summary>
        /// 获取所有岗位级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                return;
            }
            else
            {
                postLevelDicts = e.Result;
                //获取已设置的岗位
                client.GetPostLevelDistinctionBySystemIDAsync(salarySystemSelected.SALARYSYSTEMID);
            }
        }
        /// <summary>
        /// 获取所有的岗位薪资 和级差额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostLevelDistinctionBySystemIDCompleted(object sender, GetPostLevelDistinctionBySystemIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                postLevels = e.Result;
                if (postLevels != null)
                {
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
                }
                else
                {
                    postLevels = new ObservableCollection<T_HR_POSTLEVELDISTINCTION>();
                    foreach (var dict in postLevelDicts)
                    {

                        T_HR_POSTLEVELDISTINCTION level = new T_HR_POSTLEVELDISTINCTION();
                        level.POSTLEVEL = Convert.ToDecimal(dict.DICTIONARYVALUE);
                        level.POSTLEVELID = Guid.NewGuid().ToString();

                        postLevels.Add(level);

                    }
                }


                DtGridPostDis.ItemsSource = postLevels.OrderBy(c => c.POSTLEVEL);
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


        #region  审核状态下拉框事件
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SALARYSYSTEM");
                if (Checkstate != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    //btnCreatSalarySystem.IsEnabled = false;
                    //cbSalaryLevelA.IsEnabled = false;
                    //cbSalaryLevelB.IsEnabled = false;
                    // gdselect.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //btnCreatSalarySystem.IsEnabled = false;
                    //cbSalaryLevelA.IsEnabled = false;
                    //cbSalaryLevelB.IsEnabled = false;
                    // gdselect.Visibility = Visibility.Visible;
                }

                LoadData();
            }
        }
        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
        #endregion
        #region  薪资体系
        /// <summary>
        ///获取薪资体系
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSalarySystemWithPagingCompleted(object sender, GetSalarySystemWithPagingCompletedEventArgs e)
        {
            List<T_HR_SALARYSYSTEM> list = new List<T_HR_SALARYSYSTEM>();
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
                salarySystemSelected = list.FirstOrDefault();
                if (salarySystemSelected != null)
                {
                    //  txtSalarySystemName.DataContext = salarySystemSelected;
                    gSalarySystem.DataContext = salarySystemSelected;
                    //  BindSalaryLevel(salarySystemSelected);
                    permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
                }
                else
                {
                    DtGridPostDis.ItemsSource = null;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }
        /// <summary>
        /// 加载薪资体系列表
        /// </summary>
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
            client.GetSalarySystemWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SALARYSYSTEMID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);

        }
        /// <summary>
        /// 薪资体系表翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        /// <summary>
        /// 加载表头图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SALARYSYSTEM");
        }

        #endregion
        /// <summary>
        /// 生成岗位工资 和级差额表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreatPostLevel_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            client.PostLevelDistinctionUpdateAsync(postLevels);
        }

        /// <summary>
        /// 生成薪资体系表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreatSalarySystem_Click(object sender, RoutedEventArgs e)
        {
            if (salarySystemSelected == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTSALARYSYSTEM"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTSALARYSYSTEM"));
                return;

            }
            if (salarySystemSelected.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SALARYSYSTEMUNAPPROVED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SALARYSYSTEMUNAPPROVED"));
                return;

            }
            ///判断
            if (cbSalaryLevelA.SelectedIndex <= 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTSALARYLEVEL"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTSALARYLEVEL"));
                cbSalaryLevelA.Focus();
                return;
            }
            if (cbSalaryLevelB.SelectedIndex <= 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDSALARYLEVEL"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDSALARYLEVEL"));
                cbSalaryLevelB.Focus();
                return;
            }
            if ((cbSalaryLevelB.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE < (cbSalaryLevelA.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTSALARYLEVELBIG"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTSALARYLEVELBIG"));
                return;
            }
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            int lowSalaryLevel = Convert.ToInt32((cbSalaryLevelB.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE);
            int highSalaryLevel = Convert.ToInt32((cbSalaryLevelA.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE);
            string systemID = salarySystemSelected.SALARYSYSTEMID;
            com.OnSelectionBoxClosed += (objects, result) =>
            {
                salarySystemSelected.STARTSALARYLEVEL = highSalaryLevel;
                salarySystemSelected.ENDSALARYLEVEL = lowSalaryLevel;
                loadbar.Start();
                client.SalarySystemUpdateAsync(salarySystemSelected);
                client.GenerateSalaryLevelAsync(lowSalaryLevel, highSalaryLevel, systemID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            };
            com.SelectionBox(Utility.GetResourceStr("SALARYSYSTEMSET"), Utility.GetResourceStr("SALARYSYSTEMDESC"), ComfirmWindow.confirmation, Result);
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {

            Form.Salary.SalarySystemNameForm form = new SMT.HRM.UI.Form.Salary.SalarySystemNameForm(FormTypes.New, null);

            EntityBrowser browser = new EntityBrowser(form);
            //form.MinWidth =400;
            //form.MinHeight = 230;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // client.PostLevelDistinctionUpdateAsync(postLevels);
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSYSTEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;
                if (tmpEnt.CHECKSTATE == "0")
                {
                    Form.Salary.SalarySystemNameForm form = new SMT.HRM.UI.Form.Salary.SalarySystemNameForm(FormTypes.Edit, tmpEnt.SALARYSYSTEMID);

                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    //form.MinWidth = 400;
                    //form.MinHeight = 230;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTEDIT"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }
        //void BtnView_Click(object sender, RoutedEventArgs e)
        //{
        //    // client.PostLevelDistinctionUpdateAsync(postLevels);
        //    if (DtGrid.SelectedItems.Count > 0)
        //    {
        //        T_HR_SALARYSYSTEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;

        //        Form.Salary.SalarySystemForm form = new SMT.HRM.UI.Form.Salary.SalarySystemForm(tmpEnt.SALARYSYSTEMID);

        //        EntityBrowser browser = new EntityBrowser(form);
        //        browser.TitleContent = Utility.GetResourceStr("SALARYSYSTEMTABEL");
        //        //form.MinWidth = 400;
        //        //form.MinHeight = 230;
        //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //        browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
        //        return;
        //    }
        //}
        void btnPreView_Click(object sender, RoutedEventArgs e)
        {
            // client.PostLevelDistinctionUpdateAsync(postLevels);
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSYSTEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;

                Form.Salary.SalarySystemForm form = new SMT.HRM.UI.Form.Salary.SalarySystemForm(tmpEnt.SALARYSYSTEMID);
                //form.Show();
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                // browser.FormType = FormTypes.Edit; 
                //browser.EntityEditor.v
                browser.TitleContent = Utility.GetResourceStr("SALARYSYSTEMTABEL");
                //form.MinWidth = 400;
                //form.MinHeight = 230;

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "PREVIEW"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "PREVIEW"));
                return;
            }
        }
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            // client.PostLevelDistinctionUpdateAsync(postLevels);
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSYSTEM tmpEnt = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;

                Form.Salary.SalarySystemNameForm form = new SMT.HRM.UI.Form.Salary.SalarySystemNameForm(FormTypes.Audit, tmpEnt.SALARYSYSTEMID);

                EntityBrowser browser = new EntityBrowser(form);
                //form.MinWidth = 400;
                //form.MinHeight = 230;
                browser.FormType = FormTypes.Audit;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }
        }
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                T_HR_SALARYSYSTEM temp = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;
                if (!(temp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString()))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);                    
                    return;
                }
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();
                    bool flag = false;
                    foreach (T_HR_SALARYSYSTEM tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);                            
                            flag = true;
                            break;
                        }
                        ids.Add(tmp.SALARYSYSTEMID);
                    }
                    if (flag == true)
                    {
                        return;
                    }
                    client.SalarySystemDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }
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

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DtGrid.SelectedItems.Count > 0)
            {
                salarySystemSelected = DtGrid.SelectedItems[0] as T_HR_SALARYSYSTEM;
                // txtSalarySystemName.DataContext = salarySystemSelected;
                gSalarySystem.DataContext = salarySystemSelected;
                //  BindSalaryLevel(salarySystemSelected);
                permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
            }
        }
        void BindSalaryLevel(T_HR_SALARYSYSTEM system)
        {
            foreach (T_SYS_DICTIONARY item in cbSalaryLevelA.Items)
            {
                if (item.DICTIONARYVALUE == system.STARTSALARYLEVEL)
                {
                    cbSalaryLevelA.SelectedItem = item;
                    break;
                }
            }
            foreach (T_SYS_DICTIONARY item in cbSalaryLevelB.Items)
            {
                if (item.DICTIONARYVALUE == system.ENDSALARYLEVEL)
                {
                    cbSalaryLevelB.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
