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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class CheckProjectSetForm : BaseForm, IEntityEditor, IClient
    {
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private T_HR_CHECKPROJECTSET projectSet;

        public T_HR_CHECKPROJECTSET ProjectSet
        {
            get { return projectSet; }
            set
            {
                projectSet = value;
                this.DataContext = value;
            }
        }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        PersonnelServiceClient client;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private string checkProjectSetID;
        public CheckProjectSetForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            this.checkProjectSetID = strID;
            InitEvent(strID);
        }

        private void InitEvent(string strID)
        {
            client = new PersonnelServiceClient();
            client.GetCheckProjectSetByIDCompleted += new EventHandler<GetCheckProjectSetByIDCompletedEventArgs>(client_GetCheckProjectSetByIDCompleted);
            client.CheckProjectSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CheckProjectSetAddCompleted);
            client.CheckProjectSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CheckProjectSetUpdateCompleted);
            this.Loaded += new RoutedEventHandler(CheckProjectSetForm_Loaded);
            if (FormType == FormTypes.Browse)
            {
                this.IsEnabled = false;
            }

        }

        void CheckProjectSetForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (FormType == FormTypes.New)
            {
                ProjectSet = new T_HR_CHECKPROJECTSET();
                projectSet.CHECKPROJECTID = Guid.NewGuid().ToString();
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetCheckProjectSetByIDAsync(checkProjectSetID);
            }
        }

        void client_GetCheckProjectSetByIDCompleted(object sender, GetCheckProjectSetByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            ProjectSet = e.Result;
            SetToolBar();
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        #region 考核项目新增，修改保存
        private bool Save()
        {
            //   List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(txtScore.Text.Trim()))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CHECKPROJECTSCORE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CHECKPROJECTSCORE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (FormType == FormTypes.New)
            {
                ProjectSet.CREATEDATE = DateTime.Now;
                ProjectSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.CheckProjectSetAddAsync(ProjectSet);
            }
            else
            {
                ProjectSet.UPDATEDATE = DateTime.Now;
                ProjectSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.CheckProjectSetUpdateAsync(ProjectSet);
            }
            return true;
        }

        void client_CheckProjectSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                // this.ReloadData();
                RefreshUI(RefreshedTypes.All);
            }
            // this.Close();
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_CheckProjectSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                // this.ReloadData();
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
            // this.Close();
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
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

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CHECKPROJECTSET");
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
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    // Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            //List<ToolbarItem> items = Utility.CreateFormSaveButton();

            return ToolbarItems;
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

        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
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
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_LEFTOFFICE", LeftOffice.OWNERID,
            //        LeftOffice.OWNERPOSTID, LeftOffice.OWNERDEPARTMENTID, LeftOffice.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                //ToolbarItems = Utility.CreateFormEditButton("T_HR_LEFTOFFICE", ProjectSet.OWNERID,
                //    ProjectSet.OWNERPOSTID, ProjectSet.OWNERDEPARTMENTID, ProjectSet.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }
    }
}

