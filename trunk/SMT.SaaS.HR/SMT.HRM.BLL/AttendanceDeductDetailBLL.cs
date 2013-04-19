
/*
 * 文件名：AttendanceDeductDetailBLL.cs
 * 作  用：考勤异常扣款明细 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 10:16:34
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
    public class AttendanceDeductDetailBLL : BaseBll<T_HR_ATTENDANCEDEDUCTDETAIL>
    {
        public AttendanceDeductDetailBLL()
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

            AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();

            flag = dalAttendanceDeductDetail.IsExistsRd(strFilter, objArgs);

            return flag;
        }

        /// <summary>
        /// 获取考勤异常扣款明细信息
        /// </summary>
        /// <param name="strAttendanceDeductDetailId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDANCEDEDUCTDETAIL GetAttendanceDeductDetailByID(string strAttendanceDeductDetailId)
        {
            if (string.IsNullOrEmpty(strAttendanceDeductDetailId))
            {
                return null;
            }

            AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendanceDeductDetailId))
            {
                strfilter.Append(" DEDUCTDETAILID == @0");
                objArgs.Add(strAttendanceDeductDetailId);
            }

            T_HR_ATTENDANCEDEDUCTDETAIL entRd = dalAttendanceDeductDetail.GetAttendanceDeductDetailRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款明细信息
        /// </summary>
        /// <param name="strOwnerID">登录用户的员工ID(权限控制)</param>
        /// <param name="strDeductMasterID">外键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCEDEDUCTDETAIL> GetAllAttendanceDeductDetailRdListByMultSearch(string strOwnerID, string strDeductMasterID, string strSortKey)
        {
            AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strDeductMasterID))
            {
                strfilter.Append(" T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @0");
                objArgs.Add(strDeductMasterID);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " DEDUCTDETAILID ";
            }

            string filterString = strfilter.ToString();

            //SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDANCEDEDUCTDETAIL");

            var q = dalAttendanceDeductDetail.GetAttendanceDeductDetailRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款明细信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">登录用户的员工ID(权限控制)</param>
        /// <param name="strDeductMasterID">外键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>考勤异常扣款明细信息</returns>
        public IQueryable<T_HR_ATTENDANCEDEDUCTDETAIL> GetAttendanceDeductDetailRdListByMultSearch(string strOwnerID, string strDeductMasterID, string strSortKey,
            int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendanceDeductDetailRdListByMultSearch(strOwnerID, strDeductMasterID, strSortKey);

            return Utility.Pager<T_HR_ATTENDANCEDEDUCTDETAIL>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤异常扣款明细信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddDeductDetail(T_HR_ATTENDANCEDEDUCTDETAIL entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                if (entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LateFineType1) + 1).ToString() ||
                    entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LateFineType2) + 1).ToString() ||
                    entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LeaveEarly1) + 1).ToString() ||
                    entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LeaveEarly2) + 1).ToString() ||
                    entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.SkipWork) + 1).ToString())
                {

                    strFilter.Append(" T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @0");
                    strFilter.Append(" && FINETYPE == @1");

                    objArgs.Add(entTemp.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID);
                    objArgs.Add(entTemp.FINETYPE);

                    flag = dalAttendanceDeductDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());
                }

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                if (entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LateFineType3) + 1).ToString() ||
                   entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LateFineType4) + 1).ToString() ||
                   entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LeaveEarly3) + 1).ToString() ||
                   entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.LeaveEarly4) + 1).ToString() ||
                   entTemp.FINETYPE == (Convert.ToInt32(Common.AttexFineType.DrainPunch) + 1).ToString())
                {
                    //分段扣款的，检查新加记录的最低次数是否小于已有记录中最高次数，如果小于，则提示用户修改最低次数
                    strMsg = CheckIsLimitTimes(entTemp);
                }

                if (!string.IsNullOrEmpty(strMsg))
                {
                    return strMsg;
                }

                T_HR_ATTENDANCEDEDUCTDETAIL ent = new T_HR_ATTENDANCEDEDUCTDETAIL();
                Utility.CloneEntity<T_HR_ATTENDANCEDEDUCTDETAIL>(entTemp, ent);
                ent.T_HR_ATTENDANCEDEDUCTMASTERReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCEDEDUCTMASTER", "DEDUCTMASTERID", entTemp.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID);
                Utility.RefreshEntity(ent);

                dalAttendanceDeductDetail.Add(ent);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤异常扣款明细信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyDeductDetail(T_HR_ATTENDANCEDEDUCTDETAIL entTemp)
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

                strFilter.Append(" DEDUCTDETAILID == @0");

                objArgs.Add(entTemp.DEDUCTDETAILID);

                AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();
                flag = dalAttendanceDeductDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                //检查提交的记录的次数范围是否与已有记录的次数范围有重叠
                strMsg = CheckIsLimitTimes(entTemp);
                if (!string.IsNullOrEmpty(strMsg))
                {
                    return strMsg;
                }

                T_HR_ATTENDANCEDEDUCTDETAIL entUpdate = dalAttendanceDeductDetail.GetAttendanceDeductDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAttendanceDeductDetail.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除考勤异常扣款明细信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteDeductDetail(string strAttendanceDeductDetailId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceDeductDetailId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" DEDUCTDETAILID == @0");

                objArgs.Add(strAttendanceDeductDetailId);

                AttendanceDeductDetailDAL dalAttendanceDeductDetail = new AttendanceDeductDetailDAL();
                flag = dalAttendanceDeductDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCEDEDUCTDETAIL entDel = dalAttendanceDeductDetail.GetAttendanceDeductDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendanceDeductDetail.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 分段扣款的，检查提交的记录的次数范围是否与已有记录的次数范围有重叠,然后返回检查后的消息
        /// </summary>
        /// <param name="entTemp">T_HR_ATTENDANCEDEDUCTDETAIL</param>
        /// <returns></returns>
        private string CheckIsLimitTimes(T_HR_ATTENDANCEDEDUCTDETAIL entTemp)
        {
            string strRes = string.Empty;

            //检测分段最低最高次数是否已填
            if (entTemp.LOWESTTIMES == null && entTemp.HIGHESTTIMES == null)
            {
                return "{REQUIREDFIELDS}";
            }

            //检测分段最低最高次数是否为非负数
            if (entTemp.LOWESTTIMES < 0)
            {
                return strRes;

            }

            bool flag = false;

            //对已有记录按最低次数进行升序排列
            var ents = from d in dal.GetObjects().Include("T_HR_ATTENDANCEDEDUCTMASTER")
                       where d.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == entTemp.T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID && d.FINETYPE == entTemp.FINETYPE
                       orderby d.LOWESTTIMES
                       select d;


            //无记录，即可认为是添加的第一条记录，返回空消息
            if (ents.Count() == 0)
            {
                return strRes;
            }           

            //判断预添加记录是否在已添加记录的限定次数范围内
            var qh = from r in ents
                     where r.LOWESTTIMES > entTemp.LOWESTTIMES && r.HIGHESTTIMES < entTemp.HIGHESTTIMES
                     select r;

            if (qh.Count() > 0)
            {
                flag = true;
            }

            if (flag)
            {
                return "{ATTENDANCEDEDUCTLIMITTIMESERROR}"; 
            }

            return strRes;
        }

        #endregion


        public string DeleteDeductDetailByMasterID(string strAttendanceDeductMasterId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceDeductMasterId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @0");

                objArgs.Add(strAttendanceDeductMasterId);

                AttendanceDeductDetailDAL dalDeductDetail = new AttendanceDeductDetailDAL();
                flag = dalDeductDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                string strOrderBy = " DEDUCTMASTERID ";
                var q = dalDeductDetail.GetAttendanceDeductDetailRdListByMultSearch(strOrderBy, strFilter.ToString(), objArgs.ToArray());

                if (q == null)
                {
                    return strMsg;
                }

                if (q.Count() == 0)
                {
                    return strMsg;
                }

                foreach (T_HR_ATTENDANCEDEDUCTDETAIL item in q)
                {
                    dalDeductDetail.Delete(item);
                }

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }
    }
}