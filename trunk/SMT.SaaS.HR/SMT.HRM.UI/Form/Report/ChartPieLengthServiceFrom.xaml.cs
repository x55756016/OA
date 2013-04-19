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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.ReportChartServiceWS;
using System.Windows.Controls.DataVisualization.Charting;

namespace SMT.HRM.UI.Form.Report
{
    /// <summary>
    /// 员工工龄对比Pie图 weirui 2012-7-22
    /// </summary>
    public partial class ChartPieLengthServiceFrom : System.Windows.Controls.Window
    {
        //服务
        ReportChartServiceClient serviceClient = null;
        //公司ID
        private string StrCompanyIDs = "";
        public List<PieEmployeece> listPieEmployeece { get; set; }
        public ChartPieLengthServiceFrom()
        {
            InitializeComponent();
            serviceClient = new ReportChartServiceClient();
            serviceClient.GetLengthServicePieCompleted += new EventHandler<GetLengthServicePieCompletedEventArgs>(serviceClient_GetLengthServicePieCompleted);
            this.nuYear.Value = DateTime.Now.Year;
            this.EndnuYear.Value = DateTime.Now.Year;
        }

        void serviceClient_GetLengthServicePieCompleted(object sender, GetLengthServicePieCompletedEventArgs e)
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
                        int workAge = 0;
                        if (item.ENTRYDATE != null)
                        {
                            DateTime ENTRYDATE = DateTime.Parse(item.ENTRYDATE.ToString());
                            //得到公龄
                            workAge = GetWorkAge(ENTRYDATE);
                        }
                        pieEmployeece.Age = workAge.ToString();
                        //pieEmployeece.EMPLOYEEID = item.EMPLOYEEID;
                        pieEmployeece.Name = item.Name;
                        employeeceList.Add(pieEmployeece);
                    }

                    //1年及1年以内
                    int m1 = employeeceList.Where(t => int.Parse(t.Age) <= 1).Count();
                    //1~3年(含3年)
                    int m2 = employeeceList.Where(t => int.Parse(t.Age) >1 && int.Parse(t.Age) <=3).Count();
                    //3~5年(含5年)
                    int m3 = employeeceList.Where(t => int.Parse(t.Age) >3 && int.Parse(t.Age) <= 5).Count();
                    //5~8年(含8年)
                    int m4 = employeeceList.Where(t => int.Parse(t.Age) >5 && int.Parse(t.Age) <= 8).Count();
                    //8~10年(不含10年)
                    int m5 = employeeceList.Where(t => int.Parse(t.Age) >8 && int.Parse(t.Age) <10).Count();
                    //10年以上
                    int m6 = employeeceList.Where(t => int.Parse(t.Age) >=10).Count();
                    List<PieEmployeece> newEmployeeceList = new List<PieEmployeece>();
                    for (int i = 0; i < 6; i++)
                    {
                        PieEmployeece pie = new PieEmployeece();
                        switch (i)
                        {
                            case 0:
                                if (m1 != 0)
                                {
                                    pie.Name = "1年及1年以内";
                                    pie.CountEmployeece = m1;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                            case 1:
                                if (m2 != 0)
                                {
                                    pie.Name = "1~3年(含3年)";
                                    pie.CountEmployeece = m2;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                            case 2:
                                if (m3 != 0)
                                {
                                    pie.Name = "3~5年(含5年)";
                                    pie.CountEmployeece = m3;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                            case 3:
                                if (m4 != 0)
                                {
                                    pie.Name = "5~8年(含8年)";
                                    pie.CountEmployeece = m4;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                            case 4:
                                if (m5 != 0)
                                {
                                    pie.Name = "8~10年(不含10年)";
                                    pie.CountEmployeece = m5;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                            case 5:
                                if (m5 != 0)
                                {
                                    pie.Name = "10年以上";
                                    pie.CountEmployeece = m5;
                                    newEmployeeceList.Add(pie);
                                }
                                break;
                        }
                    }

                    listPieEmployeece = newEmployeeceList;
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
                        MessageBox.Show(this.nuYear.Value.ToString() + "~" + this.EndnuYear.Value.ToString() + "年没有查询到数据!");
                        listPieEmployeece = null;
                    }
                }
            }
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
            string style = "1";
            if (this.RadY.IsChecked == true)
            {
                style = "0";
            }
            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            serviceClient.GetLengthServicePieAsync(StrCompanyIDs,style,dtLandStart, dtLandEnd, filterString, paras, userID);
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
        /// 创建Pie图
        /// </summary>
        public void CreatePieChart()
        {
            Chart chart = new Chart();
            //chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.Title = "员工工龄状况图表";
            chart.Height = 500;
            chart.Width = 600;
            PieSeries pieSeries = new PieSeries();
            pieSeries.ItemsSource = listPieEmployeece;
            pieSeries.IndependentValueBinding = new System.Windows.Data.Binding("Name");
            pieSeries.DependentValueBinding = new System.Windows.Data.Binding("CountEmployeece");
            chart.Series.Clear();
            chart.Series.Add(pieSeries);
            PieChart.Children.Clear();
            PieChart.Children.Add(chart);
        }

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

        private int GetWorkAge(DateTime ENTRYDATE)
        {
            int WorkAge = DateTime.Now.Year - ENTRYDATE.Year;
            if (WorkAge ==0)
            {
                WorkAge = 1;
            }
            if (WorkAge!=1)
            {
                if (DateTime.Now < ENTRYDATE.AddYears(WorkAge))
                    WorkAge--;
            }
            return WorkAge;
        }
    }
}

