
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
    public partial class AttendMonthlyBalanceBLL : BaseBll<T_HR_ATTENDMONTHLYBALANCE>
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
                        entTemp.REALNEEDATTENDDAYS = GetDecimalValue(strLine[2]); //应出勤天数
                        entTemp.REALATTENDDAYS = GetDecimalValue(strLine[3]); //实际出勤天数

                        GetEmployeeWorkServiceMonths(entCurrEmp.EMPLOYEEID, ref dWorkServiceMonths);
                        entTemp.WORKSERVICEMONTHS = dWorkServiceMonths;

                        entTemp.FORGETCARDTIMES = GetDecimalValue(strLine[4]); //漏打卡次数
                        entTemp.LATETIMES = GetDecimalValue(strLine[5]); //迟到次数
                        entTemp.LATEMINUTES = GetDecimalValue(strLine[6]); //迟到时长

                        entTemp.LEAVEEARLYTIMES = GetDecimalValue(strLine[7]); //早退次数

                        entTemp.ABSENTDAYS = GetDecimalValue(strLine[8]); //旷工天数

                        entTemp.ABSENTMINUTES = GetDecimalValue(strLine[9]); //矿工时长

                        entTemp.ANNUALLEVELDAYS = GetDecimalValue(strLine[10]); //年休假天数

                        entTemp.LEAVEUSEDDAYS = GetDecimalValue(strLine[11]); //调休假天数

                        entTemp.AFFAIRLEAVEDAYS = GetDecimalValue(strLine[12]); //事假天数
                        entTemp.SICKLEAVEDAYS = GetDecimalValue(strLine[13]); //病假天数
                        entTemp.MARRYDAYS = GetDecimalValue(strLine[14]); //婚假天数
                        entTemp.MATERNITYLEAVEDAYS = GetDecimalValue(strLine[15]); //产假天数
                        entTemp.NURSESDAYS = GetDecimalValue(strLine[16]); //看护假天数

                        entTemp.TRIPDAYS = GetDecimalValue(strLine[17]); //路程假天数
                        entTemp.INJURYLEAVEDAYS = GetDecimalValue(strLine[18]); //工伤假天数
                        entTemp.PRENATALCARELEAVEDAYS = GetDecimalValue(strLine[19]); //产前检查假天数
                        entTemp.FUNERALLEAVEDAYS = GetDecimalValue(strLine[20]);  //丧假天数

                        entTemp.OTHERLEAVEDAYS = entTemp.AFFAIRLEAVEDAYS + entTemp.SICKLEAVEDAYS + entTemp.ANNUALLEVELDAYS + entTemp.LEAVEUSEDDAYS
                            + entTemp.MARRYDAYS + entTemp.MATERNITYLEAVEDAYS + entTemp.NURSESDAYS + entTemp.FUNERALLEAVEDAYS + entTemp.TRIPDAYS
                            + entTemp.INJURYLEAVEDAYS + entTemp.PRENATALCARELEAVEDAYS;


                        entTemp.EVECTIONTIME = GetDecimalValue(strLine[21]); //出差时长
                        entTemp.OUTAPPLYTIME = GetDecimalValue(strLine[22]); //外出时长


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
                        //if (strLine.Length > 22)
                        //{
                        //    entTemp.REMARK = strLine[22];
                        //}
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

                    T_HR_ATTENDMONTHLYBALANCE entTemp = new T_HR_ATTENDMONTHLYBALANCE();
                    entTemp.EMPLOYEECODE = strLine[0];
                    entTemp.EMPLOYEENAME = strLine[1];
                    entTemp.NEEDATTENDDAYS = GetDecimalValue(strLine[2]);
                    entTemp.REALATTENDDAYS = GetDecimalValue(strLine[3]);
                    entTemp.FORGETCARDTIMES = GetDecimalValue(strLine[4]);
                    entTemp.LATEDAYS = GetDecimalValue(strLine[5]);
                    entTemp.LATEMINUTES = GetDecimalValue(strLine[6]);
                    entTemp.LEAVEEARLYDAYS = GetDecimalValue(strLine[7]);
                    entTemp.ABSENTDAYS = GetDecimalValue(strLine[8]);
                    entTemp.ABSENTMINUTES = GetDecimalValue(strLine[9]);
                    entTemp.ANNUALLEVELDAYS = GetDecimalValue(strLine[10]);
                    entTemp.LEAVEUSEDDAYS = GetDecimalValue(strLine[11]);
                    entTemp.AFFAIRLEAVEDAYS = GetDecimalValue(strLine[12]);
                    entTemp.SICKLEAVEDAYS = GetDecimalValue(strLine[13]);
                    entTemp.MARRYDAYS = GetDecimalValue(strLine[14]);
                    entTemp.MATERNITYLEAVEDAYS = GetDecimalValue(strLine[15]);
                    entTemp.NURSESDAYS = GetDecimalValue(strLine[16]);
                    entTemp.TRIPDAYS = GetDecimalValue(strLine[17]);
                    entTemp.INJURYLEAVEDAYS = GetDecimalValue(strLine[18]);
                    entTemp.PRENATALCARELEAVEDAYS = GetDecimalValue(strLine[19]);
                    entTemp.FUNERALLEAVEDAYS = GetDecimalValue(strLine[20]);
                    entTemp.EVECTIONTIME = GetDecimalValue(strLine[21]);
                    entTemp.OUTAPPLYTIME = GetDecimalValue(strLine[22]);

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

            s.Append("<td  align=center class=\"title\" colspan=\"26\">" + CompanyName + "产业单位" + Strdate + "考勤统计备案表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"25\">" + Strdate + "</td>");
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
            s.Append("<td align=center class=\"title\" >迟到时长(分钟)</td>");
            s.Append("<td align=center class=\"title\" >早退次数</td>");
            s.Append("<td align=center class=\"title\" >旷工天数</td>");
            s.Append("<td align=center class=\"title\" >旷工时长(分钟)</td>");
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
            s.Append("<td align=center class=\"title\" >外出时长</td>");
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
                    s.Append("<td class=\"x1282\">" + Collects[i].LATEMINUTES + "</td>"); //迟到时长（分钟）
                    s.Append("<td class=\"x1282\">" + Collects[i].LEAVEEARLYTIMES + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ABSENTDAYS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ABSENTMINUTES + "</td>"); //矿工时长(分钟)
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
                    s.Append("<td class=\"x1282\">" + Collects[i].OUTAPPLYTIME + "</td>");//外出时长

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
