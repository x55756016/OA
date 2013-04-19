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
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

namespace SMT.SaaS.OA.UI.Views.Bumf
{
    public partial class AuditBumf : ChildWindow
    {
        public AuditBumf(V_BumfCompanySendDoc bumfObj)
        {
            InitializeComponent();
            GetSendDocDetailInfo(bumfObj);
        }

        private void GetSendDocDetailInfo(V_BumfCompanySendDoc obj)
        {
            this.tblTitle.Text = "审核公司发文详情";
            this.tbltitle.Text = obj.OACompanySendDoc.SENDDOCTITLE;
            this.tblsend.Text = obj.OACompanySendDoc.SEND;
            this.tblcopy.Text = obj.OACompanySendDoc.CC;
            //this.tblcontent.Text = HttpUtility.HtmlDecode(obj.OACompanySendDoc.CONTENT);
            this.tbldepartment.Text = obj.OACompanySendDoc.DEPARTID;
            //this.tbldoctype.Text = obj.OACompanySendDoc.SENDDOCTYPE;
            //this.tblprioritity.Text = obj.OACompanySendDoc.PRIORITIES;
            //this.tblgrade.Text = obj.OACompanySendDoc.GRADED;
            this.tbladddate.Text = System.Convert.ToDateTime(obj.OACompanySendDoc.CREATEDATE).ToShortDateString();
            this.tblupdatedate.Text = obj.OACompanySendDoc.UPDATEDATE.ToString();
            string StrState = "待审核";
            string StrSave = "";
            string Strdistrbute = "";
            
            switch (obj.OACompanySendDoc.ISSAVE)
            {
                case "0":
                    StrSave = "未归档";
                    break;
                case "1":
                    StrSave = "已归档";
                    break;
            }
            switch (obj.OACompanySendDoc.ISDISTRIBUTE)
            {
                case "0":
                    Strdistrbute = "未发布";
                    break;
                case "1":
                    Strdistrbute = "已发布";
                    break;
            }
            this.tbldistrbute.Text =Strdistrbute;
            this.tblsave.Text = StrSave;
            this.tblcheckstate.Text = StrState;

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

