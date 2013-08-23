using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Workflow.Runtime;
using SMT.EntityFlowSys;
using SMT.FLOWDAL;
using System.Collections.ObjectModel;
using System.Workflow.Activities;
using SMT.WFLib;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using SMT.FlowWFService.PublicClass;
using System.ServiceModel.Description;

using SMT.Foundation.Log;
using SMT.FLOWDAL.ADO;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.OrganizationWS;
using SMT.SaaS.BLLCommonServices.PersonnelWS;

namespace SMT.FlowWFService
{
    public class FlowBLL
    {
        #region 龙康才新增
      
        #endregion
        #region 咨询
        public void AddConsultation(FLOW_CONSULTATION_T flowConsultation)
        {
            FLOW_CONSULTATION_TDAL.Add(flowConsultation);
            //FLOWDAL.FLOW_CONSULTATION_TDAL dal = new FLOW_CONSULTATION_TDAL();
            //dal.AddConsultation(flowConsultation);
        }
        public void ReplyConsultation(FLOW_CONSULTATION_T flowConsultation)
        {
            FLOW_CONSULTATION_TDAL.Update(flowConsultation);
            //FLOWDAL.FLOW_CONSULTATION_TDAL dal = new FLOW_CONSULTATION_TDAL();
            //dal.ReplyConsultation(flowConsultation);
        }

        public static FLOW_FLOWRECORDMASTER_T GetFLOW_FLOWRECORDMASTER_T(string masterID)
        {
            return FLOW_FLOWRECORDMASTER_TDAL.GetFLOW_FLOWRECORDMASTER_T(masterID);
        }
        #endregion 

        #region 处理审批数据

        
        /// <summary>
        /// 流程数据处理(对应SubmitFlow)
        /// </summary>
        /// <param name="workflowRuntime"></param>
        /// <param name="instance"></param>
        /// <param name="entity"></param>
        /// <param name="NextStateCode"></param>
        /// <param name="EditUserId"></param>
        /// <param name="EditUserName"></param>
        /// <param name="SubmitFlag"></param>
        /// <param name="FlowType"></param>
        /// <returns></returns>
        public DataResult DoFlowRecord(WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, UserInfo AppUser, UserInfo AgentUser, SubmitFlag SubmitFlag, FlowType FlowType)
        {
            DataResult tmpDataResult = new DataResult();
            UserInfo tmpUserInfo = AppUser;

            //tmpUserInfo.UserID = EditUserId;
            //tmpUserInfo.UserName = EditUserName;

            tmpDataResult.UserInfo.Add(tmpUserInfo);
            try
            {

                if (SubmitFlag == SubmitFlag.New)
                {
                    #region 新增流程
                    //添加启动状态
                    
                    entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID = Guid.NewGuid().ToString();
                    
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "1";

                    entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID = entity.CREATEPOSTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDATE = DateTime.Now;

                    entity.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    // entity.FLOW_FLOWRECORDMASTER_T .INSTANCEID  = instance.InstanceId.ToString();
                    entity.FLAG = "1";
                    entity.CHECKSTATE = "1";

                    //entity.FlowCode = "TestFlow";  //正式使用时关屏蔽

                    entity.STATECODE = "StartFlow";
                    entity.PARENTSTATEID = entity.FLOWRECORDDETAILID;

                    entity.CREATEDATE = DateTime.Now;
                    entity.EDITDATE = DateTime.Now;
                    entity.EDITUSERID = entity.CREATEUSERID;
                    entity.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.EDITCOMPANYID = entity.CREATECOMPANYID;
                    entity.EDITDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.EDITPOSTID = entity.CREATEPOSTID;

                    #region 引擎自动提交时停留在提交人处

                    if (FlowType == FlowType.Pending)
                    {
                        entity.FLAG = "0";
                        entity.EDITUSERID = AppUser.UserID;
                        entity.EDITUSERNAME = AppUser.UserName;
                        entity.EDITCOMPANYID = AppUser.CompanyID;
                        entity.EDITDEPARTMENTID = AppUser.DepartmentID;
                        entity.EDITPOSTID = AppUser.PostID;
                        FLOW_FLOWRECORDMASTER_TDAL.Add(entity.FLOW_FLOWRECORDMASTER_T);
                        AddFlowRecord(entity, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        return tmpDataResult;
                    }

                    #endregion

                    FLOW_FLOWRECORDMASTER_TDAL.Add(entity.FLOW_FLOWRECORDMASTER_T);
                    AddFlowRecord(entity, NextStateCode, AppUser.UserID);

                    

                    FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();

                    //添加下一状态
                    entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                    entity2.STATECODE = NextStateCode == "" ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                    entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;//entity.StateCode;

                    //entity2.Content = ".";
                    entity2.FLAG = "0";
                    entity2.CHECKSTATE = "2";
                    entity2.CREATEPOSTID = entity.CREATEPOSTID;
                    entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity2.CREATEUSERID = entity.EDITUSERID;
                    entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                    entity2.CREATEDATE = DateTime.Now;
                    entity2.EDITUSERID = AppUser.UserID;
                    entity2.EDITUSERNAME = AppUser.UserName;
                    entity2.EDITCOMPANYID = AppUser.CompanyID;
                    entity2.EDITDEPARTMENTID = AppUser.DepartmentID;
                    entity2.EDITPOSTID = AppUser.PostID;
                    entity2.EDITDATE = DateTime.Now;

                    if (AgentUser != null)  //如果启用了代理，把代理人信息写入
                    {
                        entity2.AGENTUSERID = AgentUser.UserID;
                        entity2.AGENTERNAME = AgentUser.UserName;
                        entity2.AGENTEDITDATE = DateTime.Now;
                    }

                    tmpDataResult.AppState = entity2.STATECODE;

                    if (entity2.STATECODE != "EndFlow")
                    {
                        AddFlowRecord(entity2, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                    }
                    else
                    {
                        tmpDataResult.CheckState = "2";
                        tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                    }

                    return tmpDataResult;

                    #endregion
                }
                else
                {
                    #region 更新流程
                    //如果NextStateCode为空则自动获取模型中的下一状态，如果不为空则使用传入状态代码，并使用传入下一任务人ID

                    //更新本流程



                    entity = UpdateFlowRecord(entity, NextStateCode, AppUser.UserID);

                    //添加下一状态
                    FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();

                    //添加下一状态
                    entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();


                    if (NextStateCode != "")
                    {
                        entity2.STATECODE = NextStateCode;
                        //entity2.EditUserID = EditUserId;
                        //entity2.EditUserName = EditUserName;
                    }
                    else
                    {
                        entity2.STATECODE = SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE);
                        // entity2.EditUserID = entity2.StateCode=="EndFlow" ? "" : "EditUserId"; //根据状态查询权限表中用户ID
                    }

                    if (entity2.STATECODE == "EndFlow")
                    {

                        entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "2"; //设为终审通过
                        if (entity.EDITDATE == entity.AGENTEDITDATE)  //代理审核时
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.AGENTUSERID;
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.AGENTERNAME;
                        }
                        else   //正常审核时
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.EDITUSERID;
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.EDITUSERNAME;
                        }

                        entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;

                        UpdateFlowRecord(entity, NextStateCode, AppUser.UserID);
                        FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    }



                    
                    entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;// entity.StateCode;

                    //entity2.Content = "";
                    entity2.FLAG = "0";
                    entity2.CHECKSTATE = "2";
                    entity2.CREATEPOSTID = entity.CREATEPOSTID;
                    entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;

                    //if (entity.EDITDATE == entity.AGENTEDITDATE) //代理审核时
                    //{
                    //    entity2.CREATEUSERID = entity.AGENTUSERID;
                    //    entity2.CREATEUSERNAME = entity.AGENTERNAME;
                    //}
                    //else   //正常审核时
                    //{
                        entity2.CREATEUSERID = entity.EDITUSERID;
                        entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                    //}

                    entity2.EDITUSERID = AppUser.UserID;
                    entity2.EDITUSERNAME = AppUser.UserName;
                    entity2.EDITCOMPANYID = AppUser.CompanyID;
                    entity2.EDITDEPARTMENTID = AppUser.DepartmentID;
                    entity2.EDITPOSTID = AppUser.PostID;

                    entity2.CREATEDATE = DateTime.Now;

                    entity2.EDITDATE = DateTime.Now;
                    if (AgentUser != null)  //如果启用了代理，把代理人信息写入
                    {
                        entity2.AGENTUSERID = AgentUser.UserID;
                        entity2.AGENTERNAME = AgentUser.UserName;
                        entity2.AGENTEDITDATE = DateTime.Now;
                    }

                    tmpDataResult.AppState = entity2.STATECODE;

                    if (entity2.STATECODE != "EndFlow")
                    {
                        entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                        AddFlowRecord(entity2, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";

                    }
                    else
                    {
                        tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                        tmpDataResult.CheckState = "2";
                    }

                    return tmpDataResult;   //如有下一节点，返回SUCCESS

                    #endregion

                }
            }
            catch (Exception ex)
            {
                throw new Exception("DoFlowRecord:" + ex.InnerException + ex.Message );
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }

        public DataResult DoFlowRecord2(WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, UserInfo AppUser, UserInfo AgentUser, SubmitFlag SubmitFlag, FlowType FlowType)
        {
            DataResult tmpDataResult = new DataResult();
            UserInfo tmpUserInfo = AppUser;

            //tmpUserInfo.UserID = EditUserId;
            //tmpUserInfo.UserName = EditUserName;

            tmpDataResult.UserInfo.Add(tmpUserInfo);
            try
            {

                if (SubmitFlag == SubmitFlag.New)
                {
                    #region 新增流程
                    //添加启动状态
                    if (SubmitFlag == FlowWFService.SubmitFlag.New)
                    {
                        entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID = Guid.NewGuid().ToString();
                    }
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "1";

                    entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID = entity.CREATEPOSTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDATE = DateTime.Now;

                    entity.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    // entity.FLOW_FLOWRECORDMASTER_T .INSTANCEID  = instance.InstanceId.ToString();
                    entity.FLAG = "1";
                    entity.CHECKSTATE = "1";

                    //entity.FlowCode = "TestFlow";  //正式使用时关屏蔽

                    entity.STATECODE = "StartFlow";
                    entity.PARENTSTATEID = entity.FLOWRECORDDETAILID;

                    entity.CREATEDATE = DateTime.Now;
                    entity.EDITDATE = DateTime.Now;
                    entity.EDITUSERID = entity.CREATEUSERID;
                    entity.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.EDITCOMPANYID = entity.CREATECOMPANYID;
                    entity.EDITDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.EDITPOSTID = entity.CREATEPOSTID;

                    #region 引擎自动提交时停留在提交人处

                    if (FlowType == FlowType.Pending)
                    {
                        entity.FLAG = "0";
                        entity.EDITUSERID = AppUser.UserID;
                        entity.EDITUSERNAME = AppUser.UserName;
                        entity.EDITCOMPANYID = AppUser.CompanyID;
                        entity.EDITDEPARTMENTID = AppUser.DepartmentID;
                        entity.EDITPOSTID = AppUser.PostID;
                        FLOW_FLOWRECORDMASTER_TDAL.Add(entity.FLOW_FLOWRECORDMASTER_T);
                        AddFlowRecord(entity, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        return tmpDataResult;
                    }

                    #endregion

                    FLOW_FLOWRECORDMASTER_TDAL.Add(entity.FLOW_FLOWRECORDMASTER_T);
                    AddFlowRecord(entity, NextStateCode, AppUser.UserID);
                   
                    FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();

                    //添加下一状态
                    entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                    entity2.STATECODE = NextStateCode == "" ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                    entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;//entity.StateCode;

                    //entity2.Content = ".";
                    entity2.FLAG = "0";
                    entity2.CHECKSTATE = "2";
                    entity2.CREATEPOSTID = entity.CREATEPOSTID;
                    entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity2.CREATEUSERID = entity.EDITUSERID;
                    entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                    entity2.CREATEDATE = DateTime.Now;
                    entity2.EDITUSERID = AppUser.UserID;
                    entity2.EDITUSERNAME = AppUser.UserName;
                    entity2.EDITCOMPANYID = AppUser.CompanyID;
                    entity2.EDITDEPARTMENTID = AppUser.DepartmentID;
                    entity2.EDITPOSTID = AppUser.PostID;
                    entity2.EDITDATE = DateTime.Now;

                    if (AgentUser != null)  //如果启用了代理，把代理人信息写入
                    {
                        entity2.AGENTUSERID = AgentUser.UserID;
                        entity2.AGENTERNAME = AgentUser.UserName;
                        entity2.AGENTEDITDATE = DateTime.Now;
                    }

                    tmpDataResult.AppState = entity2.STATECODE;

                    if (entity2.STATECODE != "EndFlow")
                    {
                        AddFlowRecord(entity2, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                    }
                    else
                    {
                        tmpDataResult.CheckState = "2";
                        tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                    }
                    tmpDataResult.IsCountersignComplete = true;
                    return tmpDataResult;

                    #endregion
                }
                
                else
                {
                    #region 更新流程
                    //如果NextStateCode为空则自动获取模型中的下一状态，如果不为空则使用传入状态代码，并使用传入下一任务人ID

                    //更新本流程



                    entity = UpdateFlowRecord(entity, NextStateCode, AppUser.UserID);
                    string stateCode = "";
                    if (NextStateCode.ToUpper() == "ENDFLOW")
                    {
                        stateCode = NextStateCode;
                    }
                    else
                    {
                       stateCode= string.IsNullOrEmpty(NextStateCode) ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                    }
                    if (stateCode == "EndFlow")
                    {
                        #region 
                        entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "2"; //设为终审通过
                        if (entity.EDITDATE == entity.AGENTEDITDATE)  //代理审核时
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.AGENTUSERID;
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.AGENTERNAME;
                        }
                        else   //正常审核时
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.EDITUSERID;
                            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.EDITUSERNAME;
                        }

                        entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;

                        UpdateFlowRecord(entity, NextStateCode, AppUser.UserID);

                        FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                        tmpDataResult.AppState = stateCode;
                        tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                        tmpDataResult.CheckState = "2";
                        #endregion 
                    }
                    else
                    {
                        #region 
                        //添加下一状态
                        FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();
                        //添加下一状态
                        entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                        if (NextStateCode != "")
                        {
                            entity2.STATECODE = NextStateCode;
                            //entity2.EditUserID = EditUserId;
                            //entity2.EditUserName = EditUserName;
                        }
                        else
                        {
                            entity2.STATECODE = stateCode;
                            // entity2.EditUserID = entity2.StateCode=="EndFlow" ? "" : "EditUserId"; //根据状态查询权限表中用户ID
                        }


                        entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                        entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;// entity.StateCode;

                        //entity2.Content = "";
                        entity2.FLAG = "0";
                        entity2.CHECKSTATE = "2";
                        entity2.CREATEPOSTID = entity.CREATEPOSTID;
                        entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                        entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;

                        //if (entity.EDITDATE == entity.AGENTEDITDATE) //代理审核时
                        //{
                        //    entity2.CREATEUSERID = entity.AGENTUSERID;
                        //    entity2.CREATEUSERNAME = entity.AGENTERNAME;
                        //}
                        //else   //正常审核时
                        //{
                        entity2.CREATEUSERID = entity.EDITUSERID;
                        entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                        //}

                        entity2.EDITUSERID = AppUser.UserID;
                        entity2.EDITUSERNAME = AppUser.UserName;
                        entity2.EDITCOMPANYID = AppUser.CompanyID;
                        entity2.EDITDEPARTMENTID = AppUser.DepartmentID;
                        entity2.EDITPOSTID = AppUser.PostID;

                        entity2.CREATEDATE = DateTime.Now;

                        entity2.EDITDATE = DateTime.Now;
                        if (AgentUser != null)  //如果启用了代理，把代理人信息写入
                        {
                            entity2.AGENTUSERID = AgentUser.UserID;
                            entity2.AGENTERNAME = AgentUser.UserName;
                            entity2.AGENTEDITDATE = DateTime.Now;
                        }

                        tmpDataResult.AppState = entity2.STATECODE;

                        AddFlowRecord(entity2, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        #endregion 
                    }

                    tmpDataResult.IsCountersignComplete = true;
                    return tmpDataResult;   //如有下一节点，返回SUCCESS

                    #endregion

                }
            }
            catch (Exception ex)
            {
                throw new Exception("DoFlowRecord2:" + ex.InnerException + ex.Message);
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }

        /// <summary>
        /// 会签
        /// </summary>
        /// <param name="workflowRuntime"></param>
        /// <param name="instance"></param>
        /// <param name="entity"></param>
        /// <param name="NextStateCode"></param>
        /// <param name="dictUserInfo"></param>
        /// <param name="dictAgentUserInfo"></param>
        /// <param name="SubmitFlag"></param>
        /// <param name="FlowType"></param>
        /// <returns></returns>

        public DataResult DoFlowRecord_Add(WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, Dictionary<Role_UserType, List<UserInfo>> dictUserInfo, Dictionary<UserInfo, UserInfo> dictAgentUserInfo, SubmitFlag SubmitFlag, FlowType FlowType)
        {
            DataResult tmpDataResult = new DataResult();
            tmpDataResult.DictCounterUser = dictUserInfo;
           
            try
            {
                if (SubmitFlag == SubmitFlag.New)
                {

                    #region 添加启动状态

                    entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID = Guid.NewGuid().ToString();
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "1";
                    entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID = entity.CREATEPOSTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDATE = DateTime.Now;

                    entity.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    // entity.FLOW_FLOWRECORDMASTER_T .INSTANCEID  = instance.InstanceId.ToString();
                    entity.FLAG = "1";
                    entity.CHECKSTATE = "1";
                    //entity.FlowCode = "TestFlow";  //正式使用时关屏蔽

                    entity.STATECODE = "StartFlow";
                    entity.PARENTSTATEID = entity.FLOWRECORDDETAILID;
                    entity.CREATEDATE = DateTime.Now;
                    entity.EDITDATE = DateTime.Now;
                    entity.EDITUSERID = entity.CREATEUSERID;
                    entity.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.EDITCOMPANYID = entity.CREATECOMPANYID;
                    entity.EDITDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.EDITPOSTID = entity.CREATEPOSTID;
                    FLOW_FLOWRECORDMASTER_TDAL.Add(entity.FLOW_FLOWRECORDMASTER_T);
                    AddFlowRecord2(entity);
                    
                    #endregion
                }
                else
                {
                    #region

                    //entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID = Guid.NewGuid().ToString();
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "1";
                    entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID = entity.CREATECOMPANYID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID = entity.CREATEPOSTID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.CREATEUSERID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.FLOW_FLOWRECORDMASTER_T.CREATEDATE = DateTime.Now;

                    entity.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                    // entity.FLOW_FLOWRECORDMASTER_T .INSTANCEID  = instance.InstanceId.ToString();
                    entity.FLAG = "1";
                    entity.CHECKSTATE = "1";
                    //entity.FlowCode = "TestFlow";  //正式使用时关屏蔽

                    entity.STATECODE = "StartFlow";
                    entity.PARENTSTATEID = entity.FLOWRECORDDETAILID;
                    entity.CREATEDATE = DateTime.Now;
                    entity.EDITDATE = DateTime.Now;
                    entity.EDITUSERID = entity.CREATEUSERID;
                    entity.EDITUSERNAME = entity.CREATEUSERNAME;
                    entity.EDITCOMPANYID = entity.CREATECOMPANYID;
                    entity.EDITDEPARTMENTID = entity.CREATEDEPARTMENTID;
                    entity.EDITPOSTID = entity.CREATEPOSTID;
                    entity.CHECKSTATE = "6";
                    entity.STATECODE = "ReSubmit";
                    entity.FLAG = "1";
                    AddFlowRecord2(entity);
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    #endregion 
                }
                System.Threading.Thread.Sleep(1000);
                string stateCode = NextStateCode == "" ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                tmpDataResult.AppState = stateCode;
                if (stateCode != "EndFlow")
                {
                    #region
                    dictUserInfo.Values.ToList().ForEach(users =>
                    {
                        users.ForEach(user =>
                        {
                            #region
                            FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();
                            entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                            entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                            entity2.STATECODE = stateCode;
                            entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;//entity.StateCode;                            
                            entity2.FLAG = "0";
                            entity2.CHECKSTATE = "2";
                            entity2.CREATEPOSTID = entity.CREATEPOSTID;
                            entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                            entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                            entity2.CREATEUSERID = entity.EDITUSERID;
                            entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                            entity2.CREATEDATE = DateTime.Now;
                            entity2.EDITUSERID = user.UserID;
                            entity2.EDITUSERNAME = user.UserName;
                            entity2.EDITCOMPANYID = user.CompanyID;
                            entity2.EDITDEPARTMENTID = user.DepartmentID;
                            entity2.EDITPOSTID = user.PostID;
                            entity2.EDITDATE = DateTime.Now;
                            if (dictAgentUserInfo.ContainsKey(user))
                            {
                                entity2.AGENTUSERID = dictAgentUserInfo[user].UserID;
                                entity2.AGENTERNAME = dictAgentUserInfo[user].UserName;
                                entity2.AGENTEDITDATE = DateTime.Now;
                            }
                            AddFlowRecord2(entity2);

                            #endregion
                        });
                    });
                    #endregion
                    tmpDataResult.AppState = stateCode;
                    tmpDataResult.FlowResult = FlowResult.SUCCESS;
                    tmpDataResult.CheckState = "1";
                }
                else
                {
                    tmpDataResult.CheckState = "2";
                    tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                }
                tmpDataResult.IsCountersignComplete = true;
                return tmpDataResult;

                
            }
            catch (Exception ex)
            {
                throw new Exception("DoFlowRecord_Add:" + ex.InnerException + ex.Message);
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }

        public DataResult DoFlowRecord_Approval(WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, Dictionary<Role_UserType, List<UserInfo>> dictUserInfo, Dictionary<UserInfo, UserInfo> dictAgentUserInfo, SubmitFlag SubmitFlag, FlowType FlowType)
        {
            DataResult tmpDataResult = new DataResult();
            tmpDataResult.DictCounterUser = dictUserInfo;

            try
            {


                #region 更新流程
                //如果NextStateCode为空则自动获取模型中的下一状态，如果不为空则使用传入状态代码，并使用传入下一任务人ID

                //更新本流程



                entity = UpdateFlowRecord2(entity);
                string stateCode = "";
                if (NextStateCode.ToUpper() == "ENDFLOW")
                {
                    stateCode = NextStateCode;
                }
                else
                {
                    stateCode = string.IsNullOrEmpty(NextStateCode) ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                }
                //string stateCode = NextStateCode == "" ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
                tmpDataResult.AppState = stateCode;

               

                if (stateCode == "EndFlow")
                {
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "2"; //设为终审通过
                    if (entity.EDITDATE == entity.AGENTEDITDATE)  //代理审核时
                    {
                        entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.AGENTUSERID;
                        entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.AGENTERNAME;
                    }
                    else   //正常审核时
                    {
                        entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.EDITUSERID;
                        entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.EDITUSERNAME;
                    }

                    entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;

                    UpdateFlowRecord2(entity);
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    tmpDataResult.FlowResult = FlowResult.END;   //如果没有下一处理节点，则返回END
                    tmpDataResult.CheckState = "2";
                }
                else
                {
                    dictUserInfo.Values.ToList().ForEach(users =>
                    {
                        users.ForEach(user =>
                        {
                            #region
                            //添加下一状态
                            //FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();

                            ////添加下一状态
                            //entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                            //entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                            //entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;// entity.StateCode;

                            ////entity2.Content = "";
                            //entity2.STATECODE = stateCode;
                            //entity2.FLAG = "0";
                            //entity2.CHECKSTATE = "2";
                            //entity2.CREATEPOSTID = entity.CREATEPOSTID;
                            //entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                            //entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;

                            ////if (entity.EDITDATE == entity.AGENTEDITDATE) //代理审核时
                            ////{
                            ////    entity2.CREATEUSERID = entity.AGENTUSERID;
                            ////    entity2.CREATEUSERNAME = entity.AGENTERNAME;
                            ////}
                            ////else   //正常审核时
                            ////{
                            //entity2.CREATEUSERID = entity.EDITUSERID;
                            //entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                            ////}

                            //entity2.EDITUSERID = user.UserID;
                            //entity2.EDITUSERNAME = user.UserName;
                            //entity2.EDITCOMPANYID = user.CompanyID;
                            //entity2.EDITDEPARTMENTID = user.DepartmentID;
                            //entity2.EDITPOSTID = user.PostID;

                            //entity2.CREATEDATE = DateTime.Now;

                            //entity2.EDITDATE = DateTime.Now;
                            //if (dictAgentUserInfo.ContainsKey(user))
                            //{
                            //    entity2.AGENTUSERID = dictAgentUserInfo[user].UserID;
                            //    entity2.AGENTERNAME = dictAgentUserInfo[user].UserName;
                            //    entity2.AGENTEDITDATE = DateTime.Now;
                            //}
                            //AddFlowRecord2(entity2);
                            
                            #endregion

                            #region
                            FLOW_FLOWRECORDDETAIL_T entity2 = new FLOW_FLOWRECORDDETAIL_T();
                            entity2.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
                            entity2.FLOW_FLOWRECORDMASTER_T = entity.FLOW_FLOWRECORDMASTER_T;
                            entity2.STATECODE = stateCode;
                            entity2.PARENTSTATEID = entity.FLOWRECORDDETAILID;//entity.StateCode;                            
                            entity2.FLAG = "0";
                            entity2.CHECKSTATE = "2";
                            entity2.CREATEPOSTID = entity.CREATEPOSTID;
                            entity2.CREATECOMPANYID = entity.CREATECOMPANYID;
                            entity2.CREATEDEPARTMENTID = entity.CREATEDEPARTMENTID;
                            entity2.CREATEUSERID = entity.EDITUSERID;
                            entity2.CREATEUSERNAME = entity.EDITUSERNAME;
                            entity2.CREATEDATE = DateTime.Now;
                            entity2.EDITUSERID = user.UserID;
                            entity2.EDITUSERNAME = user.UserName;
                            entity2.EDITCOMPANYID = user.CompanyID;
                            entity2.EDITDEPARTMENTID = user.DepartmentID;
                            entity2.EDITPOSTID = user.PostID;
                            entity2.EDITDATE = DateTime.Now;
                            if (dictAgentUserInfo.ContainsKey(user))
                            {
                                entity2.AGENTUSERID = dictAgentUserInfo[user].UserID;
                                entity2.AGENTERNAME = dictAgentUserInfo[user].UserName;
                                entity2.AGENTEDITDATE = DateTime.Now;
                            }
                            AddFlowRecord2(entity2);

                            #endregion
                        });
                    });

                    tmpDataResult.AppState = stateCode;
                    tmpDataResult.FlowResult = FlowResult.SUCCESS;
                    tmpDataResult.CheckState = "1";

                }

                tmpDataResult.IsCountersignComplete = true;
                return tmpDataResult;  

                #endregion


            }
            catch (Exception ex)
            {
                throw new Exception("DoFlowRecord_Approval:" + ex.InnerException + ex.Message);
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }


        #endregion

        #region 操作流程数据

        void AddFlowRecord(FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, string EditUserId)
        {
            //FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            //Dal.AddFlowRecord(entity);
            FLOW_FLOWRECORDDETAIL_TDAL.Add(entity);




        }
        void AddFlowRecord2(FLOW_FLOWRECORDDETAIL_T entity)
        {
            //FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            //Dal.AddFlowRecord(entity);
            FLOW_FLOWRECORDDETAIL_TDAL.Add(entity);

        }

        public FLOW_FLOWRECORDDETAIL_T UpdateFlowRecord(FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, string EditUserId)
        {
            FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();


            //List<FLOW_FLOWRECORDDETAIL_T> Temp = Dal.GetFlowRecord("", entity.FLOWRECORDDETAILID, "", "", "", "", "");
            //Temp[0].CONTENT = entity.CONTENT;
            //Temp[0].CHECKSTATE = entity.CHECKSTATE;
            //Temp[0].FLAG = "1";
            //Temp[0].EDITDATE = DateTime.Now;
            //if (entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE != null)
            //    Temp[0].FLOW_FLOWRECORDMASTER_T.CHECKSTATE = entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE;
            //if (entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID != null)
            //    Temp[0].FLOW_FLOWRECORDMASTER_T.EDITUSERID = entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID;
            //if (entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME != null)
            //    Temp[0].FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME;
            //if (entity.FLOW_FLOWRECORDMASTER_T.EDITDATE != null)
            //    Temp[0].FLOW_FLOWRECORDMASTER_T.EDITDATE = entity.FLOW_FLOWRECORDMASTER_T.EDITDATE;

            //entity = Temp[0];

            //  entity.EDITDATE = DateTime.Now;
            entity.FLAG = "1";
            FLOW_FLOWRECORDDETAIL_TDAL.Update(entity);
            //Dal.UpdateFlowRecord(entity);
            return entity;




        }

        public FLOW_FLOWRECORDDETAIL_T UpdateFlowRecord2(FLOW_FLOWRECORDDETAIL_T entity)
        {
            FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();


            entity.FLAG = "1";
            FLOW_FLOWRECORDDETAIL_TDAL.Update(entity);
            //Dal.UpdateFlowRecord2(entity);
            return entity;




        }

        #endregion

        #region 查询流程信息

       

        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {
            
            try
            {
                List<string> FlowTypeList = new List<string>();

                FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
                var dt = FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecord(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));

                if (dt.Count > 0)
                    return dt;
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFlowInfo:" + ex.Message);
            }
           

        }


        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfoV(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {
            
            try
            {
                
               

                
                var dt = FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecordV(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));

                if (dt.Count > 0)
                    return dt;
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFlowInfoV:" + ex.Message);
            }
           

        }

        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfoTop(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {
            return FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecordTop(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));
        }
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
        public static List<TaskInfo> GetTaskInfo(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            try
            {
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(FlowType.Task);
                List<FLOW_FLOWRECORDDETAIL_T> FLOWRECORDDETAIList = GetFlowInfo(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, FlowTypeList);
                if (FLOWRECORDDETAIList == null || FLOWRECORDDETAIList.Count == 0)
                    return null;

                string ACTIVEROLE = FLOWRECORDDETAIList[0].FLOW_FLOWRECORDMASTER_T.ACTIVEROLE;
                List<TaskInfo> TaskInfoList = new List<TaskInfo>();
                for (int i = 0; i < FLOWRECORDDETAIList.Count; i++)
                {
                    TaskInfo tmpTaskInfo = new TaskInfo();
                    tmpTaskInfo.FlowInfo = FLOWRECORDDETAIList[i];
                    tmpTaskInfo.SubModelCode = Utility.GetString(Utility.GetSubModelCode(ACTIVEROLE, FLOWRECORDDETAIList[i].STATECODE));
                    TaskInfoList.Add(tmpTaskInfo);
                }

                return TaskInfoList;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordMaster(string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            //FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            //var dt = Dal.GetFlowRecordMaster(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            List<FLOW_FLOWRECORDDETAIL_T> listDetail= FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecord(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, null);
            List<FLOW_FLOWRECORDMASTER_T> listMaster = new List<FLOW_FLOWRECORDMASTER_T>();
            listDetail.ForEach(detail =>
                {
                    if (listMaster.FirstOrDefault(d => d.FLOWRECORDMASTERID == detail.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID) == null)
                    {
                        listMaster.Add(detail.FLOW_FLOWRECORDMASTER_T);
                    }
                });
            return listMaster;
        }

        public static List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordBySubmitUserID(string CheckState, string EditUserID)
        {
            //FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            //var dt = Dal.GetFlowRecordBySubmitUserID( CheckState,EditUserID);
            var dt = FLOW_FLOWRECORDMASTER_TDAL.GetFlowRecordBySubmitUserID(CheckState, EditUserID);
            if (dt.Count > 0)
                return dt;
            return null;
        }

        #endregion

        #region 通过模块代码查询系统代码

        public static ModelInfo GetSysCodeByModelCode(string ModelCode)
        {
            ModelInfo tmpModelInfo = new ModelInfo();
            try
            {
                var dt= FLOW_MODELDEFINE_TDAL.GetModelDefineByCode(ModelCode);
                //Flow_ModelDefine_TDAL Dal = new Flow_ModelDefine_TDAL();
                //var dt = Dal.GetModelDefineByCode(ModelCode);

                if (dt.Count > 0)
                {
                    tmpModelInfo.SysCode = dt[0].SYSTEMCODE;
                    tmpModelInfo.ModelName = dt[0].DESCRIPTION;                    
                    return tmpModelInfo;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                tmpModelInfo = null;
            }

        }

        #endregion

        #region 通过模块名查找使用流程
        public static List<FLOW_MODELFLOWRELATION_T> GetFlowByModelName(string CompanyID, string DepartID, string ModelCode, string FlowType)
        {
            try
            {
                //Flow_ModelFlowRelation_TDAL Dal = new Flow_ModelFlowRelation_TDAL();
                //以部门查找流程
                List<FLOW_MODELFLOWRELATION_T> xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(CompanyID, DepartID, ModelCode, FlowType, "1");

                if (xoml != null && xoml.Count > 0) //找到匹配流程返回
                    return xoml;
                //部门的上级机构查找流程
                OrganizationServiceClient Organization = new OrganizationServiceClient ();
                Dictionary<string, string> OrganizationList= Organization.GetFatherByDepartmentID(DepartID);
                  if(OrganizationList==null || OrganizationList.Count <=0)
                      throw new Exception("GetFlowByModelName-->GetFatherByDepartmentID:没有找到部门的上级机构!");
                  foreach (var item in OrganizationList) 
                  {
                      if (item.Value == "0")
                      {
                          xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(CompanyID, item.Key, ModelCode, FlowType, "0"); //如果上级机构是公司直接查询公司关联流程并返回
                          return xoml;
                      }

                      xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(CompanyID, item.Key, ModelCode, FlowType, "1");
                      if (xoml != null && xoml.Count > 0) //找到匹配流程返回
                          return xoml;
                      
                  }    
                return xoml;

            }
            catch (Exception ex)
            {
                throw new Exception("GetFlowByModelName:"+ex.Message);// return null;
            }

        }

        #endregion

        #region 获取工作流状态

        /// <summary>
        /// 获取工作流状态
        /// </summary>
        /// <param name="workflowRuntime">工作流运行时</param>
        /// <param name="instance">工作流实例</param>
        /// <returns></returns>
        public static string GetState(WorkflowRuntime workflowRuntime, WorkflowInstance instance, string CurrentStateName)
        {
            string StateName = CurrentStateName;

            while (StateName == CurrentStateName)
            {
                StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
                StateName = workflowinstance.CurrentStateName;

                StateName = StateName == null ? "EndFlow" : StateName;
            }
            //System.Threading.Thread.Sleep(1 * 1000);
            //ReadOnlyCollection<WorkflowQueueInfo> queueInfoData = instance.GetWorkflowQueueData();
            //if (queueInfoData.Count != 0)
            //{

            //    foreach (WorkflowQueueInfo info in queueInfoData)
            //    {
            //        if (info.QueueName.Equals("SetStateQueue"))
            //        {

            //            StateName = info.SubscribedActivityNames[0];
            //        }
            //    }

            //}
            return StateName;

        }

        #endregion

        #region 通过流程查找审核人

        /// <summary>
        /// 通过流程查找审核人
        /// </summary>
        /// <param name="Xoml"></param>
        /// <param name="Rules"></param>
        /// <param name="xml"></param>
        /// <param name="UserID"></param>
        /// <param name="DataResult"></param>
        //public void GetUserByFlow(string Xoml, string Rules, string xml, string UserID, ref DataResult DataResult)
        //{
        //    WorkflowRuntime WfRuntime = null;
        //    WorkflowInstance Instance = null;
        //    try
        //    {
        //        WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
        //        Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime, Xoml, Rules);

        //        string strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, "StartFlow", xml);



        //        List<UserInfo> AppUserInfo = GetUserByStateCode(strNextState, UserID);
        //        if (AppUserInfo == null || AppUserInfo.Count == 0)
        //        {
        //            DataResult.Err = "没有找到审核人";
        //            DataResult.FlowResult = FlowResult.FAIL;
        //        }
        //        else if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
        //        {
        //            DataResult.FlowResult = FlowResult.MULTIUSER;
        //        }
        //        DataResult.UserInfo = AppUserInfo;


        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //    finally
        //    {
        //        Instance = null;
        //        SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

        //    }


        //}

        #endregion

        #region 通过流程查找审核人(对应SubmitFlow)

        /// <summary>
        /// 通过流程查找审核人(对应SubmitFlow)
        /// </summary>
        /// <param name="Xoml"></param>
        /// <param name="Rules"></param>
        /// <param name="Layout"></param>
        /// <param name="xml"></param>
        /// <param name="UserID"></param>
        /// <param name="FlowType"></param>
        /// <param name="DataResult"></param>
        public void GetUserByFlow(string companyID,string Xoml, string Rules, string Layout, string xml, string UserID, string PostID, FlowType FlowType, ref DataResult DataResult)        
        {
           

            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            string strCurrState = "StartFlow";
            string strNextState = "StartFlow";

            Role_UserType RuleName;
            List<UserInfo> AppUserInfo = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime, Xoml, Rules);

                bool iscurruser = true;

                while (iscurruser)
                {
                    strCurrState = strNextState;

                    strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml);

                    if (strNextState == "EndFlow")
                    {
                        strNextState = strCurrState;
                        iscurruser = false;
                    }
                    else
                    {

                        RuleName = Utility.GetRlueName(Layout, strNextState);
                        if (RuleName == null)
                        {
                            DataResult.Err = "没有找到对应角色";
                            DataResult.FlowResult = FlowResult.FAIL;
                            return;
                        }
                        bool isHigher = false;
                        AppUserInfo = GetUserByStateCode(RuleName.RoleName, UserID, PostID,ref isHigher);

                        #region beyond
                        if (!isHigher&& RuleName.IsOtherCompany != null)
                        {
                            if (RuleName.IsOtherCompany.Value==true)
                            {
                                AppUserInfo = AppUserInfo.Where(user => user.CompanyID == RuleName.OtherCompanyID).ToList();
                            }
                            else if(RuleName.IsOtherCompany.Value==false)
                            {
                                AppUserInfo = AppUserInfo.Where(user => user.CompanyID == companyID).ToList();
                            }
                        }
                        #endregion 

                        if (AppUserInfo == null || AppUserInfo.Count == 0)
                        {
                            DataResult.Err = "没有找到审核人";
                            DataResult.FlowResult = FlowResult.FAIL;
                            return;
                        }

                        if (AppUserInfo.Where(c => c.UserID == UserID).Count() == 0)
                            iscurruser = false;
                    }
                }


                //if (AppUserInfo == null || AppUserInfo.Count == 0)
                //{
                //    DataResult.Err = "没有找到审核人";
                //    DataResult.FlowResult = FlowResult.FAIL;
                //}
                // else if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                {
                    DataResult.FlowResult = FlowResult.MULTIUSER;
                }
                DataResult.AppState = strNextState;
                DataResult.UserInfo = AppUserInfo;


            }
            catch (Exception ex)
            {
                throw new Exception("GetUserByFlow:" + ex.Message);
            }
            finally
            {
                strCurrState = null;
                strNextState = null;
                RuleName = null;
                AppUserInfo = null;
                Instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

            }


        }

        public void GetUserByFlow2(string companyID,string Xoml, string Rules, string Layout, string xml, string UserID, string PostID, FlowType FlowType, ref DataResult DataResult) 
        {
           
            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            string strCurrState = "StartFlow";
            string strNextState = "StartFlow";
            bool IsCountersign = false;
            string CountersignType = "0";
            //Role_UserType RuleName;
            List<UserInfo> AppUserInfo = null;
            Dictionary<Role_UserType, List<UserInfo>> DictCounterUser = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime, Xoml, Rules);

                bool iscurruser = true;
                int testflag = 0;
                while (iscurruser)
                {
                    testflag++;
                    if (testflag > 10)
                    {
                        throw new Exception("循环超过10次");
                    }
                    #region
                    strCurrState = strNextState;

                    strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml);

                    if (strNextState == "EndFlow")
                    {
                        strNextState = strCurrState;
                        iscurruser = false;
                    }
                    else
                    {
                        List<Role_UserType> listRole = Utility.GetRlueName2(Layout, strNextState, ref IsCountersign, ref CountersignType);
                        if (listRole.Count == 0)
                        {
                            DataResult.Err = "没有找到对应角色";
                            DataResult.FlowResult = FlowResult.FAIL;
                            return;
                        }
                        if (!IsCountersign)
                        {
                            #region
                            bool isHigher = false;
                            //根据角色找人
                            AppUserInfo = GetUserByStateCode(listRole[0].RoleName, UserID, PostID,ref isHigher);
                            #region beyond

                            if (!isHigher)
                            {
                                if (listRole[0].IsOtherCompany != null&&listRole[0].IsOtherCompany.Value == true)
                                {
                                    //过滤人
                                    AppUserInfo = AppUserInfo.Where(user => user.CompanyID == listRole[0].OtherCompanyID).ToList();
                                }
                                else 
                                {
                                    AppUserInfo = AppUserInfo.Where(user => user.CompanyID == companyID).ToList();
                                }
                            }

                           
                            
                           
                            #endregion 
                            if (AppUserInfo == null || AppUserInfo.Count == 0)
                            {
                                DataResult.Err = "没有找到审核人";
                                DataResult.FlowResult = FlowResult.FAIL;
                                return;
                            }


                            if (AppUserInfo.Where(c => c.UserID == UserID).Count() == 0)
                                iscurruser = false;
                            #endregion
                        }
                        else
                        {
                            #region
                            DictCounterUser = new Dictionary<Role_UserType, List<UserInfo>>();
                            if (CountersignType == "0")
                            {
                                #region
                                for (int i = 0; i < listRole.Count; i++)
                                {
                                    bool isHigher = false;
                                    

                                    var listuserinfo = GetUserByStateCode(listRole[i].RoleName, UserID, PostID,ref isHigher);
                                    if (!isHigher)
                                    {
                                        if (listRole[i].IsOtherCompany != null&&listRole[i].IsOtherCompany.Value == true)
                                        {
                                            listuserinfo = listuserinfo.Where(user => user.CompanyID == listRole[i].OtherCompanyID).ToList();
                                        }
                                        else 
                                        {
                                            listuserinfo = listuserinfo.Where(user => user.CompanyID == companyID).ToList();
                                        }
                                    }

                                    if (listuserinfo == null || listuserinfo.Count == 0)
                                    {
                                        DataResult.Err = "没有找到审核人";
                                        DataResult.FlowResult = FlowResult.FAIL;
                                        return;
                                    }
                                    DictCounterUser.Add(listRole[i], listuserinfo);
                                }
                                iscurruser = false;
                                #endregion
                            }
                            else
                            {
                                #region
                                iscurruser = false;
                                bool bFlag = false;
                                for (int i = 0; i < listRole.Count; i++)
                                {
                                    bool isHigher = false;
                                    var listuserinfo = GetUserByStateCode(listRole[i].RoleName, UserID, PostID,ref isHigher);
                                    if (!isHigher)
                                    {
                                        if (listRole[i].IsOtherCompany != null&&listRole[i].IsOtherCompany.Value == true)
                                        {
                                            listuserinfo = listuserinfo.Where(user => user.CompanyID == listRole[i].OtherCompanyID).ToList();
                                        }
                                        else 
                                        {
                                            listuserinfo = listuserinfo.Where(user => user.CompanyID == companyID).ToList();
                                        }
                                    }

                                    if (listuserinfo != null && listuserinfo.Count > 0)
                                    {
                                        bFlag = true;
                                        if (listuserinfo.FirstOrDefault(u => u.UserID == UserID) != null)
                                        {
                                            iscurruser = true;
                                            break;
                                        }
                                        //DataResult.Err = "没有找到审核人";
                                        //DataResult.FlowResult = FlowResult.FAIL;
                                        //return;
                                    }
                                   
                                    DictCounterUser.Add(listRole[i], listuserinfo);
                                }
                                if (!bFlag)
                                {
                                    DataResult.Err = "没有找到审核人";
                                    DataResult.FlowResult = FlowResult.FAIL;
                                    return;
                                }
                                //iscurruser = false;
                                #endregion
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                DataResult.IsCountersign = IsCountersign;
                DataResult.AppState = strNextState;
                DataResult.CountersignType = CountersignType;
                if (!IsCountersign)
                {
                    #region
                    if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                    {
                        DataResult.FlowResult = FlowResult.MULTIUSER;
                    }

                    DataResult.UserInfo = AppUserInfo;
                    #endregion
                }
                else
                {
                    #region
                    DataResult.DictCounterUser = DictCounterUser;
                    List<Role_UserType> listkeys = DictCounterUser.Keys.ToList();
                    for (int i = 0; i < listkeys.Count; i++)
                    {
                        Role_UserType key = listkeys[i];
                        if (DictCounterUser[key].Count > 1)
                        {
                            DataResult.FlowResult = FlowResult.Countersign;
                            break;
                        }
                    }
                    #endregion

                }


            }
            catch (Exception ex)
            {
                throw new Exception("GetUserByFlow2:" + ex.Message);
            }
            finally
            {
                strCurrState = null;
                strNextState = null;
                //RuleName = null;
                AppUserInfo = null;
                Instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

            }


        }

        

        #endregion

        #region 通过持久化服务查询下一处理人

        /// <summary>
        /// 通过持久化服务查询下一处理人
        /// </summary>
        /// <param name="WfRuntimeClone">持久化运行时</param>
        /// <param name="instanceClone">持久化实例</param>
        /// <param name="xml">条件XML</param>
        /// <param name="CurrentStateName">当前状态代码</param>
        /// <param name="DataResult">操作结果</param>
        public void GetUserByInstance(WorkflowRuntime WfRuntimeClone, WorkflowInstance instanceClone, string xml, string CurrentStateName, string UserID, string PostID, ref DataResult DataResult)
        {
            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CloneWorkflowInstance(WfRuntimeClone, instanceClone, WfRuntime);

                string strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, CurrentStateName, xml);
                bool isHigher = false;
                List<UserInfo> AppUserInfo = GetUserByStateCode(strNextState, UserID, PostID,ref isHigher);
                if (AppUserInfo == null || AppUserInfo.Count == 0)
                {
                    DataResult.Err = "没有找到审核人";
                    DataResult.FlowResult = FlowResult.FAIL;
                }
                else if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                {
                    DataResult.FlowResult = FlowResult.MULTIUSER;
                }
                DataResult.UserInfo = AppUserInfo;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                Instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

            }


        }

        #endregion

        #region 通过持久化服务查询下一处理人(对应SubmitFlow)

        /// <summary>
        /// 通过持久化服务查询下一处理人(对应SubmitFlow)
        /// </summary>
        /// <param name="WfRuntimeClone">持久化运行时</param>
        /// <param name="instanceClone">持久化实例</param>
        /// <param name="xml">条件XML</param>
        /// <param name="CurrentStateName">当前状态代码</param>
        /// <param name="DataResult">操作结果</param>
        public void GetUserByInstance(string companyID, WorkflowRuntime WfRuntimeClone, WorkflowInstance instanceClone, string Layout, string xml, string CurrentStateName, List<string> UserID, List<string> PostID, FlowType FlowType, ref DataResult DataResult)
        {
            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            List<UserInfo> AppUserInfo = null;
            string strNextState = CurrentStateName;
            Role_UserType RuleName;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CloneWorkflowInstance(WfRuntimeClone, instanceClone, WfRuntime);
                bool iscurruser = true;

                while (iscurruser)
                {
                 //   CurrentStateName = strNextState;
                    strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml);
                    //if (FlowType == FlowType.Task && strNextState != "EndFlow")
                    //{
                    //    XmlReader XmlReader;

                    //    StringReader tmpLayout = new StringReader(Layout);
                    //    XmlReader = XmlReader.Create(tmpLayout);
                    //    XElement XElementS = XElement.Load(XmlReader);
                    //    var a = from c in XElementS.Descendants("Activity")
                    //            where c.Attribute("Name").Value == strNextState
                    //            select c.Attribute("RoleName").Value;
                    //    if (a.Count() > 0)
                    //    {
                    //        strNextState = a.First().ToString();
                    //    }
                    //    else
                    //    {
                    //        DataResult.Err = "没有找到对应角色";
                    //        DataResult.FlowResult = FlowResult.FAIL;
                    //        return;
                    //    }

                    //    tmpLayout = null;
                    //    XmlReader = null;
                    //    XElementS = null;
                    //    a = null;
                    //}


                     RuleName = Utility.GetRlueName(Layout, strNextState);



                    if (RuleName == null)
                    {
                        DataResult.Err = "没有找到对应角色";
                        DataResult.FlowResult = FlowResult.FAIL;
                        return;
                    }

                    string tmpPostID = RuleName.UserType == "CREATEUSER" ? PostID[0] : PostID[1];
                    bool isHigher = false;
                    AppUserInfo = GetUserByStateCode(RuleName.RoleName,null, tmpPostID,ref isHigher);
                    #region beyond
                    if (!isHigher&&RuleName.IsOtherCompany != null)
                    {
                        if (RuleName.IsOtherCompany.Value == true)
                        {
                            AppUserInfo = AppUserInfo.Where(user => user.CompanyID == RuleName.OtherCompanyID).ToList();
                        }
                        else if (RuleName.IsOtherCompany.Value == false)
                        {
                            AppUserInfo = AppUserInfo.Where(user => user.CompanyID == companyID).ToList();
                        }
                    }
                   
                    #endregion 
                    if (AppUserInfo == null || AppUserInfo.Count == 0)
                    {
                        DataResult.Err = "没有找到审核人";
                        DataResult.FlowResult = FlowResult.FAIL;
                        return;
                    }
                    if (AppUserInfo.Where(c => c.UserID == UserID[1]).Count() == 0)
                        iscurruser = false;
                }

                if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                {
                    DataResult.FlowResult = FlowResult.MULTIUSER;
                }

                DataResult.AppState = strNextState;
                DataResult.UserInfo = AppUserInfo;


            }
            catch (Exception ex)
            {
                throw new Exception("GetUserByInstance:"+ex.Message);
            }
            finally
            {
                strNextState = null;
                AppUserInfo = null;
                RuleName = null;
                Instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

            }


        }

        public void GetUserByInstance2(string companyID,WorkflowRuntime WfRuntimeClone, WorkflowInstance instanceClone, string Layout, string xml, string CurrentStateName, List<string> UserID, List<string> PostID, FlowType FlowType, ref DataResult DataResult)
        {
            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            List<UserInfo> AppUserInfo = null;
            string strNextState = CurrentStateName;
            bool IsCountersign = false;
            string CountersignType = "0";
            //Role_UserType RuleName;
            //List<UserInfo> AppUserInfo = null;
            Dictionary<Role_UserType, List<UserInfo>> DictCounterUser = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CloneWorkflowInstance(WfRuntimeClone, instanceClone, WfRuntime);
                bool iscurruser = true;
                int testflag = 0;
                while (iscurruser)
                {
                    testflag++;
                    if (testflag > 10)
                    {
                        throw new Exception("循环超过10次");
                    }
                    #region
                   

                    strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml);
                    List<Role_UserType> listRole = Utility.GetRlueName2(Layout, strNextState, ref IsCountersign, ref CountersignType);
                    if (listRole.Count == 0)
                    {
                        DataResult.Err = "没有找到对应角色";
                        DataResult.FlowResult = FlowResult.FAIL;
                        return;
                    }
                   
                    if (!IsCountersign)
                    {
                        #region
                        string tmpPostID = listRole[0].UserType == "CREATEUSER" ? PostID[0] : PostID[1];
                        bool isHigher = false;
                        AppUserInfo = GetUserByStateCode(listRole[0].RoleName, null, tmpPostID,ref isHigher);
                        #region beyond
                       
                        if (!isHigher&& strNextState.ToUpper() != "ENDFLOW")
                        {
                            if (listRole[0].IsOtherCompany != null && listRole[0].IsOtherCompany.Value == true)
                            {
                                AppUserInfo = AppUserInfo.Where(user => user.CompanyID == listRole[0].OtherCompanyID).ToList();
                            }
                            else
                            {
                                AppUserInfo = AppUserInfo.Where(user => user.CompanyID == companyID).ToList();
                            }
                            
                           
                        }


                        #endregion
                        if (AppUserInfo == null || AppUserInfo.Count == 0)
                        {
                            DataResult.Err = "没有找到审核人";
                            DataResult.FlowResult = FlowResult.FAIL;
                            return;
                        }


                        if (AppUserInfo.Where(c => c.UserID == UserID[1]).Count() == 0)
                            iscurruser = false;
                        #endregion
                    }
                    else
                    {
                        #region
                        DictCounterUser = new Dictionary<Role_UserType, List<UserInfo>>();
                        if (CountersignType == "0")
                        {
                            #region
                            for (int i = 0; i < listRole.Count; i++)
                            {
                                string tmpPostID = listRole[i].UserType == "CREATEUSER" ? PostID[0] : PostID[1];
                                bool isHigher = false;
                                var listuserinfo = GetUserByStateCode(listRole[i].RoleName, null, tmpPostID,ref isHigher);
                                if (!isHigher)
                                {
                                    if (listRole[i].IsOtherCompany != null&&listRole[i].IsOtherCompany.Value == true)
                                    {
                                        listuserinfo = listuserinfo.Where(user => user.CompanyID == listRole[i].OtherCompanyID).ToList();
                                    }
                                    else 
                                    {
                                        listuserinfo = listuserinfo.Where(user => user.CompanyID == companyID).ToList();
                                    }
                                }

                                if (listuserinfo == null || listuserinfo.Count == 0)
                                {
                                    DataResult.Err = "没有找到审核人";
                                    DataResult.FlowResult = FlowResult.FAIL;
                                    return;
                                }
                                DictCounterUser.Add(listRole[i], listuserinfo);
                            }
                            iscurruser = false;
                            #endregion
                        }
                        else
                        {
                            #region
                            iscurruser = false;
                            bool bFlag = false;
                            for (int i = 0; i < listRole.Count; i++)
                            {
                                string tmpPostID = listRole[i].UserType == "CREATEUSER" ? PostID[0] : PostID[1];
                                bool isHigher = false;
                                var listuserinfo = GetUserByStateCode(listRole[i].RoleName, null, tmpPostID,ref isHigher);
                                if (!isHigher)
                                {
                                    if (listRole[i].IsOtherCompany != null&&listRole[i].IsOtherCompany.Value == true)
                                    {
                                        listuserinfo = listuserinfo.Where(user => user.CompanyID == listRole[i].OtherCompanyID).ToList();
                                    }
                                    else 
                                    {
                                        listuserinfo = listuserinfo.Where(user => user.CompanyID == companyID).ToList();
                                    }
                                }
                                if (listuserinfo != null && listuserinfo.Count > 0)
                                {
                                    bFlag = true;
                                    if (listuserinfo.FirstOrDefault(u => u.UserID == UserID[1]) != null)
                                    {
                                        iscurruser = true;
                                        break;
                                    }
                                    //DataResult.Err = "没有找到审核人";
                                    //DataResult.FlowResult = FlowResult.FAIL;
                                    //return;
                                }
                                DictCounterUser.Add(listRole[i], listuserinfo);
                            }
                            if (!bFlag)
                            {
                                DataResult.Err = "没有找到审核人";
                                DataResult.FlowResult = FlowResult.FAIL;
                                return;
                            }
                            #endregion
                        }
                        #endregion
                    }

                   
                    #endregion
                }
                DataResult.IsCountersign = IsCountersign;
                DataResult.AppState = strNextState;
                DataResult.CountersignType = CountersignType;
                if (!IsCountersign)
                {
                    #region
                    if (AppUserInfo!=null&&AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                    {
                        DataResult.FlowResult = FlowResult.MULTIUSER;
                    }
                    DataResult.UserInfo = AppUserInfo;
                    #endregion
                }
                else
                {
                    #region
                    DataResult.DictCounterUser = DictCounterUser;


                    List<Role_UserType> listkeys = DictCounterUser.Keys.ToList();
                    for (int i = 0; i < listkeys.Count; i++)
                    {
                        Role_UserType key = listkeys[i];
                        if (DictCounterUser[key].Count > 1)
                        {
                            DataResult.FlowResult = FlowResult.Countersign;
                            break;
                        }
                    }
                    #endregion

                }

            }
            catch (Exception ex)
            {
                throw new Exception("GetUserByInstance2:" + ex.Message);
            }
            finally
            {
                strNextState = null;
                AppUserInfo = null;
                //RuleName = null;
                Instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);

            }


        }

        


        #endregion

        #region 通过状态代码查询下一处理人
        /// <summary>
        /// 通过状态代码查询下一处理人
        /// </summary>
        /// <param name="StateCode">状态代码</param>
        /// <returns></returns>
        private List<UserInfo> GetUserByStateCode(string StateCode, string UserID, string PostID, ref bool isHigher)
        {
            PermissionServiceClient WcfPermissionService = new PermissionServiceClient();
            PersonnelServiceClient WcfPersonnel = new PersonnelServiceClient();
            try
            {

                string CurrentStateName = StateCode == "EndFlow" ? "End" : StateCode; //取得当前状态
                List<UserInfo> listUser = new List<UserInfo>();

                
       
                if (CurrentStateName != "End")
                {
                    //if (CurrentStateName.Substring(0, 5) == "State")
                    //{
                    //    CurrentStateName = CurrentStateName.Substring(5);
                    //}

                    string WFCurrentStateName = "";
                    int isDirect = 0;
                    foreach (Higher c in Enum.GetValues(typeof(Higher)))
                    {
                        if (CurrentStateName.ToUpper() == c.ToString().ToUpper())
                        {
                            isDirect = (int)c;//== 1 ? true : false;

                            WFCurrentStateName = CurrentStateName;
                            isHigher = true;
                        }
                    }


                    if (WFCurrentStateName != "")
                    {
                        //PermissionService
                        
                        V_EMPLOYEEVIEW[] User = WcfPersonnel.GetEmployeeLeaders(PostID, isDirect);
                        if (User != null && User.Length > 0)
                            for (int i = 0; i < User.Length; i++)
                            {
                                UserInfo tmp = new UserInfo();
                                tmp.UserID = User[i].EMPLOYEEID;
                                tmp.UserName = User[i].EMPLOYEECNAME;
                                tmp.CompanyID = User[i].OWNERCOMPANYID;
                                tmp.DepartmentID = User[i].OWNERDEPARTMENTID;
                                tmp.PostID = User[i].OWNERPOSTID;
                                listUser.Add(tmp);
                            }
                    }
                    else
                    {
                        WFCurrentStateName = new Guid(CurrentStateName).ToString("D");

                        foreach (var op in WcfPermissionService.Endpoint.Contract.Operations)
                        {
                            var dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                                as DataContractSerializerOperationBehavior;
                            if (dataContractBehavior != null)
                            {
                                dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue; //int.MaxValue;
                            }
                        }
                        T_SYS_USER[] User = WcfPermissionService.GetSysUserByRole(WFCurrentStateName); //检索本状态（角色）对应用户
                 
                        if (User != null && User.Length > 0)
                            for (int i = 0; i < User.Length; i++)
                            {
                                UserInfo tmp = new UserInfo();
                                tmp.UserID = User[i].EMPLOYEEID;
                                tmp.UserName = User[i].EMPLOYEENAME;
                                tmp.CompanyID = User[i].OWNERCOMPANYID;
                                tmp.DepartmentID = User[i].OWNERDEPARTMENTID;
                                tmp.PostID = User[i].OWNERPOSTID;
                                listUser.Add(tmp);
                            }

                    }

                }
                else
                {
                    //已经到流程结束状态
                    UserInfo tmp = new UserInfo();
                    tmp.UserID = "End";
                    tmp.UserName = "End";

                    listUser.Add(tmp);
                }

                return listUser;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserByStateCode:"+ex.Message);
                // return null ;
            }
            finally
            {
                WcfPermissionService.Close();
                WcfPersonnel.Close();
                WcfPermissionService = null;
                WcfPersonnel = null;
            }


        }

        #endregion

        #region 检查是否已提交流程
        /// <summary>
        /// 检查是否已提交流程
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
        public CheckResult CheckFlow(SubmitData ApprovalData, DataResult APPDataResult)
        {

            CheckResult CheckFlowResult = new CheckResult();
            try
            {

                CheckFlowResult.APPDataResult = APPDataResult;
                APPDataResult.RunTime += "---GetFlowInfoStart:" + DateTime.Now.ToString();
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(ApprovalData.FlowType);
                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo(ApprovalData.FormID, "", "", "0", ApprovalData.ModelCode, "", "", FlowTypeList);
                CheckFlowResult.fd = fd;
                APPDataResult.RunTime += "---GetFlowInfoEnd:" + DateTime.Now.ToString();

                if (ApprovalData.SubmitFlag == SubmitFlag.New)
                {

                    if (fd != null && fd.Count > 0)
                    {
                        CheckFlowResult.APPDataResult.FlowResult = FlowResult.SUCCESS;
                        CheckFlowResult.Flag = 0;
                        UserInfo AppUser = new UserInfo();//下一审核人
                        AppUser.UserID = fd[0].EDITUSERID;
                        AppUser.UserName = fd[0].EDITUSERNAME;
                        AppUser.CompanyID = fd[0].EDITCOMPANYID;
                        AppUser.DepartmentID = fd[0].EDITDEPARTMENTID;
                        AppUser.PostID = fd[0].EDITPOSTID;

                        CheckFlowResult.APPDataResult.UserInfo.Add(AppUser);
                        CheckFlowResult.APPDataResult.AppState = fd[0].STATECODE;

                        return CheckFlowResult;
                    }

                }
                else
                {
                    if (fd == null || fd.Count == 0)
                    {
                        CheckFlowResult.APPDataResult.FlowResult = FlowResult.FAIL;
                        CheckFlowResult.APPDataResult.Err = "没有待审批节点，请检查流程是否已经结束或流程有异常!";
                        CheckFlowResult.Flag = 0;

                        return CheckFlowResult;
                    }
                    else
                    {

                        if (fd.Where(c => c.EDITUSERID == ApprovalData.ApprovalUser.UserID || c.AGENTUSERID == ApprovalData.ApprovalUser.UserID).ToList().Count == 0)
                        {
                            APPDataResult.FlowResult = FlowResult.SUCCESS;
                            CheckFlowResult.Flag = 0;
                            UserInfo AppUser = new UserInfo();
                            AppUser.UserID = fd[0].EDITUSERID;
                            AppUser.UserName = fd[0].EDITUSERNAME;
                            AppUser.CompanyID = fd[0].EDITCOMPANYID;
                            AppUser.DepartmentID = fd[0].EDITDEPARTMENTID;
                            AppUser.PostID = fd[0].EDITPOSTID;

                            CheckFlowResult.APPDataResult.UserInfo.Add(AppUser);
                            CheckFlowResult.APPDataResult.AppState = fd[0].STATECODE;

                            return CheckFlowResult;
                        }

                    }


                }
                CheckFlowResult.Flag = 1;
                return CheckFlowResult;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                CheckFlowResult = null;
            }
        }

        public CheckResult CheckFlow2(SubmitData ApprovalData, DataResult APPDataResult)
        {

            CheckResult CheckFlowResult = new CheckResult();
            try
            {

                CheckFlowResult.APPDataResult = APPDataResult;
                APPDataResult.RunTime += "---GetFlowInfoStart:" + DateTime.Now.ToString();
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(ApprovalData.FlowType);
                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo(ApprovalData.FormID, "", "", "0", ApprovalData.ModelCode, "", "", FlowTypeList);
                CheckFlowResult.fd = fd;
                APPDataResult.RunTime += "---GetFlowInfoEnd:" + DateTime.Now.ToString();

                if (ApprovalData.SubmitFlag == SubmitFlag.New)
                {
                    #region 
                    if (fd != null && fd.Count > 0)
                    {
                        CheckFlowResult.APPDataResult.FlowResult = FlowResult.SUCCESS;
                        CheckFlowResult.Flag = 0;
                        UserInfo AppUser = new UserInfo();//下一审核人
                        AppUser.UserID = fd[0].EDITUSERID;
                        AppUser.UserName = fd[0].EDITUSERNAME;
                        AppUser.CompanyID = fd[0].EDITCOMPANYID;
                        AppUser.DepartmentID = fd[0].EDITDEPARTMENTID;
                        AppUser.PostID = fd[0].EDITPOSTID;

                        CheckFlowResult.APPDataResult.UserInfo.Add(AppUser);
                        CheckFlowResult.APPDataResult.AppState = fd[0].STATECODE;

                        return CheckFlowResult;
                    }
                    #endregion 

                }
                else if (ApprovalData.SubmitFlag == SubmitFlag.Cancel)
                {
                }
               
                else
                {
                    if (fd == null || fd.Count == 0)
                    {
                        CheckFlowResult.APPDataResult.FlowResult = FlowResult.FAIL;
                        CheckFlowResult.APPDataResult.Err = "没有待审批节点，请检查流程是否已经结束或流程有异常!";
                        CheckFlowResult.Flag = 0;

                        return CheckFlowResult;
                    }
                    else
                    {

                        if (fd.Where(c => c.EDITUSERID == ApprovalData.ApprovalUser.UserID || c.AGENTUSERID == ApprovalData.ApprovalUser.UserID).ToList().Count == 0)
                        {
                            APPDataResult.FlowResult = FlowResult.SUCCESS;
                            CheckFlowResult.Flag = 0;
                            UserInfo AppUser = new UserInfo();
                            AppUser.UserID = fd[0].EDITUSERID;
                            AppUser.UserName = fd[0].EDITUSERNAME;
                            AppUser.CompanyID = fd[0].EDITCOMPANYID;
                            AppUser.DepartmentID = fd[0].EDITDEPARTMENTID;
                            AppUser.PostID = fd[0].EDITPOSTID;

                            CheckFlowResult.APPDataResult.UserInfo.Add(AppUser);
                            CheckFlowResult.APPDataResult.AppState = fd[0].STATECODE;

                            return CheckFlowResult;
                        }

                    }


                }
                CheckFlowResult.Flag = 1;
                return CheckFlowResult;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                CheckFlowResult = null;
            }
        }
        #endregion
        
        #region 固定流程审批

        #region 新建流程
        /// <summary>
        /// 新增流程
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
        

        public DataResult AddFlow2(SubmitData submitData, DataResult dataResult)
        {
            WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;
            
            
            try
            {
                #region 获取定义的流程
                List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(submitData.ApprovalUser.CompanyID, submitData.ApprovalUser.DepartmentID, submitData.ModelCode, ((int)submitData.FlowType).ToString());
                if (MODELFLOWRELATION == null || MODELFLOWRELATION.Count == 0)
                {
                    dataResult.FlowResult = FlowResult.FAIL;
                    dataResult.Err = "没有找到可使用的流程";
                    return dataResult;
                }
                #endregion 
                FLOW_MODELFLOWRELATION_T flowRelation = MODELFLOWRELATION[0];
                FLOW_FLOWDEFINE_T flowDefine = flowRelation.FLOW_FLOWDEFINE_T;
                if (flowDefine.RULES != null && flowDefine.RULES.Trim() == "")
                {
                    flowDefine.RULES = null;
                }
                //if (string.IsNullOrEmpty(flowDefine.RULES.Trim()))
                //{
                //    flowDefine.RULES = null;
                //}
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime, flowDefine.XOML, flowDefine.RULES);
                workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                {
                    instance = null;

                };                
                #region master赋值
                FLOW_FLOWRECORDMASTER_T master = new FLOW_FLOWRECORDMASTER_T();
                master.INSTANCEID = instance.InstanceId.ToString();
                master.BUSINESSOBJECT = submitData.XML;
                master.FORMID = submitData.FormID;
                master.MODELCODE = submitData.ModelCode;
                master.ACTIVEROLE = Utility.GetActiveRlue(flowDefine.LAYOUT);
                master.FLOWTYPE = ((int)submitData.FlowType).ToString();
                master.FLOWSELECTTYPE = ((int)submitData.FlowSelectType).ToString();
                master.FLOWCODE = flowDefine.FLOWCODE;
                #endregion 
               
                #region 获取下一状态数据

                GetUserByFlow2(submitData.ApprovalUser.CompanyID, flowDefine.XOML, flowDefine.RULES, master.ACTIVEROLE, submitData.XML, submitData.ApprovalUser.UserID, submitData.ApprovalUser.PostID, submitData.FlowType, ref dataResult);
                if (dataResult.FlowResult == FlowResult.FAIL)
                {
                    return dataResult;
                }
                submitData.NextStateCode = dataResult.AppState;
                if (dataResult.IsCountersign)
                {
                    #region
                    if (dataResult.FlowResult == FlowResult.Countersign)
                    {
                        if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                        {
                            return dataResult;
                        }
                    }
                    else
                    {
                        if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                        {
                            submitData.DictCounterUser = dataResult.DictCounterUser;
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 
                    if (dataResult.FlowResult == FlowResult.MULTIUSER)
                    {
                        if (submitData.NextApprovalUser == null || (Utility.GetString(submitData.NextApprovalUser.UserID) == "" || Utility.GetString(submitData.NextApprovalUser.UserName) == ""))
                        {
                            return dataResult;
                        }
                    }
                    else
                    {
                        if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                        {
                            submitData.NextApprovalUser = dataResult.UserInfo[0];
                        }
                    }
                    #endregion 
                }

                #endregion
                
                #region 实体赋值
                FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
                entity.FLOW_FLOWRECORDMASTER_T = master;
                entity.CREATECOMPANYID = submitData.ApprovalUser.CompanyID;
                entity.CREATEDEPARTMENTID = submitData.ApprovalUser.DepartmentID;
                entity.CREATEPOSTID = submitData.ApprovalUser.PostID;
                entity.CREATEUSERID = submitData.ApprovalUser.UserID;
                entity.CREATEUSERNAME = submitData.ApprovalUser.UserName;
                #endregion                   
               
                #region 处理kpi时间
                string KPITime = "";
                PerformanceServiceWS.PerformanceServiceClient psc = new PerformanceServiceWS.PerformanceServiceClient();
                string pscResult = psc.GetKPIPointsByBusinessCode(flowRelation.MODELFLOWRELATIONID);
                psc.Close();
                if (!string.IsNullOrEmpty(pscResult))
                {
                    XElement xe = XElement.Parse(pscResult);
                    Func<XElement, bool> f = (x) =>
                    {
                        XAttribute xid = x.Attribute("id");
                        XAttribute xvalue = x.Attribute("value");
                        if (xid == null || xvalue == null)
                            return false;
                        else
                        {
                            if (xid.Value == dataResult.AppState)
                                return true;
                            else return false;
                        }
                    };
                    XElement FlowNode = xe.Elements("FlowNode").FirstOrDefault(f);
                    if (FlowNode != null)
                    {
                        KPITime = FlowNode.Attribute("value").Value;
                    }
                }
                
                dataResult.KPITime = KPITime;
                master.KPITIMEXML = pscResult;
                #endregion 

                FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                FlowData.xml = submitData.XML;                

                if (!dataResult.IsCountersign)
                {
                    #region  非会签
                    UserInfo AppUser = new UserInfo(); //下一审核人
                    AppUser = submitData.NextApprovalUser;
                    dataResult.UserInfo.Clear();
                    dataResult.UserInfo.Add(AppUser);
                    UserInfo AgentAppUser = GetAgentUserInfo(submitData.ModelCode, AppUser.UserID);//查询是否启用了代理人                    
                    dataResult = DoFlowRecord2(workflowRuntime, instance, entity, submitData.NextStateCode, AppUser, AgentAppUser, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                    dataResult.IsCountersign = false;
                    dataResult.AgentUserInfo = AgentAppUser;                   
                    #endregion
                }
                else
                {
                    #region  会签
                    //Tracer.Debug("-----DoFlowRecord_Add:" + DateTime.Now.ToString()+"\n");
                    dataResult.DictCounterUser = submitData.DictCounterUser;
                    Dictionary<UserInfo, UserInfo> dictAgentUserInfo = GetAgentUserInfo2(submitData.ModelCode, submitData.DictCounterUser);                    
                    dataResult = DoFlowRecord_Add(workflowRuntime, instance, entity, submitData.NextStateCode, submitData.DictCounterUser, dictAgentUserInfo, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                    //Tracer.Debug("-----DoFlowRecord_AddEnd:" + DateTime.Now.ToString()+"\n");
                    dataResult.IsCountersign = true;
                    dataResult.DictAgentUserInfo = dictAgentUserInfo;                   
                    #endregion
                }

                #region 激发流程引擎执行到一下流程
                if (dataResult.AppState == null || dataResult.AppState == "")
                    workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                else
                {
                    StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);

                    workflowinstance.SetState(dataResult.AppState); //流程跳转到指定节点
                }
                #endregion 

                System.Threading.Thread.Sleep(1000);
                dataResult.ModelFlowRelationID = flowRelation.MODELFLOWRELATIONID; //返回关联ID
                dataResult.KPITime = KPITime;
                //dataResult.CanSendMessage = true;
                if (submitData.FlowType == FlowType.Task)
                    dataResult.SubModelCode = Utility.GetSubModelCode(master.ACTIVEROLE, dataResult.AppState); //返回下一子模块代码
                return dataResult;
            }

            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            finally
            {
               
                instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(workflowRuntime);
            }


        }



        #endregion

        #region 审批流程

        public DataResult CancelFlow(SubmitData submitData, DataResult dataResult, List<FLOW_FLOWRECORDDETAIL_T> fd)
        {
            WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;

            FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
            #region
            entity.FLOWRECORDDETAILID = Guid.NewGuid().ToString();
            entity.CREATECOMPANYID = submitData.ApprovalUser.CompanyID;
            entity.CREATEDEPARTMENTID = submitData.ApprovalUser.DepartmentID;
            entity.CREATEPOSTID = submitData.ApprovalUser.PostID;
            entity.CREATEUSERID = submitData.ApprovalUser.UserID;
            entity.CREATEUSERNAME = submitData.ApprovalUser.UserName;
            entity.EDITDATE = DateTime.Now;
            entity.CONTENT = submitData.ApprovalContent;
            entity.CHECKSTATE = "9";
            entity.STATECODE = "Cancel";           
            entity.FLAG = "1";            
            entity.PARENTSTATEID = entity.FLOWRECORDDETAILID;

            entity.CREATEDATE = DateTime.Now;
            entity.EDITDATE = DateTime.Now;
            entity.EDITUSERID = entity.CREATEUSERID;
            entity.EDITUSERNAME = entity.CREATEUSERNAME;
            entity.EDITCOMPANYID = entity.CREATECOMPANYID;
            entity.EDITDEPARTMENTID = entity.CREATEDEPARTMENTID;
            entity.EDITPOSTID = entity.CREATEPOSTID;

            entity.FLOW_FLOWRECORDMASTER_T = fd[0].FLOW_FLOWRECORDMASTER_T;
            entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T.Add(entity);
            entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "9"; //设为撤销
            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = submitData.ApprovalUser.UserID;
            entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = submitData.ApprovalUser.UserName;
            entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;
            #endregion 
            
            workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
            instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime, entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID);
            instance.Terminate("0");
            FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            entity.FLAG = "1";
            //Dal.AddFlowRecord(entity);
            FLOW_FLOWRECORDDETAIL_TDAL.Add(entity);
            FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
            fd.Where(detail => detail.FLAG == "0").ToList().ForEach(item =>
                {
                    //Dal.Delete(item);
                    //Dal.DeleteFlowRecord(item);
                    FLOW_FLOWRECORDDETAIL_TDAL.Delete(item);
                });
            dataResult.CheckState = "9";//
            dataResult.FlowResult = FlowResult.SUCCESS;
            return dataResult;
        }

        //下一审核人提交审核时调用方法
        public DataResult ApprovalFlow2(SubmitData submitData, DataResult dataResult, List<FLOW_FLOWRECORDDETAIL_T> listDetail)
        {
            ///针对会签，该次审核成功后是否跳转至下一状态
            bool isGotoNextState = true;
            WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;
            FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
            entity.FLOW_FLOWRECORDMASTER_T = new FLOW_FLOWRECORDMASTER_T();
           
            try
            {
                #region Entity赋值
                List<FLOW_FLOWRECORDDETAIL_T> tmpEntity = listDetail.Where(c => (c.EDITUSERID == submitData.ApprovalUser.UserID || c.AGENTUSERID == submitData.ApprovalUser.UserID) && c.FLAG == "0").ToList();
                if (tmpEntity == null)
                {
                    dataResult.FlowResult = FlowResult.FAIL;
                    dataResult.Err = "没有找到待审核信息";                   
                    return dataResult;
                }
                entity = tmpEntity[0];
                entity.EDITDATE = DateTime.Now;  //审批时间
                if (entity.AGENTUSERID == submitData.ApprovalUser.UserID)
                {
                    entity.AGENTEDITDATE = entity.EDITDATE;  //代理审批时审批时间与代理审批时间到致
                }
                
                entity.CONTENT = submitData.ApprovalContent;
                entity.CHECKSTATE = ((int)submitData.ApprovalResult).ToString();
                #endregion 

                //workflowRuntime.StartRuntime();
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                try
                {
                    
                    instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime, entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID);// workflowRuntime.GetWorkflow(new Guid(tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID));
                    
                }
                catch (Exception exGetWorkflowInstance)
                {
                    //重新创建新流程，将新流程设置为当前状态。
                    try
                    {
                        List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID, entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID, submitData.ModelCode, ((int)submitData.FlowType).ToString());

                        FLOW_MODELFLOWRELATION_T flowRelation = MODELFLOWRELATION[0];
                        FLOW_FLOWDEFINE_T flowDefine = flowRelation.FLOW_FLOWDEFINE_T;
                        instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime, flowDefine.XOML, flowDefine.RULES);                        
                        StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
                        workflowinstance.SetState(entity.STATECODE);
                        System.Threading.Thread.Sleep(1000);                        
                        instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime, instance.InstanceId.ToString());
                        entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID = instance.InstanceId.ToString();
                        //FLOW_FLOWRECORDDETAIL_TDAL.UpdateMasterINSTANCEID(entity.FLOW_FLOWRECORDMASTER_T);
                        FLOW_FLOWRECORDMASTER_TDAL.UpdateMasterINSTANCEID(entity.FLOW_FLOWRECORDMASTER_T);

                    }
                    catch (Exception exNewInstance)
                    {
                        Tracer.Debug("exNewInstance: -" + submitData.FormID + "--submitDataXML:" + submitData.XML + "-" + exNewInstance.InnerException + exNewInstance.Message);
                        throw exGetWorkflowInstance;
                    }
                }
                #region 当前状态会签状态处理
                bool currentIsCountersign=false;
                string currentCountersignType="0";
               
                Utility.IsCountersign(entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, entity.STATECODE, ref currentIsCountersign, ref currentCountersignType);
                if (currentIsCountersign)
                {
                    if (currentCountersignType == "1")//一人通过即所有通过，可以跳转至下一状态
                    {
                        isGotoNextState = true;
                    }
                    else
                    {
                        ///该审核是会签的最后的审核人
                        if (entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T.Count == 1)
                        {
                            isGotoNextState = true;
                        }                            
                        else
                        {
                            isGotoNextState = false;
                        }
                    }
                }
                #endregion 


                //不同意状态处理
                if (submitData.ApprovalResult == ApprovalResult.NoPass)
                {
                    #region 
                    instance.Terminate("0");
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "3"; //设为终审不通过
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = submitData.ApprovalUser.UserID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = submitData.ApprovalUser.UserName;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;
                    UpdateFlowRecord(entity, submitData.NextStateCode, submitData.NextApprovalUser.UserID);
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    dataResult.CheckState = "3";//
                    dataResult.FlowResult = FlowResult.END;

                    if (currentIsCountersign)
                    {
                        #region 当前是会签状态，删除未审核记录
                        entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T
                                .Where(detail => detail.FLOWRECORDDETAILID != entity.FLOWRECORDDETAILID && detail.STATECODE == entity.STATECODE && detail.FLAG == "0")
                                .ToList().ForEach(item =>
                                {
                                    item.FLAG = "1";
                                    item.CHECKSTATE = "8";
                                    UpdateFlowRecord2(item);
                                });
                        #endregion 
                    }

                    #endregion 

                }
                else
                {
                    if (!isGotoNextState)
                    {
                        #region 
                        
                        UpdateFlowRecord2(entity);
                        dataResult.AppState = entity.STATECODE;
                        dataResult.FlowResult = FlowResult.SUCCESS;
                        dataResult.CheckState = "1";                        
                        #endregion 
                    }
                    else
                    {
                        #region 获取下一状态数据
                        List<string> User = new List<string>();
                        User.Add(entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID);
                        User.Add(submitData.ApprovalUser.UserID);

                        List<string> tmpPostID = new List<string>();
                        tmpPostID.Add(entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID);
                        tmpPostID.Add(entity.EDITPOSTID);
                        GetUserByInstance2(entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID, workflowRuntime, instance, entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, submitData.XML, entity.STATECODE, User, tmpPostID, submitData.FlowType, ref dataResult);
                        
                        if (dataResult.FlowResult == FlowResult.FAIL)
                        {
                            return dataResult;
                        }
                        submitData.NextStateCode = dataResult.AppState;
                        if (dataResult.IsCountersign)
                        {
                            #region
                            if (dataResult.FlowResult == FlowResult.Countersign)
                            {
                                if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                                {
                                    return dataResult;
                                }
                            }
                            else
                            {
                                if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                                {
                                    submitData.DictCounterUser = dataResult.DictCounterUser;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 
                            if (dataResult.FlowResult == FlowResult.MULTIUSER)
                            {
                                if (submitData.NextApprovalUser == null || (Utility.GetString(submitData.NextApprovalUser.UserID) == "" || Utility.GetString(submitData.NextApprovalUser.UserName) == ""))
                                {
                                    return dataResult;
                                }
                            }
                            else
                            {
                                if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                                {
                                    submitData.NextApprovalUser = dataResult.UserInfo[0];
                                }
                            }
                            #endregion 
                        }

                        #endregion

                        #region 对于单会签情况，需要将其他审核人的审核设为会签通过状态

                        if (currentIsCountersign && currentCountersignType=="1")
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T
                               .Where(detail => detail.FLOWRECORDDETAILID != entity.FLOWRECORDDETAILID && detail.STATECODE == entity.STATECODE && detail.FLAG == "0")
                               .ToList().ForEach(item =>
                               {
                                   item.FLAG = "1";
                                   item.CHECKSTATE = "7";
                                   UpdateFlowRecord2(item);
                               });
                        }
                        #endregion 


                        #region                   
                       
                        FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                        FlowData.xml = submitData.XML;
                        workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                        {
                            instance = null;

                        };

                        #region 处理kpi时间
                        string KPITime = "";
                        //PerformanceServiceWS.PerformanceServiceClient psc = new PerformanceServiceWS.PerformanceServiceClient();
                        string pscResult = entity.FLOW_FLOWRECORDMASTER_T.KPITIMEXML;
                        //psc.Close();
                        if (!string.IsNullOrEmpty(pscResult))
                        {
                            XElement xe = XElement.Parse(pscResult);
                            Func<XElement, bool> f = (x) =>
                            {
                                XAttribute xid = x.Attribute("id");
                                XAttribute xvalue = x.Attribute("value");
                                if (xid == null || xvalue == null)
                                    return false;
                                else
                                {
                                    if (xid.Value == dataResult.AppState)
                                        return true;
                                    else return false;
                                }
                            };
                            XElement FlowNode = xe.Elements("FlowNode").FirstOrDefault(f);
                            if (FlowNode != null)
                            {
                                KPITime = FlowNode.Attribute("value").Value;
                            }
                        }
                        dataResult.KPITime = KPITime;

                        #endregion 

                        if (!dataResult.IsCountersign)
                        {
                            #region  非会签                           
                           
                            UserInfo AppUser = submitData.NextApprovalUser;
                            dataResult.UserInfo.Clear();
                            dataResult.UserInfo.Add(AppUser);
                            UserInfo AgentAppUser = GetAgentUserInfo(submitData.ModelCode, AppUser.UserID);//查询是否启用了代理人                            
                            dataResult = DoFlowRecord2(workflowRuntime, instance, entity, submitData.NextStateCode, AppUser, AgentAppUser, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                            dataResult.AgentUserInfo = AgentAppUser;
                            dataResult.IsCountersign = false;                           
                            #endregion
                        }
                        else
                        {
                            #region  会签
                            
                            dataResult.DictCounterUser = submitData.DictCounterUser;                           
                            Dictionary<UserInfo, UserInfo> dictAgentUserInfo = GetAgentUserInfo2(submitData.ModelCode, submitData.DictCounterUser);                            
                            dataResult = DoFlowRecord_Approval(workflowRuntime, instance, entity, submitData.NextStateCode, submitData.DictCounterUser, dictAgentUserInfo, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                            dataResult.DictAgentUserInfo = dictAgentUserInfo;
                            dataResult.IsCountersign = true;

                           
                            #endregion
                        }

                        #region 激发流程引擎执行到一下流程
                        if (dataResult.AppState == null || dataResult.AppState == "")
                            workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                        else
                        {
                            StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);

                            workflowinstance.SetState(dataResult.AppState); //流程跳转到指定节点
                        }
                        #endregion 
                        //dataResult.CanSendMessage = true;
                        
                        System.Threading.Thread.Sleep(1000);
                        if (submitData.FlowType == FlowType.Task)
                            dataResult.SubModelCode = Utility.GetSubModelCode(entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, dataResult.AppState); //返回下一子模块代码

                        #endregion 
                    }
                   

                }
                dataResult.CurrentIsCountersign = currentIsCountersign;
                dataResult.IsGotoNextState = isGotoNextState;
                return dataResult;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
               
                entity = null;
                instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(workflowRuntime);
            }
        }

        #endregion
        #endregion

        #region 自选流程审批
        public DataResult AddFreeFlow(SubmitData ApprovalData, DataResult APPDataResult)
        {
            WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;
            FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
            entity.FLOW_FLOWRECORDMASTER_T = new FLOW_FLOWRECORDMASTER_T();
            UserInfo AppUser = new UserInfo(); //下一审核人
            UserInfo AgentAppUser = new UserInfo(); //代理下一审核人
            try
            {
                entity.FLOW_FLOWRECORDMASTER_T.BUSINESSOBJECT = ApprovalData.XML;
                entity.FLOW_FLOWRECORDMASTER_T.FORMID = ApprovalData.FormID;
                entity.FLOW_FLOWRECORDMASTER_T.MODELCODE = ApprovalData.ModelCode;

                // entity.FLOWRECORDDETAILID = FlowGUID;
                entity.CREATECOMPANYID = ApprovalData.ApprovalUser.CompanyID;
                entity.CREATEDEPARTMENTID = ApprovalData.ApprovalUser.DepartmentID;
                entity.CREATEPOSTID = ApprovalData.ApprovalUser.PostID;
                entity.CREATEUSERID = ApprovalData.ApprovalUser .UserID;
                entity.CREATEUSERNAME = ApprovalData.ApprovalUser.UserName;
                entity.FLOW_FLOWRECORDMASTER_T.FLOWTYPE = ((int)ApprovalData.FlowType).ToString();
                entity.FLOW_FLOWRECORDMASTER_T.FLOWSELECTTYPE = ((int)ApprovalData.FlowSelectType).ToString();
                entity.FLOW_FLOWRECORDMASTER_T.FLOWCODE = "FreeFlow";
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime, "FreeFlow.xml");

                entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID = instance.InstanceId.ToString();


                //下一审核人赋值
                AppUser = ApprovalData.NextApprovalUser ;

                APPDataResult.RunTime += "---DoFlowStart:" + DateTime.Now.ToString();
                ApprovalData.NextStateCode = "Approval";
                AgentAppUser = GetAgentUserInfo(ApprovalData.ModelCode, AppUser.UserID); //查询是否启用了代理人

                APPDataResult = DoFlowRecord(workflowRuntime, instance, entity, ApprovalData.NextStateCode, AppUser, AgentAppUser, ApprovalData.SubmitFlag, ApprovalData.FlowType); //处理流程数据
                APPDataResult.AgentUserInfo = AgentAppUser;
                APPDataResult.RunTime += "---DoFlowEnd:" + DateTime.Now.ToString();
                return APPDataResult;
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            finally
            {
                AppUser = null;
                AgentAppUser = null;
                entity = null;
                instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(workflowRuntime);
            }
        }

        

        public DataResult ApprovalFreeFlow(SubmitData ApprovalData, DataResult APPDataResult, List<FLOW_FLOWRECORDDETAIL_T> fd)
        {
            WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;
            FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
            entity.FLOW_FLOWRECORDMASTER_T = new FLOW_FLOWRECORDMASTER_T();
            UserInfo AppUser = new UserInfo(); //下一审核人
            UserInfo AgentAppUser = new UserInfo(); //代理下一审核人
            try
            {
                APPDataResult.RunTime += "---GetAppInfoStart:" + DateTime.Now.ToString();
                List<FLOW_FLOWRECORDDETAIL_T> tmp = fd.Where(c => (c.EDITUSERID == ApprovalData.ApprovalUser.UserID || c.AGENTUSERID == ApprovalData.ApprovalUser.UserID) && c.FLAG == "0").ToList();
                APPDataResult.RunTime += "---GetAppInfoEnd:" + DateTime.Now.ToString();
                if (tmp == null)
                {
                    APPDataResult.FlowResult = FlowResult.FAIL;
                    APPDataResult.Err = "没有找到待审核信息";
                    // DataResult.UserInfo = null;
                    return APPDataResult;
                }

                entity = tmp[0];
                entity.EDITDATE = DateTime.Now;  //审批时间

                if (entity.AGENTUSERID == ApprovalData.ApprovalUser.UserID)
                {
                    entity.AGENTEDITDATE = entity.EDITDATE;  //代理审批时审批时间与代理审批时间到致
                }
                //entity.EditUserID = AppUserId;
                entity.CONTENT = ApprovalData.ApprovalContent;
                entity.CHECKSTATE = ((int)ApprovalData.ApprovalResult).ToString();


                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime, tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID);// workflowRuntime.GetWorkflow(new Guid(tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID));

                //不同意状态处理
                if (ApprovalData.ApprovalResult == ApprovalResult.NoPass)
                {
                    instance.Terminate("0");
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "3"; //设为终审不通过
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = ApprovalData.ApprovalUser.UserID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = ApprovalData.ApprovalUser.UserName;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;
                    UpdateFlowRecord(entity, ApprovalData.NextStateCode, ApprovalData.NextApprovalUser .UserID );
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    APPDataResult.CheckState = "3";//
                    APPDataResult.FlowResult = FlowResult.END;
                    // DataResult.UserInfo = null;
                    // return DataResult;

                }
                else
                {


                    //下一审核人赋值
                    AppUser = ApprovalData.NextApprovalUser;

                    AgentAppUser = GetAgentUserInfo(ApprovalData.ModelCode, AppUser.UserID); //查询是否启用了代理人

                    FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                    FlowData.xml = ApprovalData.XML;



                    workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                    {
                        instance = null;

                    };

                    ApprovalData.NextStateCode = ApprovalData.NextStateCode == "EndFlow" ? "EndFlow" : "Approval";
                         APPDataResult.RunTime += "---DoFlowStart:" + DateTime.Now.ToString();

                    APPDataResult = DoFlowRecord(workflowRuntime, instance, entity, ApprovalData.NextStateCode, AppUser, AgentAppUser, ApprovalData.SubmitFlag, ApprovalData.FlowType); //处理流程数据
                    APPDataResult.AgentUserInfo = AgentAppUser;
                    APPDataResult.RunTime += "---DoFlowEnd:" + DateTime.Now.ToString();
                    if (ApprovalData.NextStateCode == "EndFlow")
                    {
                        workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                        System.Threading.Thread.Sleep(1000);
                    }
                   

                   
                }
                return APPDataResult;
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            finally
            {
                AppUser = null;
                AgentAppUser = null;
                entity = null;
                instance = null;
                SMTWorkFlowManage.ColseWorkFlowRuntime(workflowRuntime);
            }
        }
        #endregion


        #region 查询是否使用代理
        public UserInfo GetAgentUserInfo(string ModelCode, string UserID)
        {
            UserInfo AgentUser = new UserInfo();
            try
            {
                OAWS.AgentServicesClient oa = new OAWS.AgentServicesClient();
                //OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent("TravelApplication1", "da844654-49c4-4138-ad83-e369cf03af5c");
                OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent( UserID,ModelCode);
                if (AGENTSET == null)
                    return null;

                AgentUser.UserID = AGENTSET.EMPLOYEEID;//"userid0";
                AgentUser.UserName = AGENTSET.EMPLOYEECNAME;//"testuser";

                return AgentUser;
            }
            catch (Exception e)
            {

                throw new Exception("GetAgentUserInfo:" + e.Message);
            }
            finally
            {
                AgentUser = null;

            }

        }
        //查找会签角色
        public Dictionary<UserInfo,UserInfo> GetAgentUserInfo2(string ModelCode,Dictionary<Role_UserType,List<UserInfo>> dictUserInfo)
        {
            Dictionary<UserInfo, UserInfo> dict = new Dictionary<UserInfo, UserInfo>();
            
            try
            {
                dictUserInfo.Values.ToList().ForEach(users =>
                    {
                        users.ForEach(user =>
                            {
                                UserInfo AgentUser = new UserInfo();
                                OAWS.AgentServicesClient oa = new OAWS.AgentServicesClient();
                                //OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent("TravelApplication1", "da844654-49c4-4138-ad83-e369cf03af5c");
                                OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent(user.UserID, ModelCode);
                                if (AGENTSET != null)
                                {
                                    AgentUser.UserID = AGENTSET.EMPLOYEEID;//"userid0";
                                    AgentUser.UserName = AGENTSET.EMPLOYEECNAME;//"testuser";
                                    dict[user] = AgentUser;
                                }
                            });
                    });
               

                return dict;
            }
            catch (Exception e)
            {

                throw new Exception("GetAgentUserInfo:" + e.Message);
            }
            finally
            {
               

            }

        }
        #endregion


        /// <summary>
        /// 构建引擎消息
        /// </summary>
        /// <param name="EngineMessageData"></param>
        /// <returns></returns>
        public StringBuilder BuildMessageData(MessageData EngineMessageData)
        {
           

            StringBuilder FlowResultXml = new StringBuilder(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            FlowResultXml.Append(Environment.NewLine);
            FlowResultXml.Append(@"    <System>");
            FlowResultXml.Append(Environment.NewLine);
            FlowResultXml.Append(@"       <Name>""" + EngineMessageData.MessageSystemCode + @"""</Name>");
            FlowResultXml.Append(Environment.NewLine);
            FlowResultXml.Append(@"       <SystemCode>""" + EngineMessageData.SystemCode + @"""</SystemCode>");
            FlowResultXml.Append(Environment.NewLine);
            FlowResultXml.Append(@"       <Message>");
            FlowResultXml.Append(@"           <Attribute Name=""CompanyID""  DataValue=""" + EngineMessageData.CompanyID + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""ModelCode""  DataValue=""" + EngineMessageData.ModelCode + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""ModelName""  DataValue=""" + EngineMessageData.ModelName + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""FormID""     DataValue=""" + EngineMessageData.FormID + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""StateCode""  DataValue=""" + EngineMessageData.StateCode + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""CheckState""  DataValue=""" + EngineMessageData.CheckState + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""IsTask""     DataValue=""" + EngineMessageData.IsTask + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""AppUserID""  DataValue=""" + EngineMessageData.AppUserID + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""AppUserName""  DataValue=""" + EngineMessageData.AppUserName + @"""></Attribute>");
            FlowResultXml.Append(@"           <Attribute Name=""OutTime""  DataValue=""" + EngineMessageData.KPITime + @"""></Attribute>");
            FlowResultXml.Append(@"       </Message>");
            FlowResultXml.Append(@"     </System>");

            return FlowResultXml;
        }

        public string GetFlowDefine(SubmitData ApprovalData)
        {
            try
            {

           
            List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(ApprovalData.ApprovalUser.CompanyID, ApprovalData.ApprovalUser.DepartmentID, ApprovalData.ModelCode, ((int)ApprovalData.FlowType).ToString());
           

            if (MODELFLOWRELATION == null || MODELFLOWRELATION.Count == 0)
            {
               
                return null;
            }
            return MODELFLOWRELATION.First().FLOW_FLOWDEFINE_T.LAYOUT;
            }
            catch (Exception e)
            {

                throw e ;
            }
        }

        public bool IsExistFlowDataByUserID(string UserID, string PostID)
        {
            try
            {


                return FLOW_FLOWRECORDMASTER_TDAL.IsExistFlowDataByUserID(UserID,PostID);
            }
            catch (Exception e)
            {

                throw e;
            } 
        }



        public List<FLOW_FLOWRECORDMASTER_T> GetFlowDataByUserID(string UserID)
        {
            try
            {

                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(FlowType.Approval);
              
                //FlowTypeList.Add(2);


                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo("", "", "", "0", "", "", UserID, FlowTypeList);
                List<FLOW_FLOWRECORDMASTER_T> fd2 = FlowBLL.GetFlowRecordBySubmitUserID("1", UserID);
                if (fd != null)
                {
                    if (fd2 == null)
                        fd2 = new List<FLOW_FLOWRECORDMASTER_T>();
                    foreach (FLOW_FLOWRECORDDETAIL_T item in fd)
                    {

                        fd2.Add(item.FLOW_FLOWRECORDMASTER_T);
                    }
                }
            
            return fd2;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        #region 检查流程参数是否符合规则

        public static bool CheckFlowData(SubmitData ApprovalData, ref DataResult APPDataResult)
        {
            try
            {

                if (ApprovalData.FormID == null || ApprovalData.FormID == "")
                {
                    APPDataResult.Err = "业务对象的FORMID为空";
                    return false;
                }

                if (ApprovalData.ModelCode == null || ApprovalData.ModelCode == "")
                {
                    APPDataResult.Err = "模块代码为空";
                    return false;
                }

                //if (ApprovalData.SubmitFlag == null || (ApprovalData.SubmitFlag != SubmitFlag.New && ApprovalData.SubmitFlag != SubmitFlag.Approval))
                //{
                //    APPDataResult.Err = "流程提交标志(SubmitFlag)有误,需要设置成SubmitFlag.New或者SubmitFlag.Approval";
                //    return false;
                //}

                if (ApprovalData.SubmitFlag == null )
                {
                    APPDataResult.Err = "流程提交标志(SubmitFlag)不能为空";
                    return false;
                }

                if (ApprovalData.FlowSelectType == null || (ApprovalData.FlowSelectType != FlowSelectType.FixedFlow && ApprovalData.FlowSelectType != FlowSelectType.FreeFlow))
                {
                    APPDataResult.Err = "流程审批类型设置有误，应设置成FlowSelectType.FixedFlow或FlowSelectType.FreeFlow";
                    return false;
                }

                if (ApprovalData.ApprovalUser == null )
                {
                    APPDataResult.Err = "提交用户信息不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.CompanyID == null || ApprovalData.ApprovalUser.CompanyID=="")
                {
                    APPDataResult.Err = "提交用户所属公司不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.DepartmentID == null || ApprovalData.ApprovalUser.DepartmentID == "")
                {
                    APPDataResult.Err = "提交用户所属部门不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.PostID == null || ApprovalData.ApprovalUser.PostID == "")
                {
                    APPDataResult.Err = "提交用户所属岗位不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.UserID == null || ApprovalData.ApprovalUser.UserID == "")
                {
                    APPDataResult.Err = "提交用户ID不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.UserName == null || ApprovalData.ApprovalUser.UserName == "")
                {
                    APPDataResult.Err = "提交用户名称不能为空";
                    return false;
                }

                if (ApprovalData.NextApprovalUser != null)
                {
                    if ((ApprovalData.NextApprovalUser.CompanyID != null && ApprovalData.NextApprovalUser.CompanyID != "")
                        || (ApprovalData.NextApprovalUser.DepartmentID != null && ApprovalData.NextApprovalUser.DepartmentID != "")
                        || (ApprovalData.NextApprovalUser.PostID != null && ApprovalData.NextApprovalUser.PostID != "")
                        || (ApprovalData.NextApprovalUser.UserID != null && ApprovalData.NextApprovalUser.UserID != "")
                        || (ApprovalData.NextApprovalUser.UserName != null && ApprovalData.NextApprovalUser.UserName != ""))
                    {
                        if (ApprovalData.NextStateCode == null || ApprovalData.NextStateCode == "")
                        {
                            //APPDataResult.Err = "设置了下一审核人时下一审核节点代码不能为空";
                            //return false;
                        }
                        else if (ApprovalData.NextApprovalUser.CompanyID == null || ApprovalData.NextApprovalUser.CompanyID == "")
                        {
                            //APPDataResult.Err = "下一审核用户所属公司不能为空";
                            //return false;
                        }
                        else if (ApprovalData.NextApprovalUser.DepartmentID == null || ApprovalData.NextApprovalUser.DepartmentID == "")
                        {
                            //APPDataResult.Err = "下一审核用户所属部门不能为空";
                            //return false;
                        }
                        else if (ApprovalData.NextApprovalUser.PostID == null || ApprovalData.NextApprovalUser.PostID == "")
                        {
                            //APPDataResult.Err = "下一审核用户所属岗位不能为空";
                            //return false;
                        }
                        else if (ApprovalData.NextApprovalUser.UserID == null || ApprovalData.NextApprovalUser.UserID == "")
                        {
                            //APPDataResult.Err = "下一审核用户ID不能为空";
                            //return false;
                        }
                        else if (ApprovalData.NextApprovalUser.UserName == null || ApprovalData.NextApprovalUser.UserName == "")
                        {
                            //APPDataResult.Err = "下一审核用户名称不能为空";
                            //return false;
                        }
                    }
                    else if (ApprovalData.NextStateCode != null && ApprovalData.NextStateCode != "")
                    {
                        //APPDataResult.Err = "未设置下一审核人时，不能设置下一审核节点代码";
                        //return false;
                    }
                }
                else if (ApprovalData.NextStateCode != null && ApprovalData.NextStateCode != "")
                {
                    //APPDataResult.Err = "未设置下一审核人时，不能设置下一审核节点代码";
                    //return false;
                }
          
                return true;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        #endregion
    }
}

