using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEEPOSTCHANGE
    {
        public SMT_HRM_EFModel.T_HR_EMPLOYEEPOSTCHANGE EMPLOYEEPOSTCHANGE { get; set; }
        public string FROMCOMPANY { get; set; }
        public string TOCOMPANY { get; set; }
        public string FROMDEPARTMENT { get; set; }
        public string TODEPARTMENT { get; set; }
        public string FROMPOST { get; set; }
        public string TOPOST { get; set; }
        public DateTime? ENDDATE { get; set; }
        public string CREATEUSERID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERID { get; set; }
    }
}
