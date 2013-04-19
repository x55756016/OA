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
using SMT.HRM.CustomModel;
namespace SMT.HRM.Services
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EngineService : IApplicationService
    {

        public void CallApplicationService(string strXml)
        {
            Tracer.Debug(strXml);

            string strReturn = string.Empty;
            Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(strXml);
            XElement xele = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
            var eGFunc = from c in xele.Descendants("Para")
                         select c;
            string funcName = eGFunc.FirstOrDefault().Attribute("TableName").Value;

            ModelNames modelName = (ModelNames)Enum.Parse(typeof(ModelNames), funcName);
            switch (modelName)
            {
                case ModelNames.T_HR_PENSIONMASTER:
                    AddPensionMaster(eGFunc);
                    break;
                //case "T_HR_PENSIONMASTER":
                //    AddPensionMaster(eGFunc);
                //    break;
                default:
                    break;
            }
        }
        #region IApplicationService 成员


        public string CallWaitAppService(string strXml)
        {

            try
            {
                //生成社包单的业务逻辑
                Tracer.Debug("from engine : " + strXml);

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
                //switch (funcName)
                //{
                //    case "T_HR_PENSIONMASTER":
                //        paraValue = AddPensionMaster(eGFunc);
                //        break;
                //    case "":
                //        break;
                //    default:
                //        break;
                //}
                foreach (var item in eGFunc.Attributes("TableName"))
                {
                    SMT.Foundation.Log.Tracer.Debug("表单的名字为："+ item.Value);
                    if (item.Value == ModelNames.T_HR_PENSIONMASTER.ToString())
                    {
                        paraValue = AddPensionMaster(eGFunc);
                        formID = "PENSIONMASTERID";
                        modelCode = ModelNames.T_HR_PENSIONMASTER.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_HR_EMPLOYEECONTRACT.ToString())
                    {
                        
                        paraValue = AddEmployeeContract(eGFunc);
                        formID = "EMPLOYEECONTACTID";
                        modelCode = ModelNames.T_HR_EMPLOYEECONTRACT.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_HR_SALARYARCHIVE.ToString())
                    {
                        paraValue = AddEmployeeSalaryArchive(eGFunc);
                        formID = "SALARYARCHIVEID";
                        modelCode = ModelNames.T_HR_SALARYARCHIVE.ToString();
                        break;
                    }
                    if (item.Value == "T_HR_LEFTOFFICECONFIRM")
                    {
                        paraValue = AddLeftOfficeConfirm(eGFunc);
                        formID = "CONFIRMID";
                        modelCode = "T_HR_LEFTOFFICECONFIRM";
                        break;
                    }
                    if (item.Value == ModelNames.T_HR_EMPLOYEESALARYRECORDPAYMENT.ToString())
                    {
                        paraValue = AddSalaryRecordMaster(eGFunc);
                        formID = "MONTHLYBATCHID";
                        modelCode = ModelNames.T_HR_EMPLOYEESALARYRECORDPAYMENT.ToString();
                        break;
                    }
                    if (item.Value == ModelNames.T_HR_EMPLOYEEENTRY.ToString())
                    {
                        paraValue = AddEmployeeEntry(eGFunc);
                        formID = "EMPLOYEEENTRYID";
                        modelCode = ModelNames.T_HR_EMPLOYEEENTRY.ToString();
                        break;
                    }
                }
                //  *****************************
                strReturn = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                  "<System>" +
                                  "  <NewFormID>" + paraValue + "</NewFormID>" +
                                  "<IsNewFlow>0</IsNewFlow>" +
                                  "<Attribute  Name=\"" + formID + "\" DataValue=\"" + paraValue + "\"></Attribute>" +
                                      "<Attribute  Name=\"SYSTEMCODE\" DataValue=\"HR\"></Attribute>" +
                                       "<Attribute  Name=\"MODELCODE\" DataValue=\"" + modelCode + "\"></Attribute>" +
                                    "</System>";
                Tracer.Debug(strReturn);
                return strReturn;
            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

        public string CallAppService(string strXml)
        {

            try
            {
                //生成薪资单的业务逻辑
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
                //switch (funcName)
                //{
                //    case "T_HR_PENSIONMASTER":
                //        paraValue = AddPensionMaster(eGFunc);
                //        break;
                //    case "":
                //        break;
                //    default:
                //        break;
                //}
                foreach (var item in eGFunc.Attributes("TableName"))
                {
                    if (item.Value == ModelNames.T_HR_EMPLOYEESALARYRECORDPAYMENT.ToString())
                    {
                        paraValue = AddSalaryRecordMaster(eGFunc);
                        formID = "EMPLOYEESALARYRECORDID";
                        modelCode = ModelNames.T_HR_EMPLOYEESALARYRECORDPAYMENT.ToString();
                        break;
                    }
                }
                //  *****************************
                strReturn = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                                  "<System>" +
                                  "  <NewFormID>" + paraValue + "</NewFormID>" +
                                  "<IsNewFlow>0</IsNewFlow>" +
                                  "<Attribute  Name=\"" + formID + "\" DataValue=\"" + paraValue + "\"></Attribute>" +
                                      "<Attribute  Name=\"SYSTEMCODE\" DataValue=\"HR\"></Attribute>" +
                                       "<Attribute  Name=\"MODELCODE\" DataValue=\"" + modelCode + "\"></Attribute>" +
                                    "</System>";
                Tracer.Debug(strReturn);
                return strReturn;
            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

        #endregion

        /// <summary>
        /// 根据传回的XML,添加薪资记录
        /// </summary>
        /// <param name="xele"></param>
        private static string AddSalaryRecordMaster(IEnumerable<XElement> eGFunc)
        {
            string strMONTHLYBATCHID = string.Empty;
            try
            {

                if (eGFunc.Count() == 0)
                {
                    return Guid.NewGuid().ToString();
                }

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "MONTHLYBATCHID":
                            strMONTHLYBATCHID = q.Attribute("Value").Value;
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }
            return strMONTHLYBATCHID;
        }

        /// <summary>
        /// 根据传回的XML，添加员工社保档案
        /// </summary>
        /// <param name="xele"></param>
        private static string AddPensionMaster(IEnumerable<XElement> eGFunc)
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
                        case "EMPLOYEEID":
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


                PersonnelService ser = new PersonnelService();
                string employeeid = strEmployeeID.Replace("{", "").Replace("}", "");
                //注释获取社保的原因：如果兼职的则存在，这样待办还是有，但实际社保却没有添加
                //var pensionTmp = ser.GetPensionMasterByEmployeeID(employeeid);
                //if (pensionTmp == null)
                //{

                    T_HR_PENSIONMASTER entity = new T_HR_PENSIONMASTER();
                    entity.PENSIONMASTERID = Guid.NewGuid().ToString();
                    entity.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    entity.T_HR_EMPLOYEE.EMPLOYEEID = employeeid;
                    //  entity.T_HR_PENSIONDETAIL = new T_HR_PENSIONDETAIL();

                    entity.CARDID = string.Empty;
                    entity.COMPUTERNO = string.Empty;
                    entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                    entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
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
                    T_HR_EMPLOYEE emp = ser.GetEmployeeByID(entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    if (emp != null)
                    {
                        entity.OWNERID = emp.EMPLOYEEID;
                        entity.OWNERPOSTID = emp.OWNERPOSTID;
                        entity.OWNERDEPARTMENTID = emp.OWNERDEPARTMENTID;
                        entity.OWNERCOMPANYID = emp.OWNERCOMPANYID;
                    }
                    ser.PensionMasterAdd(entity, ref strMsg);
                    return entity.PENSIONMASTERID;
                //}
                //else
                //{
                //    return pensionTmp.PENSIONMASTERID;
                //}
            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        /// <summary>
        /// 根据传回的xml生成员工合同
        /// </summary>
        /// <param name="eGFunc"></param>
        /// <returns></returns>
        private static string AddEmployeeContract(IEnumerable<XElement> eGFunc)
        {
            SMT.Foundation.Log.Tracer.Debug("开始添加员工合同信息");
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
                        case "EMPLOYEEID":
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


                PersonnelService ser = new PersonnelService();

                T_HR_EMPLOYEECONTRACT entity = new T_HR_EMPLOYEECONTRACT();
                entity.EMPLOYEECONTACTID = Guid.NewGuid().ToString();
                entity.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                entity.T_HR_EMPLOYEE.EMPLOYEEID = strEmployeeID.Replace("{", "").Replace("}", "");
                //  entity.T_HR_PENSIONDETAIL = new T_HR_PENSIONDETAIL();
                SMT.Foundation.Log.Tracer.Debug("合同员工ID:" + entity.T_HR_EMPLOYEE.EMPLOYEEID);

                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
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
                PersonnelService ps = new PersonnelService();
                T_HR_EMPLOYEE emp = ps.GetEmployeeByID(entity.T_HR_EMPLOYEE.EMPLOYEEID);
                if (emp != null)
                {
                    entity.OWNERID = emp.EMPLOYEEID;
                    entity.OWNERPOSTID = emp.OWNERPOSTID;
                    entity.OWNERDEPARTMENTID = emp.OWNERDEPARTMENTID;
                    entity.OWNERCOMPANYID = emp.OWNERCOMPANYID;
                    ser.EmployeeContractAdd(entity, ref strMsg);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("引擎触发员工合同记录失败");
                }
                SMT.Foundation.Log.Tracer.Debug("合同工ID:"+entity.EMPLOYEECONTACTID);
                SMT.Foundation.Log.Tracer.Debug("开始添加员工信息："+emp.EMPLOYEECNAME);
                
                SMT.Foundation.Log.Tracer.Debug("添加完员工信息的结果："+strMsg);
                return entity.EMPLOYEECONTACTID;

            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        /// <summary>
        /// 根据传回的xml生成薪资档案
        /// </summary>
        /// <param name="eGFunc"></param>
        /// <returns></returns>
        private static string AddEmployeeSalaryArchive(IEnumerable<XElement> eGFunc)
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
                        case "EMPLOYEEID":
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


                SalaryService ser = new SalaryService();

                T_HR_SALARYARCHIVE entity = new T_HR_SALARYARCHIVE();
                entity.SALARYARCHIVEID = Guid.NewGuid().ToString();
                entity.EMPLOYEEID = strEmployeeID.Replace("{", "").Replace("}", "");

                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strOwnerID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                PersonnelService ps = new PersonnelService();
                T_HR_EMPLOYEEPOST emp = ps.GetEmployeePostActivedByEmployeeID(entity.EMPLOYEEID);
                if (emp != null)
                {
                    entity.EMPLOYEECODE = emp.T_HR_EMPLOYEE.EMPLOYEECODE;
                    entity.EMPLOYEENAME = emp.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    
                    entity.OWNERID = emp.T_HR_EMPLOYEE.EMPLOYEEID;
                    entity.OWNERPOSTID = emp.T_HR_EMPLOYEE.OWNERPOSTID;
                    entity.OWNERDEPARTMENTID = emp.T_HR_EMPLOYEE.OWNERDEPARTMENTID;
                    entity.OWNERCOMPANYID = emp.T_HR_EMPLOYEE.OWNERCOMPANYID;
                    entity.POSTLEVEL = emp.POSTLEVEL;
                    entity.EMPLOYEEPOSTID = emp.EMPLOYEEPOSTID;
                }
                ser.SalaryArchiveAdd(entity);
                return entity.SALARYARCHIVEID;

            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }
        /// <summary>
        /// 根据传回的xml生成员工离职确认
        /// </summary>
        /// <param name="eGFunc"></param>
        /// <returns></returns>
        private static string AddLeftOfficeConfirm(IEnumerable<XElement> eGFunc)
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
                string strDimissionID = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "EMPLOYEEID":
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
                        case "DIMISSIONID":
                            strDimissionID = q.Attribute("Value").Value;
                            break;
                    }
                }


                PersonnelService ser = new PersonnelService();

                T_HR_LEFTOFFICECONFIRM entity = new T_HR_LEFTOFFICECONFIRM();
                entity.CONFIRMID = Guid.NewGuid().ToString();
                entity.T_HR_LEFTOFFICE = new T_HR_LEFTOFFICE();
                //entity.T_HR_LEFTOFFICE.DIMISSIONID = strDimissionID.Replace("{", "").Replace("}", "");
                //  entity.T_HR_PENSIONDETAIL = new T_HR_PENSIONDETAIL();
                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = strEmployeeID;
                entity.OWNERPOSTID = strOwnerPostID;
                entity.OWNERDEPARTMENTID = strOwnerDepartmentID;
                entity.OWNERCOMPANYID = strOwnerCompanyID;
                entity.CREATEUSERID = strOwnerID;
                entity.CREATEPOSTID = strOwnerPostID;
                entity.CREATEDEPARTMENTID = strOwnerDepartmentID;
                entity.CREATECOMPANYID = strOwnerCompanyID;
                T_HR_LEFTOFFICE leftOffice = ser.GetLeftOfficeByEmployeeIDAndPostID(strEmployeeID, strOwnerPostID);
                if (leftOffice != null)
                {
                    entity.LEFTOFFICECATEGORY = leftOffice.LEFTOFFICECATEGORY;
                    entity.LEFTOFFICEDATE = leftOffice.LEFTOFFICEDATE;
                    entity.LEFTOFFICEREASON = leftOffice.LEFTOFFICEREASON;
                    entity.REMARK = leftOffice.REMARK;
                    entity.T_HR_LEFTOFFICE.DIMISSIONID = leftOffice.DIMISSIONID;
                    entity.EMPLOYEECNAME = leftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    entity.APPLYDATE = leftOffice.APPLYDATE;
                    entity.EMPLOYEEPOSTID = leftOffice.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID;
                    V_EMPLOYEEPOST eps = ser.GetEmployeeDetailByID(leftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                    entity.EMPLOYEEID = leftOffice.T_HR_EMPLOYEE.EMPLOYEEID;
                    entity.CREATEUSERID = leftOffice.T_HR_EMPLOYEE.CREATEUSERID;
                    foreach (T_HR_EMPLOYEEPOST ep in eps.EMPLOYEEPOSTS)
                    {
                        if (ep.EMPLOYEEPOSTID == leftOffice.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID)
                        {
                            entity.OWNERID = leftOffice.T_HR_EMPLOYEE.EMPLOYEEID;
                            entity.OWNERPOSTID = ep.T_HR_POST.POSTID;
                            entity.OWNERDEPARTMENTID = ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                            entity.OWNERCOMPANYID = ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                        }
                    }
                }



                ser.LeftOfficeConfirmAdd(entity);
                return entity.CONFIRMID;

            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

        /// <summary>
        /// 根据传回的xml生成员工入职
        /// </summary>
        /// <param name="eGFunc"></param>
        /// <returns></returns>
        private static string AddEmployeeEntry(IEnumerable<XElement> eGFunc)
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
                string Name = string.Empty;
                string Idnumber = string.Empty;
                string sex = string.Empty;
                string isAcceptemploied = string.Empty;
                string createuserid = string.Empty;

                foreach (var q in eGFunc)
                {
                    string strName = q.Attribute("Name").Value;
                    switch (strName)
                    {
                        case "EMPLOYEEID":
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
                        case "NAME":
                            Name = q.Attribute("Value").Value;
                            break;
                        case "IDCARDNUMBER":
                            Idnumber = q.Attribute("Value").Value;
                            break;
                        case "SEX":
                            sex = q.Attribute("Value").Value;
                            break;
                        case "ISACCEPTEMPLOIED":
                            isAcceptemploied = q.Attribute("Value").Value;
                            break;
                        case "CREATEUSERID":
                            createuserid = q.Attribute("Value").Value;
                            break;

                    }
                }
                //if (isAcceptemploied != "2")
                //{
                //    return string.Empty;
                //}

                PersonnelService ser = new PersonnelService();
                T_HR_EMPLOYEEENTRY entry = new T_HR_EMPLOYEEENTRY();
                T_HR_EMPLOYEE employee = new T_HR_EMPLOYEE();
                T_HR_EMPLOYEEPOST epost = new T_HR_EMPLOYEEPOST();
                employee.EMPLOYEEID = Guid.NewGuid().ToString();
                employee.EMPLOYEECNAME = Name;
                employee.IDNUMBER = Idnumber.Replace("{", "").Replace("}", "");
                employee.OWNERCOMPANYID = strOwnerCompanyID.Replace("{", "").Replace("}", "");
                employee.OWNERDEPARTMENTID = strOwnerDepartmentID.Replace("{", "").Replace("}", "");
                employee.OWNERPOSTID = strOwnerPostID.Replace("{", "").Replace("}", "");
                employee.OWNERID = employee.EMPLOYEEID.Replace("{", "").Replace("}", "");
                employee.CREATEDATE = DateTime.Now;
                employee.SEX = sex;
                employee.CREATEUSERID = createuserid;
                
                epost.EMPLOYEEPOSTID = Guid.NewGuid().ToString();
                epost.ISAGENCY = "0";
                epost.CREATEDATE = DateTime.Now;
                epost.T_HR_POST = new T_HR_POST();
                epost.T_HR_POST.POSTID = employee.OWNERPOSTID;
                epost.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                epost.T_HR_EMPLOYEE.EMPLOYEEID = employee.EMPLOYEEID;
                epost.CREATEUSERID = createuserid;
                epost.CHECKSTATE = "0";

                entry.EMPLOYEEENTRYID = Guid.NewGuid().ToString();
                entry.CHECKSTATE = "0";
                entry.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                entry.T_HR_EMPLOYEE.EMPLOYEEID = employee.EMPLOYEEID;
                entry.CREATEDATE = System.DateTime.Now;
                entry.EMPLOYEEPOSTID = epost.EMPLOYEEPOSTID;
                entry.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                entry.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                entry.OWNERPOSTID = employee.OWNERPOSTID;
                entry.OWNERID = employee.EMPLOYEEID;
                entry.CREATEUSERID = createuserid;
               

                ser.AddEmployeeEntry(employee, entry, epost);
                return entry.EMPLOYEEENTRYID;

            }
            catch (Exception e)
            {
                string abc = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
                Tracer.Debug(abc);
                return abc;

            }

        }

                

    }
}
