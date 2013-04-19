
/*
 * 文件名：AttendanceSolutionDeductBLL.cs
 * 作  用：考勤方案异常扣款 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 15:49:25
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
    public class AttendanceSolutionDeductBLL : BaseBll<T_HR_ATTENDANCESOLUTIONDEDUCT>
    {
        public AttendanceSolutionDeductBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 根据参数，检查是否存在指定记录(方便进行数据增删改)
        /// </summary>
        /// <param name="strFilter">查询语句</param>
        /// <param name="objArgs">查询参数集合</param>
        /// <returns>True/False(是/否)</returns>
        public bool IsExistsRd(string strFilter, params object[] objArgs)
        {
            bool flag = false;

            if (string.IsNullOrEmpty(strFilter) && objArgs == null)
            {
                return false;
            }

            if (objArgs.Count() == 0)
            {
                return false;
            }

            AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();

            flag = dalAttendanceSolutionDeduct.IsExistsRd(strFilter, objArgs);

            return flag;
        }

        /// <summary>
        /// 获取考勤方案异常扣款信息
        /// </summary>
        /// <param name="strAttendanceSolutionDeductId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTIONDEDUCT GetAttendanceSolutionDeductByID(string strAttendanceSolutionDeductId)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionDeductId))
            {
                return null;
            }

            AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendanceSolutionDeductId))
            {
                strfilter.Append(" SOLUTIONDEDUCTID == @0");
                objArgs.Add(strAttendanceSolutionDeductId);
            }

            T_HR_ATTENDANCESOLUTIONDEDUCT entRd = dalAttendanceSolutionDeduct.GetAttendanceSolutionDeductRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取考勤方案异常扣款信息
        /// </summary>
        /// <param name="strAttendanceSolID"></param>
        /// <param name="strVacYear"></param>
        /// <param name="strCountyType"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCESOLUTIONDEDUCT> GetAttendanceSolutionDeductRdListByAttSolID(string strAttendanceSolID, string strSortKey)
        {
            AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();

            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strAttendanceSolID))
            {
                strfilter.Append(" T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID = @0");
                objArgs.Add(strAttendanceSolID);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " SOLUTIONDEDUCTID ";
            }

            var q = dalAttendanceSolutionDeduct.GetAttendanceSolutionDeductRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs.ToArray());
            return q;
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤方案异常扣款信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddAttSolDeduct(T_HR_ATTENDANCESOLUTIONDEDUCT entTemp)
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
                strFilter.Append(" && T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @1");

                objArgs.Add(entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                objArgs.Add(entTemp.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID);

                AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();
                flag = dalAttendanceSolutionDeduct.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                T_HR_ATTENDANCESOLUTIONDEDUCT ent = new T_HR_ATTENDANCESOLUTIONDEDUCT();
                Utility.CloneEntity<T_HR_ATTENDANCESOLUTIONDEDUCT>(entTemp, ent);
                ent.T_HR_ATTENDANCESOLUTIONReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCESOLUTION", "ATTENDANCESOLUTIONID", entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);

                ent.T_HR_ATTENDANCEDEDUCTMASTERReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCEDEDUCTMASTER", "DEDUCTMASTERID", entTemp.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID);


                dalAttendanceSolutionDeduct.Add(ent);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改T_HR_ATTENDANCESOLUTIONDEDUCT信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyAttSolDeduct(T_HR_ATTENDANCESOLUTIONDEDUCT entTemp)
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

                strFilter.Append(" SOLUTIONDEDUCTID == @0");

                objArgs.Add(entTemp.SOLUTIONDEDUCTID);

                AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();
                flag = dalAttendanceSolutionDeduct.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCESOLUTIONDEDUCT entUpdate = dalAttendanceSolutionDeduct.GetAttendanceSolutionDeductRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                entUpdate.T_HR_ATTENDANCESOLUTION = entTemp.T_HR_ATTENDANCESOLUTION;
                entUpdate.T_HR_ATTENDANCEDEDUCTMASTER = entTemp.T_HR_ATTENDANCEDEDUCTMASTER;
                entUpdate.REMARK = entTemp.REMARK;
                entUpdate.CREATEUSERID = entTemp.CREATEUSERID;
                entUpdate.CREATEDATE = entTemp.CREATEDATE;
                entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                entUpdate.UPDATEDATE = entTemp.UPDATEDATE;

                dalAttendanceSolutionDeduct.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除T_HR_ATTENDANCESOLUTIONDEDUCT信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteAttSolDeduct(string strAttendanceSolutionDeductId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceSolutionDeductId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" SOLUTIONDEDUCTID == @0");

                objArgs.Add(strAttendanceSolutionDeductId);

                AttendanceSolutionDeductDAL dalAttendanceSolutionDeduct = new AttendanceSolutionDeductDAL();
                flag = dalAttendanceSolutionDeduct.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCESOLUTIONDEDUCT entDel = dalAttendanceSolutionDeduct.GetAttendanceSolutionDeductRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendanceSolutionDeduct.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据考勤方案ID删除考勤方案异常记录
        /// </summary>
        /// <param name="strAttSolID">考勤方案ID</param>
        public void DeleteAttSolDeductByAttSolID(string strAttSolID)
        {
            try
            {
                var ents = from d in dal.GetObjects().Include("T_HR_ATTENDANCESOLUTION").Include("T_HR_ATTENDANCEDEDUCTMASTER")
                           where d.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID == strAttSolID
                           select d;

                if (ents != null)
                {
                    foreach (T_HR_ATTENDANCESOLUTIONDEDUCT ent in ents)
                    {
                        DeleteAttSolDeduct(ent.SOLUTIONDEDUCTID);
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

        #endregion

        
    }
}