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
using SMT.FB.UI.Common;
using SMT.FB.UI.FBCommonWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.Validator;

namespace SMT.FB.UI.Form
{
    public partial class Test : ChildWindow
    {
        public Test()
        {
            InitializeComponent();
            //Order = new ApplyOrder();
            //CurAuditInfo = new AuditInfo();


            //CurAuditInfo.CreateCompanyID = "SMT";
            //CurAuditInfo.CreateDepartmentID = "Develeper";
            //CurAuditInfo.CreatePostID = "Manage";
            //CurAuditInfo.CreateUserID = "001";
            //CurAuditInfo.CreateUserName = "001";
            //CurAuditInfo.EditUserID = "001";
            //CurAuditInfo.EditUserName = "001";

            //CurAuditInfo.FormID = "0b658967-9cf4-4049-aae5-3efcaa83e014";
            //CurAuditInfo.ModelCode = "SMTTestFlow";
            

            //InitAuditControlontrol();
        }
        public ApplyOrder Order { get; set; }
        public AuditInfo CurAuditInfo { get; set; }

        private void InitAuditControlontrol()
        {
            //// 提交前，需要对AuditEntity赋值 ，以下属性必填

            //auditControl.AuditEntity.CreateCompanyID = CurAuditInfo.CreateCompanyID;
            //auditControl.AuditEntity.CreateDepartmentID = CurAuditInfo.CreateDepartmentID;
            //auditControl.AuditEntity.CreatePostID = CurAuditInfo.CreatePostID;
            //auditControl.AuditEntity.CreateUserID = CurAuditInfo.CreateUserID;
            //auditControl.AuditEntity.CreateUserName = CurAuditInfo.CreateUserName;
            //auditControl.AuditEntity.EditUserID = CurAuditInfo.EditUserID;
            //auditControl.AuditEntity.EditUserName = CurAuditInfo.EditUserName;

            //auditControl.AuditEntity.ModelCode = CurAuditInfo.ModelCode;
            //auditControl.AuditEntity.FormID = CurAuditInfo.FormID;

            //auditControl.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(auditControl_AuditCompleted);
        }

        void auditControl_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (e.Result == AuditEventArgs.AuditResult.Auditing)
            //{
            //    this.Order.EditState = "审核中";
            //}
            //else if (e.Result == AuditEventArgs.AuditResult.Successful)
            //{
            //    this.Order.EditState = "审核通过";
            //}
            //else if (e.Result == AuditEventArgs.AuditResult.Fail)
            //{
            //    this.Order.EditState = "审核不通过";

            //}
        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            //auditControl.Submit();

        }

        public class AuditInfo
        {
            public string CreateCompanyID { get; set; }
            public string CreateDepartmentID { get; set; }
            public string CreatePostID { get; set; }
            public string CreateUserID { get; set; }
            public string CreateUserName { get; set; }
            public string EditUserID { get; set; }
            public string EditUserName { get; set; }
            
            public string ModelCode { get; set; }
            public string FormID { get; set; }
        }

        public class ApplyOrder
        {
            public string EditState { get; set; }
        }

        private void btnBindingData_Click(object sender, RoutedEventArgs e)
        {

           // auditControl.BindingData();
        }
    }
}

