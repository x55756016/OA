using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;
namespace SMT.HRM.BLL
{
    public class EmployeeCancelLeaveBLL : BaseBll<T_HR_EMPLOYEECANCELLEAVE>, IOperate
    {
        #region 获取数据
        

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEECANCELLEAVE> EmployeeCancelLeavePaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }
                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEECANCELLEAVE");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("CANCELLEAVEID", "T_HR_EMPLOYEECANCELLEAVE", strOwnerID, ref strCheckState, ref filterString, ref paras);
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

            IQueryable<T_HR_EMPLOYEECANCELLEAVE> ents = dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEECANCELLEAVE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 根据员工ID，请假的起止时间查询请假记录
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEELEAVERECORD> GetEmployeeLeaveRdListByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState)
        {
            IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                                                        where e.EMPLOYEEID == strEmployeeID && e.STARTDATETIME >= dtStart && e.ENDDATETIME <= dtEnd && e.CHECKSTATE == strCheckState
                                                        select e;

            return ents;
        }

        /// <summary>
        /// 根据ID获取销假记录
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEECANCELLEAVE GetEmployeeCancelLeaveByID(string strID)
        {
            var ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD").Include("T_HR_EMPLOYEELEAVERECORD.T_HR_LEAVETYPESET")
                       where c.CANCELLEAVEID == strID
                       select c;
                        
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 根据请假记录ID,获取对应的销假记录
        /// </summary>
        /// <param name="strLeaveRecordID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEECANCELLEAVE> GetEmployeeLeaveRdListByLeaveRecordID(string strLeaveRecordID, string strCheckState)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strCheckState))
                {
                      var ent = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                           where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID
                           select c;
                      return ent;
                }
                var ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                           where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID && c.CHECKSTATE == strCheckState
                           select c;
                return ents;
                //return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据请假记录ID,获取全部的销假记录
        /// </summary>
        /// <param name="strLeaveRecordID">请假记录ID</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEECANCELLEAVE> GetEmployeeLeaveRdListsByLeaveRecordID(string strLeaveRecordID, string strCheckState)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strCheckState))
                {
                    var ent = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                              where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID && (c.CHECKSTATE == "2" || c.CHECKSTATE == "1")
                              select c;
                    return ent.Any() ? ent.ToList() : null;
                }
                var ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                           where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveRecordID && c.CHECKSTATE == strCheckState
                           select c;
                return ents.Any() ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return new List<T_HR_EMPLOYEECANCELLEAVE>();
            }
        }

        /// <summary>
        /// 获取指定员工的实际销假天数(实际销假天数=销假天数-公休假天数-每周休息天数)，实际销假时长(按小时计，实际销假合计时长=非整天销假时长-当日作息间隙休息时间+整天销假时长)
        /// </summary>
        /// <param name="strCancelLeaveId">当前销假记录的ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStartTime">销假起始时间</param>
        /// <param name="dtLeaveEndTime">销假截止时间</param>
        /// <param name="dLeaveDay">实际销假天数</param>
        /// <param name="dLeaveTime">实际销假时长</param>
        /// <param name="dLeaveTotalTime">实际销假合计时长</param>
        public string GetRealCancelLeaveDayByEmployeeIdAndDate(string strCancelLeaveId, string strEmployeeID, DateTime dtCancelLeaveStartTime, DateTime dtCancelLeaveEndTime, ref decimal dCancelLeaveDay, ref decimal dCancelLeaveTime, ref decimal dCancelLeaveTotalTime)
        {
            string strMsg = string.Empty;
            try
            {
                T_HR_EMPLOYEECANCELLEAVE entCancelLeaveRecord = GetEmployeeCancelLeaveByID(strCancelLeaveId);
                bool flag = false;

                if (entCancelLeaveRecord != null)
                {
                    if (entCancelLeaveRecord.STARTDATETIME == dtCancelLeaveStartTime && entCancelLeaveRecord.ENDDATETIME == dtCancelLeaveEndTime)
                    {
                        if (entCancelLeaveRecord.LEAVEDAYS == null)
                        {
                            dCancelLeaveDay = 0;
                        }
                        else
                        {
                            dCancelLeaveDay = entCancelLeaveRecord.LEAVEDAYS.Value;
                        }

                        if (entCancelLeaveRecord.LEAVEHOURS == null)
                        {
                            dCancelLeaveTime = 0;
                        }
                        else
                        {
                            dCancelLeaveTime = entCancelLeaveRecord.LEAVEHOURS.Value;
                        }

                        if (entCancelLeaveRecord.TOTALHOURS == null)
                        {
                            dCancelLeaveTotalTime = 0;
                        }
                        else
                        {
                            dCancelLeaveTotalTime = entCancelLeaveRecord.TOTALHOURS.Value;
                        }

                        flag = true;
                    }
                }

                if (flag)
                {
                    return strMsg;
                }

                DateTime dtStart, dtEnd = new DateTime();
                decimal dTotalLeaveDay = 0;                         //起止时间的时长

                DateTime.TryParse(dtCancelLeaveStartTime.ToString("yyyy-MM-dd"), out dtStart);        //获取销假起始日期
                DateTime.TryParse(dtCancelLeaveEndTime.ToString("yyyy-MM-dd"), out dtEnd);            //获取销假截止日期

                AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtStart);
                if (entAttendSolAsign == null)
                {
                    //当前员工没有分配考勤方案，无法提交销假申请
                    return "{NONEXISTASIGNEDATTENSOL}";
                }

                //获取考勤方案
                T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;
                decimal dWorkTimePerDay = entAttendSol.WORKTIMEPERDAY.Value;
                decimal dWorkMode = entAttendSol.WORKMODE.Value;
                int iWorkMode = 0;
                int.TryParse(dWorkMode.ToString(), out iWorkMode);//获取工作制(工作天数/周)

                List<int> iWorkDays = new List<int>();
                Utility.GetWorkDays(iWorkMode, ref iWorkDays);//获取每周上班天数

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(strEmployeeID);

                string strVacDayType = (Convert.ToInt32(Common.OutPlanDaysType.Vacation) + 1).ToString();
                string strWorkDayType = (Convert.ToInt32(Common.OutPlanDaysType.WorkDay) + 1).ToString();
                IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);
                //IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE >= dtStart && s.ENDDATE <= dtEnd);
                // 销假时间要在开始时间和结束时间之间
                IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE <= dtStart && s.ENDDATE >= dtEnd);
                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(entAttendSol.ATTENDANCESOLUTIONID);
                T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                TimeSpan ts = dtEnd.Subtract(dtStart);

                decimal dVacDay = 0, dWorkDay = 0;
                decimal dLeaveFirstDayTime = 0, dLeaveLastDayTime = 0, dLeaveFirstLastTime = 0;//请假第一天的时长，请假最后一天的时长，请假首尾两天合计时长
                if (dtCancelLeaveStartTime != dtCancelLeaveEndTime)
                {
                    EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
                    bllLeaveRecord.CalculateNonWholeDayLeaveTime(dtCancelLeaveStartTime, dtStart, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "S", ref dLeaveFirstDayTime);
                    bllLeaveRecord.CalculateNonWholeDayLeaveTime(dtCancelLeaveEndTime, dtEnd, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "E", ref dLeaveLastDayTime);

                    dLeaveFirstLastTime = dLeaveFirstDayTime + dLeaveLastDayTime;

                    if (dtStart == dtEnd)
                    {
                        dLeaveFirstLastTime = dLeaveFirstLastTime - dWorkTimePerDay * 60;
                    }
                }
                else
                {
                    dLeaveFirstLastTime = dWorkTimePerDay * 60;
                }


                dTotalLeaveDay = ts.Days;
                if (ts.Days > 0)
                {
                    //取得总的请假天数(此天数扣除了首尾两天的时间,根据请假的情况，可能包含了公休假及周假天数,扣除首尾两天的计算只适合请三天以上的)
                    int iDays = ts.Days - 1;
                    dTotalLeaveDay = iDays;

                    for (int i = 0; i < iDays; i++)
                    {
                        int j = i + 1;
                        bool isVacDay = false;
                        DateTime dtCurDate = dtStart.AddDays(j);
                        if (iWorkDays.Contains(Convert.ToInt32(dtCurDate.DayOfWeek)) == false)
                        {
                            dVacDay += 1;
                        }

                        if (entVacDays.Count() > 0)
                        {
                            foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                            {
                                if (item_Vac.STARTDATE.Value <= dtCurDate && item_Vac.ENDDATE >= dtCurDate)
                                {
                                    isVacDay = true;
                                    break;
                                }
                            }
                        }

                        if (isVacDay)
                        {
                            dVacDay += 1;
                        }

                        if (entWorkDays.Count() > 0)
                        {
                            foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                            {
                                if (item_Work.STARTDATE.Value <= dtCurDate && item_Work.ENDDATE >= dtCurDate)
                                {
                                    dWorkDay += 1;
                                    break;
                                }
                            }
                        }
                    }
                }

                dCancelLeaveDay = dTotalLeaveDay - dVacDay + dWorkDay;    //请假天数 = 请假天数-首尾两天 - 总休假天数 + 休假调剂工作天数
                decimal dTempTime = decimal.Round((dLeaveFirstLastTime) / 60, 1);
                if (dTempTime >= dWorkTimePerDay)
                {
                    decimal dTempDay = decimal.Round(dTempTime / dWorkTimePerDay, 2);
                    string[] strList = dTempDay.ToString().Split('.');
                    if (strList.Length == 2)
                    {
                        dCancelLeaveDay += decimal.Parse(strList[0].ToString());
                        dCancelLeaveTime = dTempTime - dWorkTimePerDay * decimal.Parse(strList[0].ToString());
                    }
                    else
                    {
                        dCancelLeaveDay += dTempDay;
                    }
                }
                else if (dTempTime < dWorkTimePerDay)
                {
                    dCancelLeaveTime = dTempTime;
                }

                dCancelLeaveTotalTime = dCancelLeaveDay * dWorkTimePerDay + dCancelLeaveTime;

            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }
        #endregion

        #region 操作
        
        /// <summary>
        /// 添加销假记录
        /// </summary>
        /// <param name="obj">销假记录实体</param>
        /// <returns></returns>
        public string EmployeeCancelLeaveAdd(T_HR_EMPLOYEECANCELLEAVE obj)
        {
            string strMsg = string.Empty;
            try
            {
                string strLeaveId = string.Empty;
                if (obj.T_HR_EMPLOYEELEAVERECORD == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                strLeaveId = obj.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;

                if (string.IsNullOrWhiteSpace(strLeaveId))
                {
                    return "{REQUIREDFIELDS}";
                }

                var ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                           where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveId && (c.CHECKSTATE == "1" || c.CHECKSTATE == "2")
                           select c;

                if (ents.Any())
                {
                    //请假对于的销假记录，如果此次销假的时间段之前销过假，那么则不能保存
                    foreach (var item in ents)
                    {
                        if (obj.STARTDATETIME <= item.STARTDATETIME)
                        {
                            if (obj.ENDDATETIME >= item.STARTDATETIME && obj.ENDDATETIME <= item.ENDDATETIME)
                            {
                                return "{LEAVERECORDISCANCELED}";
                            }
                            if (obj.ENDDATETIME >= item.ENDDATETIME)
                            {
                                return "{LEAVERECORDISCANCELED}";
                            }
                        }
                        if (obj.STARTDATETIME >= item.STARTDATETIME && obj.STARTDATETIME < item.ENDDATETIME)
                        {
                            return "{LEAVERECORDISCANCELED}";
                        }
                    }
                }
                //添加请假记录
                T_HR_EMPLOYEECANCELLEAVE ent = new T_HR_EMPLOYEECANCELLEAVE();
                Utility.CloneEntity(obj, ent);
                ent.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", obj.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                //dal.GetObjects<AddObject("T_HR_EMPLOYEELEAVERECORD", ent);
                //添加请假调休记录
                dal.AddToContext(ent);
                dal.SaveContextChanges();
                SaveMyRecord(ent);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                strMsg = "{ADDERROR}";
            }
            return strMsg;
        }
        /// <summary>
        /// 修改销假记录
        /// </summary>
        /// <param name="LeaveRecord"></param>
        /// <param name="AdjustLeave"></param>
        public string EmployeeCancelLeaveUpdate(T_HR_EMPLOYEECANCELLEAVE obj)
        {
            string strMsg = string.Empty;
            try
            {
                string strLeaveId = string.Empty;
                if (obj.T_HR_EMPLOYEELEAVERECORD == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                strLeaveId = obj.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;

                if (string.IsNullOrWhiteSpace(strLeaveId))
                {
                    return "{REQUIREDFIELDS}";
                }

                //Modified by : Sam
                //Date:2011-9-28
                //For:修改的时候销假记录肯定已经存在
                //Date:2013-9-29
                var ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEELEAVERECORD")
                           where c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strLeaveId && (c.CHECKSTATE == "1" || c.CHECKSTATE == "2" )
                           select c;
                if (ents.Any())
                {
                    //请假对于的销假记录，如果此次销假的时间段之前销过假，那么则不能保存
                    foreach (var item in ents)
                    {
                        if (obj.STARTDATETIME <= item.STARTDATETIME)
                        {
                            if (obj.ENDDATETIME >= item.STARTDATETIME && obj.ENDDATETIME <= item.ENDDATETIME)
                            {
                                return "{LEAVERECORDISCANCELED}";
                            }
                            if (obj.ENDDATETIME >= item.ENDDATETIME)
                            {
                                return "{LEAVERECORDISCANCELED}";
                            }
                        }
                        if (obj.STARTDATETIME >= item.STARTDATETIME && obj.STARTDATETIME <= item.ENDDATETIME)
                        {
                            return "{LEAVERECORDISCANCELED}";
                        }
                    }
                }
                //if (ents.Count() > 0)
                //{
                //    return "{LEAVERECORDISCANCELED}";
                //}

                //修改记录
                var ent = dal.GetObjects().FirstOrDefault(s => s.CANCELLEAVEID == obj.CANCELLEAVEID);
                if (ent != null)
                {
                    Utility.CloneEntity(obj, ent);
                    if (obj.T_HR_EMPLOYEELEAVERECORD != null)
                    {
                        ent.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", obj.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                    }
                }

                dal.UpdateFromContext(ent);
                dal.SaveContextChanges();
                SaveMyRecord(ent);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                strMsg = "{ADDERROR}";
            }
            return strMsg;
        }

        /// <summary>
        /// 删除请假记录组
        /// </summary>
        /// <param name="leaveRecordIDs">请假记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        public int EmployeeCancelLeaveDelete(string[] cancelLeaveIDs)
        {
            try
            {
                foreach (var id in cancelLeaveIDs)
                {
                    //先删除请假调休记录,再删除请假记录
                    var ent = dal.GetObjects().FirstOrDefault(s => s.CANCELLEAVEID == id);
                    if (ent != null)
                    {
                        //dal.DeleteFromContext(ent);
                        Delete(ent);
                    }

                    //根据id删除代办信息
                    //EngineWcfGlobalFunctionClient wcfClient=new EngineWcfGlobalFunctionClient();
                    //wcfClient.DeleteTrigger(id);
                }
                return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return 0;
            }
        }
        #endregion

        /// <summary>
        /// 引擎服务
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
                string strMsg = string.Empty;
                var ents = (from ent in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                           where ent.CANCELLEAVEID == EntityKeyValue
                           select new 
                           {
                               EMPLOYEEENTRY = ent
                           }).FirstOrDefault();

                if (ents!=null)
                {
                    //销假信息
                    var employeeEntry = ents.EMPLOYEEENTRY;
                    //审核状态
                    employeeEntry.CHECKSTATE = CheckState;
                    //修改时间
                    employeeEntry.UPDATEDATE = DateTime.Now;
                    //更新员工销假申请表
                    dal.UpdateFromContext(employeeEntry);
                    i = dal.SaveContextChanges();
                    //strMsg = EmployeeCancelLeaveUpdate(ents.FirstOrDefault());

                    if (i > 0)
                    {
                        SMT.Foundation.Log.Tracer.Debug("员工销假开始调用我的单据服务");
                        SaveMyRecord(employeeEntry);
                    }
                }
                //if (strMsg == "{SAVESUCCESSED}")
                //{
                //    i = 1;
                //}
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
