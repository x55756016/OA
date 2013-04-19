using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEEENTRY
    {
        //public T_HR_EMPLOYEEENTRY T_HR_EMPLOYEEENTRY { get; set; }
        //public T_HR_EMPLOYEE T_HR_EMPLOYEE { get; set; }
        public string EMPLOYEEENTRYID { get; set; }
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string EMPLOYEECNAME { get; set; }
        public string IDNUMBER { get; set; }
        public DateTime? ENTRYDATE { get; set; }
        public DateTime? ONPOSTDATE { get; set; }
        public string CHECKSTATE { get; set; }
        public string CREATEUSERID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
    }
}
