
/*
 * 文件名：AttendMonthlyBatchBalanceBLL.cs
 * 作  用：考勤月度批量结算 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-5-21 16:26:02
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
    public class AttendMonthlyBatchBalanceBLL : BaseBll<T_HR_ATTENDMONTHLYBATCHBALANCE>,IOperate
    {
        public AttendMonthlyBatchBalanceBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取考勤月度批量结算信息
        /// </summary>
        /// <param name="strAttendMonthlyBatchBalanceId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByID(string strAttendMonthlyBatchBalanceId)
        {
            if (string.IsNullOrEmpty(strAttendMonthlyBatchBalanceId))
            {
                return null;
            }

            AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAttendMonthlyBatchBalanceId))
            {
                strfilter.Append(" MONTHLYBATCHID == @0");
                objArgs.Add(strAttendMonthlyBatchBalanceId);
            }

            T_HR_ATTENDMONTHLYBATCHBALANCE entRd = dalAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 获取考勤月度批量结算信息
        /// </summary>
        /// <param name="strAttendMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByMonthlyBalanceId(string strAttendMonthlyBalanceId)
        {
            if (string.IsNullOrEmpty(strAttendMonthlyBalanceId))
            {
                return null;
            }

            var q = from a in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>().Include("T_HR_ATTENDMONTHLYBATCHBALANCE")
                    where a.MONTHLYBALANCEID == strAttendMonthlyBalanceId
                    select a;

            if (q == null)
            {
                return null;
            }

            if (q.Count() == 0)
            {
                return null;
            }

            return q.FirstOrDefault().T_HR_ATTENDMONTHLYBATCHBALANCE;
        }

        /// <summary>
        /// 获取考勤月度批量结算信息
        /// </summary>
        /// <param name="strBalanceObjectType"></param>
        /// <param name="strBalanceObjectId"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        public T_HR_ATTENDMONTHLYBATCHBALANCE GetAttendMonthlyBatchBalanceByMultSearch(string strOwnerID, string strBalanceObjectType, string strBalanceObjectId, decimal dBalanceYear, decimal dBalanceMonth, string strCheckState)
        {
            if (string.IsNullOrEmpty(strBalanceObjectType) || string.IsNullOrEmpty(strBalanceObjectId) || dBalanceYear <= 0 || dBalanceMonth <= 0)
            {
                return null;
            }

            AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strBalanceObjectType))
            {
                strfilter.Append(" BALANCEOBJECTTYPE == @0");
                objArgs.Add(strBalanceObjectType);
            }

            if (!string.IsNullOrEmpty(strBalanceObjectId))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" BALANCEOBJECTID == @" + iIndex.ToString());
                objArgs.Add(strBalanceObjectId);
            }

            string filterString = strfilter.ToString();

            if (strCheckState != Convert.ToInt32(Common.CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDMONTHLYBALANCE");
            }

            SetFilterWithflow("MONTHLYBATCHID", "T_HR_ATTENDMONTHLYBATCHBALANCE", strOwnerID, ref strCheckState, ref filterString, ref objArgs);

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

            T_HR_ATTENDMONTHLYBATCHBALANCE entRd = dalAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceByMultSearch(dBalanceYear, dBalanceMonth, filterString, objArgs.ToArray());

            return entRd;
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增考勤月度批量结算信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
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

                strFilter.Append(" MONTHLYBATCHID == @0");

                objArgs.Add(entTemp.MONTHLYBATCHID);

                AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
                flag = dalAttendMonthlyBatchBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dal.AddToContext(entTemp);
                dal.SaveContextChanges();
                SaveMyRecord(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 修改考勤月度批量结算信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
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

                strFilter.Append(" MONTHLYBATCHID == @0");

                objArgs.Add(entTemp.MONTHLYBATCHID);

                AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
                flag = dalAttendMonthlyBatchBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDMONTHLYBATCHBALANCE entUpdate = dalAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAttendMonthlyBatchBalance.Update(entUpdate);
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
        /// 根据主键索引，删除考勤月度批量结算信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteMonthlyBatchBalance(string strAttendMonthlyBatchBalanceId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendMonthlyBatchBalanceId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" MONTHLYBATCHID == @0");

                objArgs.Add(strAttendMonthlyBatchBalanceId);

                AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
                flag = dalAttendMonthlyBatchBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDMONTHLYBATCHBALANCE entDel = dalAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendMonthlyBatchBalance.Delete(entDel);
                DeleteMyRecord(entDel);
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
        /// 月度结算批量审批
        /// </summary>
        /// <param name="entTemp"></param>
        /// <param name="entBalanceList"></param>
        /// <returns></returns>
        public string AuditMonthlyBatchBalance(T_HR_ATTENDMONTHLYBATCHBALANCE entTemp)
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

                strFilter.Append(" MONTHLYBATCHID == @0");
                objArgs.Add(entTemp.MONTHLYBATCHID);

                AttendMonthlyBatchBalanceDAL dalAttendMonthlyBatchBalance = new AttendMonthlyBatchBalanceDAL();
                flag = dalAttendMonthlyBatchBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    AddMonthlyBatchBalance(entTemp);
                }

                T_HR_ATTENDMONTHLYBATCHBALANCE entAudit = dalAttendMonthlyBatchBalance.GetAttendMonthlyBatchBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                if (entAudit == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                if (flag)
                {
                    Utility.CloneEntity(entTemp, entAudit);
                    Update(entAudit);
                }
                string strTempCheckState = Convert.ToInt32(Common.CheckStates.All).ToString();

                AttendMonthlyBalanceBLL bllBalance = new AttendMonthlyBalanceBLL();
                IQueryable<T_HR_ATTENDMONTHLYBALANCE> entBalanceList = bllBalance.GetAllAttendMonthlyBalanceRdListForAudit(entTemp.BALANCEOBJECTTYPE,
                    entTemp.BALANCEOBJECTID, entTemp.OWNERID, strTempCheckState, entTemp.BALANCEYEAR.Value, entTemp.BALANCEMONTH.Value, "BALANCEYEAR, BALANCEMONTH");
                int cout = 0;
                foreach (T_HR_ATTENDMONTHLYBALANCE item in entBalanceList)
                {
                    //item.T_HR_ATTENDMONTHLYBATCHBALANCEReference.EntityKey = new EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDMONTHLYBATCHBALANCE", "MONTHLYBATCHID", entAudit.MONTHLYBATCHID);                    
                    item.T_HR_ATTENDMONTHLYBATCHBALANCE = entAudit;
                    item.CHECKSTATE = entTemp.CHECKSTATE;
                    item.EDITSTATE = entTemp.EDITSTATE;
                    bllBalance.ModifyMonthlyBalance(item);
                    cout++;
                }
                strMsg = "{SAVESUCCESSED}";
                //添加日志
                if (entAudit != null)
                {
                    string str = entAudit.MONTHLYBATCHID + entAudit.BALANCEOBJECTNAME + entAudit.BALANCEYEAR + "年" + entAudit.BALANCEMONTH + "月" + "的人数为" + cout;
                    Utility.SaveLog(str);
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var attendBatch = (from c in dal.GetObjects<T_HR_ATTENDMONTHLYBATCHBALANCE>()
                                   where c.MONTHLYBATCHID == EntityKeyValue
                                   select c).FirstOrDefault();
                if (attendBatch != null)
                {
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        attendBatch.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        attendBatch.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    attendBatch.CHECKSTATE = CheckState;
                    strMsg = AuditMonthlyBatchBalance(attendBatch);
                    if (strMsg == "{SAVESUCCESSED}")
                    {
                        i = 1;
                    }
                }

                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
        #endregion

    }
}