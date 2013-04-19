using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class WelfarePaymentDetails : BasePage
    {
        //SmtOADocumentAdminClient client;
        SMTLoading loadbar = new SMTLoading();
        public WelfarePaymentDetails()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEDETAIL");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_WELFAREDISTRIBUTEDETAIL", false);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void InitPara()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.Visibility = Visibility.Collapsed;
            //client = new SmtOADocumentAdminClient();
            //client.GetWelfarePaymentDetailsInfosCompleted +=new EventHandler<GetWelfarePaymentDetailsInfosCompletedEventArgs>(client_GetWelfarePaymentDetailsInfosCompleted);

            this.Loaded += new RoutedEventHandler(WelfarePaymentDetails_Loaded);
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void WelfarePaymentDetails_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            //filter += "CHECKSTATE==@" + paras.Count().ToString();
            //paras.Add(Convert.ToInt32(CheckStates.Approved).ToString());

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (!string.IsNullOrEmpty(txtEmpName.Text))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "OWNERNAME==@" + paras.Count().ToString();
                paras.Add(txtEmpName.Text.Trim());
            }
            //LoginUserInfo info = new LoginUserInfo();
            //info.companyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //info.userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //client.GetWelfarePaymentDetailsInfosAsync(dataPager.PageIndex, dataPager.PageSize, "welfareDetailsViews.CREATEDATE", filter, new object[]{},
            //    pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        //void client_GetWelfarePaymentDetailsInfosCompleted(object sender, GetWelfarePaymentDetailsInfosCompletedEventArgs e)
        //{
        //    List<V_WelfareDetails> list = new List<V_WelfareDetails>();
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //        }
        //        DtGrid.ItemsSource = list;
        //    }
        //    loadbar.Stop();
        //}

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_OA_WELFAREDISTRIBUTEDETAIL");
        }
    }
}
