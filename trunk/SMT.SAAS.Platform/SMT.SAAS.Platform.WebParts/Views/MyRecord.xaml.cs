using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Windows.Threading;

using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using SMT.Saas.Tools.PersonalRecordWS;
using System.Windows.Browser;
using System.Threading;


namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class MyRecord : UserControl, IWebpart
    {


        PersonalRecordServiceClient clientPerRd = new PersonalRecordServiceClient();
        string strCheckState = string.Empty, BeginDate = string.Empty, EndDate = string.Empty;
        string strIsForward = string.Empty;
        private DispatcherTimer _refdateTimer;
        private SMTLoading loadbar = new SMTLoading();
        private string _currentXmlObj = string.Empty;
        AsyncTools ayTools = new AsyncTools();
        public MyRecord()
        {
            InitializeComponent();
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            clientPerRd.GetPersonalRecordListCompleted += new EventHandler<GetPersonalRecordListCompletedEventArgs>(clientPerRd_GetPersonalRecordListCompleted);
            clientPerRd.GetPersonalRecordCompleted += new EventHandler<GetPersonalRecordCompletedEventArgs>(clientPerRd_GetPersonalRecordCompleted);
            this.Pager.Click += new SaaS.DatePager.SmtPager.PagerButtonClick(Pager_Click);
            ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
        }
        /// <summary>
        /// 禁用键盘切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;

        }

        /// <summary>
        /// 禁用键盘切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = false;

        }

        void Pager_Click(object sender, RoutedEventArgs e)
        {
            int index = this.Pager.PageIndex;//分页索引        
            string strHeaderText = ((TabItem)tcPersonalRd.SelectedItem).Header.ToString();
            switch (strHeaderText)
            {
                //case "未提交":
                //    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();
                //    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                 
                //    BindGrid(index);
                //    break;
                case "审核中":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approving).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                   
                    InitGrid(index);
                    break;
                case "审核通过":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approved).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                  
                    InitGrid(index);
                    break;
                case "审核未通过":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnApproved).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                  
                    InitGrid(index);
                    break;
                case "转发":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approved).ToString();
                    strIsForward = "1";     //"0"表示非转发单据，"1"表示转发单据                 
                    BindGrid(index);
                    break;
                default:
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                  
                    BindGrid(index);
                    break;
            }


        }

        /// <summary>
        /// 加载未提交、和转发的数据
        /// </summary>
        private void BindGrid(int pageIndex)
        {
            string strCreateUserID = string.Empty, strfilterString = string.Empty, strSortKey = string.Empty;
            int pageCount = 0;
            strCreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //strCreateUserID = "5347a18b-5c9c-4e40-a1c4-ce30389224ed";//
            strSortKey = " CREATEDATE DESC ";
            strfilterString = "";//过滤条件，严格按照标准SQL语句         
            clientPerRd.GetPersonalRecordListAsync(pageIndex, strSortKey, strCheckState, strfilterString, strCreateUserID, strIsForward, BeginDate, EndDate, pageCount);

            loadbar.Start();
        }


        /// <summary>
        /// 加载审批中、审批通过、审批不通过的数据
        /// </summary>
        private void InitGrid(int pageIndex)
        {
            string strCreateUserID = string.Empty, strfilterString = string.Empty, strSortKey = string.Empty;
            int pageCount = 0;
            strCreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //strCreateUserID = "5347a18b-5c9c-4e40-a1c4-ce30389224ed";  
            strSortKey = " CREATEDATE DESC ";
            strfilterString = "";//过滤条件，严格按照标准SQL语句        
            clientPerRd.GetPersonalRecordAsync(pageIndex, strSortKey, strCheckState, strfilterString, strCreateUserID, BeginDate, EndDate, pageCount);
            loadbar.Start();
        }



        /// <summary>
        /// 加载PersonalRecord WebPart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PersonalRdWebPart_Loaded(object sender, RoutedEventArgs e)
        {
            BindGrid(1);
        }

        void clientPerRd_GetPersonalRecordCompleted(object sender, GetPersonalRecordCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                this.Pager.PageCount = e.pageCount;
                ObservableCollection<T_PF_PERSONALRECORD> entTemps = e.Result;
                if (e.Result == null)
                {
                    return;
                }
                lbApprovingPRList.ItemsSource = null;
                lbApprovedPRList.ItemsSource = null;
                lbUnApprovedPRList.ItemsSource = null;
                string strHeaderText = ((TabItem)tcPersonalRd.SelectedItem).Header.ToString();
                switch (strHeaderText)
                {
                    case "审核中":
                        lbApprovingPRList.ItemsSource = entTemps;
                        break;
                    case "审核通过":
                        lbApprovedPRList.ItemsSource = entTemps;
                        break;
                    case "审核未通过":
                        lbUnApprovedPRList.ItemsSource = entTemps;
                        break;
                }
            }
        }

        /// <summary>
        /// 绑定数据到DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPerRd_GetPersonalRecordListCompleted(object sender, GetPersonalRecordListCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                this.Pager.PageCount = e.pageCount;
                ObservableCollection<T_PF_PERSONALRECORD> entTemps = e.Result;
                if (e.Result == null)
                {
                    return;
                }
                //lbUnSubmitPRList.ItemsSource = null;
                lbApprovingPRList.ItemsSource = null;
                lbApprovedPRList.ItemsSource = null;
                lbUnApprovedPRList.ItemsSource = null;
                string strHeaderText = ((TabItem)tcPersonalRd.SelectedItem).Header.ToString();
                switch (strHeaderText)
                {
                    //case "未提交":
                    //    lbUnSubmitPRList.ItemsSource = entTemps;
                    //    break;
                    case "转发":
                        lbForwardPRList.ItemsSource = entTemps;
                        break;
                }
            }
        }

        #region IWebPart 成员

        public int RefreshTime
        {
            get;
            set;
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

        public void RefreshData()
        {
        }

        public event EventHandler OnMoreChanged;

        #endregion


        /// <summary>
        /// Tab页选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcPersonalRd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tc = sender as TabControl;
            if (tc == null)
            {
                return;
            }
            if (this.Pager != null)
            {
                this.Pager.PageCount = 0;
                this.Pager.PageIndex = 1;
            }
            string strHeaderText = ((TabItem)tc.SelectedItem).Header.ToString();
            foreach (object obj in tc.Items)
            {
                TabItem tiCurr = obj as TabItem;
                if (tiCurr.Header.ToString() != strHeaderText)
                {
                    tiCurr.Tag = "close";
                }
                else
                {
                    tiCurr.Tag = "open";
                }
            }

            switch (strHeaderText)
            {
                //case "未提交":
                //    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();
                //    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                
                //    BindGrid(1);//第一次分页索引是1
                //    break;
                case "审核中":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approving).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据 
                    //strCreateDateFrom = DateTime.Now.AddDays(-30).ToString();
                    //strCreateDateTo = DateTime.Now.ToString();
                    InitGrid(1);
                    break;
                case "审核通过":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approved).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                  
                    InitGrid(1);
                    break;
                case "审核未通过":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnApproved).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                  
                    InitGrid(1);
                    break;
                case "转发":
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approved).ToString();
                    strIsForward = "1";     //"0"表示非转发单据，"1"表示转发单据                  
                    BindGrid(1);
                    break;
                default:
                    strCheckState = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approving).ToString();
                    strIsForward = "0";     //"0"表示非转发单据，"1"表示转发单据                 
                    BindGrid(1);
                    break;
            }
        }

        /// <summary>
        /// 查看单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            ShowMyRecord(((TextBlock)hbnView.Content).Tag.ToString());
        }

        public void ShowMyRecord(string strConfig)
        {
            loadbar.Start();
            _currentXmlObj = strConfig;
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("strConfig:" + strConfig);
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

        //管理匿名事件
        EventHandler<ViewModel.LoadModuleEventArgs> LoadRecordHandler = null;
        private void CheckeDepends(string moduleName)
        {
            var module = ViewModel.Context.Managed.Catalog.FirstOrDefault(item => item.ModuleName == moduleName);
            if (module != null)
            {
                ViewModel.Context.Managed.OnSystemLoadModuleCompleted += LoadRecordHandler = (o, e) =>
                {
                    ViewModel.Context.Managed.OnSystemLoadModuleCompleted -= LoadRecordHandler;
                    if (e.Error == null)
                    {

                        ResolverRecord(_currentXmlObj);

                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);

            }
        }


        private static void ResolverRecord(string strXmlObj)
        {
            if (string.IsNullOrEmpty(strXmlObj))
            {
                return;
            }
            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(strXmlObj)))
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

                    //if (AssemblyName == "SMT.FB.UI")
                    //    defaultVersion = " , Version=1.1.1.1230, Culture=neutral, PublicKeyToken=null";

                    StringBuilder typeString = new StringBuilder();
                    typeString.Append(PublicClass);
                    typeString.Append(", ");
                    typeString.Append(AssemblyName);
                    typeString.Append(defaultVersion);

                    Type type = Type.GetType(typeString.ToString());
                    //Border borderPanel = new Border();
                    Type[] types = new Type[] { typeof(string), typeof(string), typeof(string) };
                    MethodInfo method = type.GetMethod(ProcessName, types);
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("开始调用业务系统打开单据方法,typeString:"
                    +typeString+"ProcessName:" + ProcessName
                        + "types:" + types);
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    method.Invoke(null, BindingFlags.Static | BindingFlags.InvokeMethod, null, new object[] { ApplicationOrder, PageParameter, FormType }, null);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("我的单据打开异常,请查看系统日志！"+ex.ToString());
                Logging.Logger.Current.Log("10000", "Platform", "我的单据", "我的单据打开异常", ex, Logging.Category.Exception, Logging.Priority.High);

            }
        }

        public int Top
        {
            get;
            set;
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

        }

        public void Stop()
        {
            if (_refdateTimer != null)
                _refdateTimer.Stop();
        }

        public void Star()
        {
            if (_refdateTimer != null)
                _refdateTimer.Start();
        }

        public void Initialize()
        {

        }

        public void Cleanup()
        {

        }




    }
}
