/*
 * 文件名：AttendanceSolutionAsign.xaml.cs
 * 作  用：考勤方案应用页
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
    public partial class AttendanceSolutionAsign : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public AttendanceSolutionAsign()
        {
            InitializeComponent();
            clientAtt = new AttendanceServiceClient();
            RegisterEvents();
            GetEntityLogo("T_HR_ATTENDANCESOLUTIONASIGN");
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
            
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            clientAtt.GetAttendanceSolutionAsignRdListByMultSearchCompleted += new EventHandler<GetAttendanceSolutionAsignRdListByMultSearchCompletedEventArgs>(clientAtt_GetAttendanceSolutionAsignRdListByMultSearchCompleted);
            clientAtt.RemoveAttendanceSolutionAsignCompleted += new EventHandler<RemoveAttendanceSolutionAsignCompletedEventArgs>(clientAtt_RemoveAttendanceSolutionAsignCompleted);

            this.Loaded += new RoutedEventHandler(AttendanceSolutionAsign_Loaded);
        }        

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_ATTENDANCESOLUTIONASIGN", true);
            BindComboxBox();
            BindGrid();
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
            string strAttendanceSolutionID = string.Empty, strAssignedObjectType = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " ATTENDANCESOLUTIONASIGNID ";
            CheckInputFilter(ref strAttendanceSolutionID, ref strAssignedObjectType, ref strCheckState);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;
            if (strCheckState == Convert.ToInt32(CheckStates.All).ToString()) strCheckState = "";

            clientAtt.GetAttendanceSolutionAsignRdListByMultSearchAsync(strOwnerID, strCheckState, strAttendanceSolutionID, strAssignedObjectType, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strFineType"></param>
        private void CheckInputFilter(ref string strAttendanceSolutionID, ref string strAssignedObjectType, ref string strCheckState)
        {
            if (lkAttSol.DataContext != null)
            {
                T_HR_ATTENDANCESOLUTION ent = lkAttSol.DataContext as T_HR_ATTENDANCESOLUTION;
                strAttendanceSolutionID = ent.ATTENDANCESOLUTIONID;
            }

            if (cbxkAssignedObjectType.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    strAssignedObjectType = entDic.DICTIONARYVALUE.ToString();
                }
            }

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                strCheckState = entDic.DICTIONARYVALUE.ToString();
            }
        }
        #endregion

        #region 事件

        /// <summary>
        /// 根据审核状态显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;                
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_ATTENDANCESOLUTIONASIGN");
                BindGrid();
            }
        }        
        /// <summary>
        /// 页面加载时，预绑定FormToolBar的状态ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendanceSolutionAsign_Loaded(object sender, RoutedEventArgs e)
        {
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionAsignRdListByMultSearchCompleted(object sender, GetAttendanceSolutionAsignRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_ATTENDANCESOLUTIONASIGN> entlist = e.Result;
                dgAttSolAsignList.ItemsSource = entlist;
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
        void clientAtt_RemoveAttendanceSolutionAsignCompleted(object sender, RemoveAttendanceSolutionAsignCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCESOLUTIONASIGNFORM")));
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
            ReLoadGrid();
        }

        /// <summary>
        /// 请假类型表单提交后，重新加载数据到数据列表
        /// </summary>
        private void ReLoadGrid()
        {
            BindGrid();
        }

        private void lkAttSol_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();            
            cols.Add("ATTENDANCESOLUTIONNAME", "ATTENDANCESOLUTIONNAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.AttendanceSolution,
                typeof(T_HR_ATTENDANCESOLUTION[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_ATTENDANCESOLUTION ent = lookup.SelectedObj as T_HR_ATTENDANCESOLUTION;

                if (ent != null)
                {
                    lkAttSol.DataContext = ent;
                }
            };
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAttSolAsignList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAttSolAsignList, e.Row, "T_HR_ATTENDANCESOLUTIONASIGN");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 弹出表单子窗口，以便新增考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;
            AttendanceSolutionAsignForm formAttendanceSolutionAsign = new AttendanceSolutionAsignForm(FormTypes.New, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.New;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 刷新
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
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTIONASIGN ent = dgAttSolAsignList.SelectedItems[0] as T_HR_ATTENDANCESOLUTIONASIGN;

            strAttendanceSolutionAsignID = ent.ATTENDANCESOLUTIONASIGNID.ToString();
            AttendanceSolutionAsignForm formAttendanceSolutionAsign = new AttendanceSolutionAsignForm(FormTypes.Resubmit, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Resubmit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出子窗口，以便浏览指定的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTIONASIGN ent = dgAttSolAsignList.SelectedItems[0] as T_HR_ATTENDANCESOLUTIONASIGN;

            strAttendanceSolutionAsignID = ent.ATTENDANCESOLUTIONASIGNID.ToString();
            AttendanceSolutionAsignForm formAttendanceSolutionAsign = new AttendanceSolutionAsignForm(FormTypes.Browse, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionAsignID = string.Empty;

            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTIONASIGN ent = dgAttSolAsignList.SelectedItems[0] as T_HR_ATTENDANCESOLUTIONASIGN;

            strAttendanceSolutionAsignID = ent.ATTENDANCESOLUTIONASIGNID.ToString();
            AttendanceSolutionAsignForm formAttendanceSolutionAsign = new AttendanceSolutionAsignForm(FormTypes.Edit, strAttendanceSolutionAsignID);
            EntityBrowser entBrowser = new EntityBrowser(formAttendanceSolutionAsign);
            formAttendanceSolutionAsign.MinWidth = 600;
            formAttendanceSolutionAsign.MinHeight = 240;
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});

        }

        /// <summary>
        /// 删除指定考勤方案应用记录(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgAttSolAsignList.SelectedItems)
            {
                T_HR_ATTENDANCESOLUTIONASIGN ent = ovj as T_HR_ATTENDANCESOLUTIONASIGN;

                string Result = "";
                if (ent != null)
                {
                    strID = ent.ATTENDANCESOLUTIONASIGNID.ToString();
                    if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETEAUDITERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveAttendanceSolutionAsignAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strID = string.Empty;
            if (dgAttSolAsignList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolAsignList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = dgAttSolAsignList.SelectedItems[0] as T_HR_ATTENDANCESOLUTIONASIGN;
            strID = entAttSolAsign.ATTENDANCESOLUTIONASIGNID;
            AttendanceSolutionAsignForm formAttSolAsign = new AttendanceSolutionAsignForm(FormTypes.Audit, strID);
            EntityBrowser entBrowser = new EntityBrowser(formAttSolAsign);
            formAttSolAsign.MinWidth = 600;
            formAttSolAsign.MinHeight = 240;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.FormType = FormTypes.Audit;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
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
