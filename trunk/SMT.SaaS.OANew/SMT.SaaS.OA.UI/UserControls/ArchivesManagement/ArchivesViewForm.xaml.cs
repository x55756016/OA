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

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ArchivesViewForm : BaseForm,IClient,IEntityEditor
    {
        private SmtOADocumentAdminClient client;
        private string archiveID;

        public ArchivesViewForm(string archivesID)
        {
            InitializeComponent();
            archiveID = archivesID;
            InitEvent();
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetLendingListByLendingIdCompleted += new EventHandler<GetLendingListByLendingIdCompletedEventArgs>(client_GetLendingListByLendingIdCompleted);
            client.GetArchivesByIdCompleted += new EventHandler<GetArchivesByIdCompletedEventArgs>(client_GetArchivesByIdCompleted);
            client.GetLendingListByLendingIdAsync(archiveID);
        }

        private void client_GetLendingListByLendingIdCompleted(object sender, GetLendingListByLendingIdCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    client.GetArchivesByIdAsync(e.Result[0].T_OA_ARCHIVES.ARCHIVESID);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_GetArchivesByIdCompleted(object sender, GetArchivesByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    this.txtTitle.Text = e.Result.ARCHIVESTITLE;
                    txtContent.RichTextBoxContext = e.Result.CONTENT;
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {            
             return Utility.GetResourceStr("VIEWARCHIVE");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Close();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                //ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = RefreshedTypes.All;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region 保存
        private void Save()
        {
            //RefreshUI("1");
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void Close()
        {
            //saveType = "1";
            //Save();
            //RefreshUI("1");
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        #endregion

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
