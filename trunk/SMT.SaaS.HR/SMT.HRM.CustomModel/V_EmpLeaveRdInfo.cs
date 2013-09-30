using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EmpLeaveRdInfo
    {
        public string CHECKSTATE { get; set; }

        public string CREATECOMPANYID { get; set; }

        public DateTime? CREATEDATE { get; set; }

        public string CREATEDEPARTMENTID { get; set; }

        public string CREATEPOSTID { get; set; }

        public string CREATEUSERID { get; set; }

        public string EMPLOYEECODE { get; set; }

        public string EMPLOYEEID { get; set; }

        public string EMPLOYEENAME { get; set; }
        
        public decimal? LEAVEDAYS { get; set; }

        public decimal? LEAVEHOURS { get; set; }

        public string LEAVERECORDID { get; set; }

        public string OWNERCOMPANYID { get; set; }

        public string OWNERDEPARTMENTID { get; set; }

        public string OWNERID { get; set; }

        public string OWNERPOSTID { get; set; }

        public string REASON { get; set; }

        public string REMARK { get; set; }

        public DateTime? STARTDATETIME { get; set; }
        
        public DateTime? ENDDATETIME { get; set; }

        public string LEAVETYPENAME { get; set; }

        public decimal? TOTALHOURS { get; set; }

        public DateTime? UPDATEDATE { get; set; }

        /// <summary>
        /// 销假总时长
        /// </summary>
        public decimal?  CANCELTOTALHOURS { get; set; }
    }
}
