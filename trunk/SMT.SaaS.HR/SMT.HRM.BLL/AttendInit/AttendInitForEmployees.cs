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
    public partial class AttendanceSolutionAsignBLL : BaseBll<T_HR_ATTENDANCESOLUTIONASIGN>, IOperate
    {

        #region 私有方法
        /// <summary>
        /// 对指定公司，指定的员工，按照指定的考勤方案应用生成指定时段内的考勤初始化记录
        /// </summary>
        /// <param name="entTemp"></param>
        /// <param name="entCompany"></param>
        /// <param name="entEmployees"></param>
        /// <param name="dtAsignDate"></param>
        /// <returns></returns>
        private string AsignAttendSolForEmployees(T_HR_ATTENDANCESOLUTIONASIGN entTemp, T_HR_COMPANY entCompany, List<T_HR_EMPLOYEE> entEmployees, DateTime dtAsignDate)
        {
            string strRes = string.Empty;
            try
            {

                decimal dWorkMode = entTemp.T_HR_ATTENDANCESOLUTION.WORKMODE.Value;
                string strAttendanceSolutionID = entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID;

                int iWorkMode = 0;
                int.TryParse(dWorkMode.ToString(), out iWorkMode);

                List<int> iWorkDays = new List<int>();
                Utility.GetWorkDays(iWorkMode, ref iWorkDays);

                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = new List<T_HR_SCHEDULINGTEMPLATEDETAIL>();

                var q = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(strAttendanceSolutionID);

                if (q == null)
                {
                    Tracer.Debug("考勤初始化考勤班次不存在");
                    return "{NOTFOUND}";
                }

                entTemplateDetails = q.ToList();

                if (entTemplateDetails == null)
                {
                    Tracer.Debug("考勤初始化考勤班次不存在");
                    return "{NOTFOUND}";
                }

                if (entTemplateDetails.Count == 0)
                {
                    Tracer.Debug("考勤初始化考勤班次不存在");
                    return "{NOTFOUND}";
                }

                int iTotalDay = 0;
                //DateTime dtCheck = new DateTime();
                DateTime dtStart = DateTime.Parse(dtAsignDate.ToString("yyyy-MM") + "-1");

                //DateTime dtStart = DateTime.Parse("2012-10-1");

                if (entTemp.STARTDATE > dtStart)
                {
                    dtStart = entTemp.STARTDATE.Value;
                }

                DateTime dtInitAttandRecordEndDate = dtStart.AddMonths(1).AddDays(-1);

                AttendanceRecordBLL bllAttRd = new AttendanceRecordBLL();
                EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                EmployeeEntryBLL bllEntry = new EmployeeEntryBLL();
                LeftOfficeConfirmBLL bllConfirm = new LeftOfficeConfirmBLL();

                //操作表T_HR_EMPLOYEELEVELDAYCOUNT数据的方式：0：直接逐条新增或修改；
                //1：先把要新增或修改的记录存到内存，然后一次性提交到数据库修改

                Tracer.Debug("开始生成员工考勤初始化记录,总员工数：" + entEmployees.Count());
                for (int n = 0; n < entEmployees.Count(); n++)
                {
                    bool AttendNoCheck = false;
                    try
                    {
                        T_HR_EMPLOYEE item_emp = entEmployees[n];
                        DateTime dtInitAttandRecordStartDate = new DateTime();

                        #region 判断是否免打卡
                        //如果是免打卡的用户，在这里还是需要初始化，因为结算的时候需要计算出勤天数
                        if (!entTemp.T_HR_ATTENDANCESOLUTIONReference.IsLoaded)
                        {
                            entTemp.T_HR_ATTENDANCESOLUTIONReference.Load();
                        }
                        if (entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())//考勤方案设置为不考勤
                        {
                            AttendNoCheck = true;
                            Tracer.Debug("初始化员工,考勤方案设置为免打卡，员工姓名："
                                + entEmployees.FirstOrDefault().EMPLOYEECNAME
                                + " 考勤方案名：" + entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME);

                        }

                        #endregion

                        #region 初始化开始日期大于结束日期
                        dtInitAttandRecordStartDate = dtStart;

                        if (dtInitAttandRecordStartDate >= dtInitAttandRecordEndDate)
                        {
                            Tracer.Debug("初始化员工考勤记录被跳过，dtInitAttandRecordStartDate >= dtEnd" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                            continue;
                        }
                        #endregion

                        #region 判断员工状态，是否有入职记录，是否已离职,入职，离职日期
                        string usedAttendSolutionName = ",使用的考勤方案：" + entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME
                            + "，当前线程id：" + Thread.CurrentThread.ManagedThreadId;
                        Tracer.Debug("初始化员工考勤记录：员工状态：" + item_emp.EMPLOYEESTATE + "，员工姓名" + item_emp.EMPLOYEECNAME
                            + usedAttendSolutionName);
                        if (item_emp.EMPLOYEESTATE == "0")
                        {
                            T_HR_EMPLOYEEENTRY entEntry = bllEntry.GetEmployeeEntryByEmployeeID(item_emp.EMPLOYEEID);
                            if (entEntry == null)
                            {
                                Tracer.Debug("初始化员工考勤记录被跳过,该员工入职为空" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                                continue;
                            }

                            if (entEntry.ONPOSTDATE.Value > dtInitAttandRecordStartDate && entEntry.ONPOSTDATE.Value < dtInitAttandRecordEndDate)
                            {
                                Tracer.Debug("初始化员工考勤记录开始日期被修改：entEntry.ONPOSTDATE.Value > dtInitAttandRecordStartDate" + "，员工姓名" + item_emp.EMPLOYEECNAME
                                  + " 入职日期：" + entEntry.ENTRYDATE.Value.ToString("yyyy-MM-dd")
                                  + " 到岗日期：" + entEntry.ONPOSTDATE.Value.ToString("yyyy-MM-dd"));
                                dtInitAttandRecordStartDate = entEntry.ONPOSTDATE.Value;
                            }

                            if (entEntry.ONPOSTDATE.Value > dtInitAttandRecordEndDate)
                            {
                                Tracer.Debug("初始化员工考勤记录被跳过：员工到岗日期大于考勤初始化结束日期entEntry.ONPOSTDATE.Value > dtEnd" + "，员工姓名" + item_emp.EMPLOYEECNAME
                                    + " 入职日期：" + entEntry.ENTRYDATE.Value.ToString("yyyy-MM-dd")
                                    + " 到岗日期：" + entEntry.ONPOSTDATE.Value.ToString("yyyy-MM-dd")
                                    + " 考勤初始化结束日期：" + dtInitAttandRecordEndDate);
                                continue;
                            }
                        }
                        if (item_emp.EMPLOYEESTATE == "1")
                        {
                            T_HR_EMPLOYEEENTRY entEntry = bllEntry.GetEmployeeEntryByEmployeeID(item_emp.EMPLOYEEID);
                            if (entEntry == null)
                            {
                                Tracer.Debug("该员工入职为空" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                                continue;
                            }

                            if (entEntry.ONPOSTDATE.Value > dtInitAttandRecordStartDate && entEntry.ONPOSTDATE.Value < dtInitAttandRecordEndDate)
                            {
                                Tracer.Debug("初始化员工考勤记录开始日期被修改：entEntry.ONPOSTDATE.Value > dtInitAttandRecordStartDate" + "，员工姓名" + item_emp.EMPLOYEECNAME
                                    + " 入职日期：" + entEntry.ENTRYDATE.Value.ToString("yyyy-MM-dd")
                                    + " 到岗日期：" + entEntry.ONPOSTDATE.Value.ToString("yyyy-MM-dd"));
                                dtInitAttandRecordStartDate = entEntry.ONPOSTDATE.Value;
                            }

                            if (entEntry.ENTRYDATE.Value > dtInitAttandRecordEndDate)
                            {
                                Tracer.Debug("初始化员工考勤记录被跳过,员工入职日期大于本月最后一天" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                                continue;
                            }
                        }
                        else if (item_emp.EMPLOYEESTATE == "2")
                        {
                            T_HR_LEFTOFFICECONFIRM entConfirm = bllConfirm.GetLeftOfficeConfirmByEmployeeId(item_emp.EMPLOYEEID);
                            if (entConfirm.STOPPAYMENTDATE != null && entConfirm.STOPPAYMENTDATE.Value < dtStart)
                            {
                                Tracer.Debug("初始化员工考勤记录被跳过,entConfirm.STOPPAYMENTDATE !=null && entConfirm.STOPPAYMENTDATE.Value < dtStart" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                                continue;
                            }

                            if (entConfirm.STOPPAYMENTDATE != null && entConfirm.STOPPAYMENTDATE.Value > dtStart && entConfirm.STOPPAYMENTDATE.Value < dtInitAttandRecordEndDate)
                            {
                                dtInitAttandRecordEndDate = entConfirm.STOPPAYMENTDATE.Value;
                            }

                            if (entConfirm.STOPPAYMENTDATE != null && entConfirm.STOPPAYMENTDATE.Value < dtStart)
                            {
                                Tracer.Debug("初始化员工考勤记录被跳过,entConfirm.STOPPAYMENTDATE != null && entConfirm.STOPPAYMENTDATE.Value < dtStart" + "，员工姓名" + item_emp.EMPLOYEECNAME);
                                continue;
                            }
                        }
                        #endregion

                        TimeSpan ts = dtInitAttandRecordEndDate.Subtract(dtInitAttandRecordStartDate);
                        iTotalDay = ts.Days;

                        T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails[0].T_HR_SCHEDULINGTEMPLATEMASTER;
                        int iCircleDay = 0;
                        if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Month) + 1).ToString())
                        {
                            iCircleDay = 31;
                        }
                        else if (entTemplateMaster.SCHEDULINGCIRCLETYPE == (Convert.ToInt32(Common.SchedulingCircleType.Week) + 1).ToString())
                        {
                            iCircleDay = 7;
                        }

                        int iPeriod = iTotalDay / iCircleDay;
                        if (iTotalDay % iCircleDay > 0)
                        {
                            iPeriod += 1;
                        }

                        OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                        IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(item_emp.EMPLOYEEID);

                        string strVacDayType = (Convert.ToInt32(Common.OutPlanDaysType.Vacation) + 1).ToString();
                        string strWorkDayType = (Convert.ToInt32(Common.OutPlanDaysType.WorkDay) + 1).ToString();
                        IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);
                        IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE >= dtInitAttandRecordStartDate && s.ENDDATE <= dtInitAttandRecordEndDate);
                        //IQueryable<T_HR_OUTPLANDAYS> entVacWorkDays = entOutPlanDays.Where(s => s.STARTDATE >= dtInitAttandRecordStartDate && s.ENDDATE <= dtInitAttandRecordEndDate);
                        //例外工作日考勤初始化记录公共假期
                        CreateOutPlanWorkDay(entCompany, item_emp, entTemp, entTemplateDetails, entWorkDays, AttendNoCheck);
                        //IQueryable<T_HR_OUTPLANDAYS> entVacWorkHalfDays 
                        //    = entVacDays.Where(s => s.STARTDATE >= dtInitAttandRecordStartDate 
                        //    && s.ENDDATE <= dtInitAttandRecordEndDate && s.ISHALFDAY=="1");
                        int addCount = 0;
                        int updateCount = 0;
                        for (int i = 0; i < iPeriod; i++)
                        {
                            for (int j = 0; j < iCircleDay; j++)
                            {
                                #region 开始生成员工考勤初始化记录
                                try
                                {
                                    int m = (i * iCircleDay) + j;
                                    DateTime dtCurDate = dtInitAttandRecordStartDate.AddDays(m);

                                    if (dtCurDate > entTemp.ENDDATE.Value)
                                    {
                                        break;
                                    }

                                    bool isVacDay = false;

                                    if (iWorkDays.Contains(Convert.ToInt32(dtCurDate.DayOfWeek)) == false)
                                    {
                                        continue;
                                    }

                                    if (entVacDays.Count() > 0)
                                    {
                                        foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                                        {
                                            if (item_Vac.STARTDATE.Value <= dtCurDate && item_Vac.ENDDATE >= dtCurDate)
                                            {
                                                //如果是公共假期并未设置半天，设为公共假期不考勤
                                                if (string.IsNullOrEmpty(item_Vac.ISHALFDAY))
                                                {
                                                    isVacDay = true;
                                                    break;
                                                }
                                                if (!string.IsNullOrEmpty(item_Vac.ISHALFDAY)
                                                   && item_Vac.ISHALFDAY == "0")
                                                {
                                                    isVacDay = true;
                                                    break;
                                                }
                                                else
                                                { //例外工作日考勤初始化记录（公共假期半天休息，还有半天上班也需要初始化考勤记录）
                                                }
                                            }
                                        }
                                    }

                                    if (isVacDay)
                                    {
                                        continue;
                                    }

                                    T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == (j + 1).ToString()).FirstOrDefault();

                                    var qc = from ar in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                             where //ar.OWNERCOMPANYID == entCompany.COMPANYID && 
                                             ar.EMPLOYEEID == item_emp.EMPLOYEEID && ar.ATTENDANCEDATE == dtCurDate
                                             select ar;

                                    T_HR_ATTENDANCERECORD entUpdate = qc.FirstOrDefault();
                                    if (entUpdate == null)
                                    {
                                        Tracer.Debug("开始新增员工T_HR_ATTENDANCERECORD记录,日期：" + dtCurDate.ToString("yyyy-MM-dd") + "，员工姓名:" + item_emp.EMPLOYEECNAME
                                             + usedAttendSolutionName);
                                        T_HR_ATTENDANCERECORD entAttRd = new T_HR_ATTENDANCERECORD();
                                        entAttRd.ATTENDANCERECORDID = System.Guid.NewGuid().ToString().ToUpper();
                                        entAttRd.ATTENDANCESOLUTIONID = entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID;
                                        entAttRd.EMPLOYEEID = item_emp.EMPLOYEEID;
                                        entAttRd.EMPLOYEECODE = item_emp.EMPLOYEECODE;
                                        entAttRd.EMPLOYEENAME = item_emp.EMPLOYEECNAME;
                                        entAttRd.ATTENDANCEDATE = dtCurDate;

                                        //外键实体
                                        //entAttRd.T_HR_SHIFTDEFINE = item.T_HR_SHIFTDEFINE;
                                        entAttRd.T_HR_SHIFTDEFINEReference.EntityKey = new EntityKey("SMT_HRM_EFModelContext.T_HR_SHIFTDEFINE", "SHIFTDEFINEID", item.T_HR_SHIFTDEFINE.SHIFTDEFINEID);

                                        //第一段工作时间
                                        entAttRd.FIRSTSTARTTIME = item.T_HR_SHIFTDEFINE.FIRSTSTARTTIME;
                                        entAttRd.FIRSTENDTIME = item.T_HR_SHIFTDEFINE.FIRSTENDTIME;

                                        //第二段工作时间
                                        entAttRd.SECONDSTARTTIME = item.T_HR_SHIFTDEFINE.SECONDSTARTTIME;
                                        entAttRd.SECONDENDTIME = item.T_HR_SHIFTDEFINE.SECONDENDTIME;

                                        //第三段工作时间
                                        entAttRd.THIRDSTARTTIME = item.T_HR_SHIFTDEFINE.THIRDSTARTTIME;
                                        entAttRd.THIRDENDTIME = item.T_HR_SHIFTDEFINE.THIRDENDTIME;

                                        //第四段工作时间
                                        entAttRd.FOURTHENDTIME = item.T_HR_SHIFTDEFINE.FOURTHENDTIME;
                                        entAttRd.FOURTHSTARTTIME = item.T_HR_SHIFTDEFINE.FOURTHSTARTTIME;
                                        //查询公共假期设置判断是否只上半天班公共假期
                                        var qVacDay = from ent in entVacDays
                                                      where ent.STARTDATE == entAttRd.ATTENDANCEDATE
                                                      select ent;
                                        if (qVacDay.Count() > 0)
                                        {
                                            var set = qVacDay.FirstOrDefault();
                                            if (!string.IsNullOrEmpty(set.ISHALFDAY))
                                            {
                                                if (set.ISHALFDAY == "1")
                                                {
                                                    if (set.PEROID == "0")//上午
                                                    {
                                                        Tracer.Debug("考勤初始化（新增）-检测到假期设置了休假（半天）："
                                                           + " 员工：" + entAttRd.EMPLOYEENAME
                                                           + " 日期：" + entAttRd.ATTENDANCEDATE
                                                           + " 上班分段（0上午）：" + set.PEROID);
                                                        entAttRd.NEEDFRISTATTEND = "1";//上午上班
                                                        entAttRd.NEEDSECONDATTEND = "0";//下午不上班
                                                    }
                                                    else if (set.PEROID == "1")
                                                    {
                                                        Tracer.Debug("考勤初始化（新增）-检测到假期设置了休假（半天）："
                                                            + " 员工：" + entAttRd.EMPLOYEENAME
                                                            + " 日期：" + entAttRd.ATTENDANCEDATE                                                            
                                                            + " 上班分段（1下午）：" + set.PEROID);
                                                        entAttRd.NEEDFRISTATTEND = "0";//上午不上班
                                                        entAttRd.NEEDSECONDATTEND = "1";//下午上班
                                                    }
                                                }
                                            }
                                        }
                                        entAttRd.ATTENDANCESTATE = string.Empty;    //新生成的考勤记录，出勤状态为空

                                        //权限
                                        entAttRd.OWNERCOMPANYID = item_emp.OWNERCOMPANYID;
                                        entAttRd.OWNERDEPARTMENTID = item_emp.OWNERDEPARTMENTID;
                                        entAttRd.OWNERPOSTID = item_emp.OWNERPOSTID;
                                        entAttRd.OWNERID = item_emp.OWNERID;

                                        entAttRd.CREATEUSERID = entTemp.CREATEUSERID;
                                        entAttRd.CREATEDATE = DateTime.Now;
                                        entAttRd.REMARK = entTemp.REMARK;
                                        entAttRd.UPDATEUSERID = entTemp.UPDATEUSERID;
                                        entAttRd.UPDATEDATE = DateTime.Now;
                                        entAttRd.CREATECOMPANYID = entTemp.CREATECOMPANYID;
                                        entAttRd.CREATEDEPARTMENTID = entTemp.CREATEDEPARTMENTID;
                                        entAttRd.CREATEPOSTID = entTemp.CREATEPOSTID;

                                        if (AttendNoCheck) entAttRd.ATTENDANCESTATE = "1";//免打卡员工
                                        addCount += dal.Add(entAttRd);
                                    }
                                    else
                                    {
                                        if (AttendNoCheck)
                                        {
                                            entUpdate.ATTENDANCESTATE = "1";//免打卡员工
                                        }
                                        else
                                        {   //非免打卡员工，跳过
                                            Tracer.Debug("初始化考勤记录已存在，" + " 员工姓名" + item_emp.EMPLOYEECNAME + " 考勤初始化日期：" + entUpdate.ATTENDANCEDATE.Value.ToString("yyyy-MM-dd")
                                                 + usedAttendSolutionName);
                                            if (string.IsNullOrEmpty(entUpdate.ATTENDANCESTATE))
                                            {
                                                continue;//如果存在直接跳过
                                            }

                                        }
                                        Tracer.Debug("更新考勤初始化记录，ATTENDANCESTATE考勤状态为空,日期：" + dtCurDate.ToString("yyyy-MM-dd") + "，员工姓名:" + item_emp.EMPLOYEECNAME
                                            + "，初始化考勤状态：" + entUpdate.ATTENDANCESTATE + usedAttendSolutionName);
                                        entUpdate.ATTENDANCESOLUTIONID = entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID;
                                        entUpdate.EMPLOYEEID = item_emp.EMPLOYEEID;
                                        entUpdate.EMPLOYEECODE = item_emp.EMPLOYEECODE;
                                        entUpdate.EMPLOYEENAME = item_emp.EMPLOYEECNAME;
                                        entUpdate.ATTENDANCEDATE = dtCurDate;

                                        entUpdate.T_HR_SHIFTDEFINE = item.T_HR_SHIFTDEFINE;

                                        //第一段工作时间
                                        entUpdate.FIRSTSTARTTIME = item.T_HR_SHIFTDEFINE.FIRSTSTARTTIME;
                                        entUpdate.FIRSTENDTIME = item.T_HR_SHIFTDEFINE.FIRSTENDTIME;

                                        //第二段工作时间
                                        entUpdate.SECONDSTARTTIME = item.T_HR_SHIFTDEFINE.SECONDSTARTTIME;
                                        entUpdate.SECONDENDTIME = item.T_HR_SHIFTDEFINE.SECONDENDTIME;

                                        //第三段工作时间
                                        entUpdate.THIRDSTARTTIME = item.T_HR_SHIFTDEFINE.THIRDSTARTTIME;
                                        entUpdate.THIRDENDTIME = item.T_HR_SHIFTDEFINE.THIRDENDTIME;

                                        //第四段工作时间
                                        entUpdate.FOURTHENDTIME = item.T_HR_SHIFTDEFINE.FOURTHENDTIME;
                                        entUpdate.FOURTHSTARTTIME = item.T_HR_SHIFTDEFINE.FOURTHSTARTTIME;

                                        //查询公共假期设置判断是否只上半天班
                                        var qVacDay = from ent in entVacDays
                                                      where ent.STARTDATE == entUpdate.ATTENDANCEDATE
                                                      select ent;
                                        if (qVacDay.Count() > 0)
                                        {
                                            var set = qVacDay.FirstOrDefault();
                                            if (!string.IsNullOrEmpty(set.ISHALFDAY))
                                            {
                                                if (set.ISHALFDAY == "1")
                                                {
                                                    if (set.PEROID == "0")//上午
                                                    {
                                                        Tracer.Debug("考勤初始化（修改）-检测到假期设置了休假（半天）："
                                                          + " 员工：" + entUpdate.EMPLOYEENAME
                                                          + " 日期：" + entUpdate.ATTENDANCEDATE
                                                          + " 上班分段（0上午）：" + set.PEROID);
                                                        entUpdate.NEEDFRISTATTEND = "0";//上午不上班休息
                                                        entUpdate.NEEDSECONDATTEND = "1";//下午上班
                                                    }
                                                    else if (set.PEROID == "1")
                                                    {
                                                        Tracer.Debug("考勤初始化（修改）-检测到假期设置了休假（半天）："
                                                            + " 员工：" + entUpdate.EMPLOYEENAME
                                                            + " 日期：" + entUpdate.ATTENDANCEDATE
                                                            + " 上班分段（1下午）：" + set.PEROID);                                                       
                                                        entUpdate.NEEDFRISTATTEND = "1";//上午上班
                                                        entUpdate.NEEDSECONDATTEND = "0";//下午不上班休息
                                                    }
                                                }
                                            }
                                        }
                                        entUpdate.ATTENDANCESTATE = string.Empty;    //新生成的考勤记录，出勤状态为空

                                        //权限
                                        entUpdate.OWNERCOMPANYID = item_emp.OWNERCOMPANYID;
                                        entUpdate.OWNERDEPARTMENTID = item_emp.OWNERDEPARTMENTID;
                                        entUpdate.OWNERPOSTID = item_emp.OWNERPOSTID;
                                        entUpdate.OWNERID = item_emp.OWNERID;

                                        entUpdate.REMARK = entTemp.REMARK;
                                        entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                                        entUpdate.UPDATEDATE = DateTime.Now;

                                        updateCount += dal.Update(entUpdate);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Tracer.Debug("生成考勤初始化记录出错：" + item_emp.EMPLOYEECNAME + ex.ToString() + usedAttendSolutionName);
                                    continue;
                                }
                                #endregion
                            }
                        }
                        //bllLevelDayCount.CalculateEmployeeLevelDayCount(entTemp, item_emp, strOperationType);
                        //int saveCount=dal.SaveContextChanges();
                        Tracer.Debug(n + "生成员工：" + item_emp.EMPLOYEECNAME + " 考勤记录成功,开始日期" + dtStart.ToString("yyyy-MM-dd") + "结束日期："
                            + dtInitAttandRecordEndDate.ToString("yyyy-MM-dd") + "共新增考勤记录" + addCount.ToString() + " 更新记录条数：" + updateCount
                            + usedAttendSolutionName);
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("生成考勤初始化记录出错：" + entEmployees[n].EMPLOYEECNAME + ex.ToString());
                        continue;
                    }
                }
                Tracer.Debug("生成所有员工考勤记录成功,开始日期" + dtStart.ToString("yyyy-MM-dd") + "结束日期：" + dtInitAttandRecordEndDate.ToString("yyyy-MM-dd"));
                strRes = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Tracer.Debug("生成考勤初始化记录出错：" + ex.ToString());
                strRes = ex.Message.ToString();
            }

            return strRes;
        }

        /// <summary>
        /// 对指定员工生成列外日期的考勤作息记录
        /// </summary>
        /// <param name="entCompany">员工所在公司</param>
        /// <param name="item_emp">员工信息</param>
        /// <param name="entTemp">考勤方案分配</param>
        /// <param name="entTemplateDetails">作息方案</param>
        /// <param name="entWorkDays">调剂工作日</param>
        private void CreateOutPlanWorkDay(T_HR_COMPANY entCompany, T_HR_EMPLOYEE item_emp, T_HR_ATTENDANCESOLUTIONASIGN entTemp,
            List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails, IQueryable<T_HR_OUTPLANDAYS> entWorkDays, bool AttendNoCheck)
        {
            if (entWorkDays == null)
            {
                return;
            }

            if (entWorkDays.Count() == 0)
            {
                return;
            }

            List<DateTime> listWorkDay = new List<DateTime>();
            foreach (T_HR_OUTPLANDAYS entOutPlanDay in entWorkDays)
            {
                TimeSpan ts = entOutPlanDay.ENDDATE.Value.Subtract(entOutPlanDay.STARTDATE.Value);
                int iDayPeriod = ts.Days;
                iDayPeriod += 1;//实际天数应包含起始天数

                for (int i = 0; i < iDayPeriod; i++)
                {
                    DateTime dtTemp = entOutPlanDay.STARTDATE.Value.AddDays(i);
                    listWorkDay.Add(dtTemp);
                }
            }

            if (listWorkDay.Count() == 0)
            {
                return;
            }

            for (int j = 0; j < listWorkDay.Count(); j++)
            {
                int m = 0;
                DateTime dtCurDate = listWorkDay[j];
                m = dtCurDate.Day;

                T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == m.ToString()).FirstOrDefault();

                var qc = from ar in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                         where //ar.OWNERCOMPANYID == entCompany.COMPANYID && 
                         ar.EMPLOYEEID == item_emp.EMPLOYEEID && ar.ATTENDANCEDATE == dtCurDate
                         select ar;

                T_HR_ATTENDANCERECORD entUpdate = qc.FirstOrDefault();
                if (entUpdate == null)
                {
                    T_HR_ATTENDANCERECORD entAttRd = new T_HR_ATTENDANCERECORD();
                    entAttRd.ATTENDANCERECORDID = System.Guid.NewGuid().ToString().ToUpper();
                    entAttRd.ATTENDANCESOLUTIONID = entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID;
                    entAttRd.EMPLOYEEID = item_emp.EMPLOYEEID;
                    entAttRd.EMPLOYEECODE = item_emp.EMPLOYEECODE;
                    entAttRd.EMPLOYEENAME = item_emp.EMPLOYEECNAME;
                    entAttRd.ATTENDANCEDATE = dtCurDate;
                    if (item.T_HR_SHIFTDEFINE == null)
                    {
                        Tracer.Debug("生成员工列外工作日期错误，排班定义T_HR_SCHEDULINGTEMPLATEDETAIL:" + item.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATENAME + " T_HR_SHIFTDEFINE为Null");
                        return;
                    }
                    entAttRd.T_HR_SHIFTDEFINEReference.EntityKey = new EntityKey("SMT_HRM_EFModelContext.T_HR_SHIFTDEFINE", "SHIFTDEFINEID", item.T_HR_SHIFTDEFINE.SHIFTDEFINEID);

                    //第一段工作时间
                    entAttRd.FIRSTSTARTTIME = item.T_HR_SHIFTDEFINE.FIRSTSTARTTIME;
                    entAttRd.FIRSTENDTIME = item.T_HR_SHIFTDEFINE.FIRSTENDTIME;

                    //第二段工作时间
                    entAttRd.SECONDSTARTTIME = item.T_HR_SHIFTDEFINE.SECONDSTARTTIME;
                    entAttRd.SECONDENDTIME = item.T_HR_SHIFTDEFINE.SECONDENDTIME;

                    //第三段工作时间
                    entAttRd.THIRDSTARTTIME = item.T_HR_SHIFTDEFINE.THIRDSTARTTIME;
                    entAttRd.THIRDENDTIME = item.T_HR_SHIFTDEFINE.THIRDENDTIME;

                    //第四段工作时间
                    entAttRd.FOURTHENDTIME = item.T_HR_SHIFTDEFINE.FOURTHENDTIME;
                    entAttRd.FOURTHSTARTTIME = item.T_HR_SHIFTDEFINE.FOURTHSTARTTIME;

                    //查询公共假期设置判断是否只上半天班公共假期
                    var q = from ent in entWorkDays
                            where ent.STARTDATE == entAttRd.ATTENDANCEDATE
                            select ent;
                    if (q.Count() > 0)
                    {
                        var set = q.FirstOrDefault();
                        if (!string.IsNullOrEmpty(set.ISHALFDAY))
                        {
                            if (set.ISHALFDAY == "1")
                            {
                                if (set.PEROID == "0")//上午
                                {
                                    Tracer.Debug("考勤初始化（新增）-检测设置了半天工作日："
                                               + " 员工：" + entAttRd.EMPLOYEENAME
                                               + " 日期：" + entAttRd.ATTENDANCEDATE
                                               + " 上班分段（0上午）：" + set.PEROID);
                                    entAttRd.NEEDFRISTATTEND = "1";//上午上班
                                    entAttRd.NEEDSECONDATTEND = "0";//下午不上班
                                }
                                else if (set.PEROID == "1")
                                {
                                    Tracer.Debug("考勤初始化（新增）-检测设置了半天工作日："
                                             + " 员工：" + entAttRd.EMPLOYEENAME
                                             + " 日期：" + entAttRd.ATTENDANCEDATE
                                             + " 上班分段（1下午）：" + set.PEROID);
                                    entAttRd.NEEDFRISTATTEND = "0";//上午不上班
                                    entAttRd.NEEDSECONDATTEND = "1";//下午上班
                                }
                            }
                        }
                    }
                    entAttRd.ATTENDANCESTATE = string.Empty;    //新生成的考勤记录，出勤状态为空

                    //权限
                    entAttRd.OWNERCOMPANYID = item_emp.OWNERCOMPANYID;
                    entAttRd.OWNERDEPARTMENTID = item_emp.OWNERDEPARTMENTID;
                    entAttRd.OWNERPOSTID = item_emp.OWNERPOSTID;
                    entAttRd.OWNERID = item_emp.OWNERID;

                    entAttRd.CREATEUSERID = entTemp.CREATEUSERID;
                    entAttRd.CREATEDATE = DateTime.Now;
                    entAttRd.REMARK = entTemp.REMARK;
                    entAttRd.UPDATEUSERID = entTemp.UPDATEUSERID;
                    entAttRd.UPDATEDATE = DateTime.Now;
                    entAttRd.CREATECOMPANYID = entTemp.CREATECOMPANYID;
                    entAttRd.CREATEDEPARTMENTID = entTemp.CREATEDEPARTMENTID;
                    entAttRd.CREATEPOSTID = entTemp.CREATEPOSTID;
                    if (AttendNoCheck) entAttRd.ATTENDANCESTATE = "1";//免打卡员工
                    dal.Add(entAttRd);
                    Tracer.Debug("初始化设置的例外工作日考勤记录新增记录，" + " 员工姓名" + item_emp.EMPLOYEECNAME + " 考勤初始化日期：" + dtCurDate.ToString("yyyy-MM-dd"));

                }
                else
                {
                    Tracer.Debug("初始化设置的例外工作日考勤记录已存在，跳过，" + " 员工姓名" + item_emp.EMPLOYEECNAME + " 考勤初始化日期：" + dtCurDate.ToString("yyyy-MM-dd"));
                    //continue;//如果已考勤直接跳过
                    if (!string.IsNullOrEmpty(entUpdate.ATTENDANCESTATE))
                    {
                        continue;
                    }

                    entUpdate.ATTENDANCESOLUTIONID = entTemp.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID;
                    entUpdate.EMPLOYEEID = item_emp.EMPLOYEEID;
                    entUpdate.EMPLOYEECODE = item_emp.EMPLOYEECODE;
                    entUpdate.EMPLOYEENAME = item_emp.EMPLOYEECNAME;
                    entUpdate.ATTENDANCEDATE = dtCurDate;

                    entUpdate.T_HR_SHIFTDEFINE = item.T_HR_SHIFTDEFINE;

                    //第一段工作时间
                    entUpdate.FIRSTSTARTTIME = item.T_HR_SHIFTDEFINE.FIRSTSTARTTIME;
                    entUpdate.FIRSTENDTIME = item.T_HR_SHIFTDEFINE.FIRSTENDTIME;

                    //第二段工作时间
                    entUpdate.SECONDSTARTTIME = item.T_HR_SHIFTDEFINE.SECONDSTARTTIME;
                    entUpdate.SECONDENDTIME = item.T_HR_SHIFTDEFINE.SECONDENDTIME;

                    //第三段工作时间
                    entUpdate.THIRDSTARTTIME = item.T_HR_SHIFTDEFINE.THIRDSTARTTIME;
                    entUpdate.THIRDENDTIME = item.T_HR_SHIFTDEFINE.THIRDENDTIME;

                    //第四段工作时间
                    entUpdate.FOURTHENDTIME = item.T_HR_SHIFTDEFINE.FOURTHENDTIME;
                    entUpdate.FOURTHSTARTTIME = item.T_HR_SHIFTDEFINE.FOURTHSTARTTIME;

                    //查询公共假期设置判断是否只上半天班公共假期
                    var q = from ent in entWorkDays
                            where ent.STARTDATE == entUpdate.ATTENDANCEDATE
                            select ent;
                    if (q.Count() > 0)
                    {
                        var set = q.FirstOrDefault();
                        if (!string.IsNullOrEmpty(set.ISHALFDAY))
                        {
                            if (set.ISHALFDAY == "1")
                            {
                                if (set.PEROID == "0")//上午
                                {
                                    Tracer.Debug("考勤初始化（修改）-检测设置了半天工作日："
                                               + " 员工：" + entUpdate.EMPLOYEENAME
                                               + " 日期：" + entUpdate.ATTENDANCEDATE
                                               + " 上班分段（0上午）：" + set.PEROID);
                                    entUpdate.NEEDFRISTATTEND = "1";//上午上班
                                    entUpdate.NEEDSECONDATTEND = "0";//下午不上班
                                }
                                else if (set.PEROID == "1")
                                {
                                    Tracer.Debug("考勤初始化（修改）-检测设置了半天工作日："
                                                  + " 员工：" + entUpdate.EMPLOYEENAME
                                                  + " 日期：" + entUpdate.ATTENDANCEDATE
                                                  + " 上班分段（1下午）：" + set.PEROID);
                                    entUpdate.NEEDFRISTATTEND = "0";//上午不上班
                                    entUpdate.NEEDSECONDATTEND = "1";//下午上班
                                }
                            }
                        }
                    }
                    entUpdate.ATTENDANCESTATE = string.Empty;    //新生成的考勤记录，出勤状态为空

                    //权限
                    entUpdate.OWNERCOMPANYID = item_emp.OWNERCOMPANYID;
                    entUpdate.OWNERDEPARTMENTID = item_emp.OWNERDEPARTMENTID;
                    entUpdate.OWNERPOSTID = item_emp.OWNERPOSTID;
                    entUpdate.OWNERID = item_emp.OWNERID;

                    entUpdate.REMARK = entTemp.REMARK;
                    entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                    entUpdate.UPDATEDATE = DateTime.Now;

                    dal.UpdateFromContext(entUpdate);
                }
            }
        }

        /// <summary>
        /// 转化员工状态
        /// </summary>
        /// <param name="strEmployeeState"></param>
        /// <returns></returns>
        private string GetEmployeeState(string strEmployeeState)
        {
            string strRes = string.Empty;
            if (string.IsNullOrEmpty(strEmployeeState))
            {
                return strRes;
            }

            switch (strEmployeeState)
            {
                case "0":
                    strRes = (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString();
                    break;
                case "1":
                    strRes = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
                    break;
                case "2":
                    strRes = string.Empty;
                    break;
                default:
                    strRes = string.Empty;
                    break;
            }

            return strRes;
        }


        #endregion
    }
}
