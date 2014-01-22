using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using Smt.Global.IContract;
using System.Xml.Linq;
using System.IO;
using SMT_HRM_EFModel;
using SMT.HRM.BLL.Common;
using SMT.Foundation.Log;
using System.Reflection;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;

namespace SMT.HRM.Services
{
    // 注意: 如果更改此处的类名 "EngineTriggerService"，也必须更新 Web.config 中对 "EngineTriggerService" 的引用。
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EngineTriggerService : IEventTriggerProcess
    {

        #region IEventTriggerProcess 成员

        public void EventTriggerProcess(string param)
        {
            try
            {
                string strXml = string.Empty;
                if (!string.IsNullOrWhiteSpace(param))
                {
                    param = param.Trim();
                    if (param.IndexOf("<?xml version=\"1.0\"") < 0)
                    {
                        strXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                "<WcfFuncParamter>" + param +
                                "</WcfFuncParamter>";
                    }
                    else
                    {
                        strXml = param;
                    }
                }
                else
                {
                    throw new Exception("参数为空");
                }
                Tracer.Debug("HR 提醒触发："+strXml);
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
                XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var eGFunc = from c in xele.Descendants("Para")
                             select c;
                string funcName = eGFunc.FirstOrDefault().Attribute("FuncName").Value;
                Tracer.Debug("模块名：" + funcName);
                switch (funcName)
                {
                    case "AbnormRecordCheckAlarm":
                        AbnormRecordCheckAlarmTrigger(eGFunc);
                        break;
                    case "AsignAttendanceSolution":
                        AsignAttendSolTrigger(eGFunc);
                        break;
                    case "CreateLevelDayCountWithAll":
                        CreateLevelDayCountTrigger(eGFunc);
                        break;
                    case "CalculateEmployeeAttendanceMonthly":
                        CalculateAttendMonthlyTrigger(eGFunc);
                        break;
                    case "UpdateEmployeeWorkAgeByID":
                        UpdateEmployeeWorkAgeByID(eGFunc);
                        break;
                    case "SalarySolutionRemind":
                        SalarySolutionRemindTrigger(eGFunc);
                        break;
                    case "EmployeeCheckRemind":
                        EmployeeCheckRemindTrigger(eGFunc);
                        break;
                    case "EmployeeContractRemind":
                        EmployeeContractRemindTrigger(eGFunc);
                        break;
                    case "T_HR_ORGANAZITIONCHANGE":
                        ORGCHANGERemindTrigger(eGFunc);
                        break;
                    default:
                        break;
                }
                
            }
            catch(Exception e)
            {
                Tracer.Debug("-------------"+e.Message+"-------------");
            }
        }

        #region 考勤定时服务触发私有方法
        /// <summary>
        /// 考勤记录定时生成触发
        /// </summary>
        /// <param name="eGFunc"></param>
        private void AsignAttendSolTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strAttendSolAsignId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "ATTENDANCESOLUTIONASIGNID")
                {
                    strAttendSolAsignId = item.Attribute("Value").Value;
                    break;
                }
            }

            AttendanceService svcAttend = new AttendanceService();
            svcAttend.AsignAttendanceSolutionWithAll();
        }

        /// <summary>
        /// 考勤异常消息提醒
        /// </summary>
        /// <param name="eGFunc"></param>
        private void AbnormRecordCheckAlarmTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strEmployeeId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "EMPLOYEEID")
                {
                    strEmployeeId = item.Attribute("Value").Value;
                    break;
                }
            }

            AttendanceService svcAttend = new AttendanceService();
            svcAttend.AbnormRecordCheckAlarm(strEmployeeId);
        }

        /// <summary>
        /// 薪资发放日提醒定时生成触发
        /// </summary>
        /// <param name="eGFunc"></param>
        private void SalarySolutionRemindTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strAttendSolAsignId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "SALARYSOLUTIONID")
                {
                    strAttendSolAsignId = item.Attribute("Value").Value;
                    break;
                }
            }
            SalaryService svcSalarySol = new SalaryService();
            svcSalarySol.TimingPay(svcSalarySol.GetSalarySolutionByID(strAttendSolAsignId));
        }

        /// <summary>
        /// 更新工龄
        /// </summary>
        /// <param name="eGFunc"></param>
        private void UpdateEmployeeWorkAgeByID(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string companyID = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "COMPANYID")
                {
                    companyID = item.Attribute("Value").Value;
                    break;
                }
            }

            PersonnelService svcPersonnel = new PersonnelService();
            svcPersonnel.UpdateEmployeeWorkAgeByID(companyID);
        }
        /// <summary>
        /// 员工带薪假实时更新触发
        /// </summary>
        /// <param name="eGFunc"></param>
        private void CreateLevelDayCountTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strAttendSolAsignId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "ATTENDANCESOLUTIONASIGNID")
                {
                    strAttendSolAsignId = item.Attribute("Value").Value;
                    break;
                }
            }

            AttendanceService svcAttend = new AttendanceService();
            svcAttend.CreateLevelDayCountWithAll();
        }

        /// <summary>
        /// 每月考勤结算触发
        /// </summary>
        /// <param name="eGFunc"></param>
        private void CalculateAttendMonthlyTrigger(IEnumerable<XElement> eGFunc)
        {
            if (eGFunc.Count() == 0)
            {
                return;
            }

            string strIsCurrentMonth = string.Empty;
            string strAssignedObjectType = string.Empty;
            string strAssignedObjectId = string.Empty;

            foreach (var item in eGFunc)
            {
                if (item.Attribute("Name").Value == "ISCURRENTMONTH")
                {
                    strIsCurrentMonth = item.Attribute("Value").Value;
                }
                else if (item.Attribute("Name").Value == "ASSIGNEDOBJECTTYPE")
                {
                    strAssignedObjectType = item.Attribute("Value").Value;
                }
                else if (item.Attribute("Name").Value == "ASSIGNEDOBJECTID")
                {
                    strAssignedObjectId = item.Attribute("Value").Value;
                }
            }

            AttendanceService svcAttend = new AttendanceService();
            svcAttend.CalculateEmployeeAttendanceMonthly(strIsCurrentMonth, strAssignedObjectType, strAssignedObjectId);
        }
        /// <summary>
        /// 员工转正提醒
        /// </summary>
        /// <param name="eGFunc"></param>
        private void EmployeeCheckRemindTrigger(IEnumerable<XElement> eGFunc)
        {
            try
            {
                if (eGFunc.Count() == 0)
                {
                    return;
                }

                string strId = string.Empty;

                foreach (var item in eGFunc)
                {
                    if (item.Attribute("Name").Value == "BEREGULARID")
                    {
                        strId = item.Attribute("Value").Value;
                        break;
                    }
                }

                PersonnelService svcPersonnel = new PersonnelService();
                T_HR_EMPLOYEEENTRY entry = svcPersonnel.GetEmployeeEntryByEmployeeID(strId);
                if (entry != null)
                {
                    string strMsg = "";
                    T_HR_EMPLOYEECHECK employeeCheck = new T_HR_EMPLOYEECHECK();
                    employeeCheck.BEREGULARID = Guid.NewGuid().ToString();
                    employeeCheck.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID = entry.T_HR_EMPLOYEE.EMPLOYEEID;
                    employeeCheck.EMPLOYEECODE = entry.T_HR_EMPLOYEE.EMPLOYEECODE;
                    employeeCheck.EMPLOYEENAME = entry.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    employeeCheck.PROBATIONPERIOD = entry.PROBATIONPERIOD;
                    employeeCheck.REPORTDATE = entry.ENTRYDATE;
                    employeeCheck.ONDUTYDATE = entry.ONPOSTDATE;
                    employeeCheck.OWNERID = entry.OWNERID;
                    employeeCheck.OWNERCOMPANYID = entry.OWNERCOMPANYID;
                    employeeCheck.CREATEUSERID = entry.CREATEUSERID;
                    employeeCheck.CHECKSTATE = "0";
                    svcPersonnel.EmployeeCheckAdd(employeeCheck,ref strMsg);
                    svcPersonnel.EmployeeCheckAlarm(employeeCheck);
                }
            }
            catch (Exception e)
            {
                Tracer.Debug("员工转正提醒出现错误" + e.ToString());
                throw e;
            }

        }

        /// <summary>
        /// 合同到期提醒
        /// </summary>
        /// <param name="eGFunc"></param>
        private void EmployeeContractRemindTrigger(IEnumerable<XElement> eGFunc)
        {
            try
            {
                SMT.Foundation.Log.Tracer.Debug("员工合同到期开始" );
                if (eGFunc.Count() == 0)
                {
                    return;
                }

                string strId = string.Empty;

                foreach (var item in eGFunc)
                {
                    if (item.Attribute("Name").Value == "EMPLOYEECONTACTID")
                    {
                        strId = item.Attribute("Value").Value;
                        break;
                    }
                }
                PersonnelService svcPersonnel = new PersonnelService();
                
                T_HR_EMPLOYEECONTRACT entry = svcPersonnel.GetEmployeeContractByID(strId);
    
                if (entry != null)
                {
                    if (entry.T_HR_EMPLOYEE!=null && entry.T_HR_EMPLOYEE.EMPLOYEESTATE != "2")
                    {
                        string strMsg = "";
                        T_HR_EMPLOYEECONTRACT employeeContract = new T_HR_EMPLOYEECONTRACT();
                        employeeContract.EMPLOYEECONTACTID = Guid.NewGuid().ToString();
                        employeeContract.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                        employeeContract.T_HR_EMPLOYEE.EMPLOYEEID = entry.T_HR_EMPLOYEE.EMPLOYEEID;
                        employeeContract.FROMDATE = entry.FROMDATE;
                        employeeContract.TODATE = entry.TODATE;
                        employeeContract.ENDDATE = entry.ENDDATE;
                        employeeContract.CONTACTPERIOD = entry.CONTACTPERIOD;
                        employeeContract.CONTACTCODE = entry.CONTACTCODE;
                        employeeContract.ATTACHMENT = entry.ATTACHMENT;
                        employeeContract.ATTACHMENTPATH = entry.ATTACHMENTPATH;
                        employeeContract.ALARMDAY = entry.ALARMDAY;
                        employeeContract.CHECKSTATE = "0";
                        employeeContract.EDITSTATE = "0";
                        employeeContract.ISSPECIALCONTRACT = entry.ISSPECIALCONTRACT;
                        employeeContract.REASON = entry.REASON;
                        employeeContract.REMARK = entry.REMARK;
                        employeeContract.OWNERID = entry.OWNERID;
                        employeeContract.OWNERPOSTID = entry.OWNERPOSTID;
                        employeeContract.OWNERDEPARTMENTID = entry.OWNERDEPARTMENTID;
                        employeeContract.OWNERCOMPANYID = entry.OWNERCOMPANYID;
                        employeeContract.CREATECOMPANYID = entry.CREATECOMPANYID;
                        employeeContract.CREATEDATE = System.DateTime.Now;
                        employeeContract.CREATEUSERID = entry.CREATEUSERID;
                        employeeContract.CREATEPOSTID = entry.CREATEPOSTID;
                        employeeContract.CREATEDEPARTMENTID = entry.CREATEDEPARTMENTID;
                        employeeContract.CREATECOMPANYID = entry.CREATECOMPANYID;
                        employeeContract.UPDATEUSERID = entry.UPDATEUSERID;
                        employeeContract.UPDATEDATE = entry.UPDATEDATE;
                        //获取附件问题
                        //SMT.SaaS.BLLCommonServices PermClient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();


                        svcPersonnel.EmployeeContractAdd(employeeContract, ref strMsg);
                        svcPersonnel.EmployeeContractAlarm(employeeContract);
                    }
                    else
                    {
                        var EmployeeEnt = entry.T_HR_EMPLOYEE;
                        SMT.Foundation.Log.Tracer.Debug("员工" + EmployeeEnt.EMPLOYEEID + "的在EMPLOYEESTATE:" + EmployeeEnt.EMPLOYEESTATE
                            + ",不自动生成合同。");
                    }
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("员工合同到期没有获取到合同信息");
                }
                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工合同到期提醒出现错误:"+ex.ToString());
            }
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eGFunc"></param>
        private void ORGCHANGERemindTrigger(IEnumerable<XElement> eGFunc)
        {
            OrganizationService orclient = new OrganizationService();
            orclient.OrgChange();
        }
        #endregion

        #endregion
    }
}
