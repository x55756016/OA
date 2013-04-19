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
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class AreaForm : BaseForm, IEntityEditor,IClient
    {

        public T_HR_AREADIFFERENCE area { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        public AreaForm(FormTypes type, string areaID)
        {
            InitializeComponent();

            InitParas();

            FormType = type;
            if (string.IsNullOrEmpty(areaID))
            {
                area = new T_HR_AREADIFFERENCE();
                area.AREADIFFERENCEID = Guid.NewGuid().ToString();
                area.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                area.CREATEDATE = System.DateTime.Now;

                area.UPDATEDATE = System.DateTime.Now;
                area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = area;
            }
            else
            {
                client.GetAreaCategoryByIDAsync(areaID);
            }

        }

        private void InitParas()
        {
            client.GetAreaCategoryByIDCompleted += new EventHandler<GetAreaCategoryByIDCompletedEventArgs>(client_GetAreaCategoryByIDCompleted);
            client.AreaCategoryADDCompleted += new EventHandler<AreaCategoryADDCompletedEventArgs>(client_AreaCategoryADDCompleted);
            client.AreaCategoryUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaCategoryUpdateCompleted);

        }

        void client_AreaCategoryADDCompleted(object sender, AreaCategoryADDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == "SUCCESSED")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AREADIFFERENCECATEGORY"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }


            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_AreaCategoryUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREADIFFERENCECATEGORY"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }


        void client_GetAreaCategoryByIDCompleted(object sender, GetAreaCategoryByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                //if (e.Result == null)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                //    return;
                //}
                area = e.Result;
                this.DataContext = area;
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("AREACATEGORY");
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
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }

        }
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("BASEINFO"),
            //    Tooltip = Utility.GetResourceStr("BASEINFO")
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Tooltip = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Url = "/Salary/SalaryArchive"
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
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
             //   Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            else
            {


                if (FormType == FormTypes.Edit)
                {
                    area.UPDATEDATE = System.DateTime.Now;
                    area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.AreaCategoryUpdateAsync(area);
                }
                else
                {
                    client.AreaCategoryADDAsync(area);
                }

            }
            return true;
        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }


        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
