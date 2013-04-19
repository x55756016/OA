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
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.Platform;
using System.Windows.Threading;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SaaS.OA.UI.UserControls.WebPart
{
    public partial class OAStaffSurveyWebPart : BaseForm, IClient, SMT.SaaS.Platform.IWebPart
    {
        private SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();

        private MouseClickManager mouseClickManager;

        public OAStaffSurveyWebPart()
        {
            InitializeComponent();
            client.GetStaffSurveyInfosCompleted += new EventHandler<GetStaffSurveyInfosCompletedEventArgs>(client_GetStaffSurveyInfosCompleted);
            LoadData();
        }

        void client_GetStaffSurveyInfosCompleted(object sender, GetStaffSurveyInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    NewsList.ItemsSource = null;
                    NewsList.ItemsSource = e.Result.ToList();
                }
            }
        }
        void NewsWebPart_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void _refdateTimer_Tick(object sender, EventArgs e)
        {
            LoadData();
        }

        void LoadData()
        {
            client.GetStaffSurveyInfosAsync(Common.CurrentLoginUserInfo.EmployeeID, Common.CurrentLoginUserInfo.UserPosts[0].PostID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
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

                switch (source.Formtype)
                {
                    case "StaffSurvey"://员工调查
                        StaffSurveyWebPart form = new StaffSurveyWebPart(source.FormId);
                        EntityBrowser browser = new EntityBrowser(form);
                        browser.MinHeight = 550;
                        browser.MinWidth = 750;
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                        break;
                    case "SatisfactionSurvey"://满意度调查
                        SatisfactionSurveyWebPart Houseform = new SatisfactionSurveyWebPart(source.FormId);
                        EntityBrowser Housebrowser = new EntityBrowser(Houseform);
                        Housebrowser.MinHeight = 650;
                        Housebrowser.MinWidth = 800;
                        Housebrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        Housebrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            throw new NotImplementedException();
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
