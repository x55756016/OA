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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Controls.Primitives;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryArchiveItemForm : BaseForm, IEntityEditor,IClient
    {
        private T_HR_SALARYARCHIVEITEM salaryArchiveItem { get; set; }
        private FormTypes FormType { get; set; }
        private V_SALARYARCHIVEITEM vsalaryArchiveItem { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        public SalaryArchiveItemForm(FormTypes formtype, string itemID)
        {
            InitializeComponent();
            InitParas();
            client.GetSalaryArchiveItemViewByIDAsync(itemID);
        }
        void InitParas()
        {
            client.GetSalaryArchiveItemViewByIDCompleted += new EventHandler<GetSalaryArchiveItemViewByIDCompletedEventArgs>(client_GetSalaryArchiveItemViewByIDCompleted);
            client.SalaryArchiveItemUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryArchiveItemUpdateCompleted);
        }
        #region 完成事件
        void client_SalaryArchiveItemUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYITEM"));
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        void client_GetSalaryArchiveItemViewByIDCompleted(object sender, GetSalaryArchiveItemViewByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    vsalaryArchiveItem = e.Result;
                    this.DataContext = vsalaryArchiveItem;
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                }
            }
        }
        #endregion
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYITEMNAME");
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
                    SaveAndClose();
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

        #region 保存

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (vsalaryArchiveItem == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEM"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYITEM"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            salaryArchiveItem = new T_HR_SALARYARCHIVEITEM();
            salaryArchiveItem.SALARYARCHIVEITEM = vsalaryArchiveItem.SALARYARCHIVEITEM;
            salaryArchiveItem.SALARYITEMID = vsalaryArchiveItem.SALARYITEMID;
            salaryArchiveItem.T_HR_SALARYARCHIVE = new T_HR_SALARYARCHIVE();
            salaryArchiveItem.T_HR_SALARYARCHIVE.SALARYARCHIVEID = vsalaryArchiveItem.SALARYARCHIVEID;
            salaryArchiveItem.SUM = vsalaryArchiveItem.SUM;
            salaryArchiveItem.REMARK = vsalaryArchiveItem.REMARK;
            if (salaryArchiveItem != null)
            {
                client.SalaryArchiveItemUpdateAsync(salaryArchiveItem);
            }

            return true;
        }
        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void SaveAndClose()
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
        #endregion

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

    }
}
