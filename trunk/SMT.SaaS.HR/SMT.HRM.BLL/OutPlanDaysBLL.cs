
/*
 * 文件名：OutPlanDaysBLL.cs
 * 作  用：T_HR_OUTPLANDAYS 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-7-5 20:26:58
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

using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    public class OutPlanDaysBLL : BaseBll<T_HR_OUTPLANDAYS>
    {
        public OutPlanDaysBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取列外日期信息
        /// </summary>
        /// <param name="strOutPlanDaysId">主键索引</param>
        /// <returns></returns>
        public T_HR_OUTPLANDAYS GetOutPlanDaysByID(string strOutPlanDaysId)
        {
            if (string.IsNullOrEmpty(strOutPlanDaysId))
            {
                return null;
            }

            OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strOutPlanDaysId))
            {
                strfilter.Append(" OUTPLANDAYID == @0");
                objArgs.Add(strOutPlanDaysId);
            }

            T_HR_OUTPLANDAYS entRd = dalOutPlanDays.GetOutPlanDaysRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据员工ID获取考勤方案信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetOutPlanDaysRdListByEmployeeID(string employeeID)
        {
            EmployeeBLL bll = new EmployeeBLL();
            V_EMPLOYEEPOST entity = bll.GetEmployeeDetailByID(employeeID);
            if (entity == null)
            {
                return null;
            }

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

            var ent = dal.GetObjects().Include("T_HR_VACATIONSET").Where(s => employeeID.Contains(s.T_HR_VACATIONSET.ASSIGNEDOBJECTID));
            if (ent.Count() == 0)
            {
                ent = dal.GetObjects().Include("T_HR_VACATIONSET").Where(s => s.T_HR_VACATIONSET.ASSIGNEDOBJECTID == strPostID);
                if (ent.Count() == 0)
                {
                    ent = dal.GetObjects().Include("T_HR_VACATIONSET").Where(s => s.T_HR_VACATIONSET.ASSIGNEDOBJECTID == strDepartmentID);
                    if (ent.Count() == 0)
                    {
                        ent = dal.GetObjects().Include("T_HR_VACATIONSET").Where(s => s.T_HR_VACATIONSET.ASSIGNEDOBJECTID == strCompanyID);
                    }
                }
            }            
            return ent;
        }

        /// <summary>
        /// 根据公司ID，查询一段时间内的公休假
        /// </summary>
        /// <param name="T_HR_ATTENDANCESOLUTION"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetOutPlanDaysRdListByCompanyIdAndDate(T_HR_ATTENDANCESOLUTION entAttSol, DateTime dtStart, DateTime dtEnd, string strDayType)
        {
            if (entAttSol == null)
            {
                return null;
            }

            return GetOutPlanDaysByCompanyIDAndDate(entAttSol.OWNERCOMPANYID, dtStart, dtEnd, strDayType);
        }

        /// <summary>
        /// 根据公司ID，查询一段时间内的公休假/工作日
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strDayType"></param>
        /// <returns></returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetOutPlanDaysByCompanyIDAndDate(string strCompanyID, DateTime dtStart, DateTime dtEnd, string strDayType)
        {
            var ents = from o in dal.GetObjects().Include("T_HR_VACATIONSET")
                       where o.T_HR_VACATIONSET.OWNERCOMPANYID == strCompanyID && o.STARTDATE >= dtStart && o.ENDDATE <= dtEnd && o.DAYTYPE == strDayType
                       select o;

            return ents;
        }

        /// <summary>
        /// 获取公共假期设置信息
        /// </summary>
        /// <param name="strAsignObjectType">分配对象类型</param>
        /// <param name="strAsignObjectID">分配对象ID</param>
        /// <param name="strCountyType">国家/地区</param>
        /// <param name="strDaysType">列外日期类别(公共假期/工作日)</param>
        /// <returns></returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetOutPlanDaysRdListByAsignObjectAndCountyType(string strAsignObjectType, string strAsignObjectID, string strCountyType, string strDaysType)
        {
            if (string.IsNullOrEmpty(strCountyType))
            {
                return null;
            }

            OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = " STARTDATE ";
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strAsignObjectType.Trim()))
            {
                strfilter.Append(" T_HR_VACATIONSET.ASSIGNEDOBJECTTYPE == @0");
                objArgs.Add(strAsignObjectType);
            }

            if (!string.IsNullOrEmpty(strAsignObjectID))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" T_HR_VACATIONSET.ASSIGNEDOBJECTID == @" + iIndex.ToString());
                objArgs.Add(strAsignObjectID);
            }

            if (!string.IsNullOrEmpty(strCountyType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" T_HR_VACATIONSET.COUNTYTYPE == @" + iIndex.ToString());
                objArgs.Add(strCountyType);
            }

            if (!string.IsNullOrEmpty(strDaysType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" DAYTYPE == @" + iIndex.ToString());
                objArgs.Add(strDaysType);
            }

            var q = dalOutPlanDays.GetOutPlanDaysRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取列外日期信息
        /// </summary>
        /// <param name="strOwnerID"></param>
        /// <param name="strVacationId"></param>
        /// <param name="strCountyType"></param>
        /// <param name="strDayType"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetAllOutPlanDaysRdListByMultSearch(string strOwnerID, string strVacationId, string strCountyType, string strDayType, string strSortKey)
        {
            OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strVacationId))
            {
                strfilter.Append(" T_HR_VACATIONSET.VACATIONID == @0");
                objArgs.Add(strVacationId);
            }

            if (!string.IsNullOrEmpty(strCountyType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" T_HR_VACATIONSET.COUNTYTYPE == @" + iIndex.ToString());
                objArgs.Add(strCountyType);
            }

            if (!string.IsNullOrEmpty(strDayType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count() - 1;
                }

                strfilter.Append(" DAYTYPE == @" + iIndex.ToString());
                objArgs.Add(strDayType);
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilterWithPrefix(ref filterString, ref objArgs, strOwnerID, "T_HR_VACATIONSET", "T_HR_VACATIONSET.");

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " STARTDATE ";
            }

            var q = dalOutPlanDays.GetOutPlanDaysRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 在查询条件中字段增加所属表名(例：CREATEUSERID => T_HR_VACATIONSET.CREATEUSERID)
        /// </summary>
        /// <param name="filterString">查询条件</param>
        /// <param name="objArgs">查询参数</param>
        /// <param name="strOwnerID">浏览用的员工ID</param>
        /// <param name="entityName">实体名</param>
        /// <param name="entityNamePerfix">字段所属表名</param>
        private void SetOrganizationFilterWithPrefix(ref string filterString, ref List<object> objArgs, string strOwnerID, string entityName, string entityNamePerfix)
        {
            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_VACATIONSET");

            if (string.IsNullOrWhiteSpace(filterString))
            {
                return;
            }

            if (filterString.Contains("CREATEUSERID"))
            {
                filterString = filterString.Replace("CREATEUSERID", entityNamePerfix + "CREATEUSERID");
            }

            if (filterString.Contains("OWNERCOMPANYID"))
            {
                filterString = filterString.Replace("OWNERCOMPANYID", entityNamePerfix + "OWNERCOMPANYID");
            }

            if (filterString.Contains("OWNERDEPARTMENTID"))
            {
                filterString = filterString.Replace("OWNERDEPARTMENTID", entityNamePerfix + "OWNERDEPARTMENTID");
            }

            if (filterString.Contains("OWNERPOSTID"))
            {
                filterString = filterString.Replace("OWNERPOSTID", entityNamePerfix + "OWNERPOSTID");
            }

            if (filterString.Contains("OWNERID"))
            {
                filterString = filterString.Replace("OWNERID", entityNamePerfix + "OWNERID");
            }

            if (filterString.Contains(entityNamePerfix + entityNamePerfix))
            {
                filterString = filterString.Replace(entityNamePerfix + entityNamePerfix, entityNamePerfix);
            }
        }

        /// <summary>
        /// 根据条件，获取列外日期信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">浏览用户的员工ID索引</param>
        /// <param name="strVacationId">关联的T_HR_VACATIONSET(工作日历)主键索引</param>
        /// <param name="strCountyType">国家/地区</param>
        /// <param name="strDayType">列外日期的类型</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>列外日期信息</returns>
        public IQueryable<T_HR_OUTPLANDAYS> GetOutPlanDaysRdListByMultSearch(string strOwnerID, string strVacationId, string strCountyType, string strDayType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllOutPlanDaysRdListByMultSearch(strOwnerID, strVacationId, strCountyType, strDayType, strSortKey);

            return Utility.Pager<T_HR_OUTPLANDAYS>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增列外日期信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddOutPlanDays(T_HR_OUTPLANDAYS entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_VACATIONSET entVacSet = entTemp.T_HR_VACATIONSET;
                if (entVacSet == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OUTPLANDAYID == @0");

                objArgs.Add(entTemp.OUTPLANDAYID);

                OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
                flag = dalOutPlanDays.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                T_HR_OUTPLANDAYS ent = new T_HR_OUTPLANDAYS();
                Utility.CloneEntity<T_HR_OUTPLANDAYS>(entTemp, ent);
                ent.T_HR_VACATIONSETReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_VACATIONSET", "VACATIONID", entTemp.T_HR_VACATIONSET.VACATIONID);

                Utility.RefreshEntity(ent);
                
                dalOutPlanDays.Add(ent);
                        
                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改列外日期信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyOutPlanDays(T_HR_OUTPLANDAYS entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_VACATIONSET entVacSet = entTemp.T_HR_VACATIONSET;
                if (entVacSet == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OUTPLANDAYID == @0");

                objArgs.Add(entTemp.OUTPLANDAYID);

                OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
                flag = dalOutPlanDays.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_OUTPLANDAYS entUpdate = dalOutPlanDays.GetOutPlanDaysRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalOutPlanDays.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除列外日期信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteOutPlanDays(string strOutPlanDaysId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strOutPlanDaysId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" OUTPLANDAYID == @0");

                objArgs.Add(strOutPlanDaysId);

                OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
                flag = dalOutPlanDays.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_OUTPLANDAYS entDel = dalOutPlanDays.GetOutPlanDaysRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                dalOutPlanDays.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据工作日历主键索引，删除与工作日历关联的列外日期记录
        /// </summary>
        /// <param name="strVacationId">工作日历主键索引</param>
        /// <returns></returns>
        public string DeleteByVacationID(string strVacationId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strVacationId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" T_HR_VACATIONSET.VACATIONID == @0");

                objArgs.Add(strVacationId);

                OutPlanDaysDAL dalOutPlanDays = new OutPlanDaysDAL();
                flag = dalOutPlanDays.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                string strOrderBy = " OUTPLANDAYID ";
                var q = dalOutPlanDays.GetOutPlanDaysRdListByMultSearch(strOrderBy, strFilter.ToString(), objArgs.ToArray());

                if (q == null)
                {
                    return strMsg;
                }

                if (q.Count() == 0)
                {
                    return strMsg;
                }

                foreach (T_HR_OUTPLANDAYS item in q)
                {
                    dalOutPlanDays.Delete(item);
                }

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }
        #endregion

    }
}