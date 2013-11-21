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
        public IQueryable<T_HR_OUTAPPLYRECORD> EmployeeOutApplyRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
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

            IQueryable<T_HR_OUTAPPLYRECORD> ents = dal.GetObjects().Include("T_HR_OUTAPPLYCONFIRM");
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
        public IQueryable<T_HR_OUTAPPLYRECORD> GetOutApplyListByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState)
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
        public T_HR_OUTAPPLYRECORD GetOutApplyByID(string strOverTimeRecordId)
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

        public string OutApplySetValue(string msg, T_HR_OUTAPPLYRECORD entity)
        {
            if (entity.ISSAMEDAYRETURN == "0")//非当天往返，设置外出结束时间为当天下班时间
            {
                if (GetAttendanceSolution(entity.EMPLOYEEID, entity.STARTDATE) == null)
                {
                    return "未获取到用户的考勤方案，保存失败";
                }

                AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(entity.EMPLOYEEID, entity.STARTDATE.Value);

                if (entAttendSolAsign.T_HR_ATTENDANCESOLUTION == null)
                {
                    //当前员工没有分配考勤方案，无法提交外出申请
                    msg = msg + "当前员工没有分配考勤方案，无法提交外出申请";
                    Tracer.Debug(msg);
                    return msg;
                }
                else
                {
                    var entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;

                    var tempMaster = from ent in dal.GetObjects<T_HR_SCHEDULINGTEMPLATEMASTER>()
                                     where ent.TEMPLATEMASTERID == entAttendSol.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID
                                     select ent;

                    if (!entAttendSol.T_HR_SCHEDULINGTEMPLATEMASTERReference.IsLoaded)
                    {
                        entAttendSol.T_HR_SCHEDULINGTEMPLATEMASTERReference.Load();
                    }

                    var entsched = from a in dal.GetObjects<T_HR_SCHEDULINGTEMPLATEMASTER>()
                                   join b in dal.GetObjects<T_HR_SCHEDULINGTEMPLATEDETAIL>()
                                   on a.TEMPLATEMASTERID equals b.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID
                                   join c in dal.GetObjects<T_HR_SHIFTDEFINE>()
                                   on b.T_HR_SHIFTDEFINE.SHIFTDEFINEID equals c.SHIFTDEFINEID
                                   where a.TEMPLATEMASTERID == entAttendSol.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID
                                   select c;
                    if (entsched.Count() > 0)
                    {
                        var defineTime = entsched.FirstOrDefault();
                        DateTime ShiftFirstStartTime = new DateTime();
                        if (defineTime.NEEDTHIRDOFFCARD == "2" && !string.IsNullOrEmpty(defineTime.THIRDENDTIME))
                        {
                            ShiftFirstStartTime = DateTime.Parse(defineTime.THIRDENDTIME);//设置3段打卡,第2段下班打卡时间：一般为17:50
                        }
                        else if (defineTime.NEEDSECONDOFFCARD == "2" && !string.IsNullOrEmpty(defineTime.SECONDENDTIME))
                        {
                            ShiftFirstStartTime = DateTime.Parse(defineTime.SECONDENDTIME);//设置2段打卡,第2段下班打卡时间：一般为17:50
                        }
                        else
                        {
                            msg = msg + "外出申请班次定义设置错误，没有找到下班时间定义" + " 考勤方案名：" + entAttendSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME;
                            Tracer.Debug(msg);
                            return msg;
                        }
                        DateTime ShiftstartDateAndTime = new DateTime(entity.STARTDATE.Value.Year, entity.STARTDATE.Value.Month, entity.STARTDATE.Value.Day
                            , ShiftFirstStartTime.Hour, ShiftFirstStartTime.Minute, ShiftFirstStartTime.Second);
                        entity.ENDDATE = ShiftstartDateAndTime;
                    }
                    else
                    {
                        msg = msg + "外出申请班次定义未找到" + " 考勤方案名：" + entAttendSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME;
                        Tracer.Debug(msg);
                        return msg;
                    }
                }

                if (entity.ENDDATE == null)
                {
                    msg = msg + "外出申请为非当天往返，但是结束时间未找到，请检查考勤方案排版设置是否正确。" + " 考勤方案名：" + entAttendSolAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME;
                    Tracer.Debug(msg);
                    return msg;
                }
            }
            //计算外出时长
            string dTotalHours = string.Empty;
            string strMsg = CalculateOverTimeHours(entity.EMPLOYEEID, entity.STARTDATE.Value, entity.ENDDATE.Value, ref dTotalHours);

            entity.OUTAPLLYTIMES = dTotalHours.ToString();
            return strMsg;
        }

        /// <summary>
        /// 新增加班申请记录
        /// </summary>
        /// <param name="entity">加班修改申请记录实体</param>
        /// <returns></returns>
        public string AddOutApply(T_HR_OUTAPPLYRECORD entity)
        {
            string msg = "添加外出申请单:" + entity.EMPLOYEENAME + " 外出时间：" + entity.STARTDATE
                + " 外出结束时间：" + entity.ENDDATE + " 外出原因：" + entity.REASON
                + " 是否当天往返：0为否:" + entity.ISSAMEDAYRETURN;
            try
            {
                string strMsg = string.Empty;
                strMsg=OutApplySetValue(msg, entity);
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    msg = msg + strMsg;
                    Tracer.Debug(msg);
                    return strMsg;
                }

                if (!Add(entity))
                {
                    msg = msg + " Add(entity)失败";
                    Tracer.Debug(msg);
                    strMsg = "{ERROR}";
                }
                strMsg = "ok";
                return strMsg;
            }
            catch (Exception ex)
            {
                Utility.SaveLog(msg+ex.ToString());
                return "{ERROR}";
            }
        }

        public int UpdateOutApply(T_HR_OUTAPPLYRECORD entity)
        {
            var ent = GetOutApplyByID(entity.OUTAPPLYID);
            if (ent != null)
            {
                string msg = "修改外出申请单:" + entity.EMPLOYEENAME + " 外出时间：" + entity.STARTDATE
              + " 外出结束时间：" + entity.ENDDATE + " 外出原因：" + entity.REASON
              + " 是否当天往返：0为否:" + entity.ISSAMEDAYRETURN;
                //计算外出时长
                ent.ISSAMEDAYRETURN = entity.ISSAMEDAYRETURN;
                ent.STARTDATE = entity.STARTDATE;
                ent.ENDDATE = entity.ENDDATE;
                ent.REASON = entity.REASON;
                ent.CHECKSTATE = entity.CHECKSTATE;
                ent.UPDATEDATE = DateTime.Now;
                string strMsg = OutApplySetValue(msg, ent);

                //Utility.CloneEntity(entity, ent);
                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    Tracer.Debug(strMsg);
                    return 0;
                }
                return dal.Update(ent);
            }
            else
                return 0;
        }

        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        public int DeleteOutApply(string[] strOverTimeRecordId)
        {
            try
            {
                foreach (var id in strOverTimeRecordId)
                {
                    var ent = GetOutApplyByID(id);
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
        public string AuditOutApply(string strOverTimeRecordID, string strCheckState)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strOverTimeRecordID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{REQUIREDFIELDS}";
                }

                T_HR_OUTAPPLYRECORD entOTRd = GetOutApplyByID(strOverTimeRecordID);

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

                try
                {
                    Tracer.Debug("流程更新外出申请,外出员工"+entOTRd.EMPLOYEENAME+ " 外出时间："+entOTRd.STARTDATE+" 外出原因："+entOTRd.REASON
                        +" 审核状态："+entOTRd.CHECKSTATE);
                    if (entOTRd.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        T_HR_OUTAPPLYCONFIRM confirm = new T_HR_OUTAPPLYCONFIRM();
                        confirm.OUTAPPLYCONFIRMID = Guid.NewGuid().ToString();
                        confirm.STARTDATE = entOTRd.STARTDATE;
                        confirm.ENDDATE = entOTRd.ENDDATE;
                        confirm.CHECKSTATE = "0";
                        confirm.OWNERCOMPANYID = entOTRd.OWNERCOMPANYID;
                        confirm.OWNERDEPARTMENTID = entOTRd.OWNERDEPARTMENTID;
                        confirm.OWNERID = entOTRd.OWNERID;
                        confirm.CREATECOMPANYID = entOTRd.CREATECOMPANYID;
                        confirm.CREATEDATE = DateTime.Now;
                        confirm.CREATEDEPARTMENTID = entOTRd.CREATEDEPARTMENTID;
                        confirm.CREATEPOSTID = entOTRd.CREATEPOSTID;
                        confirm.CREATEUSERID = entOTRd.CREATEUSERID;
                        confirm.EMPLOYEEID = entOTRd.EMPLOYEEID;
                        confirm.EMPLOYEENAME = entOTRd.EMPLOYEENAME;
                        //confirm.T_HR_OUTAPPLYRECORD = entOTRd;
                        confirm.T_HR_OUTAPPLYRECORDReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_OUTAPPLYRECORD", "OUTAPPLYID", entOTRd.OUTAPPLYID);

                        OutApplyConfirmBLL bll = new OutApplyConfirmBLL();
                        if (bll.Add(confirm))
                        {
                            Tracer.Debug("外出申请审核新增外出确认单成功,外出确认员工" + confirm.EMPLOYEENAME + " 外出确认时间：" + confirm.STARTDATE + " 外出确认原因：" + confirm.REMARK);

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
                        else
                        {
                            Tracer.Debug("外出申请审核新增外出确认单保存失败！");
                        }

                        return "{SAVESUCCESSED}";
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug(ex.ToString());
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
        public string CalculateOverTimeHours(string strEmployeeId, DateTime dtOTStart, DateTime dtOTEnd, ref string dOverTimeHours)
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
            if (dTotalOverTimeHours > 0)
            {
                dOverTimeHours = Math.Round(dTotalOverTimeHours,0) + "小时";
            }
            if (ts.Minutes > 0)
            {
                dOverTimeHours = dOverTimeHours + ts.Minutes + "分";
            }
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
                strMsg = AuditOutApply(EntityKeyValue, CheckState);
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

        #region 审核手机版元数据构造

        public string GetXmlString(string Formid)
        {
            T_HR_OUTAPPLYRECORD Info = GetOutApplyByID(Formid);
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            string checkStateDict
                = PermClient.GetDictionaryByCategoryArray(new string[] { "CHECKSTATE" }).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
            checkState = checkStateDict == null ? "" : checkStateDict;

            SMT.SaaS.BLLCommonServices.PersonnelWS.V_EMPLOYEEPOST employee
                = SMT.SaaS.BLLCommonServices.Utility.GetEmployeeOrgByid(Info.EMPLOYEEID);
            decimal? postlevelValue = Convert.ToDecimal(employee.EMPLOYEEPOSTS[0].POSTLEVEL.ToString());
            string postLevelName = string.Empty;
            string postLevelDict
                 = PermClient.GetDictionaryByCategoryArray(new string[] { "CHECKSTATE" }).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
            postLevelName = postLevelDict == null ? "" : postLevelDict;

            //decimal? overTimeValue = Convert.ToDecimal(Info);
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "CURRENTEMPLOYEENAME", employee.T_HR_EMPLOYEE.EMPLOYEECNAME, employee.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "CHECKSTATE", Info.CHECKSTATE, checkState));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "POSTLEVEL", employee.EMPLOYEEPOSTS[0].POSTLEVEL.ToString(), postLevelName));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "EMPLOYEENAM", Info.EMPLOYEENAME, Info.EMPLOYEENAME));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, Info.EMPLOYEENAME));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, Info.OWNERDEPARTMENTID));
            AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERPOSTID", Info.OWNERPOSTID, Info.OWNERPOSTID));

            string StrSource = GetBusinessObject("T_HR_OUTAPPLYRECORD");
            string outApplyXML = mx.TableToXml(Info, null, StrSource, AutoList);

            return outApplyXML;
        }
        #endregion
    }
}
