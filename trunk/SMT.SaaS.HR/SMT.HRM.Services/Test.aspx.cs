using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.HRM.BLL;
using System.Xml.Linq;

namespace SMT.HRM.Services
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            //测试请假消除异常
            //EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL();

            //bll.updateAllLeve();
            //return;

            //bll.UpdateCheckState("T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", "68042611-70b3-45ca-a530-26eaaf28ea94", "2");

            //EmployeePostChangeBLL bll = new EmployeePostChangeBLL();
            //bll.UpdateCheckState("T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGEID", "33550c08-aa08-47e5-ad65-9db98faf5375", "1");

            EngineService eg = new EngineService();
            string xml = System.IO.File.ReadAllText("c:/HRXML.txt");
            XElement.Parse(xml);
            eg.CallWaitAppService(xml);
        }
    }
}