/*
 * 文件名：ClockInRecordBLL.cs
 * 作  用：日常考勤打卡业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 17:07:59
 * 修改人：
 * 修改时间：
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using SMT.HRM.CustomModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Data.OleDb;
using System.Data;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class ClockInRecordBLL : BaseBll<T_HR_EMPLOYEECLOCKINRECORD>
    {
        #region 获取数据

        /// <summary>
        /// 根据指纹编号，打卡日期及时间，检查是否存在记录
        /// </summary>
        /// <param name="strFingerPrintID">指纹编号</param>
        /// <param name="dtPunchDate">打卡日期</param>
        /// <param name="strPunchTime">打卡时间</param>
        /// <returns>true/false</returns>
        public bool IsExistsRd(string strFingerPrintID, DateTime? dtPunchDate, string strPunchTime)
        {
            bool flag = false;

            ClockInRecordDAL dalClockInRecord = new ClockInRecordDAL();
            flag = dalClockInRecord.IsExistsRd(strFingerPrintID, dtPunchDate, strPunchTime);

            return flag;
        }

        /// <summary>
        /// 根据公司的ID，取得当前录入的员工打卡记录的最新打卡日期
        /// </summary>
        /// <param name="strCompanyId"></param>
        /// <returns></returns>
        public DateTime GetLatestPunchDateByCompanyId(string strCompanyId)
        {
            DateTime dtRes = new DateTime();
            if (string.IsNullOrWhiteSpace(strCompanyId))
            {
                return dtRes;
            }


            IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds = GetAllClockInRdListByMultSearch("Company", strCompanyId, string.Empty, string.Empty, string.Empty, string.Empty, "PUNCHDATE DESC");
            if (entClockInRds.Count() == 0)
            {
                return dtRes;
            }

            T_HR_EMPLOYEECLOCKINRECORD entClockInRd = entClockInRds.FirstOrDefault();

            return entClockInRd.PUNCHDATE.Value;
        }

        public IQueryable<T_HR_EMPLOYEECLOCKINRECORD> GetAllClockInRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom,
            string strPunchDateTo, string strSortKey)
        {
            ClockInRecordDAL dalClockInRecord = new ClockInRecordDAL();

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
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEECLOCKINRECORD");
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = "CLOCKINRECORDID";
            }

            var q = dalClockInRecord.GetClockInRdListByMultSearch(sType, sValue, strPunchDateFrom, strPunchDateTo, strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        public List<T_HR_EMPLOYEECLOCKINRECORD> GetAllClockInRdListBySql(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom,
            string strPunchDateTo, string strTimeFrom, string strTimeTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            ClockInRecordDAL dalClockInRecord = new ClockInRecordDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" t.EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            string filterString = strfilter.ToString();

            if (!string.IsNullOrEmpty(strOwnerID))
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEECLOCKINRECORD");
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = "t.CLOCKINRECORDID";
            }

            var q = dalClockInRecord.GetClockInRdListBySql(sType, sValue, strPunchDateFrom, strPunchDateTo, strTimeFrom, strTimeTo, strOrderBy, filterString, pageIndex, pageSize, ref pageCount, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 获取员工日常打卡信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">查看人的员工ID(权限控制)</param>
        /// <param name="strEmployeeID">员工序号(唯一，GUID)</param>
        /// <param name="strPunchDateFrom">打卡搜寻起始日期</param>
        /// <param name="strPunchDateTo">打卡搜寻截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页码</param>
        /// <returns>返回员工日常打卡列表</returns>
        public List<T_HR_EMPLOYEECLOCKINRECORD> GetClockInRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom,
            string strPunchDateTo, string strTimeFrom, string strTimeTo, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            return GetAllClockInRdListBySql(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strTimeFrom, strTimeTo, strSortKey, pageIndex, pageSize, ref pageCount);
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增员工打卡信息
        /// </summary>
        /// <param name="entClockInRd"></param>
        /// <returns></returns>
        public string AddClockInRecord(T_HR_EMPLOYEECLOCKINRECORD entTemp)
        {
            string strMsg = string.Empty;
            bool flag = false;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }
                if (entTemp.PUNCHDATE.Value.Year < DateTime.Now.Year)
                {
                    Tracer.Debug("导入的打卡记录非今年记录，跳过。");
                    return "{REQUIREDFIELDS}";
                }
                #region 判断是否已经初始化考勤记录，没有就初始化整月数据
                DateTime dt = new DateTime(entTemp.PUNCHDATE.Value.Year, entTemp.PUNCHDATE.Value.Month, entTemp.PUNCHDATE.Value.Day);
                IQueryable<T_HR_ATTENDANCERECORD> entArs = from att in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                                                           where att.EMPLOYEEID == entTemp.EMPLOYEEID
                                                            && att.ATTENDANCEDATE == dt
                                                           select att;               
                if (entArs.Count() < 1)
                {                    
                    string dtInit = entTemp.PUNCHDATE.Value.Year.ToString() + "-" + entTemp.PUNCHDATE.Value.Month.ToString();
                    Tracer.Debug("打卡记录导入发现没有考勤初始化记录，开始初始化考勤记录，员工姓名" + entTemp.EMPLOYEENAME + " 初始化年月"
                        + dtInit + " 员工id：" + entTemp.EMPLOYEEID);
                    try
                    {
                        AttendanceSolutionAsignBLL bllAttendanceSolutionAsign = new AttendanceSolutionAsignBLL();
                        //初始化该员工当月考勤记录
                        bllAttendanceSolutionAsign.AsignAttendanceSolutionByOrgID("4", entTemp.EMPLOYEEID, dtInit);

                        Tracer.Debug("打卡记录导入没有查到考勤初始化数据，初始化员工考勤成功，初始化月份：" + dtInit);
                       
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("打卡记录导入初始化考勤失败：月份：" + dtInit + " 姓名："+
                            entTemp.EMPLOYEENAME +" 员工id："+ entTemp.EMPLOYEEID + "失败原因：" + ex.ToString());                      
                    }
                }
                #endregion

                ClockInRecordDAL dalClockInRecord = new ClockInRecordDAL();
                flag = dalClockInRecord.IsExistsRd(entTemp.FINGERPRINTID, entTemp.PUNCHDATE, entTemp.PUNCHTIME);

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalClockInRecord.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";


            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        public void ImportClockInRdList(string strCompanyId, List<T_HR_EMPLOYEECLOCKINRECORD> entTempList, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strCompanyId))
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                if (entTempList == null)
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                if (entTempList.Count() == 0)
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();

                for (int i = 0; i < entTempList.Count(); i++)
                {
                    T_HR_EMPLOYEECLOCKINRECORD entTemp = entTempList[i];
                    string strFingerPrintID = entTemp.FINGERPRINTID;
                    T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByFingerPrintID(strFingerPrintID);

                    T_HR_EMPLOYEECLOCKINRECORD entClockInRd = new T_HR_EMPLOYEECLOCKINRECORD();
                    entClockInRd.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                    if (entEmployee != null)
                    {
                        entClockInRd.EMPLOYEEID = entEmployee.EMPLOYEEID;
                        entClockInRd.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                        entClockInRd.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                        entClockInRd.FINGERPRINTID = entEmployee.FINGERPRINTID;

                        entClockInRd.OWNERID = entEmployee.EMPLOYEEID; //.OWNERID;
                        entClockInRd.OWNERPOSTID = entEmployee.OWNERPOSTID;
                        entClockInRd.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                        entClockInRd.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                        entClockInRd.CREATECOMPANYID = entEmployee.CREATECOMPANYID;
                        entClockInRd.CREATEDEPARTMENTID = entEmployee.CREATEDEPARTMENTID;
                        entClockInRd.CREATEPOSTID = entEmployee.CREATEPOSTID;
                        entClockInRd.CREATEUSERID = entEmployee.CREATEUSERID;
                        entClockInRd.VERIFYCODE = 0;
                    }

                    entClockInRd.PUNCHDATE = entTemp.PUNCHDATE;
                    entClockInRd.PUNCHTIME = entTemp.PUNCHTIME;


                    strMsg = AddClockInRecord(entClockInRd);
                }

                if (strMsg != "{SAVESUCCESSED}" && strMsg != "{ALREADYEXISTSRECORD}")
                {
                    return;
                }

                var q = from s in dal.GetObjects<T_HR_ATTENDANCESOLUTIONASIGN>().Include("T_HR_ATTENDANCESOLUTION")
                        where s.OWNERCOMPANYID == strCompanyId && s.STARTDATE <= dtStart && s.ENDDATE >= dtEnd
                        select s;

                if (q.Count() == 0)
                {
                    strMsg = "NOATTENDSOLASIGN";
                }

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 通过客户端注册的Windows服务，自动取得打卡机数据，转换后导入到数据库中
        /// </summary>
        /// <param name="entTempList">打卡机临时数据</param>
        /// <param name="dtStart">导入起始日期</param>
        /// <param name="dtEnd">导入截止日期</param>
        /// <param name="strMsg">处理消息</param>
        public void ImportClockInRdListByWindowsService(string strCompanyId, List<T_HR_EMPLOYEECLOCKINRECORD> entTempList, DateTime dtStart, DateTime dtEnd, string strClientIP, ref string strMsg)
        {
            try
            {
                Utility.SaveLog("本次打卡自动导入调用服务开始。导入的打卡机IP:" + strClientIP + "。 " + DateTime.Now.ToString() + "开始调用ImportClockInRdListByWindowsService函数计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的公司ID为：" + strCompanyId);
                if (string.IsNullOrWhiteSpace(strCompanyId))
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                if (string.IsNullOrWhiteSpace(strClientIP))
                {
                    strMsg = "打卡机IP未知，禁止导入！";
                    Utility.SaveLog(strMsg + "此次导入，计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的公司ID为：" + strCompanyId);
                    return;
                }

                if (entTempList == null)
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                if (entTempList.Count() == 0)
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                string[] strs = entTempList.Select(c => c.FINGERPRINTID).ToArray();
                string filter = string.Empty;
                string strCheckAutoImportComps = "";
                string strAutoImportCompIDs = System.Configuration.ConfigurationManager.AppSettings["AutoImportCompanys"];

                if (!string.IsNullOrWhiteSpace(strAutoImportCompIDs))
                {
                    strCheckAutoImportComps = strAutoImportCompIDs;
                }

                for (int i = 0; i < strs.Length; i++)
                {
                    if (!filter.Contains(strs[i]))
                    {
                        filter = filter + strs[i];

                        if (i != strs.Length - 1)
                        {
                            filter = filter + ",";
                        }
                    }
                }

                //var entEmployees = dal.GetObjects<T_HR_EMPLOYEE>().Where(c => c.FINGERPRINTID.Contains(filter));

                //var listCompany = (from ent in entEmployees
                //                  select ent.OWNERCOMPANYID).Distinct().ToList();

                string strIsAgnecy = Convert.ToInt32(Common.IsAgencyPost.No).ToString();
                string strEditState = Convert.ToInt32(Common.EditStates.Actived).ToString();
                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();

                Utility.SaveLog(strMsg + "此次导入，(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的所有员工指纹编码为：" + filter);
                    

                var listCompany = (from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                   join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                   join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                   where e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState && e.CHECKSTATE == strCheckState
                                   && e.T_HR_EMPLOYEE.FINGERPRINTID.Contains(filter)
                                   select c.COMPANYID).Distinct().ToList();

                if (listCompany == null)
                {
                    strMsg = "当前导入的人员在系统内无对应的所属公司";
                    Utility.SaveLog(strMsg + "此次导入，计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的公司ID为：" + strCompanyId);
                    return;
                }

                int n = listCompany.Count();
                if (n == 0)
                {
                    strMsg = "当前导入的人员在系统内无对应的所属公司";
                    Utility.SaveLog(strMsg + "此次导入，计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的公司ID为：" + strCompanyId);
                    return;
                }

                filter = string.Empty;
                int m = 0;
                foreach (var item in listCompany)
                {
                    if (strCheckAutoImportComps.Contains(item))
                    {
                        filter = filter + item;
                    }

                    if (m != n - 1)
                    {
                        filter = filter + ",";
                    }

                    m++;
                }

                SMT.Foundation.Log.Tracer.Debug("导入打卡记录的公司Id：" + filter);

                var entEmployees = from e in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST")
                                   join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on e.T_HR_POST.POSTID equals p.POSTID
                                   join d in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY") on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                                   join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                                   where e.ISAGENCY == strIsAgnecy && e.EDITSTATE == strEditState && e.CHECKSTATE == strCheckState
                                   && c.COMPANYID.Contains(filter)
                                   select e.T_HR_EMPLOYEE;


                for (int i = 0; i < entTempList.Count(); i++)
                {
                    T_HR_EMPLOYEECLOCKINRECORD entTemp = entTempList[i];
                    string strFingerPrintID = entTemp.FINGERPRINTID;
                    var q = from ent in entEmployees
                            where ent.FINGERPRINTID == strFingerPrintID
                            select ent;
                    T_HR_EMPLOYEE entEmployee = q.FirstOrDefault();

                    T_HR_EMPLOYEECLOCKINRECORD entClockInRd = new T_HR_EMPLOYEECLOCKINRECORD();
                    entClockInRd.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                    if (entEmployee != null)
                    {
                        entClockInRd.EMPLOYEEID = entEmployee.EMPLOYEEID;
                        entClockInRd.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                        entClockInRd.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                        entClockInRd.FINGERPRINTID = entEmployee.FINGERPRINTID;

                        entClockInRd.OWNERID = entEmployee.EMPLOYEEID; //.OWNERID;
                        entClockInRd.OWNERPOSTID = entEmployee.OWNERPOSTID;
                        entClockInRd.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                        entClockInRd.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                        entClockInRd.CREATECOMPANYID = entEmployee.CREATECOMPANYID;
                        entClockInRd.CREATEDEPARTMENTID = entEmployee.CREATEDEPARTMENTID;
                        entClockInRd.CREATEPOSTID = entEmployee.CREATEPOSTID;
                        entClockInRd.CREATEUSERID = entEmployee.CREATEUSERID;
                        entClockInRd.VERIFYCODE = 0;
                    }

                    entClockInRd.PUNCHDATE = entTemp.PUNCHDATE;
                    entClockInRd.PUNCHTIME = entTemp.PUNCHTIME;

                    var qc = from ar in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                             where ar.OWNERCOMPANYID == entClockInRd.OWNERCOMPANYID && ar.EMPLOYEEID 
                             == entClockInRd.EMPLOYEEID 
                             && ar.PUNCHDATE == entClockInRd.PUNCHDATE 
                             && ar.PUNCHTIME == entClockInRd.PUNCHTIME
                             orderby ar.PUNCHDATE
                             select ar;

                    T_HR_EMPLOYEECLOCKINRECORD entUpdate = qc.FirstOrDefault();
                    if (entUpdate != null)
                    {
                        continue;
                    }

                    strMsg = AddClockInRecord(entClockInRd);
                }

                if (strMsg != "{SAVESUCCESSED}" && strMsg != "{ALREADYEXISTSRECORD}")
                {
                    return;
                }

                DateTime dtCheck = DateTime.Parse(dtStart.ToString("yyyy-MM") + "-1");

                AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                List<T_HR_EMPLOYEE> listEmpss = entEmployees.ToList();
                bllAbnormRecord.CheckAbnormRecordForEmployees(listEmpss, dtStart, dtEnd, ref strMsg);
                Utility.SaveLog("本次打卡自动导入调用服务完毕。导入的打卡机IP:" + strClientIP + "。 " + DateTime.Now.ToString() + "，计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的公司ID为：" + strCompanyId + ",本次处理的结果为：" + strMsg);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();

                string strerr = "本次打卡自动导入调用服务发生异常。导入的打卡机IP:" + strClientIP + "。 " + DateTime.Now.ToString() + "，计算考勤异常(计算的考勤时间段为：{0}-{1})。计算考勤的公司ID为：" + strCompanyId + ",出错原因：" + ex.ToString();

                string strStartTime = string.Empty, strEndTime = string.Empty;
                if (dtStart == null)
                {
                    strStartTime = "null";
                }
                else
                {
                    strStartTime = dtStart.ToString();
                }

                if (dtEnd == null)
                {
                    strEndTime = "null";
                }
                else
                {
                    strStartTime = dtEnd.ToString();
                }

                strerr = string.Format(strerr, strStartTime, strEndTime);

                Utility.SaveLog(strerr);
            }
        }

        /// <summary>
        /// 通过csv格式文件导入员工打卡记录
        /// </summary>
        /// <param name="strPhysicalPath"></param>
        /// <param name="strUnitType"></param>
        /// <param name="strUnitObjectId"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strMsg"></param>
        public void ImportClockInRdListByImportCSV(string strPhysicalPath, string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            try
            {
                Utility.SaveLog(DateTime.Now.ToString() + "手动导入打卡记录，开始调用ImportClockInRdListByImportCSV函数计算考勤异常(计算的考勤时间段为：" + dtStart.ToString() + "-" + dtEnd.ToString() + ")。计算考勤的机构类型为：" + strUnitType + ",机构ID为：" + strUnitObjectId);
                if (string.IsNullOrEmpty(strUnitType))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strUnitObjectId))
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

                DateTime dtCheck = DateTime.Parse(dtStart.ToString("yyyy-MM") + "-1");
                if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                {
                    entEmployees = bllEmployee.GetEmployeeByCompanyID(strUnitObjectId, dtCheck).ToList();
                }
                else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                {
                    entEmployees = bllEmployee.GetEmployeeByDepartmentID(strUnitObjectId).ToList();
                }
                else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                {
                    entEmployees = bllEmployee.GetEmployeeByPostID(strUnitObjectId).ToList();
                }

                if (entEmployees == null)
                {
                    strMsg = "{NOTFOUND}";
                    return;
                }

                if (entEmployees.Count == 0)
                {
                    strMsg = "{NOTFOUND}";
                    return;
                }

                ImportClockInRdListByImportCSV(strPhysicalPath, ref strMsg);

                if (strMsg != "{SAVESUCCESSED}" && strMsg != "{ALREADYEXISTSRECORD}")
                {
                    return;
                }

                AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strMsg);

                Utility.DeleteUploadFile(strPhysicalPath);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();

                string strerr = DateTime.Now.ToString() + "手动导入打卡记录，计算考勤异常(计算的考勤时间段为：{0}-{1})。计算考勤的机构类型为：" + strUnitType + ",机构ID为：" + strUnitObjectId + ",出错原因：" + ex.ToString();

                string strStartTime = string.Empty, strEndTime = string.Empty;
                if (dtStart == null)
                {
                    strStartTime = "null";
                }
                else
                {
                    strStartTime = dtStart.ToString();
                }

                if (dtEnd == null)
                {
                    strEndTime = "null";
                }
                else
                {
                    strStartTime = dtEnd.ToString();
                }

                strerr = string.Format(strerr, strStartTime, strEndTime);

                Utility.SaveLog(strerr);
            }
        }

        /// <summary>
        /// 导入Excel的员工日常打卡信息
        /// </summary>
        /// <param name="strPhysicalPath">当前上传的Excel文件路径</param>
        /// <param name="strUnitType">导入的对象类型</param>
        /// <param name="strUnitObjectId">导入的对象</param>
        /// <param name="dtStart">考勤录入起始日期</param>
        /// <param name="dtEnd">考勤录入截止日期</param>
        /// <param name="strMsg">处理返回结果后的消息</param>
        public void ImportClockInRdListByImportExcel(string strPhysicalPath, string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            if (string.IsNullOrEmpty(strUnitType))
            {
                return;
            }

            if (string.IsNullOrEmpty(strUnitObjectId))
            {
                return;
            }

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

            DateTime dtCheck = DateTime.Parse(dtStart.ToString("yyyy-MM") + "-1");
            if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByCompanyID(strUnitObjectId, dtCheck).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByDepartmentID(strUnitObjectId).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByPostID(strUnitObjectId).ToList();
            }

            if (entEmployees == null)
            {
                strMsg = "{NOTFOUND}";
                return;
            }

            if (entEmployees.Count == 0)
            {
                strMsg = "{NOTFOUND}";
                return;
            }

            ImportClockInRdListByImportExcel(strPhysicalPath, ref strMsg);

            if (strMsg != "{SAVESUCCESSED}" && strMsg != "{ALREADYEXISTSRECORD}")
            {
                return;
            }

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strMsg);

            Utility.DeleteUploadFile(strPhysicalPath);
        }

        /// <summary>
        /// 导入登陆系统的员工日常打卡信息
        /// </summary>
        /// <param name="strUnitType">导入的对象类型</param>
        /// <param name="strUnitObjectId">导入的对象</param>
        /// <param name="dtStart">考勤录入起始日期</param>
        /// <param name="dtEnd">考勤录入截止日期</param>
        /// <param name="strMsg">处理返回结果后的消息</param>
        public void ImportClockInRdListByLoginData(string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            if (string.IsNullOrEmpty(strUnitType))
            {
                return;
            }

            if (string.IsNullOrEmpty(strUnitObjectId))
            {
                return;
            }

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

            DateTime dtCheck = DateTime.Parse(dtStart.ToString("yyyy-MM") + "-1");
            if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByCompanyID(strUnitObjectId, dtCheck).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByDepartmentID(strUnitObjectId).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByPostID(strUnitObjectId).ToList();
            }

            if (entEmployees == null)
            {
                strMsg = "{NOTFOUND}";
                return;
            }

            if (entEmployees.Count == 0)
            {
                strMsg = "{NOTFOUND}";
                return;
            }
            ImportClockInRdListByLoginData(entEmployees, dtStart, dtEnd, ref strMsg);

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strMsg);
        }

        /// <summary>
        /// 导入打卡+登陆系统的员工日常打卡信息
        /// </summary>
        /// <param name="strPhysicalPath">当前上传的Excel文件路径</param>
        /// <param name="strUnitType">导入的对象类型</param>
        /// <param name="strUnitObjectId">导入的对象</param>
        /// <param name="dtStart">考勤录入起始日期</param>
        /// <param name="dtEnd">考勤录入截止日期</param>
        /// <param name="strMsg">处理返回结果后的消息</param>
        public void ImportClockInRdListByImportFileAndLoginData(string strPhysicalPath, string strFileType, string strUnitType, string strUnitObjectId, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            if (string.IsNullOrEmpty(strUnitType))
            {
                return;
            }

            if (string.IsNullOrEmpty(strUnitObjectId))
            {
                return;
            }

            EmployeeBLL bllEmployee = new EmployeeBLL();
            List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();

            DateTime dtCheck = DateTime.Parse(dtStart.ToString("yyyy-MM") + "-1");
            if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByCompanyID(strUnitObjectId, dtCheck).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByDepartmentID(strUnitObjectId).ToList();
            }
            else if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByPostID(strUnitObjectId).ToList();
            }

            if (entEmployees == null)
            {
                strMsg = "{NOTFOUND}";
                return;
            }

            if (entEmployees.Count == 0)
            {
                strMsg = "{NOTFOUND}";
                return;
            }

            if (strFileType.ToLower() == "csv")
            {
                ImportClockInRdListByImportCSV(strPhysicalPath, ref strMsg);
            }
            else if (strFileType.ToLower() == "xls")
            {
                ImportClockInRdListByImportExcel(strPhysicalPath, ref strMsg);
            }

            ImportClockInRdListByLoginData(entEmployees, dtStart, dtEnd, ref strMsg);

            AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
            bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strMsg);
        }

        /// <summary>
        /// 导出员工的指定时间段的原始打卡记录
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strPunchDateFrom">打卡起始日期</param>
        /// <param name="strPunchDateTo">打卡截止日期</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="strMsg">处理消息</param>
        /// <returns>返回转换成byte类型的数据</returns>
        public byte[] OutClockInRdListByMultSearch(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom, string strPunchDateTo,
            string strSortKey, out string strMsg)
        {
            //byte[] exPort = null;
            //strMsg = string.Empty;
            //return exPort;

            strMsg = string.Empty;
            DataTable dt = MakeTableToExport();

            IQueryable<T_HR_EMPLOYEECLOCKINRECORD> ents = GetAllClockInRdListByMultSearch(sType, sValue, strOwnerID, strEmployeeID, strPunchDateFrom, strPunchDateTo, strSortKey);

            long totalCount = ents.Count();

            if (totalCount > 65535)
            {
                strMsg = "OVERMAXEXPORTSIZE";
                return null;
            }


            if (totalCount == 0)
            {
                strMsg = "NOEXPORTDATA";
                return null;
            }

            DataTable dtExport = GetExportData(dt, ents);

            return Utility.OutFileStream(Utility.GetResourceStr("CLOCKINRD"), dtExport);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 读取指定路径的csv，将文件数据转换为员工日常打卡记录，并存储到数据库
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strMsg"></param>
        private void ImportClockInRdListByImportCSV(string strPath, ref string strMsg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strPath))
                {
                    strMsg = "{IMPORTCLOCKINRDERROR}";
                    return;
                }

                Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPath, Encoding.GetEncoding("GB2312"));

                Dictionary<string, T_HR_EMPLOYEE> entExistsEmployees = new Dictionary<string, T_HR_EMPLOYEE>();

                TF.Delimiters = new string[] { "," }; //设置分隔符
                string[] strLine;
                int readLine = 0;
                while (!TF.EndOfData)
                {
                    try
                    {
                        if (readLine == 0)
                        {
                            readLine++;
                            continue;//首行跳过
                        }
                        readLine++;
                        strLine = TF.ReadFields();
                        string strDeptName = strLine[1];
                        string strEmployeeName = strLine[1];
                        string strFingerPrintID = strLine[2];
                        string strPunchDate = strLine[3];
                        string strClockID = strLine[4];

                        Tracer.Debug("导入csv打卡记录第 " + readLine + " 行数据，员工姓名："
                            + strEmployeeName + " 指纹编码：" + strFingerPrintID
                            + " 打卡时间：" + strPunchDate
                            + " 打卡类型：" + strClockID);

                        if (string.IsNullOrEmpty(strFingerPrintID))
                        {
                            continue;
                        }

                        DateTime dtTemp = new DateTime();
                        DateTime dtPunchDate = new DateTime();
                        DateTime.TryParse(strPunchDate, out dtTemp);
                        dtPunchDate = dtTemp;
                        string strPunchTime = dtTemp.ToString("HH:mm");

                        EmployeeBLL bllEmployee = new EmployeeBLL();
                        T_HR_EMPLOYEE entEmployee = null;

                        if (entExistsEmployees.Keys.Contains(strFingerPrintID))
                        {
                            entEmployee = entExistsEmployees[strFingerPrintID];
                        }
                        else
                        {
                            entEmployee = bllEmployee.GetEmployeeByFingerPrintID(strFingerPrintID);
                            entExistsEmployees.Add(strFingerPrintID, entEmployee);
                        }

                        T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                        entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                        if (entEmployee != null)
                        {
                            entTemp.EMPLOYEEID = entEmployee.EMPLOYEEID;
                            entTemp.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                            entTemp.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                            entTemp.FINGERPRINTID = entEmployee.FINGERPRINTID;

                            entTemp.OWNERID = entEmployee.EMPLOYEEID; //.OWNERID;
                            entTemp.OWNERPOSTID = entEmployee.OWNERPOSTID;
                            entTemp.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                            entTemp.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                            entTemp.CREATECOMPANYID = entEmployee.CREATECOMPANYID;
                            entTemp.CREATEDEPARTMENTID = entEmployee.CREATEDEPARTMENTID;
                            entTemp.CREATEPOSTID = entEmployee.CREATEPOSTID;
                            entTemp.CREATEUSERID = entEmployee.CREATEUSERID;
                            entTemp.VERIFYCODE = 0;
                        }

                        entTemp.PUNCHDATE = dtPunchDate;
                        entTemp.PUNCHTIME = strPunchTime;

                        var qc = from ar in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                 where ar.OWNERCOMPANYID == entTemp.OWNERCOMPANYID && ar.EMPLOYEEID == entTemp.EMPLOYEEID && ar.PUNCHDATE == entTemp.PUNCHDATE && ar.PUNCHTIME == strPunchTime
                                 orderby ar.PUNCHDATE
                                 select ar;

                        T_HR_EMPLOYEECLOCKINRECORD entUpdate = qc.FirstOrDefault();
                        if (entUpdate != null)
                        {
                            continue;
                        }

                        strMsg = AddClockInRecord(entTemp);
                    }
                    catch (Exception ex)
                    {
                        Utility.SaveLog(ex.ToString());
                    }
                }
                TF.Close();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 读取指定路径的Excel，将文件数据转换为员工日常打卡记录，并存储到数据库
        /// </summary>
        /// <param name="strPath">当前上传的Excel文件路径</param>
        /// <param name="dtStart">导入有效数据起始日期</param>
        /// <param name="dtEnd">导入有效数据截止日期</param>
        /// <param name="strMsg">处理消息</param>
        private void ImportClockInRdListByImportExcel(string strPath, ref string strMsg)
        {
            DataTable dt = new DataTable();

            try
            {
                //dt = GetDataBySql(strPath);
                dt = GetDataFromFile(strPath);
            }
            catch
            {
                strMsg = "{EXCELUPLOADFILEERROR}";
                return;
            }

            ImportDataToList(dt, ref strMsg);
        }

        /// <summary>
        /// 读取指定路径的Excel，将数据导入到DataTable内(依靠ODBC方式读取)
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        private DataTable GetDataBySql(string strPath)
        {
            DataTable dtRes = new DataTable();
            string conn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + strPath + ";Extended Properties=Excel 8.0;";
            //Sheet1为excel中表的名字
            string sql = "select * from [Sheet1$]";
            OleDbCommand cmd = new OleDbCommand(sql, new OleDbConnection(conn));
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(dtRes);

            return dtRes;
        }

        /// <summary>
        /// 读取指定路径的Excel，将数据导入到DataTable内(依靠Office组件读取)
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private DataTable GetDataFromFile(string strPath)
        {
            DataTable dtRes = MakeTableForImport();
            dtRes.Clear();

            Excel.Application xApp = new Excel.ApplicationClass();
            xApp.Visible = true;
            try
            {
                Excel.Workbook xBook = xApp.Workbooks._Open(strPath,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value
                , Missing.Value, Missing.Value, Missing.Value, Missing.Value
                , Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                Excel.Worksheet xSheet = (Excel.Worksheet)xBook.Sheets[1];
                int iCount = xSheet.UsedRange.Rows.Count;
                if (iCount < 1)
                {
                    return dtRes;
                }

                DateTime dtCheck = new DateTime();

                for (int i = 0; i < iCount; i++)
                {
                    int iRowIndex = i + 2;  //Excel起始列从1开始计数，首列为标题列，因此数据列计数应先加2
                    Excel.Range rngDep = (Excel.Range)xSheet.Cells[iRowIndex, 1];         //所属部门
                    Excel.Range rngEmpName = (Excel.Range)xSheet.Cells[iRowIndex, 2];     //员工姓名
                    Excel.Range rngFID = (Excel.Range)xSheet.Cells[iRowIndex, 3];         //员工指纹编码
                    Excel.Range rngDate = (Excel.Range)xSheet.Cells[iRowIndex, 4];        //员工打卡时间
                    Excel.Range rngClockID = (Excel.Range)xSheet.Cells[iRowIndex, 5];     //员工卡钟序号

                    DataRow dr = dtRes.NewRow();

                    dr[0] = string.Empty;
                    if (rngDep.Text != null)
                    {
                        dr[0] = rngDep.Text.ToString().Trim();
                    }

                    dr[1] = string.Empty;
                    if (rngEmpName.Text != null)
                    {
                        dr[1] = rngEmpName.Text.ToString().Trim();
                    }

                    dr[2] = string.Empty;
                    if (rngFID.Text != null)
                    {
                        dr[2] = rngFID.Text.ToString().Trim();
                    }

                    dr[3] = string.Empty;
                    if (rngDate.Text != null)
                    {
                        DateTime dtTemp = new DateTime();
                        DateTime.TryParse(rngDate.Text.ToString().Trim(), out dtTemp);
                        if (dtTemp > dtCheck)
                        {
                            dr[3] = dtTemp;
                        }
                    }

                    dr[4] = string.Empty;
                    if (rngDep.Text != null)
                    {
                        dr[4] = rngDep.Text.ToString().Trim();
                    }

                    dtRes.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
            finally
            {
                xApp.Quit();
            }

            return dtRes;
        }

        private void ImportClockInRdListByLoginData(List<T_HR_EMPLOYEE> entEmployees, DateTime dtStart, DateTime dtEnd, ref string strMsg)
        {
            SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient PermClient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
            foreach (T_HR_EMPLOYEE item in entEmployees)
            {
                string employeeid = item.EMPLOYEEID;
                SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USERLOGINRECORD[] entLoginRds = PermClient.GetUserLoginRecordByEmployeeIDAndDate(employeeid, dtStart, dtEnd);

                if (entLoginRds == null)
                {
                    continue;
                }

                if (entLoginRds.Count() == 0)
                {
                    continue;
                }

                foreach (SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USERLOGINRECORD entLoginRd in entLoginRds)
                {
                    T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                    entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                    //员工信息
                    entTemp.EMPLOYEEID = item.EMPLOYEEID;
                    entTemp.EMPLOYEECODE = item.EMPLOYEECODE;
                    entTemp.EMPLOYEENAME = item.EMPLOYEECNAME;
                    entTemp.FINGERPRINTID = item.FINGERPRINTID;

                    //打卡信息
                    entTemp.PUNCHDATE = entLoginRd.LOGINDATE;
                    entTemp.PUNCHTIME = entLoginRd.LOGINTIME;

                    //权限信息
                    entTemp.OWNERID = item.OWNERID;
                    entTemp.OWNERPOSTID = item.OWNERPOSTID;
                    entTemp.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                    entTemp.OWNERCOMPANYID = item.OWNERCOMPANYID;
                    entTemp.CREATECOMPANYID = item.CREATECOMPANYID;
                    entTemp.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
                    entTemp.CREATEPOSTID = item.CREATEPOSTID;
                    entTemp.VERIFYCODE = 0;

                    var qc = from ar in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                             where ar.OWNERCOMPANYID == entTemp.OWNERCOMPANYID && ar.EMPLOYEEID == entTemp.EMPLOYEEID && ar.PUNCHDATE == entTemp.PUNCHDATE && ar.PUNCHTIME == entTemp.PUNCHTIME
                             orderby ar.PUNCHDATE
                             select ar;

                    T_HR_EMPLOYEECLOCKINRECORD entUpdate = qc.FirstOrDefault();
                    if (entUpdate != null)
                    {
                        continue;
                    }

                    strMsg = AddClockInRecord(entTemp);
                };
            }
        }

        /// <summary>
        /// 将指定的DataTable数据转换为员工日常打卡记录，并存储到数据库
        /// </summary>
        /// <param name="dt">指定的DataTable</param>
        /// <param name="strMsg">返回处理结果的消息</param>
        private void ImportDataToList(DataTable dt, ref string strMsg)
        {
            try
            {
                if (dt == null)
                {
                    return;
                }

                if (dt.Rows.Count == 0)
                {
                    return;
                }

                IList<T_HR_EMPLOYEECLOCKINRECORD> entList = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DateTime dtTemp = new DateTime();
                    DateTime dtPunchDate = new DateTime();
                    string strEmpName = string.Empty, strFingerPrintID = string.Empty, strClockID = string.Empty, strPunchTime = string.Empty;

                    strEmpName = dt.Rows[i][1].ToString();
                    strFingerPrintID = dt.Rows[i][2].ToString();
                    DateTime.TryParse(dt.Rows[i][3].ToString(), out dtTemp);
                    strClockID = dt.Rows[i][4].ToString();
                    //DateTime.TryParse(dtTemp.ToShortDateString(), out dtPunchDate);
                    dtPunchDate = dtTemp;
                    strPunchTime = dtTemp.ToString("HH:mm");


                    if (string.IsNullOrEmpty(strFingerPrintID))
                    {
                        continue;
                    }

                    EmployeeBLL bllEmployee = new EmployeeBLL();
                    T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByFingerPrintID(strFingerPrintID);

                    T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                    entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                    if (entEmployee != null)
                    {
                        entTemp.EMPLOYEEID = entEmployee.EMPLOYEEID;
                        entTemp.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                        entTemp.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                        entTemp.FINGERPRINTID = entEmployee.FINGERPRINTID;

                        entTemp.OWNERID = entEmployee.EMPLOYEEID; //.OWNERID;
                        entTemp.OWNERPOSTID = entEmployee.OWNERPOSTID;
                        entTemp.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                        entTemp.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                        entTemp.CREATECOMPANYID = entEmployee.CREATECOMPANYID;
                        entTemp.CREATEDEPARTMENTID = entEmployee.CREATEDEPARTMENTID;
                        entTemp.CREATEPOSTID = entEmployee.CREATEPOSTID;
                        entTemp.CREATEUSERID = entEmployee.CREATEUSERID;
                        entTemp.VERIFYCODE = 0;
                    }

                    entTemp.PUNCHDATE = dtPunchDate;
                    entTemp.PUNCHTIME = strPunchTime;

                    var qc = from ar in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                             where ar.OWNERCOMPANYID == entTemp.OWNERCOMPANYID && ar.EMPLOYEEID == entTemp.EMPLOYEEID && ar.PUNCHDATE == entTemp.PUNCHDATE && ar.PUNCHTIME == strPunchTime
                             orderby ar.PUNCHDATE
                             select ar;

                    T_HR_EMPLOYEECLOCKINRECORD entUpdate = qc.FirstOrDefault();
                    if (entUpdate != null)
                    {
                        continue;
                    }

                    strMsg = AddClockInRecord(entTemp);
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }
        }

        /// <summary>
        /// 构建导入的DataTable
        /// </summary>
        /// <returns></returns>
        private DataTable MakeTableForImport()
        {
            DataTable dt = new DataTable();

            DataColumn colDepartment = new DataColumn();
            colDepartment.ColumnName = Utility.GetResourceStr("DEPARTMENTNAME");
            colDepartment.DataType = typeof(string);
            dt.Columns.Add(colDepartment);

            DataColumn colEmployeename = new DataColumn();
            colEmployeename.ColumnName = Utility.GetResourceStr("EMPLOYEENAME");
            colEmployeename.DataType = typeof(string);
            dt.Columns.Add(colEmployeename);

            DataColumn colFingerprintid = new DataColumn();
            colFingerprintid.ColumnName = Utility.GetResourceStr("FINGERPRINTID");
            colFingerprintid.DataType = typeof(string);
            dt.Columns.Add(colFingerprintid);

            DataColumn colPunchdate = new DataColumn();
            colPunchdate.ColumnName = Utility.GetResourceStr("PUNCHDATE");
            colPunchdate.DataType = typeof(string);
            dt.Columns.Add(colPunchdate);

            DataColumn colClockID = new DataColumn();
            colClockID.ColumnName = Utility.GetResourceStr("CLOCKID");
            colClockID.DataType = typeof(string);
            dt.Columns.Add(colClockID);

            return dt;
        }

        /// <summary>
        /// 构建导出的DataTable
        /// </summary>
        /// <returns></returns>
        private DataTable MakeTableToExport()
        {
            DataTable dt = new DataTable();

            DataColumn colEmployeename = new DataColumn();
            colEmployeename.ColumnName = Utility.GetResourceStr("EMPLOYEENAME");
            colEmployeename.DataType = typeof(string);
            dt.Columns.Add(colEmployeename);

            DataColumn colFingerprintid = new DataColumn();
            colFingerprintid.ColumnName = Utility.GetResourceStr("FINGERPRINTID");
            colFingerprintid.DataType = typeof(string);
            dt.Columns.Add(colFingerprintid);

            DataColumn colPunchdate = new DataColumn();
            colPunchdate.ColumnName = Utility.GetResourceStr("PUNCHDATE");
            colPunchdate.DataType = typeof(string);
            dt.Columns.Add(colPunchdate);

            DataColumn colPunchtime = new DataColumn();
            colPunchtime.ColumnName = Utility.GetResourceStr("PUNCHTIME");
            colPunchtime.DataType = typeof(string);
            dt.Columns.Add(colPunchtime);

            return dt;
        }

        /// <summary>
        /// 将List的数据转存到DataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="ents"></param>
        /// <returns></returns>
        private DataTable GetExportData(DataTable dt, IQueryable<T_HR_EMPLOYEECLOCKINRECORD> ents)
        {
            dt.Rows.Clear();

            List<T_HR_EMPLOYEECLOCKINRECORD> entList = ents.ToList();

            for (int i = 0; i < ents.Count(); i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {
                        case 0:
                            row[n] = entList[i].EMPLOYEENAME;
                            break;
                        case 1:
                            row[n] = entList[i].FINGERPRINTID;
                            break;
                        case 2:
                            row[n] = entList[i].PUNCHDATE.Value.ToString("yyyy-M-d");
                            break;
                        case 3:
                            row[n] = entList[i].PUNCHTIME;
                            break;
                    }
                }

                dt.Rows.Add(row);

            }

            return dt;
        }

        #endregion

        /// <summary>
        /// 添加员工日常打卡记录，并返回处理结果
        /// </summary>
        /// <param name="strUserId">安全用户登录Id</param>
        /// <param name="strPwd">安全用户登录密码</param>
        /// <param name="strFingerPrintID">指纹编号</param>
        /// <param name="dtPunchStartTime">打卡起始时间</param>
        /// <param name="dtPunchEndTime">打卡截止时间</param>
        /// <returns>返回处理结果</returns>
        public string AddEmployeeClockInRd(string strUserId, string strPwd, string strFingerPrintID, DateTime dtPunchStartTime, DateTime dtPunchEndTime)
        {
            if (string.IsNullOrEmpty(strUserId) || string.IsNullOrEmpty(strPwd))
            {
                return "非法用户，不能录入考勤记录！";
            }

            if (CheckUser(strUserId, strPwd) == false)
            {
                return "非法用户，不能录入考勤记录！";
            }

            if (string.IsNullOrEmpty(strFingerPrintID))
            {
                return "缺少指纹编号，不能录入考勤记录！";
            }

            DateTime dtCheck = new DateTime();
            if (dtCheck >= dtPunchStartTime)
            {
                return "填入时间不正确，不能录入考勤记录！";
            }

            string strTemp = string.Empty;
            strTemp = AddNewClockInRdByOutPort(strFingerPrintID, dtPunchStartTime);
            strTemp = AddNewClockInRdByOutPort(strFingerPrintID, dtPunchEndTime);

            return strTemp;
        }

        private string AddNewClockInRdByOutPort(string strFingerPrintID, DateTime dtPunchTime)
        {
            string strTemp = string.Empty;
            try
            {
                DateTime dtPunchDate = new DateTime();
                string strEmpName = string.Empty, strPunchTime = string.Empty;

                DateTime.TryParse(dtPunchTime.ToShortDateString(), out dtPunchDate);
                strPunchTime = dtPunchTime.ToString("HH:mm");

                EmployeeBLL bllEmployee = new EmployeeBLL();
                T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByFingerPrintID(strFingerPrintID);

                T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();

                if (entEmployee != null)
                {
                    entTemp.EMPLOYEEID = entEmployee.EMPLOYEEID;
                    entTemp.EMPLOYEECODE = entEmployee.EMPLOYEECODE;
                    entTemp.EMPLOYEENAME = entEmployee.EMPLOYEECNAME;
                    entTemp.FINGERPRINTID = entEmployee.FINGERPRINTID;

                    entTemp.OWNERID = entEmployee.EMPLOYEEID; //.OWNERID;
                    entTemp.OWNERPOSTID = entEmployee.OWNERPOSTID;
                    entTemp.OWNERDEPARTMENTID = entEmployee.OWNERDEPARTMENTID;
                    entTemp.OWNERCOMPANYID = entEmployee.OWNERCOMPANYID;
                    entTemp.CREATECOMPANYID = entEmployee.CREATECOMPANYID;
                    entTemp.CREATEDEPARTMENTID = entEmployee.CREATEDEPARTMENTID;
                    entTemp.CREATEPOSTID = entEmployee.CREATEPOSTID;
                    entTemp.CREATEUSERID = entEmployee.CREATEUSERID;
                    entTemp.VERIFYCODE = 0;
                }

                entTemp.PUNCHDATE = dtPunchDate;
                entTemp.PUNCHTIME = strPunchTime;

                strTemp = AddClockInRecord(entTemp);

                if (strTemp == "{SAVESUCCESSED}" && !string.IsNullOrEmpty(entTemp.EMPLOYEEID))
                {
                    string[] ids = new string[] { entEmployee.EMPLOYEEID };
                    DateTime dtStart = dtPunchDate;
                    DateTime dtEnd = dtPunchDate;
                    List<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByIDs(ids);
                    AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                    bllAbnormRecord.CheckAbnormRecordForEmployees(entEmployees, dtStart, dtEnd, ref strTemp);
                }

            }
            catch
            {
                strTemp = "添加失败！";
            }

            return strTemp;
        }

    }
}
