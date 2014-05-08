using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices;

namespace SMT.SaaS.OA.Services
{
    public partial class TestEngineUpdateCheckState : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Test();
            if (!IsPostBack)
            {
                txtSystemcode.Text = "OA";
                txtEntityType.Text = "T_OA_BUSINESSTRIP";
                txtEntityKey.Text = "BUSINESSTRIPID";
                txtEntityId.Text = "aaa23ae1-ce1f-4ba7-a1d9-73c40da61372";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {

            string strMessage = string.Empty;
            string strXmlParams = string.Empty;
            Utility.UpdateFormCheckState("OA", "T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", txtEntityId.Text, SMT.SaaS.BLLCommonServices.CheckStates.Approving, ref strMessage,strXmlParams);
            txtMessage.Text = strMessage;
        }

        protected void btnSyncAttend_Click(object sender, EventArgs e)
        {
            string strCompanyID = string.Empty, strMsg = string.Empty;
            DateTime dtTravle = new DateTime();
            ltlMsg.Text = string.Empty;
            strCompanyID = txtCompanyID.Text.Trim();
            DateTime.TryParse(txtTravelMonth.Text + "-1", out dtTravle);
            SMT.SaaS.OA.BLL.TravelmanagementBLL bllTravel = new BLL.TravelmanagementBLL();
            bllTravel.SyncAttenceRecordForCompany(strCompanyID, dtTravle, ref strMsg);
            ltlMsg.Text = strMsg;
        }

        private void Test()
        {
            SMT.SaaS.OA.Services.SmtOAPersonOffice sopo = new SmtOAPersonOffice();
            SMT_OA_EFModel.T_OA_BUSINESSTRIP tob = new SMT_OA_EFModel.T_OA_BUSINESSTRIP();
            tob = sopo.GetTravelmanagementById("c4d642b3-f02e-48cd-8e43-7460cb775ebb");
            tob.BUSINESSTRIPID = System.Guid.NewGuid().ToString();

            SMT_OA_EFModel.T_OA_BUSINESSTRIPDETAIL bd = new SMT_OA_EFModel.T_OA_BUSINESSTRIPDETAIL();
            bd.BUSINESSTRIPDETAILID = System.Guid.NewGuid().ToString();
            bd.T_OA_BUSINESSTRIP = tob;
            bd.T_OA_BUSINESSTRIP.BUSINESSTRIPID = tob.BUSINESSTRIPID;
            bd.DEPCITY = "0";
            bd.DESTCITY = "7";
            tob.T_OA_BUSINESSTRIPDETAIL.Add(bd);
            List<SMT_OA_EFModel.T_OA_BUSINESSTRIPDETAIL> bdlist = new List<SMT_OA_EFModel.T_OA_BUSINESSTRIPDETAIL>();
            //bdlist.Add(bd);
            
            sopo.TravelmanagementAdd(tob,bdlist);
        }

        protected void btnGetRefreshDocData_Click(object sender, EventArgs e)
        {
            SmtOACommonAdmin sv = new SmtOACommonAdmin();
            SmtOAPersonOffice office = new SmtOAPersonOffice();
            List<string>  ddddd = office.GetApprovalTypeByCompanyid("703dfb3c-d3dc-4b1d-9bf0-3507ba01b716");
            //List<string> ddddd = office.GetApprovalTypeByCompanyid("cafdca8a-c630-4475-a65d-490d052dca36");
            bool needreFreshData = false;
            int pagecont=0,contdata = 0;
            //List<string> postid = new List<string>() { "d798ead2-559b-488c-ae76-b7640afff8e7" };
            //List<string> departmentid = new List<string>() { "c18d20e3-9f94-4b4c-ac24-b8256132e7d2" };
            List<string> postid = new List<string>() ;
            List<string> departmentid = new List<string>();
            List<string> companyid = new List<string>() { "bac05c76-0f5b-40ae-b73b-8be541ed35ed" };
            var q = sv.RefreshSendDocData("ca356aeb-ea37-41a4-a09d-0d4491b6acf2", 1, 15, ref pagecont, ref contdata,
                postid, departmentid, companyid, ref needreFreshData, true, string.Empty, null, "2013年");
            if (q == null)
            {
                Tracer.Debug("获取到公文为空");
            }
            else
            {
                Tracer.Debug("获取到公文数：" + q.Count());
            }
        }
    }
}