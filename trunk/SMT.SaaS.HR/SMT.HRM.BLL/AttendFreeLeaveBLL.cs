
/*
 * 文件名：AttendFreeLeaveBLL.cs
 * 作  用：考勤方案带薪假 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-25 15:36:56
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

namespace SMT.HRM.BLL
{
    public class AttendFreeLeaveBLL : BaseBll<T_HR_ATTENDFREELEAVE>
    {
        public AttendFreeLeaveBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取考勤方案带薪假信息
        /// </summary>
        /// <param name="strAttendFreeLeaveId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDFREELEAVE GetAttendFreeLeaveByID(string strAttendFreeLeaveId)
        {
            if (string.IsNullOrEmpty(strAttendFreeLeaveId))
            {
                return null;
            }

            AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendFreeLeaveId))
            {
                strfilter.Append(" ATTENDFREELEAVEID == @0");
                objArgs.Add(strAttendFreeLeaveId);
            }

            T_HR_ATTENDFREELEAVE entRd = dalAttendFreeLeave.GetAttendFreeLeaveRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取考勤方案带薪假信息
        /// </summary>
        /// <param name="strAttendanceSolutionID"></param>
        /// <param name="strFreeLeaveDaySetID"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDFREELEAVE> GetAllAttendFreeLeaveRdListByMultSearch(string strAttendanceSolutionID, string strFreeLeaveDaySetID, string strSortKey)
        {
            AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();

            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strAttendanceSolutionID))
            {
                strfilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID = @0");
                objArgs.Add(strAttendanceSolutionID);
            }

            if (!string.IsNullOrEmpty(strFreeLeaveDaySetID))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" T_HR_ATTENDANCESOLUTION.FREELEAVEDAYSETID == @" + iIndex.ToString());
                objArgs.Add(strFreeLeaveDaySetID);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ATTENDFREELEAVEID ";
            }

            var q = dalAttendFreeLeave.GetAttendFreeLeaveRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取考勤方案带薪假信息,并进行分页
        /// </summary>
        /// <param name="strAttendFreeLeaveName">考勤方案编号</param>
        /// <param name="strFreeLeaveDaySetID">带薪假设置编号</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>考勤方案带薪假信息</returns>
        public IQueryable<T_HR_ATTENDFREELEAVE> GetAttendFreeLeaveRdListByMultSearch(string strAttendanceSolutionID, string strFreeLeaveDaySetID,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendFreeLeaveRdListByMultSearch(strAttendanceSolutionID, strFreeLeaveDaySetID, strSortKey);

            return Utility.Pager<T_HR_ATTENDFREELEAVE>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤方案带薪假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddAttendFreeLeave(T_HR_ATTENDFREELEAVE entTemp)
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

                strFilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == @0");
                strFilter.Append(" && T_HR_LEAVETYPESET.LEAVETYPESETID == @1");

                objArgs.Add(entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                objArgs.Add(entTemp.T_HR_LEAVETYPESET.LEAVETYPESETID);

                AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();
                flag = dalAttendFreeLeave.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                T_HR_ATTENDFREELEAVE ent = new T_HR_ATTENDFREELEAVE();
                Utility.CloneEntity<T_HR_ATTENDFREELEAVE>(entTemp, ent);

                ent.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_LEAVETYPESET", "LEAVETYPESETID", entTemp.T_HR_LEAVETYPESET.LEAVETYPESETID);
                
                dalAttendFreeLeave.Add(ent);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤方案带薪假信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyAttendFreeLeave(T_HR_ATTENDFREELEAVE entTemp)
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

                strFilter.Append(" ATTENDFREELEAVEID == @0");

                objArgs.Add(entTemp.ATTENDFREELEAVEID);

                AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();
                flag = dalAttendFreeLeave.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDFREELEAVE entUpdate = dalAttendFreeLeave.GetAttendFreeLeaveRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                entUpdate.T_HR_ATTENDANCESOLUTION = entTemp.T_HR_ATTENDANCESOLUTION;
                entUpdate.T_HR_LEAVETYPESET = entTemp.T_HR_LEAVETYPESET;
                entUpdate.REMARK = entTemp.REMARK;
                entUpdate.CREATEUSERID = entTemp.CREATEUSERID;
                entUpdate.CREATEDATE = entTemp.CREATEDATE;
                entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                entUpdate.UPDATEDATE = entTemp.UPDATEDATE;

                dalAttendFreeLeave.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除考勤方案带薪假信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strAttendFreeLeaveId">主键索引</param>
        /// <returns></returns>
        public string DeleteAttendFreeLeave(string strAttendFreeLeaveId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendFreeLeaveId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDFREELEAVEID == @0");

                objArgs.Add(strAttendFreeLeaveId);

                AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();
                flag = dalAttendFreeLeave.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDFREELEAVE entDel = dalAttendFreeLeave.GetAttendFreeLeaveRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendFreeLeave.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion

        /// <summary>
        /// 根据条件，获取考勤方案带薪假信息
        /// </summary>
        /// <param name="strAttendSolID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDFREELEAVE> GetAttendFreeLeaveByAttendSolID(string strAttendSolID)
        {
            if (string.IsNullOrEmpty(strAttendSolID))
            {
                return null;
            }

            AttendFreeLeaveDAL dalAttendFreeLeave = new AttendFreeLeaveDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendSolID))
            {
                strfilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == @0");
                objArgs.Add(strAttendSolID);
            }

            string strOrderBy = " ATTENDFREELEAVEID ";

            var q = dalAttendFreeLeave.GetAttendFreeLeaveRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据考勤方案ID删除考勤方案带薪假记录
        /// </summary>
        /// <param name="strAttendSolID"></param>
        public void ModifyAttendFreeLeaveByAttSolID(string strAttendSolID)
        {
            try
            {
                IQueryable<T_HR_ATTENDFREELEAVE> ents = GetAttendFreeLeaveByAttendSolID(strAttendSolID);
                if (ents != null)
                {
                    foreach (T_HR_ATTENDFREELEAVE ent in ents)
                    {
                        DeleteAttendFreeLeave(strAttendSolID);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(ex.Message);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }
        }
    }
}