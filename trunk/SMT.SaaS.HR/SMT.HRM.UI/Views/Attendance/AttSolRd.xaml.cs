/*
 * 文件名：VacationSetBLL.cs
 * 作  用：考勤方案设置页
 * 创建人：吴鹏
 * 创建时间：2010年1月30日, 11:48:48
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

using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AttSolRd : BasePage, IClient
    {
        #region 全局变量
        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public AttSolRd()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AttSolRd_Loaded);            
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

                                  
        #endregion

        #region 私有方法

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_ATTENDANCESOLUTION", true);
            BindComboxBox();
            BindGrid();
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
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            clientAtt.GetAttendanceSolutionListByMultSearchCompleted += new EventHandler<GetAttendanceSolutionListByMultSearchCompletedEventArgs>(clientAtt_GetAttendanceSolutionListByMultSearchCompleted);
            clientAtt.RemoveAttendanceSolutionCompleted += new EventHandler<RemoveAttendanceSolutionCompletedEventArgs>(clientAtt_RemoveAttendanceSolutionCompleted);

            //this.Loaded += new RoutedEventHandler(AttSolRd_Loaded);
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
            string strAttSolName = string.Empty, strAttendanceType = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " ATTENDANCESOLUTIONID ";
            CheckInputFilter(ref strAttSolName, ref strAttendanceType, ref strCheckState);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetAttendanceSolutionListByMultSearchAsync(strOwnerID, strCheckState, strAttSolName, strAttendanceType, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strFineType"></param>
        private void CheckInputFilter(ref string strAttSolName, ref string strAttendanceType, ref string strCheckState)
        {
            if (!string.IsNullOrEmpty(txtAttSolName.Text.Trim()))
            {
                strAttSolName = txtAttSolName.Text.Trim();
            }

            if (cbxkAttendanceType.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkAttendanceType.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    strAttendanceType = entDic.DICTIONARYVALUE.ToString();
                }
            }

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                strCheckState = entDic.DICTIONARYVALUE.ToString();
            }
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
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_ATTENDANCESOLUTION");
                BindGrid();
            }           
        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        void AttSolRd_Loaded(object sender, RoutedEventArgs e)
        {
            InitPage();
            RegisterEvents();
            GetEntityLogo("T_HR_ATTENDANCESOLUTION");
        } 

        /// <summary>
        /// 加载考勤方案数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionListByMultSearchCompleted(object sender, GetAttendanceSolutionListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dgAttSolList.ItemsSource = e.Result;
                dataPager.PageCount = e.pageCount;

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            loadbar.Stop();
        }

        /// <summary>
        /// 删除指定的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveAttendanceSolutionCompleted(object sender, RemoveAttendanceSolutionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "ATTENDANCESOLUTION")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            BindGrid();
        }        

        /// <summary>
        /// 根据查询参数，搜寻符合条件的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAttSolList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAttSolList, e.Row, "T_HR_ATTENDANCESOLUTION");
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
        /// 弹出子窗口，以便浏览指定的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgAttSolList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTION entAttSol = dgAttSolList.SelectedItems[0] as T_HR_ATTENDANCESOLUTION;
            strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;
            AttSolRdForm formAttSolRd = new AttSolRdForm(FormTypes.Browse, strAttendanceSolutionID);

            EntityBrowser entBrowser = new EntityBrowser(formAttSolRd);
            formAttSolRd.MinWidth = 670;
            formAttSolRd.MinHeight = 380;
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便新增考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            AttSolRdForm formAttSolRd = new AttSolRdForm(FormTypes.New, strAttendanceSolutionID);
            EntityBrowser entBrowser = new EntityBrowser(formAttSolRd);
            formAttSolRd.MinWidth = 670;
            formAttSolRd.MinHeight = 380;
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
        /// 弹出表单子窗口，以便编辑考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgAttSolList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTION entAttSol = dgAttSolList.SelectedItems[0] as T_HR_ATTENDANCESOLUTION;
            strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;
            AttSolRdForm formAttSolRd = new AttSolRdForm(FormTypes.Edit, strAttendanceSolutionID);

            EntityBrowser entBrowser = new EntityBrowser(formAttSolRd);
            formAttSolRd.MinWidth = 670;
            formAttSolRd.MinHeight = 380;
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 删除指定的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgAttSolList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgAttSolList.SelectedItems)
            {
                T_HR_ATTENDANCESOLUTION entAttSol = ovj as T_HR_ATTENDANCESOLUTION;
                string Result = "";
                if (entAttSol != null)
                {
                    strID = entAttSol.ATTENDANCESOLUTIONID;
                    if (entAttSol.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETEAUDITERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveAttendanceSolutionAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }            
        }      

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgAttSolList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTION entAttSol = dgAttSolList.SelectedItems[0] as T_HR_ATTENDANCESOLUTION;
            strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;
            AttSolRdForm formAttSolRd = new AttSolRdForm(FormTypes.Audit, strAttendanceSolutionID);
            EntityBrowser entBrowser = new EntityBrowser(formAttSolRd);
            formAttSolRd.MinWidth = 630;
            formAttSolRd.MinHeight = 380;
            entBrowser.FormType = FormTypes.Audit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;

            if (dgAttSolList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAttSolList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_ATTENDANCESOLUTION ent = dgAttSolList.SelectedItems[0] as T_HR_ATTENDANCESOLUTION;

            strAttendanceSolutionID = ent.ATTENDANCESOLUTIONID.ToString();
            AttSolRdForm formAttSolRd = new AttSolRdForm(FormTypes.Resubmit, strAttendanceSolutionID);
            EntityBrowser entBrowser = new EntityBrowser(formAttSolRd);
            formAttSolRd.MinWidth = 630;
            formAttSolRd.MinHeight = 380;
            entBrowser.FormType = FormTypes.Resubmit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
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
