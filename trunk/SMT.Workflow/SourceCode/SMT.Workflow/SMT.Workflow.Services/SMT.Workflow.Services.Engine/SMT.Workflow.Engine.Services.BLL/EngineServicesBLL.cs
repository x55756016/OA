/*---------------------------------------------------------------------  
	 * 版　权：Copyright ?   2011    
	 * 文件名：EngineServicesBll.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/19 9:39:38   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.BLL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using SMT.Workflow.Engine.Services.DAL;
using System.Xml.Linq;
using System.IO;
using System.Net.Mail;
using SMT.Workflow.Engine.BLL;
using System.Data;
using SMT.Global.IEngineContract;
using EngineDataModel;
using SMT.SaaS.BLLCommonServices;
using System.Xml;
using SMT.Workflow.Common.DataAccess;
using SMT.Workflow.SMTCache;
using System.Text.RegularExpressions;

namespace SMT.Workflow.Engine.Services.BLL
{
    public class EngineServicesBLL
    {

        /// <summary>
        /// 业务系统直接新增消息
        /// </summary>
        /// <param name="strFlowMessage"></param>
        /// <param name="strBOObject"></param>
        /// <returns></returns>
        public bool SaveFlowTriggerData(string strFlow, string strBOObject)
        {

            T_WF_DOTASK entity = new T_WF_DOTASK();
            string strEntityType = string.Empty;//EntityType (表名)
            string strEntityKey = string.Empty;//EntityKey (主键)
            string IsTask = "1";//是否任务
            try
            {
                try
                {
                    #region 解析XML
                    StringReader strRdr = new StringReader(strFlow);
                    XmlReader xr = XmlReader.Create(strRdr);
                    StringBuilder _ConbinString = new StringBuilder();
                    while (xr.Read())
                    {
                        if (xr.NodeType == XmlNodeType.Element)
                        {
                            string elementName = xr.Name;
                            if (elementName == "Message")
                            {
                                while (xr.Read())
                                {
                                    string type = xr.NodeType.ToString();
                                    #region

                                    if (xr["Name"] != null)
                                    {
                                        _ConbinString.Append(xr["Name"] + "|" + xr["DataValue"] + "Ё");
                                        if (xr["Name"].ToUpper() == "COMPANYID")
                                        {
                                            entity.COMPANYID = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "MODELCODE")
                                        {
                                            entity.MODELCODE = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "MODELNAME")
                                        {
                                            entity.MODELNAME = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "FORMID")
                                        {
                                            entity.ORDERID = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "CHECKSTATE")
                                        {
                                            entity.ORDERSTATUS = int.Parse(xr["DataValue"]);
                                        }
                                        if (xr["Name"].ToUpper() == "APPUSERID")
                                        {
                                            entity.RECEIVEUSERID = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "ISTASK")
                                        {
                                            IsTask = xr["DataValue"];
                                        }
                                        if (xr["Name"].ToUpper() == "OUTTIME")
                                        {

                                            if (!string.IsNullOrEmpty(xr["DataValue"]))
                                            {
                                                entity.BEFOREPROCESSDATE = DateTime.Now.AddMinutes(int.Parse(xr["DataValue"]));
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                        entity.FLOWXML = strFlow;
                        entity.APPXML = strBOObject;
                    }
                    #endregion
                    #region strBusiness
                    if (!string.IsNullOrEmpty(strBOObject))
                    {
                        StringReader rd = new StringReader(strBOObject);
                        XmlReader xdr = XmlReader.Create(rd);
                        StringBuilder BOObject = new StringBuilder();
                        while (xdr.Read())
                        {
                            if (xdr.NodeType == XmlNodeType.Element)
                            {
                                string elementName = xdr.Name;
                                if (elementName == "Name")
                                {
                                    while (xdr.Read())
                                    {
                                        entity.SYSTEMCODE = xdr.Value;
                                        break;
                                    }
                                }
                                if (elementName == "Object")
                                {
                                    try
                                    { //非手机的XML时没有表明和主键的
                                        strEntityType = xdr["Name"];
                                        strEntityKey = xdr["Key"];
                                    }
                                    catch
                                    {
                                        LogHelper.WriteLog("异常无法取得业务数据表，单据:" + entity.ORDERID + "：表：" + strEntityType + ":主键：" + strEntityKey + "");
                                    }
                                    while (xdr.Read())
                                    {
                                        if (xdr.Name == "Attribute")
                                        {
                                            if (xdr["Name"] != null)
                                            {
                                                BOObject.Append(xdr["Name"] + "|" + xdr["DataValue"] + "Г" + (xdr["DataText"] != null ? xdr["DataText"] : xdr["DataValue"]) + "Ё");
                                                #region
                                                if (xdr["Name"].ToUpper() == "OWNERID")
                                                {
                                                    entity.ORDERUSERID = xdr["DataValue"];
                                                }
                                                if (xdr["Name"].ToUpper() == "OWNERNAME")
                                                {
                                                    entity.ORDERUSERNAME = xdr["DataValue"];
                                                }
                                                if (xdr["Name"].ToUpper() == "CREATEUSERID")
                                                {
                                                    //有些特殊的模块需要改变接收人
                                                    if (Common.CheckModelName(entity.MODELCODE) && entity.ORDERSTATUS == 2)
                                                    {
                                                        entity.RECEIVEUSERID = xdr["DataValue"];
                                                    }
                                                }

                                                #endregion
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        entity.APPFIELDVALUE = _ConbinString.ToString() + BOObject.ToString().TrimEnd('Ё');

                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    return false;
                }
                bool IsExecute = EngineExecute(entity, IsTask);
                if (IsExecute)
                {
                    try
                    {
                        /****************更新业务系统单据审核状态***********************/
                        if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
                        {
                            if (!string.IsNullOrEmpty(entity.SYSTEMCODE) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                            {
                                UpdateAuditStatus(entity.SYSTEMCODE, strEntityType, strEntityKey, entity.ORDERID, entity.ORDERSTATUS.ToString());
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return IsExecute;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private bool EngineExecute(T_WF_DOTASK Entity, string IsTask)
        {
            bool result = false;
            EngineServicesDAL dal = new EngineServicesDAL();
            string strAppFieldValue = string.Empty;
            DataTable sourceTable = Common.FieldStringToDataTable(Entity.APPFIELDVALUE, ref strAppFieldValue);//将业务数据与流程数据转换成DataTable作为替换的源数据        
            //通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容

            DataTable dtFlowTrigger = dal.FlowTriggerTable(Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString(), Entity.COMPANYID);
            if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
            {
                #region
                DataRow[] drs = dtFlowTrigger.Select("ISDEFAULTMSG=1");//发的是默认流程触发条件
                if (drs.Count() > 0)
                {
                    if (IsTask == "1")//新增待办任务
                    {
                        TaskCache.TaskCacheReflesh(Entity.RECEIVEUSERID);
                        dal.AddDoTask(Entity, drs[0], sourceTable, strAppFieldValue);//新增待办任务
                    }
                    else if (IsTask == "0")//消息
                    {
                        dal.AddDoTaskMessage(Entity, drs[0], sourceTable);
                    }
                }
                DataRow[] NotDefaultMsg = dtFlowTrigger.Select("ISDEFAULTMSG=0");//非默认消息，需要调用的WCF待办任务
                if (NotDefaultMsg != null && NotDefaultMsg.Count() > 0)
                {
                    foreach (DataRow dr1 in NotDefaultMsg)
                    {
                        string strAppMsg = string.Empty;
                        CallWCFService(dr1, sourceTable, ref strAppMsg);//调用WCF服务
                        if (!string.IsNullOrEmpty(strAppMsg))
                        {
                            try
                            {
                                string IsNewFlow = "1";
                                string NewFormID = string.Empty;
                                string strFormTypes = string.Empty;//表单状态
                                DataRow DRNewTrigger = null;
                                if (ApplicationValueToDataTable(strAppMsg, string.Concat(Entity.COMPANYID), ref sourceTable, ref IsNewFlow, ref NewFormID, ref strFormTypes, ref DRNewTrigger))
                                {
                                    //通过岗位查找用户，并且取第一个用户为发送消息的对像
                                    PersonnelWS.PersonnelServiceClient HRClient = new PersonnelWS.PersonnelServiceClient();
                                    if (!string.IsNullOrEmpty(dr1["OWNERPOSTID"].ToString()))
                                    {
                                        string[] Employees = HRClient.GetEmployeeIDsByPostID(dr1["OWNERPOSTID"].ToString());
                                        if (Employees != null && Employees.Count() > 0)
                                        {
                                            TaskCache.TaskCacheReflesh(NewFormID);
                                            dal.AddDoTask(Entity, dr1, sourceTable, Employees[0], NewFormID, strAppFieldValue, string.Concat(dr1["MESSAGEBODY"]), strFormTypes);//发送消息
                                        }
                                    }
                                    else
                                    {
                                        result = false;
                                        LogHelper.WriteLog("命名空间:SMT.Workflow.Engine.Services.BLL类方法：EngineExecute()引擎调用新流程时没有选定岗位");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                LogHelper.WriteLog("命名空间:SMT.Workflow.Engine.Services.BLL类方法：EngineExecute()" + ex.Message);
                            }
                        }
                    }
                }
                result = true;
                #endregion
            }
            else
            {

                string strMsg = "系统代号" + Entity.SYSTEMCODE + "\r\n" +
                                 "模块代号：" + Entity.MODELCODE + "\r\n" +
                                 "触发条件：" + Entity.ORDERSTATUS +
                                 "公司ID：" + Entity.COMPANYID + "\r\n" +
                                 "单据ID：" + Entity.ORDERID;
                LogHelper.WriteLog("该单据所对应的引擎系统设置未找到,请先配置该模块引擎消息(审核通过、审核不通过、审核中)" + strMsg);
            }
            return result;
        }
        /// <summary>
        /// 调用WCF
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="sourceTable"></param>
        /// <param name="strAppMsg"></param>
        private void CallWCFService(DataRow dr, DataTable sourceTable, ref string strAppMsg)
        {
            string ErroMessage = "";
            try
            {
                string Binding = dr["WCFBINDINGCONTRACT"].ToString();
                string WCFUrl = dr["WCFURL"].ToString();
                WCFUrl = Config.GetSystemCode(dr["SYSTEMCODE"].ToString()) + WCFUrl;
                string FuncName = dr["FUNCTIONNAME"].ToString();
                string FuncPameter = dr["FUNCTIONPARAMTER"].ToString();
                string SplitChar = dr["PAMETERSPLITCHAR"].ToString();
                string strDeptID = dr["OWNERDEPARTMENTID"].ToString();
                string strCorpID = dr["OWNERCOMPANYID"].ToString();
                string strPostID = dr["OWNERPOSTID"].ToString();
                string strOwnerID = dr["RECEIVEUSER"].ToString();
                FuncPameter = Common.ReplaceValue(FuncPameter, sourceTable);
                string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                    "<Paras>" + "\r\n" +
                                    "<Para TableName=\"\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"" + strOwnerID + "\"></Para>" +
                                    "<Para TableName=\"\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"" + strDeptID + "\"></Para>" +
                                    "<Para TableName=\"\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"" + strPostID + "\"></Para>" +
                                    "<Para TableName=\"\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"" + strCorpID + "\"></Para>" +
                                    "{0}" +
                                    "</Paras>";
                FuncPameter = string.Format(XmlTemplete, FuncPameter);

                if (!string.IsNullOrEmpty(Binding) && !string.IsNullOrEmpty(WCFUrl) && !string.IsNullOrEmpty(FuncName))
                {
                    //GlobalFunction.CustomizeSplitChar = CustomsizeSplitChar;

                    object TmpreceiveString = SMT.Workflow.Engine.BLL.Utility.CallWCFService(Binding, WCFUrl, FuncName, FuncPameter, ref ErroMessage);
                    strAppMsg = TmpreceiveString == null ? "" : TmpreceiveString.ToString();
                    string cMessage = "执行链接：" + WCFUrl + "\r\n" +
                                      "执行方法:" + FuncName + "\r\n" +
                                      "绑定契约：" + Binding + "\r\n" +
                                      "执行结果：" + strAppMsg + "\r\n" +
                                      "执行参数：" + FuncPameter + "\r\n" +
                                      "----------------------------------------------------------";
                    LogHelper.WriteLog("引擎调用WCF记录" + cMessage);

                }
                else
                {
                    ErroMessage += "命名空间:SMT.Workflow.Engine.Services.BLL类方法：CallWCFService() 调用WCF参数不全WCF协议：" + Binding + "WCF地址：" + WCFUrl + "WCF方法：" + FuncName + "";
                    LogHelper.WriteLog("命名空间:SMT.Workflow.Engine.Services.BLL类方法：CallWCFService() 调用WCF参数不全WCF协议：" + Binding + "WCF地址：" + WCFUrl + "WCF方法：" + FuncName + "");
                }
            }
            catch (Exception e)
            {
                ErroMessage += e.ToString();
                LogHelper.WriteLog(ErroMessage);
                throw new Exception("命名空间:SMT.Workflow.Engine.Services.BLL类方法：CallWCFService()" + e.Message);
            }

        }
        /// <summary>
        /// 流程撤销
        /// </summary>
        /// <param name="strFlowXML"></param>
        /// <param name="strAppXml"></param>
        /// <returns></returns>
        public bool FlowCancel(string strFlowXML, string strAppXml)
        {
            if (string.IsNullOrEmpty(strFlowXML) && string.IsNullOrEmpty(strAppXml))
            {
                return false;
            }
            string strCompanyID = string.Empty;//公司ID 
            string strSystemCode = string.Empty;//系统代号
            string strModelCode = string.Empty;//模块代号
            string strFormID = string.Empty;//FORMID
            string strModelName = string.Empty;//模块名称
            string strEntityType = string.Empty;//EntityType (表名)
            string strEntityKey = string.Empty;//EntityKey (主键)
            string strCheckState = string.Empty;//审核状态
            string strReceiveID = string.Empty;//接收人

            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                /*解析流程和业务数据XML*/
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strFlowXML);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                strCompanyID = Common.XMLToAttribute(xele, "COMPANYID");
                strSystemCode = string.Empty;
                strModelCode = Common.XMLToAttribute(xele, "MODELCODE");
                strFormID = Common.XMLToAttribute(xele, "FORMID");
                strModelName = Common.XMLToAttribute(xele, "ModelName");
                strCheckState = Common.XMLToAttribute(xele, "CheckState");
                strReceiveID = Common.XMLToAttribute(xele, "APPUSERID");
                if (!string.IsNullOrEmpty(strAppXml))
                {
                    Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(strAppXml);
                    XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
                    strSystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
                    try
                    {
                        strEntityType = (from item in xemeBoObject.Descendants("Object") select item).FirstOrDefault().Attribute("Name").Value;
                        strEntityKey = (from item in xemeBoObject.Descendants("Object") select item).FirstOrDefault().Attribute("Key").Value;
                        //有些特殊的模块需要改变接收人
                        if (Common.CheckModelName(strModelCode) && strCheckState == "2")
                        {
                            strReceiveID = (from item in xemeBoObject.Descendants("Object").Descendants("Attribute")
                                            where item.Attribute("Name").Value.ToUpper() == "CREATEUSERID"
                                            select item).FirstOrDefault().Attribute("DataValue").Value;
                        }
                    }
                    catch
                    {
                        LogHelper.WriteLog("FlowCancel该单号:" + strFormID + " 中业务数据无法取得EntityKey");
                    }
                }

                string strAppFieldValue = Common.ConbinString(xele) + Common.BOObjectEscapeString(strAppXml);
                DataTable dtValue = Common.FieldStringToDataTable(strAppFieldValue, ref strAppFieldValue);
                string content = dal.GetMessageDefineString("FLOWCANCEL");
                string strUrl = string.Empty;
                Common.ReplaceUrl(ref content, ref strUrl, dtValue);
                dal.DoTaskCancel(strSystemCode, strModelCode, strFormID, strReceiveID, content);

                if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
                {
                    if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                    {
                        UpdateAuditStatus(strSystemCode, strEntityType, strEntityKey, strFormID, strCheckState);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                string cMessage = "流程待办任务撤销异常" + "\r\n" +
                                  "SystemCode:" + strSystemCode + "\r\n" +
                                  "MODELCODE:" + strModelCode + "\r\n" +
                                  "COMPANYID:" + strCompanyID + "\r\n" +
                                   "Error:" + e.Message + "\r\n" +
                                    "FORMID:" + strFormID + "\r\n" +
                                   "流程XML:" + strFlowXML + "\r\n" +
                                  "AppXml:" + strAppXml + "\r\n";
                LogHelper.WriteLog("FlowCancel流程待办任务撤销异常" + cMessage);
                return false;
            }
        }
        public bool UpdateAuditStatus(string strSystemCode, string EntityType, string EntityKey, string EntityId, string strCheckState)
        {
            bool bol = true;
            string strMsg = "系统代号：" + strSystemCode + "\r\n"
                               + "EntityType:" + EntityType + "\r\n"
                               + "EntityId:" + EntityId + "\r\n"
                               + "EntityKey:" + EntityKey + "\r\n"
                               + "CheckState:" + strCheckState + "\r\n";
            try
            {
                // 1：审核中；2审核通过；3：审核不通过 
                CheckStates CheckState = CheckStates.Approving;
                switch (strCheckState)
                {
                    case "1":
                        CheckState = CheckStates.Approving;
                        break;
                    case "2":
                        CheckState = CheckStates.Approved;
                        break;
                    case "3":
                        CheckState = CheckStates.UnApproved;
                        break;
                    case "4":
                        CheckState = CheckStates.Cancel;
                        break;
                }

                #region 配合新预算使用
                string serviceErrorMsg = "";//业务系统的异常信息
                bool bResult = SMT.SaaS.BLLCommonServices.Utility.UpdateFormCheckState(strSystemCode, EntityType, EntityKey, EntityId, CheckState, ref serviceErrorMsg,"");
                if (!bResult)
                {
                    LogHelper.WriteLog("更新审核状态失败方法:UpdateFormCheckState:" + strMsg);
                    return bol;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("更新审核状态出现异常" + strMsg + "UpdateAuditStatus" + ex);
                return false;
            }
        }
        /// <summary>
        //待办任务分页查询
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref int rowCount, ref int pageCount)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                return dal.MsgListByPaging(msgParams.PageIndex, msgParams.PageSize, msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.BeginDate, msgParams.EndDate, ref rowCount, ref pageCount);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 手机版待办任务分页查询
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndexForMobile(MsgParms msgParams, ref int rowCount, ref int pageCount)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                return dal.MsgListByPagingForMobile(msgParams.PageIndex, msgParams.PageSize, msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.BeginDate, msgParams.EndDate, ref rowCount, ref pageCount);
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, T_FLOW_ENGINEMSGLIST> GetPendingTaskPrevNext(MsgParms msgParams)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            return dal.GetPendingTaskPrevNext(msgParams.MessageId, msgParams.Status, msgParams.UserID);
        }

        /// <summary>
        /// 平台待办分页接口
        /// </summary>
        /// <param name="msgParams"></param>
        /// <param name="IsAutofresh"></param>
        /// <param name="HaveNewTask"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingTasksParmsPageIndex(MsgParms msgParams, ref bool IsAutofresh, ref bool HaveNewTask, ref int rowCount, ref int pageCount)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                HaveNewTask = TaskCache.CurrentUserTaskStatus(msgParams.UserID, true);
                //LogHelper.WriteLog("获取缓存状态Page：" + HaveNewTask + "||索引页：" + msgParams.PageIndex + "||IsAutofresh:" + IsAutofresh);
                if (!IsAutofresh)//主动切换
                {
                    List<T_FLOW_ENGINEMSGLIST> List = dal.MsgListByPaging(msgParams.PageIndex, msgParams.PageSize, msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.BeginDate, msgParams.EndDate, ref rowCount, ref pageCount);
                    //LogHelper.WriteLog("手动获取缓存状态1：" + HaveNewTask + msgParams.UserID + " RowCount:" + List.Count);
                    return List;
                }
                else//如果是自动刷新
                {
                    List<T_FLOW_ENGINEMSGLIST> List = new List<T_FLOW_ENGINEMSGLIST>();
                    if (HaveNewTask)//判断是否有待办任务变更
                    {
                        List = dal.MsgListByPaging(msgParams.PageIndex, msgParams.PageSize, msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.BeginDate, msgParams.EndDate, ref rowCount, ref pageCount);
                        //LogHelper.WriteLog("自动获取缓存状态2：" + HaveNewTask + msgParams.UserID + " RowCount:" + List.Count);
                    }
                    return List;
                }

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 待办任务数据（主节）
        /// </summary>
        /// <param name="msgParams"></param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {

                List<T_FLOW_ENGINEMSGLIST> List = dal.GetEngineMainMsgList(msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.Top, msgParams.LastDay).ToList();
                return List;
            }
            catch
            {
                return null;

            }
        }


        /// <summary>
        /// 待办任务主节（门户自动刷新缓存处理）
        /// </summary>
        /// <param name="msgParams">参数</param>
        /// <param name="IsAutofresh">是否自动刷新</param>
        /// <returns></returns>
        public List<T_FLOW_ENGINEMSGLIST> PendingMainTasksParms(MsgParms msgParams, ref  bool IsAutofresh, ref bool HaveNewTask)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                HaveNewTask = TaskCache.CurrentUserTaskStatus(msgParams.UserID, false);
                //LogHelper.WriteLog("获取缓存状态：" + HaveNewTask + "索引页：" + msgParams.PageIndex);
                if (!IsAutofresh)//主动切换
                {
                    List<T_FLOW_ENGINEMSGLIST> List = dal.GetEngineMainMsgList(msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.Top, msgParams.LastDay).ToList();
                    //LogHelper.WriteLog("手动获取缓存状态3：" + HaveNewTask + msgParams.UserID + " RowCount:" + List.Count);

                    return List;
                }
                else//如果是自动刷新
                {
                    List<T_FLOW_ENGINEMSGLIST> List = new List<T_FLOW_ENGINEMSGLIST>();
                    if (HaveNewTask)//判断是否有待办任务变更
                    {
                        List = dal.GetEngineMainMsgList(msgParams.UserID, msgParams.Status, msgParams.MessageBody, msgParams.Top, msgParams.LastDay).ToList();
                        //LogHelper.WriteLog("自动获取缓存状态4：" + HaveNewTask + msgParams.UserID + " RowCount:" + List.Count);

                    }
                    return List;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 待办任务数据（不带业务数据字段）
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>
        public T_FLOW_ENGINEMSGLIST PendingDetailTasks(string daskID)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                return dal.GetEngineMsgDetail(daskID, true);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// PendingDetailTasksByOrderNodeCodeForMobile
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public T_FLOW_ENGINEMSGLIST PendingDetailTasksByPhone(string orderCode)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                if (orderCode.IndexOf('_') > 0)
                {
                    orderCode = orderCode.Substring(orderCode.IndexOf('_') + 1);
                }
                return dal.PendingDetailTasksByPhone(orderCode);
            }
            catch (Exception ex)
            {

                return null;
            }
        }



        /// <summary>
        /// 待办任务数据（带业务数据字段）
        /// </summary>
        /// <param name="strMsgID"></param>
        /// <returns></returns>
        public T_FLOW_ENGINEMSGLIST PendingDetailByID(string daskID)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {
                return dal.GetEngineMsgDetail(daskID, false);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public List<T_FLOW_ENGINENOTES> EngineNotes(string strUserID, string strStatus, int iTop)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            try
            {

                return dal.GetMsgNodes(strUserID, strStatus, iTop).ToList();
            }
            catch
            {
                return null;
            }
        }
        private string EventXmlAttribute(XElement xele, string strDescendants)
        {
            try
            {
                return xele.Descendants(strDescendants).First().Value.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
        #region 定时事件触发方法
        /// <summary>
        /// 事件定时
        /// </summary>
        /// <param name="strEventXml">定时所需的XML模版</param>
        /// <returns></returns>
        public bool SaveEventTriggerData(string strEventXml)
        {
            if (!string.IsNullOrEmpty(strEventXml))
            {

                try
                {
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strEventXml);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    T_WF_TIMINGTRIGGERACTIVITY trigger = new T_WF_TIMINGTRIGGERACTIVITY();
                    trigger.COMPANYID = EventXmlAttribute(xele, "CompanyCode");
                    trigger.SYSTEMCODE = EventXmlAttribute(xele, "SystemCode");
                    trigger.MODELCODE = EventXmlAttribute(xele, "ModelCode");
                    trigger.TRIGGERTIME = Convert.ToDateTime(EventXmlAttribute(xele, "TaskStartDate"));
                    trigger.BUSINESSID = EventXmlAttribute(xele, "ApplicationOrderCode");
                    string ProcessCycle = EventXmlAttribute(xele, "ProcessCycle");
                    // 0 只触发一次 1 分钟 2小时 3 天 4 月 5年 6周 7未知
                    switch (ProcessCycle.ToUpper())
                    {
                        case "YEAR":
                            trigger.TRIGGERROUND = 5;
                            break;
                        case "MONTH":
                            trigger.TRIGGERROUND = 4;
                            break;
                        case "DAY":
                            trigger.TRIGGERROUND = 3;
                            break;
                        case "HOUR":
                            trigger.TRIGGERROUND = 2;
                            break;
                        case "MINUTE":
                            trigger.TRIGGERROUND = 1;
                            break;
                        case "WEEK":
                            trigger.TRIGGERROUND = 6;
                            break;
                        case " ":
                            trigger.TRIGGERROUND = 0;
                            break;
                        case "":
                            trigger.TRIGGERROUND = 0;
                            break;
                        default:
                            trigger.TRIGGERROUND = 100;
                            trigger.REMARK = "未知的触发周期：" + ProcessCycle;
                            break;
                    }
                    trigger.RECEIVERUSERID = EventXmlAttribute(xele, "ReceiveUser");
                    trigger.RECEIVEROLE = EventXmlAttribute(xele, "ReceiveRole");
                    trigger.MESSAGEBODY = EventXmlAttribute(xele, "MessageBody");
                    trigger.WCFURL = EventXmlAttribute(xele, "ProcessWcfUrl");
                    trigger.FUNCTIONNAME = EventXmlAttribute(xele, "WcfFuncName");
                    string TriggerType = EventXmlAttribute(xele, "TriggerType");
                    if (string.IsNullOrEmpty(TriggerType))//
                    {
                        TriggerType = "User";
                    }
                    trigger.TRIGGERTYPE = TriggerType;
                    string WcfFuncParamter = string.Empty;
                    string MsgLinkUrl = string.Empty;
                    try
                    {
                        WcfFuncParamter = xele.Element("WcfFuncParamter").ToString().Replace("<WcfFuncParamter>", "").Replace("</WcfFuncParamter>", "");
                        trigger.FUNCTIONPARAMTER = WcfFuncParamter;

                    }
                    catch { }
                    try
                    {
                        MsgLinkUrl = xele.Element("MsgLinkUrl").ToString().Replace("<MsgLinkUrl>", "").Replace("</MsgLinkUrl>", "");
                        trigger.MESSAGEURL = MsgLinkUrl;
                    }
                    catch { }
                    trigger.PAMETERSPLITCHAR = EventXmlAttribute(xele, "WcfParamSplitChar");
                    trigger.WCFBINDINGCONTRACT = EventXmlAttribute(xele, "WcfBinding");
                    trigger.TRIGGERDESCRIPTION = "EventTrigger";
                    if (trigger.TRIGGERSTART == null)
                    {
                        trigger.TRIGGERSTART = DateTime.Now;
                    }
                    if (trigger.TRIGGEREND == null)
                    {
                        trigger.TRIGGEREND = DateTime.Now.AddYears(60);
                    }
                    if (trigger.TRIGGERMULTIPLE <= 0)
                    {
                        trigger.TRIGGERMULTIPLE = 1;
                    }
                    EngineServicesDAL dal = new EngineServicesDAL();
                    return dal.AddTimingTrigger(trigger);
                }


                catch (Exception e)
                {
                    string cMessage = "<消息引擎>Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]";
                    LogHelper.WriteLog(cMessage);
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        #endregion
        #region 定时事件触发方法（IsOneWay）
        /// <summary>
        /// 定时事件触发方法（IsOneWay）
        /// </summary>
        /// <returns></returns>
        public void SaveEventData(string strEventXml)
        {
            if (!string.IsNullOrEmpty(strEventXml))
            {
                try
                {

                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strEventXml);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    T_WF_TIMINGTRIGGERACTIVITY trigger = new T_WF_TIMINGTRIGGERACTIVITY();
                    trigger.COMPANYID = EventXmlAttribute(xele, "CompanyCode");
                    trigger.SYSTEMCODE = EventXmlAttribute(xele, "SystemCode");
                    trigger.MODELCODE = EventXmlAttribute(xele, "ModelCode");
                    trigger.TRIGGERTIME = Convert.ToDateTime(EventXmlAttribute(xele, "TaskStartDate"));
                    trigger.BUSINESSID = EventXmlAttribute(xele, "ApplicationOrderCode");
                    string ProcessCycle = EventXmlAttribute(xele, "ProcessCycle");
                    if (ProcessCycle.ToUpper() == "MONTH")
                    {
                        trigger.TRIGGERROUND = 4;
                    }
                    else if (ProcessCycle.ToUpper() == "HOUR")//
                    {
                        trigger.TRIGGERROUND = 2;
                    }
                    else if (ProcessCycle.ToUpper() == "DAY")
                    {
                        trigger.TRIGGERROUND = 3;
                    }
                    else if (ProcessCycle.ToUpper() == "WEEK")
                    {
                        trigger.TRIGGERROUND = 6;
                    }
                    else if (ProcessCycle == "" || ProcessCycle == " ")
                    {
                        trigger.TRIGGERROUND = 0;
                    }
                    else
                    {
                        trigger.TRIGGERROUND = 100;
                        trigger.REMARK = "未知的触发周期：" + ProcessCycle;
                    }
                    trigger.RECEIVERUSERID = EventXmlAttribute(xele, "ReceiveUser");
                    trigger.RECEIVEROLE = EventXmlAttribute(xele, "ReceiveRole");
                    trigger.MESSAGEBODY = EventXmlAttribute(xele, "MessageBody");
                    trigger.WCFURL = EventXmlAttribute(xele, "ProcessWcfUrl");
                    trigger.FUNCTIONNAME = EventXmlAttribute(xele, "WcfFuncName");
                    trigger.CONTRACTTYPE = EventXmlAttribute(xele, "ContractType");
                    string TriggerType = EventXmlAttribute(xele, "TriggerType");
                    if (string.IsNullOrEmpty(TriggerType))//
                    {
                        TriggerType = "User";
                    }
                    trigger.TRIGGERTYPE = TriggerType;
                    string WcfFuncParamter = string.Empty;
                    string MsgLinkUrl = string.Empty;
                    try
                    {
                        WcfFuncParamter = xele.Element("WcfFuncParamter").ToString().Replace("<WcfFuncParamter>", "").Replace("</WcfFuncParamter>", "");
                        trigger.FUNCTIONPARAMTER = WcfFuncParamter;

                    }
                    catch { }
                    try
                    {
                        MsgLinkUrl = xele.Element("MsgLinkUrl").ToString().Replace("<MsgLinkUrl>", "").Replace("</MsgLinkUrl>", "");
                        trigger.MESSAGEURL = MsgLinkUrl;
                    }
                    catch { }
                    trigger.PAMETERSPLITCHAR = EventXmlAttribute(xele, "WcfParamSplitChar");
                    trigger.WCFBINDINGCONTRACT = EventXmlAttribute(xele, "WcfBinding");
                    trigger.TRIGGERDESCRIPTION = "EventTrigger";
                    if (string.IsNullOrWhiteSpace(trigger.TRIGGERID))
                    {
                        trigger.TRIGGERID = Guid.NewGuid().ToString();
                    }
                    if (trigger.TRIGGERSTART == null)
                    {
                        trigger.TRIGGERSTART = DateTime.Now;
                    }
                    if (trigger.TRIGGEREND == null)
                    {
                        trigger.TRIGGERSTART = DateTime.Now.AddYears(60);
                    }
                    trigger.TRIGGERMULTIPLE = 1;
                    EngineServicesDAL dal = new EngineServicesDAL();
                    dal.AddTimingTrigger(trigger);
                }
                catch (Exception e)
                {
                    string cMessage = "<消息引擎>Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]";
                    LogHelper.WriteLog(cMessage);
                }

            }

        }
        #endregion





        #region 任务消息触发
        /// <summary>
        /// 消息触发
        /// </summary>
        /// <param name="recevieUser">接收人</param>
        /// <param name="SystemCode">系统代号</param>
        /// <param name="MsgKey">消息Key</param>
        /// <param name="ID">业务系统ID</param>
        /// <param name="strXml">业务数据</param>
        public void MsgTrigger(string[] recevieUser, string SystemCode, string MsgKey, string ID, string strXml)
        {
            if (recevieUser.Length > 0)
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    DataTable dt = dal.GetMessageDefine(MsgKey);
                    DataTable dtValue = Common.EncrytXmlToDataTable(strXml);
                    string Content = dt.Rows[0]["MESSAGEBODY"].ToString();
                    string Url = dt.Rows[0]["MESSAGEURL"].ToString();
                    Common.ReplaceValue(ref Content, ref Url, dtValue);
                    foreach (string user in recevieUser)
                    {
                        //刷新缓存用户是否有新的待办
                        TaskCache.TaskCacheReflesh(user);
                        dal.SendTriggerTaskMsg(user, SystemCode, "", ID, Content, Url, Common.BOObjectEscapeString(strXml), strXml);
                    }
                }
                catch (Exception e)
                {
                    string cMessage = "Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]" + "\r\n";
                    LogHelper.WriteLog("MsgTrigger():" + cMessage);
                }

            }
        }

        #endregion
        /// <summary>
        /// 取消事件定时
        /// </summary>
        /// <param name="strApplicationOrderCode">业务系统Guid</param>
        /// <returns></returns>
        public bool CancelEventTriggerData(string strApplicationOrderCode)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            return dal.DeleteTimingTrigger(strApplicationOrderCode);

        }

        #region 任务完成后，消息关闭
        public void TaskMsgClose(string strSystemCode, string strFormID, string strReceiveUser)//kangxf
        {
            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID) && !string.IsNullOrEmpty(strReceiveUser))
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    if (strReceiveUser.IndexOf(',') != -1)
                    {
                        string[] users = strReceiveUser.Split(',');
                        foreach (string user in users)
                        {
                            //刷新缓存用户是否有新的待办
                            TaskCache.TaskCacheReflesh(strReceiveUser);
                            dal.ClosedDoTaskStatus(strSystemCode, strFormID, user);
                        }
                    }
                    else
                    {
                        //刷新缓存用户是否有新的待办
                        TaskCache.TaskCacheReflesh(strReceiveUser);
                        dal.ClosedDoTaskStatus(strSystemCode, strFormID, strReceiveUser);
                    }
                }
                catch (Exception e)
                {

                    string cMessage = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + strReceiveUser + "',Error='" + e.Message + "'";
                    LogHelper.WriteLog("TaskMsgClose()" + cMessage);
                }
            }
        }
        #endregion
        #region 模块消息关闭
        public void ModelMsgClose(string strModelCode, string strReceiveUser)
        {
            if (!string.IsNullOrEmpty(strModelCode) && !string.IsNullOrEmpty(strReceiveUser))
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    //刷新缓存用户是否有新的待办
                    TaskCache.TaskCacheReflesh(strReceiveUser);
                    dal.ClosedDoTaskStatus(strModelCode, strReceiveUser);
                }
                catch (Exception e)
                {
                    string cMessage = "Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]" + "\r\n";
                    LogHelper.WriteLog("ModelMsgClose():" + cMessage);
                }
            }
        }

        public void ClosedDoTaskFromID(string strModelCode, string fromID)
        {
            if (!string.IsNullOrEmpty(strModelCode) && !string.IsNullOrEmpty(fromID))
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    //刷新缓存用户是否有新的待办
                    DataTable dt = dal.SelectTaskReceiveID(strModelCode, fromID);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            TaskCache.TaskCacheReflesh(dr["RECEIVEUSERID"].ToString());
                            dal.ClosedDoTaskOrderID(fromID, strModelCode, dr["RECEIVEUSERID"].ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    string cMessage = "Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]" + "\r\n";
                    LogHelper.WriteLog("ClosedDoTaskFromID():" + cMessage);
                }
            }
        }

        /// <summary>
        /// 关闭代办
        /// </summary>
        /// <param name="list"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strReceiveUser"></param>
        public void ClosedDoTaskOrderID(List<string> list, string strModelCode, string strReceiveUser)
        {
            if (!string.IsNullOrEmpty(strModelCode) && !string.IsNullOrEmpty(strReceiveUser))
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    //刷新缓存用户是否有新的待办
                    TaskCache.TaskCacheReflesh(strReceiveUser);
                    foreach (string item in list)
                    {
                        dal.ClosedDoTaskOrderID(item, strModelCode, strReceiveUser);
                    }
                }
                catch (Exception e)
                {
                    string cMessage = "Message=[" + e.Message + "]" + "<消息引擎>Source=[" + e.Source + "]<消息引擎>StackTrace=[" + e.StackTrace + "]<消息引擎>TargetSite=[" + e.TargetSite + "]" + "\r\n";
                    LogHelper.WriteLog("ClosedDoTaskOrderID():" + cMessage);
                }
            }
        }
        #endregion
        #region 消息关闭
        /// <summary>
        /// 关闭消息，与定时触发
        /// </summary>
        /// <param name="DaskID"></param>
        /// <param name="strEventID"></param>
        public void MsgClose(string DaskID, string strEventID)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            if (!string.IsNullOrEmpty(DaskID))
            {
                dal.ClosedDoTaskStatus(DaskID);
            }
            if (!string.IsNullOrEmpty(strEventID))
            {
                dal.DeleteTimingTrigger(strEventID);
            }
        }
        public void MsgListClose(string strSystemCode, List<ReceiveUserForm> List)
        {
            string strMsg = string.Empty;
            if (List != null && List.Count > 0)
            {
                if (!string.IsNullOrEmpty(strSystemCode))
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    foreach (ReceiveUserForm user in List)
                    {
                        if (user.FormID != null && user.FormID.Count > 0)
                        {
                            foreach (string strFormID in user.FormID)
                            {
                                try
                                {
                                    TaskCache.TaskCacheReflesh(user.ReceiveUser);
                                    dal.ClosedDoTaskStatus(strSystemCode, strFormID, user.ReceiveUser);
                                }
                                catch (Exception e)
                                {

                                    strMsg = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + user.ReceiveUser + "',Error='" + e.Message + "'";
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                strMsg = "MsgListClose（批量关闭待办任务） 没有得到关闭列表数据：SystemCode='" + strSystemCode + "'";
            }
            if (!string.IsNullOrEmpty(strMsg))
            {
                LogHelper.WriteLog("MsgListClose" + strMsg);
            }
        }
        #endregion

        #region 消息触发

        public void AddDoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType)
        {
            try
            {
                LogHelper.WriteLog("传入参数：companyID||" + companyID + "||orderID：" + orderID + "||modelCode：" + modelCode + "||strXML：" + strXML);
                EngineServicesDAL dal = new EngineServicesDAL();
                T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(systemCode, modelCode, companyID, 0).FirstOrDefault();
                if (MsgBodyDefine == null)
                {
                    LogHelper.WriteLog("EngineServicesBLL.AddDoDask()没有查询到默认消息：modelCode||" + modelCode + "||systemCode" + systemCode + "||companyID" + companyID);
                    return;
                }
                DataTable dtValue = Common.EncrytXmlToDataTable(strXML);
                string Content = MsgBodyDefine.MESSAGEBODY;
                string Url = MsgBodyDefine.MESSAGEURL;
                LogHelper.WriteLog("查询到的URL：" + Url);
                Content = Content.Replace("new:", "");
                Common.ReplaceValue(ref Content, ref Url, dtValue);
                LogHelper.WriteLog("替换后的URL：" + Url);
                //T_WF_DOTASK dask = new T_WF_DOTASK();
                if (MsgBodyDefine.RECEIVETYPE == 1 && MsgBodyDefine.RECEIVERUSERID != null)
                {
                    for (int i = 0; i < MsgBodyDefine.RECEIVERUSERID.Split(',').Length; i++)
                    {

                        if (MsgType.Task == msgType)//代办任务
                        {
                            TaskCache.TaskCacheReflesh(MsgBodyDefine.RECEIVERUSERID.Split(',')[i]);
                            dal.ClosedDoTaskStatus(systemCode, orderID, MsgBodyDefine.RECEIVERUSERID.Split(',')[i]);
                            LogHelper.WriteLog("开始进行保存时的URL：" + Url);
                            dal.SendTriggerTaskMsg(MsgBodyDefine.RECEIVERUSERID.Split(',')[i], systemCode, modelCode, orderID, Content, Url, Common.BOObjectEscapeString(strXML), strXML);
                        }
                        else if (MsgType.Msg == msgType)
                        {
                            dal.SendTriggerMsg(MsgBodyDefine.RECEIVERUSERID.Split(',')[i], systemCode, orderID, Content);
                        }

                    }
                }
                else if (MsgBodyDefine.RECEIVETYPE == 0 && MsgBodyDefine.RECEIVEPOSTID != null)
                {
                    for (int i = 0; i < MsgBodyDefine.RECEIVEPOSTID.Split(',').Length; i++)
                    {

                        PersonnelWS.PersonnelServiceClient HRClient = new PersonnelWS.PersonnelServiceClient();
                        LogHelper.WriteLog("岗位:" + MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]);
                        if (!string.IsNullOrEmpty(MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]))
                        {
                            string[] Employees = HRClient.GetEmployeeIDsByPostID(MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]);
                            LogHelper.WriteLog("岗位人:" + Employees[0]);
                            if (MsgType.Task == msgType)//代办任务
                            {
                                TaskCache.TaskCacheReflesh(Employees[0]);
                                dal.ClosedDoTaskStatus(systemCode, orderID, Employees[0]);
                                LogHelper.WriteLog("开始进行保存时的URL：" + Url);
                                dal.SendTriggerTaskMsg(Employees[0], systemCode, modelCode, orderID, Content, Url, Common.BOObjectEscapeString(strXML), strXML);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(Employees[0], systemCode, orderID, Content);
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("EngineServicesBLL.AddDoDask()" + ex.Message);
            }
        }


        public List<UserInfo> ReturnUserInfoDask(string companyID, string orderID, string systemCode, string modelCode, string modelName, string strXML, MsgType msgType)
        {
            try
            {
                PersonnelWS.PersonnelServiceClient HRClient = new PersonnelWS.PersonnelServiceClient();
                List<UserInfo> list = new List<UserInfo>();
                EngineServicesDAL dal = new EngineServicesDAL();
                T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(systemCode, modelCode, companyID, 0).FirstOrDefault();
                if (MsgBodyDefine == null)
                {
                    LogHelper.WriteLog("EngineServicesBLL.ReturnUserInfoDask()没有查询到默认消息：modelCode||" + modelCode + "||systemCode" + systemCode + "||companyID" + companyID);
                    return null;
                }
                DataTable dtValue = Common.EncrytXmlToDataTable(strXML);
                string Content = MsgBodyDefine.MESSAGEBODY;
                string Url = MsgBodyDefine.MESSAGEURL.Replace("Details", "Edit");//MVC 审核通过之后发送指定人的代办              
                Content = Content.Replace("new:", "");
                Common.ReplaceValue(ref Content, ref Url, dtValue);
                LogHelper.WriteLog("替换后的URL：" + Url);
                if (MsgBodyDefine.RECEIVETYPE == 1 && MsgBodyDefine.RECEIVERUSERID != null)
                {
                    for (int i = 0; i < MsgBodyDefine.RECEIVERUSERID.Split(',').Length; i++)
                    {
                        var Users = HRClient.GetFlowUserByUserID(MsgBodyDefine.RECEIVERUSERID.Split(',')[i]);
                        UserInfo info = new UserInfo();
                        info.CompanyID = Users[0].CompayID;
                        info.CompanyName = Users[0].CompayName;
                        info.DepartmentID = Users[0].DepartmentID;
                        info.DepartmentName = Users[0].DepartmentName;
                        info.PostID = Users[0].PostID;
                        info.PostName = Users[0].PostName;
                        info.UserID = Users[0].UserID;
                        info.UserName = Users[0].EmployeeName;
                        list.Add(info);
                        if (MsgType.Task == msgType)//代办任务
                        {
                            TaskCache.TaskCacheReflesh(MsgBodyDefine.RECEIVERUSERID.Split(',')[i]);
                            dal.ClosedDoTaskStatus(systemCode, orderID, MsgBodyDefine.RECEIVERUSERID.Split(',')[i]);
                            LogHelper.WriteLog("开始进行保存时的URL：" + Url);
                            dal.SendTriggerTaskMsg(MsgBodyDefine.RECEIVERUSERID.Split(',')[i], systemCode, modelCode, orderID, Content, Url, Common.BOObjectEscapeString(strXML), strXML);
                        }
                        else if (MsgType.Msg == msgType)
                        {
                            dal.SendTriggerMsg(MsgBodyDefine.RECEIVERUSERID.Split(',')[i], systemCode, orderID, Content);
                        }

                    }
                }
                else if (MsgBodyDefine.RECEIVETYPE == 0 && MsgBodyDefine.RECEIVEPOSTID != null)
                {
                    for (int i = 0; i < MsgBodyDefine.RECEIVEPOSTID.Split(',').Length; i++)
                    {
                        LogHelper.WriteLog("岗位:" + MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]);
                        if (!string.IsNullOrEmpty(MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]))
                        {
                            string[] Employees = HRClient.GetEmployeeIDsByPostID(MsgBodyDefine.RECEIVEPOSTID.Split(',')[i]);
                            var Users = HRClient.GetFlowUserByUserID(Employees[0]);
                            UserInfo info = new UserInfo();
                            info.CompanyID = Users[0].CompayID;
                            info.CompanyName = Users[0].CompayName;
                            info.DepartmentID = Users[0].DepartmentID;
                            info.DepartmentName = Users[0].DepartmentName;
                            info.PostID = Users[0].PostID;
                            info.PostName = Users[0].PostName;
                            info.UserID = Users[0].UserID;
                            info.UserName = Users[0].EmployeeName;
                            list.Add(info);
                            LogHelper.WriteLog("岗位人:" + Employees[0]);
                            if (MsgType.Task == msgType)//代办任务
                            {
                                TaskCache.TaskCacheReflesh(Employees[0]);
                                dal.ClosedDoTaskStatus(systemCode, orderID, Employees[0]);
                                LogHelper.WriteLog("开始进行保存时的URL：" + Url);
                                dal.SendTriggerTaskMsg(Employees[0], systemCode, modelCode, orderID, Content, Url, Common.BOObjectEscapeString(strXML), strXML);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(Employees[0], systemCode, orderID, Content);
                            }
                        }

                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("EngineServicesBLL.ReturnUserInfoDask()" + ex.Message);
                return null;
            }
        }


        public void ApplicationMsgTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strXml, MsgType msgType)
        {
            string strUserAndForm = string.Empty;
            string cMessage = string.Empty;
            if (UserAndForm.Count > 0)
            {

                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(SystemCode, ModelCode, string.Empty, 0).FirstOrDefault();
                    if (MsgBodyDefine != null) //在默认消息中有定义消息
                    {
                        string strMsg = "SystemCode:" + SystemCode + "\r\n" +
                                        "ModelCode:" + ModelCode + "\r\n" +
                                        "人员记录(Count)：" + UserAndForm.Count();
                        LogHelper.WriteLog("ApplicationMsgTrigger记录日志" + strMsg);
                        DataTable dtValue = Common.EncrytXmlToDataTable(strXml);
                        string Content = MsgBodyDefine.MESSAGEBODY;
                        string Url = MsgBodyDefine.MESSAGEURL;
                        Content = Content.Replace("new:", "");
                        Common.ReplaceValue(ref Content, ref Url, dtValue);
                        foreach (CustomUserMsg custom in UserAndForm)
                        {
                            strUserAndForm += "FormID:" + custom.FormID + " ReceiveUserID:" + custom.UserID + "\r\n";
                            if (MsgType.Task == msgType)//代办任务
                            {
                                TaskCache.TaskCacheReflesh(custom.UserID);
                                //刷新缓存用户是否有新的待办
                                //EngineCache.TaskCache.TaskCacheReflesh(custom.UserID);
                                dal.ClosedDoTaskStatus(SystemCode, custom.FormID, string.Empty);

                                dal.SendTriggerTaskMsg(custom.UserID, SystemCode, ModelCode, custom.FormID, Content, Url, Common.BOObjectEscapeString(strXml), strXml);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(custom.UserID, SystemCode, custom.FormID, Content);
                            }
                        }

                    }
                    else
                    {
                        cMessage = "未在消息定义中定义默认数据:ApplicationMsgTrigger" + "\r\n" +
                                         "SystemCode:" + SystemCode + "\r\n" +
                                         "ModelCode:" + ModelCode + "\r\n" +
                                         "执行时间：" + DateTime.Now.ToString();
                        LogHelper.WriteLog("ApplicationMsgTrigger()" + cMessage);
                    }
                }
                catch (Exception e)
                {
                    cMessage = "ApplicationMsgTrigger出现异常" + "\r\n" +
                                     "SystemCode:" + SystemCode + "\r\n" +
                                     "ModelCode:" + ModelCode + "\r\n" +
                                     "业务数据XML：" + strXml + "\r\n" +
                                     "异常信息:" + e.Message + "\r\n" +
                                     "异常来源:" + e.Source;
                    LogHelper.WriteLog("ApplicationMsgTrigger()" + cMessage);
                }

            }
            cMessage = "ApplicationMsgTrigger被执行调用" + "\r\n" +
                                        "SystemCode:" + SystemCode + "\r\n" +
                                        "ModelCode:" + ModelCode + "\r\n" +
                                        "业务数据XML：" + strXml + "\r\n" +
                                         "UserAndForm Count：" + UserAndForm.Count + "\r\n" +
                                         "UserAndForm:" + strUserAndForm;
            LogHelper.WriteLog("ApplicationMsgTrigger" + cMessage);


        }

        public void ApplicationMsgTriggerNew(List<CustomUserMsg> UserAndForm, string companyID, string SystemCode, string ModelCode, string strXml, MsgType msgType)
        {
            string strUserAndForm = string.Empty;
            string cMessage = string.Empty;
            if (UserAndForm.Count > 0)
            {

                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(SystemCode, ModelCode, companyID, 0).FirstOrDefault();
                    if (MsgBodyDefine != null) //在默认消息中有定义消息
                    {
                        DataTable dtValue = Common.EncrytXmlToDataTable(strXml);
                        string Content = MsgBodyDefine.MESSAGEBODY;
                        string Url = MsgBodyDefine.MESSAGEURL;
                        Content = Content.Replace("new:", "");
                        Common.ReplaceValue(ref Content, ref Url, dtValue);
                        foreach (CustomUserMsg custom in UserAndForm)
                        {
                            strUserAndForm += "FormID:" + custom.FormID + " ReceiveUserID:" + custom.UserID + "\r\n";
                            if (MsgType.Task == msgType)//代办任务
                            {
                                TaskCache.TaskCacheReflesh(custom.UserID);
                                dal.ClosedDoTaskStatus(SystemCode, custom.FormID, custom.UserID);

                                dal.SendTriggerTaskMsg(custom.UserID, SystemCode, ModelCode, custom.FormID, Content, Url, Common.BOObjectEscapeString(strXml), strXml);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(custom.UserID, SystemCode, custom.FormID, Content);
                            }
                        }

                    }
                    else
                    {
                        cMessage = "未在消息定义中定义默认数据:ApplicationMsgTriggerNew" + "\r\n" +
                                         "SystemCode:" + SystemCode + "\r\n" +
                                         "ModelCode:" + ModelCode + "\r\n" +
                                         "执行时间：" + DateTime.Now.ToString();
                        LogHelper.WriteLog("ApplicationMsgTriggerNew()" + cMessage);
                    }
                }
                catch (Exception e)
                {
                    cMessage = "ApplicationMsgTriggerNew出现异常" + "\r\n" +
                                     "SystemCode:" + SystemCode + "\r\n" +
                                     "ModelCode:" + ModelCode + "\r\n" +
                                     "业务数据XML：" + strXml + "\r\n" +
                                     "异常信息:" + e.Message + "\r\n" +
                                     "异常来源:" + e.Source;
                    LogHelper.WriteLog("ApplicationMsgTriggerNew()" + cMessage);
                }

            }
            cMessage = "ApplicationMsgTriggerNew被执行调用" + "\r\n" +
                                        "SystemCode:" + SystemCode + "\r\n" +
                                        "ModelCode:" + ModelCode + "\r\n" +
                                        "业务数据XML：" + strXml + "\r\n" +
                                         "UserAndForm Count：" + UserAndForm.Count + "\r\n" +
                                         "UserAndForm:" + strUserAndForm;
            LogHelper.WriteLog("ApplicationMsgTriggerNew" + cMessage);

        }

        /// <summary>
        /// 新增消息(2013-05-06加入新的方法)
        /// </summary>
        /// <param name="list">UserID|FromID XML</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelCode">模块代码</param>
        public void SendTaskMessage(List<CustomUserMsg> list, string SystemCode, string ModelCode)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(SystemCode, ModelCode, string.Empty, 0).FirstOrDefault();
                if (MsgBodyDefine != null) //在默认消息中有定义消息
                {
                    //TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
                    StringReader strRdr = null;
                    foreach (CustomUserMsg item in list)
                    {
                        strRdr = new StringReader(item.FormID);
                        XDocument xdoc = XDocument.Load(strRdr);
                        string content = MsgBodyDefine.MESSAGEBODY;
                        Regex reg = new Regex("\\{(.*?)\\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        MatchCollection matchs = reg.Matches(content);
                        foreach (var matitem in matchs)
                        {
                            string field = matitem.ToString().Replace("new:", "").Replace("{", "").Replace("}", "").ToUpper();
                            content = content.Replace(matitem.ToString(), (from ent in xdoc.Descendants("Object").Descendants("Attribute")
                                                                           where ent.Attribute("Name").Value.ToUpper() == field
                                                                           select ent).FirstOrDefault().Attribute("DataValue").Value);
                        }
                        dal.SendTriggerMsg(item.UserID.Split('|')[0], SystemCode, item.UserID.Split('|')[1], content);                     
                    }
                    //TimeSpan ts2= new TimeSpan(DateTime.Now.Ticks);
                    //TimeSpan ts = ts2.Subtract(ts1).Duration();//计算时间差
                    strRdr.Dispose();
                }
                else
                {
                    throw new Exception("没有定义默认消息");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
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
            string strUserAndForm = string.Empty;

            string cMessage = string.Empty;
            if (UserAndForm.Count > 0)
            {

                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(SystemCode, ModelCode, string.Empty, -1).FirstOrDefault();

                    if (MsgBodyDefine != null) //在默认消息中有定义消息
                    {
                        string strMsg = "SystemCode:" + SystemCode + "\r\n" +
                                        "ModelCode:" + ModelCode + "\r\n" +
                                        "人员记录(Count)：" + UserAndForm.Count;
                        LogHelper.WriteLog("ApplicationMsgTrigger记录日志" + strMsg);
                        DataTable dtValue = Common.EncrytXmlToDataTable(strXml);

                        string Content = messageBody;
                        string Url = MsgBodyDefine.MESSAGEURL;
                        Common.ReplaceValue(ref Content, ref Url, dtValue);
                        foreach (CustomUserMsg custom in UserAndForm)
                        {
                            strUserAndForm += "FormID:" + custom.FormID + " ReceiveUserID:" + custom.UserID + "\r\n";
                            if (MsgType.Task == msgType)//代办任务
                            {
                                //刷新缓存用户是否有新的待办
                                //EngineCache.TaskCache.TaskCacheReflesh(custom.UserID);
                                dal.ClosedDoTaskStatus(SystemCode, custom.FormID, string.Empty);

                                dal.SendTriggerTaskMsg(custom.UserID, SystemCode, ModelCode, custom.FormID, Content, Url, Common.BOObjectEscapeString(strXml), strXml);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(custom.UserID, SystemCode, custom.FormID, Content);
                            }
                        }

                    }
                    else
                    {
                        cMessage = "未在消息定义中定义默认数据:ApplicationMsgTriggerCustom" + "\r\n" +
                                         "SystemCode:" + SystemCode + "\r\n" +
                                         "ModelCode:" + ModelCode + "\r\n" +
                                         "执行时间：" + DateTime.Now.ToString();
                        LogHelper.WriteLog("ApplicationMsgTriggerCustom记录日志" + cMessage);
                    }
                }
                catch (Exception e)
                {
                    cMessage = "ApplicationMsgTriggerCustom出现异常" + "\r\n" +
                                     "SystemCode:" + SystemCode + "\r\n" +
                                     "ModelCode:" + ModelCode + "\r\n" +
                                     "业务数据XML：" + strXml + "\r\n" +
                                     "异常信息:" + e.Message + "\r\n" +
                                     "异常来源:" + e.Source;
                    LogHelper.WriteLog("ApplicationMsgTriggerCustom记录日志" + cMessage);
                }

            }
            cMessage = "ApplicationMsgTriggerCustom被执行调用" + "\r\n" +
                                        "SystemCode:" + SystemCode + "\r\n" +
                                        "ModelCode:" + ModelCode + "\r\n" +
                                        "业务数据XML：" + strXml + "\r\n" +
                                         "UserAndForm Count：" + UserAndForm.Count + "\r\n" +
                                         "UserAndForm:" + strUserAndForm;
            LogHelper.WriteLog("ApplicationMsgTriggerCustom记录日志" + cMessage);


        }
        public void ApplicationEngineTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string ModelCode, string strCompanyID, string strXml, MsgType msgType)
        {

            if (UserAndForm.Count > 0)
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    T_WF_MESSAGEBODYDEFINE MsgBodyDefine = dal.GetMessageBodyDefine(SystemCode, ModelCode, strCompanyID, 0).FirstOrDefault();
                    if (MsgBodyDefine != null) //在默认消息中有定义消息
                    {
                        DataTable dtValue = Common.EncrytXmlToDataTable(strXml);
                        string Content = MsgBodyDefine.MESSAGEBODY;
                        Content = Content.Replace("new:", "");
                        string Url = MsgBodyDefine.MESSAGEURL;
                        Common.ReplaceValue(ref Content, ref Url, dtValue);
                        foreach (CustomUserMsg custom in UserAndForm)
                        {
                            if (MsgType.Task == msgType)//代办任务
                            {
                                TaskCache.TaskCacheReflesh(custom.UserID);
                                dal.ClosedDoTaskStatus(SystemCode, custom.FormID, custom.UserID);//做车辆的时候调整，原因是同一种单据发不同的人代办                             
                                dal.SendTriggerTaskMsg(custom.UserID, SystemCode, ModelCode, custom.FormID, Content, Url, Common.BOObjectEscapeString(strXml), string.Empty);
                            }
                            else if (MsgType.Msg == msgType)
                            {
                                dal.SendTriggerMsg(custom.UserID, SystemCode, custom.FormID, Content);
                            }
                        }
                    }
                    else
                    {
                        string Message = "未在消息定义中定义默认数据:ApplicationMsgTrigger" + "\r\n" +
                                         "SystemCode:" + SystemCode + "\r\n" +
                                         "ModelCode:" + ModelCode + "\r\n" +
                                         "执行时间：" + DateTime.Now.ToString();
                        LogHelper.WriteLog("ApplicationMsgTrigger()" + Message);
                    }
                }
                catch (Exception e)
                {
                    string cMessage = "ApplicationMsgTrigger出现异常" + "\r\n" +
                                      "SystemCode:" + SystemCode + "\r\n" +
                                      "ModelCode:" + ModelCode + "\r\n" +
                                      "业务数据XML：" + strXml + "\r\n" +
                                      "异常信息:" + e.Message + "\r\n" +
                                      "异常来源:" + e.Source;
                    LogHelper.WriteLog("ApplicationMsgTrigger()" + cMessage);
                }

            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ReceiveUserID"></param>
        /// <param name="strFormID"></param>
        /// <param name="MsgContent"></param>
        /// <param name="ModelCode"></param>
        /// <param name="strXml"></param>
        public void AppMsgTrigger(string ReceiveUserID, string strFormID, string MsgContent, string ModelCode, string strXml, string strNewGuid)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                string SystemCode = string.Empty;
                if (!string.IsNullOrEmpty(strXml))
                {
                    Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                    XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
                    SystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
                }
                DataTable dtFlowTrigger = dal.FlowTriggerTable(SystemCode, ModelCode, null, null);
                if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
                {
                    TaskCache.TaskCacheReflesh(ReceiveUserID);
                    DataTable dtValue = Common.EncrytXmlToDataTable(strXml);
                    string Url = dtFlowTrigger.Rows[0]["applicationurl"] == null ? "" : dtFlowTrigger.Rows[0]["applicationurl"].ToString();
                    Url = Common.EncyptUrlNeedXMLHeader(Url, strFormID);
                    // MsgTriggerBLL.ReplaceValue(ref Content, ref Url, dtValue);
                    string strApplicationValue = Common.BOObjectEscapeString(strXml);
                    //刷新缓存用户是否有新的待办
                    //EngineCache.TaskCache.TaskCacheReflesh(ReceiveUserID);
                    dal.SendTriggerTaskMsg(ReceiveUserID, SystemCode, "", strNewGuid, MsgContent, Url, strApplicationValue, string.Empty);
                }
            }
            catch (Exception e)
            {
                string cMessage = "AppMsgTrigger出现异常" + "\r\n" +
                                  "ModelCode:" + ModelCode + "\r\n" +
                                   "strFormID:" + strFormID + "\r\n" +
                                  "异常信息:" + e.Message + "\r\n" +
                                  "异常来源:" + e.Source;
                LogHelper.WriteLog("ApplicationMsgTrigger" + cMessage);
            }
        }

        public void NotesSend(List<ReceiveUserAndContent> List, string SystemCode, string strFormID)
        {
            if (List != null)
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                foreach (ReceiveUserAndContent entity in List)
                {
                    dal.SendTriggerMsg(entity.ReceiveUser, SystemCode, strFormID, entity.Content);
                }
            }
        }
        /// <summary>
        /// 引擎直接发消息
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="SystemCode"></param>
        /// <param name="Content"></param>
        public void ApplicationNotesTrigger(List<CustomUserMsg> UserAndForm, string SystemCode, string Content)
        {

            if (UserAndForm.Count > 0)
            {
                try
                {
                    EngineServicesDAL dal = new EngineServicesDAL();
                    foreach (CustomUserMsg custom in UserAndForm)
                    {
                        dal.SendTriggerMsg(custom.UserID, SystemCode, custom.FormID, Content);
                    }

                }
                catch (Exception e)
                {
                    string cMessage = "ApplicationNotesTrigger出现异常" + "\r\n" +
                                      "SystemCode:" + SystemCode + "\r\n" +
                                      "Content:" + Content + "\r\n" +
                                      "异常信息:" + e.Message + "\r\n" +
                                      "异常来源:" + e.Source;
                    LogHelper.WriteLog("ApplicationMsgTrigger" + cMessage);
                }

            }

        }
        #endregion

        #region 用户自定义发起流程数据
        public List<T_FLOW_CUSTOMFLOWDEFINE> CustomFlowDefineList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCompanyID)
        {
            LogHelper.WriteLog("用户自定义发起流程数据方法：CustomFlowDefineList（）");
            //try
            //{
            //    return null;
            //}
            //catch (Exception ex)
            //{
            //    SMT.Framwork.Global.ErrorLog.WriteErrorMessage(ex);
            //}
            return null;
        }
        #endregion

        #region 自定义发起流程Call服务
        public void CallCustomFlowTrigger(T_FLOW_CUSTOMFLOWDEFINE define)
        {
            string strResult = CallWcf(define);//Call 服务
            if (!string.IsNullOrEmpty(strResult))//判断是否返回内容
            {
                try
                {
                    string NewFormID = string.Empty;
                    DataRow DRNewTrigger = null;
                    DataTable sourceTable = new DataTable();
                    if (ApplicationValueToDataTable(strResult, ref sourceTable, ref NewFormID, ref DRNewTrigger))
                    {

                        string strUser = SMT.Workflow.Engine.BLL.Utility.ReceiveUser(define.RECEIVEUSER);
                        TaskCache.TaskCacheReflesh(strUser);
                        EngineServicesDAL dal = new EngineServicesDAL();
                        dal.SendTriggerMsg(sourceTable, NewFormID, define, strUser);//发送消息
                    }
                }
                catch (Exception ex)
                {
                    string cMessage = "CallCustomFlowTrigger:" + strResult;
                    LogHelper.WriteLog(cMessage + ex.Message);
                }
            }

        }

        /// <summary>
        /// 调用WCF返回数据转换成数据源
        /// </summary>
        /// <param name="strAppMsg">返回数据内容</param>
        /// <param name="sourceTable">原数据源Table</param>
        /// <param name="IsNewFlow">是否是新流程</param>
        /// <param name="DRFlowTrigger">新流程对应的流程定义</param>
        /// <returns></returns>
        private bool ApplicationValueToDataTable(string strAppMsg, string strCompanyID, ref DataTable sourceTable, ref string IsNewFlow, ref string NewFormID, ref string strFormTypes, ref DataRow DRFlowTrigger)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                if (sourceTable == null)
                {
                    sourceTable = new DataTable();
                    sourceTable.Columns.Add("FieldType", typeof(string));
                    sourceTable.Columns.Add("ColumnName", typeof(string));
                    sourceTable.Columns.Add("ColumnValue", typeof(string));
                }
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strAppMsg);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var v = from c in xele.Descendants("Attribute")
                        select c;
                IsNewFlow = string.Empty;
                try
                {
                    //是否是新流程（0：新流程，1：旧流程）
                    IsNewFlow = (from c in xele.Descendants("IsNewFlow") select c).FirstOrDefault().Value;
                }
                catch
                {
                    IsNewFlow = "1";
                }
                try
                {
                    NewFormID = (from c in xele.Descendants("NewFormID") select c).FirstOrDefault().Value;
                }
                catch { }
                try
                {
                    strFormTypes = (from c in xele.Descendants("FormTypes") select c).FirstOrDefault().Value;
                }
                catch { }

                if (v.Count() > 0)
                {
                    string SystemCode = string.Empty;
                    string ModelCode = string.Empty;
                    foreach (var vv in v)
                    {
                        DataRow dr = sourceTable.NewRow();
                        string Name = vv.Attribute("Name").Value.ToString();
                        DataRow[] drs = sourceTable.Select("ColumnName='" + Name + "'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow row in drs)
                            {
                                sourceTable.Rows.Remove(row);//移除相同数据
                            }
                        }
                        string Value = vv.Attribute("DataValue").Value.ToString();
                        dr["ColumnName"] = Name;
                        dr["ColumnValue"] = Value;
                        dr["FieldType"] = "sys";
                        if (IsNewFlow == "0")
                        {
                            switch (Name)
                            {
                                case "SYSTEMCODE":
                                    SystemCode = Value;
                                    break;
                                case "MODELCODE":
                                    ModelCode = Value;
                                    break;
                            }
                        }
                        sourceTable.Rows.Add(dr);
                    }
                    //新流程，并且系统代号与模块代号不为空  
                    if (IsNewFlow == "0" && !string.IsNullOrEmpty(SystemCode) && !string.IsNullOrEmpty(ModelCode))
                    {
                        DataTable table = dal.FlowTriggerTable(SystemCode, ModelCode, null, strCompanyID);
                        if (table != null && table.Rows.Count > 0)
                        {
                            DRFlowTrigger = table.Rows[0];
                        }

                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        /// <summary>
        /// 调用WCF返回数据转换成数据源
        /// </summary>
        /// <param name="strAppMsg">返回数据内容</param>
        /// <param name="sourceTable">原数据源Table</param>
        /// <param name="IsNewFlow">是否是新流程</param>
        /// <param name="DRFlowTrigger">新流程对应的流程定义</param>
        /// <returns></returns>
        private bool ApplicationValueToDataTable(string strAppMsg, ref DataTable sourceTable, ref string NewFormID, ref DataRow DRFlowTrigger)
        {
            try
            {

                sourceTable = new DataTable();
                sourceTable.Columns.Add("FieldType", typeof(string));
                sourceTable.Columns.Add("ColumnName", typeof(string));
                sourceTable.Columns.Add("ColumnValue", typeof(string));

                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strAppMsg);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var v = from c in xele.Descendants("Attribute")
                        select c;
                try
                {
                    NewFormID = (from c in xele.Descendants("NewFormID") select c).FirstOrDefault().Value;
                }
                catch { }
                if (v.Count() > 0)
                {
                    string SystemCode = string.Empty;
                    string ModelCode = string.Empty;
                    foreach (var vv in v)
                    {
                        DataRow dr = sourceTable.NewRow();
                        string Name = vv.Attribute("Name").Value;
                        DataRow[] drs = sourceTable.Select("ColumnName='" + Name + "'");
                        if (drs != null && drs.Length > 0)
                        {
                            foreach (DataRow row in drs)
                            {
                                sourceTable.Rows.Remove(row);//移除相同数据
                            }
                        }
                        string Value = vv.Attribute("DataValue").Value;
                        dr["ColumnName"] = Name;
                        dr["ColumnValue"] = Value;
                        dr["FieldType"] = "sys";

                        sourceTable.Rows.Add(dr);
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        private string CallWcf(T_FLOW_CUSTOMFLOWDEFINE define)
        {
            string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                              "<Paras>" + "\r\n" +
                              "<Para TableName=\"\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"" + define.RECEIVEUSER + "\"></Para>" +
                              "<Para TableName=\"\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"" + define.OWNERDEPARTMENTID + "\"></Para>" +
                              "<Para TableName=\"\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"" + define.OWNERPOSTID + "\"></Para>" +
                              "<Para TableName=\"\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"" + define.OWNERCOMPANYID + "\"></Para>" +
                              "{0}" +
                              "</Paras>";
            string TmpWCFBindingContract = string.Concat(define.WCFBINDINGCONTRACT);
            string TmpProcessFuncWCFUrl = Config.GetSystemCode(define.SYSTEMCODE) + define.PROCESSWCFURL;
            string TmpProcessFuncName = define.PROCESSFUNCNAME;
            string TmpProcessFuncPameter = string.Format(XmlTemplete, define.PROCESSFUNCPAMETER);
            try
            {
                if (!string.IsNullOrEmpty(TmpWCFBindingContract) && !string.IsNullOrEmpty(TmpProcessFuncWCFUrl) && !string.IsNullOrEmpty(TmpProcessFuncName))
                {

                    string ErroMesssage = "";
                    object TmpreceiveString = SMT.Workflow.Engine.BLL.Utility.CallWCFService(TmpWCFBindingContract, TmpProcessFuncWCFUrl, TmpProcessFuncName, TmpProcessFuncPameter, ref  ErroMesssage);
                    string cMessage = "执行链接：" + TmpProcessFuncWCFUrl + "\r\n" +
                                      "执行方法:" + TmpProcessFuncName + "\r\n" +
                                      "绑定契约：" + TmpWCFBindingContract + "\r\n" +
                                      "执行参数：" + TmpProcessFuncPameter + "\r\n" +
                                      "----------------------------------------------------------";
                    LogHelper.WriteLog("自定义发起流程Call服务:方法CallWcf（）" + cMessage);
                    return TmpreceiveString == null ? string.Empty : TmpreceiveString.ToString();

                }
            }
            catch (Exception e)
            {

                string cMessage = "执行链接：" + TmpProcessFuncWCFUrl + "\r\n" +
                                "执行方法:" + TmpProcessFuncName + "\r\n" +
                                "绑定契约：" + TmpWCFBindingContract + "\r\n" +
                                "执行参数：" + TmpProcessFuncPameter + "\r\n" +
                                "----------------------------------------------------------";
                LogHelper.WriteLog("自定义发起流程Call服务异常:方法CallWcf（）" + cMessage + e.Message);
            }
            return string.Empty;
        }
        #endregion

        #region 发送邮件
        public void SendMail(List<MailParams> mailParams)
        {
            try
            {
                if (mailParams != null && mailParams.Count > 0)
                {
                    MailAddress sendAddress = new MailAddress(Config.MailAddress);
                    foreach (MailParams param in mailParams)
                    {
                        MailAddress receiveAddress = new MailAddress(param.ReceiveUserMail);
                        MailMessage message = new MailMessage(sendAddress, receiveAddress);
                        string Title = param.MailTitle;
                        string PutDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        string BodyTemplete = GetMailBodeTempete();
                        string MsgUrl = Config.MailUrl;
                        PutDate = Convert.ToDateTime(PutDate).ToString("yyyy年MM月dd日");
                        BodyTemplete = string.Format(BodyTemplete, MsgUrl, Title, PutDate);
                        message.Subject = string.Format(Config.MailTitle, param.MailTitle);
                        message.IsBodyHtml = true;
                        message.Body = Utf8(BodyTemplete);
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        string smtp = Config.MailServerAddress;
                        SmtpClient sc = new SmtpClient(smtp);
                        sc.Port = Config.MailServerPort;
                        sc.Credentials = new System.Net.NetworkCredential(Config.MailAddress, Config.MailPwd);
                        sc.Timeout = 3000;
                        sc.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("发送邮件出现异常(SendMail)" + ex.Message);
            }
        }

        public bool SendMailParam(List<MailParams> mailParams, string mailType, string mailParameter)
        {
            try
            {
                if (mailParams != null && mailParams.Count > 0)
                {
                    MailAddress sendAddress = new MailAddress(Config.MailAddress);
                    foreach (MailParams param in mailParams)
                    {
                        MailAddress receiveAddress = new MailAddress(param.ReceiveUserMail);
                        MailMessage message = new MailMessage(sendAddress, receiveAddress);
                        //string Title = param.MailTitle;
                        //string PutDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        //string BodyTemplete = GetMailBodeTempete();
                        //string MsgUrl = Config.MailUrl;
                        //PutDate = Convert.ToDateTime(PutDate).ToString("yyyy年MM月dd日");
                        //BodyTemplete = string.Format(BodyTemplete, MsgUrl, Title, PutDate);
                        message.Subject = param.MailTitle;
                        if (mailType == "0")
                        {
                            message.IsBodyHtml = false;
                        }
                        else
                        {
                            message.IsBodyHtml = true;
                        }
                        message.Body = Utf8(param.MailContent);
                        message.BodyEncoding = System.Text.Encoding.UTF8;
                        string smtp = Config.MailServerAddress;
                        SmtpClient sc = new SmtpClient(smtp);
                        sc.Port = Config.MailServerPort;
                        sc.Credentials = new System.Net.NetworkCredential(Config.MailAddress, Config.MailPwd);
                        sc.Timeout = 3000;
                        sc.Send(message);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public string Utf8(string strLine)
        {
            byte[] utf8byte = Encoding.Unicode.GetBytes(strLine);
            string strOutLine = Encoding.UTF8.GetString(utf8byte);
            return strLine;

        }
        public string GetMailBodeTempete()
        {
            try
            {
                StreamReader sr = File.OpenText(Config.MailTempletePath);
                string strRead = sr.ReadToEnd();
                sr.Dispose();
                return strRead;
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("引擎对外公开服务发送邮件时出错（没有找到邮件内容模版）" + e.Message);
                return "";
            }
        }
        #endregion

        #region 新增接口

        public T_WF_DOTASK GetDoTaskEntity(string orderID, string receiveUserID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderID) || string.IsNullOrWhiteSpace(receiveUserID))
                {
                    throw new Exception("参数不全！");
                }
                EngineServicesDAL dal = new EngineServicesDAL();
                return dal.GetDoTaskEntity(orderID, receiveUserID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public bool AddTimingTrigger(T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                if (string.IsNullOrWhiteSpace(entity.TRIGGERID))
                {
                    entity.TRIGGERID = Guid.NewGuid().ToString();
                }
                if (entity.TRIGGERACTIVITYTYPE < 1)
                {
                    throw new Exception("触发类型必填");
                }
                if (entity.TRIGGERTIME == null)
                {
                    throw new Exception("触发时间必填");
                }
                if (entity.TRIGGERROUND < 0)
                {
                    throw new Exception("触发周期必填");
                }
                if (entity.TRIGGERSTART == null)
                {
                    entity.TRIGGERSTART = DateTime.Now;
                }
                if (entity.TRIGGEREND == null)
                {
                    entity.TRIGGEREND = DateTime.Now.AddYears(60);
                }
                if (entity.TRIGGERMULTIPLE <= 0)
                {
                    entity.TRIGGERMULTIPLE = 1;
                }
                return dal.AddTimingTrigger(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        public void DeleteTrigger(string orderID)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                if (!string.IsNullOrEmpty(orderID))
                {
                    dal.DeleteTimingTrigger(orderID);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        public void AddTask(T_WF_DOTASK dask)
        {
            try
            {
                EngineServicesDAL dal = new EngineServicesDAL();
                if (string.IsNullOrWhiteSpace(dask.DOTASKID))
                {
                    dask.DOTASKID = Guid.NewGuid().ToString();
                }
                if (string.IsNullOrWhiteSpace(dask.ORDERID))
                {
                    throw new Exception("单据ID不能为空");
                }
                if (string.IsNullOrWhiteSpace(dask.RECEIVEUSERID))
                {
                    throw new Exception("接收人ID不能为空");
                }
                TaskCache.TaskCacheReflesh(dask.RECEIVEUSERID);
                dal.AddTask(dask);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }
        #endregion

        #region 删除待办
        public void TaskDelete(List<CustomUserMsg> UserAndForm, string strSystemCode)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            if (UserAndForm != null && UserAndForm.Count > 0)
            {
                foreach (CustomUserMsg user in UserAndForm)
                {
                    //刷新缓存用户是否有新的待办
                    //EngineCache.TaskCache.TaskCacheReflesh(user.UserID);
                    TaskCache.TaskCacheReflesh(user.UserID);
                    dal.DeleteDoTaskStatus(strSystemCode, user.FormID, user.UserID);
                }
            }

        }
        public void TaskDelete(string strSystemCode, string strFormID, string strReceiveID)
        {
            EngineServicesDAL dal = new EngineServicesDAL();
            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID))
            {
                TaskCache.TaskCacheReflesh(strReceiveID);
                dal.DeleteDoTaskStatus(strSystemCode, strFormID, strReceiveID);
            }
        }

        #endregion

    }
}

