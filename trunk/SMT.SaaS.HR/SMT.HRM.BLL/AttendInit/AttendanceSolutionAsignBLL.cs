/*
 * 文件名：AttendanceSolutionAsignBLL.cs
 * 作  用：考勤方案应用 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-5 11:16:15
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;
using System.Threading;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 考勤方案分配业务逻辑类
    /// </summary>
    public partial class AttendanceSolutionAsignBLL : BaseBll<T_HR_ATTENDANCESOLUTIONASIGN>, IOperate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AttendanceSolutionAsignBLL()
        {
        }

        #region 获取数据

        /// <summary>
        /// 根据方案分配的主键ID,及其审核状态，检查是否存在符合的记录
        /// </summary>
        /// <param name="strAttendanceSolutionId"></param>
        /// <param name="strCheckStates"></param>
        /// <returns></returns>
        public int CheckAttSolIsExistsAsignRd(string strAttendanceSolutionId, string strCheckStates)
        {
            int i = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(strAttendanceSolutionId) || string.IsNullOrWhiteSpace(strCheckStates))
                {
                    i = -1;
                    return i;
                }

                var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                           where n.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttendanceSolutionId && n.CHECKSTATE == strCheckStates
                           select n;

                i = ents.Count();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
            return i;
        }

        /// <summary>
        /// 获取考勤方案应用信息
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByID(string strAttendanceSolutionAsignId)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionAsignId))
            {
                return null;
            }

            AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendanceSolutionAsignId))
            {
                strfilter.Append(" ATTENDANCESOLUTIONASIGNID == @0");
                objArgs.Add(strAttendanceSolutionAsignId);
            }

            T_HR_ATTENDANCESOLUTIONASIGN entRd = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取考勤方案应用信息
        /// </summary>
        /// <param name="strOwnerID">登录人的员工ID</param>
        /// <param name="strCheckState">浏览的审核状态</param>
        /// <param name="strAttendanceSolutionName">考勤方案定义的名称</param>
        /// <param name="strAssignedObjectType">考勤方案应用类型</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回考勤方案应用信息</returns>
        public IQueryable<V_ATTENDANCESOLUTIONASIGN> GetAllAttendanceSolutionAsignRdListByMultSearch(string strOwnerID,
            string strCheckState, string strAttendanceSolutionName, string strAssignedObjectType, string strSortKey, DateTime dtStart, DateTime dtEnd)
        {
            AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strAttendanceSolutionName))
            {
                strfilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == @0");
                objArgs.Add(strAttendanceSolutionName);
            }

            if (!string.IsNullOrEmpty(strAssignedObjectType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" ASSIGNEDOBJECTTYPE == @" + iIndex.ToString());
                objArgs.Add(strAssignedObjectType);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ATTENDANCESOLUTIONASIGNID ";
            }

            string filterString = strfilter.ToString();

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDANCESOLUTIONASIGN");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("ATTENDANCESOLUTIONASIGNID", "T_HR_ATTENDANCESOLUTIONASIGN", strOwnerID, ref strCheckState,
                    ref filterString, ref objArgs);
                if (string.Compare(strCheckfilter, filterString) == 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                objArgs.Add(strCheckState);
            }

            var q = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdListByMultSearch(strOrderBy, filterString,
                objArgs.ToArray());
            if (dtStart != DateTime.MinValue)
            {
                q = q.Where(t => t.STARTDATE >= dtStart);
            }
            if (dtEnd != DateTime.MinValue)
            {
                q = q.Where(t => t.STARTDATE <= dtEnd);
            }
            q = q.Where(t => t.CREATEDEPARTMENTID == null).OrderByDescending(t => t.CREATEDATE);
            List<V_ATTENDANCESOLUTIONASIGN> signList = new List<V_ATTENDANCESOLUTIONASIGN>();
            foreach (var item in q.ToList())
            {
                V_ATTENDANCESOLUTIONASIGN model = new V_ATTENDANCESOLUTIONASIGN();
                model.ATTENDANCESOLUTIONASIGNID = item.ATTENDANCESOLUTIONASIGNID;
                model.ATTENDANCESOLUTIONNAME = item.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME;
                model.ASSIGNEDOBJECTTYPE = item.ASSIGNEDOBJECTTYPE;
                //if(item.ASSIGNEDOBJECTTYPE == 
                //var dictList = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "ASSIGNEDOBJECTTYPE"});
                if (item.ASSIGNEDOBJECTTYPE == "1") //公司
                {
                    var companyObj = dalAttendanceSolutionAsign.GetObjects<T_HR_COMPANY>().Where(t => t.COMPANYID == item.ASSIGNEDOBJECTID).FirstOrDefault();
                    if (companyObj != null)
                    {
                        model.ASSIGNEDOBJECTID = companyObj.CNAME;
                    }
                }
                if (item.ASSIGNEDOBJECTTYPE == "2") //部门
                {
                    if (string.IsNullOrEmpty(item.CREATECOMPANYID))
                    {
                        var departmentObj = dalAttendanceSolutionAsign.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY").Where(t => t.DEPARTMENTID == item.ASSIGNEDOBJECTID).FirstOrDefault();
                        if (departmentObj != null)
                        {
                            model.ASSIGNEDOBJECTID = departmentObj.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        }
                    }
                    else
                    {
                        string deptNames = null;
                        var sameDeptsList = dalAttendanceSolutionAsign.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Where(t => t.CREATECOMPANYID == item.CREATECOMPANYID).ToList();
                        foreach (var dept in sameDeptsList)
                        {
                            var departmentObj = dalAttendanceSolutionAsign.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY").Where(t => t.DEPARTMENTID == dept.ASSIGNEDOBJECTID).FirstOrDefault();
                            if (departmentObj != null)
                            {
                                deptNames += departmentObj.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + ",";
                            }
                        }
                        model.ASSIGNEDOBJECTID = deptNames.Trim(',');
                    }
                }
                if (item.ASSIGNEDOBJECTTYPE == "3") //岗位
                {
                    var postObj = dalAttendanceSolutionAsign.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY").Where(t => t.POSTID == item.ASSIGNEDOBJECTID).FirstOrDefault();
                    if (postObj != null)
                    {
                        model.ASSIGNEDOBJECTID = postObj.T_HR_POSTDICTIONARY.POSTNAME;
                    }
                }
                if (item.ASSIGNEDOBJECTTYPE == "4") //员工
                {
                    string[] empIds = item.ASSIGNEDOBJECTID.Split(',');
                    string empNames = null;
                    foreach (string empId in empIds)
                    {
                        if (!string.IsNullOrEmpty(empId))
                        {
                            var empObject = dalAttendanceSolutionAsign.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEEID == empId).FirstOrDefault();
                            if (empObject != null)
                            {
                                empNames += empObject.EMPLOYEECNAME + ",";
                            }
                        }
                        model.ASSIGNEDOBJECTID = empNames.Trim(',');
                    }
                }

                //model.ASSIGNEDOBJECTID = item.ASSIGNEDOBJECTID;
                model.STARTDATE = item.STARTDATE.Value;
                model.ENDDATE = item.ENDDATE.Value;
                model.CHECKSTATE = item.CHECKSTATE;
                signList.Add(model);
            }
            return signList.AsQueryable();
        }

        /// <summary>
        /// 考勤方案选择部门，根据createcompanyid找出1张单保存多个部门ID
        /// </summary>
        /// <param name="createCompanyId"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignRdListByCreateCompanyId(string createCompanyId)
        {
            AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
            var q = dalAttendanceSolutionAsign.GetObjects().Where(t => t.CREATECOMPANYID == createCompanyId);
            return q;
        }
       
       /// <summary>
        /// 根据条件，获取考勤方案应用信息,并进行分页
        /// </summary>
        /// <param name="strAttendanceSolutionName">考勤方案名</param>
        /// <param name="strAssignedObjectType">分配对象类型</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>返回考勤方案应用信息</returns>
        public IQueryable<V_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignRdListByMultSearch(string strOwnerID, string strCheckState, string strAttendanceSolutionName, string strAssignedObjectType,
            string strSortKey, DateTime dtStart,DateTime dtEnd, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendanceSolutionAsignRdListByMultSearch(strOwnerID, strCheckState, strAttendanceSolutionName, strAssignedObjectType, strSortKey, dtStart, dtEnd);

            if (q == null)
            {
                return null;
            }

            return Utility.Pager<V_ATTENDANCESOLUTIONASIGN>(q, pageIndex, pageSize, ref pageCount);
        }

        /// <summary>
        /// 根据员工ID获取考勤方案信息(解决一个员工在系统内多个公司入职的情况)
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByEmployeeID(string strCompanyID, string strEmployeeID)
        {
            T_HR_ATTENDANCESOLUTIONASIGN entRes = null;
            if (string.IsNullOrEmpty(strCompanyID) || string.IsNullOrEmpty(strEmployeeID))
            {
                return entRes;
            }

            DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                       where n.OWNERCOMPANYID == strCompanyID && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                       orderby n.ASSIGNEDOBJECTTYPE ascending
                       select n;

            EmployeeBLL bll = new EmployeeBLL();
            V_EMPLOYEEPOST entity = bll.GetEmployeeDetailByID(strEmployeeID);
            string strIsAgenPost = Convert.ToInt32(Common.IsAgencyPost.No).ToString();//主岗位                
            string strPostID = string.Empty, strDepId = string.Empty;
            if (entity == null)
            {
                return entRes;
            }

            strPostID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.POSTID;
            strDepId = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;

            foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
            {
                if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    if (item.ASSIGNEDOBJECTID.Contains(strEmployeeID))
                    {
                        entRes = item;
                        break;
                    }
                }
                else
                {
                    if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strPostID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strDepId)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strCompanyID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                }
            }

            return entRes;
        }

        /// <summary>
        /// 根据员工ID获取考勤方案信息(解决一个员工在系统内多个公司入职的情况)
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByEmployeeID(string strCompanyID, string strDepId, string strPostID, string strEmployeeID)
        {
            T_HR_ATTENDANCESOLUTIONASIGN entRes = null;
            if (string.IsNullOrEmpty(strCompanyID) || string.IsNullOrEmpty(strDepId) || string.IsNullOrEmpty(strPostID) || string.IsNullOrEmpty(strEmployeeID))
            {
                return entRes;
            }

            DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                       where n.OWNERCOMPANYID == strCompanyID && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                       orderby n.ASSIGNEDOBJECTTYPE ascending
                       select n;

            foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
            {
                if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    if (item.ASSIGNEDOBJECTID.Contains(strEmployeeID))
                    {
                        entRes = item;
                        break;
                    }
                }
                else
                {
                    if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strPostID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strDepId)
                        {
                            entRes = item;
                            break;
                        }
                    }
                    else if (item.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                    {
                        if (item.ASSIGNEDOBJECTID == strCompanyID)
                        {
                            entRes = item;
                            break;
                        }
                    }
                }
            }

            return entRes;
        }


        ///// <summary>
        ///// 2012/12/19
        ///// 根据传入的id获取考勤方案信息
        ///// </summary>
        ///// <param name="objectID">主键id</param>
        ///// <param name="objectType">（1.公司，2.部门，3.岗位，4.员工）</param>
        ///// <returns>考勤方案</returns>
        //public List<T_HR_ATTENDANCESOLUTIONASIGN> GetAttendanceSolutionAsignByObjectID(string objectID, int objectType)
        //{
        //    List<T_HR_ATTENDANCESOLUTIONASIGN> listAtt = new List<T_HR_ATTENDANCESOLUTIONASIGN>();
        //    try
        //    {
        //        string approvedState = Convert.ToInt32(CheckStates.Approved).ToString();
        //        switch (objectType)
        //        {
        //            //公司
        //            case 1:
        //                {
        //                    var ent = from ats in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
        //                              where ats.ASSIGNEDOBJECTID == objectID && ats.CHECKSTATE == approvedState
        //                              select ats;
        //                    listAtt = ent.Count() > 0 ? ent.ToList() : null;
        //                }; break;

        //            //部门
        //            case 2:
        //                {
        //                    DepartmentBLL bll = new DepartmentBLL();
        //                    string strCompanyID = string.Empty;
        //                    T_HR_DEPARTMENT entity = bll.GetDepartmentById(objectID);
        //                    if (entity != null)
        //                    {
        //                        strCompanyID = entity.T_HR_COMPANY.COMPANYID;
        //                    }
        //                    var ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == objectID && s.CHECKSTATE == approvedState);
        //                    if (ent == null || ent.Count() <= 0)
        //                    {
        //                        ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strCompanyID && s.CHECKSTATE == approvedState);
        //                    }
        //                    listAtt = ent.Count() > 0 ? ent.ToList() : null;
        //                }; break;

        //            //岗位
        //            case 3:
        //                {
        //                    PostBLL bll = new PostBLL();
        //                    string strDepartmentID = string.Empty, strCompanyID = string.Empty;
        //                    T_HR_POST entity = bll.GetPostById(objectID);
        //                    if (entity != null)
        //                    {
        //                        strDepartmentID = entity.T_HR_DEPARTMENT.DEPARTMENTID;
        //                        strCompanyID = entity.COMPANYID;
        //                    }
        //                    var ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == objectID && s.CHECKSTATE == approvedState);
        //                    if (ent == null || ent.Count() <= 0)
        //                    {
        //                        ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strDepartmentID && s.CHECKSTATE == approvedState);
        //                        if (ent == null || ent.Count() <= 0)
        //                        {
        //                            ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strCompanyID && s.CHECKSTATE == approvedState);
        //                        }
        //                    }
        //                    listAtt = ent.Count() > 0 ? ent.ToList() : null;
                           
        //                }; break;

        //            //员工
        //            default:
        //                {
        //                    listAtt.AddRange(ListGetAttendanceSolutionAsignByEmployeeID(objectID).ToList());
        //                    listAtt = listAtt.Where(s => s.CHECKSTATE == approvedState).ToList();
        //                }; break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug("获取考勤方案失败 " + ex.Message+ DateTime.Now);
        //    }
        //    return listAtt;
        //}

        /// <summary>
        /// 根据员工ID获取所有所分配的所有考勤方案信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public List<T_HR_ATTENDANCESOLUTIONASIGN> ListGetAttendanceSolutionAsignByEmployeeID(string employeeID)
        {
            EmployeeBLL bll = new EmployeeBLL();
            V_EMPLOYEEPOST entity = bll.GetEmployeeDetailByID(employeeID);
            if (entity != null)
            {
                if (entity.T_HR_EMPLOYEE == null)
                {
                    return null;
                }

                string strPostID = string.Empty, strDepartmentID = string.Empty, strCompanyID = string.Empty;
                string strIsAgenPost = Convert.ToInt32(Common.IsAgencyPost.No).ToString();//主岗位                

                if (entity.T_HR_EMPLOYEE.EMPLOYEESTATE == Convert.ToInt32(Common.EmployeeState.Dimission).ToString())
                {
                    strPostID = entity.T_HR_EMPLOYEE.OWNERPOSTID;
                    strDepartmentID = entity.T_HR_EMPLOYEE.OWNERDEPARTMENTID;
                    strCompanyID = entity.T_HR_EMPLOYEE.OWNERCOMPANYID;
                }
                else
                {
                    if (entity.EMPLOYEEPOSTS == null)
                    {
                        return null;
                    }

                    if (entity.EMPLOYEEPOSTS.Count() == 0)
                    {
                        return null;
                    }

                    strPostID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.POSTID;
                    strDepartmentID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    strCompanyID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                }

                var ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID.Contains(employeeID));
                if (ent == null || ent.Count()<=0)
                {
                    ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strPostID);
                    if (ent == null || ent.Count() <= 0)
                    {
                        ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strDepartmentID);
                        if (ent == null || ent.Count() <= 0)
                        {
                            ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Where(s => s.ASSIGNEDOBJECTID == strCompanyID);
                        }
                    }
                }
                return ent.Count() > 0 ? ent.ToList() : null;
            }
            return null;
        }

        /// <summary>
        /// 根据员工ID获取考勤方案信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByEmployeeID(string employeeID)
        {
            EmployeeBLL bll = new EmployeeBLL();
            V_EMPLOYEEPOST entity = bll.GetEmployeeDetailByID(employeeID);
            if (entity != null)
            {
                if (entity.T_HR_EMPLOYEE == null)
                {
                    return null;
                }

                string strPostID = string.Empty, strDepartmentID = string.Empty, strCompanyID = string.Empty;
                string strIsAgenPost = Convert.ToInt32(Common.IsAgencyPost.No).ToString();//主岗位                

                if (entity.T_HR_EMPLOYEE.EMPLOYEESTATE == Convert.ToInt32(Common.EmployeeState.Dimission).ToString())
                {
                    strPostID = entity.T_HR_EMPLOYEE.OWNERPOSTID;
                    strDepartmentID = entity.T_HR_EMPLOYEE.OWNERDEPARTMENTID;
                    strCompanyID = entity.T_HR_EMPLOYEE.OWNERCOMPANYID;
                }
                else
                {
                    if (entity.EMPLOYEEPOSTS == null)
                    {
                        return null;
                    }

                    if (entity.EMPLOYEEPOSTS.Count() == 0)
                    {
                        return null;
                    }

                    strPostID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.POSTID;
                    strDepartmentID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    strCompanyID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                }

                var ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").FirstOrDefault(s => s.ASSIGNEDOBJECTID.Contains(employeeID));
                if (ent == null)
                {
                    ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").FirstOrDefault(s => s.ASSIGNEDOBJECTID == strPostID);
                    if (ent == null)
                    {
                        ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").FirstOrDefault(s => s.ASSIGNEDOBJECTID == strDepartmentID);
                        if (ent == null)
                        {
                            ent = dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").FirstOrDefault(s => s.ASSIGNEDOBJECTID == strCompanyID);
                        }
                    }
                }
                return ent;
            }
            return null;
        }

        /// <summary>
        /// 根据时间段获取
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttendanceSolutionAsignByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart)
        {
            EmployeeBLL bll = new EmployeeBLL();
            V_EMPLOYEEPOST entity = bll.GetEmployeeDetailByID(strEmployeeID);
            if (entity != null)
            {
                if (entity.T_HR_EMPLOYEE == null)
                {
                    Tracer.Debug("获取员工的考勤方案方法查询员工详情时返回员工为空：员工id" + strEmployeeID);
                    return null;
                }

                string strPostID = string.Empty, strDepartmentID = string.Empty, strCompanyID = string.Empty;
                string strIsAgenPost = Convert.ToInt32(Common.IsAgencyPost.No).ToString();//主岗位                

                if (entity.T_HR_EMPLOYEE.EMPLOYEESTATE == Convert.ToInt32(Common.EmployeeState.Dimission).ToString())
                {
                    strPostID = entity.T_HR_EMPLOYEE.OWNERPOSTID;
                    strDepartmentID = entity.T_HR_EMPLOYEE.OWNERDEPARTMENTID;
                    strCompanyID = entity.T_HR_EMPLOYEE.OWNERCOMPANYID;
                }
                else
                {
                    if (entity.EMPLOYEEPOSTS == null)
                    {
                        Tracer.Debug("获取员工的考勤方案方法查询员工详情时返回员工岗位为空：员工id" + strEmployeeID);
                        return null;
                    }

                    if (entity.EMPLOYEEPOSTS.Count() == 0)
                    {
                        Tracer.Debug("获取员工的考勤方案方法查询员工详情时返回员工无生效岗位：员工id" + strEmployeeID);
                        return null;
                    }

                    strPostID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.POSTID;
                    strDepartmentID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                    strCompanyID = entity.EMPLOYEEPOSTS.FirstOrDefault(ep => ep.ISAGENCY == strIsAgenPost).T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                }

                string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                var ent = from en in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                          where en.OWNERCOMPANYID == strCompanyID
                          && strEmployeeID.Contains(en.ASSIGNEDOBJECTID)
                          && en.STARTDATE <= dtStart &&
                          en.ENDDATE >= dtStart
                          && en.CHECKSTATE == strCheckStates
                          select en; 
                if (ent.FirstOrDefault() == null)
                {
                    ent = from en in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                          where en.OWNERCOMPANYID == strCompanyID
                          && strPostID.Contains(en.ASSIGNEDOBJECTID)//linq反写，表示en.ASSIGNEDOBJECTID 包含strPostID
                          && en.STARTDATE <= dtStart &&
                          en.ENDDATE >= dtStart
                          && en.CHECKSTATE == strCheckStates
                          select en;
                    if (ent.FirstOrDefault() == null)
                    {
                        ent = from en in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                              where en.OWNERCOMPANYID == strCompanyID
                              && strDepartmentID.Contains(en.ASSIGNEDOBJECTID)
                              && en.STARTDATE <= dtStart &&
                              en.ENDDATE >= dtStart
                              && en.CHECKSTATE == strCheckStates
                              select en;

                        if (ent.FirstOrDefault() == null)
                        {
                            ent = from en in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                                  where en.OWNERCOMPANYID == strCompanyID
                                  && strCompanyID.Contains(en.ASSIGNEDOBJECTID)
                                  && en.STARTDATE <= dtStart &&
                                  en.ENDDATE >= dtStart
                                  && en.CHECKSTATE == strCheckStates
                                  select en;
                        }
                    }
                }

                T_HR_ATTENDANCESOLUTIONASIGN entityAsing = ent.FirstOrDefault();
                if (entityAsing == null)
                {                   
                    Tracer.Debug("检测到员工没有分配考勤方案" + entity.T_HR_EMPLOYEE.EMPLOYEECNAME);
                    return null;
                }
                if (Utility.IsNoNeedCardEmployee(strEmployeeID, dtStart))
                {
                    entityAsing.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE = (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString();
                    Tracer.Debug("检测到员工设置为免打卡员工" + entity.T_HR_EMPLOYEE.EMPLOYEECNAME + "：修改考勤方案状态为（2为免打卡）：" + entityAsing.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE);
                }
                return entityAsing;
            }
            return null;
        }


        /// <summary>
        /// 根据员工ID，起止日期，获取考勤方案分配记录
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">起始日期</param>
        /// <param name="dtEnd">截止日期</param>
        /// <returns>考勤方案分配记录</returns>
        public T_HR_ATTENDANCESOLUTIONASIGN GetAttSolAsignByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            var ents = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(s => s.EMPLOYEEID == strEmployeeID);

            if (ents == null)
            {
                return null;
            }

            string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            DateTime dtCheckDate = new DateTime();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
            List<T_HR_EMPLOYEEPOST> entList = new List<T_HR_EMPLOYEEPOST>(); ;

            var temps = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                        where ep.T_HR_EMPLOYEE.EMPLOYEEID == ents.EMPLOYEEID && ep.ISAGENCY == strIsAgnecy && ep.CHECKSTATE == strCheckState && ep.CREATEDATE >= dtCheckDate
                        orderby ep.CREATEDATE descending
                        select ep;

            if (temps == null)
            {
                return null;
            }

            if (temps.Count() == 0)
            {
                return null;
            }

            entList = temps.ToList();

            T_HR_EMPLOYEEPOST temp = entList.FirstOrDefault();

            string strPostID = string.Empty, strDepartmentID = string.Empty, strCompanyID = string.Empty;
            string strIsAgenPost = Convert.ToInt32(Common.IsAgencyPost.No).ToString();//主岗位                

            if (temp.T_HR_EMPLOYEE.EMPLOYEESTATE == Convert.ToInt32(Common.EmployeeState.Dimission).ToString())
            {
                strPostID = temp.T_HR_EMPLOYEE.OWNERPOSTID;
                strDepartmentID = temp.T_HR_EMPLOYEE.OWNERDEPARTMENTID;
                strCompanyID = temp.T_HR_EMPLOYEE.OWNERCOMPANYID;
            }
            else
            {
                strPostID = temp.T_HR_POST.POSTID;
                strDepartmentID = temp.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                strCompanyID = temp.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            }

            string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            var entas = from s in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                        where s.OWNERCOMPANYID == strCompanyID && strEmployeeID.Contains(s.ASSIGNEDOBJECTID) && s.STARTDATE <= dtStart && s.ENDDATE > dtStart && s.CHECKSTATE == strCheckStates
                        orderby s.CREATEDATE descending
                        select s;

            var entae = entas.ToList().FirstOrDefault();

            if (entae == null)
            {
                entas = from s in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                        where s.OWNERCOMPANYID == strCompanyID && s.ASSIGNEDOBJECTID == strPostID && s.STARTDATE <= dtStart && s.ENDDATE > dtStart && s.CHECKSTATE == strCheckStates
                        orderby s.CREATEDATE descending
                        select s;
                entae = entas.ToList().FirstOrDefault();
                if (entae == null)
                {
                    entas = from s in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                            where s.OWNERCOMPANYID == strCompanyID && s.ASSIGNEDOBJECTID == strDepartmentID && s.STARTDATE <= dtStart && s.ENDDATE > dtStart && s.CHECKSTATE == strCheckStates
                            orderby s.CREATEDATE descending
                            select s;
                    entae = entas.ToList().FirstOrDefault();
                    if (entae == null)
                    {
                        entas = from s in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                                where s.OWNERCOMPANYID == strCompanyID && s.ASSIGNEDOBJECTID == strCompanyID && s.STARTDATE <= dtStart && s.ENDDATE > dtStart && s.CHECKSTATE == strCheckStates
                                orderby s.CREATEDATE descending
                                select s;
                        entae = entas.ToList().FirstOrDefault();
                    }
                }
            }
            return entae;
        }


        /// <summary>
        /// 获取当前月份所有已应用考勤方案的公司ID集合
        /// </summary>
        /// <param name="strCurYearMonth"></param>
        /// <returns></returns>
        public List<string> GetAllCompanyIDByAttendSolAsign(string strCurYearMonth)
        {
            List<string> strResList = new List<string>();
            bool bIsParse = false;
            DateTime dtCur = new DateTime();
            bIsParse = DateTime.TryParse(strCurYearMonth + "-1", out dtCur);
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            if (!bIsParse)
            {
                return strResList;
            }

            string strOrgType = (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString();

            var entas = from s in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                        where s.ASSIGNEDOBJECTTYPE == strOrgType && s.STARTDATE <= dtCur && s.ENDDATE > dtCur && s.CHECKSTATE == strCheckState
                        orderby s.CREATEDATE descending
                        select s;

            foreach (var ent in entas)
            {
                if (string.IsNullOrWhiteSpace(ent.ASSIGNEDOBJECTID))
                {
                    continue;
                }

                if (strResList.Contains(ent.ASSIGNEDOBJECTID))
                {
                    continue;
                }

                strResList.Add(ent.ASSIGNEDOBJECTID);
            }

            return strResList;
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤方案应用信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        /// <summary>
        public string AddAttSolAsign(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = CheckAsign(entTemp.ASSIGNEDOBJECTTYPE, entTemp.ASSIGNEDOBJECTID, entTemp.STARTDATE);//检查是否需要下月生效
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    return strMsg;


                }
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == @0");
                strFilter.Append(" AND ASSIGNEDOBJECTID == @1");

                objArgs.Add(entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                objArgs.Add(entTemp.ASSIGNEDOBJECTID);

                AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
                // flag = dalAttendanceSolutionAsign.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                //if (flag)
                //{
                //    return "{ALREADYEXISTSRECORD}";
                //}

                flag = dalAttendanceSolutionAsign.IsExistSame(strFilter.ToString(), objArgs.ToArray());
                bool flagDateTime = false;

                if (flag)
                {
                    List<string> strAssignedobjectID = dalAttendanceSolutionAsign.GetExistsID(strFilter.ToString(), objArgs.ToArray());
                    flagDateTime = dalAttendanceSolutionAsign.IsExistsDateTime(entTemp.STARTDATE.ToString(), entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID, strAssignedobjectID);
                }

                if (flagDateTime)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                //如果分配对象类型是部门
                if (entTemp.ASSIGNEDOBJECTTYPE == "2" && entTemp.ASSIGNEDOBJECTID.Split(',').Count() > 1)
                {
                    string gPKId = Guid.NewGuid().ToString();
                    string[] deptIds = entTemp.ASSIGNEDOBJECTID.Split(',');
                    T_HR_ATTENDANCESOLUTIONASIGN ent = new T_HR_ATTENDANCESOLUTIONASIGN();
                    entTemp.ASSIGNEDOBJECTID = deptIds[0];
                    entTemp.CREATECOMPANYID = gPKId;
                    Utility.CloneEntity<T_HR_ATTENDANCESOLUTIONASIGN>(entTemp, ent);
                    ent.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                        new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                    Utility.RefreshEntity(ent);
                    dalAttendanceSolutionAsign.Add(ent);
                    SaveMyRecord(ent);

                    for (int i = 1; i < deptIds.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(deptIds[i]))
                        {
                            ent = new T_HR_ATTENDANCESOLUTIONASIGN();
                            Utility.CloneEntity<T_HR_ATTENDANCESOLUTIONASIGN>(entTemp, ent);

                            ent.ATTENDANCESOLUTIONASIGNID = Guid.NewGuid().ToString();
                            ent.ASSIGNEDOBJECTID = deptIds[i];
                            ent.CREATEDEPARTMENTID = Guid.Empty.ToString();
                            ent.CREATECOMPANYID = gPKId;

                            ent.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                                new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                            Utility.RefreshEntity(ent);
                            dalAttendanceSolutionAsign.Add(ent);
                        }
                    }

                    strMsg = "{SAVESUCCESSED}";

                }
                else
                {
                    T_HR_ATTENDANCESOLUTIONASIGN ent = new T_HR_ATTENDANCESOLUTIONASIGN();
                    Utility.CloneEntity<T_HR_ATTENDANCESOLUTIONASIGN>(entTemp, ent);
                    ent.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                        new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                    ent.REMARK = "1";
                    Utility.RefreshEntity(ent);

                    dalAttendanceSolutionAsign.Add(ent);
                    SaveMyRecord(ent);
                    strMsg = "{SAVESUCCESSED}";
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行AddAttSolAsign函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strMsg;
        }

        /// <summary>
        ///  根据考勤方案分配的对象类型和对象ID，检查在考勤初始化记录表是否存在符合的记录
        /// </summary>
        /// <param name="strAssignobjectType">对象类型</param>
        /// <param name="strAssignobjectID">对象ID</param>
        /// <returns>检查结果（true/false）</returns>
        public string CheckAsign(string strAssignobjectType, string strAssignobjectID, DateTime? startDate)
        {
            string strMsg = string.Empty;
            try
            {
                if (Convert.ToInt32(strAssignobjectType) == Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1)
                {
                    #region 为员工时，则去考勤记录查找是否有记录，如果则为下个月生效
                    string id = strAssignobjectID.Replace(",", string.Empty).Trim();
                    var ent = from a in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                              join b in dal.GetObjects<T_HR_SHIFTDEFINE>() on a.T_HR_SHIFTDEFINE.SHIFTDEFINEID equals b.SHIFTDEFINEID
                              // where a.EMPLOYEEID == id
                              where id.Contains(a.EMPLOYEEID) && a.ATTENDANCEDATE >= startDate
                              select b.SHIFTDEFINEID;
                    if (ent.Count() > 0)
                    {
                        if (Convert.ToDateTime(startDate).Year == DateTime.Now.Year)
                        {
                            if (Convert.ToDateTime(startDate).Month <= DateTime.Now.Month)
                            {
                                strMsg = Utility.GetResourceStr("{EARLYEFFECTIVEDATE}");
                                //return strMsg;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    //其他考勤机构则要下个月才生效
                    if (Convert.ToDateTime(startDate).Year == DateTime.Now.Year)
                    {
                        if (Convert.ToDateTime(startDate).Month <= DateTime.Now.Month)
                        {
                            strMsg = Utility.GetResourceStr("{EARLYEFFECTIVEDATE}");
                            //return strMsg;
                        }
                    }
                }
                return strMsg;
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog("CheckAsign方法验证考勤方案分配出错：" + strMsg);
                return strMsg;
            }

        }

        /// <summary>
        /// 修改考勤方案应用信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyAttSolAsign(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = CheckAsign(entTemp.ASSIGNEDOBJECTTYPE, entTemp.ASSIGNEDOBJECTID, entTemp.STARTDATE);//检查是否需要下月生效
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    return strMsg;
                }
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDANCESOLUTIONASIGNID == @0");

                objArgs.Add(entTemp.ATTENDANCESOLUTIONASIGNID);

                AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
                flag = dalAttendanceSolutionAsign.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                //如果分配对象类型是部门
                if (entTemp.ASSIGNEDOBJECTTYPE == "2" && entTemp.ASSIGNEDOBJECTID.Split(',').Count() > 1)
                {
                    var list = dalAttendanceSolutionAsign.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Where(t => t.CREATECOMPANYID == entTemp.CREATECOMPANYID);
                    foreach (var item in list)
                    {
                        item.STARTDATE = entTemp.STARTDATE;
                        item.ENDDATE = entTemp.ENDDATE;
                        item.REMARK = entTemp.REMARK;
                        dalAttendanceSolutionAsign.Update(item);
                    }
                    var entSource = dalAttendanceSolutionAsign.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Where(t => t.ATTENDANCESOLUTIONASIGNID == entTemp.ATTENDANCESOLUTIONASIGNID).FirstOrDefault();
                    string s = SaveMyRecord(entSource);

                    strMsg = "{SAVESUCCESSED}";
                }
                else
                {

                    //T_HR_ATTENDANCESOLUTIONASIGN entUpdate = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                    //Utility.CloneEntity(entTemp, entUpdate);
                    //entUpdate.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                    //    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                    entTemp.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                           new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                    entTemp.T_HR_ATTENDANCESOLUTION.EntityKey =
                           new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);


                    dalAttendanceSolutionAsign.Update(entTemp);
                    SaveMyRecord(entTemp);
                    strMsg = "{SAVESUCCESSED}";
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行ModifyAttSolAsign函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除考勤方案应用信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteAttSolAsign(string strAttendanceSolutionAsignId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceSolutionAsignId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDANCESOLUTIONASIGNID == @0");

                objArgs.Add(strAttendanceSolutionAsignId);

                AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
                flag = dalAttendanceSolutionAsign.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCESOLUTIONASIGN entDel = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                if (!string.IsNullOrEmpty(entDel.CREATECOMPANYID))
                {
                    //删除分配对象是部门 多个部门的记录
                    string gEmpty = Guid.Empty.ToString();
                    var list = dalAttendanceSolutionAsign.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Where(t => t.CREATECOMPANYID == entDel.CREATECOMPANYID && t.CREATEDEPARTMENTID == gEmpty).ToList();
                    foreach (var item in list)
                    {
                        dalAttendanceSolutionAsign.Delete(item);
                    }
                }
                DeleteMyRecord(entDel);
                dalAttendanceSolutionAsign.Delete(entDel);
                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行DeleteAttSolAsign函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strMsg;
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="strAttendanceSolutionAsignId"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        public string AuditAttSolAsign(string strAttendanceSolutionAsignId, string strCheckState, string strActionType)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceSolutionAsignId) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{NOTFOUND}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDANCESOLUTIONASIGNID == @0");

                objArgs.Add(strAttendanceSolutionAsignId);

                AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
                flag = dalAttendanceSolutionAsign.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCESOLUTIONASIGN entAudit = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                //已审核通过的记录禁止再次提交审核
                //if (entAudit.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                //{
                //    return "{REPEATAUDITERROR}";
                //}

                //此处生成考勤初始化记录屏蔽，此处初始化功能已转入Windows服务中处理
                //审核状态变为审核通过时，生成对应的员工考勤记录(应用的员工范围，视应用对象而定)
                //if (strCheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                //{
                //    if (strActionType == "Auto")
                //    {
                //        GetAttendSolAsignForOutEngineXml(entAudit);
                //        GetFreeLeaveDayForOutEngineXml(entAudit);
                //        GetAttendMonthlyBalanceForOutEngineXml(entAudit);
                //    }
                //    else if (strActionType == "Manual")
                //    {
                //        AsignAttendanceSolution(entAudit);
                //    }
                //}

                entAudit.CHECKSTATE = strCheckState;

                dalAttendanceSolutionAsign.Update(entAudit);
                SaveMyRecord(entAudit);

                T_HR_ATTENDANCESOLUTIONASIGN entUdp = dalAttendanceSolutionAsign.GetAttendanceSolutionAsignRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                if (!string.IsNullOrEmpty(entUdp.CREATECOMPANYID))
                {
                    //更新分配对象是部门 多个部门的记录
                    string gEmpty = Guid.Empty.ToString();
                    var list = dalAttendanceSolutionAsign.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Where(t => t.CREATECOMPANYID == entUdp.CREATECOMPANYID && t.CREATEDEPARTMENTID == gEmpty).ToList();
                    foreach (var item in list)
                    {
                        item.CHECKSTATE = strCheckState;
                        dalAttendanceSolutionAsign.Update(item);
                    }
                }

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行AuditAttSolAsign函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strMsg;
        }
        /// <summary>
        /// 对所有公司解决方案生成考勤初始化记录
        /// </summary>
        public string AsignAttendanceSolutionWithAllCompany()
        {
            string strRes = string.Empty;
            try
            {
                DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
                string strCheckStates = Convert.ToInt32(CheckStates.Approved).ToString();
                string strAssignObjectType = (Convert.ToInt32(AssignObjectType.Company) + 1).ToString();

                var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                           where n.ASSIGNEDOBJECTTYPE == strAssignObjectType && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                List<string> strIDs = new List<string>();

                foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
                {
                    if (strIDs.Contains(item.OWNERCOMPANYID))
                    {
                        continue;
                    }

                    AsignAttendanceSolutionByComapny(item.OWNERCOMPANYID, dtCur);
                    strIDs.Add(item.OWNERCOMPANYID);
                }

                return "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strRes = "{ERROR}";
                Utility.SaveLog("执行AsignAttendanceSolutionWithAllCompany函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strRes;
        }

        /// <summary>
        /// 根据当前公司ID和日期，获取其可用的考勤方案分配记录，然后循环生成初始化记录
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <returns></returns>
        public string AsignAttendanceSolutionByComapny(string strCompanyId, DateTime dtCur)
        {
            string strRes = string.Empty;
            try
            {
                string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                if (string.IsNullOrWhiteSpace(strCompanyId))
                {
                    return "{NOTFOUND}";
                }

                var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                           where n.OWNERCOMPANYID == strCompanyId && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                           orderby n.ASSIGNEDOBJECTTYPE ascending
                           select n;

                DateTime dtAsignDate = DateTime.Parse(dtCur.ToString("yyyy-MM") + "-1");
                foreach (T_HR_ATTENDANCESOLUTIONASIGN item in ents)
                {
                    AsignAttendanceSolution(item, dtAsignDate);
                }

                return "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strRes = "{ERROR}";
                Utility.SaveLog("执行AsignAttendanceSolutionByComapny函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strRes;
        }

        /// <summary>
        /// 为单一员工生成考勤记录
        /// </summary>
        /// <param name="entEmployee"></param>
        /// <returns></returns>
        public string AsignAttendanceSolutionForSingleEmployee(T_HR_EMPLOYEE entEmployee)
        {
            return AsignAttendanceSolutionForEmployeeByDate(entEmployee, DateTime.Now);
        }

        public string AsignAttendanceSolutionForEmployeeByDate(T_HR_EMPLOYEE entEmployee, DateTime dtAsignDate)
        {
            string strMsg = string.Empty;
            try
            {
                if (entEmployee == null)
                {
                    return "{NOTFOUND}";
                }

                DateTime dtCurDate = DateTime.Now;

                T_HR_ATTENDANCESOLUTIONASIGN entAttendanceSolution = GetAttendanceSolutionAsignByEmployeeIDAndDate(entEmployee.EMPLOYEEID, dtCurDate);

                CompanyBLL bllCompany = new CompanyBLL();
                T_HR_COMPANY entCompany = bllCompany.GetCompanyByEmployeeID(entEmployee.EMPLOYEEID);

                if (entCompany == null)
                {
                    return "{NOTFOUND}";
                }

                List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();
                entEmployees.Add(entEmployee);

                strMsg = AsignAttendSolForEmployees(entAttendanceSolution, entCompany, entEmployees, dtAsignDate);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行AsignAttendanceSolutionForSingleEmployee函数发生错误，错误详细信息如下：" + ex.ToString());
            }
            return strMsg;
        }

        /// <summary>
        /// 应用考勤方案，生成员工考勤记录
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AsignAttendanceSolution(T_HR_ATTENDANCESOLUTIONASIGN entTemp, DateTime dtAsignDate)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDANCESOLUTIONASIGNID == @0");

                objArgs.Add(entTemp.ATTENDANCESOLUTIONASIGNID);

                AttendanceSolutionAsignDAL dalAttendanceSolutionAsign = new AttendanceSolutionAsignDAL();
                flag = dalAttendanceSolutionAsign.IsExistsRd(strFilter.ToString(), objArgs.ToArray());
                
                if (!flag)
                {
                    Tracer.Debug("考勤考勤方案应用不存在");
                    return "{NOTFOUND}";
                }

                if (entTemp.T_HR_ATTENDANCESOLUTION == null)
                {
                    Tracer.Debug("考勤方案不存在");
                    return "{NOTFOUND}";
                }

                string strAssignedObjectType = entTemp.ASSIGNEDOBJECTTYPE;
                string strAssignedObjectID = entTemp.ASSIGNEDOBJECTID;

                CompanyBLL bllCompany = new CompanyBLL();
                T_HR_COMPANY entCompany = new T_HR_COMPANY();

                EmployeeBLL bllEmployee = new EmployeeBLL();
                List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

                if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                {
                    entCompany = bllCompany.GetCompanyByID(strAssignedObjectID);

                    entEmployees = bllEmployee.GetEmployeeByCompanyID(strAssignedObjectID, dtAsignDate).ToList();
                }
                else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                {
                    entCompany = bllCompany.GetCompanyByDepartmentID(strAssignedObjectID);
                    entEmployees = bllEmployee.GetEmployeeByDepartmentID(strAssignedObjectID).ToList();
                }
                else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                {
                    entCompany = bllCompany.GetCompanyByPostID(strAssignedObjectID);
                    entEmployees = bllEmployee.GetEmployeeByPostID(strAssignedObjectID).ToList();
                }
                else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    string[] sArray = strAssignedObjectID.Split(',');
                    entCompany = bllCompany.GetCompanyByEmployeeID(sArray[0].ToString());
                    entEmployees = bllEmployee.GetEmployeeByIDs(sArray);
                }

                if (entCompany == null)
                {
                    Tracer.Debug("考勤初始化公司不存在，公司id：" + strAssignedObjectID);
                    return "{NOTFOUND}";
                }

                if (entEmployees == null)
                {
                    Tracer.Debug("考勤初始化员工不存在，公司id：" + strAssignedObjectID);
                    return "{NOTFOUND}";
                }

                if (entEmployees.Count == 0)
                {
                    Tracer.Debug("考勤初始化员工人数为0，公司id：" + strAssignedObjectID);
                    return "{NOTFOUND}";
                }

                AsignAttendSolForEmployees(entTemp, entCompany, entEmployees, dtAsignDate);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                Utility.SaveLog("执行AsignAttendanceSolution函数发生错误，错误详细信息如下：" + ex.ToString());
            }

            return strMsg;
        }

        /// <summary>
        /// 根据当前组织机构ID，应用其可用的考勤方案
        /// </summary>
        /// <param name="strOrgType"></param>
        /// <param name="strOrgId"></param>
        /// <returns></returns>
        public string AsignAttendanceSolutionByOrgID(string strOrgType, string strOrgId)
        {
            string strRes = string.Empty;
            try
            {
                DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                if (string.IsNullOrWhiteSpace(strOrgType) && string.IsNullOrWhiteSpace(strOrgId))
                {
                    return "{NOTFOUND}";
                }

                DateTime dtAsignDate = DateTime.Parse(dtCur.ToString("yyyy-MM") + "-1");
                if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                {
                    var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                               where n.OWNERCOMPANYID == strOrgId && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtCur
                               orderby n.ASSIGNEDOBJECTTYPE ascending
                               select n;

                    List<T_HR_ATTENDANCESOLUTIONASIGN> entList = ents.ToList();
                    var comps = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "1").OrderBy(c => c.UPDATEDATE);
                    var deps = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "2").OrderBy(c => c.UPDATEDATE);
                    var poss = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "3").OrderBy(c => c.UPDATEDATE);
                    var pers = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "4").OrderBy(c => c.UPDATEDATE);

                    foreach (T_HR_ATTENDANCESOLUTIONASIGN item in comps)
                    {
                        AsignAttendanceSolution(item, dtAsignDate);
                    }

                    foreach (T_HR_ATTENDANCESOLUTIONASIGN item in deps)
                    {
                        AsignAttendanceSolution(item, dtAsignDate);
                    }

                    foreach (T_HR_ATTENDANCESOLUTIONASIGN item in poss)
                    {
                        AsignAttendanceSolution(item, dtAsignDate);
                    }

                    foreach (T_HR_ATTENDANCESOLUTIONASIGN item in pers)
                    {
                        AsignAttendanceSolution(item, dtAsignDate);
                    }

                    return "{SAVESUCCESSED}";
                }
                else if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    EmployeeBLL bllEmployee = new EmployeeBLL();
                    T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByID(strOrgId);
                    AsignAttendanceSolutionForSingleEmployee(entEmployee);
                    return "{SAVESUCCESSED}";
                }
                else
                {
                    return "{NOTFOUND}";
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                strRes = "{ERROR}";
            }

            return strRes;
        }


        public string AsignAttendanceSolutionByOrgID(string strOrgType, string strOrgId, string strCurMonth)
        {
            string strRes = string.Empty;
            if (string.IsNullOrWhiteSpace(strOrgType) && string.IsNullOrWhiteSpace(strOrgId) && string.IsNullOrWhiteSpace(strCurMonth))
            {
                return "机构类型，机构序号，考勤月份不可为空";
            }

            bool bFormat = false;
            DateTime dtAsignDate = new DateTime();
            bFormat = DateTime.TryParse(strCurMonth + "-1", out dtAsignDate);
            if (!bFormat)
            {
                return "考勤月份转换为日期失败，请检查考勤月份(例：2010-1)";
            }
            string strMessage="开始初始化考勤:考勤机构类型：" + strOrgType + " 考勤机构id：" + strOrgId + "考勤月份：" + strCurMonth;
            Tracer.Debug(strMessage);
            try
            {
                string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                {
                    //var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION")
                    //           where n.OWNERCOMPANYID == strOrgId && n.CHECKSTATE == strCheckStates && n.ENDDATE > dtAsignDate
                    //           orderby n.ASSIGNEDOBJECTTYPE ascending
                    //           select n;

                    //List<T_HR_ATTENDANCESOLUTIONASIGN> entList = ents.ToList();
                    //var comps = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "1").OrderBy(c => c.UPDATEDATE);
                    //var deps = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "2").OrderBy(c => c.UPDATEDATE);
                    //var poss = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "3").OrderBy(c => c.UPDATEDATE);
                    //var pers = entList.Where(c => c.ASSIGNEDOBJECTTYPE == "4").OrderBy(c => c.UPDATEDATE);

                    //foreach (T_HR_ATTENDANCESOLUTIONASIGN item in comps)
                    //{
                    //    AsignAttendanceSolution(item, dtAsignDate);
                    //}

                    //foreach (T_HR_ATTENDANCESOLUTIONASIGN item in deps)
                    //{
                    //    AsignAttendanceSolution(item, dtAsignDate);
                    //}

                    //foreach (T_HR_ATTENDANCESOLUTIONASIGN item in poss)
                    //{
                    //    AsignAttendanceSolution(item, dtAsignDate);
                    //}

                    //foreach (T_HR_ATTENDANCESOLUTIONASIGN item in pers)
                    //{
                        //AsignAttendanceSolution(item, dtAsignDate);
                    //}
                    var employees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on ent.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                    join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                    where ep.ISAGENCY == "0"//主岗位
                                    && ep.EDITSTATE == "1"//生效中
                                    && ep.CHECKSTATE == strCheckStates//审核通过
                                    && p.COMPANYID == strOrgId
                                    select ent;

                    if (employees.Count() > 0)
                    {
                        foreach (var emp in employees)
                        {
                            AsignAttendanceSolutionForEmployeeByDate(emp, dtAsignDate);
                        }
                    }
                    else
                    {
                        Tracer.Debug("通过公司生成考勤初始化记录失败，获取的员工数为0，公司di：" + strOrgId);
                    }
                    string str=" 考勤初始化执行完毕";
                    Tracer.Debug(strMessage+str);
                    return str;
                }
                else if (strOrgType == (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString())
                {
                    EmployeeBLL bllEmployee = new EmployeeBLL();
                    T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByID(strOrgId);
                    AsignAttendanceSolutionForEmployeeByDate(entEmployee, dtAsignDate);
                    return "考勤初始化执行完毕";
                }
                else
                {
                    return "考勤初始化的执行范围无员工，生成失败";
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("考勤初始化错误：执行函数AsignAttendanceSolutionByOrgID失败，失败原因：" + ex.ToString());
                strRes = "考勤初始化失败";
            }
            return strRes;
        }
        #endregion

        #region 配置引擎需要的XML参数

        /// <summary>
        /// 对所有公司解决方案定时生成考勤记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public void GetAttendSolAsignForOutEngineXml()
        {

            DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
            string strStartTime = "01:00";

            List<object> objArds = new List<object>();
            objArds.Add("703dfb3c-d3dc-4b1d-9bf0-3507ba01b716");//此处默认为集团公司的ID，但是仅作为填充使用，非生成时调用所需数据
            objArds.Add("HR");
            objArds.Add("T_HR_ATTENDANCESOLUTIONASIGN");
            objArds.Add("9AB89A36-D5A0-4bc3-834B-1B7B4B295D5F");//此处默认为集团公司的考情方案分配记录ID，但是仅作为填充使用，非生成时调用所需数据
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Month");
            objArds.Add(string.Empty);
            objArds.Add("于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置考勤记录定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + "开始按月自动生成员工考勤记录");
            objArds.Add(string.Empty);
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"AsignAttendanceSolution\" Name=\"ATTENDANCESOLUTIONASIGNID\" Value=\"9AB89A36-D5A0-4bc3-834B-1B7B4B295D5F\"></Para>");
            objArds.Add("Г");
            objArds.Add("customBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 对指定解决方案定时生成员工可休假记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public void GetFreeLeaveDayForOutEngineXml()
        {

            DateTime dtStart = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-1");
            string strStartTime = "03:00";

            List<object> objArds = new List<object>();
            objArds.Add("703dfb3c-d3dc-4b1d-9bf0-3507ba01b716");//此处默认为集团公司的ID，但是仅作为填充使用，非生成时调用所需数据
            objArds.Add("HR");
            objArds.Add("T_HR_EMPLOYEELEVELDAYCOUNT");
            objArds.Add("9AB89A36-D5A0-4bc3-834B-1B7B4B295D5F");//此处默认为集团公司的考情方案分配记录ID，但是仅作为填充使用，非生成时调用所需数据
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Month");
            objArds.Add(string.Empty);
            objArds.Add("于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置考勤记录定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + "开始按天自动计算员工的可休假记录");
            objArds.Add(string.Empty);
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"CreateLevelDayCountWithAll\" Name=\"ATTENDANCESOLUTIONASIGNID\" Value=\"9AB89A36-D5A0-4bc3-834B-1B7B4B295D5F\"></Para>");
            objArds.Add("Г");
            objArds.Add("customBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 对指定解决方案定时生成考勤记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public void GetAttendSolAsignForOutEngineXml(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {

            DateTime dtStart = entTemp.STARTDATE.Value;
            string strStartTime = "01:00";

            List<object> objArds = new List<object>();
            objArds.Add(entTemp.ASSIGNEDOBJECTID);//此处是分配对象的ID(该分配对象类型可能为如下几种：公司，部门，岗位)
            objArds.Add("HR");
            objArds.Add("T_HR_ATTENDANCESOLUTIONASIGN");
            objArds.Add(entTemp.ATTENDANCESOLUTIONASIGNID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Month");
            objArds.Add(string.Empty);
            objArds.Add("于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置考勤记录定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + "开始按月自动生成员工考勤记录");
            objArds.Add(string.Empty);
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"AsignAttendanceSolution\" Name=\"ATTENDANCESOLUTIONASIGNID\" Value=\"" + entTemp.ATTENDANCESOLUTIONASIGNID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("customBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 对指定解决方案定时生成员工可休假记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        private void GetFreeLeaveDayForOutEngineXml(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {

            DateTime dtStart = entTemp.STARTDATE.Value;
            string strStartTime = "03:00";

            List<object> objArds = new List<object>();
            objArds.Add(entTemp.ASSIGNEDOBJECTID);//此处是分配对象的ID(该分配对象类型可能为如下几种：公司，部门，岗位)
            objArds.Add("HR");
            objArds.Add("T_HR_EMPLOYEELEVELDAYCOUNT");
            objArds.Add(entTemp.ATTENDANCESOLUTIONASIGNID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Day");
            objArds.Add(string.Empty);
            objArds.Add(entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME + "于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置考勤记录定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + "开始按天自动计算员工的可休假记录");
            objArds.Add(string.Empty);
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"CreateLevelDayCountByAsignAttSol\" Name=\"ATTENDANCESOLUTIONASIGNID\" Value=\"" + entTemp.ATTENDANCESOLUTIONASIGNID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("customBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 对指定解决方案应用对象定时生成考勤月度结算记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        private void GetAttendMonthlyBalanceForOutEngineXml(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {
            if (entTemp.T_HR_ATTENDANCESOLUTION == null)
            {
                return;
            }

            DateTime dtStart = new DateTime();
            string strStartTime = "01:00";
            if (entTemp.T_HR_ATTENDANCESOLUTION.ISCURRENTMONTH == (Convert.ToInt32(Common.IsChecked.No) + 1).ToString())   //判断考勤方案设定考勤月度结算是否为当月结算
            {
                dtStart = DateTime.Parse(entTemp.STARTDATE.Value.AddMonths(1).ToString("yyyy-MM") + "-" + entTemp.T_HR_ATTENDANCESOLUTION.SETTLEMENTDATE);
            }
            else
            {
                dtStart = DateTime.Parse(entTemp.STARTDATE.Value.ToString("yyyy-MM") + "-" + entTemp.T_HR_ATTENDANCESOLUTION.SETTLEMENTDATE);
            }

            List<object> objArds = new List<object>();
            objArds.Add(entTemp.ASSIGNEDOBJECTID);//此处是分配对象的ID(该分配对象类型可能为如下几种：公司，部门，岗位)
            objArds.Add("HR");
            objArds.Add("T_HR_ATTENDMONTHLYBALANCE");
            objArds.Add(entTemp.ATTENDANCESOLUTIONASIGNID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("Month");
            objArds.Add(string.Empty);
            objArds.Add(entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME + "于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "配置考勤月度结算记录定时生成,将于" + dtStart.ToString("yyyy-MM-dd") + strStartTime + "开始按月自动生成员工考勤记录");
            objArds.Add(string.Empty);
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"CalculateEmployeeAttendanceMonthly\" Name=\"ISCURRENTMONTH\" Value=\"" + entTemp.T_HR_ATTENDANCESOLUTION.ISCURRENTMONTH + "\"></Para><Para Name=\"ASSIGNEDOBJECTTYPE\" Value=\"" + entTemp.ASSIGNEDOBJECTTYPE + "\"></Para><Para Name=\"ASSIGNEDOBJECTID\" Value=\"" + entTemp.ASSIGNEDOBJECTID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("customBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        #endregion

        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="strEntityKeyName"></param>
        /// <param name="strEntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string strEntityKeyName, string strEntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strResult = AuditAttSolAsign(strEntityKeyValue, CheckState, "Manual");
                if (strResult == "{SAVESUCCESSED}")
                {
                    i = 1;
                    if (CheckState == ((int)CheckStates.Approved).ToString())
                    {   //发送到期提醒定时任务
                        T_HR_ATTENDANCESOLUTIONASIGN q = (from ent in dal.GetObjects()
                                                          where ent.ATTENDANCESOLUTIONASIGNID == strEntityKeyValue
                                                          select ent).FirstOrDefault();
                        SendEngineXml(q);
                    }
                }
                else
                {
                    i = 0;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + strEntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }

        #region 到期定时提醒
        /// <summary>
        /// 员工合同到期提醒xml
        /// </summary>
        /// <param name="entTemp"></param>
        public void SendEngineXml(T_HR_ATTENDANCESOLUTIONASIGN entTemp)
        {
            DateTime dtStart = System.DateTime.Now;
            string strStartTime = "10:00";
            int alarmDay = 7;//提前7天提醒
            if (entTemp.ENDDATE != null)
            {
                dtStart = Convert.ToDateTime(entTemp.ENDDATE).AddDays(-alarmDay);
            }
            //dtStart = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, remindDate);
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.OWNERCOMPANYID);
            objArds.Add("HR");
            objArds.Add("T_HR_ATTENDANCESOLUTIONASIGN");
            objArds.Add(entTemp.ATTENDANCESOLUTIONASIGNID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("");
            objArds.Add("");
            objArds.Add(entTemp.ASSIGNEDOBJECTTYPE+" "+entTemp.ASSIGNEDOBJECTID + " 考勤方案分配到期提醒");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"ATTENDANCESOLUTIONASIGNRemindTrigger\" Name=\"ATTENDANCESOLUTIONASIGNID\" Value=\"" + entTemp.ATTENDANCESOLUTIONASIGNID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("CustomBinding");
            SMT.Foundation.Log.Tracer.Debug("调用引擎信息，考勤方案分配ID：" + entTemp.ATTENDANCESOLUTIONASIGNID);
            SMT.Foundation.Log.Tracer.Debug("调用引擎信息，考勤方案分配类型及id：" + entTemp.ASSIGNEDOBJECTTYPE + " " + entTemp.ASSIGNEDOBJECTID);
            SMT.Foundation.Log.Tracer.Debug("开始调用引擎的默认消息");

            Utility.SendEngineEventTriggerData(objArds);
        }
        #endregion 
    }
}

