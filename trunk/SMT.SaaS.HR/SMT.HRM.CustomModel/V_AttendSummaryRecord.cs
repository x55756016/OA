using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_AttendSummaryRecord
    {
        public V_AttendSummaryRecord() { }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { set; get; }
        /// <summary>
        /// 员工编号
        /// </summary>
        public string EmployeeCode { set; get; }
        /// <summary>
        /// 员工名称
        /// </summary>
        public string EmployeeName { set; get; }
        /// <summary>
        /// 考勤假期名称
        /// </summary>
        public string AttendLeaveName { set; get; }
        /// <summary>
        /// 上年度未休调休天数
        /// </summary>
        public string LastUseLevelDayCount { set; get; }
        /// <summary>
        /// 上年度未休的年假天数
        /// </summary>
        public string LastUseYearDays { set; get; }
        /// <summary>
        /// 本年度的年假天数
        /// </summary>
        public string YearDays { set; get; }
        /// <summary>
        /// 本年度的调休天数
        /// </summary>
        public string LeavelDayCount { set; get; }
        /// <summary>
        /// 本年度已调休天数
        /// </summary>
        public string UsedLeavelDays { set; get; }
        /// <summary>
        /// 本年度可用调休天数
        /// </summary>
        public string UseLeavelDay { set; get; }
        /// <summary>
        /// 本年度已用的年假天数
        /// </summary>
        public string UsedYearDays { set; get; }
        /// <summary>
        /// 本年度可用的年假天数
        /// </summary>
        public string UseYearDays { set; get; }
    }
}
