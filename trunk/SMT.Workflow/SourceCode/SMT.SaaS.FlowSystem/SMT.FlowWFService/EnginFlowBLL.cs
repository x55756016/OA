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
namespace SMT.FlowWFService
{
    public class EnginFlowBLL
    {
        #region 龙康才新增
        /// <summary>
        /// 流程触发
        /// </summary>
        /// <param name="strFlow">流程数据</param>
        /// <param name="strBusiness">业务数据</param>    
        /// <returns></returns>
        public bool SaveFlowTriggerData(OracleConnection con, string strFlow, string strBusiness, ref string ErroMessage)
        {
            
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
                    LogHelper.WriteLog("开始消息解析");
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
                    #region 原有旧代码

                    //Byte[] bFlow = System.Text.UTF8Encoding.UTF8.GetBytes(strFlow);
                    //XElement xeFlow = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(bFlow)));
                    //entity.COMPANYID = XMLToAttribute(xeFlow, "COMPANYID");
                    //entity.MODELCODE = XMLToAttribute(xeFlow, "MODELCODE");
                    //entity.MODELNAME = XMLToAttribute(xeFlow, "ModelName");
                    //entity.ORDERID = XMLToAttribute(xeFlow, "FORMID");
                    //entity.ORDERSTATUS = int.Parse(XMLToAttribute(xeFlow, "CheckState"));
                    //entity.RECEIVEUSERID = XMLToAttribute(xeFlow, "APPUSERID");
                    //entity.FLOWXML = strFlow;
                    //entity.APPXML = strBusiness;
                    //IsTask = XMLToAttribute(xeFlow, "IsTask");
                    //if (!string.IsNullOrEmpty(XMLToAttribute(xeFlow, "OutTime")))
                    //{
                    //    entity.BEFOREPROCESSDATE = DateTime.Now.AddMinutes(int.Parse(XMLToAttribute(xeFlow, "OutTime")));
                    //}
                    //if (!string.IsNullOrEmpty(strBusiness))
                    //{
                    //    Byte[] BBusiness = System.Text.UTF8Encoding.UTF8.GetBytes(strBusiness);
                    //    XElement xeBusiness = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(BBusiness)));
                    //    entity.SYSTEMCODE = (from item in xeBusiness.Descendants("Name") select item).FirstOrDefault().Value;
                    //    try
                    //    { //非手机的XML时没有表明和主键的
                    //        strEntityType = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Name").Value;
                    //        strEntityKey = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Key").Value;
                    //    }
                    //    catch
                    //    {
                    //        LogHelper.WriteLog("异常无法取得业务数据表，单据:" + entity.ORDERID + "：表：" + strEntityType + ":主键：" + strEntityKey + "");
                    //    }
                    //    entity.ORDERUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
                    //                          where item.Attribute("Name").Value.ToUpper() == "OWNERID"
                    //                          select item).FirstOrDefault().Attribute("DataValue").Value;
                    //    entity.ORDERUSERNAME = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
                    //                            where item.Attribute("Name").Value.ToUpper() == "OWNERNAME"
                    //                            select item).FirstOrDefault().Attribute("DataValue").Value;
                    //    //有些特殊的模块需要改变接收人
                    //    if (CheckModelName(entity.MODELCODE) && entity.ORDERSTATUS == 2)
                    //    {
                    //        entity.RECEIVEUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
                    //                                where item.Attribute("Name").Value.ToUpper() == "CREATEUSERID"
                    //                                select item).FirstOrDefault().Attribute("DataValue").Value;
                    //    }
                    //    entity.APPFIELDVALUE = ConbinString(xeFlow) + BOObjectEscapeString(strBusiness);
                    //}
                    #endregion
                    LogHelper.WriteLog("结束消息解析");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()解析XML出错" + ex.Message);

                    throw new Exception(ErroMessage);                   
                }
                if (string.IsNullOrEmpty(entity.SYSTEMCODE))
                {
                    entity.SYSTEMCODE = SystemCode;
                }
                bool IsExecute = EngineExecute(con, entity, IsTask, ref  ErroMessage);
                LogHelper.WriteLog("执行EngineExecute(" + con + ", " + entity + ", " + IsTask + ", ref  " + ErroMessage + ")返回值IsExecute＝" + IsExecute.ToString());
                if (IsExecute)
                {
                    try
                    {
                        LogHelper.WriteLog("开始更新业务系统单据审核状态Config.IsNeedUpdateAudit=" + Config.IsNeedUpdateAudit.ToString());
                        /****************更新业务系统单据审核状态***********************/
                        if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
                        {
                            if (!string.IsNullOrEmpty(entity.SYSTEMCODE) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                            {
                               LogHelper.WriteLog("UpdateAuditStatus开始更新...FORMID=" + entity.ORDERID + "");
                               bool bol= UpdateAuditStatus(entity.SYSTEMCODE, strEntityType, strEntityKey, entity.ORDERID, entity.ORDERSTATUS.ToString(),ref ErroMessage);
                               LogHelper.WriteLog("UpdateAuditStatus结束更新...FORMID=" + entity.ORDERID + "");
                                if (!bol)
                               {
                                   throw new Exception(ErroMessage); //抛出异常终止执行流程 
                                  
                               }
                            }
                        }
                        LogHelper.WriteLog("结束更新业务系统单据审核状态");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()更新审核状态出现异常" + ex.Message);

                        throw new Exception(ErroMessage); 
                    }
                }
                //LogHelper.WriteLog("流程触发存储记录SaveFlowTriggerDataFlowXML:" + strFlow + "\r\n" + "AppXml:" + strBusiness + "\r\n");
                return IsExecute;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("命名空间:SMT.FlowWFService.EnginFlowBLL 类方法SaveFlowTriggerData()" + ex.Message);
                throw new Exception(ErroMessage); //抛出异常终止执行流程                
                
            }
        }

        private bool EngineExecute(OracleConnection con, T_WF_DOTASK Entity, string IsTask, ref string ErroMessage)
        {
            bool result = false;
            EnginFlowDAL dal = new EnginFlowDAL();
            string strAppFieldValue = string.Empty;
            LogHelper.WriteLog("开始将数据源字段转换成数据表....");
            DataTable sourceTable = FieldStringToDataTable(Entity.APPFIELDVALUE, ref strAppFieldValue);//将业务数据与流程数据转换成DataTable作为替换的源数据
            LogHelper.WriteLog("结束将数据源字段转换成数据表");
            //通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容

            LogHelper.WriteLog("开始通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容....");
            DataTable dtFlowTrigger = dal.FlowTriggerTable(con, Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString(), Entity.COMPANYID);
            LogHelper.WriteLog("结束通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容");
            if (dtFlowTrigger == null || dtFlowTrigger.Rows.Count == 0)
            {

                dtFlowTrigger = dal.FlowTriggerTable(con, Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString());
            }          
            LogHelper.WriteLog("数量：" + dtFlowTrigger.Rows.Count.ToString());
           
            if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
            {
                #region
                
                DataRow[] drs = dtFlowTrigger.Select("ISDEFAULTMSG=1");//发的是默认流程触发条件
                if (drs.Count() > 0)
                {
                    ErroMessage+="开始-发的是默认流程触发条件\r\n";
                    if (IsTask == "1")//新增待办任务
                    {
                        ErroMessage += "开始-新增待办任务\r\n";
                        dal.AddDoTask(con, Entity, drs[0], sourceTable, strAppFieldValue);//新增待办任务
                        ErroMessage += "结束-新增待办任务\r\n";
                    }
                    else if (IsTask == "0")//消息
                    {
                        ErroMessage += "开始-新增消息\r\n";
                        dal.AddDoTaskMessage(con, Entity, drs[0], sourceTable);
                        ErroMessage += "结束-新增消息\r\n";
                    }
                    ErroMessage += "结束-发的是默认流程触发条件\r\n";
                }
                
                DataRow[] NotDefaultMsg = dtFlowTrigger.Select("ISDEFAULTMSG=0");//非默认消息，需要调用的WCF待办任务
                if (NotDefaultMsg != null && NotDefaultMsg.Count() > 0)
                {
                    foreach (DataRow dr1 in NotDefaultMsg)
                    {
                        string strAppMsg = string.Empty;
                        ErroMessage += "开始-调用WCF服务\r\n";
                        CallWCFService(con, dr1, sourceTable, ref strAppMsg, ref  ErroMessage);//调用WCF服务
                        ErroMessage += "结束-调用WCF服务\r\n";
                        if (!string.IsNullOrEmpty(strAppMsg))
                        {
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
                                        string[] Employees = HRClient.GetEmployeeIDsByPostID(dr1["OWNERPOSTID"].ToString());
                                        if (Employees != null && Employees.Count() > 0)
                                        {
                                            dal.AddDoTask(con, Entity, dr1, sourceTable, Employees[0], NewFormID, strAppFieldValue, string.Concat(dr1["MESSAGEBODY"]), strFormTypes);//发送消息
                                        }
                                    }
                                    else
                                    {
                                        string cMessage = "引擎调用新流程时没有选定岗位 SystemCode:" + string.Concat(DRNewTrigger["SYSTEMCODE"]) + " MODELCODE:" + string.Concat(DRNewTrigger["MODELCODE"]) + " NewFormID:" + NewFormID;
                                        ErroMessage += cMessage;
                                        throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL类方法：EngineExecute()引擎调用新流程时没有选定岗位");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteLog(ErroMessage);
                                throw new Exception("命名空间:SMT.FlowWFService.EnginFlowBLL类方法：EngineExecute()" + ex.Message);
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
                ErroMessage += "该单据所对应的引擎系统设置未找到,请先配置该模块引擎消息(审核通过、审核不通过、审核中):\r\n" + strMsg;
                LogHelper.WriteLog(ErroMessage);
                throw new Exception("该单据所对应的引擎系统设置未找到,请先配置该模块引擎消息(审核通过、审核不通过、审核中)" + strMsg);
            }
            return result;
        }

        /// <summary>
        /// 调用WCF
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="sourceTable"></param>
        /// <param name="strAppMsg"></param>
        private void CallWCFService(OracleConnection con, DataRow dr, DataTable sourceTable, ref string strAppMsg, ref string ErroMessage)
        {

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
                FuncPameter = ReplaceValue(FuncPameter, sourceTable);
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
                    LogHelper.WriteLog(" 成功调用业务系统的WCF记录如下:\r\n" + cMessage);
                }
                else
                {
                    ErroMessage += "命名空间:SMT.FlowWFService类方法：CallWCFService() 调用WCF参数不全WCF协议：" + Binding + "WCF地址：" + WCFUrl + "WCF方法：" + FuncName + "";
                    throw new Exception("命名空间:SMT.FlowWFService类方法：CallWCFService() 调用WCF参数不全WCF协议：" + Binding + "WCF地址：" + WCFUrl + "WCF方法：" + FuncName + "");
                }
            }
            catch (Exception e)
            {
                ErroMessage +="\r\n"+e.ToString();
                LogHelper.WriteLog(ErroMessage);
                throw new Exception("命名空间:SMT.FlowWFService类方法：CallWCFService()" + e.Message);
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
                            //EngineCache.TaskCache.TaskCacheReflesh(user);
                            //MsgTriggerBLL.MsgClose(strSystemCode, strFormID, user);
                            dal.CloseDoTaskStatus(con, strSystemCode, strFormID, user);
                        }
                    }
                    else
                    {
                        //刷新缓存用户是否有新的待办
                        //EngineCache.TaskCache.TaskCacheReflesh(strReceiveUser);
                        //MsgTriggerBLL.MsgClose(strSystemCode, strFormID, strReceiveUser);
                        dal.CloseDoTaskStatus(con, strSystemCode, strFormID, strReceiveID);//关闭待办
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
        public bool FlowCancel(OracleConnection con, string strFlowXml, string strAppXml, ref string ErroMessage)
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

                if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
                {
                    if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
                    {
                        LogHelper.WriteLog("UpdateAuditStatus开始更新...FORMID=" + strFormID + "");
                        bool bol= UpdateAuditStatus(strSystemCode, strEntityType, strEntityKey, strFormID, strCheckState, ref ErroMessage);
                        LogHelper.WriteLog("UpdateAuditStatus结束更新...FORMID=" + strFormID + "");
                        if (!bol)
                        {
                            throw new Exception(ErroMessage); //抛出异常终止执行流程                                  
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
               // LogHelper.WriteLog("FlowCancel流程待办任务撤销异常" + cMessage + ErroMessage);
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
                        }
                        return true;
                    }
                    else
                    {
                        //未能在触发规则定义中找到一条数据，无法发送咨讯
                        string cMessage = "未能在触发规则定义中找到一条数据，无法发送咨讯" + "\r\n" +
                                     "SystemCode:" + strSystemCode + "\r\n" +
                                     "Content:" + content + "\r\n" +
                                     "MODELCODE:" + strModelCode + "\r\n" +
                                     "COMPANYID:" + strCompanyID + "\r\n" +
                                     "FORMID:" + strFormID + "\r\n";
                        LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
                        return false;
                    }

                }
                catch (Exception e)
                {
                    string cMessage = "发送咨讯出现异常" + "\r\n" +
                                      "SystemCode:" + strSystemCode + "\r\n" +
                                      "MODELCODE:" + strModelCode + "\r\n" +
                                      "COMPANYID:" + strCompanyID + "\r\n" +
                                       "Error:" + e.Message + "\r\n" +
                                        "FORMID:" + strFormID + "\r\n";

                    LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
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
        #endregion
        #region 原来旧的
        ///// <summary>
        ///// 流程触发
        ///// </summary>
        ///// <param name="strFlow">流程数据</param>
        ///// <param name="strBusiness">业务数据</param>    
        ///// <returns></returns>
        //public bool SaveFlowTriggerData(string strFlow, string strBusiness)
        //{
        //    T_WF_DOTASK entity = new T_WF_DOTASK();
        //    string strEntityType = string.Empty;//EntityType (表名)
        //    string strEntityKey = string.Empty;//EntityKey (主键)
        //    string IsTask = "1";//是否任务
        //    try
        //    {
        //        try
        //        {
        //            //消息解析
        //            Byte[] bFlow = System.Text.UTF8Encoding.UTF8.GetBytes(strFlow);
        //            XElement xeFlow = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(bFlow)));
        //            entity.COMPANYID = XMLToAttribute(xeFlow, "COMPANYID");
        //            entity.MODELCODE = XMLToAttribute(xeFlow, "MODELCODE");
        //            entity.MODELNAME = XMLToAttribute(xeFlow, "ModelName");
        //            entity.ORDERID = XMLToAttribute(xeFlow, "FORMID");
        //            entity.ORDERSTATUS = int.Parse(XMLToAttribute(xeFlow, "CheckState"));
        //            entity.RECEIVEUSERID = XMLToAttribute(xeFlow, "APPUSERID");
        //            entity.FLOWXML = strFlow;
        //            entity.APPXML = strBusiness;
        //            IsTask = XMLToAttribute(xeFlow, "IsTask");
        //            if (!string.IsNullOrEmpty(XMLToAttribute(xeFlow, "OutTime")))
        //            {
        //                entity.BEFOREPROCESSDATE = DateTime.Now.AddMinutes(int.Parse(XMLToAttribute(xeFlow, "OutTime")));
        //            }
        //            if (!string.IsNullOrEmpty(strBusiness))
        //            {
        //                Byte[] BBusiness = System.Text.UTF8Encoding.UTF8.GetBytes(strBusiness);
        //                XElement xeBusiness = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(BBusiness)));
        //                entity.SYSTEMCODE = (from item in xeBusiness.Descendants("Name") select item).FirstOrDefault().Value;
        //                try
        //                { //非手机的XML时没有表明和主键的
        //                    strEntityType = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Name").Value;
        //                    strEntityKey = (from item in xeBusiness.Descendants("Object") select item).FirstOrDefault().Attribute("Key").Value;
        //                }
        //                catch
        //                {
        //                    LogHelper.WriteLog("异常无法取得业务数据表，单据:" + entity.ORDERID + "：表：" + strEntityType + ":主键：" + strEntityKey + "");
        //                }
        //                entity.ORDERUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
        //                                      where item.Attribute("Name").Value.ToUpper() == "OWNERID"
        //                                      select item).FirstOrDefault().Attribute("DataValue").Value;
        //                entity.ORDERUSERNAME = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
        //                                        where item.Attribute("Name").Value.ToUpper() == "OWNERNAME"
        //                                        select item).FirstOrDefault().Attribute("DataValue").Value;
        //                //有些特殊的模块需要改变接收人
        //                if (CheckModelName(entity.MODELCODE) && entity.ORDERSTATUS == 2)
        //                {
        //                    entity.RECEIVEUSERID = (from item in xeBusiness.Descendants("Object").Descendants("Attribute")
        //                                            where item.Attribute("Name").Value.ToUpper() == "CREATEUSERID"
        //                                            select item).FirstOrDefault().Attribute("DataValue").Value;
        //                }
        //                entity.APPFIELDVALUE = ConbinString(xeFlow) + BOObjectEscapeString(strBusiness);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.WriteLog("SaveFlowTriggerData数据读取出现异常:模块：" + entity.MODELCODE + "单据ID：" + entity.ORDERID + "\r\n" + ex.Message);
        //        }
        //        bool IsExecute = EngineExecute(entity, IsTask);
        //        if (IsExecute)
        //        {
        //            try
        //            {
        //                /****************更新业务系统单据审核状态***********************/
        //                if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
        //                {
        //                    if (!string.IsNullOrEmpty(entity.SYSTEMCODE) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
        //                    {
        //                        UpdateAuditStatus(entity.SYSTEMCODE, strEntityType, strEntityKey, entity.ORDERID, entity.ORDERSTATUS.ToString());
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.WriteLog("SaveFlowTriggerData更新审核状态出现异常:" + ex.Message);
        //            }
        //        }
        //        LogHelper.WriteLog("流程触发存储记录SaveFlowTriggerDataFlowXML:" + strFlow + "\r\n" + "AppXml:" + strBusiness + "\r\n");
        //        return IsExecute;
        //    }
        //    catch (Exception ex)
        //    {

        //        string cMessage = string.Empty;
        //        cMessage += strFlow + "\r\n";
        //        cMessage += strBusiness + "\r\n";
        //        LogHelper.WriteLog("对外公开服务，流程触发出现异常SaveFlowTriggerData" + cMessage + ex);
        //        return false;
        //    }
        //}

        //private bool EngineExecute(T_WF_DOTASK Entity, string IsTask)
        //{
        //    bool result = false;
        //    EnginFlowDAL dal = new EnginFlowDAL();
        //    string strAppFieldValue = string.Empty;
        //    DataTable sourceTable = FieldStringToDataTable(Entity.APPFIELDVALUE, ref strAppFieldValue);//将业务数据与流程数据转换成DataTable作为替换的源数据
        //    //通过系统代号，模块代号，企业ID，流程状态查到引擎配置内容
        //    DataTable dtFlowTrigger = dal.FlowTriggerTable(Entity.SYSTEMCODE, Entity.MODELCODE, Entity.ORDERSTATUS.ToString(), Entity.COMPANYID);
        //    if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
        //    {
        //        DataRow[] drs = dtFlowTrigger.Select("ISDEFAULTMSG=1");//发的是默认流程触发条件
        //        if (drs.Count() > 0)
        //        {
        //            if (IsTask == "1")//新增待办任务
        //            {
        //                dal.AddDoTask(Entity, drs[0], sourceTable, strAppFieldValue);//新增待办任务
        //            }
        //            else if (IsTask == "0")//消息
        //            {
        //                dal.AddDoTaskMessage(Entity, drs[0], sourceTable);
        //            }
        //        }
        //        else
        //        {
        //            string strMsg = "系统代号" + Entity.SYSTEMCODE + "\r\n" +
        //                            "模块代号：" + Entity.MODELCODE + "\r\n" +
        //                            "触发条件：" + Entity.ORDERSTATUS +
        //                            "公司ID：" + Entity.COMPANYID + "\r\n" +
        //                            "单据ID：" + Entity.ORDERUSERID;
        //            LogHelper.WriteLog("没有设置引擎触发默认条件" + strMsg);
        //        }
        //        DataRow[] NotDefaultMsg = dtFlowTrigger.Select("ISDEFAULTMSG=0");//非默认消息，需要调用的WCF待办任务
        //        if (NotDefaultMsg != null && NotDefaultMsg.Count() > 0)
        //        {
        //            foreach (DataRow dr1 in NotDefaultMsg)
        //            {
        //                string strAppMsg = string.Empty;
        //                CallWCFService(dr1, sourceTable, ref strAppMsg);//调用WCF服务
        //                if (!string.IsNullOrEmpty(strAppMsg))
        //                {
        //                    try
        //                    {
        //                        string IsNewFlow = "1";
        //                        string NewFormID = string.Empty;
        //                        string strFormTypes = string.Empty;//表单状态
        //                        DataRow DRNewTrigger = null;
        //                        if (ApplicationValueToDataTable(strAppMsg, string.Concat(Entity.COMPANYID), ref sourceTable, ref IsNewFlow, ref NewFormID, ref strFormTypes, ref DRNewTrigger))
        //                        {
        //                            //通过岗位查找用户，并且取第一个用户为发送消息的对像
        //                            PersonnelServiceClient HRClient = new PersonnelServiceClient();
        //                            if (!string.IsNullOrEmpty(dr1["OWNERPOSTID"].ToString()))
        //                            {
        //                                string[] Employees = HRClient.GetEmployeeIDsByPostID(dr1["OWNERPOSTID"].ToString());
        //                                if (Employees != null && Employees.Count() > 0)
        //                                {
        //                                    dal.AddDoTask(Entity, dr1, sourceTable, Employees[0], NewFormID, strAppFieldValue, string.Concat(dr1["MESSAGEBODY"]), strFormTypes);//发送消息
        //                                }
        //                            }
        //                            else
        //                            {
        //                                string cMessage = "引擎调用新流程时没有选定岗位 SystemCode:" + string.Concat(DRNewTrigger["SYSTEMCODE"]) + " MODELCODE:" + string.Concat(DRNewTrigger["MODELCODE"]) + " NewFormID:" + NewFormID;
        //                                LogHelper.WriteLog("引擎调用新流程时没有选定岗位" + cMessage);
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        string cMessage = "EngineExecute:" + strAppMsg;
        //                        LogHelper.WriteLog("EngineExecute" + cMessage + ex.Message);
        //                    }
        //                }
        //            }
        //        }
        //        result = true;
        //    }
        //    else//没有在引擎规则定义，发默认消息
        //    {
        //        string strMsg = "系统代号" + Entity.SYSTEMCODE + "\r\n" +
        //                         "模块代号：" + Entity.MODELCODE + "\r\n" +
        //                         "触发条件：" + Entity.ORDERSTATUS +
        //                         "公司ID：" + Entity.COMPANYID + "\r\n" +
        //                         "单据ID：" + Entity.ORDERID;
        //        LogHelper.WriteLog("该单据所对应的引擎系统设置未找到" + strMsg);
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 调用WCF
        ///// </summary>
        ///// <param name="dr"></param>
        ///// <param name="sourceTable"></param>
        ///// <param name="strAppMsg"></param>
        //private void CallWCFService(DataRow dr, DataTable sourceTable, ref string strAppMsg)
        //{
        //    string Binding = dr["WCFBINDINGCONTRACT"].ToString();
        //    string WCFUrl = dr["WCFURL"].ToString();
        //    WCFUrl = Config.GetSystemCode(dr["SYSTEMCODE"].ToString()) + WCFUrl;
        //    string FuncName = dr["FUNCTIONNAME"].ToString();
        //    string FuncPameter = dr["FUNCTIONPARAMTER"].ToString();
        //    string SplitChar = dr["PAMETERSPLITCHAR"].ToString();
        //    string strDeptID = dr["OWNERDEPARTMENTID"].ToString();
        //    string strCorpID = dr["OWNERCOMPANYID"].ToString();
        //    string strPostID = dr["OWNERPOSTID"].ToString();
        //    string strOwnerID = dr["RECEIVEUSER"].ToString();

        //    FuncPameter = ReplaceValue(FuncPameter, sourceTable);
        //    string XmlTemplete = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + "\r\n" +
        //                        "<Paras>" + "\r\n" +
        //                        "<Para TableName=\"\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"" + strOwnerID + "\"></Para>" +
        //                        "<Para TableName=\"\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"" + strDeptID + "\"></Para>" +
        //                        "<Para TableName=\"\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"" + strPostID + "\"></Para>" +
        //                        "<Para TableName=\"\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"" + strCorpID + "\"></Para>" +
        //                        "{0}" +
        //                        "</Paras>";
        //    FuncPameter = string.Format(XmlTemplete, FuncPameter);
        //    int Start = System.Environment.TickCount;
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(Binding) && !string.IsNullOrEmpty(WCFUrl) && !string.IsNullOrEmpty(FuncName))
        //        {
        //            //GlobalFunction.CustomizeSplitChar = CustomsizeSplitChar;

        //            object TmpreceiveString = SMT.Workflow.Engine.BLL.Utility.CallWCFService(Binding, WCFUrl, FuncName, FuncPameter);
        //            strAppMsg = TmpreceiveString.ToString();
        //            int End = System.Environment.TickCount - Start;
        //            string cMessage = "执行链接：" + WCFUrl + "\r\n" +
        //                              "执行方法:" + FuncName + "\r\n" +
        //                              "执行时间:" + End.ToString() + "\r\n" +
        //                              "绑定契约：" + Binding + "\r\n" +
        //                              "执行结果：" + strAppMsg + "\r\n" +
        //                              "执行参数：" + FuncPameter + "\r\n" +
        //                              "----------------------------------------------------------";
        //            LogHelper.WriteLog("引擎内核调用WCF(正常执行)" + cMessage);

        //        }
        //        else
        //        {
        //            string cMessage = "执行链接：" + WCFUrl + "\r\n" +
        //                            "执行方法:" + FuncName + "\r\n" +
        //                            "绑定契约：" + Binding + "\r\n" +
        //                            "执行结果：" + strAppMsg + "\r\n" +
        //                            "执行参数：" + FuncPameter + "\r\n" +
        //                            "----------------------------------------------------------";
        //            LogHelper.WriteLog("引擎内核调用WCF(出现异常)：参数不全" + cMessage);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        int End = System.Environment.TickCount - Start;
        //        string cMessage = "执行链接：" + WCFUrl + "\r\n" +
        //                           "执行方法:" + FuncName + "\r\n" +
        //                           "执行时间:" + End.ToString() + "\r\n" +
        //                           "绑定契约：" + Binding + "\r\n" +
        //                           "执行结果：" + strAppMsg + "\r\n" +
        //                           "执行参数：" + FuncPameter + "\r\n" +
        //                        "异常：" + e.Message + "\r\n" +
        //                        "执行XML:" + XmlTemplete + "\r\n" +
        //                        "----------------------------------------------------------";
        //        LogHelper.WriteLog("调用WCF执行时间(出现异常)" + cMessage);
        //    }

        //}

        ///// <summary>
        ///// 调用WCF返回数据转换成数据源
        ///// </summary>
        ///// <param name="strAppMsg">返回数据内容</param>
        ///// <param name="sourceTable">原数据源Table</param>
        ///// <param name="IsNewFlow">是否是新流程</param>
        ///// <param name="DRFlowTrigger">新流程对应的流程定义</param>
        ///// <returns></returns>
        //private bool ApplicationValueToDataTable(string strAppMsg, string strCompanyID, ref DataTable sourceTable, ref string IsNewFlow, ref string NewFormID, ref string strFormTypes, ref DataRow DRFlowTrigger)
        //{
        //    try
        //    {
        //        EnginFlowDAL dal = new EnginFlowDAL();
        //        if (sourceTable == null)
        //        {
        //            sourceTable = new DataTable();
        //            sourceTable.Columns.Add("FieldType", typeof(string));
        //            sourceTable.Columns.Add("ColumnName", typeof(string));
        //            sourceTable.Columns.Add("ColumnValue", typeof(string));
        //        }
        //        Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strAppMsg);
        //        XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
        //        var v = from c in xele.Descendants("Attribute")
        //                select c;
        //        IsNewFlow = string.Empty;
        //        try
        //        {
        //            //是否是新流程（0：新流程，1：旧流程）
        //            IsNewFlow = (from c in xele.Descendants("IsNewFlow") select c).FirstOrDefault().Value;
        //        }
        //        catch
        //        {
        //            IsNewFlow = "1";
        //        }
        //        try
        //        {
        //            NewFormID = (from c in xele.Descendants("NewFormID") select c).FirstOrDefault().Value;
        //        }
        //        catch { }
        //        try
        //        {
        //            strFormTypes = (from c in xele.Descendants("FormTypes") select c).FirstOrDefault().Value;
        //        }
        //        catch { }

        //        if (v.Count() > 0)
        //        {
        //            string SystemCode = string.Empty;
        //            string ModelCode = string.Empty;
        //            foreach (var vv in v)
        //            {
        //                DataRow dr = sourceTable.NewRow();
        //                string Name = vv.Attribute("Name").Value.ToString();
        //                DataRow[] drs = sourceTable.Select("ColumnName='" + Name + "'");
        //                if (drs != null && drs.Length > 0)
        //                {
        //                    foreach (DataRow row in drs)
        //                    {
        //                        sourceTable.Rows.Remove(row);//移除相同数据
        //                    }
        //                }
        //                string Value = vv.Attribute("DataValue").Value.ToString();
        //                dr["ColumnName"] = Name;
        //                dr["ColumnValue"] = Value;
        //                dr["FieldType"] = "sys";
        //                if (IsNewFlow == "0")
        //                {
        //                    switch (Name)
        //                    {
        //                        case "SYSTEMCODE":
        //                            SystemCode = Value;
        //                            break;
        //                        case "MODELCODE":
        //                            ModelCode = Value;
        //                            break;
        //                    }
        //                }
        //                sourceTable.Rows.Add(dr);
        //            }
        //            //新流程，并且系统代号与模块代号不为空
        //            if (IsNewFlow == "0" && !string.IsNullOrEmpty(SystemCode) && !string.IsNullOrEmpty(ModelCode))
        //            {
        //                DataTable table = dal.FlowTriggerTable(SystemCode, ModelCode, null, strCompanyID);
        //                if (table != null && table.Rows.Count > 0)
        //                {
        //                    DRFlowTrigger = table.Rows[0];
        //                }

        //            }
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    return false;
        //}
        ///// <summary>
        ///// 任务完成后，消息关闭
        ///// </summary>
        ///// <param name="strSystemCode">系统代码</param>
        ///// <param name="strFormID">单据ID</param>
        ///// <param name="strReceiveID">接收人</param>
        //public void TaskMsgClose(string strSystemCode, string strFormID, string strReceiveID)
        //{
        //    if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID) && !string.IsNullOrEmpty(strReceiveID))
        //    {
        //        try
        //        {
        //            EnginFlowDAL dal = new EnginFlowDAL();
        //            if (strReceiveID.IndexOf(',') != -1)
        //            {
        //                string[] users = strReceiveID.Split(',');
        //                foreach (string user in users)
        //                {
        //                    //刷新缓存用户是否有新的待办 暂时没有写入缓存
        //                    //EngineCache.TaskCache.TaskCacheReflesh(user);
        //                    //MsgTriggerBLL.MsgClose(strSystemCode, strFormID, user);
        //                    dal.CloseDoTaskStatus(strSystemCode, strFormID, user);
        //                }
        //            }
        //            else
        //            {
        //                //刷新缓存用户是否有新的待办
        //                //EngineCache.TaskCache.TaskCacheReflesh(strReceiveUser);
        //                //MsgTriggerBLL.MsgClose(strSystemCode, strFormID, strReceiveUser);
        //                dal.CloseDoTaskStatus(strSystemCode, strFormID, strReceiveID);//关闭待办
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //            string cMessage = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + strReceiveID + "',Error='" + e.Message + "'";
        //            LogHelper.WriteLog("TaskMsgClose" + cMessage);
        //        }
        //    }
        //}


        ///// <summary>
        ///// 流程待办任务撤销
        ///// </summary>
        ///// <param name="strFlowXml"></param>
        ///// <param name="strAppXml"></param>      
        ///// <returns></returns>
        //public bool FlowCancel(string strFlowXml, string strAppXml)
        //{
        //    if (string.IsNullOrEmpty(strFlowXml) && string.IsNullOrEmpty(strAppXml))
        //    {
        //        return false;
        //    }
        //    string strCompanyID = string.Empty;//公司ID 
        //    string strSystemCode = string.Empty;//系统代号
        //    string strModelCode = string.Empty;//模块代号
        //    string strFormID = string.Empty;//FORMID
        //    string strModelName = string.Empty;//模块名称
        //    string strEntityType = string.Empty;//EntityType (表名)
        //    string strEntityKey = string.Empty;//EntityKey (主键)
        //    string strCheckState = string.Empty;//审核状态
        //    string strReceiveID = string.Empty;//接收人

        //    try
        //    {
        //        EnginFlowDAL dal = new EnginFlowDAL();
        //        /*解析流程和业务数据XML*/
        //        Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strFlowXml);
        //        XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
        //        strCompanyID = XMLToAttribute(xele, "COMPANYID");
        //        strSystemCode = string.Empty;
        //        strModelCode = XMLToAttribute(xele, "MODELCODE");
        //        strFormID = XMLToAttribute(xele, "FORMID");
        //        strModelName = XMLToAttribute(xele, "ModelName");
        //        strCheckState = XMLToAttribute(xele, "CheckState");
        //        strReceiveID = XMLToAttribute(xele, "APPUSERID");
        //        if (!string.IsNullOrEmpty(strAppXml))
        //        {
        //            Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(strAppXml);
        //            XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
        //            strSystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
        //            try
        //            {
        //                strEntityType = (from item in xemeBoObject.Descendants("Object") select item).FirstOrDefault().Attribute("Name").Value;
        //                strEntityKey = (from item in xemeBoObject.Descendants("Object") select item).FirstOrDefault().Attribute("Key").Value;
        //                //有些特殊的模块需要改变接收人
        //                if (CheckModelName(strModelCode) && strCheckState == "2")
        //                {
        //                    strReceiveID = (from item in xemeBoObject.Descendants("Object").Descendants("Attribute")
        //                                    where item.Attribute("Name").Value.ToUpper() == "CREATEUSERID"
        //                                    select item).FirstOrDefault().Attribute("DataValue").Value;
        //                }
        //            }
        //            catch
        //            {
        //                LogHelper.WriteLog("FlowCancel该单号:" + strFormID + " 中业务数据无法取得EntityKey");
        //            }
        //        }

        //        string strAppFieldValue = ConbinString(xele) + BOObjectEscapeString(strAppXml);
        //        DataTable dtValue = FieldStringToDataTable(strAppFieldValue, ref strAppFieldValue);
        //        string content = dal.GetMessageDefine("FLOWCANCEL");
        //        string strUrl = string.Empty;
        //        ReplaceUrl(ref content, ref strUrl, dtValue);
        //        dal.DoTaskCancel(strSystemCode, strModelCode, strFormID, strReceiveID, content);

        //        if (Config.IsNeedUpdateAudit)//是否执行更新审核状态
        //        {
        //            if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strEntityType) && !string.IsNullOrEmpty(strEntityKey))
        //            {
        //                UpdateAuditStatus(strSystemCode, strEntityType, strEntityKey, strFormID, strCheckState);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        string cMessage = "流程待办任务撤销异常" + "\r\n" +
        //                          "SystemCode:" + strSystemCode + "\r\n" +
        //                          "MODELCODE:" + strModelCode + "\r\n" +
        //                          "COMPANYID:" + strCompanyID + "\r\n" +
        //                           "Error:" + e.Message + "\r\n" +
        //                            "FORMID:" + strFormID + "\r\n" +
        //                           "流程XML:" + strFlowXml + "\r\n" +
        //                          "AppXml:" + strAppXml + "\r\n";
        //        LogHelper.WriteLog("FlowCancel流程待办任务撤销异常" + cMessage);
        //        return false;
        //    }

        //}

        ///// <summary>
        /////  流程咨讯 
        ///// </summary>
        ///// <param name="UserAndForm"></param>
        ///// <param name="strTitle"></param>
        ///// <param name="strFlowXML"></param>
        ///// <param name="strAppXml"></param>  
        ///// <returns></returns>
        //public bool FlowConsultati(List<CustomUserMsg> UserAndForm, string strTitle, string strFlowXML, string strAppXml)
        //{
        //    string strCompanyID = string.Empty;//公司ID 
        //    string strSystemCode = string.Empty;//系统代号
        //    string strModelCode = string.Empty;//模块代号
        //    string strFormID = string.Empty;//FORMID
        //    string strModelName = string.Empty;//模块名称
        //    if (UserAndForm.Count > 0)
        //    {
        //        try
        //        {
        //            EnginFlowDAL dal = new EnginFlowDAL();
        //            /*解析流程和业务数据XML*/
        //            Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strFlowXML);
        //            XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
        //            strCompanyID = XMLToAttribute(xele, "COMPANYID");
        //            strSystemCode = string.Empty;
        //            strModelCode = XMLToAttribute(xele, "MODELCODE");
        //            strFormID = XMLToAttribute(xele, "FORMID");
        //            strModelName = XMLToAttribute(xele, "ModelName");
        //            if (!string.IsNullOrEmpty(strAppXml))
        //            {
        //                Byte[] Bo = System.Text.UTF8Encoding.UTF8.GetBytes(strAppXml);
        //                XElement xemeBoObject = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(Bo)));
        //                strSystemCode = (from item in xemeBoObject.Descendants("Name") select item).FirstOrDefault().Value;
        //            }

        //            /*在T_WF_MESSAGEDEFINE 中查找消息内容*/
        //            string content = dal.GetMessageDefine("FLOWCONSULTATI");
        //            string strAppFieldValue = ConbinString(xele) + BOObjectEscapeString(strAppXml);
        //            DataTable dtValue = FieldStringToDataTable(strAppFieldValue, ref strAppFieldValue);

        //            /*在消息触发规则定义中查找消息链接*/
        //            DataTable dtFlowTrigger = dal.FlowTriggerTable(strSystemCode, strModelCode, string.Empty, strCompanyID);
        //            if (dtFlowTrigger != null && dtFlowTrigger.Rows.Count > 0)
        //            {
        //                string Url = dtFlowTrigger.Rows[0]["applicationurl"].ToString();
        //                ReplaceUrl(ref content, ref Url, dtValue);
        //                string strContent = string.Empty;
        //                foreach (CustomUserMsg custom in UserAndForm)
        //                {
        //                    if (!string.IsNullOrEmpty(strTitle))
        //                    {
        //                        content = strTitle;
        //                    }
        //                    //刷新缓存用户是否有新的待办
        //                    //EngineCache.TaskCache.TaskCacheReflesh(custom.UserID);
        //                    dal.FlowConsultatiMsg(custom.UserID, strSystemCode, strModelCode, custom.FormID, content, Url, BOObjectEscapeString(strAppXml), strFlowXML, strAppXml);
        //                }
        //                return true;
        //            }
        //            else
        //            {
        //                //未能在触发规则定义中找到一条数据，无法发送咨讯
        //                string cMessage = "未能在触发规则定义中找到一条数据，无法发送咨讯" + "\r\n" +
        //                             "SystemCode:" + strSystemCode + "\r\n" +
        //                             "Content:" + content + "\r\n" +
        //                             "MODELCODE:" + strModelCode + "\r\n" +
        //                             "COMPANYID:" + strCompanyID + "\r\n" +
        //                             "FORMID:" + strFormID + "\r\n";
        //                LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
        //                return false;
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            string cMessage = "发送咨讯出现异常" + "\r\n" +
        //                              "SystemCode:" + strSystemCode + "\r\n" +
        //                              "MODELCODE:" + strModelCode + "\r\n" +
        //                              "COMPANYID:" + strCompanyID + "\r\n" +
        //                               "Error:" + e.Message + "\r\n" +
        //                                "FORMID:" + strFormID + "\r\n";

        //            LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
        //            return false;
        //        }

        //    }
        //    else
        //    {
        //        string cMessage = "发送咨讯出现错误（没有指定接收人员）" + "\r\n" +
        //                             "SystemCode:" + strSystemCode + "\r\n" +
        //                             "MODELCODE:" + strModelCode + "\r\n" +
        //                             "COMPANYID:" + strCompanyID + "\r\n" +
        //                             "FORMID:" + strFormID + "\r\n";
        //        LogHelper.WriteLog("FlowConsultati()流程咨讯" + cMessage);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 流程咨讯关闭
        ///// </summary>
        ///// <param name="strSystemCode"></param>
        ///// <param name="strFormID"></param>
        ///// <param name="strReceiveID"></param>      
        //public void FlowConsultatiClose(string strSystemCode, string strFormID, string strReceiveID)
        //{
        //    if (!string.IsNullOrEmpty(strSystemCode) && !string.IsNullOrEmpty(strFormID) && !string.IsNullOrEmpty(strReceiveID))
        //    {
        //        try
        //        {
        //            EnginFlowDAL dal = new EnginFlowDAL();
        //            if (strReceiveID.IndexOf(',') != -1)
        //            {
        //                string[] users = strReceiveID.Split(',');
        //                foreach (string user in users)
        //                {
        //                    //刷新缓存用户是否有新的待办
        //                    //EngineCache.TaskCache.TaskCacheReflesh(user);
        //                    //SMTEngineBLL.CloseMsg(strSystemCode, strFormID, user);
        //                    dal.CloseConsultati(strSystemCode, strFormID, user);
        //                }
        //            }
        //            //SMTEngineBLL.CloseMsg(strSystemCode, strFormID, strReceiveUser);
        //            dal.CloseConsultati(strSystemCode, strFormID, strReceiveID);
        //        }
        //        catch (Exception e)
        //        {

        //            string cMessage = "TaskMsgClose出现异常：SystemCode='" + strSystemCode + "',FormID='" + strFormID + "',ReceiveUser='" + strReceiveID + "',Error='" + e.Message + "'";
        //            LogHelper.WriteLog("TaskMsgClose" + cMessage);
        //        }
        //    }
        //}
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

        public bool UpdateAuditStatus(string strSystemCode, string EntityType, string EntityKey, string EntityId, string strCheckState, ref string errorMsg)
        {
            return false;
            //bool bol = true;
            //string strMsg = "系统代号：" + strSystemCode + "\r\n"
            //                   + "EntityType:" + EntityType + "\r\n"
            //                   + "EntityId:" + EntityId + "\r\n"
            //                   + "EntityKey:" + EntityKey + "\r\n"
            //                   + "CheckState:" + strCheckState + "\r\n";
            //try
            //{
            //    // 1：审核中；2审核通过；3：审核不通过 
            //    CheckStates CheckState = CheckStates.Approving;
            //    switch (strCheckState)
            //    {
            //        case "1":
            //            CheckState = CheckStates.Approving;
            //            break;
            //        case "2":
            //            CheckState = CheckStates.Approved;
            //            break;
            //        case "3":
            //            CheckState = CheckStates.UnApproved;
            //            break;
            //        case "4":
            //            CheckState = CheckStates.Cancel;
            //            break;
            //    }

            //    #region 配合新预算使用
            //    string serviceErrorMsg = "";//业务系统的异常信息
            //    bool bResult = Utility.UpdateFormCheckState(strSystemCode, EntityType, EntityKey, EntityId, CheckState, ref serviceErrorMsg);
            //    if (!bResult)
            //    {
            //        LogHelper.WriteLog("更新审核状态失败 FormID=" + EntityId + "\r\n" + strMsg);
            //        bol = false;
            //        //errorMsg = "更新审核状态失败方法:UpdateFormCheckState:" + strMsg+errorMsg;
            //        errorMsg = serviceErrorMsg;
            //        LogHelper.WriteLog("bResult=" + bResult.ToString() + ";Utility.UpdateFormCheckState发生异常：" + strMsg + "；业务系统异常信息：" + serviceErrorMsg);
            //        return bol;
            //        //throw new Exception("更新审核状态失败,方法:UpdateFormCheckState" + strMsg);
            //    }
            //    #endregion

            //    //bool bResult = SMT.SaaS.BLLCommonServices.Utility.UpdateFormCheckState(strSystemCode, EntityType, EntityKey, EntityId, CheckState);
            //    //if (!bResult)
            //    //{
            //    //    LogHelper.WriteLog("更新审核状态失败\r\n" + strMsg);
            //    //    LogHelper.WriteLog("更新审核状态失败" + strMsg);
            //    //}
            //    else
            //    {

            //        LogHelper.WriteLog("bResult=" + bResult.ToString() + ";更新审核状态成功\r\n" + strMsg);
            //        LogHelper.WriteLog("更新审核状态成功" + strMsg);
            //        return bol;
            //    }

            //}
            //catch (Exception ex)
            //{

            //    LogHelper.WriteLog("更新审核状态出现异常" + strMsg + "UpdateAuditStatus" + ex);
            //    LogHelper.WriteLog("更新审核状态出现异常" + strMsg + "UpdateAuditStatus" + ex);
            //    return false;
            //}
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
                        drvalue["ColumnValue"] = strValue;
                        drvalue["ColumnText"] = strText;
                    }
                }
            }
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
