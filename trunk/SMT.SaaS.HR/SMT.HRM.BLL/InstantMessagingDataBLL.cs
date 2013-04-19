using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using System.Xml;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 获取即时通讯所需数据
    /// </summary>
    public class InstantMessagingDataBLL
    {
        /// <summary>
        /// 获取所有公司
        /// </summary>
        /// <returns>所有有效的公司</returns>
        public List<CompanyModel> GetAllCompany()
        {
            using (BaseBll<T_HR_COMPANY> bsb = new BaseBll<T_HR_COMPANY>())
            {
                var companyList = from v in bsb.GetTable()
                                  where v.EDITSTATE =="1" && v.CHECKSTATE=="2"
                                  select new CompanyModel
                                  {
                                      CompanyID = v.COMPANYID,
                                      ShortName = v.BRIEFNAME,
                                      CompanyName = v.CNAME,
                                      ParentID = v.FATHERID,
                                      SORTINDEX = v.SORTINDEX,
                                      FatherType = v.FATHERTYPE
                                  };
                return companyList.ToList();
            }
        }
        /// <summary>
        /// 获取所有部门
        /// </summary>
        /// <returns>所有有效的部门</returns>
        public List<DepartmentModel> GetAllDepartment()
        {
            using (CommDal<T_HR_DEPARTMENT> cdl = new CommDal<T_HR_DEPARTMENT>())
            {
                var departmentList = from v in cdl.GetObjects()
                                         .Include("T_HR_COMPANY")
                                         .Include("T_HR_DEPARTMENTDICTIONARY")
                                         .Where(s => s.EDITSTATE == "1" && s.CHECKSTATE == "2")
                                     select new DepartmentModel
                                     {
                                         DeptID = v.DEPARTMENTID,
                                         DepartName = v.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                         CompanyID = v.T_HR_COMPANY.COMPANYID,
                                         ParentID = v.FATHERID,
                                         SORTINDEX = v.SORTINDEX,
                                         FatherType = v.FATHERTYPE
                                     };
                return departmentList.ToList();
            }
        }
        /// <summary>
        /// 获取所有员工
        /// </summary>
        /// <returns>所有有效的员工</returns>
        public List<EmployeeModel> GetAllEmployee()
        {
            return GetEmployeeComm();
           
        }
        /// <summary>
        /// 获取员工详细信息
        /// </summary>
        /// <param name="ID">编号</param>
        /// <returns>此员工的所有信息</returns>
        public EmployeeModel GetSingelEmployee(string ID)
        {
            var ents = GetEmployeeComm().Where(s => s.EmployeeId == ID).FirstOrDefault();
            return ents;
        }
        /// <summary>
        /// 获取员工
        /// </summary>
        /// <returns>所有有效的员工</returns>
        public List<EmployeeModel> GetEmployeeComm()
        {
            using (CommDal<T_HR_EMPLOYEE> cdl = new CommDal<T_HR_EMPLOYEE>())
            {
                var ents = from o in cdl.GetObjects()
                           join ep in cdl.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                           join p in cdl.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join pd in cdl.GetObjects<T_HR_POSTDICTIONARY>() on p.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals pd.POSTDICTIONARYID
                           join d in cdl.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in cdl.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           join cd in cdl.GetObjects<T_HR_DEPARTMENTDICTIONARY>() on
                               d.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals cd.DEPARTMENTDICTIONARYID
                           where o.EDITSTATE == "1" && o.EMPLOYEESTATE != "2" && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                           select new EmployeeModel
                           {
                               CompanyName = c.CNAME,
                               EmployeeId = o.EMPLOYEEID,
                               EmployeeName = o.EMPLOYEECNAME,
                               LoginAccount = o.EMPLOYEEENAME,
                               Sex = o.SEX,
                               Age = "",
                               Email = o.EMAIL,
                               Address = o.FAMILYADDRESS,
                               AddCode = "",
                               Mobile = o.MOBILE,
                               Tel = o.OFFICEPHONE,
                               Nation = "",
                               Province = "",
                               City = "",
                               Remark = o.REMARK,
                               DeptID = d.DEPARTMENTID,
                               DepartMentName = cd.DEPARTMENTNAME,
                               PostName = pd.POSTNAME,
                               PostID = p.POSTID,
                               IsAgencePost=ep.ISAGENCY
                           };
                return ents.ToList();
            }
        }

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
                    using (CommDal<T_HR_EMPLOYEE> cdl = new CommDal<T_HR_EMPLOYEE>())
                    {
                        var ents = from ent in cdl.GetObjects<T_HR_EMPLOYEE>()
                                   where ent.EMPLOYEEENAME == loginAccount
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            EmployeeModel employee = new EmployeeModel();

                            if (ents.FirstOrDefault() != null)
                            {
                                T_HR_EMPLOYEE UserInfo = ents.FirstOrDefault();
                                employee = GetSingelEmployee(UserInfo.EMPLOYEEID);
                                if (employee != null)
                                {

                                    writer.WriteStartElement("BizRegReq");
                                    writer.WriteStartElement("Employee");
                                    writer.WriteAttributeString("EmployeeId", employee.EmployeeId);
                                    writer.WriteAttributeString("EmployeeName", employee.EmployeeName);
                                    writer.WriteAttributeString("LoginAccount", employee.LoginAccount);
                                    writer.WriteAttributeString("Sex", employee.Sex);
                                    //writer.WriteAttributeString("Age", "");
                                    writer.WriteAttributeString("PostName", employee.PostName);
                                    writer.WriteAttributeString("Email", employee.Email);
                                    writer.WriteAttributeString("Address", employee.Address);
                                    writer.WriteAttributeString("AddCode", "");//没有默认为空
                                    writer.WriteAttributeString("Mobile", employee.Mobile);
                                    writer.WriteAttributeString("Tel", employee.Tel);
                                    writer.WriteAttributeString("Nation", "");
                                    writer.WriteAttributeString("Province", "");
                                    writer.WriteAttributeString("City", "");
                                    writer.WriteAttributeString("PostID", employee.PostID);
                                    writer.WriteAttributeString("Remark", employee.Remark);
                                    writer.WriteAttributeString("CompanyName", employee.CompanyName);
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
            return StrReturn.ToString().Replace("\r", "").Replace("\n", "");
        }
        #endregion

        public byte[] GetEmployeePhoto(string employeeid)
        {
            using (CommDal<T_HR_EMPLOYEE> cdl = new CommDal<T_HR_EMPLOYEE>())
            {
                var q = from ent in cdl.GetObjects<T_HR_EMPLOYEE>()
                        where ent.EMPLOYEEID == employeeid
                        select ent.PHOTO;
                if (q.Count() > 0)
                    return q.FirstOrDefault();
                else
                    return null;
            }
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
    }
    #region  Model
    /// <summary>
    /// 所需的公司模型
    /// </summary>
    public class CompanyModel
    {
        /// <summary>
        /// 公司ID
        /// </summary>
        public string CompanyID { get; set; }
        /// <summary>
        /// 公司简称
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 上级公司ID
        /// </summary>
        public string ParentID { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public decimal? SORTINDEX { get; set; }
        /// <summary>
        /// 上级类型
        /// </summary>
        public string FatherType { get; set; }
    }
    /// <summary>
    /// 所需的部门模型
    /// </summary>
    public class DepartmentModel
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DeptID { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartName { get; set; }
        /// <summary>
        /// 该部门所属公司ID
        /// </summary>
        public string CompanyID { get; set; }
        /// <summary>
        /// 上级部门ID
        /// </summary>
        public string ParentID { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public decimal? SORTINDEX { get; set; }
        /// <summary>
        /// 上级类型
        /// </summary>
        public string FatherType { get; set; }
    }
    /// <summary>
    /// 所需的员工模型
    /// </summary>
    public class EmployeeModel
    {
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName { get; set; }
        /// <summary>
        /// 员工姓名
        /// </summary>
        public string LoginAccount { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public string Age { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string AddCode { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 办公电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 个人说明
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string DeptID { get; set; }
        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartMentName { get; set; }
        /// <summary>
        /// 所属岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 所属公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 岗位ID
        /// </summary>
        public string PostID { get; set; }
        /// <summary>
        /// 岗位类型：0主岗位，1兼职岗位
        /// </summary>
        public string IsAgencePost { get; set; }
    }
    #endregion
}
