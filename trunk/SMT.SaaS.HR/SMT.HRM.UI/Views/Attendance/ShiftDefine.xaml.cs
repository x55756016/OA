/*
 * 文件名：ShiftDefine.xaml.cs
 * 作  用：考勤班次定义页
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 14:26:11
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class ShiftDefine : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        public T_HR_SHIFTDEFINE entShiftDefine { get; set; }
        #endregion

        #region 初始化
        public ShiftDefine()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ShiftDefine_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 初始化，加载页面数据
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_SHIFTDEFINE", true);
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

            clientAtt.GetShiftDefineListByMultSearchCompleted += new EventHandler<GetShiftDefineListByMultSearchCompletedEventArgs>(clientAtt_GetShiftdEfineListByMultSearchCompleted);
            clientAtt.RemoveShiftDefineCompleted += new EventHandler<RemoveShiftDefineCompletedEventArgs>(clientAtt_RemoveShiftDefineCompleted);

        }                      
        #endregion

        #region 私有方法
       
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
        /// 校验输入的查询条件
        /// </summary>
        /// <param name="strShiftdEfineName"></param>
        /// <param name="strCompanyID"></param>
        private void CheckInputFilter(ref string strShiftdEfineName, ref string strCompanyID)
        {
            if (!string.IsNullOrEmpty(txtShiftDefineName.Text.Trim()))
            {
                strShiftdEfineName = txtShiftDefineName.Text.Trim();
            }
        }

        private void BindGrid()
        {
            string strShiftdEfineName = string.Empty, strCompanyID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty;            
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " SHIFTDEFINEID ";
            CheckInputFilter(ref strShiftdEfineName, ref strCompanyID);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetShiftDefineListByMultSearchAsync(strOwnerID, strShiftdEfineName, strCompanyID, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        private void ShowShiftDefine()
        {
            if (dgSEList.ItemsSource == null)
            {
                entShiftDefine = new T_HR_SHIFTDEFINE();
                this.DataContext = entShiftDefine;
                return;
            }

            ObservableCollection<T_HR_SHIFTDEFINE> entList = dgSEList.ItemsSource as ObservableCollection<T_HR_SHIFTDEFINE>;
            if (entList.Count() == 0)
            {
                entShiftDefine = new T_HR_SHIFTDEFINE();
                this.DataContext = entShiftDefine;
                return;
            }

            if (dgSEList.SelectedItems.Count == 0)
            {                
                entShiftDefine = entList.FirstOrDefault();
            }
            else
            {
                entShiftDefine = dgSEList.SelectedItems[0] as T_HR_SHIFTDEFINE;
            }            

            this.DataContext = entShiftDefine;

            //上班
            IsNeedCard(entShiftDefine.NEEDFIRSTCARD, cbNeedFirstCard);
            IsNeedCard(entShiftDefine.NEEDSECONDCARD, cbNeedSecondCard);
            IsNeedCard(entShiftDefine.NEEDTHIRDCARD, cbNeedThirdCard);
            IsNeedCard(entShiftDefine.NEEDFOURTHCARD, cbNeedFourthCard);

            //下班
            IsNeedCard(entShiftDefine.NEEDFIRSTOFFCARD, cbNeedFirstOffCard);
            IsNeedCard(entShiftDefine.NEEDSECONDOFFCARD, cbNeedSecondOffCard);
            IsNeedCard(entShiftDefine.NEEDTHIRDOFFCARD, cbNeedThirdOffCard);
            IsNeedCard(entShiftDefine.NEEDFOURTHOFFCARD, cbNeedFourthOffCard);
        }

        /// <summary>
        /// 检查当前的CheckBox是否需要勾选
        /// </summary>
        /// <param name="strFlag"></param>
        private void IsNeedCard(string strNeedCardFlag, CheckBox cbNeedCard)
        {
            cbNeedCard.IsChecked = false;
            if (strNeedCardFlag == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
            {
                cbNeedCard.IsChecked = true;
            }
        }
        #endregion

        #region 事件

        void ShiftDefine_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            GetEntityLogo("T_HR_SHIFTDEFINE");
            RegisterEvents();
            UnVisibleGridToolControl();
            InitPage();
        }  

        /// <summary>
        /// 获取班次定义记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetShiftdEfineListByMultSearchCompleted(object sender, GetShiftDefineListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<T_HR_SHIFTDEFINE> entList = e.Result;

                dgSEList.ItemsSource = entList;
                dataPager.PageCount = e.pageCount;

                ShowShiftDefine();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }        

        /// <summary>
        /// 删除班次定义记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveShiftDefineCompleted(object sender, RemoveShiftDefineCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result.IndexOf("SUCCESSED") > 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "SHIFTDEFINEFORM")));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.Replace("{", "").Replace("{", ""))); 
                }
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
        /// 加载图片列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSEList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgSEList, e.Row, "T_HR_SHIFTDEFINE");
        }

        /// <summary>
        /// DataGrid选择行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSEList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowShiftDefine();
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
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 弹出表单子窗口，新增班次定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strShiftDefineID = string.Empty;
            ShiftDefineForm formShiftDefine = new ShiftDefineForm(FormTypes.New, strShiftDefineID);
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});

        }

        /// <summary>
        /// 弹出子窗口，以便浏览班次定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strShiftDefineID = string.Empty;
            if (dgSEList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSEList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_SHIFTDEFINE ent = dgSEList.SelectedItems[0] as T_HR_SHIFTDEFINE;
            strShiftDefineID = ent.SHIFTDEFINEID.ToString();

            ShiftDefineForm formShiftDefine = new ShiftDefineForm(FormTypes.Browse, strShiftDefineID);
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.Browse;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，编辑班次定义
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strShiftDefineID = string.Empty;
            if (dgSEList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSEList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_SHIFTDEFINE ent = dgSEList.SelectedItems[0] as T_HR_SHIFTDEFINE;
            strShiftDefineID = ent.SHIFTDEFINEID.ToString();

            ShiftDefineForm formShiftDefine = new ShiftDefineForm(FormTypes.Edit, strShiftDefineID);
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.Edit;
            
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 删除指定班次定义(物理删除，暂定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgSEList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgSEList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgSEList.SelectedItems)
            {
                T_HR_SHIFTDEFINE entShiftDefine = ovj as T_HR_SHIFTDEFINE;
                string Result = "";
                if (entShiftDefine != null)
                {
                    strID = entShiftDefine.SHIFTDEFINEID;                    

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveShiftDefineAsync(strID);
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
