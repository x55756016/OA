
/*
 * 文件名：SchedulingTemplateMasterBLL.cs
 * 作  用：排班模板 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-8 16:21:15
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

namespace SMT.HRM.BLL
{
    public class SchedulingTemplateMasterBLL : BaseBll<T_HR_SCHEDULINGTEMPLATEMASTER>, ILookupEntity
    {
        public SchedulingTemplateMasterBLL()
        { }

        #region 获取数据


        /// <summary>
        /// 获取排班模板信息
        /// </summary>
        /// <param name="strSchedulingTemplateMasterId">主键索引</param>
        /// <returns></returns>
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterByID(string strSchedulingTemplateMasterId)
        {
            if (string.IsNullOrEmpty(strSchedulingTemplateMasterId))
            {
                return null;
            }

            SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strSchedulingTemplateMasterId))
            {
                strfilter.Append(" TEMPLATEMASTERID == @0");
                objArgs.Add(strSchedulingTemplateMasterId);
            }

            T_HR_SCHEDULINGTEMPLATEMASTER entRd = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }

        /// <summary>
        /// 获取排班模板信息
        /// </summary>
        /// <param name="strSchedulingTemplateMasterId">主键索引</param>
        /// <returns></returns>
        public T_HR_SCHEDULINGTEMPLATEMASTER GetSchedulingTemplateMasterByAttSolID(string strAttendanceSolutionId)
        {
            if (string.IsNullOrEmpty(strAttendanceSolutionId))
            {
                return null;
            }

            SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
            T_HR_SCHEDULINGTEMPLATEMASTER entRd = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterByAttSolID(strAttendanceSolutionId);
            return entRd;
        }

        /// <summary>
        /// 根据条件，获取排班模板信息
        /// </summary>
        /// <param name="strSchedulingTemplateName"></param>
        /// <param name="strCircleType"></param>
        /// <param name="strSortKey"></param>
        /// <returns></returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEMASTER> GetAllSchedulingTemplateMasterRdListByMultSearch(string strOwnerID, string strSchedulingTemplateName, string strCircleType, string strSortKey)
        {
            SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strSchedulingTemplateName))
            {
                strfilter.Append(" @0.Contains(TEMPLATENAME)");
                objArgs.Add(strSchedulingTemplateName);
            }

            if (!string.IsNullOrEmpty(strCircleType))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" SCHEDULINGCIRCLETYPE == @" + iIndex.ToString());
                objArgs.Add(strCircleType);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " TEMPLATEMASTERID ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_OVERTIMEREWARD");

            var q = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterRdListByMultSearch(strOrderBy, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取排班模板信息,并进行分页
        /// </summary>
        /// <param name="strSchedulingTemplateName">排班模板名称</param>
        /// <param name="strCircleType">排班模板循环方式</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>排班模板信息</returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEMASTER> GetSchedulingTemplateMasterRdListByMultSearch(string strOwnerID, string strSchedulingTemplateName, string strCircleType,
            string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllSchedulingTemplateMasterRdListByMultSearch(strOwnerID, strSchedulingTemplateName, strCircleType, strSortKey);

            return Utility.Pager<T_HR_SCHEDULINGTEMPLATEMASTER>(q, pageIndex, pageSize, ref pageCount);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 新增排班模板信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddTemplateMaster(T_HR_SCHEDULINGTEMPLATEMASTER entTemp)
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

                strFilter.Append(" TEMPLATENAME == @0");

                objArgs.Add(entTemp.TEMPLATENAME);

                SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
                flag = dalSchedulingTemplateMaster.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalSchedulingTemplateMaster.Add(entTemp);
                
                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改排班模板信息
        /// </summary>
        /// <param name="entLTRd"></param>
        /// <returns></returns>
        public string ModifyTemplateMaster(T_HR_SCHEDULINGTEMPLATEMASTER entTemp)
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

                strFilter.Append(" TEMPLATEMASTERID == @0");

                objArgs.Add(entTemp.TEMPLATEMASTERID);

                SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
                flag = dalSchedulingTemplateMaster.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_SCHEDULINGTEMPLATEMASTER entUpdate = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                entUpdate.TEMPLATENAME = entTemp.TEMPLATENAME;
                entUpdate.SCHEDULINGCIRCLETYPE = entTemp.SCHEDULINGCIRCLETYPE;
                entUpdate.REMARK = entTemp.REMARK;
                entUpdate.UPDATEDATE = entTemp.UPDATEDATE;
                entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                entUpdate.CREATEDATE = entTemp.CREATEDATE;
                entUpdate.CREATEUSERID = entTemp.CREATEUSERID;

                dalSchedulingTemplateMaster.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除排班模板信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string DeleteTemplateMaster(string strSchedulingTemplateMasterId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strSchedulingTemplateMasterId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" TEMPLATEMASTERID == @0");

                objArgs.Add(strSchedulingTemplateMasterId);

                SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
                flag = dalSchedulingTemplateMaster.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }
                
                //先删除明细表记录
                SchedulingTemplateDetailBLL bllSchedulingTemplateDetail = new SchedulingTemplateDetailBLL();
                bllSchedulingTemplateDetail.DeleteByTemplateMasterID(strSchedulingTemplateMasterId);

                //无异常出现，即删除主表记录
                T_HR_SCHEDULINGTEMPLATEMASTER entDel = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalSchedulingTemplateMaster.Delete(entDel);               

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 新增排班模板及明细信息
        /// </summary>
        /// <param name="entMasterTemp"></param>
        /// <param name="entDetailTemps"></param>
        /// <returns></returns>
        public string AddSchedulingTemplateMasterAndDetail(T_HR_SCHEDULINGTEMPLATEMASTER entMasterTemp, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entDetailTemps)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = AddTemplateMaster(entMasterTemp);

                if (strMsg != "{SAVESUCCESSED}")
                {
                    return strMsg;
                }

                if (entDetailTemps.Count() == 0)
                {
                    return strMsg;
                }

                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();
                foreach (T_HR_SCHEDULINGTEMPLATEDETAIL item in entDetailTemps)
                {
                    if (item.EntityKey != null)
                    {
                        item.EntityKey = null;  //清除EntityKey不为null的情况
                    }
                    bllTemplateDetail.AddTemplateDetail(item);
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
        /// 修改排班模板及明细
        /// </summary>
        /// <param name="entMasterTemp"></param>
        /// <param name="entDetailTemps"></param>
        /// <returns></returns>
        public string ModifySchedulingTemplateMasterAndDetail(T_HR_SCHEDULINGTEMPLATEMASTER entMasterTemp, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entDetailTemps)
        {
            string strMsg = string.Empty;
            try
            {
                strMsg = ModifyTemplateMaster(entMasterTemp);

                if (strMsg != "{SAVESUCCESSED}")
                {
                    return strMsg;
                }

                if (entDetailTemps.Count() == 0)
                {
                    return strMsg;
                }
                SchedulingTemplateDetailBLL bllTemplateDetail = new SchedulingTemplateDetailBLL();

                bllTemplateDetail.DeleteByTemplateMasterID(entMasterTemp.TEMPLATEMASTERID);

                foreach (T_HR_SCHEDULINGTEMPLATEDETAIL item in entDetailTemps)
                {
                    if (item.EntityKey != null)
                    {
                        item.EntityKey = null;  //清除EntityKey不为null的情况
                    }

                    if (item.T_HR_SCHEDULINGTEMPLATEMASTER == null)
                    {
                        item.T_HR_SCHEDULINGTEMPLATEMASTER = entMasterTemp;
                    }

                    bllTemplateDetail.AddTemplateDetail(item);
                }

                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        #endregion


        #region ILookupEntity 成员

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();

            SchedulingTemplateMasterDAL dalSchedulingTemplateMaster = new SchedulingTemplateMasterDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;
            strOrderBy = " TEMPLATEMASTERID ";

            IQueryable<T_HR_SCHEDULINGTEMPLATEMASTER> ents = dalSchedulingTemplateMaster.GetSchedulingTemplateMasterRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        #endregion



        
    }
}