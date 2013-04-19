using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel.Reports
{
    public  class AbnormalAttendanceeEntity
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string cname { get; set; }

        /// <summary>
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }

        /// <summary>
        /// 异常类型
        /// </summary>
        public string ABNORMCATEGORY { get; set; }

        /// <summary>
        /// 异常时长
        /// </summary>
        public decimal? ABNORMALTIME { get; set; }

        /// <summary>
        /// 迟到/早退总次数
        /// </summary>
        public int outTimes { get; set; }

        /// <summary>
        /// 迟到/早退合计（分钟)
        /// </summary>
        public decimal? outMinutes { get; set; }

        /// <summary>
        /// 漏打卡次数
        /// </summary>
        public int DrainTimeNumber { get; set; }

        /// <summary>
        /// 超额工时合计（小时)
        /// </summary>
        public int ExcessHoursTotal { get; set; }

        /// <summary>
        /// 可调休假（小时）
        /// </summary>
        public decimal? AdjustableVacation { get; set; }

        /// <summary>
        /// 事假（小时）
        /// </summary>
        public decimal? LeaveHour { get; set; }

        /// <summary>
        /// 年休假（小时）
        /// </summary>
        public decimal? AnnualLeave { get; set; }

        /// <summary>
        /// 病假（小时）
        /// </summary>
        public decimal? SickLeave { get; set; }

        /// <summary>
        /// 调休假（小时）
        /// </summary>
        public decimal? OffHour { get; set; }

        /// <summary>
        /// 请假类型
        /// </summary>
        public string LeaverecordStyple { get; set; }

        /// <summary>
        /// 请假时长
        /// </summary>
        public decimal? LeaverecordTime { get; set; }

        /// <summary>
        /// 可修假天数
        /// </summary>
        public decimal? AdjustableDay { get; set; }

        /// <summary>
        /// 打卡时间
        /// </summary>
        public DateTime? Punchdate { get; set; }
    }
}
