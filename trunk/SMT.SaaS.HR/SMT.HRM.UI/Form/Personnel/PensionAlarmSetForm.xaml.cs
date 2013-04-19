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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class PensionAlarmSetForm : BaseForm, IEntityEditor, IClient
    {
        private T_HR_PENSIONALARMSET pension;

        public T_HR_PENSIONALARMSET Pension
        {
            get { return pension; }
            set
            {
                pension = value;
                this.DataContext = value;
            }
        }

        private FormTypes formType;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private string pensionAlarmSetID;
        PersonnelServiceClient client;
        public PensionAlarmSetForm(FormTypes type, string pensionAlarmSetID)
        {
            InitializeComponent();
            FormType = type;
            this.pensionAlarmSetID = pensionAlarmSetID;
            InitParas(pensionAlarmSetID);
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            client.GetPensionAlarmSetViewByIDCompleted += new EventHandler<GetPensionAlarmSetViewByIDCompletedEventArgs>(client_GetPensionAlarmSetViewByIDCompleted);
            client.PensionAlarmSetAddCompleted += new EventHandler<PensionAlarmSetAddCompletedEventArgs>(client_PensionAlarmSetAddCompleted);
            client.PensionAlarmSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PensionAlarmSetUpdateCompleted);
            this.Loaded += new RoutedEventHandler(PensionAlarmSetForm_Loaded);
            if (FormType == FormTypes.Browse)
            {
                // this.IsEnabled = false;
            }

        }

        void PensionAlarmSetForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (FormType == FormTypes.New)
            {
                Pension = new T_HR_PENSIONALARMSET();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetPensionAlarmSetViewByIDAsync(pensionAlarmSetID);
            }
        }



        void client_GetPensionAlarmSetViewByIDCompleted(object sender, GetPensionAlarmSetViewByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Pension = e.Result.T_HR_PENSIONALARMSET;
                lkCompany.TxtLookUp.Text = e.Result.CNAME;
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_GetPensionAlarmSetByIDCompleted(object sender, GetPensionAlarmSetByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                Pension = e.Result;
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("PENSIONALARMSET");
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
        private bool Save()
        {
            //  List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (string.IsNullOrEmpty(lkCompany.TxtLookUp.Text.Trim()))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANYNAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANYNAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            //if (validators.Count > 0)
            //{
            //    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                Pension.ALARMDOWN = Convert.ToInt32(numAlermDown.Value);
                Pension.ALARMPAY = Convert.ToInt32(numAlermPay.Value);
                if (FormType == FormTypes.New)
                {
                    Pension.PENSIONSETID = Guid.NewGuid().ToString();
                    Pension.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Pension.CREATEDATE = DateTime.Now;
                    pension.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    pension.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    pension.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    string strMsg = "";
                    client.PensionAlarmSetAddAsync(Pension, strMsg);
                }
                else
                {
                    Pension.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Pension.UPDATEDATE = DateTime.Now;
                    client.PensionAlarmSetUpdateAsync(Pension);
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

        void client_PensionAlarmSetAddCompleted(object sender, PensionAlarmSetAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return;
                }
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
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_PensionAlarmSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;

                if (ent != null)
                {
                    lkCompany.DataContext = ent;
                    Pension.COMPANYID = ent.COMPANYID;
                    pension.OWNERCOMPANYID = ent.COMPANYID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}

