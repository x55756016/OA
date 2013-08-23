using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.EntityFlowSys;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.Workflow.Common.Model.FlowEngine;


namespace SMT.FlowWFService
{
    // 注意: 如果更改此处的接口名称“IService1”，也必须更新 App.config 中对“IService1”的引用。
    [ServiceContract]
    public interface IService
    {

        #region 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowConsultation"></param>
        /// <param name="submitData"></param>
        [OperationContract]
        void AddConsultation(FLOW_CONSULTATION_T flowConsultation, SubmitData submitData);
        [OperationContract]
        void ReplyConsultation(FLOW_CONSULTATION_T flowConsultation, SubmitData submitData);
        #endregion 

        /// <summary>
        /// 调用流程系统
        /// </summary>
        /// <param name="entity">审批数据</param>
        /// <param name="NextStateCode">临时下一状态，正常情况为空</param>
        /// <param name="EditUserId">下一状态用户ID</param>
        /// <param name="Status">操作码，Add为启动新工作流，Update为审批已存在的工作流</param>
        /// <returns>OK为操作成功，End为审批结束信息</returns>
        //[OperationContract]
        //DataResult StartFlow(string FormID, string FlowGUID, string ModelCode, string CompanyID, string DepartmentID, string PostID, 
        //    string CreateUserID, string CreateUserName, string NextStateCode, string AppUserId, string AppUserName, string Content,
        //    string AppOpt, string Status, string xml, SubmitType SubmitType);

        [OperationContract]
        DataResult SubimtFlow(SubmitData ApprovalData);


        [OperationContract]
        DataResult GetAppUser(string CompanyID, string ModelCode, string FlowGUID, string xml);

        [OperationContract]
        List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordMaster(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);
        /// <summary>
        /// 直接修改审批记录，用来修正数据错误
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        string UpdateFlow(FLOW_FLOWRECORDDETAIL_T entity);

        /// <summary>
        /// 查询审批流程
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);

        /// <summary>
        /// 根据模块代码和用户id查询待审核单据
        /// </summary>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="EditUserID">系统用户id</param>
        /// <returns>Formids</returns>
        [OperationContract]
        List<string> GetWaitingApprovalForm(string ModelCode, string EditUserID);

        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FlowGUID"></param>
        /// <param name="CheckState"></param>
        /// <param name="Flag"></param>
        /// <param name="ModelCode"></param>
        /// <param name="CompanyID"></param>
        /// <param name="EditUserID"></param>
        /// <returns></returns>
        [OperationContract]
        List<TaskInfo> GetTaskInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);

        /// <summary>
        /// 通过机构ID和模块代码查询对应流程代码
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        //[OperationContract]
        //List<FLOW_MODELFLOWRELATION_T> GetFlowByModel(FLOW_MODELFLOWRELATION_T entity);
        /// <summary>
        /// 通过用户ID检索用户是否有未处理完成的流程记录
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [OperationContract]
        string IsExistFlowDataByUserID(string UserID,string PostID);

        [OperationContract]
        List<FLOW_FLOWRECORDMASTER_T> GetFlowDataByUserID(string UserID);

        /// <summary>
        /// 根据传入公司、部门查询对应的流程定义信息
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <returns></returns>
        [OperationContract]
        String GetFlowDefine(SubmitData ApprovalData);
        /// <summary>
        /// 对外接口：根据我的单据ID获取记录实体
        /// </summary>
        /// <param name="personalrecordid">我的单据ID</param>
        /// <returns></returns>
        [OperationContract]
        T_WF_PERSONALRECORD GetPersonalRecordByID(string personalrecordid);
        // 任务: 在此处添加服务操作

        /// <summary>
        /// 根据流程ID获取流程的所有分支
        /// </summary>
        /// <param name="FlowID"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetFlowBranch(string FlowID);
         /// <summary>
        /// 判断是否可能用自选流程或提单人可以撒回流程
        /// string[0]=1 可以用自选流程
        /// string[1]=1 提交人可以撒回流程
        /// </summary>
        /// <param name="modelcode">模块代码</param>
        /// <param name="companyid">公司ID</param>
        [OperationContract]
        string[] IsFreeFlowAndIsCancel(string modelcode, string companyid);
        [OperationContract]
        List<FLOW_MODELFLOWRELATION_T> GetModelFlowRelationInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount);
        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="formid"></param>
        /// <returns></returns> 
        [OperationContract]
        string GetMetadataByFormid(string formid);
        /// <summary>
        /// 更新元数据
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="xml"></param>
        /// <returns></returns>

        [OperationContract]
        bool UpdateMetadataByFormid(string formid, string xml);
    }

    #region 类型定义
    [DataContract]
    public class TaskInfo
    {
        [DataMember]
        public FLOW_FLOWRECORDDETAIL_T FlowInfo { set; get; }
        [DataMember]
        public string SubModelCode { set; get; }
    }


    [DataContract]
    public class SubmitData
    {
        /// <summary>
        /// 选择使用流程的类型
        /// </summary>
        [DataMember]
        public FlowSelectType FlowSelectType { get; set; } //选择使用流程的类型
      /// <summary>
        /// 表单ID
      /// </summary>
        [DataMember]
        public string FormID { get; set; }  //表单ID
       /// <summary>
        /// 模块代码
       /// </summary>
        [DataMember]
        public string ModelCode { get; set; }  //模块代码
       /// <summary>
        /// 审核人
       /// </summary>
        [DataMember]
        public UserInfo ApprovalUser { get; set; } //审核人
        /// <summary>
        /// 下一审批代码
        /// </summary>
        [DataMember]
        public string NextStateCode { get; set; } //下一审批代码
        /// <summary>
        /// 下一审批人
        /// </summary>
        [DataMember]
        public UserInfo NextApprovalUser { get; set; } //下一审批人
        /// <summary>
        /// 审批意见
        /// </summary>
        [DataMember]
        public string ApprovalContent { get; set; } //审批意见
       /// <summary>
        /// 审批结果
       /// </summary>
        [DataMember]
        public ApprovalResult ApprovalResult { get; set; } //审批结果
       /// <summary>
        /// 流程类型
       /// </summary>
        [DataMember]
        public FlowType FlowType { get; set; } //流程类型
        /// <summary>
        /// /提交标志
        /// </summary>
        [DataMember]
        public SubmitFlag SubmitFlag { get; set; } //提交标志
        /// <summary>
        /// 业务数据
        /// </summary>
        [DataMember]
        public string XML { get; set; }    //业务数据

        #region beyond
        /// <summary>
        /// 会签人角色人员列表
        /// </summary>
        [DataMember]
        public Dictionary<Role_UserType, List<UserInfo>> DictCounterUser
        {
            get;
            set;
        }

        ///// <summary>
        ///// 代理
        ///// </summary>
        //[DataMember]
        //public Dictionary<Role_UserType, List<UserInfo>> DictAgentUserInfo
        //{
        //    get;
        //    set;
        //}

        //[DataMember]
        //public bool IsCountersign
        //{
        //    get;
        //    set;
        //}

        //[DataMember]
        //public string CountersignType
        //{
        //    get;
        //    set;
        //}

        #endregion

        #region 提交人的身份信息
        /// <summary>
        /// 提交人的属人公司ID
        /// </summary>
        [DataMember]
        public string SumbitCompanyID { get; set; }    //公司ID
        /// <summary>
        /// 提交人的属人部门ID
        /// </summary>
        [DataMember]
        public string SumbitDeparmentID { get; set; }    //部门ID
        /// <summary>
        /// 提交人的属人岗位ID
        /// </summary>
        [DataMember]
        public string SumbitPostID { get; set; }    //岗位ID
        /// <summary>
        /// 提交人的ID
        /// </summary>
        [DataMember]
        public string SumbitUserID { get; set; }    //用户ID
        /// <summary>
        /// 提交人的姓名
        /// </summary>
        [DataMember]
        public string SumbitUserName { get; set; }    //用户姓名
        #endregion

    }
    /// <summary>
    /// 审核结果
    /// </summary>
    [DataContract]
    public enum ApprovalResult
    {
        [EnumMember]
        NoPass = 0, //不通过
        [EnumMember]
        Pass = 1   //通过

    }
    /// <summary>
    /// 服务标志
    /// </summary>
    [DataContract]
    public enum SubmitFlag
    {
        [EnumMember]
        New = 0,  //提交审核
        [EnumMember]
        Approval = 1, //审核通不通过状态
        [EnumMember]
        Cancel=5   //撤销


    }
    /// <summary>
    /// 流程服务返回结果状态
    /// </summary>
    [DataContract]
    public enum FlowResult
    {
        [EnumMember]
        FAIL = 0,  //失败
        [EnumMember]
        SUCCESS = 1, //成功
        [EnumMember]
        END = 2, //结束
        [EnumMember]
        MULTIUSER = 3, //下一个节点多人状态（选人）
        [EnumMember]
        Countersign = 4 //下一个节点是会签状态（选人）

    }
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public enum SubmitType
    {
        /// <summary>
        /// 审核通过
        /// </summary>
        [EnumMember]
        Approval = 1, //
       /// <summary>
       /// 审核中
       /// </summary>
        [EnumMember]
        Pending = 0 //

    }
    /// <summary>
    /// 流程类型
    /// </summary>
    [DataContract]
    public enum FlowType
    {
        /// <summary>
        /// 审批
        /// </summary>
        [EnumMember]
        Approval = 0,//审批
      /// <summary>
        /// 任务
      /// </summary>
        [EnumMember]
        Task = 1, //任务
        [EnumMember]
        Pending = 2

    }
    [DataContract]
    public enum FlowSelectType
    {
        /// <summary>
        /// 固定流程
        /// </summary>
        [EnumMember]
        FixedFlow = 0,  //固定流程
       /// <summary>
        /// 自选流程
       /// </summary>
        [EnumMember]
        FreeFlow = 1,  //自选流程


    }

    [DataContract]
    public class DataResult
    {
        FlowResult flowResult;   //流程结果
        List<UserInfo> userInfo = new List<UserInfo>();  //下一审批人列表
        UserInfo agentuser;
        [DataMember]
        public FlowResult FlowResult
        {
            get { return flowResult; }
            set { flowResult = value; }
        }

        [DataMember]
        public List<UserInfo> UserInfo
        {
            get { return userInfo; }
            set { userInfo = value; }
        }
        /// <summary>
        /// 代理审核
        /// </summary>
        [DataMember]
        public UserInfo AgentUserInfo
        {
            get { return agentuser; }
            set { agentuser = value; }
        }
        /// <summary>
        /// 错误号
        /// </summary>
        [DataMember]
        public int ErrNum   //错误号
        {
            get;
            set;
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        [DataMember]
        public string Err   //错误信息
        {
            get;
            set;
        }
        /// <summary>
        /// 下一审批状态代码
        /// </summary>
        [DataMember]
        public string AppState     //下一审批状态代码
        {
            get;
            set;
        }
        /// <summary>
        /// 审批结果
        /// </summary>
        [DataMember]
        public string CheckState   //审批结果
        {
            get;
            set;
        }
        /// <summary>
        /// 模块关联流程ID
        /// </summary>
        [DataMember]
        public String ModelFlowRelationID { get; set; }  //模块关联流程ID
      /// <summary>
        /// 子模块代码
      /// </summary>
        [DataMember]
        public String SubModelCode { get; set; }         //子模块代码
        /// <summary>
        /// 运行时间日志
        /// </summary>
        [DataMember]
        public string RunTime                            //运行时间日志
        {
            get;
            set;
        }

        #region beyond
        /// <summary>
        /// 会签人角色人员列表
        /// </summary>
        [DataMember]
        public Dictionary<Role_UserType, List<UserInfo>> DictCounterUser
        {
            get;
            set;
        }

        /// <summary>
        /// 代理
        /// </summary>
        [DataMember]
        public Dictionary<UserInfo, UserInfo> DictAgentUserInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 是否会签
        /// </summary>
        [DataMember]
        public bool IsCountersign
        {
            get;
            set;
        }

        /// <summary>
        /// 0即所有人通过才通过
        /// 1有一人通过就算通过
        /// </summary>
        [DataMember]
        public string CountersignType
        {
            get;
            set;
        }
        /// <summary>
        /// 是否会签完成
        /// </summary>
        [DataMember]
        public bool IsCountersignComplete
        {
            get;
            set;
        }

        [DataMember]
        public SubmitFlag SubmitFlag
        {
            get;
            set;
        }

        /// <summary>
        /// 该状态的kpi时间
        /// </summary>
        internal string KPITime
        {
            get;
            set;
        }

        private bool _CurrentIsCountersign = false;
        internal bool CurrentIsCountersign
        {
            get
            {
                return this._CurrentIsCountersign;
            }
            set
            {
                this._CurrentIsCountersign = value;
            }
        }

        private bool _IsGotoNextState = false;
        internal bool IsGotoNextState
        {
            get
            {
                return this._IsGotoNextState;
            }
            set
            {
                this._IsGotoNextState = value;
            }
        }
        //public bool CanSendMessage
        //{
        //    get;
        //    set;
        //}
        #endregion
    }

    [DataContract]
    public class UserInfo
    {
        string userID = "";
        string userName = "";

        [DataMember]
        public string CompanyID { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string DepartmentID { get; set; }
        [DataMember]
        public string DepartmentName { get; set; }
        [DataMember]
        public string PostID { get; set; }
        [DataMember]
        public string PostName { get; set; }
        /// <summary>
        /// 所包含的角色：一个人在同一家公司可以有多个角色
        /// </summary>
         [DataMember]
        public List<T_SYS_ROLE> Roles { get; set; }
        /// <summary>
        /// 是否是所在的部门的负责人
        /// </summary>
         [DataMember]
        public string IsHead { get; set; }
        /// <summary>
        /// 是否是所在的岗位的直接上级
        /// </summary>
         [DataMember]
        public string IsSuperior { get; set; }    

        [DataMember]
        public string UserID
        {
            get { return userID; }
            set { userID = value; }
        }

        [DataMember]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

    }

    #endregion
}


