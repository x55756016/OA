using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.HRM.BLL;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class InstantMessagingService
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        // 在此处添加更多操作并使用 [OperationContract] 标记它们

        InstantMessagingDataBLL imdb = new InstantMessagingDataBLL();
        /// <summary>
        /// 获取所有公司
        /// </summary>
        /// <returns>所有有效的公司</returns>
        [OperationContract]
        public List<CompanyModel> GetAllCompany()
        {
            return imdb.GetAllCompany();
        }

        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <returns>所有有效的部门</returns>
        [OperationContract]
        public List<DepartmentModel> GetAllDepartment()
        {
            return imdb.GetAllDepartment();
        }
        /// <summary>
        /// 及时通讯接口获取数据
        /// </summary>
        /// <param name="companyNum"></param>
        /// <param name="departNum"></param>
        /// <param name="employeeNum"></param>
        /// <returns></returns>
        [OperationContract]
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
                    //HrInstantMessageWS.InstantMessagingServiceClient InstantMessage = new InstantMessagingServiceClient();
                    List<CompanyModel> companys = GetAllCompany();//获取所有公司
                    List<DepartmentModel> department = GetAllDepartment();//获取所有部门
                    List<EmployeeModel> employes = GetAllEmployee();//获取所有员工

                    if (employes.Count<=0)
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
                            writer.WriteAttributeString("ShortName", companys[i].ShortName);
                            writer.WriteAttributeString("ParentID", companys[i].ParentID);
                            writer.WriteAttributeString("FatherType", companys[i].FatherType);
                            writer.WriteAttributeString("SORTINDEX", companys[i].SORTINDEX.ToString());
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
                            writer.WriteAttributeString("FatherType", department[k].FatherType);
                            writer.WriteAttributeString("SORTINDEX", department[k].SORTINDEX.ToString());
                            writer.WriteEndElement();//完成Department节点
                        }

                        writer.WriteEndElement();//完成DepartmentList 节点
                        writer.WriteStartElement("EmployeeList");
                        Tracer.Debug("开始打印员工信息");
                        string StrEmployees = "";
                        for (int n = 0; n < employes.Count(); n++)
                        {
                            StrEmployees += employes[n].EmployeeName + "帐号：" + employes[n].LoginAccount;
                            StrEmployees += "部门ID:" + employes[n].DeptID + "公司名" + employes[n].CompanyName;
                            StrEmployees += "岗位名：" + employes[n].PostName;
                            StrEmployees += "岗位ID:" + employes[n].PostID;
                            StrEmployees += "地址:" + employes[n].Address;
                            StrEmployees += "办公电话:" + employes[n].Tel;
                            
                            writer.WriteStartElement("Employee");
                            writer.WriteAttributeString("EmployeeID", employes[n].EmployeeId);
                            writer.WriteAttributeString("EmployeeName", employes[n].EmployeeName);
                            writer.WriteAttributeString("LoginAccount", employes[n].LoginAccount);
                            writer.WriteAttributeString("Address", employes[n].Address);//地址                            
                            writer.WriteAttributeString("Tel", employes[n].Tel);
                            writer.WriteAttributeString("PostID", employes[n].PostID);
                            writer.WriteAttributeString("Email", employes[n].Email);
                            writer.WriteAttributeString("Sex", employes[n].Sex);
                            writer.WriteAttributeString("Moblie", employes[n].Mobile);
                            writer.WriteAttributeString("DeptID", employes[n].DeptID);
                            writer.WriteAttributeString("PostName", employes[n].PostName);
                            writer.WriteAttributeString("IsAgencePost", employes[n].IsAgencePost);
                            writer.WriteAttributeString("CompanyName", employes[n].CompanyName);

                            writer.WriteEndElement();//完成Employee节点

                            if (employes[n].EmployeeName == "张鹏")
                            {

                                //Tracer.Debug("EmployeeID：" + employes[n].EmployeeId);
                                //Tracer.Debug("EmployeeName：" + employes[n].EmployeeName);
                                //Tracer.Debug("LoginAccount：" + employes[n].LoginAccount);
                                //Tracer.Debug("Email：" + employes[n].Email);
                                //Tracer.Debug("Sex：" + employes[n].Sex);
                                //Tracer.Debug("Moblie：" + employes[n].Mobile);
                                //Tracer.Debug("DeptID：" + employes[n].DeptID);

                                Tracer.Debug("CompanyName：" + employes[n].CompanyName + "-"
                                    + employes[n].DepartMentName + "-"
                                    + employes[n].PostName + "-"
                                    + employes[n].EmployeeName);


                            }
                            else
                            {
                                Tracer.Debug(StrEmployees);
                            }
                        }
                        Tracer.Debug("结束打印员工信息");
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

            return StrReturn.ToString().Replace("\r", "").Replace("\n", "");
        }

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
        /// <summary>
        /// 获取所有员工
        /// </summary>
        /// <returns>所有有效的员工</returns>
        [OperationContract]
        public List<EmployeeModel> GetAllEmployee()
        {
            return imdb.GetAllEmployee();
        }

        /// <summary>
        /// 获取员工详细信息
        /// </summary>
        /// <param name="ID">编号</param>
        /// <returns>此员工的所有信息</returns>
        [OperationContract]
        public EmployeeModel GetSingelEmployee(string ID)
        {
            return imdb.GetSingelEmployee(ID);
        }

        #region 获取员工信息


        /// <summary>
        /// 通过员工登录帐号返回员工信息，格式如下
        /// </summary>
        /// <param name="LoginAccount">登录帐号</param>
        /// <returns>XML格式的字符串</returns>
        /// 
        ///成功 
        ///<Employee EmployeeId="" EmployeeName="" LoginAccount="" Sex="" DepartMentName=" " 
        ///Age="" PostName="" Email=" " Address=" " AddCode="" Mobile="" Tel="" Nation=" " Province=" " City=" " Remark=" " />
        ///</BizRegReq>
        ///失败：
        ///<BizRegReq>
        ///<Error ErrorMsg=" " />
        ///</BizRegReq>
        [OperationContract]
        public string GetEmployeeInfo(string loginAccount)
        {
            return imdb.GetEmployeeInfo(loginAccount);
        }
        /// <summary>
        /// 通过员工id获取员工头像
        /// </summary>
        /// <param name="employeeid">员工id</param>
        /// <returns></returns>
        [OperationContract]
        public byte[] GetEmployeePhoto(string employeeid)
        {
            Tracer.Debug("即时通讯接口开始调用获取员工头像图片");
            byte[] photo=imdb.GetEmployeePhoto(employeeid);
            Tracer.Debug("即时通讯接口结束调用获取员工头像图片");
            return photo;
        }
        #endregion
    }
}
