using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_LEFTOFFICEVIEW
    {
        public string DIMISSIONID { get; set; }
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string EMPLOYEECNAME { get; set; }
        public string LEFTOFFICECATEGORY { get; set; }
        public string LEFTOFFICEREASON { get; set; }
        public string REMARK { get; set; }
        public DateTime? LEFTOFFICEDATE { get; set; }
        public DateTime? APPLYDATE { get; set; }
        public string CHECKSTATE { get; set; }
        public string ISCONFIRMED { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string EMPLOYEEPOSTID { get; set; }
        public string CREATEUSERID { get; set; }
        public string POSTID { get; set; }
    }
}
