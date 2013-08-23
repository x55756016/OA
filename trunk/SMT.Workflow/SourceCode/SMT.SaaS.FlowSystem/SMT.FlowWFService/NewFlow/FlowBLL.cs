/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：FlowBLL.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2011/12/15 08:51:55   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.FlowWFService
	 * 模块名称：
	 * 描　　述： 对原有的FlowBLL.cs进行优化和简化调整
* ---------------------------------------------------------------------*/
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
using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;
using System.Data.SqlClient;
using System.Configuration;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.OrganizationWS;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.Workflow.Common.Model.FlowEngine;
using System.Collections;
using System.Workflow.Runtime.Hosting;

namespace SMT.FlowWFService.NewFlow
{
    public class FlowBLL
    {
        public WorkflowRuntime workflowRuntime = null;
        //   OracleConnection con = ADOHelper.GetOracleConnection();

        #region 咨询
        public void AddConsultation(OracleConnection con, FLOW_CONSULTATION_T flowConsultation)
        {
            FLOW_CONSULTATION_TDAL.Add(con, flowConsultation);
            //FLOWDAL.FLOW_CONSULTATION_TDAL dal = new FLOW_CONSULTATION_TDAL();
            //dal.AddConsultation(flowConsultation);
        }
        public void ReplyConsultation(OracleConnection con, FLOW_CONSULTATION_T flowConsultation)
        {
            FLOW_CONSULTATION_TDAL.Update(con, flowConsultation);
            //FLOWDAL.FLOW_CONSULTATION_TDAL dal = new FLOW_CONSULTATION_TDAL();
            //dal.ReplyConsultation(flowConsultation);
        }

        public static FLOW_FLOWRECORDMASTER_T GetFLOW_FLOWRECORDMASTER_T(OracleConnection con, string masterID)
        {
            return FLOW_FLOWRECORDMASTER_TDAL.GetFLOW_FLOWRECORDMASTER_T(con, masterID);
        }
        #endregion

        #region 处理审批数据


        /// <summary>
        /// 自选流程使用:流程数据处理(对应SubmitFlow)对数据库操作
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
        public DataResult DoFlowRecord(OracleConnection con, WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, UserInfo AppUser, UserInfo AgentUser, SubmitFlag SubmitFlag, FlowType FlowType,ref FlowUser fUser)
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
                        FLOW_FLOWRECORDMASTER_TDAL.Add(con, entity.FLOW_FLOWRECORDMASTER_T);//对数据库操作
                        fUser.NextEditUserID = entity.EDITUSERID;
                        fUser.NextEditUserName = entity.EDITUSERNAME;
                        AddFlowRecord(con, entity, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        return tmpDataResult;
                    }

                    #endregion

                    FLOW_FLOWRECORDMASTER_TDAL.Add(con, entity.FLOW_FLOWRECORDMASTER_T);//对数据库操作
                    fUser.NextEditUserID = entity.EDITUSERID;
                    fUser.NextEditUserName = entity.EDITUSERNAME;
                    AddFlowRecord(con, entity, NextStateCode, AppUser.UserID);



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
                        fUser.NextEditUserID = entity2.EDITUSERID;
                        fUser.NextEditUserName = entity2.EDITUSERNAME;
                        AddFlowRecord(con, entity2, NextStateCode, AppUser.UserID);//对数据库操作
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



                    entity = UpdateFlowRecord(con, entity, NextStateCode, AppUser.UserID);//对数据库操作

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

                        UpdateFlowRecord(con, entity, NextStateCode, AppUser.UserID);//对数据库操作
                        FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);//对数据库操作
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
                        fUser.NextEditUserID = entity2.EDITUSERID;
                        fUser.NextEditUserName = entity2.EDITUSERNAME;
                        AddFlowRecord(con, entity2, NextStateCode, AppUser.UserID);//对数据库操作
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
                LogHelper.WriteLog("提交自选流程数据出错,DoFlowRecord异常信息 ：" + ex.ToString());
                throw new Exception("提交自选流程数据出错!请联系管理员!");
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }
        /// <summary>
        /// 非会签是使用
        /// </summary>
        /// <param name="con"></param>
        /// <param name="workflowRuntime"></param>
        /// <param name="instance"></param>
        /// <param name="entity"></param>
        /// <param name="NextStateCode"></param>
        /// <param name="AppUser"></param>
        /// <param name="AgentUser"></param>
        /// <param name="SubmitFlag"></param>
        /// <param name="FlowType"></param>
        /// <returns></returns>
        public DataResult DoFlowRecord2(OracleConnection con, WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, UserInfo AppUser, UserInfo AgentUser, SubmitFlag SubmitFlag, FlowType FlowType,ref FlowUser fUser)
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
                        fUser.NextEditUserID = entity.EDITUSERID;
                        fUser.NextEditUserName = entity.EDITUSERNAME;
                        FLOW_FLOWRECORDMASTER_TDAL.Add(con, entity.FLOW_FLOWRECORDMASTER_T);
                        AddFlowRecord(con, entity, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        return tmpDataResult;
                    }

                    #endregion

                    FLOW_FLOWRECORDMASTER_TDAL.Add(con, entity.FLOW_FLOWRECORDMASTER_T);
                    fUser.NextEditUserID = entity.EDITUSERID;
                    fUser.NextEditUserName = entity.EDITUSERNAME;
                    AddFlowRecord(con, entity, NextStateCode, AppUser.UserID);

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
                        fUser.NextEditUserID = entity2.EDITUSERID;
                        fUser.NextEditUserName = entity2.EDITUSERNAME;
                        AddFlowRecord(con, entity2, NextStateCode, AppUser.UserID);
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



                    entity = UpdateFlowRecord(con, entity, NextStateCode, AppUser.UserID);
                    string stateCode = "";
                    if (NextStateCode.ToUpper() == "ENDFLOW")
                    {
                        stateCode = NextStateCode;
                    }
                    else
                    {
                        stateCode = string.IsNullOrEmpty(NextStateCode) ? SMTWorkFlowManage.GetNextState(workflowRuntime, instance, entity.STATECODE) : NextStateCode;
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

                        UpdateFlowRecord(con, entity, NextStateCode, AppUser.UserID);

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
                        fUser.NextEditUserID = entity2.EDITUSERID;
                        fUser.NextEditUserName = entity2.EDITUSERNAME;
                        AddFlowRecord(con, entity2, NextStateCode, AppUser.UserID);
                        tmpDataResult.FlowResult = FlowResult.SUCCESS;
                        tmpDataResult.CheckState = "1";
                        #endregion
                        #region 更新审核主表的审核人(龙康才新增)
                        entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "1"; 
                        if (entity.EDITDATE == entity.AGENTEDITDATE)  
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
                        FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                       
                        #endregion
                    }

                    tmpDataResult.IsCountersignComplete = true;
                    return tmpDataResult;   //如有下一节点，返回SUCCESS

                    #endregion

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("DoFlowRecord2异常信息 ：" + ex.ToString());
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

        public DataResult DoFlowRecord_Add(OracleConnection con, WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, Dictionary<Role_UserType, List<UserInfo>> dictUserInfo, Dictionary<UserInfo, UserInfo> dictAgentUserInfo, SubmitFlag SubmitFlag, FlowType FlowType)
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
                    FLOW_FLOWRECORDMASTER_TDAL.Add(con, entity.FLOW_FLOWRECORDMASTER_T);
                    AddFlowRecord2(con, entity);

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
                    AddFlowRecord2(con, entity);
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    #endregion
                }
                //System.Threading.Thread.Sleep(1000);
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
                            AddFlowRecord2(con, entity2);

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
                LogHelper.WriteLog("DoFlowRecord_Add异常信息 ：" + ex.ToString());
                throw new Exception("DoFlowRecord_Add:" + ex.InnerException + ex.Message);
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }
        /// <summary>
        /// 回定流程中,会签
        /// </summary>
        /// <param name="con"></param>
        /// <param name="workflowRuntime"></param>
        /// <param name="instance"></param>
        /// <param name="entity"></param>
        /// <param name="NextStateCode"></param>
        /// <param name="dictUserInfo"></param>
        /// <param name="dictAgentUserInfo"></param>
        /// <param name="SubmitFlag"></param>
        /// <param name="FlowType"></param>
        /// <returns></returns>
        public DataResult DoFlowRecord_Approval(OracleConnection con, WorkflowRuntime workflowRuntime, WorkflowInstance instance, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, Dictionary<Role_UserType, List<UserInfo>> dictUserInfo, Dictionary<UserInfo, UserInfo> dictAgentUserInfo, SubmitFlag SubmitFlag, FlowType FlowType)
        {
            DataResult tmpDataResult = new DataResult();
            tmpDataResult.DictCounterUser = dictUserInfo;

            try
            {


                #region 更新流程
                //如果NextStateCode为空则自动获取模型中的下一状态，如果不为空则使用传入状态代码，并使用传入下一任务人ID

                //更新本流程



                entity = UpdateFlowRecord2(con, entity);
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

                    UpdateFlowRecord2(con, entity);
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
                            AddFlowRecord2(con, entity2);

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
                LogHelper.WriteLog("DoFlowRecord_Approval异常信息 ：" + ex.ToString());
                throw new Exception("DoFlowRecord_Approval:" + ex.InnerException + ex.Message);
                //tmpDataResult.FlowResult = FlowResult.FAIL;
                //tmpDataResult.Err = ex.Message;
                //return tmpDataResult;
            }
        }


        #endregion

        #region 操作流程数据
        /// <summary>
        /// 新增[流程审批明细表]
        /// </summary>
        /// <param name="entity">流程审批明细表</param>
        /// <param name="NextStateCode">下一个状态代码</param>
        /// <param name="EditUserId">编辑用户ID</param>
        void AddFlowRecord(OracleConnection con, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, string EditUserId)
        {
            FLOW_FLOWRECORDDETAIL_TDAL.Add(con, entity);
        }
        /// <summary>
        /// 新增[流程审批明细表]
        /// </summary>
        /// <param name="entity">流程审批明细表</param>
        void AddFlowRecord2(OracleConnection con, FLOW_FLOWRECORDDETAIL_T entity)
        {
            FLOW_FLOWRECORDDETAIL_TDAL.Add(con, entity);
        }
        /// <summary>
        /// 更新[流程审批明细表]
        /// </summary>
        /// <param name="entity">流程审批明细表</param>
        /// <param name="NextStateCode">下一个状态代码</param>
        /// <param name="EditUserId">编辑用户ID</param>
        /// <returns></returns>
        public FLOW_FLOWRECORDDETAIL_T UpdateFlowRecord(OracleConnection con, FLOW_FLOWRECORDDETAIL_T entity, string NextStateCode, string EditUserId)
        {
            entity.FLAG = "1";
            FLOW_FLOWRECORDDETAIL_TDAL.Update(con, entity);
            return entity;
        }
        /// <summary>
        /// 更新[流程审批明细表]
        /// </summary>
        /// <param name="entity">流程审批明细表</param>
        /// <returns></returns>
        public FLOW_FLOWRECORDDETAIL_T UpdateFlowRecord2(OracleConnection con, FLOW_FLOWRECORDDETAIL_T entity)
        {
            FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            entity.FLAG = "1";
            FLOW_FLOWRECORDDETAIL_TDAL.Update(con, entity);
            return entity;
        }

        #endregion

        #region 查询流程信息

        /// <summary>
        /// 获取流程信息(对数据库操作)
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="FlowGUID">明细ID</param>
        /// <param name="CheckState">审批状态(同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8)</param>
        /// <param name="Flag">审批状态（已审批：1，未审批:0）</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="EditUserID">操作人</param>
        /// <param name="FlowType">流程类型（0:审批流程，1：任务流程）</param>
        /// <returns></returns>       
        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfo(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {

            try
            {
                List<string> FlowTypeList = new List<string>();

                FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
                var dt = FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecord(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));

                if (dt.Count > 0)
                    return dt;
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("获取流程信息GetFlowInfo异常信息(FormID=" + FormID + ";ModelCode=" + ModelCode + ";CompanyID=" + CompanyID + ";EditUserID=" + EditUserID + ")：" + ex.Message);
                throw new Exception("获取流程信息出错,请联系管理员! \r\n FormID=" + FormID + "");
            }


        }
        /// <summary>
        /// 获取流程信息
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="FlowGUID">明细ID</param>
        /// <param name="CheckState">审批状态(同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8)</param>
        /// <param name="Flag">审批状态（已审批：1，未审批:0）</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="EditUserID">操作人</param>
        /// <param name="FlowType">流程类型（0:审批流程，1：任务流程）</param>
        /// <returns></returns>       
        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfoV(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {

            try
            {
                var dt = FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecordV(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));

                if (dt.Count > 0)
                    return dt;
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GetFlowInfoV异常信息：" + ex.Message);
                throw new Exception("GetFlowInfoV:" + ex.Message);
            }


        }
        /// <summary>
        /// 获取流程信息
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="FlowGUID">明细ID</param>
        /// <param name="CheckState">审批状态(同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8)</param>
        /// <param name="Flag">审批状态（已审批：1，未审批:0）</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="EditUserID">操作人</param>
        /// <param name="FlowType">流程类型（0:审批流程，1：任务流程）</param>
        /// <returns></returns>           
        public static List<FLOW_FLOWRECORDDETAIL_T> GetFlowInfoTop(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID, List<FlowType> FlowType)
        {
            return FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecordTop(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, Utility.FlowTypeListToStringList(FlowType));
        }
        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="FlowGUID">明细ID</param>
        /// <param name="CheckState">审批状态(同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8)</param>
        /// <param name="Flag">审批状态（已审批：1，未审批:0）</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="EditUserID">操作人</param>     
        /// <returns></returns>
        public static List<TaskInfo> GetTaskInfo(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            try
            {
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(FlowType.Task);
                List<FLOW_FLOWRECORDDETAIL_T> FLOWRECORDDETAIList = GetFlowInfo(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, FlowTypeList);
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
        /// <summary>
        /// 获取[流程审批实例]信息
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="FlowGUID">明细ID</param>
        /// <param name="CheckState">审批状态(同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8)</param>
        /// <param name="Flag">审批状态（已审批：1，未审批:0）</param>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="CompanyID">创建公司ID</param>
        /// <param name="EditUserID">操作人</param>      
        /// <returns></returns>  
        public static List<FLOW_FLOWRECORDMASTER_T> GetFlowRecordMaster(OracleConnection con, string FormID, string FlowGUID, string CheckState, string Flag, string ModelCode, string CompanyID, string EditUserID)
        {
            //FLOW_FLOWRECORDDETAIL_TDAL Dal = new FLOW_FLOWRECORDDETAIL_TDAL();
            //var dt = Dal.GetFlowRecordMaster(FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID);
            List<FLOW_FLOWRECORDDETAIL_T> listDetail = FLOW_FLOWRECORDDETAIL_TDAL.GetFlowRecord(con, FormID, FlowGUID, CheckState, Flag, ModelCode, CompanyID, EditUserID, null);
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

        #region "查询带审核单据"
        /// <summary>
        /// 根据模块代码和用户id查询待审核单据
        /// </summary>
        /// <param name="ModelCode">模块代码</param>
        /// <param name="EditUserID">用户id</param>
        /// <returns></returns>
        public static List<string> GetWaitingApprovalForm(string ModelCode, string EditUserID)
        {
            var dt = FLOW_FLOWRECORDDETAIL_TDAL.GetWaitingApprovalForm(ModelCode, EditUserID);
            if (dt.Count > 0)
                return dt;
            return null;
        }
        #endregion

        #region 通过模块代码查询系统代码
        /// <summary>
        /// 通过模块代码查询系统代码
        /// </summary>
        /// <param name="ModelCode">模块代码</param>
        /// <returns></returns>
        public static ModelInfo GetSysCodeByModelCode(OracleConnection con, string ModelCode)
        {
            ModelInfo tmpModelInfo = new ModelInfo();
            try
            {
                var dt = FLOW_MODELDEFINE_TDAL.GetModelDefineByCode(con, ModelCode);
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
                LogHelper.WriteLog("GetSysCodeByModelCode异常信息 ：" + ex.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                tmpModelInfo = null;
            }

        }

        #endregion

        #region 通过模块名查找使用流程
        /// <summary>
        /// 通过模块名查找使用流程
        /// </summary>
        /// <param name="CompanyID"></param>
        /// <param name="DepartID"></param>
        /// <param name="ModelCode"></param>
        /// <param name="FlowType"></param>
        /// <returns></returns>
        public static List<FLOW_MODELFLOWRELATION_T> GetFlowByModelName(OracleConnection con, string CompanyID, string DepartID, string ModelCode, string FlowType, ref FlowUser user)
        {
            try
            {
                //Flow_ModelFlowRelation_TDAL Dal = new Flow_ModelFlowRelation_TDAL();
                //以部门查找流程
                LogHelper.WriteLog("以部门查找流程FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName:OrgType='1' ;CompanyID=" + CompanyID + ";DepartID=" + DepartID + ";FlowType=" + FlowType + "");
                List<FLOW_MODELFLOWRELATION_T> xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(con, CompanyID, DepartID, ModelCode, FlowType, "1");
                if (xoml != null && xoml.Count > 0) //找到匹配流程返回
                {
                    #region 写日志
                    if (user != null)
                    {
                        if (CompanyID == user.CompayID && DepartID == user.DepartmentID)
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + ";找到公司[ " + user.CompayName + " ]下部门[ " + user.DepartmentName + " ]的匹配流程返回");
                        }
                    }
                    #endregion
                    return xoml;
                }
                //部门的上级机构(一般是公司)查找流程
                if (user != null)
                {
                    LogHelper.WriteLog("FormID=" + user.FormID + ";没有找到公司[ " + user.CompayName + " ]下部门[ " + user.DepartmentName + " ]的匹配流程返回,继续部门的上级机构查找流程");
                }
                OrganizationServiceClient Organization = new OrganizationServiceClient();
                LogHelper.WriteLog("Organization.InnerChannel.LocalAddress=" + Organization.InnerChannel.LocalAddress.Uri + ";Organization.InnerChannel.RemoteAddress.Uri=" + Organization.InnerChannel.RemoteAddress.Uri);
                Dictionary<string, string> OrganizationList = Organization.GetFatherByDepartmentID(DepartID);
                LogHelper.WriteLog("FormID=" + user.FormID + ";继续查找部门的上级机构");
                if (OrganizationList == null || OrganizationList.Count <= 0)
                {
                    string info = "FormID=" + user.FormID + ";没有找到所在部门的上级机构";
                    #region 写日志
                    if (user != null)
                    {
                        if (DepartID == user.DepartmentID)
                        {
                            info = "FormID=" + user.FormID + ";没有找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构";
                            LogHelper.WriteLog(info);
                        }
                    }
                    #endregion
                    throw new Exception("没有找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构");
                }
                foreach (var item in OrganizationList)
                {
                    if (item.Value == "0")
                    {

                        xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(con, CompanyID, item.Key, ModelCode, FlowType, "0"); //如果上级机构是公司直接查询公司关联流程并返回
                        #region 写日志
                        if (user.CompayID == CompanyID)
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + ";找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构 [" + user.CompayName + "]");
                        }
                        else
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + ";找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构 ");
                        }
                        #endregion
                        return xoml;
                    }

                    xoml = FLOW_MODELFLOWRELATION_TDAL.GetFlowByModelName(con, CompanyID, item.Key, ModelCode, FlowType, "1");

                    if (xoml != null && xoml.Count > 0) //找到匹配流程返回
                    {
                        #region 写日志
                        if (user.CompayID == CompanyID)
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + ";找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构 [" + user.CompayName + "]");
                        }
                        else
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + ";找到所在部门[ " + user.DepartmentName + " ]的上一级上级机构 ");
                        }
                        #endregion
                        return xoml;
                    }

                }
                return xoml;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FlowBLL->GetFlowByModelName：" + ex.Message);
                throw new Exception("GetFlowByModelName:" + ex.Message);// return null;
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
        public void GetUserByFlow(string companyID, string Xoml, string Rules, string Layout, string xml, string UserID, string PostID, FlowType FlowType, ref DataResult DataResult)
        {
            string Msg = "companyID=" + companyID + "\r\n";
            Msg += "Xoml=" + Xoml + "\r\n";
            Msg += "Rules=" + Rules + "\r\n";
            Msg += "Layout=" + Layout + "\r\n";
            Msg += "xml=" + xml + "\r\n";
            Msg += "UserID=" + UserID + "\r\n";
            Msg += "PostID=" + PostID + "\r\n";
            Msg += "FlowType=" + FlowType + "\r\n";

            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            string strCurrState = "StartFlow";
            string strNextState = "StartFlow";

            Role_UserType RuleName;
            List<UserInfo> AppUserInfo = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime,Xoml, Rules);
                LogHelper.WriteLog("GetUserByFlow创建工作流实例ID=" + Instance.InstanceId);
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
                        AppUserInfo = GetUserByStateCode(RuleName.RoleName, UserID, PostID, ref isHigher);
                        #region 打印审核人
                        string names = "\r\n=======打印审核人A(listRole[0].RoleName=" + RuleName.RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                        foreach (var user in AppUserInfo)
                        {
                            names += "CompanyID:" + user.CompanyID + "\r\n";
                            names += "DepartmentID:" + user.DepartmentID + "\r\n";
                            names += "PostID:" + user.PostID + "\r\n";
                            names += "UserID:" + user.UserID + "\r\n";
                           

                            names += "CompanyName:" + user.CompanyName + "\r\n";
                            names += "DepartmentName:" + user.DepartmentName + "\r\n";
                            names += "PostName:" + user.PostName + "\r\n";
                            names += "UserName:" + user.UserName + "\r\n";
                            names += "----------------------------------------------------\r\n";
                        }
                        if (!isHigher && RuleName.IsOtherCompany != null)
                        {
                            if (RuleName.IsOtherCompany.Value == true)
                            {
                                names += "是否指定公司：" + RuleName.IsOtherCompany.Value.ToString() + "\r\n";
                                names += "公司的ID：" + RuleName.OtherCompanyID + "\r\n";
                                if (string.IsNullOrEmpty(RuleName.OtherCompanyID))
                                {
                                    names += "Layout=" + Layout + "\r\n";
                                }
                            }
                            else if (RuleName.IsOtherCompany.Value == false)
                            {
                                names += "实际要查找公司的ID:" + companyID + "\r\n";
                                
                            }
                        }

                        LogHelper.WriteLog(names);
                        #endregion
                        #region beyond
                        if (!isHigher && RuleName.IsOtherCompany != null)
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
                LogHelper.WriteLog("GetUserByFlow发生异常：" + Msg);
                throw new Exception("GetUserByFlow:" + ex.ToString());
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
        /// <summary>
        /// 获取下一状态数据
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="Xoml"></param>
        /// <param name="Rules"></param>
        /// <param name="Layout"></param>
        /// <param name="xml"></param>
        /// <param name="UserID"></param>
        /// <param name="PostID"></param>
        /// <param name="FlowType"></param>
        /// <param name="DataResult"></param>
        /// <param name="user"></param>
        public void GetUserByFlow2(string companyID, string Xoml, string Rules, string Layout, string xml, string UserID, string PostID, FlowType FlowType, ref DataResult DataResult, ref FlowUser user)
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
                user.TrackingMessage += "创建工作流运行时 SMTWorkFlowManage.CreateWorkFlowRuntime(false)开始\r\n";
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                user.TrackingMessage += "创建工作流运行时SMTWorkFlowManage.CreateWorkFlowRuntime(false)完成\r\n";
                Instance = SMTWorkFlowManage.CreateWorkflowInstance(WfRuntime,Xoml, Rules);
                bool iscurruser = true;
                int testflag = 0;
                while (iscurruser)
                {
                    testflag++;
                    if (testflag > 10)
                    {
                        throw new Exception("循环超过10次");
                    }
                    #region 激发事件到一下状态
                    strCurrState = strNextState;
                    user.TrackingMessage += "激发事件到一下状态，并获取状态代码 SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml)开始" + Instance.InstanceId.ToString() + "\r\n";
                    strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml);
                    user.TrackingMessage += "激发事件到一下状态，并获取状态代码 SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, strNextState, xml)完成" + Instance.InstanceId.ToString() + "\r\n";
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
                            DataResult.Err = "流程:" + user.FlowName + " 没有找到对应角色";
                            DataResult.FlowResult = FlowResult.FAIL;
                            LogHelper.WriteLog("Formid=" + user.FormID + ";活动属性 Name=" + strNextState + ";没有在流程:" + user.FlowName + " Layout中找到对应角色,Layout如下:\r\r" + Layout);
                            return;
                        }
                        if (!IsCountersign)
                        {
                            LogHelper.WriteLog("Formid=" + user.FormID + ";(非会签) 根活动的字符串查找角色状态码(即活动Name属性)StateCode=" + strNextState + " Layout=" + Layout + "");
                            #region 非会签
                            bool isHigher = false;
                            //根据角色找人,如果角色有多个人,只找其中一个
                            AppUserInfo = GetUserByStateCode(listRole[0].RoleName, UserID, PostID, ref isHigher);
                            #region 打印审核人
                            string names = "\r\n=======FormID="+user.FormID+" 非会签 根据角色找人,如果角色有多个人,只找其中一个 打印审核人B(listRole[0].RoleName=" + listRole[0].RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                            foreach (var us in AppUserInfo)
                            {
                                names += "CompanyID:" + us.CompanyID + "\r\n";
                                names += "DepartmentID:" + us.DepartmentID + "\r\n";
                                names += "PostID:" + us.PostID + "\r\n";
                                names += "UserID:" + us.UserID + "\r\n";

                                names += "CompanyName:" + us.CompanyName + "\r\n";
                                names += "DepartmentName:" + us.DepartmentName + "\r\n";
                                names += "PostName:" + us.PostName + "\r\n";
                                names += "UserName:" + us.UserName + "\r\n";
                                names += "----------------------------------------------------\r\n";
                            }
                            if (!isHigher && listRole[0].IsOtherCompany != null)
                            {
                                if (listRole[0].IsOtherCompany.Value == true)
                                {
                                    names += "是否指定公司：" + listRole[0].IsOtherCompany.Value.ToString() + "\r\n";
                                    names += "公司的ID：" + listRole[0].OtherCompanyID + "\r\n";
                                    if (string.IsNullOrEmpty(listRole[0].OtherCompanyID))
                                    {
                                        names +="Layout=" + Layout + "\r\n";
                                    }
                                }
                                else if (listRole[0].IsOtherCompany.Value == false)
                                {
                                    names += "实际要查找公司的ID:" + companyID + " " + user.GetCompanyName(companyID) + "\r\n";
                                }
                            }
                            user.ErrorMsg += names;
                            LogHelper.WriteLog(names);
                            #endregion
                            #region beyond

                            if (!isHigher)
                            {
                                if (listRole[0].IsOtherCompany != null && listRole[0].IsOtherCompany.Value == true)
                                {//指定公司
                                    //过滤人
                                    AppUserInfo = AppUserInfo.Where(u => u.CompanyID == listRole[0].OtherCompanyID).ToList();
                                }
                                else
                                {
                                    AppUserInfo = AppUserInfo.Where(u => u.CompanyID == companyID).ToList();
                                }
                            }
                            #endregion
                            if (AppUserInfo == null || AppUserInfo.Count == 0)
                            {
                                DataResult.Err = user.GetCompanyName(companyID) + " " + listRole[0].Remark + " 没有找到审核人";
                                DataResult.FlowResult = FlowResult.FAIL;
                                return;
                            }


                            if (AppUserInfo.Where(c => c.UserID == UserID).Count() == 0)
                                iscurruser = false;
                            #endregion
                        }
                        else
                        {
                            LogHelper.WriteLog("Formid=" + user.FormID + ";(会签) 根活动的字符串查找角色状态码(即活动Name属性)StateCode=" + strNextState + " Layout=" + Layout + "");
                            #region 会签
                            DictCounterUser = new Dictionary<Role_UserType, List<UserInfo>>();
                            if (CountersignType == "0")
                            {
                                #region 全部审核通过才算通过
                                for (int i = 0; i < listRole.Count; i++)
                                {
                                    bool isHigher = false;


                                    var listuserinfo = GetUserByStateCode(listRole[i].RoleName, UserID, PostID, ref isHigher);
                                    #region 打印审核人
                                    string names = "\r\n=======FormID=" + user.FormID + "会签 全部审核通过才算通过  打印审核人C(listRole[0].RoleName=" + listRole[i].RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                                    if (listuserinfo != null)
                                    {
                                        foreach (var u in listuserinfo)
                                        {
                                            names += "CompanyID:" + u.CompanyID + "\r\n";
                                            names += "DepartmentID:" + u.DepartmentID + "\r\n";
                                            names += "PostID:" + u.PostID + "\r\n";
                                            names += "UserID:" + u.UserID + "\r\n";

                                            names += "CompanyName:" + u.CompanyName + "\r\n";
                                            names += "DepartmentName:" + u.DepartmentName + "\r\n";
                                            names += "PostName:" + u.PostName + "\r\n";
                                            names += "UserName:" + u.UserName + "\r\n";
                                            names += "----------------------------------------------------\r\n";
                                        }
                                    }
                                    if (!isHigher && listRole[i].IsOtherCompany != null)
                                    {
                                        if (listRole[i].IsOtherCompany.Value == true)
                                        {
                                            names += "是否指定公司：" + listRole[i].IsOtherCompany.Value.ToString() + "\r\n";
                                            names += "公司的ID：" + listRole[i].OtherCompanyID + "\r\n";
                                            if (string.IsNullOrEmpty(listRole[i].OtherCompanyID))
                                            {
                                                names += "Layout=" + Layout + "\r\n";
                                            }
                                        }
                                        else if (listRole[i].IsOtherCompany.Value == false)
                                        {
                                            names += "实际要查找公司的ID:" + companyID + "\r\n";
                                        }
                                    }
                                    user.ErrorMsg += names;
                                    LogHelper.WriteLog(names);
                                    #endregion
                                    if (!isHigher)
                                    {
                                        if (listRole[i].IsOtherCompany != null && listRole[i].IsOtherCompany.Value == true)
                                        {
                                            listuserinfo = listuserinfo.Where(u => u.CompanyID == listRole[i].OtherCompanyID).ToList();
                                        }
                                        else
                                        {
                                            listuserinfo = listuserinfo.Where(u => u.CompanyID == companyID).ToList();
                                        }
                                    }

                                    if (listuserinfo == null || listuserinfo.Count == 0)
                                    {
                                        DataResult.Err = user.GetCompanyName(companyID) + " " + listRole[i].Remark + " 没有找到审核人";
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
                                #region 只有一个审核通过了就算审核通过了
                                iscurruser = false;
                                bool bFlag = false;//判断是否找到审核人
                                string roles = "";//得到所有的角色
                                user.TrackingMessage += "GetUserByStateCode\r\n";
                                for (int i = 0; i < listRole.Count; i++)
                                {
                                    roles += listRole[i].Remark + "、";
                                    #region
                                    bool isHigher = false;

                                    var listuserinfo = GetUserByStateCode(listRole[i].RoleName, UserID, PostID, ref isHigher);
                                    #region 打印审核人
                                    string names = "\r\n=======FormID=" + user.FormID + " 会签 只有一个审核通过了就算审核通过了  打印审核人C(listRole[0].RoleName=" + listRole[i].RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                                    foreach (var u in listuserinfo)
                                    {
                                        names += "CompanyID:" + u.CompanyID + "\r\n";
                                        names += "DepartmentID:" + u.DepartmentID + "\r\n";
                                        names += "PostID:" + u.PostID + "\r\n";
                                        names += "UserID:" + u.UserID + "\r\n";

                                        names += "CompanyName:" + u.CompanyName + "\r\n";
                                        names += "DepartmentName:" + u.DepartmentName + "\r\n";
                                        names += "PostName:" + u.PostName + "\r\n";
                                        names += "UserName:" + u.UserName + "\r\n";
                                        names += "----------------------------------------------------\r\n";
                                    }
                                    if (!isHigher && listRole[i].IsOtherCompany != null)
                                    {
                                        if (listRole[i].IsOtherCompany.Value == true)
                                        {
                                            names += "是否指定公司：" + listRole[i].IsOtherCompany.Value.ToString() + "\r\n";
                                            names += "公司的ID：" + listRole[i].OtherCompanyID + "\r\n";
                                            if (string.IsNullOrEmpty(listRole[i].OtherCompanyID))
                                            {
                                                names += "Layout=" + Layout + "\r\n";
                                            }
                                        }
                                        else if (listRole[i].IsOtherCompany.Value == false)
                                        {
                                            names += "实际要查找公司的ID:" + companyID + "\r\n";
                                        }
                                    }
                                    user.ErrorMsg += names;
                                    LogHelper.WriteLog(names);
                                    #endregion
                                    if (!isHigher)
                                    {
                                        if (listRole[i].IsOtherCompany != null && listRole[i].IsOtherCompany.Value == true)
                                        {
                                            listuserinfo = listuserinfo.Where(u => u.CompanyID == listRole[i].OtherCompanyID).ToList();
                                        }
                                        else
                                        {
                                            listuserinfo = listuserinfo.Where(u => u.CompanyID == companyID).ToList();
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
                                    #endregion
                                }
                                if (!bFlag)
                                {
                                    DataResult.Err = user.GetCompanyName(companyID) + " " + roles + " 没有找到审核人";
                                    DataResult.FlowResult = FlowResult.FAIL;
                                    return;
                                }
                                user.TrackingMessage += " GetUserByStateCode完成\r\n";
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
                    #region 检查非会签角色是否有多个审核人
                    LogHelper.WriteLog("FormID=" + user.FormID + " 检查非会签角色的审核人数＝" + AppUserInfo.Count.ToString());
                    if (AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                    {
                        DataResult.FlowResult = FlowResult.MULTIUSER;
                    }

                    DataResult.UserInfo = AppUserInfo;
                    #endregion
                }
                else
                {
                    LogHelper.WriteLog("FormID=" + user.FormID + " 检查会签角色的审核人数＝" + DictCounterUser.Count.ToString());
                    #region 检查会签角色是否有多个审核人,如果有多个审核人,则返回                    
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
                user.TrackingMessage += " iscurruser完成\r\n";

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Formid=" + user.FormID + ";GetUserByFlow2异常信息 ：" + ex.ToString());
                throw new Exception("获取下一状态数据出错,请联系管理员! \r\n FormID=" + user.FormID + "");
            }
            finally
            {
                strCurrState = null;
                strNextState = null;
                //RuleName = null;
                AppUserInfo = null;
                Instance = null;
                user.TrackingMessage += "  SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime)\r\n";
                SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime);
                user.TrackingMessage += "  SMTWorkFlowManage.ColseWorkFlowRuntime(WfRuntime)完成\r\n";

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
            if (!WfRuntimeClone.IsStarted)
            {
                WfRuntimeClone.StartRuntime();
            }
            WorkflowRuntime WfRuntime = null;
            WorkflowInstance Instance = null;
            try
            {
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CloneWorkflowInstance(WfRuntimeClone, instanceClone, WfRuntime);

                string strNextState = SMTWorkFlowManage.GetNextStateByEvent(WfRuntime, Instance, CurrentStateName, xml);
                bool isHigher = false;
                List<UserInfo> AppUserInfo = GetUserByStateCode(strNextState, UserID, PostID, ref isHigher);
                #region 打印审核人
                string names = "\r\n=======打印审核人D(strNextState" + strNextState + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                foreach (var user in AppUserInfo)
                {
                    names += "CompanyID:" + user.CompanyID + "\r\n";
                    names += "DepartmentID:" + user.DepartmentID + "\r\n";
                    names += "PostID:" + user.PostID + "\r\n";
                    names += "UserID:" + user.UserID + "\r\n";

                    names += "CompanyName:" + user.CompanyName + "\r\n";
                    names += "DepartmentName:" + user.DepartmentName + "\r\n";
                    names += "PostName:" + user.PostName + "\r\n";
                    names += "UserName:" + user.UserName + "\r\n";
                    names += "----------------------------------------------------\r\n";
                }
                #endregion
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
                LogHelper.WriteLog("GetUserByInstance异常信息 ：" + ex.ToString());
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
                if (!WfRuntimeClone.IsStarted)
                {
                    WfRuntimeClone.StartRuntime();
                }
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
                    AppUserInfo = GetUserByStateCode(RuleName.RoleName, null, tmpPostID, ref isHigher);
                    #region 打印审核人
                    string names = "\r\n=======打印审核人E(RuleName.RoleName=" + RuleName.RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                    foreach (var user in AppUserInfo)
                    {
                        names += "CompanyID:" + user.CompanyID + "\r\n";
                        names += "DepartmentID:" + user.DepartmentID + "\r\n";
                        names += "PostID:" + user.PostID + "\r\n";
                        names += "UserID:" + user.UserID + "\r\n";
                      
                        names += "CompanyName:" + user.CompanyName + "\r\n";
                        names += "DepartmentName:" + user.DepartmentName + "\r\n";
                        names += "PostName:" + user.PostName + "\r\n";
                        names += "UserName:" + user.UserName + "\r\n";
                        names += "----------------------------------------------------\r\n";
                    }
                    if (!isHigher && RuleName.IsOtherCompany != null)
                    {
                        if (RuleName.IsOtherCompany.Value == true)
                        {
                            names += "是否指定公司：" + RuleName.IsOtherCompany.Value.ToString() + "\r\n";
                            names += "公司的ID：" + RuleName.OtherCompanyID + "\r\n";
                            if (string.IsNullOrEmpty(RuleName.OtherCompanyID))
                            {
                                names += "Layout=" + Layout + "\r\n";
                            }
                        }
                        else if (RuleName.IsOtherCompany.Value == false)
                        {
                            names += "实际要查找公司的ID:" + companyID + "\r\n";
                        }
                    }
                    
                    LogHelper.WriteLog(names);
                    #endregion
                    #region beyond
                    if (!isHigher && RuleName.IsOtherCompany != null)
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
                LogHelper.WriteLog("GetUserByInstance异常信息 ：" + ex.ToString());
                throw new Exception("GetUserByInstance:" + ex.Message);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="WfRuntimeClone"></param>
        /// <param name="instanceClone"></param>
        /// <param name="Layout">从审核主表记录ACTIVEROLE字段获取</param>
        /// <param name="xml"></param>
        /// <param name="CurrentStateName">当前状态</param>
        /// <param name="UserID"></param>
        /// <param name="PostID"></param>
        /// <param name="FlowType"></param>
        /// <param name="DataResult"></param>
        public void GetUserByInstance2(string companyID, WorkflowRuntime WfRuntimeClone, WorkflowInstance instanceClone, string Layout, string xml, string CurrentStateName, List<string> UserID, List<string> PostID, FlowType FlowType, ref DataResult DataResult,ref FlowUser fUser)
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
                if (!WfRuntimeClone.IsStarted)
                {
                    WfRuntimeClone.StartRuntime();
                }
                WfRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(false);
                Instance = SMTWorkFlowManage.CloneWorkflowInstance(WfRuntimeClone, instanceClone, WfRuntime);
                bool iscurruser = true;
                int testflag = 0;
                while (iscurruser)
                {
                    testflag++;
                    if (testflag > 10)
                    {
                        throw new Exception("循环处理流程超过10次，请联系系统管理员");
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
                        AppUserInfo = GetUserByStateCode(listRole[0].RoleName, null, tmpPostID, ref isHigher);
                        #region 打印审核人
                        string names = "\r\nFormID=" + fUser.FormID + ";=======打印审核人F(listRole[0].RoleName=" + listRole[0].RoleName + ";审核人数量=" + AppUserInfo.Count + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                        foreach (var user in AppUserInfo)
                        {
                            names += "CompanyID:" + user.CompanyID + "\r\n";
                            names += "DepartmentID:" + user.DepartmentID + "\r\n";
                            names += "PostID:" + user.PostID + "\r\n";
                            names += "UserID:" + user.UserID + "\r\n";

                            names += "CompanyName:" + user.CompanyName + "\r\n";
                            names += "DepartmentName:" + user.DepartmentName + "\r\n";
                            names += "PostName:" + user.PostName + "\r\n";
                            names += "UserName:" + user.UserName + "\r\n";
                            names += "----------------------------------------------------\r\n";
                        }
                        if (!isHigher && listRole[0].IsOtherCompany != null)
                        {
                            if (listRole[0].IsOtherCompany.Value == true)
                            {
                                names += "是否指定公司：" + listRole[0].IsOtherCompany.Value.ToString() + "\r\n";
                                names += "公司的ID：" + listRole[0].OtherCompanyID + "\r\n";
                                if (string.IsNullOrEmpty(listRole[0].OtherCompanyID))
                                {
                                    names += "Layout=" + Layout + "\r\n";
                                }
                            }
                            else if (listRole[0].IsOtherCompany.Value == false)
                            {
                                names += "实际要查找公司的ID:" + companyID + "\r\n";
                            }
                        }
                        fUser.ErrorMsg += names;
                        LogHelper.WriteLog(names);
                        #endregion
                        #region beyond

                        if (!isHigher && strNextState.ToUpper() != "ENDFLOW")
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
                            DataResult.Err = "当前角色 " + listRole[0].Remark + " 没有找到审核人" ;
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
                                var listuserinfo = GetUserByStateCode(listRole[i].RoleName, null, tmpPostID, ref isHigher);
                                #region 打印审核人
                                string names = "\r\n=======打印审核人G(listRole[i].RoleName=" + listRole[0].RoleName + ";UserID=" + UserID + ";PostID=" + PostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                                foreach (var user in listuserinfo)
                                {
                                    names += "CompanyID:" + user.CompanyID + "\r\n";
                                    names += "DepartmentID:" + user.DepartmentID + "\r\n";
                                    names += "PostID:" + user.PostID + "\r\n";
                                    names += "UserID:" + user.UserID + "\r\n";
                                   
                                    names += "CompanyName:" + user.CompanyName + "\r\n";
                                    names += "DepartmentName:" + user.DepartmentName + "\r\n";
                                    names += "PostName:" + user.PostName + "\r\n";
                                    names += "UserName:" + user.UserName + "\r\n";
                                    names += "----------------------------------------------------\r\n";
                                }
                                if (!isHigher && listRole[0].IsOtherCompany != null)
                                {
                                    if (listRole[0].IsOtherCompany.Value == true)
                                    {
                                        names += "是否指定公司：" + listRole[0].IsOtherCompany.Value.ToString() + "\r\n";
                                        names += "公司的ID：" + listRole[0].OtherCompanyID + "\r\n";
                                        if (string.IsNullOrEmpty(listRole[0].OtherCompanyID))
                                        {
                                            names += "Layout=" + Layout + "\r\n";
                                        }
                                    }
                                    else if (listRole[0].IsOtherCompany.Value == false)
                                    {
                                        names += "实际要查找公司的ID:" + companyID + "\r\n";
                                    }
                                }
                                fUser.ErrorMsg += names;
                                LogHelper.WriteLog(names);
                                #endregion
                                if (!isHigher)
                                {
                                    if (listRole[i].IsOtherCompany != null && listRole[i].IsOtherCompany.Value == true)
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
                                    DataResult.Err = "角色 " + listRole[i].Remark + "没有找到审核人";
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
                            string roleNames = "";//所有角色名称
                            iscurruser = false;
                            bool bFlag = false;
                            for (int i = 0; i < listRole.Count; i++)
                            {
                                roleNames += listRole[i].Remark + "、";
                                string tmpPostID = listRole[i].UserType == "CREATEUSER" ? PostID[0] : PostID[1];
                                bool isHigher = false;
                                var listuserinfo = GetUserByStateCode(listRole[i].RoleName, null, tmpPostID, ref isHigher);
                                #region 打印审核人
                                string names = "\r\n=======打印审核人H(listRole[0].RoleName=" + listRole[i].RoleName + ";UserID=" + UserID + ";PostID=" + tmpPostID + ";isHigher=" + isHigher.ToString() + ")=======\r\n";
                                foreach (var user in listuserinfo)
                                {
                                    names += "CompanyID:" + user.CompanyID + "\r\n";
                                    names += "DepartmentID:" + user.DepartmentID + "\r\n";
                                    names += "PostID:" + user.PostID + "\r\n";
                                    names += "UserID:" + user.UserID + "\r\n";
                                    

                                    names += "CompanyName:" + user.CompanyName + "\r\n";
                                    names += "DepartmentName:" + user.DepartmentName + "\r\n";
                                    names += "PostName:" + user.PostName + "\r\n";
                                    names += "UserName:" + user.UserName + "\r\n";
                                    names += "----------------------------------------------------\r\n";
                                }
                                if (!isHigher && listRole[0].IsOtherCompany != null)
                                {
                                    if (listRole[0].IsOtherCompany.Value == true)
                                    {
                                        names += "是否指定公司：" + listRole[0].IsOtherCompany.Value.ToString() + "\r\n";
                                        names += "公司的ID：" + listRole[0].OtherCompanyID + "\r\n";
                                        if (string.IsNullOrEmpty(listRole[0].OtherCompanyID))
                                        {
                                            names += "Layout=" + Layout + "\r\n";
                                        }
                                    }
                                    else if (listRole[0].IsOtherCompany.Value == false)
                                    {
                                        names += "实际要查找公司的ID:" + companyID + "\r\n";
                                    }
                                }
                                fUser.ErrorMsg += names;
                                LogHelper.WriteLog(names);
                                #endregion
                                if (!isHigher)
                                {
                                    if (listRole[i].IsOtherCompany != null && listRole[i].IsOtherCompany.Value == true)
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
                                DataResult.Err = "当前的角色 " + roleNames + " 没有找到审核人";
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
                    if (AppUserInfo != null && AppUserInfo.Count > 1) //处理角色对应多个用户,返回用户集给提交人，选择一个处理人
                    {
                        DataResult.FlowResult = FlowResult.MULTIUSER;
                    }
                    DataResult.UserInfo = AppUserInfo;
                    #endregion
                }
                else
                {
                    #region
                    if (DataResult.DictCounterUser == null)
                    {
                        DataResult.DictCounterUser = new Dictionary<Role_UserType, List<UserInfo>>();
                    }
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
                //throw new Exception("GetUserByInstance2:" + ex.Message);//旧的
                LogHelper.WriteLog("FORMID="+fUser.FormID+";通过实体例查找用户Instance=" + Instance.InstanceId.ToString()+" 异常信息:\r\n" + ex.ToString());
                throw new Exception(ex.Message);
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
        /// 通过状态代码查询下一处理人(对服务操作)
        /// </summary>
        /// <param name="StateCode">状态代码(角色ID(RoleName))</param>
        /// <returns></returns>
        private List<UserInfo> GetUserByStateCode(string StateCode, string UserID, string PostID, ref bool isHigher)
        {
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
                    #region 是否是直接上级,隔级上级,部门负责人
                    foreach (Higher c in Enum.GetValues(typeof(Higher)))
                    {
                        if (CurrentStateName.ToUpper() == c.ToString().ToUpper())
                        {
                            isDirect = (int)c;//== 1 ? true : false;

                            WFCurrentStateName = CurrentStateName;
                            isHigher = true;
                        }
                    }
                    #endregion

                    if (WFCurrentStateName != "")
                    {
                        //PermissionService
                        #region 如果是直接上级,隔级上级,部门负责人
                        LogHelper.WriteLog("UserID=" + UserID + " 开始调用 直接上级,隔级上级,部门负责人：WcfPersonnel.GetEmployeeLeaders(岗位ID=" + PostID + ", isDirect=" + isDirect + ")");
                        PersonnelServiceClient WcfPersonnel = new PersonnelServiceClient();//Commented by Alan 2012-7-25 ,只在使用时构造
                        V_EMPLOYEEVIEW[] User = WcfPersonnel.GetEmployeeLeaders(PostID, isDirect);
                        string strtemp = "UserID=" + UserID + " 结束调用 直接上级,隔级上级,部门负责人：WcfPersonnel.GetEmployeeLeaders(岗位ID=" + PostID + ", isDirect=" + isDirect + ")";
                        if (User != null && User.Length > 0)
                        {
                            for (int i = 0; i < User.Length; i++)
                            {
                                UserInfo tmp = new UserInfo();
                                tmp.UserID = User[i].EMPLOYEEID;
                                tmp.UserName = User[i].EMPLOYEECNAME;
                                tmp.CompanyID = User[i].OWNERCOMPANYID;
                                tmp.DepartmentID = User[i].OWNERDEPARTMENTID;
                                tmp.PostID = User[i].OWNERPOSTID;

                                tmp.CompanyName = User[i].COMPANYNAME;
                                tmp.DepartmentName = User[i].DEPARTMENTNAME;
                                tmp.PostName = User[i].POSTNAME;
                                tmp.Roles = new List<T_SYS_ROLE>();
                                //foreach (var role in User[i].Roles)
                                //{
                                //    tmp.Roles.Add(role);
                                //    strRole += "角色ID   = " + role.ROLEID + "\r\n";
                                //    strRole += "角色名称 = " + role.ROLENAME + "\r\n";
                                //}
                                listUser.Add(tmp);  
                                strtemp += "公司ID   = " + User[i].EMPLOYEEID + "\r\n";
                                strtemp += "部门ID   = " + User[i].OWNERDEPARTMENTID + "\r\n";
                                strtemp += "岗位ID   = " + User[i].OWNERPOSTID + "\r\n";
                                strtemp += "员工ID   = " + User[i].EMPLOYEEID + "\r\n";

                                strtemp += "公司名称 = " + User[i].COMPANYNAME + "\r\n";
                                strtemp += "部门名称 = " + User[i].DEPARTMENTNAME + "\r\n";
                                strtemp += "岗位名称 = " + User[i].POSTNAME + "\r\n";
                                strtemp += "员工姓名 = " + User[i].EMPLOYEECNAME + "\r\n";
                            }
                        }
                        #endregion
                        LogHelper.WriteLog(strtemp);
                    }
                    else
                    {
                        #region 根据角色ID查找人
                        LogHelper.WriteLog("UserID=" + UserID + " 开始调用 检索本状态（角色）对应用户：WcfPermissionService.GetFlowUserInfoByRoleID(角色ID=" + WFCurrentStateName + ")");
                        WFCurrentStateName = new Guid(CurrentStateName).ToString("D");

                        PermissionServiceClient WcfPermissionService = new PermissionServiceClient();

                        foreach (var op in WcfPermissionService.Endpoint.Contract.Operations)
                        {
                            var dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                                as DataContractSerializerOperationBehavior;
                            if (dataContractBehavior != null)
                            {
                                dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue; //int.MaxValue;
                            }
                        }
                        try
                        {
                            FlowUserInfo[] User = WcfPermissionService.GetFlowUserInfoByRoleID(WFCurrentStateName);//新的接口
                            string strRole = "UserID=" + UserID + " 结束调用 检索本状态（角色）对应用户：WcfPermissionService.GetFlowUserInfoByRoleID(角色ID=" + WFCurrentStateName + ")\r\n";

                            if (User != null && User.Length > 0)
                            {
                                for (int i = 0; i < User.Length; i++)
                                {
                                    #region
                                    UserInfo tmp = new UserInfo();
                                    strRole += "公司ID   = " + User[i].CompayID + "\r\n";
                                    strRole += "部门ID   = " + User[i].DepartmentID + "\r\n";
                                    strRole += "岗位ID   = " + User[i].PostID + "\r\n";
                                    strRole += "员工ID   = " + User[i].UserID + "\r\n";

                                    strRole += "公司名称 = " + User[i].CompayName + "\r\n";
                                    strRole += "部门名称 = " + User[i].DepartmentName + "\r\n";
                                    strRole += "岗位名称 = " + User[i].PostName + "\r\n";
                                    strRole += "员工姓名 = " + User[i].EmployeeName + "\r\n";

                                    tmp.UserID = User[i].UserID;
                                    tmp.UserName = User[i].EmployeeName;
                                    tmp.CompanyID = User[i].CompayID;
                                    tmp.DepartmentID = User[i].DepartmentID;
                                    tmp.PostID = User[i].PostID;

                                    tmp.CompanyName = User[i].CompayName;
                                    tmp.DepartmentName = User[i].DepartmentName;
                                    tmp.PostName = User[i].PostName;
                                    tmp.Roles = new List<T_SYS_ROLE>();
                                    foreach (var role in User[i].Roles)
                                    {
                                        tmp.Roles.Add(role);
                                        strRole += "角色ID   = " + role.ROLEID + "\r\n";
                                        strRole += "角色名称 = " + role.ROLENAME + "\r\n";
                                    }
                                    listUser.Add(tmp);
                                    strRole += "\r\n";
                                    strRole += "==================================================================================\r\n";
                                    #endregion

                                }
                            }
                            LogHelper.WriteLog(strRole);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("权限服务GetSysUserByRole异常信息 角色id：" + WFCurrentStateName + "" + ex.ToString());
                            throw new Exception("下一审核人为空，请联系公司权限管理员检查角色下的人员,角色id：" + WFCurrentStateName);
                        }
                        #endregion

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
                LogHelper.WriteLog("通过状态代码调用权限服务出错查询下一处理人(对服务操作) GetUserByStateCode异常信息 ：" + ex.ToString());
                throw new Exception("调用权限服务出错,请联系管理员!");
                // return null ;
            }
            //finally
            //{
            //    WcfPermissionService.Close();
            //    WcfPermissionService = null;
            //    WcfPersonnel.Close();
            //    WcfPersonnel = null;
            //}


        }

        #endregion

        #region 检查是否已提交流程
        /// <summary>
        /// 检查是否已提交流程
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
        public CheckResult CheckFlow(OracleConnection con, SubmitData ApprovalData, DataResult APPDataResult)
        {

            CheckResult CheckFlowResult = new CheckResult();
            try
            {

                CheckFlowResult.APPDataResult = APPDataResult;
                APPDataResult.RunTime += "---GetFlowInfoStart:" + DateTime.Now.ToString();
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(ApprovalData.FlowType);
                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo(con, ApprovalData.FormID, "", "", "0", ApprovalData.ModelCode, "", "", FlowTypeList);
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
        /// <summary>
        /// 检查是否已提交流程(对数据库操作)
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
        public CheckResult CheckFlow2(OracleConnection con, SubmitData ApprovalData, DataResult APPDataResult)
        {

            CheckResult CheckFlowResult = new CheckResult();
            try
            {

                CheckFlowResult.APPDataResult = APPDataResult;
                APPDataResult.RunTime += "---GetFlowInfoStart:" + DateTime.Now.ToString();
                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(ApprovalData.FlowType);
                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo(con, ApprovalData.FormID, "", "", "0", ApprovalData.ModelCode, "", "", FlowTypeList);//对数据库操作
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
        /// 新增流程(对数据库操作)
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>    
        public DataResult AddFlow2(OracleConnection con, SubmitData submitData, DataResult dataResult, ref FlowUser user)
        {
            // WorkflowRuntime workflowRuntime = null;
            WorkflowInstance instance = null;


            try
            {
                #region 获取定义的流程
                user.TrackingMessage += "获取定义的流程.GetFlowByModelName:submitData.ApprovalUser.DepartmentID=" + submitData.ApprovalUser.DepartmentID + ";OrgType='" + ((int)submitData.FlowType).ToString() + "'";
                List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(con, submitData.ApprovalUser.CompanyID, submitData.ApprovalUser.DepartmentID, submitData.ModelCode, ((int)submitData.FlowType).ToString(), ref user);//对数据库操作
                if (MODELFLOWRELATION == null || MODELFLOWRELATION.Count == 0)
                {
                    dataResult.FlowResult = FlowResult.FAIL;
                    dataResult.Err = "没有找到可使用的流程";
                    if (submitData.ApprovalUser.CompanyID == user.CompayID && submitData.ApprovalUser.DepartmentID == user.DepartmentID)
                    {
                        dataResult.Err = "没有找到公司[ " + user.CompayName + " ]下部门[ " + user.DepartmentName + " ]的匹配流程返回";
                    }
                    else
                    {
                        dataResult.Err = "没有找到公司[ " + user.CompayName + " ]的可使用匹配流程";

                    }
                    return dataResult;
                }

                #endregion
                FLOW_MODELFLOWRELATION_T flowRelation = MODELFLOWRELATION[0];//只取其中一条流程
                FLOW_FLOWDEFINE_T flowDefine = flowRelation.FLOW_FLOWDEFINE_T;
                user.FlowCode = flowDefine.FLOWCODE;//流程代码
                user.FlowName = flowDefine.DESCRIPTION;//流程名称
                if (flowDefine.RULES != null && flowDefine.RULES.Trim() == "")
                {
                    flowDefine.RULES = null;
                }
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime, flowDefine.XOML, flowDefine.RULES);
                LogHelper.WriteLog("新增 FormID=" + user.FormID + " 流程名称＝" + flowDefine.DESCRIPTION + "("+flowDefine.FLOWCODE+") 提交人＝" + user.UserName + " 公司名称＝" + user.CompayName + " 部门名称＝" + user.DepartmentName + " 岗位名称＝" + user.PostName + "  WorkflowInstance ID=" + instance.InstanceId.ToString());
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
                user.TrackingMessage += "FORMID=" + user.FormID + "获取下一状态数据(开始)";
                GetUserByFlow2(submitData.ApprovalUser.CompanyID, flowDefine.XOML, flowDefine.RULES, master.ACTIVEROLE, submitData.XML, submitData.ApprovalUser.UserID, submitData.ApprovalUser.PostID, submitData.FlowType, ref dataResult, ref user);
                LogHelper.WriteLog("FormID=" + user.FormID + " 获取下一状态数据! dataResult.FlowResult=" + dataResult.FlowResult.ToString());
                user.TrackingMessage += "FORMID=" + user.FormID + "获取下一状态数据(结束)";
                if (dataResult.FlowResult == FlowResult.FAIL)
                {
                    return dataResult;
                }
                submitData.NextStateCode = dataResult.AppState;
                if (dataResult.IsCountersign)
                {
                    #region 检查会签是角色是否有审核人员
                    #region 记录日志
                    if(submitData.DictCounterUser!=null)
                    {
                         LogHelper.WriteLog("FormID=" + user.FormID + " submitData.DictCounterUser=" + submitData.DictCounterUser.Count.ToString());
                    }
                    if (dataResult.DictCounterUser != null)
                    {
                        LogHelper.WriteLog("FormID=" + user.FormID + "  dataResult.DictCounterUser=" + dataResult.DictCounterUser.Count.ToString());
                    }
                    #endregion
                    
                    if (dataResult.FlowResult == FlowResult.Countersign)
                    {
                        if (submitData.DictCounterUser == null || submitData.DictCounterUser.Keys.Count == 0)
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + " submitData.DictCounterUser 会签角色里没有发现有审核人员,所以返回!");
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
                    #region 检查非会签角色里是否有审核人员
                    if (dataResult.FlowResult == FlowResult.MULTIUSER)
                    {
                        LogHelper.WriteLog("FormID=" + user.FormID + " 发现有多个审核人员!");
                        if (submitData.NextApprovalUser == null || (Utility.GetString(submitData.NextApprovalUser.UserID) == "" || Utility.GetString(submitData.NextApprovalUser.UserName) == ""))
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + " 发现有多个审核人员!但下一审核人为空，所以返回选择审核人！");
                            return dataResult;
                        }
                        else
                        {
                            LogHelper.WriteLog("FormID=" + user.FormID + " 发现有多个审核人员,但发现下一审核人不为空 usrid="+(Utility.GetString(submitData.NextApprovalUser.UserID)+" 姓名="+Utility.GetString(submitData.NextApprovalUser.UserName)));
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

                #region 实体赋值 当提交人为空时，创建人变成单据所属人，如果不为空，则创建人保存为系统登录人;创建公司，部门，岗位，仍然保存单据所属人的公司，部门，岗位
                FLOW_FLOWRECORDDETAIL_T entity = new FLOW_FLOWRECORDDETAIL_T();
                entity.FLOW_FLOWRECORDMASTER_T = master;
                entity.CREATECOMPANYID = submitData.ApprovalUser.CompanyID;
                entity.CREATEDEPARTMENTID = submitData.ApprovalUser.DepartmentID ;
                entity.CREATEPOSTID = submitData.ApprovalUser.PostID;
                entity.CREATEUSERID = string.IsNullOrEmpty(submitData.SumbitUserID) ? submitData.ApprovalUser.UserID : submitData.SumbitUserID;
                entity.CREATEUSERNAME = string.IsNullOrEmpty(submitData.SumbitUserName) ? submitData.ApprovalUser.UserName : submitData.SumbitUserName;
                #endregion
                user.TrackingMessage += " 处理kpi时间\r\n";
                #region 处理kpi时间
                string KPITime = "";
                #region 加入缓存
                string pscResult = CacheProvider.GetCache<string>(flowRelation.MODELFLOWRELATIONID);
                if (string.IsNullOrEmpty(pscResult))
                {
                    PerformanceServiceWS.PerformanceServiceClient psc = new PerformanceServiceWS.PerformanceServiceClient();
                    pscResult = psc.GetKPIPointsByBusinessCode(flowRelation.MODELFLOWRELATIONID);//调用服务
                    CacheProvider.Add<string>(flowRelation.MODELFLOWRELATIONID, pscResult);
                    psc.Close();
                }
                #endregion
                //PerformanceServiceWS.PerformanceServiceClient psc = new PerformanceServiceWS.PerformanceServiceClient();
                //string pscResult = psc.GetKPIPointsByBusinessCode(flowRelation.MODELFLOWRELATIONID);//调用服务
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
                master.KPITIMEXML = pscResult;
                #endregion
                user.TrackingMessage += " 处理kpi时间完成\r\n";
                FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                FlowData.xml = submitData.XML;

                if (!dataResult.IsCountersign)
                {
                    #region  确定非会签的下一个审核人
                    UserInfo AppUser = new UserInfo(); //下一审核人
                    AppUser = submitData.NextApprovalUser;
                    dataResult.UserInfo.Clear();
                    dataResult.UserInfo.Add(AppUser);
                    UserInfo AgentAppUser = GetAgentUserInfo(submitData.ModelCode, AppUser.UserID);//查询是否启用了代理人                    
                    dataResult = DoFlowRecord2(con, workflowRuntime, instance, entity, submitData.NextStateCode, AppUser, AgentAppUser, submitData.SubmitFlag, submitData.FlowType,ref user); //处理流程数据
                    dataResult.IsCountersign = false;
                    dataResult.AgentUserInfo = AgentAppUser;
                    #endregion
                }
                else
                {
                    user.TrackingMessage += " 会签\r\n";
                    #region  确定会签角色里的审核人员
                    //Tracer.Debug("-----DoFlowRecord_Add:" + DateTime.Now.ToString()+"\n");
                    dataResult.DictCounterUser = submitData.DictCounterUser;
                    Dictionary<UserInfo, UserInfo> dictAgentUserInfo = GetAgentUserInfo2(submitData.ModelCode, submitData.DictCounterUser);
                    dataResult = DoFlowRecord_Add(con, workflowRuntime, instance, entity, submitData.NextStateCode, submitData.DictCounterUser, dictAgentUserInfo, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                    //Tracer.Debug("-----DoFlowRecord_AddEnd:" + DateTime.Now.ToString()+"\n");
                    dataResult.IsCountersign = true;
                    dataResult.DictAgentUserInfo = dictAgentUserInfo;
                    #endregion
                    user.TrackingMessage += "会签完成\r\n";
                }
                user.TrackingMessage += "激发流程引擎执行到一下流程\r\n";
                #region 激发流程引擎执行到一下流程
                string ss = "";
                int n = 0;
                StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
                ManualWorkflowSchedulerService scheduleService = workflowRuntime.GetService(typeof(ManualWorkflowSchedulerService)) as ManualWorkflowSchedulerService;
               if (dataResult.AppState == null || dataResult.AppState == "")
                {
                    user.TrackingMessage += " workflowRuntime.GetService<FlowEvent>()\r\n";
                    scheduleService.RunWorkflow(workflowinstance.InstanceId);
                    workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                    scheduleService.RunWorkflow(workflowinstance.InstanceId);
                    user.TrackingMessage += " workflowRuntime.GetService<FlowEvent>()完成\r\n";
                }
                else
                {
                    scheduleService.RunWorkflow(workflowinstance.InstanceId);
                    workflowinstance.SetState(dataResult.AppState); //流程跳转到指定节点
                }

                #endregion
                user.TrackingMessage += "激发流程引擎执行到一下流程完成\r\n";
                user.TrackingMessage += "System.Threading.Thread.Sleep(1000)\r\n";
                //System.Threading.Thread.Sleep(1000);//当前用到
                dataResult.ModelFlowRelationID = flowRelation.MODELFLOWRELATIONID; //返回关联ID
                dataResult.KPITime = KPITime;
                //dataResult.CanSendMessage = true;
                if (submitData.FlowType == FlowType.Task)
                {
                    dataResult.SubModelCode = Utility.GetSubModelCode(master.ACTIVEROLE, dataResult.AppState); //返回下一子模块代码
                }
                user.TrackingMessage += "System.Threading.Thread.Sleep(1000)完成\r\n";
                return dataResult;
            }

            catch (Exception e)
            {
                user.ErrorMsg += "新增流程出错 FormID=" + user.FormID + " CompayName=" + user.CompayName + "FlowName=" + user.FlowName + "异常信息:\r\n" + e.ToString() + "\r\n";
                LogHelper.WriteLog("FormID=" + user.FormID + " CompayName=" + user.CompayName + "FlowName=" + user.FlowName + " 新增流程出错,异常信息:\r\n" + e.ToString());
                throw new Exception("FormID=" + user.FormID+" 时间："+DateTime.Now.ToString()+" 新增流程出错,请联系管理员! ");
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
            //WorkflowRuntime workflowRuntime = null;
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
            LogHelper.WriteLog("CancelFlow从持久化库在恢复创建工作流实例ID=" + entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID);
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
        /// <summary>
        /// 固定流程:下一审核人提交审核时调用方法
        /// </summary>
        /// <param name="submitData"></param>
        /// <param name="dataResult"></param>
        /// <param name="listDetail"></param>
        /// <returns></returns>
        public DataResult ApprovalFlow2(OracleConnection con, SubmitData submitData, DataResult dataResult, List<FLOW_FLOWRECORDDETAIL_T> listDetail, ref FlowUser user, ref string msg)
        {
            if (submitData.NextApprovalUser == null)
            {
                submitData.NextApprovalUser = new UserInfo();
            }
            ///针对会签，该次审核成功后是否跳转至下一状态
            bool isGotoNextState = true;
            // WorkflowRuntime workflowRuntime = null;
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
                    dataResult.Err = "没有找到待审核信息 FORMID=" + user.FormID + "\r\n";
                    user.TrackingMessage += "没有找到待审核信息 FORMID=" + user.FormID + "\r\n";
                   
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
                #region backup persisted workflow instanceState
                if (!string.IsNullOrEmpty(entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID))
                {
                    String connStringPersistence = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;//Data Source=172.30.50.110;Initial Catalog=WorkflowPersistence;Persist Security Info=True;User ID=sa;Password=fbaz2012;MultipleActiveResultSets=True";
                    string sql=string.Format("select * from instance_state where instance_id='{0}'", entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID);
                    user.InstanceState = MsOracle.GetDataSetByConnection(connStringPersistence, sql);
                }
                #endregion
                //workflowRuntime.StartRuntime();
                user.TrackingMessage += "创建工作流运行时开始 FORMID=" + user.FormID + "\r\n";
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                try
                {
                    instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime,entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID);// workflowRuntime.GetWorkflow(new Guid(tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID));
                    user.TrackingMessage += "FormID=" + submitData.FormID + ";ApprovalFlow2(try)从持久化库[ 完成 ]恢复创建工作流实例ID=" + entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID + "\r\n";
                    LogHelper.WriteLog("审核 FormID=" + user.FormID + " WorkflowInstance ID=" + instance.InstanceId.ToString());

                }
                catch (Exception exGetWorkflowInstance)
                {
                    #region 重新创建新流程，将新流程设置为当前状态。
                    try
                    {
                        user.TrackingMessage += "FormID=" + submitData.FormID + ";从持久化恢复工作流失败 SMTWorkFlowManage.GetWorkflowInstance(" + workflowRuntime.Name + ", " + entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID + ");原因如下:\r\n" + exGetWorkflowInstance.ToString() + ";\r\n下面重新创建新流程，并将新流程设置为当前状态;\r\nGetFlowByModelName:submitData.ApprovalUser.DepartmentID=" + submitData.ApprovalUser.DepartmentID + ";OrgType='" + ((int)submitData.FlowType).ToString() + "'";

                        List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(con, entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID, entity.FLOW_FLOWRECORDMASTER_T.CREATEDEPARTMENTID, submitData.ModelCode, ((int)submitData.FlowType).ToString(), ref  user);

                        FLOW_MODELFLOWRELATION_T flowRelation = MODELFLOWRELATION[0];
                        FLOW_FLOWDEFINE_T flowDefine = flowRelation.FLOW_FLOWDEFINE_T;
                        instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime , flowDefine.XOML, flowDefine.RULES);
                        user.TrackingMessage += "FormID=" + submitData.FormID + ";ApprovalFlow2(catch)完成重新创建工作流实例ID=" + instance.InstanceId + "\r\n";
                        StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
                        ManualWorkflowSchedulerService scheduleService = workflowRuntime.GetService(typeof(ManualWorkflowSchedulerService)) as ManualWorkflowSchedulerService;
                        scheduleService.RunWorkflow(workflowinstance.InstanceId);

                        workflowinstance.SetState(entity.STATECODE);

                        //System.Threading.Thread.Sleep(1000); //commented by alan 2012/9/7
                        instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime,instance.InstanceId.ToString());
                        user.TrackingMessage += "FormID=" + submitData.FormID + ";ApprovalFlow2(catch)从持久化库再恢复刚才创建工作流实例ID=" + instance.InstanceId + "\r\n";

                        entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID = instance.InstanceId.ToString();
                        //FLOW_FLOWRECORDDETAIL_TDAL.UpdateMasterINSTANCEID(entity.FLOW_FLOWRECORDMASTER_T);
                        FLOW_FLOWRECORDMASTER_TDAL.UpdateMasterINSTANCEID(con, entity.FLOW_FLOWRECORDMASTER_T);

                    }
                    catch (Exception exNewInstance)
                    {
                        user.ErrorMsg += "重新创建新流程，将新流程设置为当前状态失败:FormID=" + submitData.FormID + "异常信息：\r\n" + exNewInstance.Message + "\r\n";
                        LogHelper.WriteLog("重新创建新流程，将新流程设置为当前状态失败:FormID=" + submitData.FormID + "FlowBLL->ApprovalFlow2" + exNewInstance.Message);
                        //Tracer.Debug("exNewInstance: -" + submitData.FormID + "--submitDataXML:" + submitData.XML + "-" + exNewInstance.InnerException + exNewInstance.Message);
                        throw new Exception("重新创建新流程，将新流程设置为当前状态失败,请联系管理!");
                    }
                    #endregion
                }
                user.TrackingMessage += "SMTWorkFlowManage.CreateWorkFlowRuntime(true)完成FORMID=" + user.FormID + " \r\n";


                #region 当前状态会签状态处理
                bool currentIsCountersign = false;
                string currentCountersignType = "0";

                Utility.IsCountersign(entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, entity.STATECODE, ref currentIsCountersign, ref currentCountersignType);
                if (currentIsCountersign)
                {
                    user.TrackingMessage += "状态会签状态处理 FORMID=" + user.FormID + "  \r\n";
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
                    user.TrackingMessage += "状态会签状态处理完成 FORMID=" + user.FormID + " \r\n";
                }
                #endregion
                //不同意状态处理
                if (submitData.ApprovalResult == ApprovalResult.NoPass)
                {
                    user.TrackingMessage += "审核不通过状态处理(开始) FORMID=" + user.FormID + " \r\n";
                    #region
                    instance.Terminate("0");
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "3"; //设为终审不通过
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = submitData.ApprovalUser.UserID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = submitData.ApprovalUser.UserName;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;
                    user.TrackingMessage += "审核不通过【开始更新明细表】!FORMID=" + user.FormID;
                    //user.TrackingMessage += "entity.FLOWRECORDDETAILID" + entity.FLOWRECORDDETAILID + "\r\n";//
                    //user.TrackingMessage += "entity.STATECODE=" + entity.STATECODE + "\r\n";//
                    //user.TrackingMessage += "entity.PARENTSTATEID =" + entity.PARENTSTATEID + "\r\n";//
                    //user.TrackingMessage += "entity.CONTENT=" + entity.CONTENT + "\r\n";//
                    //user.TrackingMessage += "entity.CHECKSTATE=" + entity.CHECKSTATE + "\r\n";//同意：1，不同意:0 ,未处理:2，会签同意7，会签不同意8
                    //user.TrackingMessage += "entity.FLAG =" + entity.FLAG + "\r\n";//已审批：1，未审批:0
                    //user.TrackingMessage += " entity.CREATEUSERID =" + entity.CREATEUSERID + "\r\n";//
                    //user.TrackingMessage += "entity.CREATEUSERNAME=" + entity.CREATEUSERNAME + "\r\n";//
                    //user.TrackingMessage += " entity.CREATECOMPANYID=" + entity.CREATECOMPANYID + "\r\n";//
                    //user.TrackingMessage += " entity.CREATEDEPARTMENTID =" + entity.CREATEDEPARTMENTID + "\r\n";//
                    //user.TrackingMessage += "entity.CREATEPOSTID=" + entity.CREATEPOSTID + "\r\n";//
                    //user.TrackingMessage += "entity.CREATEDATE=" + entity.CREATEDATE + "\r\n";//
                    //user.TrackingMessage += " entity.EDITUSERID=" + entity.EDITUSERID + "\r\n";//
                    //user.TrackingMessage += "entity.EDITUSERNAME=" + entity.EDITUSERNAME + "\r\n";//
                    //user.TrackingMessage += " entity.EDITCOMPANYID =" + entity.EDITCOMPANYID + "\r\n";//
                    //user.TrackingMessage += " entity.EDITDEPARTMENTID=" + entity.EDITDEPARTMENTID + "\r\n";//
                    //user.TrackingMessage += "entity.EDITPOSTID=" + entity.EDITPOSTID + "\r\n";//
                    //user.TrackingMessage += "entity.EDITDATE=" + entity.EDITDATE + "\r\n";//
                    //user.TrackingMessage += "entity.AGENTUSERID =" + entity.AGENTUSERID + "\r\n";//
                    //user.TrackingMessage += " entity.AGENTERNAME=" + entity.AGENTERNAME + "\r\n";//
                    //user.TrackingMessage += "entity.AGENTEDITDATE=" + entity.AGENTEDITDATE + "\r\n";//
                    //user.TrackingMessage += "submitData.NextStateCode=" + submitData.NextStateCode + "\r\n";//
                    //user.TrackingMessage += "submitData.NextApprovalUser.UserID=" + submitData.NextApprovalUser.UserID + "\r\n";//
                    //user.TrackingMessage += " entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID=" + entity.FLOW_FLOWRECORDMASTER_T.FLOWRECORDMASTERID + "\r\n";//

                    
                    UpdateFlowRecord(con, entity, submitData.NextStateCode, submitData.NextApprovalUser.UserID);
                    user.TrackingMessage += "审核不通过【开始更新主表】!FORMID=" + user.FormID;
                    FLOW_FLOWRECORDMASTER_TDAL.Update(entity.FLOW_FLOWRECORDMASTER_T);
                    dataResult.CheckState = "3";//
                    dataResult.FlowResult = FlowResult.END;

                    if (currentIsCountersign)
                    {
                        #region 当前是会签状态，删除未审核记录
                        user.TrackingMessage += "审核不通过【当前是会签状态，删除未审核记录】!FORMID=" + user.FormID;
                        entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T
                                .Where(detail => detail.FLOWRECORDDETAILID != entity.FLOWRECORDDETAILID && detail.STATECODE == entity.STATECODE && detail.FLAG == "0")
                                .ToList().ForEach(item =>
                                {
                                    item.FLAG = "1";
                                    item.CHECKSTATE = "8";
                                    UpdateFlowRecord2(con, item);
                                });
                        #endregion
                    }

                    #endregion
                    user.TrackingMessage += "审核通过状态处理(完成) FORMID=" + user.FormID + " \r\n";

                }
                else
                {
                    if (!isGotoNextState)
                    {
                        user.TrackingMessage += "isGotoNextState开始 FORMID=" + user.FormID + " \r\n";
                        #region

                        UpdateFlowRecord2(con, entity);
                        dataResult.AppState = entity.STATECODE;
                        dataResult.FlowResult = FlowResult.SUCCESS;
                        dataResult.CheckState = "1";
                        #endregion
                        user.TrackingMessage += "isGotoNextState完成 FORMID=" + user.FormID + "\r\n";
                    }
                    else
                    {
                        user.TrackingMessage += "获取下一状态数据开始 FORMID=" + user.FormID + " \r\n";
                        #region 获取下一状态数据
                        List<string> User = new List<string>();
                        User.Add(entity.FLOW_FLOWRECORDMASTER_T.CREATEUSERID);
                        User.Add(submitData.ApprovalUser.UserID);

                        List<string> tmpPostID = new List<string>();
                        tmpPostID.Add(entity.FLOW_FLOWRECORDMASTER_T.CREATEPOSTID);
                        tmpPostID.Add(entity.EDITPOSTID);
                        GetUserByInstance2(entity.FLOW_FLOWRECORDMASTER_T.CREATECOMPANYID, workflowRuntime, instance, entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, submitData.XML, entity.STATECODE, User, tmpPostID, submitData.FlowType, ref dataResult, ref user);

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

                        user.TrackingMessage += "获取下一状态数据完成 FORMID=" + user.FormID + "\r\n";


                        user.TrackingMessage += "单据会签情况开始 FORMID=" + user.FormID + "\r\n";
                        #region 对于单会签情况，需要将其他审核人的审核设为会签通过状态

                        if (currentIsCountersign && currentCountersignType == "1")
                        {
                            entity.FLOW_FLOWRECORDMASTER_T.FLOW_FLOWRECORDDETAIL_T
                               .Where(detail => detail.FLOWRECORDDETAILID != entity.FLOWRECORDDETAILID && detail.STATECODE == entity.STATECODE && detail.FLAG == "0")
                               .ToList().ForEach(item =>
                               {
                                   item.FLAG = "1";
                                   item.CHECKSTATE = "7";
                                   UpdateFlowRecord2(con, item);
                               });
                        }
                        #endregion
                        user.TrackingMessage += "单据会签情况完成 FORMID=" + user.FormID + "\r\n";

                        #region


                        FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                        FlowData.xml = submitData.XML;
                        workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                        {
                            instance = null;

                        };
                        user.TrackingMessage += "处理kpi 开始 FORMID=" + user.FormID + "\r\n";

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

                        user.TrackingMessage += "处理kpi 完成 FORMID=" + user.FormID + "\r\n";

                        if (!dataResult.IsCountersign)
                        {
                            user.TrackingMessage += "非会签 开始 FORMID=" + user.FormID + "\r\n";
                            #region  非会签

                            UserInfo AppUser = submitData.NextApprovalUser;
                            dataResult.UserInfo.Clear();
                            dataResult.UserInfo.Add(AppUser);
                            UserInfo AgentAppUser = GetAgentUserInfo(submitData.ModelCode, AppUser.UserID);//查询是否启用了代理人                            
                            dataResult = DoFlowRecord2(con, workflowRuntime, instance, entity, submitData.NextStateCode, AppUser, AgentAppUser, submitData.SubmitFlag, submitData.FlowType,ref user); //处理流程数据
                            dataResult.AgentUserInfo = AgentAppUser;
                            dataResult.IsCountersign = false;
                            #endregion
                            user.TrackingMessage += "非会签 完成 FORMID=" + user.FormID + "\r\n";
                        }
                        else
                        {
                            #region  会签

                            dataResult.DictCounterUser = submitData.DictCounterUser;
                            Dictionary<UserInfo, UserInfo> dictAgentUserInfo = GetAgentUserInfo2(submitData.ModelCode, submitData.DictCounterUser);
                            dataResult = DoFlowRecord_Approval(con, workflowRuntime, instance, entity, submitData.NextStateCode, submitData.DictCounterUser, dictAgentUserInfo, submitData.SubmitFlag, submitData.FlowType); //处理流程数据
                            dataResult.DictAgentUserInfo = dictAgentUserInfo;
                            dataResult.IsCountersign = true;


                            #endregion
                        }

                        user.TrackingMessage += "激发流程引擎执行到一下流程 开始 FORMID=" + user.FormID + "\r\n";
                        #region 激发流程引擎执行到一下流程
                        StateMachineWorkflowInstance workflowinstance = new StateMachineWorkflowInstance(workflowRuntime, instance.InstanceId);
                        ManualWorkflowSchedulerService scheduleService = workflowRuntime.GetService(typeof(ManualWorkflowSchedulerService)) as ManualWorkflowSchedulerService;
                        if (dataResult.AppState == null || dataResult.AppState == "")
                        {
                            scheduleService.RunWorkflow(workflowinstance.InstanceId);
                            workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                            scheduleService.RunWorkflow(workflowinstance.InstanceId);

                        }
                        else
                        {
                            string ss = "";
                            int n = 0;
                            scheduleService.RunWorkflow(workflowinstance.InstanceId);

                            workflowinstance.SetState(dataResult.AppState); //流程跳转到指定节点
                            //while (true)
                            //{
                            //    ss += (n++).ToString()+"|" + workflowinstance.CurrentStateName;
                            //    string stateName = workflowinstance.CurrentStateName;

                            //    if (stateName != null && stateName.ToUpper().IndexOf("START") == -1)
                            //    {
                            //        break;
                            //    }
                            //}
                        }
                        #endregion
                        user.TrackingMessage += "激发流程引擎执行到一下流程 完成 FORMID=" + user.FormID + "\r\n";
                        //dataResult.CanSendMessage = true;

                        user.TrackingMessage += "System.Threading.Thread.Sleep(1000)\r\n";
                       //System.Threading.Thread.Sleep(1000); //Commented by Alan 2012-7-25 ,使用手动ScheduleService运行工作流,此处不需要
                        if (submitData.FlowType == FlowType.Task)
                            dataResult.SubModelCode = Utility.GetSubModelCode(entity.FLOW_FLOWRECORDMASTER_T.ACTIVEROLE, dataResult.AppState); //返回下一子模块代码
                        user.TrackingMessage += "System.Threading.Thread.Sleep(1000)完成\r\n";

                        #endregion
                    }


                }
                dataResult.CurrentIsCountersign = currentIsCountersign;
                dataResult.IsGotoNextState = isGotoNextState;
                return dataResult;
            }
            catch (Exception e)
            {
                user.ErrorMsg += "提交审核时出错!FORMID=" + user.FormID + " 异常信息:" + e.ToString()+ "\r\n";
                LogHelper.WriteLog("提交审核时出错!FORMID=" + user.FormID + " 异常信息:" + e.ToString());
                throw new Exception("提交审核时出错,请联系管理员! \r\n FormID=" + user.FormID + "");
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
        /// <summary>
        /// 自选流程审批（对服务操作）
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
        public DataResult AddFreeFlow(OracleConnection con, SubmitData ApprovalData, DataResult APPDataResult, ref FlowUser user)
        {
            // WorkflowRuntime workflowRuntime = null;
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
                entity.CREATEUSERID = ApprovalData.ApprovalUser.UserID;
                entity.CREATEUSERNAME = ApprovalData.ApprovalUser.UserName;
                entity.FLOW_FLOWRECORDMASTER_T.FLOWTYPE = ((int)ApprovalData.FlowType).ToString();
                entity.FLOW_FLOWRECORDMASTER_T.FLOWSELECTTYPE = ((int)ApprovalData.FlowSelectType).ToString();
                entity.FLOW_FLOWRECORDMASTER_T.FLOWCODE = "FreeFlow";
                workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime , "FreeFlow.xml");//自选流程使用
                user.TrackingMessage += "自选流程使用 AddFreeFlow(try)创建工作流实例完成 ID=" + instance.InstanceId;
                entity.FLOW_FLOWRECORDMASTER_T.INSTANCEID = instance.InstanceId.ToString();


                //下一审核人赋值
                AppUser = ApprovalData.NextApprovalUser;

                APPDataResult.RunTime += "---DoFlowStart:" + DateTime.Now.ToString();
                ApprovalData.NextStateCode = "Approval";
                AgentAppUser = GetAgentUserInfo(ApprovalData.ModelCode, AppUser.UserID); //查询是否启用了代理人（对服务操作）
                if (AgentAppUser != null)
                {
                    LogHelper.WriteLog("查询 启用了代理人 FormID=" + user.FormID+ " UserID=" + AgentAppUser.UserID);                    
                }
                else
                {
                    LogHelper.WriteLog("查询 没有启用了代理人 FormID=" + user.FormID + "  AgentAppUser=null");   
                }
                APPDataResult = DoFlowRecord(con, workflowRuntime, instance, entity, ApprovalData.NextStateCode, AppUser, AgentAppUser, ApprovalData.SubmitFlag, ApprovalData.FlowType,ref user); //处理流程数据
                APPDataResult.AgentUserInfo = AgentAppUser;
                APPDataResult.RunTime += "---DoFlowEnd:" + DateTime.Now.ToString();
                LogHelper.WriteLog("Formid=" + ApprovalData.FormID + ";自选流程工作流实例ID:" + instance.InstanceId.ToString());
                return APPDataResult;
            }
            catch (Exception e)
            {
                user.ErrorMsg += "自选流程审批出错 FormID=" + ApprovalData.FormID + ";异常信息:\r\n" + e.ToString()+"\r\n";
                LogHelper.WriteLog("自选流程审批出错 FormID=" + ApprovalData.FormID + ";跟踪信息:\r\n" + user.TrackingMessage + "异常信息:\r\n" + e.ToString());
                throw new Exception("自选流程审批出错,请联系管理员! \r\n FormID=" + user.FormID + "");
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


        /// <summary>
        /// 自选流程（对数据库操作、对服务操作）
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <param name="fd"></param>
        /// <returns></returns>
        public DataResult ApprovalFreeFlow(OracleConnection con, SubmitData ApprovalData, DataResult APPDataResult, List<FLOW_FLOWRECORDDETAIL_T> fd, ref FlowUser user)
        {
            // WorkflowRuntime workflowRuntime = null;
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
                    user.TrackingMessage += "没有找到待审核信息\r\n";
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

                try
                {
                    workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                    LogHelper.WriteLog("Formid=" + ApprovalData.FormID + ";开始 审核获取[自选流程]工作流实例ID=" + tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID);
                    instance = SMTWorkFlowManage.GetWorkflowInstance(workflowRuntime, tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID);// workflowRuntime.GetWorkflow(new Guid(tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID));
                    LogHelper.WriteLog("Formid=" + ApprovalData.FormID + ";完成 审核获取[自选流程]工作流实例ID=" + instance.InstanceId.ToString());
                }
                catch
                {
                    LogHelper.WriteLog("Formid=" + ApprovalData.FormID + ";完成 审核获取[自选流程]工作流实例 出错,需要重新构造工作流程实例,原来的实例ID=" + tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID);
                    workflowRuntime = SMTWorkFlowManage.CreateWorkFlowRuntime(true);
                    instance = SMTWorkFlowManage.CreateWorkflowInstance(workflowRuntime, "FreeFlow.xml");//自选流程使用 
                    tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID = instance.InstanceId.ToString();
                    LogHelper.WriteLog("Formid=" + ApprovalData.FormID + ";完成 重新构造[自选流程]工作流程实例,新的实例ID=" + tmp[0].FLOW_FLOWRECORDMASTER_T.INSTANCEID);
                }
                //不同意状态处理
                if (ApprovalData.ApprovalResult == ApprovalResult.NoPass)
                {
                    instance.Terminate("0");
                    user.TrackingMessage += "终审不通过,中止流程 FORMID=" + user.FormID;
                    entity.FLOW_FLOWRECORDMASTER_T.CHECKSTATE = "3"; //设为终审不通过
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERID = ApprovalData.ApprovalUser.UserID;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITUSERNAME = ApprovalData.ApprovalUser.UserName;
                    entity.FLOW_FLOWRECORDMASTER_T.EDITDATE = DateTime.Now;
                    UpdateFlowRecord(con, entity, ApprovalData.NextStateCode, ApprovalData.NextApprovalUser.UserID);
                    FLOW_FLOWRECORDMASTER_TDAL.Update(con, entity.FLOW_FLOWRECORDMASTER_T);//对数据库操作
                    APPDataResult.CheckState = "3";//
                    user.TrackingMessage += "终审不通过,设置状态 CheckState=3;FORMID=" + user.FormID;
                    APPDataResult.FlowResult = FlowResult.END;
                    // DataResult.UserInfo = null;
                    // return DataResult;

                }
                else
                {


                    //下一审核人赋值
                    if (ApprovalData.NextApprovalUser != null && !string.IsNullOrEmpty(ApprovalData.NextApprovalUser.UserID))
                    {
                        AppUser = ApprovalData.NextApprovalUser;
                    }
                    else
                    {
                        AppUser = ApprovalData.ApprovalUser;//如果没有下一审核人，下一审核人就是当前的审核人
                    }
                    user.TrackingMessage += "选择了下一个审核人 AppUser=" + AppUser.UserID + ";FORMID=" + user.FormID;
                    AgentAppUser = GetAgentUserInfo(ApprovalData.ModelCode, AppUser.UserID); //查询是否启用了代理人（对服务操作）
                    user.TrackingMessage += "查询是否启用了代理人 AppUser=" + AppUser.UserID + ";FORMID=" + user.FormID;
                    FlowDataType.FlowData FlowData = new FlowDataType.FlowData();
                    FlowData.xml = ApprovalData.XML;



                    workflowRuntime.WorkflowCompleted += delegate(object sender, WorkflowCompletedEventArgs e)
                    {//完成工作流实例

                        instance = null;

                    };

                    ApprovalData.NextStateCode = ApprovalData.NextStateCode == "EndFlow" ? "EndFlow" : "Approval";
                    APPDataResult.RunTime += "---DoFlowStart:" + DateTime.Now.ToString();

                    APPDataResult = DoFlowRecord(con, workflowRuntime, instance, entity, ApprovalData.NextStateCode, AppUser, AgentAppUser, ApprovalData.SubmitFlag, ApprovalData.FlowType,ref user); //处理流程数据
                    APPDataResult.AgentUserInfo = AgentAppUser;
                    APPDataResult.RunTime += "---DoFlowEnd:" + DateTime.Now.ToString();
                    if (ApprovalData.NextStateCode == "EndFlow")
                    {
                        ManualWorkflowSchedulerService scheduleService = workflowRuntime.GetService(typeof(ManualWorkflowSchedulerService)) as ManualWorkflowSchedulerService;
                        scheduleService.RunWorkflow(instance.InstanceId);

                        workflowRuntime.GetService<FlowEvent>().OnDoFlow(instance.InstanceId, FlowData); //激发流程引擎执行到一下流程
                        scheduleService.RunWorkflow(instance.InstanceId);
                        //System.Threading.Thread.Sleep(1000);
                    }



                }
                return APPDataResult;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("自选流程提交出错:FORMID=" + user.FormID + "\r\n 异常信息:" + e.ToString());
                throw new Exception("自选流程提交出错,请联系管理员! \r\n FormID=" + user.FormID + "");
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
        /// <summary>
        /// 查询是否使用代理(对服务操作)
        /// </summary>
        /// <param name="ModelCode"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public UserInfo GetAgentUserInfo(string ModelCode, string UserID)
        {
            UserInfo AgentUser = new UserInfo();
            try
            {
                OAWS.AgentServicesClient oa = new OAWS.AgentServicesClient();
                //OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent("TravelApplication1", "da844654-49c4-4138-ad83-e369cf03af5c");
                OAWS.T_HR_EMPLOYEE AGENTSET = oa.GetQueryAgent(UserID, ModelCode);
                if (AGENTSET == null)
                    return null;

                AgentUser.UserID = AGENTSET.EMPLOYEEID;//"userid0";
                AgentUser.UserName = AGENTSET.EMPLOYEECNAME;//"testuser";

                return AgentUser;
            }
            catch (Exception e)
            {
                
                LogHelper.WriteLog("ModelCode=" + ModelCode + ",UserID=" + UserID + " ;查询是否使用代理出错,异常信息:\r\n" + e.ToString());
                throw new Exception("查询是否使用代理出错,请联系管理员!");
            }
            finally
            {
                AgentUser = null;

            }

        }
        /// <summary>
        /// 查找会签角色
        /// </summary>
        /// <param name="ModelCode"></param>
        /// <param name="dictUserInfo"></param>
        /// <returns></returns>
        public Dictionary<UserInfo, UserInfo> GetAgentUserInfo2(string ModelCode, Dictionary<Role_UserType, List<UserInfo>> dictUserInfo)
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
                LogHelper.WriteLog("ModelCode=" + ModelCode + "查找会签角色出错,异常信息:\r\n" + e.ToString());
                throw new Exception("查找会签角色出错:请联系管理员!");
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

        public string GetFlowDefine(OracleConnection con, SubmitData ApprovalData)
        {
            try
            {
                FlowUser user = new FlowUser(ApprovalData.ApprovalUser.CompanyID, ApprovalData.ApprovalUser.UserID, con, ApprovalData.ModelCode);
                user.TrackingMessage += "构建引擎消息FlowBLL.GetFlowDefine.GetFlowByModelName(ApprovalData.ApprovalUser.DepartmentID=" + ApprovalData.ApprovalUser.DepartmentID + ";OrgType='" + ((int)ApprovalData.FlowType).ToString() + ")'";

                List<FLOW_MODELFLOWRELATION_T> MODELFLOWRELATION = GetFlowByModelName(con, ApprovalData.ApprovalUser.CompanyID, ApprovalData.ApprovalUser.DepartmentID, ApprovalData.ModelCode, ((int)ApprovalData.FlowType).ToString(), ref user);


                if (MODELFLOWRELATION == null || MODELFLOWRELATION.Count == 0)
                {

                    return null;
                }
                return MODELFLOWRELATION.First().FLOW_FLOWDEFINE_T.LAYOUT;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("构建引擎消息出错;异常信息\r\n" + e.ToString());
                throw e;
            }
        }

        public string IsExistFlowDataByUserID(OracleConnection con, string UserID, string PostID)
        {
            try
            {


                return FLOW_FLOWRECORDMASTER_TDAL.IsExistFlowDataByUserID(con, UserID, PostID);
            }
            catch (Exception e)
            {

                throw e;
            }
        }



        public List<FLOW_FLOWRECORDMASTER_T> GetFlowDataByUserID(OracleConnection con, string UserID)
        {
            try
            {

                List<FlowType> FlowTypeList = new List<FlowWFService.FlowType>();
                FlowTypeList.Add(FlowType.Approval);

                //FlowTypeList.Add(2);


                List<FLOW_FLOWRECORDDETAIL_T> fd = FlowBLL.GetFlowInfo(con, "", "", "", "0", "", "", UserID, FlowTypeList);
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
        /// <summary>
        /// 检查流程参数是否符合规则
        /// </summary>
        /// <param name="ApprovalData"></param>
        /// <param name="APPDataResult"></param>
        /// <returns></returns>
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

                if (ApprovalData.SubmitFlag == null)
                {
                    APPDataResult.Err = "流程提交标志(SubmitFlag)不能为空";
                    return false;
                }

                if (ApprovalData.FlowSelectType == null || (ApprovalData.FlowSelectType != FlowSelectType.FixedFlow && ApprovalData.FlowSelectType != FlowSelectType.FreeFlow))
                {
                    APPDataResult.Err = "流程审批类型设置有误，应设置成FlowSelectType.FixedFlow或FlowSelectType.FreeFlow";
                    return false;
                }

                if (ApprovalData.ApprovalUser == null)
                {
                    APPDataResult.Err = "提交用户信息不能为空";
                    return false;
                }
                else if (ApprovalData.ApprovalUser.CompanyID == null || ApprovalData.ApprovalUser.CompanyID == "")
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

        #region 2012/05/21 加入 亢
        Dictionary<string, string> lineList = new Dictionary<string, string>();

        /// <summary>
        /// 通过事项类型获取流程路径
        /// </summary>
        /// <param name="typeNumber">事项类型的值</param>
        /// <returns></returns>
        public string GetFlowPathByNumber(string typeNumber)
        {
            #region 找路径
            Dictionary<string, string> TypeName = new Dictionary<string, string>();
            TypeName.Add("41", "重要合同");
            TypeName.Add("461", "集团合同");
            TypeName.Add("35", "重要金额");
            //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；

            #endregion
            if (lineList.Count > 0)
            {
                if (lineList.ContainsKey(typeNumber))
                {
                    System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(lineList[typeNumber].ToString());
                    for (int i = 0; i < matchs.Count; i++)
                    {
                        string value = matchs[i].Groups[1].Value;
                    }
                           
                    return lineList[typeNumber].ToString();
                }
                else
                {
                    return "流程条件出错";
                }
            }
            else
            {
                return "";
            }
        }
        private List<string> GetFlowType(string layoutXml)
        {
            try
            {
                List<string> list = new List<string>();               
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(layoutXml);
                XElement xElement = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var lines = from item in xElement.Descendants("Rule")
                            where item.Attribute("StrStartActive").Value == "StartFlow"
                            select item;
                foreach (var line in lines)
                {
                    string conditions = "", conditionsValue = "", Operate = "";
                    if (line.Element("Conditions") != null)
                    {
                        conditions = "[" + line.Element("Conditions").Element("Condition").Attribute("Description").Value + "";
                        Operate = "" + line.Element("Conditions").Element("Condition").Attribute("Operate").Value + "";
                        conditionsValue = "" + line.Element("Conditions").Element("Condition").Attribute("CompareValue").Value + "]";
                    }
                    var Element = (from item in xElement.Descendants("Activity")
                                   where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                                   select item).FirstOrDefault();
                    string path = "开始--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                    list = GetActivityPath(xElement, Element.Attribute(XName.Get("Name")).Value, path, list);
                    if (list[0].ToString() == "开始-->流程设计格式不正确")
                    {
                        return list;
                    }
                }
                #region 获取事项类型
                foreach (var li in list)
                {
                    if (li.Contains("事项类型") || li.Contains("审批类型"))
                    {
                        try
                        {
                            //开始--[一级事项类型==127]-->分支机构财务负责人5---->分支机构负责人5---->商务部副经理---->营销总监---->营运中心办公室助理总监---->财务中心办公室总监---->副总经理5---->法务主管---->总经理5---->结束；
                            //开始---->直接上级---->部门负责人--[事项审批类型==35]-->财务经理-在线---->律师---->副总经理--[一级事项类型==42]-->人力资源部负责人---->副总裁（刘小强）---->总裁---->结束；"
                            //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；

                            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(li);
                            for (int i = 0; i < matchs.Count; ++i)
                            {
                                string charStr = "";
                                #region 比较字符
                                if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                                {
                                    charStr = ">";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                                {
                                    charStr = "<";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                                {
                                    charStr = "==";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                                {
                                    charStr = ">=";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                                {
                                    charStr = "<=";
                                }

                                #endregion
                                if (charStr != "")
                                {
                                    string name = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];
                                    if (lineList.ContainsKey(name))
                                    {
                                        lineList[name] = lineList[name] + "\r\n" + li;
                                    }
                                    else
                                    {
                                        lineList.Add(name, li);
                                    }
                                }
                            }
                        }
                        catch
                        { }
                    }

                }
                #endregion

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFlowBreach:" + ex.Message);
            }
        }
        public List<string> GetFlowBranch(string FlowID)
        {
           
            try
            {
                List<string> list = new List<string>();
                string Xml = FLOW_FLOWDEFINE_TDAL.GetFlowBranch(FlowID);
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(Xml);
                XElement xElement = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var lines = from item in xElement.Descendants("Rule")
                            where item.Attribute("StrStartActive").Value == "StartFlow"
                            select item;
                foreach (var line in lines)
                {
                    string conditions = "", conditionsValue = "", Operate = "";
                    if (line.Element("Conditions") != null)
                    {
                        conditions = "[" + line.Element("Conditions").Element("Condition").Attribute("Description").Value + "";
                        Operate = "" + line.Element("Conditions").Element("Condition").Attribute("Operate").Value + "";
                        conditionsValue = "" + line.Element("Conditions").Element("Condition").Attribute("CompareValue").Value + "]";
                    }
                    var Element = (from item in xElement.Descendants("Activity")
                                   where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                                   select item).FirstOrDefault();
                    string path = "开始--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                    list = GetActivityPath(xElement, Element.Attribute(XName.Get("Name")).Value, path, list);
                    if (list[0].ToString() == "开始-->流程设计格式不正确")
                    {
                        return list;
                    }
                }
                #region 获取事项类型
               
                foreach (var li in list)
                {
                    if (li.Contains("事项类型") || li.Contains("审批类型"))
                    {
                        try
                        {
                            //开始--[一级事项类型==127]-->分支机构财务负责人5---->分支机构负责人5---->商务部副经理---->营销总监---->营运中心办公室助理总监---->财务中心办公室总监---->副总经理5---->法务主管---->总经理5---->结束；
                            //开始---->直接上级---->部门负责人--[事项审批类型==35]-->财务经理-在线---->律师---->副总经理--[一级事项类型==42]-->人力资源部负责人---->副总裁（刘小强）---->总裁---->结束；"
                            //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；

                             System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            //System.Text.RegularExpressions.Match match = reg.Match(li);
                            //for (int i = 1; i < match.Groups.Count; ++i)
                            //{

                            //    string anme = match[i].Groups[i].Value;
                            //}

                            System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(li);
                            for (int i = 0; i < matchs.Count; i++)
                            {
                                string charStr = "";
                                #region 比较字符
                                if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                                {
                                    charStr = ">";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                                {
                                    charStr = "<";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                                {
                                    charStr = "==";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                                {
                                    charStr = ">=";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                                {
                                    charStr = "<=";
                                }
                                
                                #endregion
                                if (charStr != "")
                                {
                                    //matchs[i].Groups[1].Value  =  一级事项类型==39
                                    string name = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];                                    
                                    if (lineList.ContainsKey(name))
                                    {
                                        lineList[name] = lineList[name] + "\r\n" + li;
                                    }
                                    else
                                    {
                                        lineList.Add(name, li);
                                    }
                                }
                            }                            
                        }
                        catch
                        { }
                    }

                }
                #endregion
                #region 找路径
                Dictionary<string, string> TypeName = new Dictionary<string, string>();
                TypeName.Add("41", "重要合同41");
                TypeName.Add("39", "集团合同39");
                TypeName.Add("35", "普通合同35");
                TypeName.Add("42", "在线合同");
               Dictionary<string, string> newList = new Dictionary<string, string>();
                //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；
                foreach (KeyValuePair<string, string> line in lineList)
                {

                    System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(line.Value);
                    for (int i = 0; i < matchs.Count; i++)
                    {
                        string charStr = "";
                        #region 比较字符
                        if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                        {
                            charStr = ">";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                        {
                            charStr = "<";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                        {
                            charStr = "==";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                        {
                            charStr = ">=";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                        {
                            charStr = "<=";
                        }

                        #endregion
                        if (charStr != "")
                        {
                            //matchs[i].Groups[1].Value  =  一级事项类型==39
                            string name = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];
                            string typeName = name;
                            if (TypeName.ContainsKey(typeName))
                            {
                                typeName = TypeName[typeName];//事项类型名称
                            }
                            string typeAllName = matchs[i].Groups[1].Value.Replace(name, typeName);// 一级事项类型==集团合同
                            if (!newList.ContainsKey(line.Key))
                            {
                                newList.Add(line.Key, line.Value.Replace(matchs[i].Groups[1].Value, typeAllName));
                            }
                            else
                            {
                                newList[line.Key] = newList[line.Key].Replace(matchs[i].Groups[1].Value, typeAllName);
                            }
                        }
                        else
                        {
                            newList.Add(line.Key, line.Value);
                        }
                    }
                }
                #endregion
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("GetFlowBreach:" + ex.Message);
            }
        }

       
        private List<string> GetActivityPath(XElement xElement, string ActivityID, string path, List<string> list)
        {
            string snap = path;//foreach中循环的变量
            var lines = from item in xElement.Descendants("Rule")
                        where item.Attribute("StrStartActive").Value == ActivityID
                        select item;
            foreach (var line in lines)
            {
                string conditions = "", conditionsValue = "", Operate = "";
                if (line.Element("Conditions") != null)
                {
                    conditions = "[" + line.Element("Conditions").Element("Condition").Attribute("Description").Value + "";
                    Operate = "" + line.Element("Conditions").Element("Condition").Attribute("Operate").Value + "";
                    conditionsValue = "" + line.Element("Conditions").Element("Condition").Attribute("CompareValue").Value + "]";
                }
                var Element = (from item in xElement.Descendants("Activity")
                               where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                               select item).FirstOrDefault();
                if (line.Attribute("StrEndActive").Value == "EndFlow")
                {
                    if (list.Count() > 0 && list.Count() == 2 && list[0].ToString() == "开始-->流程设计格式不正确")
                    {
                        return list;
                    }
                    else
                    {                        
                        list.Add(snap + "---->结束；");                        
                    }
                    continue;
                }
                else
                {

                    if (snap.IndexOf("-->" + Element.Attribute(XName.Get("Remark")).Value + "--") < 1)
                    {
                        if (lines.Count() > 1)
                        {
                            path = snap + "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                        }
                        else
                        {

                            path += "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                        }
                        GetActivityPath(xElement, Element.Attribute(XName.Get("Name")).Value, path, list);
                    }
                    else
                    {
                        #region 获取事项类型
                        foreach (var li in list)
                        {
                            if (li.Split('>')[0].Contains("事项类型"))
                            {
                                try
                                {
                                    //开始--[一级事项类型==127]-->分支机构财务负责人5---->分支机构负责人5---->商务部副经理---->营销总监---->营运中心办公室助理总监---->财务中心办公室总监---->副总经理5---->法务主管---->总经理5---->结束；
                                    //开始---->直接上级---->部门负责人--[事项审批类型==35]-->财务经理-在线---->律师---->副总经理--[一级事项类型==42]-->人力资源部负责人---->副总裁（刘小强）---->总裁---->结束；"
                                    //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；
                                    string x = System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(li, "]-->")[0], "==")[1];
                                    
                                    // tring str = "我是[001]真心求救的[002]，你能帮帮我吗";
                                    //Pattern pattern = Pattern.compile("\\[(.*?)\\]");
                                    //Matcher matcher = pattern.matcher(str);
                                    //while(matcher.find()){
                                    //    System.out.println(matcher.group(1));
                                    //}

                                    System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                    //System.Text.RegularExpressions.Match match = reg.Match(li);
                                    //for (int i = 1; i < match.Groups.Count; ++i)
                                    //{

                                    //    string anme = match[i].Groups[i].Value;
                                    //}

                                    System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(li);
                                    for (int i = 0; i < matchs.Count; ++i)
                                    {
                                        string charStr = "";
                                        #region 比较字符
                                        if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                                        {
                                            charStr = "=="; 
                                        }
                                        if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                                        {
                                            charStr = ">=";
                                        }
                                        if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                                        {
                                            charStr = "<=";
                                        }
                                        if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                                        {
                                            charStr = ">";
                                        }
                                        if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                                        {
                                            charStr = "<";
                                        }
                                        #endregion
                                        if (charStr != "")
                                        {
                                            string name = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];
                                            if (lineList.ContainsKey(name))
                                            {
                                                lineList[name] = lineList[name] + "\r\n" + li;
                                            }
                                            else
                                            {
                                                lineList.Add(name, li);
                                            }
                                        }
                                    }                                   

                                    //string name = li.Replace("]-->", "|").Split('|')[0].Replace("==", "|").Split('|')[1];
                                    //if (lineList.ContainsKey(name))
                                    //{
                                    //    lineList[name] = lineList[name] + "\r\n" + li;
                                    //}
                                    //else
                                    //{
                                    //    lineList.Add(name, li);
                                    //}
                                }
                                catch
                                { }
                            }

                        }
                        #endregion
                        list.Clear();
                        list.Add("开始-->流程设计格式不正确");
                        list.Add("错误分支：" + snap + "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value + "");
                        return list;
                    }
                }
            }
            return list;
        }
        #endregion

        #region 对外接口获到我的单据实体
        public T_WF_PERSONALRECORD GetPersonalRecordByID(OracleConnection con,string id)
        {
           return T_WF_PERSONALRECORDDAL.GetPersonalRecordByPersonalrecordid(con,id);
        }
        #endregion
    }
}

