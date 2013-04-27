using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SMT.HRM.BLL;

namespace SMT.HRM.Services
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //测试请假消除异常
            EmployeeLeaveRecordBLL bll = new EmployeeLeaveRecordBLL();
            bll.UpdateCheckState("T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", "ff8db61b-1835-4f13-9dd3-f99c65f96e70", "2");
        }
    }
}