using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.Permission.DAL;
using SMT.Foundation.Log;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data.Objects;

using SMT.SaaS.Permission.DAL.views;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.SaaS.Permission.BLL.HrInstantMessageWS;
using System.Xml;
using SMT.SaaS.SmtOlineEn;

namespace SMT.SaaS.Permission.BLL
{
    public class InstantMessageBll : BaseBll<T_SYS_USER>
    {
              

        #region 根据员工帐号获取员工信息        
        
        /// <summary>
        /// 根据帐号获取员工信息
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <returns></returns>
        public string GetEmployeeInfo(string loginAccount)
        {
            StringBuilder StrReturn = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            try
            {
                using (XmlWriter writer = XmlWriter.Create(StrReturn, settings))
                {
                    var ents = from ent in dal.GetObjects<T_SYS_USER>()
                               where ent.USERNAME == loginAccount
                               select ent;
                    if (ents.Count() > 0)
                    {
                        HrInstantMessageWS.InstantMessagingServiceClient InstantMessage = new InstantMessagingServiceClient();
                        
                        PersonnelServiceClient client = new PersonnelServiceClient();
                        EmployeeModel employee = new EmployeeModel();

                        if (ents.FirstOrDefault() != null)
                        {
                            T_SYS_USER UserInfo = ents.FirstOrDefault();

                            employee = InstantMessage.GetSingelEmployee(UserInfo.EMPLOYEEID);
                            if (employee != null)
                            {

                                writer.WriteStartElement("BizRegReq");
                                writer.WriteStartElement("Employee");
                                writer.WriteAttributeString("EmployeeId", employee.EmployeeId);
                                writer.WriteAttributeString("EmployeeName", employee.EmployeeName);
                                writer.WriteAttributeString("LoginAccount", UserInfo.USERNAME);
                                writer.WriteAttributeString("Sex", "");
                                writer.WriteAttributeString("Age", "");
                                writer.WriteAttributeString("PostName", employee.PostName);
                                writer.WriteAttributeString("Email", employee.Email);
                                writer.WriteAttributeString("Address", employee.Address);
                                writer.WriteAttributeString("AddCode", "");//没有默认为空
                                writer.WriteAttributeString("Mobile", employee.Mobile);
                                writer.WriteAttributeString("Tel", employee.Tel);
                                writer.WriteAttributeString("Nation", "");
                                writer.WriteAttributeString("Province", "");
                                writer.WriteAttributeString("City", "");
                                writer.WriteAttributeString("Remark", employee.Remark);
                                writer.WriteEndElement();//完成Employee节点
                                writer.WriteEndElement();//完成BizRegReq节点

                            }
                            else
                            {
                                ErrorMessage(writer, "没有获取到该员工信息");
                            }
                        }
                        else
                        {
                            ErrorMessage(writer, "没有获取到该员工信息"); 
                        }
                    }
                    else
                    {
                        ErrorMessage(writer, "没有获取到该员工信息");                       
                        
                    }
                    writer.Flush();
                }
               
                
            }
            catch (Exception ex)
            {
                using (XmlWriter catchError = XmlWriter.Create(StrReturn, settings))
                {
                    ErrorMessage(catchError, "服务器错误");
                    Tracer.Debug("即时通讯-GetEmployeeInfo：" + ex.ToString() + System.DateTime.Now);
                    catchError.Flush();
                }
            }
            return StrReturn.ToString().Replace("\r","").Replace("\n","");
        }
        #endregion

        #region 修改员工信息
        public string UpdateEmployeeInfo(EmployeeInfo employeeInfo)
        {
            StringBuilder StrReturn = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            try
            {                
                using (XmlWriter writer = XmlWriter.Create(StrReturn, settings))
                {
                    var ents = from ent in dal.GetObjects<T_SYS_USER>()
                               where ent.USERNAME == employeeInfo.loginAccount
                               select ent;
                    if (ents.Count() > 0)
                    {
                        PersonnelServiceClient client = new PersonnelServiceClient();
                        
                        T_HR_EMPLOYEE employee = new T_HR_EMPLOYEE();
                        if (ents.FirstOrDefault() != null)
                        {
                            T_SYS_USER UserInfo = ents.FirstOrDefault();
                            T_HR_EMPLOYEE CachePerson = client.GetEmployeeByID(UserInfo.EMPLOYEEID);
                            CachePerson.REMARK = employeeInfo.Remark;
                            CachePerson.OFFICEPHONE = employeeInfo.Tel;
                            CachePerson.MOBILE = employeeInfo.Mobile;
                            CachePerson.FAMILYADDRESS = employeeInfo.Address;
                            string StringRef = "";                            
                            Tracer.Debug("更新员工信息"+ UserInfo.EMPLOYEEID+" " + StringRef + System.DateTime.Now);
                            
                            if (CachePerson != null)
                            {
                                CachePerson.T_HR_RESUMEReference = null;
                                client.EmployeeUpdate(CachePerson, UserInfo.OWNERCOMPANYID, ref StringRef);
                                if (StringRef == "")
                                {
                                    HrInstantMessageWS.InstantMessagingServiceClient InstantMessage = new InstantMessagingServiceClient();
                                    EmployeeModel employeeHr = new EmployeeModel();
                                    employeeHr = InstantMessage.GetSingelEmployee(UserInfo.EMPLOYEEID);
                                    if (employeeHr != null)
                                    {
                                        writer.WriteStartElement("BizRegReq");
                                        writer.WriteStartElement("Employee");
                                        writer.WriteAttributeString("EmployeeId", employeeHr.EmployeeId);
                                        writer.WriteAttributeString("EmployeeName", employeeHr.EmployeeName);
                                        writer.WriteAttributeString("LoginAccount", UserInfo.USERNAME);
                                        writer.WriteAttributeString("Sex", "");
                                        writer.WriteAttributeString("Age", "");
                                        writer.WriteAttributeString("PostName", "");//待完善
                                        writer.WriteAttributeString("Email", employeeHr.Email);
                                        writer.WriteAttributeString("Address", employeeHr.Address);
                                        writer.WriteAttributeString("AddCode", "");//没有默认为空
                                        writer.WriteAttributeString("Mobile", employeeHr.Mobile);
                                        writer.WriteAttributeString("Tel", employeeHr.Tel);
                                        writer.WriteAttributeString("Nation", "");
                                        writer.WriteAttributeString("Province", "");
                                        writer.WriteAttributeString("City", "");
                                        writer.WriteAttributeString("Remark", employeeHr.Remark);
                                        writer.WriteEndElement();//完成Employee节点
                                        writer.WriteEndElement();//完成BizRegReq节点
                                        //writer.Flush();
                                    }
                                }
                                else
                                {
                                    ErrorMessage(writer, "修改员工信息错误，请联系管理员");
                                }
                            }
                            else
                            {
                                ErrorMessage(writer, "没有获取到该员工信息");
                                    
                            }
                            
                            
                        }
                        else
                        {
                            ErrorMessage(writer, "没有获取到该员工信息");
                        }
                    }
                    else
                    {
                        ErrorMessage(writer, "没有获取到该员工信息");
                        
                    }
                    writer.Flush();
                }


            }
            catch (Exception ex)
            {
                using (XmlWriter catchError = XmlWriter.Create(StrReturn, settings))
                {
                    ErrorMessage(catchError, "服务器错误");
                    Tracer.Debug("即时通讯-UpdateEmployeeInfo：" + ex.ToString() + System.DateTime.Now);
                    catchError.Flush();
                }
            }
            return StrReturn.ToString().Replace("\r","").Replace("\n","");
        }

        
        #endregion

        #region 错误信息提示
        /// <summary>
        /// 错误信息提示
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="strMessage"></param>
        private static void ErrorMessage(XmlWriter writer, string strMessage)
        {
            writer.WriteStartElement("BizRegReq");
            writer.WriteStartElement("Error");
            writer.WriteAttributeString("ErrorMsg", strMessage);
            writer.WriteEndElement();//完成Error节点
            writer.WriteEndElement();//完成BizRegReq节点
        }
        #endregion

        #region 员工登录
        public string EmployeeLogin(string loginAccount, string LodingPwd,T_SYS_USER ListUsers)
        {
            StringBuilder StrReturn = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            try
            {
                using (XmlWriter writer = XmlWriter.Create(StrReturn, settings))
                {
                    if (ListUsers != null)
                    {
                        var employee = ListUsers;
                        //enpwd = employee.PASSWORD;
                        //解密
                        SMT.SaaS.SmtOlineEn.SmtOlineDES des = new SmtOlineDES();
                        string UserPwd = "";
                        try
                        {
                            UserPwd = des.getValue(employee.PASSWORD);
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("des.getValue(employee.PASSWORD)出错:" + ex.ToString());
                        }
                        UserPwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(UserPwd, "MD5");
                        //string strNew = depwd;
                        Tracer.Debug("即时通讯传递的用户名：" + loginAccount + " 用户密码:" + LodingPwd.ToUpper()+@" 协同办公系统用户名:" + employee.USERNAME + " 密码:" + UserPwd.ToUpper());
                        if (UserPwd.ToUpper() == LodingPwd.ToUpper())
                        {
                            //HrInstantMessageWS.InstantMessagingServiceClient InstantMessage = new InstantMessagingServiceClient();
                            writer.WriteStartElement("BizRegReq");
                            writer.WriteStartElement("Employee");
                            writer.WriteAttributeString("EmployeeId", string.Empty);
                            writer.WriteAttributeString("EmployeeName", string.Empty);
                            writer.WriteAttributeString("LoginAccount", employee.USERNAME);
                            writer.WriteAttributeString("Sex", "");
                            writer.WriteAttributeString("Age", "");
                            writer.WriteAttributeString("PostName", "");//待完善
                            writer.WriteAttributeString("Email", string.Empty);
                            writer.WriteAttributeString("Address", string.Empty);
                            writer.WriteAttributeString("AddCode", "");//没有默认为空
                            writer.WriteAttributeString("Mobile", string.Empty);
                            writer.WriteAttributeString("Tel", string.Empty);
                            writer.WriteAttributeString("Nation", "");
                            writer.WriteAttributeString("Province", "");
                            writer.WriteAttributeString("City", "");
                            writer.WriteAttributeString("Remark", string.Empty);
                            writer.WriteEndElement();//完成Employee节点
                            writer.WriteEndElement();//完成BizRegReq节点
                        }
                        else
                        {
                            ErrorMessage(writer, "用户名或密码不对");
                        }
                    }
                    else
                    {
                        ErrorMessage(writer, "没有获取到该员工信息");
                    }
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                using (XmlWriter catchError = XmlWriter.Create(StrReturn, settings))
                {
                    ErrorMessage(catchError, "服务器错误");
                    Tracer.Debug("即时通讯登录错误：" + ex.ToString() + System.DateTime.Now);
                    catchError.Flush();
                }
            }
            return StrReturn.ToString().Replace("\r","").Replace("\n","");
        }
        #endregion

        #region 获取组织架构信息
        public string GetAllOrganization(int companyNum, int departNum, int employeeNum)
        {
            StringBuilder StrReturn = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            try
            {
                using (XmlWriter writer = XmlWriter.Create(StrReturn, settings))
                {
                    HrInstantMessageWS.InstantMessagingServiceClient InstantMessage = new InstantMessagingServiceClient();
                    CompanyModel[] companys = InstantMessage.GetAllCompany();//获取所有公司
                    DepartmentModel[] department = InstantMessage.GetAllDepartment();//获取所有部门
                    EmployeeModel[] employes = InstantMessage.GetAllEmployee();//获取所有员工
                    var sysUsers = from ent in dal.GetObjects<T_SYS_USER>()
                                   select ent;
                    if (sysUsers.Count() == 0)
                        return "";
                    writer.WriteStartElement("BizRegReq");
                    if (companyNum == companys.Count() && departNum == department.Count() && employeeNum == employes.Count())
                    {
                        return "";
                    }
                    else
                    {                        
                        writer.WriteStartElement("CompanyList");
                        for (int i = 0; i < companys.Count(); i++)
                        {
                            writer.WriteStartElement("Company");
                            writer.WriteAttributeString("CompanyID", companys[i].CompanyID);
                            writer.WriteAttributeString("CompanyName", companys[i].CompanyName);
                            writer.WriteAttributeString("ParentID", companys[i].ParentID);
                            writer.WriteEndElement();//完成Company节点

                        }
                        writer.WriteEndElement();//完成CompanyList节点
                        writer.WriteStartElement("DepartmentList");
                        for (int k = 0; k < department.Count(); k++)
                        {
                            writer.WriteStartElement("Department");
                            writer.WriteAttributeString("DeptID", department[k].DeptID);
                            writer.WriteAttributeString("DepartName", department[k].DepartName);
                            writer.WriteAttributeString("CompanyID", department[k].CompanyID);
                            writer.WriteAttributeString("ParentID", department[k].ParentID);
                            writer.WriteEndElement();//完成Department节点
                        }

                        writer.WriteEndElement();//完成DepartmentList 节点
                        writer.WriteStartElement("EmployeeList");
                        for (int n = 0; n < employes.Count(); n++)
                        {
                            string employeeId = employes[n].EmployeeId;
                            var ents = sysUsers.Where(p => p.EMPLOYEEID == employeeId);
                            if (ents.Count() > 0)
                            {
                                writer.WriteStartElement("Employee");
                                writer.WriteAttributeString("EmployeeID", employes[n].EmployeeId);
                                writer.WriteAttributeString("EmployeeName", employes[n].EmployeeName);
                                writer.WriteAttributeString("LoginAccount", ents.FirstOrDefault().USERNAME);
                                writer.WriteAttributeString("Moblie", employes[n].Mobile);
                                writer.WriteAttributeString("DeptID", employes[n].DeptID);
                                writer.WriteAttributeString("PostName", employes[n].PostName);
                                writer.WriteEndElement();//完成Employee节点
                            }
                           
                        }
                        writer.WriteEndElement();//完成EmployeeList节点

                        writer.WriteEndElement();//完成BizRegReq节点
                        writer.Flush();
                    }
                    
                    
                }


            }
            catch (Exception ex)
            {
                using (XmlWriter catchError = XmlWriter.Create(StrReturn, settings))
                {
                    ErrorMessage(catchError, "服务器错误");
                    Tracer.Debug("即时通讯-GetAllOrganization：" + ex.ToString() + System.DateTime.Now);
                    catchError.Flush();
                }
            }
            
            return StrReturn.ToString().Replace("\r","").Replace("\n","");
        }
        #endregion

        /// <summary>
        /// 返回正常状态的用户信息
        /// </summary>
        /// <returns></returns>
        public T_SYS_USER GetUsers(string username)
        {
            var ents = from ent in dal.GetObjects<T_SYS_USER>()
                       where ent.STATE == "1" && ent.USERNAME == username
                       select ent;
            return ents != null ? ents.FirstOrDefault() : null;
        }

        
    }
}
