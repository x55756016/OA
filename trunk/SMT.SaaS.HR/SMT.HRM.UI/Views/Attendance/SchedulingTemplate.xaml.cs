/*
 * 文件名：SchedulingTemplate.xaml.cs
 * 作  用：排班模板设置页
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
    public partial class SchedulingTemplate : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        public T_HR_SCHEDULINGTEMPLATEMASTER SchedulingTemplateMaster { get; set; }
        #endregion

        #region 初始化
        public SchedulingTemplate()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SchedulingTemplate_Loaded);
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

            clientAtt.GetSchedulingTemplateMasterListByMultSearchCompleted += new EventHandler<GetSchedulingTemplateMasterListByMultSearchCompletedEventArgs>(clientAtt_GetSchedulingTemplateMasterListByMultSearchCompleted);
            clientAtt.RemoveSchedulingTemplateMasterCompleted += new EventHandler<RemoveSchedulingTemplateMasterCompletedEventArgs>(clientAtt_RemoveSchedulingTemplateMasterCompleted);

            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdCompleted += new EventHandler<GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs>(clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_SCHEDULINGTEMPLATEMASTER", true);
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
        

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strTemplateName = string.Empty, strSchedulingCircleType = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " TEMPLATEMASTERID ";
            CheckInputFilter(ref strTemplateName, ref strSchedulingCircleType);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetSchedulingTemplateMasterListByMultSearchAsync(strOwnerID, strTemplateName, strSchedulingCircleType, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strFineType"></param>
        private void CheckInputFilter(ref string strTemplateName, ref string strSchedulingCircleType)
        {

            if (!string.IsNullOrEmpty(txtSTName.Text))
            {
                strTemplateName = txtSTName.Text;
            }

            if (cbxkCircleType.SelectionBoxItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkCircleType.SelectionBoxItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    return;
                }

                strSchedulingCircleType = entDic.DICTIONARYVALUE.Value.ToString();

            }
        }

        /// <summary>
        /// 显示单个模板信息
        /// </summary>
        private void ShowTemplateMasterAndDetail()
        {
            if (dgSTList.ItemsSource == null)
            {
                SchedulingTemplateMaster = new T_HR_SCHEDULINGTEMPLATEMASTER();
                this.DataContext = SchedulingTemplateMaster;
                BindTemplateDetail();
                return;
            }

            if (dgSTList.SelectedItems.Count == 0)
            {
                IEnumerable<T_HR_SCHEDULINGTEMPLATEMASTER> entList = dgSTList.ItemsSource as IEnumerable<T_HR_SCHEDULINGTEMPLATEMASTER>;
                SchedulingTemplateMaster = entList.FirstOrDefault();
            }
            else
            {
                SchedulingTemplateMaster = dgSTList.SelectedItems[0] as T_HR_SCHEDULINGTEMPLATEMASTER;
            }

            if (SchedulingTemplateMaster != null)
            {
                this.DataContext = SchedulingTemplateMaster;

                BindTemplateDetail();
            }
        }

        /// <summary>
        /// 绑定模板明细
        /// </summary>
        private void BindTemplateDetail()
        {
            string strTemplateMasterID = string.Empty, strSortKey = string.Empty;

            if (SchedulingTemplateMaster == null)
            {
                List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = new List<T_HR_SCHEDULINGTEMPLATEDETAIL>();
                dgTemplateDetails.ItemsSource = entTemplateDetails;
                return;
            }

            if (string.IsNullOrEmpty(SchedulingTemplateMaster.TEMPLATEMASTERID))
            {
                List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = new List<T_HR_SCHEDULINGTEMPLATEDETAIL>();
                dgTemplateDetails.ItemsSource = entTemplateDetails;
                return;
            }

            strSortKey = " SCHEDULINGINDEX ";
            strTemplateMasterID = SchedulingTemplateMaster.TEMPLATEMASTERID;

            clientAtt.GetAllSchedulingTemplateDetailRdListByMasterIdAsync(strTemplateMasterID, strSortKey);
        }
        #endregion

        #region 事件

        void SchedulingTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            GetEntityLogo("T_HR_SCHEDULINGTEMPLATEMASTER");
            RegisterEvents();
            UnVisibleGridToolControl();
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetSchedulingTemplateMasterListByMultSearchCompleted(object sender, GetSchedulingTemplateMasterListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_SCHEDULINGTEMPLATEMASTER> entlist = e.Result;
                dgSTList.ItemsSource = entlist;
                dataPager.PageCount = e.pageCount;

                ShowTemplateMasterAndDetail();                
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
        void clientAtt_RemoveSchedulingTemplateMasterCompleted(object sender, RemoveSchedulingTemplateMasterCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "SCHEDULINGTEMPLATEMASTER")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            BindGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAllSchedulingTemplateDetailRdListByMasterIdCompleted(object sender, GetAllSchedulingTemplateDetailRdListByMasterIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = e.Result as IEnumerable<T_HR_SCHEDULINGTEMPLATEDETAIL>;
                dgTemplateDetails.ItemsSource = entTemplateDetails;
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
            ReLoadGrid();
        }

        /// <summary>
        /// 排班模板表单提交后，重新加载数据到数据列表
        /// </summary>
        private void ReLoadGrid()
        {
            string strTemplateName = string.Empty, strSchedulingCircleType = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " TEMPLATEMASTERID ";
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetSchedulingTemplateMasterListByMultSearchAsync(strOwnerID, strTemplateName, strSchedulingCircleType, strSortKey, pageIndex, pageSize, pageCount);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        private void dgSTList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgSTList, e.Row, "T_HR_SCHEDULINGTEMPLATEMASTER");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSTList_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowTemplateMasterAndDetail();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strSchedulingTemplateMasterID = string.Empty;
            SchedulingTemplateForm formSchedulingTemplate = new SchedulingTemplateForm(FormTypes.New, strSchedulingTemplateMasterID);
            EntityBrowser entBrowser = new EntityBrowser(formSchedulingTemplate);
            entBrowser.FormType = FormTypes.New;
            
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
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
        /// 弹出子窗口，以便浏览排班模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strSchedulingTemplateMasterID = string.Empty;
            if (dgSTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_SCHEDULINGTEMPLATEMASTER ent = dgSTList.SelectedItems[0] as T_HR_SCHEDULINGTEMPLATEMASTER;
            strSchedulingTemplateMasterID = ent.TEMPLATEMASTERID.ToString();

            SchedulingTemplateForm formSchedulingTemplate = new SchedulingTemplateForm(FormTypes.Browse, strSchedulingTemplateMasterID);
            EntityBrowser entBrowser = new EntityBrowser(formSchedulingTemplate);
            entBrowser.FormType = FormTypes.Browse;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strSchedulingTemplateMasterID = string.Empty;
            if (dgSTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_SCHEDULINGTEMPLATEMASTER ent = dgSTList.SelectedItems[0] as T_HR_SCHEDULINGTEMPLATEMASTER;
            strSchedulingTemplateMasterID = ent.TEMPLATEMASTERID.ToString();

            SchedulingTemplateForm formSchedulingTemplate = new SchedulingTemplateForm(FormTypes.Edit, strSchedulingTemplateMasterID);
            EntityBrowser entBrowser = new EntityBrowser(formSchedulingTemplate);
            entBrowser.FormType = FormTypes.Edit;
            
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgSTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgSTList.SelectedItems)
            {
                T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaste = ovj as T_HR_SCHEDULINGTEMPLATEMASTER;
                string Result = "";
                if (entTemplateMaste != null)
                {
                    strID = entTemplateMaste.TEMPLATEMASTERID;

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveSchedulingTemplateMasterAsync(strID);
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
