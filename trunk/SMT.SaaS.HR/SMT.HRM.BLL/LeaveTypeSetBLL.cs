/*
 * 文件名：LeaveTypeSetBLL.cs
 * 作  用：请假类型设置业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010年2月23日, 15:53:05
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
    public class LeaveTypeSetBLL : BaseBll<T_HR_LEAVETYPESET>, ILookupEntity
    {
        public LeaveTypeSetBLL()
        { }

        #region 获取数据
        /// <summary>
        /// 根据权限获取假期标准列表
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>返回假期标准列表</returns>
        public List<T_HR_LEAVETYPESET> GetLeaveTypeSetAll(string employeeID)
        {
            string strFilter = "";
            List<object> queryParas = new List<object>();
            SetOrganizationFilter(ref strFilter, ref queryParas, employeeID, "T_HR_LEAVETYPESET");
            IQueryable<T_HR_LEAVETYPESET> ents = dal.GetObjects();
            ents = ents.Where(strFilter, queryParas.ToArray());
            return ents.ToList();
        }
        /// <summary>
        /// 获取请假类型设置信息
        /// </summary>
        /// <param name="strLeaveTypeSetId">主键索引</param>
        /// <returns></returns>
        public T_HR_LEAVETYPESET GetLeaveTypeSetByID(string strLeaveTypeSetId)
        {
            if (string.IsNullOrEmpty(strLeaveTypeSetId))
            {
                return null;
            }

            LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strLeaveTypeSetId))
            {
                strfilter.Append(" LEAVETYPESETID == @0");
                objArgs.Add(strLeaveTypeSetId);
            }

            T_HR_LEAVETYPESET entLTRd = dalLeaveTypeSet.GetLeaveTypeSetRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entLTRd;
        }

        /// <summary>
        /// 根据条件，获取请假类型设置信息
        /// </summary>
        /// <param name="strAttendanceSolutionId"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_LEAVETYPESET> GetLeaveTypeSetRdListForAttendanceSolution(string strAttendanceSolutionId, string strSortKey)
        {
            var q = from af in dal.GetObjects<T_HR_ATTENDFREELEAVE>().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_LEAVETYPESET")
                    where af.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttendanceSolutionId
                    select af.T_HR_LEAVETYPESET;

            string strOrderBy = string.Empty;
            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " LEAVETYPESETID ";
            }

            q = q.OrderBy(strOrderBy);

            return q;
        }

        /// <summary>
        /// 根据条件，获取请假类型设置信息
        /// </summary>
        /// <param name="strOwnerID">登录人的员工ID</param>
        /// <param name="strLeaveTypeValue">假期类别</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        public IQueryable<T_HR_LEAVETYPESET> GetAllLeaveTypeSetRdListByMultSearch(string strOwnerID, string strLeaveTypeValue, string strSortKey)
        {
            LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strLeaveTypeValue))
            {
                strfilter.Append(" LEAVETYPEVALUE == @0 ");
                objArgs.Add(strLeaveTypeValue);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " LEAVETYPESETID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_LEAVETYPESET");

            var q = dalLeaveTypeSet.GetLeaveTypeSetRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取请假类型设置信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">登录人的员工ID</param>
        /// <param name="strLeaveTypeValue">假期类别</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>请假类型信息</returns>
        public IQueryable<T_HR_LEAVETYPESET> GetLeaveTypeSetRdListByMultSearch(string strOwnerID, string strLeaveTypeValue,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllLeaveTypeSetRdListByMultSearch(strOwnerID, strLeaveTypeValue, strSortKey);

            return Utility.Pager<T_HR_LEAVETYPESET>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增请假类型设置信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddLeaveTypeSet(T_HR_LEAVETYPESET entTemp)
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

                strFilter.Append(" LEAVETYPENAME == @0");
                strFilter.Append(" && LEAVETYPEVALUE == @1");

                objArgs.Add(entTemp.LEAVETYPENAME);
                objArgs.Add(entTemp.LEAVETYPEVALUE);

                LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();
                flag = dalLeaveTypeSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalLeaveTypeSet.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改请假类型设置信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyLeaveTypeSet(T_HR_LEAVETYPESET entTemp)
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

                strFilter.Append(" LEAVETYPESETID == @0");

                objArgs.Add(entTemp.LEAVETYPESETID);

                LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();
                flag = dalLeaveTypeSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_LEAVETYPESET entUpdate = dalLeaveTypeSet.GetLeaveTypeSetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalLeaveTypeSet.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除请假类型设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteLeaveTypeSet(string strLeaveTypeSetId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strLeaveTypeSetId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" LEAVETYPESETID == @0");

                objArgs.Add(strLeaveTypeSetId);

                LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();
                flag = dalLeaveTypeSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_LEAVETYPESET entDel = dalLeaveTypeSet.GetLeaveTypeSetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                var entAL = from s in dal.GetObjects<T_HR_ATTENDFREELEAVE>().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_LEAVETYPESET")
                            where s.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId
                            select s;

                if (entAL.Count() > 0)
                {
                    return "{EXISTRELATIONRECORD}";
                }

                var entLR = from r in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                            where r.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId
                            select r;

                if (entLR.Count() > 0)
                {
                    return "{EXISTRELATIONRECORD}";
                    //dal.Delete(entLR);
                }

                entDel.T_HR_FREELEAVEDAYSET.Load();

                dal.DeleteFromContext(entDel);
                dal.SaveContextChanges();

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion

        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            LeaveTypeSetDAL dalLeaveTypeSet = new LeaveTypeSetDAL();

            List<object> queryParas = new List<object>();
            string strOrderBy = string.Empty;

            if (paras.Count() > 0)
            {
                for (int i = 0; i < paras.Count(); i++)
                {
                    queryParas.Add(paras[i]);
                }
            }

            strOrderBy = " LEAVETYPESETID ";

            //受权限限制，假如权限不够则查询条件filterString可能为空，则要判断
            if (string.IsNullOrWhiteSpace(filterString))
            {
                filterString = string.Empty;
            }
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEAVETYPESET");

            IQueryable<T_HR_LEAVETYPESET> ents = dalLeaveTypeSet.GetLeaveTypeSetRdListByMultSearch(strOrderBy, filterString, queryParas.ToArray());

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion


    }
}
