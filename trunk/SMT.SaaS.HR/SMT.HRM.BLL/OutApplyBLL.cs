/*
 * 文件名：ClockInRecordBLL.cs
 * 作  用：员工加班业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010年1月26日, 14:21:24
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Dynamic;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.Foundation.Log;
using System.Threading;


namespace SMT.HRM.BLL
{
    public class OutApplyBLL : BaseBll<T_HR_OUTAPPLYRECORD>, IOperate
    {
        public OutApplyBLL()
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
        public IQueryable<T_HR_OUTAPPLYRECORD> EmployeeOverTimeRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_OUTAPPLYRECORD");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("OVERTIMERECORDID", "T_HR_OUTAPPLYRECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
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

            IQueryable<T_HR_OUTAPPLYRECORD> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            if (!string.IsNullOrEmpty(sort))
            {
                ents = ents.OrderBy(sort);
            }
            ents = Utility.Pager<T_HR_OUTAPPLYRECORD>(ents, pageIndex, pageSize, ref pageCount);

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
        public IQueryable<T_HR_OUTAPPLYRECORD> GetOverTimeRdListByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState)
        {
            IQueryable<T_HR_OUTAPPLYRECORD> ents = from o in dal.GetObjects()
                                                           where o.EMPLOYEEID == strEmployeeID && o.STARTDATE >= dtStart && o.ENDDATE <= dtEnd && o.CHECKSTATE == strCheckState
                                                           select o;
            return ents;
        }


        /// <summary>
        /// 获取员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        public T_HR_OUTAPPLYRECORD GetOverTimeRdByID(string strOverTimeRecordId)
        {
            var ents = from a in dal.GetTable()
                       where a.OUTAPPLYID == strOverTimeRecordId
                       select a;
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault();
            }
            return null;
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增加班申请记录
        /// </summary>
        /// <param name="entity">加班修改申请记录实体</param>
        /// <returns></returns>
        public string OverTimeRecordAdd(T_HR_OUTAPPLYRECORD entity)
        {
            try
            {
                string strMsg = string.Empty;
                if (GetAttendanceSolution(entity.EMPLOYEEID, entity.STARTDATE) == null)
                {
                    return "TYPEERROR";
                }

                decimal dTotalHours = 0;
                strMsg = CalculateOverTimeHours(entity.EMPLOYEEID, entity.STARTDATE.Value, entity.ENDDATE.Value, ref dTotalHours);

                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    return strMsg;
                }
                entity.OUTAPLLYTIMES = dTotalHours.ToString();

                //entity.STARTDATETIME = entity.STARTDATE.Value.ToString("hh:mm:ss");
                //entity.ENDDATETIME = entity.ENDDATE.Value.ToString("hh:mm:ss");

                if (!Add(entity))
                {
                    strMsg = "{ERROR}";
                }

                return strMsg;
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return "{ERROR}";
            }
        }

        /// <summary>
        /// 修改员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        public string ModifyOverTimeRd(T_HR_OUTAPPLYRECORD entOTRd)
        {
            try
            {
                string strMsg = string.Empty;
                var ent = GetOverTimeRdByID(entOTRd.OUTAPPLYID);
                if (ent != null)
                {
                    string strOldCheckState = string.Copy(ent.CHECKSTATE);
                    string strNewCheckState = entOTRd.CHECKSTATE;

                    if ((strOldCheckState == Convert.ToInt32(Common.CheckStates.UnSubmit).ToString() && strNewCheckState == Convert.ToInt32(Common.CheckStates.UnSubmit).ToString())
                        || (strOldCheckState == Convert.ToInt32(Common.CheckStates.UnSubmit).ToString() && strNewCheckState == Convert.ToInt32(Common.CheckStates.Approving).ToString()))
                    {
                        decimal dTotalHours = 0;
                        strMsg = CalculateOverTimeHours(entOTRd.EMPLOYEEID, entOTRd.STARTDATE.Value, entOTRd.ENDDATE.Value, ref dTotalHours);
                        if (!string.IsNullOrWhiteSpace(strMsg))
                        {
                            return strMsg;
                        }
                        entOTRd.OUTAPLLYTIMES = dTotalHours.ToString();

                        //entOTRd.STARTDATETIME = entOTRd.STARTDATE.Value.ToString("HH:mm:ss");
                        //entOTRd.ENDDATETIME = entOTRd.ENDDATE.Value.ToString("HH:mm:ss");
                    }

                    entOTRd.UPDATEDATE = DateTime.Now;

                    Utility.CloneEntity(entOTRd, ent);
                    dal.Update(ent);
                    SaveMyRecord(ent);
                }

                return "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return "{ERROR}";
            }

        }

        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        public int DeleteOverTimeRd(string[] strOverTimeRecordId)
        {
            try
            {
                foreach (var id in strOverTimeRecordId)
                {
                    var ent = GetOverTimeRdByID(id);
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

        /// <summary>
        /// 审核加班记录
        /// </summary>
        /// <param name="strOverTimeRecordID">主键索引</param>
        /// <param name="strCheckState"></param>
        /// <returns></returns>
        public string AuditOverTimeRd(string strOverTimeRecordID, string strCheckState)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strOverTimeRecordID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_OUTAPPLYRECORD entOTRd = GetOverTimeRdByID(strOverTimeRecordID);

                if (entOTRd == null)
                {
                    return "{NOTFOUND}";
                }

                entOTRd.CHECKSTATE = strCheckState;

                entOTRd.UPDATEDATE = DateTime.Now;

                //Utility.CloneEntity(entOTRd, ent);
                int i=dal.Update(entOTRd);
                if (i == 0)
                {
                    Tracer.Debug("更新T_HR_OUTAPPLYRECORD失败");
                    throw new Exception("更新T_HR_OUTAPPLYRECORD失败");
                }
                SaveMyRecord(entOTRd);


                if (entOTRd.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    return "{SAVESUCCESSED}";
                }
                else
                {

                    #region  启动处理考勤异常的线程

                    string attState = (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("EMPLOYEEID", entOTRd.EMPLOYEEID);
                    d.Add("STARTDATETIME", entOTRd.STARTDATE.Value);
                    d.Add("ENDDATETIME", entOTRd.ENDDATE.Value);
                    d.Add("ATTSTATE", attState);
                    Thread thread = new Thread(dealAttend);
                    thread.Start(d);

                    Tracer.Debug("外出申请启动消除异常的线程，外出申请开始时间:" + entOTRd.STARTDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            + " 结束时间：" + entOTRd.ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss") + "员工id：" + entOTRd.EMPLOYEEID);

                    #endregion
                }
                return "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = "{AUDITERROR}";
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }

        /// <summary>
        /// 外出消除异常
        /// </summary>
        /// <param name="obj"></param>
        private void dealAttend(object obj)
        {
            Dictionary<string, object> parameterDic = (Dictionary<string, object>)obj;
            string employeeid = parameterDic["EMPLOYEEID"].ToString();
            DateTime STARTDATETIME = (DateTime)parameterDic["STARTDATETIME"];
            DateTime ENDDATETIME = (DateTime)parameterDic["ENDDATETIME"];
            string attState = parameterDic["ATTSTATE"].ToString();

            using (AbnormRecordBLL bll = new AbnormRecordBLL())
            {
                //请假消除异常
                bll.DealEmployeeAbnormRecord(employeeid, STARTDATETIME, ENDDATETIME, attState);
            }
        }
        #endregion

        #region 私有方法

        private T_HR_ATTENDANCESOLUTION GetAttendanceSolution(string employeeid, DateTime? dtOTStartDate)
        {
            if (dtOTStartDate == null || string.IsNullOrWhiteSpace(employeeid))
            {
                return null;
            }

            AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(employeeid, dtOTStartDate.Value);

            if (entAttSolAsign == null)
            {
                return null;
            }

            return entAttSolAsign.T_HR_ATTENDANCESOLUTION;
        }

        #endregion

        /// <summary>
        /// 计算加班时长
        /// </summary>
        /// <param name="strEmployeeId"></param>
        /// <param name="dtOTStart"></param>
        /// <param name="dtOTEnd"></param>
        /// <param name="dOverTimeHours"></param>
        /// <returns></returns>
        public string CalculateOverTimeHours(string strEmployeeId, DateTime dtOTStart, DateTime dtOTEnd, ref decimal dOverTimeHours)
        {
            string strRes = string.Empty;
            decimal dTotalOverTimeHours = 0;

            DateTime dtStart, dtEnd = new DateTime();
            dtStart = dtOTStart;
            dtEnd = dtOTEnd;
            //DateTime.TryParse(dtOTStart.ToString("yyyy-MM-dd"), out dtStart);        //获取请假起始日期
            //DateTime.TryParse(dtOTEnd.ToString("yyyy-MM-dd"), out dtEnd);            //获取请假截止日期
            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeId, dtStart);
            if (entAttendSolAsign == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return "{NONEXISTASIGNEDATTENSOL}";
            }

            //获取考勤方案
            T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;
            decimal dWorkTimePerDay = entAttendSol.WORKTIMEPERDAY.Value;
            decimal dWorkMode = entAttendSol.WORKMODE.Value;
            int iWorkMode = 0;
            int.TryParse(dWorkMode.ToString(), out iWorkMode);//获取工作制(工作天数/周)

            List<int> iWorkDays = new List<int>();
            Utility.GetWorkDays(iWorkMode, ref iWorkDays);

            OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
            IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(strEmployeeId);

            string strVacDayType = (Convert.ToInt32(Common.OutPlanDaysType.Vacation) + 1).ToString();
            string strWorkDayType = (Convert.ToInt32(Common.OutPlanDaysType.WorkDay) + 1).ToString();
            IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);
            IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE >= dtStart && s.ENDDATE <= dtEnd);

            SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
            IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(entAttendSol.ATTENDANCESOLUTIONID);
            T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;


            TimeSpan ts = dtEnd.Subtract(dtStart);
            int iOTDays = ts.Days;
            string strMsg = string.Empty;
            dTotalOverTimeHours = iOTDays * dWorkTimePerDay + ts.Hours;
            //if (iOTDays == 0)
            //{
            //    CalculateNonWholeDayOverTimeHours(dWorkTimePerDay, entWorkDays, entVacDays, iWorkDays, dtOTStart, dtOTEnd,
            //        entTemplateDetails, entTemplateMaster, ref dTotalOverTimeHours, ref strRes);
            //}
            //else
            //{
            //    CalculateNonWholeDayOverTimeHours(dWorkTimePerDay, entWorkDays, entVacDays, iWorkDays, dtOTStart,
            //        dtStart.AddDays(1).AddSeconds(-1), entTemplateDetails, entTemplateMaster, ref dTotalOverTimeHours, ref strMsg);
            //    CalculateNonWholeDayOverTimeHours(dWorkTimePerDay, entWorkDays, entVacDays, iWorkDays, dtEnd, dtOTEnd,
            //        entTemplateDetails, entTemplateMaster, ref dTotalOverTimeHours, ref strRes);

            //    if (ts.Days > 0)
            //    {
            //        int iDays = ts.Days - 1;
            //        decimal dVacDays = 0;

            //        for (int i = 0; i < iDays; i++)
            //        {
            //            int j = i + 1;
            //            bool bIsWorkDay = false;
            //            bool bIsVacDay = false;
            //            DateTime dtCurDate = dtStart.AddDays(j);


            //            if (entWorkDays.Count() > 0)
            //            {
            //                foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
            //                {
            //                    if (item_Work.STARTDATE.Value <= dtCurDate && item_Work.ENDDATE >= dtCurDate)
            //                    {
            //                        bIsWorkDay = true;
            //                        break;
            //                    }
            //                }
            //            }

            //            if (bIsWorkDay)
            //            {
            //                strRes = "{OVERTIMEINWORKDAY}";
            //                break;
            //            }

            //            if (entVacDays.Count() > 0)
            //            {
            //                foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
            //                {
            //                    if (item_Vac.STARTDATE.Value <= dtCurDate && item_Vac.ENDDATE >= dtCurDate)
            //                    {
            //                        bIsVacDay = true;
            //                        break;
            //                    }
            //                }
            //            }

            //            if (!bIsVacDay && !bIsWorkDay)
            //            {
            //                if (iWorkDays.Contains(Convert.ToInt32(dtCurDate.DayOfWeek)))
            //                {
            //                    strRes = "{OVERTIMEINWORKDAY}";
            //                    break;
            //                }
            //            }

            //            if (bIsVacDay || !bIsWorkDay)
            //            {
            //                dVacDays += 1;
            //            }
            //        }

            //        if (!string.IsNullOrWhiteSpace(strRes))
            //        {
            //            return strRes;
            //        }

            //        dTotalOverTimeHours += dVacDays * dWorkTimePerDay;
            //    }
            //}

            dOverTimeHours = dTotalOverTimeHours;
            return strRes + strMsg;
        }

        //private void CalculateNonWholeDayOverTimeHoursNew


        /// <summary>
        /// 计算非全天加班的时长(节假日加班)
        /// </summary>
        /// <param name="dWorkTimePerDay">每日工作时长</param>
        /// <param name="entVacDays">公休假记录</param>
        /// <param name="entWorkDays">工作周记录</param>
        /// <param name="iWorkDays">工作日对应星期类型集合</param>
        /// <param name="dtOTStart">加班起始时间</param>
        /// <param name="dtOTEnd">加班截止时间</param>
        /// <param name="entTemplateMaster">作息方案</param>
        /// <param name="dTotalOverTimeHours">加班累计时长</param>
        private void CalculateNonWholeDayOverTimeHours(decimal dWorkTimePerDay, IQueryable<T_HR_OUTPLANDAYS> entWorkDays, IQueryable<T_HR_OUTPLANDAYS> entVacDays,
            List<int> iWorkDays, DateTime dtOTStart, DateTime dtOTEnd, IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails,
            T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster, ref decimal dOverTimeHours, ref string strMsg)
        {
            bool bNoCalculate = false;  //True，计算出勤日加班；False，计算节假日加班
            DateTime dtStartDate = DateTime.Parse(dtOTStart.ToString("yyyy-MM") + "-1");
            DateTime dtCurOTDate = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd"));

            if (entWorkDays.Count() > 0)
            {
                foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                {
                    if (item_Work.STARTDATE.Value <= dtCurOTDate && item_Work.ENDDATE >= dtCurOTDate)
                    {
                        bNoCalculate = true;
                        break;
                    }
                }
            }

            if (bNoCalculate)
            {
                strMsg = CalculateNonWholeDayOverTimeHoursOnWorkDay(dWorkTimePerDay, dtOTStart, dtOTEnd, entTemplateDetails,
             entTemplateMaster, ref dOverTimeHours);
                return;
            }

            if (iWorkDays.Contains(Convert.ToInt32(dtCurOTDate.DayOfWeek)))
            {
                if (entVacDays.Count() > 0)
                {
                    bNoCalculate = true;
                    foreach (T_HR_OUTPLANDAYS item_Work in entVacDays)
                    {
                        //如果当前加班时间在节假日时间
                        if (item_Work.STARTDATE.Value <= dtCurOTDate && item_Work.ENDDATE >= dtCurOTDate)
                        {
                            //计算节假日加班
                            bNoCalculate = false;
                            break;
                        }
                    }
                }

                if (bNoCalculate)
                {
                    strMsg = CalculateNonWholeDayOverTimeHoursOnWorkDay(dWorkTimePerDay, dtOTStart, dtOTEnd, entTemplateDetails,
                 entTemplateMaster, ref dOverTimeHours);
                    return;
                }
            }

            DateTime dtCheckDate = new DateTime();
            DateTime dtShiftEndDate = new DateTime();
            decimal dTempOverTimeHours = 0;

            int iCircleDay = 0;
            if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Month) + 1).ToString())
            {
                iCircleDay = 31;
            }
            else if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Week) + 1).ToString())
            {
                iCircleDay = 7;
            }

            for (int j = 0; j < iCircleDay; j++)
            {
                bool bIsOnDuty = false;

                string strSchedulingDate = (j + 1).ToString();
                DateTime dtCurDate = new DateTime();

                dtCurDate = dtStartDate.AddDays(j);

                if (dtCurDate != dtCurOTDate)
                {
                    continue;
                }

                T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == strSchedulingDate).FirstOrDefault();
                T_HR_SHIFTDEFINE entShiftDefine = item.T_HR_SHIFTDEFINE;
                //根据考勤方案中的上午上班时间计算上班加班时长
                if (entShiftDefine.FIRSTSTARTTIME != null && entShiftDefine.FIRSTENDTIME != null)
                {
                    //上午8：30
                    DateTime dtFirstStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTSTARTTIME).ToString("HH:mm"));
                    //上午12：00
                    DateTime dtFirstEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTENDTIME).ToString("HH:mm"));
                    //如果加班结束时间比上午下班时间小
                    if (dtFirstEnd >= dtOTEnd)
                    {
                        //加班结束时间-加班开始时间
                        TimeSpan tsFirst = dtOTEnd.Subtract(dtOTStart);
                        dTempOverTimeHours = tsFirst.Hours * 60 + tsFirst.Minutes;
                    }
                    else
                    {
                        if (dtFirstEnd > dtOTStart)
                        {
                            TimeSpan tsFirst = dtFirstEnd.Subtract(dtOTStart);
                            dTempOverTimeHours = tsFirst.Hours * 60 + tsFirst.Minutes;
                            dtShiftEndDate = dtFirstEnd;
                            //  bIsOnDuty = true;
                        }
                    }
                }


                if (entShiftDefine.SECONDSTARTTIME != null && entShiftDefine.SECONDENDTIME != null)
                {
                    DateTime dtSecondStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDSTARTTIME).ToString("HH:mm"));
                    DateTime dtSecondEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDENDTIME).ToString("HH:mm"));


                    //if (dtSecondEnd <= dtOTEnd)
                    //{
                    //    if (dtSecondStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtSecondEnd.Subtract(dtSecondStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;

                    //        dtShiftEndDate = dtSecondEnd;
                    //    }
                    //    else
                    //    {
                    //        if (dtSecondEnd > dtOTStart)
                    //        {
                    //            TimeSpan tsSecond = dtSecondEnd.Subtract(dtOTStart);
                    //            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //            dtShiftEndDate = dtSecondEnd;
                    //            bIsOnDuty = true;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (dtSecondStart >= dtOTEnd)
                    //    {
                    //        break;
                    //    }

                    //    if (dtSecondStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtSecondStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //    else
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //}

                    //如果加班结束时间小于二次打卡开始时间，就不往下走
                    if (dtOTEnd <= dtSecondStart)
                    {
                        break;
                    }
                    //加班结束时间小于等于二次打卡结束时间
                    if (dtOTEnd <= dtSecondEnd)
                    {
                        //加班开始时间小于等于二次打卡开始时间
                        if (dtOTStart <= dtSecondStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtSecondStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始时间大于二次打卡开始时间
                        else if (dtOTStart > dtSecondStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;

                        }

                    }
                    //加班结束时间大于二次打卡结束时间
                    else if (dtOTEnd > dtSecondEnd)
                    {
                        //加班开始时间要小于二次打卡开始时间
                        if (dtOTStart <= dtSecondStart)
                        {
                            TimeSpan tsSecond = dtSecondEnd.Subtract(dtSecondStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始时间大于二次打卡开始时间
                        else if (dtOTStart > dtSecondStart && dtOTStart <= dtSecondEnd)
                        {
                            TimeSpan tsSecond = dtSecondEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                    }
                    dtShiftEndDate = dtSecondEnd;
                }

                if (entShiftDefine.THIRDSTARTTIME != null && entShiftDefine.THIRDENDTIME != null)
                {
                    DateTime dtThirdStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDSTARTTIME).ToString("HH:mm"));
                    DateTime dtThirdEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDENDTIME).ToString("HH:mm"));

                    //if (dtThirdEnd <= dtOTEnd)
                    //{
                    //    if (dtThirdStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtThirdEnd.Subtract(dtThirdStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;

                    //        //dtShiftEndDate = dtThirdEnd;
                    //    }
                    //    else
                    //    {
                    //        if (dtThirdEnd > dtOTStart)
                    //        {
                    //            TimeSpan tsSecond = dtThirdEnd.Subtract(dtOTStart);
                    //            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //            dtShiftEndDate = dtThirdEnd;
                    //            bIsOnDuty = true;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (dtThirdStart >= dtOTEnd)
                    //    {
                    //        break;
                    //    }

                    //    if (dtThirdStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtThirdStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //    else
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //}

                    //如果加班结束时间小于三次打卡开始时间，就不往下走
                    if (dtOTEnd <= dtThirdStart)
                    {
                        break;
                    }
                    //加班时间小于等于三次打卡结束时间
                    if (dtOTEnd <= dtThirdEnd)
                    {
                        //加班开始时间小于等于三次打卡开始时间
                        if (dtOTStart <= dtThirdStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtThirdStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始时间大于三次打卡开始时间
                        else if (dtOTStart > dtThirdStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }

                    }
                    //加班时间大于三次打卡时间
                    else if (dtOTEnd > dtThirdEnd)
                    {
                        //加班开始时间要小于三次打卡开始时间
                        if (dtOTStart <= dtThirdStart)
                        {
                            TimeSpan tsSecond = dtThirdEnd.Subtract(dtThirdStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始大于三次打卡开始时间
                        else if (dtOTStart > dtThirdStart)
                        {
                            TimeSpan tsSecond = dtThirdEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                    }
                    dtShiftEndDate = dtThirdEnd;
                }

                if (entShiftDefine.FOURTHSTARTTIME != null && entShiftDefine.FOURTHENDTIME != null)
                {
                    DateTime dtFourthStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHSTARTTIME).ToString("HH:mm"));
                    DateTime dtFourthEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHENDTIME).ToString("HH:mm"));
                    //if (dtFourthEnd <= dtOTEnd)
                    //{
                    //    if (dtFourthStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtFourthEnd.Subtract(dtFourthStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;

                    //      //  dtShiftEndDate = dtFourthEnd;
                    //    }
                    //    else
                    //    {
                    //        if (dtFourthEnd > dtOTStart)
                    //        {
                    //            TimeSpan tsSecond = dtFourthEnd.Subtract(dtOTStart);
                    //            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //            dtShiftEndDate = dtFourthEnd;
                    //            bIsOnDuty = true;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (dtFourthStart >= dtOTEnd)
                    //    {
                    //        break;
                    //    }

                    //    if (dtFourthStart >= dtOTStart)
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtFourthStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //    else
                    //    {
                    //        TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                    //        dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                    //    }
                    //}

                    //如果加班结束时间小于四次打卡开始时间，就不往下走
                    if (dtOTEnd <= dtFourthStart)
                    {
                        break;
                    }
                    //加班时间小于四次打卡结束时间
                    if (dtOTEnd <= dtFourthEnd)
                    {
                        //加班开始时间小于等于三次打卡开始时间
                        if (dtOTStart <= dtFourthStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtFourthStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始时间大于三次打卡开始时间
                        else if (dtOTStart > dtFourthStart)
                        {
                            TimeSpan tsSecond = dtOTEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }

                    }
                    //加班时间大于四次打卡时间
                    else if (dtOTEnd > dtFourthEnd)
                    {
                        //加班开始时间要小于四次打卡开始时间
                        if (dtOTStart <= dtFourthStart)
                        {
                            TimeSpan tsSecond = dtFourthEnd.Subtract(dtFourthStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }
                        //加班开始大于四次打卡开始时间
                        else if (dtOTStart > dtFourthStart)
                        {
                            TimeSpan tsSecond = dtFourthEnd.Subtract(dtOTStart);
                            dTempOverTimeHours += tsSecond.Hours * 60 + tsSecond.Minutes;
                        }

                    }
                    dtShiftEndDate = dtFourthEnd;
                }

                //if (!bIsOnDuty)
                //{
                //    TimeSpan tsTemp = dtOTEnd.Subtract(dtOTStart);
                //    dTempOverTimeHours += tsTemp.Hours * 60 + tsTemp.Minutes;
                //}
                //else
                //{
                //    if (dtShiftEndDate > dtCheckDate)
                //    {
                //        TimeSpan tsTemp = dtOTEnd.Subtract(dtShiftEndDate);
                //        dTempOverTimeHours += tsTemp.Hours * 60 + tsTemp.Minutes;
                //    }
                //}
                //加班结束时间大于最后一次打卡时间
                if (dtOTEnd > dtShiftEndDate)
                {
                    //加班开始时间小于等打卡结束时间
                    if (dtOTStart <= dtShiftEndDate)
                    {
                        TimeSpan tsTemp = dtOTEnd.Subtract(dtShiftEndDate);
                        dTempOverTimeHours += tsTemp.Hours * 60 + tsTemp.Minutes;
                    }
                    //加班开始时间大于打卡结束时间
                    else if (dtOTStart > dtShiftEndDate)
                    {
                        TimeSpan tsTemp = dtOTEnd.Subtract(dtOTStart);
                        dTempOverTimeHours += tsTemp.Hours * 60 + tsTemp.Minutes;
                    }
                }


            }

            if (dTempOverTimeHours != 0)
            {
                dTempOverTimeHours = decimal.Round((dTempOverTimeHours) / 60, 1);

                //按自然时长算，以前是最多为一天工作时长
                //if (dTempOverTimeHours > dWorkTimePerDay)
                //{
                //    dTempOverTimeHours = dWorkTimePerDay;
                //}

                dOverTimeHours += dTempOverTimeHours;
            }
        }

        /// <summary>
        /// 计算非全天加班的时长(出勤日加班)
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dtOTStart"></param>
        /// <param name="dtOTEnd"></param>
        /// <param name="entTemplateDetails"></param>
        /// <param name="entTemplateMaster"></param>
        /// <param name="dOverTimeHours"></param>
        private string CalculateNonWholeDayOverTimeHoursOnWorkDay(decimal dWorkTimePerDay, DateTime dtOTStart, DateTime dtOTEnd,
            IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails, T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster,
            ref decimal dOverTimeHours)
        {
            DateTime dtStartDate = DateTime.Parse(dtOTStart.ToString("yyyy-MM") + "-1");
            DateTime dtCurOTDate = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd"));
            string strRes = string.Empty;
            decimal dTempOverTimeHours = 0;

            int iCircleDay = 0;
            if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Month) + 1).ToString())
            {
                iCircleDay = 31;
            }
            else if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Week) + 1).ToString())
            {
                iCircleDay = 7;
            }

            for (int j = 0; j < iCircleDay; j++)
            {
                bool bIsOnduty = false;
                string strSchedulingDate = (j + 1).ToString();
                DateTime dtCurDate = new DateTime();

                dtCurDate = dtStartDate.AddDays(j);

                if (dtCurDate != dtCurOTDate)
                {
                    continue;
                }

                T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == strSchedulingDate).FirstOrDefault();
                T_HR_SHIFTDEFINE entShiftDefine = item.T_HR_SHIFTDEFINE;

                if (entShiftDefine.FIRSTSTARTTIME != null && entShiftDefine.FIRSTENDTIME != null)
                {
                    DateTime dtFirstStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTSTARTTIME).ToString("HH:mm"));
                    DateTime dtFirstEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTENDTIME).ToString("HH:mm"));

                    if (dtFirstStart >= dtOTEnd)
                    {
                        TimeSpan tsTemp = dtOTEnd.Subtract(dtOTStart);
                        dTempOverTimeHours += tsTemp.Hours * 60 + tsTemp.Minutes;
                        break;
                    }
                    else
                    {
                        if (dtFirstEnd >= dtOTStart)
                        {
                            bIsOnduty = true;
                        }
                    }
                }

                if (bIsOnduty)
                {
                    strRes = "{OVERTIMEINWORKDAY}"; ;
                    break;
                }

                if (entShiftDefine.SECONDSTARTTIME != null && entShiftDefine.SECONDENDTIME != null)
                {
                    DateTime dtSecondStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDSTARTTIME).ToString("HH:mm"));
                    DateTime dtSecondEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDENDTIME).ToString("HH:mm"));

                    if (dtSecondEnd >= dtOTStart)
                    {
                        bIsOnduty = true;
                    }
                }

                if (bIsOnduty)
                {
                    strRes = "{OVERTIMEINWORKDAY}"; ;
                    break;
                }

                if (entShiftDefine.THIRDSTARTTIME != null && entShiftDefine.THIRDENDTIME != null)
                {
                    DateTime dtThirdStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDSTARTTIME).ToString("HH:mm"));
                    DateTime dtThirdEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDENDTIME).ToString("HH:mm"));

                    if (dtThirdEnd >= dtOTStart)
                    {
                        bIsOnduty = true;
                    }
                }

                if (bIsOnduty)
                {
                    strRes = "{OVERTIMEINWORKDAY}"; ;
                    break;
                }

                if (entShiftDefine.FOURTHSTARTTIME != null && entShiftDefine.FOURTHENDTIME != null)
                {
                    DateTime dtFourthStart = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHSTARTTIME).ToString("HH:mm"));
                    DateTime dtFourthEnd = DateTime.Parse(dtOTStart.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHENDTIME).ToString("HH:mm"));

                    if (dtFourthEnd >= dtOTStart)
                    {
                        bIsOnduty = true;
                    }
                }

                if (bIsOnduty)
                {
                    strRes = "{OVERTIMEINWORKDAY}"; ;
                    break;
                }

                TimeSpan tsCur = dtOTEnd.Subtract(dtOTStart);
                dTempOverTimeHours += tsCur.Hours * 60 + tsCur.Minutes;
            }

            if (dTempOverTimeHours != 0)
            {
                dTempOverTimeHours = decimal.Round((dTempOverTimeHours) / 60, 1);

                //按自然时长算，以前是最多为一天工作时长
                //if (dTempOverTimeHours > dWorkTimePerDay)
                //{
                //    dTempOverTimeHours = dWorkTimePerDay;
                //}

                dOverTimeHours += dTempOverTimeHours;
            }

            return strRes;
        }

        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                Tracer.Debug("开始更新员工外出申请");
                int i = 0;
                string strMsg = string.Empty;
                strMsg = AuditOverTimeRd(EntityKeyValue, CheckState);
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
                    +" 异常："+e.ToString());
                return 0;
            }
        }
    }
}
