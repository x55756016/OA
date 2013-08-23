using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Activities;
using System.Workflow.ComponentModel.Compiler;
using System.Threading;
using SMT.WFLib;
using System.Collections.ObjectModel;
using SMT.EntityFlowSys;
using WFTools.Services;
using WFTools.Services.Persistence.Ado;
using System.Configuration;
using WFTools.Services.Tracking.Ado;
using WFTools.Services.Batching.Ado;
using System.Xml;
using System.IO;
using System.Transactions;
using SMT.Foundation.Log;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Activities.Rules;
using System.Xml.Linq;
using System.Diagnostics;

using SMT.FLOWDAL.ADO;
using System.Data.OracleClient;
using SMT.Workflow.Common.Model.FlowEngine;




namespace SMT.FlowWFService
{
    // 注意: 如果更改此处的类名“IService1”，也必须更新 App.config 中对“IService1”的引用。
    public class Service : IService
    {
        private static string _strIsFlowEngine;
        private static string strIsFlowEngine
        {
            get
            {
                if (_strIsFlowEngine == null)
                {
                    _strIsFlowEngine = ConfigurationManager.AppSettings["IsFlowEngine"];
                    if (_strIsFlowEngine == null)
                    {
                        _strIsFlowEngine = "";
                    }
                }
                return _strIsFlowEngine;
            }
        }




        #region 咨询
        public void AddConsultation(FLOW_CONSULTATION_T flowConsultation, SubmitData submitData)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            s2.AddConsultation(con,flowConsultation,  submitData);
            #region 旧代码
            //if (strIsFlowEngine.ToLower() == "true")
            //{
            //    try
            //    {
            //        flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T =
            //            FlowBLL.GetFLOW_FLOWRECORDMASTER_T(flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID);
            //        FlowEngineService.EngineWcfGlobalFunctionClient FlowEngine = new FlowEngineService.EngineWcfGlobalFunctionClient();
            //        FlowEngineService.CustomUserMsg[] cs = new FlowEngineService.CustomUserMsg[1];

            //        FlowEngineService.CustomUserMsg cu = new FlowEngineService.CustomUserMsg();
            //        cu.FormID = flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FORMID;
            //        cu.UserID = flowConsultation.REPLYUSERID;
            //        cs[0] = cu;
            //        ModelInfo modelinfo = FlowBLL.GetSysCodeByModelCode(submitData.ModelCode);
            //        MessageData tmpMessageData = new MessageData("Flow", modelinfo.SysCode,
            //            flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID,
            //            submitData.ModelCode, modelinfo.ModelName, submitData.FormID, flowConsultation.FLOW_FLOWRECORDDETAIL_T.STATECODE, flowConsultation.FLOW_FLOWRECORDDETAIL_T.CHECKSTATE, "", "", "", "");
            //        FlowBLL flowBLL = new FlowBLL();
            //        StringBuilder FlowResultXml = flowBLL.BuildMessageData(tmpMessageData);
            //        //FlowEngine = new FlowEngineService.EngineWcfGlobalFunctionClient();
            //        //log = FlowEngine.SaveFlowTriggerData(FlowResultXml.ToString(), submitData.XML);
            //        if (!string.IsNullOrEmpty(flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT))
            //        {
            //            submitData.XML = flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT;
            //        }

            //        bool b = FlowEngine.FlowConsultati(cs, "", FlowResultXml.ToString(), submitData.XML);
            //        if (!b)
            //        {
            //            Tracer.Debug("FlowEngineService-FlowConsultati:" + flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FORMID
            //                + "\nsubmitData:" + submitData.XML);
            //        }

            //        FlowBLL bll = new FlowBLL();

            //        bll.AddConsultation(flowConsultation);
            //    }
            //    catch (Exception ex)
            //    {
            //        Tracer.Debug("AddConsultation: -" + flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FORMID + "-" + ex.InnerException + ex.Message);
            //        throw ex;
            //    }
            //}

            #endregion

        }
        public void ReplyConsultation(FLOW_CONSULTATION_T flowConsultation, SubmitData submitData)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            s2.ReplyConsultation(con, flowConsultation, submitData);
            #region 旧代码
            //if (strIsFlowEngine.ToLower() == "true")
            //{
            //    try
            //    {


            //        FlowEngineService.EngineWcfGlobalFunctionClient FlowEngine = new FlowEngineService.EngineWcfGlobalFunctionClient();
            //        //Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(submitData.XML);
            //        //XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
            //        //string strSystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;


            //        ModelInfo modelinfo = FlowBLL.GetSysCodeByModelCode(submitData.ModelCode);
            //        FlowEngine.FlowConsultatiClose(modelinfo.SysCode, flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FORMID, flowConsultation.REPLYUSERID);

            //        FlowBLL bll = new FlowBLL();
            //        bll.ReplyConsultation(flowConsultation);
            //    }
            //    catch (Exception ex)
            //    {
            //        Tracer.Debug("ReplyConsultation: -" + flowConsultation.FLOW_FLOWRECORDDETAIL_T.FLOW_FLOWRECORDMASTER_T.FORMID + "-" + ex.InnerException + ex.Message);
            //        throw ex;
            //    }
            //}
            #endregion
        }
        #endregion

        #region 流程处理




        #region 流程与任务审批


        public DataResult SubimtFlow(SubmitData submitData)
        {
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.SubimtFlow(submitData);
            #region 旧代码
            //DateTime dtStart = DateTime.Now;
            //DateTime dtEngineStart = DateTime.Now;
            //DateTime dtEnd = DateTime.Now;
            //DateTime dtCheckData = DateTime.Now;
            ////using (TransactionScope ts = new TransactionScope())
            ////设置2分钟超时时间
            //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(DateTime.Now.AddMinutes(2).Ticks)))
            //{
            //    //System.Threading.Thread.Sleep(60000);
            //    DataResult dataResult = new DataResult();
            //    FlowBLL Flowbill = new FlowBLL();
            //    string AppCompanyID = "";  //申请公司
            //    string MessageUserID = ""; //申请人ID
            //    string MessageUserName = ""; //申请人名
            //    dataResult.FlowResult = FlowResult.SUCCESS;
            //    try
            //    {
            //        #region 检查流程数据是否规范
            //        if (!FlowBLL.CheckFlowData(submitData, ref dataResult))
            //        {
            //            dataResult.FlowResult = FlowResult.FAIL;
            //            {
            //                ts.Complete();
            //                return dataResult;
            //            }
            //        }
            //        #endregion
            //        submitData.ApprovalResult = submitData.SubmitFlag == SubmitFlag.New ? ApprovalResult.Pass : submitData.ApprovalResult;
            //        submitData.FlowSelectType = submitData.FlowSelectType == null ? FlowSelectType.FixedFlow : submitData.FlowSelectType;
            //        #region
            //        #region 检查是否已提交流程
            //        CheckResult CheckFlowResult = Flowbill.CheckFlow2(submitData, dataResult);
            //        dtCheckData = DateTime.Now;
            //        dataResult = CheckFlowResult.APPDataResult;
            //        if (CheckFlowResult.Flag == 0)
            //        {
            //            ts.Complete();
            //            return dataResult;
            //        }
            //        #endregion
            //        dataResult.AppState = submitData.NextStateCode;
            //        //提交新流程
            //        if (submitData.SubmitFlag == SubmitFlag.New)
            //        {
            //            #region 新增
            //            AppCompanyID = submitData.ApprovalUser.CompanyID;
            //            if (submitData.FlowSelectType == FlowSelectType.FreeFlow)
            //                //自选流程
            //                dataResult = Flowbill.AddFreeFlow(submitData, dataResult);
            //            else
            //            {
            //                //固定流程
            //                dataResult = Flowbill.AddFlow2(submitData, dataResult);
            //            }
            //            #endregion

            //        }
            //        else if (submitData.SubmitFlag == SubmitFlag.Cancel)
            //        {
            //            #region 撤单
            //            if (!string.IsNullOrEmpty(CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT))
            //            {
            //                submitData.XML = CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT;
            //            }
            //            submitData.ApprovalContent = "";
            //            dataResult = Flowbill.CancelFlow(submitData, dataResult, CheckFlowResult.fd);
            //            dataResult.SubmitFlag = submitData.SubmitFlag;
            //            #endregion
            //        }

            //        //审批流程
            //        else
            //        {
            //            #region  审核
            //            if (!string.IsNullOrEmpty(CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT))
            //            {
            //                submitData.XML = CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT;
            //            }
            //            //引擎消息数据
            //            AppCompanyID = CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID;
            //            MessageUserID = CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.CREATEUSERID;
            //            MessageUserName = CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.CREATEUSERNAME;
            //            submitData.ApprovalUser.CompanyID = CheckFlowResult.fd[0].EDITCOMPANYID;
            //            submitData.ApprovalUser.DepartmentID = CheckFlowResult.fd[0].EDITDEPARTMENTID;
            //            submitData.ApprovalUser.PostID = CheckFlowResult.fd[0].EDITPOSTID;
            //            submitData.FlowSelectType = (FlowSelectType)Convert.ToInt32(CheckFlowResult.fd[0].FLOW_FLOWRECORDMASTER_T.FLOWSELECTTYPE);
            //            if (submitData.FlowSelectType == FlowSelectType.FreeFlow)
            //                dataResult = Flowbill.ApprovalFreeFlow(submitData, dataResult, CheckFlowResult.fd);
            //            else
            //                dataResult = Flowbill.ApprovalFlow2(submitData, dataResult, CheckFlowResult.fd);
            //            #endregion
            //        }
            //        #endregion
            //        ModelInfo modelinfo = FlowBLL.GetSysCodeByModelCode(submitData.ModelCode);
            //        ts.Complete(); //提交事务
            //        bool bOK = true;
            //        if (string.IsNullOrEmpty(strIsFlowEngine) || strIsFlowEngine.ToLower() == "true")
            //        {
            //            dtEngineStart = DateTime.Now;
            //            #region 发送审批消息
            //            try
            //            {
            //                if (dataResult.FlowResult == FlowResult.SUCCESS || dataResult.FlowResult == FlowResult.END)
            //                {
            //                    FlowEngineService.EngineWcfGlobalFunctionClient FlowEngine = new FlowEngineService.EngineWcfGlobalFunctionClient();
            //                    string IsTask = dataResult.FlowResult == FlowResult.SUCCESS ? "1" : "0";
            //                    MessageData tmpMessageData = null;
            //                    StringBuilder FlowResultXml = null;
            //                    #region
            //                    switch (submitData.SubmitFlag)
            //                    {
            //                        case SubmitFlag.New:
            //                            #region 新增
            //                            if (dataResult.IsCountersign)
            //                            {
            //                                #region
            //                                if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                {
            //                                    List<string> listUserID = new List<string>();
            //                                    List<string> listUserName = new List<string>();
            //                                    if (dataResult.DictCounterUser != null && dataResult.DictCounterUser.Keys.Count > 0)
            //                                    {
            //                                        dataResult.DictCounterUser.Values.ToList().ForEach(users =>
            //                                        {
            //                                            users.ForEach(user =>
            //                                            {
            //                                                listUserID.Add(user.UserID);
            //                                                listUserName.Add(user.UserName);
            //                                            });
            //                                        });

            //                                        MessageUserID = string.Join("|", listUserID);
            //                                        MessageUserName = string.Join("|", listUserName);
            //                                    }

            //                                }
            //                                if (dataResult.DictAgentUserInfo != null && dataResult.DictAgentUserInfo.Count > 0)
            //                                {
            //                                    dataResult.DictAgentUserInfo.Values.ToList().ForEach(user =>
            //                                    {
            //                                        MessageUserID += "|" + user.UserID.Trim();
            //                                        MessageUserName += "|" + user.UserName.Trim();

            //                                    });
            //                                }
            //                                #endregion
            //                            }
            //                            else
            //                            {
            //                                #region
            //                                if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                {
            //                                    MessageUserID = dataResult.UserInfo[0].UserID.Trim();
            //                                    MessageUserName = dataResult.UserInfo[0].UserName.Trim();
            //                                }
            //                                if (dataResult.AgentUserInfo != null)
            //                                {
            //                                    MessageUserID += "|" + dataResult.AgentUserInfo.UserID.Trim();
            //                                    MessageUserName += "|" + dataResult.AgentUserInfo.UserName.Trim();
            //                                }
            //                                #endregion
            //                            }
            //                            tmpMessageData = new MessageData("Flow", modelinfo.SysCode, AppCompanyID, submitData.ModelCode, modelinfo.ModelName,
            //                                submitData.FormID, dataResult.AppState, dataResult.CheckState, IsTask, MessageUserID, MessageUserName, dataResult.KPITime);
            //                            FlowResultXml = Flowbill.BuildMessageData(tmpMessageData);
            //                            bOK = FlowEngine.SaveFlowTriggerData(FlowResultXml.ToString(), submitData.XML);
            //                            if (!bOK)
            //                            {
            //                                Tracer.Debug("FlowEngineService:FormID-" + submitData.FormID + "--submitDataXML:" + submitData.XML);
            //                            }

            //                            #endregion
            //                            break;
            //                        case SubmitFlag.Cancel:
            //                            #region 撤单
            //                            tmpMessageData = new MessageData("Flow", modelinfo.SysCode, AppCompanyID, submitData.ModelCode, modelinfo.ModelName,
            //                                submitData.FormID, dataResult.AppState, dataResult.CheckState, IsTask, MessageUserID, MessageUserName, dataResult.KPITime);
            //                            FlowResultXml = Flowbill.BuildMessageData(tmpMessageData);
            //                            bool bCancel = FlowEngine.FlowCancel(FlowResultXml.ToString(), submitData.XML);
            //                            if (!bCancel)
            //                            {
            //                                Tracer.Debug("FlowEngineService:DateTime:" + DateTime.Now.ToString() + "\n" + "FlowCancel:submitData.XML" + "\n\nFlowResultXml" + FlowResultXml.ToString());
            //                            }
            //                            #endregion
            //                            break;
            //                        case SubmitFlag.Approval:
            //                            #region 审核
            //                            FlowEngine.TaskMsgClose(modelinfo.SysCode, submitData.FormID, submitData.ApprovalUser.UserID);
            //                            if (dataResult.CurrentIsCountersign)
            //                            {
            //                                #region
            //                                if (submitData.ApprovalResult == ApprovalResult.NoPass)
            //                                {
            //                                    #region
            //                                    List<string> listMessageUserID = new List<string>();
            //                                    CheckFlowResult.fd.Where(detail => detail.EDITUSERID != submitData.ApprovalUser.UserID).ToList().ForEach(item =>
            //                                    {
            //                                        listMessageUserID.Add(item.EDITUSERID);
            //                                        if (!string.IsNullOrEmpty(item.AGENTUSERID))
            //                                        {
            //                                            listMessageUserID.Add(item.AGENTUSERID);
            //                                        }
            //                                    });
            //                                    if (listMessageUserID.Count > 0)
            //                                    {
            //                                        string messageUserID = string.Join(",", listMessageUserID);
            //                                        FlowEngine.TaskMsgClose(modelinfo.SysCode, submitData.FormID, messageUserID);
            //                                    }
            //                                    tmpMessageData = new MessageData("Flow", modelinfo.SysCode, AppCompanyID, submitData.ModelCode, modelinfo.ModelName,
            //                                  submitData.FormID, dataResult.AppState, dataResult.CheckState, IsTask, MessageUserID, MessageUserName, dataResult.KPITime);
            //                                    FlowResultXml = Flowbill.BuildMessageData(tmpMessageData);
            //                                    bOK = FlowEngine.SaveFlowTriggerData(FlowResultXml.ToString(), submitData.XML);
            //                                    if (!bOK)
            //                                    {
            //                                        Tracer.Debug("FlowEngineService:FormID-" + submitData.FormID + "--submitDataXML:" + submitData.XML);
            //                                    }
            //                                    #endregion
            //                                }
            //                                else if (dataResult.IsGotoNextState)
            //                                {
            //                                    #region
            //                                    List<string> listMessageUserID = new List<string>();
            //                                    CheckFlowResult.fd.Where(detail => detail.EDITUSERID != submitData.ApprovalUser.UserID).ToList().ForEach(item =>
            //                                    {
            //                                        listMessageUserID.Add(item.EDITUSERID);
            //                                        if (!string.IsNullOrEmpty(item.AGENTUSERID))
            //                                        {
            //                                            listMessageUserID.Add(item.AGENTUSERID);
            //                                        }
            //                                    });
            //                                    if (listMessageUserID.Count > 0)
            //                                    {
            //                                        string messageUserID = string.Join(",", listMessageUserID);
            //                                        FlowEngine.TaskMsgClose(modelinfo.SysCode, submitData.FormID, messageUserID);
            //                                    }
            //                                    if (dataResult.IsCountersign)
            //                                    {
            //                                        #region
            //                                        if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                        {

            //                                            List<string> listUserID = new List<string>();
            //                                            List<string> listUserName = new List<string>();
            //                                            if (dataResult.DictCounterUser != null && dataResult.DictCounterUser.Keys.Count > 0)
            //                                            {
            //                                                dataResult.DictCounterUser.Values.ToList().ForEach(users =>
            //                                                {
            //                                                    users.ForEach(user =>
            //                                                    {
            //                                                        listUserID.Add(user.UserID);
            //                                                        listUserName.Add(user.UserName);
            //                                                    });
            //                                                });

            //                                                MessageUserID = string.Join("|", listUserID);
            //                                                MessageUserName = string.Join("|", listUserName);
            //                                            }

            //                                        }
            //                                        if (dataResult.DictAgentUserInfo != null && dataResult.DictAgentUserInfo.Count > 0)
            //                                        {
            //                                            dataResult.DictAgentUserInfo.Values.ToList().ForEach(user =>
            //                                            {
            //                                                MessageUserID += "|" + user.UserID.Trim();
            //                                                MessageUserName += "|" + user.UserName.Trim();

            //                                            });
            //                                        }
            //                                        #endregion
            //                                    }
            //                                    else
            //                                    {
            //                                        #region
            //                                        if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                        {
            //                                            MessageUserID = dataResult.UserInfo[0].UserID.Trim();
            //                                            MessageUserName = dataResult.UserInfo[0].UserName.Trim();

            //                                        }

            //                                        if (dataResult.AgentUserInfo != null)
            //                                        {
            //                                            MessageUserID += "|" + dataResult.AgentUserInfo.UserID.Trim();
            //                                            MessageUserName += "|" + dataResult.AgentUserInfo.UserName.Trim();
            //                                        }
            //                                        #endregion
            //                                    }
            //                                    tmpMessageData = new MessageData("Flow", modelinfo.SysCode, AppCompanyID, submitData.ModelCode, modelinfo.ModelName,
            //                                        submitData.FormID, dataResult.AppState, dataResult.CheckState, IsTask, MessageUserID, MessageUserName, dataResult.KPITime);
            //                                    FlowResultXml = Flowbill.BuildMessageData(tmpMessageData);
            //                                    bOK = FlowEngine.SaveFlowTriggerData(FlowResultXml.ToString(), submitData.XML);
            //                                    if (!bOK)
            //                                    {
            //                                        Tracer.Debug("FlowEngineService:FormID-" + submitData.FormID + "--submitDataXML:" + submitData.XML);
            //                                    }
            //                                    #endregion
            //                                }
            //                                #endregion
            //                            }
            //                            else
            //                            {
            //                                #region
            //                                if (dataResult.IsCountersign)
            //                                {
            //                                    #region
            //                                    if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                    {

            //                                        List<string> listUserID = new List<string>();
            //                                        List<string> listUserName = new List<string>();
            //                                        if (dataResult.DictCounterUser != null && dataResult.DictCounterUser.Keys.Count > 0)
            //                                        {
            //                                            dataResult.DictCounterUser.Values.ToList().ForEach(users =>
            //                                            {
            //                                                users.ForEach(user =>
            //                                                {
            //                                                    listUserID.Add(user.UserID);
            //                                                    listUserName.Add(user.UserName);
            //                                                });
            //                                            });

            //                                            MessageUserID = string.Join("|", listUserID);
            //                                            MessageUserName = string.Join("|", listUserName);
            //                                        }

            //                                    }
            //                                    if (dataResult.DictAgentUserInfo != null && dataResult.DictAgentUserInfo.Count > 0)
            //                                    {
            //                                        dataResult.DictAgentUserInfo.Values.ToList().ForEach(user =>
            //                                        {
            //                                            MessageUserID += "|" + user.UserID.Trim();
            //                                            MessageUserName += "|" + user.UserName.Trim();

            //                                        });
            //                                    }
            //                                    #endregion
            //                                }
            //                                else
            //                                {
            //                                    #region
            //                                    if (dataResult.FlowResult == FlowResult.SUCCESS)
            //                                    {
            //                                        MessageUserID = dataResult.UserInfo[0].UserID.Trim();
            //                                        MessageUserName = dataResult.UserInfo[0].UserName.Trim();

            //                                    }

            //                                    if (dataResult.AgentUserInfo != null)
            //                                    {
            //                                        MessageUserID += "|" + dataResult.AgentUserInfo.UserID.Trim();
            //                                        MessageUserName += "|" + dataResult.AgentUserInfo.UserName.Trim();
            //                                    }
            //                                    #endregion
            //                                }
            //                                tmpMessageData = new MessageData("Flow", modelinfo.SysCode, AppCompanyID, submitData.ModelCode, modelinfo.ModelName,
            //                                    submitData.FormID, dataResult.AppState, dataResult.CheckState, IsTask, MessageUserID, MessageUserName, dataResult.KPITime);
            //                                FlowResultXml = Flowbill.BuildMessageData(tmpMessageData);
            //                                bOK = FlowEngine.SaveFlowTriggerData(FlowResultXml.ToString(), submitData.XML);
            //                                if (!bOK)
            //                                {
            //                                    Tracer.Debug("FlowEngineService:FormID-" + submitData.FormID + "--submitDataXML:" + submitData.XML);
            //                                }
            //                                #endregion
            //                            }
            //                            #endregion
            //                            break;
            //                    }
            //                    #endregion
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                Tracer.Debug("FlowEngineService: -" + "\n\nError:" + ex.InnerException + ex.Message);
            //            }
            //            #endregion
            //        }
            //        dtEnd = DateTime.Now;
            //        dataResult.SubmitFlag = submitData.SubmitFlag;
            //        return dataResult;
            //    }
            //    catch (Exception ex)
            //    {
            //        ts.Dispose();
            //        dataResult.RunTime += "---FlowEnd:" + DateTime.Now.ToString();
            //        dataResult.FlowResult = FlowResult.FAIL;
            //        Flowbill = null;
            //        dataResult.Err = ex.Message;
            //        Tracer.Debug("FlowService: -" + submitData.FormID + "--submitDataXML:" + submitData.XML + "-" + ex.InnerException + ex.Message);
            //        return dataResult;
            //    }
            //    finally
            //    {
            //        dataResult = null;
            //        Tracer.Debug("-------Trace--FormID:" + submitData.FormID + "DateTime: Start:" + dtStart.ToString() + "  EngineStart:" + dtEngineStart.ToString() + "  End:" + dtEnd.ToString() + "\n");


            //    }
            //}
            #endregion
        }
        #endregion


        /// <summary>
        /// 检测用户是否有未处理的单据
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        //public bool IsExistFlowDataByUserID(string UserID)
        //{
        //    try
        //    {
        //        FlowBLL Flow = new FlowBLL();
        //        return Flow.IsExistFlowDataByUserID(UserID);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("IsExistFlowDataByUserID:" + UserID + " Ex:" + ex.Message);
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// 获取用户有哪些未处理的单据
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<FLOW_FLOWRECORDMASTER_T> GetFlowDataByUserID(string UserID)
        {
            try
            {
                FlowBLL Flow = new FlowBLL();
                return Flow.GetFlowDataByUserID(UserID);
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetFlowDataByUserID:" + UserID + " Ex:" + ex.Message);
                throw ex;
            }
        }
        #endregion



        #region 查询下一节点用户

        /// <summary>
        /// 启动与工作流程相同类型流程，查询对应节点用户
        /// </summary>
        /// <param name="CompanyID">公司ID</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="FlowGUID">待审批流GUID，新增时为空或者为StartFlow</param>
        /// <returns></returns>
        public DataResult GetAppUser(string CompanyID, string ModelCode, string FlowGUID, string xml)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetAppUser(con, CompanyID,  ModelCode,  FlowGUID,  xml);
            #region 旧代码
            //DataResult GetAppUserResult = new DataResult();
            //try
            //{
            //    string StateName = null;


            //    if (FlowGUID == "" || FlowGUID == "StartFlow")
            //    {
            //        StateName = "StartFlow";
            //    }
            //    else
            //    {
            //        //根据待审批流程GUID,检索待审批状态节点代码
            //        List<FLOW_FLOWRECORDDETAIL_T> FlowRecord = FlowBLL.GetFlowInfo("", FlowGUID, "", "", "", "", "", null);
            //        if (FlowRecord == null)
            //        {
            //            GetAppUserResult.Err = "没有待处理的审核";
            //            GetAppUserResult.UserInfo = null;
            //            return GetAppUserResult;
            //        }
            //        StateName = FlowRecord[0].STATECODE;
            //    }

            //    //根据公司ID，模块代码获取配置的流程
            //    WorkflowInstance instance = null;
            //    List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = FlowBLL.GetFlowByModelName(CompanyID, "", ModelCode, "0");

            //    if (MODELFLOWRELATION == null || MODELFLOWRELATION.Count == 0)
            //    {
            //        GetAppUserResult.Err = "没有可使用的流程";
            //        GetAppUserResult.UserInfo = null;
            //        return GetAppUserResult;
            //    }
            //    FLOW_FLOWDEFINE_T Xoml = MODELFLOWRELATION[0].FLOW_FLOWDEFINE_T;

            //    XmlReader readerxoml, readerule;
            //    StringReader strXoml = new StringReader(Xoml.XOML);
            //    StringReader strRules = new StringReader(Xoml.RULES == null ? "" : Xoml.RULES);

            //    readerxoml = XmlReader.Create(strXoml);
            //    readerule = XmlReader.Create(strRules);

            //    WorkflowRuntime workflowRuntime = new WorkflowRuntime();
            //    workflowRuntime.StartRuntime();

            //    FlowEvent ExternalEvent = new FlowEvent();
            //    ExternalDataExchangeService objService = new ExternalDataExchangeService();
            //    workflowRuntime.AddService(objService);
            //    objService.AddService(ExternalEvent);
            //    TypeProvider typeProvider = new TypeProvider(null);
            //    workflowRuntime.AddService(typeProvider);

            //    //XmlReader readerxoml = XmlReader.Create(HttpContext.Current.Server.MapPath ("TestFlow.xml"));
            //    // instance = workflowRuntime.CreateWorkflow(readerxoml);
            //    if (Xoml.RULES == null)
            //        instance = workflowRuntime.CreateWorkflow(readerxoml);
            //    else
            //        instance = workflowRuntime.CreateWorkflow(readerxoml, readerule, null);
            //    // instance = workflowRuntime.CreateWorkflow(typeof(TestFlow));
            //    instance.Start();
            //    StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);

            //    //从实例中获取定义
            //    if (1 == 2)
            //    {
            //        System.Workflow.Activities.StateMachineWorkflowActivity smworkflow = new StateMachineWorkflowActivity();
            //        smworkflow = workflowinstance.StateMachineWorkflow;
            //        RuleDefinitions ruleDefinitions = smworkflow.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            //        WorkflowMarkupSerializer markupSerializer = new WorkflowMarkupSerializer();

            //        StringBuilder xoml = new StringBuilder();
            //        StringBuilder rule = new StringBuilder();
            //        XmlWriter xmlWriter = XmlWriter.Create(xoml);
            //        XmlWriter ruleWriter = XmlWriter.Create(rule);
            //        markupSerializer.Serialize(xmlWriter, smworkflow);
            //        markupSerializer.Serialize(ruleWriter, ruleDefinitions);
            //        xmlWriter.Close();
            //        ruleWriter.Close();
            //        StringReader readxoml = new StringReader(xoml.ToString());
            //        StringReader readrule = new StringReader(rule.ToString());
            //        XmlReader readerxoml2 = XmlReader.Create(readxoml);
            //        XmlReader readerrule2 = XmlReader.Create(readrule);
            //        WorkflowInstance instance1 = workflowRuntime.CreateWorkflow(readerxoml2, readerrule2, null);
            //        instance1.Start();
            //        StateMachineWorkflowInstance workflowinstance1 = new StateMachineWorkflowInstance(workflowRuntime, instance1.InstanceId);
            //        workflowinstance1.SetState(StateName);
            //    }
            //    //从实例中获取定义并启动新实例

            //    //跳转到节点StateName
            //    workflowinstance.SetState(StateName);

            //    FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
            //    FlowData.xml = xml;
            //    //  FlowData.Flow_FlowRecord_T = null;

            //    ExternalEvent.OnDoFlow(instance.InstanceId, FlowData);//激发流程引擎流转到下一状态
            //    System.Threading.Thread.Sleep(1000);
            //    PermissionServiceClient WcfPermissionService = new PermissionServiceClient();
            //    string CurrentStateName = workflowinstance.CurrentStateName == null ? "End" : workflowinstance.CurrentStateName; //取得当前状态
            //    List<UserInfo> listUser = new List<UserInfo>();
            //    if (CurrentStateName != "End")
            //    {
            //        if (CurrentStateName.Substring(0, 5) == "State")
            //        {
            //            CurrentStateName = CurrentStateName.Substring(5);
            //        }
            //        string WFCurrentStateName = new Guid(CurrentStateName).ToString("D");
            //        T_SYS_USER[]  User = WcfPermissionService.GetSysUserByRole(WFCurrentStateName); //检索本状态（角色）对应用户

            //        if (User != null)
            //            for (int i = 0; i < User.Length; i++)
            //            {
            //                UserInfo tmp = new UserInfo();
            //                tmp.UserID = User[i].EMPLOYEEID;
            //                tmp.UserName = User[i].EMPLOYEENAME;
            //                listUser.Add(tmp);
            //            }



            //    }
            //    else
            //    {
            //        //已经到流程结束状态
            //        UserInfo tmp = new UserInfo();
            //        tmp.UserID = "End";
            //        tmp.UserName = "End";

            //        listUser.Add(tmp);
            //    }


            //    GetAppUserResult.UserInfo = listUser.Count > 0 ? listUser : null;

            //    if (GetAppUserResult.UserInfo == null)
            //        GetAppUserResult.Err = "没有找到用户";

            //    return GetAppUserResult;
            //    // return listUser;


            //    //return workflowinstance.CurrentStateName == null ? "End" : workflowinstance.CurrentStateName;
            //}
            //catch (Exception ex)
            //{
            //    GetAppUserResult.Err = ex.Message;
            //    GetAppUserResult.UserInfo = null;
            //    return GetAppUserResult;
            //}
            #endregion
        }

        #endregion


        private bool GetUser(string OptFlag, string Xoml, string Rules, string xml, ref DataResult DataResult)
        {          
            #region 旧代码
            try
            {
                WorkflowRuntime WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                WorkflowInstance Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime, Xoml, Rules);
                string strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, "StartFlow", xml);


            }
            catch (Exception ex)
            {
            }
            finally
            {
            }

            return true;
            #endregion
        }

        public string UpdateFlow(FLOW_FLOWRECORDDETAIL_T entity)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.UpdateFlow(con, entity);
            #region 旧代码
            //FlowBLL bll = new FlowBLL();
            //bll.UpdateFlowRecord(entity, "", "");

            //return "";
            #endregion
        }

        #region 查询流程信息
        /// <summary>
        /// 查询审批流程
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            using(OracleConnection con = ADOHelper.GetOracleConnection())
            {
                SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
                return s2.GetFlowInfo(con,  FormID,  FlowGUID,  CheckState,  Flag,  ModelCode,  CompanyID,  EditUserID);
            }
            #region 旧代码
            //List<FLOW_FLOWRECORDDETAIL_T> list = new List<FLOW_FLOWRECORDDETAIL_T>();
            //try
            //{
            //    List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
            //    FlowTypeList.Add(FlowType.Approval);
            //    FlowTypeList.Add(FlowType.Pending);
            //    //Debug.WriteLine("GetFlowInfo\n");
            //    //Debug.WriteLine(DateTime.Now.ToString());
            //    //Debug.WriteLine("\n");
            //    //有formid和modelcode不对返回数据量作限制，否则只返回前20条master数据
            //    if (!string.IsNullOrEmpty(FormID) && !string.IsNullOrEmpty(ModelCode))
            //    {
            //        list = FlowBLL.GetFlowInfoV(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, FlowTypeList);
            //    }
            //    else
            //    {
            //        Tracer.Debug("GetFlowInfoTop: formID:" + FormID + "--FlowGuid:" + FlowGUID
            //            + "--CheckState:" + CheckState + "--Flag:" + Flag + "--ModelCode:" + ModelCode + "--CompanyID:" + CompanyID + "--EditUserID:" + EditUserID);
            //        list = FlowBLL.GetFlowInfoTop(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, FlowTypeList);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Tracer.Debug("GetFlowInfo: -" + FormID + "-" + ex.InnerException + "\n" + ex.Message);
            //    throw ex;

            //}

            //return list;
            #endregion
        }

        public List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordMaster(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetFlowRecordMaster(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            #region 旧代码
            //try
            //{
            //    return FlowBLL.GetFlowRecordMaster(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            //}
            //catch (Exception ex)
            //{
            //    Tracer.Debug("GetFlowRecordMaster: -" + ex.InnerException + ex.Message);
            //    throw ex;
            //}
            #endregion
            
        }

        /// <summary>
        /// 查询任务信息
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FlowGUID"></param>
        /// <param name="CheckState"></param>
        /// <param name="Flag"></param>
        /// <param name="ModelCode"></param>
        /// <param name="CompanyID"></param>
        /// <param name="EditUserID"></param>
        /// <returns></returns>
        public List<TaskInfo> GetTaskInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetTaskInfo(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            #region 旧代码
            //return FlowBLL.GetTaskInfo(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            #endregion
        }
        #endregion

        #region "查询待审核单据"
        /// <summary>
        /// 根据模块代码和用户id查询待审核单据
        /// </summary>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="EditUserID">用户id</param>
        /// <returns>Formids</returns>
        public List<string> GetWaitingApprovalForm(string ModelCode, string EditUserID)
        {
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetWaitingApprovalForm(ModelCode, EditUserID);
        }
        #endregion
        /// <summary>
        /// 通过机构ID和模块代码查询对应流程代码
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public List<FLOW_MODELFLOWRELATION_T> GetFlowByModel(FLOW_MODELFLOWRELATION_T entity)
        //{
        //    return FlowBLL.GetFlowByModel(entity);
        //}

        public string GetFlowDefine(SubmitData ApprovalData)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetFlowDefine(con, ApprovalData);
            #region 旧代码
            //FlowBLL Flow = new FlowBLL();
            //return Flow.GetFlowDefine(ApprovalData);
            #endregion
           
        }

        /// <summary>
        /// 对外接口：根据我的单据ID获取记录实体
        /// </summary>
        /// <param name="personalrecordid">我的单据ID</param>
        /// <returns></returns>
        public T_WF_PERSONALRECORD GetPersonalRecordByID(string personalrecordid)
        {
            SMT.FlowWFService.NewFlow.Service s2 = new SMT.FlowWFService.NewFlow.Service();
            return s2.GetPersonalRecordByID(personalrecordid);
        }

        public string IsExistFlowDataByUserID(string UserID, string PostID)
        {
            OracleConnection con = ADOHelper.GetOracleConnection();
            try
            {
                FlowBLL2 Flow = new FlowBLL2();
                return Flow.IsExistFlowDataByUserID(con, UserID, PostID);
            }
            catch (Exception ex)
            {
                Tracer.Debug("IsExistFlowDataByUserID:" + UserID + " Ex:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 根据流程ID获取流程的所有分支
        /// </summary>
        /// <param name="FlowID"></param>
        /// <returns></returns>
        public List<string> GetFlowBranch(string FlowID)
        {
            SMT.FlowWFService.NewFlow.Service Flow = new SMT.FlowWFService.NewFlow.Service();
            return Flow.GetFlowBranch(FlowID);
        }
        /// <summary>
        /// 判断是否可能用自选流程或提单人可以撒回流程
        /// string[0]=1 可以用自选流程
        /// string[1]=1 提交人可以撒回流程
        /// </summary>
        /// <param name="modelcode">模块代码</param>
        /// <param name="companyid">公司ID</param>       
        public string[] IsFreeFlowAndIsCancel(string modelcode, string companyid)
        {
            SMT.FlowWFService.NewFlow.Service Flow = new SMT.FlowWFService.NewFlow.Service();
            return Flow.IsFreeFlowAndIsCancel(modelcode, companyid);
        }
        public List<FLOW_MODELFLOWRELATION_T> GetModelFlowRelationInfosListBySearch(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount)
        {
            SMT.FlowWFService.NewFlow.Service Flow = new SMT.FlowWFService.NewFlow.Service();
            return Flow.GetModelFlowRelationInfosListBySearch( pageIndex,  pageSize,  sort,  filterString, paras, ref  pageCount); 
        }
        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="formid"></param>
        /// <returns></returns>       
        public string GetMetadataByFormid(string formid)
        {
            SMT.FlowWFService.NewFlow.Service Flow = new SMT.FlowWFService.NewFlow.Service();            
            return Flow.GetMetadataByFormid(formid);
        }
        /// <summary>
        /// 更新元数据
        /// </summary>
        /// <param name="formid"></param>
        /// <param name="xml"></param>
        /// <returns></returns>       
        public bool UpdateMetadataByFormid(string formid, string xml)
        {
            SMT.FlowWFService.NewFlow.Service Flow = new SMT.FlowWFService.NewFlow.Service();
            return Flow.UpdateMetadataByFormid(formid, xml);
        }

    }
}
