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

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractTemplateDetailInfos : BaseForm,IClient, IEntityEditor
    {
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtText.Height = ((Grid)sender).ActualHeight * 0.3;
        }
        private T_OA_CONTRACTTEMPLATE contraTemplate;

        public ContractTemplateDetailInfos(T_OA_CONTRACTTEMPLATE obj)
        {
            contraTemplate = obj;
            InitializeComponent();
            ShowTemplateDetailInfos();
        }
        void ShowTemplateDetailInfos()
        {
            this.TemplateName.Text = contraTemplate.CONTRACTTEMPLATENAME;
            this.TemplateTitle.Text = contraTemplate.CONTRACTTITLE;
            this.TemplateFounder.Text = contraTemplate.CREATEUSERNAME;
            this.TemplateChangesWere.Text = contraTemplate.UPDATEUSERNAME;
            //this.txtText.Text = contraTemplate.CONTENT;
            if (contraTemplate.UPDATEDATE != null)
            {
                this.TemplateModified.Text = contraTemplate.UPDATEDATE.ToString();
            }
            else
            {
                this.TemplateModified.Text = "";
            }
            TemplateTime.Text = contraTemplate.CREATEDATE.ToLongDateString() + contraTemplate.CREATEDATE.ToLongTimeString();
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "模板详细信息";
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
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

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "关闭",
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
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        public void Close()
        {
            RefreshUI(RefreshedTypes.Close);
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new NotImplementedException();
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
