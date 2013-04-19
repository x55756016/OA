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
    public partial class BlackListForm2 : BaseForm, IEntityEditor, IClient
    {
        PersonnelServiceClient client;

        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private string blackListID;
        private T_HR_BLACKLIST blacklist;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public T_HR_BLACKLIST BlackList
        {
            get { return blacklist; }
            set
            {
                blacklist = value;
                this.DataContext = value;
            }
        }
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public BlackListForm2()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            this.blackListID = "";
            InitParas(blackListID);
        }
        public BlackListForm2(FormTypes type, string blackListID)
        {
            InitializeComponent();
            FormType = type;
            this.blackListID = blackListID;
            InitParas(blackListID);
        }

        private void InitParas(string blackListID)
        {
            client = new PersonnelServiceClient();
            client.GetBlackListByIDCompleted += new EventHandler<GetBlackListByIDCompletedEventArgs>(client_GetBlackListByIDCompleted);
            // client.BlackListAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_BlackListAddCompleted);
            client.BlackListAddCompleted += new EventHandler<BlackListAddCompletedEventArgs>(client_BlackListAddCompleted);
            client.BlackListUpdateCompleted += new EventHandler<BlackListUpdateCompletedEventArgs>(client_BlackListUpdateCompleted);
            this.Loaded += new RoutedEventHandler(BlackListForm2_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse)
            {
                this.IsEnabled = false;
            }
            */
            #endregion

        }

        void BlackListForm2_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Browse)
            {
                this.IsEnabled = false;
            }
            #endregion

            if (FormType == FormTypes.New)
            {
                BlackList = new T_HR_BLACKLIST();
                BlackList.BLACKLISTID = Guid.NewGuid().ToString();
                BlackList.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetBlackListByIDAsync(blackListID);
            }
        }

        void client_BlackListUpdateCompleted(object sender, BlackListUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
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

        void client_BlackListAddCompleted(object sender, BlackListAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != "SAVESUCCESSED")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.Result),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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

                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_GetBlackListByIDCompleted(object sender, GetBlackListByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            BlackList = e.Result;
            RefreshUI(RefreshedTypes.HideProgressBar);
        }


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("BLACKLIST");
        }
        public string GetStatus()
        {
            //return BlackList != null ? BlackList.BLACKLISTID : "";
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
                    //  Cancel();
                    break;
            }
        }
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
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
        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entVacationSet"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = true;
        }
        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            //  List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //if (validators.Count > 0)
            //{
            //    //  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(txtCardNumber.Text.Trim()))
            {
                //could use the content of the list to show an invalid message summary somehow
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "IDCARDNUMBER"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "IDCARDNUMBER"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                txtCardNumber.Focus();
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            if (FormType == FormTypes.Edit)
            {
                BlackList.UPDATEDATE = System.DateTime.Now;
                BlackList.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //BlackList.NAME = txtBlackName.Text;
                //blacklist.IDCARDNUMBER = txtCardNumber.Text;
                string strMsg = "";
                client.BlackListUpdateAsync(BlackList, strMsg);
            }
            else
            {
                //所属
                BlackList.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                BlackList.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                BlackList.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                BlackList.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.BlackListAddAsync(BlackList);
            }

            return true;
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
            entBrowser.Close();//Margin="6,8,6,8"
        }
    }
}
