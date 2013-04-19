using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工离职统计信息
    /// </summary>
    public class V_EmployeeLeftOfficeInfos
    {
        /// <summary>
        /// 离职确认ID
        /// </summary>
        public string CONFIRMID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 由总公司名称  没有则填写神州通集团
        /// </summary>
        public string ORGANIZENAME { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string COMPANYNAME { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DEPARTMENTNAME { get; set; }
        /// <summary>
        /// 员工名
        /// </summary>
        public string EMPLOYEECNAME { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string SEX { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDNUMBER { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string POSTNAME { get; set; }
        /// <summary>
        /// 离职类型
        /// </summary>
        public string LEFTOFFICECATEGORY { get; set; }
        /// <summary>
        /// 离职原因
        /// </summary>
        public string LEFTOFFICEREASON { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        /// <summary>
        /// 离职时间
        /// </summary>
        public DateTime? LEFTOFFICEDATE { get; set; }
        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime? APPLYDATE { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public string CHECKSTATE { get; set; }
        public string CREATEUSERID { get; set; }
        public string OWNERID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string EMPLOYEEPOSTID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CREATEDATE { get; set; }
        
    }
}
