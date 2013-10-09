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
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class SelectWelfarePaymentRecords : BasePage, IEntityEditor
    {

        #region 全局变量
        public V_WelfareProvision WelfareProvision;
        private SmtOADocumentAdminClient BenefitsAdministration;
        private string checkState = ((int)CheckStates.Approved).ToString();//审核通过
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        private FormTypes actions;
        #endregion

        #region 构造函数
        public SelectWelfarePaymentRecords()
        {
            InitializeComponent();
            InitEvent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEMASTER");
            //WelfareProvision = obj;
        }
        #endregion
       
        #region 初始化
        private void InitEvent()
        {
            BenefitsAdministration = new SmtOADocumentAdminClient();
            WelfareProvision = new V_WelfareProvision();
            BenefitsAdministration.GetWelfarePSelectRecordCompleted += new EventHandler<GetWelfarePSelectRecordCompletedEventArgs>(BenefitsAdministration_GetWelfarePSelectRecordCompleted);
            LoadData();
        }

        void BenefitsAdministration_GetWelfarePSelectRecordCompleted(object sender, GetWelfarePSelectRecordCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }

        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_WelfareProvision> obj, int pageCount)
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

        #region 查询、分页LoadData()
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            int pageCount = 0;

            BenefitsAdministration.GetWelfarePSelectRecordAsync(dpGrid.PageIndex, dpGrid.PageSize, "welfareProvision.CHECKSTATE",null, pageCount, checkState);
        }
        #endregion

        #region 确定选择
        private void Save()
        {

            if (DaGr.SelectedItem != null)
            {
                V_WelfareProvision tmp = DaGr.SelectedItem as V_WelfareProvision;

                WelfareProvision = tmp;

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("PLEASESELECTPAYMENTRECORDS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SELECT", "WELFAREPROVISIONPAGE");
        }
        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CANCELBUTTON"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            };
            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        private void Cancel()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region 分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region DaGr_LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_WELFAREDISTRIBUTEMASTER");
        }
        #endregion
    }
}
