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
using SMT.Saas.Tools.EngineWS;
using SMT.Saas.Tools.SalaryWS;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class ResignForm : BaseForm, IClient, IEntityEditor
    {
        SalaryServiceClient client;
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        public event EventHandler SaveClicked;
        private int closeFlag = 0;//0 不关闭窗口 ，1关闭窗口
        public List<V_RESIGN> SelectedEmployees = new List<V_RESIGN>();//选中的人员


        public ResignForm()
        {
            InitializeComponent();
            InitPara();
        }



        public void InitPara()
        {
            try
            {
                client = new SalaryServiceClient();
                client.GetResignCompleted += new EventHandler<GetResignCompletedEventArgs>(client_GetResignCompleted);
                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            LoadData();
        }

        public void LoadData()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            int pageCount = 0;
            string filter = "";
            int sType = 0;
            string sValue = "";
            DateTime? starttimes;
            DateTime? endtimes = DateTime.Now.Date;
            starttimes = new DateTime(DateTime.Now.Year - 2, 1, 1);
            endtimes = new DateTime(DateTime.Now.Year, 12, 1);

            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            if (!string.IsNullOrEmpty(treeOrganization.sType))
            {
                string IsTag = treeOrganization.sType;
                sValue = treeOrganization.sValue;
                switch (IsTag)
                {
                    case "0":
                        sType = 0;
                        break;
                    case "1":
                        sType = 1;
                        break;
                    case "2":
                        sType = 2;
                        break;
                }
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                // filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                //filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEENAME)";
                filter += " EMPLOYEENAME.Contains(@" + paras.Count().ToString() + ")";
                paras.Add(txtEmpName.Text.Trim());
            }

            client.GetResignAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEEID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), userID, sType, sValue);

        }

        //保存
        private void Save()
        {
            if (SaveClicked != null)
            {
                SaveClicked(this, new EventArgs());
            }
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        /// <summary>
        /// 获取离职员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetResignCompleted(object sender, GetResignCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    DtGrid.ItemsSource = e.Result;
                }
                else
                {
                    DtGrid.ItemsSource = null;
                }
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }


        private void dataPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btSelected_Click(object sender, RoutedEventArgs e)
        {

            if (SaveClicked != null)
            {
                SaveClicked(this, new EventArgs());
            }
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //获取该行人员信息
            var ent = e.Row.DataContext as V_RESIGN;
            //获取该行的CheckBox
            CheckBox ckh = DtGrid.Columns[0].GetCellContent(e.Row).FindName("chkMyChkBox") as CheckBox;
            //是否已经是抽查组人员
            if (SelectedEmployees.Count > 0)
            {
                if (SelectedEmployees.Contains(ent))
                {
                    ckh.IsChecked = true;
                }

            }
            //增加CheckBox事件
            ckh.Click += new RoutedEventHandler(chkMyChkBox_Click);
        }

        private void chkMyChkBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            var ent = DtGrid.SelectedItem as V_RESIGN;
            //判断是增加还是删除
            if (chk.IsChecked.Value)
            {
                if (!SelectedEmployees.Contains(ent))
                {
                    SelectedEmployees.Add(ent);
                }

            }
            else
            {
                if (SelectedEmployees.Contains(ent))
                {
                    SelectedEmployees.Remove(ent);
                }
            }

        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("离职人员选择");
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
                    closeFlag = 0;
                    break;
                case "1":
                    closeFlag = 1;
                    break;
            }
            Save();
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
                Title = Utility.GetResourceStr("SAVE"),// "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
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

        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
