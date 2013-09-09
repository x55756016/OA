using System;

namespace SMT.HRM.BLL.Common
{
    /// <summary>
    /// 窗体类型
    /// </summary>
    public enum FormTypes
    {
        /// <summary>
        /// 新增
        /// </summary>
        New,
        /// <summary>
        /// 编辑
        /// </summary>
        Edit,
        /// <summary>
        /// 浏览
        /// </summary>
        Browse,
        /// <summary>
        /// 审核
        /// </summary>
        Audit
    }
    public enum CheckStates
    {
        /// <summary>
        /// 未提交
        /// </summary>
        UnSubmit,
        /// <summary>
        /// 审核中
        /// </summary>
        Approving,
        /// <summary>
        /// 审核通过
        /// </summary>
        Approved,
        /// <summary>
        /// 审核未通过
        /// </summary>
        UnApproved,
        /// <summary>
        /// 待审核
        /// </summary>
        WaittingApproval,
        /// <summary>
        /// 所有
        /// </summary>
        All
    }
    public enum EditStates
    {
        /// <summary>
        /// 未生效
        /// </summary>
        UnActived,
        /// <summary>
        /// 已生效
        /// </summary>
        Actived,
        /// <summary>
        /// 撤消中
        /// </summary>
        PendingCanceled,
        /// <summary>
        /// 已撤消
        /// </summary>
        Canceled,
        /// <summary>
        /// 删除状态
        /// </summary>
        Deleted
    }
    /// <summary>
    /// 员工状态
    /// </summary>
    public enum EmployeeState
    {
        /// <summary>
        /// 试用
        /// </summary>
        OnTrial,
        /// <summary>
        /// 转正
        /// </summary>
        Regular,
        /// <summary>
        /// 已离职
        /// </summary>
        Dimission,
        /// <summary>
        /// 离职中
        /// </summary>
        OnLeaving
    }
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageTypes
    {
        /// <summary>
        /// 提示
        /// </summary>
        Message,
        /// <summary>
        /// 警告
        /// </summary>
        Caution,
        /// <summary>
        /// 错误
        /// </summary>
        Error
    }
    /// <summary>
    /// 是否代理岗位
    /// </summary>
    public enum IsAgencyPost
    {
        /// <summary>
        /// 否(主岗位)
        /// </summary>
        No,
        /// <summary>
        /// 是(代理岗位) 
        /// </summary>
        Yes
    }
    /// <summary>
    /// 组织架构类型
    /// </summary>
    public enum OrgTreeItemTypes
    {
        /// <summary>
        /// 公司
        /// </summary>
        Company,
        /// <summary>
        /// 部门
        /// </summary>
        Department,
        /// <summary>
        /// 岗位
        /// </summary>
        Post,
        /// <summary>
        /// 员工
        /// </summary>
        Personnel,
        /// <summary>
        /// 所有
        /// </summary>
        All
    }
    /// <summary>
    /// 分配对象类别
    /// </summary>
    public enum AssignedObjectType
    {
        /// <summary>
        /// 公司
        /// </summary>
        Company,
        /// <summary>
        /// 部门
        /// </summary>
        Department,
        /// <summary>
        /// 岗位
        /// </summary>
        Post,
        /// <summary>
        /// 员工
        /// </summary>
        Personnel
    }
    /// <summary>
    /// 考勤列外日期
    /// </summary>
    public enum OutPlanDaysType
    {
        /// <summary>
        /// 公共假期
        /// </summary>
        Vacation,
        /// <summary>
        /// 工作日
        /// </summary>
        WorkDay
    }
    /// <summary>
    /// 工作天数计算方式
    /// </summary>
    public enum WorkDayType
    {
        /// <summary>
        /// 固定方式
        /// </summary>
        Fixed,
        /// <summary>
        /// 实际方式
        /// </summary>
        Fact
    }
    /// <summary>
    /// 考勤排班循环方式
    /// </summary>
    public enum SchedulingCircleType
    {
        /// <summary>
        /// 月
        /// </summary>
        Month,
        /// <summary>
        /// 周
        /// </summary>
        Week
    }
    /// <summary>
    /// 是否选中
    /// </summary>
    public enum IsChecked
    {
        /// <summary>
        /// 否
        /// </summary>
        No,
        /// <summary>
        /// 是
        /// </summary>
        Yes
    }
    /// <summary>
    /// 考勤异常状态
    /// </summary>
    public enum AttendAbnormalType
    {
        /// <summary>
        /// 迟到
        /// </summary>
        Late,
        /// <summary>
        /// 早退
        /// </summary>
        LeaveEarly,
        /// <summary>
        /// 漏打卡
        /// </summary>
        DrainPunch,
        /// <summary>
        /// 旷工
        /// </summary>
        SkipWork
    }
    /// <summary>
    /// 考勤异常扣款
    /// </summary>
    public enum AttexFineType
    {
        /// <summary>
        /// 迟到1：每次扣X元
        /// </summary>
        LateFineType1,
        /// <summary>
        /// 迟到1：按日薪/分钟 * 迟到的分钟数（不足一分按一分算），最低扣 X 元
        /// </summary>
        LateFineType2,
        /// <summary>
        /// 迟到1：N次以内（含N次）按日薪/分钟 * 迟到的分钟数（不足一分按一分算），超过N次的，按X元/次
        /// </summary>
        LateFineType3,
        /// <summary>
        /// 迟到1：N次以内（含N次）按X元/次，超过N次的，按Y元/次
        /// </summary>
        LateFineType4,
        /// <summary>
        /// 早退：每次扣X元
        /// </summary>
        LeaveEarly1,
        /// <summary>
        /// 早退：按日薪/分钟 * 迟到的分钟数（不足一分按一分算），最低扣 X 元
        /// </summary>
        LeaveEarly2,
        /// <summary>
        /// 早退：N次以内（含N次）按日薪/分钟 * 迟到的分钟数（不足一分按一分算），超过N次的，按X元/次
        /// </summary>
        LeaveEarly3,
        /// <summary>
        /// 早退：N次以内（含N次）按X元/次，超过N次的，按Y元/次
        /// </summary>
        LeaveEarly4,
        /// <summary>
        /// 未刷卡：每月允许X次（含X）漏打，超过X次后，每次扣M元，超过 Y(Y>X)次后，每次扣N（N>M）元
        /// </summary>
        DrainPunch,
        /// <summary>
        /// 旷工：日薪（按分钟计）* 旷工时间（分钟）* X 倍
        /// </summary>
        SkipWork
    }
    /// <summary>
    /// 请假类型值
    /// </summary>
    public enum LeaveTypeValue
    {
        /// <summary>
        /// 调休假
        /// </summary>
        AdjustLeave,
        /// <summary>
        /// 事假
        /// </summary>
        AffairLeave,
        /// <summary>
        /// 病假
        /// </summary>
        SickLeave,
        /// <summary>
        /// 年假
        /// </summary>
        AnnualLeave,
        /// <summary>
        /// 产假
        /// </summary>
        MaternityLeave,
        /// <summary>
        /// 婚假
        /// </summary>
        MarryLeave,
        /// <summary>
        /// 看护假
        /// </summary>
        NursesLeave,
        /// <summary>
        /// 路程假
        /// </summary>
        TripLeave,
        /// <summary>
        /// 工伤假
        /// </summary>
        InjuryLeave,
        /// <summary>
        /// 丧假
        /// </summary>
        FuneralLeave,
        /// <summary>
        /// 产前检查假
        /// </summary>
        PrenatalcareLeave
    }
    /// <summary>
    /// 请假扣款方式
    /// </summary>
    public enum LeaveFineType
    {
        /// <summary>
        /// 不扣
        /// </summary>
        Free,
        /// <summary>
        /// 按日薪扣
        /// </summary>
        Deduct,
        /// <summary>
        /// 调休+扣款
        /// </summary>
        AdjLevDeduct,
        /// <summary>
        /// 调休+带薪+扣款
        /// </summary>
        AdjLevPaidDayDeduct
    }
    /// <summary>
    /// 请假类型值
    /// </summary>
    public enum OffestType
    {
        /// <summary>
        /// 一次性修完年假
        /// </summary>
        OnceOnly,
        /// <summary>
        /// 多次修假，用于冲减病事假
        /// </summary>
        NotOnce,
        /// <summary>
        /// 无限制
        /// </summary>
        LimitLess
    }

    /// <summary>
    /// 发薪方式
    /// </summary>
    public enum PaidType
    {
        /// <summary>
        /// 银行代发
        /// </summary>
        BANKSUBSTITUTING,
        /// <summary>
        /// 现金发放
        /// </summary>
        CASHPAYMENT
    }

    /// <summary>
    /// 发放状态
    /// </summary>
    public enum PaymentState
    {
        /// <summary>
        /// 未发放
        /// </summary>
        NOPAYMENT,
        /// <summary>
        /// 发放中
        /// </summary>
        PAYMENTING,
        /// <summary>
        /// 已发放
        /// </summary>
        ALREADYPAYMENT
    }
    /// <summary>
    /// 加班薪酬方式
    /// </summary>
    public enum OverTimePayType
    {
        /// <summary>
        /// 调休假
        /// </summary>
        AdjustLeave,
        /// <summary>
        /// 加班工资
        /// </summary>
        OverTimePay,
        /// <summary>
        /// 无报酬
        /// </summary>
        NoPay
    }
    /// <summary>
    /// 加班生效方式
    /// </summary>
    public enum OverTimeValid
    {
        /// <summary>
        /// 需审核通过
        /// </summary>
        ToCheck,
        /// <summary>
        /// 自动累计(这个要看打卡记录)
        /// </summary>
        AutoAccumulate,
        /// <summary>
        /// 仅节假日计算(这个要看打卡记录)
        /// </summary>
        OnlyHoliday
    }
    /// <summary>
    /// 考勤方式
    /// </summary>
    public enum AttendanceType
    {
        /// <summary>
        /// 打卡
        /// </summary>
        PunchCheck,
        /// <summary>
        /// 不考勤
        /// </summary>
        NoCheck,
        /// <summary>
        /// 登陆系统考勤
        /// </summary>
        LoginCheck,
        /// <summary>
        /// 打卡及登陆系统考勤
        /// </summary>
        LoginAndPubch
    }
    /// <summary>
    /// 出勤状态
    /// </summary>
    public enum AttendanceState
    {
        /// <summary>
        /// 正常实际枚举值加1
        /// </summary>
        Regular,
        /// <summary>
        /// 考勤异常实际枚举值加1=2
        /// </summary>
        Abnormal,
        /// <summary>
        /// 旷工实际枚举值加1=3
        /// </summary>
        SkipWork,
        /// <summary>
        /// 出差实际枚举值加1=4
        /// </summary>
        Travel,
        /// <summary>
        /// 休息实际枚举值加1=5
        /// </summary>
        Rest,
        /// <summary>
        /// 请假实际枚举值加1=6
        /// </summary>
        Leave,
        /// <summary>
        /// 混合状态(针对一天内同时出现请假，考勤异常)实际枚举值加1=7
        /// </summary>
        MixLeveAbnormal,
        /// <summary>
        /// 混合状态(针对一天内同时出现出差，考勤异常)实际枚举值加1=8
        /// </summary>
        MixTravelAbnormal,
        /// <summary>
        /// 外出申请实际枚举值加1=9
        /// </summary>
        OutApply,
        /// <summary>
        /// 混合状态(针对一天内同时出现外出申请，考勤异常)实际枚举值加1=10
        /// </summary>
        MixOutApplyAbnormal
    }
    /// <summary>
    /// 考勤异常类型(异常记录明细表专用)
    /// </summary>
    public enum AbnormCategory
    {
        /// <summary>
        /// 迟到
        /// </summary>
        Late,
        /// <summary>
        /// 早退
        /// </summary>
        LeaveEarly,
        /// <summary>
        /// 缺勤
        /// </summary>
        Absent
    }
    /// <summary>
    /// 考勤异常时间段(异常记录明细表专用)
    /// </summary>
    public enum AttendPeriod
    {
        /// <summary>
        /// 上午
        /// </summary>
        Morning,
        /// <summary>
        /// 中午
        /// </summary>
        Midday,
        /// <summary>
        /// 下午
        /// </summary>
        Afternoon,
        /// <summary>
        /// 晚上
        /// </summary>
        Evening
    }

    /// <summary>
    /// 考勤异常类型(异常记录明细表专用)
    /// </summary>
    public enum AbnormReasonCategory
    {
        /// <summary>
        /// 漏打卡
        /// </summary>
        DrainPunch,
        /// <summary>
        /// 未发卡
        /// </summary>
        NoPunch,
        /// <summary>
        /// 因工外出
        /// </summary>
        OutOnDuty,
        /// <summary>
        /// 机械故障
        /// </summary>
        Mechanicalfail
    }
    /// <summary>
    /// 实体枚举类
    /// </summary>
    public enum ModelNames
    {
        /// <summary>
        /// 
        /// </summary>
        T_HR_ADJUSTLEAVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_AREAALLOWANCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_AREACITY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_AREADIFFERENCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ASSESSMENTFORMDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ASSESSMENTFORMMASTER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCEDEDUCTDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCEDEDUCTMASTER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCERECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCESOLUTION,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCESOLUTIONASIGN,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDANCESOLUTIONDEDUCT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDFREELEAVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDMACHINESET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDMONTHLYBALANCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDMONTHLYBATCHBALANCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_ATTENDYEARLYBALANCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_BLACKLIST,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CALCULATEFORMULA,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CHECKPOINTLEVELSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CHECKPOINTSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CHECKPROJECTSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_COMPANY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_COMPANYHISTORY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CUSTOMGUERDON,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CUSTOMGUERDONARCHIVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CUSTOMGUERDONARCHIVEHIS,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CUSTOMGUERDONRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_CUSTOMGUERDONSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_DEPARTMENT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_DEPARTMENTDICTIONARY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_DEPARTMENTHISTORY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EDUCATEHISTORY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEABNORMRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEADDSUM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEECANCELLEAVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEECHECK,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEECLOCKINRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEECLOCKINRECORD2,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEECONTRACT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEENTRY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEEVECTIONRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEEVECTIONREPORT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEINSURANCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEELEAVEDAY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEELEAVERECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEELEVELDAYCOUNT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEELEVELDAYDETAILS,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEELOGINRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEOVERTIMERECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEPOST,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEPOSTCHANGE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEPOSTHISTORY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEESALARYRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEESALARYRECORDPAYMENT,
        /// <summary>
        ///
        /// </summary>
        T_HR_EMPLOYEESALARYRECORDHIS,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEESALARYRECORDITEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEESIGNINDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEESIGNINRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEWELFARE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EMPLOYEEWELFARESET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_EXPERIENCE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_FAMILYMEMBER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_FREELEAVEDAYSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_IMPORTSETDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_IMPORTSETMASTER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_KPIPOINT,
        /// <summary>
        /// 
        /// </summary>
        T_HR_KPIRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_KPIRECORDCOMPLAIN,
        /// <summary>
        /// 
        /// </summary>
        T_HR_KPIREMIND,
        /// <summary>
        /// 
        /// </summary>
        T_HR_KPITYPE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_LEAVETYPESET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_LEFTOFFICE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_OVERTIMEREWARD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_OrgCheckSummary,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PENSIONALARMSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PENSIONDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PENSIONMASTER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PERFORMANCEDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PERFORMANCERECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PERFORMANCEREWARDRECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_PERFORMANCEREWARDSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_POST,
        /// <summary>
        /// 
        /// </summary>
        T_HR_POSTDICTIONARY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_POSTHISTORY,
        /// <summary>
        /// 
        /// </summary>
        T_HR_POSTLEVELDISTINCTION,
        /// <summary>
        /// 
        /// </summary>
        T_HR_POSTLEVELSYSTEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_RAMDONGROUPPERSON,
        /// <summary>
        /// 
        /// </summary>
        T_HR_RANDOMGROUP,
        /// <summary>
        /// 
        /// </summary>
        T_HR_RELATIONPOST,
        /// <summary>
        /// 
        /// </summary>
        T_HR_RESUME,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYARCHIVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYARCHIVEHIS,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYARCHIVEITEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYITEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYLEVEL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSOLUTION,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSOLUTIONASSIGN,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSOLUTIONITEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSOLUTIONSTANDARD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSTANDARD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSTANDARDITEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYSYSTEM,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SALARYTAXES,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SCHEDULINGTEMPLATEDETAIL,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SCHEDULINGTEMPLATEMASTER,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SCORETYPE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SHIFTDEFINE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_STANDARDPERFORMANCEREWARD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_STANDREWARDARCHIVE,
        /// <summary>
        /// 
        /// </summary>
        T_HR_STANDREWARDARCHIVEHIS,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SUMPERFORMANCERECORD,
        /// <summary>
        /// 
        /// </summary>
        T_HR_SYSTEMSETTING,
        /// <summary>
        /// 
        /// </summary>
        T_HR_VACATIONSET,
        /// <summary>
        /// 
        /// </summary>
        T_HR_WELFARESET
    }
}
