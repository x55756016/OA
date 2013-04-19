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
using System.Windows.Navigation;

using System.Windows.Data;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Controls.Primitives;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Views.Salary
{
    public partial class SalaryPasswordSet : BasePage, IClient
    {
        SMTLoading loadbar = new SMTLoading();
        private SalaryServiceClient client = new SalaryServiceClient();
        public SalaryPasswordSet()
        {
            InitializeComponent();
            GetEntityLogo("T_HR_AREADIFFERENCE");
            InitParas();
        }

        public void InitParas()
        {
            PARENT.Children.Add(loadbar);
            client.GetSystemParamSetPagingCompleted += new EventHandler<GetSystemParamSetPagingCompletedEventArgs>(client_GetSystemParamSetPagingCompleted);
            client.SystemParamSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SystemParamSetUpdateCompleted);
            loadbar.Stop();
        }

        public void EditData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            filter += "MODELTYPE==@" + paras.Count().ToString();
            paras.Add("4");

            filter += " and PARAMETERVALUE==@" + paras.Count().ToString();
            paras.Add(SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(oldpwd.Password));

            filter += " and OWNERID==@" + paras.Count().ToString();
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            loadbar.Start();

            client.GetSystemParamSetPagingAsync(1, 20, "PARAMETERNAME", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

        }

        void client_GetSystemParamSetPagingCompleted(object sender, GetSystemParamSetPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    T_HR_SYSTEMSETTING temp = e.Result.FirstOrDefault();
                    temp.PARAMETERVALUE = newpwd.Password;
                    temp.PARAMETERVALUE = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(newpwd.Password);
                    client.SystemParamSetUpdateAsync(temp);
                }
                else
                {
                    tbmsg.Text = Utility.GetResourceStr("OLDPWDERROR");
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OLDPWDERROR"));
                    loadbar.Stop();
                }

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
        }

        void client_SystemParamSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                tbmsg.Text = Utility.GetResourceStr("UPDATESUCCESSED", "SALARYPASSWORDSET");
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYPASSWORDSET"));
            }
            loadbar.Stop();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_AREADIFFERENCE", true);
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            EditTitle("SALARY", "SALARYPASSWORDSET");
        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        private void btPwdEdit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckData())
            {
                EditData();
            }
        }

        private void EditTitle(string parentTitle, string newTitle)
        {
            System.Text.StringBuilder sbtitle = new System.Text.StringBuilder();
            sbtitle.Append(Utility.GetResourceStr(parentTitle));
            sbtitle.Append(">>");
            sbtitle.Append(Utility.GetResourceStr(newTitle));
            ViewTitles.TextTitle.Text = sbtitle.ToString();
        }

        private bool CheckData()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                return false;
            }
            if (string.IsNullOrEmpty(newpwd.Password))
            {
                tbmsg.Text = Utility.GetResourceStr("PASSWORDNOTNULL");
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PASSWORDNOTNULL"));
                return false;
            }
            if (newpwd.Password != renewpwd.Password)
            {
                tbmsg.Text = Utility.GetResourceStr("NEWOLDDIFFERENT");
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NEWOLDDIFFERENT"));
                return false;
            }
            tbmsg.Text = "";
            return true;
        }

    }
}
