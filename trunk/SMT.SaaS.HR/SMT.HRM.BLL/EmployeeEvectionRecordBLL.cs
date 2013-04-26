using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
using System.Collections;
using System.Linq.Dynamic;
using SMT.HRM.DAL;
using SMT.Foundation.Log;


namespace SMT.HRM.BLL
{
    public class EmployeeEvectionRecordBLL:BaseBll<T_HR_EMPLOYEEEVECTIONRECORD>
    {
        public IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> GetEmployeeEvectionRecordsByEmployeeIdAndDate(string strEmployeeId, DateTime dtDate)
        {
            var q = from e in dal.GetObjects()
                    where e.EMPLOYEEID == strEmployeeId && e.ENDDATE >= dtDate && e.CHECKSTATE == "2"
                    select e;
            return q;
        }

        /// <summary>
        /// 获取所有的出差记录信息
        /// </summary>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strDateFrom">出差起始日期</param>
        /// <param name="strDateTo">出差截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        private IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> GetAllEmployeeEvectionRecordByMultSearch(string strOwnerID, string strEmployeeID, string strDateFrom, string strDateTo, string strSortKey)
        {
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            string filterString = strfilter.ToString();

            if (!string.IsNullOrEmpty(strOwnerID))
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEEEVECTIONRECORD");
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = "STARTDATE";
            }

            var ents = from r in dal.GetObjects()
                    select r;

            if (!string.IsNullOrEmpty(strDateFrom))
            {
                DateTime dtStart = new DateTime();
                DateTime.TryParse(strDateFrom, out dtStart);

                ents = ents.Where(v => v.STARTDATE >= dtStart);

            }

            if (!string.IsNullOrEmpty(strDateTo))
            {
                DateTime dtEnd = new DateTime();
                DateTime.TryParse(strDateTo, out dtEnd);
                dtEnd = dtEnd.AddDays(1).AddMinutes(-1);

                ents = ents.Where(v => v.ENDDATE <= dtEnd);
            }

            if (objArgs.Count() > 0 && !string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, objArgs.ToArray());
            }

            //零天的记录不再获取
            if (ents != null && ents.Count() > 0)
            {
                ents = ents.Where(v => v.TOTALDAYS > 0);
            }

            return ents.OrderBy(strOrderBy); 
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strDateFrom">出差起始日期</param>
        /// <param name="strDateTo">出差截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> EmployeeEvectionRecordPaging(string strOwnerID, string strEmployeeID, string strDateFrom,
            string strDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllEmployeeEvectionRecordByMultSearch(strOwnerID, strEmployeeID, strDateFrom, strDateTo, strSortKey);

            return Utility.Pager<T_HR_EMPLOYEEEVECTIONRECORD>(q, pageIndex, pageSize, ref pageCount);
        }



        /// <summary>
        /// 新增出差记录信息
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        public void EmployeeEvectionRecordADD(T_HR_EMPLOYEEEVECTIONRECORD entity)
        {
            try
            {
                //IQueryable<T_HR_ATTENDANCERECORD> entArs = from r in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                //                                           where r.EMPLOYEEID == entity.EMPLOYEEID && r.ATTENDANCEDATE >= entity.STARTDATE && r.ATTENDANCEDATE <= entity.ENDDATE
                //                                           select r;
                //if (entArs.Count() > 0)
                //{
                //    AttendanceRecordBLL bllAttendRecord = new AttendanceRecordBLL();
                //    EmployeeSignInRecordBLL bllSignInRd = new EmployeeSignInRecordBLL();

                //    foreach (T_HR_ATTENDANCERECORD item in entArs)
                //    {
                       //item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.OutOnDuty) + 1).ToString();
                       //bllAttendRecord.ModifyAttRd(item);

                //        string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                //        IQueryable<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = from a in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>().Include("T_HR_ATTENDANCERECORD")
                //                                                                 where a.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID == item.ATTENDANCERECORDID && a.ABNORMCATEGORY == strAbnormCategory
                //                                                                 select a;

                //        if (entAbnormRecords.Count() == 0)
                //        {
                //            continue;
                //        }

                //        bllSignInRd.ClearNoSignInRecord("T_HR_EMPLOYEEABNORMRECORD", item.EMPLOYEEID, entAbnormRecords);

                //        foreach (T_HR_EMPLOYEEABNORMRECORD entAbnormRecord in entAbnormRecords)
                //        {
                //            dal.DeleteFromContext(entAbnormRecord);
                //        }

                //        dal.SaveContextChanges();
                //    }
                //}

                Tracer.Debug("出差消除异常开始，请假开始时间:" + entity.STARTDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")
                      + " 结束时间：" + entity.ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                AbnormRecordBLL bll = new AbnormRecordBLL();

                string attState = (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString();
                bll.DealEmployeeAbnormRecord(entity.EMPLOYEEID, entity.STARTDATE.Value, entity.ENDDATE.Value, attState);

                Tracer.Debug("出差消除异常结束，请假开始时间:" + entity.STARTDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")
                  + " 结束时间：" + entity.ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                dal.Add(entity);            
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 修改出差记录信息
        /// </summary>
        /// <param name="entity">出差记录实体</param>
        public void EmployeeEvectionRecordUpdate(T_HR_EMPLOYEEEVECTIONRECORD entity)
        {
            try
            {
                var ent = dal.GetObjects().FirstOrDefault(s=>s.EVECTIONRECORDID==entity.EVECTIONRECORDID);
                if (ent != null)
                {
                    Utility.CloneEntity(entity, ent);
                    dal.Update(ent);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }
        /// <summary>
        /// 删除出差记录信息
        /// </summary>
        /// <param name="evectionRecordIDs">出差记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        public int EmployeeEvectionRecordDelete(string[] evectionRecordIDs)
        {
            try
            {
                foreach (var id in evectionRecordIDs)
                {
                    var ent = dal.GetObjects().FirstOrDefault(s => s.EVECTIONRECORDID == id);
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                return 0;
                Utility.SaveLog(ex.ToString());
            }
        }
        /// <summary>
        /// 根据出差记录ID获到出差记录信息
        /// </summary>
        /// <param name="strID">出差记录ID</param>
        /// <returns>返回出差记录实体</returns>
        public T_HR_EMPLOYEEEVECTIONRECORD GetEmployeeEvectionRecordByID(string strID)
        {
            return dal.GetObjects().FirstOrDefault(s => s.EVECTIONRECORDID == strID);
        }

        /// <summary>
        /// 根据员工ID和时间获取出差申请记录
        /// </summary>
        /// <param name="strEmployeeId"></param>
        /// <param name="dtTempDate"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEEVECTIONRECORD GetEmployeeEvectionRdByEmployeeIdAndDate(string strEmployeeId, DateTime? dtTempDate)
        {
            return dal.GetObjects().FirstOrDefault(s => s.EMPLOYEEID == strEmployeeId && s.STARTDATE <= dtTempDate && s.ENDDATE >= dtTempDate);
        }

        /// <summary>
        /// 批量新增出差记录
        /// </summary>
        /// <param name="entTempList"></param>
        public void AddEvectionRdList(List<T_HR_EMPLOYEEEVECTIONRECORD> entTempList)
        {
            try
            {
                EmployeeEvectionRecordBLL bll = new EmployeeEvectionRecordBLL();
                if (entTempList == null)
                {
                    return;
                }

                if (entTempList.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_EMPLOYEEEVECTIONRECORD entTemp in entTempList)
                {
                    bll.EmployeeEvectionRecordADD(entTemp);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }
    }
}
