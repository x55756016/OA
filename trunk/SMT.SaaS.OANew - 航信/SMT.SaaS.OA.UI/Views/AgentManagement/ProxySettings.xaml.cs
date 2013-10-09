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
using SMT.SaaS.OA.UI.UserControls.AgentManagement;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.AgentChannel;

namespace SMT.SaaS.OA.UI.Views.AgentManagement
{
    public partial class ProxySettings : BasePage
    {

        #region 全局变量
        private ObservableCollection<string> agentsetId = new ObservableCollection<string>();
        private SmtOACommonOfficeClient SoaChannel;
        PersonnelServiceClient psc = new PersonnelServiceClient();

        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public ProxySettings()
        {
            InitializeComponent();
            InitEvent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_AGENTSET");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_AGENTSET", true);
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;//检查审核状态
            //Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
            //隐藏未使用按钮
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
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

            T_OA_AGENTSET ent = DaGr.SelectedItems[0] as T_OA_AGENTSET;

            ProxySettingsForm AddWin = new ProxySettingsForm(FormTypes.Edit, ent.AGENTSETID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Edit;
            browser.MinWidth = 600;
            browser.MinHeight = 180;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 删除按钮事件
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            agentsetId = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    T_OA_AGENTSET ent = DaGr.SelectedItems[i] as T_OA_AGENTSET;

                    agentsetId.Add((DaGr.SelectedItems[i] as T_OA_AGENTSET).AGENTSETID);

                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        SoaChannel.DeleteAgentSetAsync(agentsetId);
                        LoadData();
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        #endregion

        #region 新增代理设置
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ProxySettingsForm AddWin = new ProxySettingsForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 600;
            browser.MinHeight = 180;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
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

            T_OA_AGENTSET ent = DaGr.SelectedItems[0] as T_OA_AGENTSET;

            ProxySettingsForm AddWin = new ProxySettingsForm(FormTypes.Browse, ent.AGENTSETID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 500;
            browser.MinHeight = 300;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 刷新
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 查询、分页LoadData()
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = ""; //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值

            if (!string.IsNullOrEmpty(txtSYSTEMCODE.Text.Trim())) //系统类型
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SYSCODE ^@" + paras.Count().ToString();
                paras.Add(txtSYSTEMCODE.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtAGENTMODULE.Text.Trim()))//代理模块
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "MODELCODE ^@" + paras.Count().ToString();
                paras.Add(txtAGENTMODULE.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOACommonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            SoaChannel.GetAgentSetListByIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region 初始化

        private void InitEvent()
        {
            SoaChannel = new SmtOACommonOfficeClient();
            SoaChannel.DeleteAgentSetCompleted += new EventHandler<DeleteAgentSetCompletedEventArgs>(SoaChannel_DeleteAgentSetCompleted);
            SoaChannel.GetAgentSetListByIdCompleted += new EventHandler<GetAgentSetListByIdCompletedEventArgs>(SoaChannel_GetAgentSetListByIdCompleted);
            //AgentChannel.AgentServicesClient aa = new AgentServicesClient();
            //aa.GetQueryAgentCompleted += new EventHandler<GetQueryAgentCompletedEventArgs>(aa_GetQueryAgentCompleted);
            //aa.GetQueryAgentAsync("BF06E969-1B2C-4a89-B0AE-A91CA1244053", "TravelApplication");
            LoadData();
        }

        //void aa_GetQueryAgentCompleted(object sender, GetQueryAgentCompletedEventArgs e)
        //{
        //    try
        //    {
        //        string aa = "";
        //        if (e.Result != null)
        //        {
        //            aa.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
        //    }
        //}

        void SoaChannel_GetAgentSetListByIdCompleted(object sender, GetAgentSetListByIdCompletedEventArgs e)
        {
            loadbar.Stop();//读取完数据后，停止动画，隐藏
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
        }

        void SoaChannel_DeleteAgentSetCompleted(object sender, DeleteAgentSetCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    if (!e.Result) //返回值为假
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                        return;
                    }
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            //SoaChannel.DoClose();//关闭WCF服务
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_AGENTSET> obj, int pageCount)
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

        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region DataGrid LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_AGENTSET");
        }
        #endregion

        #region 动态分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtSYSTEMCODE.Text = string.Empty;
            txtAGENTMODULE.Text = string.Empty;
        }
        #endregion
    }
}
