using System;
using System.Collections.Generic;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class OvertimeRewardSetForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public FormTypes FormType { get; set; }

        public string OvertimeRewardID { get; set; }

        public T_HR_OVERTIMEREWARD entOvertimeReward { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        #endregion

        #region 初始化
        public OvertimeRewardSetForm(FormTypes formtype, string strOvertimeRewardID)
        {
            FormType = formtype;
            OvertimeRewardID = strOvertimeRewardID;
            InitializeComponent();
            this.Loaded += new System.Windows.RoutedEventHandler(OvertimeRewardSetForm_Loaded);
        }

        void OvertimeRewardSetForm_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetOvertimeRewardByIDCompleted += new EventHandler<GetOvertimeRewardByIDCompletedEventArgs>(clientAtt_GetOvertimeRewardByIDCompleted);
            clientAtt.AddOvertimeRewardCompleted += new EventHandler<AddOvertimeRewardCompletedEventArgs>(clientAtt_AddOvertimeRewardCompleted);
            clientAtt.ModifyOvertimeRewardCompleted += new EventHandler<ModifyOvertimeRewardCompletedEventArgs>(clientAtt_ModifyOvertimeRewardCompleted);
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                InitForm();
                SetToolBar();
            }
            else
            {
                LoadData();
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entOvertimeReward = new T_HR_OVERTIMEREWARD();
            entOvertimeReward.OVERTIMEREWARDID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entOvertimeReward.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entOvertimeReward.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entOvertimeReward.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entOvertimeReward.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entOvertimeReward.CREATEDATE = DateTime.Now;
            entOvertimeReward.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entOvertimeReward.UPDATEDATE = System.DateTime.Now;
            entOvertimeReward.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据            
            entOvertimeReward.USUALOVERTIMEPAYRATE = decimal.Parse("0");
            entOvertimeReward.WEEKENDPAYRATE = decimal.Parse("0");
            entOvertimeReward.VACATIONPAYRATE = decimal.Parse("0");

            this.DataContext = entOvertimeReward;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(OvertimeRewardID))
            {
                return;
            }

            clientAtt.GetOvertimeRewardByIDAsync(OvertimeRewardID);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("OVERTIMEREWARDSET");            
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        #endregion

        #region 私有方法

        /// <summary>
        /// 
        /// </summary>
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;

            try
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIREDFIELDS"));
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }                

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddOvertimeRewardAsync(entOvertimeReward);
                    flag = true;
                }
                else
                {
                    clientAtt.ModifyOvertimeRewardAsync(entOvertimeReward);
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                RefreshUI(RefreshedTypes.HideProgressBar);
            }

            return flag;
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();
            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        #region 事件
        /// <summary>
        /// 根据主键索引，获得指定的假期记录以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetOvertimeRewardByIDCompleted(object sender, GetOvertimeRewardByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entOvertimeReward = e.Result;                

                entOvertimeReward.UPDATEDATE = DateTime.Now;
                entOvertimeReward.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = entOvertimeReward;

                SetToolBar();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 新增假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddOvertimeRewardCompleted(object sender, AddOvertimeRewardCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 更新假期记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyOvertimeRewardCompleted(object sender, ModifyOvertimeRewardCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "OVERTIMEREWARDSET")));
                    InitParas();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        #endregion
    }
}
