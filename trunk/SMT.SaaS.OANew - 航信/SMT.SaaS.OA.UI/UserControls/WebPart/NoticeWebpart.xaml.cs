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


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class NoticeWebpart : BaseForm, IClient, IWebPart
    {
        private SmtOACommonAdminClient client = new SmtOACommonAdminClient();
        public NoticeWebpart()
        {
            InitializeComponent();
            DaGr.HeadersVisibility = DataGridHeadersVisibility.None;
            client.GetHouseIssueAndNoticeInfosCompleted += new EventHandler<GetHouseIssueAndNoticeInfosCompletedEventArgs>(client_GetHouseIssueAndNoticeInfosCompleted);
            LoadData();
        }

        void LoadData()
        {
            //client.GetHouseIssueAndNoticeInfosAsync();
        }

        void client_GetHouseIssueAndNoticeInfosCompleted(object sender, GetHouseIssueAndNoticeInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    List<V_SystemNotice> aa = e.Result.ToList();
                    DaGr.ItemsSource = aa;
                }
            }
        }





        #region IWebPart 成员

        public event EventHandler OnMoreChanged;

        public void RefreshData()
        {
            //throw new NotImplementedException();
        }

        public int RowCount
        {
            get
            {
                return 1;
                //throw new NotImplementedException();
            }
            set
            {

                //throw new NotImplementedException();
            }
        }

        public void ShowMaxiWebPart()
        {
            //throw new NotImplementedException();
        }

        public void ShowMiniWebPart()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region loadingRow
        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_SystemNotice NoticeT = (V_SystemNotice)e.Row.DataContext;

            TextBlock mytbl = DaGr.Columns[2].GetCellContent(e.Row).FindName("formtype") as TextBlock;
            //Button titlebtn = DaGr.Columns[1].GetCellContent(e.Row).FindName("titlebtn") as Button;
            TextBlock titlebtn = DaGr.Columns[1].GetCellContent(e.Row).FindName("titlebtn") as TextBlock;
            titlebtn.Tag = NoticeT;
            //mytbl.Tag = NoticeT;

            if (NoticeT.Formtype == "Notice")//HouseIssue
            {
                mytbl.Text = Utility.GetResourceStr("MEETINGNOTICE");
            }
            if (NoticeT.Formtype == "HouseIssue")
            {
                mytbl.Text = Utility.GetResourceStr("HOUSEISSUE");
            }
            int index = e.Row.GetIndex();
            var cell = DaGr.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();　

        }
        #endregion

        #region 点击标题
        //private void titlebtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Button titleBtn = sender as Button;
        //    V_SystemNotice NoticeV = titleBtn.Tag as V_SystemNotice;
        //    switch (NoticeV.Formtype)
        //    {
        //        case "Notice"://会议通知
        //            MeetingNoticeWebPart form = new MeetingNoticeWebPart(NoticeV.FormId);
        //            EntityBrowser browser = new EntityBrowser(form);
        //            browser.MinHeight = 570;
        //            browser.MinWidth = 580;
        //            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        //            break;
        //        case "HouseIssue"://房源发布
        //            //MeetingNoticeWebPart Houseform = new MeetingNoticeWebPart(NoticeV.FormId);
        //            HouseIssueWebPart Houseform = new HouseIssueWebPart(NoticeV.FormId);
        //            EntityBrowser Housebrowser = new EntityBrowser(Houseform);
        //            Housebrowser.MinHeight = 570;
        //            Housebrowser.MinWidth = 580;
        //            Housebrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //            Housebrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                    
        //            break;
        //    }
            
        //}
        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        

        private void titlebtn_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock titleBtn = sender as TextBlock;
            V_SystemNotice NoticeV = titleBtn.Tag as V_SystemNotice;
            switch (NoticeV.Formtype)
            {
                case "Notice"://会议通知
                    MeetingNoticeWebPart form = new MeetingNoticeWebPart(NoticeV.FormId);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.MinHeight = 570;
                    browser.MinWidth = 580;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                    break;
                case "HouseIssue"://房源发布
                    //MeetingNoticeWebPart Houseform = new MeetingNoticeWebPart(NoticeV.FormId);
                    HouseIssueWebPart Houseform = new HouseIssueWebPart(NoticeV.FormId);
                    EntityBrowser Housebrowser = new EntityBrowser(Houseform);
                    Housebrowser.MinHeight = 570;
                    Housebrowser.MinWidth = 580;
                    Housebrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    Housebrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);

                    break;
                
            }

        }

        #endregion



        public int RefreshTime
        {
            get
            {
                return 1;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
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
