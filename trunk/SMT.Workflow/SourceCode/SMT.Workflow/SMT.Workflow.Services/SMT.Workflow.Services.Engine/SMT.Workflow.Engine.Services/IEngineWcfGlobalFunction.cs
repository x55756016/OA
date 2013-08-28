using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Global.IEngineContract;
using EngineDataModel;
using SMT.Workflow.Engine.Services.BLL;

namespace SMT.Workflow.Engine.Services
{

    [ServiceContract]
    public interface IEngineWcfGlobalFunction
    {
        [OperationContract]
        void TaskCacheReflesh(string userID);
        /// <summary>
        /// Remove cache after TODO Task was added
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        bool RemoveCache(string userID);
        /// <summary>
        /// 流程触发接口（流程操作完成后调用，触发待办事项）
        /// </summary>
        /// <param name="strFlowMessage">流程信息数据</param>
        /// <param name="strBOObject">业务元数据</param>
        /// <returns></returns>
        [OperationContract]
        bool SaveFlowTriggerData(string strFlowMessage, string strBOObject);
        /// <summary>
        /// 待办任务主数据（分页）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>
        [OperationContract]
        List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref int rowCount, ref int pageCount);

        /// <summary>
        /// 手机版待办任务主数据（分页）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract]
        List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndexForMobile(MsgParms msgParams, ref int rowCount, ref int pageCount);

        /// <summary>
        /// 根据MessageId获取上一条、下一条记录
        /// </summary>
        /// <param name="msgParms"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, T_FLOW_ENGINEMSGLIST> GetPendingTaskPrevNext(MsgParms msgParms);

        /// <summary>
        /// 待办任务主数据（分页）缓存接口
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="IsAutofresh"></param>
        /// <param name="HaveNewTask"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        [OperationContract(Name = "PendingCacheTasksParmsPageIndex")]
        List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask, ref int rowCount, ref int pageCount);

        /// <summary>
        /// 待办任务细数据（不带业务数据字段）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>
        [OperationContract]
        List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams);
        /// <summary>
        /// 获取待办扩展接口
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="IsAutofresh">是否是门户自动刷新</param>
        /// <returns></returns>
        [OperationContract(Name = "PendingCacheMainTasksParms")]
        List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask);

        /// <summary>
        /// 待办任务细数据（带业务数据字段）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>
        [OperationContract]
        T_FLOW_ENGINEMSGLIST PendingDetailByID(string strMsgID);
        /// <summary>
        /// 待办任务详细数据（旧接口）
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>
        [OperationContract]
        T_FLOW_ENGINEMSGLIST PendingDetailTasks(string strMsgID);
        /// <summary>
        /// 待办任务详细数据（手机接口）
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>
        [OperationContract(Name = "PendingDetailTasksByPhone")]
        T_FLOW_ENGINEMSGLIST PendingDetailTasksByPhone(string strMsgID);

        [OperationContract]
        T_FLOW_ENGINEMSGLIST PendingDetailTasksByOrderNodeCodeForMobile(string orderNodeCode);

        /// <summary>
        /// 获取消息接口（老接口）
        /// </summary>
        /// <param name="strUserID">接收用户</param>
        /// <param name="strStatus">状态（Open,Close）</param>
        /// <param name="iTop">条目</param>
        /// <returns></returns>
        [OperationContract]
        List<T_FLOW_ENGINENOTES> EngineNotes(string strUserID, string strStatus, int iTop);

        /// <summary>
        /// 取消事件定时
        /// </summary>
        /// <param name="strApplicationOrderCode">业务系统Guid</param>
        /// <returns></returns>
        [OperationContract]
        bool CancelEventTriggerData(string strApplicationOrderCode);
        /// <summary>
        /// 事件定时触发
        /// </summary>
        /// <param name="strEventXml"></param>
        [OperationContract(IsOneWay = true)]
        void SaveEventData(string strEventXml);
        /// <summary>
        /// 消息触发（任务，任务汇报）
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void MsgTrigger(string[] recevieUser, string SystemCode, string MsgKey, string ID, string strXml);
        [OperationContract(IsOneWay = true)]
        void NotesSend(List<ReceiveUserAndContent> List, string SystemCode, string strFormID);

        /// <summary>
        /// 待办任务关闭(单个关闭)
        /// </summary>
        /// <param name="strSystemCode">系统代号</param>
        /// <param name="strFormID">单据ID</param>
        /// <param name="strReceiveUser">接收用户</param>
        [OperationContract(IsOneWay = true)]
        void TaskMsgClose(string strSystemCode, string strFormID, string strReceiveUser);
        /// <summary>
        /// 关闭代办
        /// </summary>
        /// <param name="listOrderID"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strReceiveUser"></param>
        [OperationContract]
        void CloseDoTask(List<string> listOrderID, string strModelCode, string strReceiveUser);
        /// <summary>
        /// 待办任务关闭
        /// </summary>
        /// <param name="strSystemCode">系统代号</param>
        /// <param name="List"></param>
        [OperationContract(IsOneWay = true, Name = "MsgListClose")]
        void MsgListClose(string strSystemCode, List<ReceiveUserForm> List);

        /// <summary>
        /// 待办任务模块关闭
        /// </summary>
        /// <param name="strModelCode">模块代号</param>
        /// <param name="strReceiveUser">接收用户</param>
        [OperationContract(IsOneWay = true)]
        void ModelMsgClose(string strModelCode, string strReceiveUser);


        /// <summary>
        /// 关闭代办
        /// </summary>
        /// <param name="strModelCode"></param>
        /// <param name="fromID"></param>
        [OperationContract]
        void ClosedDoTaskFromID(string strModelCode, string fromID);

        /// <summary>
        /// 消息直接关闭
        /// </summary>
        /// <param name="strMessageID"></param>
        /// <param name="strEventID"></param>
        [OperationContract(IsOneWay = true)]
        void MsgClose(string strMessageID, string strEventID);
        /// <summary>
        /// 发送待办（较旧接口） 没有公司过滤
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="ModelCode">模块代号</param>
        /// <param name="strXml">业务数据XML<</param>
        /// <param name="msgType">消息类型</param>

        [OperationContract(IsOneWay = true)]
        void ApplicationMsgTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strXml, MsgType msgType);


        /// <summary>
        /// 发送待办(根据生成的待办提醒消息不能自定义－新增)
        /// </summary>
        /// <param name="UserAndForm">用户ID和FORMID</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="strXml">将业务数据XM</param>
        /// <param name="msgType">消息类型： Msg消息  ; Task代办任务; Cancel撤消 </param>
        /// <param name="messageBody">用户自定义的消息实体，由业务系统确定，不再用数据库表的默认消息</param>
        [OperationContract(IsOneWay = true)]
        void ApplicationMsgTriggerCustom(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strXml, MsgType msgType, string messageBody);
        /// <summary>
        /// 发送待办(默认消息需要公司ID)
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="ModelCode">模块代号</param>
        /// <param name="strCompanyID">公司ID</param>
        /// <param name="strXml">业务数据XML</param>
        /// <param name="msgType">消息类型</param>
        [OperationContract(IsOneWay = true)]
        void ApplicationEngineTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strCompanyID, string strXml, MsgType msgType);
        /// <summary>
        /// 发送待办
        /// </summary>
        /// <param name="ReceiveUserID"></param>
        /// <param name="strFormID"></param>
        /// <param name="MsgContent"></param>
        /// <param name="ModelCode"></param>
        /// <param name="strXml"></param>
        /// <param name="strNewGuid"></param>
        [OperationContract]
        void AppMsgTrigger(string ReceiveUserID, string strFormID, string MsgContent, string ModelCode, string strXml, string strNewGuid);
        /// <summary>
        /// 发送待办
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="Content">消息内容</param>
        [OperationContract(IsOneWay = true)]
        void ApplicationNotesTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string Content);
        /// <summary>
        /// 自定义流程列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        [OperationContract]
        List<T_FLOW_CUSTOMFLOWDEFINE> CustomFlowDefineList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCompanyID);
        /// <summary>
        /// 发起自定义流程（任务系统）
        /// </summary>
        /// <param name="define"></param>
        [OperationContract(IsOneWay = true)]
        void CallCustomFlowTrigger(T_FLOW_CUSTOMFLOWDEFINE define);

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailParams"></param>
        [OperationContract(IsOneWay = true)]
        void SendMail(List<MailParams> mailParams);
        [OperationContract]
        bool SendMailParam(List<MailParams> mailParams, string mailType, string mailParameter);

        /// <summary>
        /// 流程撤消
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strAppXml"></param>
        /// <param name="msgType"></param>
        [OperationContract]
        bool FlowCancel(string strFlowXML, string strAppXml);
        /// <summary>
        /// 流程咨讯
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="SystemCode"></param>
        /// <param name="ModelCode"></param>
        /// <param name="strCompanyID"></param>
        /// <param name="strXml"></param>
        /// <param name="msgType"></param>
        [OperationContract]
        bool FlowConsultati(List<CustomUserMsg> UserAndForm, string strTitle, string strFlowXML, string strAppXml);
        [OperationContract(IsOneWay = true)]
        void FlowConsultatiClose(string strSystemCode, string strFormID, string strReceiveUser);
        /// <summary>
        /// 删除待办(删除该系统指定的接收用户单据)
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="strSystemCode"></param>
        [OperationContract(IsOneWay = true, Name = "TaskDelete")]
        void TaskDelete(List<CustomUserMsg> UserAndForm, string strSystemCode);
        /// <summary>
        /// 删除待办（删除该系统中指定的单据）
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        [OperationContract(IsOneWay = true, Name = "TaskDeleteALL")]
        void TaskDelete(string strSystemCode, string strFormID, string strReceiveID);

        /// <summary>
        /// 新增消息(2013-05-06加入新的方法)
        /// </summary>
        /// <param name="list">UserID|FromID XML</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelCode">模块代码</param>
        [OperationContract]
        string SendTaskMessage(List<CustomUserMsg> list, string SystemCode, string ModelCode);

        [OperationContract]
        string WFAddTimingTrigger(T_WF_TIMINGTRIGGERACTIVITY entity);

        [OperationContract]
        string AddTask(T_WF_DOTASK dask);

        [OperationContract]
        T_WF_DOTASK GetDoTaskEntity(string orderID, string receiveUserID);
        /// <summary>
        /// 删除定时触发
        /// </summary>
        /// <param name="orderID"></param>
        [OperationContract]
        void DeleteTrigger(string orderID);

        [OperationContract]
        void AddDoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType);

        [OperationContract]
        List<UserInfo> ReturnUserInfoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType);
        [OperationContract(IsOneWay = true)]
        void ApplicationMsgTriggerNew(List<CustomUserMsg> UserAndForm, string companyID, string SystemCode, string ModelCode, string strXml, MsgType msgType);

        #region 待办缓存信息接口
        //[OperationContract]
        //Dictionary<string, TaskCacheEntity> ListCache();
        //[OperationContract]
        //void RemoveCache();
        #endregion



    }
}
