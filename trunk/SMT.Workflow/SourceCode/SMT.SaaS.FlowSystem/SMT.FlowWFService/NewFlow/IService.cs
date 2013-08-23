/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：IService2.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/12/15 08:51:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.FlowWFService
	 * 模块名称：
	 * 描　　述： 对原有的IService.cs进行重写，加入了OracleConnection作为参数，以便作事务处理
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.EntityFlowSys;
using System.Data.OracleClient;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.FlowWFService.NewFlow
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
        void AddConsultation(OracleConnection con, FLOW_CONSULTATION_T flowConsultation, SubmitData submitData);
        [OperationContract]
        void ReplyConsultation(OracleConnection con, FLOW_CONSULTATION_T flowConsultation, SubmitData submitData);
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
        DataResult GetAppUser(OracleConnection con, string CompanyID, string ModelCode, string FlowGUID, string xml);

        [OperationContract]
        List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordMaster(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);
        /// <summary>
        /// 直接修改审批记录，用来修正数据错误
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        string UpdateFlow(OracleConnection con, FLOW_FLOWRECORDDETAIL_T entity);

        /// <summary>
        /// 查询审批流程
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfo(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);


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
        List<TaskInfo> GetTaskInfo(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID);

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
        string IsExistFlowDataByUserID(string UserID, string PostID);

        [OperationContract]
        List<FLOW_FLOWRECORDMASTER_T> GetFlowDataByUserID(OracleConnection con, string UserID);

        /// <summary>
        /// 根据传入公司、部门查询对应的流程定义信息
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <returns></returns>
        [OperationContract]
        String GetFlowDefine(OracleConnection con, SubmitData ApprovalData);
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
        /// <summary>
        /// KPI调用
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
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
}


