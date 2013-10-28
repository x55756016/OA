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
    public class NoAttendCardEmployeesBLL : BaseBll<T_HR_NOATTENDCARDEMPLOYEES>, IOperate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public NoAttendCardEmployeesBLL()
        {
        }
        #region 获取数据


        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的加班记录信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值,不能为空，否则报错</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_NOATTENDCARDEMPLOYEES> GetNoAttendCardEmployeesPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            string employeeName = string.Empty;
            if (paras.Count > 0)
            {
                employeeName = paras[0].ToString();
                paras.Clear();
            }
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }
                
                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_NOATTENDCARDEMPLOYEES");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("OVERTIMERECORDID", "T_HR_NOATTENDCARDEMPLOYEES", strOwnerID, ref strCheckState, ref filterString, ref paras);
                if (string.Compare(strCheckfilter, filterString) == 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(strCheckState))
            {
                int iIndex = 0;
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (paras.Count() > 0)
                {
                    iIndex = paras.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                paras.Add(strCheckState);
            }

            IQueryable<T_HR_NOATTENDCARDEMPLOYEES> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            if (!string.IsNullOrEmpty(sort))
            {
                ents = ents.OrderBy(sort);
            }
            if (!string.IsNullOrEmpty(employeeName))
            {
                ents = from ent in ents
                       where employeeName.Contains(ent.EMPLOYEENAME)
                       select ent;
            }
            ents = Utility.Pager<T_HR_NOATTENDCARDEMPLOYEES>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据员工ID获取一段时间内员工加班信息
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        public bool IsNoNeedCardEmployee(string strEmployeeID, DateTime dtStart)
        {
            string strCheckStates = Convert.ToInt32(Common.CheckStates.Approved).ToString();

            var ents = from en in dal.GetObjects()
                      where strEmployeeID.Contains(en.EMPLOYEEID)
                     && en.STARTDATE <= dtStart &&
                     en.ENDDATE >= dtStart
                     && en.CHECKSTATE == strCheckStates
                      select en;
            if (ents.Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取免打卡设置单信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        public T_HR_NOATTENDCARDEMPLOYEES GetRecordByID(string strId)
        {
            var ents = from a in dal.GetTable()
                       where a.NOATTENDCARDEMPLOYEESID == strId
                       select a;

            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault();
            }
            else
            {
                Tracer.Debug("T_HR_NOATTENDCARDEMPLOYEES GetRecordByID 获取为空：+NOATTENDCARDEMPLOYEESID=" + strId);
            }
            return null;
        }

        #endregion


        #region 操作
        //使用父类增删改查方法
        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        public int DeleteRecordByIds(string[] strOverTimeRecordId)
        {
            try
            {
                foreach (var id in strOverTimeRecordId)
                {
                    var ent = GetRecordByID(id);
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        DeleteMyRecord(ent);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return 0;
            }
        }

        public int Update(T_HR_NOATTENDCARDEMPLOYEES entity)
        {
            var ent = GetRecordByID(entity.NOATTENDCARDEMPLOYEESID);
            if (ent != null)
            {
                Utility.CloneEntity(entity, ent);
                return dal.Update(ent);
            }
            else
                return 0;
        }
        #endregion

        #region 审核流程调用更新业务单据状态
        /// <summary>
        /// 引擎更新单据状态专用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="strEntityKeyName"></param>
        /// <param name="strEntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                Tracer.Debug("开始更新员工免打卡设置单");
                int i = 0;
                string strMsg = string.Empty;
                strMsg = AuditRecord(EntityKeyValue, CheckState);
                if (strMsg == "{SAVESUCCESSED}")
                {
                    i = 1;
                }
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + strMsg);
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + CheckState
                    + " 异常：" + e.ToString());
                return 0;
            }
        }
        /// <summary>
        /// 审核加班记录
        /// </summary>
        /// <param name="strOverTimeRecordID">主键索引</param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        public string AuditRecord(string strOverTimeRecordID, string strCheckState)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strOverTimeRecordID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_NOATTENDCARDEMPLOYEES entOTRd = GetRecordByID(strOverTimeRecordID);

                if (entOTRd == null)
                {
                    return "{NOTFOUND}";
                }

                entOTRd.CHECKSTATE = strCheckState;

                entOTRd.UPDATEDATE = DateTime.Now;

                //Utility.CloneEntity(entOTRd, ent);
                int i = dal.Update(entOTRd);
                if (i == 0)
                {
                    Tracer.Debug("更新T_HR_NOATTENDCARDEMPLOYEES失败");
                    throw new Exception("更新T_HR_NOATTENDCARDEMPLOYEES失败");
                }
                SaveMyRecord(entOTRd);


                return "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = "{AUDITERROR}";
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }
        #endregion
    }
}

