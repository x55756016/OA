using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class EmployeeContactWays
    {
        public string EMPLOYEEID { get; set; }
        public string EMPLOYEENAME { get; set; }
        public string EMPLOYEEENAME { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string SEX { get; set; }
        public byte[] PHOTO { get; set; }
        public string TELPHONE { get; set; }//手机
        public string RTX { get; set; }
        public string MAILADDRESS { get; set; }//邮箱
        public string OFFICEPHONE { get; set; }//办公电话
        public string URGENCYPERSON { get; set; }//紧急联系人
        public string URGENCYCONTACT { get; set; }//紧急联系电话
        public string CURRENTADDRESS { get; set; }//当前住址
        public string EMPLOYEESTATE { get; set; }//员工状态：0试用 1在职 2已离职 3离职中

        public string COMPANYID { get; set; }
        public string DEPARTMENTID { get; set; }
        public string POSTID { get; set; }
        public string COMPANYNAME { get; set; }
        public string DEPARTMENTNAME { get; set; }
        public string POSTNAME { get; set; }
    }
}
