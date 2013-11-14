using System;
using SmtOnlinePortalTools.PermissWS;
using SmtOnlinePortalTools.AttWS;
using SmtOnlinePortalTools.HrUpdateWS;
using SmtOnlinePortalTools.FBWS;
using SmtOnlinePortalTools.PersonWS;
using SmtOnlinePortalTools.SalaryWS;
using System.Text;
using System.Security.Cryptography;
namespace SMT.HRM.Services
{
    /// <summary>
    /// HR控制台
    /// </summary>
    public partial class TestDo : System.Web.UI.Page
    {
        private AttendanceServiceClient AttRdSvc = new AttendanceServiceClient();
        private PersonnelServiceClient PersonSvc = new PersonnelServiceClient();
        private SalaryServiceClient SalarySvc = new SalaryServiceClient();
        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                plLogin.Visible = true;
                plManage.Visible = false;

                if (Session["LOGINUSER"] != null)
                {
                    plLogin.Visible = false;
                    plManage.Visible = true;
                }
            }
        }

        private static string GetMd5Hash(System.Security.Cryptography.MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                PermissionServiceClient clPerm = new PermissionServiceClient();
                SmtOnlinePortalTools.PermissWS.T_SYS_USER entUser = clPerm.UserLogin(UserName.Text, GetMd5Hash(MD5.Create(),Password.Text));

                if (entUser == null)
                {
                    ltlMsg.Text = "用户不存在或密码错误，请重新输入。";
                    return;
                }
                //if (UserName.Text != "xianghanyong" && Password.Text != "xianghanyong")
                //{
                //    ltlMsg.Text = "用户不存在或密码错误，请重新输入。";
                //    return;
                //}

                Session["LOGINUSER"] = entUser;
                plLogin.Visible = false;
                plManage.Visible = true;
            }
            catch (Exception ex)
            {
                ltlMsg.Text = "登录异常，请联系管理员。" + ex.ToString(); ;
            }
        }

        /// <summary>
        /// 生成考勤初始化记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCreateAtt_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOrgType = string.Empty, strOrgID = string.Empty, strAttMonth = string.Empty;
            if (string.IsNullOrWhiteSpace(txtOrgType.Text) || string.IsNullOrWhiteSpace(txtOrgID.Text) || string.IsNullOrWhiteSpace(txtCreateAttMonth.Text))
            {
                return;
            }

            strOrgID = txtOrgID.Text;
            strOrgType = txtOrgType.Text;
            strAttMonth = txtCreateAttMonth.Text;

            //待发布
            AttRdSvc.AsignAttendanceSolutionByOrgIDAndMonth(strOrgType, strOrgID, strAttMonth);

            ltlMsg.Text = "考勤初始化完毕";
        }


        /// <summary>
        /// 检查请假及出差情况
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateAttRd_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strCompanyID = string.Empty, strCurMonth = string.Empty;
            if (string.IsNullOrWhiteSpace(txtCompanyID.Text) || string.IsNullOrWhiteSpace(txtCurMonth.Text))
            {
                return;
            }

            strCompanyID = txtCompanyID.Text;
            strCurMonth = txtCurMonth.Text;


            AttRdSvc.UpdateAttendRecordByEvectionAndLeaveRd(strCompanyID, strCurMonth);

            ltlMsg.Text = "检查请假及出差情况完毕";
        }

        /// <summary>
        /// 检查考勤异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCheckAbnormal_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strAbnormalOrgType = string.Empty, strAbnormalOrgId = string.Empty, strPunchFrom = string.Empty, strPunchTo = string.Empty, strMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(txtAbnormalOrgType.Text) || string.IsNullOrWhiteSpace(txtAbnormalOrgId.Text) || string.IsNullOrWhiteSpace(txtPunchFrom.Text) || string.IsNullOrWhiteSpace(txtPunchTo.Text))
            {
                return;
            }

            strAbnormalOrgType = txtAbnormalOrgType.Text;
            strAbnormalOrgId = txtAbnormalOrgId.Text;
            strPunchFrom = txtPunchFrom.Text;
            strPunchTo = txtPunchTo.Text;

            DateTime dtPunchFrom = new DateTime(), dtPunchTo = new DateTime();
            DateTime.TryParse(strPunchFrom, out dtPunchFrom);
            DateTime.TryParse(strPunchTo, out dtPunchTo);

            if (strAbnormalOrgType == "1")
            {

                //待发布
                AttRdSvc.CheckAbnormRdForCompanyByDate(strAbnormalOrgId, dtPunchFrom, dtPunchTo, ref strMsg);
            }
            else if (strAbnormalOrgType == "4")
            {
                //待发布
                AttRdSvc.CheckAbnormRdForEmployeesByDate(strAbnormalOrgId, dtPunchFrom, dtPunchTo, ref strMsg);
            }

            if (!strMsg.Contains("SUCCESS"))
            {
                ltlMsg.Text = strMsg;
                return;
            }

            ltlMsg.Text = "检查考勤异常完毕";
        }

        /// <summary>
        /// 生成&更新带薪假记录(调休除外)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateFreeLeaveRd_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOrgType = string.Empty, strOrgID = string.Empty;
            strOrgID = txtFLOrgID.Text;
            strOrgType = txtFLOrgType.Text;


            AttRdSvc.CalculateEmployeeLevelDayCountByOrgID(strOrgType, strOrgID);

            ltlMsg.Text = "更新带薪假记录完毕";
        }

        /// <summary>
        /// 清理考勤异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClearAtt_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOrgType = string.Empty, strOrgID = string.Empty;
            if (string.IsNullOrWhiteSpace(txtFLOrgType.Text) || string.IsNullOrWhiteSpace(txtFLOrgID.Text))
            {
                return;
            }

            strOrgID = txtOrgID.Text;
            strOrgType = txtOrgType.Text;

            ltlMsg.Text = "清理考勤异常完毕";
        }

        /// <summary>
        /// 更新单据状态，检查单据更新情况
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateRecord_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strTableName = string.Empty, strTableKeyName = string.Empty, strFormID = string.Empty, strCheckStates = string.Empty;
            if (string.IsNullOrWhiteSpace(txtTableName.Text) || string.IsNullOrWhiteSpace(txtTableKeyName.Text) || string.IsNullOrWhiteSpace(txtFormID.Text) || string.IsNullOrWhiteSpace(txtCheckStates.Text))
            {
                return;
            }

            strTableName = txtTableName.Text;
            strTableKeyName = txtTableKeyName.Text;
            strFormID = txtFormID.Text;
            strCheckStates = txtCheckStates.Text;

            HRUpdateCheckStateServicesClient updWS = new HRUpdateCheckStateServicesClient();
            updWS.UpdateCheckState(strTableName, strTableKeyName, strFormID, strCheckStates);
        }

        /// <summary>
        /// 将定时触发考勤初始化记录生成的配置信息存入引擎
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInitAttendRdByEngine_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }
            //待发布
            DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
            T_HR_ATTENDANCESOLUTIONASIGN entTemp = new T_HR_ATTENDANCESOLUTIONASIGN();
            entTemp.STARTDATE = dtStart;
            AttRdSvc.GetAttendSolAsignForOutEngineXml(entTemp);
        }

        /// <summary>
        /// 将定时触发更新带薪假记录的配置信息存入引擎
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInitFreeLeaveDaysByEngine_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            //AttendanceSolutionAsignBLL bllAttRd = new AttendanceSolutionAsignBLL();
            //bllAttRd.GetFreeLeaveDayForOutEngineXml();

            AttRdSvc.CreateLevelDayCountByAsignAttSol("9977C301-62FC-459C-B798-EBF37DE746DE");
        }

        /// <summary>
        /// 下拨活动经费
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnAssignCompany_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAssignCompanyID.Text))
            {
                return;
            }


            using (FBServiceClient clientFB = new FBServiceClient())
            {
                string strAssignOwnerID = System.Configuration.ConfigurationManager.AppSettings["PersonMoneyAssignOwner"];
                clientFB.CreatePersonMoneyAssignInfo(txtAssignCompanyID.Text.Trim(), strAssignOwnerID);
            }
        }

        /// <summary>
        /// 检查在岗人数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGetInserviceCount_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOwnerID = string.Empty, strOrgType = string.Empty, strOrgID = string.Empty, strCheckDate = string.Empty, strMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(txtInserviceOwnerID.Text) || string.IsNullOrWhiteSpace(txtInserviceOrgType.Text) || string.IsNullOrWhiteSpace(txtInserviceOrgID.Text) || string.IsNullOrWhiteSpace(txtInserviceDateTo.Text))
            {
                return;
            }

            DateTime dtCheckDate = new DateTime();

            strOrgType = txtInserviceOrgType.Text;
            strOrgID = txtInserviceOrgID.Text;
            strOwnerID = txtInserviceOwnerID.Text;
            strCheckDate = txtInserviceDateTo.Text;
            DateTime.TryParse(strCheckDate, out dtCheckDate);


            PersonSvc.GetInserviceEmployeeCount(strOwnerID, strOrgType, strOrgID, dtCheckDate, ref strMsg);

            ltlMsg.Text = strMsg;
        }

        /// <summary>
        /// 获取可用的带薪假天数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGetFreeDays_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOwnerID = string.Empty, strLeavetypesetID = string.Empty, strStartDate = string.Empty, strEndDate = string.Empty;
            if (string.IsNullOrWhiteSpace(txtLeaveStartDate.Text) || string.IsNullOrWhiteSpace(txtLeaveEndDate.Text) || string.IsNullOrWhiteSpace(txtLeaveTypeSetID.Text) || string.IsNullOrWhiteSpace(txtLeaveUserID.Text))
            {
                return;
            }

            DateTime dtStartDate = new DateTime();
            DateTime dtEndDate = new DateTime();

            strLeavetypesetID = txtLeaveTypeSetID.Text;
            strOwnerID = txtLeaveUserID.Text;
            strStartDate = txtLeaveStartDate.Text;
            strEndDate = txtLeaveEndDate.Text;
            DateTime.TryParse(strStartDate, out dtStartDate);
            DateTime.TryParse(strEndDate, out dtEndDate);

            decimal dCurLevelDays = 0;

            AttRdSvc.GetCurLevelDaysByEmployeeIDAndLeaveFineType(strOwnerID, string.Empty, strLeavetypesetID, dtStartDate, dtEndDate, ref dCurLevelDays);
            ltlMsg.Text = dCurLevelDays.ToString();
        }

        /// <summary>
        /// 获取实际的请假时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGetLeaveDays_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOwnerID = string.Empty, strLeaveRdID = string.Empty, strStartDate = string.Empty, strEndDate = string.Empty;
            decimal dLeaveDay = 0, dLeaveTime = 0, dLeaveTotalTime = 0;
            if (string.IsNullOrWhiteSpace(txtLeaveFrom.Text) || string.IsNullOrWhiteSpace(txtLeaveTo.Text) || string.IsNullOrWhiteSpace(txtLeaveRdID.Text) || string.IsNullOrWhiteSpace(txtLeaveEmployeeID.Text))
            {
                return;
            }

            DateTime dtStartDate = new DateTime();
            DateTime dtEndDate = new DateTime();

            strLeaveRdID = txtLeaveRdID.Text;
            strOwnerID = txtLeaveEmployeeID.Text;
            strStartDate = txtLeaveFrom.Text;
            strEndDate = txtLeaveTo.Text;
            DateTime.TryParse(strStartDate, out dtStartDate);
            DateTime.TryParse(strEndDate, out dtEndDate);

            AttRdSvc.GetRealLeaveDayByEmployeeIdAndDate(strLeaveRdID, strOwnerID, dtStartDate, dtEndDate, ref dLeaveDay, ref dLeaveTime, ref dLeaveTotalTime);
            litRealLeaveDays.Text = "请假总小时数" + dLeaveTotalTime.ToString();
        }

        /// <summary>
        /// 检查薪资发放提醒是否成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCheckPayment_Click(object sender, EventArgs e)
        {
            if (Session["LOGINUSER"] == null)
            {
                plLogin.Visible = true;
                plManage.Visible = false;
                return;
            }

            ltlMsg.Text = string.Empty;
            string strOwnerID = string.Empty, strOrgType = string.Empty, strOrgID = string.Empty, strCheckDate = string.Empty, strMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(txtPayOrgType.Text) || string.IsNullOrWhiteSpace(txtPayOrgID.Text) || string.IsNullOrWhiteSpace(txtPayMonth.Text))
            {
                ltlMsg.Text = "薪资发放检查所填写的发放月份,机构类型,机构ID都不能为空";
                return;
            }

            DateTime dtCheckDate = new DateTime();

            strOrgType = txtPayOrgType.Text;
            strOrgID = txtPayOrgID.Text;
            strCheckDate = txtPayMonth.Text;
            bool bDate = DateTime.TryParse(strCheckDate + "-1", out dtCheckDate);

            if (!bDate)
            {
                ltlMsg.Text = "薪资发放检查所填写的发放月份格式为：年-月，例：2012-1";
                return;
            }

            //待发布
            SalarySvc.PayRemindByOrgID(strOrgType, strOrgID, dtCheckDate, ref strMsg);

            ltlMsg.Text = strMsg;
        }

        /// <summary>
        /// 检查流程获取上级员工信息是否正常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCheckEmployeeLeader_Click(object sender, EventArgs e)
        {
            ltlMsg.Text = string.Empty;
            string strLeaderType = string.Empty, strLeaderPostID = string.Empty, strMsg = string.Empty;
            int iLeaderType = -1;
            if (string.IsNullOrWhiteSpace(txtLeaderPostID.Text) || string.IsNullOrWhiteSpace(txtLeaderType.Text))
            {
                return;
            }

            strLeaderPostID = txtLeaderPostID.Text;
            int.TryParse(txtLeaderType.Text, out iLeaderType);

            PersonSvc.GetEmployeeLeaders(strLeaderPostID, iLeaderType);
        }

    }
}