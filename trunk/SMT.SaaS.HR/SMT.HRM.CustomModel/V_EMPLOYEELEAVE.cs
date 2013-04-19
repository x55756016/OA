using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEELEAVE
    {
        public V_EMPLOYEELEAVE() { }
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
        /// 假期名称
        /// </summary>
        public string LeaveTypeName { set; get; }
        /// <summary>
        /// 假期类型ID
        /// </summary>
        public string LeaveTypeSetID { set; get; }
        /// <summary>
        /// 最多请假天数
        /// </summary>
        public decimal MaxDays { set; get; }
        /// <summary>
        /// 扣款方式
        /// </summary>
        public string FineType { set; get; }
        /// <summary>
        /// 是否扣全勤
        /// </summary>
        public string IsPerfectAttendanceFactor { set; get; }
        /// <summary>
        /// 休假天数
        /// </summary>
        public decimal LeaveDays { set; get; }
        /// <summary>
        /// 已休假天数
        /// </summary>
        public decimal UsedLeaveDays { set; get; }
        /// <summary>
        /// 可休假天数
        /// </summary>
        public decimal UseableLeaveDays { set; get; }

    }
}
