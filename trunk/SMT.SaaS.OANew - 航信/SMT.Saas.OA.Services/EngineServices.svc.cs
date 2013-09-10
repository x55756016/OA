using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Globalization;
using Smt.Global.IContract;
using System.Xml.Linq;
using System.IO;
using SMT_OA_EFModel;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using EmployeeWS = SMT.SaaS.BLLCommonServices.PersonnelWS;
using CityName = SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.Foundation.Log;
using SMT.SaaS.OA.BLL;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace SMT.SaaS.OA.Services
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EngineServices : IApplicationService
    {
        private static EmployeeWS.PersonnelServiceClient cinet = new EmployeeWS.PersonnelServiceClient();//人事服务(查询员工岗位级别用)
        private static EmployeeWS.V_EMPLOYEEDETAIL emp = new EmployeeWS.V_EMPLOYEEDETAIL();
        private static List<T_OA_CANTAKETHEPLANELINE> PlaneObj = new List<T_OA_CANTAKETHEPLANELINE>();
        private static List<T_OA_TAKETHESTANDARDTRANSPORT> StandardObj = new List<T_OA_TAKETHESTANDARDTRANSPORT>();
        SmtOAPersonOffice doc = new SmtOAPersonOffice();
        private static string postLevel = string.Empty;
        private static string companyId = string.Empty;
        private static string solutionID = string.Empty;

        #region IApplicationService 成员
        
        /// <summary>
        /// 引擎调用入口
        /// ModelNames  是个枚举类型  需要在里面添加相应的表单名
        /// AddSenddoc  是个函数
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        public string CallWaitAppService(string strXml)
        {
            try
            {
                //生成社包单的业务逻辑
                Tracer.Debug(strXml);

                string strReturn = string.Empty;
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var eGFunc = from c in xele.Descendants("Para")
                             select c;
                // string funcName = eGFunc.FirstOrDefault().Attribute("TableName").Value;
                string paraValue = string.Empty;
                string formID = string.Empty;
                string systemCode = string.Empty;
                string modelCode = string.Empty;
                //SMT.Foundation.Log.Tracer.Debug(eGFunc.ToString());
                bool FlagSend = true;
                foreach (var item in eGFunc.Attributes("TableName"))
                {
                    SMT.Foundation.Log.Tracer.Debug("luojie " + item.Value.ToString());
                    if (item.Value == ModelNames.T_OA_SENDDOC.ToString())//公司发文
                    {
                        paraValue = AddSenddoc(eGFunc);
                        formID = "SENDDOCID";
                        modelCode = ModelNames.T_OA_SENDDOC.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_BUSINESSTRIP.ToString())//出差申请
                    {
                        //出差报销的定时信息 luojie
                        try
                        {
                            if (FlagSend)
                            {
                                MakeTravelreimbursementTriggerEntity(strXml);
                                FlagSend = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SMT.Foundation.Log.Tracer.Debug("EngineServices-MakeTravelreimbursementTriggerEntity-" + ex.ToString());
                        }
                        //====================================================
                        //paraValue = TravelmanagementAddFromEngine(strXml);
                        //formID = "BUSINESSTRIPID";
                        //modelCode = ModelNames.T_OA_BUSINESSTRIP.ToString();               
                        break;
                    }
                    //if (item.Value == ModelNames.T_OA_BUSINESSREPORT.ToString())//出差报告
                    //{
                    //    paraValue = MissionReportsAdd(eGFunc);
                    //    formID = "BUSINESSREPORTID";
                    //    modelCode = ModelNames.T_OA_BUSINESSREPORT.ToString();
                    //    break;
                    //}
                    //if (item.Value == ModelNames.T_OA_TRAVELREIMBURSEMENT.ToString())//出差报销
                    //{
                    //    //paraValue = TravelReimbursementAdd(eGFunc);
                    //    //formID = "TRAVELREIMBURSEMENTID";
                    //    //modelCode = ModelNames.T_OA_TRAVELREIMBURSEMENT.ToString();
                    //    break;
                    //}
                    if (item.Value == ModelNames.T_OA_CONTRACTAPP.ToString())//合同申请
                    {
                        paraValue = ContractApprovalAdd(eGFunc);
                        formID = "CONTRACTAPPID";
                        modelCode = ModelNames.T_OA_CONTRACTAPP.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_CONTRACTVIEW.ToString())//合同查看申请
                    {
                        paraValue = ContractViewapplicationsAdd(eGFunc);
                        formID = "CONTRACTVIEWID";
                        modelCode = ModelNames.T_OA_CONTRACTVIEW.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_WELFAREMASERT.ToString())//福利标准
                    {
                        paraValue = WelfareAdd(eGFunc);
                        formID = "WELFAREID";
                        modelCode = ModelNames.T_OA_WELFAREMASERT.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_WELFAREDISTRIBUTEMASTER.ToString())//福利发放
                    {
                        paraValue = WelfareProvisionAdd(eGFunc);
                        formID = "WELFAREDISTRIBUTEMASTERID";
                        modelCode = ModelNames.T_OA_WELFAREDISTRIBUTEMASTER.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_WELFAREDISTRIBUTEUNDO.ToString())//福利发放撤销
                    {
                        paraValue = WelfarePaymentWithdrawalAdd(eGFunc);
                        formID = "WELFAREDISTRIBUTEUNDOID";
                        modelCode = ModelNames.T_OA_WELFAREDISTRIBUTEUNDO.ToString();
                        break;
                    }

                    //-----------------add by zl
                    if (item.Value == ModelNames.T_OA_CONSERVATION.ToString())/// 保养申请
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "CONSERVATIONID";
                        modelCode = ModelNames.T_OA_CONSERVATION.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_CONSERVATIONRECORD.ToString())/// 保养记录
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "CONSERVATIONRECORDID";
                        modelCode = ModelNames.T_OA_CONSERVATIONRECORD.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_MAINTENANCEAPP.ToString())/// 维修申请
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "MAINTENANCEAPPID";
                        modelCode = ModelNames.T_OA_MAINTENANCEAPP.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_MAINTENANCERECORD.ToString())/// 维修记录
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "MAINTENANCERECORDID";
                        modelCode = ModelNames.T_OA_MAINTENANCERECORD.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_VEHICLEDISPATCH.ToString())/// 派车单
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "VEHICLEDISPATCHID";
                        modelCode = ModelNames.T_OA_VEHICLEDISPATCH.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_VEHICLEDISPATCHRECORD.ToString())/// 派车记录
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "VEHICLEDISPATCHRECORDID";
                        modelCode = ModelNames.T_OA_VEHICLEDISPATCHRECORD.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_VEHICLEUSEAPP.ToString())/// 用车申请
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "VEHICLEUSEAPPID";
                        modelCode = ModelNames.T_OA_VEHICLEUSEAPP.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_HOUSEINFOISSUANCE.ToString())/// 房源信息发布
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "ISSUANCEID";
                        modelCode = ModelNames.T_OA_HOUSEINFOISSUANCE.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_HIREAPP.ToString())/// 租房申请
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "HIREAPPID";
                        modelCode = ModelNames.T_OA_HIREAPP.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_ORGANIZATION.ToString())/// 机构表
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "ORGANIZATIONID";
                        modelCode = ModelNames.T_OA_ORGANIZATION.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_LICENSEUSER.ToString())/// 证照印章外借记录
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "LICENSEUSERID";
                        modelCode = ModelNames.T_OA_LICENSEUSER.ToString();
                        break;
                    }

                    if (item.Value == ModelNames.T_OA_LICENSEMASTER.ToString())/// 证照印章表
                    {
                        paraValue = ConservationAdd(eGFunc);
                        formID = "LICENSEMASTERID";
                        modelCode = ModelNames.T_OA_LICENSEMASTER.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_REQUIREMASTER.ToString())//员工调查方案
                    {
                        paraValue = EmployeeSurveyMasterAdd(eGFunc);
                        formID = "REQUIREMASTERID";
                        modelCode = ModelNames.T_OA_REQUIREMASTER.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_REQUIREDISTRIBUTE.ToString())//员工调查发布申请
                    {
                        paraValue = EmployeeSurveyRequireDistributeAdd(eGFunc);
                        formID = "RequireDistributeID";
                        modelCode = ModelNames.T_OA_REQUIREDISTRIBUTE.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_SATISFACTIONMASTER.ToString())//满意度调查申请表
                    {
                        paraValue = EmployeeSatisfactionAdd(eGFunc);
                        formID = "SatisfactionMasterID";
                        modelCode = ModelNames.T_OA_SATISFACTIONMASTER.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_OA_SATISFACTIONDISTRIBUTE.ToString())//满意度调查发布申请
                    {
                        paraValue = EmployeeSatisfactionDistributeAdd(eGFunc);
                        formID = "SatisfactionDistributeID";
                        modelCode = ModelNames.T_OA_SATISFACTIONDISTRIBUTE.ToString();
                        break;
                    }
                    //-----------------end
                }
                //  *****************************
                if (!string.IsNullOrEmpty(paraValue))
                {
                    strReturn = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                      "<System>" +
                                      "  <NewFormID>" + paraValue + "</NewFormID>" +
                                      "<IsNewFlow>0</IsNewFlow>" +
                                      "<Attribute  Name=\"" + formID + "\" DataValue=\"" + paraValue + "\"></Attribute>" +
                                          "<Attribute  Name=\"SYSTEMCODE\" DataValue=\"OA\"></Attribute>" +
                                           "<Attribute  Name=\"MODELCODE\" DataValue=\"" + modelCode + "\"></Attribute>" +
                                        "</System>";
                }
                Tracer.Debug(strReturn);
                return strReturn;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

        #endregion


        #region 出差申请终审后新增出差报销信息
        /// <summary>
        /// 填充TIMINGTRIGGERACTIVITY实体 -luojie
        /// </summary>
        public void MakeTravelreimbursementTriggerEntity(string strXml)
        {
            Tracer.Debug("出差申请开始调用定时触发出差报销生成的接口");
            try
            {
                using (EngineWcfGlobalFunctionClient wcfClient = new EngineWcfGlobalFunctionClient())
                {
                    string strBusinesStripId = string.Empty;
                    StringReader strRdr = new StringReader(strXml);
                    XmlReader xr = XmlReader.Create(strRdr);
                    while (xr.Read())
                    {
                        if (xr.NodeType == XmlNodeType.Element)
                        {
                            string elementName = xr.Name;
                            if (elementName == "Paras" || elementName == "System")
                            {
                                while (xr.Read())
                                {
                                    string type = xr.NodeType.ToString();
                                    #region
                                    if (xr["Name"] != null)
                                    {
                                        if (xr["Name"].ToUpper() == "BUSINESSTRIPID")
                                        {
                                            strBusinesStripId = xr["Value"];
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }

                    T_WF_TIMINGTRIGGERACTIVITY triggerEntity = new T_WF_TIMINGTRIGGERACTIVITY();
                    triggerEntity.TRIGGERID = Guid.NewGuid().ToString();
                    triggerEntity.BUSINESSID = strBusinesStripId;
                    triggerEntity.TRIGGERNAME = strBusinesStripId;
                    triggerEntity.SYSTEMCODE = "OA";
                    triggerEntity.SYSTEMNAME = "办公系统";
                    triggerEntity.MODELCODE = "T_OA_BUSINESSTRIP";
                    triggerEntity.MODELNAME = "出差申请";
                    triggerEntity.TRIGGERACTIVITYTYPE = 2;

                    //获取出差申请的结束时间
                    DateTime arriveTime = GetLatestTimeOfBusinesstrip(GetBusinessIdFromString(strXml));
                    triggerEntity.TRIGGERTIME = arriveTime;//待改-出差申请的结束时间
                    Tracer.Debug("出差申请结束时间:" + arriveTime.ToString());
                    triggerEntity.TRIGGERROUND = 0;
                    triggerEntity.WCFURL = "EngineEventServices.svc";//需要传输数据至的服务名
                    triggerEntity.FUNCTIONNAME = "EventTriggerProcess";//需要传输数据至的方法名称
                    //因两次调用回调函数的问题在此产生出差报销id
                    //strXml += "<Para FuncName=\"DelayTravelreimbursmentAdd\"  Name=\"TRAVELREIMBURSEMENTID\"  Description=\"出差报销ID\" Value=\""+Guid.NewGuid().ToString()+"\" ValueName=\"出差报销ID\" ></Para>";
                    //处理消息规则里T_OA_BUSINESSTRIP的信息
                    strXml = strXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\" ?>", "").Replace("<Paras>", "").Replace("</Paras>", "").Replace("TableName", "FuncName").Replace("T_OA_BUSINESSTRIP", "DelayTravelreimbursmentAdd").Trim();
                    //处理消息规则里T_OA_TRAVELREIMBURSEMENT的信息
                    strXml = strXml.Replace("T_OA_TRAVELREIMBURSEMENT", "DelayTravelreimbursmentAdd");
                    triggerEntity.FUNCTIONPARAMTER = strXml;//传输的对象方法获取的数据
                    triggerEntity.PAMETERSPLITCHAR = "Г";
                    triggerEntity.WCFBINDINGCONTRACT = "CustomBinding";
                    triggerEntity.CREATEDATETIME = System.DateTime.Now;
                    //triggerEntity.TRIGGERSTART = System.DateTime.Now;
                    //triggerEntity.TRIGGEREND = Convert.ToDateTime("2099/12/30 18:00:00");
                    wcfClient.WFAddTimingTrigger(triggerEntity);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差申请调用定时触发出差报销生成的接口失败:" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return;
            }
            Tracer.Debug("出差申请调用定时触发出差报销生成的接口成功");
        }


        #endregion

        #region 公司发文
        /// <summary>
        /// 根据传回的XML，添加公文信息
        /// </summary>
        /// <param name="xele"></param>
        private static string AddSenddoc(IEnumerable<XElement> eGFunc)
        {

            try
            {
                string strMsg = "";
                if (eGFunc.Count() == 0)
                {
                    return strMsg;
                }

                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;
                string strSendDocID = string.Empty;
                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "SENDDOCID":
                            strSendDocID = q.Attribute("Value").Value;
                            break;
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }
                //如果有公司发文就直接产生该公文的代办
                if (!string.IsNullOrEmpty(strSendDocID))
                {
                    return strSendDocID;
                }
                SmtOACommonOffice doc = new SmtOACommonOffice();
                //获取默认一个公司发文类型
                T_OA_SENDDOCTYPE doctype = new T_OA_SENDDOCTYPE();
                List<T_OA_SENDDOCTYPE> listtype = new List<T_OA_SENDDOCTYPE>();
                listtype = doc.GetDocTypeInfos();
                if (listtype.Count() > 0)
                    doctype = listtype.FirstOrDefault();
                if (doctype == null)
                    return strMsg;
                if (string.IsNullOrEmpty(doctype.SENDDOCTYPE))
                    return strMsg;
                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                //if (pensionTmp == null)
                //{

                T_OA_SENDDOC entity = new T_OA_SENDDOC();
                entity.SENDDOCID = Guid.NewGuid().ToString();
                entity.T_OA_SENDDOCTYPE = doctype;
                entity.GRADED = "普通";
                entity.PRIORITIES = "一般";
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                strMsg = doc.SendDocAdd(entity);
                if (string.IsNullOrEmpty(strMsg))
                {
                    strMsg = entity.SENDDOCID;
                }

                return strMsg;

            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        #endregion

        #region 事项审批
        private static string AddApprovalInfo(IEnumerable<XElement> eGFunc)
        {

            try
            {
                string StrResult = string.Empty;
                if (eGFunc.Count() == 0)
                {
                    return StrResult;
                }

                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                SmtOAPersonOffice person = new SmtOAPersonOffice();
                PermissionServiceClient client = new PermissionServiceClient();


                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");


                T_OA_APPROVALINFO entity = new T_OA_APPROVALINFO();
                entity.APPROVALID = Guid.NewGuid().ToString();
                List<string> liststr = new List<string>();
                liststr = person.GetApprovalTypeByCompanyandDepartmentid(strOwnerCompanyID, strOwnerDepartmentID);
                if (liststr.Count() > 0)
                    entity.TYPEAPPROVAL = liststr[0];
                else
                    StrResult = "";
                entity.APPROVALCODE = "BPJ" + DateTime.Now.ToString("yyyyMMdd");
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                int i = person.AddApporval(entity, ref StrResult);
                if (i > 0)
                {
                    StrResult = entity.APPROVALID;
                }

                return StrResult;

            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        #endregion

        #region 会议申请
        private static string AddMeetingInfo(IEnumerable<XElement> eGFunc)
        {

            try
            {
                string StrReturn = "";
                if (eGFunc.Count() == 0)
                {
                    return StrReturn;
                }

                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                SmtOACommonOffice RoomApp = new SmtOACommonOffice();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                //if (pensionTmp == null)
                //{

                T_OA_MEETINGROOMAPP entity = new T_OA_MEETINGROOMAPP();
                entity.MEETINGROOMAPPID = Guid.NewGuid().ToString();
                T_OA_MEETINGROOM room = new T_OA_MEETINGROOM();
                List<T_OA_MEETINGROOM> listroom = new List<T_OA_MEETINGROOM>();
                listroom = RoomApp.GetMeetingRoomNameInfosToCombox();
                if (listroom.Count() > 0)
                    room = listroom.FirstOrDefault();
                else
                    return StrReturn;

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                entity.T_OA_MEETINGROOM = room;
                entity.STARTTIME = DateTime.Now;
                entity.ENDTIME = DateTime.Now.AddDays(2);
                string strMsg = "";
                //doc.SendDocAdd(entity);
                //ser.PensionMasterAdd(entity, ref strMsg);
                strMsg = RoomApp.MeetingRoomAppInfoAdd(entity);
                if (string.IsNullOrEmpty(strMsg))
                {
                    StrReturn = entity.MEETINGROOMAPPID;
                }
                return StrReturn;

            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

        #endregion

        #region 会议室申请
        private static string AddMeetingRoomInfo(IEnumerable<XElement> eGFunc)
        {

            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }

                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                SmtOACommonOffice doc = new SmtOACommonOffice();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                //if (pensionTmp == null)
                //{

                T_OA_SENDDOC entity = new T_OA_SENDDOC();
                entity.SENDDOCID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                string strMsg = "";
                doc.SendDocAdd(entity);
                //ser.PensionMasterAdd(entity, ref strMsg);
                return entity.SENDDOCID;
                //}
                //else
                //{
                //    return pensionTmp.PENSIONMASTERID;
                //}
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        #endregion


        /// <summary>
        /// 获取出差申请最后的时间 -luojie
        /// </summary>
        /// <param name="bustpid">出差申请FormId</param>
        /// <returns>出差结束时间</returns>
        private DateTime GetLatestTimeOfBusinesstrip(string bustpid)
        {
            try
            {
                DateTime latestTime = System.DateTime.Now;
                SmtOAPersonOffice oaOffice = new SmtOAPersonOffice();
                List<T_OA_BUSINESSTRIPDETAIL> TravelDetail = oaOffice.GetBusinesstripDetail(bustpid);
                var latestDate = (from t in TravelDetail
                                  orderby t.ENDDATE descending
                                  select t).FirstOrDefault();
                //引擎只识别“yyyy/mm/dd hh24:mi:ss”格式的时间
                if (latestDate != null)
                {
                    latestTime = latestDate.ENDDATE.Value;
                    string strDate = latestTime.ToString();
                    if (strDate.IndexOf("0:00:00") > 0)
                    {
                        strDate = strDate.Replace("0:00:00", "07:00:00");
                        DateTimeFormatInfo dtf = new System.Globalization.DateTimeFormatInfo();
                        dtf.ShortDatePattern = "yyyy/MM/dd hh24:mi:ss";
                        latestTime = Convert.ToDateTime(strDate,dtf);
                    }
                    else
                    {
                        latestTime = latestDate.ENDDATE.Value;
                    }
                    return latestTime;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("无法获取出差申请最后时间-GetLatestTimeOfBusinesstrip:" + ex.ToString());
            }
            return DateTime.Now;
        }

        /// <summary>
        /// 通过xml文件获取出差申请id
        /// </summary> luojie 20121011
        /// <param name="strXml"></param>
        /// <returns></returns>
        private string GetBusinessIdFromString(string strXml)
        {
            string busntpid=string.Empty;
            try
            {
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var eGFunc = from c in xele.Descendants("Para")
                             select c;

                foreach (var e in eGFunc)
                {
                    if (e.Attribute("Name").Value == "BUSINESSTRIPID" && !string.IsNullOrEmpty(e.Attribute("Value").Value))
                    {
                        busntpid = e.Attribute("Value").Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("获取出差申请信息出错:GetBusinessIdFromString" + ex.ToString());
            }
            return busntpid;
        }

        bool SaveLock = true;
        #region 根据传回的XML，添加出差申请信息
        /// <summary>
        /// 根据传回的XML，添加出差申请信息
        /// </summary>====暂不使用===
        /// <param name="xele"></param>
        //public string TravelmanagementAddFromEngine(string strXml)
        //{

        //    //bool SaveLock = true;//奇怪的问题，下面的运行导致乱序，此乃无法解决的暂缓之道<==坑爹,没用
        //    try
        //    {

        //        string strEmployeeID = string.Empty;
        //        string strOwnerID = string.Empty;
        //        string strOwnerPostID = string.Empty;
        //        string strOwnerDepartmentID = string.Empty;
        //        string strOwnerCompanyID = string.Empty;
        //        string strClaimsWereName = string.Empty;
        //        string strCheckState = string.Empty;
        //        string strTEL = string.Empty;
        //        string strBusinesStripId = string.Empty;
        //        StringReader strRdr = new StringReader(strXml);
        //        XmlReader xr = XmlReader.Create(strRdr);
        //        while (xr.Read())
        //        {
        //            if (xr.NodeType == XmlNodeType.Element)
        //            {
        //                string elementName = xr.Name;
        //                if (elementName == "Paras" || elementName == "System")
        //                {
        //                    while (xr.Read())
        //                    {
        //                        string type = xr.NodeType.ToString();
        //                        #region


        //                        if (xr["Name"] != null)
        //                        {
        //                            if (xr["Name"].ToUpper() == "OWNERPOSTID")
        //                            {
        //                                strOwnerPostID = xr["Value"];
        //                            }
        //                            if (xr["Name"].ToUpper() == "OWNERID")
        //                            {
        //                                strOwnerID = xr["Value"];
        //                            }
        //                            if (xr["Name"].ToUpper() == "OWNERDEPARTMENTID")
        //                            {
        //                                strOwnerDepartmentID = xr["Value"];
        //                            }
        //                            if (xr["Name"].ToUpper() == "OWNERCOMPANYID")
        //                            {
        //                                strOwnerCompanyID = xr["Value"];
        //                            }
        //                            if (xr["Name"].ToUpper() == "BUSINESSTRIPID")
        //                            {
        //                                strBusinesStripId = xr["Value"];
        //                            }
        //                        }

        //                        #endregion
        //                    }
        //                }
        //            }
        //        }
        //        string ff = strBusinesStripId;







        //        //Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
        //        //XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));

        //        //var eGFunc = from c in xele.Descendants("Para")
        //        //             select c;
        //        //if (eGFunc.Count() == 0)
        //        //{
        //        //    return "";
        //        //}

        //        //Tracer.Debug("TravelmanagementAdd+++++333333333333333333333333333");
        //        //#region
        //        ////foreach (var q in eGFunc)
        //        //var tempXml = eGFunc.ToArray();
        //        //for (int j = 0; j < eGFunc.Count();j++ )
        //        //{
        //        //    var q = tempXml[j];
        //        //    string strName = q.Attribute("Name").Value;
        //        //    switch (strName)
        //        //    {
        //        //        case "CREATEUSERID":
        //        //            strEmployeeID = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "OWNERID":
        //        //            strOwnerID = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "OWNERPOSTID":
        //        //            strOwnerPostID = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "OWNERDEPARTMENTID":
        //        //            strOwnerDepartmentID = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "OWNERCOMPANYID":
        //        //            strOwnerCompanyID = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "CLAIMSWERENAME":
        //        //            strClaimsWereName = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "CHECKSTATE":
        //        //            strCheckState = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "TEL":
        //        //            strTEL = q.Attribute("Value").Value;
        //        //            break;
        //        //        case "BUSINESSTRIPID":
        //        //            strBusinesStripId = q.Attribute("Value").Value;
        //        //            break;
        //        //    }
        //        //}
        //        //#endregion
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++777777777777777777777777777777");
        //        //SmtOAPersonOffice doc = new SmtOAPersonOffice();
        //        //string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++11118888" + strBusinesStripId);
        //        T_OA_BUSINESSTRIP buip = doc.GetTravelmanagementById(strBusinesStripId);
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++8888"+buip.OWNERID);
        //        T_OA_TRAVELREIMBURSEMENT entity = new T_OA_TRAVELREIMBURSEMENT();
        //        //Tracer.Debug("We got businessid id " + buip.BUSINESSTRIPID);
        //        entity.TRAVELREIMBURSEMENTID = Guid.NewGuid().ToString();
        //        Tracer.Debug("We got reimbursement id " + entity.TRAVELREIMBURSEMENTID);
        //        entity.T_OA_BUSINESSTRIP = buip;
        //        entity.T_OA_BUSINESSTRIP.BUSINESSTRIPID = buip.BUSINESSTRIPID;
        //        entity.CLAIMSWERE = buip.OWNERID;
        //        entity.CLAIMSWERENAME = buip.OWNERNAME;
        //        entity.REIMBURSEMENTTIME = DateTime.Now;
        //        entity.CHECKSTATE = "0";
        //        entity.TEL = buip.TEL;
        //        entity.CREATEDATE = buip.UPDATEDATE;
        //        entity.OWNERID = buip.OWNERID;
        //        entity.OWNERNAME = buip.OWNERNAME;
        //        entity.OWNERPOSTID = buip.OWNERPOSTID;
        //        entity.OWNERDEPARTMENTID = buip.OWNERDEPARTMENTID;
        //        entity.OWNERCOMPANYID = buip.OWNERCOMPANYID;
        //        entity.CREATEUSERID = buip.CREATEUSERID;
        //        entity.CREATEUSERNAME = buip.CREATEUSERNAME;
        //        entity.CREATEPOSTID = buip.CREATEPOSTID;
        //        entity.CREATEDEPARTMENTID = buip.CREATEDEPARTMENTID;
        //        entity.CREATECOMPANYID = buip.CREATECOMPANYID;
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++GGGGGGGGGGGGGGGGGGGGGGGGGGGGG");
        //        //添加子表数据
        //        emp = cinet.GetEmployeeDetailViewByID(entity.OWNERID);//根据员工ID查询出岗位级别
        //        postLevel = emp.EMPLOYEEPOSTS.Where(s => s.POSTID == buip.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取出差人的岗位级别
        //        companyId = emp.EMPLOYEEPOSTS.Where(s => s.CompanyID == buip.OWNERCOMPANYID).FirstOrDefault().CompanyID.ToString();//获取出差人的所属公司
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++zzzzzzzzzzzzzzzzzzzzzzzzG");
        //        T_OA_TRAVELSOLUTIONS travelsolutions = doc.GetTravelSolutionByCompanyID(entity.OWNERCOMPANYID, ref PlaneObj, ref StandardObj);//出差方案
        //        if (travelsolutions != null)
        //        {
        //            solutionID = travelsolutions.TRAVELSOLUTIONSID;//出差方案ID
        //        }
        //        List<T_OA_BUSINESSTRIPDETAIL> TravelDetail = doc.GetBusinesstripDetail(strBusinesStripId);
        //        List<T_OA_REIMBURSEMENTDETAIL> TrDetail = new List<T_OA_REIMBURSEMENTDETAIL>();//出差报销子表
        //        List<string> cityscode = new List<string>();
        //        double BusinessDays = 0;
        //        int i = 0;
        //        double total = 0;
        //        //Tracer.Debug("TravelmanagementAdd+++++++++++++EEEEEEEEEEEEEEEEEEEEE");
        //        #region
        //        //foreach (var detail in TravelDetail)
        //        for (int j = 0; j < TravelDetail.Count(); j++)
        //        {
        //            var detail = TravelDetail[i];
        //            i++;
        //            double toodays = 0;

        //            //计算本次出差的时间
        //            List<string> list = new List<string>
        //                {
        //                     detail.BUSINESSDAYS
        //                };
        //            if (detail.BUSINESSDAYS != null)
        //            {
        //                double totalHours = System.Convert.ToDouble(list[0]);

        //                BusinessDays += totalHours;//总天数
        //                toodays = totalHours;//单条数据的天数
        //            }
        //            double tresult = toodays;//计算本次出差的总天数

        //            T_OA_REIMBURSEMENTDETAIL TrListInfo = new T_OA_REIMBURSEMENTDETAIL();
        //            TrListInfo.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

        //            TrListInfo.STARTDATE = detail.STARTDATE;//开始时间
        //            TrListInfo.ENDDATE = detail.ENDDATE;//结束时间
        //            TrListInfo.BUSINESSDAYS = detail.BUSINESSDAYS;//出差天数
        //            TrListInfo.DEPCITY = detail.DEPCITY;//出发城市
        //            TrListInfo.DESTCITY = detail.DESTCITY;//目标城市
        //            TrListInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
        //            TrListInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//外出开会
        //            TrListInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
        //            TrListInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
        //            TrListInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
        //            TrListInfo.CREATEDATE = Convert.ToDateTime(buip.UPDATEDATE);//创建时间
        //            TrListInfo.CREATEUSERNAME = buip.CREATEUSERNAME;//创建人
        //            cityscode.Add(TrListInfo.DESTCITY);

        //            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
        //            string cityValue = cityscode[i - 1];//目标城市值
        //            entareaallowance = GetAllowanceByCityValue(cityValue);

        //            #region 根据本次出差的总天数,根据天数获取相应的补贴
        //            if (travelsolutions != null)
        //            {
        //                if (tresult <= int.Parse(travelsolutions.MINIMUMINTERVALDAYS))//本次出差总时间小于等于设定天数的报销标准
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;//交通补贴
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    if (entareaallowance.TRANSPORTATIONSUBSIDIES != null)
        //                                    {
        //                                        TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * toodays).ToString());
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }

        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//餐费补贴
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1")//如果是开会
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * toodays).ToString());
        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.MEALSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                        TrListInfo.MEALSUBSIDIES = 0;
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
        //            if (travelsolutions != null)
        //            {
        //                if (tresult > int.Parse(travelsolutions.MAXIMUMRANGEDAYS))
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
        //                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
        //                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
        //                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
        //                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
        //                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
        //                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }

        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1")//如果是开会
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
        //                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
        //                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
        //                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());

        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.MEALSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                        TrListInfo.MEALSUBSIDIES = 0;
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
        //            if (travelsolutions != null)
        //            {
        //                if (tresult >= Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) && tresult <= Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS))
        //                {
        //                    if (entareaallowance != null)
        //                    {
        //                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
        //                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
        //                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
        //                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
        //                            {
        //                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
        //                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
        //                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }

        //                        if (detail.BUSINESSDAYS != null)
        //                        {
        //                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else if (detail.GOOUTTOMEET == "1")//如果是开会
        //                            {
        //                                TrListInfo.MEALSUBSIDIES = 0;
        //                            }
        //                            else
        //                            {
        //                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                                {
        //                                    //最小区间段金额
        //                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
        //                                    //中间区间段金额
        //                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
        //                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
        //                                }
        //                                else
        //                                {
        //                                    TrListInfo.MEALSUBSIDIES = 0;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
        //                    {
        //                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
        //                        TrListInfo.MEALSUBSIDIES = 0;
        //                    }
        //                }
        //            }
        //            total += Convert.ToDouble(TrListInfo.TRANSPORTATIONSUBSIDIES + TrListInfo.MEALSUBSIDIES);
        //            entity.THETOTALCOST = decimal.Parse(total.ToString());//差旅费用总和
        //            entity.REIMBURSEMENTOFCOSTS = decimal.Parse(total.ToString());//报销费用总和

        //            #endregion

        //            TrDetail.Add(TrListInfo);
        //        }
        //        #endregion
        //        Tracer.Debug("TravelmanagementAdd+++++++++++++WWWWWWWWWWWW");
        //        string result = BusinessDays.ToString(); //计算本次出差的总时间,超过24小时天数加1
        //        entity.COMPUTINGTIME = result;//总时间
        //        //if (SaveLock)
        //        //{
        //        //    Tracer.Debug("测试保存-" + entity.TRAVELREIMBURSEMENTID);
        //        //    doc.TravelReimbursementAdd(entity, TrDetail);
        //        //    SaveLock = false;
        //        //}
        //        Tracer.Debug("测试保存outter-" + entity.TRAVELREIMBURSEMENTID);
        //        //AddTravelreimbursementOnce(entity, TrDetail);

        //        //BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
        //        //EngineWS.CustomUserMsg[] user = new EngineWS.CustomUserMsg[1];
        //        //user[0] = new EngineWS.CustomUserMsg() { UserID = entity.OWNERID, FormID = entity.TRAVELREIMBURSEMENTID };
        //        //Dictionary<string, string> dic = new Dictionary<string, string>();
        //        //dic.Add("BUSINESSTRIPID", buip.BUSINESSTRIPID);
        //        //Client.ApplicationMsgTrigger(user, "OA", "T_OA_TRAVELREIMBURSEMENT", Utility.ObjListToXml<T_OA_TRAVELREIMBURSEMENT>(entity, dic, "OA", null), EngineWS.MsgType.Task);

        //        return entity.TRAVELREIMBURSEMENTID;
        //    }
        //    catch (Exception e)
        //    {
        //        string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
        //        Tracer.Debug(abc);
        //        return abc;
        //    }
        //}
        #endregion

       

        //#region 根据传回的XML，添加出差报告信息
        ///// <summary>
        ///// 根据传回的XML，添加出差报告信息
        ///// </summary>
        ///// <param name="xele"></param>
        //private static string MissionReportsAdd(IEnumerable<XElement> eGFunc)
        //{
        //    try
        //    {
        //        if (eGFunc.Count() == 0)
        //        {
        //            return "";
        //        }
        //        string strEmployeeID = string.Empty;
        //        string strOwnerID = string.Empty;
        //        string strOwnerPostID = string.Empty;
        //        string strOwnerDepartmentID = string.Empty;
        //        string strOwnerCompanyID = string.Empty;
        //        string strContent = string.Empty;
        //        string strCheckState = string.Empty;
        //        string strOwnerName = string.Empty;
        //        string strTel = string.Empty;
        //        string strBusinesStripId = string.Empty;

        //        foreach (var q in eGFunc)
        //        {
        //            string strName = q.Attribute("Name").Value;
        //            switch (strName)
        //            {
        //                case "CREATEUSERID":
        //                    strEmployeeID = q.Attribute("Value").Value;
        //                    break;
        //                case "OWNERID":
        //                    strOwnerID = q.Attribute("Value").Value;
        //                    break;
        //                case "OWNERPOSTID":
        //                    strOwnerPostID = q.Attribute("Value").Value;
        //                    break;
        //                case "OWNERDEPARTMENTID":
        //                    strOwnerDepartmentID = q.Attribute("Value").Value;
        //                    break;
        //                case "OWNERCOMPANYID":
        //                    strOwnerCompanyID = q.Attribute("Value").Value;
        //                    break;
        //                case "CONTENT":
        //                    strContent = q.Attribute("Value").Value;
        //                    break;
        //                case "CHECKSTATE":
        //                    strCheckState = q.Attribute("Value").Value;
        //                    break;
        //                case "OWNERNAME":
        //                    strOwnerName = q.Attribute("Value").Value;
        //                    break;
        //                case "TEL":
        //                    strTel = q.Attribute("Value").Value;
        //                    break;
        //                case "BUSINESSTRIPID":
        //                    strBusinesStripId = q.Attribute("Value").Value;
        //                    break;
        //            }
        //        }
        //        SmtOAPersonOffice doc = new SmtOAPersonOffice();

        //        string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

        //        T_OA_BUSINESSTRIP buip = doc.GetTravelmanagementById(strBusinesStripId);
        //        T_OA_BUSINESSREPORT entity = new T_OA_BUSINESSREPORT();

        //        entity.BUSINESSREPORTID = Guid.NewGuid().ToString();
        //        entity.T_OA_BUSINESSTRIP = buip;
        //        entity.T_OA_BUSINESSTRIP.BUSINESSTRIPID = buip.BUSINESSTRIPID;
        //        entity.CHECKSTATE = "0";
        //        entity.CREATEDATE = buip.UPDATEDATE;
        //        entity.OWNERID = buip.OWNERID;
        //        entity.OWNERNAME = buip.OWNERNAME;
        //        entity.OWNERPOSTID = buip.OWNERPOSTID;
        //        entity.OWNERDEPARTMENTID = buip.OWNERDEPARTMENTID;
        //        entity.OWNERCOMPANYID = buip.OWNERCOMPANYID;
        //        entity.CREATEUSERID = buip.CREATEUSERID;
        //        entity.CREATEUSERNAME = buip.CREATEUSERNAME;
        //        entity.CREATEPOSTID = buip.CREATEPOSTID;
        //        entity.CREATEDEPARTMENTID = buip.CREATEDEPARTMENTID;
        //        entity.CREATECOMPANYID = buip.CREATECOMPANYID;
        //        //entity.CONTENT = buip.CONTENT;
        //        entity.TEL = buip.TEL;

        //        //添加子表数据
        //        List<T_OA_BUSINESSTRIPDETAIL> TravelDetail = doc.GetBusinesstripDetail(strBusinesStripId);
        //        List<T_OA_BUSINESSREPORTDETAIL> ReportDetail = new List<T_OA_BUSINESSREPORTDETAIL>();//出差报告从表
        //        T_OA_TRAVELSOLUTIONS travelsolutions = doc.GetTravelSolutionByCompanyID(entity.OWNERCOMPANYID, ref PlaneObj, ref StandardObj);//出差方案

        //        foreach (var detail in TravelDetail)
        //        {
        //            T_OA_BUSINESSREPORTDETAIL RepotDetailInfo = new T_OA_BUSINESSREPORTDETAIL();
        //            RepotDetailInfo.BUSINESSREPORTDETAILID = Guid.NewGuid().ToString();

        //            RepotDetailInfo.DEPCITY = detail.DEPCITY;//出发城市
        //            RepotDetailInfo.DESTCITY = detail.DESTCITY;//到达城市
        //            RepotDetailInfo.STARTDATE = detail.STARTDATE;//出发时间
        //            RepotDetailInfo.ENDDATE = detail.ENDDATE;//到达时间
        //            RepotDetailInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
        //            RepotDetailInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//是否是开会
        //            RepotDetailInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
        //            RepotDetailInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
        //            RepotDetailInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
        //            ReportDetail.Add(RepotDetailInfo);
        //        }
        //        doc.MissionReportsAdd(entity, ReportDetail);

        //        BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
        //        EngineWS.CustomUserMsg[] user = new EngineWS.CustomUserMsg[1];
        //        user[0] = new EngineWS.CustomUserMsg() { UserID = entity.OWNERID, FormID = entity.BUSINESSREPORTID };
        //        Dictionary<string, string> dic = new Dictionary<string, string>();
        //        dic.Add("BUSINESSTRIPID", buip.BUSINESSTRIPID);
        //        Client.ApplicationMsgTrigger(user, "OA", "T_OA_BUSINESSREPORT", Utility.ObjListToXml<T_OA_BUSINESSREPORT>(entity, dic, "OA", null), EngineWS.MsgType.Task);

        //        return entity.BUSINESSREPORTID;
        //    }
        //    catch (Exception e)
        //    {
        //        string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
        //        Tracer.Debug(abc);
        //        return abc;
        //    }
        //}
        //#endregion

        #region 城市值转换
        private static string GetCityName(string cityvalue)
        {
            try
            {
                CityName.PermissionServiceClient PS = new PermissionServiceClient();
                List<T_SYS_DICTIONARY> ListDnary = PS.GetSysDictionaryByCategory("CITY").ToList();
                var ents = from a in ListDnary
                           where a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region 根据城市值  获取相应的出差补贴
        /// <summary>
        /// 根据城市值  获取相应的出差补贴
        /// </summary>
        /// <param name="CityValue"></param>
        private static T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            SmtOAPersonOffice doc = new SmtOAPersonOffice();
            List<T_OA_AREACITY> citys = new List<T_OA_AREACITY>();
            List<T_OA_AREAALLOWANCE> areaallowance = doc.GetTravleAreaAllowanceByPostValue(postLevel, solutionID, ref citys);//出差补贴

            var q = from ent in areaallowance
                    join ac in citys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == solutionID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }
        #endregion

        //#region 根据传回的XML，添加出差报销信息
        ///// <summary>
        ///// 根据传回的XML，添加出差报销信息
        ///// </summary>
        ///// <param name="xele"></param>
        //private static string TravelReimbursementAdd(IEnumerable<XElement> eGFunc)
        //{

        //}
        //#endregion

        #region 根据传回的XML，添加合同申请信息
        /// <summary>
        /// 根据传回的XML，添加合同申请信息
        /// </summary>
        /// <param name="xele"></param>
        private static string ContractApprovalAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;
                string strContRactCode = string.Empty;
                string strContRactTypeid = string.Empty;
                string strContRactLevel = string.Empty;
                string strPartya = string.Empty;
                string strPartyb = string.Empty;
                string strStartDate = string.Empty;
                string strEndDate = string.Empty;
                string strContRactFlag = string.Empty;
                string strExpirationReminder = string.Empty;
                string strContRactTitle = string.Empty;
                string strContent = string.Empty;
                string strCheckState = string.Empty;
                string strTEL = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                        case "CONTRACTCODE":
                            strContRactCode = q.Attribute("Value").Value;
                            break;
                        case "CONTRACTTYPEID":
                            strContRactTypeid = q.Attribute("Value").Value;
                            break;
                        case "CONTRACTLEVEL":
                            strContRactLevel = q.Attribute("Value").Value;
                            break;
                        case "PARTYA":
                            strPartya = q.Attribute("Value").Value;
                            break;
                        case "PARTYB":
                            strPartyb = q.Attribute("Value").Value;
                            break;
                        case "STARTDATE":
                            strStartDate = q.Attribute("Value").Value;
                            break;
                        case "ENDDATE":
                            strEndDate = q.Attribute("Value").Value;
                            break;
                        case "CONTRACTFLAG":
                            strContRactFlag = q.Attribute("Value").Value;
                            break;
                        case "EXPIRATIONREMINDER":
                            strExpirationReminder = q.Attribute("Value").Value;
                            break;
                        case "CONTRACTTITLE":
                            strContRactTitle = q.Attribute("Value").Value;
                            break;
                        case "CONTENT":
                            strContent = q.Attribute("Value").Value;
                            break;
                        case "CHECKSTATE":
                            strCheckState = q.Attribute("Value").Value;
                            break;
                        case "TEL":
                            strTEL = q.Attribute("Value").Value;
                            break;
                    }
                }
                SmtOADocumentAdmin doc = new SmtOADocumentAdmin();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_CONTRACTAPP entity = new T_OA_CONTRACTAPP();
                entity.CONTRACTAPPID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                doc.ContractApprovalAdd(entity);
                return entity.CONTRACTAPPID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region 根据传回的XML，添加合同查看申请信息
        /// <summary>
        /// 根据传回的XML，添加合同查看申请信息
        /// </summary>
        /// <param name="xele"></param>
        private static string ContractViewapplicationsAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }
                SmtOADocumentAdmin doc = new SmtOADocumentAdmin();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_CONTRACTVIEW entity = new T_OA_CONTRACTVIEW();
                entity.CONTRACTVIEWID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                doc.ContractViewapplicationsAdd(entity);
                return entity.CONTRACTVIEWID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region 根据传回的XML，添加福利标准信息
        /// <summary>
        /// 根据传回的XML，添加福利标准信息
        /// </summary>
        /// <param name="xele"></param>
        private static string WelfareAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                List<T_OA_WELFAREDETAIL> WelfaredDetails = new List<T_OA_WELFAREDETAIL>();
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }
                SmtOADocumentAdmin doc = new SmtOADocumentAdmin();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_WELFAREMASERT entity = new T_OA_WELFAREMASERT();
                entity.WELFAREID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                doc.WelfareStandardAdd(entity, WelfaredDetails);
                return entity.WELFAREID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region 根据传回的XML，添加福利发放信息
        /// <summary>
        /// 根据传回的XML，添加福利发放信息
        /// </summary>
        /// <param name="xele"></param>
        private static string WelfareProvisionAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                List<T_OA_WELFAREDISTRIBUTEDETAIL> WelfareDetails = new List<T_OA_WELFAREDISTRIBUTEDETAIL>();
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }
                SmtOADocumentAdmin doc = new SmtOADocumentAdmin();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_WELFAREDISTRIBUTEMASTER entity = new T_OA_WELFAREDISTRIBUTEMASTER();
                entity.WELFAREDISTRIBUTEMASTERID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                doc.WelfareProvisionAdd(entity, WelfareDetails);
                return entity.WELFAREDISTRIBUTEMASTERID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region 根据传回的XML，添加福利发放撤销信息
        /// <summary>
        /// 根据传回的XML，添加福利发放撤销信息
        /// </summary>
        /// <param name="xele"></param>
        private static string WelfarePaymentWithdrawalAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }
                SmtOADocumentAdmin doc = new SmtOADocumentAdmin();

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_WELFAREDISTRIBUTEUNDO entity = new T_OA_WELFAREDISTRIBUTEUNDO();
                entity.WELFAREDISTRIBUTEUNDOID = Guid.NewGuid().ToString();

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                doc.WelfarePaymentWithdrawalAdd(entity);
                return entity.WELFAREDISTRIBUTEUNDOID;
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        //----------------------------------add by zl
        #region 根据传回的XML，添加保养申请
        /// <summary>
        /// 根据传回的XML，添加保养申请
        /// </summary>
        /// <param name="xele"></param>
        private static string ConservationAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_CONSERVATION entity = new T_OA_CONSERVATION();
                entity.CONSERVATIONID = Guid.NewGuid().ToString();

                SmtOACommonAdmin oa = new SmtOACommonAdmin();
                //VehicleDispatchManageBll oa = new VehicleDispatchManageBll();
                List<T_OA_VEHICLE> vehicleInfoList = oa.GetVehicleInfoList();

                entity.T_OA_VEHICLE = vehicleInfoList[0];

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                int i = oa.AddConserVation_i(entity);
                if (i > 0)
                {
                    return entity.CONSERVATIONID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加保养记录
        /// <summary>
        /// 根据传回的XML，添加保养记录
        /// </summary>
        /// <param name="xele"></param>
        private static string ConservationrecordAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_CONSERVATIONRECORD entity = new T_OA_CONSERVATIONRECORD();
                entity.CONSERVATIONRECORDID = Guid.NewGuid().ToString();

                ConserVationManagementBll cvmbll = new ConserVationManagementBll();
                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
                List<string> bb = new List<string>();
                int iPageCount = 1;
                List<T_OA_CONSERVATION> conservationList = cvmbll.GetInfoList(1, 1, "UPDATEDATE", string.Empty, null, ref iPageCount, strEmployeeID, new List<string>(), "1").ToList();

                entity.T_OA_CONSERVATION = conservationList[0];

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                int i = cvmbll.Add_VCRecord(entity);
                if (i > 0)
                {
                    return entity.CONSERVATIONRECORDID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加维修申请
        /// <summary>
        /// 根据传回的XML，添加维修申请
        /// </summary>
        /// <param name="xele"></param>
        private static string MaintenanceappAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_MAINTENANCEAPP entity = new T_OA_MAINTENANCEAPP();
                entity.MAINTENANCEAPPID = Guid.NewGuid().ToString();

                SmtOACommonAdmin oa = new SmtOACommonAdmin();
                List<T_OA_VEHICLE> vehicleInfoList = oa.GetVehicleInfoList();

                entity.T_OA_VEHICLE = vehicleInfoList[0];

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                MaintenanceAPPBll maBll = new MaintenanceAPPBll();
                bool i = maBll.AddInfo(entity);
                if (i)
                {
                    return entity.MAINTENANCEAPPID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加维修记录
        /// <summary>
        /// 根据传回的XML，添加维修记录
        /// </summary>
        /// <param name="xele"></param>
        private static string MaintenanceapprecordAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_MAINTENANCERECORD entity = new T_OA_MAINTENANCERECORD();
                entity.MAINTENANCERECORDID = Guid.NewGuid().ToString();

                int pageCount = 1;
                MaintenanceAPPBll maBll = new MaintenanceAPPBll();
                List<T_OA_MAINTENANCEAPP> maintenanceappList = maBll.GetInfoList(1, 1, "UPDATEDATE", string.Empty, null, ref  pageCount, strEmployeeID, new List<string>(), "2").ToList();

                entity.T_OA_MAINTENANCEAPP = maintenanceappList[0];
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                int i = maBll.Add_VMRecord(entity);
                if (i > 0)
                {
                    return entity.MAINTENANCERECORDID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加派车单
        /// <summary>
        /// 根据传回的XML，添加派车单
        /// </summary>
        /// <param name="xele"></param>
        private static string VehicledispatchAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_VEHICLEDISPATCH entity = new T_OA_VEHICLEDISPATCH();
                entity.VEHICLEDISPATCHID = Guid.NewGuid().ToString();

                SmtOACommonAdmin oa = new SmtOACommonAdmin();
                List<T_OA_VEHICLE> vehicleInfoList = oa.GetVehicleInfoList();

                entity.T_OA_VEHICLE = vehicleInfoList[0];

                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
                if (vehicleDispatchManagerBll.AddVehicleDispatch(entity) > 0)
                {
                    return entity.VEHICLEDISPATCHID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加派车记录
        /// <summary>
        /// 根据传回的XML，添加派车记录
        /// </summary>
        /// <param name="xele"></param>
        private static string VehicledispatchrecordAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_VEHICLEDISPATCHRECORD entity = new T_OA_VEHICLEDISPATCHRECORD();
                entity.VEHICLEDISPATCHRECORDID = Guid.NewGuid().ToString();

                VehicleDispatchManageBll vehicleDispatchManagerBll = new VehicleDispatchManageBll();
                List<T_OA_VEHICLEDISPATCH> vehicleDispatchList = vehicleDispatchManagerBll.Gets_VDAndDetail(string.Empty, string.Empty, null);

                entity.T_OA_VEHICLEDISPATCHDETAIL = vehicleDispatchList[0].T_OA_VEHICLEDISPATCHDETAIL.FirstOrDefault();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                List<T_OA_VEHICLEDISPATCHRECORD> lst = new List<T_OA_VEHICLEDISPATCHRECORD>();
                lst.Add(entity);
                int i = vehicleDispatchManagerBll.Add_VDRecord(lst);
                if (i > 0)
                {
                    return entity.VEHICLEDISPATCHRECORDID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加用车申请
        /// <summary>
        /// 根据传回的XML，添加用车申请
        /// </summary>
        /// <param name="xele"></param>
        private static string VehicleuseappAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_VEHICLEUSEAPP entity = new T_OA_VEHICLEUSEAPP();
                entity.VEHICLEUSEAPPID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                VehicleUseAppManageBll vehicleUseManagerBll = new VehicleUseAppManageBll();
                if (vehicleUseManagerBll.AddVehicleUseApp(entity))
                {
                    return entity.VEHICLEUSEAPPID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加房源信息发布
        /// <summary>
        /// 根据传回的XML，添加房源信息发布
        /// </summary>
        /// <param name="xele"></param>
        private static string HouseinfoissuanceAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_HOUSEINFOISSUANCE entity = new T_OA_HOUSEINFOISSUANCE();
                HouseInfoManagerBll houseBll = new HouseInfoManagerBll();
                int pageCount = 1;
                List<T_OA_HOUSEINFO> ent = houseBll.QueryWithPaging(1, 1, string.Empty, string.Empty, null, ref  pageCount, strEmployeeID, "T_OA_HOUSEINFO").ToList();
                List<T_OA_HOUSELIST> hlist = ent[0].T_OA_HOUSELIST.ToList();
                entity.T_OA_HOUSELIST = ent.ToList()[0].T_OA_HOUSELIST;

                entity.ISSUANCEID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                List<T_OA_DISTRIBUTEUSER> distributeLists = new List<T_OA_DISTRIBUTEUSER>();

                HouseInfoIssuanceBll issuanceBll = new HouseInfoIssuanceBll();
                bool i = issuanceBll.AddHouseInfoIssuance(entity, hlist, distributeLists);
                if (i)
                {
                    return entity.ISSUANCEID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加租房申请
        /// <summary>
        /// 根据传回的XML，添加租房申请
        /// </summary>
        /// <param name="xele"></param>
        private static string HireappAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_HIREAPP entity = new T_OA_HIREAPP();
                HouseInfoManagerBll houseBll = new HouseInfoManagerBll();
                int pageCount = 1;
                List<T_OA_HOUSEINFO> ent = houseBll.QueryWithPaging(1, 1, string.Empty, string.Empty, null, ref  pageCount, strEmployeeID, "T_OA_HOUSEINFO").ToList();
                List<T_OA_HOUSELIST> hlist = ent[0].T_OA_HOUSELIST.ToList();
                entity.T_OA_HOUSELIST = hlist[0];

                entity.HIREAPPID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                List<T_OA_DISTRIBUTEUSER> distributeLists = new List<T_OA_DISTRIBUTEUSER>();

                HouseHireAppManagementBll houseHireAppBll = new HouseHireAppManagementBll();
                if (!houseHireAppBll.IsHired(entity.T_OA_HOUSELIST, entity.OWNERID))
                {
                    houseHireAppBll.AddHireApp(entity);
                    return entity.HIREAPPID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加机构表
        /// <summary>
        /// 根据传回的XML，添加机构表
        /// </summary>
        /// <param name="xele"></param>
        private static string OrganizationAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_ORGANIZATION entity = new T_OA_ORGANIZATION();

                entity.ORGANIZATIONID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                OrganManagementBll organBll = new OrganManagementBll();
                if (organBll.AddOrgan(entity, null))
                {
                    return entity.ORGANIZATIONID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加证照印章外借记录
        /// <summary>
        /// 根据传回的XML，添加证照印章外借记录
        /// </summary>
        /// <param name="xele"></param>
        private static string LicenseuserAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                T_OA_LICENSEUSER entity = new T_OA_LICENSEUSER();

                entity.LICENSEUSERID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;

                LicenseBorrowBll licenseBorrowBll = new LicenseBorrowBll();
                if (licenseBorrowBll.AddLicenseBorrow(entity))
                {
                    return entity.LICENSEUSERID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加证照印章表
        /// <summary>
        /// 根据传回的XML，添加证照印章表
        /// </summary>
        /// <param name="xele"></param>
        private static string LicensemasterAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_LICENSEMASTER> lm = new List<T_OA_LICENSEMASTER>();
                T_OA_LICENSEMASTER entity = new T_OA_LICENSEMASTER();

                entity.LICENSEMASTERID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                OrganManagementBll organBll = new OrganManagementBll();
                if (organBll.AddOrgan(null, lm))
                {
                    return entity.LICENSEMASTERID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        //--------------------------------------Add by Lezy

        #region 根据传回的XML，添加员工调查方案表
        /// <summary>
        /// 根据传回的XML，添加员工调查方案表
        /// </summary>
        /// <param name="master"></param>
        private static string EmployeeSurveyMasterAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_REQUIREMASTER> lm = new List<T_OA_REQUIREMASTER>();
                T_OA_REQUIREMASTER entity = new T_OA_REQUIREMASTER();

                entity.REQUIREMASTERID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                SmtOAPersonOffice masterBll = new SmtOAPersonOffice();
                if (masterBll.AddEmployeeSurvey(entity))
                {
                    return entity.REQUIREMASTERID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加员工调查发布申请表
        /// <summary>
        /// 根据传回的XML，添加员工调查发布申请表
        /// </summary>
        /// <param name="master"></param>
        private static string EmployeeSurveyRequireDistributeAdd(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_REQUIREDISTRIBUTE> lm = new List<T_OA_REQUIREDISTRIBUTE>();
                T_OA_REQUIREDISTRIBUTE entity = new T_OA_REQUIREDISTRIBUTE();

                entity.REQUIREDISTRIBUTEID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                SmtOAPersonOffice requireBll = new SmtOAPersonOffice();
                if (requireBll.Add_ESurveyResult(entity) == 1)
                {
                    return entity.REQUIREDISTRIBUTEID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加员工满意度调查方案表
        /// <summary>
        /// 根据传回的XML，添加员工满意度调查方案表
        /// </summary>
        /// <param name="master"></param>
        private static string EmployeeSatisfactionAdd(IEnumerable<XElement> eGFunc)
        {
            return "";
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_SATISFACTIONMASTER> lm = new List<T_OA_SATISFACTIONMASTER>();
                T_OA_SATISFACTIONMASTER entity = new T_OA_SATISFACTIONMASTER();

                entity.SATISFACTIONMASTERID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                SmtOAPersonOffice requireBll = new SmtOAPersonOffice();
                bool bl = false;
                bl = requireBll.AddSatisfactionsMaster(entity);
                if (bl == true)
                {
                    return entity.SATISFACTIONMASTERID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }
        #endregion

        #region 根据传回的XML，添加员工满意度调查发布申请表
        /// <summary>
        /// 根据传回的XML，添加员工满意度调查发布申请表
        /// </summary>
        /// <param name="master"></param>
        private static string EmployeeSatisfactionDistributeAdd(IEnumerable<XElement> eGFunc)
        {
            return "";
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_SATISFACTIONDISTRIBUTE> lm = new List<T_OA_SATISFACTIONDISTRIBUTE>();
                T_OA_SATISFACTIONDISTRIBUTE entity = new T_OA_SATISFACTIONDISTRIBUTE();

                entity.SATISFACTIONDISTRIBUTEID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                SmtOAPersonOffice requireBll = new SmtOAPersonOffice();
                if (requireBll.Add_SSurveyResult(entity) == 1)
                {
                    return entity.SATISFACTIONDISTRIBUTEID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        #region 根据传回的XML，添加员工满意度调查申请表
        /// <summary>
        /// 根据传回的XML，添加员工满意度调查申请表
        /// </summary>
        /// <param name="master"></param>
        private static string EmployeeSatisfactionRequireAdd(IEnumerable<XElement> eGFunc)
        {
            return "";
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return "";
                }
                string strEmployeeID = string.Empty;
                string strOwnerID = string.Empty;
                string strOwnerPostID = string.Empty;
                string strOwnerDepartmentID = string.Empty;
                string strOwnerCompanyID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "CREATEUSERID":
                            strEmployeeID = q.Attribute("Value").Value;
                            break;
                        case "OWNERID":
                            strOwnerID = q.Attribute("Value").Value;
                            break;
                        case "OWNERPOSTID":
                            strOwnerPostID = q.Attribute("Value").Value;
                            break;
                        case "OWNERDEPARTMENTID":
                            strOwnerDepartmentID = q.Attribute("Value").Value;
                            break;
                        case "OWNERCOMPANYID":
                            strOwnerCompanyID = q.Attribute("Value").Value;
                            break;
                    }
                }

                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");

                List<T_OA_SATISFACTIONREQUIRE> lm = new List<T_OA_SATISFACTIONREQUIRE>();
                T_OA_SATISFACTIONREQUIRE entity = new T_OA_SATISFACTIONREQUIRE();

                entity.SATISFACTIONREQUIREID = Guid.NewGuid().ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                lm.Add(entity);

                SmtOAPersonOffice requireBll = new SmtOAPersonOffice();
                bool bl = false;
                bl = requireBll.Add_SSurveyApp(entity);
                if (bl == true)
                {
                    return entity.SATISFACTIONREQUIREID;
                }
                else
                {
                    string err = "Error";
                    return err;
                }
            }
            catch (Exception e)
            {
                string abc = "<OA>Message=[" + e.Message + "]" + "<OA>Source=[" + e.Source + "]<OA>StackTrace=[" + e.StackTrace + "]<OA>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;
            }
        }

        #endregion

        //--------------------------------------------End

        // 在此处添加更多操作并使用 [OperationContract] 标记它们

        public void CallApplicationService(string strXml)
        {
            throw new NotImplementedException();
        }
    }
}
