using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class ResumeForm : BaseForm, IEntityEditor,IClient
    {
        public FormTypes FormType { get; set; }
        private PersonnelServiceClient client = new PersonnelServiceClient();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private string resumeID;

        public string ResumeID
        {
            get { return resumeID; }
            set { resumeID = value; }
        }

        public ResumeForm()
        {
            ResumeID = "";
            InitializeComponent();
            FormType =  FormTypes.New;
            this.Loaded += (sender, args) =>
            {
                InitParas();
            };
        }

        public ResumeForm(FormTypes type, string resumeID)
        {
            ResumeID = resumeID;
            InitializeComponent();
            FormType = type;
            this.Loaded += (sender, args) =>
            {
                InitParas();
            };
        }
        private void InitParas()
        {
            baseinfo.LoadData(FormType, ResumeID);
            if (FormType == FormTypes.Edit)
            {
                experience.LoadData(FormType, ResumeID, baseinfo.Resume);
                educateHistory.LoadData(FormType, ResumeID, baseinfo.Resume);
            }
            client = new PersonnelServiceClient();
            client.ResumeAddCompleted += new EventHandler<ResumeAddCompletedEventArgs>(client_ResumeAddCompleted);
            client.ResumeUpdateCompleted += new EventHandler<ResumeUpdateCompletedEventArgs>(client_ResumeUpdateCompleted);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("RESUME");
        }
        public string GetStatus()
        {
            return "编辑中";
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
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = "详细信息",
            //    Tooltip = "详细信息"
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    //Url = "/Salary/SalaryStandard"
            //    //Url = "/Views/Salary/SalaryStandard.xaml?ID=1"
            //    Url = "/Salary/SalaryStandard.xaml"
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = "方案应用",
            //    Tooltip = "方案应用",
            //    Url = "/Personnel/EmployeeEntry.xaml"
            //};
            //items.Add(item);

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
        #endregion

        void client_ResumeAddCompleted(object sender, ResumeAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                 ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IDCARDNUMBERREPETITION"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("IDCARDNUMBERREPETITION"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "RESUME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //待修改
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }


        void client_ResumeUpdateCompleted(object sender, ResumeUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //待修改
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("IDCARDNUMBERREPETITION"),
                         Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "RESUME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        public bool Save()
        {
            string strMsg = "";
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (FormType == FormTypes.New)
            {
                //判断是否添加啦工作经历
                if (experience.ExperienceList.Count() <= 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXPERIENCEADD"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EXPERIENCEADD"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //if (experience.Group1.ValidateAll().Count > 0)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXPERIENCEADD"));
                //    RefreshUI(RefreshedTypes.HideProgressBar);
                //    return false;
                //}
                if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(experience.Group1))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //判断是否添加啦教育记录
                if (educateHistory.EducateHistory.Count() <= 0)
                {
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EDUCATEHISTORYADD"),
                    //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EDUCATEHISTORYADD"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //if (educateHistory.Group1.ValidateAll().Count > 0)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EDUCATEHISTORYADD"));
                //    RefreshUI(RefreshedTypes.HideProgressBar);
                //    return false;
                //}
                if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(educateHistory.Group1))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (baseinfo.FieldValue())
                {
                    client.ResumeAddAsync(baseinfo.Resume, experience.ExperienceList, educateHistory.EducateHistory,strMsg);
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    tbcContainer.SelectedIndex = 0;
                }
            }
            else
            {
                //判断是否添加啦工作经历
                if (experience.ExperienceList == null || experience.ExperienceList.Count() <= 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXPERIENCEADD"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EXPERIENCEADD"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(experience.Group1))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //判断是否添加啦教育记录
                if (educateHistory.EducateHistory == null || educateHistory.EducateHistory.Count() <= 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EDUCATEHISTORYADD"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("EDUCATEHISTORYADD"),
                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(educateHistory.Group1))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                //判断基本信息字段值不能为空
                if (baseinfo.FieldValue())
                {
                    baseinfo.Resume.UPDATEDATE = DateTime.Now;
                    baseinfo.Resume.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.ResumeUpdateAsync(baseinfo.Resume, experience.ExperienceList, educateHistory.EducateHistory, experience.DelableExp, educateHistory.DelableEdu,strMsg);
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    tbcContainer.SelectedIndex = 0;
                }
            }
            return true;
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
        private void tbcContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbcContainer == null)
            {
                return;
            }
            switch (tbcContainer.SelectedIndex.ToString())
            {
                case "0":
                    baseinfo.LoadData(FormType, ResumeID);
                    break;
                case "1":
                    experience.LoadData(FormType, ResumeID, baseinfo.Resume);
                    break;
                default:
                    educateHistory.LoadData(FormType, ResumeID, baseinfo.Resume);
                    break;
            }
        }
        #region IClient
        public void ClosedWCFClient()
        {
            //  throw new NotImplementedException();
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
    }
}
