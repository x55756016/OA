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
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.Platform;
using System.Windows.Threading;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class OAWebPart : BaseForm,IClient, SMT.SaaS.Platform.IWebPart
    {
        private SmtOACommonAdminClient client = new SmtOACommonAdminClient();
        //private DispatcherTimer _refdateTimer;
        private MouseClickManager mouseClickManager;

        public OAWebPart()
        {
            InitializeComponent();
            
            //_refdateTimer = new DispatcherTimer();
            //_refdateTimer.Interval = new TimeSpan(0, 0, 10);
            //_refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            client.GetHouseIssueAndNoticeInfosCompleted += new EventHandler<GetHouseIssueAndNoticeInfosCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosCompleted);
            client.GetHouseIssueAndNoticeInfosToMobileCompleted += new EventHandler<GetHouseIssueAndNoticeInfosToMobileCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosToMobileCompleted);
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
            
            client.GetHouseIssueAndNoticeInfosAsync(Common.CurrentLoginUserInfo.EmployeeID,posts, companys, departs);
            
            //////client.GetHouseIssueAndNoticeInfosToMobileAsync(1,20,6,100,"ddddddddddddddd",SAAS1,SAAS2,SAAS3);
            //V_SystemNotice aa = new V_SystemNotice();
            //V_SystemNotice bb = new V_SystemNotice();
            //client.GetSysNoticeByFormidToMobileAsync("a7b7582c-5cb9-468e-b396-23fd85f1f0f3", SAAS1, SAAS2, SAAS3, "3303d827-e2df-48d6-8c21-8224a589c551", aa, bb);
        }

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
            try
            {
                V_SystemNotice source = (sender as HyperlinkButton).DataContext as V_SystemNotice;
                //NewsShow newsview = new NewsShow() { ViewModel = source };
                
                switch (source.Formtype)
                {
                    case "会议通知"://会议通知
                        MeetingNoticeWebPart form = new MeetingNoticeWebPart(source.FormId);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 570;
                        browser.MinWidth = 580;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                        break;
                    case "房源发布"://房源发布
                        //MeetingNoticeWebPart Houseform = new MeetingNoticeWebPart(NoticeV.FormId);
                        HouseIssueWebPart Houseform = new HouseIssueWebPart(source.FormId);
                        EntityBrowser Housebrowser = new EntityBrowser(Houseform);
                        Housebrowser.MinHeight = 570;
                        Housebrowser.MinWidth = 580;
                        Housebrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        Housebrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

                        break;
                    //case "CompanyDoc"://公司公文
                    //    CompanyDocWebPart SendDocform = new CompanyDocWebPart(source.FormId);
                    //    System.Windows.Controls.Window.Show("公司发文", "", source.FormId, true, true, SendDocform, null);
                    //    //EntityBrowser SendDocbrowser = new EntityBrowser(SendDocform);
                    //    //SendDocbrowser.MinHeight = 850;
                    //    //SendDocbrowser.MinWidth = 650;
                    //    //SendDocbrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    //    //SendDocbrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

                    //break;
                }
                if (source.Formtype != "会议通知" && source.Formtype != "房源发布")
                {
                    CompanyDocWebPart SendDocform = new CompanyDocWebPart(source.FormId);
                    System.Windows.Controls.Window wd = new System.Windows.Controls.Window();
                    wd.MinWidth = 980;
                    wd.MinHeight = 460;
                    wd.Content = SendDocform;
                    wd.TitleContent = "公司发文";
                    
                    wd.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true, false, source.FormId);

                }
                //switch (source.NEWSTYPEID)
                //{
                //    case "0": titel = "新闻"; break;
                //    case "1": titel = "动态"; break;
                //    case "2": titel = "公告"; break;
                //    case "3": titel = "通知"; break;
                //    default:
                //        break;
                //}
                //System.Windows.Controls.Window.Show(titel, "", source.NEWSID, true, true, newsview, null);

            }
            catch (Exception ex)
            {
                string aa = "";
            }
        }

        public static void ShowCompanyDoc(string FormId)
        {
            CompanyDocWebPart SendDocform = new CompanyDocWebPart(FormId);
            System.Windows.Controls.Window wd = new System.Windows.Controls.Window();
            wd.MinWidth = 980;
            wd.MinHeight = 460;
            wd.Content = SendDocform;
            wd.TitleContent = "公司发文";

            wd.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true, false, FormId);

 
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
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
    }
}
