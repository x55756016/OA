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
using SMT.Saas.Tools.PermissionWS;


namespace SMT.HRM.UI.Form.Personnel
{
    public partial class BlackListForm : BaseForm 
    {                                                   
        public T_HR_BLACKLIST entVacationSet { get; set; }
        private PersonnelServiceClient clientAtt = new PersonnelServiceClient();

        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private T_HR_BLACKLIST blackList;

        public T_HR_BLACKLIST BlackList
        {
            get { return blackList; }
            set 
            { 
                blackList = value;
                this.DataContext = value;
            }
        }

        PersonnelServiceClient client;

        public BlackListForm(FormTypes type,string blackListID)
        {
            InitializeComponent();
            FormType = type;
            InitParas(blackListID);
        }

        private void InitParas(string blackListID)
        {
            client = new PersonnelServiceClient();
            client.GetBlackListByIDCompleted += new EventHandler<GetBlackListByIDCompletedEventArgs>(client_GetBlackListByIDCompleted);
           // client.BlackListAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_BlackListAddCompleted);
          //  client.BlackListUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_BlackListUpdateCompleted);

            if (FormType == FormTypes.New)
            {
                BlackList = new T_HR_BLACKLIST();
                BlackList.BLACKLISTID = Guid.NewGuid().ToString(); 
            }
            else
            {
                client.GetBlackListByIDAsync(blackListID);
            }
        }

        void client_GetBlackListByIDCompleted(object sender, GetBlackListByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            BlackList = e.Result;
        }

        #region 保存
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //判断数据不能为空
            //if (string.IsNullOrEmpty(txtBlackName.Text.Trim()))
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NAMEISNOTNULL"));
            //    txtBlackName.Focus();
            //    return;
            //}
            //if (string.IsNullOrEmpty(txtCardNumber.Text.Trim()))
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("IDCARDNUMBERNOTNULL"));
            //    txtCardNumber.Focus();
            //    return;
            //}
            //if (string.IsNullOrEmpty(dpcBeginDate.Text.Trim()))
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BEGINDATENOTNULL"));
            //    dpcBeginDate.Focus();
            //    return;
            //}
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            }
            else
            {
                if (FormType == FormTypes.New)
                {
                    BlackList.CREATEDATE = DateTime.Now;
                    BlackList.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.BlackListAddAsync(BlackList);
                }
                else
                {
                    BlackList.UPDATEDATE = DateTime.Now;
                    BlackList.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    string strMsg = "";
                    client.BlackListUpdateAsync(BlackList,strMsg);
                }
            }
        }

        void client_BlackListUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
             //   this.ReloadData();
            }
          //  this.Close();
        }

        void client_BlackListAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED"));
             //   this.ReloadData();
            }
           // this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           // this.Close();
        }
        #endregion


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("BLACKLISTFORM");
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
                    Cancel();
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

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "保存",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = "保存并关闭",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

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
            bool flag = false;

            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
                if (validators.Count > 0)
                {
                    return false;
                }

                CheckSubmitForm(out flag);

                if (!flag)
                {
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                   // clientAtt..AddVacationSetAsync(entVacationSet);
                }
                else
                {
                   // clientAtt.ModifyVacationSetAsync(entVacationSet);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
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
    }
}

