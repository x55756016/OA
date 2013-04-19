using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.SaaS.OA.UI.UserControls
{
    /// <summary>
    /// 参与调查
    /// </summary>
    public partial class Satisfaction_0 : BaseForm, IClient, IEntityEditor
    {
        private SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        /// <summary>
        /// 取字典 答案
        /// </summary>
        private PermissionServiceClient permissionClient = new PermissionServiceClient();
        /// <summary>
        /// 模板答案
        /// </summary>
        ObservableCollection<T_SYS_DICTIONARY> _oanswer;

        private string userID;
        public string UserID { get { return userID; } set { userID = value; } }
        private T_OA_SATISFACTIONREQUIRE require;
        /// <summary>
        /// 调查申请
        /// </summary>
        public T_OA_SATISFACTIONREQUIRE Require { get { return require; } set { require = value; } }
        private V_Satisfaction employeeSurveyInfo;
        public V_Satisfaction EmployeeSurveyInfo { get { return employeeSurveyInfo; } set { employeeSurveyInfo = value; } }

        public Satisfaction_0()
        {
            InitializeComponent();
            _VM.Result_SaveCompleted += new EventHandler<Result_SaveCompletedEventArgs>(Result_SaveCompleted);
            _VM.Get_SSurveyCompleted += new EventHandler<Get_SSurveyCompletedEventArgs>(Get_SSurveyCompleted);
            permissionClient.GetSysDictionaryByFatherIDCompleted += new EventHandler<GetSysDictionaryByFatherIDCompletedEventArgs>(GetSysDictionaryByFatherIDCompleted);
        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            permissionClient.GetSysDictionaryByFatherIDAsync(require.ANSWERGROUPID);          
        }
        //答案
        void GetSysDictionaryByFatherIDCompleted(object sender, GetSysDictionaryByFatherIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                //获取方案 题目，答案
                _VM.Get_SSurveyAsync(require.T_OA_SATISFACTIONMASTER.SATISFACTIONMASTERID);
                _oanswer = e.Result;
            }
        }
        void Get_SSurveyCompleted(object sender, Get_SSurveyCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                employeeSurveyInfo = e.Result as V_Satisfaction;
                txbTitle.Text = EmployeeSurveyInfo.RequireMaster.SATISFACTIONTITLE;
                txbContent.Text = EmployeeSurveyInfo.RequireMaster.CONTENT;

                //题目，答案
                foreach (T_OA_SATISFACTIONDETAIL subInfo in EmployeeSurveyInfo.SubjectViewList)
                {
                    SurveyShowList_sat subjectItem = new SurveyShowList_sat();
                    subjectItem.SubjectViewInfo = subInfo;
                    //subjectItem.ResultDetail = subInfo;
                    subjectItem.oanswer = _oanswer;
                    subjectItem.IsShowResult = 0;
                    spSurveysList.Children.Add(subjectItem);
                }
            }
        }
        //保存
        private void Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            ObservableCollection<T_OA_SATISFACTIONRESULT> resultList = new ObservableCollection<T_OA_SATISFACTIONRESULT>();
            T_OA_SATISFACTIONRESULT resultInfo = null;
            for (int i = 0; i < spSurveysList.Children.Count; i++)
            {
                resultInfo = (spSurveysList.Children[i] as SurveyShowList_sat).ResultDetail;
                resultInfo.T_OA_SATISFACTIONREQUIRE = require;

                resultInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                resultInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                resultInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                resultInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                resultInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                resultInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                resultInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                resultInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                resultInfo.UPDATEDATE = System.DateTime.Now;

                resultList.Add(resultInfo);
            }
            _VM.Result_SaveAsync(resultList);
        }
        void Result_SaveCompleted(object sender, Result_SaveCompletedEventArgs e)
        {
            if (e.Result > 0)
            {
                RefreshUI(RefreshedTypes.CloseAndReloadData);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EmployeeSubmission"));
            }
            else
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("ADDFAILED", "EmployeeSubmission"));
            }
        }
      
        #region 模式窗口
        public event UIRefreshedHandler OnUIRefreshed;

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
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
        private bool isSubmitFlow = false;
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    RefreshUI(RefreshedTypes.All);
                    isSubmitFlow = false;
                    Save();
                    break;
                case "1":
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                    break;
            }
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

        public string GetStatus()
        {
            return "";
        }

        public string GetTitle()
        {
            return Utility.GetResourceStr("SurveysContent");
        }

        private void CancelAndClose()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            permissionClient.DoClose();
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