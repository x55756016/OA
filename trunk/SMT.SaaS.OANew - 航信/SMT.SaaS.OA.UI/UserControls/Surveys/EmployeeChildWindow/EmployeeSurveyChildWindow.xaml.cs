//----------------------------------------------------
// 文件名：EmployeeSurveyChildWindow.xaml.cs
//作  用：员工调查申请
//创建人：勒中玉
//创建时间：2011-7-1
//修改人：勒中玉
//修改时间：2011-8-2
//---------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.Validator;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmployeeSurveyChildWindow : BaseForm, IAudit, IClient, IEntityEditor
    {
        #region  全局变量定义

        int _subjectid = 2;//题目ID
        int _answerIndex = 0;//问题索引
        List<string> codeList = null;//答案编号列表
        string masterID = string.Empty;//主键ID
        FormTypes actionTypes = 0;//窗体类型
        SmtOAPersonOfficeClient client = null;//WCF服务
        V_EmployeeSurveyMaster masterView = null;//员工调查自定类
        ObservableCollection<V_EmployeeSurveyInformation> subjectList = null;

        #endregion

        #region 构造函数
        public EmployeeSurveyChildWindow(FormTypes actionTypes, string masterID)
        {
            this.masterID = masterID;
            this.actionTypes = actionTypes;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSurveyChildWindow_Loaded);
        }
        #endregion

        #region XAML回调函数

        /// <summary>
        /// 删除题目
        /// </summary>
        private void questionOperation_Click(object sender, RoutedEventArgs e)
        {

            {
                if (subjectList.Count() > 1)
                {
                    V_EmployeeSurveyInformation data = ((Button)sender).DataContext as V_EmployeeSurveyInformation;
                    if (data != null)
                    {
                        subjectList.Remove(data);
                       // _subjectid--;
                        return;
                    }
                    ShowMessage("error", "ERROR", "ERROR", "", false);                    
                }
                else
                {
                    CreateNewData(true);
                }
                return;


            }

        }

        /// <summary>
        /// 删除答案
        /// </summary>
        private void answerOperation_Click(object sender, RoutedEventArgs e)
        {
           
           V_EmployeeSurveyInformation mation =
                this.dgQuestion.SelectedItem as V_EmployeeSurveyInformation;

           foreach (var items in subjectList)
           {
                if(items == mation)
                {
                    T_OA_REQUIREDETAIL tempData = ((Button)sender).DataContext as T_OA_REQUIREDETAIL;

                    if (items.AnswerList.Count > 1)
                    {
                        items.AnswerList.Remove(tempData);
                        _answerIndex--;

                        List<string> code = new List<string>() {"A", "B", "C", "D", "E", "F", "G" };
                       
                        for (int j = 0; j < items.AnswerList.Count; j++)
                        {
                            items.AnswerList[j].CODE = code[j];
                        }
                    }
                }
           }

       }

        /// <summary>
        /// 新增问题模板列
        /// </summary>
        private void textQuestion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                CreateNewData(false);
            }
        }

        /// <summary>
        /// 新增答案模板列
        /// </summary>
        private void textAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGrid dgParent = new DataGrid();
                TextBox _textBox = (TextBox)sender;
                dgParent = Utility.GetParentObject<DataGrid>(_textBox, "dgAnswer");
                if (dgParent != null)
                {
                    var data = dgParent.ItemsSource as ObservableCollection<T_OA_REQUIREDETAIL>;
                    if (data != null)
                    {
                        if (_answerIndex < 6)
                        {
                            data.Add(new T_OA_REQUIREDETAIL { CODE = codeList[_answerIndex], CONTENT = "",REQUIREDETAILID = Guid.NewGuid().ToString()});
                            _answerIndex++;
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBoxs("警告","最多只能添加6个选项", Utility.GetResourceStr("CONFIRM"), MessageIcon.None);
                        }
                    }

                }
            }
        }

        #endregion

        #region 事件完成函数

        /// <summary>
        /// 初始化变量,根据窗体类型判断操作
        /// </summary>
        void EmployeeSurveyChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EventResgister();

            if (actionTypes != FormTypes.New)
            {
                if (!string.IsNullOrEmpty(masterID))
                {
                    if (actionTypes == FormTypes.Browse || actionTypes == FormTypes.Audit)
                    {
                        this.dgQuestion.IsEnabled = false;
                        this.textTitle.IsEnabled = false;
                        this.textContent.IsEnabled = false;
                    }
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    client.GetMasterDataByLoadingAsync(masterID);
                    return;
                }
                ShowMessage("error", "ERROR", "ERROR","", false);
            }
            CreateNewData(true);
            Bingding();
        }

        /// <summary>
        /// 修改后
        /// </summary>
        void client_UpdRequireMasterCompleted(object sender, UpdRequireMasterCompletedEventArgs e)
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
                if (masterView.masterEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("error", "AUDITFAILURE", "SUBMITFAILED", "", true);
                }
                else if (masterView.masterEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("error", "ERROR", "ERROR", "", false);
                }
                else
                {
                    ShowMessage("error", "UPDATEFAILED", "UPDATEISSUEFAILED", "", true);
                }
            }
            else
            {
                if (masterView.masterEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("", "SUCCESSSUBMITAUDIT", "SUBMITSUCCESSED", "", true);
                }
                else if (masterView.masterEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("", "AUDITPASS", "AUDITSUCCESSED","", true);
                }
                else
                {
                    ShowMessage("", "UPDATASUCCESSED", "UPDATESUCCESSED", "", true);
                }
            }

        }

        /// <summary>
        /// 新增后
        /// </summary>
        void client_AddRequireMasterCompleted(object sender, AddRequireMasterCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage("error", "ERROR", "ERROR", "", false);
                return;
            }
            if (e.Result)
            {
                EntityBrowser entityBrowser = this.FindParentByType<EntityBrowser>();
                entityBrowser.FormType = actionTypes = FormTypes.Edit;
                ShowAuditControl();
                ShowMessage("", "ADDDATASUCCESSED", "ADDSUCCESSED","", true);
                return;
            }
            ShowMessage("error", "ADDFAILED", "ADDDATAFAILED","", true);
        }

        /// <summary>
        /// 非新增时，获取数据并绑定
        /// </summary>
        void client_GetMasterDataByLoadingCompleted(object sender, GetMasterDataByLoadingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ShowMessage("error", "ERROR", "GETDATAFAILED","", false);
                return;
            }
            if (e.Result == null)
            {
                ShowMessage("", "NODATA", "DIDNOTFINDRELEVANT","", true);
                return;
            }
            masterView.masterEntity = e.Result.masterEntity;
            masterView.SubjectList = e.Result.SubjectList;
            masterView.RequireMasterId = e.Result.RequireMasterId;
            Bingding();
            ShowAuditControl();
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
        private void ShowMessage(string action, string title, string message, string other,bool isshow)
        {
            if (action == "error" && isshow)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message,"EmployeeSurvey"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else if (action == "error")
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else if (action == "check")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr(title), Utility.GetResourceStr(message,other), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr(title), Utility.GetResourceStr(message,"EmployeeSurvey"), Utility.GetResourceStr("CONFIRM"));
            }
        }

        /// <summary>
        /// 后台事件注册及全局变量初始化
        /// </summary>
        private void EventResgister()
        {
            client = new SmtOAPersonOfficeClient();//实例化服务
            client.AddRequireMasterCompleted += new EventHandler<AddRequireMasterCompletedEventArgs>(client_AddRequireMasterCompleted);
          //  client.UpdRequireMasterCompleted += new EventHandler<UpdRequireMasterCompletedEventArgs>(client_UpdRequireMasterCompleted);
            client.GetMasterDataByLoadingCompleted += new EventHandler<GetMasterDataByLoadingCompletedEventArgs>(client_GetMasterDataByLoadingCompleted);

            masterView = new V_EmployeeSurveyMaster();
            masterView.masterEntity = new T_OA_REQUIREMASTER();
            masterView.masterEntity.T_OA_REQUIREDETAIL2 = new ObservableCollection<T_OA_REQUIREDETAIL2>();
            masterView.answerList = new ObservableCollection<T_OA_REQUIREDETAIL>();
            subjectList = new ObservableCollection<V_EmployeeSurveyInformation>();
            codeList = new List<string>() { "B", "C", "D", "E", "F", "G" };
            client.UpdateEmployeeSurveysCompleted += new EventHandler<UpdateEmployeeSurveysCompletedEventArgs>(client_UpdateEmployeeSurveysCompleted);
        }

        void client_UpdateEmployeeSurveysCompleted(object sender, UpdateEmployeeSurveysCompletedEventArgs e)
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
                if (masterView.masterEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("error", "AUDITFAILURE", "SUBMITFAILED", "", true);
                }
                else if (masterView.masterEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("error", "ERROR", "ERROR", "", false);
                }
                else
                {
                    ShowMessage("error", "UPDATEFAILED", "UPDATEISSUEFAILED", "", true);
                }
            }
            else
            {
                if (masterView.masterEntity.CHECKSTATE == _approving)
                {
                    ShowMessage("", "SUCCESSSUBMITAUDIT", "SUBMITSUCCESSED", "", true);
                }
                else if (masterView.masterEntity.CHECKSTATE == _approved)
                {
                    ShowMessage("", "AUDITPASS", "AUDITSUCCESSED", "", true);
                }
                else
                {
                    ShowMessage("", "UPDATASUCCESSED", "UPDATESUCCESSED", "", true);
                }
            }
        }

        /// <summary>
        /// 创建或增加绑定到DataGrid的初始化数据,使得模板列可以显示出来
        /// </summary>
        private void CreateNewData(bool action)
        {
            if (action)
            {
                subjectList.Clear();
                subjectList.Add(
                    new V_EmployeeSurveyInformation
                    {
                        Subject = new T_OA_REQUIREDETAIL2 { SUBJECTID = 1, CONTENT = "" },
                        AnswerList =
                            new ObservableCollection<T_OA_REQUIREDETAIL> { new T_OA_REQUIREDETAIL { CODE = "A", SUBJECTID = 1, CONTENT = "" } }
                    });
            }
            else
            {
                subjectList.Add(
                        new V_EmployeeSurveyInformation
                        {
                            Subject = new T_OA_REQUIREDETAIL2 { SUBJECTID = _subjectid, CONTENT = "" },
                            AnswerList =
                                new ObservableCollection<T_OA_REQUIREDETAIL> { new T_OA_REQUIREDETAIL { CODE = "A", SUBJECTID = _subjectid, CONTENT = "" } }
                        });

                _answerIndex = 0;
                codeList = new List<string>() { "B", "C", "D", "E", "F", "G" };
                _subjectid++;
            }
        }

        private void Bingding()
        {
            this.parentsInfo.DataContext = masterView.masterEntity;
            if (actionTypes != FormTypes.New)
            {
                subjectList = masterView.SubjectList;
            }
            this.dgQuestion.ItemsSource = null;
            this.dgQuestion.ItemsSource = subjectList;
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
        /// 新增或修改填充方案数据
        /// </summary>
        private void CreateMasterData()
        {
            if (actionTypes == FormTypes.New)
            {
                masterView.masterEntity.REQUIREMASTERID = Guid.NewGuid().ToString();
                masterView.masterEntity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                Utility.CreateUserInfo<T_OA_REQUIREMASTER>(masterView.masterEntity);
                return;
            }
            masterView.RequireMasterId = masterView.masterEntity.REQUIREMASTERID;
            masterView.masterEntity.UPDATEDATE = DateTime.Now;
            masterView.masterEntity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            masterView.masterEntity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }

        /// <summary>
        /// 新增或修改时填充问题数据
        /// </summary>
        private void CreateChildData()
        {
            foreach (var data in subjectList)
            {
                if (actionTypes == FormTypes.New)
                {
                    data.Subject.REQUIREDETAIL2ID = Guid.NewGuid().ToString();
                    data.Subject.REQUIREMASTERID = masterView.masterEntity.REQUIREMASTERID;
                    Utility.CreateUserInfo<T_OA_REQUIREDETAIL2>(data.Subject);
                }
                else
                {
                    data.Subject.UPDATEDATE = DateTime.Now;
                    data.Subject.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    data.Subject.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                }

                foreach (var child in data.AnswerList)
                {
                    Utility.CreateUserInfo<T_OA_REQUIREDETAIL>(child);
                    child.REQUIREMASTERID = masterView.masterEntity.REQUIREMASTERID;
                    if (actionTypes == FormTypes.New)
                    {
                        child.REQUIREDETAILID = Guid.NewGuid().ToString();
                      //  child.REQUIREMASTERID = masterView.masterEntity.REQUIREMASTERID;
                    }
                    else
                    {
                        child.UPDATEDATE = DateTime.Now;
                        child.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        child.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                       
                    }
                    child.SUBJECTID = data.Subject.SUBJECTID;
                    masterView.answerList.Add(child);
                }
                masterView.masterEntity.T_OA_REQUIREDETAIL2.Add(data.Subject
  );
            }

        }

        /// <summary>
        /// 验证数据正确性
        /// </summary>
        private bool CheckGropup()
        {

            if (string.IsNullOrEmpty(this.textTitle.Text))
            {
                ShowMessage("check", "ERROR", "CANNOTBEEMPTYSELECT", "SurveysTITLE",true);
               return false;
            }
            if (string.IsNullOrEmpty(this.textContent.Text))
            {
                ShowMessage("check", "ERROR", "CANNOTBEEMPTYSELECT", "SurveysContent", true);
                return false;
            }
            TextBox textQuestion = Utility.FindChildControl<TextBox>(this.dgQuestion, "textQuestion");
            if (string.IsNullOrEmpty(textQuestion.Text))
            {
                ShowMessage("check", "ERROR", "CANNOTBEEMPTYSELECT", "SubjectContent", true);
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
        /// 保存或修改
        /// </summary>
        private void SaveMasterData()
        {
            if (CheckGropup())
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                CreateMasterData();
                CreateChildData();
                if (actionTypes == FormTypes.New)
                {
                    client.AddRequireMasterAsync(masterView);
                    return;
                }
                //client.UpdRequireMasterAsync(masterView);
                client.UpdateEmployeeSurveysAsync(masterView);
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
                    return Utility.GetResourceStr("ADDTITLE", "EmployeeSurvey");
                case FormTypes.Edit:
                    return Utility.GetResourceStr("EDITTITLE", "EmployeeSurvey");
                case FormTypes.Browse:
                    return Utility.GetResourceStr("VIEWTITLE", "EmployeeSurvey");
                case FormTypes.Audit:
                    return Utility.GetResourceStr("AUDIT1", "EmployeeSurvey");
                default:
                    return Utility.GetResourceStr("ReSubmitTitle", "EmployeeSurvey");
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
                    SaveMasterData();
                    break;
                case "1":
                    SaveMasterData();
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
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
                  new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="1",Title=Utility.GetResourceStr("SAVEANDCLOSE"),ImageUrl="/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"},

             new ToolbarItem{DisplayType=ToolbarItemDisplayTypes.Image,Key="0",Title=Utility.GetResourceStr("SAVE"),
             ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"}
           
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
            string masterID = masterView.masterEntity.REQUIREMASTERID;
            string xmlObjectSource = Utility.ObjListToXml<T_OA_REQUIREMASTER>(masterView.masterEntity
                , "OA");
            Utility.SetAuditEntity(entity, "T_OA_REQUIREMASTER", masterID, xmlObjectSource);

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
            masterView.masterEntity.CHECKSTATE = state;
            SaveMasterData();
        }

        public string GetAuditState()
        {
            string state = string.Empty;

            if (masterView != null)
            {
                state = masterView.masterEntity.CHECKSTATE;
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
