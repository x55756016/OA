
/*
 * 文件名：AttendMonthlyBalanceBLL.cs
 * 作  用：员工考勤月度结算 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-29 9:11:18
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

namespace SMT.HRM.BLL
{
    public class AttendMonthlyBalanceBLL : BaseBll<T_HR_ATTENDMONTHLYBALANCE>
    {
        public AttendMonthlyBalanceBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取员工考勤月度结算信息
        /// </summary>
        /// <param name="strMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        public T_HR_ATTENDMONTHLYBALANCE GetAttendMonthlyBalanceByID(string strMonthlyBalanceId)
        {
            if (string.IsNullOrEmpty(strMonthlyBalanceId))
            {
                return null;
            }

            AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strMonthlyBalanceId))
            {
                strfilter.Append(" MONTHLYBALANCEID == @0");
                objArgs.Add(strMonthlyBalanceId);
            }

            T_HR_ATTENDMONTHLYBALANCE entRd = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取要进行审批的员工考勤月度结算信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAllAttendMonthlyBalanceRdListForAudit(string sType, string sValue, string strOwnerID,
            string strCheckState, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey)
        {
            AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();

            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " MONTHLYBALANCEID ";
            }

            string filterString = string.Empty;

            if (strCheckState == Convert.ToInt32(Common.CheckStates.WaittingApproval).ToString())
            {
                strCheckState = Convert.ToInt32(Common.CheckStates.Approving).ToString();
            }
            //去掉权限过滤
            //else
            //{
            //    SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDMONTHLYBALANCE");
            //}

            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                if (strCheckState == Convert.ToInt32(Common.CheckStates.All).ToString())
                {
                    strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                    filterString += " CHECKSTATE != @" + iIndex.ToString();
                    objArgs.Add(strCheckState);
                }
                else
                {
                    filterString += " CHECKSTATE == @" + iIndex.ToString();
                    objArgs.Add(strCheckState);
                }
            }

            var q = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOrderBy, dBalanceYear, dBalanceMonth, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取要进行审批的员工考勤月度结算信息,并进行分页
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListForAudit(string sType, string sValue, string strOwnerID, string strCheckState,
            decimal dBalanceYear, decimal dBalanceMonth, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendMonthlyBalanceRdListForAudit(sType, sValue, strOwnerID, strCheckState, dBalanceYear, dBalanceMonth, strSortKey);

            if (pageIndex == 0 && pageSize == 0)
            {
                return q;
            }

            return Utility.Pager<T_HR_ATTENDMONTHLYBALANCE>(q, pageIndex, pageSize, ref pageCount);
        }

        /// <summary>
        /// 根据条件，获取员工考勤月度结算信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAllAttendMonthlyBalanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strCheckState, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey)
        {
            AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            string filterString = strfilter.ToString();

            if (strCheckState == Convert.ToInt32(Common.CheckStates.All).ToString())
            {
                strCheckState = string.Empty;
            }

            if (strCheckState == Convert.ToInt32(Common.CheckStates.WaittingApproval).ToString())
            {
                strCheckState = Convert.ToInt32(Common.CheckStates.Approving).ToString();
                string strTempFilter = string.Empty;
                List<object> objTempArgs = new List<object>();
                SetFilterWithflow("MONTHLYBALANCEID", "T_HR_ATTENDMONTHLYBALANCE", strOwnerID, ref strCheckState, ref strTempFilter, ref objTempArgs);

                var ents = from v in dal.GetObjects().Include("T_HR_ATTENDMONTHLYBATCHBALANCE")
                           select v;

                if (!string.IsNullOrWhiteSpace(strTempFilter) && objTempArgs.Count > 0)
                {
                    ents = ents.Where(strTempFilter, objTempArgs.ToArray());

                    if (ents.Count() > 0)
                    {
                        string strIds = string.Empty;
                        foreach (T_HR_ATTENDMONTHLYBALANCE entTarget in ents)
                        {
                            if (strIds.Contains(entTarget.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID) == false)
                            {
                                strIds += entTarget.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID + ",";
                            }
                        }

                        if (!string.IsNullOrEmpty(strIds.ToString()))
                        {
                            if (!string.IsNullOrEmpty(filterString))
                            {
                                filterString += " AND";
                            }
                            if (objArgs.Count() > 0)
                            {
                                iIndex = objArgs.Count();
                            }
                            filterString += " T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID" + ".Contains(@" + iIndex.ToString() + ")";
                            objArgs.Add(strIds.ToString());
                        }
                    }
                }
            }
            else
            {
                SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDMONTHLYBALANCE");
            }


            if (!string.IsNullOrEmpty(strCheckState))
            {
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString = "(" + filterString + ") AND";
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                objArgs.Add(strCheckState);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " MONTHLYBALANCEID ";
            }

            var q = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOrderBy, dBalanceYear, dBalanceMonth, filterString, objArgs.ToArray());
            return q;
        }



        /// <summary>
        /// 根据条件，获取员工考勤月度结算信息
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns>员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> ExportAttendMonthlyBalanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strCheckState, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey)
        {
            IQueryable<T_HR_ATTENDMONTHLYBALANCE> Ents = null;
            try
            {
                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                StringBuilder strfilter = new StringBuilder();
                List<object> objArgs = new List<object>();
                string strOrderBy = string.Empty;
                int iIndex = 0;

                if (!string.IsNullOrEmpty(strEmployeeID))
                {
                    strfilter.Append(" EMPLOYEEID == @0");
                    objArgs.Add(strEmployeeID);
                }

                string filterString = strfilter.ToString();

                if (strCheckState == Convert.ToInt32(Common.CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                if (strCheckState == Convert.ToInt32(Common.CheckStates.WaittingApproval).ToString())
                {
                    strCheckState = Convert.ToInt32(Common.CheckStates.Approving).ToString();
                    string strTempFilter = string.Empty;
                    List<object> objTempArgs = new List<object>();
                    SetFilterWithflow("MONTHLYBALANCEID", "T_HR_ATTENDMONTHLYBALANCE", strOwnerID, ref strCheckState, ref strTempFilter, ref objTempArgs);

                    var ents = from v in dal.GetObjects().Include("T_HR_ATTENDMONTHLYBATCHBALANCE")
                               select v;

                    if (!string.IsNullOrWhiteSpace(strTempFilter) && objTempArgs.Count > 0)
                    {
                        ents = ents.Where(strTempFilter, objTempArgs.ToArray());

                        if (ents.Count() > 0)
                        {
                            string strIds = string.Empty;
                            foreach (T_HR_ATTENDMONTHLYBALANCE entTarget in ents)
                            {
                                if (strIds.Contains(entTarget.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID) == false)
                                {
                                    strIds += entTarget.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID + ",";
                                }
                            }

                            if (!string.IsNullOrEmpty(strIds.ToString()))
                            {
                                if (!string.IsNullOrEmpty(filterString))
                                {
                                    filterString += " AND";
                                }
                                if (objArgs.Count() > 0)
                                {
                                    iIndex = objArgs.Count();
                                }
                                filterString += " T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID" + ".Contains(@" + iIndex.ToString() + ")";
                                objArgs.Add(strIds.ToString());
                            }
                        }
                    }
                }
                else
                {
                    SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_ATTENDMONTHLYBALANCE");
                }


                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString = "(" + filterString + ") AND";
                    }

                    if (objArgs.Count() > 0)
                    {
                        iIndex = objArgs.Count();
                    }

                    filterString += " CHECKSTATE == @" + iIndex.ToString();
                    objArgs.Add(strCheckState);
                }

                if (!string.IsNullOrEmpty(strSortKey))
                {
                    strOrderBy = strSortKey;
                }
                else
                {
                    strOrderBy = " MONTHLYBALANCEID ";
                }

                var q = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOrderBy, dBalanceYear, dBalanceMonth, filterString, objArgs.ToArray());
                List<V_EmployeeAttenceReports> ListReports = new List<V_EmployeeAttenceReports>();
                if (q.Count() > 0)
                {
                    //q.ToList().ForEach(item =>
                    //{
                    //    V_EmployeeAttenceReports Report = new V_EmployeeAttenceReports();
                    //    Report.EMPLOYEEID = item.EMPLOYEEID;
                    //    Report.ABSENTDAYS = item.ABSENTDAYS;
                    //    Report.AFFAIRLEAVEDAYS = item.AFFAIRLEAVEDAYS;
                    //    Report.ANNUALLEVELDAYS = item.ANNUALLEVELDAYS;
                    //    Report.ATTENDANCERATE = item.NEEDATTENDDAYS;
                    //    Report.CHECKSTATE = item.CHECKSTATE;
                    //    Report.EMPLOYEEID = item.EMPLOYEEID;
                    //    Report.ENTRYDATE = null;
                    //    Report.EMPLOYEECNAME = "";
                    //    Report.EVECTIONTIME = item.EVECTIONTIME;
                    //    Report.ATTENDANCERATE = item.REALATTENDDAYS / item.NEEDATTENDDAYS;//出勤率
                    //    Report.FORGETCARDTIMES = item.FORGETCARDTIMES;
                    //    Report.FUNERALLEAVEDAYS = item.FUNERALLEAVEDAYS;
                    //    Report.IDNUMBER = "";
                    //    Report.INJURYLEAVEDAYS = item.INJURYLEAVEDAYS;
                    //    Report.LATETIMES = item.LATETIMES;
                    //    Report.LEAVEEARLYTIMES = item.LEAVEEARLYTIMES;
                    //    Report.LEAVEUSEDDAYS = item.LEAVEUSEDDAYS;
                    //    Report.MARRYDAYS = item.MARRYDAYS;
                    //    Report.MATERNITYLEAVEDAYS = item.MATERNITYLEAVEDAYS;
                    //    Report.NEEDATTENDDAYS = item.NEEDATTENDDAYS;
                    //    Report.NURSESDAYS = item.NURSESDAYS;
                    //    Report.ORGANIZENAME = "";
                    //    Report.OVERTIMESUMDAYS = item.OVERTIMESUMDAYS;
                    //    Report.REALATTENDDAYS = item.REALATTENDDAYS;
                    //    Report.SICKLEAVEDAYS = item.SICKLEAVEDAYS;
                    //    var employeetmp = from a in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST").Include("T_HR_POST.T_HR_POSTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY").Include("T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY")
                    //                      where a.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID
                    //                      && a.T_HR_POST.POSTID == item.OWNERPOSTID
                    //                      && a.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == item.OWNERCOMPANYID
                    //                      && a.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID == item.OWNERDEPARTMENTID
                    //                      select a;
                    //    if (employeetmp.Count() > 0)
                    //    {
                    //        Report.EMPLOYEECNAME = employeetmp.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME;
                    //        Report.ORGANIZENAME = employeetmp.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME + "/" + employeetmp.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //        Report.IDNUMBER = employeetmp.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME;

                    //    }
                    //    var EntEry = from a in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                    //                 where a.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID
                    //                 select a;
                    //    if (EntEry.Count() > 0)
                    //    {
                    //        Report.ENTRYDATE = EntEry.FirstOrDefault().ENTRYDATE;
                    //    }
                    //    ListReports.Add(Report);
                    //});

                    //if (ListReports.Count() > 0)
                    //{
                    //    Ents = ListReports.AsQueryable();
                    //    Ents = Ents.OrderBy(strSortKey);
                    //}
                    Ents = q;

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("导出考勤备案出现错误-ExportAttendMonthlyBalanceRdListByMultSearch" + ex.ToString());
            }
            return Ents;
        }

        /// <summary>
        /// 根据条件，获取员工考勤月度结算信息,并进行分页
        /// </summary>
        /// <param name="sType">查看的员工范围对象类型</param>
        /// <param name="sValue">查看的员工范围对象的ID</param>
        /// <param name="strOwnerID">权限控制所有人的员工序号</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strEmployeeID">员工序号</param>
        /// <param name="dBalanceYear">结算年份</param>
        /// <param name="dBalanceMonth">结算月份</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工考勤月度结算信息</returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListByMultSearch(string sType, string sValue, string strOwnerID, string strCheckState,
            string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey);

            return Utility.Pager<T_HR_ATTENDMONTHLYBALANCE>(q, pageIndex, pageSize, ref pageCount);
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="dYear"></param>
        /// <returns></returns>
        public IQueryable<T_HR_ATTENDMONTHLYBALANCE> GetAttendMonthlyBalanceRdListByEmployeeAndYear(string strEmployeeID, decimal dBalanceYear, string strCheckState)
        {
            var q = from m in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>()
                    where m.EMPLOYEEID == strEmployeeID && m.BALANCEYEAR == dBalanceYear && m.CHECKSTATE == strCheckState
                    select m;
            return q;
        }

        #endregion

        #region 操作

        /// <summary>
        /// 批量导入员工考勤结算数据
        /// </summary>
        /// <param name="strPhysicalPath">导入文件的物理地址</param>
        /// <param name="strUnitType">导入对象的类型</param>
        /// <param name="strUnitObjectId">导入对象的ID</param>
        /// <param name="dBalanceYear">考勤结算年份</param>
        /// <param name="dBalanceMonth">考勤结算月份</param>
        /// <param name="strMsg">导入完成后的消息</param>
        public void ImportMonthlyBalance(string strCreateUserID, string strPhysicalPath, string strUnitType, string strUnitObjectId, decimal dBalanceYear, decimal dBalanceMonth, ref string strMsg)
        {
            try
            {
                List<T_HR_EMPLOYEE> entEmployees = new List<T_HR_EMPLOYEE>();
                CheckImportFilters(strPhysicalPath, strUnitType, strUnitObjectId, dBalanceYear, dBalanceMonth, ref strMsg, ref entEmployees);

                if (!string.IsNullOrWhiteSpace(strMsg))
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                T_HR_EMPLOYEE entCreateUser = bllEmployee.GetEmployeeByID(strCreateUserID);
                if (entCreateUser == null)
                {
                    strMsg = "{REQUIREDFIELDS}";
                    return;
                }

                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();

                DateTime.TryParse(dBalanceYear.ToString() + "-" + dBalanceMonth.ToString() + "-1", out dtStart);
                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                AttendanceSolutionAsignBLL bllAttSolAsign = new AttendanceSolutionAsignBLL();

                Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPhysicalPath, Encoding.GetEncoding("GB2312"));

                TF.Delimiters = new string[] { "," }; //设置分隔符
                string[] strLine;
                while (!TF.EndOfData)
                {
                    try
                    {
                        strLine = TF.ReadFields();
                        string strFingerPrintId = strLine[0];
                        Tracer.Debug("开始导入员工考勤月度结算结果，员工姓名：" + strLine[1] + " 员工指纹编码:" + strLine[0]);
                        if (strLine.Length < 23)
                        {
                            Tracer.Debug("导入的数据少于23列，请检查导入模板，确保最后一列不为空");
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(strFingerPrintId))
                        {
                            Utility.SaveLog("执行ImportMonthlyBalance函数，检查第一列指纹编号的值发现为空。导入对象的类型为："
                                + strUnitType + ", 导入对象的ID：" + strUnitObjectId + "导入行:" +System.Environment.NewLine+ strLine);
                            continue;
                        }

                        T_HR_EMPLOYEE entCurrEmp = entEmployees.FirstOrDefault(t => t.FINGERPRINTID == strFingerPrintId);
                        if (entCurrEmp == null)
                        {
                            Utility.SaveLog("执行ImportMonthlyBalance函数，检查员工发现不存在。导入对象的类型为："
                                + strUnitType + ", 导入对象的ID：" + strUnitObjectId + ",导入的员工指纹编号为：" + strUnitObjectId + "导入行:" + System.Environment.NewLine + strLine);
                            continue;
                        }

                        DeleteMonthlyBalance(entCurrEmp.OWNERCOMPANYID, entCurrEmp.EMPLOYEEID, dBalanceYear, dBalanceMonth);

                        T_HR_ATTENDANCESOLUTIONASIGN entAttSolAsign = bllAttSolAsign.GetAttSolAsignByEmployeeIDAndDate(entCurrEmp.EMPLOYEEID, dtStart, dtEnd);

                        if (entAttSolAsign == null)
                        {
                            Utility.SaveLog("执行ImportMonthlyBalance函数，检查员工所分配的考勤方案不存在。导入对象的类型为："
                                + strUnitType + ", 导入对象的ID：" + strUnitObjectId + ",导入的员工指纹编号为：" + strFingerPrintId
                                + ",导入的年份：" + dBalanceYear.ToString() + ",导入的月份：" + dBalanceMonth.ToString() + strUnitObjectId + "导入行:" + System.Environment.NewLine + strLine);
                            continue;
                        }

                        T_HR_ATTENDANCESOLUTION entAttSol = entAttSolAsign.T_HR_ATTENDANCESOLUTION;
                        if (entAttSol == null)
                        {
                            Utility.SaveLog("执行ImportMonthlyBalance函数，检查员工所分配的考勤方案不存在。导入对象的类型为："
                                + strUnitType + ", 导入对象的ID：" + strUnitObjectId + ",导入的员工指纹编号为：" + strFingerPrintId
                                + ",导入的年份：" + dBalanceYear.ToString() + ",导入的月份：" + dBalanceMonth.ToString() + strUnitObjectId + "导入行:" + System.Environment.NewLine + strLine);
                            continue;
                        }

                        decimal dWorkServiceMonths = 0; //在职总月份数

                        T_HR_ATTENDMONTHLYBALANCE entTemp = new T_HR_ATTENDMONTHLYBALANCE();
                        entTemp.MONTHLYBALANCEID = System.Guid.NewGuid().ToString().ToUpper();
                        entTemp.BALANCEDATE = DateTime.Now;
                        entTemp.BALANCEYEAR = dBalanceYear;
                        entTemp.BALANCEMONTH = dBalanceMonth;
                        entTemp.CHECKSTATE = Convert.ToInt32(Common.CheckStates.UnSubmit).ToString();
                        entTemp.EDITSTATE = Convert.ToInt32(Common.EditStates.UnActived).ToString();

                        entTemp.EMPLOYEEID = entCurrEmp.EMPLOYEEID;
                        entTemp.EMPLOYEECODE = entCurrEmp.EMPLOYEECODE;
                        entTemp.EMPLOYEENAME = entCurrEmp.EMPLOYEECNAME;

                        entTemp.NEEDATTENDDAYS = GetNeedAttendDays(dtStart, dtEnd, entCurrEmp.OWNERCOMPANYID, entAttSol);
                        entTemp.REALNEEDATTENDDAYS = GetDecimalValue(strLine[2]);
                        entTemp.REALATTENDDAYS = GetDecimalValue(strLine[3]);

                        GetEmployeeWorkServiceMonths(entCurrEmp.EMPLOYEEID, ref dWorkServiceMonths);
                        entTemp.WORKSERVICEMONTHS = dWorkServiceMonths;

                        entTemp.FORGETCARDTIMES = GetDecimalValue(strLine[4]);
                        entTemp.LATETIMES = GetDecimalValue(strLine[5]);
                        entTemp.LEAVEEARLYTIMES = GetDecimalValue(strLine[6]);

                        entTemp.LATEDAYS = GetDecimalValue(strLine[5]);
                        entTemp.LEAVEEARLYDAYS = GetDecimalValue(strLine[6]);
                        entTemp.ABSENTDAYS = GetDecimalValue(strLine[7]);

                        entTemp.AFFAIRLEAVEDAYS = GetDecimalValue(strLine[8]);
                        entTemp.SICKLEAVEDAYS = GetDecimalValue(strLine[9]);
                        entTemp.ANNUALLEVELDAYS = GetDecimalValue(strLine[10]);
                        entTemp.LEAVEUSEDDAYS = GetDecimalValue(strLine[11]);
                        entTemp.MARRYDAYS = GetDecimalValue(strLine[12]);
                        entTemp.MATERNITYLEAVEDAYS = GetDecimalValue(strLine[13]);
                        entTemp.NURSESDAYS = GetDecimalValue(strLine[14]);
                        entTemp.FUNERALLEAVEDAYS = GetDecimalValue(strLine[15]);
                        entTemp.TRIPDAYS = GetDecimalValue(strLine[16]);
                        entTemp.INJURYLEAVEDAYS = GetDecimalValue(strLine[17]);
                        entTemp.PRENATALCARELEAVEDAYS = GetDecimalValue(strLine[18]);
                        entTemp.OTHERLEAVEDAYS = entTemp.AFFAIRLEAVEDAYS + entTemp.SICKLEAVEDAYS + entTemp.ANNUALLEVELDAYS + entTemp.LEAVEUSEDDAYS
                            + entTemp.MARRYDAYS + entTemp.MATERNITYLEAVEDAYS + entTemp.NURSESDAYS + entTemp.FUNERALLEAVEDAYS + entTemp.TRIPDAYS
                            + entTemp.INJURYLEAVEDAYS + entTemp.PRENATALCARELEAVEDAYS;

                        entTemp.EVECTIONTIME = GetDecimalValue(strLine[19]);
                        entTemp.OVERTIMETIMES = GetDecimalValue(strLine[20]);
                        entTemp.OVERTIMESUMHOURS = GetDecimalValue(strLine[21]);
                        entTemp.OVERTIMESUMDAYS = GetDecimalValue(strLine[22]);

                        entTemp.OWNERCOMPANYID = entCurrEmp.OWNERCOMPANYID;
                        entTemp.OWNERDEPARTMENTID = entCurrEmp.OWNERDEPARTMENTID;
                        entTemp.OWNERPOSTID = entCurrEmp.OWNERPOSTID;
                        entTemp.OWNERID = entCurrEmp.EMPLOYEEID;

                        entTemp.CREATEPOSTID = entCreateUser.OWNERPOSTID;
                        entTemp.CREATEDEPARTMENTID = entCreateUser.OWNERDEPARTMENTID;
                        entTemp.CREATECOMPANYID = entCreateUser.OWNERCOMPANYID;
                        entTemp.CREATEUSERID = strCreateUserID;
                        entTemp.UPDATEUSERID = strCreateUserID;
                        entTemp.UPDATEDATE = DateTime.Now;
                        entTemp.CREATEDATE = DateTime.Now;

                        entTemp.REMARK = string.Empty;
                        if (strLine.Length > 22)
                        {
                            entTemp.REMARK = strLine[22];
                        }
                        //bool flag = this.IsExitEmployeeMonthlyBalance(entCurrEmp.EMPLOYEEID, dBalanceYear, dBalanceMonth);
                        //if (!flag)//返回false，即没有该员工该月审核中月度考勤才进行添加 //AddMonthlyBalance里面已有判断
                        //{
                        AddMonthlyBalance(entTemp);
                        // }
                    }
                    catch (Exception ex)
                    {
                        strMsg = "打卡记录导入失败";
                        Utility.SaveLog(ex.ToString());
                    }
                }
                TF.Close();
                strMsg = "{IMPORTSUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = "打卡记录导入失败";
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 从Excel读取数据
        /// </summary>
        /// <param name="strPhysicalPath"></param>
        /// <returns></returns>
        public List<T_HR_ATTENDMONTHLYBALANCE> ImportMonthlyBalanceForShow(string strPhysicalPath)
        {
            Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPhysicalPath, Encoding.GetEncoding("GB2312"));
            List<T_HR_ATTENDMONTHLYBALANCE> balanceList = new List<T_HR_ATTENDMONTHLYBALANCE>();
            TF.Delimiters = new string[] { "," }; //设置分隔符
            string[] strLine;
            while (!TF.EndOfData)
            {
                try
                {
                    strLine = TF.ReadFields();
                    string strFingerPrintId = strLine[0];
                    decimal dWorkServiceMonths = 0; //在职总月份数

                    T_HR_ATTENDMONTHLYBALANCE entTemp = new T_HR_ATTENDMONTHLYBALANCE();
                    entTemp.EMPLOYEECODE = strLine[0];
                    entTemp.EMPLOYEENAME = strLine[1];
                    entTemp.NEEDATTENDDAYS = GetDecimalValue(strLine[2]);
                    entTemp.REALATTENDDAYS = GetDecimalValue(strLine[3]);

                    entTemp.WORKSERVICEMONTHS = dWorkServiceMonths;

                    entTemp.FORGETCARDTIMES = GetDecimalValue(strLine[4]);
                    entTemp.LATETIMES = GetDecimalValue(strLine[5]);
                    entTemp.LEAVEEARLYTIMES = GetDecimalValue(strLine[6]);

                    entTemp.LATEDAYS = GetDecimalValue(strLine[5]);
                    entTemp.LEAVEEARLYDAYS = GetDecimalValue(strLine[6]);
                    entTemp.ABSENTDAYS = GetDecimalValue(strLine[7]);

                    entTemp.AFFAIRLEAVEDAYS = GetDecimalValue(strLine[8]);
                    entTemp.SICKLEAVEDAYS = GetDecimalValue(strLine[9]);
                    entTemp.ANNUALLEVELDAYS = GetDecimalValue(strLine[10]);
                    entTemp.LEAVEUSEDDAYS = GetDecimalValue(strLine[11]);
                    entTemp.MARRYDAYS = GetDecimalValue(strLine[12]);
                    entTemp.MATERNITYLEAVEDAYS = GetDecimalValue(strLine[13]);
                    entTemp.NURSESDAYS = GetDecimalValue(strLine[14]);
                    entTemp.FUNERALLEAVEDAYS = GetDecimalValue(strLine[15]);
                    entTemp.TRIPDAYS = GetDecimalValue(strLine[16]);
                    entTemp.INJURYLEAVEDAYS = GetDecimalValue(strLine[17]);
                    entTemp.PRENATALCARELEAVEDAYS = GetDecimalValue(strLine[18]);
                    entTemp.OTHERLEAVEDAYS = entTemp.AFFAIRLEAVEDAYS + entTemp.SICKLEAVEDAYS + entTemp.ANNUALLEVELDAYS + entTemp.LEAVEUSEDDAYS
                        + entTemp.MARRYDAYS + entTemp.MATERNITYLEAVEDAYS + entTemp.NURSESDAYS + entTemp.FUNERALLEAVEDAYS + entTemp.TRIPDAYS
                        + entTemp.INJURYLEAVEDAYS + entTemp.PRENATALCARELEAVEDAYS;

                    entTemp.EVECTIONTIME = GetDecimalValue(strLine[19]);
                    entTemp.OVERTIMETIMES = GetDecimalValue(strLine[20]);
                    entTemp.OVERTIMESUMHOURS = GetDecimalValue(strLine[21]);
                    entTemp.OVERTIMESUMDAYS = GetDecimalValue(strLine[22]);

                    entTemp.REMARK = strLine[23];

                    balanceList.Add(entTemp);
                }
                catch (Exception ex)
                {
                    Utility.SaveLog(ex.ToString());
                }
            }
            TF.Close();
            return balanceList;
        }


        /// <summary>
        /// 判断员工在该月是否已经有审核中的月度考勤
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <param name="year">考勤结算年份</param>
        /// <param name="month">考勤结算月份</param>
        /// <returns>存在返回true</returns>
        private bool IsExitEmployeeMonthlyBalance(string employeeID, decimal year, decimal month)
        {
            try
            {
                bool isExit = false;
                var employee = from a in dal.GetObjects()
                               where a.EMPLOYEEID == employeeID && a.BALANCEYEAR == year && a.BALANCEMONTH == month && a.CHECKSTATE == "1"
                               select a;
                if (employee.Count() > 0)
                {
                    isExit = true;
                    Utility.SaveLog("员工ID： " + employeeID + " 的员工没有生成月度考勤结算，因为该员工有在审核中的考勤数据" + DateTime.Now.ToString());
                }
                return isExit;
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 删除员工当月考勤记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <returns></returns>
        public string DeleteMonthlyBalance(string strCompanyID, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(strCompanyID) || string.IsNullOrWhiteSpace(strEmployeeID))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;

                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                flag = dalAttendMonthlyBalance.IsExistsRd(strCompanyID, strEmployeeID, dBalanceYear, dBalanceMonth);

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDMONTHLYBALANCE entDel = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByEmployeeID(strCompanyID, strEmployeeID, dBalanceYear, dBalanceMonth);

                Delete(entDel);
                Utility.SaveLog("删除T_HR_ATTENDMONTHLYBALANCE 员工ID：" + strEmployeeID + " 公司ID：" + strCompanyID + "的" + dBalanceYear + "年" + dBalanceMonth + "月考勤成功");
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 转化字符串为浮点数,且最多保留小数点后两位
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private decimal GetDecimalValue(string p)
        {
            decimal dRes = 0;
            decimal.TryParse(p, out dRes);
            dRes = decimal.Round(dRes, 2);
            return dRes;
        }

        /// <summary>
        /// 检查导入数据准备的参数是否符合要求，并返回要导入的员工集合
        /// </summary>
        /// <param name="strPhysicalPath">导入文件的物理地址</param>
        /// <param name="strUnitType">导入对象的类型</param>
        /// <param name="strUnitType">导入对象的类型</param>
        /// <param name="strUnitObjectId">导入对象的ID</param>
        /// <param name="dBalanceYear">考勤结算年份</param>
        /// <param name="dBalanceMonth">考勤结算月份</param>
        /// <param name="strMsg">导入完成后的消息</param>
        /// <param name="entEmployees">返回要导入的员工集合</param>
        private static void CheckImportFilters(string strPhysicalPath, string strUnitType, string strUnitObjectId, decimal dBalanceYear, decimal dBalanceMonth, ref string strMsg, ref List<T_HR_EMPLOYEE> entEmployees)
        {
            if (string.IsNullOrEmpty(strPhysicalPath))
            {
                strMsg = "{REQUIREDFIELDS}";
                return;
            }

            if (string.IsNullOrEmpty(strUnitType))
            {
                strMsg = "{REQUIREDFIELDS}";
                return;
            }

            if (string.IsNullOrEmpty(strUnitObjectId))
            {
                strMsg = "{REQUIREDFIELDS}";
                return;
            }

            //decimal dCheckYear = DateTime.Now.Year;
            //decimal dCheckMonth = DateTime.Now.Month;

            //if (dCheckYear != dBalanceYear || (dCheckMonth != dBalanceMonth && dCheckMonth != dBalanceMonth)
            //{
            //    strMsg = "{IMPORTMONTHLYBALANCEERROR}";
            //    return;
            //}

            DateTime dtBalance = DateTime.Parse(dBalanceYear.ToString() + "-" + dBalanceMonth.ToString() + "-1");

            EmployeeBLL bllEmployee = new EmployeeBLL();

            if (strUnitType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
            {
                entEmployees = bllEmployee.GetEmployeeByCompanyID(strUnitObjectId, dtBalance).ToList();
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
        }


        /// <summary>
        /// 新增T_HR_ATTENDMONTHLYBALANCE信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string AddMonthlyBalance(T_HR_ATTENDMONTHLYBALANCE entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;

                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                flag = dalAttendMonthlyBalance.IsExistsRd(entTemp.OWNERCOMPANYID, entTemp.EMPLOYEEID, entTemp.BALANCEYEAR.Value, entTemp.BALANCEMONTH.Value);

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalAttendMonthlyBalance.Add(entTemp);
                Utility.SaveLog("添加T_HR_ATTENDMONTHLYBALANCE 员工ID：" + entTemp.EMPLOYEEID + " 公司ID：" + entTemp.OWNERCOMPANYID + "的" + entTemp.BALANCEYEAR.Value + "年" + entTemp.BALANCEMONTH.Value + "月考勤成功");
                
                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
                if (ex.InnerException == null)
                {
                    Utility.SaveLog(strMsg);
                }
                else
                {
                    Utility.SaveLog(ex.InnerException.Message);
                }
            }

            return strMsg;
        }

        /// <summary>
        /// 修改员工考勤月度结算信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyMonthlyBalance(T_HR_ATTENDMONTHLYBALANCE entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;

                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                flag = dalAttendMonthlyBalance.IsExistsRd(entTemp.OWNERCOMPANYID, entTemp.EMPLOYEEID, entTemp.BALANCEYEAR.Value, entTemp.BALANCEMONTH.Value);

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_ATTENDMONTHLYBALANCE entUpdate = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByEmployeeID(entTemp.OWNERCOMPANYID, entTemp.EMPLOYEEID, entTemp.BALANCEYEAR.Value, entTemp.BALANCEMONTH.Value);

                Utility.CloneEntity(entTemp, entUpdate);

                if (entTemp.T_HR_ATTENDMONTHLYBATCHBALANCE != null)
                {
                    entUpdate.T_HR_ATTENDMONTHLYBATCHBALANCE = entTemp.T_HR_ATTENDMONTHLYBATCHBALANCE;
                    entUpdate.T_HR_ATTENDMONTHLYBATCHBALANCEReference.EntityKey = new EntityKey("SMT_HRM_EFModelContext.T_HR_ATTENDMONTHLYBATCHBALANCE", "MONTHLYBATCHID", entTemp.T_HR_ATTENDMONTHLYBATCHBALANCE.MONTHLYBATCHID);
                }

                dalAttendMonthlyBalance.Update(entUpdate);

                //此部分计算年度考勤结算记录转到客户端执行，提高月度考勤终审的执行速度
                //if (entTemp.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                //{
                //    EmployeeBLL bllEmployee = new EmployeeBLL();
                //    T_HR_EMPLOYEE entEmployee = bllEmployee.GetEmployeeByID(entTemp.EMPLOYEEID);

                //    AttendYearlyBalanceBLL bllAttendYearlyBalance = new AttendYearlyBalanceBLL();
                //    bllAttendYearlyBalance.CalculateSingleEmployeeAttendYearlyBalance(entEmployee, entTemp.BALANCEYEAR.Value, Convert.ToInt32(CheckStates.Approved).ToString());
                //}

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除员工考勤月度结算信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strMonthlyBalanceId">主键索引</param>
        /// <returns></returns>
        public string DeleteMonthlyBalance(string strMonthlyBalanceIds)
        {
            string strMsg = string.Empty;
            try
            {
                string[] ids = strMonthlyBalanceIds.Split(',');
                foreach (var strMonthlyBalanceId in ids)
                {
                    if (!string.IsNullOrEmpty(strMonthlyBalanceId))
                    {
                        // return "{REQUIREDFIELDS}";





                        bool flag = false;
                        StringBuilder strFilter = new StringBuilder();
                        List<string> objArgs = new List<string>();

                        strFilter.Append(" MONTHLYBALANCEID == @0");

                        objArgs.Add(strMonthlyBalanceId);


                        AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                        flag = dalAttendMonthlyBalance.IsExistsRd(strFilter.ToString(), objArgs.ToArray());



                        if (!flag)
                        {
                            return "{NOTFOUND}";
                        }

                        T_HR_ATTENDMONTHLYBALANCE entDel = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                        dalAttendMonthlyBalance.Delete(entDel);
                    }
                }
                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.ToString();
                Utility.SaveLog(strMsg);
            }

            return strMsg;
        }

        /// <summary>
        /// 计算指定月份指定员工的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份(年-月：2010-7)</param>
        /// <param name="strAssignedObjectType">需进行考勤结算的所属分配对象类型</param>
        /// <param name="strAssignedObjectID">需进行考勤结算的所属分配对象ID</param>
        public void CalculateEmployeeAttendanceMonthly(string strIsCurrentMonth, string strAssignedObjectType, string strAssignedObjectID)
        {
            try
            {
                if (string.IsNullOrEmpty(strAssignedObjectType) || string.IsNullOrEmpty(strAssignedObjectID))
                {
                    return;
                }

                string strCurMonth = string.Empty;
                if (strCurMonth == (Convert.ToInt32(Common.IsChecked.No) + 1).ToString())
                {
                    strCurMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
                }
                else
                {
                    strCurMonth = DateTime.Now.ToString("yyyy-MM");
                }

                if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Company) + 1).ToString())
                {
                    CalculateEmployeeAttendanceMonthlyByCompanyID(strCurMonth, strAssignedObjectID);
                }
                else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Department) + 1).ToString())
                {
                    CalculateEmployeeAttendanceMonthlyByDepartmentID(strCurMonth, strAssignedObjectID);
                }
                else if (strAssignedObjectType == (Convert.ToInt32(Common.AssignedObjectType.Post) + 1).ToString())
                {
                    CalculateEmployeeAttendanceMonthlyByPostID(strCurMonth, strAssignedObjectID);
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定月份指定公司的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">指定月份(年-月：2010-7)</param>
        /// <param name="strCompanyID">指定公司ID</param>
        public void CalculateEmployeeAttendanceMonthlyByCompanyID(string strCurDateMonth, string strCompanyID)
        {
            try
            {
                if (string.IsNullOrEmpty(strCompanyID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateMonth))
                {
                    return;
                }

                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                bool flag = false;
                flag = DateTime.TryParse((strCurDateMonth + "-1"), out dtStart);
                if (!flag)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByCompanyID(strCompanyID, dtStart);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                CalculateEmployeeAttendMonthlyBalance(ref dtStart, ref dtEnd, entEmployees, strCompanyID);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

        }

        /// <summary>
        /// 计算指定月份指定部门的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">月份(年-月：2010-7)</param>
        /// <param name="strDepartmentID">需进行考勤结算的员工所在部门ID</param>
        public void CalculateEmployeeAttendanceMonthlyByDepartmentID(string strCurDateMonth, string strDepartmentID)
        {
            try
            {
                if (string.IsNullOrEmpty(strDepartmentID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateMonth))
                {
                    return;
                }

                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                bool flag = false;
                flag = DateTime.TryParse((strCurDateMonth + "-1"), out dtStart);
                if (!flag)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                CompanyBLL bllCompany = new CompanyBLL();
                T_HR_COMPANY entCompany = bllCompany.GetCompanyByDepartmentID(strDepartmentID);

                if (entCompany == null)
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByDepartmentID(strDepartmentID);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                CalculateEmployeeAttendMonthlyBalance(ref dtStart, ref dtEnd, entEmployees, entCompany.COMPANYID);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定月份指定岗位的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">月份(年-月：2010-7)</param>
        /// <param name="strPostID">需进行考勤结算的员工所在岗位ID</param>
        public void CalculateEmployeeAttendanceMonthlyByPostID(string strCurDateMonth, string strPostID)
        {
            try
            {
                if (string.IsNullOrEmpty(strPostID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateMonth))
                {
                    return;
                }

                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                bool flag = false;
                flag = DateTime.TryParse((strCurDateMonth + "-1"), out dtStart);
                if (!flag)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                CompanyBLL bllCompany = new CompanyBLL();
                T_HR_COMPANY entCompany = bllCompany.GetCompanyByPostID(strPostID);

                if (entCompany == null)
                {
                    return;
                }

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = bllEmployee.GetEmployeeByPostID(strPostID);

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                CalculateEmployeeAttendMonthlyBalance(ref dtStart, ref dtEnd, entEmployees, entCompany.COMPANYID);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定月份指定员工的考勤月度结算
        /// </summary>
        /// <param name="strCurDateMonth">月份(年-月：2010-7)</param>
        /// <param name="strEmployeeID">需进行考勤结算的员工的员工ID</param>
        public void CalculateEmployeeAttendanceMonthlyByEmployeeID(string strCurDateMonth, string strEmployeeID)
        {
            try
            {
                if (string.IsNullOrEmpty(strEmployeeID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(strCurDateMonth))
                {
                    return;
                }

                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                bool flag = false;
                flag = DateTime.TryParse((strCurDateMonth + "-1"), out dtStart);
                if (!flag)
                {
                    return;
                }

                dtEnd = dtStart.AddMonths(1).AddDays(-1);

                EmployeeBLL bllEmployee = new EmployeeBLL();
                IQueryable<T_HR_EMPLOYEE> entEmployees = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                                         where e.EMPLOYEEID == strEmployeeID
                                                         select e;

                if (entEmployees.Count() == 0)
                {
                    return;
                }

                string strCompanyID = entEmployees.ToList().FirstOrDefault().OWNERCOMPANYID;

                CalculateEmployeeAttendMonthlyBalance(ref dtStart, ref dtEnd, entEmployees, strCompanyID);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        #region 私有方法

        /// <summary>
        /// 计算员工一段时间内的考勤情况
        /// </summary>
        /// <param name="dtStart">考勤结算起始日期</param>
        /// <param name="dtEnd">考勤结算截止日期</param>
        /// <param name="entEmployees">需进行考勤结算的员工</param>
        private void CalculateEmployeeAttendMonthlyBalance(ref DateTime dtStart, ref DateTime dtEnd,
            IQueryable<T_HR_EMPLOYEE> entEmployees, string strCompanyID)
        {
            try
            {
                AttendMonthlyBalanceDAL dalAttendMonthlyBalance = new AttendMonthlyBalanceDAL();
                AttendanceSolutionBLL bllAttendanceSolution = new AttendanceSolutionBLL();
                AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
                EmployeeEntryBLL bllEntry = new EmployeeEntryBLL();
                LeftOfficeConfirmBLL bllConfirm = new LeftOfficeConfirmBLL();

                List<string> entEmployeeList = new List<string>();

                foreach (T_HR_EMPLOYEE item in entEmployees)
                {
                    string strEmployeeID = item.EMPLOYEEID;
                    if (entEmployeeList.Contains(strEmployeeID))
                    {
                        continue;
                    }

                    entEmployeeList.Add(strEmployeeID);

                    IQueryable<T_HR_ATTENDANCERECORD> entAttRds = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(strCompanyID, item.EMPLOYEEID, dtStart, dtEnd);

                    if (entAttRds.Count() == 0)
                    {
                        continue;
                    }
                    
                    string strEmployeeId = entAttRds.FirstOrDefault().EMPLOYEEID;
                    DateTime dt=entAttRds.FirstOrDefault().ATTENDANCEDATE.Value;

                    //获取考勤记录对应的考勤方案
                    T_HR_ATTENDANCESOLUTION entAttSol = bllAttendanceSolution.GetAttendanceSolutionByEmployeeIDAndDate(strEmployeeId, dtStart, dtEnd);

                    Tracer.Debug(" 结算月度考勤结果，结算时间范围：" + dtStart.ToString("yyyy-MM-dd") + "--" + dtEnd.ToString("yyyy-MM-dd")
                        +"员工姓名：+" + item.EMPLOYEECNAME + " 使用的考勤方案名：" + entAttSol.ATTENDANCESOLUTIONNAME 
                        );

                    decimal dNeedAttendDays = 0, dRealNeedAttendDays = 0;    //标称应出勤天数, 实际应出勤天数
                    decimal? dWorkTimePerDay = entAttSol.WORKTIMEPERDAY;    //每日工作时长
                    decimal dWorkServiceMonths = 0; //在职总月份数
                    DateTime dtRealStart = dtStart;
                    DateTime dtRealEnd = dtEnd;

                    //检查考勤方案定义的工作制：1 固定制；2 实际天数(按月)
                    if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fixed) + 1).ToString())
                    {
                        dNeedAttendDays = entAttSol.WORKDAYS.Value; //固定制，则应出勤天数为考勤方案设定的WorkDays值                    
                        dRealNeedAttendDays = GetRealNeedAttendDaysForEmployee(ref dtRealStart, ref dtRealEnd, dNeedAttendDays, item.EMPLOYEEID, strCompanyID, entAttSol);    //实际应出勤天数                        
                    }
                    else if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fact) + 1).ToString())
                    {
                        //说明：以往是以考勤初始化记录来判断当月的应出勤天数，现在进行修正，改为实时计算
                        //dNeedAttendDays = entAttRds.Count();        //按实际天数,旧计算方式

                        dNeedAttendDays = GetNeedAttendDays(dtStart, dtEnd, strCompanyID, entAttSol);            //标称应出勤天数
                        dRealNeedAttendDays = GetRealNeedAttendDaysForEmployee(ref dtRealStart, ref dtRealEnd, dNeedAttendDays, item.EMPLOYEEID, strCompanyID, entAttSol);    //实际应出勤天数
                    }

                    if (dWorkTimePerDay == null)
                    {
                        continue;
                    }

                    if (dWorkTimePerDay.Value == 0)
                    {
                        continue;
                    }

                    GetEmployeeWorkServiceMonths(strEmployeeID, ref dWorkServiceMonths);//计算当前月员工的在职总月份数


                    T_HR_ATTENDMONTHLYBALANCE entAttendMonthlyBalance = dalAttendMonthlyBalance.GetAttendMonthlyBalanceRdByEmployeeID(strCompanyID, strEmployeeID, dtStart.Year, dtStart.Month);

                    if (entAttendMonthlyBalance != null)
                    {
                        DeleteMonthlyBalance(entAttendMonthlyBalance.MONTHLYBALANCEID);
                    }

                    entAttendMonthlyBalance = new T_HR_ATTENDMONTHLYBALANCE();
                    entAttendMonthlyBalance.MONTHLYBALANCEID = System.Guid.NewGuid().ToString().ToUpper();

                    //基础部分
                    entAttendMonthlyBalance.EMPLOYEEID = strEmployeeID;
                    entAttendMonthlyBalance.EMPLOYEECODE = item.EMPLOYEECODE;
                    entAttendMonthlyBalance.EMPLOYEENAME = item.EMPLOYEECNAME;
                    entAttendMonthlyBalance.BALANCEYEAR = dtStart.Year;
                    entAttendMonthlyBalance.BALANCEMONTH = dtStart.Month;
                    entAttendMonthlyBalance.BALANCEDATE = DateTime.Now;
                    entAttendMonthlyBalance.REMARK = string.Empty;
                    entAttendMonthlyBalance.WORKSERVICEMONTHS = dWorkServiceMonths;
                    entAttendMonthlyBalance.WORKTIMEPERDAY = dWorkTimePerDay;

                    entAttendMonthlyBalance.NEEDATTENDDAYS = dNeedAttendDays;   //应出勤天数
                    entAttendMonthlyBalance.REALNEEDATTENDDAYS = dRealNeedAttendDays; //应出勤天数(实际)

                    //考勤异常部分
                    decimal dAbsentMinutes = 0, dAbsentDays = 0, dLateMinutes = 0, dLateDays = 0, dLeaveEarlyDays = 0;//旷工，迟到，早退
                    int iLateTimes = 0, iLeaveEarlyTimes = 0, iForgetCardTimes = 0;//迟到，早退，漏打卡
                    decimal? dSkipWork = 0;//计算旷工总时数，以便换算成天数
                    decimal dNonWorkDays = 0;   //未计算的出勤天数

                    dNonWorkDays = GetNonWorkDays(strCompanyID, item.EMPLOYEEID, dtRealStart, dtRealEnd, dNeedAttendDays);

                    //2012-9-20 修改关于迟到的计算方式，达到4次以上（含四次），
                    //即按如下处理：旷工天数=迟到第四次算作旷工0.5天+迟到第5次算作旷工1天+未签卡旷工天数
                    CalculateSignInDetail(strEmployeeID, dtRealStart, dtRealEnd, dNeedAttendDays, dWorkTimePerDay, ref dAbsentMinutes,
                        ref dAbsentDays, ref dLateMinutes, ref dLateDays, ref dLeaveEarlyDays, ref iLateTimes, ref iLeaveEarlyTimes,
                        ref iForgetCardTimes, ref dSkipWork);

                    dAbsentDays += dNonWorkDays;
                    dAbsentMinutes += dNonWorkDays * entAttSol.WORKTIMEPERDAY.Value * 60;

                    if (entAttSol.ATTENDANCETYPE == (Convert.ToInt32(Common.AttendanceType.NoCheck) + 1).ToString())
                    {
                        //不考勤：实际未出勤天数 = 所有请假天数之和
                        dAbsentDays = 0;
                        dAbsentMinutes = 0;
                    }

                    entAttendMonthlyBalance.LATEDAYS = dLateDays;
                    entAttendMonthlyBalance.LEAVEEARLYDAYS = dLeaveEarlyDays;
                    entAttendMonthlyBalance.LATETIMES = iLateTimes;
                    entAttendMonthlyBalance.LATEMINUTES = dLateMinutes;
                    entAttendMonthlyBalance.LEAVEEARLYTIMES = iLeaveEarlyTimes;
                    entAttendMonthlyBalance.FORGETCARDTIMES = iForgetCardTimes;
                    entAttendMonthlyBalance.ABSENTDAYS = dAbsentDays;
                    entAttendMonthlyBalance.ABSENTMINUTES = dAbsentMinutes;

                    //请假部分
                    string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                    decimal? dAdjustLeaveDays = 0, dAffairLeaveDays = 0, dSickLeaveDays = 0, dOtherLeaveDays = 0;
                    decimal? dAnnualLevelDays = 0, dLeaveUsedDays = 0, dMarryDays = 0, dMaternityLeaveDays = 0;
                    decimal? dNursesDays = 0, dFuneralLeaveDays = 0, dTripDays = 0, dInjuryLeaveDays = 0, dPrenatalcareLeaveDays = 0;
                    decimal dSickLeaveFreeDay = 0;  //针对每月一天带薪病假使用

                    decimal dLeaveTotalDays = 0;  //实际请假天数

                    string strAttendState = (Convert.ToInt32(Common.AttendanceState.Leave) + 1).ToString();
                    //增加考勤结算时计算同时存在考勤异常和请假的天数
                    string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixLeveAbnormal) + 1).ToString();
                    IQueryable<T_HR_ATTENDANCERECORD> entCurAttRds = entAttRds.Where(t => t.ATTENDANCESTATE == strAttendState || t.ATTENDANCESTATE == strAttendStateMix);

                    decimal dCurLeaveDays = entCurAttRds.Count();//标称请假天数

                    CalculateEmployeeLeaveDays(strEmployeeID, dtRealStart, dtRealEnd, strCheckState, dWorkTimePerDay, dNeedAttendDays, dRealNeedAttendDays, dCurLeaveDays,
                        ref dAdjustLeaveDays, ref dAffairLeaveDays, ref dSickLeaveDays, ref dOtherLeaveDays, ref dAnnualLevelDays, ref dLeaveUsedDays,
                        ref dMarryDays, ref dMaternityLeaveDays, ref dNursesDays, ref dFuneralLeaveDays, ref dTripDays, ref dInjuryLeaveDays, ref dPrenatalcareLeaveDays, ref dSickLeaveFreeDay);

                    if (dAffairLeaveDays == dRealNeedAttendDays)
                    {
                        dAffairLeaveDays = dNeedAttendDays;
                    }

                    if (dSickLeaveDays == dRealNeedAttendDays)
                    {
                        dSickLeaveDays = dNeedAttendDays;
                    }

                    if (dLeaveUsedDays == dRealNeedAttendDays)
                    {
                        dLeaveUsedDays = dNeedAttendDays;
                    }

                    if (dAnnualLevelDays == dRealNeedAttendDays)
                    {
                        dAnnualLevelDays = dNeedAttendDays;
                    }

                    if (dMaternityLeaveDays == dRealNeedAttendDays)
                    {
                        dMaternityLeaveDays = dNeedAttendDays;
                    }

                    if (dMarryDays == dRealNeedAttendDays)
                    {
                        dMarryDays = dNeedAttendDays;
                    }

                    if (dNursesDays == dRealNeedAttendDays)
                    {
                        dNursesDays = dNeedAttendDays;
                    }

                    if (dTripDays == dRealNeedAttendDays)
                    {
                        dTripDays = dNeedAttendDays;
                    }

                    if (dInjuryLeaveDays == dRealNeedAttendDays)
                    {
                        dInjuryLeaveDays = dNeedAttendDays;
                    }

                    if (dPrenatalcareLeaveDays == dRealNeedAttendDays)
                    {
                        dPrenatalcareLeaveDays = dNeedAttendDays;
                    }

                    if (dFuneralLeaveDays == dRealNeedAttendDays)
                    {
                        dFuneralLeaveDays = dNeedAttendDays;
                    }

                    if (dAdjustLeaveDays == dRealNeedAttendDays)
                    {
                        dAdjustLeaveDays = dNeedAttendDays;
                    }

                    if (dSickLeaveDays != null)
                    {
                        if (dSickLeaveDays < dSickLeaveFreeDay)
                        {
                            dSickLeaveFreeDay = dSickLeaveDays.Value;
                        }
                    }

                    entAttendMonthlyBalance.AFFAIRLEAVEDAYS = dAffairLeaveDays;
                    entAttendMonthlyBalance.SICKLEAVEDAYS = dSickLeaveDays;
                    entAttendMonthlyBalance.OTHERLEAVEDAYS = dOtherLeaveDays;
                    entAttendMonthlyBalance.ANNUALLEVELDAYS = dAnnualLevelDays;
                    entAttendMonthlyBalance.LEAVEUSEDDAYS = dLeaveUsedDays;
                    entAttendMonthlyBalance.MARRYDAYS = dMarryDays;
                    entAttendMonthlyBalance.MATERNITYLEAVEDAYS = dMaternityLeaveDays;
                    entAttendMonthlyBalance.NURSESDAYS = dNursesDays;
                    entAttendMonthlyBalance.FUNERALLEAVEDAYS = dFuneralLeaveDays;
                    entAttendMonthlyBalance.TRIPDAYS = dTripDays;
                    entAttendMonthlyBalance.INJURYLEAVEDAYS = dInjuryLeaveDays;
                    entAttendMonthlyBalance.PRENATALCARELEAVEDAYS = dPrenatalcareLeaveDays;

                    //实际请假天数 = 所有请假天数之和
                    dLeaveTotalDays = dLeaveUsedDays.Value + dAffairLeaveDays.Value + dSickLeaveDays.Value + dAnnualLevelDays.Value + dMarryDays.Value
                            + dMaternityLeaveDays.Value + dNursesDays.Value + dFuneralLeaveDays.Value + dTripDays.Value + dInjuryLeaveDays.Value + dPrenatalcareLeaveDays.Value;

                    if (dLeaveTotalDays < 0)
                    {
                        dLeaveTotalDays = 0;
                    }

                    dRealNeedAttendDays = dRealNeedAttendDays - dAbsentDays;
                    if (dRealNeedAttendDays <= 0)
                    {
                        dRealNeedAttendDays = 0;
                    }

                    if (dLeaveTotalDays != 0)
                    {
                        //如果大于等于应出勤天数，则进一步判断标称请假天数是否等于实际应出勤天数(为请假人员使用)
                        if (dLeaveTotalDays >= dRealNeedAttendDays)
                        {
                            dRealNeedAttendDays = 0;
                        }
                        else if (dLeaveTotalDays < dRealNeedAttendDays)
                        {
                            dRealNeedAttendDays = dRealNeedAttendDays - dLeaveTotalDays;
                        }
                    }

                    //实际出勤天数
                    entAttendMonthlyBalance.REALATTENDDAYS = dRealNeedAttendDays;

                    //加班部分
                    int iOverTimeTimes = 0;
                    decimal? dOverTimeSumHours = 0, dOvertimeSumDays = 0;
                    CalculateEmployeeOverTimeDays(entAttSol, strEmployeeID, dtRealStart, dtRealEnd, strCheckState, dWorkTimePerDay, ref iOverTimeTimes, ref dOverTimeSumHours, ref dOvertimeSumDays);

                    entAttendMonthlyBalance.OVERTIMETIMES = iOverTimeTimes;
                    entAttendMonthlyBalance.OVERTIMESUMHOURS = dOverTimeSumHours;
                    entAttendMonthlyBalance.OVERTIMESUMDAYS = dOvertimeSumDays;

                    //出差部分
                    decimal? dEvectionTime = 0;
                    CalculateEmployeeEvectionTime(entAttSol, entAttRds, strEmployeeID, dtRealStart, dtRealEnd, dWorkTimePerDay, ref dEvectionTime);
                    entAttendMonthlyBalance.EVECTIONTIME = dEvectionTime;

                    //外出申请时长计算部分
                    decimal? dOutApplyTime = 0;
                    CalculateEmployeeOutApplyTime(entAttSol, entAttRds, strEmployeeID, dtRealStart, dtRealEnd, dWorkTimePerDay, ref dOutApplyTime);
                    entAttendMonthlyBalance.OUTAPPLYTIME = dOutApplyTime;

                    //权限
                    entAttendMonthlyBalance.OWNERCOMPANYID = item.OWNERCOMPANYID;
                    entAttendMonthlyBalance.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                    entAttendMonthlyBalance.OWNERPOSTID = item.OWNERPOSTID;
                    entAttendMonthlyBalance.OWNERID = item.OWNERID;
                    entAttendMonthlyBalance.CREATEPOSTID = item.CREATEPOSTID;
                    entAttendMonthlyBalance.CREATEDEPARTMENTID = item.CREATEDEPARTMENTID;
                    entAttendMonthlyBalance.CREATECOMPANYID = item.CREATECOMPANYID;
                    entAttendMonthlyBalance.CREATEUSERID = item.CREATEUSERID;
                    entAttendMonthlyBalance.CREATEDATE = DateTime.Now;
                    entAttendMonthlyBalance.UPDATEUSERID = item.UPDATEUSERID;
                    entAttendMonthlyBalance.UPDATEDATE = DateTime.Now;

                    //审批
                    entAttendMonthlyBalance.CHECKSTATE = Convert.ToInt32(Common.CheckStates.UnSubmit).ToString();
                    entAttendMonthlyBalance.EDITSTATE = Convert.ToInt32(Common.EditStates.UnActived).ToString();

                    //string strMsg = AddMonthlyBalance(entAttendMonthlyBalance);
                    //if (strMsg == "{ALREADYEXISTSRECORD}")
                    //{
                    //    ModifyMonthlyBalance(entAttendMonthlyBalance);
                    //}
                    bool flag = this.IsExitEmployeeMonthlyBalance(entAttendMonthlyBalance.EMPLOYEEID, dtStart.Year, dtStart.Month);
                    if (!flag)//返回false，即没有该员工该月审核中月度考勤才进行添加
                    {
                        string strMsg = AddMonthlyBalance(entAttendMonthlyBalance);
                        if (strMsg == "{ALREADYEXISTSRECORD}")
                        {
                            ModifyMonthlyBalance(entAttendMonthlyBalance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算考勤实际应出勤天数（针对月中入离职人员与普通在职人员的应出勤天数区别）
        /// </summary>
        /// <param name="dtStart">考勤结算起始日期</param>
        /// <param name="dtEnd">考勤结算截止日期</param>
        /// <param name="dNeedAttendDays">本月应出勤天数</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strCompanyID">员工所在公司ID</param>
        /// <param name="entAttSol">员工使用的考勤方案</param>
        /// <returns></returns>
        private decimal GetRealNeedAttendDaysForEmployee(ref DateTime dtStart, ref DateTime dtEnd, decimal dNeedAttendDays, string strEmployeeID, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = dNeedAttendDays;
            DateTime? dtEntry = null;
            DateTime? dtLeft = null;

            string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();

            var eEntry = from n in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                         where n.T_HR_EMPLOYEE.EMPLOYEEID == strEmployeeID && n.CHECKSTATE == strCheckState && n.OWNERCOMPANYID == strCompanyID
                         orderby n.UPDATEDATE descending
                         select n;

            if (eEntry == null)
            {
                return dRes;
            }

            T_HR_EMPLOYEEENTRY entEntry = eEntry.ToList().FirstOrDefault();

            if (entEntry == null)
            {
                return dRes;
            }

            if (entEntry.ENTRYDATE == null)
            {
                return dRes;
            }

            dtEntry = entEntry.ENTRYDATE.Value;

            var eLeft = from n in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>()
                        where n.EMPLOYEEID == strEmployeeID && n.CHECKSTATE == strCheckState && n.OWNERCOMPANYID == strCompanyID
                        orderby n.CREATEDATE descending
                        select n;

            if (eLeft != null)
            {
                T_HR_LEFTOFFICECONFIRM entLeft = eLeft.ToList().FirstOrDefault();

                if (entLeft != null)
                {
                    string strEmpPostID = entLeft.EMPLOYEEPOSTID;

                    var empos = from n in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where n.EMPLOYEEPOSTID == strEmpPostID
                                select n;

                    T_HR_EMPLOYEEPOST empost = empos.ToList().FirstOrDefault();

                    if (entLeft.STOPPAYMENTDATE != null && empost != null)
                    {
                        if (empost.ISAGENCY == Convert.ToInt32(Common.IsAgencyPost.No).ToString())
                        {
                            dtLeft = entLeft.STOPPAYMENTDATE.Value;
                        }
                    }
                }
            }

            bool bCalculate = false;

            if (dtEntry != null)
            {
                if (dtEntry <= dtEnd && dtEntry > dtStart)
                {
                    dtStart = dtEntry.Value;
                    bCalculate = true;
                }
            }

            if (dtLeft != null)
            {
                if (dtLeft <= dtEnd && dtLeft > dtStart)
                {
                    dtEnd = dtLeft.Value;
                    bCalculate = true;
                }
            }

            if (bCalculate)
            {
                dRes = this.GetRealAttendDays(dtStart, dtEnd, strCompanyID, entAttSol);
            }

            return dRes;
        }

        /// <summary>
        /// 获取指定员工的在职总月份
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dWorkServiceMonths">在职总月份</param>
        private void GetEmployeeWorkServiceMonths(string strEmployeeID, ref decimal dWorkServiceMonths)
        {
            try
            {
                EmployeeBLL bllEmp = new EmployeeBLL();
                V_EMPLOYEEDETAIL entEmpDetail = bllEmp.GetEmployeeDetailView(strEmployeeID);

                if (entEmpDetail == null)
                {
                    return;
                }

                dWorkServiceMonths = entEmpDetail.WORKAGE;
            }
            catch (Exception ex)
            {
                Utility.SaveLog("执行GetEmployeeWorkServiceMonths函数获取指定员工的在职总月份失败，参数：strEmployeeID= "
                    + strEmployeeID + ",失败原因：" + ex.ToString());
            }
        }

        /// <summary>
        /// 实时计算应出勤天数
        /// </summary>
        /// <param name="dtStart">起始日期</param>
        /// <param name="dtEnd">截止日期</param>
        /// <param name="strCompanyID">所属公司</param>
        /// <param name="entAttSol">考勤方案定义</param>
        /// <returns>应出勤天数</returns>
        public decimal GetNeedAttendDays(DateTime dtStart, DateTime dtEnd, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = 0;
            decimal dWorkMode = entAttSol.WORKMODE.Value;
            string strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;

            int iWorkMode = 0;
            int.TryParse(dWorkMode.ToString(), out iWorkMode);

            List<int> iWorkDays = new List<int>();
            Utility.GetWorkDays(iWorkMode, ref iWorkDays);

            if (entAttSol.WORKDAYTYPE != (Convert.ToInt32(Common.WorkDayType.Fact) + 1).ToString())
            {
                if (entAttSol.WORKDAYS != null)
                {
                    dRes = entAttSol.WORKDAYS.Value;
                }
                return dRes;
            }

            TimeSpan ts = dtEnd.Subtract(dtStart);
            int iTotalDay = ts.Days + 1;
            int n = 0;
            DateTime dtCheck = dtStart;

            while (n < iTotalDay)
            {
                dtCheck = dtStart.AddDays(n);
                n++;
                if (iWorkDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)) == false)
                {
                    continue;
                }

                dRes += 1;
            }

            return dRes;
        }



        /// <summary>
        /// 实时计算实际出勤天数
        /// </summary>
        /// <param name="dtStart">起始日期</param>
        /// <param name="dtEnd">截止日期</param>
        /// <param name="strCompanyID">所属公司</param>
        /// <param name="entAttSol">考勤方案定义</param>
        /// <returns>实际出勤天数</returns>
        public decimal GetRealAttendDays(DateTime dtStart, DateTime dtEnd, string strCompanyID, T_HR_ATTENDANCESOLUTION entAttSol)
        {
            decimal dRes = 0;
            decimal dWorkMode = entAttSol.WORKMODE.Value;
            string strAttendanceSolutionID = entAttSol.ATTENDANCESOLUTIONID;

            int iWorkMode = 0;
            int.TryParse(dWorkMode.ToString(), out iWorkMode);

            List<int> iWorkDays = new List<int>();
            Utility.GetWorkDays(iWorkMode, ref iWorkDays);

            TimeSpan ts = dtEnd.Subtract(dtStart);
            int iTotalDay = ts.Days + 1;
            int n = 0;
            DateTime dtCheck = dtStart;

            while (n < iTotalDay)
            {
                dtCheck = dtStart.AddDays(n);
                n++;
                if (iWorkDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)) == false)
                {
                    continue;
                }

                dRes += 1;
            }

            if (entAttSol.WORKDAYTYPE == (Convert.ToInt32(Common.WorkDayType.Fixed) + 1).ToString())
            {
                if (entAttSol.WORKDAYS != null)
                {
                    if (dRes > entAttSol.WORKDAYS.Value)
                    {
                        dRes = entAttSol.WORKDAYS.Value;
                    }
                }
            }

            return dRes;
        }

        /// <summary>
        /// 计算未进行考勤结算的天数
        /// </summary>
        /// <param name="strCompanyID">公司ID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">结算起始日期</param>
        /// <param name="dtEnd">结算截至日期</param>
        /// <returns></returns>
        private decimal GetNonWorkDays(string strCompanyID, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal dNeedAttendDays)
        {
            AttendanceRecordBLL bllAttendanceRecord = new AttendanceRecordBLL();
            IQueryable<T_HR_ATTENDANCERECORD> entAttRds = bllAttendanceRecord.GetAttendanceRecordByEmployeeIDAndDate(strCompanyID, strEmployeeID, dtStart, dtEnd);
            
            decimal dDays = 0;
            if (entAttRds == null)
            {
                return dDays;
            }

            int dCheckDays = entAttRds.Count();

            if (dCheckDays == 0)
            {
                return dDays;
            }

            foreach (T_HR_ATTENDANCERECORD item in entAttRds)
            {
                if(string.IsNullOrWhiteSpace(item.ATTENDANCESTATE))
                {
                    dDays += 1;
                }
            }

            if (dDays == dCheckDays)
            {
                dDays = dNeedAttendDays;
            }

            return dDays;
        }

        /// <summary>
        ///  获取计算时间内，公休假天数，方便计算应出勤天数
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="iPubVacDays"></param>
        private void GetPubVacDays(T_HR_ATTENDANCESOLUTION entAttSol, DateTime dtStart, DateTime dtEnd, ref int iPubVacDays)
        {
            try
            {
                if (entAttSol == null)
                {
                    return;
                }

                if (entAttSol.WORKMODE == null)
                {
                    return;
                }

                int iWorkmode = int.Parse(entAttSol.WORKMODE.Value.ToString());

                string strDayType = "1";

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                IQueryable<T_HR_OUTPLANDAYS> ents = bllOutPlanDays.GetOutPlanDaysRdListByCompanyIdAndDate(entAttSol, dtStart, dtEnd, strDayType);

                if (ents == null)
                {
                    return;
                }

                if (ents.Count() == 0)
                {
                    return;
                }

                decimal? dTotalVacDays = ents.Sum(t => t.DAYS);

                if (dTotalVacDays == null)
                {
                    return;
                }

                if (dTotalVacDays == 7)
                {
                    iPubVacDays = 3;
                }
                else if (dTotalVacDays >= 1 && dTotalVacDays < 7)
                {
                    iPubVacDays = 1;
                }

            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的各类型考勤异常的天数，次数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">考勤日期查询起始日</param>
        /// <param name="dtEnd">考勤日期查询截止日</param>
        /// <param name="dNeedAttendDays">应出勤天数</param>
        /// <param name="dWorkTimePerDay">每日工作时长</param>
        /// <param name="dAbsentMinutes">旷工时长（分钟）</param>
        /// <param name="dAbsentDays">旷工天数</param>
        /// <param name="dLateMinutes">迟到时长（分钟）</param>
        /// <param name="dLateDays">迟到天数</param>
        /// <param name="dLeaveEarlyDays">早退天数</param>
        /// <param name="iLateTimes">迟到次数</param>
        /// <param name="iLeaveEarlyTimes">早退次数</param>
        /// <param name="iForgetCardTimes">漏打卡次数</param>
        /// <param name="dSkipWork">旷工天数</param>
        private static void CalculateSignInDetail(string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal dNeedAttendDays, decimal? dWorkTimePerDay,
            ref decimal dAbsentMinutes, ref decimal dAbsentDays, ref decimal dLateMinutes, ref decimal dLateDays, ref decimal dLeaveEarlyDays,
            ref int iLateTimes, ref int iLeaveEarlyTimes, ref int iForgetCardTimes, ref decimal? dSkipWork)
        {
            try
            {
                AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords = bllAbnormRecord.GetAbnormRecordRdListByEmpIdAndDate(strEmployeeID, dtStart, dtEnd);

                string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();

                if (entAbnormRecords.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_EMPLOYEEABNORMRECORD item in entAbnormRecords)
                {
                    if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AbnormCategory.Absent) + 1).ToString())
                    {
                        if (item.SINGINSTATE != (Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString())
                        {
                            dSkipWork += item.ABNORMALTIME;
                        }
                        else
                        {
                            EmployeeSignInDetailBLL bllSignInDetail = new EmployeeSignInDetailBLL();
                            T_HR_EMPLOYEESIGNINDETAIL ent = bllSignInDetail.GetEmployeeSignInDetailByAbnormRecordIDAndCheckState(item.ABNORMRECORDID, strCheckState);

                            if (ent == null)
                            {
                                continue;
                            }

                            if (ent.REASONCATEGORY == (Convert.ToInt32(Common.AbnormReasonCategory.DrainPunch) + 1).ToString())
                            {
                                iForgetCardTimes += 1;
                            }
                        }
                    }
                    else if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AttendAbnormalType.Late) + 1).ToString())
                    {
                        iLateTimes += 1;
                        dLateMinutes += item.ABNORMALTIME.Value;
                    }
                    else if (item.ABNORMCATEGORY == (Convert.ToInt32(Common.AttendAbnormalType.LeaveEarly) + 1).ToString())
                    {
                        iLeaveEarlyTimes += 1;
                    }
                }

                string strLateCate = (Convert.ToInt32(Common.AttendAbnormalType.Late) + 1).ToString();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> lat = entAbnormRecords.Where(c => c.ABNORMCATEGORY == strLateCate);
                dLateDays = lat.Select(s => s.T_HR_ATTENDANCERECORD).Distinct().Count();

                string strLeaveEarlyCate = (Convert.ToInt32(Common.AttendAbnormalType.LeaveEarly) + 1).ToString();
                IQueryable<T_HR_EMPLOYEEABNORMRECORD> lea = entAbnormRecords.Where(c => c.ABNORMCATEGORY == strLeaveEarlyCate);
                dLeaveEarlyDays = lea.Select(s => s.T_HR_ATTENDANCERECORD).Distinct().Count();

                dAbsentMinutes = dSkipWork.Value;
                dWorkTimePerDay = dWorkTimePerDay.Value * 60;
                dAbsentDays = decimal.Round(dSkipWork.Value / dWorkTimePerDay.Value, 0);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的各类请假天数
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtStart">请假日期起始时间</param>
        /// <param name="dtEnd">请假日期截止时间</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="dWorkTimePerDay">日工作时长</param>
        /// <param name="dNeedAttendDays">标称应出勤天数</param>
        /// <param name="dRealNeedAttendDays">实际应出勤天数</param>
        /// <param name="dCurLeaveDays">标称应出勤天数</param>
        /// <param name="dAffairLeaveDays">事假天数</param>
        /// <param name="dSickLeaveDays">病假天数</param>
        /// <param name="dOtherLeaveDays">其他假期天数</param>
        /// <param name="dAnnualLevelDays">年休假天数</param>
        /// <param name="dLeaveUsedDays">调休假天数</param>
        /// <param name="dMarryDays">婚假天数</param>
        /// <param name="dMaternityLeaveDays">产假天数</param>
        /// <param name="dNursesDays">看护假天数</param>
        /// <param name="dFuneralLeaveDays">丧假天数</param>
        /// <param name="dTripDays">路程假天数</param>
        /// <param name="dInjuryLeaveDays">工伤假天数</param>
        /// <param name="dPrenatalcareLeaveDays">产前检查假天数</param>
        /// <param name="dInjuryLeaveDays">每月一天带薪病假</param>
        private void CalculateEmployeeLeaveDays(string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState, decimal? dWorkTimePerDay,
            decimal dNeedAttendDays, decimal dRealNeedAttendDays, decimal dCurLeaveDays, ref decimal? dAdjustLeaveDays, ref decimal? dAffairLeaveDays,
            ref decimal? dSickLeaveDays, ref decimal? dOtherLeaveDays, ref decimal? dAnnualLevelDays, ref decimal? dLeaveUsedDays, ref decimal? dMarryDays,
            ref decimal? dMaternityLeaveDays, ref decimal? dNursesDays, ref decimal? dFuneralLeaveDays, ref decimal? dTripDays, ref decimal? dInjuryLeaveDays,
            ref decimal? dPrenatalcareLeaveDays, ref decimal dSickLeaveFreeDay)
        {
            try
            {
                EmployeeLeaveRecordBLL bllEmpLeaveRd = new EmployeeLeaveRecordBLL();
                dtEnd = dtEnd.AddDays(1).AddSeconds(-1);
                IQueryable<T_HR_EMPLOYEELEAVERECORD> entEmployeeLeaveRecords = bllEmpLeaveRd.GetEmployeeLeaveRdListByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd, strCheckState);

                if (entEmployeeLeaveRecords == null)
                {
                    return;
                }

                if (entEmployeeLeaveRecords.Count() == 0)
                {
                    return;
                }

                foreach (T_HR_EMPLOYEELEAVERECORD entEmployeeLeaveRecord in entEmployeeLeaveRecords)
                {
                    decimal? dLeaveTotalHours = 0;//计算实际请假总时长
                    decimal? dCancelLeaveTotalHours = 0;//计算实际销假总时长
                    if (entEmployeeLeaveRecord.TOTALHOURS == null)
                    {
                        continue;
                    }

                    AdjustLeaveBLL bllAdjustLeave = new AdjustLeaveBLL();
                    T_HR_ADJUSTLEAVE entAdjustLeave = bllAdjustLeave.GetAdjustLeaveByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID);

                    EmployeeCancelLeaveBLL bllCancelLeave = new EmployeeCancelLeaveBLL();
                    //T_HR_EMPLOYEECANCELLEAVE entCancelLeave = bllCancelLeave.GetEmployeeLeaveRdListByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID, strCheckState);
                    IQueryable<T_HR_EMPLOYEECANCELLEAVE> entCancelLeaveList = bllCancelLeave.GetEmployeeLeaveRdListByLeaveRecordID(entEmployeeLeaveRecord.LEAVERECORDID, strCheckState);
                    
                    T_HR_LEAVETYPESET entLeaveTypeSet = entEmployeeLeaveRecord.T_HR_LEAVETYPESET;

                    if (entLeaveTypeSet == null)
                    {
                        continue;
                    }

                    if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AffairLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dAffairLeaveDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                        else if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
                        {
                            if (entAdjustLeave != null)
                            {

                                dAdjustLeaveDays += entAdjustLeave.ADJUSTLEAVEDAYS * dWorkTimePerDay.Value;
                            }

                        }


                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dAffairLeaveDays = dAffairLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.SickLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dSickLeaveDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dSickLeaveFreeDay = 1;
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                        else if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevDeduct) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
                        {
                            dSickLeaveFreeDay = 1;
                            if (entAdjustLeave != null)
                            {
                                dAdjustLeaveDays += entAdjustLeave.ADJUSTLEAVEDAYS * dWorkTimePerDay.Value;
                            }

                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dSickLeaveDays = dSickLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AdjustLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dLeaveUsedDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }


                        dLeaveUsedDays = dLeaveUsedDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.AnnualLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dAnnualLevelDays += dLeaveTotalHours;

                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }


                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dAnnualLevelDays = dAnnualLevelDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.MaternityLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dMaternityLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dMaternityLeaveDays = dMaternityLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.MarryLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dMarryDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.NursesLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dNursesDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dNursesDays = dNursesDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.TripLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dTripDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }


                        dTripDays = dTripDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.InjuryLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dInjuryLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dInjuryLeaveDays = dInjuryLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.PrenatalcareLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dPrenatalcareLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dPrenatalcareLeaveDays = dPrenatalcareLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                    else if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(Common.LeaveTypeValue.FuneralLeave) + 1).ToString())
                    {
                        CalculateRealCurrMonthLeaveDays(entEmployeeLeaveRecord.EMPLOYEEID, entEmployeeLeaveRecord.STARTDATETIME.Value, entEmployeeLeaveRecord.ENDDATETIME.Value, dtStart, dtEnd, ref dLeaveTotalHours);
                        dFuneralLeaveDays += dLeaveTotalHours;
                        if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(Common.LeaveFineType.Free) + 1).ToString())
                        {
                            dAdjustLeaveDays += dLeaveTotalHours;
                        }

                        if (entCancelLeaveList != null)
                        {
                            foreach (var entCancelLeave in entCancelLeaveList)
                            {
                                CalculateRealCurrMonthLeaveDays(entCancelLeave.EMPLOYEEID, entCancelLeave.STARTDATETIME.Value, entCancelLeave.ENDDATETIME.Value, dtStart, dtEnd, ref dCancelLeaveTotalHours);
                            }
                        }

                        dFuneralLeaveDays = dFuneralLeaveDays - dCancelLeaveTotalHours;
                        dAdjustLeaveDays = dAdjustLeaveDays - dCancelLeaveTotalHours;
                    }
                }

                dWorkTimePerDay = dWorkTimePerDay.Value;
                string strNumOfDecDefault = "0.5";

                if (dAdjustLeaveDays == null)
                {
                    dAdjustLeaveDays = 0;
                }

                if (dAffairLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dAffairLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dSickLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dSickLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dLeaveUsedDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dLeaveUsedDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dAnnualLevelDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dAnnualLevelDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dMaternityLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dMaternityLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dMarryDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dMarryDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dNursesDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dNursesDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dTripDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dTripDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dInjuryLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dInjuryLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                if (dFuneralLeaveDays > dCurLeaveDays * dWorkTimePerDay.Value)
                {
                    dFuneralLeaveDays = dCurLeaveDays * dWorkTimePerDay.Value;
                }

                dAffairLeaveDays = RoundOff(dAffairLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dSickLeaveDays = RoundOff(dSickLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dOtherLeaveDays = 0;
                dLeaveUsedDays = RoundOff(dLeaveUsedDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dAnnualLevelDays = RoundOff(dAnnualLevelDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dMaternityLeaveDays = RoundOff(dMaternityLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dMarryDays = RoundOff(dMarryDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dNursesDays = RoundOff(dNursesDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dTripDays = RoundOff(dTripDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dInjuryLeaveDays = RoundOff(dInjuryLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dFuneralLeaveDays = RoundOff(dFuneralLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dPrenatalcareLeaveDays = RoundOff(dPrenatalcareLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
                dAdjustLeaveDays = RoundOff(dAdjustLeaveDays.Value / dWorkTimePerDay.Value, strNumOfDecDefault, 1);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取员工考勤结算月每条 请假/销假 的实际总时长
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="dtLeaveStart">当前请假起始日期</param>
        /// <param name="dtLeaveEnd">当前请假结束日期</param>
        /// <param name="dtCurrMonthStart">考勤结算月起始日期</param>
        /// <param name="dtCurrMonthEnd">考勤结算月截止日期</param>
        /// <param name="dLeaveTotalHours">单条 请假/销假 的实际总时长</param>
        private void CalculateRealCurrMonthLeaveDays(string strEmployeeID, DateTime dtLeaveStart, DateTime dtLeaveEnd,
            DateTime dtCurrMonthStart, DateTime dtCurrMonthEnd, ref decimal? dLeaveTotalHours)
        {
            if (string.IsNullOrWhiteSpace(strEmployeeID))
            {
                return;
            }

            if (dtLeaveStart == null || dtLeaveEnd == null)
            {
                return;
            }

            DateTime dtCalculateStart = new DateTime();
            DateTime dtCalculateEnd = new DateTime();
            DateTime dtCheck = new DateTime();

            //判断 请假/销假 起始日期是否小于考勤结算月起始日期，根据日期大小进一步判断 请假/销假 结束日期与考勤结算截止日期大小，从而获取
            // 请假/销假 实际请假的起止日期，以便计算实际的  请假/销假 天数
            if (dtLeaveStart < dtCurrMonthStart)
            {
                if (dtLeaveEnd >= dtCurrMonthEnd)
                {
                    dtCalculateStart = dtCurrMonthStart;
                    dtCalculateEnd = dtCurrMonthEnd;
                }
                else
                {
                    if (dtLeaveEnd > dtCurrMonthStart)
                    {
                        dtCalculateStart = dtCurrMonthStart;
                        dtCalculateEnd = dtLeaveEnd;
                    }
                }
            }
            else
            {
                dtCalculateStart = dtLeaveStart;

                if (dtLeaveEnd >= dtCurrMonthEnd)
                {
                    dtCalculateStart = dtLeaveStart;
                    dtCalculateEnd = dtCurrMonthEnd;
                }
                else
                {
                    dtCalculateEnd = dtLeaveEnd;
                }
            }

            if (dtCalculateStart <= dtCheck || dtCalculateEnd <= dtCheck)
            {
                return;
            }

            decimal dRealLeaveDays = 0, dRealLeaveHours = 0, dRealLeaveTotalHours = 0;
            string strId = System.Guid.NewGuid().ToString();
            EmployeeLeaveRecordBLL bllLeaveRecord = new EmployeeLeaveRecordBLL();
            bllLeaveRecord.GetRealLeaveDayByEmployeeIdAndDate(strId, strEmployeeID, dtCalculateStart, dtCalculateEnd, ref dRealLeaveDays, ref dRealLeaveHours, ref dRealLeaveTotalHours);

            dLeaveTotalHours = dRealLeaveTotalHours;
        }

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
                if (dValue == 0)
                {
                    return dRes;
                }

                dRes = decimal.Round(dValue, ilength);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }

            return dRes;
        }

        #region 计算加班，出差，外出申请时长
        /// <summary>
        /// 计算指定员工一段时间内的加班情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="strCheckState"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="iOverTimeTimes"></param>
        /// <param name="dOverTimeSumHours"></param>
        /// <param name="dOvertimeSumDays"></param>
        private void CalculateEmployeeOverTimeDays(T_HR_ATTENDANCESOLUTION entAttSol, string strEmployeeID, DateTime dtStart, DateTime dtEnd, string strCheckState,
            decimal? dWorkTimePerDay, ref int iOverTimeTimes, ref decimal? dOverTimeSumHours, ref decimal? dOvertimeSumDays)
        {
            try
            {
                if (entAttSol.OVERTIMEPAYTYPE == (Convert.ToInt32(Common.OverTimePayType.NoPay) + 1).ToString())
                {
                    return;
                }

                T_HR_OVERTIMEREWARD entOvertimereward = entAttSol.T_HR_OVERTIMEREWARD;  //获取员工加班报酬倍率设置记录

                EmployeeBLL bllEmployee = new EmployeeBLL();
                V_EMPLOYEEPOST entEmployeeDetail = bllEmployee.GetEmployeeDetailByID(strEmployeeID);    //获取员工的关联信息，如公司，岗位，部门，个人信息等

                //获取员工 所在地区/国家
                string strCountryType = "1";    //默认为"1"，即中国
                if (entEmployeeDetail.EMPLOYEEPOSTS != null)
                {
                    if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST != null)
                    {
                        if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT != null)
                        {
                            if (entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY != null)
                            {
                                if (!string.IsNullOrWhiteSpace(entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COUNTYTYPE))
                                {
                                    strCountryType = entEmployeeDetail.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COUNTYTYPE;
                                }
                            }
                        }
                    }
                }
                int iWorkMode = int.Parse(entAttSol.WORKMODE.Value.ToString());                                                 //获取当前应用的考勤方案的工作制,即每周工作几天

                List<int> iWeekDays = GetWeekDayList(iWorkMode, dtStart, dtEnd);        //获取员工当月休假日序号
                List<int> iVacDays = GetVacDayList(strCountryType, dtStart, dtEnd);     //获取员工当月休息日序号

                //获取员工的加班记录(审核通过)
                OverTimeRecordBLL bllOverTimeRecord = new OverTimeRecordBLL();
                IQueryable<T_HR_EMPLOYEEOVERTIMERECORD> entEmpOverTimeRds = bllOverTimeRecord.GetOverTimeRdListByEmployeeIDAndDate(strEmployeeID, dtStart, dtEnd, strCheckState);

                //检查当前考勤方案是否要求加班打卡
                if (entAttSol.OVERTIMECHECK == (Convert.ToInt32(Common.IsChecked.No) + 1).ToString())
                {
                    //要求加班打卡，则检查当前考勤方案的加班生效方式(0 审核通过的加班申请；1 超过工作时间外自动累计；2 仅节假日累计；)
                    //判断当前加班生效方式是否为：0 审核通过的加班申请；
                    if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.ToCheck) + 1).ToString())
                    {
                        if (entEmpOverTimeRds.Count() == 0)
                        {
                            return;
                        }

                        iOverTimeTimes = entEmpOverTimeRds.Count();

                        foreach (T_HR_EMPLOYEEOVERTIMERECORD entEmpOverTimeRd in entEmpOverTimeRds)
                        {
                            DateTime dtNoCheckStartDate = entEmpOverTimeRd.STARTDATE.Value;
                            DateTime dtNoCheckEndDate = entEmpOverTimeRd.STARTDATE.Value;
                            string strStartTime = entEmpOverTimeRd.STARTDATETIME;
                            string strEndTime = entEmpOverTimeRd.ENDDATETIME;

                            GetOTSumHoursByNoCheck(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtNoCheckStartDate, dtNoCheckEndDate, strStartTime, strEndTime, ref dOverTimeSumHours);
                        }
                    }
                }
                else
                {
                    //预设加班报酬倍率变量，以便后面赋值使用
                    decimal? dVacPayRate = 1;

                    //要求加班打卡，则检查当前考勤方案的加班生效方式(0 审核通过的加班申请；1 超过工作时间外自动累计；2 仅节假日累计；)
                    //判断当前加班生效方式是否为：0 审核通过的加班申请；
                    if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.ToCheck) + 1).ToString())
                    {
                        //当前加班生效方式为：0 审核通过的加班申请
                        //无加班申请，返回
                        if (entEmpOverTimeRds.Count() == 0)
                        {
                            return;
                        }

                        foreach (T_HR_EMPLOYEEOVERTIMERECORD entEmpOverTimeRd in entEmpOverTimeRds)
                        {
                            DateTime dtEmpOTStartDate = entEmpOverTimeRd.STARTDATE.Value;
                            DateTime dtEmpOTEndDate = entEmpOverTimeRd.ENDDATE.Value;

                            IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                       where c.PUNCHDATE >= entEmpOverTimeRd.STARTDATE && c.PUNCHDATE <= entEmpOverTimeRd.ENDDATE
                                                                                       orderby c.PUNCHDATE
                                                                                       select c;
                            //无打卡记录，转向下一条记录
                            if (entClockInRecords.Count() == 0)
                            {
                                continue;
                            }

                            //当日加班，加班次数累加1次
                            if (entEmpOverTimeRd.STARTDATE == entEmpOverTimeRd.ENDDATE)
                            {
                                iOverTimeTimes += 1;
                            }

                            //重新填充打卡记录，打卡日期附加打卡时间
                            List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                            foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                            {
                                entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                                entTemps.Add(entTempRd);
                            }

                            var cls = from c in entTemps
                                      orderby c.PUNCHDATE
                                      select c;

                            T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                            T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                            TimeSpan tsStart = new TimeSpan(dtEmpOTStartDate.Ticks);
                            TimeSpan tsEnd = new TimeSpan(dtEmpOTEndDate.Ticks);
                            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                            if (ts.Days == 0)
                            {
                                decimal dHours = 0;
                                GetOTSumHoursByLessOT(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtEmpOTStartDate, ts, ref dOverTimeSumHours, ref dVacPayRate, ref dHours);
                            }
                            else
                            {
                                for (int i = 0; i < ts.Days; i++)
                                {
                                    DateTime dtCheck = DateTime.Parse(dtEmpOTStartDate.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                    DateTime dtCheckEnd = DateTime.Parse(dtEmpOTStartDate.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                    //检查加班日是否为节假日
                                    if (iVacDays.Contains(dtCheck.Day))
                                    {
                                        iOverTimeTimes += 1;
                                        //获取节假日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                    else
                                    {
                                        //检查加班日是否为休息日
                                        if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                                        {
                                            iOverTimeTimes += 1;
                                            //获取休息日加班报酬倍率
                                            if (entOvertimereward != null)
                                            {
                                                dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                            }

                                            GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                        }
                                        else
                                        {
                                            iOverTimeTimes += 1;
                                            //获取工作日加班报酬倍率
                                            if (entOvertimereward != null)
                                            {
                                                dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                            }

                                            GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.AutoAccumulate) + 1).ToString())
                    {
                        IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                   where c.PUNCHDATE >= dtStart && c.PUNCHDATE <= dtEnd
                                                                                   orderby c.PUNCHDATE
                                                                                   select c;
                        //无打卡记录，转向下一条记录
                        if (entClockInRecords.Count() == 0)
                        {
                            return;
                        }

                        List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                        foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                        {
                            entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                            entTemps.Add(entTempRd);
                        }

                        var cls = from c in entTemps
                                  orderby c.PUNCHDATE
                                  select c;

                        T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                        T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                        TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
                        TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
                        TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                        if (ts.Days > 0)
                        {
                            for (int i = 0; i < ts.Days; i++)
                            {
                                DateTime dtCheck = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                DateTime dtCheckEnd = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                //检查加班日是否为节假日
                                if (iVacDays.Contains(dtCheck.Day))
                                {
                                    iOverTimeTimes += 1;
                                    //获取节假日加班报酬倍率
                                    if (entOvertimereward != null)
                                    {
                                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                    }

                                    GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                }
                                else
                                {
                                    //检查加班日是否为休息日
                                    if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                                    {
                                        iOverTimeTimes += 1;
                                        //获取休息日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                    else
                                    {
                                        iOverTimeTimes += 1;
                                        //获取工作日加班报酬倍率
                                        if (entOvertimereward != null)
                                        {
                                            dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                        }

                                        GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                    }
                                }
                            }
                        }
                    }
                    else if (entAttSol.OVERTIMEVALID == (Convert.ToInt32(Common.OverTimeValid.OnlyHoliday) + 1).ToString())
                    {
                        IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRecords = from c in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                                                                                   where c.PUNCHDATE >= dtStart && c.PUNCHDATE <= dtEnd
                                                                                   orderby c.PUNCHDATE
                                                                                   select c;
                        //无打卡记录，转向下一条记录
                        if (entClockInRecords.Count() == 0)
                        {
                            return;
                        }

                        List<T_HR_EMPLOYEECLOCKINRECORD> entTemps = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                        foreach (T_HR_EMPLOYEECLOCKINRECORD entTempRd in entClockInRecords)
                        {
                            entTempRd.PUNCHDATE = DateTime.Parse(entTempRd.PUNCHDATE.Value.ToString("yyyy-MM-dd") + " " + entTempRd.PUNCHTIME);
                            entTemps.Add(entTempRd);
                        }

                        var cls = from c in entTemps
                                  orderby c.PUNCHDATE
                                  select c;

                        T_HR_EMPLOYEECLOCKINRECORD entFirst = entTemps.FirstOrDefault();
                        T_HR_EMPLOYEECLOCKINRECORD entLast = entTemps.LastOrDefault();

                        TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
                        TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
                        TimeSpan ts = tsEnd.Subtract(tsStart).Duration();
                        if (ts.Days > 0)
                        {
                            for (int i = 0; i < ts.Days; i++)
                            {
                                DateTime dtCheck = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "0:00:00");
                                DateTime dtCheckEnd = DateTime.Parse(dtStart.AddDays(i).ToString("yyyy-MM-dd") + "23:59:59");

                                //检查加班日是否为节假日
                                if (iVacDays.Contains(dtCheck.Day))
                                {
                                    iOverTimeTimes += 1;
                                    //获取节假日加班报酬倍率
                                    if (entOvertimereward != null)
                                    {
                                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                                    }

                                    GetOverTimeSumHours(dWorkTimePerDay, dVacPayRate, entTemps, dtCheck, dtCheckEnd, ref dOverTimeSumHours);
                                }
                            }
                        }
                    }
                }

                dWorkTimePerDay = dWorkTimePerDay.Value * 60;
                dOvertimeSumDays = decimal.Round(dOverTimeSumHours.Value / dWorkTimePerDay.Value, 0);
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的出差情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dEvectionTime"></param>
        private void CalculateEmployeeEvectionTime(T_HR_ATTENDANCESOLUTION entAttSol, IQueryable<T_HR_ATTENDANCERECORD> entAttRds, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal? dWorkTimePerDay, ref decimal? dEvectionTime)
        {
            try
            {
                string strAttState = (Convert.ToInt32(Common.AttendanceState.Travel) + 1).ToString();
                //考勤结算时计算同时存在考勤异常和出差天数
                string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixTravelAbnormal) + 1).ToString();
                IQueryable<T_HR_ATTENDANCERECORD> entAttRdTemps = from r in entAttRds
                                                                  where r.ATTENDANCESTATE == strAttState
                                                                  || r.ATTENDANCESTATE == strAttendStateMix
                                                                  select r;

                if (entAttRdTemps.Count() == 0)
                {
                    return;
                }
                else
                {
                    dEvectionTime = entAttRdTemps.Count();
                }

                //decimal? dCurEvecDays = entAttRdTemps.Count();

                //IQueryable<T_HR_EMPLOYEEEVECTIONRECORD> entEvecRds = from n in dal.GetObjects<T_HR_EMPLOYEEEVECTIONRECORD>()
                //                                                     where n.EMPLOYEEID == strEmployeeID 
                //                                                     && n.STARTDATE>=dtStart
                //                                                     && n.ENDDATE <=dtEnd
                //                                                     select n;

                //if (entEvecRds.Count() == 0)
                //{
                //    return;
                //}
                //List<string> startDayList = new List<string>();
                //foreach (T_HR_EMPLOYEEEVECTIONRECORD item in entEvecRds)
                //{
                //    string starDay=item.STARTDATE.Value.ToString("yyyy-MM-dd");
                //    if (startDayList.Contains(starDay))
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        dEvectionTime += item.TOTALDAYS;
                //        startDayList.Add(starDay);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算指定员工一段时间内的外出申请情况
        /// </summary>
        /// <param name="entAttSol"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dOutApplyHours"></param>
        private void CalculateEmployeeOutApplyTime(T_HR_ATTENDANCESOLUTION entAttSol, IQueryable<T_HR_ATTENDANCERECORD> entAttRds, string strEmployeeID, DateTime dtStart, DateTime dtEnd, decimal? dWorkTimePerDay, ref decimal? dOutApplyHours)
        {
            try
            {
                string strAttState = (Convert.ToInt32(Common.AttendanceState.OutApply) + 1).ToString();
                //考勤结算时计算同时存在考勤异常和出差天数
                string strAttendStateMix = (Convert.ToInt32(Common.AttendanceState.MixOutApplyAbnormal) + 1).ToString();
                IQueryable<T_HR_ATTENDANCERECORD> entAttRdTemps = from r in entAttRds
                                                                  where r.ATTENDANCESTATE == strAttState
                                                                  || r.ATTENDANCESTATE == strAttendStateMix
                                                                  select r;

                if (entAttRdTemps.Count() == 0)
                {
                    return;
                }

                //decimal? dCurEvecDays = entAttRdTemps.Count();

                IQueryable<T_HR_OUTAPPLYRECORD> entEvecRds = from n in dal.GetObjects<T_HR_OUTAPPLYRECORD>()
                                                                     where n.EMPLOYEEID == strEmployeeID 
                                                                     && n.STARTDATE >= dtStart
                                                                     && n.ENDDATE<=dtEnd
                                                                     && n.CHECKSTATE=="2"
                                                                     select n;

                if (entEvecRds.Count() == 0)
                {
                    return;
                }
                //decimal? dCheckEveDays = 0;
                //DateTime dtCheckStart = new DateTime(), dtCheckEnd = new DateTime();
                foreach (T_HR_OUTAPPLYRECORD item in entEvecRds)
                {
                    decimal dHOURS = 0;
                    decimal.TryParse(item.OUTAPLLYTIMES,out dHOURS);
                    dOutApplyHours += dHOURS;
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 无需打卡，但需要审核的加班，计算其加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="entOvertimereward"></param>
        /// <param name="iWeekDays"></param>
        /// <param name="iVacDays"></param>
        /// <param name="dtStartDate"></param>
        /// <param name="dtEndDate"></param>
        /// <param name="strStartTime"></param>
        /// <param name="strEndTime"></param>
        /// <param name="dOverTimeSumHours"></param>
        private static void GetOTSumHoursByNoCheck(decimal? dWorkTimePerDay, T_HR_OVERTIMEREWARD entOvertimereward, List<int> iWeekDays, List<int> iVacDays,
            DateTime dtStartDate, DateTime dtEndDate, string strStartTime, string strEndTime, ref decimal? dOverTimeSumHours)
        {
            try
            {
                decimal? dVacPayRate = 1;
                decimal dHours = 0;

                dtStartDate = DateTime.Parse(dtStartDate.ToString("yyyy-MM-dd") + strStartTime);
                dtEndDate = DateTime.Parse(dtEndDate.ToString("yyyy-MM-dd") + strEndTime);

                TimeSpan tsStart = new TimeSpan(dtStartDate.Ticks);
                TimeSpan tsEnd = new TimeSpan(dtEndDate.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                if (ts.Days == 0)
                {
                    GetOTSumHoursByLessOT(dWorkTimePerDay, entOvertimereward, iWeekDays, iVacDays, dtStartDate, ts, ref dOverTimeSumHours, ref dVacPayRate, ref dHours);
                }
                else
                {
                    int iTotalDays = ts.Days;
                    if (ts.Hours >= dWorkTimePerDay.Value)
                    {
                        iTotalDays += 1;
                    }
                    else
                    {
                        dHours = ts.Hours;
                    }

                    for (int i = 0; i < iTotalDays; i++)
                    {
                        DateTime dtCheck = dtStartDate.AddDays(i);
                        //检查加班日是否为节假日
                        if (iVacDays.Contains(dtCheck.Day))
                        {
                            //获取节假日加班报酬倍率
                            if (entOvertimereward != null)
                            {
                                dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                            }

                            dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                        }
                        else
                        {
                            //检查加班日是否为休息日
                            if (iWeekDays.Contains(Convert.ToInt32(dtCheck.DayOfWeek)))
                            {
                                //获取休息日加班报酬倍率
                                if (entOvertimereward != null)
                                {
                                    dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                                }

                                dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                            }
                            else
                            {
                                //获取工作日加班报酬倍率
                                if (entOvertimereward != null)
                                {
                                    dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                                }

                                dOverTimeSumHours += (dHours + dWorkTimePerDay) * dVacPayRate;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 计算少于一天的加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="entOvertimereward"></param>
        /// <param name="iWeekDays"></param>
        /// <param name="iVacDays"></param>
        /// <param name="dtStartDate"></param>
        /// <param name="ts"></param>
        /// <param name="dOverTimeSumHours"></param>
        /// <param name="dVacPayRate"></param>
        /// <param name="dHours"></param>
        private static void GetOTSumHoursByLessOT(decimal? dWorkTimePerDay, T_HR_OVERTIMEREWARD entOvertimereward, List<int> iWeekDays, List<int> iVacDays,
            DateTime dtStartDate, TimeSpan ts, ref decimal? dOverTimeSumHours, ref decimal? dVacPayRate, ref decimal dHours)
        {
            try
            {
                if (ts.Hours >= dWorkTimePerDay.Value)
                {
                    dHours = dWorkTimePerDay.Value;
                }
                else
                {
                    dHours = ts.Hours;
                }

                //检查加班日是否为节假日
                if (iVacDays.Contains(dtStartDate.Day))
                {
                    //获取节假日加班报酬倍率
                    if (entOvertimereward != null)
                    {
                        dVacPayRate = entOvertimereward.VACATIONPAYRATE.Value == 0 ? 0 : entOvertimereward.VACATIONPAYRATE;
                    }

                    dOverTimeSumHours += dHours * dVacPayRate;
                }
                else
                {
                    //检查加班日是否为休息日
                    if (iWeekDays.Contains(Convert.ToInt32(dtStartDate.DayOfWeek)))
                    {
                        //获取休息日加班报酬倍率
                        if (entOvertimereward != null)
                        {
                            dVacPayRate = entOvertimereward.WEEKENDPAYRATE.Value == 0 ? 0 : entOvertimereward.WEEKENDPAYRATE;
                        }

                        dOverTimeSumHours += dHours * dVacPayRate;
                    }
                    else
                    {
                        //获取工作日加班报酬倍率
                        if (entOvertimereward != null)
                        {
                            dVacPayRate = entOvertimereward.USUALOVERTIMEPAYRATE.Value == 0 ? 0 : entOvertimereward.USUALOVERTIMEPAYRATE;
                        }

                        dOverTimeSumHours += dHours * dVacPayRate;
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 根据起止时间，获取当日的加班打卡记录，并根据报酬倍率，计算返回实际的加班时长
        /// </summary>
        /// <param name="dWorkTimePerDay"></param>
        /// <param name="dVacPayRate"></param>
        /// <param name="entTemps"></param>
        /// <param name="dtCheck"></param>
        /// <param name="dtCheckEnd"></param>
        /// <param name="dOverTimeSumHours"></param>
        private static void GetOverTimeSumHours(decimal? dWorkTimePerDay, decimal? dVacPayRate, List<T_HR_EMPLOYEECLOCKINRECORD> entTemps,
            DateTime dtCheck, DateTime dtCheckEnd, ref decimal? dOverTimeSumHours)
        {
            try
            {
                //获取当日的加班打卡记录
                var vcs = from n in entTemps
                          where n.PUNCHDATE >= dtCheck && n.PUNCHDATE <= dtCheckEnd
                          orderby n.PUNCHDATE
                          select n;

                T_HR_EMPLOYEECLOCKINRECORD entVacFirst = vcs.FirstOrDefault();
                T_HR_EMPLOYEECLOCKINRECORD entVacLast = vcs.LastOrDefault();

                TimeSpan tsVacStart = new TimeSpan(dtCheck.Ticks);
                TimeSpan tsVacEnd = new TimeSpan(dtCheckEnd.Ticks);
                TimeSpan tsVac = tsVacEnd.Subtract(tsVacStart).Duration();

                if (tsVac.Hours >= dWorkTimePerDay.Value)
                {
                    dOverTimeSumHours += dWorkTimePerDay * dVacPayRate;
                }
                else
                {
                    dOverTimeSumHours += tsVac.Hours * dVacPayRate;
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取当月，所在地区/国家的节假日
        /// </summary>
        /// <param name="strCountryType">所在地区/国家</param>
        /// <param name="dtStart">当月起始日</param>
        /// <param name="dtEnd">当月截止日</param>
        /// <returns>返回节假日序号数组</returns>
        private List<int> GetVacDayList(string strCountryType, DateTime dtStart, DateTime dtEnd)
        {
            List<int> entVacList = new List<int>();

            var qv = from v in dal.GetObjects<T_HR_OUTPLANDAYS>().Include("T_HR_VACATIONSET")
                     where v.T_HR_VACATIONSET.COUNTYTYPE == strCountryType && v.STARTDATE >= dtStart && v.ENDDATE <= dtEnd
                     select v;

            if (qv.Count() == 0)
            {
                return entVacList;
            }

            foreach (T_HR_OUTPLANDAYS item in qv)
            {
                TimeSpan tsStart = new TimeSpan(item.STARTDATE.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(item.ENDDATE.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                if (ts.Days == 0)
                {
                    entVacList.Add(item.STARTDATE.Value.Day);
                }
                else
                {
                    for (int i = 0; i < ts.Days; i++)
                    {
                        int j = item.STARTDATE.Value.Day + i;
                        entVacList.Add(j);
                    }
                }
            }

            return entVacList;
        }

        /// <summary>
        /// 获取当月，普通
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        private List<int> GetWeekDayList(int iWorkMode, DateTime dtStart, DateTime dtEnd)
        {
            List<int> entWeekDayList = new List<int>();

            switch (iWorkMode)
            {
                case 1:
                    entWeekDayList.Add(1);
                    break;
                case 2:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    break;
                case 3:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    break;
                case 4:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    break;
                case 5:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    break;
                case 6:
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    entWeekDayList.Add(6);
                    break;
                case 7:
                    entWeekDayList.Add(0);
                    entWeekDayList.Add(1);
                    entWeekDayList.Add(2);
                    entWeekDayList.Add(3);
                    entWeekDayList.Add(4);
                    entWeekDayList.Add(5);
                    entWeekDayList.Add(6);
                    break;
            }
            return entWeekDayList;
        }
        #endregion

        #endregion


        #region 导出报表
        /// <summary>
        /// 导出考勤报表
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strOwnerID"></param>
        /// <param name="strCheckState"></param>
        /// <param name="strEmployeeID"></param>
        /// <param name="dBalanceYear"></param>
        /// <param name="dBalanceMonth"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public byte[] ExportAttendMonthlyBalanceReports(string sType, string sValue, string strOwnerID, string strCheckState, string strEmployeeID, decimal dBalanceYear, decimal dBalanceMonth, string strSortKey)
        {

            byte[] result = null;
            try
            {

                List<T_HR_ATTENDMONTHLYBALANCE> entlist = new List<T_HR_ATTENDMONTHLYBALANCE>();
                IQueryable<T_HR_ATTENDMONTHLYBALANCE> employeeInfos = ExportAttendMonthlyBalanceRdListByMultSearch(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey);
                if (employeeInfos.Count() > 0)
                {
                    entlist = employeeInfos.ToList();
                }

                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == sValue
                           select ent;
                string CompanyName = "";
                string StrDate = "";
                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = dBalanceYear.ToString() + "年" + dBalanceMonth.ToString() + "月";

                result = OutEmployeeAttendStream(CompanyName, StrDate, entlist);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeftOfficeConfirmReports:" + ex.Message);

            }
            return result;


        }

        public static byte[] OutEmployeeAttendStream(string CompanyName, string Strdate, List<T_HR_ATTENDMONTHLYBALANCE> EmployeeInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeAttendBody(CompanyName, Strdate, EmployeeInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        public static StringBuilder GetEmployeeAttendBody(string CompanyName, string Strdate, List<T_HR_ATTENDMONTHLYBALANCE> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" BORDER=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"22\">" + CompanyName + "产业单位" + Strdate + "考勤统计备案表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"21\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >序号</td>");
            s.Append("<td align=center class=\"title\" >员工姓名</td>");
            s.Append("<td align=center class=\"title\" >结算年份</td>");
            s.Append("<td align=center class=\"title\" >结算月份</td>");
            s.Append("<td align=center class=\"title\" >结算时间</td>");
            s.Append("<td align=center class=\"title\" >应出勤天数</td>");

            s.Append("<td align=center class=\"title\" >实际出勤天数</td>");
            s.Append("<td align=center class=\"title\" >漏打卡次数</td>");
            s.Append("<td align=center class=\"title\" >迟到次数</td>");
            s.Append("<td align=center class=\"title\" >早退次数</td>");
            s.Append("<td align=center class=\"title\" >旷工天数</td>");

            s.Append("<td align=center class=\"title\" >年休假天数</td>");
            s.Append("<td align=center class=\"title\" >调休假天数</td>");
            s.Append("<td align=center class=\"title\" >事假天数</td>");
            s.Append("<td align=center class=\"title\" >病假天数</td>");
            s.Append("<td align=center class=\"title\" >婚假天数</td>");

            s.Append("<td align=center class=\"title\" >产假天数</td>");
            s.Append("<td align=center class=\"title\" >看护假天数</td>");
            s.Append("<td align=center class=\"title\" >路程假天数</td>");
            s.Append("<td align=center class=\"title\" >工伤假天数</td>");
            s.Append("<td align=center class=\"title\" >产前检查假天数</td>");            
            s.Append("<td align=center class=\"title\" >丧假天数</td>");
            s.Append("<td align=center class=\"title\" >出差天数</td>");

            s.Append("</tr>");




            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (i + 1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BALANCEYEAR + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BALANCEMONTH + "</td>");
                    if (Collects[i].BALANCEDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].BALANCEDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }

                    //s.Append("<td class=\"x1282\">" + Collects[i].NEEDATTENDDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].REALNEEDATTENDDAYS + "</td>");//应出勤天数，注释的为原来的                    
                    s.Append("<td class=\"x1282\">" + Collects[i].REALATTENDDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].FORGETCARDTIMES + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LATETIMES + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVEEARLYTIMES + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ABSENTDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ANNUALLEVELDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVEUSEDDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].AFFAIRLEAVEDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SICKLEAVEDAYS + "</td>");//病假
                    s.Append("<td class=\"x1282\">" + Collects[i].MARRYDAYS + "</td>");//婚假
                    s.Append("<td class=\"x1282\">" + Collects[i].MATERNITYLEAVEDAYS + "</td>");//产假
                    s.Append("<td class=\"x1282\">" + Collects[i].NURSESDAYS + "</td>");//看护假
                    s.Append("<td class=\"x1282\">" + Collects[i].TRIPDAYS + "</td>");//路程假
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYLEAVEDAYS + "</td>");//公伤假
                    s.Append("<td class=\"x1282\">" + Collects[i].PRENATALCARELEAVEDAYS + "</td>");//产前检查假
                    s.Append("<td class=\"x1282\">" + Collects[i].FUNERALLEAVEDAYS + "</td>");//丧假
                    s.Append("<td class=\"x1282\">" + Collects[i].EVECTIONTIME + "</td>");//出差天数




                    s.Append("</tr>");
                }
            }



            s.Append("</table>");

            s.Append("</body></html>");
            return s;
        }



        #endregion
    }
}
