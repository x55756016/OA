using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 自动打卡BLL
    /// </summary>
    public class ClockInRecordBLLTe : BaseBll<T_HR_EMPLOYEECLOCKINRECORD>
    {
        /// <summary>
        /// 自动打卡服务
        /// </summary>
        /// <param name="sType">类型</param>
        /// <param name="sValue">类型值</param>
        /// <param name="strOwnerID">员工ID</param>
        /// <param name="strEmployeeID">EmployeeID</param>
        /// <param name="strPunchDateFrom">打卡开始时间</param>
        /// <param name="strPunchDateTo">打卡结束时间</param>
        /// <param name="strTimeFrom">开始时间</param>
        /// <param name="strTimeTo">开始时间</param>
        /// <param name="strSortKey">key</param>
        /// <param name="pageIndex">分页</param>
        /// <param name="pageSize">大小</param>
        /// <param name="pageCount">行数</param>
        /// <returns>实体</returns>
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
                 this.SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEECLOCKINRECORD");
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
         /// 根据公司的ID，取得当前录入的员工打卡记录的最新打卡日期
         /// </summary>
         /// <param name="strCompanyId">公司ID</param>
         /// <returns>返回最新打卡日期</returns>
         public DateTime GetLatestPunchDateByCompanyId(string strCompanyId)
         {
             DateTime dtRes = new DateTime();
             if (string.IsNullOrWhiteSpace(strCompanyId))
             {
                 return dtRes;
             }

             IQueryable<T_HR_EMPLOYEECLOCKINRECORD> entClockInRds = this.GetAllClockInRdListByMultSearch("Company", strCompanyId, string.Empty, string.Empty, string.Empty, string.Empty, "PUNCHDATE DESC");
             if (entClockInRds.Count() == 0)
             {
                 return dtRes;
             }

             T_HR_EMPLOYEECLOCKINRECORD entClockInRd = entClockInRds.FirstOrDefault();

             return entClockInRd.PUNCHDATE.Value;
         }

        /// <summary>
         /// 导出员工的指定时间段的原始打卡记录
        /// </summary>
        /// <param name="sType">值类型</param>
         /// <param name="sValue">值</param>
         /// <param name="strOwnerID">OwnerID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strPunchDateFrom">打卡开始时间</param>
        /// <param name="strPunchDateTo">结束时间</param>
        /// <param name="strSortKey">key</param>
         /// <returns>T_HR_EMPLOYEECLOCKINRECORD实体</returns>
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
                 this.SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEECLOCKINRECORD");
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

        /// <summary>
        /// sql得到所以打卡信息
        /// </summary>
        /// <param name="sType">类型</param>
        /// <param name="sValue">值</param>
         /// <param name="strOwnerID">OwnerID</param>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="strPunchDateFrom">打卡开始时间</param>
        /// <param name="strPunchDateTo">打卡结束时间</param>
        /// <param name="strTimeFrom">开始时间</param>
        /// <param name="strTimeTo">结束时间</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">页面号</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="pageCount">总页数</param>
         /// <returns></returns>
         public List<T_HR_EMPLOYEECLOCKINRECORD> GetListAllClockInRdListBySql(string sType, string sValue, string strOwnerID, string strEmployeeID, string strPunchDateFrom,
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
                 this.SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEECLOCKINRECORD");
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
        /// 新增员工加班信息
        /// </summary>
        /// <param name="entClockInRd"></param>
        /// <returns>加班信息</returns>
        public string addClockInRecord(T_HR_EMPLOYEECLOCKINRECORD entTemp)
        {
            string strMsg = string.Empty;
            bool flag = false;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

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
    }
}
