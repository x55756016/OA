using System;
using System.Collections.Generic;
using System.Windows.Controls;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class BaseinfoForm : BaseForm,IClient
    {
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        private T_HR_RESUME resume;

        public T_HR_RESUME Resume
        {
            get { return resume; }
            set
            {
                resume = value;
                this.DataContext = value;
            }
        }

        PersonnelServiceClient client = new PersonnelServiceClient();
        public BaseinfoForm()
        {
            InitializeComponent();
            this.Loaded += (sender, agrs) =>
            {
                InitControlEvent();
            };
        }

        public void LoadData(SMT.SaaS.FrameworkUI.FormTypes type, string resumeID)
        {
            formType = type;
            if (formType == FormTypes.Browse)
            {
                this.IsEnabled = false;
            }
            if (formType == SMT.SaaS.FrameworkUI.FormTypes.New)
            {
                if (Resume == null)
                {
                    Resume = new T_HR_RESUME();
                    Resume.RESUMEID = Guid.NewGuid().ToString();
                    Resume.CREATEDATE = DateTime.Now;
                    Resume.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                }
            }
            else
            {
                if (Resume == null)
                {
                    client.GetResumeByidAsync(resumeID);
                }
            }
        }
        private void InitControlEvent()
        {
            //client = new PersonnelServiceClient();
            client.GetResumeByidCompleted += new EventHandler<GetResumeByidCompletedEventArgs>(client_GetResumeByidCompleted);

        }

        void client_GetResumeByidCompleted(object sender, GetResumeByidCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            }
            else
            {
                Resume = e.Result;
                //未绑定的控件赋值
                OtherControlBinder();
            }
        }

        private void OtherControlBinder()
        {
            if (Resume != null)
            {
                //性别
                //if (Resume.SEX == "男")
                //{
                //    rdoSex.IsChecked = true;
                //}
                //else
                //{
                //    rdoSex1.IsChecked = true;
                //}
                //是否亲属
                if (Resume.HAVECOMRELATION == "是")
                {
                    rdoHaveComrelation.IsChecked = true;
                }
                else
                {
                    rdoHaveComrelation1.IsChecked = true;
                }
            }
        }

        public bool FieldValue()
        {
            //if (rdoSex.IsChecked == true)
            //{
            //    Resume.SEX = "男";
            //}
            //else
            //{
            //    Resume.SEX = "女";
            //}
            if (rdoHaveComrelation.IsChecked == true)
            {
                Resume.HAVECOMRELATION = "是";
            }
            else
            {
                Resume.HAVECOMRELATION = "否";
            }
            if (txtName.Text == "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NAMEISNOTNULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NAMEISNOTNULL"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                txtName.Focus();
                return false;
            }
            if (txtIdCardNumber.Text == "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CARDNUMBERNONULL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("CARDNUMBERNONULL"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                txtIdCardNumber.Focus();
                return false;
            }
            return true;
        }

        private void lkCompany_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("COMPANRYCODE", "COMPANRYCODE");
            cols.Add("CNAME", "CNAME");
            cols.Add("ENAME", "ENAME");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Company,
                typeof(T_HR_COMPANY[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_COMPANY ent = lookup.SelectedObj as T_HR_COMPANY;

                if (ent != null)
                {
                    lkCompany.DataContext = ent;
                    //HandleComapnyChanged();
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void lkPost_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("POSTCODE", "T_HR_POSTDICTIONARY.POSTCODE");
            cols.Add("POSTNAME", "T_HR_POSTDICTIONARY.POSTNAME");

            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Post,
                typeof(T_HR_POST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_POST ent = lookup.SelectedObj as T_HR_POST;

                if (ent != null)
                {
                    lkPost.DataContext = ent;

                    //HandlePostChanged(ent);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
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
