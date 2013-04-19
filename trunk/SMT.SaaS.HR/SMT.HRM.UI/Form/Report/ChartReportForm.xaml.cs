using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.OrganizationControl;
namespace SMT.HRM.UI.Form.Report
{
    public partial class ChartReportForm : System.Windows.Controls.Window
    {
        ////ReportChartServiceClient
        //ReportChartServiceClient ChartClient = null;
        ////ChartReportService
        ////<summary>
        //// Gets the number of checkups for each pet during Q2.
        ////</summary>
        //public List<ReportChart> AllNEmployeeentryInfo { get; set; }
        ////<summary>
        //// Gets the number of checkups for each pet during Q1.
        ////</summary>
        //public List<ReportChart> AllYEmployeeentryInfo { get; set; }
        ////<summary>
        //// Gets the number of checkups for each pet during Q2.
        ////</summary>
        //public List<ReportChart> AllLEmployeeentryInfo { get; set; }
        ////获取公司ID
        //private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>(); 
        //private string sType ="";
        private string StrCompanyIDs = "";
        public ChartReportForm()
        {
            InitializeComponent();

            //ChartClient = new ReportChartServiceClient();
            //ChartClient.NewGetAllEmployeeentryInfoCompleted += new EventHandler<NewGetAllEmployeeentryInfoCompletedEventArgs>(ChartClient_NewGetAllEmployeeentryInfoCompleted);
            //this.Loaded += new RoutedEventHandler(EmployeeReports_Loaded);
            //nuYear.Value = DateTime.Now.Year;
            //this.Title = "员工婚姻状况分析表";
        }

        //#region 公共函数转—化数据类型，替代缺失月份数据
        //private List<ReportChart> GetClass2(List<ReportChart> list)
        //{
        //    //考虑到缺失月份，填充数据（1~12月）
        //    List<ReportChart> l1 = new List<ReportChart>();
        //    for (int i = 1; i <= 12; i++)
        //    {
        //        ReportChart c = new ReportChart();
        //        c.MouthE = i.ToString();
        //        c.CountE = 0;
        //        l1.Add(c);
        //    }
        //    //转化数据类型，替代缺失月份数据
        //    foreach (ReportChart c in l1)
        //    {
        //        foreach (ReportChart t in list)
        //        {
        //            if (c.MouthE == t.MouthE)
        //            {
        //                c.CountE = t.CountE;
        //            }
        //        }
        //    }
        //    return l1;
        //}
        //#endregion

        //#region 绑定数据
        //void ChartClient_NewGetAllEmployeeentryInfoCompleted(object sender, NewGetAllEmployeeentryInfoCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        MessageBox.Show("错误信息:" + e.Error.ToString());
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {

        //            //得每个月份未婚、已婚、离异人员的到值
        //            var count = from p in e.Result
        //                        group p by p.DataEnty.Value.Month into g
        //                        select new
        //                        {
        //                            g.Key,
        //                            NumProducts = g.Count()
        //                        };
        //            //未婚人员信息
        //            var WEmployeeentryInfo = from p in e.Result
        //                                     where p.MarriageEnty == "0"
        //                                     group p by p.DataEnty.Value.Month into g
        //                                     select new
        //                                     {
        //                                         g.Key,
        //                                         NumProducts = g.Count()
        //                                     };
        //            //遍历未婚人员信息,并且转换数据类型
        //            List<ReportChart> list1 = new List<ReportChart>();
        //            foreach (var item in WEmployeeentryInfo)
        //            {
        //                foreach (var c in count)
        //                {
        //                    if (item.Key == c.Key)
        //                    {
        //                        ReportChart cl = new ReportChart();
        //                        cl.MouthE = item.Key.ToString();
        //                        cl.CountE = (float)item.NumProducts / c.NumProducts;
        //                        //cl.CountE = (float)item.NumProducts;
        //                        list1.Add(cl);
        //                    }
        //                }
        //            }
        //            AllNEmployeeentryInfo = GetClass2(list1);

        //            //已婚人员信息
        //            var YEmployeeentryInfo = from p in e.Result
        //                                     where p.MarriageEnty == "1"
        //                                     group p by p.DataEnty.Value.Month into g
        //                                     select new
        //                                     {
        //                                         g.Key,
        //                                         NumProducts = g.Count()
        //                                     };
        //            //遍历已婚人员信息,并且转换数据类型
        //            List<ReportChart> list2 = new List<ReportChart>();
        //            foreach (var item in YEmployeeentryInfo)
        //            {
        //                foreach (var c in count)
        //                {
        //                    if (item.Key == c.Key)
        //                    {
        //                        ReportChart cl = new ReportChart();
        //                        cl.MouthE = item.Key.ToString();
        //                        cl.CountE = (float)item.NumProducts / c.NumProducts;
        //                        //cl.CountE = (float)item.NumProducts;
        //                        list2.Add(cl);
        //                    }
        //                }
        //            }
        //            AllYEmployeeentryInfo = GetClass2(list2);

        //            //离异人员信息
        //            var LEmployeeentryInfo = from p in e.Result
        //                                     where p.MarriageEnty == "2"
        //                                     group p by p.DataEnty.Value.Month into g
        //                                     select new
        //                                     {
        //                                         g.Key,
        //                                         NumProducts = g.Count()
        //                                     };
        //            //遍历离异人员信息,并且转换数据类型
        //            List<ReportChart> list3 = new List<ReportChart>();
        //            foreach (var item in LEmployeeentryInfo)
        //            {
        //                foreach (var c in count)
        //                {
        //                    if (item.Key == c.Key)
        //                    {
        //                        ReportChart cl = new ReportChart();
        //                        cl.MouthE = item.Key.ToString();
        //                        cl.CountE = (float)item.NumProducts / c.NumProducts;
        //                        //cl.CountE = (float)item.NumProducts;
        //                        list2.Add(cl);
        //                    }
        //                }
        //            }
        //            AllLEmployeeentryInfo = GetClass2(list3);
        //            //首次创建
        //            CreateChart();
        //        }
        //        else
        //        {
        //            if (StrCompanyIDs == "")
        //            {
        //                MessageBox.Show("请选择公司!");
        //            }
        //            else
        //            {
        //                MessageBox.Show(nuYear.Value.ToString() + "年没有查询到数据!");
        //                AllNEmployeeentryInfo = null;
        //                AllYEmployeeentryInfo = null;
        //                AllLEmployeeentryInfo = null;
        //            }
        //            //CreateChart();
        //        }
        //    }
        //}
        //#endregion

        //#region 调用服务
        //private void showColumnChart()
        //{
        //    DateTime dtLandStart = new DateTime();
        //    DateTime dtLandEnd = new DateTime();
        //    DateTime.TryParse(nuYear.Value.ToString() + "-01-01", out dtLandStart);
        //    DateTime.TryParse(nuYear.Value.ToString() + "-12-31", out dtLandEnd);
        //    ChartClient.NewGetAllEmployeeentryInfoAsync(StrCompanyIDs, dtLandStart, dtLandEnd);
        //}
        //#endregion

        //#region 创建一个StackedColumn

        ///// <summary>
        ///// 创建一个多维柱状图StackedColumn
        ///// </summary>
        //public void CreateChart()
        //{

        //    ReportChartList.Children.Clear();
        //    Chart chart = new Chart();
        //    chart.ColorSet = "VisiRed"; // Visifire1
        //    chart.Loaded += new RoutedEventHandler(chart_Loaded);
        //    //更具用户选择，动态生成3D/2D图形
        //    if (this.RadY.IsChecked == true)
        //    {
        //        chart.View3D = true;
        //    }
        //    else if (this.RadN.IsChecked == true)
        //    {
        //        chart.View3D = false;
        //    }
        //    //设置Chart的style
        //    chart.Height = 700;
        //    chart.Width = 800;
        //    //chart.ShadowEnabled = false;
        //    //chart.Legends = false;
        //    //chart.SetValue(Canvas.LeftProperty, 30.0); 
        //    //chart.SetValue(Canvas.TopProperty, 30.0);
        //    //chart.BorderBrush = new SolidColorBrush(Colors.Gray); 
        //    //chart.AnimatedUpdate = true; 
        //    //chart.CornerRadius = new CornerRadius(7); 
        //    //chart.ShadowEnabled = true; 
        //     //chart.Padding = new Thickness(20, 20, 20, 50);
        //     //PlotArea plot = new PlotArea(); 
        //     //plot.ShadowEnabled = false; 
        //     //chart.PlotArea = plot;
        //     //添加Chart如表title
        //    Grid grid = new Grid();
        //    grid.Margin = new Thickness(15, 0, 0, 0);
        //    ColumnDefinition cd1 = new ColumnDefinition();
        //    grid.ColumnDefinitions.Add(cd1);
            
        //    Title title = new Title();
        //    title.Text = "员工婚姻状况分析图";
        //    grid.Children.Add(title);
        //    //chart.Titles.Add(title);
        //    //动态循环多个dataSeries、DataPoint
        //    for (int c = 0; c <= 2; c++)
        //    {
        //        //new第一个数据源
        //        DataSeries dataSeries = new DataSeries();
        //        dataSeries.RenderAs = RenderAs.StackedColumn;
        //        dataSeries.ShowInLegend = false;
        //        DataPoint dataPoint;
        //        List<ReportChart> listReportChart = null;

        //        //dataSeries.LegendText = c.ToString();
        //        //根据图例绑定源数据
        //        if (c == 0)
        //        {

        //            //dataSeries.LegendText = "未婚";
        //            listReportChart = AllNEmployeeentryInfo;
        //        }
        //        else if (c == 1)
        //        {
        //            //dataSeries.LegendText = "已婚";
        //            listReportChart = AllYEmployeeentryInfo;
        //        }
        //        else if (c == 2)
        //        {
        //            //dataSeries.LegendText = "离异";
        //            listReportChart = AllLEmployeeentryInfo;
        //        }

        //        if (listReportChart != null)
        //        {
        //            //未婚、已婚、离异
        //            foreach (ReportChart Employ in listReportChart)
        //            {
        //                dataPoint = new DataPoint();
        //                //Y
        //                dataPoint.YValue = Employ.CountE;
        //                //X
        //                dataPoint.AxisXLabel = Employ.MouthE + "月";
        //                //Add添加dataPoint
        //                dataSeries.DataPoints.Add(dataPoint);
        //            }
        //        }
        //        chart.Series.Add(dataSeries);
        //    }
        //    this.ReportChartList.Children.Add(chart);
        //    //HideDiv(this.ChartPanel);

        //}

        //void chart_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Chart chart = sender as Chart;

        //    chart.ColorSet = "Visifire1";
        //    if (chart != null)
        //    {
        //        // 新建一个 ColorSet 集合
        //        ColorSets emcs = new ColorSets();
        //        string resourceName = "Visifire.Charts.ColorSets.xaml";  // Visifire 默认颜色集合的文件
        //        using (System.IO.Stream s = typeof(Chart).Assembly.GetManifestResourceStream(resourceName))
        //        {
        //            if (s != null)
        //            {
        //                System.IO.StreamReader reader = new System.IO.StreamReader(s);
        //                String xaml = reader.ReadToEnd();
        //                emcs = System.Windows.Markup.XamlReader.Load(xaml) as ColorSets;
        //                reader.Close();
        //                s.Close();
        //            }
        //        }
        //        // 根据名称取得 Chart 的 ColorSet ( Chart 的 ColorSet 属性为颜色集的名称 )
        //        ColorSet cs = emcs.GetColorSetByName(chart.ColorSet);

        //        // 显示图例的 StackPanel
        //        StackPanel sp = new StackPanel()
        //        {
        //            Orientation = Orientation.Horizontal,
        //            VerticalAlignment = System.Windows.VerticalAlignment.Top,
        //            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        //        };

        //        // 图例文本
        //        string[] legendText = { "离异", "已婚", "未婚" };

        //        // 自定义图例
        //        for (int i = 0; i < chart.Series.Count(); i++)
        //        {
        //            Grid grid = new Grid();
        //            grid.Margin = new Thickness(15, 0, 0, 0);
        //            ColumnDefinition cd1 = new ColumnDefinition();
        //            grid.ColumnDefinitions.Add(cd1);
        //            ColumnDefinition cd2 = new ColumnDefinition();
        //            grid.ColumnDefinitions.Add(cd2);

        //            Rectangle rect = new Rectangle()
        //            {
        //                Width = 10,
        //                Height = 10,
        //                Fill = cs.Brushes[i]
        //            };
        //            rect.SetValue(Grid.ColumnProperty, 0);
        //            grid.Children.Add(rect);

        //            TextBlock tb = new TextBlock()
        //            {
        //                Text = legendText[i],
        //                Margin = new Thickness(5, 0, 0, 0),
        //                Foreground = cs.Brushes[i]
        //            };
        //            tb.SetValue(Grid.ColumnProperty, 1);
        //            grid.Children.Add(tb);

        //            sp.Children.Add(grid);
        //        }
        //        ReportChartList.Children.Add(sp);
        //    }
        //}

        //#endregion

        //#region 创建一个Pie

        ///// <summary>
        ///// 动态创建一个Pie
        ///// </summary>
        //private void CreateCreatePieChart()
        //{
        //    Chart chart = new Chart();
        //    if (this.RadY.IsChecked == true)
        //    {
        //        chart.View3D = true;
        //    }
        //    else if (this.RadN.IsChecked == true)
        //    {
        //        chart.View3D = false;
        //    }
        //    chart.Height = 600;
        //    chart.Width = 800;
        //    Title title = new Title();
        //    title.Text = "员工婚姻状况分析图";
        //    chart.Titles.Add(title);
        //    DataPoint dataPoint;
           
        //    for (int i = 0; i <3; i++)
        //    {
        //        DataSeries dataSeries = new DataSeries();
        //        dataSeries.RenderAs = RenderAs.Pie;
        //        //List<ReportChart> listReportChart = null;
        //        int t = 0;
        //        if (i==0)
        //        {
        //            dataSeries.LegendText = "未婚";
        //            t = rand.Next(10, 100);
        //        }
        //        else if (i==1)
        //        {
        //            dataSeries.LegendText = "已婚";
        //            t = rand.Next(10, 100);
        //        }
        //        else if(i==3)
        //        {
        //            dataSeries.LegendText = "离异";
        //            t = rand.Next(10, 100);
        //        }
        //        if (t!= 0)
        //        {
        //            dataPoint = new DataPoint();
        //            dataPoint.YValue = t;
        //            dataSeries.DataPoints.Add(dataPoint);
        //        }
        //        chart.Series.Add(dataSeries);
        //    }
        //    ReportChartList.Children.Add(chart);
        //    //this.currentSetp = 3;
        //}
        //#endregion

        //#region 创建柱状图
        ///// <summary>
        ///// 创建柱状图
        ///// </summary>
        //private void CreateCreateColumnChart()
        //{
        //    Chart chart = new Chart();
        //    if (this.RadY.IsChecked == true)
        //    {
        //        chart.View3D = true;
        //    }
        //    else if (this.RadN.IsChecked == true)
        //    {
        //        chart.View3D = false;
        //    }
        //    //chart.View3D = true;
        //    chart.Height = 600;
        //    chart.Width = 800;
        //    Title title = new Title();
        //    title.Text = "Bak";
        //    chart.Titles.Add(title);
        //    DataSeries dataSeries = new DataSeries();
        //    dataSeries.RenderAs = RenderAs.Column;
        //    dataSeries.LegendText = "";
        //    DataPoint dataPoint;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        dataPoint = new DataPoint();
        //        dataPoint.YValue = rand.Next(10, 100);
        //        //dataPoint.MouseLeftButtonDown += new MouseButtonEventHandler(dataPoint_MouseLeftButtonDownColumnChart);
        //        dataSeries.DataPoints.Add(dataPoint);

        //    }
        //    chart.Series.Add(dataSeries);
        //    this.ReportChartList.Children.Add(chart);
        //    //this.currentSetp = 2;
        //}
        //#endregion

        //#region 创建线图
        //private void CreateLineChart()
        //{
        //    Chart chart = new Chart();
        //    chart.View3D = true;
        //    chart.Height = 600;
        //    chart.Width = 800;
        //    Title title = new Title();
        //    title.Text = "Line";
        //    chart.Titles.Add(title);
        //    DataSeries dataSeries = new DataSeries();
        //    dataSeries.RenderAs = RenderAs.Line;
        //    dataSeries.LegendText = "";
        //    DataPoint dataPoint;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        dataPoint = new DataPoint();
        //        dataPoint.YValue = rand.Next(10, 100);
        //        dataSeries.DataPoints.Add(dataPoint);
        //    }
        //    chart.Series.Add(dataSeries);
        //    this.ReportChartList.Children.Add(chart);
        //}
        //#endregion

        //#region 点击按钮事件
        /// <summary>
        /// 点击按钮StackedColumn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_StackedColumn_Click(object sender, RoutedEventArgs e)
        {
            //showColumnChart();
            ///CreateCreatePieChart();
            //this.HideDispaly();
        }

        ///// <summary>
        ///// 点击按钮Back
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Btn_Back_Click(object sender, RoutedEventArgs e)
        //{
        //    CreateCreateColumnChart();
        //    this.HideDispaly();
        //}

        ///// <summary>
        ///// 点击按钮Pie
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Btn_Pie_Click(object sender, RoutedEventArgs e)
        //{
        //    CreateCreatePieChart();
        //    this.HideDispaly();
        //}

        ///// <summary>
        ///// 点击Line
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Btn_Line_Click(object sender, RoutedEventArgs e)
        //{
        //    CreateLineChart();
        //    this.HideDispaly();
        //}
        //#endregion

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

        //#region 隐藏水印
        //private void HideDiv(Grid g)
        //{
        //    //增加一个遮罩层到gr，将visifire的水印遮掉。     
        //    StackPanel sp = new StackPanel();
        //    sp.Width = 161;
        //    sp.Height = 18;
        //    sp.Margin = new Thickness(0, 3, 6, 0);
        //    sp.VerticalAlignment = VerticalAlignment.Top;
        //    sp.HorizontalAlignment = HorizontalAlignment.Right;
        //    sp.Background = new SolidColorBrush(Colors.White);
        //    g.Children.Add(sp);
        //    //LayoutRoot.Children.Add(g);
        //}
        //#endregion

        //#region 设置style
        ///// <summary>
        ///// 设置style
        ///// </summary>
        //private void HideDispaly()
        //{
        //}
        //#endregion

        //#region 导出图表
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //ElementToPNG eTP = new ElementToPNG();
            //eTP.ShowSaveDialog(this.ChartPanel);
        }
        //#endregion
        //Random rand = new Random(DateTime.Now.Millisecond);
    }
}
