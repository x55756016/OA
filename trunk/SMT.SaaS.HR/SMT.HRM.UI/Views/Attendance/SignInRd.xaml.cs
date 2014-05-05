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

using SMT.HRM.UI.Form.Attendance;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.OrganizationControl;


namespace SMT.HRM.UI.Views.Attendance
{
    public partial class SignInRd : BasePage, IClient
    {
        #region 全局变量

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        public string Checkstate { get; set; }
        private SMTLoading loadbar = new SMTLoading();
         private SaveFileDialog dialog = new SaveFileDialog();
         private bool? result;
        #endregion

        #region 初始化
        public SignInRd()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SignInRd_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            nuYear.Value = DateTime.Now.Year;
            nuMonth.Value = DateTime.Now.Month;

            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.btnOutExcel.Visibility = Visibility;
            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);//导出签卡明细
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            clientAtt.EmployeeSignInRecordPagingCompleted += new EventHandler<EmployeeSignInRecordPagingCompletedEventArgs>(client_EmployeeSignInRecordPagingCompleted);
            clientAtt.EmployeeSigninRecordDeleteCompleted += new EventHandler<EmployeeSigninRecordDeleteCompletedEventArgs>(client_EmployeeSigninRecordDeleteCompleted);
            clientAtt.ExportEmployeeSignInCompleted += new EventHandler<ExportEmployeeSignInCompletedEventArgs>(clientAtt_ExportEmployeeSignInCompleted);
        }

        /// <summary>
        /// 导出单条员工异常签卡明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {

            //ExportToCSV.ExportDataGridSaveAs(dgSignInList);
            //return;

            try
            {
                string strSignInID = string.Empty;
                if (dgSignInList.SelectedItems == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTONE"));
                    return;
                }
                if (dgSignInList.SelectedItems.Count == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTONE"));
                    return;
                }
                T_HR_EMPLOYEESIGNINRECORD entSignInRd = dgSignInList.SelectedItems[0] as T_HR_EMPLOYEESIGNINRECORD;
                strSignInID = entSignInRd.SIGNINID;
                dialog.Filter = "MS Excel Files|*.xls";
                dialog.FilterIndex = 1;
                result = dialog.ShowDialog();
                clientAtt.ExportEmployeeSignInAsync(strSignInID);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 导出单条员工异常签卡明细完成时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ExportEmployeeSignInCompleted(object sender, ExportEmployeeSignInCompletedEventArgs e)
        {
            try
            {
                if (result == true)
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null)
                        {
                            using (System.IO.Stream stream = dialog.OpenFile())
                            {
                                stream.Write(e.Result, 0, e.Result.Length);
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            }
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.ToString(), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        private void InitPage()
        {
            BindComboxBox();
        }

        /// <summary>
        /// 加载审核状态列表
        /// </summary>
        private void BindComboxBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            }
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            string recorderDate = string.Empty;

            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "EMPLOYEEID==@" + paras.Count().ToString();
                    paras.Add(ent.EMPLOYEEID);
                }
            }

            recorderDate = nuYear.Value.ToString() + "-" + nuMonth.Value.ToString() + "-1";
            if (DateTime.Parse(recorderDate) <= DateTime.Parse("1900-1-1"))
            {
                recorderDate = string.Empty;
            }

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                Checkstate = entDic.DICTIONARYVALUE.ToString();
            }

            clientAtt.EmployeeSignInRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SIGNINTIME", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, recorderDate);
            loadbar.Start();
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void browser_ReloadDataEvent()
        {
            BindGrid();
        }
        #endregion

        #region 事件

        void SignInRd_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            GetEntityLogo("T_HR_EMPLOYEESIGNINRECORD");
            InitPage();
        }

        /// <summary>
        /// 加载签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeSignInRecordPagingCompleted(object sender, EmployeeSignInRecordPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEESIGNINRECORD> list = new List<T_HR_EMPLOYEESIGNINRECORD>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgSignInList.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 返回删除记录后的消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeSigninRecordDeleteCompleted(object sender, EmployeeSigninRecordDeleteCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEESIGNINRECORD"));
                BindGrid();
            }
        }

        /// <summary>
        /// 根据审核状态下拉列表选择项，控制签卡记录显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_EMPLOYEESIGNINRECORD");
                BindGrid();
            }
        }

        /// <summary>
        /// 选取员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();
            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;

            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 查询签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 对签卡记录进行分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSignInList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgSignInList, e.Row, "T_HR_EMPLOYEESIGNINRECORD");
        }

        /// <summary>
        /// 弹出表单子窗口，以便新增签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strSignInID = string.Empty;
            SignInRdForm formSignInRd = new SignInRdForm(FormTypes.New, strSignInID);
            EntityBrowser entBrowser = new EntityBrowser(formSignInRd);
            entBrowser.FormType = FormTypes.New;

            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        ///  刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strSignInID = string.Empty;
            if (dgSignInList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }

            if (dgSignInList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = dgSignInList.SelectedItems[0] as T_HR_EMPLOYEESIGNINRECORD;
            strSignInID = entSignInRd.SIGNINID;
            SignInRdForm formSignInRd = new SignInRdForm(FormTypes.Resubmit, strSignInID);

            EntityBrowser entBrowser = new EntityBrowser(formSignInRd);

            entBrowser.FormType = FormTypes.Resubmit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strSignInID = string.Empty;
            if (dgSignInList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"));
                return;
            }

            if (dgSignInList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"));
                return;
            }

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = dgSignInList.SelectedItems[0] as T_HR_EMPLOYEESIGNINRECORD;
            strSignInID = entSignInRd.SIGNINID;
            SignInRdForm formSignInRd = new SignInRdForm(FormTypes.Browse, strSignInID);

            EntityBrowser entBrowser = new EntityBrowser(formSignInRd);

            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strSignInID = string.Empty;
            if (dgSignInList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

            if (dgSignInList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "EDIT"));
                return;
            }

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = dgSignInList.SelectedItems[0] as T_HR_EMPLOYEESIGNINRECORD;

            //if (entSignInRd.OWNERID != SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "只能修改自己的单据.");
            //    return;
            //}
            if (entSignInRd.CHECKSTATE != "0")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "只能修改未提交的单据.");
                return;
            }


            strSignInID = entSignInRd.SIGNINID;
            SignInRdForm formSignInRd = new SignInRdForm(FormTypes.Edit, strSignInID);

            EntityBrowser entBrowser = new EntityBrowser(formSignInRd);
            entBrowser.FormType = FormTypes.Edit;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 删除指定的签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSignInList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSignInList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (object ovj in dgSignInList.SelectedItems)
            {
                T_HR_EMPLOYEESIGNINRECORD ent = ovj as T_HR_EMPLOYEESIGNINRECORD;

                if (ent == null)
                {
                    continue;
                }

                if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEAUDITERROR"));
                    break;
                }
            
                ids.Add(ent.SIGNINID);
            }

            string Result = "";
            ComfirmWindow delComfirm = new ComfirmWindow();
            delComfirm.OnSelectionBoxClosed += (obj, result) =>
            {
                clientAtt.EmployeeSigninRecordDeleteAsync(ids);
            };
            delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strSignInID = string.Empty;
            if (dgSignInList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"));
                return;
            }

            if (dgSignInList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"));
                return;
            }

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = dgSignInList.SelectedItems[0] as T_HR_EMPLOYEESIGNINRECORD;
            strSignInID = entSignInRd.SIGNINID;
            SignInRdForm formSignInRd = new SignInRdForm(FormTypes.Audit, strSignInID);
            EntityBrowser browser = new EntityBrowser(formSignInRd);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.FormType = FormTypes.Audit;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            clientAtt.DoClose();
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
