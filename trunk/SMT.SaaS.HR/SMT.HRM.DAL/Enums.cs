using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.DAL
{
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
        Delete = -1
    }
    /// <summary>
    /// 编辑状态
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
    /// 审批状态
    /// </summary>
    public enum FlowState
    {
        /// <summary>
        /// 添加
        /// </summary>
        Add,
        /// <summary>
        /// 审批
        /// </summary>
        Update
    }

    /// <summary>
    /// 分配对像的类型
    /// </summary>
    public enum AssignObjectType
    {         
        Organize,
        Company,
        Department,
        Post,
        Employee
        
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
}
