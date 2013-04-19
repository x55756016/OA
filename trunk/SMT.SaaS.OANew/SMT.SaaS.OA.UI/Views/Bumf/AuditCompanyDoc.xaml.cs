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
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

using System.Windows.Browser;
using SMT.Saas.Tools.FlowWFService;
using SMT.SaaS.FrameworkUI.AuditControl;

namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class AuditCompanyDoc : ChildWindow
    {

         #region 初始化变量

        private T_OA_SENDDOC tmpSendDocT = new T_OA_SENDDOC();
        public SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T AuditEntity;
        public event EventHandler<AuditEventArgs> AfterAudit;
        //public SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult aa = new AuditEventArgs.AuditResult();
        private string StrCompanyID = "1";
        private string StrDepartmentID = "1";
        private string StrPositionID = "1";
        private string StrUserID = "1";
        private string StrMainUserID = "1";//主送人
        private string StrCopyUserID = "1";//抄送人ID
        
        private string tmpBtnName = "";  //按钮名
        
        
        private string tempStrSendDocName = "";//发文标题
        private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();

        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        
        #endregion

        

        #region 构造函数


        

        public AuditCompanyDoc(string operationType, T_OA_SENDDOC TypeObj)
        {
            InitializeComponent();
            
            if (operationType == "Add")
            {
                
                this.tblTitle.Text = "审核公司发文信息";
                this.ChildWindow.Title = "审核公司发文信息";
            }
            else
            {                
                this.tblTitle.Text = "审核公文发文信息";
                this.ChildWindow.Title = "审核公司发文信息";
                
            }
            AuditC.AuditEntity.CreateCompanyID = this.StrCompanyID;
            AuditC.AuditEntity.ModelCode = "CompanyDoc";
            AuditC.AuditEntity.CreatePostID = this.StrPositionID;
            AuditC.AuditEntity.CreateUserID = this.StrUserID;
            AuditC.AuditEntity.EditUserID = this.StrUserID;
            AuditC.AuditEntity.FormID = TypeObj.SENDDOCID;

            //AuditC.AfterAudit += new EventHandler<AuditEventArgs>(AuditControl_AfterAudit);
            GetSendDocInfo(TypeObj);
            

           
        }

        private void GetSendDocInfo(T_OA_SENDDOC SendDocObj)
        {
            if (SendDocObj != null)
            {
                tmpSendDocT = SendDocObj;
                this.tblDocTitle.Text = SendDocObj.SENDDOCTITLE;
                this.tblNumber.Text = SendDocObj.NUM;
                //this.tblDocContent.Text = HttpUtility.HtmlDecode(SendDocObj.CONTENT);
                this.tbldepartment.Text = SendDocObj.DEPARTID;
                //this.tblgrade.Text = SendDocObj.GRADED;
                //this.tblproritity.Text = SendDocObj.PRIORITIES;
                //this.tblDocType.Text = SendDocObj.SENDDOCTYPE;
                this.tblsend.Text = SendDocObj.SEND;
                this.tblcopy.Text = SendDocObj.CC;
                this.tblAddDate.Text = System.Convert.ToDateTime(SendDocObj.CREATEDATE).ToShortDateString() + " " + System.Convert.ToDateTime(SendDocObj.CREATEDATE).ToShortTimeString();
                this.tblAdduser.Text = SendDocObj.CREATEUSERID.ToString();//暂时定为ID

            }
         }
        #endregion

        


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            AuditC.AuditEntity.CreateCompanyID = this.StrCompanyID;
            AuditC.AuditEntity.ModelCode = "CompanyDoc";
            AuditC.AuditEntity.CreatePostID = this.StrPositionID;
            AuditC.AuditEntity.CreateUserID = this.StrUserID;
            AuditC.AuditEntity.EditUserID = this.StrUserID;
            AuditC.AuditEntity.FormID = this.tmpSendDocT.SENDDOCID;

            //AuditC.AfterAudit += new EventHandler<AuditEventArgs>(AuditControl_AfterAudit);
            //AuditC.Submit();//提交审核


        }



       
        void AuditControl_AfterAudit(object sender, AuditEventArgs e)
        {
            if (e.Result == SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing)
            {
                //this.Order.States = "审核中";
                MessageBox.Show("审核通过了");
                return;
            }
            else if (e.Result == SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful)
            {
                //this.Order.States = "审核通过";
            }
            else if (e.Result == SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail)
            {
                //this.Order.States = "审核不通过";
            }
        }
	



    }
}

