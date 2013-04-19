using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PerformanceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Performance
{
    public partial class GroupInfo : BaseForm, IEntityEditor
    {
        public T_HR_RANDOMGROUP RandomGroup { get; set; }
        public FormTypes FormType { get; set; }
        private PerformanceServiceClient client = new PerformanceServiceClient();
        private bool isClose = false;

        public GroupInfo(FormTypes type, string randomGroupID)
        {
            FormType = type;
            InitializeComponent();
            SetDescEnable(false);
            InitPara(randomGroupID);
        }

        private void SetDescEnable(bool p)
        {
            txtRandomGroupName.IsEnabled = p;
            txtRandomGroupDesc.IsEnabled = p;
        }

        private void InitPara(string randomGroupID)
        {
            //加载事件
            client.GetRandomGroupByIDCompleted += new EventHandler<GetRandomGroupByIDCompletedEventArgs>(client_GetRandomGroupByIDCompleted);
            client.AddRandomGroupCompleted += new EventHandler<AddRandomGroupCompletedEventArgs>(client_AddRandomGroupCompleted);
            client.UpdateRandomGroupCompleted += new EventHandler<UpdateRandomGroupCompletedEventArgs>(client_UpdateRandomGroupCompleted);
            //窗口状态
            switch ((int)FormType)
            {
                case 0://NEW
                    RandomGroup = new T_HR_RANDOMGROUP();
                    RandomGroup.RANDOMGROUPID = Guid.NewGuid().ToString();
                    SetDescEnable(true);

                    break;
                case 1://EDIT
                    client.GetRandomGroupByIDAsync(randomGroupID);
                    SetDescEnable(true);

                    break;
                case 2: //BROWE
                    client.GetRandomGroupByIDAsync(randomGroupID);
                    SetDescEnable(false);
                    break;
                case 3: //ADUIT
                    break;
            }
        }

       


        /// <summary>
        /// 绑定窗口数据
        /// </summary>
        private void BindData()
        {
            //窗口绑定
            LayoutRoot.DataContext = RandomGroup;
        }

        private void Save()
        {
            //处理页面验证
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "RANDOMGROUPNAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "RANDOMGROUPNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);

            }
            else
            {
                //抽查组
                RandomGroup.RANDOMGROUPNAME = txtRandomGroupName.Text.Trim();
                RandomGroup.GROUPREMARK = txtRandomGroupDesc.Text.Trim();
                RandomGroup.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                RandomGroup.UPDATEDATE = DateTime.Now;
                string strMsg = string.Empty;
                if (FormType == FormTypes.Edit)
                {
                    client.UpdateRandomGroupAsync(RandomGroup,strMsg);
                }
                else if (FormType == FormTypes.New)
                {
                    //所属
                    RandomGroup.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    RandomGroup.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    RandomGroup.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    RandomGroup.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    RandomGroup.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    RandomGroup.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    RandomGroup.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    RandomGroup.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    RandomGroup.CREATEDATE = DateTime.Now;
                    client.AddRandomGroupAsync(RandomGroup, strMsg);
                }
            }
        }

        #region 所有事件

        /// <summary>
        /// 获取抽查组完成事件，加载抽查组的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetRandomGroupByIDCompleted(object sender, GetRandomGroupByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //异常
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                //未获取到信息
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                    return;
                }
                //获取类别
                RandomGroup = e.Result;
                BindData();
            }
        }

        /// <summary>
        /// 添加完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        void client_AddRandomGroupCompleted(object sender, AddRandomGroupCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITION", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITION", "RANDOMGROUPNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.HideProgressBar);
                FormType = FormTypes.Edit;
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        void client_UpdateRandomGroupCompleted(object sender, UpdateRandomGroupCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITION", "RANDOMGROUPNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITION", "RANDOMGROUPNAME"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (isClose)
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                else
                    RefreshUI(RefreshedTypes.All);
            }
        }
       

        #endregion 所有事件

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("RANDOMGROUP");
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
                    isClose = false;
                    break;
                case "1":
                    isClose = true;
                    break;
            }
            Save();
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("RANDOMGROUPINFO"),
                Tooltip = Utility.GetResourceStr("RANDOMGROUPINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
            }

            return items;
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
    }
}
