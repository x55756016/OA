using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.SaaS.OA.DAL
{
    /// <summary>
    /// 审批状态
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
        /// 审核不通过
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

    public enum Action
    { 
        /// <summary>
        /// 新增
        /// </summary>
        Add,
        /// <summary>
        /// 修改
        /// </summary>
        Edit,
        /// <summary>
        /// 借出
        /// </summary>
        Lend,
        /// <summary>
        /// 归还
        /// </summary>
        Return
    }
}
