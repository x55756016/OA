using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Form.EmployeeSurveys;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Collections.ObjectModel;
using System.Linq;
using FrUi = SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.Validator;
using System.Windows.Media;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeesSurveyAppChildWindow : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        FormTypes actionTypes = 0;//页面类型:新增、修改等
        string key = string.Empty;//员工调查申请ID，用于在修改时从数据获取数据
        SmtOAPersonOfficeClient client = null;
        PersonnelServiceClient personalClient = null;//用于调用HR服务,根据用户ID转换成姓名绑定到UI
        List<ExtOrgObj> lookupObjectList = null;
        ObservableCollection<T_OA_DISTRIBUTEUSER> distributeuserList = null;
        SelectScheme scheme = null;//选择调查方案页面
        V_EmployeeSurveyApp appView = null;//调查申请与分发范围的自定义类
        #endregion

        #region 构造函数
        public EmployeesSurveyAppChildWindow(FormTypes actionTypes, string key)
        {
            InitializeComponent();
            this.actionTypes = actionTypes;
            this.key = key;
            this.Loaded += new RoutedEventHandler(EmployeesSurveyAppChildWindow_Loaded);
        }
        #endregion

        #region 事件回调函数
        void EmployeesSurveyAppChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventRegister();
            if (actionTypes != FormTypes.New && !string.IsNullOrEmpty(key))
            {
                if (actionTypes == FormTypes.Browse && actionTypes == FormTypes.Audit)
                {
                    this.txtTitle.IsEnabled = false;
                    this.txtTitle.IsEnabled = false;
                    this.StartDate.IsEnabled = false;
                    this.EndDate.IsEnabled = false;
                    this.dgDistributeusers.IsEnabled = false;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetRequireDataAsync(key);
                if (actionTypes == FormTypes.Resubmit)
                {
                    appView.requireEntity.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString(); ;
                }
            }
            this.contextInfo.DataContext = appView.requireEntity;
        }

        /// <summary>
        /// 当当前分发对象是个人时,根据员工ID集合查询员工姓名集合
        /// </summary>
        void personalClient_GetEmployeeByIDsCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeByIDsCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage("error", "ERROR", "GETDATAFAILED", "", false);
                return;
            }
            try
            {
                int _index = 0;
                foreach (T_OA_DISTRIBUTEUSER convert in distributeuserList)
                {
                    if (convert.VIEWER == null)
                    {
                        convert.VIEWER = e.Result[_index].EMPLOYEECNAME;
                        _index++;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                ShowMessage("error", "ERROR", "DATAOUTOFRANGEEXCEPTION", "", false);
            }
        }

        void client_GetRequireDataCompleted(object sender, GetRequireDataCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage("error", "ERROR", "GETDATAFAILED", "", false);
                return;
            }
            if (e.Result == null)
            {
                ShowMessage("", "NODATA", "DIDNOTFINDRELEVANT", "", true);
                return;
            }
            appView = e.Result;
            this.contextInfo.DataContext = appView.requireEntity;
            this.IsFill.IsChecked = appView.requireEntity.WAY == "1" ? true : false;
            this.IsImplement.IsChecked = appView.requireEntity.OPTFLAG == "1" ? true : false;
            appView.oldDistributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            appView.oldDistributeuserList = appView.distributeuserList.Clone();//先克隆,以备删除
            distributeuserList = appView.distributeuserList;
            ConvertByGetData(distributeuserList);
            BindingDataGrid();
            ShowAuditControl();
        }

        void client_UpdRequireCompleted(object sender, UpdRequireCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage("error", "ERROR", "ERROR", "", false);
                return;
            }
            string _approving = ((int)CheckStates.Approving).ToString();
            string _approved = ((int)CheckStates.Approved).ToString();
            if (!e.Result)
            {
                if (appView.requireEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("error", "AUDITFAILURE", "SUBMITFAILED", "", true);
                }
                else if (appView.requireEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("error", "ERROR", "ERROR", "", false);
                }
                else
                {
                    ShowMessage("error", "UPDATEFAILED", "UPDATEISSUEFAILED", "", true);
                }
                return;
            }
            if (e.Result)
            {
                appView.oldDistributeuserList = appView.distributeuserList.Clone();
                if (appView.requireEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("", "SUCCESSSUBMITAUDIT", "SUBMITSUCCESSED", "", true);
                }
                else if (appView.requireEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("", "AUDITPASS", "AUDITSUCCESSED", "", true);
                }
                else
                {
                    ShowMessage("", "UPDATASUCCESSED", "UPDATESUCCESSED", "", true);
                }
            }
        }

        void client_AddRequireCompleted(object sender, AddRequireCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);

            if (e.Result)
            {
                EntityBrowser entityBrowser = this.FindParentByType<EntityBrowser>();
                entityBrowser.FormType = actionTypes = FormTypes.Edit;
                appView.oldDistributeuserList = appView.distributeuserList.Clone();
                ShowAuditControl();
                ShowMessage("", "ADDDATASUCCESSED", "ADDSUCCESSED", "", true);
                return;
            }
            ShowMessage("error", "ADDFAILED", "ADDDATAFAILED", "", true);
        }
        #endregion

        #region 其他函数

        /// <summary>
        /// 显示提示信息窗口
        /// </summary>
        /// <param name="action">弹出错误或提示窗口</param>
        /// <param name="title">窗口标题信息</param>
        /// <param name="message">窗口提示信息</param>
        /// <param name="isshow">错误提示信息中是否有"调查方案"</param>
        /// /// <param name="other">传入提示信息的其他信息</param>
        private void ShowMessage(string action, string title, string message, string other, bool isShow)
        {
            if (action == "error" && isShow)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message, "EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else if (action == "error")
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else if (action == "check")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message, other), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr(title), Utility.GetResourceStr(message, "EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"));
            }
        }

        /// <summary>
        /// 显示审核控件
        /// </summary>
        private void ShowAuditControl()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// WCF事件注册和全局变量初始化
        /// </summary>
        private void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            personalClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            client.AddRequireCompleted += new EventHandler<AddRequireCompletedEventArgs>(client_AddRequireCompleted);
            client.UpdRequireCompleted += new EventHandler<UpdRequireCompletedEventArgs>(client_UpdRequireCompleted);
            client.GetRequireDataCompleted += new EventHandler<GetRequireDataCompletedEventArgs>(client_GetRequireDataCompleted);
            personalClient.GetEmployeeByIDsCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeByIDsCompletedEventArgs>(personalClient_GetEmployeeByIDsCompleted);
            client.CheckRequireCompleted += new EventHandler<CheckRequireCompletedEventArgs>(client_CheckRequireCompleted);
            appView = new V_EmployeeSurveyApp();
            appView.requireEntity = new T_OA_REQUIRE();
            appView.distributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeuserList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            lookupObjectList = new List<ExtOrgObj>();

            personalClient.GetEmployeeDetailByParasPagingCompleted += new EventHandler<GetEmployeeDetailByParasPagingCompletedEventArgs>(personalClient_GetEmployeeDetailByParasPagingCompleted);
        }

        void personalClient_GetEmployeeDetailByParasPagingCompleted(object sender, GetEmployeeDetailByParasPagingCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                foreach (var item in e.Result)
                {
                    T_OA_REQUIREDISTRIBUTE ute = new T_OA_REQUIREDISTRIBUTE();

                    ute.REQUIREDISTRIBUTEID = Guid.NewGuid().ToString();
                    ute.T_OA_REQUIRE = new T_OA_REQUIRE();
                    ute.T_OA_REQUIRE = appView.requireEntity;
                    ute.DISTRIBUTETITLE = appView.requireEntity.APPTITLE;
                    ute.CONTENT = appView.requireEntity.CONTENT;
                    ute.CHECKSTATE = "0";
                    ute.OWNERID = item.T_HR_EMPLOYEE.EMPLOYEEID;
                    ute.OWNERNAME = item.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    ute.OWNERCOMPANYID = item.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                    ute.OWNERDEPARTMENTID = item.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    ute.OWNERPOSTID = item.T_HR_POST.POSTID;
                    ute.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ute.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    ute.CREATECOMPANYID = item.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                    ute.CREATEDEPARTMENTID = item.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    ute.CREATEPOSTID = item.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    ute.CREATEDATE = DateTime.Now;
                    ute.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ute.UPDATEDATE = DateTime.Now;

                    appView.requireEntity.T_OA_REQUIREDISTRIBUTE.Add(ute);
                   
                    //notifyfeedback.REGISTERID = System.Guid.NewGuid().ToString();
                    //notifyfeedback.T_TM_PROJECTPLAN = new T_TM_PROJECTPLAN();
                    //notifyfeedback.T_TM_PROJECTPLAN = projectplan;
                    //notifyfeedback.ATTEND = "0";// 是否参加
                    //notifyfeedback.OWNERID = item.T_HR_EMPLOYEE.EMPLOYEEID; ; //报名人ID 
                    //notifyfeedback.OWNERNAME = item.T_HR_EMPLOYEE.EMPLOYEECNAME;//报名人名称
                    //notifyfeedback.OWNERCOMPANYID = item.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;//报名人公司
                    //notifyfeedback.OWNERCOMPANYNAME = item.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    //notifyfeedback.OWNERDEPARTMENTID = item.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;//报名人部门
                    //notifyfeedback.OWNERDEPARTMENTNAME = item.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //notifyfeedback.OWNERPOSTID = item.T_HR_POST.POSTID;//岗位
                    //notifyfeedback.OWNERPOSTNAME = item.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    //notifyfeedback.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    //notifyfeedback.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    //notifyfeedback.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    //notifyfeedback.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    //notifyfeedback.CHECKSTATES = "0";//审核状态
                    //if (isMustjoins[e.UserState.ToInt32()] == "1")
                    //    notifyfeedback.ISMUSTJOIN = "1";
                    //else
                    //    notifyfeedback.ISMUSTJOIN = "0";

                    //listNotifyfeedback.Add(notifyfeedback);
                }
            }
        }

        void client_CheckRequireCompleted(object sender, CheckRequireCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result)
                        switch (appView.requireEntity.CHECKSTATE)
                        {
                           
                            case "1":
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "成功", "提交成功",MessageIcon.None);
                                break;
                            default:
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "成功", "审核成功",MessageIcon.None);
                                break;
                        }

                    else
                        switch (appView.requireEntity.CHECKSTATE)
                        {
                            case "1":
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "提交失败", MessageIcon.Error);
                                break;
                            default:
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "审核失败", MessageIcon.Error);
                                break;
                        }
                }
                else
                {
                    switch (appView.requireEntity.CHECKSTATE)
                    {
                      case "1":
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "提交失败", MessageIcon.Error);
                            break;
                        default:
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "失败", "审核失败", MessageIcon.Error);
                            break;
                    }
                }

                if (appView.requireEntity.CHECKSTATE != "0")
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                    RefreshUI(RefreshedTypes.ProgressBar);
                    // 设置控件不可用
                   // this.SetEnabled();
                }
                else
                {
                    RefreshUI(RefreshedTypes.All);
                   // loadbar.Stop();
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), ex.ToString(), MessageIcon.Error);
               // loadbar.Stop();
            }
        }

        /// <summary>
        /// 验证UI界面的数据正确性(为空或开始日期必须小于结束日期等)
        /// </summary>
        private bool CheckGroup()
        {
            string start = string.Empty;
            string end = string.Empty;
            if (string.IsNullOrEmpty(this.txtTitle.Text))
            {
                ShowMessage("check", "ERROR", "CANNOTBEEMPTYSELECT", "SurveysTITLE", true);
                return false;
            }
            if (string.IsNullOrEmpty(this.txtContent.Text))
            {
                ShowMessage("check", "ERROR", "CANNOTBEEMPTYSELECT", "SurveysContent", true);
                return false;
            }
            if (dgDistributeusers.SelectedIndex < 0)
            {
                ShowMessage("check", "ERROR", "STRINGNOTNULL", "DISTRBUTEOBJECT", true);
                return false;
            }
            if (string.IsNullOrEmpty(this.StartDate.Text))
            {
                ShowMessage("check", "ERROR", "STRINGNOTNULL", "STARTDATE", true);
                this.StartDate.Focus();
                return false;
            }
            start = this.StartDate.SelectedDate.Value.ToString("d");
            if (string.IsNullOrEmpty(this.EndDate.Text))
            {
                ShowMessage("check", "ERROR", "STRINGNOTNULL", "ENDTDATE", true);
                this.EndDate.Focus();
                return false;
            }
            end = this.EndDate.SelectedDate.Value.ToString("d");
            if (this.EndDate.SelectedDate.Value < this.StartDate.SelectedDate.Value)
            {
                ShowMessage("check", "ERROR", "CANNOTBEGREATERTHANENDDATE", "STARTDATE", true);
                return false;
            }
            List<ValidatorBase> validators = checkGroup.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (ValidatorBase validator in validators)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(validator.ErrorMessage), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 触发动画事件
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
        /// 弹出选择分发范围的lookup，并在选择后绑定到DataGrid
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
                    if(actionTypes!=FormTypes.Browse&&actionTypes!=FormTypes.Audit)
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
                    ShowMessage("error", "ERROR", "ERROR", "", false);
                    return;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        /// <summary>
        /// 绑定分发范围到页面DataGrid
        /// </summary>
        private void BindingDataGrid()
        {
            if (distributeuserList == null)
            {
                ShowMessage("check", "ERROR", "DISTRBUTEOBJECT", "CANNOTBEEMPTYSELECT", true);
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
        private void SelectEmployeesSurvey()
        {
            //scheme = new SelectScheme("requireMaster");
            //EntityBrowser browser = new EntityBrowser(scheme);
            //browser.MinHeight = 400;
            //browser.MinWidth = 600;
            //browser.FormType = FormTypes.Browse;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            SelectSchemeCW cw = new SelectSchemeCW();
            cw.SelectedClick += (obj, ev) =>
            {
                if (cw.SelectedOneObj != null)
                {
                    T_OA_REQUIREMASTER master = cw.SelectedOneObj;

                    appView.MasterId = master.REQUIREMASTERID;
                    appView.requireEntity.APPTITLE = master.REQUIRETITLE;
                    appView.requireEntity.CONTENT = master.CONTENT;
                    appView.requireEntity.T_OA_REQUIREMASTER = new T_OA_REQUIREMASTER();
                    appView.requireEntity.T_OA_REQUIREMASTER = master;
                }
            };
            cw.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 点击选择方案时,引发的事件回调函数，用来接收所选择的调查方案数据
        /// </summary>
        private void browser_ReloadDataEvent()
        {
            if (scheme.EmployeeMasterView == null)
            {
                ShowMessage("check", "ERROR", "CANNOTBEGREATERTHANENDDATE", "STARTDATE", true);
                return;
            }
            appView.MasterId = scheme.EmployeeMasterView.RequireMasterId.ToString();
            appView.requireEntity.APPTITLE = scheme.EmployeeMasterView.SurveyTitle;
            appView.requireEntity.CONTENT = scheme.EmployeeMasterView.Content;
            appView.requireEntity.T_OA_REQUIREMASTER = new T_OA_REQUIREMASTER();
            appView.requireEntity.T_OA_REQUIREMASTER.REQUIREMASTERID = scheme.EmployeeMasterView.RequireMasterId;
            //appView.requireEntity.T_OA_REQUIREMASTER = scheme.EmployeeMasterView;
        }



        /// <summary>
        /// 保存或修改数据
        /// </summary>
        private void SaveEmployeesSurveyApp()
        {
            appView.requireEntity.OPTFLAG = (bool)IsImplement.IsChecked ? "1" : "0";
            appView.requireEntity.WAY = (bool)IsFill.IsChecked ? "1" : "0";
            if (CheckGroup())
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                ObservableCollection<T_OA_DISTRIBUTEUSER> _transformList = SelectData();
                if (actionTypes == FormTypes.New)
                {
                    FillAppdata(appView.requireEntity);
                    FillDistributeuserData(_transformList);
                    appView.distributeuserList = _transformList;
                    client.AddRequireAsync(appView);
                    return;
                }              
                appView.requireEntity.UPDATEDATE = DateTime.Now;
                appView.requireEntity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                appView.requireEntity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                if (lookupObjectList.Count > 0)
                {
                    FillDistributeuserData(_transformList);
                    appView.distributeuserList = _transformList;
                }
                else
                {
                    ObservableCollection<T_OA_DISTRIBUTEUSER> _saveList = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
                    foreach (T_OA_DISTRIBUTEUSER items in _transformList)
                    {
                        var IDList = from user in distributeuserList
                                     where items.VIEWER == user.VIEWER && items.VIEWTYPE == user.VIEWTYPE
                                     select user.DISTRIBUTEUSERID;
                        if (IDList != null)
                        {
                            string ID = IDList.FirstOrDefault();
                            var dataByID = from users in appView.oldDistributeuserList
                                           where users.DISTRIBUTEUSERID == ID
                                           select users;
                            if (dataByID != null)
                            {
                                _saveList.Add(dataByID.FirstOrDefault());
                            }
                        }
                    }
                    appView.distributeuserList = _saveList;
                }
                client.UpdRequireAsync(appView);
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
                    ShowMessage("error", "ERROR", "ERROR", "",false);
                    return null;
                }
                _hanerList.Add(hander as T_OA_DISTRIBUTEUSER);
            }
            _cloneList = _hanerList.Clone();//克隆一份数据传递,以免转换数据改变UI显示
            return _cloneList;
        }


        /// <summary>
        /// 新增时填充调查申请数据
        /// </summary>
        private void FillAppdata(T_OA_REQUIRE fillApp)
        {
            appView.requireEntity.REQUIREID = key = Guid.NewGuid().ToString();
            appView.requireEntity.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
            Utility.CreateUserInfo<T_OA_REQUIRE>(appView.requireEntity);
        }

        /// <summary>
        /// 新增填充分发范围
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
                fill.FORMID = appView.requireEntity.REQUIREID;
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
        /// 从lookup选择集合中转换数据
        /// </summary>
        private string FoundView(string viewType, string view)
        {
            var data = lookupObjectList.Where(item => (item.ObjectType.ToString() == viewType) && (item.ObjectName == view)).Select(item => item.ObjectID);
            if (data != null)
            {
                return data.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 获取数据转换绑定
        /// </summary>
        private void ConvertByGetData(ObservableCollection<T_OA_DISTRIBUTEUSER> convertList)
        {
          

            for (int i = 0; i < convertList.Count; i++)
            {
                ObservableCollection<string> companyIDs = new ObservableCollection<string>();
                ObservableCollection<string> departmentIDs = new ObservableCollection<string>();
                ObservableCollection<string> postIDs = new ObservableCollection<string>();
                ObservableCollection<string> employeeIDs = new ObservableCollection<string>();

                if (convertList[i].VIEWTYPE == "0")
                    companyIDs.Add(convertList[i].VIEWER);
                else if (convertList[i].VIEWTYPE == "1")
                    departmentIDs.Add(convertList[i].VIEWER);
                else if (convertList[i].VIEWTYPE == "2")
                    postIDs.Add(convertList[i].VIEWER);
                else if (convertList[i].VIEWTYPE == "4")
                    employeeIDs.Add(convertList[i].VIEWER);

                personalClient.GetEmployeeDetailByParasPagingAsync(1, 100000,
                                                                                "T_HR_EMPLOYEE.EMPLOYEECNAME", 1,
                                                                                companyIDs,
                                                                                departmentIDs, postIDs, employeeIDs,
                                                                                i);
            }



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
                    convert.VIEWTYPE = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel.ToString();
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

        #endregion

        #region IClient资源回收

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
            switch (actionTypes)
            {
                case FormTypes.New:
                    return Utility.GetResourceStr("ADDTITLE", "EmployeeSurveyApp");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDITTITLE", "EmployeeSurveyApp");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "EmployeeSurveyApp");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT1", "EmployeeSurveyApp");
                default:
                    return Utility.GetResourceStr("ReSubmitTitle", "EmployeeSurveyApp");
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
                    SaveEmployeesSurveyApp();
                    break;
                case "1":
                    SaveEmployeesSurveyApp();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);//关闭页面
                    break;
                case "2":
                    SelectViewer();
                    break;
                case "3":
                    SelectEmployeesSurvey();
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
            if (actionTypes != FormTypes.Browse && actionTypes != FormTypes.Audit)
            {
                List<ToolbarItem> toolBaritems = new List<ToolbarItem>()
             {
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="3",Title=Utility.GetResourceStr("CHOOSEEMPLOYEESURVEY"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="2",Title=Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"},

              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},
              new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
                 ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"}
             };
                return toolBaritems;
            }
            return new List<ToolbarItem>();
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region IAudit审核流程


        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string xmlSourcce = Utility.ObjListToXml<T_OA_REQUIRE>(appView.requireEntity, "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIRE", key, xmlSourcce);
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
            appView.requireEntity.CHECKSTATE = state;
           // SaveEmployeesSurveyApp();//审核后,更改数据审核状态
            client.CheckRequireAsync(appView.requireEntity);
           
        }

        public string GetAuditState()
        {
            string state = string.Empty;
            if (appView == null)
            {
                state = null;
            }
            else
            {
                state = appView.requireEntity.CHECKSTATE;
            }

            if (actionTypes == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion
      
    }
}
