using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    /// <summary>
    /// 员工异动信息统计表
    /// </summary>
    public class V_EmployeeChangeInfos
    {
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
        /// 异动类型
        /// </summary>
        public string POSTCHANGCATEGORY { get; set; }
        /// <summary>
        /// 异动前工资级别
        /// </summary>
        public string INTFROMSALARYLEVEL { get; set; }
        public string FROMSALARYLEVEL { get; set; }
        /// <summary>
        /// 工资级别
        /// </summary>
        public string INTTOSALARYLEVEL { get; set; }
        public string TOSALARYLEVEL { get; set; }
        /// <summary>
        /// 异动前公司
        /// </summary>
        public string FROMCOMPANY { get; set; }
        /// <summary>
        /// 异动后公司
        /// </summary>
        public string TOCOMPANY { get; set; }
        /// <summary>
        /// 异动前部门
        /// </summary>
        public string FROMDEPARTMENT { get; set; }
        /// <summary>
        /// 异动后部门
        /// </summary>
        public string TODEPARTMENT { get; set; }
        /// <summary>
        /// 异动前岗位
        /// </summary>
        public string FROMPOST { get; set; }
        /// <summary>
        /// 异动后岗位
        /// </summary>
        public string TOPOST { get; set; }
        /// <summary>
        /// 异动日期
        /// </summary>
        public DateTime? CHANGEDATE { get; set; }
        public string STRCHANGEDATE { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime? CREATEDATE { get; set; }
        public string CREATEUSERID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string OWNERID { get; set; }
        public string CHECKSTATE { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }

    }
}
