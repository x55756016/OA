/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：EnginFlowBLL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/12 9:58:33   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.FlowWFService 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Data;
using SMT.FlowDAL;
using SMT.SaaS.BLLCommonServices;
using SMT.Workflow.Common.Model.FlowEngine;
using System.ServiceModel;
using System.Reflection;
using System.Data.OracleClient;
using System.Xml;
using SMT.Workflow.Common.DataAccess;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using EngineDataModel;
using SMT.EntityFlowSys;
using SMT.Foundation.Log;


namespace SMT.FlowWFService.NewFlow
{
    public class EnginFlowBLL
    {

        #region
        /// <summary>
        /// 我的单据新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool AddPersonalRecord(OracleConnection con, SubmitData submitData, string state, FlowUser user, FLOW_FLOWRECORDMASTER_T entity)
        {
            try
            {
                LogHelper.WriteLog("在填写我的单据时候的参数CHECKSTATE:" + state + "||模块名称：" + user.ModelName);
                T_PF_PERSONALRECORD model = new T_PF_PERSONALRECORD();
                model.SYSTYPE = user.SysCode;               
                model.PERSONALRECORDID = Guid.NewGuid().ToString();
                model.ISFORWARD = "0";            
                model.ISVIEW = "0";            
                switch (state)
                {
                    case "1":
                        model.MODELCODE = submitData.ModelCode;
                        model.MODELID = submitData.FormID;
                        if (string.IsNullOrWhiteSpace(submitData.SumbitUserID) && string.IsNullOrWhiteSpace(submitData.SumbitCompanyID))
                        {
                            //预算有这样情况submitData.SumbitUserID为空
                            model.OWNERCOMPANYID = submitData.ApprovalUser.CompanyID;
                            model.OWNERDEPARTMENTID = submitData.ApprovalUser.DepartmentID;
                            model.OWNERID = submitData.ApprovalUser.UserID;
                            model.OWNERPOSTID = submitData.ApprovalUser.PostID;
                        }
                        else
                        {
                            model.OWNERCOMPANYID = submitData.SumbitCompanyID;
                            model.OWNERDEPARTMENTID = submitData.SumbitDeparmentID;
                            model.OWNERID = submitData.SumbitUserID;
                            model.OWNERPOSTID = submitData.SumbitPostID;
                        }
                        model.MODELDESCRIPTION = string.Format("您{0}的[{1}]正在审核中！", DateTime.Now.ToString("MM月dd日HH:mm"), user.ModelName.Trim());                        
                        break;
                    case "2":
                        model.MODELID = submitData.FormID;
                        model.MODELCODE = entity.MODELCODE;                 
                        model.MODELDESCRIPTION = string.Format("您{0}的[{1}]已经审核通过！", entity.CREATEDATE.ToString("MM月dd日HH:mm"), user.ModelName.Trim());
                        break;
                    case "3":
                        model.MODELID = submitData.FormID;
                        model.MODELCODE = entity.MODELCODE;                   
                        model.MODELDESCRIPTION = string.Format("您{0}的[{1}]已经审核不通过！", entity.CREATEDATE.ToString("MM月dd日HH:mm"), user.ModelName.Trim());
                        break;
                }
                model.CHECKSTATE = state;
                model.CREATEDATE = DateTime.Now;
                model.CONFIGINFO = ConvertXML(submitData);
                if (!string.IsNullOrWhiteSpace(model.CONFIGINFO))
                {
                    EnginFlowDAL dal = new EnginFlowDAL();
                    string recordID = dal.GetExistRecord(con,model.SYSTYPE, model.MODELCODE, model.MODELID, model.ISFORWARD);
                    if (recordID != "")
                    {
                        return dal.UpdatePersonalRecord(con,model, recordID);
                    }
                    else
                    {
                        return dal.AddPersonalRecord(con,model);
                    }
                }
                else
                {
                    LogHelper.WriteLog("在填写我的单据时候获取不到配置FormID:" + submitData.FormID + "");
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("在填写我的单据时候错误FormID:" + submitData.FormID + "||Exception" + ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        private static string ConvertXML(SubmitData submitData)
        {
        
            string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                "<System>" + "\r\n" +
                                "{0}" +
                                "</System>";
            try
            {
                StringReader strRdr = new StringReader(submitData.XML);
                XDocument xdoc = XDocument.Load(strRdr);
                string strXml = xdoc.Document.ToString();
                int Start = strXml.IndexOf("<MsgOpen>") + "<MsgOpen>".Length;
                int End = strXml.IndexOf("</MsgOpen>");
                string MsgLinkUrl = strXml.Substring(Start, End - Start);
                Start = MsgLinkUrl.IndexOf("<ApplicationOrder>") + "<ApplicationOrder>".Length;
                End = MsgLinkUrl.IndexOf("</ApplicationOrder>");
                string Order = MsgLinkUrl.Substring(Start, End - Start).Replace("{", "").Replace("}", "");
                string[] arr = Order.Split('*');
                foreach (string arrItem in arr)
                {
                    MsgLinkUrl = MsgLinkUrl.Replace("{" + arrItem + "}", (from item in xdoc.Descendants("Object").Descendants("Attribute")
                                                                          where item.Attribute("Name").Value.ToUpper() == arrItem.ToUpper()
                                                                          select item).FirstOrDefault().Attribute("DataValue").Value);

                }
                MsgLinkUrl = string.Format(XmlTemplete, MsgLinkUrl);
                return MsgLinkUrl;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("Formid="+submitData.FormID+";转换元数据XML出错："+submitData.XML);
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion
        #region 龙康才新增
        /// <summary>
        /// 流程触发
        /// </summary>
        /// <param name="strFlow">流程数据</param>
        /// <param name="strBusiness">业务数据</param>    
        /// <returns></returns>
        public bool SaveFlowTriggerData(OracleConnection con, SubmitData submitData, string strFlow, string strBusiness, ref FlowUser sUser, ref string ErroMessage)
        {

            string tmpFlow = "";
            string tmpEngline = "";
            T_WF_DOTASK entity = new T_WF_DOTASK();
            string strEntityType = string.Empty;//EntityType (表名)
            string strEntityKey = string.Empty;//EntityKey (主键)
            string IsTask = "1";//是否任务
            #region 获取 SystemCode
            string SystemCode = "";
            Byte[] bFlow = System.Text.UTF8Encoding.UTF8.GetBytes(strFlow);
            XElement xeFlow = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(bFlow)));
            var strReturn = from item in xeFlow.Descendants("SystemCode")
                               select item;
            if (strReturn.FirstOrDefault() != null)
            {
                SystemCode=strReturn.FirstOrDefault().Value.Replace("\"", "");
            }
            #endregion
            try
            {
                try
                {
                    //消息解析
                    //LogHelper.WriteLog("strFlow:\r\n" + strFlow);
                    //LogHelper.WriteLog("strBusiness:\r\n" + strBusiness);
                    string xmlred = "  开始消息解析XML\r\n";
                    xmlred += "FlowXML=" + strFlow+"\r\n";
                    xmlred += "AppXML=" + strBusiness + "\r\n";
                    #region 龙康才新增
                    #region XmlReader
                    #region strFlow
                    //Byte[] BytebFlow = System.Text.UTF8Encoding.UTF8.GetBytes(strFlow);

                    //XmlReader xr = XmlReader.Create(new MemoryStream(BytebFlow));
                    //StringBuilder _ConbinString = new StringBuilder();
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
                                        tmpFlow += xr["Name"] + "|" + xr["DataValue"] + "Ё\r\n";
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
                        entity.APPXML = strBusiness;
                    }
                    #endregion
                    #region strBusiness
                    if (!string.IsNullOrEmpty(strBusiness))
                    {
                        StringReader rd = new StringReader(strBusiness);
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
                                                tmpEngline += xdr["Name"] + "|" + xdr["DataValue"] + "Г" + (xdr["DataText"] != null ? xdr["DataText"] : xdr["DataValue"]) + "Ё\r\n";
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
                                                    if (CheckModelName(entity.MODELCODE) && entity.ORDERSTATUS == 2)
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
                    #endregion
                    #endregion                  
                    xmlred+="FormID=" + entity.ORDERID+"  结束消息解析XML";
                    LogHelper.WriteLog("FormID=" + entity.ORDERID + xmlred);
                }
                catch (Exception ex)
                {

                    sUser.ErrorMsg += "命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()解析XML出错!FORMID=" + sUser.FormID + " 异常信息:" + ex.ToString() + "\r\n";
                    ErroMessage = "命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()解析XML出错";
                    throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()解析XML出错" + ex.Message);                   
                }
                if (string.IsNullOrEmpty(entity.SYSTEMCODE))
                {
                    entity.SYSTEMCODE = SystemCode;
                }
                //思路：
                //1、先检验流程的默认消息 
                //2、调用业务系统更新 
                //3、自动发起流程 
                bool IsExecute = EngineExecute(con, entity, IsTask,ref sUser, ref  ErroMessage);
                if (IsExecute)
                {
                    bool isOK = CallBusinessSystem(submitData,entity, strEntityType, strEntityKey, ref ErroMessage);//调用业务系统
                    if (!isOK)
                    {
                        throw new Exception(ErroMessage);//把业务系统的错误信息给客户端
                    }
                    AutoCallFlow(con, dal, entity, dtFlowTrigger, sourceTable, ref  sUser, strAppFieldValue, ref ErroMessage);
                }
                else
                {
                    LogHelper.WriteLog("没有默认消息，也调用业务系统更新（没有待办任务产生） FORMID=" + sUser.FormID + " 异常信息：" + ErroMessage);
                    CallBusinessSystem(submitData,entity, strEntityType, strEntityKey, ref ErroMessage);//调用业务系统
                }
                return IsExecute;
            }
            catch (Exception ex)
            {
                sUser.ErrorMsg += "命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()!FORMID=" + sUser.FormID + " 异常信息:" + ex.ToString() + "\r\n";
                throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()" + ex.Message);
            }
        }
        /// <summary>
        ///调用业务系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="strEntityType"></param>
        /// <param name="strEntityKey"></param>
        /// <param name="ErroMessage"></param>
        private bool CallBusinessSystem(SubmitData submitData, T_WF_DOTASK entity, string strEntityType, string strEntityKey, ref string ErroMessage)
        {
            bool isOK = true;
            LogHelper.WriteLog("设置业务系统的审核状态是否需要审核 Config.IsNeedUpdateAudit=" + Config.IsNeedUpdateAudit.ToString());
            /****************更新业务系统单据审核状态***********************/
            if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
            {
                if (!string.IsNullOrEmpty(entity.SYSTEMCODE) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                {
                    LogHelper.WriteLog("UpdateAuditStatus 开始 更新业务系统单据审核状态 FormID=" + entity.ORDERID);
                    bool bol = UpdateAuditStatus(submitData,entity.SYSTEMCODE, strEntityType, strEntityKey, entity.ORDERID, entity.ORDERSTATUS.ToString(), ref ErroMessage);
                    LogHelper.WriteLog("UpdateAuditStatus 结束 更新业务系统单据审核状态 FormID=" + entity.ORDERID);
                    if (!bol)
                    {
                        return bol;
                        //throw new Exception(ErroMessage);
                    }
                    ErroMessage = "";
                   
                }
                else
                {
                    isOK = false;
                   LogHelper.WriteLog("不能更新业务系统，因为entity.SYSTEMCODE="+entity.SYSTEMCODE+" ;strEntityType="+strEntityType+" ;strEntityKey="+strEntityKey+" ;FormID=" + entity.ORDERID);
                }
            }
            
            return isOK;
        }
        #region 检验流程 非默认消息 和 自动发起流程 共用的变量
        string strAppFieldValue;
        DataTable dtFlowTrigger;
        DataTable sourceTable;
        EnginFlowDAL dal;
        private FlowEngineService.EngineWcfGlobalFunctionClient FlowEngine = new FlowEngineService.EngineWcfGlobalFunctionClient();      
        #endregion
       
        private bool EngineExecute(OracleConnection con, T_WF_DOTASK Entity, string IsTask,ref FlowUser sUser, ref string ErroMessage)
        {
            bool result = false;
            dal = new EnginFlowDAL();
            strAppFieldValue = string.Empty;
            sUser.TrackingMessage+="开始将数据源字段转换成数据表 FORMID="+sUser.FormID+"r\n";
             sourceTable = FieldStringToDataTable(Entity.APPFIELDVALUE, ref strAppFieldValue);//将业务数据与流程数据转换成DataTable作为替换的源数据
            sUser.TrackingMessage += "结束将数据源字段转换成数据表 FORMID=" + sUser.FormID + "r\n";
            //通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容

            LogHelper.WriteLog("开始通过系统代号，模块代号，企业ID，流程状态查［消息规则］内容 FORMID=" + sUser.FormID + "");
            dtFlowTrigger = dal.FlowTriggerTable(con, Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString(), Entity.COMPANYID);
            LogHelper.WriteLog("结束通过系统代号，模块代号，企业ID，流程状态查［消息规则］内容 FORMID=" + sUser.FormID + "");
            if (dtFlowTrigger == null || dtFlowTrigger.Rows.Count == 0)
            {
                sUser.ErrorMsg += "没有找到［消息规则］内容 再次通过全局默认消息继续查找 FORMID=" + sUser.FormID + " 异常信息:" + ErroMessage + "\r\n";
                LogHelper.WriteLog("dtFlowTrigger=null或者dtFlowTrigger.Rows.Count == 0 　没有找到［消息规则］内容 再次通过全局默认消息继续查找　 FORMID=" + sUser.FormID + "");
                dtFlowTrigger = dal.FlowTriggerTable(con, Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString());
            }                
            if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
            {
                #region
                DataRow[] drs = dtFlowTrigger.Select("ISDEFAULTMSG=1");//发的是默认流程触发条件
                if (drs.Count() > 0)
                {
                    sUser.ErrorMsg += "发现默认流程触发［消息规则］内容 FORMID=" + sUser.FormID + " IsTask:" + IsTask.ToString() + "\r\n";
                    LogHelper.WriteLog("FORMID=" + sUser.FormID + " 发现默认流程触发消息规则 IsTask=" + IsTask.ToString());
                    if (IsTask == "1")//新增待办任务
                    {                      
                        dal.AddDoTask(con, Entity, drs[0], sourceTable, strAppFieldValue);//新增待办任务
                        FlowEngine.TaskCacheReflesh(Entity.RECEIVEUSERID);
                    }
                    else if (IsTask == "0")//消息
                    {
                        dal.AddDoTaskMessage(con, Entity, drs[0], sourceTable);
                    }
                }
                else
                {
                    sUser.ErrorMsg += "没有发现默认流程触发（审核通过、审核不通过、审核中）［消息规则］内容 FORMID=" + sUser.FormID + " IsTask:" + IsTask.ToString() + "\r\n";
                    LogHelper.WriteLog("FORMID=" + sUser.FormID + " 没有发现默认流程触发（审核通过、审核不通过、审核中）消息规则 ");
                    //throw new Exception("该单据所对应的默认[消息规则]设置未找到,请先配置该模块 " + sUser.ModelName + " 默认消息规则(审核通过、审核不通过、审核中)");
                }
                DataRow[] NotDefaultMsg = dtFlowTrigger.Select("ISDEFAULTMSG=0");//非默认消息，需要调用的WCF待办任务
                if (NotDefaultMsg != null && NotDefaultMsg.Count() > 0)
                {
                    //AutoCallFlow( con, dal,Entity,dtFlowTrigger,sourceTable, ref  sUser, strAppFieldValue, ref ErroMessage);
                    #region 非默认消息时，自动发起流程
                    //foreach (DataRow dr1 in NotDefaultMsg)
                    //{
                    //    string strAppMsg = string.Empty;
                    //    CallWCFService(con, dr1, sourceTable, ref sUser, ref strAppMsg, ref  ErroMessage);//调用WCF服务
                    //    if (!string.IsNullOrEmpty(strAppMsg))
                    //    {
                    //        try
                    //        {
                    //            string IsNewFlow = "1";
                    //            string NewFormID = string.Empty;
                    //            string strFormTypes = string.Empty;//表单状态
                    //            DataRow DRNewTrigger = null;
                    //            if (ApplicationValueToDataTable(con, strAppMsg, string.Concat(Entity.COMPANYID), ref sourceTable, ref IsNewFlow, ref NewFormID, ref strFormTypes, ref DRNewTrigger))
                    //            {
                    //                //通过岗位查找用户，并且取第一个用户为发送消息的对像
                                   
                    //                PersonnelServiceClient HRClient = new PersonnelServiceClient();
                    //                if (!string.IsNullOrEmpty(dr1["OWNERPOSTID"].ToString()))
                    //                {
                    //                    string[] Employees = HRClient.GetEmployeeIDsByPostID(dr1["OWNERPOSTID"].ToString());
                    //                    if (Employees != null && Employees.Count() > 0)
                    //                    {
                    //                        dal.AddDoTask(con, Entity, dr1, sourceTable, Employees[0], NewFormID, strAppFieldValue, string.Concat(dr1["MESSAGEBODY"]), strFormTypes);//发送消息
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    string cMessage = "引擎调用新流程时没有选定岗位 SystemCode:" + string.Concat(DRNewTrigger["SYSTEMCODE"]) + " MODELCODE:" + string.Concat(DRNewTrigger["MODELCODE"]) + " NewFormID:" + NewFormID;
                    //                    ErroMessage = cMessage;
                                       
                    //                    throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL类方法：EngineExecute()引擎调用新流程时没有选定岗位");
                    //                }
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL类方法：EngineExecute() FORMID=" + sUser.FormID + "" + ex.Message);
                    //        }
                    //    }
                    //}
                    #endregion
                }
                result = true;
                #endregion
            }
            else
            {

                string strMsg =  "系统代号：" + Entity.SYSTEMCODE + "\r\n" +
                                 "模块代号：" + Entity.MODELCODE + "\r\n" +
                                 "触发条件：" + Entity.ORDERSTATUS + "\r\n" +
                                 "公司 ID：" + Entity.COMPANYID + "\r\n" +
                                 "单据 ID：" + Entity.ORDERID;
                ErroMessage = "FORMID=" + sUser.FormID + " 发送待办任务失败， 该单据所对应的［消息规则］设置未找到,请先配置公司[" + sUser.CompayName+ "("+sUser.CompayID+")] 下的模块[ " + sUser.ModelName + " ]的　［消息规则］(审核通过、审核不通过、审核中):\r\n" + strMsg;
                sUser.ErrorMsg += "发送待办任务失败 FORMID=" + sUser.FormID + " 异常信息:" + ErroMessage + "\r\n";
                LogHelper.WriteLog(ErroMessage);
                //暂是没有默认消息也可以审核通过
               // throw new Exception("该单据所对应的引擎系统设置未找到,请先配置该模块 " + sUser.ModelName + " 引擎消息(审核通过、审核不通过、审核中)");
            }
            return result;
        }
        /// <summary>
        /// 非默认消息时，自动发起流程
        /// </summary>
        private void AutoCallFlow(OracleConnection con, EnginFlowDAL dal, T_WF_DOTASK Entity, DataTable dtFlowTrigger, DataTable sourceTable, ref FlowUser sUser,string strAppFieldValue, ref string ErroMessage)
        {
            DataRow[] NotDefaultMsg = dtFlowTrigger.Select("ISDEFAULTMSG=0");//非默认消息，需要调用的WCF待办任务
            if (NotDefaultMsg != null && NotDefaultMsg.Count() > 0)
            {
                #region 非默认消息时，自动发起流程
                foreach (DataRow dr1 in NotDefaultMsg)
                {
                    string strAppMsg = string.Empty;
                    CallWCFService(con, dr1, sourceTable, ref sUser, ref strAppMsg, ref  ErroMessage);//调用WCF服务
                    if (!string.IsNullOrEmpty(strAppMsg))
                    {
                        #region 执行自动发起流程结果返回不为空
                        try
                        {                            
                            string IsNewFlow = "1";
                            string NewFormID = string.Empty;
                            string strFormTypes = string.Empty;//表单状态
                            DataRow DRNewTrigger = null;
                            if (ApplicationValueToDataTable(con, strAppMsg, string.Concat(Entity.COMPANYID), ref sourceTable, ref IsNewFlow, ref NewFormID, ref strFormTypes, ref DRNewTrigger))
                            {
                                //通过岗位查找用户，并且取第一个用户为发送消息的对像

                                PersonnelServiceClient HRClient = new PersonnelServiceClient();
                                if (!string.IsNullOrEmpty(dr1["OWNERPOSTID"].ToString()))
                                {
                                    if (DRNewTrigger != null)
                                    {
                                        LogHelper.WriteLog("自动发起流程 消息规则设置中 选定岗位=" + dr1["OWNERPOSTID"].ToString() + " ;消息=" + string.Concat(dr1["MESSAGEBODY"]) + "：MODELCODE=" + string.Concat(DRNewTrigger["MODELCODE"]) + ";AutoCallFlow() FORMID=" + sUser.FormID);
                                    }
                                    else
                                    {
                                        LogHelper.WriteLog("DRNewTrigger=null 自动发起流程的第三方消息规则中没有设置没有消息规则,当前的模块消息规则设置中 选定岗位=" + dr1["OWNERPOSTID"].ToString() + " ;消息=" + string.Concat(dr1["MESSAGEBODY"]) + "：MODELCODE=null;AutoCallFlow() FORMID=" + sUser.FormID);
                                    }
                                    string[] Employees = HRClient.GetEmployeeIDsByPostID(dr1["OWNERPOSTID"].ToString());
                                    if (Employees != null && Employees.Count() > 0)
                                    {
                                        Entity.SYSTEMCODE = dr1["SYSTEMCODE"].ToString();//解决傅意成遇到的问题（发起员工入职手续的待办，数据库中数据存在问题，系统代号是错误的RM代号，应是HR代号）
                                        dal.AddDoTask(con, Entity, dr1, sourceTable, Employees[0], NewFormID, strAppFieldValue, string.Concat(dr1["MESSAGEBODY"]), strFormTypes);//发送消息
                                        FlowEngine.TaskCacheReflesh(Employees[0]);
                                    }
                                }
                                else
                                {
                                    string cMessage = "";
                                    if (DRNewTrigger != null)
                                    {
                                        cMessage = "SystemCode:" + string.Concat(DRNewTrigger["SYSTEMCODE"]) + " MODELCODE:" + string.Concat(DRNewTrigger["MODELCODE"]) + " ;消息=" + string.Concat(dr1["MESSAGEBODY"]) + " NewFormID:" + NewFormID;
                                    }
                                    else
                                    {
                                        cMessage = "DRNewTrigger=null 消息=" + string.Concat(dr1["MESSAGEBODY"]) + " NewFormID:" + NewFormID;
                                    }
                                    //string cMessage = "SystemCode:" + string.Concat(DRNewTrigger["SYSTEMCODE"]) + " MODELCODE:" + string.Concat(DRNewTrigger["MODELCODE"]) + " ;消息=" + string.Concat(dr1["MESSAGEBODY"]) + " NewFormID:" + NewFormID;
                                    ErroMessage = cMessage;

                                    if (DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_oa_businesstrip" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_oa_businessreport" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_salaryrecordbatch" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_attendancesolutionasign" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_attendancesolution" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_employeesalaryrecord" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_employeeentry" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_leftoffice" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_oa_travelreimbursement" ||
                                        DRNewTrigger["MODELCODE"].ToString().ToLower() == "t_hr_leftofficeconfirm"
                                        )
                                    {//不需要接收岗位的:
                                        //出差申请	T_OA_BUSINESSTRIP
                                        //出差报告	T_OA_BUSINESSREPORT
                                        //月薪批量审核	T_HR_SALARYRECORDBATCH
                                        //考勤方案分配	T_HR_ATTENDANCESOLUTIONASIGN
                                        //考勤方案定义	T_HR_ATTENDANCESOLUTION
                                        //薪资记录	T_HR_EMPLOYEESALARYRECORD
                                        //员工入职	T_HR_EMPLOYEEENTRY                                       
                                        //员工离职	T_HR_LEFTOFFICE
                                        //离职确认	T_HR_LEFTOFFICECONFIRM
                                        //出差报销 T_OA_TRAVELREIMBURSEMENT
                                        LogHelper.WriteLog("自动发起流程 消息规则中非默认的设置中 不需要 选定岗位： AutoCallFlow() FORMID=" + sUser.FormID + "   " + cMessage);
                                    }
                                    else
                                    {
                                        LogHelper.WriteLog("自动发起流程(失败) 消息规则中非默认的设置中没有选定岗位：AutoCallFlow() FORMID=" + sUser.FormID + "   " + cMessage);
                                        throw new Exception("自动发起流程(失败) 消息规则中非默认的设置中没有选定岗位 \r\n FORMID=" + sUser.FormID);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WriteLog("自动发起流程(失败)：消息规则中非默认的设置中检查选定岗位出错 消息=" + string.Concat(dr1["MESSAGEBODY"]) + " AutoCallFlow() FORMID=" + sUser.FormID + " 异常信息：\r\n" + ex.Message);
                            throw new Exception("自动发起流程(失败)：消息规则中非默认的设置中检查选定岗位出错 \r\n FORMID=" + sUser.FormID);
                        }
                        #endregion
                    }
                    else
                    {
                        LogHelper.WriteLog("自动发起流程成功,但返回的结果为空 消息规则设置中 ;选定岗位=" + dr1["OWNERPOSTID"].ToString() + " ;消息=" + string.Concat(dr1["MESSAGEBODY"]) + ";MODELCODE=" + string.Concat(dr1["MODELCODE"]) + "：AutoCallFlow() FORMID=" + sUser.FormID); 
                    }
                }
                #endregion
            }
 
        }
        /// <summary>
        /// 调用WCF 自动发起流程
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="sourceTable"></param>
        /// <param name="strAppMsg"></param>
        private void CallWCFService(OracleConnection con, DataRow dr, DataTable sourceTable, ref FlowUser sUser,ref string strAppMsg, ref string ErroMessage)
        {

            try
            {
                string tableName =dr["MODELCODE"]!=null? dr["MODELCODE"].ToString():"";
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
                FuncPameter = ReplaceValue(FuncPameter, sourceTable);
                string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
                                    "<Paras>" + "\r\n" +
                                    "<Para TableName=\"" + tableName + "\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"" + strOwnerID + "\"></Para>" +
                                    "<Para TableName=\"" + tableName + "\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"" + strDeptID + "\"></Para>" +
                                    "<Para TableName=\"" + tableName + "\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"" + strPostID + "\"></Para>" +
                                    "<Para TableName=\"" + tableName + "\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"" + strCorpID + "\"></Para>" +
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
                    LogHelper.WriteLog("引擎调用WCF记录（自动发起流程） 成功  FORMID=" + sUser.FormID + "" + cMessage);

                }
                else
                {
                       string cMessage = "执行链接：" + WCFUrl + "\r\n" +
                                      "执行方法:" + FuncName + "\r\n" +
                                      "绑定契约：" + Binding + "\r\n" +
                                      "执行结果：" + strAppMsg + "\r\n" +
                                      "执行参数：" + FuncPameter + "\r\n" +
                                      "----------------------------------------------------------\r\n";
                       sUser.ErrorMsg += "引擎调用WCF记录（自动发起流程）失败 FORMID=" + sUser.FormID + " 异常信息:" + ErroMessage + "\r\n";
                       ErroMessage = "FORMID=" + sUser.FormID + " 命名空间:SMT.FlowWFService类方法：CallWCFService() 调用WCF参数不全 \r\n" + cMessage;
                       LogHelper.WriteLog("引擎调用WCF记录（自动发起流程）失败　暂时不抛出异常，只记录日志,异常信息：" + ErroMessage);
                    //throw new Exception("命名空间:SMT.FlowWFService类方法：CallWCFService() 调用WCF参数不全 WCF协议：" + Binding + "WCF地址：" + WCFUrl + "WCF方法：" + FuncName + "");//暂时不抛出异常，只记录日志
                }
            }
            catch (Exception e)
            {
                sUser.ErrorMsg += "调用WCF 自动发起流程 FORMID=" + sUser.FormID + " 异常信息:" + e.Message + "\r\n";
                throw new Exception("FORMID=" + sUser.FormID + " 命名空间:SMT.FlowWFService类方法：CallWCFService()" + e.Message);
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
        private bool ApplicationValueToDataTable(OracleConnection con, string strAppMsg, string strCompanyID, ref DataTable sourceTable, ref string IsNewFlow, ref string NewFormID, ref string strFormTypes, ref DataRow DRFlowTrigger)
        {
            try
            {
                EnginFlowDAL dal = new EnginFlowDAL();
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
                        DataTable table = dal.FlowTriggerTable(con, SystemCode, ModelCode, null, strCompanyID);
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
        /// 任务完成后，消息关闭
        /// </summary>
        /// <param name="strSystemCode">系统代码</param>
        /// <param name="strFormID">单据ID</param>
        /// <param name="strReceiveID">接收人</param>
        public void TaskMsgClose(OracleConnection con, string strSystemCode, string strFormID, string strReceiveID)
        {
            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID) && !string.IsNullOrEmpty(strReceiveID))
            {
                try
                {
                    EnginFlowDAL dal = new EnginFlowDAL();
                    if (strReceiveID.IndexOf(',') != -1)
                    {
                        string[] users = strReceiveID.Split(',');
                        foreach (string user in users)
                        {
                            //刷新缓存用户是否有新的待办 暂时没有写入缓存                        
                            dal.CloseDoTaskStatus(con, strSystemCode, strFormID, user);
                            FlowEngine.TaskCacheReflesh(user);
                        }
                    }
                    else
                    {
                        //刷新缓存用户是否有新的待办                                   
                        dal.CloseDoTaskStatus(con, strSystemCode, strFormID, strReceiveID);//关闭待办
                        FlowEngine.TaskCacheReflesh(strReceiveID);
                    }
                }
                catch (Exception e)
                {

                    string cMessage = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + strReceiveID + "',Error='" + e.Message + "'";
                    LogHelper.WriteLog("TaskMsgClose" + cMessage);
                }
            }
        }


        /// <summary>
        /// 流程待办任务撤销
        /// </summary>
        /// <param name="strFlowXml"></param>
        /// <param name="strAppXml"></param>      
        /// <returns></returns>
        public bool FlowCancel(OracleConnection con, SubmitData submitData,string strFlowXml, string strAppXml, ref string ErroMessage)
        {
            if (string.IsNullOrEmpty(strFlowXml) && string.IsNullOrEmpty(strAppXml))
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
                EnginFlowDAL dal = new EnginFlowDAL();
                /*解析流程和业务数据XML*/
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strFlowXml);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                strCompanyID = XMLToAttribute(xele, "COMPANYID");
                strSystemCode = string.Empty;
                strModelCode = XMLToAttribute(xele, "MODELCODE");
                strFormID = XMLToAttribute(xele, "FORMID");
                strModelName = XMLToAttribute(xele, "ModelName");
                strCheckState = XMLToAttribute(xele, "CheckState");
                strReceiveID = XMLToAttribute(xele, "APPUSERID");
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
                        if (CheckModelName(strModelCode) && strCheckState == "2")
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

                string strAppFieldValue = ConbinString(xele) + BOObjectEscapeString(strAppXml);
                DataTable dtValue = FieldStringToDataTable(strAppFieldValue, ref strAppFieldValue);
                string content = dal.GetMessageDefine(con, "FLOWCANCEL");
                string strUrl = string.Empty;
                ReplaceUrl(ref content, ref strUrl, dtValue);               
                dal.DoTaskCancel(con, strSystemCode, strModelCode, strFormID, strReceiveID, content);
                FlowEngine.TaskCacheReflesh(strReceiveID);
                if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
                {
                    if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                    {
                        bool bol = UpdateAuditStatus(submitData,strSystemCode, strEntityType, strEntityKey, strFormID, strCheckState, ref ErroMessage);
                        if (!bol)
                        {
                            throw new Exception(ErroMessage);
                        }
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
                                   "流程XML:" + strFlowXml + "\r\n" +
                                  "AppXml:" + strAppXml + "\r\n";               
                LogHelper.WriteLog("FlowCancel流程待办任务撤销异常" + cMessage + ErroMessage);
                return false;
            }

        }

        /// <summary>
        ///  流程咨讯 
        /// </summary>
        /// <param name="UserAndForm"></param>
        /// <param name="strTitle"></param>
        /// <param name="strFlowXML"></param>
        /// <param name="strAppXml"></param>  
        /// <returns></returns>
        public bool FlowConsultati(OracleConnection con, List<CustomUserMsg> UserAndForm, string strTitle, string strFlowXML, string strAppXml)
        {
            string strCompanyID = string.Empty;//公司ID 
            string strSystemCode = string.Empty;//系统代号
            string strModelCode = string.Empty;//模块代号
            string strFormID = string.Empty;//FORMID
            string strModelName = string.Empty;//模块名称
            if (UserAndForm.Count > 0)
            {
                try
                {
                    EnginFlowDAL dal = new EnginFlowDAL();
                    /*解析流程和业务数据XML*/
                    Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strFlowXML);
                    XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                    strCompanyID = XMLToAttribute(xele, "COMPANYID");
                    strSystemCode = string.Empty;
                    strModelCode = XMLToAttribute(xele, "MODELCODE");
                    strFormID = XMLToAttribute(xele, "FORMID");
                    strModelName = XMLToAttribute(xele, "ModelName");
                    if (!string.IsNullOrEmpty(strAppXml))
                    {
                        Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(strAppXml);
                        XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
                        strSystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
                    }

                    /*在T_WF_MESSAGEDEFINE 中查找消息内容*/
                    string content = dal.GetMessageDefine(con, "FLOWCONSULTATI");
                    string strAppFieldValue = ConbinString(xele) + BOObjectEscapeString(strAppXml);
                    DataTable dtValue = FieldStringToDataTable(strAppFieldValue, ref strAppFieldValue);

                    /*在消息触发规则定义中查找消息链接*/
                    DataTable dtFlowTrigger = dal.FlowTriggerTable(con, strSystemCode, strModelCode, string.Empty, strCompanyID);
                    if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
                    {
                        string Url = dtFlowTrigger.Rows[0]["applicationurl"].ToString();
                        ReplaceUrl(ref content, ref Url, dtValue);
                        string strContent = string.Empty;
                        foreach (CustomUserMsg custom in UserAndForm)
                        {
                            if (!string.IsNullOrEmpty(strTitle))
                            {
                                content = strTitle;
                            }
                            //刷新缓存用户是否有新的待办
                            //EngineCache.TaskCache.TaskCacheReflesh(custom.UserID);                        
                            dal.FlowConsultatiMsg(con, custom.UserID, strSystemCode, strModelCode, custom.FormID, content, Url, BOObjectEscapeString(strAppXml), strFlowXML, strAppXml);
                            FlowEngine.TaskCacheReflesh(custom.UserID);
                        }
                        return true;
                    }
                    else
                    {
                        //未能在触发规则定义中找到一条数据，无法发送咨讯
                        string cMessage = "发送咨讯失败，未能在触发规则定义（审核通过、审核不通过、审核中）中找到一条数据，无法发送咨讯" + "\r\n" +
                                         "SystemCode:" + strSystemCode + "\r\n" +
                                         "Content:" + content + "\r\n" +
                                         "MODELCODE:" + strModelCode + "\r\n" +
                                         "COMPANYID:" + strCompanyID + "\r\n" +
                                         "FORMID:" + strFormID + "\r\n";
                        LogHelper.WriteLog("FlowConsultati()发送咨讯失败：" + cMessage);
                        return false;
                    }

                }
                catch (Exception e)
                {
                    string cMessage = "发送咨讯出现异常：" + "\r\n" +
                                      "SystemCode:" + strSystemCode + "\r\n" +
                                      "MODELCODE:" + strModelCode + "\r\n" +
                                      "COMPANYID:" + strCompanyID + "\r\n" +
                                      "Error:" + e.Message + "\r\n" +
                                      "FORMID:" + strFormID + "\r\n";

                    LogHelper.WriteLog(cMessage);
                    return false;
                }

            }
            else
            {
                string cMessage = "发送咨讯出现错误（没有指定接收人员）" + "\r\n" +
                                     "SystemCode:" + strSystemCode + "\r\n" +
                                     "MODELCODE:" + strModelCode + "\r\n" +
                                     "COMPANYID:" + strCompanyID + "\r\n" +
                                     "FORMID:" + strFormID + "\r\n";
                LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
                return false;
            }
        }

        /// <summary>
        /// 流程咨讯关闭
        /// </summary>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <param name="strReceiveID"></param>      
        public void FlowConsultatiClose(OracleConnection con, string strSystemCode, string strFormID, string strReceiveID)
        {
            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID) && !string.IsNullOrEmpty(strReceiveID))
            {
                try
                {
                    EnginFlowDAL dal = new EnginFlowDAL();
                    if (strReceiveID.IndexOf(',') != -1)
                    {
                        string[] users = strReceiveID.Split(',');
                        foreach (string user in users)
                        {
                            //刷新缓存用户是否有新的待办
                            //EngineCache.TaskCache.TaskCacheReflesh(user);
                            //SMTEngineBLL.CloseMsg(strSystemCode, strFormID, user);                           
                            dal.CloseConsultati(con, strSystemCode, strFormID, user);
                            FlowEngine.TaskCacheReflesh(user);
                        }
                    }
                    //SMTEngineBLL.CloseMsg(strSystemCode, strFormID, strReceiveUser);
                    dal.CloseConsultati(con, strSystemCode, strFormID, strReceiveID);
                }
                catch (Exception e)
                {

                    string cMessage = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + strReceiveID + "',Error='" + e.Message + "'";
                    LogHelper.WriteLog("TaskMsgClose" + cMessage);
                }
            }
        }
        /// <summary>
        /// 审核结束时关闭所有的流程资讯在待办中
        /// </summary>
        /// <param name="con"></param>
        /// <param name="strSystemCode"></param>
        /// <param name="strFormID"></param>
        /// <returns></returns>
        public bool FlowConsultatiCloseAll(OracleConnection con, string strSystemCode, string strFormID)
        {
            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID))
            {
                try
                {
                    EnginFlowDAL dal = new EnginFlowDAL();
                    return dal.CloseAllConsultati(con, strSystemCode, strFormID);

                }
                catch (Exception e)
                {

                    string cMessage = "CloseAllConsultati出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "'Error='" + e.Message + "'";
                    LogHelper.WriteLog("CloseAllConsultati" + cMessage);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion     


        #region 方法




        /// <summary>
        /// 元数据替换 DataValue 适用于替换 Guid
        /// </summary>
        /// <param name="PorcessString"></param>
        /// <param name="SourceValueDT"></param>
        /// <returns></returns>
        public static string ReplaceValue(string PorcessString, DataTable SourceValueDT)
        {
            foreach (DataRow dr in SourceValueDT.Rows)
            {
                PorcessString = PorcessString.Replace("{" + dr["ColumnName"].ToString() + "}", dr["ColumnValue"].ToString());
            }
            return PorcessString;
        }
        /// <summary>
        /// 特殊模块需要改变接收人
        /// </summary>
        /// <returns></returns>
        private bool CheckModelName(string strModelName)
        {
            if (strModelName == "T_HR_EMPLOYEECHECK" ||
                strModelName == "T_HR_EMPLOYEECONTRACT" ||
                strModelName == "T_HR_EMPLOYEEENTRY" ||
                strModelName == "T_HR_EMPLOYEEINSURANCE" ||
                strModelName == "T_HR_EMPLOYEEPOSTCHANGE" ||
                strModelName == "T_HR_LEFTOFFICECONFIRM" ||
                strModelName == "T_HR_LEFTOFFICE" ||
                 strModelName == "T_HR_PENSIONMASTER" ||
                 strModelName == "T_HR_SALARYARCHIVE")
            {
                return true;
            }
            return false;
        }
        static int iUpdate = 0;
        public bool UpdateAuditStatus(SubmitData submitData,string strSystemCode, string EntityType, string EntityKey, string EntityId, string strCheckState, ref string errorMsg)
        {
            bool bol = true;
            iUpdate = iUpdate + 1;
            string strMsg = "第 "+iUpdate+" 次更新业务系统"
                               +"系统代号：" + strSystemCode + "\r\n"
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
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<Params>");
                sb.AppendLine("	<AuditUserID>" + submitData.ApprovalUser.UserID+ "</AuditUserID>");
                sb.AppendLine("	<AuditUserName>" + submitData.ApprovalUser.UserName + "</AuditUserName>");
                sb.AppendLine("</Params>");
                LogHelper.WriteLog(strMsg + ";\r\n 回调给业务系统的参数：" + sb.ToString());


                bool bResult = Utility.UpdateFormCheckState(strSystemCode, EntityType, EntityKey, EntityId, CheckState, ref serviceErrorMsg, sb.ToString());
                if (!bResult)
                {
                    LogHelper.WriteLog("更新审核状态失败\r\n"+strMsg);
                    bol = false;

                    if (string.IsNullOrEmpty(serviceErrorMsg.Trim()))
                    {
                        errorMsg = "业务系统更新出错，请联系管理员！\r\n FormID=" + EntityId;
                    }
                    else
                    {
                        errorMsg = serviceErrorMsg;
                    }
                    LogHelper.WriteLog("bResult=" + bResult.ToString() + ";FormID=" + EntityId + " Utility.UpdateFormCheckState发生异常：" + strMsg + "；业务系统异常信息：" + serviceErrorMsg);
                    return bol;
                   
                }
                #endregion

                //bool bResult = SMT.SaaS.BLLCommonServices.Utility.UpdateFormCheckState(strSystemCode, EntityType, EntityKey, EntityId, CheckState);
                //if (!bResult)
                //{
                //    LogHelper.WriteLog("更新审核状态失败\r\n" + strMsg);
                //    LogHelper.WriteLog("更新审核状态失败" + strMsg);
                //}
                else
                {

                    LogHelper.WriteLog("bResult=" + bResult.ToString() + ";FormID=" + EntityId + " 更新审核状态成功\r\n" + strMsg);                 
                    return bol;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("FormID=" + EntityId +" 更新审核状态出现异常" + strMsg + "UpdateAuditStatus" + ex);
                return false;
            }
        }

        private void ReplaceUrl(ref string strContent, ref string strUrl, DataTable dtValue)
        {
            if (dtValue != null && dtValue.Rows.Count > 0)
            {
                foreach (DataRow dr in dtValue.Rows)
                {
                    strContent = strContent.Replace(string.Concat("{" + dr["ColumnName"].ToString().ToUpper() + "}"), string.Concat(dr["ColumnText"]));
                    strUrl = strUrl.Replace("{" + string.Concat(dr["ColumnName"].ToString().ToUpper()) + "}", string.Concat(dr["ColumnValue"]));
                }
                string replaceUrl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                              "<System>" +
                              "{0}" +
                              "</System>";
                strUrl = string.Format(replaceUrl, strUrl);
            }
        }
        /// <summary>
        /// 将数据源字段转换成数据表
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private DataTable FieldStringToDataTable(string strAppValue, ref string TmpFieldValueString)
        {
            string temp = "";
            DataRow[] list;
            DataRow drvalue;
            DataTable valueTable = new DataTable();
            valueTable.Columns.Add("FieldType", typeof(string));
            valueTable.Columns.Add("ColumnName", typeof(string));
            valueTable.Columns.Add("ColumnValue", typeof(string));
            valueTable.Columns.Add("ColumnText", typeof(string));
            TmpFieldValueString = strAppValue;
            if (!string.IsNullOrEmpty(TmpFieldValueString))
            {
                string[] valuerownode = TmpFieldValueString.Split('Ё');
                for (int j = 0; j < valuerownode.Length; j++)
                {
                    if (valuerownode[j] != "")
                    {
                        string[] valuecolnode = valuerownode[j].Split('|');
                        list = valueTable.Select("ColumnName='" + valuecolnode[0] + "'");
                        if (list.Length > 0)
                        {
                            drvalue = list[0];
                        }
                        else
                        {
                            drvalue = valueTable.NewRow();
                            valueTable.Rows.Add(drvalue);
                        }
                        drvalue["FieldType"] = "sys";
                        drvalue["ColumnName"] = valuecolnode[0];
                        string strValueText = valuecolnode[1];
                        string strValue = string.Empty;
                        string strText = string.Empty;
                        if (strValueText.IndexOf("Г") != -1)
                        {
                            strValue = strValueText.Split('Г')[0];
                            strText = strValueText.Split('Г')[1];
                        }
                        else
                        {
                            strValue = strValueText;
                        }
                        if (string.IsNullOrEmpty(strText))
                        {
                            strText = strValue;
                        }
                        drvalue["ColumnValue"] = (string.IsNullOrEmpty(drvalue["ColumnValue"].ToString())) ? strValue : drvalue["ColumnValue"].ToString();
                        drvalue["ColumnText"] = strText;
                        
                    }
                }
            }
            for (int i = 0; i < valueTable.Rows.Count; i++)
            {
                temp += i.ToString() + "|" + valueTable.Rows[i]["ColumnName"].ToString() + "=" + valueTable.Rows[i]["ColumnValue"].ToString()+"\r\n"; 
            }
            LogHelper.WriteLog("将数据源字段转换成数据表FieldStringToDataTable\r\n" + temp);
            return valueTable;
        }
        /// <summary>
        /// 将业务数据字段，组合成以Ё分割的字符串
        /// </summary>
        /// <param name="xe"></param>
        /// <returns></returns>
        private string ConbinString(XElement xe)
        {
            StringBuilder sb = new StringBuilder();
            var v = from item in xe.Descendants("Message").Descendants("Attribute")
                    select item;
            if (v.Count() > 0)
            {
                foreach (var vv in v)
                {
                    string Name = vv.Attribute("Name").Value;
                    string Value = vv.Attribute("DataValue").Value;
                    sb.Append(Name + "|" + Value + "Ё");
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 根据节点属性得到值（公共方法）
        /// </summary>
        /// <param name="xelement">XML节点</param>
        /// <param name="strAttrName">属性</param>
        /// <returns></returns>
        private string XMLToAttribute(XElement xelement, string strAttrName)
        {
            try
            {
                string strReturn = (from item in xelement.Descendants("Message").Descendants("Attribute")
                                    where item.Attribute("Name").Value.ToUpper() == strAttrName.ToUpper()
                                    select item).FirstOrDefault().Attribute("DataValue").Value;
                return strReturn;
            }
            catch
            {
                return string.Empty;
            }
        }
        private string BOObjectEscapeString(string strBOObject)
        {
            if (!string.IsNullOrEmpty(strBOObject))
            {
                StringBuilder sb = new StringBuilder();
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strBOObject);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var Object = (from item in xele.Descendants("Object")
                              select item).FirstOrDefault();
                var SysteName = (from item in xele.Descendants("Name")
                                 select item).FirstOrDefault().Value;
                string ObjectName = Object.Attribute("Name").Value;
                string ObjectDesc = Object.Attribute("Description").Value;
                var attributes = from c in xele.Descendants("Attribute")
                                 select c;
                if (attributes.Count() > 0)
                {
                    var list = attributes.ToList();
                    if (list.Count == 1)
                    {
                        // "Г"
                        var attr = list[0];
                        string name = attr.Attribute("Name").Value;
                        string desc = attr.Attribute("Description").Value;
                        string datatype = attr.Attribute("DataType").Value;
                        string datavalue = attr.Attribute("DataValue").Value;
                        string dataText = datavalue;
                        try
                        {
                            dataText = attr.Attribute("DataText").Value;
                        }
                        catch
                        {

                        }
                        sb.Append(name + "|" + datavalue + "Г" + dataText + "Ё");
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        var attr = list[i];
                        string name = attr.Attribute("Name").Value;
                        string desc = attr.Attribute("Description").Value;
                        string datatype = attr.Attribute("DataType").Value;
                        string datavalue = attr.Attribute("DataValue").Value;
                        string dataText = datavalue;
                        try
                        {
                            dataText = attr.Attribute("DataText").Value;
                        }
                        catch
                        {

                        }
                        if (i == list.Count - 1)
                        {

                            sb.Append(name + "|" + datavalue + "Г" + dataText);
                        }
                        else
                        {
                            sb.Append(name + "|" + datavalue + "Г" + dataText + "Ё");
                        }
                    }

                }
                return sb.ToString();
            }
            return string.Empty;
        }
        #endregion
    }
}
