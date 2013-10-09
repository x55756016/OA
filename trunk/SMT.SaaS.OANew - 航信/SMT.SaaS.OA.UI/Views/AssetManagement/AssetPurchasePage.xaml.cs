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
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.AssetManagement
{
    public partial class AssetPurchasePage : BasePage
    {
        private string checkState = ((int)CheckStates.WaittingApproval).ToString();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件

        public AssetPurchasePage()
        {
            InitializeComponent();
            //PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            SetButtonVisible();
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);//删除
            FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);//提交审核
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
            Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //隐藏未使用按钮
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
           // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
        }

        #region 新增
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            AssetPurchaseControl AddWin = new AssetPurchaseControl(Action.Add);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 620;
            browser.MinHeight = 450;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion
       
        #region 修改
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion
       
        #region 删除
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion
        
        #region 提交审核
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion
        
        #region 刷新
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion
        
        #region 查看详细
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            
        }
        #endregion

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormToolBar1.cbxCheckState.SelectedItem != null)
            {
                checkState = Utility.GetCbxSelectItemValue(FormToolBar1.cbxCheckState);
                SetButtonVisible();
                LoadData();
            }
        }
        #endregion

        #region 查询、分页LoadData()
        private void LoadData()
        {
           
        }
        #endregion

        #region SetButtonVisible
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿箱
                    FormToolBar1.btnNew.Visibility = Visibility.Visible;
                    FormToolBar1.btnEdit.Visibility = Visibility.Visible;
                    FormToolBar1.btnDelete.Visibility = Visibility.Visible;
                  //  FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    break;
                case "1":  //审批中 
                    FormToolBar1.btnNew.Visibility = Visibility.Visible;
                    FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;
                   // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Visible;
                    FormToolBar1.retNew.Visibility = Visibility.Collapsed;//新增分隔符
                    FormToolBar1.retEdit.Visibility = Visibility.Collapsed;//修改分隔符
                    FormToolBar1.retRead.Visibility = Visibility.Collapsed;//删除分隔符
                    break;
                case "2":  //审批通过  审核人身份
                    FormToolBar1.btnNew.Visibility = Visibility.Visible;
                    FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;
                  //  FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.retNew.Visibility = Visibility.Collapsed;//新增分隔符
                    FormToolBar1.retEdit.Visibility = Visibility.Collapsed;//修改分隔符
                    FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                    FormToolBar1.retRead.Visibility = Visibility.Collapsed;//删除分隔符
                    break;
                case "3":  //审批未通过
                    FormToolBar1.btnNew.Visibility = Visibility.Visible;
                    FormToolBar1.btnEdit.Visibility = Visibility.Visible;
                    FormToolBar1.btnDelete.Visibility = Visibility.Visible;
                   // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                    break;
                case "4":  //待审核
                    FormToolBar1.btnNew.Visibility = Visibility.Visible;
                    FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;
                  //  FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Visible;
                    FormToolBar1.retNew.Visibility = Visibility.Collapsed;//新增分隔符
                    FormToolBar1.retEdit.Visibility = Visibility.Collapsed;//修改分隔符
                    FormToolBar1.retRead.Visibility = Visibility.Collapsed;//删除分隔符
                    break;
            }
        }
        #endregion

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_ASSETPURREQUIRE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region 查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
        #endregion
        
        #region 服务器分页控件
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
