/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：EngineWcfGlobalFunction.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/23 11:38:56   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Engine.Services.BLL;
using SMT.Global.IEngineContract;
using EngineDataModel;
using SMT.Workflow.Common.Model;
using SMT.Workflow.SMTCache;
using System.Diagnostics;
using System.Web;
using System.Configuration;
using System.IO;
using System.ServiceModel.Description;
using SMT.SaaS.Common;

namespace SMT.Workflow.Engine.Services
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class EngineWcfGlobalFunction : IEngineWcfGlobalFunction
    {
        /// <summary>
        /// 流程触发接口（流程操作完成后调用，触发待办事项）
        /// </summary>
        /// <param name="strFlowMessage">流程信息数据</param>
        /// <param name="strBOObject">业务元数据</param>
        /// <returns></returns>
        public bool SaveFlowTriggerData(string strFlowMessage, string strBOObject)
        {
            Record.WriteLogFunction("SaveFlowTriggerData():strFlowMessage:" + strFlowMessage + "strBOObject:" + strBOObject + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.SaveFlowTriggerData(strFlowMessage, strBOObject);
        }

        /// <summary>
        /// 流程撤消
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strAppXml"></param>
        /// <param name="msgType"></param>    
        public bool FlowCancel(string strFlowXML, string strAppXml)
        {
            Record.WriteLogFunction("FlowCancel():strFlowXML:" + strFlowXML + "strAppXml:" + strAppXml + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.FlowCancel(strFlowXML, strAppXml);
        }
        /// <summary>
        /// 流程咨讯
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="SystemCode"></param>
        /// <param name="ModelCode"></param>
        /// <param name="strCompanyID"></param>
        /// <param name="strXml"></param>
        /// <param name="msgType"></param>
        public bool FlowConsultati(List<CustomUserMsg> UserAndForm, string strTitle, string strFlowXML, string strAppXml)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("FlowConsultati():UserAndForm:" + ss + "strTitle:" + strTitle + "strFlowXML:" + strFlowXML + "strAppXml:" + strAppXml + "");
            return true;
        }

        public void FlowConsultatiClose(string strSystemCode, string strFormID, string strReceiveUser)
        {
            Record.WriteLogFunction("FlowConsultatiClose():strSystemCode:" + strSystemCode + "strFormID:" + strFormID + "strReceiveUser:" + strReceiveUser + "");
        }

        /// <summary>
        /// 待办任务主数据（分页）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>      
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref int rowCount, ref int pageCount)
        {

            string ss = "测试一：BeginDate:" + msgParams.BeginDate + "||EndDate:" + msgParams.EndDate + "||LastDay:" + msgParams.LastDay;
            ss += "||MessageBody:" + msgParams.MessageBody + "||MessageId:" + msgParams.MessageId + "||PageIndex:" + msgParams.PageIndex;
            ss += "||PageSize:" + msgParams.PageSize + "||Status:" + msgParams.Status + "||Top:" + msgParams.Top;
            ss += "||UserID:" + msgParams.UserID + "";
            Record.WriteLogFunction("PendingTasksParmsPageIndex()msgParams:" + ss + "||rowCount:" + rowCount + "||pageCount:" + pageCount + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingTasksParmsPageIndex(msgParams, ref rowCount, ref pageCount);
        }


        /// <summary>
        /// 根据MessageId获取上一条、下一条记录
        /// </summary>
        /// <param name="msgParms"></param>
        /// <returns></returns>

        public Dictionary<string, T_FLOW_ENGINEMSGLIST> GetPendingTaskPrevNext(MsgParms msgParams)
        {
            string ss = "BeginDate:" + msgParams.BeginDate + "EndDate:" + msgParams.EndDate + "LastDay:" + msgParams.LastDay;
            ss += "MessageBody:" + msgParams.MessageBody + "MessageId:" + msgParams.MessageId + "PageIndex:" + msgParams.PageIndex;
            ss += "PageSize:" + msgParams.PageSize + "Status:" + msgParams.Status + "Top:" + msgParams.Top;
            ss += "UserID:" + msgParams.UserID + "";
            Record.WriteLogFunction("GetPendingTaskPrevNext()msgParams:" + ss + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.GetPendingTaskPrevNext(msgParams);
        }


        #region 平台所调方法

        /// <summary>
        /// 平台待办任务分页数据  kangxf
        /// </summary>
        /// <param name="msgParams"></param>
        /// ：BeginDate:0001-1-1 0:00:00||EndDate:0001-1-1 0:00:00||LastDay:0||MessageBody:||MessageId:||
        /// PageIndex:1||PageSize:14||Status:open||Top:0||UserID:f1a472dc-d1be-4a06-a52e-a2b296397704||IsAutofresh:True||HaveNewTask:False
        /// <param name="IsAutofresh"></param>
        /// <param name="HaveNewTask"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask, ref int rowCount, ref int pageCount)
        {
            //string ss = "测试二：BeginDate:" + msgParams.BeginDate + "||EndDate:" + msgParams.EndDate + "||LastDay:" + msgParams.LastDay;
            //ss += "||MessageBody:" + msgParams.MessageBody + "||MessageId:" + msgParams.MessageId + "||PageIndex:" + msgParams.PageIndex;
            //ss += "||PageSize:" + msgParams.PageSize + "||Status:" + msgParams.Status + "||Top:" + msgParams.Top;
            //ss += "||UserID:" + msgParams.UserID + "";
            //Record.WriteLogFunction("PendingTasksParmsPageIndex()msgParams:" + ss + "||IsAutofresh:" + IsAutofresh + "||HaveNewTask:" + HaveNewTask + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingTasksParmsPageIndex(msgParams, ref IsAutofresh, ref HaveNewTask, ref rowCount, ref pageCount);
        }
        /// <summary>
        /// 平台待办任务不第一次显示的数据，未分页 kangxf
        /// </summary>
        /// <param name="msgParams">
        ///  msgParams:BeginDate:0001-1-1 0:00:00EndDate:0001-1-1 0:00:00LastDay:0MessageBody:MessageId:PageIndex:0PageSize:0Status:openTop:20
        ///   UserID:1bc4b78a-d178-4499-b948-16e18a9d73d3
        /// </param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingMainTasksParms(msgParams);
        }

        /// <summary>
        /// 平台待办任务不第一次显示的数据，未分页 kangxf
        /// </summary>
        /// <param name="msgParams">msgParams:BeginDate:0001-1-1 0:00:00EndDate:0001-1-1 0:00:00LastDay:0MessageBody:MessageId:
        /// PageIndex:0PageSize:0Status:CloseTop:20UserID:1bc4b78a-d178-4499-b948-16e18a9d73d3IsAutofresh:TrueHaveNewTask:False</param>
        /// <param name="IsAutofresh">是否是门户自动刷新</param>
        /// <returns></returns>       
        public List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingMainTasksParms(msgParams, ref  IsAutofresh, ref  HaveNewTask);
        }
        /// <summary>
        /// 平台单击链接时候调用的方法（单条明细数据）  kangxf
        /// </summary>
        /// <param name="strMsgID">PendingDetailTasks()strMsgID:ef4b1b4e-2987-40c0-8a00-30ffe98f4d95</param>
        /// <returns></returns>       
        public T_FLOW_ENGINEMSGLIST PendingDetailTasks(string strMsgID)
        {
            //Record.WriteLogFunction("PendingDetailTasks()strMsgID:" + strMsgID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingDetailTasks(strMsgID);
        }
        #endregion


        /// <summary>
        /// 待办任务细数据（带业务数据字段）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>      
        public T_FLOW_ENGINEMSGLIST PendingDetailByID(string strMsgID)
        {
            Record.WriteLogFunction("PendingDetailByID()strMsgID:" + strMsgID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingDetailByID(strMsgID);
        }



        #region  针对手机的接口 陈新海
        /// <summary>
        /// 手机版待办任务主数据（分页）kangxf
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndexForMobile(MsgParms msgParams, ref int rowCount, ref int pageCount)
        {
            //Record.WriteLogFunction("PendingTasksParmsPageIndexForMobile()msgParams:" + ss + "rowCount:" + rowCount + "pageCount:" + pageCount + "");
            //日志内容:PendingTasksParmsPageIndexForMobile()msgParams:BeginDate:0001-1-1 0:00:00EndDate:0001-1-1 0:00:00LastDay:0MessageBody:MessageId:PageIndex:1PageSize:10000Status:close
            //Top:0UserID:1bc4b78a-d178-4499-b948-16e18a9d73d3rowCount:0pageCount:0
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingTasksParmsPageIndexForMobile(msgParams, ref rowCount, ref pageCount);
        }

        /// <summary>
        /// 待办任务详细数据（手机接口）kangxf  strMsgID:d967d709-405c-4aa6-8654-ce16ad729365
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>      
        public T_FLOW_ENGINEMSGLIST PendingDetailTasksByPhone(string strMsgID)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingDetailTasks(strMsgID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNodeCode">OA_d967d709-405c-4aa6-8654-ce16ad729365</param>
        /// <returns></returns>
        public T_FLOW_ENGINEMSGLIST PendingDetailTasksByOrderNodeCodeForMobile(string orderNodeCode)
        {
            Record.WriteLogFunction("PendingDetailTasksByOrderNodeCodeForMobile()orderNodeCode:" + orderNodeCode + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.PendingDetailTasksByPhone(orderNodeCode);
        }
        #endregion   针对手机的接口 陈新海



        /// <summary>
        /// 获取消息接口（老接口）
        /// </summary>
        /// <param name="strUserID">接收用户</param>
        /// <param name="strStatus">状态（Open,Close）</param>
        /// <param name="iTop">条目</param>
        /// <returns></returns>       
        public List<T_FLOW_ENGINENOTES> EngineNotes(string strUserID, string strStatus, int iTop)
        {
            Record.WriteLogFunction("EngineNotes()strUserID:" + strUserID + "strStatus:" + strStatus + "iTop:" + iTop + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.EngineNotes(strUserID, strStatus, iTop);
        }

        /// <summary>
        /// 取消事件定时
        /// </summary>
        /// <param name="strApplicationOrderCode">业务系统Guid</param>
        /// <returns></returns>      
        public bool CancelEventTriggerData(string strApplicationOrderCode)
        {
            Record.WriteLogFunction("CancelEventTriggerData()strApplicationOrderCode:" + strApplicationOrderCode + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.CancelEventTriggerData(strApplicationOrderCode);
        }

        /// <summary>
        /// 消息触发（任务，任务汇报）
        /// </summary>      
        public void MsgTrigger(string[] recevieUser, string SystemCode, string MsgKey, string ID, string strXml)
        {
            string ss = "";
            for (int i = 0; i < recevieUser.Length - 1; i++)
            {
                ss += "recevieUser:" + recevieUser[i] + "";
            }
            Record.WriteLogFunction("MsgTrigger()SystemCode:" + SystemCode + "MsgKey:" + MsgKey + "ID" + ID + "strXml" + strXml + "recevieUser" + recevieUser + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.MsgTrigger(recevieUser, SystemCode, MsgKey, ID, strXml);
        }

        public void NotesSend(List<ReceiveUserAndContent> List, string SystemCode, string strFormID)
        {
            string ss = "";
            foreach (ReceiveUserAndContent list in List)
            {
                ss += "Content:" + list.Content + "ReceiveUser:" + list.ReceiveUser;
            }
            Record.WriteLogFunction("NotesSend()SystemCode:" + SystemCode + "List:" + ss + "strFormID" + strFormID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.NotesSend(List, SystemCode, strFormID);
        }

        /// <summary>
        /// 待办任务关闭(单个关闭)
        /// </summary>
        /// <param name="strSystemCode">系统代号</param>
        /// <param name="strFormID">单据ID</param>
        /// <param name="strReceiveUser">接收用户</param>       
        public void TaskMsgClose(string strSystemCode, string strFormID, string strReceiveUser)
        {
            Record.WriteLogFunction("TaskMsgClose()strSystemCode:" + strSystemCode + "strFormID:" + strFormID + "strReceiveUser:" + strReceiveUser + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.TaskMsgClose(strSystemCode, strFormID, strReceiveUser);
        }

        /// <summary>
        /// 待办任务关闭
        /// </summary>
        /// <param name="strSystemCode">系统代号</param>
        /// <param name="List"></param>       
        public void MsgListClose(string strSystemCode, List<ReceiveUserForm> List)
        {
            string ss = "";
            foreach (ReceiveUserForm list in List)
            {
                ss += "FormID:" + list.FormID + "ReceiveUser:" + list.ReceiveUser;
            }
            Record.WriteLogFunction("MsgListClose()strSystemCode:" + strSystemCode + "List:" + ss + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.MsgListClose(strSystemCode, List);
        }

        /// <summary>
        /// 待办任务模块关闭
        /// </summary>
        /// <param name="strModelCode">模块代号</param>
        /// <param name="strReceiveUser">接收用户</param>       
        public void ModelMsgClose(string strModelCode, string strReceiveUser)
        {
            Record.WriteLogFunction("ModelMsgClose()strModelCode:" + strModelCode + "strReceiveUser:" + strReceiveUser + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ModelMsgClose(strModelCode, strReceiveUser);
        }


        /// <summary>
        /// 代办任务关闭
        /// </summary>
        /// <param name="listOrderID"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strReceiveUser"></param>
        public void CloseDoTask(List<string> listOrderID, string strModelCode, string strReceiveUser)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ClosedDoTaskOrderID(listOrderID, strModelCode, strReceiveUser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strModelCode"></param>
        /// <param name="fromID"></param>
        public void ClosedDoTaskFromID(string strModelCode, string fromID)
        {
            Record.WriteLogFunction("ClosedDoTaskFromID()strModelCode:" + strModelCode + "fromID:" + fromID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ClosedDoTaskFromID(strModelCode, fromID);
        }
        /// <summary>
        /// 消息直接关闭
        /// </summary>
        /// <param name="strMessageID"></param>
        /// <param name="strEventID"></param>       
        public void MsgClose(string strMessageID, string strEventID)
        {
            Record.WriteLogFunction("MsgClose()strMessageID:" + strMessageID + "strEventID:" + strEventID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.MsgClose(strMessageID, strEventID);
        }
        /// <summary>
        /// 发送待办（较旧接口） 没有公司过滤
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="ModelCode">模块代号</param>
        /// <param name="strXml">业务数据XML<</param>
        /// <param name="msgType">消息类型</param>     
        public void ApplicationMsgTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strXml, MsgType msgType)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("ApplicationMsgTrigger()UserAndForm:" + ss + "SystemCode:" + SystemCode + "ModelCode:" + ModelCode + "strXml:" + strXml + "msgType:" + msgType.ToString() + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ApplicationMsgTrigger(UserAndForm, SystemCode, ModelCode, strXml, msgType);
        }

        /// <summary>
        /// 发送待办(根据生成的待办提醒消息不能自定义－新增)
        /// </summary>
        /// <param name="UserAndForm">用户ID和FORMID</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="strXml">将业务数据XM</param>
        /// <param name="msgType">消息类型： Msg消息  ; Task代办任务; Cancel撤消 </param>
        /// <param name="messageBody">用户自定义的消息实体，由业务系统确定，不再用数据库表的默认消息</param>
        public void ApplicationMsgTriggerCustom(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strXml, MsgType msgType, string messageBody)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("ApplicationMsgTriggerCustom()UserAndForm:" + ss + "SystemCode:" + SystemCode + "ModelCode:" + ModelCode + "strXml:" + strXml + "msgType:" + msgType.ToString() + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ApplicationMsgTriggerCustom(UserAndForm, SystemCode, ModelCode, strXml, msgType, messageBody);


        }

        /// <summary>
        /// 发送待办(默认消息需要公司ID)
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="ModelCode">模块代号</param>
        /// <param name="strCompanyID">公司ID</param>
        /// <param name="strXml">业务数据XML</param>
        /// <param name="msgType">消息类型</param>
        public void ApplicationEngineTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strCompanyID, string strXml, MsgType msgType)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("ApplicationEngineTrigger()UserAndForm:" + ss + "SystemCode:" + SystemCode + "ModelCode:" + ModelCode + "strCompanyID:" + strCompanyID + "strXml:" + strXml + "msgType:" + msgType.ToString() + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ApplicationEngineTrigger(UserAndForm, SystemCode, ModelCode, strCompanyID, strXml, msgType);
        }
        /// <summary>
        /// 发送待办
        /// </summary>
        /// <param name="ReceiveUserID"></param>
        /// <param name="strFormID"></param>
        /// <param name="MsgContent"></param>
        /// <param name="ModelCode"></param>
        /// <param name="strXml"></param>
        /// <param name="strNewGuid"></param>       
        public void AppMsgTrigger(string ReceiveUserID, string strFormID, string MsgContent, string ModelCode, string strXml, string strNewGuid)
        {
            Record.WriteLogFunction("AppMsgTrigger()ReceiveUserID:" + ReceiveUserID + "strFormID:" + strFormID + "MsgContent:" + MsgContent + "strXml:" + strXml + "strNewGuid:" + strNewGuid + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.AppMsgTrigger(ReceiveUserID, strFormID, MsgContent, ModelCode, strXml, strNewGuid);
        }
        /// <summary>
        /// 发送待办
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="Content">消息内容</param>        
        public void ApplicationNotesTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string Content)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("ApplicationNotesTrigger()UserAndForm:" + ss + "SystemCode:" + SystemCode + "Content:" + Content + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ApplicationNotesTrigger(UserAndForm, SystemCode, Content);
        }


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
        public List<T_FLOW_CUSTOMFLOWDEFINE> CustomFlowDefineList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCompanyID)
        {
            string ww = "";
            foreach (object s in paras)
            {
                ww += s.ToString();
            }
            Record.WriteLogFunction("CustomFlowDefineList()pageIndex:" + pageIndex + "pageSize:" + pageSize + "sort:" + sort + "filterString:" + filterString + "strCompanyID:" + strCompanyID + "paras:" + ww + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.CustomFlowDefineList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCompanyID);
        }
        /// <summary>
        /// 发起自定义流程（任务系统）
        /// </summary>
        /// <param name="define"></param>       
        public void CallCustomFlowTrigger(T_FLOW_CUSTOMFLOWDEFINE define)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.CallCustomFlowTrigger(define);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailParams"></param>       
        public void SendMail(List<MailParams> mailParams)
        {
            string ss = "";
            foreach (MailParams list in mailParams)
            {
                ss += "MailContent:" + list.MailContent + "MailTitle:" + list.MailTitle + "ReceiveUserMail:" + list.ReceiveUserMail;
            }
            Record.WriteLogFunction("SendMail()mailParams:" + ss + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.SendMail(mailParams);
        }
        public bool SendMailParam(List<MailParams> mailParams, string mailType, string mailParameter)
        {
            try
            {
                EngineServicesBLL bll = new EngineServicesBLL();
                return bll.SendMailParam(mailParams, mailType, mailParameter);
            }
            catch (Exception ex)
            {
                Record.WriteLogFunction("SendMailParam()Exception:" + ex.Message + "");
                return false;
            }
        }

        /// <summary>
        /// 删除待办(删除该系统指定的接收用户单据)
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="strSystemCode"></param>       
        public void TaskDelete(List<CustomUserMsg> UserAndForm, string strSystemCode)
        {
            string ss = "";
            foreach (CustomUserMsg list in UserAndForm)
            {
                ss += "FormID:" + list.FormID + "UserID:" + list.UserID;
            }
            Record.WriteLogFunction("TaskDelete()UserAndForm:" + ss + "strSystemCode:" + strSystemCode + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.TaskDelete(UserAndForm, strSystemCode);
        }
        /// <summary>
        /// 删除待办（删除该系统中指定的单据）
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>        
        public void TaskDelete(string strSystemCode, string strFormID, string strReceiveID)
        {
            Record.WriteLogFunction("TaskDelete()strSystemCode:" + strSystemCode + "strFormID:" + strFormID + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.TaskDelete(strSystemCode, strFormID, strReceiveID);
        }

        #region 待办缓存信息接口

        //public Dictionary<string, TaskCacheEntity> ListCache()
        //{
        //    Record.WriteLogFunction("ListCache()");
        //    //Dictionary<string, CacheObject<TaskCacheEntity>> dic = TaskCache.ListCacheInfo();
        //    Dictionary<string, TaskCacheEntity> list = new Dictionary<string, TaskCacheEntity>();
        //    //foreach (var v in dic)
        //    //{
        //    //    list.Add(v.Key, v.Value.Object);
        //    //}
        //    return list;
        //}
        //public void RemoveCache()
        //{
        //    Record.WriteLogFunction("RemoveCache()");
        //    //TaskCache.RemoveAllCache();
        //}
        #endregion

        #region 定时触发的接口

        /// <summary>
        /// 新增消息(2013-05-06加入新的方法)
        /// </summary>
        /// <param name="list">UserID|FromID XML</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelCode">模块代码</param>
        public string SendTaskMessage(List<CustomUserMsg> list, string SystemCode, string ModelCode)
        {
            try
            {
                Record.WriteLogFunction("开始调用SendTaskMessage()ModelCode:" + ModelCode + "");
                EngineServicesBLL bll = new EngineServicesBLL();
                bll.SendTaskMessage(list, SystemCode, ModelCode);
                return "1";
            }
            catch (Exception ex)
            {
                Record.WriteLogFunction("新增方法SendTaskMessage():" + ex.Message + "");
                return ex.Message;
            }
        }

        /// <summary>
        /// 新增定时触发
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string WFAddTimingTrigger(T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            try
            {
                Record.WriteLogFunction("WFAddTimingTrigger()开始调用");
                EngineServicesBLL bll = new EngineServicesBLL();
                bll.AddTimingTrigger(entity);
                return "1";
            }
            catch (Exception ex)
            {
                Record.WriteLogFunction("WFAddTimingTrigger()" + ex.Message);
                return ex.Message;
            }
        }

        /// <summary>
        /// 新增未提交的代办
        /// </summary>
        /// <param name="dask"></param>
        /// <returns></returns>
        public string AddTask(T_WF_DOTASK dask)
        {
            try
            {
                Record.WriteLogFunction("AddTask()开始调用");
                EngineServicesBLL bll = new EngineServicesBLL();
                bll.AddTask(dask);
                return "1";
            }
            catch (Exception ex)
            {
                Record.WriteLogFunction("AddTask()" + ex.Message);
                return ex.Message;
            }
        }



        /// <summary>
        /// 删除定时触发
        /// </summary>
        /// <param name="orderID"></param>
        public void DeleteTrigger(string orderID)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.DeleteTrigger(orderID);
        }

        /// <summary>
        /// 事件定时触发
        /// </summary>
        /// <param name="strEventXml"></param>       
        public void SaveEventData(string strEventXml)
        {
            Record.WriteLogFunction("SaveEventData()strEventXml:" + strEventXml + "");
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.SaveEventData(strEventXml);
        }
        #endregion

        #region 新增接口
        public T_WF_DOTASK GetDoTaskEntity(string orderID, string receiveUserID)
        {
            try
            {
                EngineServicesBLL bll = new EngineServicesBLL();
                return bll.GetDoTaskEntity(orderID, receiveUserID);
            }
            catch (Exception ex)
            {
                Record.WriteLogFunction("GetDoTaskEntity()orderID:" + orderID + "receiveUserID:" + receiveUserID + "||Message:" + ex.Message + "||StackTrace:" + ex.StackTrace);
                return null;
            }
        }

        public void AddDoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType)
        {
            Record.WriteLogFunction("AddDoDask()modelCode||" + modelCode);
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.AddDoDask(companyID, orderID, systemCode, modelCode, modelName, strXML, msgType);
        }
        public List<UserInfo> ReturnUserInfoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType)
        {
            Record.WriteLogFunction("ReturnUserInfoDask()modelCode||" + modelCode);
            EngineServicesBLL bll = new EngineServicesBLL();
            return bll.ReturnUserInfoDask(companyID, orderID, systemCode, modelCode, modelName, strXML, msgType);
        }
        /// <summary>
        /// 发送待办(李国艺要求可以同一张单可以发送给多个人)
        /// </summary>
        /// <param name="UserAndForm">接收用户与FormID</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="ModelCode">模块代号</param>
        /// <param name="strXml">业务数据XML<</param>
        /// <param name="msgType">消息类型</param>     
        public void ApplicationMsgTriggerNew(List<CustomUserMsg> UserAndForm, string companyID, string SystemCode, string ModelCode, string strXml, MsgType msgType)
        {
            EngineServicesBLL bll = new EngineServicesBLL();
            bll.ApplicationMsgTriggerNew(UserAndForm, companyID, SystemCode, ModelCode, strXml, msgType);
        }
        #endregion

        public void TaskCacheReflesh(string userID)
        {
            TaskCache.TaskCacheReflesh(userID);
        }
        public bool RemoveCache(string userID)
        {
            TaskCache.RemoveCache(userID);
            return true;
        }

    }
}
