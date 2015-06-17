using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using TM_SaaS_OA_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;
using SMT.HRM.CustomModel.Request;
using SMT.HRM.CustomModel.Response;
using SMT.HRM.CustomModel.Common;
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
                //注释掉没考虑某一区间的情况
                //IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE <= dtStart && s.ENDDATE >= dtEnd);
                //条件过滤有四种情况
                //1:在区间内
                //2：大于开始时间且结束时间小于销假结束时间
                //3：开始日期大于 销假开始日期且结束日期处于有效期之间
                //4：开始日期小于销假开始日期且结束日期在开始时间和结束时间
                IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType 
                                                            && ((s.STARTDATE <= dtStart && s.ENDDATE >= dtEnd) 
                                                            || (s.STARTDATE >= dtStart && s.ENDDATE <=dtEnd)
                                                            || (s.STARTDATE >= dtStart && s.STARTDATE <=dtEnd && s.ENDDATE >=dtEnd)
                                                            || (s.STARTDATE <= dtStart && s.ENDDATE >=dtStart && s.ENDDATE <= dtEnd)));
                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(entAttendSol.ATTENDANCESOLUTIONID);
                T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                TimeSpan ts = dtEnd.Subtract(dtStart);

                decimal dVacDay = 0, dWorkDay = 0;
                decimal dLeaveFirstDayTime = 0, dLeaveLastDayTime = 0, dLeaveFirstLastTime = 0;//请假第一天的时长，请假最后一天的时长，请假首尾两天合计时长
                if (dtCancelLeaveStartTime != dtCancelLeaveEndTime)
                {
                    EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
                    bllLeaveRecord.CalculateNonWholeDayLeaveTime(dtCancelLeaveStartTime, dtStart, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "S", ref dLeaveFirstDayTime, dtCancelLeaveEndTime);
                    bllLeaveRecord.CalculateNonWholeDayLeaveTime(dtCancelLeaveEndTime, dtEnd, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "E", ref dLeaveLastDayTime, dtCancelLeaveStartTime);

                    dLeaveFirstLastTime = dLeaveFirstDayTime + dLeaveLastDayTime;

                    if (dtStart == dtEnd)
                    {
                        //dLeaveFirstLastTime = dLeaveFirstLastTime - dWorkTimePerDay * 60;
                        if (dLeaveFirstLastTime > dWorkTimePerDay * 60)
                        {
                            dLeaveFirstLastTime = dLeaveFirstLastTime - dWorkTimePerDay * 60;
                        } 
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
                int intResult = 0;
                foreach (var id in cancelLeaveIDs)
                {
                    //先删除请假调休记录,再删除请假记录
                    var ent = dal.GetObjects().FirstOrDefault(s => s.CANCELLEAVEID == id);
                    if (ent != null)
                    {
                        //dal.DeleteFromContext(ent);
                        bool bl =Delete(ent);
                        if (bl)
                        {
                            intResult = 1;
                        }
                    }

                    //根据id删除代办信息
                    //EngineWcfGlobalFunctionClient wcfClient=new EngineWcfGlobalFunctionClient();
                    //wcfClient.DeleteTrigger(id);
                }
                return intResult;
                //return dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Utility.SaveLog("EmployeeCancelLeaveDelete删除失败：" + ex.ToString());
                return -1;
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
                        if (CheckState == "2")
                        {
                            SMT.Foundation.Log.Tracer.Debug("开始调用销假记录FormID:" + EntityKeyValue);
                            Cancelleave(EntityKeyValue);
                        }
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

        public int Cancelleave(string cancelID)
        {
            int intResult = 0;
            try
            {
                var entCancel = from ent in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                                where ent.CANCELLEAVEID == cancelID
                                select ent;
                T_HR_EMPLOYEELEAVERECORD leave = new T_HR_EMPLOYEELEAVERECORD();
                if (entCancel.Count() > 0)
                {
                    T_HR_EMPLOYEECANCELLEAVE cancelLeave = entCancel.FirstOrDefault();
                    
                    var entLeave = from ent in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                                   where ent.LEAVERECORDID == cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID
                                   select ent;
                    if (entLeave.Count() > 0)
                    {
                        leave = entLeave.FirstOrDefault();                        
                    }
                    if (leave.T_HR_LEAVETYPESET.LEAVETYPEVALUE != (Convert.ToInt32(Common.LeaveTypeValue.AffairLeave) + 1).ToString())
                    {
                        //调休假
                        var entRots = from ent in dal.GetObjects<T_HR_LEAVEREFEROT>()
                                 where ent.LEAVE_RECORDID == leave.LEAVERECORDID
                                 && ent.EMPLOYEEID == cancelLeave.OWNERID     
                                 && ent.ACTION ==1
                                 select ent ;
                        //entRots = entRots.OrderByDescending(s => s.EXPIREDATE);
                        List<string> listCountIDs = new List<string>();
                        if (entRots.Count() > 0)
                        {
                            entRots.ToList().ForEach(s => {
                                listCountIDs.Add(s.OVERTIME_RECORDID);
                            });
                        }
                        var overTimes = from ent in dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>()
                                        where listCountIDs.Contains(ent.OVERTIMERECORDID)
                                        select ent;
                        var entDayCounts = from ent in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                                           where listCountIDs.Contains(ent.RECORDID)
                                           || listCountIDs.Contains(ent.REMARK)
                                           select ent;
                        entDayCounts = entDayCounts.OrderByDescending(s=>s.TERMINATEDATE);
                        decimal? cancelHours = cancelLeave.TOTALHOURS;
                        decimal? UseHours = cancelHours;
                        if (entDayCounts.Count() > 0)
                        {                          
                           foreach(var s in entDayCounts.ToList()) 
                           {
                                if (UseHours > 0)
                                {
                                    if (s.HOURS > s.LEFTHOURS)
                                    {
                                        //总时间-剩余时间
                                        decimal? plusHours = 0;
                                        plusHours = s.HOURS - s.LEFTHOURS;
                                        if (plusHours <= 0)
                                        {
                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS + "plusHours为：" + plusHours);
                                            continue;
                                        }
                                        if (UseHours > plusHours)
                                        {                                            
                                            UseHours = UseHours - plusHours;
                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);                                            
                                            s.LEFTHOURS += plusHours;
                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                            //修改加班记录的剩余小时数
                                            var singleOverTime = overTimes.Where(m=>m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                            if (singleOverTime.Count() > 0)
                                            {
                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                overtime.LEFTHOURS += plusHours;
                                                overtime.STATUS = 1;
                                                dal.UpdateFromContext(overtime);
                                            }
                                        }
                                        else
                                        {
                                            Tracer.Debug("销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);                                                                                        
                                            s.LEFTHOURS += UseHours;
                                            //修改加班记录的剩余小时数
                                            var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                            if (singleOverTime.Count() > 0)
                                            {
                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                overtime.LEFTHOURS += UseHours;
                                                overtime.STATUS = 1;
                                                dal.UpdateFromContext(overtime);
                                            }
                                            Tracer.Debug("销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                            //将时间设置为0
                                            UseHours = 0;
                                        }
                                    }
                                    if (s.LEFTHOURS > s.HOURS)
                                    {
                                        Tracer.Debug("销假后剩余时间比总时间大，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                        continue;
                                    }
                                    
                                    s.STATUS = 1;
                                    dal.UpdateFromContext(s);
                                    T_HR_LEAVEREFEROT rot = new T_HR_LEAVEREFEROT();
                                    rot.RECORDID = Guid.NewGuid().ToString();
                                    rot.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                    rot.ACTION = 2;
                                    rot.EFFECTDATE = s.EFFICDATE;
                                    rot.EXPIREDATE = s.TERMINATEDATE;
                                    rot.EMPLOYEEID = cancelLeave.OWNERID;
                                    rot.LEAVE_TOTAL_DAYS = s.DAYS;
                                    rot.LEAVE_TOTAL_HOURS = s.HOURS;
                                    rot.LEAVE_TYPE_SETID = s.LEAVETYPESETID;
                                    rot.LEAVE_APPLY_DATE = DateTime.Now;
                                    rot.OVERTIME_RECORDID = s.RECORDID;
                                    rot.STATUS = 1;
                                    dal.AddToContext(rot);
                                }
                            }
                        }

                    }
                    else
                    {
                        #region "   性别限制，冲减和扣款类型，假期冲减  "

                        string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
                        DateTime dtCur = DateTime.Parse(leave.CREATEDATE.Value.ToString("yyyy-MM-dd"));
                        AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
                        T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(leave.EMPLOYEEID, dtCur);

                        //获取考勤方案关联的假期标准(只为带薪假的)
                        AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                        IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                        T_HR_ATTENDFREELEAVE freeLtsEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == leave.T_HR_LEAVETYPESET.LEAVETYPESETID).OrderByDescending(t => t.CREATEDATE).FirstOrDefault();
                        T_HR_LEAVETYPESET ltsEntity = new T_HR_LEAVETYPESET(); // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
                        List<T_HR_LEAVETYPESET> ltsList = leaveTypeSetBll.GetLeaveTypeSetAll(leave.EMPLOYEEID);

                        if (freeLtsEntity != null)
                        {
                            ltsEntity = freeLtsEntity.T_HR_LEAVETYPESET; // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        }
                        else
                        {
                            ltsEntity = ltsList.Where(t => t.LEAVETYPESETID == leave.T_HR_LEAVETYPESET.LEAVETYPESETID).FirstOrDefault();
                        }
                        string strAdjustLeaveLeaveTypeValue = Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString();
                        string strAnnualLeaveLeaveTypeValue = Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString();
                        //T_HR_LEAVETYPESET ltsEntity = leaveTypeSetBll.GetLeaveTypeSetByID(request.LeaveTypeID);
                        //FineType: 0,不扣(带薪假) 1、扣款；2、调休+扣款；3、调休+带薪假抵扣+扣款；
                        string strFineType = string.Empty;//请假时，如果年假和调休有剩余时间就可以供其他假期抵扣
                        if (ltsEntity != null)
                        {
                            //避免手机端无法提供LeaveTpeValue，因此重新赋值
                            // request.LeaveTypeValue = string.IsNullOrEmpty(ltsEntity.LEAVETYPEVALUE) ? request.LeaveTypeValue : int.Parse(ltsEntity.LEAVETYPEVALUE);
                            //假期之间如何冲抵，例如：请事假的时候根据该状态来指示，是否可用年假或调休假抵扣
                            //航信要求:调休+带薪假+扣款，即请事假时将调休+带薪假用完后再扣款;
                            strFineType = string.IsNullOrEmpty(ltsEntity.FINETYPE) ? "0" : ltsEntity.FINETYPE;
                            //response.FineType = string.IsNullOrEmpty(ltsEntity.FINETYPE) ? "0" : ltsEntity.FINETYPE;
                            var daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == leave.EMPLOYEEID && t.VACATIONTYPE == leave.T_HR_LEAVETYPESET.LEAVETYPEVALUE);
                            List<T_HR_LEAVEREFEROT> refOTList = new List<T_HR_LEAVEREFEROT>();
                            var refotQueryable = dal.GetObjects<T_HR_LEAVEREFEROT>().Where(t => t.LEAVE_RECORDID == leave.LEAVERECORDID && t.ACTION == 1 && t.STATUS == 1);
                            EmployeeLevelDayCountBLL daycountBll = new EmployeeLevelDayCountBLL();
                            EmployeeLeaveRecordBLL leaveBll = new EmployeeLeaveRecordBLL();
                            if (refotQueryable != null)
                            {
                                refOTList = refotQueryable.ToList();
                            }
                            switch (strFineType)
                            {                         // 扣款性质
                                // 1 不扣，2 扣款，3 调休+扣款，4 调休+带薪假+扣款
                                case "1":
                                case "2":
                                    T_HR_LEAVEREFEROT rot = new T_HR_LEAVEREFEROT();
                                    rot.RECORDID = Guid.NewGuid().ToString();
                                    rot.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                    rot.ACTION = 2;
                                    rot.EFFECTDATE = cancelLeave.STARTDATETIME;
                                    rot.EXPIREDATE = cancelLeave.ENDDATETIME;
                                    rot.EMPLOYEEID = cancelLeave.OWNERID;
                                    rot.LEAVE_TOTAL_DAYS = cancelLeave.LEAVEDAYS;
                                    rot.LEAVE_TOTAL_HOURS = cancelLeave.TOTALHOURS;
                                    rot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                    rot.LEAVE_APPLY_DATE = DateTime.Now;
                                    rot.OVERTIME_RECORDID = "";
                                    rot.STATUS = 1;
                                    Tracer.Debug("销假为事假，只添加一条记录");
                                    dal.AddToContext(rot);
                                    break;
                                case "3":
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == leave.EMPLOYEEID
                                        && t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue);
                                    if (leave.FINEHOURS > 0)
                                    {
                                        decimal worktime = 0;
                                        decimal salaryHours = 0;
                                        salaryHours = (decimal)leave.FINEHOURS;

                                        if (entAttAsign.T_HR_ATTENDANCESOLUTION != null)
                                        {
                                            worktime = (decimal)entAttAsign.T_HR_ATTENDANCESOLUTION.WORKTIMEPERDAY;
                                        }
                                        if (cancelLeave.TOTALHOURS <= salaryHours)
                                        {
                                            //销假时间小于等于扣款小时数，则只处理扣款记录
                                            leave.FINEHOURS = leave.FINEHOURS - cancelLeave.TOTALHOURS;
                                            leave.FINEDAYS = leave.FINEDAYS - (cancelLeave.TOTALHOURS / worktime);
                                            //leaveBll.Update(leave);
                                            dal.UpdateFromContext(leave);
                                            T_HR_LEAVEREFEROT rot2 = new T_HR_LEAVEREFEROT();
                                            rot2.RECORDID = Guid.NewGuid().ToString();
                                            rot2.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                            rot2.ACTION = 2;
                                            rot2.EFFECTDATE = cancelLeave.STARTDATETIME;
                                            rot2.EXPIREDATE = cancelLeave.ENDDATETIME;
                                            rot2.EMPLOYEEID = cancelLeave.OWNERID;
                                            rot2.LEAVE_TOTAL_DAYS = cancelLeave.LEAVEDAYS;
                                            rot2.LEAVE_TOTAL_HOURS = cancelLeave.TOTALHOURS;
                                            rot2.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            rot2.LEAVE_APPLY_DATE = DateTime.Now;
                                            rot2.OVERTIME_RECORDID = "";
                                            rot2.STATUS = 1;
                                            Tracer.Debug("事假包含调休和扣款销假为事假，只消除了扣款的记录，只添加一条记录");
                                            dal.AddToContext(rot2);
                                        }
                                        else
                                        {
                                            //销假时间大于扣款小时数，先将扣款数置为0，再进行销假处理
                                            leave.FINEHOURS = 0;
                                            leave.FINEDAYS = 0;
                                            //leaveBll.Update(leave);
                                            dal.UpdateFromContext(leave);
                                            T_HR_LEAVEREFEROT rot2 = new T_HR_LEAVEREFEROT();
                                            rot2.RECORDID = Guid.NewGuid().ToString();
                                            rot2.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                            rot2.ACTION = 2;
                                            rot2.EFFECTDATE = cancelLeave.STARTDATETIME;
                                            rot2.EXPIREDATE = cancelLeave.ENDDATETIME;
                                            rot2.EMPLOYEEID = cancelLeave.OWNERID;
                                            rot2.LEAVE_TOTAL_DAYS = salaryHours/worktime;
                                            rot2.LEAVE_TOTAL_HOURS = salaryHours;
                                            rot2.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            rot2.LEAVE_APPLY_DATE = DateTime.Now;
                                            rot2.OVERTIME_RECORDID = "";
                                            rot2.STATUS = 1;
                                            Tracer.Debug("事假包含调休和扣款销假为事假，一部分为扣款小时数，一部分为调休假只添加一条记录");
                                            dal.AddToContext(rot2);
                                            decimal surplusHours = (decimal)(cancelLeave.TOTALHOURS - salaryHours);
                                            //调休假
                                            var entRots = from ent in dal.GetObjects<T_HR_LEAVEREFEROT>()
                                                          where ent.LEAVE_RECORDID == leave.LEAVERECORDID
                                                          && ent.EMPLOYEEID == cancelLeave.OWNERID
                                                          && ent.ACTION ==1
                                                          select ent;
                                            //entRots = entRots.OrderByDescending(s => s.EXPIREDATE);
                                            List<string> listCountIDs = new List<string>();
                                            if (entRots.Count() > 0)
                                            {
                                                entRots.ToList().ForEach(s =>
                                                {
                                                    listCountIDs.Add(s.OVERTIME_RECORDID);
                                                });
                                            }
                                            var overTimes = from ent in dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>()
                                                            where listCountIDs.Contains(ent.OVERTIMERECORDID)
                                                            select ent;
                                            var entDayCounts = from ent in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                                                               where listCountIDs.Contains(ent.RECORDID)
                                                               || listCountIDs.Contains(ent.REMARK)
                                                               select ent;
                                            entDayCounts = entDayCounts.OrderByDescending(s => s.TERMINATEDATE);
                                            decimal? cancelHours = surplusHours;//剩余的小时数
                                            decimal? UseHours = cancelHours;
                                            if (entDayCounts.Count() > 0)
                                            {
                                                foreach (var s in entDayCounts.ToList())
                                                {
                                                    if (UseHours > 0)
                                                    {
                                                        if (s.HOURS > s.LEFTHOURS)
                                                        {
                                                            //总时间-剩余时间
                                                            decimal? plusHours = 0;
                                                            plusHours = s.HOURS - s.LEFTHOURS;
                                                            if (plusHours <= 0)
                                                            {
                                                                Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS + "plusHours为：" + plusHours);
                                                                continue;
                                                            }
                                                            if (UseHours > plusHours)
                                                            {
                                                                UseHours = UseHours - plusHours;
                                                                Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                s.LEFTHOURS += plusHours;
                                                                Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                //修改加班记录的剩余小时数
                                                                var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                                if (singleOverTime.Count() > 0)
                                                                {
                                                                    T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                    overtime.LEFTHOURS += plusHours;
                                                                    overtime.STATUS = 1;
                                                                    dal.UpdateFromContext(overtime);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Tracer.Debug("事假包含调休和扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                s.LEFTHOURS += UseHours;
                                                                //修改加班记录的剩余小时数
                                                                var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                                if (singleOverTime.Count() > 0)
                                                                {
                                                                    T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                    overtime.LEFTHOURS += UseHours;
                                                                    overtime.STATUS = 1;
                                                                    dal.UpdateFromContext(overtime);
                                                                }
                                                                Tracer.Debug("事假包含调休和扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                //将时间设置为0
                                                                UseHours = 0;
                                                            }
                                                        }
                                                        if (s.LEFTHOURS > s.HOURS)
                                                        {
                                                            Tracer.Debug("事假包含调休和扣款销假后剩余时间比总时间大，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            continue;
                                                        }

                                                        s.STATUS = 1;
                                                        dal.UpdateFromContext(s);
                                                        T_HR_LEAVEREFEROT rot3 = new T_HR_LEAVEREFEROT();
                                                        rot3.RECORDID = Guid.NewGuid().ToString();
                                                        rot3.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                                        rot3.ACTION = 2;
                                                        rot3.EFFECTDATE = s.EFFICDATE;
                                                        rot3.EXPIREDATE = s.TERMINATEDATE;
                                                        rot3.EMPLOYEEID = cancelLeave.OWNERID;
                                                        rot3.LEAVE_TOTAL_DAYS = s.DAYS;
                                                        rot3.LEAVE_TOTAL_HOURS = s.HOURS;
                                                        rot3.LEAVE_TYPE_SETID = s.LEAVETYPESETID;
                                                        rot3.LEAVE_APPLY_DATE = DateTime.Now;
                                                        rot3.OVERTIME_RECORDID = s.RECORDID;
                                                        rot3.STATUS = 1;
                                                        dal.AddToContext(rot3);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //调休假
                                        var entRots = from ent in dal.GetObjects<T_HR_LEAVEREFEROT>()
                                                      where ent.LEAVE_RECORDID == leave.LEAVERECORDID
                                                      && ent.EMPLOYEEID == cancelLeave.OWNERID
                                                      && ent.ACTION ==1
                                                      select ent;
                                        //entRots = entRots.OrderByDescending(s => s.EXPIREDATE);
                                        List<string> listCountIDs = new List<string>();
                                        if (entRots.Count() > 0)
                                        {
                                            entRots.ToList().ForEach(s =>
                                            {
                                                listCountIDs.Add(s.OVERTIME_RECORDID);
                                            });
                                        }
                                        var overTimes = from ent in dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>()
                                                        where listCountIDs.Contains(ent.OVERTIMERECORDID)
                                                        select ent;
                                        var entDayCounts = from ent in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                                                           where listCountIDs.Contains(ent.RECORDID)
                                                           || listCountIDs.Contains(ent.REMARK)
                                                           select ent;
                                        entDayCounts = entDayCounts.OrderByDescending(s => s.TERMINATEDATE);
                                        decimal? cancelHours = cancelLeave.TOTALHOURS;
                                        decimal? UseHours = cancelHours;
                                        if (entDayCounts.Count() > 0)
                                        {
                                            foreach (var s in entDayCounts.ToList())
                                            {
                                                if (UseHours > 0)
                                                {
                                                    if (s.HOURS > s.LEFTHOURS)
                                                    {
                                                        //总时间-剩余时间
                                                        decimal? plusHours = 0;
                                                        plusHours = s.HOURS - s.LEFTHOURS;
                                                        if (plusHours <= 0)
                                                        {
                                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            Tracer.Debug("带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS + "plusHours为：" + plusHours);
                                                            continue;
                                                        }
                                                        if (UseHours > plusHours)
                                                        {
                                                            UseHours = UseHours - plusHours;
                                                            Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            s.LEFTHOURS += plusHours;
                                                            Tracer.Debug("事假包含调休和扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            //修改加班记录的剩余小时数
                                                            var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                            if (singleOverTime.Count() > 0)
                                                            {
                                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                overtime.LEFTHOURS += plusHours;
                                                                overtime.STATUS = 1;
                                                                dal.UpdateFromContext(overtime);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Tracer.Debug("事假包含调休和扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            s.LEFTHOURS += UseHours;
                                                            //修改加班记录的剩余小时数
                                                            var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                            if (singleOverTime.Count() > 0)
                                                            {
                                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                overtime.LEFTHOURS += UseHours;
                                                                overtime.STATUS = 1;
                                                                dal.UpdateFromContext(overtime);
                                                            }
                                                            Tracer.Debug("事假包含调休和扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            //将时间设置为0
                                                            UseHours = 0;
                                                        }
                                                    }
                                                    if (s.LEFTHOURS > s.HOURS)
                                                    {
                                                        Tracer.Debug("事假包含调休和扣款销假后剩余时间比总时间大，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                        continue;
                                                    }

                                                    s.STATUS = 1;
                                                    dal.UpdateFromContext(s);
                                                    T_HR_LEAVEREFEROT rot4 = new T_HR_LEAVEREFEROT();
                                                    rot4.RECORDID = Guid.NewGuid().ToString();
                                                    rot4.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                                    rot4.ACTION = 2;
                                                    rot4.EFFECTDATE = s.EFFICDATE;
                                                    rot4.EXPIREDATE = s.TERMINATEDATE;
                                                    rot4.EMPLOYEEID = cancelLeave.OWNERID;
                                                    rot4.LEAVE_TOTAL_DAYS = s.DAYS;
                                                    rot4.LEAVE_TOTAL_HOURS = s.HOURS;
                                                    rot4.LEAVE_TYPE_SETID = s.LEAVETYPESETID;
                                                    rot4.LEAVE_APPLY_DATE = DateTime.Now;
                                                    rot4.OVERTIME_RECORDID = s.RECORDID;
                                                    rot4.STATUS = 1;
                                                    dal.AddToContext(rot4);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "4":
                                    if (leave.FINEHOURS > 0)
                                    {
                                        decimal worktime = 0;
                                        decimal salaryHours = 0;
                                        salaryHours = (decimal)leave.FINEHOURS;

                                        if (entAttAsign.T_HR_ATTENDANCESOLUTION != null)
                                        {
                                            worktime = (decimal)entAttAsign.T_HR_ATTENDANCESOLUTION.WORKTIMEPERDAY;
                                        }
                                        if (cancelLeave.TOTALHOURS <= salaryHours)
                                        {
                                            //销假时间小于等于扣款小时数，则只处理扣款记录
                                            leave.FINEHOURS = leave.FINEHOURS - cancelLeave.TOTALHOURS;
                                            leave.FINEDAYS = leave.FINEDAYS - (cancelLeave.TOTALHOURS / worktime);
                                            dal.UpdateFromContext(leave);
                                            //leaveBll.Update(leave);
                                            T_HR_LEAVEREFEROT rot2 = new T_HR_LEAVEREFEROT();
                                            rot2.RECORDID = Guid.NewGuid().ToString();
                                            rot2.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                            rot2.ACTION = 2;
                                            rot2.EFFECTDATE = cancelLeave.STARTDATETIME;
                                            rot2.EXPIREDATE = cancelLeave.ENDDATETIME;
                                            rot2.EMPLOYEEID = cancelLeave.OWNERID;
                                            rot2.LEAVE_TOTAL_DAYS = cancelLeave.LEAVEDAYS;
                                            rot2.LEAVE_TOTAL_HOURS = cancelLeave.TOTALHOURS;
                                            rot2.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            rot2.LEAVE_APPLY_DATE = DateTime.Now;
                                            rot2.OVERTIME_RECORDID = "";
                                            rot2.STATUS = 1;
                                            dal.AddToContext(rot2);
                                        }
                                        else
                                        {
                                            //如果消了扣款后还是有剩余则消年假或调休假
                                            //销假时间大于扣款小时数，先将扣款数置为0，再进行销假处理
                                            leave.FINEHOURS = 0;
                                            leave.FINEDAYS = 0;
                                            //leaveBll.Update(leave);
                                            dal.UpdateFromContext(leave);
                                            T_HR_LEAVEREFEROT rot2 = new T_HR_LEAVEREFEROT();
                                            rot2.RECORDID = Guid.NewGuid().ToString();
                                            rot2.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                            rot2.ACTION = 2;
                                            rot2.EFFECTDATE = cancelLeave.STARTDATETIME;
                                            rot2.EXPIREDATE = cancelLeave.ENDDATETIME;
                                            rot2.EMPLOYEEID = cancelLeave.OWNERID;
                                            rot2.LEAVE_TOTAL_DAYS = cancelLeave.LEAVEDAYS;
                                            rot2.LEAVE_TOTAL_HOURS = cancelLeave.TOTALHOURS;
                                            rot2.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            rot2.LEAVE_APPLY_DATE = DateTime.Now;
                                            rot2.OVERTIME_RECORDID = "";
                                            rot2.STATUS = 1;
                                            //修改了请假记录中的扣款天数
                                            dal.AddToContext(rot2);
                                            decimal surplusHours = (decimal)(cancelLeave.TOTALHOURS - salaryHours);
                                            //调休假
                                            var entRots = from ent in dal.GetObjects<T_HR_LEAVEREFEROT>()
                                                          where ent.LEAVE_RECORDID == leave.LEAVERECORDID
                                                          && ent.EMPLOYEEID == cancelLeave.OWNERID
                                                          && ent.ACTION ==1
                                                          select ent;
                                            //entRots = entRots.OrderByDescending(s => s.EXPIREDATE);
                                            List<string> listCountIDs = new List<string>();
                                            if (entRots.Count() > 0)
                                            {
                                                entRots.ToList().ForEach(s =>
                                                {
                                                    listCountIDs.Add(s.OVERTIME_RECORDID);
                                                });
                                            }
                                            var overTimes = from ent in dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>()
                                                            where listCountIDs.Contains(ent.OVERTIMERECORDID)
                                                            select ent;
                                            var entDayCounts = from ent in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                                                               where listCountIDs.Contains(ent.RECORDID)
                                                               || listCountIDs.Contains(ent.REMARK)
                                                               select ent;
                                            entDayCounts = entDayCounts.OrderByDescending(s => s.TERMINATEDATE);
                                            decimal? cancelHours = surplusHours;//剩余的小时数
                                            decimal? UseHours = cancelHours;
                                            if (entDayCounts.Count() > 0)
                                            {
                                                foreach (var s in entDayCounts.ToList())
                                                {
                                                    if (UseHours > 0)
                                                    {
                                                        if (s.HOURS > s.LEFTHOURS)
                                                        {
                                                            //总时间-剩余时间
                                                            decimal? plusHours = 0;
                                                            plusHours = s.HOURS - s.LEFTHOURS;
                                                            if (plusHours <= 0)
                                                            {
                                                                Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS + "plusHours为：" + plusHours);
                                                                continue;
                                                            }
                                                            if (UseHours > plusHours)
                                                            {
                                                                UseHours = UseHours - plusHours;
                                                                Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                s.LEFTHOURS += plusHours;
                                                                Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                //修改加班记录的剩余小时数
                                                                var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                                if (singleOverTime.Count() > 0)
                                                                {
                                                                    T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                    overtime.LEFTHOURS += plusHours;
                                                                    overtime.STATUS = 1;
                                                                    dal.UpdateFromContext(overtime);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Tracer.Debug("事假包含调休年假扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                s.LEFTHOURS += UseHours;
                                                                //修改加班记录的剩余小时数
                                                                var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                                if (singleOverTime.Count() > 0)
                                                                {
                                                                    T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                    overtime.LEFTHOURS += UseHours;
                                                                    overtime.STATUS = 1;
                                                                    dal.UpdateFromContext(overtime);
                                                                }
                                                                Tracer.Debug("事假包含调休年假扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                                //将时间设置为0
                                                                UseHours = 0;
                                                            }
                                                        }
                                                        if (s.LEFTHOURS > s.HOURS)
                                                        {
                                                            Tracer.Debug("事假包含调休年假扣款销假后剩余时间比总时间大，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            continue;
                                                        }

                                                        s.STATUS = 1;
                                                        dal.UpdateFromContext(s);
                                                        T_HR_LEAVEREFEROT rot3 = new T_HR_LEAVEREFEROT();
                                                        rot3.RECORDID = Guid.NewGuid().ToString();
                                                        rot3.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                                        rot3.ACTION = 2;
                                                        rot3.EFFECTDATE = s.EFFICDATE;
                                                        rot3.EXPIREDATE = s.TERMINATEDATE;
                                                        rot3.EMPLOYEEID = cancelLeave.OWNERID;
                                                        rot3.LEAVE_TOTAL_DAYS = s.DAYS;
                                                        rot3.LEAVE_TOTAL_HOURS = s.HOURS;
                                                        rot3.LEAVE_TYPE_SETID = s.LEAVETYPESETID;
                                                        rot3.LEAVE_APPLY_DATE = DateTime.Now;
                                                        rot3.OVERTIME_RECORDID = s.RECORDID;
                                                        rot3.STATUS = 1;
                                                        dal.AddToContext(rot3);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //调休假
                                        var entRots = from ent in dal.GetObjects<T_HR_LEAVEREFEROT>()
                                                      where ent.LEAVE_RECORDID == leave.LEAVERECORDID
                                                      && ent.EMPLOYEEID == cancelLeave.OWNERID
                                                      && ent.ACTION == 1
                                                      select ent;
                                        //entRots = entRots.OrderByDescending(s => s.EXPIREDATE);
                                        List<string> listCountIDs = new List<string>();
                                        if (entRots.Count() > 0)
                                        {
                                            entRots.ToList().ForEach(s =>
                                            {
                                                listCountIDs.Add(s.OVERTIME_RECORDID);
                                            });
                                        }
                                        var overTimes = from ent in dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>()
                                                        where listCountIDs.Contains(ent.OVERTIMERECORDID)
                                                        select ent;
                                        var entDayCounts = from ent in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                                                           where listCountIDs.Contains(ent.RECORDID)
                                                           || listCountIDs.Contains(ent.REMARK)
                                                           select ent;
                                        entDayCounts = entDayCounts.OrderByDescending(s => s.TERMINATEDATE);
                                        decimal? cancelHours = cancelLeave.TOTALHOURS;
                                        decimal? UseHours = cancelHours;
                                        if (entDayCounts.Count() > 0)
                                        {
                                            foreach (var s in entDayCounts.ToList())
                                            {
                                                if (UseHours > 0)
                                                {
                                                    if (s.HOURS > s.LEFTHOURS)
                                                    {
                                                        //总时间-剩余时间
                                                        decimal? plusHours = 0;
                                                        plusHours = s.HOURS - s.LEFTHOURS;
                                                        if (plusHours <= 0)
                                                        {
                                                            Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS + "plusHours为：" + plusHours);
                                                            continue;
                                                        }
                                                        if (UseHours > plusHours)
                                                        {
                                                            UseHours = UseHours - plusHours;
                                                            Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            s.LEFTHOURS += plusHours;
                                                            Tracer.Debug("事假包含调休年假扣款带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            //修改加班记录的剩余小时数
                                                            var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                            if (singleOverTime.Count() > 0)
                                                            {
                                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                overtime.LEFTHOURS += plusHours;
                                                                overtime.STATUS = 1;
                                                                dal.UpdateFromContext(overtime);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Tracer.Debug("事假包含调休年假扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            s.LEFTHOURS += UseHours;
                                                            //修改加班记录的剩余小时数
                                                            var singleOverTime = overTimes.Where(m => m.OVERTIMERECORDID == s.RECORDID || m.OVERTIMERECORDID == s.REMARK);
                                                            if (singleOverTime.Count() > 0)
                                                            {
                                                                T_HR_EMPLOYEEOVERTIMERECORD overtime = singleOverTime.FirstOrDefault();
                                                                overtime.LEFTHOURS += UseHours;
                                                                overtime.STATUS = 1;
                                                                dal.UpdateFromContext(overtime);
                                                            }
                                                            Tracer.Debug("事假包含调休年假扣款销假时间比差值小，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                            //将时间设置为0
                                                            UseHours = 0;
                                                        }
                                                    }
                                                    if (s.LEFTHOURS > s.HOURS)
                                                    {
                                                        Tracer.Debug("事假包含调休年假扣款销假后剩余时间比总时间大，带薪假中ID:" + s.RECORDID + "HOURS:时间为:" + s.HOURS + "修改后的剩余时间为LEFTHOURS：" + s.LEFTHOURS);
                                                        continue;
                                                    }

                                                    s.STATUS = 1;
                                                    dal.UpdateFromContext(s);
                                                    T_HR_LEAVEREFEROT rot4 = new T_HR_LEAVEREFEROT();
                                                    rot4.RECORDID = Guid.NewGuid().ToString();
                                                    rot4.LEAVE_RECORDID = cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID;
                                                    rot4.ACTION = 2;
                                                    rot4.EFFECTDATE = s.EFFICDATE;
                                                    rot4.EXPIREDATE = s.TERMINATEDATE;
                                                    rot4.EMPLOYEEID = cancelLeave.OWNERID;
                                                    rot4.LEAVE_TOTAL_DAYS = s.DAYS;
                                                    rot4.LEAVE_TOTAL_HOURS = s.HOURS;
                                                    rot4.LEAVE_TYPE_SETID = s.LEAVETYPESETID;
                                                    rot4.LEAVE_APPLY_DATE = DateTime.Now;
                                                    rot4.OVERTIME_RECORDID = s.RECORDID;
                                                    rot4.STATUS = 1;
                                                    dal.AddToContext(rot4);
                                                }
                                            }
                                        }
                                    }

                                    break;
                            }
                        }
                        #endregion "   性别限制，冲减和扣款类型，假期冲减  "
                        //事假不会再t_hr_employeeLeaveDayCount产生记录
                        
                    }
                }
                int i = dal.SaveContextChanges();
                if (i > 0)
                {
                    intResult = 1;
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("销假申请终审通过后更新记录失败");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + cancelID + " UpdateCheckState:" + ex.Message);
            }
            return intResult;
        }

        #region 获取元数据
        public string GetXmlString(string Formid)
        {
            string strReturn = string.Empty;
            Tracer.Debug("进入GetXmlString函数-表单ID:" + Formid);
            try
            {
                T_HR_EMPLOYEECANCELLEAVE Info = dal.GetObjects().Where(t => t.CANCELLEAVEID == Formid).FirstOrDefault();
                
                decimal? stateValue = Convert.ToDecimal("1");
                string checkState = string.Empty;
                string checkStateDict
                    = sysDicbll.GetSysDictionaryByCategoryArray(new string[] { "CHECKSTATE" }).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
                checkState = checkStateDict == null ? "" : checkStateDict;

                V_EMPLOYEEPOST employee = new V_EMPLOYEEPOST();
                using (EmployeeBLL bll = new EmployeeBLL())
                {
                    employee = bll.GetEmployeeDetailByID(Info.EMPLOYEEID);
                }
                decimal? postlevelValue = Convert.ToDecimal(employee.EMPLOYEEPOSTS[0].POSTLEVEL.ToString());
                string postLevelName = string.Empty;
                string postLevelDict
                     = sysDicbll.GetSysDictionaryByCategoryArray(new string[] { "CHECKSTATE" }).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
                postLevelName = postLevelDict == null ? "" : postLevelDict;

                SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
                List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "CURRENTEMPLOYEENAME", employee.T_HR_EMPLOYEE.EMPLOYEECNAME, employee.T_HR_EMPLOYEE.EMPLOYEECNAME));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "CHECKSTATE", Info.CHECKSTATE, checkState));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "POSTLEVEL", employee.EMPLOYEEPOSTS[0].POSTLEVEL.ToString(), postLevelName));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "EMPLOYEENAM", Info.EMPLOYEENAME, Info.EMPLOYEENAME));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERCOMPANYID", Info.OWNERCOMPANYID, Info.EMPLOYEENAME));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, Info.OWNERDEPARTMENTID));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERPOSTID", Info.OWNERPOSTID, Info.OWNERPOSTID));
                AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "ENTITYKEY", Info.CANCELLEAVEID, string.Empty));
                Tracer.Debug("进入GetXmlString函数-GetBusinessObject:" + Formid);
                string StrSource = GetBusinessObject("T_HR_EMPLOYEECANCELLEAVE");
                Tracer.Debug("获取销假申请的元数据模板为：" + StrSource);
                strReturn = mx.TableToXml(Info, null, StrSource, AutoList);
                Tracer.Debug("组合销假申请后的元数据为：" + strReturn);
            }
            catch (Exception ex)
            {
                Tracer.Debug("获取员工销假的元数据模板出现错误：" + ex.ToString());
            }
            return strReturn;
        }
        #endregion

        #region "   周文斌添加，计算销假的天数时长     "
        /// <summary>
        /// 计算销假时长
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CalculateLeaveCancelResponse CalculateLeaveCancelHours(CalculateLeaveCancelRequest request)
        {
            CalculateLeaveCancelResponse response = new CalculateLeaveCancelResponse();

            #region "   基本数据设置  "
            //避免手机端无法提供LeaveTypeValue值，因此根据LeaveTypeSetID取出
            LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
            T_HR_LEAVETYPESET ltsEntity = leaveTypeSetBll.GetLeaveTypeSetByID(request.LeaveTypeID);

            DateTime dtStartDate = Convert.ToDateTime(request.StartDate + " " + request.StartTime);
            DateTime dtEndDate = Convert.ToDateTime(request.EndDate + " " + request.EndTime);

            DateTime FirstCardStartDate = DateTime.MinValue;
            DateTime FirstCardEndDate = DateTime.MinValue;
            DateTime SecondCardStartDate = DateTime.MinValue;
            DateTime SecondCardEndDate = DateTime.MinValue;

            DateTime ThirdCardStartDate = DateTime.MinValue;
            DateTime ThirdCardEndDate = DateTime.MinValue;
            DateTime FourthCardStartDate = DateTime.MinValue;
            DateTime FourthCardEndDate = DateTime.MinValue;

            double totalHours = 0;
            double totalDays = 0;
            double totalVacationHours = 0;
            double totalVacationDays = 0;
            double averageWorkPerDay = 7.5;
            int hasFirstSetting = 1;//0,未设置；1，已设置
            int hasSecondSetting = 1;//0,未设置；1，已设置
            int hasThirdSetting = 1;//0,未设置；1，已设置
            int hasFourthSetting = 1;//0,未设置；1，已设置

            response.Result = Enums.Result.Success.GetHashCode();
            response.Message = Constants.Success;

            #endregion

            T_HR_EMPLOYEELEAVERECORD leaveEntity = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Where(t => t.LEAVERECORDID == request.LeaveRecordID).FirstOrDefault();

            if (leaveEntity != null)
            {
                //销假的时间超出请假时间范围
                if (dtStartDate < leaveEntity.STARTDATETIME && dtEndDate > leaveEntity.ENDDATETIME)
                {
                    response.Result = Enums.Result.IsOutOfLeaveDateRange.GetHashCode();
                    response.Message = Constants.IsOutOfLeaveDateRange;
                    return response;
                }

                #region "   获取考勤方案，排班明细，排班时间段   "

                //存储每个月考勤方案的字典
                //2014-12-10->2015-02-18
                int startYear = dtStartDate.Year;
                int endYear = dtEndDate.Year;
                int endMonth = dtEndDate.Month;
                List<string> dateList = new List<string>();
                //大小月份的判断
                List<int> bigMonth = new List<int>() { 1, 3, 5, 7, 8, 10, 12 };
                List<int> smallMonth = new List<int>() { 2, 4, 6, 9, 11 };
                #region "   将起止时间日历化    "
                List<string> dateDetailList = new List<string>();
                DateTime dtDateStartDetail = Convert.ToDateTime(dtStartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                DateTime dtDateEndDetail = Convert.ToDateTime(dtEndDate.ToString("yyyy-MM-dd HH:mm:ss"));

                if (dtDateStartDetail.ToString("yyyy-MM-dd") == dtDateEndDetail.ToString("yyyy-MM-dd"))
                {
                    string strEndDate = string.Empty;
                    if (dtDateEndDetail.Hour == 0)
                    {
                        strEndDate = dtDateStartDetail.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        strEndDate = dtDateEndDetail.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    dateDetailList.Add(dtDateStartDetail.ToString("yyyy-MM-dd HH:mm:ss") + "|" + strEndDate);
                }
                else
                {
                    bool symbol = true;
                    while (symbol)
                    {
                        //dateDetailList.Add(dtDateStartDetail);
                        string strStartDate = string.Empty;
                        string strEndDate = string.Empty;
                        if (dtDateStartDetail.ToString("yyyy-MM-dd") != dtDateEndDetail.ToString("yyyy-MM-dd"))
                        {
                            strStartDate = dtDateStartDetail.ToString("yyyy-MM-dd HH:mm:ss");
                            strEndDate = Convert.ToDateTime(dtDateStartDetail.ToString("yyyy-MM-dd")).AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                            dateDetailList.Add(strStartDate + "|" + strEndDate);

                            dtDateStartDetail = Convert.ToDateTime(dtDateStartDetail.AddDays(1).ToString("yyyy-MM-dd"));
                        }
                        else
                        {
                            strStartDate = dtDateStartDetail.ToString("yyyy-MM-dd HH:mm:ss");
                            if (dtDateEndDetail.Hour == 0)
                            {
                                strEndDate = dtDateStartDetail.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                strEndDate = dtDateEndDetail.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            dateDetailList.Add(strStartDate + "|" + strEndDate);
                            symbol = false;
                        }
                    }
                }
                #endregion

                #region "   获取请假时间段中所跨的月份   "

                DateTime dtDateStartMonth = dtStartDate;
                bool flag = true;
                while (flag)
                {
                    string yearStartMonth = string.Empty;
                    string yearEndMonth = string.Empty;
                    yearStartMonth = dtDateStartMonth.ToString("yyyy-MM-01");                   
                    yearEndMonth = Convert.ToDateTime(dtDateStartMonth.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString();
                    dateList.Add(yearStartMonth + "|" + yearEndMonth);
                    if (dtDateStartMonth.Month != endMonth)
                    {
                        dtDateStartMonth = dtDateStartMonth.AddMonths(1);
                    }
                    else
                    {
                        flag = false;
                    }
                }

                #endregion

                //DateList的格式：List<2014-05-01|2014-05-31>
                AttendanceSolutionBLL attendSolutionBll = new AttendanceSolutionBLL();
                List<T_HR_ATTENDANCESOLUTION> attendSolutionList = attendSolutionBll.GetAttendenceSolutionByEmployeeIDAndStartDateAndEndDate(request.EmployeeID, dateList);


                if (ltsEntity != null)
                {
                    //避免手机端无法提供LeaveTpeValue，因此重新赋值
                    request.LeaveTypeValue = string.IsNullOrEmpty(ltsEntity.LEAVETYPEVALUE) ? request.LeaveTypeValue : int.Parse(ltsEntity.LEAVETYPEVALUE);
                    //假期之间如何冲抵，例如：请事假的时候根据该状态来指示，是否可用年假或调休假抵扣
                    //航信要求:调休+带薪假+扣款，即请事假时将调休+带薪假用完后再扣款;
                    response.FineType = string.IsNullOrEmpty(ltsEntity.FINETYPE) ? "0" : ltsEntity.FINETYPE;
                    response.SexRestrict = ltsEntity.SEXRESTRICT;
                }

                if (attendSolutionList.Count > 0)
                {
                    //计算跨多个月份时的平均工作时间，只用于参考，因为每个月的考勤不一样，上班时间长度也可能不一样，可能有的7.5小时/天
                    //有的8小时/天，在此取一个平均数,四舍五入计算
                    averageWorkPerDay = Math.Round(Convert.ToDouble(attendSolutionList.Sum(t => t.WORKTIMEPERDAY) / attendSolutionList.Count()), 2);
                    response.AvgWorkPerDay = averageWorkPerDay;
                }

                #endregion
                //计算每个月份考勤方案对应的时长
                //例如：5月12日对应5月的考勤方案和每日工作时长，每日工作时间段设置
                foreach (string strDateString in dateDetailList)
                {
                    List<string> sWorkArr = new List<string>();//工作时间
                    List<string> sNotWorkArr = new List<string>();//休息时间
                    List<string> tempWorkArr = new List<string>();
                    double totalTempHours = 0;
                    double totalTempDays = 0;
                    DateTime dtTempStartDate = Convert.ToDateTime(strDateString.Split('|')[0]);
                    DateTime dtTempEndDate = Convert.ToDateTime(strDateString.Split('|')[1]);
                    DateTime dtTempEndDate1 = Convert.ToDateTime(strDateString.Split('|')[1]);

                    //获取这个月份的考勤方案
                    AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL();
                    T_HR_ATTENDANCESOLUTION LeavePeriodAttendSolution = bllAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(request.EmployeeID,
                                                                            Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd")),
                                                                            Convert.ToDateTime(dtTempEndDate.ToString("yyyy-MM-dd")));

                    SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                    IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> scheduleSetDetail = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(LeavePeriodAttendSolution.ATTENDANCESOLUTIONID);
                    T_HR_SCHEDULINGTEMPLATEMASTER scheduleSetting = scheduleSetDetail.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                    int iCycleDays = 0;
                    DateTime dtCycleStartDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM") + "-1"));//按月为周期的排班表
                    DateTime dtCurCycleOTDate = Convert.ToDateTime(DateTime.Parse(dtTempStartDate.ToString("yyyy-MM-dd")));
                    //找出加班时间与循环排班详细中对应的日历号，然后通过T_HR_SCHEDULINGTEMPLATEDETAIL找到对应的 打卡时间段： T_HR_SHIFTDEFINE
                    if (scheduleSetting.SCHEDULINGCIRCLETYPE == (Common.SchedulingCircleType.Month.GetHashCode() + 1).ToString())
                    {
                        //按月循环的排班打卡方式
                        iCycleDays = 31;
                    }

                    if ((scheduleSetting.SCHEDULINGCIRCLETYPE == (Common.SchedulingCircleType.Week.GetHashCode() + 1).ToString()))
                    {
                        //按周排班打卡方式
                        iCycleDays = 7;
                        //如果是按周统计，则从当前算起
                        dtCycleStartDate = Convert.ToDateTime(DateTime.Parse(dtTempStartDate.ToString("yyyy-MM-dd")));
                    }

                    T_HR_SHIFTDEFINE dayCardSetting = null;//具体的排班明细，最多包括了4个时段的打卡设置,用于计算加班小时数
                    for (int i = 0; i < iCycleDays; i++)//找出加班日期对应的日历中对应明细排班： T_HR_SHIFTDEFINE
                    {
                        string strSchedulingDate = (i + 1).ToString();
                        DateTime dtCurDate = new DateTime();

                        dtCurDate = dtCycleStartDate.AddDays(i);

                        if (dtCurDate != dtCurCycleOTDate)
                        {
                            continue;
                        }

                        T_HR_SCHEDULINGTEMPLATEDETAIL item = scheduleSetDetail.Where(c => c.SCHEDULINGDATE == strSchedulingDate).FirstOrDefault();
                        if (item != null)
                        {
                            dayCardSetting = item.T_HR_SHIFTDEFINE;//具体的排班明细
                        }
                    }

                    string strMonth = Convert.ToDateTime(dtTempStartDate).ToString("yyyy年MM月");
                    if (LeavePeriodAttendSolution != null)
                    {
                        if (dayCardSetting != null)
                        {
                            #region "   休息时间段的设置，以最大四个班次计算  "
                            DateTime notWorkTimeStart = DateTime.MinValue;
                            DateTime notWorkTimeEnd = DateTime.MinValue;

                            DateTime notWorkTimeStart1 = DateTime.MinValue;
                            DateTime notWorkTimeEnd1 = DateTime.MinValue;

                            DateTime notWorkTimeStart2 = DateTime.MinValue;
                            DateTime notWorkTimeEnd2 = DateTime.MinValue;
                            #endregion
                            #region "   第一时段打卡起始时间  "
                            if (!string.IsNullOrEmpty(dayCardSetting.FIRSTSTARTTIME) && !string.IsNullOrEmpty(dayCardSetting.FIRSTENDTIME))
                            {
                                FirstCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Minute));
                                FirstCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Minute));

                                notWorkTimeStart = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Minute));

                                sWorkArr.Add(FirstCardStartDate.ToString() + "|" + FirstCardEndDate.ToString());

                                response.FirstStartTime = string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Minute);
                                response.FirstEndTime = string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Minute);
                                //如果用户没有选择具体的结束小时数，那么就设置结束时间为作息的结束时间
                                if (dtTempEndDate1 > FirstCardEndDate)
                                {
                                    dtTempEndDate = FirstCardEndDate;
                                }
                            }
                            else
                            {
                                hasFirstSetting = 0;
                            }
                            #endregion
                            #region "   第二时段打卡起始时间  "
                            if (!string.IsNullOrEmpty(dayCardSetting.SECONDSTARTTIME) && !string.IsNullOrEmpty(dayCardSetting.SECONDENDTIME))
                            {
                                SecondCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Minute));
                                SecondCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute));

                                notWorkTimeEnd = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " "
                                    + string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Minute));
                                notWorkTimeStart1 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute));

                                sWorkArr.Add(SecondCardStartDate.ToString() + "|" + SecondCardEndDate.ToString());
                                sNotWorkArr.Add(notWorkTimeStart.ToString() + "|" + notWorkTimeEnd.ToString());

                                response.SecondStartTime = string.Format("{0}:{1}",
                                    Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Minute);
                                response.SecondEndTime = string.Format("{0}:{1}",
                                    Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute);
                                //如果用户没有选择具体的结束小时数，那么就设置结束时间为作息的结束时间
                                if (dtTempEndDate1 > SecondCardEndDate)
                                {
                                    dtTempEndDate = SecondCardEndDate;
                                }
                                else
                                {
                                    dtTempEndDate = dtTempEndDate1;
                                }
                            }
                            else
                            {
                                hasSecondSetting = 0;
                            }
                            #endregion
                            #region "   第三时段打卡起始时间  "
                            if (!string.IsNullOrEmpty(dayCardSetting.THIRDSTARTTIME) && !string.IsNullOrEmpty(dayCardSetting.THIRDENDTIME))
                            {
                                ThirdCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Minute));
                                ThirdCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Minute));

                                notWorkTimeEnd1 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Minute));
                                notWorkTimeStart2 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Minute));

                                sWorkArr.Add(ThirdCardStartDate.ToString() + "|" + ThirdCardEndDate.ToString());
                                sNotWorkArr.Add(notWorkTimeStart1.ToString() + "|" + notWorkTimeEnd1.ToString());

                                response.ThirdStartTime = string.IsNullOrEmpty(dayCardSetting.THIRDSTARTTIME) ? string.Empty :
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Minute);
                                response.ThirdEndTime = string.IsNullOrEmpty(dayCardSetting.THIRDENDTIME) ? string.Empty :
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Minute);
                                //如果用户没有选择具体的结束小时数，那么就设置结束时间为作息的结束时间
                                if (dtTempEndDate1 > ThirdCardEndDate)
                                {
                                    dtTempEndDate = ThirdCardEndDate;
                                }
                                else
                                {
                                    dtTempEndDate = dtTempEndDate1;
                                }
                            }
                            else
                            {
                                hasThirdSetting = 0;
                            }
                            #endregion
                            #region "   第四时段打卡起始时间  "
                            if (!string.IsNullOrEmpty(dayCardSetting.FOURTHSTARTTIME) && !string.IsNullOrEmpty(dayCardSetting.FOURTHENDTIME))
                            {
                                FourthCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Minute));
                                FourthCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Minute));

                                notWorkTimeEnd2 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Minute));

                                sWorkArr.Add(FourthCardStartDate.ToString() + "|" + FourthCardEndDate.ToString());
                                sNotWorkArr.Add(notWorkTimeStart2.ToString() + "|" + notWorkTimeEnd2.ToString());

                                response.FourthStartTime = string.IsNullOrEmpty(dayCardSetting.FOURTHSTARTTIME) ? string.Empty :
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).Minute);
                                response.FourthEndTime = string.IsNullOrEmpty(dayCardSetting.FOURTHENDTIME) ? string.Empty :
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Minute);
                                //如果用户没有选择具体的结束小时数，那么就设置结束时间为作息的结束时间
                                if (dtTempEndDate1 > FourthCardEndDate)
                                {
                                    dtTempEndDate = FourthCardEndDate;
                                }
                                else
                                {
                                    dtTempEndDate = dtTempEndDate1;
                                }
                            }
                            else
                            {
                                hasFourthSetting = 0;
                            }
                            #endregion
                            #region "   判断是否设置了至少两个的打卡时间段   "
                            //为设置打卡时间段,至少设置两个打卡时间段
                            if (hasFirstSetting == 0)
                            {
                                response.Result = Enums.Result.NonFirstSetting.GetHashCode();
                                //response.FirstSetting = hasFirstSetting;
                                //response.Month = strMonth;
                                //response.AttendSolution = LeavePeriodAttendSolution.ATTENDANCESOLUTIONNAME;
                                //response.WorkPerDay = LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? LeavePeriodAttendSolution.WORKTIMEPERDAY.Value : 0;
                                response.Message = dtTempStartDate.ToString("yyyy-MM") + Constants.NonFirstSetting;
                                return response;
                            }

                            if (hasSecondSetting == 0)
                            {
                                response.Result = Enums.Result.NonSecondSetting.GetHashCode();
                                //response.SecondSetting = hasSecondSetting;
                                //response.Month = strMonth;
                                //response.AttendSolution = LeavePeriodAttendSolution.ATTENDANCESOLUTIONNAME;
                                //response.WorkPerDay = LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? LeavePeriodAttendSolution.WORKTIMEPERDAY.Value : 0;
                                response.Message = dtTempStartDate.ToString("yyyy-MM") + Constants.NonSecondSetting;
                                return response;
                            }
                            #endregion

                            //这类条件的假期按自然日计算请假时长
                            //为了避免用户要动态设置哪些假期按自然日休，哪些假期排除公休日，将其配置在config中
                            string SpecialTypeValue = Utility.GetAppConfigByName("NaturalVacationDay");
                            if (!SpecialTypeValue.Contains(request.LeaveTypeValue.ToString()))
                            {
                                #region "   检查加班时间是否在公共假期，或是工作日，或是三八，或是五四     "

                                decimal dWorkMode = LeavePeriodAttendSolution.WORKMODE.Value;
                                int iWorkMode = 0;
                                int.TryParse(dWorkMode.ToString(), out iWorkMode);//获取工作制(工作天数/周)

                                List<int> iWorkDays = new List<int>();
                                SMT.HRM.BLL.Utility.GetWorkDays(iWorkMode, ref iWorkDays);

                                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                                IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(request.EmployeeID);
                                string strVacDayType = (Convert.ToInt32(SMT.HRM.BLL.Common.OutPlanDaysType.Vacation) + 1).ToString();
                                string strWorkDayType = (Convert.ToInt32(SMT.HRM.BLL.Common.OutPlanDaysType.WorkDay) + 1).ToString();

                                //获取公共假期设置，请假时，假期要减去，工作日要算上
                                DateTime vacTempStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd"));
                                DateTime vacTempEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd"));
                                IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType
                                                                                                && vacTempStartDate >= s.STARTDATE
                                                                                                && vacTempEndDate <= s.ENDDATE);

                                //获取工作日设置
                                DateTime workTempStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd"));
                                DateTime workTempEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd"));
                                IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType
                                                                                                && workTempStartDate >= s.STARTDATE
                                                                                                && workTempEndDate <= s.ENDDATE);


                                //当前星期几,是否要工作
                                //Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6.
                                int iDayOfWeek = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd")).DayOfWeek.GetHashCode();
                                bool iDayCount = iWorkDays.Contains(iDayOfWeek);

                                DateTime WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                    string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute));

                                if (dayCardSetting != null)
                                {
                                    if (hasThirdSetting == 1)
                                    {
                                        WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                            string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDENDTIME).Minute));
                                    }
                                    if (hasFourthSetting == 1)
                                    {
                                        WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                            string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Minute));
                                    }
                                }

                                #region "   这个时间段内存在假期，要扣除    "
                                if (entVacDays.Count() == 0)
                                {   //也不是设置的工作日
                                    if (entWorkDays.Count() == 0)
                                    {   //并且不在上班时间列表中那就是休息日,除去休息日
                                        if (!iDayCount)
                                        {
                                            tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                            totalVacationHours += LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;
                                            totalVacationDays += 1;
                                        }
                                    }
                                }
                                else//节假日
                                {
                                    //加班时间在假期设置中，但只是半天的设置
                                    foreach (var vac in entVacDays)
                                    {
                                        #region "   半天公共假期，三八妇女节，青年节    "
                                        if (vac.ISHALFDAY == "1")
                                        {
                                            if (vac.PEROID == "1")//下午放假
                                            {
                                                if (dayCardSetting != null)
                                                {
                                                    DateTime HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                       string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).Minute));
                                                    DateTime HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                       string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour, Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute));
                                                    //四个时间中，下午时间的开始与结束
                                                    if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                    {
                                                        HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                            string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Hour, Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).Minute));
                                                        HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                            string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Hour, Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).Minute));
                                                    }
                                                    //获取半天放假时间
                                                    tempWorkArr.Add(HalfNoonStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfNoonEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                    totalVacationHours += HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours;
                                                    if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                    {
                                                        totalVacationDays += Math.Round(HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                    }
                                                }
                                            }
                                            else//上午放假
                                            {
                                                if (dayCardSetting != null)
                                                {
                                                    DateTime HalfMorningStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                        string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Hour,
                                                        Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).Minute));

                                                    DateTime HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                       string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Hour,
                                                       Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).Minute));

                                                    //四个时间中，上午时间的开始与结束
                                                    if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                    {
                                                        HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " +
                                                            string.Format("{0}:{1}", Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Hour,
                                                            Convert.ToDateTime(dayCardSetting.SECONDENDTIME).Minute));
                                                    }
                                                    //获取上午放假时间
                                                    tempWorkArr.Add(HalfMorningStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfMorningEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                    totalVacationHours += HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours;
                                                    if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                    {
                                                        totalVacationDays += Math.Round(HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {   //全天假期
                                            tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                            totalVacationHours += LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;
                                            totalVacationDays += 1;
                                        }
                                        #endregion
                                    }
                                }
                                #endregion

                                #endregion
                            }

                            //填写的时段都在4个设置的上班时间段里面，则直接用结束时间减去开始时间
                            if ((dtTempStartDate >= FirstCardStartDate && dtTempStartDate <= FirstCardEndDate && dtTempEndDate >= FirstCardStartDate && dtTempEndDate <= FirstCardEndDate)
                                  ||
                                  (dtTempStartDate >= SecondCardStartDate && dtTempStartDate <= SecondCardEndDate && dtTempEndDate >= SecondCardStartDate && dtTempEndDate <= SecondCardEndDate)
                                  ||
                                  (dtTempStartDate >= ThirdCardStartDate && dtTempStartDate <= ThirdCardEndDate && dtTempEndDate >= ThirdCardStartDate && dtTempEndDate <= ThirdCardEndDate)
                                  ||
                                  (dtTempStartDate >= FourthCardStartDate && dtTempStartDate <= FourthCardEndDate && dtTempEndDate >= FourthCardStartDate && dtTempEndDate <= FourthCardEndDate))
                            {
                                totalHours = dtTempEndDate.Subtract(dtTempStartDate).TotalHours;
                            }
                            else
                            {
                                #region "   计算销假时间     "

                                //早7：50打开，将计算加班的有效开始时间设置为8：30，也就是dtCardStartDate
                                //23:00下班，假定四个工作时间段
                                if (dtTempStartDate < FirstCardStartDate)
                                {
                                    dtTempStartDate = FirstCardStartDate;
                                }

                                #region "   找出开始计算加班的时间点    "

                                DateTime tempLeaveDate = new DateTime();
                                foreach (string str in sWorkArr)
                                {
                                    string[] s = str.Split('|');
                                    DateTime WorkStartDate = Convert.ToDateTime(s[0]);
                                    DateTime WorkEndDate = Convert.ToDateTime(s[1]);
                                    //如果开始时间在工作时间范围内，那就从开始时间算加班
                                    if (dtTempStartDate >= WorkStartDate && dtTempStartDate <= WorkEndDate)
                                    {
                                        tempLeaveDate = dtTempStartDate;
                                    }
                                    //如果开始时间大于工作时间段的结束时间，则属于休息时间段的时间点
                                    //找出里他最近的上班时间点作为加班开始时间
                                    if (dtTempStartDate > WorkEndDate)
                                    {
                                        foreach (string str1 in sNotWorkArr)
                                        {
                                            string[] sn = str1.Split('|');
                                            DateTime notWorkStartDate = Convert.ToDateTime(sn[0]);
                                            DateTime notWorkEndDate = Convert.ToDateTime(sn[1]);
                                            //加班开始时间在休息时间点内，则加班的开始时间从休息时间的结束点开始
                                            if (dtTempStartDate >= notWorkStartDate && dtTempStartDate <= notWorkEndDate)
                                            {
                                                tempLeaveDate = notWorkEndDate;
                                            }
                                            else
                                            {
                                                //不在所有的休息时间段内，则说明加班是在
                                                //一天正常的上班时间段以外进行
                                                tempLeaveDate = dtTempStartDate;
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #region "   根据上班时间段，设置具体的销假时间段     "
                                //计算请假的时间段有可能是节假日，这时已经在上面的假期中减去了，
                                //而总时长里却因为用户选择了结束时间在凌成1：30分，因此程序没能计算到这一天的时长，导致总时长少了一天
                                if (tempWorkArr.Count == 0)
                                {
                                    for (int i = 0; i < sWorkArr.Count(); i++)
                                    {
                                        string[] ss = sWorkArr[i].Split('|');
                                        DateTime WorkStartDate = Convert.ToDateTime(ss[0]);
                                        DateTime WorkEndDate = Convert.ToDateTime(ss[1]);
                                        //工作时间段的开始时间大于加班开始时间
                                        if (WorkStartDate >= tempLeaveDate)
                                        {
                                            //加班结束时间大于工作时间段的结束时间
                                            //则说明加班时间包含这段工作时间段，计算完整的加班时间段
                                            if (dtTempEndDate >= WorkEndDate)
                                            {
                                                if (sNotWorkArr.Count() == 0)
                                                {
                                                    tempWorkArr.Add(WorkStartDate.ToString() + "|" + dtTempEndDate.ToString());
                                                }

                                                foreach (string str3 in sNotWorkArr)
                                                {
                                                    string[] sn = str3.Split('|');
                                                    DateTime notWorkStartDate = Convert.ToDateTime(sn[0]);
                                                    DateTime notWorkEndDate = Convert.ToDateTime(sn[1]);
                                                    if (dtTempEndDate >= notWorkStartDate && dtTempEndDate <= notWorkEndDate)
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString() + "|" + WorkEndDate.ToString());
                                                    }
                                                    else if (dtTempEndDate >= WorkEndDate)
                                                    {
                                                        if (i == sWorkArr.Count() - 1)
                                                        {
                                                            WorkEndDate = dtTempEndDate;
                                                        }
                                                        tempWorkArr.Add(WorkStartDate.ToString() + "|" + WorkEndDate.ToString());
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString() + "|" + notWorkStartDate.ToString());
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (sNotWorkArr.Count() == 0)
                                                {
                                                    tempWorkArr.Add(WorkStartDate.ToString() + "|" + dtTempEndDate.ToString());
                                                }

                                                foreach (string str3 in sNotWorkArr)
                                                {
                                                    string[] sn = str3.Split('|');
                                                    DateTime notWorkStartDate = Convert.ToDateTime(sn[0]);
                                                    DateTime notWorkEndDate = Convert.ToDateTime(sn[1]);
                                                    if (dtTempEndDate >= notWorkStartDate && dtTempEndDate <= notWorkEndDate)
                                                    {

                                                    }
                                                    else if (dtTempEndDate >= WorkEndDate)
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString() + "|" + WorkEndDate.ToString());
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString() + "|" + dtTempEndDate.ToString());
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (i == sWorkArr.Count() - 1)
                                            {
                                                WorkEndDate = dtTempEndDate;
                                            }
                                            tempWorkArr.Add(tempLeaveDate.ToString() + "|" + WorkEndDate.ToString());
                                        }
                                    }
                                }
                                #endregion

                                #region "   计算请假时长  "

                                tempWorkArr = tempWorkArr.Distinct().ToList();
                                foreach (string str4 in tempWorkArr)
                                {
                                    string[] sss = str4.Split('|');
                                    DateTime WorkStartDate = Convert.ToDateTime(sss[0]);
                                    DateTime WorkEndDate = Convert.ToDateTime(sss[1]);
                                    if (WorkStartDate <= WorkEndDate)
                                    {
                                        totalTempHours += Math.Round(WorkEndDate.Subtract(WorkStartDate).TotalHours, 1);
                                    }
                                }
                                //计算这一天中请假时长，将其折合成天数，取最小整数，剩余的小时数在后面的代码中会计算，供参考
                                if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue)
                                {
                                    totalTempDays = Math.Floor(totalTempHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value));
                                }
                                #endregion

                                #endregion
                            }
                        }
                    }
                    totalHours += totalTempHours;
                    totalDays += totalTempDays;
                }//foreach (string strDateString in dateList)       

                totalHours = totalHours - totalVacationHours;
                totalDays = totalDays - totalVacationDays;

                response.CancelDays = Math.Floor(totalHours / averageWorkPerDay);
                response.CancelHours = totalHours % averageWorkPerDay;//取余数，用于前台显示的剩余天数
                response.CancelTotalDays = Math.Round(totalHours / averageWorkPerDay, 2);
                response.CancelTotalHours = totalHours;

                //EmployeeBLL empBll = new EmployeeBLL();
                //T_HR_EMPLOYEE employee = empBll.GetEmployeeByID(request.EmployeeID);
                //航信版本有要求写在下列代码中
                string strVersion = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                if (strVersion == "false")
                {
                }
            }
            else
            {
                response.Result = Enums.Result.NonLeaveRecord.GetHashCode();
                response.Message = Constants.NonLeaveRecord;
                return response;
            }
            return response;
        }
        #endregion
    }
}
