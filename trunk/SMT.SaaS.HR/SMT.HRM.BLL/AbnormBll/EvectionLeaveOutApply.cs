
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data;
using SMT.HRM.CustomModel;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.HRM.CustomModel.Reports;
using SMT.Foundation.Log;
using SMT.HRM.BLL.Common;
using System.Threading;

namespace SMT.HRM.BLL
{
    public partial class AbnormRecordBLL : BaseBll<T_HR_EMPLOYEEABNORMRECORD>
    {
        /// <summary>
        /// 检查员工请假出差情况
        /// </summary>
        /// <param name="entAttRd">当天的考勤初始化记录</param>
        /// <param name="entClockInRds">打卡原始记录</param>
        /// <param name="strMsg">处理结果的消息</param>
        /// <param name="bIsAbnorm">是否出现考勤异常的标志(true/false)</param>
        private void CheckEvectionRecordAndLeaveRecordAttendState(string EMPLOYEEID, DateTime dtStartDate, DateTime dtEndDate, ref string strMsg, ref bool bIsAbnorm)
        {
            try
            {
                dtEndDate = dtEndDate.AddDays(1).AddSeconds(-1);
                //获取请假记录使用的起止时间
                //DateTime dtStartDate = entAttRd.ATTENDANCEDATE.Value;
                //DateTime dtEndDate = entAttRd.ATTENDANCEDATE.Value.AddDays(1).AddSeconds(-1);
                #region  启动出差处理考勤异常的线程
                //查询出差记录，检查当天存在出差情况
                EmployeeEvectionRecordBLL bllEvectionRd = new EmployeeEvectionRecordBLL();
                var entityEvections = from ent in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                                      where ent.EMPLOYEEID == EMPLOYEEID && ent.CHECKSTATE == "2"
                                       && (
                                            (ent.STARTDATE <= dtStartDate && ent.ENDDATE >= dtStartDate)
                                            || (ent.STARTDATE <= dtEndDate && ent.ENDDATE >= dtEndDate)
                                             || (ent.STARTDATE >= dtStartDate && ent.ENDDATE <= dtEndDate)
                                            )
                                      select ent;
                //如果有出差记录，就判断出差是否为全天
                if (entityEvections.Count() > 0)
                {
                    foreach (var ent in entityEvections)
                    {

                        //出差消除异常
                        string attState = (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString();
                        DealEmployeeAbnormRecord(EMPLOYEEID, ent.STARTDATE.Value, ent.ENDDATE.Value, attState);
                    }
                }
                #endregion

                #region  启动请假处理考勤异常的线程
                //查询请假记录，检查当天存在请假情况
                EmployeeLeaveRecordBLL bllLeaveRd = new EmployeeLeaveRecordBLL();
                IQueryable<T_HR_EMPLOYEELEAVERECORD> entLeaveRds = from e in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET")
                                                                   where e.EMPLOYEEID == EMPLOYEEID
                                                                   && e.CHECKSTATE == "2"
                                                                   && (
                                                                   (e.STARTDATETIME <= dtStartDate && e.ENDDATETIME >= dtStartDate)
                                                                   || (e.STARTDATETIME <= dtEndDate && e.ENDDATETIME >= dtEndDate)
                                                                   || (e.STARTDATETIME >= dtStartDate && e.ENDDATETIME <= dtEndDate)
                                                                   )
                                                                   select e;

                var sql = ((System.Data.Objects.ObjectQuery)entLeaveRds).ToTraceString();
                sql.ToString();
                if (entLeaveRds.Count() > 0)
                {

                    foreach (var ent in entLeaveRds)
                    {
                        //请假消除异常
                        string attState = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                        DealEmployeeAbnormRecord(ent.EMPLOYEEID, ent.STARTDATETIME.Value, ent.ENDDATETIME.Value, attState);
                    }
                }
                #endregion

                #region  启动外出申请处理考勤异常的线程
                //查询请假记录，检查当天存在请假情况
                var entOutApplyRds = from ent in dal.GetObjects<T_HR_OUTAPPLYRECORD>()
                                     where ent.EMPLOYEEID == EMPLOYEEID
                                     && ent.CHECKSTATE == "2"
                                      && (
                                     (ent.STARTDATE <= dtStartDate && ent.ENDDATE >= dtStartDate)
                                     || (ent.STARTDATE <= dtEndDate && ent.ENDDATE >= dtEndDate)
                                     || (ent.STARTDATE >= dtStartDate && ent.ENDDATE <= dtEndDate)
                                     )
                                     select ent;
                if (entOutApplyRds.Count() > 0)
                {
                    foreach (var ent in entOutApplyRds)
                    {
                        //外出申请消除异常
                        string attState = (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString();
                        DealEmployeeAbnormRecord(ent.EMPLOYEEID, ent.STARTDATE.Value, ent.ENDDATE.Value, attState);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工打卡记录导入检查员工请假出差情况异常：" + ex.ToString());
            }

            strMsg = "{SAVESUCCESSED}";
        }

        #region "出差请假外出检测并清除指定员工指定时间段内考勤异常记录并重置考勤初始化记录状态"
        /// <summary>
        /// 检测并清除指定员工指定时间段内考勤异常记录并重置考勤初始化记录状态
        /// </summary>
        /// <param name="EMPLOYEEID">员工id</param>
        /// <param name="datLevestart">开始时间长日期格式2013/2/16 8:30:00</param>
        /// <param name="dtLeveEnd">结束时间长日期格式2013/2/16 8:30:00</param>
        public void DealEmployeeAbnormRecord(string EMPLOYEEID, DateTime datLevestart, DateTime dtLeveEnd, string attState)
        {
            var emp = (from em in dal.GetObjects<T_HR_EMPLOYEE>()
                       where em.EMPLOYEEID == EMPLOYEEID
                       select em.EMPLOYEEENAME).FirstOrDefault();

            string dealType = "员工：" + emp + " 请假消除异常," + "时间区间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                    + "----" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss");

            if (attState == (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString())
            {
                dealType = "员工：" + emp + " 出差消除异常," + "时间区间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                    + "----" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (attState == (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString())
            {
                dealType = "员工：" + emp + " 外出申请消除异常," + "时间区间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                    + "----" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss");
            }

            Tracer.Debug(dealType + " 开始");

            DateTime dtStart = new DateTime(datLevestart.Year, datLevestart.Month, datLevestart.Day);
            DateTime dtEnd = new DateTime(dtLeveEnd.Year, dtLeveEnd.Month, dtLeveEnd.Day);

            DateTime dtAtt = new DateTime();
            int iDate = 0;


            #region 判断是否要初始化考勤
            while (dtAtt < dtEnd)
            {
                dtAtt = dtStart.AddDays(iDate);
                iDate++;
                IQueryable<T_HR_ATTENDANCERECORD> entArs = from r in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                                           where r.EMPLOYEEID == EMPLOYEEID
                                                            && r.ATTENDANCEDATE == dtAtt
                                                           select r;
                if (entArs.Count() < 1)
                {
                    string dtInit = datLevestart.Year.ToString() + "-" + datLevestart.Month.ToString();
                    try
                    {
                        Tracer.Debug(dealType + " 没有查到考勤初始化数据，开始初始化员工考勤：" + dtInit);
                        AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL();
                        //初始化该员工当月考勤记录
                        bllAttendanceSolutionAsign.AsignAttendanceSolutionByOrgID("4", EMPLOYEEID, dtInit);

                        Tracer.Debug(dealType + " 初始化员工考勤成功，初始化月份：" + dtInit);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug(dealType + " 初始化考勤失败：月份：" + dtInit + "失败原因：" + ex.ToString());
                        return;
                    }
                }
            }
            #endregion

            #region 循环检查考勤初始化记录状态

            IQueryable<T_HR_ATTENDANCERECORD> entAttAll = from r in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                                          where r.EMPLOYEEID == EMPLOYEEID
                                                          && r.ATTENDANCEDATE >= dtStart
                                                          && r.ATTENDANCEDATE <= dtEnd
                                                          select r;


            foreach (T_HR_ATTENDANCERECORD item in entAttAll)
            {
                string strAbnormCategory = (Convert.ToInt32(AbnormCategory.Absent) + 1).ToString();
                //获取请假当天所有异常考勤(针对补请假的情况，用于删除异常考勤)
                List<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords
                    = (from a in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>().Include("T_HR_ATTENDANCERECORD")
                       where a.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID == item.ATTENDANCERECORDID
                       && a.ABNORMCATEGORY == strAbnormCategory
                       select a).ToList();
                int i = 0;
                i = entAbnormRecords.Count();
                #region 如果当天没有异常，直接更新此考勤记录的考勤状态
                if (i == 0)
                {
                    //Tracer.Debug(strMsg);
                    item.ATTENDANCESTATE = attState;
                    item.UPDATEDATE = DateTime.Now;

                    string strMsg = item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + dealType + " ，无考勤异常，查询时间:" + item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd")
                           + "，没有查到异常记录，修改考勤记录状态为：" + attState;
                    item.REMARK = dealType + strMsg;
                    int iUpdate = dal.Update(item);
                    if (iUpdate == 1)
                    {
                        Tracer.Debug(dealType + strMsg + " 成功");
                    }
                    else
                    {
                        Tracer.Debug(dealType + strMsg + " 失败");
                    }
                    continue;
                }
                #endregion
                try
                {
                    Tracer.Debug(dealType + " 日期：" + item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + "，发现当天有异常考勤，需要重新检查当天出勤状态，检查到异常记录共：" + i + "条记录"
                         + "开始处理重新检查考勤异常——————————————————————————————————————Start");

                    Dictionary<AttendPeriod, AttendanceState> thisDayAttendState = new Dictionary<AttendPeriod, AttendanceState>();//考勤时间段1上午，2中午，3下午 考勤异常状态 1 缺勤 2 请假
                    foreach (T_HR_EMPLOYEEABNORMRECORD AbnormRecorditem in entAbnormRecords.ToList())
                    {
                        //需根据请假时间判断是否要删除掉考勤异常
                        //DateTime datLevestart = entLeaveRecord.STARTDATETIME.Value;//长日期格式2013/2/16 8:30:00
                        //DateTime datLeveEnd = entLeaveRecord.ENDDATETIME.Value;//长日期格式2013/2/16 8:30:00
                        DateTime dtDateAbnorm = AbnormRecorditem.ABNORMALDATE.Value;//短日期格式2013/3/8
                        #region 循环当天考勤异常检查出勤状态并修改考勤记录状态
                        var q = (from entsf in dal.GetObjects<T_HR_SHIFTDEFINE>()
                                 join ab in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                    on entsf.SHIFTDEFINEID equals ab.T_HR_SHIFTDEFINE.SHIFTDEFINEID
                                 where ab.ATTENDANCERECORDID == AbnormRecorditem.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID
                                 select entsf);
                        if (q.Count() > 0)
                        {
                            T_HR_SHIFTDEFINE defineTime = q.FirstOrDefault();
                            if (AbnormRecorditem.ATTENDPERIOD.Trim() == "1")//上午上班8:30异常
                            {
                                if (!string.IsNullOrEmpty(defineTime.NEEDFIRSTCARD) && defineTime.NEEDFIRSTCARD == "2")//以集团打卡举例，第一段上班打卡8:30
                                {
                                    Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第一段开始上班时间需打卡");
                                    if (!string.IsNullOrEmpty(defineTime.FIRSTSTARTTIME))
                                    {
                                        //定义的开始上班时间8:30
                                        DateTime ShiftFirstStartTime = DateTime.Parse(defineTime.FIRSTSTARTTIME);
                                        DateTime ShiftstartDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, ShiftFirstStartTime.Hour, ShiftFirstStartTime.Minute, ShiftFirstStartTime.Second);
                                        //定义的第一段上班结束时间12:00
                                        if (!string.IsNullOrEmpty(defineTime.FIRSTENDTIME))
                                        {
                                            DateTime FirstEndTime = DateTime.Parse(defineTime.FIRSTENDTIME);
                                            DateTime FirstEndDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, FirstEndTime.Hour, FirstEndTime.Minute, FirstEndTime.Second);

                                            //如果请假时间包括了第一段上班时间，那么消除异常
                                            if (datLevestart <= ShiftstartDateAndTime
                                                && dtLeveEnd >= ShiftstartDateAndTime)
                                            {
                                                Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第一段开始上班时间需打卡时间被请假时间覆盖，消除异常"
                                                    + " 消除异常，开始时间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                                            + " 结束时间：" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班开始时间：" +
                                            ShiftstartDateAndTime.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班结束时间:" + FirstEndDateAndTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                                //消除第一段异常生成的签卡
                                                DeleteSigFromAbnormal(AbnormRecorditem);
                                                //消除第一段打卡时间异常考勤
                                                dal.Delete(AbnormRecorditem);
                                                //第一段考勤时间标记为请假
                                                thisDayAttendState.Add(AttendPeriod.Morning, AttendanceState.Leave);
                                            }
                                            else
                                            {   //第一段考勤时间标记为异常
                                                thisDayAttendState.Add(AttendPeriod.Morning, AttendanceState.Abnormal);
                                            }
                                        }
                                        else
                                        {
                                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第一段开始上班时间需打卡，但班次定义中的FIRSTSTARTTIME为空");
                                        }
                                    }
                                    else
                                    {
                                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第一段开始上班时间需打卡，但班次定义中的FIRSTSTARTTIME为空");
                                    }
                                }
                            }
                            if (AbnormRecorditem.ATTENDPERIOD.Trim() == "2")//中午上班13:30异常
                            {
                                if (!string.IsNullOrEmpty(defineTime.NEEDSECONDCARD) && defineTime.NEEDSECONDCARD == "2")//以集团打卡举例，第二段上班打卡13:30
                                {
                                    Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段开始上班时间需打卡");
                                    if (!string.IsNullOrEmpty(defineTime.SECONDSTARTTIME))
                                    {
                                        DateTime SecondStartTime = DateTime.Parse(defineTime.SECONDSTARTTIME);
                                        DateTime SecondStartDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, SecondStartTime.Hour, SecondStartTime.Minute, SecondStartTime.Second);
                                        if (!string.IsNullOrEmpty(defineTime.SECONDENDTIME))
                                        {
                                            DateTime SencondEndTime = DateTime.Parse(defineTime.SECONDENDTIME);
                                            DateTime SencondEndDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, SencondEndTime.Hour, SencondEndTime.Minute, SencondEndTime.Second);

                                            if (datLevestart <= SecondStartDateAndTime
                                                && dtLeveEnd >= SecondStartDateAndTime)
                                            {
                                                Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段开始上班时间需打卡时间被请假时间覆盖，消除异常"
                                                    + " 消除异常，开始时间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                                            + " 结束时间：" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班开始时间：" +
                                            SecondStartDateAndTime.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班结束时间:" + SencondEndDateAndTime.ToString("yyyy-MM-dd HH:mm:ss"));

                                                //消除第二段异常生成的签卡
                                                DeleteSigFromAbnormal(AbnormRecorditem);
                                                //消除第二段打卡时间异常考勤
                                                dal.Delete(AbnormRecorditem);
                                                thisDayAttendState.Add(AttendPeriod.Midday, AttendanceState.Leave);
                                            }
                                            else
                                            {
                                                thisDayAttendState.Add(AttendPeriod.Midday, AttendanceState.Abnormal);
                                            }
                                        }
                                        else
                                        {
                                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段开始上班时间需打卡，但班次定义中的SECONDENDTIME为空");
                                        }
                                    }
                                    else
                                    {
                                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段开始上班时间需打卡，但班次定义中的SECONDSTARTTIME为空");
                                    }
                                }
                            }
                            if (AbnormRecorditem.ATTENDPERIOD.Trim() == "3")//下午17:30下班异常
                            {
                                if (!string.IsNullOrEmpty(defineTime.NEEDSECONDOFFCARD) && defineTime.NEEDSECONDOFFCARD == "2")//以集团打卡举例，第二段下班打卡17:30
                                {
                                    Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段结束上班时间需打卡");
                                    if (!string.IsNullOrEmpty(defineTime.SECONDSTARTTIME))
                                    {
                                        DateTime SecondStartTime = DateTime.Parse(defineTime.SECONDSTARTTIME);
                                        DateTime SecondStartDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, SecondStartTime.Hour, SecondStartTime.Minute, SecondStartTime.Second);

                                        if (!string.IsNullOrEmpty(defineTime.SECONDENDTIME))
                                        {
                                            DateTime SencondEndTime = DateTime.Parse(defineTime.SECONDENDTIME);
                                            DateTime SencondEndDateAndTime = new DateTime(dtDateAbnorm.Year, dtDateAbnorm.Month, dtDateAbnorm.Day, SencondEndTime.Hour, SencondEndTime.Minute, SencondEndTime.Second);

                                            if (datLevestart <= SencondEndDateAndTime
                                                && dtLeveEnd >= SencondEndDateAndTime)
                                            {
                                                Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段结束上班时间需打卡时间被请假时间覆盖，消除异常"
                                                     + " 消除异常，开始时间:" + datLevestart.ToString("yyyy-MM-dd HH:mm:ss")
                                            + " 结束时间：" + dtLeveEnd.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班开始时间：" +
                                            SecondStartDateAndTime.ToString("yyyy-MM-dd HH:mm:ss") + "定义的上班结束时间:" + SencondEndDateAndTime.ToString("yyyy-MM-dd HH:mm:ss"));

                                                //消除第三段异常生成的签卡
                                                DeleteSigFromAbnormal(AbnormRecorditem);
                                                //消除第三段打卡时间异常考勤
                                                dal.Delete(AbnormRecorditem);
                                                thisDayAttendState.Add(AttendPeriod.Evening, AttendanceState.Leave);
                                            }
                                            else
                                            {
                                                thisDayAttendState.Add(AttendPeriod.Evening, AttendanceState.Abnormal);
                                            }
                                        }
                                        else
                                        {
                                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段结束上班时间需打卡，但班次定义中的SECONDENDTIME为空");
                                        }
                                    }
                                    else
                                    {
                                        Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "考勤班次定义T_HR_SHIFTDEFINE第二段结束上班时间需打卡，但班次定义中的SECONDSTARTTIME为空");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + "消除异常，通过异常记录获取到考勤初始化记录但通过考勤初始化记录获取的考勤班次定义T_HR_SHIFTDEFINE为空");
                        }
                        #endregion
                    }
                    #region 修改考勤记录状态
                    if (thisDayAttendState.Count() > 0)//如果当天存在异常或请假情况
                    {   //如果当天存在请假，同时也存在异常
                        if (thisDayAttendState.Values.Contains(AttendanceState.Leave) && thisDayAttendState.Values.Contains(AttendanceState.Abnormal))
                        {   //标记当天出勤状况为Mix状态
                            if (attState == (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString())
                            {
                                item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.MixLeveAbnormal) + 1).ToString();
                            }
                            if (attState == (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString())
                            {
                                item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.MixTravelAbnormal) + 1).ToString();
                            }
                            if (attState == (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString())
                            {
                                item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.MixOutApplyAbnormal) + 1).ToString();
                            }
                            item.UPDATEDATE = DateTime.Now;

                            string strMsg = item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd") + " 消除异常修改状态完成，员工姓名：" + item.EMPLOYEENAME
                            + ",修改的状态为：" + item.ATTENDANCESTATE;

                            item.REMARK = dealType + strMsg;
                            int iUpdate = dal.Update(item);
                            if (iUpdate == 1)
                            {
                                Tracer.Debug(dealType + strMsg + " 成功");
                            }
                            else
                            {
                                Tracer.Debug(dealType + strMsg + " 失败");
                            }
                        }
                        else if (thisDayAttendState.Values.Contains(AttendanceState.Leave) && !thisDayAttendState.Values.Contains(AttendanceState.Abnormal))
                        {
                            //如果当天异常时间已全部被请假时间涵盖，删除签卡提醒并标记考勤为请假
                            EmployeeSignInRecordBLL bllSignInRd = new EmployeeSignInRecordBLL();
                            bllSignInRd.ClearNoSignInRecord("T_HR_EMPLOYEEABNORMRECORD", item.EMPLOYEEID, entAbnormRecords);
                            item.ATTENDANCESTATE = attState;
                            item.UPDATEDATE = DateTime.Now;

                            string strMsg = item.ATTENDANCEDATE.Value.ToString("yyy-MM-dd") + " 消除异常修改状态完成，员工姓名：" + item.EMPLOYEENAME
                            + ",修改的状态为：" + item.ATTENDANCESTATE;
                            item.REMARK = dealType + strMsg;
                            int iUpdate = dal.Update(item);
                            if (iUpdate == 1)
                            {
                                Tracer.Debug(dealType + strMsg + " 成功");
                            }
                            else
                            {
                                Tracer.Debug(dealType + strMsg + " 失败");
                            }
                        }
                    }
                    #endregion

                    Tracer.Debug(dealType + " 日期：" + item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + "，发现当天有异常考勤，需要重新检查当天出勤状态，检查到异常记录共：" + i + "条记录"
                         + "开始处理重新检查考勤异常——————————————————————————————————————End");

                }
                catch (Exception ex)
                {
                    Tracer.Debug(item.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd") + " " + dealType + " 异常：" + ex.ToString());
                }
            }//for each T_HR_ATTENDANCERECORD End
            #endregion

            Tracer.Debug(dealType + " 结束!");
        }


        private void DeleteSigFromAbnormal(T_HR_EMPLOYEEABNORMRECORD AbnormRecorditem)
        {
            try
            {
                int i = 0;
                var sigs = from sig in dal.GetObjects<T_HR_EMPLOYEESIGNINDETAIL>().Include("T_HR_EMPLOYEESIGNINRECORD")
                           where sig.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == AbnormRecorditem.ABNORMRECORDID
                           select sig;
                if (sigs.Count() > 0)
                {
                    string sigMasterId = string.Empty;
                    if (sigs.FirstOrDefault().T_HR_EMPLOYEESIGNINRECORD != null)
                    {
                        sigMasterId = sigs.FirstOrDefault().T_HR_EMPLOYEESIGNINRECORD.SIGNINID;
                    }

                    foreach (var sig in sigs.ToList())
                    {
                        try
                        {
                            i = dal.Delete(sig);
                            Tracer.Debug("删除签卡明细,员工id：" + sig.OWNERID + " 异常日期：" + sig.ABNORMALDATE + "异常时长"
                                + sig.ABNORMALTIME + " 异常时段：" + sig.ATTENDPERIOD + "异常类型" + sig.ABNORMCATEGORY);
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("删除签卡明细异常,员工id：" + sig.OWNERID + " 异常日期：" + sig.ABNORMALDATE + "异常时长"
                               + sig.ABNORMALTIME + " 异常时段：" + sig.ATTENDPERIOD + "异常类型" + sig.ABNORMCATEGORY + " 异常信息：" + ex.ToString());
                        }
                    }


                    if (!string.IsNullOrEmpty(sigMasterId))
                    {

                        var sigMaster = from sigM in dal.GetObjects<T_HR_EMPLOYEESIGNINRECORD>().Include("T_HR_EMPLOYEESIGNINDETAIL")
                                        where sigM.SIGNINID == sigMasterId
                                        select sigM;


                        if (sigMaster.Count() > 0)
                        {
                            if (sigMaster.FirstOrDefault().T_HR_EMPLOYEESIGNINDETAIL.Count() < 1)
                            {
                                T_HR_EMPLOYEESIGNINRECORD record = sigMaster.FirstOrDefault();
                                //如果当天异常时间已全部被请假时间涵盖，删除签卡提醒并标记考勤为请假
                                try
                                {
                                    SMT.SaaS.BLLCommonServices.Utility.RemoveMyRecord<T_HR_EMPLOYEESIGNINRECORD>(record);
                                    Tracer.Debug("关闭签卡我的单据,员工名：" + record.EMPLOYEENAME + " 员工id：" + record.EMPLOYEEID + " 创建日期：" + record.CREATEDATE + " 签卡备注"
                                                            + record.REMARK + " 签卡时间：" + record.SIGNINTIME);

                                    List<string> list = new List<string>() { record.SIGNINID };
                                    if (CloseAttendAbnormAlarmMsg(list, "T_HR_EMPLOYEESIGNINRECORD", AbnormRecorditem.OWNERID))
                                    {
                                        Tracer.Debug("关闭签卡待办成功,员工名：" + record.EMPLOYEENAME + " 员工id：" + record.EMPLOYEEID + " 创建日期：" + record.CREATEDATE + " 签卡备注"
                                                                + record.REMARK + " 签卡时间：" + record.SIGNINTIME);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SMT.Foundation.Log.Tracer.Debug("删除我的单据或关闭待办出现错误" + ex.ToString());
                                }

                                try
                                {
                                    i = dal.Delete(record);
                                    Tracer.Debug("删除签卡主表,员工名：" + record.EMPLOYEENAME + " 员工id：" + record.EMPLOYEEID + " 创建日期：" + record.CREATEDATE + " 签卡备注"
                                                                         + record.REMARK + " 签卡时间：" + record.SIGNINTIME);
                                }
                                catch (Exception ex)
                                {
                                    Tracer.Debug("删除签卡主表异常,员工名：" + record.EMPLOYEENAME + " 员工id：" + record.EMPLOYEEID + " 创建日期：" + record.CREATEDATE + " 签卡备注"
                                                                             + record.REMARK + " 签卡时间：" + record.SIGNINTIME + " 异常信息：" + ex.ToString());
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("DeleteSigFromAbnormal异常：" + ex.ToString());
            }
        }

        #endregion
    }
}
