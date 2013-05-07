
/*
 * 文件名：AttendanceSolutionBLL.cs
 * 作  用：考勤方案 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-2-26 14:49:08
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    public class AttendanceSolutionBLL : BaseBll<T_HR_ATTENDANCESOLUTION>, ILookupEntity, IOperate
    {
        public AttendanceSolutionBLL()
        { }

        #region 获取数据吴鹏新增
        /// <summary>
        /// 根据考勤方案ID，当日的起止时间，获取当天的实际上下班时间（计算考勤异常时使用）
        /// </summary>
        /// <param name="strAttendanceSolutionId">考勤方案ID</param>
        /// <param name="dtWorkStart">当日的起始时间</param>
        /// <param name="dtWorkEnd">当日的截止时间</param>
        /// <returns>返回结果（true/ false）</returns>
        public bool GetAttendDateWorkTime(string strAttendanceSolutionId, ref DateTime dtWorkStart, ref DateTime dtWorkEnd)
        {
            bool bflag = false;
            T_HR_ATTENDANCESOLUTION entAttSol = GetAttendanceSolutionByID(strAttendanceSolutionId);

            if(entAttSol == null)
            {
                return bflag;
            }

            T_HR_SCHEDULINGTEMPLATEMASTER entScheduleMaster = entAttSol.T_HR_SCHEDULINGTEMPLATEMASTER;

            if (entScheduleMaster == null)
            {
                return bflag;
            }

            var ent = (from s in dal.GetObjects<T_HR_SCHEDULINGTEMPLATEDETAIL>().Include("T_HR_SHIFTDEFINE")
                       where s.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID == entScheduleMaster.TEMPLATEMASTERID
                       select s.T_HR_SHIFTDEFINE).FirstOrDefault();

            if (ent == null)
            {
                return bflag;
            }

            bool bDate = false;
            if (!string.IsNullOrWhiteSpace(ent.FIRSTSTARTTIME))
            {
                bDate = DateTime.TryParse(dtWorkStart.ToString("yyyy-MM-dd") + " " + ent.FIRSTSTARTTIME, out dtWorkStart);
            }

            if (!bDate)
            {
                return bflag;
            }

            if (!string.IsNullOrWhiteSpace(ent.FOURTHENDTIME))
            {
                bDate = DateTime.TryParse(dtWorkEnd.ToString("yyyy-MM-dd") + " " + ent.FOURTHENDTIME, out dtWorkEnd);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(ent.THIRDENDTIME))
                {
                    bDate = DateTime.TryParse(dtWorkEnd.ToString("yyyy-MM-dd") + " " + ent.THIRDENDTIME, out dtWorkEnd);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(ent.SECONDENDTIME))
                    {
                        bDate = DateTime.TryParse(dtWorkEnd.ToString("yyyy-MM-dd") + " " + ent.SECONDENDTIME, out dtWorkEnd);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(ent.FIRSTENDTIME))
                        {
                            bDate = DateTime.TryParse(dtWorkEnd.ToString("yyyy-MM-dd") + " " + ent.FIRSTENDTIME, out dtWorkEnd);
                        }
                    }
                }
            }

            return bflag;
        }

        /// <summary>
        /// 根据加班设置id查找考勤方案定义表是否有用到
        /// </summary>
        /// <param name="OVERTimeId">加班设置id</param>
        /// <returns>返回考勤方案信息</returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByOVERTime(string OVERTimeId)
        {
            if (string.IsNullOrEmpty(OVERTimeId))
            {
                return null;
            }

            AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();

            if (!string.IsNullOrEmpty(OVERTimeId))
            {
                strfilter.Append(" OVERTIMEREWARDID == @0");
                objArgs.Add(OVERTimeId);
            }

            T_HR_ATTENDANCESOLUTION entRd = dalAttendanceSolution.GetAttendanceSolutionRdByOVERId(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 获取考勤方案信息
        /// </summary>
        /// <param name="strAttendanceSolutionId">主键索引</param>
        /// <returns>返回考勤方案信息</returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByID(string strAttendanceSolutionId)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionId))
            {
                return null;
            }

            AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();

            if (!string.IsNullOrEmpty(strAttendanceSolutionId))
            {
                strfilter.Append(" ATTENDANCESOLUTIONID == @0");
                objArgs.Add(strAttendanceSolutionId);
            }

            T_HR_ATTENDANCESOLUTION entRd = dalAttendanceSolution.GetAttendanceSolutionRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据员工ID获取其应用的考勤方案
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeID(string strEmployeeID)
        {
            if (string.IsNullOrEmpty(strEmployeeID))
            {
                return null;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeID(strEmployeeID);
            if (entAttendSolAsign == null)
            {
                return null;
            }

            return entAttendSolAsign.T_HR_ATTENDANCESOLUTION;
        }

        /// <summary>
        /// 根据员工ID获取其应用的考勤方案(解决一个员工在系统内多个公司入职的情况)
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeID(string strCompanyID, string strEmployeeID)
        {
            if (string.IsNullOrEmpty(strCompanyID) || string.IsNullOrEmpty(strEmployeeID))
            {
                return null;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeID(strCompanyID, strEmployeeID);
            if (entAttendSolAsign == null)
            {
                return null;
            }

            return entAttendSolAsign.T_HR_ATTENDANCESOLUTION;
        }

        /// <summary>
        /// 根据员工ID,起止时间(一般为月头至月尾)获取其应用的考勤方案
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤记录参考起始日期</param>
        /// <param name="dtEnd">考勤记录参考截止日期</param>
        /// <returns></returns>
        public T_HR_ATTENDANCESOLUTION GetAttendanceSolutionByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            if (string.IsNullOrEmpty(strEmployeeID))
            {
                return null;
            }

            AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
            T_HR_ATTENDANCESOLUTION entAttSol = dalAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd);

            if (entAttSol == null)
            {
                AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtStart);

                if (entAttSolAsign == null)
                {
                    return null;
                }

                return entAttSolAsign.T_HR_ATTENDANCESOLUTION;
            }

            return entAttSol;
        }

        /// <summary>
        /// 根据条件，获取考勤方案信息
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strAttendanceSolutionName">考勤方案名称</param>
        /// <param name="strAttendanceType">考勤方式</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>返回考勤方案信息</returns>
        public IQueryable<T_HR_ATTENDANCESOLUTION> GetAllAttendanceSolutionRdListByMultSearch(string strOwnerID, string strCheckState, string strAttendanceSolutionName, string strAttendanceType, string strSortKey)
        {
            AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strAttendanceSolutionName))
            {
                strfilter.Append(" @0.Contains(ATTENDANCESOLUTIONNAME)");
                objArgs.Add(strAttendanceSolutionName);
            }

            if (!string.IsNullOrEmpty(strAttendanceType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" ATTENDANCETYPE == @" + iIndex.ToString());
                objArgs.Add(strAttendanceType);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ATTENDANCESOLUTIONID ";
            }

            string filterString = strfilter.ToString();

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDANCESOLUTION");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("ATTENDANCESOLUTIONID", "T_HR_ATTENDANCESOLUTION", strOwnerID, ref strCheckState, ref filterString, ref objArgs);
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

            var q = dalAttendanceSolution.GetAttendanceSolutionRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取考勤方案信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strAttendanceSolutionName">考勤方案名称</param>
        /// <param name="strAttendanceType">考勤方式</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>考勤方案信息</returns>
        public IQueryable<T_HR_ATTENDANCESOLUTION> GetAttendanceSolutionRdListByMultSearch(string strOwnerID, string strCheckState, string strAttendanceSolutionName, string strAttendanceType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendanceSolutionRdListByMultSearch(strOwnerID, strCheckState, strAttendanceSolutionName, strAttendanceType, strSortKey);

            return Utility.Pager<T_HR_ATTENDANCESOLUTION>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤方案信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddAttSol(T_HR_ATTENDANCESOLUTION entTemp)
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
                List<object> objArgs = new List<object>();

                strFilter.Append(" ATTENDANCESOLUTIONNAME == @0");

                objArgs.Add(entTemp.ATTENDANCESOLUTIONNAME);

                AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
                flag = dalAttendanceSolution.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                T_HR_ATTENDANCESOLUTION ent = new T_HR_ATTENDANCESOLUTION();
                Utility.CloneEntity<T_HR_ATTENDANCESOLUTION>(entTemp, ent);
                ent.T_HR_OVERTIMEREWARDReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_OVERTIMEREWARD", "OVERTIMEREWARDID", entTemp.T_HR_OVERTIMEREWARD.OVERTIMEREWARDID);
                ent.T_HR_SCHEDULINGTEMPLATEMASTERReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SCHEDULINGTEMPLATEMASTER", "TEMPLATEMASTERID", entTemp.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID);

                Utility.RefreshEntity(ent);

                dalAttendanceSolution.Add(ent);
                SaveMyRecord(ent);
                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(strMsg);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤方案信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyAttSol(T_HR_ATTENDANCESOLUTION entTemp)
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
                List<object> objArgs = new List<object>();

                strFilter.Append(" ATTENDANCESOLUTIONID == @0");

                objArgs.Add(entTemp.ATTENDANCESOLUTIONID);

                AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
                flag = dalAttendanceSolution.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                StringBuilder strCheckFilter = new StringBuilder();
                List<object> objCheckArgs = new List<object>();
                strCheckFilter.Append(" ATTENDANCESOLUTIONNAME == @0");
                strCheckFilter.Append(" && ATTENDANCESOLUTIONID != @1");
                objCheckArgs.Add(entTemp.ATTENDANCESOLUTIONNAME);
                objCheckArgs.Add(entTemp.ATTENDANCESOLUTIONID);

                flag = dalAttendanceSolution.IsExistsRd(strCheckFilter.ToString(), objCheckArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                T_HR_ATTENDANCESOLUTION entUpdate = dalAttendanceSolution.GetAttendanceSolutionRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                Utility.CloneEntity(entTemp, entUpdate);
                entUpdate.T_HR_OVERTIMEREWARD.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_OVERTIMEREWARD", "OVERTIMEREWARDID", entTemp.T_HR_OVERTIMEREWARD.OVERTIMEREWARDID);
                entUpdate.T_HR_OVERTIMEREWARDReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_OVERTIMEREWARD", "OVERTIMEREWARDID", entTemp.T_HR_OVERTIMEREWARD.OVERTIMEREWARDID);


                entUpdate.T_HR_SCHEDULINGTEMPLATEMASTER.EntityKey =
                     new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SCHEDULINGTEMPLATEMASTER", "TEMPLATEMASTERID", entTemp.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID);
                entUpdate.T_HR_SCHEDULINGTEMPLATEMASTERReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SCHEDULINGTEMPLATEMASTER", "TEMPLATEMASTERID", entTemp.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID);

                dalAttendanceSolution.Update(entUpdate);
                SaveMyRecord(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(strMsg);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 审核当前考勤方案
        /// </summary>
        /// <returns></returns>
        public string AuditAttSol(string strAttendanceSolutionId, string strCheckState)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceSolutionId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<object> objArgs = new List<object>();

                strFilter.Append(" ATTENDANCESOLUTIONID == @0");

                objArgs.Add(strAttendanceSolutionId);

                AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
                flag = dalAttendanceSolution.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                //审核状态变为审核中或者审核通过时，生成对应的员工考勤记录(应用的员工范围，视应用对象而定)
                string strEditState = string.Empty;
                if (strCheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                }
                else if (strCheckState == Convert.ToInt32(CheckStates.Approving).ToString())
                {
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                }
                else if (strCheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    strEditState = Convert.ToInt32(EditStates.Canceled).ToString();
                }

                T_HR_ATTENDANCESOLUTION entAudit = dalAttendanceSolution.GetAttendanceSolutionRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                //已审核通过的记录禁止再次提交审核
                if (entAudit.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    return "{REPEATAUDITERROR}";
                }

                entAudit.EDITSTATE = strEditState;
                entAudit.CHECKSTATE = strCheckState;

                dal.Update(entAudit);
                SaveMyRecord(entAudit);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
                else
                {
                    Utility.SaveLog(strMsg);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除考勤方案信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteAttSol(string strAttendanceSolutionId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceSolutionId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<object> objArgs = new List<object>();

                strFilter.Append(" ATTENDANCESOLUTIONID == @0");

                objArgs.Add(strAttendanceSolutionId);

                AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
                flag = dalAttendanceSolution.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCESOLUTION entDel = dalAttendanceSolution.GetAttendanceSolutionRdByMultSearch(strFilter.ToString(), objArgs.ToArray());

                if (entDel.CHECKSTATE != Convert.ToInt32(Common.CheckStates.UnSubmit).ToString())
                {
                    return "{DELETEAUDITERROR}";
                }

                dalAttendanceSolution.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(strMsg);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entTemp"></param>
        /// <param name="entAttendanceSolutionDeducts"></param>
        /// <param name="entAttendFreeLeaves"></param>
        /// <returns></returns>
        public string AddAndCreateRelation(T_HR_ATTENDANCESOLUTION entTemp, List<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts, List<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = AddAttSol(entTemp);

                if (strMsg != "{SAVESUCCESSED}")
                {
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        strMsg = "{ADDERROR}";
                    }

                    return strMsg;
                }

                foreach (T_HR_ATTENDANCESOLUTIONDEDUCT item in entAttendanceSolutionDeducts)
                {
                    AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL();
                    bllAttendanceSolutionDeduct.AddAttSolDeduct(item);
                }

                foreach (T_HR_ATTENDFREELEAVE item in entAttendFreeLeaves)
                {
                    AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                    bllAttendFreeLeave.AddAttendFreeLeave(item);
                }

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤方案信息,并更改其关联设置的关系
        /// </summary>
        /// <param name="entTemp"></param>
        /// <param name="entAttendanceSolutionDeducts"></param>
        /// <param name="entAttendFreeLeaves"></param>
        /// <returns></returns>
        public string ModifyAndChangeRelation(T_HR_ATTENDANCESOLUTION entTemp, List<T_HR_ATTENDANCESOLUTIONDEDUCT> entAttendanceSolutionDeducts, List<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = ModifyAttSol(entTemp);

                if (strMsg != "{SAVESUCCESSED}")
                {
                    return "{EDITERROR}";
                }

                AttendanceSolutionDeductBLL bllAttendanceSolutionDeduct = new AttendanceSolutionDeductBLL();
                bllAttendanceSolutionDeduct.DeleteAttSolDeductByAttSolID(entTemp.ATTENDANCESOLUTIONID);

                foreach (T_HR_ATTENDANCESOLUTIONDEDUCT itemDeduct in entAttendanceSolutionDeducts)
                {
                    if (itemDeduct.EntityKey != null)
                    {
                        itemDeduct.EntityKey = null;  //清除EntityKey不为null的情况
                    }

                    if (itemDeduct.T_HR_ATTENDANCESOLUTION == null)
                    {
                        itemDeduct.T_HR_ATTENDANCESOLUTION = entTemp;
                    }

                    strMsg = bllAttendanceSolutionDeduct.AddAttSolDeduct(itemDeduct);
                }

                AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                bllAttendFreeLeave.ModifyAttendFreeLeaveByAttSolID(entTemp.ATTENDANCESOLUTIONID);
                foreach (T_HR_ATTENDFREELEAVE itemFL in entAttendFreeLeaves)
                {
                    if (itemFL.EntityKey != null)
                    {
                        itemFL.EntityKey = null;  //清除EntityKey不为null的情况
                    }

                    if (itemFL.T_HR_ATTENDANCESOLUTION == null)
                    {
                        itemFL.T_HR_ATTENDANCESOLUTION = entTemp;
                    }

                    strMsg = bllAttendFreeLeave.AddAttendFreeLeave(itemFL);
                }

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(strMsg);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }
        #endregion

        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            AttendanceSolutionDAL dalAttendanceSolution = new AttendanceSolutionDAL();
            List<object> queryParas = new List<object>();

            if (paras.Count() > 0)
            {
                for (int i = 0; i < paras.Count(); i++)
                {
                    queryParas.Add(paras[i]);
                }
            }

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_ATTENDANCESOLUTION");

            string strOrderBy = string.Empty;
            strOrderBy = " ATTENDANCESOLUTIONID ";

            IQueryable<T_HR_ATTENDANCESOLUTION> ents = dalAttendanceSolution.GetAttendanceSolutionRdListByMultSearch(strOrderBy, filterString, queryParas.ToArray());

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion

        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strResult = AuditAttSol(EntityKeyValue, CheckState);
                if (strResult == "{SAVESUCCESSED}")
                {
                    i = 1;
                }
                else
                {
                    i = 0;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}