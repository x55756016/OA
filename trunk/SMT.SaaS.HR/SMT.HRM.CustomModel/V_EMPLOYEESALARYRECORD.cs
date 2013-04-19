/*
 * 文件名：V_EMPLOYEESALARYRECORD
 * 作  用：薪资记录实体扩展类
 * 创建人： 喻建华
 * 创建时间：2011年1月18日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEESALARYRECORD
    {
        /// <summary>
        /// 薪资记录ID
        /// </summary>
        public string EMPLOYEESALARYRECORDID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 发薪年份
        /// </summary>
        public string SALARYYEAR { get; set; }
        /// <summary>
        /// 发薪月份
        /// </summary>
        public string SALARYMONTH { get; set; }
        /// <summary>
        /// 实发工资
        /// </summary>
        public string ACTUALLYPAY { get; set; }
        /// <summary>
        /// 月度批量结算ID
        /// </summary>
        public string MONTHLYBATCHID{ get; set; }
        /// <summary>
        /// 职级代码
        /// </summary>
        public string POSTLEVELCODE { get; set; }
        /// <summary>
        /// 岗位级别
        /// </summary>
        public decimal? POSTLEVEL { get; set; }
        /// <summary>
        /// 薪资级别
        /// </summary>
        public decimal? SALARYLEVEL { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string DEPARTMENT { get; set; }
        /// <summary>
        /// 岗位
        /// </summary>
        public string POST { get; set; }
        /// <summary>
        /// 发薪总额
        /// </summary>
        public string PAYTOTALMONEY { get; set; }
        /// <summary>
        /// 人均薪水
        /// </summary>
        public string AVGMONEY { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public string CHECKSTATE { get; set; }
        /// <summary>
        /// 发放状态
        /// </summary>
        public string PAYCONFIRM { get; set; }
        /// <summary>
        /// 自己公司ID
        /// </summary>
        public string OWNERCOMPANYID { get; set; }
        /// <summary>
        /// 创建用户ID
        /// </summary>
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 排列序号
        /// </summary>
        public string ORDINAL { get; set; }
        /// <summary>
        /// 薪资记录实体
        /// </summary>
        public T_HR_EMPLOYEESALARYRECORD T_HR_EMPLOYEESALARYRECORD { get; set; }
    }
}
