/*
 * 文件名：VacationSetBLL.cs
 * 作  用：公共假期设置页
 * 创建人：吴鹏
 * 创建时间：2010年1月12日, 9:22:58
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
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
    public partial class PublicVacation : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        #endregion

        #region 初始化

        public PublicVacation()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(VacationSetting_Loaded);
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

            clientAtt.GetVacationSetRdListByMultSearchCompleted += new EventHandler<GetVacationSetRdListByMultSearchCompletedEventArgs>(clientAtt_GetVacationSetRdListByMultSearchCompleted);
            clientAtt.RemoveVacationSetCompleted += new EventHandler<RemoveVacationSetCompletedEventArgs>(clientAtt_RemoveVacationSetCompleted);

        }

        /// <summary>
        /// 初始化，加载页面数据
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_VACATIONSET", true);
            BindGrid();
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            string strOwnerID = string.Empty, strVacName = string.Empty, strVacYear = string.Empty, strCountyType = string.Empty, strSortKey = string.Empty;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = " VACATIONNAME ";
            CheckInputFilter(ref strVacName, ref strVacYear, ref strCountyType);
            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            clientAtt.GetVacationSetRdListByMultSearchAsync(strOwnerID, strVacName, strVacYear, strCountyType, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 校验输入的参数
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strVacYear"></param>
        /// <param name="strCountyType"></param>
        private void CheckInputFilter(ref string strVacName, ref string strVacYear, ref string strCountyType)
        {
            if (!string.IsNullOrEmpty(txtVacName.Text.Trim()))
            {
                strVacName = txtVacName.Text.Trim();
            }

            if (!string.IsNullOrEmpty(txtVacYear.Text.Trim()))
            {
                strVacYear = txtVacYear.Text.Trim();
            }

            if (cbxkPubVacArea.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = cbxkPubVacArea.SelectedItem as T_SYS_DICTIONARY;
                if (!string.IsNullOrEmpty(entDic.DICTIONARYID) && !string.IsNullOrEmpty(entDic.DICTIONCATEGORY))
                {
                    strCountyType = entDic.DICTIONARYVALUE.ToString();
                }
            }

        }
        #endregion

        #region 事件
        void VacationSetting_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            GetEntityLogo("T_HR_VACATIONSET");
            RegisterEvents();
            UnVisibleGridToolControl();
            InitPage();
        }

        /// <summary>
        /// 加载数据列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetVacationSetRdListByMultSearchCompleted(object sender, GetVacationSetRdListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_VACATIONSET> entVacList = e.Result;

                dgVacList.ItemsSource = entVacList;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            loadbar.Stop();
        }        

        /// <summary>
        /// 删除假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveVacationSetCompleted(object sender, RemoveVacationSetCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "PUBLICVACATIONFORM")));
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
        /// 假期表单提交后，重新加载数据到数据列表
        /// </summary>
        private void ReLoadGrid()
        {
            BindGrid();
        }

        /// <summary>
        /// 查询数据
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
        private void dgVacList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgVacList, e.Row, "T_HR_VACATIONSET");
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
        /// 弹出表单子窗口，以便新增假期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strVacationID = string.Empty;
            PublicVacationForm formPublicVac = new PublicVacationForm(FormTypes.New, strVacationID);
            EntityBrowser entBrowser = new EntityBrowser(formPublicVac);

            formPublicVac.MinWidth = 640;
            formPublicVac.MinHeight = 540;
            entBrowser.FormType = FormTypes.New;
            
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //Utility.CreateFormFromEngine("28418b91-ed0c-47e8-a730-c07245a156d4", "SMT.HRM.UI.Form.Personnel.PensionMasterForm");            
        }

        /// <summary>
        /// 页面刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 弹出表单子窗口，以便浏览指定假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strVacationID = string.Empty;

            if (dgVacList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgVacList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_VACATIONSET ent = dgVacList.SelectedItems[0] as T_HR_VACATIONSET;

            strVacationID = ent.VACATIONID.ToString();
            PublicVacationForm formPublicVac = new PublicVacationForm(FormTypes.Browse, strVacationID);
            EntityBrowser entBrowser = new EntityBrowser(formPublicVac);

            formPublicVac.MinWidth = 660;
            formPublicVac.MinHeight = 480;
            entBrowser.FormType = FormTypes.Browse;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑指定假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strVacationID = string.Empty;

            if (dgVacList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgVacList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_VACATIONSET ent = dgVacList.SelectedItems[0] as T_HR_VACATIONSET;

            strVacationID = ent.VACATIONID.ToString();
            PublicVacationForm formPublicVac = new PublicVacationForm(FormTypes.Edit, strVacationID);
            EntityBrowser entBrowser = new EntityBrowser(formPublicVac);

            formPublicVac.MinWidth = 660;
            formPublicVac.MinHeight = 480;
            entBrowser.FormType = FormTypes.Edit;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        /// <summary>
        /// 删除指定假期记录(物理删除，待定)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgVacList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgVacList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgVacList.SelectedItems)
            {
                T_HR_VACATIONSET ent = ovj as T_HR_VACATIONSET;
                string Result = "";
                if (ent != null)
                {
                    strID = ent.VACATIONID;

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveVacationSetAsync(strID);
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
