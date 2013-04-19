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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;
using Visifire.Charts;
using Visifire.Commons;
using SMT.HRM.UI.ReportChartServiceWS;
namespace SMT.HRM.UI.Form.Report
{
    public partial class ChartPieReportFrom : System.Windows.Controls.Window
    {
        //服务
        ReportChartServiceClient client = null;
        
        public List<PieEmployeece> listPieEmployeece {get ;set;}
        //公司ID
        private string StrCompanyIDs = "";
        public ChartPieReportFrom()
        {
            InitializeComponent();

            client = new ReportChartServiceClient();
            client.GetPieEmployeeceInfoCompleted += new EventHandler<GetPieEmployeeceInfoCompletedEventArgs>(client_GetPieEmployeeceInfoCompleted); 
            nuYear.Value = DateTime.Now.Year;
        }

        void client_GetPieEmployeeceInfoCompleted(object sender, GetPieEmployeeceInfoCompletedEventArgs e)
        {
            if (e.Error!=null)
            {
                MessageBox.Show("错误信息："+e.Error.ToString());
            }
            else
            {
                if (e.Result!=null )
                {
                    //等到员工数据
                    var employeeInfo = from p in e.Result select p;

                    List<PieEmployeece> employeeceList = new List<PieEmployeece>();
                    foreach (var item in employeeInfo)
                    {
                        PieEmployeece pieEmployeece = new PieEmployeece();
                        //婚姻状态
                        if (item.marriage =="" ||item.marriage==null)
                        {
                            pieEmployeece.marriage = "离异";
                        }

                        else if (item.marriage == "1")
                        {
                            pieEmployeece.marriage = "已婚";
                        }

                        else if (item.marriage == "0")
                        {
                            pieEmployeece.marriage = "未婚";
                        }
                        
                        //人数
                        pieEmployeece.CountEmployeece = item.CountEmployeece;
                        employeeceList.Add(pieEmployeece);
                    }

                    listPieEmployeece = employeeceList;

                    CreateCreatePieChart();
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

        #region 调用服务
        private void showColumnChart()
        {
            DateTime dtLandStart = new DateTime();
            DateTime dtLandEnd = new DateTime();
            DateTime.TryParse(nuYear.Value.ToString() + "-01-01", out dtLandStart);
            DateTime.TryParse(nuYear.Value.ToString() + "-12-31", out dtLandEnd);
            //client.GetPieEmployeeceInfoAsync(StrCompanyIDs, dtLandStart, dtLandEnd);
        }
        #endregion

        #region 创建一个Pie
        Random rand = new Random(DateTime.Now.Millisecond);
        /// <summary>
        /// 动态创建一个Pie
        /// </summary>
        private void CreateCreatePieChart()
        {
            ReportChartList.Children.Clear();
            Chart chart = new Chart();

            chart.ColorSet = "VisiRed"; // Visifire1
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            Title title = new Title();
            title.Text = "员工婚姻状况分析图";
            chart.Titles.Add(title);

            if (this.RadY.IsChecked == true)
            {
                chart.View3D = true;
            }
            else if (this.RadN.IsChecked == true)
            {
                chart.View3D = false;
            }

            chart.Height = 500;
            chart.Width = 650;
            DataSeries dataSeries = new DataSeries();
            dataSeries.RenderAs = RenderAs.Pie;
            dataSeries.LabelEnabled = true;
            DataPoint dataPoint;
            foreach (PieEmployeece employeece in listPieEmployeece)
            {
                dataPoint = new DataPoint();
                dataPoint.YValue = employeece.CountEmployeece;
                dataPoint.AxisXLabel = employeece.marriage;

                dataSeries.DataPoints.Add(dataPoint);
            }
            dataSeries.LabelText = "#AxisXLabel,#YValue";
            chart.Series.Add(dataSeries);
            chart.ShadowEnabled = true;
            ReportChartList.Children.Add(chart);
            //MyChartOfColumnSeries.View3D=
        }

        void chart_Loaded(object sender, RoutedEventArgs e)
        {
            Chart chart = sender as Chart;

            chart.ColorSet = "Visifire1";
            if (chart != null)
            {
                // 新建一个 ColorSet 集合
                ColorSets emcs = new ColorSets();
                string resourceName = "Visifire.Charts.ColorSets.xaml";  // Visifire 默认颜色集合的文件
                using (System.IO.Stream s = typeof(Chart).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (s != null)
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(s);
                        String xaml = reader.ReadToEnd();
                        emcs = System.Windows.Markup.XamlReader.Load(xaml) as ColorSets;
                        reader.Close();
                        s.Close();
                    }
                }
                // 根据名称取得 Chart 的 ColorSet ( Chart 的 ColorSet 属性为颜色集的名称 )
                ColorSet cs = emcs.GetColorSetByName(chart.ColorSet);

                // 显示图例的 StackPanel
                StackPanel sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                // 图例文本
                string[] legendText = { "离异", "已婚", "未婚" };

                // 自定义图例
                for (int i = 0; i < chart.Series[0].DataPoints.Count; i++)
                {
                    Grid grid = new Grid();
                    grid.Margin = new Thickness(15, 0, 0, 0);
                    ColumnDefinition cd1 = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(cd1);
                    ColumnDefinition cd2 = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(cd2);

                    Rectangle rect = new Rectangle()
                    {
                        Width = 10,
                        Height = 10,
                        Fill = cs.Brushes[i]
                    };
                    rect.SetValue(Grid.ColumnProperty, 0);
                    grid.Children.Add(rect);

                    TextBlock tb = new TextBlock()
                    {
                        Text = legendText[i],
                        Margin = new Thickness(5, 0, 0, 0),
                        Foreground = cs.Brushes[i]
                    };
                    tb.SetValue(Grid.ColumnProperty, 1);
                    grid.Children.Add(tb);

                    sp.Children.Add(grid);
                }
                ReportChartList.Children.Add(sp);
            }
        }
        #endregion

        #region 绑定公司控件
        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {

            string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string perm = (((int)FormTypes.Browse) + 1).ToString();   //zl 3.1
            //string entity = "MonthlyBudgetAnalysis";
            //OrganizationLookup ogzLookup = new OrganizationLookup(userID, perm, entity);
            OrganizationLookup ogzLookup = new OrganizationLookup();
            ogzLookup.MultiSelected = false;
            ogzLookup.SelectedObjType = OrgTreeItemTypes.Company;
            //ogzLookup.ShowMessageForSelectOrganization();

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
        /// 点击按钮StackedColumn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_StackedColumn_Click(object sender, RoutedEventArgs e)
        {
            showColumnChart();
        }
    }
}

