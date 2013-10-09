using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 获取派车单，合并派车
    /// </summary>
    public partial class Satisfaction_sel : BaseForm, IClient, IEntityEditor
    {
        /// <summary>
        /// 存放选中的派车单
        /// </summary>
        public List<V_Satisfaction> _lst;
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

        public Satisfaction_sel()
        {
            InitializeComponent();
            _VM.Get_SSurveyCheckedCompleted += new EventHandler<Get_SSurveyCheckedCompletedEventArgs>(Get_SSurveyCheckedCompleted);

            DateStart.Text = DateTime.Now.AddDays(-7).ToShortDateString();
            DateEnd.Text = DateTime.Today.ToShortDateString();
        }
        
        //窗体加载事件
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            _lst = new List<V_Satisfaction>();
            GetData(dataPager.PageIndex, "2");
        }
        //加载数据
        private void GetData(int pageIndex, string checkState)
        {         
                int pageCount = 0;
                ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值     

                paras.Add(DateStart.Text);
                paras.Add(DateEnd.Text);
                SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
                loginInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                loginInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
                _VM.Get_SSurveyCheckedAsync(pageIndex, dataPager.PageSize, "RequireMaster.CREATEDATE", "", paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, "2");
           
        }
        //获取数据 2
        void Get_SSurveyCheckedCompleted(object sender, Get_SSurveyCheckedCompletedEventArgs e)
        {
            ObservableCollection<V_Satisfaction> vehicleInfoData = e.Result;          
            if (vehicleInfoData != null)
                dg.ItemsSource = vehicleInfoData.ToList();
            else
                dg.ItemsSource = null;
        }       
        //分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData(dataPager.PageIndex, "2");
        }       
        public string GetTitle()
        {
            return Utility.GetResourceStr("OASatisfaction");
        }

        public string GetStatus()
        {
            return "";
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            object[,] arr = new object[,]{
             {ToolbarItemDisplayTypes.Image,"1","SEARCH","/SMT.SaaS.FrameworkUI;Component/Images/(09,24).png"},
             {ToolbarItemDisplayTypes.Image,"0","CONFIRMBUTTON","/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"},
            };
            return VehicleMgt.GetToolBarItems(ref arr);
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "1":
                    search();
                    
                    break;
            }
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
        /// <summary>
        /// 查询
        /// </summary>
        public void search()
        {
            dataPager.PageIndex = 1;
            GetData(dataPager.PageIndex, "2");
        }

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lst.Clear();
            if (dg.SelectedIndex > -1)
            {
                V_Satisfaction vd = dg.SelectedItem as V_Satisfaction;             
                _lst.Add(vd);
            }
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
