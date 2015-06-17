using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using TM_SaaS_OA_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data.Objects.DataClasses;
using SMT.HRM.BLL.Common;
using SMT.Foundation.Log;
using System.Threading;
using SMT.HRM.CustomModel.Common;
using SMT.HRM.CustomModel.Response;
using SMT.HRM.CustomModel.Request;
using SMT.HRM.BLL.Permission;

namespace SMT.HRM.BLL
{
    public class EmployeeLeaveRecordBLL : BaseBll<T_HR_EMPLOYEELEAVERECORD>, ILookupEntity, IOperate
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
        public IQueryable<V_EmpLeaveRdInfo> EmployeeLeaveRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {
            try
            {
                if (strCheckState != Convert.ToInt32(SMT.HRM.DAL.CheckStates.WaittingApproval).ToString())
                {
                    if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.All).ToString())
                    {
                        strCheckState = string.Empty;
                    }

                    SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                }
                else
                {
                    string strCheckfilter = string.Copy(filterString);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
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

                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = dal.GetObjects().Include("T_HR_LEAVETYPESET");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray());
                }
                //if (!string.IsNullOrEmpty(recorderDate))
                //{
                //    DateTime tmpDate = Convert.ToDateTime(recorderDate);
                //    ents = ents.Where(p => p.STARTDATETIME.Value.Year == tmpDate.Year && p.STARTDATETIME.Value.Month == tmpDate.Month);
                //}
                ents = ents.OrderBy(sort);

                var entrs = from e in ents
                            join vDepartment in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals vDepartment.DEPARTMENTID
                            select new V_EmpLeaveRdInfo
                            {
                                CHECKSTATE = e.CHECKSTATE,
                                OWNERCOMPANYID = e.OWNERCOMPANYID,
                                OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                                DEPARTMENTNAME = vDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                OWNERPOSTID = e.OWNERPOSTID,
                                OWNERID = e.OWNERID,
                                EMPLOYEEID = e.EMPLOYEEID,
                                EMPLOYEECODE = e.EMPLOYEECODE,
                                EMPLOYEENAME = e.EMPLOYEENAME,
                                LEAVEDAYS = e.LEAVEDAYS,
                                LEAVEHOURS = e.LEAVEHOURS,
                                TOTALHOURS = e.TOTALHOURS,
                                STARTDATETIME = e.STARTDATETIME,
                                ENDDATETIME = e.ENDDATETIME,
                                LEAVERECORDID = e.LEAVERECORDID,
                                LEAVETYPENAME = e.T_HR_LEAVETYPESET.LEAVETYPENAME,
                                CREATECOMPANYID = e.CREATECOMPANYID,
                                CREATEDEPARTMENTID = e.CREATEDEPARTMENTID,
                                CREATEPOSTID = e.CREATEPOSTID,
                                CREATEUSERID = e.CREATEUSERID,
                                CREATEDATE = e.CREATEDATE,
                                UPDATEDATE = e.UPDATEDATE,
                                CANCELTOTALHOURS = e.T_HR_EMPLOYEECANCELLEAVE.Where(t => t.CHECKSTATE == "2").Sum(t => t.TOTALHOURS),
                                REASON = e.REASON
                            };
                entrs = entrs.OrderBy(sort);
                entrs = Utility.Pager<V_EmpLeaveRdInfo>(entrs, pageIndex, pageSize, ref pageCount);

                return entrs;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工请假发生错误：EmployeeLeaveRecordPaging：" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">所有者</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="recorderDate">截止日期</param>
        /// <returns>查询结果集</returns>
        public IQueryable<V_EmpLeaveRdInfo> EmployeeLeaveRecordPaged(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string startDate, string recorderDate, string employeeID, string leaveTypeSetID)
        {
            try
            {
                string filter1 = string.Empty, filter2 = string.Empty, filter3 = string.Empty, filter4 = string.Empty;
                List<object> paraTemp1 = new List<object>();
                List<object> paraTemp2 = new List<object>();
                List<object> paraTemp3 = new List<object>();
                List<object> paraTemp4 = new List<object>();
                //开始月份在请假开始时间
                if (!string.IsNullOrEmpty(employeeID))
                {
                    if (!string.IsNullOrEmpty(filter1))
                    {
                        filter1 += " and ";
                    }
                    filter1 += "EMPLOYEEID==@" + paraTemp1.Count().ToString();
                    paraTemp1.Add(employeeID);
                }
                if (!string.IsNullOrEmpty(leaveTypeSetID))
                {
                    if (!string.IsNullOrEmpty(filter1))
                    {
                        filter1 += " and ";
                    }
                    filter1 += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paraTemp1.Count().ToString();
                    paraTemp1.Add(leaveTypeSetID);
                }
                //开始月份在请假开始时间
                if (!string.IsNullOrEmpty(employeeID))
                {
                    if (!string.IsNullOrEmpty(filter2))
                    {
                        filter2 += " and ";
                    }
                    filter2 += "EMPLOYEEID==@" + paraTemp2.Count().ToString();
                    paraTemp2.Add(employeeID);
                }
                if (!string.IsNullOrEmpty(leaveTypeSetID))
                {
                    if (!string.IsNullOrEmpty(filter2))
                    {
                        filter2 += " and ";
                    }
                    filter2 += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paraTemp2.Count().ToString();
                    paraTemp2.Add(leaveTypeSetID);
                }
                //截止月份在请假开始时间
                if (!string.IsNullOrEmpty(employeeID))
                {
                    if (!string.IsNullOrEmpty(filter3))
                    {
                        filter3 += " and ";
                    }
                    filter3 += "EMPLOYEEID==@" + paraTemp3.Count().ToString();
                    paraTemp3.Add(employeeID);
                }
                if (!string.IsNullOrEmpty(leaveTypeSetID))
                {
                    if (!string.IsNullOrEmpty(filter3))
                    {
                        filter3 += " and ";
                    }
                    filter3 += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paraTemp3.Count().ToString();
                    paraTemp3.Add(leaveTypeSetID);
                }
                //开始月份在请假截止时间
                if (!string.IsNullOrEmpty(employeeID))
                {
                    if (!string.IsNullOrEmpty(filter4))
                    {
                        filter4 += " and ";
                    }
                    filter4 += "EMPLOYEEID==@" + paraTemp4.Count().ToString();
                    paraTemp4.Add(employeeID);
                }
                if (!string.IsNullOrEmpty(leaveTypeSetID))
                {
                    if (!string.IsNullOrEmpty(filter4))
                    {
                        filter4 += " and ";
                    }
                    filter4 += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paraTemp4.Count().ToString();
                    paraTemp4.Add(leaveTypeSetID);
                }
                //处理跨月
                //if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(recorderDate))
                //{
                //    //开始月份在请假开始时间
                //    DateTime startFirstDay = new DateTime(Convert.ToDateTime(startDate).Year, Convert.ToDateTime(startDate).Month, 1);
                //    DateTime startLastDay = new DateTime(Convert.ToDateTime(startDate).AddMonths(1).Year, Convert.ToDateTime(startDate).AddMonths(1).Month, 1).AddDays(-1);
                //    if (!string.IsNullOrEmpty(filter1))
                //    {
                //        filter1 += " and ";
                //    }
                //    filter1 += " STARTDATETIME>=@" + paraTemp1.Count().ToString();
                //    paraTemp1.Add(startFirstDay);
                //    filter1 += " and STARTDATETIME<=@" + paraTemp1.Count().ToString();
                //    paraTemp1.Add(startLastDay);
                //    filter1 += " and ENDDATETIME>=@" + paraTemp1.Count().ToString();
                //    paraTemp1.Add(startLastDay);
                //    //截止月份在请假开始时间
                //    if (!string.IsNullOrEmpty(filter4))
                //    {
                //        filter4 += " and ";
                //    }
                //    filter4 += " STARTDATETIME<@" + paraTemp4.Count().ToString();
                //    paraTemp4.Add(startFirstDay);
                //    filter4 += " and ENDDATETIME>=@" + paraTemp4.Count().ToString();
                //    paraTemp4.Add(startFirstDay);
                //    filter4 += " and ENDDATETIME<=@" + paraTemp4.Count().ToString();
                //    paraTemp4.Add(startLastDay);
                //    DateTime recorderFirstDay = new DateTime(Convert.ToDateTime(recorderDate).AddMonths(-1).Year, Convert.ToDateTime(recorderDate).AddMonths(-1).Month, 1);
                //    DateTime recorderLastDay = new DateTime(Convert.ToDateTime(recorderDate).Year, Convert.ToDateTime(recorderDate).Month, 1).AddDays(-1);
                //    //截止月份在请假截止时间
                //    if (!string.IsNullOrEmpty(filter2))
                //    {
                //        filter2 += " and ";
                //    }
                //    filter2 += " STARTDATETIME<@" + paraTemp2.Count().ToString();
                //    paraTemp2.Add(recorderFirstDay);
                //    filter2 += " and ENDDATETIME>=@" + paraTemp2.Count().ToString();
                //    paraTemp2.Add(recorderFirstDay);
                //    filter2 += " and ENDDATETIME<=@" + paraTemp2.Count().ToString();
                //    paraTemp2.Add(recorderLastDay);
                //    //开始月份在请假截止时间
                //    if (!string.IsNullOrEmpty(filter3))
                //    {
                //        filter3 += " and ";
                //    }
                //    filter3 += " STARTDATETIME>=@" + paraTemp3.Count().ToString();
                //    paraTemp3.Add(recorderFirstDay);
                //    filter3 += " and STARTDATETIME<=@" + paraTemp3.Count().ToString();
                //    paraTemp3.Add(recorderLastDay);
                //    filter3 += " and ENDDATETIME>@" + paraTemp3.Count().ToString();
                //    paraTemp3.Add(recorderLastDay);
                //}
                if (strCheckState != Convert.ToInt32(SMT.HRM.BLL.Common.CheckStates.WaittingApproval).ToString())
                {
                    if (strCheckState == Convert.ToInt32(SMT.HRM.BLL.Common.CheckStates.All).ToString())
                    {
                        strCheckState = string.Empty;
                    }
                    SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                    SetOrganizationFilter(ref filter1, ref paraTemp1, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                    SetOrganizationFilter(ref filter2, ref paraTemp2, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                    SetOrganizationFilter(ref filter3, ref paraTemp3, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                    SetOrganizationFilter(ref filter4, ref paraTemp4, strOwnerID, "T_HR_EMPLOYEELEAVERECORD");
                }
                else
                {
                    string strCheckfilter = string.Copy(filterString);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filter1, ref paraTemp1);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filter2, ref paraTemp2);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filter3, ref paraTemp3);
                    SetFilterWithflow("LEAVERECORDID", "T_HR_EMPLOYEELEAVERECORD", strOwnerID, ref strCheckState, ref filter4, ref paraTemp4);
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
                if (!string.IsNullOrEmpty(strCheckState))
                {
                    int iIndex1 = 0;
                    if (!string.IsNullOrEmpty(filter1))
                    {
                        filter1 += " AND";
                    }

                    if (paraTemp1.Count() > 0)
                    {
                        iIndex1 = paraTemp1.Count();
                    }

                    filter1 += " CHECKSTATE == @" + iIndex1.ToString();
                    paraTemp1.Add(strCheckState);
                }
                if (!string.IsNullOrEmpty(strCheckState))
                {
                    int iIndex2 = 0;
                    if (!string.IsNullOrEmpty(filter2))
                    {
                        filter2 += " AND";
                    }

                    if (paraTemp2.Count() > 0)
                    {
                        iIndex2 = paraTemp2.Count();
                    }

                    filter2 += " CHECKSTATE == @" + iIndex2.ToString();
                    paraTemp2.Add(strCheckState);
                }
                if (!string.IsNullOrEmpty(strCheckState))
                {
                    int iIndex3 = 0;
                    if (!string.IsNullOrEmpty(filter3))
                    {
                        filter3 += " AND";
                    }

                    if (paraTemp3.Count() > 0)
                    {
                        iIndex3 = paraTemp3.Count();
                    }

                    filter3 += " CHECKSTATE == @" + iIndex3.ToString();
                    paraTemp3.Add(strCheckState);
                }
                if (!string.IsNullOrEmpty(strCheckState))
                {
                    int iIndex4 = 0;
                    if (!string.IsNullOrEmpty(filter4))
                    {
                        filter4 += " AND";
                    }

                    if (paraTemp4.Count() > 0)
                    {
                        iIndex4 = paraTemp4.Count();
                    }

                    filter4 += " CHECKSTATE == @" + iIndex4.ToString();
                    paraTemp4.Add(strCheckState);
                }
                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = dal.GetObjects().Include("T_HR_LEAVETYPESET");
                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents1 = null;
                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents2 = null;
                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents3 = null;
                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents4 = null;
                if (!string.IsNullOrEmpty(filter1))
                {
                    ents1 = ents.Where(filter1, paraTemp1.ToArray());
                }
                if (!string.IsNullOrEmpty(filter2))
                {
                    ents2 = ents.Where(filter2, paraTemp2.ToArray());
                }
                if (!string.IsNullOrEmpty(filter3))
                {
                    ents3 = ents.Where(filter3, paraTemp3.ToArray());
                }
                if (!string.IsNullOrEmpty(filter4))
                {
                    ents4 = ents.Where(filter4, paraTemp4.ToArray());
                }
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray()).Union(ents1).Union(ents2).Union(ents3).Union(ents4);
                }
                DateTime dtstart = DateTime.Parse(startDate);
                DateTime dtend = DateTime.Parse(recorderDate);

                ents = from ent in ents
                       where (ent.STARTDATETIME >= dtstart && ent.ENDDATETIME <= dtend)
                       || (ent.STARTDATETIME >= dtstart && ent.ENDDATETIME <= dtend)
                       || (ent.STARTDATETIME <= dtend && ent.ENDDATETIME >= dtend)
                       || (ent.STARTDATETIME >= dtstart && ent.ENDDATETIME <= dtend)
                       || (ent.ENDDATETIME >= dtstart && ent.ENDDATETIME <= dtend)
                       select ent;
                //if (!string.IsNullOrEmpty(recorderDate))
                //{
                //    DateTime tmpDate = Convert.ToDateTime(recorderDate);
                //    ents = ents.Where(p => p.STARTDATETIME.Value.Year == tmpDate.Year && p.STARTDATETIME.Value.Month == tmpDate.Month);
                //}
                //ents = ents.OrderBy(sort);

                var entrs = from e in ents
                            join vDepartment in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals vDepartment.DEPARTMENTID
                            select new V_EmpLeaveRdInfo
                            {
                                CHECKSTATE = e.CHECKSTATE,
                                OWNERCOMPANYID = e.OWNERCOMPANYID,
                                OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                                DEPARTMENTNAME = vDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                OWNERPOSTID = e.OWNERPOSTID,
                                OWNERID = e.OWNERID,
                                EMPLOYEEID = e.EMPLOYEEID,
                                EMPLOYEECODE = e.EMPLOYEECODE,
                                EMPLOYEENAME = e.EMPLOYEENAME,
                                LEAVEDAYS = e.LEAVEDAYS,
                                LEAVEHOURS = e.LEAVEHOURS,
                                TOTALHOURS = e.TOTALHOURS,
                                STARTDATETIME = e.STARTDATETIME,
                                ENDDATETIME = e.ENDDATETIME,
                                LEAVERECORDID = e.LEAVERECORDID,
                                LEAVETYPEVALUE = e.T_HR_LEAVETYPESET.LEAVETYPEVALUE,
                                LEAVETYPEID = e.T_HR_LEAVETYPESET.LEAVETYPESETID,
                                LEAVETYPENAME = e.T_HR_LEAVETYPESET.LEAVETYPENAME,
                                CREATECOMPANYID = e.CREATECOMPANYID,
                                CREATEDEPARTMENTID = e.CREATEDEPARTMENTID,
                                CREATEPOSTID = e.CREATEPOSTID,
                                CREATEUSERID = e.CREATEUSERID,
                                CREATEDATE = e.CREATEDATE,
                                UPDATEDATE = e.UPDATEDATE,
                                CANCELTOTALHOURS = e.T_HR_EMPLOYEECANCELLEAVE.Where(t => t.CHECKSTATE == "2").Sum(t => t.TOTALHOURS),
                                REASON = e.REASON
                            };
                entrs = entrs.OrderBy(sort);

                //entrs = Utility.Pager<V_EmpLeaveRdInfo>(entrs, pageIndex, pageSize, ref pageCount);

                return entrs;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工请假发生错误：EmployeeLeaveRecordPaging：" + ex.ToString());
                return null;
            }
        }
        #region 导出报表
        /// <summary>
        /// 导出请假报表
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">所有者</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="recorderDate">截止日期</param>
        /// <returns>查询结果集</returns>
        public byte[] ExportEmployeeLeaveRecordReports(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string startDate, string recorderDate, string employeeID, string leaveTypeSetID)
        {

            byte[] result = null;
            try
            {
                List<V_EmpLeaveRdInfo> entlist = new List<V_EmpLeaveRdInfo>();
                IQueryable<V_EmpLeaveRdInfo> leaveRecordInfos = EmployeeLeaveRecordPaged(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID, startDate, recorderDate, employeeID, leaveTypeSetID);
                if (leaveRecordInfos.Count() > 0)
                {
                    entlist = leaveRecordInfos.ToList();
                }
                result = EmployeeLeaveRecordStream(entlist);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeftOfficeConfirmReports:" + ex.Message);

            }
            return result;


        }

        public static byte[] EmployeeLeaveRecordStream(List<V_EmpLeaveRdInfo> leaveRecordInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeLeaveRecordBody(leaveRecordInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        public static StringBuilder GetEmployeeLeaveRecordBody(List<V_EmpLeaveRdInfo> Collects)
        {
            StringBuilder s = new StringBuilder();
            List<T_SYS_DICTIONARY> tmp=new List<T_SYS_DICTIONARY>();//= new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "CHECKSTATE" });
            using (SysDictionaryBLL bll = new SysDictionaryBLL())
            {
                tmp = bll.GetSysDictionaryByCategory(new List<string>{ "CHECKSTATE" });
            }
            
            string checkStateName = string.Empty;
            s.Append("<body>\n\r");
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >员工编号</td>");
            s.Append("<td align=center class=\"title\" >员工姓名</td>");
            s.Append("<td align=center class=\"title\" >部门</td>");
            s.Append("<td align=center class=\"title\" >假期标准</td>");
            s.Append("<td align=center class=\"title\" >开始时间</td>");
            s.Append("<td align=center class=\"title\" >结束时间</td>");
            s.Append("<td align=center class=\"title\" >请假天数</td>");
            s.Append("<td align=center class=\"title\" >请假时长(小时)</td>");
            s.Append("<td align=center class=\"title\" >审批状态</td>");
            s.Append("<td align=center class=\"title\" >原因</td>");
            s.Append("<td align=center class=\"title\" >销假合计时长(小时)</td>");
            s.Append("</tr>");

            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    try
                    {
                        checkStateName = tmp.Where(e => e.DICTIONARYVALUE == decimal.Parse(Collects[i].CHECKSTATE.Trim())).FirstOrDefault().DICTIONARYNAME;
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug("GetEmployeeLeaveRecordBody:" + ex.Message);
                    }
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECODE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].DEPARTMENTNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVETYPENAME + "</td>");//应出勤天数，注释的为原来的
                    s.Append("<td class=\"x1282\">" + Collects[i].STARTDATETIME.Value.ToString("yyyy-MM-dd HH:mm") + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ENDDATETIME.Value.ToString("yyyy-MM-dd HH:mm") + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVEDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVEHOURS + "</td>");
                    s.Append("<td class=\"x1282\">" + checkStateName + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].REASON + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].CANCELTOTALHOURS + "</td>");
                    s.Append("</tr>");
                }
            }
            s.Append("</table>");
            s.Append("</body></html>");
            return s;
        }



        #endregion
        /// <summary>
        /// 根据员工ID，请假的起止时间查询请假记录
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEELEAVERECORD> GetEmployeeLeaveRdListByEmployeeIDAndDate(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState)
        {
            if (string.IsNullOrEmpty(strEmployeeID))
            {
                return null;
            }

            StringBuilder strTemps = new StringBuilder();

            IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                                                        where e.EMPLOYEEID == strEmployeeID
                                                        && e.CHECKSTATE == strCheckState
                                                        && (
                                                        (e.STARTDATETIME <= dtStart && e.ENDDATETIME >= dtStart)
                                                        || (e.STARTDATETIME <= dtEnd && e.ENDDATETIME >= dtEnd)
                                                        || (e.STARTDATETIME >= dtStart && e.ENDDATETIME <= dtEnd)
                                                        )
                                                        select e;

            if (ents.Count() > 0)
            {
                return ents;
            }
            return null;
            //IQueryable<T_HR_EMPLOYEELEAVERECORD> qEnds = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
            //                                             where e.EMPLOYEEID == strEmployeeID && e.ENDDATETIME > dtStart && e.ENDDATETIME <= dtEnd && e.CHECKSTATE == strCheckState
            //                                             select e;

            //if (qEnds.Count() > 0)
            //{
            //    foreach (T_HR_EMPLOYEELEAVERECORD item in qEnds)
            //    {
            //        if (strTemps.ToString().Contains(item.LEAVERECORDID))
            //        {
            //            continue;
            //        }
            //        strTemps.Append(item.LEAVERECORDID + ",");
            //    }
            //}

            //IQueryable<T_HR_EMPLOYEELEAVERECORD> qCrosss = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
            //                                               where e.EMPLOYEEID == strEmployeeID && e.STARTDATETIME < dtStart && e.ENDDATETIME > dtEnd && e.CHECKSTATE == strCheckState
            //                                               select e;

            //if (qCrosss.Count() > 0)
            //{
            //    foreach (T_HR_EMPLOYEELEAVERECORD item in qCrosss)
            //    {
            //        if (strTemps.ToString().Contains(item.LEAVERECORDID))
            //        {
            //            continue;
            //        }
            //        strTemps.Append(item.LEAVERECORDID + ",");
            //    }
            //}

            //IQueryable<T_HR_EMPLOYEELEAVERECORD> qCrosssSpec = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
            //                                               where e.EMPLOYEEID == strEmployeeID && e.STARTDATETIME < dtEnd && e.ENDDATETIME > dtEnd && e.CHECKSTATE == strCheckState
            //                                               select e;

            //if (qCrosssSpec.Count() > 0)
            //{
            //    foreach (T_HR_EMPLOYEELEAVERECORD item in qCrosssSpec)
            //    {
            //        if (strTemps.ToString().Contains(item.LEAVERECORDID))
            //        {
            //            continue;
            //        }
            //        strTemps.Append(item.LEAVERECORDID + ",");
            //    }
            //}

            //string strLeaveIds = strTemps.ToString();

            //IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
            //                                            where e.LEAVERECORDID.Contains(strLeaveIds)
            //                                            select e;

        }

        /// <summary>
        /// 根据请假记录ID获取员工请假信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEELEAVERECORD GetLeaveRecordByID(string strID)
        {
            if (string.IsNullOrWhiteSpace(strID))
            {
                return null;
            }

            IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                                                        where e.LEAVERECORDID == strID
                                                        select e;

            if (ents.Count() == 0)
            {
                return null;
            }

            return ents.FirstOrDefault();
        }

        /// <summary>
        /// 根据请假记录ID、员工ID获取信息
        /// </summary>
        /// <param name="strID"></param>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public V_EMPLOYEELEAVERECORD GetEmployeeLeaveRecordByID(string strID)
        {
            V_EMPLOYEELEAVERECORD entity = new V_EMPLOYEELEAVERECORD();
            try
            {

                AttendanceSolutionAsignBLL asignBll = new AttendanceSolutionAsignBLL();
                FreeLeaveDaySetBLL bll = new FreeLeaveDaySetBLL();
                //根据请假记录ID获取请假记录信息
                entity.EmployeeLeaveRecord = dal.GetObjects().Include("T_HR_LEAVETYPESET").FirstOrDefault(s => s.LEAVERECORDID == strID);

                //根据请假记录ID获取请假调休记录信息
                var ent = from a in dal.GetObjects<T_HR_ADJUSTLEAVE>()
                          where a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == strID
                          select a;
                entity.AdjustLeave = ent.Count() > 0 ? ent.ToList() : null;
                if (entity.EmployeeLeaveRecord != null)
                {
                    entity.EmployeeLeave = bll.GetFreeLeaveDaySetByEmployeeID(entity.EmployeeLeaveRecord.EMPLOYEEID);
                    //每天工作时长
                    var temp = asignBll.GetAttendanceSolutionAsignByEmployeeID(entity.EmployeeLeaveRecord.EMPLOYEEID);
                    if (temp != null)
                    {
                        entity.WorkTimePerDay = temp.T_HR_ATTENDANCESOLUTION.WORKTIMEPERDAY.Value;
                    }
                    else
                    {
                        entity.WorkTimePerDay = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("GetEmployeeLeaveRecordByID异常：" + ex.ToString());
            }
            return entity;
        }

        /// <summary>
        /// 获取员工同类型假本年已用天数，本月已用天数，冲减带薪假已用天数
        /// </summary>
        /// <param name="entEmployeeView">员工信息(主岗位)</param>
        /// <param name="strLeaveRecordId">当前请假记录ID</param>
        /// <param name="strLeaveTypeSetId">当前请假对应的请假标准ID</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假结束时间</param>
        /// <param name="dLeaveYearDays">同类型假本年已用天数</param>
        /// <param name="dLeaveMonthDays">本月已用天数</param>
        /// <param name="dAdjLevPaidDays">冲减带薪假已用天数</param>
        public void GetLeaveDaysHistory(V_EMPLOYEEVIEW entEmployeeView, string strLeaveRecordId, string strLeaveTypeSetId, DateTime dtLeaveStartTime,
            DateTime dtLeaveEndTime, ref decimal dLeaveYearDays, ref decimal dLeaveMonthDays, ref decimal dAdjLevPaidDays)
        {
            if (entEmployeeView == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(strLeaveTypeSetId) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(entEmployeeView.EMPLOYEEID) || string.IsNullOrWhiteSpace(entEmployeeView.OWNERCOMPANYID))
            {
                return;
            }

            LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
            T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveTypeSetId);

            if (entLeaveTypeSet == null)
            {
                return;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(entEmployeeView.EMPLOYEEID, dtLeaveStartTime);
            if (entAttendSolAsign == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            //获取考勤方案
            T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;

            DateTime dtYearStart = new DateTime();
            DateTime dtMonthStart = new DateTime();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-1-1", out dtYearStart);
            DateTime.TryParse(dtLeaveStartTime.ToString("yyyy-MM") + "-1", out dtMonthStart);

            var ey = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId && e.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && e.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     && e.STARTDATETIME >= dtYearStart && e.ENDDATETIME <= dtLeaveEndTime && e.CHECKSTATE == strCheckState
                     select e;

            var em = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId && e.OWNERCOMPANYID == entEmployeeView.OWNERCOMPANYID && e.EMPLOYEEID == entEmployeeView.EMPLOYEEID
                     && e.STARTDATETIME >= dtMonthStart && e.ENDDATETIME <= dtLeaveEndTime && e.CHECKSTATE == strCheckState
                     select e;

            var ec = from c in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                     join l in em on c.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals l.LEAVERECORDID
                     select c;

            if (ey.Count() == 0)
            {
                return;
            }

            decimal dCancelLeaveDays = 0;
            foreach (T_HR_EMPLOYEECANCELLEAVE item in ec)
            {
                dCancelLeaveDays += item.TOTALHOURS.Value;
            }

            foreach (T_HR_EMPLOYEELEAVERECORD item in ey)
            {
                if (item.TOTALHOURS != null)
                {
                    dLeaveYearDays += item.TOTALHOURS.Value;
                }
            }

            dLeaveYearDays = RoundOff((dLeaveYearDays - dCancelLeaveDays) / entAttendSol.WORKTIMEPERDAY.Value, "0.5", 1);

            if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
            {
                dAdjLevPaidDays = dLeaveYearDays;
            }

            if (em.Count() == 0)
            {
                return;
            }

            foreach (T_HR_EMPLOYEELEAVERECORD item in ey)
            {
                dLeaveMonthDays += item.TOTALHOURS.Value;
            }

            dLeaveMonthDays = RoundOff((dLeaveMonthDays - dCancelLeaveDays) / entAttendSol.WORKTIMEPERDAY.Value, "0.5", 1);

            if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Deduct) + 1).ToString())
            {
                return;
            }

            var ep = from ad in dal.GetObjects<T_HR_ADJUSTLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                     join l in em on ad.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals l.LEAVERECORDID
                     select ad;

            foreach (T_HR_ADJUSTLEAVE item in ep)
            {
                if (item.ADJUSTLEAVEDAYS != null)
                {
                    dAdjLevPaidDays += item.ADJUSTLEAVEDAYS.Value;
                }
            }
        }

        /// <summary>
        /// 获取同类假期年度总次数，年度总时长，月度总次数，月度总次数，月度总时长
        /// </summary>
        /// <param name="strLeaveTypeSetId">假期标准Id</param>
        /// <param name="strLeaveRecordId">请假记录Id</param>
        /// <param name="strEmployeeID">员工Id</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假截止时间</param>
        /// <param name="dLeaveYearTimes">同类假期年度总次数</param>
        /// <param name="dLeaveYearDays">同类假期年度总时长</param>
        /// <param name="dLeaveMonthTimes">同类假期月度总次数</param>
        /// <param name="dLeaveMonthDays">同类假期月度总时长</param>
        public void GetLeaveDaysHistory(string strLeaveTypeSetId, string strLeaveRecordId, string strEmployeeID,
            DateTime dtLeaveStartTime, DateTime dtLeaveEndTime, ref decimal dLeaveYearTimes,
            ref decimal dLeaveYearDays, ref decimal dLeaveMonthTimes, ref decimal dLeaveMonthDays, ref DateTime dLeaveFistDate, ref decimal dLeaveSYearTimes)
        {
            if (string.IsNullOrWhiteSpace(strLeaveTypeSetId) || string.IsNullOrWhiteSpace(strLeaveRecordId) || string.IsNullOrWhiteSpace(strEmployeeID))
            {
                return;
            }

            AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
            T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtLeaveStartTime);
            if (entAttendSolAsign == null)
            {
                //当前员工没有分配考勤方案，无法提交请假申请
                return;
            }

            //获取考勤方案
            T_HR_ATTENDANCESOLUTION entAttendSol = entAttendSolAsign.T_HR_ATTENDANCESOLUTION;

            DateTime dtYearStart = new DateTime();
            DateTime dtMonthStart = new DateTime();
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            DateTime.TryParse(dtLeaveStartTime.Year.ToString() + "-1-1", out dtYearStart);
            DateTime.TryParse(dtLeaveStartTime.ToString("yyyy-MM") + "-1", out dtMonthStart);

            //查询上一年请假
            DateTime dtSYearStart = new DateTime();
            DateTime.TryParse((dtLeaveStartTime.Year - 1).ToString() + "-1-1", out dtSYearStart);
            var eu = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId
                     && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId && e.EMPLOYEEID == strEmployeeID
                     && e.STARTDATETIME >= dtSYearStart && e.ENDDATETIME <= dtLeaveEndTime && e.CHECKSTATE == strCheckState
                     select e;
            dLeaveSYearTimes = eu.Count();
            //取得本年时间段内此请假类型，第一次的请假时间是什么时候
            if (eu.Count() != 0)
            {
                var ep = (from e in eu
                          orderby e.CREATEDATE ascending
                          select e).FirstOrDefault().STARTDATETIME;
                if (ep != null)
                {
                    dLeaveFistDate = DateTime.Parse(ep.ToString());
                }
            }

            var ey = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId && e.EMPLOYEEID == strEmployeeID
                     && e.STARTDATETIME >= dtYearStart && e.ENDDATETIME <= dtLeaveEndTime && e.CHECKSTATE == strCheckState
                     select e;

            var em = from e in dal.GetObjects().Include("T_HR_LEAVETYPESET")
                     where e.LEAVERECORDID != strLeaveRecordId && e.T_HR_LEAVETYPESET.LEAVETYPESETID == strLeaveTypeSetId && e.EMPLOYEEID == strEmployeeID
                     && e.STARTDATETIME >= dtMonthStart && e.ENDDATETIME <= dtLeaveEndTime && e.CHECKSTATE == strCheckState
                     select e;

            if (ey.Count() == 0)
            {
                return;
            }

            //本年请假次数
            dLeaveYearTimes = ey.Count();
            foreach (T_HR_EMPLOYEELEAVERECORD item in ey)
            {
                if (item.TOTALHOURS != null)
                {
                    dLeaveYearDays += item.TOTALHOURS.Value;
                }
            }

            dLeaveYearDays = RoundOff(dLeaveYearDays / entAttendSol.WORKTIMEPERDAY.Value, "0.5", 1);

            if (em.Count() == 0)
            {
                return;
            }

            dLeaveMonthTimes = em.Count();
            foreach (T_HR_EMPLOYEELEAVERECORD item in em)
            {
                dLeaveMonthDays += item.TOTALHOURS.Value;
            }

            dLeaveMonthDays = RoundOff(dLeaveMonthDays / entAttendSol.WORKTIMEPERDAY.Value, "0.5", 1);
        }

        private void CalculateEmployeeLevelDayCount(string fingerprintids)
        {
            string[] arr = fingerprintids.Split(',');
            foreach (string s in arr)
            {
                if (s.Length > 0)
                {
                    var employee = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                   where e.FINGERPRINTID == s.Trim()
                                   select e;

                    if (employee == null)
                    {
                        return;
                    }
                    else
                    {
                        AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();
                        T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttSolAsign.GetAttendanceSolutionAsignByID("D65F9765-14BB-4712-A646-8E28125794DD");

                        EmployeeLevelDayCountBLL bllLevelDayCount = new EmployeeLevelDayCountBLL();
                        bllLevelDayCount.CalculateEmployeeLevelDayCount(entAttSolAsign, employee.FirstOrDefault(), "0");
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定员工的实际请假天数(实际请假天数=请假天数-公休假天数-每周休息天数+休假调剂工作天数)，实际请假时长(按小时计，实际请假时长=非整天请假时长-当日作息间隙休息时间)
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStartTime">请假起始时间</param>
        /// <param name="dtLeaveEndTime">请假截止时间</param>
        /// <param name="dLeaveDay">实际请假天数</param>
        /// <param name="dLeaveTime">实际请假时长</param>
        /// <param name="dLeaveTotalTime">实际请假时长</param>
        public string GetRealLeaveDayByEmployeeIdAndDate(string strLeaveRecordId, string strEmployeeID, DateTime dtLeaveStartTime,
            DateTime dtLeaveEndTime, ref decimal dLeaveDay, ref decimal dLeaveTime, ref decimal dLeaveTotalTime)
        {
            string strMsg = string.Empty;
            try
            {
                string[] strArr = strLeaveRecordId.Split('|');
                string LeaveTypeValue = string.Empty;//
                if (strArr.Count() >= 2)
                {
                    string strLeaveSetId = strArr[1];
                    strLeaveRecordId = strArr[0];
                    LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
                    T_HR_LEAVETYPESET entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(strLeaveSetId);
                    if (entLeaveTypeSet != null)
                    {
                        LeaveTypeValue = entLeaveTypeSet.LEAVETYPEVALUE;
                    }
                }

                #region 有数据情况则直接返回
                T_HR_EMPLOYEELEAVERECORD entLeaveRecord = GetLeaveRecordByID(strLeaveRecordId);
                bool flag = false;
                //考勤方案生成带薪假测试结束
                if (entLeaveRecord != null)
                {
                    if (entLeaveRecord.STARTDATETIME == dtLeaveStartTime && entLeaveRecord.ENDDATETIME == dtLeaveEndTime)
                    {
                        if (entLeaveRecord.LEAVEDAYS == null)
                        {
                            dLeaveDay = 0;
                        }
                        else
                        {
                            dLeaveDay = entLeaveRecord.LEAVEDAYS.Value;
                        }

                        if (entLeaveRecord.LEAVEHOURS == null)
                        {
                            dLeaveTime = 0;
                        }
                        else
                        {
                            dLeaveTime = entLeaveRecord.LEAVEHOURS.Value;
                        }

                        if (entLeaveRecord.TOTALHOURS == null)
                        {
                            dLeaveTotalTime = 0;
                        }
                        else
                        {
                            dLeaveTotalTime = entLeaveRecord.TOTALHOURS.Value;
                        }

                        flag = true;
                    }
                }

                if (flag)
                {
                    return strMsg;
                }
                #endregion
                DateTime dtStart, dtEnd = new DateTime();
                decimal dTotalLeaveDay = 0;                         //起止时间的时长

                DateTime.TryParse(dtLeaveStartTime.ToString("yyyy-MM-dd"), out dtStart);        //获取请假起始日期
                DateTime.TryParse(dtLeaveEndTime.ToString("yyyy-MM-dd"), out dtEnd);            //获取请假截止日期

                AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtStart);
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
                Utility.GetWorkDays(iWorkMode, ref iWorkDays);//获取每周上班天数

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                IQueryable<T_HR_OUTPLANDAYS> entOutPlanDays = bllOutPlanDays.GetOutPlanDaysRdListByEmployeeID(strEmployeeID);

                string strVacDayType = (Convert.ToInt32(Common.OutPlanDaysType.Vacation) + 1).ToString();
                string strWorkDayType = (Convert.ToInt32(Common.OutPlanDaysType.WorkDay) + 1).ToString();

                //节假日
                IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);

                //取出请假起始日年份和结束日年份的休假调剂工作天数
                DateTime startDate = Convert.ToDateTime(dtStart.Year + "-1-1");
                DateTime endDate = Convert.ToDateTime(dtEnd.Year + "-12-31");
                IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE >= startDate && s.ENDDATE <= endDate);

                if (LeaveTypeValue == "5" || LeaveTypeValue == "6")//5为产假，6为婚假，为产假和婚假时，是算自然日，不算上节假日
                {
                    entVacDays = entVacDays.Where(s => s.DAYTYPE == "0");
                    entWorkDays = entWorkDays.Where(s => s.DAYTYPE == "0");
                }

                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(entAttendSol.ATTENDANCESOLUTIONID);
                T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                TimeSpan ts = dtEnd.Subtract(dtStart);

                decimal dVacDay = 0, dWorkDay = 0;
                decimal dLeaveFirstDayTime = 0, dLeaveLastDayTime = 0, dLeaveFirstLastTime = 0;//请假第一天的时长，请假最后一天的时长，请假首尾两天合计时长
                if (dtLeaveStartTime != dtLeaveEndTime)
                {
                    CalculateNonWholeDayLeaveTime(dtLeaveStartTime, dtStart, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "S", ref dLeaveFirstDayTime, dtLeaveEndTime);
                    CalculateNonWholeDayLeaveTime(dtLeaveEndTime, dtEnd, entTemplateMaster, entTemplateDetails, entVacDays, entWorkDays, iWorkDays, "E", ref dLeaveLastDayTime, dtLeaveStartTime);

                    dLeaveFirstLastTime = dLeaveFirstDayTime + dLeaveLastDayTime;

                    if (dtStart == dtEnd)
                    {
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
                //公共假期中半天的情况下需要上班的分钟数
                decimal dayOndutyMintues = 0;
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
                        DateTime dtHalfStart = new DateTime();
                        DateTime.TryParse(dtCurDate.ToString("yyyy-MM-dd"), out dtHalfStart);
                        bool isHalfDay = false;//是否是半天
                        bool isWorkHalfDay = false;//工作周是否半天
                        bool isAfternoon = false;//是否下午
                        bool isWorkAfternoon = false;//工作周是否下午
                        if (entVacDays.Count() > 0)
                        {
                            //遍历节假日集合，取得节假日总天数
                            foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                            {
                                if (item_Vac.STARTDATE.Value <= dtCurDate && item_Vac.ENDDATE >= dtCurDate)
                                {
                                    if (item_Vac.ISHALFDAY != "1")
                                    {
                                        isVacDay = true;
                                    }
                                    else
                                    {
                                        isHalfDay = true;
                                        if (item_Vac.PEROID == "1")
                                        {
                                            isAfternoon = true;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        //如果是节假日,普通节假日总天数加1
                        if (isVacDay)
                        {
                            dVacDay += 1;
                        }
                        else//否则是周末，普通节假日总天数加1
                        {

                            if (isHalfDay && isAfternoon)
                            {
                                //下午半天休假，则上午是要计算的
                                //dVacDay += (decimal)0.47;
                                //dTotalLeaveDay -= 1;
                                //dLeaveFirstLastTime -= dWorkTimePerDay * 60 * (decimal)0.53;
                                dVacDay += 1;
                                CalculateLeaveTimeForafternoonOrMorning(isHalfDay, false, false, dtCurDate, dtHalfStart, entTemplateMaster, entTemplateDetails, "S", ref dayOndutyMintues);
                                //dayOndutyMintues += dWorkTimePerDay * 60 * (decimal)0.47;
                            }
                            if (isHalfDay && !isAfternoon)
                            {
                                //上午半天
                                //dVacDay += (decimal)0.53;
                                //dTotalLeaveDay -= 1;
                                //dLeaveFirstLastTime -= dWorkTimePerDay * 60 * (decimal)0.47;
                                dVacDay += 1;
                                CalculateLeaveTimeForafternoonOrMorning(isHalfDay, false, true, dtCurDate, dtHalfStart, entTemplateMaster, entTemplateDetails, "E", ref dayOndutyMintues);
                                //dayOndutyMintues += dWorkTimePerDay * 60 * (decimal)0.53;
                            }
                            if (iWorkDays.Contains(Convert.ToInt32(dtCurDate.DayOfWeek)) == false)
                            {
                                dVacDay += 1;
                            }
                        }

                        if (entWorkDays.Count() > 0)
                        {
                            //遍历休假调剂工作天数的集合，取得休假调剂工作天数
                            foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                            {
                                if (item_Work.STARTDATE.Value <= dtCurDate && item_Work.ENDDATE >= dtCurDate)
                                {
                                    //dWorkDay += 1;
                                    //break;
                                    if (item_Work.ISHALFDAY != "1")
                                    {
                                        dWorkDay += 1;
                                    }
                                    else
                                    {
                                        if (item_Work.PEROID == "1")
                                        {
                                            //dWorkDay += (decimal)0.53;
                                            //dLeaveFirstLastTime += dWorkTimePerDay * 60 * (decimal)0.53;
                                            CalculateLeaveTimeForafternoonOrMorning(false, true, true, dtCurDate, dtHalfStart, entTemplateMaster, entTemplateDetails, "E", ref dayOndutyMintues);
                                        }
                                        else
                                        {
                                            //dWorkDay += (decimal)0.47;
                                            //dLeaveFirstLastTime += dWorkTimePerDay * 60 * (decimal)0.47;
                                            CalculateLeaveTimeForafternoonOrMorning(false, true, false, dtCurDate, dtHalfStart, entTemplateMaster, entTemplateDetails, "S", ref dayOndutyMintues);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                if (LeaveTypeValue == "5" || LeaveTypeValue == "6")//5为产假，6为婚假，为产假和婚假时，是算自然日，应该加上双休和节假日
                {
                    dLeaveDay = dTotalLeaveDay;
                }
                else
                {
                    dLeaveDay = dTotalLeaveDay - dVacDay + dWorkDay;    //请假天数 = 请假天数-首尾两天 - 总休假天数 + 休假调剂工作天数
                }
                decimal dTempTime = decimal.Round((dLeaveFirstLastTime + dayOndutyMintues) / 60, 1);
                if (dTempTime >= dWorkTimePerDay)
                {
                    decimal dTempDay = decimal.Round(dTempTime / dWorkTimePerDay, 2);
                    string[] strList = dTempDay.ToString().Split('.');
                    if (strList.Length == 2)
                    {
                        dLeaveDay += decimal.Parse(strList[0].ToString());
                        dLeaveTime = dTempTime - dWorkTimePerDay * decimal.Parse(strList[0].ToString());
                    }
                    else
                    {
                        dLeaveDay += dTempDay;
                    }
                }
                else if (dTempTime < dWorkTimePerDay)
                {
                    dLeaveTime = dTempTime;
                }

                dLeaveTotalTime = dLeaveDay * dWorkTimePerDay + dLeaveTime;
                //如果不是同一天则对请假的时间进行处理
                //如果是7小时的则处理为1天（2个上午），如果是0.5小时的则舍掉(2个下午)
                string strVersion = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                if (strVersion == "false")
                {
                    if (dtStart.Date != dtEnd.Date)
                    {
                        if (dLeaveDay == 0 && dLeaveTime == 7)
                        {
                            dLeaveDay = 1;
                            dLeaveTime = 0;
                        }
                        if (dLeaveDay > 0 && dLeaveTime == (decimal)0.5)
                        {
                            dLeaveTotalTime -= (decimal)0.5;
                            dLeaveTime = 0;
                        }
                        if (dLeaveDay > 0 && dLeaveTime == 7)
                        {
                            dLeaveDay += 1;
                            dLeaveTime = 0;
                            dLeaveTotalTime += (decimal)0.5;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }

        public void CalculateLeaveTimeForafternoonOrMorning(bool isHalfDay, bool isWorkHalfDay, bool isAfternoon, DateTime dtRealDateTime, DateTime dtRealDate, T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster,
            IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails, string strDayFlag, ref decimal dLeaveDayTime)
        {
            //检查请假时间是否为节假日
            //bool isHalfDay = false;//是否是半天
            //bool isWorkHalfDay = false;//工作周是否半天
            //bool isAfternoon = false;//是否下午
            DateTime dtCurStartDate = DateTime.Parse(dtRealDate.ToString("yyyy-MM") + "-1");
            DateTime dtCurEndDate = DateTime.Parse(dtRealDate.ToString("yyyy-MM") + "-1").AddMonths(1).AddDays(-1);
            TimeSpan ts = dtCurEndDate.Subtract(dtCurStartDate);
            int iTotalDay = ts.Days;

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

            bool flag = false;
            for (int i = 0; i < iPeriod; i++)
            {
                for (int j = 0; j < iCircleDay; j++)
                {
                    int m = i * iCircleDay + j;
                    string strSchedulingDate = (j + 1).ToString();
                    DateTime dtCurDate = dtRealDate.AddDays(m);
                    T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == strSchedulingDate).FirstOrDefault();
                    T_HR_SHIFTDEFINE entShiftDefine = item.T_HR_SHIFTDEFINE;

                    if (entShiftDefine.FIRSTSTARTTIME != null && entShiftDefine.FIRSTENDTIME != null)
                    {
                        DateTime dtFirstStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTSTARTTIME).ToString("HH:mm"));
                        DateTime dtFirstEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTENDTIME).ToString("HH:mm"));
                        //早上
                        if (isHalfDay || isWorkHalfDay)
                        {
                            if (!isAfternoon)
                            {
                                TimeSpan tsFirst = dtFirstEnd.Subtract(dtFirstStart);
                                dLeaveDayTime = tsFirst.Hours * 60 + tsFirst.Minutes;
                                break;
                            }
                        }

                    }

                    if (entShiftDefine.SECONDSTARTTIME != null && entShiftDefine.SECONDENDTIME != null)
                    {
                        DateTime dtSecondStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDSTARTTIME).ToString("HH:mm"));
                        DateTime dtSecondEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDENDTIME).ToString("HH:mm"));
                        if (isAfternoon)
                        {
                            TimeSpan tsSecond = dtSecondEnd.Subtract(dtSecondStart);
                            dLeaveDayTime += tsSecond.Hours * 60 + tsSecond.Minutes;
                            break;
                        }
                    }
                    #region 不考虑有三、四段的情况

                    //if (entShiftDefine.THIRDSTARTTIME != null && entShiftDefine.THIRDENDTIME != null)
                    //{
                    //    DateTime dtThirdStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDSTARTTIME).ToString("HH:mm"));
                    //    DateTime dtThirdEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDENDTIME).ToString("HH:mm"));

                    //    if (strDayFlag == "S")
                    //    {

                    //        if (dtRealDateTime >= dtThirdStart)
                    //        {
                    //            TimeSpan tsThird = dtThirdEnd.Subtract(dtThirdStart);
                    //            dLeaveDayTime += tsThird.Hours * 60 + tsThird.Minutes;
                    //            //早上
                    //            if (isHalfDay && !isAfternoon)
                    //            {
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    else if (strDayFlag == "E")
                    //    {
                    //        if (dtRealDateTime <= dtThirdEnd)
                    //        {
                    //            TimeSpan tsThird = dtThirdEnd.Subtract(dtThirdStart);
                    //            dLeaveDayTime += tsThird.Hours * 60 + tsThird.Minutes;
                    //            if (isHalfDay && isAfternoon)
                    //            {
                    //                break;
                    //            }
                    //        }                                 
                    //    }

                    //}

                    //if (entShiftDefine.FOURTHSTARTTIME != null && entShiftDefine.FOURTHENDTIME != null)
                    //{
                    //    DateTime dtFourthStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHSTARTTIME).ToString("HH:mm"));
                    //    DateTime dtFourthEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHENDTIME).ToString("HH:mm"));

                    //    if (strDayFlag == "S")
                    //    {
                    //        if (dtFourthEnd <= dtRealDateTime)
                    //        {
                    //            dLeaveDayTime += 0;
                    //        }
                    //        else
                    //        {
                    //            if (dtFourthStart < dtRealDateTime)
                    //            {
                    //                TimeSpan tsFourth = dtFourthEnd.Subtract(dtRealDateTime);
                    //                dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                    //            }
                    //            else
                    //            {
                    //                TimeSpan tsFourth = dtFourthEnd.Subtract(dtFourthStart);
                    //                dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                    //            }
                    //        }
                    //    }
                    //    else if (strDayFlag == "E")
                    //    {
                    //        if (dtFourthEnd <= dtRealDateTime)
                    //        {
                    //            TimeSpan tsFourth = dtFourthEnd.Subtract(dtFourthStart);
                    //            dLeaveDayTime = tsFourth.Hours * 60 + tsFourth.Minutes;
                    //        }
                    //        else
                    //        {
                    //            if (dtFourthStart < dtRealDateTime)
                    //            {
                    //                TimeSpan tsFourth = dtRealDateTime.Subtract(dtFourthStart);
                    //                dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                    //            }
                    //            else
                    //            {
                    //                dLeaveDayTime += 0;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //}
                    #endregion
                    if (dLeaveDayTime > 0)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    break;
                }
            }
        }

        #endregion

        #region 操作
        ///// <summary>
        ///// 添加请假记录和请假调休记录
        ///// </summary>
        ///// <param name="LeaveRecord">请假记录实体</param>
        ///// <param name="AdjustLeave">请假调休记录实体</param>
        //public void EmployeeLeaveRecordAdd(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        //{
        //    try
        //    {
        //        //添加请假记录
        //        T_HR_EMPLOYEELEAVERECORD ent = new T_HR_EMPLOYEELEAVERECORD();
        //        Utility.CloneEntity(LeaveRecord, ent);
        //        ent.T_HR_LEAVETYPESETReference.EntityKey =
        //            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);

        //        //添加请假调休记录
        //        if (AdjustLeaves != null)
        //        {
        //            foreach (V_ADJUSTLEAVE item in AdjustLeaves)
        //            {
        //                T_HR_ADJUSTLEAVE entity = new T_HR_ADJUSTLEAVE();
        //                Utility.CloneEntity(item.T_HR_ADJUSTLEAVE, entity);
        //                //entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
        //                //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
        //                //DataContext.AddObject("T_HR_ADJUSTLEAVE", entity);
        //                ent.T_HR_ADJUSTLEAVE = new System.Data.Objects.DataClasses.EntityCollection<T_HR_ADJUSTLEAVE>();
        //                ent.T_HR_ADJUSTLEAVE.Add(entity);
        //            }
        //        }

        //        base.Add(ent);
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.SaveLog(ex.ToString());
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// 添加请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord">请假记录实体</param>
        /// <param name="AdjustLeave">请假调休记录实体</param>
        public string EmployeeLeaveRecordAdd(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            string strReturn = string.Empty;
            try
            {

                //添加请假记录
                T_HR_EMPLOYEELEAVERECORD ent = new T_HR_EMPLOYEELEAVERECORD();
                Utility.CloneEntity(LeaveRecord, ent);
                #region 结束日期在返岗后1个工作日完成
                string strVersion = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                if (strVersion == "false")
                {
                    if (LeaveRecord.ENDDATETIME < DateTime.Now || LeaveRecord.STARTDATETIME < DateTime.Now)
                    {
                        if (LeaveRecord.LEAVEDAYS > 5 || (LeaveRecord.LEAVEDAYS == 5 && LeaveRecord.LEAVEHOURS > 0))
                        {
                            strReturn = "事后的请假单最长不超过5天";
                            return strReturn;
                        }
                        if (LeaveRecord.ENDDATETIME < DateTime.Now)
                        {
                            double days = CheckLeaveEndDate(LeaveRecord);
                            if (LeaveRecord.ENDDATETIME.Value.AddDays(days) < DateTime.Now)
                            {
                                strReturn = "事后的请假单必须在结束日期后1工作日内提单";
                                return strReturn;
                            }
                        }
                    }
                }
                #endregion
                //注释掉，需求有变                
                #region 五四青年节
                try
                {
                    string strBirthDay = string.Empty;
                    EmployeeBLL employee = new EmployeeBLL();
                    T_HR_EMPLOYEE employeeInfo = employee.GetEmployeeByID(LeaveRecord.EMPLOYEEID);
                    if (employeeInfo == null)
                    {
                        strReturn = "获取员工信息为空";
                        return strReturn;
                    }
                    if (employeeInfo.BIRTHDAY == null)
                    {
                        strReturn = "没有获取到生日信息";
                        return strReturn;
                    }
                    strBirthDay = employeeInfo.BIRTHDAY.ToString();
                    //五四青年节
                    if (LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPEVALUE == "12")
                    {
                        if (string.IsNullOrEmpty(strBirthDay))
                        {
                            strReturn = "没有获取到生日信息";
                            return strReturn;
                        }
                        DateTime dtBirthday = new DateTime();
                        DateTime dtYouth = new DateTime();
                        DateTime.TryParse(strBirthDay, out dtBirthday);
                        DateTime.TryParse(DateTime.Now.Year.ToString() + "-05-04", out dtYouth);
                        if (dtBirthday.AddYears(28) < dtYouth)
                        {
                            strReturn = "已超过五四假的设置条件，不能保存此假";
                            return strReturn;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug("修改请假记录出现错误：" + ex.ToString());
                }

                #endregion
                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);

                //添加请假调休记录
                if (AdjustLeaves != null)
                {
                    foreach (V_ADJUSTLEAVE item in AdjustLeaves)
                    {
                        T_HR_ADJUSTLEAVE entity = new T_HR_ADJUSTLEAVE();
                        Utility.CloneEntity(item.T_HR_ADJUSTLEAVE, entity);
                        //entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                        //    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                        //DataContext.AddObject("T_HR_ADJUSTLEAVE", entity);
                        ent.T_HR_ADJUSTLEAVE = new System.Data.Objects.DataClasses.EntityCollection<T_HR_ADJUSTLEAVE>();
                        ent.T_HR_ADJUSTLEAVE.Add(entity);
                    }
                }

                bool blSave = base.Add(ent);
                if (!blSave)
                {
                    strReturn = "保存失败";
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                throw ex;
            }
            return strReturn;
        }


        /// <summary>
        /// 周文斌添加，用于MVC版
        /// 添加请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord">请假记录实体</param>
        /// <param name="AdjustLeave">请假调休记录实体</param>
        public string EmployeeLeaveRecordAdd_Grady_MVC(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            string strReturn = string.Empty;
            string preCheckState = LeaveRecord.CHECKSTATE;
            try
            {
                #region "   周文斌添加，再次验证数据   "
                //再次验证数据
                string strLeaveTypeSetID = LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID;
                LeaveTypeSetBLL _leaveTypeSetBll = new LeaveTypeSetBLL();
                T_HR_LEAVETYPESET leaveTypeSetEntity = _leaveTypeSetBll.GetLeaveTypeSetByID(strLeaveTypeSetID);

                CaculateLeaveHoursRequest request = new CaculateLeaveHoursRequest();
                request.LeaveTypeID = strLeaveTypeSetID;
                request.LeaveTypeValue = leaveTypeSetEntity != null ? Convert.ToInt32(leaveTypeSetEntity.LEAVETYPEVALUE) : 0;
                request.StartDate = LeaveRecord.STARTDATETIME.HasValue ? LeaveRecord.STARTDATETIME.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
                request.EndDate = LeaveRecord.ENDDATETIME.HasValue ? LeaveRecord.ENDDATETIME.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
                request.EmployeeID = LeaveRecord.EMPLOYEEID;
                request.LeaveRecordID = LeaveRecord.LEAVERECORDID;
                request.StartTime = LeaveRecord.STARTDATETIME.HasValue ? LeaveRecord.STARTDATETIME.Value.ToString("HH:mm:ss") : "0:00:00";
                request.EndTime = LeaveRecord.ENDDATETIME.HasValue ? LeaveRecord.ENDDATETIME.Value.ToString("HH:mm:ss") : "0:00:00";
                request.IsSave = true;
                CaculateLeaveHoursResponse response = CaculateLeaveHours(request);
                //有错误出现
                if (response != null && response.Message != string.Empty && response.Result != Enums.Result.Success.GetHashCode())
                {
                    return response.Message;
                }

                LeaveRecord.STARTDATETIME = Convert.ToDateTime(LeaveRecord.STARTDATETIME.Value.ToString("yyyy-MM-dd") + " " + response.StartTime);
                LeaveRecord.ENDDATETIME = Convert.ToDateTime(LeaveRecord.ENDDATETIME.Value.ToString("yyyy-MM-dd") + " " + response.EndTime);
                LeaveRecord.FINEDAYS = Convert.ToDecimal(response.FineDays);
                LeaveRecord.FINEHOURS = Convert.ToDecimal(response.FineHours);
                LeaveRecord.LEAVEDAYS = Convert.ToDecimal(response.LeaveDays);

                LeaveRecord.LEAVEHOURS = Convert.ToDecimal(response.LeaveHours);
                LeaveRecord.TOTALHOURS = Convert.ToDecimal(response.LeaveTotalHours);
                LeaveRecord.USEABLEDAYS = Convert.ToDecimal(response.LeftDays);
                LeaveRecord.USEABLEHOURS = Convert.ToDecimal(response.LeftHours);

                #endregion

                //添加请假记录
                T_HR_EMPLOYEELEAVERECORD ent = new T_HR_EMPLOYEELEAVERECORD();
                Utility.CloneEntity(LeaveRecord, ent);

                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);

                //添加请假调休记录
                if (AdjustLeaves != null)
                {
                    foreach (V_ADJUSTLEAVE item in AdjustLeaves)
                    {
                        T_HR_ADJUSTLEAVE entity = new T_HR_ADJUSTLEAVE();
                        Utility.CloneEntity(item.T_HR_ADJUSTLEAVE, entity);

                        ent.T_HR_ADJUSTLEAVE = new System.Data.Objects.DataClasses.EntityCollection<T_HR_ADJUSTLEAVE>();
                        ent.T_HR_ADJUSTLEAVE.Add(entity);
                    }
                }

                bool blSave = base.Add(ent);
                if (!blSave)
                {
                    strReturn = Constants.SaveFail;
                }
                else
                {
                    ////直接提交

                    #region
                    //获取在有效状态的假期
                    LeaveReferOTBLL _referOtBll = new LeaveReferOTBLL();
                    EmployeeLevelDayCountBLL daycountBll = new EmployeeLevelDayCountBLL();
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountList = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                    var daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE && t.STATUS == 1);

                    if (response.leaveReferOT != null)
                    {
                        foreach (T_HR_LEAVEREFEROT refotEntity in response.leaveReferOT)
                        {
                            //T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                            ////如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中
                            //if (ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE == Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString())
                            //{
                            //    daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                            //                                            && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                            //                                            && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                            //    if (daycountEntity != null)
                            //    {
                            //        daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                            //        if (daycountEntity.LEFTHOURS <= 0)
                            //        {
                            //            daycountEntity.STATUS = 1;
                            //        }
                            //        daycountBll.Update(daycountEntity);
                            //    }

                            //    T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                            //    if (otEntity != null)
                            //    {
                            //        otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                            //        if (otEntity.LEFTHOURS <= 0)
                            //        {
                            //            otEntity.STATUS = 3;//调休完
                            //        }
                            //        dal.Update(otEntity);
                            //    }

                            //}
                            //else
                            //{
                            //    daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                            //                                                && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                            //    if (daycountEntity != null)
                            //    {
                            //        daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                            //        if (daycountEntity.LEFTHOURS <= 0)
                            //        {
                            //            daycountEntity.STATUS = 0;
                            //        }
                            //        daycountBll.Update(daycountEntity);
                            //    }
                            //}

                            //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                            refotEntity.STATUS = 0;
                            _referOtBll.Add(refotEntity);
                        }
                    #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                throw ex;
            }
            return strReturn;
        }

        /// <summary>
        /// 判断请假时间是否是事后提单
        /// </summary>
        /// <param name="leave"></param>
        /// <returns></returns>
        private double CheckLeaveEndDate(T_HR_EMPLOYEELEAVERECORD leave)
        {
            string strReturn = string.Empty;
            //默认先添加1个工作日
            double dbFreeDay = 0;
            try
            {
                string strEmployeeID = leave.EMPLOYEEID;
                string LeaveTypeValue = string.Empty;//
                T_HR_LEAVETYPESET entLeaveTypeSet = new T_HR_LEAVETYPESET();
                LeaveTypeSetBLL bllLeaveTypeSet = new LeaveTypeSetBLL();
                if (leave.T_HR_LEAVETYPESET != null)
                {
                    entLeaveTypeSet = bllLeaveTypeSet.GetLeaveTypeSetByID(leave.T_HR_LEAVETYPESET.LEAVETYPESETID);
                }
                else
                {
                    return dbFreeDay;
                }
                if (entLeaveTypeSet != null)
                {
                    LeaveTypeValue = entLeaveTypeSet.LEAVETYPEVALUE;
                }

                DateTime dtStart, dtEnd = new DateTime(), dtMinuteEnd = new DateTime();
                decimal dTotalLeaveDay = 0;
                //请假的结束时间
                dtStart = (DateTime)leave.ENDDATETIME;
                dtEnd = dtStart.AddHours(24);
                dtMinuteEnd = dtStart.AddHours(24);
                DateTime.TryParse(dtStart.ToString("yyyy-MM-dd"), out dtStart);        //获取请假起始日期
                DateTime.TryParse(dtEnd.ToString("yyyy-MM-dd"), out dtEnd);            //获取请假截止日期

                AttendanceSolutionAsignBLL bllAttendSolAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttendSolAsign = bllAttendSolAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(strEmployeeID, dtStart);
                if (entAttendSolAsign == null)
                {
                    SMT.Foundation.Log.Tracer.Debug("没有获取到考勤方案");
                    return dbFreeDay;
                    //当前员工没有分配考勤方案，无法提交请假申请
                    //return "{NONEXISTASIGNEDATTENSOL}";
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

                //节假日
                IQueryable<T_HR_OUTPLANDAYS> entVacDays = entOutPlanDays.Where(s => s.DAYTYPE == strVacDayType);

                //取出请假起始日年份和结束日年份的休假调剂工作天数
                DateTime startDate = Convert.ToDateTime(dtStart.Year + "-1-1");
                DateTime endDate = Convert.ToDateTime(dtEnd.Year + "-12-31");
                IQueryable<T_HR_OUTPLANDAYS> entWorkDays = entOutPlanDays.Where(s => s.DAYTYPE == strWorkDayType && s.STARTDATE >= startDate && s.ENDDATE <= endDate);

                if (LeaveTypeValue == "5" || LeaveTypeValue == "6")//5为产假，6为婚假，为产假和婚假时，是算自然日，不算上节假日
                {
                    entVacDays = entVacDays.Where(s => s.DAYTYPE == "0");
                    entWorkDays = entWorkDays.Where(s => s.DAYTYPE == "0");
                }

                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(entAttendSol.ATTENDANCESOLUTIONID);
                T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster = entTemplateDetails.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                //TimeSpan ts = dtEnd.Subtract(dtStart);


                if (entVacDays.Count() > 0)
                {
                    foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                    {
                        if (item_Vac.STARTDATE.Value <= dtEnd && item_Vac.ENDDATE >= dtEnd)
                        {
                            //TimeSpan aa = (DateTime)item_Vac.ENDDATE.Value - (DateTime)item_Vac.STARTDATE.Value;
                            //休假天数
                            dbFreeDay += (double)item_Vac.DAYS;
                        }
                    }
                }
                bool isFirstWeek = false;
                bool isWeek = false;
                if (entWorkDays.Count() > 0)
                {
                    foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                    {
                        if (item_Work.STARTDATE.Value <= dtEnd && item_Work.ENDDATE >= dtEnd)
                        {
                            dbFreeDay -= (double)item_Work.DAYS;
                            //dbFreeDay += 1;//加回原来的1天
                            break;
                        }
                    }
                }

                double satDays = 0;

                if (iWorkDays.Contains(Convert.ToInt32(dtEnd.AddDays(dbFreeDay).DayOfWeek)) == false && (entVacDays.Any() || entWorkDays.Any()))
                {
                    if (entWorkDays.Count() > 0)
                    {
                        DateTime dt = dtEnd.AddDays(dbFreeDay);
                        var ents = from ent in entWorkDays
                                   where ent.STARTDATE <= dt
                                   && ent.ENDDATE >= dt
                                   select ent;
                        if (ents.Count() == 0)
                        {
                            dbFreeDay += 1;
                            isFirstWeek = true;
                        }
                        else
                        {
                            isFirstWeek = true;
                            satDays = 1;
                        }
                    }
                    else
                    {
                        dbFreeDay += 1;
                        isFirstWeek = true;
                    }
                }
                //else
                //{
                //    if (dtEnd.AddDays(dbFreeDay).DayOfWeek == DayOfWeek.Saturday)
                //    {
                //        isFirstWeek = true;
                //        satDays = 1;
                //    }
                //}


                //如果加1后是星期六则再判断是否是星期日
                if (isFirstWeek)
                {
                    if (iWorkDays.Contains(Convert.ToInt32(dtEnd.AddDays(dbFreeDay + satDays).DayOfWeek)) == false && (entVacDays.Any() || entWorkDays.Any()))
                    {
                        if (entWorkDays.Count() > 0)
                        {
                            DateTime dt = dtEnd.AddDays(dbFreeDay);
                            var ents = from ent in entWorkDays
                                       where ent.STARTDATE <= dt
                                       && ent.ENDDATE >= dt
                                       select ent;
                            if (ents.Count() == 0)
                            {
                                dbFreeDay += 1;
                            }
                        }
                        else
                        {
                            dbFreeDay += 1;
                        }
                        //return;
                    }
                }
                #region 考虑双休日后的情况
                DateTime dtEndWeek = new DateTime();
                dtEndWeek = dtEnd.AddDays(dbFreeDay);
                if (entVacDays.Count() > 0)
                {
                    foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                    {
                        if (item_Vac.STARTDATE.Value <= dtEndWeek && item_Vac.ENDDATE >= dtEndWeek)
                        {
                            //TimeSpan aa = (DateTime)item_Vac.ENDDATE.Value - (DateTime)item_Vac.STARTDATE.Value;
                            //休假天数
                            dbFreeDay += (double)item_Vac.DAYS;
                        }
                    }
                }

                if (entWorkDays.Count() > 0)
                {
                    foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                    {
                        if (item_Work.STARTDATE.Value <= dtEndWeek && item_Work.ENDDATE >= dtEndWeek)
                        {
                            dbFreeDay -= (double)item_Work.DAYS;
                            //dbFreeDay += 1;//加回原来的1天
                            break;
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
            if (dbFreeDay == 0)
            {
                dbFreeDay = 1;
            }
            else
            {
                dbFreeDay += 1;
            }
            return dbFreeDay;
        }

        ///// <summary>
        ///// 修改请假记录和请假调休记录
        ///// </summary>
        ///// <param name="LeaveRecord"></param>
        ///// <param name="AdjustLeave"></param>
        //public void EmployeeLeaveRecordUpdate(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        //{
        //    try
        //    {
        //        //修改请假记录
        //        var ent = dal.GetObjects().FirstOrDefault(s => s.LEAVERECORDID == LeaveRecord.LEAVERECORDID);
        //        if (ent == null)
        //        {
        //            return;
        //        }


        //        Utility.CloneEntity(LeaveRecord, ent);
        //        ent.T_HR_LEAVETYPESETReference.EntityKey =
        //            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);
        //        dal.UpdateFromContext(ent);
        //        dal.SaveContextChanges();
        //        SaveMyRecord(ent);

        //        if (AdjustLeaves != null)
        //        {
        //            foreach (var temp in AdjustLeaves)
        //            {
        //                var entity = dal.GetObjects<T_HR_ADJUSTLEAVE>().FirstOrDefault(s => s.ADJUSTLEAVEID == temp.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID);
        //                //如果找到就修改,反之就添加
        //                if (entity != null)
        //                {
        //                    Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
        //                    if (entity.T_HR_EMPLOYEELEAVERECORD != null)
        //                    {
        //                        entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
        //                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
        //                    }
        //                }
        //                else
        //                {
        //                    entity = new T_HR_ADJUSTLEAVE();
        //                    Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
        //                    entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
        //                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
        //                    //DataContext.AddObject("T_HR_ADJUSTLEAVE", entity);
        //                    dal.AddToContext(entity);
        //                }
        //            }
        //        }
        //        dal.SaveContextChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.SaveLog(ex.ToString());
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// 修改请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord"></param>
        /// <param name="AdjustLeave"></param>
        public string EmployeeLeaveRecordUpdate(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            string strReturn = string.Empty;
            try
            {
                //修改请假记录
                var ent = dal.GetObjects().FirstOrDefault(s => s.LEAVERECORDID == LeaveRecord.LEAVERECORDID);
                if (ent == null)
                {
                    strReturn = "请假记录不存在";
                    return strReturn;
                }
                #region 五四青年节
                try
                {
                    string strBirthDay = string.Empty;
                    EmployeeBLL employee = new EmployeeBLL();
                    T_HR_EMPLOYEE employeeInfo = employee.GetEmployeeByID(LeaveRecord.EMPLOYEEID);
                    if (employeeInfo == null)
                    {
                        strReturn = "获取员工信息为空";
                        return strReturn;
                    }
                    if (employeeInfo.BIRTHDAY == null)
                    {
                        strReturn = "没有获取到生日信息";
                        return strReturn;
                    }
                    strBirthDay = employeeInfo.BIRTHDAY.ToString();
                    //五四青年节
                    if (LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPEVALUE == "12")
                    {
                        if (string.IsNullOrEmpty(strBirthDay))
                        {
                            strReturn = "没有获取到生日信息";
                            return strReturn;
                        }
                        DateTime dtBirthday = new DateTime();
                        DateTime dtYouth = new DateTime();
                        DateTime.TryParse(strBirthDay, out dtBirthday);
                        DateTime.TryParse(DateTime.Now.Year.ToString() + "-05-04", out dtYouth);
                        if (dtBirthday.AddYears(28) < dtYouth)
                        {
                            strReturn = "已超过五四假的设置条件，不能保存此假";
                            return strReturn;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug("修改请假记录出现错误：" + ex.ToString());
                }

                #endregion
                #region 结束日期在返岗后1个工作日完成
                //ent.ATTACHMENTPATH 在终审通过后会被赋值为 3，否则是空的
                string strVersion = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                if (strVersion == "false")
                {
                    if (ent.ATTACHMENTPATH != "3")
                    {
                        if (LeaveRecord.ENDDATETIME < DateTime.Now || LeaveRecord.STARTDATETIME < DateTime.Now)
                        {
                            if (LeaveRecord.LEAVEDAYS > 5 || (LeaveRecord.LEAVEDAYS == 5 && LeaveRecord.LEAVEHOURS > 0))
                            {
                                strReturn = "事后的请假单最长不超过5天";
                                return strReturn;
                            }
                            if (LeaveRecord.ENDDATETIME < DateTime.Now)
                            {
                                double days = CheckLeaveEndDate(LeaveRecord);
                                if (LeaveRecord.ENDDATETIME.Value.AddDays(days) < DateTime.Now)
                                {
                                    strReturn = "事后的请假单必须在结束日期后1工作日内提单";
                                    return strReturn;
                                }
                            }
                        }
                    }
                }
                #endregion
                //注释掉，需求有变                
                //if (LeaveRecord.STARTDATETIME < DateTime.Now)
                //{
                //    strReturn = "请假开始时间不能小于当前时间";
                //    return strReturn;
                //}
                Utility.CloneEntity(LeaveRecord, ent);

                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);
                dal.UpdateFromContext(ent);
                dal.SaveContextChanges();
                SaveMyRecord(ent);

                if (AdjustLeaves != null)
                {
                    foreach (var temp in AdjustLeaves)
                    {
                        var entity = dal.GetObjects<T_HR_ADJUSTLEAVE>().FirstOrDefault(s => s.ADJUSTLEAVEID == temp.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID);
                        //如果找到就修改,反之就添加
                        if (entity != null)
                        {
                            Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
                            if (entity.T_HR_EMPLOYEELEAVERECORD != null)
                            {
                                entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                                     new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                            }
                        }
                        else
                        {
                            entity = new T_HR_ADJUSTLEAVE();
                            Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
                            entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                            //DataContext.AddObject("T_HR_ADJUSTLEAVE", entity);
                            dal.AddToContext(entity);
                        }
                    }
                }
                dal.SaveContextChanges();

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                throw ex;
            }
            return strReturn;
        }

        /// <summary>
        /// 周文斌添加，用于MVC版
        /// 修改请假记录和请假调休记录
        /// </summary>
        /// <param name="LeaveRecord"></param>
        /// <param name="AdjustLeave"></param>
        public string EmployeeLeaveRecordUpdate_Grady_MVC(T_HR_EMPLOYEELEAVERECORD LeaveRecord, List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            string strReturn = string.Empty;
            string beforeCheckState = LeaveRecord.CHECKSTATE;
            try
            {
                //修改请假记录
                var ent = dal.GetObjects().Include("T_HR_LEAVETYPESET").FirstOrDefault(s => s.LEAVERECORDID == LeaveRecord.LEAVERECORDID);
                if (ent == null)
                {
                    strReturn = Constants.NonLeaveRecord;
                    return strReturn;
                }

                #region "   周文斌添加，再次验证数据   "
                //再次验证数据
                string strLeaveTypeSetID = LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID;
                LeaveTypeSetBLL _leaveTypeSetBll = new LeaveTypeSetBLL();
                T_HR_LEAVETYPESET leaveTypeSetEntity = _leaveTypeSetBll.GetLeaveTypeSetByID(strLeaveTypeSetID);

                CaculateLeaveHoursRequest request = new CaculateLeaveHoursRequest();
                request.EmployeeID = ent.EMPLOYEEID;
                request.LeaveTypeID = strLeaveTypeSetID;
                request.LeaveTypeValue = leaveTypeSetEntity != null ? Convert.ToInt32(leaveTypeSetEntity.LEAVETYPEVALUE) : 0;
                request.StartDate = LeaveRecord.STARTDATETIME.HasValue ? LeaveRecord.STARTDATETIME.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
                request.EndDate = LeaveRecord.ENDDATETIME.HasValue ? LeaveRecord.ENDDATETIME.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
                request.EmployeeID = LeaveRecord.EMPLOYEEID;
                request.LeaveRecordID = LeaveRecord.LEAVERECORDID;
                request.StartTime = LeaveRecord.STARTDATETIME.HasValue ? LeaveRecord.STARTDATETIME.Value.ToString("HH:mm:ss") : "0:00:00";
                request.EndTime = LeaveRecord.ENDDATETIME.HasValue ? LeaveRecord.ENDDATETIME.Value.ToString("HH:mm:ss") : "0:00:00";
                request.IsSave = true;
                CaculateLeaveHoursResponse response = CaculateLeaveHours(request);
                //有错误出现
                if (response != null && response.Message != string.Empty && response.Result != Enums.Result.Success.GetHashCode())
                {
                    return response.Message;
                }

                LeaveRecord.EMPLOYEEID = ent.EMPLOYEEID;
                LeaveRecord.EMPLOYEENAME = ent.EMPLOYEENAME;
                LeaveRecord.UPDATEDATE = DateTime.Now;

                LeaveRecord.OWNERID = ent.EMPLOYEEID;
                LeaveRecord.OWNERPOSTID = ent.OWNERPOSTID;
                LeaveRecord.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
                LeaveRecord.OWNERCOMPANYID = ent.OWNERCOMPANYID;

                LeaveRecord.CREATEPOSTID = ent.CREATEPOSTID;
                LeaveRecord.CREATEDEPARTMENTID = ent.CREATEDEPARTMENTID;
                LeaveRecord.CREATECOMPANYID = ent.CREATECOMPANYID;
                LeaveRecord.CREATEUSERID = ent.CREATEUSERID;
                LeaveRecord.CREATEDATE = ent.CREATEDATE;

                LeaveRecord.STARTDATETIME = Convert.ToDateTime(LeaveRecord.STARTDATETIME.Value.ToString("yyyy-MM-dd") + " " + response.StartTime);
                LeaveRecord.ENDDATETIME = Convert.ToDateTime(LeaveRecord.ENDDATETIME.Value.ToString("yyyy-MM-dd") + " " + response.EndTime);

                LeaveRecord.FINEDAYS = Convert.ToDecimal(response.FineDays);
                LeaveRecord.FINEHOURS = Convert.ToDecimal(response.FineHours);
                LeaveRecord.LEAVEDAYS = Convert.ToDecimal(response.LeaveDays);

                LeaveRecord.LEAVEHOURS = Convert.ToDecimal(response.LeaveHours);
                LeaveRecord.TOTALHOURS = Convert.ToDecimal(response.LeaveTotalHours);
                LeaveRecord.USEABLEDAYS = Convert.ToDecimal(response.LeftDays);
                LeaveRecord.USEABLEHOURS = Convert.ToDecimal(response.LeftHours);

                #endregion

                Utility.CloneEntity(LeaveRecord, ent);

                ent.T_HR_LEAVETYPESETReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEAVETYPESET", "LEAVETYPESETID", LeaveRecord.T_HR_LEAVETYPESET.LEAVETYPESETID);
                dal.UpdateFromContext(ent);
                dal.SaveContextChanges();
                SaveMyRecord(ent);

                if (AdjustLeaves != null)
                {
                    foreach (var temp in AdjustLeaves)
                    {
                        var entity = dal.GetObjects<T_HR_ADJUSTLEAVE>().FirstOrDefault(s => s.ADJUSTLEAVEID == temp.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID);
                        //如果找到就修改,反之就添加
                        if (entity != null)
                        {
                            Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
                            if (entity.T_HR_EMPLOYEELEAVERECORD != null)
                            {
                                entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                                     new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                            }
                        }
                        else
                        {
                            entity = new T_HR_ADJUSTLEAVE();
                            Utility.CloneEntity(temp.T_HR_ADJUSTLEAVE, entity);
                            entity.T_HR_EMPLOYEELEAVERECORDReference.EntityKey =
                                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEELEAVERECORD", "LEAVERECORDID", temp.T_HR_ADJUSTLEAVE.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID);
                            //DataContext.AddObject("T_HR_ADJUSTLEAVE", entity);
                            dal.AddToContext(entity);
                        }
                    }
                }
                dal.SaveContextChanges();

                //添加请假和假期之间的关系
                //if (response.leaveReferOT != null)
                //{
                //    LeaveReferOTBLL _leaveRefOtBll = new LeaveReferOTBLL();
                //    _leaveRefOtBll.UpdateLeaveReferOvertime(response.leaveReferOT, LeaveRecord.LEAVERECORDID);
                //}


                #region
                //获取在有效状态的假期
                LeaveReferOTBLL _referOtBll = new LeaveReferOTBLL();
                EmployeeLevelDayCountBLL daycountBll = new EmployeeLevelDayCountBLL();
                List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountList = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                var daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE && t.STATUS == 1);

                List<T_HR_LEAVEREFEROT> existLeaveReferOT = dal.GetObjects<T_HR_LEAVEREFEROT>().Where(t => t.LEAVE_RECORDID == LeaveRecord.LEAVERECORDID).ToList();

                foreach (T_HR_LEAVEREFEROT referEntity in existLeaveReferOT)
                {
                    _referOtBll.Delete(referEntity);
                }


                if (response.leaveReferOT != null)
                {
                    foreach (T_HR_LEAVEREFEROT refotEntity in response.leaveReferOT)
                    {
                        //T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                        ////如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中
                        //if (ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE == Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString())
                        //{
                        //    daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                        //                                            && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                        //                                            && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                        //    if (daycountEntity != null)
                        //    {
                        //        daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                        //        if (daycountEntity.LEFTHOURS <= 0)
                        //        {
                        //            daycountEntity.STATUS = 1;
                        //        }
                        //        daycountBll.Update(daycountEntity);
                        //    }

                        //    T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                        //    if (otEntity != null)
                        //    {
                        //        otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                        //        if (otEntity.LEFTHOURS <= 0)
                        //        {
                        //            otEntity.STATUS = 3;//调休完
                        //        }
                        //        dal.Update(otEntity);
                        //    }

                        //}
                        //else
                        //{
                        //    daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                        //                                                && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                        //    if (daycountEntity != null)
                        //    {
                        //        daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                        //        if (daycountEntity.LEFTHOURS <= 0)
                        //        {
                        //            daycountEntity.STATUS = 0;
                        //        }
                        //        daycountBll.Update(daycountEntity);
                        //    }
                        //}

                        //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                        refotEntity.STATUS = 0;
                        _referOtBll.Add(refotEntity);
                    }

                #endregion
                }

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                throw ex;
            }
            return strReturn;
        }
        /// <summary>
        /// 删除请假记录组
        /// </summary>
        /// <param name="leaveRecordIDs">请假记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        public int EmployeeLeaveRecordDelete(string[] leaveRecordIDs)
        {
            try
            {
                foreach (var id in leaveRecordIDs)
                {
                    //先删除请假调休记录,再删除请假记录
                    var entity = from a in dal.GetObjects<T_HR_ADJUSTLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                                 where a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == id
                                 select a;
                    if (entity.Count() > 0)
                    {
                        foreach (var temp in entity)
                        {
                            var tempEnt = dal.GetObjects<T_HR_ADJUSTLEAVE>().FirstOrDefault(s => s.ADJUSTLEAVEID == temp.ADJUSTLEAVEID);
                            //DataContext.DeleteObject(tempEnt);
                            dal.DeleteFromContext(tempEnt);
                        }
                    }

                    var ent = dal.GetObjects().FirstOrDefault(s => s.LEAVERECORDID == id);
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
        /// 审核请假申请
        /// </summary>
        /// <param name="strLeaveRecordID">请假申请主键ID</param>
        /// <param name="AdjustLeaves">低假记录</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns>返回处理消息</returns>
        public string AuditLeaveRecord(string strLeaveRecordID, List<V_ADJUSTLEAVE> AdjustLeaves, string strCheckState)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strLeaveRecordID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{NOTFOUND}";
                }

                //修改请假记录
                T_HR_EMPLOYEELEAVERECORD ent = dal.GetObjects().FirstOrDefault(s => s.LEAVERECORDID == strLeaveRecordID);
                if (ent == null)
                {
                    return "{NOTFOUND}";
                }

                //已审核通过的记录禁止再次提交审核
                //if (ent.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                //{
                //    return "{REPEATAUDITERROR}";
                //}

                //审核状态变为审核通过时，生成对应的员工考勤记录(应用的员工范围，视应用对象而定)
                if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.Approved).ToString())
                {
                    if (ent.STARTDATETIME == null || ent.ENDDATETIME == null)
                    {
                        return "{REQUIREDFIELDS}";
                    }

                    ModifyAdjustLeavesByAudit(AdjustLeaves);


                    DateTime dtCheck = new DateTime();
                    DateTime dtStart = new DateTime();
                    DateTime dtEnd = new DateTime();

                    DateTime.TryParse(ent.STARTDATETIME.Value.ToString("yyyy-MM-dd"), out dtStart);
                    DateTime.TryParse(ent.ENDDATETIME.Value.ToString("yyyy-MM-dd"), out dtEnd);

                    if (dtStart <= dtCheck || dtEnd <= dtCheck)
                    {
                        return "{REQUIREDFIELDS}";
                    }

                    #region  启动处理考勤异常的线程

                    string attState = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("EMPLOYEEID", ent.EMPLOYEEID);
                    d.Add("STARTDATETIME", ent.STARTDATETIME.Value);
                    d.Add("ENDDATETIME", ent.ENDDATETIME.Value);
                    d.Add("ATTSTATE", attState);
                    Thread thread = new Thread(dealAttend);
                    thread.Start(d);

                    Tracer.Debug("请假启动消除异常的线程，出差开始时间:" + ent.STARTDATETIME.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            + " 结束时间：" + ent.ENDDATETIME.Value.ToString("yyyy-MM-dd HH:mm:ss") + "员工id：" + ent.EMPLOYEEID);

                    #endregion
                }
                #region 审核不通过
                if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.UnApproved).ToString())
                {
                    //如果是审核不通过那么 attachmentpath 设置为3，用来判断是否是审核不通过的数据。
                    ent.ATTACHMENTPATH = "3";
                }
                #endregion
                ent.CHECKSTATE = strCheckState;
                ent.UPDATEDATE = DateTime.Now;
                Update(ent);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }

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


        public void updateAllLeve()
        {
            DateTime dtStar = new DateTime(2013, 4, 1);
            DateTime dtend = new DateTime(2013, 5, 1);
            var q = from ent in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                    where ent.STARTDATETIME >= dtStar
                    && ent.ENDDATETIME <= dtend
                    && ent.CHECKSTATE == "2"
                    select ent;
            if (q.Count() > 0)
            {
                foreach (var item in q.ToList())
                {
                    try
                    {
                        AuditLeaveRecord_Grady_MVC(item.LEAVERECORDID, null, "2");
                        SMT.Foundation.Log.Tracer.Debug(item.EMPLOYEENAME + item.STARTDATETIME + item.ENDDATETIME + " 成功");
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug(item.EMPLOYEENAME + item.STARTDATETIME + item.ENDDATETIME + ex.ToString());
                        continue;
                    }
                }
            }
        }

        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                SMT.Foundation.Log.Tracer.Debug("GradyZhou Tracking:FormID:" + EntityKeyValue + " UpdateCheckState:跟踪请假申请业务审核情况，");
                int i = 0;
                string strMsg = string.Empty;
                strMsg = AuditLeaveRecord_Grady_MVC(EntityKeyValue, null, CheckState);
                if (strMsg == "{SAVESUCCESSED}")
                {
                    i = 1;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
        private void ModifyAdjustLeavesByAudit(List<V_ADJUSTLEAVE> AdjustLeaves)
        {
            try
            {
                if (AdjustLeaves == null)
                {
                    return;
                }

                if (AdjustLeaves.Count() == 0)
                {
                    return;
                }

                foreach (V_ADJUSTLEAVE item in AdjustLeaves)
                {
                    string strEmployeeID = item.T_HR_ADJUSTLEAVE.EMPLOYEEID;
                    string strVacType = item.VacationType;
                    DateTime currDate = DateTime.Now;

                    EmployeeLevelDayCountBLL bllLevelCount = new EmployeeLevelDayCountBLL();
                    T_HR_EMPLOYEELEVELDAYCOUNT ent = bllLevelCount.GetCurLevelDayCountByEmployeeID(strEmployeeID, strVacType, currDate);

                    if (ent == null)
                    {
                        continue;
                    }

                    decimal dDays = ent.DAYS.Value;
                    decimal dLeaveDays = item.T_HR_ADJUSTLEAVE.OFFSETDAYS.Value;
                    ent.DAYS = dDays - dLeaveDays;
                    bllLevelCount.ModifyLevelDayCount(ent);

                    T_HR_EMPLOYEELEVELDAYDETAILS entDetails = new T_HR_EMPLOYEELEVELDAYDETAILS();
                    entDetails.T_HR_EMPLOYEELEVELDAYCOUNT = ent;
                    entDetails.EMPLOYEEID = strEmployeeID;
                    entDetails.EMPLOYEENAME = item.T_HR_ADJUSTLEAVE.EMPLOYEENAME;
                    entDetails.EMPLOYEECODE = item.T_HR_ADJUSTLEAVE.EMPLOYEECODE;
                    entDetails.VACATIONTYPE = strVacType;
                    entDetails.DAYS = (0 - dLeaveDays);
                    entDetails.EFFICDATE = currDate;
                    entDetails.REMARK = string.Empty;
                    entDetails.OWNERPOSTID = item.T_HR_ADJUSTLEAVE.OWNERPOSTID;
                    entDetails.OWNERDEPARTMENTID = item.T_HR_ADJUSTLEAVE.OWNERDEPARTMENTID;
                    entDetails.OWNERCOMPANYID = item.T_HR_ADJUSTLEAVE.OWNERCOMPANYID;
                    entDetails.OWNERID = item.T_HR_ADJUSTLEAVE.OWNERID;
                    entDetails.CREATEPOSTID = item.T_HR_ADJUSTLEAVE.CREATEPOSTID;
                    entDetails.CREATEDEPARTMENTID = item.T_HR_ADJUSTLEAVE.CREATEDEPARTMENTID;
                    entDetails.CREATECOMPANYID = item.T_HR_ADJUSTLEAVE.CREATECOMPANYID;
                    entDetails.CREATEUSERID = item.T_HR_ADJUSTLEAVE.CREATEUSERID;
                    entDetails.CREATEDATE = item.T_HR_ADJUSTLEAVE.CREATEDATE;
                    entDetails.UPDATEUSERID = item.T_HR_ADJUSTLEAVE.UPDATEUSERID;
                    entDetails.UPDATEDATE = item.T_HR_ADJUSTLEAVE.UPDATEDATE;

                    //DataContext.AddObject("T_HR_EMPLOYEELEVELDAYDETAILS", entDetails);
                    dal.AddToContext(entDetails);
                }
                dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 四舍五入浮点数
        /// </summary>
        /// <param name="dValue">浮点数</param>
        /// <param name="strNumOfDec">小数位数值比较值</param>
        /// <param name="ilength"></param>
        /// <returns></returns>
        private decimal RoundOff(decimal dValue, string strNumOfDec, int ilength)
        {
            decimal dRes = 0;
            try
            {
                dRes = decimal.Round(dValue, ilength);

                if (!string.IsNullOrEmpty(strNumOfDec))
                {
                    decimal dNumOfDec = 0, dCheck = 0;
                    decimal.TryParse(strNumOfDec, out dNumOfDec);

                    string[] strlist = dRes.ToString().Split('.');
                    if (strlist.Length == 2)
                    {
                        decimal.TryParse("0." + strlist[1].ToString(), out dCheck);

                        if (dCheck > dNumOfDec)
                        {
                            dRes = decimal.Parse(strlist[0]) + 1;
                        }
                        else
                        {
                            dRes = decimal.Parse(strlist[0]) + dNumOfDec;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return dRes;
        }

        /// <summary>
        /// 获取非整天的请假时长(按天计)
        /// </summary>
        /// <param name="dtRealDateTime">请假日期时间</param>
        /// <param name="dtRealDate">请假日期</param>
        /// <param name="entTemplateMaster">作息方案</param>
        /// <param name="entTemplateDetails">作息方案明细</param>
        /// <param name="entVacDays">公休假</param>
        /// <param name="strDayFlag">请假日属于:"S",请假第一天；"E",请假最后一天</param>
        /// <param name="dLeaveDayTime">请假时长(按分钟计)</param>
        public void CalculateNonWholeDayLeaveTime(DateTime dtRealDateTime, DateTime dtRealDate, T_HR_SCHEDULINGTEMPLATEMASTER entTemplateMaster,
            IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemplateDetails, IQueryable<T_HR_OUTPLANDAYS> entVacDays, IQueryable<T_HR_OUTPLANDAYS> entWorkDays,
            List<int> iWorkDays, string strDayFlag, ref decimal dLeaveDayTime, DateTime realEndDate)
        {
            bool bCalculate = false;

            //检查请假时间是否为节假日
            bool isHalfDay = false;//是否是半天
            bool isWorkHalfDay = false;//工作周是否半天
            bool isAfternoon = false;//是否下午
            bool isWorkAfternoon = false;//工作周是否下午
            if (entVacDays.Count() > 0)
            {
                foreach (T_HR_OUTPLANDAYS item_Vac in entVacDays)
                {
                    if (item_Vac.STARTDATE.Value <= dtRealDate && item_Vac.ENDDATE >= dtRealDate)
                    {
                        if (item_Vac.ISHALFDAY == "1")
                        {
                            //dLeaveDayTime = (decimal)0.5;
                            isHalfDay = true;
                            if (item_Vac.PEROID == "1")
                            {
                                isAfternoon = true;
                            }
                        }
                        else
                        {
                            dLeaveDayTime = 0;
                            return;
                        }

                    }
                }
            }

            if (entWorkDays.Count() > 0)
            {
                foreach (T_HR_OUTPLANDAYS item_Work in entWorkDays)
                {
                    if (item_Work.STARTDATE.Value <= dtRealDate && item_Work.ENDDATE >= dtRealDate)
                    {
                        bCalculate = true;
                        if (item_Work.ISHALFDAY == "1")
                        {
                            //dLeaveDayTime = (decimal)0.5; 
                            isWorkHalfDay = true;
                            if (item_Work.PEROID == "1")
                            {
                                isWorkAfternoon = true;
                            }
                        }
                        break;
                    }
                }
            }

            if (!bCalculate && iWorkDays.Contains(Convert.ToInt32(dtRealDate.DayOfWeek)) == false && (entVacDays.Any() || entWorkDays.Any()))
            {
                dLeaveDayTime = 0;
                return;
            }

            DateTime dtCurStartDate = DateTime.Parse(dtRealDate.ToString("yyyy-MM") + "-1");
            DateTime dtCurEndDate = DateTime.Parse(dtRealDate.ToString("yyyy-MM") + "-1").AddMonths(1).AddDays(-1);
            TimeSpan ts = dtCurEndDate.Subtract(dtCurStartDate);
            int iTotalDay = ts.Days;

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

            bool flag = false;

            for (int i = 0; i < iPeriod; i++)
            {
                for (int j = 0; j < iCircleDay; j++)
                {
                    int m = i * iCircleDay + j;
                    string strSchedulingDate = (j + 1).ToString();
                    DateTime dtCurDate = dtRealDate.AddDays(m);
                    T_HR_SCHEDULINGTEMPLATEDETAIL item = entTemplateDetails.Where(c => c.SCHEDULINGDATE == strSchedulingDate).FirstOrDefault();
                    T_HR_SHIFTDEFINE entShiftDefine = item.T_HR_SHIFTDEFINE;

                    //检查是否为全天请假
                    if (dtRealDateTime == dtCurDate)
                    {
                        if (!isHalfDay && !isWorkHalfDay)
                        {
                            dLeaveDayTime = entShiftDefine.WORKTIME.Value * 60;
                            flag = true;
                            break;
                        }
                    }

                    //检查请假日期时间
                    if (strDayFlag == "S")
                    {
                        DateTime dtCheckStart = new DateTime();
                        dtCheckStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTSTARTTIME).ToString("HH:mm"));
                        if (dtRealDateTime <= dtCheckStart)
                        {
                            if (!isHalfDay && !isWorkHalfDay)
                            {
                                dLeaveDayTime = entShiftDefine.WORKTIME.Value * 60;
                                flag = true;
                                break;
                            }

                        }
                    }
                    else if (strDayFlag == "E")
                    {
                        DateTime dtCheck = new DateTime();
                        DateTime dtCheckEnd = new DateTime();
                        GetWorkEndDate(dtRealDateTime, entShiftDefine, ref dtCheckEnd);

                        if (dtCheckEnd != dtCheck)
                        {
                            if (dtRealDateTime >= dtCheckEnd)
                            {
                                if (!isHalfDay && !isWorkHalfDay)
                                {
                                    if (realEndDate.Date != dtRealDateTime.Date)
                                    {
                                        dLeaveDayTime = entShiftDefine.WORKTIME.Value * 60;
                                        flag = true;
                                        break;
                                    }
                                    else
                                    {
                                        //时间已经由开始时间进行了统计计算
                                        if (strDayFlag == "E")
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (entShiftDefine.FIRSTSTARTTIME != null && entShiftDefine.FIRSTENDTIME != null)
                    {
                        DateTime dtFirstStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTSTARTTIME).ToString("HH:mm"));
                        DateTime dtFirstEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTENDTIME).ToString("HH:mm"));

                        if (strDayFlag == "S")
                        {
                            if (dtFirstEnd <= dtRealDateTime)
                            {
                                if (dtRealDateTime.Date == realEndDate.Date)
                                {
                                    dLeaveDayTime = 0;
                                    if (isHalfDay && !isAfternoon)
                                    {
                                        if (dtFirstEnd >= dtRealDateTime)
                                        {
                                            break;
                                        }
                                    }
                                    //工作周下午上班
                                    if (isWorkHalfDay && isWorkAfternoon)
                                    {
                                        if (realEndDate <= dtFirstEnd)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dtFirstStart < dtRealDateTime)
                                {
                                    TimeSpan tsFirst = dtFirstEnd.Subtract(dtRealDateTime);
                                    dLeaveDayTime = tsFirst.Hours * 60 + tsFirst.Minutes;
                                    if (isHalfDay && !isAfternoon)
                                    {
                                        if (realEndDate.Date == dtRealDateTime.Date)
                                        {
                                            dLeaveDayTime = 0;
                                            if (dtRealDateTime > dtFirstEnd)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            dLeaveDayTime = 0;
                                            //break;
                                        }
                                    }
                                    if (isWorkHalfDay && isWorkAfternoon)
                                    {
                                        dLeaveDayTime = 0;
                                        //if (realEndDate.Date == dtRealDateTime.Date)
                                        //{
                                        //    break;
                                        //}
                                    }
                                }
                                else
                                {
                                    TimeSpan tsFirst = dtFirstEnd.Subtract(dtFirstStart);
                                    dLeaveDayTime = tsFirst.Hours * 60 + tsFirst.Minutes;
                                    if (isHalfDay && !isAfternoon)
                                    {
                                        dLeaveDayTime = 0;
                                        //if (dtRealDate == realEndDate)
                                        //{                                            
                                        //    break;
                                        //}
                                    }
                                    //工作周
                                    if (isWorkHalfDay && isWorkAfternoon)
                                    {
                                        dLeaveDayTime = 0;
                                    }
                                    //工作周上午上班
                                    if (isWorkHalfDay && !isWorkAfternoon)
                                    {
                                        if (realEndDate.Date == dtRealDateTime.Date)
                                        {
                                            if (realEndDate < dtFirstEnd)
                                            {
                                                //工作周的结束时间-工作周的开始时间
                                                TimeSpan tsFirstWork = realEndDate.Subtract(dtRealDateTime);
                                                dLeaveDayTime = tsFirstWork.Hours * 60 + tsFirstWork.Minutes;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (strDayFlag == "E")
                        {
                            if (dtFirstEnd <= dtRealDateTime)
                            {
                                TimeSpan tsFirst = dtFirstEnd.Subtract(dtFirstStart);
                                dLeaveDayTime = tsFirst.Hours * 60 + tsFirst.Minutes;
                                //公共假期半天休息或工作周上午上班
                                //if ((isHalfDay && isAfternoon) || (isWorkHalfDay && !isWorkAfternoon))
                                //{
                                //    dLeaveDayTime = 0;
                                //    //break;
                                //}
                                if (realEndDate <= dtFirstEnd)
                                {
                                    if ((isHalfDay && !isAfternoon) || (isWorkHalfDay && isWorkAfternoon))
                                    {
                                        if (isHalfDay && !isAfternoon)
                                        {
                                            if (dtFirstEnd >= dtRealDateTime)
                                            {
                                                dLeaveDayTime = 0;
                                                break;
                                            }
                                        }
                                        if (isWorkHalfDay)
                                        {
                                            if (isWorkAfternoon)
                                            {
                                                //下午上班则将上午的时间清空
                                                dLeaveDayTime = 0;
                                            }
                                            else
                                            {
                                                if (realEndDate.Date == dtRealDateTime.Date)
                                                {
                                                    dLeaveDayTime = 0;
                                                }
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                if (dtFirstStart < dtRealDateTime)
                                {
                                    TimeSpan tsFirst = dtRealDateTime.Subtract(dtFirstStart);
                                    dLeaveDayTime = tsFirst.Hours * 60 + tsFirst.Minutes;
                                }
                                else
                                {
                                    dLeaveDayTime = 0;
                                }
                            }
                        }
                    }

                    if (entShiftDefine.SECONDSTARTTIME != null && entShiftDefine.SECONDENDTIME != null)
                    {
                        DateTime dtSecondStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDSTARTTIME).ToString("HH:mm"));
                        DateTime dtSecondEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDENDTIME).ToString("HH:mm"));

                        if (strDayFlag == "S")
                        {
                            if (dtSecondEnd <= dtRealDateTime)
                            {
                                dLeaveDayTime += 0;
                            }
                            else
                            {
                                if ((isHalfDay && isAfternoon) || (isWorkHalfDay && !isWorkAfternoon))
                                {
                                    break;
                                }
                                if (dtSecondStart < dtRealDateTime)
                                {
                                    TimeSpan tsSecond = dtSecondEnd.Subtract(dtRealDateTime);
                                    dLeaveDayTime += tsSecond.Hours * 60 + tsSecond.Minutes;
                                }
                                else
                                {
                                    TimeSpan tsSecond = dtSecondEnd.Subtract(dtSecondStart);
                                    dLeaveDayTime += tsSecond.Hours * 60 + tsSecond.Minutes;
                                    if (isWorkHalfDay && isWorkAfternoon)
                                    {
                                        //结束时间为第一段时间
                                        if (realEndDate < dtSecondStart)
                                        {
                                            dLeaveDayTime = 0;
                                        }
                                    }
                                    if (isHalfDay && !isAfternoon)
                                    {
                                        //结束时间为第一段时间
                                        if (realEndDate < dtSecondStart)
                                        {
                                            dLeaveDayTime = 0;
                                        }
                                    }
                                }

                            }
                        }
                        else if (strDayFlag == "E")
                        {
                            if (dtSecondEnd <= dtRealDateTime)
                            {
                                if ((isHalfDay && isAfternoon) || (isWorkHalfDay && !isWorkAfternoon))
                                {
                                    if (dtRealDateTime.Date == realEndDate.Date)
                                    {
                                        dLeaveDayTime = 0;
                                    }
                                    break;
                                }
                                if (realEndDate.Date == dtRealDateTime.Date)
                                {
                                    if (realEndDate > dtSecondStart)
                                    {
                                        TimeSpan tsSecond = dtRealDateTime.Subtract(realEndDate);
                                        dLeaveDayTime = tsSecond.Hours * 60 + tsSecond.Minutes;
                                    }
                                    //前面已经计算了值，所以清空
                                    if (isHalfDay || isWorkHalfDay)
                                    {
                                        dLeaveDayTime = 0;
                                    }
                                }
                                else
                                {
                                    TimeSpan tsSecond = dtSecondEnd.Subtract(dtSecondStart);
                                    dLeaveDayTime = tsSecond.Hours * 60 + tsSecond.Minutes;
                                }
                            }
                            else
                            {
                                if (dtSecondStart < dtRealDateTime)
                                {
                                    //if (isHalfDay && !isAfternoon )
                                    //{
                                    //    break;
                                    //}
                                    if (isHalfDay)
                                    {
                                        if (isAfternoon)
                                        {
                                            dLeaveDayTime = 0;
                                        }
                                        if (realEndDate.Date == dtRealDateTime.Date)
                                        {
                                            if (dtRealDateTime < dtSecondEnd)
                                            {
                                                TimeSpan tsSecond1 = dtRealDateTime.Subtract(dtSecondEnd);
                                                dLeaveDayTime = tsSecond1.Hours * 60 + tsSecond1.Minutes;
                                            }
                                        }
                                        if (!isAfternoon)
                                        {
                                            //不是同一天的
                                            if (dtRealDateTime.Date != realEndDate.Date)
                                            {
                                                if (dtRealDateTime < dtSecondEnd)
                                                {
                                                    TimeSpan tsSecond1 = dtRealDateTime.Subtract(dtSecondStart);
                                                    dLeaveDayTime = tsSecond1.Hours * 60 + tsSecond1.Minutes;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    TimeSpan tsSecond = dtRealDateTime.Subtract(dtSecondStart);
                                    dLeaveDayTime += tsSecond.Hours * 60 + tsSecond.Minutes;
                                    if (realEndDate.Date == dtRealDateTime.Date)
                                    {
                                        if (dtRealDateTime < dtSecondEnd)
                                        {
                                            if (isWorkHalfDay && !isAfternoon)
                                            {
                                                dLeaveDayTime = 0;
                                            }
                                            else
                                            {
                                                TimeSpan tsSecond2 = dtRealDateTime.Subtract(dtSecondEnd);
                                                dLeaveDayTime = tsSecond2.Hours * 60 + tsSecond2.Minutes;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (isHalfDay && !isAfternoon)
                                    {
                                        dLeaveDayTime = 0;
                                    }
                                    else
                                    {
                                        if (realEndDate <= dtSecondStart)
                                        {
                                            if (isHalfDay && !isAfternoon)
                                            {
                                                dLeaveDayTime = 0;
                                            }
                                            if (isWorkHalfDay && !isWorkAfternoon)
                                            {
                                                if (realEndDate.Date == dtRealDateTime.Date)
                                                {
                                                    dLeaveDayTime = 0;
                                                }
                                            }
                                            if (realEndDate.Date == dtRealDateTime.Date)
                                            {
                                                if (isHalfDay && isAfternoon)
                                                {
                                                    dLeaveDayTime = 0;
                                                }
                                                if (isWorkHalfDay && isWorkAfternoon)
                                                {
                                                    //结束时间小于第2段的开始时间清空为0
                                                    if (dtRealDateTime < dtSecondStart)
                                                    {
                                                        dLeaveDayTime = 0;
                                                    }
                                                }
                                            }
                                            if (isWorkHalfDay && isWorkAfternoon)
                                            {
                                                if (dtRealDateTime < dtSecondStart)
                                                {
                                                    if (realEndDate.Date != dtRealDateTime.Date)
                                                    {
                                                        dLeaveDayTime = 0;
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            dLeaveDayTime += 0;
                                        }
                                    }
                                }
                            }
                            if ((isHalfDay && !isAfternoon) || (isWorkHalfDay && isWorkAfternoon))
                            {
                                break;
                            }
                        }
                    }

                    if (entShiftDefine.THIRDSTARTTIME != null && entShiftDefine.THIRDENDTIME != null)
                    {
                        DateTime dtThirdStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDSTARTTIME).ToString("HH:mm"));
                        DateTime dtThirdEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDENDTIME).ToString("HH:mm"));

                        if (strDayFlag == "S")
                        {
                            if (dtThirdEnd <= dtRealDateTime)
                            {
                                dLeaveDayTime += 0;
                            }
                            else
                            {
                                if (dtThirdStart < dtRealDateTime)
                                {
                                    TimeSpan tsThird = dtThirdEnd.Subtract(dtRealDateTime);
                                    dLeaveDayTime += tsThird.Hours * 60 + tsThird.Minutes;
                                }
                                else
                                {
                                    TimeSpan tsThird = dtThirdEnd.Subtract(dtThirdStart);
                                    dLeaveDayTime += tsThird.Hours * 60 + tsThird.Minutes;
                                }
                            }
                        }
                        else if (strDayFlag == "E")
                        {
                            if (dtThirdEnd <= dtRealDateTime)
                            {
                                TimeSpan tsThird = dtThirdEnd.Subtract(dtThirdStart);
                                dLeaveDayTime = tsThird.Hours * 60 + tsThird.Minutes;
                            }
                            else
                            {
                                if (dtThirdStart < dtRealDateTime)
                                {
                                    TimeSpan tsThird = dtRealDateTime.Subtract(dtThirdStart);
                                    dLeaveDayTime += tsThird.Hours * 60 + tsThird.Minutes;
                                }
                                else
                                {
                                    dLeaveDayTime += 0;
                                }
                            }
                        }
                    }

                    if (entShiftDefine.FOURTHSTARTTIME != null && entShiftDefine.FOURTHENDTIME != null)
                    {
                        DateTime dtFourthStart = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHSTARTTIME).ToString("HH:mm"));
                        DateTime dtFourthEnd = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHENDTIME).ToString("HH:mm"));

                        if (strDayFlag == "S")
                        {
                            if (dtFourthEnd <= dtRealDateTime)
                            {
                                dLeaveDayTime += 0;
                            }
                            else
                            {
                                if (dtFourthStart < dtRealDateTime)
                                {
                                    TimeSpan tsFourth = dtFourthEnd.Subtract(dtRealDateTime);
                                    dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                                }
                                else
                                {
                                    TimeSpan tsFourth = dtFourthEnd.Subtract(dtFourthStart);
                                    dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                                }
                            }
                        }
                        else if (strDayFlag == "E")
                        {
                            if (dtFourthEnd <= dtRealDateTime)
                            {
                                TimeSpan tsFourth = dtFourthEnd.Subtract(dtFourthStart);
                                dLeaveDayTime = tsFourth.Hours * 60 + tsFourth.Minutes;
                            }
                            else
                            {
                                if (dtFourthStart < dtRealDateTime)
                                {
                                    TimeSpan tsFourth = dtRealDateTime.Subtract(dtFourthStart);
                                    dLeaveDayTime += tsFourth.Hours * 60 + tsFourth.Minutes;
                                }
                                else
                                {
                                    dLeaveDayTime += 0;
                                }
                            }
                        }
                    }

                    if (dLeaveDayTime > 0)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 获取下班时间
        /// </summary>
        /// <param name="dtRealDateTime"></param>
        /// <param name="entShiftDefine"></param>
        /// <param name="dtRes"></param>
        private static void GetWorkEndDate(DateTime dtRealDateTime, T_HR_SHIFTDEFINE entShiftDefine, ref DateTime dtRes)
        {
            if (entShiftDefine.FOURTHENDTIME != null)
            {
                dtRes = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FOURTHENDTIME).ToString("HH:mm"));
                return;
            }

            if (entShiftDefine.THIRDENDTIME != null)
            {
                dtRes = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.THIRDENDTIME).ToString("HH:mm"));
                return;
            }

            if (entShiftDefine.SECONDENDTIME != null)
            {
                dtRes = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.SECONDENDTIME).ToString("HH:mm"));
                return;
            }

            if (entShiftDefine.FIRSTENDTIME != null)
            {
                dtRes = DateTime.Parse(dtRealDateTime.ToString("yyyy-MM-dd") + " " + DateTime.Parse(entShiftDefine.FIRSTENDTIME).ToString("HH:mm"));
                return;
            }
        }
        #endregion

        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEELEAVERECORD");

            IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = dal.GetObjects().Include("T_HR_LEAVETYPESET").Include("T_HR_EMPLOYEECANCELLEAVE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
            DateTime? date = bll.GetLastSalaryDateByEemployeeID(userID, "2");
            if (date != null)
            {
                ents = ents.Where(t => t.ENDDATETIME > date);
            }
            //var d = from a in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
            //        where a.EMPLOYEEID == userID && (a.CHECKSTATE == "0" || a.CHECKSTATE == "1" || a.CHECKSTATE == "2")
            //        select a.T_HR_EMPLOYEELEAVERECORD;
            //ents = ents.Except(d);

            var dv = from a in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                     where a.EMPLOYEEID == userID && (a.CHECKSTATE == "2" || a.CHECKSTATE == "1")
                     group a by a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID into g
                     select new
                     {
                         id = g.Key,
                         totalHours = g.Sum(p => p.TOTALHOURS)
                     };
            var ex = from e in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                     join c in dv on e.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals c.id
                     where e.T_HR_EMPLOYEELEAVERECORD.TOTALHOURS <= c.totalHours
                     select e.T_HR_EMPLOYEELEAVERECORD;
            ents = ents.Except(ex);
            ents = from ent in ents.ToList().AsQueryable()
                   orderby ent.STARTDATETIME descending
                   select ent;

            ents = Utility.Pager<T_HR_EMPLOYEELEAVERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion

        #region "   周文斌添加的方法    "

        /// <summary>
        /// 审核请假申请,周文斌，2014-07-29
        /// </summary>
        /// <param name="strLeaveRecordID">请假申请主键ID</param>
        /// <param name="AdjustLeaves">低假记录</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns>返回处理消息</returns>
        public string AuditLeaveRecord_Grady_MVC(string strLeaveRecordID, List<V_ADJUSTLEAVE> AdjustLeaves, string strCheckState)
        {
            string strAdjustLeaveLeaveTypeValue = Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString();
            string strAnnualLeaveLeaveTypeValue = Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString();
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strLeaveRecordID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{NOTFOUND}";
                }

                //修改请假记录
                T_HR_EMPLOYEELEAVERECORD ent = dal.GetObjects().Include("T_HR_LEAVETYPESET").FirstOrDefault(s => s.LEAVERECORDID == strLeaveRecordID);
                if (ent == null)
                {
                    return "{NOTFOUND}";
                }

                if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.Approving).ToString())
                {
                    Utility.SaveLog("Grady.zhou:提交请假申请开始，状态：" + strCheckState + ",请假记录ID：" + strLeaveRecordID);

                    #region "   周文斌,审核通过后去T_HR_EmployeeLeaveDayCount中减去假期时间长度   "

                    //获取在有效状态的假期
                    LeaveReferOTBLL _referOtBll = new LeaveReferOTBLL();
                    EmployeeLevelDayCountBLL daycountBll = new EmployeeLevelDayCountBLL();
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountList = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                    var daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE && t.STATUS == 1);

                    List<T_HR_LEAVEREFEROT> refOTList = new List<T_HR_LEAVEREFEROT>();
                    var refotQueryable = dal.GetObjects<T_HR_LEAVEREFEROT>().Where(t => t.LEAVE_RECORDID == ent.LEAVERECORDID && t.ACTION == 1 && t.STATUS == 0);

                    if (refotQueryable != null)
                    {
                        refOTList = refotQueryable.ToList();
                    }

                    if (daycountQueryable != null && daycountQueryable.Count() > 0)
                    {
                        Utility.SaveLog("Grady.zhou:提交事假请假申请开始，if (daycountQueryable != null) 状态：" + strCheckState + ",请假记录ID：" + strLeaveRecordID);
                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                        {
                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中
                            if (ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE == Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString())
                            {
                                daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                                                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                        && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                if (daycountEntity != null)
                                {
                                    daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    if (daycountEntity.LEFTHOURS <= 0)
                                    {
                                        daycountEntity.STATUS = 1;
                                    }
                                    daycountBll.Update(daycountEntity);
                                }

                                T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                if (otEntity != null)
                                {
                                    otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    if (otEntity.LEFTHOURS <= 0)
                                    {
                                        otEntity.STATUS = 3;//调休完
                                    }
                                    dal.Update(otEntity);
                                }

                            }
                            else
                            {
                                daycountEntity = daycountQueryable.Where(t => (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                            && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                if (daycountEntity != null)
                                {
                                    Tracer.Debug("提交请假申请，获取通过调休+年假+扣款方式的请假时，要返还的假期ID：" + refotEntity.OVERTIME_RECORDID + ",");
                                    daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    if (daycountEntity.LEFTHOURS <= 0)
                                    {
                                        daycountEntity.STATUS = 0;
                                    }
                                    daycountBll.Update(daycountEntity);
                                }
                                else
                                {
                                    Tracer.Debug("提交请假申请，获取通过调休+年假+扣款方式的请假时，要扣款的假期ID：" + refotEntity.OVERTIME_RECORDID + ",");
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID
                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID) && t.STATUS == 1);

                                    if (daycountQueryable != null && daycountQueryable.Count() > 0)
                                    {
                                        daycountEntity = daycountQueryable.Where(t => (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                        if (daycountEntity != null)
                                        {
                                            daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                            if (daycountEntity.LEFTHOURS <= 0)
                                            {
                                                daycountEntity.STATUS = 0;
                                            }
                                            daycountBll.Update(daycountEntity);
                                        }
                                    }
                                }



                                T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                if (otEntity != null)
                                {
                                    otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    if (otEntity.LEFTHOURS <= 0)
                                    {
                                        otEntity.STATUS = 3;//调休完
                                    }
                                    dal.Update(otEntity);
                                }

                            }

                            //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                            refotEntity.STATUS = 1;
                            _referOtBll.Update(refotEntity);
                        }
                    }
                    else
                    {
                        Utility.SaveLog("Grady.zhou:提交事假请假申请开始，if (daycountQueryable == null) 状态：" + strCheckState + ",请假记录ID：" + strLeaveRecordID);
                        #region "   性别限制，冲减和扣款类型，假期冲减  "

                        string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
                        DateTime dtCur = DateTime.Parse(ent.CREATEDATE.Value.ToString("yyyy-MM-dd"));
                        AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
                        T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(ent.EMPLOYEEID, dtCur);

                        //获取考勤方案关联的假期标准(只为带薪假的)
                        AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                        IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                        T_HR_ATTENDFREELEAVE freeLtsEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == ent.T_HR_LEAVETYPESET.LEAVETYPESETID).OrderByDescending(t => t.CREATEDATE).FirstOrDefault();
                        T_HR_LEAVETYPESET ltsEntity = new T_HR_LEAVETYPESET(); // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
                        List<T_HR_LEAVETYPESET> ltsList = leaveTypeSetBll.GetLeaveTypeSetAll(ent.EMPLOYEEID);

                        if (freeLtsEntity != null)
                        {
                            ltsEntity = freeLtsEntity.T_HR_LEAVETYPESET; // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        }
                        else
                        {
                            ltsEntity = ltsList.Where(t => t.LEAVETYPESETID == ent.T_HR_LEAVETYPESET.LEAVETYPESETID).FirstOrDefault();
                        }

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
                            switch (strFineType)
                            {                         // 扣款性质
                                // 1 不扣，2 扣款，3 调休+扣款，4 调休+带薪假+扣款
                                case "1":
                                case "2":
                                    break;
                                case "3":
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID
                                        && t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue && t.STATUS == 1);
                                    if (daycountQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中

                                            daycountEntity = daycountQueryable.Where(t => t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue
                                                                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                        && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                if (daycountEntity.LEFTHOURS <= 0)
                                                {
                                                    daycountEntity.STATUS = 1;
                                                }
                                                daycountBll.Update(daycountEntity);
                                            }

                                            T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                            if (otEntity != null)
                                            {
                                                otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                if (otEntity.LEFTHOURS <= 0)
                                                {
                                                    otEntity.STATUS = 3;//调休完
                                                }
                                                dal.Update(otEntity);
                                            }

                                            //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                                            refotEntity.STATUS = 1;
                                            _referOtBll.Update(refotEntity);
                                        }
                                    }
                                    break;
                                case "4":

                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue && t.STATUS == 1);
                                    var daycountAnnualQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == strAnnualLeaveLeaveTypeValue && t.STATUS == 1);

                                    if (daycountQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中

                                            daycountEntity = daycountQueryable.Where(t => t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue
                                                                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                        && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                if (daycountEntity.LEFTHOURS <= 0)
                                                {
                                                    daycountEntity.STATUS = 1;
                                                }
                                                daycountBll.Update(daycountEntity);
                                            }

                                            T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                            if (otEntity != null)
                                            {
                                                otEntity.LEFTHOURS = otEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                if (otEntity.LEFTHOURS <= 0)
                                                {
                                                    otEntity.STATUS = 3;//调休完
                                                }
                                                dal.Update(otEntity);
                                            }

                                            //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                                            refotEntity.STATUS = 1;
                                            _referOtBll.Update(refotEntity);
                                        }
                                    }

                                    if (daycountAnnualQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中

                                            daycountEntity = daycountAnnualQueryable.Where(t => t.RECORDID == refotEntity.OVERTIME_RECORDID
                                                                            && t.VACATIONTYPE == strAnnualLeaveLeaveTypeValue
                                                                           && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS - (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                if (daycountEntity.LEFTHOURS <= 0)
                                                {
                                                    daycountEntity.STATUS = 0;
                                                }
                                                daycountBll.Update(daycountEntity);
                                            }

                                            //审核通过后，将T_HR_LEAVEREFEROT表中的记录Status改为1
                                            refotEntity.STATUS = 1;
                                            _referOtBll.Update(refotEntity);
                                        }
                                    }
                                    break;
                            }
                        }
                        #endregion "   性别限制，冲减和扣款类型，假期冲减  "
                    }

                    #endregion
                    Utility.SaveLog("Grady.zhou:提交请假申请结束，状态：" + strCheckState + ",请假记录ID：" + strLeaveRecordID);
                }

                //审核状态变为审核通过时，生成对应的员工考勤记录(应用的员工范围，视应用对象而定)
                if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.Approved).ToString())
                {
                    #region " 审核通过  "
                    if (ent.STARTDATETIME == null || ent.ENDDATETIME == null)
                    {
                        return "{REQUIREDFIELDS}";
                    }

                    ModifyAdjustLeavesByAudit(AdjustLeaves);

                    DateTime dtCheck = new DateTime();
                    DateTime dtStart = new DateTime();
                    DateTime dtEnd = new DateTime();

                    DateTime.TryParse(ent.STARTDATETIME.Value.ToString("yyyy-MM-dd"), out dtStart);
                    DateTime.TryParse(ent.ENDDATETIME.Value.ToString("yyyy-MM-dd"), out dtEnd);

                    if (dtStart <= dtCheck || dtEnd <= dtCheck)
                    {
                        return "{REQUIREDFIELDS}";
                    }

                    #region  启动处理考勤异常的线程

                    string attState = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    Dictionary<string, object> d = new Dictionary<string, object>();
                    d.Add("EMPLOYEEID", ent.EMPLOYEEID);
                    d.Add("STARTDATETIME", ent.STARTDATETIME.Value);
                    d.Add("ENDDATETIME", ent.ENDDATETIME.Value);
                    d.Add("ATTSTATE", attState);
                    Thread thread = new Thread(dealAttend);
                    thread.Start(d);

                    Tracer.Debug("请假启动消除异常的线程，出差开始时间:" + ent.STARTDATETIME.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            + " 结束时间：" + ent.ENDDATETIME.Value.ToString("yyyy-MM-dd HH:mm:ss") + "员工id：" + ent.EMPLOYEEID);

                    #endregion

                    #endregion
                }
                #region 审核不通过
                if (strCheckState == Convert.ToInt32(SMT.HRM.DAL.CheckStates.UnApproved).ToString())
                {
                    //如果是审核不通过那么 attachmentpath 设置为3，用来判断是否是审核不通过的数据。
                    ent.ATTACHMENTPATH = "3";

                    //获取在有效状态的假期
                    LeaveReferOTBLL _referOtBll = new LeaveReferOTBLL();
                    EmployeeLevelDayCountBLL daycountBll = new EmployeeLevelDayCountBLL();
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountList = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                    var daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE);

                    List<T_HR_LEAVEREFEROT> refOTList = new List<T_HR_LEAVEREFEROT>();
                    var refotQueryable = dal.GetObjects<T_HR_LEAVEREFEROT>().Where(t => t.LEAVE_RECORDID == ent.LEAVERECORDID && t.ACTION == 1 && t.STATUS == 1);

                    if (refotQueryable != null)
                    {
                        refOTList = refotQueryable.ToList();
                    }

                    if (daycountQueryable != null && daycountQueryable.Count() > 0)
                    {
                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                        {
                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中
                            if (ent.T_HR_LEAVETYPESET.LEAVETYPEVALUE == Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString())
                            {
                                daycountEntity = daycountQueryable.Where(t => t.LEAVETYPESETID == refotEntity.LEAVE_TYPE_SETID
                                                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                        && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                if (daycountEntity != null)
                                {
                                    daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    daycountEntity.STATUS = 1;
                                    daycountBll.Update(daycountEntity);
                                }

                                T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                if (otEntity != null)
                                {
                                    otEntity.LEFTHOURS = otEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    otEntity.STATUS = 1;//调休完                                    
                                    dal.Update(otEntity);
                                }
                            }
                            else
                            {
                                daycountEntity = daycountQueryable.Where(t => (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                            && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();

                                if (daycountEntity != null)
                                {
                                    Tracer.Debug("审核不通过，获取通过调休+年假+扣款方式的请假时，要返还的假期ID：" + refotEntity.OVERTIME_RECORDID + ",");
                                    daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    daycountEntity.STATUS = 1;
                                    daycountBll.Update(daycountEntity);
                                }
                                else
                                {
                                    Tracer.Debug("审核不通过，获取通过调休+年假+扣款方式的请假时，要返还的假期ID：" + refotEntity.OVERTIME_RECORDID + ",");
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID
                                        && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID));

                                    if (daycountQueryable != null && daycountQueryable.Count() > 0)
                                    {
                                        daycountEntity = daycountQueryable.Where(t => (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                        if (daycountEntity != null)
                                        {
                                            daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                            daycountEntity.STATUS = 1;
                                            daycountBll.Update(daycountEntity);
                                        }
                                    }
                                }

                                T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                if (otEntity != null)
                                {
                                    otEntity.LEFTHOURS = otEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                    otEntity.STATUS = 1;//调休完                                    
                                    dal.Update(otEntity);
                                }
                            }

                            //审核不通过后，将T_HR_LEAVEREFEROT表中的记录删除
                            // refotEntity.STATUS = 0;
                            _referOtBll.Delete(refotEntity);
                        }
                    }
                    else
                    {
                        #region "   性别限制，冲减和扣款类型，假期冲减  "

                        string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
                        DateTime dtCur = DateTime.Parse(ent.CREATEDATE.Value.ToString("yyyy-MM-dd"));
                        AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
                        T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(ent.EMPLOYEEID, dtCur);

                        //获取考勤方案关联的假期标准(只为带薪假的)
                        AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                        IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                        T_HR_ATTENDFREELEAVE freeLtsEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == ent.T_HR_LEAVETYPESET.LEAVETYPESETID).OrderByDescending(t => t.CREATEDATE).FirstOrDefault();
                        T_HR_LEAVETYPESET ltsEntity = new T_HR_LEAVETYPESET(); // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
                        List<T_HR_LEAVETYPESET> ltsList = leaveTypeSetBll.GetLeaveTypeSetAll(ent.EMPLOYEEID);

                        if (freeLtsEntity != null)
                        {
                            ltsEntity = freeLtsEntity.T_HR_LEAVETYPESET; // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                        }
                        else
                        {
                            ltsEntity = ltsList.Where(t => t.LEAVETYPESETID == ent.T_HR_LEAVETYPESET.LEAVETYPESETID).FirstOrDefault();
                        }

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

                            switch (strFineType)
                            {                         // 扣款性质
                                // 1 不扣，2 扣款，3 调休+扣款，4 调休+带薪假+扣款
                                case "1":
                                case "2":
                                    break;
                                case "3":
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID
                                        && t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue);
                                    if (daycountQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();

                                            daycountEntity = daycountQueryable.Where(t => t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue
                                                && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                    && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                daycountEntity.STATUS = 1;
                                                daycountBll.Update(daycountEntity);
                                            }

                                            T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                            if (otEntity != null)
                                            {
                                                otEntity.LEFTHOURS = otEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                otEntity.STATUS = 1;//调休完                                    
                                                dal.Update(otEntity);
                                            }
                                            //审核不通过后，将T_HR_LEAVEREFEROT表中的记录删除
                                            // refotEntity.STATUS = 0;
                                            _referOtBll.Delete(refotEntity);
                                        }
                                    }
                                    break;
                                case "4":
                                    daycountQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue);
                                    var daycountAnnualQueryable = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(t => t.EMPLOYEEID == ent.EMPLOYEEID && t.VACATIONTYPE == strAnnualLeaveLeaveTypeValue);

                                    if (daycountQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();

                                            daycountEntity = daycountQueryable.Where(t => t.VACATIONTYPE == strAdjustLeaveLeaveTypeValue
                                                && (t.RECORDID == refotEntity.OVERTIME_RECORDID || t.REMARK == refotEntity.OVERTIME_RECORDID)
                                                                                    && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();
                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                daycountEntity.STATUS = 1;
                                                daycountBll.Update(daycountEntity);
                                            }

                                            T_HR_EMPLOYEEOVERTIMERECORD otEntity = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(t => t.OVERTIMERECORDID == refotEntity.OVERTIME_RECORDID).FirstOrDefault();
                                            if (otEntity != null)
                                            {
                                                otEntity.LEFTHOURS = otEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                otEntity.STATUS = 1;//调休完                                    
                                                dal.Update(otEntity);
                                            }
                                            //审核不通过后，将T_HR_LEAVEREFEROT表中的记录删除
                                            // refotEntity.STATUS = 0;
                                            _referOtBll.Delete(refotEntity);

                                        }
                                    }

                                    if (daycountAnnualQueryable != null)
                                    {
                                        foreach (T_HR_LEAVEREFEROT refotEntity in refOTList)
                                        {
                                            T_HR_EMPLOYEELEVELDAYCOUNT daycountEntity = new T_HR_EMPLOYEELEVELDAYCOUNT();
                                            //如果是调休假，则有对应的加班记录存在于T_HR_EMPLOYEELEVELDAYCOUNT表中

                                            daycountEntity = daycountAnnualQueryable.Where(t => t.RECORDID == refotEntity.OVERTIME_RECORDID
                                                                            && t.VACATIONTYPE == strAnnualLeaveLeaveTypeValue
                                                                           && t.EMPLOYEEID == refotEntity.EMPLOYEEID).FirstOrDefault();

                                            if (daycountEntity != null)
                                            {
                                                daycountEntity.LEFTHOURS = daycountEntity.LEFTHOURS + (refotEntity.LEAVE_TOTAL_HOURS.HasValue ? refotEntity.LEAVE_TOTAL_HOURS.Value : 0);
                                                daycountEntity.STATUS = 1;
                                                daycountBll.Update(daycountEntity);
                                            }
                                            //审核不通过后，将T_HR_LEAVEREFEROT表中的记录删除
                                            // refotEntity.STATUS = 0;
                                            _referOtBll.Delete(refotEntity);
                                        }
                                    }
                                    break;
                            }
                        }
                        #endregion "   性别限制，冲减和扣款类型，假期冲减  "
                    }

                }
                #endregion
                ent.CHECKSTATE = strCheckState;
                ent.UPDATEDATE = DateTime.Now;
                Update(ent);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return strMsg;
        }

        #region 定时触发接口实现
        /// <summary>
        /// 周文斌添加，审核通过后添加定时任务，方便诸如：加班，带薪假期过期后自动更新
        /// </summary>
        /// <param name="param"></param>
        public void EventTriggerProcess(string param)
        {
            try
            {
                Utility.SaveLog("Module:" + SaaS.Common.SMTAppModule.HR.ToString()
                                + " Function: EventTriggerProcess"
                                + " Business Logic:EmployeeLeaveRecordBLL"
                                + " Parameters：" + param);
                EventTriggerProcessHelper.processEvent(param);
            }
            catch (Exception ex)
            {
                Utility.SaveLog("Module:" + SaaS.Common.SMTAppModule.HR.ToString()
                    + " Function: EventTriggerProcess"
                    + " Business Logic:EmployeeLeaveRecordBLL"
                    + " Parameters：" + param
                    + " Function Owner:zhou wen bin"
                    + " LineNumber:3295"
                    + " ERROR:" + ex.Message
                    + " Source:" + ex.Source);
            }
        }
        #endregion

        /// <summary>
        /// 周文斌添加，用于MVC版审核组建元数据
        /// </summary>
        /// <param name="Formid"></param>
        /// <returns></returns>
        public string GetXmlString(string Formid)
        {
            T_HR_EMPLOYEELEAVERECORD Info = dal.GetObjects().Where(t => t.LEAVERECORDID == Formid).FirstOrDefault();
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
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

            //decimal? overTimeValue = Convert.ToDecimal(Info);
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "CURRENTEMPLOYEENAME", employee.T_HR_EMPLOYEE.EMPLOYEECNAME, employee.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "CHECKSTATE", Info.CHECKSTATE, checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "POSTLEVEL", employee.EMPLOYEEPOSTS[0].POSTLEVEL.ToString(), postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "EMPLOYEEORGNAME", employee.T_HR_EMPLOYEE.EMPLOYEECNAME, employee.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "EMPLOYEENAME", Info.EMPLOYEENAME, Info.EMPLOYEENAME));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, Info.OWNERCOMPANYID));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, Info.OWNERDEPARTMENTID));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERPOSTID", Info.OWNERPOSTID, Info.OWNERPOSTID));

            string StrSource = GetBusinessObject("T_HR_EMPLOYEELEAVERECORD");
            Tracer.Debug("获取的元数据模板为：" + StrSource);
            string employeeLeaveXML = mx.TableToXml(Info, null, StrSource, AutoList);
            Tracer.Debug("组合后的元数据为：" + employeeLeaveXML);
            return employeeLeaveXML;
        }


        /// <summary>
        /// 周文斌添加，用于MVC计算假期
        /// CaculateLeaveHours
        /// 计算请假时间
        /// 为了方便调用：
        /// 传入必要的条件判断
        /// 返回所有的信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CaculateLeaveHoursResponse CaculateLeaveHours(CaculateLeaveHoursRequest request)
        {
            CaculateLeaveHoursResponse response = new CaculateLeaveHoursResponse();
            try
            {
                EmployeeLevelDayCountBLL leaveDayCountBll = new EmployeeLevelDayCountBLL();
                #region "   基本数据设置  "
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

                double lefthours = 0;
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
                response.Message = string.Empty;
                response.LeaveDays = 0;
                response.LeaveHours = 0;
                response.Month = string.Empty;
                response.AttendSolution = string.Empty;
                response.WorkPerDay = 0;
                response.LeaveTotalHours = 0;
                response.LeftDays = 0;
                response.LeftHours = 0;
                response.FineDays = 0;
                response.FineHours = 0;
                response.MaxVacationDays = 0;
                response.MaxVacationHours = 0;
                //开始时间和结束时间，用于返回给前端
                //在销假的时候需要有准确的时分秒用于计算
                //否则会出错
                response.StartTime = request.StartTime;
                response.EndTime = request.EndTime;

                List<string> SatSundayList = new List<string>();
                #endregion

                #region "   取得员工信息公用  "

                EmployeeBLL empBll = new EmployeeBLL();
                T_HR_EMPLOYEE employee = empBll.GetEmployeeByID(request.EmployeeID);

                //if (employee.EMPLOYEESTATE == "0" || employee.EMPLOYEESTATE == "2")
                //{
                //    response.Result = Enums.Result.EmployeeNotNormal.GetHashCode();
                //    response.Message = Constants.EmployeeNotNormal;
                //    return response;
                //}
                #endregion

                #region "   判断是否有出差     "

                //EmployeeEvectionRecordBLL _evectionRecordBll = new EmployeeEvectionRecordBLL();
                //var evectionList = _evectionRecordBll.GetEmployeeEvectionRecordsByEmployeeIdAndDate(request.EmployeeID, dtEndDate);
                //if (evectionList != null && evectionList.Count() > 0)
                //{
                //    response.Result = Enums.Result.HasEvection.GetHashCode();
                //    response.Message = Constants.HasEvection;
                //    return response;
                //}

                #endregion

                #region "   判断是否有外出申请     "

                OutApplyBLL _outApplyBll = new OutApplyBLL();
                var outApplyList = _outApplyBll.GetOutApplyListByEmployeeIDAndDate(request.EmployeeID, dtStartDate, dtEndDate, "2");
                if (outApplyList != null && outApplyList.Count() > 0)
                {
                    response.Result = Enums.Result.HasOutApply.GetHashCode();
                    response.Message = Constants.HasOutApply;
                    return response;
                }

                #endregion

                #region "   判断请假申请是否重复 "
                //&& t.STARTDATETIME >= dtStartDate
                //&& t.ENDDATETIME <= dtEndDate
                //时间上有重叠:
                //1,开始时间落在已有时间范围内,
                //2结束时间落在已有范围内,
                //3,当dtStartDate与开始时间重叠时，ENDDATETIME需要>开始时间，
                //4,新增的请假完全包含已有请假
                //ENDDATETIME与结束时间重叠时，dtStartDate需要小于结束时间    
                //CHECKSTATE:0未提交,1,审核中,2审核通过,3未通过;仅判断1,2是否有时间上的重叠3
                List<T_HR_EMPLOYEELEAVERECORD> leaveEntityList = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Where(t => t.LEAVERECORDID != request.LeaveRecordID
                                                                         && t.EMPLOYEEID == request.EmployeeID).ToList();
                T_HR_EMPLOYEELEAVERECORD leaveEntity = leaveEntityList.Where(t => (
                                                                                (dtStartDate > t.STARTDATETIME && dtStartDate < t.ENDDATETIME)
                                                                                || (dtEndDate > t.STARTDATETIME && dtEndDate < t.ENDDATETIME)
                                                                                || (dtStartDate == t.STARTDATETIME && dtEndDate > t.STARTDATETIME)
                                                                                || (dtEndDate == t.ENDDATETIME && dtStartDate < t.ENDDATETIME)
                                                                                || (dtStartDate < t.STARTDATETIME && dtEndDate > t.ENDDATETIME))
                                                                                && t.CHECKSTATE != "3" && t.CHECKSTATE != "0"
                                                                               ).FirstOrDefault();
                if (leaveEntity != null)
                {
                    ////有的请假结束日期为：2014-08-08，没有时分秒
                    DateTime tempLeaveStartDate = leaveEntity.STARTDATETIME.HasValue ? leaveEntity.STARTDATETIME.Value : DateTime.MinValue;
                    DateTime tempLeaveEndDate = leaveEntity.ENDDATETIME.HasValue ? leaveEntity.ENDDATETIME.Value : DateTime.MinValue;
                    response.Result = Enums.Result.HasDuplicateRecord.GetHashCode();//结果：失败
                    response.Message = Constants.HasDuplicateRecord;//无论何种状态的请假记录，只要存在就提示重复
                    response.DuplicateStartDate = tempLeaveStartDate.ToString("yyyy-MM-dd HH:mm:ss");
                    response.DuplicateEndDate = tempLeaveEndDate.ToString("yyyy-MM-dd HH:mm:ss");
                    return response;
                    // }
                }
                #endregion

                #region "   性别限制，冲减和扣款类型，假期冲减  "

                string orgType = (Convert.ToInt32(Common.AssignedObjectType.Personnel) + 1).ToString();
                DateTime dtCur = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                AttendanceSolutionAsignBLL bllAttAsign = new AttendanceSolutionAsignBLL();
                T_HR_ATTENDANCESOLUTIONASIGN entAttAsign = bllAttAsign.GetAttendanceSolutionAsignByEmployeeIDAndDate(request.EmployeeID, dtCur);

                if (entAttAsign == null)
                {
                    response.Result = Enums.Result.NonAttendenceSolution.GetHashCode();
                    response.Message = Constants.NonAttendenceSolution;
                    return response;
                }

                //获取考勤方案关联的假期标准(只为带薪假的)
                AttendFreeLeaveBLL bllAttendFreeLeave = new AttendFreeLeaveBLL();
                IQueryable<T_HR_ATTENDFREELEAVE> entAttendFreeLeaves = bllAttendFreeLeave.GetAttendFreeLeaveByAttendSolID(entAttAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID);
                T_HR_ATTENDFREELEAVE freeLtsEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPESETID == request.LeaveTypeID).OrderByDescending(t => t.CREATEDATE).FirstOrDefault();
                T_HR_LEAVETYPESET ltsEntity = new T_HR_LEAVETYPESET(); // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                LeaveTypeSetBLL leaveTypeSetBll = new LeaveTypeSetBLL();
                List<T_HR_LEAVETYPESET> ltsList = leaveTypeSetBll.GetLeaveTypeSetAll(request.EmployeeID);

                if (freeLtsEntity != null)
                {
                    ltsEntity = freeLtsEntity.T_HR_LEAVETYPESET; // ltsList.Where(t => t.LEAVETYPEVALUE == strSickLeaveDayType).FirstOrDefault();
                }
                else
                {
                    ltsEntity = ltsList.Where(t => t.LEAVETYPESETID == request.LeaveTypeID).FirstOrDefault();
                }

                //T_HR_LEAVETYPESET ltsEntity = leaveTypeSetBll.GetLeaveTypeSetByID(request.LeaveTypeID);
                //FineType: 0,不扣(带薪假) 1、扣款；2、调休+扣款；3、调休+带薪假抵扣+扣款；
                string strFineType = string.Empty;//请假时，如果年假和调休有剩余时间就可以供其他假期抵扣
                double iMaxTakeVacationDays = 5;
                if (ltsEntity != null)
                {
                    //避免手机端无法提供LeaveTpeValue，因此重新赋值
                    request.LeaveTypeValue = string.IsNullOrEmpty(ltsEntity.LEAVETYPEVALUE) ? request.LeaveTypeValue : int.Parse(ltsEntity.LEAVETYPEVALUE);
                    iMaxTakeVacationDays = ltsEntity.MAXDAYS.HasValue ? Convert.ToDouble(ltsEntity.MAXDAYS.Value) : iMaxTakeVacationDays;
                    //假期之间如何冲抵，例如：请事假的时候根据该状态来指示，是否可用年假或调休假抵扣
                    //航信要求:调休+带薪假+扣款，即请事假时将调休+带薪假用完后再扣款;
                    strFineType = string.IsNullOrEmpty(ltsEntity.FINETYPE) ? "0" : ltsEntity.FINETYPE;
                    response.FineType = string.IsNullOrEmpty(ltsEntity.FINETYPE) ? "0" : ltsEntity.FINETYPE;

                    switch (strFineType)
                    {                         // 扣款性质
                        // 1 不扣，2 扣款，3 调休+扣款，4 调休+带薪假+扣款
                        case "1":
                            response.FineTypeMessage = "扣款方式：不扣";
                            break;
                        case "2":
                            response.FineTypeMessage = "扣款方式：扣款";
                            break;
                        case "3":
                            response.FineTypeMessage = "扣款方式：调休+扣款";
                            break;
                        case "4":
                            response.FineTypeMessage = "扣款方式：调休+带薪假+扣款";
                            break;
                        default:
                            response.FineTypeMessage = "扣款方式：不扣";
                            break;
                    }

                    response.SexRestrict = ltsEntity.SEXRESTRICT;
                    //性别限制：0女，1男，2不限.Sex:0女，1男
                    if (ltsEntity.SEXRESTRICT == "0" && employee.SEX == "1")
                    {
                        response.Result = Enums.Result.IsOnlyForFemale.GetHashCode();
                        response.Message = Constants.IsOnlyForFemale;
                        return response;
                    }

                    if (ltsEntity.SEXRESTRICT == "1" && employee.SEX == "0")
                    {
                        response.Result = Enums.Result.IsOnlyForMale.GetHashCode();
                        response.Message = Constants.IsOnlyForMale;
                        return response;
                    }

                    if (request.LeaveTypeValue != Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode())
                    {
                        //计算并声称没有年假的员工                    
                        leaveDayCountBll.GetCurLevelDaysByEmployeeIDAndLeaveFineTypeForMVC(request.EmployeeID, request.LeaveRecordID, request.LeaveTypeID, Convert.ToDateTime(request.StartDate), Convert.ToDateTime(request.EndDate));
                    }
                }

                #endregion

                //if (Convert.ToDateTime(request.StartDate) <= Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31"))
                //    && request.LeaveTypeValue == Enums.LeaveVacationType.AnnualDay.GetHashCode()
                //    && Convert.ToDateTime(request.EndDate) > Convert.ToDateTime(DateTime.Now.ToString("yyyy-12-31")))
                //{
                //    response.Result = Enums.Result.AnnualVacationNotAllowCross.GetHashCode();
                //    response.Message = Constants.AnnualVacationNotAllowCross;
                //    return response;
                //}

                #region "   获取请假时间段中所跨的月份,不同月份的每日工作小时数，用于计算平均值   "

                List<string> dateList = new List<string>();
                //存储每个月考勤方案的字典
                //2014-12-10->2015-02-18
                int startYear = dtStartDate.Year;
                int endYear = dtEndDate.Year;
                int endMonth = dtEndDate.Month;

                DateTime dtDateStartMonth = dtStartDate;
                bool flag = true;
                while (flag)
                {
                    string yearStartMonth = string.Empty;
                    string yearEndMonth = string.Empty;
                    yearStartMonth = dtDateStartMonth.ToString("yyyy-MM-01");
                    yearEndMonth = Convert.ToDateTime(dtDateStartMonth.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");

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

                //DateList的格式：List<2014-05-01|2014-05-31>
                AttendanceSolutionBLL attendSolutionBll = new AttendanceSolutionBLL();
                List<T_HR_ATTENDANCESOLUTION> attendSolutionList = attendSolutionBll.GetAttendenceSolutionByEmployeeIDAndStartDateAndEndDate(request.EmployeeID, dateList);

                //假期的相关设置  
                if (attendSolutionList.Count > 0)
                {
                    //计算跨多个月份时的平均工作时间，只用于参考，因为每个月的考勤不一样，上班时间长度也可能不一样，可能有的7.5小时/天
                    //有的8小时/天，在此取一个平均数,四舍五入计算
                    averageWorkPerDay = Math.Round(Convert.ToDouble(attendSolutionList.Sum(t => t.WORKTIMEPERDAY) / attendSolutionList.Count()), 2);
                    response.AvgWorkPerDay = averageWorkPerDay;
                }
                #endregion

                response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);

                #region "   判断该类的假期是否要求一次休完   "

                FreeLeaveDaySetBLL freeLeaveDaySetLogic = new FreeLeaveDaySetBLL();
                var fldsEntityList = freeLeaveDaySetLogic.GetFreeLeaveDaySetByLeaveTypeID(request.LeaveTypeID);

                if (ltsEntity.T_HR_FREELEAVEDAYSET != null)
                {
                    fldsEntityList = ltsEntity.T_HR_FREELEAVEDAYSET.AsQueryable<T_HR_FREELEAVEDAYSET>();
                }

                if (fldsEntityList != null && fldsEntityList.Count() > 0)
                {
                    decimal fldsMaxLeaveDays = fldsEntityList.Max(t => t.LEAVEDAYS).HasValue ? fldsEntityList.Max(t => t.LEAVEDAYS).Value : 0;
                    T_HR_FREELEAVEDAYSET fldsEntity = fldsEntityList.FirstOrDefault();
                    //1 一次休完；2 分N次休，冲减一次病/事假算一次，3 无限制，可用于冲减病/事假
                    //请假天数必须和假期天数一致
                    if (!string.IsNullOrEmpty(fldsEntity.OFFESTTYPE) && fldsEntity.OFFESTTYPE == "1")
                    {
                        if (request.LeaveTypeValue == Enums.LeaveVacationType.WomenDay.GetHashCode()
                            || request.LeaveTypeValue == Enums.LeaveVacationType.YouthLeaveDay.GetHashCode()
                            )
                        {
                            iMaxTakeVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);
                            response.Result = Enums.Result.IsOnceRestVacation.GetHashCode();

                        }
                        else
                        {
                            iMaxTakeVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);
                            response.Result = Enums.Result.IsOnceRestVacation.GetHashCode();
                            dtStartDate = Convert.ToDateTime(Convert.ToDateTime(request.StartDate + " " + request.StartTime).ToString("yyyy-MM-dd HH:mm"));
                            dtEndDate = Convert.ToDateTime(Convert.ToDateTime(request.StartDate + " " + request.EndTime).AddDays(-1).AddDays(response.MaxVacationDays).ToString("yyyy-MM-dd HH:mm"));
                        }
                    }
                }
                else
                {
                    decimal fldsMaxLeaveDays = ltsEntity.MAXDAYS.HasValue ? ltsEntity.MAXDAYS.Value : 0;
                    //1 一次休完；2 分N次休，冲减一次病/事假算一次，3 无限制，可用于冲减病/事假
                    //请假天数必须和假期天数一致
                    if (!string.IsNullOrEmpty(ltsEntity.OFFESTTYPE) && ltsEntity.OFFESTTYPE == "1")
                    {
                        if (request.LeaveTypeValue == Enums.LeaveVacationType.WomenDay.GetHashCode()
                            || request.LeaveTypeValue == Enums.LeaveVacationType.YouthLeaveDay.GetHashCode()
                            )
                        {
                            iMaxTakeVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);
                            response.Result = Enums.Result.IsOnceRestVacation.GetHashCode();
                        }
                        else
                        {
                            iMaxTakeVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationDays = (double)fldsMaxLeaveDays;
                            response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);
                            response.Result = Enums.Result.IsOnceRestVacation.GetHashCode();
                            dtStartDate = Convert.ToDateTime(Convert.ToDateTime(request.StartDate + " " + request.StartTime).ToString("yyyy-MM-dd HH:mm"));
                            dtEndDate = Convert.ToDateTime(Convert.ToDateTime(request.StartDate + " " + request.EndTime).AddDays(-1).AddDays(response.MaxVacationDays).ToString("yyyy-MM-dd HH:mm"));
                        }
                    }
                }
                #endregion

                #region "   由于起止时间发生了变化，需要重新计算考勤方案   "
                if (dtEndDate.ToString("yyyy-MM-dd") != request.EndDate)
                {
                    dateList = new List<string>();
                    //存储每个月考勤方案的字典
                    //2014-12-10->2015-02-18
                    startYear = dtStartDate.Year;
                    endYear = dtEndDate.Year;
                    endMonth = dtEndDate.Month;

                    dtDateStartMonth = dtStartDate;
                    flag = true;
                    while (flag)
                    {
                        string yearStartMonth = string.Empty;
                        string yearEndMonth = string.Empty;
                        yearStartMonth = dtDateStartMonth.ToString("yyyy-MM-01");
                        yearEndMonth = Convert.ToDateTime(dtDateStartMonth.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
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

                    //DateList的格式：List<2014-05-01|2014-05-31>
                    attendSolutionList = attendSolutionBll.GetAttendenceSolutionByEmployeeIDAndStartDateAndEndDate(request.EmployeeID, dateList);

                    //假期的相关设置  
                    if (attendSolutionList.Count > 0)
                    {
                        //计算跨多个月份时的平均工作时间，只用于参考，因为每个月的考勤不一样，上班时间长度也可能不一样，可能有的7.5小时/天
                        //有的8小时/天，在此取一个平均数,四舍五入计算
                        averageWorkPerDay = Math.Round(Convert.ToDouble(attendSolutionList.Sum(t => t.WORKTIMEPERDAY) / attendSolutionList.Count()), 2);
                        response.AvgWorkPerDay = averageWorkPerDay;
                    }
                    request.EndDate = dtEndDate.ToString("yyyy-MM-dd");
                    response.MaxVacationHours = Math.Round(iMaxTakeVacationDays * averageWorkPerDay, 1);
                }

                #endregion

                #region "   将起止时间日历化,将其变换为每日的时间段，例如：2014-08-01 0：00：00～2014-08-01 23：59：59    "

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
                        if (dtDateStartDetail > dtDateEndDetail)
                        {
                            dtDateEndDetail = dtDateStartDetail;
                        }
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

                #region "   获取考勤方案，排班明细，排班时间段   "
                AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL();
                Dictionary<string, decimal> dicYearVacation = new Dictionary<string, decimal>();
                Dictionary<string, decimal> dicYearFreeVacation = new Dictionary<string, decimal>();
                //KeyValuePair<string, decimal> kvYearVacation = new KeyValuePair<string, decimal>();
                int iCountVal = 0;//该计数器只用于判断请假开始第一天和结束最后一天
                //计算每天对应的请假时长：8：30-17：30，扣除假期，周末，扣除午间休息            
                foreach (string strDateString in dateDetailList)
                {
                    iCountVal++;
                    List<string> sWorkArr = new List<string>();//工作时间
                    List<string> sNotWorkArr = new List<string>();//休息时间
                    List<string> tempWorkArr = new List<string>();
                    List<string> tempHalfNotWorkArr = new List<string>();
                    double totalTempHours = 0;
                    double totalTempDays = 0;
                    DateTime dtTempStartDate = Convert.ToDateTime(strDateString.Split('|')[0]);
                    DateTime dtTempEndDate = Convert.ToDateTime(strDateString.Split('|')[1]);
                    DateTime dtTempEndDate1 = Convert.ToDateTime(strDateString.Split('|')[1]);
                    DateTime dtTempHalfNotWorkStartDate = DateTime.MinValue;
                    DateTime dtTempHalfNotWorkEndDate = DateTime.MinValue;
                    //获取这个月份的考勤方案                
                    T_HR_ATTENDANCESOLUTION LeavePeriodAttendSolution = bllAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(request.EmployeeID,
                                                                            Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd")), Convert.ToDateTime(dtTempEndDate.ToString("yyyy-MM-dd")));
                    SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                    IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> scheduleSetDetail = bllTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(LeavePeriodAttendSolution.ATTENDANCESOLUTIONID);
                    T_HR_SCHEDULINGTEMPLATEMASTER scheduleSetting = scheduleSetDetail.FirstOrDefault().T_HR_SCHEDULINGTEMPLATEMASTER;

                    int iCycleDays = 0;
                    DateTime dtCycleStartDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM") + "-1"));//按月为周期的排班表
                    DateTime dtCurCycleOTDate = Convert.ToDateTime(DateTime.Parse(dtTempStartDate.ToString("yyyy-MM-dd")));
                    //找出加班时间与循环排班详细中对应的日历号，然后通过T_HR_SCHEDULINGTEMPLATEDETAIL找到对应的 打卡时间段： T_HR_SHIFTDEFINE
                    if (scheduleSetting.SCHEDULINGCIRCLETYPE == (Common.SchedulingCircleType.Month.GetHashCode() + 1).ToString())
                    {//按月循环的排班打卡方式
                        iCycleDays = 31;
                    }

                    if ((scheduleSetting.SCHEDULINGCIRCLETYPE == (Common.SchedulingCircleType.Week.GetHashCode() + 1).ToString()))
                    {//按周排班打卡方式
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
                                FirstCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).ToString("HH:mm"));
                                FirstCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).ToString("HH:mm"));
                                notWorkTimeStart = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).ToString("HH:mm"));

                                sWorkArr.Add(FirstCardStartDate.ToString("HH:mm:ss") + "|" + FirstCardEndDate.ToString("HH:mm:ss"));
                                response.FirstStartTime = Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).ToString("HH:mm");
                                response.FirstEndTime = Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).ToString("HH:mm");
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
                                SecondCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).ToString("HH:mm"));
                                SecondCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));

                                notWorkTimeEnd = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).ToString("HH:mm"));
                                notWorkTimeStart1 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));

                                sWorkArr.Add(SecondCardStartDate.ToString("HH:mm:ss") + "|" + SecondCardEndDate.ToString("HH:mm:ss"));
                                sNotWorkArr.Add(notWorkTimeStart.ToString() + "|" + notWorkTimeEnd.ToString());

                                response.SecondStartTime = Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).ToString("HH:mm");
                                response.SecondEndTime = Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm");
                                //如果用户没有选择具体的结束小时数，那么就设置结束时间为作息的结束时间
                                if (dtTempEndDate1 >= SecondCardEndDate)
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
                                //考勤时间段
                                ThirdCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).ToString("HH:mm"));
                                ThirdCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDENDTIME).ToString("HH:mm"));
                                //休息时间段
                                notWorkTimeEnd1 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).ToString("HH:mm"));
                                notWorkTimeStart2 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDENDTIME).ToString("HH:mm"));

                                sWorkArr.Add(ThirdCardStartDate.ToString("HH:mm:ss") + "|" + ThirdCardEndDate.ToString("HH:mm:ss"));
                                sNotWorkArr.Add(notWorkTimeStart1.ToString() + "|" + notWorkTimeEnd1.ToString());
                                response.ThirdStartTime = Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).ToString("HH:mm");
                                response.ThirdEndTime = Convert.ToDateTime(dayCardSetting.THIRDENDTIME).ToString("HH:mm");
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
                                FourthCardStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).ToString("HH:mm"));
                                FourthCardEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).ToString("HH:mm"));
                                notWorkTimeEnd2 = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).ToString("HH:mm"));

                                sWorkArr.Add(FourthCardStartDate.ToString("HH:mm:ss") + "|" + FourthCardEndDate.ToString("HH:mm:ss"));
                                sNotWorkArr.Add(notWorkTimeStart2.ToString() + "|" + notWorkTimeEnd2.ToString());

                                response.FourthStartTime = Convert.ToDateTime(dayCardSetting.FOURTHSTARTTIME).ToString("HH:mm");
                                response.FourthEndTime = Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).ToString("HH:mm");
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
                                response.Message = Constants.NonFirstSetting + ",具体日期：" + dtTempStartDate.ToString("yyyy-MM-dd");
                                return response;
                            }

                            if (hasSecondSetting == 0)
                            {
                                response.Result = Enums.Result.NonSecondSetting.GetHashCode();
                                response.Message = Constants.NonSecondSetting + ",具体日期：" + dtTempStartDate.ToString("yyyy-MM-dd");
                                return response;
                            }
                            #endregion
                            //这类条件的假期按自然日计算请假时长
                            //为了避免用户要动态设置哪些假期按自然日休，哪些假期排除公休日，将其配置在service的config中
                            string SpecialTypeValue = Utility.GetAppConfigByName("NaturalVacationDay");
                            string strLeaveTypeValue = request.LeaveTypeValue.ToString();
                            if (request.LeaveTypeValue < 10)
                            {
                                strLeaveTypeValue = "0" + request.LeaveTypeValue.ToString();
                            }

                            if (!SpecialTypeValue.Contains(strLeaveTypeValue))
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
                                bool isDayCount = iWorkDays.Contains(iDayOfWeek);
                                DateTime WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));

                                if (dayCardSetting != null)
                                {
                                    if (hasThirdSetting == 1)
                                    {
                                        WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDENDTIME).ToString("HH:mm"));
                                    }
                                    if (hasFourthSetting == 1)
                                    {
                                        WorkDayEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).ToString("HH:mm"));
                                    }
                                }

                                #region "   这个时间段内存在假期，要扣除    "
                                if (entVacDays.Count() == 0)
                                {   //也不是设置的工作日

                                    if (entWorkDays.Count() == 0)
                                    {   //并且不在上班时间列表中那就是休息日,除去休息日

                                        if (!isDayCount)
                                        {

                                            SatSundayList.Add(strDateString);

                                            tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                            totalVacationHours += LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;
                                            totalVacationDays += 1;

                                            double dayVacationHours = LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;

                                            //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                            KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                            if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                            {
                                                decimal dTotalYearVacation = 0;
                                                dTotalYearVacation = (decimal)dayVacationHours;
                                                dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                            }
                                            else
                                            {
                                                decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                dTotalYearVacation += (decimal)dayVacationHours;
                                                dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                            }
                                        }
                                    }
                                    else//周六，日设为工作周
                                    {
                                        foreach (var work in entWorkDays)
                                        {
                                            #region "   半天公共假期，三八妇女节，青年节    "
                                            if (work.ISHALFDAY == "1")
                                            {

                                                //tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                                if (work.PEROID == "0")//上午上班，下午放假
                                                {
                                                    if (dayCardSetting != null)
                                                    {
                                                        DateTime HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).ToString("HH:mm"));
                                                        DateTime HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));
                                                        //四个时间中，下午时间的开始与结束
                                                        if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                        {
                                                            HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).ToString("HH:mm"));
                                                            HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).ToString("HH:mm"));
                                                        }
                                                        //获取半天放假时间
                                                        //tempWorkArr.Add(HalfNoonStartDate.ToString("HH:mm:ss") + "|" + HalfNoonEndDate.ToString("HH:mm:ss"));
                                                        //tempHalfNotWorkArr.Add(HalfNoonStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfNoonEndDate.ToString("yyyy-MM-dd HH:mm:ss"));

                                                        dtTempHalfNotWorkStartDate = HalfNoonStartDate;
                                                        dtTempHalfNotWorkEndDate = HalfNoonEndDate;
                                                        totalVacationHours += HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours;
                                                        if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                        {
                                                            totalVacationDays += Math.Round(HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                        }

                                                        double dayVacationHours = HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours; ;

                                                        //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                                        KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                                        if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                                        {
                                                            decimal dTotalYearVacation = 0;
                                                            dTotalYearVacation = (decimal)dayVacationHours;
                                                            dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                                        }
                                                        else
                                                        {
                                                            decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                            dTotalYearVacation += (decimal)dayVacationHours;
                                                            dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                            dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                                        }
                                                    }
                                                }
                                                else//上午放假，下午上班
                                                {
                                                    if (dayCardSetting != null)
                                                    {
                                                        DateTime HalfMorningStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).ToString("HH:mm"));
                                                        DateTime HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).ToString("HH:mm"));
                                                        //四个时间中，上午时间的开始与结束
                                                        if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                        {
                                                            HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));
                                                        }
                                                        //获取上午放假时间
                                                        //tempWorkArr.Add(HalfMorningStartDate.ToString("HH:mm:ss") + "|" + HalfMorningEndDate.ToString("HH:mm:ss"));
                                                        //tempHalfNotWorkArr.Add(HalfMorningStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfMorningEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        dtTempHalfNotWorkStartDate = HalfMorningStartDate;
                                                        dtTempHalfNotWorkEndDate = HalfMorningEndDate;
                                                        totalVacationHours += HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours;
                                                        if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                        {
                                                            totalVacationDays += Math.Round(HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                        }

                                                        double dayVacationHours = HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours;

                                                        //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                                        KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                                        if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                                        {
                                                            decimal dTotalYearVacation = 0;
                                                            dTotalYearVacation = (decimal)dayVacationHours;
                                                            dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                                        }
                                                        else
                                                        {
                                                            decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                            dTotalYearVacation += (decimal)dayVacationHours;
                                                            dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                            dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {   //周六日全天上班                                               
                                                //tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                                //totalVacationHours += LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;
                                                //totalVacationDays += 1;
                                            }
                                            #endregion
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
                                            //tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                            if (vac.PEROID == "1")//下午放假
                                            {
                                                if (dayCardSetting != null)
                                                {
                                                    DateTime HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDSTARTTIME).ToString("HH:mm"));
                                                    DateTime HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));
                                                    //四个时间中，下午时间的开始与结束
                                                    if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                    {
                                                        HalfNoonStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.THIRDSTARTTIME).ToString("HH:mm"));
                                                        HalfNoonEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FOURTHENDTIME).ToString("HH:mm"));
                                                    }
                                                    //获取半天放假时间
                                                    //tempWorkArr.Add(HalfNoonStartDate.ToString("HH:mm:ss") + "|" + HalfNoonEndDate.ToString("HH:mm:ss"));
                                                    //tempHalfNotWorkArr.Add(HalfNoonStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfNoonEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                    dtTempHalfNotWorkStartDate = HalfNoonStartDate;
                                                    dtTempHalfNotWorkEndDate = HalfNoonEndDate;
                                                    totalVacationHours += HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours;
                                                    if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                    {
                                                        totalVacationDays += Math.Round(HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                    }

                                                    double dayVacationHours = HalfNoonEndDate.Subtract(HalfNoonStartDate).TotalHours;

                                                    //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                                    KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                                    if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                                    {
                                                        decimal dTotalYearVacation = 0;
                                                        dTotalYearVacation = (decimal)dayVacationHours;
                                                        dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                                    }
                                                    else
                                                    {
                                                        decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                        dTotalYearVacation += (decimal)dayVacationHours;
                                                        dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                        dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                                    }
                                                }
                                            }
                                            else//上午放假
                                            {
                                                if (dayCardSetting != null)
                                                {
                                                    DateTime HalfMorningStartDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTSTARTTIME).ToString("HH:mm"));
                                                    DateTime HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.FIRSTENDTIME).ToString("HH:mm"));
                                                    //四个时间中，上午时间的开始与结束
                                                    if (hasThirdSetting == 1 && hasFourthSetting == 1)
                                                    {
                                                        HalfMorningEndDate = Convert.ToDateTime(dtTempStartDate.ToString("yyyy-MM-dd") + " " + Convert.ToDateTime(dayCardSetting.SECONDENDTIME).ToString("HH:mm"));
                                                    }
                                                    //获取上午放假时间
                                                    //tempWorkArr.Add(HalfMorningStartDate.ToString("HH:mm:ss") + "|" + HalfMorningEndDate.ToString("HH:mm:ss"));
                                                    //tempHalfNotWorkArr.Add(HalfMorningStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + HalfMorningEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                    dtTempHalfNotWorkStartDate = HalfMorningStartDate;
                                                    dtTempHalfNotWorkEndDate = HalfMorningEndDate;
                                                    totalVacationHours += HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours;
                                                    if (LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue && LeavePeriodAttendSolution.WORKTIMEPERDAY.Value > 0)
                                                    {
                                                        totalVacationDays += Math.Round(HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value), 2);
                                                    }

                                                    double dayVacationHours = HalfMorningEndDate.Subtract(HalfMorningStartDate).TotalHours;

                                                    //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                                    KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                                    if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                                    {
                                                        decimal dTotalYearVacation = 0;
                                                        dTotalYearVacation = (decimal)dayVacationHours;
                                                        dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                                    }
                                                    else
                                                    {
                                                        decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                        dTotalYearVacation += (decimal)dayVacationHours;
                                                        dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                        dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {   //全天假期
                                            tempWorkArr = sWorkArr;//赋值后让假期包含在总时长中
                                            totalVacationHours += LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;
                                            totalVacationDays += 1;

                                            double dayVacationHours = LeavePeriodAttendSolution.WORKTIMEPERDAY.HasValue ? Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value) : 0;

                                            //获取各个年份的假期，主要为了避免年假跨年使用的情况
                                            KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                                            if (string.IsNullOrEmpty(kvYearFreeVacation.Key))
                                            {
                                                decimal dTotalYearVacation = 0;
                                                dTotalYearVacation = (decimal)dayVacationHours;
                                                dicYearFreeVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                                            }
                                            else
                                            {
                                                decimal dTotalYearVacation = kvYearFreeVacation.Value;
                                                dTotalYearVacation += (decimal)dayVacationHours;
                                                dicYearFreeVacation.Remove(kvYearFreeVacation.Key);
                                                dicYearFreeVacation.Add(kvYearFreeVacation.Key, dTotalYearVacation);
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                #endregion

                                #endregion
                            }

                            #region "   计算请假时间     "
                            //填写的时段都在4个设置的上班时间段里面，则直接用结束时间减去开始时间
                            if (
                                  (dtTempStartDate >= FirstCardStartDate && dtTempStartDate <= FirstCardEndDate && dtTempEndDate >= FirstCardStartDate && dtTempEndDate <= FirstCardEndDate)
                                  ||
                                  (dtTempStartDate >= SecondCardStartDate && dtTempStartDate <= SecondCardEndDate && dtTempEndDate >= SecondCardStartDate && dtTempEndDate <= SecondCardEndDate)
                                  ||
                                  (dtTempStartDate >= ThirdCardStartDate && dtTempStartDate <= ThirdCardEndDate && dtTempEndDate >= ThirdCardStartDate && dtTempEndDate <= ThirdCardEndDate)
                                  ||
                                  (dtTempStartDate >= FourthCardStartDate && dtTempStartDate <= FourthCardEndDate && dtTempEndDate >= FourthCardStartDate && dtTempEndDate <= FourthCardEndDate)
                                )
                            {
                                #region "   填写的时段都在4个设置的上班时间段里面  "
                                //totalVacationHours,如果有假期存在，则加上假期时间，如果没有，假期就为0；
                                if (dtTempStartDate >= dtTempHalfNotWorkStartDate && dtTempStartDate <= dtTempHalfNotWorkEndDate)
                                {
                                    totalTempHours += totalVacationHours;
                                    totalTempDays += Math.Floor(totalTempHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value)) + totalVacationDays;
                                }
                                else
                                {
                                    totalTempHours += dtTempEndDate.Subtract(dtTempStartDate).TotalHours + totalVacationHours;
                                    totalTempDays += Math.Floor(totalTempHours / Convert.ToDouble(LeavePeriodAttendSolution.WORKTIMEPERDAY.Value)) + totalVacationDays;
                                }
                                //totalHours = dtTempEndDate.Subtract(dtTempStartDate).TotalHours;   
                                #endregion
                            }
                            else
                            {

                                #region "   设置开始和结束时间点：8：30-17：30"
                                //早7：50打开，将计算加班的有效开始时间设置为8：30，也就是dtCardStartDate
                                //23:00下班，假定四个工作时间段
                                if (dtTempStartDate < FirstCardStartDate)
                                {
                                    dtTempStartDate = FirstCardStartDate;
                                    //设置开始时间点
                                    if (iCountVal == 1)
                                    {
                                        response.StartTime = dtTempStartDate.ToString("HH:mm:ss");
                                    }
                                }


                                //设置结束时间点,如果用于选择了上班时间以外的开始和结束时间
                                //则为其转换为完整的一天工作时间
                                if (iCountVal == dateDetailList.Count())
                                {
                                    if (hasSecondSetting == 1)
                                    {
                                        if (dtTempEndDate <= FirstCardStartDate || dtTempEndDate >= SecondCardEndDate)
                                        {
                                            response.EndTime = SecondCardEndDate.ToString("HH:mm:ss");
                                            dtTempEndDate = Convert.ToDateTime(dtTempEndDate.ToString("yyyy-MM-dd") + " " + SecondCardEndDate.ToString("HH:mm:ss"));
                                        }
                                    }

                                    if (hasThirdSetting == 1)
                                    {
                                        if (dtTempEndDate <= FirstCardStartDate || dtTempEndDate >= ThirdCardEndDate)
                                        {
                                            response.EndTime = ThirdCardEndDate.ToString("HH:mm:ss");
                                            dtTempEndDate = Convert.ToDateTime(dtTempEndDate.ToString("yyyy-MM-dd") + " " + ThirdCardEndDate.ToString("HH:mm:ss"));
                                        }
                                    }

                                    if (hasFourthSetting == 1)
                                    {
                                        if (dtTempEndDate <= FirstCardStartDate || dtTempEndDate >= FourthCardEndDate)
                                        {
                                            response.EndTime = FourthCardEndDate.ToString("HH:mm:ss");
                                            dtTempEndDate = Convert.ToDateTime(dtTempEndDate.ToString("yyyy-MM-dd") + " " + FourthCardEndDate.ToString("HH:mm:ss"));
                                        }
                                    }
                                }


                                #endregion

                                #region "   找出开始计算加班的时间点    "

                                DateTime tempLeaveDate = new DateTime();
                                foreach (string str in sWorkArr)
                                {
                                    string[] s = str.Split('|');
                                    DateTime WorkStartDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate).ToString("yyyy-MM-dd") + " " + s[0]);
                                    DateTime WorkEndDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate).ToString("yyyy-MM-dd") + " " + s[1]);
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

                                #region "   设置具体的加班是时间段     "
                                //计算请假的时间段有可能是节假日，这时已经在上面的假期中减去了，
                                //而总时长里却因为用户选择了结束时间在凌成1：30分，因此程序没能计算到这一天的时长，导致总时长少了一天
                                if (tempWorkArr.Count == 0)
                                {
                                    for (int i = 0; i < sWorkArr.Count(); i++)
                                    {
                                        string[] ss = sWorkArr[i].Split('|');
                                        DateTime WorkStartDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate).ToString("yyyy-MM-dd") + " " + ss[0]);
                                        DateTime WorkEndDate = Convert.ToDateTime(Convert.ToDateTime(dtTempStartDate).ToString("yyyy-MM-dd") + " " + ss[1]);
                                        //工作时间段的开始时间大于加班开始时间
                                        if (WorkStartDate >= tempLeaveDate)
                                        {
                                            //加班结束时间大于工作时间段的结束时间
                                            //则说明加班时间包含这段工作时间段，计算完整的加班时间段
                                            if (dtTempEndDate >= WorkEndDate)
                                            {
                                                if (sNotWorkArr.Count() == 0)
                                                {
                                                    tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + dtTempEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                }

                                                foreach (string str3 in sNotWorkArr)
                                                {
                                                    string[] sn = str3.Split('|');
                                                    DateTime notWorkStartDate = Convert.ToDateTime(sn[0]);
                                                    DateTime notWorkEndDate = Convert.ToDateTime(sn[1]);
                                                    if (dtTempEndDate >= notWorkStartDate && dtTempEndDate <= notWorkEndDate)
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + WorkEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                    }
                                                    else if (dtTempEndDate >= WorkEndDate)
                                                    {
                                                        if (i == sWorkArr.Count() - 1)
                                                        {
                                                            WorkEndDate = dtTempEndDate;
                                                        }
                                                        tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + WorkEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + notWorkStartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (sNotWorkArr.Count() == 0)
                                                {
                                                    tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + dtTempEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
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
                                                        tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + WorkEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                                        sNotWorkArr.Remove(str3);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        tempWorkArr.Add(WorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + dtTempEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
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
                                            tempWorkArr.Add(tempLeaveDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + WorkEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                        }
                                    }
                                    List<string> tempViaArr = new List<string>();

                                    foreach (var str1 in tempWorkArr)
                                    {
                                        string[] nsr1 = str1.Split('|');
                                        if (nsr1[0] != dtTempHalfNotWorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") && Convert.ToDateTime(nsr1[1]) > dtTempHalfNotWorkEndDate)
                                        {
                                            tempViaArr.Add(str1);
                                        }

                                        if (nsr1[0] != dtTempHalfNotWorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") && Convert.ToDateTime(nsr1[1]) < dtTempHalfNotWorkEndDate)
                                        {
                                            tempViaArr.Add(str1);
                                        }
                                    }
                                    tempViaArr.Add(dtTempHalfNotWorkStartDate.ToString("yyyy-MM-dd HH:mm:ss") + "|" + dtTempHalfNotWorkEndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                    tempWorkArr = tempViaArr;
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


                            }
                            #endregion
                        }
                    }
                    totalHours += totalTempHours;
                    totalDays += totalTempDays;

                    //获取各个年份的假期，主要为了避免年假跨年使用的情况
                    KeyValuePair<string, decimal> kvYearVacation = dicYearVacation.Where(t => t.Key == dtTempStartDate.Year.ToString()).FirstOrDefault();
                    if (string.IsNullOrEmpty(kvYearVacation.Key))
                    {
                        decimal dTotalYearVacation = 0;
                        dTotalYearVacation = (decimal)totalTempHours;
                        dicYearVacation.Add(dtTempStartDate.Year.ToString(), dTotalYearVacation);
                    }
                    else
                    {
                        decimal dTotalYearVacation = kvYearVacation.Value;
                        dTotalYearVacation += (decimal)totalTempHours;
                        dicYearVacation.Remove(kvYearVacation.Key);
                        dicYearVacation.Add(kvYearVacation.Key, dTotalYearVacation);
                    }

                }//foreach (string strDateString in dateList)       
                #endregion

                totalHours = totalHours - totalVacationHours;
                totalDays = totalDays - totalVacationDays;

                totalHours = totalHours > 0 ? totalHours : 0;
                totalDays = totalDays > 0 ? totalDays : 0;
                #region "   根据员工ID，假期类型，以及该假期的状态，获取该假期类型的LeftHours  "

                List<T_HR_EMPLOYEELEVELDAYCOUNT> daycount = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                var daycountQueryable = leaveDayCountBll.GetLevelDayCountByEmployeeIDAndVacType(request.EmployeeID, request.LeaveTypeValue.ToString());

                List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountAnual = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                var daycountAnualQueryable = leaveDayCountBll.GetLevelDayCountByEmployeeIDAndVacType(request.EmployeeID, Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString());

                List<T_HR_EMPLOYEELEVELDAYCOUNT> daycountAdjust = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();
                var daycountAdjustQueryable = leaveDayCountBll.GetLevelDayCountByEmployeeIDAndVacType(request.EmployeeID, Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString());

                if (daycountAdjustQueryable != null)
                {

                    DateTime dtTempLeaveStartDate = Convert.ToDateTime(request.StartDate).AddMonths(-6);
                    DateTime dtTempLeaveEndDate = Convert.ToDateTime(request.StartDate).AddDays(1);
                    daycountAdjustQueryable = daycountAdjustQueryable.Where(t => t.LEFTHOURS > 0 && t.EFFICDATE >= dtTempLeaveStartDate && t.EFFICDATE < dtTempLeaveEndDate).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS);
                }

                DateTime dtCompareDate = new DateTime();
                //汇总年假天数
                //并且在请假日期开始这天未过期的假期
                if (daycountAnualQueryable != null)
                {
                    decimal dYearAnnualVacation = 0;
                    //decimal dYearAnnualFreeVacation = 0;
                    //if (Convert.ToDateTime(request.StartDate) <= Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31"))
                    //    && Convert.ToDateTime(request.EndDate) > Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31")))
                    //{
                    //    if (dicYearFreeVacation != null && dicYearFreeVacation.Count > 0)
                    //    {
                    //        KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == Convert.ToDateTime(request.StartDate).Year.ToString()).FirstOrDefault();
                    //        {
                    //            dYearAnnualFreeVacation = kvYearFreeVacation.Value;
                    //        }
                    //    }

                    //    if (dicYearVacation != null && dicYearVacation.Count > 0)
                    //    {
                    //        KeyValuePair<string, decimal> kvYearVacation = dicYearVacation.Where(t => t.Key == Convert.ToDateTime(request.StartDate).Year.ToString()).FirstOrDefault();
                    //        {
                    //            dYearAnnualVacation = kvYearVacation.Value - dYearAnnualFreeVacation;
                    //        }
                    //    }
                    //}

                    daycountAnual = daycountAnualQueryable.Where(t => t.STATUS == 1 && t.LEFTHOURS > 0).ToList();
                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT entity in daycountAnual)
                    {
                        dtCompareDate = DateTime.Now.AddDays(1).AddSeconds(-1);
                        if (entity.TERMINATEDATE.HasValue && entity.TERMINATEDATE.Value.Year != 9999)
                        {
                            dtCompareDate = entity.TERMINATEDATE.Value.AddDays(1).AddSeconds(-1);
                        }
                        else
                        {
                            dtCompareDate = entity.TERMINATEDATE.Value;
                        }
                        if (dtStartDate <= dtCompareDate)
                        {
                            response.AnnualVacationHours += Convert.ToDouble(entity.LEFTHOURS);
                        }

                    }

                    //if (Convert.ToDateTime(request.StartDate) <= Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31"))
                    //   && Convert.ToDateTime(request.EndDate) > Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31")))
                    //{
                    //    response.AnnualVacationHours = (double)dYearAnnualVacation;
                    //    response.AnnualVacationDays = Math.Round((double)dYearAnnualVacation / averageWorkPerDay, 2);
                    //    response.LeftHours = 0;
                    //}
                    //else
                    //{
                    response.AnnualVacationHours = response.AnnualVacationHours - (double)dYearAnnualVacation;
                    response.AnnualVacationDays = Math.Round(response.AnnualVacationHours / averageWorkPerDay, 2);
                    //}
                }
                //汇总加班假天数
                //并且在请假日期开始这天未过期的假期
                if (daycountAdjustQueryable != null)
                {
                    daycountAdjust = daycountAdjustQueryable.Where(t => t.STATUS == 1 && t.LEFTHOURS > 0).ToList();
                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT entity in daycountAdjust)
                    {
                        dtCompareDate = DateTime.Now.AddDays(1).AddSeconds(-1);
                        if (entity.TERMINATEDATE.HasValue && entity.TERMINATEDATE.Value.Year != 9999)
                        {
                            dtCompareDate = entity.TERMINATEDATE.Value.AddDays(1).AddSeconds(-1);
                        }
                        else
                        {
                            dtCompareDate = entity.TERMINATEDATE.Value;
                        }

                        if (dtStartDate <= dtCompareDate)
                        {
                            response.AdjustLeaveHours += Convert.ToDouble(entity.LEFTHOURS);
                        }
                    }
                    response.AdjustLeaveDays = Math.Round(response.AdjustLeaveHours / averageWorkPerDay, 2);
                }

                DateTime dtYouthDayExpiredDate = DateTime.MinValue;
                //汇总该用户请假时选择的假期类型的天数
                //并且在请假日期开始这天未过期的假期
                double OnceRestVacationDays = 0;
                if (daycountQueryable != null)
                {
                    if (request.LeaveTypeValue == Enums.LeaveVacationType.AnnualDay.GetHashCode())
                    {
                        //decimal dYearAnnualVacation = 0;
                        //decimal dYearAnnualFreeVacation = 0;
                        //if (Convert.ToDateTime(request.StartDate) <= Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31"))
                        //    && Convert.ToDateTime(request.EndDate) > Convert.ToDateTime(Convert.ToDateTime(request.StartDate).ToString("yyyy-12-31")))
                        //{
                        //    if (dicYearFreeVacation != null && dicYearFreeVacation.Count > 0)
                        //    {
                        //        KeyValuePair<string, decimal> kvYearFreeVacation = dicYearFreeVacation.Where(t => t.Key == Convert.ToDateTime(request.StartDate).Year.ToString()).FirstOrDefault();
                        //        {
                        //            dYearAnnualFreeVacation = kvYearFreeVacation.Value;
                        //        }
                        //    }

                        //    if (dicYearVacation != null && dicYearVacation.Count > 0)
                        //    {
                        //        KeyValuePair<string, decimal> kvYearVacation = dicYearVacation.Where(t => t.Key == Convert.ToDateTime(request.StartDate).Year.ToString()).FirstOrDefault();
                        //        {
                        //            dYearAnnualVacation = kvYearVacation.Value - dYearAnnualFreeVacation;
                        //        }
                        //    }
                        //    lefthours = (double)dYearAnnualVacation;
                        //}
                        //else
                        //{//不是年假跨年
                        lefthours = response.AnnualVacationHours;
                        OnceRestVacationDays = response.AnnualVacationDays;
                        //}
                    }
                    else
                    {
                        daycount = daycountQueryable.Where(t => t.STATUS == 1 && t.LEFTHOURS > 0).ToList();

                        if (request.LeaveTypeValue == Enums.LeaveVacationType.YouthLeaveDay.GetHashCode() || request.LeaveTypeValue == Enums.LeaveVacationType.WomenDay.GetHashCode())
                        {
                            if (daycount != null)
                            {
                                var YouthDayDaycount = daycount.FirstOrDefault();
                                if (YouthDayDaycount != null)
                                {
                                    dtYouthDayExpiredDate = YouthDayDaycount.TERMINATEDATE.HasValue ? YouthDayDaycount.TERMINATEDATE.Value : DateTime.MinValue;
                                }
                            }

                            foreach (T_HR_EMPLOYEELEVELDAYCOUNT entity in daycount)
                            {
                                dtCompareDate = DateTime.Now.AddDays(1).AddSeconds(-1);
                                if (entity.TERMINATEDATE.HasValue && entity.TERMINATEDATE.Value.Year != 9999)
                                {
                                    dtCompareDate = entity.TERMINATEDATE.Value.AddDays(1).AddSeconds(-1);
                                }
                                else
                                {
                                    dtCompareDate = entity.TERMINATEDATE.Value;
                                }
                                if (dtStartDate <= dtCompareDate)
                                {
                                    lefthours += Convert.ToDouble(entity.LEFTHOURS);
                                    OnceRestVacationDays += Convert.ToDouble(entity.DAYS);
                                }
                            }
                        }
                        else if (request.LeaveTypeValue == Enums.LeaveVacationType.MaternityLeaveDay.GetHashCode())
                        {
                            //产假无需管过期时间
                            foreach (T_HR_EMPLOYEELEVELDAYCOUNT entity in daycount)
                            {
                                lefthours += Convert.ToDouble(entity.LEFTHOURS);
                                OnceRestVacationDays += Convert.ToDouble(entity.DAYS);
                            }
                        }
                        else
                        {
                            foreach (T_HR_EMPLOYEELEVELDAYCOUNT entity in daycount)
                            {
                                dtCompareDate = DateTime.Now.AddDays(1).AddSeconds(-1);
                                if (entity.TERMINATEDATE.HasValue && entity.TERMINATEDATE.Value.Year != 9999)
                                {
                                    dtCompareDate = entity.TERMINATEDATE.Value.AddDays(1).AddSeconds(-1);
                                }
                                else
                                {
                                    dtCompareDate = entity.TERMINATEDATE.Value;
                                }
                                if (dtStartDate <= dtCompareDate)
                                {
                                    lefthours += Convert.ToDouble(entity.LEFTHOURS);
                                    OnceRestVacationDays += Convert.ToDouble(entity.DAYS);
                                }
                            }
                        }
                    }
                }
                #endregion

                //汇总加班天数，时长，已经时间零头
                response.LeftHours = lefthours;
                response.LeftDays = Math.Round(lefthours / averageWorkPerDay, 2);
                response.LeaveDays = Math.Floor(totalHours / averageWorkPerDay);
                response.LeaveHours = totalHours % averageWorkPerDay;//取余数，用于前台显示的剩余天数
                response.LeaveTotalHours = totalHours;
                response.LeaveTotalDays = Math.Round(totalHours / averageWorkPerDay, 2);

                //计算加班的时长是否够调休
                List<T_HR_LEAVEREFEROT> storeOverTimeList = new List<T_HR_LEAVEREFEROT>();
                List<string> storeOverTime = new List<string>();
                if (response.AdjustLeaveHours >= 0 && request.LeaveTypeValue == (int)Enums.LeaveVacationType.AdjustLeaveDay)
                {
                    string strAnnualDayType = Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString();
                    #region "   申请加班调休假     "
                    T_HR_ATTENDFREELEAVE freeLeaveSetEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strAnnualDayType).FirstOrDefault();
                    T_HR_LEAVETYPESET tempLeaveTypeEntity = freeLeaveSetEntity.T_HR_LEAVETYPESET;// ltsList.Where(t => t.LEAVETYPEVALUE == strAnnualDayType).FirstOrDefault();
                    //请假时间大于可用的调休时间
                    if ((response.LeaveTotalHours - response.AdjustLeaveHours) >= 0)
                    {
                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                        {
                            //加班记录ID|加班时间
                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                            adjLeaveOTEntity.STATUS = 0;
                            storeOverTimeList.Add(adjLeaveOTEntity);
                        }


                        #region "   请假时需要扣款，根据类型用年假和加班调休假抵扣      "

                        //FineType: 0,不扣(带薪假) 1、扣款；2、调休+扣款；3、调休+带薪假抵扣+扣款；
                        //请假时长不够，用年假和加班调休假抵扣                    
                        switch (strFineType)//请假时，如果年假和调休有剩余时间就可以供其他假期抵扣
                        {
                            case "1":
                            case "2":
                            case "3":
                                break;
                            case "4":
                                response.LeftHours = lefthours + response.AnnualVacationHours;
                                response.LeftDays = Math.Round(response.LeftHours / averageWorkPerDay, 2);
                                if (response.LeftHours <= response.LeaveTotalHours)
                                {
                                    response.FineDays = Math.Round((response.LeaveTotalHours - response.LeftHours) / response.AvgWorkPerDay, 2);
                                    response.FineHours = response.LeaveTotalHours - response.LeftHours;
                                    //有可能年假为0
                                    if (daycountAnual.Count > 0)
                                    {
                                        T_HR_EMPLOYEELEVELDAYCOUNT ent = daycountAnual.FirstOrDefault();

                                        if (ent != null)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = ent.RECORDID;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                        }
                                    }
                                }
                                else
                                {//加上年假后足够，有剩余,差的时间=response.LeaveTotalHours - response.AdjustLeaveHours
                                    double tempLessAnnualHours = response.LeaveTotalHours - response.AdjustLeaveHours;
                                    T_HR_EMPLOYEELEVELDAYCOUNT ent = daycountAnual.FirstOrDefault();
                                    if (ent != null)
                                    {
                                        //加班记录ID|加班时间
                                        T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                        adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                        adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                        adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                        adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(tempLessAnnualHours);
                                        adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                        adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                        adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                        adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                        adjLeaveOTEntity.OVERTIME_RECORDID = ent.RECORDID;
                                        adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                        adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                        adjLeaveOTEntity.STATUS = 0;
                                        storeOverTimeList.Add(adjLeaveOTEntity);
                                    }
                                }
                                break;
                        }

                        #endregion
                    }
                    else
                    {
                        #region "   请假时间小于可用的调休时间   "

                        double oweLeaveHours = 0;
                        double leftLeaveHours = response.LeaveTotalHours;
                        string strDayCount = string.Empty;
                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                        {
                            //加班记录ID|加班时间
                            oweLeaveHours = leftLeaveHours - Convert.ToDouble(ent.LEFTHOURS.HasValue ? ent.LEFTHOURS.Value : 0);
                            //请假时间大于加班时间
                            if (oweLeaveHours > 0)
                            {
                                leftLeaveHours = oweLeaveHours;

                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                adjLeaveOTEntity.STATUS = 0;
                                storeOverTimeList.Add(adjLeaveOTEntity);
                                continue;
                            }
                            //请假时间还差oweLeaveHours
                            if (oweLeaveHours <= 0)
                            {
                                //oweLeaveHours = Convert.ToDouble(ent.LEFTHOURS.HasValue ? ent.LEFTHOURS.Value : 0) - leftLeaveHours;

                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(leftLeaveHours);
                                adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                adjLeaveOTEntity.STATUS = 0;
                                storeOverTimeList.Add(adjLeaveOTEntity);
                                break;
                            }
                        }

                        #endregion
                    }
                    response.leaveReferOT = storeOverTimeList;
                    #endregion
                }
                else if (response.AnnualVacationHours >= 0 && request.LeaveTypeValue == (int)Enums.LeaveVacationType.AnnualDay.GetHashCode())
                {
                    #region "   申请年假    "
                    string strAdjustLeaveDayType = Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString();

                    T_HR_ATTENDFREELEAVE freeLeaveSetEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strAdjustLeaveDayType).FirstOrDefault();
                    T_HR_LEAVETYPESET tempLeaveTypeEntity = freeLeaveSetEntity.T_HR_LEAVETYPESET;// ltsList.Where(t => t.LEAVETYPEVALUE == strAnnualDayType).FirstOrDefault();

                    //T_HR_LEAVETYPESET tempLeaveTypeEntity = ltsList.Where(t => t.LEAVETYPEVALUE == strAdjustLeaveDayType).FirstOrDefault();
                    //请假时间大于可用的年假时间
                    if ((response.LeaveTotalHours - response.AnnualVacationHours) >= 0)
                    {
                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAnual)
                        {
                            //加班记录ID|加班时间
                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                            adjLeaveOTEntity.OVERTIME_RECORDID = ent.RECORDID;
                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                            adjLeaveOTEntity.STATUS = 0;
                            storeOverTimeList.Add(adjLeaveOTEntity);
                        }

                        #region "   请假时需要扣款，根据类型用年假和加班调休假抵扣      "

                        //FineType: 0,不扣(带薪假) 1、扣款；2、调休+扣款；3、调休+带薪假抵扣+扣款；
                        //请假时长不够，用年假和加班调休假抵扣                    
                        switch (strFineType)//请假时，如果年假和调休有剩余时间就可以供其他假期抵扣
                        {
                            case "1":
                            case "2":
                            case "3":
                                break;
                            case "4":
                                response.LeftHours = lefthours + response.AdjustLeaveHours;
                                response.LeftDays = Math.Round(response.LeftHours / averageWorkPerDay, 2);
                                if (response.LeftHours <= response.LeaveTotalHours)
                                {
                                    response.FineDays = Math.Round((response.LeaveTotalHours - response.LeftHours) / response.AvgWorkPerDay, 2);
                                    response.FineHours = response.LeaveTotalHours - response.LeftHours;

                                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                                    {

                                        //加班记录ID|加班时间
                                        T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                        adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                        adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                        adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                        adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                        adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                        adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                        adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                        adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                        adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                        adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                        adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                        adjLeaveOTEntity.STATUS = 0;
                                        storeOverTimeList.Add(adjLeaveOTEntity);
                                    }
                                }
                                else
                                {
                                    #region "   请假时间小于可用的调休时间   "

                                    double oweLeaveHours = 0;
                                    double leftLeaveHours = response.LeaveTotalHours;
                                    string strDayCount = string.Empty;
                                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                                    {
                                        //加班记录ID|加班时间
                                        oweLeaveHours = leftLeaveHours - Convert.ToDouble(ent.LEFTHOURS.HasValue ? ent.LEFTHOURS.Value : 0);
                                        //请假时间大于加班时间
                                        if (oweLeaveHours > 0)
                                        {
                                            leftLeaveHours = oweLeaveHours;

                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                            continue;
                                        }
                                        //请假时间还差oweLeaveHours
                                        if (oweLeaveHours <= 0)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(leftLeaveHours);
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                            break;

                                        }
                                    }

                                    #endregion
                                }
                                break;
                        }

                        #endregion
                    }
                    else//请假时长小于年假时长
                    {
                        #region "   请假时间小于可用的调休时间   "

                        T_HR_EMPLOYEELEVELDAYCOUNT ent = daycountAnual.FirstOrDefault();

                        if (ent != null)
                        {
                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);

                            adjLeaveOTEntity.OVERTIME_RECORDID = ent.RECORDID;
                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;

                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                            adjLeaveOTEntity.STATUS = 0;
                            storeOverTimeList.Add(adjLeaveOTEntity);
                        }
                        #endregion
                    }
                    response.leaveReferOT = storeOverTimeList;
                    #endregion
                }
                else
                {
                    #region "   申请其他类型假期，根据FineType用年假和加班调休假抵扣      "
                    string strAdjustLeaveDayType = Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode().ToString();
                    string strAnnualLeaveDayType = Enums.LeaveVacationType.AnnualDay.GetHashCode().ToString();

                    T_HR_ATTENDFREELEAVE AdjustFreeLeaveSetEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strAdjustLeaveDayType).FirstOrDefault();
                    T_HR_LEAVETYPESET tempAdjustLeaveTypeEntity = AdjustFreeLeaveSetEntity.T_HR_LEAVETYPESET;

                    T_HR_ATTENDFREELEAVE AnnualFreeLeaveSetEntity = entAttendFreeLeaves.Where(t => t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strAnnualLeaveDayType).FirstOrDefault();
                    T_HR_LEAVETYPESET tempAnnualLeaveTypeEntity = AnnualFreeLeaveSetEntity.T_HR_LEAVETYPESET;


                    #region "   申请病假类型      "

                    //请病假，当月有请病假就不扣减，没有就扣减
                    if (request.LeaveTypeValue == Enums.LeaveVacationType.SickLeaveDay.GetHashCode() && isHuNanHangXingSalary == "false")
                    {
                        DateTime currentStartDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
                        DateTime currentEndDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddSeconds(-1);
                        string strSickLeaveType = Enums.LeaveVacationType.SickLeaveDay.GetHashCode().ToString();
                        List<T_HR_EMPLOYEELEAVERECORD> SickLeaveEntityList = new List<T_HR_EMPLOYEELEAVERECORD>();
                        SickLeaveEntityList = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(t => t.STARTDATETIME >= currentStartDate
                            && t.ENDDATETIME <= currentEndDate
                            && t.CHECKSTATE == "2" && t.T_HR_LEAVETYPESET.LEAVETYPEVALUE == strSickLeaveType
                            && t.EMPLOYEEID == request.EmployeeID).ToList();

                        decimal totalsickleavehour = SickLeaveEntityList.Sum(t => t.TOTALHOURS.Value);
                        decimal totalsickleaveday = Math.Round(totalsickleavehour / (decimal)averageWorkPerDay, 2);

                        T_HR_EMPLOYEELEVELDAYCOUNT sickLeaveDayCount = new T_HR_EMPLOYEELEVELDAYCOUNT();
                        if (daycountQueryable != null)
                        {
                            sickLeaveDayCount = daycountQueryable.Where(t => t.STATUS == 1 && t.LEFTHOURS > 0).FirstOrDefault();
                        }

                        if (sickLeaveDayCount != null)
                        {
                            if (totalsickleavehour >= 0 && totalsickleavehour < (decimal)averageWorkPerDay)
                            {
                                decimal sickleavehours = (decimal)averageWorkPerDay - totalsickleavehour;
                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT otherAdjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                otherAdjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                otherAdjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                otherAdjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();

                                otherAdjLeaveOTEntity.LEAVE_TOTAL_HOURS = sickleavehours;
                                otherAdjLeaveOTEntity.LEAVE_TOTAL_DAYS = Math.Round(sickleavehours / (decimal)averageWorkPerDay, 2); ;

                                response.LeftHours = (double)sickleavehours;
                                response.LeftDays = Math.Round(response.LeftHours / averageWorkPerDay, 2);

                                response.FineHours = response.LeaveTotalHours - (double)sickleavehours;
                                response.FineDays = Math.Round(response.FineHours / averageWorkPerDay, 2);

                                otherAdjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                otherAdjLeaveOTEntity.OVERTIME_RECORDID = sickLeaveDayCount.RECORDID;
                                otherAdjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                otherAdjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                otherAdjLeaveOTEntity.STATUS = 0;
                                storeOverTimeList.Add(otherAdjLeaveOTEntity);
                            }
                            else if (totalsickleavehour >= 1)
                            {
                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT otherAdjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                otherAdjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                otherAdjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                otherAdjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();

                                otherAdjLeaveOTEntity.LEAVE_TOTAL_HOURS = 0;
                                otherAdjLeaveOTEntity.LEAVE_TOTAL_DAYS = 0;

                                otherAdjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                otherAdjLeaveOTEntity.OVERTIME_RECORDID = sickLeaveDayCount.RECORDID;
                                otherAdjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                otherAdjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                otherAdjLeaveOTEntity.STATUS = 0;
                                storeOverTimeList.Add(otherAdjLeaveOTEntity);
                                lefthours = 0;
                                response.LeftHours = 0;
                                response.LeftDays = 0;
                                response.FineHours = response.LeaveTotalHours;
                                response.FineDays = Math.Round(response.LeaveTotalHours / averageWorkPerDay, 2);
                            }
                        }
                        else
                        {
                            //加班记录ID|加班时间
                            T_HR_LEAVEREFEROT otherAdjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                            otherAdjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                            otherAdjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                            otherAdjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();

                            otherAdjLeaveOTEntity.LEAVE_TOTAL_HOURS = 0;
                            otherAdjLeaveOTEntity.LEAVE_TOTAL_DAYS = 0;

                            otherAdjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                            otherAdjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                            otherAdjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                            otherAdjLeaveOTEntity.OVERTIME_RECORDID = string.Empty;
                            otherAdjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                            otherAdjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                            otherAdjLeaveOTEntity.STATUS = 0;
                            storeOverTimeList.Add(otherAdjLeaveOTEntity);
                            lefthours = 0;
                            response.LeftHours = 0;
                            response.LeftDays = 0;
                            response.FineHours = response.LeaveTotalHours;
                            response.FineDays = Math.Round(response.LeaveTotalHours / averageWorkPerDay, 2);
                        }
                    }

                    #endregion

                    //FineType: 1,不扣(带薪假) 2、扣款；3、调休+扣款；4、调休+带薪假抵扣+扣款；
                    //请假时长不够，用年假和加班调休假抵扣
                    if (request.LeaveTypeValue != Enums.LeaveVacationType.AdjustLeaveDay.GetHashCode() && request.LeaveTypeValue != Enums.LeaveVacationType.AnnualDay.GetHashCode())
                    {
                        //申请其他类型假期，如果是不扣款的带薪假也应该往关系表中插入数据
                        switch (strFineType)//请假时，如果年假和调休有剩余时间就可以供其他假期抵扣
                        {
                            #region "  申请其他类型假期     "
                            case "1":
                                #region "   不扣  "
                                var otherDayCount = daycountQueryable.Where(t => t.LEAVETYPESETID == request.LeaveTypeID && t.STATUS == 1).FirstOrDefault();
                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT otherAdjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                otherAdjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                otherAdjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                otherAdjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                otherAdjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                                otherAdjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                otherAdjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                otherAdjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                if (otherDayCount != null)
                                {
                                    otherAdjLeaveOTEntity.OVERTIME_RECORDID = otherDayCount.RECORDID;
                                    otherAdjLeaveOTEntity.EFFECTDATE = otherDayCount.EFFICDATE;
                                    otherAdjLeaveOTEntity.EXPIREDATE = otherDayCount.TERMINATEDATE;
                                }
                                else
                                {
                                    otherAdjLeaveOTEntity.OVERTIME_RECORDID = string.Empty;
                                }
                                otherAdjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                otherAdjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                otherAdjLeaveOTEntity.STATUS = 0;
                                storeOverTimeList.Add(otherAdjLeaveOTEntity);
                                #endregion
                                break;
                            case "2":
                                #region "   扣款  "
                                #endregion
                                break;
                            case "3":
                                #region "   调休+扣款   "
                                //剩余假期
                                response.LeftHours = lefthours + response.AdjustLeaveHours;
                                response.LeftDays = Math.Round(response.LeftHours / averageWorkPerDay, 2);
                                if (response.LeftHours <= response.LeaveTotalHours)
                                {
                                    response.FineDays = Math.Round((response.LeaveTotalHours - response.LeftHours) / response.AvgWorkPerDay, 2);
                                    response.FineHours = response.LeaveTotalHours - response.LeftHours;

                                    if (daycountAdjust.Count > 0)
                                    {
                                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE.Value;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE.Value;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                        }
                                    }
                                    else
                                    {
                                        //加班记录ID|加班时间
                                        T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                        adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                        adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                        adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                        adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                                        adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                        adjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                        adjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                        adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                        adjLeaveOTEntity.OVERTIME_RECORDID = string.Empty;
                                        adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                        adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                        adjLeaveOTEntity.STATUS = 0;
                                        storeOverTimeList.Add(adjLeaveOTEntity);
                                    }
                                }
                                else
                                {
                                    #region "   请假时间小于可用的调休时间   "

                                    response.FineHours = 0;
                                    response.FineDays = 0;

                                    double oweLeaveHours = 0;
                                    double leftLeaveHours = response.LeaveTotalHours;
                                    string strDayCount = string.Empty;
                                    foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in daycountAdjust)
                                    {
                                        //加班记录ID|加班时间
                                        oweLeaveHours = leftLeaveHours - Convert.ToDouble(ent.LEFTHOURS.HasValue ? ent.LEFTHOURS.Value : 0);
                                        //请假时间大于加班时间
                                        if (oweLeaveHours > 0)
                                        {
                                            leftLeaveHours = oweLeaveHours;

                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                            continue;
                                        }
                                        //请假时间还差oweLeaveHours
                                        if (oweLeaveHours <= 0)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(leftLeaveHours);
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                            break;

                                        }
                                    }

                                    #endregion
                                }
                                #endregion
                                break;
                            case "4":
                                #region "   调休+年假+扣款    "
                                bool hasTakeVacation = true;
                                T_HR_EMPLOYEELEVELDAYCOUNT entAnnual = new T_HR_EMPLOYEELEVELDAYCOUNT();
                                if (daycountAnual != null)
                                {
                                    entAnnual = daycountAnual.FirstOrDefault();
                                }
                                //原则上先用加班调休，再用年假，但需要判断加班和年假的有效期，如果年假先到期，则先用年假抵扣
                                List<T_HR_EMPLOYEELEVELDAYCOUNT> tempDaycountAdjust = new List<T_HR_EMPLOYEELEVELDAYCOUNT>();

                                tempDaycountAdjust = daycountAdjust.OrderBy(t => t.TERMINATEDATE).ToList();


                                if (tempDaycountAdjust != null && tempDaycountAdjust.Count() > 0)
                                {
                                    response.AdjustLeaveHours = Convert.ToDouble(tempDaycountAdjust.Sum(t => t.LEFTHOURS));
                                }

                                response.LeftHours = lefthours + response.AdjustLeaveHours + response.AnnualVacationHours;
                                response.LeftDays = Math.Round(response.LeftHours / averageWorkPerDay, 2);
                                if (response.LeftHours <= response.LeaveTotalHours)
                                {
                                    response.FineDays = Math.Round((response.LeaveTotalHours - response.LeftHours) / response.AvgWorkPerDay, 2);
                                    response.FineHours = response.LeaveTotalHours - response.LeftHours;
                                    string strDayCount = string.Empty;
                                    if (tempDaycountAdjust.Count > 0)
                                    {
                                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in tempDaycountAdjust)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE;
                                            adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                        }
                                    }
                                    else
                                    {
                                        hasTakeVacation = false;
                                    }

                                    //T_HR_EMPLOYEELEVELDAYCOUNT entAnnual = daycountAnual.FirstOrDefault();
                                    //记录ID|抵扣时间|生效日期|过期日期|假期类型
                                    if (entAnnual != null)
                                    {
                                        //加班记录ID|加班时间
                                        T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                        adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                        adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                        adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                        adjLeaveOTEntity.LEAVE_TOTAL_HOURS = entAnnual.LEFTHOURS;
                                        adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                        adjLeaveOTEntity.EFFECTDATE = entAnnual.EFFICDATE;
                                        adjLeaveOTEntity.EXPIREDATE = entAnnual.TERMINATEDATE;
                                        adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                        adjLeaveOTEntity.OVERTIME_RECORDID = entAnnual.RECORDID;
                                        adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                        adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                        adjLeaveOTEntity.STATUS = 0;
                                        storeOverTimeList.Add(adjLeaveOTEntity);
                                    }
                                    else
                                    {
                                        hasTakeVacation = false;
                                    }
                                    //如果年假和调休假都没有可用记录，
                                    //则添加一条该类型假期的记录放在关系表中
                                    if (!hasTakeVacation)
                                    {
                                        //加班记录ID|加班时间
                                        T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                        adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                        adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                        adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                        adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                                        adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                        adjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                        adjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                        adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                        adjLeaveOTEntity.OVERTIME_RECORDID = string.Empty;
                                        adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                        adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                        adjLeaveOTEntity.STATUS = 0;
                                        storeOverTimeList.Add(adjLeaveOTEntity);
                                    }
                                }
                                else
                                {
                                    #region "   请假时间小于可用的调休时间   "
                                    string strDayCount = string.Empty;
                                    double oweLeaveForAdjust = 0;
                                    double oweLeaveForAnnual = 0;

                                    response.FineHours = 0;
                                    response.FineDays = 0;

                                    oweLeaveForAdjust = response.LeaveTotalHours - response.AdjustLeaveHours;
                                    oweLeaveForAnnual = response.LeaveTotalHours - response.AnnualVacationHours;
                                    //先将调休假休抵扣，刚好，则用完
                                    if (oweLeaveForAdjust == 0)
                                    {
                                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in tempDaycountAdjust)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                            adjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                        }
                                    }
                                    else if (oweLeaveForAdjust > 0)
                                    {//先将调休假休抵扣，不够，则从年假里扣，避免年假过期了而调休假还未过期
                                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in tempDaycountAdjust)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                            adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntity.EFFECTDATE = DateTime.Now;
                                            adjLeaveOTEntity.EXPIREDATE = DateTime.Now;
                                            adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                            adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntity.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntity);
                                        }

                                        T_HR_EMPLOYEELEVELDAYCOUNT entForOtherVacation = daycountAnual.FirstOrDefault();

                                        if (entForOtherVacation != null)
                                        {
                                            //加班记录ID|加班时间
                                            T_HR_LEAVEREFEROT adjLeaveOTEntityForOther = new T_HR_LEAVEREFEROT();
                                            adjLeaveOTEntityForOther.EMPLOYEEID = request.EmployeeID;
                                            adjLeaveOTEntityForOther.LEAVE_RECORDID = request.LeaveRecordID;
                                            adjLeaveOTEntityForOther.RECORDID = Guid.NewGuid().ToString();
                                            adjLeaveOTEntityForOther.LEAVE_TOTAL_HOURS = Convert.ToDecimal(oweLeaveForAdjust);
                                            adjLeaveOTEntityForOther.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                            adjLeaveOTEntityForOther.EFFECTDATE = entForOtherVacation.EFFICDATE;
                                            adjLeaveOTEntityForOther.EXPIREDATE = entForOtherVacation.TERMINATEDATE;
                                            adjLeaveOTEntityForOther.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                            adjLeaveOTEntityForOther.OVERTIME_RECORDID = entForOtherVacation.RECORDID;
                                            adjLeaveOTEntityForOther.LEAVE_APPLY_DATE = DateTime.Now;
                                            adjLeaveOTEntityForOther.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                            adjLeaveOTEntityForOther.STATUS = 0;
                                            storeOverTimeList.Add(adjLeaveOTEntityForOther);
                                        }
                                    }
                                    else
                                    {
                                        double oweLeaveHours = 0;
                                        double leftLeaveHours = response.LeaveTotalHours;

                                        foreach (T_HR_EMPLOYEELEVELDAYCOUNT ent in tempDaycountAdjust)
                                        {
                                            //加班记录ID|加班时间
                                            oweLeaveHours = leftLeaveHours - Convert.ToDouble(ent.LEFTHOURS.HasValue ? ent.LEFTHOURS.Value : 0);
                                            //请假时间大于加班时间
                                            if (oweLeaveHours > 0)
                                            {
                                                leftLeaveHours = oweLeaveHours;

                                                //加班记录ID|加班时间
                                                T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                                adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                                adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                                adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                                adjLeaveOTEntity.LEAVE_TOTAL_HOURS = ent.LEFTHOURS;
                                                adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                                adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE;
                                                adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE;
                                                adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                                adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                                adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                                adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                                adjLeaveOTEntity.STATUS = 0;
                                                storeOverTimeList.Add(adjLeaveOTEntity);
                                                continue;
                                            }
                                            //请假时间还差oweLeaveHours
                                            if (oweLeaveHours <= 0)
                                            {

                                                //加班记录ID|加班时间
                                                T_HR_LEAVEREFEROT adjLeaveOTEntity = new T_HR_LEAVEREFEROT();
                                                adjLeaveOTEntity.EMPLOYEEID = request.EmployeeID;
                                                adjLeaveOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                                adjLeaveOTEntity.RECORDID = Guid.NewGuid().ToString();
                                                adjLeaveOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(leftLeaveHours);
                                                adjLeaveOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                                adjLeaveOTEntity.EFFECTDATE = ent.EFFICDATE;
                                                adjLeaveOTEntity.EXPIREDATE = ent.TERMINATEDATE;
                                                adjLeaveOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                                adjLeaveOTEntity.OVERTIME_RECORDID = string.IsNullOrEmpty(ent.REMARK) ? ent.RECORDID : ent.REMARK;
                                                adjLeaveOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                                adjLeaveOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                                adjLeaveOTEntity.STATUS = 0;
                                                storeOverTimeList.Add(adjLeaveOTEntity);
                                                break;
                                            }
                                        }
                                    }

                                    #endregion
                                }
                                #endregion
                                break;
                            default:
                                #region "   不扣  "
                                var otherDefaultDayCount = daycountQueryable.Where(t => t.LEAVETYPESETID == request.LeaveTypeID && t.STATUS == 1).FirstOrDefault();
                                //加班记录ID|加班时间
                                T_HR_LEAVEREFEROT otherDefaultRefOTEntity = new T_HR_LEAVEREFEROT();
                                otherDefaultRefOTEntity.EMPLOYEEID = request.EmployeeID;
                                otherDefaultRefOTEntity.LEAVE_RECORDID = request.LeaveRecordID;
                                otherDefaultRefOTEntity.RECORDID = Guid.NewGuid().ToString();
                                otherDefaultRefOTEntity.LEAVE_TOTAL_HOURS = Convert.ToDecimal(response.LeaveTotalHours);
                                otherDefaultRefOTEntity.LEAVE_TOTAL_DAYS = Convert.ToDecimal(response.LeaveTotalDays);
                                otherDefaultRefOTEntity.EFFECTDATE = DateTime.Now;
                                otherDefaultRefOTEntity.EXPIREDATE = DateTime.Now;
                                otherDefaultRefOTEntity.LEAVE_TYPE_SETID = request.LeaveTypeID;
                                if (otherDefaultDayCount != null)
                                {
                                    otherDefaultRefOTEntity.OVERTIME_RECORDID = otherDefaultDayCount.RECORDID;
                                    otherDefaultRefOTEntity.EFFECTDATE = otherDefaultDayCount.EFFICDATE;
                                    otherDefaultRefOTEntity.EXPIREDATE = otherDefaultDayCount.TERMINATEDATE;
                                }
                                else
                                {
                                    otherDefaultRefOTEntity.OVERTIME_RECORDID = string.Empty;
                                }
                                otherDefaultRefOTEntity.LEAVE_APPLY_DATE = DateTime.Now;
                                otherDefaultRefOTEntity.ACTION = Enums.LeaveAction.Leave.GetHashCode();
                                otherDefaultRefOTEntity.STATUS = 0;
                                storeOverTimeList.Add(otherDefaultRefOTEntity);
                                #endregion
                                break;
                            #endregion
                        }
                        response.leaveReferOT = storeOverTimeList;
                    }

                    #endregion
                }

                if (
                    (response.LeftHours <= 0 || response.LeftHours < response.LeaveTotalHours)
                    && request.LeaveTypeValue != Enums.LeaveVacationType.AffairLeaveDay.GetHashCode()
                    && request.LeaveTypeValue != Enums.LeaveVacationType.SickLeaveDay.GetHashCode()
                    )
                {
                    response.Result = Enums.Result.NotEnoughVacationDays.GetHashCode();
                    response.Message = Constants.NotEnoughVacationDays;
                    return response;
                }


                //扣款天数和小时数
                if (response.LeftHours < response.LeaveTotalHours)
                {
                    response.FineDays = Math.Round((response.LeaveTotalHours - response.LeftHours) / response.AvgWorkPerDay, 2);
                    response.FineHours = response.LeaveTotalHours - response.LeftHours;
                }

                T_HR_EMPLOYEELEAVERECORD LeaveRecord = new T_HR_EMPLOYEELEAVERECORD();
                T_HR_LEAVETYPESET leaveTypeSetEntity = new T_HR_LEAVETYPESET();
                leaveTypeSetEntity.LEAVETYPESETID = request.LeaveTypeID;
                leaveTypeSetEntity.LEAVETYPEVALUE = request.LeaveTypeValue.ToString();
                LeaveRecord.T_HR_LEAVETYPESET = leaveTypeSetEntity;
                LeaveRecord.EMPLOYEEID = request.EmployeeID;
                LeaveRecord.STARTDATETIME = Convert.ToDateTime(request.StartDate + " " + request.StartTime);
                LeaveRecord.ENDDATETIME = Convert.ToDateTime(request.EndDate + " " + request.EndTime);

                //是否航信版本
                string strVersion = Utility.GetAppConfigByName("isForHuNanHangXingSalary");
                //int MaxTakeVacationDays = string.IsNullOrEmpty(Utility.GetAppConfigByName("MaxTakeVacationDays")) ? 5 : Convert.ToInt32(Utility.GetAppConfigByName("MaxTakeVacationDays"));
                if (strVersion == "false")
                {
                    #region "   事后提单，必须在下一个工作日的23：59：59内完成    "
                    if (Convert.ToDateTime(request.EndDate).AddDays(1).AddSeconds(-1) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1).AddSeconds(-1))
                    {
                        double days = CheckLeaveEndDate(LeaveRecord);
                        if (Convert.ToDateTime(dtEndDate.ToString("yyyy-MM-dd")).AddDays(days).AddDays(1).AddSeconds(-1) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1).AddSeconds(-1)
                            && (response.LeaveDays > 5 || (response.LeaveDays == 5 && response.LeaveHours > 0)))
                        {
                            response.Result = Enums.Result.IsPastOneDayAndPastMaxdays.GetHashCode();
                            response.Message = string.Format(Constants.IsPastOneDayAndPastMaxdays, "5天(" + Math.Round(5 * averageWorkPerDay, 2).ToString() + "小时)");
                            return response;
                        }
                        else
                        {
                            if (Convert.ToDateTime(dtEndDate.ToString("yyyy-MM-dd")).AddDays(days).AddDays(1).AddSeconds(-1) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(1).AddSeconds(-1))
                            {
                                response.Result = Enums.Result.IsPastOneDay.GetHashCode();
                                response.Message = Constants.IsPastOneDay;
                                return response;
                            }
                        }
                    }
                    else
                    {//假期不能超过最大天数
                        if (response.LeaveDays > iMaxTakeVacationDays || (response.LeaveDays == iMaxTakeVacationDays && response.LeaveHours > 0))
                        {
                            response.Result = Enums.Result.IsPastMaxVacationDays.GetHashCode();
                            response.Message = string.Format(Constants.IsPastMaxVacationDays, iMaxTakeVacationDays.ToString() + "天(" + response.MaxVacationHours.ToString() + "小时)");
                            return response;
                        }
                    }
                    #endregion

                    #region "   三八妇女节   "

                    if (request.LeaveTypeValue == (int)Enums.LeaveVacationType.WomenDay)
                    {
                        DateTime curLeaveDate = Convert.ToDateTime(request.StartDate);
                        DateTime curDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                        DateTime womenDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-03-08"));

                        if (curDate != womenDate)
                        {
                            if (curLeaveDate > dtYouthDayExpiredDate)
                            {
                                response.Result = Enums.Result.WomenDayHasExpried.GetHashCode();
                                response.Message = Constants.WomenDayHasExpried;
                                return response;
                            }
                        }
                    }

                    #endregion

                    #region "   五四青年节是否超龄   "

                    string strBirthDay = string.Empty;
                    if (employee != null)
                    {
                        if (employee.BIRTHDAY.HasValue)
                        {
                            strBirthDay = employee.BIRTHDAY.ToString();
                            //五四青年节
                            if (request.LeaveTypeValue == (int)Enums.LeaveVacationType.YouthLeaveDay)
                            {
                                DateTime curLeaveDate = Convert.ToDateTime(request.StartDate);
                                DateTime curDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                                DateTime youthDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-05-04"));

                                if (curDate != youthDate)
                                {
                                    if (curLeaveDate > dtYouthDayExpiredDate)
                                    {
                                        response.Result = Enums.Result.YouthDayHasExpried.GetHashCode();
                                        response.Message = Constants.YouthDayHasExpried;
                                        return response;
                                    }
                                }

                                if (employee.BIRTHDAY.Value.AddYears(28) <= youthDate)
                                {
                                    response.Result = Enums.Result.IsPastYouthDay.GetHashCode();
                                    response.Message = Constants.IsPastYouthDay;
                                    return response;
                                }
                            }
                        }
                        else
                        {
                            response.Result = Enums.Result.NonBirthday.GetHashCode();
                            response.Message = Constants.NonBirthday;
                            return response;
                        }
                    }
                    #endregion

                    #region "   请假时长必须在1小时（含）以上 "

                    if (response.LeaveTotalHours < 1 && SatSundayList.Count() > 0)
                    {
                        string strSatSunday = string.Empty;
                        SatSundayList.ForEach(t => { strSatSunday += t + ","; });

                        response.Result = Enums.Result.MinLeaveHours.GetHashCode();
                        response.Message = Constants.MinLeaveHours + "。原因：" + strSatSunday.Replace('|', '~').Trim(',') + "时间段是休息日，请更改请假时间。";
                        return response;
                    }
                    else if (response.LeaveTotalHours < 1)
                    {
                        response.Result = Enums.Result.MinLeaveHours.GetHashCode();
                        response.Message = Constants.MinLeaveHours;
                        return response;
                    }
                    #endregion
                }
                else
                {//假期不能超过最大天数
                    if (response.LeaveDays > iMaxTakeVacationDays || (response.LeaveDays == iMaxTakeVacationDays && response.LeaveHours > 0))
                    {
                        response.Result = Enums.Result.IsPastMaxVacationDays.GetHashCode();
                        response.Message = string.Format(Constants.IsPastMaxVacationDays, iMaxTakeVacationDays.ToString() + "天(" + response.MaxVacationHours.ToString() + "小时)");
                        return response;
                    }
                }

                response.LeftHours = Math.Round(response.LeftHours, 2);
                response.LeftDays = Math.Round(response.LeftDays, 2);

                response.LeaveDays = Math.Round(response.LeaveDays, 2);
                response.LeaveHours = Math.Round(response.LeaveHours, 2);

                response.LeaveTotalHours = Math.Round(response.LeaveTotalHours, 2);
                response.LeaveTotalDays = Math.Round(response.LeaveTotalDays, 2);

                response.AdjustLeaveDays = Math.Round(response.AdjustLeaveDays, 2);
                response.AdjustLeaveHours = Math.Round(response.AdjustLeaveHours, 2);

                response.AnnualVacationDays = Math.Round(response.AnnualVacationDays, 2);
                response.AnnualVacationHours = Math.Round(response.AnnualVacationHours, 2);

                response.FineDays = Math.Round(response.FineDays, 2);
                response.FineHours = Math.Round(response.FineHours, 2);

                response.AvgWorkPerDay = Math.Round(response.AvgWorkPerDay, 2);
                response.WorkPerDay = Math.Round(response.WorkPerDay, 2);

                response.MaxVacationDays = Math.Round(response.MaxVacationDays, 2);
                response.MaxVacationHours = Math.Round(response.MaxVacationHours, 2);
                return response;
            }
            catch (Exception ex)
            {
                Tracer.Debug("周文斌写的日志，ex" + ex.Message 
                    + ",source:" + ex.Source 
                    + ",StackTrace:" + ex.StackTrace
                    + ",InnerException" + ex.InnerException);
                response.Message = "出错了，请联系管理员。";
            }
            return response;
        }


        public void UpdateLeftHours(string EmployeeCode, string EmployeeName)
        {
            try
            {
                EmployeeBLL bll = new EmployeeBLL();

                List<T_HR_EMPLOYEE> emplist = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEESTATE == "1" || t.EMPLOYEESTATE == "0" || t.EMPLOYEESTATE == "3").ToList();

                if (string.IsNullOrEmpty(EmployeeCode) && string.IsNullOrEmpty(EmployeeName))
                {
                    //所有员工
                    foreach (var employee in emplist)
                    {
                        EmployeeCode = employee.EMPLOYEECODE;
                        EmployeeName = employee.EMPLOYEECNAME;

                        if (employee.EMPLOYEECODE == "28917362")
                        {
                            string tt = "";
                        }

                        List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                               t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                               && t.VACATIONTYPE == "1").OrderBy("efficdate asc").ToList();

                        List<T_HR_EMPLOYEEOVERTIMERECORD> overtime = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(
                            t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                           && t.CHECKSTATE == "2").OrderBy("startdate asc").ToList();

                        foreach (var item in overtime)
                        {
                            var leaveday = leavedaycount.Where(t => t.EFFICDATE.Value.ToString("yyyy-MM-dd") == item.EFFECTIVEDATE.Value.ToString("yyyy-MM-dd") && t.LEFTHOURS <= 0).FirstOrDefault();

                            if (leaveday != null)
                            {

                                leaveday.LEFTHOURS = item.LEFTHOURS;
                                leaveday.HOURS = item.LEFTHOURS;
                                leaveday.EFFICDATE = item.EFFECTIVEDATE;
                                //leaveday.TERMINATEDATE = item.EXPIREDATE;
                                leavedaycount.Remove(leaveday);
                                dal.Update(leaveday);
                            }
                        }
                    }
                }
                else
                {
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                               t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                               && t.VACATIONTYPE == "1").OrderBy("efficdate asc").ToList();

                    List<T_HR_EMPLOYEEOVERTIMERECORD> overtime = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(
                        t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                       && t.CHECKSTATE == "2").OrderBy("startdate asc").ToList();

                    foreach (var item in overtime)
                    {
                        var leaveday = leavedaycount.Where(t => t.EFFICDATE.Value.ToString("yyyy-MM-dd") == item.EFFECTIVEDATE.Value.ToString("yyyy-MM-dd") && t.LEFTHOURS <= 0).FirstOrDefault();
                        if (leaveday != null)
                        {
                            leaveday.LEFTHOURS = item.LEFTHOURS;
                            leaveday.HOURS = item.LEFTHOURS;
                            leavedaycount.Remove(leaveday);
                            dal.Update(leaveday);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void UpdateOvertimeRecord(string EmployeeCode, string EmployeeName, string Type)
        {
            try
            {
                EmployeeLeaveRecordBLL leaveBll = new EmployeeLeaveRecordBLL();
                OverTimeRecordBLL overtimeBll = new OverTimeRecordBLL();
                EmployeeBLL bll = new EmployeeBLL();

                List<T_HR_EMPLOYEE> emplist = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEESTATE == "1" || t.EMPLOYEESTATE == "0" || t.EMPLOYEESTATE == "3").ToList();

                if (string.IsNullOrEmpty(EmployeeCode) && string.IsNullOrEmpty(EmployeeName))
                {
                    //所有员工
                    foreach (var employee in emplist)
                    {
                        EmployeeCode = employee.EMPLOYEECODE;
                        EmployeeName = employee.EMPLOYEECNAME;


                        List<T_HR_EMPLOYEEOVERTIMERECORD> overtime = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(
                            t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                           && t.CHECKSTATE == "2").OrderBy("startdate asc").ToList();

                        List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                            t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                                && t.CHECKSTATE == "2"
                            ).OrderBy("startdatetime asc").ToList();

                        var templeavecount = from t in leaverecord
                                             join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                             where k.LEAVETYPEVALUE == Type
                                             select t;

                        //销假
                        List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                            .Where(t => t.EMPLOYEECODE == t.EMPLOYEECODE
                                && t.EMPLOYEENAME == EmployeeName
                                && t.CHECKSTATE == "2"
                                ).ToList();

                        decimal leavehours = templeavecount.Sum(t => t.TOTALHOURS).Value;
                        overtime = overtime.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.STARTDATE).ThenBy(t => t.LEFTHOURS).ToList();

                        if (overtime.Count == 0)
                        {
                            continue;
                        }

                        List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                        if (templeavecount != null)
                        {
                            leavelist = templeavecount.ToList();
                        }

                        if (Type == "1")
                        {
                            #region "   建立销假和请假的数据关系    "
                            foreach (var cancel in cancellist)
                            {
                                var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                                if (leave != null)
                                {
                                    leave.TOTALHOURS = (leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0) - (cancel.TOTALHOURS.HasValue ? cancel.TOTALHOURS.Value : 0);
                                    leave.TOTALHOURS = leave.TOTALHOURS > 0 ? leave.TOTALHOURS : 0;
                                    T_HR_EMPLOYEELEAVERECORD entity = leave;
                                    leavelist.Remove(leave);
                                    leavelist.Add(entity);
                                }
                            }
                            #endregion

                            if (leavelist != null)
                            {
                                leavelist = leavelist.OrderBy(t => t.STARTDATETIME).ToList();
                            }

                            #region "   处理请假和加班关系   "
                            foreach (var leave in leavelist)
                            {
                                decimal temp = leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0;
                                while (true)
                                {

                                    DateTime dtTempLeaveStartDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddMonths(-6).ToString("yyyy-MM-dd")) : DateTime.MinValue;
                                    DateTime dtTempLeaveEndDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddDays(1).ToString("yyyy-MM-dd")) : DateTime.MinValue;

                                    var ot = overtime.Where(t => t.LEFTHOURS > 0 && t.EFFECTIVEDATE >= dtTempLeaveStartDate && t.EFFECTIVEDATE < dtTempLeaveEndDate).OrderBy(t => t.EFFECTIVEDATE).ThenBy(t => t.LEFTHOURS).FirstOrDefault();

                                    if (ot != null)
                                    {
                                        if (Convert.ToDateTime(ot.EXPIREDATE.HasValue ? ot.EXPIREDATE.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd"))
                                            < Convert.ToDateTime(leave.STARTDATETIME.HasValue ? leave.STARTDATETIME.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd")).AddMonths(-6))
                                        {
                                            ot.STATUS = 2;//系统自动过期
                                            ot.UPDATEDATE = DateTime.Now;
                                            dal.Update(ot);
                                            overtime.Remove(ot);
                                            continue;
                                        }

                                        if (Convert.ToDateTime(ot.EFFECTIVEDATE.Value.ToString("yyyy-MM-dd")) > Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")))
                                        {
                                            break;
                                        }


                                        if (temp >= (ot.LEFTHOURS.HasValue ? ot.LEFTHOURS.Value : 0) && temp != 0)
                                        {
                                            T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                            referot.OVERTIME_RECORDID = ot.OVERTIMERECORDID;
                                            referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                            referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                            referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                            referot.LEAVE_TOTAL_HOURS = ot.LEFTHOURS.HasValue ? ot.LEFTHOURS.Value : 0;
                                            referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            referot.RECORDID = Guid.NewGuid().ToString();
                                            referot.STATUS = 1;
                                            referot.ACTION = 1;
                                            referot.EFFECTDATE = ot.EFFECTIVEDATE;
                                            referot.EMPLOYEEID = leave.EMPLOYEEID;
                                            referot.EXPIREDATE = ot.EXPIREDATE;
                                            dal.Add(referot);

                                            temp = temp - (ot.LEFTHOURS.HasValue ? ot.LEFTHOURS.Value : 0);
                                            ot.LEFTHOURS = 0;
                                            ot.STATUS = 3;
                                            overtime.Remove(ot);

                                        }
                                        else
                                        {
                                            if (temp != 0)
                                            {
                                                T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                                referot.OVERTIME_RECORDID = ot.OVERTIMERECORDID;
                                                referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                                referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                                referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                                referot.LEAVE_TOTAL_HOURS = temp;
                                                referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                                referot.RECORDID = Guid.NewGuid().ToString();
                                                referot.STATUS = 1;
                                                referot.ACTION = 1;
                                                referot.EFFECTDATE = ot.EFFECTIVEDATE;
                                                referot.EMPLOYEEID = leave.EMPLOYEEID;
                                                referot.EXPIREDATE = ot.EXPIREDATE;

                                                dal.Add(referot);

                                                ot.LEFTHOURS = (ot.LEFTHOURS.HasValue ? ot.LEFTHOURS.Value : 0) - temp;
                                                temp = 0;
                                                T_HR_EMPLOYEEOVERTIMERECORD otEntity = ot;
                                                overtime.Remove(ot);
                                                overtime.Add(otEntity);
                                            }
                                        }

                                        dal.Update(ot);

                                        if (temp == 0)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                else
                {//针对单个员工
                    List<T_HR_EMPLOYEEOVERTIMERECORD> overtime = dal.GetObjects<T_HR_EMPLOYEEOVERTIMERECORD>().Where(
                                               t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                                              && t.CHECKSTATE == "2").OrderBy("startdate asc").ToList();

                    List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                        t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                            && t.CHECKSTATE == "2"
                        ).OrderBy("startdatetime asc").ToList();

                    var templeavecount = from t in leaverecord
                                         join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                         where k.LEAVETYPEVALUE == Type
                                         select t;

                    //销假
                    List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                        .Where(t => t.EMPLOYEECODE == t.EMPLOYEECODE
                            && t.EMPLOYEENAME == EmployeeName
                            && t.CHECKSTATE == "2"
                            ).ToList();

                    decimal leavehours = templeavecount.Sum(t => t.TOTALHOURS).Value;
                    overtime = overtime.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.STARTDATE).ThenBy(t => t.LEFTHOURS).ToList();

                    List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                    if (templeavecount != null)
                    {
                        leavelist = templeavecount.ToList();
                    }

                    if (Type == "1")
                    {
                        #region "   单个员工请假和加班的关系    "
                        foreach (var cancel in cancellist)
                        {
                            var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                            if (leave != null)
                            {
                                leave.TOTALHOURS = (leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0) - (cancel.TOTALHOURS.HasValue ? cancel.TOTALHOURS.Value : 0);
                                leave.TOTALHOURS = leave.TOTALHOURS > 0 ? leave.TOTALHOURS : 0;
                                T_HR_EMPLOYEELEAVERECORD entity = leave;
                                leavelist.Remove(leave);
                                leavelist.Add(entity);
                            }
                        }

                        if (leavelist != null)
                        {
                            leavelist = leavelist.OrderBy(t => t.STARTDATETIME).ToList();
                        }


                        foreach (var leave in leavelist)
                        {
                            decimal temp = leave.TOTALHOURS.Value;
                            while (true)
                            {
                                DateTime dtTempLeaveStartDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddMonths(-6).ToString("yyyy-MM-dd")) : DateTime.MinValue;
                                DateTime dtTempLeaveEndDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddDays(1).ToString("yyyy-MM-dd")) : DateTime.MinValue;

                                var ot = overtime.Where(t => t.LEFTHOURS > 0 && t.EFFECTIVEDATE >= dtTempLeaveStartDate && t.EFFECTIVEDATE < dtTempLeaveEndDate).OrderBy(t => t.EFFECTIVEDATE).ThenBy(t => t.LEFTHOURS).FirstOrDefault();
                                if (ot != null)
                                {
                                    if (Convert.ToDateTime(ot.EXPIREDATE.Value.ToString("yyyy-MM-dd")) < Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")).AddMonths(-6))
                                    {
                                        ot.STATUS = 2;//系统自动过期
                                        ot.UPDATEDATE = DateTime.Now;
                                        dal.Update(ot);
                                        overtime.Remove(ot);
                                        continue;
                                    }

                                    if (Convert.ToDateTime(ot.EFFECTIVEDATE.Value.ToString("yyyy-MM-dd")) > Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")))
                                    {
                                        break;
                                    }


                                    if (temp >= ot.LEFTHOURS.Value && temp != 0)
                                    {
                                        T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                        referot.OVERTIME_RECORDID = ot.OVERTIMERECORDID;
                                        referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                        referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                        referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                        referot.LEAVE_TOTAL_HOURS = ot.LEFTHOURS;
                                        referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                        referot.RECORDID = Guid.NewGuid().ToString();
                                        referot.STATUS = 1;
                                        referot.ACTION = 1;
                                        referot.EFFECTDATE = ot.EFFECTIVEDATE;
                                        referot.EMPLOYEEID = leave.EMPLOYEEID;
                                        referot.EXPIREDATE = ot.EXPIREDATE;
                                        dal.Add(referot);

                                        temp = temp - ot.LEFTHOURS.Value;
                                        ot.LEFTHOURS = 0;
                                        ot.STATUS = 3;
                                        overtime.Remove(ot);
                                    }
                                    else
                                    {
                                        if (temp != 0)
                                        {
                                            T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                            referot.OVERTIME_RECORDID = ot.OVERTIMERECORDID;
                                            referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                            referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                            referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                            referot.LEAVE_TOTAL_HOURS = temp;
                                            referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                            referot.RECORDID = Guid.NewGuid().ToString();
                                            referot.STATUS = 1;
                                            referot.ACTION = 1;
                                            referot.EFFECTDATE = ot.EFFECTIVEDATE;
                                            referot.EMPLOYEEID = leave.EMPLOYEEID;
                                            referot.EXPIREDATE = ot.EXPIREDATE;

                                            dal.Add(referot);

                                            ot.LEFTHOURS = ot.LEFTHOURS - temp;
                                            temp = 0;
                                            T_HR_EMPLOYEEOVERTIMERECORD otEntity = ot;
                                            overtime.Remove(ot);
                                            overtime.Add(otEntity);
                                        }
                                    }

                                    dal.Update(ot);

                                    if (temp == 0)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateEmployeeLeaveDayCount(string EmployeeCode, string EmployeeName, string Type)
        {
            try
            {
                EmployeeLeaveRecordBLL leaveBll = new EmployeeLeaveRecordBLL();
                OverTimeRecordBLL overtimeBll = new OverTimeRecordBLL();
                EmployeeLevelDayCountBLL leaveDayCountBll = new EmployeeLevelDayCountBLL();

                EmployeeBLL bll = new EmployeeBLL();

                List<T_HR_EMPLOYEE> emplist = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEESTATE == "1" || t.EMPLOYEESTATE == "0" || t.EMPLOYEESTATE == "3").ToList();

                if (string.IsNullOrEmpty(EmployeeCode) && string.IsNullOrEmpty(EmployeeName))
                {
                    #region 所有员工
                    //所有员工
                    foreach (var employee in emplist)
                    {
                        EmployeeCode = employee.EMPLOYEECODE;
                        EmployeeName = employee.EMPLOYEECNAME;

                        List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                            t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                            && t.VACATIONTYPE == Type).OrderBy("efficdate asc").ToList();

                        List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                            t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                                && t.CHECKSTATE == "2"
                            ).OrderBy("startdatetime asc").ToList();

                        var templeavecount = from t in leaverecord
                                             join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                             where k.LEAVETYPEVALUE == Type
                                             select t;

                        //销假
                        List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                            .Where(t => t.EMPLOYEECODE == t.EMPLOYEECODE
                                && t.EMPLOYEENAME == EmployeeName
                                && t.CHECKSTATE == "2"
                                ).ToList();

                        if (leavedaycount == null || leavedaycount.Count == 0)
                        {
                            continue;
                        }

                        List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                        if (templeavecount != null)
                        {
                            leavelist = templeavecount.ToList();
                        }

                        leavedaycount = leavedaycount.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).ToList();

                        //病假
                        if (Type == "3")
                        {
                            #region "   处理病假    "
                            foreach (var cancel in cancellist)
                            {
                                var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                                if (leave != null)
                                {
                                    leave.TOTALHOURS = leave.TOTALHOURS - cancel.TOTALHOURS;
                                    T_HR_EMPLOYEELEAVERECORD entity = leave;
                                    leavelist.Remove(leave);
                                    leavelist.Add(entity);
                                }
                            }

                            foreach (var leave in leavelist)
                            {
                                var sick = leavedaycount.Where(t => t.EFFICDATE.Value.Year == leave.STARTDATETIME.Value.Year).FirstOrDefault();
                                if (sick != null)
                                {
                                    T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                    referot.OVERTIME_RECORDID = string.Empty;
                                    referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                    referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                    referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                    referot.LEAVE_TOTAL_HOURS = leave.TOTALHOURS;
                                    referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                    referot.RECORDID = Guid.NewGuid().ToString();
                                    referot.STATUS = 1;
                                    referot.ACTION = 1;
                                    referot.EFFECTDATE = sick.EFFICDATE;
                                    referot.EMPLOYEEID = sick.EMPLOYEEID;
                                    referot.EXPIREDATE = sick.TERMINATEDATE;
                                    dal.Add(referot);

                                    sick.LEFTHOURS = (sick.LEFTHOURS - leave.TOTALHOURS) > 0 ? (sick.LEFTHOURS - leave.TOTALHOURS) : 0;
                                    dal.Update(sick);

                                    //var canncelleaveday = cancellist.Where(t => t.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == leave.LEAVERECORDID).FirstOrDefault();

                                    //if (canncelleaveday != null)
                                    //{
                                    //    sick.LEFTHOURS = sick.LEFTHOURS + canncelleaveday.TOTALHOURS.Value;
                                    //    sick.STATUS = 1;
                                    //    dal.Update(sick);
                                    //}
                                }
                            }
                            #endregion
                        }

                        if (Type == "4")
                        {
                            #region "   处理年假    "

                            foreach (var cancel in cancellist)
                            {
                                var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                                if (leave != null)
                                {
                                    leave.TOTALHOURS = leave.TOTALHOURS - cancel.TOTALHOURS;
                                    T_HR_EMPLOYEELEAVERECORD entity = leave;
                                    leavelist.Remove(leave);
                                    leavelist.Add(entity);
                                }
                            }

                            foreach (var leave in leavelist)
                            {
                                var annual = leavedaycount.Where(t => t.EFFICDATE.Value.Year == leave.STARTDATETIME.Value.Year).FirstOrDefault();
                                if (annual != null)
                                {
                                    T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                    referot.OVERTIME_RECORDID = string.Empty;
                                    referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                    referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                    referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                    referot.LEAVE_TOTAL_HOURS = leave.TOTALHOURS;
                                    referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                    referot.RECORDID = Guid.NewGuid().ToString();
                                    referot.STATUS = 1;
                                    referot.ACTION = 1;
                                    referot.EFFECTDATE = annual.EFFICDATE;
                                    referot.EMPLOYEEID = leave.EMPLOYEEID;
                                    referot.EXPIREDATE = annual.TERMINATEDATE;
                                    dal.Add(referot);

                                    annual.LEFTHOURS = (annual.LEFTHOURS - leave.TOTALHOURS) > 0 ? (annual.LEFTHOURS - leave.TOTALHOURS) : 0;
                                    dal.Update(annual);

                                    //var canncelleaveday = cancellist.Where(t => t.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == leave.LEAVERECORDID).FirstOrDefault();

                                    //if (canncelleaveday != null)
                                    //{
                                    //    annual.LEFTHOURS = annual.LEFTHOURS + canncelleaveday.TOTALHOURS.Value;
                                    //    annual.STATUS = 1;                                        
                                    //    dal.Update(annual);
                                    //}
                                }
                            }
                            #endregion
                        }

                        if (Type == "1")
                        {
                            #region "   处理加班    "
                            foreach (var cancel in cancellist)
                            {
                                var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                                if (leave != null)
                                {
                                    leave.TOTALHOURS = (leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0) - (cancel.TOTALHOURS.HasValue ? cancel.TOTALHOURS.Value : 0);
                                    leave.TOTALHOURS = leave.TOTALHOURS > 0 ? leave.TOTALHOURS : 0;
                                    T_HR_EMPLOYEELEAVERECORD entity = leave;
                                    leavelist.Remove(leave);
                                    leavelist.Add(entity);
                                }
                            }

                            foreach (var leave in leavelist)
                            {
                                decimal temp = leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0;

                                while (true)
                                {
                                    DateTime dtTempLeaveStartDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddMonths(-6).ToString("yyyy-MM-dd")) : DateTime.MinValue;
                                    DateTime dtTempLeaveEndDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddDays(1).ToString("yyyy-MM-dd")) : DateTime.MinValue;

                                    var ot = leavedaycount.Where(t => t.LEFTHOURS > 0 && t.EFFICDATE >= dtTempLeaveStartDate && t.EFFICDATE < dtTempLeaveEndDate).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).FirstOrDefault();
                                    if (ot != null)
                                    {
                                        if (Convert.ToDateTime(ot.TERMINATEDATE.Value.ToString("yyyy-MM-dd")) < Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")).AddMonths(-6))
                                        {
                                            ot.STATUS = 1;//系统自动过期
                                            ot.LEFTHOURS = 0;
                                            ot.UPDATEDATE = DateTime.Now;
                                            dal.Update(ot);
                                            leavedaycount.Remove(ot);
                                            continue;
                                        }

                                        if (Convert.ToDateTime(ot.EFFICDATE.Value.ToString("yyyy-MM-dd")) > Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")))
                                        {
                                            break;
                                        }

                                        if (temp >= ot.LEFTHOURS.Value && temp != 0)
                                        {
                                            temp = temp - ot.LEFTHOURS.Value;
                                            ot.LEFTHOURS = 0;
                                            ot.STATUS = 1;
                                            leavedaycount.Remove(ot);
                                        }
                                        else
                                        {
                                            if (temp != 0)
                                            {

                                                ot.LEFTHOURS = ot.LEFTHOURS - temp;
                                                temp = 0;
                                                T_HR_EMPLOYEELEVELDAYCOUNT otEntity = ot;
                                                leavedaycount.Remove(ot);
                                                leavedaycount.Add(otEntity);
                                            }
                                        }

                                        dal.Update(ot);

                                        if (temp == 0)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    //单个员工
                    //假期
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                                t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                                && t.VACATIONTYPE == Type).OrderBy("efficdate asc").ToList();
                    //请假
                    List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                        t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                            && t.CHECKSTATE == "2"
                        ).OrderBy("startdatetime asc").ToList();
                    //销假
                    List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                        .Where(t => t.EMPLOYEECODE == t.EMPLOYEECODE
                            && t.EMPLOYEENAME == EmployeeName
                            && t.CHECKSTATE == "2"
                            ).ToList();

                    var templeavecount = from t in leaverecord
                                         join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                         where k.LEAVETYPEVALUE == Type
                                         select t;

                    List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                    if (templeavecount != null)
                    {
                        leavelist = templeavecount.ToList();
                    }

                    decimal leavehours = templeavecount.Sum(t => t.TOTALHOURS).Value;
                    leavedaycount = leavedaycount.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).ToList();

                    if (Type == "4")
                    {
                        #region "   处理年假    "
                        foreach (var cancel in cancellist)
                        {
                            var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                            if (leave != null)
                            {
                                leave.TOTALHOURS = leave.TOTALHOURS - cancel.TOTALHOURS;
                                T_HR_EMPLOYEELEAVERECORD entity = leave;
                                leavelist.Remove(leave);
                                leavelist.Add(entity);
                            }
                        }

                        foreach (var leave in leavelist)
                        {
                            var annual = leavedaycount.Where(t => t.EFFICDATE.Value.Year == leave.STARTDATETIME.Value.Year).FirstOrDefault();
                            if (annual != null)
                            {
                                T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                referot.OVERTIME_RECORDID = string.Empty;
                                referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                referot.LEAVE_TOTAL_HOURS = leave.TOTALHOURS;
                                referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                referot.RECORDID = Guid.NewGuid().ToString();
                                referot.STATUS = 1;
                                referot.ACTION = 1;
                                referot.EFFECTDATE = annual.EFFICDATE;
                                referot.EMPLOYEEID = leave.EMPLOYEEID;
                                referot.EXPIREDATE = annual.TERMINATEDATE;
                                dal.Add(referot);

                                annual.LEFTHOURS = (annual.LEFTHOURS - leave.TOTALHOURS) > 0 ? (annual.LEFTHOURS - leave.TOTALHOURS) : 0;
                                dal.Update(annual);
                            }

                            var canncelleaveday = cancellist.Where(t => t.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == leave.LEAVERECORDID).FirstOrDefault();

                            if (canncelleaveday != null)
                            {
                                annual.LEFTHOURS = annual.LEFTHOURS + canncelleaveday.TOTALHOURS.Value;
                                annual.STATUS = 1;
                                dal.Update(annual);
                            }
                        }
                        #endregion
                    }

                    if (Type == "1")
                    {
                        #region "   处理加班    "
                        foreach (var cancel in cancellist)
                        {
                            var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                            if (leave != null)
                            {
                                leave.TOTALHOURS = (leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0) - (cancel.TOTALHOURS.HasValue ? cancel.TOTALHOURS.Value : 0);
                                leave.TOTALHOURS = leave.TOTALHOURS > 0 ? leave.TOTALHOURS : 0;
                                T_HR_EMPLOYEELEAVERECORD entity = leave;
                                leavelist.Remove(leave);
                                leavelist.Add(entity);
                            }
                        }

                        foreach (var leave in leavelist)
                        {
                            decimal temp = leave.TOTALHOURS.HasValue ? leave.TOTALHOURS.Value : 0;

                            while (true)
                            {
                                DateTime dtTempLeaveStartDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddMonths(-6).ToString("yyyy-MM-dd")) : DateTime.MinValue;
                                DateTime dtTempLeaveEndDate = leave.STARTDATETIME.HasValue ? Convert.ToDateTime(leave.STARTDATETIME.Value.AddDays(1).ToString("yyyy-MM-dd")) : DateTime.MinValue;

                                var ot = leavedaycount.Where(t => t.LEFTHOURS > 0 && t.EFFICDATE >= dtTempLeaveStartDate && t.EFFICDATE < dtTempLeaveEndDate).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).FirstOrDefault();

                                if (ot != null)
                                {
                                    if (Convert.ToDateTime(ot.TERMINATEDATE.Value.ToString("yyyy-MM-dd")) < Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")).AddMonths(-6))
                                    {
                                        ot.STATUS = 1;//系统自动过期
                                        ot.LEFTHOURS = 0;
                                        ot.UPDATEDATE = DateTime.Now;
                                        dal.Update(ot);
                                        leavedaycount.Remove(ot);
                                        continue;
                                    }

                                    if (Convert.ToDateTime(ot.EFFICDATE.Value.ToString("yyyy-MM-dd")) > Convert.ToDateTime(leave.STARTDATETIME.Value.ToString("yyyy-MM-dd")))
                                    {
                                        break;
                                    }

                                    if (temp >= ot.LEFTHOURS.Value && temp != 0)
                                    {
                                        temp = temp - ot.LEFTHOURS.Value;
                                        ot.LEFTHOURS = 0;
                                        ot.STATUS = 1;
                                        leavedaycount.Remove(ot);
                                    }
                                    else
                                    {
                                        if (temp != 0)
                                        {

                                            ot.LEFTHOURS = ot.LEFTHOURS - temp;
                                            temp = 0;
                                            T_HR_EMPLOYEELEVELDAYCOUNT otEntity = ot;
                                            leavedaycount.Remove(ot);
                                            leavedaycount.Add(otEntity);
                                        }
                                    }

                                    dal.Update(ot);

                                    if (temp == 0)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }


                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateAnnualDay(string EmployeeCode, string EmployeeName, string Type)
        {
            try
            {
                EmployeeLeaveRecordBLL leaveBll = new EmployeeLeaveRecordBLL();
                OverTimeRecordBLL overtimeBll = new OverTimeRecordBLL();
                EmployeeLevelDayCountBLL leaveDayCountBll = new EmployeeLevelDayCountBLL();
                string EmployeeID = string.Empty;
                EmployeeBLL bll = new EmployeeBLL();

                List<T_HR_EMPLOYEE> emplist = dal.GetObjects<T_HR_EMPLOYEE>().Where(t => t.EMPLOYEESTATE == "1" || t.EMPLOYEESTATE == "3").ToList();

                if (string.IsNullOrEmpty(EmployeeCode) && string.IsNullOrEmpty(EmployeeName))
                {
                    #region 所有员工
                    //所有员工
                    foreach (var employee in emplist)
                    {
                        EmployeeID = employee.EMPLOYEEID;
                        EmployeeCode = employee.EMPLOYEECODE;
                        EmployeeName = employee.EMPLOYEECNAME;

                        List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                            t => t.EMPLOYEEID == employee.EMPLOYEEID && t.VACATIONTYPE == Type
                            && t.EFFICDATE >= Convert.ToDateTime("2014-01-01") && t.TERMINATEDATE <= Convert.ToDateTime("2015-04-15")
                            ).OrderBy("efficdate asc").ToList();

                        List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                            t => t.EMPLOYEEID == employee.EMPLOYEEID
                                && t.CHECKSTATE == "2"
                            ).OrderBy("startdatetime asc").ToList();

                        var templeavecount = from t in leaverecord
                                             join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                             where k.LEAVETYPEVALUE == Type
                                             select t;

                        //销假
                        List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                            .Where(t => t.EMPLOYEEID == employee.EMPLOYEEID
                                && t.CHECKSTATE == "2"
                                ).ToList();



                        if (leavedaycount == null || leavedaycount.Count == 0)
                        {
                            continue;
                        }

                        List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                        if (templeavecount != null)
                        {
                            leavelist = templeavecount.ToList();
                        }

                        leavedaycount = leavedaycount.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).ToList();

                        if (Type == "4")
                        {
                            #region "   处理年假    "

                            foreach (var cancel in cancellist)
                            {
                                var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                                if (leave != null)
                                {
                                    leave.TOTALHOURS = leave.TOTALHOURS - cancel.TOTALHOURS;
                                    T_HR_EMPLOYEELEAVERECORD entity = leave;
                                    leavelist.Remove(leave);
                                    leavelist.Add(entity);
                                }
                            }

                            foreach (var leave in leavelist)
                            {
                                var annual = leavedaycount.Where(t => t.EFFICDATE.Value.Year == leave.STARTDATETIME.Value.Year).FirstOrDefault();
                                if (annual != null)
                                {
                                    T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                    referot.OVERTIME_RECORDID = string.Empty;
                                    referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                    referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                    referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                    referot.LEAVE_TOTAL_HOURS = leave.TOTALHOURS;
                                    referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                    referot.RECORDID = Guid.NewGuid().ToString();
                                    referot.STATUS = 1;
                                    referot.ACTION = 1;
                                    referot.EFFECTDATE = annual.EFFICDATE;
                                    referot.EMPLOYEEID = leave.EMPLOYEEID;
                                    referot.EXPIREDATE = annual.TERMINATEDATE;
                                    dal.Add(referot);

                                    annual.LEFTHOURS = (annual.LEFTHOURS - leave.TOTALHOURS) > 0 ? (annual.LEFTHOURS - leave.TOTALHOURS) : 0;
                                    dal.Update(annual);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    //单个员工
                    //假期
                    List<T_HR_EMPLOYEELEVELDAYCOUNT> leavedaycount = dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>().Where(
                                t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                                && t.VACATIONTYPE == Type).OrderBy("efficdate asc").ToList();
                    //请假
                    List<T_HR_EMPLOYEELEAVERECORD> leaverecord = dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>().Include("T_HR_LEAVETYPESET").Where(
                        t => t.EMPLOYEENAME == EmployeeName && t.EMPLOYEECODE == EmployeeCode
                            && t.CHECKSTATE == "2"
                        ).OrderBy("startdatetime asc").ToList();
                    //销假
                    List<T_HR_EMPLOYEECANCELLEAVE> cancellist = dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>().Include("T_HR_EMPLOYEELEAVERECORD")
                        .Where(t => t.EMPLOYEECODE == t.EMPLOYEECODE
                            && t.EMPLOYEENAME == EmployeeName
                            && t.CHECKSTATE == "2"
                            ).ToList();

                    var templeavecount = from t in leaverecord
                                         join k in dal.GetObjects<T_HR_LEAVETYPESET>() on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals k.LEAVETYPESETID
                                         where k.LEAVETYPEVALUE == Type
                                         select t;

                    List<T_HR_EMPLOYEELEAVERECORD> leavelist = new List<T_HR_EMPLOYEELEAVERECORD>();
                    if (templeavecount != null)
                    {
                        leavelist = templeavecount.ToList();
                    }

                    decimal leavehours = templeavecount.Sum(t => t.TOTALHOURS).Value;
                    leavedaycount = leavedaycount.Where(t => t.LEFTHOURS > 0).OrderBy(t => t.EFFICDATE).ThenBy(t => t.LEFTHOURS).ToList();

                    if (Type == "4")
                    {
                        #region "   处理年假    "
                        foreach (var cancel in cancellist)
                        {
                            var leave = leavelist.Where(t => t.LEAVERECORDID == cancel.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID).FirstOrDefault();
                            if (leave != null)
                            {
                                leave.TOTALHOURS = leave.TOTALHOURS - cancel.TOTALHOURS;
                                T_HR_EMPLOYEELEAVERECORD entity = leave;
                                leavelist.Remove(leave);
                                leavelist.Add(entity);
                            }
                        }

                        foreach (var leave in leavelist)
                        {
                            var annual = leavedaycount.Where(t => t.EFFICDATE.Value.Year == leave.STARTDATETIME.Value.Year).FirstOrDefault();
                            if (annual != null)
                            {
                                T_HR_LEAVEREFEROT referot = new T_HR_LEAVEREFEROT();
                                referot.OVERTIME_RECORDID = string.Empty;
                                referot.LEAVE_RECORDID = leave.LEAVERECORDID;
                                referot.LEAVE_APPLY_DATE = leave.CREATEDATE;
                                referot.LEAVE_TOTAL_DAYS = leave.LEAVEDAYS;
                                referot.LEAVE_TOTAL_HOURS = leave.TOTALHOURS;
                                referot.LEAVE_TYPE_SETID = leave.T_HR_LEAVETYPESET.LEAVETYPESETID;
                                referot.RECORDID = Guid.NewGuid().ToString();
                                referot.STATUS = 1;
                                referot.ACTION = 1;
                                referot.EFFECTDATE = annual.EFFICDATE;
                                referot.EMPLOYEEID = leave.EMPLOYEEID;
                                referot.EXPIREDATE = annual.TERMINATEDATE;
                                dal.Add(referot);

                                annual.LEFTHOURS = (annual.LEFTHOURS - leave.TOTALHOURS) > 0 ? (annual.LEFTHOURS - leave.TOTALHOURS) : 0;
                                dal.Update(annual);
                            }

                            var canncelleaveday = cancellist.Where(t => t.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID == leave.LEAVERECORDID).FirstOrDefault();

                            if (canncelleaveday != null)
                            {
                                annual.LEFTHOURS = annual.LEFTHOURS + canncelleaveday.TOTALHOURS.Value;
                                annual.STATUS = 1;
                                dal.Update(annual);
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 销假时获取相应的请假信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_EmpLeaveRdInfo> EmployeeLeaveRecordToCalcelLeave(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                //SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEELEAVERECORD");

                IQueryable<T_HR_EMPLOYEELEAVERECORD> ents = dal.GetObjects().Include("T_HR_LEAVETYPESET").Include("T_HR_EMPLOYEECANCELLEAVE");

                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                DateTime? date = bll.GetLastSalaryDateByEemployeeID(userID, "2");
                if (date != null)
                {
                    ents = ents.Where(t => t.ENDDATETIME > date);
                }
                else
                {
                    DateTime dt = DateTime.Now;
                    date = System.Convert.ToDateTime(dt.Year.ToString() + "-" + dt.AddMonths(-1).Month.ToString() + "-01");
                    ents = ents.Where(t => t.ENDDATETIME > date);
                }


                var dv = from a in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                         where a.EMPLOYEEID == userID && (a.CHECKSTATE == "2" || a.CHECKSTATE == "1")
                         group a by a.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID into g
                         select new
                         {
                             id = g.Key,
                             totalHours = g.Sum(p => p.TOTALHOURS)
                         };
                var ex = from e in dal.GetObjects<T_HR_EMPLOYEECANCELLEAVE>()
                         join c in dv on e.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID equals c.id
                         where e.T_HR_EMPLOYEELEAVERECORD.TOTALHOURS <= c.totalHours
                         select e.T_HR_EMPLOYEELEAVERECORD;
                ents = ents.Except(ex);


                //ents = Utility.Pager<T_HR_EMPLOYEELEAVERECORD>(ents, pageIndex, pageSize, ref pageCount);
                var entrs = from e in ents
                            join vDepartment in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals vDepartment.DEPARTMENTID
                            select new V_EmpLeaveRdInfo
                            {
                                CHECKSTATE = e.CHECKSTATE,
                                OWNERCOMPANYID = e.OWNERCOMPANYID,
                                OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                                //DEPARTMENTNAME = vDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                OWNERPOSTID = e.OWNERPOSTID,
                                OWNERID = e.OWNERID,
                                EMPLOYEEID = e.EMPLOYEEID,
                                EMPLOYEECODE = e.EMPLOYEECODE,
                                EMPLOYEENAME = e.EMPLOYEENAME,
                                LEAVEDAYS = e.LEAVEDAYS,
                                LEAVEHOURS = e.LEAVEHOURS,
                                TOTALHOURS = e.TOTALHOURS,
                                STARTDATETIME = e.STARTDATETIME,
                                ENDDATETIME = e.ENDDATETIME,
                                LEAVERECORDID = e.LEAVERECORDID,
                                LEAVETYPEVALUE = e.T_HR_LEAVETYPESET.LEAVETYPEVALUE,
                                LEAVETYPEID = e.T_HR_LEAVETYPESET.LEAVETYPESETID,
                                LEAVETYPENAME = e.T_HR_LEAVETYPESET.LEAVETYPENAME,
                                CREATECOMPANYID = e.CREATECOMPANYID,
                                CREATEDEPARTMENTID = e.CREATEDEPARTMENTID,
                                CREATEPOSTID = e.CREATEPOSTID,
                                CREATEUSERID = e.CREATEUSERID,
                                CREATEDATE = e.CREATEDATE,
                                UPDATEDATE = e.UPDATEDATE,
                                //CANCELTOTALHOURS = e.T_HR_EMPLOYEECANCELLEAVE.Where(t => t.CHECKSTATE == "2").Sum(t => t.TOTALHOURS),
                                REASON = e.REASON
                            };
                //entrs = entrs.OrderBy(sort);
                entrs = from ent in entrs.ToList().AsQueryable()
                        orderby ent.STARTDATETIME descending
                        select ent;
                return entrs;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工销假时获取请假信息错误：EmployeeLeaveRecordToCalcelLeave：" + ex.ToString());
                return null;
            }
        }


        #endregion
    }
}
