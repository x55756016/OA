﻿/*
 * 文件名：VacationSetBLL.cs
 * 作  用：公共假期设置业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010年2月10日, 9:18:45
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

using SMT.HRM.CustomModel;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    public class VacationSetBLL : BaseBll<T_HR_VACATIONSET>
    {
        public VacationSetBLL()
        {

        }

        #region 获取数据

        /// <summary>
        /// 获取公共假期设置信息
        /// </summary>
        /// <param name="strVacationSetId">主键索引</param>
        /// <returns></returns>
        public T_HR_VACATIONSET GetVacationSetByID(string strVacationSetId)
        {
            if (string.IsNullOrEmpty(strVacationSetId))
            {
                return null;
            }

            VacationSetDAL dalVacationSet = new VacationSetDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strVacationSetId))
            {
                strfilter.Append(" VACATIONID == @0");
                objArgs.Add(strVacationSetId);
            }

            T_HR_VACATIONSET entVacRd = dalVacationSet.GetVacationSetRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entVacRd;
        }        

        /// <summary>
        /// 根据条件，获取公共假期设置信息
        /// </summary>
        /// <param name="strVacName"></param>
        /// <param name="strVacYear"></param>
        /// <param name="strCountyType"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_VACATIONSET> GetAllVacationSetRdListByMultSearch(string strOwnerID, string strVacName, string strVacYear, string strCountyType, string strSortKey)
        {
            VacationSetDAL dalVacationSet = new VacationSetDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strVacName))
            {
                strfilter.Append(" @0.Contains(VACATIONNAME)");
                objArgs.Add(strVacName);
            }

            if (!string.IsNullOrEmpty(strVacYear))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" VACATIONYEAR == @" + iIndex.ToString());
                objArgs.Add(strVacYear);
            }

            if (!string.IsNullOrEmpty(strCountyType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" &&");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" COUNTYTYPE == @" + iIndex.ToString());
                objArgs.Add(strCountyType);
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_VACATIONSET");


            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " COUNTYTYPE ";
            }

            var q = dalVacationSet.GetVacationSetRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取公共假期设置信息,并进行分页
        /// </summary>
        /// <param name="strVacName">假期名称</param>
        /// <param name="strVacYear">假期生效年份</param>
        /// <param name="strCountyType">假期执行国家</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>公共假期信息</returns>
        public IQueryable<T_HR_VACATIONSET> GetVacationSetRdListByMultSearch(string strOwnerID, string strVacName, string strVacYear, string strCountyType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllVacationSetRdListByMultSearch(strOwnerID, strVacName, strVacYear, strCountyType, strSortKey);

            return Utility.Pager<T_HR_VACATIONSET>(q, pageIndex, pageSize, ref pageCount);
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增公共假期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        public string AddVacationSet(T_HR_VACATIONSET entVacRd)
        {
            string strMsg = string.Empty;
            try
            {
                if (entVacRd == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" VACATIONNAME == @0");
                strFilter.Append(" && COUNTYTYPE == @1");
                strFilter.Append(" && VACATIONYEAR == @2");

                objArgs.Add(entVacRd.VACATIONNAME);
                objArgs.Add(entVacRd.COUNTYTYPE);
                objArgs.Add(entVacRd.VACATIONYEAR);

                VacationSetDAL dalVacationSet = new VacationSetDAL();
                flag = dalVacationSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                List<T_HR_OUTPLANDAYS> entOutPlanDays = entVacRd.T_HR_OUTPLANDAYS.ToList();

                T_HR_VACATIONSET entAdd = new T_HR_VACATIONSET();
                Utility.CloneEntity(entVacRd, entAdd);
                dalVacationSet.Add(entAdd);

                if (entOutPlanDays.Count() > 0)
                {
                    OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                    foreach (T_HR_OUTPLANDAYS item in entOutPlanDays)
                    {
                        if (item.EntityKey != null)
                        {
                            item.EntityKey = null;  //清除EntityKey不为null的情况
                        }

                        if (item.T_HR_VACATIONSET == null)
                        {
                            item.T_HR_VACATIONSET = entAdd;
                        }

                        if (!string.IsNullOrEmpty(item.ISADJUSTLEAVE))
                        {
                            if (item.ISADJUSTLEAVE.ToUpper() == "TRUE" || item.ISADJUSTLEAVE == "1")
                            {
                                item.ISADJUSTLEAVE = "1";
                            }
                            else
                            {
                                item.ISADJUSTLEAVE = "0";
                            }
                        }

                        bllOutPlanDays.AddOutPlanDays(item);
                    }
                }


                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改公共假期设置信息
        /// </summary>
        /// <param name="entVacRd"></param>
        /// <returns></returns>
        public string ModifyVacationSet(T_HR_VACATIONSET entVacRd)
        {
            string strMsg = string.Empty;
            try
            {
                if (entVacRd == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" VACATIONID == @0");

                objArgs.Add(entVacRd.VACATIONID);

                VacationSetDAL dalVacationSet = new VacationSetDAL();
                flag = dalVacationSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                List<T_HR_OUTPLANDAYS> entOutPlanDays = entVacRd.T_HR_OUTPLANDAYS.ToList();

                T_HR_VACATIONSET entUpdate = dalVacationSet.GetVacationSetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entVacRd, entUpdate);

                dalVacationSet.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";

                if (entOutPlanDays == null)
                {
                    return strMsg;
                }

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                bllOutPlanDays.DeleteByVacationID(entVacRd.VACATIONID);

                if (entOutPlanDays.Count() == 0)
                {
                    return strMsg;
                }

                foreach (T_HR_OUTPLANDAYS item in entOutPlanDays)
                {
                    if (item.EntityKey != null)
                    {
                        item.EntityKey = null;  //清除EntityKey不为null的情况
                    }

                    if (item.T_HR_VACATIONSET == null)
                    {
                        item.T_HR_VACATIONSET = entVacRd;
                    }

                    if (!string.IsNullOrEmpty(item.ISADJUSTLEAVE))
                    {
                        if (item.ISADJUSTLEAVE.ToUpper() == "TRUE" || item.ISADJUSTLEAVE == "1")
                        {
                            item.ISADJUSTLEAVE = "1";
                        }
                        else
                        {
                            item.ISADJUSTLEAVE = "0";
                        }
                    }

                    bllOutPlanDays.AddOutPlanDays(item);
                }

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除公共假期设置信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteVacationSet(string strVacationId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strVacationId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" VACATIONID == @0");

                objArgs.Add(strVacationId);

                VacationSetDAL dalVacationSet = new VacationSetDAL();
                flag = dalVacationSet.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                OutPlanDaysBLL bllOutPlanDays = new OutPlanDaysBLL();
                bllOutPlanDays.DeleteByVacationID(strVacationId);

                T_HR_VACATIONSET entDel = dalVacationSet.GetVacationSetRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalVacationSet.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion        
    }
}