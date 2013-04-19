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

namespace SMT.HRM.UI.Form.Salary
{
    public partial class PaymentForm : BaseForm, IEntityEditor
    {
        SalaryServiceClient client;
        public PaymentForm()
        {
            InitializeComponent();
            new BasePage().GetEntityLogo("T_HR_EMPLOYEESALARYRECORD");
            //InitParas();
        }

        public PaymentForm(FormTypes type, string customGuerdonSetID)
        {
            InitializeComponent();
        }

        //public void FlashData(string employeeid, string year, string month)
        //{
        //    DGSalary.Visibility = Visibility.Visible;
        //    client.GetSalaryRecordOneAsync(employeeid, year, month);
        //} 

        //private void InitParas()
        //{
        //    client = new SalaryServiceClient();
        //    client.GetSalaryRecordOneCompleted += new EventHandler<GetSalaryRecordOneCompletedEventArgs>(client_GetSalaryRecordOneCompleted);
        //}

        //void client_GetSalaryRecordOneCompleted(object sender, GetSalaryRecordOneCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            List<T_HR_EMPLOYEESALARYRECORD> salaryrecordlast = new List<T_HR_EMPLOYEESALARYRECORD>();
        //            txtbtitle.Text = Utility.GetResourceStr("SALARYLAST") + ":";
        //            salaryrecordlast.Add(e.Result);
        //            DGSalary.ItemsSource = salaryrecordlast;
        //        }
        //        else 
        //        {
        //            DGSalary.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //}

        //private void DGSalary_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void GridPager_Click(object sender, RoutedEventArgs e)
        //{

        //}

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("RS");
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
            //item = new NavigateItem
            //{
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    Url = "/Salary/SalaryStandard.xaml"
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
                Title = "保存",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
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

        void Save()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            if (!flag)
            {
                return;
            }
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        private void tbcContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
