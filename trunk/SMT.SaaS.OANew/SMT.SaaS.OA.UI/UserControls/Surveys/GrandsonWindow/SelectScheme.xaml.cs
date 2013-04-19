//****满意度调查选择子页面***
//负责人:lezy
//创建时间:2011-6-7
//完成时间：2011-6-30
//**************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Validator;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class SelectScheme : BaseForm, IClient, IEntityEditor
    {

        #region 全局变量定义
        SmtOAPersonOfficeClient client=null;
        ObservableCollection<T_OA_SATISFACTIONMASTER> masterList=null;
        ObservableCollection<T_OA_SATISFACTIONREQUIRE> requireList=null;
        ObservableCollection<V_Satisfactions> dataViewList=null;
        ObservableCollection<V_EmployeeSurveyMaster> employeeMasterList=null;
        ObservableCollection<V_EmployeeSurveyApp> employeeAppList=null;
        V_EmployeeSurveyMaster _employeeMasterView=null;
        V_EmployeeSurveyApp _employeeAppView=null;
        V_Satisfactions _dataView=null;

        public V_Satisfactions DataView
        {
            get { return _dataView; } 
        }

        public V_EmployeeSurveyMaster EmployeeMasterView 
        {
            get { return _employeeMasterView; } 
        }

        public V_EmployeeSurveyApp EmployeeAppView 
        {
            get { return _employeeAppView; }
        }

        string searchType = string.Empty;
        #endregion

        #region 构造函数
        public SelectScheme(string searchType)
        {
            InitializeComponent();
            this.searchType = searchType;
            this.Loaded += new RoutedEventHandler(SelectScheme_Loaded);
          }

        #endregion

        #region 事件注册
        void EventResgister()
        {
            client = new SmtOAPersonOfficeClient();
            client.GetMasterByCheckstateAndDateCompleted += new EventHandler<GetMasterByCheckstateAndDateCompletedEventArgs>(client_GetMasterByCheckstateAndDateCompleted);
            client.GetRequireByCheckstateAndDateCompleted += new EventHandler<GetRequireByCheckstateAndDateCompletedEventArgs>(client_GetRequireByCheckstateAndDateCompleted);
            client.GetEmployeeSurveyByCheckstateAndDateCompleted += new EventHandler<GetEmployeeSurveyByCheckstateAndDateCompletedEventArgs>(client_GetEmployeeSurveyByCheckstateAndDateCompleted);
            client.GetEmployeeSurveyAppByCheckstateAndDateCompleted += new EventHandler<GetEmployeeSurveyAppByCheckstateAndDateCompletedEventArgs>(client_GetEmployeeSurveyAppByCheckstateAndDateCompleted);
        }

        
        #endregion

        #region 事件处理程序


        void SelectScheme_Loaded(object sender, RoutedEventArgs e)
        {
            EventResgister();
        }

        void client_GetRequireByCheckstateAndDateCompleted(object sender, GetRequireByCheckstateAndDateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error!=null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCONDITION"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            dataViewList = new ObservableCollection<V_Satisfactions>();
            dataViewList = e.Result as ObservableCollection<V_Satisfactions>;
            dgScheme.ItemsSource = null;
            dgScheme.ItemsSource = dataViewList;
        }

        void client_GetMasterByCheckstateAndDateCompleted(object sender, GetMasterByCheckstateAndDateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error!=null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
             }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCON"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            dataViewList = new ObservableCollection<V_Satisfactions>();
            dataViewList = e.Result as ObservableCollection<V_Satisfactions>;
            dgScheme.ItemsSource = null; 
            dgScheme.ItemsSource = dataViewList;
        }

        void client_GetEmployeeSurveyAppByCheckstateAndDateCompleted(object sender, GetEmployeeSurveyAppByCheckstateAndDateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCONDITION"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            employeeAppList = new ObservableCollection<V_EmployeeSurveyApp>();
            employeeAppList = e.Result as ObservableCollection<V_EmployeeSurveyApp>;
            dgScheme.ItemsSource = null;
            dgScheme.ItemsSource = employeeAppList;
           }

        void client_GetEmployeeSurveyByCheckstateAndDateCompleted(object sender, GetEmployeeSurveyByCheckstateAndDateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTFOUNDDATAOFMATCHCONDITION"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }
            employeeMasterList = new ObservableCollection<V_EmployeeSurveyMaster>();
            employeeMasterList = e.Result as ObservableCollection<V_EmployeeSurveyMaster>;
            dgScheme.ItemsSource = null;
            dgScheme.ItemsSource = employeeMasterList;
        }
        #endregion

        #region XAML事件处理程序
        void dataPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
         }

        private void dgScheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dgBySelectData=(DataGrid)sender;
            if (e.AddedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (searchType == "satisfactionMaster" || searchType == "satisfactionRequire")
            {
                _dataView = new V_Satisfactions();
                _dataView = dgBySelectData.SelectedItem as V_Satisfactions;
            }
            else if (searchType == "requireMaster")
            {
                _employeeMasterView = new V_EmployeeSurveyMaster();
                _employeeMasterView = dgBySelectData.SelectedItem as V_EmployeeSurveyMaster;
            }
            else
            {
                _employeeAppView = new V_EmployeeSurveyApp();
                _employeeAppView = dgBySelectData.SelectedItem as V_EmployeeSurveyApp;
            }
        }
        #endregion

        #region 其他函数
        void SearchSatisfactionMasterOrRequire()//查询方案或申请
        {
            if (CheckGroup())
            {
                GetData();
            }
        }

        void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        void GetData()//获取数据
        {
            int pageCount = 0;
            ObservableCollection<DateTime> dateTimes = new ObservableCollection<DateTime>() { dateStart.SelectedDate.Value, dateEnd.SelectedDate.Value };
            RefreshUI(RefreshedTypes.ShowProgressBar);
            switch (searchType)
            {
                case "satisfactionMaster":
                    client.GetMasterByCheckstateAndDateAsync(pageCount, dataPager.PageIndex, dataPager.PageSize, "2", dateTimes);
                    break;
                case "satisfactionRequire":
                    client.GetRequireByCheckstateAndDateAsync(pageCount, dataPager.PageIndex, dataPager.PageSize, "2", dateTimes);
                    break;
                case "requireMaster":
                    client.GetEmployeeSurveyByCheckstateAndDateAsync(pageCount, dataPager.PageIndex, dataPager.PageSize, "2", dateTimes);
                    break;
                case "require":
                    client.GetEmployeeSurveyAppByCheckstateAndDateAsync(pageCount, dataPager.PageIndex, dataPager.PageSize, "2", dateTimes);
                    break;
                default:
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    break;
            }
           
        }

        bool CheckGroup()//验证界面UI正确性
        {
            if (string.IsNullOrEmpty(dateStart.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "STARTTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return false;
            }
            if (string.IsNullOrEmpty(dateEnd.Text.ToString()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL","ENDTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return false;
            }
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            startDate = Convert.ToDateTime(dateStart.Text.ToString());
            endDate = Convert.ToDateTime(dateEnd.Text.ToString());
            if (startDate > endDate)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEGREATERTHANENDDATE", "STARTTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return false;
            }
            List<ValidatorBase> validators = checkGroup.ValidateAll();
            if (validators != null && validators.Count > 0)
            {
                foreach (ValidatorBase validator in validators)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(validator.ErrorMessage), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region  接口实现

        #region  IClient资源回收
        public void ClosedWCFClient()
        {
            client.DoClose();
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

        #region IEntityEditor窗体控制
        public string GetTitle()
        {
            switch (searchType)
            {
                case "satisfactionMaster":
                    return Utility.GetResourceStr("SELECTOASATISFACTION");
                case "satisfactionRequire":
                    return Utility.GetResourceStr("SELECTOASATISFACTIONAPP");
                case "requireMaster":
                    return Utility.GetResourceStr("SELECTOASURVEY");
                case "require":
                    return Utility.GetResourceStr("SELECTOASURVEYAPP");
                default:
                    return string.Empty;
            }
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
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
                case "1":
                    SearchSatisfactionMasterOrRequire();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> navigateItems = new List<NavigateItem>() { new NavigateItem { Title = Utility.GetResourceStr("InfoDetail"), Tooltip = Utility.GetResourceStr("InfoDetail") } };
            return navigateItems;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> tooltbalitems = new List<ToolbarItem>()
            {
                new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("SEARCH"),Key="1",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/(09,24).png"},
           new ToolbarItem
           {DisplayType=ToolbarItemDisplayTypes.Image,Title=Utility.GetResourceStr("CONFIRMBUTTON"),Key="0",ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"}
            };
            return tooltbalitems;
        }
        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

       
        #endregion
    }
}
