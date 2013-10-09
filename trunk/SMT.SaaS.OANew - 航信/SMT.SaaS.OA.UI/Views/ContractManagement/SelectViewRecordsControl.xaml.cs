/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-22

** 修改人：刘锦

** 修改时间：2010-07-28

** 描述：

**    主要用于合同查看申请的获取，获取已打印并且上传过合同附件的数据。

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
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class SelectViewRecordsControl : BasePage, IEntityEditor
    {

        #region 全局变量
        private SmtOADocumentAdminClient caswsc;
        private string checkState = ((int)CheckStates.Approved).ToString();//审核通过
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        public V_ContractPrint selecContractInfoObj;
        //private FormTypes actions;
        #endregion

        #region 构造函数
        public SelectViewRecordsControl()
        {
            InitializeComponent();
            //actions = action;
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            InitEvent();
            GetEntityLogo("T_OA_CONTRACTPRINT");
        }
        #endregion
       
        #region 初始化
        private void InitEvent()
        {
            caswsc = new SmtOADocumentAdminClient();
            selecContractInfoObj = new V_ContractPrint();
            caswsc.GetInquiryContractPrintingRecordInfoCompleted += new EventHandler<GetInquiryContractPrintingRecordInfoCompletedEventArgs>(caswsc_GetInquiryContractPrintingRecordInfoCompleted);
            LoadData();
        }

        void caswsc_GetInquiryContractPrintingRecordInfoCompleted(object sender, GetInquiryContractPrintingRecordInfoCompletedEventArgs e)
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
        private void BindDataGrid(List<V_ContractPrint> obj, int pageCount)
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

        #region 确定选择
        private void Save()
        {

            if (DaGr.SelectedItem != null)
            {
                V_ContractPrint tmp = DaGr.SelectedItem as V_ContractPrint;

                selecContractInfoObj = tmp;

            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
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

            caswsc.GetInquiryContractPrintingRecordInfoAsync(dpGrid.PageIndex, dpGrid.PageSize, "contractApp.contractApp.CREATEDATE", "", null, pageCount, loginUserInfo);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SELECT", "APPLICATIONSFORCONTRACTS");
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

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {

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
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTPRINT");
        }
        #endregion
    }
}
