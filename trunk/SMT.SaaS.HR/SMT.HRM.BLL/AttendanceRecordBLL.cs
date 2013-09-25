using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 根据员工ID和日期获取班次定义的前两段起止时间
    /// </summary>
    public class AttendanceRecordBLL : BaseBll<T_HR_ATTENDANCERECORD>
    {
        #region 获取数据
        /// <summary>
        /// 根据员工ID和日期获取班次定义的前两段起止时间
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public List<V_ATTENDANCERECORD> GetAttendanceRecordByEmployeeID(string employeeID, string strDate)
        {
            DateTime dtAttendanceDate = new DateTime();
            DateTime.TryParse(strDate, out dtAttendanceDate);
            var ent = dal.GetObjects().Include("T_HR_SHIFTDEFINE").FirstOrDefault(s => s.EMPLOYEEID == employeeID && s.ATTENDANCEDATE == dtAttendanceDate);
            List<V_ATTENDANCERECORD> attendanceRecord = new List<V_ATTENDANCERECORD>();
            if (ent != null)
            {
                V_ATTENDANCERECORD temp = new V_ATTENDANCERECORD();
                temp.STARTTIME = ent.T_HR_SHIFTDEFINE.FIRSTSTARTTIME;
                temp.STARTVALUE = 0;
                temp.ENDTIME = ent.T_HR_SHIFTDEFINE.FIRSTENDTIME;
                temp.ENDVALUE = 0.5;
                attendanceRecord.Add(temp);
                temp.STARTTIME = ent.T_HR_SHIFTDEFINE.SECONDSTARTTIME;
                temp.STARTVALUE = 0.5;
                temp.ENDTIME = ent.T_HR_SHIFTDEFINE.SECONDENDTIME;
                temp.ENDVALUE = 1;
                attendanceRecord.Add(temp);
            }
            return attendanceRecord;
        }

        /// <summary>
        /// 根据员工起止时间
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRecordByDate(DateTime dtStart, DateTime dtEnd)
        {
            var q = from a in dal.GetObjects().Include("T_HR_SHIFTDEFINE")
                    where a.ATTENDANCEDATE >= dtStart && a.ATTENDANCEDATE <= dtEnd
                    select a;
            return q;
        }

        /// <summary>
        /// 根据员工ID和时间区间获取一段时间内的员工考勤记录
        /// </summary>
        /// <param name="strCompanyID">员工所在公司ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤日期起始日</param>
        /// <param name="dtEnd">考勤日期截止日</param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRecordByEmployeeIDAndDate(string strCompanyID, string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            var q = from a in dal.GetObjects().Include("T_HR_SHIFTDEFINE")
                    where a.OWNERCOMPANYID == strCompanyID 
                    && a.EMPLOYEEID == strEmployeeID 
                    && a.ATTENDANCEDATE >= dtStart && a.ATTENDANCEDATE <= dtEnd
                    select a;
            return q;
        }

        /// <summary>
        /// 根据员工ID和时间区间获取一段时间内的员工考勤记录
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤日期起始日</param>
        /// <param name="dtEnd">考勤日期截止日</param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRecordByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            var q = from a in dal.GetObjects().Include("T_HR_SHIFTDEFINE")
                    where a.EMPLOYEEID == strEmployeeID && a.ATTENDANCEDATE >= dtStart && a.ATTENDANCEDATE <= dtEnd
                    select a;
            return q;
        }

        /// <summary>
        /// 日常打卡信息
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strOwnerID"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="strAttendDateFrom"></param>
        /// <param name="strAttendDateTo"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAllAttendanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strAttendDateFrom,
            string strAttendDateTo, string strSortKey)
        {
            AttendanceRecordDAL dalAttendanceRecord = new AttendanceRecordDAL();

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
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDANCERECORD");
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = "EMPLOYEEID";
            }

            var q = dalAttendanceRecord.GetAttendanceRdListByMultSearch(sType, sValue, strAttendDateFrom, strAttendDateTo, strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 获取员工日常打卡信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strAttendDateFrom">打卡搜寻起始日期</param>
        /// <param name="strAttendDateTo">打卡搜寻截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工日常打卡列表</returns>
        public IQueryable<T_HR_ATTENDANCERECORD> GetAttendanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strAttendDateFrom,
            string strAttendDateTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendanceRdListByMultSearch(sType, sValue, strOwnerID, strEmployeeID, strAttendDateFrom, strAttendDateTo, strSortKey);

            return Utility.Pager<T_HR_ATTENDANCERECORD>(q, pageIndex, pageSize, ref pageCount);
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增T_HR_ATTENDANCERECORD信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddAttRd(T_HR_ATTENDANCERECORD entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                //bool flag = false;
                //AttendanceRecordDAL dalAttendanceRecord = new AttendanceRecordDAL();
                //flag = dalAttendanceRecord.IsExistsRd(entTemp.EMPLOYEEID, entTemp.ATTENDANCEDATE);
                //if (flag)
                //{
                //    return "{ALREADYEXISTSRECORD}";
                //}
                //T_HR_ATTENDANCERECORD ent = new T_HR_ATTENDANCERECORD();
                //Utility.CloneEntity<T_HR_ATTENDANCERECORD>(entTemp, ent);
                //ent.T_HR_SHIFTDEFINEReference.EntityKey =
                //    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SHIFTDEFINE", "SHIFTDEFINEID", entTemp.T_HR_SHIFTDEFINE.SHIFTDEFINEID);
                //if (ent.T_HR_SHIFTDEFINE != null)
                //{
                //    Utility.RefreshEntity(ent);                
                //}
                //dal.Add(ent);
                dal.AddToContext(entTemp);
                dal.SaveContextChanges();
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改T_HR_ATTENDANCERECORD信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyAttRd(T_HR_ATTENDANCERECORD entTemp)
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

                strFilter.Append(" ATTENDANCERECORDID == @0");

                objArgs.Add(entTemp.ATTENDANCERECORDID);

                AttendanceRecordDAL dalAttendanceRecord = new AttendanceRecordDAL();
                flag = dalAttendanceRecord.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCERECORD entUpdate = dalAttendanceRecord.GetAttendanceRecordRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAttendanceRecord.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除T_HR_ATTENDANCERECORD信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteAttRd(string strAttendanceRecordId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAttendanceRecordId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ATTENDANCERECORDID == @0");

                objArgs.Add(strAttendanceRecordId);

                AttendanceRecordDAL dalAttendanceRecord = new AttendanceRecordDAL();
                flag = dalAttendanceRecord.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDANCERECORD entDel = dalAttendanceRecord.GetAttendanceRecordRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAttendanceRecord.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 批量删除指定员工的一段时间内未正式启用的考勤记录
        /// </summary>
        /// <param name="entEmployees">员工集合</param>
        /// <param name="dtStart">上班起始日期</param>
        /// <param name="dtEnd">上班截止日期</param>
        public void DeleteUnEffectiveRecordByDate(List<T_HR_EMPLOYEE> entEmployees, DateTime dtStart, DateTime dtEnd, ref DateTime dtRes)
        {
            if (entEmployees == null)
            {
                return;
            }

            if (entEmployees.Count() == 0)
            {
                return;
            }

            var q = from ac in dal.GetObjects()
                    join ec in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>() on ac.EMPLOYEEID equals ec.EMPLOYEEID
                    where ec.PUNCHDATE >= dtStart && ec.PUNCHDATE <= dtEnd && ec.EMPLOYEEID != string.Empty
                    orderby ac.ATTENDANCEDATE descending
                    select ac;

            if (q.Count() == 0)
            {
                return;
            }

            DateTime dtDelStart = q.FirstOrDefault().ATTENDANCEDATE.Value.AddDays(1);

            var dels = from ac in dal.GetObjects()
                       where ac.ATTENDANCEDATE >= dtDelStart
                       select ac;

            if (dels.Count() == 0)
            {
                return;
            }

            dtRes = dtDelStart;

            foreach (T_HR_EMPLOYEE item in entEmployees)
            {
                for (int i = 0; i < dels.Count(); i++)
                {
                    T_HR_ATTENDANCERECORD entAttRd = dels.ToList()[i] as T_HR_ATTENDANCERECORD;
                    if (item.EMPLOYEEID == entAttRd.EMPLOYEEID)
                    {
                        dal.DeleteFromContext(entAttRd);
                    }
                }
            }

            dal.SaveContextChanges();
        }

        /// <summary>
        /// 未正式启用的考勤记录
        /// </summary>
        /// <param name="entEmployees">员工集合</param>
        /// <param name="dtStart">上班起始日期</param>
        /// <param name="dtEnd">上班截止日期</param>
        public void DeleteUnEffectiveRecordByDate(T_HR_COMPANY entCompany, T_HR_EMPLOYEE entEmployee, DateTime dtStart, DateTime dtEnd, ref DateTime dtRes)
        {
            try
            {
                if (entEmployee == null)
                {
                    return;
                }

                string strEmployeeID = entEmployee.EMPLOYEEID;

                var dels = from ac in dal.GetObjects()
                           where ac.EMPLOYEEID == strEmployeeID && ac.OWNERCOMPANYID == entCompany.COMPANYID
                            && ac.ATTENDANCEDATE >= dtStart && ac.ATTENDANCEDATE <= dtEnd
                           orderby ac.ATTENDANCEDATE descending
                           select ac;

                if (dels.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_ATTENDANCERECORD entAttRd in dels)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(entAttRd.ATTENDANCESTATE)) continue;

                        Tracer.Debug("初始化考勤删除未启用的员工初始化考勤记录，员工姓名：" + entAttRd.EMPLOYEENAME + " 时间：" + entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd"));
                        dal.Delete(entAttRd);
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("删除员工" + entEmployee.EMPLOYEECNAME + "考勤初始化记录异常：" + " 考勤日期：" + entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd")
                            +" 异常原因：" +ex.ToString());
                        continue;
                    }
                }

                //dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                dtRes = dtEnd;
                Utility.SaveLog("删除员工考勤初始化记录出错，执行函数为:DeleteUnEffectiveRecordByDate, 出错原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 更新指定公司员工，指定月份的作息记录状态
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <param name="strCurMonth"></param>
        public void UpdateAttendRecordByEvectionAndLeaveRd(string strCompanyID, string strCurMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strCompanyID) || string.IsNullOrWhiteSpace(strCurMonth))
                {
                    return;
                }

                DateTime dtCheck = new DateTime();
                DateTime dtStart = new DateTime(), dtEnd = new DateTime();
                DateTime.TryParse(strCurMonth + "-1", out dtStart);
                if (dtStart <= dtCheck)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                var evs = from e in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                          where e.OWNERCOMPANYID == strCompanyID && e.ENDDATE >= dtStart && e.CHECKSTATE == "2"
                          select e.EMPLOYEEID;

                if (evs.Count() > 0)
                {
                    foreach (string strEmployeeID in evs)
                    {
                        UpdateAttendRecordByEmployeeIdWithEvectionRecord(strEmployeeID, strCurMonth);
                    }
                }

                var els = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                          where e.OWNERCOMPANYID == strCompanyID && e.ENDDATETIME >= dtStart && e.CHECKSTATE == "2"
                          select e.EMPLOYEEID;

                if (els.Count() > 0)
                {
                    foreach (string strEmployeeID in els)
                    {
                        UpdateAttendRecordByEmployeeIdWithLeaveRecord(strEmployeeID, strCurMonth);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 根据传入员工ID和月份(年-月：2011-6)，更新指定月份员工作息记录
        /// </summary>
        /// <param name="strEmployeeID"></param>
        public void UpdateAttendRecordByEmployeeIdWithEvectionRecord(string strEmployeeID, string strCurMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strEmployeeID) || string.IsNullOrWhiteSpace(strCurMonth))
                {
                    return;
                }

                DateTime dtCheck = new DateTime();
                DateTime dtStart = new DateTime(), dtEnd = new DateTime();
                DateTime.TryParse(strCurMonth + "-1", out dtStart);
                if (dtStart <= dtCheck)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                IQueryable<T_HR_ATTENDANCERECORD> entAttRds = GetAttendanceRecordByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd);

                EmployeeEvectionRecordBLL bllEvection = new EmployeeEvectionRecordBLL();
                IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> entEvectionRds = bllEvection.GetEmployeeEvectionRecordsByEmployeeIdAndDate(strEmployeeID, dtStart);

                if (entAttRds == null || entEvectionRds == null)
                {
                    return;
                }

                if (entAttRds.Count() == 0 || entEvectionRds.Count() == 0)
                {
                    return;
                }

                List<T_HR_EMPLOYEEEVECTIONRECORD> entCheckEvectionRds = entEvectionRds.ToList();
                List<T_HR_ATTENDANCERECORD> entCheckAttRds = new List<T_HR_ATTENDANCERECORD>();

                foreach (T_HR_ATTENDANCERECORD item in entAttRds)
                {
                    bool bIsEvection = false;
                    foreach (T_HR_EMPLOYEEEVECTIONRECORD checkItem in entCheckEvectionRds)
                    {
                        DateTime dtCheckStart = DateTime.Parse(checkItem.STARTDATE.Value.ToString("yyyy-MM-dd"));
                        DateTime dtCheckEnd = DateTime.Parse(checkItem.ENDDATE.Value.ToString("yyyy-MM-dd"));

                        if (dtCheckStart <= item.ATTENDANCEDATE && dtCheckEnd >= item.ATTENDANCEDATE)
                        {
                            bIsEvection = true;
                            break;
                        }
                    }

                    if (!bIsEvection)
                    {
                        continue;
                    }

                    if (item.ATTENDANCESTATE == (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString())
                    {
                        continue;
                    }

                    item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString();
                    ModifyAttRd(item);
                    entCheckAttRds.Add(item);
                }

                if (entCheckAttRds.Count() > 0)
                {
                    RemoveWrongSignRds(entCheckAttRds);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 根据传入员工ID和月份(年-月：2011-6)，更新指定月份员工作息记录
        /// </summary>
        /// <param name="strEmployeeID"></param>
        public void UpdateAttendRecordByEmployeeIdWithLeaveRecord(string strEmployeeID, string strCurMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strEmployeeID) || string.IsNullOrWhiteSpace(strCurMonth))
                {
                    return;
                }

                DateTime dtCheck = new DateTime();
                DateTime dtStart = new DateTime(), dtEnd = new DateTime();
                DateTime.TryParse(strCurMonth + "-1", out dtStart);
                if (dtStart <= dtCheck)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddSeconds(-1);

                IQueryable<T_HR_ATTENDANCERECORD> entAttRds = GetAttendanceRecordByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd);

                EmployeeLeaveRecordBLL bllLeave = new EmployeeLeaveRecordBLL();
                IQueryable<T_HR_EMPLOYEELEAVERECORD> entLeaveRds = bllLeave.GetEmployeeLeaveRdListByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd, "2");

                if (entAttRds == null || entLeaveRds == null)
                {
                    return;
                }

                if (entAttRds.Count() == 0 || entLeaveRds.Count() == 0)
                {
                    return;
                }

                List<T_HR_EMPLOYEELEAVERECORD> entCheckLeaveRds = entLeaveRds.ToList();
                List<T_HR_ATTENDANCERECORD> entCheckAttRds = new List<T_HR_ATTENDANCERECORD>();

                foreach (T_HR_ATTENDANCERECORD item in entAttRds)
                {
                    bool bIsLeave = false;
                    foreach (T_HR_EMPLOYEELEAVERECORD checkItem in entCheckLeaveRds)
                    {
                        DateTime dtCheckStart = DateTime.Parse(checkItem.STARTDATETIME.Value.ToString("yyyy-MM-dd"));
                        DateTime dtCheckEnd = DateTime.Parse(checkItem.ENDDATETIME.Value.ToString("yyyy-MM-dd"));

                        if (dtCheckStart <= item.ATTENDANCEDATE && dtCheckEnd >= item.ATTENDANCEDATE)
                        {
                            bIsLeave = true;
                            break;
                        }
                    }

                    if (!bIsLeave)
                    {
                        continue;
                    }

                    item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    ModifyAttRd(item);
                    entCheckAttRds.Add(item);
                }

                if (entCheckAttRds.Count() > 0)
                {
                    RemoveWrongSignRds(entCheckAttRds);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 针对补请假的情况，用于删除异常考勤
        /// </summary>
        /// <param name="entAttRds"></param>
        private void RemoveWrongSignRds(List<T_HR_ATTENDANCERECORD> entAttRds)
        {
            EmployeeSignInRecordBLL bllSignInRd = new EmployeeSignInRecordBLL();
            AbnormRecordBLL bllAbnormRd = new AbnormRecordBLL();

            string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();

            try
            {
                foreach (T_HR_ATTENDANCERECORD item in entAttRds)
                {
                    //获取请假当天所有异常考勤(针对补请假的情况，用于删除异常考勤)
                    List<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = (from a in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>().Include("T_HR_ATTENDANCERECORD")
                                                                             where a.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID == item.ATTENDANCERECORDID && a.ABNORMCATEGORY == strAbnormCategory
                                                                             select a).ToList();

                    if (entAbnormRecords.Count() == 0)
                    {
                        continue;
                    }

                    bllSignInRd.ClearNoSignInRecord("T_HR_EMPLOYEEABNORMRECORD", item.EMPLOYEEID, entAbnormRecords);

                    foreach (T_HR_EMPLOYEEABNORMRECORD entAbnormRecord in entAbnormRecords)
                    {
                        bllAbnormRd.Delete(entAbnormRecord);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 重新初始化考勤记录，强制删除未提交签卡记录，异常考勤，考勤记录，重新初始化考勤
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="objId"></param>
        /// <param name="dtStar"></param>
        /// <param name="dtEnd"></param>
        /// <param name="DealType">0处理免打卡人员，1处理打卡人员，2处理所有</param>
        /// <returns></returns>
        public string CompulsoryInitialization(string objType, string objId, DateTime dtStar, DateTime dtEnd,string DealType)
        {
            string strMsg = string.Empty;
            try
            {
                //List<T_HR_EMPLOYEE> employees = new List<T_HR_EMPLOYEE>();
                EmployeeBLL empbll = new EmployeeBLL();
                AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL();

                switch (objType)
                {
                    case "0":
                        var employees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                        join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on ent.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                                        join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                                        where ep.ISAGENCY == "0"//主岗位
                                        && ep.EDITSTATE == "1"//生效中
                                        && ep.CHECKSTATE == "2"//审核通过
                                        && p.COMPANYID == objId
                                        select ent;

                        if (employees.Count() > 0)
                        {
                            foreach (var emp in employees)
                            {
                                T_HR_ATTENDANCESOLUTIONASIGN entAttendanceSolution = bll.GetAttendanceSolutionAsignByEmployeeIDAndDate(emp.EMPLOYEEID, dtStar);

                                if (DealType == "0")//如果是处理需要打卡人员的记录，跳过免打卡记录员工
                                {

                                    if (entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE != (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())//
                                    {
                                        Tracer.Debug("" + emp.EMPLOYEECNAME + entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME
                                            + "," + entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE);
                                        continue;
                                    }
                                }
                                else if (DealType == "1")//如果是处理免打卡人员的记录，跳过打卡人员
                                {

                                    if (entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())//
                                    {
                                        Tracer.Debug("" + emp.EMPLOYEECNAME + entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME
                                              + "," + entAttendanceSolution.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE);

                                        continue;
                                    }
                                }
                                else
                                {
                                }
                                strMsg = "开始强制删除考勤初始化记录，员工姓名：" + emp.EMPLOYEECNAME;
                                var q = from ent in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                        where ent.EMPLOYEEID == emp.EMPLOYEEID
                                        && ent.ATTENDANCEDATE >= dtStar
                                        && ent.ATTENDANCEDATE <= dtEnd
                                        select ent;
                                if (q.Count() > 0)
                                {
                                    List<T_HR_ATTENDANCERECORD> attlist = q.ToList();
                                    RemoveWrongSignRds(attlist);
                                    foreach (var att in attlist)
                                    {
                                        dal.Delete(att);
                                        strMsg += "强制删除考勤初始化记录,员工姓名：" + att.EMPLOYEENAME
                                            + " 考勤日期：" + att.ATTENDANCEDATE.ToString()
                                            + " 考勤状态：" + att.ATTENDANCESTATE;

                                        Tracer.Debug("强制删除考勤初始化记录,员工姓名：" + att.EMPLOYEENAME
                                            + " 考勤日期：" + att.ATTENDANCEDATE.ToString()
                                            + " 考勤状态：" + att.ATTENDANCESTATE);
                                    }
                                }

                                bll.AsignAttendanceSolutionForEmployeeByDate(emp, dtStar);
                                strMsg += "强制删除考勤初始化记录，初始化考勤记录成功！,员工姓名：" + emp.EMPLOYEECNAME;
                                Tracer.Debug("强制删除考勤初始化记录，初始化考勤记录成功！,员工姓名：" + emp.EMPLOYEECNAME);
                            }
                        };
                        break;
                    case "4":
                        //employees.Add(empbll.GetEmployeeByID(objId));
                        break;
                }


            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }

            return strMsg;
        }
        #endregion
    }
}
