using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class OvertimeRewardSet : BasePage,IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化
        public OvertimeRewardSet()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OvertimeRewardSet_Loaded);
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

            clientAtt.GetOvertimeRewardRdListByMultSearchCompleted += new EventHandler<GetOvertimeRewardRdListByMultSearchCompletedEventArgs>(clientAtt_GetOvertimeRewardRdListByMultSearchCompleted);
            clientAtt.RemoveOvertimeRewardCompleted += new EventHandler<RemoveOvertimeRewardCompletedEventArgs>(clientAtt_RemoveOvertimeRewardCompleted);

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

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_OVERTIMEREWARD", true);
            BindGrid();
        }
        #endregion

        #region 私有方法


        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strOverTimePayType = string.Empty, strOverTimeValID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " OVERTIMEREWARDID ";
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetOvertimeRewardRdListByMultSearchAsync(strOwnerID, strOverTimePayType, strOverTimeValID, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }
        #endregion

        #region 事件

        void OvertimeRewardSet_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            RegisterEvents();
            GetEntityLogo("T_HR_OVERTIMEREWARD");
            UnVisibleGridToolControl();
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetOvertimeRewardRdListByMultSearchCompleted(object sender, GetOvertimeRewardRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_OVERTIMEREWARD> entlist = e.Result;
                dgORList.ItemsSource = entlist;
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
        void clientAtt_RemoveOvertimeRewardCompleted(object sender, RemoveOvertimeRewardCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "OVERTIMEREWARDSET")));
                if (e.Result == "falseOver")
                {
                    MessageBox.Show("选中的加班设置定义，在考勤方案中有使用，无法删除.");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "OVERTIMEREWARDSET")));
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 刷新Grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void dgORList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgORList, e.Row, "T_HR_OVERTIMEREWARD");
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strOvertimeRewardID = string.Empty;
            OvertimeRewardSetForm formOvertimeReward = new OvertimeRewardSetForm(FormTypes.New, strOvertimeRewardID);
            EntityBrowser entBrowser = new EntityBrowser(formOvertimeReward);
            formOvertimeReward.MinWidth = 600;
            formOvertimeReward.MinHeight = 200;
            entBrowser.FormType = FormTypes.New;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strOvertimeRewardID = string.Empty;
            if (dgORList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgORList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_OVERTIMEREWARD ent = dgORList.SelectedItems[0] as T_HR_OVERTIMEREWARD;

            strOvertimeRewardID = ent.OVERTIMEREWARDID.ToString();
            OvertimeRewardSetForm formOvertimeReward = new OvertimeRewardSetForm(FormTypes.Browse, strOvertimeRewardID);
            EntityBrowser entBrowser = new EntityBrowser(formOvertimeReward);
            formOvertimeReward.MinWidth = 600;
            formOvertimeReward.MinHeight = 200;
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strOvertimeRewardID = string.Empty;
            if (dgORList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgORList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_OVERTIMEREWARD ent = dgORList.SelectedItems[0] as T_HR_OVERTIMEREWARD;

            strOvertimeRewardID = ent.OVERTIMEREWARDID.ToString();
            OvertimeRewardSetForm formOvertimeReward = new OvertimeRewardSetForm(FormTypes.Edit, strOvertimeRewardID);
            EntityBrowser entBrowser = new EntityBrowser(formOvertimeReward);
            formOvertimeReward.MinWidth = 600;
            formOvertimeReward.MinHeight = 200;
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});

        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgORList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgORList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgORList.SelectedItems)
            {
                T_HR_OVERTIMEREWARD entOT = ovj as T_HR_OVERTIMEREWARD;

                string Result = "";
                if (entOT != null)
                {
                    strID = entOT.OVERTIMEREWARDID.ToString();
                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveOvertimeRewardAsync(strID);
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
