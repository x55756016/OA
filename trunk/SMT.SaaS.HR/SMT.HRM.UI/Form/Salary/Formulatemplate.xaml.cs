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
using System.Xml.Linq;

using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class Formulatemplate : BaseForm, IEntityEditor
    {
        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        List<T_HR_SALARYITEM> SalaryItemList = new List<T_HR_SALARYITEM>();
        T_HR_SALARYITEM salaryItem = new T_HR_SALARYITEM();
        public T_HR_SALARYITEM SalaryItem
        {
            get
            {
                return salaryItem;
            }
            set
            {
                salaryItem = value;
            }
        }
        public Formulatemplate()
        {
            InitializeComponent();
            Init();
            LoadData();
        }

        public void LoadData()
        {
            if (SalaryItemList.Count > 0)
                DtGrid.ItemsSource = SalaryItemList;
            loadbar.Stop();
        }

        public void Init()
        {
            PARENT.Children.Add(loadbar);
            client.FormulaTemplateAddCompleted += new EventHandler<FormulaTemplateAddCompletedEventArgs>(client_FormulaTemplateAddCompleted);
            List<T_HR_SALARYITEM> salaryItemList = SalaryItemXmlOperator.GetSalaryItemXML();
            foreach (T_HR_SALARYITEM siList in salaryItemList)
            {
                T_HR_SALARYITEM list = siList;
                list.SALARYITEMID = Guid.NewGuid().ToString();
                list.CREATEDATE = list.UPDATEDATE = System.DateTime.Now;

                list.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                list.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                list.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                list.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                list.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                list.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                list.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                list.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                SalaryItemList.Add(list);
            }
        }

        void client_FormulaTemplateAddCompleted(object sender, FormulaTemplateAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result)
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("GENERATE", "SALARYITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("GENERATE", "SALARYITEM"));
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GENERATEFAIL", "SALARYITEM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GENERATEFAIL", "SALARYITEM"));
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        public void Save()
        {
            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (objects, result) =>
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                System.Collections.ObjectModel.ObservableCollection<T_HR_SALARYITEM> lists = new System.Collections.ObjectModel.ObservableCollection<T_HR_SALARYITEM>();
                foreach (var ent in SalaryItemList)
                {
                    lists.Add(ent);
                }
                client.FormulaTemplateAddAsync(lists);
            };
            com.SelectionBox(Utility.GetResourceStr("SALARYITEM"), Utility.GetResourceStr("SALARYITEMDESC"), ComfirmWindow.confirmation, Result);
        }

        public void Cancel()
        {

        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("RECOVERYSALARY");
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
                Title = Utility.GetResourceStr("CREATE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
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

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }
    }
}
