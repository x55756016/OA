using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    ///  1看调查结果明细
    /// </summary>
    public partial class EmployeeList : BaseForm,IClient, IEntityEditor
    {
        private T_OA_REQUIREDISTRIBUTE require;
        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_REQUIREDISTRIBUTE Require { get { return require; } set { require = value; } }
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();

        public EmployeeList()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeList_Loaded);
          
        }

        void EmployeeList_Loaded(object sender, RoutedEventArgs e)
        {
            //获取方案 题目，答案
            _VM.Get_ESurveyAsync(require.T_OA_REQUIRE.T_OA_REQUIREMASTER.REQUIREMASTERID);
            sopoClient.GetEmployeeListCompleted += new EventHandler<GetEmployeeListCompletedEventArgs>(sopoClient_GetEmployeeListCompleted);
            _VM.Get_ESurveyCompleted += new EventHandler<Get_ESurveyCompletedEventArgs>(Get_ESurveyCompleted);
        }
        
        void Get_ESurveyCompleted(object sender, Get_ESurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_EmployeeSurvey;

                sopoClient.GetEmployeeListAsync(employeeSurveyInfo.RequireMaster.REQUIREMASTERID);
            }
        }
        void sopoClient_GetEmployeeListCompleted(object sender, GetEmployeeListCompletedEventArgs e)
        {
            List<V_EmployeeID> empIdList = e.Result != null ? e.Result.ToList() : null;
            dgvEmployeeList.ItemsSource = empIdList;
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
        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }

        private V_EmployeeSurvey employeeSurveyInfo;
        public V_EmployeeSurvey EmployeeSurveyInfo
        {
            get
            {
                return employeeSurveyInfo;
            }
            set { employeeSurveyInfo = value; }
        }

        #region 继承
        public event UIRefreshedHandler OnUIRefreshed;

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("ShowDetail"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);
            return items;
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

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    string empId = GetSelectEmpID();
                    if (empId != null)
                    {
                        EmployeeSubmissions_3 frmEmpSurveysSubmit = new EmployeeSubmissions_3();//empSurveysInfo
                        frmEmpSurveysSubmit.UserID = empId;
                        frmEmpSurveysSubmit.EmployeeSurveyInfo = employeeSurveyInfo;
                        EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                        browser.MinHeight = 550;
                        browser.MinWidth = 750;
                        //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    }
                    break;
                case "1":
                    CancelAndClose();
                    break;
            }
        }

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("EmployeeSurveyDistribute");
        }
        #endregion
        private SmtOAPersonOfficeClient sopoClient = new SmtOAPersonOfficeClient();
        
        private void dgvEmployeeList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int index = e.Row.GetIndex();
            TextBlock cell = dgvEmployeeList.Columns[1].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
        }

        /// <summary>
        /// 获取一个
        /// </summary>
        /// <returns></returns>
        private string GetSelectEmpID()
        {
            if (dgvEmployeeList.ItemsSource != null)
            {
                foreach (object obj in dgvEmployeeList.ItemsSource)
                {
                    if (dgvEmployeeList.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = dgvEmployeeList.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox; //cb为
                        if (ckbSelect.IsChecked == true)
                        {
                            return ((V_EmployeeID)obj).EmployeeID;
                        }
                    }
                }
            }
            return null;
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