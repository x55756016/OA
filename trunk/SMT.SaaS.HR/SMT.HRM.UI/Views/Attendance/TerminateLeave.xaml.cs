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
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.OrganizationControl;
namespace SMT.HRM.UI.Views.Attendance
{
    public partial class TerminateLeave : BasePage, IClient
    {

        #region 全局变量
        public string Checkstate { get; set; }
        AttendanceServiceClient client = new AttendanceServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public TerminateLeave()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeLeaveRecord_Loaded);
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
            client.EmployeeCancelLeavePagingCompleted += new EventHandler<EmployeeCancelLeavePagingCompletedEventArgs>(client_EmployeeCancelLeavePagingCompleted);
            client.EmployeeCancelLeaveDeleteCompleted += new EventHandler<EmployeeCancelLeaveDeleteCompletedEventArgs>(client_EmployeeCancelLeaveDeleteCompleted);

            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }


            T_HR_EMPLOYEECANCELLEAVE tmpEnt = dgEmpLeaveRdList.SelectedItems[0] as T_HR_EMPLOYEECANCELLEAVE;

            TerminateLeaveForm form = new TerminateLeaveForm(FormTypes.Resubmit, tmpEnt.CANCELLEAVEID);
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.FormType = FormTypes.Resubmit;
            entBrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });            //BindGrid();
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

            client.EmployeeCancelLeavePagingAsync(dataPager.PageIndex, dataPager.PageSize, "STARTDATETIME", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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

        void EmployeeLeaveRecord_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_EMPLOYEECANCELLEAVE");
            RegisterEvents();
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_EMPLOYEECANCELLEAVE", true);
            BindComboxBox();
        }
        /// <summary>
        /// 加载销假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeCancelLeavePagingCompleted(object sender, EmployeeCancelLeavePagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_EMPLOYEECANCELLEAVE> list = new List<T_HR_EMPLOYEECANCELLEAVE>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgEmpLeaveRdList.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 删除完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeCancelLeaveDeleteCompleted(object sender, EmployeeCancelLeaveDeleteCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEECANCELLEAVE"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            BindGrid();
        }

        /// <summary>
        /// 根据审核状态下拉列表选择项，控制请假记录显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_EMPLOYEECANCELLEAVE");
                BindGrid();
            }
        }

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
        /// 查询，分页
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
        private void dgEmpLeaveRdList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmpLeaveRdList, e.Row, "T_HR_EMPLOYEECANCELLEAVE");
        }

        #region 添加，修改，删除
        /// <summary>
        /// 弹出表单子窗口，以便新增请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            TerminateLeaveForm form = new TerminateLeaveForm(FormTypes.New, "");
            EntityBrowser entBrowser = new EntityBrowser(form);
            entBrowser.FormType = FormTypes.New;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            T_HR_EMPLOYEECANCELLEAVE tmpEnt = dgEmpLeaveRdList.SelectedItems[0] as T_HR_EMPLOYEECANCELLEAVE;

            TerminateLeaveForm form = new TerminateLeaveForm(FormTypes.Edit, tmpEnt.CANCELLEAVEID);
            EntityBrowser entBrowser = new EntityBrowser(form);
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
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (object ovj in dgEmpLeaveRdList.SelectedItems)
            {
                T_HR_EMPLOYEECANCELLEAVE ent = ovj as T_HR_EMPLOYEECANCELLEAVE;
                if (ent == null)
                {
                    continue;
                }

                if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEAUDITERROR"));
                    break;
                }

                ids.Add(ent.CANCELLEAVEID);
            }

            string Result = "";
            if (ids.Count > 0)
            {
                ComfirmWindow delComfirm = new ComfirmWindow();
                delComfirm.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.EmployeeCancelLeaveDeleteAsync(ids);
                };
                delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strEvectionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_EMPLOYEECANCELLEAVE entEvectionRd = dgEmpLeaveRdList.SelectedItems[0] as T_HR_EMPLOYEECANCELLEAVE;
            strEvectionID = entEvectionRd.CANCELLEAVEID;
            TerminateLeaveForm form = new TerminateLeaveForm(FormTypes.Audit, strEvectionID);
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.FormType = FormTypes.Audit;
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }


        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strEvectionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_EMPLOYEECANCELLEAVE entEvectionRd = dgEmpLeaveRdList.SelectedItems[0] as T_HR_EMPLOYEECANCELLEAVE;
            strEvectionID = entEvectionRd.CANCELLEAVEID;
            TerminateLeaveForm form = new TerminateLeaveForm(FormTypes.Browse, strEvectionID);
            EntityBrowser browser = new EntityBrowser(form);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.FormType = FormTypes.Browse;
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #endregion

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
