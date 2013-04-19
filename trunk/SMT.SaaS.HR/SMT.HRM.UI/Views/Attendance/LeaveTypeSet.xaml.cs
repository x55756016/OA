/*
 * 文件名：LeaveTypeSet.xaml.cs
 * 作  用：请假类型设置页
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 14:26:11
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class LeaveTypeSet : BasePage,IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        public T_HR_LEAVETYPESET entLeaveTypeSet { get; set; }

        #endregion

        #region 初始化
        public LeaveTypeSet()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LeaveTypeSet_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #endregion

        #region 私有方法
        
        /// <summary>
        /// 事件注册
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            toolbarLT.btnNew.Click += new RoutedEventHandler(btnLTNew_Click);
            toolbarLT.btnRefresh.Click += new RoutedEventHandler(btnLTRefresh_Click);
            toolbarLT.BtnView.Click += new RoutedEventHandler(BtnLTView_Click);
            toolbarLT.btnEdit.Click += new RoutedEventHandler(btnLTEdit_Click);
            toolbarLT.btnDelete.Click += new RoutedEventHandler(btnLTDelete_Click);

            clientAtt.GetLeaveTypeSetRdListByMultSearchCompleted += new EventHandler<GetLeaveTypeSetRdListByMultSearchCompletedEventArgs>(clientAtt_GetLeaveTypeSetRdListByMultSearchCompleted);
            clientAtt.RemoveLeaveTypeSetCompleted += new EventHandler<RemoveLeaveTypeSetCompletedEventArgs>(clientAtt_RemoveLeaveTypeSetCompleted);

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchCompleted += new EventHandler<GetFreeLeaveDaySetRdListByMultSearchCompletedEventArgs>(clientAtt_GetFreeLeaveDaySetRdListByMultSearchCompleted);
        }


        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbarLT, "T_HR_LEAVETYPESET", true);
            GetBasicInfo();
        }

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //假期标准
            //toolbarLT.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbarLT.btnAudit.Visibility = Visibility.Collapsed;
            //toolbarLT.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbarLT.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbarLT.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbarLT.retRead.Visibility = Visibility.Collapsed;            
        }

        /// <summary>
        /// 加载页面普通信息 --加载登录用户的相关信息，及配置权限
        /// </summary>
        private void GetBasicInfo()
        {
            BindGrid();
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strLeaveTypeValue = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " LEAVETYPESETID ";
            CheckInputFilter(ref strLeaveTypeValue);
            pageIndex = dpLTList.PageIndex;
            pageSize = dpLTList.PageSize;

            clientAtt.GetLeaveTypeSetRdListByMultSearchAsync(strOwnerID, strLeaveTypeValue, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strFineType"></param>
        private void CheckInputFilter(ref string strLeaveTypeValue)
        {            
            if (cbxkLeaveTypeValue.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkLeaveTypeValue.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    strLeaveTypeValue = entDic.DICTIONARYVALUE.ToString();
                }
            }
        }

        /// <summary>
        /// 显示选中项的假期标准详细内容
        /// </summary>
        private void ShowLeaveTypeSetForm()
        {
            if (dgLTList.ItemsSource == null)
            {
                entLeaveTypeSet = new T_HR_LEAVETYPESET();
                this.DataContext = entLeaveTypeSet;
                BindFreeLeaveDaySet();
                return;
            }

            if (dgLTList.SelectedItems.Count == 0)
            {
                IEnumerable<T_HR_LEAVETYPESET> entList = dgLTList.ItemsSource as IEnumerable<T_HR_LEAVETYPESET>;
                entLeaveTypeSet = entList.FirstOrDefault();
            }
            else
            {
                entLeaveTypeSet = dgLTList.SelectedItems[0] as T_HR_LEAVETYPESET;
            }

            if (entLeaveTypeSet != null)
            {
                this.DataContext = entLeaveTypeSet;

                switch (entLeaveTypeSet.SEXRESTRICT)
                {
                    case "0":
                        tbSexRestrict.Text = Utility.GetResourceStr("GRIL");
                        break;
                    case "1":
                        tbSexRestrict.Text = Utility.GetResourceStr("MAN");
                        break;
                    case "2":
                        tbSexRestrict.Text = Utility.GetResourceStr("NOLIMIT");
                        break;
                    default:
                        tbSexRestrict.Text = string.Empty;
                        break;
                }

                BindFreeLeaveDaySet();
            }
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindFreeLeaveDaySet()
        {
            string strLeaveTypeSetID = string.Empty, strIsFactor = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            if (entLeaveTypeSet == null)
            {
                List<T_HR_FREELEAVEDAYSET> entFreeleaveDaySets = new List<T_HR_FREELEAVEDAYSET>();
                dgFreeLeaveDaySetList.ItemsSource = entFreeleaveDaySets;
                return;
            }

            if (string.IsNullOrEmpty(entLeaveTypeSet.LEAVETYPESETID))
            {
                List<T_HR_FREELEAVEDAYSET> entFreeleaveDaySets = new List<T_HR_FREELEAVEDAYSET>();
                dgFreeLeaveDaySetList.ItemsSource = entFreeleaveDaySets;
                return;
            }

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " FREELEAVEDAYSETID ";
            strLeaveTypeSetID = entLeaveTypeSet.LEAVETYPESETID;

            pageIndex = dpFreeLeaveDaySet.PageIndex;
            pageSize = dpFreeLeaveDaySet.PageSize;

            clientAtt.GetFreeLeaveDaySetRdListByMultSearchAsync(strOwnerID, strLeaveTypeSetID, strIsFactor, strSortKey, pageIndex, pageSize, pageCount);
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadFreeLTDataEvent()
        {
            BindFreeLeaveDaySet();
        }
        #endregion

        #region 事件

        #region 假期标准

        /// <summary>
        /// 页面数据初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LeaveTypeSet_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            GetEntityLogo("T_HR_LEAVETYPESET");
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
        void clientAtt_GetLeaveTypeSetRdListByMultSearchCompleted(object sender, GetLeaveTypeSetRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_LEAVETYPESET> entlist = e.Result;
                dgLTList.ItemsSource = entlist;
                dpLTList.PageCount = e.pageCount;

                ShowLeaveTypeSetForm();
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
        void clientAtt_RemoveLeaveTypeSetCompleted(object sender, RemoveLeaveTypeSetCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result!="{DELETESUCCESSED}")
                {
                    MessageBox.Show("无法删除，设置在考勤方案带薪假或者请假记录中有应用");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "LEAVESETFORM")));

                    BindGrid();
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void entBrowser_ReloadDataEvent()
        {
            BindGrid();
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
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnLTRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpLTList_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 首列显示图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLTList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgLTList, e.Row, "T_HR_LEAVETYPESET");
        }

        private void dgLTList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowLeaveTypeSetForm();
        }        

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnLTNew_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            LeaveTypeSetForm formLeaveTypeSet = new LeaveTypeSetForm(FormTypes.New, strLeaveTypeSetID);
            EntityBrowser entBrowser = new EntityBrowser(formLeaveTypeSet);
            formLeaveTypeSet.MinWidth = 630;
            formLeaveTypeSet.MinHeight = 600;
            entBrowser.FormType = FormTypes.New;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnLTView_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (dgLTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgLTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_LEAVETYPESET ent = dgLTList.SelectedItems[0] as T_HR_LEAVETYPESET;

            strLeaveTypeSetID = ent.LEAVETYPESETID.ToString();
            LeaveTypeSetForm formLeaveTypeSet = new LeaveTypeSetForm(FormTypes.Browse, strLeaveTypeSetID);
            EntityBrowser entBrowser = new EntityBrowser(formLeaveTypeSet);
            formLeaveTypeSet.MinWidth = 630;
            formLeaveTypeSet.MinHeight = 600;
            entBrowser.FormType = FormTypes.Browse;
            
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnLTEdit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (dgLTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgLTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_LEAVETYPESET ent = dgLTList.SelectedItems[0] as T_HR_LEAVETYPESET;

            strLeaveTypeSetID = ent.LEAVETYPESETID.ToString();
            LeaveTypeSetForm formLeaveTypeSet = new LeaveTypeSetForm(FormTypes.Edit, strLeaveTypeSetID);
            EntityBrowser entBrowser = new EntityBrowser(formLeaveTypeSet);
            formLeaveTypeSet.MinWidth = 630;
            formLeaveTypeSet.MinHeight = 600;
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnLTDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgLTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgLTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgLTList.SelectedItems)
            {
                T_HR_LEAVETYPESET ent = ovj as T_HR_LEAVETYPESET;
                string Result = "";
                if (ent != null)
                {
                    strID = ent.LEAVETYPESETID;

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveLeaveTypeSetAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }
        }

        #endregion

        #region 带薪假设置

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
                dpFreeLeaveDaySet.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 首列显示图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgFreeLeaveDaySetList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgFreeLeaveDaySetList, e.Row, "T_HR_FREELEAVEDAYSET");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpFreeLeaveDaySet_Click(object sender, RoutedEventArgs e)
        {
            BindFreeLeaveDaySet();
        }

        #endregion

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
