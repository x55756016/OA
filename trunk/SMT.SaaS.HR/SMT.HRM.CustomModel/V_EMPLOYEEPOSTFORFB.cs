using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 预算所需员工岗位
    /// </summary>
    public class V_EMPLOYEEPOSTFORFB
    {
        public string PERSONBUDGETAPPLYDETAILID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        /// <summary>
        /// 员工状态 0：试用期  1：在职  2：离职 3：离职中  4：未入职 10：异动中 11：异动过 12：岗位异常
        /// </summary>
        public string EMPLOYEESTATE { get; set; }
        /// <summary>
        /// 是否代理:0非代理,1代理岗位
        /// </summary>
        public string ISAGENCY { get; set; }
        public string NEWCOMPANYID { get; set; }
        public string NEWCOMPANYNAME { get; set; }
        public string NEWDEPARTMENTID { get; set; }
        public string NEWDEPARTMENTNAME { get; set; }
        public string NEWPOSTID { get; set; }
        public string NEWPOSTNAME { get; set; }
        public decimal? SUM { get; set; }
    }
}
