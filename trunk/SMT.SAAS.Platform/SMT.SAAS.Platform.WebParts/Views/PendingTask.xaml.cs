/*最后修改人:安凯航
最后修改日期:2011年9月9日
最后修改内容:
待办任务新增两个待办获取接口
分页待办接口： 
   List<T_FLOW_ENGINEMSGLIST> PendingCacheTasksParmsPageIndex(MsgParms msgParams,ref bool IsAutofresh, ref bool HaveNewTask, ref int rowCount, ref int pageCount);
取待办Top条数接口：
   List<T_FLOW_ENGINEMSGLIST> PendingCacheMainTasksParms(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask); 
    增加两个参数 
            IsAutofresh ：是否是自动刷新，如果是自动刷新值为true,手动刷新为false
            HaveNewTask:该参数是返回给门户的字段，是自动刷新，引擎缓存发现该用户没有新的待办将返回false,并且返回列表数据为null。引擎缓存如果发现该用户有新的待办将返回true,并且返回新的列表。
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Browser;
using SMT.Saas.Tools.EngineWS;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class PendingTask : UserControl, IWebpart
    {
        private DispatcherTimer _refdateTimer;
        private int _top = 20;
        private static string _state = "open";
        private int timeer = 10;
        private EngineWcfGlobalFunctionClient client;
        private MsgParms _params = null;
        private string _currentMessageID = string.Empty;
        //private bool IsAutofresh = false;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _currentXmlObj = string.Empty;
        private string _currentSourceMsgID = string.Empty;
        AsyncTools ayTools = new AsyncTools();

        public PendingTask()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PendingTask_Loaded);
        }

        void PendingTask_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading != null)
                loading.Start();

            if (client != null)
            {
                _state = ((sender as TabControl).SelectedItem as TabItem).Tag.ToString();

                Resulted.ItemsSource = null;
                Results.ItemsSource = null;
                if (_state == "open")
                {
                    Resulted.Visibility = Visibility.Collapsed;
                    Results.Visibility = Visibility.Visible;
                }
                else
                {
                    Results.Visibility = Visibility.Collapsed;
                    Resulted.Visibility = Visibility.Visible;
                }

                LoadDate(_state, false);
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            T_FLOW_ENGINEMSGLIST source = (sender as HyperlinkButton).DataContext as T_FLOW_ENGINEMSGLIST;

            ShowTask(source.MESSAGEID, source.APPLICATIONURL);           
        }

        public void ShowTask(string strMessageid, string strConfig)
        {
            _currentSourceMsgID = strMessageid;
            _currentXmlObj = strConfig;

            if (string.IsNullOrWhiteSpace(_currentXmlObj))
            {
                return;
            }

            loading.Start();
            ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
            ayTools.BeginRun();
        }

        void ayTools_InitAsyncCompleted(object sender, EventArgs e)
        {
            loading.Stop();
            PendingTaskManager manager = new PendingTaskManager(_currentSourceMsgID, _state);
            using (XmlReader reader = XmlReader.Create(new StringReader(_currentXmlObj)))
            {
                XElement xmlClient = XElement.Load(reader);
                var temp = from c in xmlClient.DescendantsAndSelf("System")
                           select c;
                string AssemblyName = temp.Elements("AssemblyName").SingleOrDefault().Value.Trim();
                string strUrl = temp.Elements("PageParameter").SingleOrDefault().Value.Trim();
                string strOid = temp.Elements("ApplicationOrder").SingleOrDefault().Value.Trim();
                if (AssemblyName == "GiftApplyMaster" || AssemblyName == "GiftPlan" || AssemblyName == "SumGiftPlan")
                {

                    try
                    {
                        HtmlWindow wd = HtmlPage.Window;
                        if (strUrl.IndexOf('?') > -1)
                        {
                            strUrl = strUrl + "&uid=" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + "&oid=" + strOid;
                        }
                        else
                        {
                            strUrl = strUrl + "?uid=" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + "&oid=" + strOid;
                        }
                        string strHost = SMT.SAAS.Main.CurrentContext.Common.HostAddress.ToString().Split('/')[0];
                        strUrl = "http://" + strHost + "/" + strUrl;
                        Uri uri = new Uri(strUrl);
                        HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
                        options.Directories = false;
                        options.Location = false;
                        options.Menubar = false;
                        options.Status = false;
                        options.Toolbar = false;
                        options.Status = false;
                        options.Resizeable = true;
                        options.Left = 280;
                        options.Top = 100;
                        options.Width = 800;
                        options.Height = 600;
                        // HtmlPage.PopupWindow(uri, AssemblyName, options);
                        string strWindow = System.DateTime.Now.ToString("yyMMddHHmsssfff");
                        wd.Navigate(uri, strWindow, "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");
                    }
                    catch
                    {
                        MessageBox.Show("模块链接异常：" + strUrl);
                    }
                }
                else
                {
                    if (ViewModel.Context.MainPanel != null)
                    {
                        if (ViewModel.Context.MainPanel.DefaultContent != null)
                        {
                            IWebpart webpart = ViewModel.Context.MainPanel.DefaultContent as IWebpart;
                            if (webpart != null)
                                webpart.Stop();

                        }
                        Stop();
                        ViewModel.Context.MainPanel.Navigation(manager, "待办任务");
                    }
                }
            }
        }

        private void LoadDate(string state, bool IsAutofresh)
        {
            if (client.State == CommunicationState.Opened || client.State == CommunicationState.Created)
            {
                _params.Status = state;
                //bool haveNewTask = false;
                //_startTime = DateTime.Now;
                client.PendingCacheMainTasksParmsAsync(_params, IsAutofresh, false);
            }
        }

        private void _refdateTimer_Tick(object sender, EventArgs e)
        {
            _refdateTimer.Stop();
            LoadDate(_state, true);
        }

        private void client_PendingMainTasksParmsCompleted(object sender, PendingCacheMainTasksParmsCompletedEventArgs e)
        {
            try
            {
                if (loading != null)
                    loading.Stop();

                // IsAutofresh ：是否是自动刷新，如果是自动刷新值为true,手动刷新为false
                // HaveNewTask:该参数是返回给门户的字段，是自动刷新，
                //引擎缓存发现该用户没有新的待办将返回false,并且返回列表数据为null。
                //引擎缓存如果发现该用户有新的待办将返回true,并且返回新的列表。
                if (e.IsAutofresh && !e.HaveNewTask)
                {
                    return;
                }
                if (e.Result != null)
                {
                    List<T_FLOW_ENGINEMSGLIST> result = new List<T_FLOW_ENGINEMSGLIST>();
                    foreach (var item in e.Result)
                    {
                        T_FLOW_ENGINEMSGLIST temp = item;

                        temp.CREATEDATE = item.CREATEDATE + " " + item.CREATETIME;
                        temp.OPERATIONDATE = item.OPERATIONDATE + " " + item.OPERATIONTIME;
                        result.Add(item);
                    }

                    switch (_state)
                    {
                        case "Close":
                            Resulted.ItemsSource = result.OrderByDescending(item => item.OPERATIONDATE); break;
                        case "open":
                            //Results.ItemsSource = result.OrderBy(item => item.CREATEDATE); break;
                            Results.ItemsSource = result; break;
                        default:
                            break;
                    }
                }
                else
                {
                    Resulted.ItemsSource = null;
                    Results.ItemsSource = null;

                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Current.Log("10000", "Platform", "待办任务", "读取待办任务异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
            finally
            {
                //_endTime = DateTime.Now;
                //WriterLog();
                if (_refdateTimer != null && _state == "open")
                    _refdateTimer.Start();
            }
        }

        private void client_PendingDetailTasksCompleted(object sender, PendingDetailTasksCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        T_FLOW_ENGINEMSGLIST source = e.Result;
                        string sss = source.APPLICATIONURL.Trim();
                        if (sss.Length > 0)
                        {
                            using (XmlReader reader = XmlReader.Create(new StringReader(sss)))
                            {
                                XElement xmlClient = XElement.Load(reader);
                                var temp = from c in xmlClient.DescendantsAndSelf("System")
                                           select c;
                                string AssemblyName = temp.Elements("AssemblyName").SingleOrDefault().Value.Trim();
                                string PublicClass = temp.Elements("PublicClass").SingleOrDefault().Value.Trim();
                                string ProcessName = temp.Elements("ProcessName").SingleOrDefault().Value.Trim();
                                string PageParameter = temp.Elements("PageParameter").SingleOrDefault().Value.Trim();
                                string ApplicationOrder = temp.Elements("ApplicationOrder").SingleOrDefault().Value.Trim();
                                string FormType = temp.Elements("FormTypes").SingleOrDefault().Value.Trim();

                                string defaultVersion = " , Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                                StringBuilder typeString = new StringBuilder();
                                typeString.Append(PublicClass);
                                typeString.Append(", ");
                                typeString.Append(AssemblyName);
                                typeString.Append(defaultVersion);

                                Type type = Type.GetType(typeString.ToString());

                                object instance = Activator.CreateInstance(type);

                                bool isKPI = false;
                                string TmpFieldValueString = string.Empty;
                                if (source.APPFIELDVALUE.Length > 0)
                                {
                                    TmpFieldValueString = source.APPFIELDVALUE;
                                    string[] valuerownode = TmpFieldValueString.Split('Ё');
                                    for (int j = 0; j < valuerownode.Length; j++)
                                    {
                                        if (valuerownode[j] != "")
                                        {
                                            string[] valuecolnode = valuerownode[j].Split('|');
                                            if (valuecolnode[0] == "IsKpi")
                                            {
                                                isKPI = valuecolnode[1] == "1" ? true : false;
                                            }
                                        }
                                    }
                                }

                                if (isKPI)
                                {
                                    TmpFieldValueString += "ЁMESSAGEID|" + _currentMessageID;
                                    Type[] types = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) };
                                    MethodInfo method = type.GetMethod(ProcessName, types);
                                    method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType, TmpFieldValueString }, null);
                                }
                                else
                                {

                                    Type[] types = new Type[] { typeof(string), typeof(string), typeof(string) };
                                    MethodInfo method = type.GetMethod(ProcessName, types);
                                    method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType }, null);

                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
            }
        }

        public int RefDate
        {
            get;
            set;
        }

        public string Titel
        {
            get;
            set;
        }

        public void LoadDate()
        {
            LoadDate(_state, false);
        }

        /// <summary>
        /// 是否已加载过.
        /// added by 安凯航.2011年9月5日
        /// </summary>
        private static bool isLoaded;
        public void Initialize()
        {
            _params = new MsgParms() { Status = _state, Top = _top, UserID = Common.CurrentLoginUserInfo.EmployeeID };

            _refdateTimer = new DispatcherTimer();
            _refdateTimer.Interval = new TimeSpan(0, 0, timeer);
            _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            client = new EngineWcfGlobalFunctionClient();
            client.PendingCacheMainTasksParmsCompleted += new EventHandler<PendingCacheMainTasksParmsCompletedEventArgs>(client_PendingMainTasksParmsCompleted);
            client.PendingDetailTasksCompleted += new EventHandler<PendingDetailTasksCompletedEventArgs>(client_PendingDetailTasksCompleted);
            //if (!isLoaded)
            //{
            //    LoadDate();
            //    isLoaded = true;
            //}
            //else
            //{
            //    _refdateTimer.Start();
            //}

            LoadDate();
        }

        public void Cleanup()
        {

            if (_refdateTimer != null)
            {
                _refdateTimer.Tick -= _refdateTimer_Tick;
                _refdateTimer.Stop();
                _refdateTimer = null;

            }
            if (client != null)
                client = null;
        }

        public void Stop()
        {
            if (_refdateTimer != null)
                _refdateTimer.Stop();
        }

        public void Star()
        {
            LoadDate();
        }

        private void btnCreateTask_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTask createnewtask = new CreateNewTask();
            if (ViewModel.Context.MainPanel != null)
                ViewModel.Context.MainPanel.Navigation(createnewtask, "新建任务");
        }

        private void WriterLog()
        {
            TimeSpan ts = _endTime-_startTime ;
            double d = ts.TotalMilliseconds;
            string msg = string.Format("引擎单次请求耗时：{0} 毫秒", d);
            Logging.Logger.Current.Log("10000", "Platform", "待办任务", msg, new Exception(), Logging.Category.Exception, Logging.Priority.High);

        }
    }
}
