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
using SMT.HRM.UI.Form.Report;

namespace SMT.HRM.UI.Views.Reports
{
    public partial class ReporstMainPage : BasePage
    {
        //SMTLoading loadbar = new SMTLoading();
        public ReporstMainPage()
        {
            InitializeComponent();

            InitPara();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEECHANGEHISTORY", true);
            //ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        /// <summary>
        /// 员工婚姻状况饼图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonEmployeeTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartToolkitPieReportFrom formShiftDefine = new ChartToolkitPieReportFrom();
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 员工性别比例图表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonStaffMarriageTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartToolkitPieSexReportFrom formShiftDefine = new ChartToolkitPieSexReportFrom();
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 员工学历比例图表Education
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonSexRatioTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartEducationReportFrom formShiftDefine = new ChartEducationReportFrom();
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 员工年龄分析表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonEducationTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartAgeReportFrom formShiftDefine = new ChartAgeReportFrom();
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 员工司龄对比图表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonAgeStructureTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartPieLengthServiceFrom formShiftDefine = new ChartPieLengthServiceFrom();
            EntityBrowser entBrowser = new EntityBrowser(formShiftDefine);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 集团各产业人员数量分配比例图Industrialallocation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonStaffDivisionTotal_Click(object sender, RoutedEventArgs e)
        {
            ChartIndustrialalPieFrom fromIndustrialalPie = new ChartIndustrialalPieFrom();
            EntityBrowser entBrowser = new EntityBrowser(fromIndustrialalPie);
            entBrowser.FormType = FormTypes.New;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// XX
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _ImgButtonQuantityPreparationTotal_Click(object sender, RoutedEventArgs e)
        {

        }

        public void InitPara()
        {
            //PARENT.Children.Add(loadbar);
            //loadbar.Stop();
            //this.ToolBar.retNew.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnNew.Visibility = Visibility.Collapsed;
            //this.ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            //this.ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            //this.ToolBar.retAudit.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnNew.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            //this.ToolBar.BtnView.Visibility = Visibility.Collapsed;
            //this.ToolBar.btnEdit.Visibility = Visibility.Collapsed;

            //this.Loaded += new RoutedEventHandler(ReporstMainPage_Loaded);

            //ImageButton _ImgButtonEmployeeTotal = new ImageButton();
            //_ImgButtonEmployeeTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonEmployeeTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("员工婚姻状况图表")).Click += new RoutedEventHandler(_ImgButtonEmployeeTotal_Click);
            ////ToolBar.stpOtherAction.Children.Add(_ImgButtonEmployeeTotal);

            //ImageButton _ImgButtonStaffMarriageTotal = new ImageButton();
            //_ImgButtonStaffMarriageTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonStaffMarriageTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png", Utility.GetResourceStr("员工性别对比图表")).Click += new RoutedEventHandler(_ImgButtonStaffMarriageTotal_Click);
            ////ToolBar.stpOtherAction.Children.Add(_ImgButtonStaffMarriageTotal);

            //ImageButton _ImgButtonSexRatioTotal = new ImageButton();
            //_ImgButtonSexRatioTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonSexRatioTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
            //    Utility.GetResourceStr("员工学历状况图表")).Click += new RoutedEventHandler(_ImgButtonSexRatioTotal_Click);
            //ToolBar.stpOtherAction.Children.Add(_ImgButtonSexRatioTotal);

            //ImageButton _ImgButtonEducationTotal = new ImageButton();
            //_ImgButtonEducationTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonEducationTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
            //    Utility.GetResourceStr("员工年龄分析图表")).Click += new RoutedEventHandler(_ImgButtonEducationTotal_Click);
            ////ToolBar.stpOtherAction.Children.Add(_ImgButtonEducationTotal);


            //ImageButton _ImgButtonAgeStructureTotal = new ImageButton();
            //_ImgButtonAgeStructureTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonAgeStructureTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
            //    Utility.GetResourceStr("员工工龄分析图表")).Click += new RoutedEventHandler(_ImgButtonAgeStructureTotal_Click);
            ////ToolBar.stpOtherAction.Children.Add(_ImgButtonAgeStructureTotal);

            //ImageButton _ImgButtonStaffDivisionTotal = new ImageButton();
            //_ImgButtonStaffDivisionTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonStaffDivisionTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
            //    Utility.GetResourceStr("XXX")).Click += new RoutedEventHandler(_ImgButtonStaffDivisionTotal_Click);
            ////ToolBar.stpOtherAction.Children.Add(_ImgButtonStaffDivisionTotal);

            //ImageButton _ImgButtonQuantityPreparationTotal = new ImageButton();
            //_ImgButtonQuantityPreparationTotal.VerticalAlignment = VerticalAlignment.Center;
            //_ImgButtonQuantityPreparationTotal.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1055.png",
            //    Utility.GetResourceStr("XXX")).Click += new RoutedEventHandler(_ImgButtonQuantityPreparationTotal_Click);
            //ToolBar.stpOtherAction.Children.Add(_ImgButtonQuantityPreparationTotal);
            //loadbar.Stop();
        }

        void ReporstMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //GetEntityLogo("T_HR_EMPLOYEECHANGEHISTORY");
            //Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
        }
    }
}