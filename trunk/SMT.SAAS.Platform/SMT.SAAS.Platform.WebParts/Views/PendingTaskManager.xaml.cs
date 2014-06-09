/*最后修改人:安凯航
最后修改日期:2011年9月9日
最后修改内容:
待办任务新增两个待办获取接口
分页待办接口： 
   List<T_FLOW_ENGINEMSGLIST> PendingCacheTasksParmsPageIndex(MsgParms msgParams,bool IsAutofresh, ref bool HaveNewTask, ref int rowCount, ref int pageCount);
取待办Top条数接口：
   List<T_FLOW_ENGINEMSGLIST> PendingCacheMainTasksParms(MsgParms msgParams, bool IsAutofresh, ref bool HaveNewTask); 
    增加两个参数 
            IsAutofresh ：是否是自动刷新，如果是自动刷新值为true,手动刷新为false
            HaveNewTask:该参数是返回给门户的字段，是自动刷新，引擎缓存发现该用户没有新的待办将返回false,并且返回列表数据为null。
 *  引擎缓存如果发现该用户有新的待办将返回true,并且返回新的列表。
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SMT.SAAS.Main.CurrentContext;
using System.ServiceModel;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Browser;
using SMT.Saas.Tools.EngineWS;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class PendingTaskManager : UserControl, ICleanup
    {

        private DispatcherTimer _refdateTimer;
        private int _top = 20;
        private static string _state = "open";
        private int timeer = 10;
        private EngineWcfGlobalFunctionClient client;
        private MsgParms _params = null;
        private string _currentMessageID = string.Empty;
        private T_FLOW_ENGINEMSGLIST _currentEngineTask;
        private ViewModels.DataGridPageViewModel pageerVM;
        private string _filterStr = string.Empty;

        public PendingTaskManager()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PendingTask_Loaded);
            this.Unloaded += new RoutedEventHandler(PendingTaskManager_Unloaded);
        }



        public PendingTaskManager(string messageid, string state)
            : this()
        {
            _currentMessageID = messageid;
            _state = state;
            if (state == "open")
            {
                Results.Visibility = Visibility.Visible;
                Resulted.Visibility = Visibility.Collapsed;
            }
            else
            {
                Results.Visibility = Visibility.Collapsed;
                Resulted.Visibility = Visibility.Visible;
            }
            loading.Start();

        }



        void PendingTask_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        void PendingTaskManager_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        public void Initialize()
        {
            pageerVM = new ViewModels.DataGridPageViewModel();
            pageerVM.OnPageIndexChanged += new EventHandler(pageerVM_OnPageIndexChanged);
            Pager.DataContext = pageerVM;
            // Top = _top,
            _params = new MsgParms()
            {
                Status = _state,
                UserID = Common.CurrentLoginUserInfo.EmployeeID,
                PageSize = pageerVM.PageSize

            };

            _refdateTimer = new DispatcherTimer();
            _refdateTimer.Interval = new TimeSpan(0, 0, timeer);
            _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            client = new EngineWcfGlobalFunctionClient();

            client.PendingMainTasksParmsCompleted += new EventHandler<PendingMainTasksParmsCompletedEventArgs>(client_PendingMainTasksParmsCompleted);
            client.PendingDetailTasksCompleted += new EventHandler<PendingDetailTasksCompletedEventArgs>(client_PendingDetailTasksCompleted);
            client.PendingCacheTasksParmsPageIndexCompleted += new EventHandler<PendingCacheTasksParmsPageIndexCompletedEventArgs>(client_PendingCacheTasksParmsPageIndexCompleted);
            LoadDate();
        }

        void pageerVM_OnPageIndexChanged(object sender, EventArgs e)
        {
            LoadDate(_state, false);
        }


        public void LoadDate()
        {
            LoadDate(_state, false);
            client.PendingDetailTasksAsync(_currentMessageID);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (client != null)
            {
                _state = ((sender as TabControl).SelectedItem as TabItem).Tag.ToString();
                LoadDate(_state, false);
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            loading.Start();
            T_FLOW_ENGINEMSGLIST source = (sender as HyperlinkButton).DataContext as T_FLOW_ENGINEMSGLIST;
            _currentMessageID = source.MESSAGEID.ToString();

            client.PendingDetailTasksAsync(source.MESSAGEID.ToString());
        }

        int rowcount = 0;
        private void LoadDate(string state, bool IsAutofresh)
        {
            if (client.State == CommunicationState.Opened || client.State == CommunicationState.Created)
            {
                _params.Status = state;
                _params.PageIndex = pageerVM.PageIndex;

                client.PendingCacheTasksParmsPageIndexAsync(_params, IsAutofresh, false, rowcount, pageerVM.PageCount);
            }
        }

        private void _refdateTimer_Tick(object sender, EventArgs e)
        {
            _refdateTimer.Stop();
            LoadDate(_state, true);
        }

        private void client_PendingMainTasksParmsCompleted(object sender, PendingMainTasksParmsCompletedEventArgs e)
        {

        }

        void client_PendingCacheTasksParmsPageIndexCompleted(object sender, PendingCacheTasksParmsPageIndexCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    // IsAutofresh ：是否是自动刷新，如果是自动刷新值为true,手动刷新为false
                    //HaveNewTask:该参数是返回给门户的字段，是自动刷新，引擎缓存发现该用户没有新的待办将返回false,并且返回列表数据为null。引擎缓存如果发现该用户有新的待办将返回true,并且返回新的列表。
                    if (e.IsAutofresh && !e.HaveNewTask)
                    {
                        return;
                    }
                    if (e.Result != null)
                    {
                        pageerVM.PageCount = e.pageCount;

                        List<T_FLOW_ENGINEMSGLIST> result = new List<T_FLOW_ENGINEMSGLIST>();
                        foreach (var item in e.Result)
                        {
                            T_FLOW_ENGINEMSGLIST temp = item;
                            temp.CREATEDATE = item.CREATEDATE + " " + item.CREATETIME;
                            temp.OPERATIONDATE = item.OPERATIONDATE + " " + item.OPERATIONTIME;
                            result.Add(item);
                        }
                        Resulted.ItemsSource = null;
                        Results.ItemsSource = null;
                        switch (_state)
                        {
                            case "Close":
                                Resulted.ItemsSource = result; break;
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
                        pageerVM.PageCount = 0;
                    }

                }
            }
            finally
            {
                if (_refdateTimer != null && _state == "open")
                    _refdateTimer.Start();
            }
        }

        #region 解析单条引擎的详细信息
        private int i = 0;
        private void client_PendingDetailTasksCompleted(object sender, PendingDetailTasksCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        string titel = "";
                        if (!string.IsNullOrEmpty(e.Result.MODELNAME))
                        {
                            titel = e.Result.MODELNAME;
                        }

                        ViewModel.Context.MainPanel.SetTitel(titel);
                        _currentEngineTask = e.Result;
                        string messageContent = _currentEngineTask.APPLICATIONURL.Trim();
                        using (XmlReader reader = XmlReader.Create(new StringReader(messageContent)))
                        {
                            XElement xmlClient = XElement.Load(reader);
                            var temp = from c in xmlClient.DescendantsAndSelf("System")
                                       select c;
                            string AssemblyName = temp.Elements("AssemblyName").SingleOrDefault().Value.Trim();
                            string strUrl = temp.Elements("PageParameter").SingleOrDefault().Value.Trim();
                            string strOid = temp.Elements("ApplicationOrder").SingleOrDefault().Value.Trim();
                            if (AssemblyName == "GiftApplyMaster" || AssemblyName == "GiftPlan" || AssemblyName == "SumGiftPlan")
                            {
                                loading.Stop();
                                try
                                {
                                    HtmlWindow wd = HtmlPage.Window;
                                    strUrl = strUrl.Split(',')[0];
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

                                CheckeDepends(AssemblyName);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("待办任务打开异常,请查看系统日志！");
                Logging.Logger.Current.Log("10000", "Platform", "待办任务", "待办任务打开异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
        }

        //管理匿名事件
        EventHandler<ViewModel.LoadModuleEventArgs> LoadTaskHandler = null;
        private void CheckeDepends(string moduleName)
        {
            var module = ViewModel.Context.Managed.Catalog.FirstOrDefault(item => item.ModuleName == moduleName);
            if (module != null)
            {
                ViewModel.Context.Managed.OnSystemLoadModuleCompleted += LoadTaskHandler = (o, e) =>
                {
                    ViewModel.Context.Managed.OnSystemLoadModuleCompleted -= LoadTaskHandler;
                    if (e.Error == null)
                    {
                        ResolverTask(_currentEngineTask);
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }
            else
            {

            }
        }

        /// <summary>
        /// 解析待办任务
        /// </summary>
        /// <param name="engineTask"></param>
        private void ResolverTask(T_FLOW_ENGINEMSGLIST engineTask)
        {
            try
            {
                #region 解析代码任务的内容
                string messageContent = engineTask.APPLICATIONURL.Trim();

                if (messageContent.Length > 0)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("ResolverTask(messageContent):" + messageContent);
                    using (XmlReader reader = XmlReader.Create(new StringReader(messageContent)))
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

                        //  if (AssemblyName == "SMT.FB.UI")
                        //      defaultVersion = " , Version=1.1.1.1230, Culture=neutral, PublicKeyToken=null";

                        StringBuilder typeString = new StringBuilder();
                        typeString.Append(PublicClass);
                        typeString.Append(", ");
                        typeString.Append(AssemblyName);
                        typeString.Append(defaultVersion);

                        Type type = Type.GetType(typeString.ToString());

                        bool isKPI = false;
                        string TmpFieldValueString = string.Empty;
                        if (engineTask.APPFIELDVALUE.Length > 0)
                        {
                            TmpFieldValueString = engineTask.APPFIELDVALUE;
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
                            Type[] types = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(Border) };
                            MethodInfo method = type.GetMethod(ProcessName, types);

                            if (method == null)
                                throw new Exception(string.Format("未找到匹配的公共方法，请检测是否存在方法签名为{0}(string,string,string,string,Border);的公共方法", ProcessName));


                            method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType, TmpFieldValueString, borTaskContent }, null);
                        }
                        else
                        {
                            borTaskContent.Child = null;

                            Type[] types = new Type[] { typeof(string), typeof(string), typeof(string), typeof(Border) };

                            MethodInfo method = type.GetMethod(ProcessName, types);

                            if (method == null)
                                throw new Exception(string.Format("未找到匹配的公共方法，请检测是否存在方法签名为{0}(string,string,string,Border);的公共方法", ProcessName));

                            method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType, borTaskContent }, null);

                        }

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("待办任务打开异常,请查看系统日志！");
                Logging.Logger.Current.Log("10000", "Platform", "待办任务", "待办任务打开异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
            finally
            {
                loading.Stop();
            }
        }
        #endregion

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

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            _params.MessageBody = txtFilterStr.Text.Trim();

            LoadDate(_state, false);
        }
    }
}