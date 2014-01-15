using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工异常签卡申请
    /// </summary>
   public class V_EMPLOYEESIGNINRECORD
    {
       public string SIGNINID { get; set; }
       public string EMPLOYEEID { get; set; }
       public string EMPLOYEENAME { get; set; }
       public string EMPLOYEECODE { get; set; }
       public DateTime? SIGNINTIME { get; set; }
       public string CHECKSTATE { get; set; }
       public string SIGNINCATEGORY { get; set; }
       public string REMARK { get; set; }
       public string OWNERID { get; set; }
       public string OWNERPOSTID { get; set; }
       public string OWNERDEPARTMENTID { get; set; }
       public string OWNERCOMPANYID { get; set; }
       public string CREATEPOSTID { get; set; }
       public string CREATEDEPARTMENTID { get; set; }
       public string CREATECOMPANYID { get; set; }
       public string CREATEUSERID { get; set; }
       public DateTime? CREATEDATE { get; set; }
       public string DEPTNAME { get; set; }
    }
}
