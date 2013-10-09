//********满意度参与调查管理页******
//修改人：lezy
//修改时间：2011-6-1
//完成时间：2011-6-30
//*********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Media.Imaging;

namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class Satisfactioning : BasePage
    {
        #region 全局变量定义
        private SMTLoading loadbar = new SMTLoading();
        private SmtOAPersonOfficeClient client = null;
        private ObservableCollection<T_OA_SATISFACTIONREQUIRE> deletedList = new ObservableCollection<T_OA_SATISFACTIONREQUIRE>();//标记被删除的对象
        private bool IsQuery = false;
        #endregion

        #region 构造函数
        public Satisfactioning()
        {
            InitializeComponent();
            EventRegister();
            Utility.DisplayGridToolBarButton(ToolBar, "Satisfactioning", true);


            ImageButton btnShowDetail = new ImageButton();
            btnShowDetail.TextBlock.Text = "参与调查";
            btnShowDetail.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_2_d.png", UriKind.Relative));
            btnShowDetail.Click += new RoutedEventHandler(btnSurveyDetail_Click);
            ToolBar.stpOtherAction.Children.Add(btnShowDetail);


            //  ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "4");
            //ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);            
            PARENT.Children.Add(loadbar);
            

            //新增
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            GetData(dataPager.PageIndex, stateFlg);
            
        }
        #endregion

        #region  事件注册
        private void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            this.Loaded += new RoutedEventHandler(Satisfactioning_Loaded);
            client.Get_StaticfactionSurveyAppCheckedCompleted += new EventHandler<Get_StaticfactionSurveyAppCheckedCompletedEventArgs>(client_Get_StaticfactionSurveyAppCheckedCompleted);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
 
        }
        #endregion

        #region 事件注册处理程序
        private void Satisfactioning_Loaded(object sender, RoutedEventArgs e)
        {
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            GetEntityLogo("T_OA_SATISFACTIONMASTER");
        }
        private void client_Get_StaticfactionSurveyAppCheckedCompleted(object sender, Get_StaticfactionSurveyAppCheckedCompletedEventArgs e)
        {
            loadbar.Stop();
            IsQuery = false;
            ObservableCollection<V_MyStaticfaction> o = e.Result;
            if (o != null)
            {
                List<V_MyStaticfaction> lst = o.ToList();
                BindDateGrid(lst);
            }
            else
                BindDateGrid(null);

        }
        #endregion

        #region XAML事件处理程序
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)//行加载
        {
            SetRowLogo(dg, e.Row, "T_OA_SATISFACTIONMASTER");

        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)//清空
        {
            this.txtSurveysContent.Text = string.Empty;
            this.txtSurveysTITLE.Text = string.Empty;
            this.dpStart.SelectedDate = null;
            this.dpEnd.SelectedDate = null;
        }
        private void SearchBtn_Click(object sender, RoutedEventArgs e)//查询
        {
            IsQuery = true;
            GetData(dataPager.PageIndex, stateFlg);
        }
        #endregion

        #region 其他函数
        #endregion

        
      

      
       
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex, stateFlg);
        }
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
              
        string stateFlg = "0";

        //绑定数据
        private void BindDateGrid(List<V_MyStaticfaction> lst)
        {
            if (lst != null && lst.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(lst);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
                dg.ItemsSource = null;
        }       
        /// <summary>
        /// 获取数据 1
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="checkState"></param>
        private void GetData(int pageIndex, string checkState)
        {
            
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值          
            if (IsQuery)
            {
                string StrTitle = "";
                string StrContent = "";

                string StrStart = "";
                string StrEnd = "";
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrTitle = this.txtSurveysTITLE.Text.ToString().Trim();
                StrContent = this.txtSurveysContent.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //MessageBox.Show("结束时间不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                    //MessageBox.Show("开始时间不能为空");
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OARequire.STARTDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "OARequire.STARTDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OARequire.SATISFACTIONTITLE ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrTitle);
                }
                if (!string.IsNullOrEmpty(StrContent))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "OARequire.CONTENT ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrContent);

                }
                //if (!string.IsNullOrEmpty(filter))
                //{
                //    filter += " and ";
                //}
                //filter += "OARequire.STARTDATE <=@" + paras.Count().ToString();//类型名称
                //paras.Add(System.DateTime.Now);

            }
            loadbar.Start();
            client.Get_StaticfactionSurveyAppCheckedAsync(dataPager.PageIndex, dataPager.PageSize, "OARequire.UPDATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState, Common.CurrentLoginUserInfo.UserPosts[0].PostID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
           
        }
       

     
       

        void browser_ReloadDataEvent()
        {
            GetData(dataPager.PageIndex, stateFlg);
        }

        

        private void SetDataForm()
        { }
        /// <summary>
        /// 获取选中项
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<V_MyStaticfaction> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<V_MyStaticfaction> selectList = new ObservableCollection<V_MyStaticfaction>();
                foreach (V_MyStaticfaction obj in dg.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }
     
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex, stateFlg);
        }
        //参与调查
        private void btnSurveyDetail_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<V_MyStaticfaction> selectEmpSurveyList = GetSelectList();
            if (selectEmpSurveyList != null)
            {
                T_OA_SATISFACTIONREQUIRE empSurveysInfo = selectEmpSurveyList[0].OARequire;
                if (empSurveysInfo != null)
                {
                    if (empSurveysInfo.CHECKSTATE == "0")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    DateTime DtNow = System.DateTime.Now;
                    if (empSurveysInfo.ENDDATE < DtNow)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STATICFACTIONISENDYOUCANVISIST"));//调查已经过期，不能参与调查
                        return;
                    }
                    //if()
                    Satisfaction_0 frmEmpSurveysSubmit = new Satisfaction_0();//empSurveysInfo
                    empSurveysInfo.T_OA_SATISFACTIONMASTER = selectEmpSurveyList[0].OAMaster;
                    frmEmpSurveysSubmit.Require = empSurveysInfo;                  
                    EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                    browser.MinHeight = 500;
                    browser.MinWidth = 750;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ShowDetail"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        #region 查询按钮        
        
       
        #endregion
    }
}