/*
 * 文件名：V_PAYMENT.cs
 * 作  用：薪资发放实体扩展类
 * 创建人： 喻建华
 * 创建时间：2010年3月17日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
   public class V_PAYMENT
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
       /// 员工编号
       /// </summary>
       public string EMPLOYEECODE { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
       public string EMPLOYEENAME { get; set; }
        /// <summary>
        /// 员工开户银行ID
        /// </summary>
        public string BLANKID { get; set; }
        /// <summary>
        /// 银行帐号
        /// </summary>
        public string BANKCARDNUMBER { get; set; }
        /// <summary>
        /// 考勤异常扣款
        /// </summary>
        public decimal? ATTENDANCEUNUSUALDEDUCT { get; set; }
        /// <summary>
        /// 实发工资
        /// </summary>
        public string ACTUALLYPAY { get; set; }
        /// <summary>
        /// 基本工资
        /// </summary>
        public decimal? BASICSALARY { get; set; }
        /// <summary>
        /// 绩效奖金额
        /// </summary>
        public decimal? PERFORMANCESUM { get; set; }
        /// <summary>
        /// 发薪银行
        /// </summary>
        public string BANKNAME { get; set; }
        /// <summary>
        /// 发薪银行帐号
        /// </summary>
        public string BANKACCOUNTNO { get; set; }
        /// <summary>
        /// 发薪方式
        /// </summary>
        public string PAYTYPE { get; set; }
        /// <summary>
        /// 薪资记录创建时间
        /// </summary>
        public DateTime? RECORDCREATEDATE { get; set; }
        /// <summary>
        /// 薪资档案创建时间
        /// </summary>
        public DateTime? ARCHIVECREATEDATE { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public string CHECKSTATE { get; set; }
        /// <summary>
        /// 发放状态
        /// </summary>
        public string PAYCONFIRM { get; set; }
        /// <summary>
        /// 发薪日期
        /// </summary>
        public DateTime? PAIDDATE { get; set; }
        /// <summary>
        /// 发薪经手人
        /// </summary>
        public string PAIDBY { get; set; }
        /// <summary>
        /// 所属年份
        /// </summary>
        public string SALARYYEAR { get; set; }
        /// <summary>
        /// 所属月份
        /// </summary>
        public string SALARYMONTH { get; set; }
    }
}
