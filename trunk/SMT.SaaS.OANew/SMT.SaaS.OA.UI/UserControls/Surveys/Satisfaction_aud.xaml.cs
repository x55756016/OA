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

using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class Satisfaction_aud : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        /// <summary>
        /// 方案
        /// </summary>
        private V_Satisfaction _survey;
        public V_Satisfaction _Survey { get { return _survey; } set { _survey = value; } }
        /// <summary>
        /// 题目
        /// </summary>
        private ObservableCollection<T_OA_SATISFACTIONDETAIL> _osub = new ObservableCollection<T_OA_SATISFACTIONDETAIL>();

        private bool isSubmitFlow = false;
        private RefreshedTypes saveType;
        private FormTypes types;
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        #endregion

        #region 构造函数
        public Satisfaction_aud()
        {
            InitializeComponent();
            _VM.Upd_SSurveyCheckedCompleted += new EventHandler<Upd_SSurveyCheckedCompletedEventArgs>(Upd_SSurveyCheckedCompleted);
        }
        public Satisfaction_aud(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();
            this.types = operationType;
            _VM.Upd_SSurveyCheckedCompleted += new EventHandler<Upd_SSurveyCheckedCompletedEventArgs>(Upd_SSurveyCheckedCompleted);
            _VM.Get_SSurveyCompleted += new EventHandler<Get_SSurveyCompletedEventArgs>(Get_SSurveyCompleted);
            _VM.Get_SSurveyAsync(SendDocID);
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Load_Data();
        }
        #endregion

        #region Load_Data
        private void Load_Data()
        {
            T_OA_SATISFACTIONMASTER s = _survey.RequireMaster;
            txtTitle.Text = s.SATISFACTIONTITLE;
            txtContent.Text = s.CONTENT;
            _osub = _survey.SubjectViewList;

            dg.ItemsSource = _osub;
            dg.SelectedIndex = 0;

            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //InitAudit(_survey.RequireMaster.SATISFACTIONMASTERID);
            //viewApproval.XmlObject = DataObjectToXml<T_OA_SATISFACTIONMASTER>.ObjListToXml(_survey.RequireMaster, "OA");
        }
        #endregion
        
        #region 题目


        #endregion 题目

        #region IEntityEditor

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("OASatisfaction");
        }
        //工具栏
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
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

        #region 设置 方案其它信息
        /// <summary>
        /// 设置 方案其它信息
        /// </summary>
        /// <param name="i"></param>
        private void SetSurvey()
        {
            _Survey.RequireMaster.CREATEDATE = System.DateTime.Now;
            _Survey.RequireMaster.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            _Survey.RequireMaster.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            _Survey.RequireMaster.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            _Survey.RequireMaster.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            _Survey.RequireMaster.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            _Survey.RequireMaster.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        #endregion

        #region 设置 题目其它信息
        /// <summary>
        /// 设置 题目其它信息
        /// </summary>
        /// <param name="i"></param>
        private static void SetSubject(ref T_OA_SATISFACTIONDETAIL i)
        {
            i.CREATEDATE = System.DateTime.Now;
            i.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            i.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            i.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            i.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            i.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            i.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
        }
        #endregion

        #region 保存
        private void Save()
        {
            //先更新
            _VM.Upd_SSurveyCheckedAsync(_Survey);
        }
        #endregion

        #region 修改
        void Upd_SSurveyCheckedCompleted(object sender, Upd_SSurveyCheckedCompletedEventArgs e)//先更新方案成功后， 添加 和修改题目、答案
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
            if (e.Result > 0)
            {
                Utility.ShowMessageBox("AUDITSUCCESSED", true, true);
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
            else
            {
                Utility.ShowMessageBox("AUDITFAILURE", true, false);
            }
        }
        #endregion

        #region 根据ID查询
        void Get_SSurveyCompleted(object sender, Get_SSurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                _survey = e.Result;
                Load_Data();
            }
        }
        #endregion
       
        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_SATISFACTIONMASTER>(_survey.RequireMaster, "OA");
            Utility.SetAuditEntity(entity, "T_OA_SATISFACTIONMASTER", _survey.RequireMaster.SATISFACTIONMASTERID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = string.Empty;
            string UserState = string.Empty;
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (_survey.RequireMaster.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            _survey.RequireMaster.CHECKSTATE = state;
            isSubmitFlow = true;
            _VM.Upd_SSurveyAsync(_Survey, UserState);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (_survey != null)
                state = _survey.RequireMaster.CHECKSTATE;
            if (types == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

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
