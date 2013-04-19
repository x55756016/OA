/*
 * 文件名：FreeLeaveDaySet.xaml.cs
 * 作  用：带薪假设置页
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 14:26:11
 * 修改人：
 * 修改时间：
 */

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
using System.Collections.ObjectModel;
using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class FreeLeaveDaySet : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public FreeLeaveDaySet()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FreeLeaveDaySet_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchCompleted += new EventHandler<GetFreeLeaveDaySetRdListByMultSearchCompletedEventArgs>(clientAtt_GetFreeLeaveDaySetRdListByMultSearchCompleted);
            clientAtt.RemoveFreeLeaveDaySetCompleted += new EventHandler<RemoveFreeLeaveDaySetCompletedEventArgs>(clientAtt_RemoveFreeLeaveDaySetCompleted);

        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_FREELEAVEDAYSET", true);
            BindGrid();
        }

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strLeaveTypeSetID = string.Empty, strIsFactor = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " FREELEAVEDAYSETID ";
            CheckInputFilter(ref strLeaveTypeSetID);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchAsync(strOwnerID, strLeaveTypeSetID, strIsFactor, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strLeaveTypeSetID"></param>
        /// <param name="strIsFactor"></param>
        private void CheckInputFilter(ref string strLeaveTypeSetID)
        {
            if (lkLeaveTypeName.SelectItem != null)
            {
                T_HR_LEAVETYPESET entLeaveTypeSet = lkLeaveTypeName.SelectItem as T_HR_LEAVETYPESET;
                if (!string.IsNullOrEmpty(entLeaveTypeSet.LEAVETYPESETID))
                {
                    strLeaveTypeSetID = entLeaveTypeSet.LEAVETYPESETID;
                }
            }
        }
        #endregion

        #region 事件

        void FreeLeaveDaySet_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            GetEntityLogo("T_HR_FREELEAVEDAYSET");
            RegisterEvents();
            UnVisibleGridToolControl();
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetFreeLeaveDaySetRdListByMultSearchCompleted(object sender, GetFreeLeaveDaySetRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_FREELEAVEDAYSET> entlist = e.Result;
                dgFreeLeaveDaySetList.ItemsSource = entlist;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            loadbar.Stop();
        }

        /// <summary>
        /// 删除指定记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveFreeLeaveDaySetCompleted(object sender, RemoveFreeLeaveDaySetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "FREELEAVEDAYSETFORM")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            BindGrid();
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            BindGrid();
        }

        /// <summary>
        /// 选择请假类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkLeaveTypeName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("VACATIONNAME", "LEAVETYPENAME");
            cols.Add("FINETYPE", "FINETYPE");
            cols.Add("LEAVEMAXDAYS", "MAXDAYS");
            cols.Add("ISPERFECTATTENDANCEFACTOR", "ISPERFECTATTENDANCEFACTOR");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(T_HR_LEAVETYPESET[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_LEAVETYPESET ent = lookup.SelectedObj as T_HR_LEAVETYPESET;

                if (ent != null)
                {
                    lkLeaveTypeName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgFreeLeaveDaySetList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgFreeLeaveDaySetList, e.Row, "T_HR_FREELEAVEDAYSET");
        }


        /// <summary>
        /// 弹出表单子窗口，以便新增带薪假设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.New, strFreeLeaveDaySetID, null);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.FormType = FormTypes.New;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_FREELEAVEDAYSET ent = dgFreeLeaveDaySetList.SelectedItems[0] as T_HR_FREELEAVEDAYSET;
            strFreeLeaveDaySetID = ent.FREELEAVEDAYSETID.ToString();

            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.Browse, strFreeLeaveDaySetID, null);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.FormType = FormTypes.Browse;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑带薪假设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strFreeLeaveDaySetID = string.Empty;
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_FREELEAVEDAYSET ent = dgFreeLeaveDaySetList.SelectedItems[0] as T_HR_FREELEAVEDAYSET;
            strFreeLeaveDaySetID = ent.FREELEAVEDAYSETID.ToString();

            FreeLeaveDaySetForm formFreeLeaveDaySet = new FreeLeaveDaySetForm(FormTypes.Edit, strFreeLeaveDaySetID, null);
            EntityBrowser entBrowser = new EntityBrowser(formFreeLeaveDaySet);
            entBrowser.FormType = FormTypes.Edit;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });


        }

        /// <summary>
        /// 删除指定的带薪假设置(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgFreeLeaveDaySetList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgFreeLeaveDaySetList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgFreeLeaveDaySetList.SelectedItems)
            {
                T_HR_FREELEAVEDAYSET ent = ovj as T_HR_FREELEAVEDAYSET;

                string Result = "";
                if (ent != null)
                {
                    strID = ent.FREELEAVEDAYSETID.ToString();
                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveFreeLeaveDaySetAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }

            }

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
