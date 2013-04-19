
/*
 * 文件名：AttendanceDeductMasterBLL.cs
 * 作  用：考勤异常扣款 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 9:19:14
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
using SMT.HRM.CustomModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{

    public class AttendanceDeductMasterBLL : BaseBll<T_HR_ATTENDANCEDEDUCTMASTER>, ILookupEntity
    {
        protected SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient permClient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();

        public AttendanceDeductMasterBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取考勤异常扣款信息
        /// </summary>
        /// <param name="strAttendanceDeductMasterId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDANCEDEDUCTMASTER GetAttendanceDeductMasterByID(string strAttendanceDeductMasterId)
        {
            if (string.IsNullOrEmpty(strAttendanceDeductMasterId))
            {
                return null;
            }

            AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendanceDeductMasterId))
            {
                strfilter.Append(" DEDUCTMASTERID == @0");
                objArgs.Add(strAttendanceDeductMasterId);
            }

            T_HR_ATTENDANCEDEDUCTMASTER entRd = dalAttendanceDeductMaster.GetAttendanceDeductMasterRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款信息
        /// </summary>
        /// <param name="strAttType"></param>
        /// <param name="strFineType"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCEDEDUCTMASTER> GetAllAttendanceDeductMasterRdListByMultSearch(string strOwnerID, string strAttType, string strSortKey)
        {
            AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strAttType))
            {
                strfilter.Append(" ATTENDABNORMALTYPE == @0");
                objArgs.Add(strAttType);
            }            

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " DEDUCTMASTERID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDANCEDEDUCTMASTER");


            var q = dalAttendanceDeductMaster.GetAttendanceDeductMasterRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取考勤异常扣款主表信息,并进行分页
        /// </summary>
        /// <param name="strAttType">考勤状态</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>T_HR_ATTENDANCEDEDUCTMASTER信息</returns>
        public IQueryable<T_HR_ATTENDANCEDEDUCTMASTER> GetAttendanceDeductMasterRdListByMultSearch(string strOwnerID, string strAttType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendanceDeductMasterRdListByMultSearch(strOwnerID, strAttType, strSortKey);

            return Utility.Pager<T_HR_ATTENDANCEDEDUCTMASTER>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤异常扣款主表信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddDeductMaster(T_HR_ATTENDANCEDEDUCTMASTER entTemp)
        {
            string strMsg = string.Empty;
            try
            {

                StringBuilder strfilter = new StringBuilder();
                List<string> objArgs = new List<string>();
                bool flag = false;

                strfilter.Append("  ATTENDABNORMALNAME == @0");
                strfilter.Append(" && ATTENDABNORMALTYPE == @1");

                objArgs.Add(entTemp.ATTENDABNORMALNAME);
                objArgs.Add(entTemp.ATTENDABNORMALTYPE);

                AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();
                flag = dalAttendanceDeductMaster.IsExistsRd(strfilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }
                
                dalAttendanceDeductMaster.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤异常扣款主表信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyDeductMaster(T_HR_ATTENDANCEDEDUCTMASTER entTemp)
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

                strFilter.Append(" DEDUCTMASTERID == @0");

                objArgs.Add(entTemp.DEDUCTMASTERID);

                AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();
                flag = dalAttendanceDeductMaster.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                //dalAttendanceDeductMaster.DataContext.SaveChanges();
                T_HR_ATTENDANCEDEDUCTMASTER entUpdate = dalAttendanceDeductMaster.GetAttendanceDeductMasterRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAttendanceDeductMaster.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除考勤异常扣款信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteDeductMaster(string strAttendanceDeductMasterId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceDeductMasterId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strAttDedFilter = new StringBuilder();
                List<string> objAttDedArgs = new List<string>();

                strAttDedFilter.Append(" DEDUCTMASTERID == @0");

                objAttDedArgs.Add(strAttendanceDeductMasterId);

                AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();
                flag = dalAttendanceDeductMaster.IsExistsRd(strAttDedFilter.ToString(), objAttDedArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                StringBuilder strAttDetailFilter = new StringBuilder();
                List<string> objAttDetailArgs = new List<string>();

                strAttDetailFilter.Append(" T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @0");

                objAttDetailArgs.Add(strAttendanceDeductMasterId);

                AttendanceDeductDetailBLL bllAttendanceDeductDetail = new AttendanceDeductDetailBLL();
                bllAttendanceDeductDetail.DeleteDeductDetailByMasterID(strAttendanceDeductMasterId);

                StringBuilder strAttSolDedFilter = new StringBuilder();
                List<string> objAttSolDedArgs = new List<string>();

                strAttSolDedFilter.Append(" T_HR_ATTENDANCESOLUTION.CHECKSTATE != @0");
                strAttSolDedFilter.Append(" && T_HR_ATTENDANCEDEDUCTMASTER.DEDUCTMASTERID == @1");

                objAttSolDedArgs.Add(Convert.ToInt32(CheckStates.UnSubmit).ToString());
                objAttSolDedArgs.Add(strAttendanceDeductMasterId);

                AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL();
                flag = bllAttendanceSolutionDeduct.IsExistsRd(strAttSolDedFilter.ToString(), objAttSolDedArgs.ToArray());

                if (flag)
                {
                    return "已存在进入审核状态的考勤方案与当前的考勤异常扣款设置有关联，不能直接删除";
                }

                T_HR_ATTENDANCEDEDUCTMASTER entDel = dalAttendanceDeductMaster.GetAttendanceDeductMasterRdByMultSearch(strAttDedFilter.ToString(), objAttDedArgs.ToArray());
                dalAttendanceDeductMaster.Delete(entDel);

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
            AttendanceDeductMasterDAL dalAttendanceDeductMaster = new AttendanceDeductMasterDAL();

            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            strOrderBy = " DEDUCTMASTERID ";
            objArgs.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref objArgs, userID, "T_HR_ATTENDANCEDEDUCTMASTER");

            IQueryable<T_HR_ATTENDANCEDEDUCTMASTER> ents = dalAttendanceDeductMaster.GetAttendanceDeductMasterRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            T_HR_ATTENDANCEDEDUCTMASTER[] temp = ents.Count() > 0 ? ents.ToArray() : null;
                        
            return temp;
        }

        private string FormatTextBySysDic(string strColValue, string strSysCategory)
        {
            string strTemp = string.Empty;
            decimal dValue = 0;
            decimal.TryParse(strColValue, out dValue);
            SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_DICTIONARY[] entDics = permClient.GetSysDictionaryByCategory(strSysCategory);
            strTemp = entDics.Where(c => c.DICTIONARYVALUE == dValue).First().DICTIONARYNAME;

            strTemp = string.IsNullOrEmpty(strTemp) == true ? "-" : strTemp;
            return strTemp;
        }
        #endregion
    }
}