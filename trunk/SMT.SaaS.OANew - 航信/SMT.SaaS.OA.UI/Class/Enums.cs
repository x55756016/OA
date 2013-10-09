using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.OA.UI
{  
    //public enum FormTypes
    //{
    //    New,
    //    Edit
    //}
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
        ALL
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
        PendingCancelled,
        /// <summary>
        /// 已撤消
        /// </summary>
        Cancelled,
        /// <summary>
        /// 删除状态
        /// </summary>
        Deleted
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
        /// 删除
        /// </summary>
        Delete,
        /// <summary>
        /// 查看
        /// </summary>
        Read,
        /// <summary>
        /// 打印
        /// </summary>
        Print,
        /// <summary>
        /// 上传附件
        /// </summary>
        FromAnnex,
        /// <summary>
        /// 申请查看
        /// </summary>
        ApplicationView,
        /// <summary>
        /// 借出
        /// </summary>
        Lend,
        /// <summary>
        /// 归还
        /// </summary>
        Return,
        /// <summary>
        /// 审批
        /// </summary>
        AUDIT,
        /// <summary>
        /// 重新提交
        /// </summary>
        ReSubmit
    }
    public enum TravelType
    {
        /// <summary>
        /// 出差申请
        /// </summary>
        TravelApp,
        /// <summary>
        /// 出差报告
        /// </summary>
        TravelReport,
        /// <summary>
        /// 出差报销
        /// </summary>
        Reimbursement
    }
}
