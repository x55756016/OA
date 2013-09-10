using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.SaaS.OA.Services
{
    public enum FormTypes
    {
        New,
        Edit
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
        AUDIT
    }

    public enum ModelNames
    { 
        /// <summary>
        /// 公文
        /// </summary>
        T_OA_SENDDOC,
        /// <summary>
        /// 事项审批
        /// </summary>
        T_OA_APPROVALINFO,
        /// <summary>
        /// 会议申请
        /// </summary>
        T_OA_MEETINGINFO,
        /// <summary>
        /// 出差申请
        /// </summary>
        T_OA_BUSINESSTRIP,
        /// <summary>
        /// 出差报告
        /// </summary>
        T_OA_BUSINESSREPORT,
        /// <summary>
        /// 出差报销
        /// </summary>
        T_OA_TRAVELREIMBURSEMENT,
        /// <summary>
        /// 合同申请
        /// </summary>
        T_OA_CONTRACTAPP,
        /// <summary>
        /// 合同查看申请
        /// </summary>
        T_OA_CONTRACTVIEW,
        /// <summary>
        /// 福利发放
        /// </summary>
        T_OA_WELFAREDISTRIBUTEMASTER,
        /// <summary>
        /// 福利发放撤销
        /// </summary>
        T_OA_WELFAREDISTRIBUTEUNDO,
        /// <summary>
        /// 福利标准
        /// </summary>
        T_OA_WELFAREMASERT,

        //-----------------
        /// <summary>
        /// 保养申请
        /// </summary>
        T_OA_CONSERVATION,

        /// <summary>
        /// 保养记录
        /// </summary>
        T_OA_CONSERVATIONRECORD,

        /// <summary>
        /// 维修申请
        /// </summary>
        T_OA_MAINTENANCEAPP,

        /// <summary>
        /// 维修记录
        /// </summary>
        T_OA_MAINTENANCERECORD,

        /// <summary>
        /// 派车单
        /// </summary>
        T_OA_VEHICLEDISPATCH,

        /// <summary>
        /// 用车申请
        /// </summary>
        T_OA_VEHICLEUSEAPP,

        /// <summary>
        /// 派车记录
        /// </summary>
        T_OA_VEHICLEDISPATCHRECORD,

        
        /// <summary>
        /// 房源信息发布
        /// </summary>
        T_OA_HOUSEINFOISSUANCE,

        /// <summary>
        /// 租房申请
        /// </summary>
        T_OA_HIREAPP,


        /// <summary>
        /// 机构表
        /// </summary>
        T_OA_ORGANIZATION,

        /// <summary>
        /// 证照印章外借记录
        /// </summary>
        T_OA_LICENSEUSER,

        /// <summary>
        /// 证照印章表
        /// </summary>
        T_OA_LICENSEMASTER,

        /// <summary>
        /// 员工满意度调查发布申请
        /// </summary>
         T_OA_SATISFACTIONDISTRIBUTE,
        /// <summary>
        /// 员工调查方案表
        /// </summary>
        T_OA_REQUIREMASTER,

        /// <summary>
        /// 员工调查发布申请表
        /// </summary>
        T_OA_REQUIREDISTRIBUTE,

        /// <summary>
        /// 员工满意度调查方案表
        /// </summary>
        T_OA_SATISFACTIONMASTER,

        /// <summary>
        /// 员工满意度调查申请表
        /// </summary>
        T_OA_SATISFACTIONREQUIRE
       

        
       
        
    }
}
