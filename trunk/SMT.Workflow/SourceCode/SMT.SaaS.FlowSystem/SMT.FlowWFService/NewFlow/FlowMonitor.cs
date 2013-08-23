/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2012     
	 * 文件名：FlowMonitor.cs  
	 * 创建者：LONGKC   
	 * 创建日期：2012/9/7 17:03:10   
	 * NET版本： 4.0.30319.225 
	 * 命名空间：SMT.Workflow.Monitoring.DAL 
	 * 模块名称：流程监控
	 * 描　　述：流程监控处理相关数据
	 * 修改人员：
	 * 修改日期：
	 * 修改内容：
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model;
using SMT.FLOWDAL.ADO;
using SMT.Workflow.Common.DataAccess;

namespace SMT.FlowWFService.NewFlow
{
    class FlowMonitor
    {
        #region 流程监控操作
        #region 将异常信息记录到流程监控表里
        /// <summary>
        /// 将异常信息记录到流程监控表里
        /// </summary>
        /// <param name="submitData">SubmitData</param>
        /// <param name="flowUser">FlowUser</param>
        public static void AddFlowMonitor(SubmitData submitData, FlowUser flowUser)
        {
            try
            {
                #region 提交信息
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("submitData.FlowSelectType =FlowSelectType." + submitData.FlowSelectType + ";");
                sb.AppendLine("submitData.FormID = \"" + submitData.FormID + "\";");
                sb.AppendLine("submitData.ModelCode = \"" + submitData.ModelCode + "\";");
                sb.AppendLine("submitData.ApprovalUser = new UserInfo();");
                sb.AppendLine("submitData.ApprovalUser.CompanyID = \"" + submitData.ApprovalUser.CompanyID + "\";");
                sb.AppendLine("submitData.ApprovalUser.DepartmentID = \"" + submitData.ApprovalUser.DepartmentID + "\";");
                sb.AppendLine("submitData.ApprovalUser.PostID = \"" + submitData.ApprovalUser.PostID + "\";");
                sb.AppendLine("submitData.ApprovalUser.UserID = \"" + submitData.ApprovalUser.UserID + "\";");
                sb.AppendLine("submitData.ApprovalUser.UserName = \"" + submitData.ApprovalUser.UserName + "\";");
                sb.AppendLine("submitData.NextStateCode = \"" + (submitData.NextStateCode != null ? submitData.NextStateCode : "空") + "\";");
                sb.AppendLine("submitData.NextApprovalUser = new UserInfo();");
                sb.AppendLine("submitData.NextApprovalUser.CompanyID = \"" + (submitData.NextApprovalUser != null ? submitData.NextApprovalUser.CompanyID : "空") + "\";");
                sb.AppendLine("submitData.NextApprovalUser.DepartmentID = \"" + (submitData.NextApprovalUser != null ? submitData.NextApprovalUser.DepartmentID : "空") + "\";");
                sb.AppendLine("submitData.NextApprovalUser.PostID = \"" + (submitData.NextApprovalUser != null ? submitData.NextApprovalUser.PostID : "空") + "\";");
                sb.AppendLine("submitData.NextApprovalUser.UserID = \"" + (submitData.NextApprovalUser != null ? submitData.NextApprovalUser.UserID : "空") + "\";");
                sb.AppendLine("submitData.NextApprovalUser.UserName = \"" + (submitData.NextApprovalUser != null ? submitData.NextApprovalUser.UserName : "空") + "\";");
                sb.AppendLine("submitData.SubmitFlag = SubmitFlag." + submitData.SubmitFlag + ";");
                // sb.AppendLine("submitData.XML = \"" + submitData.XML + "\";");
                sb.AppendLine("submitData.FlowType = FlowType." + submitData.FlowType.ToString() + ";");
                sb.AppendLine("submitData.ApprovalResult = ApprovalResult." + submitData.ApprovalResult.ToString() + ";");
                sb.AppendLine("submitData.ApprovalContent = \"" + submitData.ApprovalContent + "\";");

                #region 提交人信息
                sb.AppendLine("submitData.SumbitCompanyID = \"" + submitData.SumbitCompanyID + "\";");
                sb.AppendLine("submitData.SumbitDeparmentID = \"" + submitData.SumbitDeparmentID + "\";");
                sb.AppendLine("submitData.SumbitPostID = \"" + submitData.SumbitPostID + "\";");
                sb.AppendLine("submitData.SumbitUserID = \"" + submitData.SumbitUserID + "\";");
                sb.AppendLine("submitData.SumbitUserName = \"" + submitData.SumbitUserName + "\";");

                #endregion
                #endregion
                FLOW_EXCEPTIONLOG entity = new FLOW_EXCEPTIONLOG();

                entity.ID = Guid.NewGuid().ToString().Replace("-", "");//主键ID
                entity.FORMID = submitData.FormID;//业务ID
                entity.MODELCODE = submitData.ModelCode;//模块代码           
                entity.CREATEDATE = DateTime.Now;//创建日期
                entity.CREATENAME = submitData.SumbitUserID == null ? submitData.ApprovalUser.UserName : submitData.SumbitUserName;//创建人
                entity.SUBMITINFO = sb.ToString();//提交信息
                entity.LOGINFO = flowUser.ErrorMsg;//异常日志信息
                entity.MODELNAME = flowUser.ModelName;//模块名称
                entity.OWNERID = submitData.ApprovalUser.UserID;//单据所属人ID
                entity.OWNERNAME = submitData.ApprovalUser.UserName;//单据所属人姓名
                entity.OWNERCOMPANYID = submitData.ApprovalUser.CompanyID;//单据所属人公司ID
                entity.OWNERCOMPANYNAME =(submitData.SumbitUserID == submitData.ApprovalUser.UserID)?flowUser.CompayName: submitData.ApprovalUser.CompanyName;//单据所属人公司名称
                entity.OWNERDEPARMENTID = submitData.ApprovalUser.DepartmentID;//单据所属人部门ID
                entity.OWNERDEPARMENTNAME = (submitData.SumbitUserID == submitData.ApprovalUser.UserID) ? flowUser.DepartmentName : submitData.ApprovalUser.DepartmentName;//单据所属人部门名称
                entity.OWNERPOSTID = submitData.ApprovalUser.PostID;//单据所属人岗位ID
                entity.OWNERPOSTNAME = (submitData.SumbitUserID == submitData.ApprovalUser.UserID) ? flowUser.PostName : submitData.ApprovalUser.PostName;//单据所属人岗位名称
                entity.AUDITSTATE = submitData.ApprovalResult == ApprovalResult.Pass ? "审核通过" : "审核不通过";//审核状态;审核通过,审核不通过
                FLOW_EXCEPTIONLOGDAL dal = new FLOW_EXCEPTIONLOGDAL();
                dal.Add(dal.GetOracleConnection(), entity);
                LogHelper.WriteLog("Formid=" + submitData.FormID + ";将异常信息记录到流程监控表里,成功!");
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("Formid=" + submitData.FormID + ";将异常信息记录到流程监控表里,出错:异常信息:" + e.ToString());
               // throw  e;
            }

        }
        #endregion
        #region 将每一步的流程审核过程中的持久化实例保存下来
        /// <summary>
        /// 将每一步的流程审核过程中的持久化实例保存下来
        /// </summary>
        /// <param name="submitData">SubmitData</param>
        public static void AddInstance(SubmitData submitData,FlowUser fUser)
        {
            string instanceid = "";
            try
            {
                //１查找工作流程实例ID;2查询工作流持久化对像；３保存持久化 
                FLOW_INSTANCE_STATEDAL dal = new FLOW_INSTANCE_STATEDAL();
                FLOW_FLOWRECORDMASTER_T master = dal.GetFlowerMasterIDByFormid(dal.GetOracleConnection(), submitData.FormID);
                if (master != null && !string.IsNullOrEmpty(master.INSTANCEID.Trim()))
                {
                    FLOW_INSTANCE_STATE entity = new FLOW_INSTANCE_STATE();
                    FLOW_INSTANCE_STATE model = dal.GetInstanceModel(dal.GetOracleConnection("OracleConnection"), master.INSTANCEID);
                    if (!string.IsNullOrEmpty(model.INSTANCE_ID))
                    {
                        instanceid = model.INSTANCE_ID;
                        entity.INSTANCE_ID = model.INSTANCE_ID;//
                        entity.STATE = model.STATE;//
                        entity.STATUS = model.STATUS;//
                        entity.UNLOCKED = model.UNLOCKED;//
                        entity.BLOCKED = model.BLOCKED;//
                        entity.INFO = model.INFO;//
                        entity.MODIFIED = model.MODIFIED;//
                        entity.OWNER_ID = model.OWNER_ID;//
                        entity.OWNED_UNTIL = model.OWNED_UNTIL;//
                        entity.NEXT_TIMER = model.NEXT_TIMER;//                
                        entity.CREATEID = submitData.ApprovalUser.UserID;//创建人ID
                        entity.CREATENAME = submitData.ApprovalUser.UserName;//创建人姓名               
                        entity.EDITID = fUser.NextEditUserID;//下一个审核人ID
                        entity.EDITNAME = fUser.NextEditUserName;//下一个审核人姓名
                        entity.FORMID = master.FORMID;
                        FLOW_INSTANCE_STATEDAL inDal = new FLOW_INSTANCE_STATEDAL();
                        inDal.Add(inDal.GetOracleConnection(), entity);
                        LogHelper.WriteLog("Formid=" + submitData.FormID + "; instanceid=" + instanceid + " 将每一步的流程审核过程中的持久化实例保存下来,成功!");
                    }
                    else
                    {
                        LogHelper.WriteLog("Formid=" + submitData.FormID + "; 没法找到持久化数据库的instanceid,可能丢失 instanceid=" + master.INSTANCEID + " "); 
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("Formid=" + submitData.FormID + "; instanceid=" + instanceid + " 将每一步的流程审核过程中的持久化实例保存下来出错:异常信息:" + e.ToString());
                //throw new Exception(e.Message, e);
            }
 
        }
        #endregion
        #endregion
    }
}
