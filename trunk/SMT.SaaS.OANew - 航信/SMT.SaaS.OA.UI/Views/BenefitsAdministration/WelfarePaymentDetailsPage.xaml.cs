/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-18

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于福利发放明细数据的展示，获取已审批通过的福利发放数据

*********************************************************************************/
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
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Data;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class WelfarePaymentDetailsPage : BasePage
    {

        #region 全局变量
        public SmtOADocumentAdminClient wssc;
        private ObservableCollection<string> PaymentDetailsID = new ObservableCollection<string>();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public WelfarePaymentDetailsPage()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEDETAIL");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREDISTRIBUTEDETAIL", true);
            InitEvent();
            this.Loaded += new RoutedEventHandler(WelfarePaymentDetailsPage_Loaded);
        }
        #endregion

        #region 加载ToolBar
        void WelfarePaymentDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;//修改
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            //隐藏未使用按钮
            FormToolBar1.btnNew.Visibility = Visibility.Collapsed;//新增
            FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核的分隔符
            FormToolBar1.retEdit.Visibility = Visibility.Collapsed;//编辑的分隔符
            FormToolBar1.retNew.Visibility = Visibility.Collapsed;//新增的分隔符
            FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;//删除
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;//提交不审核
            // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交并审核
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;//读取
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;//导入
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;//检查审核状态
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
            SetStyle(AppConfig._CurrentStyleCode);
        }
        #endregion

        #region 刷新
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_WelfareDetails ent = DaGr.SelectedItems[0] as V_WelfareDetails;

            UpdateWelfarePaymentDetailsChildWindows AddWin = new UpdateWelfarePaymentDetailsChildWindows(FormTypes.Browse, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 400;
            browser.MinHeight = 230;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 修改
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            V_WelfareDetails ent = DaGr.SelectedItems[0] as V_WelfareDetails;

            UpdateWelfarePaymentDetailsChildWindows AddWin = new UpdateWelfarePaymentDetailsChildWindows(FormTypes.Edit, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 400;
            browser.MinHeight = 230;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region AddWin_ReloadDataEvent
        void AddWin_ReloadDataEvent()
        {
            InitEvent();
        }
        #endregion

        #region 查询、分页LoadData
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            //string StrStart = "";
            //string StrEnd = "";
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(txtPaymentOfUser.Text.Trim()))//发放员工
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "welfareDetailsViews.OWNERID ^@" + paras.Count().ToString();
                paras.Add((txtPaymentOfUser.DataContext as SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj).ObjectID);
            }
            //StrStart = ReleaseTime.Text.ToString();
            //StrEnd = ReleaseEndTime.Text.ToString();
            //DateTime DtStart = new DateTime();
            //DateTime DtEnd = new DateTime();
            //if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            //{
            //    DtStart = System.Convert.ToDateTime(StrStart);
            //    DtEnd = System.Convert.ToDateTime(StrEnd);
            //    if (DtStart > DtEnd)
            //    {
            //        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
            //        return;
            //    }
            //    else
            //    {

            //        if (!string.IsNullOrEmpty(filter))
            //        {
            //            filter += " and ";
            //        }
            //        filter += "welfareDetailsViews.CREATEDATE >=@" + paras.Count().ToString();
            //        paras.Add(DtStart);
            //        if (!string.IsNullOrEmpty(filter))
            //        {
            //            filter += " and ";
            //        }
            //        filter += "welfareDetailsViews.CREATEDATE <=@" + paras.Count().ToString();
            //        paras.Add(DtEnd);
            //    }
            //}
            //else
            //{
            //    //开始时间不为空  结束时间为空   
            //    if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
            //    {
            //        if (!string.IsNullOrEmpty(filter))
            //        {
            //            filter += " and ";
            //        }
            //        filter += "welfareDetailsViews.CREATEDATE <=@" + paras.Count().ToString();
            //        paras.Add(DtStart);
            //    }
            //    //结束时间不为空
            //    if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            //    {
            //        if (!string.IsNullOrEmpty(filter))
            //        {
            //            filter += " and ";
            //        }
            //        filter += "welfareDetailsViews.CREATEDATE >=@" + paras.Count().ToString();
            //        paras.Add(DtEnd);
            //    }
            //}
            wssc.GetWelfarePaymentDetailsInfosAsync(dpGrid.PageIndex, dpGrid.PageSize, "welfareDetailsViews.CREATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }
        #endregion

        #region InitEvent
        private void InitEvent()
        {
            wssc = new SmtOADocumentAdminClient();
            wssc.GetWelfarePaymentDetailsInfosCompleted += new EventHandler<GetWelfarePaymentDetailsInfosCompletedEventArgs>(wssc_GetWelfarePaymentDetailsInfosCompleted);
            LoadData();
        }

        private void wssc_GetWelfarePaymentDetailsInfosCompleted(object sender, GetWelfarePaymentDetailsInfosCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    if (e.Result.Count == 0)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFAREPAYMENTDETAILS"));
                    }
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "WELFAREPAYMENTDETAILS"));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion

        #region DataGrid 页面数据加载
        private void BindDataGrid(List<V_WelfareDetails> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dpGrid, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_WELFAREDISTRIBUTEDETAIL");
        }
        #endregion

        #region 保存修改
        private void myBtn_Click(object sender, RoutedEventArgs e)
        {
            Button DetailBtn = sender as Button;
            string ContractModified = DateTime.Now.ToShortDateString();//修改时间
            T_OA_WELFAREDISTRIBUTEDETAIL detail = DetailBtn.DataContext as T_OA_WELFAREDISTRIBUTEDETAIL;

            detail.UPDATEDATE = Convert.ToDateTime(ContractModified);//修改时间
            detail.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
            detail.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//修改人姓名
            detail.USERID = Common.CurrentLoginUserInfo.EmployeeID;
            detail.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            detail.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            detail.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            detail.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            detail.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            try
            {
                wssc.UpdateWelfarePaymentDetailsCompleted += new EventHandler<UpdateWelfarePaymentDetailsCompletedEventArgs>(wsscs_UpdateWelfarePaymentDetailsCompleted);
                wssc.UpdateWelfarePaymentDetailsAsync(detail);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        void wsscs_UpdateWelfarePaymentDetailsCompleted(object sender, UpdateWelfarePaymentDetailsCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PAYMENTDETAILS"));
                LoadData();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }
        #endregion

        #region 设定样式
        public void SetStyle(int Istyle)
        {
            switch (Istyle)
            {
                case -1:
                    break;
                case 0:
                    AppConfig.SetStyles("Assets/Styles.xaml", LayoutRoot);
                    break;
                case 1:
                    AppConfig.SetStyles("Assets/ShinyBlue.xaml", LayoutRoot);
                    btnApp.Style = (Style)Application.Current.Resources["CommonButtonStyle1"];
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 搜索查询
        private void btnApp_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 查询发放员工
        private void btnLookUpUserName_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //InfoObj.APPRAISEUSER = companyInfo.ObjectID;
                    //txtAppraiseUser.Text = companyInfo.ObjectName;
                    txtPaymentOfUser.DataContext = companyInfo;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtPaymentOfUser.Text = string.Empty;
        }
        #endregion
    }
}
