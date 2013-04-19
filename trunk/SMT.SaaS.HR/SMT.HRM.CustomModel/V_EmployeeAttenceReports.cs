using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_EmployeeAttenceReports
    {
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
        /// 员工ID
        /// </summary>
        public string EMPLOYEEID { get; set; }        
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDNUMBER { get; set; }
        /// <summary>
        /// 入职时间
        /// </summary>
        public DateTime? ENTRYDATE { get; set; }
        /// <summary>
        /// 当月应出勤天数
        /// </summary>
        public decimal? NEEDATTENDDAYS { get; set; }
        /// <summary>
        /// 实际出勤天数
        /// </summary>
        public decimal? REALATTENDDAYS { get; set; }
        /// <summary>
        /// 出勤率
        /// </summary>
        public decimal? ATTENDANCERATE { get; set; }
        /// <summary>
        /// 加班天数
        /// </summary>
        public decimal? OVERTIMESUMDAYS { get; set; }
        /// <summary>
        /// 调休天数
        /// </summary>
        public decimal? LEAVEUSEDDAYS { get; set; }
        /// <summary>
        /// 漏打卡次数
        /// </summary>
        public decimal? FORGETCARDTIMES { get; set; }
        /// <summary>
        /// 迟到次数
        /// </summary>
        public decimal? LATETIMES { get; set; }
        /// <summary>
        /// 早退次数
        /// </summary>
        public decimal? LEAVEEARLYTIMES { get; set; }
        /// <summary>
        /// 旷工天数
        /// </summary>
        public decimal? ABSENTDAYS { get; set; }
        /// <summary>
        /// 事假天数
        /// </summary>
        public decimal? AFFAIRLEAVEDAYS { get; set; }
        /// <summary>
        /// 病假天数
        /// </summary>
        public decimal? SICKLEAVEDAYS { get; set; }
        /// <summary>
        /// 年休假天数
        /// </summary>
        public decimal? ANNUALLEVELDAYS { get; set; }
        /// <summary>
        /// 婚假天数
        /// </summary>
        public decimal? MARRYDAYS { get; set; }
        /// <summary>
        /// 产假天数
        /// </summary>
        public decimal? MATERNITYLEAVEDAYS { get; set; }
        /// <summary>
        /// 看护假天数
        /// </summary>
        public decimal? NURSESDAYS { get; set; }
        /// <summary>
        /// 丧假天数
        /// </summary>
        public decimal? FUNERALLEAVEDAYS { get; set; }
        /// <summary>
        /// 工伤假天数
        /// </summary>
        public decimal? INJURYLEAVEDAYS { get; set; }
        /// <summary>
        /// 出差时长
        /// </summary>
        public decimal? EVECTIONTIME { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string REMARK { get; set; }
        #region 公共
        
        /// <summary>
        /// 审核状态
        /// </summary>
        public string CHECKSTATE { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public string CREATEUSERID { get; set; }
        /// <summary>
        /// 所属用户ID
        /// </summary>
        public string OWNERID { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string OWNERPOSTID { get; set; }
        /// <summary>
        ///所属部门ID
        /// </summary>
        public string OWNERDEPARTMENTID { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string OWNERCOMPANYID { get; set; }

        #endregion

    }
}
