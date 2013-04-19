/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-22

** 修改人：刘锦

** 修改时间：2010-07-02

** 描述：

**    主要用于合同打印数据的获取，根据合同审批的状态获取已审批通过的合同申请数据。

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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractPrintingControl : BasePage, IEntityEditor
    {

        #region 全局变量
        public V_ContractApplications printInfo;
        private SmtOADocumentAdminClient caswsc;
        private string checkState = ((int)CheckStates.Approved).ToString();//审核通过
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        private FormTypes actions;
        #endregion

        #region 构造函数
        public ContractPrintingControl()
        {
            InitializeComponent();
            //actions = action;
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            //ctapp = obj;
            GetEntityLogo("T_OA_CONTRACTAPP");
            InitEvent();
        }
        #endregion

        #region InitEvent
        private void InitEvent()
        {
            caswsc = new SmtOADocumentAdminClient();
            printInfo = new V_ContractApplications();
            caswsc.GetApprovalListByIdCompleted += new EventHandler<GetApprovalListByIdCompletedEventArgs>(caswsc_GetApprovalListByIdCompleted);//根据ID查询审批通过的数据
            LoadData();
        }

        void caswsc_GetApprovalListByIdCompleted(object sender, GetApprovalListByIdCompletedEventArgs e)
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
        private void BindDataGrid(List<V_ContractApplications> obj, int pageCount)
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
                V_ContractApplications tmp = DaGr.SelectedItem as V_ContractApplications;

                printInfo = tmp;

            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ADDTITLE", "CONTRACTPRINTING");
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

        #region 选择
        private void Select()
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "PRINT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "PRINT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_ContractPrint contractApplications = new V_ContractPrint();
            contractApplications.contractApp = DaGr.SelectedItem as V_ContractApplications;
            contractApplications.contractPrint = null;
            ContractPrintUploadControl AddWin = new ContractPrintUploadControl(Action.Print, contractApplications);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 550;
            browser.MinHeight = 430;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            RefreshUI(RefreshedTypes.Close);
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
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
            //string filter = "";    //查询过滤条件
            //ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //filter += "contractApp.CONTRACTAPPID ^@" + paras.Count().ToString();
            //paras.Add(ctapp.contractApp.CONTRACTAPPID);
            caswsc.GetApprovalListByIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "contractApp.CHECKSTATE", "", null, pageCount, checkState, loginUserInfo);
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
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTAPP");
        }
        #endregion
    }
}
