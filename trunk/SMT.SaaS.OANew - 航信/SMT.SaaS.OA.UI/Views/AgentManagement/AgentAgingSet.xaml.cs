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
using System.Windows.Media.Imaging;

namespace SMT.SaaS.OA.UI.Views.AgentManagement
{
    public partial class AgentAgingSet : BasePage
    {
        #region 全局变量
        private ObservableCollection<string> agentsetId = new ObservableCollection<string>();
        private SmtOACommonOfficeClient SoaChannel;

        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public AgentAgingSet()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_AGENTDATESET");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_AGENTDATESET", true);
            InitEvent();

            ImageButton btnShowDetail = new ImageButton();
            btnShowDetail.TextBlock.Text = Utility.GetResourceStr("CLOSEAGENT");//关闭代理
            btnShowDetail.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_close.png", UriKind.Relative));
            btnShowDetail.Click += new RoutedEventHandler(TravelapplicationPage_Click);
            FormToolBar1.stpOtherAction.Children.Add(btnShowDetail);

            FormToolBar1.stpOtherAction.Visibility = Visibility.Visible;//关闭代理
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;//检查审核状态

            //隐藏未使用按钮
            FormToolBar1.btnNew.Visibility = Visibility.Visible;
            FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
        }
        #endregion

        #region 关闭代理
        void TravelapplicationPage_Click(object sender, RoutedEventArgs e)
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

            for (int i = 0; i < DaGr.SelectedItems.Count; i++)
            {
                T_OA_AGENTDATESET ent = DaGr.SelectedItems[i] as T_OA_AGENTDATESET;

                AgentAgingSetForm AddWin = new AgentAgingSetForm(FormTypes.Edit, ent.AGENTDATESETID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 550;
                browser.MinHeight = 220;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }
        #endregion

        #region 修改
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
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
                    T_OA_AGENTDATESET ent = DaGr.SelectedItems[i] as T_OA_AGENTDATESET;

                    agentsetId.Add((DaGr.SelectedItems[i] as T_OA_AGENTDATESET).AGENTDATESETID);

                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        SoaChannel.DeleteAgentDataSetAsync(agentsetId);
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

        #region 新增查看申请
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            AgentAgingSetForm AddWin = new AgentAgingSetForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 550;
            browser.MinHeight = 220;
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

            T_OA_AGENTDATESET ent = DaGr.SelectedItems[0] as T_OA_AGENTDATESET;

            AgentAgingSetForm AddWin = new AgentAgingSetForm(FormTypes.Browse, ent.AGENTDATESETID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 500;
            browser.MinHeight = 220;
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

            if (!string.IsNullOrEmpty(dEFFECTDATE.Text.Trim()))//生效时间
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "EFFECTIVEDATE >=@" + paras.Count().ToString();
                paras.Add(DateTime.Parse(dEFFECTDATE.Text.Trim()));
            }
            if (!string.IsNullOrEmpty(dINVALIDDATE.Text.Trim()))//失效时间
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "EXPIRATIONDATE <=@" + paras.Count().ToString();
                paras.Add(DateTime.Parse(dINVALIDDATE.Text.Trim()));
            }
            if (!string.IsNullOrEmpty(dPLANEXPIRATIONDATE.Text.Trim()))//计划失效时间
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "PLANEXPIRATIONDATE <=@" + paras.Count().ToString();
                paras.Add(DateTime.Parse(dPLANEXPIRATIONDATE.Text.Trim()));
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

            SoaChannel.GetAgentDataSetListByIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region 初始化

        private void InitEvent()
        {
            SoaChannel = new SmtOACommonOfficeClient();
            SoaChannel.DeleteAgentDataSetCompleted += new EventHandler<DeleteAgentDataSetCompletedEventArgs>(SoaChannel_DeleteAgentDataSetCompleted);
            SoaChannel.GetAgentDataSetListByIdCompleted += new EventHandler<GetAgentDataSetListByIdCompletedEventArgs>(SoaChannel_GetAgentDataSetListByIdCompleted);
            LoadData();
        }

        void SoaChannel_GetAgentDataSetListByIdCompleted(object sender, GetAgentDataSetListByIdCompletedEventArgs e)
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
            //SoaChannel.DoClose();//关闭WCF服务
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }

        void SoaChannel_DeleteAgentDataSetCompleted(object sender, DeleteAgentDataSetCompletedEventArgs e)
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
        private void BindDataGrid(List<T_OA_AGENTDATESET> obj, int pageCount)
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
            SetRowLogo(DaGr, e.Row, "T_OA_AGENTDATESET");
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
            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = dEFFECTDATE.Text.ToString();
            StrEnd = dINVALIDDATE.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd);
                if (DtStart > DtEnd)//生效日期不能大于失效日期
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANTHEFAILURETIME", "EFFECTDATE"));
                    return;
                }
            }
            LoadData();
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            dEFFECTDATE.Text = string.Empty;
            dINVALIDDATE.Text = string.Empty;
            dPLANEXPIRATIONDATE.Text = string.Empty;
            txtAGENTMODULE.Text = string.Empty;
        }
        #endregion
    }
}
