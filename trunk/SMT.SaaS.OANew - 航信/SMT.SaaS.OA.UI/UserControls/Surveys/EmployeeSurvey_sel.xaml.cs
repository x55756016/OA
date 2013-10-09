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
    public partial class EmployeeSurvey_sel : BaseForm, IClient, IEntityEditor
    {
        /// <summary>
        /// 存放选中的派车单
        /// </summary>
        public List<V_EmployeeSurvey> _lst;
        private SmtOAPersonOfficeClient _VM ;
        private string checkState = ((int)CheckStates.Approved).ToString();

        public EmployeeSurvey_sel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSurvey_sel_Loaded);
         }

        void EmployeeSurvey_sel_Loaded(object sender, RoutedEventArgs e)
        {
            _VM = new SmtOAPersonOfficeClient();
            _VM.Get_ESurveyCheckedCompleted += new EventHandler<Get_ESurveyCheckedCompletedEventArgs>(Get_ESurveyCheckedCompleted);

            DateStart.Text = DateTime.Now.AddDays(-7).ToShortDateString();
            DateEnd.Text = DateTime.Today.ToShortDateString();
            _lst = new List<V_EmployeeSurvey>();
            GetData();
        }
        //加载数据
        private void GetData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            string StrStart = "";
            string StrEnd = "";
            StrStart = DateStart.Text.ToString();
            StrEnd = DateEnd.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值     

            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                if (DtStart > DtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "RequireMaster.CREATEDATE >=@" + paras.Count().ToString();//开始时间
                    paras.Add(DtStart);
                    filter += " and ";
                    filter += "RequireMaster.CREATEDATE <=@" + paras.Count().ToString();//结束时间
                    paras.Add(DtEnd);
                }
            }

            paras.Add(DateStart.Text);
            paras.Add(DateEnd.Text);
            SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo loginInfo = new SMT.SaaS.OA.UI.SmtOACommonAdminService.LoginUserInfo();
            loginInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            _VM.Get_ESurveyCheckedAsync(dataPager.PageIndex, dataPager.PageSize, "RequireMaster.CREATEDATE", "", paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);

        }
        //获取数据 2
        void Get_ESurveyCheckedCompleted(object sender, Get_ESurveyCheckedCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            ObservableCollection<V_EmployeeSurvey> vehicleInfoData = e.Result;
            if (vehicleInfoData != null)
                dg.ItemsSource = vehicleInfoData.ToList();
            else
                dg.ItemsSource = null;
        }
        //分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        public string GetTitle()
        {
            return Utility.GetResourceStr("EmployeeSurvey_sel");
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
            GetData();
        }

        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lst.Clear();
            if (dg.SelectedIndex > -1)
            {
                V_EmployeeSurvey vd = dg.SelectedItem as V_EmployeeSurvey;
                _lst.Add(vd);
            }
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
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
