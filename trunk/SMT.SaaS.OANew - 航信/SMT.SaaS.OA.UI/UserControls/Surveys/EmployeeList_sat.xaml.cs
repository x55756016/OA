using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    ///  1看调查结果明细
    /// </summary>
    public partial class EmployeeList_sat : BaseForm,IClient, IEntityEditor
    {
        /// <summary>
        /// 存放选中信息
        /// </summary>
        V_EmployeeID _selectedInfo = null;
        /// <summary>
        /// 模板答案
        /// </summary>
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;
        private T_OA_SATISFACTIONREQUIRE require;
        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_SATISFACTIONREQUIRE Require { get { return require; } set { require = value; } }
        SmtOAPersonOfficeClient _VM;
        /// <summary>
        /// 取字典 答案
        /// </summary>
        private PermissionServiceClient permissionClient;

        public EmployeeList_sat()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeList_sat_Loaded);
           
        }

        void EmployeeList_sat_Loaded(object sender, RoutedEventArgs e)
        {
            _VM = new SmtOAPersonOfficeClient();
            permissionClient = new PermissionServiceClient();
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(GetSysDictionaryByFatherIDCompleted);
            _VM.Result_EmployeeByRequireIDCompleted += new EventHandler<Result_EmployeeByRequireIDCompletedEventArgs>(Result_EmployeeByRequireIDCompleted);
            _VM.Get_SSurveyCompleted += new EventHandler<Get_SSurveyCompletedEventArgs>(Get_SSurveyCompleted);
            //模板答案
            permissionClient.GetSysDictionaryByFatherIDAsync(require.ANSWERGROUPID);
            //获取方案 题目，答案
            _VM.Get_SSurveyAsync(require.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID);
        }
       
        //模板答案
        void GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            if (e.Result != null)
                _oanswer = e.Result;
        }
        void Get_SSurveyCompleted(object sender, Get_SSurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_Satisfaction;

                _VM.Result_EmployeeByRequireIDAsync(require.SATISFACTIONREQUIREID);
            }
        }
        void Result_EmployeeByRequireIDCompleted(object sender, Result_EmployeeByRequireIDCompletedEventArgs e)
        {
            List<V_EmployeeID> empIdList = e.Result != null ? e.Result.ToList() : null;
            dg.ItemsSource = empIdList;
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

        private V_Satisfaction employeeSurveyInfo;
        public V_Satisfaction EmployeeSurveyInfo
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
                    if (_selectedInfo != null && _oanswer != null)
                    {
                        Satisfaction_3 frmEmpSurveysSubmit = new Satisfaction_3(_oanswer);//empSurveysInfo
                        frmEmpSurveysSubmit.UserID = _selectedInfo.EmployeeID;
                        frmEmpSurveysSubmit.EmployeeSurveyInfo = employeeSurveyInfo;
                        EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                        browser.MinHeight = 510;
                        browser.MinWidth = 750;
                        browser.FormType = FormTypes.Browse;
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
            return Utility.GetResourceStr("EMPLOYEEINFO");
        }
        #endregion
        
        private void dgvEmployeeList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            int index = e.Row.GetIndex();
            TextBlock cell = dg.Columns[1].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();
        }
        private void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (dg.SelectedIndex > -1)
            {
               _selectedInfo = dg.SelectedItem as V_EmployeeID;
            }
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
            permissionClient.DoClose();
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