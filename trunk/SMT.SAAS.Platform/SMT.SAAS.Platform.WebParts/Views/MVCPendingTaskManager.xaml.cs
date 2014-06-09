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
    public partial class MVCPendingTaskManager : UserControl, ICleanup
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
        private bool bMVCOpen = false;
        public bool isMyrecord = false;
        public string applicationUrl = string.Empty;
        public string Titel = string.Empty;

        public MVCPendingTaskManager()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PendingTask_Loaded);
            this.Unloaded += new RoutedEventHandler(PendingTaskManager_Unloaded);
        }



        public MVCPendingTaskManager(string messageid, string state)
            : this()
        {
            _currentMessageID = messageid;
            _state = state;
            if (!System.Windows.Application.Current.Resources.Contains("CurrentSysUserID"))
            {
                MessageBox.Show(@"System.Windows.Application.Current.Resources 没有传入 CurrentSysUserID
                ，无法打开系统，请联系管理员");
                return;
            }
            if (!System.Windows.Application.Current.Resources.Contains("MvcOpenRecordSource"))
            {
                MessageBox.Show(@"System.Windows.Application.Current.Resources 没有传入 MvcOpenRecordSource
                ，无法打开系统，请联系管理员");
                return;
            }

            if (System.Windows.Application.Current.Resources["CurrentSysUserID"] != null && System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                bMVCOpen = true;
                loading.Start();
                return;
            }

        }

        void PendingTask_Loaded(object sender, RoutedEventArgs e)
        {
            //AppContext.ShowSystemMessageText();
            if (isMyrecord == false)
            {
                AppContext.SystemMessage("PendingTask_Loaded 开始打开待办任务");               
                Initialize();
            }
            else
            {
                AppContext.SystemMessage("PendingTask_Loaded 开始打开我的单据");
                OpenFromXML(Titel, applicationUrl);
            }
        }
        void PendingTaskManager_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        public void Initialize()
        {
            client = new EngineWcfGlobalFunctionClient();            
            client.PendingDetailTasksCompleted += new EventHandler<PendingDetailTasksCompletedEventArgs>(client_PendingDetailTasksCompleted);
            
            LoadDate();
        }


        public void LoadDate()
        {
            client.PendingDetailTasksAsync(_currentMessageID);
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
                        if (!string.IsNullOrEmpty(e.Result.MODELNAME))
                        {
                            Titel = e.Result.MODELNAME;
                        }                       
                        _currentEngineTask = e.Result;

                        applicationUrl = _currentEngineTask.APPLICATIONURL.Trim();

                        OpenFromXML(Titel, applicationUrl);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("待办任务打开异常,请查看系统日志！");
                Logging.Logger.Current.Log("10000", "Platform", "待办任务", "待办任务打开异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
        }

        private void OpenFromXML(string titel,string applicationUrl)
        {

            ViewModel.Context.MainPanel.SetTitel(titel);

            using (XmlReader reader = XmlReader.Create(new StringReader(applicationUrl)))
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
                        ResolverTask();
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }
        }

        /// <summary>
        /// 解析待办任务
        /// </summary>
        /// <param name="engineTask"></param>
        private void ResolverTask()
        {
            try
            {
                #region 解析代码任务的内容
                //string applicationUrl = engineTask.APPLICATIONURL.Trim();

                if (applicationUrl.Length > 0)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("ResolverTask()applicationUrl:" + applicationUrl);
                    using (XmlReader reader = XmlReader.Create(new StringReader(applicationUrl)))
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
                        //if (engineTask.APPFIELDVALUE.Length > 0)
                        //{
                        //    TmpFieldValueString = engineTask.APPFIELDVALUE;
                        //    string[] valuerownode = TmpFieldValueString.Split('Ё');
                        //    for (int j = 0; j < valuerownode.Length; j++)
                        //    {
                        //        if (valuerownode[j] != "")
                        //        {
                        //            string[] valuecolnode = valuerownode[j].Split('|');
                        //            if (valuecolnode[0] == "IsKpi")
                        //            {
                        //                isKPI = valuecolnode[1] == "1" ? true : false;
                        //            }
                        //        }
                        //    }
                        //}

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
                            if (PageParameter == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                            {
                                types = new Type[] { typeof(string), typeof(string), typeof(string) };
                            }
                            MethodInfo method = type.GetMethod(ProcessName, types);

                            if (method == null)
                                throw new Exception(string.Format("未找到匹配的公共方法，请检测是否存在方法签名为{0}(string,string,string,Border);的公共方法", ProcessName));
                            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("开始调用业务系统打开单据方法,typeString:"
                            + typeString + "ProcessName:" + ProcessName+ "types:" + types);
                            //SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                            if (PageParameter == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                            {
                                method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType}, null);
                            }
                            else
                            {
                                method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType, borTaskContent }, null);
                            }
                        }

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("待办任务打开异常,请查看系统日志！");
                AppContext.SystemMessage("待办任务打开异常,请查看系统日志！"+ex.ToString());
                AppContext.ShowSystemMessageText();
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
            if (client != null)
                client = null;
        }
    }
}