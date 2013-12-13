
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
using SMT.Foundation.Log;
using SMT.HRM.BLL.Common;
using System.Threading;

namespace SMT.HRM.BLL
{
    public partial class AbnormRecordBLL : BaseBll<T_HR_EMPLOYEEABNORMRECORD>
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
        
        #region 无效代码
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

    }
}
