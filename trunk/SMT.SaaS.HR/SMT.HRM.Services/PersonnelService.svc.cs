using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using SMT_HRM_EFModel;
using SMT.HRM.BLL;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using SMT.SaaS.BLLCommonServices.FlowWFService;

using SMT.HRM.CustomModel;
using System.Configuration;
using System.Web;
using SMT.Foundation.Log;

namespace SMT.HRM.Services
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PersonnelService
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        #region 文件上传服务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="strFilePath">上传文件存储的相对路径</param>
        [OperationContract]
        public void SaveFile(UploadFileModel UploadFile, out string strFilePath)
        {
            // Store File to File System
            string strVirtualPath = ConfigurationManager.AppSettings["FileUploadLocation"].ToString();
            string strPath = HttpContext.Current.Server.MapPath(strVirtualPath) + UploadFile.FileName;
            if (Directory.Exists(HttpContext.Current.Server.MapPath(strVirtualPath)) == false)
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(strVirtualPath));
            }
            FileStream FileStream = new FileStream(strPath, FileMode.Create);
            FileStream.Write(UploadFile.File, 0, UploadFile.File.Length);

            FileStream.Close();
            FileStream.Dispose();

            strFilePath = strVirtualPath + UploadFile.FileName;
        }
        #endregion

        #region T_HR_EMPLOYEE 员工档案

        /// <summary>
        /// 新增员工档案
        /// </summary>
        /// <param name="ent">员工档案</param>
        [OperationContract]
        public void EmployeeAdd(T_HR_EMPLOYEE entity, string companyID, ref string strMsg)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                bll.EmployeeAdd(entity, companyID, ref strMsg);
            }
        }
        /// <summary>
        /// 新增或者修改员工档案
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="strMsg"></param>
        [OperationContract]
        public void EmployeeAddOrUpdate(T_HR_EMPLOYEE entity, ref string strMsg)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                bll.EmployeeAddOrUpdate(entity, ref strMsg);
            }
        }

        /// <summary>
        /// 更新员工档案
        /// </summary>
        /// <param name="entity">员工档案</param>
        [OperationContract]
        public void EmployeeUpdate(T_HR_EMPLOYEE entity, string companyID, ref string strMsg)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                bll.EmployeeUpdate(entity, companyID, ref strMsg);
            }
        }

        /// <summary>
        /// 根据员工ID查询员工档案
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工信息</returns>
        [OperationContract]
        public T_HR_EMPLOYEE GetEmployeeByID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeByID(employeeID);
            }
        }

        /// <summary>
        /// 导出员工档案（公司的非离职员工）
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployee(string companyID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.ExportEmployee(companyID);
            }
        }
        /// <summary>
        /// 根据身份证获取员工信息
        /// </summary>
        /// <param name="idnumbers"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeesByIdnumber(string[] idnumbers)
        {
            EmployeeBLL employeebll = new EmployeeBLL();
            var ents = employeebll.GetEmployeesByIdnumber(idnumbers);
            return ents;
        }
        /// <summary>
        /// 根据公司ID获取所有员工
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeeByCompanyID(string companyID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                var q = bll.GetEmployeeByCompanyID(companyID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 根据岗位ID获取所有员工
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeeByPostID(string postID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                var q = bll.GetEmployeeByPostID(postID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        /// 根据岗位ID获取所有员工
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetEmployeeIDsByPostID(string postID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                var q = bll.GetEmployeeIDsByPostID(postID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        /// <summary>
        ///获取员工上级
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="isDirect">0 表示直接上级 ,1 表示间接上级,2 部门负责人</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeeLeader(string employeeID, int LeaderType)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeLeader(employeeID, LeaderType);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PostID"></param>
        /// <param name="LeaderType"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeLeaders(string PostID, int LeaderType)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeLeaders(PostID, LeaderType);

                //DepartmentBLL b = new DepartmentBLL();
                //b.GetDepartmentAll("c7d6b7bd-033b-4d80-8c14-95344516665f");
                //List<T_HR_DEPARTMENT> ss=b.GetDepartmentAll("c7d6b7bd-033b-4d80-8c14-95344516665f").ToList();
                //List<string> aa = new List<string>();
                //foreach (var temp in ss)
                //{
                //    aa.Add(temp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME);
                //}
                //DepartmentBLL b = new DepartmentBLL();
                //b.UpdateCheckState("T_HR_DEPARTMENT", "DEPARTMENTID", "b95b27c4-e518-4943-b401-97fdc92e3691", "2");
                //return null;
                //OverTimeRecordBLL b = new OverTimeRecordBLL();
                //b.UpdateCheckState("T_HR_EMPLOYEEOVERTIMERECORD", "OVERTIMERECORDID", "12c3ddbb-d60d-4431-b5ac-b89275ee9826", "1");
                //return null;
                //DepartmentBLL b1 = new DepartmentBLL();
                //b1.UpdateCheckState("T_HR_DEPARTMENT", "DEPARTMENTID", "272ae780-0447-48b3-b796-a3b36b3c89b4", "1");
                //return null;
                //EmployeePostBLL b1 = new EmployeePostBLL();
                //b1.GetEmployeePostBriefByEmployeeID("f3357185-cb16-4f32-9fa1-e653350a918f");
                //return null;
                //GetEmployeeEntryByID("5b9edea4-aad1-4d66-aeaa-d9ab1ce4357b");
                //return null;
                //EngineService es = new EngineService();
                //string xml = File.ReadAllText("D:\\testfile.txt");
                //es.CallWaitAppService(xml);
                //return null;
                
                //bll1.ExportExcel("EMPLOYEESALARYRECORDID", "CHECKSTATE==@0", paras, "2011", "7", 0, "cafdca8a-c630-4475-a65d-490d052dca36");
                //return null;
                //EmployeePostBLL bll1 = new EmployeePostBLL();
                //bll1.GetEmployeePostBriefByPostID("d19517f2-efc3-417b-b154-b9b46bcfec75");

                //员工入职测试 weirui 7/18 通过
                //EmployeeEntryBLL bb = new EmployeeEntryBLL();
                //bb.UpdateCheckState("T_HR_EMPLOYEEENTRY", "employeeentryid", "6332ef44-948e-46a6-8edd-fefbad82e432", "2");
                
                //员工异动测试 weirui 7/18 通过
                //EmployeePostChangeBLL bb = new EmployeePostChangeBLL();
                //bb.UpdateCheckState("T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGEID", "97df6341-d3cb-4bcb-b4c7-07f9a2cc4f7a", "2");
                //return null;
                //CompanyBLL company = new CompanyBLL();
                //company.UpdateCheckState("T_HR_COMPANY", "COMPANYID", "7f4451a6-24a9-48d8-8f47-b3ec74f690b6", "2");
                //return null;
                //DepartmentBLL dept = new DepartmentBLL();
                //dept.UpdateCheckState("T_HR_DEPARTMENT", "DEPARTMENTID", "4f21b681-c01a-47c4-81b9-2221c67a06de", "3");
                //return null;
                //DepartmentBLL dept = new DepartmentBLL();
                //dept.GetDepartmentByCompanyIDs("cafdca8a-c630-4475-a65d-490d052dca36,1a1f745c-e0df-4d21-b000-8d80d91b1743");
                //return null; 
                //员工离职测试 weirui 7/18 通过
                //LeftOfficeConfirmBLL bb = new LeftOfficeConfirmBLL();
                //bb.UpdateCheckState("T_HR_LEFTOFFICECONFIRM", "CONFIRMID", "e8e5f6b1-8fad-4102-b765-b10bc2001af6", "2");
                //return null;
                //员工转正测试 weirui 7/18 通过
                //EmployeeCheckBLL bb = new EmployeeCheckBLL();
                //bb.UpdateCheckState("T_HR_EMPLOYEECHECK", "BEREGULARID", "a452069f-667f-4e97-b871-507f96597e97", "2");
                //员工工薪档案测试 weirui 7/18 通过
                //SalaryArchiveBLL bb = new SalaryArchiveBLL();
                //bb.UpdateCheckState("T_HR_SALARYARCHIVE", "SALARYARCHIVEID", "e0798203-b846-4dc2-a2e4-3b09314d5641", "2");
                //return null;
                //return null;
                //EmployeeContractBLL bbb = new EmployeeContractBLL();
                //bbb.UpdateCheckState("T_HR_EMPLOYEECONTRACT", "EMPLOYEECONTACTID", "0ce4b304-8f89-48eb-ad33-21334a786cb5", "2");
                //EmployeeEntryBLL bb = new EmployeeEntryBLL();
                //bb.UpdateCheckState("T_HR_EMPLOYEEENTRY", "EMPLOYEEENTRYID", "812cd3f0-5df7-497a-926c-dc0d32c94f4d", "2");
                //return null;
                ////SMT.HRM.Services.InstantMessagingService dd = new InstantMessagingService();
                //InstantMessagingDataBLL bb = new InstantMessagingDataBLL();
                //bb.GetAllEmployee();
                //dd.GetAllOrganization(100,100,100);
                //return null;
                //string checkid = "cb01bc8f-f47b-4732-aa2e-ed042a681b26";
                ////EmployeeContractAlarm(GetEmployeeContractByID(checkid));
                //T_HR_EMPLOYEECONTRACT entry = GetEmployeeContractByID(checkid);
                //if (entry != null)
                //{
                //    string strMsg = "";
                //    T_HR_EMPLOYEECONTRACT employeeContract = new T_HR_EMPLOYEECONTRACT();
                //    employeeContract.EMPLOYEECONTACTID = Guid.NewGuid().ToString();
                //    employeeContract.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                //    employeeContract.T_HR_EMPLOYEE.EMPLOYEEID = entry.T_HR_EMPLOYEE.EMPLOYEEID;
                //    employeeContract.FROMDATE = entry.FROMDATE;
                //    employeeContract.TODATE = entry.TODATE;
                //    employeeContract.ENDDATE = entry.ENDDATE;
                //    employeeContract.CONTACTPERIOD = entry.CONTACTPERIOD;
                //    employeeContract.CONTACTCODE = entry.CONTACTCODE;
                //    employeeContract.ATTACHMENT = entry.ATTACHMENT;
                //    employeeContract.ATTACHMENTPATH = entry.ATTACHMENTPATH;
                //    employeeContract.ALARMDAY = entry.ALARMDAY;
                //    employeeContract.CHECKSTATE = "0";
                //    employeeContract.EDITSTATE = "0";
                //    employeeContract.ISSPECIALCONTRACT = entry.ISSPECIALCONTRACT;
                //    employeeContract.REASON = entry.REASON;
                //    employeeContract.REMARK = entry.REMARK;
                //    employeeContract.OWNERID = entry.OWNERID;
                //    employeeContract.OWNERPOSTID = entry.OWNERPOSTID;
                //    employeeContract.OWNERDEPARTMENTID = entry.OWNERDEPARTMENTID;
                //    employeeContract.OWNERCOMPANYID = entry.OWNERCOMPANYID;
                //    employeeContract.CREATECOMPANYID = entry.CREATECOMPANYID;
                //    employeeContract.CREATEDATE = System.DateTime.Now;
                //    employeeContract.CREATEUSERID = entry.CREATEUSERID;
                //    employeeContract.CREATEPOSTID = entry.CREATEPOSTID;
                //    employeeContract.CREATEDEPARTMENTID = entry.CREATEDEPARTMENTID;
                //    employeeContract.CREATECOMPANYID = entry.CREATECOMPANYID;
                //    employeeContract.UPDATEUSERID = entry.UPDATEUSERID;
                //    employeeContract.UPDATEDATE = entry.UPDATEDATE;

                //    EmployeeContractAdd(employeeContract, ref strMsg);
                //    EmployeeContractAlarm(employeeContract);
                //}
                //return null;
                //T_HR_EMPLOYEEENTRY entry = GetEmployeeEntryByEmployeeID(checkid);
                //if (entry != null)
                //{
                //    string strMsg = "";
                //    T_HR_EMPLOYEECHECK employeeCheck = new T_HR_EMPLOYEECHECK();
                //    employeeCheck.BEREGULARID = Guid.NewGuid().ToString();
                //    employeeCheck.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                //    employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID = entry.T_HR_EMPLOYEE.EMPLOYEEID;
                //    employeeCheck.EMPLOYEECODE = entry.T_HR_EMPLOYEE.EMPLOYEECODE;
                //    employeeCheck.EMPLOYEENAME = entry.T_HR_EMPLOYEE.EMPLOYEEENAME;
                //    employeeCheck.PROBATIONPERIOD = entry.PROBATIONPERIOD;
                //    employeeCheck.REPORTDATE = entry.ENTRYDATE;
                //    employeeCheck.ONDUTYDATE = entry.ONPOSTDATE;
                //    employeeCheck.OWNERID = entry.OWNERID;
                //    employeeCheck.OWNERCOMPANYID = entry.OWNERCOMPANYID;
                //    employeeCheck.CREATEUSERID = entry.CREATEUSERID;
                //    EmployeeCheckAdd(employeeCheck, ref strMsg);
                //    EmployeeCheckAlarm(employeeCheck);
                //}
                //return null;
                //EngineService aa = new EngineService();

                //string bb = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
                //bb += "<Paras>";
                //bb += "<Para TableName=\"T_HR_LEFTOFFICECONFIRM\" Name=\"OWNERID\" Description=\"strOwnerID\" Value=\"\"></Para>";
                //bb += "<Para TableName=\"T_HR_LEFTOFFICECONFIRM\" Name=\"OWNERDEPARTMENTID\" Description=\"OWNERDEPARTMENTID\" Value=\"c1f72286-eee5-45bd-bded-5993e8a317c9\"></Para>";
                //bb += "<Para TableName=\"T_HR_LEFTOFFICECONFIRM\" Name=\"OWNERPOSTID\" Description=\"strOwnerID\" Value=\"b7c4f515-a0b8-42ff-a103-c178170a51f7\"></Para>";
                //bb += "<Para TableName=\"T_HR_LEFTOFFICECONFIRM\" Name=\"OWNERCOMPANYID\" Description=\"strOwnerID\" Value=\"bac05c76-0f5b-40ae-b73b-8be541ed35ed\"></Para>";
                //bb += "<Para TableName=\"T_HR_LEFTOFFICECONFIRM\"  Name=\"EMPLOYEEID\"  Description=\"员工ID\" Value=\"94085cae-4096-4ccb-bcf8-8ec0c882514b\" ValueName=\"员工ID\" ></Para>";


                //bb += "</Paras>";
                //aa.CallWaitAppService(bb);
                //return null;
            }
        }
        /// <summary>
        ///  根据部门ID获取部门负责人
        /// </summary>
        /// <param name="departmentID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetDepartmentLeaders(string departmentID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetDepartmentLeaders(departmentID);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="pageCount"></param>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetEmployeeDetailByParasPaging(int pageIndex, int pageSize, string sort, ref int pageCount, string[] companyIDs, string[] departmentIDs, string[] postIDs, string[] employeeIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeDetailByParasPaging(pageIndex, pageSize, sort, ref pageCount, companyIDs, departmentIDs, postIDs, employeeIDs);
            }
        }
        /// <summary>
        /// 根据员工ID集合查询所有员工
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工信息</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeeByIDs(string[] employeeIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeByIDs(employeeIDs);
            }
        }
        [OperationContract]
        public List<EmployeeContactWays> GetEmployeeToEngine(string[] employeeIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeToEngine(employeeIDs);
            }
        }
        /// <summary>
        /// 导出员工通讯录
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeContract()
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.ExportEmployeeContract();
            }
        }

        /// <summary>
        /// 根据员工ID得到员工信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工信息</returns>
        [OperationContract]
        public V_EMPLOYEEVIEW GetEmployeeInfoByEmployeeID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeInfoByEmployeeID(employeeID);
            }
        }

        /// <summary>
        /// 根据员工ID查询员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工详细信息</returns>
        [OperationContract]
        public V_EMPLOYEEPOST GetEmployeeDetailByID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeDetailByID(employeeID);
            }
        }
        /// <summary>
        /// 根据员工ID查询员工的岗位，部门，公司ID
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL GetEmployeeDetailViewByID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeDetailView(employeeID);
            }
        }
        /// <summary>
        /// 根据员工ID集合查询员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID集合</param>
        /// <returns>员工详细信息</returns>
        [OperationContract]
        public List<V_EMPLOYEEPOST> GetEmployeeDetailByIDs(string[] employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeDetailByIDs(employeeID);
            }
        }

        /// <summary>
        /// 根据员工ID获取员工照片
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>员工照片，为byte，没有返回null</returns>
        [OperationContract]
        public byte[] GetEmployeePhotoByID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeePhotoByID(employeeID);
            }
        }

        /// <summary>
        /// 根据员工id获取员工工作时间，以员工入职里面的入职时间计算
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <returns>工作时间</returns>
        [OperationContract]
        public int GetEmployeeWorkAgeByID(string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeWorkAgeByID(employeeID);
            }
        }

        /// <summary>
        /// 删除员工档案
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool EmployeeDelete(string[] employeeIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                int rslt = bll.EmployeeDelete(employeeIDs);

                return (rslt > 0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeIDs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeeByDepartmentID(string departmentID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                var q = bll.GetEmployeeByDepartmentID(departmentID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeePaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                IQueryable<T_HR_EMPLOYEE> q = bll.GetEmployeesPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 员工信息视图
        /// </summary>
        /// <param name="pageIndex">当前页数，(必填，最小为1)</param>
        /// <param name="pageSize">每页个数，(必填，最小为1)</param>
        /// <param name="sort">排序字段，可为空</param>
        /// <param name="filterString">过滤条件，没有请填空</param>
        /// <param name="paras">过滤条件值，没有请填空数组</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="sType">机构类型：公司“Company”，部门“Department",岗位：”Post“(必填)</param>
        /// <param name="sValue">机构id，对应的公司，部门，岗位id(必填)</param>
        /// <param name="userID">当前操作用户的员工id(必填)</param>
        /// <returns>员工信息视图</returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetEmployeeViewsPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                return q;
            }
        }


        /// <summary>
        /// 员工信息视图
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeBasicInfoPagingView(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetEmployeeBasicInfoPagingView(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                return q;
            }
        }


        /// <summary>
        /// 根据员工ID集合返回员工视图集合
        /// </summary>
        /// <param name="Employeeids">员工ID集合</param>
        /// <returns>返回员工视图集合</returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeDetailViewByEmployeeID(string[] Employeeids)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeesByEmployeeIds(Employeeids);
            }
        }
        /// <summary>
        /// 根据参数获取员工ID
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="departmentIDs"></param>
        /// <param name="postIDs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetEmployeeIDsByParas(IList<string> companyIDs, IList<string> departmentIDs, IList<string> postIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<string> q = bll.GetEmployeeIDsByParas(companyIDs, departmentIDs, postIDs);
                return q;
            }
        }

        /// <summary>
        /// 获取员工
        /// </summary>
        /// <param name="companyIDs"></param>
        /// <param name="isContainChildCompany"> 是否获取子公司</param>
        /// <param name="departmentIDs"></param>
        /// <param name="isContainChildDepartment">是否获取子部门</param>
        /// <param name="postIDs"></param>
        /// <returns></returns>
        [OperationContract]
        public List<string> GetEmployeeIDsWithParas(List<string> companyIDs, bool isContainChildCompany, List<string> departmentIDs, bool isContainChildDepartment, List<string> postIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<string> q = bll.GetEmployeeIDsWithParas(companyIDs, isContainChildCompany, departmentIDs, isContainChildDepartment, postIDs);
                return q;
            }
        }

        /// <summary>
        /// 获取离职员工
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetLeaveEmployeeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetLeaveEmployeeViewsPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                return q;
            }
        }
        /// <summary>
        /// 有效员工信息视图
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsActivedPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetEmployeeViewsActivedPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
                return q;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeesWithOutPermissions(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                IQueryable<T_HR_EMPLOYEE> q = bll.GetEmployeesWithOutPermissions(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 员工视图分页（用户授权时调用）
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeViewsWithOutPermissions(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                List<V_EMPLOYEEVIEW> q = bll.GetEmployeeViewsWithOutPermissions(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue);
                return q;
            }
        }
        /// <summary>
        /// 获取员工集合
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数值</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeePagingByFilter(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                IQueryable<T_HR_EMPLOYEE> q = bll.GetEmployeesPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        [OperationContract]
        public List<V_EMPLOYEESTATICINFO> GetEmployeesIntime(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeesIntime(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
            }
        }
        [OperationContract]
        public byte[] ExportEmployeesIntime(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.ExportEmployeesIntime(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, sType, sValue, userID);
            }
        }
        /// <summary>
        /// 根据身份证号码判断
        /// </summary>
        /// <param name="sNumberID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeIsEntry(string sNumberID, ref string blackMessage, ref string[] leaveMessage)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.EmployeeIsEntry(sNumberID, ref blackMessage, ref leaveMessage);
            }
        }
        /// <summary>
        /// 根据身份证号码，查询简历库是否有员工信息，如果有，就赋值给员工实体
        /// </summary>
        /// <param name="sNumberID">身份证号码</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEE GetEmployeeByNumberID(string sNumberID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeByNumberID(sNumberID);
            }
        }        /// <summary>
        /// 根据身份证号码，查询简历库是否有员工信息，如果有，就赋值给员工实体
        /// </summary>
        /// <param name="sNumberID">身份证号码</param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL GetEmployeeByFingerPrintID(string FingerPrintID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeOrgByFingerPrintID(FingerPrintID);
            }
        }

        /// <summary>
        /// 登录验证，并返回员工组织架构信息
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="UserPassword"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL GetEmployeeOrgByEmployeeid(string employeeid)
        {
            try
            {
                using (EmployeeBLL bll = new EmployeeBLL())
                {
                    V_EMPLOYEEDETAIL employee = bll.GetEmployeeDetailView(employeeid);
                    if (employee == null)
                    {
                        Tracer.Debug("通过员工id获取到员工详细信息为空，员工id：" + employeeid);
                        return null;
                    }
                    return employee;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("通过员工id获取到员工详细信息出错，员工id：" + employeeid + ex.ToString());
                throw ex;
            }
        }
        /// <summary>
        /// 是否存在相同的指纹编号
        /// </summary>
        /// <param name="FingerPrintID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsExistFingerPrintID(string FingerPrintID, string employeeID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.IsExistFingerPrintID(FingerPrintID, employeeID);
            }
        }

        /// <summary>
        /// 根据截止时间查在岗人数
        /// </summary>
        /// <param name="strOwnerID">登录员工ID</param>
        /// <param name="strOrgType">机构类型</param>
        /// <param name="strOrgID">机构ID</param>
        /// <param name="dtCheckDate">截止时间</param>
        /// <param name="strMsg">返回处理消息</param>
        /// <returns></returns>
        [OperationContract]
        public int GetInserviceEmployeeCount(string strOwnerID, string strOrgType, string strOrgID, DateTime dtCheckDate, ref string strMsg)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetInserviceEmployeeCount(strOwnerID, strOrgType, strOrgID, dtCheckDate, ref strMsg);
            }
        }

        /// <summary>
        /// 根据传入的实体名和主键id，去相应实体里面根据
        /// employeeID，postID，departmentID，companyID找出相应的
        /// 员工姓名，岗位名称，部门名称和公司名称
        /// </summary>
        /// <param name="employeeID">员工id</param>
        /// <param name="postID">岗位id</param>
        /// <param name="departmentID">部门id</param>
        /// <param name="companyID">公司id</param>
        /// <returns>相应员工的组织架构名称</returns>
        [OperationContract]
        public V_EMPLOYEEVIEW GetEmpOrgInfoByID(string employeeID, string postID, string departmentID, string companyID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmpOrgInfoByID(employeeID, postID, departmentID, companyID);
            }
        }
        #endregion

        #region T_HR_RESUME 简历库
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_RESUME> GetResumePaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount)
        {
            using (ResumeBLL bll = new ResumeBLL())
            {
                IQueryable<T_HR_RESUME> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 根据简历ID返回简历库信息
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>返回简历实体</returns>
        [OperationContract]
        public T_HR_RESUME GetResumeByid(string resumeID)
        {
            using (ResumeBLL bll = new ResumeBLL())
            {
                return bll.GetResumeByid(resumeID);
            }
        }
        /// <summary>
        /// 添加简历信息
        /// </summary>
        /// <param name="entity">简历信息实体</param>
        /// <param name="experience">学历信息列表</param>
        /// <param name="eduHistory">工作经历信息列表</param>
        [OperationContract]
        public void ResumeAdd(T_HR_RESUME entity, T_HR_EXPERIENCE[] experience, T_HR_EDUCATEHISTORY[] eduHistory, ref string strMsg)
        {
            using (ResumeBLL bll = new ResumeBLL())
            {
                bll.ResumeAdd(entity, ref strMsg);
                ExperienceBLL expbll = new ExperienceBLL();
                foreach (var exp in experience)
                {
                    expbll.ExperienceAdd(exp);
                }
                EducateHistoryBLL edubll = new EducateHistoryBLL();
                foreach (var edu in eduHistory)
                {
                    edubll.EducateHistoryAdd(edu);
                }
            }
        }
        /// <summary>
        /// 更新简历信息
        /// </summary>
        /// <param name="entity">简历信息实体</param>
        /// <param name="experience">学历信息列表</param>
        /// <param name="eduHistory">工作经历信息列表</param>
        /// <param name="delexps">待删除的学历信息列表</param>
        /// <param name="deledus">待删除的工作经历信息列表</param>
        [OperationContract]
        public void ResumeUpdate(T_HR_RESUME entity, List<T_HR_EXPERIENCE> experience, List<T_HR_EDUCATEHISTORY> eduHistory, List<T_HR_EXPERIENCE> delexps, List<T_HR_EDUCATEHISTORY> deledus, ref string strMsg)
        {
            using (ResumeBLL bll = new ResumeBLL())
            {
                bll.ResumeUpdate(entity, ref  strMsg);
                ExperienceBLL expbll = new ExperienceBLL();
                expbll.ExperienceDelete(delexps);
                expbll.ExperienceUpdate(experience);
                EducateHistoryBLL edubll = new EducateHistoryBLL();
                edubll.EducateHistoryDelete(deledus);
                edubll.EducateHistoryUpdate(eduHistory);
            }
        }
        /// <summary>
        /// 删除简历信息
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>是否删除</returns>
        [OperationContract]
        public bool ResumeDelete(string[] resumeID)
        {
            using (ResumeBLL bll = new ResumeBLL())
            {
                return (bll.ResumeDelete(resumeID) == 0);
            }
        }

        /// <summary>
        /// 根据身份证号码读取简历基本信息
        /// </summary>
        /// <param name="sNumberId"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_RESUME GetResumeByNumber(string sNumberId)
        {

            using (ResumeBLL bll = new ResumeBLL())
            {
                return bll.GetResumeByNumber(sNumberId);
            }
        }
        #endregion

        #region T_HR_EXPERIENCE 工作经历
        /// <summary>
        /// 根据简历ID,返回工作经历信息
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>返回工作经历列表</returns>
        [OperationContract]
        public List<T_HR_EXPERIENCE> GetExperienceAll(string resumeID)
        {
            using (ExperienceBLL bll = new ExperienceBLL())
            {
                IQueryable<T_HR_EXPERIENCE> q = bll.GetExperienceAll(resumeID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 新增工作经历信息
        /// </summary>
        /// <param name="entity">工作经历实体</param>
        [OperationContract]
        public void ExperienceAdd(T_HR_EXPERIENCE entity)
        {
            using (ExperienceBLL bll = new ExperienceBLL())
            {
                bll.ExperienceAdd(entity);
            }
        }
        /// <summary>
        /// 更新工作经历信息
        /// </summary>
        /// <param name="entity">工作经历实体</param>
        [OperationContract]
        public int ExperienceUpdate(List<T_HR_EXPERIENCE> entity)
        {
            using (ExperienceBLL bll = new ExperienceBLL())
            {
                return bll.ExperienceUpdate(entity);
            }
        }

        /// <summary>
        /// 删除工作经历
        /// </summary>
        /// <param name="deledus">待删除的工作经历信息列表</param>
        [OperationContract]
        public int ExperienceDelete(List<T_HR_EXPERIENCE> delexps)
        {
            using (ExperienceBLL expbll = new ExperienceBLL())
            {
                return expbll.ExperienceDelete(delexps);
            }
        }
        #endregion

        #region T_HR_EDUCATEHISTORY 教育培训记录
        /// <summary>
        /// 返回当前简历ID所有的教育记录信息
        /// </summary>
        /// <param name="resumeID">简历ID</param>
        /// <returns>返回教育记录信息</returns>
        [OperationContract]
        public List<T_HR_EDUCATEHISTORY> GetEducateHistoryAll(string resumeID)
        {
            using (EducateHistoryBLL bll = new EducateHistoryBLL())
            {
                IQueryable<T_HR_EDUCATEHISTORY> q = bll.GetEducateHistoryAll(resumeID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 新增教育记录信息
        /// </summary>
        /// <param name="entity">教育记录实体</param>
        [OperationContract]
        public void EducateHistoryAdd(T_HR_EDUCATEHISTORY entity)
        {
            using (EducateHistoryBLL bll = new EducateHistoryBLL())
            {
                bll.EducateHistoryAdd(entity);
            }
        }
        /// <summary>
        /// 更新教育记录信息
        /// </summary>
        /// <param name="entity">教育记录实体</param>
        [OperationContract]
        public int EducateHistoryUpdate(List<T_HR_EDUCATEHISTORY> entity)
        {
            using (EducateHistoryBLL bll = new EducateHistoryBLL())
            {
                return bll.EducateHistoryUpdate(entity);
            }
        }

        /// <summary>
        /// 删除教育经历
        /// </summary>>
        /// <param name="delexps">待删除的学历信息列表</param>
        [OperationContract]
        public int EducateHistoryDelete(List<T_HR_EDUCATEHISTORY> deledus)
        {
            using (EducateHistoryBLL edubll = new EducateHistoryBLL())
            {
                return edubll.EducateHistoryDelete(deledus);
            }
        }
        #endregion

        #region T_HR_BLACKLIST 添加黑名单
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_BLACKLIST> GetBlackListPaging(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int pageCount, string userID)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                IQueryable<T_HR_BLACKLIST> q = bll.GetBlackListPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 添加黑名单记录
        /// </summary>
        /// <param name="entity">黑名单实体</param>
        [OperationContract]
        public string BlackListAdd(T_HR_BLACKLIST entity)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                return bll.BlackListAdd(entity);
            }
        }
        /// <summary>
        /// 查看姓名是否在黑名单中
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        public bool CheckBlackListByName(string name)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                return bll.CheckBlackListByName(name);
            }
        }
        /// <summary>
        /// 更新黑名单记录
        /// </summary>
        /// <param name="entity">黑名单实体</param>
        [OperationContract]
        public void BlackListUpdate(T_HR_BLACKLIST entity, ref string strMsg)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                bll.BlackListUpdate(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 删除黑名单实体
        /// </summary>
        /// <param name="blackLists">黑名单ID组</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool BlackListDelete(string[] blackLists)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                int rslt = bll.BlackListDelete(blackLists);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据黑名单ID查询实体
        /// </summary>
        /// <param name="blackListID">黑名单ID</param>
        /// <returns>返回黑名单实体</returns>
        [OperationContract]
        public T_HR_BLACKLIST GetBlackListByID(string blackListID)
        {
            using (BlackListBLL bll = new BlackListBLL())
            {
                return bll.GetBlackListByID(blackListID);
            }
        }
        #endregion


        #region T_HR_EMPLOYEEENTRY 入职
        /// <summary>
        /// 添加员工入职信息
        /// </summary>
        /// <param name="entity">员工入职实体</param>
        [OperationContract]
        public void EmployeeEntryAdd(T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEPOST ent)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                bll.EmployeeEntryAdd(entity, ent);
            }
        }
        /// <summary>
        /// 添加员工入职信息
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="entity"></param>
        /// <param name="ent"></param>
        [OperationContract]
        public string AddEmployeeEntry(T_HR_EMPLOYEE employee, T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEPOST ent)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.EmployeeEntryAdd(employee, entity, ent);
            }
        }
        /// <summary>
        /// 更新员工入职信息
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void EmployeeEntryUpdate(T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEPOST ent)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                bll.EmployeeEntryUpdate(entity, ent);
            }
        }

        /// <summary>
        /// 删除员工入职信息
        /// </summary>
        /// <param name="employeeEntryID">入职信息ID</param>
        /// <returns>是否成功删除</returns>
        [OperationContract]
        public bool EmployeeEntryDelete(string[] employeeEntryIDs)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                int rslt = bll.EmployeeEntryDelete(employeeEntryIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_EMPLOYEEENTRY> GetEmployeeEntryPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string sType, string sValue, string userID, string checkState)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                List<V_EMPLOYEEENTRY> q = bll.EmployeeEntryPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, sType, sValue, userID, checkState);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }

        /// <summary>
        /// 根据入职ID获取员工入职的信息
        /// </summary>
        /// <param name="employeeEntryID">员工入职ID</param>
        /// <returns>员工入职信息</returns>
        [OperationContract]
        public T_HR_EMPLOYEEENTRY GetEmployeeEntryByID(string employeeEntryID)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.GetEmployeeEntryByID(employeeEntryID);
            }
        }

        /// <summary>
        /// 根据员工ID获取员工入职的信息
        /// </summary>
        /// <param name="employeeEntryID">员工ID</param>
        /// <returns>员工入职信息</returns>
        [OperationContract]
        public T_HR_EMPLOYEEENTRY GetEmployeeEntryByEmployeeID(string employeeID)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.GetEmployeeEntryByEmployeeID(employeeID);
            }
        }


        /// <summary>
        /// 根据员工ID获取员工入职的信息 weirui 2012/8/24 重载 员工ID和公司ID一起查询
        /// </summary>
        /// <param name="employeeEntryID">员工ID</param>
        /// <returns>员工入职信息</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEENTRY> GetEmployeeEntryByEmployeeIDAndCOMPANYID(string employeeID, string COMPANYID)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.GetEmployeeEntryByEmployeeIDAndCOMPANYID(employeeID, COMPANYID);
            }
        }
        /// <summary>
        /// 获取员工入职批量导入信息
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="companyID"></param>
        /// <param name="empInfoDic"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EmployeeEntryInfo> ImportEmployeeEntry(UploadFileModel uploadFile, string companyID, Dictionary<string, string> empInfoDic)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                string strPath = string.Empty;
                SaveFile(uploadFile, out strPath);//获取文件路径
                string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);//到时测试strPath为空是是否报错
                return bll.ImportEmployeeEntry(strPhysicalPath, companyID, empInfoDic);
            }
        }

        /// <summary>
        /// 验证用户名是否存在
        /// </summary>
        /// <param name="listEmpInfo"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EmployeeEntryInfo> ValidUserNameIsExist(List<V_EmployeeEntryInfo> listEmpInfo)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.ValidUserNameIsExist(listEmpInfo);
            }
        }
        /// <summary>
        /// 批量添加员工入职信息
        /// </summary>
        /// <param name="listEmpEntry"></param>
        /// <param name="companyID"></param>
        /// <param name="strMsg">错误信息等</param>
        /// <returns></returns>
        [OperationContract]
        public bool AddBatchEmployeeEntry(List<V_EmployeeEntryInfo> listEmpEntry, string companyID, ref string strMsg)
        {
            using (EmployeeEntryBLL bll = new EmployeeEntryBLL())
            {
                return bll.AddBatchEmployeeEntry(listEmpEntry, companyID, ref strMsg);
            }
        }
        #endregion

        #region T_HR_PENSIONALARMSET 社保提醒设置
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<V_PENSIONALARMSET> GetPensionAlarmSetPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                IQueryable<V_PENSIONALARMSET> q = bll.GetPensionAlarmSetPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 添加社保提醒设置
        /// </summary>
        /// <param name="entity">社保提醒设置实体</param>
        [OperationContract]
        public void PensionAlarmSetAdd(T_HR_PENSIONALARMSET entity, ref string strMsg)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                bll.PensionAlarmSetAdd(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 更新社保提醒设置
        /// </summary>
        /// <param name="entity">社保提醒设置实体</param>
        [OperationContract]
        public void PensionAlarmSetUpdate(T_HR_PENSIONALARMSET entity)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                bll.PensionAlarmSetUpdate(entity);
            }
        }
        /// <summary>
        /// 根据ID删除社保提醒设置
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID</param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool PensionAlarmSetDelete(string[] pensionAlarmSetIDs)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                int rslt = bll.PensionAlarmSetDelete(pensionAlarmSetIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据社保提醒设置ID获取实体
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID</param>
        /// <returns>获取社保提醒设置信息</returns>
        [OperationContract]
        public T_HR_PENSIONALARMSET GetPensionAlarmSetByID(string pensionAlarmSetID)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                return bll.GetPensionAlarmSetByID(pensionAlarmSetID);
            }
        }
        /// <summary>
        /// 根据社保提醒设置ID获取实体
        /// </summary>
        /// <param name="pensionAlarmSetID">社保提醒设置ID</param>
        /// <returns>获取社保提醒设置信息</returns>
        [OperationContract]
        public V_PENSIONALARMSET GetPensionAlarmSetViewByID(string pensionAlarmSetID)
        {
            using (PensionAlarmSetBLL bll = new PensionAlarmSetBLL())
            {
                return bll.GetPensionAlarmSetViewByID(pensionAlarmSetID);
            }
        }

        #endregion

        #region T_HR_PENSIONMASTER 社保档案
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_PENSIONMASTER> PensionMasterPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckstate, string userID)
        {
            using (PensionMasterBLL bll = new PensionMasterBLL())
            {
                IQueryable<T_HR_PENSIONMASTER> q = bll.PensionMasterPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckstate, userID);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }

        /// <summary>
        /// 员工社保缴交日期字段数据导入到员工个人档案里去（PL/SQL也可以）
        /// </summary>
        [OperationContract]
        public void PensionMaterToEmployee()
        {
            try
            {
                using (PensionMasterBLL bll = new PensionMasterBLL())
                {
                    bll.PensionMaterToEmployee();
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.Message);
            }
        }
        /// <summary>
        /// 新增社保档案记录
        /// </summary>
        /// <param name="entity">社保档案实体</param>
        [OperationContract]
        public void PensionMasterAdd(T_HR_PENSIONMASTER entity, ref string strMsg)
        {
            try
            {
                using (PensionMasterBLL bll = new PensionMasterBLL())
                {
                    bll.PensionMasterAdd(entity, ref strMsg);

                    //entity.SOCIALSERVICE;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.Message);
            }

        }
        /// <summary>
        /// 更新社保档案记录
        /// </summary>
        /// <param name="entity">社保档案实体</param>
        [OperationContract]
        public void PensionMasterUpdate(T_HR_PENSIONMASTER entity)
        {
            try
            {
                using (PensionMasterBLL bll = new PensionMasterBLL())
                {
                    bll.PensionMasterUpdate(entity);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.Message);
            }
        }
        /// <summary>
        /// 可删除多条记录
        /// </summary>
        /// <param name="pensionMasterID">社保档案ID组</param>
        /// <returns></returns>
        [OperationContract]
        public bool PensionMasterDelete(string[] pensionMasterIDs)
        {
            try
            {
                using (PensionMasterBLL bll = new PensionMasterBLL())
                {
                    int rslt = bll.PensionMasterDelete(pensionMasterIDs);
                    return (rslt > 0);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("*********MY*********" + ex.Message + pensionMasterIDs[0]);
                return false;
            }
        }
        /// <summary>
        /// 根据社保档案ID查询实体
        /// </summary>
        /// <param name="pensionMasterID">社保档案ID</param>
        /// <returns>返回社保档案信息</returns>
        [OperationContract]
        public T_HR_PENSIONMASTER GetPensionMasterByID(string pensionMasterID)
        {
            using (PensionMasterBLL bll = new PensionMasterBLL())
            {
                return bll.GetPensionMasterByID(pensionMasterID);
            }
        }
        /// <summary>
        /// 根据员工ID获取社保档案
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_PENSIONMASTER GetPensionMasterByEmployeeID(string employeeID)
        {
            using (PensionMasterBLL bll = new PensionMasterBLL())
            {
                return bll.GetPensionMasterByEmployeeID(employeeID);
            }
        }
        /// <summary>
        /// 更新员工工龄
        /// </summary>
        /// <param name="companyID"></param>
        [OperationContract]
        public void UpdateEmployeeWorkAgeByID(string companyID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                bll.UpdateEmployeeWorkAgeByID(companyID);
            }
        }
        #endregion

        #region T_HR_PENSIONDETAIL 社保记录
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_PENSIONDETAIL> PensionDetailPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (PensionDetailBLL bll = new PensionDetailBLL())
            {
                IQueryable<T_HR_PENSIONDETAIL> q = bll.PensionDetailPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }


        /// <summary>
        /// 导出社保记录
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <param name="CompanyID"></param>
        /// <param name="StrYear"></param>
        /// <param name="StrMonth"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportPensionDetailReport(string sort, string filterString, object[] paras, string userID, string CompanyID, string StrYear, string StrMonth)
        {
            using (PensionDetailBLL bll = new PensionDetailBLL())
            {
                return bll.ExportPensionDetailReport( sort, filterString, paras, userID, CompanyID, StrYear, StrMonth);
            }
        }
        /// <summary>
        /// 删除社保记录
        /// </summary>
        /// <param name="pensionDetailIDs">社保记录ID组</param>
        /// <returns>是否成功删除</returns>
        [OperationContract]
        public bool PensionDetailDelete(string[] pensionDetailIDs)
        {
            using (PensionDetailBLL bll = new PensionDetailBLL())
            {
                int rslt = bll.PensionDetailDelete(pensionDetailIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 导入Excel的员工社保记录
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public void ImportClockInRdListFromExcel(UploadFileModel UploadFile, Dictionary<string, string> paras, ref string strMsg)
        {
            try
            {
                Tracer.Debug("import start" + paras.Count.ToString());
                string strPath = string.Empty;
                SaveFile(UploadFile, out strPath);
                string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

                using (PensionDetailBLL bll = new PensionDetailBLL())
                {
                    bll.ImportPensionByImportExcel(strPhysicalPath, paras, ref strMsg);
                }
                Tracer.Debug("import sucess");
            }
            catch (Exception ex)
            {
                Tracer.Debug("ImportClockInRdListFromExcelWS:" + ex.Message);
            }
        }



        /// <summary>
        /// 导入Excel的员工社保记录返回并显示  ljx
        /// </summary>
        /// <param name="UploadFile">上传载体</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        [OperationContract]
        public List<T_HR_PENSIONDETAIL> ImportClockInRdListFromExcelForShow(UploadFileModel UploadFile, Dictionary<string, string> paras, ref string strMsg)
        {
            List<T_HR_PENSIONDETAIL> ListResult = new List<T_HR_PENSIONDETAIL>();
            try
            {
                Tracer.Debug("import start" + paras.Count.ToString());
                string strPath = string.Empty;
                SaveFile(UploadFile, out strPath);
                string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

                using (PensionDetailBLL bll = new PensionDetailBLL())
                {
                    ListResult = bll.ImportPensionByImportExcelForShow(strPhysicalPath, paras, ref strMsg);
                }
                Tracer.Debug("import sucess");
            }
            catch (Exception ex)
            {
                Tracer.Debug("ImportClockInRdListFromExcelWS:" + ex.Message);
            }
            return ListResult;
        }

        /// <summary>
        /// 批量添加员工社保
        /// </summary>
        /// <param name="ListPension"></param>
        /// <param name="StrMsg"></param>
        /// <returns></returns>
        [OperationContract]
        public bool BatchAddPensionDetail(List<T_HR_PENSIONDETAIL> listPension,Dictionary<string, string> paras, ref string StrMsg)
        {
            bool IsResult = false;
            try
            {
                Tracer.Debug("导入社保的数量："+listPension.Count().ToString());
                Tracer.Debug("开始导入社保时间：" + System.DateTime.Now.ToString());
                

                using (PensionDetailBLL bll = new PensionDetailBLL())
                {
                    IsResult = bll.BatchAddPensionDetail(listPension, paras, ref StrMsg);
                }
                Tracer.Debug("import end");
                Tracer.Debug("社保导入结束时间："+System.DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                Tracer.Debug("ImportClockInRdListFromExcelWS:" + ex.Message);
            }
            return IsResult;
        }
        //[OperationContract]
        //public void ImportClockInRdListFromExcel(UploadFileModel UploadFile, string employeeID, ref string strMsg)
        //{
        //    string strPath = string.Empty;
        //    SaveFile(UploadFile, out strPath);
        //    string strPhysicalPath = HttpContext.Current.Server.MapPath(strPath);

        //    PensionDetailBLL bll = new PensionDetailBLL();
        //    bll.ImportPensionByImportExcel(strPhysicalPath, employeeID, ref strMsg);
        //}
        #endregion

        #region T_HR_EMPLOYEEINSURANCE 保险记录
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEINSURANCE> EmployeeInsurancePaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string strCheckState, string userID)
        {
            using (EmployeeInsuranceBLL bll = new EmployeeInsuranceBLL())
            {
                IQueryable<T_HR_EMPLOYEEINSURANCE> q = bll.EmployeeInsurancePaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckState, userID);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            //  return q.Count() > 0 ? q.ToList() : null;
        }

        /// <summary>
        /// 添加保险记录
        /// </summary>
        /// <param name="entity">保险记录实体</param>
        [OperationContract]
        public void EmployeeInsuranceAdd(T_HR_EMPLOYEEINSURANCE entity, ref string strMsg)
        {
            using (EmployeeInsuranceBLL bll = new EmployeeInsuranceBLL())
            {
                bll.EmployeeInsuranceAdd(entity, ref  strMsg);
            }
        }

        /// <summary>
        /// 更新保险记录
        /// </summary>
        /// <param name="entity">保险记录实体</param>
        [OperationContract]
        public void EmployeeInsuranceUpdate(T_HR_EMPLOYEEINSURANCE entity, ref string strMsg)
        {
            using (EmployeeInsuranceBLL bll = new EmployeeInsuranceBLL())
            {
                bll.EmployeeInsuranceUpdate(entity, ref  strMsg);
            }
        }

        /// <summary>
        /// 删除多项保险记录
        /// </summary>
        /// <param name="employInsuranceIDs">保险记录组</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeInsuranceDelete(string[] employInsuranceIDs)
        {
            using (EmployeeInsuranceBLL bll = new EmployeeInsuranceBLL())
            {
                int rslt = bll.EmployeeInsuranceDelete(employInsuranceIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据保险记录ID查询保险记录实体
        /// </summary>
        /// <param name="employInsuranceID">保险记录ID</param>
        /// <returns>返回记录实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEEINSURANCE GetEmployeeInsuranceByID(string employInsuranceID)
        {
            using (EmployeeInsuranceBLL bll = new EmployeeInsuranceBLL())
            {
                return bll.GetEmployeeInsuranceByID(employInsuranceID);
            }
        }
        #endregion

        #region T_HR_EMPLOYEECHECK 转正审核
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECHECK> EmployeeCheckPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckstate, string userID)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                IQueryable<T_HR_EMPLOYEECHECK> q = bll.EmployeeCheckPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckstate, userID);
                if (q != null)
                {
                    return q.Count() > 0 ? bll.EmployeecheckWithEmployeecode(q) : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }
        /// <summary>
        /// 根据转正审核ID获取信息
        /// </summary>
        /// <param name="employeeCheckID">转正审核ID</param>
        /// <returns>转正审核实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEECHECK GetEmployeeCheckByID(string employeeCheckID)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                return bll.GetEmployeeCheckByID(employeeCheckID);
            }
        }
        /// <summary>
        /// 添加转正审核信息
        /// </summary>
        /// <param name="entity">转正审核实体</param>
        [OperationContract]
        public void EmployeeCheckAdd(T_HR_EMPLOYEECHECK entity, ref string strMsg)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                bll.EmployeeCheckAdd(entity, ref strMsg);
            }
        }
        /// <summary>
        /// 修改转正审核信息
        /// </summary>
        /// <param name="entity">转正审核实体</param>
        [OperationContract]
        public void EmployeeCheckUpdate(T_HR_EMPLOYEECHECK entity)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                bll.EmployeeCheckUpdate(entity);
            }
        }
        /// <summary>
        /// 可删除转正审核组
        /// </summary>
        /// <param name="employeeCheckIDs">转正审核ID组</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeCheckDelete(string[] employeeCheckIDs)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                int rslt = bll.EmployeeCheckDelete(employeeCheckIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 转正提醒xml
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void GetEmployeeCheckEngineXml(T_HR_EMPLOYEEENTRY entity)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                bll.GetEmployeeCheckEngineXml(entity);
            }
        }
        /// <summary>
        /// 指定提醒日期
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void EmployeeCheckAlarm(T_HR_EMPLOYEECHECK entity)
        {
            using (EmployeeCheckBLL bll = new EmployeeCheckBLL())
            {
                bll.EmployeeCheckAlarm(entity);
            }
        }
        #endregion

        #region T_HR_EMPLOYEEPOSTCHANGE 员工异动记录
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOSTCHANGE> EmployeePostChangePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                IQueryable<T_HR_EMPLOYEEPOSTCHANGE> q = bll.EmployeePostChangePaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CheckState);

                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            //return q.Count() > 0 ? q.ToList() : null;
        }

        [OperationContract]
        public List<V_EMPLOYEEPOSTCHANGE> EmployeePostChangeViewPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                IQueryable<V_EMPLOYEEPOSTCHANGE> q = bll.EmployeePostChangeViewPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CheckState);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 添加员工异动记录
        /// </summary>
        /// <param name="entity">员工异动实体</param>
        [OperationContract]
        public void EmployeePostChangeAdd(T_HR_EMPLOYEEPOSTCHANGE entity, ref string strMsg)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                bll.EmployeePostChangeAdd(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 更新员工异动记录
        /// </summary>
        /// <param name="entity">员工异动实体</param>
        [OperationContract]
        public void EmployeePostChangeUpdate(T_HR_EMPLOYEEPOSTCHANGE entity, ref string strMsg)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                bll.EmployeePostChangeUpdate(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 删除员工异动记录
        /// </summary>
        /// <param name="employeePostChangeIDs">员工异动记录ID组</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeePostChangeDelete(string[] employeePostChangeIDs)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                int rslt = bll.EmployeePostChangeDelete(employeePostChangeIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据员工记录ID获取员工异动记录信息
        /// </summary>
        /// <param name="employeePostChangeID">员工异动记录ID</param>
        /// <returns>返回员工异动记录信息</returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOSTCHANGE GetEmployeePostChangeByID(string employeePostChangeID)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                return bll.GetEmployeePostChangeByID(employeePostChangeID);
            }
        }
        /// <summary>
        /// 根据员工ID获取最新的非代理异动
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOSTCHANGE GetLastChangeByEmployeeID(string employeeID)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                return bll.GetLastChangeByEmployeeID(employeeID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeePostChangeID"></param>
        /// <returns></returns>
        [OperationContract]
        public string EmployeePostChange(T_HR_EMPLOYEEPOSTCHANGE entity)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                return bll.EmployeePostChange(entity);
            }
        }
        /// <summary>
        /// 为员工入职添加异动记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public void AddEmployeePostChangeForEntry(T_HR_EMPLOYEEPOSTCHANGE entity)
        {
            using (EmployeePostChangeBLL bll = new EmployeePostChangeBLL())
            {
                bll.AddEmployeePostChangeForEntry(entity);
            }
        }

        /// <summary>
        /// 检查是否有出差信息
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        [OperationContract]
        public Dictionary<string, string> CheckBusinesstrip(string employeeid)
        {
            using (EmployeePostChangeBLL PgBll = new EmployeePostChangeBLL())
            {
                return PgBll.CheckBusinesstrip(employeeid);
            }
        }
        #endregion

        #region T_HR_LEFTOFFICE 离职申请记录
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_LEFTOFFICE> LeftOfficePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                IQueryable<T_HR_LEFTOFFICE> q = bll.LeftOfficePaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CheckState);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }

        [OperationContract]
        public List<V_LEFTOFFICEVIEW> LeftOfficeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                IQueryable<V_LEFTOFFICEVIEW> q = bll.LeftOfficeViewsPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CheckState);
                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }
        /// <summary>
        /// 添加离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        [OperationContract]
        public void LeftOfficeAdd(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                bll.LeftOfficeAdd(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 更新离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        [OperationContract]
        public void LeftOfficeUpdate(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                bll.LeftOfficeUpdate(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 删除离职申请记录
        /// </summary>
        /// <param name="dimissionIDs"></param>
        /// <returns>是否成功删除</returns>
        [OperationContract]
        public int LeftOfficeDelete(string[] dimissionIDs)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                int rslt = bll.LeftOfficeDelete(dimissionIDs);
                return rslt;
            }
        }
        /// <summary>
        /// 根据离职申请记录ID获取信息
        /// </summary>
        /// <param name="dimissionID">离职信息ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_LEFTOFFICE GetLeftOfficeByID(string dimissionID)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                return bll.GetLeftOfficeByID(dimissionID);
            }
        }

        /// <summary>
        /// 根据员工ID和岗位ID获取信息
        /// </summary>
        /// <param name="EmployeeID">员工ID</param>
        /// <param name="PostID">岗位ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_LEFTOFFICE GetLeftOfficeByEmployeeIDAndPostID(string EmployeeID,string Postid)
        {
            using (LeftOfficeBLL bll = new LeftOfficeBLL())
            {
                return bll.GetLeftOfficeByEmployeeIDAndPostID(EmployeeID,Postid);
            }
        }
        #endregion

        #region T_HR_CHECKPROJECTSET 考核项目
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_CHECKPROJECTSET> CheckProjectSetPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                IQueryable<T_HR_CHECKPROJECTSET> q = bll.QueryWithPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 根据考核点类型获取考核点信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<V_PROJECTPOINT> GetCheckProjectSetByType(string sType)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                return bll.GetCheckProjectSetByType(sType);
            }
        }
        /// <summary>
        /// 添加考核项目信息
        /// </summary>
        /// <param name="entity">考核项目实体</param>
        [OperationContract]
        public void CheckProjectSetAdd(T_HR_CHECKPROJECTSET entity)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                bll.Add(entity);
            }
        }
        /// <summary>
        /// 更新考核项目信息
        /// </summary>
        /// <param name="entity">考核项目实体</param>
        [OperationContract]
        public void CheckProjectSetUpdate(T_HR_CHECKPROJECTSET entity)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                bll.CheckProjectSetUpdate(entity);
            }
        }
        /// <summary>
        /// 删除考核项目信息
        /// </summary>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns></returns>
        [OperationContract]
        public bool CheckProjectSetDelete(string projectSetID)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                int rslt = bll.CheckProjectSetDelete(projectSetID);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据考核项目ID查询考核项目信息
        /// </summary>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_CHECKPROJECTSET GetCheckProjectSetByID(string projectSetID)
        {
            using (CheckProjectSetBLL bll = new CheckProjectSetBLL())
            {
                return bll.GetCheckProjectSetByID(projectSetID);
            }
        }
        #endregion

        #region T_HR_CHECKPOINTSET 考核项目点
        /// <summary>
        /// 根据考核项目ID获取考核点信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_CHECKPOINTSET> GetCheckPointSetByPrimaryID(string projectSetID, string Type)
        {
            using (CheckPointSetBLL bll = new CheckPointSetBLL())
            {
                return bll.GetCheckPointSetByPrimaryID(projectSetID, Type);
            }
        }
        /// <summary>
        /// 根据考核点ID获取考核点信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public T_HR_CHECKPOINTSET GetCheckPointSetByID(string pointSetID)
        {
            using (CheckPointSetBLL bll = new CheckPointSetBLL())
            {
                return bll.GetCheckPointSetByID(pointSetID);
            }
        }
        /// <summary>
        /// 添加考核点和考核等级
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="levelEntitys"></param>
        [OperationContract]
        public void CheckPointSetAdd(T_HR_CHECKPOINTSET entity, List<T_HR_CHECKPOINTLEVELSET> levelEntitys)
        {
            using (CheckPointSetBLL pointBll = new CheckPointSetBLL())
            {
                pointBll.CheckPointSetAdd(entity);
                CheckPointLevelSetBLL levelBll = new CheckPointLevelSetBLL();
                levelBll.CheckPointLevelSetAdd(levelEntitys);
            }
        }
        /// <summary>
        /// 修改考核点和考核等级
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="levelEntitys"></param>
        [OperationContract]
        public void CheckPointSetUpdate(T_HR_CHECKPOINTSET entity, List<T_HR_CHECKPOINTLEVELSET> levelEntitys)
        {
            using (CheckPointSetBLL pointBll = new CheckPointSetBLL())
            {
                pointBll.CheckPointSetUpdate(entity);
                CheckPointLevelSetBLL levelBll = new CheckPointLevelSetBLL();
                levelBll.CheckPointLevelSetUpdate(levelEntitys);
            }
        }
        /// <summary>
        /// 删除考核点和考核等级
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>是否删除成功</returns>
        [OperationContract]
        public bool CheckPointSetDelete(string projectSetID)
        {
            using (CheckPointSetBLL bll = new CheckPointSetBLL())
            {
                int rslt = bll.CheckPointSetDelete(projectSetID);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据员工类型，考核项目ID获取考核点可用分数
        /// </summary>
        /// <param name="type">员工类型</param>
        /// <param name="projectSetID">考核项目ID</param>
        /// <returns>返回可用的考核点分数</returns>
        [OperationContract]
        public int GetCheckPointAvailable(string employeeTyee, string projectSetID)
        {
            using (CheckPointSetBLL bll = new CheckPointSetBLL())
            {
                return bll.GetCheckPointAvailable(employeeTyee, projectSetID);
            }
        }
        /// <summary>
        /// 根据员工类型获取总分
        /// </summary>
        /// <param name="employeeType">员工类型</param>
        /// <returns>员工类型的总分</returns>
        [OperationContract]
        public int GetCheckPointByEmployeeTypeSum(string employeeType)
        {
            using (CheckPointSetBLL bll = new CheckPointSetBLL())
            {
                return bll.GetCheckPointByEmployeeTypeSum(employeeType);
            }
        }
        #endregion

        #region T_HR_CHECKPOINTLEVELSET 考核等级
        /// <summary>
        /// 根据考核项目ID获取考核点信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_CHECKPOINTLEVELSET> GetCheckPointLevelSetByPrimaryID(string pointID)
        {
            using (CheckPointLevelSetBLL bll = new CheckPointLevelSetBLL())
            {
                return bll.GetCheckPointLevelSetByPrimaryID(pointID);
            }
        }
        #endregion

        #region 员工岗位
        /// <summary>
        /// 根据员工信息ID找到对应的岗位信息
        /// </summary>
        /// <param name="employeeID">员工信息ID</param>
        /// <returns>返回员工岗位实体</returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeeID(string employeeID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByEmployeeID(employeeID);
            }
        }
        /// <summary>
        /// 获取员工有效非代理岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOST GetEmployeePostActivedByEmployeeID(string employeeID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostActivedByEmployeeID(employeeID);
            }
        }

        [OperationContract]
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeeIDAndPostID(string employeeID, string postID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByEmployeeIDAndPostID(employeeID, postID);
            }
        }
        /// <summary>
        /// 新增岗位
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public string EmployeePostAdd(T_HR_EMPLOYEEPOST entity)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.EmployeePostAdd(entity);
            }
        }
        /// <summary>
        /// 根据ID获取员工岗位
        /// </summary>
        /// <param name="employeePost"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOST GetEmployeePostByID(string employeePost)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByID(employeePost);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeePost"></param>
        /// <returns></returns>
        [OperationContract]
        public string EmployeePostSalaryLevelUpdate(T_HR_EMPLOYEEPOST employeePost)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.EmployeePostSalaryLevelUpdate(employeePost);
            }
        }

        /// <summary>
        /// 员工的薪资等级修改
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        [OperationContract]
        public string EmployeeSamePostSalaryLevelUpdate(string employeeid, decimal salaryevel)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.EmployeePostSalaryLevelUpdate(employeeid, salaryevel);
            }
        }

        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [OperationContract]
        public string EmployeePostUpdate(T_HR_EMPLOYEEPOST entity)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.EmployeePostUpdate(entity);
            }
        }
        /// <summary>
        /// 更新员工所有岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public string updateAllpostByemployeeID(string employeeID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.updateAllpostByemployeeID(employeeID);
            }
        }

        /// <summary>
        /// 根据员工信息ID找到所有对应的岗位信息
        /// </summary>
        /// <param name="employeeID">员工信息ID</param>
        /// <returns>员工所有岗位实体</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetAllPostByEmployeeID(string employeeID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetAllPostByEmployeeID(employeeID);
            }
        }
        /// <summary>
        /// 根据ID获取员工岗位
        /// </summary>
        /// <param name="employeePostID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEPOST GetEmployeePostByEmployeePostID(string employeePostID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByEmployeePostID(employeePostID);
            }
        }

        /// <summary>
        /// 根据岗位ID找到所有对应的岗位信息
        /// </summary>
        /// <param name="postID">岗位ID</param>
        /// <returns>员工所有岗位实体</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetEmployeePostByPostID(string postID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByPostID(postID);
            }
        }

        /// <summary>
        /// 根据岗位ID找到所有对应的岗位信息(带权限控制)
        /// </summary>
        /// <param name="postID">岗位id</param>
        /// <param name="userID">员工id</param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeePostByPostIDView(string postID, string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeePostByPostIDView(postID, userID);
            }
        }

        /// <summary>
        /// 根据岗位ID找到所有对应的岗位信息
        /// </summary>
        /// <param name="postID">岗位ID</param>
        /// <returns>员工所有岗位实体</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetEmployeePostByPostIDs(string[] postIDs)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostByPostIDs(postIDs);
            }
        }
        /// <summary>
        /// 获取指定岗位下的所有员工
        /// </summary>
        /// <param name="level">员工级别</param>
        /// <returns>所有员工</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeesByPostLevel(string level)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeesByPostLevel(level);
            }
        }
        /// <summary>
        /// 获取指定岗位区间的所有员工
        /// </summary>
        /// <param name="level">员工级别</param>
        /// <returns>所有员工</returns>
        [OperationContract]
        public List<V_EMPOYEEPOSTLEVEL> GetEmployeesByPostLevelInterval(int[] levelA, int[] levelB)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeesByPostLevel(levelA, levelB);
            }
        }
        [OperationContract]
        public List<V_EMPLOYEEPOST> GetEmployeeDetailByParas(string[] companyIDs, string[] departmentIDs, string[] postIDs, string[] employeeIDs)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeDetailByParas(companyIDs, departmentIDs, postIDs, employeeIDs);
            }
        }
        /// <summary>
        /// 获取员工的岗位级别等简要信息
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL GetEmployeePostBriefByEmployeeID(string employeeid)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostBriefByEmployeeID(employeeid);
            }
        }
        /// <summary>
        /// 获取员工的岗位级别等简要信息,包括离职岗位
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL GetAllEmployeePostBriefByEmployeeID(string employeeid)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetAllEmployeePostBriefByEmployeeID(employeeid);
            }
        }

        /// <summary>
        /// 通过岗位ID获取刚岗位下员工的信息
        /// 只是取审核通过且有效的岗位ID
        /// 员工信息取生效的员工信息
        /// </summary>
        /// <param name="StrPostID">岗位ID</param>
        /// <returns>员工信息集合</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEE> GetEmployeePostBriefByPostID(string StrPostID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostBriefByPostID(StrPostID);
            }
        }
        /// <summary>
        ///根据员工ID集合获取的岗位级别等简要信息
        /// </summary>
        /// <param name="employeeids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEDETAIL> GetEmployeesPostBriefByEmployeeID(List<string> employeeids)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetEmployeePostBriefByEmployeeID(employeeids);
            }
        }
        /// <summary>
        /// 获取员工的有效岗位
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEPOST> GetPostsActivedByEmployeeID(string employeeID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetPostsActivedByEmployeeID(employeeID);
            }
        }

        /// <summary>
        ///     add by 罗捷
        ///     根据员工ID和岗位ID判断此岗位是否为代理岗位（兼职岗位）
        /// </summary>
        /// <param name="employeeId">员工ID</param>
        /// <param name="postId">岗位ID</param>
        /// <returns>返回 1为代理岗位，0不是代理岗位，-1为出错</returns>
        [OperationContract]
        public int GetIsAgencyByEmployeeIdAndPostId(string employeeId, string postId)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                return bll.GetIsAgencyByEmployeeIdAndPostId(employeeId, postId);
            }
        }

        #endregion

        #region 劳动合同

        /// <summary>
        /// 员工劳动合同根据条件进行分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strCheckState"></param>
        /// <param name="ownerID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECONTRACT> EmployeeContractsPaging(int pageIndex, int pageSize, string sort, string filterString,
            IList<object> paras, ref int pageCount, string sType, string sValue, string strCheckState, string ownerID)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                IQueryable<T_HR_EMPLOYEECONTRACT> q = bll.EmployeeContractsPaging(pageIndex, pageSize, sort, filterString, paras,
                    ref pageCount, sType, sValue, strCheckState, ownerID);

                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEECONTRACT> EmployeeContractPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userID)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                IQueryable<T_HR_EMPLOYEECONTRACT> q = bll.EmployeeContractPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, strCheckState, userID);

                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }
        /// <summary>
        /// 添加劳动合同
        /// </summary>
        /// <param name="entity">劳动合同实体</param>
        [OperationContract]
        //public void EmployeeContractAdd(T_HR_EMPLOYEECONTRACT entity, UploadFileModel uploadFile)
        //{
        //    string strPath;
        //    EmployeeContractBLL bll = new EmployeeContractBLL();
        //    SaveFile(uploadFile, out strPath);
        //    strPath = HttpContext.Current.Server.MapPath(strPath);
        //    entity.ATTACHMENTPATH = strPath;
        //    bll.EmployeeContractAdd(entity);
        //}
        public void EmployeeContractAdd(T_HR_EMPLOYEECONTRACT entity, ref string strMsg)
        {

            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                bll.EmployeeContractAdd(entity, ref  strMsg);
            }
        }
        /// <summary>
        /// 修改劳动合同记录
        /// </summary>
        /// <param name="entity">劳动合同实体</param>
        [OperationContract]
        //public void EmployeeContractUpdate(T_HR_EMPLOYEECONTRACT entity, UploadFileModel uploadFile)
        //{
        //    string strPath;
        //    EmployeeContractBLL bll = new EmployeeContractBLL();
        //    SaveFile(uploadFile, out strPath);
        //    strPath = HttpContext.Current.Server.MapPath(strPath);
        //    entity.ATTACHMENTPATH = strPath;
        //    bll.EmployeeContractUpdate(entity);
        //}
        public void EmployeeContractUpdate(T_HR_EMPLOYEECONTRACT entity)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                bll.EmployeeContractUpdate(entity);
            }
        }
        /// <summary>
        /// 可删除劳动合同组
        /// </summary>
        /// <param name="employeeCheckIDs">劳动合同ID组</param>
        /// <returns></returns>
        [OperationContract]
        public bool EmployeeContractDelete(string[] employeeContractIDs)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                int rslt = bll.EmployeeContractDelete(employeeContractIDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据ID查询劳动合同信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEECONTRACT GetEmployeeContractByID(string strID)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                return bll.GetEmployeeContractByID(strID);
            }
        }
        /// <summary>
        /// 根据员工ID获取劳动合同信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEECONTRACT GetEmployeeContractByEmployeeID(string employeeID)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                return bll.GetEmployeeContractByEmployeeID(employeeID);
            }
        }
        /// <summary>
        /// 合同到期提醒xml
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void GetEmployeeContractEngineXml(T_HR_EMPLOYEECONTRACT entity)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                bll.GetEmployeeContractEngineXml(entity);
            }
        }
        /// <summary>
        /// 指定提醒日期
        /// </summary>
        /// <param name="entity"></param>
        [OperationContract]
        public void EmployeeContractAlarm(T_HR_EMPLOYEECONTRACT entity)
        {
            using (EmployeeContractBLL bll = new EmployeeContractBLL())
            {
                bll.EmployeeContractAlarm(entity);
            }
        }
        #endregion

        #region T_HR_ASSESSMENTFORMMASTER 人事考核
        /// <summary>
        /// 添加考核
        /// </summary>
        /// <param name="entity">人事考核主表</param>
        /// <param name="tmpList">人事考核明细表</param>
        [OperationContract]
        public void AssessmentFormMasterAdd(T_HR_ASSESSMENTFORMMASTER entity, List<T_HR_ASSESSMENTFORMDETAIL> tmpList)
        {
            using (AssessmentFormMasterBLL bll = new AssessmentFormMasterBLL())
            {
                bll.AssessmentFormMasterAdd(entity, tmpList);
            }
        }
        /// <summary>
        /// 修改考核评分
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tmpList"></param>
        [OperationContract]
        public void AssessmentFormMasterUpdate(T_HR_ASSESSMENTFORMMASTER entity, List<T_HR_ASSESSMENTFORMDETAIL> tmpList)
        {
            using (AssessmentFormMasterBLL bll = new AssessmentFormMasterBLL())
            {
                bll.AssessmentFormMasterUpdate(entity, tmpList);
            }
        }
        /// <summary>
        /// 根据对象ID查找考核信息
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ASSESSMENTFORMMASTER GetAssessMentFormMasterByObjectID(string objectID)
        {
            using (AssessmentFormMasterBLL bll = new AssessmentFormMasterBLL())
            {
                return bll.GetAssessMentFormMasterByObjectID(objectID);
            }
        }
        /// <summary>
        /// 根据ID查找考核信息
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_ASSESSMENTFORMMASTER GetAssessMentFormMasterByID(string strID)
        {
            using (AssessmentFormMasterBLL bll = new AssessmentFormMasterBLL())
            {
                return bll.GetAssessMentFormMasterByID(strID);
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的考核信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_ASSESSMENTFORMMASTER> AssessmentFormMasterPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            using (AssessmentFormMasterBLL bll = new AssessmentFormMasterBLL())
            {
                IQueryable<T_HR_ASSESSMENTFORMMASTER> q = bll.AssessmentFormMasterPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }
        #endregion

        #region T_HR_ASSESSMENTFORMDETAIL 人事考核明细
        /// <summary>
        /// 根据考核表ID获取考核明细
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<T_HR_ASSESSMENTFORMDETAIL> GetAssessmentFormDetailByMasterID(string masterID)
        {
            using (AssessmentFormDetailBLL bll = new AssessmentFormDetailBLL())
            {
                return bll.GetAssessmentFormDetailByMasterID(masterID);
            }
        }
        #endregion

        #region T_HR_IMPORTSETMASTER 导入设置主表
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_IMPORTSETMASTER> ImportSetMasterPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (ImportSetMasterBLL bll = new ImportSetMasterBLL())
            {
                IQueryable<T_HR_IMPORTSETMASTER> q = bll.ImportSetMasterPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        [OperationContract]
        public void ImportSetMasterAdd(T_HR_IMPORTSETMASTER entity, List<T_HR_IMPORTSETDETAIL> entList, ref string strMsg)
        {
            using (ImportSetMasterBLL bll = new ImportSetMasterBLL())
            {
                bll.ImportSetMasterAdd(entity, entList, ref strMsg);
            }
        }

        [OperationContract]
        public void ImportSetMasterUpdate(T_HR_IMPORTSETMASTER entity, List<T_HR_IMPORTSETDETAIL> entList, ref string strMsg)
        {
            using (ImportSetMasterBLL bll = new ImportSetMasterBLL())
            {
                bll.ImportSetMasterUpdate(entity, entList, ref strMsg);
            }
        }

        [OperationContract]
        public bool ImportSetMasterDelete(string[] ids)
        {
            using (ImportSetMasterBLL bll = new ImportSetMasterBLL())
            {
                int rslt = bll.ImportSetMasterDelete(ids);
                return (rslt > 0);
            }
        }

        [OperationContract]
        public T_HR_IMPORTSETMASTER GetImportSetMasterByID(string masterID)
        {
            using (ImportSetMasterBLL bll = new ImportSetMasterBLL())
            {
                return bll.GetImportSetMasterByID(masterID);
            }
        }
        #endregion

        #region T_HR_IMPORTSETDETAIL 导入设置明细表
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_IMPORTSETDETAIL> ImportSetDetailPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (ImportSetDetailBLL bll = new ImportSetDetailBLL())
            {
                IQueryable<T_HR_IMPORTSETDETAIL> q = bll.ImportSetDetailPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
                return q.Count() > 0 ? q.ToList() : null;
            }
        }

        [OperationContract]
        public List<T_HR_IMPORTSETDETAIL> GetImportSetDetailByMasterID(string masterID)
        {
            using (ImportSetDetailBLL bll = new ImportSetDetailBLL())
            {
                return bll.GetImportSetDetailByMasterID(masterID);
            }
        }
        #endregion
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        #region T_HR_LEFTOFFICECONFIRM 离职确认
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_LEFTOFFICECONFIRM> LeftOfficeConfirmPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            using (LeftOfficeConfirmBLL bll = new LeftOfficeConfirmBLL())
            {
                IQueryable<T_HR_LEFTOFFICECONFIRM> q = bll.LeftOfficeConfirmPaging(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID, CheckState);

                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }
        /// <summary>
        /// 添加离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        [OperationContract]
        public void LeftOfficeConfirmAdd(T_HR_LEFTOFFICECONFIRM entity)
        {
            using (LeftOfficeConfirmBLL bll = new LeftOfficeConfirmBLL())
            {
                bll.LeftOfficeConfirmAdd(entity);
            }
        }
        /// <summary>
        /// 更新离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        [OperationContract]
        public void LeftOfficeConfirmUpdate(T_HR_LEFTOFFICECONFIRM entity)
        {
            using (LeftOfficeConfirmBLL bll = new LeftOfficeConfirmBLL())
            {
                bll.LeftOfficeConfirmUpdate(entity);
            }
        }
        /// <summary>
        /// 删除离职申请记录
        /// </summary>
        /// <param name="dimissionIDs"></param>
        /// <returns>是否成功删除</returns>
        [OperationContract]
        public bool LeftOfficeConfirmDelete(string[] IDs)
        {
            using (LeftOfficeConfirmBLL bll = new LeftOfficeConfirmBLL())
            {
                int rslt = bll.LeftOfficeConfirmDelete(IDs);
                return (rslt > 0);
            }
        }
        /// <summary>
        /// 根据离职确认ID获取信息
        /// </summary>
        /// <param name="dimissionID">离职确认ID</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_LEFTOFFICECONFIRM GetLeftOfficeConfirmByID(string id)
        {
            using (LeftOfficeConfirmBLL bll = new LeftOfficeConfirmBLL())
            {
                return bll.GetLeftOfficeConfirmByID(id);
            }
        }
        #endregion

        #region  手机端通讯录接口
        /// <summary>
        /// 获取权限内的所有员工
        /// </summary>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="paras">条件的参数</param>
        /// <param name="pageCount">页面数量</param>
        /// <param name="userID">登录人ID，权限过滤所需</param>
        /// <param name="isGetPhoto">是否需要照片</param>
        /// <returns>返回符合条件的员工列表</returns>
        [OperationContract]
        public List<V_MOBILEEMPLOYEE> GetEmployeeListMobile(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool isGetPhoto)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeListMobile(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, isGetPhoto).ToList();
            }
        }
        /// <summary>
        /// 获取员工详细信息
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="employeePostID">员工岗位ID</param>
        /// <returns>返回员工详细信息</returns>
        [OperationContract]
        public V_MOBILEEMPLOYEE GetEmployeeSingleMobile(string employeeID, string employeePostID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeSingleMobile(employeeID, employeePostID);
            }
        }
        #endregion

        #region  预算所需的员工岗位情况的接口
        /// <summary>
        /// 个人费用报销离职入职提示接口
        /// </summary>
        /// <param name="list">传入集合参数</param>
        /// <returns>返回处理后的集合</returns>
        [OperationContract]
        public List<V_EMPLOYEEPOSTFORFB> GetEmployeeListForFB(List<V_EMPLOYEEPOSTFORFB> list)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeListForFB(list).ToList();
            }
        }
        /// <summary>
        /// 获取员工个人活动经费(根据公司生成)
        /// </summary>
        /// <param name="strCompantID">公司ID</param>
        /// <returns>返回员工个人活动经费</returns>
        [OperationContract]
        public List<V_EMPLOYEEFUNDS> GetEmployeeFunds(string strCompantID,string OwnerId)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeFunds(strCompantID, OwnerId);
            }
        }
        /// <summary>
        /// 获取员工个人活动经费(根据所选员工)
        /// </summary>
        /// <param name="list">自定义员工信息集合</param>
        /// <returns>返回自定义员工信息集合</returns>
        [OperationContract]
        public List<V_EMPLOYEEFUNDS> GetEmployeeFundsList(List<V_EMPLOYEEFUNDS> list)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeFundsList(list);
            }
        }
        /// <summary>
        /// 检查该用户所属公司是否存在下级公司
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns>返回该用户所属公司是否存在下级公司</returns>
        [OperationContract]
        public bool CheckChildCompanyByUserID(string userID)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.CheckChildCompanyByUserID(userID);
            }
        }
        #endregion
        
        #region 流程调用信息
        /// <summary>
        /// 根据员工ID集合获取用户信息
        /// </summary>
        /// <param name="employeeids"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_FlowUserInfo> GetFlowUserInfoPostBriefByEmployeeID(List<string> employeeids)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                Tracer.Debug("进入GetFlowUserInfoPostBriefByEmployeeID："+employeeids.Count().ToString());
                return bll.GetFlowUserInfoPostBriefByEmployeeID(employeeids);
            }
            
        }

        /// <summary>
        /// 通过部门ID:(查找部门负责人,包括人员所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="departmentID">部门ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_FlowUserInfo> GetDepartmentHeadByDepartmentID(string departmentID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                Tracer.Debug("进入GetDepartmentHeadByDepartmentID：" + departmentID);
                return bll.GetDepartmentHeadByDepartmentID(departmentID);
            }
        }

        /// <summary>
        ///通过岗位ID: (查找[直接上级]，[隔级上级]，包括所在的公司、部门、岗位、角色)
        /// </summary>
        /// <param name="postID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_FlowUserInfo> GetSuperiorByPostID(string postID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                Tracer.Debug("进入GetSuperiorByPostID：" + postID);
                return bll.GetSuperiorByPostID(postID);
            }
        }

        /// <summary>
        /// 通过用户ID:（查找所在的公司、部门、岗位、角色，一个人可能同时在多个公司任职）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_FlowUserInfo> GetFlowUserByUserID(string userID)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                Tracer.Debug("进入GetFlowUserByUserID：" + userID);
                return bll.GetFlowUserByUserID(userID);
            }
        }

        /// <summary>
        /// 通过用户ID,模块代码:（查询是否使用代理人，包括所在的公司、部门、岗位、角色）
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="modelCode">模块代码</param>
        /// <returns></returns>
        [OperationContract]
        public V_FlowUserInfo GetAgentUser(string userID, string modelCode)
        {
            using (EmployeePostBLL bll = new EmployeePostBLL())
            {
                Tracer.Debug("进入GetFlowUserByUserID：" + userID +"模块代码：" + modelCode);
                return bll.GetAgentUser(userID, modelCode);
            }
        }
        #endregion


        #region 员工报表模块
        /// <summary>
        /// 员工基本信息报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EmployeeBasicInfo> GetEmployeeBasicInfosByCompany(List<string> CompanyIDs, string sort, string filterString, object[] paras, string userID, string IsType, DateTime Start, DateTime End)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                IQueryable<V_EmployeeBasicInfo> q = bll.GetEmployeeBasicInfosByCompany(CompanyIDs, sort, filterString, paras, userID, IsType,Start,End);
                return q != null ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 导出员工信息报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeBasicInfosByCompanyReports(List<string> companyids, string sort, string filterString, object[] paras, string userID,string IsType,DateTime start,DateTime end)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportGetEmployeeBasicInfosByCompany(companyids, sort, filterString, paras, userID, IsType, start,end);
            }
        }
        /// <summary>
        /// 已取出数据时员工报表导出
        /// </summary>
        /// <param name="ListInfos"></param>
        /// <param name="Dt"></param>
        /// <param name="companyids"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeBasicInfosNoGetReports(List<V_EmployeeBasicInfo> ListInfos,DateTime Dt,List<string> companyids)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportGetEmployeeBasicInfosByCompany(ListInfos,Dt,companyids);
            }
        }

        
        #endregion

        #region 员工异动报表
        /// <summary>
        /// 获取员工异动信息
        /// </summary>
        /// <param name="companyids"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <param name="DtStart"></param>
        /// <param name="DtEnd"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EmployeeChangeInfos> EmployeePostReportInfos(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                IQueryable<V_EmployeeChangeInfos> q = bll.EmployeePostReportInfos(companyids, sort, filterString, paras, userID, DtStart,DtEnd);
                return q != null  ? q.ToList() : null;
            }
        }
        /// <summary>
        /// 导出员工异动报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeePostReportInfos(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeePostReportInfos( companyids,  sort,  filterString,  paras, userID,DtStart,DtEnd);
            }
        }
        /// <summary>
        /// 根据数据源直接导出员工异动报表
        /// </summary>
        /// <param name="companyids"></param>
        /// <param name="ListInfos"></param>
        /// <param name="Dt"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeePostChangeNoqueryReport(List<string> companyids,List<V_EmployeeChangeInfos> ListInfos,DateTime Dt)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeePostChangeNoQueryReport(companyids,ListInfos,Dt);
            }
        }
        #endregion
        /// <summary>
        /// 根据员工姓名获取员工信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetEmployeeByNames(List<string> employeeCNames)
        {
            using (EmployeeBLL bll = new EmployeeBLL())
            {
                return bll.GetEmployeeByNames(employeeCNames);
            }
        }
        #region 员工离职报表
        /// <summary>
        /// 员工离职报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="IsType"></param>
        /// <param name="IsValue"></param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EmployeeLeftOfficeInfos> GetEmployeeLeftOfficeConfirmReports(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, string IsType,DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                IQueryable<V_EmployeeLeftOfficeInfos> q = bll.GetEmployeeLeftOfficeConfirmReports(companyids,sort, filterString, paras, userID, IsType, DtStart,DtEnd);

                if (q != null)
                {
                    return q.Count() > 0 ? q.ToList() : null;
                }
                else
                {
                    return null;
                }
            }
            // return q.Count() > 0 ? q.ToList() : null;
        }

        /// <summary>
        /// 导出员工离职信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="IsType"></param>
        /// <param name="IsValue"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeLeftOfficeConfirmReports(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, string IsType, DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeeLeftOfficeConfirmReports(companyids, sort, filterString, paras,  userID,IsType,DtStart,DtEnd);
            }
        }
        /// <summary>
        /// 根据离职记录直接导出报表
        /// </summary>
        /// <param name="companyids"></param>
        /// <param name="ListInfos"></param>
        /// <param name="DtStart"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeLeftOfficeConfirmNoQueryReports(List<string> companyids, List<V_EmployeeLeftOfficeInfos> ListInfos, DateTime DtStart)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeeLeftOfficeConfirmReports(companyids,ListInfos,DtStart);
            }
        }
        /// <summary>
        /// 导出员工统计报表
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <param name="CompanyID"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        [OperationContract]        
        public byte[] ExportEmployeeCollectsReports(string sort, string filterString, object[] paras,string userID, string CompanyID,DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeesCollectReports( sort, filterString, paras,  userID, CompanyID,DtStart,DtEnd);
            }
        }

        /// <summary>
        /// 导出员工结构统计报表
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="userID"></param>
        /// <param name="CompanyID"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        [OperationContract]
        public byte[] ExportEmployeeTructReports(List<string> companyids,string sort, string filterString, object[] paras, string userID, string CompanyID, DateTime DtStart,DateTime DtEnd)
        {
            using (ReportsBLL bll = new ReportsBLL())
            {
                return bll.ExportEmployeesTructReports(companyids,sort, filterString, paras, userID, CompanyID, DtStart,DtEnd);
            }
        }



        
        #endregion

    }
}
