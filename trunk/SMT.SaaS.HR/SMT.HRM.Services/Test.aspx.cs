using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.HRM.BLL;
using System.Xml.Linq;
using SMT_HRM_EFModel;

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

            //EngineService eg = new EngineService();
            //string xml = System.IO.File.ReadAllText("c:/HRXML.txt");
            //XElement.Parse(xml);
            //eg.CallWaitAppService(xml);

            using (ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL())
            {
                EmployeeBLL bll = new EmployeeBLL();
                string strMsg = string.Empty;
                List<T_HR_EMPLOYEECLOCKINRECORD> entTempList = new List<T_HR_EMPLOYEECLOCKINRECORD>();
               
                T_HR_EMPLOYEE emp = bll.GetEmployeeByName("曹利宁");

                DateTime dt= new DateTime(2013, 7, 4);
                DateTime dtStar = dt;
                DateTime dtEnd = dt.AddDays(1).AddSeconds(-1);

                AttendanceRecordBLL attbll = new AttendanceRecordBLL();
               
                string smtmsg = attbll.CompulsoryInitialization("4", emp.EMPLOYEEID, dtStar, dtEnd);
                

                //T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                //entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                //entTemp.FINGERPRINTID = idwEnrollNumber.ToString();
                //entTemp.CLOCKID = idwTMachineNumber.ToString();
                //entTemp.PUNCHDATE = DateTime.Parse(dtCurrent.ToString("yyyy-MM-dd") + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":00");
                //entTemp.PUNCHTIME = idwHour.ToString() + ":" + idwMinute.ToString();
                T_HR_EMPLOYEECLOCKINRECORD record = new T_HR_EMPLOYEECLOCKINRECORD();
                record.CLOCKINRECORDID = System.Guid.NewGuid().ToString();
                record.FINGERPRINTID = emp.FINGERPRINTID;
                record.PUNCHDATE = new DateTime(2013,7,4);
                record.PUNCHTIME = "08:17";
                entTempList.Add(record);

                T_HR_EMPLOYEECLOCKINRECORD record2 = new T_HR_EMPLOYEECLOCKINRECORD();
                record2.CLOCKINRECORDID = System.Guid.NewGuid().ToString();
                record2.FINGERPRINTID = emp.FINGERPRINTID;
                record2.PUNCHDATE = new DateTime(2013, 7, 4);
                record2.PUNCHTIME = "17:38";
                entTempList.Add(record2);

                bllClockInRecord.ImportClockInRdListByWindowsService("", entTempList, dtStar
                    , dtEnd, "", ref strMsg);
            }
        }
    }
}