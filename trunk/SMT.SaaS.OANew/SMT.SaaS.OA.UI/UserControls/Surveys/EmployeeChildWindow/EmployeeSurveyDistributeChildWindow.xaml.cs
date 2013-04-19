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
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyDistributeChildWindow : BaseForm, IAudit, IClient, IEntityEditor
    {
        #region 全局变量定义

        FormTypes actionTypes = 0;
        string key = string.Empty;
        List<ExtOrgObj> lookupObjectList = null;//
        ObservableCollection<T_OA_DISTRIBUTEUSER> distributeuserList = null;//分发范围集合 
        SmtOAPersonOfficeClient client = null;
        V_EmployeeSurveyRequireDistribute distributeView = null;//发布申请自定义类
        PersonnelServiceClient personalClient = null;//用于调用HR服务,根据用户ID转换成姓名绑定到UI
        SelectScheme scheme = null;
        #endregion

        #region 构造函数

        public EmployeeSurveyDistributeChildWindow(FormTypes actionTypes, string key)
        {
            InitializeComponent();
            this.actionTypes = actionTypes;
            this.key = key;
            this.Loaded += new RoutedEventHandler(EmployeeSurveyDistributeChildWindow_Loaded);
        }

        #endregion

        #region 后台事件回调函数

        void EmployeeSurveyDistributeChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventResgister();
            if (actionTypes != FormTypes.New && !string.IsNullOrEmpty(key))
            {
                if (actionTypes == FormTypes.Browse && actionTypes == FormTypes.Audit)
                {
                    this.txtTitle.IsEnabled = false;
                    this.txtTitle.IsEnabled = false;
                    this.dgDistributeusers.IsEnabled = false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetDistributeDataAsync(key);
                if (actionTypes == FormTypes.Resubmit)
                {
                    distributeView.requiredistributeEntity.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString(); ;
                }
            }
            this.contextInfo.DataContext = distributeView.requiredistributeEntity;
        }

        void client_GetDistributeDataCompleted(object sender, GetDistributeDataCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage(true, "ERROR", "GETDATAFAILED", "");
                return;
            }
            if (e.Result == null)
            {
                ShowMessage(false, "NODATA", "DIDNOTFINDRELEVANT", "");
            }
            distributeView = e.Result;
            this.contextInfo.DataContext = distributeView.requiredistributeEntity;
            distributeuserList = distributeView.distributeuserList;
            distributeView.oldDistributeuserList = distributeuserList.Clone();
            ConvertByGetData(distributeuserList);
            BindingDataGrid();
            ShowAuditControl();
        }

        void client_UpdRequireDistributeCompleted(object sender, UpdRequireDistributeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage(true, "ERROR", "ERROR", "");
                return;
            }
            string _approving = ((int)CheckStates.Approving).ToString();
            string _approved = ((int)CheckStates.Approved).ToString();
            if (e.Result)
            {
                //lookupObjectList.Clear();
                distributeView.oldDistributeuserList = distributeView.distributeuserList.Clone();
                if (distributeView.requiredistributeEntity.CHECKSTATE == _approving)
                {
                    ShowMessage(false, "SUCCESSSUBMITAUDIT", "SUBMITSUCCESSED", "EmployeeSurveyDistribute");
                }
                else if (distributeView.requiredistributeEntity.CHECKSTATE == _approved)
                {
                    ShowMessage(false, "AUDITPASS", "AUDITSUCCESSED", "EmployeeSurveyDistribute");
                }
                else
                {
                    ShowMessage(false, "UPDATASUCCESSED", "UPDATESUCCESSED", "EmployeeSurveyDistribute");
                }
            }
            else
            {
                if (distributeView.requiredistributeEntity.CHECKSTATE == _approving)
                {
                    ShowMessage(true, "AUDITFAILURE", "SUBMITFAILED", "EmployeeSurveyDistribute");
                }
                else if (distributeView.requiredistributeEntity.CHECKSTATE == _approved)
                {
                    ShowMessage(true, "ERROR", "ERROR", "");
                }
                else
                {
                    ShowMessage(true, "UPDATEFAILED", "UPDATEISSUEFAILED", "EmployeeSurveyDistribute");
                }

            }
        }

        void client_AddRequireDistributeCompleted(object sender, AddRequireDistributeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage(true, "ERROR", "ERROR", "");
                return;
            }
            if (e.Result)
            {
                ShowMessage(false, "ADDDATASUCCESSED", "ADDSUCCESSED", "EmployeeSurveyDistribute");
                EntityBrowser entityBrowser = this.FindParentByType<EntityBrowser>();
                entityBrowser.FormType = actionTypes = FormTypes.Edit;
                distributeView.oldDistributeuserList = distributeView.distributeuserList.Clone();
                //lookupObjectList.Clear();
                distributeView.oldDistributeuserList = distributeView.distributeuserList.Clone();
                ShowAuditControl();
                return;
            }
            ShowMessage(true, "ADDFAILED", "ADDDATAFAILED", "EmployeeSurveyDistribute");
        }

        void personalClient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage(true, "ERROR", "ERROR", "");
                return;
            }
            if (e.Result == null)
            {
                ShowMessage(true, "ERROR", "GETDATAFAILED", "");
                return;
            }
            try
            {
                int _index = 0;
                foreach (T_OA_DISTRIBUTEUSER convert in distributeuserList)
                {
                    if (string.IsNullOrEmpty(convert.VIEWER))
                    {
                        convert.VIEWER = e.Result[_index].EMPLOYEECNAME;
                        _index++;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                ShowMessage(true, "ERROR", "DATAOUTOFRANGEEXCEPTION", "");
            }
        }

        #endregion

        #region 其他函数

        /// <summary>
        /// 后台事件注册及初始化
        /// </summary>
        private void EventResgister()
        {
            client = new SmtOAPersonOfficeClient();
            personalClient = new PersonnelServiceClient();
            client.AddRequireDistributeCompleted += new EventHandler<AddRequireDistributeCompletedEventArgs>(client_AddRequireDistributeCompleted);
            client.UpdRequireDistributeCompleted += new EventHandler<UpdRequireDistributeCompletedEventArgs>(client_UpdRequireDistributeCompleted);
            client.GetDistributeDataCompleted += new EventHandler<GetDistributeDataCompletedEventArgs>(client_GetDistributeDataCompleted);
            personalClient.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(personalClient_GetEmployeeByIDsCompleted);

            distributeView = new V_EmployeeSurveyRequireDistribute();
            distributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeView.distributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeView.requiredistributeEntity = new T_OA_REQUIREDISTRIBUTE();
            distributeView.oldDistributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            lookupObjectList = new List<ExtOrgObj>();
        }

        /// <summary>
        /// 弹出消息框
        /// </summary>
        /// <param name="isError">是否为错误消息</param>
        /// <param name="title">消息标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="other">消息详细</param>
        private void ShowMessage(bool isError, string title, string message, string other)
        {
            if (isError)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message, other
), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr(title), Utility.GetResourceStr(message, other), Utility.GetResourceStr("CONFIRM"));
            }
        }

        /// <summary>
        /// 加载审核控件
        /// </summary>
        private void ShowAuditControl()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 动画事件
        /// </summary>
        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        /// <summary>
        /// 新增填充发布申请表
        /// </summary>
        private void FillData(T_OA_REQUIREDISTRIBUTE data)
        {
            data.REQUIREDISTRIBUTEID = Guid.NewGuid().ToString();
            data.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            Utility.CreateUserInfo<T_OA_REQUIREDISTRIBUTE>(data);
        }

        /// <summary>
        /// 填充分发范围
        /// </summary>
        private void FillDistributeuserData(ObservableCollection<T_OA_DISTRIBUTEUSER> fillList)
        {
            string _company = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company.ToString();
            string _department = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department.ToString();
            string _post = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post.ToString();
            string _personnel = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel.ToString();
          
            foreach (T_OA_DISTRIBUTEUSER fill in fillList)
            {
                fill.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
                fill.FORMID = distributeView.requiredistributeEntity.REQUIREDISTRIBUTEID;
                fill.MODELNAME = "EmployeeSurvey";
                Utility.CreateUserInfo<T_OA_DISTRIBUTEUSER>(fill);
                fill.UPDATEDATE = DateTime.Now;
                fill.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                fill.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                if (fill.VIEWTYPE == _company)
                {
                    fill.VIEWTYPE = ((int)IssuanceObjectType.Company).ToString();
                    fill.VIEWER = FoundView(_company, fill.VIEWER);
                }
                else if (fill.VIEWTYPE == _department)
                {
                    fill.VIEWTYPE = ((int)IssuanceObjectType.Department).ToString();
                    fill.VIEWER = FoundView(_department, fill.VIEWER);
                }
                else if (fill.VIEWTYPE == _post)
                {
                    fill.VIEWTYPE = ((int)IssuanceObjectType.Post).ToString();
                    fill.VIEWER = FoundView(_post, fill.VIEWER);
                }
                else if (fill.VIEWTYPE == _personnel)
                {
                    fill.VIEWTYPE = ((int)IssuanceObjectType.Employee).ToString();
                    fill.VIEWER = FoundView(_personnel, fill.VIEWER);
                }
            }
        }

        /// <summary>
        /// 修改时,并未从新选择分发范围时,对选择的数据进行转换
        /// </summary>
        private void TransformDataForSave(T_OA_DISTRIBUTEUSER transform)
        {
            var ohterData = distributeView.oldDistributeuserList.Where(item => item.DISTRIBUTEUSERID == transform.DISTRIBUTEUSERID).Select(item => item);
            if (ohterData.Count() > 0)
            {
                T_OA_DISTRIBUTEUSER _distributeuser = ohterData.FirstOrDefault();
                transform.VIEWTYPE = _distributeuser.VIEWTYPE;
                transform.VIEWER = _distributeuser.VIEWER;
            }
        }

        /// <summary>
        /// 从lookup选择集合中转换数据
        /// </summary>
        private string FoundView(string viewType, string view)
        {

            var data = lookupObjectList.Where(item => (item.ObjectType.ToString() == viewType) && (item.ObjectName == view)).Select(item => item.ObjectID);
            return data != null ? data.FirstOrDefault() : null;

        }

        /// <summary>
        /// 保存或修改发布申请
        /// </summary>
        private void SaveDistribute()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (CheckGroup())
            {
                ObservableCollection<T_OA_DISTRIBUTEUSER> _transformList = SelectData();
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (actionTypes == FormTypes.New)
                {
                    FillData(distributeView.requiredistributeEntity);
                    FillDistributeuserData(_transformList);
                    distributeView.distributeuserList = _transformList;
                    client.AddRequireDistributeAsync(distributeView);
                    return;
                }
                distributeView.requiredistributeEntity.UPDATEDATE = System.DateTime.Now;
                distributeView.requiredistributeEntity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                distributeView.requiredistributeEntity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                if (lookupObjectList.Count() > 0)
                {
                    FillDistributeuserData(_transformList);
                    distributeView.distributeuserList = _transformList;
                }
                else
                {
                    ObservableCollection<T_OA_DISTRIBUTEUSER> _saveList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
                    foreach(T_OA_DISTRIBUTEUSER items in _transformList)
                    {
                        var IDList = from user in distributeuserList
                                 where items.VIEWER==user.VIEWER&&items.VIEWTYPE==user.VIEWTYPE
                                 select user.DISTRIBUTEUSERID;
                        if (IDList!=null)
                        {
                            string ID=IDList.FirstOrDefault();
                            var dataByID = from users in distributeView.oldDistributeuserList
                                           where users.DISTRIBUTEUSERID == ID
                                           select users;
                            if (dataByID != null)
                            {
                                _saveList.Add(dataByID.FirstOrDefault());
                            }
                        }
                         }
                    distributeView.distributeuserList = _saveList;
                }               
                client.UpdRequireDistributeAsync(distributeView);
            }
        }

        /// <summary>
        /// 获取选择的数据并进行填充
        /// </summary>
        private ObservableCollection<T_OA_DISTRIBUTEUSER> SelectData()
        {
            ObservableCollection<T_OA_DISTRIBUTEUSER> _hanerList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            ObservableCollection<T_OA_DISTRIBUTEUSER> _cloneList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            foreach (object hander in this.dgDistributeusers.SelectedItems)
            {
                if (hander as T_OA_DISTRIBUTEUSER == null)
                {
                    ShowMessage(true, "ERROR", "ERROR", "");
                    return null;
                }
                _hanerList.Add(hander as T_OA_DISTRIBUTEUSER);
            }
            _cloneList = _hanerList.Clone();//克隆一份数据传递,以免转换数据改变UI显示
            return _cloneList;
        }

        /// <summary>
        /// 验证界面UI数据正确性
        /// </summary>
        private bool CheckGroup()
        {
            string start = string.Empty;
            string end = string.Empty;
            if (string.IsNullOrEmpty(this.txtTitle.Text))
            {
                ShowMessage(true, "ERROR", "CANNOTBEEMPTYSELECT", "SurveysTITLE");
                return false;
            }
            if (string.IsNullOrEmpty(this.txtContent.Text))
            {
                ShowMessage(true, "ERROR", "CANNOTBEEMPTYSELECT", "SurveysContent");
                return false;
            }
            if (dgDistributeusers.SelectedIndex < 0)
            {
                ShowMessage(true, "ERROR", "STRINGNOTNULL", "DISTRBUTEOBJECT");
                return false;
            }
            List<ValidatorBase> validators = checkGroup.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (ValidatorBase validator in validators)
                {
                    ShowMessage(true, "ERROR", validator.ErrorMessage, "");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 选择发布对象
        /// </summary>
        private void SelectViewer()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    lookupObjectList = ent;
                    if (actionTypes != FormTypes.Browse && actionTypes != FormTypes.Audit)
                    {
                        distributeuserList.Clear();
                    }
                    foreach (var data in lookupObjectList)
                    {
                        distributeuserList.Add(new T_OA_DISTRIBUTEUSER { VIEWTYPE = data.ObjectType.ToString(), VIEWER = data.ObjectName });
                    }
                    BindingDataGrid();
                }
                else
                {
                    ShowMessage(true, "ERROR", "ERROR", "");
                    return;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        /// <summary>
        /// 绑定到DataGrid
        /// </summary>
        private void BindingDataGrid()
        {
            if (distributeuserList == null)
            {
                ShowMessage(true, "ERROR", "DISTRBUTEOBJECT", "CANNOTBEEMPTYSELECT");
                dgDistributeusers.ItemsSource = null;
                return;
            }
            dgDistributeusers.ItemsSource = null;
            dgDistributeusers.ItemsSource = distributeuserList;
            GridHelper.HandleAllCheckBoxClick(dgDistributeusers, "", true);
        }

        /// <summary>
        /// 弹出选择调查方案的页面
        /// </summary>
        private void SelectEmployeesApp()
        {
            scheme = new SelectScheme("require");
            EntityBrowser browser = new EntityBrowser(scheme);
            browser.MinHeight = 400;
            browser.MinWidth = 600;
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        /// <summary>
        /// 获取数据转换绑定
        /// </summary>
        private void ConvertByGetData(ObservableCollection<T_OA_DISTRIBUTEUSER> convertList)
        {
            string _company = ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company).ToString();
            string _department = ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString();
            string _post = ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString();
            string _personnel = ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString();
            ObservableCollection<string> idList = new ObservableCollection<string>();
            foreach (T_OA_DISTRIBUTEUSER convert in convertList)
            {
                if (convert.VIEWTYPE == _company)
                {
                    convert.VIEWTYPE = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company.ToString();
                    convert.VIEWER = Utility.GetCompanyName(convert.VIEWER);
                }
                else if (convert.VIEWTYPE == _department)
                {
                    convert.VIEWTYPE = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department.ToString();
                    convert.VIEWER = Utility.GetDepartmentName(convert.VIEWER);
                }
                else if (convert.VIEWTYPE == _post)
                {
                    convert.VIEWTYPE = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post.ToString();
                    convert.VIEWER = Utility.GetPostName(convert.VIEWER);
                }
                else if (convert.VIEWTYPE == _personnel)
                {
                    convert.VIEWTYPE =SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel.ToString();
                    idList.Add(convert.VIEWER);
                    convert.VIEWER = string.Empty;                  
                }
            }
            if (idList.Count > 0)
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                personalClient.GetEmployeeByIDsAsync(idList);
            }
        }

        /// <summary>
        /// 点击选择方案时,引发的事件回调函数，用来接收所选择的调查方案数据
        /// </summary>
        private void browser_ReloadDataEvent()
        {
            if (scheme.EmployeeAppView == null)
            {
                ShowMessage(true, "ERROR", "CANNOTBEGREATERTHANENDDATE", "STARTDATE");
                return;
            }
            distributeView.RequireId = scheme.EmployeeAppView.RequireId.ToString();
            distributeView.requiredistributeEntity.DISTRIBUTETITLE = scheme.EmployeeAppView.SurveyTitle;
            distributeView.requiredistributeEntity.CONTENT = scheme.EmployeeAppView.RequireContent;
        }
        #endregion

        #region IClient资源回收接口

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void ClosedWCFClient()
        {
            client.DoClose();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAudit审核接口

        public string GetAuditState()
        {
            string state = string.Empty;
            if (distributeView == null)
            {
                state = null;
            }
            else
            {
                state = distributeView.requiredistributeEntity.CHECKSTATE;
            }

            if (actionTypes == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = string.Empty;
            switch (args)
            {
                case AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            distributeView.requiredistributeEntity.CHECKSTATE = state;
            SaveDistribute();
        }

        public event UIRefreshedHandler OnUIRefreshed;

        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string xmlSourcce = Utility.ObjListToXml<T_OA_REQUIREDISTRIBUTE>(distributeView.requiredistributeEntity, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIREDISTRIBUTE", key, xmlSourcce);
        }
        #endregion

        #region IEntityEditor窗体控制

        public void DoAction(string actionType)
        {

            switch (actionType)
            {
                case "0":
                    SaveDistribute();
                    break;
                case "1":
                    SaveDistribute();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);//关闭页面
                    break;
                case "2":
                    SelectViewer();
                    break;
                case "3":
                    SelectEmployeesApp();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> navigateItems = new List<NavigateItem>() { new NavigateItem { Title = Utility.GetResourceStr("InfoDetail"), Tooltip = Utility.GetResourceStr("InfoDetail") } };
            return navigateItems;
        }

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            switch (actionTypes)
            {
                case FormTypes.New:
                    return Utility.GetResourceStr("ADDTITLE", "EmployeeSurveyDistribute");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDITTITLE", "EmployeeSurveyDistribute");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "EmployeeSurveyDistribute");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT1", "EmployeeSurveyDistribute");
                default:
                    return Utility.GetResourceStr("ReSubmitTitle", "EmployeeSurveyDistribute");
            }
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            if (actionTypes != FormTypes.Browse && actionTypes != FormTypes.Audit)
            {
                List<ToolbarItem> toolBaritems = new List<ToolbarItem>()
             {
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="3",Title=Utility.GetResourceStr("CHOOSEEMPLOYEESURVEYAPP"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="2",Title=Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
                 ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
             };
                return toolBaritems;
            }
            return new List<ToolbarItem>();
        }

        #endregion
      
    }
}
