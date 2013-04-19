
/*
 * 文件名：AbnormRecordBLL.cs
 * 作  用：T_HR_EMPLOYEEABNORMRECORD 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-6-12 18:58:21
 * 修改人：
 * 修改时间：
 */

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

namespace SMT.HRM.BLL
{
    public class AbnormRecordBLL : BaseBll<T_HR_EMPLOYEEABNORMRECORD>
    {
        public AbnormRecordBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取员工考勤异常信息
        /// </summary>
        /// <param name="strAbnormRecordId">主键索引</param>
        /// <returns></returns>
        public T_HR_EMPLOYEEABNORMRECORD GetAbnormRecordByID(string strAbnormRecordId)
        {
            if (string.IsNullOrEmpty(strAbnormRecordId))
            {
                return null;
            }

            AbnormRecordDAL dalAbnormRecord = new AbnormRecordDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strAbnormRecordId))
            {
                strfilter.Append(" ABNORMRECORDID == @0");
                objArgs.Add(strAbnormRecordId);
            }

            T_HR_EMPLOYEEABNORMRECORD entRd = dalAbnormRecord.GetAbnormRecordRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据员工ID获取该员工所有未签卡记录
        /// </summary>
        /// <param name="EmployeeID">员工ID</param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordByEmployeeID(string employeeID)
        {
            AttendanceSolutionAsignBLL bll = new AttendanceSolutionAsignBLL();
            string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
            string strSingInState = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();

            var temp = bll.GetAttendanceSolutionAsignByEmployeeID(employeeID);
            if (temp == null)
            {
                return null;
            }

            DateTime currDate = DateTime.Now.Date;
            DateTime dStart;
            //是否当月结算:1表示否,2表示是
            string strTag = temp.T_HR_ATTENDANCESOLUTION.ISCURRENTMONTH;
            if (strTag == "1")
            {
                if (currDate.Day <= Convert.ToInt32(temp.T_HR_ATTENDANCESOLUTION.SETTLEMENTDATE))
                {
                    dStart = currDate.AddMonths(-1).AddDays(-currDate.Day + 1);
                }
                else
                {
                    dStart = DateTime.Parse(currDate.AddMonths(-1).ToString("yyyy-MM") + "-1");
                }
            }
            else
            {
                dStart = DateTime.Parse(currDate.ToString("yyyy-MM") + "-1");
            }
            return dal.GetObjects().Include("T_HR_ATTENDANCERECORD").Where(s =>
                    s.T_HR_ATTENDANCERECORD.EMPLOYEEID == employeeID && s.ABNORMCATEGORY == strAbnormCategory && s.SINGINSTATE == strSingInState
                    && (s.ABNORMALDATE >= dStart && s.ABNORMALDATE <= currDate)).OrderBy("ABNORMALDATE");

        }

        /// <summary>
        /// 根据员工ID，查询指定时间段内的异常记录
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">异常日期查询起始日</param>
        /// <param name="dtEnd">异常日期查询截止日</param>
        /// <returns>异常记录</returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByEmpIdAndDate(string strEmployeeID, string strAbnormCategory, DateTime dtStart, DateTime dtEnd)
        {
            string strAttState = (Convert.ToInt32(Common.AttendanceState.Abnormal) + 1).ToString();
            var q = from e in dal.GetObjects().Include("T_HR_ATTENDANCERECORD")
                    where e.T_HR_ATTENDANCERECORD.EMPLOYEEID == strEmployeeID && e.ABNORMCATEGORY == strAbnormCategory && e.T_HR_ATTENDANCERECORD.ATTENDANCESTATE == strAttState && e.ABNORMALDATE >= dtStart && e.ABNORMALDATE <= dtEnd
                    select e;
            return q;
        }

        /// <summary>
        /// 根据员工ID，查询指定时间段内的异常记录
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">异常日期查询起始日</param>
        /// <param name="dtEnd">异常日期查询截止日</param>
        /// <returns>异常记录</returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByEmpIdAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd)
        {
            //alter by ken 2013.4.3
            //string strAttState = (Convert.ToInt32(Common.AttendanceState.Abnormal) + 1).ToString();
            var q = from e in dal.GetObjects().Include("T_HR_ATTENDANCERECORD")
                    where e.T_HR_ATTENDANCERECORD.EMPLOYEEID == strEmployeeID && e.ABNORMALDATE >= dtStart && e.ABNORMALDATE <= dtEnd
                    select e;
            return q;
        }

        /// <summary>
        /// 获取员工异常记录信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strEmployeeID">异常记录对应关联的员工序号</param>
        /// <param name="strSignInState">签卡状态(参数为"1"，则取未签卡的异常；参数为"2"，则取对应已签卡的异常)</param>
        /// <param name="strCurDateMonth">当前日期(年-月)</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工异常记录信息</returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAllAbnormRecordRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSignInState,
            string strCurStartDate, string strCurEndDate, string strSortKey)
        {
            AbnormRecordDAL dalAbnormRecord = new AbnormRecordDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" T_HR_ATTENDANCERECORD.EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            if (!string.IsNullOrEmpty(strSignInState))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                strfilter.Append(" (SINGINSTATE == @" + objArgs.Count().ToString());
                objArgs.Add("1");
                strfilter.Append(" OR ");
                strfilter.Append(" SINGINSTATE == @" + objArgs.Count().ToString() + ")");
                objArgs.Add("3");
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ABNORMALDATE ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEEABNORMRECORD");

            var q = dalAbnormRecord.GetAbnormRecordRdListByMultSearch(strOrderBy, strCurStartDate, strCurEndDate, filterString, objArgs.ToArray());
            return q;
        }




        /// <summary>
        /// 获取员工异常记录信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strEmployeeID">异常记录对应关联的员工序号</param>
        /// <param name="strSignInState">签卡状态(参数为"1"，则取未签卡的异常；参数为"2"，则取对应已签卡的异常)</param>
        /// <param name="strCurDateMonth">当前日期(年-月)</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工异常记录信息</returns>
        public IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSignInState,
           string strCurStartDate, string strCurEndDate, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAbnormRecordRdListByMultSearch(strOwnerID, strEmployeeID, strSignInState, strCurStartDate, strCurEndDate, strSortKey);
            q = q.OrderBy(m => m.ABNORMALDATE);

            return Utility.Pager<T_HR_EMPLOYEEABNORMRECORD>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增员工考勤异常信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddAbnormRecord(T_HR_EMPLOYEEABNORMRECORD entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                if (entTemp.T_HR_ATTENDANCERECORDReference.EntityKey == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                string strAttendRdId = string.Empty;
                if (entTemp.T_HR_ATTENDANCERECORDReference.EntityKey != null)
                {
                    strAttendRdId = entTemp.T_HR_ATTENDANCERECORDReference.EntityKey.EntityKeyValues[0].Value.ToString();
                }

                if (entTemp.T_HR_ATTENDANCERECORD != null)
                {
                    strAttendRdId = entTemp.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID;
                }

                if (string.IsNullOrWhiteSpace(strAttendRdId))
                {
                    return "{REQUIREDFIELDS}";
                }

                var ents = from n in dal.GetObjects().Include("T_HR_ATTENDANCERECORD")
                           where n.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID == strAttendRdId
                                && n.ABNORMCATEGORY == entTemp.ABNORMCATEGORY
                                && n.ABNORMALDATE == entTemp.ABNORMALDATE
                                && n.ATTENDPERIOD == entTemp.ATTENDPERIOD
                           select n;

                if (ents == null)
                {
                    dal.AddToContext(entTemp);
                    dal.SaveContextChanges();

                    return "{SAVESUCCESSED}";
                }

                if (ents.Count() == 0)
                {
                    dal.AddToContext(entTemp);
                    dal.SaveContextChanges();

                    return "{SAVESUCCESSED}";
                }

                ModifyAbnormRecord(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改员工考勤异常信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyAbnormRecord(T_HR_EMPLOYEEABNORMRECORD entTemp)
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

                strFilter.Append(" ABNORMRECORDID == @0");

                objArgs.Add(entTemp.ABNORMRECORDID);

                AbnormRecordDAL dalAbnormRecord = new AbnormRecordDAL();
                flag = dalAbnormRecord.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEEABNORMRECORD entUpdate = dalAbnormRecord.GetAbnormRecordRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalAbnormRecord.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除员工考勤异常信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string RemoveAbnormRecord(string strAbnormRecordId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strAbnormRecordId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" ABNORMRECORDID == @0");

                objArgs.Add(strAbnormRecordId);

                AbnormRecordDAL dalAbnormRecord = new AbnormRecordDAL();
                flag = dalAbnormRecord.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEEABNORMRECORD entDel = dalAbnormRecord.GetAbnormRecordRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalAbnormRecord.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion


        /// <summary>
        /// 根据员工指纹编号，计算该员工指定月份考勤异常情况
        /// </summary>
        /// <param name="strFingertId">员工指纹编号</param>
        /// <param name="strPunchMonth">月份</param>
        /// <param name="strMsg">返回处理消息</param>
        public void CheckAbnormRecordByFingertId(string strFingertId, string strPunchMonth, ref string strMsg)
        {
            DateTime dtCheck = new DateTime();
            DateTime dtStart = new DateTime(), dtEnd = new DateTime();
            DateTime.TryParse(strPunchMonth + "-1", out dtStart);
            if (dtStart <= dtCheck)
            {
                return;
            }

            dtEnd = dtStart.AddMonths(1).AddDays(-1);
            //DateTime.TryParse("2011-10-16", out dtEnd);

            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

            EmployeeBLL bllEmployee = new EmployeeBLL();
            T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByFingerPrintID(strFingertId);

            if (entEmployee == null)
            {
                strMsg = "当前指纹编号无对应的员工";
                return;
            }

            entEmployees.Add(entEmployee);

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strMsg);
        }

        /// <summary>
        /// 根据员工ID，计算该员工指定月份考勤异常情况
        /// </summary>
        /// <param name="strFingertId">员工指纹编号</param>
        /// <param name="strPunchMonth">月份</param>
        /// <param name="strMsg">返回处理消息</param>
        public void CheckAbnormRecordByEmployeeIds(string strEmployeeIds, string strPunchMonth, ref string strMsg)
        {
            DateTime dtCheck = new DateTime();
            DateTime dtStart = new DateTime(), dtEnd = new DateTime();
            DateTime.TryParse(strPunchMonth + "-1", out dtStart);
            if (dtStart <= dtCheck)
            {
                return;
            }

            dtEnd = dtStart.AddMonths(1).AddDays(-1);

            CheckAbnormRdForEmployeesByDate(strEmployeeIds, dtStart, dtEnd, ref strMsg);
        }

        /// <summary>
        /// 根据员工ID，计算该公司指定时间段内员工考勤异常情况
        /// </summary>
        /// <param name="strEmployeeIds"></param>
        /// <param name="dtPunchFrom"></param>
        /// <param name="dtPunchTo"></param>
        /// <param name="strMsg"></param>
        public void CheckAbnormRdForEmployeesByDate(string strEmployeeIds, DateTime dtPunchFrom, DateTime dtPunchTo, ref string strMsg)
        {
            if (dtPunchFrom > dtPunchTo)
            {
                return;
            }

            string[] strlist = strEmployeeIds.Split(',');

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByIDs(strlist);

            if (entEmployees == null)
            {
                strMsg = "当前员工编号无对应的员工";
                return;
            }

            if (entEmployees.Count() == 0)
            {
                strMsg = "当前员工编号无对应的员工";
                return;
            }

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtPunchFrom, dtPunchTo, ref strMsg);
        }

        /// <summary>
        /// 根据公司ID，计算该公司指定月份员工考勤异常情况
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <param name="strPunchMonth"></param>
        /// <param name="strMsg"></param>
        public void CheckAbnormRecordByCompanyId(string strCompanyId, string strPunchMonth, ref string strMsg)
        {
            DateTime dtCheck = new DateTime();
            DateTime dtStart = new DateTime(), dtEnd = new DateTime();
            DateTime.TryParse(strPunchMonth + "-1", out dtStart);
            if (dtStart <= dtCheck)
            {
                return;
            }

            dtEnd = dtStart.AddMonths(1).AddDays(-1);

            CheckAbnormRdForCompanyByDate(strCompanyId, dtStart, dtEnd, ref strMsg);
        }

        /// <summary>
        /// 根据公司ID，计算该公司指定时间段内员工考勤异常情况
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <param name="dtPunchFrom"></param>
        /// <param name="dtPunchTo"></param>
        /// <param name="strMsg"></param>
        public void CheckAbnormRdForCompanyByDate(string strCompanyId, DateTime dtPunchFrom, DateTime dtPunchTo, ref string strMsg)
        {
            if (dtPunchFrom > dtPunchTo)
            {
                return;
            }

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();
            var ents = bllEmployee.GetEmployeeByCompanyID(strCompanyId, dtPunchFrom);
            if (ents == null)
            {
                strMsg = "当前查询公司无员工记录";
                return;
            }

            if (ents.Count() == 0)
            {
                strMsg = "当前查询公司无员工记录";
                return;
            }

            entEmployees = ents.ToList();

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtPunchFrom, dtPunchTo, ref strMsg);
        }

        /// <summary>
        /// 根据指定范围的员工和时间段，检查各员工的考勤记录是否存在异常
        /// </summary>
        /// <param name="entEmployees">根据指定范围的员工</param>
        /// <param name="dtStart">考勤检查起始日期</param>
        /// <param name="dtEnd">考勤检查截止日期</param>
        /// <param name="strMsg">检查后返回的消息</param>
        public void CheckAbnormRecordForEmployees(List<T_HR_EMPLOYEE> entEmployees, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            foreach (T_HR_EMPLOYEE entEmployee in entEmployees)
            {
                bool isAbnorm = false;
                string strEmployeeID = entEmployee.EMPLOYEEID;
                //获取导入打卡记录对应时间段的考勤记录，以便进行比对
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                IQueryable<T_HR_ATTENDANCERECORD> entAttRds = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(entEmployee.OWNERCOMPANYID, strEmployeeID, dtStart, dtEnd);

                if (entAttRds.Count() == 0)
                {
                    continue;
                }

                string strSortKey = "PUNCHDATE";
                ClockInRecordBLL bllClockInRecord = new ClockInRecordBLL();
                IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = bllClockInRecord.GetAllClockInRdListByMultSearch(string.Empty, string.Empty, string.Empty, strEmployeeID, dtStart.ToString(), dtEnd.ToString(), strSortKey);

                CheckAbnormRecord(entAttRds, entClockInRecords, ref strMsg, ref isAbnorm);

                if (isAbnorm && strMsg == "{SAVESUCCESSED}")
                {
                    AbnormRecordCheckAlarm(entEmployee.EMPLOYEEID, dtStart.ToString("yyyy-MM") + "-1", dtEnd.ToString("yyyy-MM-dd"));
                }
            }
        }

        /// <summary>
        /// 考勤异常提醒消息
        /// </summary>
        /// <param name="employeeCheck"></param>
        public void AbnormRecordCheckAlarm(string strEmployeeId)
        {

            string submitName = string.Empty;
            string strAttendState = (Convert.ToInt32(Common.AttendanceState.Abnormal) + 1).ToString();
            var ents = from a in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                       where a.EMPLOYEEID == strEmployeeId && a.ATTENDANCESTATE == strAttendState
                       select a;

            if (ents.Count() == 0)
            {
                return;
            }

            T_HR_ATTENDANCERECORD entTemp = ents.FirstOrDefault();

            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = Guid.NewGuid().ToString();
            userMsg.UserID = entTemp.EMPLOYEEID;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            if (ents.Count() > 0) submitName = ents.FirstOrDefault().EMPLOYEENAME;
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEESIGNINRECORD", Utility.ObjListToXml(entTemp, "HR", submitName), EngineWS.MsgType.Msg);
        }
        /// <summary>
        ///  考勤异常提醒xml
        /// </summary>
        /// <param name="employeeCheck"></param>
        public void GetAbnormRecordCheckEngineXml(T_HR_EMPLOYEE entTemp)
        {
            DateTime dtStart = System.DateTime.Now;
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.OWNERCOMPANYID);
            objArds.Add("HR");
            objArds.Add("T_HR_EMPLOYEE");
            objArds.Add(entTemp.EMPLOYEEID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(dtStart.ToString("HH:mm"));
            objArds.Add("");
            objArds.Add("");
            objArds.Add("你的考勤数据出现异常，请及时处理");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para Name=\"AbnormRecordCheckAlarm\" Name=\"EMPLOYEEID\" Value=\"" + entTemp.EMPLOYEEID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("basicHttpBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }


        #region 私有方法---检查考勤日每天各工作段的出勤情况
        /// <summary>
        /// 检查上班打卡情况
        /// </summary>
        /// <param name="entAttRds"></param>
        /// <param name="entClockInRecords"></param>
        /// <param name="strMsg"></param>
        /// <param name="bIsAbnorm"></param>
        private void CheckAbnormRecord(IQueryable<T_HR_ATTENDANCERECORD> entAttRds, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strMsg, ref bool bIsAbnorm)
        {
            try
            {
                //对考勤记录进行轮询
                foreach (T_HR_ATTENDANCERECORD item in entAttRds)
                {
                    //获取对应的考勤方案
                    AttendanceSolutionBLL bllAttSol = new AttendanceSolutionBLL();
                    T_HR_ATTENDANCESOLUTION entAttSol = bllAttSol.GetAttendanceSolutionByID(item.ATTENDANCESOLUTIONID);
                    AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();

                    if (entAttSol == null)
                    {
                        continue;
                    }

                    //考勤方案设定为不需要考勤时，跳过考勤异常检查
                    if (entAttSol.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())
                    {
                        item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString();
                        bllAttendanceRecord.ModifyAttRd(item);
                        continue;
                    }


                    //考勤记录的考勤状态只有为空时，才可以检查是否存在考勤异常，并对异常进行记录
                    if (!string.IsNullOrWhiteSpace(item.ATTENDANCESTATE))
                    {
                        CheckAttendStateWithSpecialCase(item, entClockInRds, ref strMsg, ref bIsAbnorm);
                        continue;
                    }

                    string strAbnormCategory = string.Empty;

                    //检查第一段工作期，打卡情况
                    CheckAbnormRecordByFirstWorkTime(item, entClockInRds, ref strAbnormCategory);

                    //检查第二段上班，打卡情况
                    CheckAbnormRecordBySecondWorkTime(item, entClockInRds, ref strAbnormCategory);

                    //检查第三段上班，打卡情况
                    CheckAbnormRecordByThirdWorkTime(item, entClockInRds, ref strAbnormCategory);

                    //检查第四段上班，打卡情况
                    CheckAbnormRecordByFourthWorkTime(item, entClockInRds, ref strAbnormCategory);

                    if (string.IsNullOrEmpty(strAbnormCategory))
                    {
                        item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString();
                    }
                    else
                    {
                        bIsAbnorm = true;
                        EmployeeEvectionRecordBLL bllEvectionRd = new EmployeeEvectionRecordBLL();
                        T_HR_EMPLOYEEEVECTIONRECORD entEvectionRd = bllEvectionRd.GetEmployeeEvectionRdByEmployeeIdAndDate(item.EMPLOYEEID, item.ATTENDANCEDATE);

                        if (entEvectionRd != null)
                        {
                            item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Regular) + 1).ToString();
                        }
                        else
                        {
                            item.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Abnormal) + 1).ToString();
                        }
                    }

                    bllAttendanceRecord.ModifyAttRd(item);
                }

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                Utility.SaveLog("调用私有函数CheckAbnormRecord(代码行起点位置：631)发生错误，报错时间为:" + DateTime.Now.ToString() + "，报错原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 检查员工在一天内是否出现多个考勤状态的情况
        /// </summary>
        /// <param name="entAttRd">当天的考勤初始化记录</param>
        /// <param name="entClockInRds">打卡原始记录</param>
        /// <param name="strMsg">处理结果的消息</param>
        /// <param name="bIsAbnorm">是否出现考勤异常的标志(true/false)</param>
        private void CheckAttendStateWithSpecialCase(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strMsg, ref bool bIsAbnorm)
        {
            bool bflag = false;

            //获取请假记录使用的起止时间
            DateTime dtStartDate = entAttRd.ATTENDANCEDATE.Value;
            DateTime dtEndDate = entAttRd.ATTENDANCEDATE.Value.AddDays(1).AddSeconds(-1);

            //检查出差、请假记录使用的的上班时间（校验比对当前出差，请假是否为全天）
            DateTime dtCheckStart = DateTime.Parse(dtStartDate.ToString("yyyy-MM-dd"));
            DateTime dtCheckEnd = DateTime.Parse(dtStartDate.ToString("yyyy-MM-dd"));

            //获取当天的正常上下班时间
            AttendanceSolutionBLL bllAttSol = new AttendanceSolutionBLL();
            bflag = bllAttSol.GetAttendDateWorkTime(entAttRd.ATTENDANCESOLUTIONID, ref dtCheckStart, ref dtCheckEnd);

            //上下班时间未获取到，则比对终止，返回
            if(!bflag)
            {
                return;
            }

            //查询出差记录，检查当天存在出差情况
            EmployeeEvectionRecordBLL bllEvectionRd = new EmployeeEvectionRecordBLL();
            T_HR_EMPLOYEEEVECTIONRECORD entEvectionRd = bllEvectionRd.GetEmployeeEvectionRdByEmployeeIdAndDate(entAttRd.EMPLOYEEID, entAttRd.ATTENDANCEDATE);

            //如果有出差记录，就判断出差是否为全天
            if (entEvectionRd != null)
            {
                //如果为全天，则不再进行考勤异常检查
                if(entEvectionRd.STARTDATE <= dtCheckStart && entEvectionRd.ENDDATE >= dtCheckEnd)
                {
                    return;
                }
            }

            //查询请假记录，检查当天存在请假情况
            EmployeeLeaveRecordBLL bllLeaveRd = new EmployeeLeaveRecordBLL();
            IQueryable<T_HR_EMPLOYEELEAVERECORD> entLeaveRds = bllLeaveRd.GetEmployeeLeaveRdListByEmployeeIDAndDate(entAttRd.EMPLOYEEID, dtStartDate, dtEndDate, "2");

            if (entLeaveRds != null)
            {
                //当天不存在出差和请假记录时，就不需要重复进行检查了，直接中断返回
                if (bflag)
                {
                    return;
                }
            }

            List<T_HR_EMPLOYEELEAVERECORD> entLeaveRdList = entLeaveRds.ToList();
            if (entLeaveRdList.Count() > 0)
            {
                foreach (T_HR_EMPLOYEELEAVERECORD item in entLeaveRdList)
                {
                    if (item.STARTDATETIME <= dtCheckStart && item.ENDDATETIME >= dtCheckEnd)
                    {
                        bflag = true;
                        break;
                    }
                }
            }

            if (bflag)
            {
                return;
            }
            
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty, strAbnormCategory = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime,
                strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime, strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);

            if (!string.IsNullOrEmpty(strAbnormCategory))
            {
                bIsAbnorm = true;

                entAttRd.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Mix) + 1).ToString();
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                bllAttendanceRecord.ModifyAttRd(entAttRd);
            }

            strMsg = "{SAVESUCCESSED}";
        }

        /// <summary>
        /// 检查第一段上班打卡情况
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByFirstWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFIRSTOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FIRSTOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime,
                strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime, strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第二段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordBySecondWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDSECONDCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDSECONDOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;


            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.SECONDOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第三段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByThirdWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDTHIRDCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDTHIRDOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.THIRDOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 检查第四段工作时间考勤是否存在异常
        /// </summary>
        /// <param name="entAttRd"></param>
        /// <param name="entClockInRds"></param>
        /// <param name="strAbnormCategory"></param>
        private void CheckAbnormRecordByFourthWorkTime(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, ref string strAbnormCategory)
        {
            string strNeedCard = string.Empty, strNeedOffCard = string.Empty, strStartAttendPeriod = string.Empty, strOffAttendPeriod = string.Empty;
            strNeedCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFOURTHCARD;
            strNeedOffCard = entAttRd.T_HR_SHIFTDEFINE.NEEDFOURTHOFFCARD;
            strStartAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME);
            strOffAttendPeriod = GetAttendPeriod(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME);

            string strAttStartTime = string.Empty, strAttEndTime = string.Empty, strAttCardStartTime = string.Empty;
            string strAttCardEndTime = string.Empty, strAttOffCardStartTime = string.Empty, strAttOffCardEndTime = string.Empty;

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME))
            {
                strAttStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME))
            {
                strAttEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDSTARTTIME))
            {
                strAttCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDENDTIME))
            {
                strAttCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHCARDENDTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDSTARTTIME))
            {
                strAttOffCardStartTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDSTARTTIME).ToString("HH:mm");
            }

            if (!string.IsNullOrEmpty(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDENDTIME))
            {
                strAttOffCardEndTime = entAttRd.ATTENDANCEDATE.Value.ToString("yyyy-MM-d") + " " + DateTime.Parse(entAttRd.T_HR_SHIFTDEFINE.FOURTHOFFCARDENDTIME).ToString("HH:mm");
            }

            CheckAbnormRecordWithShiftDefine(entAttRd, entClockInRds, strStartAttendPeriod, strOffAttendPeriod, strAttStartTime, strAttEndTime, strAttCardStartTime, strAttCardEndTime, strAttOffCardStartTime,
                strAttOffCardEndTime, strNeedCard, strNeedOffCard, ref strAbnormCategory);
        }

        /// <summary>
        /// 判断该段上班及下班考勤是否存在考勤异常
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="dtAttStart">考勤记录设定的上班时间</param>
        /// <param name="dtAttEnd">考勤记录设定的下班时间</param>
        /// <param name="dtAttCardStart">考勤记录设定的上班打卡有效起始时间</param>
        /// <param name="dtAttCardEnd">考勤记录设定的上班打卡有效截止时间</param>
        /// <param name="dtAttOffCardStart">考勤记录设定的下班打卡有效起始时间</param>
        /// <param name="dtAttOffCardEnd">考勤记录设定的下班打卡有效截止时间</param>
        private void CheckAbnormRecordWithShiftDefine(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds, string strStartAttendPeriod,
            string strOffAttendPeriod, string strAttStartTime, string strAttEndTime, string strAttCardStartTime, string strAttCardEndTime, string strAttOffCardStartTime,
            string strAttOffCardEndTime, string strNeedCard, string strNeedOffCard, ref string strAbnormCategory)
        {
            bool bCheck = false; //判断该段工作期是否存在未刷卡的标志位;
            string strNo = string.Empty, strYes = string.Empty;

            strNo = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
            strYes = (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString();

            //上下班都不打卡，则不记录该段工作期的异常
            if (strNeedCard == strNo && strNeedOffCard == strNo)
            {
                return;
            }

            StringBuilder strTemp = new StringBuilder();
            if (strNeedCard == strYes)
            {
                CheckAbnormRecordWithWorkStart(entAttRd, entClockInRds, strStartAttendPeriod, strAttStartTime, strAttCardStartTime, strAttCardEndTime, ref bCheck, ref strTemp, ref strAbnormCategory);
            }

            if (strNeedOffCard == strYes)
            {
                CheckAbnormRecordWithWorkOff(entAttRd, entClockInRds, strOffAttendPeriod, strAttEndTime, strAttOffCardStartTime, strAttOffCardEndTime, ref bCheck, ref strTemp, ref strAbnormCategory);
            }

            if (bCheck)
            {
                string strFlag = strTemp.ToString();
                if (strFlag == "START,")
                {
                    TimeSpan ts = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strStartAttendPeriod, strReasonCategory);
                }
                else if (strFlag == "END,")
                {
                    TimeSpan ts = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strOffAttendPeriod, strReasonCategory);
                }
                else if (strFlag == "START,END,")
                {
                    TimeSpan tsStart = DateTime.Parse(strAttStartTime).Subtract(DateTime.Parse(strAttStartTime));
                    TimeSpan tsEnd = DateTime.Parse(strAttEndTime).Subtract(DateTime.Parse(strAttStartTime));
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
                    strReasonCategory = string.Empty;
                    CreateAbnormRecordByCheckClockInRd(entAttRd, tsStart, strAbnormCategory, strStartAttendPeriod, strReasonCategory);
                    CreateAbnormRecordByCheckClockInRd(entAttRd, tsEnd, strAbnormCategory, strOffAttendPeriod, strReasonCategory);
                }
            }
        }

        /// <summary>
        /// 检查上班是否存在异常情况
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="strAttStartTime">考勤记录设定的上班时间</param>
        /// <param name="strAttCardStartTime">考勤记录设定的上班打卡有效起始时间</param>
        /// <param name="strAttCardEndTime">考勤记录设定的上班打卡有效截止时间</param>
        /// <param name="bCheck">判断该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strTemp">记录该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        private void CheckAbnormRecordWithWorkStart(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInList, string strAttendPeriod,
            string strAttStartTime, string strAttCardStartTime, string strAttCardEndTime, ref bool bCheck, ref StringBuilder strTemp, ref string strAbnormCategory)
        {
            DateTime dtAttStart = new DateTime();
            DateTime dtAttCardStart = new DateTime();
            DateTime dtAttCardEnd = new DateTime();

            DateTime.TryParse(strAttStartTime.Trim(), out dtAttStart);
            DateTime.TryParse(strAttCardStartTime.Trim(), out dtAttCardStart);
            DateTime.TryParse(strAttCardEndTime.Trim(), out dtAttCardEnd);

            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

            foreach (T_HR_EMPLOYEECLOCKINRECORD item in entClockInList)
            {
                item.PUNCHDATE = DateTime.Parse(item.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + item.PUNCHTIME);
                entTemps.Add(item);
            }

            //先检查该段上班
            var cls = from c in entTemps
                      where c.PUNCHDATE >= dtAttCardStart && c.PUNCHDATE <= dtAttCardEnd
                      orderby c.PUNCHDATE
                      select c;

            //无记录，即视为缺勤
            if (cls.Count() == 0)
            {
                if (CheckLeaveRecord(entAttRd, dtAttCardStart, dtAttCardEnd) == true)
                {
                    return;
                }
                bCheck = true;
                strTemp.Append("START,");
            }
            else
            {
                T_HR_EMPLOYEECLOCKINRECORD entStart = cls.FirstOrDefault();
                DateTime dtStartPunch = entStart.PUNCHDATE.Value;

                //判断迟到
                if (dtAttStart < dtStartPunch && dtAttCardEnd >= dtStartPunch)
                {
                    TimeSpan ts = dtStartPunch.Subtract(dtAttStart);
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Late) + 1).ToString();
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strAttendPeriod, strReasonCategory);
                }
            }
        }

        /// <summary>
        /// 检查下班是否存在异常情况
        /// </summary>
        /// <param name="entAttRd">当前对应的考勤记录</param>
        /// <param name="entClockInList">当前日期下的所有该员工的打卡记录</param>
        /// <param name="strAttendPeriod">当前时间所属时间段(1:上午;2:中午;3:下午;4:晚上)</param>
        /// <param name="strAttEndTime">考勤记录设定的上班时间</param>
        /// <param name="strAttOffCardStartTime">考勤记录设定的下班打卡有效起始时间</param>
        /// <param name="strAttOffCardEndTime">考勤记录设定的下班打卡有效截止时间</param>
        /// <param name="bCheck">判断该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strTemp">记录该段工作期是否存在未刷卡的标志位</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        private void CheckAbnormRecordWithWorkOff(T_HR_ATTENDANCERECORD entAttRd, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInList, string strAttendPeriod, string strAttEndTime,
            string strAttOffCardStartTime, string strAttOffCardEndTime, ref bool bCheck, ref StringBuilder strTemp, ref string strAbnormCategory)
        {
            DateTime dtAttEnd = new DateTime();
            DateTime dtAttCardStart = new DateTime();
            DateTime dtAttCardEnd = new DateTime();

            DateTime.TryParse(strAttEndTime.Trim(), out dtAttEnd);
            DateTime.TryParse(strAttOffCardStartTime.Trim(), out dtAttCardStart);
            DateTime.TryParse(strAttOffCardEndTime.Trim(), out dtAttCardEnd);

            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

            foreach (T_HR_EMPLOYEECLOCKINRECORD item in entClockInList)
            {
                item.PUNCHDATE = DateTime.Parse(item.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + item.PUNCHTIME);
                entTemps.Add(item);
            }

            //先检查该段下班
            var clo = from c in entTemps
                      where c.PUNCHDATE >= dtAttCardStart && c.PUNCHDATE <= dtAttCardEnd
                      orderby c.PUNCHDATE descending
                      select c;

            //无记录，即视为缺勤
            if (clo.Count() == 0)
            {
                if (CheckLeaveRecord(entAttRd, dtAttCardStart, dtAttCardEnd) == true)
                {
                    return;
                }
                bCheck = true;
                strTemp.Append("END,");
            }
            else
            {
                T_HR_EMPLOYEECLOCKINRECORD entOff = clo.FirstOrDefault();
                DateTime dtOffPunch = entOff.PUNCHDATE.Value;

                //判断早退
                if (dtAttCardStart < dtOffPunch && dtAttEnd >= dtOffPunch)
                {
                    TimeSpan ts = dtOffPunch.Subtract(dtAttCardStart);
                    string strReasonCategory = string.Empty;
                    strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.LeaveEarly) + 1).ToString();
                    CreateAbnormRecordByCheckClockInRd(entAttRd, ts, strAbnormCategory, strAttendPeriod, strReasonCategory);
                }
            }
        }

        /// <summary>
        /// 检查当前起止时间内是否有请假记录,如存在，
        /// 则判断当前考勤作息记录的考勤状态是否是请假，不是即更新考勤作息记录
        /// </summary>
        /// <param name="entAttRd">考勤作息记录</param>
        /// <param name="dtAttCardStart">打卡起始时间</param>
        /// <param name="dtAttCardEnd">打卡截止时间</param>
        /// <returns></returns>
        private bool CheckLeaveRecord(T_HR_ATTENDANCERECORD entAttRd, DateTime dtAttCardStart, DateTime dtAttCardEnd)
        {
            string strEmployeeId = entAttRd.EMPLOYEEID;
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            var q = from l in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                    where l.EMPLOYEEID == strEmployeeId && l.STARTDATETIME >= dtAttCardStart && l.ENDDATETIME <= dtAttCardEnd && l.CHECKSTATE == strCheckState
                    select l;

            if (q.Count() > 0)
            {
                if (entAttRd.ATTENDANCESTATE != (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString())
                {
                    entAttRd.ATTENDANCESTATE = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    AttendanceRecordBLL bllAttRd = new AttendanceRecordBLL();
                    bllAttRd.ModifyAttRd(entAttRd);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 新增考勤记录下对应的异常记录
        /// </summary>
        /// <param name="item">考勤记录</param>
        /// <param name="ts">考勤异常累计时间</param>
        /// <param name="strAbnormCategory">考勤异常类型</param>
        /// <returns></returns>
        private void CreateAbnormRecordByCheckClockInRd(T_HR_ATTENDANCERECORD item, TimeSpan ts, string strAbnormCategory, string strAttendPeriod, string strReasonCategory)
        {
            T_HR_EMPLOYEEABNORMRECORD entAbnormRecord = new T_HR_EMPLOYEEABNORMRECORD();

            entAbnormRecord.ABNORMRECORDID = System.Guid.NewGuid().ToString().ToUpper();

            entAbnormRecord.T_HR_ATTENDANCERECORDReference.EntityKey =
                   new EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDANCERECORD", "ATTENDANCERECORDID", item.ATTENDANCERECORDID);

            entAbnormRecord.ABNORMALDATE = item.ATTENDANCEDATE;
            entAbnormRecord.ABNORMCATEGORY = strAbnormCategory;
            entAbnormRecord.ATTENDPERIOD = strAttendPeriod;
            entAbnormRecord.ABNORMALTIME = ts.Hours * 60 + ts.Minutes;
            entAbnormRecord.SINGINSTATE = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();   //默认未签卡。"1"为未签卡，"2"为已签卡
            entAbnormRecord.REMARK = string.Empty;

            entAbnormRecord.CREATEUSERID = item.CREATEUSERID;
            entAbnormRecord.CREATEDATE = DateTime.Now;
            entAbnormRecord.UPDATEUSERID = item.UPDATEUSERID;
            entAbnormRecord.UPDATEDATE = DateTime.Now;

            entAbnormRecord.OWNERID = item.EMPLOYEEID;//.OWNERID;
            entAbnormRecord.OWNERPOSTID = item.OWNERPOSTID;
            entAbnormRecord.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
            entAbnormRecord.OWNERCOMPANYID = item.OWNERCOMPANYID;

            entAbnormRecord.CREATEPOSTID = item.CREATEPOSTID;
            entAbnormRecord.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
            entAbnormRecord.CREATECOMPANYID = item.CREATECOMPANYID;

            AddAbnormRecord(entAbnormRecord);
        }

        /// <summary>
        /// 判断当前打卡时间属于哪个时间段
        /// </summary>
        /// <param name="strPunchTime"></param>
        /// <returns></returns>
        private string GetAttendPeriod(string strPunchTime)
        {
            string strRes = string.Empty;
            DateTime dtCheck = new DateTime();
            DateTime.TryParse(strPunchTime, out dtCheck);

            if (dtCheck.Hour > 6 && dtCheck.Hour <= 11)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Morning) + 1).ToString();
            }

            if (dtCheck.Hour > 11 && dtCheck.Hour <= 13)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Midday) + 1).ToString();
            }

            if (dtCheck.Hour > 13 && dtCheck.Hour <= 17)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Afternoon) + 1).ToString();
            }

            if (dtCheck.Hour > 17 && dtCheck.Hour <= 24)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Evening) + 1).ToString();
            }

            if (dtCheck.Hour > 0 && dtCheck.Hour <= 6)
            {
                strRes = (Convert.ToInt32(Common.AttendPeriod.Evening) + 1).ToString();
            }

            return strRes;
        }

        /// <summary>
        /// 根据员工id，起始日期检测考勤异常，如果存在即触发引擎发送待办任务
        /// </summary>
        /// <param name="strEmployeeId"></param>
        /// <param name="strdtStart"></param>
        /// <param name="strdtEnd"></param>
        private void AbnormRecordCheckAlarm(string strEmployeeId, string strdtStart, string strdtEnd)
        {
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();
            DateTime.TryParse(strdtStart, out dtStart);
            DateTime.TryParse(strdtEnd, out dtEnd);

            CreateTempSignInRecord(strEmployeeId, dtStart, dtEnd);
        }

        /// <summary>
        /// 出现考勤异常时，主动为有异常的员工创建一条签卡记录供其签卡
        /// </summary>
        /// <param name="strEmployeeId">员工Id</param>
        /// <param name="dtStart">打卡起始日期</param>
        /// <param name="dtEnd">打卡截止日期</param>
        private void CreateTempSignInRecord(string strEmployeeId, DateTime dtStart, DateTime dtEnd)
        {
            EmployeeBLL bllEmployee = new EmployeeBLL();
            V_EMPLOYEEPOST entEmployeeDetail = bllEmployee.GetEmployeeDetailByID(strEmployeeId);

            if (entEmployeeDetail == null)
            {
                return;
            }

            if (entEmployeeDetail.EMPLOYEEPOSTS == null)
            {
                return;
            }

            if (entEmployeeDetail.EMPLOYEEPOSTS.Count() == 0)
            {
                return;
            }

            string strIsAgency = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
            T_HR_EMPLOYEEPOST entEmpPost = entEmployeeDetail.EMPLOYEEPOSTS.Where(c => c.ISAGENCY == strIsAgency).FirstOrDefault();

            if (entEmpPost == null)
            {
                return;
            }

            string strAbnormCategory = (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString();
            string strSignState = (Convert.ToInt32(Common.IsChecked.No) + 1).ToString();
            string strOrderKey = "ABNORMALDATE";
            IQueryable<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = GetAbnormRecordRdListByEmpIdAndDate(strEmployeeId, strAbnormCategory, strSignState, dtStart, dtEnd, strOrderKey);
            if (entAbnormRecords == null)
            {
                return;
            }

            if (entAbnormRecords.Count() == 0)
            {
                return;
            }

            EmployeeSignInRecordBLL bllSignInRecord = new EmployeeSignInRecordBLL();
            bllSignInRecord.ClearNoSignInRecord("T_HR_EMPLOYEESIGNINRECORD", entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID, entAbnormRecords);

            T_HR_EMPLOYEESIGNINRECORD entSignInRd = new T_HR_EMPLOYEESIGNINRECORD();
            entSignInRd.SIGNINID = Guid.NewGuid().ToString().ToUpper();
            entSignInRd.EMPLOYEEID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
            entSignInRd.EMPLOYEENAME = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEECNAME;
            entSignInRd.EMPLOYEECODE = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEECODE;
            entSignInRd.SIGNINTIME = DateTime.Now;
            entSignInRd.SIGNINCATEGORY = string.Empty;
            entSignInRd.CHECKSTATE = Convert.ToInt32(Common.CheckStates.UnSubmit).ToString();
            entSignInRd.REMARK = string.Empty;
            entSignInRd.CREATEUSERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
            entSignInRd.CREATEDATE = DateTime.Now;
            entSignInRd.UPDATEUSERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
            entSignInRd.UPDATEDATE = DateTime.Now;
            entSignInRd.OWNERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
            entSignInRd.OWNERPOSTID = entEmpPost.T_HR_POST.POSTID;
            entSignInRd.OWNERDEPARTMENTID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
            entSignInRd.OWNERCOMPANYID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            entSignInRd.CREATECOMPANYID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            entSignInRd.CREATEDEPARTMENTID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
            entSignInRd.CREATEPOSTID = entEmpPost.T_HR_POST.POSTID;

            List<T_HR_EMPLOYEESIGNINDETAIL> entSignInDetails = new List<T_HR_EMPLOYEESIGNINDETAIL>();
            foreach (T_HR_EMPLOYEEABNORMRECORD item in entAbnormRecords)
            {
                T_HR_EMPLOYEESIGNINDETAIL entSignInDetail = new T_HR_EMPLOYEESIGNINDETAIL();
                entSignInDetail.SIGNINDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                entSignInDetail.T_HR_EMPLOYEESIGNINRECORD = entSignInRd;
                entSignInDetail.T_HR_EMPLOYEEABNORMRECORD = item;
                entSignInDetail.ABNORMALDATE = item.ABNORMALDATE;
                entSignInDetail.ABNORMCATEGORY = item.ABNORMCATEGORY;
                entSignInDetail.ATTENDPERIOD = item.ATTENDPERIOD;
                entSignInDetail.ABNORMALTIME = item.ABNORMALTIME;
                entSignInDetail.REASONCATEGORY = (Convert.ToInt32(Common.AbnormReasonCategory.DrainPunch) + 1).ToString();
                entSignInDetail.DETAILREASON = string.Empty;
                entSignInDetail.REMARK = string.Empty;
                entSignInDetail.CREATEUSERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
                entSignInDetail.CREATEDATE = DateTime.Now;
                entSignInDetail.UPDATEUSERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
                entSignInDetail.UPDATEDATE = DateTime.Now;
                entSignInDetail.OWNERID = entEmployeeDetail.T_HR_EMPLOYEE.EMPLOYEEID;
                entSignInDetail.OWNERPOSTID = entEmpPost.T_HR_POST.POSTID;
                entSignInDetail.OWNERDEPARTMENTID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                entSignInDetail.OWNERCOMPANYID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                entSignInDetail.CREATECOMPANYID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                entSignInDetail.CREATEDEPARTMENTID = entEmpPost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                entSignInDetail.CREATEPOSTID = entEmpPost.T_HR_POST.POSTID;

                entSignInDetails.Add(entSignInDetail);
            }

            string strMsg = bllSignInRecord.EmployeeSignInRecordAdd(entSignInRd, entSignInDetails);

            if (strMsg != "{SAVESUCCESSED}")
            {
                return;
            }

            string submitName = string.Empty;

            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = entSignInRd.SIGNINID;
            userMsg.UserID = strEmployeeId;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            submitName = entSignInRd.EMPLOYEENAME;
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEESIGNINRECORD", Utility.ObjListToXml(entSignInRd, "HR", submitName), EngineWS.MsgType.Task);
        }

        /// <summary>
        /// 根据员工ID，异常签卡状态，异常类型及考勤异常起止时间查询考勤异常记录
        /// </summary>
        /// <param name="strEmployeeId"></param>
        /// <param name="strAbnormCategory"></param>
        /// <param name="strSignInState"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        private IQueryable<T_HR_EMPLOYEEABNORMRECORD> GetAbnormRecordRdListByEmpIdAndDate(string strEmployeeId, string strAbnormCategory, string strSignInState, DateTime dtStart, DateTime dtEnd, string strSortKey)
        {
            var q = GetAbnormRecordRdListByEmpIdAndDate(strEmployeeId, strAbnormCategory, dtStart, dtEnd);

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strSignInState))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                strfilter.Append(" SINGINSTATE == @" + objArgs.Count().ToString());
                objArgs.Add(strSignInState);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ABNORMALDATE ";
            }

            q = q.Where(strfilter.ToString(), objArgs.ToArray());
            return q.OrderBy(strOrderBy);
        }

        #endregion

        #region  在线考勤汇总一览表 weirui 2012-12-13 add

        /// <summary>
        /// 根据员工ID和公司ID查询考勤记录表 weirui 2012-12-6 add
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownerCompanyId"></param>
        /// <returns></returns>
        public List<AbnormalAttendanceeEntity> ListEMPLOYEEABNORMRECORD(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> listEMPLOYEE = new List<AbnormalAttendanceeEntity>();

            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);
            try
            {
                //根据

                //根据公司ID查询此公司的员工信息 目标集合
                var companyInfo = from t in dal.GetObjects<T_HR_EMPLOYEE>()
                                  where t.EMPLOYEESTATE != "2"
                                  && t.OWNERCOMPANYID == ownerCompanyId
                                  select t;
                //表示选择人员查询,数据过滤
                if (!string.IsNullOrEmpty(ownerId))
                {
                    companyInfo = from t in companyInfo
                                  where t.EMPLOYEEID == ownerId
                                  select t;
                    //companyInfo.Where(c => c.EMPLOYEEID == ownerId);
                }

                //查询异常考勤记录表
                var q = from t in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>()
                        join p in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                        on t.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID equals p.ATTENDANCERECORDID
                        where t.OWNERCOMPANYID == ownerCompanyId
                        && t.ABNORMALDATE >= startDates
                        && t.ABNORMALDATE <= endDates
                        select t;


                //已目标值为基础 左连接
                var enployeeInfo = from t in companyInfo
                                   join y in q
                                   on t.EMPLOYEEID equals y.OWNERID into EMPLOYEE
                                   from n in EMPLOYEE.DefaultIfEmpty()
                                   select new AbnormalAttendanceeEntity
                                      {
                                          //员工名字
                                          cname = t.EMPLOYEECNAME,
                                          //员工ID
                                          EMPLOYEEID = t.EMPLOYEEID,
                                          //异常类型
                                          ABNORMCATEGORY = n.ABNORMCATEGORY,
                                          //异常时长
                                          ABNORMALTIME = n.ABNORMALTIME
                                      };

                if (enployeeInfo.Count() > 0)
                {
                    return enployeeInfo.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// 根据公司IDOr员工ID查找员工请假记录
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<AbnormalAttendanceeEntity> GetEmployeeLeaverecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);

            //根据公司ID查询假期类型表，得到事假2、年休假4、病假3、调休假1 id
            //var y=from t in dal.GetObjects<T_HR_LEAVETYPESET>()LEAVETYPESETID
            var o = from t in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                    join p in dal.GetObjects<T_HR_LEAVETYPESET>().Where(c => c.LEAVETYPEVALUE == "2" || c.LEAVETYPEVALUE == "4"
                        || c.LEAVETYPEVALUE == "3" || c.LEAVETYPEVALUE == "1")
                    on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals p.LEAVETYPESETID
                    where t.OWNERCOMPANYID == ownerCompanyId
                    && t.STARTDATETIME >= startDates
                    && t.ENDDATETIME <= endDates
                    && t.CHECKSTATE == "2"
                    select new AbnormalAttendanceeEntity
                    {
                        //员工名字
                        cname = t.EMPLOYEENAME,
                        //员工ID
                        EMPLOYEEID = t.EMPLOYEEID,
                        //请假类型
                        LeaverecordStyple = p.LEAVETYPEVALUE,
                        //请假时长
                        LeaverecordTime = t.TOTALHOURS
                    };
            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                o.Where(c => c.EMPLOYEEID == ownerId);
            }

            #region 删除
            ////员工的 事假2、年休假4、病假3、调休假1计算
            //var str = (from t in o
            //        group t by
            //        new
            //        {
            //            t.EMPLOYEEID,
            //            t.cname
            //        }
            //            into g
            //            select new AbnormalAttendanceeEntity
            //            {
            //                //员工ID
            //                EMPLOYEEID = g.Key.EMPLOYEEID,
            //                //员工姓名
            //                cname = g.Key.cname,

            //                //请事假时长
            //                LeaveHour = g.Where(c => c.LeaverecordStyple == "2").Sum(c => c.LeaverecordTime),
            //                //请年假时长
            //                AnnualLeave = g.Where(c => c.LeaverecordStyple == "4").Sum(c => c.LeaverecordTime),
            //                //请病假时长
            //                SickLeave = g.Where(c => c.LeaverecordStyple == "3").Sum(c => c.LeaverecordTime),
            //                //请调休假时长
            //                OffHour = g.Where(c => c.LeaverecordStyple == "1").Sum(c => c.LeaverecordTime),

            //                AdjustableVacation=0
            //            }).ToList();


            //在时间范围内可修假天数
            //var v = from t in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
            //        where t.EFFICDATE >= startDates
            //        && t.TERMINATEDATE <= endDates
            //        && t.VACATIONTYPE=="1"
            //        && (t.EMPLOYEEID == ownerId || t.OWNERCOMPANYID == ownerCompanyId)
            //        select new AbnormalAttendanceeEntity
            //        {
            //            //员工名字
            //            cname = t.EMPLOYEENAME,
            //            //员工ID
            //            EMPLOYEEID = t.EMPLOYEEID,
            //            //请假天数
            //            AdjustableDay =t.DAYS

            //        };

            //var str2 = (from t in v
            //           group t by
            //           new
            //           {
            //               t.EMPLOYEEID,
            //               t.cname
            //           } into g
            //           select new AbnormalAttendanceeEntity
            //           {
            //               //员工ID
            //               EMPLOYEEID = g.Key.EMPLOYEEID,
            //               //员工姓名
            //               cname = g.Key.cname,
            //               //可休假天数
            //               AdjustableDay=g.Sum(c=>c.AdjustableDay)
            //           }).ToList();

            //if (str2.Count()>0 && str.Count()>0)
            //{
            //    foreach (var item in str2)
            //    {
            //        foreach (var item2 in str)
            //        {
            //            if (item.EMPLOYEEID == item2.EMPLOYEEID)
            //            {
            //                item2.AdjustableVacation = decimal.Parse((7.5 * double.Parse(item.AdjustableDay.ToString())
            //                    - double.Parse(item2.OffHour.ToString())).ToString());
            //            }
            //        }
            //    }
            //}

            //str2.ToList();
            #endregion

            if (o.Count() > 0)
            {
                return o.ToList();
            }
            return null;
        }

        /// <summary>
        /// 根据公司IDOr员工ID查找员工可以调休的请假天数(掉休假)
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns></returns>
        public List<AbnormalAttendanceeEntity> GetAdjustableVacation(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);


            var v = from t in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                    where t.EFFICDATE >= startDates
                    && t.TERMINATEDATE <= endDates
                    && t.VACATIONTYPE == "1"
                    && (t.EMPLOYEEID == ownerId || t.OWNERCOMPANYID == ownerCompanyId)
                    select new AbnormalAttendanceeEntity
                    {
                        //员工名字
                        cname = t.EMPLOYEENAME,
                        //员工ID
                        EMPLOYEEID = t.EMPLOYEEID,
                        //请假天数
                        AdjustableDay = t.DAYS
                    };

            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                v.Where(c => c.EMPLOYEEID == ownerId);
            }

            if (v.Count() > 0)
            {
                return v.ToList();
            }
            return null;
        }

        /// <summary>
        /// 得到每天最后的打卡时间
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="ownerCompanyId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<AbnormalAttendanceeEntity> GetLasterClockInRecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);

            var v = from t in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                    where t.PUNCHDATE >= startDates
                    && t.PUNCHDATE <= endDates
                    && t.OWNERCOMPANYID == ownerCompanyId
                    group t by
                    new
                    {
                        EMPLOYEENAME = t.EMPLOYEENAME,
                        EMPLOYEEID = t.EMPLOYEEID,
                        //PUNCHDATE=new DateTime(t.PUNCHDATE.Value.Year,t.PUNCHDATE.Value.Month,t.PUNCHDATE.Value.Day)
                        YEAR = t.PUNCHDATE.Value.Year,
                        Month = t.PUNCHDATE.Value.Month,
                        Day = t.PUNCHDATE.Value.Day
                    } into g
                    select new AbnormalAttendanceeEntity
                    {
                        cname = g.Key.EMPLOYEENAME,
                        EMPLOYEEID = g.Key.EMPLOYEEID,
                        //打卡时间
                        Punchdate = g.Max(c => c.PUNCHDATE)
                    };

            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                v.Where(c => c.EMPLOYEEID == ownerId);
            }

            if (v.Count() > 0)
            {
                return v.ToList();
            }
            return null;
        }

        /// <summary>
        /// 考勤汇总导出
        /// </summary>
        /// <returns></returns>
        public List<AbnormalAttendanceeEntity> ExportEmployees(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity2 = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity3 = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity4 = new List<AbnormalAttendanceeEntity>();
            //请假记录
            abnormalAttendanceeEntity = GetEmployeeLeaverecord(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity != null)
            {
                var v = from t in abnormalAttendanceeEntity
                        group t by
                        new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        }
                            into g
                            select new AbnormalAttendanceeEntity
                            {
                                //员工ID
                                EMPLOYEEID = g.Key.EMPLOYEEID,
                                //员工姓名
                                cname = g.Key.cname,

                                //请事假时长
                                LeaveHour = g.Where(c => c.LeaverecordStyple == "2").Sum(c => c.LeaverecordTime),
                                //请年假时长
                                AnnualLeave = g.Where(c => c.LeaverecordStyple == "4").Sum(c => c.LeaverecordTime),
                                //请病假时长
                                SickLeave = g.Where(c => c.LeaverecordStyple == "3").Sum(c => c.LeaverecordTime),
                                //请调休假时长
                                OffHour = g.Where(c => c.LeaverecordStyple == "1").Sum(c => c.LeaverecordTime)
                            };
                abnormalAttendanceeEntity = v.ToList();
            }
            //超额工时
            abnormalAttendanceeEntity4 = GetLasterClockInRecord(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity4 != null)
            {
                var v = from t in abnormalAttendanceeEntity4
                        group t by new
                        {
                            t.cname,
                            t.EMPLOYEEID
                        } into g
                        select new AbnormalAttendanceeEntity
                        {
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //员工姓名
                            cname = g.Key.cname,

                            ExcessHoursTotal = g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Hour)
                            + (g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Minute) / 60)
                        };
                abnormalAttendanceeEntity4 = v.ToList();
            }
            //可以调休的请假天数
            abnormalAttendanceeEntity3 = GetAdjustableVacation(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity3 != null)
            {
                var v = from t in abnormalAttendanceeEntity3
                        group t by
                        new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        } into g
                        select new AbnormalAttendanceeEntity
                        {
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //员工姓名
                            cname = g.Key.cname,

                            //可调休假
                            AdjustableDay = g.Sum(c => c.AdjustableDay)
                        };
                abnormalAttendanceeEntity3 = v.ToList();
            }

            //考勤
            abnormalAttendanceeEntity2 = ListEMPLOYEEABNORMRECORD(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity2 != null)
            {
                var v = from t in abnormalAttendanceeEntity2
                        group t by new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        } into g
                        select new AbnormalAttendanceeEntity
                        {
                            //名字
                            cname = g.Key.cname,
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //迟到/早退次数
                            outTimes = g.Where(c => c.ABNORMCATEGORY == "1").Count(),
                            //迟到/早退合计小时
                            outMinutes = g.Where(c => c.ABNORMCATEGORY == "1").Sum(c => c.ABNORMALTIME),
                            //缺勤次数
                            DrainTimeNumber = g.Where(c => c.ABNORMCATEGORY == "3").Count(),

                            //超额工时合计
                            ExcessHoursTotal = 0,

                            //可调休假
                            AdjustableVacation = 0,

                            //事假
                            LeaveHour = 0,
                            //年休假
                            AnnualLeave = 0,
                            //病假
                            SickLeave = 0,
                            //调休假
                            OffHour = 0
                        };
                abnormalAttendanceeEntity2 = v.ToList();

                if (abnormalAttendanceeEntity != null)
                {
                    foreach (var item in abnormalAttendanceeEntity)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                item2.LeaveHour = item.LeaveHour;
                                item2.AnnualLeave = item.AnnualLeave;
                                item2.SickLeave = item.SickLeave;
                                item2.OffHour = item.OffHour;
                            }


                        }
                    }
                }


                if (abnormalAttendanceeEntity3 != null)
                {
                    foreach (var item in abnormalAttendanceeEntity3)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                double dou = Double.Parse(item.AdjustableDay.ToString()) * 7.5
                                    - Double.Parse(item2.OffHour.ToString());

                                item2.AdjustableVacation = decimal.Parse(dou.ToString());
                            }
                        }
                    }
                }


                if (abnormalAttendanceeEntity4 != null)
                {
                    foreach (var item in abnormalAttendanceeEntity4)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                item2.ExcessHoursTotal = item.ExcessHoursTotal;
                            }
                        }
                    }
                }
            }
            return abnormalAttendanceeEntity2;
        }


        public byte[] ExportEmployeesIntime(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity = ExportEmployees(ownerId, ownerCompanyId, startDate, endDate);

            try
            {
                if (abnormalAttendanceeEntity.Count() > 0)
                {
                    List<string> colName = new List<string>();
                    colName.Add("员工姓名");
                    colName.Add("迟到/早退总次数");
                    colName.Add("迟到/早退合计（分钟)");
                    colName.Add("漏打卡次数");
                    colName.Add("超额工时合计（小时）");
                    colName.Add("可调休假（小时）");
                    colName.Add("事假（小时）");
                    colName.Add("年休假（小时）");
                    colName.Add("病假（小时）");
                    colName.Add("调休假（小时）");
                    //var tmp = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //内容
                    foreach (var employeeinfo in abnormalAttendanceeEntity)
                    {
                        sb.Append(employeeinfo.cname + ",");
                        sb.Append(employeeinfo.outTimes + ",");
                        sb.Append(employeeinfo.outMinutes + ",");
                        sb.Append(employeeinfo.DrainTimeNumber + ",");
                        sb.Append(employeeinfo.ExcessHoursTotal + ",");
                        sb.Append(employeeinfo.AdjustableVacation + ",");
                        sb.Append(employeeinfo.LeaveHour + ",");
                        sb.Append(employeeinfo.AnnualLeave + ",");
                        sb.Append(employeeinfo.SickLeave + ",");
                        sb.Append(employeeinfo.OffHour + ",");
                        sb.Append("\r\n");
                    }
                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesIntime:" + ex.Message);
                return null;
            }

        }
        #endregion

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public List<T_HR_EMPLOYEELEAVERECORD> ListEMPLOYEELEAVERECORD()
        {
            return null;
        }

        /// <summary>
        /// 取得所有公司ID数据
        /// </summary>
        /// <returns></returns>
        public List<T_HR_COMPANY> ListT_HR_COMPANY()
        {
            var v = from e in dal.GetObjects<T_HR_COMPANY>()
                    select e;

            if (v.Count() > 0)
            {
                return v.ToList();
            }
            return null;
        }
    }
}
