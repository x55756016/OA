using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Browser;
using SMT.Saas.Tools.OACommonAdminWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
//using SMT.SaaS.Platform;

using SMT.SAAS.Controls.Toolkit.Windows;

using SMT.Saas.Tools.EngineWS;


namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class OAWebPart :UserControl/* BaseForm*/, IClient, SMT.SaaS.Platform.IWebPart
    {
        private DispatcherTimer _refdateTimer;
        private SmtOACommonAdminClient client;
        //private DispatcherTimer _refdateTimer;

        private int timeer = 10;

        private SMTLoading loadbar = new SMTLoading();
        private string _currentXmlObj = string.Empty;
        AsyncTools ayTools = new AsyncTools();

        public OAWebPart()
        {
            InitializeComponent();

            //_refdateTimer = new DispatcherTimer();
            //_refdateTimer.Interval = new TimeSpan(0, 0, 10);
            //_refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            
            //LoadData();
            PARENT.Children.Add(loadbar);
            
            this.Loaded += new RoutedEventHandler(WebPart_Loaded);
        }

        void WebPart_Loaded(object sender, RoutedEventArgs e)
        {
            _refdateTimer = new DispatcherTimer();
            _refdateTimer.Interval = new TimeSpan(0, 0, timeer);
            _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            client = new SmtOACommonAdminClient();
            client.GetHouseIssueAndNoticeInfosCompleted += new EventHandler<GetHouseIssueAndNoticeInfosCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosCompleted);
            client.GetHouseIssueAndNoticeInfosToMobileCompleted += new EventHandler<GetHouseIssueAndNoticeInfosToMobileCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosToMobileCompleted);
            ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
            LoadData();
        }

        void client_GetHouseIssueAndNoticeInfosToMobileCompleted(object sender, GetHouseIssueAndNoticeInfosToMobileCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<V_SystemNotice> aa = e.Result.ToList();
            }
        }
        void NewsWebPart_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void _refdateTimer_Tick(object sender, EventArgs e)
        {
            //_refdateTimer.Stop();
            LoadData();
        }




        void LoadData()
        {
            System.Collections.ObjectModel.ObservableCollection<string> posts = new System.Collections.ObjectModel.ObservableCollection<string>();
            System.Collections.ObjectModel.ObservableCollection<string> companys = new System.Collections.ObjectModel.ObservableCollection<string>();
            System.Collections.ObjectModel.ObservableCollection<string> departs = new System.Collections.ObjectModel.ObservableCollection<string>();
            for (int i = 0; i < Common.CurrentLoginUserInfo.UserPosts.Count(); i++)
            {
                posts.Add(Common.CurrentLoginUserInfo.UserPosts[i].PostID);
                companys.Add(Common.CurrentLoginUserInfo.UserPosts[i].CompanyID);
                departs.Add(Common.CurrentLoginUserInfo.UserPosts[i].DepartmentID);
            }

            client.GetHouseIssueAndNoticeInfosAsync(Common.CurrentLoginUserInfo.EmployeeID, posts, companys, departs);
        }
        #region IWebPart 成员

        public event EventHandler OnMoreChanged;

        public void RefreshData()
        {

        }

        public int RowCount
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void ShowMaxiWebPart()
        {

        }

        public void ShowMiniWebPart()
        {

        }
        public int RefreshTime
        {
            get;
            set;
        }

        #endregion

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hbnView = sender as HyperlinkButton;
            if (hbnView == null)
            {
                return;
            }

            if (hbnView.Content == null)
            {
                return;
            }

            _currentXmlObj = ((TextBlock)hbnView.Content).Tag.ToString();
            _currentXmlObj = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                        <System><AssemblyName>SMT.SaaS.OA.UI</AssemblyName>
                        <PublicClass>SMT.SaaS.OA.UI.UserControls.OAWebPart</PublicClass>
                        <ProcessName>ShowCompanyDoc</ProcessName>
                        <PageParameter>SMT.SaaS.OA.UI.UserControls.CompanyDocWebPart</PageParameter>
                        <ApplicationOrder>" + _currentXmlObj+@"</ApplicationOrder>
                        <FormTypes>Audit</FormTypes>
                        </System>";

            if (string.IsNullOrWhiteSpace(_currentXmlObj))
            {
                return;
            }

            loadbar.Start();
            ayTools.BeginRun();
        }

        void ayTools_InitAsyncCompleted(object sender, EventArgs e)
        {
            loadbar.Stop();
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
                        //wd.Navigate(uri, "_bank");
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

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        public void Stop()
        {
            if (_refdateTimer != null)
                _refdateTimer.Stop();
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        void client_GetHouseIssueAndNoticeInfosCompleted(object sender, GetHouseIssueAndNoticeInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    NewsList.ItemsSource = null;
                    NewsList.ItemsSource = e.Result.ToList();

                    //_refdateTimer.Tick -= new EventHandler(_refdateTimer_Tick);
                    //_refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
                    //_refdateTimer.Start();

                    //List<V_SystemNotice> aa = e.Result.ToList();
                    //DaGr.ItemsSource = aa;
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
                        NavigateToForm(_currentXmlObj);
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }
        }

        private void NavigateToForm(string engineTask)
        {

            try
            {
                #region 解析代码任务的内容
                string messageContent = engineTask.Trim();
                if (messageContent.Length > 0)
                {
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

                        string defaultVersion = ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

                        //if (AssemblyName == "SMT.FB.UI")
                        //    defaultVersion = " , Version=1.1.1.1230, Culture=neutral, PublicKeyToken=null";

                        StringBuilder typeString = new StringBuilder();
                        typeString.Append(PublicClass);
                        typeString.Append(", ");
                        typeString.Append(AssemblyName);
                        typeString.Append(defaultVersion);

                        Type type = Type.GetType(typeString.ToString());

                        //Type[] types = new Type[] { typeof(string), typeof(string), typeof(string) };
                        Type[] types = new Type[] { typeof(string) };

                        MethodInfo method = type.GetMethod(ProcessName, types);
                        method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder }, null);
                    }
                }
                #endregion
            }

            catch (Exception ex)
            {
                MessageBox.Show("我的单据打开异常,请查看系统日志！");
                Logging.Logger.Current.Log("10000", "Platform", "我的单据", "我的单据打开异常", ex, Logging.Category.Exception, Logging.Priority.High);

            }
        }
    }
}
                