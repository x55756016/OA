using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
    }
}