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
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
//using SMT.HRM.UI.HrCommonWS;
using SMT.Saas.Tools.SalaryWS;

namespace SMT.HRM.UI.Form.Setting
{
    public partial class OperatorParamSet : BaseForm, IEntityEditor
    {
        //private HrCommonServiceClient client;
        private SalaryServiceClient client = new SalaryServiceClient();
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public T_HR_SYSTEMSETTING systemSettingPost { get; set; }

        private T_HR_SYSTEMSETTING systemSetting;

        public T_HR_SYSTEMSETTING SystemSetting
        {
            get { return systemSetting; }
            set
            {
                systemSetting = value;
                this.DataContext = value;
                try
                {
                    cbxmodetype.SelectedIndex = Convert.ToInt32(SystemSetting.MODELTYPE)+1;
                    tbmodetypevalue.Text=SystemSetting.MODELVALUE;
                    txtparamname.Text=SystemSetting.PARAMETERNAME;
                    txtparamvalue.Text= SystemSetting.PARAMETERVALUE;
                }
                catch { }
            }
        }

        public OperatorParamSet()
        {
            InitializeComponent();
        }
        public OperatorParamSet(FormTypes type, string SystemParamSetID,string modeltype)
        {
            InitializeComponent();
            FormType = type;
            InitContent(SystemParamSetID,modeltype);
        }

        public void InitContent(string SystemParamSetID, string modeltype)
        {
            client.AddSystemParamSetCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AddSystemParamSetCompleted);
            client.SystemParamSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SystemParamSetUpdateCompleted);
            client.GetSystemParamSetCompleted += new EventHandler<GetSystemParamSetCompletedEventArgs>(client_GetSystemParamSetCompleted);
            if (FormType == FormTypes.New)
            {
                SystemSetting = new T_HR_SYSTEMSETTING();
                cbxmodetype.SelectedIndex = Convert.ToInt32(modeltype)+1;
                SystemSetting.SYSTEMSETTINGID = Guid.NewGuid().ToString();
            }
            else
            {
                client.GetSystemParamSetAsync(SystemParamSetID);
            }
        }

        void client_GetSystemParamSetCompleted(object sender, GetSystemParamSetCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            SystemSetting = e.Result;
        }
        void client_SystemParamSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SYSTEMPARAMSET"));
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        void client_AddSystemParamSetCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SYSTEMPARAMSET"));
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("SYSTEMPARAMSET");
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
                    SaveAndClose();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息A",
                Tooltip = "详细信息A"
            };
            items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = "详细信息B",
            //    Tooltip = "详细信息B"
            //};
            //items.Add(item);
            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };


            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

        public void Save()
        {
            //List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            //if (validators.Count > 0)
            //{
            //    // MessageBox.Show(validators.Count.ToString() + " invalid validators");
            //}

                SystemSetting.MODELTYPE = (cbxmodetype.SelectedIndex-1).ToString();
                SystemSetting.MODELVALUE = tbmodetypevalue.Text;
                SystemSetting.PARAMETERNAME = txtparamname.Text;
                SystemSetting.PARAMETERVALUE = txtparamvalue.Text;
                if (SystemSetting.MODELTYPE == "4")
                    SystemSetting.PARAMETERVALUE = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(SystemSetting.PARAMETERVALUE);
                SystemSetting.REMARK = txtremark.Text;
                RefreshUI(RefreshedTypes.ProgressBar);
                if (FormType == FormTypes.Edit)
                {
                    SystemSetting.UPDATEDATE = System.DateTime.Now;
                    client.SystemParamSetUpdateAsync(SystemSetting,systemSettingPost);
                }
                else
                {
                    SystemSetting.CREATEDATE = System.DateTime.Now;
                    SystemSetting.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    SystemSetting.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    SystemSetting.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SystemSetting.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    SystemSetting.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    SystemSetting.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    SystemSetting.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    SystemSetting.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    client.AddSystemParamSetAsync(SystemSetting);
                }
        }

        public void SaveAndClose()
        {

        }
    }
}
