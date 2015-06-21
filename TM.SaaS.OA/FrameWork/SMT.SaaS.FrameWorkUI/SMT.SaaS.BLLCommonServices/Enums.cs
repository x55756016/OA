using System;
using System.Net;

namespace SMT.SaaS.BLLCommonServices
{

    public enum OperationType
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
        /// 查看
        /// </summary>
        Browse,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
    }
    /// <summary>
    /// 客户端缓存Key
    /// </summary>
    public enum SysClientCache
    {
        /// <summary>
        /// 公司信息
        /// </summary>
        SYS_CompanyInfo,
        /// <summary>
        /// 部门信息
        /// </summary>
        SYS_DepartmentInfo,
        /// <summary>
        /// 岗位信息
        /// </summary>
        SYS_PostInfo,
        /// <summary>
        /// 当前用户的员工ID
        /// </summary>
        CurrentUserID
    }
    /// <summary>
    /// 窗体状态
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
        /// 查看
        /// </summary>
        Browse,
        /// <summary>
        /// 审核
        /// </summary>
        Audit,
        /// <summary>
        /// 重新提交
        /// </summary>
        Resubmit
    }
    public enum SalaryGenerateType
    { 
       SalaryRecord,
       CustomSalaryRecord,
       PerformanceRecord
    }
    /// <summary>
    /// 审核状态
    /// </summary>
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
        All,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = -1,
        /// <summary>
        /// 撤单
        /// </summary>
        Cancel=9
        
    }
    /// <summary>
    /// 表单状态
    /// </summary>
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

    public enum MessageTypes
    {
        Message,
        Caution,
        Error
    }
    public enum OrgTreeItemTypes
    {
        Company,
        Department,
        Post,
        Personnel,
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
        /// 正常
        /// </summary>
        Regular,
        /// <summary>
        /// 异常
        /// </summary>
        Abnormal,
        /// <summary>
        /// 旷工
        /// </summary>
        SkipWork,
        /// <summary>
        /// 出差
        /// </summary>
        OutOnDuty,
        /// <summary>
        /// 休息
        /// </summary>
        Rest,
        /// <summary>
        /// 请假
        /// </summary>
        Leave
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
        /// 未刷卡
        /// </summary>
        DrainPunch,
        /// <summary>
        /// 旷工
        /// </summary>
        SkipWork
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
        /// 其他假期
        /// </summary>
        OtherLeave
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
        /// 调休+带薪+扣款
        /// </summary>
        AdjLevDeduct,
        /// <summary>
        /// 调休+带薪+扣款
        /// </summary>
        AdjLevPaidDayDeduct
    }
    /// <summary>
    /// 打卡记录数据导入方式
    /// </summary>
    public enum ClockInRdUploadFileType
    {
        /// <summary>
        /// 文件
        /// </summary>
        File,
        /// <summary>
        /// 登录记录
        /// </summary>
        Login,
        /// <summary>
        /// 文件+登录
        /// </summary>
        FileAndLogin
    }
    /// <summary>
    /// 分配对像的类型
    /// </summary>
    public enum AssignObjectType
    {
        Company,
        Department,
        Post,
        Employee,
        Organize
    }

    /// <summary>
    /// 权限范围
    /// </summary>
    public enum PermissionRange
    {
        Organize,
        Company,
        Department,
        Post,
        Employee
    }

    public enum HREntity
    {
        //员工
        T_HR_EMPLOYEE
    }

    public enum IssuanceObjectType
    {
        Company,
        Department,
        Post,
        Employee,
    }
    /// <summary>
    /// 面试通知状态
    /// </summary>
    public enum NotifiedResult
    {
        /// <summary>
        /// 答应来面试
        /// </summary>
        Agree,
        /// <summary>
        /// 拒绝来面试
        /// </summary>
        Reject,
        /// <summary>
        /// 联系不上
        /// </summary>
        NotContact
    }
    /// <summary>
    /// 需求状态
    /// </summary>
    public enum RequirStates
    {
        /// <summary>
        /// 未生效
        /// </summary>
        UnActived,
        /// <summary>
        /// 未发布
        /// </summary>
        UnPublished,
        /// <summary>
        /// 发布中
        /// </summary>
        Published,
        /// <summary>
        /// 已完成
        /// </summary>
        Finished,
        /// <summary>
        /// 已过期
        /// </summary>
        TimeOut,
        /// <summary>
        /// 手动关闭
        /// </summary>
        Closed
    }


    /// <summary>
    /// 弹出窗体中按钮所对应的键值
    /// 如果需要增加不同的按钮则手动添加,添加时请添加注释
    /// 最好加上所属项目名
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 保存
        /// </summary>
        Save,
        /// <summary>
        /// 保存并关闭
        /// </summary>
        SaveAndClose,
        /// <summary>
        /// 保存并新建
        /// </summary>
        SaveAndNew,
        /// <summary>
        /// 提交审核
        /// </summary>
        SubmitAudit,

        /// <summary>
        /// OA  选择证照
        /// </summary>
        ChooseLicens
        
    }

    /// <summary>
    /// 协同办公子系统类型
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        /// OA办公自动化系统
        /// </summary>
        OA,
        /// <summary>
        /// 人力资源管理系统
        /// </summary>
        HR,
        /// <summary>
        /// 预算管理系统
        /// </summary>
        FB

    }
}
