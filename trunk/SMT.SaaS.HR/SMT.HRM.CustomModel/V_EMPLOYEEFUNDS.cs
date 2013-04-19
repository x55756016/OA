using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEEFUNDS
    {
        public string EMPLOYEEID { get; set; }
        public string EMPLOYECNAME { get; set; }
        /// <summary>
        /// 员工状态 0：试用期  1：在职  2：离职 3：离职中  4：未入职
        /// </summary>
        public string EMPLOYEESTATE { get; set; }
        /// <summary>
        /// 是否代理:0非代理,1代理岗位
        /// </summary>
        public string ISAGENCY { get; set; }
        public string COMPANYID { get; set; }
        public string COMPANYNAME { get; set; }
        public string DEPARTMENTID { get; set; }
        public string DEPARTMENTNAME { get; set; }
        public string POSTID { get; set; }
        public string POSTNAME { get; set; }
        public decimal? NEEDSUM { get; set; }
        public decimal? REALSUM { get; set; }
        public decimal? NEEDATTENDDAYS { get; set; }
        public decimal? REALATTENDDAYS { get; set; }
        public string ATTENDREMARK { get; set; }
    }
}
