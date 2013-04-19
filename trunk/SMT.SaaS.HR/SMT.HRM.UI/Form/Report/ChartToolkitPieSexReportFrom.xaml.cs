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
using SMT.HRM.UI.ReportChartServiceWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI;
using System.Windows.Controls.DataVisualization.Charting;

namespace SMT.HRM.UI.Form.Report
{
    /// <summary>
    /// weirui 2012-7-20 员工性别对比Pie图
    /// </summary>
    public partial class ChartToolkitPieSexReportFrom : System.Windows.Controls.Window
    {
        //服务
        ReportChartServiceClient serviceClient = null;
        //公司ID
        private string StrCompanyIDs = "";
        public List<PieEmployeece> listPieEmployeece { get; set; }
        public ChartToolkitPieSexReportFrom()
        {
            InitializeComponent();
            
            serviceClient = new ReportChartServiceClient();
            serviceClient.GetSexPieEmployeeceInfoCompleted += new EventHandler<GetSexPieEmployeeceInfoCompletedEventArgs>(serviceClient_GetSexPieEmployeeceInfoCompleted);
            this.nuYear.Value = DateTime.Now.Year;
            this.EndnuYear.Value = DateTime.Now.Year;
        }

        void serviceClient_GetSexPieEmployeeceInfoCompleted(object sender, GetSexPieEmployeeceInfoCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("错误信息：" + e.Error.ToString());
            }
            else
            {
                if (e.Result != null)
                {
                    //等到员工数据
                    var employeeInfo = from p in e.Result select p;

                    List<PieEmployeece> employeeceList = new List<PieEmployeece>();
                    foreach (var item in employeeInfo)
                    {
                        PieEmployeece pieEmployeece = new PieEmployeece();
                        //婚姻状态
                        if (item.Sex =="1")
                        {
                            pieEmployeece.Sex = "男";
                        }

                        else if (item.Sex == "0")
                        {
                            pieEmployeece.Sex = "女";
                        }
                        //人数
                        pieEmployeece.CountEmployeece = item.CountEmployeece;
                        employeeceList.Add(pieEmployeece);
                    }

                    listPieEmployeece = employeeceList;

                    CreatePieChart();
                }
                else
                {
                    if (StrCompanyIDs == "")
                    {
                        MessageBox.Show("请选择公司!");
                    }
                    else
                    {
                        MessageBox.Show(nuYear.Value.ToString() + "年没有查询到数据!");
                        listPieEmployeece = null;
                    }
                }
            }
        }

        /// <summary>
        /// 创建Pie图
        /// </summary>
        public void CreatePieChart()
        {
            Chart chart = new Chart();
            chart.Title = "员工性别状况图表";
            chart.Height = 500;
            chart.Width = 600;
            PieSeries pieSeries = new PieSeries();
            pieSeries.ItemsSource = listPieEmployeece;
            pieSeries.IndependentValueBinding = new System.Windows.Data.Binding("Sex");
            pieSeries.DependentValueBinding = new System.Windows.Data.Binding("CountEmployeece");
            chart.Series.Clear();
            chart.Series.Add(pieSeries);
            PieChart.Children.Clear();
            PieChart.Children.Add(chart);
        }

        #region 调用服务
        private void showColumnChart(string nuYear, string EndnuYear)
        {
            //(Companyid, StartDate, EndDate, filterString, paras, userID);
            //DateTime dtLandStart = new DateTime();
            //DateTime dtLandEnd = new DateTime();
            //DateTime.TryParse(nuYear + "-01-01", out dtLandStart);
            //DateTime.TryParse(EndnuYear + "-12-31", out dtLandEnd);
            string dtLandStart = nuYear + "-01-01";
            string dtLandEnd = EndnuYear + "-12-01";
            string filterString = "";
            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            string style = "1";
            if (this.RadY.IsChecked == true)
            {
                style = "0";
            }
            //serviceClient.GetSexPieEmployeeceInfoAsync(StrCompanyIDs,style,dtLandStart, dtLandEnd, filterString, paras, userID);
        }
        #endregion

        #region 绑定公司控件
        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {

            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string perm = (((int)FormTypes.Browse) + 1).ToString();   //zl 3.1
            OrganizationLookup ogzLookup = new OrganizationLookup();
            ogzLookup.MultiSelected = false;
            ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;

            ogzLookup.SelectedClick += (o, ev) =>
            {
                StrCompanyIDs = "";
                List<ExtOrgObj> ent = ogzLookup.SelectedObj as List<ExtOrgObj>;
                if (ent.Count() == 0)
                {
                    return;
                }
                List<ExtOrgObj> entall = new List<ExtOrgObj>();
                if (ent != null && ent.Count > 0)
                {
                    foreach (var h in ent)
                    {
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)//公司
                        {
                            StrCompanyIDs = h.ObjectID;
                            //先添加总公司
                            ExtOrgObj obj2 = new ExtOrgObj();
                            obj2.ObjectID = h.ObjectID;
                            obj2.ObjectName = h.ObjectName;
                            obj2.ObjectType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
                            entall.Add(obj2);
                            lkSelectObj.DataContext = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)h.ObjectInstance;
                            lkSelectObj.DisplayMemberPath = "CNAME";
                        }
                    }
                }
            };
            ogzLookup.Show<string>(DialogMode.ApplicationModal, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, userID);
        }
        #endregion

        /// <summary>
        /// 点击按钮Pie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_StackedColumn_Click(object sender, RoutedEventArgs e)
        {
            string StartYear = this.nuYear.Value.ToString();
            string EndYear = this.EndnuYear.Value.ToString();
            if (StrCompanyIDs == "")
            {
                MessageBox.Show("请选择公司！");
            }
            else if (int.Parse(StartYear) > int.Parse(EndYear))
            {
                MessageBox.Show("结束年份必须大于开始年份！");
            }
            else
            {
                showColumnChart(StartYear, EndYear);
            }
        }
    }
}

